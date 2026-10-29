using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
namespace CascadeAccountModule;

public sealed class UnityBackendRouter:IAccountBackend
{
    public static async Task<string> Mode(IGameApiClient api,IExecutionContext context)
    {
        var raw=await ServerExecution.Stable("route|"+context.ProjectId+"|"+context.EnvironmentId,()=>new PrivateCloudRecords(api,context).Read(CloudSaveAtomicStorage.Prefix+"control","route"));
        string mode=raw==null?"Legacy":raw.Value.Value<string>()??throw new InvalidDataException("Backend route missing.");return mode=="Unity2"?"Unity":mode;
    }
    public async Task<T> Send<T>(IGameApiClient api,IExecutionContext context,string path,object data=null)
    {
        string mode=await Mode(api,context);
        if(mode!="Unity")throw new InvalidOperationException("Account migration in progress. Retry safely.");
        return await new UnityOwnedAccountBackend(api,context).Send<T>(path,data);
    }
}

// All player state and reward authority live in private Unity Cloud Save.
// The HTTP notification gateway never receives or writes these profile snapshots.
public sealed class UnityOwnedAccountBackend
{
    readonly IGameApiClient api;readonly IExecutionContext context;readonly CloudSaveAtomicStorage storage;
    public UnityOwnedAccountBackend(IGameApiClient api,IExecutionContext context)
    {
        this.api=api;this.context=context;
        storage=new(new PrivateCloudRecords(api,context),context.ProjectId,context.EnvironmentId);
    }
    static long Now=>DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    static string Nonce()=>Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)).TrimEnd('=').Replace('+','-').Replace('/','_');
    internal async Task<UnityAccountSnapshot> Mutate(string player,Action<UnityAccountSnapshot> mutation)
    {
        for(int retry=0;retry<8;retry++){
            var read=await storage.Read(player);if(read.Pointer.Version==0||read.Snapshot.Payload==null)throw new InvalidOperationException("Sync before requesting a reward.");
            mutation(read.Snapshot);
            if(await storage.Write(player,read.Pointer.Version,read.Snapshot))return read.Snapshot;
            await Task.Delay(Random.Shared.Next(8,36));
        }
        throw new InvalidOperationException("Account is busy. Retry safely.");
    }
    public async Task<T> Send<T>(string path,object data=null)
    {
        var body=JObject.FromObject(data??new{});string player=context.PlayerId;object result;
        if(string.IsNullOrEmpty(player)&&path!="maintenance")throw new InvalidOperationException("Player authentication required.");
        switch(path){
            case "account/read":{
                var read=await storage.Read(player);if(read.Pointer.Version==0)await GuardLegacyDeletion(player);read.Snapshot.Effects.Subscriptions=UnityCloudMigration.UnitySubscriptionEffects(read.Snapshot.Subscriptions);result=new AccountRecord{Version=read.Pointer.Version,Payload=read.Snapshot.Payload,Effects=read.Snapshot.Effects};break;
            }
            case "account/write":{
                long version=body.Value<long>("version");string payload=body.Value<string>("payload");
                if(payload==null||System.Text.Encoding.UTF8.GetByteCount(payload)>1536*1024)throw new InvalidDataException("Account budget.");
                var read=await storage.Read(player);
                bool written=false;if(read.Pointer.Version==version){read.Snapshot.Payload=payload;written=await storage.Write(player,version,read.Snapshot);}
                result=new WriteResult{Written=written,Version=version+1};break;
            }
            case "purchase/claim":{
                string platform=body.Value<string>("platform"),transaction=body.Value<string>("transactionId"),product=body.Value<string>("productId"),bundle=body.Value<string>("bundleId");
                if(platform is not ("AppleAppStore" or "GooglePlay")||string.IsNullOrEmpty(transaction)||transaction.Length>256)throw new ArgumentException("Invalid purchase transaction.");
                result=new ClaimResult{Claimed=await storage.Claim(player,"transaction|"+CloudSaveAtomicStorage.Hash(platform+"|"+transaction),product,bundle)};break;
            }
            case "account/effects":{
                var read=await storage.Read(player);read.Snapshot.Effects.Subscriptions=UnityCloudMigration.UnitySubscriptionEffects(read.Snapshot.Subscriptions);result=read.Snapshot.Effects;break;
            }
            case "reward/intent":{
                string kind=body.Value<string>("kind"),subject=body.Value<string>("subject")??"",unit=body.Value<string>("unit");
                if(kind is not ("coins" or "hint" or "parcel_open" or "parcel_bonus")||unit is not ("1665469215" or "8065944779"))throw new ArgumentException("Invalid reward offer.");
                if(kind is "coins" or "hint")subject="";
                else if(!System.Text.RegularExpressions.Regex.IsMatch(subject,@"^parcel:([1-9]\d{0,2})$"))throw new ArgumentException("Invalid parcel.");
                string id=null;long now=Now;string finalSubject=subject;
                await Mutate(player,s=>{
                    var outstanding=s.RewardIntents.FirstOrDefault(x=>x.Kind==kind&&x.Subject==finalSubject&&x.Unit==unit&&x.Transaction==null&&x.Expires>now);
                    if(outstanding!=null){id=outstanding.Id;return;}
                    s.RewardIntents.RemoveAll(x=>x.Expires<now-3*86400&&x.Transaction==null);
                    if(s.RewardIntents.Count>=10000)throw new InvalidOperationException("Reward history budget.");
                    id=Nonce();s.RewardIntents.Add(new UnityRewardIntent{Id=id,Kind=kind,Subject=finalSubject,Unit=unit,Created=now,Expires=now+900});
                });result=new RewardIntent{Id=id,UserId=player};break;
            }
            case "reward/status":{
                var read=await storage.Read(player);var intent=read.Snapshot.RewardIntents.FirstOrDefault(x=>x.Id==body.Value<string>("id"));
                result=new RewardStatus{ReceiptId=intent?.Transaction==null?null:"ssv_"+intent.Transaction};break;
            }
            case "account/deletion-code":{
                string code=Nonce(),hash=CloudSaveAtomicStorage.Hash(code);long expires=Now+600;
                // The high-entropy immutable code is a capability, not an account ID.
                string json=JsonConvert.SerializeObject(new{PlayerId=player,ExpiresUnix=expires});
                await storage.PutDeletionCapability(player,hash,json,expires);
                result=new DeletionResult{Code=code,ExpiresUnix=expires};break;
            }
            case "account/delete":{
                await storage.Tombstone(player);try{await new UnityServerOperations(api).CleanupDeleted(context,storage.PlayerHash(player));}catch{/* Durable tombstone retains the job for maintenance. */}result=new DeletionResult{Status="DeletionPending",RequestId=storage.PlayerHash(player)};break;
            }
            case "maintenance":result=await new UnityServerOperations(api).Maintain(context);break;
            default:throw new ArgumentException("Unknown Unity account operation.");
        }
        return JObject.FromObject(result).ToObject<T>();
    }
    async Task GuardLegacyDeletion(string player)
    {
        var read=await storage.FindOwner(storage.PlayerHash(player));if(read!=null)return;
        var records=new PrivateCloudRecords(api,context);var record=await records.Read(CloudSaveAtomicStorage.Prefix+"legacy_deleted","value");
        if(record==null)throw new InvalidOperationException("Legacy deletion migration missing.");
        var deleted=JsonConvert.DeserializeObject<string[]>(record.Value.Value<string>());
        if(deleted.Length==0)return;
        var config=await UnityPublisherVault.Load(api,context);
        using var hmac=new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(config.LegacyOwnerKey));
        string fingerprint=Convert.ToHexString(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("owner|"+player))).ToLowerInvariant();
        if(deleted.Contains(fingerprint,StringComparer.Ordinal)){await storage.Tombstone(player);throw new InvalidOperationException("AccountDeleted");}
    }
    public async Task RememberSubscription(string player,string platform,string product,PurchaseVerifier.VerifiedPurchase proof)
    {
        if(string.IsNullOrEmpty(proof.SubscriptionChain))throw new InvalidDataException("Subscription chain missing.");
        string chain="chain|"+platform+"|"+CloudSaveAtomicStorage.Hash(proof.SubscriptionChain);
        if(!await storage.Claim(player,chain,"subscription-chain",platform))throw new InvalidOperationException("Subscription ownership mismatch.");
        string linked=proof.LinkedPurchaseToken==null?null:"chain|GooglePlay|"+CloudSaveAtomicStorage.Hash(proof.LinkedPurchaseToken);
        if(linked!=null&&!await storage.Claim(player,linked,"subscription-chain",platform))throw new InvalidOperationException("Subscription ownership mismatch.");
        await Mutate(player,s=>{
            var sub=s.Subscriptions.FirstOrDefault(x=>x.Chain==chain);
            if(sub==null){if(s.Subscriptions.Count>=2000)throw new InvalidDataException("Subscription history budget.");sub=new UnitySubscription{Chain=chain,Store=platform};s.Subscriptions.Add(sub);}
            sub.Environment=proof.Environment;sub.Product=product;sub.Token=proof.PurchaseToken;sub.OriginalTransactionId=proof.OriginalTransactionId;sub.Expires=proof.SubscriptionExpiresUnix;sub.Observed=Now;sub.Generation++;
            if(linked!=null)foreach(var old in s.Subscriptions.Where(x=>x.Chain==linked)){old.SupersededBy=chain;old.Expires=0;old.Generation++;}
        });
    }
    public async Task ConfirmReward(IReadOnlyDictionary<string,string> notification)
    {
        string Value(string key)=>notification.TryGetValue(key,out var value)?value:null;
        string player=Value("user_id"),id=Value("custom_data"),unit=Value("ad_unit"),transaction=Value("transaction_id");
        if(string.IsNullOrEmpty(player)||player.Length>128||id?.Length!=32||string.IsNullOrEmpty(transaction)||transaction.Length>256
            ||Value("reward_amount")!="1"||!long.TryParse(Value("timestamp"),out long ms))throw new ArgumentException("Invalid reward notification.");
        var read=await storage.Read(player);var offer=read.Snapshot.RewardIntents.FirstOrDefault(x=>x.Id==id);
        long timestamp=ms/1000;
        if(offer==null||unit!=offer.Unit||timestamp<offer.Created-30||timestamp>offer.Expires||Now>offer.Expires+3*86400)throw new InvalidOperationException("Reward notification scope mismatch.");
        string receipt=CloudSaveAtomicStorage.Hash("admob|"+transaction);
        if(!await storage.Claim(player,"admob-transaction|"+receipt,id,unit)
            ||!await storage.Claim(player,"admob-intent|"+id,receipt,unit))throw new InvalidOperationException("Reward notification reused.");
        await Mutate(player,s=>{
            var intent=s.RewardIntents.Single(x=>x.Id==id);
            if(intent.Transaction!=null&&intent.Transaction!=receipt)throw new InvalidOperationException("Reward intent reused.");
            if(intent.Kind is "parcel_open" or "parcel_bonus"&&s.RewardIntents.Any(x=>x.Id!=id&&x.Kind==intent.Kind&&x.Subject==intent.Subject&&x.Transaction!=null))return;
            intent.Transaction=receipt;
            if(!s.Effects.Rewards.Any(x=>x.Id==receipt))s.Effects.Rewards.Add(new RewardEffect{Id=receipt,Kind=intent.Kind,Subject=intent.Subject});
        });
    }
}
