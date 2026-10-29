using CascadeAccountModule;
using Newtonsoft.Json.Linq;

public static class DirectoryWorkChecks
{
    public static async Task Run()
    {
        int checks=0;void Check(bool value,string label){if(!value)throw new Exception(label);checks++;}
        long now=100000;var records=new AtomicMemoryRecords();records.Bootstrap();
        var storage=new CloudSaveAtomicStorage(records,"project","production",4,()=>now);
        var players=Enumerable.Range(0,100000).Select(i=>"owner-"+i).Where(p=>storage.PlayerHash(p).StartsWith("ab")).Take(8).ToArray();
        for(int i=0;i<4;i++)Check(await storage.Write(players[i],0,new UnityAccountSnapshot{PlayerId=players[i],Payload="profile-"+i}),"Initial directory owners persist");
        records.RejectNextBranchPublication=1;
        Check(!await storage.Write(players[4],0,new UnityAccountSnapshot{PlayerId=players[4],Payload="profile-4"}),"A failed parent split CAS does not publish partial children");
        Check(await storage.Write(players[4],0,new UnityAccountSnapshot{PlayerId=players[4],Payload="profile-4"}),"A split retry publishes another uniquely prepared generation");
        for(int i=5;i<players.Length;i++)Check(await storage.Write(players[i],0,new UnityAccountSnapshot{PlayerId=players[i],Payload="profile-"+i}),"New owners route through existing split children");
        for(int i=0;i<players.Length;i++)Check((await storage.Read(players[i])).Snapshot.Payload=="profile-"+i,"Directory split preserves every owner's authoritative payload");
        var branchJobs=records.Jobs().Where(j=>j.Work.Kind=="index").ToArray();
        Check(branchJobs.Length==32,"Two competing directory generations remain independently tracked");
        now+=301;
        var processor=new CloudWorkProcessor(records,storage,(_,_)=>throw new Exception("Unexpected subscription"),_=>throw new Exception("Unexpected deletion"),()=>now);
        foreach(var job in records.Jobs()) {
            var result=await processor.Process(job);
            if(result.Remove)await records.DeleteWorkObject(job.Collection);
            else {var next=job.Work.Copy();next.Due=result.NextDue;await records.UpdateWork(job,next);}
        }
        Check(branchJobs.Count(j=>records.Jobs().Any(x=>x.Collection==j.Collection))==16,"Only the published index generation survives cleanup");
        for(int i=0;i<players.Length;i++)Check((await storage.Read(players[i])).Snapshot.Payload=="profile-"+i,"Orphan cleanup cannot erase published children or current snapshots");

        string alice=players[0];var before=await storage.Read(alice);string oldCollection=CloudSaveAtomicStorage.ContentCollection("blob",before.Pointer.Blob);
        var gcRead=await records.ReadWork(oldCollection);var gcDecision=await processor.Process(gcRead);
        Check(gcDecision.NextDue==CloudWork.Never,"An idle current snapshot leaves the due queue");
        Check(await storage.Write(alice,before.Pointer.Version,new UnityAccountSnapshot{PlayerId=alice,Payload="newer"}),"A write retires its previous snapshot durably before publication");
        var staleUpdate=gcRead.Work.Copy();staleUpdate.Due=CloudWork.Never;
        Check(!await records.UpdateWork(gcRead,staleUpdate),"A stale collector cannot cancel the writer's newly scheduled retirement");
        now+=301;var retired=await records.ReadWork(oldCollection);Check((await processor.Process(retired)).Remove,"Retired snapshots become collectable after the in-flight-write grace");
        await records.DeleteWorkObject(oldCollection);Check((await storage.Read(alice)).Snapshot.Payload=="newer","Deleting a retired snapshot preserves the newer account");

        string retryOwner="never-published";records.RejectNextAccountPublication=1;
        Check(!await storage.Write(retryOwner,0,new UnityAccountSnapshot{PlayerId=retryOwner,Payload="same"}),"A failed first publication remains an uncommitted candidate");
        var orphan=records.Jobs().Single(j=>j.Work.Owner==storage.PlayerHash(retryOwner));
        Check(await storage.Write(retryOwner,0,new UnityAccountSnapshot{PlayerId=retryOwner,Payload="same"}),"The same user data can retry its first publication");
        Check(CloudSaveAtomicStorage.ContentCollection("blob",(await storage.Read(retryOwner)).Pointer.Blob)!=orphan.Collection,"A fresh publication nonce prevents reuse of an old failed snapshot name");
        now+=301;Check((await processor.Process(orphan)).Remove,"A failed candidate can be removed without waiting for another account revision");await records.DeleteWorkObject(orphan.Collection);
        Check((await storage.Read(retryOwner)).Snapshot.Payload=="same","Candidate removal never removes a future retry's state");

        var workRecords=new AtomicMemoryRecords();workRecords.Bootstrap();string owner=CloudSaveAtomicStorage.Hash("work-owner");int lane=Convert.ToInt32(owner[..1],16);
        for(int i=0;i<5;i++)await workRecords.InitializeUnpublished(CloudSaveAtomicStorage.Prefix+"delete_"+i,new JValue("test"),new CloudWork{Kind="capability",Owner=owner,Due=now-1,EligibleAfter=now-1,UniquePublication=true});
        string failing=CloudSaveAtomicStorage.Prefix+"delete_0";
        var pump=new CloudWorkPump(workRecords,()=>now);
        var summary=await pump.Run(lane,job=>job.Collection==failing?throw new HttpRequestException("injected outage"):Task.FromResult(CloudWorkOutcome.Delete));
        Check(summary.Failed==1&&summary.Completed==4,"One failed maintenance job does not block four independent owners/jobs");
        var pending=await workRecords.ReadWork(failing);Check(pending.Work.Attempts==1&&pending.Work.Due>now,"Failure persists backoff instead of acknowledging or dropping work");
        now=pending.Work.Due+1;summary=await pump.Run(lane,_=>Task.FromResult(CloudWorkOutcome.Delete));Check(summary.Completed==1&&workRecords.Jobs().Length==0,"A later worker completes a failed job after a process/request restart");
        bool refused=false;try{await workRecords.DeleteWorkObject(CloudSaveAtomicStorage.Prefix+"control");}catch(InvalidDataException){refused=true;}Check(refused,"Maintenance cannot erase configuration or account directories");

        long time=200000;var active=new UnitySubscription{Observed=time,Expires=time+100};
        Check(CloudWorkProcessor.SubscriptionDue(active,time)==time+130,"Subscription fallback wakes shortly after expiry");
        active.Expires=time+30*86400;Check(CloudWorkProcessor.SubscriptionDue(active,time)==time+21600,"Long-lived subscriptions avoid hourly publisher polling");
        active.SupersededBy="new";Check(CloudWorkProcessor.SubscriptionDue(active,time)==CloudWork.Never,"Superseded chains do not poll forever");
        int sideEffect=0;bool canceled=false;
        try{await ServerExecution.Run("deadline-test",null,async()=>{await Task.Delay(1000,ServerExecution.Token);sideEffect++;return true;},0.03);}catch(OperationCanceledException){canceled=true;}
        await Task.Delay(50);Check(canceled&&sideEffect==0,"Request cancellation reaches awaited I/O before a late mutation can start");
        Check(records.UnlockedMutableWrites==0,"Live directory/account writes still require real existing CAS locks");
        Console.WriteLine($"Directory/work reliability checks: {checks} PASS; isolated transport, no live data changes.");
    }
}
