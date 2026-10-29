using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Cascade.Application;
using Cascade.Persistence;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace CascadeAccountModule;

// Receipts select a publisher lookup, never a trusted verdict. Apple transaction
// data below comes from Apple's authenticated HTTPS API, not a client JWS payload.
public sealed class UnityDirectPublisher(HttpClient http)
{
    static readonly PublisherTokenCache tokens = new();
    static string Url64(byte[] value)=>Convert.ToBase64String(value).TrimEnd('=').Replace('+','-').Replace('/','_');
    static byte[] Un64(string value){value=value.Replace('-','+').Replace('_','/');return Convert.FromBase64String(value.PadRight((value.Length+3)/4*4,'='));}
    static JObject JwtData(string value)
    {
        if(value==null||value.Length>65536)throw new InvalidDataException("Invalid publisher transaction.");
        var parts=value.Split('.');if(parts.Length!=3)throw new InvalidDataException("Invalid publisher transaction.");
        return JObject.Parse(Encoding.UTF8.GetString(Un64(parts[1])));
    }
    static string Jwt(object header,object claims,Func<byte[],byte[]> sign)
    {
        string input=Url64(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header)))+"."+Url64(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(claims)));
        return input+"."+Url64(sign(Encoding.ASCII.GetBytes(input)));
    }
    static string AppleBearer(UnityPublisherConfiguration c)
    {
        using var key=ECDsa.Create();key.ImportFromPem(c.ApplePrivateKey);long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return Jwt(new{alg="ES256",kid=c.AppleKeyId,typ="JWT"},new{iss=c.AppleIssuerId,iat=now,exp=now+300,aud="appstoreconnect-v1",bid=c.BundleId},
            data=>key.SignData(data,HashAlgorithmName.SHA256,DSASignatureFormat.IeeeP1363FixedFieldConcatenation));
    }
    async Task<JObject> Json(HttpRequestMessage request,CancellationToken cancellation)
    {
        using(request){using var response=await http.SendAsync(request,HttpCompletionOption.ResponseHeadersRead,cancellation);
            if(!response.IsSuccessStatusCode)throw new HttpRequestException("Publisher request failed.",null,response.StatusCode);
            if(response.Content.Headers.ContentLength>262144)throw new InvalidDataException("Publisher response budget.");
            using var input=await response.Content.ReadAsStreamAsync(cancellation);using var output=new MemoryStream();var buffer=new byte[8192];int count;
            while((count=await input.ReadAsync(buffer,cancellation))>0){if(output.Length+count>262144)throw new InvalidDataException("Publisher response budget.");output.Write(buffer,0,count);}
            return JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(output.ToArray()),new JsonSerializerSettings{DateParseHandling=DateParseHandling.None});}
    }
    async Task<(JObject Data,string Environment)> Apple(UnityPublisherConfiguration c,string path,CancellationToken cancellation,string preferred=null)
    {
        string first=preferred is "Sandbox" or "Production"?preferred:c.AppleEnvironment;
        async Task<JObject> Fetch(string environment){
            string root=environment=="Sandbox"?"https://api.storekit-sandbox.itunes.apple.com":"https://api.storekit.itunes.apple.com";
            var request=new HttpRequestMessage(HttpMethod.Get,root+"/inApps/v1/"+path);
            string scope=PublisherTokenCache.Scope("apple|"+c.BundleId+"|"+c.AppleIssuerId+"|"+c.AppleKeyId+"|"+c.ApplePrivateKey);
            string token=await tokens.Get(scope,()=>Task.FromResult((AppleBearer(c),300L)),cancellation);
            request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);return await Json(request,cancellation);
        }
        try{return(await Fetch(first),first);}
        // An authenticated lookup in the other Apple environment is allowed only
        // for Not Found. Outages/authentication failures must remain retryable.
        catch(HttpRequestException error)when(error.StatusCode==System.Net.HttpStatusCode.NotFound){
            string second=first=="Sandbox"?"Production":"Sandbox";return(await Fetch(second),second);
        }
    }
    static string EnvironmentHint(string jws)
    {
        try{return JwtData(jws).Value<string>("environment");}
        catch(Exception error)when(error is InvalidDataException or FormatException or JsonException){return null;}
    }
    string GoogleScope(UnityPublisherConfiguration c)=>PublisherTokenCache.Scope("google|"+c.BundleId+"|"+c.GoogleCredentialsJson);
    Task<string> GoogleBearer(UnityPublisherConfiguration c,CancellationToken cancellation)
        =>tokens.Get(GoogleScope(c),async()=>{
            var credentials=JObject.Parse(c.GoogleCredentialsJson);long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();using var rsa=RSA.Create();rsa.ImportFromPem(credentials.Value<string>("private_key"));
            string assertion=Jwt(new{alg="RS256",typ="JWT"},new{iss=credentials.Value<string>("client_email"),scope="https://www.googleapis.com/auth/androidpublisher",aud="https://oauth2.googleapis.com/token",iat=now,exp=now+300},
                data=>rsa.SignData(data,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1));
            var request=new HttpRequestMessage(HttpMethod.Post,"https://oauth2.googleapis.com/token"){Content=new FormUrlEncodedContent(new Dictionary<string,string>{{"grant_type","urn:ietf:params:oauth:grant-type:jwt-bearer"},{"assertion",assertion}})};
            var response=await Json(request,cancellation);return(response.Value<string>("access_token"),response.Value<long?>("expires_in")??300);
        },cancellation);
    async Task<JObject> Google(UnityPublisherConfiguration c,string path,CancellationToken cancellation)
    {
        for(int retry=0;;retry++){
            string token=await GoogleBearer(c,cancellation);var request=new HttpRequestMessage(HttpMethod.Get,"https://androidpublisher.googleapis.com/androidpublisher/v3/applications/"+Uri.EscapeDataString(c.BundleId)+"/purchases/"+path);
            request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);
            try{return await Json(request,cancellation);}
            catch(HttpRequestException error)when(retry==0&&error.StatusCode==System.Net.HttpStatusCode.Unauthorized){await tokens.Invalidate(GoogleScope(c),cancellation,token);}
        }
    }
    static long UnixDate(string value)=>DateTimeOffset.TryParse(value,out var date)?Math.Max(0,date.ToUnixTimeSeconds()):0;
    async Task<long> AppleExpiry(UnityPublisherConfiguration c,string chain,string product,string binding,CancellationToken cancellation,string environment)
    {
        var lookedUp=await Apple(c,"subscriptions/"+Uri.EscapeDataString(chain),cancellation,environment);var response=lookedUp.Data;
        if(response.Value<string>("bundleId")!=c.BundleId)throw new InvalidDataException("Publisher bundle mismatch.");
        long expires=0;
        foreach(var item in (response["data"] as JArray??new JArray()).SelectMany(x=>x["lastTransactions"] as JArray??new JArray())){
            var tx=JwtData(item.Value<string>("signedTransactionInfo"));int status=item.Value<int>("status");
            if(tx.Value<string>("environment")!=lookedUp.Environment||tx.Value<string>("originalTransactionId")!=chain||tx.Value<string>("bundleId")!=c.BundleId||tx.Value<string>("productId")!=product
                ||binding!=null&&!string.Equals(tx.Value<string>("appAccountToken"),binding,StringComparison.OrdinalIgnoreCase)||tx["revocationDate"]?.Type is not (null or JTokenType.Null))continue;
            if(status is not (1 or 4))continue;long end=tx.Value<long?>("expiresDate")??0;
            if(status==4&&item.Value<string>("signedRenewalInfo") is string renewal)end=Math.Max(end,JwtData(renewal).Value<long?>("gracePeriodExpiresDate")??0);
            expires=Math.Max(expires,end/1000);
        }
        return Math.Max(0,expires);
    }
    public async Task<UnitySubscription> Refresh(UnityPublisherConfiguration c,string project,string environment,string player,UnitySubscription prior)
    {
        using var deadline=CancellationTokenSource.CreateLinkedTokenSource(ServerExecution.Token);deadline.CancelAfter(TimeSpan.FromSeconds(9));var cancellation=deadline.Token;var binding=PurchaseIdentity.For(project,environment,player);
        string product=prior.Product,observedEnvironment=prior.Environment;long expires=0;
        if(prior.Store=="AppleAppStore"){
            var lookedUp=await Apple(c,"subscriptions/"+Uri.EscapeDataString(prior.OriginalTransactionId),cancellation,prior.Environment);var state=lookedUp.Data;observedEnvironment=lookedUp.Environment;
            if(state.Value<string>("bundleId")!=c.BundleId)throw new InvalidDataException("Subscription bundle mismatch.");
            foreach(var item in (state["data"] as JArray??new JArray()).SelectMany(x=>x["lastTransactions"] as JArray??new JArray())){
                var tx=JwtData(item.Value<string>("signedTransactionInfo"));
                if(tx.Value<string>("environment")!=lookedUp.Environment||tx.Value<string>("originalTransactionId")!=prior.OriginalTransactionId||tx.Value<string>("bundleId")!=c.BundleId||!PurchaseCatalog.IsNoAdsSubscription(tx.Value<string>("productId"))||!string.Equals(tx.Value<string>("appAccountToken"),binding.AppleToken,StringComparison.OrdinalIgnoreCase))continue;
                long end=0;if(item.Value<int>("status") is 1 or 4&&tx["revocationDate"]?.Type is null or JTokenType.Null){end=tx.Value<long?>("expiresDate")??0;if(item.Value<int>("status")==4&&item.Value<string>("signedRenewalInfo") is string renewal)end=Math.Max(end,JwtData(renewal).Value<long?>("gracePeriodExpiresDate")??0);}
                if(end/1000>=expires){expires=end/1000;product=tx.Value<string>("productId");}
            }
        }else{
            var state=await Google(c,"subscriptionsv2/tokens/"+Uri.EscapeDataString(prior.Token),cancellation);
            if(state["externalAccountIdentifiers"]?.Value<string>("obfuscatedExternalAccountId")!=binding.GoogleAccountId)throw new InvalidDataException("Subscription binding mismatch.");
            if(state.Value<string>("subscriptionState") is "SUBSCRIPTION_STATE_ACTIVE" or "SUBSCRIPTION_STATE_IN_GRACE_PERIOD" or "SUBSCRIPTION_STATE_CANCELED")
                foreach(var line in state["lineItems"] as JArray??new JArray())if(PurchaseCatalog.IsNoAdsSubscription(line.Value<string>("productId"))&&UnixDate(line.Value<string>("expiryTime"))>=expires){product=line.Value<string>("productId");expires=UnixDate(line.Value<string>("expiryTime"));}
        }
        return new UnitySubscription{Product=product,Expires=Math.Max(0,expires),Environment=observedEnvironment};
    }
    public async Task<PurchaseVerifier.VerifiedPurchase> Verify(UnityPublisherConfiguration c,string project,string environment,string player,
        string product,string transaction,string platform,string receipt,string jws,string bundle,bool accountBound=true)
    {
        if(c.BundleId!=bundle||!PurchaseCatalog.IsKnown(product)||string.IsNullOrWhiteSpace(transaction)||transaction.Length>256||accountBound&&string.IsNullOrEmpty(player))return null;
        using var deadline=CancellationTokenSource.CreateLinkedTokenSource(ServerExecution.Token);deadline.CancelAfter(TimeSpan.FromSeconds(10));var cancellation=deadline.Token;
        var binding=accountBound?PurchaseIdentity.For(project,environment,player):null;bool subscription=PurchaseCatalog.IsNoAdsSubscription(product);
        if(platform=="AppleAppStore"){
            if(string.IsNullOrEmpty(jws))return null;
            var lookedUp=await Apple(c,"transactions/"+Uri.EscapeDataString(transaction),cancellation,EnvironmentHint(jws));var tx=JwtData(lookedUp.Data.Value<string>("signedTransactionInfo"));
            string type=subscription?"Auto-Renewable Subscription":PurchaseCatalog.Coins(product)>0?"Consumable":"Non-Consumable";
            if(tx.Value<string>("transactionId")!=transaction||tx.Value<string>("productId")!=product||tx.Value<string>("bundleId")!=bundle||tx.Value<string>("type")!=type
                ||tx.Value<string>("inAppOwnershipType")!="PURCHASED"||tx.Value<int?>("quantity")!=1
                ||tx.Value<string>("environment")!=lookedUp.Environment||binding!=null&&!string.Equals(tx.Value<string>("appAccountToken"),binding.AppleToken,StringComparison.OrdinalIgnoreCase)
                ||!subscription&&tx["revocationDate"]?.Type is not (null or JTokenType.Null))return null;
            string chain=tx.Value<string>("originalTransactionId");
            return new PurchaseVerifier.VerifiedPurchase{SubscriptionExpiresUnix=subscription?await AppleExpiry(c,chain,product,binding?.AppleToken,cancellation,lookedUp.Environment):0,
                SubscriptionChain=subscription?chain:null,OriginalTransactionId=subscription?chain:null,Environment=lookedUp.Environment};
        }
        if(platform!="GooglePlay"||receipt==null||receipt.Length>2*1024*1024)return null;
        var envelope=JObject.Parse(receipt);if(envelope.Value<string>("Store")!="GooglePlay")return null;
        var payload=JObject.Parse(envelope.Value<string>("Payload"));var submitted=JObject.Parse(payload.Value<string>("json"));string token=submitted.Value<string>("purchaseToken");
        if(submitted.Value<string>("packageName")!=bundle||submitted.Value<string>("productId")!=product||string.IsNullOrEmpty(token)||token.Length>16384)return null;
        string path=subscription?"subscriptionsv2/tokens/"+Uri.EscapeDataString(token):"products/"+Uri.EscapeDataString(product)+"/tokens/"+Uri.EscapeDataString(token);
        var purchase=await Google(c,path,cancellation);
        if(!subscription){
            if(purchase.Value<int?>("purchaseState")!=0||(purchase.Value<int?>("quantity")??1)!=1||purchase.Value<string>("orderId")!=transaction
                ||binding!=null&&purchase.Value<string>("obfuscatedExternalAccountId")!=binding.GoogleAccountId)return null;
            return new PurchaseVerifier.VerifiedPurchase();
        }
        var lines=(purchase["lineItems"] as JArray??new JArray()).Where(x=>x.Value<string>("productId")==product).ToArray();
        string order=purchase.Value<string>("latestOrderId")??lines.FirstOrDefault()?.Value<string>("latestSuccessfulOrderId");
        if(order!=transaction||lines.Length==0||binding!=null&&purchase["externalAccountIdentifiers"]?.Value<string>("obfuscatedExternalAccountId")!=binding.GoogleAccountId)return null;
        bool active=purchase.Value<string>("subscriptionState") is "SUBSCRIPTION_STATE_ACTIVE" or "SUBSCRIPTION_STATE_IN_GRACE_PERIOD" or "SUBSCRIPTION_STATE_CANCELED";
        long expiry=active?lines.Select(x=>UnixDate(x.Value<string>("expiryTime"))).DefaultIfEmpty(0).Max():0;
        return new PurchaseVerifier.VerifiedPurchase{SubscriptionExpiresUnix=expiry,SubscriptionChain=token,PurchaseToken=token,LinkedPurchaseToken=purchase.Value<string>("linkedPurchaseToken")};
    }
}
