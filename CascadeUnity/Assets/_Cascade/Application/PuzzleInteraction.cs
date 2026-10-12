using System;
using Cascade.Domain;

namespace Cascade.Application
{
    public enum PracticeControl { None, Undo, Hint, Toolbag, Tool, NextStage }
    // One pointer, one committed command. Presentation only opens/closes the input gate.
    public sealed class PuzzleInteraction
    {
        public AttemptService Attempt { get; }
        public int? SelectedSlot { get; private set; }
        public bool IsPresenting { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsSuspended { get; private set; }
        public bool IsPreparing { get; private set; }
        public PuzzleCommand? LastRequestedCommand { get; private set; }
        public bool CanControl => !IsPresenting && !IsPaused && !IsSuspended && !IsPreparing;
        public bool CanPlay => CanControl && Attempt.Current.Status == PuzzleStatus.Playing;
        public event Action<TransitionResult> Committed;
        public bool PracticeRestricted { get; private set; }
        public bool PracticeGuidanceOnly { get; private set; }
        public PuzzleCommand? PracticeCommand { get; private set; }
        public PracticeControl PracticeAction { get; private set; }
        PuzzleState practiceState;
        TravelTool? practiceTool;
        int? pointer;
        int pressedSlot;
        TransitionResult publishedToolTransition;

        public PuzzleInteraction(AttemptService attempt) { Attempt = attempt ?? throw new ArgumentNullException(nameof(attempt)); }
        public static bool NeedsPreciseInput(int slots, float usableWidth) => slots >= 7 && usableWidth < 430;
        public bool RestrictPractice(PuzzleCommand? command = null, PracticeControl control = PracticeControl.None, TravelTool? tool = null, bool guidanceOnly = false)
        {
            if(PracticeRestricted && ReferenceEquals(practiceState,Attempt.Current)
                && Nullable.Equals(PracticeCommand,command) && PracticeAction==control && practiceTool==tool&&PracticeGuidanceOnly==guidanceOnly)return false;
            bool capture=guidanceOnly&&(!PracticeRestricted||PracticeGuidanceOnly);
            PracticeRestricted=true;PracticeGuidanceOnly=guidanceOnly;practiceState=Attempt.Current;PracticeCommand=command;PracticeAction=control;practiceTool=tool;
            if(!capture){CancelPointer();SelectedSlot=null;}return true;
        }
        public bool ClearPracticeRestriction()
        {
            bool changed=PracticeRestricted;
            PracticeRestricted=PracticeGuidanceOnly=false;practiceState=null;PracticeCommand=null;PracticeAction=PracticeControl.None;practiceTool=null;
            return changed;
        }
        bool CurrentPractice => ReferenceEquals(practiceState,Attempt.Current);
        public bool AllowsCommand(PuzzleCommand command) => !PracticeRestricted || PracticeGuidanceOnly || CurrentPractice && PracticeCommand.HasValue && PracticeCommand.Value.Equals(command);
        public bool AllowsControl(PracticeControl control) => !PracticeRestricted || PracticeGuidanceOnly || CurrentPractice && control!=PracticeControl.None
            && (PracticeAction==control || control==PracticeControl.Toolbag && PracticeAction==PracticeControl.Tool);
        public bool AllowsTool(TravelTool tool) => !PracticeRestricted || PracticeGuidanceOnly || AllowsControl(PracticeControl.Tool) && practiceTool==tool;
        public bool AllowsRetry => !PracticeRestricted || PracticeGuidanceOnly;
        public bool AllowsSlot(int slot) => !PracticeRestricted || PracticeGuidanceOnly || CurrentPractice && PracticeCommand.HasValue
            && PracticeCommand.Value.Kind!=CommandKind.EarlyDrop && (slot==PracticeCommand.Value.Edge
                ||slot==(PracticeCommand.Value.Kind==CommandKind.LongReach?PracticeCommand.Value.End:PracticeCommand.Value.Edge+1));
        bool ValidSlot(int slot) => slot >= 0 && slot < Attempt.Current.Row.Count;
        public bool PointerDown(int pointerId, int slot)
        {
            if (!CanPlay || pointer.HasValue || !ValidSlot(slot) || !AllowsSlot(slot)) return false;
            pointer = pointerId; pressedSlot = slot; return true;
        }
        public TransitionResult PointerUp(int pointerId, int releaseSlot, bool dragged)
        {
            if (pointer != pointerId) return null;
            int first = pressedSlot; pointer = null;
            if (!CanPlay || !ValidSlot(releaseSlot)) return null;
            if (dragged)
            {
                int span=Math.Abs(releaseSlot-first);
                if(span==1)return Execute(PuzzleCommand.Swap(Math.Min(first,releaseSlot)));
                return span>=2 && span<=3 && Attempt.Current.ReachCharges>0 ? Execute(PuzzleCommand.Reach(first,releaseSlot)) : null;
            }
            return TapSlot(releaseSlot);
        }
        public void CancelPointer() { pointer = null; }
        public bool OwnsPointer(int pointerId) => pointer == pointerId;
        public TransitionResult TapSlot(int slot)
        {
            if (!CanPlay || pointer.HasValue || !ValidSlot(slot) || !AllowsSlot(slot)) return null;
            if(SelectedSlot.HasValue && Math.Abs(SelectedSlot.Value-slot)>=2 && Math.Abs(SelectedSlot.Value-slot)<=3 && Attempt.Current.ReachCharges>0) return Execute(PuzzleCommand.Reach(SelectedSlot.Value,slot));
            if (SelectedSlot.HasValue && Math.Abs(SelectedSlot.Value - slot) == 1)
                return Execute(PuzzleCommand.Swap(Math.Min(SelectedSlot.Value, slot)));
            SelectedSlot = SelectedSlot == slot ? (int?)null : slot; return null;
        }
        public void FocusSlot(int slot) { if (CanPlay && !pointer.HasValue && ValidSlot(slot) && AllowsSlot(slot)) SelectedSlot = slot; }
        public CommandPreview PreviewNeighbour(int slot)
        {
            if (!CanPlay || !SelectedSlot.HasValue || Math.Abs(slot - SelectedSlot.Value) != 1) return null;
            var command=PuzzleCommand.Swap(Math.Min(slot,SelectedSlot.Value));
            return AllowsCommand(command)?Attempt.Resolver.Preview(Attempt.Current,command):null;
        }
        public PuzzleCommand? FocusedCommand(int direction)
        {
            if (!SelectedSlot.HasValue || (direction != -1 && direction != 1)) return null;
            if(PracticeRestricted&&PracticeCommand?.Kind==CommandKind.LongReach){
                var required=PracticeCommand.Value;
                return SelectedSlot==required.Edge&&direction==1||SelectedSlot==required.End&&direction==-1?required:(PuzzleCommand?)null;
            }
            int neighbour = SelectedSlot.Value + direction;
            return !ValidSlot(neighbour) ? null : PuzzleCommand.Swap(Math.Min(neighbour, SelectedSlot.Value));
        }
        public TransitionResult SwapFocused(int direction)
        {
            var command=FocusedCommand(direction);return command.HasValue?Execute(command.Value):null;
        }
        public TransitionResult Execute(PuzzleCommand command)
        {
            if (!CanPlay || pointer.HasValue || !AllowsCommand(command)) return null;
            LastRequestedCommand = command;
            var result = Attempt.Execute(command);
            if (result.Accepted) Present(result); return result;
        }
        void Present(TransitionResult result)
        {
            SelectedSlot = null; pointer = null; IsPresenting = true;
            Committed?.Invoke(result);
        }
        // Publish only the exact result already committed by the atomic inventory transaction.
        public bool PresentToolTransition(TransitionResult result)
        {
            if(!CanControl||pointer.HasValue||result==null||!result.Accepted
                ||!ReferenceEquals(result,Attempt.LastToolTransition)||ReferenceEquals(result,publishedToolTransition)
                ||!ReferenceEquals(result.State,Attempt.Current))return false;
            publishedToolTransition=result;Present(result);return true;
        }
        public void FinishPresentation() { IsPresenting = false; }
        // The snapshot was already restored by Undo; this gate protects only its picture.
        public void BeginRestorePresentation() { IsPresenting = true; CancelPointer(); SelectedSlot = null; }
        public bool Undo()=>Undo(false);
        public bool Undo(bool firstLesson)
        {
            if (!CanControl || pointer.HasValue || !AllowsControl(PracticeControl.Undo)) return false;
            bool rehearsal=firstLesson&&PracticeRestricted&&!PracticeGuidanceOnly&&CurrentPractice&&PracticeAction==PracticeControl.Undo;
            if(!(rehearsal?Attempt.UndoFirstLesson():Attempt.Undo()))return false;
            SelectedSlot = null; return true;
        }
        public bool Retry()
        {
            if (!CanControl || pointer.HasValue || !AllowsRetry) return false;
            SelectedSlot = null; Attempt.Retry(); return true;
        }
        public void SetPaused(bool paused) { IsPaused = paused; CancelPointer(); }
        public void SetSuspended(bool suspended) { IsSuspended = suspended; CancelPointer(); }
        public void SetPreparing(bool preparing) { IsPreparing = preparing; CancelPointer(); }
    }
}
