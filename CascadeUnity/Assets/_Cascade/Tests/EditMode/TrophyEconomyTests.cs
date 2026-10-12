using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Persistence;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class TrophyEconomyTests
    {
        static string[] Nodes(int count)=>Enumerable.Range(1,count).Select(i=>"C"+i.ToString("D2")).ToArray();
        static PlayerProfile Earned(int count)
        {
            var profile=new PlayerProfile();foreach(var node in Nodes(count))profile.Clears.Add(new ClearReceipt{
                CampaignNodeId=node,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64)});
            profile.Rewards.Synchronize(Nodes(count));
            profile.Rewards.Parcels=profile.Rewards.Parcels.Select(p=>p.Order<=12?new ParcelReceipt{Order=p.Order,GiftId=JourneyRewards.Gifts[(p.Order-1)/3],RecipeVersion=3}:JourneyRewards.BonusParcel(p.Order,3)).ToList();return profile;
        }
        static PlayerProfile Copy(PlayerProfile profile)=>JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(profile));
        static AdService Service(PlayerProfile profile)=>new AdService(profile.Monetization,next=>profile.Monetization=next);
        static PlayerProfile AddCurrentCosmeticParcel(PlayerProfile profile,int order)
        {profile.Rewards.Parcels.RemoveAll(p=>p.Order==order);profile.Rewards.Parcels.Add(JourneyRewards.VarietyParcel(order));return profile;}

        [TestCase(0,0)] [TestCase(9,360)] [TestCase(10,500)] [TestCase(11,540)]
        [TestCase(39,1860)] [TestCase(40,2000)] [TestCase(160,8000)] [TestCase(540,27000)]
        public void PublishedClearMilestonesKeepEarlyIncomeAndContinueToCampaignEnd(int count,int expected)
            =>Assert.That(Earned(count).Rewards.EarnedCoins(Nodes(540)),Is.EqualTo(expected));

        [Test] public void NewWalletUsesLowerClearRateWhileLegacyWalletKeepsItsEarnedCoins()
        {
            var fresh=Earned(10);fresh.Rewards.CoinEconomyRevision=1;
            Assert.That(fresh.Rewards.EarnedCoins(Nodes(10)),Is.EqualTo(250));
            var legacy=Earned(10);
            Assert.That(legacy.Rewards.EarnedCoins(Nodes(10)),Is.EqualTo(500));
            Assert.That(legacy.Rewards.CoinIncomeCarryover,Is.EqualTo(250));
        }

        [Test] public void LegacyBalanceAndToolInventorySurvivePriceMigration()
        {
            var profile=Earned(10);profile.Monetization.SpentCoins=80;profile.Monetization.UndoSpools=1;
            profile.MigrateCoinEconomy();
            Assert.That(profile.Rewards.EarnedCoins(Nodes(10))-profile.Monetization.SpentCoins,Is.EqualTo(420));
            Assert.That(profile.Monetization.SpentCoins,Is.EqualTo(120));
            Assert.That(profile.Monetization.UndoSpools,Is.EqualTo(1));profile.Validate();
        }

        [Test] public void FirstFragmentClaimAddsCoinsOnlyAfterDurableSaveAndCannotRepeat()
        {
            var profile=Earned(3);int income=profile.Rewards.EarnedCoins(Nodes(3));bool fail=true;
            var service=new AdService(profile.Monetization,next=>{if(fail)throw new IOException("disk");profile.Monetization=next;});
            Assert.That(service.TryClaimParcel(1,profile.Rewards),Is.False);
            Assert.That(profile.Rewards.Fragments("leaf"),Is.Zero);Assert.That(profile.Rewards.EarnedCoins(Nodes(3)),Is.EqualTo(income));
            fail=false;Assert.That(service.TryClaimParcel(1,profile.Rewards),Is.True);
            var saved=Copy(profile);Assert.That(saved.Rewards.Fragments("leaf"),Is.EqualTo(1));
            Assert.That(saved.Rewards.EarnedCoins(Nodes(3)),Is.EqualTo(income+20));
            saved.Rewards.Synchronize(Nodes(3));Assert.That(Service(saved).TryClaimParcel(1,saved.Rewards),Is.False);saved.Validate();
        }

        [Test] public void EveryNewParcelHasUsefulIncomeAndCampaignBonusIsBounded()
        {
            var current=Earned(540);var legacy=Copy(current);
            foreach(var parcel in legacy.Rewards.Parcels)parcel.RecipeVersion=parcel.Order<=12?0:2;
            foreach(var profile in new[]{current,legacy})foreach(var parcel in profile.Rewards.Parcels)
                Assert.That(Service(profile).TryClaimParcel(parcel.Order,profile.Rewards),Is.True);
            Assert.That(current.Rewards.Parcels.All(p=>JourneyRewards.ParcelCoins(p)>=20),Is.True);
            Assert.That(current.Rewards.EarnedCoins(Nodes(540))-legacy.Rewards.EarnedCoins(Nodes(540)),Is.EqualTo(1080));
            current.Validate();legacy.Validate();
        }

        [Test] public void TwoDevicesClaimOneFragmentWithOneCoinBonusAndOldRecipesStayOld()
        {
            var baseline=Earned(3);var remote=Copy(baseline);var local=Copy(baseline);
            Assert.That(Service(remote).TryClaimParcel(1,remote.Rewards),Is.True);
            Assert.That(Service(local).TryClaimParcel(1,local.Rewards),Is.True);
            var merge=CloudReconciler.Merge(remote,baseline,local,Nodes(3));Assert.That(merge.EconomyConflict,Is.False);
            Assert.That(merge.Profile.Rewards.EarnedCoins(Nodes(3)),Is.EqualTo(140));merge.Profile.Validate();
            var old=Earned(3);old.Rewards.Parcels[0].RecipeVersion=0;
            Assert.That(Service(old).TryClaimParcel(1,old.Rewards),Is.True);
            Assert.That(old.Rewards.EarnedCoins(Nodes(3)),Is.EqualTo(120));
            string before=JsonConvert.SerializeObject(old.Rewards.Parcels[0]);old.Clears.AddRange(Earned(6).Clears.Skip(3));old.Rewards.Synchronize(Nodes(6));
            Assert.That(JsonConvert.SerializeObject(old.Rewards.Parcels[0]),Is.EqualTo(before));old.Validate();
            var mismatch=CloudReconciler.Merge(old,new PlayerProfile(),local,Nodes(3));Assert.That(mismatch.EconomyConflict,Is.True);
            Assert.That(mismatch.Profile.Rewards.Parcels[0].RecipeVersion,Is.Zero);
        }

        [Test] public void MilestonesIgnoreRouteOrderUnknownsDuplicatesAndModeReplay()
        {
            var nodes=Nodes(540);var clears=new[]{"C01","C02","C03","C185","C186","C187","C188","C189","C190","C191"};
            var profile=new PlayerProfile();profile.Rewards.Synchronize(clears.Concat(new[]{"unpublished"}));
            Assert.That(profile.Rewards.EarnedCoins(nodes),Is.EqualTo(500));
            Assert.That(profile.Rewards.EarnedCoins(nodes.Reverse().Concat(nodes).ToArray()),Is.EqualTo(500));
            string parcels=JsonConvert.SerializeObject(profile.Rewards.Parcels);profile.SelectJourney("Words");
            profile.Rewards.Synchronize(clears.Reverse());profile.SelectJourney("Mixed");profile.Rewards.Synchronize(clears);
            Assert.That(profile.Rewards.EarnedCoins(nodes),Is.EqualTo(500));
            Assert.That(JsonConvert.SerializeObject(profile.Rewards.Parcels),Is.EqualTo(parcels));
        }

        [Test] public void LegacyCompleteGroupIncomeNeverDecreasesForScatteredClears()
        {
            var nodes=Nodes(40);for(int mask=0;mask<16;mask++){
                var rewards=new JourneyRewards();var clears=nodes.Where((node,index)=>(mask&(1<<(index/10)))!=0).ToArray();
                rewards.Synchronize(clears);int legacy=40*clears.Length+100*Enumerable.Range(0,4).Count(c=>rewards.WorkshopCount(c,nodes)==10);
                Assert.That(rewards.EarnedCoins(nodes),Is.GreaterThanOrEqualTo(legacy));
            }
        }

        [Test] public void RecipeThreeKeepsFixedExpansionTrackAndAddsUsefulCoins()
        {
            var profile=Earned(540);Assert.That(profile.Rewards.Parcels.Take(12).All(p=>p.RecipeVersion==3&&p.Kind==null),Is.True);
            Assert.That(profile.Rewards.Parcels.Where(p=>p.Order>12).All(p=>p.RecipeVersion==3),Is.True);
            Assert.That(profile.Rewards.Parcels.Where(p=>ShopCatalog.ExpansionTrophyIds.Contains(p.GiftId)).Select(p=>p.Order*3),
                Is.EqualTo(new[]{60,96,168,204,276,312,384,420,492,528}));
            Assert.That(profile.Rewards.Parcels.Where(p=>ShopCatalog.BonusTrophyIds.Contains(p.GiftId)).Select(p=>p.Order*3),
                Is.EqualTo(new[]{84,192,300,408,516}));
            Assert.That(profile.Rewards.Parcels.Where(p=>ShopCatalog.NewTrophyIds.Contains(p.GiftId)).Select(p=>p.Order*3),
                Is.EqualTo(new[]{132,240,348,456}));
            for(int i=0;i<ShopCatalog.ExpansionTrophyIds.Length;i++){
                var receipt=JourneyRewards.BonusParcel(20+36*(i/2)+12*(i%2),2);Assert.That(receipt.GiftId,Is.EqualTo(ShopCatalog.ExpansionTrophyIds[i]));
            }
            string before=JsonConvert.SerializeObject(profile.Rewards.Parcels);profile.Rewards.Synchronize(Nodes(540).Reverse());
            Assert.That(JsonConvert.SerializeObject(profile.Rewards.Parcels),Is.EqualTo(before));profile.Validate();
        }

        [Test] public void ExistingRecipeZeroAndOneReceiptsRemainByteStableWhenNewOrdersMint()
        {
            foreach(int version in new[]{0,1}){
                var profile=Earned(60);profile.Rewards.Parcels=profile.Rewards.Parcels.Select(p=>p.Order<=12?p:JourneyRewards.BonusParcel(p.Order,version)).ToList();
                Assert.That(Service(profile).TryClaimParcel(20,profile.Rewards),Is.True);
                string original=JsonConvert.SerializeObject(profile.Rewards.Parcels);profile.Rewards.Synchronize(Nodes(72));
                Assert.That(JsonConvert.SerializeObject(profile.Rewards.Parcels.Take(14)),Is.EqualTo(original));
                Assert.That(profile.Rewards.Parcels.Single(p=>p.Order==24).RecipeVersion,Is.EqualTo(5));
            }
        }

        [Test] public void NewLootBoxesAreMostlyCoinsWithOccasionalToolsAndRareTrophies()
        {
            var rewards=new JourneyRewards();rewards.Synchronize(Nodes(540));rewards.Validate();
            Assert.That(rewards.Parcels.Count,Is.EqualTo(54));
            Assert.That(rewards.Parcels.Count(p=>p.Kind=="coins"),Is.EqualTo(44));
            Assert.That(rewards.Parcels.Count(p=>p.Kind=="tool"),Is.EqualTo(8));
            Assert.That(rewards.Parcels.Count(p=>p.Kind=="cosmetic"),Is.EqualTo(2));
            Assert.That(rewards.Parcels.Where(p=>p.Kind=="coins").All(p=>JourneyRewards.ParcelCoins(p)==12),Is.True);
            Assert.That(rewards.Parcels.Where(p=>p.Kind=="cosmetic").All(p=>ShopCatalog.IsScenery(p.GiftId)||ShopCatalog.IsFrame(p.GiftId)||ShopCatalog.IsParcelExclusive(p.GiftId)),Is.True);
            var duplicate=JourneyRewards.VarietyParcel(44);duplicate.DuplicateCoins=true;
            Assert.That(JourneyRewards.ParcelCoins(duplicate),Is.EqualTo(20));
        }

        [Test] public void CloudMergeKeepsClaimedOldParcelOrUsesNewRecipeWhenBothAreUnclaimed()
        {
            var baseline=Earned(60);var remote=Copy(baseline);var local=Copy(baseline);int order=20;
            local.Rewards.Parcels[local.Rewards.Parcels.FindIndex(p=>p.Order==order)]=JourneyRewards.VarietyParcel(order);
            var merged=CloudReconciler.Merge(remote,baseline,local,Nodes(60));
            Assert.That(merged.EconomyConflict,Is.False);Assert.That(merged.Profile.Rewards.Parcels.Single(p=>p.Order==order).RecipeVersion,Is.EqualTo(5));

            remote=Copy(baseline);local=Copy(baseline);remote.Rewards.Parcels.Single(p=>p.Order==order).Claimed=true;
            local.Rewards.Parcels[local.Rewards.Parcels.FindIndex(p=>p.Order==order)]=JourneyRewards.VarietyParcel(order);
            merged=CloudReconciler.Merge(remote,baseline,local,Nodes(60));
            Assert.That(merged.EconomyConflict,Is.False);Assert.That(merged.Profile.Rewards.Parcels.Single(p=>p.Order==order).RecipeVersion,Is.EqualTo(3));
            Assert.That(merged.Profile.Rewards.Parcels.Single(p=>p.Order==order).Claimed,Is.True);
        }

        [Test] public void NewCosmeticClaimFailureRollsBackAndOwnedFindConvertsOnlyOnce()
        {
            var profile=Earned(60);bool fail=true;var service=new AdService(profile.Monetization,next=>{
                if(fail)throw new IOException("disk");profile.Monetization=next;});
            Assert.That(service.TryClaimParcel(20,profile.Rewards),Is.False);Assert.That(profile.Rewards.Owns("mushroom_cap"),Is.False);
            Assert.That(profile.Rewards.EquippedGift,Is.Null);fail=false;
            Assert.That(service.TryBuyKeepsake("mushroom_cap",profile.Rewards.EarnedCoins(Nodes(60)),profile.Rewards),Is.True);
            int income=profile.Rewards.EarnedCoins(Nodes(60));Assert.That(service.TryClaimParcel(20,profile.Rewards),Is.True);
            Assert.That(profile.Rewards.Parcels.Single(p=>p.Order==20).DuplicateCoins,Is.True);
            Assert.That(profile.Rewards.EarnedCoins(Nodes(60)),Is.EqualTo(income+40));Assert.That(service.TryClaimParcel(20,profile.Rewards),Is.False);
            profile.Validate();
        }

        [Test] public void EarnedSceneryUsesItsOwnSlotAndSurvivesSaveModeChangeAndTwoDeviceClaim()
        {
            var nodes=Nodes(372);var baseline=AddCurrentCosmeticParcel(Earned(372),124);var remote=Copy(baseline);var local=Copy(baseline);
            foreach(var profile in new[]{remote,local}){
                var service=Service(profile);Assert.That(service.TryClaimParcel(124,profile.Rewards),Is.True);
                Assert.That(profile.Rewards.Owns("firefly_dance"),Is.True);Assert.That(profile.Rewards.EquippedGift,Is.Null);
                Assert.That(service.TryEquipScenery("firefly_dance",profile.Rewards),Is.True);
                Assert.That(profile.Rewards.Equip("firefly_dance"),Is.False);profile.Validate();
            }
            var merge=CloudReconciler.Merge(remote,baseline,local,nodes);Assert.That(merge.EconomyConflict,Is.False);
            Assert.That(merge.Profile.Rewards.Parcels.Count(p=>p.Order==124&&p.Claimed),Is.EqualTo(1));
            Assert.That(merge.Profile.Rewards.EquippedScenery,Is.EqualTo("firefly_dance"));Assert.That(merge.Profile.Monetization.Cosmetics,Is.Empty);
            var joined=CloudReconciler.Merge(merge.Profile,merge.Profile,new PlayerProfile(),nodes,retainRemoteEconomy:true);
            Assert.That(joined.Profile.Rewards.EquippedScenery,Is.EqualTo("firefly_dance"));joined.Profile.Validate();
            string directory=Path.Combine(Path.GetTempPath(),"CascadeEarnedScenery",Guid.NewGuid().ToString("N"));
            try{
                var store=new ProfileStore(Path.Combine(directory,"profile.json"));joined.Profile.SelectJourney("Words");store.Save(joined.Profile);
                var loaded=store.Load().Profile;loaded.Validate();Assert.That(loaded.Rewards.Owns("firefly_dance"),Is.True);
                Assert.That(loaded.Rewards.EquippedScenery,Is.EqualTo("firefly_dance"));Assert.That(loaded.Rewards.EquippedGift,Is.Null);
            }finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }

        [Test] public void RejectedScenePurchaseDuplicateBecomesClaimableAsEarnedScenery()
        {
            var nodes=Nodes(372);var baseline=AddCurrentCosmeticParcel(Earned(372),124);var remote=Copy(baseline);var local=Copy(baseline);var service=Service(local);
            Assert.That(service.TryBuyKeepsake("firefly_dance",local.Rewards.EarnedCoins(nodes),local.Rewards),Is.True);
            Assert.That(service.TryClaimParcel(124,local.Rewards),Is.True);Assert.That(service.TryEquipScenery("firefly_dance",local.Rewards),Is.True);
            local.Monetization.SpentCoins=100000;var merged=CloudReconciler.Merge(remote,baseline,local,nodes);
            Assert.That(merged.EconomyConflict,Is.True);Assert.That(merged.Profile.Rewards.Parcels.Single(p=>p.Order==124).Claimed,Is.False);
            Assert.That(merged.Profile.Rewards.EquippedScenery,Is.Null);
            Assert.That(Service(merged.Profile).TryClaimParcel(124,merged.Profile.Rewards),Is.True);
            Assert.That(Service(merged.Profile).TryEquipScenery("firefly_dance",merged.Profile.Rewards),Is.True);merged.Profile.Validate();
        }

        [Test] public void NewLooksCannotUseLegacyAppearancePaymentGrantsButPriorGrantsStillValidate()
        {
            var service=new AdService(new MonetizationLedger(),_=>{});
            foreach(var id in ShopCatalog.ExpansionTrophyIds){
                Assert.That(service.VerifiedMockPurchase("mock.keepsake."+id,"paid_"+id,true),Is.False);
                var forged=new MonetizationLedger();forged.Grants.Add(new GrantReceipt{Id="forged_"+id,Kind="cosmetic",Product=id});
                Assert.Throws<ArgumentException>(()=>forged.Validate());
            }
            foreach(var id in ShopCatalog.MikaTrophyIds.Take(16)){
                var legacy=new MonetizationLedger();legacy.Grants.Add(new GrantReceipt{Id="legacy_"+id,Kind="cosmetic",Product=id});legacy.Validate();
            }
            Assert.That(PurchaseCatalog.CoinPurchaseWalletThreshold,Is.EqualTo(ShopCatalog.TrophyIds.Max(ShopCatalog.TrophyPrice)));
            Assert.That(PurchaseCatalog.CoinPurchaseWalletThreshold,Is.EqualTo(2160));
        }
    }
}
