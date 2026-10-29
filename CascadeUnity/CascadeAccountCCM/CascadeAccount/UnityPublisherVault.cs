using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
namespace CascadeAccountModule;

public sealed class UnityPublisherConfiguration
{
    public string BundleId,ApplePrivateKey,AppleKeyId,AppleIssuerId,AppleEnvironment,AppleAppId,GoogleCredentialsJson;
    public string ServiceAccountKeyId,ServiceAccountSecret,LegacyOwnerKey;
}
public sealed class UnityCipherEnvelope
{
    public int Schema=1;
    public string Iv,Ciphertext,Tag;
}
// Ciphertext is private server configuration. Its master remains in Unity Secret
// Manager; no endpoint returns decrypted credentials or puts them in player data.
public static class UnityPublisherVault
{
    sealed record Cached(UnityPublisherConfiguration Value,long Until);
    static readonly Dictionary<string,Cached> cache=new(StringComparer.Ordinal);
    static readonly SemaphoreSlim cacheGate=new(1,1);
    static string Aad(IExecutionContext context)=>"cascade-unity-vault-v1|"+context.ProjectId+"|"+context.EnvironmentId;
    static byte[] Key(string token)=>SHA256.HashData(Encoding.UTF8.GetBytes("cascade-unity-vault-v1|"+token));
    public static string Open(string envelope,string master,string aad)
    {
        var value=JsonConvert.DeserializeObject<UnityCipherEnvelope>(envelope);
        if(value?.Schema!=1||value.Iv==null||value.Tag==null||value.Ciphertext?.Length>3*1024*1024)throw new InvalidDataException("Invalid migration envelope.");
        byte[] iv=Convert.FromBase64String(value.Iv),tag=Convert.FromBase64String(value.Tag),cipher=Convert.FromBase64String(value.Ciphertext),plain=new byte[cipher.Length];
        if(iv.Length!=12||tag.Length!=16)throw new InvalidDataException("Invalid migration envelope.");
        using var aes=new AesGcm(Key(master),16);aes.Decrypt(iv,cipher,tag,plain,Encoding.UTF8.GetBytes(aad));
        return Encoding.UTF8.GetString(plain);
    }
    public static string Seal(string json,string master,string aad)
    {
        byte[] iv=RandomNumberGenerator.GetBytes(12),plain=Encoding.UTF8.GetBytes(json),cipher=new byte[plain.Length],tag=new byte[16];
        using var aes=new AesGcm(Key(master),16);aes.Encrypt(iv,plain,cipher,tag,Encoding.UTF8.GetBytes(aad));
        return JsonConvert.SerializeObject(new UnityCipherEnvelope{Iv=Convert.ToBase64String(iv),Ciphertext=Convert.ToBase64String(cipher),Tag=Convert.ToBase64String(tag)});
    }
    public static async Task<string> OpenMigration(IGameApiClient api,IExecutionContext context,string envelope)
    {
        string master=(await api.SecretManager.GetSecret(context,"CASCADE_RECEIPT_VERIFIER_TOKEN")).Value;
        return Open(envelope,master,Aad(context));
    }
    public static async Task<UnityPublisherConfiguration> Load(IGameApiClient api,IExecutionContext context)
    {
        var records=new PrivateCloudRecords(api,context);var reference=await ServerExecution.Stable("vaultRef|"+context.ProjectId+"|"+context.EnvironmentId,()=>records.Read(CloudSaveAtomicStorage.Prefix+"control","vaultRef"));string hash=reference?.Value.Value<string>();
        if(hash?.Length!=64)throw new InvalidOperationException("Unity publisher credentials are not installed.");
        string scope=context.ProjectId+"|"+context.EnvironmentId+"|"+hash;
        await cacheGate.WaitAsync(ServerExecution.Token);
        try {
            long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if(cache.TryGetValue(scope,out var cached)&&cached.Until>now) return JsonConvert.DeserializeObject<UnityPublisherConfiguration>(JsonConvert.SerializeObject(cached.Value));
            var record=await records.Read(CloudSaveAtomicStorage.ContentCollection("vault",hash),"value");
            string envelope=record?.Value.Value<string>();if(envelope==null)throw new InvalidOperationException("Unity publisher credentials are not installed.");
            var config=JsonConvert.DeserializeObject<UnityPublisherConfiguration>(await OpenMigration(api,context,envelope));
            Validate(config);
            if(cache.Count>=8)cache.Remove(cache.MinBy(x=>x.Value.Until).Key);
            cache[scope]=new(config,now+600);
            return JsonConvert.DeserializeObject<UnityPublisherConfiguration>(JsonConvert.SerializeObject(config));
        } finally {cacheGate.Release();}
    }
    static void Validate(UnityPublisherConfiguration config)
    {
        if(config?.BundleId!="com.vickand.flyingladders"||config.AppleEnvironment is not ("Sandbox" or "Production")||string.IsNullOrWhiteSpace(config.GoogleCredentialsJson)
            ||string.IsNullOrWhiteSpace(config.ApplePrivateKey)||string.IsNullOrWhiteSpace(config.LegacyOwnerKey))throw new InvalidDataException("Publisher configuration incomplete.");
    }

    public static async Task Install(IGameApiClient api,IExecutionContext context,UnityPublisherConfiguration config)
    {
        if(await UnityBackendRouter.Mode(api,context)=="Unity")throw new InvalidOperationException("Publisher installation is closed after activation.");
        if(config.BundleId!="com.vickand.flyingladders"||config.AppleEnvironment is not ("Sandbox" or "Production")||string.IsNullOrWhiteSpace(config.LegacyOwnerKey))throw new InvalidDataException("Publisher configuration incomplete.");
        string master=(await api.SecretManager.GetSecret(context,"CASCADE_RECEIPT_VERIFIER_TOKEN")).Value;
        var records=new PrivateCloudRecords(api,context);string json=JsonConvert.SerializeObject(config);
        var reference=await records.Read(CloudSaveAtomicStorage.Prefix+"control","vaultRef");
        if(reference==null||string.IsNullOrEmpty(reference.WriteLock))throw new InvalidOperationException("Unity vault is not bootstrapped.");
        string existingHash=reference.Value.Value<string>();
        if(!string.IsNullOrEmpty(existingHash)){
            var existing=await records.Read(CloudSaveAtomicStorage.ContentCollection("vault",existingHash),"value");
            string old=Open(existing.Value.Value<string>(),master,Aad(context));
            if(old!=json)throw new InvalidOperationException("Existing publisher credentials do not match migration.");return;
        }
        string envelope=Seal(json,master,Aad(context)),hash=CloudSaveAtomicStorage.Hash(envelope);
        await records.PutImmutable(CloudSaveAtomicStorage.ContentCollection("vault",hash),"value",envelope);
        if(!await records.CompareExchange(CloudSaveAtomicStorage.Prefix+"control","vaultRef",new JValue(hash),reference.WriteLock)){
            var installed=await Load(api,context);
            if(JsonConvert.SerializeObject(installed)!=json)throw new InvalidOperationException("Publisher installation conflict.");
        }
    }
}
