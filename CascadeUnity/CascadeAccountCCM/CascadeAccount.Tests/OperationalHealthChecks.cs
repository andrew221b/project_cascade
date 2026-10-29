using CascadeAccountModule;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.Core;
public static class OperationalHealthChecks
{
 public static async Task Run(){
  int count=0;void Check(bool x,string label){if(!x)throw new Exception(label);count++;}
  var sdk=new WorkSdkTransport();sdk.Memory.Bootstrap();string control=CloudSaveAtomicStorage.Prefix+"control";long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
  await sdk.Memory.PutImmutable(control,"workIndexesReady","true");
  // The in-memory adapter stores actual JTokens for service metadata.
  var ready=await sdk.Memory.Read(control,"workIndexesReady");await sdk.Memory.CompareExchange(control,"workIndexesReady",new JValue(true),ready.WriteLock);
  await sdk.Memory.PutImmutable(control,"migration","{}");var manifest=await sdk.Memory.Read(control,"migration");await sdk.Memory.CompareExchange(control,"migration",new JObject{{"Status","Verified"},{"WorkUpgraded",true}},manifest.WriteLock);
  for(int i=0;i<16;i++){string key="maint_"+i.ToString("x2");await sdk.Memory.PutImmutable(control,key,"{}");var row=await sdk.Memory.Read(control,key);await sdk.Memory.CompareExchange(control,key,new JObject{{"At",now},{"Failed",0},{"OldestDueAge",5000}},row.WriteLock);}
  var operations=new BackendOperations(sdk.Client);
  async Task<JObject> Read()=>JObject.FromObject(await operations.Status(sdk.Context));
  Check((await Read()).Value<bool>("Healthy"),"Hourly maintenance tolerates a recovered backlog below two cycles instead of flapping after one delayed batch");
  var lane=await sdk.Memory.Read(control,"maint_00");await sdk.Memory.CompareExchange(control,"maint_00",new JObject{{"At",now-6000},{"Failed",0},{"OldestDueAge",0}},lane.WriteLock);
  Check(!(await Read()).Value<bool>("Healthy"),"A missed hourly heartbeat is unhealthy after ninety minutes");
  lane=await sdk.Memory.Read(control,"maint_00");await sdk.Memory.CompareExchange(control,"maint_00",new JObject{{"At",now},{"Failed",1},{"OldestDueAge",0}},lane.WriteLock);
  Check(!(await Read()).Value<bool>("Healthy"),"A failed independent maintenance job remains visible");
  lane=await sdk.Memory.Read(control,"maint_00");await sdk.Memory.CompareExchange(control,"maint_00",new JObject{{"At",now},{"Failed",0},{"OldestDueAge",8000}},lane.WriteLock);
  Check(!(await Read()).Value<bool>("Healthy"),"Backlog above two hourly cycles is unhealthy");
  var player=(IExecutionContext)Proxy.Make(typeof(IExecutionContext),(method,_)=>method.Name=="get_PlayerId"?"untrusted":method.Name=="get_ProjectId"?sdk.Project:method.Name=="get_EnvironmentId"?sdk.Environment:"token");
  bool rejected=false;try{await operations.Status(player);}catch(UnauthorizedAccessException){rejected=true;}Check(rejected,"Player tokens cannot read service diagnostics");
  Console.WriteLine($"Operational health checks: {count} PASS; actual SDK adapter with isolated metadata.");
 }
}
