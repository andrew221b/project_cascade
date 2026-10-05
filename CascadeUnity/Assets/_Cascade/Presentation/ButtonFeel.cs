using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed class ButtonFeel:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
    {
        CascadeGame game;Button button;bool pressed;float size=1;
        public void Initialize(CascadeGame owner){game=owner;button=GetComponent<Button>();}
        public void OnPointerDown(PointerEventData e){pressed=button!=null&&button.IsInteractable();}
        public void OnPointerUp(PointerEventData e){pressed=false;}
        public void OnPointerExit(PointerEventData e){pressed=false;}
        void LateUpdate(){if(game==null)return;float target=pressed?.955f:1;size=game.ReducedMotion?1:Mathf.Lerp(size,target,1-Mathf.Exp(-24*Time.unscaledDeltaTime));transform.localScale=Vector3.one*size;}
        void OnDisable(){pressed=false;size=1;transform.localScale=Vector3.one;}
    }
}
