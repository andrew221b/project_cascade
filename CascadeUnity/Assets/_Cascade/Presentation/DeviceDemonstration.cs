using System;
using UnityEngine;
namespace Cascade.Presentation
{
    // A picture of precomputed Resolve outcomes. It never owns or mutates the playable attempt.
    public sealed class DeviceDemonstration:MonoBehaviour
    {
        CascadeGame game;Action<float,bool> paint;bool separateClock;float elapsed;
        public void Initialize(CascadeGame owner,Action<float,bool> painter,bool isolatedClock=false){game=owner;paint=painter;separateClock=isolatedClock;}
        void LateUpdate(){if(game==null||paint==null)return;if(separateClock&&game.IntroductionAnimationAllowed)elapsed+=Time.unscaledDeltaTime;float t=game.ReducedMotion?2.0f:Mathf.Repeat(separateClock?elapsed:game.Machine.AmbientPhase,7.0f);paint(t,t>=3.5f);}
    }
}
