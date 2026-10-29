using UnityEngine;

namespace Cascade.Presentation
{
    public enum WildlifePerchState { None, Approaching, Resting, Departing }
    public static class WildlifePerchMotion
    {
        public const float EmptyIdle=3.2f,OccupiedIdle=9,ApproachTime=1.7f,DepartureTime=1.1f;
        public static float LandingProgress(float elapsed)
        {float t=Mathf.Clamp01(elapsed/ApproachTime);return t*(4-t*t*t)/3;}
        public static float LeavingProgress(float elapsed)
        {float t=Mathf.Clamp01(elapsed/DepartureTime);return t*t*(2-t);}
        public static float Fold(float elapsed)=>Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.38f));
        public static float LandingWingClock(float elapsed)
        {float boost=Mathf.Clamp01((elapsed-.7f)/(ApproachTime-.7f));return elapsed+.35f*boost*boost;}
        public static bool Clings(WildlifeSpecies species)=>species==WildlifeSpecies.Bumblebee||species==WildlifeSpecies.FieldCricket;
        public static float FrameAngle(bool right)=>right?90:-90;
    }
    // One pooled bird may land at a time. Presentation randomness never enters the puzzle.
    internal sealed class WildlifePerch
    {
        readonly ToyMachineView source;
        readonly Camera camera;
        readonly WildlifeRig bird;
        readonly int sessionSalt;
        WildlifePerchTarget? lastTarget;
        public WildlifePerchState State{get;private set;}
        public WildlifePerchKind? Kind=>State==WildlifePerchState.None?null:target.Kind;
        WildlifePerchTarget target;
        WildlifePose touchdown;
        Vector3 origin,contact,lastContact;
        float started,wingStarted,nextVisit,lastPhase=-1,size,facing,seed,restTime,lastAngle;
        float approachTime=WildlifePerchMotion.ApproachTime,departureTime=WildlifePerchMotion.DepartureTime;
        bool crawling,clinging;
        float paintedAngle,departureAngle;
        int activity,revision,visit;
        public WildlifePerch(ToyMachineView source,Camera camera,WildlifeRig bird,int sessionSalt=0){this.source=source;this.camera=camera;this.bird=bird;this.sessionSalt=sessionSalt;}
        public void Cancel(float phase){State=WildlifePerchState.None;nextVisit=phase+2;lastPhase=phase;}
        public bool Paint(float phase,int zone)
        {
            if(source==null||camera==null||!source.WildlifeSceneVisible){Cancel(phase);return false;}
            if(phase<lastPhase||revision!=source.WildlifeRevision){Cancel(phase);revision=source.WildlifeRevision;}
            lastPhase=phase;
            if(State==WildlifePerchState.None){
                if(!source.WildlifeCanLand||phase<nextVisit||source.WildlifeIdleSeconds<WildlifePerchMotion.EmptyIdle)return false;
                seed=WildlifeMotion.Hash(sessionSalt+zone*71+visit*173+revision*43);
                int preferred=visit%2==0?0:3;
                if(!source.TryChooseWildlifePerch(preferred,seed,source.WildlifeIdleSeconds>=WildlifePerchMotion.OccupiedIdle,out target,lastTarget))return false;
                if(!source.TryWildlifeSupport(target,out contact,out lastAngle))return false;
                float h=camera.orthographicSize*2,w=h*camera.aspect;
                var species=WorldFauna.Perched(zone,visit,target.Kind==WildlifePerchKind.Frame);
                crawling=species==WildlifeSpecies.FieldCricket;
                clinging=target.Kind==WildlifePerchKind.Frame&&WildlifePerchMotion.Clings(species);
                approachTime=crawling?2.6f:species==WildlifeSpecies.Bumblebee?2.35f:species==WildlifeSpecies.BarnOwl?2.4f:WildlifePerchMotion.ApproachTime;
                departureTime=crawling?1.7f:species==WildlifeSpecies.BarnOwl?1.8f:WildlifePerchMotion.DepartureTime;
                size=Mathf.Min(w,h)*(target.Kind==WildlifePerchKind.Board?.039f:species==WildlifeSpecies.BarnOwl?.062f:.050f);
                if(!WildlifeMotion.IsBird(species))size*=species==WildlifeSpecies.Bumblebee?.28f:.38f;
                float side=WildlifeMotion.Hash(sessionSalt+visit*71+41)<.5f?-1:1;facing=-side;
                origin=camera.ViewportToWorldPoint(new Vector3(side<0?-.07f:1.07f,Mathf.Clamp(camera.WorldToViewportPoint(contact).y+.15f,.2f,.83f),camera.WorldToViewportPoint(contact).z));
                origin.z=contact.z;lastContact=contact;wingStarted=started=phase;restTime=4.5f+WildlifeMotion.Hash(sessionSalt+visit*101+59)*4.5f;lastTarget=target;
                if(target.Kind==WildlifePerchKind.Frame){
                    facing=target.Index==0?1:-1;
                    if(clinging){facing=-facing;lastAngle=WildlifePerchMotion.FrameAngle(target.Index!=0);origin=camera.ViewportToWorldPoint(new Vector3(target.Index==0?.28f:.72f,-.12f,camera.WorldToViewportPoint(contact).z));origin.z=contact.z;}
                    if(crawling){origin=contact-Vector3.up*(size*3);restTime+=5;}
                }
                if(species==WildlifeSpecies.BarnOwl)restTime+=4;
                activity=source.WildlifeActivity;State=WildlifePerchState.Approaching;visit++;
                bird.Select(species);
            }
            if(State!=WildlifePerchState.Departing){
                bool support=source.TryWildlifeSupport(target,out contact,out var angle);
                if(support){lastContact=contact;lastAngle=clinging?WildlifePerchMotion.FrameAngle(target.Index!=0):angle;}
                if(!source.WildlifeCanLand||activity!=source.WildlifeActivity||!support)Depart(phase);
            }
            float age=phase-started;
            var tint=zone==8?new Color(.84f,.90f,1):zone==2||zone==7?new Color(1,.94f,.83f):Color.white;
            float flightAge=phase-wingStarted;
            float wingAge=State==WildlifePerchState.Approaching?WildlifePerchMotion.LandingWingClock(flightAge):flightAge+.35f;
            float rate=bird.Species==WildlifeSpecies.BarnOwl?1.8f:bird.Species==WildlifeSpecies.Bumblebee?11.2f:5.1f;
            var pose=WildlifeMotion.Pose(WildlifeMotion.Style(bird.Species),wingAge,rate,seed,effort:1);
            Vector3 feet;float fold=0,tilt=0;
            if(State==WildlifePerchState.Approaching){
                float t=WildlifePerchMotion.LandingProgress(age/approachTime*WildlifePerchMotion.ApproachTime);
                var a=origin;var b=origin+new Vector3(facing*size*4,-size,0);
                var c=lastContact+new Vector3(-facing*size*.8f,size*1.6f,0);
                if(clinging){var normal=new Vector3(target.Index==0?1:-1,0,0);b=origin+Vector3.up*(size*6)+normal*size;c=lastContact+normal*(size*.8f)-Vector3.up*size;}
                feet=crawling?Vector3.Lerp(origin,lastContact,t)+Vector3.up*(Mathf.Pow(Mathf.Sin(t*Mathf.PI*3),2)*size*.22f):WildlifeMotion.Bezier(a,b,c,lastContact,t);
                tilt=crawling?lastAngle:Mathf.LerpAngle(-facing*12,lastAngle,Mathf.SmoothStep(0,1,t));
                tint.a=Mathf.SmoothStep(0,1,age/.25f);
                if(age>=approachTime){touchdown=pose;State=WildlifePerchState.Resting;started=phase;feet=lastContact;}
            }else if(State==WildlifePerchState.Resting){
                feet=lastContact;fold=WildlifePerchMotion.Fold(age);
                tilt=lastAngle+(clinging?0:Mathf.Sin(age*1.25f+seed*8)*1.4f);
                pose=new WildlifePose(touchdown.NearWing,touchdown.FarWing,touchdown.WingFold,Mathf.Sin(age*.9f+seed)*1.8f,0,touchdown.NearFlex,touchdown.FarFlex,touchdown.Phase,true);
                if(crawling){float phrase=Mathf.SmoothStep(0,1,Mathf.Sin(age*1.3f+seed*7));pose=new WildlifePose(0,0,Mathf.Sin(age*19)*phrase,Mathf.Sin(age*1.7f+seed)*3,0,0,0,0,true);}
                if(age>restTime){Depart(phase);age=0;fold=1;}
            }else{
                float t=WildlifePerchMotion.LeavingProgress(age/departureTime*WildlifePerchMotion.DepartureTime);
                var edge=camera.ViewportToWorldPoint(new Vector3(facing<0?-.12f:1.12f,Mathf.Clamp(camera.WorldToViewportPoint(origin).y+.18f,.2f,.86f),camera.WorldToViewportPoint(origin).z));
                edge.z=origin.z;
                if(crawling){edge=origin-Vector3.up*(size*4);feet=Vector3.Lerp(origin,edge,t)+Vector3.up*(Mathf.Pow(Mathf.Sin(t*Mathf.PI*2),2)*size*.25f);}
                else {var lift=clinging?new Vector3(target.Index==0?size*2:-size*2,size*2,0):new Vector3(facing*size,size*3,0);feet=WildlifeMotion.Bezier(origin,origin+lift,edge+new Vector3(-facing*size*4,-size,0),edge,t);}
                fold=1-WildlifePerchMotion.Fold(age*2);tilt=crawling?departureAngle:Mathf.LerpAngle(departureAngle,facing*14*(1-t),Mathf.SmoothStep(0,1,t*2));
                tint.a=1-Mathf.SmoothStep(0,1,(age/departureTime-.70f)/.30f);
                if(age>=departureTime){State=WildlifePerchState.None;return false;}
            }
            bird.PaintOnFeet(feet,camera.transform.rotation*Quaternion.Euler(0,0,tilt),size,facing,pose,tint,fold);
            paintedAngle=tilt;
            bird.Resting=State==WildlifePerchState.Resting;
            return true;
        }
        void Depart(float phase)
        {
            // Current painted foot position is recomputed from the rig, avoiding a jump mid-approach.
            origin=bird.FeetPosition;
            departureAngle=paintedAngle;
            // Keep the landed facing: launch forwards rather than mirror the entire body in one frame.
            started=phase;State=WildlifePerchState.Departing;nextVisit=phase+8+WildlifeMotion.Hash(sessionSalt+visit*137+97)*7;
        }
    }
}
