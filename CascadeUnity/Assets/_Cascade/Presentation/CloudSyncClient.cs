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
        public string Status,Error,PackedAccount;
        public CloudAccount Account;
        public bool EconomyConflict;
        public long LastSequence;
    }
    // One request in flight per device; servers serialize accounts through a Cloud Save write lock.
    public sealed class CloudSyncClient:IDisposable
    {
        readonly CloudPlayerSettings settings;readonly CloudJournalStore store;readonly Func<PlayerProfile> current;
        readonly Action<PlayerProfile,bool> apply;readonly IReadOnlyList<string> nodes;
        CloudJournal journal;bool disposed,busy,dirty=true,subscribed,openingSignIn,awaitingSignIn,identityReady;double next;int failures;
        Task servicesInitialization;int identityPreparations;
        const double SignInTimeoutSeconds=180;
        double signInDeadline;int signInGeneration;
        public CloudSyncState State {get;private set;}=CloudSyncState.LocalOnly;
        public event Action Changed;
        public CloudAccount Account {get;private set;}
        public bool CanVerifyPurchases=>Configured&&!journal.SignedOut&&settings.purchaseVerificationConfigured;
        public bool EnabledForPlayer=>Configured;
        public bool CanLink=>Configured&&settings?.playerAccountsConfigured==true&&!busy&&!openingSignIn&&!awaitingSignIn&&!identityReady&&!disposed;
        public bool CanCancelSignIn=>!disposed&&(openingSignIn||awaitingSignIn);
        public bool AccountOperationPending=>busy||openingSignIn||awaitingSignIn||identityReady||journal.AccountLoadPending;
        public bool Linked=>Configured&&!journal.SignedOut&&!journal.AccountLoadPending&&!NeedsAccountChoice&&UnityServices.State==ServicesInitializationState.Initialized&&AuthenticationService.Instance.IsSignedIn
            &&(journal.PlayerId==null||journal.PlayerId==AuthenticationService.Instance.PlayerId)&&!string.IsNullOrEmpty(AuthenticationService.Instance.PlayerInfo?.GetUnityId());
        public bool SignedOut=>journal.SignedOut;
        public bool CanSignOut=>Linked&&!busy&&!CanCancelSignIn&&!identityReady&&!NeedsAccountChoice&&identityPreparations==0&&!disposed;
        public bool NeedsAccountChoice=>State==CloudSyncState.AccountChoice;
        public string LastFailureCode {get;private set;}
        public string LastSyncUtc=>journal.LastSyncUtc;
        public string DeviceId=>journal.DeviceId;
        public bool LoadingAccount {get;private set;}
        public CloudSyncClient(CloudPlayerSettings settings,string journalPath,Func<PlayerProfile> current,Action<PlayerProfile,bool> apply,IReadOnlyList<string> nodes)
        {this.settings=settings;this.current=current;this.apply=apply;this.nodes=nodes;store=new CloudJournalStore(journalPath);journal=store.Load();
            if(Configured&&!journal.SignedOut&&journal.EconomyConflict&&journal.PendingJson==null)State=CloudSyncState.Conflict;}
        bool Configured=>settings!=null&&settings.enabledForPlayers&&settings.projectId==UnityEngine.Application.cloudProjectId&&!string.IsNullOrWhiteSpace(settings.environment);
        public void Queue(){dirty=true;if(!busy)next=Math.Max(next,Time.realtimeSinceStartupAsDouble+3);}
        public void Retry(){failures=0;next=0;dirty=true;if(State==CloudSyncState.Conflict){journal.EconomyConflict=false;store.Save(journal);}Tick();}
        public void Tick()
        {
            if(CanCancelSignIn&&Time.realtimeSinceStartupAsDouble>=signInDeadline){CancelSignIn("SignInTimeout");return;}
            if(disposed||busy||openingSignIn||awaitingSignIn)return;
            if(identityReady){identityReady=false;_ = CompleteAccountLink();return;}
            if(journal.SignedOut)return;
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
        async Task EnsureIdentity()
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
            if(!subscribed){PlayerAccountService.Instance.SignedIn+=PlayerSignedIn;PlayerAccountService.Instance.SignInFailed+=PlayerSignInFailed;subscribed=true;}
            string player=AuthenticationService.Instance.PlayerId;
            if(!journal.AccountLoadPending&&journal.PlayerId!=null&&(journal.PlayerId!=player||journal.ProjectId!=settings.projectId||journal.Environment!=settings.environment))
                throw new InvalidOperationException("Cloud identity differs from the local journal.");
            if(!journal.AccountLoadPending)journal.PlayerId=player;journal.ProjectId=settings.projectId;journal.Environment=settings.environment;
        }
        async Task Synchronize()
        {
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
            }catch(Exception){if(!disposed){dirty=true;failures=Math.Min(6,failures+1);next=Time.realtimeSinceStartupAsDouble+Math.Min(120,Math.Pow(2,failures)*3);Set(CloudSyncState.Offline);}}
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
        public async void LinkAccount()
        {
            if(!CanLink||disposed)return;
            int generation=BeginSignIn();
            try {
                await EnsureIdentity();if(!SignInCurrent(generation)){if(journal.SignedOut&&!CanCancelSignIn)AuthenticationService.Instance.SignOut();return;}
                // Keep the guest snapshot durable before handing control to a browser.
                store.Save(journal);
                if(PlayerAccountService.Instance.IsSignedIn)PlayerSignedIn();
                else {Set(CloudSyncState.Connecting);await PlayerAccountService.Instance.StartSignInAsync();}
            }
            catch(Exception){if(SignInCurrent(generation)){awaitingSignIn=false;LastFailureCode="SignInFailed";Set(CloudSyncState.Offline);}}
            finally{if(generation==signInGeneration){openingSignIn=false;Changed?.Invoke();}}
        }
        int BeginSignIn()
        {
            openingSignIn=true;awaitingSignIn=true;identityReady=false;LastFailureCode=null;
            signInDeadline=Time.realtimeSinceStartupAsDouble+SignInTimeoutSeconds;
            int generation=++signInGeneration;Set(CloudSyncState.Connecting);return generation;
        }
        bool SignInCurrent(int generation)=>!disposed&&generation==signInGeneration;
        public void CancelSignIn()=>CancelSignIn("SignInCancelled");
        void CancelSignIn(string reason)
        {
            if(!CanCancelSignIn)return;
            ++signInGeneration;openingSignIn=false;awaitingSignIn=false;identityReady=false;
            // Signing out the browser SDK also rejects later deep links while its state is SignedOut.
            // The guest UGS session and every durable journal field are left intact.
            if(subscribed)PlayerAccountService.Instance.SignOut();
            if(journal.SignedOut&&UnityServices.State==ServicesInitializationState.Initialized)AuthenticationService.Instance.SignOut();
            LastFailureCode=reason;Set(journal.AccountLoadPending?CloudSyncState.AccountChoice:CloudSyncState.Offline);
        }
        void PlayerSignInFailed(Unity.Services.Core.RequestFailedException failure)
        {
            if(disposed||!CanCancelSignIn)return;
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
            busy=true;
            try{await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);if(disposed)return;ResumeSignedInJournal();LastFailureCode=null;Set(CloudSyncState.Connecting);dirty=true;next=0;}
            catch(AuthenticationException e)when(e.ErrorCode==AuthenticationErrorCodes.AccountAlreadyLinked){Set(CloudSyncState.AccountChoice);}
            catch(Exception){if(journal.SignedOut)AuthenticationService.Instance.SignOut();LastFailureCode="SignInFailed";Set(CloudSyncState.Offline);}
            finally{ReleaseBusy();}
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
                identityReady=false;awaitingSignIn=false;PlayerAccountService.Instance.SignOut();if(journal.SignedOut)auth.SignOut();
                dirty=true;next=0;Set(CloudSyncState.Offline);
            }catch(Exception){Set(CloudSyncState.AccountChoice);}finally{ReleaseBusy();}
        }
        public void ManageAccount()
        {
            if(!disposed&&Linked)UnityEngine.Application.OpenURL(PlayerAccountService.Instance.AccountPortalUrl);
        }
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
            AuthenticationService.Instance.SignOut();PlayerAccountService.Instance.SignOut();
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
            if(!PlayerAccountService.Instance.IsSignedIn){
                int generation=BeginSignIn();
                try{await PlayerAccountService.Instance.StartSignInAsync();}
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
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                if(disposed)return;
                await FinishAccountLoad();
            }catch(Exception){Set(CloudSyncState.AccountChoice);}finally{ReleaseBusy();}
        }
        public async Task<PurchaseIdentity> PreparePurchaseIdentityAsync()
        {
            if(disposed||journal.SignedOut||busy||openingSignIn||awaitingSignIn||identityReady||NeedsAccountChoice||journal.AccountLoadPending)
                throw new InvalidOperationException("Finish the account operation before starting a purchase.");
            identityPreparations++;
            try{
                await EnsureIdentity();if(disposed)throw new ObjectDisposedException(nameof(CloudSyncClient));
                return await CloudCodeService.Instance.CallModuleEndpointAsync<PurchaseIdentity>(settings.moduleName,"PurchaseIdentity",new Dictionary<string,object>());
            }finally{identityPreparations--;}
        }
        public async Task<bool> VerifyPurchaseAsync(string productId,string transactionId,string platform,string receipt,string jws,string bundleId)
        {
            if(disposed||busy||openingSignIn||awaitingSignIn||identityReady||NeedsAccountChoice||journal.AccountLoadPending||!CanVerifyPurchases||journal.EconomyConflict)return false;
            if(dirty||journal.PendingJson!=null)await Synchronize();
            if(journal.PendingJson!=null||journal.EconomyConflict||State!=CloudSyncState.Saved)return false;
            busy=true;
            try {
                await EnsureIdentity();
                // Preserve unsent spending/consumption while the server verifies a store receipt.
                var baseline=CloudJson.Clone(Account.Profile);
                var reply=await Call("VerifyPurchase",new Dictionary<string,object>{{"productId",productId},{"transactionId",transactionId},{"platform",platform},
                    {"receipt",receipt??""},{"jws",jws??""},{"bundleId",bundleId},{"deviceId",journal.DeviceId}});
                if(disposed||reply.Status!="Ok"||reply.Account==null)return false;
                var merged=CloudReconciler.Merge(reply.Account.Profile,baseline,current(),nodes,true);
                apply(merged.Profile,merged.EconomyConflict);
                Account=reply.Account;journal.Revision=Account.Revision;journal.BaseJson=CloudProfilePayload.Pack(CloudJson.Encode(Account.Profile));
                journal.EconomyConflict=merged.EconomyConflict;store.Save(journal);dirty=true;next=0;Set(merged.EconomyConflict?CloudSyncState.Conflict:CloudSyncState.Saved);return true;
            }catch(Exception){return false;}finally{ReleaseBusy();}
        }
        void ReleaseBusy(){busy=false;if(!disposed)Changed?.Invoke();}
        void Set(CloudSyncState value){if(disposed)return;State=value;Changed?.Invoke();}
        public void Dispose(){disposed=true;++signInGeneration;if(subscribed){PlayerAccountService.Instance.SignedIn-=PlayerSignedIn;PlayerAccountService.Instance.SignInFailed-=PlayerSignInFailed;}Changed=null;}
    }
}
