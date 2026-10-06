using UnityEngine;

namespace Cascade.Presentation
{
    [CreateAssetMenu(menuName="CASCADE/Store and ad settings")]
    public sealed class MonetizationSettings:ScriptableObject
    {
        [Tooltip("Session-only showroom and mock ads. Disable for a production release.")]
        public bool testMonetization;
        [Tooltip("Enable only after registering the draft catalog in both stores.")]
        public bool storeCatalogRegistered;
        [Tooltip("Publisher-owned HTTPS endpoint that validates Apple JWS / Google purchase receipts.")]
        public string receiptValidationUrl="";
        public string iosAppKey="",androidAppKey="",rewardedUnitId="",interstitialUnitId="";
        [Tooltip("Keep false until native dependency setup and the consent/age integration are validated.")]
        public bool adsReleaseReady;
    }
}
