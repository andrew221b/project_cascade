using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
#if UNITY_EDITOR
        // Editor capture/test sessions supply an isolated directory before the scene is loaded.
        public static string EditorProfileDirectory { get; set; }
#endif
        ProfileStore profileStore;
        PlayerProfile profile;
        bool persistenceReady, saveBlocked;
        System.Collections.Generic.IEnumerable<LevelContent> legacyContent = Enumerable.Empty<LevelContent>();
        public SaveRecovery Recovery { get; private set; }
        public string ProfilePath=>profileStore?.Path;
        public bool HasSaveError { get; private set; }
        public JourneyRewards Rewards=>profile.Rewards;
        void ReadProfile()
        {
            string directory=UnityEngine.Application.persistentDataPath;
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-cascade-perf")>=0)directory=Path.Combine(Path.GetTempPath(),"CascadeCraftBenchmark");
#if UNITY_EDITOR
            if(!string.IsNullOrEmpty(EditorProfileDirectory))directory=EditorProfileDirectory;
#endif
            profileStore=new ProfileStore(Path.Combine(directory,"cascade-profile.json"));
            var loaded=profileStore.Load();profile=loaded.Profile;Recovery=loaded.Recovery;
            Preferences=profile.Preferences;PreferredMode=(PuzzleMode)Enum.Parse(typeof(PuzzleMode),profile.PreferredMode);
            sessionClears.UnionWith(profile.Clears.Select(r=>r.CampaignNodeId));
            heardStory.UnionWith(profile.HeardStory);firstGestureShown=profile.FirstGestureShown;
            saveBlocked=Recovery!=SaveRecovery.None;
        }
        void RestoreProfile()
        {
            try {
                if(profile.Attempt!=null && !Catalog.PrimaryLevels.Concat(Catalog.LocalizedLevels).Concat(Catalog.BonusLevels).Any(c=>c.Definition.ContentHash==(profile.Descent?.SourceHash??profile.Attempt.Direction?.SourceHash??profile.Attempt.ContentHash)))
                    legacyContent=ContentImporter.ImportLegacyDurability(Catalog,Source("campaign_extension"),Source("campaign_durability_v1"));
                var restored=profile.RestoreAttempt(Catalog,legacyContent);
                if(restored!=null) {
                    MountAttempt(restored,profile.Descent);journeyActive=profile.JourneyAttempt;
                    selectedJourneyStop=Math.Max(0,Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node));
                    if(journeyActive&&descent==null){levelLabel.SetCopy(JourneyStory.Task(JourneyStory.PageFor(selectedJourneyStop)));levelLabel.fontSize=21;}
                }
            }
            catch(Exception e) when(e is ArgumentException || e is InvalidDataException) {
                Recovery=SaveRecovery.AttemptUnavailable;saveBlocked=true;
            }
            persistenceReady=true;
            InitializeRewards();
            if(Recovery==SaveRecovery.None){ShowHome();PersistProgress();}
            else SetScreen(GameScreen.SaveRecovery);
        }
        void PersistProgress()
        {
            if(!persistenceReady || saveBlocked || Interaction==null)return;
            profile.Preferences=Preferences;profile.PreferredMode=PreferredMode.ToString();
            profile.HeardStory=heardStory.OrderBy(p=>p).ToList();profile.FirstGestureShown=firstGestureShown;
            profile.JourneyAttempt=journeyActive;profile.Attempt=AttemptRecord.Capture(Interaction.Attempt);profile.Descent=descent;
            try {profileStore.Save(profile);HasSaveError=false;}
            catch(Exception e) when(e is IOException || e is UnauthorizedAccessException || e is InvalidOperationException) {
                HasSaveError=true;
                if(status!=null)status.SetCopy("Could not save. Keep the game open and try again.");
            }
        }
        void RecordCommittedProgress()
        {
            var attempt=Interaction.Attempt;
            // QA/custom drafts are playable but do not grant campaign receipts.
            bool published=Catalog.PrimaryLevels.Concat(Catalog.LocalizedLevels).Concat(Catalog.BonusLevels).Concat(legacyContent).Any(c=>descent==null?DeliveryDirector.MatchesSource(attempt,c.Definition)
                :c.Definition.ContentHash==descent.SourceHash&&attempt.PinnedContentHash==descent.Level(c.Definition,descent.StageIndex).ContentHash);
            if(published) {
                bool full=descent==null||descent.IsFinal;
                if(journeyActive&&!sessionClears.Contains(node)&&(full||attempt.Current.Status==PuzzleStatus.Lost))profile.Director.ObserveTerminal(attempt,MinimumMoves,descent?.Performance(attempt));
                profile.Descent=descent;
                if(full) {
                    if(profile.RecordClear(attempt))sessionClears.Add(node);
                    InitializeRewards();profile.Rewards.RecordDelivery(attempt,descent?.Performance(attempt));
                }
            }
            PersistProgress();
        }
        void InitializeRewards()
        {
            if(Recovery==SaveRecovery.NewerVersion)return;
            var known=Catalog.PrimaryLevels.Select(c=>c.Definition.CampaignNodeId).ToHashSet();
            profile.Rewards.Synchronize(profile.Clears.Where(r=>known.Contains(r.CampaignNodeId)).Select(r=>r.CampaignNodeId));
            profile.Rewards.RegisterVisit(DateTime.Now);
        }
        public void AcknowledgeRecovery()
        {
            if(Recovery==SaveRecovery.NewerVersion)return;
            if(Recovery==SaveRecovery.WriteFailed) {
                PersistProgress();if(HasSaveError)return;
                Recovery=SaveRecovery.None;ShowGame();return;
            }
            bool restart=Recovery==SaveRecovery.AttemptUnavailable||Recovery==SaveRecovery.Unreadable;
            Recovery=SaveRecovery.None;saveBlocked=false;
            if(restart){profile.Attempt=null;profile.Descent=null;LoadNode(initialNode);}
            ShowHome();PersistProgress();
        }
        void DrawSaveRecovery()
        {
            UiCopy body=Recovery==SaveRecovery.WriteFailed?"Could not save. Keep the game open and try again."
                :Recovery==SaveRecovery.Backup?"We found a backup. Your last move may be missing."
                :Recovery==SaveRecovery.AttemptUnavailable?"This puzzle changed. Your saved lights are safe. Start a new puzzle."
                :Recovery==SaveRecovery.NewerVersion?"This path needs a newer game. Update CASCADE to open it."
                :"We could not read your saved path. You can start a new one.";
            MenuTitle("Your saved path","");
            Text("Recovery explanation",menuRoot,new Vector2(.10f,.48f),new Vector2(.90f,.74f),20,body);
            if(Recovery!=SaveRecovery.NewerVersion)MenuButton(Recovery==SaveRecovery.WriteFailed?"Try saving again":Recovery==SaveRecovery.Backup?"Continue":Recovery==SaveRecovery.Unreadable?"Start a new path":"Start a new puzzle",.35f,AcknowledgeRecovery,true);
        }
        void OnApplicationQuit(){PersistProgress();}
    }
}
