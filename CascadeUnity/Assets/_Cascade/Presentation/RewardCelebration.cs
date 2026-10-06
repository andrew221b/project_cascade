using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Finite, skippable presentation of an existing receipt. Never grants or advances anything.
    public sealed class RewardCelebration:MonoBehaviour
    {
        CascadeGame game;Image[] stars;int earned,score;TMP_Text number;float elapsed;bool gift;
        Image[] confetti;Vector2[] origin;int chimed=-1;
        public float Progress=>Mathf.Clamp01(elapsed/2.1f);
        public void Initialize(CascadeGame owner,Image[] awardStars,int count,TMP_Text scoreLabel,int finalScore,Sprite papers,bool opening)
        {
            game=owner;stars=awardStars;earned=count;number=scoreLabel;score=finalScore;gift=opening;
            int pieces=owner.ReducedMotion||owner.Preferences.EffectQuality==0?0:owner.Preferences.EffectQuality==1?2:4;
            confetti=new Image[pieces];origin=new Vector2[pieces];
            for(int i=0;i<pieces;i++){
                var obj=new GameObject("Celebration fabric "+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));obj.transform.SetParent(transform,false);
                var rect=(RectTransform)obj.transform;rect.anchorMin=rect.anchorMax=new Vector2(i%2==0?.13f:.87f,.79f-i/2*.11f);rect.sizeDelta=new Vector2(90,90);
                confetti[i]=obj.GetComponent<Image>();confetti[i].sprite=papers;confetti[i].preserveAspect=true;confetti[i].raycastTarget=false;
                origin[i]=rect.anchoredPosition;confetti[i].color=new Color(1,1,1,0);
            }
            Paint();
        }
        void Update()
        {
            if(game==null||elapsed>=3.2f)return;
            if(!game.CelebrationMotionAllowed)return;
            elapsed+=Time.unscaledDeltaTime;Paint();
        }
        void Paint()
        {
            if(game==null)return;
            bool quiet=game.ReducedMotion;
            if(stars!=null)for(int i=0;i<stars.Length;i++){
                if(stars[i]==null||i>=earned)continue;
                float t=quiet?1:Mathf.Clamp01((elapsed-.16f-i*.32f)/.55f),ease=1-Mathf.Pow(1-t,3);
                stars[i].color=new Color(1,1,1,quiet?1:Mathf.Lerp(.18f,1,ease));
                stars[i].rectTransform.localScale=Vector3.one*(quiet?1:Mathf.Lerp(.78f,1,ease)+.14f*Mathf.Sin(t*Mathf.PI));
                stars[i].rectTransform.localRotation=Quaternion.Euler(0,0,quiet?0:(1-ease)*(i%2==0?-12:12));
                if(t>.3f&&i>chimed){chimed=i;game.Feedback.Sound(i==earned-1?ToyCue.Success:ToyCue.Stamp);}
            }
            if(number!=null){float t=quiet?1:Mathf.Clamp01((elapsed-.3f)/1.25f);number.text=Mathf.RoundToInt(score*(1-Mathf.Pow(1-t,3))).ToString("N0");}
            if(confetti!=null)for(int i=0;i<confetti.Length;i++){
                if(confetti[i]==null)continue;
                float t=Mathf.Clamp01((elapsed-(gift?.15f:.58f)-i*.09f)/2.1f);var rect=confetti[i].rectTransform;
                rect.anchoredPosition=origin[i]+new Vector2((i%2==0?1:-1)*Mathf.Sin(t*Mathf.PI)*44,35*Mathf.Sin(t*Mathf.PI)-110*t*t);
                rect.localRotation=Quaternion.Euler(0,0,(i%2==0?1:-1)*30*t);confetti[i].color=new Color(1,1,1,Mathf.Sin(t*Mathf.PI)*.82f);
            }
        }
        void OnDisable(){foreach(var image in confetti??System.Array.Empty<Image>())if(image!=null)Destroy(image.gameObject);Destroy(this);}
    }
}
