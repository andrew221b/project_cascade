using System.Reflection;
using Cascade.Editor.Release;
using Cascade.Persistence;
using Cascade.Presentation;
using Newtonsoft.Json;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using Time=UnityEngine.Time;

int passed=0;string root=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../../"));
string temp=Path.Combine(Path.GetTempPath(),"cascade-account-checks-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);
void Check(bool value,string name){if(!value)throw new Exception(name);passed++;}
void Reject(Action action,string name){try{action();}catch(Exception e)when(e is InvalidDataException||e is JsonException){Check(true,name);return;}throw new Exception(name);}
CloudJournal Journal(CloudSyncClient client)=>(CloudJournal)typeof(CloudSyncClient).GetField("journal",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(client);
void Choice(CloudSyncClient client)=>typeof(CloudSyncClient).GetMethod("Set",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(client,new object[]{CloudSyncState.AccountChoice});
void PauseSync(CloudSyncClient client)=>typeof(CloudSyncClient).GetField("dirty",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(client,false);
CloudPlayerSettings Settings()=>new(){enabledForPlayers=true,playerAccountsConfigured=true,purchaseVerificationConfigured=true,environment="production"};
void Reset(){UnityServices.State=ServicesInitializationState.Uninitialized;UnityServices.Initialize=()=>Task.CompletedTask;AuthenticationService.Instance=new();PlayerAccountService.Instance=new();CloudCodeService.Instance=new();Time.realtimeSinceStartupAsDouble=10;}
CloudSyncClient Client(Action<PlayerProfile,bool> apply=null,Func<PlayerProfile> current=null)=>new(Settings(),Path.Combine(temp,Guid.NewGuid().ToString("N"),"journal.json"),current??(()=>new PlayerProfile()),apply??((_,__)=>{}),new[]{"C01"});
try {
    var config=AuthenticationConfiguration.Read(Path.Combine(root,"Configs/authentication.json"),UnityEngine.Application.cloudProjectId);
    Check(!config.HasClient&&!config.cloudCodeDeployedAndVerified,"deferred configuration stays inactive");
    var valid=JsonConvert.DeserializeObject<AuthenticationConfiguration>(JsonConvert.SerializeObject(config));
    valid.unityPlayerAccountsClientId="public-client_123";valid.cloudCodeDeployedAndVerified=true;valid.purchaseVerificationConfigured=true;
    valid.Validate(UnityEngine.Application.cloudProjectId);Check(true,"complete public config accepted");
    valid.unityProjectId=Guid.NewGuid().ToString("D");Reject(()=>valid.Validate(UnityEngine.Application.cloudProjectId),"wrong project blocked");valid.unityProjectId=config.unityProjectId;
    valid.unityPlayerAccountsClientId="";Reject(()=>valid.Validate(config.unityProjectId),"active cloud without client blocked");valid.unityPlayerAccountsClientId="client";
    valid.cloudCodeDeployedAndVerified=false;Reject(()=>valid.Validate(config.unityProjectId),"purchase flag without backend blocked");valid.purchaseVerificationConfigured=false;
    foreach(string bad in new[]{"production;rm"," production","prod/other","é",""}){valid.unityEnvironmentName=bad;Reject(()=>valid.Validate(config.unityProjectId),"invalid environment blocked");}
    string malformed=Path.Combine(temp,"malformed.json");File.WriteAllText(malformed,"{\"unityProjectId\":\""+config.unityProjectId+"\",\"clientSecret\":\"not-a-real-secret\"}");
    Reject(()=>AuthenticationConfiguration.Read(malformed,config.unityProjectId),"missing fields/secret injection blocked");
    File.WriteAllText(malformed,JsonConvert.SerializeObject(config).Replace("\"cloudCodeDeployedAndVerified\":false","\"cloudCodeDeployedAndVerified\":\"false\""));
    Reject(()=>AuthenticationConfiguration.Read(malformed,config.unityProjectId),"boolean strings cannot accidentally activate features");
    File.WriteAllText(malformed,JsonConvert.SerializeObject(config).Replace("\"purchaseVerificationConfigured\":false","\"purchaseVerificationConfigured\":false,\"purchaseVerificationConfigured\":true"));
    Reject(()=>AuthenticationConfiguration.Read(malformed,config.unityProjectId),"duplicate configuration fields rejected");

    Reset();using(var client=Client()){
        client.LinkAccount();Check(client.CanCancelSignIn&&!client.CanLink&&PlayerAccountService.Instance.Starts==1,"browser waits and blocks duplicate sign-in");
        string before=CloudJson.Encode(Journal(client));client.CancelSignIn();
        Check(client.CanLink&&!client.CanCancelSignIn&&client.State==CloudSyncState.Offline,"cancel unlocks UI");
        Check(before==CloudJson.Encode(Journal(client))&&AuthenticationService.Instance.PlayerId=="guest","cancel preserves guest identity/journal");
        PlayerAccountService.Instance.Succeed();PauseSync(client);client.Tick();
        Check(AuthenticationService.Instance.LinkCalls==0,"late browser success cannot link after cancellation");
        PlayerAccountService.Instance.SignOut();client.LinkAccount();Check(PlayerAccountService.Instance.Starts==2,"browser can be reopened");
        Time.realtimeSinceStartupAsDouble+=181;client.Tick();Check(client.LastFailureCode=="SignInTimeout"&&client.CanLink,"timeout unlocks retry");
        PlayerAccountService.Instance.Fail();Check(client.LastFailureCode=="SignInTimeout","late failure cannot overwrite timeout");
    }
    Reset();var initialization=new TaskCompletionSource();UnityServices.Initialize=()=>initialization.Task;
    using(var client=Client()){
        client.LinkAccount();client.CancelSignIn();initialization.SetResult();await Task.Delay(10);
        Check(PlayerAccountService.Instance.Starts==0&&!client.CanCancelSignIn,"cancel during services initialization cannot open browser later");
    }
    Reset();var delayed=new TaskCompletionSource();PlayerAccountService.Instance.Open=()=>delayed.Task;
    using(var client=Client()){
        client.LinkAccount();client.CancelSignIn();PlayerAccountService.Instance.Open=()=>Task.CompletedTask;client.LinkAccount();
        delayed.SetException(new InvalidOperationException("old browser failed"));await Task.Delay(10);
        Check(client.CanCancelSignIn&&client.State==CloudSyncState.Connecting,"old asynchronous failure cannot cancel new attempt");client.CancelSignIn();
    }
    Reset();using(var client=Client()){
        int unlocked=0;client.Changed+=()=>{if(client.CanLink)unlocked++;};client.LinkAccount();PlayerAccountService.Instance.Succeed();
        Check(!client.CanLink,"completed browser remains locked until identity link finishes");client.Tick();
        Check(client.Linked&&AuthenticationService.Instance.PlayerId=="guest"&&unlocked>0,"link preserves guest and refreshes unlocked UI");
    }
    Reset();AuthenticationService.Instance.Link=()=>throw new AuthenticationException(AuthenticationErrorCodes.AccountAlreadyLinked);
    using(var client=Client()){
        client.LinkAccount();PlayerAccountService.Instance.Succeed();client.Tick();
        Check(client.NeedsAccountChoice&&client.CanLink,"existing account requires an explicit choice and enabled button");
        string before=CloudJson.Encode(Journal(client));client.KeepLocalAccount();
        Check(AuthenticationService.Instance.ExistingCalls==0&&AuthenticationService.Instance.PlayerId=="guest","keep local does not switch identities");
        Check(CloudJson.Encode(Journal(client))==before&&!client.NeedsAccountChoice,"keep local retains durable guest journal");
    }
    Reset();using(var client=Client()){
        await client.InitializeServicesAsync();Choice(client);client.LoadExistingAccount();
        Check(client.CanCancelSignIn&&Journal(client).AccountLoadPending,"existing-account browser records replacement intent");
        client.CancelSignIn();Check(client.NeedsAccountChoice&&client.CanLink,"cancel load returns to explicit choice");client.KeepLocalAccount();
        Check(!Journal(client).AccountLoadPending&&Journal(client).AccountCandidateProfile==null,"keep local clears only replacement intent");
    }
    Reset();AuthenticationService.Instance.Link=()=>throw new AuthenticationException(AuthenticationErrorCodes.AccountAlreadyLinked);
    int applied=0;PlayerProfile incoming=null;
    CloudCodeService.Instance.Handler=(name,args)=>new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile{Monetization=new Cascade.Application.MonetizationLedger{BonusCoins=7}},Revision=2}};
    using(var client=Client((p,archive)=>{applied++;incoming=p;Check(archive,"account load archives original save");})){
        client.LinkAccount();PlayerAccountService.Instance.Succeed();client.Tick();client.LoadExistingAccount();
        Check(applied==1&&incoming.Monetization.BonusCoins==7&&AuthenticationService.Instance.PlayerId=="existing","explicit load applies only remote save");
        Check(Journal(client).PlayerId=="existing"&&!Journal(client).AccountLoadPending&&Journal(client).AuthenticationProfile!="default","loaded account gets its own durable SDK profile");
        Check(AuthenticationService.Instance.Sessions.ContainsKey(Journal(client).AuthenticationProfile),"existing account cached session retained");
    }
    Reset();AuthenticationService.Instance.Sessions["default"]="guest";
    AuthenticationService.Instance.Link=()=>throw new AuthenticationException(AuthenticationErrorCodes.AccountAlreadyLinked);
    using(var client=Client((_,__)=>throw new Exception("empty cloud must never apply"))){
        client.LinkAccount();PlayerAccountService.Instance.Succeed();client.Tick();client.LoadExistingAccount();
        Check(client.NeedsAccountChoice&&Journal(client).AccountLoadPending,"empty remote save cannot overwrite guest");client.KeepLocalAccount();
        Check(AuthenticationService.Instance.PlayerId=="guest"&&AuthenticationService.Instance.Profile=="default","failed load can recover original cached guest");
    }
    Reset();using(var client=Client()){
        client.LinkAccount();int calls=CloudCodeService.Instance.Calls;
        Check(!await client.VerifyPurchaseAsync("cascade.coins_240","t","AppleAppStore","receipt","","bundle"),"purchase verification blocked during identity change");
        try{await client.PreparePurchaseIdentityAsync();throw new Exception("Expected rejection");}catch(InvalidOperationException){Check(true,"purchase identity cannot race sign-in");}
        Check(CloudCodeService.Instance.Calls==calls,"blocked purchase cannot call backend");client.CancelSignIn();
    }
    Reset();AuthenticationService.Instance.Sessions["default"]="guest";
    AuthenticationService.Instance.Link=()=>throw new AuthenticationException(AuthenticationErrorCodes.AccountAlreadyLinked);
    var local=new PlayerProfile{Monetization=new Cascade.Application.MonetizationLedger{BonusCoins=2}};
    using(var client=Client((p,archive)=>local=CloudJson.Clone(p),()=>CloudJson.Clone(local))){
        client.LinkAccount();PlayerAccountService.Instance.Succeed();client.Tick();
        var store=typeof(CloudSyncClient).GetField("store",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(client);
        var path=typeof(CloudJournalStore).GetField("path",BindingFlags.Instance|BindingFlags.NonPublic);string originalPath=(string)path.GetValue(store);
        string blocked=Path.Combine(temp,"directory-not-journal");Directory.CreateDirectory(blocked);
        CloudCodeService.Instance.Handler=(name,args)=>{
            path.SetValue(store,blocked);return new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile{Monetization=new Cascade.Application.MonetizationLedger{BonusCoins=7}},Revision=2}};
        };
        client.LoadExistingAccount();
        Check(local.Monetization.BonusCoins==2&&client.NeedsAccountChoice,"journal I/O failure rolls profile replacement back");
        Check(Journal(client).PlayerId=="guest"&&Journal(client).AccountLoadPending,"failed journal commit retains recoverable guest identity");
        path.SetValue(store,originalPath);client.KeepLocalAccount();
        Check(AuthenticationService.Instance.PlayerId=="guest"&&!Journal(client).AccountLoadPending,"disk failure can return to original guest session");
    }
    Reset();var disposedClient=Client();disposedClient.LinkAccount();disposedClient.Dispose();PlayerAccountService.Instance.Succeed();disposedClient.Tick();
    Check(AuthenticationService.Instance.LinkCalls==0,"disposed client ignores browser completion");
    Console.WriteLine($"Account activation: {passed} PASS. Actual client/config sources; SDK transport faked. No live OAuth or device claim.");
} finally {Directory.Delete(temp,true);}
