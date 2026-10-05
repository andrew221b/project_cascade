using System.Collections.Generic;
using Cascade.Application;
using Cascade.Domain;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    // A camera-facing, real mesh machine. Rects supply layout only; this view never sends commands.
    public sealed partial class ToyMachineView : MonoBehaviour
    {
        sealed class Part
        {
            public Transform transform;
            public MeshRenderer renderer;
            public Mesh mesh;
            public RectTransform anchor;
            public Vector2 point, size;
            public float depth, z, angle;
            public Vector3 basePosition,baseScale; public Quaternion baseRotation; public Material material; public bool stretch;
        }
        sealed class FloorRig
        {
            public readonly List<Part> parts = new List<Part>();
            public readonly List<Part> cracks = new List<Part>();
            public readonly List<Part> locks = new List<Part>();
            public readonly List<Part> bridge = new List<Part>();
            public readonly List<Part> bomb = new List<Part>();
            public readonly List<Part> repair = new List<Part>();
            public readonly List<Part> key = new List<Part>();
            public readonly List<Part> stamp = new List<Part>();
            public readonly List<Part> fuse = new List<Part>(), durability = new List<Part>(), rope = new List<Part>();
            public Part board, bombFlame; public Mesh bendMesh;
            public Vector3[] vertices; public float sag,sagVelocity,sway,swayVelocity;
        }
        ToyArt art;
        Camera viewCamera;
        Vector3 cameraRestPosition;
        public bool ScreenShakeEnabled { get; set; }
        Transform machine, scenery, windmill;
        LayeredWorld livingWorld;
        public LayeredWorld LivingWorld => livingWorld;
        int theme;
        public void SetZone(int zone) { theme=Mathf.Clamp(zone,0,7); livingWorld?.SetZone(theme); }
        Part backdrop;
        readonly List<Part> all = new List<Part>();
        readonly List<Part> tokens = new List<Part>();
        readonly List<Part> tokenShadows = new List<Part>();
        readonly List<Part> targetBodies = new List<Part>();
        readonly List<Part> selections = new List<Part>();
        readonly List<FloorRig> floors = new List<FloorRig>();
        readonly List<Part> supports = new List<Part>();
        RectTransform upperAnchor, lowerAnchor;
        readonly Vector3[] corners = new Vector3[4];
        PuzzleInteraction interaction;
        bool reducedMotion,ambientPaused;
        public void SetAmbientPaused(bool value){ambientPaused=value;}
        float windTime;
        public bool ReducedMotion { get => reducedMotion; set => reducedMotion = value; }
        public float AmbientPhase => windTime;
        public int TokenCount => tokens.Count;
        bool gameplayView;
        public void SetGameplayVisible(bool visible) { gameplayView=visible;if(machine!=null)machine.gameObject.SetActive(visible);livingWorld?.SetGameplayVisible(visible); }
        public static float TokenFaceY(bool selected) => selected ? .48f : .38f;
        public PresentationFrame Frame { get; set; }
        readonly float[] floorOffsets = new float[4];
        readonly Vector3[] restPositions = new Vector3[8], restScales = new Vector3[8];
        public Vector3 TokenPosition(int index) => restPositions[index];
        public Vector3 PresentedTokenPosition(int index) => tokens[index].transform.position;
        public float SelectionLiftWorld(int index) { tokens[index].anchor.GetWorldCorners(corners); return (corners[2].y-corners[0].y)*.1f; }
        public Vector3 FloorVisualOffset(int floor)=>new Vector3(floors[floor].sway,-floors[floor].sag+floorOffsets[floor],0);
        public Vector3 FloorPosition(int floor)
        {
            var anchor = floors[floor].parts[0].anchor; anchor.GetWorldCorners(corners);
            return new Vector3((corners[0].x + corners[2].x) * .5f, Mathf.Lerp(corners[0].y, corners[2].y, .33f), -.3f);
        }
        public Vector3 ContactPosition(int floor, int slot)
        {
            var anchor = floors[floor].parts[0].anchor; anchor.GetWorldCorners(corners);
            float x=.045f+.91f*(slot+.5f)/tokens.Count;
            return new Vector3(Mathf.Lerp(corners[0].x,corners[2].x,x)+floors[floor].sway,Mathf.Lerp(corners[0].y,corners[2].y,.31f)-Mathf.Sin(x*Mathf.PI)*floors[floor].sag+floorOffsets[floor],-.5f);
        }
        public void RecoilFloor(int floor, float offset)
        {
            foreach (var p in floors[floor].parts) p.transform.position += Vector3.up * (offset - floorOffsets[floor]);
            floorOffsets[floor] = offset;
            if(viewCamera!=null)viewCamera.transform.position=cameraRestPosition+(ScreenShakeEnabled&&!reducedMotion?Vector3.right*offset*.2f:Vector3.zero);
        }
        public void OffsetToken(int index, Vector3 offset, Vector2 stretch)
        {
            visualOffsets[index]=offset;visualStretch[index]=stretch;
            tokens[index].transform.position = restPositions[index] + dynamicOffsets[index] + offset;
            tokens[index].transform.localScale = Vector3.Scale(restScales[index], new Vector3(stretch.x, stretch.y, 1));
        }

        public void Initialize(ToyArt artwork, Camera camera)
        {
            art = artwork; viewCamera = camera;cameraRestPosition=camera.transform.position;
            scenery = new GameObject("Sky workshop scenery").transform; scenery.SetParent(transform, false);
            backdrop = Create("Painted cloud garden", scenery, null, art.quad, art.backdrop, Vector2.zero, Vector2.one, 3, .01f);
            livingWorld = gameObject.AddComponent<LayeredWorld>(); livingWorld.Initialize(art,viewCamera);
            if(livingWorld.Ready)backdrop.transform.gameObject.SetActive(false);
            var lightObject = new GameObject("Warm workshop key light", typeof(Light)); lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = Quaternion.Euler(32, -24, 0);
            var keyLight = lightObject.GetComponent<Light>(); keyLight.type = LightType.Directional;
            keyLight.color = new Color(1, .96f, .88f); keyLight.intensity = 1.05f; keyLight.shadows = LightShadows.None;
            var fillObject=new GameObject("Soft sky rim",typeof(Light));fillObject.transform.SetParent(transform,false);fillObject.transform.rotation=Quaternion.Euler(-18,155,0);
            var fill=fillObject.GetComponent<Light>();fill.type=LightType.Directional;fill.color=new Color(.64f,.85f,1);fill.intensity=.35f;fill.shadows=LightShadows.None;
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.69f, .79f, .80f);

        }

        public void Bind(PuzzleInteraction model, IReadOnlyList<RectTransform> anchors, IReadOnlyList<TokenSlot> slots,
            IReadOnlyList<TMPro.TextMeshProUGUI> targets, RectTransform receiver)
        {
            if (machine != null) { machine.gameObject.SetActive(false); Destroy(machine.gameObject); }
            machine = new GameObject("Toy machine").transform; machine.SetParent(transform, false);
            foreach(var mesh in woodMeshes)if(mesh!=null)Destroy(mesh);woodMeshes.Clear();
            all.Clear(); tokens.Clear(); selections.Clear(); tokenShadows.Clear(); targetBodies.Clear(); floors.Clear(); supports.Clear(); interaction = model;
            Frame = new PresentationFrame(model.Attempt.Current);
            upperAnchor = anchors[0]; lowerAnchor = receiver;
            InitializeWoodMaterials();
            InitializeRopeRig();
            for (int i = 0; i < anchors.Count; i++) BuildFloor(anchors[i], model.Attempt.Level.Floors[i], slots.Count);
            Add("Receiver contact", receiver, art.quad, art.shadow, new Vector2(.51f, .38f), new Vector2(1.07f, .96f), .7f, .01f);
            Add("Golden receiver rim", receiver, art.beam, art.gold, new Vector2(.5f, .48f), new Vector2(1, .76f), .35f, .30f);
            Add("Ivory inset", receiver, art.beam, art.paleGold, new Vector2(.5f, .53f), new Vector2(.93f, .64f), .12f, .18f);
            for (int i = 0; i < targets.Count; i++) {
                var anchor = targets[i].rectTransform;
                Add("Target socket", anchor, art.tile, art.gold, new Vector2(.5f, .5f), new Vector2(.83f, .90f), -.06f, .06f);
                var symbol = model.Attempt.Level.TargetSymbolIds[i];
                targetBodies.Add(Add("Target " + i, anchor, art.SymbolMesh(symbol), art.SymbolMaterial(symbol), new Vector2(.5f, .5f), new Vector2(.66f, .72f), -.12f, .10f));
            }
            for (int i = 0; i < slots.Count; i++) {
                var anchor = (RectTransform)slots[i].transform;
                tokenShadows.Add(Add("Token contact " + i, anchor, art.quad, art.shadow, new Vector2(.53f, .22f), new Vector2(1.04f, .35f), .04f, .01f));
                selections.Add(Add("Selection rim " + i, anchor, art.tile, art.gold, new Vector2(.5f, .48f), new Vector2(.94f, 1.04f), -.18f, .12f));
                tokens.Add(Add("Persistent token " + i, anchor, art.tile, art.cream, new Vector2(.5f, .5f), new Vector2(.82f, .90f), -.35f, .34f));
            }
            Sync();
        }
        void BuildFloor(RectTransform anchor, FloorDefinition floor, int count)
        {
            var rig = new FloorRig(); floors.Add(rig);
            Part P(string name, Mesh mesh, Material material, float x, float y, float w, float h, float z = .1f, float angle = 0) {
                var p = Add(name, anchor, mesh, material, new Vector2(x, y), new Vector2(w, h), z, .16f, angle); rig.parts.Add(p); return p;
            }
            P("Platform contact", art.quad, art.shadow, .51f, .25f, 1.05f, .29f, .65f);
            rig.bendMesh=BoardMesh();rig.vertices=rig.bendMesh.vertices;
            rig.board=P("Wooden suspended board",rig.bendMesh,woodMaterial,.5f,.33f,1,.19f,.15f);rig.board.stretch=true;
            for(int end=0;end<2;end++) {
                float x=end==0?.025f:.975f;
                // Continuous ropes are shared by the whole hanging machine.
                P("Rope knot",art.disk,ropeMaterial,x,.38f,.05f,.09f,-.14f);

            }

            for (int i = 0; i < 3; i++) rig.cracks.Add(P("Damage seam " + i, art.beam, art.graphite, .28f + i * .23f, .33f, .055f, .023f, -.17f, i % 2 == 0 ? 65 : -58));
            for (int i = 0; i < count - 1; i++) {
                float x = .045f + .91f * (i + 1f) / count;
                var gate = P("Edge latch " + i, art.tile, art.gold, x, .55f, .025f, .18f, -.4f); rig.locks.Add(gate);
                if (floor.FragileEdges.Contains(i)) {
                    P("Fragile notch", art.beam, art.graphite, x - .008f, .35f, .06f, .032f, -.21f, -45);
                    P("Fragile notch", art.beam, art.graphite, x + .007f, .29f, .05f, .032f, -.22f, 45);
                }
                if (floor.OneUseBridgeEdge == i) {
                    rig.bridge.Add(P("One use bridge", art.beam, art.gold, x, .46f, .12f, .055f, -.4f));
                    rig.bridge.Add(P("Bridge hinge", art.disk, art.graphite, x, .46f, .024f, .045f, -.51f));
                }
            }
        }

        Part Add(string name, RectTransform anchor, Mesh mesh, Material material, Vector2 point, Vector2 size, float z, float depth, float angle = 0)
        {
            var part = Create(name, machine, anchor, mesh, material, point, size, z, depth, angle); all.Add(part); return part;
        }
        static Part Create(string name, Transform parent, RectTransform anchor, Mesh mesh, Material material, Vector2 point, Vector2 size, float z, float depth, float angle = 0)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh; var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            return new Part { transform = go.transform, renderer = renderer, material=material, mesh = mesh, anchor = anchor, point = point, size = size, z = z, depth = depth, angle = angle };
        }
        void Place(Part part)
        {
            part.anchor.GetWorldCorners(corners); var size = corners[2] - corners[0];
            part.transform.position = new Vector3(corners[0].x + size.x * part.point.x, corners[0].y + size.y * part.point.y, part.z);
            part.transform.rotation = Quaternion.Euler(0, 0, part.angle);
            var bounds = part.mesh.bounds.size;
            float width = size.x * part.size.x, height = size.y * part.size.y;
            if (!part.stretch && part.mesh != art.beam && part.mesh != art.quad) { float fit = Mathf.Min(width / bounds.x, height / bounds.y); width = fit * bounds.x; height = fit * bounds.y; }
            part.transform.localScale = new Vector3(width / bounds.x, height / bounds.y, part.depth / Mathf.Max(.01f, bounds.z));
            part.basePosition=part.transform.position;part.baseRotation=part.transform.rotation;part.baseScale=part.transform.localScale;
        }
        public void Sync()
        {
            if (viewCamera == null) return;
            viewCamera.transform.position=cameraRestPosition;
            float height = viewCamera.orthographicSize * 2, width = height * viewCamera.aspect;
            var texture = art.backdrop.mainTexture; float ratio = texture == null ? .6667f : (float)texture.width / texture.height;
            float coverH = Mathf.Max(height, width / ratio);
            backdrop.transform.position = new Vector3(viewCamera.transform.position.x, viewCamera.transform.position.y, 3);
            backdrop.transform.localScale = new Vector3(coverH * ratio, coverH, 1);

            if (interaction == null) return;
            var state = Frame;
            foreach (var target in targetBodies) target.transform.gameObject.SetActive(true);
            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i]; var mesh = art.SymbolMesh(state.Row[i]);
                token.mesh = mesh; token.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
                token.renderer.sharedMaterial = art.SymbolMaterial(state.Row[i]);
                token.point = new Vector2(.5f, state.Status == PuzzleStatus.Won ? .5f : TokenFaceY(interaction.SelectedSlot == i));
                token.size = state.Status == PuzzleStatus.Won ? new Vector2(.66f, .72f)
                    : interaction.SelectedSlot == i ? new Vector2(.81f, .915f) : new Vector2(.78f, .88f);
                selections[i].transform.gameObject.SetActive(interaction.SelectedSlot == i && token.anchor.gameObject.activeInHierarchy);
                token.transform.gameObject.SetActive(token.anchor.gameObject.activeInHierarchy);
                tokenShadows[i].transform.gameObject.SetActive(token.anchor.gameObject.activeInHierarchy);
            }
            for (int f = 0; f < floors.Count; f++) {
                var rig = floors[f]; bool past = !state.FloorVisible(f);
                bool active = f == state.ActiveFloor && state.Status == PuzzleStatus.Playing;
                foreach (var p in rig.parts) p.transform.gameObject.SetActive(!past);
                int hp = active ? state.Durability : interaction.Attempt.Level.Floors[f].BaseDurability;
                int damage = Mathf.Clamp(interaction.Attempt.Level.Floors[f].BaseDurability - hp, 0, 3);
                for (int j = 0; j < rig.cracks.Count; j++) rig.cracks[j].transform.gameObject.SetActive(!past && j < damage);
                for (int edge = 0; edge < rig.locks.Count; edge++) {
                    bool open = active ? state.EdgeVisibleOpen(interaction.Attempt.Level.Floors[f], edge) : interaction.Attempt.Level.Floors[f].OpenEdges[edge];
                    rig.locks[edge].transform.gameObject.SetActive(!past && !open);
                }
                SetVisible(rig.bridge, !past && !(active && state.BridgeUsed));
                SetVisible(rig.bomb,!past && (!active || state.BombPhase!=BombPhase.Exploded));
                foreach(var p in rig.bomb)p.renderer.sharedMaterial=active&&state.BombPhase==BombPhase.Defused&&p.transform.name.Contains("mount")?art.teal:p.material;
                if(rig.bombFlame!=null)rig.bombFlame.transform.gameObject.SetActive(!past&&(!active||state.BombPhase==BombPhase.Armed));
                for(int t=0;t<rig.fuse.Count;t++)rig.fuse[t].transform.gameObject.SetActive(!past&&(!active||state.BombPhase==BombPhase.Armed)&&t<(active?state.BombTicks:rig.fuse.Count));
                for(int pip=0;pip<rig.durability.Count;pip++) {
                    var p=rig.durability[pip];p.transform.gameObject.SetActive(!past&&pip<Mathf.Max(hp,interaction.Attempt.Level.Floors[f].BaseDurability));
                    p.renderer.sharedMaterial=active&&forecastHp.HasValue&&pip>=forecastHp.Value?art.coral:pip<hp?art.paleGold:ropeMaterial;
                }
                SetVisible(rig.key, !past && !(active && state.KeyUsed));
                SetVisible(rig.repair, !past && !(active && state.RepairUsed));
                SetVisible(rig.stamp, !past && (state.StampBits & (1 << f)) == 0);
            }
            foreach (var part in all) if (part.transform.gameObject.activeSelf) Place(part);
            for (int i = 0; i < floorOffsets.Length; i++) floorOffsets[i] = 0;
            for (int i = 0; i < tokens.Count; i++) {
                if(state.Status==PuzzleStatus.Playing) {
                    var p=tokens[i];var rig=floors[state.ActiveFloor];rig.board.anchor.GetWorldCorners(corners);
                    // The painted plank occupies the middle of its padded mesh. Seat pieces on visible wood.
                    float boardTop=Mathf.Lerp(corners[0].y,corners[2].y,.365f);
                    float half=p.mesh.bounds.extents.y*p.transform.localScale.y;
                    p.transform.position=new Vector3(p.transform.position.x,boardTop+half-p.mesh.bounds.center.y*p.transform.localScale.y+(interaction.SelectedSlot==i?SelectionLiftWorld(i):0),p.transform.position.z);
                    var shadow=tokenShadows[i];shadow.basePosition=new Vector3(p.transform.position.x,boardTop-.012f,shadow.basePosition.z);shadow.transform.position=shadow.basePosition;
                    shadow.baseScale=new Vector3(p.mesh.bounds.size.x*p.transform.localScale.x*1.12f,.075f,.01f);shadow.transform.localScale=shadow.baseScale;
                }
                restPositions[i]=tokens[i].transform.position;restScales[i]=tokens[i].transform.localScale;
                visualOffsets[i]=Vector3.zero;visualStretch[i]=Vector2.one;visualAngles[i]=0;
            }
            upperAnchor.GetWorldCorners(corners); float left = corners[0].x, right = corners[2].x;
            float top = Mathf.Lerp(corners[0].y, corners[2].y, .34f);
            lowerAnchor.GetWorldCorners(corners); float bottom = Mathf.Lerp(corners[0].y, corners[2].y, .55f);
            for (int i = 0; i < supports.Count; i++) {
                supports[i].transform.gameObject.SetActive(state.Status==PuzzleStatus.Playing);
                var support = supports[i]; support.transform.position = new Vector3(Mathf.Lerp(left, right, i == 0 ? .035f : .965f), (top + bottom) * .5f, .65f);
                support.transform.rotation = Quaternion.Euler(0, 0, 90);
                support.transform.localScale = new Vector3((top - bottom) / art.beam.bounds.size.x, .035f, .24f);
            }
            UpdateRopeRig();
        }
        static void SetVisible(List<Part> parts, bool visible) { foreach (var p in parts) p.transform.gameObject.SetActive(visible); }
        void LateUpdate()
        {
            if (!reducedMotion && interaction != null && !ambientPaused && !(interaction.IsPaused&&gameplayView) && !interaction.IsSuspended) windTime += Time.unscaledDeltaTime;
            livingWorld?.Paint(reducedMotion ? 0 : windTime);
            if (windmill != null) windmill.localRotation = Quaternion.Euler(0, 0, reducedMotion ? 18 : 18 + windTime * 7);
        }
    }
}
