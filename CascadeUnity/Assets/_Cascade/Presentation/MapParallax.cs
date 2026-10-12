using UnityEngine;
using UnityEngine.InputSystem;

namespace Cascade.Presentation
{
    // Scenery and island hit targets share one transform, so decoration cannot move a target away.
    public sealed class MapParallax:MonoBehaviour
    {
        CascadeGame game;RectTransform rect;Vector2 rest,offset,referenceGravity;bool calibrated;
        GravitySensor sensor;bool enabledSensor;
        public Vector2 Offset=>offset;
        public void Initialize(CascadeGame owner)
        {
            game=owner;rect=(RectTransform)transform;rest=rect.anchoredPosition;offset=owner.MapAmbientOffset;rect.anchoredPosition=rest+offset;
            sensor=GravitySensor.current;
            if(sensor!=null&&!sensor.enabled){InputSystem.EnableDevice(sensor);enabledSensor=true;}
        }
        void LateUpdate()
        {
            if(game==null||rect==null)return;
            bool quiet=game.ReducedMotion||game.Interaction.IsSuspended;
            Vector2 tilt=Vector2.zero;
            if(!quiet&&sensor!=null&&sensor.enabled){
                var gravity=(Vector2)sensor.gravity.ReadValue();
                if(!calibrated){referenceGravity=gravity;calibrated=true;}
                tilt=Vector2.ClampMagnitude((gravity-referenceGravity)*.45f,1);
            }
            else if(!quiet&&Mouse.current!=null){var p=Mouse.current.position.ReadValue();tilt=new Vector2(p.x/Mathf.Max(1,Screen.width)-.5f,p.y/Mathf.Max(1,Screen.height)-.5f);tilt=Vector2.ClampMagnitude(tilt,1);}
            float t=game.Machine.AmbientPhase;
            var target=quiet?Vector2.zero:tilt*5+new Vector2(Mathf.Sin(t*.12f)*1.5f,Mathf.Sin(t*.17f)*1.1f);
            offset=quiet?Vector2.zero:Vector2.Lerp(offset,target,1-Mathf.Exp(-Time.unscaledDeltaTime*4));rect.anchoredPosition=rest+offset;game.MapAmbientOffset=offset;
        }
        void OnDisable(){if(enabledSensor&&sensor!=null&&sensor.added){InputSystem.DisableDevice(sensor);enabledSensor=false;}}
    }
}
