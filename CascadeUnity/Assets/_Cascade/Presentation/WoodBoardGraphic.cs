using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoodBoardGraphic:MaskableGraphic
    {
        Texture texture;Rect uv=new Rect(0,0,1,1);int surfaceStyle=-1;
        public void SetSurface(Cascade.Domain.FloorDefinition floor){surfaceStyle=PlatformSurfaceArt.Style(floor);texture=surfaceStyle<0?Resources.Load<Texture2D>("CascadeArt/Reactions/TimberStates"):PlatformSurfaceArt.Texture;SetDamage(0);}
        public void SetDamage(int stage){var f=surfaceStyle<0?WorkshopSpriteSet.WoodUv(stage):PlatformSurfaceArt.Uv(surfaceStyle,stage);uv=new Rect(f.z,f.w,f.x,f.y);SetVerticesDirty();}
        public override Texture mainTexture=>texture!=null?texture:Texture2D.whiteTexture;
        protected override void Awake()
        {
            base.Awake();texture=Resources.Load<Texture2D>("CascadeArt/Reactions/TimberStates");SetDamage(0);
            raycastTarget=false;
        }
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();Rect r=rectTransform.rect;
            void Slice(float left,float right,float from,float to){
                int start=helper.currentVertCount;
                helper.AddVert(new Vector3(left,r.yMin),color,new Vector2(from,uv.yMin));helper.AddVert(new Vector3(left,r.yMax),color,new Vector2(from,uv.yMax));
                helper.AddVert(new Vector3(right,r.yMax),color,new Vector2(to,uv.yMax));helper.AddVert(new Vector3(right,r.yMin),color,new Vector2(to,uv.yMin));
                helper.AddTriangle(start,start+1,start+2);helper.AddTriangle(start,start+2,start+3);
            }
            if(surfaceStyle<0)Slice(r.xMin,r.xMax,uv.xMin,uv.xMax);
            else {
                float cap=Mathf.Min(r.height*.83f,r.width*.18f);
                Slice(r.xMin,r.xMin+cap,uv.xMin,uv.xMin+uv.width*.15f);
                Slice(r.xMin+cap,r.xMax-cap,uv.xMin+uv.width*.15f,uv.xMax-uv.width*.15f);
                Slice(r.xMax-cap,r.xMax,uv.xMax-uv.width*.15f,uv.xMax);
            }
        }
    }
}
