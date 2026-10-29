#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed partial class AccountReferenceTests
    {
        IEnumerator AdScenarioPreviewFixture()
        {
            // Session-only test ledger; never save QA switches into the production asset.
            var settings=Resources.Load<MonetizationSettings>("CascadeArt/MonetizationSettings");
            bool previous=settings.testMonetization;
            try{settings.testMonetization=true;yield return Load();game.ShowHome();}
            finally{settings.testMonetization=previous;}
            game.AdProvider.UseLocalPreviewForReview(true);
        }
        void AdScenarioShot(string name)
        {
            var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/AdScenarioOct17"));Directory.CreateDirectory(folder);Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=portrait;
            var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(pixels);
        }
        IEnumerator AdScenarioFinishVideo(bool wait=true)
        {
            if(wait)yield return FooterWait(()=>((Button)Field("previewFinish")).interactable,"The foreground rewarded countdown finishes");
            else {typeof(CascadeGame).GetField("previewAdAge",Private).SetValue(game,5.1f);typeof(CascadeGame).GetMethod("TickPreviewAd",Private).Invoke(game,null);}
            ((Button)Field("previewFinish")).onClick.Invoke();yield return null;
        }
        [UnityTest] public IEnumerator AdScenarioProductionDisablesTestScreensAndExplicitQaStillShowsSecondLevelInterstitial()
        {
            var settings=Resources.Load<MonetizationSettings>("CascadeArt/MonetizationSettings");
            Assert.That(settings.localAdTestScreen,Is.False);Assert.That(settings.admobTestAds,Is.False);
            yield return AdScenarioPreviewFixture();
            Assert.That((bool)typeof(NativeAdProvider).GetField("localPreview",Private).GetValue(game.AdProvider),Is.True,"Only the explicit Editor QA call enables local preview");
            Assert.That(game.AdProvider.RewardReady,Is.True);Assert.That(game.AdProvider.AutomaticReady,Is.True);
            yield return ParcelAdWin(2);game.ContinueToNextLevel();Assert.That(game.AdProvider.Showing,Is.True);Assert.That((bool)Field("previewRewarded"),Is.False);
            AdScenarioShot("test-interstitial-ru");Control("Close test advertisement").onClick.Invoke();yield return FooterWait(()=>game.CurrentScreen!=GameScreen.Win,"Interstitial closes into the next screen");Assert.That(game.Monetization.AutomaticShows,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator AdScenarioInitialAndExtraVideoBothWorkWithNoAdsAndCancelGrantsNothing()
        {
            yield return AdScenarioPreviewFixture();
            WelcomePhone(320,693);yield return ParcelAdWin(10);game.Monetization.Ledger.NoAutomaticAds=true;game.ContinueCompletion();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Parcel));Assert.That(game.AdProvider.Showing,Is.False);string hash=game.Interaction.Attempt.CurrentStateHash;int coins=game.ShopCoins;var receipt=Profile.Rewards.Parcels[0];string subject=MonetizationLedger.ParcelSubject(receipt.Order);
            var closed=game.GetComponentInChildren<ParcelRevealView>();yield return new WaitForSecondsRealtime(.15f);Assert.That(closed.RevealAge,Is.Zero);Assert.That(ParcelMore.interactable,Is.True);Assert.That(Control("Parcel claim").interactable,Is.True);AdScenarioShot("parcel-closed-before-video-ru-320");
            ParcelMore.onClick.Invoke();Assert.That((bool)Field("previewRewarded"),Is.True);AdScenarioShot("test-rewarded-open-ru-320");
            Assert.That(((Button)Field("previewFinish")).interactable,Is.False);((Button)Field("previewFinish")).onClick.Invoke();Assert.That(game.Monetization.HasParcelOpen(subject),Is.False,"Early confirmation cannot bypass the countdown");
            Control("Close test advertisement").onClick.Invoke();yield return null;Assert.That(game.Monetization.HasParcelOpen(subject),Is.False);Assert.That(receipt.Claimed,Is.False);Assert.That(game.ShopCoins,Is.EqualTo(coins));Assert.That(closed.RevealAge,Is.Zero);
            ParcelMore.onClick.Invoke();yield return AdScenarioFinishVideo();Assert.That(game.Monetization.HasParcelOpen(subject),Is.True);Assert.That(game.Monetization.Ledger.HintTokens,Is.Zero);Assert.That(game.ShopCoins,Is.EqualTo(coins));
            yield return FooterWait(()=>!game.GetComponentInChildren<ParcelRevealView>().Busy,"Verified video opens the base parcel");Assert.That(receipt.Claimed,Is.False);AdScenarioShot("parcel-opened-after-video-ru-320");
            Control("Parcel claim").onClick.Invoke();yield return FooterWait(()=>!game.GetComponentInChildren<ParcelRevealView>().Busy,"Base reward collection finishes");Assert.That(receipt.Claimed,Is.True);Assert.That(ParcelMore.interactable,Is.True);AdScenarioShot("parcel-extra-offer-with-no-ads-ru-320");
            ParcelMore.onClick.Invoke();Control("Close test advertisement").onClick.Invoke();yield return null;Assert.That(game.Monetization.HasParcelBonus(subject),Is.False);Assert.That(receipt.BonusClaimed,Is.False);
            ParcelMore.onClick.Invoke();yield return AdScenarioFinishVideo();yield return FooterWait(()=>!game.GetComponentInChildren<ParcelRevealView>().Busy,"Second video opens only the bonus parcel");Assert.That(game.Monetization.HasParcelBonus(subject),Is.True);Assert.That(game.Monetization.Ledger.Grants.Count(g=>g.Kind=="parcel_open"&&g.Product==subject),Is.EqualTo(1));Assert.That(game.Monetization.Ledger.Grants.Count(g=>g.Kind=="coins"&&g.Product==subject),Is.EqualTo(1));
            Assert.That(game.Monetization.AutomaticShows,Is.Zero);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Profile.Validate();
        }
        [UnityTest] public IEnumerator AdScenarioUnavailableVideoKeepsParcelPendingAndLaterReturnsSafely()
        {
            yield return AdScenarioPreviewFixture();
            yield return ParcelAdWin(10);ParcelAdReady(false);game.ShowTravelBag();game.ShowParcel();yield return null;
            int coins=game.ShopCoins;var receipt=Profile.Rewards.Parcels[0];Assert.That(ParcelMore.gameObject.activeSelf,Is.True);Assert.That(ParcelMore.interactable,Is.False);Assert.That(Control("Parcel claim").interactable,Is.True);
            game.WatchParcelBonus(receipt.Order);Assert.That(game.AdProvider.Showing,Is.False);Assert.That(game.GetComponentInChildren<ParcelRevealView>().RevealAge,Is.Zero);
            Control("Parcel claim").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));Assert.That(receipt.Claimed,Is.False);Assert.That(game.ShopCoins,Is.EqualTo(coins));
            typeof(CascadeGame).GetField("parcelBonusRound",Private).SetValue(game,true);typeof(CascadeGame).GetField("parcelAnimationShown",Private).SetValue(game,true);
            typeof(CascadeGame).GetMethod("OpenSelectedTrophyParcel",Private).Invoke(game,new object[]{receipt.GiftId});yield return null;
            Assert.That((bool)Field("parcelBonusRound"),Is.False,"The trophy entry cannot reuse a previous parcel's bonus round");Assert.That(game.GetComponentInChildren<ParcelRevealView>().RevealAge,Is.Zero);Assert.That(receipt.Claimed,Is.False);
        }
        [UnityTest] public IEnumerator AdScenarioLateOpenRewardNeverNavigatesAwayFromAnotherScreen()
        {
            yield return AdScenarioPreviewFixture();
            yield return ParcelAdWin(10);game.Monetization.Ledger.NoAutomaticAds=true;game.ShowTravelBag();game.ShowParcel();yield return null;string subject=MonetizationLedger.ParcelSubject(Profile.Rewards.Parcels[0].Order);
            ParcelMore.onClick.Invoke();game.ShowHome();yield return AdScenarioFinishVideo(false);Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That(game.Monetization.HasParcelOpen(subject),Is.True);Assert.That(Profile.Rewards.Parcels[0].Claimed,Is.False);
            game.ShowParcel();yield return null;yield return FooterWait(()=>!game.GetComponentInChildren<ParcelRevealView>().Busy,"Returning to a verified parcel needs no second opening video");Assert.That(game.AdProvider.Showing,Is.False);
        }
        [UnityTest] public IEnumerator AdScenarioParcelGateAndTestScreenFitAllSupportedLanguages()
        {
            yield return AdScenarioPreviewFixture();
            yield return ParcelAdWin(10);game.Monetization.Ledger.NoAutomaticAds=true;WelcomePhone(320,693);
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);yield return FooterWait(()=>game.UiLanguage==locale&&UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation.IsDone,"Locale is ready");
                game.ShowTravelBag();game.ShowParcel();yield return null;game.RefreshLayout();yield return null;AdScenarioShot("parcel-gate-"+locale+"-320");
                foreach(var label in ParcelMore.GetComponentsInChildren<TMP_Text>()){label.ForceMeshUpdate();Assert.That(label.isTextOverflowing||label.isTextTruncated||label.text.Contains("No translation"),Is.False,locale+" "+label.text);}
                ParcelMore.onClick.Invoke();yield return null;AdScenarioShot("test-rewarded-"+locale+"-320");
                foreach(var label in ((RectTransform)Field("previewAd")).GetComponentsInChildren<TMP_Text>()){label.ForceMeshUpdate();Assert.That(label.isTextOverflowing||label.isTextTruncated||label.text.Contains("No translation"),Is.False,locale+" "+label.text);}
                Control("Close test advertisement").onClick.Invoke();yield return null;
            }
        }
    }
}
#endif
