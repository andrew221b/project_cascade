using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Purchasing;
using Object=UnityEngine.Object;

namespace Cascade.Tests
{
    public sealed class CoinStoreTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Field(NativePurchaseStore target,string name,object value)=>typeof(NativePurchaseStore).GetField(name,Private).SetValue(target,value);
        static void Call(NativePurchaseStore target,string name,object value)=>typeof(NativePurchaseStore).GetMethod(name,Private).Invoke(target,new[]{value});
        static PendingOrder Payment(string transaction)
        {
            var product=(Product)Activator.CreateInstance(typeof(Product),Private,null,new object[]{new ProductDefinition(PurchaseCatalog.Coins240,ProductType.Consumable),new ProductMetadata(),true},null);
            var info=(IOrderInfo)Activator.CreateInstance(typeof(Order).Assembly.GetType("UnityEngine.Purchasing.OrderInfo"),new object[]{"test-receipt",transaction,"GooglePlay"});
            return new PendingOrder(new Cart(new CartItem(product)),info);
        }
        [UnityTest] public IEnumerator ColdConsentCallbackRunsBeforeAdSdkInitialization()
        {
            var executor=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("GoogleMobileAds.Common.MobileAdsEventExecutor")).Single(t=>t!=null);
            Func<bool> active=()=>(bool)executor.GetMethod("IsActive").Invoke(null,null);
            Action remove=()=>Object.DestroyImmediate(((MonoBehaviour)executor.GetField("instance").GetValue(null)).gameObject);
            bool hadExecutor=active();
            if(hadExecutor)remove();
            var root=new GameObject("Cold ad callback boundary");var config=ScriptableObject.CreateInstance<MonetizationSettings>();
            try{
                var provider=root.AddComponent<NativeAdProvider>();provider.enabled=false;
                provider.Initialize(null,new AdService(new MonetizationLedger(),_=>{}),config);
                bool delivered=false;
                typeof(NativeAdProvider).GetMethod("Main",Private).Invoke(provider,new object[]{(Action)(()=>delivered=true)});
                for(int frame=0;frame<4&&!delivered;frame++)yield return null;
                Assert.That(delivered,Is.True,"Consent callbacks must drain before MobileAds.Initialize can be reached");
            }finally{
                Object.Destroy(root);Object.Destroy(config);
                if(active())remove();
                if(hadExecutor)executor.GetMethod("Initialize").Invoke(null,null);
            }
            yield return null;
        }
        [UnityTest] public IEnumerator IdentityPreparationRunsAlongsideSlowCatalogLoading()
        {
            var owner=new GameObject("Parallel catalog and account preparation");var config=ScriptableObject.CreateInstance<MonetizationSettings>();config.storeCatalogRegistered=true;
            var pending=new TaskCompletionSource<Cascade.Persistence.PurchaseIdentity>();
            try{
                int calls=0;bool allow=false;
                var store=owner.AddComponent<NativePurchaseStore>();store.enabled=false;
                store.Initialize(new AdService(new MonetizationLedger(),_=>{}),config,
                    accountIdentity:()=>{calls++;return pending.Task;},anonymousVerifier:_=>Task.FromResult(new NativePurchaseStore.Verification{Valid=false}),identityWarmupAllowed:()=>allow);
                Field(store,"storeConnected",true);Field(store,"productReady",false);
                typeof(NativePurchaseStore).GetMethod("Update",Private).Invoke(store,null);Assert.That(calls,Is.Zero,"Account UI still owns identity changes");
                allow=true;typeof(NativePurchaseStore).GetMethod("Update",Private).Invoke(store,null);
                Assert.That(calls,Is.EqualTo(1),"A slow native price request must not postpone independent account preparation");
                pending.SetResult(Cascade.Persistence.PurchaseIdentity.For("project","production","parallel"));yield return null;
            }finally{pending.TrySetCanceled();Object.Destroy(owner);Object.Destroy(config);}
            yield return null;
        }
        [UnityTest] public IEnumerator ParentApprovalSurvivesPurchaseRefreshAndNeverGrantsBeforeApproval()
        {
            var owner=new GameObject("Parent approval boundary");var config=ScriptableObject.CreateInstance<MonetizationSettings>();config.storeCatalogRegistered=true;
            try{
                int verified=0,received=0,acks=0;var ledger=new AdService(new MonetizationLedger(),_=>{});
                var store=owner.AddComponent<NativePurchaseStore>();store.enabled=false;
                store.Initialize(ledger,config,anonymousVerifier:_=>{verified++;return Task.FromResult(new NativePurchaseStore.Verification{Valid=true});});
                store.CoinsGranted+=n=>received+=n;Field(store,"confirmPurchase",(Action<PendingOrder>)(_=>acks++));Field(store,"productReady",true);
                var payment=Payment("approved-parent-order");
                Call(store,"Deferred",new DeferredOrder(payment.CartOrdered,payment.Info));
                Assert.That(store.State,Is.EqualTo(PurchaseState.Deferred));
                Call(store,"Purchases",new Orders(Array.Empty<ConfirmedOrder>(),Array.Empty<PendingOrder>(),Array.Empty<DeferredOrder>()));
                Assert.That(store.State,Is.EqualTo(PurchaseState.Deferred),"A foreground refresh must not erase Awaiting approval");
                Assert.That(verified,Is.Zero);Assert.That(received,Is.Zero);Assert.That(acks,Is.Zero);Assert.That(ledger.Ledger.Grants,Is.Empty);
                Call(store,"Pending",payment);yield return null;
                Assert.That(verified,Is.EqualTo(1));Assert.That(received,Is.EqualTo(240));Assert.That(acks,Is.EqualTo(1));
                Call(store,"Confirmed",new ConfirmedOrder(payment.CartOrdered,payment.Info));
                Assert.That(store.State,Is.EqualTo(PurchaseState.Complete));Assert.That(ledger.Ledger.BonusCoins,Is.EqualTo(240));
            }finally{Object.Destroy(owner);Object.Destroy(config);}
            yield return null;
        }
        [UnityTest] public IEnumerator PurchaseIdentityWarmsOnceAndIsRecheckedBeforeTheNextPayment()
        {
            var owner=new GameObject("Identity preparation boundary");var config=ScriptableObject.CreateInstance<MonetizationSettings>();config.storeCatalogRegistered=true;
            try{
                int requests=0;bool allow=false;var pending=new TaskCompletionSource<Cascade.Persistence.PurchaseIdentity>();
                var first=Cascade.Persistence.PurchaseIdentity.For("project","production","first");
                var second=Cascade.Persistence.PurchaseIdentity.For("project","production","second");
                var store=owner.AddComponent<NativePurchaseStore>();store.Initialize(new AdService(new MonetizationLedger(),_=>{}),config,
                    accountIdentity:()=>{requests++;return requests==1?pending.Task:Task.FromResult(second);},anonymousVerifier:_=>Task.FromResult(new NativePurchaseStore.Verification{Valid=false}),identityWarmupAllowed:()=>allow);
                Field(store,"storeConnected",true);Field(store,"productReady",true);
                typeof(NativePurchaseStore).GetMethod("Update",Private).Invoke(store,null);Assert.That(requests,Is.Zero,"Do not prepare an identity while the system account UI is active");
                allow=true;typeof(NativePurchaseStore).GetMethod("Update",Private).Invoke(store,null);
                var prepare=typeof(NativePurchaseStore).GetMethod("PrepareBinding",Private);
                var tap=(Task<Cascade.Persistence.PurchaseIdentity>)prepare.Invoke(store,null);
                Assert.That(requests,Is.EqualTo(1),"A buy tap reuses in-flight warmup");
                pending.SetResult(first);while(!tap.IsCompleted)yield return null;
                Assert.That(tap.Result.AppleToken,Is.EqualTo(first.AppleToken));
                var next=(Task<Cascade.Persistence.PurchaseIdentity>)prepare.Invoke(store,null);while(!next.IsCompleted)yield return null;
                Assert.That(requests,Is.EqualTo(2));Assert.That(next.Result.AppleToken,Is.EqualTo(second.AppleToken),"A new payment checks the active account instead of keeping an old store token");
            }finally{Object.Destroy(owner);Object.Destroy(config);}
            yield return null;
        }
        [UnityTest] public IEnumerator VerifiedCoinsRefreshBeforeSlowStoreAcknowledgementAndOnlyOnce()
        {
            var owner=new GameObject("Delayed native acknowledgement");var config=ScriptableObject.CreateInstance<MonetizationSettings>();config.storeCatalogRegistered=true;
            try{
                var response=new TaskCompletionSource<NativePurchaseStore.Verification>();int received=0,acknowledgements=0,saves=0;
                var ledger=new AdService(new MonetizationLedger(),_=>saves++);var store=owner.AddComponent<NativePurchaseStore>();
                store.Initialize(ledger,config,anonymousVerifier:_=>response.Task);store.CoinsGranted+=coins=>received+=coins;
                Field(store,"confirmPurchase",(Action<PendingOrder>)(_=>acknowledgements++));var order=Payment("delayed-ack");Call(store,"Pending",order);
                Assert.That(store.State,Is.EqualTo(PurchaseState.Busy));Assert.That(received,Is.Zero);Assert.That(acknowledgements,Is.Zero);Assert.That(ledger.Ledger.BonusCoins,Is.Zero);
                response.SetResult(new NativePurchaseStore.Verification{Valid=true});
                float until=Time.realtimeSinceStartup+3;while(acknowledgements==0&&Time.realtimeSinceStartup<until)yield return null;
                Assert.That(acknowledgements,Is.EqualTo(1));Assert.That(saves,Is.EqualTo(1));Assert.That(ledger.Ledger.BonusCoins,Is.EqualTo(240));Assert.That(received,Is.EqualTo(240),"Wallet feedback must not wait for native acknowledgement");
                Assert.That(store.State,Is.EqualTo(PurchaseState.Busy));
                Call(store,"Pending",order);yield return null;Assert.That(acknowledgements,Is.EqualTo(2));Assert.That(received,Is.EqualTo(240));Assert.That(saves,Is.EqualTo(1),"A redelivered receipt cannot grant twice");
                Call(store,"Confirmed",new ConfirmedOrder(order.CartOrdered,order.Info));Assert.That(store.State,Is.EqualTo(PurchaseState.Complete));Assert.That(received,Is.EqualTo(240));
            }finally{Object.Destroy(owner);Object.Destroy(config);}
            yield return null;
        }
        [UnityTest] public IEnumerator RejectedReceiptCannotCreditOrAcknowledge()
        {
            var owner=new GameObject("Rejected receipt");var config=ScriptableObject.CreateInstance<MonetizationSettings>();config.storeCatalogRegistered=true;
            try{
                int received=0,acknowledgements=0;var ledger=new AdService(new MonetizationLedger(),_=>{});var store=owner.AddComponent<NativePurchaseStore>();
                store.Initialize(ledger,config,anonymousVerifier:_=>Task.FromResult(new NativePurchaseStore.Verification{Valid=false}));store.CoinsGranted+=coins=>received+=coins;
                Field(store,"confirmPurchase",(Action<PendingOrder>)(_=>acknowledgements++));Call(store,"Pending",Payment("invalid"));yield return null;
                Assert.That(store.State,Is.EqualTo(PurchaseState.Retry));Assert.That(received,Is.Zero);Assert.That(acknowledgements,Is.Zero);Assert.That(ledger.Ledger.Grants,Is.Empty);
            }finally{Object.Destroy(owner);Object.Destroy(config);}
            yield return null;
        }
        [UnityTest] public IEnumerator TestWalletStopsNewPacksButKeepsNoAdsAvailable()
        {
            var owner=new GameObject("Isolated preview store");
            try{
                var ledger=new AdService(new MonetizationLedger(),_=>{},previewOnly:true);
                var store=owner.AddComponent<NativePurchaseStore>();store.Initialize(ledger,null,()=>ledger.CoinBalance(0));
                int received=0;store.CoinsGranted+=value=>received+=value;
                int packs=(PurchaseCatalog.CoinPurchaseWalletThreshold+479)/480;
                for(int i=0;i<packs;i++)store.Buy(PurchaseCatalog.Coins480);
                Assert.That(received,Is.EqualTo(packs*480));Assert.That(ledger.Ledger.BonusCoins,Is.EqualTo(packs*480));
                Assert.That(store.CanBuyProduct(PurchaseCatalog.Coins240),Is.False);store.Buy(PurchaseCatalog.Coins240);
                Assert.That(received,Is.EqualTo(packs*480));Assert.That(store.CanBuy,Is.True);
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
