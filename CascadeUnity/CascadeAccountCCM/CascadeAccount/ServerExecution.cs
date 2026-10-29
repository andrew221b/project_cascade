using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CascadeAccountModule;

// AsyncLocal is scoped to a single asynchronous invocation, including its
// bounded child jobs. Nothing here is a cross-worker lock or a player cache.
public static class ServerExecution
{
    sealed class Counters { public int Reads,Writes,Conflicts; }
    sealed class Frame
    {
        public CancellationToken Token;
        public double EndsAt;
        public Counters Counts;
        public ConcurrentDictionary<string,Lazy<Task<object>>> Stable;
    }
    static readonly AsyncLocal<Frame> current = new();
    public static CancellationToken Token => current.Value?.Token ?? CancellationToken.None;
    public static TimeSpan Remaining => current.Value is { } frame ? TimeSpan.FromSeconds(Math.Max(0,(frame.EndsAt-Stopwatch.GetTimestamp())/Stopwatch.Frequency)) : TimeSpan.MaxValue;
    public static void Read() { if(current.Value is { } frame) Interlocked.Increment(ref frame.Counts.Reads); }
    public static void Write() { if(current.Value is { } frame) Interlocked.Increment(ref frame.Counts.Writes); }
    public static void Conflict() { if(current.Value is { } frame) Interlocked.Increment(ref frame.Counts.Conflicts); }

    public static async Task<T> Stable<T>(string key,Func<Task<T>> load)
    {
        var frame=current.Value;
        if(frame==null) return await load();
        // Only route/vault reference callers use this. Mutable accounts, grants,
        // ownership indexes and write locks must always be read afresh.
        return (T)await frame.Stable.GetOrAdd(key,_=>new Lazy<Task<object>>(async()=>await load())).Value;
    }

    public static async Task<T> Run<T>(string operation,ILogger logger,Func<Task<T>> action,double seconds=12)
    {
        var parent=current.Value;
        using var deadline=CancellationTokenSource.CreateLinkedTokenSource(parent?.Token ?? CancellationToken.None);
        deadline.CancelAfter(TimeSpan.FromSeconds(seconds));
        var frame=new Frame{Token=deadline.Token,EndsAt=Math.Min(parent?.EndsAt??double.MaxValue,Stopwatch.GetTimestamp()+seconds*Stopwatch.Frequency),Counts=parent?.Counts??new Counters(),Stable=parent?.Stable??new()};
        current.Value=frame;var timer=Stopwatch.StartNew();string outcome="ok";
        try { return await action().WaitAsync(deadline.Token); }
        catch(OperationCanceledException) { outcome="deadline";throw; }
        catch { outcome="failed";throw; }
        finally {
            current.Value=parent;
            if(logger!=null) logger.LogInformation("Backend {Operation} {Outcome} elapsed_ms={ElapsedMs} reads={Reads} writes={Writes} conflicts={Conflicts}",
                operation,outcome,timer.ElapsedMilliseconds,frame.Counts.Reads,frame.Counts.Writes,frame.Counts.Conflicts);
        }
    }
}
