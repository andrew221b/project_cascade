using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ChevronGraphic : MaskableGraphic
    {
        public bool PointsRight;
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear(); var rect = rectTransform.rect; float scale = Mathf.Min(rect.width, rect.height);
            var points = new[] { new Vector2(.34f,.24f), new Vector2(.42f,.17f), new Vector2(.72f,.50f),
                new Vector2(.42f,.83f), new Vector2(.34f,.76f), new Vector2(.58f,.50f) };
            foreach (var p in points) helper.AddVert(rect.center + new Vector2((PointsRight ? p.x : 1-p.x)-.5f,p.y-.5f)*scale,color,Vector2.zero);
            helper.AddTriangle(0,1,5); helper.AddTriangle(1,2,5); helper.AddTriangle(2,3,5); helper.AddTriangle(3,4,5);
        }
    }
}
