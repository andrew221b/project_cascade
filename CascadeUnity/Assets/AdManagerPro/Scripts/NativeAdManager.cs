#if NATIVE_ADS
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoogleMobileAds.Api;

namespace AdManagerPro
{
    public static class NativeAdManager
    {
        private static NativeAd nativeAd;
        public static bool isNativeAdReady = false;

        private static int retryAttempt;

        public static Image adImage;
        public static TMP_Text adHeadlineText;
        public static TMP_Text adBodyText;

        public static void RequestNativeAd()
        {
            AdManagerCore.EnsureInitialized(LoadAd);
        }

        private static void LoadAd()
        {
            AdLoader adLoader = new AdLoader.Builder(AdManagerCore.Settings.GetNativeID()).ForNativeAd().Build();
            adLoader.OnNativeAdLoaded += HandleNativeAdLoaded;
            adLoader.OnAdFailedToLoad += HandleNativeAdFailedToLoad;

            adLoader.LoadAd(new AdRequest());
        }

        private static void HandleNativeAdLoaded(object sender, NativeAdEventArgs e)
        {
            Debug.Log("Native ad loaded");
            retryAttempt = 0;
            nativeAd = e.nativeAd;
            isNativeAdReady = true;

            if (adImage != null)
            {
                Texture2D iconTexture = nativeAd.GetIconTexture();
                if (iconTexture != null)
                {
                    adImage.sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), Vector2.one * 0.5f);
                }
            }

            if (adHeadlineText != null)
            {
                adHeadlineText.text = nativeAd.GetHeadlineText();
            }

            if (adBodyText != null)
            {
                adBodyText.text = nativeAd.GetBodyText();
            }
        }

        private static void HandleNativeAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            Debug.LogError("Native ad failed to load: " + e.LoadAdError.GetMessage());
            isNativeAdReady = false;
            retryAttempt++;
            AdManagerCore.RetryAfterBackoff(retryAttempt, LoadAd);
        }

        public static void DestroyNativeAd()
        {
            if (nativeAd != null)
            {
                nativeAd.Destroy();
                nativeAd = null;
                isNativeAdReady = false;
            }
        }
    }
}
#endif
