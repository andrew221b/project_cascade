using System.Security.Cryptography;
using System.Text;

namespace CascadeAccountModule;

// A worker-local optimization, never an ownership or transaction lock.
// Cache only completed, expiring service authorizations, scoped to the complete
// credential fingerprint. Failures are not cached; rotation changes the key.
public sealed class PublisherTokenCache(Func<long> clock = null)
{
    sealed class Entry
    {
        public string Value;
        public long Until,Touched;
        public int Leases;
        public readonly SemaphoreSlim Gate=new(1,1);
    }
    readonly Dictionary<string,Entry> entries = new(StringComparer.Ordinal);
    readonly object sync=new();
    readonly Func<long> now = clock ?? (() => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    public static string Scope(string material) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material)));

    public async Task<string> Get(string scope,Func<Task<(string Value,long Lifetime)>> create,CancellationToken cancellation)
    {
        Entry entry=Lease(scope,true);
        if(entry==null){var fresh=await create();if(string.IsNullOrWhiteSpace(fresh.Value))throw new InvalidDataException("Publisher authorization unavailable.");return fresh.Value;}
        bool entered=false;
        try {
            await entry.Gate.WaitAsync(cancellation);entered=true;
            if(entry.Value!=null&&entry.Until>now())return entry.Value;
            var fresh = await create();
            if(string.IsNullOrWhiteSpace(fresh.Value)) throw new InvalidDataException("Publisher authorization unavailable.");
            entry.Value=fresh.Value;entry.Until=now()+Math.Clamp(fresh.Lifetime-60,1,600);
            return fresh.Value;
        } finally {if(entered)entry.Gate.Release();Release(entry);}
    }
    public async Task Invalidate(string scope,CancellationToken cancellation,string rejectedToken=null)
    {
        Entry entry=Lease(scope,false);if(entry==null)return;bool entered=false;
        try {
            await entry.Gate.WaitAsync(cancellation);entered=true;
            // A delayed 401 for an older token must not evict a token another
            // request has already refreshed successfully.
            if(rejectedToken==null||entry.Value==rejectedToken){entry.Value=null;entry.Until=0;}
        } finally {if(entered)entry.Gate.Release();Release(entry);}
    }
    Entry Lease(string scope,bool create)
    {
        lock(sync) {
            if(!entries.TryGetValue(scope,out var entry)) {
                if(!create)return null;
                if(entries.Count>=16) {
                    var idle=entries.Where(x=>x.Value.Leases==0).OrderBy(x=>x.Value.Touched).FirstOrDefault();
                    if(idle.Value==null)return null;
                    entries.Remove(idle.Key);
                }
                entry=new Entry();entries.Add(scope,entry);
            }
            entry.Leases++;entry.Touched=now();return entry;
        }
    }
    void Release(Entry entry){lock(sync){entry.Leases--;}}
}
