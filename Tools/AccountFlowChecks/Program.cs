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
    var liveConfig=AuthenticationConfiguration.Read(Path.Combine(root,"Configs/authentication.json"),UnityEngine.Application.cloudProjectId);
    Check(liveConfig.unityProjectId==UnityEngine.Application.cloudProjectId,"current public configuration accepted");
    var config=new AuthenticationConfiguration{unityProjectId=UnityEngine.Application.cloudProjectId,unityPlayerAccountsClientId="",unityEnvironmentName="production",cloudCodeModuleName="CascadeAccount"};
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

    var metadata=Newtonsoft.Json.Linq.JObject.FromObject(config);
    metadata["unityPlayerAccountsDisplayName"]="Flying Ladders";metadata["privacyPolicyUrl"]="https://example.com/privacy/";metadata["termsOfUseUrl"]="https://example.com/terms/";
    File.WriteAllText(malformed,metadata.ToString());Check(AuthenticationConfiguration.Read(malformed,config.unityProjectId).unityPlayerAccountsDisplayName=="Flying Ladders","public sign-in metadata accepted");
    foreach(var key in new[]{"unityPlayerAccountsDisplayName","privacyPolicyUrl","termsOfUseUrl"}){
        var bad=(Newtonsoft.Json.Linq.JObject)metadata.DeepClone();bad[key]=true;File.WriteAllText(malformed,bad.ToString());Reject(()=>AuthenticationConfiguration.Read(malformed,config.unityProjectId),"metadata must be strings");
    }
    foreach(string address in new[]{"http://example.com/privacy","https://user@example.com/privacy","https://example.com/privacy?token=x"}){
        var bad=(Newtonsoft.Json.Linq.JObject)metadata.DeepClone();bad["privacyPolicyUrl"]=address;File.WriteAllText(malformed,bad.ToString());Reject(()=>AuthenticationConfiguration.Read(malformed,config.unityProjectId),"unsafe policy URL blocked");
    }
    metadata["clientSecret"]="not-a-real-secret";File.WriteAllText(malformed,metadata.ToString());Reject(()=>AuthenticationConfiguration.Read(malformed,config.unityProjectId),"metadata still rejects secret fields");

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
    Reset();var backgroundRead=new TaskCompletionSource<object>();
    CloudCodeService.Instance.AsyncHandler=(name,args)=>backgroundRead.Task;
    using(var client=Client()){
        client.Tick();Check(client.AccountOperationPending,"background sync is in flight");
        Check(client.CanLink,"background save must not reject an explicit sign-in click");
        client.LinkAccount();Check(client.CanCancelSignIn&&PlayerAccountService.Instance.Starts==0,"sign-in waits for active sync without opening a concurrent identity operation");
        client.CancelSignIn();backgroundRead.SetException(new IOException("offline test"));await Task.Delay(10);client.Tick();
        Check(PlayerAccountService.Instance.Starts==0&&client.CanLink,"cancelled queued sign-in never opens later");
    }
    Reset();var pendingRead=new TaskCompletionSource<object>();
    CloudCodeService.Instance.AsyncHandler=(name,args)=>pendingRead.Task;
    using(var client=Client()){
        client.Tick();client.LinkAccount();pendingRead.SetException(new IOException("offline test"));await Task.Delay(10);client.Tick();
        Check(PlayerAccountService.Instance.Starts==1&&client.CanCancelSignIn,"queued sign-in opens after background sync releases even when save failed");
        client.Tick();Check(PlayerAccountService.Instance.Starts==1,"queued sign-in opens exactly once");client.CancelSignIn();
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
    Reset();string signedOutPath=Path.Combine(temp,"signed-out","journal.json");
    var preserved=new PlayerProfile{Monetization=new Cascade.Application.MonetizationLedger{BonusCoins=31}};
    var signedOutSettings=Settings();signedOutSettings.serverRewardsConfigured=true;
    using(var client=new CloudSyncClient(signedOutSettings,signedOutPath,()=>CloudJson.Clone(preserved),(_,__)=>throw new Exception("Sign out cannot replace local progress"),new[]{"C01"})){
        client.LinkAccount();PlayerAccountService.Instance.Succeed();client.Tick();
        Check(client.Linked&&client.CanSignOut,"completed identity can sign out");
        var j=Journal(client);j.Sequence=1;j.RequestId="1:"+Guid.NewGuid().ToString("D");j.PendingJson=CloudProfilePayload.Pack(CloudJson.Encode(preserved));
        string pending=j.PendingJson,request=j.RequestId,device=j.DeviceId,player=j.PlayerId,authProfile=j.AuthenticationProfile;
        Check(client.SignOut()&&!client.Linked&&client.SignedOut&&client.State==CloudSyncState.LocalOnly,"explicit sign-out stops account access");
        Check(!AuthenticationService.Instance.IsSignedIn&&!PlayerAccountService.Instance.IsSignedIn,"both SDK sessions sign out without unlinking");
        Check(j.PendingJson==pending&&j.RequestId==request&&j.DeviceId==device&&preserved.Monetization.BonusCoins==31,"sign-out preserves pending request and local wallet");
        Check(new CloudJournalStore(signedOutPath).Load().SignedOut,"sign-out choice is durable");
        int calls=CloudCodeService.Instance.Calls;Time.realtimeSinceStartupAsDouble+=1000;client.Queue();client.Retry();client.Tick();
        Check(CloudCodeService.Instance.Calls==calls&&!AuthenticationService.Instance.IsSignedIn,"queued/retried work cannot silently reconnect signed-out account");
        Check(!client.CanVerifyPurchases&&!await client.VerifyPurchaseAsync("cascade.coins_240","t","AppleAppStore","receipt","","bundle"),"signed-out identity cannot verify purchases");
        string signedOutJournal=CloudJson.Encode(j),signedOutDisk=File.ReadAllText(signedOutPath);
        int signedOutAnonymousCalls=AuthenticationService.Instance.AnonymousCalls;
        Check(!client.CanVerifyRewards&&client.RewardBlockReason=="signed_out","signed-out reward gate is diagnosed without exposing account identity");
        Check(await client.CreateRewardIntentAsync("hint","","unit")==null&&!await client.RefreshRewardAsync("intent-test"),"signed-out reward preparation and polling remain blocked");
        Check(CloudCodeService.Instance.Calls==calls&&AuthenticationService.Instance.AnonymousCalls==signedOutAnonymousCalls&&!AuthenticationService.Instance.IsSignedIn,"blocked rewards cannot contact the backend or silently authenticate");
        Check(CloudJson.Encode(j)==signedOutJournal&&File.ReadAllText(signedOutPath)==signedOutDisk&&preserved.Monetization.BonusCoins==31,"reward diagnosis preserves the durable journal and local wallet");
        try{await client.PreparePurchaseIdentityAsync();throw new Exception("Expected explicit sign-in");}catch(InvalidOperationException){Check(true,"signed-out purchase preparation requires explicit sign-in");}
        client.LinkAccount();client.CancelSignIn();Check(client.SignedOut&&!client.Linked&&!AuthenticationService.Instance.IsSignedIn,"cancelled re-login keeps explicit sign-out");
        PlayerAccountService.Instance.Succeed();client.Tick();Check(client.SignedOut,"late browser callback cannot undo sign-out");
        client.LinkAccount();PlayerAccountService.Instance.Succeed();client.Tick();Check(client.Linked&&!client.SignedOut,"explicit successful re-login resumes the same journal");
        Check(j.PendingJson==pending&&j.RequestId==request,"re-login does not mint a new pending request");
        Check(client.CanVerifyRewards&&client.RewardBlockReason=="none"&&AuthenticationService.Instance.PlayerId==player&&j.PlayerId==player&&j.DeviceId==device&&j.AuthenticationProfile==authProfile,"explicit successful re-login restores reward eligibility on the original identity and device");
        Check(client.SignOut(),"signed-in session can sign out again");
    }
    using(var reloaded=new CloudSyncClient(Settings(),signedOutPath,()=>CloudJson.Clone(preserved),(_,__)=>{},new[]{"C01"})){
        int calls=CloudCodeService.Instance.Calls;reloaded.Tick();Check(reloaded.SignedOut&&!reloaded.Linked&&CloudCodeService.Instance.Calls==calls,"restart respects durable sign-out");
    }
    Reset();using(var client=Client()){
        client.LinkAccount();PlayerAccountService.Instance.Succeed();client.Tick();
        var journalStore=typeof(CloudSyncClient).GetField("store",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(client);
        var pathField=typeof(CloudJournalStore).GetField("path",BindingFlags.Instance|BindingFlags.NonPublic);
        string original=(string)pathField.GetValue(journalStore),blocked=Path.Combine(temp,"signout-blocked");Directory.CreateDirectory(blocked);pathField.SetValue(journalStore,blocked);
        Check(!client.SignOut()&&client.Linked&&!client.SignedOut&&AuthenticationService.Instance.IsSignedIn,"failed durable sign-out leaves original session usable");
        pathField.SetValue(journalStore,original);
    }
    Reset();using(var client=Client()){
        client.LinkAccount();Check(!client.SignOut(),"sign-out cannot race browser sign-in");client.CancelSignIn();
    }
    Reset();using(var client=Client()){
        client.LinkAccount();PlayerAccountService.Instance.Succeed();client.Tick();var identity=new TaskCompletionSource<object>();
        CloudCodeService.Instance.AsyncHandler=(name,args)=>identity.Task;
        var preparation=client.PreparePurchaseIdentityAsync();Check(!client.CanSignOut&&!client.SignOut(),"sign-out cannot race purchase identity preparation");
        identity.SetResult(new PurchaseIdentity());await preparation;Check(client.CanSignOut,"purchase preparation releases sign-out guard");
    }
    Reset();var platformLink=new TaskCompletionSource();var linkingPlatform=new FakePlatformAccount{Link=()=>platformLink.Task};
    using(var client=new CloudSyncClient(Settings(),Path.Combine(temp,Guid.NewGuid().ToString("N"),"journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"},linkingPlatform)){
        client.LinkAccount();client.Tick();Check(client.CompletingSignIn&&!client.CanLink&&client.AccountOperationPending,"linking the native identity stays one pending sign-in without duplicate clicks");
        client.LinkAccount();Check(linkingPlatform.AuthCalls==1&&linkingPlatform.LinkCalls==1,"duplicate sign-in cannot restart a native link");
        platformLink.SetResult();await Task.Delay(10);Check(client.Linked&&!client.CompletingSignIn,"confirmed native linking exits the sign-in stage");
    }
    Reset();var coldServices=new TaskCompletionSource();UnityServices.Initialize=()=>coldServices.Task;
    var immediatePlatform=new FakePlatformAccount();
    using(var client=new CloudSyncClient(Settings(),Path.Combine(temp,"native-cold-start","journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"},immediatePlatform)){
        client.LinkAccount();
        Check(immediatePlatform.AuthCalls==1,"native system sign-in opens before slow Unity services initialization");
        Check(immediatePlatform.LinkCalls==0&&client.CanCancelSignIn,"cloud identity waits for services while native proof is ready");
        coldServices.SetResult();await Task.Delay(10);client.Tick();PauseSync(client);
        Check(immediatePlatform.LinkCalls==1&&client.Linked,"native proof links once after service readiness");
    }
    Reset();var guestWrite=new TaskCompletionSource<object>();CloudCodeService.Instance.AsyncHandler=(name,args)=>guestWrite.Task;
    var overlappingPlatform=new FakePlatformAccount();
    using(var client=new CloudSyncClient(Settings(),Path.Combine(temp,"native-during-guest-save","journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"},overlappingPlatform)){
        client.Tick();client.LinkAccount();
        Check(overlappingPlatform.AuthCalls==1,"native system sign-in opens while guest cloud save is in flight");
        Check(overlappingPlatform.LinkCalls==0,"native proof cannot change identity during the guest write");
        guestWrite.SetResult(new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=1}});await Task.Delay(10);client.Tick();PauseSync(client);client.Tick();
        Check(overlappingPlatform.AuthCalls==1&&overlappingPlatform.LinkCalls==1&&client.Linked,"guest save completion reuses one proof and links only once");
    }
    Reset();var cancelledServices=new TaskCompletionSource();UnityServices.Initialize=()=>cancelledServices.Task;
    var cancelledPlatform=new FakePlatformAccount();
    using(var client=new CloudSyncClient(Settings(),Path.Combine(temp,"native-cancel-cold-start","journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"},cancelledPlatform)){
        client.LinkAccount();client.CancelSignIn();cancelledServices.SetResult();await Task.Delay(10);PauseSync(client);client.Tick();
        Check(cancelledPlatform.LinkCalls==0&&!client.Linked&&client.CanLink,"cancelling native login during cloud preparation ignores late completion");
    }
    Reset();var binding=PurchaseIdentity.For("project","production","guest");
    CloudCodeService.Instance.Handler=(name,args)=>name=="PurchaseIdentity"?binding:throw new IOException("Offline test");
    using(var client=Client()){
        var first=await client.PreparePurchaseIdentityAsync();int calls=CloudCodeService.Instance.Calls;
        Check(client.PreparePurchaseIdentityAsync().IsCompletedSuccessfully&&ReferenceEquals(first,await client.PreparePurchaseIdentityAsync()),"ready purchase binding returns synchronously at buy tap");
        Check(CloudCodeService.Instance.Calls==calls,"repeat buy preparation avoids a second network request");
        var write=new TaskCompletionSource<object>();CloudCodeService.Instance.AsyncHandler=(name,args)=>write.Task;client.Tick();
        Check(client.PreparePurchaseIdentityAsync().IsCompletedSuccessfully&&CloudCodeService.Instance.Calls==calls+1,"cached binding stays immediate during same-player background sync");
        write.SetResult(new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=1}});await Task.Delay(10);PauseSync(client);
        AuthenticationService.Instance.PlayerId="other-player";
        try{await client.PreparePurchaseIdentityAsync();throw new Exception("Expected account mismatch");}catch(InvalidOperationException){Check(true,"cached purchase binding cannot cross player identities");}
    }
    Reset();var paidProfile=new PlayerProfile();var grantedProfile=new PlayerProfile();var grantService=new Cascade.Application.AdService(grantedProfile.Monetization,_=>{});grantService.VerifiedStorePurchase(Cascade.Application.PurchaseCatalog.Coins240,"store_test_timing",true);grantedProfile.Monetization=grantService.Ledger;var saveInFlight=new TaskCompletionSource<object>();int receiptCalls=0;
    CloudCodeService.Instance.AsyncHandler=(name,args)=>{
        if(name=="ReadAccount")return Task.FromResult<object>(new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=CloudJson.Clone(paidProfile),Revision=1}});
        if(name=="SyncAccount")return saveInFlight.Task;
        if(name=="VerifyPurchase"){receiptCalls++;return Task.FromResult<object>(new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=grantedProfile,Revision=2}});}
        throw new Exception("Unexpected request: "+name);
    };
    using(var client=Client((p,_)=>paidProfile=CloudJson.Clone(p),()=>CloudJson.Clone(paidProfile))){
        client.Tick();var verify=client.VerifyPurchaseAsync("cascade.coins_240","paid-while-saving","AppleAppStore","receipt","","bundle");
        Check(!verify.IsCompleted&&receiptCalls==0,"paid receipt waits for an in-flight save instead of failing locally");
        Check(!client.CanLink&&!client.CanSignOut,"queued receipt prevents an account change");
        saveInFlight.SetResult(new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=1}});
        bool verified=await verify;Check(verified&&receiptCalls==1&&paidProfile.Monetization.BonusCoins==240,"receipt credits the current wallet directly after the save completes");
    }
    Reset();var abandonedSave=new TaskCompletionSource<object>();CloudCodeService.Instance.AsyncHandler=(name,args)=>abandonedSave.Task;
    var abandonedClient=Client();abandonedClient.Tick();var abandonedReceipt=abandonedClient.VerifyPurchaseAsync("cascade.coins_240","pending","AppleAppStore","receipt","","bundle");
    abandonedClient.Dispose();Check(!await abandonedReceipt,"disposing the scene releases a queued receipt without granting");abandonedSave.SetResult(new CloudSyncReply());
    Reset();int scheduledWrites=0;
    CloudCodeService.Instance.Handler=(name,args)=>{
        if(name=="SyncAccount")scheduledWrites++;
        return new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=scheduledWrites+1}};
    };
    using(var client=Client()){
        client.Tick();Check(scheduledWrites==1,"initial account warms immediately");
        client.Queue();Time.realtimeSinceStartupAsDouble+=3.1;client.Tick();
        Check(scheduledWrites==1,"new progress preserves the successful-sync cooldown");
        Time.realtimeSinceStartupAsDouble+=30;client.Tick();
        Check(scheduledWrites==2,"new progress is synchronized after the cooldown expires");
    }
    Reset();var unsentLocal=new PlayerProfile();int redundantWrites=0,fastReceipts=0;
    CloudCodeService.Instance.Handler=(name,args)=>{
        if(name=="SyncAccount")redundantWrites++;
        if(name=="VerifyPurchase"){fastReceipts++;return new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=CloudJson.Clone(grantedProfile),Revision=3}};}
        return new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=1}};
    };
    using(var client=Client((p,_)=>unsentLocal=CloudJson.Clone(p),()=>CloudJson.Clone(unsentLocal))){
        client.Tick();unsentLocal.FirstGestureShown=true;client.Queue();
        Check(await client.VerifyPurchaseAsync("cascade.coins_240","dirty-not-sent","AppleAppStore","receipt","","bundle"),"verified receipt is accepted while local progress is only queued");
        Check(fastReceipts==1&&redundantWrites==1,"receipt does not pay for a redundant pre-verification upload");
        Check(unsentLocal.FirstGestureShown&&unsentLocal.Monetization.BonusCoins==240,"fast receipt retains unsent progress and applies authoritative coins");
    }
    Reset();var rewardSettings=Settings();rewardSettings.serverRewardsConfigured=true;
    var unconfiguredRewards=new CloudSyncClient(null,Path.Combine(temp,"reward-unconfigured","journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"});
    Check(unconfiguredRewards.RewardBlockReason=="cloud_unconfigured","missing cloud configuration has a stable reward diagnostic");
    unconfiguredRewards.Dispose();Check(unconfiguredRewards.RewardBlockReason=="disposed","disposed reward diagnostics do not inspect an SDK session");
    using(var client=new CloudSyncClient(rewardSettings,Path.Combine(temp,"reward-reasons","journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"})){
        Check(client.RewardBlockReason=="none","fresh guest is eligible before its ordinary save completes");
        rewardSettings.serverRewardsConfigured=false;
        Check(client.RewardBlockReason=="rewards_unconfigured","disabled server rewards have a distinct diagnostic");
        rewardSettings.serverRewardsConfigured=true;Journal(client).EconomyConflict=true;
        Check(client.RewardBlockReason=="economy_conflict"&&await client.CreateRewardIntentAsync("hint","","unit")==null,"unresolved economy conflict remains reward-blocking");
        Journal(client).EconomyConflict=false;client.LinkAccount();
        int calls=CloudCodeService.Instance.Calls;
        Check(client.RewardBlockReason=="account_operation"&&await client.CreateRewardIntentAsync("hint","","unit")==null&&CloudCodeService.Instance.Calls==calls,"pending sign-in diagnostic does not start a reward request");
        client.CancelSignIn();
        Check(client.RewardBlockReason=="none","cancelled fresh-guest sign-in removes only its transient reward block");
    }
    Reset();
    var rewardSave=new TaskCompletionSource<object>();int rewardIntents=0;
    CloudCodeService.Instance.AsyncHandler=(name,args)=>{
        if(name=="ReadAccount")return Task.FromResult<object>(new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=1}});
        if(name=="SyncAccount")return rewardSave.Task;
        if(name=="CreateRewardIntent"){rewardIntents++;return Task.FromResult<object>(new CloudSyncClient.ServerRewardIntent{Id="intent-test",UserId="player-test"});}
        throw new Exception(name);
    };
    using(var client=new CloudSyncClient(rewardSettings,Path.Combine(temp,"reward-waits","journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"})){
        client.Tick();var intent=client.CreateRewardIntentAsync("coins","","unit");
        Check(!intent.IsCompleted&&rewardIntents==0,"reward preparation waits for the active save instead of silently failing");
        Check(client.RewardBlockReason=="none"&&AuthenticationService.Instance.AnonymousCalls==1&&PlayerAccountService.Instance.Starts==0,"fresh anonymous guest can prepare SSV without a linked-account prompt");
        Check(!client.CanLink,"identity cannot change while a reward intent is queued");
        rewardSave.SetResult(new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=2}});
        Check((await intent)?.Id=="intent-test"&&rewardIntents==1,"reward intent starts directly after the save releases");
    }
    Reset();var rewardLocal=new PlayerProfile();var rewardInFlight=new TaskCompletionSource<object>();int rewardReads=0;
    CloudCodeService.Instance.AsyncHandler=(name,args)=>{
        if(name=="SyncAccount"&&rewardReads==-1)return rewardInFlight.Task;
        if(name=="ReadReward"){rewardReads=1;return Task.FromResult<object>(new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=4}});}
        return Task.FromResult<object>(new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=2}});
    };
    using(var client=new CloudSyncClient(rewardSettings,Path.Combine(temp,"reward-read-waits","journal.json"),()=>CloudJson.Clone(rewardLocal),(p,_)=>rewardLocal=CloudJson.Clone(p),new[]{"C01"})){
        client.Tick();rewardReads=-1;client.Retry();var reward=client.RefreshRewardAsync("intent-test");
        Check(!reward.IsCompleted,"reward polling waits for an in-flight account save");
        rewardInFlight.SetResult(new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=3}});
        Check(!await reward&&rewardReads==1,"missing verified SSV grant cannot be presented as credited");
        Check(rewardLocal.Monetization.BonusCoins==0,"waiting for SSV never invents a local reward");
        var rewardedProfile=new PlayerProfile();var rewardedService=new Cascade.Application.AdService(rewardedProfile.Monetization,_=>{});
        var verifiedTicket=rewardedService.TryRewarded(new Cascade.Application.AdContext{Surface=Cascade.Application.AdSurface.Inventory,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true},"coins",true);
        rewardedService.Opened(verifiedTicket.Id);rewardedService.RewardConfirmed(verifiedTicket.Id,"server_reward_test",true);rewardedProfile.Monetization=rewardedService.Ledger;
        CloudCodeService.Instance.AsyncHandler=(name,args)=>Task.FromResult<object>(new CloudSyncReply{Status="Ok",RewardId="server_reward_test",Account=new CloudAccount{Profile=rewardedProfile,Revision=5}});
        Check(await client.RefreshRewardAsync("intent-test")&&rewardLocal.Monetization.BonusCoins==40,"verified server reward is immediately merged into the current wallet");
        Check(await client.RefreshRewardAsync("intent-test")&&rewardLocal.Monetization.BonusCoins==40,"repeated SSV reads cannot duplicate coins");

    }
    var authSignal=new CloudPlatformAuthenticationSignal();authSignal.Authenticated(false);
    Check(!authSignal.Completion.IsCompleted,"showing the Game Center system window is not authentication completion");
    authSignal.Authenticated(true);Check(await authSignal.Completion==0,"confirmed Game Center callback completes authentication");
    var cancelledSignal=new CloudPlatformAuthenticationSignal();cancelledSignal.Authenticated(false);cancelledSignal.Rejected(2);
    Check(await cancelledSignal.Completion==2,"system cancellation resolves the pending callback without a fake success");
    foreach(string reason in new[]{"SignInCancelled","GameCenterSignInRequired","GameCenterUnavailable"}){
        Reset();var rejectedPlatform=new FakePlatformAccount{Authenticate=()=>Task.FromException<CloudPlatformProof>(new CloudPlatformSignInException(reason,2))};
        using(var client=new CloudSyncClient(Settings(),Path.Combine(temp,Guid.NewGuid().ToString("N"),"journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"},rejectedPlatform)){
            client.LinkAccount();Check(client.LastFailureCode==reason&&client.CanLink&&!client.CanCancelSignIn,"native cancellation/recovery reason remains actionable: "+reason);
            Check(AuthenticationService.Instance.PlayerId=="guest"&&!client.Linked,"native cancellation/recovery preserves the guest: "+reason);
        }
    }
    Reset();var guestLogin=new TaskCompletionSource();AuthenticationService.Instance.Anonymous=()=>guestLogin.Task;
    CloudCodeService.Instance.Handler=(name,args)=>name=="PurchaseIdentity"?new PurchaseIdentity():throw new IOException("Offline test");
    using(var client=Client()){
        var preparation=client.PreparePurchaseIdentityAsync();client.Tick();
        Check(AuthenticationService.Instance.AnonymousCalls==1&&CloudCodeService.Instance.Calls==0,"startup sync cannot race the store identity login");
        guestLogin.SetResult();await preparation;client.Tick();
        Check(AuthenticationService.Instance.AnonymousCalls==1&&CloudCodeService.Instance.Calls==2,"sync resumes on the same guest after purchase identity is ready");
    }
    Reset();var delayedInitialization=new TaskCompletionSource();UnityServices.Initialize=()=>delayedInitialization.Task;
    using(var client=new CloudSyncClient(Settings(),signedOutPath,()=>CloudJson.Clone(preserved),(_,__)=>{},new[]{"C01"})){
        client.LinkAccount();client.CancelSignIn();delayedInitialization.SetResult();await Task.Delay(10);
        Check(client.SignedOut&&!AuthenticationService.Instance.IsSignedIn&&PlayerAccountService.Instance.Starts==0,"late initialization cannot reconnect after signed-out login cancellation");
    }
    Reset();AuthenticationService.Instance.Link=()=>throw new AuthenticationException(AuthenticationErrorCodes.AccountAlreadyLinked);
    using(var client=Client()){
        client.LinkAccount();PlayerAccountService.Instance.Succeed();client.Tick();
        var remoteRead=new TaskCompletionSource<object>();CloudCodeService.Instance.AsyncHandler=(name,args)=>remoteRead.Task;
        client.LoadExistingAccount();Check(client.AccountOperationPending&&!client.Linked,"browser identity alone cannot expose a replacement account before its save loads");
        Check(client.NeedsAccountChoice&&!client.CanSignOut,"pending account replacement keeps recovery controls authoritative");
        remoteRead.SetResult(new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=1}});await Task.Delay(10);
        Check(client.Linked&&!client.AccountOperationPending&&!client.NeedsAccountChoice,"account becomes ready only after profile and journal commit");
    }
    Reset();using(var client=Client()){
        client.LinkAccount();PlayerAccountService.Instance.Succeed();client.Tick();PauseSync(client);
        CloudCodeService.Instance.Handler=(name,args)=>name=="DeleteAccount"?throw new IOException("Lost response after request"):throw new Exception("Unexpected request: "+name);
        Check(!await client.DeleteAccountAsync()&&client.DeletionPending,"lost deletion response persists an upload-blocking marker");
        int calls=CloudCodeService.Instance.Calls;client.Queue();client.Tick();
        Check(CloudCodeService.Instance.Calls==calls&&!client.CanVerifyPurchases&&!client.CanLink&&!client.CanSignOut,"deleting account cannot sync, buy, link or sign out into a new identity");
        string deletionJournal=CloudJson.Encode(Journal(client));int deletionAnonymousCalls=AuthenticationService.Instance.AnonymousCalls;
        Check(client.RewardBlockReason=="deletion_pending"&&await client.CreateRewardIntentAsync("hint","","unit")==null&&!await client.RefreshRewardAsync("intent-test"),"pending deletion has an explicit reward block and cannot create or poll an intent");
        Check(CloudCodeService.Instance.Calls==calls&&AuthenticationService.Instance.AnonymousCalls==deletionAnonymousCalls&&CloudJson.Encode(Journal(client))==deletionJournal,"deletion reward diagnosis makes no calls and preserves the marker");
        CloudCodeService.Instance.Handler=(name,args)=>new CloudSyncClient.AccountDeletion{Status="DeletionPending"};
        Check(await client.DeleteAccountAsync()&&client.SignedOut&&!AuthenticationService.Instance.IsSignedIn&&!client.DeletionPending&&client.NeedsLocalDeletion,"retry completes accepted deletion and clears the local SDK session");
    }
    Reset();var native=new FakePlatformAccount();var nativeSettings=Settings();nativeSettings.playerAccountsConfigured=false;
    using(var client=new CloudSyncClient(nativeSettings,Path.Combine(temp,"native","journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"},native)){
        Check(client.CanLink&&client.PlatformAccountName=="Game Center","native sign-in does not depend on a browser client");
        client.LinkAccount();Check(PlayerAccountService.Instance.Starts==0&&native.AuthCalls==1,"native platform does not open Unity browser");client.Tick();
        Check(client.Linked&&native.LinkCalls==1&&AuthenticationService.Instance.PlayerId=="guest","native link preserves guest identity");
        Check(client.CanSignOut&&client.SignOut()&&!client.Linked,"native session signs out of game without changing device Game Center");
        Check(PlayerAccountService.Instance.SignOuts==0,"native sign-out does not use browser SDK");
        native.Link=()=>throw new AuthenticationException(AuthenticationErrorCodes.AccountAlreadyLinked);
        client.LinkAccount();client.Tick();
        Check(client.Linked&&!client.NeedsAccountChoice&&native.LinkCalls==1,"returning to the same verified native identity does not link it twice");

    }
    Reset();native=new FakePlatformAccount();native.Link=()=>throw new AuthenticationException(AuthenticationErrorCodes.AccountAlreadyLinked);
    CloudCodeService.Instance.Handler=(name,args)=>new CloudSyncReply{Status="Ok",Account=new CloudAccount{Profile=new PlayerProfile(),Revision=2}};
    using(var client=new CloudSyncClient(Settings(),Path.Combine(temp,"native-existing","journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"},native)){
        client.LinkAccount();client.Tick();Check(client.NeedsAccountChoice&&!client.Linked,"native occupied identity requires explicit account choice");
        Journal(client).AccountCandidateProfile="previous-incomplete-load";
        native.Authenticate=()=>throw new InvalidOperationException("Native sign-in cancelled");client.LoadExistingAccount();
        Check(client.NeedsAccountChoice&&client.CanLink&&AuthenticationService.Instance.PlayerId=="guest","failed native account replacement keeps guest and explicit recovery controls");
        native.Authenticate=null;client.LoadExistingAccount();client.Tick();
        Check(native.AuthCalls==3&&native.SignInCalls==1&&client.Linked&&Journal(client).PlayerId=="existing","native account load refreshes one-time proof and commits remote identity");
        Check(PlayerAccountService.Instance.Starts==0,"native account replacement never opens browser");
    }
    Reset();native=new FakePlatformAccount();native.Link=()=>throw new AuthenticationException(AuthenticationErrorCodes.AccountLinkLimitExceeded);
    using(var client=new CloudSyncClient(Settings(),Path.Combine(temp,"native-switch","journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"},native)){
        client.LinkAccount();client.Tick();Check(client.NeedsAccountChoice,"a different native account requires an explicit save choice");
        Check(AuthenticationService.Instance.PlayerId=="guest"&&AuthenticationService.Instance.ExistingCalls==0,"native account change cannot force-switch identity or merge saves");
    }
    Reset();native=new FakePlatformAccount();var delayedNative=new TaskCompletionSource<CloudPlatformProof>();
    native.Authenticate=()=>delayedNative.Task;
    using(var client=new CloudSyncClient(Settings(),Path.Combine(temp,"native-cancel","journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"},native)){
        client.LinkAccount();client.CancelSignIn();native.Authenticate=null;delayedNative.SetResult(await native.AuthenticateAsync());await Task.Delay(10);PauseSync(client);client.Tick();
        Check(native.LinkCalls==0&&!client.Linked&&client.CanLink,"late native callback cannot link after cancellation");
    }
    Reset();var disposedClient=Client();disposedClient.LinkAccount();disposedClient.Dispose();PlayerAccountService.Instance.Succeed();disposedClient.Tick();
    Check(AuthenticationService.Instance.LinkCalls==0,"disposed client ignores browser completion");
    Console.WriteLine($"Account activation: {passed} PASS. Actual client/config sources; SDK transport faked. No live OAuth or device claim.");
} finally {Directory.Delete(temp,true);}
