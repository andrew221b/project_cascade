using System.Collections.Generic;
using Cascade.Content;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    // Camera-space scenery only. A shared presentation clock never drives puzzle rules.
    public sealed class LayeredWorld : MonoBehaviour
    {
        readonly List<Material> owned = new List<Material>();
        readonly List<Material> aircraftMaterials=new List<Material>();
        readonly Transform[] extraAircraft=new Transform[6];
        public int ExtraAircraftCount {get{int count=0;foreach(var a in extraAircraft)if(a!=null&&a.gameObject.activeSelf)count++;return count;}}
        readonly Transform[] clouds = new Transform[3];
        readonly Transform[] islands = new Transform[6];
        readonly MeshRenderer[] islandRenderers=new MeshRenderer[6];readonly float[] islandRatios=new float[6];int islandWorld=-1;
        public int IslandSetWorld=>islandWorld;public int IslandVariantCount=>islands.Length;
        Transform landscape, plantsLeft, plantsRight, glider, lagoonSkiff,climateFrame;
        Texture2D workshopTexture,workshopPortrait,mapSkyTexture;bool workshop;
        WorldSeasonalWeather weather;
        Camera cameraView;
        Material landscapeMaterial, gliderMaterial,climateFrameMaterial;
        WorldLifeView wildlife;
        WorldNightSkyView nightSky;
        float gliderAlpha=1,previousPhase;Mesh frameMesh;float frameAspect=-1;
        public int EffectQuality { get; set; }=2;
        public bool ReducedMotion { get; set; }
        public int ActiveWeatherCount { get; private set; }
        readonly Texture2D[] backgrounds = new Texture2D[JourneyStory.WorldCount];
        int zone;
        WorldLighting.Rig lighting;
        public bool WorkshopVisible=>workshop;
        public void BindLighting(WorldLighting.Rig value){lighting=value;lighting?.Paint(zone,workshop,Phase,!ReducedMotion&&EffectQuality>0);}
        float flightBottom, flightTop;
        bool gameplayVisible, holdingToken,quietFlight;
        public float Phase { get; private set; }
        public int Zone => zone;
        public float DescentDepth {get;set;}
        public bool DescentTravelling {get;set;}
        public string CosmeticScenery {get;set;}
        public int CosmeticIsland {get;set;}
        public int CosmeticStage {get;set;}
        public Vector3 CloudPosition => clouds[0]==null?Vector3.zero:clouds[0].position;
        public Vector3 GliderPosition => glider==null?Vector3.zero:glider.position;
        public Vector3 GliderScale => glider==null?Vector3.zero:glider.localScale;
        public Vector3 IslandPosition(int index) => islands[index]==null?Vector3.zero:islands[index].position;
        public Bounds GliderBounds => glider==null?default:glider.GetComponent<MeshRenderer>().bounds;
        public Vector2 FlightBand => new Vector2(flightBottom,flightTop);
        public bool Ready => landscape!=null;
        public float FoliageWind{get;set;}=float.NaN;
        public void SetMapSky(Texture2D texture){mapSkyTexture=texture;SetZone(zone);}
        public void SetWorkshop(bool value){workshop=value;SetZone(zone);}
        public void SetFlightBand(float bottom,float top) { flightBottom=bottom;flightTop=top; }
        public void SetGameplayVisible(bool visible)
        {
            gameplayVisible=visible;
            if(!visible){
                wildlife?.Paint(Phase,zone,false,ReducedMotion,EffectQuality,false);
                nightSky?.Paint(Phase,false,ReducedMotion,0);
                ActiveWeatherCount=weather==null?0:weather.Paint(Phase,zone,0,1,1,Vector3.zero);
                foreach(var cloud in clouds)if(cloud!=null)cloud.gameObject.SetActive(false);
                foreach(var island in islands)if(island!=null)island.gameObject.SetActive(false);
                foreach(var aircraft in extraAircraft)if(aircraft!=null)aircraft.gameObject.SetActive(false);
                if(glider!=null)glider.gameObject.SetActive(false);
                if(lagoonSkiff!=null)lagoonSkiff.gameObject.SetActive(false);
                if(climateFrame!=null)climateFrame.gameObject.SetActive(false);
            }
        }
        public void SetQuietFlight(bool value){quietFlight=value;}
        public void SetTokenHeld(bool held) { holdingToken=held; }

        public void Initialize(ToyArt art,Camera camera)
        {
            cameraView=camera;
            // Load the visible sky only; other worlds keep their full artwork on demand.
            backgrounds[0]=MenuWorldArt.Sky(0);
            if(backgrounds[0]==null)return;
            landscape=RequiredPlate("Far island sky",backgrounds[0],art.quad,false);
            landscapeMaterial=landscape.GetComponent<MeshRenderer>().sharedMaterial;
            var sceneryTemplate=Resources.Load<Material>("CascadeArt/Craft/IslandScenery");
            landscapeMaterial.shader=sceneryTemplate!=null?sceneryTemplate.shader:Shader.Find("CASCADE/IslandScenery");
            climateFrame=RequiredPlate("Thematic level framing",GameplayReferenceArt.Texture("GameplayFrame"),art.quad,true);climateFrameMaterial=climateFrame.GetComponent<MeshRenderer>().sharedMaterial;climateFrameMaterial.shader=Resources.Load<Shader>(WorldFrameArt.Path+"FrameScene");frameMesh=new Mesh{name="Proportional nine-slice world frame"};climateFrame.GetComponent<MeshFilter>().sharedMesh=frameMesh;
            for(int i=0;i<islands.Length;i++){islands[i]=RequiredPlate("Floating world island "+i,WorldIslandArt.Texture(0,i),art.quad,true);islandRenderers[i]=islands[i].GetComponent<MeshRenderer>();islandRenderers[i].sharedMaterial.shader=Resources.Load<Shader>(WorldIslandArt.Path+"IslandSurface");}
            workshopTexture=Resources.Load<Texture2D>("CascadeArt/Craft/WorkshopRoomQuiet");
            workshopPortrait=Resources.Load<Texture2D>("CascadeArt/Craft/WorkshopPortraitQuiet");
            var weatherObject=new GameObject("Batched regional flowers and weather",typeof(WorldSeasonalWeather));weatherObject.transform.SetParent(transform,false);
            weather=weatherObject.GetComponent<WorldSeasonalWeather>();weather.Initialize();
            var cloud=Resources.Load<Texture2D>("CascadeArt/Journey/Cloud");
            for(int i=0;i<clouds.Length;i++)clouds[i]=Plate("Moving cloud "+i,cloud,art.quad,true);
            var plants=Resources.Load<Texture2D>("CascadeArt/Journey/GardenPlants");
            plantsLeft=Plate("Near leaves left",plants,art.quad,true);plantsRight=Plate("Near leaves right",plants,art.quad,true);
            glider=Plate("Travelling glider",Resources.Load<Texture2D>("CascadeArt/Journey/Glider"),art.quad,true);
            lagoonSkiff=Plate("Glass Lagoon sky skiff",Resources.Load<Texture2D>("CascadeArt/Woodland/LagoonSkiff"),art.quad,true);
            if(lagoonSkiff!=null)lagoonSkiff.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_BaseColor",new Color(1,1,1,.84f));
            if(glider!=null)gliderMaterial=glider.GetComponent<MeshRenderer>().sharedMaterial;
            for(int i=0;i<extraAircraft.Length;i++)extraAircraft[i]=Plate("Distant travelling aircraft "+i,Resources.Load<Texture2D>(MenuWorldArt.Path+"Aircraft"+i),art.quad,true);
            PrepareAircraft(glider);PrepareAircraft(lagoonSkiff);foreach(var craft in extraAircraft)PrepareAircraft(craft);
            wildlife=gameObject.AddComponent<WorldLifeView>();wildlife.Initialize(art,cameraView);
            nightSky=gameObject.AddComponent<WorldNightSkyView>();nightSky.Initialize(art,cameraView);
            SetZone(0);Paint(0);
        }
        void PrepareAircraft(Transform craft)
        {
            if(craft==null)return;var material=craft.GetComponent<MeshRenderer>().sharedMaterial;
            material.shader=Resources.Load<Shader>(WorldIslandArt.Path+"IslandSurface");aircraftMaterials.Add(material);
        }
        Transform RequiredPlate(string name,Texture texture,Mesh quad,bool transparent)
        {
            if(texture==null)throw new System.InvalidOperationException("Missing required world texture: "+name);
            return Plate(name,texture,quad,transparent);
        }
        Transform Plate(string name,Texture texture,Mesh quad,bool transparent)
        {
            if(texture==null)return null;
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);
            go.GetComponent<MeshFilter>().sharedMesh=quad;
            // Use the retained final sky shader directly: a temporary Shader.Find-only
            // URP material can lose its shader during player stripping.
            var template=transparent?null:Resources.Load<Material>("CascadeArt/Craft/IslandScenery");
            var shader=transparent?Resources.Load<Shader>("CascadeArt/IllustratedSurface"):template?.shader;
            if(shader==null)throw new System.InvalidOperationException("Missing retained world shader: "+name);
            var mat=new Material(shader){name=name+" material"};owned.Add(mat);
            mat.SetTexture("_BaseMap",texture);mat.SetColor("_BaseColor",Color.white);
            if(transparent) {
                mat.SetFloat("_Relief",name.Contains("cloud")?1.05f:.78f);
                mat.SetFloat("_Emission",name.Contains("observatory")?1:0);
                mat.SetOverrideTag("RenderType","Transparent");mat.renderQueue=3000;
            }
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=mat;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return go.transform;
        }
        public void SetZone(int value)
        {
            zone=Mathf.Clamp(value,0,JourneyStory.WorldCount-1);
            if(backgrounds[zone]==null)backgrounds[zone]=MenuWorldArt.Sky(zone);
            if(climateFrame!=null){var frameMaterial=climateFrame.GetComponent<MeshRenderer>().sharedMaterial;frameMaterial.SetTexture("_BaseMap",WorldFrameArt.Texture(zone));frameMaterial.SetTexture("_ThemeAccentTex",WorldFrameArt.Accents());frameMaterial.SetFloat("_World",zone);frameAspect=-1;}
            if(islandWorld!=zone){for(int i=0;i<islands.Length;i++){var tex=WorldIslandArt.Texture(zone,i);if(tex==null)throw new System.InvalidOperationException("Missing world island texture: "+WorldIslandArt.Theme(zone)+i);islandRenderers[i].sharedMaterial.SetTexture("_BaseMap",tex);islandRatios[i]=tex.width/(float)tex.height;}islandWorld=zone;}
            for(int i=0;i<islands.Length;i++){var mat=islandRenderers[i].sharedMaterial;mat.SetTexture("_SkyTex",backgrounds[zone]);mat.SetFloat("_Haze",zone==1?.25f:.17f);mat.SetColor("_BaseColor",Color.white);}
            foreach(var mat in aircraftMaterials){mat.SetTexture("_SkyTex",backgrounds[zone]);mat.SetFloat("_Haze",zone==1?.25f:.17f);}

            lighting?.Paint(zone,workshop,Phase,!ReducedMotion&&EffectQuality>0);
            if(landscapeMaterial!=null){landscapeMaterial.SetTexture("_BaseMap",mapSkyTexture!=null?mapSkyTexture:workshop&&workshopTexture!=null?workshopTexture:backgrounds[zone]??backgrounds[0]);
                Color[] skies={new Color(.46f,.78f,.9f),new Color(.42f,.65f,.74f),new Color(.88f,.63f,.46f),new Color(.49f,.76f,.85f),new Color(.37f,.70f,.78f),new Color(.49f,.68f,.52f),new Color(.63f,.76f,.86f),new Color(.9f,.71f,.42f),new Color(.045f,.105f,.23f)};
                landscapeMaterial.SetColor("_SkyTop",skies[zone]);landscapeMaterial.SetColor("_SkyBottom",zone==8?new Color(.09f,.20f,.34f):Color.Lerp(skies[zone],new Color(1,.94f,.75f),.72f));}
            if(landscapeMaterial!=null){
                // Authored sky and clouds are atmospheric light, not a solid relief surface.
                landscapeMaterial.SetFloat("_SkyOnly",workshop?0:1);
                landscapeMaterial.SetColor("_MistColor",zone==8?new Color(.18f,.32f,.52f):zone==1||zone==4||zone==6?new Color(.86f,.95f,1):new Color(1,.94f,.83f));
                landscapeMaterial.SetFloat("_MistStrength",mapSkyTexture!=null?0:workshop?.045f:zone==8?.035f:zone==0?.10f:.08f);
            }
            var tint=Color.white; // The shared source supplies illumination, not a second baked tint.
            foreach(var cloud in clouds)if(cloud!=null)cloud.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_BaseColor",tint);
            foreach(var plant in new[]{plantsLeft,plantsRight})if(plant!=null)plant.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_BaseColor",tint);
        }
        public void Paint(float phase)
        {
            if(!Ready)return;float delta=Mathf.Clamp(phase-previousPhase,0,.05f);previousPhase=phase;Phase=phase;
            lighting?.Paint(zone,workshop,phase,!ReducedMotion&&EffectQuality>0);
            float h=cameraView.orthographicSize*2,w=h*cameraView.aspect;
            float cx=cameraView.transform.position.x,cy=cameraView.transform.position.y;
            var workshopMap=cameraView.aspect<.85f&&workshopPortrait!=null?workshopPortrait:workshopTexture;
            if(workshop&&workshopMap!=null&&landscapeMaterial.mainTexture!=workshopMap)landscapeMaterial.SetTexture("_BaseMap",workshopMap);
            landscapeMaterial.SetFloat("_Workshop",workshop?1:0);
            landscapeMaterial.SetFloat("_Night",!workshop&&zone==8?1:0);
            var texture=landscapeMaterial.mainTexture;float ratio=(float)texture.width/texture.height;
            var aircraftLight=MenuWorldArt.Light(zone,phase,!ReducedMotion&&EffectQuality>0);
            landscapeMaterial.SetFloat("_MistPhase",phase);
            landscape.position=new Vector3(cx,cy,3.2f);landscape.localScale=new Vector3(w,h,1);
            // Portrait menus use the sky opening as their content area: move timber outside
            // the safe UI, with a slim sill below it. Map/gameplay retain their own framing.
            float imageHeight=Mathf.Max(1,w/ratio/h),imageWidth=Mathf.Max(1,h*ratio/w);
            var imageRect=workshop&&cameraView.aspect<.85f?new Vector4(-.13f,-.17f,1.26f,1.19f):new Vector4((1-imageWidth)*.5f,(1-imageHeight)*.5f-(gameplayVisible?DescentDepth:0)*.022f,imageWidth,imageHeight);
            if(workshop&&cameraView.aspect>=.85f) {
                // Widen the same opening on landscape; preserve the illustration's proportions.
                float width=imageWidth*1.52f;imageRect=new Vector4((1-width)*.5f,-.47f,width,1.52f);
            }
            landscapeMaterial.SetVector("_ImageRect",imageRect);
            wildlife?.Paint(phase,zone,gameplayVisible&&!workshop&&!quietFlight,ReducedMotion,EffectQuality,mapSkyTexture!=null,CosmeticIsland,CosmeticStage,CosmeticScenery);
            nightSky?.Paint(phase,gameplayVisible&&zone==8&&mapSkyTexture==null&&!workshop,ReducedMotion,EffectQuality);
            float foliageWind=ReducedMotion||EffectQuality==0?0:float.IsNaN(FoliageWind)?Cascade.Application.WorkshopWind.Sample(phase):FoliageWind;
            bool plantSupportVisible=climateFrame!=null&&gameplayVisible&&!workshop&&!quietFlight&&mapSkyTexture==null;
            ActiveWeatherCount=weather==null?0:weather.Paint(ReducedMotion?0:phase,zone,gameplayVisible&&mapSkyTexture==null&&!workshop?EffectQuality:0,w,h,new Vector3(cx,cy,2.35f),foliageWind,plantSupportVisible);
            for(int i=0;i<clouds.Length;i++) {
                var cloud=clouds[i];if(cloud==null)continue;cloud.gameObject.SetActive(gameplayVisible&&mapSkyTexture==null&&!(workshop&&quietFlight));
                float cw=workshop?w*(i==0?.24f:i==1?.20f:.28f):Mathf.Min(w,h)*(i==0?.35f:i==1?.28f:.42f);
                float x=Mathf.Repeat(phase*(.004f+i*.0025f)+i*.42f,1.7f)-.85f;
                if(workshop)x=Mathf.Sin(phase*.035f+i*2)*.18f;
                cloud.position=new Vector3(cx+x*w,cy+h*(workshop?(i==0?.12f:i==1?.24f:-.10f):(i==0?.29f:i==1?.43f:-.27f)+(!workshop?DescentDepth*.032f:0)),2.9f-i*.05f);
                cloud.localScale=new Vector3(cw,cw*.5f,1);
            }
            if(climateFrame!=null){climateFrame.gameObject.SetActive(plantSupportVisible);climateFrame.position=new Vector3(cx,cy,DescentTravelling?-4.5f:2.75f);climateFrame.localScale=new Vector3(w,h,1);FitFrame(w/h);climateFrameMaterial.SetFloat("_Age",ReducedMotion||EffectQuality==0?0:phase);climateFrameMaterial.SetFloat("_Wind",foliageWind);climateFrameMaterial.SetColor("_LightTint",MenuWorldArt.Light(zone,phase,!ReducedMotion&&EffectQuality>0));}
            Plants(plantsLeft,false);Plants(plantsRight,true);
            for(int i=0;i<islands.Length;i++){
                var island=islands[i];if(island==null)continue;island.gameObject.SetActive(gameplayVisible&&(i==2||i==3)&&mapSkyTexture==null&&!workshop&&!quietFlight);
                float seed=i*1.83f,x=i%2==0?-.36f:.36f,y=i<2?.25f:i<4?-.06f:-.30f;
                float size=Mathf.Min(w,h)*(i<2?.23f:i<4?.15f:.17f);
                island.position=new Vector3(cx+w*x+Mathf.Sin(phase*.22f+seed)*w*.012f,cy+h*(y+DescentDepth*.065f)+Mathf.Sin(phase*(.36f+i*.027f)+seed)*h*.012f,3.02f-i*.012f);
                island.localScale=new Vector3(size*islandRatios[i],size,1);island.rotation=Quaternion.Euler(0,0,Mathf.Sin(phase*.24f+seed)*.7f);
                islandRenderers[i].sharedMaterial.SetVector("_ImageRect",imageRect);
            }
            // Legacy glider/skiff artwork participates in the same two-visitor
            // schedule as the other aircraft; it is never an additional layer.
            if(glider!=null)glider.gameObject.SetActive(false);
            if(lagoonSkiff!=null)lagoonSkiff.gameObject.SetActive(false);
            foreach(var craft in extraAircraft)if(craft!=null)craft.gameObject.SetActive(false);
            gliderAlpha=phase==0?1:Mathf.MoveTowards(gliderAlpha,holdingToken?0:1,delta*(holdingToken?8:3));
            for(int i=0;i<extraAircraft.Length;i++){
                float band=flightTop-flightBottom;
                if(!gameplayVisible||band<=0||workshop||mapSkyTexture!=null||quietFlight||ReducedMotion||EffectQuality<=0)continue;
                if(!SkyFlightMotion.TryFleet(phase,i,zone,EffectQuality==1?1:2,out var flight))continue;
                var a=i==2&&zone!=8?glider:i==3&&(zone==3||zone==4||zone==7)?lagoonSkiff:extraAircraft[i];if(a==null)continue;
                a.gameObject.SetActive(true);var mat=a.GetComponent<MeshRenderer>().sharedMaterial;var tex=mat.GetTexture("_BaseMap");
                mat.SetFloat("_Haze",Mathf.Lerp(zone==1?.46f:.40f,zone==1?.25f:.17f,flight.Proximity));
                mat.SetColor("_BaseColor",new Color(aircraftLight.r,aircraftLight.g,aircraftLight.b,flight.Alpha*gliderAlpha*Mathf.Lerp(.68f,.90f,flight.Proximity)));mat.SetVector("_ImageRect",imageRect);
                float width=Mathf.Min(w*SkyFlightMotion.Width(i,flight),band*.32f),height=width*tex.height/tex.width;
                a.position=new Vector3(cx+(flight.Position.x-.5f)*w,flightBottom+band*flight.Position.y,2.95f+(1-flight.Proximity)*.20f);
                a.localScale=new Vector3(width*flight.Facing*(a==glider||a==lagoonSkiff?1:SkyFlightMotion.SourceFacing(i)),height,1);a.rotation=Quaternion.Euler(0,0,flight.Roll);
            }
            void Plants(Transform plant,bool right)
            {
                if(plant==null)return;plant.gameObject.SetActive(false);float size=Mathf.Min(w*.42f,h*.25f);
                var sway=Quaternion.Euler(0,0,Mathf.Sin(phase*.65f+(right?1:0))*1.6f);
                var stem=new Vector3(cx+w*(right?.43f:-.43f),cy-h*.43f-size*.5f,1.9f);
                plant.position=stem+sway*(Vector3.up*size*.5f);
                plant.localScale=new Vector3(size*(right?-1:1),size,1);
                plant.rotation=sway;
            }
        }
        void FitFrame(float aspect)
        {
            if(frameMesh==null||Mathf.Abs(frameAspect-aspect)<.0001f)return;frameAspect=aspect;
            float u=110f/941f,v=180f/1672f;float y=WorldFrameArt.SillViewportHeight(aspect);
            float[] xs={-.5f,-.5f+u,.5f-u,.5f},ys={-.5f,-.5f+y,.5f-y,.5f},us={0,u,1-u,1},vs={0,v,1-v,1};
            var vertices=new Vector3[16];var uv=new Vector2[16];var triangles=new int[54];int n=0;
            for(int j=0;j<4;j++)for(int i=0;i<4;i++){vertices[j*4+i]=new Vector3(xs[i],ys[j],0);uv[j*4+i]=new Vector2(us[i],vs[j]);}
            for(int j=0;j<3;j++)for(int i=0;i<3;i++){int k=j*4+i;triangles[n++]=k;triangles[n++]=k+4;triangles[n++]=k+5;triangles[n++]=k;triangles[n++]=k+5;triangles[n++]=k+1;}
            frameMesh.Clear();frameMesh.vertices=vertices;frameMesh.uv=uv;frameMesh.triangles=triangles;frameMesh.RecalculateBounds();
        }
        void OnDestroy(){foreach(var material in owned)if(material!=null)Destroy(material);if(frameMesh!=null)Destroy(frameMesh);}
    }
}
