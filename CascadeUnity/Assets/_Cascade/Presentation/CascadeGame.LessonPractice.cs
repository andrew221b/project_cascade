using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Solver;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        public MechanicLesson? PendingLesson { get; private set; }
        int lessonFloor,lessonGust;
        readonly Dictionary<MechanicLesson,int> rehearsedLessons=new Dictionary<MechanicLesson,int>();
        float lessonTime;
        int? practiceFloorOverride;
        HintRequest lessonRequest;
        Task<(HintRequest Request,PuzzleCommand? Command)> lessonSearch;
        PuzzleCommand? lessonSpecial;
        CancellationTokenSource lessonCancellation;
        Image lessonButtonHalo;
        Button lessonHaloTarget;
        // Discovery waits for active practice; it must not prevent that practice's next input.
        bool AwaitingMechanicScan=>needsMechanicScan&&!PendingLesson.HasValue&&!MechanicPopupOpen;

        void BeginLessonPractice(MechanicLesson lesson)
        {
            ClearLessonPractice();PendingLesson=lesson;lessonFloor=practiceFloorOverride??Interaction.Attempt.Current.ActiveFloor;practiceFloorOverride=null;
            rehearsedLessons[lesson]=lessonFloor;
            lessonGust=machine.AmbientGustCount;lessonTime=0;
            // Keep the point of the finger on the indicated piece or control.
            tutorialHand.rectTransform.pivot=new Vector2(.60f,.865f);
            if(lesson==MechanicLesson.Swap){firstGestureShown=true;RetireInstruction("Drag a piece onto its neighbour.");}
            SyncLessonRestriction();EnsureLessonRoute();RenderControls();
        }
        void ClearLessonPractice()
        {
            PendingLesson=null;Interaction?.ClearPracticeRestriction();CancelLessonRoute();ClearLessonHighlight();
            if(tutorialHand!=null)tutorialHand.gameObject.SetActive(false);
        }
        void CancelLessonRoute()
        {
            lessonCancellation?.Cancel();lessonCancellation?.Dispose();lessonCancellation=null;
            if(lessonSearch!=null)lessonSearch.ContinueWith(t=>{var ignored=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
            lessonSearch=null;lessonRequest=null;lessonSpecial=null;
        }
        void CompleteLessonPractice(MechanicLesson lesson,bool persist=true)
        {
            if(!rehearsedLessons.ContainsKey(lesson))return;
            bool added=!LessonWasSeen(lesson);if(added)profile.SeenMechanics.Add(lesson);
            string coach=lesson==MechanicLesson.Swap?TwoWaysCoach:lesson==MechanicLesson.Undo?UndoCoach:lesson==MechanicLesson.ClosedPath?ClosedPathCoach:null;
            bool coachAdded=coach!=null&&!profile.SeenCoachPrompts.Contains(coach);if(coachAdded)profile.SeenCoachPrompts.Add(coach);
            string triadCoach=TriadCoach(lesson);bool triadAdded=triadCoach!=null&&!profile.SeenCoachPrompts.Contains(triadCoach);
            if(triadAdded)profile.SeenCoachPrompts.Add(triadCoach);
            if(persist)PersistProgress();
            if(persist&&HasSaveError&&added)profile.SeenMechanics.Remove(lesson);
            if(persist&&HasSaveError&&coachAdded)profile.SeenCoachPrompts.Remove(coach);
            if(persist&&HasSaveError&&triadAdded)profile.SeenCoachPrompts.Remove(triadCoach);
            rehearsedLessons.Remove(lesson);if(PendingLesson==lesson)ClearLessonPractice();
            needsMechanicScan=Interaction.Attempt.Current.Status==PuzzleStatus.Playing;
        }
        void ObserveLessonTransition(TransitionResult result)
        {
            if(result.Accepted&&result.State.Status==PuzzleStatus.Playing
                &&System.Linq.Enumerable.Any(result.Events,e=>e.Kind==PuzzleEventKind.FloorEntered||e.Kind==PuzzleEventKind.RibbonCollected))needsMechanicScan=true;
            if(result.Accepted&&PendingLesson==MechanicLesson.Hint&&offeredHint?.Result.Command!=null
                &&Interaction.Attempt.IsCurrent(offeredHint)==false&&Interaction.LastRequestedCommand.HasValue
                &&Interaction.LastRequestedCommand.Value.Equals(offeredHint.Result.Command.Value))CompleteLessonPractice(MechanicLesson.Hint,false);
            foreach(var taught in new List<KeyValuePair<MechanicLesson,int>>(rehearsedLessons)){
                if(taught.Key!=MechanicLesson.Gust&&LessonPractice.CompletedBy(taught.Key,taught.Value,result,Interaction.LastRequestedCommand??PuzzleCommand.Drop)){
                    CompleteLessonPractice(taught.Key,false);continue;
                }
                if(result.Accepted&&System.Linq.Enumerable.Any(result.Events,e=>e.Floor==taught.Value&&e.Kind==PuzzleEventKind.FloorExited)){
                    // The board is gone, so its cue cannot be followed. Show this
                    // lesson again on a future encounter instead of claiming mastery.
                    rehearsedLessons.Remove(taught.Key);if(PendingLesson==taught.Key)ClearLessonPractice();
                    needsMechanicScan=result.State.Status==PuzzleStatus.Playing;
                }
            }
            CancelLessonRoute();
            SyncLessonRestriction();
        }
        bool RequireActionLesson(MechanicLesson lesson,int? onFloor=null)
        {
            if(!Preferences.Tutorials)return false;
            if(MechanicPopupOpen)return true;
            if(PendingLesson.HasValue){
                if(PendingLesson.Value!=lesson&&Interaction.PracticeGuidanceOnly)DeferLessonPractice();
                else return PendingLesson.Value!=lesson;
            }
            if(profile==null||!NeedsControlLesson(lesson)||rehearsedLessons.ContainsKey(lesson))return false;
            OpenMechanicPopup(lesson);practiceFloorOverride=onFloor;return true;
        }
        // Called before a real command. It only pauses; the player must repeat the gesture.
        public bool RequireCommandLesson(PuzzleCommand command)
        {
            if(!Interaction.AllowsCommand(command))return true;
            if(!Interaction.CanPlay||PendingLesson.HasValue)return false;
            var preview=Interaction.Attempt.Resolver.Preview(Interaction.Attempt.Current,command);
            if(!preview.IsLegal){
                if(preview.Rejection==RejectionReason.ClosedEdge&&RequireActionLesson(Interaction.Attempt.Level.Floors[Interaction.Attempt.Current.ActiveFloor].Key!=null&&!Interaction.Attempt.Current.KeyUsed?MechanicLesson.Key:MechanicLesson.ClosedPath))return true;
                return false;
            }
            if(command.Kind==CommandKind.LongReach&&RequireActionLesson(MechanicLesson.FarSwap))return true;
            if(command.Kind==CommandKind.EarlyDrop&&RequireActionLesson(MechanicLesson.Descent))return true;
            var predicted=Interaction.Attempt.Predict(command);
            if(predicted==null||!predicted.Accepted)return false;
            foreach(var e in predicted.Events){
                {
                    MechanicLesson? contact=e.Kind==PuzzleEventKind.BombDefused||e.Kind==PuzzleEventKind.BombExploded?MechanicLesson.Bomb:
                        e.Kind==PuzzleEventKind.KeyOpened?MechanicLesson.Key:e.Kind==PuzzleEventKind.Repaired?MechanicLesson.Repair:
                        e.Kind==PuzzleEventKind.RibbonCollected?MechanicLesson.Ribbon:e.Kind==PuzzleEventKind.HourglassCollected?MechanicLesson.BombHourglass:
                        e.Kind==PuzzleEventKind.StampCollected?MechanicLesson.Stamp:e.Kind==PuzzleEventKind.WindPermuted?MechanicLesson.LiveGust:null;
                    if(contact.HasValue&&RequireActionLesson(contact.Value,e.Floor))return true;
                }
                if(e.Kind==PuzzleEventKind.EntryPermuted){
                var floor=Interaction.Attempt.Level.Floors[e.Floor];
                var lesson=floor.EntryTransform==EntryTransform.MirrorNext?MechanicLesson.Mirror:IsWindFloor(e.Floor)?MechanicLesson.LandingWind:MechanicLesson.Rollers;
                if(RequireActionLesson(lesson))return true;
                }
            }
            return false;
        }
        void PlayFocusedSwap(int direction)
        {
            var command=Interaction.FocusedCommand(direction);if(command.HasValue)PlayCommand(command.Value);
        }
        void PlayCommand(PuzzleCommand command)
        {
            if(RequireCommandLesson(command))return;
            ReportInput(Interaction.Execute(command));
        }
        void EnsureLessonRoute()
        {
            if(!PendingLesson.HasValue||CurrentScreen!=GameScreen.Playing||Interaction.Attempt.Current.Status!=PuzzleStatus.Playing)return;
            var lesson=PendingLesson.Value;
            if(lesson==MechanicLesson.Gust||lesson==MechanicLesson.Hint||lesson==MechanicLesson.Undo||lesson==MechanicLesson.Toolbag
                ||lesson>=MechanicLesson.Wayfinder&&lesson<=MechanicLesson.LandingLens)return;
            if(LessonRequestCurrent)return;
            if(lessonSearch!=null)return;
            lessonCancellation=new CancellationTokenSource();var token=lessonCancellation.Token;
            var request=Interaction.Attempt.CaptureHintRequest();
            var resolver=Interaction.Attempt.Resolver;var goal=Interaction.Attempt.Goal;var state=Interaction.Attempt.Current;
            int practiceFloor=lessonFloor;
            lessonSearch=Task.Run(()=>(request,LessonRoute.Find(resolver,state,goal,lesson,practiceFloor,cancellationToken:token)),token);
        }
        bool LessonRequestCurrent=>lessonRequest!=null&&lessonRequest.AttemptId==Interaction.Attempt.AttemptId
            &&lessonRequest.StateHash==Interaction.Attempt.CurrentStateHash;
        bool SyncLessonRestriction()
        {
            if(!PendingLesson.HasValue)return Interaction.ClearPracticeRestriction();
            var lesson=PendingLesson.Value;
            if(lesson==MechanicLesson.Undo)return Interaction.RestrictPractice(control:PracticeControl.Undo);
            else if(lesson==MechanicLesson.Hint){
                if(offeredHint?.Result.Command!=null&&Interaction.Attempt.IsCurrent(offeredHint))return Interaction.RestrictPractice(offeredHint.Result.Command);
                return Interaction.RestrictPractice(control:PracticeControl.Hint);
            }
            else if(lesson==MechanicLesson.Toolbag)return Interaction.RestrictPractice(control:PracticeControl.Toolbag);
            else if(lesson>=MechanicLesson.Wayfinder&&lesson<=MechanicLesson.LandingLens)
                return Interaction.RestrictPractice(control:PracticeControl.Tool,tool:(TravelTool)((int)lesson-(int)MechanicLesson.Wayfinder));
            else if(CurrentScreen==GameScreen.Checkpoint&&(lesson==MechanicLesson.Mirror||lesson==MechanicLesson.Rollers||lesson==MechanicLesson.LandingWind))
                return Interaction.RestrictPractice(control:PracticeControl.NextStage);
            // A stamp demonstrates a possible bonus; it must not force the exact
            // solver command. A wrong symbol is a valid player choice and simply
            // leaves the stamp uncollected.
            bool suggestion=lesson<=MechanicLesson.LandingWind||lesson==MechanicLesson.LiveGust||lesson==MechanicLesson.Stamp;
            return Interaction.RestrictPractice(LessonRequestCurrent?lessonSpecial:null,guidanceOnly:suggestion);
        }
        void DeferLessonPractice()
        {
            if(PendingLesson.HasValue){
                deferredLessons.Add((PendingLesson.Value,lessonFloor));
                rehearsedLessons.Remove(PendingLesson.Value);
            }
            ClearLessonPractice();RenderControls();
        }
        bool UpdateLessonPractice(bool foreground)
        {
            if(!PendingLesson.HasValue)return false;
            bool show=IntroductionAnimationAllowed&&!MechanicPopupOpen&&!ConversationOpen&&!zoomOpen
                &&!Interaction.IsPresenting&&!Interaction.IsPreparing
                &&(CurrentScreen==GameScreen.Playing&&Interaction.CanPlay||CurrentScreen==GameScreen.Checkpoint||CurrentScreen==GameScreen.Fail&&PendingLesson==MechanicLesson.Undo);
            tutorialHand.gameObject.SetActive(false);
            if(!show){ClearLessonHighlight();return true;}
            var lesson=PendingLesson.Value;lessonTime+=Mathf.Min(Time.unscaledDeltaTime,.1f);
            if(lesson==MechanicLesson.Gust&&(machine.AmbientGustCount>lessonGust||ReducedMotion&&lessonTime>2.6f)){
                CompleteLessonPractice(lesson);return true;
            }
            if(lessonSearch!=null&&lessonSearch.IsCompleted){
                var task=lessonSearch;lessonSearch=null;lessonCancellation.Dispose();lessonCancellation=null;
                if(task.IsFaulted){var ignored=task.Exception;DeferLessonPractice();return true;}
                else if(!task.IsCanceled&&task.Result.Request.AttemptId==Interaction.Attempt.AttemptId
                    &&task.Result.Request.StateHash==Interaction.Attempt.CurrentStateHash){
                    lessonRequest=task.Result.Request;lessonSpecial=task.Result.Command;
                    if(!lessonSpecial.HasValue){DeferLessonPractice();return true;}
                }
            }
            if(SyncLessonRestriction())RenderControls();EnsureLessonRoute();
            if(IsHoldingToken){ClearLessonHighlight();return true;}
            if(lesson==MechanicLesson.Gust){machine.SetLessonFocus(0,machine.TokenCount-1,.12f);return true;}
            Button control=null;
            if(lesson==MechanicLesson.Undo)control=undo;
            else if(lesson==MechanicLesson.Hint&&offeredHint==null)control=hint;
            else if(lesson==MechanicLesson.Toolbag)control=fieldTools;
            else if(lesson>=MechanicLesson.Wayfinder&&lesson<=MechanicLesson.LandingLens
                &&CurrentScreen==GameScreen.Playing)control=fieldTrayOpen?fieldToolButtons[(int)lesson-(int)MechanicLesson.Wayfinder]:fieldTools;
            else if(CurrentScreen==GameScreen.Checkpoint&&(lesson==MechanicLesson.Descent||lesson==MechanicLesson.Mirror||lesson==MechanicLesson.Rollers||lesson==MechanicLesson.LandingWind))
                control=resultRoot.Find("Nextdelivery")?.GetComponent<Button>();
            float phase=Mathf.Repeat(lessonTime,3.4f),progress=ReducedMotion?0:Mathf.SmoothStep(0,1,Mathf.Clamp01((phase-.75f)/1.0f));
            Vector3 from,to;
            if(control!=null&&control.gameObject.activeInHierarchy){
                var hit=(RectTransform)control.transform;
                from=to=hit.TransformPoint(hit.rect.center);SetLessonControlHighlight(control);machine.SetLessonFocus(-1,-1,0);
            }else{
                ClearLessonControlHighlight();
                var command=Interaction.PracticeCommand;
                if(!command.HasValue){ClearLessonHighlight();return true;}
                if(command.Value.Kind==CommandKind.EarlyDrop){
                    if(drop.gameObject.activeInHierarchy&&drop.interactable){from=to=drop.transform.position;SetLessonControlHighlight(drop);machine.SetLessonFocus(-1,-1,0);}
                    else {ClearLessonHighlight();return true;}
                }else{
                    int first=command.Value.Edge,second=command.Value.Kind==CommandKind.LongReach?command.Value.End:first+1;
                    if(first<0||second>=machine.TokenCount){ClearLessonHighlight();return true;}
                    from=machine.TokenVisualPosition(first)+machine.TokenBodyCentre(first);
                    to=machine.TokenVisualPosition(second)+machine.TokenBodyCentre(second);
                    machine.SetLessonFocus(first,second,ReducedMotion?.20f:.16f+Mathf.Sin(phase*Mathf.PI)*.045f);
                }
            }
            tutorialPointerCanvas.sortingOrder=35;tutorialHand.gameObject.SetActive(tutorialHand.sprite!=null);
            tutorialHand.rectTransform.position=Vector3.Lerp(from,to,progress);
            tutorialHand.rectTransform.localScale=Vector3.one*(ReducedMotion?1:1-.06f*Mathf.Sin(Mathf.Clamp01(phase/.45f)*Mathf.PI));
            tutorialHand.color=new Color(1,1,1,ReducedMotion?1:phase>2.8f?Mathf.Clamp01((3.4f-phase)/.6f):1);
            return true;
        }
        void SetLessonControlHighlight(Button control)
        {
            if(lessonHaloTarget==control&&lessonButtonHalo!=null)return;
            ClearLessonControlHighlight();lessonHaloTarget=control;
            lessonButtonHalo=MapMikaHalo.Attach(this,control.GetComponent<Image>());
        }
        void ClearLessonControlHighlight()
        {
            if(lessonButtonHalo!=null){lessonButtonHalo.gameObject.SetActive(false);Destroy(lessonButtonHalo.gameObject);}
            lessonButtonHalo=null;lessonHaloTarget=null;
        }
        void ClearLessonHighlight(){machine?.SetLessonFocus(-1,-1,0);ClearLessonControlHighlight();}
    }
}
