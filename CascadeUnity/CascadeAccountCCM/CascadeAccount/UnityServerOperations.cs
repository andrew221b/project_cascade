using System.Net;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
namespace CascadeAccountModule;

// Public HTTP receivers are stateless adapters. All lookup, ownership, validation
// of rewarded callbacks, entitlements and deletion jobs execute inside Unity.
public sealed class UnityServerOperations(IGameApiClient api,ILogger<UnityServerOperations> logger=null,HttpClient injectedHttp=null,Func<long> clock=null)
{
    static readonly HttpClient sharedHttp=new(new HttpClientHandler{AllowAutoRedirect=false}){Timeout=TimeSpan.FromSeconds(10)};
    readonly HttpClient http=injectedHttp??sharedHttp;
    long Now=>clock?.Invoke()??DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    [CloudCodeFunction("UnityAdmobNotification")]
    public async Task<object> Admob(IExecutionContext context,string callbackUrl)
    {
        return await ServerExecution.Run<object>("UnityAdmobNotification",logger,async ()=>{
        UnityCloudMigration.RequireService(context);
        var data=await UnityAdmobSignature.Verify(callbackUrl,http);
        if(data.GetValueOrDefault("ad_unit")=="1234567890")return new{Verified=true,Test=true};
        if(await UnityBackendRouter.Mode(api,context)!="Unity")throw new InvalidOperationException("Unity migration in progress.");
        await new UnityOwnedAccountBackend(api,context).ConfirmReward(data);return new{Accepted=true};
    
        });
    }
    [CloudCodeFunction("UnitySubscriptionNotification")]
    public async Task<object> Subscription(IExecutionContext context,string chain)
    {
        return await ServerExecution.Run<object>("UnitySubscriptionNotification",logger,async ()=>{
        UnityCloudMigration.RequireService(context);
        if(await UnityBackendRouter.Mode(api,context)!="Unity")throw new InvalidOperationException("Unity migration in progress.");
        if(!Regex.IsMatch(chain??"",@"^chain\|(AppleAppStore|GooglePlay)\|[a-f0-9]{64}$"))throw new ArgumentException("Invalid subscription chain.");
        var storage=new CloudSaveAtomicStorage(new PrivateCloudRecords(api,context),context.ProjectId,context.EnvironmentId);
        var claim=await storage.Owner(chain);if(claim==null)return new{Ignored=true};
        var owner=await storage.FindOwner(claim.Owner);if(owner==null||owner.Deleted)return new{Ignored=true};
        await Refresh(context,owner.PlayerId,chain);return new{Accepted=true};
    
        });
    }
    [CloudCodeFunction("UnityVerifyLocalPurchase")]
    public async Task<object> VerifyLocal(IExecutionContext context,string productId,string transactionId,string receipt,string jws,string bundleId)
    {
        return await ServerExecution.Run<object>("UnityVerifyLocalPurchase",logger,async ()=>{
        UnityCloudMigration.RequireService(context);
        var proof=await new UnityDirectPublisher(http).Verify(await UnityPublisherVault.Load(api,context),context.ProjectId,context.EnvironmentId,null,productId,transactionId,string.IsNullOrEmpty(jws)?"GooglePlay":"AppleAppStore",receipt,jws,bundleId,false);
        return proof==null?new JObject{{"Valid",false}}:new JObject{{"Valid",true},{"CatalogVerified",true},{"ProductId",productId},{"TransactionId",transactionId},{"Platform",string.IsNullOrEmpty(jws)?"GooglePlay":"AppleAppStore"},{"BundleId",bundleId},{"SubscriptionExpiresUnix",proof.SubscriptionExpiresUnix}};
    
        });
    }
    [CloudCodeFunction("UnityRedeemDeletion")]
    public async Task<DeletionResult> Redeem(IExecutionContext context,string code,string confirmation)
    {
        return await ServerExecution.Run<DeletionResult>("UnityRedeemDeletion",logger,async ()=>{
        UnityCloudMigration.RequireService(context);if(confirmation!="DELETE"||!Regex.IsMatch(code??"",@"^[A-Za-z0-9_-]{32}$"))throw new ArgumentException("Invalid deletion capability.");
        var records=new PrivateCloudRecords(api,context);var value=await records.Read(CloudSaveAtomicStorage.ContentCollection("delete",CloudSaveAtomicStorage.Hash(code)),"value");
        var data=value==null?null:JObject.Parse(value.Value.Value<string>());if(data==null||data.Value<long>("ExpiresUnix")<Now)throw new UnauthorizedAccessException("Deletion capability expired.");
        string player=data.Value<string>("PlayerId");var storage=new CloudSaveAtomicStorage(records,context.ProjectId,context.EnvironmentId);await storage.Tombstone(player);
        try{await CleanupDeleted(context,storage.PlayerHash(player));}catch{/* The tombstone is the durable cleanup job. */}
        return new DeletionResult{Status="DeletionPending",RequestId=storage.PlayerHash(player)};
    
        });
    }
    [CloudCodeFunction("UnityContinueDeletion")]
    public async Task<object> ContinueDeletion(IExecutionContext context,string owner)
    {
        return await ServerExecution.Run<object>("UnityContinueDeletion",logger,async ()=>{
        UnityCloudMigration.RequireService(context);if(!Regex.IsMatch(owner??"",@"^[a-f0-9]{64}$"))throw new ArgumentException("Invalid deletion job.");
        await CleanupDeleted(context,owner);return new{Accepted=true};
    
        });
    }
    async Task Refresh(IExecutionContext context,string player,string chain)
    {
        var records=new PrivateCloudRecords(api,context);var storage=new CloudSaveAtomicStorage(records,context.ProjectId,context.EnvironmentId);
        var read=await storage.Read(player);var sub=read.Snapshot.Subscriptions.FirstOrDefault(x=>x.Chain==chain);if(sub==null)return;
        long generation=sub.Generation;var fresh=sub.SupersededBy!=null?new UnitySubscription{Product=sub.Product,Expires=0}:await new UnityDirectPublisher(http).Refresh(await UnityPublisherVault.Load(api,context),context.ProjectId,context.EnvironmentId,player,sub);
        await new UnityOwnedAccountBackend(api,context).Mutate(player,s=>{
            var current=s.Subscriptions.FirstOrDefault(x=>x.Chain==chain);if(current==null||current.Generation!=generation)throw new InvalidOperationException("Subscription changed. Retry safely.");
            current.Environment=fresh.Environment??current.Environment;current.Product=fresh.Product;current.Expires=fresh.Expires;current.Observed=Now;current.Generation++;
        });
    }
    static string Basic(UnityPublisherConfiguration config)=>"Basic "+Convert.ToBase64String(Encoding.UTF8.GetBytes(config.ServiceAccountKeyId+":"+config.ServiceAccountSecret));
    async Task Remove(string url,string authorization)
    {
        using var request=new HttpRequestMessage(HttpMethod.Delete,url);request.Headers.TryAddWithoutValidation("Authorization",authorization);
        using var response=await http.SendAsync(request,ServerExecution.Token);if(response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.NoContent or HttpStatusCode.NotFound))throw new InvalidOperationException("Unity data deletion pending.");
    }
    public async Task CleanupDeleted(IExecutionContext context,string owner)
    {
        var records=new PrivateCloudRecords(api,context);var storage=new CloudSaveAtomicStorage(records,context.ProjectId,context.EnvironmentId);
        if(string.IsNullOrEmpty(context.PlayerId))UnityCloudMigration.RequireService(context);
        else if(storage.PlayerHash(context.PlayerId)!=owner)throw new UnauthorizedAccessException("Account erasure scope mismatch.");
        var pointer=await storage.FindOwner(owner);if(pointer?.Deleted!=true||pointer.DeletionPlayerId==null)return;
        string player=pointer.DeletionPlayerId;
        if(storage.PlayerHash(player)!=owner)throw new InvalidDataException("Erasure identity scope mismatch.");
        if(!pointer.LegacyErased) {
            var config=await UnityPublisherVault.Load(api,context);string auth=Basic(config);
            if(string.IsNullOrEmpty(config.ServiceAccountKeyId)||string.IsNullOrEmpty(config.ServiceAccountSecret))throw new InvalidOperationException("Unity deletion credential missing.");
            string[] environments={"2cf87fca-ee3b-46cc-acc5-b5cbf2ad12d7","7e2d1cd6-b8fa-49f8-a689-cbb2bf4abee8"};
            string[] legacyIds=Array.Empty<string>();
            if(pointer.Blob!=null) {
                var blob=await records.Read(CloudSaveAtomicStorage.ContentCollection("blob",pointer.Blob),"value")??throw new InvalidDataException("Pending erasure snapshot missing.");
                var snapshot=JsonConvert.DeserializeObject<UnityAccountSnapshot>(blob.Value.Value<string>());
                var account=AccountJson.Decode<StoredAccount>(Cascade.Persistence.CloudProfilePayload.Unpack(snapshot.Payload,4*1024*1024));
                legacyIds=account.Account.Profile.Monetization.Grants.Where(g=>Regex.IsMatch(g.Id??"",@"^store_[a-f0-9]{64}$")).Select(g=>"iap_"+g.Id.Substring(6,46)).Distinct().OrderBy(x=>x,StringComparer.Ordinal).ToArray();
            }
            foreach(string environment in environments) {
                string root="https://services.api.unity.com/cloud-save/v1/data/projects/"+Uri.EscapeDataString(context.ProjectId)+"/environments/"+environment+"/players/"+Uri.EscapeDataString(player);
                await Task.WhenAll(new[]{"/items","/public/items","/protected/items","/private/items"}.Select(path=>Remove(root+path,auth)));
                foreach(string id in legacyIds.Skip(pointer.LegacyCleaned).Take(6)) {
                    string url="https://services.api.unity.com/cloud-save/v1/data/projects/"+context.ProjectId+"/environments/"+environment+"/custom/"+id+"/private/items";
                    using var request=new HttpRequestMessage(HttpMethod.Get,url+"?keys=owner");request.Headers.TryAddWithoutValidation("Authorization",auth);
                    using var response=await http.SendAsync(request,ServerExecution.Token);
                    if(response.StatusCode==HttpStatusCode.NotFound)continue;response.EnsureSuccessStatusCode();
                    var body=JObject.Parse(await response.Content.ReadAsStringAsync(ServerExecution.Token));
                    var item=(body["results"] as JArray)?.FirstOrDefault(x=>x.Value<string>("key")=="owner");
                    if(item?["value"]?.Value<string>("PlayerId")==player)await Remove(url+"/owner?writeLock="+Uri.EscapeDataString(item.Value<string>("writeLock")),auth);
                }
            }
            int completed=Math.Min(legacyIds.Length,pointer.LegacyCleaned+6);
            await storage.UpdatePointer(owner,p=>{p.LegacyCleaned=Math.Max(p.LegacyCleaned,completed);if(completed>=legacyIds.Length)p.LegacyErased=true;});
            if(completed<legacyIds.Length)return;
        }
        if(!pointer.IdentityErased) {
            var config=await UnityPublisherVault.Load(api,context);
            await Remove("https://services.api.unity.com/player-identity/v1/projects/"+Uri.EscapeDataString(context.ProjectId)+"/users/"+Uri.EscapeDataString(player),Basic(config));
            await storage.UpdatePointer(owner,p=>p.IdentityErased=true);
        }
        if(Now-pointer.DeletedAt<30)return;
        var work=(ICloudWorkRecords)records;
        // Drain the previous bounded queue for this deleted owner before its
        // metadata upgrade completes. No live owner/snapshot is eligible here.
        string legacyCollection=CloudSaveAtomicStorage.Prefix+"index_"+owner[..2];
        var legacyQueue=await records.Read(legacyCollection,"garbage");var legacyJobs=(JObject)legacyQueue.Value;
        var old=legacyJobs.Properties().Where(x=>x.Value.Value<string>("Owner")==owner).Take(32).ToArray();
        await Parallel.ForEachAsync(old,new ParallelOptions{MaxDegreeOfParallelism=8,CancellationToken=ServerExecution.Token},async(item,_)=>{
            bool capability=item.Value.Value<string>("Kind")=="delete";
            await work.DeleteWorkObject(CloudSaveAtomicStorage.ContentCollection(capability?"delete":"blob",capability?item.Name[7..]:item.Name));
        });
        foreach(var item in old)item.Remove();
        if(old.Length>0&&!await records.CompareExchange(legacyCollection,"garbage",legacyJobs,legacyQueue.WriteLock))throw new InvalidOperationException("Legacy cleanup changed. Retry safely.");
        if(legacyJobs.Properties().Any(x=>x.Value.Value<string>("Owner")==owner))return;
        var owned=await work.Owned(owner,100);
        var objects=owned.Where(x=>x.Collection!=pointer.DeletionJob).ToArray();
        if(objects.Any(x=>x.Work.Owner!=owner||x.Work.Kind=="index"))throw new InvalidDataException("Foreign erasure object.");
        await Parallel.ForEachAsync(objects,new ParallelOptions{MaxDegreeOfParallelism=8,CancellationToken=ServerExecution.Token},async(item,_)=>await work.DeleteWorkObject(item.Collection));
        if(objects.Length>0){await storage.UpdatePointer(owner,p=>p.ErasureEmptyObserved=0);return;}
        if(pointer.ErasureEmptyObserved==0){await storage.UpdatePointer(owner,p=>p.ErasureEmptyObserved=Now);return;}
        if(Now-pointer.ErasureEmptyObserved<30)return;
        await storage.UpdatePointer(owner,p=>{if(!p.Deleted)throw new InvalidDataException("Erasure state changed.");p.PlayerId=null;p.DeletionPlayerId=null;p.Blob=null;});
    }
    public async Task<MaintenanceResult> Maintain(IExecutionContext context,int lane=0)
    {
        UnityCloudMigration.RequireService(context);
        var records=new PrivateCloudRecords(api,context);string control=CloudSaveAtomicStorage.Prefix+"control";
        var ready=await records.Read(control,"workIndexesReady");
        if(ready?.Value.Value<bool>()!=true)return new MaintenanceResult{Status="Initializing"};
        var storage=new CloudSaveAtomicStorage(records,context.ProjectId,context.EnvironmentId);
        var processor=new CloudWorkProcessor(records,storage,(player,chain)=>Refresh(context,player,chain),owner=>CleanupDeleted(context,owner));
        var summary=await new CloudWorkPump(records).Run(lane,processor.Process,logger);
        await BackendOperations.Heartbeat(records,lane,summary);
        if(lane==0&&ServerExecution.Remaining.TotalSeconds>4){
            await new LegacyWorkUpgrade(records,storage,records).Step();
            var manifest=await records.Read(control,"migration");var value=(JObject)manifest.Value;
            if(Now-value.Value<long>("LastMaintenanceUnix")>=300){
                value["LastMaintenanceUnix"]=Now;value["MaintenanceVersion"]=2;
                await records.CompareExchange(control,"migration",value,manifest.WriteLock);
            }
        }
        return new MaintenanceResult{Status="Complete",Selected=summary.Selected,Completed=summary.Completed,Deferred=summary.Deferred,Failed=summary.Failed,OldestDueAge=summary.OldestDueAge};
    }
    [CloudCodeFunction("Maintenance01")]
    public Task<MaintenanceResult> Maintenance01(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance01",logger,()=>Maintain(context,1));
    [CloudCodeFunction("Maintenance02")]
    public Task<MaintenanceResult> Maintenance02(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance02",logger,()=>Maintain(context,2));
    [CloudCodeFunction("Maintenance03")]
    public Task<MaintenanceResult> Maintenance03(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance03",logger,()=>Maintain(context,3));
    [CloudCodeFunction("Maintenance04")]
    public Task<MaintenanceResult> Maintenance04(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance04",logger,()=>Maintain(context,4));
    [CloudCodeFunction("Maintenance05")]
    public Task<MaintenanceResult> Maintenance05(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance05",logger,()=>Maintain(context,5));
    [CloudCodeFunction("Maintenance06")]
    public Task<MaintenanceResult> Maintenance06(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance06",logger,()=>Maintain(context,6));
    [CloudCodeFunction("Maintenance07")]
    public Task<MaintenanceResult> Maintenance07(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance07",logger,()=>Maintain(context,7));
    [CloudCodeFunction("Maintenance08")]
    public Task<MaintenanceResult> Maintenance08(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance08",logger,()=>Maintain(context,8));
    [CloudCodeFunction("Maintenance09")]
    public Task<MaintenanceResult> Maintenance09(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance09",logger,()=>Maintain(context,9));
    [CloudCodeFunction("Maintenance10")]
    public Task<MaintenanceResult> Maintenance10(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance10",logger,()=>Maintain(context,10));
    [CloudCodeFunction("Maintenance11")]
    public Task<MaintenanceResult> Maintenance11(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance11",logger,()=>Maintain(context,11));
    [CloudCodeFunction("Maintenance12")]
    public Task<MaintenanceResult> Maintenance12(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance12",logger,()=>Maintain(context,12));
    [CloudCodeFunction("Maintenance13")]
    public Task<MaintenanceResult> Maintenance13(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance13",logger,()=>Maintain(context,13));
    [CloudCodeFunction("Maintenance14")]
    public Task<MaintenanceResult> Maintenance14(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance14",logger,()=>Maintain(context,14));
    [CloudCodeFunction("Maintenance15")]
    public Task<MaintenanceResult> Maintenance15(IExecutionContext context)=>ServerExecution.Run<MaintenanceResult>("Maintenance15",logger,()=>Maintain(context,15));

}
public static class UnityAdmobSignature
{
    static readonly System.Collections.Concurrent.ConcurrentDictionary<string,(string Pem,long At)> cache=new();
    static byte[] Un64(string value){value=value.Replace('-','+').Replace('_','/');return Convert.FromBase64String(value.PadRight((value.Length+3)/4*4,'='));}
    public static async Task<Dictionary<string,string>> Verify(string url,HttpClient http)
    {
        if(url==null||url.Length>16384||!Uri.TryCreate(url,UriKind.Absolute,out var uri))throw new ArgumentException("Invalid AdMob callback.");
        string raw=uri.Query.TrimStart('?');if(raw.Length>8192)throw new ArgumentException("Invalid AdMob callback.");
        var match=Regex.Match(raw,@"^(.+)&signature=([A-Za-z0-9_%-]+={0,2})&key_id=(\d+)$");if(!match.Success)throw new UnauthorizedAccessException("Invalid reward signature.");
        var fields=new Dictionary<string,string>(StringComparer.Ordinal);
        foreach(string part in raw.Split('&')){var split=part.Split('=',2);string key=Uri.UnescapeDataString(split[0].Replace('+',' ')),value=Uri.UnescapeDataString((split.Length>1?split[1]:"").Replace('+',' '));if(!fields.TryAdd(key,value))throw new ArgumentException("Duplicate reward field.");}
        string id=match.Groups[3].Value;long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if(!cache.TryGetValue(id,out var keyData)||now-keyData.At>3600){
            using var response=await http.GetAsync("https://www.gstatic.com/admob/reward/verifier-keys.json",ServerExecution.Token);response.EnsureSuccessStatusCode();
            if(response.Content.Headers.ContentLength>65536)throw new InvalidDataException("AdMob key budget.");
            var data=JObject.Parse(await response.Content.ReadAsStringAsync(ServerExecution.Token));foreach(var item in data["keys"] as JArray??new JArray())cache[item.Value<string>("keyId")]=(item.Value<string>("pem"),now);
            if(!cache.TryGetValue(id,out keyData))throw new UnauthorizedAccessException("Unknown reward signing key.");
        }
        using var publicKey=ECDsa.Create();publicKey.ImportFromPem(keyData.Pem);byte[] signature=Un64(Uri.UnescapeDataString(match.Groups[2].Value));
        string encoded=match.Groups[1].Value;
        // Google console sends form-encoded labels; served callbacks can use %20.
        // Both canonicalizations still require Google's real ECDSA signature.
        bool valid=new[]{Uri.UnescapeDataString(encoded),Uri.UnescapeDataString(encoded.Replace('+',' '))}.Distinct().Any(text=>publicKey.VerifyData(Encoding.UTF8.GetBytes(text),signature,HashAlgorithmName.SHA256,DSASignatureFormat.Rfc3279DerSequence));
        if(!valid)throw new UnauthorizedAccessException("Invalid reward signature.");return fields;
    }
}
