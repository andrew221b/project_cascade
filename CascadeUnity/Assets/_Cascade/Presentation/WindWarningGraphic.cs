using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Three soft air trails point toward the impending canonical shift; never a timer.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WindWarningGraphic : MaskableGraphic
    {
        public bool Right { get; set; }
        float phase;
        bool still;
        public void Paint(float time,bool reduced){phase=time;still=reduced;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;
            for(int trail=0;trail<3;trail++){
                float start=still?.1f:Mathf.Repeat(phase*.44f+trail*.16f,.34f);
                Vector2 Point(float t){float x=start+t*.56f;return new Vector2(r.xMin+r.width*(Right?x:1-x),r.yMin+r.height*(.20f+trail*.28f+Mathf.Sin(t*Mathf.PI)*.065f));}
                for(int i=0;i<32;i++){
                    float t=i/32f,next=(i+1)/32f;var a=Point(t);var b=Point(next);
                    float width=r.height*.016f*Mathf.Sin((.10f+t*.85f)*Mathf.PI);
                    var n=new Vector2(-(b-a).y,(b-a).x).normalized*width;
                    var shade=new Color(1,.99f,.87f,(.30f+.24f*trail/2)*Mathf.Sin((.05f+t*.90f)*Mathf.PI));int k=mesh.currentVertCount;
                    mesh.AddVert(a-n,shade,Vector2.zero);mesh.AddVert(a+n,shade,Vector2.zero);mesh.AddVert(b+n,shade,Vector2.zero);mesh.AddVert(b-n,shade,Vector2.zero);
                    mesh.AddTriangle(k,k+1,k+2);mesh.AddTriangle(k,k+2,k+3);
                }
                var tip=Point(1);float sign=Right?1:-1;int v=mesh.currentVertCount;var ink=new Color(1,.97f,.82f,.8f);
                mesh.AddVert(tip+Vector2.right*sign*r.height*.12f,ink,Vector2.zero);
                mesh.AddVert(tip+new Vector2(-sign*.055f,.09f)*r.height,ink,Vector2.zero);
                mesh.AddVert(tip+new Vector2(-sign*.055f,-.09f)*r.height,ink,Vector2.zero);mesh.AddTriangle(v,v+1,v+2);
            }
        }
    }
}
