using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    public enum WildlifeSpecies
    {
        Swallow,Robin,Gull,SnowBunting,Goldfinch,Tern,Kingfisher,WinterWren,
        Dragonfly,Butterfly,LanternBeetle,LunaMoth,Damselfly,Ladybird,BookMoth,JewelBeetle,MoonMoth,
        Bullfinch,BarnOwl,Bumblebee,FieldCricket
    }

    // Shared painted atlases and cached parts. The rig never owns an animation timer.
    internal sealed class WildlifeRigLibrary
    {
        internal readonly Mesh Quad;
        internal readonly Material[] Materials=new Material[6];
        internal readonly Texture2D[] Textures=new Texture2D[6];
        internal static readonly int BaseColor=Shader.PropertyToID("_BaseColor");
        internal static readonly int BaseMapST=Shader.PropertyToID("_BaseMap_ST");
        public bool Ready=>Quad!=null&&Materials[0]!=null&&Materials[1]!=null&&Materials[3]!=null;

        public WildlifeRigLibrary(ToyArt art)
        {
            Quad=art?.quad;var shader=Resources.Load<Shader>("CascadeArt/IllustratedSurface");
            if(Quad==null||shader==null)return;
            for(int i=0;i<6;i++) {
                Textures[i]=Resources.Load<Texture2D>("CascadeArt/Atmosphere/"+(i==0?"BirdRigOct08":i==1?"InsectRigOct08":i==2?"WildlifeRearOct10":i==3?"RegionalRigOct15":i==4?"RegionalRearOct15":"RegionalFlightBodiesOct15"));
                if(Textures[i]==null)continue;
                var mat=new Material(shader){name=i==0?"Layered birds":i==1?"Layered insects":"Painted rear wildlife",renderQueue=3000};
                mat.SetTexture("_BaseMap",Textures[i]);mat.SetColor(BaseColor,Color.white);
                mat.SetFloat("_Relief",.32f);
                mat.SetFloat("_Cutoff",.025f);
                mat.SetOverrideTag("RenderType","Transparent");Materials[i]=mat;
            }
        }
        public void Dispose(){foreach(var material in Materials)if(material!=null)Object.Destroy(material);}
    }

    internal sealed class WildlifeRig
    {
        sealed class Part
        {
            public Transform Joint,Art;
            public MeshRenderer Renderer;
            public readonly MaterialPropertyBlock Tint=new MaterialPropertyBlock();
            public Vector2 Low,High;
            public bool Draw=true;
            public MeshFilter Filter;
            public Mesh Mesh;
            public Vector3[] Rest,Deformed;
            public float BindAngle;
        }
        readonly Part[] parts={new Part(),new Part(),new Part(),new Part()};
        readonly Part[] rearParts={new Part(),new Part(),new Part()};
        readonly Transform frontRoot,rearRoot;
        readonly WildlifeRigLibrary library;
        public readonly Transform Root;
        public bool Visible{get;private set;}
        public float Opacity{get;private set;}
        public bool Resting{get;set;}
        public WildlifeSpecies Species{get;private set;}
        public int Phase{get;private set;}
        public bool Gliding{get;private set;}
        public Vector3 FeetPosition{get;private set;}
        public Vector2 Footprint{get;private set;}
        public float RearAmount{get;private set;}
        bool configured;
        bool flightBody;Vector3 perchBodyPosition,perchBodyScale;Vector4 perchBodyUv;Material perchBodyMaterial;

        public WildlifeRig(Transform owner,string name,WildlifeRigLibrary source,CascadeGame game=null,Camera camera=null)
        {
            library=source;Root=new GameObject(name).transform;Root.SetParent(owner,false);Root.gameObject.layer=owner.gameObject.layer;
            if(game!=null)Root.gameObject.AddComponent<WildlifeAudioEmitter>().Bind(game,this,camera);
            frontRoot=new GameObject("Side view").transform;frontRoot.SetParent(Root,false);frontRoot.gameObject.layer=owner.gameObject.layer;
            rearRoot=new GameObject("Rear view").transform;rearRoot.SetParent(Root,false);rearRoot.gameObject.layer=owner.gameObject.layer;
            for(int i=0;i<parts.Length+rearParts.Length;i++) {
                var part=i<4?parts[i]:rearParts[i-4];part.Joint=new GameObject(i==0||i==4?"Torso":i==1||i==5?"Near wing":i==2||i==6?"Far wing":"Tail or hindwing").transform;
                part.Joint.SetParent(i<4?frontRoot:rearRoot,false);part.Joint.gameObject.layer=owner.gameObject.layer;
                var go=new GameObject("Painted part",typeof(MeshFilter),typeof(MeshRenderer));go.layer=owner.gameObject.layer;
                part.Art=go.transform;part.Art.SetParent(part.Joint,false);
                part.Filter=go.GetComponent<MeshFilter>();part.Filter.sharedMesh=source.Quad;
                part.Renderer=go.GetComponent<MeshRenderer>();part.Renderer.enabled=false;
                part.Renderer.shadowCastingMode=ShadowCastingMode.Off;part.Renderer.receiveShadows=false;
                part.Renderer.lightProbeUsage=LightProbeUsage.Off;part.Renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
                part.Renderer.allowOcclusionWhenDynamic=false;
            }
        }

        public void Select(WildlifeSpecies species)
        {
            if(configured&&species==Species)return;
            Species=species;configured=true;flightBody=false;int index=(int)species;bool regional=index>=(int)WildlifeSpecies.Bullfinch;
            int atlas=regional?3:index<8?0:1,row=regional?index-(int)WildlifeSpecies.Bullfinch:atlas==0?index:index-8;
            var cells=regional?RegionalWildlifeArt.Cells:atlas==0?WildlifeRigAtlas.Birds:WildlifeRigAtlas.Insects;
            var texture=library.Textures[atlas];float torsoLength=cells[row*4].Pixels.width;
            bool bird=WildlifeMotion.IsBird(species),dragon=species==WildlifeSpecies.Dragonfly||species==WildlifeSpecies.Damselfly;
            bool moth=species==WildlifeSpecies.LunaMoth||species==WildlifeSpecies.MoonMoth||species==WildlifeSpecies.BookMoth;
            for(int i=0;i<4;i++) {
                var part=parts[i];int column=!regional&&!bird&&i==2?3:!regional&&!bird&&i==3?2:i;
                part.Draw=!(i==3&&(regional||moth||species==WildlifeSpecies.Butterfly))&&(species!=WildlifeSpecies.FieldCricket||i==0);
                var cell=cells[row*4+column];float pixel=1/torsoLength;
                part.BindAngle=cell.BindAngle;
                if(part.Mesh!=null)Object.Destroy(part.Mesh);
                part.Mesh=species==WildlifeSpecies.FieldCricket&&i==0?WildlifeRigGeometry.CreateFlexibleBody(out part.Rest):WildlifeRigGeometry.Create(cell,out part.Rest);part.Deformed=new Vector3[part.Rest.Length];part.Filter.sharedMesh=part.Mesh;
                if(i>0) {
                    float span=bird?(i==3?(species==WildlifeSpecies.Swallow||species==WildlifeSpecies.Tern?1.08f:.65f):i==1?1.26f:1.09f)
                        :dragon?(i==3?.78f:.94f):moth?(i==3?1.04f:1.26f):1.13f;
                    pixel=span/cell.Pixels.width;
                }
                float z=i==0?0:i==1?-.008f:i==2?.012f:.004f;
                part.Joint.localPosition=bird?(i==3?new Vector3(-.38f,-.17f,z):new Vector3(0,.035f,z))
                    :new Vector3(i==3?-.055f:0,i==3?-.025f:0,z);
                if(i==0)part.Joint.localPosition=Vector3.zero;
                part.Art.localPosition=new Vector3((cell.Pixels.center.x-cell.Pivot.x)*pixel,(cell.Pivot.y-cell.Pixels.center.y)*pixel,0);
                part.Art.localScale=new Vector3(cell.Pixels.width*pixel,cell.Pixels.height*pixel,1);
                part.Low=(Vector2)part.Art.localPosition-(Vector2)part.Art.localScale*.5f;
                part.High=(Vector2)part.Art.localPosition+(Vector2)part.Art.localScale*.5f;
                if(i>0){part.Low.y-=part.Art.localScale.y*.17f;part.High.y+=part.Art.localScale.y*.17f;}
                part.Renderer.sharedMaterial=library.Materials[atlas];
                float insetX=.5f/texture.width,insetY=.5f/texture.height;
                part.Tint.SetVector(WildlifeRigLibrary.BaseMapST,new Vector4(cell.Pixels.width/texture.width-2*insetX,
                    cell.Pixels.height/texture.height-2*insetY,cell.Pixels.x/texture.width+insetX,1-cell.Pixels.yMax/texture.height+insetY));
            }
            var body=parts[0];perchBodyPosition=body.Art.localPosition;perchBodyScale=body.Art.localScale;
            perchBodyMaterial=body.Renderer.sharedMaterial;perchBodyUv=body.Tint.GetVector(WildlifeRigLibrary.BaseMapST);
            ConfigureRear();MeasureFootprint();
        }

        void UseFlightBody(bool flying)
        {
            bool active=flying&&WildlifeFlightArt.Has(Species)&&library.Textures[5]!=null;
            if(active==flightBody)return;flightBody=active;var body=parts[0];
            if(active){
                var texture=library.Textures[5];var rect=WildlifeFlightArt.TexturePixels(Species,texture);
                body.Filter.sharedMesh=library.Quad;body.Renderer.sharedMaterial=library.Materials[5];
                body.Art.localPosition=Vector3.zero;body.Art.localScale=new Vector3(1,rect.height/rect.width,1);
                body.Tint.SetVector(WildlifeRigLibrary.BaseMapST,new Vector4(rect.width/texture.width,rect.height/texture.height,rect.x/texture.width,1-rect.yMax/texture.height));
            }else{
                body.Filter.sharedMesh=body.Mesh;body.Renderer.sharedMaterial=perchBodyMaterial;
                body.Art.localPosition=perchBodyPosition;body.Art.localScale=perchBodyScale;
                body.Tint.SetVector(WildlifeRigLibrary.BaseMapST,perchBodyUv);
            }
        }

        void ConfigureRear()
        {
            int atlas=WildlifeRearArt.IsRegional(Species)?4:2;
            if(library.Textures[atlas]==null||!WildlifeRearArt.Has(Species))return;
            var texture=library.Textures[atlas];var rect=WildlifeRearArt.TexturePixels(Species,texture);
            var body=rearParts[0];body.Filter.sharedMesh=library.Quad;body.Renderer.sharedMaterial=library.Materials[atlas];
            body.Joint.localPosition=new Vector3(0,0,-.018f);body.Art.localPosition=Vector3.zero;
            body.Art.localScale=new Vector3(rect.width/rect.height,1,1);
            body.Tint.SetVector(WildlifeRigLibrary.BaseMapST,new Vector4(rect.width/texture.width,rect.height/texture.height,
                rect.x/texture.width,1-rect.yMax/texture.height));
            for(int i=1;i<3;i++) {
                var p=rearParts[i];var source=parts[i];p.Filter.sharedMesh=source.Mesh;
                p.Renderer.sharedMaterial=source.Renderer.sharedMaterial;
                p.Tint.SetVector(WildlifeRigLibrary.BaseMapST,source.Tint.GetVector(WildlifeRigLibrary.BaseMapST));
                p.Art.localPosition=source.Art.localPosition;p.Art.localScale=source.Art.localScale;p.BindAngle=source.BindAngle;
                p.Joint.localPosition=new Vector3(i==1?-.09f:.09f,.10f,i==1?-.030f:-.025f);
                p.Joint.localScale=new Vector3(i==1?1:-1,1,1);
            }
        }

        void MeasureFootprint()
        {
            float x=0,y=0;
            // Constant worst-pose envelope keeps sizing and the torso stable through every beat.
            for(int i=0;i<4;i++) {
                var p=parts[i];
                if(!p.Draw)continue;
                for(int step=0;step<=16;step++) {
                    float angle=i==0?0:i==3&&WildlifeMotion.IsBird(Species)?Mathf.Lerp(-12,12,step/16f)
                        :partAngle(p,step/16f);
                    var q=Quaternion.Euler(0,0,angle);
                    for(int corner=0;corner<4;corner++) {
                        var v=(Vector2)(p.Joint.localPosition+q*new Vector3((corner&1)==0?p.Low.x:p.High.x,(corner&2)==0?p.Low.y:p.High.y,0));
                        x=Mathf.Max(x,Mathf.Abs(v.x));y=Mathf.Max(y,Mathf.Abs(v.y));
                    }
                }
            }
            // The dorsal silhouette can rotate in the image plane without clipping.
            float rearRadius=WildlifeRearArt.Has(Species)?1.55f:0;
            Footprint=new Vector2(Mathf.Max(x+.025f,rearRadius),Mathf.Max(y+.025f,rearRadius));
            float partAngle(Part p,float t)=>Mathf.Lerp(-80,80,t)+p.BindAngle;
        }

        public void Paint(Vector3 position,Quaternion rotation,float bodySize,float facing,WildlifePose pose,Color tint)
        {
            Resting=false;
            Opacity=tint.a;
            frontRoot.localScale=Vector3.one;RearAmount=0;
            parts[0].Joint.localScale=parts[3].Joint.localScale=Vector3.one;
            foreach(var part in rearParts)part.Renderer.enabled=false;
            Root.position=position;Root.rotation=rotation;Root.localScale=new Vector3(bodySize*facing,bodySize,bodySize);
            parts[1].Joint.localScale=parts[2].Joint.localScale=Vector3.one;
            bool bird=WildlifeMotion.IsBird(Species),dragon=Species==WildlifeSpecies.Dragonfly||Species==WildlifeSpecies.Damselfly;
            parts[3].Joint.localPosition=bird?new Vector3(-.38f,-.17f,.004f):new Vector3(-.055f,-.025f,.004f);
            float fold=dragon?pose.WingFold*13:pose.WingFold*23;
            parts[1].Joint.localRotation=Quaternion.Euler(0,fold,pose.NearWing+parts[1].BindAngle);
            parts[2].Joint.localRotation=Quaternion.Euler(0,-fold*.7f,pose.FarWing+parts[2].BindAngle);
            parts[3].Joint.localRotation=bird?Quaternion.Euler(0,0,pose.Tail)
                :Quaternion.Euler(0,-fold*.6f,pose.NearWing*.78f+6);
            // The torso stays rigid; a slight lift comes from the wing downstroke, not card scaling.
            parts[0].Joint.localRotation=Quaternion.identity;
            if(Species==WildlifeSpecies.FieldCricket){
                var p=parts[0];for(int v=0;v<p.Rest.Length;v++){
                    var point=p.Rest[v];float antenna=Mathf.SmoothStep(0,1,(point.x-.04f)*3)*Mathf.SmoothStep(0,1,(point.y+.05f)*3);
                    point.x+=pose.Tail*.004f*antenna;point.y+=pose.WingFold*.006f*Mathf.Clamp01((point.y+.3f)*2);p.Deformed[v]=point;
                }p.Mesh.SetVertices(p.Deformed);p.Mesh.RecalculateBounds();
            }
            for(int i=0;i<4;i++) {
                if(parts[i].Draw&&i>0)Flex(parts[i],i==1?pose.NearFlex:i==2?pose.FarFlex:bird?pose.Tail*.0015f:pose.NearFlex*.72f);
                parts[i].Tint.SetColor(WildlifeRigLibrary.BaseColor,tint);
                parts[i].Renderer.SetPropertyBlock(parts[i].Tint);parts[i].Renderer.enabled=parts[i].Draw;
            }
            Phase=pose.Phase;Gliding=pose.Gliding;Visible=true;
        }
        public void PaintOnFeet(Vector3 contact,Quaternion rotation,float size,float facing,WildlifePose pose,Color tint,float folded)
        {
            UseFlightBody(false);
            FeetPosition=contact;
            var body=parts[0];
            float bottom=body.Art.localPosition.y+body.Art.localScale.y*body.Mesh.bounds.min.y;
            float footX=Species==WildlifeSpecies.Bullfinch?-.037f:Species==WildlifeSpecies.BarnOwl?.048f:Species==WildlifeSpecies.Bumblebee?.016f:Species==WildlifeSpecies.FieldCricket?-.05f:-.08f;
            var feet=new Vector3(footX*facing,bottom,0)*size;
            Paint(contact-rotation*feet,rotation,size,facing,pose,tint);
            folded=Mathf.Clamp01(folded);
            for(int i=1;i<=2;i++){
                var p=parts[i];
                p.Joint.localScale=new Vector3(Mathf.Lerp(1,.43f,folded),Mathf.Lerp(1,.70f,folded),1);
                float angle=Mathf.Lerp(i==1?pose.NearWing:pose.FarWing,20,folded)+p.BindAngle;
                p.Joint.localRotation=Quaternion.Euler(0,folded*35,angle);
            }
            parts[2].Renderer.enabled=parts[2].Draw&&folded<.86f;
            if(Species==WildlifeSpecies.Bullfinch||Species==WildlifeSpecies.BarnOwl)parts[1].Renderer.enabled=parts[1].Draw&&folded<.92f;
        }
        public void Hide(){Visible=false;foreach(var part in parts)if(part.Renderer!=null)part.Renderer.enabled=false;foreach(var part in rearParts)part.Renderer.enabled=false;}
        public void Fly(Camera camera,Vector3 a,Vector3 b,Vector3 c,Vector3 d,float age,float duration,float rate,
            float seed,float flutter,int zone,float side,float bodySize,float distance,Color tint,float gutter,Vector2 flightBand=default,float depthSeed=-1)
        {
            UseFlightBody(true);
            var style=WildlifeMotion.Style(Species);
            var point=Path(age);var heading=Heading(age);float facing=heading.x>=0?1:-1;
            float turn=WildlifeMotion.Bank(heading,facing);
            float previous=WildlifeMotion.Bank(Heading(age-.18f),facing);
            float steering=Mathf.Abs(Vector3.SignedAngle(Heading(age-.18f),Heading(age+.18f),Vector3.forward));
            var pose=WildlifeMotion.Pose(style,age,rate,seed,turn,previous,Mathf.SmoothStep(0,1,(steering-5)/32));
            float progress=age/duration;
            // Billboards stay camera-facing. Separate painted views supply the turn,
            // rather than rotating every flat mesh 90 degrees into an invisible edge.
            var localRotation=Quaternion.Euler(0,0,turn);
            var right=localRotation*Vector3.right;var up=localRotation*Vector3.up;
            float boxWidth=2*(Mathf.Abs(right.x)*Footprint.x+Mathf.Abs(up.x)*Footprint.y)*bodySize;
            float boxHeight=2*(Mathf.Abs(right.y)*Footprint.x+Mathf.Abs(up.y)*Footprint.y)*bodySize;
            float height=camera.orthographic?camera.orthographicSize*2:2*distance*Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);
            float width=height*camera.aspect;
            float fit=Mathf.Min(1,Mathf.Min(width*gutter/Mathf.Max(.001f,boxWidth),height*.105f/Mathf.Max(.001f,boxHeight)));
            bool upper=flightBand.y>flightBand.x+.2f;
            if(upper)fit=Mathf.Min(fit,(flightBand.y-flightBand.x)*.72f/Mathf.Max(.001f,boxHeight));
            bodySize*=fit;boxWidth*=fit;boxHeight*=fit;
            var body=camera.ViewportToWorldPoint(new Vector3(point.x+.5f,point.y+.5f,distance));
            body+=camera.transform.up*(pose.Lift*bodySize);
            if(upper)body.y=Mathf.Clamp(Mathf.Lerp(flightBand.x,flightBand.y,.5f+point.y*2),flightBand.x+boxHeight*.5f,flightBand.y-boxHeight*.5f);
            // Non-crossing routes already travel inside a generated side corridor.
            // Re-clamping every sample to one edge erased their curves; let the
            // authored path carry the animal fully in and back out of frame.
            Paint(body,camera.transform.rotation*localRotation,bodySize,1,pose,tint);
            float rear=WildlifeMotion.RearView(heading);
            if(depthSeed>=0) {
                float recession=WildlifeMotion.Depth(progress-.035f,depthSeed)-WildlifeMotion.Depth(progress+.035f,depthSeed);
                rear=Mathf.Max(rear,Mathf.SmoothStep(0,1,recession/.06f));
            }
            PaintRear(rear,heading,turn,pose,tint);
            Vector3 Path(float at)=>WildlifeMotion.Path(a,b,c,d,style,Mathf.Clamp(at,0,duration),duration,seed,flutter,zone,side);
            Vector3 Heading(float at)=>Vector3.Scale(WildlifeMotion.Heading(a,b,c,d,style,Mathf.Clamp(at,0,duration),duration,seed,zone,side),new Vector3(camera.aspect,1,1)).normalized;
        }
        void PaintRear(float amount,Vector3 heading,float bank,WildlifePose pose,Color tint)
        {
            if(library.Materials[WildlifeRearArt.IsRegional(Species)?4:2]==null||!WildlifeRearArt.Has(Species)){frontRoot.localScale=new Vector3(heading.x>=0?1:-1,1,1);return;}
            RearAmount=amount;
            float angle=Mathf.Atan2(heading.y,heading.x)*Mathf.Rad2Deg-bank;
            float facing=heading.x>=0?1:-1;
            float profile=angle-(facing<0?180:0);
            rearRoot.localRotation=Quaternion.Euler(0,0,Mathf.LerpAngle(profile,angle-90,amount));
            parts[0].Joint.localScale=new Vector3(facing,1,1);
            parts[0].Joint.localRotation=Quaternion.Euler(0,0,angle-(facing<0?180:0));
            // Keep the separate tail/hindwing in profile; the rear painting already
            // includes the folded tail, so only this appendage fades with the torso.
            var profileRotation=Quaternion.Euler(0,0,profile);
            bool bird=WildlifeMotion.IsBird(Species);
            parts[3].Joint.localPosition=profileRotation*(bird?new Vector3(-.38f*facing,-.17f,.004f):new Vector3(-.055f*facing,-.025f,.004f));
            parts[3].Joint.localScale=new Vector3(facing,1,1);
            parts[3].Joint.localRotation=Quaternion.Euler(0,0,profile+(bird?pose.Tail:pose.NearWing*.78f+6)*facing);
            for(int i=1;i<3;i++){
                var p=rearParts[i];var source=parts[i];float wing=i==1?pose.NearWing:pose.FarWing;
                p.Joint.localPosition=Vector3.Lerp(source.Joint.localPosition,new Vector3(i==1?-.09f:.09f,.10f,i==1?-.030f:-.025f),amount);
                p.Joint.localScale=new Vector3(Mathf.Lerp(facing,i==1?1:-1,amount),1,1);
                float dorsal=(i==1?1:-1)*(wing*.38f+8+p.BindAngle);
                p.Joint.localRotation=Quaternion.Euler(0,pose.WingFold*12,Mathf.LerpAngle(wing+p.BindAngle,dorsal,amount));
            }
            var front=tint;front.a*=1-amount;var rear=tint;rear.a*=amount;
            // A single opaque wing pair follows the heading in both views. Blending
            // two wing assemblies created translucent four-wing ghosts in a turn.
            for(int i=0;i<parts.Length;i++) {
                var p=parts[i];p.Tint.SetColor(WildlifeRigLibrary.BaseColor,front);p.Renderer.SetPropertyBlock(p.Tint);
                p.Renderer.enabled=(i==0||i==3)&&p.Draw&&front.a>.025f;
            }
            for(int i=0;i<rearParts.Length;i++) {
                var p=rearParts[i];var color=i==0?rear:tint;
                p.Tint.SetColor(WildlifeRigLibrary.BaseColor,color);p.Renderer.SetPropertyBlock(p.Tint);p.Renderer.enabled=color.a>.025f&&(i==0||Species!=WildlifeSpecies.FieldCricket);
            }
        }
        static void Flex(Part part,float amount)
        {
            var low=new Vector3(100,100,100);var high=new Vector3(-100,-100,-100);
            for(int i=0;i<part.Rest.Length;i++) {
                var v=part.Rest[i];float tip=Mathf.Clamp01((.38f-v.x)/.82f);v.y+=amount*tip*tip;
                part.Deformed[i]=v;low=Vector3.Min(low,v);high=Vector3.Max(high,v);
            }
            part.Mesh.SetVertices(part.Deformed);part.Mesh.bounds=new Bounds((low+high)*.5f,high-low);
        }
        public void Dispose(){foreach(var part in parts)if(part.Mesh!=null)Object.Destroy(part.Mesh);if(Root!=null)Object.Destroy(Root.gameObject);}
    }
}
