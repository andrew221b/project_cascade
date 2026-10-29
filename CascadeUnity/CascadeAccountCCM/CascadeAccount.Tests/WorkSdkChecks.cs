using System.Net;
using System.Reflection;
using CascadeAccountModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

public static class WorkSdkChecks
{
    public static async Task Run()
    {
        int checks=0;void Check(bool value,string label){if(!value)throw new Exception(label);checks++;}
        var sdk=new WorkSdkTransport();sdk.Memory.Bootstrap();
        var records=new PrivateCloudRecords(sdk.Client,sdk.Context);
        var storage=new CloudSaveAtomicStorage(records,sdk.Project,sdk.Environment);
        Check(await storage.Write("alice",0,new UnityAccountSnapshot{PlayerId="alice",Payload="sdk-profile"}),"Actual SDK DTO adapter publishes a snapshot and its indexed work atomically");
        Check((await storage.Read("alice")).Snapshot.Payload=="sdk-profile","SDK data serialization preserves content-addressed snapshot bytes");
        string owner=storage.PlayerHash("alice");int lane=Convert.ToInt32(owner[..1],16);
        var jobs=await records.Due(lane,DateTimeOffset.UtcNow.ToUnixTimeSeconds()+301,100);
        Check(jobs.Count==1&&jobs[0].Work.Owner==owner,"Private compound query returns only due metadata with its real write lock");
        Check((await records.Due((lane+1)%16,DateTimeOffset.UtcNow.ToUnixTimeSeconds()+301,100)).Count==0,"The SDK query passes the requested maintenance lane");
        Check((await records.Owned(owner,100)).Count==1,"Owner erasure query selects the immutable owner fingerprint");
        var next=jobs[0].Work.Copy();next.Due=CloudWork.Never;
        Check(await records.UpdateWork(jobs[0],next),"Metadata and indexed scalar keys update in a guarded batch");
        Check(!await records.UpdateWork(jobs[0],next),"A stale batch write lock cannot cancel newer work");
        Check((await records.Due(lane,DateTimeOffset.UtcNow.ToUnixTimeSeconds()+301,100)).Count==0,"Updated due metadata leaves the pending query");
        bool denied=false;try{await records.DeleteWorkObject(CloudSaveAtomicStorage.Prefix+"control");}catch(InvalidDataException){denied=true;}Check(denied,"The SDK delete adapter rejects configuration collections before network I/O");
        int writes=sdk.BatchWrites;next.Owner=storage.PlayerHash("bob");denied=false;
        try{await records.UpdateWork(await records.ReadWork(jobs[0].Collection),next);}catch(InvalidDataException){denied=true;}
        Check(denied&&sdk.BatchWrites==writes,"Work cannot change owners through a metadata update");
        Check(sdk.LastBatchIndexedKeysMatched,"Indexed scalar metadata matches the guarded work document");
        Console.WriteLine($"Cloud Save work SDK checks: {checks} PASS; actual SDK DTOs, isolated transport.");
    }
}

public sealed class WorkSdkTransport
{
    public const string ProductionProject="a1b8a760-5a0c-4c2d-8ae7-54c1b411259a",ProductionEnvironment="2cf87fca-ee3b-46cc-acc5-b5cbf2ad12d7";
    public readonly string Project=ProductionProject,Environment=ProductionEnvironment;
    public readonly AtomicMemoryRecords Memory=new();
    public readonly IGameApiClient Client;
    public readonly IExecutionContext Context;
    public string Secret="fixture-master";
    public int BatchWrites;
    public bool LastBatchIndexedKeysMatched;
    public WorkSdkTransport()
    {
        Context=(IExecutionContext)Proxy.Make(typeof(IExecutionContext),(method,_)=>method.Name switch{"get_ProjectId"=>Project,"get_EnvironmentId"=>Environment,"get_ServiceToken"=>"trusted-service","get_PlayerId"=>null,_=>""});
        var apiType=typeof(IGameApiClient).GetProperty("CloudSaveData")!.PropertyType;
        var data=Proxy.Make(apiType,Invoke);
        Client=(IGameApiClient)Proxy.Make(typeof(IGameApiClient),(method,_)=>{
            if(method.Name=="get_CloudSaveData")return data;
            if(method.Name=="get_SecretManager")return Proxy.Make(method.ReturnType,(get,_)=>{
                Type result=get.ReturnType.GetGenericArguments()[0];
                object value=Activator.CreateInstance(result,new object[]{Secret})!;
                return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(result).Invoke(null,new[]{value})!;
            });
            throw new InvalidOperationException("Unconfigured SDK service");
        });
    }
    object Invoke(MethodInfo method,object[] args)
    {
        if(args.LastOrDefault() is CancellationToken cancellation)cancellation.ThrowIfCancellationRequested();
        if(method.Name=="GetPrivateCustomItemsAsync")return Get((string)args[3],(IEnumerable<string>)args[4]);
        if(method.Name=="SetPrivateCustomItemAsync")return Set((string)args[3],(SetItemBody)args[4]);
        if(method.Name=="SetPrivateCustomItemBatchAsync")return Batch((string)args[3],(SetItemBatchBody)args[4]);
        if(method.Name=="QueryPrivateCustomDataAsync")return Query((QueryIndexBody)args[3]);
        if(method.Name=="DeletePrivateCustomItemsAsync")return Delete((string)args[3]);
        throw new InvalidOperationException("Unsupported SDK operation: "+method.Name);
    }
    static Item Item(string key,JToken value,string writeLock)=>new(key,value,writeLock,new ModifiedMetadata(DateTime.UtcNow),new ModifiedMetadata(DateTime.UtcNow));
    async Task<ApiResponse<GetItemsResponse>> Get(string collection,IEnumerable<string> keys)
    {
        var items=new List<Item>();foreach(string key in keys){var value=await Memory.Read(collection,key);if(value!=null)items.Add(Item(key,value.Value,value.WriteLock));}
        return Reply(new GetItemsResponse(items,new GetItemsResponseLinks("")));
    }
    async Task<ApiResponse<SetItemResponse>> Set(string collection,SetItemBody body)
    {
        if(!await Memory.CompareExchange(collection,body.Key,JToken.FromObject(body.Value),body.WriteLock))throw Conflict();
        return Reply(new SetItemResponse((await Memory.Read(collection,body.Key))!.WriteLock));
    }
    async Task<ApiResponse<SetItemBatchResponse>> Batch(string collection,SetItemBatchBody body)
    {
        var metadata=body.Data.Single(x=>x.Key=="_work");var work=JToken.FromObject(metadata.Value).ToObject<CloudWork>();
        LastBatchIndexedKeysMatched=Convert.ToInt32(body.Data.Single(x=>x.Key=="fl_lane").Value)==work.Lane&&Convert.ToInt64(body.Data.Single(x=>x.Key=="fl_due").Value)==work.Due&&(string)body.Data.Single(x=>x.Key=="fl_owner").Value==work.Owner;
        if(!LastBatchIndexedKeysMatched)throw new Exception("Unmatched indexed metadata");BatchWrites++;
        if(body.Data.FirstOrDefault(x=>x.Key=="value") is { } value)await Memory.InitializeUnpublished(collection,JToken.FromObject(value.Value),work);
        else {
            var prior=await Memory.ReadWork(collection)??throw Conflict();
            if(prior.WriteLock!=metadata.WriteLock||!await Memory.UpdateWork(prior,work))throw Conflict();
        }
        string next=(await Memory.ReadWork(collection))!.WriteLock;
        return Reply(new SetItemBatchResponse(body.Data.Select(x=>new SetItemBatchResponseResultsInner(x.Key,next)).ToList()));
    }
    async Task<ApiResponse<QueryIndexResponse>> Query(QueryIndexBody body)
    {
        IReadOnlyList<CloudWorkRecord> jobs;
        if(body.Fields[0].Key=="fl_owner")jobs=await Memory.Owned((string)body.Fields[0].Value,body.Limit);
        else {
            if(body.Fields.Count!=2||body.Fields[0].Key!="fl_lane"||body.Fields[1].Key!="fl_due"||body.Fields[0].Op!=FieldFilter.OpEnum.EQ||body.Fields[1].Op!=FieldFilter.OpEnum.LE)throw new Exception("Wrong compound query");
            jobs=await Memory.Due(Convert.ToInt32(body.Fields[0].Value),Convert.ToInt64(body.Fields[1].Value),body.Limit);
        }
        if(body.ReturnKeys.Count!=1||body.ReturnKeys[0]!="_work")throw new Exception("Query must not transfer account payloads");
        return Reply(new QueryIndexResponse(jobs.Select(j=>new QueryIndexResponseResultsInner(j.Collection,new(){Item("_work",JObject.FromObject(j.Work),j.WriteLock)})).ToList()));
    }
    async Task<ApiResponse> Delete(string collection){await Memory.DeleteWorkObject(collection);return new ApiResponse();}
    static ApiResponse<T> Reply<T>(T data)
    {
        object response=new ApiResponse<T>();
        typeof(ApiResponse<T>).GetField("<StatusCode>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(response,HttpStatusCode.OK);
        typeof(ApiResponse<T>).GetField("<Data>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(response,data);return(ApiResponse<T>)response;
    }
    static ApiException Conflict()
    {
        object response=new ApiResponse();typeof(ApiResponse).GetField("<StatusCode>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(response,HttpStatusCode.Conflict);
        return new ApiException((ApiExceptionType)0,"CAS conflict",(IApiResponse)response);
    }
}
