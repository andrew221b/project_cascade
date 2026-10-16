using UnityEngine;

namespace Cascade.Presentation
{
    // Decorative wildlife reads the shared ambient clock and owns no input or puzzle state.
    public sealed class WorldLifeView : MonoBehaviour
    {
        readonly struct Flight
        {
            public readonly float Width,Period,Duration,WingRate,Flutter;
            public Flight(float width,float period,float duration,float rate,float flutter)
            { Width=width;Period=period;Duration=duration;WingRate=rate;Flutter=flutter; }
        }
        static readonly Flight[] flights={
            new Flight(.075f,29,7.6f,4.0f,.002f),new Flight(.065f,37,8.4f,4.7f,.004f),
            new Flight(.09f,43,12.2f,1.6f,.001f),new Flight(.062f,39,7.2f,4.2f,.003f),
            new Flight(.060f,31,8.8f,5.1f,.003f),new Flight(.085f,41,11.2f,2.1f,.001f),
            new Flight(.064f,34,7.4f,4.6f,.003f),new Flight(.058f,36,8.5f,4.8f,.003f),
            new Flight(.065f,26,8.8f,7.8f,.006f),new Flight(.058f,30,11.2f,1.45f,.010f),
            new Flight(.045f,32,9.4f,3.4f,.006f),new Flight(.075f,35,13.2f,.9f,.005f)
        };
        static readonly WildlifeSpecies[] worldLife={
            WildlifeSpecies.Swallow,WildlifeSpecies.Goldfinch,WildlifeSpecies.Butterfly,WildlifeSpecies.Bumblebee,
            WildlifeSpecies.Swallow,WildlifeSpecies.Robin,WildlifeSpecies.Dragonfly,WildlifeSpecies.LanternBeetle,
            WildlifeSpecies.Robin,WildlifeSpecies.Goldfinch,WildlifeSpecies.Butterfly,WildlifeSpecies.Bumblebee,
            WildlifeSpecies.Gull,WildlifeSpecies.Tern,WildlifeSpecies.Dragonfly,WildlifeSpecies.Butterfly,
            WildlifeSpecies.Gull,WildlifeSpecies.Kingfisher,WildlifeSpecies.Dragonfly,WildlifeSpecies.Butterfly,
            WildlifeSpecies.Robin,WildlifeSpecies.WinterWren,WildlifeSpecies.LanternBeetle,WildlifeSpecies.LunaMoth,
            WildlifeSpecies.Bullfinch,WildlifeSpecies.SnowBunting,WildlifeSpecies.WinterWren,WildlifeSpecies.SnowBunting,
            WildlifeSpecies.Gull,WildlifeSpecies.Tern,WildlifeSpecies.LanternBeetle,WildlifeSpecies.LunaMoth,
            WildlifeSpecies.BarnOwl,WildlifeSpecies.WinterWren,WildlifeSpecies.MoonMoth,WildlifeSpecies.LanternBeetle
        };
        static Flight FlightFor(WildlifeSpecies species)=>species==WildlifeSpecies.Bullfinch?new Flight(.078f,34,10,4.3f,.002f):
            species==WildlifeSpecies.BarnOwl?new Flight(.12f,48,15,1.3f,.001f):
            species==WildlifeSpecies.Bumblebee?new Flight(.055f,29,10.2f,10.8f,.008f):
            (int)species<flights.Length?flights[(int)species]:new Flight(.06f,35,12,1.4f,.007f);
        readonly WildlifeRig[] creatures=new WildlifeRig[4];
        readonly WildlifeMotion.VisitGate[] visitGates=new WildlifeMotion.VisitGate[4];
        readonly int sessionSalt=System.Guid.NewGuid().GetHashCode()&0x3fffffff;
        int visitContext=int.MinValue;
        float contextStarted;
        public static WildlifeSpecies BirdSpecies(int zone,bool second)=>worldLife[Mathf.Clamp(zone,0,8)*4+(second?1:0)];
        WildlifeRigLibrary library;
        Camera cameraView;
        LayeredWorld layeredWorld;
        ToyMachineView source;
        WorldLifeExtraView extraVisitors;
        WildlifePerch perch;
        int perchZone=-1;
        bool initialized;
        public bool Ready=>initialized&&cameraView!=null&&library!=null&&library.Ready;
        public int ActiveCreatureCount{get;private set;}
        public int ExtraCreatureCount=>extraVisitors!=null?extraVisitors.ActiveCreatureCount:0;
        public WildlifePerchState BirdState=>perch?.State??WildlifePerchState.None;
        public WildlifePerchKind? BirdSupportKind=>perch?.Kind;
        public Vector3 BirdFeet=>creatures[0]?.FeetPosition??Vector3.zero;
        public WildlifeSpecies PerchedSpecies=>creatures[0]?.Species??WildlifeSpecies.Swallow;

        public void Initialize(ToyArt art,Camera camera)
        {
            cameraView=camera;layeredWorld=GetComponent<LayeredWorld>();source=GetComponent<ToyMachineView>();
            if(initialized){extraVisitors?.InitializeShared(art,camera,library);perch=new WildlifePerch(GetComponent<ToyMachineView>(),camera,creatures[0],sessionSalt);return;}
            if(art==null||art.quad==null||camera==null)return;
            library?.Dispose();library=new WildlifeRigLibrary(art);
            if(!library.Ready){Debug.LogWarning("CASCADE layered wildlife needs both rig atlases and the existing Unlit shader.",this);return;}
            var owner=GetComponent<CascadeGame>();
            for(int i=0;i<creatures.Length;i++)creatures[i]=new WildlifeRig(transform,i==0?"Near wildlife":i==1?"Far wildlife":"Small wildlife "+i,library,owner,camera);
            perch=new WildlifePerch(GetComponent<ToyMachineView>(),camera,creatures[0],sessionSalt);
            var extra=new GameObject("Stage wildlife",typeof(WorldLifeExtraView));extra.layer=gameObject.layer;extra.transform.SetParent(transform,false);
            extraVisitors=extra.GetComponent<WorldLifeExtraView>();extraVisitors.InitializeShared(art,camera,library);initialized=true;
        }

        public void Paint(float phase,int zone,bool visible,bool reducedMotion,int quality,bool map,int island=0,int stage=0,string cosmeticId=null)
        {
            ActiveCreatureCount=0;
            if(!Ready||!isActiveAndEnabled||!visible||reducedMotion||quality<=0||float.IsNaN(phase)||float.IsInfinity(phase)){
                visitContext=int.MinValue;for(int i=0;i<visitGates.Length;i++)visitGates[i].Reset();
                perch?.Cancel(float.IsNaN(phase)||float.IsInfinity(phase)?0:phase);Hide();return;
            }
            phase=Mathf.Max(0,phase);zone=Mathf.Clamp(zone,0,8);island=Mathf.Clamp(island,0,4);
            if(map||perchZone!=zone){perch?.Cancel(phase);perchZone=zone;}
            int context=((zone*2048+Mathf.Clamp(stage,0,2047))*5+island)*8+(map?4:0)+Mathf.Clamp(quality,0,3);
            if(context!=visitContext||phase<contextStarted){visitContext=context;contextStarted=phase;for(int i=0;i<visitGates.Length;i++)visitGates[i].Reset();}
            bool perched=!map&&perch!=null&&perch.Paint(phase,zone);
            var band=layeredWorld!=null?layeredWorld.FlightBand:Vector2.zero;
            if(!map&&source!=null&&source.TryWildlifeFlightBand(out var corridor))band=corridor;
            extraVisitors?.Paint(phase,zone,stage,visible,reducedMotion,quality,map,cosmeticId,band,island);
            float baseDistance=Mathf.Max(cameraView.nearClipPlane+.5f,Vector3.Dot(transform.position-cameraView.transform.position,cameraView.transform.forward)+2.1f);
            for(int i=0;i<creatures.Length;i++) {
                if(i==0&&perched){ActiveCreatureCount++;continue;}
                var creature=creatures[i];creature.Hide();if(i>0&&quality==1||map&&i>1||i==3)continue;
                var species=worldLife[zone*4+i];
                var flight=FlightFor(species);
                int salt=sessionSalt+zone*97+i*211+island*43;
                float clock=phase-contextStarted-i*5.3f+WildlifeMotion.Hash(salt+311)*.8f;
                float period=flight.Period*(map?1.08f:1f);
                float longest=i==2?Mathf.Max(flight.Duration,FlightFor(worldLife[zone*4+3]).Duration):flight.Duration;
                period=Mathf.Max(period,longest*1.42f+8.5f);
                int cycle=Mathf.FloorToInt(clock/period);
                float seed=WildlifeMotion.Hash(cycle*173+salt);
                // One insect slot alternates residents; no extra simultaneous swarm.
                if(i==2&&seed>.5f){species=worldLife[zone*4+3];flight=FlightFor(species);}
                bool bird=WildlifeMotion.IsBird(species);
                float age=Mathf.Repeat(clock,period)-(1.5f+seed*2.7f);
                float duration=flight.Duration*1.42f*Mathf.Lerp(.90f,1.05f,WildlifeMotion.Hash(cycle*71+salt+101));
                float depthSeed=WildlifeMotion.Hash(cycle*191+salt+59);
                duration*=Mathf.Lerp(1.22f,.86f,WildlifePerspective.Proximity(.5f,depthSeed,i));
                if(!visitGates[i].Admit(cycle,age)||age<0||age>=duration)continue;
                float progress=age/duration;
                float side=WildlifeMotion.Hash(cycle*83+salt+11)<.5f?-1:1;
                bool crossSky=!map&&seed>(bird?.55f:.42f)&&band.y>band.x+.2f;
                WildlifeMotion.Route(side,seed,crossSky,map,out var a,out var b,out var c,out var d);
                float depth=WildlifePerspective.Proximity(progress,depthSeed,i);
                // Keep free-flight paintings between the foreground frame and
                // the opaque sky; visual distance comes from size, haze and pace.
                float distance=baseDistance+.72f+(1-depth)*.24f;
                float height=cameraView.orthographic?cameraView.orthographicSize*2:2*distance*Mathf.Tan(cameraView.fieldOfView*.5f*Mathf.Deg2Rad);
                float size=WildlifePerspective.BodySize(species,Mathf.Min(height*cameraView.aspect,height),depth)*(map?.86f:1);
                var tint=zone==8?new Color(.88f,.93f,1):zone==1||zone==6?new Color(.86f,.94f,1):zone==2||zone==7?new Color(1,.91f,.78f):Color.white;
                tint=WildlifePerspective.Tint(tint,depth,Mathf.SmoothStep(0,1,Mathf.Min(progress,1-progress)/.16f));
                creature.Select(species);creature.Fly(cameraView,a,b,c,d,age,duration,flight.WingRate,seed,flight.Flutter,zone,side,size,distance,tint,crossSky?.16f:.105f,crossSky?band:Vector2.zero,depthSeed);
                ActiveCreatureCount++;
            }
        }
        void Hide(){ActiveCreatureCount=0;extraVisitors?.Hide();foreach(var creature in creatures)creature?.Hide();}
        void OnDisable()=>Hide();
        void OnDestroy(){foreach(var creature in creatures)creature?.Dispose();if(extraVisitors!=null)Destroy(extraVisitors.gameObject);library?.Dispose();}
    }
}
