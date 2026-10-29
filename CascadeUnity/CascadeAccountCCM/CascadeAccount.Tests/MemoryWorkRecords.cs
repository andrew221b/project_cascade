using CascadeAccountModule;
using Newtonsoft.Json.Linq;

public sealed partial class AtomicMemoryRecords:ICloudWorkRecords
{
    readonly object workGate=new();
    public int DeletedObjects;
    public async Task InitializeUnpublished(string collection,JToken value,CloudWork work)
    {
        await Task.Yield();work.Validate();
        lock(workGate) {
            string key=collection+"|value";
            if(data.TryGetValue(key,out var existing)&&!JToken.DeepEquals(existing.Value,value))throw new InvalidDataException("Unpublished collision");
            data.TryAdd(key,(value.DeepClone(),Guid.NewGuid().ToString("N")));
            data[collection+"|_work"]=(JObject.FromObject(work),Guid.NewGuid().ToString("N"));
        }
    }
    public async Task<CloudWorkRecord> ReadWork(string collection)
    {
        await Task.Yield();return data.TryGetValue(collection+"|_work",out var value)?new(collection,value.Value.ToObject<CloudWork>(),value.Lock):null;
    }
    public async Task<bool> UpdateWork(CloudWorkRecord prior,CloudWork work)
    {
        await Task.Yield();work.Validate();
        lock(workGate) {
            string key=prior.Collection+"|_work";
            if(!data.TryGetValue(key,out var existing)||existing.Lock!=prior.WriteLock)return false;
            if(work.Owner!=prior.Work.Owner||work.Kind!=prior.Work.Kind)throw new InvalidDataException("Changed scope");
            data[key]=(JObject.FromObject(work),Guid.NewGuid().ToString("N"));return true;
        }
    }
    public Task<IReadOnlyList<CloudWorkRecord>> Due(int lane,long now,int limit)
    {
        IReadOnlyList<CloudWorkRecord> result=Jobs().Where(x=>x.Work.Lane==lane&&x.Work.Due<=now).OrderBy(x=>x.Work.Due).Take(limit).ToArray();return Task.FromResult(result);
    }
    public Task<IReadOnlyList<CloudWorkRecord>> Owned(string owner,int limit)
    {
        IReadOnlyList<CloudWorkRecord> result=Jobs().Where(x=>x.Work.Owner==owner).Take(limit).ToArray();return Task.FromResult(result);
    }
    public Task DeleteWorkObject(string collection)
    {
        if(!new[]{"blob_","node_","delete_","job_"}.Any(k=>collection.StartsWith(CloudSaveAtomicStorage.Prefix+k)))throw new InvalidDataException("Protected directory");
        lock(workGate){foreach(string key in data.Keys.Where(k=>k.StartsWith(collection+"|")))data.TryRemove(key,out _);DeletedObjects++;}
        return Task.CompletedTask;
    }
    public CloudWorkRecord[] Jobs()=>data.Where(x=>x.Key.EndsWith("|_work")).Select(x=>new CloudWorkRecord(x.Key[..^6],x.Value.Value.ToObject<CloudWork>(),x.Value.Lock)).ToArray();
}
