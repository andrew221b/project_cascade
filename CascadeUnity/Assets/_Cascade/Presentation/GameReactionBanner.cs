using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // A bounded presentation beat. It has no access to commands, inventory or rewards.
    public sealed class GameReactionBanner : MonoBehaviour
    {
        CascadeGame game;
        RectTransform root,paper,heroMount,clothRect;
        TMP_Text instruction;
        Image paperImage,cloth;
        TextMeshProUGUI title;
        CanvasGroup opacity,clothOpacity;
        Vector3 initialScale;
        float initialAngle;
        readonly Vector3[] corners=new Vector3[4];
        public RectTransform Source { get; private set; }
        public bool StatusVisible => !hasTool||game.ReducedMotion||age>=.88f;
        GameObject hero;
        Transform home;
        Vector2 homeMin,homeMax,homeOffsetMin,homeOffsetMax;
        Vector3 homeScale;
        Quaternion homeRotation;
        Color homeColor;
        bool homeRaycast;
        readonly System.Collections.Generic.List<(TMP_Text text,float size,float max)> homeText=new System.Collections.Generic.List<(TMP_Text,float,float)>();
        Vector2 sourcePoint;
        readonly Image[] motes=new Image[8];
        readonly Sprite[] motifs=new Sprite[6];
        readonly Material[] cleanMotifs=new Material[3];
        float age;
        int motif,count;
        bool hasTool,windWarning;
        WindWarningGraphic windLines;
        public bool IsWindWarning => IsShowing&&windWarning;
        public bool IsShowing { get; private set; }
        public float Elapsed => age;
        public string Message => title==null?"":title.text;
        public float BeatDuration => windWarning?1.05f:game.ReducedMotion?.55f:hasTool?1.72f:.60f;
        public void Initialize(CascadeGame owner,RectTransform band,TMP_Text status,TMP_FontAsset font)
        {
            game=owner;paper=band;instruction=status;paperImage=band.GetComponent<Image>();root=(RectTransform)transform;
            opacity=gameObject.AddComponent<CanvasGroup>();opacity.blocksRaycasts=false;opacity.interactable=false;
            heroMount=Child("Reaction focus",root,new Vector2(.5f,.68f),new Vector2(.5f,.68f));
            var atlas=Resources.Load<Texture2D>("CascadeArt/Reactions/ToolMotifsClean");
            for(int i=0;i<6;i++)motifs[i]=WorkshopSpriteSet.Tool(atlas,i);
            for(int i=0;i<3;i++){cleanMotifs[i]=new Material(Resources.Load<Shader>("CascadeArt/Reactions/CleanSprite")){name="Clean reaction motif "+i};cleanMotifs[i].SetFloat("_AlphaFloor",i==0?.72f:i==1?.46f:.42f);cleanMotifs[i].SetFloat("_AlphaFeather",i==0?.035f:.10f);}
            for(int i=0;i<motes.Length;i++){
                motes[i]=Child("Reaction motif "+i,root,new Vector2(.5f,.68f),new Vector2(.5f,.68f)).gameObject.AddComponent<Image>();
                motes[i].raycastTarget=false;motes[i].preserveAspect=true;motes[i].gameObject.SetActive(false);
            }
            windLines=Child("Approaching wind",root,new Vector2(.12f,.38f),new Vector2(.88f,.95f)).gameObject.AddComponent<WindWarningGraphic>();
            windLines.raycastTarget=false;windLines.gameObject.SetActive(false);
            clothRect=Child("Woven announcement",root,new Vector2(0,.08f),new Vector2(1,.30f));cloth=clothRect.gameObject.AddComponent<Image>();clothOpacity=clothRect.gameObject.AddComponent<CanvasGroup>();clothOpacity.blocksRaycasts=false;
            var texture=Resources.Load<Texture2D>("CascadeArt/Reactions/Announcement");
            cloth.sprite=Sprite.Create(texture,new Rect(0,texture.height*.18f,texture.width,texture.height*.64f),new Vector2(.5f,.5f),100);cloth.raycastTarget=false;
            title=Child("Reaction title",clothRect,new Vector2(.13f,.12f),new Vector2(.87f,.88f)).gameObject.AddComponent<TextMeshProUGUI>();
            title.font=font;title.color=new Color(1,.97f,.83f);title.fontStyle=FontStyles.Bold;title.alignment=TextAlignmentOptions.Center;
            title.textWrappingMode=TextWrappingModes.NoWrap;title.overflowMode=TextOverflowModes.Overflow;title.margin=Vector4.zero;title.raycastTarget=false;
            title.gameObject.AddComponent<SymbolGlyphCenter>();gameObject.SetActive(false);
        }
        static RectTransform Child(string name,Transform parent,Vector2 min,Vector2 max)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        public void Show(UiCopy message,string tool,RectTransform source=null)
        {
            if(game==null||game.CurrentScreen!=GameScreen.Playing)return;
            ReleaseTool();
            foreach(var m in motes)m.gameObject.SetActive(false);
            windWarning=false;windLines.gameObject.SetActive(false);Source=source;heroMount.anchoredPosition=Vector2.zero;heroMount.localScale=Vector3.one;heroMount.localRotation=Quaternion.identity;hasTool=source!=null;age=0;IsShowing=true;gameObject.SetActive(true);root.SetAsLastSibling();RefreshLayout();
            sourcePoint=source==null?Vector2.zero:(Vector2)root.InverseTransformPoint(source.TransformPoint(source.rect.center))-(Vector2)heroMount.localPosition;
            if(source!=null){
                source.GetWorldCorners(corners);var a=root.InverseTransformPoint(corners[0]);var b=root.InverseTransformPoint(corners[1]);var c=root.InverseTransformPoint(corners[2]);
                initialScale=new Vector3(Vector3.Distance(b,c)/heroMount.rect.width,Vector3.Distance(a,b)/heroMount.rect.height,1);
                initialAngle=source.eulerAngles.z-root.eulerAngles.z;
            }
            if(hasTool){
                hero=source.gameObject;home=source.parent;homeMin=source.anchorMin;homeMax=source.anchorMax;
                homeOffsetMin=source.offsetMin;homeOffsetMax=source.offsetMax;homeScale=source.localScale;homeRotation=source.localRotation;
                var originalImage=source.GetComponent<Image>();homeColor=originalImage.color;homeRaycast=originalImage.raycastTarget;
                homeText.Clear();foreach(var text in source.GetComponentsInChildren<TMP_Text>(true))homeText.Add((text,text.fontSize,text.fontSizeMax));
                source.SetParent(heroMount,false);hero.SetActive(true);
                var r=(RectTransform)hero.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;r.localScale=Vector3.one;r.localRotation=Quaternion.identity;
                var image=hero.GetComponent<Image>();image.color=Color.white;image.raycastTarget=false;
                foreach(var text in hero.GetComponentsInChildren<TMP_Text>()){text.fontSizeMax=54;text.fontSize=42;}
            }
            motif=tool=="RepairKit"?1:tool=="Stamp"?2:tool=="RibbonSpool"?3:tool=="Hourglass"?4:tool=="Bomb"?5:0;
            count=motif==2?1:motif==1?3:5;
            if(game.Preferences.EffectQuality==1)count=Mathf.Min(3,count);if(game.Preferences.EffectQuality==0)count=0;if(game.ReducedMotion)count=Mathf.Min(2,count);
            title.text=message.ToString();title.ForceMeshUpdate();opacity.alpha=1;heroMount.gameObject.SetActive(hasTool);
            instruction.enabled=false;paperImage.enabled=false;Paint();
        }
        public void ShowWind(bool right)
        {
            Show(right?"Gust coming: right":"Gust coming: left","");windWarning=true;
            windLines.gameObject.SetActive(true);windLines.Right=right;RefreshLayout();Paint();
        }
        void Update()
        {
            if(!IsShowing||game.Interaction==null)return;
            if(game.CurrentScreen!=GameScreen.Playing){Clear();return;}
            RefreshLayout();if(!game.enabled||game.Interaction.IsPaused||game.Interaction.IsSuspended)return;
            age+=Time.unscaledDeltaTime;if(age>=BeatDuration){Clear();return;}Paint();
        }
        void Paint()
        {
            float unit=1/Mathf.Min(1,GetComponentInParent<Canvas>().scaleFactor);
            if(windWarning)windLines.Paint(game.ReducedMotion?0:age,game.ReducedMotion);
            float arrive=game.ReducedMotion?1:1-Mathf.Pow(1-Mathf.Clamp01(age/.42f),3);
            float pulse=game.ReducedMotion?0:Mathf.Sin(Mathf.Clamp01((age-.42f)/.44f)*Mathf.PI);
            heroMount.anchoredPosition=game.ReducedMotion?Vector2.zero:sourcePoint*(1-arrive)+Vector2.up*(Mathf.Sin(Mathf.Clamp01(age/.42f)*Mathf.PI)*18*unit);
            heroMount.localScale=Vector3.one*(game.ReducedMotion?1:1+.085f*pulse);
            if(!game.ReducedMotion)heroMount.localScale=Vector3.Lerp(initialScale,Vector3.one,arrive)*(1+.085f*pulse);
            float statusReveal=game.ReducedMotion||!hasTool?1:Mathf.SmoothStep(0,1,Mathf.InverseLerp(.88f,1.06f,age));
            clothOpacity.alpha=statusReveal;clothRect.localScale=new Vector3(1,.88f+.12f*statusReveal,1);
            heroMount.localRotation=Quaternion.Euler(0,0,game.ReducedMotion?0:Mathf.LerpAngle(initialAngle,0,arrive)+(motif==0?10:motif==3?-7:3)*pulse);
            float exit=windWarning?BeatDuration-.18f:game.ReducedMotion?BeatDuration-.14f:BeatDuration-.27f;opacity.alpha=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(exit,BeatDuration,age));
            for(int i=0;i<motes.Length;i++){
                bool active=hasTool&&i<count&&age>=(game.ReducedMotion?0:.46f);motes[i].gameObject.SetActive(active);if(!active)continue;
                var m=motes[i];m.sprite=motifs[motif];m.material=cleanMotifs[motif==4?1:motif==5?2:0];float t=game.ReducedMotion?.35f:Mathf.Clamp01((age-.46f)/.76f);float angle=i*2.39996f;
                var direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                Vector2 offset=direction*(42+46*t);
                if(motif==1)offset=new Vector2((i-1)*32,44-28*t+12*Mathf.Sin(t*Mathf.PI));
                else if(motif==2)offset=new Vector2(56,8);
                else if(motif==3)offset=new Vector2(direction.x*(30+28*t),38+54*t);
                else if(motif==4)offset=new Vector2(Mathf.Cos(angle+t),Mathf.Sin(angle+t))*64;
                m.rectTransform.anchoredPosition=offset*unit;m.rectTransform.sizeDelta=Vector2.one*(motif==5?72:motif==2?72:52)*unit;
                m.rectTransform.localRotation=Quaternion.Euler(0,0,game.ReducedMotion?0:direction.x*t*24);
                m.color=new Color(1,1,1,(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.48f,1,t)))*(motif==5?.6f:1));
            }
        }
        public void RefreshLayout()
        {
            if(!IsShowing)return;float unit=1/Mathf.Min(1,GetComponentInParent<Canvas>().scaleFactor);
            root.localScale=Vector3.one;
            if(windWarning){root.anchorMin=root.anchorMax=new Vector2(.5f,.53f);root.anchoredPosition=Vector2.zero;root.sizeDelta=new Vector2(((RectTransform)root.parent).rect.width*.9f,115*unit);clothRect.anchorMin=new Vector2(0,.02f);clothRect.anchorMax=new Vector2(1,.40f);}
            else if(hasTool){root.anchorMin=root.anchorMax=new Vector2(.5f,.46f);root.anchoredPosition=Vector2.zero;root.sizeDelta=new Vector2(((RectTransform)root.parent).rect.width*.90f,220*unit);clothRect.anchorMin=new Vector2(0,.08f);clothRect.anchorMax=new Vector2(1,.30f);}
            else{root.anchorMin=paper.anchorMin;root.anchorMax=paper.anchorMax;root.anchoredPosition=paper.anchoredPosition;root.sizeDelta=paper.sizeDelta;clothRect.anchorMin=Vector2.zero;clothRect.anchorMax=Vector2.one;}
            heroMount.sizeDelta=Vector2.one*132*unit;
            title.enableAutoSizing=true;title.fontSizeMin=16*unit;title.fontSizeMax=(game.Preferences.LargeText?23:21)*unit;
        }
        public void Clear()
        {
            IsShowing=false;Source=null;if(instruction!=null)instruction.enabled=true;if(paperImage!=null)paperImage.enabled=true;gameObject.SetActive(false);
            ReleaseTool();
        }
        void ReleaseTool()
        {
            if(hero!=null&&home!=null){
                var r=(RectTransform)hero.transform;r.SetParent(home,false);r.anchorMin=homeMin;r.anchorMax=homeMax;
                r.offsetMin=homeOffsetMin;r.offsetMax=homeOffsetMax;r.localScale=homeScale;r.localRotation=homeRotation;
                var image=hero.GetComponent<Image>();image.color=homeColor;image.raycastTarget=homeRaycast;
                foreach(var entry in homeText)if(entry.text!=null){entry.text.fontSizeMax=entry.max;entry.text.fontSize=entry.size;}
                hero.SetActive(true);
            }
            hero=null;home=null;homeText.Clear();
        }
        void OnDestroy(){ReleaseTool();if(cloth!=null&&cloth.sprite!=null)Destroy(cloth.sprite);foreach(var s in motifs)if(s!=null)Destroy(s);foreach(var m in cleanMotifs)if(m!=null)Destroy(m);}
    }
}
