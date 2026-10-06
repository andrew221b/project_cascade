using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Finite, skippable presentation of an existing receipt. Never grants or advances anything.
    public sealed class RewardCelebration:MonoBehaviour
    {
        CascadeGame game;Image[] stars;int earned,score;TMP_Text number;float elapsed;bool gift;
        Image[] confetti;PaperBody[] paperBodies;Vector2[] starOrigins;int chimed=-1;Tween ceremony;RectTransform ribbon;CeremonyGlowGraphic glow;
        public float Progress=>Mathf.Clamp01(elapsed/3.2f);
        public void Initialize(CascadeGame owner,Image[] awardStars,int count,TMP_Text scoreLabel,int finalScore,Sprite papers,bool opening,RectTransform awardRibbon=null)
        {
            game=owner;stars=awardStars;earned=count;number=scoreLabel;score=finalScore;gift=opening;ribbon=awardRibbon;
            starOrigins=new Vector2[stars?.Length??0];for(int i=0;i<starOrigins.Length;i++)starOrigins[i]=stars[i].rectTransform.anchoredPosition;
            var light=new GameObject("Earned lantern light",typeof(RectTransform),typeof(CanvasRenderer),typeof(CeremonyGlowGraphic));light.transform.SetParent(transform,false);light.transform.SetSiblingIndex(0);
            var lightRect=(RectTransform)light.transform;lightRect.anchorMin=new Vector2(.03f,gift?.40f:.61f);lightRect.anchorMax=new Vector2(.97f,gift?.83f:.81f);lightRect.offsetMin=lightRect.offsetMax=Vector2.zero;glow=light.GetComponent<CeremonyGlowGraphic>();glow.raycastTarget=false;
            int pieces=owner.ReducedMotion||owner.Preferences.EffectQuality==0?0:owner.Preferences.EffectQuality==1?12:24;
            confetti=new Image[pieces];paperBodies=new PaperBody[pieces];var random=new System.Random(finalScore+count*31+(opening?419:73));
            for(int i=0;i<pieces;i++){
                var obj=new GameObject("Celebration paper "+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));obj.transform.SetParent(transform,false);
                var rect=(RectTransform)obj.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,gift?.62f:.73f);
                float size=18+(float)random.NextDouble()*16;rect.sizeDelta=new Vector2(size,size);
                confetti[i]=obj.GetComponent<Image>();confetti[i].sprite=owner.CelebrationPaper(i%6);confetti[i].preserveAspect=true;confetti[i].raycastTarget=false;
                paperBodies[i]=new PaperBody{position=new Vector2(((float)random.NextDouble()-.5f)*84,0),
                    velocity=new Vector2(((float)random.NextDouble()-.5f)*220,110+(float)random.NextDouble()*80),
                    phase=(float)random.NextDouble()*Mathf.PI*2,angle=(float)random.NextDouble()*360,
                    spin=((float)random.NextDouble()-.5f)*260,drag=.45f+(float)random.NextDouble()*.7f,
                    gravity=140+(float)random.NextDouble()*65,delay=(gift?.50f:.40f)+i*.015f};
                confetti[i].color=new Color(1,1,1,0);
            }
            Paint();
            ceremony=DOTween.To(()=>elapsed,value=>{elapsed=value;Paint();},3.2f,3.2f).SetEase(Ease.Linear).SetUpdate(UpdateType.Manual).SetTarget(this).Pause();
        }
        void Update()
        {
            if(game==null)return;
            if(game.ReducedMotion){if(elapsed<3.2f||ceremony!=null){ceremony?.Kill();ceremony=null;elapsed=3.2f;Paint();}return;}
            if(elapsed>=3.2f){ceremony?.Kill();ceremony=null;return;}
            if(ceremony==null||!ceremony.IsActive())return;
            if(game.CelebrationMotionAllowed){
                float dt=Mathf.Min(.05f,Time.unscaledDeltaTime);AdvancePaper(dt);ceremony.Goto(Mathf.Min(3.2f,elapsed+dt),false);
            }
        }
        void Paint()
        {
            if(game==null)return;
            bool quiet=game.ReducedMotion;
            if(ribbon!=null){float p=quiet?1:Mathf.Clamp01(elapsed/.62f),ease=1-Mathf.Pow(1-p,4);ribbon.localScale=new Vector3(Mathf.Lerp(.88f,1,ease),Mathf.Lerp(.72f,1,ease),1);ribbon.localRotation=Quaternion.Euler(0,0,quiet?0:Mathf.Sin(p*Mathf.PI)*1.2f);}
            if(glow!=null){float t=Mathf.Clamp01((elapsed-(gift?.50f:.24f))/2.0f);glow.SetLight(quiet||game.Preferences.EffectQuality==0?0:Mathf.Sin(t*Mathf.PI)*.65f,t);}
            if(stars!=null)for(int i=0;i<stars.Length;i++){
                if(stars[i]==null||i>=earned)continue;
                float t=quiet?1:Mathf.Clamp01((elapsed-.16f-i*.32f)/.55f),ease=1-Mathf.Pow(1-t,3);
                stars[i].color=new Color(1,1,1,quiet?1:Mathf.Lerp(.18f,1,ease));
                stars[i].rectTransform.localScale=Vector3.one*(quiet?1:Mathf.Lerp(.78f,1,ease)+.14f*Mathf.Sin(t*Mathf.PI));
                stars[i].rectTransform.anchoredPosition=starOrigins[i]+Vector2.down*(quiet?0:(1-ease)*14);
                stars[i].rectTransform.localRotation=Quaternion.Euler(0,0,quiet?0:(1-ease)*(i%2==0?-12:12));
                if(t>.3f&&i>chimed){chimed=i;game.Feedback.Sound(i==earned-1?ToyCue.Success:ToyCue.Stamp);}
            }
            if(number!=null){float delay=stars!=null&&stars.Length>3?1.95f:1.05f;float t=quiet?1:Mathf.Clamp01((elapsed-delay)/1.25f);number.text=Mathf.RoundToInt(score*(1-Mathf.Pow(1-t,3))).ToString("N0");}
            if(confetti!=null)for(int i=0;i<confetti.Length;i++){
                if(confetti[i]==null)continue;var body=paperBodies[i];float age=elapsed-body.delay;
                var rect=confetti[i].rectTransform;rect.anchoredPosition=body.position;
                rect.localRotation=Quaternion.Euler(0,0,body.angle);
                rect.localScale=new Vector3(.25f+.75f*Mathf.Abs(Mathf.Cos(age*4+body.phase)),1,1);
                float opacity=age<0?0:Mathf.SmoothStep(0,1,age/.12f)*(1-Mathf.SmoothStep(0,1,(age-1.75f)/.60f));
                confetti[i].color=new Color(1,1,1,quiet||game.Preferences.EffectQuality==0?0:opacity);
            }
        }
        sealed class PaperBody
        {
            public Vector2 position,velocity;
            public float angle,spin,phase,drag,gravity,delay;
        }
        void AdvancePaper(float dt)
        {
            for(int i=0;i<paperBodies.Length;i++){
                var b=paperBodies[i];float activeDt=Mathf.Clamp(elapsed+dt-b.delay,0,dt);if(activeDt<=0)continue;
                float wind=20*Mathf.Sin(elapsed*1.7f+b.phase)+12*Mathf.Sin(elapsed*4+b.phase*.7f);
                b.velocity+=new Vector2(wind,-b.gravity)*activeDt;b.velocity*=Mathf.Exp(-b.drag*activeDt);
                b.position+=b.velocity*activeDt;b.angle+=(b.spin+Mathf.Sin(elapsed*5+b.phase)*35)*activeDt;
            }
        }
        void OnDisable(){ceremony?.Kill();if(glow!=null)Destroy(glow.gameObject);foreach(var image in confetti??System.Array.Empty<Image>())if(image!=null)Destroy(image.gameObject);Destroy(this);}
        void OnDestroy(){ceremony?.Kill();}
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CeremonyGlowGraphic:MaskableGraphic
    {
        float strength,phase;
        public void SetLight(float value,float progress){strength=value;phase=progress;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();if(strength<=0)return;var r=rectTransform.rect;var center=r.center;
            for(int i=0;i<32;i++){
                float a=i*Mathf.PI*2/32,b=(i+1)*Mathf.PI*2/32;int v=h.currentVertCount;
                h.AddVert(center,new Color(1,.79f,.32f,strength*.24f),Vector2.zero);
                var tint=new Color(1,.88f,.55f,0);var radius=r.size*.5f*(.75f+.25f*phase);
                h.AddVert(center+Vector2.Scale(radius,new Vector2(Mathf.Cos(a),Mathf.Sin(a))),tint,Vector2.zero);
                h.AddVert(center+Vector2.Scale(radius,new Vector2(Mathf.Cos(b),Mathf.Sin(b))),tint,Vector2.zero);h.AddTriangle(v,v+1,v+2);
            }
        }
    }
}
