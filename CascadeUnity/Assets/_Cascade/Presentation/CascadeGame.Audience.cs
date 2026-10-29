using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public enum AudienceAgeBand
    {
        Unknown=0,
        NineToTwelve=1,
        ThirteenToFifteen=2,
        SixteenPlus=3
    }

    public static class AudiencePolicy
    {
        public const string PreferenceKey="cascade.audience.age-band.v1";
        public static bool IsKnown(AudienceAgeBand value)=>value>=AudienceAgeBand.NineToTwelve&&value<=AudienceAgeBand.SixteenPlus;
        public static bool IsChild(AudienceAgeBand value)=>IsUnderAgeOfConsent(value);
        public static bool IsUnderAgeOfConsent(AudienceAgeBand value)=>value==AudienceAgeBand.NineToTwelve||value==AudienceAgeBand.ThirteenToFifteen;
        public static bool AllowsLinkedAccount(AudienceAgeBand value)=>value==AudienceAgeBand.SixteenPlus;
    }

    public sealed partial class CascadeGame
    {
        AudienceAgeBand audienceAgeBand;
        bool audienceServicesStarted;
#if UNITY_EDITOR
        public static AudienceAgeBand? EditorAudienceAge {get;set;}
#endif

        public AudienceAgeBand AudienceAgeBand=>audienceAgeBand;
        public bool AudienceAgeNeedsChoice=>!AudiencePolicy.IsKnown(audienceAgeBand);
        public bool AudienceIsChild=>AudiencePolicy.IsChild(audienceAgeBand);
        public bool AudienceUnderAgeOfConsent=>AudiencePolicy.IsUnderAgeOfConsent(audienceAgeBand);
        public bool AudienceCanUseLinkedAccount=>AudiencePolicy.AllowsLinkedAccount(audienceAgeBand);

        void InitializeAudiencePolicy()
        {
#if UNITY_EDITOR
            // Isolated captures and tests must remain deterministic and never inherit a
            // developer machine's PlayerPrefs selection.
            if(!string.IsNullOrEmpty(EditorProfileDirectory)){audienceAgeBand=EditorAudienceAge??AudienceAgeBand.SixteenPlus;return;}
#endif
            var stored=(AudienceAgeBand)PlayerPrefs.GetInt(AudiencePolicy.PreferenceKey,0);
            audienceAgeBand=AudiencePolicy.IsKnown(stored)?stored:AudienceAgeBand.Unknown;
        }

        void InitializeAudienceServices()
        {
            if(audienceServicesStarted||AudienceAgeNeedsChoice||saveBlocked)return;
            audienceServicesStarted=true;
            // Under-16 players stay local: the first-run answer never opens an
            // external account browser or creates an anonymous cloud identity.
            if(AudienceCanUseLinkedAccount)InitializeCloudSave();
            EnsureAudienceMonetization(); // Preload store metadata while the account screen is open.
        }

        void EnsureAudienceMonetization()
        {
            if(audienceServicesStarted&&!AudienceAgeNeedsChoice&&monetization==null)InitializeMonetization();
        }

        void SelectAudienceAge(AudienceAgeBand value)
        {
            if(!AudiencePolicy.IsKnown(value))return;
            audienceAgeBand=value;
            PlayerPrefs.SetInt(AudiencePolicy.PreferenceKey,(int)value);PlayerPrefs.Save();
            InitializeAudienceServices();
            // Adults choose their platform account (or guest play) before Home.
            // Under-16 players keep the direct local-only route.
            if(AudienceCanUseLinkedAccount)ShowAccountGate();else SkipAccountGate();
        }

        void ResetAudienceAge()
        {
            PersistProgress(true);
            PlayerPrefs.DeleteKey(AudiencePolicy.PreferenceKey);PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        UiCopy AudienceAgeLabel()=>AudiencePolicy.IsUnderAgeOfConsent(audienceAgeBand)?ReferenceCopy("Under 16","Младше 16","Молодше 16"):
            audienceAgeBand==AudienceAgeBand.SixteenPlus?ReferenceCopy("16 or older","16 или старше","16 або старше"):
            ReferenceCopy("Not selected","Не выбрано","Не вибрано");

        void DrawReferenceAgeGate()
        {
            var stage=HomeReferenceSurface(true,out var layout);
            var sign=HomeBounds("Age choice hanging sign",stage,108,146,504,144);
            var face=sign.gameObject.AddComponent<Image>();face.sprite=AccountReferenceArt.Get("Header");face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=270/144f;face.raycastTarget=false;
            var title=ReferenceText("age choice heading",sign,ReferenceCopy("Before we begin","Перед началом","Перед початком"),42,22,420,100,49);
            title.fontSizeMin=31;title.color=new Color(.20f,.065f,.015f);CentreHeading(face,title);
            sign.GetComponent<CenteredContentRow>().Padding=24;

            var panel=ReferencePicture("Age choice paper",stage,SettingsReferenceArt.Get("Panel"),0,0,520,408,false);
            panel.type=Image.Type.Sliced;panel.pixelsPerUnitMultiplier=2;
            panel.rectTransform.anchorMin=panel.rectTransform.anchorMax=panel.rectTransform.pivot=new Vector2(.5f,.5f);
            panel.rectTransform.anchoredPosition=Vector2.zero;
            var prompt=ReferenceText("age choice explanation",panel.transform,ReferenceCopy(
                "Are you 16 or older?",
                "Вам уже исполнилось\n16 лет?",
                "Вам уже виповнилося\n16 років?"),
                32,0,456,96,35,false,true);
            prompt.fontSizeMin=27;
            prompt.alignment=TextAlignmentOptions.Center;
            var yes=AudienceAgeButton(panel.rectTransform,layout,"Age 16 or older yes",ReferenceCopy("Yes","Да","Так"),()=>SelectAudienceAge(AudienceAgeBand.SixteenPlus));
            var no=AudienceAgeButton(panel.rectTransform,layout,"Age under 16 no",ReferenceCopy("No","Нет","Ні"),()=>SelectAudienceAge(AudienceAgeBand.NineToTwelve),true);
            panel.gameObject.AddComponent<AgeChoiceLayout>().Initialize(panel.rectTransform,prompt.rectTransform,(RectTransform)yes.transform,(RectTransform)no.transform);
            layout.Fit();
        }

        Button AudienceAgeButton(RectTransform parent,HomeReferenceLayout layout,string name,UiCopy copy,System.Action click,bool yellow=false)
        {
            const float width=352,height=96;
            var button=ReferenceButton(name,parent,0,0,width,height,click);
            var body=ReferenceButtonBody(button);
            // The green export reserves space for foliage around a smaller face.
            // Compensate the artwork, keeping both actions' touch bounds identical.
            float artWidth=yellow?width:width+40,artHeight=yellow?height:height+40;
            var face=ReferencePicture("Age choice button face",body,AccountReferenceArt.Get(yellow?"Gold":"Green"),(width-artWidth)*.5f,(height-artHeight)*.5f,artWidth,artHeight,false);
            face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=(yellow?182:227)/artHeight;
            var text=ReferenceText("age choice button label",body,copy,28,8,width-56,height-16,31,true);
            text.color=yellow?Color.black:Color.white;
            text.alignment=TextAlignmentOptions.Center;text.textWrappingMode=TextWrappingModes.NoWrap;
            CentreContent((RectTransform)body,text.rectTransform);
            layout.BindTouch((RectTransform)button.transform,new Vector2(width,height));
            return button;
        }
    }
}
