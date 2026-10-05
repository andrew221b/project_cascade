using System;
using Cascade.Domain;

namespace Cascade.Application
{
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
        int? pointer;
        int pressedSlot;

        public PuzzleInteraction(AttemptService attempt) { Attempt = attempt ?? throw new ArgumentNullException(nameof(attempt)); }
        public static bool NeedsPreciseInput(int slots, float usableWidth) => slots >= 7 && usableWidth < 430;
        bool ValidSlot(int slot) => slot >= 0 && slot < Attempt.Current.Row.Count;
        public bool PointerDown(int pointerId, int slot)
        {
            if (!CanPlay || pointer.HasValue || !ValidSlot(slot)) return false;
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
            if (!CanPlay || pointer.HasValue || !ValidSlot(slot)) return null;
            if(SelectedSlot.HasValue && Math.Abs(SelectedSlot.Value-slot)>=2 && Math.Abs(SelectedSlot.Value-slot)<=3 && Attempt.Current.ReachCharges>0) return Execute(PuzzleCommand.Reach(SelectedSlot.Value,slot));
            if (SelectedSlot.HasValue && Math.Abs(SelectedSlot.Value - slot) == 1)
                return Execute(PuzzleCommand.Swap(Math.Min(SelectedSlot.Value, slot)));
            SelectedSlot = SelectedSlot == slot ? (int?)null : slot; return null;
        }
        public void FocusSlot(int slot) { if (CanPlay && !pointer.HasValue && ValidSlot(slot)) SelectedSlot = slot; }
        public CommandPreview PreviewNeighbour(int slot)
        {
            if (!CanPlay || !SelectedSlot.HasValue || Math.Abs(slot - SelectedSlot.Value) != 1) return null;
            return Attempt.Resolver.Preview(Attempt.Current, PuzzleCommand.Swap(Math.Min(slot, SelectedSlot.Value)));
        }
        public TransitionResult SwapFocused(int direction)
        {
            if (!SelectedSlot.HasValue || (direction != -1 && direction != 1)) return null;
            int neighbour = SelectedSlot.Value + direction;
            return !ValidSlot(neighbour) ? null : Execute(PuzzleCommand.Swap(Math.Min(neighbour, SelectedSlot.Value)));
        }
        public TransitionResult Execute(PuzzleCommand command)
        {
            if (!CanPlay || pointer.HasValue) return null;
            LastRequestedCommand = command;
            var result = Attempt.Execute(command);
            if (result.Accepted) Present(result); return result;
        }
        public TransitionResult ApplyHint(HintOffer offer)
        {
            if (!CanPlay || pointer.HasValue || !Attempt.TryApplyHint(offer, out var result)) return null;
            Present(result); return result;
        }
        void Present(TransitionResult result)
        {
            SelectedSlot = null; pointer = null; IsPresenting = true;
            Committed?.Invoke(result);
        }
        public void FinishPresentation() { IsPresenting = false; }
        public bool Undo()
        {
            if (!CanControl || pointer.HasValue || !Attempt.Undo()) return false;
            SelectedSlot = null; return true;
        }
        public bool Retry()
        {
            if (!CanControl || pointer.HasValue) return false;
            SelectedSlot = null; Attempt.Retry(); return true;
        }
        public void SetPaused(bool paused) { IsPaused = paused; CancelPointer(); }
        public void SetSuspended(bool suspended) { IsSuspended = suspended; CancelPointer(); }
        public void SetPreparing(bool preparing) { IsPreparing = preparing; CancelPointer(); }
    }
}
