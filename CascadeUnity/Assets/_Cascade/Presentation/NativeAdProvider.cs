using System;
using System.Collections;
using Cascade.Application;
using AdManagerPro;
using GoogleMobileAds.Common;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace Cascade.Presentation
{
    // Game bridge only: AdManagerPro owns SDK initialization, loading, retries and shows.
    // Consent, age treatment and signed game transactions remain at this boundary.
    public sealed class NativeAdProvider:MonoBehaviour
    {
        CascadeGame game;AdService policy;MonetizationSettings settings;
        AdTicket pending;float deadline,rewardLoadedAt,automaticLoadedAt,retryAt;
        bool returnedFromAd,rewardSuppressed,automaticSuppressed;float returnDeadline;
        bool privacyReady,initialized,initializing,dead,localPreview,trackingReady,privacyRequested,privacyLoading;
#if UNITY_EDITOR
        // Isolated PlayMode fixtures can emulate a stalled native load.
        bool loadingReward;
#endif
        int retryCount,privacyRetryCount;float privacyRetryAt;
        public string LastFailure { get; private set; }="";
        public bool RewardReady=>initialized&&game.RewardedOffersAllowed&&(localPreview||RewardedAdManager.IsReady);
        public bool RewardLoading=>privacyLoading||initializing||RewardedAdManager.IsLoading
#if UNITY_EDITOR
            ||loadingReward
#endif
            ;
        public bool AutomaticReady=>initialized&&(localPreview||InterstitialAdManager.IsReady);
        public bool PrivacyReady=>privacyReady&&initialized;
        public bool Showing=>pending!=null;
        public bool PrivacyOptionsRequired=>!localPreview&&ConsentInformation.PrivacyOptionsRequirementStatus==PrivacyOptionsRequirementStatus.Required;
        void Main(Action action)=>MobileAdsEventExecutor.ExecuteInUpdate(()=>{if(!dead)action();});
        public void Initialize(CascadeGame owner,AdService ledger,MonetizationSettings configuration)
        {
            game=owner;policy=ledger;settings=configuration;
            if(settings==null)return;
            RewardedAdManager.AvailabilityChanged+=RewardedAvailabilityChanged;
            InterstitialAdManager.AvailabilityChanged+=InterstitialAvailabilityChanged;
            AdManagerCore.AdFailed+=PluginFailure;
            // UMP answers arrive before MobileAds.Initialize. Its main-thread queue
            // must already have a runner or consent and all ad preparation deadlock.
            MobileAdsEventExecutor.Initialize();
            if(policy.PreviewOnly&&settings.localAdTestScreen){localPreview=privacyReady=initialized=true;return;}
#if UNITY_EDITOR
            // Isolated profile fixtures never open native consent or the vendor's
            // mock-ad prefabs. Explicit preview offers keep their existing path.
            if(!string.IsNullOrEmpty(CascadeGame.EditorProfileDirectory)){
                if(policy.PreviewOnly)localPreview=privacyReady=initialized=true;
                return;
            }
#endif
#if !UNITY_IOS && !UNITY_ANDROID
            if(policy.PreviewOnly){localPreview=privacyReady=initialized=true;}return;
#else
            if(!settings.admobTestAds&&!settings.adsReleaseReady)return;
            TryPreparePrivacy();
#endif
        }
        void TryPreparePrivacy()
        {
            if(dead||settings==null||localPreview||privacyRequested||Time.realtimeSinceStartup<privacyRetryAt
                ||game.AudienceAgeNeedsChoice||game.CurrentScreen==GameScreen.AccountGate||!game.IntroductionAnimationAllowed)return;
            privacyRequested=true;RequestPrivacy();
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
            privacyLoading=true;
            var request=new ConsentRequestParameters{TagForUnderAgeOfConsent=game.AudienceUnderAgeOfConsent||settings.underAgeOfConsent};
            ConsentInformation.Update(request,error=>Main(()=>{
                game.NotifyStoreChanged();
                if(error!=null){PrivacyFailed("consent_update",error.ErrorCode,error.Message);return;}
                game.SetNativeAdSuspended(true);
                ConsentForm.LoadAndShowConsentFormIfRequired(formError=>Main(()=>{
                    privacyLoading=false;game.SetNativeAdSuspended(false);
                    if(formError!=null)PrivacyFailed("consent_form",formError.ErrorCode,formError.Message);
                    else{
                        StartAdsIfAllowed();
                        if(privacyReady)privacyRetryCount=0;
                        else PrivacyFailed("consent_not_ready",-1,"Google consent state does not permit ad requests.");
                        game.NotifyStoreChanged();
                    }
                }));
            }));
        }
        void PrivacyFailed(string stage,int code,string message)
        {
            privacyLoading=false;RecordFailure(stage,"ump",code,message);StartAdsIfAllowed();
            // A cold offline start must not latch consent initialization forever.
            // Recheck Google's actual state; never substitute a saved consent flag.
            if(!privacyReady){privacyRequested=false;privacyRetryAt=Time.realtimeSinceStartup+Mathf.Min(120,15*Mathf.Pow(2,Mathf.Min(3,privacyRetryCount++)));}
            game.NotifyStoreChanged();
        }
        void RecordFailure(string stage,string domain,int code,string message)
        {
            var detail=(message??"").Replace('\n',' ').Replace('\r',' ');if(detail.Length>240)detail=detail.Substring(0,240);
            LastFailure=stage+" ["+domain+":"+code+"] "+detail;
            Debug.LogWarning("[CascadeAds] "+LastFailure);
        }
        float nextHintDiagnostic;
        public void ReportUnavailableHint()
        {
            // Input-triggered, rate-limited and identity-free. A generic no-video
            // banner must be distinguishable from consent/account/load failure.
            if(Time.realtimeSinceStartup<nextHintDiagnostic)return;
            nextHintDiagnostic=Time.realtimeSinceStartup+10;
            string stage=string.IsNullOrEmpty(LastFailure)?"none":LastFailure.Split(' ')[0];
            Debug.LogWarning("[CascadeAds] hint_unavailable privacy="+privacyReady+" initialized="+initialized
                +" loading="+RewardLoading+" eligible="+(game?.RewardedOffersAllowed==true)
                +" ageChoice="+(game?.AudienceAgeNeedsChoice==true)+" failureStage="+stage
                +" rewardGate="+(game?.ServerRewardBlockReason??"no_game")
                +" screen="+game?.CurrentScreen+" introduction="+(game?.IntroductionAnimationAllowed==true)
                +" privacyRequested="+privacyRequested+" privacyLoading="+privacyLoading
                +" trackingReady="+trackingReady+" initializing="+initializing);
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
            bool child=game.AudienceIsChild||settings.childDirected;
            bool underAge=game.AudienceUnderAgeOfConsent||settings.underAgeOfConsent;
            // Advertising consent and Apple's permission are separate. Keep the
            // ad SDK dormant until an eligible adult has answered the system ATT
            // prompt; refusal never blocks play, purchases or contextual ads.
            if(!trackingReady){
                if(!underAge&&!child){
                    initializing=true;
                    IosTrackingConsent.Request(()=>Main(()=>{initializing=false;trackingReady=true;StartAdsIfAllowed();}));
                    return;
                }
                trackingReady=true;
            }
            // Configure the request before SDK initialization. The age screen keeps the
            // ad SDK dormant while the age is unknown; child and teen choices receive
            // Google's restricted treatment and an age-appropriate content ceiling.
            MobileAds.SetRequestConfiguration(new RequestConfiguration{
                AgeRestrictedTreatment=child?AgeRestrictedTreatment.Child:underAge?AgeRestrictedTreatment.Teen:AgeRestrictedTreatment.Unspecified,
                MaxAdContentRating=child?MaxAdContentRating.G:MaxAdContentRating.PG,
                PublisherFirstPartyIdEnabled=!underAge});
            initializing=true;
            AdManagerCore.EnsureInitialized(()=>Main(()=>{initializing=false;initialized=true;LoadUnits();}),
                ()=>Main(()=>{initializing=false;ScheduleRetry();}));
        }
        string Unit(bool video)
        {
            var config=AdManagerCore.Settings;
            return config==null?"":video?config.GetRewardedVideoID():config.GetInterstitialID();
        }
        void PluginFailure(string stage,string domain,int code,string message)
        {if(!dead)RecordFailure(stage,domain,code,message);}
        void RewardedAvailabilityChanged()
        {if(dead)return;if(RewardedAdManager.IsReady)rewardLoadedAt=Time.realtimeSinceStartup;game?.NotifyStoreChanged();}
        void InterstitialAvailabilityChanged()
        {if(dead)return;if(InterstitialAdManager.IsReady)automaticLoadedAt=Time.realtimeSinceStartup;game?.NotifyStoreChanged();}
        void LoadUnits()
        {
            if(!initialized||!privacyReady||Showing||localPreview)return;
            if(game.RewardedOffersAllowed)RewardedAdManager.RequestRewardedAd();
            if(!policy.Ledger.HasNoAutomaticAds)InterstitialAdManager.RequestAdInterstitial();
        }
        void ScheduleRetry(){retryAt=Time.realtimeSinceStartup+Mathf.Min(120,15*Mathf.Pow(2,Mathf.Min(3,retryCount++)));game.NotifyStoreChanged();}
        public bool TryAutomatic(AdContext context)
        {
            var ticket=policy.TryAutomatic(context,AutomaticReady);if(ticket==null)return false;Show(ticket,false);return true;
        }
        public bool TryAutomaticRetry(AdContext context)
        {
            var ticket=policy.TryAutomaticRetry(context,AutomaticReady);if(ticket==null)return false;Show(ticket,false);return true;
        }
        public bool OfferHint(AdContext context){var ticket=policy.TryRewarded(context,"hint",RewardReady);if(ticket==null)return false;Show(ticket,true);return true;}
        public void OfferCoins(AdContext context){var ticket=policy.TryRewarded(context,"coins",RewardReady);if(ticket!=null)Show(ticket,true);}
        public bool OfferParcelOpen(AdContext context){var ticket=policy.TryRewarded(context,"parcel_open",RewardReady);if(ticket==null)return false;Show(ticket,true);return true;}
        public bool OfferParcelBonus(AdContext context){var ticket=policy.TryRewarded(context,"parcel_bonus",RewardReady);if(ticket==null)return false;Show(ticket,true);return true;}
        async void Show(AdTicket ticket,bool video)
        {
            pending=ticket;returnedFromAd=false;returnDeadline=0;deadline=Time.realtimeSinceStartup+12;game.SetNativeAdSuspended(true);
            if(localPreview){
                policy.Opened(ticket.Id);deadline=0;
                game.ShowPreviewAd(video,confirmed=>{
                    if(dead||pending!=ticket)return;
                    bool granted=video&&confirmed&&policy.RewardConfirmed(ticket.Id,"preview_"+ticket.Id,true);
                    if(!confirmed)policy.Failed(ticket.Id);Close(ticket);if(granted)game.ShowAdReward(ticket);
                });return;
            }
            CloudSyncClient.ServerRewardIntent serverIntent=null;
            if(video&&game.UsesServerRewards){
                serverIntent=await game.PrepareServerReward(ticket,Unit(true));
                if(dead||pending!=ticket)return;
                if(serverIntent==null){
                    RecordFailure("reward_intent","cloud",-1,"Could not prepare signed reward; gate="+game.ServerRewardBlockReason);
                    Fail(ticket);return;
                }
            }
            if(dead||pending!=ticket)return;
            if(video){
                bool granted=false,closed=false;
                var verification=serverIntent==null?null:new ServerSideVerificationOptions{UserId=serverIntent.UserId,CustomData=serverIntent.Id};
                bool shown=RewardedAdManager.ShowRewardedAd(watched=>{
                    if(dead)return;closed=true;Close(ticket);LoadUnits();
                    if(granted)game.ShowAdReward(ticket);else StartCoroutine(ExpireReward(ticket));
                },()=>{if(!dead)Opened(ticket);},()=>{
                    if(dead)return;policy.Opened(ticket.Id);
                    if(serverIntent!=null){StartCoroutine(WaitForVerifiedReward(ticket,serverIntent.Id));return;}
                    granted=policy.RewardConfirmed(ticket.Id,"admob_"+ticket.Id,true);
                    if(granted){game.NotifyStoreChanged();if(closed)game.ShowAdReward(ticket);}
                },verification);
                if(!shown&&pending==ticket)Fail(ticket);
            }else if(!InterstitialAdManager.ShowAdInterstitial(()=>{if(!dead){Close(ticket);if(policy.Ledger.HasNoAutomaticAds)InterstitialAdManager.Reset();LoadUnits();}},()=>{if(!dead)Opened(ticket);}))Fail(ticket);
        }
        IEnumerator WaitForVerifiedReward(AdTicket ticket,string intent){
            for(int attempt=0;attempt<30&&!dead;attempt++){
                var task=game.RefreshServerReward(intent);while(!task.IsCompleted&&!dead)yield return null;
                if(dead)yield break;
                if(task.Status==System.Threading.Tasks.TaskStatus.RanToCompletion&&task.Result){policy.Failed(ticket.Id);while(pending==ticket&&!dead)yield return null;if(!dead){game.NotifyStoreChanged();game.ShowAdReward(ticket);}yield break;}
                yield return new WaitForSecondsRealtime(attempt<4?.25f:attempt<8?.5f:1);
            }
            // A delayed signed callback is still reconciled by the next ordinary cloud read/sync.
        }
        IEnumerator ExpireReward(AdTicket ticket){yield return new WaitForSecondsRealtime(30);policy.Failed(ticket.Id);}
        void Opened(AdTicket ticket){if(pending!=ticket)return;policy.Opened(ticket.Id);deadline=0;}
        void Close(AdTicket ticket)
        {
            policy.Closed(ticket.Id);if(pending!=ticket)return;pending=null;deadline=0;returnedFromAd=false;returnDeadline=0;game.SetNativeAdSuspended(false);game.NotifyStoreChanged();
        }
        void Fail(AdTicket ticket){policy.Failed(ticket.Id);Close(ticket);}
        void DestroyUnits(){RewardedAdManager.Reset();InterstitialAdManager.Reset();}
        void OnApplicationPause(bool paused)
        {
            if(pending==null||localPreview)return;
            if(paused)returnedFromAd=true;
            else if(returnedFromAd)returnDeadline=Time.realtimeSinceStartup+3;
        }
        void OnApplicationFocus(bool focused)
        {
            if(pending==null||localPreview)return;
            if(!focused)returnedFromAd=true;
            else if(returnedFromAd)returnDeadline=Time.realtimeSinceStartup+3;
        }
        void Update()
        {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            if(settings!=null&&(settings.admobTestAds||settings.adsReleaseReady))TryPreparePrivacy();
#endif
            // Recover missing SDK close callbacks only after the OS returns control to the game.
            if(pending!=null&&returnDeadline>0&&Time.realtimeSinceStartup>=returnDeadline) {
                var ticket=pending;Close(ticket);StartCoroutine(ExpireReward(ticket));
                DestroyUnits();LoadUnits();
            }
            if(pending!=null&&deadline>0&&Time.realtimeSinceStartup>=deadline){Fail(pending);LoadUnits();}
            float now=Time.realtimeSinceStartup;
            if(!initialized&&privacyReady&&!initializing&&!localPreview&&game.IntroductionAnimationAllowed&&now>=retryAt){retryAt=0;StartAdsIfAllowed();}
            if(!initialized||Showing||localPreview)return;
            // Stop the plugin's pending retries when the game revokes an offer,
            // rather than silently continuing requests after a no-ads purchase.
            bool suppressRewards=!game.RewardedOffersAllowed,suppressAutomatic=policy.Ledger.HasNoAutomaticAds;
            if(rewardSuppressed!=suppressRewards){rewardSuppressed=suppressRewards;if(suppressRewards)RewardedAdManager.Reset();}
            if(automaticSuppressed!=suppressAutomatic){automaticSuppressed=suppressAutomatic;if(suppressAutomatic)InterstitialAdManager.Reset();}
            if(RewardedAdManager.IsReady&&now-rewardLoadedAt>3500)RewardedAdManager.Reset();
            if(InterstitialAdManager.IsReady&&(policy.Ledger.HasNoAutomaticAds||now-automaticLoadedAt>3500))InterstitialAdManager.Reset();
            // Managers coalesce requests and own their retry backoff.
            if(retryAt<=0&&(game.RewardedOffersAllowed&&!RewardedAdManager.IsReady
                ||!policy.Ledger.HasNoAutomaticAds&&!InterstitialAdManager.IsReady))LoadUnits();
            if(retryAt>0&&now>=retryAt){retryAt=0;LoadUnits();}
        }
        void OnDestroy()
        {
            dead=true;RewardedAdManager.AvailabilityChanged-=RewardedAvailabilityChanged;
            InterstitialAdManager.AvailabilityChanged-=InterstitialAvailabilityChanged;AdManagerCore.AdFailed-=PluginFailure;
            if(policy!=null&&pending!=null)policy.Failed(pending.Id);DestroyUnits();
        }
    }
}
