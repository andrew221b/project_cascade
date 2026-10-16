using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RepairKitMarkGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;
            void Stroke(Vector2 a,Vector2 b,float width,Color shade){
                a=new Vector2(r.xMin+a.x*r.width,r.yMin+a.y*r.height);b=new Vector2(r.xMin+b.x*r.width,r.yMin+b.y*r.height);
                var normal=new Vector2(-(b-a).y,(b-a).x).normalized*width*Mathf.Min(r.width,r.height);
                int first=mesh.currentVertCount;mesh.AddVert(a-normal,shade,Vector2.zero);mesh.AddVert(a+normal,shade,Vector2.zero);
                mesh.AddVert(b+normal,shade,Vector2.zero);mesh.AddVert(b-normal,shade,Vector2.zero);
                mesh.AddTriangle(first,first+1,first+2);mesh.AddTriangle(first,first+2,first+3);
            }
            var outline=new Color(.27f,.18f,.09f);var brass=new Color(.83f,.55f,.16f);
            void Tool(Color shade,float width){
                Stroke(new Vector2(.20f,.14f),new Vector2(.76f,.78f),width,shade);
                Stroke(new Vector2(.58f,.94f),new Vector2(.96f,.62f),width*1.6f,shade);
                Stroke(new Vector2(.80f,.14f),new Vector2(.28f,.72f),width,shade);
                Stroke(new Vector2(.28f,.72f),new Vector2(.08f,.80f),width,shade);
                Stroke(new Vector2(.28f,.72f),new Vector2(.29f,.96f),width,shade);
            }
            Tool(outline,.095f);Tool(brass,.052f);
        }
    }
}
