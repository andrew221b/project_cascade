using UnityEngine;

namespace Cascade.Presentation
{
    // Stage/cosmetic visitors share the same painted rigs, poses and owner clock as world wildlife.
    public sealed class WorldLifeExtraView : MonoBehaviour
    {
        readonly struct Flight
        {
            public readonly float Width,Period,Duration,WingRate,Flutter,BodyFraction;
            public Flight(float width,float period,float duration,float rate,float flutter,float fraction)
            { Width=width;Period=period;Duration=duration;WingRate=rate;Flutter=flutter;BodyFraction=fraction; }
        }
        static readonly Flight[] flights={
            new Flight(.055f,25,9.2f,3.2f,.0015f,.43f),new Flight(.043f,24,7.5f,6.8f,.004f,.60f),
            new Flight(.034f,31,9.6f,3.8f,.004f,.43f),new Flight(.068f,33,13,1.7f,.001f,.46f),
            new Flight(.059f,28,8.8f,3.7f,.002f,.43f),new Flight(.048f,35,12.5f,1.35f,.006f,.29f),
            new Flight(.048f,27,7.9f,4,.0025f,.43f),new Flight(.033f,37,8.7f,3.2f,.004f,.42f),
            new Flight(.054f,39,13.4f,1.15f,.006f,.27f)
        };
        static readonly WildlifeSpecies[] residents={
            WildlifeSpecies.Goldfinch,WildlifeSpecies.Damselfly,WildlifeSpecies.Ladybird,WildlifeSpecies.Tern,
            WildlifeSpecies.Kingfisher,WildlifeSpecies.BookMoth,WildlifeSpecies.WinterWren,WildlifeSpecies.JewelBeetle,WildlifeSpecies.MoonMoth
        };
        static readonly int[] companions={2,1,0,3,1,5,6,7,8};
        readonly WildlifeRig[] visitors=new WildlifeRig[3];
        readonly WildlifeMotion.VisitGate[] visitGates=new WildlifeMotion.VisitGate[3];
        readonly int[] speciesRows={-1,-1,-1};
        readonly int sessionSalt=System.Guid.NewGuid().GetHashCode()&0x3fffffff;
        Camera cameraView;
        WildlifeRigLibrary library;
        bool initialized,ownsLibrary;
        string previousCosmetic;
        int cosmeticFamily=-1;
        int visitContext=int.MinValue;
        public bool Ready=>initialized&&cameraView!=null&&library!=null&&library.Ready;
        public int ActiveCreatureCount{get;private set;}
        public int SpeciesAt(int actor)=>actor>=0&&actor<visitors.Length?speciesRows[actor]:-1;
        public int FrameAt(int actor)=>SpeciesAt(actor)>=0?visitors[actor].Phase:-1;
        public Vector3 BodyPositionAt(int actor)=>SpeciesAt(actor)>=0?visitors[actor].Root.position:Vector3.zero;

        public void Initialize(ToyArt art,Camera camera)=>InitializeShared(art,camera,null);
        internal void InitializeShared(ToyArt art,Camera camera,WildlifeRigLibrary shared)
        {
            cameraView=camera;if(initialized||art==null||art.quad==null||camera==null)return;
            if(ownsLibrary)library?.Dispose();
            ownsLibrary=shared==null;library=shared??new WildlifeRigLibrary(art);
            if(!library.Ready){Debug.LogWarning("CASCADE stage visitors need the layered bird/insect atlases.",this);return;}
            for(int i=0;i<visitors.Length;i++)visitors[i]=new WildlifeRig(transform,"World visitor "+i,library);
            initialized=true;
        }

        public void Paint(float phase,int zone,int stage,bool visible,bool reducedMotion,int quality,bool map,
            string cosmeticId=null,Vector2 flightBand=default,int island=0)
        {
            ActiveCreatureCount=0;
            if(!Ready||!isActiveAndEnabled||!visible||reducedMotion||quality<=0||float.IsNaN(phase)||float.IsInfinity(phase)){
                visitContext=int.MinValue;for(int i=0;i<visitGates.Length;i++)visitGates[i].Reset();Hide();return;
            }
            phase=Mathf.Max(0,phase);zone=Mathf.Clamp(zone,0,8);stage=Mathf.Clamp(stage,0,2047);island=Mathf.Clamp(island,0,4);
            if(previousCosmetic!=cosmeticId){previousCosmetic=cosmeticId;cosmeticFamily=CosmeticFamily(cosmeticId);}
            int context=((((zone*2048+stage)*5+island)*10+cosmeticFamily+1)*2+(map?1:0))*3+Mathf.Clamp(quality,0,2);
            if(context!=visitContext){visitContext=context;for(int i=0;i<visitGates.Length;i++)visitGates[i].Reset();}
            float baseDistance=Mathf.Max(cameraView.nearClipPlane+.5f,Vector3.Dot(transform.position-cameraView.transform.position,cameraView.transform.forward)+2.14f);
            // Non-overlapping slots keep one stage/cosmetic guest airborne at a time.
            // All residents still visit; none pop out just because another rig appears.
            float slotLength=map?36:28;int slots=quality==1?1:visitors.Length;
            float sharedClock=phase+zone*7.31f+stage*2.47f+island*1.91f+WildlifeMotion.Hash(sessionSalt+311)*31;
            for(int i=0;i<visitors.Length;i++) {
                var visitor=visitors[i];visitor.Hide();speciesRows[i]=-1;
                if(quality==1&&i>0)continue;
                int row=SelectSpecies(zone,stage,i,cosmeticFamily,quality);
                var species=residents[row];var flight=flights[row];bool bird=(int)species<8;
                int salt=sessionSalt+zone*173+stage*59+island*97+i*317;
                float period=slotLength*slots,clock=sharedClock-i*slotLength;
                int cycle=Mathf.FloorToInt(clock/period);float seed=WildlifeMotion.Hash(salt+cycle*43);
                float age=Mathf.Repeat(clock,period)-(2+seed*3);
                float duration=flight.Duration*1.42f*Mathf.Lerp(.90f,1.05f,WildlifeMotion.Hash(salt+cycle*71+101));
                if(i==2&&WildlifeMotion.Hash(salt+cycle*29+211)<.22f||!visitGates[i].Admit(cycle,age)||age<0||age>=duration)continue;
                float progress=age/duration;
                float side=WildlifeMotion.Hash(salt+cycle*83+11)<.5f?-1:1;
                bool upperFlight=!map&&quality>1&&seed>(bird?.43f:.32f)&&flightBand.y>flightBand.x+.2f;
                WildlifeMotion.Route(side,seed,upperFlight,map,out var a,out var b,out var c,out var d);
                float depthSeed=WildlifeMotion.Hash(salt+cycle*193+59);
                float depth=WildlifeMotion.Depth(progress,depthSeed)*(i==0?1:i==1?.80f:.65f);
                float distance=baseDistance+(1.6f-depth)*1.5f+i*.24f;
                float height=cameraView.orthographic?cameraView.orthographicSize*2:2*distance*Mathf.Tan(cameraView.fieldOfView*.5f*Mathf.Deg2Rad);
                float size=Mathf.Min(height*cameraView.aspect,height)*flight.Width*depth*(map?.9f:1);
                if(!bird)size*=1.5f;
                if(upperFlight)size*=bird?1.3f:1.2f;
                var tint=zone==8?new Color(.88f,.92f,1):zone==1||zone==6?new Color(.90f,.96f,1):zone==2||zone==7?new Color(1,.94f,.84f):Color.white;
                tint.a=Mathf.SmoothStep(0,1,Mathf.Min(progress,1-progress)/.14f)*(i==0?.86f:i==1?.70f:.62f);
                visitor.Select(species);visitor.Fly(cameraView,a,b,c,d,age,duration,flight.WingRate,seed,flight.Flutter,zone,side,
                    size*flight.BodyFraction,distance,tint,upperFlight?.15f:.10f,upperFlight?flightBand:Vector2.zero,depthSeed);
                speciesRows[i]=row;ActiveCreatureCount++;
            }
        }
        static int SelectSpecies(int zone,int stage,int actor,int family,int quality)
        {
            if(family>=0&&(actor>0||quality==1)) {
                bool alternate=(stage+actor)%2!=0;
                switch(family){
                    case 0:return alternate?2:0;case 1:return alternate?8:7;case 2:return alternate?2:5;
                    case 3:return alternate?0:3;case 4:return alternate?4:1;case 5:return alternate?7:5;
                    case 6:return 6;case 7:return 8;
                }
            }
            if(actor==0)return zone;if(actor==1)return companions[zone];
            if(zone==0||zone==2)return stage%2==0?2:0;
            if(zone==1||zone==4)return stage%2==0?1:4;
            if(zone==3)return stage%2==0?3:1;if(zone==6)return 6;return stage%2==0?7:5;
        }
        static int CosmeticFamily(string id)
        {
            switch(id){
                case "petal_drift":return 0;case "firefly_dance":return 1;case "butterfly_garden":return 2;
                case "swallow_flock":return 3;case "dragonfly_stream":return 4;case "lantern_flight":return 5;
                case "snow_wings":return 6;case "comet_trails":return 7;default:return -1;
            }
        }
        public void Hide(){ActiveCreatureCount=0;for(int i=0;i<visitors.Length;i++){speciesRows[i]=-1;visitors[i]?.Hide();}}
        void OnDisable()=>Hide();
        void OnDestroy(){foreach(var visitor in visitors)visitor?.Dispose();if(ownsLibrary)library?.Dispose();}
    }
}
