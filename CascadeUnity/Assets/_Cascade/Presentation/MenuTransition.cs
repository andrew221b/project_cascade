using UnityEngine;
namespace Cascade.Presentation
{
    // Readability is present on frame one; navigation never waits for the fade.
    public sealed class MenuTransition:MonoBehaviour
    {
        CanvasGroup group;CascadeGame game;float elapsed=1;
        public void Initialize(CascadeGame owner){game=owner;group=gameObject.AddComponent<CanvasGroup>();}
        public void Arrive(){elapsed=0;if(group!=null)group.alpha=game.ReducedMotion?1:.72f;}
        void Update(){if(group==null||elapsed>=.16f)return;elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/.16f);group.alpha=game.ReducedMotion?1:Mathf.Lerp(.72f,1,1-Mathf.Pow(1-t,3));}
        void OnDisable(){elapsed=1;if(group!=null)group.alpha=1;}
    }
}
