using UnityEngine;using UnityEngine.UI;
namespace Cascade.Presentation{
    [RequireComponent(typeof(CanvasRenderer))]public sealed class RoundHudBadge:MaskableGraphic{
        protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;float radius=Mathf.Min(r.width,r.height)*.48f;for(int ring=0;ring<2;ring++){float size=radius*(ring==0?1:.91f);int n=vh.currentVertCount;vh.AddVert(r.center,ring==0?new Color(.08f,.22f,.12f):new Color(.09f,.52f,.22f),Vector2.zero);for(int i=0;i<=40;i++){float a=i*Mathf.PI/20;var p=r.center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*size;var c=ring==0?new Color(.07f,.22f,.12f):Color.Lerp(new Color(.035f,.29f,.10f),new Color(.22f,.73f,.36f),Mathf.InverseLerp(r.center.y-size,r.center.y+size,p.y));vh.AddVert(p,c,Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);}}}
    }
}
