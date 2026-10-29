using Microsoft.Extensions.Logging;

namespace CascadeAccountModule;

public sealed record CloudWorkOutcome(bool Remove,long NextDue)
{
    public static CloudWorkOutcome Delete => new(true,0);
    public static CloudWorkOutcome Later(long due)=>new(false,due);
}
public sealed record CloudWorkSummary(int Selected,int Completed,int Deferred,int Failed,long OldestDueAge);

// Only due indexed records are read. Independent failures cannot poison the
// cursor or block another owner. Duplicates remain safe through the underlying
// snapshot/ownership CAS and idempotent publisher verification.
public sealed class CloudWorkPump(ICloudWorkRecords records,Func<long> clock=null)
{
    readonly Func<long> now=clock??(()=>DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    public async Task<CloudWorkSummary> Run(int lane,Func<CloudWorkRecord,Task<CloudWorkOutcome>> process,ILogger logger=null,int limit=100)
    {
        var jobs=await records.Due(lane,now(),limit);int done=0,deferred=0,failed=0;
        await Parallel.ForEachAsync(jobs,new ParallelOptions{MaxDegreeOfParallelism=4,CancellationToken=ServerExecution.Token},async(job,cancellation)=>{
            cancellation.ThrowIfCancellationRequested();
            if(ServerExecution.Remaining.TotalSeconds<3){Interlocked.Increment(ref deferred);return;}
            try {
                var outcome=await ServerExecution.Run("maintenance_"+job.Work.Kind,null,()=>process(job),Math.Min(4,ServerExecution.Remaining.TotalSeconds-2));
                if(outcome.Remove)await records.DeleteWorkObject(job.Collection);
                else {
                    var next=job.Work.Copy();next.Due=outcome.NextDue;next.Attempts=0;
                    await records.UpdateWork(job,next); // A CAS loser leaves the winner's durable job intact.
                }
                if(outcome.Remove||outcome.NextDue==CloudWork.Never)Interlocked.Increment(ref done);
                else Interlocked.Increment(ref deferred);
            } catch(Exception error)when(!ServerExecution.Token.IsCancellationRequested) {
                Interlocked.Increment(ref failed);
                var next=job.Work.Copy();next.Attempts=Math.Min(30,next.Attempts+1);
                next.Due=now()+Math.Min(3600,5L<<Math.Min(10,next.Attempts))+Random.Shared.Next(0,5);
                try {await records.UpdateWork(job,next);}catch(Exception)when(!ServerExecution.Token.IsCancellationRequested){}
                logger?.LogWarning("Backend maintenance retry kind={Kind} attempt={Attempt} failure={FailureType}",job.Work.Kind,next.Attempts,error.GetType().Name);
            }
        });
        long age=jobs.Count==0?0:Math.Max(0,now()-jobs.Min(x=>x.Work.Due));
        logger?.LogInformation("Backend maintenance lane={Lane} selected={Selected} completed={Completed} deferred={Deferred} failed={Failed} oldest_due_seconds={OldestDueAge}",lane,jobs.Count,done,deferred,failed,age);
        return new(jobs.Count,done,deferred,failed,age);
    }
}
