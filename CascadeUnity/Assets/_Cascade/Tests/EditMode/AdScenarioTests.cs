using System;
using System.IO;
using Cascade.Application;
using Newtonsoft.Json;
using NUnit.Framework;
namespace Cascade.Tests
{
    public sealed class AdScenarioTests
    {
        static AdContext Context(string subject=null,int clears=2)=>new AdContext{Surface=subject==null?AdSurface.Win:AdSurface.Inventory,NodeId="test_"+clears,UniqueClears=clears,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true,SavedWin=true,BaseRewardGiven=true,RewardSubject=subject};
        [Test] public void EverySecondCompletedLevelIncludesWorldFinalAndIgnoresRetry()
        {
            var service=new AdService(new MonetizationLedger(),_=>{});
            for(int clear=1;clear<=6;clear++){
                var context=Context(clears:clear);context.ChapterFinal=clear==6;
                var ticket=service.TryAutomatic(context,true);Assert.That(ticket!=null,Is.EqualTo(clear%2==0));
                if(ticket!=null){service.Opened(ticket.Id);service.Closed(ticket.Id);}
                Assert.That(service.TryAutomatic(context,true),Is.Null,"A repeated continuation cannot show another break");
            }
            Assert.That(service.AutomaticShows,Is.EqualTo(3));Assert.That(service.TryAutomaticRetry(Context(),true),Is.Null);
        }
        [Test] public void VerifiedParcelOpeningIsScopedDurableAndAwardsNoCurrencyOrHints()
        {
            MonetizationLedger saved=null;var service=new AdService(new MonetizationLedger(),candidate=>saved=candidate.Copy());
            var ticket=service.TryRewarded(Context("parcel:1"),"parcel_open",true);Assert.That(ticket,Is.Not.Null);
            service.Opened(ticket.Id);Assert.That(service.RewardConfirmed(ticket.Id,"access_1",true),Is.True);service.Closed(ticket.Id);
            Assert.That(service.Ledger.BonusCoins,Is.Zero);Assert.That(service.Ledger.HintTokens,Is.Zero);
            var restored=JsonConvert.DeserializeObject<MonetizationLedger>(JsonConvert.SerializeObject(saved));restored.Validate();var reloaded=new AdService(restored,_=>{});
            Assert.That(reloaded.HasParcelOpen("parcel:1"),Is.True);Assert.That(reloaded.HasParcelOpen("parcel:2"),Is.False);
            Assert.That(reloaded.TryRewarded(Context("parcel:1"),"parcel_open",true),Is.Null);
            Assert.That(reloaded.TryRewarded(Context("parcel:2"),"parcel_open",true),Is.Not.Null);
        }
        [Test] public void NoAdsRemovesOnlyInterstitialAndKeepsBothParcelRewardedOffers()
        {
            var service=new AdService(new MonetizationLedger{NoAutomaticAds=true},_=>{});
            Assert.That(service.TryAutomatic(Context(),true),Is.Null);
            var open=service.TryRewarded(Context("parcel:1"),"parcel_open",true);Assert.That(open,Is.Not.Null);service.Opened(open.Id);Assert.That(service.RewardConfirmed(open.Id,"open",true),Is.True);service.Closed(open.Id);
            var more=service.TryRewarded(Context("parcel:1"),"parcel_bonus",true);Assert.That(more,Is.Not.Null);service.Opened(more.Id);Assert.That(service.RewardConfirmed(more.Id,"bonus",true),Is.True);service.Closed(more.Id);
            Assert.That(service.HasParcelOpen("parcel:1"),Is.True);Assert.That(service.HasParcelBonus("parcel:1"),Is.True);Assert.That(service.Ledger.HasNoAutomaticAds,Is.True);
        }
        [Test] public void CancelCannotOpenAndLateRewardRemainsBoundToItsOwnParcel()
        {
            var service=new AdService(new MonetizationLedger(),_=>{});var cancelled=service.TryRewarded(Context("parcel:1"),"parcel_open",true);service.Opened(cancelled.Id);service.Failed(cancelled.Id);
            Assert.That(service.RewardConfirmed(cancelled.Id,"cancelled",true),Is.False);Assert.That(service.HasParcelOpen("parcel:1"),Is.False);
            var completed=service.TryRewarded(Context("parcel:1"),"parcel_open",true);service.Opened(completed.Id);service.Closed(completed.Id);
            Assert.That(service.RewardConfirmed(completed.Id,"late",true),Is.True);Assert.That(service.RewardConfirmed(completed.Id,"again",true),Is.False);Assert.That(service.HasParcelOpen("parcel:2"),Is.False);Assert.That(service.Ledger.Grants.Count,Is.EqualTo(1));
        }
        [Test] public void FailedSaveKeepsParcelLockedAndAllowsRetryWithoutDuplicateReward()
        {
            bool fail=true;var service=new AdService(new MonetizationLedger(),_=>{if(fail)throw new IOException("Full disk");});var ticket=service.TryRewarded(Context("parcel:1"),"parcel_open",true);service.Opened(ticket.Id);
            Assert.That(service.RewardConfirmed(ticket.Id,"proof",true),Is.False);Assert.That(service.HasParcelOpen("parcel:1"),Is.False);Assert.That(service.Ledger.Grants,Is.Empty);
            fail=false;Assert.That(service.RewardConfirmed(ticket.Id,"proof",true),Is.True);Assert.That(service.RewardConfirmed(ticket.Id,"proof",true),Is.False);Assert.That(service.Ledger.Grants.Count,Is.EqualTo(1));
        }
        [Test] public void ParcelOpeningRejectsForgedDuplicateAndMalformedReceipts()
        {
            var service=new AdService(new MonetizationLedger(),_=>{});Assert.That(service.TryRewarded(Context("parcel:01"),"parcel_open",true),Is.Null);
            var ticket=service.TryRewarded(Context("parcel:1"),"parcel_open",true);service.Opened(ticket.Id);Assert.That(service.RewardConfirmed(ticket.Id,"forged",false),Is.False);
            var ledger=new MonetizationLedger();ledger.Grants.Add(new GrantReceipt{Id="a",Kind="parcel_open",Product="parcel:1"});ledger.Grants.Add(new GrantReceipt{Id="b",Kind="parcel_open",Product="parcel:1"});Assert.Throws<ArgumentException>(()=>ledger.Validate());
        }
    }
}
