using Newtonsoft.Json.Linq;

namespace CascadeAccountModule;

// A bounded, restartable upgrade of maintenance metadata, never a second game
// database. Existing account payloads/versions and immutable owners are untouched.
public sealed class LegacyWorkUpgrade(IPrivateCloudRecords records,CloudSaveAtomicStorage storage,ICloudWorkRecords work)
{
    public async Task Step()
    {
        string control=CloudSaveAtomicStorage.Prefix+"control";
        var manifest=await records.Read(control,"migration");var state=(JObject)manifest.Value;
        if(state.Value<bool?>("WorkUpgraded")==true)return;
        var rawRoute=await records.Read(control,"route");var readyAt=await records.Read(control,"directoryReadyAt");long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if(rawRoute?.Value.Value<string>()!="Unity2"||readyAt==null||readyAt.Value.Value<long>()>now)return;
        var active=(JObject)(await records.Read(control,"activeShards")).Value;
        var shards=active.Properties().Where(x=>x.Value.Value<bool>()).Select(x=>x.Name).OrderBy(x=>x,StringComparer.Ordinal).ToArray();
        int position=state.Value<int?>("WorkUpgradeShard")??0;string after=state.Value<string>("WorkUpgradeAfter");
        if(position>=shards.Length)state["WorkUpgraded"]=true;
        else {
            string shard=shards[position],collection=CloudSaveAtomicStorage.Prefix+"index_"+shard;
            var garbage=await records.Read(collection,"garbage");var jobs=(JObject)garbage.Value;
            foreach(var item in jobs.Properties().Take(8).ToArray()) {
                string owner=item.Value.Value<string>("Owner");bool capability=item.Value.Value<string>("Kind")=="delete";
                string target=CloudSaveAtomicStorage.ContentCollection(capability?"delete":"blob",capability?item.Name[7..]:item.Name);
                if(await work.ReadWork(target)==null) {
                    var data=await records.Read(target,"value");
                    if(data!=null) {
                        long eligible=Math.Max(now+30,item.Value.Value<long>("EligibleAfter"));
                        await work.InitializeUnpublished(target,data.Value,new CloudWork{Kind=capability?"capability":"snapshot",Owner=owner,Version=item.Value.Value<long>("Version"),Due=eligible,EligibleAfter=eligible,UniquePublication=true});
                    }
                }
                jobs.Remove(item.Name);
            }
            if(!await records.CompareExchange(collection,"garbage",jobs,garbage.WriteLock))return;
            var page=await storage.PointerPage(shard,after);
            foreach(var (owner,pointer) in page.Entries) {
                if(pointer.Deleted&&pointer.DeletionPlayerId!=null&&pointer.DeletionJob==null) {
                    string job=CloudSaveAtomicStorage.Prefix+"job_"+Guid.NewGuid().ToString("N");
                    await work.InitializeUnpublished(job,new JObject{{"Owner",owner}},new CloudWork{Kind="deletion",Owner=owner,Due=now+30,EligibleAfter=now+30,UniquePublication=true});
                    await storage.UpdatePointer(owner,p=>{if(p.Deleted&&p.DeletionJob==null)p.DeletionJob=job;});
                } else if(!pointer.Deleted&&pointer.Blob!=null) {
                    string target=CloudSaveAtomicStorage.ContentCollection("blob",pointer.Blob);
                    if(await work.ReadWork(target)==null) {
                        var data=await records.Read(target,"value");if(data==null)throw new InvalidDataException("Current legacy snapshot missing.");
                        await work.InitializeUnpublished(target,data.Value,new CloudWork{Kind="snapshot",Owner=owner,Version=pointer.Version,Due=now+30,EligibleAfter=now+30,UniquePublication=true});
                    }
                }
            }
            if(page.After==null&&jobs.Count==0){position++;after=null;}else after=page.After;
            state["WorkUpgradeShard"]=position;state["WorkUpgradeAfter"]=after;
            if(position>=shards.Length)state["WorkUpgraded"]=true;
        }
        await records.CompareExchange(control,"migration",state,manifest.WriteLock);
    }
}
