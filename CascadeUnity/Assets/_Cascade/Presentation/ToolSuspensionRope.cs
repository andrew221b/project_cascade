using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
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
