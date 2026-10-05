using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoodBoardGraphic:MaskableGraphic
    {
        Texture texture;Rect uv=new Rect(0,0,1,1);
        public override Texture mainTexture=>texture!=null?texture:Texture2D.whiteTexture;
        protected override void Awake()
        {
            base.Awake();texture=Resources.Load<Texture2D>("CascadeArt/Woodland/WoodBoard");
            var source=Resources.Load<TextAsset>("CascadeArt/Woodland/WoodBoardFrame");
            if(source!=null){var frame=JsonUtility.FromJson<WoodBodyFrame>(source.text);uv=new Rect(frame.left,frame.bottom,frame.width,frame.height);}
            raycastTarget=false;
        }
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();Rect r=rectTransform.rect;
            helper.AddVert(new Vector3(r.xMin,r.yMin),color,new Vector2(uv.xMin,uv.yMin));helper.AddVert(new Vector3(r.xMin,r.yMax),color,new Vector2(uv.xMin,uv.yMax));
            helper.AddVert(new Vector3(r.xMax,r.yMax),color,new Vector2(uv.xMax,uv.yMax));helper.AddVert(new Vector3(r.xMax,r.yMin),color,new Vector2(uv.xMax,uv.yMin));
            helper.AddTriangle(0,1,2);helper.AddTriangle(0,2,3);
        }
    }
}
