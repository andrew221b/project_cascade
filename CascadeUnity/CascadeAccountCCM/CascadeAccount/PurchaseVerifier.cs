using Newtonsoft.Json;
using Cascade.Application;
using System.Net.Http.Headers;
using System.Text;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CascadeAccountModule;

public sealed class PurchaseVerifier
{
    // One connection pool per worker. Client never supplies the verifier URL or credential.
    readonly HttpClient http;
    public PurchaseVerifier()
    {
        http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(11) };
    }
    public async Task<VerifiedPurchase> Verify(IGameApiClient api, IExecutionContext context, string productId, string transactionId,
        string platform, string receipt, string jws, string bundleId)
    {
        try {
            if(await UnityBackendRouter.Mode(api,context)!="Unity")return null;
            return await new UnityDirectPublisher(http).Verify(await UnityPublisherVault.Load(api,context),context.ProjectId,context.EnvironmentId,context.PlayerId,productId,transactionId,platform,receipt,jws,bundleId);
        } catch (Exception error) when (error is HttpRequestException or OperationCanceledException) {
            // Preserve a transient outage as a retryable operation, never a
            // permanent purchase verdict or an acknowledged ungranted order.
            throw new InvalidOperationException("Publisher verification temporarily unavailable. Retry safely.");
        } catch (Exception) { return null; } // Malformed or unverified evidence never mints a grant.
    }
    public sealed class VerifiedPurchase
    {
        public long SubscriptionExpiresUnix { get; set; }
        public string SubscriptionChain,OriginalTransactionId,PurchaseToken,LinkedPurchaseToken,Environment;
    }
    sealed class VerificationResult
    {
        public bool Valid { get; set; }
        public bool PlayerBindingVerified { get; set; }
        public bool CatalogVerified { get; set; }
        public string PlayerId { get; set; }
        public string ProjectId { get; set; }
        public string EnvironmentId { get; set; }
        public string ProductId { get; set; }
        public string TransactionId { get; set; }
        public string Platform { get; set; }
        public string BundleId { get; set; }
        public long SubscriptionExpiresUnix { get; set; }
    }
}
