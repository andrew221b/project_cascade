using DG.Tweening;
using UnityEngine;
namespace Cascade.Presentation
{
    // Readability is present on frame one; navigation never waits for the fade.
    public sealed class MenuTransition:MonoBehaviour
    {
        CanvasGroup group;CascadeGame game;RectTransform page;Vector2 rest;Tween arrival;float progress;bool fromRight,move;
        public void Initialize(CascadeGame owner,bool movePage=true){game=owner;move=movePage;page=(RectTransform)transform;rest=page.anchoredPosition;group=gameObject.AddComponent<CanvasGroup>();}
        public void Arrive(bool reverse=false)
        {
            Settle();if(game==null||game.ReducedMotion||!game.IntroductionAnimationAllowed)return;
            fromRight=reverse;progress=0;Paint();
            arrival=DOTween.To(()=>progress,value=>{progress=value;Paint();},1,.24f).SetEase(Ease.OutCubic).SetUpdate(true).SetTarget(this);
        }
        void Paint(){if(group==null||page==null)return;group.alpha=Mathf.Lerp(.70f,1,progress);if(move)page.anchoredPosition=rest+new Vector2((fromRight?1:-1)*(1-progress)*18,0);}
        void Update()
        {
            if(arrival==null||!arrival.IsActive())return;
            if(game.ReducedMotion){Settle();return;}
            if(!game.IntroductionAnimationAllowed)Settle();
        }
        void OnApplicationFocus(bool focused){if(!focused)Settle();}
        void OnApplicationPause(bool paused){if(paused)Settle();}
        void Settle(){arrival?.Kill();arrival=null;progress=1;if(group!=null)group.alpha=1;if(move&&page!=null)page.anchoredPosition=rest;}
        void OnDisable()=>Settle();
        void OnDestroy()=>Settle();
    }
}
