#if ADS
using UnityEngine;
using GoogleMobileAds.Api;
using System;

namespace AdManagerPro
{
    public static class AppOpenAdManager
    {
        private static AppOpenAd appOpenAd;
        public static bool isAdmobAppOpenReady = false;
        private static DateTime lastAdLoadTime;

        private static int retryAttempt;

        public static void LoadAppOpenAd()
        {
            AdManagerCore.EnsureInitialized(LoadAd);
        }

        private static void LoadAd()
        {
            if (appOpenAd != null)
            {
                appOpenAd.Destroy();
                appOpenAd = null;
            }

            AppOpenAd.Load(AdManagerCore.Settings.GetAppOpenID(), new AdRequest(), (AppOpenAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Failed to load app open ad: " + (error != null ? error.GetMessage() : "ad is null"));
                    RetryLoad();
                    return;
                }

                retryAttempt = 0;
                appOpenAd = ad;
                isAdmobAppOpenReady = true;
                lastAdLoadTime = DateTime.UtcNow;
                RegisterAppOpenEventHandlers(ad);
            });
        }

        private static void RetryLoad()
        {
            isAdmobAppOpenReady = false;
            retryAttempt++;
            AdManagerCore.RetryAfterBackoff(retryAttempt, LoadAd);
        }

        private static void RegisterAppOpenEventHandlers(AppOpenAd appOpenAd)
        {
            appOpenAd.OnAdFullScreenContentClosed += () =>
            {
                isAdmobAppOpenReady = false;
                LoadAppOpenAd();
            };

            appOpenAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                isAdmobAppOpenReady = false;
                LoadAppOpenAd();
            };
        }

        public static void ShowAppOpenAd()
        {
            if (IsAdAvailable())
            {
                appOpenAd.Show();
            }
            else
            {
                Debug.LogWarning("App open ad is not ready.");
                LoadAppOpenAd();
            }
        }

        private static bool IsAdAvailable()
        {
            return appOpenAd != null && isAdmobAppOpenReady && (DateTime.UtcNow - lastAdLoadTime).TotalHours < 4;
        }
    }
}
#endif
