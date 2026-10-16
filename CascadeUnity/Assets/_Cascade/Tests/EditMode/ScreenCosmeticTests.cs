using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Persistence;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class ScreenCosmeticTests
    {
        static string[] Nodes(int count)=>Enumerable.Range(1,count).Select(i=>"C"+i.ToString("D2")).ToArray();
        static PlayerProfile Earned(int count=120)
        {
            var profile=new PlayerProfile();foreach(var node in Nodes(count))profile.Clears.Add(new ClearReceipt{
                CampaignNodeId=node,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64)});
            profile.Rewards.Synchronize(Nodes(count));return profile;
        }
        static PlayerProfile Copy(PlayerProfile p)=>JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(p));
        static AdService Service(PlayerProfile p)=>new AdService(p.Monetization,next=>p.Monetization=next);
        static void Buy(PlayerProfile p,string id)=>Assert.That(Service(p).TryBuyKeepsake(id,p.Rewards.EarnedCoins(Nodes(540)),p.Rewards),Is.True);

        [Test] public void AppendedCatalogPricesUseCoinsAndPreserveMikaAndLootIds()
        {
            Assert.That(ShopCatalog.TrophyIds.Take(ShopCatalog.MikaTrophyIds.Length),Is.EqualTo(ShopCatalog.MikaTrophyIds));
            Assert.That(ShopCatalog.TrophyIds.Skip(ShopCatalog.MikaTrophyIds.Length),Is.EqualTo(ShopCatalog.SceneryTrophyIds.Concat(ShopCatalog.FrameTrophyIds)));
            Assert.That(ShopCatalog.TrophyIds.Distinct().Count(),Is.EqualTo(46));
            Assert.That(ShopCatalog.SceneryTrophyIds.Select(ShopCatalog.TrophyPrice),Is.EqualTo(new[]{480,600,720,840,720,960,840,1080,600,720,960,1080}));
            Assert.That(ShopCatalog.FrameTrophyIds.Select(ShopCatalog.TrophyPrice),Is.EqualTo(new[]{720,840,960,1200}));
            var profile=Earned(300);var service=Service(profile);
            foreach(var id in ShopCatalog.TrophyIds.Skip(ShopCatalog.MikaTrophyIds.Length)){
                Assert.That(ShopCatalog.EarnedClearMilestone(id),Is.Zero);Assert.That(ShopCatalog.PurchaseClearMilestone(id),Is.Zero);
                Assert.That(profile.Rewards.Owns(id),Is.False);Assert.That(PurchaseCatalog.IsKnown(id),Is.False);
                Assert.That(service.VerifiedMockPurchase("mock.keepsake."+id,"paid_"+id,true),Is.False);
                Assert.That(service.TryBuyKeepsake(id,profile.Rewards.EarnedCoins(Nodes(540)),profile.Rewards),Is.True);
                Assert.That(service.TryBuyKeepsake(id,profile.Rewards.EarnedCoins(Nodes(540)),profile.Rewards),Is.False);
            }
            Assert.That(profile.Monetization.SpentCoins,Is.EqualTo(13320));Assert.That(profile.Monetization.Grants,Is.Empty);
            Assert.That(profile.Rewards.EquippedGift,Is.Null);Assert.That(profile.Rewards.EquippedScenery,Is.Null);Assert.That(profile.Rewards.EquippedFrame,Is.Null);
            foreach(int version in new[]{0,1})for(int order=16;order<=180;order+=4)
                Assert.That(ShopCatalog.IsScreenCosmetic(JourneyRewards.BonusParcel(order,version).GiftId),Is.False);
            profile.Validate();
            var forged=Copy(profile);forged.Monetization.Grants.Add(new GrantReceipt{Id="paid_screen",Kind="cosmetic",Product="petal_drift"});
            Assert.Throws<ArgumentException>(()=>forged.Validate());
        }

        [Test] public void OwnershipCategoryAndIndependentSlotsAreEnforced()
        {
            var profile=Earned();Buy(profile,"leaf");Buy(profile,"petal_drift");Buy(profile,"garden_frame");
            Assert.That(profile.Rewards.Equip("leaf",profile.Monetization.Cosmetics),Is.True);
            var service=Service(profile);
            Assert.That(service.TryEquipScenery("petal_drift",profile.Rewards),Is.True);
            Assert.That(service.TryEquipFrame("garden_frame",profile.Rewards),Is.True);
            Assert.That(service.TryEquipScenery("firefly_dance",profile.Rewards),Is.False);
            Assert.That(service.TryEquipScenery("garden_frame",profile.Rewards),Is.False);
            Assert.That(service.TryEquipFrame("petal_drift",profile.Rewards),Is.False);
            Assert.That(profile.Rewards.Equip("petal_drift",profile.Monetization.Cosmetics),Is.False);
            Assert.That(profile.Rewards.EquippedGift,Is.EqualTo("leaf"));Assert.That(profile.Rewards.EquippedScenery,Is.EqualTo("petal_drift"));
            Assert.That(profile.Rewards.EquippedFrame,Is.EqualTo("garden_frame"));profile.Validate();
            var malformed=Copy(profile);malformed.Rewards.EquippedFrame="petal_drift";Assert.Throws<ArgumentException>(()=>malformed.Validate());
            malformed=Copy(profile);malformed.Rewards.EquippedScenery="firefly_dance";Assert.Throws<ArgumentException>(()=>malformed.Validate());
        }

        [Test] public void PurchaseAndEquipFailuresRollBackAndCanRetry()
        {
            var profile=Earned();Buy(profile,"petal_drift");Buy(profile,"firefly_dance");Buy(profile,"garden_frame");
            bool fail=true;int saves=0;var service=new AdService(profile.Monetization,next=>{
                if(fail)throw new IOException("disk");profile.Monetization=next;saves++;});
            int spent=profile.Monetization.SpentCoins;
            Assert.That(service.TryBuyKeepsake("sky_frame",5000,profile.Rewards),Is.False);
            Assert.That(profile.Monetization.SpentCoins,Is.EqualTo(spent));Assert.That(profile.Monetization.Cosmetics.Contains("sky_frame"),Is.False);
            Assert.That(service.TryEquipScenery("petal_drift",profile.Rewards),Is.False);Assert.That(profile.Rewards.EquippedScenery,Is.Null);
            Assert.That(service.TryEquipFrame("garden_frame",profile.Rewards),Is.False);Assert.That(profile.Rewards.EquippedFrame,Is.Null);
            fail=false;Assert.That(service.TryEquipScenery("petal_drift",profile.Rewards),Is.True);
            Assert.That(service.TryEquipScenery("petal_drift",profile.Rewards),Is.False);Assert.That(saves,Is.EqualTo(1));
            fail=true;Assert.That(service.TryEquipScenery("firefly_dance",profile.Rewards),Is.False);
            Assert.That(profile.Rewards.EquippedScenery,Is.EqualTo("petal_drift"));
            Assert.That(service.TryEquipScenery(null,profile.Rewards),Is.False);Assert.That(profile.Rewards.EquippedScenery,Is.EqualTo("petal_drift"));
            fail=false;Assert.That(service.TryEquipScenery(null,profile.Rewards),Is.True);Assert.That(profile.Rewards.EquippedScenery,Is.Null);
            Assert.That(profile.Monetization.SpentCoins,Is.EqualTo(spent));profile.Validate();
        }

        [Test] public void MissingSlotsAreOmittedAndDurableSlotsSurviveModeSwitchReload()
        {
            string json=JsonConvert.SerializeObject(Earned());Assert.That(json.Contains("EquippedScenery"),Is.False);Assert.That(json.Contains("EquippedFrame"),Is.False);
            var old=JsonConvert.DeserializeObject<PlayerProfile>(json);old.Validate();Assert.That(old.Rewards.EquippedScenery,Is.Null);
            string directory=Path.Combine(Path.GetTempPath(),"CascadeScreenCosmetics",Guid.NewGuid().ToString("N"));
            try{
                var profile=Earned();var store=new ProfileStore(Path.Combine(directory,"profile.json"));
                var service=MonetizationTransaction.Create(profile,store);
                Assert.That(service.TryBuyKeepsake("petal_drift",profile.Rewards.EarnedCoins(Nodes(540)),profile.Rewards),Is.True);
                Assert.That(service.TryBuyKeepsake("garden_frame",profile.Rewards.EarnedCoins(Nodes(540)),profile.Rewards),Is.True);
                Assert.That(service.TryEquipScenery("petal_drift",profile.Rewards),Is.True);Assert.That(service.TryEquipFrame("garden_frame",profile.Rewards),Is.True);
                profile.SelectJourney("Words");store.Save(profile);var loaded=store.Load().Profile;loaded.Validate();
                Assert.That(loaded.Rewards.EquippedScenery,Is.EqualTo("petal_drift"));Assert.That(loaded.Rewards.EquippedFrame,Is.EqualTo("garden_frame"));
                Assert.That(loaded.Monetization.SpentCoins,Is.EqualTo(1200));Assert.That(loaded.Monetization.Cosmetics.Count,Is.EqualTo(2));
            }finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }

        [Test] public void IndependentRemoteAndLocalSlotChangesMergeWithoutSpending()
        {
            var baseline=Earned();foreach(var id in new[]{"petal_drift","firefly_dance","garden_frame","sky_frame"})Buy(baseline,id);
            Assert.That(Service(baseline).TryEquipScenery("petal_drift",baseline.Rewards),Is.True);
            Assert.That(Service(baseline).TryEquipFrame("garden_frame",baseline.Rewards),Is.True);
            var remote=Copy(baseline);var local=Copy(baseline);
            Assert.That(Service(remote).TryEquipScenery("firefly_dance",remote.Rewards),Is.True);
            Assert.That(Service(local).TryEquipFrame("sky_frame",local.Rewards),Is.True);
            var merge=CloudReconciler.Merge(remote,baseline,local,Nodes(120));Assert.That(merge.EconomyConflict,Is.False);
            Assert.That(merge.Profile.Rewards.EquippedScenery,Is.EqualTo("firefly_dance"));Assert.That(merge.Profile.Rewards.EquippedFrame,Is.EqualTo("sky_frame"));
            Assert.That(merge.Profile.Monetization.SpentCoins,Is.EqualTo(baseline.Monetization.SpentCoins));
        }

        [Test] public void LocalUnequipWinsSimultaneousRemoteChange()
        {
            var baseline=Earned();Buy(baseline,"petal_drift");Buy(baseline,"firefly_dance");
            Assert.That(Service(baseline).TryEquipScenery("petal_drift",baseline.Rewards),Is.True);
            var remote=Copy(baseline);var local=Copy(baseline);
            Assert.That(Service(remote).TryEquipScenery("firefly_dance",remote.Rewards),Is.True);
            Assert.That(Service(local).TryEquipScenery(null,local.Rewards),Is.True);
            var merge=CloudReconciler.Merge(remote,baseline,local,Nodes(120));Assert.That(merge.EconomyConflict,Is.False);
            Assert.That(merge.Profile.Rewards.EquippedScenery,Is.Null);merge.Profile.Validate();
        }

        [Test] public void RejectedOwnershipFallsBackToRemoteAndJoiningDeviceAdoptsAccountSlots()
        {
            var baseline=Earned(30);Buy(baseline,"petal_drift");Assert.That(Service(baseline).TryEquipScenery("petal_drift",baseline.Rewards),Is.True);
            var remote=Copy(baseline);Buy(remote,"garden_frame");Assert.That(Service(remote).TryEquipFrame("garden_frame",remote.Rewards),Is.True);
            var local=Copy(baseline);Buy(local,"firefly_dance");Assert.That(Service(local).TryEquipScenery("firefly_dance",local.Rewards),Is.True);
            var merge=CloudReconciler.Merge(remote,baseline,local,Nodes(30));Assert.That(merge.EconomyConflict,Is.True);
            Assert.That(merge.Profile.Rewards.EquippedScenery,Is.EqualTo("petal_drift"));Assert.That(merge.Profile.Rewards.EquippedFrame,Is.EqualTo("garden_frame"));
            Assert.That(merge.Profile.Monetization.Cosmetics.Contains("firefly_dance"),Is.False);merge.Profile.Validate();
            var joined=CloudReconciler.Merge(remote,remote,new PlayerProfile(),Nodes(30),retainRemoteEconomy:true);
            Assert.That(joined.EconomyConflict,Is.False);Assert.That(joined.Profile.Rewards.EquippedScenery,Is.EqualTo("petal_drift"));
            Assert.That(joined.Profile.Rewards.EquippedFrame,Is.EqualTo("garden_frame"));joined.Profile.Validate();
        }

        [Test] public void PreviewServiceCannotPersistentlyEquipScreenItems()
        {
            var profile=Earned();Buy(profile,"petal_drift");var preview=new AdService(profile.Monetization.Copy(),_=>{},previewOnly:true);
            Assert.That(preview.TryEquipScenery("petal_drift",profile.Rewards),Is.False);Assert.That(profile.Rewards.EquippedScenery,Is.Null);
        }

        [Test] public void ThirdLegacyLeafPartGrantsOwnershipWithoutAutomaticAppearance()
        {
            var profile=Earned(9);profile.Rewards.Parcels.Clear();
            for(int order=1;order<=3;order++)profile.Rewards.Parcels.Add(new ParcelReceipt{Order=order,GiftId="leaf"});
            var service=Service(profile);
            for(int order=1;order<=3;order++)Assert.That(service.TryClaimParcel(order,profile.Rewards),Is.True);
            Assert.That(profile.Rewards.Owns("leaf"),Is.True);Assert.That(profile.Rewards.Fragments("leaf"),Is.EqualTo(3));
            Assert.That(profile.Rewards.EquippedGift,Is.Null);
            Assert.That(service.TryEquipMika("leaf",profile.Rewards),Is.True);
            Assert.That(service.TryEquipMika(null,profile.Rewards),Is.True);
            Assert.That(Copy(profile).Rewards.EquippedGift,Is.Null);Assert.That(service.TryClaimParcel(3,profile.Rewards),Is.False);
            Assert.That(profile.Rewards.EquippedGift,Is.Null);profile.Validate();
            var whole=Earned(3);Assert.That(Service(whole).TryClaimParcel(1,whole.Rewards),Is.True);
            Assert.That(whole.Rewards.Owns(whole.Rewards.Parcels[0].GiftId),Is.True);Assert.That(whole.Rewards.EquippedGift,Is.Null);
        }

        [Test] public void MikaRemovalPreservesOtherSlotsAndFailedSaveRestoresTheLook()
        {
            var profile=Earned();Buy(profile,"leaf");Buy(profile,"petal_drift");Buy(profile,"garden_frame");
            var service=Service(profile);Assert.That(service.TryEquipMika("leaf",profile.Rewards),Is.True);
            Assert.That(service.TryEquipScenery("petal_drift",profile.Rewards),Is.True);Assert.That(service.TryEquipFrame("garden_frame",profile.Rewards),Is.True);
            var failed=new AdService(profile.Monetization,_=>throw new IOException("disk"));
            Assert.That(failed.TryEquipMika(null,profile.Rewards),Is.False);Assert.That(profile.Rewards.EquippedGift,Is.EqualTo("leaf"));
            Assert.That(service.TryEquipMika("moon",profile.Rewards),Is.False);Assert.That(service.TryEquipMika("petal_drift",profile.Rewards),Is.False);
            Assert.That(new AdService(profile.Monetization.Copy(),_=>{},previewOnly:true).TryEquipMika(null,profile.Rewards),Is.False);
            Assert.That(service.TryEquipMika(null,profile.Rewards),Is.True);
            Assert.That(profile.Rewards.EquippedScenery,Is.EqualTo("petal_drift"));Assert.That(profile.Rewards.EquippedFrame,Is.EqualTo("garden_frame"));
            Assert.That(service.TryEquipMika(null,profile.Rewards),Is.False);profile.Validate();
        }

        [Test] public void MikaChoiceUsesThreeWayCloudMergeIncludingRemovalAndAccountJoin()
        {
            var baseline=Earned();Buy(baseline,"leaf");Buy(baseline,"sun");Assert.That(Service(baseline).TryEquipMika("leaf",baseline.Rewards),Is.True);
            var remote=Copy(baseline);Assert.That(Service(remote).TryEquipMika("sun",remote.Rewards),Is.True);
            var unchanged=CloudReconciler.Merge(remote,baseline,Copy(baseline),Nodes(120));
            Assert.That(unchanged.Profile.Rewards.EquippedGift,Is.EqualTo("sun"));
            var local=Copy(baseline);Assert.That(Service(local).TryEquipMika(null,local.Rewards),Is.True);
            var removed=CloudReconciler.Merge(remote,baseline,local,Nodes(120));
            Assert.That(removed.EconomyConflict,Is.False);Assert.That(removed.Profile.Rewards.EquippedGift,Is.Null);
            var joined=CloudReconciler.Merge(remote,remote,new PlayerProfile(),Nodes(120),retainRemoteEconomy:true);
            Assert.That(joined.Profile.Rewards.EquippedGift,Is.EqualTo("sun"));joined.Profile.Validate();
        }
    }
}
