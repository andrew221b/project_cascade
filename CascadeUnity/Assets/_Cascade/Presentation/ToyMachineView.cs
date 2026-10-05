using System.Collections.Generic;
using Cascade.Application;
using Cascade.Domain;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    // A camera-facing, real mesh machine. Rects supply layout only; this view never sends commands.
    public sealed class ToyMachineView : MonoBehaviour
    {
        sealed class Part
        {
            public Transform transform;
            public MeshRenderer renderer;
            public Mesh mesh;
            public RectTransform anchor;
            public Vector2 point, size;
            public float depth, z, angle;
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
        }
        ToyArt art;
        Camera viewCamera;
        Transform machine, scenery, windmill;
        Part backdrop;
        readonly List<Part> all = new List<Part>();
        readonly List<Part> tokens = new List<Part>();
        readonly List<Part> tokenShadows = new List<Part>();
        readonly List<Part> selections = new List<Part>();
        readonly List<FloorRig> floors = new List<FloorRig>();
        readonly List<Part> supports = new List<Part>();
        RectTransform upperAnchor, lowerAnchor;
        readonly Vector3[] corners = new Vector3[4];
        PuzzleInteraction interaction;
        bool reducedMotion;
        float windTime;
        public bool ReducedMotion { get => reducedMotion; set => reducedMotion = value; }
        public float AmbientPhase => windTime;
        public int TokenCount => tokens.Count;
        public static float TokenFaceY(bool selected) => selected ? .48f : .38f;

        public void Initialize(ToyArt artwork, Camera camera)
        {
            art = artwork; viewCamera = camera;
            scenery = new GameObject("Sky workshop scenery").transform; scenery.SetParent(transform, false);
            backdrop = Create("Painted cloud garden", scenery, null, art.quad, art.backdrop, Vector2.zero, Vector2.one, 3, .01f);
            var lightObject = new GameObject("Warm workshop key light", typeof(Light)); lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = Quaternion.Euler(32, -24, 0);
            var keyLight = lightObject.GetComponent<Light>(); keyLight.type = LightType.Directional;
            keyLight.color = new Color(1, .96f, .88f); keyLight.intensity = 1.05f; keyLight.shadows = LightShadows.None;
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.69f, .79f, .80f);
            windmill = new GameObject("Quiet wind wheel").transform; windmill.SetParent(scenery, false);
            for (int i = 0; i < 4; i++) {
                var blade = Create("Enamel sail", windmill, null, art.tile, i % 2 == 0 ? art.cream : art.teal, Vector2.zero, Vector2.one, 0, .1f);
                blade.transform.localPosition = Quaternion.Euler(0, 0, 90 * i) * new Vector3(0, .28f, 0);
                blade.transform.localRotation = Quaternion.Euler(0, 0, 90 * i - 22); blade.transform.localScale = new Vector3(.2f, .55f, .45f);
            }
            var hub = Create("Brass axle", windmill, null, art.disk, art.gold, Vector2.zero, Vector2.one, 0, .1f);
            hub.transform.localPosition = new Vector3(0, 0, -.12f); hub.transform.localScale = new Vector3(.22f, .22f, .5f);
        }

        public void Bind(PuzzleInteraction model, IReadOnlyList<RectTransform> anchors, IReadOnlyList<TokenSlot> slots,
            IReadOnlyList<TMPro.TextMeshProUGUI> targets, RectTransform receiver)
        {
            if (machine != null) { machine.gameObject.SetActive(false); Destroy(machine.gameObject); }
            machine = new GameObject("Toy machine").transform; machine.SetParent(transform, false);
            all.Clear(); tokens.Clear(); selections.Clear(); tokenShadows.Clear(); floors.Clear(); supports.Clear(); interaction = model;
            upperAnchor = anchors[0]; lowerAnchor = receiver;
            for (int i = 0; i < 2; i++) supports.Add(Create("Brass guide " + i, machine, null, art.beam, art.paleGold, Vector2.zero, Vector2.one, .65f, .08f));
            for (int i = 0; i < anchors.Count; i++) BuildFloor(anchors[i], model.Attempt.Level.Floors[i], slots.Count);
            Add("Receiver contact", receiver, art.quad, art.shadow, new Vector2(.51f, .38f), new Vector2(1.07f, .96f), .7f, .01f);
            Add("Golden receiver rim", receiver, art.beam, art.gold, new Vector2(.5f, .48f), new Vector2(1, .76f), .35f, .30f);
            Add("Ivory inset", receiver, art.beam, art.paleGold, new Vector2(.5f, .53f), new Vector2(.93f, .64f), .12f, .18f);
            for (int i = 0; i < targets.Count; i++) {
                var anchor = targets[i].rectTransform;
                Add("Target socket", anchor, art.tile, art.gold, new Vector2(.5f, .5f), new Vector2(.83f, .90f), -.06f, .06f);
                var symbol = model.Attempt.Level.TargetSymbolIds[i];
                Add("Target " + i, anchor, art.SymbolMesh(symbol), art.SymbolMaterial(symbol), new Vector2(.5f, .5f), new Vector2(.66f, .72f), -.12f, .10f);
            }
            for (int i = 0; i < slots.Count; i++) {
                var anchor = (RectTransform)slots[i].transform;
                tokenShadows.Add(Add("Token contact " + i, anchor, art.quad, art.shadow, new Vector2(.53f, .22f), new Vector2(1.04f, .35f), .04f, .01f));
                selections.Add(Add("Selection rim " + i, anchor, art.tile, art.gold, new Vector2(.5f, .48f), new Vector2(.94f, 1.04f), -.18f, .12f));
                tokens.Add(Add("Persistent token " + i, anchor, art.tile, art.cream, new Vector2(.5f, .5f), new Vector2(.78f, .88f), -.35f, .22f));
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
            P("Mint platform", art.beam, art.teal, .5f, .33f, 1, .19f, .15f);
            P("Cream rail", art.beam, art.cream, .5f, .41f, .94f, .045f, -.02f);
            P("Left brass end", art.disk, art.gold, .03f, .33f, .045f, .14f, -.12f);
            P("Right brass end", art.disk, art.gold, .97f, .33f, .045f, .14f, -.12f);
            for (int i = 0; i < 3; i++) rig.cracks.Add(P("Damage seam " + i, art.beam, art.graphite, .28f + i * .23f, .33f, .055f, .023f, -.17f, i % 2 == 0 ? 65 : -58));
            for (int i = 0; i < count - 1; i++) {
                float x = .045f + .91f * (i + 1f) / count;
                var gate = P("Edge latch " + i, art.tile, art.gold, x, .55f, .025f, .18f, -.4f); rig.locks.Add(gate);
                if (floor.FragileEdges.Contains(i)) {
                    P("Fragile notch", art.beam, art.coral, x - .008f, .35f, .06f, .032f, -.21f, -45);
                    P("Fragile notch", art.beam, art.coral, x + .007f, .29f, .05f, .032f, -.22f, 45);
                }
                if (floor.LinkedPair.HasValue && floor.LinkedPair.Value.Contains(i)) {
                    P("Linked loop", art.disk, art.colors[3], x, .31f, .066f, .14f, -.23f);
                    P("Linked loop inset", art.disk, art.cream, x, .31f, .035f, .07f, -.33f);
                }
                if (floor.OneUseBridgeEdge == i) {
                    rig.bridge.Add(P("One use bridge", art.beam, art.gold, x, .46f, .12f, .055f, -.4f));
                    rig.bridge.Add(P("Bridge hinge", art.disk, art.graphite, x, .46f, .024f, .045f, -.51f));
                }
            }
            if (floor.Bomb != null) Device("Bomb", floor.Bomb, rig.bomb, art.coral);
            if (floor.Key != null) Device("Key", floor.Key, rig.key, art.gold);
            if (floor.Repair != null) Device("Repair", floor.Repair, rig.repair, art.teal);
            if (floor.OptionalStamp != null) Device("Stamp", floor.OptionalStamp, rig.stamp, art.colors[3]);
            if (floor.EntryTransform != EntryTransform.None) {
                float angle = floor.EntryTransform == EntryTransform.ShiftLeftOne ? 180 : 0;
                P("Entry roller", art.disk, art.graphite, .10f, .30f, .045f, .10f, -.3f);
                P("Entry roller", art.disk, art.graphite, .90f, .30f, .045f, .10f, -.3f);
                P("Entry direction", art.shapes[0], art.gold, .5f, .31f, .06f, .1f, -.3f, -90 + angle);
                if (floor.EntryTransform == EntryTransform.MirrorNext) P("Mirror direction", art.shapes[0], art.gold, .43f, .31f, .06f, .1f, -.3f, 90);
            }
            void Device(string name, ContactDefinition contact, List<Part> parts, Material color) {
                float x = .045f + .91f * (contact.Slot + .5f) / count;
                parts.Add(P(name + " mount", art.disk, color, x, .31f, .12f, .34f, -.4f));
                parts.Add(P(name + " face", art.SymbolMesh(contact.RequiredSymbol), art.SymbolMaterial(contact.RequiredSymbol), x, .31f, .077f, .245f, -.54f));
                if (name == "Bomb") parts.Add(P("Bomb timer cap", art.tile, art.graphite, x, .47f, .035f, .055f, -.41f));
                if (name == "Repair") {
                    parts.Add(P("Repair cross", art.beam, art.cream, x + .06f, .3f, .037f, .03f, -.5f));
                    parts.Add(P("Repair cross", art.beam, art.cream, x + .06f, .3f, .01f, .09f, -.51f));
                }
                if (name == "Key") {
                    parts.Add(P("Key shaft", art.beam, art.gold, x + .067f, .3f, .06f, .055f, -.4f));
                    parts.Add(P("Key tooth", art.tile, art.gold, x + .09f, .27f, .018f, .1f, -.4f));
                }
                if (name == "Stamp") parts.Add(P("Stamp handle", art.tile, art.colors[3], x, .16f, .03f, .09f, -.4f));
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
            return new Part { transform = go.transform, renderer = renderer, mesh = mesh, anchor = anchor, point = point, size = size, z = z, depth = depth, angle = angle };
        }
        void Place(Part part)
        {
            part.anchor.GetWorldCorners(corners); var size = corners[2] - corners[0];
            part.transform.position = new Vector3(corners[0].x + size.x * part.point.x, corners[0].y + size.y * part.point.y, part.z);
            part.transform.rotation = Quaternion.Euler(0, 0, part.angle);
            var bounds = part.mesh.bounds.size;
            float width = size.x * part.size.x, height = size.y * part.size.y;
            if (part.mesh != art.beam && part.mesh != art.quad) { float fit = Mathf.Min(width / bounds.x, height / bounds.y); width = fit * bounds.x; height = fit * bounds.y; }
            part.transform.localScale = new Vector3(width / bounds.x, height / bounds.y, part.depth / Mathf.Max(.01f, bounds.z));
        }
        public void Sync()
        {
            if (viewCamera == null) return;
            float height = viewCamera.orthographicSize * 2, width = height * viewCamera.aspect;
            var texture = art.backdrop.mainTexture; float ratio = texture == null ? .6667f : (float)texture.width / texture.height;
            float coverH = Mathf.Max(height, width / ratio);
            backdrop.transform.position = new Vector3(viewCamera.transform.position.x, viewCamera.transform.position.y, 3);
            backdrop.transform.localScale = new Vector3(coverH * ratio, coverH, 1);
            windmill.position = new Vector3(-Mathf.Min(width * .44f, 3.1f), -3.1f, 1.7f);
            if (interaction == null) return;
            var state = interaction.Attempt.Current;
            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i]; var mesh = art.SymbolMesh(state.Row[i]);
                token.mesh = mesh; token.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
                token.renderer.sharedMaterial = art.SymbolMaterial(state.Row[i]);
                token.point = new Vector2(.5f, TokenFaceY(interaction.SelectedSlot == i));
                token.size = interaction.SelectedSlot == i ? new Vector2(.81f, .915f) : new Vector2(.78f, .88f);
                selections[i].transform.gameObject.SetActive(interaction.SelectedSlot == i && token.anchor.gameObject.activeInHierarchy);
                token.transform.gameObject.SetActive(token.anchor.gameObject.activeInHierarchy);
                tokenShadows[i].transform.gameObject.SetActive(token.anchor.gameObject.activeInHierarchy);
            }
            for (int f = 0; f < floors.Count; f++) {
                var rig = floors[f]; bool past = f < state.ActiveFloor || state.Status != PuzzleStatus.Playing;
                bool active = f == state.ActiveFloor && state.Status == PuzzleStatus.Playing;
                foreach (var p in rig.parts) p.transform.gameObject.SetActive(!past);
                int hp = active ? state.Durability : interaction.Attempt.Level.Floors[f].BaseDurability;
                int damage = Mathf.Clamp(interaction.Attempt.Level.Floors[f].BaseDurability - hp, 0, 3);
                for (int j = 0; j < rig.cracks.Count; j++) rig.cracks[j].transform.gameObject.SetActive(!past && j < damage);
                for (int edge = 0; edge < rig.locks.Count; edge++) {
                    bool open = active ? interaction.Attempt.Resolver.IsEdgeOpen(state, edge) : interaction.Attempt.Level.Floors[f].OpenEdges[edge];
                    rig.locks[edge].transform.gameObject.SetActive(!past && !open);
                }
                SetVisible(rig.bridge, !past && !(active && state.BridgeUsed));
                SetVisible(rig.bomb, !past && (!active || state.BombPhase == BombPhase.Armed));
                SetVisible(rig.key, !past && !(active && state.KeyUsed));
                SetVisible(rig.repair, !past && !(active && state.RepairUsed));
                SetVisible(rig.stamp, !past && (state.StampBits & (1 << f)) == 0);
            }
            foreach (var part in all) if (part.transform.gameObject.activeSelf) Place(part);
            upperAnchor.GetWorldCorners(corners); float left = corners[0].x, right = corners[2].x;
            float top = Mathf.Lerp(corners[0].y, corners[2].y, .34f);
            lowerAnchor.GetWorldCorners(corners); float bottom = Mathf.Lerp(corners[0].y, corners[2].y, .55f);
            for (int i = 0; i < supports.Count; i++) {
                var support = supports[i]; support.transform.position = new Vector3(Mathf.Lerp(left, right, i == 0 ? .035f : .965f), (top + bottom) * .5f, .65f);
                support.transform.rotation = Quaternion.Euler(0, 0, 90);
                support.transform.localScale = new Vector3((top - bottom) / art.beam.bounds.size.x, .035f, .24f);
            }
        }
        static void SetVisible(List<Part> parts, bool visible) { foreach (var p in parts) p.transform.gameObject.SetActive(visible); }
        void LateUpdate()
        {
            if (!reducedMotion && interaction != null && !interaction.IsPaused && !interaction.IsSuspended) windTime += Time.unscaledDeltaTime;
            if (windmill != null) windmill.localRotation = Quaternion.Euler(0, 0, reducedMotion ? 18 : 18 + windTime * 7);
        }
    }
}
