using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        sealed class ContactFace {public int floor,slot;public TextMeshProUGUI label;}
        readonly List<ContactFace> contactFaces=new List<ContactFace>();
        void LateUpdate()
        {
            if(machine==null||Interaction==null)return;
            machine.TickWoodDynamics(Time.unscaledDeltaTime);
            for(int i=0;i<deviceOverlays.Count;i++)deviceOverlays[i].position=floorViews[i].position+machine.FloorVisualOffset(i);
            float dt=Mathf.Clamp(Time.unscaledDeltaTime,0,.05f);
            foreach(var dock in deviceDocks) {
                if(dock.root==null||!dock.root.gameObject.activeSelf)continue;
                float target=ReducedMotion?0:Mathf.Sin(machine.AmbientPhase*.95f+dock.floor*.8f+dock.dockX*3f)*2.3f-machine.FloorVisualOffset(dock.floor).x*13f;
                dock.angleVelocity+=(target-dock.angle)*65f*dt;
                dock.angleVelocity*=Mathf.Exp(-11f*dt);
                dock.angle+=dock.angleVelocity*dt;
                dock.root.localRotation=Quaternion.Euler(0,0,dock.angle);
                float drift=Mathf.Sin(dock.angle*Mathf.Deg2Rad)*.025f;
                dock.leftStrap?.SetEndpoints(new Vector2(dock.slotX-.018f,.32f),new Vector2(dock.dockX-.055f+drift,.23f));
                dock.rightStrap?.SetEndpoints(new Vector2(dock.slotX+.018f,.32f),new Vector2(dock.dockX+.055f+drift,.23f));
            }
            SyncTokenFaces();
            foreach(var face in contactFaces)if(face.label!=null&&face.label.gameObject.activeInHierarchy){var rect=face.label.rectTransform;var position=machine.ContactPosition(face.floor,face.slot);position.z=rect.position.z;rect.position=position;}
        }
        void SyncTokenFaces()
        {
            if(machine==null)return;
            for(int i=0;i<tokenLabels.Count;i++) {
                var rect=tokenLabels[i].rectTransform;var position=machine.TokenVisualPosition(i);position.z=rect.position.z;rect.position=position;
                rect.rotation=Quaternion.Euler(0,0,machine.TokenVisualAngle(i));
            }
        }
    }
}
