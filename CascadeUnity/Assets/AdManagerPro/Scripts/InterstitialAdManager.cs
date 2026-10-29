#if ADS
using System;
using GoogleMobileAds.Api;
namespace AdManagerPro
{
    public static class InterstitialAdManager
    {
        static InterstitialAd interstitial,activeAd;
        public static bool isAdmobInterstitialReady=false;
        public static bool IsLoading { get; private set; }
        public static bool IsReady=>isAdmobInterstitialReady&&interstitial!=null&&interstitial.CanShowAd();
        public static event Action InterstitialFinished,AvailabilityChanged;
        static int retryAttempt,generation;static bool retryScheduled;
        public static void RequestAdInterstitial()
        {
            if(IsLoading||retryScheduled||activeAd!=null||IsReady)return;
            IsLoading=true;int requestGeneration=generation;
            AdManagerCore.EnsureInitialized(()=>{if(requestGeneration==generation)LoadAd(requestGeneration);},()=>{
                if(requestGeneration!=generation)return;IsLoading=false;RetryLoad(requestGeneration);});
        }
        static void LoadAd(int requestGeneration)
        {
            string id=AdManagerCore.Settings.GetInterstitialID();
            if(string.IsNullOrWhiteSpace(id)){IsLoading=false;AdManagerCore.ReportFailure("interstitial_load","configuration",-1,"Interstitial ID is empty");AvailabilityChanged?.Invoke();return;}
            interstitial?.Destroy();interstitial=null;isAdmobInterstitialReady=false;
            InterstitialAd.Load(id,new AdRequest(),(ad,error)=>{
                if(requestGeneration!=generation){ad?.Destroy();return;}IsLoading=false;
                if(error!=null||ad==null){ad?.Destroy();RetryLoad(requestGeneration);AdManagerCore.ReportFailure("interstitial_load",error?.GetDomain()??"gma",error?.GetCode()??-1,error?.GetMessage()??"Empty response");return;}
                retryAttempt=0;interstitial=ad;isAdmobInterstitialReady=true;AvailabilityChanged?.Invoke();
            });
        }
        static void RetryLoad(int requestGeneration)
        {
            isAdmobInterstitialReady=false;retryScheduled=true;AvailabilityChanged?.Invoke();
            AdManagerCore.RetryAfterBackoff(++retryAttempt,()=>{if(requestGeneration!=generation)return;retryScheduled=false;RequestAdInterstitial();});
        }
        public static bool ShowAdInterstitial()=>ShowAdInterstitial(null,null);
        public static bool ShowAdInterstitial(Action onClosed,Action onOpened)
        {
            if(!IsReady){RequestAdInterstitial();return false;}
            var ad=interstitial;interstitial=null;activeAd=ad;isAdmobInterstitialReady=false;
            int showGeneration=generation;bool completed=false;
            void Complete(){if(completed||showGeneration!=generation)return;completed=true;
                if(ReferenceEquals(activeAd,ad))activeAd=null;ad.Destroy();onClosed?.Invoke();InterstitialFinished?.Invoke();AvailabilityChanged?.Invoke();if(showGeneration==generation)RequestAdInterstitial();}
            try{
                ad.OnAdFullScreenContentOpened+=()=>{if(showGeneration==generation&&!completed)onOpened?.Invoke();};
                ad.OnAdFullScreenContentClosed+=Complete;
                ad.OnAdFullScreenContentFailed+=error=>{if(showGeneration!=generation)return;
                    AdManagerCore.ReportFailure("interstitial_show",error?.GetDomain()??"gma",error?.GetCode()??-1,error?.GetMessage());Complete();};
                ad.Show();return true;
            }catch(Exception e){AdManagerCore.ReportFailure("interstitial_show","gma",-1,e.Message);Complete();return false;}
        }
        public static void Reset()
        {
            generation++;retryScheduled=IsLoading=false;retryAttempt=0;isAdmobInterstitialReady=false;
            interstitial?.Destroy();interstitial=null;activeAd?.Destroy();activeAd=null;AvailabilityChanged?.Invoke();
        }
    }
}
#endif
