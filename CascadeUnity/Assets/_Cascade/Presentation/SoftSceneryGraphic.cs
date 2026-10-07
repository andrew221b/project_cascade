using UnityEngine;
using System.Linq;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    // Feather only empty sky at the artwork boundary; map hit points remain in the same rect.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SoftSceneryGraphic : MaskableGraphic
    {
        Sprite source;
        CascadeGame game;Rect crop=new Rect(0,0,1,1);Vector2[] centers;readonly Vector2[] islandDrift=new Vector2[5];float phase,spacing;
        public void SetMotion(CascadeGame owner,Rect sourceCrop){game=owner;crop=sourceCrop;}
        public void SetIslandSpacing(float gap){spacing=gap;SetVerticesDirty();}
        public Vector2 Displacement(Vector2 uv)
        {
            if(game==null||centers==null)return Vector2.zero;
            Vector2 sum=Vector2.zero;float total=0;
            for(int i=0;i<centers.Length;i++){
                var d=uv-centers[i];d.y*=source.rect.height/source.rect.width;
                float squared=d.sqrMagnitude+.0001f,weight=1/(squared*squared);
                sum+=((game.ReducedMotion?Vector2.zero:islandDrift[i])+Vector2.up*((i-2)*spacing))*weight;total+=weight;
            }
            return sum/Mathf.Max(.001f,total);
        }
        Material availability;
        Texture2D sky;Camera viewportCamera;float previousAspect=-1;
        public void SetAvailability(Vector2[] points,int unlocked)
            =>SetAvailability(points,Enumerable.Range(0,5).Select(i=>i<=unlocked).ToArray());
        public void SetAvailability(Vector2[] points,bool[] open)
        {
            centers=points;
            var shader=Resources.Load<Shader>("CascadeArt/MapAvailability");if(shader==null)return;
            availability=new Material(shader);material=availability;
            for(int i=0;i<5;i++)availability.SetVector("_Island"+i,new Vector4(points[i].x,points[i].y,open[i]?1:0,0));
        }
        public void SetSky(Texture2D texture,Camera camera,bool night=false)
        {sky=texture;viewportCamera=camera;if(availability!=null&&sky!=null){availability.SetTexture("_SkyTex",sky);availability.SetFloat("_BlendSky",1);availability.SetFloat("_DarkSky",night?1:0);LateUpdate();}}
        void LateUpdate()
        {
            if(game!=null){float next=game.ReducedMotion?0:game.Machine.AmbientPhase;if(!Mathf.Approximately(phase,next)){phase=next;for(int i=0;i<5;i++)islandDrift[i]=new Vector2(Mathf.Sin(phase*.31f+i*1.73f)*.65f,Mathf.Sin(phase*.55f+i*1.73f)*2.4f);SetVerticesDirty();}}
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
            var r=rectTransform.rect;var uv=UnityEngine.Sprites.DataUtility.GetOuterUV(source);const int steps=36;
            for(int y=0;y<=steps;y++)for(int x=0;x<=steps;x++){
                float u=x/(float)steps,v=y/(float)steps;
                var c=color;c.a*=Mathf.SmoothStep(0,1,Mathf.Min(u,1-u)/.12f)*Mathf.SmoothStep(0,1,Mathf.Min(v,1-v)/.12f);
                var sample=new Vector2(crop.x+u*crop.width,crop.y+v*crop.height);
                var drift=Displacement(sample);
                vh.AddVert(new Vector3(r.xMin+r.width*u+drift.x,r.yMin+r.height*v+drift.y),c,new Vector2(Mathf.Lerp(uv.x,uv.z,sample.x),Mathf.Lerp(uv.y,uv.w,sample.y)));
            }
            for(int y=0;y<steps;y++)for(int x=0;x<steps;x++){int i=y*(steps+1)+x;vh.AddTriangle(i,i+steps+1,i+1);vh.AddTriangle(i+1,i+steps+1,i+steps+2);}
        }
    }
    public sealed class MapIslandAnchor:MonoBehaviour
    {
        SoftSceneryGraphic scenery;RectTransform rect;Vector2 uv;
        public void Initialize(SoftSceneryGraphic view,Vector2 point){scenery=view;rect=(RectTransform)transform;uv=point;}
        void LateUpdate(){if(scenery!=null)rect.anchoredPosition=scenery.Displacement(uv);}
    }
    public sealed class MapFlight:MonoBehaviour
    {
        CascadeGame game;RectTransform rect;
        public void Initialize(CascadeGame owner){game=owner;rect=(RectTransform)transform;}
        void LateUpdate(){if(game==null)return;float t=game.ReducedMotion?0:game.Machine.AmbientPhase*.12f;
            rect.anchorMin=rect.anchorMax=new Vector2(.5f+Mathf.Sin(t)*.36f,.65f+Mathf.Sin(t*2)*.045f);
            float depth=.72f+.18f*Mathf.Cos(t);rect.localScale=new Vector3(depth*Mathf.Clamp(Mathf.Cos(t)*4,-1,1),depth,1);
            rect.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t)*-7);}
    }

}
