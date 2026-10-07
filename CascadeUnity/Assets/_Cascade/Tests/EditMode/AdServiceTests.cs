using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Solver;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class AdServiceTests
    {
        static AdContext Context(AdSurface surface=AdSurface.Win,int clears=15,string node="C15")=>new AdContext{Surface=surface,NodeId=node,UniqueClears=clears,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true,SavedWin=true,BaseRewardGiven=true};
        static AdService Service(MonetizationLedger ledger=null,Action<MonetizationLedger> persist=null)=>new AdService(ledger??new MonetizationLedger(),persist??(_=>{}),12);
        static AttemptService Attempt()=>new AttemptService(ContentTests.LoadCatalog().GetPrimary("C02").Definition);
        static HintOffer Hint(AttemptService a,int budget=PuzzleSolver.DefaultStateBudget)=>AttemptService.CalculateHint(a.CaptureHintRequest(),budget);
        [Test] public void NativeEntitlementRejectsUnverifiedOrWrongProductsAndRestoresExactlyOnce()
        {
            int saves=0;var service=Service(persist:_=>saves++);
            Assert.That(service.VerifiedStorePurchase(PurchaseCatalog.NoAutomaticAds,"store_a",false),Is.False);
            Assert.That(service.VerifiedStorePurchase("mock.no_automatic_ads","store_a",true),Is.False);
            Assert.That(service.VerifiedStorePurchase(PurchaseCatalog.NoAutomaticAds,"store_a",true),Is.True);
            Assert.That(service.VerifiedStorePurchase(PurchaseCatalog.NoAutomaticAds,"store_a",true),Is.True);
            Assert.That(service.Ledger.NoAutomaticAds,Is.True);Assert.That(service.Ledger.Grants.Count,Is.EqualTo(1));
            Assert.That(saves,Is.EqualTo(1));Assert.That(service.Ledger.BonusCoins+service.Ledger.HintTokens,Is.Zero);
        }
        [Test] public void NativeEntitlementIsNotPublishedBeforeItsSaveSucceeds()
        {
            bool fail=true;var service=Service(persist:_=>{if(fail)throw new IOException("disk");});
            Assert.That(service.VerifiedStorePurchase(PurchaseCatalog.NoAutomaticAds,"store_b",true),Is.False);
            Assert.That(service.Ledger.NoAutomaticAds,Is.False);Assert.That(service.Ledger.Grants,Is.Empty);
            fail=false;Assert.That(service.VerifiedStorePurchase(PurchaseCatalog.NoAutomaticAds,"store_b",true),Is.True);
        }
        [Test] public void AutomaticAdsRespectEveryGateAndNeverAppearDuringPuzzleFlows()
        {
            foreach(var surface in Enum.GetValues(typeof(AdSurface)).Cast<AdSurface>().Where(s=>s!=AdSurface.Win)) {
                var s=Service();s.AdvanceForeground(180);Assert.That(s.TryAutomatic(Context(surface),true),Is.Null);
            }
            Action<AdContext>[] gates={c=>c.CoreAccepted=false,c=>c.PrivacyReady=false,c=>c.ProviderEnabled=false,
                c=>c.UniqueClears=2,c=>c.SavedWin=false,c=>c.BaseRewardGiven=false,c=>c.ContinueVisible=true,c=>c.ChapterFinal=true,c=>c.Replay=true};
            foreach(var gate in gates){var s=Service();s.AdvanceForeground(180);var c=Context();gate(c);Assert.That(s.TryAutomatic(c,true),Is.Null);}
            var early=Service();early.AdvanceForeground(179);Assert.That(early.TryAutomatic(Context(),true),Is.Null);
            var few=Service();few.AdvanceForeground(180);Assert.That(few.TryAutomatic(Context(clears:14),true),Is.Null);
        }
        [Test] public void FirstSessionCanShowAfterThreeCompleteNewLevelsAndThreeMinutes()
        {
            var s=new AdService(new MonetizationLedger(),_=>{});var c=Context(clears:3,node:"C03");c.FirstSession=true;
            s.AdvanceForeground(179);Assert.That(s.TryAutomatic(Context(clears:2,node:"C02"),true),Is.Null);
            s.AdvanceForeground(1);var ticket=s.TryAutomatic(c,true);Assert.That(ticket,Is.Not.Null);
            s.Opened(ticket.Id);s.Closed(ticket.Id);s.AdvanceForeground(180);
            Assert.That(s.TryAutomatic(Context(clears:5,node:"C05"),true),Is.Null);
            Assert.That(s.TryAutomatic(Context(clears:6,node:"C06"),true),Is.Not.Null);
        }
        [Test] public void NotReadySkipsThatWinAndTwoAutomaticShowsAreTheSessionLimit()
        {
            var s=Service();s.AdvanceForeground(180);Assert.That(s.TryAutomatic(Context(),false),Is.Null);
            Assert.That(s.TryAutomatic(Context(),true),Is.Null);
            var first=s.TryAutomatic(Context(clears:16,node:"C16"),true);Assert.That(first,Is.Not.Null);
            Assert.That(s.Opened(first.Id),Is.True);Assert.That(s.Opened(first.Id),Is.False);s.Closed(first.Id);
            s.AdvanceForeground(180);Assert.That(s.TryAutomatic(Context(clears:18,node:"C18"),true),Is.Null);
            var second=s.TryAutomatic(Context(clears:19,node:"C19"),true);Assert.That(second,Is.Not.Null);s.Opened(second.Id);s.Closed(second.Id);
            s.AdvanceForeground(180);Assert.That(s.TryAutomatic(Context(clears:22,node:"C22"),true),Is.Null);Assert.That(s.AutomaticShows,Is.EqualTo(2));
        }
        [Test] public void RewardIsVerifiedOnceEvenAfterCloseAndCloseAloneNeverGrants()
        {
            var s=Service();var c=Context(AdSurface.Inventory);var ticket=s.TryRewarded(c,"coins",true);
            Assert.That(s.TryRewarded(c,"hint",true),Is.Null);Assert.That(s.Opened("wrong"),Is.False);
            Assert.That(s.RewardConfirmed(ticket.Id,"before-open",true),Is.False);s.Opened(ticket.Id);s.Closed(ticket.Id);
            Assert.That(s.Ledger.BonusCoins,Is.Zero);Assert.That(s.RewardConfirmed(null,"null",true),Is.False);
            Assert.That(s.RewardConfirmed(ticket.Id,"r1",false),Is.False);Assert.That(s.RewardConfirmed(ticket.Id,"r1",true),Is.True);
            Assert.That(s.RewardConfirmed(ticket.Id,"r2",true),Is.False);Assert.That(s.Ledger.BonusCoins,Is.EqualTo(40));
            var next=s.TryRewarded(c,"hint",true);s.Opened(next.Id);Assert.That(s.RewardConfirmed(next.Id,"r1",true),Is.False);
            s.Failed(next.Id);s.Failed(null);Assert.That(s.RewardConfirmed(next.Id,"r3",true),Is.False);
        }
        [Test] public void NoAutomaticAdsAndHideOffersHaveSeparateMeaning()
        {
            var s=Service(new MonetizationLedger{NoAutomaticAds=true});s.AdvanceForeground(180);
            Assert.That(s.TryAutomatic(Context(),true),Is.Null);Assert.That(s.TryRewarded(Context(),"coins",true),Is.Null);
            Assert.That(s.TryRewarded(Context(AdSurface.Inventory),"coins",true),Is.Not.Null);
            Assert.That(Service(new MonetizationLedger{HideVideoOffers=true}).TryRewarded(Context(AdSurface.Inventory),"coins",true),Is.Null);
        }
        [Test] public void RewardedShowRestartsTheAutomaticInterval()
        {
            var s=Service();s.AdvanceForeground(180);var t=s.TryRewarded(Context(AdSurface.Inventory,16),"hint",true);
            s.Opened(t.Id);s.AdvanceForeground(300);s.Closed(t.Id);Assert.That(s.TryAutomatic(Context(clears:19,node:"C19"),true),Is.Null);
            s.AdvanceForeground(180);Assert.That(s.TryAutomatic(Context(clears:20,node:"C20"),true),Is.Not.Null);
        }
        [Test] public void FailedSavePublishesNoRewardAndCanBeRetried()
        {
            bool fail=true;var s=Service(persist:_=>{if(fail)throw new IOException("disk");});var t=s.TryRewarded(Context(AdSurface.Inventory),"hint",true);s.Opened(t.Id);
            Assert.That(s.RewardConfirmed(t.Id,"r",true),Is.False);Assert.That(s.Ledger.HintTokens,Is.Zero);Assert.That(s.Ledger.Grants,Is.Empty);
            fail=false;Assert.That(s.RewardConfirmed(t.Id,"r",true),Is.True);Assert.That(s.Ledger.HintTokens,Is.EqualTo(1));
        }
        [Test] public void MockNonConsumablesRestoreIdempotentlyWithoutConsumableRewards()
        {
            var s=Service();Assert.That(s.VerifiedMockPurchase("mock.no_automatic_ads","p",false),Is.False);
            Assert.That(s.VerifiedMockPurchase("unknown","u",true),Is.False);
            Assert.That(s.VerifiedMockPurchase("mock.no_automatic_ads","p",true),Is.True);
            Assert.That(s.VerifiedMockPurchase("mock.no_automatic_ads","p",true,true),Is.False);
            Assert.That(s.VerifiedMockPurchase("mock.keepsake.leaf","l",true,true),Is.True);
            Assert.That(s.VerifiedMockPurchase("mock.keepsake.leaf","l2",true,true),Is.True);
            Assert.That(s.Ledger.Cosmetics,Is.EqualTo(new[]{"leaf"}));Assert.That(s.Ledger.NoAutomaticAds,Is.True);
            Assert.That(s.Ledger.BonusCoins+s.Ledger.HintTokens,Is.Zero);
        }
        [Test] public void UnknownStaleAndAlreadyRevealedHintsDoNotSpendAndStepDoesNotExecute()
        {
            var a=Attempt();var s=Service(new MonetizationLedger{HintTokens=2});string before=a.CurrentStateHash;
            var unknown=Hint(a,1);Assert.That(unknown.Result.Status,Is.EqualTo(HintStatus.Unknown));Assert.That(s.TrySpendHint(a,unknown),Is.False);
            var stale=Hint(a);a.Execute(PuzzleCommand.Swap(1));Assert.That(s.TrySpendHint(a,stale),Is.False);a.Undo();
            var hint=Hint(a);Assert.That(s.TrySpendHint(a,hint),Is.True);Assert.That(s.TrySpendHint(a,hint),Is.False);
            Assert.That(a.CurrentStateHash,Is.EqualTo(before));Assert.That(a.HintUsed,Is.True);Assert.That(s.Ledger.HintTokens,Is.EqualTo(1));
        }
        [Test] public void HintFlagAndCacheRollBackWhenGrantCommitFails()
        {
            var a=Attempt();var s=Service(new MonetizationLedger{HintTokens=1},_=>throw new IOException("disk"));
            Assert.That(s.TrySpendHint(a,Hint(a)),Is.False);Assert.That(a.HintUsed,Is.False);Assert.That(a.RevealedHintStates,Is.Empty);
            Assert.That(s.Ledger.HintTokens,Is.EqualTo(1));Assert.That(s.Ledger.PaidHintStates,Is.Empty);
        }
        [Test] public void DiskTransactionSavesHintAndPaidStateTogetherAndReloadDoesNotRecharge()
        {
            string directory=Path.Combine(Path.GetTempPath(),"CascadeAds",Guid.NewGuid().ToString("N"));
            try {
                var store=new ProfileStore(Path.Combine(directory,"profile.json"));var p=new PlayerProfile();p.Monetization.HintTokens=1;var a=Attempt();
                var s=MonetizationTransaction.Create(p,store,()=>a);Assert.That(s.TrySpendHint(a,Hint(a)),Is.True);
                var loaded=store.Load().Profile;Assert.That(loaded.Monetization.HintTokens,Is.Zero);Assert.That(loaded.Attempt.HintUsed,Is.True);
                var restored=loaded.RestoreAttempt(ContentTests.LoadCatalog());var again=MonetizationTransaction.Create(loaded,store,()=>restored);
                Assert.That(again.TrySpendHint(restored,Hint(restored)),Is.True);Assert.That(again.Ledger.HintTokens,Is.Zero);
                Assert.That(again.Ledger.PaidHintStates.Count,Is.EqualTo(1));
                var old=loaded.Monetization;var oldAttempt=loaded.Attempt;
                var failing=MonetizationTransaction.Create(loaded,store,()=>throw new InvalidOperationException("capture"));
                Assert.That(failing.VerifiedMockPurchase("mock.no_automatic_ads","p",true),Is.False);
                Assert.That(loaded.Monetization,Is.SameAs(old));Assert.That(loaded.Attempt,Is.SameAs(oldAttempt));
            } finally {if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [TestCase(false)] [TestCase(true)]
        public void GrantDuringPlanningPreservesSavedDescentAndResumesNormalAttemptCapture(bool preview)
        {
            string directory=Path.Combine(Path.GetTempPath(),"CascadePlanningGrant",Guid.NewGuid().ToString("N"));
            try {
                var catalog=ContentTests.LoadCatalog();var source=catalog.GetPrimary("C06").Definition;
                var run=DescentRun.Build(source,version:3);Assert.That(run,Is.Not.Null);
                var saved=new AttemptService(run.Level(source,0));
                var profile=new PlayerProfile{Descent=run,JourneyAttempt=true,Attempt=AttemptRecord.Capture(saved)};
                var store=new ProfileStore(Path.Combine(directory,"profile.json"));store.Save(profile);
                var original=profile.Attempt;bool planning=true;int captures=0;var placeholder=Attempt();
                Func<AttemptService> capture=()=>{captures++;if(planning)throw new InvalidOperationException("Unverified placeholder capture");return placeholder;};
                var service=preview?MonetizationTransaction.CreatePreview(profile,store,capture,()=>planning)
                    :MonetizationTransaction.Create(profile,store,capture,()=>planning);
                bool Granted(string receipt)=>preview?service.TryPreviewCoinPack(PurchaseCatalog.Coins240)
                    :service.VerifiedStorePurchase(PurchaseCatalog.Coins240,receipt,true);
                Assert.That(Granted("pending-grant"),Is.True);Assert.That(captures,Is.Zero);
                Assert.That(profile.Attempt,Is.SameAs(original));Assert.That(profile.Descent,Is.SameAs(run));Assert.That(profile.JourneyAttempt,Is.True);
                var loaded=store.Load().Profile;
                Assert.That(loaded.Descent.ContentHash,Is.EqualTo(run.ContentHash));Assert.That(loaded.JourneyAttempt,Is.True);
                Assert.That(loaded.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(saved.CurrentStateHash));
                Assert.That(loaded.Monetization.BonusCoins,Is.EqualTo(preview?0:240));
                Assert.That(service.Ledger.BonusCoins,Is.EqualTo(240));
                // Mount completion supplies the new verified attempt and clears the old run before normal capture.
                planning=false;profile.Descent=null;profile.JourneyAttempt=false;
                Assert.That(Granted("mounted-grant"),Is.True);Assert.That(captures,Is.EqualTo(1));
                loaded=store.Load().Profile;Assert.That(loaded.Descent,Is.Null);Assert.That(loaded.JourneyAttempt,Is.False);
                Assert.That(loaded.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(placeholder.CurrentStateHash));
                Assert.That(loaded.Monetization.BonusCoins,Is.EqualTo(preview?0:480));
            } finally {if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test] public void PreviousSchemaOneProfileDefaultsToDisabledEmptyInventory()
        {
            string directory=Path.Combine(Path.GetTempPath(),"CascadeAds",Guid.NewGuid().ToString("N"));
            try {
                Directory.CreateDirectory(directory);var store=new ProfileStore(Path.Combine(directory,"profile.json"));
                var envelope=JObject.Parse(ProfileStore.Encode(new PlayerProfile()));var payload=JObject.Parse((string)envelope["payload"]);payload.Remove("Monetization");
                string text=payload.ToString(Newtonsoft.Json.Formatting.None);envelope["payload"]=text;envelope["sha256"]=ContentImporter.Hash(text);File.WriteAllText(store.Path,envelope.ToString());
                var loaded=store.Load();Assert.That(loaded.Recovery,Is.EqualTo(SaveRecovery.None));loaded.Profile.Monetization.Validate();
                Assert.That(loaded.Profile.Monetization.Grants,Is.Empty);Assert.That(loaded.Profile.Monetization.HintTokens,Is.Zero);
            } finally {if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
    }
}
