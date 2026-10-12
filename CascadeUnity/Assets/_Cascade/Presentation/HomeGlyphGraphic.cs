using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Distinguishes returning to the menu from browsing the previous world.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class HomeGlyphGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();var rect=rectTransform.rect;float size=Mathf.Min(rect.width,rect.height);
            void Stroke(Vector2 a,Vector2 b,float thickness){a=rect.center+(a-Vector2.one*.5f)*size;b=rect.center+(b-Vector2.one*.5f)*size;
                var normal=new Vector2(-(b-a).y,(b-a).x).normalized*size*thickness*.5f;int first=helper.currentVertCount;
                helper.AddVert(a-normal,color,Vector2.zero);helper.AddVert(a+normal,color,Vector2.zero);helper.AddVert(b+normal,color,Vector2.zero);helper.AddVert(b-normal,color,Vector2.zero);
                helper.AddTriangle(first,first+1,first+2);helper.AddTriangle(first,first+2,first+3);}
            Stroke(new Vector2(.12f,.51f),new Vector2(.5f,.86f),.10f);Stroke(new Vector2(.5f,.86f),new Vector2(.88f,.51f),.10f);
            Stroke(new Vector2(.23f,.55f),new Vector2(.23f,.19f),.09f);Stroke(new Vector2(.77f,.55f),new Vector2(.77f,.19f),.09f);
            Stroke(new Vector2(.20f,.19f),new Vector2(.43f,.19f),.09f);Stroke(new Vector2(.57f,.19f),new Vector2(.80f,.19f),.09f);
            Stroke(new Vector2(.43f,.19f),new Vector2(.43f,.42f),.075f);Stroke(new Vector2(.57f,.19f),new Vector2(.57f,.42f),.075f);Stroke(new Vector2(.40f,.42f),new Vector2(.60f,.42f),.075f);
        }
    }
}
