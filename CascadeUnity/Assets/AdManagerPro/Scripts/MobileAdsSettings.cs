using UnityEngine;

namespace AdManagerPro
{
    public class MobileAdsSettings : ScriptableObject
    {
        public bool testMode;
        public bool adaptiveBanner;

        [Header("Android")]
        public string androidAppID;
        public string androidBannerID;
        public string androidInterstitialID;
        public string androidRewardedVideoID;
        public string androidRewardedInterstitialID;
        public string androidNativeID;
        public string androidAppOpenID;

        [Header("iOS")]
        public string iosAppID;
        public string iosBannerID;
        public string iosInterstitialID;
        public string iosRewardedVideoID;
        public string iosRewardedInterstitialID;
        public string iosNativeID;
        public string iosAppOpenID;

        // Google demo ad unit IDs (https://developers.google.com/admob/unity/test-ads)
        public const string TestAppIDAndroid = "ca-app-pub-3940256099942544~3347511713";
        public const string TestAppIDIOS = "ca-app-pub-3940256099942544~1458002511";

        public string GetAppID()
        {
            if (testMode) return PlatformID(TestAppIDAndroid, TestAppIDIOS);
            return PlatformID(androidAppID, iosAppID);
        }

        public string GetBannerID()
        {
            if (testMode)
            {
                return adaptiveBanner
                    ? PlatformID("ca-app-pub-3940256099942544/9214589741", "ca-app-pub-3940256099942544/2435281174")
                    : PlatformID("ca-app-pub-3940256099942544/6300978111", "ca-app-pub-3940256099942544/2934735716");
            }
            return PlatformID(androidBannerID, iosBannerID);
        }

        public string GetInterstitialID()
        {
            if (testMode) return PlatformID("ca-app-pub-3940256099942544/1033173712", "ca-app-pub-3940256099942544/4411468910");
            return PlatformID(androidInterstitialID, iosInterstitialID);
        }

        public string GetRewardedVideoID()
        {
            if (testMode) return PlatformID("ca-app-pub-3940256099942544/5224354917", "ca-app-pub-3940256099942544/1712485313");
            return PlatformID(androidRewardedVideoID, iosRewardedVideoID);
        }

        public string GetRewardedInterstitialID()
        {
            if (testMode) return PlatformID("ca-app-pub-3940256099942544/5354046379", "ca-app-pub-3940256099942544/6978759866");
            return PlatformID(androidRewardedInterstitialID, iosRewardedInterstitialID);
        }

        public string GetNativeID()
        {
            if (testMode) return PlatformID("ca-app-pub-3940256099942544/2247696110", "ca-app-pub-3940256099942544/3986624511");
            return PlatformID(androidNativeID, iosNativeID);
        }

        public string GetAppOpenID()
        {
            if (testMode) return PlatformID("ca-app-pub-3940256099942544/9257395921", "ca-app-pub-3940256099942544/5575463023");
            return PlatformID(androidAppOpenID, iosAppOpenID);
        }

        private static string PlatformID(string android, string ios)
        {
#if UNITY_IOS
            return ios;
#else
            return android;
#endif
        }
    }
}
