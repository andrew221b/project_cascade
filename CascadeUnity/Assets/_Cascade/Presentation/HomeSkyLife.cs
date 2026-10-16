using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    // Quiet, decorative visitors reuse the game's measured wing poses and ambient clock.
    public sealed class HomeSkyLife:MonoBehaviour
    {
        CascadeGame game;RectTransform sky;float age;bool welcome;readonly Image[] actors=new Image[2];
        readonly WildlifeImageRig[] rigs=new WildlifeImageRig[2];
        readonly WildlifeAudioEmitter[] sound=new WildlifeAudioEmitter[2];
        readonly WildlifeMotion.VisitGate[] gates=new WildlifeMotion.VisitGate[2];
        int sessionSalt=System.Guid.NewGuid().GetHashCode()&0x3fffffff;
#if UNITY_EDITOR
        bool review;
        public void SetReviewAge(float age){review=true;Pose(age);}
        public void SetReviewSequence(int seed){sessionSalt=seed;for(int i=0;i<gates.Length;i++)gates[i].Reset();}
        public void EndReview()=>review=false;
#endif
        public void Initialize(CascadeGame owner)
        {
            game=owner;sky=(RectTransform)transform;welcome=game.CurrentScreen==GameScreen.AccountGate;
            if(rigs[0]!=null)return;
            for(int i=0;i<2;i++){
                rigs[i]=new WildlifeImageRig(transform,i==0?"Distant bird":"Distant insect");actors[i]=rigs[i].Body;
                sound[i]=rigs[i].Root.gameObject.AddComponent<WildlifeAudioEmitter>();
                sound[i].Bind(game,actors[i],i==0?WildlifeSpecies.Swallow:WildlifeSpecies.Butterfly,rigs[i].RearBody);
            }
            LateUpdate();
        }
        void LateUpdate()
        {
#if UNITY_EDITOR
            if(review)return;
#endif
            if(game==null||game.Machine==null)return;
            // A slow cold-start frame must not skip the visitor's admission window.
            if(game.CosmeticAmbianceMotionAllowed&&!game.ReducedMotion&&game.Preferences.EffectQuality>0)age+=Mathf.Min(Time.unscaledDeltaTime,.05f);
            Pose(age);
        }
        void Pose(float age)
        {
            int world=game.Machine.LivingWorld.Zone;
            for(int i=0;i<2;i++){
                float period=i==0?34:43,delay=i==0?(welcome?.15f:1.5f):7;
                float elapsed=age-delay;int cycle=Mathf.FloorToInt(elapsed/period);float t=Mathf.Repeat(elapsed,period);
                float seed=WildlifeMotion.Hash(sessionSalt+cycle*173+i*719+world*71);
                var species=i==0?WorldLifeView.BirdSpecies(world,(cycle&1)!=0):Companion(world,cycle);
                var style=WildlifeMotion.Style(species);
                float duration=(style==WildlifeFlightStyle.SoaringBird||style==WildlifeFlightStyle.Owl?19:style==WildlifeFlightStyle.Moth?18:style==WildlifeFlightStyle.Bee?12:14)*Mathf.Lerp(.9f,1.1f,seed);
                duration*=Mathf.Lerp(1.22f,.86f,WildlifePerspective.Proximity(.5f,seed,i));
                bool active=elapsed>=0&&gates[i].Admit(cycle,t)&&t<duration&&game.CosmeticAmbianceMotionAllowed&&!game.ReducedMotion&&game.Preferences.EffectQuality>0
                    &&(i==0||game.Preferences.EffectQuality>1);
                if(!active){rigs[i].Hide();continue;}
                float side=WildlifeMotion.Hash(sessionSalt+cycle*29+i*311)<.5f?-1:1,u=t/duration;
                WildlifeMotion.Route(side,seed,true,false,out var a,out var b,out var c,out var d);
                var point=WildlifeMotion.Path(a,b,c,d,style,t,duration,seed,.006f,world,side);
                Vector3 Heading(float at)=>Vector3.Scale(WildlifeMotion.Heading(a,b,c,d,style,at,duration,seed,world,side),new Vector3(sky.rect.width/Mathf.Max(1,sky.rect.height),1,1)).normalized;
                var heading=Heading(t);float facing=heading.x>=0?1:-1,bank=WildlifeMotion.Bank(heading,facing);
                float steering=Mathf.Abs(Vector3.SignedAngle(Heading(t-.18f),Heading(t+.18f),Vector3.forward));
                var pose=WildlifeMotion.Pose(style,t,Rate(species),seed,bank,WildlifeMotion.Bank(Heading(t-.18f),facing),Mathf.SmoothStep(0,1,(steering-5)/32));
                float depth=WildlifePerspective.Proximity(u,seed,i),size=WildlifePerspective.BodySize(species,sky.rect.width,depth);
                var tint=world==8?new Color(.84f,.90f,1):world==1||world==6?new Color(.9f,.96f,1):Color.white;
                tint=WildlifePerspective.Tint(tint,depth,Mathf.SmoothStep(0,1,Mathf.Min(u,1-u)/.13f));
                rigs[i].Select(species);sound[i].SetSpecies(species);
                rigs[i].Paint(Vector2.Scale(point,sky.rect.size),size,heading,bank,pose,tint,WildlifeMotion.RearView(heading));
            }
        }
        static WildlifeSpecies Companion(int world,int visit)=>world==0||world==2?WildlifeSpecies.Bumblebee:world==1||world==4?WildlifeSpecies.Dragonfly:world==6?WildlifeSpecies.SnowBunting:world==8?WildlifeSpecies.MoonMoth:world==5?WildlifeSpecies.BookMoth:WildlifeSpecies.Butterfly;
        static float Rate(WildlifeSpecies species)=>species==WildlifeSpecies.Bumblebee?10.8f:species==WildlifeSpecies.BarnOwl?1.3f:
            species==WildlifeSpecies.Gull?1.6f:species==WildlifeSpecies.Tern?2.1f:species==WildlifeSpecies.Dragonfly?7.8f:
            WildlifeMotion.Style(species)==WildlifeFlightStyle.Moth?1.1f:species==WildlifeSpecies.Butterfly?1.45f:4.7f;
        void OnDisable(){foreach(var rig in rigs)rig?.Hide();}
    }
}
