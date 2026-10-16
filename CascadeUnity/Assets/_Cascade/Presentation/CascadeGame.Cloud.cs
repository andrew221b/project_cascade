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
            if(saveBlocked)return;
            var settings=Resources.Load<CloudPlayerSettings>("CascadeArt/CloudPlayerSettings");
            cloudSave=new CloudSyncClient(settings,ProfilePath+".cloud",()=>CloudJson.Clone(profile),ApplyCloudProfile,OrderedNodes);
            cloudSave.Changed+=CloudStatusChanged;
            if(CurrentScreen==GameScreen.AccountGate)DrawMenu();
        }
        void CloudStatusChanged()
        {
            if(disabling)return;
            if(CurrentScreen==GameScreen.AccountGate&&cloudSave?.Linked==true&&!cloudSave.AccountOperationPending&&CloudState!=CloudSyncState.Offline){
                bool seen=profile.AccountIntroSeen;profile.AccountIntroSeen=true;PersistProgress(true);
                if(HasSaveError){profile.AccountIntroSeen=seen;return;}
                accountNotice=default;ShowHome();return;
            }
            if(CurrentScreen==GameScreen.Home)RefreshHomeAccountShortcut();
            if((CurrentScreen==GameScreen.Settings&&settingsPage==4)||CurrentScreen==GameScreen.AccountGate)DrawMenu();
        }
        void QueueCloudSave()=>cloudSave?.Queue();
        void ApplyCloudProfile(PlayerProfile incoming,bool archive)
        {
            if(saveBlocked||disabling)throw new InvalidOperationException("Local profile is not ready for cloud application.");
            incoming.Validate();
            if(archive)new ProfileStore(ProfilePath+".before-cloud").Save(profile);
            // OS permissions, language, quality and current gesture remain owned by this device.
            incoming.Preferences=Preferences;
            if(cloudSave.LoadingAccount)incoming.SelectJourney(profile.PreferredMode);
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
                EnsureSavedJourneyContent();var restored=profile.RestoreAttempt(Catalog,legacyContent);
                if(restored!=null){MountAttempt(restored,profile.Descent);journeyActive=profile.JourneyAttempt;storyRoute=profile.StoryRoute??profile.JourneyAttempt;}
                firstGestureShown=profile.FirstGestureShown;
                if(CurrentScreen!=GameScreen.AccountGate)ShowHome();
            }
            if(CurrentScreen!=GameScreen.Playing&&CurrentScreen!=GameScreen.Pause)DrawMenu();
        }
        void RestoreCloudCheckpoint()
        {
            var checkpoint=cloudSave?.Account?.Devices?.OrderByDescending(d=>d.UpdatedUnixSeconds).FirstOrDefault();
            if(checkpoint==null)return;
            PersistProgress();new ProfileStore(ProfilePath+".before-cloud").Save(profile);
            var restoredProfile=CloudJson.Clone(profile);checkpoint.ApplyTo(restoredProfile);restoredProfile.Preferences=Preferences;
            try{
                restoredProfile.Validate();var attempt=restoredProfile.RestoreAttempt(Catalog,legacyContent);
                if(attempt==null)return;
                profileStore.Save(restoredProfile);ApplyProfileFields(restoredProfile);
                PreferredMode=(PuzzleMode)Enum.Parse(typeof(PuzzleMode),profile.PreferredMode);
                StopPresentation();MountAttempt(attempt,profile.Descent);journeyActive=profile.JourneyAttempt;storyRoute=profile.StoryRoute??journeyActive;
                ShowHome();QueueCloudSave();
            }catch(Exception e)when(e is ArgumentException||e is InvalidDataException){CloudStatusChanged();}
        }
        void DrawCloudSettings()
        {
            UiCopy note=CloudState==CloudSyncState.LocalOnly?"Saved on this phone":cloudSave?.CanCancelSignIn==true?"Finish signing in in your browser, or cancel here.":
                cloudSave?.LastFailureCode=="SignInTimeout"?"Sign-in timed out. Your save is safe. Try again.":
                cloudSave?.LastFailureCode=="SignInFailed"?"Sign-in failed. Your save is safe. Try again.":CloudState==CloudSyncState.Connecting?"Connecting to your save...":
                CloudState==CloudSyncState.Saving?"Saving your progress...":cloudSave?.LastFailureCode=="UnverifiedProgress"?"Saved here. Older level results need verification before cloud transfer.":CloudState==CloudSyncState.Offline?"Saved on this device. We will retry online.":
                CloudState==CloudSyncState.Conflict?"Progress saved. Items restored from your account.":CloudState==CloudSyncState.AccountChoice?"This account already has a save. Load it?":"Your progress is saved in the cloud.";
            SettingsNote("Cloud status",note);
            // No cloud project is configured yet. Two inert buttons read as broken
            // controls; keep the local-save status visible without offering them.
            if(CloudState==CloudSyncState.LocalOnly)return;
            if(cloudSave?.CanCancelSignIn==true){SettingsButton("Cancel sign-in",()=>cloudSave.CancelSignIn());return;}
            var sync=SettingsButton("Save now",()=>{PersistProgress();cloudSave?.Retry();});sync.interactable=cloudSave!=null&&!cloudSave.NeedsAccountChoice;
            bool purchaseActive=purchaseStore!=null&&(purchaseStore.State==PurchaseState.Busy||purchaseStore.State==PurchaseState.Connecting);
            var link=SettingsButton(cloudSave?.NeedsAccountChoice==true?(UiCopy)"Load account":cloudSave?.Linked==true?"Manage account":"Sign in with Apple or Google",
                ()=>{if(purchaseActive)return;PersistProgress(true);if(HasSaveError)return;if(cloudSave?.NeedsAccountChoice==true)cloudSave.LoadExistingAccount();else if(cloudSave?.Linked==true)cloudSave.ManageAccount();else cloudSave?.LinkAccount();});
            link.GetComponent<Button>().interactable=cloudSave?.CanLink==true&&!purchaseActive;
            if(cloudSave?.NeedsAccountChoice==true)SettingsButton("Keep this phone's save",()=>cloudSave.KeepLocalAccount());
            if(cloudSave?.Account?.Devices?.Any(d=>d.Journeys.Any(j=>j.Attempt!=null))==true)SettingsButton("Load latest game",RestoreCloudCheckpoint);
            SettingsNote("Account help","Apple or Google protects your progress and verified purchases.");
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
