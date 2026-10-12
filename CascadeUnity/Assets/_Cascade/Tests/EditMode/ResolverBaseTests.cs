using System;
using System.Linq;
using Cascade.Domain;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class ResolverBaseTests
    {
        static PuzzleResolver Resolver(string start = "BAC", string target = "ABC", int hp = 1, bool[] edges = null, int floorCount = 1) =>
            new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row(start), LevelDefinitionTests.Row(target),
                Enumerable.Range(0, floorCount).Select(_ => new FloorDefinition(hp, edges ?? Enumerable.Repeat(true, start.Length - 1).ToArray())).ToArray()));

        [Test] public void LastSwapPreservesWholeRowAndWins()
        {
            var resolver = Resolver(); var before = resolver.CreateInitialState();
            var result = resolver.Resolve(before, PuzzleCommand.Swap(0));
            Assert.That(result.Accepted, Is.True); Assert.That(result.DurabilityCost, Is.EqualTo(1));
            Assert.That(result.State.Row, Is.EqualTo(LevelDefinitionTests.Row("ABC")));
            Assert.That(result.State.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(before.Row, Is.EqualTo(LevelDefinitionTests.Row("BAC")));
            Assert.That(result.Events.Select(e => e.Kind), Is.EqualTo(new[] { PuzzleEventKind.CommandAccepted, PuzzleEventKind.RowPermuted,
                PuzzleEventKind.DurabilityResolved, PuzzleEventKind.FloorExited, PuzzleEventKind.ReceiverEvaluated }));
        }
        [Test] public void TargetOnPlatformDoesNotWinUntilReceiver()
        {
            var resolver = Resolver(hp: 3); var s = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(0)).State;
            Assert.That(s.Status, Is.EqualTo(PuzzleStatus.Playing)); Assert.That(s.Durability, Is.EqualTo(2));
            var drop = resolver.Resolve(s, PuzzleCommand.Drop);
            Assert.That(drop.Accepted, Is.True); Assert.That(drop.DurabilityCost, Is.Zero);
            Assert.That(drop.State.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(drop.Events.Count(e => e.Kind == PuzzleEventKind.CommandAccepted), Is.EqualTo(1));
            Assert.That(drop.Events.Count(e => e.Kind == PuzzleEventKind.RowPermuted), Is.Zero);
        }
        [Test] public void ReceiverComparesEverySlot()
        { var resolver = Resolver("ACB"); Assert.That(resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Drop).State.Status, Is.EqualTo(PuzzleStatus.Lost)); }
        [Test] public void InvalidAndClosedCommandsAreFreeAndReturnSameSnapshot()
        {
            var resolver = Resolver(edges: new[] { false, true }); var s = resolver.CreateInitialState();
            foreach (var command in new[] { default(PuzzleCommand), PuzzleCommand.Swap(-1), PuzzleCommand.Swap(2), PuzzleCommand.Swap(0) })
            {
                var result = resolver.Resolve(s, command);
                Assert.That(result.Accepted, Is.False); Assert.That(result.State, Is.SameAs(s));
                Assert.That(result.Events, Is.Empty); Assert.That(result.DurabilityCost, Is.Zero);
            }
        }
        [Test] public void EqualPairIsFree()
        {
            var resolver = Resolver("AAB", "AAB"); var s = resolver.CreateInitialState();
            var result = resolver.Resolve(s, PuzzleCommand.Swap(0));
            Assert.That(result.Rejection, Is.EqualTo(RejectionReason.NoOp)); Assert.That(result.State, Is.SameAs(s));
        }
        [Test] public void TerminalSnapshotsRejectFurtherCommands()
        {
            var resolver = Resolver(); var s = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(0)).State;
            foreach (var command in new[] { PuzzleCommand.Drop, PuzzleCommand.Swap(0) })
                Assert.That(resolver.Resolve(s, command).State, Is.SameAs(s));
        }
        [Test] public void FourFloorsKeepOnePersistentRowAndResetLocalDurability()
        {
            var resolver = Resolver(hp: 1, floorCount: 4); var s = resolver.CreateInitialState();
            s = resolver.Resolve(s, PuzzleCommand.Swap(0)).State;
            Assert.That(s.ActiveFloor, Is.EqualTo(1)); Assert.That(s.Durability, Is.EqualTo(1));
            for (int i = 0; i < 3; i++) s = resolver.Resolve(s, PuzzleCommand.Drop).State;
            Assert.That(s.Status, Is.EqualTo(PuzzleStatus.Won)); Assert.That(s.ActiveFloor, Is.EqualTo(4));
            Assert.That(s.Row, Is.EqualTo(LevelDefinitionTests.Row("ABC")));
        }
        [Test] public void SameInputGivesEqualStateAndStableEvents()
        {
            var resolver = Resolver(hp: 2); var s = resolver.CreateInitialState();
            var first = resolver.Resolve(s, PuzzleCommand.Swap(0)); var second = resolver.Resolve(s, PuzzleCommand.Swap(0));
            Assert.That(first.State, Is.EqualTo(second.State)); Assert.That(first.State.GetHashCode(), Is.EqualTo(second.State.GetHashCode()));
            Assert.That(first.Events.Select(e => e.Kind), Is.EqualTo(second.Events.Select(e => e.Kind)));
        }
        [Test] public void RandomCommandsConserveMultisetAndStableDurability()
        {
            var random = new Random(109);
            for (int attempt = 0; attempt < 60; attempt++)
            {
                var resolver = Resolver("BACDEFGH", "ABCDEFGH", 12, floorCount: 4); var s = resolver.CreateInitialState();
                for (int action = 0; action < 70; action++)
                {
                    var next = resolver.Resolve(s, random.Next(8) == 0 ? PuzzleCommand.Drop : PuzzleCommand.Swap(random.Next(-1, 9))).State;
                    Assert.That(LevelValidator.SameMultiset(next.Row, resolver.Level.TargetSymbolIds), Is.True);
                    Assert.That(next.Status != PuzzleStatus.Playing || next.Durability > 0, Is.True);
                    s = next;
                }
            }
        }
        [Test] public void StateFromDifferentMultisetIsRejected()
        {
            var resolver = Resolver(); var foreign = Resolver("DEF", "DEF").CreateInitialState();
            Assert.That(resolver.Resolve(foreign, PuzzleCommand.Drop).Rejection, Is.EqualTo(RejectionReason.InvalidState));
        }
    }
}
