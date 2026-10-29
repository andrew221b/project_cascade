using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Monochrome native UI marks match the reference without font-dependent glyphs.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SubscriptionGlyphGraphic:MaskableGraphic
    {
        public enum Shape { Check,Restore,Manage }
        public Shape Symbol;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;float size=Mathf.Min(r.width,r.height);
            Vector2 P(float x,float y)=>r.center+(new Vector2(x,y)-Vector2.one*.5f)*size;
            void Triangle(Vector2 a,Vector2 b,Vector2 c){int n=mesh.currentVertCount;mesh.AddVert(a,color,Vector2.zero);mesh.AddVert(b,color,Vector2.zero);mesh.AddVert(c,color,Vector2.zero);mesh.AddTriangle(n,n+1,n+2);}
            void Line(Vector2 a,Vector2 b,float weight){var v=new Vector2(-(b-a).y,(b-a).x).normalized*size*weight*.5f;Triangle(a-v,a+v,b+v);Triangle(a-v,b+v,b-v);}
            void Disc(float x,float y,float radius){var c=P(x,y);for(int i=0;i<28;i++){float a=i*Mathf.PI*2/28,b=(i+1)*Mathf.PI*2/28;Triangle(c,c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius*size,c+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius*size);}}
            void Arc(float start,float end){for(int i=0;i<24;i++){float a=Mathf.Lerp(start,end,i/24f)*Mathf.Deg2Rad,b=Mathf.Lerp(start,end,(i+1)/24f)*Mathf.Deg2Rad;Line(P(.5f+Mathf.Cos(a)*.33f,.5f+Mathf.Sin(a)*.33f),P(.5f+Mathf.Cos(b)*.33f,.5f+Mathf.Sin(b)*.33f),.13f);}}
            if(Symbol==Shape.Check){Line(P(.15f,.47f),P(.41f,.22f),.13f);Line(P(.41f,.22f),P(.86f,.8f),.13f);}
            else if(Symbol==Shape.Restore){Arc(24,162);Arc(204,342);Triangle(P(.09f,.72f),P(.08f,.37f),P(.4f,.62f));Triangle(P(.91f,.28f),P(.92f,.63f),P(.6f,.38f));}
            else{
                Disc(.34f,.76f,.15f);Disc(.32f,.31f,.22f);
                for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(P(.74f+Mathf.Cos(a)*.1f,.32f+Mathf.Sin(a)*.1f),P(.74f+Mathf.Cos(a)*.24f,.32f+Mathf.Sin(a)*.24f),.10f);}
                for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;Line(P(.74f+Mathf.Cos(a)*.14f,.32f+Mathf.Sin(a)*.14f),P(.74f+Mathf.Cos(b)*.14f,.32f+Mathf.Sin(b)*.14f),.08f);}
            }
        }
    }
}
