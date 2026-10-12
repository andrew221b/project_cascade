using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Persistence;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class WardrobeParcelTests
    {
        static string[] Nodes(int count)=>Enumerable.Range(1,count).Select(i=>"C"+i.ToString("D2")).ToArray();
        static PlayerProfile Earned(int count)
        {
            var profile=new PlayerProfile();var nodes=Nodes(count);
            foreach(var node in nodes)profile.Clears.Add(new ClearReceipt{CampaignNodeId=node,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64)});
            profile.Rewards.Synchronize(nodes);
            // These regression cases retain the already published version-one receipts.
            // These fixtures model saves minted before recipe 5, whose first twelve
            // receipts were leaf fragments rather than the new coin/tool/cosmetic mix.
            profile.Rewards.Parcels=profile.Rewards.Parcels.Select(p=>p.Order<=12?new ParcelReceipt{Order=p.Order,GiftId=JourneyRewards.Gifts[(p.Order-1)/3]}:p).ToList();
            profile.Rewards.Parcels=profile.Rewards.Parcels.Select(p=>p.Order<=12?p:JourneyRewards.BonusParcel(p.Order,1)).ToList();return profile;
        }
        static PlayerProfile Copy(PlayerProfile profile)=>JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(profile));
        static AdService Service(PlayerProfile profile)=>new AdService(profile.Monetization,next=>profile.Monetization=next);

        [Test] public void CatalogUsesRebalancedPricesAndNewLooksUseOnlyCoins()
        {
            Assert.That(ShopCatalog.TrophyIds.Take(12),Is.EqualTo(new[]{"leaf","sun","sky","moon","wind_chime","rain_badge","copper_acorn","beacon_brooch","sky_pilot","moth_mask","garden_festival","cloud_gardener"}));
            Assert.That(ShopCatalog.MikaTrophyIds.Skip(12).Take(4),Is.EqualTo(new[]{"explorer_hat","frog_suit","star_knight","fox_mask"}));
            foreach(var id in ShopCatalog.TrophyIds.Take(8))Assert.That(ShopCatalog.TrophyPrice(id),Is.EqualTo(360));
            foreach(var id in ShopCatalog.BonusTrophyIds)Assert.That(ShopCatalog.TrophyPrice(id),Is.EqualTo(1440));
            Assert.That(ShopCatalog.NewTrophyIds.Select(ShopCatalog.TrophyPrice),Is.EqualTo(new[]{720,1080,1440,1800}));
            var profile=new PlayerProfile();var service=Service(profile);
            foreach(var id in ShopCatalog.NewTrophyIds){
                Assert.That(profile.Rewards.Owns(id),Is.False);Assert.That(ShopCatalog.EarnedClearMilestone(id),Is.Zero);
                Assert.That(PurchaseCatalog.IsKnown(id),Is.False);Assert.That(service.VerifiedMockPurchase("mock.keepsake."+id,"mock_"+id,true),Is.False);
                Assert.That(service.TryBuyKeepsake(id,3360,profile.Rewards),Is.True);
                Assert.That(service.TryBuyKeepsake(id,3360,profile.Rewards),Is.False);
            }
            Assert.That(service.CoinBalance(3360),Is.Zero);Assert.That(profile.Clears,Is.Empty);Assert.That(profile.Rewards.SeenClearNodes,Is.Empty);
        }

        [Test] public void VersionOneHasRareFixedLooksAndReplayCannotRerollReceipts()
        {
            var profile=Earned(540);profile.Rewards.Parcels=profile.Rewards.Parcels.Select(p=>p.Order<=12?p:JourneyRewards.BonusParcel(p.Order,1)).ToList();var before=JsonConvert.SerializeObject(profile.Rewards.Parcels);
            profile.Rewards.Synchronize(Nodes(540).Reverse());
            Assert.That(JsonConvert.SerializeObject(profile.Rewards.Parcels),Is.EqualTo(before));
            Assert.That(profile.Rewards.Parcels.Take(12).All(p=>p.RecipeVersion==0&&p.Kind==null),Is.True);
            Assert.That(profile.Rewards.Parcels.Where(p=>p.Kind=="cosmetic").Select(p=>p.Order*3),Is.EqualTo(new[]{84,132,192,240,300,348,408,456,516}));
            Assert.That(profile.Rewards.Parcels.Where(p=>p.Kind=="cosmetic"&&ShopCatalog.NewTrophyIds.Contains(p.GiftId)).Select(p=>p.GiftId),Is.EqualTo(ShopCatalog.NewTrophyIds));
            Assert.That(profile.Rewards.Parcels.Any(p=>p.Kind=="tool"),Is.True);profile.Validate();
        }

        [Test] public void OldMintedCoinsCosmeticsAndClaimsKeepTheirOriginalRecipe()
        {
            var profile=Earned(132);
            profile.Rewards.Parcels=profile.Rewards.Parcels.Select(p=>p.Order<=12?p:JourneyRewards.BonusParcel(p.Order,0)).ToList();
            var old=profile.Rewards.Parcels.Single(p=>p.Order==44);Assert.That(old.Kind,Is.EqualTo("coins"));
            Assert.That(profile.Rewards.ClaimParcel(44),Is.True);profile.Rewards.Synchronize(Nodes(144));
            Assert.That(old.Claimed,Is.True);Assert.That(old.Kind,Is.EqualTo("coins"));Assert.That(old.RecipeVersion,Is.Zero);
            Assert.That(profile.Rewards.Parcels.Single(p=>p.Order==48).RecipeVersion,Is.EqualTo(5));
            profile.Clears.AddRange(Earned(144).Clears.Skip(132));profile.Validate();
            var copy=Copy(profile);Assert.That(copy.Rewards.Parcels.Single(p=>p.Order==44).Claimed,Is.True);copy.Validate();
        }

        [Test] public void ToolChestClaimAndGrantAreAtomicRetryableAndExactlyOnce()
        {
            var profile=Earned(48);var receipt=profile.Rewards.Parcels.Single(p=>p.Order==16);bool fail=true;
            var service=new AdService(profile.Monetization,next=>{if(fail)throw new IOException("disk");profile.Monetization=next;});
            Assert.That(profile.Rewards.ClaimParcel(16),Is.False);
            Assert.That(service.TryClaimParcel(16,profile.Rewards),Is.False);
            Assert.That(receipt.Claimed||receipt.DuplicateCoins,Is.False);Assert.That(service.Ledger.LandingLenses,Is.Zero);Assert.That(service.Ledger.Grants,Is.Empty);
            fail=false;Assert.That(service.TryClaimParcel(16,profile.Rewards),Is.True);
            Assert.That(service.TryClaimParcel(16,profile.Rewards),Is.False);Assert.That(service.Ledger.LandingLenses,Is.EqualTo(1));
            Assert.That(service.Ledger.Grants.Single().Id,Is.EqualTo("parcel_tool_16"));profile.Validate();
        }

        [Test] public void FullToolStackAndOwnedCosmeticConvertOnceToTwentyCoins()
        {
            var profile=Earned(132);profile.Monetization.LandingLenses=5;profile.Monetization.SpentCoins=500;
            profile.Monetization.Cosmetics.Add("explorer_hat");profile.Monetization.SpentCoins+=480;
            var service=Service(profile);int coins=profile.Rewards.EarnedCoins(Nodes(132));
            Assert.That(service.TryClaimParcel(16,profile.Rewards),Is.True);Assert.That(service.Ledger.LandingLenses,Is.EqualTo(5));
            Assert.That(service.TryClaimParcel(44,profile.Rewards),Is.True);Assert.That(service.TryClaimParcel(44,profile.Rewards),Is.False);
            Assert.That(profile.Rewards.EarnedCoins(Nodes(132)),Is.EqualTo(coins+40));Assert.That(service.Ledger.Grants,Is.Empty);profile.Validate();
        }

        [Test] public void ChestAndNewLookSurviveDurableSaveModeSwitchAndReload()
        {
            string directory=Path.Combine(Path.GetTempPath(),"CascadeWardrobe",Guid.NewGuid().ToString("N"));
            try{
                var profile=Earned(132);var store=new ProfileStore(Path.Combine(directory,"profile.json"));
                var service=MonetizationTransaction.Create(profile,store);
                Assert.That(service.TryClaimParcel(16,profile.Rewards),Is.True);Assert.That(service.TryClaimParcel(44,profile.Rewards),Is.True);
                Assert.That(profile.Rewards.Equip("explorer_hat"),Is.True);profile.SelectJourney("Words");store.Save(profile);
                var saved=store.Load().Profile;Assert.That(saved.Monetization.LandingLenses,Is.EqualTo(1));
                Assert.That(saved.Rewards.Owns("explorer_hat"),Is.True);Assert.That(saved.Rewards.EquippedGift,Is.EqualTo("explorer_hat"));
                Assert.That(Service(saved).TryClaimParcel(16,saved.Rewards),Is.False);saved.Validate();
            }finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }

        [Test] public void TwoDevicesClaimingOneChestMergeOneToolAndOneReceipt()
        {
            var baseline=Earned(48);var remote=Copy(baseline);var local=Copy(baseline);
            Assert.That(Service(remote).TryClaimParcel(16,remote.Rewards),Is.True);Assert.That(Service(local).TryClaimParcel(16,local.Rewards),Is.True);
            var merge=CloudReconciler.Merge(remote,baseline,local,Nodes(48));Assert.That(merge.EconomyConflict,Is.False);
            Assert.That(merge.Profile.Monetization.LandingLenses,Is.EqualTo(1));Assert.That(merge.Profile.Monetization.Grants.Count,Is.EqualTo(1));
            var repeated=CloudReconciler.Merge(merge.Profile,merge.Profile,merge.Profile,Nodes(48));
            Assert.That(repeated.EconomyConflict,Is.False);Assert.That(repeated.Profile.Monetization.LandingLenses,Is.EqualTo(1));
        }

        [Test] public void RejectedToolDeltaLeavesItsChestClaimableAndKeepsRemoteStock()
        {
            var baseline=Earned(48);var remote=Copy(baseline);remote.Monetization.LandingLenses=5;remote.Monetization.SpentCoins=500;
            var local=Copy(baseline);Assert.That(Service(local).TryClaimParcel(16,local.Rewards),Is.True);
            var merge=CloudReconciler.Merge(remote,baseline,local,Nodes(48));Assert.That(merge.EconomyConflict,Is.True);
            Assert.That(merge.Profile.Monetization.LandingLenses,Is.EqualTo(5));Assert.That(merge.Profile.Rewards.Parcels.Single(p=>p.Order==16).Claimed,Is.False);
            Assert.That(Service(merge.Profile).TryClaimParcel(16,merge.Profile.Rewards),Is.True);
            Assert.That(merge.Profile.Rewards.Parcels.Single(p=>p.Order==16).DuplicateCoins,Is.True);merge.Profile.Validate();
        }

        [Test] public void ToolGrantsNeedPublishedEarnedReceiptAndCannotFinanceUnknownNodes()
        {
            var profile=Earned(48);Assert.That(Service(profile).TryClaimParcel(16,profile.Rewards),Is.True);
            var forged=Copy(profile);forged.Monetization.Grants[0].Product=TravelTool.Wayfinder.ToString();
            Assert.Throws<ArgumentException>(()=>forged.Validate());
            var filtered=CloudReconciler.Merge(new PlayerProfile(),new PlayerProfile(),profile,Nodes(3));
            Assert.That(filtered.EconomyConflict,Is.True);Assert.That(filtered.Profile.Monetization.Grants,Is.Empty);
            Assert.That(filtered.Profile.Monetization.LandingLenses,Is.Zero);
            var imported=CloudReconciler.ImportLocal(profile,Nodes(48));Assert.That(imported.EconomyConflict,Is.False);
            Assert.That(imported.Profile.Monetization.LandingLenses,Is.EqualTo(1));
        }

        [Test] public void AlternateToolFallbackOnAnotherDeviceKeepsTheCanonicalClaim()
        {
            var baseline=Earned(48);var remote=Copy(baseline);Assert.That(Service(remote).TryClaimParcel(16,remote.Rewards),Is.True);
            var local=Copy(baseline);local.Monetization.LandingLenses=5;local.Monetization.SpentCoins=500;
            Assert.That(Service(local).TryClaimParcel(16,local.Rewards),Is.True);
            var merge=CloudReconciler.Merge(remote,baseline,local,Nodes(48));Assert.That(merge.EconomyConflict,Is.True);
            Assert.That(merge.Profile.Monetization.LandingLenses,Is.EqualTo(1));
            Assert.That(merge.Profile.Rewards.Parcels.Single(p=>p.Order==16).DuplicateCoins,Is.False);merge.Profile.Validate();
        }

        [Test] public void ArchivedCosmeticPurchaseLeavesAnIncomingDuplicateChestClaimable()
        {
            var remote=Earned(132);var local=Copy(remote);var service=Service(local);
            Assert.That(service.TryBuyKeepsake("explorer_hat",local.Rewards.EarnedCoins(Nodes(132)),local.Rewards),Is.True);
            Assert.That(service.TryClaimParcel(44,local.Rewards),Is.True);
            Assert.That(local.Rewards.Parcels.Single(p=>p.Order==44).DuplicateCoins,Is.True);
            var merged=CloudReconciler.Merge(remote,remote,local,Nodes(132),retainRemoteEconomy:true).Profile;
            Assert.That(merged.Monetization.Cosmetics,Is.Empty);Assert.That(merged.Rewards.Parcels.Single(p=>p.Order==44).Claimed,Is.False);
            Assert.That(Service(merged).TryClaimParcel(44,merged.Rewards),Is.True);Assert.That(merged.Rewards.Owns("explorer_hat"),Is.True);
        }

        [Test] public void ExistingClaimedCosmeticCannotBeConvertedByAnotherDevice()
        {
            var baseline=Earned(132);var remote=Copy(baseline);Assert.That(Service(remote).TryClaimParcel(44,remote.Rewards),Is.True);
            var local=Copy(baseline);var service=Service(local);
            Assert.That(service.TryBuyKeepsake("explorer_hat",local.Rewards.EarnedCoins(Nodes(132)),local.Rewards),Is.True);
            Assert.That(service.TryClaimParcel(44,local.Rewards),Is.True);
            var merge=CloudReconciler.Merge(remote,baseline,local,Nodes(132));Assert.That(merge.EconomyConflict,Is.True);
            Assert.That(merge.Profile.Rewards.Owns("explorer_hat"),Is.True);Assert.That(merge.Profile.Rewards.Parcels.Single(p=>p.Order==44).DuplicateCoins,Is.False);
            Assert.That(merge.Profile.Monetization.SpentCoins,Is.Zero);merge.Profile.Validate();
        }

        [Test] public void UnownedDuplicateCompensationCannotFinanceANegativeWallet()
        {
            var remote=Earned(132);var local=Copy(remote);
            var receipt=local.Rewards.Parcels.Single(p=>p.Order==44);receipt.Claimed=true;receipt.DuplicateCoins=true;
            local.Monetization.SpentCoins=remote.Rewards.EarnedCoins(Nodes(132))+10;
            var merge=CloudReconciler.Merge(remote,remote,local,Nodes(132));Assert.That(merge.EconomyConflict,Is.True);
            Assert.That(merge.Profile.Monetization.SpentCoins,Is.Zero);Assert.That(merge.Profile.Rewards.Parcels.Single(p=>p.Order==44).Claimed,Is.False);
            merge.Profile.Validate();
        }

        [Test] public void ConflictingOfflineFragmentsKeepRemoteReceiptsAndFillOnlyMissingOrders()
        {
            var remote=Earned(9);var local=Earned(18);
            foreach(var parcel in local.Rewards.Parcels)parcel.GiftId=parcel.Order<=3?"sun":"leaf";
            string original=JsonConvert.SerializeObject(remote.Rewards.Parcels);
            var merge=CloudReconciler.Merge(remote,new PlayerProfile(),local,Nodes(48));Assert.That(merge.EconomyConflict,Is.True);
            Assert.That(JsonConvert.SerializeObject(remote.Rewards.Parcels),Is.EqualTo(original));
            Assert.That(merge.Profile.Rewards.Parcels.Take(3).Select(p=>p.GiftId),Is.EqualTo(new[]{"leaf","leaf","leaf"}));
            Assert.That(merge.Profile.Rewards.Parcels.Select(p=>p.Order),Is.EqualTo(Enumerable.Range(1,6)));
            merge.Profile.Clears.AddRange(Earned(48).Clears.Skip(18));merge.Profile.Rewards.Synchronize(Nodes(48));
            Assert.That(merge.Profile.Rewards.Parcels.Count(p=>p.Order<=12),Is.EqualTo(12));
            Assert.That(merge.Profile.Rewards.Parcels.Single(p=>p.Order==16).Kind,Is.EqualTo("cosmetic"));merge.Profile.Validate();
        }

        [Test] public void DisjointOfflineClearsMintOnlyMissingCanonicalChests()
        {
            var remote=Earned(36);var local=Earned(36);
            local.Clears=Earned(72).Clears.Skip(36).ToList();local.Rewards=new JourneyRewards();local.Rewards.Synchronize(local.Clears.Select(c=>c.CampaignNodeId));
            local.Rewards.Parcels=local.Rewards.Parcels.Select(p=>new ParcelReceipt{Order=p.Order,GiftId=JourneyRewards.Gifts[(p.Order-1)/3]}).ToList(); // Both offline devices use the same legacy recipe.
            string original=JsonConvert.SerializeObject(remote.Rewards.Parcels);
            var merge=CloudReconciler.Merge(remote,new PlayerProfile(),local,Nodes(72));Assert.That(merge.EconomyConflict,Is.False);
            Assert.That(merge.Profile.Rewards.CosmeticClearCount,Is.EqualTo(72));
            Assert.That(merge.Profile.Rewards.Parcels.Count,Is.EqualTo(15));
            Assert.That(JsonConvert.SerializeObject(merge.Profile.Rewards.Parcels.Take(12)),Is.EqualTo(original));
            Assert.That(merge.Profile.Rewards.Parcels.Where(p=>p.Order>12).Select(p=>p.Order),Is.EqualTo(new[]{16,20,24}));
            merge.Profile.Validate();
        }
    }
}
