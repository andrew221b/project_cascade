using System.Threading;
using Cascade.Domain;

namespace Cascade.Solver
{
    public enum HintStatus { Step, DeadEnd, Unknown, Complete }

    public sealed class HintQueryResult
    {
        public HintStatus Status { get; }
        public PuzzleCommand? Command { get; }
        public SearchResult Search { get; }
        internal HintQueryResult(HintStatus status, PuzzleCommand? command, SearchResult search)
        { Status = status; Command = command; Search = search; }
    }

    public static class HintQuery
    {
        public static HintQueryResult Calculate(PuzzleResolver resolver, PuzzleState snapshot, PuzzleGoal goal,
            int stateBudget = PuzzleSolver.DefaultStateBudget, CancellationToken cancellationToken = default)
        {
            var search = PuzzleSolver.FindShortest(resolver, snapshot, goal, stateBudget, cancellationToken);
            if (search.Status == SearchStatus.Unknown) return new HintQueryResult(HintStatus.Unknown, null, search);
            if (search.Status == SearchStatus.Unsolvable) return new HintQueryResult(HintStatus.DeadEnd, null, search);
            return search.Path.Count == 0 ? new HintQueryResult(HintStatus.Complete, null, search)
                : new HintQueryResult(HintStatus.Step, search.Path[0], search);
        }
    }
}
