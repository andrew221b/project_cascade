using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    public enum WildlifeSpecies
    {
        Swallow,Robin,Gull,SnowBunting,Goldfinch,Tern,Kingfisher,WinterWren,
        Dragonfly,Butterfly,LanternBeetle,LunaMoth,Damselfly,Ladybird,BookMoth,JewelBeetle,MoonMoth
    }

    // Shared painted atlases and cached parts. The rig never owns an animation timer.
    internal sealed class WildlifeRigLibrary
    {
        internal readonly Mesh Quad;
        internal readonly Material[] Materials=new Material[3];
        internal readonly Texture2D[] Textures=new Texture2D[3];
        internal static readonly int BaseColor=Shader.PropertyToID("_BaseColor");
        internal static readonly int BaseMapST=Shader.PropertyToID("_BaseMap_ST");
        public bool Ready=>Quad!=null&&Materials[0]!=null&&Materials[1]!=null;

        public WildlifeRigLibrary(ToyArt art)
        {
            Quad=art?.quad;var shader=Resources.Load<Shader>("CascadeArt/IllustratedSurface");
            if(Quad==null||shader==null)return;
            for(int i=0;i<3;i++) {
                Textures[i]=Resources.Load<Texture2D>("CascadeArt/Atmosphere/"+(i==0?"BirdRigOct08":i==1?"InsectRigOct08":"WildlifeRearOct10"));
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
        public WildlifeSpecies Species{get;private set;}
        public int Phase{get;private set;}
        public bool Gliding{get;private set;}
        public Vector3 FeetPosition{get;private set;}
        public Vector2 Footprint{get;private set;}
        public float RearAmount{get;private set;}
        // Read-only atlas bounds. Source PNG remains intact, including original generation.
        static readonly Rect[] rearCells={
            new Rect(120,19,131,245),new Rect(409,22,140,242),new Rect(704,23,144,237),new Rect(992,36,139,219),
            new Rect(117,265,134,226),new Rect(387,264,187,236),new Rect(709,270,132,226),new Rect(1002,283,134,217),
            new Rect(141,512,89,233),new Rect(433,516,94,217),new Rect(710,519,131,211),new Rect(1015,500,107,247),
            new Rect(140,752,91,235),new Rect(400,767,157,191),new Rect(712,763,128,210),new Rect(993,761,147,208),
            new Rect(124,987,123,254)
        };
        bool configured;

        public WildlifeRig(Transform owner,string name,WildlifeRigLibrary source)
        {
            library=source;Root=new GameObject(name).transform;Root.SetParent(owner,false);Root.gameObject.layer=owner.gameObject.layer;
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
            Species=species;configured=true;int index=(int)species,atlas=index<8?0:1,row=atlas==0?index:index-8;
            var cells=atlas==0?WildlifeRigAtlas.Birds:WildlifeRigAtlas.Insects;
            var texture=library.Textures[atlas];float torsoLength=cells[row*4].Pixels.width;
            bool bird=atlas==0,dragon=species==WildlifeSpecies.Dragonfly||species==WildlifeSpecies.Damselfly;
            bool moth=species==WildlifeSpecies.LunaMoth||species==WildlifeSpecies.MoonMoth||species==WildlifeSpecies.BookMoth;
            for(int i=0;i<4;i++) {
                var part=parts[i];int column=!bird&&i==2?3:!bird&&i==3?2:i;
                part.Draw=!(i==3&&(moth||species==WildlifeSpecies.Butterfly));
                var cell=cells[row*4+column];float pixel=1/torsoLength;
                part.BindAngle=cell.BindAngle;
                if(part.Mesh!=null)Object.Destroy(part.Mesh);
                part.Mesh=WildlifeRigGeometry.Create(cell,out part.Rest);part.Deformed=new Vector3[part.Rest.Length];part.Filter.sharedMesh=part.Mesh;
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
            ConfigureRear();MeasureFootprint();
        }

        void ConfigureRear()
        {
            if(library.Textures[2]==null)return;
            var rect=rearCells[(int)Species];var texture=library.Textures[2];
            var body=rearParts[0];body.Filter.sharedMesh=library.Quad;body.Renderer.sharedMaterial=library.Materials[2];
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
                    float angle=i==0?0:i==3&&(int)Species<8?Mathf.Lerp(-12,12,step/16f)
                        :partAngle(p,step/16f);
                    var q=Quaternion.Euler(0,0,angle);
                    for(int corner=0;corner<4;corner++) {
                        var v=(Vector2)(p.Joint.localPosition+q*new Vector3((corner&1)==0?p.Low.x:p.High.x,(corner&2)==0?p.Low.y:p.High.y,0));
                        x=Mathf.Max(x,Mathf.Abs(v.x));y=Mathf.Max(y,Mathf.Abs(v.y));
                    }
                }
            }
            // The dorsal silhouette can rotate in the image plane without clipping.
            float rearRadius=library.Textures[2]!=null?1.55f:0;
            Footprint=new Vector2(Mathf.Max(x+.025f,rearRadius),Mathf.Max(y+.025f,rearRadius));
            float partAngle(Part p,float t)=>Mathf.Lerp(-80,80,t)+p.BindAngle;
        }

        public void Paint(Vector3 position,Quaternion rotation,float bodySize,float facing,WildlifePose pose,Color tint)
        {
            frontRoot.localScale=Vector3.one;RearAmount=0;
            parts[0].Joint.localScale=parts[3].Joint.localScale=Vector3.one;
            foreach(var part in rearParts)part.Renderer.enabled=false;
            Root.position=position;Root.rotation=rotation;Root.localScale=new Vector3(bodySize*facing,bodySize,bodySize);
            parts[1].Joint.localScale=parts[2].Joint.localScale=Vector3.one;
            bool bird=(int)Species<8,dragon=Species==WildlifeSpecies.Dragonfly||Species==WildlifeSpecies.Damselfly;
            float fold=dragon?pose.WingFold*13:pose.WingFold*23;
            parts[1].Joint.localRotation=Quaternion.Euler(0,fold,pose.NearWing+parts[1].BindAngle);
            parts[2].Joint.localRotation=Quaternion.Euler(0,-fold*.7f,pose.FarWing+parts[2].BindAngle);
            parts[3].Joint.localRotation=bird?Quaternion.Euler(0,0,pose.Tail)
                :Quaternion.Euler(0,-fold*.6f,pose.NearWing*.78f+6);
            // The torso stays rigid; a slight lift comes from the wing downstroke, not card scaling.
            parts[0].Joint.localRotation=Quaternion.identity;
            for(int i=0;i<4;i++) {
                if(parts[i].Draw&&i>0)Flex(parts[i],i==1?pose.NearFlex:i==2?pose.FarFlex:bird?pose.Tail*.0015f:pose.NearFlex*.72f);
                parts[i].Tint.SetColor(WildlifeRigLibrary.BaseColor,tint);
                parts[i].Renderer.SetPropertyBlock(parts[i].Tint);parts[i].Renderer.enabled=parts[i].Draw;
            }
            Phase=pose.Phase;Gliding=pose.Gliding;Visible=true;
        }
        public void PaintOnFeet(Vector3 contact,Quaternion rotation,float size,float facing,WildlifePose pose,Color tint,float folded)
        {
            FeetPosition=contact;
            var body=parts[0];
            float bottom=body.Art.localPosition.y+body.Art.localScale.y*body.Mesh.bounds.min.y;
            var feet=new Vector3(-.08f*facing,bottom,0)*size;
            Paint(contact-rotation*feet,rotation,size,facing,pose,tint);
            folded=Mathf.Clamp01(folded);
            for(int i=1;i<=2;i++){
                var p=parts[i];
                p.Joint.localScale=new Vector3(Mathf.Lerp(1,.43f,folded),Mathf.Lerp(1,.70f,folded),1);
                float angle=Mathf.Lerp(i==1?pose.NearWing:pose.FarWing,20,folded)+p.BindAngle;
                p.Joint.localRotation=Quaternion.Euler(0,folded*35,angle);
            }
            parts[2].Renderer.enabled=parts[2].Draw&&folded<.86f;
        }
        public void Hide(){Visible=false;foreach(var part in parts)if(part.Renderer!=null)part.Renderer.enabled=false;foreach(var part in rearParts)part.Renderer.enabled=false;}
        public void Fly(Camera camera,Vector3 a,Vector3 b,Vector3 c,Vector3 d,float age,float duration,float rate,
            float seed,float flutter,int zone,float side,float bodySize,float distance,Color tint,float gutter,Vector2 flightBand=default,float depthSeed=-1)
        {
            var style=WildlifeMotion.Style(Species);
            var point=Path(age);var heading=Heading(age);float facing=heading.x>=0?1:-1;
            float turn=WildlifeMotion.Bank(heading,facing);
            float previous=WildlifeMotion.Bank(Heading(age-.18f),facing);
            var pose=WildlifeMotion.Pose(style,age,rate,seed,turn,previous);
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
            Vector3 Heading(float at)=>WildlifeMotion.Heading(a,b,c,d,style,Mathf.Clamp(at,0,duration),duration,seed,zone,side);
        }
        void PaintRear(float amount,Vector3 heading,float bank,WildlifePose pose,Color tint)
        {
            if(library.Materials[2]==null){frontRoot.localScale=new Vector3(heading.x>=0?1:-1,1,1);return;}
            RearAmount=amount;
            float angle=Mathf.Atan2(heading.y,heading.x)*Mathf.Rad2Deg-bank;
            rearRoot.localRotation=Quaternion.Euler(0,0,angle-90);
            float facing=heading.x>=0?1:-1;
            parts[0].Joint.localScale=new Vector3(facing,1,1);
            parts[0].Joint.localRotation=Quaternion.Euler(0,0,angle-(facing<0?180:0));
            rearParts[1].Joint.localRotation=Quaternion.Euler(0,pose.WingFold*12,pose.NearWing*.38f+8+rearParts[1].BindAngle);
            rearParts[2].Joint.localRotation=Quaternion.Euler(0,-pose.WingFold*12,-(pose.FarWing*.38f+8+rearParts[2].BindAngle));
            var front=tint;front.a*=1-amount;var rear=tint;rear.a*=amount;
            // A single opaque wing pair follows the heading in both views. Blending
            // two wing assemblies created translucent four-wing ghosts in a turn.
            for(int i=0;i<parts.Length;i++) {
                var p=parts[i];p.Tint.SetColor(WildlifeRigLibrary.BaseColor,front);p.Renderer.SetPropertyBlock(p.Tint);
                p.Renderer.enabled=i==0&&front.a>.025f;
            }
            for(int i=0;i<rearParts.Length;i++) {
                var p=rearParts[i];var color=i==0?rear:tint;
                p.Tint.SetColor(WildlifeRigLibrary.BaseColor,color);p.Renderer.SetPropertyBlock(p.Tint);p.Renderer.enabled=color.a>.025f;
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
