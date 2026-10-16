using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    // Camera-space light only. All motion reads the owner's presentation phase.
    public sealed class WorldNightSkyView : MonoBehaviour
    {
        sealed class Glint
        {
            public Transform Transform;
            public MeshRenderer Renderer;
            public readonly MaterialPropertyBlock Tint = new MaterialPropertyBlock();
        }

        static readonly Vector2[] places = {
            new Vector2(.08f, .91f), new Vector2(.20f, .84f),
            new Vector2(.34f, .94f), new Vector2(.46f, .87f),
            new Vector2(.60f, .93f), new Vector2(.73f, .84f),
            new Vector2(.89f, .94f), new Vector2(.94f, .79f),
            new Vector2(.045f, .69f), new Vector2(.965f, .61f),
            new Vector2(.08f, .45f), new Vector2(.94f, .35f),
            new Vector2(.11f, .22f), new Vector2(.88f, .16f),
            new Vector2(.06f, .13f), new Vector2(.93f, .08f)
        };
        static readonly int TintColor = Shader.PropertyToID("_Color");
        readonly Glint[] stars = new Glint[16];
        MaterialPropertyBlock meteorTint;
        Camera cameraView;
        Material lightMaterial;
        Mesh starMesh, trailMesh;
        Transform meteorHead, meteorTrail;
        MeshRenderer headRenderer, trailRenderer;
        bool initialized;
        public bool Ready => initialized && cameraView != null && lightMaterial != null;
        public int ActiveStarCount { get; private set; }
        public bool MeteorVisible { get; private set; }

        public void Initialize(ToyArt art, Camera camera)
        {
            cameraView = camera;
            if (initialized || art == null || art.quad == null || camera == null) return;
            var shader = Resources.Load<Shader>("CascadeArt/SoftWeather");
            if (shader == null) {
                Debug.LogWarning("CASCADE night light needs the existing SoftWeather shader.", this);
                return;
            }
            lightMaterial = new Material(shader) { name = "Quiet night light", renderQueue = 3000 };
            meteorTint = new MaterialPropertyBlock();
            starMesh = MakeStarMesh();
            trailMesh = new Mesh { name = "Tapered distant meteor" };
            trailMesh.vertices = new[] {
                new Vector3(-1, -.015f), new Vector3(0, -.5f),
                new Vector3(0, .5f), new Vector3(-1, .015f)
            };
            // The tail starts at zero opacity; its broad end meets the soft head.
            trailMesh.uv = new[] {
                new Vector2(0, .485f), new Vector2(.62f, 0),
                new Vector2(.62f, 1), new Vector2(0, .515f)
            };
            trailMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            trailMesh.RecalculateBounds();
            for (int i = 0; i < stars.Length; i++) {
                var renderer = MakeLight("Distant star " + i, starMesh);
                stars[i] = new Glint { Transform = renderer.transform, Renderer = renderer };
            }
            headRenderer = MakeLight("Meteor soft head", art.quad); meteorHead = headRenderer.transform;
            trailRenderer = MakeLight("Meteor tapered trail", trailMesh); meteorTrail = trailRenderer.transform;
            initialized = true;
        }

        public void Paint(float phase, bool visible, bool reducedMotion, int quality)
        {
            if (!Ready || !visible || quality <= 0 || float.IsNaN(phase) || float.IsInfinity(phase)) {
                Hide(); return;
            }
            phase = reducedMotion ? 0 : Mathf.Max(0, phase);
            float distance = Mathf.Max(cameraView.nearClipPlane + .5f,
                Vector3.Dot(new Vector3(cameraView.transform.position.x, cameraView.transform.position.y, 3.05f)
                    - cameraView.transform.position, cameraView.transform.forward));
            float height = cameraView.orthographic ? cameraView.orthographicSize * 2
                : 2 * distance * Mathf.Tan(cameraView.fieldOfView * .5f * Mathf.Deg2Rad);
            float width = height * cameraView.aspect, unit = Mathf.Min(width, height);
            ActiveStarCount = 0;
            for (int i = 0; i < stars.Length; i++) {
                var star = stars[i];
                bool show = quality > 1 || i % 2 == 0;
                star.Renderer.enabled = show;
                if (!show) continue;
                float pulse = NightSkyMotion.StarPulse(phase, i);
                float size = unit * (i % 3 == 0 ? .007f : .0047f) * Mathf.Lerp(.94f, 1.10f, pulse);
                var point = places[i];
                star.Transform.position = cameraView.ViewportToWorldPoint(new Vector3(point.x, point.y, distance));
                star.Transform.rotation = cameraView.transform.rotation;
                star.Transform.localScale = new Vector3(size, size, 1);
                star.Tint.SetColor(TintColor, new Color(.86f, .94f, 1, .20f + .30f * pulse));
                star.Renderer.SetPropertyBlock(star.Tint);
                ActiveStarCount++;
            }
            NightSkyMotion.Meteor meteor = default;
            MeteorVisible = !reducedMotion && NightSkyMotion.TryMeteor(phase, out meteor);
            headRenderer.enabled = trailRenderer.enabled = MeteorVisible;
            if (!MeteorVisible) return;
            var direction = meteor.Direction;
            float angle = Mathf.Atan2(direction.y * height, direction.x * width) * Mathf.Rad2Deg;
            var rotation = cameraView.transform.rotation * Quaternion.Euler(0, 0, angle);
            var position = cameraView.ViewportToWorldPoint(new Vector3(meteor.Head.x, meteor.Head.y, distance));
            meteorHead.position = meteorTrail.position = position;
            meteorHead.rotation = meteorTrail.rotation = rotation;
            float headSize = unit * .0052f;
            meteorHead.localScale = new Vector3(headSize, headSize, 1);
            meteorTrail.localScale = new Vector3(unit * .075f * meteor.Trail, unit * .0038f, 1);
            if(meteorTint==null)meteorTint=new MaterialPropertyBlock();
            meteorTint.SetColor(TintColor, new Color(.82f, .93f, 1, .64f * meteor.Alpha));
            headRenderer.SetPropertyBlock(meteorTint);
            meteorTint.SetColor(TintColor, new Color(.72f, .86f, 1, .30f * meteor.Alpha));
            trailRenderer.SetPropertyBlock(meteorTint);
        }

        MeshRenderer MakeLight(string label, Mesh mesh)
        {
            var go = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer));
            go.layer = gameObject.layer; go.transform.SetParent(transform, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = lightMaterial; renderer.enabled = false;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off; renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.allowOcclusionWhenDynamic = false;
            return renderer;
        }

        static Mesh MakeStarMesh()
        {
            var mesh = new Mesh { name = "Small soft star glint" };
            var vertices = new Vector3[12]; var uv = new Vector2[12]; var triangles = new int[18];
            Quad(0, .17f, .17f); Quad(1, .5f, .032f); Quad(2, .032f, .5f);
            mesh.vertices = vertices; mesh.uv = uv; mesh.triangles = triangles; mesh.RecalculateBounds();
            return mesh;
            void Quad(int index, float x, float y)
            {
                int v = index * 4, t = index * 6;
                vertices[v] = new Vector3(-x, -y); vertices[v + 1] = new Vector3(x, -y);
                vertices[v + 2] = new Vector3(x, y); vertices[v + 3] = new Vector3(-x, y);
                uv[v] = Vector2.zero; uv[v + 1] = Vector2.right; uv[v + 2] = Vector2.one; uv[v + 3] = Vector2.up;
                triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
                triangles[t + 3] = v; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
            }
        }

        void Hide()
        {
            ActiveStarCount = 0; MeteorVisible = false;
            foreach (var star in stars) if (star != null && star.Renderer != null) star.Renderer.enabled = false;
            if (headRenderer != null) headRenderer.enabled = false;
            if (trailRenderer != null) trailRenderer.enabled = false;
        }
        void OnDisable() => Hide();
        void OnDestroy()
        {
            foreach (var star in stars) if (star != null && star.Transform != null) Destroy(star.Transform.gameObject);
            if (meteorHead != null) Destroy(meteorHead.gameObject);
            if (meteorTrail != null) Destroy(meteorTrail.gameObject);
            if (lightMaterial != null) Destroy(lightMaterial);
            if (starMesh != null) Destroy(starMesh);
            if (trailMesh != null) Destroy(trailMesh);
        }
    }

    // Shared immutable timings keep the gameplay and map lights on the same clock.
    internal static class NightSkyMotion
    {
        static readonly float[] intervals = { 28.4f, 34.1f, 24.8f, 31.5f, 26.2f, 29.8f };
        const float Period = 174.8f, FirstVisit = 19.3f;
        internal readonly struct Meteor
        {
            public readonly Vector2 Head, Direction;
            public readonly float Alpha, Trail;
            public Meteor(Vector2 head, Vector2 direction, float alpha, float trail)
            { Head = head; Direction = direction; Alpha = alpha; Trail = trail; }
        }

        internal static float StarPulse(float phase, int index)
        {
            float wave = .5f + .5f * Mathf.Sin(phase * (.43f + index % 4 * .055f) + index * 1.83f);
            return wave * wave;
        }

        internal static bool TryMeteor(float phase, out Meteor meteor)
        {
            meteor = default;
            if (phase < 0 || float.IsNaN(phase) || float.IsInfinity(phase)) return false;
            int cycle = Mathf.FloorToInt(phase / Period);
            float clock = phase - cycle * Period, start = FirstVisit;
            for (int i = 0; i < intervals.Length; i++) {
                float seed = Hash(cycle * 6 + i + 113);
                float duration = 1.2f + .5f * seed, age = clock - start;
                if (age >= 0 && age < duration) {
                    float progress = age / duration, direction = Hash(cycle * 6 + i + 47) < .5f ? -1 : 1;
                    float x = direction > 0 ? .13f + seed * .18f : .87f - seed * .18f;
                    var a = new Vector2(x, .925f - seed * .035f);
                    var delta = new Vector2(direction * (.27f + seed * .09f), -.065f - seed * .025f);
                    float fade = Mathf.SmoothStep(0, 1, progress / .12f)
                        * Mathf.SmoothStep(0, 1, (1 - progress) / .24f);
                    meteor = new Meteor(a + delta * progress, delta.normalized, fade, Mathf.SmoothStep(0, 1, progress / .22f));
                    return true;
                }
                start += intervals[i];
            }
            return false;
        }

        static float Hash(int value)
        {
            uint hash = unchecked((uint)value * 747796405u + 2891336453u);
            hash = ((hash >> (int)((hash >> 28) + 4)) ^ hash) * 277803737u;
            return ((hash >> 22) ^ hash) / (float)uint.MaxValue;
        }
    }
}
