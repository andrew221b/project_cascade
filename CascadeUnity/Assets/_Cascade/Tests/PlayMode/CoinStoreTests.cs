using System.Collections;
using Cascade.Application;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Cascade.Tests
{
    public sealed class CoinStoreTests
    {
        [UnityTest] public IEnumerator TestWalletStopsNewPacksButKeepsNoAdsAvailable()
        {
            var owner=new GameObject("Isolated preview store");
            try{
                var ledger=new AdService(new MonetizationLedger(),_=>{},previewOnly:true);
                var store=owner.AddComponent<NativePurchaseStore>();store.Initialize(ledger,null,()=>ledger.CoinBalance(0));
                int received=0;store.CoinsGranted+=value=>received+=value;
                store.Buy(PurchaseCatalog.Coins480);store.Buy(PurchaseCatalog.Coins480);
                Assert.That(received,Is.EqualTo(960));Assert.That(ledger.Ledger.BonusCoins,Is.EqualTo(960));
                Assert.That(store.CanBuyProduct(PurchaseCatalog.Coins240),Is.False);store.Buy(PurchaseCatalog.Coins240);
                Assert.That(received,Is.EqualTo(960));Assert.That(store.CanBuy,Is.True);
                var ticket=ledger.TryRewarded(new AdContext{Surface=AdSurface.Inventory,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true},"coins",true);
                Assert.That(store.CanBuy,Is.False);ledger.Closed(ticket.Id);Assert.That(store.CanBuy,Is.True);
                store.Buy();Assert.That(store.Owned,Is.True);Assert.That(store.CanBuy,Is.False);
                Assert.That(store.CanRestore,Is.False);Assert.That(store.CanBuyProduct("unknown"),Is.False);
                Assert.That(ledger.Ledger.Grants,Is.Empty);
            }finally{Object.Destroy(owner);}
            yield return null;
        }
        [UnityTest] public IEnumerator MissingPublisherConfigurationCannotGrantRealMoney()
        {
            var owner=new GameObject("Unconfigured store");var settings=ScriptableObject.CreateInstance<MonetizationSettings>();
            try{
                var ledger=new AdService(new MonetizationLedger(),_=>{});var store=owner.AddComponent<NativePurchaseStore>();store.Initialize(ledger,settings);
                Assert.That(store.State,Is.EqualTo(PurchaseState.Unconfigured));Assert.That(store.CanBuyProduct(PurchaseCatalog.Coins240),Is.False);
                store.Buy(PurchaseCatalog.Coins240);Assert.That(ledger.Ledger.BonusCoins,Is.Zero);Assert.That(ledger.Ledger.Grants,Is.Empty);
            }finally{Object.Destroy(owner);Object.Destroy(settings);}
            yield return null;
        }
    }
}
