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
        public int MovesMade { get; private set; }
        public double? ActiveSeconds { get; private set; }
        public void AdvanceActiveTime(double seconds)
        {
            if(double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
            if(Current.Status==PuzzleStatus.Playing&&ActiveSeconds.HasValue)ActiveSeconds=Math.Min(604800,ActiveSeconds.Value+seconds);
        }
        public bool HintUsed { get; private set; }
        public string ActiveLocale { get; }
        public PuzzleMode Mode { get; }
        public DirectionRecipe Direction { get; }
        public string PinnedContentHash => Level.ContentHash;
        public string PinnedRulesVersion => Level.RulesVersion;
        public ReadOnlyCollection<HistoryEntry> History { get; }
        public ReadOnlyDictionary<string, PuzzleCommand> RevealedHintStates { get; }
        public const int FreeUndoLimit = 2;
        public int FreeUndoUsed { get; private set; }
        public bool SpoolUsed { get; private set; }
        public bool HourglassUsed { get; private set; }
        public bool WayfinderUsed { get; private set; }
        public bool LanternUsed { get; private set; }
        public bool LensUsed { get; private set; }
        public TransitionResult LastToolTransition { get; private set; }
        public bool CanUseLens => !LensUsed && Current.Status==PuzzleStatus.Playing && Current.ActiveFloor<Level.Floors.Count-1;
        public int GuideFloor { get; private set; } = -1;
        public bool PurchasedHelpUsed => SpoolUsed || HourglassUsed || WayfinderUsed || LanternUsed || LensUsed;
        public int FreeUndoRemaining => Math.Max(0, FreeUndoLimit - FreeUndoUsed);
        public bool CanUndoHere => history.Count != 0 && Current.Status == PuzzleStatus.Playing
            && history[history.Count - 1].Before.ActiveFloor == Current.ActiveFloor;
        public bool CanUndo => CanUndoHere && FreeUndoRemaining > 0;
        public bool CanUseSpool => CanUndoHere && FreeUndoRemaining == 0 && !SpoolUsed;
        public bool CanUseHourglass => !HourglassUsed && Current.Status != PuzzleStatus.Won
            && history.Any(h => h.Before.ActiveFloor != h.After.ActiveFloor);
        public bool GuideActive => WayfinderUsed && GuideFloor == Current.ActiveFloor && Current.Status == PuzzleStatus.Playing;
        public string CurrentStateHash => StateHash.Compute(Level, Current);

        public AttemptService(LevelDefinition level, string puzzleLocale = "en", PuzzleGoal goal = null, DirectionRecipe direction = null)
        {
            if (puzzleLocale != "en" && puzzleLocale != "ru" && puzzleLocale != "uk")
                throw new ArgumentException("Unsupported puzzle locale.", nameof(puzzleLocale));
            Resolver = new PuzzleResolver(level); ActiveLocale = puzzleLocale; Goal = goal ?? PuzzleGoal.Delivery;
            Direction=direction?.Copy();
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
            var attempt=new AttemptService(level,record.Locale,new PuzzleGoal(record.RequiredStampBits),record.Direction);
            foreach(string encoded in record.Commands) {
                PuzzleCommand command;
                if(encoded=="D")command=PuzzleCommand.Drop;
                else if(int.TryParse(encoded,out int edge) && edge>=0 && edge<level.InitialSymbolIds.Count-1 && encoded==edge.ToString())command=PuzzleCommand.Swap(edge);
                else if(encoded!=null && encoded.StartsWith("R",StringComparison.Ordinal) && encoded.Split(':').Length==2 && int.TryParse(encoded.Substring(1).Split(':')[0],out int first) && int.TryParse(encoded.Split(':')[1],out int last) && encoded==PuzzleCommand.Reach(first,last).ToString())command=PuzzleCommand.Reach(first,last);
                else throw new ArgumentException("Invalid saved command.");
                if(!attempt.Execute(command).Accepted)throw new ArgumentException("Saved route cannot be replayed.");
            }
            if(attempt.CurrentStateHash!=record.StateHash)throw new ArgumentException("Saved route does not reach its recorded state.");
            if(record.MovesMade.HasValue&&(record.MovesMade.Value<attempt.RouteLength+record.UndoCount||record.MovesMade.Value>100000)
                ||record.ActiveSeconds.HasValue&&(double.IsNaN(record.ActiveSeconds.Value)||double.IsInfinity(record.ActiveSeconds.Value)||record.ActiveSeconds.Value<0||record.ActiveSeconds.Value>604800))
                throw new ArgumentException("Invalid saved performance metadata.");
            attempt.MovesMade=record.MovesMade??attempt.RouteLength+record.UndoCount;attempt.ActiveSeconds=record.ActiveSeconds;
            attempt.AttemptId=id;attempt.UndoCount=record.UndoCount;attempt.HintUsed=record.HintUsed;
            if(record.FreeUndoUsed < -1 || record.FreeUndoUsed > FreeUndoLimit || record.GuideFloor < -1 || record.GuideFloor >= level.Floors.Count
                || !record.WayfinderUsed && record.GuideFloor>=0 || (record.SpoolUsed||record.HourglassUsed||record.WayfinderUsed||record.LanternUsed||record.LensUsed)&&!record.HintUsed)
                throw new ArgumentException("Invalid travel tool proof.");
            attempt.FreeUndoUsed=record.FreeUndoUsed<0?Math.Min(FreeUndoLimit,record.UndoCount):record.FreeUndoUsed;
            attempt.SpoolUsed=record.SpoolUsed;attempt.HourglassUsed=record.HourglassUsed;attempt.WayfinderUsed=record.WayfinderUsed;attempt.GuideFloor=record.GuideFloor;attempt.LanternUsed=record.LanternUsed;attempt.LensUsed=record.LensUsed;
            return attempt;
        }
        public TransitionResult Execute(PuzzleCommand command)
        {
            var result = Resolver.Resolve(Current, command);
            if (!result.Accepted) return result;
            if(GuideActive && (result.State.ActiveFloor!=Current.ActiveFloor || result.State.Status!=PuzzleStatus.Playing))GuideFloor=-1;
            history.Add(new HistoryEntry(Current, result.State, command, RouteLength));
            Current = result.State; RouteLength++; MovesMade++;
            return result;
        }

        public bool Undo()
        {
            if (!CanUndo) return false;
            FreeUndoUsed++; RestoreLastCommand(); return true;
        }
        void RestoreLastCommand()
        {
            var previous = history[history.Count - 1]; history.RemoveAt(history.Count - 1);
            Current = previous.Before; RouteLength = previous.RouteLengthBefore; UndoCount++;
        }

        // Inventory persistence and exact replay are one transaction; failed saves roll back tool use.
        public bool TryUseToolAtomically(TravelTool tool, Func<bool> commit, HintOffer guide = null)
        {
            if(commit==null || !ShopCatalog.IsTool(tool))return false;
            if(tool==TravelTool.LandingLens&&!CanUseLens || tool==TravelTool.SignalLantern&&(LanternUsed||Current.Status!=PuzzleStatus.Playing||!IsCurrent(guide)||guide.Result.Status!=HintStatus.Step||guide.Result.Command.Value.Kind==CommandKind.EarlyDrop)
                || tool==TravelTool.UndoSpool && !CanUseSpool || tool==TravelTool.ReturnHourglass && !CanUseHourglass
                || tool==TravelTool.Wayfinder && (WayfinderUsed || Current.Status!=PuzzleStatus.Playing || !IsCurrent(guide) || guide.Result.Status!=HintStatus.Step || guide.Result.Command.Value.Kind==CommandKind.EarlyDrop))return false;
            var oldState=Current;var oldHistory=history.ToArray();int oldRoute=RouteLength,oldMoves=MovesMade,oldUndo=UndoCount,oldGuideFloor=GuideFloor;
            bool oldSpool=SpoolUsed,oldHourglass=HourglassUsed,oldGuide=WayfinderUsed,oldHint=HintUsed,oldLantern=LanternUsed,oldLens=LensUsed;
            var oldTransition=LastToolTransition;LastToolTransition=null;
            if(tool==TravelTool.UndoSpool){RestoreLastCommand();SpoolUsed=true;}
            else if(tool==TravelTool.ReturnHourglass){
                int boundary=history.FindLastIndex(h=>h.Before.ActiveFloor!=h.After.ActiveFloor);
                Current=history[boundary].Before;RouteLength=history[boundary].RouteLengthBefore;
                history.RemoveRange(boundary,history.Count-boundary);UndoCount++;HourglassUsed=true;
            }
            else if(tool==TravelTool.SignalLantern){LastToolTransition=Execute(guide.Result.Command.Value);if(!LastToolTransition.Accepted){LastToolTransition=oldTransition;return false;}LanternUsed=true;}
            else if(tool==TravelTool.LandingLens)LensUsed=true;
            else {WayfinderUsed=true;GuideFloor=Current.ActiveFloor;}
            HintUsed=true;
            try {if(commit())return true;}
            catch {Rollback();throw;}
            Rollback();return false;
            void Rollback(){Current=oldState;history.Clear();history.AddRange(oldHistory);RouteLength=oldRoute;MovesMade=oldMoves;UndoCount=oldUndo;
                GuideFloor=oldGuideFloor;SpoolUsed=oldSpool;HourglassUsed=oldHourglass;WayfinderUsed=oldGuide;HintUsed=oldHint;LanternUsed=oldLantern;LensUsed=oldLens;LastToolTransition=oldTransition;}
        }

        public void Retry()
        {
            AttemptId = Guid.NewGuid(); Current = Resolver.CreateInitialState();
            RouteLength = 0; MovesMade=0; ActiveSeconds=0; UndoCount = 0; HintUsed = false; history.Clear(); revealedHints.Clear();
            FreeUndoUsed=0;SpoolUsed=HourglassUsed=WayfinderUsed=LanternUsed=LensUsed=false;GuideFloor=-1;LastToolTransition=null;
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
