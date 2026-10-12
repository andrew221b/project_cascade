using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cascade.Presentation
{
    // A gesture changes the displayed shelf only; child level buttons lose their click on drag.
    public sealed class MenuSwipe:MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IScrollHandler
    {
        Action<int> turn;
        Vector2 start;
        public void Initialize(Action<int> callback){turn=callback;}
        public void OnBeginDrag(PointerEventData e){start=e.pressPosition;e.eligibleForClick=false;}
        public void OnDrag(PointerEventData e){e.eligibleForClick=false;}
        public void OnEndDrag(PointerEventData e)
        {
            var delta=e.position-start;e.eligibleForClick=false;
            var canvas=GetComponentInParent<Canvas>();float width=((RectTransform)transform).rect.width*(canvas==null?1:canvas.scaleFactor);
            if(Mathf.Abs(delta.x)>=Mathf.Clamp(width*.10f,42,80)&&Mathf.Abs(delta.x)>Mathf.Abs(delta.y)*1.25f)turn?.Invoke(delta.x<0?1:-1);
        }
        public void OnScroll(PointerEventData e){if(Mathf.Abs(e.scrollDelta.y)>.1f||Mathf.Abs(e.scrollDelta.x)>.1f)turn?.Invoke(e.scrollDelta.x!=0?(e.scrollDelta.x<0?1:-1):(e.scrollDelta.y<0?1:-1));}
    }
}
