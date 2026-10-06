using UnityEngine;
using UnityEngine.InputSystem;

namespace Cascade.Presentation
{
    // Accelerometer motion is a decorative impulse, never a puzzle command.
    public sealed class PhoneShakeFeedback:MonoBehaviour
    {
        CascadeGame game;Accelerometer sensor;bool enabledSensor,calibrated;
        Vector3 gravity;float cooldown;
        public int AppliedImpulses {get;private set;}
        public void Initialize(CascadeGame owner){game=owner;}
        bool CanMove=>game!=null&&game.isActiveAndEnabled&&game.CurrentScreen==GameScreen.Playing
            &&!game.ReducedMotion&&!game.Interaction.IsPaused&&!game.Interaction.IsSuspended;
        void Update()
        {
            if(!CanMove){ReleaseSensor();calibrated=false;return;}
            if(sensor==null){sensor=Accelerometer.current;if(sensor!=null&&!sensor.enabled){InputSystem.EnableDevice(sensor);enabledSensor=true;}}
            if(sensor!=null&&sensor.enabled)SampleAcceleration(sensor.acceleration.ReadValue(),Time.unscaledDeltaTime);
        }
        public void SampleAcceleration(Vector3 acceleration,float seconds)
        {
            if(!CanMove||!float.IsFinite(acceleration.x)||!float.IsFinite(acceleration.y)||!float.IsFinite(acceleration.z))return;
            float dt=Mathf.Clamp(seconds,0,.05f);cooldown=Mathf.Max(0,cooldown-dt);
            // A just-enabled sensor may still contain an all-zero sample. Wait for gravity
            // rather than interpreting its first real reading as a phone shake.
            if(!calibrated){if(acceleration.sqrMagnitude<.01f)return;gravity=acceleration;calibrated=true;return;}
            Vector3 motion=acceleration-gravity;
            gravity=Vector3.Lerp(gravity,acceleration,1-Mathf.Exp(-dt*4));
            // Sensors report g. Slow orientation changes stay below this high-pass threshold.
            if(cooldown>0||motion.magnitude<.38f||game.IsHoldingToken)return;
            cooldown=.32f;AppliedImpulses++;
            var impulse=Vector2.ClampMagnitude(new Vector2(motion.x,motion.y)*.65f,.85f);
            game.Machine.ShakeSuspension(impulse);
            if(game.Interaction.CanPlay)game.Feedback.RowPhysics.BeginShake(impulse);
        }
        void ReleaseSensor(){if(enabledSensor&&sensor!=null&&sensor.added)InputSystem.DisableDevice(sensor);enabledSensor=false;sensor=null;}
        void OnDisable(){ReleaseSensor();calibrated=false;cooldown=0;}
    }
}
