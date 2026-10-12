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
        bool nativeAdSuspended,firstMonetizationSession;
        CanvasGroup nativeAdGate;
        RectTransform coinVideoButton;
        public bool TestMonetization=>monetization?.PreviewOnly==true;
        // Cloud grants are server-owned; client rewarded callbacks stay session-only until SSV integration.
        public bool RewardedOffersAllowed=>TestMonetization||cloudSave?.EnabledForPlayer!=true;
        public NativeAdProvider AdProvider=>adProvider;
        public NativePurchaseStore PurchaseStore=>purchaseStore;
        public AdService Monetization=>monetization;
        // Fake native callbacks cannot spend or replace the player's earned inventory.
        AdService GameEconomy=>virtualEconomy??monetization;
        void InitializeMonetization()
        {
            firstMonetizationSession=profile.Clears.Count==0;
            var settings=Resources.Load<MonetizationSettings>("CascadeArt/MonetizationSettings");
            monetization=settings?.testMonetization==true&&!saveBlocked?
                MonetizationTransaction.CreatePreview(profile,profileStore,()=>Interaction.Attempt,()=>IsPlanningDescent||IsPlanningDirection||descentPlanFailed):MonetizationTransaction.Create(profile,profileStore,()=>Interaction.Attempt,()=>IsPlanningDescent||IsPlanningDirection||descentPlanFailed,QueueCloudSave);
            virtualEconomy=TestMonetization?MonetizationTransaction.Create(profile,profileStore,()=>Interaction.Attempt,()=>IsPlanningDescent||IsPlanningDirection||descentPlanFailed,QueueCloudSave):monetization;
            purchaseStore=gameObject.AddComponent<NativePurchaseStore>();purchaseStore.Initialize(monetization,saveBlocked?null:settings,()=>ShopCoins,cloudSave?.CanVerifyPurchases!=true?null:(System.Func<NativePurchaseStore.Proof,System.Threading.Tasks.Task<bool>>)(proof=>cloudSave.VerifyPurchaseAsync(proof.productId,proof.transactionId,proof.store,proof.receipt,proof.jws,proof.bundleId)),cloudSave==null?null:(System.Func<System.Threading.Tasks.Task>)cloudSave.InitializeServicesAsync,cloudSave==null?null:(System.Func<System.Threading.Tasks.Task<Cascade.Persistence.PurchaseIdentity>>)cloudSave.PreparePurchaseIdentityAsync);purchaseStore.Changed+=NotifyStoreChanged;purchaseStore.CoinsGranted+=amount=>{NotifyStoreChanged();ShowCoinReward(amount);};
            adProvider=gameObject.AddComponent<NativeAdProvider>();adProvider.Initialize(this,monetization,saveBlocked?null:settings);
        }
        public void NotifyStoreChanged(){if(CurrentScreen==GameScreen.Playing&&Interaction!=null&&hint!=null){RenderControls();return;}if(CurrentScreen==GameScreen.NoAds||CurrentScreen==GameScreen.Shop||CurrentScreen==GameScreen.Supplies||CurrentScreen==GameScreen.CoinPacks||CurrentScreen==GameScreen.Settings)DrawMenu();}
        public void SetNativeAdSuspended(bool value)
        {
            nativeAdSuspended=value;if(value){EndTokenHold();Interaction.CancelPointer();}
            if(nativeAdGate==null)nativeAdGate=safeRoot.gameObject.AddComponent<CanvasGroup>();nativeAdGate.interactable=!value;
            Interaction.SetSuspended(nativeAdSuspended||applicationPaused||!applicationFocused);
        }
        AdContext AdRequest(AdSurface surface)=>new AdContext{Surface=surface,CoreAccepted=true,
            PrivacyReady=adProvider!=null&&adProvider.PrivacyReady,ProviderEnabled=adProvider!=null&&adProvider.PrivacyReady,
            FirstSession=firstMonetizationSession,UniqueClears=profile.Clears.Count,NodeId=node,
            SavedWin=!HasSaveError&&!saveBlocked,BaseRewardGiven=profile.Clears.Exists(c=>c.CampaignNodeId==node),Replay=surface==AdSurface.Win&&(profile.Completion==null||profile.Completion.Lights==0),
            ChapterFinal=Cascade.Content.JourneyStory.IsWorldFinal(node),ContinueVisible=false};
        bool TryAutomaticAd()=>adProvider!=null&&adProvider.TryAutomatic(AdRequest(AdSurface.Win));
        bool TryAutomaticRetryAd()=>adProvider!=null&&adProvider.TryAutomaticRetry(AdRequest(AdSurface.Retry));
        public void WatchCoinVideo(){if(CurrentScreen==GameScreen.Shop&&!MechanicPopupOpen&&!saveBlocked&&!HasSaveError)adProvider?.OfferCoins(AdRequest(AdSurface.Inventory));}
        public void WatchParcelBonus(int order)
        {
            var receipt=Rewards?.Parcels.FirstOrDefault(p=>p.Order==order);
            if(CurrentScreen!=GameScreen.Parcel||receipt==null||!parcelOpened&&!receipt.Claimed||saveBlocked||HasSaveError||monetization==null||!RewardedOffersAllowed
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
            var b=NavigationButton("Purchasequiet",caption,3,bottom,()=>{if(purchaseStore.State==PurchaseState.Retry)purchaseStore.RetryConnection();else purchaseStore.Buy();},true);
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
