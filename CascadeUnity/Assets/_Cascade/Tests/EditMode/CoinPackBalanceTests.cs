using System;
using System.Linq;
using Cascade.Application;
using NUnit.Framework;
namespace Cascade.Tests
{
    public sealed class CoinPackBalanceTests
    {
        [TestCase(PurchaseCatalog.Coins600,600)]
        [TestCase(PurchaseCatalog.Coins720,720)]
        [TestCase(PurchaseCatalog.Coins960,960)]
        [TestCase(PurchaseCatalog.Coins1080,1080)]
        public void VerifiedNewTierIsExactlyOnceAndCannotReuseAnotherProduct(string product,int amount)
        {
            int saves=0;var service=new AdService(new MonetizationLedger(),_=>saves++);
            Assert.That(service.VerifiedStorePurchase(product,"new_tier_receipt",false),Is.False);
            Assert.That(service.VerifiedStorePurchase(product,"new_tier_receipt",true),Is.True);
            Assert.That(service.VerifiedStorePurchase(product,"new_tier_receipt",true),Is.True);
            Assert.That(service.VerifiedStorePurchase(PurchaseCatalog.Coins240,"new_tier_receipt",true),Is.False);
            Assert.That(service.Ledger.BonusCoins,Is.EqualTo(amount));Assert.That(saves,Is.EqualTo(1));service.Ledger.Validate();
        }
        [Test] public void OneBundleAloneCannotBuyOneOfEveryToolOrTheEntireTrophyCatalogue()
        {
            int largest=PurchaseCatalog.CoinProducts.Max(PurchaseCatalog.Coins);
            int oneOfEach=Enum.GetValues(typeof(TravelTool)).Cast<TravelTool>().Sum(ShopCatalog.Price);
            int trophies=ShopCatalog.TrophyIds.Where(id=>!ShopCatalog.IsParcelExclusive(id)).Sum(ShopCatalog.TrophyPrice);
            Assert.That(largest,Is.LessThan(oneOfEach));Assert.That(largest,Is.LessThan(trophies));
            Assert.That(PurchaseCatalog.Coins(PurchaseCatalog.Coins240),Is.EqualTo(240));
            Assert.That(PurchaseCatalog.Coins(PurchaseCatalog.Coins480),Is.EqualTo(480));
        }
    }
}
