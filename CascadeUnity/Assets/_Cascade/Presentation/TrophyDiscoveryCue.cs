using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // A first-visit menu hint. It animates only its own image and never buys or equips.
    public sealed class TrophyDiscoveryCue : MonoBehaviour
    {
        CascadeGame game;
        RectTransform hand,shelf,mika;
        Image image;
        float clock;

        public void Initialize(CascadeGame owner,RectTransform pointer,RectTransform item,RectTransform portrait)
        {
            game=owner;hand=pointer;shelf=item;mika=portrait;image=pointer.GetComponent<Image>();
        }

        void LateUpdate()
        {
            if(game==null||hand==null||shelf==null||mika==null)return;
            if(!game.ReducedMotion)clock+=Mathf.Min(Time.unscaledDeltaTime,.08f);
            float phase=Mathf.Repeat(clock,3.4f),travel=Mathf.SmoothStep(0,1,Mathf.Clamp01((phase-.60f)/.95f));
            if(phase>2.55f)travel=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((phase-2.55f)/.65f));
            // The pivot is the authored fingertip, and both endpoints are visible image centres.
            var shelfCentre=shelf.TransformPoint(shelf.rect.center);
            var mikaCentre=mika.TransformPoint(mika.rect.center+new Vector2(0,mika.rect.height*.12f));
            hand.position=Vector3.Lerp(shelfCentre,mikaCentre,game.ReducedMotion?1:travel);
            hand.localScale=Vector3.one*(game.ReducedMotion?1:1+.08f*Mathf.Sin(clock*3.5f));
            image.color=new Color(1,1,1,game.ReducedMotion?.8f:Mathf.SmoothStep(.45f,.95f,Mathf.Min(phase/.3f,(3.4f-phase)/.3f)));
        }
    }
}
