#if ADS
using UnityEngine;
using GoogleMobileAds.Api;

namespace AdManagerPro
{
    public static class BannerAdManager
    {
        private static BannerView bannerView;
        public static bool isAdmobBannerReady = false;

        private static int retryAttempt;
        private static AdPosition lastPosition = AdPosition.Bottom;

        public static void RequestAdBanner(AdPosition adPosition = AdPosition.Bottom)
        {
            lastPosition = adPosition;
            AdManagerCore.EnsureInitialized(LoadAd);
        }

        private static void LoadAd()
        {
            if (bannerView != null)
            {
                bannerView.Destroy();
                bannerView = null;
                isAdmobBannerReady = false;
            }

            AdSize adSize;
            if (AdManagerCore.Settings.adaptiveBanner)
            {
                adSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            }
            else
            {
                adSize = AdSize.Banner;
            }

            bannerView = new BannerView(AdManagerCore.Settings.GetBannerID(), adSize, lastPosition);
            ListenToBannerAdEvents();
            bannerView.LoadAd(new AdRequest());
        }

        private static void ListenToBannerAdEvents()
        {
            bannerView.OnBannerAdLoaded += () =>
            {
                retryAttempt = 0;
                isAdmobBannerReady = true;
                ShowAdBanner();
            };

            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError("Failed to load banner ad: " + error.GetMessage());
                isAdmobBannerReady = false;
                retryAttempt++;
                AdManagerCore.RetryAfterBackoff(retryAttempt, LoadAd);
            };
        }

        public static void ShowAdBanner()
        {
            if (bannerView != null && isAdmobBannerReady)
            {
                bannerView.Show();
            }
            else
            {
                Debug.LogWarning("Banner ad is not ready.");
            }
        }

        public static void HideAdBanner()
        {
            if (bannerView != null)
            {
                bannerView.Hide();
            }
        }

        public static void DestroyAdBanner()
        {
            if (bannerView != null)
            {
                bannerView.Destroy();
                bannerView = null;
                isAdmobBannerReady = false;
            }
        }
    }
}
#endif
