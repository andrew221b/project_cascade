using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly Queue<MechanicLesson> firstMechanics=new Queue<MechanicLesson>();
        readonly HashSet<(MechanicLesson Lesson,int Floor)> deferredLessons=new HashSet<(MechanicLesson,int)>();
        bool needsMechanicScan,mechanicWasPaused,mechanicPreviewOnly;
        int mechanicMountFrame;
        Guid discoveredAttempt;int discoveredFloor=-1;bool discoveredReach;
        RectTransform mechanicOverlay,mechanicSafe;
        CanvasGroup mechanicOpacity;
        Sequence mechanicArrival;
        Action afterMechanic;
        bool? mechanicWindRight;
        Sprite introBranch;
        const string TwoWaysCoach="controls-two-ways-v1",UndoCoach="controls-undo-v1",ClosedPathCoach="closed-path-v2";
        bool IndependentTriad=>descent?.Version==6&&descent.StageIndex>0;
        string TriadCoach(MechanicLesson lesson)=>descent?.Version==6&&descent.StageIndex==0&&descent.Stages[0].Triad.Lesson==(int)lesson
            ?"triad-guide-v6-"+(int)lesson:null;
        // An upgraded save can lack the newer gesture-coach marker. A cleared
        // opening level proves basic control; do not interrupt a later puzzle
        // with the opening rehearsal. Replaying C01 can still teach both ways.
        // The preference controls optional input coaching. A new visible world
        // mechanic is always introduced once, including on the lower board.
        static bool OptionalControlLesson(MechanicLesson lesson)=>lesson==MechanicLesson.Swap||lesson==MechanicLesson.Descent||lesson==MechanicLesson.Undo||lesson==MechanicLesson.Hint||lesson==MechanicLesson.Toolbag;
        static string MechanicFilmMarker(MechanicLesson lesson)=>"mechanic-film-v1-"+(int)lesson;
        void RememberMechanicFilm(MechanicLesson lesson)
        {
            string marker=MechanicFilmMarker(lesson);
            if(profile.SeenCoachPrompts.Contains(marker))return;
            profile.SeenCoachPrompts.Add(marker);PersistProgress(true);
            if(HasSaveError)profile.SeenCoachPrompts.Remove(marker);
        }
        bool NeedsControlLesson(MechanicLesson lesson)=>(Preferences.Tutorials||!OptionalControlLesson(lesson))&&!LessonWasSeen(lesson)
            &&!profile.SeenCoachPrompts.Contains(MechanicFilmMarker(lesson))
            &&!(lesson==MechanicLesson.Swap&&node!="C01"&&profile.Clears.Any(c=>c.CampaignNodeId=="C01"&&c.BestStars>0));
        public bool MechanicPopupOpen=>mechanicOverlay!=null;
        public MechanicLesson? CurrentMechanic { get; private set; }
        public IReadOnlyList<MechanicLesson> SeenMechanics=>profile.SeenMechanics.AsReadOnly();
        internal bool IntroductionAnimationAllowed=>applicationFocused&&!applicationPaused&&!nativeAdSuspended;
        static readonly string[] MechanicTitles={"Bomb","Circular saw","Repair","Ribbon spool","Stamp","Frost flask","One-use bridge","Linked pairs","Fragile edge","Closed path","Entry rollers","Mirror on entry","Wind on landing","A gust of wind","Wayfinder scroll","Undo spool","Return hourglass","Signal lantern","Landing lens","Strong gust"};
        void ResetMechanicDiscovery()
        {
            CloseMechanicPopup();ClearLessonPractice();rehearsedLessons.Clear();practiceFloorOverride=null;firstMechanics.Clear();needsMechanicScan=Interaction==null||Interaction.Attempt.Current.Status==PuzzleStatus.Playing;mechanicMountFrame=Time.frameCount;
        }
        void TryFirstMechanic()
        {
            if(!Preferences.Tutorials){
                if(MechanicPopupOpen&&OptionalControlLesson(CurrentMechanic.Value)&&!lessonReplay)CloseMechanicPopup();
                if(PendingLesson.HasValue)ClearLessonPractice();
                bool preparing=descentPlanFailed||!PreparationReady||IsDescending||IsPlanningDescent||IsPlanningDirection;
                if(Interaction!=null&&Interaction.IsPreparing!=preparing)Interaction.SetPreparing(preparing);
            }
            if(!flowReady||profile==null||CurrentScreen!=GameScreen.Playing||MechanicPopupOpen||!IntroductionAnimationAllowed
                ||fieldTrayOpen||PendingLesson.HasValue||IsHoldingToken||!PreparationReady||Interaction.IsPresenting||IsDescending||IsPlanningDescent||IsPlanningDirection||Time.frameCount<=mechanicMountFrame||saveBlocked||HasSaveError)return;
            // Restores and round mounts can change the visible puzzle without a
            // committed FloorEntered event. Discover once for that new context too.
            var attempt=Interaction.Attempt;bool reach=attempt.Current.ReachCharges>0;
            if(discoveredAttempt!=attempt.AttemptId||discoveredFloor!=attempt.Current.ActiveFloor||discoveredReach!=reach){
                discoveredAttempt=attempt.AttemptId;discoveredFloor=attempt.Current.ActiveFloor;discoveredReach=reach;needsMechanicScan=true;
            }
            if(Interaction.Attempt.CanUndo&&NeedsControlLesson(MechanicLesson.Undo))needsMechanicScan=true;
            if(needsMechanicScan){
                firstMechanics.Clear();
                var visible=MechanicLessons.DiscoverVisible(Interaction.Attempt.Level,Interaction.Attempt.Current,IsWindFloor(Interaction.Attempt.Current.ActiveFloor+1),true);
                if(CurrentScreen==GameScreen.Playing&&NeedsControlLesson(MechanicLesson.Swap))firstMechanics.Enqueue(MechanicLesson.Swap);
                if(Interaction.Attempt.CanUndo&&NeedsControlLesson(MechanicLesson.Undo))firstMechanics.Enqueue(MechanicLesson.Undo);
                foreach(var lesson in visible)
                    if(NeedsControlLesson(lesson)
                        &&!deferredLessons.Contains((lesson,Interaction.Attempt.Current.ActiveFloor)))firstMechanics.Enqueue(lesson);
                needsMechanicScan=false;
                Interaction.SetPreparing(descentPlanFailed||!PreparationReady||IsDescending||IsPlanningDescent);RenderControls();
            }
            while(firstMechanics.Count>0){var next=firstMechanics.Dequeue();if(NeedsControlLesson(next)){
                var active=MechanicLessons.DiscoverVisible(Interaction.Attempt.Level,Interaction.Attempt.Current,IsWindFloor(Interaction.Attempt.Current.ActiveFloor+1));
                OpenMechanicPopup(next);mechanicPreviewOnly=!OptionalControlLesson(next)&&!active.Contains(next);break;
            }}
        }
        // Retained for older QA callers. Ambient scenery never owns a lesson.
        public bool RequireGustIntroduction()=>false;
        bool IntroduceTool(TravelTool tool,Action use)
        {
            var lesson=MechanicLessons.Tool(tool);if(!NeedsControlLesson(lesson)||rehearsedLessons.ContainsKey(lesson))return false;
            // A first-use film replaces the inventory, never sits behind its canvas.
            if(fieldTrayOpen)SetToolTray(false,true);
            if(CurrentScreen==GameScreen.Supplies)SetScreen(GameScreen.Playing);
            OpenMechanicPopup(lesson,use);return true;
        }
        bool IntroduceDescentEntry()
        {
            var upcoming=descent.Level(DescentPublishedSource(),descent.StageIndex+1);
            var arrival=upcoming.Floors[0].EntryTransform;if(arrival==EntryTransform.None)return false;
            var lesson=arrival==EntryTransform.MirrorNext?MechanicLesson.Mirror:MechanicLesson.Rollers;
            if(!NeedsControlLesson(lesson)||rehearsedLessons.ContainsKey(lesson))return false;
            OpenMechanicPopup(lesson,windRight:arrival==EntryTransform.ShiftRightOne);return true;
        }
        void OpenMechanicPopup(MechanicLesson lesson,Action requestedAction=null,UiCopy acknowledgement=default,bool? windRight=null)
        {
            if(MechanicPopupOpen)return;
            CurrentMechanic=lesson;afterMechanic=requestedAction;mechanicWasPaused=Interaction.IsPaused;mechanicWindRight=windRight;
            EndTokenHold();Interaction.CancelPointer();Interaction.SetPaused(true);machine.SetAmbientPaused(true);
            HideInstruction();coachRoot.gameObject.SetActive(false);tutorialHand.gameObject.SetActive(false);
            if(!changingTutorialPage)tutorialControlPage=0;
            BuildReferenceTutorial(lesson,requestedAction,windRight);RenderControls();
        }
        UiCopy MechanicExplanation(MechanicLesson lesson)
        {
            switch(lesson){
                case MechanicLesson.Swap:return "Drag one piece onto another, or tap both. Every exchange wears down the board.";
                case MechanicLesson.Repair:return "Put the pictured piece above the toolbox. It restores the shown durability for more exchanges.";
                case MechanicLesson.Key:return "Put the pictured piece above the saw. It cuts the branch so you can exchange across that gap.";
                case MechanicLesson.Bomb:return "Put the pictured piece above the bomb before its counter runs out. A blast removes board durability.";
                case MechanicLesson.Ribbon:case MechanicLesson.FarSwap:return "Collect the ribbon with the pictured piece. Then exchange two pieces up to three places apart once.";
                case MechanicLesson.BombHourglass:return "Collect the frost flask. The bomb waits for two exchanges, but the board still wears down.";
                case MechanicLesson.Stamp:return "Put the pictured piece above the stamp for 100 bonus points. The receiver still needs the exact row.";
                case MechanicLesson.Bridge:return "This link works once. Plan the row before crossing it: it closes after the exchange.";
                case MechanicLesson.LinkedPairs:return "Exchange one marked pair and both marked pairs move together. One move changes four positions.";
                case MechanicLesson.Fragile:return "An exchange across a cracked link costs two durability instead of one.";
                case MechanicLesson.ClosedPath:return "Pieces cannot cross a closed link. Go down to a board with the links you need.";
                case MechanicLesson.Mirror:return "When the row lands on this board, its order reverses. Prepare the row before going down.";
                case MechanicLesson.Rollers:case MechanicLesson.LandingWind:return "On landing, every piece moves one place in the arrow direction. The end piece wraps around.";
                case MechanicLesson.Gust:case MechanicLesson.LiveGust:return "After the shown number of exchanges, wind shifts the whole row one place. Plan for the shift.";
                case MechanicLesson.Undo:return "Undo returns the pieces and board to before your last move. You have one free return.";
                case MechanicLesson.Descent:return "Go down early to use the next board. The row stays with you.";
                case MechanicLesson.Hint:return "The bulb shows one move; you make it. The counter is for this stage. When empty, an optional video adds one hint.";
                case MechanicLesson.Wayfinder:return "The scroll guides you through this board. Follow the hand; it never moves pieces for you.";
                case MechanicLesson.LandingLens:return "The lens previews the row after the next drop, including the next board’s effect. It does not move your pieces.";
                case MechanicLesson.SignalLantern:return "The lantern makes one safe move for you. One item is consumed.";
                case MechanicLesson.UndoSpool:return "After your free undo, use a spool to undo one more move on this board.";
                case MechanicLesson.ReturnHourglass:return "Return to the board you just left, with its row and durability restored.";
                case MechanicLesson.RepairKit:return "The kit adds two durability to this board. It does not stop bombs or wind. One kit is consumed.";
                case MechanicLesson.Toolbag:return "Tap the bag to open or fold the tray. Use any available tool whenever it helps.";
                default:return "Watch what changes, then try it on your row.";
            }
        }
        static UiCopy LessonTitle(MechanicLesson lesson)=>lesson==MechanicLesson.RepairKit?(UiCopy)"Repair kit":lesson==MechanicLesson.Swap?(UiCopy)"Swap neighbours":lesson==MechanicLesson.Descent?(UiCopy)"Descend":lesson==MechanicLesson.Undo?(UiCopy)"Undo":lesson==MechanicLesson.Hint?(UiCopy)"Hint":lesson==MechanicLesson.FarSwap?(UiCopy)"One far swap ready":lesson==MechanicLesson.Toolbag?(UiCopy)"My tools":MechanicTitles[(int)lesson];
        public void AcknowledgeMechanic()
        {
            if(!MechanicPopupOpen||!IntroductionAnimationAllowed||saveBlocked)return;
            var lesson=CurrentMechanic.Value;var action=afterMechanic;bool replay=lessonReplay,preview=mechanicPreviewOnly;CloseMechanicPopup();
            if(replay){MarkLessonSeen(lesson);return;}
            if(action!=null){MarkLessonSeen(lesson);if(!HasSaveError)action();return;}
            if(preview||!Preferences.Tutorials){MarkLessonSeen(lesson);needsMechanicScan=true;return;}
            RememberMechanicFilm(lesson);if(HasSaveError)return;
            BeginLessonPractice(lesson);
            if(lesson==MechanicLesson.Gust)machine.DelayAmbientGust(.8f);
            feedback.Sound(ToyCue.Select);
        }
        void CloseMechanicPopup()
        {
            if(!MechanicPopupOpen)return;
            mechanicArrival?.Kill();mechanicArrival=null;mechanicOverlay.gameObject.SetActive(false);Destroy(mechanicOverlay.gameObject);mechanicOverlay=null;mechanicSafe=null;tutorialReferenceLayout=null;CurrentMechanic=null;afterMechanic=null;lessonReplay=false;
            mechanicPreviewOnly=false;Interaction?.SetPaused(mechanicWasPaused);machine.SetAmbientPaused(CurrentScreen==GameScreen.Pause||CurrentScreen==GameScreen.SaveRecovery);if(CanPresent)RenderControls();
        }
        void FitMechanicSafeArea(){if(mechanicSafe==null)return;mechanicSafe.anchorMin=safeRoot.anchorMin;mechanicSafe.anchorMax=safeRoot.anchorMax;mechanicSafe.offsetMin=mechanicSafe.offsetMax=Vector2.zero;tutorialReferenceLayout?.Fit();}
        void TickMechanicIntroduction()
        {
            TryFirstMechanic();if(!MechanicPopupOpen)return;FitMechanicSafeArea();
            if(mechanicArrival!=null&&mechanicArrival.IsActive()){
                if(ReducedMotion)mechanicArrival.Complete();
                else if(IntroductionAnimationAllowed)mechanicArrival.Play();else mechanicArrival.Pause();
            }
        }
    }
}
