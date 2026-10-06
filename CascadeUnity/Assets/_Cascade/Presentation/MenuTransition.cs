using DG.Tweening;
using UnityEngine;
namespace Cascade.Presentation
{
    // Readability is present on frame one; navigation never waits for the fade.
    public sealed class MenuTransition:MonoBehaviour
    {
        CanvasGroup group;CascadeGame game;RectTransform page;Vector2 rest;Tween arrival;float progress;
        public void Initialize(CascadeGame owner){game=owner;page=(RectTransform)transform;rest=page.anchoredPosition;group=gameObject.AddComponent<CanvasGroup>();}
        public void Arrive(bool reverse=false)
        {
            Settle();if(game==null||game.ReducedMotion)return;
            progress=0;float shift=(reverse?-1:1)*Mathf.Min(24,page.rect.width*.06f);
            Paint(shift);
            arrival=DOTween.To(()=>progress,value=>{progress=value;Paint(shift);},1,.30f).SetEase(Ease.OutQuint).SetUpdate(true).SetTarget(this);
        }
        void Paint(float shift){if(group==null||page==null)return;group.alpha=Mathf.Lerp(.82f,1,progress);page.anchoredPosition=rest+Vector2.right*(shift*(1-progress));}
        void Update()
        {
            if(arrival==null||!arrival.IsActive())return;
            if(game.ReducedMotion){Settle();return;}
            if(game.IntroductionAnimationAllowed)arrival.Play();else arrival.Pause();
        }
        void Settle(){arrival?.Kill();arrival=null;progress=1;if(group!=null)group.alpha=1;if(page!=null)page.anchoredPosition=rest;}
        void OnDisable()=>Settle();
        void OnDestroy()=>Settle();
    }
}
