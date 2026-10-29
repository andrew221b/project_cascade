using System.Linq;
using Cascade.Application;
using Cascade.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        AdService monetization,virtualEconomy;NativePurchaseStore purchaseStore;NativeAdProvider adProvider;
        bool nativeAdSuspended,firstMonetizationSession,storeRefreshPending;int purchasedCoinNotice;
        CanvasGroup nativeAdGate;
        RectTransform coinVideoButton;
        public bool TestMonetization=>monetization?.PreviewOnly==true;
        // Cloud grants are server-owned; client rewarded callbacks stay session-only until SSV integration.
        public bool RewardedOffersAllowed=>TestMonetization||cloudSave?.EnabledForPlayer!=true||cloudSave.CanVerifyRewards;
        public NativeAdProvider AdProvider=>adProvider;
        public NativePurchaseStore PurchaseStore=>purchaseStore;
        public AdService Monetization=>monetization;
        // Fake native callbacks cannot spend or replace the player's earned inventory.
        AdService GameEconomy=>virtualEconomy??monetization;
        void InitializeMonetization()
        {
            firstMonetizationSession=profile.Clears.Count==0;
            var settings=Resources.Load<MonetizationSettings>("CascadeArt/MonetizationSettings");
#if CASCADE_STORE_CAPTURE
            settings=null; // Dedicated offline capture build: no native stores, consent requests or ads.
#endif
            monetization=settings?.testMonetization==true&&!saveBlocked?
                MonetizationTransaction.CreatePreview(profile,profileStore,()=>Interaction.Attempt,()=>IsPlanningDescent||IsPlanningDirection||descentPlanFailed):MonetizationTransaction.Create(profile,profileStore,()=>Interaction.Attempt,()=>IsPlanningDescent||IsPlanningDirection||descentPlanFailed,QueueCloudSave);
            virtualEconomy=TestMonetization?MonetizationTransaction.Create(profile,profileStore,()=>Interaction.Attempt,()=>IsPlanningDescent||IsPlanningDirection||descentPlanFailed,QueueCloudSave):monetization;
            var receiptVerifier=AudienceUnderAgeOfConsent?new PublicReceiptVerifier(settings?.receiptValidationUrl):null;
            purchaseStore=gameObject.AddComponent<NativePurchaseStore>();purchaseStore.Initialize(monetization,saveBlocked?null:settings,()=>ShopCoins,cloudSave?.CanVerifyPurchases!=true?null:(System.Func<NativePurchaseStore.Proof,System.Threading.Tasks.Task<bool>>)(proof=>cloudSave.VerifyPurchaseAsync(proof.productId,proof.transactionId,proof.store,proof.receipt,proof.jws,proof.bundleId)),cloudSave==null?null:(System.Func<System.Threading.Tasks.Task>)cloudSave.InitializeServicesAsync,cloudSave==null?null:(System.Func<System.Threading.Tasks.Task<Cascade.Persistence.PurchaseIdentity>>)cloudSave.PreparePurchaseIdentityAsync,receiptVerifier?.Configured==true?receiptVerifier.VerifyAsync:null,()=>CurrentScreen!=GameScreen.AccountGate&&cloudSave?.SignedOut!=true&&cloudSave?.AccountOperationPending!=true);purchaseStore.Changed+=NotifyStoreChanged;purchaseStore.CoinsGranted+=QueuePurchasedCoins;
            adProvider=gameObject.AddComponent<NativeAdProvider>();adProvider.Initialize(this,monetization,saveBlocked?null:settings);
        }
        UiCopy CoinPurchaseCaption(string product,bool enough)
        {
            if(purchaseStore?.State==PurchaseState.Busy)return "Checking purchase…";
            if(purchaseStore?.State==PurchaseState.Connecting)return ReferenceCopy("Loading store…","Загружаем магазин…","Завантажуємо магазин…");
            if(purchaseStore?.State==PurchaseState.Deferred)return ReferenceCopy("Awaiting approval","Ожидает одобрения","Очікує схвалення");
            if(enough)return "You have enough coins";
            if(TestMonetization)return "Test shop";
            if(purchaseStore?.CanBuyProduct(product)==true)return new UiCopy("Buy {0}",UiCopy.Raw(purchaseStore.ProductPrice(product)));
            return purchaseStore?.State==PurchaseState.Retry?(UiCopy)"Try again":"Unavailable";
        }
        void QueuePurchasedCoins(int amount){purchasedCoinNotice+=amount;storeRefreshPending=true;}
        void FlushStoreFeedback()
        {
            if(!CanPresent||!applicationFocused||applicationPaused||nativeAdSuspended)return;
            if(storeRefreshPending){storeRefreshPending=false;NotifyStoreChanged();ApplyMenuSpriteCounters();}
            if(purchasedCoinNotice<=0||menuRoot==null||!menuRoot.gameObject.activeInHierarchy||CurrentScreen==GameScreen.AccountGate)return;
            int amount=purchasedCoinNotice;purchasedCoinNotice=0;ShowCoinReward(amount,true);
        }
        public void NotifyStoreChanged(){if(!applicationFocused||applicationPaused||!CanPresent){storeRefreshPending=true;return;}if(CurrentScreen==GameScreen.Parcel){RefreshParcelAdAvailability();return;}if(CurrentScreen==GameScreen.Playing&&Interaction!=null&&hint!=null){RenderControls();return;}if(CurrentScreen==GameScreen.TravelBag||CurrentScreen==GameScreen.NoAds||CurrentScreen==GameScreen.Shop||CurrentScreen==GameScreen.Supplies||CurrentScreen==GameScreen.CoinPacks||CurrentScreen==GameScreen.Settings)DrawMenu();}
        public void SetNativeAdSuspended(bool value)
        {
            nativeAdSuspended=value;if(value){EndTokenHold();Interaction.CancelPointer();}
            if(nativeAdGate==null)nativeAdGate=safeRoot.gameObject.AddComponent<CanvasGroup>();nativeAdGate.interactable=!value;
            Interaction.SetSuspended(nativeAdSuspended||applicationPaused||!applicationFocused||landscapeLayoutBlocked);
        }
        AdContext AdRequest(AdSurface surface)=>new AdContext{Surface=surface,CoreAccepted=true,
            PrivacyReady=adProvider!=null&&adProvider.PrivacyReady,ProviderEnabled=adProvider!=null&&adProvider.PrivacyReady,
            FirstSession=firstMonetizationSession,UniqueClears=profile.Clears.Count,NodeId=node,
            SavedWin=!HasSaveError&&!saveBlocked,BaseRewardGiven=profile.Clears.Exists(c=>c.CampaignNodeId==node),Replay=surface==AdSurface.Win&&(profile.Completion==null||profile.Completion.Lights==0),
            ChapterFinal=Cascade.Content.JourneyStory.IsWorldFinal(node),ContinueVisible=false};
        bool TryAutomaticAd()=>adProvider!=null&&adProvider.TryAutomatic(AdRequest(AdSurface.Win));
        bool TryAutomaticRetryAd()=>adProvider!=null&&adProvider.TryAutomaticRetry(AdRequest(AdSurface.Retry));
        public void WatchCoinVideo(){if((CurrentScreen==GameScreen.Shop||CurrentScreen==GameScreen.TravelBag)&&RewardedOffersAllowed&&!MechanicPopupOpen&&!saveBlocked&&!HasSaveError)adProvider?.OfferCoins(AdRequest(AdSurface.Inventory));}
        void WatchParcelOpen(int order)
        {
            if(CurrentScreen!=GameScreen.Parcel||order!=parcelOrder||!ParcelAwaitingVideo||!InventoryRewardAvailable)return;
            var context=AdRequest(AdSurface.Inventory);context.RewardSubject=MonetizationLedger.ParcelSubject(order);adProvider?.OfferParcelOpen(context);
        }
        public void WatchParcelBonus(int order)
        {
            var receipt=Rewards?.Parcels.FirstOrDefault(p=>p.Order==order);
            if(CurrentScreen!=GameScreen.Parcel||receipt==null||ParcelAwaitingVideo||!parcelOpened&&!receipt.Claimed||saveBlocked||HasSaveError||monetization==null||!RewardedOffersAllowed
                ||monetization.Ledger.HideVideoOffers||monetization.HasParcelBonus(MonetizationLedger.ParcelSubject(order)))return;
            var context=AdRequest(AdSurface.Inventory);context.RewardSubject=MonetizationLedger.ParcelSubject(order);adProvider?.OfferParcelBonus(context);
        }
        void RewardedShopOffer()
        {
            if(adProvider?.RewardReady!=true||monetization.Ledger.HideVideoOffers||!RewardedOffersAllowed){coinVideoButton=null;return;}
            var b=CounterButton("Watch coin video","Video +40",.055f,.49f,.826f,.891f,WatchCoinVideo,false);
            b.interactable=adProvider?.RewardReady==true&&!saveBlocked&&!monetization.Ledger.HideVideoOffers&&monetization.CanReceiveGrant;
            menuRows.RemoveAll(row=>row.rect==(RectTransform)b.transform);
            var label=b.GetComponentInChildren<TextMeshProUGUI>();label.fontSizeMin=12*MenuUnit;label.fontSizeMax=16*MenuUnit;
            coinVideoButton=(RectTransform)b.transform;FitCoinVideoOffer(MenuUnit);
        }
        void FitCoinVideoOffer(float unit){if(coinVideoButton!=null)coinVideoButton.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,48*unit);}
        void StorePurchaseButton(float bottom)
        {
            UiCopy caption=purchaseStore?.Owned==true?"Ads removed":purchaseStore?.CanBuy==true?TestMonetization?(UiCopy)"Try removing ads":new UiCopy("Buy {0}",UiCopy.Raw(purchaseStore.Price)):
                purchaseStore?.State==PurchaseState.Retry?"Try again":purchaseStore?.State==PurchaseState.Busy?"Checking purchase…":"Coming soon";
            var b=NavigationButton("Purchasequiet",caption,3,bottom,()=>RequestStorePurchase(PurchaseCatalog.NoAdsYear),true);
            b.interactable=purchaseStore!=null&&(purchaseStore.CanBuy||purchaseStore.State==PurchaseState.Retry);
            if(!b.interactable)b.gameObject.AddComponent<CanvasGroup>().alpha=.57f;
        }
        UiCopy PurchaseExplanation=>TestMonetization?(purchaseStore?.Owned==true?(UiCopy)"Test purchase complete. Automatic ads are off for this session.":"Test only. No payment."):
            purchaseStore?.Owned==true?"Ad breaks are off":
            purchaseStore!=null&&!purchaseStore.HasGrantCapacity?"Purchases are temporarily unavailable. Contact support.":
            purchaseStore?.State==PurchaseState.Deferred?"Waiting for purchase approval. You can keep playing.":
            purchaseStore?.State==PurchaseState.Retry?"The store could not finish. Try again. Your purchase will not be lost.":
            purchaseStore?.State==PurchaseState.Busy||purchaseStore?.State==PurchaseState.Connecting?"Checking with the store. Please wait.":
            purchaseStore?.State==PurchaseState.Unconfigured||purchaseStore?.State==PurchaseState.Unavailable?"The shop is not open yet. You can enjoy the whole journey without purchases.":
            "Removes ad breaks. Reward videos stay optional.";
    }
}
