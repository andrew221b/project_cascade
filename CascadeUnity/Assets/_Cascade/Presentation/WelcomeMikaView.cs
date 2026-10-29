using System.Collections.Generic;
using Cascade.Application;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Welcome-only trial. This component never receives pointer events or puzzle commands.
    [DefaultExecutionOrder(30)]
    public sealed class WelcomeMikaView:MonoBehaviour
    {
        const string ArtPath="CascadeArt/WelcomeMotionOct16/";
        static readonly int BlinkId=Shader.PropertyToID("_Blink"),GazeId=Shader.PropertyToID("_Gaze");
        CascadeGame game;Image hero;Material originalMaterial,face;WelcomeMikaMesh skin;MikaCosmeticView costume;
        readonly List<Image> visitors=new List<Image>();bool skyBound;float paintedAge=-1;bool paintedActive;Sprite paintedSprite;MenuWorldEnvironment environment;
        float paintedBlink=float.NaN;Vector2 paintedGaze=new Vector2(float.NaN,float.NaN);int paintedQuality=-1;
        Vector2 badgeBase,badgeApplied;bool badgeBound;
        WelcomeMikaWhiskers whiskers;WelcomeMikaScarf scarf;Material cloth;
        public WelcomeMikaMotion Motion {get;private set;}
        public Sprite SourceSprite {get;private set;}
        public bool SkinActive=>hero!=null&&hero.sprite==SourceSprite&&!game.ReducedMotion&&game.Preferences.EffectQuality>0;
        public bool UsesSourceArt=>hero!=null&&hero.sprite==SourceSprite;
        public int Quality=>game.Preferences.EffectQuality;
        public float AnimationAge=>Motion?.Age??0;
        public Vector2 Gaze=>Motion?.Gaze??Vector2.zero;
#if UNITY_EDITOR
        bool review;
        public void ResetReview(int seed=197){review=true;Motion.Reset(seed);Paint();}
        public void AdvanceReview(float seconds,float wind,Vector2 gaze,bool target=false){review=true;Motion.Advance(seconds,wind,gaze,target);Paint();}
        public void EndReview()=>review=false;
        public void SetReviewBlink(float value){face.SetFloat(BlinkId,value);paintedBlink=value;}
#endif
        public void Initialize(CascadeGame owner,Image image)
        {
            game=owner;hero=image;SourceSprite=AccountReferenceArt.Get("WelcomeMika");originalMaterial=image.material;
            Motion=new WelcomeMikaMotion(System.Guid.NewGuid().GetHashCode()&0x3fffffff);
            costume=image.GetComponent<MikaCosmeticView>();
            face=new Material(Resources.Load<Shader>(ArtPath+"WelcomeFace")){name="Mika welcome eyelids and gaze"};
            face.SetTexture("_HalfBlinkTex",Resources.Load<Texture2D>(ArtPath+"HalfBlink"));face.SetTexture("_ClosedBlinkTex",Resources.Load<Texture2D>(ArtPath+"ClosedBlink"));
            face.SetFloat("_SeparateScarf",1);cloth=new Material(face){name="Mika original loose scarf"};cloth.SetFloat("_ScarfLayer",1);
            skin=image.gameObject.AddComponent<WelcomeMikaMesh>();skin.Initialize(this);
            // Keep the dynamic illustration separate from the otherwise static menu canvas.
            var canvas=image.GetComponent<Canvas>();if(canvas==null)canvas=image.gameObject.AddComponent<Canvas>();canvas.overrideSorting=false;
            var fabric=new GameObject("Mika draped scarf",typeof(RectTransform),typeof(CanvasRenderer),typeof(WelcomeMikaScarf));
            var fabricRect=(RectTransform)fabric.transform;fabricRect.SetParent(image.transform,false);fabricRect.anchorMin=Vector2.zero;fabricRect.anchorMax=Vector2.one;fabricRect.offsetMin=fabricRect.offsetMax=Vector2.zero;
            scarf=fabric.GetComponent<WelcomeMikaScarf>();scarf.Initialize(this,cloth);
            var strands=new GameObject("Mika living whiskers",typeof(RectTransform),typeof(CanvasRenderer),typeof(WelcomeMikaWhiskers));
            var rect=(RectTransform)strands.transform;rect.SetParent(image.transform,false);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            whiskers=strands.GetComponent<WelcomeMikaWhiskers>();whiskers.Initialize(this);
            Paint();
        }
        void BindSky()
        {
            if(skyBound)return;skyBound=true;
            environment=game.GetComponentInChildren<MenuWorldEnvironment>();
            foreach(var image in game.GetComponentsInChildren<Image>()){
                bool bird=(image.name=="Side painted body"||image.name=="Rear painted body")&&image.transform.parent.name=="Distant bird";
                if(bird)visitors.Insert(0,image);
                else if(image.name.StartsWith("Distant sky aircraft ",System.StringComparison.Ordinal))visitors.Add(image);
            }
        }
        void LateUpdate()
        {
            if(game==null||Motion==null)return;
#if UNITY_EDITOR
            if(review){Paint();return;}
#endif
            BindSky();
            if(SkinActive&&game.CurrentScreen==GameScreen.AccountGate&&game.CosmeticAmbianceMotionAllowed){
                bool target=false;Vector2 gaze=Vector2.zero;
                for(int i=0;i<visitors.Count;i++){
                    var visitor=visitors[i];if(visitor==null||!visitor.enabled||!visitor.gameObject.activeInHierarchy||visitor.color.a<.15f)continue;
                    var p=(Vector2)hero.rectTransform.InverseTransformPoint(visitor.rectTransform.position);
                    if(Mathf.Abs(p.x)>hero.rectTransform.rect.width*1.5f)continue;
                    gaze=Vector2.ClampMagnitude(new Vector2(p.x/hero.rectTransform.rect.width,p.y/hero.rectTransform.rect.height-.23f),.85f);target=true;break;
                }
                Motion.Advance(Mathf.Min(Time.unscaledDeltaTime,.05f),WorkshopWind.Sample(environment!=null?environment.AnimationAge:game.Machine.AmbientPhase),gaze,target);
            }
            Paint();
        }
        void Paint()
        {
            if(hero==null||face==null)return;
            var material=hero.sprite==SourceSprite?face:originalMaterial;if(hero.material!=material)hero.material=material;
            bool active=SkinActive;float blink=active?Motion.Blink:0;var gaze=active?Motion.Gaze:Vector2.zero;
            if(paintedBlink!=blink){face.SetFloat(BlinkId,blink);paintedBlink=blink;}
            if(paintedGaze!=gaze){face.SetVector(GazeId,new Vector4(gaze.x,gaze.y,0,0));paintedGaze=gaze;}
            if(costume!=null&&costume.PreviewBadge!=null){
                var badge=costume.PreviewBadge;badge.material=originalMaterial;
                if(active&&badge.gameObject.activeSelf){
                    var area=hero.rectTransform.rect;var sprite=SourceSprite.rect;float scale=Mathf.Min(area.width/sprite.width,area.height/sprite.height);
                    var observed=badge.rectTransform.anchoredPosition;
                    if(!badgeBound||(observed-badgeApplied).sqrMagnitude>.000001f){badgeBase=observed;badgeBound=true;}
                    var centre=new Vector2(sprite.center.x,SourceSprite.texture.height-sprite.center.y);
                    var pixel=centre+new Vector2(badgeBase.x,-badgeBase.y)/Mathf.Max(.001f,scale);
                    var delta=Motion.Deform(pixel)-pixel;badgeApplied=badgeBase+new Vector2(delta.x,-delta.y)*scale;badge.rectTransform.anchoredPosition=badgeApplied;
                }
            }
            if(paintedAge!=Motion.Age||paintedActive!=active||paintedSprite!=hero.sprite||paintedQuality!=Quality){hero.SetVerticesDirty();if(whiskers!=null)whiskers.SetVerticesDirty();if(scarf!=null)scarf.SetVerticesDirty();paintedAge=Motion.Age;paintedActive=active;paintedSprite=hero.sprite;paintedQuality=Quality;}
        }
        void OnDestroy(){if(face!=null)Destroy(face);if(cloth!=null)Destroy(cloth);}
    }
}
