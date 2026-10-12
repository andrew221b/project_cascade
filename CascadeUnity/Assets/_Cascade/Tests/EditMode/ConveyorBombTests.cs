using System.Linq;
using Cascade.Domain;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class ConveyorBombTests
    {
        static PuzzleResolver Resolver(int hp = 2, BombDefinition bomb = null,
            EntryTransform transform = EntryTransform.None, string start = "BACD", FloorDefinition second = null)
        {
            var first = new FloorDefinition(hp, new[] { true, true, true }, entryTransform: transform, bomb: bomb);
            return new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row(start), LevelDefinitionTests.Row("ABCD"),
                second == null ? new[] { first } : new[] { first, second }));
        }
        static BombDefinition Bomb(int slot, int ticks = 2, int damage = 2) => new BombDefinition(slot, LevelDefinitionTests.S("A"), ticks, damage);

        [Test] public void DefuseOnLastTickPrecedesExplosion()
        {
            var resolver = Resolver(bomb: Bomb(0, 1));
            var result = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(0));
            Assert.That(result.State.BombPhase, Is.EqualTo(BombPhase.Defused));
            Assert.That(result.State.DefusedCount, Is.EqualTo(1)); Assert.That(result.State.ExplodedCount, Is.Zero);
            Assert.That(result.State.Durability, Is.EqualTo(1));
            Assert.That(result.Events.Any(e => e.Kind == PuzzleEventKind.BombExploded || e.Kind == PuzzleEventKind.BombTicked), Is.False);
        }
        [Test] public void UsefulBlastDeliversTargetAndPreservesSymbols()
        {
            var resolver = Resolver(hp: 3, bomb: Bomb(3, 1));
            var result = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(0));
            Assert.That(result.State.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(result.State.Row, Is.EqualTo(LevelDefinitionTests.Row("ABCD")));
            Assert.That(result.State.ExplodedCount, Is.EqualTo(1)); Assert.That(result.State.DefusedCount, Is.Zero);
        }
        [Test] public void BlastDamageNeverCarriesToNextFloor()
        {
            var resolver = Resolver(hp: 1, bomb: Bomb(3, 1, 4), second: new FloorDefinition(3, new[] { true, true, true }));
            var result = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(0));
            Assert.That(result.State.ActiveFloor, Is.EqualTo(1)); Assert.That(result.State.Durability, Is.EqualTo(3));
            Assert.That(result.State.ExplodedCount, Is.EqualTo(1));
            Assert.That(result.Events.Count(e => e.Kind == PuzzleEventKind.FloorExited), Is.EqualTo(1));
            Assert.That(result.Events.Single(e => e.Kind == PuzzleEventKind.DurabilityResolved).Value, Is.EqualTo(-4));
        }
        [TestCase(1, 1, 0)] [TestCase(2, 0, 1)]
        public void DropTicksOldBombThenExplodesOrAbandons(int ticks, int exploded, int abandoned)
        {
            var resolver = Resolver(bomb: Bomb(3, ticks));
            var s = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Drop).State;
            Assert.That(s.ExplodedCount, Is.EqualTo(exploded)); Assert.That(s.AbandonedCount, Is.EqualTo(abandoned));
            Assert.That(s.DefusedCount, Is.Zero);
        }
        [Test] public void EntryTransformRunsBeforeContactWithoutTick()
        {
            var resolver = Resolver(bomb: Bomb(0, 2), transform: EntryTransform.ShiftLeftOne, start: "DABC");
            var s = resolver.CreateInitialState();
            Assert.That(s.Row, Is.EqualTo(LevelDefinitionTests.Row("ABCD"))); Assert.That(s.DefusedCount, Is.EqualTo(1));
            var armed = Resolver(bomb: Bomb(3, 2)).CreateInitialState();
            Assert.That(armed.BombTicks, Is.EqualTo(2)); Assert.That(armed.BombPhase, Is.EqualTo(BombPhase.Armed));
        }
        [TestCase(EntryTransform.ShiftLeftOne, "ABCD")] [TestCase(EntryTransform.ShiftRightOne, "CDAB")] [TestCase(EntryTransform.MirrorNext, "CBAD")]
        public void EntryAppliesExactlyOneTransform(EntryTransform transform, string expected)
        {
            var resolver = Resolver(transform: transform, start: "DABC");
            Assert.That(resolver.CreateInitialState().Row, Is.EqualTo(LevelDefinitionTests.Row(expected)));
        }
        [Test] public void NextFloorBombDoesNotTickOnLanding()
        {
            var next = new FloorDefinition(3, new[] { true, true, true }, entryTransform: EntryTransform.ShiftRightOne, bomb: Bomb(3, 3));
            var resolver = Resolver(hp: 1, second: next);
            var result = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(0));
            Assert.That(result.State.Row, Is.EqualTo(LevelDefinitionTests.Row("DABC")));
            Assert.That(result.State.BombTicks, Is.EqualTo(3));
            Assert.That(result.Events.Where(e => e.Floor == 1).Select(e => e.Kind), Is.EqualTo(new[] { PuzzleEventKind.FloorEntered, PuzzleEventKind.EntryPermuted }));
        }
        [Test] public void NoOpAndPreviewCannotAdvanceBomb()
        {
            var resolver = new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row("AABC"), LevelDefinitionTests.Row("AABC"),
                new[] { new FloorDefinition(3, new[] { true, true, true }, bomb: Bomb(3, 2)) }));
            var s = resolver.CreateInitialState();
            for (int i = 0; i < 250; i++) { resolver.Preview(s, PuzzleCommand.Swap(1)); Assert.That(resolver.Resolve(s, PuzzleCommand.Swap(0)).State, Is.SameAs(s)); }
            Assert.That(s.BombTicks, Is.EqualTo(2));
        }
        [Test] public void DefusedBombNeverCountsAgain()
        {
            var resolver = Resolver(hp: 5, bomb: Bomb(0)); var s = resolver.CreateInitialState();
            foreach (int edge in new[] { 0, 0, 0 }) s = resolver.Resolve(s, PuzzleCommand.Swap(edge)).State;
            Assert.That(s.DefusedCount, Is.EqualTo(1)); Assert.That(s.ExplodedCount, Is.Zero);
        }
    }
}
