using UnityEngine;
using UnityEngine.InputSystem;

namespace Cascade.Presentation
{
    // Accelerometer motion is a decorative impulse, never a puzzle command.
    public sealed class PhoneShakeFeedback:MonoBehaviour
    {
        CascadeGame game;Accelerometer sensor;bool enabledSensor,calibrated,tiltArmed=true;
        Vector3 gravity,neutralGravity;float cooldown;
        public int AppliedImpulses {get;private set;}
        public void Initialize(CascadeGame owner){game=owner;}
        bool CanMove=>game!=null&&game.isActiveAndEnabled&&game.Interaction!=null&&game.CurrentScreen==GameScreen.Playing
            &&!game.ReducedMotion&&!game.Interaction.IsPaused&&!game.Interaction.IsSuspended;
        void Update()
        {
            if(!CanMove){ReleaseSensor();calibrated=false;tiltArmed=true;cooldown=0;return;}
            if(sensor==null){sensor=Accelerometer.current;if(sensor!=null&&!sensor.enabled){InputSystem.EnableDevice(sensor);enabledSensor=true;}}
            if(sensor!=null&&sensor.enabled)SampleAcceleration(sensor.acceleration.ReadValue(),Time.unscaledDeltaTime);
        }
        public void SampleAcceleration(Vector3 acceleration,float seconds)
        {
            if(!CanMove||!float.IsFinite(acceleration.x)||!float.IsFinite(acceleration.y)||!float.IsFinite(acceleration.z))return;
            float dt=Mathf.Clamp(seconds,0,.05f);cooldown=Mathf.Max(0,cooldown-dt);
            // A just-enabled sensor may still contain an all-zero sample. Wait for gravity
            // rather than interpreting its first real reading as a phone shake.
            if(!calibrated){if(acceleration.sqrMagnitude<.01f)return;gravity=neutralGravity=acceleration;calibrated=true;tiltArmed=true;return;}
            Vector3 motion=acceleration-gravity;
            gravity=Vector3.Lerp(gravity,acceleration,1-Mathf.Exp(-dt*4));
            // Detect deliberate sideways tilt separately from the high-pass shake. The
            // latch prevents a held angle from injecting impulses every cooldown interval.
            float tilt=acceleration.x-neutralGravity.x;
            if(Mathf.Abs(tilt)<.08f)tiltArmed=true;
            bool tilted=tiltArmed&&Mathf.Abs(tilt)>=.15f;
            bool shaken=motion.magnitude>=.38f;
            if((tilted||shaken)&&Mathf.Abs(tilt)>=.15f)tiltArmed=false;
            if((!tilted&&!shaken)||cooldown>0||game.IsHoldingToken)return;
            cooldown=.32f;AppliedImpulses++;
            var impulse=tilted&&!shaken
                ?new Vector2(Mathf.Clamp(tilt*4.5f,-.9f,.9f),0)
                :Vector2.ClampMagnitude(new Vector2(motion.x,motion.y)*.65f,.85f);
            game.Machine.ShakeSuspension(impulse);
            // Seated pieces rock with the deck; shaking never launches the row.
        }
        void ReleaseSensor(){if(enabledSensor&&sensor!=null&&sensor.added)InputSystem.DisableDevice(sensor);enabledSensor=false;sensor=null;}
        void OnDisable(){ReleaseSensor();calibrated=false;tiltArmed=true;cooldown=0;}
    }
}
