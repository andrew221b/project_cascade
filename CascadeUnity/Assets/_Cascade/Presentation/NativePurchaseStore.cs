using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cascade.Application;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;

namespace Cascade.Presentation
{
    public enum PurchaseState { Unconfigured,Connecting,Ready,Busy,Deferred,Complete,Unavailable,Retry }
    public sealed class NativePurchaseStore:MonoBehaviour
    {
        StoreController store;AdService grants;MonetizationSettings settings;
        readonly HashSet<string> processing=new HashSet<string>();
        bool dead,productReady;Func<int> wallet;int pendingCoinNotice;
        public PurchaseState State {get;private set;}
        public event Action Changed;
        public event Action<int> CoinsGranted;
        public bool IsPreview=>grants?.PreviewOnly==true;
        public bool Owned=>grants?.Ledger.NoAutomaticAds==true;
        public string Price=>store?.GetProductById(PurchaseCatalog.NoAutomaticAds)?.metadata.localizedPriceString;
        public bool CanBuy=>CanBuyProduct(PurchaseCatalog.NoAutomaticAds);
        public string ProductPrice(string product)=>store?.GetProductById(product)?.metadata.localizedPriceString;
        public bool CanBuyProduct(string product)=>grants?.FullscreenActive==false&&PurchaseCatalog.IsKnown(product)&&(State==PurchaseState.Ready||State==PurchaseState.Complete)&&
            (IsPreview||store?.GetProductById(product)?.availableToPurchase==true)&&
            (product==PurchaseCatalog.NoAutomaticAds?!Owned:(wallet?.Invoke()??0)<PurchaseCatalog.CoinPurchaseWalletThreshold);
        public bool CanRestore=>!IsPreview&&store!=null&&(State==PurchaseState.Ready||State==PurchaseState.Complete||State==PurchaseState.Retry);
        public void Initialize(AdService ledger,MonetizationSettings configuration,Func<int> coinBalance=null)
        {
            grants=ledger;settings=configuration;wallet=coinBalance;
            if(IsPreview){productReady=true;Set(Owned?PurchaseState.Complete:PurchaseState.Ready);return;}
            if(settings==null||!settings.storeCatalogRegistered||!Uri.TryCreate(settings.receiptValidationUrl,UriKind.Absolute,out var uri)||uri.Scheme!="https"){
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
            Set(PurchaseState.Connecting);store=UnityIAPServices.StoreController();
            store.OnStoreConnected+=Fetch;store.OnStoreDisconnected+=Disconnected;
            store.OnProductsFetched+=Products;store.OnProductsFetchFailed+=ProductsFailed;
            store.OnPurchasePending+=Pending;store.OnPurchaseConfirmed+=Confirmed;store.OnPurchaseFailed+=Failed;store.OnPurchaseDeferred+=Deferred;
            store.OnPurchasesFetched+=Purchases;store.OnPurchasesFetchFailed+=PurchasesFailed;store.OnAuthAccountChanged+=AccountChanged;
            try{await store.Connect();}catch(Exception){if(!dead)Set(PurchaseState.Retry);}
        }
        void Fetch()=>store.FetchProducts(PurchaseCatalog.Products.Select(id=>new ProductDefinition(id,id==PurchaseCatalog.NoAutomaticAds?ProductType.NonConsumable:ProductType.Consumable)).ToList());
        void AccountChanged(){productReady=false;Set(PurchaseState.Connecting);Fetch();}
        void Products(List<Product> products)
        {
            productReady=products.Any(p=>PurchaseCatalog.IsKnown(p.definition.id)&&p.availableToPurchase);
            Set(productReady?PurchaseState.Ready:PurchaseState.Unavailable);
            store.FetchPurchases();
        }
        public void Buy()=>Buy(PurchaseCatalog.NoAutomaticAds);
        public void Buy(string product)
        {
            if(!CanBuyProduct(product))return;Set(PurchaseState.Busy);
            if(IsPreview){bool granted=product==PurchaseCatalog.NoAutomaticAds?grants.TryPreviewNoAds():grants.TryPreviewCoinPack(product);
                Set(granted?PurchaseState.Complete:PurchaseState.Retry);if(granted&&PurchaseCatalog.Coins(product)>0)CoinsGranted?.Invoke(PurchaseCatalog.Coins(product));return;}
            try{store.PurchaseProduct(product);}catch(Exception){Set(PurchaseState.Retry);}
        }
        public async void RetryConnection(){if(State!=PurchaseState.Retry)return;if(IsPreview){Set(Owned?PurchaseState.Complete:PurchaseState.Ready);return;}if(store==null){Connect();return;}Set(PurchaseState.Connecting);try{await store.Connect();}catch(Exception){if(!dead)Set(PurchaseState.Retry);}}
        public void Restore(){if(!CanRestore)return;Set(PurchaseState.Busy);try{store.RestoreTransactions((ok,error)=>{if(dead)return;if(ok)store.FetchPurchases();else Set(PurchaseState.Retry);});}catch(Exception){Set(PurchaseState.Retry);}}
        void Pending(PendingOrder order){StartValidation(order,true);}
        void Purchases(Orders orders){foreach(var order in orders.PendingOrders)StartValidation(order,true);foreach(var order in orders.ConfirmedOrders)if(order.CartOrdered.Items().All(i=>i.Product.definition.id==PurchaseCatalog.NoAutomaticAds))StartValidation(order,false);if(processing.Count==0)Set(Owned?PurchaseState.Complete:productReady?PurchaseState.Ready:PurchaseState.Unavailable);}
        void StartValidation(Order order,bool confirm)
        {
            var items=order.CartOrdered.Items();
            if(items.Count!=1||!PurchaseCatalog.IsKnown(items[0].Product.definition.id)||items[0].Quantity!=1||string.IsNullOrWhiteSpace(order.Info.TransactionID)){Set(PurchaseState.Retry);return;}
            string id=order.Info.TransactionID;if(!processing.Add(id))return;StartCoroutine(Validate(order,confirm));
        }
        [Serializable] sealed class Proof {public string productId,transactionId,store,receipt,jws,bundleId;}
        [Serializable] sealed class Verdict {public bool valid;public string productId,transactionId;}
        IEnumerator Validate(Order order,bool confirm)
        {
            Set(PurchaseState.Busy);
            var proof=new Proof{productId=order.CartOrdered.Items()[0].Product.definition.id,transactionId=order.Info.TransactionID,
                store=order.Info.Apple!=null?"AppleAppStore":"GooglePlay",receipt=order.Info.Receipt,
                jws=order.Info.Apple?.jwsRepresentation,bundleId=UnityEngine.Application.identifier};
            try{
                using(var request=new UnityWebRequest(settings.receiptValidationUrl,"POST")){
                    request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(proof)));request.downloadHandler=new DownloadHandlerBuffer();request.timeout=20;
                    request.SetRequestHeader("Content-Type","application/json");yield return request.SendWebRequest();
                    if(request.result!=UnityWebRequest.Result.Success){Set(PurchaseState.Retry);yield break;}
                    Verdict response=null;try{response=JsonUtility.FromJson<Verdict>(request.downloadHandler.text);}catch(ArgumentException){}
                    bool verified=response!=null&&response.valid&&response.productId==proof.productId&&response.transactionId==proof.transactionId;
                    string receipt=ReceiptId(proof.store,proof.transactionId);
                    bool alreadyGranted=grants.Ledger.Grants.Any(g=>g.Id==receipt);
                    if(!grants.VerifiedStorePurchase(proof.productId,receipt,verified)){Set(PurchaseState.Retry);yield break;}
                    if(!alreadyGranted)pendingCoinNotice+=PurchaseCatalog.Coins(proof.productId);
                    if(confirm){try{store.ConfirmPurchase((PendingOrder)order);}catch(Exception){Set(PurchaseState.Retry);}}
                    else Set(PurchaseState.Complete);
                }
            }finally{processing.Remove(proof.transactionId);}
        }
        static string ReceiptId(string platform,string transaction){using(var hash=SHA256.Create())return "store_"+BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(platform+"|"+transaction))).Replace("-","").ToLowerInvariant();}
        void Confirmed(Order order){Set(order is FailedOrder?PurchaseState.Retry:PurchaseState.Complete);if(pendingCoinNotice>0){int coins=pendingCoinNotice;pendingCoinNotice=0;CoinsGranted?.Invoke(coins);}}
        void Failed(FailedOrder order)=>Set(PurchaseState.Retry);
        void Deferred(DeferredOrder order)=>Set(PurchaseState.Deferred);
        void Disconnected(StoreConnectionFailureDescription error)=>Set(PurchaseState.Retry);
        void ProductsFailed(ProductFetchFailed error)=>Set(PurchaseState.Retry);
        void PurchasesFailed(PurchasesFetchFailureDescription error)=>Set(PurchaseState.Retry);
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
