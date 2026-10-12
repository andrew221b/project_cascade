using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Sparse light in the map's open sky and margins, behind its islands and controls.
    public sealed class NightSkyGraphic : MaskableGraphic
    {
        static readonly Vector2[] places = {
            new Vector2(.09f, .84f), new Vector2(.23f, .82f),
            new Vector2(.38f, .86f), new Vector2(.55f, .81f),
            new Vector2(.70f, .86f), new Vector2(.91f, .83f),
            new Vector2(.055f, .73f), new Vector2(.96f, .68f),
            new Vector2(.08f, .59f), new Vector2(.94f, .53f),
            new Vector2(.05f, .44f), new Vector2(.95f, .37f),
            new Vector2(.09f, .31f), new Vector2(.88f, .29f),
            new Vector2(.13f, .22f), new Vector2(.94f, .21f)
        };
        CascadeGame game;
        float paintedPhase = float.NaN;
        int paintedQuality = -1;
        bool paintedReducedMotion;

        public void Initialize(CascadeGame owner)
        {
            game = owner; raycastTarget = false; paintedPhase = float.NaN; SetVerticesDirty();
        }
        void Update()
        {
            if (game == null) return;
            int quality = game.Preferences.EffectQuality;
            bool reduced = game.ReducedMotion;
            float phase = Phase(reduced);
            // Phase stops with pause/focus, so an idle map does not request mesh rebuilds.
            if (quality != paintedQuality || reduced != paintedReducedMotion || float.IsNaN(paintedPhase)
                || quality > 0 && !reduced && (phase < paintedPhase || phase - paintedPhase >= 1f / 30))
                SetVerticesDirty();
        }
        float Phase(bool reduced)
        {
            float phase = reduced || game.Machine == null ? 0 : game.Machine.AmbientPhase;
            return float.IsNaN(phase) || float.IsInfinity(phase) ? 0 : Mathf.Max(0, phase);
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (game == null) return;
            paintedReducedMotion = game.ReducedMotion; paintedQuality = game.Preferences.EffectQuality;
            paintedPhase = Phase(paintedReducedMotion);
            if (paintedQuality <= 0) return;
            var r = rectTransform.rect;
            float unit = Mathf.Clamp(r.width / 390f, .75f, 1.8f);
            for (int i = 0; i < places.Length; i++) {
                if (paintedQuality == 1 && i % 2 != 0) continue;
                float pulse = NightSkyMotion.StarPulse(paintedPhase, i);
                var tint = new Color(.87f, .95f, 1, .21f + .29f * pulse);
                float size = (i % 3 == 0 ? 2.15f : 1.35f) * unit * Mathf.Lerp(.94f, 1.10f, pulse);
                Vector2 p = MapPoint(places[i], r);
                SoftGlint(vh, p, size, tint);
            }
            if (paintedReducedMotion || !NightSkyMotion.TryMeteor(paintedPhase, out var meteor)) return;
            // This Graphic is the first map atmosphere child, beneath painted islands and UI.
            Vector2 head = MapPoint(meteor.Head, r);
            Vector2 direction = new Vector2(meteor.Direction.x * r.width, meteor.Direction.y * r.height).normalized;
            Vector2 tail = head - direction * (Mathf.Min(r.width, r.height) * .075f * meteor.Trail);
            float trailWidth = 1.0f * unit;
            Vector2 normal = new Vector2(-direction.y, direction.x) * trailWidth;
            var headTint = new Color(.82f, .93f, 1, .52f * meteor.Alpha);
            var tailTint = new Color(.72f, .86f, 1, 0);
            int index = vh.currentVertCount;
            vh.AddVert(tail, tailTint, Vector2.zero);
            vh.AddVert(head - normal, tailTint, Vector2.zero);
            vh.AddVert(head, new Color(.72f, .86f, 1, .27f * meteor.Alpha), Vector2.zero);
            vh.AddVert(head + normal, tailTint, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2); vh.AddTriangle(index, index + 2, index + 3);
            SoftGlint(vh, head, 1.75f * unit, headTint);
        }

        static Vector2 MapPoint(Vector2 point, Rect rect)
            => new Vector2(rect.x + point.x * rect.width, rect.y + point.y * rect.height);

        static void SoftGlint(VertexHelper vh, Vector2 point, float size, Color tint)
        {
            AddDiamond(vh, point, Vector2.right * size, Vector2.up * size * .18f, tint);
            AddDiamond(vh, point, Vector2.up * size, Vector2.right * size * .18f, tint);
            AddDiamond(vh, point, Vector2.right * size * .38f, Vector2.up * size * .38f, tint);
        }
        static void AddDiamond(VertexHelper vh, Vector2 point, Vector2 a, Vector2 b, Color tint)
        {
            int index = vh.currentVertCount;
            var edge = new Color(tint.r, tint.g, tint.b, 0);
            vh.AddVert(point, tint, Vector2.zero);
            vh.AddVert(point - a, edge, Vector2.zero); vh.AddVert(point + b, edge, Vector2.zero);
            vh.AddVert(point + a, edge, Vector2.zero); vh.AddVert(point - b, edge, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2); vh.AddTriangle(index, index + 2, index + 3);
            vh.AddTriangle(index, index + 3, index + 4); vh.AddTriangle(index, index + 4, index + 1);
        }
    }
}
