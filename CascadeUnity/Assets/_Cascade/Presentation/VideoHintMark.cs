using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class VideoHintMark:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;float s=Mathf.Min(r.width,r.height);Vector2 P(float x,float y)=>r.center+new Vector2(x-.5f,y-.5f)*s;
            void Quad(float x,float y,float w,float h){int n=vh.currentVertCount;vh.AddVert(P(x,y),color,Vector2.zero);vh.AddVert(P(x,y+h),color,Vector2.zero);vh.AddVert(P(x+w,y+h),color,Vector2.zero);vh.AddVert(P(x+w,y),color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
            Quad(.17f,.23f,.66f,.055f);Quad(.17f,.71f,.66f,.055f);Quad(.17f,.28f,.055f,.43f);Quad(.775f,.28f,.055f,.43f);
            for(int i=0;i<4;i++){Quad(.24f+i*.145f,.30f,.070f,.050f);Quad(.24f+i*.145f,.64f,.070f,.050f);}
            int k=vh.currentVertCount;vh.AddVert(P(.43f,.38f),color,Vector2.zero);vh.AddVert(P(.43f,.63f),color,Vector2.zero);vh.AddVert(P(.65f,.505f),color,Vector2.zero);vh.AddTriangle(k,k+1,k+2);
        }
    }
}
