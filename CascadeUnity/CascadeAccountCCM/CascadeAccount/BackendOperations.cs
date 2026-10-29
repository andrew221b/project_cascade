using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CascadeAccountModule;

public sealed class BackendOperations(IGameApiClient api,ILogger<BackendOperations> logger=null)
{
    public const string Version="20261029.1";
    [CloudCodeFunction("BackendStatus")]
    public Task<object> Status(IExecutionContext context)=>ServerExecution.Run<object>("BackendStatus",logger,async()=>{
        UnityCloudMigration.RequireService(context);
        var keys=new List<string>{"route","migration","workIndexesReady"};keys.AddRange(Enumerable.Range(0,16).Select(i=>"maint_"+i.ToString("x2")));
        ServerExecution.Read();
        var response=await api.CloudSaveData.GetPrivateCustomItemsAsync(context,context.ServiceToken,context.ProjectId,CloudSaveAtomicStorage.Prefix+"control",keys,cancellationToken:ServerExecution.Token);
        var values=response.Data.Results.ToDictionary(x=>x.Key,x=>JToken.FromObject(x.Value));
        string raw=values.GetValueOrDefault("route")?.Value<string>();
        bool ready=raw is "Unity" or "Unity2"&&values.GetValueOrDefault("migration")?.Value<string>("Status")=="Verified"&&values.GetValueOrDefault("workIndexesReady")?.Value<bool>()==true;
        long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();var lanes=Enumerable.Range(0,16).Select(i=>values.GetValueOrDefault("maint_"+i.ToString("x2")) as JObject).ToArray();
        bool fresh=lanes.All(x=>x!=null&&now-x.Value<long>("At")<5400);
        int failed=lanes.Sum(x=>x?.Value<int?>("Failed")??0);
        long oldest=lanes.Select(x=>x?.Value<long?>("OldestDueAge")??0).DefaultIfEmpty().Max();
        return new{Version,Ready=ready,Healthy=ready&&fresh&&failed==0&&oldest<7200,Storage="UnityCloudSave",Authority="UnityCloudCode",MaintenanceFresh=fresh,FailedJobs=failed,OldestDueSeconds=oldest,WorkUpgraded=values.GetValueOrDefault("migration")?.Value<bool?>("WorkUpgraded")==true};
    });
    internal static async Task Heartbeat(IPrivateCloudRecords records,int lane,CloudWorkSummary summary)
    {
        string collection=CloudSaveAtomicStorage.Prefix+"control",key="maint_"+lane.ToString("x2");
        var prior=await records.Read(collection,key);
        if(prior==null)throw new InvalidOperationException("Maintenance metrics are not bootstrapped.");
        long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();var value=(JObject)prior.Value;
        if(now-value.Value<long>("At")<300&&summary.Failed==0&&value.Value<int>("Failed")==0&&summary.Selected<100&&summary.OldestDueAge<300)return;
        var next=new JObject{{"At",now},{"Selected",summary.Selected},{"Completed",summary.Completed},{"Deferred",summary.Deferred},{"Failed",summary.Failed},{"OldestDueAge",summary.OldestDueAge}};
        await records.CompareExchange(collection,key,next,prior.WriteLock);
    }
}
