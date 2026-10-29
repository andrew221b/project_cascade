using Newtonsoft.Json.Linq;

namespace CascadeAccountModule;

public sealed class CloudWorkProcessor(IPrivateCloudRecords records,CloudSaveAtomicStorage storage,
    Func<string,string,Task> refreshSubscription,Func<string,Task> erase,Func<long> clock=null)
{
    readonly Func<long> now=clock??(()=>DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    public static long SubscriptionDue(UnitySubscription sub,long now)
    {
        if(sub.SupersededBy!=null)return CloudWork.Never;
        if(sub.Observed<=0)return now;
        // Signed notifications are the primary signal. The fallback checks at
        // expiry and backs off on long-expired chains instead of polling forever.
        if(sub.Expires>now)return Math.Min(sub.Observed+21600,sub.Expires+30);
        long expired=now-sub.Expires;
        return sub.Observed+(expired<86400?300:expired<30*86400?21600:7*86400);
    }
    public async Task<CloudWorkOutcome> Process(CloudWorkRecord job)
    {
        long time=now();var work=job.Work;work.Validate();
        if(work.EligibleAfter>time)return CloudWorkOutcome.Later(work.EligibleAfter);
        if(work.Kind=="index") {
            var parent=await records.Read(work.ParentCollection,work.ParentKey);
            bool published=parent?.Value is JObject node&&node["Children"] is JArray children&&children.Any(x=>x.Value<string>()==job.Collection);
            return published?CloudWorkOutcome.Later(CloudWork.Never):CloudWorkOutcome.Delete;
        }
        if(work.Kind=="capability")return CloudWorkOutcome.Delete;
        var pointer=await storage.FindOwner(work.Owner);
        if(work.Kind=="deletion") {
            if(pointer?.Deleted!=true||pointer.DeletionJob!=job.Collection||pointer.DeletionPlayerId==null)return CloudWorkOutcome.Delete;
            await erase(work.Owner);
            pointer=await storage.FindOwner(work.Owner);
            return pointer?.DeletionPlayerId==null?CloudWorkOutcome.Delete:CloudWorkOutcome.Later(time+30);
        }
        if(pointer?.Deleted==true&&pointer.DeletionPlayerId!=null)return CloudWorkOutcome.Later(time+60);
        bool current=pointer?.Blob!=null&&CloudSaveAtomicStorage.ContentCollection("blob",pointer.Blob)==job.Collection;
        if(!current) {
            // New publications have a random per-attempt nonce. An abandoned
            // candidate can never be reused by a future writer. Legacy names
            // need the stronger version guard until their owner advances.
            if(!work.UniquePublication&&pointer?.Deleted!=true&&(pointer==null||pointer.Version<work.Version))return CloudWorkOutcome.Later(time+21600);
            return CloudWorkOutcome.Delete;
        }
        if(!pointer.HasSubscriptions)return CloudWorkOutcome.Later(CloudWork.Never);
        var state=await storage.Read(pointer.PlayerId);
        var next=state.Snapshot.Subscriptions.Select(sub=>(Sub:sub,Due:SubscriptionDue(sub,time))).OrderBy(x=>x.Due).FirstOrDefault();
        if(next.Sub==null)return CloudWorkOutcome.Later(CloudWork.Never);
        if(next.Due>time)return CloudWorkOutcome.Later(next.Due);
        await refreshSubscription(pointer.PlayerId,next.Sub.Chain);
        return CloudWorkOutcome.Later(time+300);
    }
}
