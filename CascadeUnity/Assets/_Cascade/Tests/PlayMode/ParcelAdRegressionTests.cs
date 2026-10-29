#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
namespace Cascade.Tests {
 public sealed partial class AccountReferenceTests {
  void ParcelAdShot(string name){
   var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/ParcelAdsOct17"));Directory.CreateDirectory(folder);Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=portrait;
   var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(pixels);
  }
  IEnumerator ParcelAdWin(int clears){
   yield return WinFooterFixture();game.AdProvider.UseLocalPreviewForReview(true);
   var session=(HashSet<string>)Field("sessionClears");
   foreach(var content in game.Catalog.PrimaryLevels){
    if(Profile.Clears.Count>=clears)break;
    if(Profile.Clears.Any(c=>c.CampaignNodeId==content.Definition.CampaignNodeId))continue;
    Profile.Clears.Add(new ClearReceipt{CampaignNodeId=content.Definition.CampaignNodeId,ContentHash=content.Definition.ContentHash,AttemptId=Guid.NewGuid().ToString("D"),BestStars=2});session.Add(content.Definition.CampaignNodeId);
   }
   Profile.Rewards.Synchronize(Profile.Clears.Select(c=>c.CampaignNodeId));
   if(Profile.Rewards.Parcels.Count>0)Profile.Completion.ParcelOrder=Profile.Rewards.Parcels[0].Order;
  }
  UnityEngine.UI.Button ParcelMore=>game.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.name=="Parcel open more");
  void ParcelAdReady(bool ready){typeof(NativeAdProvider).GetField("initialized",Private).SetValue(game.AdProvider,ready);game.NotifyStoreChanged();}
  [UnityTest] public IEnumerator ParcelAdCompletionShowsAutomaticBeforeParcelAndResumesOnce(){
   yield return ParcelAdWin(10);string hash=game.Interaction.Attempt.CurrentStateHash;int clears=Profile.AllClears.Count(),coins=game.ShopCoins;
   game.ContinueCompletion();Assert.That(game.AdProvider.Showing,Is.True,"A due automatic break must be offered before the pending parcel, rather than skipped until its exit");
   Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Win));game.ContinueCompletion();Assert.That(game.Monetization.AutomaticShows,Is.EqualTo(1));
   Control("Close test advertisement").onClick.Invoke();yield return FooterWait(()=>game.CurrentScreen==GameScreen.Parcel,"Closing/failing the break releases the earned parcel");yield return null;
   Assert.That(game.Monetization.AutomaticShows,Is.EqualTo(1));Assert.That(Profile.Rewards.Parcels[0].Claimed,Is.False);Assert.That(Profile.AllClears.Count(),Is.EqualTo(clears));Assert.That(game.ShopCoins,Is.EqualTo(coins));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
  }
  [UnityTest] public IEnumerator ParcelAdNextLevelDoesNotBypassTheAutomaticBreak(){
   yield return ParcelAdWin(2);int clears=Profile.AllClears.Count();game.ContinueToNextLevel();
   Assert.That(game.AdProvider.Showing,Is.True,"Next level must use the same due-ad checkpoint as Map and Parcel");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Win));
   Control("Close test advertisement").onClick.Invoke();yield return FooterWait(()=>game.CurrentScreen!=GameScreen.Win,"Continue after the break closes");
   Assert.That(Profile.AllClears.Count(),Is.EqualTo(clears));Assert.That(game.Monetization.AutomaticShows,Is.EqualTo(1));
  }
  [UnityTest] public IEnumerator ParcelAdOfferStaysVisibleUntilVideoBecomesReadyWithoutRestartingReveal(){
   yield return ParcelAdWin(10);ParcelAdReady(false);game.ShowTravelBag();game.ShowParcel();yield return null;
   var view=game.GetComponentInChildren<ParcelRevealView>();view.SetReviewPose(ParcelRevealView.RevealDuration);yield return null;
   Assert.That(ParcelMore.gameObject.activeSelf,Is.True,"Do not hide the bonus offer merely because the network has not loaded a video");Assert.That(ParcelMore.interactable,Is.False);
   var cards=game.GetComponentsInChildren<RectTransform>().Where(r=>r.name.StartsWith("Parcel reward card ")).ToArray();float age=view.RevealAge;ParcelAdReady(true);yield return null;
   Assert.That(game.GetComponentInChildren<ParcelRevealView>(),Is.SameAs(view));Assert.That(view.RevealAge,Is.EqualTo(age));Assert.That(ParcelMore.interactable,Is.True);
   CollectionAssert.AreEqual(cards,game.GetComponentsInChildren<RectTransform>().Where(r=>r.name.StartsWith("Parcel reward card ")).ToArray());
   foreach(string locale in new[]{"en","ru","uk"}){
    WelcomePhone(320,693);game.SetUiLocale(locale);game.ShowTravelBag();game.ShowParcel();
    game.GetComponentInChildren<ParcelRevealView>().SetReviewPose(ParcelRevealView.RevealDuration);ParcelAdReady(false);yield return null;game.RefreshLayout();yield return null;ParcelAdShot("unavailable-"+locale+"-320");
    foreach(var label in ParcelMore.GetComponentsInChildren<TMP_Text>()){label.ForceMeshUpdate();Assert.That(label.text.Contains("No translation")||label.isTextOverflowing||label.isTextTruncated,Is.False,locale+" unavailable "+label.text);}
    ParcelAdReady(true);yield return null;ParcelAdShot("ready-"+locale+"-320");
    foreach(var label in ParcelMore.GetComponentsInChildren<TMP_Text>()){label.ForceMeshUpdate();Assert.That(label.text.Contains("No translation")||label.isTextOverflowing||label.isTextTruncated,Is.False,locale+" "+label.text);}
   }
   Assert.That(Profile.Rewards.Parcels[0].Claimed,Is.False);Assert.That(Profile.Rewards.Parcels[0].BonusClaimed,Is.False);
  }
  [UnityTest] public IEnumerator ParcelAdUnavailableVideoKeepsParcelPendingAndNoAdsKeepsRewardedOptional(){
   yield return ParcelAdWin(10);ParcelAdReady(false);game.ContinueCompletion();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Parcel));Assert.That(game.AdProvider.Showing,Is.False);
   game.Monetization.Ledger.NoAutomaticAds=true;ParcelAdReady(true);yield return null;Assert.That(ParcelMore.gameObject.activeSelf,Is.True);
   var view=game.GetComponentInChildren<ParcelRevealView>();view.SetReviewPose(ParcelRevealView.RevealDuration);yield return null;Assert.That(ParcelMore.interactable,Is.True);
   game.Monetization.Ledger.HideVideoOffers=true;game.NotifyStoreChanged();Assert.That(ParcelMore.gameObject.activeSelf,Is.False,"An explicit hide-videos preference is still respected");
  }
  [UnityTest] public IEnumerator ParcelAdNavigationDuringBreakDoesNotOpenAStaleParcel(){
   yield return ParcelAdWin(10);game.ContinueCompletion();Assert.That(game.AdProvider.Showing,Is.True);game.ShowHome();Control("Close test advertisement").onClick.Invoke();yield return null;yield return null;
   Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That(Profile.Rewards.Parcels[0].Claimed,Is.False);
  }
 }
}
#endif
