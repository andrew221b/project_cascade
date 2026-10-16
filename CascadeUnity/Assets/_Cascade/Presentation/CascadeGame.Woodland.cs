using System.Collections.Generic;
using Cascade.Domain;
using TMPro;
using UnityEngine;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        sealed class ContactFace {public int floor,slot;public TextMeshProUGUI label;}
        readonly List<ContactFace> contactFaces=new List<ContactFace>();
        readonly Vector3[] suspensionCorners=new Vector3[4];
        float suspensionRemainder;
        void LateUpdate()
        {
            PaintShopShortfall();
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
                // The body performs the action: turn the saw, press a stamp, unfurl a spool.
                float turn=dock.kind==3?6*Mathf.Sin(response*10):dock.kind==9?-12*response:dock.kind==10?8*response:0;
                dock.icon.transform.localRotation=Quaternion.Euler(0,0,turn);
                if(dock.kind==8)dock.icon.transform.localScale=new Vector3(1+.13f*response,1-.12f*response,1);
                if(dock.kind==4)dock.icon.transform.localScale=new Vector3(1-.06f*response,1+.12f*response,1);
                var card=deviceOverlays[dock.floor];
                // Move only the cosmetic center hook clear of the engraved strength numeral.
                float hookX=Mathf.Abs(dock.slotX-.5f)<.045f?.5f+(dock.slotX<=.5f?-.052f:.052f):dock.slotX;
                var hookWorld=machine.SurfacePoint(dock.floor,hookX,.35f);
                var hookLocal=(Vector2)card.InverseTransformPoint(hookWorld);
                // A visible length of cord, limited by the next deck or receiver caption.
                float unit=1/Mathf.Min(1,dock.root.GetComponentInParent<Canvas>().scaleFactor);
                float bodyWidth=dock.dockGap==0?88:Mathf.Min(88,card.rect.width*dock.dockGap*.94f);
                var attachment=dock.kind==2?new Vector2(.365f,.69f):dock.kind==3?new Vector2(.50f,1.055f):dock.kind==4?new Vector2(.47f,.78f):dock.kind==8?new Vector2(.68f,1.02f):dock.kind==9?new Vector2(.46f,1.01f):new Vector2(.465f,1.02f);
                float obstacleY;
                if(dock.floor+1<floorViews.Count){
                    // The next deck can tilt against this deck while the prop swings sideways.
                    // Protect its whole upper edge, including an arriving/selected lower row.
                    int below=dock.floor+1;obstacleY=float.NegativeInfinity;
                    for(int side=0;side<3;side++)obstacleY=Mathf.Max(obstacleY,card.InverseTransformPoint(machine.SurfacePoint(below,side*.5f,.445f)).y);
                    if(VisibleFrame.Status==PuzzleStatus.Playing&&VisibleFrame.ActiveFloor==below)
                        for(int token=0;token<machine.TokenCount;token++){
                            var bounds=machine.TokenVisualBounds(token);
                            obstacleY=Mathf.Max(obstacleY,card.InverseTransformPoint(new Vector3(bounds.min.x,bounds.max.y,bounds.center.z)).y);
                            obstacleY=Mathf.Max(obstacleY,card.InverseTransformPoint(new Vector3(bounds.max.x,bounds.max.y,bounds.center.z)).y);
                        }
                }
                else {
                    receiverLabel.rectTransform.GetWorldCorners(suspensionCorners);
                    obstacleY=float.NegativeInfinity;
                    foreach(var corner in suspensionCorners)obstacleY=Mathf.Max(obstacleY,card.InverseTransformPoint(corner).y);
                }
                // Keep the readable, upright timer outside the rotating rigid body.
                // A dense four/five-deck layout must fit the ART too, not just shorten
                // the rope and feed an impossible rectangle to the contact solver.
                float timerSpace=dock.countdown!=null?26*unit:0;
                float lowerClearance=12*unit+timerSpace;
                float responseScale=Mathf.Max(dock.icon.transform.localScale.x,dock.icon.transform.localScale.y);
                bodyWidth=HangingPropBody.FitWidth(bodyWidth,hookLocal.y-obstacleY,unit,timerSpace,responseScale);
                var bodySize=new Vector2(bodyWidth,55*bodyWidth/88);
                if(dock.root.sizeDelta!=bodySize)dock.root.sizeDelta=bodySize;
                var gripLocal=dock.root.rect.min+Vector2.Scale(dock.root.rect.size,attachment);
                var rotation=Quaternion.identity;
                float belowGrip=float.PositiveInfinity;var low=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var high=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
                void IncludeBody(RectTransform body) {
                    body.GetWorldCorners(suspensionCorners);
                    foreach(var corner in suspensionCorners){
                        var relative=dock.root.InverseTransformPoint(corner)-(Vector3)gripLocal;
                        belowGrip=Mathf.Min(belowGrip,(rotation*relative).y);
                        low=Vector2.Min(low,(Vector2)relative);high=Vector2.Max(high,(Vector2)relative);
                    }
                }
                IncludeBody(dock.icon.rectTransform);
                float available=Mathf.Max(0,hookLocal.y+belowGrip-obstacleY-lowerClearance);
                // Measure the actual attachment-to-silhouette distance as well as
                // its diagonal estimate, including the sprite's actual hanging eye.
                float span=Mathf.Max(0,hookLocal.y-obstacleY-lowerClearance);
                float visibleCord=Mathf.Min(span*.48f,Mathf.Clamp(span*.4f,22*unit,52*unit));
                if(available<visibleCord&&belowGrip<-.01f){
                    float fit=Mathf.Clamp01((span-visibleCord)/-belowGrip);
                    dock.root.sizeDelta*=fit;
                    gripLocal=dock.root.rect.min+Vector2.Scale(dock.root.rect.size,attachment);
                    belowGrip=float.PositiveInfinity;low=new Vector2(float.PositiveInfinity,float.PositiveInfinity);high=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
                    IncludeBody(dock.icon.rectTransform);
                    available=Mathf.Max(0,hookLocal.y+belowGrip-obstacleY-lowerClearance);
                }
                dock.unit=unit;dock.gripLocal=gripLocal;
                var body=dock.body;
                if(Vector2.Distance(dock.layoutSize,card.rect.size)>.25f){dock.layoutSize=card.rect.size;body.Ready=false;dock.anchorReady=false;}
                body.Anchor=hookLocal;
                body.Length=Mathf.Min(available,(118+(dock.neighbors-1)*16+(dock.kind%3-1)*6)*unit);
                body.InverseMass=dock.kind==2?.65f:dock.kind==4?.8f:1.15f;
                body.Offset=(low+high)*.5f;
                body.HalfSize=(high-low)*.5f+Vector2.one*(1.5f*unit);
                body.Bounds=UnityEngine.Rect.MinMaxRect(card.rect.xMin+8*unit,obstacleY+lowerClearance,card.rect.xMax-8*unit,hookLocal.y-2*unit);
                bool fresh=!body.Ready;body.Initialize(dock.kind*137+dock.floor*59);
                if(fresh){body.Velocity=Vector2.zero;dock.supportVelocity=dock.supportAcceleration=Vector2.zero;}
                else if(moving&&!ReducedMotion&&dt>.0001f){
                    var supportVelocity=(Vector2)card.InverseTransformVector(hookWorld-dock.lastHookWorld)/dt;
                    var acceleration=Vector2.ClampMagnitude((supportVelocity-dock.supportVelocity)/dt,1200*unit);
                    dock.supportAcceleration=Vector2.Lerp(dock.supportAcceleration,acceleration,1-Mathf.Exp(-dt*12));
                    dock.supportVelocity=supportVelocity;
                }else dock.supportAcceleration=dock.supportVelocity=Vector2.zero;
                dock.lastHookWorld=hookWorld;
            }
            // Fixed, bounded substeps and pooled bodies keep this independent of FPS.
            // First paint settles the shared hooks before exposing the props.
            int settleSteps=0;
            foreach(var dock in deviceDocks)if(dock.body.Ready&&!dock.anchorReady){settleSteps=100;dock.anchorReady=true;}
            const float step=1f/120;
            if(settleSteps>0||!moving||ReducedMotion)suspensionRemainder=0;else suspensionRemainder+=dt;
            int steps=settleSteps>0?settleSteps:Mathf.FloorToInt(suspensionRemainder/step);
            if(settleSteps==0)suspensionRemainder-=steps*step;
            for(int sub=0;sub<steps;sub++) {
                foreach(var dock in deviceDocks)if(dock.root!=null&&dock.root.gameObject.activeInHierarchy){
                    var card=deviceOverlays[dock.floor];
                    var down=(Vector2)(Quaternion.Inverse(card.localRotation)*Vector3.down);
                    var acceleration=down*(650*dock.unit);
                    if(settleSteps==0){acceleration-=dock.supportAcceleration*.65f;acceleration.x+=machine.WindAt(dock.floor)*200*dock.unit*dock.body.InverseMass;}
                    dock.body.Integrate(step,acceleration);
                }
                for(int iteration=0;iteration<(settleSteps>0?40:18);iteration++) {
                    foreach(var dock in deviceDocks)if(dock.root!=null&&dock.root.gameObject.activeInHierarchy)dock.body.Constrain();
                    for(int i=0;i<deviceDocks.Count;i++)for(int j=i+1;j<deviceDocks.Count;j++){
                        var a=deviceDocks[i];var b=deviceDocks[j];
                        if(a.floor==b.floor&&a.root.gameObject.activeInHierarchy&&b.root.gameObject.activeInHierarchy)HangingPropBody.Contact(a.body,b.body);
                    }
                }
            }
            foreach(var dock in deviceDocks) {
                if(dock.root==null||!dock.root.gameObject.activeInHierarchy)continue;
                var body=dock.body;var card=deviceOverlays[dock.floor];
                // A final pair contact may have pushed a prop towards the next deck.
                body.Constrain();
                if(settleSteps>0)body.Velocity=Vector2.zero;
                dock.angle=body.Angle;
                var rotation=Quaternion.Euler(0,0,dock.angle);
                var centre=body.Position-(Vector2)(rotation*(Vector3)dock.gripLocal);
                dock.root.localPosition=new Vector3(centre.x,centre.y,dock.root.localPosition.z);
                dock.root.localRotation=rotation;
                if(dock.countdown!=null){
                    float unit=dock.unit;
                    var timer=dock.countdown.rectTransform;
                    timer.sizeDelta=new Vector2(104*unit,20*unit);
                    dock.icon.rectTransform.GetWorldCorners(suspensionCorners);
                    float bottom=float.PositiveInfinity;var middle=Vector3.zero;
                    foreach(var corner in suspensionCorners){bottom=Mathf.Min(bottom,corner.y);middle+=corner*.25f;}
                    // Position and rotation are both independent of the bomb's swing.
                    timer.rotation=Quaternion.identity;
                    card.GetWorldCorners(suspensionCorners);
                    float left=float.PositiveInfinity,right=float.NegativeInfinity;
                    foreach(var corner in suspensionCorners){left=Mathf.Min(left,corner.x);right=Mathf.Max(right,corner.x);}
                    float halfWidth=timer.rect.width*timer.lossyScale.x*.5f;
                    timer.position=new Vector3(Mathf.Clamp(middle.x,left+halfWidth,right-halfWidth),bottom-12*unit*timer.lossyScale.y,timer.position.z);
                }
                dock.socket.localPosition=new Vector3(body.Anchor.x,body.Anchor.y,dock.socket.localPosition.z);
                dock.socket.sizeDelta=Vector2.one*10*dock.unit;
                dock.cord.SetEndpoints(body.Anchor-card.rect.min,body.Position-card.rect.min,ReducedMotion?0:machine.WindAt(dock.floor),moving&&!ReducedMotion?dt:0,
                    machine.AmbientPhase+(.17f*dock.floor)+(.13f*dock.slotX),ReducedMotion);

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
