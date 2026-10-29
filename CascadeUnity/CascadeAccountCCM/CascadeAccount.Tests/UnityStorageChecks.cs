using System.Collections.Concurrent;
using CascadeAccountModule;
using Newtonsoft.Json.Linq;

public static class UnityStorageChecks
{
    public static async Task<int> Run()
    {
        int count=0;void Check(bool value,string message){if(!value)throw new Exception(message);count++;}
        var records=new AtomicMemoryRecords();var storage=new CloudSaveAtomicStorage(records,"project","production");
        bool blocked=false;try{await storage.Read("alice");}catch(InvalidOperationException){blocked=true;}Check(blocked,"Unbootstrapped storage must fail closed, never invent a write lock");
        records.Bootstrap();
        var first=await storage.Read("alice");Check(first.Pointer.Version==0&&first.Snapshot.Payload==null,"New player starts as a missing record");
        var starts=Enumerable.Range(0,48).Select(async i=>await storage.Write("alice",0,new UnityAccountSnapshot{PlayerId="alice",Payload="first-"+i})).ToArray();
        var results=await Task.WhenAll(starts);Check(results.Count(x=>x)==1,"Exactly one of 48 simultaneous first writes wins");
        var saved=await storage.Read("alice");Check(saved.Pointer.Version==1&&saved.Snapshot.Payload.StartsWith("first-"),"Winning first write is durable");
        var updates=Enumerable.Range(0,48).Select(async i=>await storage.Write("alice",saved.Pointer.Version,new UnityAccountSnapshot{PlayerId="alice",Payload="next-"+i})).ToArray();
        results=await Task.WhenAll(updates);Check(results.Count(x=>x)==1,"Exactly one stale-base update wins");
        Check((await storage.Read("alice")).Pointer.Version==2,"Concurrent losers cannot advance the account version");
        await storage.Write("bob",0,new UnityAccountSnapshot{PlayerId="bob",Payload="bob"});
        var claims=Enumerable.Range(0,64).Select(async i=>(Player:i%2==0?"alice":"bob",Won:await storage.Claim(i%2==0?"alice":"bob","GooglePlay|paid-1","coins_240","bundle"))).ToArray();
        var claimsDone=await Task.WhenAll(claims);var winner=claimsDone.First(x=>x.Won).Player;
        Check(claimsDone.Where(x=>x.Won).All(x=>x.Player==winner),"A transaction can belong to only one player across workers");
        Check(await storage.Claim(winner,"GooglePlay|paid-1","coins_240","bundle"),"The original owner can retry after a failed wallet commit");
        Check(!await storage.Claim(winner,"GooglePlay|paid-1","coins_480","bundle"),"The transaction cannot be reused for a different product");
        Check(!await storage.Claim(winner,"GooglePlay|paid-1","coins_240","other"),"The transaction cannot move to another bundle");
        var before=await storage.Read("alice");await storage.Tombstone("alice");
        blocked=false;try{await storage.Write("alice",before.Pointer.Version,before.Snapshot);}catch(InvalidOperationException){blocked=true;}Check(blocked,"A stale write cannot resurrect a deleted account");
        blocked=false;try{await storage.Read("alice");}catch(InvalidOperationException){blocked=true;}Check(blocked,"Deleted data cannot become a blank replacement account");
        Check(await storage.Tombstone("alice"),"Deletion is idempotent");
        var bob=await storage.Read("bob");records.CorruptBlob(bob.Pointer.Blob);
        blocked=false;try{await storage.Read("bob");}catch(InvalidDataException){blocked=true;}Check(blocked,"Corrupt content-addressed state cannot load as valid progress");
        Check(records.UnlockedMutableWrites==0,"No mutable record ever uses an empty write lock");
        Console.WriteLine("Unity atomic storage checks: "+count+" PASS; concurrent isolated fake Cloud Save, not live service verification.");return count;
    }
}
public sealed partial class AtomicMemoryRecords:IPrivateCloudRecords
{
    readonly ConcurrentDictionary<string,(JToken Value,string Lock)> data=new();
    public int UnlockedMutableWrites;
    public int RejectNextAccountPublication,RejectNextBranchPublication;
    public void Bootstrap(){data.TryAdd(CloudSaveAtomicStorage.Prefix+"control|route",(new JValue("Unity2"),Guid.NewGuid().ToString("N")));data.TryAdd(CloudSaveAtomicStorage.Prefix+"control|directoryReadyAt",(new JValue(0),Guid.NewGuid().ToString("N")));data.TryAdd(CloudSaveAtomicStorage.Prefix+"control|activeShards",(new JObject(),Guid.NewGuid().ToString("N")));for(int i=0;i<CloudSaveAtomicStorage.Shards;i++)foreach(var key in new[]{"accounts","claims","garbage"})data.TryAdd(CloudSaveAtomicStorage.Prefix+"index_"+i.ToString("x2")+"|"+key,(new JObject(),Guid.NewGuid().ToString("N")));}
    public async Task<PrivateRecord> Read(string collection,string key){await Task.Yield();return data.TryGetValue(collection+"|"+key,out var r)?new PrivateRecord(r.Value.DeepClone(),r.Lock):null;}
    public async Task PutImmutable(string collection,string key,string value){await Task.Yield();data.AddOrUpdate(collection+"|"+key,(_)=>(new JValue(value),Guid.NewGuid().ToString("N")),(_,r)=>r.Value.Value<string>()==value?r:throw new InvalidDataException("Immutable collision"));}
    public async Task<bool> CompareExchange(string collection,string key,JToken value,string writeLock){await Task.Yield();if(key=="accounts"&&Interlocked.Exchange(ref RejectNextAccountPublication,0)>0)return false;if(value is JObject branch&&branch["_flBranch"]!=null&&Interlocked.Exchange(ref RejectNextBranchPublication,0)>0)return false;if(string.IsNullOrEmpty(writeLock)){Interlocked.Increment(ref UnlockedMutableWrites);throw new InvalidOperationException("Unlocked");}string id=collection+"|"+key;if(!data.TryGetValue(id,out var old)||old.Lock!=writeLock)return false;return data.TryUpdate(id,(value.DeepClone(),Guid.NewGuid().ToString("N")),old);}
    public void CorruptBlob(string blob){string id=CloudSaveAtomicStorage.ContentCollection("blob",blob)+"|value";data[id]=(new JValue("tampered"),"different");}
}
