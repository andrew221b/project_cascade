using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Persistence;
using Newtonsoft.Json;
using NUnit.Framework;
namespace Cascade.Tests
{
    public sealed class ParcelLootTests
    {
        static string[] Nodes=>Enumerable.Range(1,540).Select(i=>"C"+i.ToString("D2")).ToArray();
        static PlayerProfile Profile(int clears=540){var p=new PlayerProfile();foreach(string n in Nodes.Take(clears))p.Clears.Add(new ClearReceipt{CampaignNodeId=n,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64)});p.Rewards.Synchronize(Nodes.Take(clears));p.MigrateCoinEconomy();return p;}
        static PlayerProfile Copy(PlayerProfile p)=>JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(p));
        static AdService Service(PlayerProfile p)=>new AdService(p.Monetization,n=>p.Monetization=n);
        static AdTicket Ad(AdService s,int order)=>s.TryRewarded(new AdContext{Surface=AdSurface.Inventory,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true,RewardSubject=MonetizationLedger.ParcelSubject(order)},"parcel_bonus",true);
        static void Verify(AdService s,int order){var t=Ad(s,order);Assert.That(t,Is.Not.Null);s.Opened(t.Id);Assert.That(s.RewardConfirmed(t.Id,"video_"+t.Id,true),Is.True);s.Closed(t.Id);}
        [Test] public void TwoRoundsHaveCorrectCardCountsMostlyGoldAndRareEarlyAndFinalTrophies()
        {
            var p=Profile();p.Validate();Assert.That(p.Rewards.Parcels.Count,Is.EqualTo(54));
            foreach(var r in p.Rewards.Parcels){Assert.That(r.Loot.Count,Is.InRange(1,2));Assert.That(ParcelLoot.Bonus(r.Order).Count,Is.InRange(2,3));Assert.That(ParcelLoot.Bonus(r.Order).Where(d=>d.Kind=="coins").Sum(d=>d.Amount),Is.EqualTo(40));}
            Assert.That(p.Rewards.Parcels.SelectMany(r=>r.Loot).Count(d=>d.Kind=="cosmetic"),Is.EqualTo(2));
            Assert.That(p.Rewards.Parcels.SelectMany(r=>r.Loot).Count(d=>d.Kind=="coins"),Is.GreaterThan(p.Rewards.Parcels.SelectMany(r=>r.Loot).Count(d=>d.Kind!="coins")));
        }
        [Test] public void ReopeningReloadAndModeChangesCannotReroll()
        {
            var p=Profile();string before=JsonConvert.SerializeObject(p.Rewards.Parcels);p.Rewards.Synchronize(Nodes.Reverse());
            Assert.That(JsonConvert.SerializeObject(Copy(p).Rewards.Parcels),Is.EqualTo(before));
            Assert.That(p.Rewards.Parcels.Select(r=>r.Loot.Where(d=>d.Kind=="coins").Sum(d=>d.Amount)).Distinct().Count(),Is.GreaterThan(5));
        }
        [Test] public void AtomicClaimRollsBackOnDiskFailureAndRetriesExactlyOnce()
        {
            var p=Profile();var receipt=p.Rewards.Parcels.First(r=>r.Loot.Any(d=>d.Kind=="tool"));bool fail=true;
            var service=new AdService(p.Monetization,n=>{if(fail)throw new IOException("full disk");p.Monetization=n;});string before=JsonConvert.SerializeObject(receipt);
            Assert.That(service.TryClaimParcel(receipt.Order,p.Rewards),Is.False);Assert.That(JsonConvert.SerializeObject(receipt),Is.EqualTo(before));Assert.That(p.Monetization.Grants,Is.Empty);
            fail=false;Assert.That(service.TryClaimParcel(receipt.Order,p.Rewards),Is.True);Assert.That(service.TryClaimParcel(receipt.Order,p.Rewards),Is.False);p.Validate();
        }
        [Test] public void OwnedTrophyBecomesHelpEvenWhenEveryToolAlreadyHasFive()
        {
            var p=Profile(9);p.Monetization.Cosmetics.Add("leaf");var s=Service(p);Assert.That(s.TryClaimParcel(3,p.Rewards),Is.True);
            Assert.That(p.Rewards.Parcels.Single(r=>r.Order==3).Loot.Last().Kind,Is.EqualTo("tool"));Assert.That(p.Monetization.WayfinderScrolls,Is.EqualTo(1));p.Validate();
            p=Profile(9);p.Monetization.Cosmetics.Add("leaf");p.Monetization.WayfinderScrolls=p.Monetization.UndoSpools=p.Monetization.ReturnHourglasses=p.Monetization.SignalLanterns=p.Monetization.LandingLenses=p.Monetization.RepairKits=5;
            Assert.That(Service(p).TryClaimParcel(3,p.Rewards),Is.True);Assert.That(p.Rewards.Parcels.Single(r=>r.Order==3).Loot.Last().Kind,Is.EqualTo("tool"));Assert.That(p.Monetization.WayfinderScrolls,Is.EqualTo(6));p.Validate();
        }
        [Test] public void FailedCancelledAndUnverifiedAdsGrantNothingAndSecondVideoCannotRepeat()
        {
            var p=Profile(9);var s=Service(p);s.TryClaimParcel(3,p.Rewards);Assert.That(s.TryClaimParcelBonus(3,p.Rewards),Is.False);
            var t=Ad(s,3);s.Opened(t.Id);Assert.That(s.RewardConfirmed(t.Id,"unverified",false),Is.False);s.Failed(t.Id);
            Assert.That(p.Monetization.BonusCoins,Is.Zero);Assert.That(p.Rewards.Parcels.Single(r=>r.Order==3).BonusClaimed,Is.False);
            Verify(s,3);Assert.That(Ad(s,3),Is.Null);Assert.That(s.TryClaimParcelBonus(3,p.Rewards),Is.True);Assert.That(s.TryClaimParcelBonus(3,p.Rewards),Is.False);Assert.That(p.Monetization.BonusCoins,Is.EqualTo(40));p.Validate();
        }
        [Test] public void BonusProofSurvivesReloadBeforeClaimAndHelpReceiptPersistsAtomically()
        {
            var p=Profile();int order=p.Rewards.Parcels.First(r=>ParcelLoot.Bonus(r.Order).Any(d=>d.Kind=="tool")).Order;var s=Service(p);s.TryClaimParcel(order,p.Rewards);Verify(s,order);
            p=Copy(p);s=Service(p);Assert.That(s.HasParcelBonus(MonetizationLedger.ParcelSubject(order)),Is.True);Assert.That(s.TryClaimParcelBonus(order,p.Rewards),Is.True);p.Validate();Copy(p).Validate();
            Assert.That(p.Monetization.Grants.Count(g=>g.Id.Contains("_bonus_")),Is.EqualTo(1));
        }
        [Test] public void MissingProofOrHelpGrantMakesForgedBonusClaimInvalid()
        {
            var p=Profile(3);var r=p.Rewards.Parcels[0];Service(p).TryClaimParcel(1,p.Rewards);r.BonusClaimed=true;r.BonusLoot=ParcelLoot.Bonus(1);
            Assert.Throws<ArgumentException>(()=>p.Validate());
        }
        [Test] public void TwoDevicesClaimingSameBaseMergeOneInventoryReceipt()
        {
            var baseline=Profile();int order=baseline.Rewards.Parcels.First(r=>r.Loot.Any(d=>d.Kind=="tool")).Order;var remote=Copy(baseline);var local=Copy(baseline);
            Service(remote).TryClaimParcel(order,remote.Rewards);Service(local).TryClaimParcel(order,local.Rewards);
            var result=CloudReconciler.Merge(remote,baseline,local,Nodes);Assert.That(result.EconomyConflict,Is.False);result.Profile.Validate();Assert.That(result.Profile.Monetization.Grants.Count,Is.EqualTo(1));
        }
        [Test] public void BonusGoldDoesNotPayTwiceOnCloudMergeOrClaim()
        {
            var baseline=Profile();var remote=Copy(baseline);var s=Service(remote);s.TryClaimParcel(1,remote.Rewards);Verify(s,1);s.TryClaimParcelBonus(1,remote.Rewards);
            var local=Copy(remote);var merged=CloudReconciler.Merge(remote,baseline,local,Nodes);Assert.That(merged.EconomyConflict,Is.False);Assert.That(merged.Profile.Monetization.BonusCoins,Is.EqualTo(40));merged.Profile.Validate();
        }
        [Test] public void AccountImportPreservesCampaignLootAndDropsUnverifiedVideoHelp()
        {
            var p=Profile();int order=p.Rewards.Parcels.First(r=>ParcelLoot.Bonus(r.Order).Any(d=>d.Kind=="tool")).Order;var s=Service(p);
            s.TryClaimParcel(order,p.Rewards);Verify(s,order);s.TryClaimParcelBonus(order,p.Rewards);p.Validate();
            var result=CloudReconciler.ImportLocal(p,Nodes);Assert.That(result.EconomyConflict,Is.False);result.Profile.Validate();
            Assert.That(result.Profile.Monetization.BonusCoins,Is.Zero);Assert.That(result.Profile.Monetization.Grants.Any(g=>g.Id.Contains("_bonus_")),Is.False);
            Assert.That(result.Profile.Rewards.Parcels.Single(r=>r.Order==order).BonusClaimed,Is.False);
            Assert.That(result.Profile.Rewards.Parcels.Single(r=>r.Order==order).Claimed,Is.True);
        }
        [Test] public void OptionalFieldsDoNotChangePublishedLegacyReceiptJson()
        {
            var r=JourneyRewards.VarietyParcel(1);Assert.That(JsonConvert.SerializeObject(r),Does.Not.Contain("Loot"));Assert.That(JsonConvert.SerializeObject(r),Does.Not.Contain("BonusClaimed"));
        }
    }
}
