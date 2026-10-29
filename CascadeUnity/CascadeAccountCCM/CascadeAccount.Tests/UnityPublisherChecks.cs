using Cascade.Persistence;

using System.Net;
using System.Security.Cryptography;
using System.Text;
using Cascade.Application;
using CascadeAccountModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
public static class UnityPublisherChecks
{
    public static async Task Run()
    {
        int count=0;void Check(bool yes,string label){if(!yes)throw new Exception(label);count++;}
        string cipher=UnityPublisherVault.Seal("secret-payload","master","scope");Check(UnityPublisherVault.Open(cipher,"master","scope")=="secret-payload","Vault roundtrip");
        foreach(var pair in new[]{("wrong","scope"),("master","other")}){bool blocked=false;try{UnityPublisherVault.Open(cipher,pair.Item1,pair.Item2);}catch(CryptographicException){blocked=true;}Check(blocked,"Vault rejects wrong master and scope");}
        foreach(string kind in new[]{"blob","vault","delete"})Check(CloudSaveAtomicStorage.ContentCollection(kind,CloudSaveAtomicStorage.Hash("hello")).Length<=64,"Real Cloud Save custom IDs fit 64 characters");
        using var apple=ECDsa.Create(ECCurve.NamedCurves.nistP256);using var google=RSA.Create(2048);
        var config=new UnityPublisherConfiguration{BundleId="com.vickand.flyingladders",AppleEnvironment="Sandbox",ApplePrivateKey=apple.ExportPkcs8PrivateKeyPem(),AppleKeyId="test",AppleIssuerId="issuer",GoogleCredentialsJson=JsonConvert.SerializeObject(new{client_email="test@example.com",private_key=google.ExportPkcs8PrivateKeyPem()})};
        string project="project",env="production",player="player";var binding=PurchaseIdentity.For(project,env,player);
        var tx=new JObject{{"transactionId","paid-1"},{"originalTransactionId","chain-1"},{"productId",PurchaseCatalog.Coins240},{"bundleId",config.BundleId},{"type","Consumable"},{"inAppOwnershipType","PURCHASED"},{"quantity",1},{"environment","Sandbox"},{"appAccountToken",binding.AppleToken}};
        string Jws(JObject payload)=>"header."+Convert.ToBase64String(Encoding.UTF8.GetBytes(payload.ToString(Formatting.None))).TrimEnd('=').Replace('+','-').Replace('/','_')+".publisher-protected-by-https";
        var transport=new PublisherHttp(request=>request.RequestUri!.Host=="oauth2.googleapis.com"?new JObject{{"access_token","test"}}:new JObject{{"signedTransactionInfo",Jws(tx)}});
        var publisher=new UnityDirectPublisher(new HttpClient(transport));
        async Task<bool> Verify()=>await publisher.Verify(config,project,env,player,PurchaseCatalog.Coins240,"paid-1","AppleAppStore",null,"untrusted-client-proof",config.BundleId)!=null;
        Check(await Verify(),"Apple authoritative publisher response is accepted");
        foreach(string field in new[]{"transactionId","productId","bundleId","type","inAppOwnershipType","environment","appAccountToken"}){var prior=tx[field];tx[field]="wrong";Check(!await Verify(),"Apple rejects mismatched "+field);tx[field]=prior;}
        tx["quantity"]=2;Check(!await Verify(),"Apple rejects quantity change");tx["quantity"]=1;tx["revocationDate"]=100;Check(!await Verify(),"Apple rejects revoked consumable");tx.Remove("revocationDate");
        var purchase=new JObject{{"purchaseState",0},{"quantity",1},{"orderId","paid-google"},{"obfuscatedExternalAccountId",binding.GoogleAccountId}};
        transport.Response=request=>request.RequestUri!.Host=="oauth2.googleapis.com"?new JObject{{"access_token","test"}}:purchase;
        string Receipt(string package,string product)=>JsonConvert.SerializeObject(new{Store="GooglePlay",Payload=JsonConvert.SerializeObject(new{json=JsonConvert.SerializeObject(new{packageName=package,productId=product,purchaseToken="token"})})});
        async Task<bool> GoogleVerify()=>await publisher.Verify(config,project,env,player,PurchaseCatalog.Coins240,"paid-google","GooglePlay",Receipt(config.BundleId,PurchaseCatalog.Coins240),null,config.BundleId)!=null;
        Check(await GoogleVerify(),"Google publisher confirmed purchase accepted");
        purchase["purchaseState"]=2;Check(!await GoogleVerify(),"Pending Google purchase cannot grant currency");purchase["purchaseState"]=0;
        purchase["obfuscatedExternalAccountId"]="someoneelse";Check(!await GoogleVerify(),"Google cannot cross account owner");purchase["obfuscatedExternalAccountId"]=binding.GoogleAccountId;
        purchase["orderId"]="other";Check(!await GoogleVerify(),"Google transaction cannot change");purchase["orderId"]="paid-google";
        Check(await publisher.Verify(config,project,env,player,PurchaseCatalog.Coins240,"paid-google","GooglePlay",Receipt("other.bundle",PurchaseCatalog.Coins240),null,config.BundleId)==null,"Client package tampering fails");
        purchase=new JObject{{"externalAccountIdentifiers",new JObject{{"obfuscatedExternalAccountId",binding.GoogleAccountId}}},{"subscriptionState","SUBSCRIPTION_STATE_IN_GRACE_PERIOD"},{"lineItems",new JArray(new JObject{{"productId",PurchaseCatalog.NoAdsMonth},{"expiryTime",DateTimeOffset.UtcNow.AddDays(2).ToString("O")}})}};
        var sub=new UnitySubscription{Store="GooglePlay",Product=PurchaseCatalog.NoAdsMonth,Token="token"};
        Check((await publisher.Refresh(config,project,env,player,sub)).Expires>DateTimeOffset.UtcNow.ToUnixTimeSeconds(),"Grace period retains paid access");
        purchase["subscriptionState"]="SUBSCRIPTION_STATE_EXPIRED";Check((await publisher.Refresh(config,project,env,player,sub)).Expires==0,"Expired subscription removes access");
        purchase["subscriptionState"]="SUBSCRIPTION_STATE_ON_HOLD";Check((await publisher.Refresh(config,project,env,player,sub)).Expires==0,"On hold subscription removes access");
        using var signer=ECDsa.Create(ECCurve.NamedCurves.nistP256);string signed="ad_unit=1234567890&reward_amount=1&reward_item=Open Lootbox&timestamp=123&transaction_id=123456789";
        string signature=Convert.ToBase64String(signer.SignData(Encoding.UTF8.GetBytes(signed),HashAlgorithmName.SHA256,DSASignatureFormat.Rfc3279DerSequence)).TrimEnd('=').Replace('+','-').Replace('/','_');
        var keys=new HttpClient(new PublisherHttp(_=>new JObject{{"keys",new JArray(new JObject{{"keyId","12345"},{"pem",signer.ExportSubjectPublicKeyInfoPem()}})}}));
        string url="https://example.com/api/admob/ssv?"+signed.Replace(" ","+")+"&signature="+signature+"&key_id=12345";
        Check((await UnityAdmobSignature.Verify(url,keys))["reward_item"]=="Open Lootbox","Google form encoded console callback verifies");
        bool rejected=false;try{await UnityAdmobSignature.Verify(url.Replace("reward_amount=1","reward_amount=2"),keys);}catch(UnauthorizedAccessException){rejected=true;}Check(rejected,"Tampered rewarded callback cannot grant");
        rejected=false;try{await UnityAdmobSignature.Verify(url.Replace("&signature=","&reward_amount=1&signature="),keys);}catch(ArgumentException){rejected=true;}Check(rejected,"Duplicate rewarded fields cannot override signed fields");
        Console.WriteLine("Unity publisher/vault/signature checks: "+count+" PASS; isolated publisher transport, no live purchase claim.");
    }
}
public sealed class PublisherHttp(Func<HttpRequestMessage,JObject> response):HttpMessageHandler
{
    public Func<HttpRequestMessage,JObject> Response=response;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(Response(request).ToString(Formatting.None),Encoding.UTF8,"application/json")});
}
