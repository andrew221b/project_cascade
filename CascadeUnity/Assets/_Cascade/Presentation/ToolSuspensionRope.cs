using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Cosmetic position constraints only. No Physics callbacks or gameplay references.
    public sealed class HangingPropBody
    {
        public Vector2 Position,Velocity,Anchor,Offset,HalfSize;
        public float Length,InverseMass=1;
        public float Angle{get;private set;}
        float sine,cosine=1;
        public Rect Bounds;
        public bool Ready;
        // Icon anchors span .83 widths by 1.24 * 55/88 heights. Its diagonal
        // bounds every swing angle; reserve the cord and timer before fitting art.
        public static float FitWidth(float requested,float deckGap,float unit,float timerSpace,float responseScale)
        {
            float envelope=Mathf.Sqrt(.83f*.83f+.775f*.775f)*responseScale;
            float span=Mathf.Max(0,deckGap-12*unit-timerSpace);
            float cord=Mathf.Min(span*.55f,Mathf.Clamp(span*.40f,18*unit,52*unit));
            return Mathf.Min(requested,Mathf.Max(0,span-cord)/envelope);
        }
        public Vector2 Center=>Position+new Vector2(Offset.x*cosine-Offset.y*sine,Offset.x*sine+Offset.y*cosine);
        public Vector2 Extent {
            get {float s=Mathf.Abs(sine),c=Mathf.Abs(cosine);return new Vector2(c*HalfSize.x+s*HalfSize.y,s*HalfSize.x+c*HalfSize.y);}
        }
        public void Initialize(int seed)
        {
            if(Ready)return;
            Position=Anchor+new Vector2((WildlifeMotion.Hash(seed)-.5f)*2,-Length);
            Velocity=Vector2.zero;Ready=true;
        }
        public void Integrate(float dt,Vector2 acceleration)
        {
            Velocity=(Velocity+acceleration*dt)*Mathf.Exp(-1.2f*dt);
            Position+=Velocity*dt;
        }
        public void Constrain()
        {
            var delta=Position-Anchor;float distance=delta.magnitude;
            if(distance>Length&&distance>.001f){
                var normal=delta/distance;Position=Anchor+normal*Length;
                float outward=Vector2.Dot(Velocity,normal);if(outward>0)Velocity-=normal*outward;
            }
            Angle=Mathf.Clamp(Mathf.Atan2(Position.x-Anchor.x,Anchor.y-Position.y)*Mathf.Rad2Deg,-55,55);
            sine=Mathf.Sin(Angle*Mathf.Deg2Rad);cosine=Mathf.Cos(Angle*Mathf.Deg2Rad);
            var center=Center;var extent=Extent;
            var fit=new Vector2(Mathf.Clamp(center.x,Bounds.xMin+extent.x,Bounds.xMax-extent.x),
                Mathf.Clamp(center.y,Bounds.yMin+extent.y,Bounds.yMax-extent.y));
            var correction=fit-center;Position+=correction;
            if(correction.x*Velocity.x<0)Velocity.x=0;if(correction.y*Velocity.y<0)Velocity.y=0;
        }
        public static bool Contact(HangingPropBody a,HangingPropBody b)
        {
            var delta=b.Center-a.Center;var span=a.Extent+b.Extent+Vector2.one*1.5f;
            float x=span.x-Mathf.Abs(delta.x),y=span.y-Mathf.Abs(delta.y);
            if(x<=0||y<=0)return false;
            // Separate along the least penetration axis; a tiny deterministic seed
            // at initialization breaks a shared-anchor symmetry without assigned docks.
            var normal=x<y?new Vector2(delta.x<0?-1:1,0):new Vector2(0,delta.y<0?-1:1);
            float sum=a.InverseMass+b.InverseMass,depth=Mathf.Min(x,y);
            a.Position-=normal*(depth*a.InverseMass/sum);b.Position+=normal*(depth*b.InverseMass/sum);
            float approach=Vector2.Dot(b.Velocity-a.Velocity,normal);
            if(approach<0){float impulse=-approach*1.12f/sum;a.Velocity-=normal*impulse*a.InverseMass;b.Velocity+=normal*impulse*b.InverseMass;}
            return true;
        }
    }

    // One tensioned hemp cord, with smooth edges; its endpoints follow the hanging body.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ToolSuspensionRope : MaskableGraphic
    {
        Vector2 top,bottom;
        readonly PresentationRope chain=new PresentationRope(9,83);
        public void SetEndpoints(Vector2 a,Vector2 b,float pressure,float seconds,float time,bool still){
            top=a+rectTransform.rect.min;bottom=b+rectTransform.rect.min;float unit=1/Mathf.Min(1,canvas.scaleFactor);
            chain.Advance(top,bottom,seconds,time,pressure,24*unit,14*unit,6*unit,still);SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();float unit=1/Mathf.Min(1,canvas.scaleFactor);
            Vector2 Point(float t)=>chain.Sample(t);
            for(int i=0;i<28;i++){
                float t=i/28f,next=(i+1)/28f;var a=Point(t);var b=Point(next);var delta=b-a;
                var across=new Vector2(-delta.y,delta.x).normalized*unit;
                Strip(a,b,-1.9f,-1.25f,new Color(.30f,.18f,.075f,0),new Color(.40f,.25f,.10f));
                Strip(a,b,-1.25f,-.25f,new Color(.40f,.25f,.10f),new Color(.86f,.68f,.40f));
                Strip(a,b,-.25f,1.15f,new Color(.86f,.68f,.40f),new Color(.51f,.33f,.15f));
                Strip(a,b,1.15f,1.9f,new Color(.51f,.33f,.15f),new Color(.30f,.18f,.075f,0));
                void Strip(Vector2 from,Vector2 to,float l,float r,Color left,Color right){
                    int n=mesh.currentVertCount;mesh.AddVert(from+across*l,left,Vector2.zero);mesh.AddVert(from+across*r,right,Vector2.zero);
                    mesh.AddVert(to+across*r,right,Vector2.zero);mesh.AddVert(to+across*l,left,Vector2.zero);mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
                }
            }
        }
    }
}
