using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cascade.Persistence;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.CloudCode;
using UnityEngine;

namespace Cascade.Presentation
{
    public enum CloudSyncState { LocalOnly,Connecting,Saving,Saved,Offline,Conflict,AccountChoice }
    public sealed class CloudSyncReply
    {
        public string Status,Error,PackedAccount,RewardId;
        public CloudAccount Account;
        public bool EconomyConflict;
        public long LastSequence;
    }
    // One request in flight per device; the server uses atomic storage compare-and-swap.
    public sealed class CloudSyncClient:IDisposable
    {
        readonly CloudPlayerSettings settings;readonly CloudJournalStore store;readonly Func<PlayerProfile> current;
        readonly Action<PlayerProfile,bool> apply;readonly IReadOnlyList<string> nodes;
        readonly ICloudPlatformAccount platformAccount;CloudPlatformProof platformProof;
        CloudJournal journal;bool disposed,busy,dirty=true,subscribed,openingSignIn,awaitingSignIn,identityReady;double next;int failures;
        Task servicesInitialization,identityInitialization;int identityPreparations,purchaseVerifications,rewardPreparations;bool signInWaitingForIdle,linkingIdentity;
        TaskCompletionSource<bool> idle;PurchaseIdentity cachedPurchaseIdentity;string purchasePlayer,purchaseProfile;
        const double SignInTimeoutSeconds=180;
        double signInDeadline;int signInGeneration;
        public CloudSyncState State {get;private set;}=CloudSyncState.LocalOnly;
        public event Action Changed;
        public CloudAccount Account {get;private set;}
        public bool CanVerifyPurchases=>Configured&&!journal.SignedOut&&!journal.DeletionPending&&settings.purchaseVerificationConfigured;
        public bool CanVerifyRewards=>Configured&&!journal.SignedOut&&!journal.DeletionPending&&settings.serverRewardsConfigured;
        // Stable, identity-free preflight diagnosis only. A normal save may be
        // awaited by reward preparation; reporting a reason never changes authority.
        public string RewardBlockReason=>disposed?"disposed":!Configured?"cloud_unconfigured":
            journal.DeletionPending?"deletion_pending":journal.SignedOut?"signed_out":
            !settings.serverRewardsConfigured?"rewards_unconfigured":
            openingSignIn||awaitingSignIn||identityReady||NeedsAccountChoice||journal.AccountLoadPending?"account_operation":
            journal.EconomyConflict?"economy_conflict":"none";
        public bool DeletionPending=>journal.DeletionPending;
        public bool NeedsLocalDeletion=>journal.LocalDeletionRequired;
        public bool EnabledForPlayer=>Configured;
        public bool CanLink=>Configured&&!journal.DeletionPending&&(platformAccount?.Available??(!CloudPlatformAccounts.RequiresNativeAccount(UnityEngine.Application.platform)&&settings?.playerAccountsConfigured==true))&&(!busy||(!NeedsAccountChoice&&!journal.AccountLoadPending))&&purchaseVerifications==0&&rewardPreparations==0&&!openingSignIn&&!awaitingSignIn&&!identityReady&&!linkingIdentity&&!disposed;
        public bool CanCancelSignIn=>!disposed&&(openingSignIn||awaitingSignIn);
        public bool AccountOperationPending=>busy||openingSignIn||awaitingSignIn||identityReady||journal.AccountLoadPending;
        public bool CompletingSignIn=>identityReady||linkingIdentity;
        public bool NativeSignInConfirmed=>platformAccount!=null&&platformProof!=null;
        public bool Linked=>Configured&&!journal.SignedOut&&!journal.AccountLoadPending&&!NeedsAccountChoice&&UnityServices.State==ServicesInitializationState.Initialized&&AuthenticationService.Instance.IsSignedIn
            &&(journal.PlayerId==null||journal.PlayerId==AuthenticationService.Instance.PlayerId)&&(platformAccount?.IsLinked(AuthenticationService.Instance.PlayerInfo)==true||!string.IsNullOrEmpty(AuthenticationService.Instance.PlayerInfo?.GetUnityId()));
        public string PlatformAccountName=>platformAccount?.DisplayName;
        public bool SignedOut=>journal.SignedOut;
        public bool CanSignOut=>Linked&&!journal.DeletionPending&&!busy&&!CanCancelSignIn&&!identityReady&&!NeedsAccountChoice&&identityPreparations==0&&purchaseVerifications==0&&!disposed;
        public bool NeedsAccountChoice=>State==CloudSyncState.AccountChoice;
        public string LastFailureCode {get;private set;}
        public string LastSyncUtc=>journal.LastSyncUtc;
        public string DeviceId=>journal.DeviceId;
        public bool LoadingAccount {get;private set;}
        public CloudSyncClient(CloudPlayerSettings settings,string journalPath,Func<PlayerProfile> current,Action<PlayerProfile,bool> apply,IReadOnlyList<string> nodes,ICloudPlatformAccount platformAccount=null)
        {this.platformAccount=platformAccount;this.settings=settings;this.current=current;this.apply=apply;this.nodes=nodes;store=new CloudJournalStore(journalPath);journal=store.Load();
            if(Configured&&!journal.SignedOut&&journal.EconomyConflict&&journal.PendingJson==null)State=CloudSyncState.Conflict;}
        bool Configured=>settings!=null&&settings.enabledForPlayers&&settings.projectId==UnityEngine.Application.cloudProjectId&&!string.IsNullOrWhiteSpace(settings.environment);
        public void Queue()
        {
            // Coalesce the first new change without shortening a successful-sync
            // cooldown or an offline backoff. Further moves cannot postpone it.
            if(!dirty&&!busy&&failures==0)next=Math.Max(next,Time.realtimeSinceStartupAsDouble+3);
            dirty=true;
        }
        public void Retry(){failures=0;next=0;dirty=true;if(State==CloudSyncState.Conflict){journal.EconomyConflict=false;store.Save(journal);}Tick();}
        public void Tick()
        {
            if(disposed)return;
            if(CanCancelSignIn&&Time.realtimeSinceStartupAsDouble>=signInDeadline){CancelSignIn("SignInTimeout");return;}
            if(signInWaitingForIdle&&!busy&&identityPreparations==0&&purchaseVerifications==0){signInWaitingForIdle=false;_=OpenSignIn(signInGeneration);return;}
            if(disposed||busy||identityPreparations>0||purchaseVerifications>0||openingSignIn||awaitingSignIn)return;
            if(identityReady){identityReady=false;_ = CompleteAccountLink();return;}
            if(journal.SignedOut||journal.DeletionPending)return;
            if(disposed||busy||(NeedsAccountChoice&&!journal.AccountLoadPending)||(journal.EconomyConflict&&journal.PendingJson==null)||!Configured||!dirty||Time.realtimeSinceStartupAsDouble<next)return;
            _=Synchronize();
        }
        public Task InitializeServicesAsync()
        {
            if(!Configured)throw new InvalidOperationException("Cloud services are not configured.");
            if(servicesInitialization==null||servicesInitialization.IsFaulted||servicesInitialization.IsCanceled)
                servicesInitialization=UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName(settings.environment)
                    .SetProfile(journal.AccountCandidateProfile??journal.AuthenticationProfile));
            return servicesInitialization;
        }
        Task EnsureIdentity()
        {
            // Share authentication work, not the slower account read/write that follows it.
            if(identityInitialization==null||identityInitialization.IsCompleted)identityInitialization=EnsureIdentityCore();
            return identityInitialization;
        }
        async Task EnsureIdentityCore()
        {
            if(journal.SignedOut&&!CanCancelSignIn)throw new InvalidOperationException("Sign in explicitly before resuming this account.");
            await InitializeServicesAsync();if(disposed)throw new ObjectDisposedException(nameof(CloudSyncClient));
            string expectedProfile=journal.AccountCandidateProfile??journal.AuthenticationProfile;
            if(AuthenticationService.Instance.Profile!=expectedProfile){
                if(AuthenticationService.Instance.IsSignedIn)throw new InvalidOperationException("Cloud SDK profile differs from the local journal.");
                AuthenticationService.Instance.SwitchProfile(expectedProfile);
            }
            if(!AuthenticationService.Instance.IsSignedIn)await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if(disposed)throw new ObjectDisposedException(nameof(CloudSyncClient));
            if(platformAccount==null&&!subscribed){PlayerAccountService.Instance.SignedIn+=PlayerSignedIn;PlayerAccountService.Instance.SignInFailed+=PlayerSignInFailed;subscribed=true;}
            string player=AuthenticationService.Instance.PlayerId;
            if(!journal.AccountLoadPending&&journal.PlayerId!=null&&(journal.PlayerId!=player||journal.ProjectId!=settings.projectId||journal.Environment!=settings.environment))
                throw new InvalidOperationException("Cloud identity differs from the local journal.");
            if(!journal.AccountLoadPending)journal.PlayerId=player;journal.ProjectId=settings.projectId;journal.Environment=settings.environment;
        }
        async Task Synchronize()
        {
            if(journal.DeletionPending)return;
            busy=true;Set(CloudSyncState.Connecting);
            try {
                await EnsureIdentity();if(disposed)return;
                if(journal.AccountLoadPending){
                    if(AuthenticationService.Instance.PlayerId==journal.PlayerId){Set(CloudSyncState.AccountChoice);next=double.MaxValue;return;}
                    await FinishAccountLoad();return;
                }
                if(journal.RequestId!=null&&current().CloudAppliedRequestId==journal.RequestId){
                    var read=await Call("ReadAccount",new Dictionary<string,object>{{"deviceId",journal.DeviceId}});if(disposed)return;
                    var applied=current();
                    if(read.Account?.Profile!=null&&CloudReplyReconciler.CanAcknowledgeApplied(applied,journal.RequestId,journal.Sequence,read.LastSequence)){
                        journal.EconomyConflict|=applied.CloudAppliedEconomyConflict;
                        Acknowledge(read);dirty=true;return;
                    }
                }
                if(Account==null){var read=await Call("ReadAccount",new Dictionary<string,object>{{"deviceId",journal.DeviceId}});Account=read.Account;
                    journal.Revision=Account?.Revision??0;
                    if(journal.RequestId==null)journal.Sequence=Math.Max(journal.Sequence,read.LastSequence);
                }
                if(journal.PendingJson==null){var snapshot=current();snapshot.Validate();journal.PendingJson=CloudProfilePayload.Pack(CloudJson.Encode(snapshot));
                    journal.RequestId=(++journal.Sequence).ToString(System.Globalization.CultureInfo.InvariantCulture)+":"+Guid.NewGuid().ToString("D");store.Save(journal);}
                Set(CloudSyncState.Saving);dirty=false;
                for(int retry=0;retry<4;retry++) {
                    var reply=await Call("SyncAccount",new Dictionary<string,object>{{"deviceId",journal.DeviceId},{"requestId",journal.RequestId},
                        {"expectedRevision",journal.Revision},{"baseProfileJson",""},{"localProfileJson",journal.PendingJson}});
                    if(disposed)return;
                    if(reply.Status=="Conflict"){Account=reply.Account;journal.Revision=Account.Revision;store.Save(journal);await Task.Delay(150+retry*170);continue;}
                    if(reply.Status=="Rejected"&&journal.RetireRejectedProgress(reply.Error)){
                        store.Save(journal);LastFailureCode=reply.Error;
                        throw new InvalidOperationException("Progress requires a verified replay.");
                    }
                    bool stale=reply.Status=="Rejected"&&reply.Error=="StaleSequence";
                    if((reply.Status!="Ok"&&!stale)||reply.Account==null)throw new InvalidOperationException("Cloud request was not accepted.");
                    var sent=CloudJson.Decode<PlayerProfile>(CloudProfilePayload.Unpack(journal.PendingJson));
                    var merged=CloudReplyReconciler.Apply(reply.Account.Profile,sent,current(),nodes,journal.Sequence,reply.LastSequence,reply.EconomyConflict||stale);
                    Account=reply.Account;journal.Revision=Account.Revision;
                    bool conflict=journal.EconomyConflict||reply.EconomyConflict||merged.EconomyConflict;
                    // Preserve rejected work once; the original atomic profile remains recoverable.
                    journal.EconomyConflict=conflict;store.Save(journal);
                    merged.Profile.CloudAppliedRequestId=journal.RequestId;merged.Profile.CloudAppliedEconomyConflict=conflict;
                    apply(merged.Profile,conflict);
                    Acknowledge(reply);return;
                }
                throw new InvalidOperationException("Cloud account is busy.");
            }catch(Exception e){if(!disposed){ReportAccountFailure("sync",e);dirty=true;failures=Math.Min(6,failures+1);next=Time.realtimeSinceStartupAsDouble+Math.Min(120,Math.Pow(2,failures)*3);Set(CloudSyncState.Offline);}}
            finally{ReleaseBusy();}
        }
        void Acknowledge(CloudSyncReply reply)
        {
            Account=reply.Account;journal.Revision=Account.Revision;journal.Sequence=Math.Max(journal.Sequence,reply.LastSequence);
            journal.BaseJson=CloudProfilePayload.Pack(CloudJson.Encode(Account.Profile));journal.PendingJson=null;journal.RequestId=null;
            journal.LastSyncUtc=DateTime.UtcNow.ToString("O");store.Save(journal);
            failures=0;LastFailureCode=null;Set(journal.EconomyConflict?CloudSyncState.Conflict:CloudSyncState.Saved);next=Time.realtimeSinceStartupAsDouble+30;
        }
        async Task<CloudSyncReply> Call(string function,Dictionary<string,object> args)
        {
            var reply=await CloudCodeService.Instance.CallModuleEndpointAsync<CloudSyncReply>(settings.moduleName,function,args);
            if(reply.Account==null&&reply.PackedAccount!=null)reply.Account=CloudJson.Decode<CloudAccount>(CloudProfilePayload.Unpack(reply.PackedAccount));
            return reply;
        }
        public void LinkAccount()
        {
            if(!CanLink||disposed)return;
            int generation=BeginSignIn();
            // The guest save owns its in-flight request until ReleaseBusy. Accept the
            // player's click now, but never link/switch an identity during that write.
            if((busy||identityPreparations>0)&&platformAccount==null){signInWaitingForIdle=true;return;}
            _=OpenSignIn(generation);
        }
        async Task OpenSignIn(int generation)
        {
            try {
                // Showing the native account controller does not change the UGS
                // identity. It need not wait for service initialization or a guest write.
                if(platformAccount!=null&&platformProof==null){
                    store.Save(journal);
                    var proof=await platformAccount.AuthenticateAsync();if(!SignInCurrent(generation))return;
                    platformProof=proof??throw new InvalidOperationException("Platform identity proof is missing.");
                    Changed?.Invoke();
                }
                // Linking still waits for the original guest write to commit. Reuse
                // this generation's proof when Tick resumes, without a second prompt.
                if(busy||identityPreparations>0||purchaseVerifications>0){signInWaitingForIdle=true;return;}
                // An explicit account replacement may retain an incomplete candidate
                // profile. Do not sign a fresh guest into it while obtaining proof.
                if(journal.AccountLoadPending)await InitializeServicesAsync();else await EnsureIdentity();if(!SignInCurrent(generation)){if(journal.SignedOut&&!CanCancelSignIn)AuthenticationService.Instance.SignOut();return;}
                store.Save(journal);
                if(platformAccount!=null)PlayerSignedIn();
                else if(PlayerAccountService.Instance.IsSignedIn)PlayerSignedIn();
                else {Set(CloudSyncState.Connecting);await PlayerAccountService.Instance.StartSignInAsync();}
            }
            catch(Exception e){if(SignInCurrent(generation)){ReportAccountFailure("open-sign-in",e);awaitingSignIn=false;LastFailureCode=e is CloudPlatformSignInException platformFailure?platformFailure.Reason:"SignInFailed";Set(journal.AccountLoadPending?CloudSyncState.AccountChoice:CloudSyncState.Offline);}}
            finally{if(generation==signInGeneration){openingSignIn=false;Changed?.Invoke();}}
        }
        int BeginSignIn()
        {
            platformProof=null;signInWaitingForIdle=false;openingSignIn=true;awaitingSignIn=true;identityReady=false;LastFailureCode=null;
            signInDeadline=Time.realtimeSinceStartupAsDouble+SignInTimeoutSeconds;
            int generation=++signInGeneration;Set(CloudSyncState.Connecting);return generation;
        }
        bool SignInCurrent(int generation)=>!disposed&&generation==signInGeneration;
        public void CancelSignIn()=>CancelSignIn("SignInCancelled");
        void CancelSignIn(string reason)
        {
            if(!CanCancelSignIn)return;
            ++signInGeneration;platformProof=null;signInWaitingForIdle=false;openingSignIn=false;awaitingSignIn=false;identityReady=false;
            // Signing out the browser SDK also rejects later deep links while its state is SignedOut.
            // The guest UGS session and every durable journal field are left intact.
            if(subscribed)PlayerAccountService.Instance.SignOut();
            if(journal.SignedOut&&UnityServices.State==ServicesInitializationState.Initialized)AuthenticationService.Instance.SignOut();
            LastFailureCode=reason;Set(journal.AccountLoadPending?CloudSyncState.AccountChoice:CloudSyncState.Offline);
        }
        void PlayerSignInFailed(Unity.Services.Core.RequestFailedException failure)
        {
            if(disposed||!CanCancelSignIn)return;
            ReportAccountFailure("browser-sign-in",failure);
            awaitingSignIn=false;identityReady=false;LastFailureCode="SignInFailed";
            if(journal.SignedOut&&UnityServices.State==ServicesInitializationState.Initialized)AuthenticationService.Instance.SignOut();
            Set(journal.AccountLoadPending?CloudSyncState.AccountChoice:CloudSyncState.Offline);
        }
        void PlayerSignedIn()
        {
            if(disposed||!awaitingSignIn)return;
            awaitingSignIn=false;identityReady=true;
        }
        async Task CompleteAccountLink()
        {
            if(disposed||busy)return;
            if(journal.AccountLoadPending){Set(CloudSyncState.AccountChoice);LoadExistingAccount();return;}
            busy=true;linkingIdentity=true;
            try{if(platformAccount!=null){var proof=platformProof??throw new InvalidOperationException("Platform identity proof is missing.");platformProof=null;if(!proof.MatchesLinkedPlayer(AuthenticationService.Instance.PlayerInfo))await proof.LinkAsync();}else await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);if(disposed)return;ResumeSignedInJournal();LastFailureCode=null;Set(CloudSyncState.Connecting);dirty=true;next=0;}
            catch(AuthenticationException e)when(e.ErrorCode==AuthenticationErrorCodes.AccountAlreadyLinked||e.ErrorCode==AuthenticationErrorCodes.AccountLinkLimitExceeded){Set(CloudSyncState.AccountChoice);}
            catch(Exception e){ReportAccountFailure("link-identity",e);if(journal.SignedOut)AuthenticationService.Instance.SignOut();LastFailureCode="SignInFailed";Set(CloudSyncState.Offline);}
            finally{linkingIdentity=false;ReleaseBusy();}
        }
        public async void KeepLocalAccount()
        {
            if(!NeedsAccountChoice||busy||disposed)return;
            ++signInGeneration;openingSignIn=false;awaitingSignIn=false;identityReady=false;
            busy=true;
            try{
                var auth=AuthenticationService.Instance;
                if(auth.Profile!=journal.AuthenticationProfile){
                    auth.SignOut(true);auth.SwitchProfile(journal.AuthenticationProfile);
                    await auth.SignInAnonymouslyAsync();if(disposed)return;
                }
                journal.AccountLoadPending=false;journal.AccountCandidateProfile=null;store.Save(journal);
                identityReady=false;awaitingSignIn=false;platformProof=null;if(platformAccount==null)PlayerAccountService.Instance.SignOut();if(journal.SignedOut)auth.SignOut();
                dirty=true;next=0;Set(CloudSyncState.Offline);
            }catch(Exception){Set(CloudSyncState.AccountChoice);}finally{ReleaseBusy();}
        }
        public void ManageAccount()
        {
            if(disposed||!Linked)return;
            if(platformAccount!=null)_=ShowPlatformAccount();else UnityEngine.Application.OpenURL(PlayerAccountService.Instance.AccountPortalUrl);
        }
        async Task ShowPlatformAccount(){try{await platformAccount.ShowAccountAsync();}catch(Exception e){ReportAccountFailure("show-platform-account",e);}}
        void ResumeSignedInJournal()
        {
            if(!journal.SignedOut)return;
            journal.SignedOut=false;
            try{store.Save(journal);}catch{journal.SignedOut=true;throw;}
        }
        public bool SignOut()
        {
            if(!CanSignOut)return false;
            // Commit the device's explicit choice before touching SDK sessions. Pending
            // cloud requests and the cached identity remain recoverable for a later login.
            journal.SignedOut=true;
            try{store.Save(journal);}catch{journal.SignedOut=false;return false;}
            ++signInGeneration;identityReady=false;openingSignIn=false;awaitingSignIn=false;
            AuthenticationService.Instance.SignOut();if(platformAccount==null)PlayerAccountService.Instance.SignOut();platformProof=null;
            LastFailureCode=null;Set(CloudSyncState.LocalOnly);return true;
        }
        // Only invoked by the explicit Load account action. Never merges one signed-in account into another.
        async Task FinishAccountLoad()
        {
            var nextJournal=new CloudJournal{PlayerId=AuthenticationService.Instance.PlayerId,ProjectId=settings.projectId,Environment=settings.environment,
                AuthenticationProfile=AuthenticationService.Instance.Profile};
            var read=await Call("ReadAccount",new Dictionary<string,object>{{"deviceId",nextJournal.DeviceId}});if(disposed)return;
            if(read.Account?.Profile==null)throw new InvalidOperationException("Account has no cloud save.");
            nextJournal.Revision=read.Account.Revision;
            nextJournal.BaseJson=CloudProfilePayload.Pack(CloudJson.Encode(read.Account.Profile));
            var original=CloudJson.Clone(current());
            LoadingAccount=true;
            try{
                apply(read.Account.Profile,true);
                // Keep the recoverable guest journal in memory until the new identity is durable.
                store.Save(nextJournal);
            }catch{
                // A disk error after profile replacement must not leave the guest UI on another account's save.
                apply(original,false);throw;
            }finally{LoadingAccount=false;}
            journal=nextJournal;Account=read.Account;dirty=true;next=0;Set(CloudSyncState.Saved);
        }
        public async void LoadExistingAccount()
        {
            if(!NeedsAccountChoice||busy||disposed||openingSignIn||awaitingSignIn)return;
            journal.AccountLoadPending=true;store.Save(journal);
            if(platformAccount!=null?platformProof==null:!PlayerAccountService.Instance.IsSignedIn){
                int generation=BeginSignIn();
                try{await OpenSignIn(generation);}
                catch(Exception){if(SignInCurrent(generation)){awaitingSignIn=false;LastFailureCode="SignInFailed";Set(CloudSyncState.AccountChoice);}}
                finally{if(generation==signInGeneration){openingSignIn=false;Changed?.Invoke();}}return;
            }
            busy=true;
            try {
                // Keep the guest's cached session in its own SDK profile until the
                // player accepts and a real existing cloud save has been loaded.
                if(journal.AccountCandidateProfile==null){journal.AccountCandidateProfile="cascade-"+Guid.NewGuid().ToString("N").Substring(0,16);store.Save(journal);}
                AuthenticationService.Instance.SignOut();
                AuthenticationService.Instance.SwitchProfile(journal.AccountCandidateProfile);
                if(platformAccount!=null){var proof=platformProof;platformProof=null;await proof.SignInAsync();}else await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                if(disposed)return;
                await FinishAccountLoad();
            }catch(Exception e){ReportAccountFailure("load-account",e);Set(CloudSyncState.AccountChoice);}finally{ReleaseBusy();}
        }
        bool PurchaseAccountBlocked=>disposed||journal.SignedOut||openingSignIn||awaitingSignIn||identityReady||NeedsAccountChoice||journal.AccountLoadPending||!CanVerifyPurchases;
        public async Task<PurchaseIdentity> PreparePurchaseIdentityAsync()
        {
            if(PurchaseAccountBlocked)throw new InvalidOperationException("Finish the account operation before starting a purchase.");
            // The server binding is stable for this authenticated player/profile. Reuse it
            // at the buy tap, including while an ordinary background save is in flight.
            if(cachedPurchaseIdentity!=null&&UnityServices.State==ServicesInitializationState.Initialized){
                var active=AuthenticationService.Instance;
                if(active.IsSignedIn&&active.PlayerId==purchasePlayer&&active.PlayerId==journal.PlayerId&&active.Profile==purchaseProfile
                    &&active.Profile==journal.AuthenticationProfile&&journal.ProjectId==settings.projectId&&journal.Environment==settings.environment)
                    return cachedPurchaseIdentity;
            }
            identityPreparations++;
            try{
                await EnsureIdentity();if(PurchaseAccountBlocked)throw new InvalidOperationException("Purchase account is not ready.");
                var auth=AuthenticationService.Instance;string player=auth.PlayerId,profile=auth.Profile;
                // The server endpoint only hashes this authenticated context. Derive
                // exactly the same IDs locally so an ordinary save/cold Cloud Code
                // call cannot delay StoreKit/Play Billing. Verification stays after pay.
                var binding=PurchaseIdentity.FromAuthenticatedSession(settings.projectId,player,auth.AccessToken)
                    ??await CloudCodeService.Instance.CallModuleEndpointAsync<PurchaseIdentity>(settings.moduleName,"PurchaseIdentity",new Dictionary<string,object>());
                if(PurchaseAccountBlocked||!auth.IsSignedIn||auth.PlayerId!=player||auth.Profile!=profile)
                    throw new InvalidOperationException("Purchase account changed during preparation.");
                cachedPurchaseIdentity=binding;purchasePlayer=player;purchaseProfile=profile;return binding;
            }finally{identityPreparations--;}
        }
        async Task WaitForIdle()
        {
            while(busy&&!disposed){if(idle==null)idle=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);await idle.Task;}
        }
        public async Task<bool> VerifyPurchaseAsync(string productId,string transactionId,string platform,string receipt,string jws,string bundleId)
        {
            if(PurchaseAccountBlocked||journal.EconomyConflict)return false;
            purchaseVerifications++;
            try{
                // A completed payment must wait for the current save, rather than fail
                // locally and remain uncredited until the next store refresh.
                await WaitForIdle();if(PurchaseAccountBlocked||journal.EconomyConflict)return false;
                // A dirty unsent local snapshot does not need a separate round trip
                // before a verified grant. Merge it into the authoritative reply below.
                if(Account==null||journal.PendingJson!=null)await Synchronize();
                if(PurchaseAccountBlocked||journal.PendingJson!=null||journal.EconomyConflict||State!=CloudSyncState.Saved)return false;
                busy=true;
                try {
                    await EnsureIdentity();
                    var baseline=CloudJson.Clone(Account.Profile);
                    var reply=await Call("VerifyPurchase",new Dictionary<string,object>{{"productId",productId},{"transactionId",transactionId},{"platform",platform},
                        {"receipt",receipt??""},{"jws",jws??""},{"bundleId",bundleId},{"deviceId",journal.DeviceId}});
                    if(disposed||reply.Status!="Ok"||reply.Account==null)return false;
                    var merged=CloudReconciler.Merge(reply.Account.Profile,baseline,current(),nodes,true);
                    apply(merged.Profile,merged.EconomyConflict);
                    Account=reply.Account;journal.Revision=Account.Revision;journal.BaseJson=CloudProfilePayload.Pack(CloudJson.Encode(Account.Profile));
                    journal.EconomyConflict=merged.EconomyConflict;store.Save(journal);dirty=true;next=0;Set(merged.EconomyConflict?CloudSyncState.Conflict:CloudSyncState.Saved);return true;
                }catch(Exception){return false;}finally{ReleaseBusy();}
            }finally{purchaseVerifications--;}
        }
        [Serializable] public sealed class ServerRewardIntent { public string Id,UserId; }
        [Serializable] public sealed class AccountDeletion { public string Status,RequestId,Code; public long ExpiresUnix; }
        bool RewardAccountBlocked=>disposed||journal.SignedOut||openingSignIn||awaitingSignIn||identityReady||NeedsAccountChoice||journal.AccountLoadPending||!CanVerifyRewards;
        public async Task<ServerRewardIntent> CreateRewardIntentAsync(string kind,string subject,string unit)
        {
            if(RewardAccountBlocked||journal.EconomyConflict)return null;
            identityPreparations++;rewardPreparations++;
            try{
                await WaitForIdle();if(RewardAccountBlocked||journal.EconomyConflict)return null;
                if(Account==null||journal.PendingJson!=null)await Synchronize();
                if(RewardAccountBlocked||State!=CloudSyncState.Saved||journal.PendingJson!=null||journal.EconomyConflict)return null;
                busy=true;
                try{await EnsureIdentity();return await CloudCodeService.Instance.CallModuleEndpointAsync<ServerRewardIntent>(settings.moduleName,"CreateRewardIntent",new Dictionary<string,object>{{"kind",kind},{"subject",subject??""},{"unit",unit}});}
                catch(Exception){return null;}finally{ReleaseBusy();}
            }finally{identityPreparations--;rewardPreparations--;if(!disposed)Changed?.Invoke();}
        }
        public async Task<bool> RefreshRewardAsync(string intent)
        {
            if(RewardAccountBlocked||Account==null)return false;
            identityPreparations++;rewardPreparations++;
            try{
                await WaitForIdle();if(RewardAccountBlocked)return false;
                if(journal.PendingJson!=null)await Synchronize();
                if(RewardAccountBlocked||journal.PendingJson!=null||Account==null)return false;
                busy=true;
                try{
                    await EnsureIdentity();var baseline=CloudJson.Clone(Account.Profile);
                    var reply=await Call("ReadReward",new Dictionary<string,object>{{"intentId",intent},{"deviceId",journal.DeviceId}});
                    if(disposed||reply.Status!="Ok"||reply.Account==null)return false;
                    var merged=CloudReconciler.Merge(reply.Account.Profile,baseline,current(),nodes,true);apply(merged.Profile,merged.EconomyConflict);
                    Account=reply.Account;journal.Revision=Account.Revision;journal.BaseJson=CloudProfilePayload.Pack(CloudJson.Encode(Account.Profile));journal.EconomyConflict=merged.EconomyConflict;store.Save(journal);dirty=true;next=0;Set(merged.EconomyConflict?CloudSyncState.Conflict:CloudSyncState.Saved);
                    return !string.IsNullOrEmpty(reply.RewardId)&&Account.Profile.Monetization.Grants.Exists(g=>g.Id==reply.RewardId);
                }catch(Exception){return false;}finally{ReleaseBusy();}
            }finally{identityPreparations--;rewardPreparations--;if(!disposed)Changed?.Invoke();}
        }
        public async Task<AccountDeletion> GetDeletionCodeAsync(){
            if(disposed||!Linked||busy||purchaseVerifications>0||identityPreparations>0||journal.DeletionPending)return null;busy=true;
            try{await EnsureIdentity();return await CloudCodeService.Instance.CallModuleEndpointAsync<AccountDeletion>(settings.moduleName,"GetDeletionCode",new Dictionary<string,object>());}catch(Exception){return null;}finally{ReleaseBusy();}
        }
        public async Task<bool> DeleteAccountAsync(){
            if(disposed||busy||purchaseVerifications>0||identityPreparations>0||(!Linked&&!journal.DeletionPending))return false;busy=true;
            try{
                // Durable before the request: a lost HTTP response must never resume uploading this account.
                journal.DeletionPending=true;store.Save(journal);await EnsureIdentity();
                var result=await CloudCodeService.Instance.CallModuleEndpointAsync<AccountDeletion>(settings.moduleName,"DeleteAccount",new Dictionary<string,object>{{"confirmation","DELETE"}});
                if(result?.Status is not ("DeletionPending" or "Complete"))return false;
                journal=new CloudJournal{SignedOut=true,LocalDeletionRequired=true,AuthenticationProfile=journal.AuthenticationProfile};store.Save(journal);Account=null;dirty=false;
                AuthenticationService.Instance.SignOut(true);if(platformAccount==null)PlayerAccountService.Instance.SignOut();platformProof=null;Set(CloudSyncState.LocalOnly);return true;
            }catch(Exception){LastFailureCode="DeletionPending";Set(CloudSyncState.Offline);return false;}finally{ReleaseBusy();}
        }
        static void ReportAccountFailure(string operation,Exception failure)
        {
            long code=failure is Unity.Services.Core.RequestFailedException request?request.ErrorCode:failure is CloudPlatformSignInException platformFailure?platformFailure.NativeCode:0;
            // Deliberately omit messages, URLs, tokens and account identifiers.
            Debug.LogWarning($"[CascadeAccount] {operation} failed: {failure.GetType().Name}; SDK code {code}.");
        }
        void ReleaseBusy(){busy=false;var waiting=idle;idle=null;waiting?.TrySetResult(true);if(!disposed)Changed?.Invoke();}
        void Set(CloudSyncState value){if(disposed)return;State=value;Changed?.Invoke();}
        public void Dispose(){disposed=true;idle?.TrySetResult(true);++signInGeneration;if(subscribed){PlayerAccountService.Instance.SignedIn-=PlayerSignedIn;PlayerAccountService.Instance.SignInFailed-=PlayerSignInFailed;}Changed=null;}
    }
}
