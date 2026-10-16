using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed class ParcelScreenFlowTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;string directory;bool background;
        PlayerProfile Profile=>(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
        ParcelRevealView View=>game.GetComponentsInChildren<ParcelRevealView>().Single();
        Button Claim=>game.GetComponentsInChildren<Button>().Single(b=>b.name=="Parcel claim");
        void Call(string name)=>typeof(CascadeGame).GetMethod(name,Private).Invoke(game,null);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeParcelTests",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            // The moved project has an independently reproduced first-run global
            // preload error. Expect only that exact SDK log, and verify the actual
            // UI table before exercising any screen. Other errors still fail tests.
            void PreloadLog(string message,string stack,LogType type){
                if(type==LogType.Error&&message.StartsWith("OperationException : Failed to initialize localization, could not preload string tables.",StringComparison.Ordinal))
                    LogAssert.Expect(type,new System.Text.RegularExpressions.Regex("^OperationException : Failed to initialize localization, could not preload string tables\\."));
            }
            UnityEngine.Application.logMessageReceived+=PreloadLog;
            try{
                yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
                foreach(string locale in new[]{"en","ru","uk"}){
                    var table=UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
                    yield return table;Assert.That(table.Status,Is.EqualTo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded),"Required UI table "+locale);
                }
            }finally{UnityEngine.Application.logMessageReceived-=PreloadLog;}
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.Preferences.Tutorials=false;
            foreach(var content in game.Catalog.PrimaryLevels.GroupBy(c=>c.Definition.CampaignNodeId).Select(g=>g.First()).Take(9))
                Profile.Clears.Add(new ClearReceipt{CampaignNodeId=content.Definition.CampaignNodeId,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=content.Definition.ContentHash});
            Profile.Rewards.Synchronize(Profile.Clears.Select(c=>c.CampaignNodeId));game.AdProvider.UseLocalPreviewForReview(true);yield return null;
        }
        [UnityTearDown] public IEnumerator Teardown(){if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=background;if(Directory.Exists(directory))Directory.Delete(directory,true);}
        [UnityTest] public IEnumerator EntryAutomaticallyOpensButOnlyClaimCommitsInventory()
        {
            game.ShowTravelBag();game.ShowParcel();Assert.That(Claim.interactable,Is.False);Assert.That(Profile.Rewards.Parcels[0].Claimed,Is.False);
            View.SetReviewPose(ParcelRevealView.RevealDuration);yield return null;Assert.That(Claim.interactable,Is.True);Assert.That(Profile.Rewards.Parcels[0].Claimed,Is.False);
            string hash=game.Interaction.Attempt.CurrentStateHash;Claim.onClick.Invoke();Claim.onClick.Invoke();Assert.That(Profile.Rewards.Parcels[0].Claimed,Is.True);Assert.That(View.Busy,Is.True);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Profile.Validate();
        }
        [UnityTest] public IEnumerator FirstPickupChangesToDoneAndReturnsToItsOrigin()
        {
            game.ShowTravelBag();game.ShowParcel();View.SetReviewPose(4.65f);yield return null;View.ResumeReview();Claim.onClick.Invoke();yield return new WaitForSecondsRealtime(1.6f);
            Assert.That(Claim.interactable,Is.True);Claim.onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));
            Call("OpenInventoryMap");game.ShowParcel();View.SetReviewPose(4.65f);yield return null;View.ResumeReview();Claim.onClick.Invoke();yield return new WaitForSecondsRealtime(1.6f);Claim.onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
        }
        [UnityTest] public IEnumerator PauseFreezesAnimationAndReducedMotionFinishesWithoutShake()
        {
            game.ShowTravelBag();game.ShowParcel();yield return new WaitForSecondsRealtime(.2f);game.SendMessage("OnApplicationPause",true);float age=View.RevealAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(View.RevealAge,Is.EqualTo(age));
            game.SendMessage("OnApplicationPause",false);game.ReducedMotion=true;yield return new WaitForSecondsRealtime(1);Assert.That(View.Busy,Is.False);Assert.That(View.transform.Find("Parcel closed box").localRotation,Is.EqualTo(Quaternion.identity));
        }
        [UnityTest] public IEnumerator CancellingSecondAdKeepsBaseAndDoesNotGrantBonus()
        {
            game.ShowTravelBag();game.ShowParcel();View.SetReviewPose(4.65f);yield return null;View.ResumeReview();game.GetComponentsInChildren<Button>().Single(b=>b.name=="Parcel open more").onClick.Invoke();yield return new WaitForSecondsRealtime(1.6f);
            Assert.That(Profile.Rewards.Parcels[0].Claimed,Is.True);game.GetComponentsInChildren<Button>().Single(b=>b.name=="Close test advertisement").onClick.Invoke();yield return null;
            Assert.That(Profile.Rewards.Parcels[0].BonusClaimed,Is.False);Assert.That(game.Monetization.HasParcelBonus(MonetizationLedger.ParcelSubject(1)),Is.False);Assert.That(game.GetComponentsInChildren<Button>().Single(b=>b.name=="Parcel open more").interactable,Is.True);
        }
        [UnityTest] public IEnumerator VerifiedSecondAdRevealsTwoOrThreeThenPickupReturns()
        {
            game.ShowTravelBag();game.ShowParcel();View.SetReviewPose(4.65f);yield return null;View.ResumeReview();game.GetComponentsInChildren<Button>().Single(b=>b.name=="Parcel open more").onClick.Invoke();yield return new WaitForSecondsRealtime(6.9f);Call("FinishPreviewAdReview");yield return null;
            int count=game.GetComponentsInChildren<RectTransform>().Count(r=>r.name.StartsWith("Parcel reward card "));Assert.That(count,Is.InRange(2,3));
            Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Parcel open more"),Is.False);View.SetReviewPose(4.65f);View.ResumeReview();yield return null;Claim.onClick.Invoke();yield return new WaitForSecondsRealtime(1.6f);Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));
        }
        [UnityTest] public IEnumerator AllLocalesFitAndIconsAndBoxKeepNativeAspect()
        {
            game.ShowTravelBag();game.ShowParcel();foreach(string locale in new[]{"ru","en","uk"}){
                game.SetUiLocale(locale);game.Preferences.LargeText=true;View.SetReviewPose(4.65f);yield return null;Canvas.ForceUpdateCanvases();
                foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){t.ForceMeshUpdate();Assert.That(t.isTextOverflowing||t.isTextTruncated,Is.False,t.name+" "+locale);}
                Assert.That(game.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Parcel reward icon")||i.name=="Parcel closed box").All(i=>i.preserveAspect),Is.True);
            }
        }
        [UnityTest] public IEnumerator AdAvailabilityRefreshDoesNotRestartRevealOrRebuildCards()
        {
            game.ShowTravelBag();game.ShowParcel();yield return null;var view=View;float age=view.RevealAge;
            game.Monetization.Ledger.HideVideoOffers=true;game.NotifyStoreChanged();Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Parcel open more"),Is.False);
            game.Monetization.Ledger.HideVideoOffers=false;game.NotifyStoreChanged();Assert.That(View,Is.SameAs(view));Assert.That(view.RevealAge,Is.EqualTo(age));
            Assert.That(game.GetComponentsInChildren<Button>().Single(b=>b.name=="Parcel open more").interactable,Is.False);
            view.SetReviewPose(4.65f);yield return null;Assert.That(game.GetComponentsInChildren<Button>().Single(b=>b.name=="Parcel open more").interactable,Is.True);
        }
        [UnityTest] public IEnumerator SunlightHasVisibleMeshAndUnclaimedBonusCanResume()
        {
            game.ShowTravelBag();game.ShowParcel();yield return null;Canvas.ForceUpdateCanvases();var light=game.GetComponentsInChildren<ParcelSunlight>().Single();var mesh=light.canvasRenderer.GetMesh();Assert.That(mesh.vertexCount,Is.GreaterThan(500));Assert.That(light.canvasRenderer.cull,Is.False);Assert.That(mesh.colors.Max(c=>c.a),Is.GreaterThan(.2));
            var service=(AdService)typeof(CascadeGame).GetProperty("GameEconomy",Private).GetValue(game);service.TryClaimParcel(1,Profile.Rewards);Profile.Monetization.Grants.Add(new GrantReceipt{Id="resume_verified",Kind="coins",Product=MonetizationLedger.ParcelSubject(1),Amount=40});Profile.Monetization.BonusCoins+=40;
            game.ShowTravelBag();game.ShowParcel();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Parcel));Profile.Validate();
        }
    }
}
