using System.Linq;
using System.Threading;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Solver;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class LessonRouteTests
    {
        static SymbolId[] Row(string value) => value.Select(c => SymbolId.Parse("s:" + c)).ToArray();
        static LevelDefinition Level(string start, string target, FloorDefinition[] floors, int reach = 0) =>
            new LevelDefinition("lesson-route", "lesson-route", "1", reach > 0 ? "2.1-longreach" : "2.0", "fixture",
                Row(start), Row(target), new[] { new TargetSegment(SegmentKind.Shapes, 0, start.Length) }, floors, reach);

        [Test] public void ContactRouteCanPrepareBeforeTheRealLessonOutcome()
        {
            var level = Level("BCA", "ABC", new[] { new FloorDefinition(3, new[] { true, true },
                repair: new RepairDefinition(0, SymbolId.Parse("s:A"), 1)) });
            var resolver = new PuzzleResolver(level); var state = resolver.CreateInitialState();
            Assert.That(LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, MechanicLesson.Repair, 0), Is.EqualTo(PuzzleCommand.Swap(1)));
            var prepare = resolver.Resolve(state, PuzzleCommand.Swap(1));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Repair, 0, prepare, PuzzleCommand.Swap(1)), Is.False);
            var next = LessonRoute.Find(resolver, prepare.State, PuzzleGoal.Delivery, MechanicLesson.Repair, 0);
            Assert.That(next, Is.EqualTo(PuzzleCommand.Swap(0)));
            var repair = resolver.Resolve(prepare.State, next.Value);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Repair, 0, repair, next.Value), Is.True);
            Assert.That(PuzzleSolver.FindShortest(resolver, repair.State).Status, Is.EqualTo(SearchStatus.Solved));
            Assert.That(state.Row, Is.EqualTo(Row("BCA"))); Assert.That(state.RepairUsed, Is.False);
        }

        [Test] public void UnreachableOptionalEffectNeverFallsBackToOrdinaryDelivery()
        {
            var level = Level("BAC", "ABC", new[] { new FloorDefinition(3, new[] { true, false },
                optionalStamp: new ContactDefinition(2, SymbolId.Parse("s:A"))) });
            var resolver = new PuzzleResolver(level); var state = resolver.CreateInitialState();
            Assert.That(PuzzleSolver.FindShortest(resolver, state).Status, Is.EqualTo(SearchStatus.Solved));
            Assert.That(LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, MechanicLesson.Stamp, 0), Is.Null);
        }

        [Test] public void CannotLeaveTeachingFloorToFindAnEffectOnAnotherFloor()
        {
            var level = Level("BAC", "ABC", new[] { new FloorDefinition(2, new[] { true, true }),
                new FloorDefinition(3, new[] { true, true }, optionalStamp: new ContactDefinition(0, SymbolId.Parse("s:A"))) });
            var resolver = new PuzzleResolver(level);
            Assert.That(LessonRoute.Find(resolver, resolver.CreateInitialState(), PuzzleGoal.Delivery, MechanicLesson.Stamp, 0), Is.Null);
        }

        [Test] public void FutureContactCanCompleteOnTheImmediateNextArrival()
        {
            var level = Level("ABC", "ABC", new[] { new FloorDefinition(2, new[] { true, true }),
                new FloorDefinition(3, new[] { true, true }, optionalStamp: new ContactDefinition(0, SymbolId.Parse("s:A"))) });
            var resolver = new PuzzleResolver(level); var state = resolver.CreateInitialState();
            var next = LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, MechanicLesson.Stamp, 1);
            Assert.That(next, Is.EqualTo(PuzzleCommand.Drop));
            var arrival = resolver.Resolve(state, next.Value);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Stamp, 1, arrival, next.Value), Is.True);
            Assert.That(PuzzleSolver.FindShortest(resolver, arrival.State).Status, Is.EqualTo(SearchStatus.Solved));
            Assert.That(state.StampBits, Is.Zero);
        }

        [Test] public void FutureContactCannotSkipItsBoardToReachAnOrdinaryDelivery()
        {
            var level = Level("BAC", "ABC", new[] { new FloorDefinition(1, new[] { true, false }),
                new FloorDefinition(3, new[] { true, false }, optionalStamp: new ContactDefinition(2, SymbolId.Parse("s:A"))),
                new FloorDefinition(3, new[] { true, true }) });
            var resolver = new PuzzleResolver(level); var state = resolver.CreateInitialState();
            Assert.That(PuzzleSolver.FindShortest(resolver, state).Status, Is.EqualTo(SearchStatus.Solved));
            Assert.That(LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, MechanicLesson.Stamp, 1), Is.Null);
        }

        [TestCase(EntryTransform.MirrorNext, MechanicLesson.Mirror, "CBA")]
        [TestCase(EntryTransform.ShiftRightOne, MechanicLesson.Rollers, "CAB")]
        [TestCase(EntryTransform.ShiftLeftOne, MechanicLesson.LandingWind, "BCA")]
        public void EntryLessonUsesTheActualNextLanding(EntryTransform transform, MechanicLesson lesson, string target)
        {
            var level = Level("ABC", target, new[] { new FloorDefinition(3, new[] { true, true }),
                new FloorDefinition(3, new[] { true, true }, entryTransform: transform) });
            var resolver = new PuzzleResolver(level); var state = resolver.CreateInitialState();
            var next = LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, lesson, 0);
            Assert.That(next, Is.EqualTo(PuzzleCommand.Drop));
            var landed = resolver.Resolve(state, next.Value);
            Assert.That(LessonPractice.CompletedBy(lesson, 0, landed, next.Value), Is.True);
            Assert.That(PuzzleSolver.FindShortest(resolver, landed.State).Status, Is.EqualTo(SearchStatus.Solved));
            Assert.That(LessonRoute.Find(resolver, landed.State, PuzzleGoal.Delivery, lesson, 0), Is.Null);
        }

        [Test] public void FarSwapRequiresAnOwnedLegalReachAndSolvableContinuation()
        {
            var level = Level("CBA", "ABC", new[] { new FloorDefinition(1, new[] { true, true }) }, 1);
            var resolver = new PuzzleResolver(level); var state = resolver.CreateInitialState();
            var next = LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, MechanicLesson.FarSwap, 0);
            Assert.That(next, Is.EqualTo(PuzzleCommand.Reach(0, 2)));
            var reached = resolver.Resolve(state, next.Value);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.FarSwap, 0, reached, next.Value), Is.True);
            Assert.That(reached.State.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(LevelValidator.SameMultiset(state.Row, reached.State.Row), Is.True);
            var noCharge = new PuzzleResolver(Level("CBA", "ABC", new[] { new FloorDefinition(5, new[] { true, true }) }));
            Assert.That(LessonRoute.Find(noCharge, noCharge.CreateInitialState(), PuzzleGoal.Delivery, MechanicLesson.FarSwap, 0), Is.Null);
        }

        [Test] public void LegalLessonActionWithoutDeliveryContinuationIsUnavailable()
        {
            var level = Level("ABC", "BCA", new[] { new FloorDefinition(1, new[] { true, true }) }, 1);
            var resolver = new PuzzleResolver(level); var state = resolver.CreateInitialState();
            Assert.That(resolver.Preview(state, PuzzleCommand.Reach(0, 2)).IsLegal, Is.True);
            Assert.That(LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, MechanicLesson.FarSwap, 0), Is.Null);
        }

        [Test] public void DescentAndLiveGustFollowRealResolverTiming()
        {
            var level = Level("ABC", "ABC", new[] { new FloorDefinition(4, new[] { true, true },
                gustAfterSwaps: 2, gustTransform: EntryTransform.ShiftRightOne), new FloorDefinition(6, new[] { true, true }) });
            var resolver = new PuzzleResolver(level); var state = resolver.CreateInitialState();
            Assert.That(LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, MechanicLesson.Descent, 0), Is.EqualTo(PuzzleCommand.Drop));
            var next = LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, MechanicLesson.LiveGust, 0);
            Assert.That(next.HasValue, Is.True);
            var prepare = resolver.Resolve(state, next.Value);
            Assert.That(prepare.State.GustTicks, Is.EqualTo(1));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.LiveGust, 0, prepare, next.Value), Is.False);
            var gustStep = LessonRoute.Find(resolver, prepare.State, PuzzleGoal.Delivery, MechanicLesson.LiveGust, 0);
            Assert.That(gustStep.HasValue, Is.True);
            var gust = resolver.Resolve(prepare.State, gustStep.Value);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.LiveGust, 0, gust, gustStep.Value), Is.True);
            Assert.That(PuzzleSolver.FindShortest(resolver, gust.State).Status, Is.EqualTo(SearchStatus.Solved));
            Assert.That(state.GustTicks, Is.EqualTo(2));
        }

        [Test] public void SharedBudgetCancellationAndOwnerOnlyLessonsReturnNoCommand()
        {
            var resolver = new PuzzleResolver(Level("BAC", "ABC", new[] { new FloorDefinition(2, new[] { true, true }) }));
            var state = resolver.CreateInitialState();
            Assert.That(LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, MechanicLesson.Swap, 0, 1), Is.Null);
            using (var cancelled = new CancellationTokenSource()) {
                cancelled.Cancel();
                Assert.That(LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, MechanicLesson.Swap, 0, cancellationToken: cancelled.Token), Is.Null);
            }
            foreach (var lesson in new[] { MechanicLesson.Gust, MechanicLesson.Undo, MechanicLesson.Hint, MechanicLesson.Toolbag, MechanicLesson.SignalLantern })
                Assert.That(LessonRoute.Find(resolver, state, PuzzleGoal.Delivery, lesson, 0), Is.Null);
        }
    }
}
