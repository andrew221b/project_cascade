using UnityEngine;

namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class HintMark : UnityEngine.UI.MaskableGraphic
    {
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;float size=Mathf.Min(r.width,r.height);
            Vector2 At(float x,float y)=>r.center+new Vector2(x-.5f,y-.5f)*size;
            void Stroke(Vector2 a,Vector2 b,float weight){var p=Vector2.Perpendicular(b-a).normalized*size*weight*.5f;Quad(a-p,a+p,b+p,b-p);}
            void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d){int n=mesh.currentVertCount;mesh.AddVert(a,color,Vector2.zero);mesh.AddVert(b,color,Vector2.zero);mesh.AddVert(c,color,Vector2.zero);mesh.AddVert(d,color,Vector2.zero);mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);}
            Vector2 centre=At(.5f,.57f);
            for(int i=0;i<40;i++){
                float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20;
                Quad(centre,centre+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*size*.14f,centre+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*size*.14f,centre);
            }
            Quad(At(.41f,.48f),At(.59f,.48f),At(.565f,.35f),At(.435f,.35f));
            Stroke(At(.435f,.30f),At(.565f,.30f),.045f);
            Stroke(At(.45f,.24f),At(.55f,.24f),.045f);
            for(int i=0;i<5;i++){
                float a=Mathf.PI*i/4;var v=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                Stroke(centre+v*size*.20f,centre+v*size*.27f,.035f);
            }
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class InstructionCloseMark : UnityEngine.UI.MaskableGraphic
    {
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;float size=Mathf.Min(r.width,r.height),half=size*.13f;
            for(int i=0;i<2;i++){
                var v=new Vector2(half,i==0?half:-half);var p=Vector2.Perpendicular(v).normalized*size*.026f;
                int n=mesh.currentVertCount;
                mesh.AddVert(r.center-v-p,color,Vector2.zero);mesh.AddVert(r.center-v+p,color,Vector2.zero);
                mesh.AddVert(r.center+v+p,color,Vector2.zero);mesh.AddVert(r.center+v-p,color,Vector2.zero);
                mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
            }
        }
    }
}
