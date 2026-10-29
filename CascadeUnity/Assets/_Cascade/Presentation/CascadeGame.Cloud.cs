using System;
using System.IO;
using System.Linq;
using Cascade.Persistence;
using Cascade.Application;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        CloudSyncClient cloudSave;
        public CloudSyncState CloudState=>cloudSave?.State??CloudSyncState.LocalOnly;
        void InitializeCloudSave()
        {
#if UNITY_EDITOR
            if(!string.IsNullOrEmpty(EditorProfileDirectory))return; // Isolated captures never contact production players.
#endif
#if CASCADE_STORE_CAPTURE
            return;
#endif
            if(saveBlocked)return;
            var settings=Resources.Load<CloudPlayerSettings>("CascadeArt/CloudPlayerSettings");
            cloudSave=new CloudSyncClient(settings,ProfilePath+".cloud",()=>profile.ProgressSnapshot(),ApplyCloudProfile,OrderedNodes,CloudPlatformAccounts.Create());
            if(cloudSave.NeedsLocalDeletion){EraseDeletedAccountData();return;}
            cloudSave.Changed+=CloudStatusChanged;
            if(CurrentScreen==GameScreen.AccountGate)DrawMenu();
        }
        void CloudStatusChanged()
        {
            if(disabling)return;
            if(CurrentScreen==GameScreen.AccountGate&&cloudSave?.Linked==true&&!cloudSave.AccountOperationPending&&CloudState==CloudSyncState.Saved){
                bool seen=profile.AccountIntroSeen;profile.AccountIntroSeen=true;PersistProgress(true);
                if(HasSaveError){profile.AccountIntroSeen=seen;return;}
                accountNotice=default;ShowHome();return;
            }
            if(CurrentScreen==GameScreen.Home)RefreshHomeAccountShortcut();
            // Signed rewards may arrive through ordinary sync after the ad's
            // immediate callback window. Repaint its committed wallet/hint state.
            if(CurrentScreen==GameScreen.Playing)NotifyStoreChanged();
            if((CurrentScreen==GameScreen.Settings&&settingsPage==4)||CurrentScreen==GameScreen.AccountGate)DrawMenu();
        }
        UiCopy PlatformSignInWaitingNote()=>cloudSave?.PlatformAccountName!=null?ReferenceCopy("Finish the system sign-in, or cancel here.","Завершите системный вход или отмените его здесь.","Завершіть системний вхід або скасуйте його тут."):(UiCopy)"Finish signing in in your browser, or cancel here.";
        UiCopy PlatformSignInLabel()=>cloudSave?.PlatformAccountName=="Game Center"?ReferenceCopy("Sign in with Game Center","Войти через Game Center","Увійти через Game Center"):
            cloudSave?.PlatformAccountName=="Google Play Games"?ReferenceCopy("Sign in with Play Games","Войти через Play Games","Увійти через Play Games"):(UiCopy)"Sign in with Apple or Google";
        void QueueCloudSave()=>cloudSave?.Queue();
        void ApplyCloudProfile(PlayerProfile incoming,bool archive)
        {
            if(saveBlocked||disabling)throw new InvalidOperationException("Local profile is not ready for cloud application.");
            incoming.Validate();
            if(archive)new ProfileStore(ProfilePath+".before-cloud",persistSessionAttempts:false).Save(profile);
            // OS permissions, language, quality and current gesture remain owned by this device.
            incoming.Preferences=Preferences;
            if(cloudSave.LoadingAccount){incoming.SelectJourney(profile.PreferredMode);incoming.DiscardSessionAttempts();}
            if(!cloudSave.LoadingAccount){
                string appliedRequest=incoming.CloudAppliedRequestId;
                bool appliedConflict=incoming.CloudAppliedEconomyConflict;
                var merged=CloudReconciler.Merge(incoming,profile,profile,OrderedNodes,true);
                incoming=merged.Profile;incoming.CloudAppliedRequestId=appliedRequest;incoming.CloudAppliedEconomyConflict=appliedConflict;
            }
            else {incoming.CloudAppliedRequestId=null;incoming.CloudAppliedEconomyConflict=false;}
            incoming.Validate();profileStore.Save(incoming);ApplyProfileFields(incoming);
            sessionClears.Clear();sessionClears.UnionWith(profile.Clears.Select(c=>c.CampaignNodeId));
            heardStory.Clear();heardStory.UnionWith(profile.HeardStory);
            GameEconomy?.ReloadLedger(profile.Monetization);
            if(cloudSave.LoadingAccount){
                StopPresentation();CancelHint();
                ResetSessionRound();
                firstGestureShown=profile.FirstGestureShown;
                if(CurrentScreen!=GameScreen.AccountGate)ShowHome();
            }
            if(CurrentScreen!=GameScreen.Playing&&CurrentScreen!=GameScreen.Pause)DrawMenu();
        }
        bool confirmAccountDeletion,localDeletionRetry;
        UiCopy deletionNotice;
        public bool UsesServerRewards=>!TestMonetization&&cloudSave?.EnabledForPlayer==true;
        public string ServerRewardBlockReason=>cloudSave?.RewardBlockReason??"local_rewards";
        public System.Threading.Tasks.Task<CloudSyncClient.ServerRewardIntent> PrepareServerReward(AdTicket ticket,string unit)=>cloudSave.CreateRewardIntentAsync(ticket.RewardKind,ticket.RewardSubject,unit.Substring(unit.LastIndexOf('/')+1));
        public System.Threading.Tasks.Task<bool> RefreshServerReward(string intent)=>cloudSave.RefreshRewardAsync(intent);
        async void RequestDeletionCode(){
            var result=await cloudSave.GetDeletionCodeAsync();if(disabling)return;
            if(result==null){deletionNotice="Could not connect. Try again.";DrawMenu();return;}
            GUIUtility.systemCopyBuffer=result.Code;deletionNotice="Deletion code copied. It is valid for 10 minutes.";
            if(Uri.TryCreate(Resources.Load<CloudPlayerSettings>("CascadeArt/CloudPlayerSettings")?.accountDeletionUrl,UriKind.Absolute,out var url)&&url.Scheme=="https")UnityEngine.Application.OpenURL(url.ToString());DrawMenu();
        }
        async void DeleteGameAccount(){
            if(!confirmAccountDeletion)return;
            bool accepted=await cloudSave.DeleteAccountAsync();if(disabling)return;
            if(!accepted){deletionNotice="Deletion is pending. Retry to finish; cloud uploads are stopped.";DrawMenu();return;}
            EraseDeletedAccountData();
        }
        void EraseDeletedAccountData(){
            StopPresentation();CancelHint();DisposeCloudSave();cloudSave=null;
            try{
                profileStore.Flush();persistenceReady=false;saveBlocked=true;
                // Write a clean profile before removing recovery files. The durable cloud marker stays until cleanup succeeds.
                var fresh=new PlayerProfile{Preferences=Preferences};fresh.Validate();profileStore.Save(fresh);
                foreach(string suffix in new[]{".bak",".tmp",".before-cloud",".before-cloud.bak",".before-cloud.tmp",".cloud.bak",".cloud.tmp",".cloud.unreadable",".cloud"}){
                    string path=ProfilePath+suffix;if(File.Exists(path))File.Delete(path);
                }
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }catch(Exception e)when(e is IOException||e is UnauthorizedAccessException){
                persistenceReady=false;saveBlocked=true;localDeletionRetry=true;
                deletionNotice="Could not clear local data. Retry to finish deletion.";if(!disabling)DrawMenu();
            }
        }
        void DrawCloudSettings()
        {
            UiCopy note=CloudState==CloudSyncState.LocalOnly?"Saved on this phone":cloudSave?.CanCancelSignIn==true?PlatformSignInWaitingNote():
                cloudSave?.LastFailureCode=="SignInTimeout"?"Sign-in timed out. Your save is safe. Try again.":
                cloudSave?.LastFailureCode=="SignInFailed"?"Sign-in failed. Your save is safe. Try again.":CloudState==CloudSyncState.Connecting?"Connecting to your save...":
                CloudState==CloudSyncState.Saving?"Saving your progress...":cloudSave?.LastFailureCode=="UnverifiedProgress"?"Saved here. Older level results need verification before cloud transfer.":CloudState==CloudSyncState.Offline?"Saved on this device. We will retry online.":
                CloudState==CloudSyncState.Conflict?"Progress saved. Items restored from your account.":CloudState==CloudSyncState.AccountChoice?"This account already has a save. Load it?":"Your progress is saved in the cloud.";
            SettingsNote("Cloud status",note);
            if(localDeletionRetry){SettingsNote("Delete account",deletionNotice);SettingsButton("Finish deleting local data",EraseDeletedAccountData);return;}
            // No cloud project is configured yet. Two inert buttons read as broken
            // controls; keep the local-save status visible without offering them.
            if(CloudState==CloudSyncState.LocalOnly)return;
            if(cloudSave?.CanCancelSignIn==true){SettingsButton("Cancel sign-in",()=>cloudSave.CancelSignIn());return;}
            var sync=SettingsButton("Save now",()=>{PersistProgress();cloudSave?.Retry();});sync.interactable=cloudSave!=null&&!cloudSave.NeedsAccountChoice;
            bool purchaseActive=purchaseStore!=null&&(purchaseStore.State==PurchaseState.Busy);
            var link=SettingsButton(cloudSave?.NeedsAccountChoice==true?(UiCopy)"Load account":cloudSave?.Linked==true?"Manage account":PlatformSignInLabel(),
                ()=>{if(purchaseActive)return;PersistProgress(true);if(HasSaveError)return;if(cloudSave?.NeedsAccountChoice==true)cloudSave.LoadExistingAccount();else if(cloudSave?.Linked==true)cloudSave.ManageAccount();else cloudSave?.LinkAccount();});
            link.GetComponent<Button>().interactable=cloudSave?.CanLink==true&&!purchaseActive;
            if(cloudSave?.NeedsAccountChoice==true)SettingsButton("Keep this phone's save",()=>cloudSave.KeepLocalAccount());
            SettingsNote("Account help","Apple or Google protects your progress and verified purchases.");
            if(cloudSave?.Linked==true||cloudSave?.DeletionPending==true){
                if(!string.IsNullOrEmpty(deletionNotice.Key))SettingsNote("Delete account",deletionNotice);
                if(cloudSave.Linked)SettingsButton("Get deletion code",RequestDeletionCode);
                SettingsButton(confirmAccountDeletion?"Confirm permanent deletion":"Delete game account",()=>{if(confirmAccountDeletion)DeleteGameAccount();else{confirmAccountDeletion=true;DrawMenu();}});
                if(confirmAccountDeletion){SettingsNote("Delete account","This permanently deletes game data. Cancel subscriptions separately in the store.");SettingsButton("Cancel",()=>{confirmAccountDeletion=false;DrawMenu();});}
            }
        }
        void DrawAccountGate()
        {
            DrawReferenceAccountGate();
        }
        void BeginAccountGateSignIn()
        {
            BeginReferenceAccountSignIn("Unity");
        }
        void SkipAccountGate()
        {
            if(cloudSave?.CanCancelSignIn==true)cloudSave.CancelSignIn();
            bool seen=profile.AccountIntroSeen;profile.AccountIntroSeen=true;PersistProgress(true);
            if(HasSaveError){profile.AccountIntroSeen=seen;return;}
            accountNotice=default;ShowHome();
        }
        void ApplyProfileFields(PlayerProfile incoming)
        {
            foreach(var field in typeof(PlayerProfile).GetFields(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public))field.SetValue(profile,field.GetValue(incoming));
        }
        void DisposeCloudSave(){if(cloudSave!=null){cloudSave.Changed-=CloudStatusChanged;cloudSave.Dispose();}}
    }
}
