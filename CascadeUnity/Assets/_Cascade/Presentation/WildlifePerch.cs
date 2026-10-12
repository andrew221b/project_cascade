using UnityEngine;

namespace Cascade.Presentation
{
    public enum WildlifePerchState { None, Approaching, Resting, Departing }
    public static class WildlifePerchMotion
    {
        public const float EmptyIdle=3.2f,OccupiedIdle=9,ApproachTime=1.7f,DepartureTime=1.1f;
        public static float LandingProgress(float elapsed)
        {float t=Mathf.Clamp01(elapsed/ApproachTime);return 1-Mathf.Pow(1-t,1.8f);}
        public static float LeavingProgress(float elapsed)
        {float t=Mathf.Clamp01(elapsed/DepartureTime);return t*t*(2-t);}
        public static float Fold(float elapsed)=>Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.38f));
        public static float LandingWingClock(float elapsed)
        {float boost=Mathf.Clamp01((elapsed-.7f)/(ApproachTime-.7f));return elapsed+.35f*boost*boost;}
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
        float started,nextVisit,lastPhase=-1,size,facing,seed,restTime,lastAngle;
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
                int preferred=Mathf.FloorToInt(WildlifeMotion.Hash(sessionSalt+visit*317+zone*89+29)*4);
                if(!source.TryChooseWildlifePerch(preferred,seed,source.WildlifeIdleSeconds>=WildlifePerchMotion.OccupiedIdle,out target,lastTarget))return false;
                if(!source.TryWildlifeSupport(target,out contact,out lastAngle))return false;
                float h=camera.orthographicSize*2,w=h*camera.aspect;
                size=Mathf.Min(w,h)*(target.Kind==WildlifePerchKind.Board?.039f:.048f);
                float side=WildlifeMotion.Hash(sessionSalt+visit*71+41)<.5f?-1:1;facing=-side;
                origin=camera.ViewportToWorldPoint(new Vector3(side<0?-.07f:1.07f,Mathf.Clamp(camera.WorldToViewportPoint(contact).y+.15f,.2f,.83f),camera.WorldToViewportPoint(contact).z));
                origin.z=contact.z;lastContact=contact;started=phase;restTime=4.5f+WildlifeMotion.Hash(sessionSalt+visit*101+59)*4.5f;lastTarget=target;
                activity=source.WildlifeActivity;State=WildlifePerchState.Approaching;visit++;
                bird.Select(WorldLifeView.BirdSpecies(zone,seed>.5f));
            }
            if(State!=WildlifePerchState.Departing){
                bool support=source.TryWildlifeSupport(target,out contact,out var angle);
                if(support){lastContact=contact;lastAngle=angle;}
                if(!source.WildlifeCanLand||activity!=source.WildlifeActivity||!support)Depart(phase);
            }
            float age=phase-started;
            var tint=zone==8?new Color(.84f,.90f,1):zone==2||zone==7?new Color(1,.94f,.83f):Color.white;
            float wingAge=State==WildlifePerchState.Approaching?WildlifePerchMotion.LandingWingClock(age):age;
            var pose=WildlifeMotion.Pose(WildlifeMotion.Style(bird.Species),wingAge,5.1f,seed,effort:1);
            Vector3 feet;float fold=0,tilt=0;
            if(State==WildlifePerchState.Approaching){
                float t=WildlifePerchMotion.LandingProgress(age);
                var a=origin;var b=origin+new Vector3(facing*size*4,-size,0);
                var c=lastContact+new Vector3(-facing*size*.8f,size*1.6f,0);
                feet=WildlifeMotion.Bezier(a,b,c,lastContact,t);
                tilt=Mathf.Lerp(-facing*12,lastAngle,Mathf.SmoothStep(0,1,t));
                tint.a=Mathf.SmoothStep(0,1,age/.25f);
                if(age>=WildlifePerchMotion.ApproachTime){touchdown=pose;State=WildlifePerchState.Resting;started=phase;feet=lastContact;}
            }else if(State==WildlifePerchState.Resting){
                feet=lastContact;fold=WildlifePerchMotion.Fold(age);
                tilt=lastAngle+Mathf.Sin(age*1.25f+seed*8)*1.4f;
                pose=new WildlifePose(touchdown.NearWing,touchdown.FarWing,touchdown.WingFold,Mathf.Sin(age*.9f+seed)*1.8f,0,touchdown.NearFlex,touchdown.FarFlex,touchdown.Phase,true);
                if(age>restTime){Depart(phase);age=0;fold=1;}
            }else{
                float t=WildlifePerchMotion.LeavingProgress(age);
                var edge=camera.ViewportToWorldPoint(new Vector3(facing<0?-.12f:1.12f,Mathf.Clamp(camera.WorldToViewportPoint(origin).y+.18f,.2f,.86f),camera.WorldToViewportPoint(origin).z));
                edge.z=origin.z;
                feet=WildlifeMotion.Bezier(origin,origin+new Vector3(facing*size,size*3,0),edge+new Vector3(-facing*size*4,-size,0),edge,t);
                fold=1-WildlifePerchMotion.Fold(age*2);tilt=facing*14*(1-t);
                tint.a=1-Mathf.SmoothStep(0,1,(age/WildlifePerchMotion.DepartureTime-.70f)/.30f);
                if(age>=WildlifePerchMotion.DepartureTime){State=WildlifePerchState.None;return false;}
            }
            bird.PaintOnFeet(feet,camera.transform.rotation*Quaternion.Euler(0,0,tilt),size,facing,pose,tint,fold);
            return true;
        }
        void Depart(float phase)
        {
            // Current painted foot position is recomputed from the rig, avoiding a jump mid-approach.
            origin=bird.FeetPosition;
            facing=origin.x<camera.transform.position.x?-1:1;
            started=phase;State=WildlifePerchState.Departing;nextVisit=phase+8+WildlifeMotion.Hash(sessionSalt+visit*137+97)*7;
        }
    }
}
