using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class ToyMachineView
    {
        readonly List<Part> receiverParts=new List<Part>();
        readonly List<TextMeshProUGUI> receiverFaces=new List<TextMeshProUGUI>();
        readonly Part[] receiverKnots=new Part[2];
        readonly ReceiverSuspension receiverMotion=new ReceiverSuspension();
        ReceiverAttraction receiverMagic;
        Vector3 receiverPivot;
        public float ReceiverAngle=>receiverMotion.Roll;
        public Vector3 ReceiverVisualOffset=>new Vector3(receiverMotion.Sway,receiverMotion.Heave,0);
        public Vector3 ReceiverPointOffset(Vector3 point)
            =>ReceiverVisualOffset+Quaternion.Euler(0,0,ReceiverAngle)*(point-receiverPivot)-(point-receiverPivot);
        public Vector3 ReceiverLandingPosition(int index)
        {
            var target=targetBodies[index];
            var point=target.basePosition+target.baseRotation*Vector3.Scale(target.mesh.bounds.center,target.baseScale);
            point.z=restPositions[index].z;
            return point+ReceiverPointOffset(point)-Quaternion.Euler(0,0,ReceiverAngle)*Vector3.Scale(tokens[index].mesh.bounds.center,restScales[index]);
        }
        public Vector3 ReceiverTargetPosition(int index)=>targetBodies[index].renderer.bounds.center;
        void BindReceiver(IReadOnlyList<TextMeshProUGUI> targets)
        {
            receiverMotion.Reset();receiverParts.Clear();receiverFaces.Clear();
            foreach(var part in all)if(part.anchor==lowerAnchor||part.anchor.IsChildOf(lowerAnchor))receiverParts.Add(part);
            for(int side=0;side<2;side++){
                float x=side==0?0f:1f;
                receiverParts.Add(Add("Receiver brass rope eye",lowerAnchor,art.disk,art.gold,new Vector2(x,.56f),new Vector2(.055f,.17f),-.05f,.09f));
                receiverKnots[side]=Add("Receiver tied hemp knot",lowerAnchor,art.disk,ropeMaterial,new Vector2(x,.54f),new Vector2(.029f,.11f),-.13f,.09f);
                receiverParts.Add(receiverKnots[side]);
            }
            // Keep the layout anchors canonical. Only a child glyph face follows the body.
            foreach(var label in targets){
                var face=Instantiate(label,label.transform,false);face.name="Moving receiver glyph";
                var rect=face.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
                face.raycastTarget=false;label.enabled=false;receiverFaces.Add(face);
            }
            receiverMagic=new GameObject("Receiver woven light").AddComponent<ReceiverAttraction>();
            receiverMagic.transform.SetParent(machine,false);receiverMagic.Initialize();
        }
        void TickReceiver(float seconds)
        {
            lowerAnchor.GetWorldCorners(corners);receiverPivot=(corners[0]+corners[2])*.5f;receiverPivot.z=0;
            receiverMotion.Tick(seconds,WindAt(Mathf.Max(0,floors.Count-1)),reducedMotion);
            var rotation=Quaternion.Euler(0,0,ReceiverAngle);
            foreach(var part in receiverParts){part.transform.position=part.basePosition+ReceiverPointOffset(part.basePosition);part.transform.rotation=rotation*part.baseRotation;}
            for(int i=0;i<receiverFaces.Count;i++){
                var face=receiverFaces[i];var position=targetBodies[i].transform.position;position.z=face.rectTransform.position.z;
                face.rectTransform.SetPositionAndRotation(position,rotation);
            }
            PaintReceiverFrame();
        }
        public void PaintReceiverAttraction(float strength,float phase,bool accepted)
        {
            if(receiverMagic==null)return;
            receiverMagic.Paint(this,Mathf.Clamp01(strength),phase,accepted,reducedMotion);
        }
        public void ClearReceiverAttraction(){receiverMagic?.Clear();SetReceiverLight(0);}
    }

    // A heavier pinned tray: slower springs and smaller impulses than the timber decks.
    // Positions are cosmetic world offsets; no gameplay or tween-completion dependency.
    public sealed class ReceiverSuspension
    {
        public float Sway,Roll,Heave;
        float swayVelocity,rollVelocity,heaveVelocity;
        public void Reset(){Sway=Roll=Heave=swayVelocity=rollVelocity=heaveVelocity=0;}
        public void Impulse(float lateral,float downward=0){swayVelocity+=lateral*.28f;rollVelocity+=lateral*2.1f;heaveVelocity-=downward*.08f;}
        public void Tick(float seconds,float wind,bool reduced)
        {
            if(reduced){Reset();return;}
            float remaining=Mathf.Clamp(seconds,0,.05f);
            while(remaining>0){float dt=Mathf.Min(.008f,remaining);remaining-=dt;
                Step(ref Sway,ref swayVelocity,Mathf.Clamp(wind,-1,1)*.072f,11,5.2f,dt);
                Step(ref Roll,ref rollVelocity,Mathf.Clamp(wind,-1,1)*.62f,9,4.8f,dt);
                Step(ref Heave,ref heaveVelocity,0,30,8,dt);
            }
            Sway=Mathf.Clamp(Sway,-.14f,.14f);Roll=Mathf.Clamp(Roll,-1.2f,1.2f);Heave=Mathf.Clamp(Heave,-.035f,.02f);
        }
        static void Step(ref float position,ref float velocity,float target,float stiffness,float damping,float dt)
        {velocity+=((target-position)*stiffness-velocity*damping)*dt;position+=velocity*dt;}
    }
}
