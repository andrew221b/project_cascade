using System;
using UnityEngine;
namespace Cascade.Presentation
{
    // A picture of precomputed Resolve outcomes. It never owns or mutates the playable attempt.
    public sealed class DeviceDemonstration:MonoBehaviour
    {
        CascadeGame game;Action<float,bool> paint;
        public void Initialize(CascadeGame owner,Action<float,bool> painter){game=owner;paint=painter;}
        void LateUpdate(){if(game==null||paint==null)return;float t=game.ReducedMotion?2.0f:Mathf.Repeat(game.Machine.AmbientPhase,7.0f);paint(t,t>=3.5f);}
    }
}
