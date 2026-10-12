using System;
using System.IO;
using Cascade.Application;
using Cascade.Persistence;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class PreviewMonetizationTests
    {
        [Test] public void TestShopItemsRespectStockCapAndDoNotSpendCoins()
        {
            var s=new AdService(new MonetizationLedger(),_=>{},previewOnly:true);
            foreach(TravelTool tool in Enum.GetValues(typeof(TravelTool))){
                for(int i=0;i<ShopCatalog.StockLimit;i++)Assert.That(s.TryPreviewItem(tool),Is.True);
                Assert.That(s.TryPreviewItem(tool),Is.False);Assert.That(s.ToolStock(tool),Is.EqualTo(5));
            }
            Assert.That(s.TryPreviewItem(TravelTool.Wayfinder,JourneyRewards.Gifts[0]),Is.True);
            Assert.That(s.TryPreviewItem(TravelTool.Wayfinder,JourneyRewards.Gifts[0]),Is.False);
            Assert.That(s.TryPreviewItem(TravelTool.Wayfinder,"unknown"),Is.False);
            Assert.That(s.Ledger.SpentCoins,Is.Zero);Assert.That(s.Ledger.Grants,Is.Empty);
            var live=new AdService(new MonetizationLedger(),_=>{});
            Assert.That(live.TryPreviewItem(TravelTool.Wayfinder),Is.False);Assert.That(live.TryPreviewNoAds(),Is.False);
        }
        [Test] public void TestAdsAndPurchasesNeverPersistFakeMoneyOwnershipOrReceipts()
        {
            string dir=Path.Combine(Path.GetTempPath(),"CascadePreview",Guid.NewGuid().ToString("N"));
            try{
                var profile=new PlayerProfile();var store=new ProfileStore(Path.Combine(dir,"profile.json"));
                var a=new AttemptService(ContentTests.LoadCatalog().GetPrimary("C02").Definition);
                var s=MonetizationTransaction.CreatePreview(profile,store,()=>a);string before=a.CurrentStateHash;
                Assert.That(s.TryPreviewItem(TravelTool.UndoSpool),Is.True);
                Assert.That(s.TryPreviewItem(TravelTool.Wayfinder,JourneyRewards.Gifts[1]),Is.True);
                var c=new AdContext{Surface=AdSurface.Inventory,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true};
                var ticket=s.TryRewarded(c,"coins",true);s.Opened(ticket.Id);s.Closed(ticket.Id);
                Assert.That(s.RewardConfirmed(ticket.Id,"test",true),Is.True);Assert.That(s.RewardConfirmed(ticket.Id,"test",true),Is.False);
                Assert.That(s.TryPreviewNoAds(),Is.True);Assert.That(s.TryPreviewNoAds(),Is.False);
                Assert.That(s.Ledger.BonusCoins,Is.EqualTo(40));Assert.That(s.Ledger.NoAutomaticAds,Is.True);
                foreach(var ledger in new[]{profile.Monetization,store.Load().Profile.Monetization}){
                    Assert.That(ledger.BonusCoins+ledger.SpentCoins+ledger.UndoSpools,Is.Zero);
                    Assert.That(ledger.NoAutomaticAds,Is.False);Assert.That(ledger.Cosmetics,Is.Empty);Assert.That(ledger.Grants,Is.Empty);
                }
                Assert.That(a.CurrentStateHash,Is.EqualTo(before));Assert.That(store.Load().Profile.Attempt,Is.Not.Null);
                Assert.That(MonetizationTransaction.CreatePreview(profile,store,()=>a).Ledger.NoAutomaticAds,Is.False);
            }finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
        [Test] public void TestPurchaseSaveFailureDoesNotPublishAnEntitlement()
        {
            var s=new AdService(new MonetizationLedger(),_=>throw new IOException("disk"),previewOnly:true);
            Assert.That(s.TryPreviewItem(TravelTool.ReturnHourglass),Is.False);Assert.That(s.TryPreviewNoAds(),Is.False);
            Assert.That(s.Ledger.NoAutomaticAds,Is.False);Assert.That(s.Ledger.ReturnHourglasses,Is.Zero);
        }
    }
}
