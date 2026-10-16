using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ShopCanopyGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;
            const int stripes=9,steps=8;
            for(int i=0;i<stripes;i++){
                var shade=i%2==0?new Color(.98f,.94f,.79f,.94f):new Color(.16f,.49f,.45f,.94f);
                float left=r.xMin+r.width*i/stripes,right=r.xMin+r.width*(i+1)/stripes;
                for(int n=0;n<steps;n++){
                    float a=n/(float)steps,b=(n+1)/(float)steps;
                    float bottomA=r.yMin+r.height*.22f*(1-Mathf.Sin(a*Mathf.PI));
                    float bottomB=r.yMin+r.height*.22f*(1-Mathf.Sin(b*Mathf.PI));
                    int start=mesh.currentVertCount;
                    mesh.AddVert(new Vector3(Mathf.Lerp(left,right,a),bottomA),shade,Vector2.zero);
                    mesh.AddVert(new Vector3(Mathf.Lerp(left,right,a),r.yMax),shade,Vector2.zero);
                    mesh.AddVert(new Vector3(Mathf.Lerp(left,right,b),r.yMax),shade,Vector2.zero);
                    mesh.AddVert(new Vector3(Mathf.Lerp(left,right,b),bottomB),shade,Vector2.zero);
                    mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
                }
            }
            int trim=mesh.currentVertCount;var brass=new Color(.77f,.49f,.17f,.94f);
            mesh.AddVert(new Vector3(r.xMin,r.yMax-r.height*.09f),brass,Vector2.zero);mesh.AddVert(new Vector3(r.xMin,r.yMax),brass,Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax,r.yMax),brass,Vector2.zero);mesh.AddVert(new Vector3(r.xMax,r.yMax-r.height*.09f),brass,Vector2.zero);
            mesh.AddTriangle(trim,trim+1,trim+2);mesh.AddTriangle(trim,trim+2,trim+3);
        }
    }
}
