using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace CascadeAccountModule;

public sealed class CloudWork
{
    public int Schema=1,Attempts;
    public string Kind,Owner,ParentCollection,ParentKey;
    public long Due,EligibleAfter,Version;
    public bool UniquePublication;
    public static long Never => 253402300799;
    public int Lane => Kind=="index" ? Convert.ToInt32(CloudSaveAtomicStorage.Hash(Owner)[..1],16) : Convert.ToInt32(Owner[..1],16);
    public CloudWork Copy() => JsonConvert.DeserializeObject<CloudWork>(JsonConvert.SerializeObject(this));
    public void Validate()
    {
        if(Schema!=1||Kind is not ("snapshot" or "index" or "capability" or "deletion")||Owner==null||Due<0||EligibleAfter<0||Version<0||Attempts<0)
            throw new InvalidDataException("Invalid maintenance metadata.");
        if(Kind=="index") {
            if(!Owner.StartsWith("index:",StringComparison.Ordinal)||Owner.Length>128||ParentCollection?.StartsWith(CloudSaveAtomicStorage.Prefix,StringComparison.Ordinal)!=true||ParentKey is not ("accounts" or "claims" or "value"))
                throw new InvalidDataException("Invalid index maintenance scope.");
        } else if(Owner.Length!=64||Owner.Any(c=>!Uri.IsHexDigit(c))) throw new InvalidDataException("Invalid maintenance owner.");
    }
}
public sealed record CloudWorkRecord(string Collection,CloudWork Work,string WriteLock);
public interface ICloudWorkRecords
{
    // Only uniquely named, unpublished objects use initialization. Any later
    // mutable metadata/index update requires a real existing write lock.
    Task InitializeUnpublished(string collection,JToken value,CloudWork work);
    Task<CloudWorkRecord> ReadWork(string collection);
    Task<bool> UpdateWork(CloudWorkRecord prior,CloudWork work);
    Task<IReadOnlyList<CloudWorkRecord>> Due(int lane,long now,int limit);
    Task<IReadOnlyList<CloudWorkRecord>> Owned(string owner,int limit);
    Task DeleteWorkObject(string collection);
}

public sealed partial class PrivateCloudRecords(IGameApiClient api,IExecutionContext context):IPrivateCloudRecords,ICloudWorkRecords
{
    static object Value(JToken value)=>value is JValue scalar?scalar.Value:value;
    static List<SetItemBody> Metadata(CloudWork work,string writeLock=null)
    {
        work.Validate();
        return new(){new("_work",JObject.FromObject(work),writeLock),new("fl_lane",work.Lane),new("fl_due",work.Due),new("fl_owner",work.Owner)};
    }
    public async Task InitializeUnpublished(string collection,JToken value,CloudWork work)
    {
        var found=await Read(collection,"value");
        if(found!=null) {
            if(!JToken.DeepEquals(found.Value,value)) throw new InvalidDataException("Unpublished object collision.");
            if(await ReadWork(collection)!=null) return;
        }
        var data=Metadata(work);data.Add(new SetItemBody("value",Value(value)));
        ServerExecution.Write();
        await api.CloudSaveData.SetPrivateCustomItemBatchAsync(context,context.ServiceToken,context.ProjectId,collection,new SetItemBatchBody(data),ServerExecution.Token);
    }
    public async Task<CloudWorkRecord> ReadWork(string collection)
    {
        var record=await Read(collection,"_work");
        if(record==null)return null;
        var work=record.Value.ToObject<CloudWork>();work.Validate();
        return new(collection,work,record.WriteLock);
    }
    public async Task<bool> UpdateWork(CloudWorkRecord prior,CloudWork work)
    {
        if(string.IsNullOrEmpty(prior.WriteLock))throw new InvalidOperationException("Existing work lock required.");
        if(work.Owner!=prior.Work.Owner||work.Kind!=prior.Work.Kind)throw new InvalidDataException("Maintenance scope cannot change.");
        try {
            ServerExecution.Write();
            await api.CloudSaveData.SetPrivateCustomItemBatchAsync(context,context.ServiceToken,context.ProjectId,prior.Collection,new SetItemBatchBody(Metadata(work,prior.WriteLock)),ServerExecution.Token);
            return true;
        } catch(ApiException error)when(error.Response?.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.NotFound){ServerExecution.Conflict();return false;}
    }
    async Task<IReadOnlyList<CloudWorkRecord>> Query(List<FieldFilter> filters,int limit)
    {
        ServerExecution.Read();
        var response=await api.CloudSaveData.QueryPrivateCustomDataAsync(context,context.ServiceToken,context.ProjectId,new QueryIndexBody(filters,new(){"_work"},0,Math.Clamp(limit,1,128)),ServerExecution.Token);
        return response.Data.Results.Select(row=>{
            var item=row.Data.Single(x=>x.Key=="_work");var work=JToken.FromObject(item.Value).ToObject<CloudWork>();work.Validate();
            if(!row.Id.StartsWith(CloudSaveAtomicStorage.Prefix,StringComparison.Ordinal))throw new InvalidDataException("Foreign maintenance collection.");
            return new CloudWorkRecord(row.Id,work,item.WriteLock);
        }).ToArray();
    }
    public Task<IReadOnlyList<CloudWorkRecord>> Due(int lane,long now,int limit)
    {
        if(lane is <0 or >15)throw new ArgumentException("Invalid maintenance lane.");
        return Query(new(){new("fl_lane",lane,FieldFilter.OpEnum.EQ,true),new("fl_due",now,FieldFilter.OpEnum.LE,true)},limit);
    }
    public Task<IReadOnlyList<CloudWorkRecord>> Owned(string owner,int limit)
        =>Query(new(){new("fl_owner",owner,FieldFilter.OpEnum.EQ,true)},limit);
    public async Task DeleteWorkObject(string collection)
    {
        if(!new[]{"blob_","node_","delete_","job_"}.Any(kind=>collection.StartsWith(CloudSaveAtomicStorage.Prefix+kind,StringComparison.Ordinal)))
            throw new InvalidDataException("Refusing to erase a directory or configuration collection.");
        try { ServerExecution.Write();await api.CloudSaveData.DeletePrivateCustomItemsAsync(context,context.ServiceToken,context.ProjectId,collection,ServerExecution.Token); }
        catch(ApiException error)when(error.Response?.StatusCode==HttpStatusCode.NotFound){}
    }
}
