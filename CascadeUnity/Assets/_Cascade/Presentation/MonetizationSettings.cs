using UnityEngine;
using UnityEngine.Serialization;

namespace Cascade.Presentation
{
    [CreateAssetMenu(menuName="CASCADE/Store and ad settings")]
    public sealed class MonetizationSettings:ScriptableObject
    {
        [Tooltip("Session-only showroom and mock ads. Disable for a production release.")]
        public bool testMonetization;
        [Tooltip("Show a labelled local ad simulation on device and in Editor while testMonetization is enabled.")]
        public bool localAdTestScreen;
        [Tooltip("Enable only for a store submission; rejects test and incomplete monetization setup.")]
        public bool productionRelease;
        [Tooltip("Enable only after registering the draft catalog in both stores.")]
        public bool storeCatalogRegistered;
        [Tooltip("Publisher-owned HTTPS endpoint that validates only Apple JWS / Google receipts and purchase identifiers for local under-16 profiles.")]
        public string receiptValidationUrl="";
        [FormerlySerializedAs("iosAppKey")] public string iosAdmobAppId="";
        [FormerlySerializedAs("androidAppKey")] public string androidAdmobAppId="";
        [FormerlySerializedAs("rewardedUnitId")] public string iosRewardedUnitId="";
        [FormerlySerializedAs("interstitialUnitId")] public string iosInterstitialUnitId="";
        public string androidRewardedUnitId="",androidInterstitialUnitId="";
        [Tooltip("Use Google's sample ads. Independent of the session-only test shop.")]
        public bool admobTestAds;
        public bool childDirected,underAgeOfConsent;
        public string AdUnit(bool ios,bool rewarded)
        {
            if(admobTestAds)return ios?rewarded?"ca-app-pub-3940256099942544/1712485313":"ca-app-pub-3940256099942544/4411468910":rewarded?"ca-app-pub-3940256099942544/5224354917":"ca-app-pub-3940256099942544/1033173712";
            return ios?rewarded?iosRewardedUnitId:iosInterstitialUnitId:rewarded?androidRewardedUnitId:androidInterstitialUnitId;
        }
        [Tooltip("Keep false until native dependency setup and the consent/age integration are validated.")]
        public bool adsReleaseReady;
    }
}
