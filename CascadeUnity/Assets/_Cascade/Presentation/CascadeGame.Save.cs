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
            var loaded=profileStore.Load();profile=loaded.Profile;profile.MigrateCoinEconomy();Recovery=loaded.Recovery;
            Preferences=profile.Preferences;PreferredMode=(PuzzleMode)Enum.Parse(typeof(PuzzleMode),profile.PreferredMode);
            sessionClears.UnionWith(profile.Clears.Select(r=>r.CampaignNodeId));
            heardStory.UnionWith(profile.HeardStory);firstGestureShown=profile.FirstGestureShown;
            saveBlocked=Recovery!=SaveRecovery.None;
        }
        void RestoreProfile()
        {
            try {
                EnsureSavedJourneyContent();
                var restored=profile.RestoreAttempt(Catalog,legacyContent);
                if(restored!=null) {
                    MountAttempt(restored,profile.Descent);journeyActive=profile.JourneyAttempt;storyRoute=profile.StoryRoute??profile.JourneyAttempt;
                    selectedJourneyStop=Math.Max(0,Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node));
                }
            }
            catch(Exception e) when(e is ArgumentException || e is InvalidDataException) {
                Recovery=SaveRecovery.AttemptUnavailable;saveBlocked=true;
            }
            persistenceReady=true;
            InitializeRewards();
            if(Recovery==SaveRecovery.None){
                if(profile.Completion!=null&&!profile.Completion.Acknowledged&&Interaction.Attempt.Current.Status==PuzzleStatus.Won){
                    if(profile.Completion.StatsSeen&&profile.Completion.ParcelOrder>0)OpenParcel(true);else ShowGame();
                }else if(NeedsAccountIntro())ShowAccountGate();else ShowHome();PersistProgress();}
            else SetScreen(GameScreen.SaveRecovery);
        }
        bool NeedsAccountIntro()=>!profile.AccountIntroSeen;
        void EnsureSavedJourneyContent()
        {
            if(profile.Attempt==null)return;
            string pinned=profile.Descent?.SourceHash??profile.Attempt.Direction?.SourceHash??profile.Attempt.ContentHash;
            if(!Catalog.PrimaryLevels.Concat(Catalog.LocalizedLevels).Concat(Catalog.BonusLevels).Concat(legacyContent).Any(c=>c.Definition.ContentHash==pinned))
                legacyContent=ContentImporter.ImportLegacyDurability(Catalog,Source("campaign_extension"),Source("campaign_durability_v1"));
        }
        void PersistProgress(bool durable=false)
        {
            if(!persistenceReady || saveBlocked || Interaction==null)return;
            profile.Preferences=Preferences;profile.PreferredMode=PreferredMode.ToString();
            profile.HeardStory=heardStory.OrderBy(p=>p).ToList();profile.FirstGestureShown=firstGestureShown;
            if(!IsPlanningDescent&&!IsPlanningDirection) {
                profile.JourneyAttempt=journeyActive;profile.Attempt=AttemptRecord.Capture(Interaction.Attempt);profile.Descent=descent;
            }
            profile.StoryRoute=storyRoute;
            try {if(durable)profileStore.Save(profile);else profileStore.QueueSave(profile);HasSaveError=false;QueueCloudSave();}
            catch(Exception e) when(e is IOException || e is UnauthorizedAccessException || e is InvalidOperationException) {
                HasSaveError=true;
                if(status!=null)status.SetCopy("Could not save. Keep the game open and try again.");
            }
        }
        void ObserveSaveFailure()
        {
            if(profileStore?.WriteError==null||HasSaveError)return;
            HasSaveError=true;
            if(status!=null)status.SetCopy("Could not save. Keep the game open and try again.");
            Debug.LogWarning("CASCADE profile write failed; durable checkpoint retry required.");
        }
        void RecordCommittedProgress()
        {
            if(PersonalMissionActive){RecordPersonalMission();return;}
            var attempt=Interaction.Attempt;
            // QA/custom drafts are playable but do not grant campaign receipts.
            bool published=Catalog.PrimaryLevels.Concat(Catalog.LocalizedLevels).Concat(Catalog.BonusLevels).Concat(legacyContent).Any(c=>descent==null?DeliveryDirector.MatchesSource(attempt,c.Definition)
                :c.Definition.ContentHash==descent.SourceHash&&attempt.PinnedContentHash==descent.Level(c.Definition,descent.StageIndex).ContentHash);
            if(published) {
                bool full=descent==null||descent.IsFinal;
                if(journeyActive&&!sessionClears.Contains(node)&&(full||attempt.Current.Status==PuzzleStatus.Lost))profile.Director.ObserveTerminal(attempt,MinimumMoves,descent?.Performance(attempt));
                profile.Descent=descent;
                if(full) {
                    if(attempt.Current.Status==PuzzleStatus.Won&&profile.Completion?.AttemptId!=attempt.AttemptId.ToString("D")) {
                        int coins=Rewards.EarnedCoins(OrderedNodes);bool accountFirst=!Rewards.SeenClearNodes.Contains(node);var previous=profile.Clears.FirstOrDefault(c=>c.CampaignNodeId==node);
                        var completion=LevelCompletion.From(attempt,MinimumMoves,descent?.Performance(attempt));
                        completion.NewBest=previous==null||completion.Score>previous.BestScore||completion.StarRating>previous.BestStarRating;
                        bool first=profile.RecordClear(attempt,completion);if(first)sessionClears.Add(node);
                        InitializeRewards();completion.Coins=Rewards.EarnedCoins(OrderedNodes)-coins;completion.Lights=first&&accountFirst&&OrderedNodes.Contains(node)?1:0;
                        completion.ParcelOrder=WaitingParcel?.Order??0;profile.Completion=completion;
                    }
                    profile.Rewards.RecordDelivery(attempt,descent?.Performance(attempt),profile.ModeRecords);
                }
            }
            PersistProgress(attempt.Current.Status==PuzzleStatus.Won);
        }
        void InitializeRewards()
        {
            if(Recovery==SaveRecovery.NewerVersion)return;
            var known=Catalog.PrimaryLevels.Select(c=>c.Definition.CampaignNodeId).ToHashSet();
            profile.Rewards.Synchronize(profile.AllClears.Where(r=>known.Contains(r.CampaignNodeId)).Select(r=>r.CampaignNodeId));
            profile.Rewards.RegisterVisit(DateTime.Now);
        }
        public void AcknowledgeRecovery()
        {
            if(Recovery==SaveRecovery.NewerVersion)return;
            if(Recovery==SaveRecovery.WriteFailed) {
                PersistProgress(true);if(HasSaveError)return;
                Recovery=SaveRecovery.None;ShowGame();return;
            }
            bool restart=Recovery==SaveRecovery.AttemptUnavailable||Recovery==SaveRecovery.Unreadable;
            Recovery=SaveRecovery.None;saveBlocked=false;
            if(restart){profile.Missions.ClearActive();profile.Attempt=null;profile.Descent=null;LoadNode(initialNode);}
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
            ReadingStrip("Recovery explanation",new Vector2(.055f,.47f),new Vector2(.945f,.72f),body,18,true);
            if(Recovery!=SaveRecovery.NewerVersion)MenuButton(Recovery==SaveRecovery.WriteFailed?"Try saving again":Recovery==SaveRecovery.Backup?"Continue":Recovery==SaveRecovery.Unreadable?"Start a new path":"Start a new puzzle",.35f,AcknowledgeRecovery,true);
        }
        void OnApplicationQuit(){PersistProgress(true);SetLocalReminderBackground(true);}
    }
}
