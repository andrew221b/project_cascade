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
        http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(6) };
    }
    public async Task<VerifiedPurchase> Verify(IGameApiClient api, IExecutionContext context, string productId, string transactionId,
        string platform, string receipt, string jws, string bundleId)
    {
        try {
            string url = (await api.SecretManager.GetSecret(context, "CASCADE_RECEIPT_VERIFIER_URL")).Value;
            string token = (await api.SecretManager.GetSecret(context, "CASCADE_RECEIPT_VERIFIER_TOKEN")).Value;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo)
                || string.IsNullOrWhiteSpace(token)) return null;
            using var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonConvert.SerializeObject(new {
                playerId = context.PlayerId, projectId = context.ProjectId, environmentId = context.EnvironmentId,
                productId, transactionId, store = platform, receipt, jws, bundleId
            }), Encoding.UTF8, "application/json");
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(6));
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength > 32 * 1024) return null;
            using var source = await response.Content.ReadAsStreamAsync(deadline.Token);
            using var buffer = new MemoryStream();
            var chunk = new byte[4096];
            int count;
            while ((count = await source.ReadAsync(chunk.AsMemory(), deadline.Token)) > 0) {
                if (buffer.Length + count > 32 * 1024) return null;
                buffer.Write(chunk, 0, count);
            }
            var result = JsonConvert.DeserializeObject<VerificationResult>(Encoding.UTF8.GetString(buffer.ToArray()), AccountJson.Settings);
            bool valid = result != null && result.Valid && result.PlayerBindingVerified && result.CatalogVerified
                && result.PlayerId == context.PlayerId && result.ProjectId == context.ProjectId && result.EnvironmentId == context.EnvironmentId
                && result.ProductId == productId && result.TransactionId == transactionId && result.Platform == platform && result.BundleId == bundleId;
            return valid && (!PurchaseCatalog.IsNoAdsSubscription(productId) || result.SubscriptionExpiresUnix >= 0)
                ? new VerifiedPurchase { SubscriptionExpiresUnix = result.SubscriptionExpiresUnix } : null;
        } catch (Exception) { return null; } // Fail closed, without logging receipts, tokens or verifier responses.
    }
    public sealed class VerifiedPurchase { public long SubscriptionExpiresUnix { get; set; } }
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
