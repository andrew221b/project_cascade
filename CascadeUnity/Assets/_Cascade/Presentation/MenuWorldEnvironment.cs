using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed class MenuWorldEnvironment:MonoBehaviour
    {
        CascadeGame game;RectTransform surface;Camera cameraView;Texture2D sky;
        Material skyOnly,scene,island,farIsland,aircraftAtmosphere,locked,seasonalFrame,landscape,headerMaterial;Material[] skyMaterials;
        readonly List<(Image Image,RectTransform Rect,Vector2 Anchor,float Seed)> distantIslands=new List<(Image,RectTransform,Vector2,float)>();
        public int BackgroundIslandCount=>distantIslands.Count;
        readonly List<Material> owned=new List<Material>();readonly List<MenuWorldChimney> chimneys=new List<MenuWorldChimney>();
        readonly Material[] aircraftDepth=new Material[3];
        readonly List<MenuWorldParticles> weather=new List<MenuWorldParticles>();
        MenuSeasonalFlecks seasonalFlecks;WorldFrameResident frameResident;WorldFrameFit frameFit;
        readonly List<(Image Image,RectTransform Rect,float Seed,bool Balloon)> flights=new List<(Image,RectTransform,float,bool)>();
        float clock=16;int world,leadKindOffset;Vector3[] corners=new Vector3[4];
        public int World=>world;public Texture2D SkyTexture=>sky;public float AnimationAge=>clock;
        public int VisibleAircraft=>flights.Count(f=>f.Image.enabled&&f.Image.color.a>.05f);
        public int ChimneyCount=>chimneys.Count;
#if UNITY_EDITOR
        bool review;public void SetReviewAge(float age){review=true;clock=age;Paint();}public void EndReview(){review=false;}
#endif
        Material Make(string shaderName,string label){var shader=Resources.Load<Shader>(MenuWorldArt.Path+shaderName);if(shader==null)throw new InvalidOperationException("Missing world UI shader: "+shaderName);var m=new Material(shader){name=label};owned.Add(m);m.SetFloat("_World",world);return m;}
        public void Initialize(CascadeGame owner,RectTransform root,int selectedWorld)
        {
            game=owner;surface=root;world=selectedWorld;sky=MenuWorldArt.Sky(world);cameraView=Camera.main;
            // The welcome opens on an established ship crossing, with later
            // arrivals still handled by the shared, staggered fleet schedule.
            if(game.CurrentScreen==GameScreen.AccountGate){clock=60;leadKindOffset=3;}
            skyOnly=Make("WorldBackdrop","Themed open sky");skyOnly.SetFloat("_Mode",0);
            skyOnly.SetTexture("_SkyTex",sky);
            scene=Make("WorldSurface","World lit foreground");island=Make("WorldSurface","World lit island scenery");island.SetFloat("_AuthoredIsland",1);
            farIsland=Make("WorldSurface","Atmospheric authored world islands");farIsland.SetFloat("_AuthoredIsland",1);farIsland.SetFloat("_Haze",world==1?.36f:.28f);farIsland.SetTexture("_SkyTex",sky);
            aircraftAtmosphere=Make("WorldSurface","Aircraft in the islands' atmosphere");aircraftAtmosphere.SetFloat("_Haze",world==1?.36f:.28f);aircraftAtmosphere.SetTexture("_SkyTex",sky);
            for(int i=0;i<aircraftDepth.Length;i++){var m=Make("WorldSurface","Aircraft distance "+i);m.SetTexture("_SkyTex",sky);m.SetFloat("_Haze",Mathf.Lerp(world==1?.49f:.44f,world==1?.34f:.26f,i*.5f));aircraftDepth[i]=m;}
            locked=Make("WorldSurface","World lit locked island");locked.SetFloat("_AuthoredIsland",1);locked.SetFloat("_Locked",1);
            landscape=Make("WorldSurface","Seasonal ground and pedestal");landscape.SetFloat("_Landscape",1);
            var shader=Resources.Load<Shader>(WorldFrameArt.Path+"FrameSurface");seasonalFrame=new Material(shader){name="First-world frame reskin "+world};seasonalFrame.SetFloat("_World",world);seasonalFrame.SetFloat("_CleanCutout",game.CurrentScreen==GameScreen.JourneyMap?0:1);seasonalFrame.SetTexture("_ThemeAccentTex",WorldFrameArt.Accents());owned.Add(seasonalFrame);
            seasonalFrame.SetFloat("_MapCornerWind",1);
            // Menu supports continue to the screen edge with transparent space
            // beside them. Floors belong to their own complete artwork/layer.
            seasonalFrame.SetFloat("_ContinuousLanding",game.CurrentScreen==GameScreen.JourneyMap?0:1);
            var posts=Resources.Load<Texture2D>(WorldFrameArt.Path+"MenuSidePostsOct15");
            seasonalFrame.SetTexture("_MenuPostTex",posts);seasonalFrame.SetFloat("_HaveMenuPosts",posts==null?0:1);
            headerMaterial=new Material(shader){name="Original wood heading reskin"};headerMaterial.SetFloat("_World",world);headerMaterial.SetFloat("_Header",1);owned.Add(headerMaterial);
            skyMaterials=new[]{skyOnly,farIsland,aircraftAtmosphere,aircraftDepth[0],aircraftDepth[1],aircraftDepth[2]};
            if(game.CurrentScreen==GameScreen.Settings){
                seasonalFrame.SetFloat("_CleanCutout",0);seasonalFrame.SetFloat("_HaveMenuPosts",0);
                var settingsSky=surface.GetComponentsInChildren<Image>().Single(i=>i.name=="Settings landscape");if(world!=0)settingsSky.material=skyOnly;
                var settingsFrame=surface.GetComponentsInChildren<Image>().Single(i=>i.name=="Settings outer wooden frame");settingsFrame.material=seasonalFrame;
                var settingsWeather=NewRect("World weather behind settings controls",settingsSky.transform.parent,Vector2.zero,Vector2.one);
                settingsWeather.SetSiblingIndex(settingsSky.transform.GetSiblingIndex()+1);
                var particles=settingsWeather.gameObject.AddComponent<MenuWorldParticles>();particles.Initialize(world);weather.Add(particles);Paint();return;
            }
            BindArt();BuildFrame();BuildLife();Paint();
        }
        void BindArt()
        {
            foreach(var image in surface.GetComponentsInChildren<Image>().Where(i=>i.gameObject.activeInHierarchy)){
                string n=image.name;
                if(image.sprite==Resources.Load<Sprite>("CascadeArt/ShopReferenceOct13/HeaderBoard"))image.material=headerMaterial;
                else if(n=="Account and home castle landscape"&&world!=0)image.material=skyOnly;
                else if(n=="Reference home umbrella airship"){image.material=aircraftAtmosphere;image.enabled=false;var motion=image.GetComponent<WorldMotion>();if(motion!=null)motion.enabled=false;}
                else if(n=="Reference shop environment")image.material=skyOnly;
                else if(n=="Reference wooden frame and platform"||n=="Slow existing clouds")image.material=skyOnly;
                else if(n.StartsWith("Reference map island artwork",StringComparison.Ordinal))image.material=image.material!=null&&image.material.shader.name=="Cascade/UI/IslandGrayscale"?locked:island;
                else if(n.StartsWith("Drifting home island",StringComparison.Ordinal)||n.StartsWith("Inventory distant island",StringComparison.Ordinal)){
                    int index=0;int.TryParse(n.Split(' ').Last(),out index);image.sprite=WorldIslandArt.Get(world,index);image.material=farIsland;image.preserveAspect=true;image.color=Color.white;
                }
                else if(n=="Timber and foliage in front of distant islands"){image.enabled=false;}
                else if(n.StartsWith("Reference map upper",StringComparison.Ordinal)||n.StartsWith("Reference map lower",StringComparison.Ordinal)||n.StartsWith("Reference map left daisies",StringComparison.Ordinal)||n.StartsWith("Reference map right daisies",StringComparison.Ordinal)){image.enabled=false;}
                else if(world!=0&&n.StartsWith("Pause daisies",StringComparison.Ordinal)){image.enabled=false;}
                else if(n=="Items pale stone pedestal"||n=="Reference shop Mika stone platform"||n=="Inventory Mika platform"||n=="Coherent stage in front of distant islands"||n=="Inventory coherent wooden floor"||n=="Reference shop collection floor"
                    ||n=="Journal Mika stone ledge"||n=="Journey end Mika landing"||n=="Account Mika stone ledge")image.material=landscape;
                else if(IsScene(n))image.material=scene;
            }
            foreach(var raw in surface.GetComponentsInChildren<RawImage>().Where(i=>i.gameObject.activeInHierarchy)){
                if(raw.name=="Inventory distant moving horizon"||raw.name=="Inventory distant island drift")raw.material=skyOnly;
                else if(raw.name=="Extended boardwalk"||raw.name=="Home continuous reference ground"){raw.enabled=true;raw.material=landscape;}
                else if(raw.name.StartsWith("Extended",StringComparison.Ordinal)||raw.name=="Reference map coherent stone floor"||raw.name=="Inventory coherent wooden floor"||raw.name=="Reference shop collection floor")raw.enabled=false;
                else if(raw.name.Contains("floor")||raw.name.Contains("boardwalk"))raw.material=landscape;
            }
        }
        void BuildFrame()
        {
            if(world!=0){foreach(var graphic in surface.GetComponentsInChildren<Graphic>().Where(g=>g.name=="Pause header daisy"||g.name=="Pause header attached leaf"))graphic.enabled=false;}
            var rect=NewRect("World-specific architectural frame",surface,Vector2.zero,Vector2.one);
            var stage=surface.Cast<Transform>().FirstOrDefault(t=>t.gameObject.activeInHierarchy&&(t.GetComponent<HomeReferenceLayout>()!=null||t.GetComponent<ReferenceShopLayout>()!=null));
            bool map=game.CurrentScreen==GameScreen.JourneyMap;
            if(stage!=null){
                rect.SetParent(stage,false);
                var floor=stage.Find("Continuous menu floor")??stage.Find("Coherent stage in front of distant islands");
                var homeSky=stage.Find("Living home sky");
                if(floor!=null)rect.SetSiblingIndex(floor.GetSiblingIndex()+1);
                else if(homeSky!=null)rect.SetSiblingIndex(homeSky.GetSiblingIndex()+1);
                else rect.SetAsFirstSibling();
                // This complete artwork includes the post feet and corner plants.
                // Cover the frame's older floor strips with one continuous surface.
                stage.GetComponent<HomeReferenceLayout>()?.PlaceGroundAfter(rect);
            }
            else rect.SetAsLastSibling();
            var image=rect.gameObject.AddComponent<Image>();image.sprite=map?WorldFrameArt.MapFrame():WorldFrameArt.MenuFrame();image.raycastTarget=false;image.material=seasonalFrame;image.color=Color.white;
            frameFit=rect.gameObject.AddComponent<WorldFrameFit>();frameFit.Initialize(map?WorldFrameArt.MapLandingViewportFraction:0);if(!WorldSeasonalArt.Rooted(world))rect.gameObject.AddComponent<AttachedPlants>().Initialize(game,world,false);
            var visitor=NewRect("Wildlife on the world frame",rect,Vector2.zero,Vector2.one);
            frameResident=visitor.gameObject.AddComponent<WorldFrameResident>();frameResident.Initialize(world);
        }
        static bool IsScene(string name)=>name=="Mika welcomes you"||name=="Inventory Mika"||name=="Trophy Mika preview"
            ||name=="Magnified actual Mika sprite"||name=="Items pale stone pedestal"||name=="Items selected object"
            ||name=="Mika victory lantern"||name=="Mika reads"
            ||name=="Pause resting Mika and bag"||name=="Pause brass lantern"||name=="Timber and foliage in front of distant islands"
            ||name=="Coherent stage in front of distant islands"||name=="Inventory coherent wooden floor"||name=="Inventory Mika platform"
            ||name=="Reference shop collection floor"||name=="Reference shop Mika stone platform"
            ||name.StartsWith("Reference map upper",StringComparison.Ordinal)||name.StartsWith("Reference map lower",StringComparison.Ordinal)
            ||name.StartsWith("Reference map left daisies",StringComparison.Ordinal)||name.StartsWith("Reference map right daisies",StringComparison.Ordinal)
            ||name.StartsWith("Pause daisies",StringComparison.Ordinal);
        RectTransform NewRect(string name,Transform parent,Vector2 min,Vector2 max)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        void BuildLife()
        {
            var home=surface.GetComponentsInChildren<RectTransform>().FirstOrDefault(r=>r.name=="Living home sky");
            var layout=surface.GetComponentsInChildren<ReferenceShopLayout>().FirstOrDefault();
            RectTransform root=home??(layout==null?surface:(RectTransform)layout.transform);
            var band=NewRect("Thematic sky life",root,home!=null?Vector2.zero:new Vector2(.07f,.13f),home!=null?Vector2.one:new Vector2(.94f,.96f));
            band.SetSiblingIndex(home==null&&layout==null?1:home==null?0:1);band.gameObject.AddComponent<RectMask2D>();
            bool alreadyDrawn=home!=null||game.CurrentScreen==GameScreen.TravelBag||game.CurrentScreen==GameScreen.Parcel;
            if(!alreadyDrawn&&game.CurrentScreen!=GameScreen.JourneyMap&&game.CurrentScreen!=GameScreen.Pause){
                Vector2[] poses={new Vector2(.10f,.72f),new Vector2(.90f,.42f),new Vector2(.10f,.42f),new Vector2(.90f,.30f),new Vector2(.24f,.17f),new Vector2(.78f,.18f)};
                int count=2;
                for(int i=0;i<count;i++){var rect=NewRect("Themed distant island "+i,band,new Vector2(.5f,.5f),new Vector2(.5f,.5f));rect.pivot=new Vector2(.5f,.5f);var picture=rect.gameObject.AddComponent<Image>();picture.sprite=WorldIslandArt.Get(world,i);picture.preserveAspect=true;picture.raycastTarget=false;picture.material=farIsland;distantIslands.Add((picture,rect,poses[i],i*1.83f));}
            }
            var particles=NewRect("World weather and lights",band,Vector2.zero,Vector2.one).gameObject.AddComponent<MenuWorldParticles>();particles.Initialize(world);weather.Add(particles);
            var seasonal=NewRect("Authored seasonal flowers and weather",band,Vector2.zero,Vector2.one);seasonalFlecks=seasonal.gameObject.AddComponent<MenuSeasonalFlecks>();seasonalFlecks.Initialize(world);
            seasonalFlecks.BindSupport(frameFit);
            for(int i=0;i<6;i++){
                var s=Resources.Load<Sprite>(MenuWorldArt.Path+"Aircraft"+i)??Resources.Load<Sprite>("CascadeArt/Journey/Glider");
                if(world==0&&i==3){var reference=surface.GetComponentsInChildren<Image>().FirstOrDefault(p=>p.name=="Reference home umbrella airship");if(reference!=null)s=reference.sprite;}
                var r=NewRect("Distant sky aircraft "+i,band,new Vector2(.5f,.5f),new Vector2(.5f,.5f));r.pivot=new Vector2(.5f,.5f);
                var picture=r.gameObject.AddComponent<Image>();picture.sprite=s;picture.preserveAspect=true;picture.raycastTarget=false;picture.material=aircraftAtmosphere;
                flights.Add((picture,r,i*.618034f,i==0||i==1||i==5));
            }
            foreach(var image in surface.GetComponentsInChildren<Image>().Where(i=>i.name=="Reference map island artwork 0")){
                var button=image.transform.parent.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name=="Island1");if(button==null||!button.interactable||(world!=0&&world!=1&&world!=6&&world!=7))continue;
                var parent=(RectTransform)image.transform.parent;var chimney=WorldIslandArt.Point(world,0,parent.rect.size,WorldIslandArt.Chimney(world));var r=NewRect("Warm cottage chimney smoke",parent,new Vector2(0,1),new Vector2(0,1));r.pivot=new Vector2(.5f,0);r.anchoredPosition=new Vector2(chimney.x,-chimney.y);r.sizeDelta=new Vector2(46,74);var puff=r.gameObject.AddComponent<MenuWorldChimney>();puff.Initialize(world);chimneys.Add(puff);
            }
        }
        void LateUpdate(){
#if UNITY_EDITOR
            if(review)return;
#endif
            if(game==null||!game.CelebrationMotionAllowed)return;
            if(!game.ReducedMotion&&game.Preferences.EffectQuality>0&&!game.MechanicPopupOpen)clock+=Mathf.Min(Time.unscaledDeltaTime,.05f);Paint();
        }
        void Paint()
        {
            if(game==null||surface==null)return;bool moving=!game.ReducedMotion&&game.Preferences.EffectQuality>0;float t=moving?clock:0;
            Color light=MenuWorldArt.Light(world,t,moving);foreach(var m in owned){m.SetColor("_LightTint",light);m.SetFloat("_Age",t);}
            seasonalFrame.SetFloat("_CornerMotionAllowed",moving?1:0);
            surface.GetWorldCorners(corners);var low=RectTransformUtility.WorldToScreenPoint(cameraView,corners[0]);var high=RectTransformUtility.WorldToScreenPoint(cameraView,corners[2]);
            float aspect=(high.x-low.x)/Mathf.Max(1,high.y-low.y),textureAspect=sky==null?aspect:sky.width/(float)sky.height;
            var cover=aspect>textureAspect?new Vector4(1,textureAspect/aspect,0,0):new Vector4(aspect/textureAspect,1,0,0);
            if(world>=3&&world<=7)cover.x*=.74f;
            foreach(var m in skyMaterials){m.SetVector("_SkyScale",cover);}
            foreach(var d in distantIslands){var band=(RectTransform)d.Rect.parent;float size=band.rect.width*(.10f)*(1-(d.Seed%1)*.16f);d.Rect.sizeDelta=new Vector2(size,size);d.Rect.anchoredPosition=Vector2.Scale(d.Anchor-Vector2.one*.5f,band.rect.size)+new Vector2(Mathf.Sin(t*.21f+d.Seed)*2,Mathf.Sin(t*.44f+d.Seed)*3);d.Rect.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*.24f+d.Seed)*.6f);}
            int capacity=game.Preferences.EffectQuality==1?1:2;
            for(int i=0;i<flights.Count;i++){
                var f=flights[i];bool scheduled=SkyFlightMotion.TryFleet(t,i,world,capacity,out var pose,leadKindOffset);
                f.Image.enabled=moving&&scheduled;var band=(RectTransform)f.Rect.parent;float size=band.rect.width*SkyFlightMotion.Width(i,pose);
                f.Rect.sizeDelta=new Vector2(size,size);f.Image.material=aircraftDepth[Mathf.Clamp(Mathf.RoundToInt(pose.Proximity*2),0,2)];
                var position=pose.Position;
                if(band.parent.name=="Living home sky")position.y=.30f+position.y*.43f;
                f.Rect.anchoredPosition=Vector2.Scale(position-Vector2.one*.5f,band.rect.size);f.Rect.localScale=new Vector3(pose.Facing*SkyFlightMotion.SourceFacing(i),1,1);f.Rect.localRotation=Quaternion.Euler(0,0,pose.Roll);
                f.Image.color=new Color(1,1,1,pose.Alpha*Mathf.Lerp(.67f,.92f,pose.Proximity));
            }
            foreach(var w in weather)w.Pose(t,moving?game.Preferences.EffectQuality:0);foreach(var c in chimneys)c.Pose(t,moving?game.Preferences.EffectQuality:0);
            seasonalFlecks?.Pose(t,moving?game.Preferences.EffectQuality:0);frameResident?.Pose(t,moving?game.Preferences.EffectQuality:0);
        }
        void OnDestroy(){foreach(var m in owned)if(m!=null)Destroy(m);}
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuWorldChimney:MaskableGraphic
    {
        int world,quality=2;float age;
        public void Initialize(int biome){world=biome;raycastTarget=false;}
        public void Pose(float t,int level){age=t;quality=level;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(quality==0)return;var r=rectTransform.rect;int count=quality==1?4:8;
            for(int i=0;i<count;i++){float p=Mathf.Repeat(age*.15f+i/(float)count,1);var c=new Vector2(r.center.x+Mathf.Sin(age*.25f+i)*r.width*.08f+p*p*r.width*.25f,r.yMin+p*r.height);float size=3+p*11,alpha=Mathf.Sin(p*Mathf.PI)*.12f;var tint=world==8?new Color(.50f,.62f,.77f,alpha):new Color(.85f,.87f,.86f,alpha);MenuWorldParticles.SoftDot(vh,c,new Vector2(size*.9f,size),tint);}
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuWorldParticles:MaskableGraphic
    {
        int world,quality=2;float age;
        public void Initialize(int biome){world=biome;raycastTarget=false;}
        public void Pose(float t,int level){age=t;quality=level;SetVerticesDirty();}
        public static void SoftDot(VertexHelper vh,Vector2 c,Vector2 radius,Color tint){int n=vh.currentVertCount;vh.AddVert(c,tint,Vector2.zero);var edge=tint;edge.a=0;for(int i=0;i<=12;i++){float a=i*Mathf.PI*2/12;vh.AddVert(c+new Vector2(Mathf.Cos(a)*radius.x,Mathf.Sin(a)*radius.y),edge,Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);}}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(quality==0)return;var r=rectTransform.rect;bool rain=world==1||world==4;int count=quality==1?18:rain?52:world==6?40:world==7?42:30;
            for(int i=0;i<count;i++){
                float u=Mathf.Repeat(i*.618034f+age*(world==6?.005f:rain?.015f:.003f),1),v=Mathf.Repeat(i*.371f-age*(rain?.19f:world==6?.042f:.003f),1);
                float fade=Mathf.SmoothStep(0,1,Mathf.Min(u,1-u)/.06f)*Mathf.SmoothStep(0,1,Mathf.Min(v,1-v)/.06f);var c=r.min+Vector2.Scale(r.size,new Vector2(u,v));
                var tint=rain?new Color(.78f,.90f,1,.56f):world==6?new Color(.94f,.98f,1,.85f):world==5?new Color(.70f,1,.39f,.45f):world==7?new Color(1,.64f,.24f,.62f):world==8?new Color(.82f,.93f,1,.65f):new Color(1,.86f,.44f,.25f);
                tint.a*=fade*(rain?1:.75f+.25f*Mathf.Sin(age*(world==8?1.4f:.7f)+i));float s=world==6?2.8f:rain?1.8f:world==7?3.3f:world==8?1.9f:2.1f;
                SoftDot(vh,c,new Vector2(s,rain?13:s),tint);
                if(world==3&&i%5==0){int n=vh.currentVertCount;var paper=new Color(1,.91f,.65f,fade*.42f);float spin=Mathf.Sin(age*.65f+i)*.6f;Vector2 a=new Vector2(3,spin*3),b=new Vector2(-spin*2,2);vh.AddVert(c-a-b,paper,Vector2.zero);vh.AddVert(c-a+b,paper,Vector2.zero);vh.AddVert(c+a+b,paper,Vector2.zero);vh.AddVert(c+a-b,paper,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
            }
        }
    }
}
