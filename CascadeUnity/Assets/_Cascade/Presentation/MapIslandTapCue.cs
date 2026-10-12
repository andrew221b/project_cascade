using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
namespace Cascade.Presentation
{
    // Cosmetic cue follows the live island anchor; only OpenJourneyIsland records discovery.
    public sealed class MapIslandTapCue : MonoBehaviour
    {
        CascadeGame game;RectTransform[] targets;int[] islands;Image image;SoftSceneryGraphic scenery;
        int preferred=-1,first;float clock;bool discovered;
        public void Initialize(CascadeGame owner,RectTransform[] anchors,int[] islandIds,SoftSceneryGraphic view,int current){
            game=owner;targets=anchors;islands=islandIds;scenery=view;preferred=current;image=GetComponent<Image>();
            first=System.Array.IndexOf(islands,current);if(first<0)first=0;
            discovered=owner.IslandTapDiscovered;image.enabled=false;
        }
        void LateUpdate(){
            if(game==null||targets==null||targets.Length==0||image==null)return;
            bool input=Mouse.current?.leftButton.isPressed==true||Touchscreen.current?.primaryTouch.press.isPressed==true||Keyboard.current?.anyKey.isPressed==true;
            if(input||game.ConversationOpen||!game.IntroductionAnimationAllowed||!game.Preferences.Tutorials||game.ReducedMotion){
                clock=0;image.enabled=false;scenery?.SetFocus(preferred);return;
            }
            clock+=Mathf.Min(Time.unscaledDeltaTime,.08f);
            float wait=discovered?9:3,elapsed=clock-wait;
            if(elapsed<0){image.enabled=false;return;}
            int cycle=Mathf.FloorToInt(elapsed/18);float age=Mathf.Repeat(elapsed,18);
            bool visible=age<2.8f;image.enabled=visible;
            scenery?.SetFocus(visible?islands[(first+cycle)%targets.Length]:preferred);
            if(!visible)return;
            var target=targets[(first+cycle)%targets.Length];if(target==null||!target.gameObject.activeInHierarchy){image.enabled=false;return;}
            float arrival=1-Mathf.Pow(1-Mathf.Clamp01(age/.65f),3);
            float tap=age>.7f&&age<1.5f?Mathf.Sin((age-.7f)/.8f*Mathf.PI):0;
            var offset=new Vector2(target.rect.width*.24f,-target.rect.height*.28f)*(1-arrival)+Vector2.down*(2*tap);
            transform.position=target.TransformPoint(target.rect.center+offset);
            transform.localScale=Vector3.one*(1-.10f*tap);
            image.canvasRenderer.SetAlpha(Mathf.Min(Mathf.Clamp01(age/.18f),Mathf.Clamp01((2.8f-age)/.35f)));
        }
    }
}
