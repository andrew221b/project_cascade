using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Content;
using Cascade.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        static readonly string[] MissionResidents={"Bram","Pip","Rue","Sora","Neri","Toma","Lumi","Gran","Aster"};
        static readonly string[] MissionInvitations={
            "A delivery cart broke the footbridge rail. I need parts before it returns. {0} Three harder stages.",
            "My brother's new well has a leaking valve. Help me get a replacement to him. {0} Three harder stages.",
            "A nest is stranded beyond the orchard path. I need a lamp for its tree. {0} Three harder stages.",
            "A mail basket missed its landing. Help me recover it before the envelopes get wet. {0} Three harder stages.",
            "A boat is waiting at the unlit pier. Will you bring a lamp down to it? {0} Three harder stages.",
            "The roof needs another patch. Help me deliver the tiles before the next rain. {0} Three harder stages.",
            "Travellers used the shelter's last fuel. Let's refill it before nightfall. {0} Three harder stages.",
            "The spare wick has arrived below the beacon. Will you bring it up with me? {0} Three harder stages.",
            "A cracked lens hides the eastern route. Help me deliver a new one intact. {0} Three harder stages."};
        Task<DescentRun> missionPlan;
        CancellationTokenSource missionCancellation;
        PersonalMissionMemory missionPlanningMemory;
        LevelContent missionSource;
        float missionCheckAt;
        public bool PersonalMissionActive=>profile?.Missions?.Active==true;
        public bool PersonalMissionAvailable=>profile?.Missions?.Offer!=null&&!PersonalMissionActive;
        void CancelMissionPlan()
        {
            missionCancellation?.Cancel();missionCancellation?.Dispose();missionCancellation=null;
            if(missionPlan!=null)missionPlan.ContinueWith(t=>{var ignored=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
            missionPlan=null;missionPlanningMemory=null;missionSource=null;
        }
        LevelContent MissionContent(string hash)=>Catalog.PrimaryLevels.Concat(Catalog.LocalizedLevels).Concat(legacyContent).FirstOrDefault(c=>c.Definition.ContentHash==hash);
        void TickPersonalMissions()
        {
            bool safe=CurrentScreen==GameScreen.JourneyMap&&BrowsedIsland<0&&!ConversationOpen&&!PersonalMissionActive
                &&IntroductionAnimationAllowed&&!saveBlocked&&!HasSaveError&&!IsPlanningDescent&&!IsPlanningDirection
                &&(profile.Completion==null||profile.Completion.Acknowledged);
            if(!safe){if(missionPlan!=null)CancelMissionPlan();return;}
            var memory=profile.Missions;
            int challenge=PersonalMissionMemory.ChallengeFor(profile.Director);
            if(memory.Offer==null&&challenge==0){if(missionPlan!=null)CancelMissionPlan();return;}
            if(missionPlan!=null){
                if(!ReferenceEquals(memory,missionPlanningMemory)){CancelMissionPlan();return;}
                if(!missionPlan.IsCompleted)return;
                var task=missionPlan;var source=missionSource;missionPlan=null;
                missionCancellation.Dispose();missionCancellation=null;missionPlanningMemory=null;missionSource=null;
                DescentRun run=null;
                if(task.IsFaulted){var ignored=task.Exception;}else if(!task.IsCanceled)run=task.Result;
                if(run==null){memory.NextAfterClears=JourneyClearCount+1;PersistProgress();return;}
                int world=JourneyStory.Zone(Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==source.Definition.CampaignNodeId));
                memory.Present(new PersonalMissionOffer{Serial=memory.Serial+1,Hero=memory.Serial%(Mathf.Clamp(world,0,8)+1),
                    SourceHash=source.Definition.ContentHash,Locale=source.Locale,Run=run},JourneyClearCount);
                PersistProgress(true);if(HasSaveError)return;
            }
            bool touching=Mouse.current?.leftButton.isPressed==true||Touchscreen.current?.primaryTouch.press.isPressed==true;
            if(touching){missionCheckAt=Time.realtimeSinceStartup+1.2f;return;}
            if(Time.realtimeSinceStartup<missionCheckAt)return;
            missionCheckAt=Time.realtimeSinceStartup+2;
            if(memory.Offer!=null){ShowPersonalMissionOffer();return;}
            if(!memory.Due(profile.Clears.Count)||!memory.Due(JourneyClearCount))return;
            var content=JourneySequence().LastOrDefault(c=>sessionClears.Contains(c.Definition.CampaignNodeId));
            if(content==null)return;
            missionSource=content;missionPlanningMemory=memory;
            missionCancellation=new CancellationTokenSource();missionCancellation.CancelAfter(TimeSpan.FromSeconds(8));
            var token=missionCancellation.Token;int serial=memory.Serial+1;string previous=memory.LastFingerprint;
            var certificates=PublishedTriads();string catalogHash=Catalog.ContentHash;
            missionPlan=Task.Run(async()=>{
                var store=await certificates;token.ThrowIfCancellationRequested();
                var reference=store?.Bind(content.Definition,content.Locale,catalogHash);
                // A missing published baseline must not offer an easier regenerated substitute.
                if(reference==null)return null;
                return CampaignTriads.BuildPersonalMission(content.Definition,serial,previous,token,reference:reference,challenge:challenge);
            },token);
        }
        static string MissionGoal(MechanicLesson lesson)
        {
            switch(lesson){
                case MechanicLesson.Repair:return "Mend the boards and bring the row home.";
                case MechanicLesson.Key:return "Open the route with the key and bring the row home.";
                case MechanicLesson.Bomb:case MechanicLesson.BombHourglass:return "Deal with the bombs and deliver the row safely.";
                case MechanicLesson.Ribbon:return "Use the ribbon to reach across the row and deliver it.";
                case MechanicLesson.Stamp:return "Collect the stamps and bring the row home.";
                case MechanicLesson.Bridge:return "Cross the one-use bridges and bring the row home.";
                case MechanicLesson.LinkedPairs:return "Move the linked pairs into order and deliver the row.";
                case MechanicLesson.Fragile:return "Plan around the fragile boards and deliver the row.";
                case MechanicLesson.Mirror:case MechanicLesson.Rollers:case MechanicLesson.LiveGust:return "Prepare for the changing row and bring it home.";
                case MechanicLesson.ClosedPath:return "Find a route through the closed paths and deliver the row.";
                default:return "Put my scattered supplies in order and bring them home.";
            }
        }
        void ShowPersonalMissionOffer()
        {
            if(!PersonalMissionAvailable||ConversationOpen)return;
            var offer=profile.Missions.Offer;
            if(MissionContent(offer.SourceHash)==null){profile.Missions.Decline();PersistProgress();return;}
            var lesson=(MechanicLesson)offer.Run.Stages[0].Triad.Lesson;
            OpenConversation(new StoryConversation(-1,new StoryLine(MissionResidents[offer.Hero],
                MissionInvitations[offer.Hero],MissionGoal(lesson))),
                "Accept",AcceptPersonalMission,DeclinePersonalMission);
            conversationSkip.GetComponentInChildren<TextMeshProUGUI>().SetCopy("Decline");
        }
        public void DeclinePersonalMission()
        {
            if(PersonalMissionActive||!PersonalMissionAvailable)return;
            CancelConversation();profile.Missions.Decline();PersistProgress();
        }
        public void AcceptPersonalMission()
        {
            if(!PersonalMissionAvailable||saveBlocked||HasSaveError)return;
            var memory=profile.Missions;var content=MissionContent(memory.Offer.SourceHash);if(content==null)return;
            try{memory.Offer.Run.Validate(content.Definition,memory.Offer.Locale);}
            catch(ArgumentException){DeclinePersonalMission();return;}
            PersistProgress(true);if(HasSaveError)return;
            CancelConversation();CancelMissionPlan();
            memory.CampaignAttempt=profile.Attempt;memory.CampaignDescent=profile.Descent;memory.CampaignCompletion=profile.Completion;
            memory.CampaignJourney=profile.JourneyAttempt;memory.CampaignStory=profile.StoryRoute;memory.Active=true;memory.Won=false;
            var run=memory.Offer.Run;profile.Completion=null;
            MountAttempt(new AttemptService(run.Level(content.Definition,0),memory.Offer.Locale),run);
            journeyActive=false;storyRoute=false;PersistProgress(true);
            if(HasSaveError){RestorePersonalCampaign(false);return;}
            ShowGame();
        }
        void RestorePersonalCampaign(bool showMap)
        {
            if(!PersonalMissionActive)return;
            var memory=profile.Missions;
            var saved=new PlayerProfile{Attempt=memory.CampaignAttempt,Descent=memory.CampaignDescent};
            var restored=saved.RestoreAttempt(Catalog,legacyContent);
            var run=memory.CampaignDescent;var completion=memory.CampaignCompletion;
            bool journey=memory.CampaignJourney;bool? story=memory.CampaignStory;
            memory.ClearActive();profile.Completion=completion;
            if(restored!=null)MountAttempt(restored,run);
            journeyActive=journey;storyRoute=story??journey;PersistProgress(true);
            if(showMap&&!HasSaveError)ShowJourney();
        }
        void RecordPersonalMission()
        {
            var memory=profile.Missions;var attempt=Interaction.Attempt;
            if(descent?.IsFinal==true&&attempt.Current.Status==Cascade.Domain.PuzzleStatus.Won){
                if(!memory.Won){memory.Won=true;memory.Completed++;}
                if(profile.Completion?.AttemptId!=attempt.AttemptId.ToString("D"))
                    profile.Completion=LevelCompletion.From(attempt,MinimumMoves,descent.Performance(attempt));
            }
            PersistProgress();
        }
    }
}
