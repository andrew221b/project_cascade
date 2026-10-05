using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // The magnifier uses the same authored silhouette as the world mesh.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ShapeFaceGraphic : MaskableGraphic
    {
        Mesh source;
        public void SetShape(Mesh mesh, Color shade) { if(source==mesh&&color==shade)return;source = mesh; color = shade; raycastTarget = false; SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear(); if (source == null) return;
            var vertices = source.vertices; int count = (vertices.Length - 2) / 5;
            var bounds = source.bounds; var rect = rectTransform.rect;
            float scale = Mathf.Min(rect.width / bounds.size.x, rect.height / bounds.size.y) * .88f;
            helper.AddVert(rect.center, color, Vector2.zero);
            for (int i = 0; i < count; i++) helper.AddVert(rect.center + (Vector2)vertices[count * 2 + i] * scale, color, Vector2.zero);
            for (int i = 0; i < count; i++) helper.AddTriangle(0, i + 1, (i + 1) % count + 1);
        }
    }
}
