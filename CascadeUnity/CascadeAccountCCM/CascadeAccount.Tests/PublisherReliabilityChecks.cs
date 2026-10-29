using System.Net;
using System.Security.Cryptography;
using System.Text;
using Cascade.Application;
using Cascade.Persistence;
using CascadeAccountModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class PublisherReliabilityChecks
{
    public static async Task Run()
    {
        int checks=0;void Check(bool pass,string label){if(!pass)throw new Exception(label);checks++;}
        long now=100;var cache=new PublisherTokenCache(()=>now);int issued=0;
        async Task<(string,long)> Issue(){Interlocked.Increment(ref issued);await Task.Delay(10);return("token-"+issued,300);}
        var concurrent=await Task.WhenAll(Enumerable.Range(0,32).Select(_=>cache.Get("same",Issue,default)));
        Check(issued==1&&concurrent.Distinct().Count()==1,"Concurrent read verifications share one completed publisher token");
        now+=241;Check(await cache.Get("same",Issue,default)=="token-2"&&issued==2,"Expired authorization is refreshed before use");
        await cache.Get("rotated",Issue,default);Check(issued==3,"A rotated credential cannot reuse the old token");
        await cache.Invalidate("same",default);await cache.Get("same",Issue,default);Check(issued==4,"A rejected authorization can be invalidated");
        int failures=0;try{await cache.Get("failure",()=>{failures++;throw new HttpRequestException();},default);}catch(HttpRequestException){}
        await cache.Get("failure",()=>{failures++;return Task.FromResult(("recovered",300L));},default);
        Check(failures==2,"Failed token exchange is never cached");

        var blocked=new TaskCompletionSource<(string,long)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var blockedGet=cache.Get("slow-credential",()=>blocked.Task,default);
        var unrelated=cache.Get("unrelated-credential",()=>Task.FromResult(("unrelated",300L)),default);
        Check(await Task.WhenAny(unrelated,Task.Delay(200))==unrelated,"A slow credential exchange cannot block an unrelated cached/provider authorization");
        blocked.SetResult(("old-token",300));await blockedGet;
        await cache.Invalidate("slow-credential",default,"old-token");
        await cache.Get("slow-credential",()=>Task.FromResult(("new-token",300L)),default);
        await cache.Invalidate("slow-credential",default,"old-token");
        Check(await cache.Get("slow-credential",()=>throw new Exception("Refreshed token was evicted"),default)=="new-token","A late rejection of an old token cannot evict its replacement");

        using var apple=ECDsa.Create(ECCurve.NamedCurves.nistP256);using var google=RSA.Create(2048);
        var config=new UnityPublisherConfiguration{BundleId="com.vickand.flyingladders",AppleEnvironment="Sandbox",AppleKeyId="test",AppleIssuerId="issuer",ApplePrivateKey=apple.ExportPkcs8PrivateKeyPem(),GoogleCredentialsJson=JsonConvert.SerializeObject(new{client_email="cache@example.com",private_key=google.ExportPkcs8PrivateKeyPem()})};
        var binding=PurchaseIdentity.For("project","production","alice");
        var tx=new JObject{{"transactionId","tx"},{"originalTransactionId","chain"},{"productId",PurchaseCatalog.Coins240},{"bundleId",config.BundleId},{"type","Consumable"},{"inAppOwnershipType","PURCHASED"},{"quantity",1},{"environment","Production"},{"appAccountToken",binding.AppleToken}};
        string Jws(JObject value)=>"header."+Convert.ToBase64String(Encoding.UTF8.GetBytes(value.ToString(Formatting.None))).TrimEnd('=').Replace('+','-').Replace('/','_')+".signature-from-authoritative-api";
        HttpResponseMessage Json(JObject value)=>new(HttpStatusCode.OK){Content=new StringContent(value.ToString(Formatting.None),Encoding.UTF8,"application/json")};
        var calls=new List<string>();
        using var transport=new ReliabilityHttp(request=>{calls.Add(request.RequestUri!.Host);return request.RequestUri.Host.Contains("sandbox")?new(HttpStatusCode.NotFound):Json(new JObject{{"signedTransactionInfo",Jws(tx)}});});
        var publisher=new UnityDirectPublisher(new HttpClient(transport));
        async Task<PurchaseVerifier.VerifiedPurchase> Apple(string evidence="untrusted-client-proof")=>await publisher.Verify(config,"project","production","alice",PurchaseCatalog.Coins240,"tx","AppleAppStore",null,evidence,config.BundleId);
        Check((await Apple())?.Environment=="Production"&&calls.Count==2,"Apple Not Found retries the other authoritative environment");
        calls.Clear();Check(await Apple(Jws(new JObject{{"environment","Production"}}))!=null&&calls.Count==1,"A routing hint saves a lookup without deciding purchase validity");
        tx["appAccountToken"]="foreign";Check(await Apple()==null,"Environment fallback never bypasses account binding");tx["appAccountToken"]=binding.AppleToken;
        tx["environment"]="Sandbox";Check(await Apple()==null,"An API environment mismatch cannot mint a grant");tx["environment"]="Production";
        calls.Clear();transport.Respond=request=>{calls.Add(request.RequestUri!.Host);return new(HttpStatusCode.Forbidden);};
        bool refused=false;try{await Apple();}catch(HttpRequestException error){refused=error.StatusCode==HttpStatusCode.Forbidden;}
        Check(refused&&calls.Count==1,"Authorization failures cannot masquerade as Apple environment selection");

        int exchanges=0,lookups=0;bool rejectOnce=true;
        transport.Respond=request=>{
            if(request.RequestUri!.Host=="oauth2.googleapis.com"){exchanges++;return Json(new JObject{{"access_token","token-"+exchanges},{"expires_in",3600}});}
            lookups++;
            if(rejectOnce){rejectOnce=false;return new(HttpStatusCode.Unauthorized);}
            return Json(new JObject{{"purchaseState",0},{"quantity",1},{"orderId","google-tx"},{"obfuscatedExternalAccountId",binding.GoogleAccountId}});
        };
        string receipt=JsonConvert.SerializeObject(new{Store="GooglePlay",Payload=JsonConvert.SerializeObject(new{json=JsonConvert.SerializeObject(new{packageName=config.BundleId,productId=PurchaseCatalog.Coins240,purchaseToken="purchase"})})});
        async Task<PurchaseVerifier.VerifiedPurchase> Google()=>await publisher.Verify(config,"project","production","alice",PurchaseCatalog.Coins240,"google-tx","GooglePlay",receipt,null,config.BundleId);
        Check(await Google()!=null&&exchanges==2&&lookups==2,"A cached/rejected Google credential refreshes once and rechecks the store");
        Check(await Google()!=null&&exchanges==2&&lookups==3,"The next receipt still contacts the store while reusing service authorization");
        transport.Respond=_=>new(HttpStatusCode.ServiceUnavailable);bool retryable=false;
        try{await Google();}catch(HttpRequestException error){retryable=error.StatusCode==HttpStatusCode.ServiceUnavailable;}
        Check(retryable,"Publisher outage is propagated, never accepted from cached receipt data");
        Console.WriteLine($"Publisher reliability checks: {checks} PASS; isolated transport, no live purchase.");
    }
}
public sealed class ReliabilityHttp(Func<HttpRequestMessage,HttpResponseMessage> respond):HttpMessageHandler
{
    public Func<HttpRequestMessage,HttpResponseMessage> Respond=respond;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellation){cancellation.ThrowIfCancellationRequested();return Task.FromResult(Respond(request));}
}
