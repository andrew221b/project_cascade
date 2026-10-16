using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Cascade.Presentation
{
    // Selection/navigation only: no puzzle command or inventory mutation is deferred to a tween.
    public sealed class MenuSwipe:MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IScrollHandler
    {
        Action<int> turn;Func<int,bool> canTurn;CascadeGame game;RectTransform moving;Canvas canvas;
        Vector2 start,rest;float began;Tween settle;CanvasGroup opacity;
        public bool IsSettling=>settle!=null&&settle.IsActive();
        public void Initialize(Action<int> callback){turn=callback;}
        public void Initialize(CascadeGame owner,Action<int> callback,RectTransform content,Func<int,bool> allowed)
        {
            game=owner;turn=callback;moving=content;canTurn=allowed;canvas=GetComponentInParent<Canvas>();rest=content.anchoredPosition;
            opacity=content.GetComponent<CanvasGroup>();if(opacity==null)opacity=content.gameObject.AddComponent<CanvasGroup>();
            if(owner.MenuPageDirection!=0&&!owner.ReducedMotion){moving.anchoredPosition=rest+Vector2.right*content.rect.width*.16f*owner.MenuPageDirection;opacity.alpha=.84f;Return();}
        }
        float LocalPixels=>moving==null?1:CenteredContentRow.Unit(moving)/Mathf.Max(.001f,canvas==null?1:canvas.scaleFactor);
        public void OnBeginDrag(PointerEventData e){settle?.Kill();start=e.pressPosition;began=Time.unscaledTime;e.eligibleForClick=false;}
        public void OnDrag(PointerEventData e)
        {
            e.eligibleForClick=false;if(moving==null||game==null||game.ReducedMotion)return;
            var delta=e.position-start;if(Mathf.Abs(delta.x)<Mathf.Abs(delta.y))return;
            int direction=delta.x<0?1:-1;float amount=delta.x*LocalPixels;
            float limit=moving.rect.width*.55f;float resistance=canTurn!=null&&!canTurn(direction)?.2f:1;
            moving.anchoredPosition=rest+Vector2.right*limit*(1-Mathf.Exp(-Mathf.Abs(amount)/limit))*Mathf.Sign(amount)*resistance;
        }
        public void OnEndDrag(PointerEventData e)
        {
            var delta=e.position-start;e.eligibleForClick=false;var parent=GetComponentInParent<Canvas>();
            float width=moving==null?((RectTransform)transform).rect.width*(parent==null?1:parent.scaleFactor):((RectTransform)transform).rect.width/Mathf.Max(.001f,LocalPixels);
            float threshold=Mathf.Clamp(width*.10f,42,80),speed=Mathf.Abs(delta.x)/Mathf.Max(.05f,Time.unscaledTime-began);
            if((Mathf.Abs(delta.x)>=threshold||Mathf.Abs(delta.x)>24&&speed>450)&&Mathf.Abs(delta.x)>Mathf.Abs(delta.y)*1.25f){
                int direction=delta.x<0?1:-1;if(canTurn==null||canTurn(direction)){Commit(direction);return;}
            }
            Return();
        }
        void Commit(int direction)
        {
            settle?.Kill();if(game==null){turn?.Invoke(direction);return;}
            game.Feedback.Sound(ToyCue.Select);game.Feedback.Haptic(.045f);game.TurnMenuPage(()=>turn?.Invoke(direction),direction);
        }
        void Return()
        {
            settle?.Kill();if(moving==null)return;
            if(game==null||game.ReducedMotion){moving.anchoredPosition=rest;if(opacity!=null)opacity.alpha=1;return;}
            var sequence=DOTween.Sequence().SetUpdate(true).SetTarget(this);
            sequence.Join(moving.DOAnchorPos(rest,.34f).SetEase(Ease.OutBack,.65f));
            if(opacity!=null)sequence.Join(opacity.DOFade(1,.18f).SetEase(Ease.OutCubic));settle=sequence;
        }
        public void OnScroll(PointerEventData e)
        {
            if(Mathf.Abs(e.scrollDelta.y)<.1f&&Mathf.Abs(e.scrollDelta.x)<.1f||IsSettling)return;
            int direction=e.scrollDelta.x!=0?(e.scrollDelta.x<0?1:-1):(e.scrollDelta.y<0?1:-1);
            if(canTurn==null||canTurn(direction))Commit(direction);
        }
        void OnApplicationFocus(bool focused){if(!focused)Reset();}
        void OnApplicationPause(bool paused){if(paused)Reset();}
        void Update(){if(game!=null&&(game.ReducedMotion||!game.IntroductionAnimationAllowed))Reset();}
        void Reset(){settle?.Kill();settle=null;if(moving!=null)moving.anchoredPosition=rest;if(opacity!=null)opacity.alpha=1;}
        void OnDisable()=>Reset();
        void OnDestroy(){settle?.Kill();}
    }
}
