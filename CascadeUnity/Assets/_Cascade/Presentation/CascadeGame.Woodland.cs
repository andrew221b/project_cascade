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
            feedback.RowPhysics.PresentShake();
            SyncDeviceSuspension(Time.unscaledDeltaTime);
            SyncTokenFaces();
            foreach(var face in contactFaces)if(face.label!=null&&face.label.gameObject.activeInHierarchy){var rect=face.label.rectTransform;var position=machine.ContactPosition(face.floor,face.slot);position.z=rect.position.z;rect.position=position;}
        }
        void SyncDeviceSuspension(float seconds)
        {
            if(machine==null||Interaction==null)return;
            for(int i=0;i<deviceOverlays.Count;i++) {
                deviceOverlays[i].position=floorViews[i].position+machine.FloorVisualOffset(i);
                deviceOverlays[i].rotation=Quaternion.Euler(0,0,machine.FloorVisualAngle(i));
                if(i<platformLegends.Count)platformLegends[i].FollowWood(machine.FloorSag(i)/Mathf.Max(.0001f,deviceOverlays[i].lossyScale.y),machine.FloorBreakProgress(i));
            }
            float dt=Mathf.Clamp(seconds,0,.05f);
            bool moving=!Interaction.IsPaused&&!Interaction.IsSuspended;
            bool movingDevice(DeviceDock d)=>moving&&d.responseTime>0;
            foreach(var dock in deviceDocks) {
                if(dock.root==null||!dock.root.gameObject.activeInHierarchy)continue;
                if(movingDevice(dock)){dock.responseTime=Mathf.Max(0,dock.responseTime-dt);}
                if(dock.countdown!=null)dock.countdown.fontSize=14/Mathf.Min(1,dock.countdown.GetComponentInParent<Canvas>().scaleFactor);
                float response=ReducedMotion?0:Mathf.Sin((1-dock.responseTime/.32f)*Mathf.PI)*dock.responseTime/.32f;
                dock.icon.transform.localScale=Vector3.one*(1+.10f*response);
                // The body performs the action: turn a key, press a stamp, unfurl a spool.
                float turn=dock.kind==3?18*response:dock.kind==9?-12*response:dock.kind==10?8*response:0;
                dock.icon.transform.localRotation=Quaternion.Euler(0,0,turn);
                if(dock.kind==8)dock.icon.transform.localScale=new Vector3(1+.13f*response,1-.12f*response,1);
                if(dock.kind==4)dock.icon.transform.localScale=new Vector3(1-.06f*response,1+.12f*response,1);
                var card=deviceOverlays[dock.floor];float height=Mathf.Max(1,card.rect.height);
                // Move only the cosmetic center hook clear of the engraved strength numeral.
                float hookX=Mathf.Abs(dock.slotX-.5f)<.045f?.5f+(dock.slotX<=.5f?-.052f:.052f):dock.slotX;
                var hookWorld=machine.SurfacePoint(dock.floor,hookX,.35f);
                var hookLocal=(Vector2)card.InverseTransformPoint(hookWorld);
                var hook=new Vector2((hookLocal.x-card.rect.xMin)/card.rect.width,(hookLocal.y-card.rect.yMin)/height);
                var anchor=(Vector2)hookWorld;
                if(ReducedMotion){dock.angle=dock.angleVelocity=0;dock.anchorReady=false;}
                else if(moving&&dt>0) {
                    var velocity=dock.anchorReady?(anchor-dock.anchorPosition)/dt:Vector2.zero;
                    float acceleration=dock.anchorReady?(velocity.x-dock.anchorVelocity.x)/dt:0;
                    dock.anchorPosition=anchor;dock.anchorVelocity=velocity;dock.anchorReady=true;
                    float weight=dock.kind==2?.75f:dock.kind==4||dock.kind==8?.88f:1.15f;
                    float target=machine.WindAt(dock.floor)*4.2f*weight-machine.FloorVisualAngle(dock.floor)-Mathf.Clamp(acceleration*2,-3,3);
                    int steps=Mathf.Max(1,Mathf.CeilToInt(dt/.012f));float step=dt/steps;
                    for(int i=0;i<steps;i++) {
                        dock.angleVelocity+=(target-dock.angle)*29f*step;
                        dock.angleVelocity*=Mathf.Exp(-4.4f*step);dock.angle+=dock.angleVelocity*step;
                    }
                    dock.angle=Mathf.Clamp(dock.angle,-10,10);
                }
                else if(!moving){dock.anchorReady=false;}
                // Constant screen-space suspension length, independent of floor spacing.
                float unit=1/Mathf.Min(1,dock.root.GetComponentInParent<Canvas>().scaleFactor);
                var attachment=dock.kind==2?new Vector2(.365f,.69f):dock.kind==3?new Vector2(.465f,1.08f):dock.kind==4?new Vector2(.47f,.78f):dock.kind==8?new Vector2(.68f,1.02f):dock.kind==9?new Vector2(.46f,1.01f):new Vector2(.465f,1.02f);
                var gripLocal=dock.root.rect.min+Vector2.Scale(dock.root.rect.size,attachment);
                float spread=Mathf.Clamp((dock.dockX-dock.slotX)*card.rect.width,-18*unit,18*unit);
                var rotation=Quaternion.Euler(0,0,dock.angle);
                var centre=hookLocal+(Vector2)(rotation*(Vector3)(new Vector2(spread,-(dock.kind==2?16:22)*unit)-gripLocal));
                dock.root.localPosition=new Vector3(centre.x,centre.y,dock.root.localPosition.z);
                dock.root.localRotation=Quaternion.Euler(0,0,dock.angle);
                dock.socket.position=new Vector3(hookWorld.x,hookWorld.y,dock.socket.position.z);
                dock.socket.sizeDelta=Vector2.one*10*unit;
                Vector2 Grip(Vector2 attachment) {
                    var point=card.InverseTransformPoint(dock.root.TransformPoint(dock.root.rect.min+Vector2.Scale(dock.root.rect.size,attachment)));
                    return new Vector2((point.x-card.rect.xMin)/card.rect.width,(point.y-card.rect.yMin)/height);
                }
                // The bomb hangs from its metal collar; its curved fuse is not a handle.
                var grip=Grip(attachment);
                dock.cord.SetEndpoints(Vector2.Scale(hook,card.rect.size),Vector2.Scale(grip,card.rect.size),ReducedMotion?0:machine.WindAt(dock.floor),moving?dt:0,machine.AmbientPhase+(.17f*dock.floor)+(.13f*dock.slotX),ReducedMotion);

            }
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
