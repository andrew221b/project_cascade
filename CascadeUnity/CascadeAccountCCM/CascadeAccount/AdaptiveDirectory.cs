using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CascadeAccountModule;

public sealed partial class CloudSaveAtomicStorage
{
    sealed record IndexSlot(string Collection,string Key,JObject Value,string WriteLock,int Depth);
    async Task<IndexSlot> Index(string hash,string key)
    {
        string collection=Collection(hash),item=key;int depth=2;
        for(int step=0;step<63;step++) {
            var record=await records.Read(collection,item);
            if(record==null||string.IsNullOrEmpty(record.WriteLock))throw new InvalidOperationException("Unity storage is not bootstrapped.");
            var value=Object(record);
            if(value["_flBranch"]==null)return new(collection,item,value,record.WriteLock,depth);
            if(value.Value<int>("_flBranch")!=2||value.Value<int>("Depth")!=depth||depth>=64||value["Children"] is not JArray children||children.Count!=16)
                throw new InvalidDataException("Invalid Unity directory branch.");
            collection=children[int.Parse(hash.AsSpan(depth,1),NumberStyles.HexNumber)]?.Value<string>();
            if(collection?.StartsWith(Prefix+"node_",StringComparison.Ordinal)!=true)throw new InvalidDataException("Invalid Unity directory scope.");
            item="value";depth++;
        }
        throw new InvalidDataException("Unity directory depth budget.");
    }
    static JObject Object(IndexSlot slot)=>slot.Value;
    async Task<bool> Publish(IndexSlot slot,JObject value)
    {
        Bound(value,MaxIndexBytes);
        bool large=value.Count>Math.Max(4,leafLimit)||System.Text.Encoding.UTF8.GetByteCount(value.ToString(Formatting.None))>128*1024;
        if(large&&slot.Key!="garbage"&&slot.Depth<63&&records is ICloudWorkRecords work&&await DirectoryV2Ready()) {
            // Each contender prepares a different generation. Children remain
            // unpublished until one real parent CAS wins. Late/failed contenders
            // can only leave disposable, indexed orphan objects, never overwrite
            // a live child initialized by another request.
            string generation=Guid.NewGuid().ToString("N");
            var names=Enumerable.Range(0,16).Select(i=>Prefix+"node_"+generation+"_"+i.ToString("x")).ToArray();
            var values=Enumerable.Range(0,16).Select(_=>new JObject()).ToArray();
            foreach(var entry in value.Properties()) {
                if(entry.Name.Length!=64||entry.Name.Any(c=>!Uri.IsHexDigit(c)))throw new InvalidDataException("Invalid directory entry.");
                values[int.Parse(entry.Name.AsSpan(slot.Depth,1),NumberStyles.HexNumber)][entry.Name]=entry.Value.DeepClone();
            }
            long due=NowUnix+300;
            await Parallel.ForEachAsync(Enumerable.Range(0,16),new ParallelOptions{MaxDegreeOfParallelism=4,CancellationToken=ServerExecution.Token},async(i,_)=>
                await work.InitializeUnpublished(names[i],values[i],new CloudWork{Kind="index",Owner="index:"+slot.Collection+"|"+slot.Key,ParentCollection=slot.Collection,ParentKey=slot.Key,Due=due,EligibleAfter=due,UniquePublication=true}));
            var branch=new JObject{{"_flBranch",2},{"Depth",slot.Depth},{"Children",new JArray(names)}};
            return await records.CompareExchange(slot.Collection,slot.Key,branch,slot.WriteLock);
        }
        return await records.CompareExchange(slot.Collection,slot.Key,value,slot.WriteLock);
    }
    async Task<bool> DirectoryV2Ready()
    {
        var route=await ServerExecution.Stable("route|"+projectId+"|"+environmentId,()=>records.Read(Prefix+"control","route"));
        if(route?.Value.Value<string>()!="Unity2")return false;
        var ready=await ServerExecution.Stable("directory-ready",()=>records.Read(Prefix+"control","directoryReadyAt"));
        return ready!=null&&ready.Value.Value<long>()<=NowUnix;
    }
    public async Task UpdatePointer(string owner,Action<UnityAccountPointer> change)
    {
        for(int retry=0;retry<8;retry++) {
            var slot=await Index(owner,"accounts");var value=Object(slot);
            var pointer=value[owner]?.ToObject<UnityAccountPointer>()??throw new InvalidDataException("Missing account pointer.");
            change(pointer);value[owner]=JObject.FromObject(pointer);
            if(await Publish(slot,value))return;
            await Task.Delay(Random.Shared.Next(8,36),ServerExecution.Token);
        }
        throw new InvalidOperationException("Account directory is busy. Retry safely.");
    }
    public async Task<(KeyValuePair<string,UnityAccountPointer>[] Entries,string After)> PointerPage(string shard,string after,int limit=16)
    {
        if(shard.Length!=2||shard.Any(c=>!Uri.IsHexDigit(c)))throw new ArgumentException("Invalid shard.");
        var output=new List<KeyValuePair<string,UnityAccountPointer>>();
        async Task Walk(string collection,string key,string prefix) {
            if(output.Count>=limit)return;
            if(!string.IsNullOrEmpty(after)&&string.CompareOrdinal(prefix+new string('f',64-prefix.Length),after)<=0)return;
            var record=await records.Read(collection,key)??throw new InvalidDataException("Missing directory node.");var value=Object(record);
            if(value["_flBranch"]!=null) {
                var children=value["Children"] as JArray??throw new InvalidDataException("Invalid directory branch.");
                if(children.Count!=16||value.Value<int>("Depth")!=prefix.Length)throw new InvalidDataException("Invalid directory depth.");
                for(int i=0;i<16&&output.Count<limit;i++)await Walk(children[i].Value<string>(),"value",prefix+i.ToString("x"));
            } else foreach(var entry in value.Properties().OrderBy(x=>x.Name,StringComparer.Ordinal)) {
                if(string.CompareOrdinal(entry.Name,after??"")<=0)continue;
                output.Add(new(entry.Name,entry.Value.ToObject<UnityAccountPointer>()));if(output.Count>=limit)break;
            }
        }
        await Walk(Prefix+"index_"+shard,"accounts",shard);
        return(output.ToArray(),output.Count==limit?output[^1].Key:null);
    }
}
