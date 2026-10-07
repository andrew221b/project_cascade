using System;
using System.Collections;
using Cascade.Application;
using GoogleMobileAds.Common;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace Cascade.Presentation
{
    // SDK callbacks supply evidence; the existing transaction owns eligibility and rewards.
    public sealed class NativeAdProvider:MonoBehaviour
    {
        CascadeGame game;AdService policy;MonetizationSettings settings;
        RewardedAd rewarded,activeRewarded;InterstitialAd interstitial,activeInterstitial;
        AdTicket pending;float deadline,rewardLoadedAt,automaticLoadedAt,retryAt;
        bool privacyReady,initialized,initializing,dead,localPreview,loadingReward,loadingAutomatic;
        int retryCount;
        public bool RewardReady=>initialized&&(localPreview||rewarded!=null&&rewarded.CanShowAd());
        public bool AutomaticReady=>initialized&&(localPreview||interstitial!=null&&interstitial.CanShowAd());
        public bool PrivacyReady=>privacyReady&&initialized;
        public bool Showing=>pending!=null;
        public bool PrivacyOptionsRequired=>!localPreview&&ConsentInformation.PrivacyOptionsRequirementStatus==PrivacyOptionsRequirementStatus.Required;
        void Main(Action action)=>MobileAdsEventExecutor.ExecuteInUpdate(()=>{if(!dead)action();});
        public void Initialize(CascadeGame owner,AdService ledger,MonetizationSettings configuration)
        {
            game=owner;policy=ledger;settings=configuration;
            if(settings==null)return;
#if !UNITY_IOS && !UNITY_ANDROID
            if(policy.PreviewOnly){localPreview=privacyReady=initialized=true;}return;
#else
            if(!settings.admobTestAds&&!settings.adsReleaseReady)return;
            RequestPrivacy();
#endif
        }
#if UNITY_EDITOR
        public void UseLocalPreviewForReview(bool value)
        {
            if(policy?.PreviewOnly!=true||Showing)return;
            localPreview=value;if(value)privacyReady=initialized=true;else RequestPrivacy();
        }
#endif
        void RequestPrivacy()
        {
            var request=new ConsentRequestParameters{TagForUnderAgeOfConsent=settings.underAgeOfConsent};
            ConsentInformation.Update(request,error=>Main(()=>{
                game.NotifyStoreChanged();
                if(error!=null){StartAdsIfAllowed();return;}
                game.SetNativeAdSuspended(true);
                ConsentForm.LoadAndShowConsentFormIfRequired(formError=>Main(()=>{game.SetNativeAdSuspended(false);StartAdsIfAllowed();game.NotifyStoreChanged();}));
            }));
        }
        public void ShowPrivacyOptions()
        {
            if(!PrivacyOptionsRequired||Showing)return;game.SetNativeAdSuspended(true);
            ConsentForm.ShowPrivacyOptionsForm(error=>Main(()=>{
                game.SetNativeAdSuspended(false);privacyReady=ConsentInformation.CanRequestAds();
                if(!privacyReady){DestroyUnits();initialized=false;}
                else StartAdsIfAllowed();game.NotifyStoreChanged();
            }));
        }
        void StartAdsIfAllowed()
        {
            privacyReady=ConsentInformation.CanRequestAds();if(!privacyReady||initialized||initializing)return;
            MobileAds.SetRequestConfiguration(new RequestConfiguration{TagForChildDirectedTreatment=settings.childDirected?TagForChildDirectedTreatment.True:TagForChildDirectedTreatment.Unspecified,
                TagForUnderAgeOfConsent=settings.underAgeOfConsent?TagForUnderAgeOfConsent.True:TagForUnderAgeOfConsent.Unspecified});
            initializing=true;MobileAds.Initialize(status=>Main(()=>{initializing=false;if(status==null)return;initialized=true;LoadUnits();}));
        }
        string Unit(bool video)
        {
            bool ios=UnityEngine.Application.platform==RuntimePlatform.IPhonePlayer;
#if UNITY_EDITOR && UNITY_IOS
            ios=true;
#endif
            return settings.AdUnit(ios,video);
        }
        void LoadUnits()
        {
            if(!initialized||!privacyReady||Showing||localPreview)return;
            if(rewarded==null&&!loadingReward){
                string id=Unit(true);if(!string.IsNullOrWhiteSpace(id)){
                    loadingReward=true;RewardedAd.Load(id,new AdRequest(),(ad,error)=>MobileAdsEventExecutor.ExecuteInUpdate(()=>{
                        loadingReward=false;if(dead){ad?.Destroy();return;}
                        if(error!=null||ad==null){ScheduleRetry();return;}
                        rewarded=ad;rewardLoadedAt=Time.realtimeSinceStartup;retryCount=0;game.NotifyStoreChanged();
                    }));
                }
            }
            if(interstitial==null&&!loadingAutomatic&&!policy.Ledger.NoAutomaticAds){
                string id=Unit(false);if(!string.IsNullOrWhiteSpace(id)){
                    loadingAutomatic=true;InterstitialAd.Load(id,new AdRequest(),(ad,error)=>MobileAdsEventExecutor.ExecuteInUpdate(()=>{
                        loadingAutomatic=false;if(dead){ad?.Destroy();return;}
                        if(error!=null||ad==null){ScheduleRetry();return;}
                        interstitial=ad;automaticLoadedAt=Time.realtimeSinceStartup;retryCount=0;game.NotifyStoreChanged();
                    }));
                }
            }
        }
        void ScheduleRetry(){retryAt=Time.realtimeSinceStartup+Mathf.Min(120,15*Mathf.Pow(2,Mathf.Min(3,retryCount++)));game.NotifyStoreChanged();}
        public bool TryAutomatic(AdContext context)
        {
            var ticket=policy.TryAutomatic(context,AutomaticReady);if(ticket==null)return false;Show(ticket,false);return true;
        }
        public void OfferCoins(AdContext context){var ticket=policy.TryRewarded(context,"coins",RewardReady);if(ticket!=null)Show(ticket,true);}
        void Show(AdTicket ticket,bool video)
        {
            pending=ticket;deadline=Time.realtimeSinceStartup+12;game.SetNativeAdSuspended(true);
            if(localPreview){
                policy.Opened(ticket.Id);deadline=0;
                game.ShowPreviewAd(video,confirmed=>{
                    if(dead||pending!=ticket)return;
                    bool granted=video&&confirmed&&policy.RewardConfirmed(ticket.Id,"preview_"+ticket.Id,true);
                    if(!confirmed)policy.Failed(ticket.Id);Close(ticket);if(granted)game.ShowCoinReward();
                });return;
            }
            try{
                if(video){
                    var ad=rewarded;rewarded=null;activeRewarded=ad;bool granted=false,closed=false;
                    ad.OnAdFullScreenContentOpened+=()=>Main(()=>Opened(ticket));
                    ad.OnAdFullScreenContentFailed+=error=>Main(()=>{Fail(ticket);ad.Destroy();activeRewarded=null;LoadUnits();});
                    ad.OnAdFullScreenContentClosed+=()=>Main(()=>{closed=true;Close(ticket);ad.Destroy();activeRewarded=null;LoadUnits();if(granted)game.ShowCoinReward();else StartCoroutine(ExpireReward(ticket));});
                    ad.Show(reward=>Main(()=>{
                        // The original ticket survives close-before-reward callbacks; a new ad cannot receive this reward.
                        policy.Opened(ticket.Id);
                        granted=policy.RewardConfirmed(ticket.Id,"admob_"+ticket.Id,true);if(granted){game.NotifyStoreChanged();if(closed)game.ShowCoinReward();}
                    }));
                }else{
                    var ad=interstitial;interstitial=null;activeInterstitial=ad;
                    ad.OnAdFullScreenContentOpened+=()=>Main(()=>Opened(ticket));
                    ad.OnAdFullScreenContentFailed+=error=>Main(()=>{Fail(ticket);ad.Destroy();activeInterstitial=null;LoadUnits();});
                    ad.OnAdFullScreenContentClosed+=()=>Main(()=>{Close(ticket);ad.Destroy();activeInterstitial=null;LoadUnits();});ad.Show();
                }
            }catch(Exception){Fail(ticket);activeRewarded?.Destroy();activeRewarded=null;activeInterstitial?.Destroy();activeInterstitial=null;LoadUnits();}
        }
        IEnumerator ExpireReward(AdTicket ticket){yield return new WaitForSecondsRealtime(30);policy.Failed(ticket.Id);}
        void Opened(AdTicket ticket){if(pending!=ticket)return;policy.Opened(ticket.Id);deadline=0;}
        void Close(AdTicket ticket)
        {
            policy.Closed(ticket.Id);if(pending!=ticket)return;pending=null;deadline=0;game.SetNativeAdSuspended(false);game.NotifyStoreChanged();
        }
        void Fail(AdTicket ticket){policy.Failed(ticket.Id);Close(ticket);}
        void DestroyUnits(){rewarded?.Destroy();rewarded=null;interstitial?.Destroy();interstitial=null;}
        void Update()
        {
            if(pending!=null&&deadline>0&&Time.realtimeSinceStartup>=deadline){Fail(pending);LoadUnits();}
            if(!initialized||Showing||localPreview)return;
            float now=Time.realtimeSinceStartup;
            if(rewarded!=null&&now-rewardLoadedAt>3500){rewarded.Destroy();rewarded=null;retryAt=now;}
            if(interstitial!=null&&(policy.Ledger.NoAutomaticAds||now-automaticLoadedAt>3500)){interstitial.Destroy();interstitial=null;retryAt=now;}
            if(retryAt>0&&now>=retryAt){retryAt=0;LoadUnits();}
        }
        void OnDestroy(){dead=true;if(policy!=null&&pending!=null)policy.Failed(pending.Id);DestroyUnits();activeRewarded?.Destroy();activeInterstitial?.Destroy();}
    }
}
