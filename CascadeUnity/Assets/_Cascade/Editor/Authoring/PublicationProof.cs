using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cascade.Domain;
using Cascade.Solver;
using Newtonsoft.Json.Linq;

namespace Cascade.Editor.Authoring
{
    public sealed class PublicationProof
    {
        public LevelDefinition Level { get; }
        public PuzzleGoal Goal { get; }
        public SearchResult Search { get; }
        public IReadOnlyList<string> Warnings { get; }
        PublicationProof(LevelDefinition level, PuzzleGoal goal, SearchResult search, List<string> warnings)
        { Level = level; Goal = goal; Search = search; Warnings = warnings.AsReadOnly(); }

        public static PublicationProof Analyze(LevelDefinition level, PuzzleGoal goal, int budget = PuzzleSolver.DefaultStateBudget,
            CancellationToken cancellation = default)
        {
            var resolver = new PuzzleResolver(level); var initial = resolver.CreateInitialState(); var warnings = new List<string>();
            if (initial.KeyUsed) warnings.Add("Floor 1 key activates on entry; make its teaching purpose visible.");
            if (initial.RepairUsed) warnings.Add("Floor 1 repair activates on entry.");
            if (initial.BombPhase == BombPhase.Defused) warnings.Add("Floor 1 bomb is defused on entry.");
            if (initial.StampBits != 0) warnings.Add("Floor 1 stamp is collected on entry.");
            var search = PuzzleSolver.Analyze(resolver, goal, budget, cancellation);
            var routeState = initial;
            foreach (var command in search.Path)
            {
                var step = resolver.Resolve(routeState, command); routeState = step.State;
                foreach (var entered in step.Events.Where(e => e.Kind == PuzzleEventKind.FloorEntered))
                    if (step.Events.Any(e => e.Kind == PuzzleEventKind.KeyOpened && e.Floor == entered.Floor))
                        warnings.Add("Floor " + (entered.Floor + 1) + " key activates on entry along the verified route.");
            }
            if (search.Metrics != null && search.Metrics.ForcedRatio > .85) warnings.Add("Most optimal decisions are forced; playtest the intended discovery.");
            if (level.InitialSymbolIds.Count >= 7 && level.Floors.Any(f => new object[] { f.Bomb, f.Key, f.Repair, f.OptionalStamp }.Count(d => d != null) > 2))
                warnings.Add("Long row with several contacts: check icon and touch readability.");
            for (int i = 0; i < level.Floors.Count && !cancellation.IsCancellationRequested; i++)
            {
                var floor = level.Floors[i]; if (floor.Key == null) continue;
                var floors = level.Floors.ToArray();
                floors[i] = new FloorDefinition(floor.BaseDurability, floor.OpenEdges, floor.FragileEdges, floor.LinkedPair,
                    floor.EntryTransform, floor.Bomb, null, floor.Repair, floor.OneUseBridgeEdge, floor.OptionalStamp);
                var withoutKey = new LevelDefinition(level.PuzzleId, level.CampaignNodeId, level.SchemaVersion, level.RulesVersion,
                    level.ContentHash, level.InitialSymbolIds, level.TargetSymbolIds, level.TargetSegments, floors);
                var alternative = PuzzleSolver.FindShortest(new PuzzleResolver(withoutKey), goal: goal, stateBudget: budget, cancellationToken: cancellation);
                if (alternative.Status == SearchStatus.Solved) warnings.Add("Floor " + (i + 1) + " key can be bypassed; the target is reachable with this edge kept locked.");
                else if (alternative.Status == SearchStatus.Unknown) warnings.Add("Floor " + (i + 1) + " key necessity is unverified: " + alternative.LimitReason + ".");
            }
            return new PublicationProof(level, goal, search, warnings);
        }

        public bool Matches(LevelDefinition current, PuzzleGoal goal) => current.ContentHash == Level.ContentHash
            && current.RulesVersion == Level.RulesVersion && goal.RequiredStampBits == Goal.RequiredStampBits;
        public JObject ExportMetadata(LevelDefinition current, PuzzleGoal goal)
        {
            if (!Matches(current, goal)) throw new InvalidOperationException("The draft changed. Solve its current version before export.");
            if (!Search.CanPublishGoal) throw new InvalidOperationException("Export needs a solved goal; Unknown and Unsolvable are not publication proofs.");
            var resolver = new PuzzleResolver(current); var state = resolver.CreateInitialState();
            foreach (var command in Search.Path)
            {
                var result = resolver.Resolve(state, command);
                if (!result.Accepted) throw new InvalidOperationException("Proof replay rejected a command."); state = result.State;
            }
            if (!goal.IsSatisfied(state)) throw new InvalidOperationException("Proof replay did not satisfy the goal.");
            return new JObject { ["proof_schema"] = "cascade.authoring-proof/1", ["puzzle_id"] = current.PuzzleId,
                ["schema_version"] = current.SchemaVersion, ["rules_version"] = current.RulesVersion, ["content_hash"] = current.ContentHash,
                ["required_stamp_bits"] = goal.RequiredStampBits, ["optimal_actions"] = Search.OptimalActions,
                ["path"] = new JArray(Search.Path.Select(c => c.ToString())), ["explored_states"] = Search.ExploredStates };
        }
    }
}
