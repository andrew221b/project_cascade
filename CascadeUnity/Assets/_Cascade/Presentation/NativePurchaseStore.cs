using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Persistence;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Cascade.Presentation
{
    public enum PurchaseState { Unconfigured,Connecting,Ready,Busy,Deferred,Complete,Unavailable,Retry }
    public sealed class NativePurchaseStore:MonoBehaviour
    {
        StoreController store;AdService grants;MonetizationSettings settings;
        Func<Proof,Task<bool>> cloudVerifier;
        Func<Task> prepareServices;Func<Task<PurchaseIdentity>> purchaseIdentity;PurchaseIdentity identity;
        readonly HashSet<string> processing=new HashSet<string>();
        bool dead,productReady;Func<int> wallet;int pendingCoinNotice;
        public PurchaseState State {get;private set;}
        public event Action Changed;
        public event Action<int> CoinsGranted;
        public bool IsPreview=>grants?.PreviewOnly==true;
        public bool Owned=>grants?.Ledger.HasNoAutomaticAds==true;
        public string ActiveSubscription=>grants?.Ledger.ActiveNoAdsSubscription;
        public bool LifetimeOwned=>grants?.Ledger.NoAutomaticAds==true;
        public string Price=>ProductPrice(PurchaseCatalog.NoAdsYear);
        public bool CanBuy=>CanBuyProduct(PurchaseCatalog.NoAdsYear);
        public string ProductPrice(string product)=>store?.GetProductById(product)?.metadata.localizedPriceString;
        public int AnnualSavingsPercent
        {
            get {
                var month=store?.GetProductById(PurchaseCatalog.NoAdsMonth)?.metadata;
                var year=store?.GetProductById(PurchaseCatalog.NoAdsYear)?.metadata;
                if(month==null||year==null||month.isoCurrencyCode!=year.isoCurrencyCode||month.localizedPrice<=0||year.localizedPrice<=0)return 0;
                return Mathf.Clamp(Mathf.RoundToInt(100f*(1f-(float)(year.localizedPrice/(month.localizedPrice*12m)))),0,99);
            }
        }
        public bool HasGrantCapacity=>grants?.CanReceiveGrant==true;
        public bool CanBuyProduct(string product)=>HasGrantCapacity&&grants?.FullscreenActive==false&&PurchaseCatalog.IsKnown(product)&&(State==PurchaseState.Ready||State==PurchaseState.Complete)&&
            (IsPreview||store?.GetProductById(product)?.availableToPurchase==true)&&
            (PurchaseCatalog.IsNoAdsSubscription(product)?!Owned:product==PurchaseCatalog.NoAutomaticAds?false:(wallet?.Invoke()??0)<PurchaseCatalog.CoinPurchaseWalletThreshold);
        public bool CanRestore=>!IsPreview&&store!=null&&(State==PurchaseState.Ready||State==PurchaseState.Complete||State==PurchaseState.Retry);
        public void Initialize(AdService ledger,MonetizationSettings configuration,Func<int> coinBalance=null,Func<Proof,Task<bool>> verifier=null,Func<Task> servicesReady=null,Func<Task<PurchaseIdentity>> accountIdentity=null)
        {
            grants=ledger;settings=configuration;wallet=coinBalance;cloudVerifier=verifier;prepareServices=servicesReady;purchaseIdentity=accountIdentity;
            if(IsPreview){productReady=true;Set(Owned?PurchaseState.Complete:PurchaseState.Ready);return;}
            if(settings==null||!settings.storeCatalogRegistered||cloudVerifier==null){
                Set(PurchaseState.Unconfigured);return;
            }
#if UNITY_EDITOR
            // Editor fake purchases must never become production entitlements in the local profile.
            Set(PurchaseState.Unavailable);
#elif UNITY_IOS || UNITY_ANDROID
            Connect();
#else
            Set(PurchaseState.Unavailable);
#endif
        }
        async void Connect()
        {
            Set(PurchaseState.Connecting);
            try{if(prepareServices!=null)await Bounded(prepareServices());}catch(Exception){if(!dead)Set(PurchaseState.Retry);return;}
            if(dead)return;
            try{identity=purchaseIdentity==null?null:await Bounded(purchaseIdentity());}catch(Exception){Set(PurchaseState.Retry);return;}
            if(dead||identity==null||!Guid.TryParse(identity.AppleToken,out _)||identity.GoogleAccountId?.Length!=64){Set(PurchaseState.Unconfigured);return;}
            store=UnityIAPServices.StoreController();
            store.AppleStoreExtendedService?.SetAppAccountToken(Guid.Parse(identity.AppleToken));
            store.GooglePlayStoreExtendedService?.SetObfuscatedAccountId(identity.GoogleAccountId);
            store.OnStoreConnected+=Fetch;store.OnStoreDisconnected+=Disconnected;
            store.OnProductsFetched+=Products;store.OnProductsFetchFailed+=ProductsFailed;
            store.OnPurchasePending+=Pending;store.OnPurchaseConfirmed+=Confirmed;store.OnPurchaseFailed+=Failed;store.OnPurchaseDeferred+=Deferred;
            store.OnPurchasesFetched+=Purchases;store.OnPurchasesFetchFailed+=PurchasesFailed;store.OnAuthAccountChanged+=AccountChanged;
            try{await store.Connect();}catch(Exception){if(!dead)Set(PurchaseState.Retry);}
        }
        static async Task Bounded(Task task)
        {
            if(task==null)throw new InvalidOperationException("Missing purchase identity request.");
            if(await Task.WhenAny(task,Task.Delay(45000))!=task){
                _=task.ContinueWith(t=>{var ignored=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
                throw new TimeoutException("Purchase identity request timed out.");
            }
            await task;
        }
        static async Task<T> Bounded<T>(Task<T> task){await Bounded((Task)task);return await task;}
        void Fetch()=>store.FetchProducts(PurchaseCatalog.Products.Select(id=>new ProductDefinition(id,id==PurchaseCatalog.NoAutomaticAds?ProductType.NonConsumable:PurchaseCatalog.IsNoAdsSubscription(id)?ProductType.Subscription:ProductType.Consumable)).ToList());
        void AccountChanged(){productReady=false;Set(PurchaseState.Connecting);Fetch();}
        void Products(List<Product> products)
        {
            productReady=products.Any(p=>PurchaseCatalog.IsKnown(p.definition.id)&&p.availableToPurchase);
            Set(productReady?PurchaseState.Ready:PurchaseState.Unavailable);
            store.FetchPurchases();
        }
        public void Buy()=>Buy(PurchaseCatalog.NoAdsYear);
        public async void Buy(string product)
        {
            if(!CanBuyProduct(product))return;Set(PurchaseState.Busy);
            if(IsPreview){bool granted=PurchaseCatalog.IsNoAdsSubscription(product)?grants.TryPreviewNoAds():grants.TryPreviewCoinPack(product);
                Set(granted?PurchaseState.Complete:PurchaseState.Retry);if(granted&&PurchaseCatalog.Coins(product)>0)CoinsGranted?.Invoke(PurchaseCatalog.Coins(product));return;}
            try{
                var bound=purchaseIdentity==null?null:await Bounded(purchaseIdentity());
                if(dead)return;
                if(bound==null||!Guid.TryParse(bound.AppleToken,out var token)||bound.GoogleAccountId?.Length!=64){Set(PurchaseState.Retry);return;}
                identity=bound;store.AppleStoreExtendedService?.SetAppAccountToken(token);
                store.GooglePlayStoreExtendedService?.SetObfuscatedAccountId(bound.GoogleAccountId);
                store.PurchaseProduct(product);
            }catch(Exception){Set(PurchaseState.Retry);}
        }
        public async void RetryConnection(){if(State!=PurchaseState.Retry)return;if(IsPreview){Set(Owned?PurchaseState.Complete:PurchaseState.Ready);return;}if(store==null){Connect();return;}Set(PurchaseState.Connecting);try{await store.Connect();}catch(Exception){if(!dead)Set(PurchaseState.Retry);}}
        public void Restore(){if(!CanRestore)return;Set(PurchaseState.Busy);try{store.RestoreTransactions((ok,error)=>{if(dead)return;if(ok)store.FetchPurchases();else Set(PurchaseState.Retry);});}catch(Exception){Set(PurchaseState.Retry);}}
        void Pending(PendingOrder order){StartValidation(order,true);}
        void Purchases(Orders orders){foreach(var order in orders.PendingOrders)StartValidation(order,true);foreach(var order in orders.ConfirmedOrders)if(order.CartOrdered.Items().All(i=>i.Product.definition.id==PurchaseCatalog.NoAutomaticAds||PurchaseCatalog.IsNoAdsSubscription(i.Product.definition.id)))StartValidation(order,false);if(processing.Count==0)Set(Owned?PurchaseState.Complete:productReady?PurchaseState.Ready:PurchaseState.Unavailable);}
        void StartValidation(Order order,bool confirm)
        {
            var items=order.CartOrdered.Items();
            if(items.Count!=1||!PurchaseCatalog.IsKnown(items[0].Product.definition.id)||items[0].Quantity!=1||string.IsNullOrWhiteSpace(order.Info.TransactionID)){Set(PurchaseState.Retry);return;}
            string id=order.Info.TransactionID;if(!processing.Add(id))return;StartCoroutine(Validate(order,confirm));
        }
        [Serializable] public sealed class Proof {public string productId,transactionId,store,receipt,jws,bundleId;}
        IEnumerator Validate(Order order,bool confirm)
        {
            Set(PurchaseState.Busy);
            var proof=new Proof{productId=order.CartOrdered.Items()[0].Product.definition.id,transactionId=order.Info.TransactionID,
                store=order.Info.Apple!=null?"AppleAppStore":"GooglePlay",receipt=order.Info.Receipt,
                jws=order.Info.Apple?.jwsRepresentation,bundleId=UnityEngine.Application.identifier};
            try {
                string receiptId=ReceiptId(proof.store,proof.transactionId);
                bool alreadyGranted=grants.Ledger.Grants.Any(g=>g.Id==receiptId);
                Task<bool> verification=null;
                try {verification=cloudVerifier?.Invoke(proof);}catch(Exception){}
                if(verification==null){Set(PurchaseState.Retry);yield break;}
                float until=Time.realtimeSinceStartup+45;
                while(!verification.IsCompleted&&Time.realtimeSinceStartup<until)yield return null;
                if(!verification.IsCompleted) {
                    // Keep the store order pending. Late server success is recovered by FetchPurchases.
                    _=verification.ContinueWith(t=>{var ignored=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
                    Set(PurchaseState.Retry);yield break;
                }
                if(verification.IsFaulted||verification.IsCanceled||!verification.Result){Set(PurchaseState.Retry);yield break;}
                // Cloud Code has durably granted the verified transaction before acknowledging the store.
                if(!grants.Ledger.Grants.Any(g=>g.Id==receiptId&&g.Product==proof.productId)){Set(PurchaseState.Retry);yield break;}
                if(!alreadyGranted)pendingCoinNotice+=PurchaseCatalog.Coins(proof.productId);
                if(confirm){try{store.ConfirmPurchase((PendingOrder)order);}catch(Exception){Set(PurchaseState.Retry);}}
                else Set(PurchaseState.Complete);
            }finally{processing.Remove(proof.transactionId);}
        }
        static string ReceiptId(string platform,string transaction){using(var hash=SHA256.Create())return "store_"+BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(platform+"|"+transaction))).Replace("-","").ToLowerInvariant();}
        void Confirmed(Order order){Set(order is FailedOrder?PurchaseState.Retry:PurchaseState.Complete);if(pendingCoinNotice>0){int coins=pendingCoinNotice;pendingCoinNotice=0;CoinsGranted?.Invoke(coins);}}
        void Failed(FailedOrder order)=>Set(PurchaseState.Retry);
        void Deferred(DeferredOrder order)=>Set(PurchaseState.Deferred);
        void Disconnected(StoreConnectionFailureDescription error)=>Set(PurchaseState.Retry);
        void ProductsFailed(ProductFetchFailed error)=>Set(PurchaseState.Retry);
        void PurchasesFailed(PurchasesFetchFailureDescription error)=>Set(PurchaseState.Retry);
        void OnApplicationFocus(bool focused){if(focused&&!dead&&store!=null&&productReady&&!IsPreview)try{store.FetchPurchases();}catch(Exception){Set(PurchaseState.Retry);}}
        void Set(PurchaseState state){if(dead)return;State=state;Changed?.Invoke();}
        void OnDestroy()
        {
            dead=true;StopAllCoroutines();if(store==null)return;
            store.OnStoreConnected-=Fetch;store.OnStoreDisconnected-=Disconnected;
            store.OnProductsFetched-=Products;store.OnProductsFetchFailed-=ProductsFailed;
            store.OnPurchasePending-=Pending;store.OnPurchaseConfirmed-=Confirmed;store.OnPurchaseFailed-=Failed;store.OnPurchaseDeferred-=Deferred;
            store.OnPurchasesFetched-=Purchases;store.OnPurchasesFetchFailed-=PurchasesFailed;store.OnAuthAccountChanged-=AccountChanged;
        }
    }
}
