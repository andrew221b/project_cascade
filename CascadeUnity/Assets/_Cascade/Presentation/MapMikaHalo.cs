using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // The original sprite supplies only alpha; neither the hero nor its material is changed.
    public sealed class MapMikaHalo : Image
    {
        static readonly int UvRectId = Shader.PropertyToID("_SpriteUVRect");
        static readonly int GlowStepId = Shader.PropertyToID("_GlowStep");
        CascadeGame game;
        Image original;
        Material haloMaterial;
        float previousAlpha = -1;

        public static MapMikaHalo Attach(CascadeGame owner, Image hero)
        {
            if (owner == null || hero == null || hero.sprite == null) return null;
            var shader = Resources.Load<Shader>("CascadeArt/MapMikaHalo");
            if (shader == null) return null;
            var go = new GameObject("Mika silhouette halo", typeof(RectTransform));
            go.transform.SetParent(hero.transform.parent, false);
            go.transform.SetSiblingIndex(hero.transform.GetSiblingIndex());
            var halo = go.AddComponent<MapMikaHalo>();
            halo.game = owner; halo.original = hero;
            halo.haloMaterial = new Material(shader) { name = "Mika soft ivory silhouette" };
            halo.material = halo.haloMaterial;
            halo.color = Color.white;
            halo.raycastTarget = false; halo.maskable = hero.maskable;
            halo.type = Type.Simple; halo.useSpriteMesh = false;
            halo.Synchronize();
            halo.canvasRenderer.SetAlpha(.46f);
            return halo;
        }

        void Synchronize()
        {
            if (original == null) return;
            var source = original.rectTransform;
            var target = rectTransform;
            if (target.anchorMin != source.anchorMin) target.anchorMin = source.anchorMin;
            if (target.anchorMax != source.anchorMax) target.anchorMax = source.anchorMax;
            if (target.pivot != source.pivot) target.pivot = source.pivot;
            if (target.sizeDelta != source.sizeDelta) target.sizeDelta = source.sizeDelta;
            if (target.anchoredPosition3D != source.anchoredPosition3D) target.anchoredPosition3D = source.anchoredPosition3D;
            if (target.localRotation != source.localRotation) target.localRotation = source.localRotation;
            if (target.localScale != source.localScale) target.localScale = source.localScale;
            var activeSprite = original.overrideSprite != null ? original.overrideSprite : original.sprite;
            if (sprite != activeSprite) sprite = activeSprite;
            if (preserveAspect != original.preserveAspect) preserveAspect = original.preserveAspect;
        }

        void LateUpdate()
        {
            Synchronize();
            bool visible = original != null && original.isActiveAndEnabled;
            bool quiet = game == null || game.ReducedMotion || game.Interaction == null || game.Interaction.IsSuspended;
            float phase = quiet || game.Machine == null ? 0 : game.Machine.AmbientPhase;
            float alpha = visible ? .46f + (quiet ? 0 : Mathf.Sin(phase * 1.35f) * .11f) : 0;
            if (Mathf.Abs(alpha - previousAlpha) < .001f) return;
            previousAlpha = alpha;
            // Renderer alpha pulses without rebuilding the sprite mesh or creating materials.
            canvasRenderer.SetAlpha(alpha);
        }

        protected override void OnPopulateMesh(VertexHelper helper)
        {
            base.OnPopulateMesh(helper);
            if (haloMaterial == null || helper.currentVertCount != 4) return;
            UIVertex vertex = default;
            Vector2 minimum = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maximum = new Vector2(float.MinValue, float.MinValue);
            Vector2 uvMinimum = minimum, uvMaximum = maximum;
            for (int i = 0; i < 4; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);
                minimum = Vector2.Min(minimum, vertex.position); maximum = Vector2.Max(maximum, vertex.position);
                uvMinimum = Vector2.Min(uvMinimum, vertex.uv0); uvMaximum = Vector2.Max(uvMaximum, vertex.uv0);
            }
            Vector2 size = maximum - minimum;
            if (size.x <= 0 || size.y <= 0) return;
            Vector2 center = (minimum + maximum) * .5f;
            Vector2 uvPerUnit = new Vector2((uvMaximum.x - uvMinimum.x) / size.x, (uvMaximum.y - uvMinimum.y) / size.y);
            float radius = Mathf.Max(2, size.y * .085f), padding = radius * 1.15f;
            haloMaterial.SetVector(UvRectId, new Vector4(uvMinimum.x, uvMinimum.y, uvMaximum.x, uvMaximum.y));
            haloMaterial.SetVector(GlowStepId, new Vector4(uvPerUnit.x * radius, uvPerUnit.y * radius, 0, 0));
            for (int i = 0; i < 4; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);
                Vector2 expansion = new Vector2(vertex.position.x < center.x ? -padding : padding, vertex.position.y < center.y ? -padding : padding);
                vertex.position += (Vector3)expansion;
                vertex.uv0 += new Vector4(expansion.x * uvPerUnit.x, expansion.y * uvPerUnit.y, 0, 0);
                helper.SetUIVertex(vertex, i);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (haloMaterial != null) Destroy(haloMaterial);
        }
    }
}
