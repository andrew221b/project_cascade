using UnityEngine;
namespace Cascade.Presentation
{
    [CreateAssetMenu(menuName="CASCADE/Cloud player settings")]
    public sealed class CloudPlayerSettings:ScriptableObject
    {
        [Tooltip("Enable only after deploying and verifying this project's Cloud Code module.")]
        public bool enabledForPlayers;
        public string projectId="a1b8a760-5a0c-4c2d-8ae7-54c1b411259a";
        public string environment="development",moduleName="CascadeAccount";
        [Tooltip("Enable after Unity Player Accounts client/provider/deep links are configured.")]
        public bool playerAccountsConfigured;
        public bool purchaseVerificationConfigured;
        public bool serverRewardsConfigured;
        public string accountDeletionUrl="";
    }
}
