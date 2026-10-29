using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudSave.Model;
namespace CascadeAccountModule;

// These endpoints are callable only with a trusted service context. A player JWT
// cannot install credentials, import state, change routing or run storage probes.
public sealed class UnityCloudMigration(IGameApiClient api,ILogger<UnityCloudMigration> logger=null)
{
    public static void RequireService(IExecutionContext context)
    {
        if(!string.IsNullOrEmpty(context.PlayerId)||string.IsNullOrEmpty(context.ServiceToken)
            ||context.ProjectId!="a1b8a760-5a0c-4c2d-8ae7-54c1b411259a"||context.EnvironmentId!="2cf87fca-ee3b-46cc-acc5-b5cbf2ad12d7")
            throw new UnauthorizedAccessException("Trusted service context required.");
    }
    public async Task<object> Bootstrap(IExecutionContext context,int firstShard,int count)
    {
        RequireService(context);if(firstShard<0||count<1||count>4||firstShard+count>256)throw new ArgumentException("Invalid bootstrap range.");
        if(await UnityBackendRouter.Mode(api,context)=="Unity")throw new InvalidOperationException("Storage bootstrap closed after activation.");
        var records=new PrivateCloudRecords(api,context);int created=0;
        async Task Ensure(string collection,string key,object value){
            if(await records.Read(collection,key)!=null)return;
            await api.CloudSaveData.SetPrivateCustomItemAsync(context,context.ServiceToken,context.ProjectId,collection,new SetItemBody(key,value));created++;
        }
        await Ensure(CloudSaveAtomicStorage.Prefix+"control","route","Legacy");
        await Ensure(CloudSaveAtomicStorage.Prefix+"control","vaultRef","");
        await Ensure(CloudSaveAtomicStorage.Prefix+"control","migration",new JObject());
        await Ensure(CloudSaveAtomicStorage.Prefix+"control","maintenanceCursor",0);
        await Ensure(CloudSaveAtomicStorage.Prefix+"control","activeShards",new JObject());
        for(int i=firstShard;i<firstShard+count;i++)foreach(string key in new[]{"accounts","claims","garbage"})
            await Ensure(CloudSaveAtomicStorage.Prefix+"index_"+i.ToString("x2"),key,new JObject());
        return new{First=firstShard,Count=count,Created=created};
    }
    public async Task<object> SetMode(IExecutionContext context,string mode)
    {
        RequireService(context);if(mode is not ("Legacy" or "Frozen" or "Unity"))throw new ArgumentException("Invalid backend mode.");
        var records=new PrivateCloudRecords(api,context);string collection=CloudSaveAtomicStorage.Prefix+"control";
        var route=await records.Read(collection,"route");if(route==null)throw new InvalidOperationException("Storage not bootstrapped.");
        string prior=route.Value.Value<string>();
        if(prior=="Unity"&&mode!="Unity")throw new InvalidOperationException("Old database has been retired. Route downgrade refused.");
        if(mode=="Unity"){
            var manifest=await records.Read(collection,"migration");if(manifest?.Value.Value<string>("Status")!="Verified")throw new InvalidOperationException("Migration is not verified.");
            await UnityPublisherVault.Load(api,context);
        }
        if(prior!=mode&&!await records.CompareExchange(collection,"route",new JValue(mode),route.WriteLock))throw new InvalidOperationException("Backend route changed. Retry safely.");
        return new{Mode=mode};
    }
    public async Task<object> Import(IExecutionContext context,string envelope)
    {
        RequireService(context);if(await UnityBackendRouter.Mode(api,context)!="Frozen")throw new InvalidOperationException("Freeze game writes before importing.");
        if(envelope?.Length>1900*1024)throw new InvalidDataException("Migration envelope budget.");
        var data=JObject.Parse(await UnityPublisherVault.OpenMigration(api,context,envelope));
        if(data.Value<int>("Schema")!=1||data.Value<string>("ProjectId")!=context.ProjectId||data.Value<string>("EnvironmentId")!=context.EnvironmentId)throw new InvalidDataException("Migration scope mismatch.");
        var config=data["Publisher"]?.ToObject<UnityPublisherConfiguration>()??throw new InvalidDataException("Publisher configuration missing.");
        await UnityPublisherVault.Install(api,context,config);
        var records=new PrivateCloudRecords(api,context);var storage=new CloudSaveAtomicStorage(records,context.ProjectId,context.EnvironmentId);
        var accounts=data["Accounts"] as JArray??throw new InvalidDataException("Accounts missing.");
        if(accounts.Count>128)throw new InvalidDataException("Import requires bounded batches.");
        var rewards=data["Rewards"] as JArray??new JArray();var subscriptions=data["Subscriptions"] as JArray??new JArray();
        var manifests=new JArray();var ownerMap=new Dictionary<string,string>();
        string LegacyOwner(string player){using var hmac=new HMACSHA256(Encoding.UTF8.GetBytes(config.LegacyOwnerKey));return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes("owner|"+player))).ToLowerInvariant();}
        foreach(var account in accounts){
            string player=account.Value<string>("player"),payload=account.Value<string>("payload");long version=account.Value<long>("version");
            if(string.IsNullOrEmpty(player)||player.Length>128||payload==null||version<1)throw new InvalidDataException("Invalid legacy account.");
            // Validate the existing profile without recanonicalizing or minting any grant.
            var state=AccountJson.Decode<StoredAccount>(Cascade.Persistence.CloudProfilePayload.Unpack(payload,4*1024*1024));state.Validate();
            var snapshot=new UnityAccountSnapshot{PlayerId=player,Payload=payload};ownerMap[LegacyOwner(player)]=storage.PlayerHash(player);
            foreach(var reward in rewards.Where(x=>x.Value<string>("player")==player)){
                var intent=new UnityRewardIntent{Id=reward.Value<string>("id"),Kind=reward.Value<string>("kind"),Subject=reward.Value<string>("subject"),Unit=reward.Value<string>("unit"),Created=reward.Value<long>("created"),Expires=reward.Value<long>("expires"),Transaction=reward.Value<int>("confirmed")==1?reward.Value<string>("transaction"):null};
                snapshot.RewardIntents.Add(intent);if(intent.Transaction!=null)snapshot.Effects.Rewards.Add(new RewardEffect{Id=intent.Transaction,Kind=intent.Kind,Subject=intent.Subject});
            }
            foreach(var subscription in subscriptions.Where(x=>x.Value<string>("player")==player)){
                var proof=JObject.Parse(UnsealLegacy(subscription.Value<string>("proof"),config.LegacyOwnerKey));
                snapshot.Subscriptions.Add(new UnitySubscription{Chain=subscription.Value<string>("chain"),Store=subscription.Value<string>("store"),Product=subscription.Value<string>("product"),Token=proof.Value<string>("token"),OriginalTransactionId=proof.Value<string>("originalTransactionId"),Expires=subscription.Value<long>("expires"),Observed=subscription.Value<long>("observed"),Generation=subscription.Value<long>("generation"),SupersededBy=subscription.Value<string>("superseded_by")});
            }
            snapshot.Effects.Subscriptions=UnitySubscriptionEffects(snapshot.Subscriptions);
            await storage.Import(player,version,snapshot);
            var read=await storage.Read(player);if(read.Pointer.Version!=version||read.Snapshot.Payload!=payload)throw new InvalidDataException("Imported account readback mismatch.");
            manifests.Add(new JObject{{"PlayerHash",storage.PlayerHash(player)},{"Version",version},{"PayloadHash",CloudSaveAtomicStorage.Hash(payload)}});
        }
        int owners=0;
        foreach(var owner in data["Owners"] as JArray??new JArray()){
            string oldOwner=owner.Value<string>("owner");
            await storage.ImportOwner(owner.Value<string>("key"),new UnityTransactionOwner{Owner=ownerMap.GetValueOrDefault(oldOwner,"legacy-deleted:"+oldOwner),Product=owner.Value<string>("product"),Bundle=owner.Value<string>("bundle")});owners++;
        }
        var deleted=(data["Tombstones"] as JArray??new JArray()).Select(x=>x.Value<string>("owner")).OrderBy(x=>x,StringComparer.Ordinal).ToArray();
        if(deleted.Any(x=>x?.Length!=64))throw new InvalidDataException("Invalid legacy deletion fingerprint.");
        await records.PutImmutable(CloudSaveAtomicStorage.Prefix+"legacy_deleted","value",JsonConvert.SerializeObject(deleted));
        foreach(var code in data["DeleteCodes"] as JArray??new JArray()){
            string hash=code.Value<string>("hash");if(hash?.Length!=64)throw new InvalidDataException("Invalid deletion capability.");
            await storage.QueueDeletionCode(code.Value<string>("player"),hash,code.Value<long>("expires"));
            await records.PutImmutable(CloudSaveAtomicStorage.ContentCollection("delete",hash),"value",JsonConvert.SerializeObject(new{PlayerId=code.Value<string>("player"),ExpiresUnix=code.Value<long>("expires")}));
        }
        foreach(var job in data["Deletions"] as JArray??new JArray())await storage.Tombstone(job.Value<string>("player"));
        var summary=new JObject{{"Status","Verified"},{"Accounts",accounts.Count},{"Owners",owners},{"Tombstones",deleted.Length},{"Rewards",rewards.Count},{"Subscriptions",subscriptions.Count},{"ManifestHash",CloudSaveAtomicStorage.Hash(manifests.ToString(Formatting.None))},{"Records",manifests}};
        var manifestRecord=await records.Read(CloudSaveAtomicStorage.Prefix+"control","migration");
        if(manifestRecord==null||!await records.CompareExchange(CloudSaveAtomicStorage.Prefix+"control","migration",summary,manifestRecord.WriteLock))throw new InvalidOperationException("Migration manifest changed. Retry safely.");
        return new{Status="Verified",Accounts=accounts.Count,Owners=owners,Tombstones=deleted.Length,Rewards=rewards.Count,Subscriptions=subscriptions.Count,ManifestHash=summary.Value<string>("ManifestHash")};
    }
    internal static List<SubscriptionEffect> UnitySubscriptionEffects(List<UnitySubscription> subscriptions)
        =>subscriptions.Count==0?new():Cascade.Application.PurchaseCatalog.NoAdsSubscriptions.Select(product=>new SubscriptionEffect{Product=product,Expires=subscriptions.Where(x=>x.Product==product&&x.SupersededBy==null).Select(x=>x.Expires).DefaultIfEmpty(0).Max()}).ToList();
    static byte[] Base64Url(string value){value=value.Replace('-','+').Replace('_','/');return Convert.FromBase64String(value.PadRight((value.Length+3)/4*4,'='));}
    static string UnsealLegacy(string value,string secret)
    {
        var buffer=Base64Url(value);if(buffer.Length<28)throw new InvalidDataException("Invalid legacy subscription proof.");var plain=new byte[buffer.Length-28];
        using var aes=new AesGcm(SHA256.HashData(Encoding.UTF8.GetBytes(secret)),16);aes.Decrypt(buffer.AsSpan(0,12),buffer.AsSpan(12,plain.Length),buffer.AsSpan(buffer.Length-16),plain);return Encoding.UTF8.GetString(plain);
    }
    public async Task<object> Handshake(IExecutionContext context,string challenge)
    {
        RequireService(context);if(challenge?.Length!=64)throw new ArgumentException("Invalid migration challenge.");
        string master=(await api.SecretManager.GetSecret(context,"CASCADE_RECEIPT_VERIFIER_TOKEN")).Value;
        using var hmac=new HMACSHA256(Encoding.UTF8.GetBytes(master));return new{Tag=Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes("unity-migration|"+challenge))).ToLowerInvariant()};
    }
    [CloudCodeFunction("UnityMigrationStatus")]
    public async Task<object> Status(IExecutionContext context)
    {
        return await ServerExecution.Run<object>("UnityMigrationStatus",logger,async ()=>{
        RequireService(context);var records=new PrivateCloudRecords(api,context);var migration=await records.Read(CloudSaveAtomicStorage.Prefix+"control","migration");
        return new{Mode=await UnityBackendRouter.Mode(api,context),Manifest=migration?.Value};
    
        });
    }
    public async Task<object> ProbeClaim(IExecutionContext context,string testId,string transaction)
    {
        RequireService(context);if(!System.Text.RegularExpressions.Regex.IsMatch(testId??"",@"^qa_unity_[a-f0-9]{32}$")||!System.Text.RegularExpressions.Regex.IsMatch(transaction??"",@"^qa_unity_[a-f0-9]{32}$"))throw new ArgumentException("Reserved disposable probe identity required.");
        return new{Claimed=await new CloudSaveAtomicStorage(new PrivateCloudRecords(api,context),context.ProjectId,context.EnvironmentId).Claim(testId,transaction,"qa","qa")};
    }
    public async Task<object> ProbeWrite(IExecutionContext context,string testId,long version,string payload)
    {
        RequireService(context);if(!System.Text.RegularExpressions.Regex.IsMatch(testId??"",@"^qa_unity_[a-f0-9]{32}$")||payload?.Length>128)throw new ArgumentException("Reserved disposable probe identity required.");
        var storage=new CloudSaveAtomicStorage(new PrivateCloudRecords(api,context),context.ProjectId,context.EnvironmentId);
        bool written=await storage.Write(testId,version,new UnityAccountSnapshot{PlayerId=testId,Payload=payload});return new{Written=written};
    }
    public async Task<object> ProbeRead(IExecutionContext context,string testId)
    {
        RequireService(context);if(!System.Text.RegularExpressions.Regex.IsMatch(testId??"",@"^qa_unity_[a-f0-9]{32}$"))throw new ArgumentException("Reserved disposable probe identity required.");
        var read=await new CloudSaveAtomicStorage(new PrivateCloudRecords(api,context),context.ProjectId,context.EnvironmentId).Read(testId);return new{Version=read.Pointer.Version,Payload=read.Snapshot.Payload};
    }
}
