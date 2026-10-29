using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Fine source-space strands remain readable after the illustration is reduced
    // to phone size. Roots follow the rigid muzzle, tips fan during short sniffs.
    public sealed class WelcomeMikaWhiskers:MaskableGraphic
    {
        const int Segments=12,Strands=6;
        readonly Vector2[] source=new Vector2[Strands*(Segments+1)];
        readonly UIVertex[] quad=new UIVertex[4];
        WelcomeMikaView owner;
        public void Initialize(WelcomeMikaView view)
        {
            owner=view;raycastTarget=false;color=new Color(1,.94f,.80f,.82f);
            Strand(0,new Vector2(549,403),new Vector2(440,387),new Vector2(365,392),new Vector2(320,409));
            Strand(1,new Vector2(549,413),new Vector2(449,421),new Vector2(359,443),new Vector2(310,471));
            Strand(2,new Vector2(549,423),new Vector2(450,459),new Vector2(375,501),new Vector2(342,535));
            Strand(3,new Vector2(691,363),new Vector2(754,309),new Vector2(825,269),new Vector2(867,245));
            Strand(4,new Vector2(691,375),new Vector2(763,331),new Vector2(837,298),new Vector2(880,291));
            Strand(5,new Vector2(691,386),new Vector2(766,354),new Vector2(830,333),new Vector2(881,331));
            SetVerticesDirty();
        }
        void Strand(int index,Vector2 a,Vector2 b,Vector2 c,Vector2 d)
        {
            for(int i=0;i<=Segments;i++){
                float t=i/(float)Segments,u=1-t;
                source[index*(Segments+1)+i]=a*(u*u*u)+b*(3*u*u*t)+c*(3*u*t*t)+d*(t*t*t);
            }
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();if(owner==null||!owner.SkinActive)return;
            var sprite=owner.SourceSprite;var area=rectTransform.rect;
            float scale=Mathf.Min(area.width/sprite.rect.width,area.height/sprite.rect.height);
            var centre=new Vector2(sprite.rect.center.x,sprite.texture.height-sprite.rect.center.y);
            for(int strand=0;strand<Strands;strand++){
                bool near=strand<3;int first=strand*(Segments+1);
                var previous=owner.Motion.WhiskerPoint(source[first],near);
                for(int i=1;i<=Segments;i++){
                    var next=owner.Motion.WhiskerPoint(source[first+i],near);
                    var tangent=(next-previous).normalized;var normal=new Vector2(-tangent.y,tangent.x);
                    float widthA=Mathf.Lerp(3.2f,1.4f,(i-1)/(float)Segments),widthB=Mathf.Lerp(3.2f,1.4f,i/(float)Segments);
                    // Coverage fades to zero on both edges. A hard subpixel quad
                    // turns into sparkling dots on a single-sample phone canvas.
                    for(int side=-1;side<=1;side+=2){
                        var a=previous;var b=previous+normal*(widthA*side);var c=next+normal*(widthB*side);var d=next;
                        for(int j=0;j<4;j++){
                            var p=j==0?a:j==1?b:j==2?c:d;var vertex=UIVertex.simpleVert;var tint=color;if(j==1||j==2)tint.a=0;
                            vertex.position=area.center+Vector2.Scale(p-centre,new Vector2(scale,-scale));vertex.color=tint;quad[j]=vertex;
                        }
                        mesh.AddUIVertexQuad(quad);
                    }
                    previous=next;
                }
            }
        }
#if UNITY_EDITOR
        public void BuildReviewMesh(VertexHelper mesh)=>OnPopulateMesh(mesh);
#endif
    }
}
