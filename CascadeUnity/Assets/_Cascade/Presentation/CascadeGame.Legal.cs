using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        RectTransform legalOverlay;
        Sequence legalArrival;
        LegalSettings LegalConfiguration=>Resources.Load<LegalSettings>("CascadeArt/LegalSettings");
        void DrawLegalControls()
        {
            var restore=SettingsButton("Restore purchases",RestorePurchasesFromSettings);
            restore.interactable=purchaseStore?.CanRestore==true;
            if(!restore.interactable)SettingsNote("Restore availability","Purchase restoration needs the store connection. No coins are spent.");
            SettingsButton("Privacy Policy",()=>OpenLegalDocument("privacy"));
            SettingsButton("Terms of Use",()=>OpenLegalDocument("terms"));
            var settings=LegalConfiguration;
            if(settings?.HasPublishedDocuments==true)SettingsButton("Support",()=>UnityEngine.Application.OpenURL(settings.supportUrl));
        }
        void RestorePurchasesFromSettings()
        {
            if(purchaseStore?.CanRestore!=true)return;
            // Reuse native restoration and server-verified idempotent grants. Never issue coins here.
            purchaseStore.Restore();
        }
        public void OpenLegalDocument(string document)
        {
            if(document!="privacy"&&document!="terms")return;
            CloseLegalDocument();
            var resource=Resources.Load<TextAsset>("CascadeLegal/"+document);
            legalOverlay=Rect("CASCADE legal document",(RectTransform)safeRoot.parent,Vector2.zero,Vector2.one);
            var canvas=legalOverlay.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=60;
            legalOverlay.gameObject.AddComponent<GraphicRaycaster>();
            Background(legalOverlay,new Color(.90f,.96f,.92f,1),true);
            var safe=Rect("Legal safe area",legalOverlay,safeRoot.anchorMin,safeRoot.anchorMax);
            var heading=Text("Legal document title",safe,new Vector2(.07f,.90f),new Vector2(.93f,.975f),26,document=="privacy"?"Privacy Policy":"Terms of Use");
            StyleCelebrationType(heading,CelebrationType.DisplayTitle);heading.color=MenuInk;
            var note=Text("Legal language",safe,new Vector2(.07f,.845f),new Vector2(.93f,.89f),14,"Full documents are in English");note.color=MenuInk;
            var viewport=Rect("Legal reading viewport",safe,new Vector2(.07f,.17f),new Vector2(.93f,.825f));
            Background(viewport,Color.clear,true);viewport.gameObject.AddComponent<RectMask2D>();
            var content=Rect("Legal reading content",viewport,new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);content.sizeDelta=Vector2.zero;
            var text=Text("Legal document text",content,Vector2.zero,Vector2.one,17,UiCopy.Raw(resource?.text??"Document unavailable in this build. Please contact the publisher before release."));
            text.richText=false;text.alignment=TextAlignmentOptions.TopLeft;text.enableAutoSizing=false;text.textWrappingMode=TextWrappingModes.Normal;
            text.color=MenuInk;text.margin=new Vector4(4,4,10,12);text.fontSize=(Preferences.LargeText?20:17)*MenuUnit;
            // A top-anchored TMP body is the ScrollRect content; the fitter owns its height.
            text.rectTransform.anchorMin=new Vector2(0,1);text.rectTransform.anchorMax=Vector2.one;text.rectTransform.pivot=new Vector2(.5f,1);
            text.rectTransform.sizeDelta=Vector2.zero;
            var fitter=text.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=text.rectTransform;
            scroll.horizontal=false;scroll.vertical=true;scroll.inertia=true;scroll.decelerationRate=.12f;scroll.movementType=ScrollRect.MovementType.Clamped;
            var close=Button("Close",safe,new Vector2(.07f,.04f),new Vector2(.93f,.115f),CloseLegalDocument);
            StandardAction(close);CentreSingleAction(close);
            close.GetComponentInChildren<TextMeshProUGUI>().color=MenuPaper;
            var settings=LegalConfiguration;string url=document=="privacy"?settings?.privacyPolicyUrl:settings?.termsOfUseUrl;
            if(settings?.HasPublishedDocuments==true){
                var external=Button("Open on website",safe,new Vector2(.07f,.12f),new Vector2(.93f,.165f),()=>UnityEngine.Application.OpenURL(url));
                StandardAction(external,true);CentreSingleAction(external);
            }
            Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=1;
            if(!ReducedMotion&&IntroductionAnimationAllowed){
                var group=legalOverlay.gameObject.AddComponent<CanvasGroup>();group.alpha=.75f;
                legalArrival=DOTween.Sequence().SetUpdate(true).SetTarget(legalOverlay);
                legalArrival.Append(group.DOFade(1,.20f).SetEase(Ease.OutCubic));
            }
            close.Select();
        }
        public void CloseLegalDocument()
        {
            legalArrival?.Kill();legalArrival=null;
            if(legalOverlay==null)return;legalOverlay.gameObject.SetActive(false);Destroy(legalOverlay.gameObject);legalOverlay=null;
        }
    }
}
