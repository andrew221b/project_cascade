using System.Net;
using System.Security.Cryptography;
using System.Text;
using Cascade.Application;
using Cascade.Persistence;
using CascadeAccountModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class ErasureReliabilityChecks
{
    public static async Task Run()
    {
        int checks=0;void Check(bool value,string label){if(!value)throw new Exception(label);checks++;}
        var sdk=new WorkSdkTransport();sdk.Memory.Bootstrap();
        using var apple=ECDsa.Create(ECCurve.NamedCurves.nistP256);using var google=RSA.Create(2048);
        var config=new UnityPublisherConfiguration{BundleId="com.vickand.flyingladders",AppleEnvironment="Sandbox",AppleKeyId="fixture",AppleIssuerId="fixture",ApplePrivateKey=apple.ExportPkcs8PrivateKeyPem(),GoogleCredentialsJson=JsonConvert.SerializeObject(new{client_email="fixture@example.com",private_key=google.ExportPkcs8PrivateKeyPem()}),ServiceAccountKeyId="fixture-id",ServiceAccountSecret="fixture-secret",LegacyOwnerKey="fixture-legacy"};
        string cipher=UnityPublisherVault.Seal(JsonConvert.SerializeObject(config),sdk.Secret,"cascade-unity-vault-v1|"+sdk.Project+"|"+sdk.Environment),hash=CloudSaveAtomicStorage.Hash(cipher);
        await sdk.Memory.PutImmutable(CloudSaveAtomicStorage.Prefix+"control","vaultRef",hash);
        await sdk.Memory.PutImmutable(CloudSaveAtomicStorage.ContentCollection("vault",hash),"value",cipher);
        long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();var records=new PrivateCloudRecords(sdk.Client,sdk.Context);
        var storage=new CloudSaveAtomicStorage(records,sdk.Project,sdk.Environment,256,()=>now);
        var account=new StoredAccount();string receipt=CloudSaveAtomicStorage.Hash("fixture-paid-receipt");
        account.Account.Profile.Monetization.Grants.Add(new GrantReceipt{Id="store_"+receipt,Kind="store_coins",Product=PurchaseCatalog.Coins240,Amount=240});account.Account.Profile.Monetization.BonusCoins=240;
        string payload=CloudProfilePayload.Pack(AccountJson.Encode(account));
        Check(await storage.Write("alice",0,new UnityAccountSnapshot{PlayerId="alice",Payload=payload}),"Erasure fixture has an existing account with store history");
        Check(await storage.Write("bob",0,new UnityAccountSnapshot{PlayerId="bob",Payload=CloudProfilePayload.Pack(AccountJson.Encode(new StoredAccount()))}),"Another account exists independently");
        await storage.PutDeletionCapability("alice",CloudSaveAtomicStorage.Hash("alice-cap"),"alice-capability",now+600);
        await storage.PutDeletionCapability("bob",CloudSaveAtomicStorage.Hash("bob-cap"),"bob-capability",now+600);
        string owner=storage.PlayerHash("alice");await storage.Tombstone("alice");now+=31;
        int identities=0,legacyDeletes=0,foreignLegacyDeletes=0,legacyClassDeletes=0;bool fail=true;
        using var http=new ReliabilityHttp(request=>{
            string path=request.RequestUri!.AbsolutePath;
            if(fail){fail=false;return new(HttpStatusCode.ServiceUnavailable);}
            if(request.Method==HttpMethod.Get){string remoteOwner=path.Contains(sdk.Environment)?"alice":"bob";return new(HttpStatusCode.OK){Content=new StringContent(new JObject{{"results",new JArray(new JObject{{"key","owner"},{"value",new JObject{{"PlayerId",remoteOwner}}},{"writeLock","fixture-lock"}})}}.ToString(),Encoding.UTF8,"application/json")};}
            if(path.Contains("/player-identity/"))identities++;
            else if(path.Contains("/custom/iap_")){legacyDeletes++;if(!path.Contains(sdk.Environment))foreignLegacyDeletes++;}
            else legacyClassDeletes++;
            return new(HttpStatusCode.NoContent);
        });
        var operations=new UnityServerOperations(sdk.Client,null,new HttpClient(http),()=>now);
        bool pending=false;try{await operations.CleanupDeleted(sdk.Context,owner);}catch(Exception){pending=true;}
        Check(pending&&(await storage.FindOwner(owner)).DeletionPlayerId=="alice","A failed external erasure keeps a durable pending tombstone");
        await operations.CleanupDeleted(sdk.Context,owner);
        var middle=await storage.FindOwner(owner);
        Check(middle.LegacyErased&&middle.IdentityErased&&middle.DeletionPlayerId!=null,"Legacy and identity stages persist before private erasure is finalized");
        Check(legacyDeletes==1&&foreignLegacyDeletes==0,"Legacy receipt cleanup validates ownership independently in both environments");
        Check(identities==1,"Identity erasure is performed once and checkpointed");
        int completedLegacyCalls=legacyClassDeletes;
        await operations.CleanupDeleted(sdk.Context,owner);
        Check((await storage.FindOwner(owner)).ErasureEmptyObserved>0,"The first empty index pass remains pending for a visibility grace");
        now+=31;await operations.CleanupDeleted(sdk.Context,owner);
        var finished=await storage.FindOwner(owner);
        Check(finished.Deleted&&finished.PlayerId==null&&finished.DeletionPlayerId==null&&finished.Blob==null,"A second empty pass erases identifying pointers while retaining the replay tombstone");
        Check(identities==1&&legacyClassDeletes==completedLegacyCalls,"Restarted cleanup does not repeat completed remote erasure stages");
        Check((await storage.Read("bob")).Snapshot.Payload!=null&&(await records.Owned(storage.PlayerHash("bob"),100)).Count==2,"Another player's profile and deletion capability survive every cleanup stage");
        var processor=new CloudWorkProcessor(records,storage,(_,_)=>Task.CompletedTask,o=>operations.CleanupDeleted(sdk.Context,o),()=>now);
        var job=(await records.Owned(owner,100)).Single();Check((await processor.Process(job)).Remove,"The completed erasure ticket can be acknowledged and removed");await records.DeleteWorkObject(job.Collection);
        Check((await records.Owned(owner,100)).Count==0,"No indexed private data remains for the erased owner");
        bool blocked=false;try{await storage.Read("alice");}catch(InvalidOperationException){blocked=true;}Check(blocked,"Erased accounts cannot be resurrected as empty profiles");
        Console.WriteLine($"Erasure reliability checks: {checks} PASS; actual backend/SDK code, fake transport only.");
    }
}
