using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CoachRingGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var r=GetPixelAdjustedRect(); var center=r.center; float outer=Mathf.Min(r.width,r.height)*.5f,inner=Mathf.Max(0,outer-2);
            const int segments=40;
            for(int i=0;i<=segments;i++) {
                float angle=i*Mathf.PI*2/segments;var direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                mesh.AddVert(center+direction*outer,color,Vector2.zero);mesh.AddVert(center+direction*inner,color,Vector2.zero);
                if(i>0){int n=i*2;mesh.AddTriangle(n-2,n,n-1);mesh.AddTriangle(n,n+1,n-1);}
            }
        }
    }
}
