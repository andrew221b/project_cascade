using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using Cascade.Solver;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class AdServiceTests
    {
        [Test] public void AuthenticatedPurchaseContextMatchesServerWithoutTrustingAnotherPlayer()
        {
            const string project="test-project",player="test-player",environment="c4c6a4c9-180a-4a8e-8422-34de96a5ab21";
            var claims=new JObject{["project_id"]=project,["sub"]=player,["exp"]=DateTimeOffset.UtcNow.ToUnixTimeSeconds()+3600,["aud"]=new JArray("urn:unity:playerapp","envId:"+environment)};
            string Token(JObject value)=>"header."+Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value.ToString())).TrimEnd('=').Replace('+','-').Replace('/','_')+".sdk-authenticated";
            var actual=PurchaseIdentity.FromAuthenticatedSession(project,player,Token(claims));var expected=PurchaseIdentity.For(project,environment,player);
            Assert.That(actual.AppleToken,Is.EqualTo(expected.AppleToken));Assert.That(actual.GoogleAccountId,Is.EqualTo(expected.GoogleAccountId));
            foreach(var mutation in new Action<JObject>[]{v=>v["project_id"]="another-project",v=>v["sub"]="another-player",v=>v["exp"]=0,v=>v["aud"]=new JArray("envId:not-an-id"),v=>v["aud"]=new JArray("envId:"+environment,"envId:"+environment)}){
                var changed=(JObject)claims.DeepClone();mutation(changed);Assert.That(PurchaseIdentity.FromAuthenticatedSession(project,player,Token(changed)),Is.Null,"Unknown or mismatched context must use server fallback");
            }
            foreach(string token in new[]{null,"","opaque-sdk-token","a.bad-base64.c",new string('x',17000)})Assert.That(PurchaseIdentity.FromAuthenticatedSession(project,player,token),Is.Null);
        }

        [Test] public void FinalWorldAdGuardUsesJourneyOrderRatherThanDigitsInNodeIds()
        {
            for(int index=0;index<JourneyStory.NodeIds.Count;index++)Assert.That(JourneyStory.IsWorldFinal(JourneyStory.NodeIds[index]),Is.EqualTo((index+1)%60==0));
            Assert.That(JourneyStory.IsWorldFinal("C10"),Is.False);Assert.That(JourneyStory.IsWorldFinal("unknown"),Is.False);
        }
        static AdContext Context(AdSurface surface=AdSurface.Win,int clears=15,string node="C15")=>new AdContext{Surface=surface,NodeId=node,UniqueClears=clears,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true,SavedWin=true,BaseRewardGiven=true};
        static AdService Service(MonetizationLedger ledger=null,Action<MonetizationLedger> persist=null)=>new AdService(ledger??new MonetizationLedger(),persist??(_=>{}),12);
        static AttemptService Attempt()=>new AttemptService(ContentTests.LoadCatalog().GetPrimary("C02").Definition);
        static HintOffer Hint(AttemptService a,int budget=PuzzleSolver.DefaultStateBudget)=>AttemptService.CalculateHint(a.CaptureHintRequest(),budget);
        [Test] public void RetryAdCadenceAlternatesEveryTwoAndThenEveryThreeRetries()
        {
            var cadence=new RetryAdCadence();
            Assert.That(cadence.RecordRetry(),Is.False);Assert.That(cadence.RecordRetry(),Is.True);
            Assert.That(cadence.RecordRetry(),Is.True,"A due ad stays due until it is shown.");cadence.AdShown();
            Assert.That(cadence.RecordRetry(),Is.False);Assert.That(cadence.RecordRetry(),Is.False);
            Assert.That(cadence.RecordRetry(),Is.True);cadence.AdShown();
            Assert.That(cadence.RecordRetry(),Is.False);Assert.That(cadence.RecordRetry(),Is.True);
        }
        [Test] public void RetryingALevelNeverForcesAnInterstitial()
        {
            var service=Service();var retry=Context(AdSurface.Retry,clears:2,node:"C02");
            for(int i=0;i<12;i++)Assert.That(service.TryAutomaticRetry(retry,true),Is.Null);
            Assert.That(service.AutomaticShows,Is.Zero);
        }
        [Test] public void SubscriptionExpiresWithoutBecomingAPermanentPurchase()
        {
            var ledger=new MonetizationLedger();ledger.SetSubscriptionExpiry(PurchaseCatalog.NoAdsWeek,DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds());
            Assert.That(ledger.HasNoAutomaticAds,Is.True);Assert.That(ledger.NoAutomaticAds,Is.False);
            var service=Service(ledger);Assert.That(service.TryAutomatic(Context(clears:2,node:"C02"),true),Is.Null);
            ledger.SetSubscriptionExpiry(PurchaseCatalog.NoAdsWeek,DateTimeOffset.UtcNow.AddSeconds(-1).ToUnixTimeSeconds());
            Assert.That(ledger.HasNoAutomaticAds,Is.False);Assert.That(ledger.ActiveNoAdsSubscription,Is.Null);
            Assert.That(service.TryAutomatic(Context(clears:4,node:"C04"),true),Is.Not.Null);
        }
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
	        [Test] public void VerifiedSubscriptionPersistsExpiryAndUpdatesIdempotently()
	        {
	            int saves=0;var service=Service(persist:_=>saves++);long first=DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds(),renewed=first+86400;
	            Assert.That(service.VerifiedStoreSubscription(PurchaseCatalog.NoAdsWeek,"subscription_a",first,false),Is.False);
	            Assert.That(service.VerifiedStoreSubscription("unknown","subscription_a",first,true),Is.False);
	            Assert.That(service.VerifiedStoreSubscription(PurchaseCatalog.NoAdsWeek,"subscription_a",first,true),Is.True);
	            Assert.That(service.Ledger.SubscriptionExpiry(PurchaseCatalog.NoAdsWeek),Is.EqualTo(first));
	            Assert.That(service.Ledger.Grants.Single().Kind,Is.EqualTo("subscription"));Assert.That(saves,Is.EqualTo(1));
	            Assert.That(service.VerifiedStoreSubscription(PurchaseCatalog.NoAdsWeek,"subscription_a",first,true),Is.True);Assert.That(saves,Is.EqualTo(1));
	            Assert.That(service.VerifiedStoreSubscription(PurchaseCatalog.NoAdsWeek,"subscription_a",renewed,true),Is.True);Assert.That(saves,Is.EqualTo(2));
	            Assert.That(service.VerifiedStoreSubscription(PurchaseCatalog.NoAdsWeek,"subscription_a",0,true),Is.True);Assert.That(saves,Is.EqualTo(3));
	            Assert.That(service.Ledger.HasNoAutomaticAds,Is.False);
	            Assert.That(service.VerifiedStoreSubscription(PurchaseCatalog.NoAdsMonth,"subscription_a",renewed,true),Is.False,"one receipt cannot be replayed for another product");
	        }
	        [Test] public void PublicReceiptEndpointMustBeExactHttpsPath()
	        {
	            Assert.That(PublicReceiptVerifier.ValidEndpoint("https://cascade-sky-gardens.active-seed-4233.chatgpt.site/api/purchase/local-verify"),Is.True);
	            Assert.That(PublicReceiptVerifier.ValidEndpoint("https://vickand.com/api/flying-ladders/purchase/local-verify"),Is.True);
	            Assert.That(PublicReceiptVerifier.ValidEndpoint("https://foreign.example/api/flying-ladders/purchase/local-verify"),Is.False);
	            foreach(var value in new[]{"http://cascade-sky-gardens.active-seed-4233.chatgpt.site/api/purchase/local-verify","https://cascade-sky-gardens.active-seed-4233.chatgpt.site/api/verify","https://user@cascade-sky-gardens.active-seed-4233.chatgpt.site/api/purchase/local-verify","https://cascade-sky-gardens.active-seed-4233.chatgpt.site/api/purchase/local-verify?player=x"})
	                Assert.That(PublicReceiptVerifier.ValidEndpoint(value),Is.False,value);
	        }
        [Test] public void AutomaticAdsRespectEveryGateAndNeverAppearDuringPuzzleFlows()
        {
            foreach(var surface in Enum.GetValues(typeof(AdSurface)).Cast<AdSurface>().Where(s=>s!=AdSurface.Win)) {
                var s=Service();s.AdvanceForeground(180);Assert.That(s.TryAutomatic(Context(surface),true),Is.Null);
            }
            Action<AdContext>[] gates={c=>c.CoreAccepted=false,c=>c.PrivacyReady=false,c=>c.ProviderEnabled=false,
                c=>c.UniqueClears=1,c=>c.SavedWin=false,c=>c.BaseRewardGiven=false,c=>c.ContinueVisible=true,c=>c.Replay=true};
            foreach(var gate in gates){var s=Service();var c=Context(clears:16,node:"C16");gate(c);Assert.That(s.TryAutomatic(c,true),Is.Null);}
            Assert.That(Service().TryAutomatic(Context(clears:1,node:"C01"),true),Is.Null);
            Assert.That(Service().TryAutomatic(Context(clears:3,node:"C03"),true),Is.Null);
        }
        [Test] public void FirstSessionAlternatesCompletedLevelsWithoutATimer()
        {
            var s=new AdService(new MonetizationLedger(),_=>{});
            Assert.That(s.TryAutomatic(Context(clears:1,node:"C01"),true),Is.Null);
            var ticket=s.TryAutomatic(Context(clears:2,node:"C02"),true);Assert.That(ticket,Is.Not.Null);
            s.Opened(ticket.Id);s.Closed(ticket.Id);
            Assert.That(s.TryAutomatic(Context(clears:3,node:"C03"),true),Is.Null);
            Assert.That(s.TryAutomatic(Context(clears:4,node:"C04"),true),Is.Not.Null);
        }
        [Test] public void NotReadySkipsThatWinAndLaterEvenLevelsRemainEligible()
        {
            var s=Service();Assert.That(s.TryAutomatic(Context(clears:16,node:"C16"),false),Is.Null);
            Assert.That(s.TryAutomatic(Context(clears:16,node:"C16"),true),Is.Null);
            Assert.That(s.TryAutomatic(Context(clears:17,node:"C17"),true),Is.Null);
            var first=s.TryAutomatic(Context(clears:18,node:"C18"),true);Assert.That(first,Is.Not.Null);
            Assert.That(s.Opened(first.Id),Is.True);Assert.That(s.Opened(first.Id),Is.False);s.Closed(first.Id);
            var second=s.TryAutomatic(Context(clears:20,node:"C20"),true);Assert.That(second,Is.Not.Null);s.Opened(second.Id);s.Closed(second.Id);
            Assert.That(s.TryAutomatic(Context(clears:22,node:"C22"),true),Is.Not.Null);Assert.That(s.AutomaticShows,Is.EqualTo(2));
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
        [Test] public void ParcelVideoBonusIsVerifiedPersistedAndLimitedToOnePerParcel()
        {
            var s=Service();var context=Context(AdSurface.Inventory);context.RewardSubject="gift:3";
            Assert.That(s.TryRewarded(context,"parcel_bonus",true),Is.Null,"parcel-only grants need a parcel identity");
            context.RewardSubject="parcel:4";var ticket=s.TryRewarded(context,"parcel_bonus",true);Assert.That(ticket,Is.Not.Null);
            Assert.That(ticket.RewardSubject,Is.EqualTo("parcel:4"));s.Opened(ticket.Id);
            Assert.That(s.RewardConfirmed(ticket.Id,"parcel-ad-4",false),Is.False);
            Assert.That(s.RewardConfirmed(ticket.Id,"parcel-ad-4",true),Is.True);
            Assert.That(s.Ledger.BonusCoins,Is.EqualTo(40));Assert.That(s.Ledger.Grants.Single().Kind,Is.EqualTo("coins"));
            Assert.That(s.Ledger.Grants.Single().Product,Is.EqualTo("parcel:4"));Assert.That(s.HasParcelBonus("parcel:4"),Is.True);
            Assert.That(s.TryRewarded(context,"parcel_bonus",true),Is.Null,"a saved parcel cannot earn a second bonus");
            s.Ledger.Validate();var restored=Service(s.Ledger.Copy());
            Assert.That(restored.HasParcelBonus("parcel:4"),Is.True);
            Assert.That(restored.TryRewarded(context,"parcel_bonus",true),Is.Null,"the one-time limit survives reload");
        }
        [Test] public void ParcelVideoBonusCannotBeCommittedWhenItsSaveFails()
        {
            bool fail=true;var s=Service(persist:_=>{if(fail)throw new IOException("disk");});
            var context=Context(AdSurface.Inventory);context.RewardSubject="parcel:2";
            var ticket=s.TryRewarded(context,"parcel_bonus",true);s.Opened(ticket.Id);
            Assert.That(s.RewardConfirmed(ticket.Id,"parcel-ad-2",true),Is.False);
            Assert.That(s.Ledger.BonusCoins,Is.Zero);Assert.That(s.HasParcelBonus("parcel:2"),Is.False);
            fail=false;Assert.That(s.RewardConfirmed(ticket.Id,"parcel-ad-2",true),Is.True);
            Assert.That(s.Ledger.BonusCoins,Is.EqualTo(40));
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
