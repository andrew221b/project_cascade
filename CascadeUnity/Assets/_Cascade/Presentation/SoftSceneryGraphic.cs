using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    // Feather only empty sky at the artwork boundary; map hit points remain in the same rect.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SoftSceneryGraphic : MaskableGraphic
    {
        Sprite source;
        Material availability;
        Texture2D sky;Camera viewportCamera;float previousAspect=-1;
        public void SetAvailability(Vector2[] points,int unlocked)
        {
            var shader=Resources.Load<Shader>("CascadeArt/MapAvailability");if(shader==null)return;
            availability=new Material(shader);material=availability;
            for(int i=0;i<5;i++)availability.SetVector("_Island"+i,new Vector4(points[i].x,points[i].y,0,0));
            availability.SetFloat("_Unlocked",unlocked);
        }
        public void SetSky(Texture2D texture,Camera camera)
        {sky=texture;viewportCamera=camera;if(availability!=null&&sky!=null){availability.SetTexture("_SkyTex",sky);availability.SetFloat("_BlendSky",1);LateUpdate();}}
        void LateUpdate()
        {
            if(availability==null||sky==null||viewportCamera==null||Mathf.Approximately(previousAspect,viewportCamera.aspect))return;
            previousAspect=viewportCamera.aspect;float ratio=sky.width/(float)sky.height;
            float width=Mathf.Max(1,ratio/previousAspect),height=Mathf.Max(1,previousAspect/ratio);
            availability.SetVector("_SkyRect",new Vector4((1-width)*.5f,(1-height)*.5f,width,height));
        }
        protected override void OnDestroy(){if(availability!=null)Destroy(availability);base.OnDestroy();}
        public override Texture mainTexture=>source==null?base.mainTexture:source.texture;
        public void Initialize(Sprite sprite){source=sprite;raycastTarget=false;SetAllDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(source==null)return;
            var r=rectTransform.rect;var uv=UnityEngine.Sprites.DataUtility.GetOuterUV(source);const int steps=20;
            for(int y=0;y<=steps;y++)for(int x=0;x<=steps;x++){
                float u=x/(float)steps,v=y/(float)steps;
                var c=color;c.a*=Mathf.SmoothStep(0,1,Mathf.Min(u,1-u)/.12f)*Mathf.SmoothStep(0,1,Mathf.Min(v,1-v)/.12f);
                vh.AddVert(new Vector3(r.xMin+r.width*u,r.yMin+r.height*v),c,new Vector2(Mathf.Lerp(uv.x,uv.z,u),Mathf.Lerp(uv.y,uv.w,v)));
            }
            for(int y=0;y<steps;y++)for(int x=0;x<steps;x++){int i=y*(steps+1)+x;vh.AddTriangle(i,i+steps+1,i+1);vh.AddTriangle(i+1,i+steps+1,i+steps+2);}
        }
    }
}
