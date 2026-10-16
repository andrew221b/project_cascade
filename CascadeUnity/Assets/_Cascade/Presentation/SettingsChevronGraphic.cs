using UnityEngine;

namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SettingsChevronGraphic:UnityEngine.UI.MaskableGraphic
    {
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;float radius=r.width*.19f;
            var a=r.center+new Vector2(-r.width*.25f,r.height*.35f);
            var b=r.center+new Vector2(r.width*.25f,0);
            var c=r.center+new Vector2(-r.width*.25f,-r.height*.35f);
            Segment(mesh,a,b,radius);Segment(mesh,b,c,radius);Disc(mesh,a,radius);Disc(mesh,b,radius);Disc(mesh,c,radius);
        }
        void Segment(UnityEngine.UI.VertexHelper mesh,Vector2 a,Vector2 b,float radius)
        {
            var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*radius;int start=mesh.currentVertCount;
            mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(a+n,color,Vector2.zero);mesh.AddVert(b+n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
        }
        void Disc(UnityEngine.UI.VertexHelper mesh,Vector2 center,float radius)
        {
            int start=mesh.currentVertCount;mesh.AddVert(center,color,Vector2.zero);
            for(int i=0;i<=12;i++){float angle=i*Mathf.PI/6;mesh.AddVert(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,color,Vector2.zero);}
            for(int i=0;i<12;i++)mesh.AddTriangle(start,start+i+1,start+i+2);
        }
    }
}
