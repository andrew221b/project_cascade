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
        Func<Proof,Task<bool>> cloudVerifier;Func<Proof,Task<Verification>> publicVerifier;
        Func<Task> prepareServices;Func<Task<PurchaseIdentity>> purchaseIdentity;PurchaseIdentity identity;
        Func<bool> canWarmIdentity;Task<PurchaseIdentity> identityRequest;float identityRetryAt;bool warmingIdentity;
        readonly HashSet<string> processing=new HashSet<string>();
        readonly HashSet<string> deferredProducts=new HashSet<string>();
        bool dead,productReady,purchaseRequested,storeConnected,catalogFetchPending;Func<int> wallet;Action<PendingOrder> confirmPurchase;
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
        public void Initialize(AdService ledger,MonetizationSettings configuration,Func<int> coinBalance=null,Func<Proof,Task<bool>> verifier=null,Func<Task> servicesReady=null,Func<Task<PurchaseIdentity>> accountIdentity=null,Func<Proof,Task<Verification>> anonymousVerifier=null,Func<bool> identityWarmupAllowed=null)
        {
            grants=ledger;settings=configuration;wallet=coinBalance;cloudVerifier=verifier;prepareServices=servicesReady;purchaseIdentity=accountIdentity;publicVerifier=anonymousVerifier;canWarmIdentity=identityWarmupAllowed;
            if(IsPreview){productReady=true;Set(Owned?PurchaseState.Complete:PurchaseState.Ready);return;}
            if(settings==null||!settings.storeCatalogRegistered||cloudVerifier==null&&publicVerifier==null){
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
            // Store connection and prices are independent of UGS identity preparation.
            // Pending orders still require verification; new payments bind below in Buy.
            store=UnityIAPServices.StoreController();confirmPurchase=store.ConfirmPurchase;
            if(identity!=null){store.AppleStoreExtendedService?.SetAppAccountToken(Guid.Parse(identity.AppleToken));store.GooglePlayStoreExtendedService?.SetObfuscatedAccountId(identity.GoogleAccountId);}
            store.OnStoreConnected+=Connected;store.OnStoreDisconnected+=Disconnected;
            store.OnProductsFetched+=Products;store.OnProductsFetchFailed+=ProductsFailed;
            store.OnPurchasePending+=Pending;store.OnPurchaseConfirmed+=Confirmed;store.OnPurchaseFailed+=Failed;store.OnPurchaseDeferred+=Deferred;
            store.OnPurchasesFetched+=Purchases;store.OnPurchasesFetchFailed+=PurchasesFailed;store.OnAuthAccountChanged+=AccountChanged;
            try{await store.Connect();}catch(Exception){if(!dead)Set(PurchaseState.Retry);}
        }
        async Task<PurchaseIdentity> PrepareBinding()
        {
            if(identityRequest!=null)return await identityRequest;
            var pending=PrepareBindingCore();identityRequest=pending;
            try{return await pending;}finally{if(ReferenceEquals(identityRequest,pending))identityRequest=null;}
        }
        async Task<PurchaseIdentity> PrepareBindingCore()
        {
            if(prepareServices!=null)await Bounded(prepareServices());
            var bound=purchaseIdentity==null?null:await Bounded(purchaseIdentity());
            if(dead)throw new ObjectDisposedException(nameof(NativePurchaseStore));
            if(purchaseIdentity!=null){
                if(bound==null||!Guid.TryParse(bound.AppleToken,out var token)||bound.GoogleAccountId?.Length!=64)throw new InvalidOperationException("Purchase account is not ready.");
                identity=bound;store?.AppleStoreExtendedService?.SetAppAccountToken(token);store?.GooglePlayStoreExtendedService?.SetObfuscatedAccountId(bound.GoogleAccountId);
            }
            return bound;
        }
        async void WarmBinding()
        {
            warmingIdentity=true;
            try{await PrepareBinding();}
            catch(Exception){identityRetryAt=Time.realtimeSinceStartup+10;}
            finally{warmingIdentity=false;}
        }
        void Update()
        {
            if(!dead&&!IsPreview&&purchaseIdentity!=null&&identity==null&&storeConnected&&!warmingIdentity&&!purchaseRequested
                &&Time.realtimeSinceStartup>=identityRetryAt&&(canWarmIdentity?.Invoke()??true))WarmBinding();
        }
        static async Task Bounded(Task task)
        {
            if(task==null)throw new InvalidOperationException("Missing purchase identity request.");
            if(!task.IsCompleted&&await Task.WhenAny(task,Task.Delay(45000))!=task){
                _=task.ContinueWith(t=>{var ignored=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
                throw new TimeoutException("Purchase identity request timed out.");
            }
            await task;
        }
        static async Task<T> Bounded<T>(Task<T> task){await Bounded((Task)task);return await task;}
        void Connected(){storeConnected=true;Fetch();}
        void Fetch()
        {
            if(!storeConnected||catalogFetchPending)return;
            catalogFetchPending=true;
            try{store.FetchProducts(PurchaseCatalog.Products.Select(id=>new ProductDefinition(id,id==PurchaseCatalog.NoAutomaticAds?ProductType.NonConsumable:PurchaseCatalog.IsNoAdsSubscription(id)?ProductType.Subscription:ProductType.Consumable)).ToList());}
            catch(Exception){catalogFetchPending=false;Set(PurchaseState.Retry);}
        }
        void AccountChanged(){productReady=false;Set(PurchaseState.Connecting);Fetch();}
        void Products(List<Product> products)
        {
            catalogFetchPending=false;
            productReady=products.Any(p=>PurchaseCatalog.IsKnown(p.definition.id)&&p.availableToPurchase);
            if(!purchaseRequested&&processing.Count==0)Set(productReady?PurchaseState.Ready:PurchaseState.Unavailable);
            if(!purchaseRequested&&processing.Count==0)store.FetchPurchases();
        }
        public void Buy()=>Buy(PurchaseCatalog.NoAdsYear);
        public async void Buy(string product)
        {
            if(!CanBuyProduct(product))return;purchaseRequested=true;Set(PurchaseState.Busy);
            if(IsPreview){bool granted=PurchaseCatalog.IsNoAdsSubscription(product)?grants.TryPreviewNoAds():grants.TryPreviewCoinPack(product);
                purchaseRequested=false;Set(granted?PurchaseState.Complete:PurchaseState.Retry);if(granted&&PurchaseCatalog.Coins(product)>0)CoinsGranted?.Invoke(PurchaseCatalog.Coins(product));return;}
            var preparing=System.Diagnostics.Stopwatch.StartNew();
            try{
                await PrepareBinding();
                if(dead)return;
                Debug.Log("[CascadeIAP] Requesting native purchase sheet; preparationMs="+preparing.ElapsedMilliseconds);
                store.PurchaseProduct(product);
            }catch(Exception){Set(PurchaseState.Retry);}
        }
        public async void RetryConnection(){if(State!=PurchaseState.Retry)return;if(IsPreview){Set(Owned?PurchaseState.Complete:PurchaseState.Ready);return;}if(store==null){Connect();return;}Set(PurchaseState.Connecting);try{await store.Connect();}catch(Exception){if(!dead)Set(PurchaseState.Retry);}}
        public void Restore(){if(!CanRestore)return;Set(PurchaseState.Busy);try{store.RestoreTransactions((ok,error)=>{if(dead)return;if(ok)store.FetchPurchases();else Set(PurchaseState.Retry);});}catch(Exception){Set(PurchaseState.Retry);}}
        void Pending(PendingOrder order){RememberApproval(order,false);StartValidation(order,true);}
        void Purchases(Orders orders){foreach(var order in orders.DeferredOrders)RememberApproval(order,true);foreach(var order in orders.PendingOrders)Pending(order);foreach(var order in orders.ConfirmedOrders)if(order.CartOrdered.Items().All(i=>i.Product.definition.id==PurchaseCatalog.NoAutomaticAds||PurchaseCatalog.IsNoAdsSubscription(i.Product.definition.id)))StartValidation(order,false);if(!purchaseRequested&&processing.Count==0)Set(Owned?PurchaseState.Complete:productReady?PurchaseState.Ready:PurchaseState.Unavailable);}
        void StartValidation(Order order,bool confirm)
        {
            var items=order.CartOrdered.Items();
            if(items.Count!=1||!PurchaseCatalog.IsKnown(items[0].Product.definition.id)||items[0].Quantity!=1||string.IsNullOrWhiteSpace(order.Info.TransactionID)){Set(PurchaseState.Retry);return;}
            string id=order.Info.TransactionID;if(!processing.Add(id))return;StartCoroutine(Validate(order,confirm));
        }
        [Serializable] public sealed class Proof {public string productId,transactionId,store,receipt,jws,bundleId;}
        public sealed class Verification {public bool Valid;public long SubscriptionExpiresUnix;}
        async Task<bool> VerifyAndGrantPublic(Proof proof,string receiptId)
        {
            var result=await publicVerifier(proof);
            if(result?.Valid!=true)return false;
            return PurchaseCatalog.IsNoAdsSubscription(proof.productId)
                ?grants.VerifiedStoreSubscription(proof.productId,receiptId,result.SubscriptionExpiresUnix,true)
                :grants.VerifiedStorePurchase(proof.productId,receiptId,true);
        }
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
                try {verification=publicVerifier!=null?VerifyAndGrantPublic(proof,receiptId):cloudVerifier?.Invoke(proof);}catch(Exception){}
                if(verification==null){Set(PurchaseState.Retry);yield break;}
                float until=Time.realtimeSinceStartup+45;
                while(!verification.IsCompleted&&Time.realtimeSinceStartup<until)yield return null;
                if(!verification.IsCompleted) {
                    // Keep the store order pending. Late server success is recovered by FetchPurchases.
                    _=verification.ContinueWith(t=>{var ignored=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
                    Set(PurchaseState.Retry);yield break;
                }
                if(verification.IsFaulted||verification.IsCanceled||!verification.Result){Set(PurchaseState.Retry);yield break;}
                // The verified transaction is durably granted before acknowledging the store.
                if(!grants.Ledger.Grants.Any(g=>g.Id==receiptId&&g.Product==proof.productId)){Set(PurchaseState.Retry);yield break;}
                // Show the durable grant now; native acknowledgement may arrive later.
                Changed?.Invoke();
                int coins=alreadyGranted?0:PurchaseCatalog.Coins(proof.productId);
                if(coins>0)CoinsGranted?.Invoke(coins);
                if(confirm){try{confirmPurchase((PendingOrder)order);}catch(Exception){Set(PurchaseState.Retry);}}
                else Set(PurchaseState.Complete);
            }finally{processing.Remove(proof.transactionId);}
        }
        static string ReceiptId(string platform,string transaction){using(var hash=SHA256.Create())return "store_"+BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(platform+"|"+transaction))).Replace("-","").ToLowerInvariant();}
        void Confirmed(Order order){RememberApproval(order,false);purchaseRequested=false;Set(order is FailedOrder?PurchaseState.Retry:PurchaseState.Complete);}
        void Failed(FailedOrder order){RememberApproval(order,false);Set(PurchaseState.Retry);}
        void RememberApproval(Order order,bool waiting)
        {
            if(order?.CartOrdered==null)return;
            foreach(var item in order.CartOrdered.Items()){
                string id=item.Product?.definition?.id;if(!PurchaseCatalog.IsKnown(id))continue;
                if(waiting)deferredProducts.Add(id);else deferredProducts.Remove(id);
            }
        }
        void Deferred(DeferredOrder order){RememberApproval(order,true);Set(PurchaseState.Deferred);}
        void Disconnected(StoreConnectionFailureDescription error){storeConnected=false;catalogFetchPending=false;Set(PurchaseState.Retry);}
        void ProductsFailed(ProductFetchFailed error){catalogFetchPending=false;Set(PurchaseState.Retry);}
        void PurchasesFailed(PurchasesFetchFailureDescription error)=>Set(PurchaseState.Retry);
        void OnApplicationFocus(bool focused){if(focused&&!dead&&store!=null&&productReady&&!IsPreview&&!purchaseRequested&&processing.Count==0&&State!=PurchaseState.Busy&&State!=PurchaseState.Connecting)try{store.FetchPurchases();}catch(Exception){Set(PurchaseState.Retry);}}
        void Set(PurchaseState state){if(dead)return;State=(state==PurchaseState.Ready||state==PurchaseState.Complete)&&deferredProducts.Count>0?PurchaseState.Deferred:state;if(State==PurchaseState.Retry||State==PurchaseState.Deferred)purchaseRequested=false;Changed?.Invoke();}
        void OnDestroy()
        {
            dead=true;StopAllCoroutines();if(store==null)return;
            store.OnStoreConnected-=Connected;store.OnStoreDisconnected-=Disconnected;
            store.OnProductsFetched-=Products;store.OnProductsFetchFailed-=ProductsFailed;
            store.OnPurchasePending-=Pending;store.OnPurchaseConfirmed-=Confirmed;store.OnPurchaseFailed-=Failed;store.OnPurchaseDeferred-=Deferred;
            store.OnPurchasesFetched-=Purchases;store.OnPurchasesFetchFailed-=PurchasesFailed;store.OnAuthAccountChanged-=AccountChanged;
        }
    }
}
