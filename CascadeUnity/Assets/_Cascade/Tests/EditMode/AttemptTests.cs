using System;
using System.Linq;
using System.Threading;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Solver;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class AttemptTests
    {
        static AttemptService Attempt(string id) => new AttemptService(ReferenceFixtures.Parse((JObject)ReferenceFixtures.Levels.Single(l => (string)l["id"] == id)));
        static HintOffer Hint(AttemptService attempt, int budget = PuzzleSolver.DefaultStateBudget) => AttemptService.CalculateHint(attempt.CaptureHintRequest(), budget);

        [Test] public void InvalidCommandsAndPredictionDoNotChangeHistory()
        {
            var attempt = Attempt("C01"); var before = attempt.Current;
            Assert.That(attempt.Predict(PuzzleCommand.Swap(0)).State.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(attempt.Current, Is.SameAs(before)); Assert.That(attempt.History, Is.Empty);
            attempt.Execute(default); attempt.Execute(PuzzleCommand.Swap(-1));
            Assert.That(attempt.RouteLength, Is.Zero); Assert.That(attempt.UndoCount, Is.Zero); Assert.That(attempt.History, Is.Empty);
        }
        [Test] public void RouteLengthRollsBackButUndoCountIsSeparate()
        {
            var attempt = Attempt("C02"); attempt.Execute(PuzzleCommand.Swap(1)); attempt.Execute(PuzzleCommand.Swap(0));
            Assert.That(attempt.RouteLength, Is.EqualTo(2)); Assert.That(attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(attempt.Undo(), Is.True); Assert.That(attempt.RouteLength, Is.EqualTo(1)); Assert.That(attempt.UndoCount, Is.EqualTo(1));
            Assert.That(attempt.Undo(), Is.True); Assert.That(attempt.RouteLength, Is.Zero); Assert.That(attempt.UndoCount, Is.EqualTo(2));
            Assert.That(attempt.Undo(), Is.False); Assert.That(attempt.UndoCount, Is.EqualTo(2));
        }
        [Test] public void UndoAcrossCollapseRestoresBombCountersAndOldFloor()
        {
            var level = LevelDefinitionTests.Level(LevelDefinitionTests.Row("BACD"), LevelDefinitionTests.Row("ABCD"), new[]
            { new FloorDefinition(1, new[] { true, true, true }, bomb: new BombDefinition(3, LevelDefinitionTests.S("A"), 1, 4)),
              new FloorDefinition(3, new[] { true, true, true }, entryTransform: EntryTransform.ShiftRightOne) });
            var attempt = new AttemptService(level); var before = attempt.Current; string hash = attempt.CurrentStateHash;
            var after = attempt.Execute(PuzzleCommand.Swap(0)).State;
            Assert.That(after.ActiveFloor, Is.EqualTo(1)); Assert.That(after.ExplodedCount, Is.EqualTo(1));
            attempt.Undo(); Assert.That(attempt.Current, Is.SameAs(before)); Assert.That(attempt.CurrentStateHash, Is.EqualTo(hash));
            Assert.That(attempt.Current.BombTicks, Is.EqualTo(1)); Assert.That(attempt.Current.ExplodedCount, Is.Zero);
            Assert.That(attempt.Execute(PuzzleCommand.Swap(0)).State, Is.EqualTo(after));
        }
        [Test] public void ActualUndoRestoresBridgeStampKeyRepairAndDefuse()
        {
            var a = LevelDefinitionTests.S("A");
            var level = LevelDefinitionTests.Level(LevelDefinitionTests.Row("BACD"), LevelDefinitionTests.Row("ABCD"),
                new[] { new FloorDefinition(5, new[] { true, true, false }, new[] { 0 }, bomb: new BombDefinition(0, a, 2, 2),
                    key: new KeyDefinition(0, a, 2), repair: new RepairDefinition(0, a, 1), oneUseBridgeEdge: 0, optionalStamp: new ContactDefinition(0, a)) });
            var attempt = new AttemptService(level); var before = attempt.Current;
            var after = attempt.Execute(PuzzleCommand.Swap(0)).State;
            Assert.That(after.BridgeUsed && after.KeyUsed && after.RepairUsed, Is.True);
            Assert.That(after.StampBits, Is.EqualTo(1)); Assert.That(after.DefusedCount, Is.EqualTo(1));
            attempt.Undo(); Assert.That(attempt.Current, Is.SameAs(before));
            Assert.That(attempt.Current.BridgeUsed || attempt.Current.KeyUsed || attempt.Current.RepairUsed, Is.False);
            Assert.That(attempt.Current.StampBits, Is.Zero); Assert.That(attempt.Current.BombPhase, Is.EqualTo(BombPhase.Armed));
        }
        [Test] public void DeadEndSuggestsUndoAndUndoRestoresPlayable()
        {
            var attempt = Attempt("C01"); attempt.Execute(PuzzleCommand.Swap(1));
            var hint = Hint(attempt); Assert.That(hint.Result.Status, Is.EqualTo(HintStatus.DeadEnd));
            Assert.That(hint.Result.Command, Is.Null); Assert.That(attempt.RevealHint(hint), Is.True);
            Assert.That(attempt.CanUndo, Is.True); attempt.Undo();
            Assert.That(attempt.Current.Status, Is.EqualTo(PuzzleStatus.Playing)); Assert.That(Hint(attempt).Result.Status, Is.EqualTo(HintStatus.Step));
        }
        [Test] public void UnknownDoesNotConsumeHintOrHistory()
        {
            var attempt = Attempt("C35"); var before = attempt.Current; var hint = Hint(attempt, 1);
            Assert.That(hint.Result.Status, Is.EqualTo(HintStatus.Unknown)); Assert.That(attempt.RevealHint(hint), Is.True);
            Assert.That(attempt.HintUsed, Is.False); Assert.That(attempt.RevealedHintStates, Is.Empty);
            Assert.That(attempt.Current, Is.SameAs(before)); Assert.That(attempt.RouteLength, Is.Zero); Assert.That(attempt.History, Is.Empty);
        }
        [Test] public void StaleHintCannotBeRevealedOrExecuted()
        {
            var attempt = Attempt("C35"); var request = attempt.CaptureHintRequest(); attempt.Execute(PuzzleCommand.Swap(1));
            var old = AttemptService.CalculateHint(request); var current = attempt.Current; int length = attempt.RouteLength;
            Assert.That(attempt.RevealHint(old), Is.False); Assert.That(attempt.TryApplyHint(old, out _), Is.False);
            Assert.That(attempt.Current, Is.SameAs(current)); Assert.That(attempt.RouteLength, Is.EqualTo(length)); Assert.That(attempt.HintUsed, Is.False);
        }
        [Test] public void RevealedHintIsMonotonicAcrossUndoAndCanBeReusedAtSameHash()
        {
            var attempt = Attempt("C35"); var hint = Hint(attempt); string hash = attempt.CurrentStateHash;
            Assert.That(attempt.TryApplyHint(hint, out var step), Is.True); Assert.That(step.Accepted, Is.True); Assert.That(attempt.HintUsed, Is.True);
            attempt.Undo(); Assert.That(attempt.HintUsed, Is.True); Assert.That(attempt.CurrentStateHash, Is.EqualTo(hash));
            Assert.That(attempt.RevealedHintStates.ContainsKey(hash), Is.True); Assert.That(attempt.TryApplyHint(hint, out _), Is.True);
        }
        [Test] public void RetryPinsSameVersionButCreatesNewAttemptAndRejectsOldHint()
        {
            var attempt = Attempt("C35"); var oldId = attempt.AttemptId; var hint = Hint(attempt); var original = attempt.Current;
            var level = attempt.Level; attempt.RevealHint(hint); attempt.Execute(PuzzleCommand.Swap(1)); attempt.Undo(); attempt.Retry();
            Assert.That(attempt.AttemptId, Is.Not.EqualTo(oldId)); Assert.That(attempt.Level, Is.SameAs(level));
            Assert.That(attempt.Current, Is.EqualTo(original)); Assert.That(attempt.PinnedContentHash, Is.EqualTo(level.ContentHash));
            Assert.That(attempt.RouteLength, Is.Zero); Assert.That(attempt.UndoCount, Is.Zero); Assert.That(attempt.HintUsed, Is.False);
            Assert.That(attempt.RevealedHintStates, Is.Empty); Assert.That(attempt.TryApplyHint(hint, out _), Is.False);
        }
        [Test] public void HintUsesActiveContractGoal()
        {
            var delivery = new AttemptService(BridgeStampTests.C35().Level); var stamped = new AttemptService(delivery.Level, goal: new PuzzleGoal(1));
            Assert.That(Hint(delivery).Result.Command, Is.EqualTo(PuzzleCommand.Swap(1)));
            Assert.That(Hint(stamped).Result.Command, Is.EqualTo(PuzzleCommand.Swap(0)));
            Assert.That(Hint(stamped).Result.Search.OptimalActions, Is.EqualTo(4));
        }
        [Test] public void EquivalentCoreStatesHaveSameHashAcrossAttemptsAndCommands()
        {
            var first = Attempt("C25"); var second = Attempt("C25");
            Assert.That(first.AttemptId, Is.Not.EqualTo(second.AttemptId)); Assert.That(first.CurrentStateHash, Is.EqualTo(second.CurrentStateHash));
            first.Execute(PuzzleCommand.Swap(0)); second.Execute(PuzzleCommand.Swap(2));
            Assert.That(first.CurrentStateHash, Is.EqualTo(second.CurrentStateHash));
            Assert.That(first.CurrentStateHash.Length, Is.EqualTo(64));
        }
        [Test] public void PuzzleLocaleAndModeAreBoundAtAttemptStart()
        {
            var glyphs = new[] { SymbolId.Parse("g:uk:К"), SymbolId.Parse("g:uk:І"), SymbolId.Parse("g:uk:Т") };
            var level = LevelDefinitionTests.Level(glyphs.Reverse().ToArray(), glyphs, segments: new[] { new TargetSegment(SegmentKind.Word, 0, 3, "uk_КІТ") });
            var attempt = new AttemptService(level, "uk"); Assert.That(attempt.ActiveLocale, Is.EqualTo("uk")); Assert.That(attempt.Mode, Is.EqualTo(PuzzleMode.Words));
            Assert.Throws<ArgumentException>(() => new AttemptService(level, "en"));
            Assert.Throws<ArgumentException>(() => new AttemptService(level, "ua"));
        }
        [Test] public void CancelledWorkerQueryIsUnknownAndDoesNotMutateAttempt()
        {
            var attempt = Attempt("C35"); var before = attempt.Current;
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel(); var offer = AttemptService.CalculateHint(attempt.CaptureHintRequest(), cancellationToken: cancellation.Token);
                Assert.That(offer.Result.Status, Is.EqualTo(HintStatus.Unknown)); Assert.That(attempt.Current, Is.SameAs(before));
            }
        }
    }
}
