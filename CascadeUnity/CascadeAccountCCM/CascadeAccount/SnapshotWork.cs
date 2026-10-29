using Newtonsoft.Json.Linq;

namespace CascadeAccountModule;

public sealed partial class CloudSaveAtomicStorage
{
    async Task RetireSnapshot(string owner,UnityAccountPointer pointer)
    {
        var work=(ICloudWorkRecords)records;string collection=BlobCollection(pointer.Blob);
        for(int retry=0;retry<8;retry++) {
            long now=NowUnix;var prior=await work.ReadWork(collection);
            if(prior==null) {
                var value=await records.Read(collection,"value")??throw new InvalidDataException("Previous snapshot missing.");
                await work.InitializeUnpublished(collection,value.Value,new CloudWork{Kind="snapshot",Owner=owner,Version=pointer.Version,Due=now+300,EligibleAfter=now+300});
                return;
            }
            if(prior.Work.Kind!="snapshot"||prior.Work.Owner!=owner||prior.Work.Version!=pointer.Version)throw new InvalidDataException("Snapshot cleanup scope mismatch.");
            // A recent snapshot already has a durable, safely delayed job. Most
            // normal gameplay writes avoid a second metadata write entirely.
            if(prior.Work.Due<CloudWork.Never&&prior.Work.Due>now+30&&prior.Work.EligibleAfter>now+30)return;
            var next=prior.Work.Copy();next.Due=next.EligibleAfter=now+300;next.Attempts=0;
            if(await work.UpdateWork(prior,next))return;
        }
        throw new InvalidOperationException("Snapshot cleanup changed. Retry safely.");
    }
    public async Task PutDeletionCapability(string player,string hash,string json,long expires)
    {
        string owner=PlayerHash(player);
        if(records is ICloudWorkRecords work) {
            await work.InitializeUnpublished(ContentCollection("delete",hash),new JValue(json),new CloudWork{Kind="capability",Owner=owner,Due=expires+30,EligibleAfter=expires+30,UniquePublication=true});
        } else {
            await QueueDeletionCode(player,hash,expires);
            await records.PutImmutable(ContentCollection("delete",hash),"value",json);
        }
    }
}
