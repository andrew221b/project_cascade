using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ChevronGraphic : MaskableGraphic
    {
        public bool PointsRight;
        public int Weight;
        public override Texture mainTexture=>NavigationArrowArt.Texture;
        protected override void OnPopulateMesh(VertexHelper helper)=>NavigationArrowArt.Draw(helper,rectTransform.rect,Weight,PointsRight,color);
    }
}
