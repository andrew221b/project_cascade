using System.Collections.Generic;
using System.Linq;
using Cascade.Domain;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class BridgeStampTests
    {
        internal static PuzzleResolver C35() => new PuzzleResolver(LevelDefinitionTests.Level(
            LevelDefinitionTests.Row("ACBD"), LevelDefinitionTests.Row("ABCD"),
            new[] { new FloorDefinition(5, new[] { true, true, true }, optionalStamp: new ContactDefinition(0, LevelDefinitionTests.S("C"))) }));
        static PuzzleResolver Bridge(string start = "BACD", string target = "ABCD", bool linked = false, bool locked = false) =>
            new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row(start), LevelDefinitionTests.Row(target),
                new[] { new FloorDefinition(5, new[] { true, true, !locked }, linkedPair: linked ? new LinkedPair(0, 2) : (LinkedPair?)null, oneUseBridgeEdge: 0) }));

        [Test] public void BridgeClosesOnlyAfterAcceptedSwap()
        {
            var resolver = Bridge(); var before = resolver.CreateInitialState();
            var result = resolver.Resolve(before, PuzzleCommand.Swap(0));
            Assert.That(result.State.BridgeUsed, Is.True); Assert.That(before.BridgeUsed, Is.False);
            Assert.That(resolver.Preview(result.State, PuzzleCommand.Swap(0)).Rejection, Is.EqualTo(RejectionReason.ClosedEdge));
            Assert.That(result.Events.Count(e => e.Kind == PuzzleEventKind.BridgeConsumed), Is.EqualTo(1));
        }
        [Test] public void NoOpInvalidAndDropDoNotConsumeBridge()
        {
            var resolver = Bridge("AABC", "AABC"); var before = resolver.CreateInitialState();
            Assert.That(resolver.Resolve(before, PuzzleCommand.Swap(0)).State, Is.SameAs(before));
            Assert.That(resolver.Resolve(before, PuzzleCommand.Swap(-1)).State, Is.SameAs(before));
            Assert.That(resolver.Resolve(before, PuzzleCommand.Drop).Events.Any(e => e.Kind == PuzzleEventKind.BridgeConsumed), Is.False);
        }
        [Test] public void LinkedBridgeClosesAfterCompleteSwapAndLockedSubpairPreservesIt()
        {
            var resolver = Bridge("BADC", "ABCD", true); var before = resolver.CreateInitialState();
            var result = resolver.Resolve(before, PuzzleCommand.Swap(2));
            Assert.That(result.State.Row, Is.EqualTo(LevelDefinitionTests.Row("ABCD"))); Assert.That(result.State.BridgeUsed, Is.True);
            var locked = Bridge("BADC", "ABCD", true, true); var s = locked.CreateInitialState();
            Assert.That(locked.Resolve(s, PuzzleCommand.Swap(0)).State, Is.SameAs(s)); Assert.That(s.BridgeUsed, Is.False);
        }
        [Test] public void SnapshotRestoresBridgeAndStampWithoutChangingTokens()
        {
            var resolver = new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row("BACD"), LevelDefinitionTests.Row("ABCD"),
                new[] { new FloorDefinition(5, new[] { true, true, true }, oneUseBridgeEdge: 0, optionalStamp: new ContactDefinition(0, LevelDefinitionTests.S("A"))) }));
            var before = resolver.CreateInitialState(); var after = resolver.Resolve(before, PuzzleCommand.Swap(0)).State;
            Assert.That(after.BridgeUsed, Is.True); Assert.That(after.StampBits, Is.EqualTo(1));
            var restored = before;
            Assert.That(restored.BridgeUsed, Is.False); Assert.That(restored.StampBits, Is.Zero);
            Assert.That(resolver.Resolve(restored, PuzzleCommand.Swap(0)).State, Is.EqualTo(after));
            Assert.That(LevelValidator.SameMultiset(after.Row, before.Row), Is.True);
        }
        [Test] public void StampDoesNotGateOrdinaryReceiver()
        {
            var resolver = C35(); var s = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(1)).State;
            s = resolver.Resolve(s, PuzzleCommand.Drop).State;
            Assert.That(s.Status, Is.EqualTo(PuzzleStatus.Won)); Assert.That(s.StampBits, Is.Zero);
            Assert.That(PuzzleGoal.Delivery.IsSatisfied(s), Is.True); Assert.That(new PuzzleGoal(1).IsSatisfied(s), Is.False);
        }
        [Test] public void StampRecordsOnceAndEntryTransformCanCollectIt()
        {
            var resolver = new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row("DABC"), LevelDefinitionTests.Row("ABCD"),
                new[] { new FloorDefinition(5, new[] { true, true, true }, entryTransform: EntryTransform.ShiftLeftOne,
                    optionalStamp: new ContactDefinition(0, LevelDefinitionTests.S("A"))) }));
            var s = resolver.CreateInitialState(); Assert.That(s.StampBits, Is.EqualTo(1));
            s = resolver.Resolve(s, PuzzleCommand.Swap(0)).State;
            var result = resolver.Resolve(s, PuzzleCommand.Swap(0));
            Assert.That(result.State.StampBits, Is.EqualTo(1)); Assert.That(result.Events.Any(e => e.Kind == PuzzleEventKind.StampCollected), Is.False);
        }
        [Test] public void C35DeliveryAndStampedMinimaAreTwoAndFour()
        {
            Assert.That(IndependentMinimum(C35(), PuzzleGoal.Delivery), Is.EqualTo(2));
            Assert.That(IndependentMinimum(C35(), new PuzzleGoal(1)), Is.EqualTo(4));
        }
        [Test] public void UnreachableStampHasNoGoalState()
        {
            var resolver = new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row("BACD"), LevelDefinitionTests.Row("ABCD"),
                new[] { new FloorDefinition(1, new[] { true, true, true }, optionalStamp: new ContactDefinition(3, LevelDefinitionTests.S("A"))) }));
            Assert.That(IndependentMinimum(resolver, PuzzleGoal.Delivery), Is.EqualTo(1));
            Assert.That(IndependentMinimum(resolver, new PuzzleGoal(1)), Is.Null);
        }

        // An independent, test-only BFS oracle; production search is introduced in P07.
        internal static int? IndependentMinimum(PuzzleResolver resolver, PuzzleGoal goal)
        {
            var root = resolver.CreateInitialState(); var visited = new HashSet<PuzzleState> { root };
            var queue = new Queue<KeyValuePair<PuzzleState, int>>(); queue.Enqueue(new KeyValuePair<PuzzleState, int>(root, 0));
            while (queue.Count != 0)
            {
                var current = queue.Dequeue(); if (goal.IsSatisfied(current.Key)) return current.Value;
                if (current.Key.Status != PuzzleStatus.Playing) continue;
                for (int edge = -1; edge < current.Key.Row.Count - 1; edge++)
                {
                    var next = resolver.Resolve(current.Key, edge == -1 ? PuzzleCommand.Drop : PuzzleCommand.Swap(edge));
                    if (next.Accepted && visited.Add(next.State)) queue.Enqueue(new KeyValuePair<PuzzleState, int>(next.State, current.Value + 1));
                }
                Assert.That(visited.Count, Is.LessThan(20000), "Fixture graph unexpectedly exceeded test budget.");
            }
            return null;
        }
    }
}
