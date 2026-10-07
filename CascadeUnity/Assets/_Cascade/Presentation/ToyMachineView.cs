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
            public readonly MaterialPropertyBlock woodProperties=new MaterialPropertyBlock();
            public readonly List<Part> locks = new List<Part>();
            public readonly List<Part> bridge = new List<Part>();
            public readonly List<Part> bomb = new List<Part>();
            public readonly List<Part> repair = new List<Part>();
            public readonly List<Part> key = new List<Part>();
            public readonly List<Part> stamp = new List<Part>();
            public readonly List<Part> fuse = new List<Part>(), durability = new List<Part>(), rope = new List<Part>();
            public readonly Part[] knots=new Part[2];
            public Part board,top,bombFlame; public Mesh bendMesh,topMesh;
            public Vector3[] vertices,topVertices; public float sag,sagVelocity,sway,swayVelocity,roll,rollVelocity,wear,fracture;
            public readonly float[] deflection=new float[17];
            public int surfaceStyle=-1;
        }
        ToyArt art;
        Camera viewCamera;
        Vector3 cameraRestPosition;
        public bool ScreenShakeEnabled { get; set; }
        Transform machine, scenery, windmill;
        LayeredWorld livingWorld;
        public LayeredWorld LivingWorld => livingWorld;
        int theme;Light worldKey,worldFill;
        public void SetZone(int zone) { theme=Mathf.Clamp(zone,0,Cascade.Content.JourneyStory.WorldCount-1); livingWorld?.SetZone(theme);
            if(worldKey==null||worldFill==null)return;bool night=theme==8;
            worldKey.color=night?new Color(1,.87f,.66f):new Color(1,.96f,.88f);worldKey.intensity=night?1.08f:1.18f;
            worldFill.color=night?new Color(.42f,.65f,1):new Color(.64f,.85f,1);worldFill.intensity=night?.64f:.35f;
            RenderSettings.ambientLight=night?new Color(.38f,.48f,.65f):new Color(.69f,.79f,.80f);
        }
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
        public void DelayAmbientGust(float seconds){nextAmbientGust=Mathf.Max(nextAmbientGust,windTime+seconds);}
        float windTime;
        float gustStarted=-100;
        float gustDirection;
        float windAnticipation;
        float nextAmbientGust=9;
        int ambientGust;
        public void PrepareWind(bool right,float strength) => windAnticipation=(right?1:-1)*Mathf.Clamp01(strength)*.48f;
        public bool ReducedMotion { get => reducedMotion; set => reducedMotion = value; }
        public float AmbientPhase => windTime;
        public int TokenCount => tokens.Count;
        bool gameplayView;
        public void SetGameplayVisible(bool visible) { gameplayView=visible;if(machine!=null)machine.gameObject.SetActive(visible);livingWorld?.SetGameplayVisible(visible); }
        public void SetRigVisible(bool visible){if(machine!=null)machine.gameObject.SetActive(visible);}
        public DescentGhost CreateDescentGhost(string name)
        {
            var ghost=new GameObject(name).AddComponent<DescentGhost>();
            foreach(var renderer in machine.GetComponentsInChildren<MeshRenderer>()) {
                var filter=renderer.GetComponent<MeshFilter>();if(filter==null||filter.sharedMesh==null)continue;
                var part=new GameObject(renderer.name,typeof(MeshFilter),typeof(MeshRenderer));part.transform.SetParent(ghost.transform,false);
                part.transform.SetPositionAndRotation(renderer.transform.position,renderer.transform.rotation);part.transform.localScale=renderer.transform.lossyScale;
                var mesh=Instantiate(filter.sharedMesh);ghost.OwnedMeshes.Add(mesh);part.GetComponent<MeshFilter>().sharedMesh=mesh;
                var copy=part.GetComponent<MeshRenderer>();copy.sharedMaterials=renderer.sharedMaterials;
                copy.shadowCastingMode=renderer.shadowCastingMode;copy.receiveShadows=renderer.receiveShadows;
            }
            GetComponent<CascadeGame>().AppendDescentGraphics(ghost);return ghost;
        }
        public static float TokenFaceY(bool selected) => selected ? .48f : .38f;
        public PresentationFrame Frame { get; set; }
        readonly float[] floorOffsets = new float[4];
        readonly Vector3[] restPositions = new Vector3[8], restScales = new Vector3[8];
        public Vector3 TokenPosition(int index) => restPositions[index];
        public Vector3 PresentedTokenPosition(int index) => tokens[index].transform.position;
        public float SelectionLiftWorld(int index) { tokens[index].anchor.GetWorldCorners(corners); return (corners[2].y-corners[0].y)*.1f; }
        public float FloorVisualAngle(int floor)=>floors[floor].roll;
        public float FloorBreakProgress(int floor)=>floors[floor].fracture;
        public float FloorSag(int floor)=>floors[floor].sag;
        public float FloorFaceHeight(int floor)=>floors[floor].board.baseScale.y;
        public float FloorSpan(int floor)=>floors[floor].board.baseScale.x;
        public void FractureFloor(int floor,float progress){floors[floor].fracture=Mathf.Clamp01(progress);TickWoodDynamics(0);}
        public Vector3 FloorVisualOffset(int floor)=>FloorPointOffset(floor,floors[floor].board.anchor.position,false);
        public float WindAt(int floor)
        {
            if(reducedMotion)return 0;
            float age=Mathf.Max(0,windTime-gustStarted);
            float gust=(1-Mathf.Exp(-age*6))*Mathf.Exp(-age*.85f)*gustDirection*1.8f;
            return Mathf.Clamp(WorkshopWind.Sample(windTime-floor*.13f)+gust+windAnticipation,-1,1)*(heldWeight>=0?.72f:1);
        }
        public void CueWind(bool right,float strength=1)
        {
            windAnticipation=0;if(reducedMotion)return;gustStarted=windTime;gustDirection=(right?1:-1)*Mathf.Clamp01(strength);
            // One pressure impulse, then the existing springs carry the recoil and recovery.
            foreach(var rig in floors){rig.swayVelocity+=gustDirection*1.45f;rig.rollVelocity+=gustDirection*17;}
            for(int side=0;side<2;side++)for(int segment=0;segment<=floors.Count;segment++)ropeSpans[side,segment]?.Impulse(gustDirection*.85f);
        }
        public Vector3 FloorPosition(int floor)
        {
            var anchor = floors[floor].parts[0].anchor; anchor.GetWorldCorners(corners);
            return new Vector3((corners[0].x + corners[2].x) * .5f, Mathf.Lerp(corners[0].y, corners[2].y, .33f), -.3f);
        }
        public Vector3 ContactPosition(int floor, int slot)
            =>SurfacePoint(floor,.045f+.91f*(slot+.5f)/tokens.Count,.31f);
        public Vector3 SurfacePoint(int floor,float x,float y)
        {
            var anchor = floors[floor].parts[0].anchor; anchor.GetWorldCorners(corners);
            var point=new Vector3(Mathf.Lerp(corners[0].x,corners[2].x,x),Mathf.Lerp(corners[0].y,corners[2].y,y),-.5f);
            return point+FloorPointOffset(floor,point,true);
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
            ApplyPiece(index);
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
            var keyLight = worldKey=lightObject.GetComponent<Light>(); keyLight.type = LightType.Directional;
            keyLight.color = new Color(1, .96f, .88f); keyLight.intensity = 1.18f; keyLight.shadows = LightShadows.Soft;
            keyLight.shadowStrength = .42f; keyLight.shadowBias = .035f; keyLight.shadowNormalBias = .15f;
            var fillObject=new GameObject("Soft sky rim",typeof(Light));fillObject.transform.SetParent(transform,false);fillObject.transform.rotation=Quaternion.Euler(-18,155,0);
            var fill=worldFill=fillObject.GetComponent<Light>();fill.type=LightType.Directional;fill.color=new Color(.64f,.85f,1);fill.intensity=.35f;fill.shadows=LightShadows.None;
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.69f, .79f, .80f);
            InitializePieceMaterials();

        }

        public void Bind(PuzzleInteraction model, IReadOnlyList<RectTransform> anchors, IReadOnlyList<TokenSlot> slots,
            IReadOnlyList<TMPro.TextMeshProUGUI> targets, RectTransform receiver)
        {
            if (machine != null) { machine.gameObject.SetActive(false); Destroy(machine.gameObject); }
            gustStarted=-100;gustDirection=0;windAnticipation=0;
            nextAmbientGust=windTime+9+(ambientGust%3);
            machine = new GameObject("Toy machine").transform; machine.SetParent(transform, false);
            foreach(var mesh in woodMeshes)if(mesh!=null)Destroy(mesh);woodMeshes.Clear();
            all.Clear(); tokens.Clear(); selections.Clear(); tokenShadows.Clear(); targetBodies.Clear(); floors.Clear(); supports.Clear(); interaction = model;
            Frame = new PresentationFrame(model.Attempt.Current);
            ResetContactLoads();
            upperAnchor = anchors[0]; lowerAnchor = receiver;
            InitializeWoodMaterials();
            InitializeRopeRig();
            for (int i = 0; i < anchors.Count; i++) BuildFloor(anchors[i], model.Attempt.Level.Floors[i], slots.Count);
            Add("Receiver contact", receiver, art.quad, art.shadow, new Vector2(.51f, .38f), new Vector2(1.07f, .96f), .7f, .01f);
            Add("Golden receiver rim", receiver, art.beam, art.gold, new Vector2(.5f, .48f), new Vector2(1, .76f), .35f, .30f);
            Add("Ivory inset", receiver, art.beam, art.cream, new Vector2(.5f, .53f), new Vector2(.93f, .64f), .12f, .18f);
            for (int i = 0; i < targets.Count; i++) {
                var anchor = targets[i].rectTransform;
                Add("Target socket", anchor, art.tile, TargetSocketMaterial, new Vector2(.5f, .5f), new Vector2(.92f, .94f), -.06f, .08f);
                var symbol = model.Attempt.Level.TargetSymbolIds[i];
                targetBodies.Add(Add("Target " + i, anchor, art.SymbolMesh(symbol), PieceMaterial(symbol), new Vector2(.5f, .5f), new Vector2(.72f, .78f), -.12f, .12f));
            }
            for (int i = 0; i < slots.Count; i++) {
                var anchor = (RectTransform)slots[i].transform;
                tokenShadows.Add(Add("Token contact " + i, anchor, art.quad, art.shadow, new Vector2(.53f, .22f), new Vector2(1.04f, .35f), .04f, .01f));
                selections.Add(Add("Piece contour " + i, anchor, art.tile, TouchGlowMaterial, new Vector2(.5f, .5f), new Vector2(.82f, .90f), -.18f, .12f));
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
            rig.surfaceStyle=PlatformSurfaceArt.Style(floor,floors.Count-1==Frame.ActiveFloor);
            var surfaceMaterial=rig.surfaceStyle>=0?platformMaterials[rig.surfaceStyle]:woodMaterial;
            rig.bendMesh=BoardMesh();rig.vertices=rig.bendMesh.vertices;
            if(rig.surfaceStyle>=0){var uv=rig.bendMesh.uv;for(int i=0;i<uv.Length;i++)uv[i].x=PlatformSurfaceArt.FaceUvX(uv[i].x);rig.bendMesh.uv=uv;}
            rig.board=P("Wooden suspended board",rig.bendMesh,surfaceMaterial,.5f,.30f,1,.25f,.15f);rig.board.stretch=true;
            rig.topMesh=BoardTopMesh();rig.topVertices=rig.topMesh.vertices;
            rig.top=P("Wooden load-bearing top",rig.topMesh,surfaceMaterial,.5f,.365f,1,.08f,-.15f);rig.top.depth=.65f;rig.top.stretch=true;
            for(int end=0;end<2;end++) {
                float x=end==0?.025f:.975f;
                // Continuous ropes are shared by the whole hanging machine.
                rig.knots[end]=P("Rope knot",art.disk,ropeMaterial,x,.38f,.05f,.09f,-.14f);

            }

            for (int i = 0; i < count - 1; i++) {
                float x = .045f + .91f * (i + 1f) / count;
                var gate = P("Edge latch " + i, art.quad, branchMaterial, x, .466f, .05f, .27f, -.32f);
                var branchUv=new MaterialPropertyBlock();branchUv.SetVector("_BaseMap_ST",new Vector4(.5f,1,((i+floors.Count)%2)*.5f,0));gate.renderer.SetPropertyBlock(branchUv);
                rig.locks.Add(gate);
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
            bool solidPiece = name.StartsWith("Persistent token") || name.StartsWith("Target ") || name.StartsWith("Wooden ") || name=="Rope knot" || name=="Golden receiver rim" || name=="Ivory inset";
            renderer.shadowCastingMode = solidPiece ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = name == "Wooden suspended board" || solidPiece;
            return new Part { transform = go.transform, renderer = renderer, material=material, mesh = mesh, anchor = anchor, point = point, size = size, z = z, depth = depth, angle = angle };
        }
        void Place(Part part)
        {
            part.anchor.GetWorldCorners(corners); var size = corners[2] - corners[0];
            part.transform.position = new Vector3(corners[0].x + size.x * part.point.x, corners[0].y + size.y * part.point.y, part.z);
            part.transform.rotation = Quaternion.Euler(0, 0, part.angle);
            var bounds = part.stretch?new Vector3(1,2,1):part.mesh.bounds.size;
            float width = size.x * part.size.x, height = size.y * part.size.y;
            if(part.material==branchMaterial)width=height*.5f; // Two portrait illustrations, each half of the square atlas.
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
                token.renderer.sharedMaterial = PieceMaterial(state.Row[i]);
                token.point = new Vector2(.5f, state.Status == PuzzleStatus.Won ? .5f : TokenFaceY(false));
                token.size = state.Status == PuzzleStatus.Won ? new Vector2(.66f, .72f)
                    : new Vector2(.78f, .88f);
                selections[i].mesh=mesh;selections[i].transform.GetComponent<MeshFilter>().sharedMesh=mesh;
                selections[i].transform.gameObject.SetActive(false);
                token.transform.gameObject.SetActive(token.anchor.gameObject.activeInHierarchy);
                tokenShadows[i].transform.gameObject.SetActive(token.anchor.gameObject.activeInHierarchy);
            }
            for (int f = 0; f < floors.Count; f++) {
                var rig = floors[f]; bool past = !state.FloorVisible(f);
                bool active = f == state.ActiveFloor && state.Status == PuzzleStatus.Playing;
                foreach (var p in rig.parts) p.transform.gameObject.SetActive(!past);
                int hp = active ? state.Durability : interaction.Attempt.Level.Floors[f].BaseDurability;
                rig.wear=active?Mathf.Clamp01(1-hp/(float)interaction.Attempt.Level.Floors[f].BaseDurability):0;
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
                    p.transform.position=new Vector3(p.transform.position.x,boardTop+half-p.mesh.bounds.center.y*p.transform.localScale.y,p.transform.position.z);
                    var shadow=tokenShadows[i];shadow.basePosition=new Vector3(p.transform.position.x,boardTop-.012f,shadow.basePosition.z);shadow.transform.position=shadow.basePosition;
                    shadow.baseScale=new Vector3(p.mesh.bounds.size.x*p.transform.localScale.x*1.12f,.075f,.01f);shadow.transform.localScale=shadow.baseScale;
                }
                restPositions[i]=tokens[i].transform.position;restScales[i]=tokens[i].transform.localScale;
                bool selected=interaction.SelectedSlot==i;
                visualOffsets[i]=selected?Vector3.up*SelectionLiftWorld(i):Vector3.zero;visualStretch[i]=Vector2.one*(selected?1.04f:1);visualAngles[i]=0;
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
            TickWoodDynamics(0); UpdateRopeRig();
        }
        static void SetVisible(List<Part> parts, bool visible) { foreach (var p in parts) p.transform.gameObject.SetActive(visible); }
        void LateUpdate()
        {
            if (!reducedMotion && interaction != null && !ambientPaused && !(interaction.IsPaused&&gameplayView) && !interaction.IsSuspended) windTime += Time.unscaledDeltaTime;
            if(gameplayView&&interaction?.Attempt.Current.Status==PuzzleStatus.Playing&&!reducedMotion&&interaction!=null&&!interaction.IsPaused&&!interaction.IsSuspended&&!interaction.IsPreparing&&!interaction.IsPresenting&&heldWeight<0&&windTime>=nextAmbientGust){
                var game=GetComponent<CascadeGame>();
                if(!game.RequireGustIntroduction()){
                    CueWind((ambientGust++%2)==0,.55f);nextAmbientGust=windTime+12+(ambientGust%4)*1.7f;
                    game.CueDeviceWind(gustDirection>0);
                }
            }
            livingWorld?.Paint(reducedMotion ? 0 : windTime);
            if (windmill != null) windmill.localRotation = Quaternion.Euler(0, 0, reducedMotion ? 18 : 18 + windTime * 7);
        }
    }
}
