using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    // Camera-space scenery only. A shared presentation clock never drives puzzle rules.
    public sealed class LayeredWorld : MonoBehaviour
    {
        readonly List<Material> owned = new List<Material>();
        readonly Transform[] clouds = new Transform[3];
        readonly Transform[] islands = new Transform[4];
        Transform landscape, plantsLeft, plantsRight, glider, lagoonSkiff;
        Texture2D panorama,workshopTexture,workshopPortrait,mapSkyTexture;bool workshop;
        readonly MeshRenderer[] weatherRenderers=new MeshRenderer[18];
        Camera cameraView;
        Material landscapeMaterial, weatherMaterial, gliderMaterial;
        MaterialPropertyBlock weatherTint;
        float gliderAlpha=1,previousPhase;
        public int EffectQuality { get; set; }=2;
        public int ActiveWeatherCount { get; private set; }
        readonly Texture2D[] backgrounds = new Texture2D[8];
        int zone;
        readonly Transform[] weather=new Transform[18];
        float flightBottom, flightTop;
        bool gameplayVisible, holdingToken,quietFlight;
        public float Phase { get; private set; }
        public int Zone => zone;
        public float DescentDepth {get;set;}
        public Vector3 CloudPosition => clouds[0]==null?Vector3.zero:clouds[0].position;
        public Vector3 GliderPosition => glider==null?Vector3.zero:glider.position;
        public Vector3 GliderScale => glider==null?Vector3.zero:glider.localScale;
        public Vector3 IslandPosition(int index) => islands[index]==null?Vector3.zero:islands[index].position;
        public Bounds GliderBounds => glider==null?default:glider.GetComponent<MeshRenderer>().bounds;
        public Vector2 FlightBand => new Vector2(flightBottom,flightTop);
        public bool Ready => landscape!=null;
        public void SetMapSky(Texture2D texture){mapSkyTexture=texture;SetZone(zone);}
        public void SetWorkshop(bool value){workshop=value;SetZone(zone);}
        public void SetFlightBand(float bottom,float top) { flightBottom=bottom;flightTop=top; }
        public void SetGameplayVisible(bool visible) { gameplayVisible=visible; }
        public void SetQuietFlight(bool value){quietFlight=value;}
        public void SetTokenHeld(bool held) { holdingToken=held; }

        public void Initialize(ToyArt art,Camera camera)
        {
            cameraView=camera;weatherTint=new MaterialPropertyBlock();
            string[] names={"WindGardenBase","RainMillBase","CopperOrchardBase","CloudPostBase","GlassLagoonBase","MossArchiveBase","SnowSpireBase","GreatBeaconBase"};
            for(int i=0;i<names.Length;i++)backgrounds[i]=Resources.Load<Texture2D>("CascadeArt/"+(i<3?"Journey/":"Woodland/")+names[i]);
            if(backgrounds[0]==null)return;
            landscape=Plate("Far island sky",backgrounds[0],art.quad,false);
            landscapeMaterial=landscape.GetComponent<MeshRenderer>().sharedMaterial;
            var sceneryTemplate=Resources.Load<Material>("CascadeArt/Craft/IslandScenery");
            landscapeMaterial.shader=sceneryTemplate!=null?sceneryTemplate.shader:Shader.Find("CASCADE/IslandScenery");
            panorama=Resources.Load<Texture2D>("CascadeArt/Craft/WindGardenSky") ?? Resources.Load<Texture2D>("CascadeArt/Craft/WindGardenPanorama");
            var islandAtlas=Resources.Load<Texture2D>("CascadeArt/Craft/FloatingGardenIslands");
            for(int i=0;i<islands.Length;i++) {
                islands[i]=Plate("Floating garden island "+i,islandAtlas,art.quad,true);
                if(islands[i]==null)continue;
                var material=islands[i].GetComponent<MeshRenderer>().sharedMaterial;
                material.SetTextureScale("_BaseMap",new Vector2(i==2?.58f:i==3?.42f:.5f,.5f));
                material.SetTextureOffset("_BaseMap",new Vector2(i==3?.58f:(i%2)*.5f,i<2?.5f:0));
                material.SetColor("_BaseColor",new Color(1,1,1,i<2?.82f:.68f));
            }
            workshopTexture=Resources.Load<Texture2D>("CascadeArt/Craft/WorkshopRoomQuiet")??Resources.Load<Texture2D>("CascadeArt/Craft/WorkshopRoom");
            workshopPortrait=Resources.Load<Texture2D>("CascadeArt/Craft/WorkshopPortraitQuiet")??Resources.Load<Texture2D>("CascadeArt/Craft/WorkshopPortrait");
            weatherMaterial=new Material(Resources.Load<Shader>("CascadeArt/SoftWeather")){name="Soft distant weather"};owned.Add(weatherMaterial);
            for(int i=0;i<weather.Length;i++) {
                var go=new GameObject("Region weather "+i,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);
                go.GetComponent<MeshFilter>().sharedMesh=art.quad;go.GetComponent<MeshRenderer>().sharedMaterial=weatherMaterial;
                weatherRenderers[i]=go.GetComponent<MeshRenderer>();weather[i]=go.transform;go.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
            }
            var cloud=Resources.Load<Texture2D>("CascadeArt/Journey/Cloud");
            for(int i=0;i<clouds.Length;i++)clouds[i]=Plate("Moving cloud "+i,cloud,art.quad,true);
            var plants=Resources.Load<Texture2D>("CascadeArt/Journey/GardenPlants");
            plantsLeft=Plate("Near leaves left",plants,art.quad,true);plantsRight=Plate("Near leaves right",plants,art.quad,true);
            glider=Plate("Travelling glider",Resources.Load<Texture2D>("CascadeArt/Journey/Glider"),art.quad,true);
            lagoonSkiff=Plate("Glass Lagoon sky skiff",Resources.Load<Texture2D>("CascadeArt/Woodland/LagoonSkiff"),art.quad,true);
            if(lagoonSkiff!=null)lagoonSkiff.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_BaseColor",new Color(1,1,1,.84f));
            if(glider!=null)gliderMaterial=glider.GetComponent<MeshRenderer>().sharedMaterial;
            SetZone(0);Paint(0);
        }
        Transform Plate(string name,Texture texture,Mesh quad,bool transparent)
        {
            if(texture==null)return null;
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);
            go.GetComponent<MeshFilter>().sharedMesh=quad;
            var mat=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name=name+" material"};owned.Add(mat);
            mat.SetTexture("_BaseMap",texture);mat.SetColor("_BaseColor",Color.white);
            if(transparent) {
                mat.SetFloat("_Surface",1);mat.SetFloat("_Blend",0);mat.SetFloat("_ZWrite",0);
                mat.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);mat.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_Cull",(float)CullMode.Off);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.SetOverrideTag("RenderType","Transparent");mat.renderQueue=3000;
            }
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=mat;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return go.transform;
        }
        public void SetZone(int value)
        {
            zone=Mathf.Clamp(value,0,7);
            if(landscapeMaterial!=null){landscapeMaterial.SetTexture("_BaseMap",mapSkyTexture!=null?mapSkyTexture:workshop&&workshopTexture!=null?workshopTexture:zone==0&&panorama!=null?panorama:backgrounds[zone]??backgrounds[0]);
                Color[] skies={new Color(.46f,.78f,.9f),new Color(.42f,.65f,.74f),new Color(.88f,.63f,.46f),new Color(.49f,.76f,.85f),new Color(.37f,.70f,.78f),new Color(.49f,.68f,.52f),new Color(.63f,.76f,.86f),new Color(.9f,.71f,.42f)};
                landscapeMaterial.SetColor("_SkyTop",skies[zone]);landscapeMaterial.SetColor("_SkyBottom",Color.Lerp(skies[zone],new Color(1,.94f,.75f),.72f));}
            if(landscapeMaterial!=null){
                landscapeMaterial.SetColor("_MistColor",zone==1||zone==4||zone==6?new Color(.86f,.95f,1):new Color(1,.94f,.83f));
                landscapeMaterial.SetFloat("_MistStrength",mapSkyTexture!=null?0:workshop?.045f:zone==0?.10f:.08f);
            }
            var tint=zone==1||zone==4||zone==6?new Color(.83f,.94f,1):zone==2||zone==7?new Color(1,.88f,.74f):Color.white;
            foreach(var cloud in clouds)if(cloud!=null)cloud.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_BaseColor",tint);
            foreach(var plant in new[]{plantsLeft,plantsRight})if(plant!=null)plant.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_BaseColor",tint);
        }
        public void Paint(float phase)
        {
            if(!Ready)return;float delta=Mathf.Clamp(phase-previousPhase,0,.05f);previousPhase=phase;Phase=phase;
            float h=cameraView.orthographicSize*2,w=h*cameraView.aspect;
            float cx=cameraView.transform.position.x,cy=cameraView.transform.position.y;
            var workshopMap=cameraView.aspect<.85f&&workshopPortrait!=null?workshopPortrait:workshopTexture;
            if(workshop&&workshopMap!=null&&landscapeMaterial.mainTexture!=workshopMap)landscapeMaterial.SetTexture("_BaseMap",workshopMap);
            landscapeMaterial.SetFloat("_Workshop",workshop?1:0);
            var texture=landscapeMaterial.mainTexture;float ratio=(float)texture.width/texture.height;
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
            ActiveWeatherCount=0;
            for(int i=0;i<weather.Length;i++) {
                var drop=weather[i];bool snow=zone==6;bool rain=zone==1||zone==4;bool pollen=zone==0||zone==2||zone==5;
                if(drop==null)continue; // Editor hot reload can invalidate transient weather objects before the scene restarts.
                bool active=mapSkyTexture==null&&!workshop&&EffectQuality>0&&i<(EffectQuality==1?8:18)&&(snow||rain||pollen);
                drop.gameObject.SetActive(active);if(!active)continue;ActiveWeatherCount++;
                float x=Mathf.Repeat(i*.618f+phase*(snow?.004f:rain?.012f:.003f),1);
                float y=Mathf.Repeat(i*.317f-phase*(snow?.018f:rain?.09f:.004f),1);
                drop.position=new Vector3(cx+(x-.5f)*w+Mathf.Sin(phase*.35f+i)*.06f,cy+(y-.5f)*h,2.3f);
                drop.localScale=snow?new Vector3(.048f,.048f,.01f):rain?new Vector3(.021f,.16f,.01f):new Vector3(.043f,.043f,.01f);
                drop.rotation=Quaternion.Euler(0,0,rain?-15:phase*(i%2==0?4:-3));
                float fade=Mathf.SmoothStep(0,1,Mathf.Min(x,1-x)/.12f)*Mathf.SmoothStep(0,1,Mathf.Min(y,1-y)/.12f);
                Color tint=rain?new Color(.67f,.84f,.91f,.34f):snow?new Color(1,.98f,.90f,.62f):new Color(1,.89f,.64f,.38f);
                tint.a*=fade;weatherTint.SetColor("_Color",tint);weatherRenderers[i].SetPropertyBlock(weatherTint);
            }
            for(int i=0;i<clouds.Length;i++) {
                var cloud=clouds[i];if(cloud==null)continue;cloud.gameObject.SetActive(mapSkyTexture==null);
                float cw=workshop?w*(i==0?.24f:i==1?.20f:.28f):Mathf.Min(w,h)*(i==0?.35f:i==1?.28f:.42f);
                float x=Mathf.Repeat(phase*(.004f+i*.0025f)+i*.42f,1.7f)-.85f;
                if(workshop)x=Mathf.Sin(phase*.035f+i*2)*.18f;
                cloud.position=new Vector3(cx+x*w,cy+h*(workshop?(i==0?.12f:i==1?.24f:-.10f):(i==0?.29f:i==1?.43f:-.27f)),2.9f-i*.05f);
                cloud.localScale=new Vector3(cw,cw*.5f,1);
            }
            Plants(plantsLeft,false);Plants(plantsRight,true);
            for(int i=0;i<islands.Length;i++) {
                var island=islands[i];if(island==null)continue;
                island.gameObject.SetActive(zone==0&&mapSkyTexture==null&&!workshop&&!quietFlight);
                float seed=i*1.83f;
                float x=i==0?-.28f:i==1?.28f:i==2?-.22f:.22f;
                float y=i==0?.23f:i==1?.16f:i==2?-.14f:-.28f;
                island.position=new Vector3(cx+w*x+Mathf.Sin(phase*.22f+seed)*w*.018f,
                    cy+h*y+Mathf.Sin(phase*(.46f+i*.047f)+seed)*h*.017f,3.02f-i*.015f);
                float size=Mathf.Min(w,h)*(i==0?.23f:i==1?.25f:i==2?.27f:.20f);
                island.localScale=new Vector3(size*(i==2?1.16f:i==3?.84f:1),size,1);
                island.rotation=Quaternion.Euler(0,0,Mathf.Sin(phase*.3f+seed)*1.2f);
            }
            if(lagoonSkiff!=null){
                lagoonSkiff.gameObject.SetActive((zone==3||zone==4||zone==7)&&mapSkyTexture==null&&!workshop&&gameplayVisible);
                float width=Mathf.Min(w*.29f,h*.17f);
                lagoonSkiff.position=new Vector3(cx+w*(.24f+Mathf.Sin(phase*.18f)*.11f),cy+h*.10f+Mathf.Sin(phase*.65f)*h*.009f,2.7f);
                lagoonSkiff.localScale=new Vector3(width,width*.9f,1);
                lagoonSkiff.rotation=Quaternion.Euler(0,0,Mathf.Sin(phase*.48f)*1.2f);
            }
            if(glider!=null) {
                float angle=phase*.08f;
                float band=flightTop-flightBottom;
                glider.gameObject.SetActive(mapSkyTexture==null&&!quietFlight&&!workshop&&(!gameplayVisible||band>0));
                gliderAlpha=phase==0?1:Mathf.MoveTowards(gliderAlpha,holdingToken?0:1,delta*(holdingToken?8:3));
                gliderMaterial.SetColor("_BaseColor",new Color(1,1,1,gliderAlpha));
                float depth=.86f+.24f*Mathf.Cos(angle);
                float height=(gameplayVisible?Mathf.Min(h*.105f,band*.55f):Mathf.Min(w,8)*.18f)*depth;
                float y=gameplayVisible?(flightBottom+flightTop)*.5f+Mathf.Cos(angle)*band*.065f:cy+h*(.255f+Mathf.Cos(angle)*.035f);
                glider.position=new Vector3(cx+Mathf.Sin(angle)*w*.33f,y,2.45f+.10f*(1-Mathf.Cos(angle)));
                // Continuous yaw, curved ascent and perspective scale describe one loop in depth.
                glider.localScale=new Vector3(height*1.5f,height,1);
                glider.rotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,-Mathf.Sin(angle)*11);
            }
            void Plants(Transform plant,bool right)
            {
                if(plant==null)return;plant.gameObject.SetActive(mapSkyTexture==null&&!workshop);float size=Mathf.Min(w*.42f,h*.25f);
                var sway=Quaternion.Euler(0,0,Mathf.Sin(phase*.65f+(right?1:0))*1.6f);
                var stem=new Vector3(cx+w*(right?.43f:-.43f),cy-h*.43f-size*.5f,1.9f);
                plant.position=stem+sway*(Vector3.up*size*.5f);
                plant.localScale=new Vector3(size*(right?-1:1),size,1);
                plant.rotation=sway;
            }
        }
        void OnDestroy(){foreach(var material in owned)if(material!=null)Destroy(material);}
    }
}
