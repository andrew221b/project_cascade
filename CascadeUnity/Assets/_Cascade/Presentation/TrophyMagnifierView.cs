using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Crops the real hero image and its actual pin; no substitute skin is generated.
    [DefaultExecutionOrder(100)]
    public sealed class TrophyMagnifierView:MonoBehaviour
    {
        Image source,detail,pin;MikaCosmeticView worn;RectTransform stage,line,marker,window;
        readonly Vector3[] corners=new Vector3[4];
        public const float Zoom=3.6f;
        public void Initialize(Image hero,MikaCosmeticView costume,Image image,Image badge,RectTransform parent,RectTransform connector,RectTransform halo)
        {source=hero;worn=costume;detail=image;pin=badge;stage=parent;line=connector;marker=halo;window=(RectTransform)transform;Refresh();}
        void LateUpdate()=>Refresh();
        void Refresh()
        {
            var badge=worn!=null?worn.PreviewBadge:null;
            if(source==null||badge==null||!badge.gameObject.activeSelf)return;
            Vector2 offset=(Vector2)source.rectTransform.InverseTransformPoint(badge.rectTransform.position)-source.rectTransform.rect.center;
            detail.sprite=source.sprite;detail.color=source.color;detail.material=source.material;
            detail.rectTransform.sizeDelta=source.rectTransform.rect.size*Zoom;detail.rectTransform.anchoredPosition=-offset*Zoom;
            pin.sprite=badge.sprite;pin.color=badge.color;pin.material=badge.material;pin.preserveAspect=badge.preserveAspect;
            pin.rectTransform.sizeDelta=badge.rectTransform.rect.size*Zoom;pin.rectTransform.anchoredPosition=offset*Zoom;
            badge.rectTransform.GetWorldCorners(corners);
            float diameter=Vector2.Distance(stage.InverseTransformPoint(corners[0]),stage.InverseTransformPoint(corners[1]))+8;
            marker.position=badge.rectTransform.position;marker.sizeDelta=Vector2.one*diameter;
            Vector2 start=stage.InverseTransformPoint(badge.rectTransform.position),centre=stage.InverseTransformPoint(window.TransformPoint(window.rect.center));
            Vector2 end=centre+(start-centre).normalized*(window.rect.width*.5f),delta=end-start;
            line.position=badge.rectTransform.position;line.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);line.sizeDelta=new Vector2(delta.magnitude,2.5f);
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TrophyLensGraphic:MaskableGraphic
    {
        bool ring;float thickness;
        public void Configure(bool hollow,float width){ring=hollow;thickness=width;color=Color.white;raycastTarget=false;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();Rect r=GetPixelAdjustedRect();Vector2 centre=r.center;float outer=Mathf.Min(r.width,r.height)*.5f,inner=Mathf.Max(0,outer-thickness);const int segments=96;
            for(int i=0;i<segments;i++){
                float a=i*Mathf.PI*2/segments,b=(i+1)*Mathf.PI*2/segments;
                Vector2 v=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),w=new Vector2(Mathf.Cos(b),Mathf.Sin(b));int index=mesh.currentVertCount;
                mesh.AddVert(centre+v*outer,color,Vector2.zero);mesh.AddVert(centre+w*outer,color,Vector2.zero);
                mesh.AddVert(ring?centre+w*inner:centre,color,Vector2.zero);mesh.AddTriangle(index,index+1,index+2);
                if(ring){mesh.AddVert(centre+v*inner,color,Vector2.zero);mesh.AddTriangle(index,index+2,index+3);}
            }
        }
    }
}
