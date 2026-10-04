using System.Linq;
using Cascade.Domain;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class KeyRepairTests
    {
        static PuzzleResolver Resolver(FloorDefinition first, string start = "BACD", FloorDefinition second = null) =>
            new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row(start), LevelDefinitionTests.Row("ABCD"), second == null ? new[] { first } : new[] { first, second }));
        static readonly bool[] Open = { true, true, true };
        static SymbolId A => LevelDefinitionTests.S("A");

        [Test] public void KeyCannotRetroactivelyAuthorizeClosedSwap()
        {
            var resolver = Resolver(new FloorDefinition(3, new[] { false, true, true }, key: new KeyDefinition(0, A, 0)));
            var s = resolver.CreateInitialState(); var result = resolver.Resolve(s, PuzzleCommand.Swap(0));
            Assert.That(result.State, Is.SameAs(s)); Assert.That(result.Accepted, Is.False); Assert.That(s.KeyUsed, Is.False);
        }
        [Test] public void KeyContactOpensOnlyFutureCommands()
        {
            var resolver = Resolver(new FloorDefinition(2, new[] { false, true, true }, key: new KeyDefinition(1, A, 0)), "BCAD");
            var s = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(1)).State;
            Assert.That(s.KeyUsed, Is.True); Assert.That(resolver.Preview(s, PuzzleCommand.Swap(0)).IsLegal, Is.True);
            Assert.That(resolver.Resolve(s, PuzzleCommand.Swap(0)).State.Status, Is.EqualTo(PuzzleStatus.Won));
        }
        [Test] public void SignedRepairDoesNotClampNegativeIntermediateHp()
        {
            var resolver = Resolver(new FloorDefinition(1, Open, new[] { 0 }, repair: new RepairDefinition(0, A, 1)));
            var s = resolver.CreateInitialState(); var result = resolver.Resolve(s, PuzzleCommand.Swap(0));
            Assert.That(resolver.Preview(s, PuzzleCommand.Swap(0)).DurabilityCost, Is.EqualTo(2));
            Assert.That(result.State.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(result.Events.Single(e => e.Kind == PuzzleEventKind.DurabilityResolved).Value, Is.Zero);
        }
        [Test] public void RepairCanSaveFloorAndIsOneShot()
        {
            var resolver = Resolver(new FloorDefinition(1, Open, repair: new RepairDefinition(0, A, 1)));
            var s = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(0)).State;
            Assert.That(s.Status, Is.EqualTo(PuzzleStatus.Playing)); Assert.That(s.Durability, Is.EqualTo(1));
            var next = resolver.Resolve(s, PuzzleCommand.Swap(0));
            Assert.That(next.Events.Any(e => e.Kind == PuzzleEventKind.Repaired), Is.False);
            Assert.That(next.State.Status, Is.EqualTo(PuzzleStatus.Lost));
        }
        [Test] public void OneShotRepairCannotBeFarmedByReturningToContact()
        {
            var resolver = Resolver(new FloorDefinition(6, Open, repair: new RepairDefinition(0, A, 1)));
            var s = resolver.CreateInitialState();
            foreach (int edge in new[] { 0, 0, 0 }) s = resolver.Resolve(s, PuzzleCommand.Swap(edge)).State;
            Assert.That(s.Durability, Is.EqualTo(4)); Assert.That(s.RepairUsed, Is.True);
        }
        [Test] public void RepairAndBlastAreSummedBeforeOneCollapse()
        {
            var resolver = Resolver(new FloorDefinition(2, Open, bomb: new BombDefinition(3, A, 1, 2), repair: new RepairDefinition(0, A, 1)));
            var result = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(0));
            Assert.That(result.State.Status, Is.EqualTo(PuzzleStatus.Won)); Assert.That(result.State.ExplodedCount, Is.EqualTo(1));
            Assert.That(result.Events.Count(e => e.Kind == PuzzleEventKind.FloorExited), Is.EqualTo(1));
            Assert.That(result.Events.Single(e => e.Kind == PuzzleEventKind.DurabilityResolved).Value, Is.Zero);
        }
        [Test] public void ShiftPrecedesBothEntrySensors()
        {
            var resolver = Resolver(new FloorDefinition(3, new[] { false, true, true }, entryTransform: EntryTransform.ShiftLeftOne,
                key: new KeyDefinition(0, A, 0), repair: new RepairDefinition(0, A, 1)), "DABC");
            var s = resolver.CreateInitialState();
            Assert.That(s.KeyUsed, Is.True); Assert.That(s.RepairUsed, Is.True); Assert.That(s.Durability, Is.EqualTo(4));
            Assert.That(resolver.Preview(s, PuzzleCommand.Swap(0)).IsLegal, Is.True);
        }
        [Test] public void DropRemainsForcedEvenWhenRepairHasAddedDurability()
        {
            var resolver = Resolver(new FloorDefinition(3, Open, repair: new RepairDefinition(0, A, 1)), "ABCD");
            var s = resolver.CreateInitialState(); Assert.That(s.Durability, Is.EqualTo(4));
            Assert.That(resolver.Resolve(s, PuzzleCommand.Drop).State.Status, Is.EqualTo(PuzzleStatus.Won));
        }
        [Test] public void CompositeContactAndPredictionUseIdenticalTransition()
        {
            var resolver = Resolver(new FloorDefinition(1, new[] { false, true, true }, bomb: new BombDefinition(1, A, 1, 2),
                key: new KeyDefinition(1, A, 0), repair: new RepairDefinition(1, A, 1)), "BCAD");
            var s = resolver.CreateInitialState(); var prediction = resolver.Resolve(s, PuzzleCommand.Swap(1));
            var apply = resolver.Resolve(s, PuzzleCommand.Swap(1));
            Assert.That(prediction.State, Is.EqualTo(apply.State)); Assert.That(s.KeyUsed, Is.False); Assert.That(s.RepairUsed, Is.False);
            Assert.That(apply.State.Durability, Is.EqualTo(1)); Assert.That(apply.State.DefusedCount, Is.EqualTo(1));
            Assert.That(apply.Events.Select(e => e.Kind), Is.EqualTo(new[] { PuzzleEventKind.CommandAccepted, PuzzleEventKind.RowPermuted,
                PuzzleEventKind.BombDefused, PuzzleEventKind.KeyOpened, PuzzleEventKind.Repaired, PuzzleEventKind.DurabilityResolved }));
        }
        [Test] public void DeviceUseFlagsResetOnNewFloor()
        {
            var second = new FloorDefinition(2, new[] { false, true, true }, key: new KeyDefinition(3, A, 0), repair: new RepairDefinition(3, A, 1));
            var resolver = Resolver(new FloorDefinition(1, new[] { false, true, true }, key: new KeyDefinition(0, A, 0), repair: new RepairDefinition(0, A, 1)), "ABCD", second);
            var s = resolver.CreateInitialState(); Assert.That(s.KeyUsed && s.RepairUsed, Is.True);
            s = resolver.Resolve(s, PuzzleCommand.Drop).State;
            Assert.That(s.KeyUsed || s.RepairUsed, Is.False); Assert.That(s.Durability, Is.EqualTo(2));
        }
    }
}
