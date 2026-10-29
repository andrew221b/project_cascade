using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    // Up to 48 weather/fleck quads share one mesh/material; no per-frame objects or arrays.
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public sealed class WorldSeasonalWeather:MonoBehaviour
    {
        const int Capacity=48;
        readonly Vector3[] vertices=new Vector3[Capacity*4];readonly Vector2[] uv=new Vector2[Capacity*4],kinds=new Vector2[Capacity*4];
        readonly Color[] colors=new Color[Capacity*4];Mesh mesh;Material material;MeshRenderer face;
        public int ActiveCount{get;private set;}
        public void Initialize()
        {
            if(mesh!=null)return;
            mesh=new Mesh{name="Pooled seasonal weather batch"};mesh.MarkDynamic();var triangles=new int[Capacity*6];
            for(int i=0;i<Capacity;i++){int v=i*4,t=i*6;triangles[t]=v;triangles[t+1]=v+1;triangles[t+2]=v+2;triangles[t+3]=v;triangles[t+4]=v+2;triangles[t+5]=v+3;}
            mesh.vertices=vertices;mesh.triangles=triangles;GetComponent<MeshFilter>().sharedMesh=mesh;
            material=new Material(Resources.Load<Shader>("CascadeArt/SoftWeather")){name="Authored seasonal art and soft weather"};material.SetTexture("_MainTex",WorldSeasonalArt.Texture);
            face=GetComponent<MeshRenderer>();face.sharedMaterial=material;face.shadowCastingMode=ShadowCastingMode.Off;face.receiveShadows=false;face.lightProbeUsage=LightProbeUsage.Off;face.reflectionProbeUsage=ReflectionProbeUsage.Off;
        }
        public int Paint(float age,int world,int quality,float width,float height,Vector3 center,float wind=float.NaN,bool plantSupportVisible=true)
        {
            // Rooted artwork belongs to the architectural frame, even when weather stays visible.
            int count=WorldSeasonalArt.Rooted(world)&&!plantSupportVisible?WorldSeasonalArt.Procedural(world):WorldSeasonalArt.Count(world);
            ActiveCount=quality<=0?0:count/(quality==1?2:1);
            if(face==null)return 0;face.enabled=ActiveCount>0;if(ActiveCount==0)return 0;
            transform.position=center;transform.rotation=Quaternion.identity;transform.localScale=Vector3.one;
            var region=WorldSeasonalArt.Uv(world);int procedural=WorldSeasonalArt.Procedural(world)/(quality==1?2:1);
            for(int i=0;i<Capacity;i++){
                bool visible=i<ActiveCount,art=i>=procedural;var p=WorldSeasonalArt.Drift(age,i,world);
                float seed=WildlifeMotion.Hash(i*197+world*31+17);
                float fade=visible?Mathf.SmoothStep(0,1,Mathf.Min(p.x,1-p.x)/.06f)*Mathf.SmoothStep(0,1,Mathf.Min(p.y,1-p.y)/.06f):0;
                bool rain=world==1||world==4,snow=world==6;
                float size=Mathf.Min(width,height)*(art?(snow?.047f:rain?.017f:WorldSeasonalArt.Rooted(world)?.062f:.038f+seed*.010f):snow?.011f:.005f);
                var extent=art?new Vector2(size,size):rain?new Vector2(width*.0042f,height*.032f):Vector2.one*size;
                bool rooted=art&&WorldSeasonalArt.Rooted(world);int plant=i-procedural;bool right=plant%2==1;
                if(rooted){p=WorldFrameArt.FrameContact(right,plant<2?0:.75f,width/height);fade=visible?1:0;}
                float turn=rooted?WorldSeasonalArt.PlantSway(age,plant,wind):rain?-12:art?age*(seed-.5f)*21+Mathf.Sin(age*.6f+seed*8)*16:0;
                var rotation=Quaternion.Euler(0,0,turn);var point=new Vector3((p.x-.5f)*width,(p.y-.5f)*height,0);
                var pivot=rooted?WorldSeasonalArt.Stem(world):Vector2.one*.5f;
                var tint=art?Color.white:rain?new Color(.77f,.89f,1):snow?new Color(.96f,.99f,1):world==5?new Color(.73f,1,.45f):world==8?new Color(.68f,.85f,1):new Color(1,.77f,.31f);
                tint.a=fade*(art?(world==8?.78f:.92f):rain?.68f:snow?.90f:.62f*(.70f+.30f*Mathf.Sin(age*.8f+i)));
                for(int k=0;k<4;k++){
                    int v=i*4+k;float x=k==1||k==2?1:0,y=k>=2?1:0;
                    vertices[v]=point+rotation*new Vector3((x-pivot.x)*extent.x*(rooted&&right?-1:1),(y-pivot.y)*extent.y,0);
                    uv[v]=art?new Vector2(region.z+x*region.x,region.w+y*region.y):new Vector2(x,y);
                    kinds[v]=new Vector2(art?1:0,snow?1:0);colors[v]=tint;
                }
            }
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetUVs(1,kinds);mesh.SetColors(colors);mesh.bounds=new Bounds(Vector3.zero,new Vector3(width*1.6f,height*1.6f,1));return ActiveCount;
        }
        void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
    }
}
