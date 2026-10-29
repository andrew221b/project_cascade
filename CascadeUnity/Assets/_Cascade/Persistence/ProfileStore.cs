using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cascade.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Cascade.Persistence
{
    public enum SaveRecovery { None, Backup, Unreadable, NewerVersion, AttemptUnavailable, WriteFailed }
    public sealed class ProfileLoad
    {
        public PlayerProfile Profile { get; }
        public SaveRecovery Recovery { get; }
        internal ProfileLoad(PlayerProfile profile,SaveRecovery recovery){Profile=profile;Recovery=recovery;}
    }
    public sealed class ProfileStore
    {
        public const int Schema=2;
        const int MaxBytes=8*1024*1024;
        readonly string path;
        readonly bool persistSessionAttempts;
        bool futureVersion;
        readonly object writes=new object(),disk=new object();
        string pending;
        Task writer;
        Exception writeError;
        public Exception WriteError { get { lock(writes)return writeError; } }
        // Snapshot first: the worker never reads mutable gameplay objects.
        public void QueueSave(PlayerProfile profile)
        {
            string text=Prepare(profile);
            lock(writes) {
                if(futureVersion)throw new InvalidOperationException("A newer save must remain untouched.");
                pending=text;
                if(writer==null||writer.IsCompleted)writer=Task.Run(Drain);
            }
        }
        void Drain()
        {
            lock(disk)while(true) {
                string next;
                lock(writes) {next=pending;pending=null;if(next==null){writer=null;return;}}
                try {Write(next);lock(writes)writeError=null;}
                catch(Exception error) {lock(writes){writeError=error;if(pending==null)pending=next;writer=null;}return;}
            }
        }
        public void Flush()
        {
            Task active;lock(writes)active=writer;
            active?.GetAwaiter().GetResult();
            Drain();
            lock(writes)if(writeError!=null)throw new IOException("Profile write failed.",writeError);
        }
        string Prepare(PlayerProfile profile)
        {
            profile.Validate();string text=Encode(persistSessionAttempts?profile:profile.ProgressSnapshot());
            if(Encoding.UTF8.GetByteCount(text)>MaxBytes)throw new InvalidDataException("Profile exceeds supported size.");
            return text;
        }
        public string Path=>path;
        public string BackupPath=>path+".bak";
        public ProfileStore(string path,bool persistSessionAttempts=true){this.path=System.IO.Path.GetFullPath(path);this.persistSessionAttempts=persistSessionAttempts;}
        PlayerProfile LoadedProfile(PlayerProfile profile){if(!persistSessionAttempts)profile.DiscardSessionAttempts();return profile;}
        public ProfileLoad Load()
        {
            Flush();
            var main=TryRead(path,out bool future);
            futureVersion=future;
            if(future)return new ProfileLoad(new PlayerProfile(),SaveRecovery.NewerVersion);
            if(main!=null)return new ProfileLoad(LoadedProfile(main),SaveRecovery.None);
            var backup=TryRead(BackupPath,out future);
            futureVersion=future;
            if(future)return new ProfileLoad(new PlayerProfile(),SaveRecovery.NewerVersion);
            if(backup!=null)return new ProfileLoad(LoadedProfile(backup),SaveRecovery.Backup);
            return new ProfileLoad(new PlayerProfile(),File.Exists(path)||File.Exists(BackupPath)?SaveRecovery.Unreadable:SaveRecovery.None);
        }
        public void Save(PlayerProfile profile)
        {
            string text=Prepare(profile);
            // An economic/cloud commit supersedes queued older autosaves atomically.
            lock(disk) {
                lock(writes)pending=null;
                Write(text);
                lock(writes)writeError=null;
            }
        }
        void Write(string text)
        {
            if(futureVersion)throw new InvalidOperationException("A newer save must remain untouched.");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            string temporary=path+".tmp";
            using(var stream=new FileStream(temporary,FileMode.Create,FileAccess.Write,FileShare.None)) {
                byte[] data=Encoding.UTF8.GetBytes(text);stream.Write(data,0,data.Length);stream.Flush(true);
            }
            // Never replace a future schema or rotate a corrupt original over the last good backup.
            var previous=TryRead(path,out bool future);
            if(future){futureVersion=true;throw new InvalidOperationException("A newer save must remain untouched.");}
            if(previous!=null)File.Replace(temporary,path,BackupPath);
            else {
                if(File.Exists(path))File.Move(path,path+".rejected-"+Guid.NewGuid().ToString("N"));
                File.Move(temporary,path);
            }
        }
        public static string Encode(PlayerProfile profile)
        {
            string payload=JsonConvert.SerializeObject(profile,Formatting.None,new JsonSerializerSettings{ContractResolver=new StoredFields()});
            return new JObject { ["schema"]=Schema,["sha256"]=ContentImporter.Hash(payload),["payload"]=payload }.ToString(Formatting.None);
        }
        sealed class StoredFields:DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type,MemberSerialization memberSerialization)
                =>base.CreateProperties(type,memberSerialization).Where(p=>p.Readable&&p.Writable).ToList();
        }
        static PlayerProfile TryRead(string file,out bool future)
        {
            future=false;
            if(!File.Exists(file))return null;
            try {
                if(new FileInfo(file).Length>MaxBytes)return null;
                var envelope=JObject.Parse(File.ReadAllText(file));
                if(envelope["schema"]?.Type!=JTokenType.Integer)return null;
                int schema=(int)envelope["schema"];
                if(schema>Schema){future=true;return null;}
                if((schema!=1&&schema!=Schema) || envelope["payload"]?.Type!=JTokenType.String || envelope["sha256"]?.Type!=JTokenType.String)return null;
                string payload=(string)envelope["payload"];
                if(ContentImporter.Hash(payload)!=(string)envelope["sha256"])return null;
                var profile=JsonConvert.DeserializeObject<PlayerProfile>(payload,new JsonSerializerSettings {MaxDepth=32,TypeNameHandling=TypeNameHandling.None});
                if(profile==null)return null;
                if(schema==1&&profile.Rewards?.Records!=null)profile.ModeRecords=profile.Rewards.Records.ToList();
                profile.MigrateCoinEconomy();
                profile.Validate();return profile;
            }
            catch(Exception e) when(e is IOException || e is UnauthorizedAccessException || e is JsonException || e is ArgumentException || e is OverflowException) { return null; }
        }
    }
}
