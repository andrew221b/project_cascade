using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // The control keeps its Button name for focus/accessibility; no font icon dependency.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PauseMark : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var r=rectTransform.rect;
            for(int i=0;i<2;i++) {
                float x=r.xMin+r.width*(i==0?.35f:.56f), w=r.width*.10f;
                float y=r.yMin+r.height*.29f, h=r.height*.42f;
                int n=mesh.currentVertCount;var ink=color;
                mesh.AddVert(new Vector3(x,y),ink,Vector2.zero);mesh.AddVert(new Vector3(x,y+h),ink,Vector2.zero);
                mesh.AddVert(new Vector3(x+w,y+h),ink,Vector2.zero);mesh.AddVert(new Vector3(x+w,y),ink,Vector2.zero);
                mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
            }
        }
    }
}
