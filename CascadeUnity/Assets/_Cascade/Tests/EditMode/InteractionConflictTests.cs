using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Solver;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class InteractionConflictTests
    {
        [Test] public void WinningLanternTransitionPublishesAfterTheAtomicCommit()
        {
            var attempt = new AttemptService(ContentTests.LoadCatalog().GetPrimary("C01").Definition);
            var input = new PuzzleInteraction(attempt);
            var hint = AttemptService.CalculateHint(attempt.CaptureHintRequest());
            Assert.That(hint.Result.Status, Is.EqualTo(HintStatus.Step));
            Assert.That(hint.Result.Command.Value.Kind, Is.EqualTo(CommandKind.Swap));
            Assert.That(attempt.TryUseToolAtomically(TravelTool.SignalLantern, () => true, hint), Is.True);
            Assert.That(attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
            var committed = attempt.Current;
            int publications = 0;
            input.Committed += result => { publications++; Assert.That(result.State, Is.SameAs(committed)); };

            Assert.That(input.PresentToolTransition(attempt.LastToolTransition), Is.True,
                "An already committed winning tool step must still publish completion and presentation events.");
            Assert.That(publications, Is.EqualTo(1));
            Assert.That(input.IsPresenting, Is.True);
            Assert.That(input.PresentToolTransition(attempt.LastToolTransition), Is.False);
            input.FinishPresentation();
            Assert.That(input.PresentToolTransition(attempt.LastToolTransition), Is.False,
                "Finishing the animation must not make an already published tool result publishable again.");
            Assert.That(attempt.Current, Is.SameAs(committed));
            Assert.That(attempt.RouteLength, Is.EqualTo(1));
        }

        [Test] public void AToolTransitionFromBeforeRetryCannotPublishIntoTheNewAttempt()
        {
            var attempt = new AttemptService(LevelDefinitionTests.Level());
            var input = new PuzzleInteraction(attempt);
            var hint = AttemptService.CalculateHint(attempt.CaptureHintRequest());
            Assert.That(attempt.TryUseToolAtomically(TravelTool.SignalLantern, () => true, hint), Is.True);
            var previousTransition = attempt.LastToolTransition;
            Assert.That(input.Retry(), Is.True);
            int publications = 0;
            input.Committed += _ => publications++;

            Assert.That(input.PresentToolTransition(previousTransition), Is.False);
            Assert.That(publications, Is.Zero);
            Assert.That(attempt.RouteLength, Is.Zero);
            Assert.That(input.IsPresenting, Is.False);
        }

        [Test] public void PreparationCancelsHeldReachAndBlocksHintAndPrecisionCommands()
        {
            var attempt = new AttemptService(ReachChallenges.Create()[1].Definition);
            var input = new PuzzleInteraction(attempt);
            var hint = AttemptService.CalculateHint(attempt.CaptureHintRequest());
            var before = attempt.Current;
            input.FocusSlot(0);
            Assert.That(input.PointerDown(7, 0), Is.True);
            input.SetPreparing(true);
            input.FinishPresentation();
            input.SetPaused(false);
            input.SetSuspended(false);

            Assert.That(input.PointerUp(7, 3, true), Is.Null);
            Assert.That(input.Execute(PuzzleCommand.Reach(0, 3)), Is.Null);
            Assert.That(input.ApplyHint(hint), Is.Null);
            Assert.That(input.SwapFocused(1), Is.Null);
            Assert.That(input.Undo(), Is.False);
            Assert.That(input.Retry(), Is.False);
            Assert.That(attempt.Current, Is.SameAs(before));
            Assert.That(attempt.Current.ReachCharges, Is.EqualTo(1));
            Assert.That(attempt.History, Is.Empty);
            Assert.That(attempt.HintUsed, Is.False);
        }
    }
}
