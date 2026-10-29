using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public static class WorldSeasonalArt
    {
        public const string Path="CascadeArt/Atmosphere/SeasonalFlecksOct15";
        static Texture2D texture;static readonly Sprite[] sprites=new Sprite[9];
        public static Texture2D Texture=>texture!=null?texture:texture=Resources.Load<Texture2D>(Path);
        public static Sprite Get(int world)
        {
            world=Mathf.Clamp(world,0,8);if(sprites[world]!=null||Texture==null)return sprites[world];
            float w=Texture.width/3f,h=Texture.height/3f;
            return sprites[world]=Sprite.Create(Texture,new Rect(world%3*w+4,(2-world/3)*h+4,w-8,h-8),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
        }
        public static Vector4 Uv(int world){var r=Get(world).rect;return new Vector4(r.width/Texture.width,r.height/Texture.height,r.x/Texture.width,r.y/Texture.height);}
        public static bool Rooted(int world)=>world==0||world==5||world==8;
        public static Vector2 Stem(int world)=>world==0?new Vector2(.41f,.10f):world==5?new Vector2(.24f,.154f):new Vector2(.5f,.45f);
        public static int Count(int world)=>Rooted(world)?Procedural(world)+4:world==1?46:world==4?34:world==6?34:world==7?18:world==3?8:12;
        public static int Procedural(int world)=>world==1?40:world==4?28:world==6?18:world==5?10:world==8||world==7?12:0;
        public static float PlantSway(float age,int plant,float wind=float.NaN)
        {
            if(float.IsNaN(wind))wind=Cascade.Application.WorkshopWind.Sample(age-plant*.11f);
            float flexibility=.88f+.22f*WildlifeMotion.Hash(plant*173+51);
            float breeze=Mathf.Sin(age*1.03f+plant*1.7f)*5.8f+Mathf.Sin(age*.57f+plant*.8f)*2.2f;
            float flutter=Mathf.Sin(age*3.6f+plant*.9f)*Mathf.Abs(wind)*1.5f;
            return Mathf.Clamp((-wind*23+breeze+flutter)*flexibility,-24,24);
        }
        public static Vector2 Drift(float age,int index,int world)
        {
            float seed=WildlifeMotion.Hash(index*197+world*31+17);
            float speed=world==1?.16f:world==6?.042f:world==3||world==5?.017f:.028f;
            // Stratify heights: a sparse group must not all hide behind one button or leave together.
            float y=Mathf.Repeat(index*.381966f+world*.117f-age*speed,1.24f)-.12f;
            if(world==7)y=1-y;
            float x=Mathf.Repeat(seed+age*(world==1?.020f:.009f),1.20f)-.10f;
            x+=Mathf.Sin(age*.63f+seed*9)*(world==1?.004f:.020f)+Mathf.Sin(age*.23f+seed*7)*.014f;
            return new Vector2(x,y);
        }
    }
    // A fixed pool sits in the existing sky layer, behind cards and all hit targets.
    public sealed class MenuSeasonalFlecks:MonoBehaviour
    {
        readonly Image[] flecks=new Image[8];readonly RectTransform[] poses=new RectTransform[8];
        RectTransform root;WorldFrameFit support;Image supportImage;int world;bool mapSupport;
        public int VisibleCount{get;private set;}
        public void BindSupport(WorldFrameFit frame)
        {
            support=frame;supportImage=support==null?null:support.GetComponent<Image>();root=(RectTransform)transform;
            mapSupport=supportImage!=null&&supportImage.sprite==WorldFrameArt.MapFrame();
            if(WorldSeasonalArt.Rooted(world)&&support!=null){root.SetParent(support.transform,false);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;}
        }
        public void Initialize(int biome)
        {
            world=biome;root=(RectTransform)transform;
            for(int i=0;i<flecks.Length;i++){
                // Editor reloads retain the children but not readonly array entries.
                // Rebind the existing pool once instead of allocating a second set.
                string name="Seasonal flower or weather "+i;var child=root.Find(name);
                if(child==null){var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(root,false);child=go.transform;}
                poses[i]=(RectTransform)child;poses[i].anchorMin=poses[i].anchorMax=Vector2.one*.5f;
                poses[i].pivot=WorldSeasonalArt.Rooted(world)?WorldSeasonalArt.Stem(world):Vector2.one*.5f;
                flecks[i]=child.GetComponent<Image>();flecks[i].sprite=WorldSeasonalArt.Get(world);flecks[i].preserveAspect=true;flecks[i].raycastTarget=false;flecks[i].enabled=false;
            }
        }
        public void Pose(float age,int quality)
        {
            if(!isActiveAndEnabled)return;
            if(root==null||flecks[0]==null)Initialize(world);bool rooted=WorldSeasonalArt.Rooted(world);
            bool hasSupport=support!=null&&support.isActiveAndEnabled&&supportImage!=null&&supportImage.isActiveAndEnabled&&supportImage.color.a>.001f;
            VisibleCount=quality<=0||rooted&&!hasSupport?0:rooted?(quality==1?2:4):quality==1?3:world==6?8:6;
            var light=MenuWorldArt.Light(world,age,true);
            for(int i=0;i<flecks.Length;i++){
                var picture=flecks[i];picture.enabled=i<VisibleCount;if(!picture.enabled)continue;
                if(rooted){
                    // Coordinates were sampled on opaque painted leaves in each source frame.
                    bool right=i%2==1;bool map=mapSupport;
                    var point=i<2?(right?(map?new Vector2(.98725f,.95335f):new Vector2(.98611f,.96719f)):(map?new Vector2(.04995f,.9695f):new Vector2(.03194f,.97422f)))
                        :(right?(map?new Vector2(.96387f,.0634f):new Vector2(.95694f,.02891f)):(map?new Vector2(.035f,.035f):new Vector2(.02778f,.03359f)));
                    if(support!=null)point=support.ProjectSourcePoint(point);
                    poses[i].anchoredPosition=Vector2.Scale(point-Vector2.one*.5f,root.rect.size);
                    poses[i].sizeDelta=Vector2.one*root.rect.width*(world==5?.055f:.062f);
                    poses[i].localScale=new Vector3(right?-1:1,1,1);
                    poses[i].localRotation=Quaternion.Euler(0,0,WorldSeasonalArt.PlantSway(age,i));
                    picture.color=light;continue;
                }
                var p=WorldSeasonalArt.Drift(age,i,world);float seed=WildlifeMotion.Hash(i*197+world*31+17);
                poses[i].anchoredPosition=Vector2.Scale(p-Vector2.one*.5f,root.rect.size);
                float size=Mathf.Min(root.rect.width,root.rect.height)*(world==1?.022f:world==6?.026f:.036f+seed*.012f);
                poses[i].sizeDelta=Vector2.one*size;
                poses[i].localRotation=Quaternion.Euler(0,0,world==1?-11:age*(seed-.5f)*21+Mathf.Sin(age*.6f+seed*8)*16);
                poses[i].localScale=new Vector3(world==1||world==6?1:.75f+.25f*Mathf.Cos(age*.8f+seed*7),1,1);
                float edge=Mathf.SmoothStep(0,1,Mathf.Min(p.x,1-p.x)/.055f)*Mathf.SmoothStep(0,1,Mathf.Min(p.y,1-p.y)/.055f);
                picture.color=new Color(1,1,1,edge*(world==8?.66f:.82f));
            }
        }
    }
}
