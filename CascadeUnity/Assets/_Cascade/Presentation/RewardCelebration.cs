using DG.Tweening;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Finite, skippable presentation of an existing receipt. Never grants or advances anything.
    public sealed class RewardCelebration:MonoBehaviour
    {
        CascadeGame game;Image[] stars;Sprite[] starStates;float earned;int score;TMP_Text number;VictoryScoreDigits scoreDigits;float elapsed;bool gift;
        Image[] confetti;PaperBody[] paperBodies;Vector2[] starOrigins,starLaunch;float[] starSizes;int chimed=-1;Tween ceremony;RectTransform ribbon;CeremonyGlowGraphic glow;
        RectTransform[] metrics;Image[] starFills;AwardGlintGraphic[] starGlints;Counter[] counters;
        // The result needs a readable pause: hollow stars arrive first, then fill,
        // then the score and rewards roll up. Parcel reveal remains shorter.
        const float ArrivalStart=.28f,ArrivalStep=.34f,ArrivalDuration=.74f,FillStart=1.92f,FillStep=.52f,FillDuration=.78f,CountStart=2.18f,CountDuration=2.30f;
        float Duration=>gift?3.65f:4.85f;
#if UNITY_EDITOR
        bool previewHeld;
#endif
        public float Progress=>Mathf.Clamp01(elapsed/Duration);
        public void Initialize(CascadeGame owner,Image[] awardStars,float count,TMP_Text scoreLabel,int finalScore,Sprite papers,bool opening,RectTransform awardRibbon=null,Sprite[] awardStarStates=null,VictoryScoreDigits customScore=null)
        {
            game=owner;stars=awardStars;starStates=awardStarStates;earned=Mathf.Clamp(count,0,awardStars?.Length??0);number=scoreLabel;scoreDigits=customScore;score=finalScore;gift=opening;ribbon=awardRibbon;
            Canvas.ForceUpdateCanvases();starOrigins=new Vector2[stars?.Length??0];starLaunch=new Vector2[starOrigins.Length];starSizes=new float[starOrigins.Length];starFills=new Image[starOrigins.Length];starGlints=new AwardGlintGraphic[starOrigins.Length];
            var area=((RectTransform)transform).rect;
            for(int i=0;i<starOrigins.Length;i++){
                var rect=stars[i].rectTransform;starOrigins[i]=rect.anchoredPosition;
                starLaunch[i]=gift?new Vector2(area.center.x,area.yMin+area.height*.62f)-(Vector2)rect.localPosition:
                    new Vector2((i%2==0?area.xMin-rect.rect.width:area.xMax+rect.rect.width)-rect.localPosition.x,-rect.rect.height*.32f);
                starSizes[i]=gift?Mathf.Clamp(area.width*.78f/Mathf.Max(1,rect.rect.width),1,6):.65f;
                if(starStates?.Length>=3){
                    stars[i].sprite=starStates[2];
                    var fillObject=new GameObject("Star colour fill "+(i+1),typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));fillObject.transform.SetParent(rect,false);
                    var fillRect=(RectTransform)fillObject.transform;fillRect.anchorMin=Vector2.zero;fillRect.anchorMax=Vector2.one;fillRect.offsetMin=fillRect.offsetMax=Vector2.zero;
                    var fill=starFills[i]=fillObject.GetComponent<Image>();fill.sprite=starStates[0];fill.preserveAspect=true;fill.raycastTarget=false;
                    fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillOrigin=0;fill.fillAmount=0;
                }
                var layer=rect.gameObject.AddComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=20;
                var shine=new GameObject("Star glint",typeof(RectTransform),typeof(CanvasRenderer),typeof(AwardGlintGraphic));shine.transform.SetParent(rect,false);
                var sr=(RectTransform)shine.transform;sr.anchorMin=Vector2.zero;sr.anchorMax=Vector2.one;sr.offsetMin=sr.offsetMax=Vector2.zero;
                starGlints[i]=shine.GetComponent<AwardGlintGraphic>();starGlints[i].Initialize(owner,i);starGlints[i].enabled=false;shine.GetComponent<Graphic>().raycastTarget=false;
            }
            var light=new GameObject("Earned lantern light",typeof(RectTransform),typeof(CanvasRenderer),typeof(CeremonyGlowGraphic));light.transform.SetParent(transform,false);light.transform.SetSiblingIndex(0);
            var lightRect=(RectTransform)light.transform;lightRect.anchorMin=new Vector2(.03f,gift?.40f:.30f);lightRect.anchorMax=new Vector2(.97f,gift?.83f:.92f);lightRect.offsetMin=lightRect.offsetMax=Vector2.zero;glow=light.GetComponent<CeremonyGlowGraphic>();glow.raycastTarget=false;
            int pieces=!opening||owner.ReducedMotion||owner.Preferences.EffectQuality==0?0:owner.Preferences.EffectQuality==1?12:24;
            confetti=new Image[pieces];paperBodies=new PaperBody[pieces];var random=new System.Random(finalScore+Mathf.RoundToInt(count*31)+(opening?419:73));
            for(int i=0;i<pieces;i++){
                var obj=new GameObject("Celebration paper "+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));obj.transform.SetParent(transform,false);
                var rect=(RectTransform)obj.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,gift?.62f:.73f);
                float size=18+(float)random.NextDouble()*16;rect.sizeDelta=new Vector2(size,size);
                confetti[i]=obj.GetComponent<Image>();confetti[i].sprite=owner.CelebrationPaper(i%6);confetti[i].preserveAspect=true;confetti[i].raycastTarget=false;
                paperBodies[i]=new PaperBody{position=new Vector2(((float)random.NextDouble()-.5f)*84,0),
                    velocity=new Vector2(((float)random.NextDouble()-.5f)*220,110+(float)random.NextDouble()*80),
                    phase=(float)random.NextDouble()*Mathf.PI*2,angle=(float)random.NextDouble()*360,
                    spin=((float)random.NextDouble()-.5f)*260,drag=.45f+(float)random.NextDouble()*.7f,
                    gravity=140+(float)random.NextDouble()*65,delay=(gift?.50f:1.08f)+i*.015f};
                confetti[i].color=new Color(1,1,1,0);
            }
            metrics=GetComponentsInChildren<RectTransform>().Where(r=>r.name.StartsWith("Award metric ",System.StringComparison.Ordinal)).ToArray();
            counters=gift?System.Array.Empty<Counter>():GetComponentsInChildren<VictoryScoreDigits>()
                .Where(display=>display!=scoreDigits).Select(Counter.Read).Where(counter=>counter!=null).ToArray();
            Paint();
            ceremony=DOTween.To(()=>elapsed,value=>{elapsed=value;Paint();},Duration,Duration).SetEase(Ease.Linear).SetUpdate(UpdateType.Manual).SetTarget(this).Pause();
        }
        void Update()
        {
            if(game==null)return;
#if UNITY_EDITOR
            if(previewHeld)return;
#endif
            if(game.ReducedMotion){if(elapsed<Duration||ceremony!=null){ceremony?.Kill();ceremony=null;elapsed=Duration;Paint();}return;}
            if(elapsed>=Duration){ceremony?.Kill();ceremony=null;return;}
            if(ceremony==null||!ceremony.IsActive())return;
            if(game.CelebrationMotionAllowed){
                // Keep the ceremony on elapsed foreground time when a frame is
                // slow; its score and large stars must not linger over the UI.
                float dt=Mathf.Max(0,Time.unscaledDeltaTime);AdvancePaper(Mathf.Min(.05f,dt));ceremony.Goto(Mathf.Min(Duration,elapsed+dt),false);
            }
        }
        void Paint()
        {
            if(game==null)return;
            bool quiet=game.ReducedMotion;
            if(ribbon!=null){float p=quiet?1:Mathf.Clamp01(elapsed/.62f),ease=1-Mathf.Pow(1-p,4);ribbon.localScale=new Vector3(Mathf.Lerp(.88f,1,ease),Mathf.Lerp(.72f,1,ease),1);ribbon.localRotation=Quaternion.Euler(0,0,quiet?0:Mathf.Sin(p*Mathf.PI)*1.2f);}
            if(glow!=null){float t=Mathf.Clamp01((elapsed-(gift?.50f:.24f))/2.0f);glow.SetLight(quiet||game.Preferences.EffectQuality==0?0:Mathf.Sin(t*Mathf.PI)*.65f,t);}
            if(stars!=null)for(int i=0;i<stars.Length;i++){
                if(stars[i]==null)continue;
                float age=elapsed-(gift?.12f:ArrivalStart)-i*(gift?.50f:ArrivalStep);
                float t=quiet?1:Mathf.Clamp01(age/(gift?.62f:ArrivalDuration)),ease=1-Mathf.Pow(1-t,3);
                stars[i].color=new Color(1,1,1,quiet?1:Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.12f)));
                var rect=stars[i].rectTransform;
                var layer=rect.GetComponent<Canvas>();if(layer!=null)layer.overrideSorting=!quiet&&t<1;
                float back=1+2.70158f*Mathf.Pow(t-1,3)+1.70158f*Mathf.Pow(t-1,2);
                rect.localScale=Vector3.one*(quiet?1:gift?Mathf.Lerp(starSizes[i],1,ease)+Mathf.Sin(t*Mathf.PI*2)*.08f:
                    Mathf.LerpUnclamped(starSizes[i],1,back));
                rect.anchoredPosition=starOrigins[i]+(quiet?Vector2.zero:starLaunch[i]*(1-ease));
                rect.localRotation=Quaternion.Euler(0,0,quiet?0:(1-ease)*(i%2==0?-12:12));
                float target=Mathf.Clamp01(earned-i);
                float fillProgress=quiet?1:Mathf.Clamp01((elapsed-FillStart-i*FillStep)/FillDuration);
                float amount=target*Mathf.SmoothStep(0,1,fillProgress);
                if(starFills[i]!=null){starFills[i].fillAmount=amount;starFills[i].color=stars[i].color;}
                else if(starStates?.Length>=3)stars[i].sprite=amount>=1?starStates[0]:amount>=.5f?starStates[1]:starStates[2];
                if(starGlints[i]!=null)starGlints[i].enabled=!quiet&&target>0&&fillProgress>=1;
                if(target>0&&fillProgress>=1&&i>chimed){chimed=i;game.Feedback.StarChime(i);if(i==Mathf.CeilToInt(earned)-1)game.Feedback.Sound(ToyCue.Success);game.Feedback.Haptic(ToyHapticMoment.Star);}
            }
            if(metrics!=null)for(int i=0;i<metrics.Length;i++){float t=quiet?1:Mathf.Clamp01((elapsed-(gift?1.45f:CountStart)-i*.10f)/.34f);metrics[i].GetComponent<CanvasGroup>().alpha=t;metrics[i].localScale=Vector3.one*Mathf.Lerp(.9f,1,1-Mathf.Pow(1-t,3));}
            float countProgress=quiet?1:Mathf.Clamp01((elapsed-CountStart)/CountDuration);
            float countEase=CasinoCount(countProgress);
            if(number!=null||scoreDigits!=null){int value=Mathf.RoundToInt(score*countEase);if(scoreDigits!=null)scoreDigits.SetNumber(value);else number.text=value.ToString("0",System.Globalization.CultureInfo.InvariantCulture);}
            foreach(var counter in counters??System.Array.Empty<Counter>())counter.Paint(countEase);
            if(confetti!=null)for(int i=0;i<confetti.Length;i++){
                if(confetti[i]==null)continue;var body=paperBodies[i];float age=elapsed-body.delay;
                var rect=confetti[i].rectTransform;rect.anchoredPosition=body.position;
                rect.localRotation=Quaternion.Euler(0,0,body.angle);
                rect.localScale=new Vector3(.25f+.75f*Mathf.Abs(Mathf.Cos(age*4+body.phase)),1,1);
                // Every paper must fade before this finite ceremony stops updating.
                float fadeStart=gift?2.45f:1.0f,fadeDuration=gift?.75f:.55f;
                float opacity=age<0?0:Mathf.SmoothStep(0,1,age/.12f)*(1-Mathf.SmoothStep(0,1,(age-fadeStart)/fadeDuration));
                confetti[i].color=new Color(1,1,1,quiet||game.Preferences.EffectQuality==0?0:opacity);
            }
        }
        static float CasinoCount(float progress)
        {
            if(progress>=1)return 1;
            // A quick opening roll settles into small, readable ticks. Quantizing
            // only the presentation keeps receipt values and gameplay untouched.
            float eased=1-Mathf.Pow(1-Mathf.Clamp01(progress),3.2f);
            return Mathf.Floor(eased*48f)/48f;
        }
        sealed class Counter
        {
            public VictoryScoreDigits display;public int final;public bool time;public string prefix;
            public static Counter Read(VictoryScoreDigits display)
            {
                string value=display.FormattedValue;if(string.IsNullOrEmpty(value))return null;
                if(value.Contains(":")){
                    var parts=value.Split(':');
                    if(parts.Length==2&&int.TryParse(parts[0],out int minutes)&&int.TryParse(parts[1],out int seconds))
                        return new Counter{display=display,final=minutes*60+seconds,time=true,prefix=string.Empty};
                    return null;
                }
                string prefix=value.StartsWith("×",System.StringComparison.Ordinal)?"×":string.Empty;
                if(!int.TryParse(value.Substring(prefix.Length),out int number))return null;
                return new Counter{display=display,final=number,prefix=prefix};
            }
            public void Paint(float progress)
            {
                int value=Mathf.RoundToInt(final*progress);
                display.SetFormatted(time?(value/60).ToString()+":"+(value%60).ToString("00"):prefix+value.ToString());
                float pulse=progress>=1?0:Mathf.Abs(Mathf.Sin(progress*Mathf.PI*24f))*(.025f+.025f*(1-progress));
                display.transform.localScale=Vector3.one*(1+pulse);
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
#if UNITY_EDITOR
        public void SetPreview(float seconds){previewHeld=true;elapsed=Mathf.Clamp(seconds,0,Duration);ceremony?.Goto(elapsed,false);Paint();}
        public void ResumePreview(){previewHeld=false;}
        public void SetPreviewRating(float starsToShow){earned=Mathf.Clamp(starsToShow,0,stars?.Length??0);Paint();}
#endif
    }
    public sealed class VictoryScoreDigits:MonoBehaviour
    {
        Material symbolOutlineMaterial;
        Sprite[] glyphs,symbolGlyphs;Image[] digitImages,symbolImages;TextMeshProUGUI[] separatorFallbacks;Vector2[] digitScales;TMP_FontAsset font;RectTransform area;float capHeight;string previous;Vector2 previousArea;
        public string FormattedValue=>previous;
        public bool Initialize(Sprite[] glyphs,TMP_FontAsset displayFont,float height,Sprite[] generatedSymbols=null)
        {
            if(glyphs==null||glyphs.Length<10||glyphs.Any(s=>s==null))return false;
            area=transform as RectTransform;font=displayFont;capHeight=height;this.glyphs=glyphs;symbolGlyphs=generatedSymbols;digitScales=Enumerable.Repeat(Vector2.one,10).ToArray();
            digitImages=System.Array.Empty<Image>();symbolImages=System.Array.Empty<Image>();separatorFallbacks=System.Array.Empty<TextMeshProUGUI>();
            SetNumber(0);return true;
        }
        void EnsureSlots(int digitsNeeded,int symbolsNeeded)
        {
            int oldDigits=digitImages.Length;if(digitsNeeded>oldDigits){System.Array.Resize(ref digitImages,digitsNeeded);for(int i=oldDigits;i<digitsNeeded;i++){
                var item=new GameObject("Score digit "+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));item.transform.SetParent(transform,false);
                digitImages[i]=item.GetComponent<Image>();digitImages[i].preserveAspect=true;digitImages[i].raycastTarget=false;
                var outline=item.AddComponent<Outline>();outline.effectColor=new Color(.19f,.075f,.018f,.98f);outline.effectDistance=new Vector2(1.6f,-1.6f);item.SetActive(false);
            }}
            int oldSymbols=symbolImages.Length;if(symbolsNeeded>oldSymbols){System.Array.Resize(ref symbolImages,symbolsNeeded);System.Array.Resize(ref separatorFallbacks,symbolsNeeded);for(int i=oldSymbols;i<symbolsNeeded;i++){
                var glyph=new GameObject("Themed number symbol "+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));glyph.transform.SetParent(transform,false);
                symbolImages[i]=glyph.GetComponent<Image>();symbolImages[i].preserveAspect=true;symbolImages[i].raycastTarget=false;
                // Trace the alpha silhouette with a thin contour instead
                // of duplicating the entire small glyph with uGUI Outline.
                if(symbolOutlineMaterial==null){
                    var shader=Resources.Load<Shader>("CascadeArt/NumberSymbolOutline");
                    if(shader!=null)symbolOutlineMaterial=new Material(shader){name="Number symbol silhouette outline",hideFlags=HideFlags.DontSave};
                }
                symbolImages[i].material=symbolOutlineMaterial;
                glyph.AddComponent<NumberSymbolContourPadding>();
                glyph.SetActive(false);
                var fallback=new GameObject("Number symbol fallback "+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI),typeof(Outline));fallback.transform.SetParent(transform,false);
                separatorFallbacks[i]=fallback.GetComponent<TextMeshProUGUI>();separatorFallbacks[i].font=font;separatorFallbacks[i].fontStyle=FontStyles.Normal;
                separatorFallbacks[i].alignment=TextAlignmentOptions.Midline;separatorFallbacks[i].textWrappingMode=TextWrappingModes.NoWrap;
                separatorFallbacks[i].margin=Vector4.zero;separatorFallbacks[i].color=new Color(1,.70f,.12f);separatorFallbacks[i].raycastTarget=false;
                var outline=fallback.GetComponent<Outline>();outline.effectColor=new Color(.18f,.08f,.015f,.94f);outline.effectDistance=new Vector2(1.4f,-1.4f);fallback.SetActive(false);
            }}
        }
        public void SetCapHeight(float height)
        {
            if(Mathf.Abs(capHeight-height)<.1f)return;
            capHeight=height;string value=previous;previous=null;SetFormatted(value);
        }
        public void SetDigitScale(char digit,Vector2 scale)
        {
            if(digit<'0'||digit>'9'||digitScales==null)return;
            scale=new Vector2(Mathf.Clamp(scale.x,.75f,1.2f),Mathf.Clamp(scale.y,.75f,1.2f));
            int index=digit-'0';if(Vector2.SqrMagnitude(digitScales[index]-scale)<.0001f)return;
            digitScales[index]=scale;string value=previous;previous=null;SetFormatted(value);
        }
        public void SetNumber(int value)
        {
            string formatted=Mathf.Max(0,value).ToString("0",System.Globalization.CultureInfo.InvariantCulture);
            SetFormatted(formatted);
        }
        public void SetFormatted(string formatted)
        {
            if(area==null||digitImages==null||glyphs==null||digitScales==null)return;
            formatted??=string.Empty;
            if(formatted==previous&&area.rect.size==previousArea)return;previous=formatted;previousArea=area.rect.size;
            if(string.IsNullOrEmpty(formatted)){
                foreach(var image in digitImages)if(image!=null)image.gameObject.SetActive(false);
                foreach(var image in symbolImages??System.Array.Empty<Image>())if(image!=null)image.gameObject.SetActive(false);
                foreach(var label in separatorFallbacks??System.Array.Empty<TextMeshProUGUI>())if(label!=null)label.gameObject.SetActive(false);
                return;
            }
            EnsureSlots(formatted.Count(c=>c>='0'&&c<='9'),formatted.Count(c=>c<'0'||c>'9'));
            float h=Mathf.Min(capHeight,area.rect.height*.88f),width=0;
            for(int i=0;i<formatted.Length;i++){
                width+=GlyphWidth(formatted[i],h);
                if(i<formatted.Length-1)width+=h*(formatted[i]=='×'?.13f:.035f);
            }
            float availableWidth=Mathf.Max(0,area.rect.width-h*.10f);
            float fit=width>availableWidth?availableWidth/width:1;h*=fit;width*=fit;
            float x=-width*.5f;int digitIndex=0,symbolIndex=0;
            for(int characterIndex=0;characterIndex<formatted.Length;characterIndex++){
                char c=formatted[characterIndex];
                float itemWidth;
                if(c<'0'||c>'9'){
                    itemWidth=GlyphWidth(c,h);int index=symbolIndex++;var sprite=c=='×'&&symbolGlyphs!=null&&symbolGlyphs.Length>0?symbolGlyphs[0]:c==':'&&symbolGlyphs!=null&&symbolGlyphs.Length>1?symbolGlyphs[1]:c=='.'&&symbolGlyphs!=null&&symbolGlyphs.Length>2?symbolGlyphs[2]:null;
                    var image=symbolImages[index];var r=image.rectTransform;
                    if(sprite!=null){
                        image.name=c=='×'?"Themed multiplication glyph":c=='.'?"Themed decimal point glyph":"Themed colon glyph";image.sprite=sprite;
                        r.sizeDelta=new Vector2(itemWidth,c=='×'?h*.46f:c=='.'?h*.30f:h*.70f);r.anchoredPosition=new Vector2(x+itemWidth*.5f,c=='.'?-h*.20f:0);image.gameObject.SetActive(true);
                    }else{
                        var label=separatorFallbacks[index];var fallbackRect=label.rectTransform;
                        fallbackRect.sizeDelta=new Vector2(itemWidth,c=='×'?h*.64f:h*1.1f);fallbackRect.anchoredPosition=new Vector2(x+itemWidth*.5f,c==','?-h*.08f:c=='.'?-h*.20f:c=='-'?-h*.02f:0);
                        label.text=c=='-'?"−":c.ToString();label.fontSize=h*(c=='×'?.40f:c==':'?.55f:c==','?.58f:c=='.'?.68f:c=='-'||c=='/'?.92f:.65f);label.gameObject.SetActive(true);
                    }
                }else{
                    int n=c-'0';var scale=digitScales[n];itemWidth=glyphs[n].rect.width/glyphs[n].rect.height*h*scale.x;var image=digitImages[digitIndex++];image.sprite=glyphs[n];var r=image.rectTransform;float glyphHeight=h*scale.y;r.sizeDelta=new Vector2(itemWidth,glyphHeight);r.anchoredPosition=new Vector2(x+itemWidth*.5f,(h-glyphHeight)*.04f);image.gameObject.SetActive(true);
                }
                x+=itemWidth;
                if(characterIndex<formatted.Length-1)x+=h*(c=='×'?.13f:.035f);
            }
            for(int i=digitIndex;i<digitImages.Length;i++)digitImages[i].gameObject.SetActive(false);
            for(int i=symbolIndex;i<symbolImages.Length;i++){symbolImages[i].gameObject.SetActive(false);separatorFallbacks[i].gameObject.SetActive(false);}
        }
        float GlyphWidth(char c,float height)
        {
            if(c>='0'&&c<='9')return glyphs[c-'0'].rect.width/glyphs[c-'0'].rect.height*height*digitScales[c-'0'].x;
            if(c==',')return height*.30f;
            if(c=='.')return height*.30f;
            if(c==':')return height*.35f;
            if(c=='×')return height*.36f;
            if(c=='/')return height*.60f;
            if(c=='—')return height*.75f;
            if(c=='-')return height*.60f;
            return height*.48f;
        }
        void OnDestroy(){if(symbolOutlineMaterial!=null)Destroy(symbolOutlineMaterial);}
    }
    // Leave space outside the original image quad for the contour. Otherwise
    // its round diagonal tips get clipped flat at the sprite rectangle edges.
    sealed class NumberSymbolContourPadding:BaseMeshEffect
    {
        public override void ModifyMesh(VertexHelper vh)
        {
            if(!IsActive()||vh.currentVertCount!=4)return;
            var vertices=new UIVertex[4];
            for(int i=0;i<4;i++)vh.PopulateUIVertex(ref vertices[i],i);
            float minX=vertices.Min(v=>v.position.x),maxX=vertices.Max(v=>v.position.x);
            float minY=vertices.Min(v=>v.position.y),maxY=vertices.Max(v=>v.position.y);
            float minU=vertices.Min(v=>v.uv0.x),maxU=vertices.Max(v=>v.uv0.x);
            float minV=vertices.Min(v=>v.uv0.y),maxV=vertices.Max(v=>v.uv0.y);
            if(maxX<=minX||maxY<=minY)return;
            const float padding=1;
            for(int i=0;i<4;i++){
                var v=vertices[i];float dx=v.position.x<(minX+maxX)*.5f?-padding:padding;
                float dy=v.position.y<(minY+maxY)*.5f?-padding:padding;
                v.position+=new Vector3(dx,dy,0);
                v.uv0+=new Vector4(dx*(maxU-minU)/(maxX-minX),dy*(maxV-minV)/(maxY-minY),0,0);
                vh.SetUIVertex(v,i);
            }
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RetryArrowGraphic:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=rectTransform.rect;float size=Mathf.Min(rect.width,rect.height);if(size<=0)return;
            Vector2 center=rect.center;float radius=size*.34f,thickness=size*.095f;
            const int segments=36;const float start=40,end=340;
            for(int i=0;i<segments;i++){
                float a0=Mathf.Lerp(start,end,i/(float)segments)*Mathf.Deg2Rad;
                float a1=Mathf.Lerp(start,end,(i+1)/(float)segments)*Mathf.Deg2Rad;
                Vector2 d0=new Vector2(Mathf.Cos(a0),Mathf.Sin(a0)),d1=new Vector2(Mathf.Cos(a1),Mathf.Sin(a1));int v=vh.currentVertCount;
                vh.AddVert(center+d0*(radius+thickness*.5f),color,Vector2.zero);vh.AddVert(center+d1*(radius+thickness*.5f),color,Vector2.zero);
                vh.AddVert(center+d1*(radius-thickness*.5f),color,Vector2.zero);vh.AddVert(center+d0*(radius-thickness*.5f),color,Vector2.zero);
                vh.AddTriangle(v,v+1,v+2);vh.AddTriangle(v+2,v+3,v);
            }
            float endAngle=end*Mathf.Deg2Rad;Vector2 radial=new Vector2(Mathf.Cos(endAngle),Mathf.Sin(endAngle));
            Vector2 tangent=new Vector2(-radial.y,radial.x),tip=center+radial*radius+tangent*size*.13f;
            Vector2 basePoint=center+radial*(radius-size*.035f);float wing=size*.13f;int tipIndex=vh.currentVertCount;
            vh.AddVert(tip,color,Vector2.zero);vh.AddVert(basePoint+radial*wing,color,Vector2.zero);vh.AddVert(basePoint-radial*wing,color,Vector2.zero);
            vh.AddTriangle(tipIndex,tipIndex+1,tipIndex+2);
        }
    }
    public sealed class VictoryHeadlineShimmer:MonoBehaviour
    {
        CascadeGame game;TMP_Text label;Color32[][] baseColors;float age=.35f;float minX,maxX;string cachedText;
        public void Initialize(CascadeGame owner,TMP_Text text)
        {
            game=owner;label=text;CacheMesh();
        }
        void CacheMesh()
        {
            if(label==null)return;label.ForceMeshUpdate();var info=label.textInfo;cachedText=label.text;
            baseColors=new Color32[info.meshInfo.Length][];minX=float.PositiveInfinity;maxX=float.NegativeInfinity;
            for(int i=0;i<info.meshInfo.Length;i++)baseColors[i]=(Color32[])info.meshInfo[i].colors32.Clone();
            for(int i=0;i<info.characterCount;i++){
                var character=info.characterInfo[i];if(!character.isVisible)continue;
                minX=Mathf.Min(minX,character.bottomLeft.x);maxX=Mathf.Max(maxX,character.topRight.x);
            }
            if(float.IsNaN(minX)||float.IsInfinity(minX)||float.IsNaN(maxX)||float.IsInfinity(maxX)||maxX-minX<1){minX=0;maxX=1;}
        }
        void Update()
        {
            if(label==null||game==null||!game.CelebrationMotionAllowed)return;
            label.ForceMeshUpdate();var info=label.textInfo;
            if(baseColors==null||baseColors.Length!=info.meshInfo.Length||cachedText!=label.text){CacheMesh();return;}
            age+=Mathf.Min(.05f,Time.unscaledDeltaTime);
            float cycle=Mathf.Repeat(age,3.2f);bool sweeping=cycle<.95f;float center=minX+(maxX-minX)*Mathf.Clamp01(cycle/.95f);
            for(int m=0;m<info.meshInfo.Length;m++){
                var colors=info.meshInfo[m].colors32;var source=baseColors[m];int count=Mathf.Min(colors.Length,source.Length);
                for(int i=0;i<count;i++)colors[i]=source[i];
            }
            if(sweeping&&!game.ReducedMotion){
                float spread=(maxX-minX)*.075f;
                for(int i=0;i<info.characterCount;i++){
                    var character=info.characterInfo[i];if(!character.isVisible)continue;
                    int material=character.materialReferenceIndex,index=character.vertexIndex;
                    if(material<0||material>=info.meshInfo.Length||index+3>=info.meshInfo[material].colors32.Length)continue;
                    float x=(character.bottomLeft.x+character.topRight.x)*.5f;
                    float shine=Mathf.Pow(Mathf.Clamp01(1-Mathf.Abs(x-center)/Mathf.Max(1,spread)),2.5f)*.82f;
                    if(shine<=0)continue;
                    var colors=info.meshInfo[material].colors32;
                    for(int v=0;v<4;v++){
                        var original=baseColors[material][index+v];
                        colors[index+v]=new Color32((byte)Mathf.Lerp(original.r,255,shine),(byte)Mathf.Lerp(original.g,255,shine),(byte)Mathf.Lerp(original.b,218,shine),original.a);
                    }
                }
            }
            label.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
    public sealed class AwardGlintGraphic:MaskableGraphic
    {
        CascadeGame game;float age;int slot;
        public void Initialize(CascadeGame owner,int index){game=owner;slot=index;}
        void Update(){if(game==null||!game.CelebrationMotionAllowed)return;age+=Mathf.Min(.05f,Time.unscaledDeltaTime);SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh){
            vh.Clear();if(game==null||game.ReducedMotion||game.Preferences.EffectQuality==0||age<1.10f+slot*.24f)return;
            var r=rectTransform.rect;
            for(int i=0;i<2;i++){
                float a=Mathf.Pow(Mathf.Max(0,Mathf.Sin(age*2.1f-slot*.7f+i*1.8f)),12)*.8f;
                var c=new Color(1,1,.86f,a);var center=new Vector2(r.xMin+r.width*(i==0?.34f:.72f),r.yMin+r.height*(i==0?.74f:.42f));
                float size=r.width*.085f;int v=vh.currentVertCount;
                vh.AddVert(center+Vector2.up*size,c,Vector2.zero);vh.AddVert(center+Vector2.right*size*.45f,c,Vector2.zero);
                vh.AddVert(center-Vector2.up*size,c,Vector2.zero);vh.AddVert(center-Vector2.right*size*.45f,c,Vector2.zero);
                vh.AddTriangle(v,v+1,v+2);vh.AddTriangle(v,v+2,v+3);
            }
        }
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
