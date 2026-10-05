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
                floorLabels[i].rectTransform.anchoredPosition=Vector2.down*(machine.FloorSag(i)/Mathf.Max(.0001f,deviceOverlays[i].lossyScale.y)*.65f);
                floorLabels[i].alpha=machine.FloorBreakProgress(i)>0?0:1;
            }
            float dt=Mathf.Clamp(seconds,0,.05f);
            bool moving=!Interaction.IsPaused&&!Interaction.IsSuspended;
            bool movingDevice(DeviceDock d)=>moving&&d.responseTime>0;
            foreach(var dock in deviceDocks) {
                if(dock.root==null||!dock.root.gameObject.activeInHierarchy)continue;
                if(movingDevice(dock)){dock.responseTime=Mathf.Max(0,dock.responseTime-dt);}
                float response=ReducedMotion?0:Mathf.Sin((1-dock.responseTime/.32f)*Mathf.PI)*dock.responseTime/.32f;
                dock.icon.transform.localScale=Vector3.one*(1+.10f*response);
                var card=deviceOverlays[dock.floor];float height=Mathf.Max(1,card.rect.height);
                var hookWorld=machine.SurfacePoint(dock.floor,dock.slotX,.32f);
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
                    dock.angle=Mathf.Clamp(dock.angle,-7,7);
                }
                else if(!moving){dock.anchorReady=false;}
                // Rotate the whole object about the deck attachment, not about its icon centre.
                var rest=card.rect.min+Vector2.Scale(card.rect.size,new Vector2(dock.dockX,-.075f));
                var rotation=Quaternion.Euler(0,0,dock.angle);
                var centre=hookLocal+(Vector2)(rotation*(Vector3)(rest-(card.rect.min+Vector2.Scale(card.rect.size,new Vector2(dock.slotX,.32f)))));
                dock.root.anchoredPosition=centre-rest;
                dock.root.localRotation=Quaternion.Euler(0,0,dock.angle);
                dock.socket.anchoredPosition=Vector2.Scale(card.rect.size,hook-new Vector2(dock.slotX,.32f));
                Vector2 Grip(Vector2 attachment) {
                    var point=card.InverseTransformPoint(dock.root.TransformPoint(dock.root.rect.min+Vector2.Scale(dock.root.rect.size,attachment)));
                    return new Vector2((point.x-card.rect.xMin)/card.rect.width,(point.y-card.rect.yMin)/height);
                }
                // The bomb hangs from its metal collar; its curved fuse is not a handle.
                dock.leftStrap?.SetEndpoints(hook+Vector2.left*.018f,Grip(dock.kind==2?new Vector2(.24f,.745f):new Vector2(.17f,.88f)));
                dock.rightStrap?.SetEndpoints(hook+Vector2.right*.018f,Grip(dock.kind==2?new Vector2(.37f,.715f):new Vector2(.40f,.88f)));
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
