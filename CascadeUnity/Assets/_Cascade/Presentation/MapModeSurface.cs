using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // One cream surface, with a green enamel selection. No wooden surround or nested plaque.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MapModeSurface : MaskableGraphic
    {
        bool selected;
        public void Configure(bool active){selected=active;raycastTarget=false;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;
            float inset=selected?2:12;r=new Rect(r.x+inset,r.y+inset,r.width-2*inset,r.height-2*inset);
            // A soft edge and a small highlight keep the surface legible without a brown rim.
            Fill(mesh,r,selected?21:24,selected?new Color(.065f,.39f,.19f):new Color(.96f,.84f,.61f),selected?new Color(.02f,.27f,.13f):new Color(.96f,.83f,.58f));
            var inner=new Rect(r.x+2,r.y+2,r.width-4,r.height-4);
            Fill(mesh,inner,selected?19:22,selected?new Color(.17f,.61f,.28f):new Color(1,.95f,.79f),selected?new Color(.025f,.36f,.17f):new Color(1,.90f,.70f));
            if(!selected)for(int i=0;i<2;i++){
                float x=rectTransform.rect.xMin+201+164*i,y=r.center.y;
                int n=mesh.currentVertCount;var shade=new Color(.76f,.61f,.35f,.55f);
                mesh.AddVert(new Vector3(x,y-r.height*.28f),shade,Vector2.zero);mesh.AddVert(new Vector3(x,y+r.height*.28f),shade,Vector2.zero);
                mesh.AddVert(new Vector3(x+1.2f,y+r.height*.28f),shade,Vector2.zero);mesh.AddVert(new Vector3(x+1.2f,y-r.height*.28f),shade,Vector2.zero);
                mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
            }
        }
        static void Fill(VertexHelper mesh,Rect r,float radius,Color top,Color bottom)
        {
            radius=Mathf.Min(radius,r.height*.5f);int start=mesh.currentVertCount;
            mesh.AddVert(r.center,Color.Lerp(bottom,top,.5f),Vector2.zero);
            const int segments=8;
            for(int corner=0;corner<4;corner++)for(int step=0;step<=segments;step++){
                float angle=(corner*90+step*90f/segments)*Mathf.Deg2Rad;
                var centre=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);
                var point=centre+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                mesh.AddVert(point,Color.Lerp(bottom,top,Mathf.InverseLerp(r.yMin,r.yMax,point.y)),Vector2.zero);
            }
            int count=4*(segments+1);for(int i=0;i<count;i++)mesh.AddTriangle(start,start+1+i,start+1+(i+1)%count);
        }
    }
}
