using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TutorialPawGraphic:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;var shade=new Color(.37f,.16f,.045f);float unit=Mathf.Min(r.width,r.height);
            void Pad(float x,float y,float w,float h){var center=r.center+new Vector2(x,y)*unit;int n=vh.currentVertCount;vh.AddVert(center,shade,Vector2.zero);for(int i=0;i<=18;i++){float a=i*Mathf.PI*2/18;vh.AddVert(center+new Vector2(Mathf.Cos(a)*w,Mathf.Sin(a)*h)*unit,shade,Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);}}
            Pad(0,-.18f,.24f,.21f);Pad(-.32f,.02f,.10f,.15f);Pad(-.13f,.26f,.105f,.15f);Pad(.12f,.26f,.105f,.15f);Pad(.31f,.02f,.10f,.15f);
        }
    }
}
