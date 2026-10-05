using UnityEngine;
namespace Cascade.Presentation
{
    // Native crisp mechanical diagram lines; no gameplay state or generated illustration.
    public sealed class DemoLine:MonoBehaviour
    {
        RectTransform root,line;Vector2 start,end;float thickness;Vector2 lastSize;
        public void Initialize(RectTransform parent,Vector2 a,Vector2 b,float width){root=parent;line=(RectTransform)transform;start=a;end=b;thickness=width;line.anchorMin=line.anchorMax=(a+b)*.5f;line.anchoredPosition=Vector2.zero;Paint();}
        void LateUpdate(){if(root.rect.size!=lastSize)Paint();}
        void Paint(){lastSize=root.rect.size;var delta=Vector2.Scale(end-start,lastSize);line.sizeDelta=new Vector2(delta.magnitude,thickness);line.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);}
    }
}
