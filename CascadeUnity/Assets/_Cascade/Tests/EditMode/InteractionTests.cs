using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class InteractionTests
    {
        PuzzleInteraction Input(string id = "C02") => new PuzzleInteraction(new AttemptService(ContentTests.LoadCatalog().GetPrimary(id).Definition));
        [Test] public void TapTapCommitsOneAdjacentExchange()
        {
            var input = Input("C01"); int count = 0; input.Committed += _ => count++;
            input.TapSlot(0); var result = input.TapSlot(1);
            Assert.That(result.Accepted, Is.True); Assert.That(input.Attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(input.Attempt.RouteLength, Is.EqualTo(1)); Assert.That(count, Is.EqualTo(1));
        }
        [TestCase(-1)][TestCase(0)][TestCase(2)]
        public void CancelledAndLongDragNeverSpendDurability(int releaseSlot)
        {
            var input = Input(); var before = input.Attempt.Current; input.PointerDown(7, 0);
            Assert.That(input.PointerUp(7, releaseSlot, true), Is.Null); Assert.That(input.Attempt.Current, Is.SameAs(before));
            Assert.That(input.Attempt.History, Is.Empty);
        }
        [Test] public void AdjacentDragAcceptsOnlyItsCapturedPointer()
        {
            var input = Input(); Assert.That(input.PointerDown(1, 0), Is.True); Assert.That(input.PointerDown(2, 1), Is.False);
            Assert.That(input.PointerUp(2, 1, true), Is.Null); Assert.That(input.PointerUp(1, 1, true).Accepted, Is.True);
            Assert.That(input.PointerUp(1, 1, true), Is.Null); Assert.That(input.Attempt.RouteLength, Is.EqualTo(1));
        }
        [Test] public void PresentationDropsInputWithoutQueueAndDoesNotMutateCore()
        {
            var input = Input(); input.Execute(PuzzleCommand.Swap(0)); var committed = input.Attempt.Current;
            Assert.That(input.Execute(PuzzleCommand.Drop), Is.Null); Assert.That(input.PointerDown(1, 1), Is.False);
            Assert.That(input.Undo(), Is.False); Assert.That(input.Retry(), Is.False);
            input.FinishPresentation(); input.FinishPresentation(); Assert.That(input.Attempt.Current, Is.SameAs(committed));
            Assert.That(input.Attempt.RouteLength, Is.EqualTo(1));
        }
        [Test] public void PauseFocusAndCancelledPointerCannotQueueOrTick()
        {
            var input = Input("C13"); var before = input.Attempt.Current; input.PointerDown(3, 0); input.SetPaused(true);
            Assert.That(input.PointerUp(3, 1, true), Is.Null); Assert.That(input.Execute(PuzzleCommand.Drop), Is.Null);
            input.SetPaused(false); input.SetSuspended(true); Assert.That(input.Execute(PuzzleCommand.Drop), Is.Null);
            input.SetSuspended(false); Assert.That(input.Attempt.Current, Is.SameAs(before));
            Assert.That(input.Attempt.RouteLength, Is.Zero);
        }
        [Test] public void PrecisionPreviewAndExecutionUseTheSameCost()
        {
            var input = Input("C26"); input.FocusSlot(0); var preview = input.PreviewNeighbour(1);
            var prediction = input.Attempt.Predict(PuzzleCommand.Swap(0)); var result = input.SwapFocused(1);
            Assert.That(preview.DurabilityCost, Is.EqualTo(result.DurabilityCost)); Assert.That(result.State, Is.EqualTo(prediction.State));
        }
        [Test] public void InvalidLockedSwapAndNoOpPreserveSelectionAndState()
        {
            var input = Input("C07"); var edge = input.Attempt.Level.Floors[0].OpenEdges.Select((v, i) => new { v, i }).First(p => !p.v).i;
            input.FocusSlot(edge); var before = input.Attempt.Current; var result = input.SwapFocused(1);
            Assert.That(result.Accepted, Is.False); Assert.That(input.Attempt.Current, Is.SameAs(before)); Assert.That(input.IsPresenting, Is.False);
            Assert.That(input.SelectedSlot, Is.EqualTo(edge));
        }
        [TestCase(8,320,true)][TestCase(7,390,true)][TestCase(8,600,false)][TestCase(6,320,false)]
        public void CompactModeDoesNotPretendEightLargeHitAreas(int slots, int width, bool expected)
            => Assert.That(PuzzleInteraction.NeedsPreciseInput(slots, width), Is.EqualTo(expected));
        [Test] public void OrdinaryUndoCannotRestoreAnAwardedDelivery()
        {
            var input = Input("C01"); input.TapSlot(0); input.TapSlot(1); input.FinishPresentation();
            Assert.That(input.CanPlay, Is.False); Assert.That(input.Undo(), Is.False); Assert.That(input.CanPlay, Is.False);
            Assert.That(input.Attempt.RouteLength, Is.EqualTo(1));
        }
    }
}
