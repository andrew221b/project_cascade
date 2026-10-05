using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cascade.Domain;

namespace Cascade.Solver
{
    public static class PuzzleSolver
    {
        public const int DefaultStateBudget = 200000;

        static IEnumerable<PuzzleCommand> Commands(PuzzleState state)
        {
            for (int edge = 0; edge < state.Row.Count - 1; edge++) yield return PuzzleCommand.Swap(edge);
            if(state.ReachCharges>0)for(int first=0;first<state.Row.Count-2;first++)for(int span=2;span<=3 && first+span<state.Row.Count;span++)yield return PuzzleCommand.Reach(first,first+span);
            yield return PuzzleCommand.Drop;
        }
        static SearchResult Unknown(int count, string reason) => new SearchResult(SearchStatus.Unknown, null, Array.Empty<PuzzleCommand>(), count, reason);
        static void ValidateStart(PuzzleResolver resolver, PuzzleState start, int stateBudget)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (stateBudget < 1) throw new ArgumentOutOfRangeException(nameof(stateBudget));
            if (start == null || start.Row.Count != resolver.Level.TargetSymbolIds.Count
                || !LevelValidator.SameMultiset(start.Row, resolver.Level.TargetSymbolIds))
                throw new ArgumentException("Search root is incompatible with the level.", nameof(start));
        }

        public static SearchResult FindShortest(PuzzleResolver resolver, PuzzleState start = null, PuzzleGoal goal = null,
            int stateBudget = DefaultStateBudget, CancellationToken cancellationToken = default)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            start = start ?? resolver.CreateInitialState(); goal = goal ?? PuzzleGoal.Delivery;
            ValidateStart(resolver, start, stateBudget);
            var queue = new Queue<PuzzleState>(); queue.Enqueue(start);
            var parent = new Dictionary<PuzzleState, Parent> { [start] = new Parent(null, default) };
            while (queue.Count != 0)
            {
                if (cancellationToken.IsCancellationRequested) return Unknown(parent.Count, "Cancelled");
                var current = queue.Dequeue();
                if (goal.IsSatisfied(current))
                {
                    var path = new List<PuzzleCommand>(); var step = current;
                    while (parent[step].State != null) { path.Add(parent[step].Command); step = parent[step].State; }
                    path.Reverse();
                    return new SearchResult(SearchStatus.Solved, path.Count, path, parent.Count);
                }
                if (current.Status != PuzzleStatus.Playing) continue;
                foreach (var command in Commands(current))
                {
                    var result = resolver.Resolve(current, command);
                    if (!result.Accepted || parent.ContainsKey(result.State)) continue;
                    if (parent.Count >= stateBudget) return Unknown(parent.Count, "State budget exceeded");
                    parent.Add(result.State, new Parent(current, command)); queue.Enqueue(result.State);
                }
            }
            return new SearchResult(SearchStatus.Unsolvable, null, Array.Empty<PuzzleCommand>(), parent.Count);
        }

        public static SearchResult Analyze(PuzzleResolver resolver, PuzzleGoal goal = null,
            int stateBudget = DefaultStateBudget, CancellationToken cancellationToken = default, PuzzleState start = null)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            start = start ?? resolver.CreateInitialState(); goal = goal ?? PuzzleGoal.Delivery;
            ValidateStart(resolver, start, stateBudget);
            var states = new List<PuzzleState> { start };
            var index = new Dictionary<PuzzleState, int> { [start] = 0 };
            var depths = new List<int> { 0 };
            var outgoing = new List<List<Arc>> { new List<Arc>() };
            var reverse = new List<HashSet<int>> { new HashSet<int>() };
            int transitions = 0;
            for (int i = 0; i < states.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested) return Unknown(states.Count, "Cancelled");
                if (states[i].Status != PuzzleStatus.Playing) continue;
                foreach (var command in Commands(states[i]))
                {
                    var result = resolver.Resolve(states[i], command);
                    if (!result.Accepted) continue;
                    if (!index.TryGetValue(result.State, out int target))
                    {
                        if (states.Count >= stateBudget) return Unknown(states.Count, "State budget exceeded");
                        target = states.Count; index.Add(result.State, target); states.Add(result.State);
                        depths.Add(depths[i] + 1); outgoing.Add(new List<Arc>()); reverse.Add(new HashSet<int>());
                    }
                    outgoing[i].Add(new Arc(command, target)); reverse[target].Add(i); transitions++;
                }
            }

            var distances = Enumerable.Repeat(-1, states.Count).ToArray();
            var queue = new Queue<int>(); int terminalVariants = 0;
            for (int i = 0; i < states.Count; i++)
                if (goal.IsSatisfied(states[i])) { distances[i] = 0; queue.Enqueue(i); terminalVariants++; }
            while (queue.Count != 0)
            {
                int target = queue.Dequeue();
                foreach (int predecessor in reverse[target])
                    if (distances[predecessor] == -1) { distances[predecessor] = distances[target] + 1; queue.Enqueue(predecessor); }
            }
            int deadStates = 0; int? earliestDeadDepth = null;
            for (int i = 0; i < states.Count; i++)
                if (states[i].Status == PuzzleStatus.Playing && distances[i] == -1)
                {
                    deadStates++;
                    if (!earliestDeadDepth.HasValue || depths[i] < earliestDeadDepth.Value) earliestDeadDepth = depths[i];
                }
            int near = 0, forced = 0, decisions = 0;
            var path = new List<PuzzleCommand>();
            if (distances[0] >= 0)
            {
                near = outgoing[0].Where(a => distances[a.Target] >= 0 && 1 + distances[a.Target] <= distances[0] + 2)
                    .Select(a => a.Target).Distinct().Count();
                int cursor = 0;
                while (distances[cursor] > 0)
                {
                    var arc = outgoing[cursor].First(a => distances[a.Target] == distances[cursor] - 1);
                    path.Add(arc.Command); cursor = arc.Target;
                }
                var visited = new HashSet<int> { 0 }; queue.Enqueue(0);
                while (queue.Count != 0)
                {
                    int source = queue.Dequeue(); if (distances[source] == 0) continue;
                    var choices = outgoing[source].Where(a => distances[a.Target] == distances[source] - 1).Select(a => a.Target).Distinct().ToArray();
                    decisions++; if (choices.Length == 1) forced++;
                    foreach (int choice in choices) if (visited.Add(choice)) queue.Enqueue(choice);
                }
            }
            var distanceMap = new Dictionary<PuzzleState, int>();
            for (int i = 0; i < states.Count; i++) if (distances[i] >= 0) distanceMap.Add(states[i], distances[i]);
            var metrics = new GraphMetrics(states.Count, transitions, deadStates, earliestDeadDepth,
                near, Math.Round((double)forced / Math.Max(1, decisions), 4, MidpointRounding.ToEven), terminalVariants);
            return new SearchResult(distances[0] >= 0 ? SearchStatus.Solved : SearchStatus.Unsolvable,
                distances[0] >= 0 ? (int?)distances[0] : null, path, states.Count, metrics: metrics, reverseDistances: distanceMap);
        }

        readonly struct Parent
        {
            internal PuzzleState State { get; }
            internal PuzzleCommand Command { get; }
            internal Parent(PuzzleState state, PuzzleCommand command) { State = state; Command = command; }
        }
        readonly struct Arc
        {
            internal PuzzleCommand Command { get; }
            internal int Target { get; }
            internal Arc(PuzzleCommand command, int target) { Command = command; Target = target; }
        }
    }
}
