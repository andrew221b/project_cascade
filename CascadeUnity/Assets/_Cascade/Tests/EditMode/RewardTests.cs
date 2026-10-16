using System;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Solver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class RewardTests
    {
        ContentCatalog catalog;
        string[] nodes;
        [OneTimeSetUp] public void Setup(){catalog=ContentTests.LoadCatalog();nodes=catalog.PrimaryLevels.Select(c=>c.Definition.CampaignNodeId).ToArray();}
        AttemptService Win(string id,string locale="en",int goal=0,bool hint=false)
        {
            var content=catalog.GetPrimary(id);
            if(locale!="en")catalog.TryGetLocalizedBinding(id,locale,out content);
            var a=new AttemptService(content.Definition,content.Locale,new PuzzleGoal(goal));
            if(hint)a.RevealHint(AttemptService.CalculateHint(a.CaptureHintRequest()));
            foreach(var command in PuzzleSolver.FindShortest(a.Resolver,goal:a.Goal).Path)a.Execute(command);
            Assert.That(a.Goal.IsSatisfied(a.Current),Is.True);return a;
        }
        [Test] public void FirstParcelPaysCoinsAndItsReceiptCannotBeRerolledOrFarmed()
        {
            var rewards=new JourneyRewards();rewards.SelectGift("sun");rewards.Synchronize(nodes.Take(2));
            Assert.That(rewards.Parcels,Is.Empty);Assert.That(rewards.OrderProgress,Is.EqualTo(2));
            rewards.Synchronize(nodes.Take(2));Assert.That(rewards.OrderProgress,Is.EqualTo(2));
            rewards.Synchronize(nodes.Take(3));Assert.That(rewards.Parcels.Count,Is.EqualTo(1));
            Assert.That(rewards.Parcels[0].Kind,Is.EqualTo("loot"));Assert.That(rewards.Parcels[0].GiftId,Is.Null);rewards.SelectGift("moon");
            Assert.That(rewards.Parcels[0].Kind,Is.EqualTo("loot"),"An earned receipt cannot be rerolled by changing the next order");
            var service=new AdService(new MonetizationLedger(),_=>{});Assert.That(service.TryClaimParcel(1,rewards),Is.True);Assert.That(service.TryClaimParcel(1,rewards),Is.False);
            rewards.Synchronize(nodes.Take(3));Assert.That(JourneyRewards.ParcelCoins(rewards.Parcels[0]),Is.InRange(12,24));
        }
        [Test] public void PendingParcelsReserveCapacityAndAllFortyClearsCannotGenerateFiller()
        {
            var rewards=new JourneyRewards();rewards.Synchronize(nodes);
            Assert.That(rewards.Parcels.Count,Is.EqualTo(12));Assert.That(rewards.Parcels.All(p=>p.RecipeVersion==6&&p.Loot.Count>=1&&p.Loot.Count<=2),Is.True);
            Assert.That(rewards.Parcels.SelectMany(p=>p.Loot).Count(d=>d.Kind=="cosmetic"),Is.EqualTo(1));
            foreach(string gift in JourneyRewards.Gifts)Assert.That(rewards.EarnedFragments(gift),Is.Zero);
            Assert.That(rewards.Equip("leaf"),Is.False);
            Assert.That(rewards.Parcels.Where(p=>p.Kind=="tool").All(p=>ShopCatalog.TryTool(p.GiftId,out _)),Is.True);
            rewards.Validate();
        }
        [Test] public void FortyClearsAndFourChaptersYieldExactlyTwoThousand()
        {
            var rewards=new JourneyRewards();rewards.Synchronize(nodes);
            Assert.That(rewards.EarnedCoins(nodes),Is.EqualTo(2000));
            Assert.That(2000-5*240-6*120,Is.EqualTo(80));
            foreach(int chapter in Enumerable.Range(0,4))Assert.That(rewards.WorkshopCount(chapter,nodes),Is.EqualTo(10));
        }
        [Test] public void ReplayReservePaysFourForTwoStarsAndOnlyTheIncrementUpToSix()
        {
            var rewards=new JourneyRewards();rewards.Synchronize(nodes.Take(1));int first=rewards.EarnedCoins(nodes);
            rewards.RecordReplayCredit(nodes[0],2);Assert.That(rewards.EarnedCoins(nodes)-first,Is.EqualTo(4));
            rewards.RecordReplayCredit(nodes[0],2);Assert.That(rewards.EarnedCoins(nodes)-first,Is.EqualTo(4));
            rewards.RecordReplayCredit(nodes[0],3);Assert.That(rewards.EarnedCoins(nodes)-first,Is.EqualTo(6));
            for(int i=0;i<100;i++)rewards.RecordReplayCredit(nodes[0],3);
            Assert.Throws<ArgumentException>(()=>rewards.RecordReplayCredit(nodes[0],5));
            Assert.That(rewards.EarnedCoins(nodes)-first,Is.EqualTo(6));Assert.That(rewards.SeenClearNodes.Count,Is.EqualTo(1));
            Assert.That(rewards.Parcels,Is.Empty);rewards.Validate();
        }
        [Test] public void SkippingC26DoesNotPretendThatWorkshopIsComplete()
        {
            var rewards=new JourneyRewards();rewards.Synchronize(nodes.Where(n=>n!="C26"));
            Assert.That(rewards.WorkshopCount(2,nodes),Is.EqualTo(9));Assert.That(rewards.EarnedCoins(nodes),Is.EqualTo(1860));
            rewards.Synchronize(nodes);Assert.That(rewards.WorkshopCount(2,nodes),Is.EqualTo(10));Assert.That(rewards.EarnedCoins(nodes),Is.EqualTo(2000));
        }
        [Test] public void VisitStampsNeverResetOnGapsAndIgnoreRepeatedDates()
        {
            var rewards=new JourneyRewards();var date=new DateTime(2026,10,5);
            Assert.That(rewards.RegisterVisit(date),Is.True);Assert.That(rewards.RegisterVisit(date.AddHours(12)),Is.False);
            Assert.That(rewards.RegisterVisit(date.AddDays(100)),Is.True);Assert.That(rewards.VisitDates.Count,Is.EqualTo(2));
            for(int i=1;i<10;i++)rewards.RegisterVisit(date.AddDays(i));
            Assert.That(rewards.VisitDates.Count,Is.EqualTo(7));rewards.Synchronize(nodes.Take(2));
            Assert.That(rewards.OrderProgress,Is.EqualTo(2));rewards.RegisterVisit(date.AddYears(2));Assert.That(rewards.OrderProgress,Is.EqualTo(2));
        }
        [Test] public void ObjectiveAndLocaleRecordsStaySeparateWhileDeliveryStillCounts()
        {
            var rewards=new JourneyRewards();var normal=Win("C35");var stamped=Win("C35",goal:1);var uk=Win("C35","uk",1);
            rewards.RecordDelivery(normal);rewards.RecordDelivery(stamped);rewards.RecordDelivery(uk);
            Assert.That(rewards.Records.Count,Is.EqualTo(4));
            Assert.That(rewards.Records.Single(r=>r.Locale=="en"&&r.StampGoal==0).BestRoute,Is.EqualTo(2));
            Assert.That(rewards.Records.Single(r=>r.Locale=="en"&&r.StampGoal==1).BestRoute,Is.EqualTo(4));
            Assert.That(rewards.Deliveries.Count,Is.EqualTo(1));Assert.That(rewards.QuestCount(2),Is.EqualTo(1));
            rewards.RecordDelivery(stamped);Assert.That(rewards.QuestCount(2),Is.EqualTo(1));
        }
        [Test] public void HintAndUndoAnnotationsDoNotRewriteTheMeaningOfBestRoute()
        {
            var rewards=new JourneyRewards();var hinted=Win("C02",hint:true);rewards.RecordDelivery(hinted);
            var record=rewards.Records.Single();Assert.That(record.BestWithoutHint,Is.Null);Assert.That(record.BestClean,Is.Null);
            var undo=new AttemptService(catalog.GetPrimary("C02").Definition);undo.Execute(PuzzleCommand.Swap(1));Assert.That(undo.Undo(),Is.True);foreach(var command in PuzzleSolver.FindShortest(undo.Resolver).Path)undo.Execute(command);rewards.RecordDelivery(undo);
            Assert.That(record.BestWithoutHint,Is.EqualTo(2));Assert.That(record.BestClean,Is.Null);
            rewards.RecordDelivery(Win("C02"));Assert.That(record.BestClean,Is.EqualTo(2));
        }
        [Test] public void DeviceRequestsUseSuccessfulUniqueNodesRatherThanRepeatedActions()
        {
            var rewards=new JourneyRewards();var repair=Win("C18");rewards.RecordDelivery(repair);rewards.RecordDelivery(repair);
            Assert.That(rewards.QuestCount(0),Is.EqualTo(1));
            var playing=new AttemptService(catalog.GetPrimary("C18").Definition);rewards.RecordDelivery(playing);
            Assert.That(rewards.Deliveries.Count,Is.EqualTo(1));
            rewards.RecordDelivery(Win("C33"));Assert.That(rewards.QuestCount(1),Is.EqualTo(1));
            Assert.That(rewards.CompletedRequests,Is.EqualTo(1));
        }
        [Test] public void SavedClaimsCannotReopenAndOlderProfileCanAcquireNewFields()
        {
            var profile=new PlayerProfile();foreach(string id in nodes.Take(9))profile.RecordClear(Win(id));
            profile.Rewards.Synchronize(profile.Clears.Select(c=>c.CampaignNodeId));profile.Rewards.ClaimParcel(1);
            profile.Validate();string encoded=ProfileStore.Encode(profile);var envelope=JObject.Parse(encoded);
            var restored=JsonConvert.DeserializeObject<PlayerProfile>((string)envelope["payload"]);restored.Validate();
            Assert.That(restored.Rewards.ClaimParcel(1),Is.False);Assert.That(restored.Rewards.Parcels.Count,Is.EqualTo(3));
            var old=JObject.Parse((string)envelope["payload"]);old.Remove("Rewards");
            restored=old.ToObject<PlayerProfile>();restored.Validate();restored.Rewards.Synchronize(restored.Clears.Select(c=>c.CampaignNodeId));
            Assert.That(restored.Rewards.Parcels.Count,Is.EqualTo(3));restored.Rewards.Synchronize(restored.Clears.Select(c=>c.CampaignNodeId));
            Assert.That(restored.Rewards.Parcels.Count,Is.EqualTo(3));
            Assert.That(((string)envelope["payload"]).Contains("KeeperPoints"),Is.False,"Derived presentation values are not save authority");
        }
        [Test] public void MissingClearAuthorityAndInvalidGiftReceiptsAreRejected()
        {
            var profile=new PlayerProfile();profile.Rewards.Synchronize(nodes.Take(3));Assert.Throws<System.IO.InvalidDataException>(()=>profile.Validate());
            var rewards=new JourneyRewards();rewards.Synchronize(nodes.Take(3));rewards.Parcels[0].GiftId="random";
            Assert.Throws<ArgumentException>(()=>rewards.Validate());
        }
        [Test] public void LaterParcelsAreRareFixedPrizesAndReplayCannotRerollThem()
        {
            var rewards=new JourneyRewards();var clearIds=Enumerable.Range(1,84).Select(i=>"C"+i).ToArray();
            rewards.Synchronize(clearIds);rewards.Synchronize(clearIds.Reverse());
            // Existing published receipts retain version0 rather than being rerolled.
            rewards.Parcels=rewards.Parcels.Select(p=>p.Order<=12?new ParcelReceipt{Order=p.Order,GiftId=JourneyRewards.Gifts[(p.Order-1)/3]}:JourneyRewards.BonusParcel(p.Order,0)).ToList();
            Assert.That(rewards.Parcels.Count,Is.EqualTo(16));
            Assert.That(rewards.Parcels.Where(p=>p.Order<=12).All(p=>p.Kind==null),Is.True);
            Assert.That(rewards.Parcels.Where(p=>p.Order>12).Select(p=>p.Order),Is.EqualTo(new[]{16,20,24,28}));
            Assert.That(rewards.Parcels.Where(p=>p.Kind=="coins").Count(),Is.EqualTo(3));
            var rare=rewards.Parcels.Single(p=>p.Kind=="cosmetic");
            Assert.That(rare.GiftId,Is.EqualTo("sky_pilot"));Assert.That(rewards.Owns(rare.GiftId),Is.False);
            foreach(var parcel in rewards.Parcels.Where(p=>p.Order>12))Assert.That(rewards.ClaimParcel(parcel.Order),Is.True);
            Assert.That(rewards.Owns("sky_pilot"),Is.True);
            Assert.That(rewards.ClaimParcel(rare.Order),Is.False);
            Assert.That(rewards.EarnedCoins(clearIds),Is.EqualTo(84*40+800+60));
            rewards.Validate();
        }
        [Test] public void AlreadyBoughtRareLookConvertsOnceToSmallCoinsAndCloudKeepsClaim()
        {
            var ids=Enumerable.Range(1,84).Select(i=>"C"+i).ToArray();
            var local=new PlayerProfile();foreach(var id in ids)local.Clears.Add(new ClearReceipt{
                CampaignNodeId=id,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64)});
            local.Rewards.Synchronize(ids);
            local.Rewards.Parcels=local.Rewards.Parcels.Select(p=>p.Order<=12?new ParcelReceipt{Order=p.Order,GiftId=JourneyRewards.Gifts[(p.Order-1)/3],RecipeVersion=3}:JourneyRewards.BonusParcel(p.Order,3)).ToList();
            local.Monetization.Cosmetics.Add("sky_pilot");local.Monetization.SpentCoins=ShopCatalog.TrophyPrice("sky_pilot");
            Assert.That(local.Rewards.ClaimParcel(28,local.Monetization.Cosmetics),Is.True);
            Assert.That(local.Rewards.Parcels.Single(p=>p.Order==28).DuplicateCoins,Is.True);
            int earned=local.Rewards.EarnedCoins(ids);Assert.That(earned,Is.EqualTo(84*40+800+40));
            int spendableBefore=earned-local.Monetization.SpentCoins;
            local.Validate();
            var merged=CloudReconciler.Merge(new PlayerProfile(),new PlayerProfile(),local,ids);
            Assert.That(merged.EconomyConflict,Is.False);
            Assert.That(merged.Profile.Rewards.Parcels.Single(p=>p.Order==28).DuplicateCoins,Is.True);
            Assert.That(merged.Profile.Rewards.EarnedCoins(ids)-merged.Profile.Monetization.SpentCoins,Is.EqualTo(spendableBefore),"Price migration preserves the spendable wallet, including the one duplicate reward");
        }
    }
}
