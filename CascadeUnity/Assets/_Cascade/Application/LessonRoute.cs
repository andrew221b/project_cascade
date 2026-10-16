using System;
using System.Collections.Generic;
using System.Threading;
using Cascade.Domain;

namespace Cascade.Application
{
    // One bounded search proves both the real lesson outcome and delivery. The
    // completed bit keeps an ordinary delivery path from standing in for practice.
    public static class LessonRoute
    {
        public static PuzzleCommand? Find(PuzzleResolver resolver, PuzzleState state, PuzzleGoal goal,
            MechanicLesson lesson, int practiceFloor, int stateBudget = 4000, CancellationToken cancellationToken = default)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (state == null || state.Row.Count != resolver.Level.TargetSymbolIds.Count
                || !LevelValidator.SameMultiset(state.Row, resolver.Level.TargetSymbolIds))
                throw new ArgumentException("Search root is incompatible with the level.", nameof(state));
            if (stateBudget < 1) throw new ArgumentOutOfRangeException(nameof(stateBudget));
            int originalFloor = state.ActiveFloor;
            bool arrivalContact = practiceFloor == originalFloor + 1
                && lesson >= MechanicLesson.Bomb && lesson <= MechanicLesson.BombHourglass;
            if (cancellationToken.IsCancellationRequested || state.Status != PuzzleStatus.Playing
                || practiceFloor < 0 || practiceFloor >= resolver.Level.Floors.Count
                || practiceFloor != originalFloor && !arrivalContact || !HasCommandOutcome(lesson)) return null;
            goal = goal ?? PuzzleGoal.Delivery;
            var before = new HashSet<PuzzleState> { state };
            var after = new HashSet<PuzzleState>();
            var queue = new Queue<Step>(); queue.Enqueue(new Step(state, false, null));
            while (queue.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested) return null;
                var current = queue.Dequeue();
                if (current.Taught && goal.IsSatisfied(current.State)) return current.First;
                if (current.State.Status != PuzzleStatus.Playing) continue;
                foreach (var command in Commands(current.State))
                {
                    if (cancellationToken.IsCancellationRequested) return null;
                    var result = resolver.Resolve(current.State, command);
                    if (!result.Accepted) continue;
                    bool taught = current.Taught || LessonPractice.CompletedBy(lesson, practiceFloor, result, command);
                    // A next-board contact may be announced before arrival.
                    // Keep its preparation on these two boards; do not skip the
                    // teaching board. Entry lessons complete on the landing.
                    if (!taught && result.State.ActiveFloor != originalFloor
                        && result.State.ActiveFloor != practiceFloor) continue;
                    var seen = taught ? after : before;
                    if (seen.Contains(result.State)) continue;
                    if (before.Count + after.Count >= stateBudget) continue;
                    seen.Add(result.State);
                    queue.Enqueue(new Step(result.State, taught, current.First ?? command));
                }
            }
            return null;
        }

        static bool HasCommandOutcome(MechanicLesson lesson) =>
            lesson >= MechanicLesson.Bomb && lesson <= MechanicLesson.LandingWind
            || lesson == MechanicLesson.LiveGust || lesson == MechanicLesson.Swap
            || lesson == MechanicLesson.Descent || lesson == MechanicLesson.FarSwap;

        static IEnumerable<PuzzleCommand> Commands(PuzzleState state)
        {
            for (int edge = 0; edge < state.Row.Count - 1; edge++) yield return PuzzleCommand.Swap(edge);
            if (state.ReachCharges > 0)
                for (int first = 0; first < state.Row.Count - 2; first++)
                    for (int span = 2; span <= 3 && first + span < state.Row.Count; span++)
                        yield return PuzzleCommand.Reach(first, first + span);
            yield return PuzzleCommand.Drop;
        }

        readonly struct Step
        {
            public PuzzleState State { get; }
            public bool Taught { get; }
            public PuzzleCommand? First { get; }
            public Step(PuzzleState state, bool taught, PuzzleCommand? first)
            { State = state; Taught = taught; First = first; }
        }
    }
}
