using Cascade.Application;
using Cascade.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        AdService monetization;NativePurchaseStore purchaseStore;NativeAdProvider adProvider;
        bool nativeAdSuspended,firstMonetizationSession;
        CanvasGroup nativeAdGate;
        RectTransform coinVideoButton;
        public bool TestMonetization=>monetization?.PreviewOnly==true;
        public NativeAdProvider AdProvider=>adProvider;
        public NativePurchaseStore PurchaseStore=>purchaseStore;
        public AdService Monetization=>monetization;
        void InitializeMonetization()
        {
            firstMonetizationSession=profile.Clears.Count==0;
            var settings=Resources.Load<MonetizationSettings>("CascadeArt/MonetizationSettings");
            monetization=settings?.testMonetization==true&&!saveBlocked?
                MonetizationTransaction.CreatePreview(profile,profileStore,()=>Interaction.Attempt):MonetizationTransaction.Create(profile,profileStore,()=>Interaction.Attempt);
            purchaseStore=gameObject.AddComponent<NativePurchaseStore>();purchaseStore.Initialize(monetization,saveBlocked?null:settings);purchaseStore.Changed+=NotifyStoreChanged;
            adProvider=gameObject.AddComponent<NativeAdProvider>();adProvider.Initialize(this,monetization,saveBlocked?null:settings);
        }
        public void NotifyStoreChanged(){if(CurrentScreen==GameScreen.NoAds||CurrentScreen==GameScreen.Shop||CurrentScreen==GameScreen.Supplies)DrawMenu();}
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
            ChapterFinal=int.TryParse(node.TrimStart('C'),out var stop)&&stop%10==0,ContinueVisible=false};
        bool TryAutomaticAd()=>adProvider!=null&&adProvider.TryAutomatic(AdRequest(AdSurface.Win));
        public void WatchCoinVideo(){if(CurrentScreen==GameScreen.Shop&&!MechanicPopupOpen&&!saveBlocked&&!HasSaveError)adProvider?.OfferCoins(AdRequest(AdSurface.Inventory));}
        void RewardedShopOffer()
        {
            if(monetization.Ledger.HideVideoOffers)return;
            var b=CounterButton("Watch coin video",adProvider?.RewardReady==true?(UiCopy)"Video · +40":"Loading video…",.46f,.91f,.805f,.862f,WatchCoinVideo,true);
            b.interactable=adProvider?.RewardReady==true&&!saveBlocked&&!monetization.Ledger.HideVideoOffers;
            coinVideoButton=(RectTransform)b.transform;FitCoinVideoOffer(MenuUnit);
        }
        void FitCoinVideoOffer(float unit){if(coinVideoButton!=null)coinVideoButton.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,44*unit);}
        void StorePurchaseButton(float bottom)
        {
            UiCopy caption=purchaseStore?.Owned==true?"Ads removed":purchaseStore?.CanBuy==true?TestMonetization?(UiCopy)"Try removing ads":new UiCopy("Buy · {0}",UiCopy.Raw(purchaseStore.Price)):
                purchaseStore?.State==PurchaseState.Retry?"Try again":purchaseStore?.State==PurchaseState.Busy?"Checking purchase…":"Coming soon";
            var b=NavigationButton("Purchasequiet",caption,3,bottom,()=>{if(purchaseStore.State==PurchaseState.Retry)purchaseStore.RetryConnection();else purchaseStore.Buy();},true);
            b.interactable=purchaseStore!=null&&(purchaseStore.CanBuy||purchaseStore.State==PurchaseState.Retry);
            if(!b.interactable)b.gameObject.AddComponent<CanvasGroup>().alpha=.57f;
        }
        UiCopy PurchaseExplanation=>TestMonetization?(purchaseStore?.Owned==true?(UiCopy)"Test purchase complete. Automatic ads are off for this session.":"Test shop. No money is charged. Try removing automatic ads."):
            purchaseStore?.Owned==true?"Automatic ads are off. Thank you for supporting Mika's journey.":
            purchaseStore?.State==PurchaseState.Deferred?"Waiting for purchase approval. You can keep playing.":
            purchaseStore?.State==PurchaseState.Retry?"The store could not finish. Try again. Your purchase will not be lost.":
            purchaseStore?.State==PurchaseState.Busy||purchaseStore?.State==PurchaseState.Connecting?"Checking with the store. Please wait.":
            purchaseStore?.State==PurchaseState.Unconfigured||purchaseStore?.State==PurchaseState.Unavailable?"The shop is not open yet. You can enjoy the whole journey without purchases.":
            "A one-time purchase removes automatic ads. Optional videos stay your choice.";
    }
}
