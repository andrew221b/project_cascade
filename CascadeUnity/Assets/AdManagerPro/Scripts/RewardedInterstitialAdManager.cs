#if ADS
using UnityEngine;
using GoogleMobileAds.Api;

namespace AdManagerPro
{
    public static class RewardedInterstitialAdManager
    {
        private static RewardedInterstitialAd rewardedInterstitialAd;
        public static bool isAdmobRewardedInterstitialReady = false;

        private static int retryAttempt;
        private static System.Action<bool> pendingCallback;
        private static bool rewardEarned;

        public static void RequestRewardedInterstitialAd()
        {
            AdManagerCore.EnsureInitialized(LoadAd);
        }

        private static void LoadAd()
        {
            if (rewardedInterstitialAd != null)
            {
                rewardedInterstitialAd.Destroy();
                rewardedInterstitialAd = null;
            }

            RewardedInterstitialAd.Load(AdManagerCore.Settings.GetRewardedInterstitialID(), new AdRequest(), (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Failed to load rewarded interstitial ad: " + (error != null ? error.GetMessage() : "ad is null"));
                    RetryLoad();
                    return;
                }

                retryAttempt = 0;
                rewardedInterstitialAd = ad;
                isAdmobRewardedInterstitialReady = true;
                RegisterRewardedInterstitialEventHandlers(ad);
            });
        }

        private static void RetryLoad()
        {
            isAdmobRewardedInterstitialReady = false;
            retryAttempt++;
            AdManagerCore.RetryAfterBackoff(retryAttempt, LoadAd);
        }

        private static void RegisterRewardedInterstitialEventHandlers(RewardedInterstitialAd rewardedInterstitialAd)
        {
            // The reward callback fires before the ad closes, so the result
            // is reported once, from the closed/failed handlers below.
            rewardedInterstitialAd.OnAdFullScreenContentClosed += () =>
            {
                isAdmobRewardedInterstitialReady = false;
                InvokePendingCallback(rewardEarned);
                RequestRewardedInterstitialAd();
            };

            rewardedInterstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                isAdmobRewardedInterstitialReady = false;
                InvokePendingCallback(false);
                RequestRewardedInterstitialAd();
            };
        }

        public static void ShowRewardedInterstitialAd(System.Action<bool> onAdWatched)
        {
            if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
            {
                pendingCallback = onAdWatched;
                rewardEarned = false;
                rewardedInterstitialAd.Show((Reward reward) =>
                {
                    rewardEarned = true;
                });
            }
            else
            {
                Debug.LogWarning("Rewarded interstitial ad is not ready.");
                onAdWatched?.Invoke(false);
                RequestRewardedInterstitialAd();
            }
        }

        private static void InvokePendingCallback(bool watched)
        {
            var callback = pendingCallback;
            pendingCallback = null;
            callback?.Invoke(watched);
        }
    }
}
#endif
