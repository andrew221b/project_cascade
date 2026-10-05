using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed class ButtonFeel:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler
    {
        CascadeGame game;Button button;bool pressed;float size=1;Outline focus;
        public void Initialize(CascadeGame owner){game=owner;button=GetComponent<Button>();focus=gameObject.AddComponent<Outline>();focus.effectColor=new Color(.20f,.37f,.34f,.75f);focus.effectDistance=new Vector2(1.2f,-1.2f);focus.enabled=false;}
        public void OnPointerDown(PointerEventData e){pressed=button!=null&&button.IsInteractable();}
        public void OnPointerUp(PointerEventData e){pressed=false;}
        public void OnPointerExit(PointerEventData e){pressed=false;}
        public void OnSelect(BaseEventData e){if(focus!=null)focus.enabled=button!=null&&button.IsInteractable();}
        public void OnDeselect(BaseEventData e){if(focus!=null)focus.enabled=false;}
        void LateUpdate(){if(game==null)return;float target=pressed?.978f:1;size=game.ReducedMotion?1:Mathf.Lerp(size,target,1-Mathf.Exp(-(pressed?36:22)*Time.unscaledDeltaTime));transform.localScale=Vector3.one*size;}
        void OnDisable(){pressed=false;if(focus!=null)focus.enabled=false;size=1;transform.localScale=Vector3.one;}
    }
}
