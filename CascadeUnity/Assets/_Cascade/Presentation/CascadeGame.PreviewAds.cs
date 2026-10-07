using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        RectTransform previewAd,previewAdSafe,coinToast;
        Button previewFinish;TMP_Text previewClock;
        Action<bool> previewAdDone;
        float previewAdAge;int previewSeconds=-1;bool previewRewarded;
        Tween previewArrival,coinArrival;

        // Marked local preview when there are no publisher keys. Never initializes a device ad SDK.
        public void ShowPreviewAd(bool rewarded,Action<bool> finished)
        {
            if(!TestMonetization||previewAd!=null)throw new InvalidOperationException("Test ad already showing or disabled.");
            previewAdDone=finished;previewRewarded=rewarded;previewAdAge=0;previewSeconds=-1;
            previewAd=Rect("Test advertisement",(RectTransform)safeRoot.parent,Vector2.zero,Vector2.one);
            var canvas=previewAd.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=60;
            previewAd.gameObject.AddComponent<GraphicRaycaster>();Background(previewAd,new Color(.035f,.12f,.13f,.94f),true);
            previewAdSafe=Rect("Advertisement safe area",previewAd,safeRoot.anchorMin,safeRoot.anchorMax);FitPreviewSafeArea();
            var sheet=Rect("Advertisement cloth",previewAdSafe,new Vector2(.06f,.18f),new Vector2(.94f,.82f));
            var face=Background(sheet,Color.white,false);face.sprite=enamelControls[0];face.type=Image.Type.Sliced;
            var heading=Text("Test advertisement heading",sheet,new Vector2(.08f,.82f),new Vector2(.92f,.96f),26,"Test ad");heading.font=art.titleFont;heading.color=MenuPaper;
            Picture("Video reward pouch",sheet,new Vector2(.27f,.47f),new Vector2(.73f,.78f),TravelArt(3),true);
            var promise=Text("Advertisement promise",sheet,new Vector2(.08f,.31f),new Vector2(.92f,.48f),20,rewarded?(UiCopy)"Watch to the end for 40 coins.":"A short break between levels.");promise.color=MenuPaper;
            previewClock=Text("Test advertisement countdown",sheet,new Vector2(.08f,.21f),new Vector2(.92f,.31f),19,"");
            previewClock.color=MenuPaper;
            previewFinish=Button(rewarded?(UiCopy)"Collect coins":"Continue",sheet,new Vector2(.10f,.08f),new Vector2(.90f,.21f),()=>FinishPreviewAd(true));
            var enamel=previewFinish.GetComponent<Image>();enamel.sprite=enamelControls[0];enamel.type=Image.Type.Sliced;enamel.color=Color.white;previewFinish.interactable=false;previewFinish.GetComponentInChildren<TMP_Text>().color=MenuPaper;
            var cancel=Button("Close",previewAdSafe,new Vector2(.25f,.07f),new Vector2(.75f,.15f),()=>FinishPreviewAd(false));cancel.name="Close test advertisement";
            var cancelFace=cancel.GetComponent<Image>();cancelFace.sprite=enamelControls[0];cancelFace.type=Image.Type.Sliced;cancelFace.color=Color.white;cancel.GetComponentInChildren<TMP_Text>().color=MenuPaper;
            var opacity=previewAd.gameObject.AddComponent<CanvasGroup>();opacity.alpha=ReducedMotion?1:0;
            if(!ReducedMotion)previewArrival=DOTween.To(()=>opacity.alpha,v=>opacity.alpha=v,1,.20f).SetUpdate(true).SetEase(Ease.OutCubic).SetTarget(previewAd);
            TickPreviewAd();
        }
        void FitPreviewSafeArea(){previewAdSafe.anchorMin=safeRoot.anchorMin;previewAdSafe.anchorMax=safeRoot.anchorMax;previewAdSafe.offsetMin=safeRoot.offsetMin;previewAdSafe.offsetMax=safeRoot.offsetMax;}
        void TickPreviewAd()
        {
            if(previewAd==null)return;FitPreviewSafeArea();
            bool active=applicationFocused&&!applicationPaused;
            if(previewArrival!=null&&previewArrival.IsActive()){if(active)previewArrival.Play();else previewArrival.Pause();}
            if(active)previewAdAge+=Time.unscaledDeltaTime;
            int seconds=Mathf.Max(0,Mathf.CeilToInt((previewRewarded?5:2)-previewAdAge));
            previewFinish.interactable=seconds==0;
            if(seconds!=previewSeconds){previewSeconds=seconds;previewClock.SetCopy(seconds==0?(UiCopy)"All done":new UiCopy("{0} seconds",seconds));}
        }
        void FinishPreviewAd(bool confirmed)
        {
            if(previewAd==null||confirmed&&!previewFinish.interactable)return;
            var callback=previewAdDone;previewAdDone=null;previewArrival?.Kill();previewArrival=null;
            previewAd.gameObject.SetActive(false);Destroy(previewAd.gameObject);previewAd=null;previewAdSafe=null;
            callback?.Invoke(confirmed);
        }
        public void ShowCoinReward(int amount=40)
        {
            if(!CanPresent||CurrentScreen!=GameScreen.Shop&&CurrentScreen!=GameScreen.CoinPacks)return;
            coinArrival?.Kill();if(coinToast!=null)Destroy(coinToast.gameObject);
            coinToast=Rect("Video coins received",menuRoot,new Vector2(.09f,.34f),new Vector2(.91f,.45f));
            var face=Background(coinToast,new Color(.04f,.28f,.28f),false);face.sprite=art.panel;face.type=Image.Type.Sliced;
            Picture("Received coin pouch",coinToast,new Vector2(.04f,.05f),new Vector2(.24f,.95f),TravelArt(3),true);
            var caption=Text("Coins received",coinToast,new Vector2(.25f,.12f),new Vector2(.94f,.88f),22,new UiCopy("{0} coins added",amount));caption.color=MenuPaper;caption.font=art.titleFont;
            caption.textWrappingMode=TextWrappingModes.NoWrap;caption.enableAutoSizing=true;caption.fontSizeMin=18*MenuUnit;caption.fontSizeMax=22*MenuUnit;
            var group=coinToast.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;
            var body=coinToast;var arrival=DOTween.Sequence().SetUpdate(true).SetTarget(body).SetLink(body.gameObject,LinkBehaviour.KillOnDisable);
            if(!ReducedMotion){body.localScale=Vector3.one*.94f;arrival.Append(body.DOScale(1,.30f).SetEase(Ease.OutBack));}
            arrival.AppendInterval(1.7f).Append(DOTween.To(()=>group.alpha,v=>group.alpha=v,0,.30f)).OnComplete(()=>{if(body!=null)Destroy(body.gameObject);});
            coinArrival=arrival;feedback.Sound(ToyCue.Success);
        }
    }
}
