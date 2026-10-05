using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Cascade.Domain;
using Cascade.Solver;

namespace Cascade.Application
{
    public sealed class HistoryEntry
    {
        public PuzzleState Before { get; }
        public PuzzleState After { get; }
        public PuzzleCommand Command { get; }
        public int RouteLengthBefore { get; }
        internal HistoryEntry(PuzzleState before, PuzzleState after, PuzzleCommand command, int routeLengthBefore)
        { Before = before; After = after; Command = command; RouteLengthBefore = routeLengthBefore; }
    }

    public sealed class HintRequest
    {
        public Guid AttemptId { get; }
        public string StateHash { get; }
        public PuzzleState Snapshot { get; }
        internal PuzzleResolver Resolver { get; }
        internal PuzzleGoal Goal { get; }
        internal HintRequest(Guid attemptId, string stateHash, PuzzleState snapshot, PuzzleResolver resolver, PuzzleGoal goal)
        { AttemptId = attemptId; StateHash = stateHash; Snapshot = snapshot; Resolver = resolver; Goal = goal; }
    }

    public sealed class HintOffer
    {
        public Guid AttemptId { get; }
        public string StateHash { get; }
        public HintQueryResult Result { get; }
        internal HintOffer(HintRequest request, HintQueryResult result)
        { AttemptId = request.AttemptId; StateHash = request.StateHash; Result = result; }
    }

    // Owned by the application thread. Hint calculation may run on a worker with an immutable request.
    public sealed class AttemptService
    {
        readonly List<HistoryEntry> history = new List<HistoryEntry>();
        readonly Dictionary<string, PuzzleCommand> revealedHints = new Dictionary<string, PuzzleCommand>(StringComparer.Ordinal);
        public PuzzleResolver Resolver { get; }
        public LevelDefinition Level => Resolver.Level;
        public PuzzleGoal Goal { get; }
        public Guid AttemptId { get; private set; }
        public PuzzleState Current { get; private set; }
        public int RouteLength { get; private set; }
        public int UndoCount { get; private set; }
        public bool HintUsed { get; private set; }
        public string ActiveLocale { get; }
        public PuzzleMode Mode { get; }
        public string PinnedContentHash => Level.ContentHash;
        public string PinnedRulesVersion => Level.RulesVersion;
        public ReadOnlyCollection<HistoryEntry> History { get; }
        public ReadOnlyDictionary<string, PuzzleCommand> RevealedHintStates { get; }
        public bool CanUndo => history.Count != 0;
        public string CurrentStateHash => StateHash.Compute(Level, Current);

        public AttemptService(LevelDefinition level, string puzzleLocale = "en", PuzzleGoal goal = null)
        {
            if (puzzleLocale != "en" && puzzleLocale != "ru" && puzzleLocale != "uk")
                throw new ArgumentException("Unsupported puzzle locale.", nameof(puzzleLocale));
            Resolver = new PuzzleResolver(level); ActiveLocale = puzzleLocale; Goal = goal ?? PuzzleGoal.Delivery;
            if (level.TargetSymbolIds.Any(s => s.Kind == SymbolKind.Glyph && s.Locale != puzzleLocale))
                throw new ArgumentException("Puzzle locale differs from bound glyphs.", nameof(puzzleLocale));
            bool shapes = level.TargetSymbolIds.Any(s => s.Kind == SymbolKind.Shape);
            bool glyphs = level.TargetSymbolIds.Any(s => s.Kind == SymbolKind.Glyph);
            Mode = shapes && glyphs ? PuzzleMode.Mixed : glyphs ? PuzzleMode.Words : PuzzleMode.Shapes;
            History = history.AsReadOnly(); RevealedHintStates = new ReadOnlyDictionary<string, PuzzleCommand>(revealedHints);
            Retry();
        }

        public TransitionResult Predict(PuzzleCommand command) => Resolver.Resolve(Current, command);
        public static AttemptService Restore(LevelDefinition level, AttemptRecord record)
        {
            if(record==null || record.PuzzleId!=level.PuzzleId || record.CampaignNodeId!=level.CampaignNodeId
                || record.ContentHash!=level.ContentHash || record.RulesVersion!=level.RulesVersion)
                throw new ArgumentException("Saved attempt requires its exact pinned content.");
            if(!Guid.TryParse(record.AttemptId,out var id) || id==Guid.Empty || record.UndoCount<0
                || record.Commands==null || record.Commands.Length>10000)
                throw new ArgumentException("Invalid saved attempt metadata.");
            var attempt=new AttemptService(level,record.Locale,new PuzzleGoal(record.RequiredStampBits));
            foreach(string encoded in record.Commands) {
                PuzzleCommand command;
                if(encoded=="D")command=PuzzleCommand.Drop;
                else if(int.TryParse(encoded,out int edge) && edge>=0 && edge<level.InitialSymbolIds.Count-1 && encoded==edge.ToString())command=PuzzleCommand.Swap(edge);
                else if(encoded!=null && encoded.StartsWith("R",StringComparison.Ordinal) && encoded.Split(':').Length==2 && int.TryParse(encoded.Substring(1).Split(':')[0],out int first) && int.TryParse(encoded.Split(':')[1],out int last) && encoded==PuzzleCommand.Reach(first,last).ToString())command=PuzzleCommand.Reach(first,last);
                else throw new ArgumentException("Invalid saved command.");
                if(!attempt.Execute(command).Accepted)throw new ArgumentException("Saved route cannot be replayed.");
            }
            if(attempt.CurrentStateHash!=record.StateHash)throw new ArgumentException("Saved route does not reach its recorded state.");
            attempt.AttemptId=id;attempt.UndoCount=record.UndoCount;attempt.HintUsed=record.HintUsed;
            return attempt;
        }
        public TransitionResult Execute(PuzzleCommand command)
        {
            var result = Resolver.Resolve(Current, command);
            if (!result.Accepted) return result;
            history.Add(new HistoryEntry(Current, result.State, command, RouteLength));
            Current = result.State; RouteLength++;
            return result;
        }

        public bool Undo()
        {
            if (history.Count == 0) return false;
            var previous = history[history.Count - 1]; history.RemoveAt(history.Count - 1);
            Current = previous.Before; RouteLength = previous.RouteLengthBefore; UndoCount++;
            return true;
        }

        public void Retry()
        {
            AttemptId = Guid.NewGuid(); Current = Resolver.CreateInitialState();
            RouteLength = 0; UndoCount = 0; HintUsed = false; history.Clear(); revealedHints.Clear();
        }

        public HintRequest CaptureHintRequest() => new HintRequest(AttemptId, CurrentStateHash, Current, Resolver, Goal);
        public static HintOffer CalculateHint(HintRequest request, int stateBudget = PuzzleSolver.DefaultStateBudget,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return new HintOffer(request, HintQuery.Calculate(request.Resolver, request.Snapshot, request.Goal, stateBudget, cancellationToken));
        }

        public bool IsCurrent(HintOffer offer) => offer != null && offer.AttemptId == AttemptId && offer.StateHash == CurrentStateHash;
        public bool RevealHint(HintOffer offer)
        {
            if (!IsCurrent(offer)) return false;
            if (offer.Result.Status == HintStatus.Step)
            {
                revealedHints[offer.StateHash] = offer.Result.Command.Value;
                HintUsed = true;
            }
            else if (offer.Result.Status == HintStatus.DeadEnd) HintUsed = true;
            return true;
        }
        public bool TryRevealHintAtomically(HintOffer offer,Func<bool> commit)
        {
            if(!IsCurrent(offer)||offer.Result.Status!=HintStatus.Step||revealedHints.ContainsKey(offer.StateHash))return false;
            bool previous=HintUsed;RevealHint(offer);
            try {if(commit())return true;}
            catch {revealedHints.Remove(offer.StateHash);HintUsed=previous;throw;}
            revealedHints.Remove(offer.StateHash);HintUsed=previous;return false;
        }

        public bool TryApplyHint(HintOffer offer, out TransitionResult result)
        {
            result = null;
            if (!IsCurrent(offer) || offer.Result.Status != HintStatus.Step) return false;
            var preview = Resolver.Preview(Current, offer.Result.Command.Value);
            if (!preview.IsLegal) return false;
            RevealHint(offer); result = Execute(offer.Result.Command.Value);
            return result.Accepted;
        }
    }
}
