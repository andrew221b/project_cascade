#if ADS
using System;
using GoogleMobileAds.Api;
namespace AdManagerPro
{
    public static class RewardedAdManager
    {
        static RewardedAd rewardedAd,activeAd;
        public static bool isAdmobRewardedReady=false;
        public static bool IsLoading { get; private set; }
        public static bool IsReady=>isAdmobRewardedReady&&rewardedAd!=null&&rewardedAd.CanShowAd();
        public static event Action AvailabilityChanged;
        static int retryAttempt,generation;
        static bool retryScheduled;
        public static void RequestRewardedAd()
        {
            if(IsLoading||retryScheduled||activeAd!=null||IsReady)return;
            IsLoading=true;int requestGeneration=generation;
            AdManagerCore.EnsureInitialized(()=>{if(requestGeneration==generation)LoadAd(requestGeneration);},()=>{
                if(requestGeneration!=generation)return;IsLoading=false;RetryLoad(requestGeneration);});
        }
        static void LoadAd(int requestGeneration)
        {
            string id=AdManagerCore.Settings.GetRewardedVideoID();
            if(string.IsNullOrWhiteSpace(id)){IsLoading=false;AdManagerCore.ReportFailure("rewarded_load","configuration",-1,"Rewarded ID is empty");AvailabilityChanged?.Invoke();return;}
            rewardedAd?.Destroy();rewardedAd=null;isAdmobRewardedReady=false;
            RewardedAd.Load(id,new AdRequest(),(ad,error)=>{
                if(requestGeneration!=generation){ad?.Destroy();return;}
                IsLoading=false;
                if(error!=null||ad==null){ad?.Destroy();RetryLoad(requestGeneration);AdManagerCore.ReportFailure("rewarded_load",error?.GetDomain()??"gma",error?.GetCode()??-1,error?.GetMessage()??"Empty response");return;}
                retryAttempt=0;rewardedAd=ad;isAdmobRewardedReady=true;AvailabilityChanged?.Invoke();
            });
        }
        static void RetryLoad(int requestGeneration)
        {
            isAdmobRewardedReady=false;retryScheduled=true;AvailabilityChanged?.Invoke();
            AdManagerCore.RetryAfterBackoff(++retryAttempt,()=>{if(requestGeneration!=generation)return;retryScheduled=false;RequestRewardedAd();});
        }
        public static void ShowRewardedAd(Action<bool> onAdWatched)=>ShowRewardedAd(onAdWatched,null,null,null);
        // Game integrations may attach an SSV intent before show and receive earned
        // evidence separately, including when the SDK delivers it after close.
        public static bool ShowRewardedAd(Action<bool> onClosed,Action onOpened,Action onEarned,ServerSideVerificationOptions verification)
        {
            if(!IsReady){onClosed?.Invoke(false);RequestRewardedAd();return false;}
            var ad=rewardedAd;rewardedAd=null;activeAd=ad;isAdmobRewardedReady=false;
            int showGeneration=generation;bool earned=false,completed=false,failed=false;
            void Complete(){if(completed||showGeneration!=generation)return;completed=true;
                if(ReferenceEquals(activeAd,ad))activeAd=null;ad.Destroy();
                onClosed?.Invoke(earned);AvailabilityChanged?.Invoke();if(showGeneration==generation)RequestRewardedAd();}
            try{
                if(verification!=null)ad.SetServerSideVerificationOptions(verification);
                ad.OnAdFullScreenContentOpened+=()=>{if(showGeneration==generation&&!failed)onOpened?.Invoke();};
                ad.OnAdFullScreenContentFailed+=error=>{if(showGeneration!=generation)return;failed=true;
                    AdManagerCore.ReportFailure("rewarded_show",error?.GetDomain()??"gma",error?.GetCode()??-1,error?.GetMessage());Complete();};
                ad.OnAdFullScreenContentClosed+=Complete;
                ad.Show(reward=>{if(showGeneration!=generation||failed||earned)return;earned=true;onEarned?.Invoke();});
                return true;
            }catch(Exception e){failed=true;AdManagerCore.ReportFailure("rewarded_show","gma",-1,e.Message);Complete();return false;}
        }
        public static void Reset()
        {
            generation++;retryScheduled=IsLoading=false;retryAttempt=0;isAdmobRewardedReady=false;
            rewardedAd?.Destroy();rewardedAd=null;activeAd?.Destroy();activeAd=null;AvailabilityChanged?.Invoke();
        }
    }
}
#endif
