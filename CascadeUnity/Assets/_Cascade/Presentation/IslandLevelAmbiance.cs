using UnityEngine;
namespace Cascade.Presentation
{
    // Weather is behind the readable controls and shares the existing quality/focus policy.
    public sealed class IslandLevelAmbiance:MonoBehaviour
    {
        CascadeGame game;MenuWorldParticles weather;float age;bool focused=true,paused;
        public void Initialize(CascadeGame owner,MenuWorldParticles particles){game=owner;weather=particles;Paint();}
        void Paint()=>weather.Pose(age,game!=null&&focused&&!paused&&!game.ReducedMotion&&game.CelebrationMotionAllowed?game.Preferences.EffectQuality:0);
        void Update(){if(game!=null&&focused&&!paused&&game.CelebrationMotionAllowed&&!game.ReducedMotion)age+=Mathf.Min(Time.unscaledDeltaTime,.05f);Paint();}
        void OnApplicationFocus(bool value){focused=value;if(weather!=null)Paint();}
        void OnApplicationPause(bool value){paused=value;if(weather!=null)Paint();}
    }
}
