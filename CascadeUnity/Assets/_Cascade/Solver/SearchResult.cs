using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cascade.Domain;

namespace Cascade.Solver
{
    public enum SearchStatus { Solved, Unsolvable, Unknown }

    public sealed class GraphMetrics
    {
        public int States { get; }
        public int Transitions { get; }
        public int DeadStates { get; }
        public int? EarliestDeadDepth { get; }
        public int NearOptimalFirstStatesPlusTwo { get; }
        public double ForcedRatio { get; }
        public int TerminalVariants { get; }
        internal GraphMetrics(int states, int transitions, int deadStates, int? earliestDeadDepth,
            int nearOptimalFirstStatesPlusTwo, double forcedRatio, int terminalVariants)
        {
            States = states; Transitions = transitions; DeadStates = deadStates; EarliestDeadDepth = earliestDeadDepth;
            NearOptimalFirstStatesPlusTwo = nearOptimalFirstStatesPlusTwo; ForcedRatio = forcedRatio; TerminalVariants = terminalVariants;
        }
    }

    public sealed class SearchResult
    {
        public SearchStatus Status { get; }
        public int? OptimalActions { get; }
        public ReadOnlyCollection<PuzzleCommand> Path { get; }
        public int ExploredStates { get; }
        public string LimitReason { get; }
        public GraphMetrics Metrics { get; }
        public ReadOnlyDictionary<PuzzleState, int> ReverseDistances { get; }
        public bool CanPublishGoal => Status == SearchStatus.Solved;
        internal SearchResult(SearchStatus status, int? optimalActions, IEnumerable<PuzzleCommand> path,
            int exploredStates, string limitReason = null, GraphMetrics metrics = null, IDictionary<PuzzleState, int> reverseDistances = null)
        {
            Status = status; OptimalActions = optimalActions; Path = System.Array.AsReadOnly(path.ToArray());
            ExploredStates = exploredStates; LimitReason = limitReason; Metrics = metrics;
            ReverseDistances = new ReadOnlyDictionary<PuzzleState, int>(reverseDistances == null
                ? new Dictionary<PuzzleState, int>() : new Dictionary<PuzzleState, int>(reverseDistances));
        }
    }
}
