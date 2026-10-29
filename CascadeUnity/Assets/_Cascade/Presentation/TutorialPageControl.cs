using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TutorialPageControl:MaskableGraphic,IPointerClickHandler
    {
        Action<int> select;public int Count{get;private set;}public int Selected{get;private set;}
        int Visible=>Mathf.Min(7,Count);int First=>Mathf.Clamp(Selected-3,0,Mathf.Max(0,Count-Visible));
        public void Initialize(int count,int selected,Action<int> callback){Count=count;Selected=selected;select=callback;raycastTarget=callback!=null;SetVerticesDirty();}
        public void OnPointerClick(PointerEventData e)
        {
            if(e.button!=PointerEventData.InputButton.Left||Count<2)return;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera??GetComponentInParent<Canvas>().rootCanvas.worldCamera,out var point)){
                float pitch=Mathf.Min(35,(rectTransform.rect.width-18)/Mathf.Max(1,Visible));
                int slot=Mathf.Clamp(Mathf.RoundToInt((point.x-rectTransform.rect.center.x)/pitch+(Visible-1)*.5f),0,Visible-1);select?.Invoke(First+slot);
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float pitch=Mathf.Min(35,(rectTransform.rect.width-18)/Mathf.Max(1,Visible));
            void Disk(Vector2 p,float radius,Color inner,Color outer){int start=vh.currentVertCount;vh.AddVert(p,inner,Vector2.zero);for(int i=0;i<=24;i++){float angle=i*Mathf.PI*2/24;vh.AddVert(p+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,outer,Vector2.zero);if(i>0)vh.AddTriangle(start,start+i,start+i+1);}}
            for(int i=0;i<Visible;i++){
                bool on=First+i==Selected;float size=(i==0&&First>0||i==Visible-1&&First+Visible<Count)?.72f:1;
                var c=rectTransform.rect.center+Vector2.right*((i-(Visible-1)*.5f)*pitch);
                Disk(c+Vector2.down,10*size,new Color(.32f,.12f,.025f),new Color(.32f,.12f,.025f));
                Disk(c,7.4f*size,on?new Color(1,.88f,.25f):new Color(1,.97f,.83f),on?new Color(1,.68f,.06f):new Color(.94f,.86f,.63f));
                Disk(c+new Vector2(-2,3)*size,2.3f*size,new Color(1,1,.8f,.8f),new Color(1,1,.8f,0));
            }
        }
    }
}
