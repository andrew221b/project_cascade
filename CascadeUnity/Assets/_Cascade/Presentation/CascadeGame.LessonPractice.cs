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
        HintOffer lessonOffer;
        Task<(HintOffer Guide,PuzzleCommand? Special)> lessonSearch;
        PuzzleCommand? lessonSpecial;
        CancellationTokenSource lessonCancellation;
        Image lessonButtonHalo;
        Button lessonHaloTarget;

        void BeginLessonPractice(MechanicLesson lesson)
        {
            ClearLessonPractice();PendingLesson=lesson;lessonFloor=practiceFloorOverride??Interaction.Attempt.Current.ActiveFloor;practiceFloorOverride=null;
            rehearsedLessons[lesson]=lessonFloor;
            lessonGust=machine.AmbientGustCount;lessonTime=0;
            if(lesson==MechanicLesson.Swap){firstGestureShown=true;RetireInstruction("Drag a piece onto its neighbour.");}
            EnsureLessonRoute();
        }
        void ClearLessonPractice()
        {
            PendingLesson=null;CancelLessonRoute();ClearLessonHighlight();
            if(tutorialHand!=null)tutorialHand.gameObject.SetActive(false);
        }
        void CancelLessonRoute()
        {
            lessonCancellation?.Cancel();lessonCancellation?.Dispose();lessonCancellation=null;
            if(lessonSearch!=null)lessonSearch.ContinueWith(t=>{var ignored=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
            lessonSearch=null;lessonOffer=null;lessonSpecial=null;
        }
        void CompleteLessonPractice(MechanicLesson lesson,bool persist=true)
        {
            if(!rehearsedLessons.ContainsKey(lesson))return;
            bool added=!profile.SeenMechanics.Contains(lesson);if(added)profile.SeenMechanics.Add(lesson);
            if(persist)PersistProgress();
            if(persist&&HasSaveError){if(added)profile.SeenMechanics.Remove(lesson);return;}
            rehearsedLessons.Remove(lesson);if(PendingLesson==lesson)ClearLessonPractice();
            needsMechanicScan=Interaction.Attempt.Current.Status==PuzzleStatus.Playing;
        }
        void ObserveLessonTransition(TransitionResult result)
        {
            if(PendingLesson==MechanicLesson.Hint&&offeredHint!=null)CompleteLessonPractice(MechanicLesson.Hint);
            foreach(var taught in new List<KeyValuePair<MechanicLesson,int>>(rehearsedLessons))
                if(taught.Key!=MechanicLesson.Gust&&LessonPractice.CompletedBy(taught.Key,taught.Value,result,Interaction.LastRequestedCommand??PuzzleCommand.Drop))CompleteLessonPractice(taught.Key);
                else if((taught.Key==MechanicLesson.Swap||taught.Key==MechanicLesson.FarSwap)&&result.Accepted
                    &&System.Linq.Enumerable.Any(result.Events,e=>e.Floor==taught.Value&&e.Kind==PuzzleEventKind.FloorExited)){
                    // Skipping this gesture is not proof that it was learned. Retire
                    // only the departed cue; another encounter still teaches it.
                    rehearsedLessons.Remove(taught.Key);if(PendingLesson==taught.Key)ClearLessonPractice();
                    needsMechanicScan=result.State.Status==PuzzleStatus.Playing;
                }
            CancelLessonRoute();
        }
        bool RequireActionLesson(MechanicLesson lesson,int? onFloor=null)
        {
            if(MechanicPopupOpen)return true;
            if(profile==null||profile.SeenMechanics.Contains(lesson)||rehearsedLessons.ContainsKey(lesson))return false;
            OpenMechanicPopup(lesson);practiceFloorOverride=onFloor;return true;
        }
        // Called before a real command. It only pauses; the player must repeat the gesture.
        public bool RequireCommandLesson(PuzzleCommand command)
        {
            if(!Interaction.CanPlay)return false;
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
            if(!Interaction.SelectedSlot.HasValue)return;
            int from=Interaction.SelectedSlot.Value,to=from+direction;if(to<0||to>=slots.Count)return;
            PlayCommand(PuzzleCommand.Swap(Math.Min(from,to)));
        }
        void PlayCommand(PuzzleCommand command)
        {
            if(RequireCommandLesson(command))return;
            ReportInput(Interaction.Execute(command));
        }
        void EnsureLessonRoute()
        {
            if(!PendingLesson.HasValue||CurrentScreen!=GameScreen.Playing||Interaction.Attempt.Current.Status!=PuzzleStatus.Playing)return;
            if(lessonOffer!=null&&Interaction.Attempt.IsCurrent(lessonOffer))return;
            if(lessonSearch!=null)return;
            lessonCancellation=new CancellationTokenSource();var token=lessonCancellation.Token;
            var request=Interaction.Attempt.CaptureHintRequest();
            var lesson=PendingLesson.Value;var resolver=Interaction.Attempt.Resolver;var goal=Interaction.Attempt.Goal;var state=Interaction.Attempt.Current;
            lessonSearch=Task.Run(()=>{
                var guide=AttemptService.CalculateHint(request,cancellationToken:token);PuzzleCommand? special=null;
                if(lesson==MechanicLesson.FarSwap){
                    for(int from=0;from<state.Row.Count&&!special.HasValue;from++)for(int span=2;span<=3&&from+span<state.Row.Count;span++){
                        var step=PuzzleCommand.Reach(from,from+span);var result=resolver.Resolve(state,step);if(!result.Accepted)continue;
                        if(PuzzleSolver.FindShortest(resolver,result.State,goal:goal,stateBudget:4000,cancellationToken:token).Status==SearchStatus.Solved){special=step;break;}
                    }
                }
                return (guide,special);
            },token);
        }
        bool UpdateLessonPractice(bool foreground)
        {
            if(!PendingLesson.HasValue)return false;
            bool show=IntroductionAnimationAllowed&&!MechanicPopupOpen&&!ConversationOpen&&!zoomOpen
                &&!Interaction.IsPresenting&&!Interaction.IsPreparing
                &&(CurrentScreen==GameScreen.Playing&&Interaction.CanPlay||CurrentScreen==GameScreen.Supplies||CurrentScreen==GameScreen.Checkpoint||CurrentScreen==GameScreen.Fail&&PendingLesson==MechanicLesson.Undo);
            tutorialHand.gameObject.SetActive(false);
            if(!show){ClearLessonHighlight();return true;}
            var lesson=PendingLesson.Value;lessonTime+=Mathf.Min(Time.unscaledDeltaTime,.1f);
            if(lesson==MechanicLesson.Gust&&(machine.AmbientGustCount>lessonGust||ReducedMotion&&lessonTime>2.6f)){
                CompleteLessonPractice(lesson);return true;
            }
            if(lessonSearch!=null&&lessonSearch.IsCompleted){
                var task=lessonSearch;lessonSearch=null;lessonCancellation.Dispose();lessonCancellation=null;
                if(task.IsFaulted){var ignored=task.Exception;}else if(!task.IsCanceled&&Interaction.Attempt.IsCurrent(task.Result.Guide)){lessonOffer=task.Result.Guide;lessonSpecial=task.Result.Special;}
            }
            EnsureLessonRoute();
            if(IsHoldingToken){ClearLessonHighlight();return true;}
            if(lesson==MechanicLesson.Gust){machine.SetLessonFocus(0,machine.TokenCount-1,.12f);return true;}
            Button control=null;
            if(lesson==MechanicLesson.Undo)control=undo;
            else if(lesson==MechanicLesson.Hint&&offeredHint==null)control=hint;
            else if(lesson==MechanicLesson.Toolbag)control=fieldTools;
            else if(lesson>=MechanicLesson.Wayfinder&&lesson<=MechanicLesson.LandingLens&&CurrentScreen==GameScreen.Supplies)
                control=shopItem==(int)lesson-(int)MechanicLesson.Wayfinder?FindLessonUseButton():FindLessonButton("Shop object "+((int)lesson-(int)MechanicLesson.Wayfinder));
            else if(CurrentScreen==GameScreen.Checkpoint&&(lesson==MechanicLesson.Descent||lesson==MechanicLesson.Mirror||lesson==MechanicLesson.Rollers||lesson==MechanicLesson.LandingWind))
                control=resultRoot.Find("Nextdelivery")?.GetComponent<Button>();
            float phase=Mathf.Repeat(lessonTime,3.4f),progress=ReducedMotion?0:Mathf.SmoothStep(0,1,Mathf.Clamp01((phase-.75f)/1.0f));
            Vector3 from,to;
            if(control!=null&&control.gameObject.activeInHierarchy){
                from=to=control.transform.position;SetLessonControlHighlight(control);machine.SetLessonFocus(-1,-1,0);
            }else{
                ClearLessonControlHighlight();
                var command=lesson==MechanicLesson.FarSwap?lessonSpecial:lesson==MechanicLesson.Hint&&offeredHint?.Result.Command!=null?offeredHint.Result.Command:lessonOffer?.Result.Command;
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
        Button FindLessonUseButton()=>FindLessonButton("Use selected tool");
        Button FindLessonButton(string name)
        {
            foreach(var button in menuRoot.GetComponentsInChildren<Button>())if(button.name==name)return button;
            return null;
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
