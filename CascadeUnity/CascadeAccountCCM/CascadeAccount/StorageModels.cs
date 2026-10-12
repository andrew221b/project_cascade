using Cascade.Persistence;
using Cascade.Application;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace CascadeAccountModule;

public sealed class CloudSyncReply
{
    public string Status { get; set; }
    public CloudAccount Account { get; set; }
    public string PackedAccount { get; set; }
    public bool EconomyConflict { get; set; }
    public string Error { get; set; }
    public long LastSequence { get; set; }
}

public sealed class StoredAccount
{
    public int Schema = 1;
    public CloudAccount Account = new CloudAccount();
    public List<DeviceSyncState> DeviceStates = new();
    public List<SyncReceipt> Receipts = new();
    public void Validate()
    {
        Account.Validate();
        if (Schema != 1 || DeviceStates == null || DeviceStates.Count > CloudAccount.MaxDevices || Receipts == null || Receipts.Count > 256
            || DeviceStates.Any(d => d == null || string.IsNullOrEmpty(d.DeviceId) || d.LastSequence < 0 || d.BaseProfile == null && d.PackedBaseProfile == null)
            || DeviceStates.Select(d => d.DeviceId).Distinct(StringComparer.Ordinal).Count() != DeviceStates.Count
            || Receipts.Any(r => r == null || r.Sequence <= 0 || string.IsNullOrEmpty(r.DeviceId) || string.IsNullOrEmpty(r.RequestId) || r.Digest?.Length != 64))
            throw new InvalidDataException("Invalid stored account envelope.");
        foreach (var device in DeviceStates) {
            var baseline=device.ReadBaseline(Account.Profile.Monetization);baseline.Validate();
            if (device.BaseHash != AccountJson.Hash(baseline)) throw new InvalidDataException("Device baseline checksum mismatch.");
        }
    }
}
public sealed class DeviceSyncState
{
    public string DeviceId;
    public long LastSequence;
    public PlayerProfile BaseProfile;
    public string BaseHash;
    // Optional compact baseline, with backward-compatible reads of the original field.
    public string PackedBaseProfile;
    public int? GrantPrefixCount;
    public string GrantPrefixHash;
    public PlayerProfile ReadBaseline(MonetizationLedger authoritative=null)
    {
        var profile=BaseProfile==null?AccountJson.Decode<PlayerProfile>(CloudProfilePayload.Unpack(PackedBaseProfile)):AccountJson.Clone(BaseProfile);
        if(GrantPrefixCount.HasValue){
            int count=GrantPrefixCount.Value;
            if(authoritative==null||count<0||count>authoritative.Grants.Count||profile.Monetization.Grants.Count!=0||GrantPrefixHash?.Length!=64)
                throw new InvalidDataException("Invalid baseline grant reference.");
            var grants=authoritative.Grants.Take(count).ToList();
            if(GrantPrefixHash!=AccountJson.Hash(grants))throw new InvalidDataException("Baseline grant prefix mismatch.");
            profile.Monetization.Grants=AccountJson.Clone(grants);
        }else if(GrantPrefixHash!=null)throw new InvalidDataException("Incomplete baseline grant reference.");
        return profile;
    }
    public void StoreBaseline(PlayerProfile profile,MonetizationLedger authoritative=null)
    {
        // Callers pass an owned baseline clone, never the authoritative account profile.
        // Ratings/reward facts and every economic receipt remain; duplicate gameplay proofs do not affect deltas.
        foreach(var clear in profile.AllClears){clear.Replay=null;clear.ScoreReplay=null;clear.ReplayRewardProof=null;clear.ReplayRewardPriorProof=null;}
        BaseHash=AccountJson.Hash(profile);GrantPrefixCount=null;GrantPrefixHash=null;
        if(authoritative!=null&&profile.Monetization.Grants.Count<=authoritative.Grants.Count){
            var prefix=authoritative.Grants.Take(profile.Monetization.Grants.Count).ToList();
            string hash=AccountJson.Hash(profile.Monetization.Grants);
            if(hash==AccountJson.Hash(prefix)){
                GrantPrefixCount=prefix.Count;GrantPrefixHash=hash;
                profile.Monetization.Grants=new List<GrantReceipt>();
            }
        }
        string json=AccountJson.Encode(profile);string packed=CloudProfilePayload.Pack(json);
        if(packed==json){BaseProfile=profile;PackedBaseProfile=null;}
        else {BaseProfile=null;PackedBaseProfile=packed;}
    }
}
public sealed class SyncReceipt
{
    public string DeviceId, RequestId, Digest;
    public long Sequence, Revision;
    public bool EconomyConflict;
}
public static class AccountJson
{
    public static readonly JsonSerializerSettings Settings = new() {
        ContractResolver = new WritableFields(), MaxDepth = 64,
        TypeNameHandling = TypeNameHandling.None, DateParseHandling = DateParseHandling.None,
        ObjectCreationHandling = ObjectCreationHandling.Replace
    };
    public static string Encode(object value) => JsonConvert.SerializeObject(value, Formatting.None, Settings);
    public static T Decode<T>(string value) => JsonConvert.DeserializeObject<T>(value, Settings);
    public static T Clone<T>(T value) => Decode<T>(Encode(value));
    public static string Hash(object value) => Digest(Encode(value));
    public static string Digest(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    sealed class WritableFields : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            => base.CreateProperties(type, memberSerialization).Where(p => p.Readable && p.Writable
                && (type != typeof(PlayerProfile) || p.PropertyName is not ("CloudAppliedRequestId" or "CloudAppliedEconomyConflict"))).ToList();
    }
}
