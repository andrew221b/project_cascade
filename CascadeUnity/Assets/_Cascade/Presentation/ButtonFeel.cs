using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed class ButtonFeel:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler
    {
        CascadeGame game;Button button;bool pressed;float size=1,entrance=1;Outline focus;Tween response;
        public void Initialize(CascadeGame owner){game=owner;button=GetComponent<Button>();focus=gameObject.AddComponent<Outline>();focus.effectColor=new Color(.20f,.37f,.34f,.75f);focus.effectDistance=new Vector2(1.2f,-1.2f);focus.enabled=false;}
        public void OnPointerDown(PointerEventData e){if(button!=null&&button.IsInteractable()){pressed=true;Animate(true);}}
        public void OnPointerUp(PointerEventData e){if(pressed){pressed=false;Animate(false);}}
        public void OnPointerExit(PointerEventData e){if(pressed){pressed=false;Animate(false);}}
        public void OnSelect(BaseEventData e){if(focus!=null)focus.enabled=button!=null&&button.IsInteractable();}
        public void OnDeselect(BaseEventData e){if(focus!=null)focus.enabled=false;}
        void Animate(bool down)
        {
            response?.Kill();if(game==null||game.ReducedMotion){Settle();return;}
            // Animate only the artwork, keeping the button's hit area and layout fixed.
            response=DOTween.To(()=>size,Paint,down?.965f:1,down?.085f:.32f).SetEase(down?Ease.OutCubic:Ease.OutBack).SetUpdate(true).SetTarget(this);
        }
        internal void SetEntrance(float value){entrance=value;Paint(size);}
        void Paint(float value){size=value;foreach(Transform child in transform)child.localScale=Vector3.one*(size*entrance);}
        void Update()
        {
            if(game==null)return;
            if(game.ReducedMotion||button==null||!button.IsInteractable()||!game.IntroductionAnimationAllowed){Settle();return;}
            if(response!=null&&response.IsActive())response.Play();
        }
        void Settle(){response?.Kill();response=null;pressed=false;Paint(1);}
        void OnDisable(){entrance=1;Settle();if(focus!=null)focus.enabled=false;}
        void OnDestroy(){response?.Kill();}
    }
}
