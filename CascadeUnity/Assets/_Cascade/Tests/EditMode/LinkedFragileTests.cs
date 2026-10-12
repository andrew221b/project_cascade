using System.Linq;
using Cascade.Domain;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class LinkedFragileTests
    {
        static PuzzleResolver Resolver(string start = "BADC", string target = "ABCD", int hp = 1,
            bool[] edges = null, int[] fragile = null, KeyDefinition key = null, BombDefinition bomb = null)
            => new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row(start), LevelDefinitionTests.Row(target),
                new[] { new FloorDefinition(hp, edges ?? new[] { true, true, true }, fragile, new LinkedPair(0, 2), key: key, bomb: bomb) }));

        [TestCase("C25", "BADC", "ABCD", 1, false)]
        [TestCase("C26", "AABA", "AAAB", 1, false)]
        [TestCase("C28", "BADC", "ABCD", 3, true)]
        public void CanonicalLinkedFixturesWin(string id, string start, string target, int hp, bool fragile)
        {
            var resolver = Resolver(start, target, hp, fragile: fragile ? new[] { 0, 2 } : null);
            var before = resolver.CreateInitialState(); var preview = resolver.Preview(before, PuzzleCommand.Swap(0));
            var result = resolver.Resolve(before, PuzzleCommand.Swap(0));
            Assert.That(result.State.Status, Is.EqualTo(PuzzleStatus.Won), id);
            Assert.That(result.State.Row, Is.EqualTo(LevelDefinitionTests.Row(target)), id);
            Assert.That(result.DurabilityCost, Is.EqualTo(fragile ? 3 : 1));
            Assert.That(result.DurabilityCost, Is.EqualTo(preview.DurabilityCost));
            Assert.That(preview.AffectedEdges, Is.EqualTo(new[] { 0, 2 }));
        }
        [Test] public void C27MustUnlockBeforeLinkedExchange()
        {
            var resolver = Resolver(hp: 4, edges: new[] { false, true, true }, key: new KeyDefinition(1, LevelDefinitionTests.S("D"), 0));
            var s = resolver.CreateInitialState(); Assert.That(resolver.Resolve(s, PuzzleCommand.Swap(2)).State, Is.SameAs(s));
            foreach (var command in new[] { PuzzleCommand.Swap(1), PuzzleCommand.Swap(1), PuzzleCommand.Swap(0), PuzzleCommand.Drop })
                s = resolver.Resolve(s, command).State;
            Assert.That(s.Status, Is.EqualTo(PuzzleStatus.Won));
        }
        [Test] public void OneLockedSubpairRejectsWholeAtomicCommand()
        {
            var resolver = Resolver(hp: 3, edges: new[] { true, true, false }); var s = resolver.CreateInitialState();
            var result = resolver.Resolve(s, PuzzleCommand.Swap(0));
            Assert.That(result.Accepted, Is.False); Assert.That(result.State, Is.SameAs(s));
            Assert.That(s.Row, Is.EqualTo(LevelDefinitionTests.Row("BADC"))); Assert.That(result.Events, Is.Empty);
        }
        [Test] public void BothLinkedCommandsProduceIdenticalStateAndCost()
        {
            var resolver = Resolver(hp: 5, fragile: new[] { 0, 2 }); var s = resolver.CreateInitialState();
            var first = resolver.Resolve(s, PuzzleCommand.Swap(0)); var second = resolver.Resolve(s, PuzzleCommand.Swap(2));
            Assert.That(first.State, Is.EqualTo(second.State)); Assert.That(first.DurabilityCost, Is.EqualTo(second.DurabilityCost));
            Assert.That(first.Events.Select(e => e.Kind), Is.EqualTo(second.Events.Select(e => e.Kind)));
        }
        [Test] public void EqualSubpairStillCostsItsFragilityAndOneBombTick()
        {
            var resolver = Resolver("AABA", "AAAB", 5, fragile: new[] { 0, 2 }, bomb: new BombDefinition(2, LevelDefinitionTests.S("A"), 3, 2));
            var s = resolver.CreateInitialState(); var result = resolver.Resolve(s, PuzzleCommand.Swap(0));
            Assert.That(result.DurabilityCost, Is.EqualTo(3)); Assert.That(result.State.Durability, Is.EqualTo(2));
            // The bomb sees the final row and defuses, rather than ticking between sub-swaps.
            Assert.That(result.State.DefusedCount, Is.EqualTo(1));
            Assert.That(result.Events.Count(e => e.Kind == PuzzleEventKind.CommandAccepted), Is.EqualTo(1));
            Assert.That(result.Events.Count(e => e.Kind == PuzzleEventKind.RowPermuted), Is.EqualTo(1));
        }
        [Test] public void LinkedCommandTicksArmedBombOnce()
        {
            var resolver = Resolver(hp: 5, bomb: new BombDefinition(3, LevelDefinitionTests.S("A"), 3, 2));
            var result = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(0));
            Assert.That(result.State.BombTicks, Is.EqualTo(2));
            Assert.That(result.Events.Count(e => e.Kind == PuzzleEventKind.BombTicked), Is.EqualTo(1));
        }
        [Test] public void WholeLinkedNoOpIsFree()
        {
            var resolver = Resolver("AABB", "AABB", 5, fragile: new[] { 0, 2 }); var s = resolver.CreateInitialState();
            Assert.That(resolver.Resolve(s, PuzzleCommand.Swap(0)).State, Is.SameAs(s));
            Assert.That(resolver.Preview(s, PuzzleCommand.Swap(0)).Rejection, Is.EqualTo(RejectionReason.NoOp));
        }
        [Test] public void UnlinkedMiddleEdgeIsAnOrdinarySwap()
        {
            var resolver = Resolver(hp: 5, fragile: new[] { 0, 2 }); var s = resolver.CreateInitialState();
            var result = resolver.Resolve(s, PuzzleCommand.Swap(1));
            Assert.That(result.DurabilityCost, Is.EqualTo(1)); Assert.That(result.State.Row, Is.EqualTo(LevelDefinitionTests.Row("BDAC")));
        }
    }
}
