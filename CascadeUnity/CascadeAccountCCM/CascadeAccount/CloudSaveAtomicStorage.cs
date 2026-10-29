using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace CascadeAccountModule;

public sealed record PrivateRecord(JToken Value,string WriteLock);
public interface IPrivateCloudRecords
{
    Task<PrivateRecord> Read(string collection,string key);
    Task PutImmutable(string collection,string key,string value);
    Task<bool> CompareExchange(string collection,string key,JToken value,string writeLock);
}

// SDK access always uses the server's authenticated context, never a client ID/token.
public sealed partial class PrivateCloudRecords
{
    public async Task<PrivateRecord> Read(string collection,string key)
    {
        try{
            ServerExecution.Read();
            var response=await api.CloudSaveData.GetPrivateCustomItemsAsync(context,context.ServiceToken,context.ProjectId,collection,new List<string>{key},cancellationToken:ServerExecution.Token);
            var item=response.Data.Results.FirstOrDefault(x=>x.Key==key);
            return item==null?null:new PrivateRecord(JToken.FromObject(item.Value),item.WriteLock);
        }catch(ApiException error)when(error.Response?.StatusCode==HttpStatusCode.NotFound){return null;}
    }
    public async Task PutImmutable(string collection,string key,string value)
    {
        // The caller supplies a content-addressed collection: simultaneous writes
        // to this name contain identical bytes. Mutable data never uses this path.
        var found=await Read(collection,key);
        if(found!=null){if(found.Value.Value<string>()!=value)throw new InvalidDataException("Immutable snapshot collision.");return;}
        ServerExecution.Write();await api.CloudSaveData.SetPrivateCustomItemAsync(context,context.ServiceToken,context.ProjectId,collection,new SetItemBody(key,value),ServerExecution.Token);
    }
    public async Task<bool> CompareExchange(string collection,string key,JToken value,string writeLock)
    {
        if(string.IsNullOrEmpty(writeLock))throw new InvalidOperationException("A real existing Cloud Save write lock is required.");
        try{ServerExecution.Write();await api.CloudSaveData.SetPrivateCustomItemAsync(context,context.ServiceToken,context.ProjectId,collection,new SetItemBody(key,value,writeLock),ServerExecution.Token);return true;}
        catch(ApiException error)when(error.Response?.StatusCode==HttpStatusCode.Conflict){ServerExecution.Conflict();return false;}
    }
}

public sealed class UnityAccountSnapshot
{
    public int Schema=1;
    public string PlayerId,Payload,PublicationId;
    public long StorageVersion;
    public AccountEffects Effects=new();
    public List<UnityRewardIntent> RewardIntents=new();
    public List<UnitySubscription> Subscriptions=new();
}
public sealed class UnityRewardIntent
{
    public string Id,Kind,Subject,Unit,Transaction;
    public long Created,Expires;
}
public sealed class UnitySubscription
{
    public string Chain,Store,Product,Token,OriginalTransactionId;
    public long Expires,Observed,Generation;
    public string SupersededBy,Environment;
}
public sealed class UnityAccountPointer
{
    public long Version,DeletedAt;
    public int LegacyCleaned;
    public string Blob,PlayerId;
    public bool Deleted,HasSubscriptions;
    public string DeletionPlayerId,DeletionJob;
    public long ErasureEmptyObserved;
    public bool LegacyErased,IdentityErased;
}
public sealed class UnityTransactionOwner
{
    public string Owner,Product,Bundle;
}
public sealed record UnityLoadedSnapshot(string PlayerHash,UnityAccountPointer Pointer,UnityAccountSnapshot Snapshot);

// Every mutable key below exists before activation. A single true Cloud Save CAS
// publishes an immutable snapshot, including the very first account write.
public sealed partial class CloudSaveAtomicStorage(IPrivateCloudRecords records,string projectId,string environmentId,int leafLimit=256,Func<long> clock=null)
{
    long NowUnix=>clock?.Invoke()??DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public const int Shards=256;
    public const string Prefix="fl_unity_v1_";
    const int MaxSnapshotBytes=3*1024*1024,MaxIndexBytes=1536*1024;
    public static string Hash(string value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    public string PlayerHash(string player)=>Hash(projectId+"|"+environmentId+"|"+player);
    static string Collection(string hash)=>Prefix+"index_"+hash[..2];
    public static string ContentCollection(string kind,string hash)=>Prefix+kind+"_"+Convert.ToBase64String(Convert.FromHexString(hash)).TrimEnd('=').Replace('+','-').Replace('/','_');
    static string BlobCollection(string blob)=>ContentCollection("blob",blob);
    static JObject Object(PrivateRecord record)=>record.Value as JObject??throw new InvalidDataException("Invalid Unity index.");
    static void Bound(JToken value,int limit){if(Encoding.UTF8.GetByteCount(value.ToString(Formatting.None))>limit)throw new InvalidDataException("Unity storage budget.");}
    public async Task<UnityLoadedSnapshot> Read(string player)
    {
        string owner=PlayerHash(player);var index=Object(await Index(owner,"accounts"));
        var pointer=index[owner]?.ToObject<UnityAccountPointer>()??new UnityAccountPointer();
        if(pointer.Deleted)throw new InvalidOperationException("AccountDeleted");
        if(pointer.Version>0&&pointer.PlayerId!=player)throw new InvalidDataException("Account pointer scope mismatch.");
        var snapshot=await ReadBlob(pointer,player);
        return new UnityLoadedSnapshot(owner,pointer,snapshot);
    }
    async Task<UnityAccountSnapshot> ReadBlob(UnityAccountPointer pointer,string player)
    {
        if(pointer.Version==0){if(pointer.Blob!=null)throw new InvalidDataException("Invalid empty account.");return new UnityAccountSnapshot{PlayerId=player};}
        if(pointer.Blob?.Length!=64)throw new InvalidDataException("Account snapshot missing.");
        var record=await records.Read(BlobCollection(pointer.Blob),"value");string json=record?.Value.Value<string>();
        if(json==null||Hash(json)!=pointer.Blob)throw new InvalidDataException("Account snapshot checksum mismatch.");
        var result=JsonConvert.DeserializeObject<UnityAccountSnapshot>(json);
        if(result?.Schema!=1||result.PlayerId!=player||result.StorageVersion!=pointer.Version||result.Effects==null||result.RewardIntents==null||result.Subscriptions==null)throw new InvalidDataException("Account snapshot scope mismatch.");
        return result;
    }
    public async Task<bool> Write(string player,long expectedVersion,UnityAccountSnapshot snapshot)
    {
        if(snapshot.PlayerId!=player||expectedVersion<0)throw new InvalidDataException("Invalid account write.");
        string owner=PlayerHash(player);await MarkActive(owner);var record=await Index(owner,"accounts");var index=Object(record);
        var pointer=index[owner]?.ToObject<UnityAccountPointer>()??new UnityAccountPointer();
        if(pointer.Deleted)throw new InvalidOperationException("AccountDeleted");
        if(pointer.Version!=expectedVersion)return false;
        snapshot.StorageVersion=checked(expectedVersion+1);snapshot.PublicationId=Guid.NewGuid().ToString("N");
        string json=JsonConvert.SerializeObject(snapshot,Formatting.None);if(Encoding.UTF8.GetByteCount(json)>MaxSnapshotBytes)throw new InvalidDataException("Account snapshot budget.");
        string blob=Hash(json);
        if(records is ICloudWorkRecords work) {
            long eligible=NowUnix+300;
            await work.InitializeUnpublished(BlobCollection(blob),new JValue(json),new CloudWork{Kind="snapshot",Owner=owner,Version=expectedVersion+1,Due=eligible,EligibleAfter=eligible,UniquePublication=true});
            if(pointer.Blob!=null)await RetireSnapshot(owner,pointer);
        } else {
            await QueueGarbage(owner,expectedVersion+1,blob);
            if(pointer.Blob!=null)await QueueGarbage(owner,expectedVersion,pointer.Blob);
            await records.PutImmutable(BlobCollection(blob),"value",json);
        }
        index[owner]=JObject.FromObject(new UnityAccountPointer{Version=checked(expectedVersion+1),Blob=blob,PlayerId=player,HasSubscriptions=snapshot.Subscriptions.Count>0});Bound(index,MaxIndexBytes);
        return await Publish(record,index);
    }
    async Task QueueGarbage(string owner,long version,params string[] blobs)
    {
        long eligible=NowUnix+300;
        for(int retry=0;retry<8;retry++){
            var record=await Index(owner,"garbage");var jobs=Object(record);
            foreach(string blob in blobs.Where(x=>!string.IsNullOrEmpty(x)))jobs[blob]=new JObject{{"Owner",owner},{"EligibleAfter",eligible},{"Version",version}};
            Bound(jobs,MaxIndexBytes);
            if(await Publish(record,jobs))return;
            await Task.Delay(Random.Shared.Next(8,36),ServerExecution.Token);
        }
        throw new InvalidOperationException("Unity cleanup queue is busy. Retry safely.");
    }
    public async Task QueueDeletionCode(string player,string hash,long expires)
    {
        string owner=PlayerHash(player);await MarkActive(owner);
        for(int retry=0;retry<8;retry++){
            var record=await Index(owner,"garbage");var jobs=Object(record);jobs["delete_"+hash]=new JObject{{"Owner",owner},{"EligibleAfter",expires+30},{"Kind","delete"},{"Version",0}};Bound(jobs,MaxIndexBytes);
            if(await Publish(record,jobs))return;
        }
        throw new InvalidOperationException("Deletion cleanup queue busy.");
    }
    async Task MarkActive(string owner)
    {
        string collection=Prefix+"control",shard=owner[..2];
        for(int retry=0;retry<8;retry++){
            var record=await records.Read(collection,"activeShards");if(record==null)throw new InvalidOperationException("Unity maintenance registry missing.");var active=Object(record);if(active[shard]?.Value<bool>()==true)return;
            active[shard]=true;if(await records.CompareExchange(collection,"activeShards",active,record.WriteLock))return;
        }
        throw new InvalidOperationException("Unity maintenance registry busy.");
    }
    public async Task<UnityAccountPointer> FindOwner(string owner)
    {
        if(owner?.Length!=64||owner.Any(c=>!Uri.IsHexDigit(c)))throw new ArgumentException("Invalid owner.");
        return Object(await Index(owner,"accounts"))[owner]?.ToObject<UnityAccountPointer>();
    }
    public async Task Import(string player,long version,UnityAccountSnapshot snapshot)
    {
        if(version<1||snapshot.PlayerId!=player)throw new InvalidDataException("Invalid imported account.");
        snapshot.StorageVersion=version;string owner=PlayerHash(player),json=JsonConvert.SerializeObject(snapshot,Formatting.None),blob=Hash(json);
        if(Encoding.UTF8.GetByteCount(json)>MaxSnapshotBytes)throw new InvalidDataException("Account snapshot budget.");
        for(int retry=0;retry<8;retry++){
            var record=await Index(owner,"accounts");var index=Object(record);var prior=index[owner]?.ToObject<UnityAccountPointer>();
            if(prior!=null){if(prior.Deleted||prior.Version!=version||prior.Blob!=blob)throw new InvalidOperationException("Imported account mismatch.");return;}
            await QueueGarbage(owner,version,blob);await records.PutImmutable(BlobCollection(blob),"value",json);
            index[owner]=JObject.FromObject(new UnityAccountPointer{Version=version,Blob=blob,PlayerId=player,HasSubscriptions=snapshot.Subscriptions.Count>0});Bound(index,MaxIndexBytes);
            if(await Publish(record,index))return;
        }
        throw new InvalidOperationException("Account import is busy. Retry safely.");
    }
    public async Task ImportOwner(string transaction,UnityTransactionOwner claim)
    {
        string key=Hash(transaction);
        for(int retry=0;retry<8;retry++){
            var record=await Index(key,"claims");var index=Object(record);var prior=index[key]?.ToObject<UnityTransactionOwner>();
            if(prior!=null){if(prior.Owner!=claim.Owner||prior.Product!=claim.Product||prior.Bundle!=claim.Bundle)throw new InvalidOperationException("Imported ownership mismatch.");return;}
            index[key]=JObject.FromObject(claim);Bound(index,MaxIndexBytes);
            if(await Publish(record,index))return;
        }
        throw new InvalidOperationException("Ownership import is busy. Retry safely.");
    }
    public async Task<bool> Claim(string player,string transaction,string product,string bundle)
    {
        var accountPointer=await FindOwner(PlayerHash(player));
        if(accountPointer?.Deleted==true)throw new InvalidOperationException("AccountDeleted"); // Never resurrect or transfer a deleted owner.
        string key=Hash(transaction),owner=PlayerHash(player);
        for(int retry=0;retry<8;retry++){
            var record=await Index(key,"claims");var index=Object(record);var found=index[key]?.ToObject<UnityTransactionOwner>();
            if(found!=null)return found.Owner==owner&&found.Product==product&&found.Bundle==bundle;
            index[key]=JObject.FromObject(new UnityTransactionOwner{Owner=owner,Product=product,Bundle=bundle});Bound(index,MaxIndexBytes);
            if(await Publish(record,index))return true;
            await Task.Delay(Random.Shared.Next(8,36),ServerExecution.Token);
        }
        throw new InvalidOperationException("Purchase ownership is busy; retry safely.");
    }
    public async Task<UnityTransactionOwner> Owner(string transaction)
    {
        string key=Hash(transaction);return Object(await Index(key,"claims"))[key]?.ToObject<UnityTransactionOwner>();
    }
    public async Task<bool> Tombstone(string player)
    {
        string owner=PlayerHash(player);await MarkActive(owner);
        for(int retry=0;retry<8;retry++){
            var record=await Index(owner,"accounts");var index=Object(record);var pointer=index[owner]?.ToObject<UnityAccountPointer>()??new UnityAccountPointer();
            if(pointer.Deleted)return true;
            if(records is ICloudWorkRecords work) {
                string job=Prefix+"job_"+Guid.NewGuid().ToString("N");long eligible=NowUnix+30;
                await work.InitializeUnpublished(job,new JObject{{"Owner",owner}},new CloudWork{Kind="deletion",Owner=owner,Due=eligible,EligibleAfter=eligible,UniquePublication=true});
                pointer.DeletionJob=job;
            } else if(pointer.Blob!=null)await QueueGarbage(owner,pointer.Version,pointer.Blob);
            pointer.Deleted=true;pointer.DeletedAt=NowUnix;pointer.DeletionPlayerId=player;pointer.Version=checked(pointer.Version+1);index[owner]=JObject.FromObject(pointer);
            if(await Publish(record,index))return true;
            await Task.Delay(Random.Shared.Next(8,36),ServerExecution.Token);
        }
        throw new InvalidOperationException("Account deletion is busy; retry safely.");
    }
}
