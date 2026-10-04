using System;
using System.Collections.Generic;
using System.Linq;

namespace Cascade.Domain
{
    public sealed class PuzzleResolver
    {
        public LevelDefinition Level { get; }
        public PuzzleResolver(LevelDefinition level)
        { LevelValidator.ThrowIfInvalid(level); Level = level; }

        public PuzzleState CreateInitialState()
        {
            var initial = new PuzzleState(Level.InitialSymbolIds, 0, 0);
            return EnterFloor(initial, 0, new List<PuzzleEvent>());
        }

        public CommandPreview Preview(PuzzleState state, PuzzleCommand command)
        {
            if (state == null || state.Row.Count != Level.TargetSymbolIds.Count || !LevelValidator.SameMultiset(state.Row, Level.TargetSymbolIds))
                return new CommandPreview(RejectionReason.InvalidState);
            if (state.Status != PuzzleStatus.Playing) return new CommandPreview(RejectionReason.Terminal);
            if (state.ActiveFloor < 0 || state.ActiveFloor >= Level.Floors.Count || state.Durability <= 0)
                return new CommandPreview(RejectionReason.InvalidState);
            if (command.Kind == CommandKind.EarlyDrop) return new CommandPreview(RejectionReason.None);
            if (command.Kind != CommandKind.Swap || command.Edge < 0 || command.Edge >= state.Row.Count - 1)
                return new CommandPreview(RejectionReason.InvalidCommand);
            var floor = Level.Floors[state.ActiveFloor];
            int[] edges = AffectedEdges(floor, command.Edge);
            if (edges.Any(e => !IsEdgeOpen(floor, state, e))) return new CommandPreview(RejectionReason.ClosedEdge);
            var row = Permute(state.Row, edges);
            if (row.SequenceEqual(state.Row)) return new CommandPreview(RejectionReason.NoOp);
            return new CommandPreview(RejectionReason.None, SwapCost(floor, edges), edges);
        }

        public TransitionResult Resolve(PuzzleState state, PuzzleCommand command)
        {
            var preview = Preview(state, command);
            if (!preview.IsLegal) return new TransitionResult(state, false, 0, preview.Rejection, Array.Empty<PuzzleEvent>());
            var events = new List<PuzzleEvent> { new PuzzleEvent(PuzzleEventKind.CommandAccepted, state, preview.DurabilityCost, preview.AffectedEdges) };
            var next = state;
            if (command.Kind == CommandKind.Swap)
            {
                next = state.With(row: Permute(state.Row, preview.AffectedEdges), durability: state.Durability - preview.DurabilityCost);
                events.Add(new PuzzleEvent(PuzzleEventKind.RowPermuted, next, edges: preview.AffectedEdges));
            }
            next = ResolveContacts(next, events);
            next = TickBomb(next, events);
            events.Add(new PuzzleEvent(PuzzleEventKind.DurabilityResolved, next, next.Durability));
            if (command.Kind == CommandKind.EarlyDrop || next.Durability <= 0)
            {
                next = LeaveFloor(next, events);
                next = EnterFloor(next, state.ActiveFloor + 1, events);
            }
            return new TransitionResult(next, true, preview.DurabilityCost, RejectionReason.None, events);
        }

        static int[] AffectedEdges(FloorDefinition floor, int edge) => new[] { edge };
        static int SwapCost(FloorDefinition floor, IEnumerable<int> edges) => 1;
        static bool IsEdgeOpen(FloorDefinition floor, PuzzleState state, int edge) => floor.OpenEdges[edge];

        static SymbolId[] Permute(IEnumerable<SymbolId> symbols, IEnumerable<int> edges)
        {
            var row = symbols.ToArray();
            foreach (int edge in edges) { var symbol = row[edge]; row[edge] = row[edge + 1]; row[edge + 1] = symbol; }
            return row;
        }

        PuzzleState ResolveContacts(PuzzleState state, List<PuzzleEvent> events) => state;
        PuzzleState TickBomb(PuzzleState state, List<PuzzleEvent> events) => state;
        PuzzleState LeaveFloor(PuzzleState state, List<PuzzleEvent> events)
        { events.Add(new PuzzleEvent(PuzzleEventKind.FloorExited, state)); return state; }

        PuzzleState EnterFloor(PuzzleState state, int floorIndex, List<PuzzleEvent> events)
        {
            if (floorIndex == Level.Floors.Count)
            {
                var terminal = state.With(activeFloor: floorIndex, durability: 0, bombPhase: BombPhase.Absent,
                    bombTicks: 0, keyUsed: false, repairUsed: false, bridgeUsed: false,
                    status: state.Row.SequenceEqual(Level.TargetSymbolIds) ? PuzzleStatus.Won : PuzzleStatus.Lost);
                events.Add(new PuzzleEvent(PuzzleEventKind.ReceiverEvaluated, terminal, terminal.Status == PuzzleStatus.Won ? 1 : 0));
                return terminal;
            }
            var floor = Level.Floors[floorIndex];
            var entered = state.With(activeFloor: floorIndex, durability: floor.BaseDurability,
                bombPhase: BombPhase.Absent, bombTicks: 0, keyUsed: false, repairUsed: false, bridgeUsed: false);
            events.Add(new PuzzleEvent(PuzzleEventKind.FloorEntered, entered, floor.BaseDurability));
            return ResolveContacts(entered, events);
        }
    }
}
