using System;
using System.IO;
using System.Text;
using Cascade.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Cascade.Persistence
{
    public sealed class CloudJournal
    {
        public int Schema=1;
        public string DeviceId=Guid.NewGuid().ToString("D"),PlayerId,ProjectId,Environment;
        public long Sequence,Revision;
        public string RequestId,PendingJson,BaseJson,LastSyncUtc;
        public string AuthenticationProfile="default",AccountCandidateProfile;
        public bool EconomyConflict,AccountLoadPending,SignedOut;
        public bool RetireRejectedProgress(string error)
        {
            if(error!="UnverifiedProgress"||PendingJson==null)return false;
            PendingJson=null;RequestId=null;return true; // Sequence is never reused.
        }
        public void Validate()
        {
            if(Schema!=1||!Guid.TryParse(DeviceId,out _)||Sequence<0||Revision<0||PlayerId?.Length>128
                ||ProjectId?.Length>128||Environment?.Length>64||PendingJson?.Length>CloudProfilePayload.MaxDecodedBytes||BaseJson?.Length>CloudProfilePayload.MaxDecodedBytes
                ||!ValidAuthProfile(AuthenticationProfile)||AccountCandidateProfile!=null&&!ValidAuthProfile(AccountCandidateProfile)
                ||(RequestId==null)!=(PendingJson==null))throw new InvalidDataException("Unsupported cloud journal.");
            if(RequestId!=null){var parts=RequestId.Split(':');if(parts.Length!=2||!long.TryParse(parts[0],out var requestSequence)||requestSequence!=Sequence||requestSequence<1||!Guid.TryParse(parts[1],out _))throw new InvalidDataException("Invalid pending request identity.");}
        }
        static bool ValidAuthProfile(string value)=>value!=null&&value.Length>0&&value.Length<=30
            &&value.All(c=>c>='a'&&c<='z'||c>='A'&&c<='Z'||c>='0'&&c<='9'||c=='_'||c=='-');
    }
    public static class CloudJson
    {
        sealed class StoredFields:DefaultContractResolver
        {protected override IList<JsonProperty> CreateProperties(Type type,MemberSerialization mode)=>base.CreateProperties(type,mode).Where(p=>p.Readable&&p.Writable).ToList();}
        static readonly JsonSerializerSettings settings=new JsonSerializerSettings{ContractResolver=new StoredFields(),TypeNameHandling=TypeNameHandling.None,MaxDepth=64};
        public static string Encode(object value)=>JsonConvert.SerializeObject(value,settings);
        public static T Decode<T>(string value)=>JsonConvert.DeserializeObject<T>(value,settings);
        public static PlayerProfile Clone(PlayerProfile value)=>Decode<PlayerProfile>(Encode(value));
    }
    // The pending request is durable before any network call. No credentials or raw purchase receipts are stored here.
    public sealed class CloudJournalStore
    {
        readonly string path;
        public CloudJournalStore(string path){this.path=path;}
        public CloudJournal Load()
        {
            foreach(var candidate in new[]{path,path+".bak"}) {
                if(!File.Exists(candidate))continue;
                try {
                    if(new FileInfo(candidate).Length>32*1024*1024)continue;
                    var envelope=JObject.Parse(File.ReadAllText(candidate));string payload=(string)envelope["payload"];
                    if(ContentImporter.Hash(payload)!=(string)envelope["sha256"])continue;
                    var journal=CloudJson.Decode<CloudJournal>(payload);journal.Validate();return journal;
                }catch(Exception e)when(e is IOException||e is JsonException||e is ArgumentException){}
            }
            // Preserve unreadable journals for recovery rather than reusing a sequence with the wrong identity.
            if(File.Exists(path))File.Copy(path,path+".unreadable",true);
            return new CloudJournal();
        }
        public void Save(CloudJournal journal)
        {
            journal.Validate();Directory.CreateDirectory(Path.GetDirectoryName(path));
            string payload=CloudJson.Encode(journal),temporary=path+".tmp";
            byte[] bytes=Encoding.UTF8.GetBytes(new JObject{["payload"]=payload,["sha256"]=ContentImporter.Hash(payload)}.ToString(Formatting.None));
            if(bytes.Length>32*1024*1024)throw new InvalidDataException("Cloud journal exceeds supported size.");
            using(var stream=new FileStream(temporary,FileMode.Create,FileAccess.Write,FileShare.None)){stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
            if(File.Exists(path))File.Replace(temporary,path,path+".bak");else File.Move(temporary,path);
        }
    }
}
