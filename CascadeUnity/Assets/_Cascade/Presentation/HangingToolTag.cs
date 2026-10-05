using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // A light wooden sign has its own pendulum, driven by the same wind as its tool.
    public sealed class HangingToolTag : MonoBehaviour
    {
        RectTransform body,swing,plate;
        ToolTagCordGraphic cords;
        float angle,velocity;
        public float Angle=>angle;
        public void Initialize(RectTransform parent,RectTransform suspension,RectTransform sign,ToolTagCordGraphic rope)
        {body=parent;swing=suspension;plate=sign;cords=rope;Paint();}
        public void Tick(float seconds,float wind,float supportAngle,bool moving,bool reduced)
        {
            if(reduced){angle=velocity=0;}
            else if(moving){
                float dt=Mathf.Clamp(seconds,0,.05f);int steps=Mathf.Max(1,Mathf.CeilToInt(dt/.012f));float step=dt/steps;
                float target=wind*7.2f-supportAngle;
                for(int i=0;i<steps;i++){velocity+=(target-angle)*38*step;velocity*=Mathf.Exp(-4.6f*step);angle+=velocity*step;}
                angle=Mathf.Clamp(angle,-11,11);
            }
            Paint();
        }
        void Paint()
        {
            if(body==null)return;swing.localRotation=Quaternion.Euler(0,0,angle);
            Vector2 Hole(float x)=>(Vector2)body.InverseTransformPoint(plate.TransformPoint(plate.rect.min+Vector2.Scale(plate.rect.size,new Vector2(x,.93f))));
            var a=body.rect.min+Vector2.Scale(body.rect.size,new Vector2(.70f,.80f));
            var b=body.rect.min+Vector2.Scale(body.rect.size,new Vector2(.86f,.80f));
            cords.SetEndpoints(a,Hole(.22f),b,Hole(.78f));
        }
    }

    // Continuous shaded hemp, with feathered edges rather than stretched UI blocks.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ToolTagCordGraphic : MaskableGraphic
    {
        Vector2 a,b,c,d;
        public Vector3 EndWorld(int side)=>transform.TransformPoint(side==0?b:d);
        public void SetEndpoints(Vector2 from,Vector2 to,Vector2 otherFrom,Vector2 otherTo)
        {
            if(a==from&&b==to&&c==otherFrom&&d==otherTo)return;
            a=from;b=to;c=otherFrom;d=otherTo;raycastTarget=false;SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();Cord(h,a,b);Cord(h,c,d);
        }
        static void Cord(VertexHelper h,Vector2 top,Vector2 bottom)
        {
            if((bottom-top).sqrMagnitude<.01f)return;
            var along=(bottom-top).normalized;var across=new Vector2(-along.y,along.x);
            const int segments=12;
            for(int i=0;i<segments;i++){
                float p=i/(float)segments,q=(i+1)/(float)segments;
                var start=Vector2.Lerp(top,bottom,p);var end=Vector2.Lerp(top,bottom,q);
                Strip(h,start,end,across,-1.8f,-1.1f,new Color(.35f,.23f,.12f,0),new Color(.35f,.23f,.12f,1));
                Strip(h,start,end,across,-1.1f,0,new Color(.43f,.29f,.15f,1),new Color(.87f,.68f,.40f,1));
                Strip(h,start,end,across,0,1.1f,new Color(.87f,.68f,.40f,1),new Color(.48f,.32f,.16f,1));
                Strip(h,start,end,across,1.1f,1.8f,new Color(.35f,.23f,.12f,1),new Color(.35f,.23f,.12f,0));
            }
        }
        static void Strip(VertexHelper h,Vector2 start,Vector2 end,Vector2 normal,float left,float right,Color near,Color far)
        {
            int n=h.currentVertCount;h.AddVert(start+normal*left,near,Vector2.zero);h.AddVert(start+normal*right,far,Vector2.zero);
            h.AddVert(end+normal*right,far,Vector2.zero);h.AddVert(end+normal*left,near,Vector2.zero);h.AddTriangle(n,n+1,n+2);h.AddTriangle(n,n+2,n+3);
        }
    }
}
