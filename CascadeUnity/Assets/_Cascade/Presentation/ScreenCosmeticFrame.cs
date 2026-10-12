using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Cosmetic edge ornaments never cover symbols, lifetime counters or touch targets.
    public sealed class ScreenCosmeticFrame:MonoBehaviour
    {
        readonly Image[] corners=new Image[4];CascadeGame game;string style;
        float clock;bool shown,focused=true;Canvas canvas;
        public int VisibleCount=>shown?4:0;
        public void Configure(CascadeGame owner,string id,bool gameplay)
        {
            game=owner;if(canvas==null)canvas=GetComponentInParent<Canvas>();style=id;shown=gameplay&&!string.IsNullOrEmpty(id);
            var sprite=shown?game.CosmeticFrameArt(id,true):null;
            for(int i=0;i<4;i++){
                if(corners[i]==null){
                    var go=new GameObject("Game frame corner "+i,typeof(RectTransform),typeof(Image));var r=(RectTransform)go.transform;r.SetParent(transform,false);
                    r.anchorMin=r.anchorMax=new Vector2(i%2==0?.015f:.985f,i<2?.985f:.015f);
                    r.pivot=new Vector2(i%2==0?0:1,i<2?1:0);
                    r.localScale=new Vector3(i%2==0?1:-1,i<2?1:-1,1);
                    corners[i]=go.GetComponent<Image>();corners[i].raycastTarget=false;corners[i].preserveAspect=true;
                }
                corners[i].enabled=shown&&sprite!=null;
                if(corners[i].sprite!=sprite)corners[i].sprite=sprite;
            }
            Paint();
        }
        void LateUpdate(){if(game==null||!shown)return;if(focused&&game.CosmeticAmbianceMotionAllowed&&!game.ReducedMotion)clock+=Mathf.Min(Time.unscaledDeltaTime,.08f);Paint();}
        void Paint(){if(game==null)return;float unit=canvas!=null?1/Mathf.Min(1,canvas.scaleFactor):1;for(int i=0;i<4;i++)if(corners[i]!=null){corners[i].rectTransform.sizeDelta=new Vector2(18,20)*unit;corners[i].canvasRenderer.SetAlpha(game.ReducedMotion?.92f:.92f+.06f*Mathf.Sin(clock*.9f+i));}}
        void OnApplicationFocus(bool value){focused=value;}
    }
}
