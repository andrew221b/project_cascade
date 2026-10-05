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
            var initial = new PuzzleState(Level.InitialSymbolIds, 0, 0, reachCharges: Level.ReachCharges);
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
            if (command.Kind == CommandKind.LongReach) {
                if (state.ReachCharges < 1 || command.Edge < 0 || command.End >= state.Row.Count || command.End-command.Edge < 2 || command.End-command.Edge > 3) return new CommandPreview(RejectionReason.InvalidCommand);
                var reachFloor=Level.Floors[state.ActiveFloor];
                var crossed=Enumerable.Range(command.Edge,command.End-command.Edge).ToArray();
                if (crossed.Any(e=>!IsEdgeOpen(reachFloor,state,e) || reachFloor.LinkedPair.HasValue && reachFloor.LinkedPair.Value.Contains(e))) return new CommandPreview(RejectionReason.ClosedEdge);
                if (state.Row[command.Edge]==state.Row[command.End]) return new CommandPreview(RejectionReason.NoOp);
                return new CommandPreview(RejectionReason.None,SwapCost(reachFloor,crossed),crossed);
            }
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
            if (command.Kind == CommandKind.Swap || command.Kind == CommandKind.LongReach)
            {
                var bridge = Level.Floors[state.ActiveFloor].OneUseBridgeEdge;
                bool consumed = state.BridgeUsed || (bridge.HasValue && preview.AffectedEdges.Contains(bridge.Value));
                var row=Permute(state.Row, command.Kind==CommandKind.LongReach ? Array.Empty<int>() : preview.AffectedEdges);
                if(command.Kind==CommandKind.LongReach){var symbol=row[command.Edge];row[command.Edge]=row[command.End];row[command.End]=symbol;}
                next = state.With(row: row, durability: state.Durability - preview.DurabilityCost, bridgeUsed: consumed, reachCharges: state.ReachCharges-(command.Kind==CommandKind.LongReach?1:0));
                events.Add(new PuzzleEvent(PuzzleEventKind.RowPermuted, next, edges: preview.AffectedEdges));
            }
            next = ResolveContacts(next, events);
            next = TickBomb(next, events);
            events.Add(new PuzzleEvent(PuzzleEventKind.DurabilityResolved, next, next.Durability));
            if (!state.BridgeUsed && next.BridgeUsed)
                events.Add(new PuzzleEvent(PuzzleEventKind.BridgeConsumed, next, Level.Floors[state.ActiveFloor].OneUseBridgeEdge.Value));
            if (command.Kind == CommandKind.EarlyDrop || next.Durability <= 0)
            {
                next = LeaveFloor(next, events);
                next = EnterFloor(next, state.ActiveFloor + 1, events);
            }
            return new TransitionResult(next, true, preview.DurabilityCost, RejectionReason.None, events);
        }

        static int[] AffectedEdges(FloorDefinition floor, int edge) => floor.LinkedPair.HasValue && floor.LinkedPair.Value.Contains(edge)
            ? new[] { floor.LinkedPair.Value.FirstEdge, floor.LinkedPair.Value.SecondEdge } : new[] { edge };
        static int SwapCost(FloorDefinition floor, IEnumerable<int> edges) => 1 + edges.Count(e => floor.FragileEdges.Contains(e));
        public bool IsEdgeOpen(PuzzleState state, int edge) => state != null && state.Status == PuzzleStatus.Playing
            && state.ActiveFloor >= 0 && state.ActiveFloor < Level.Floors.Count
            && edge >= 0 && edge < Level.Floors[state.ActiveFloor].OpenEdges.Count
            && IsEdgeOpen(Level.Floors[state.ActiveFloor], state, edge);
        static bool IsEdgeOpen(FloorDefinition floor, PuzzleState state, int edge) =>
            !(floor.OneUseBridgeEdge == edge && state.BridgeUsed) && (floor.OpenEdges[edge]
            || (floor.Key != null && state.KeyUsed && floor.Key.EdgeToOpen == edge));

        static SymbolId[] Permute(IEnumerable<SymbolId> symbols, IEnumerable<int> edges)
        {
            var row = symbols.ToArray();
            foreach (int edge in edges) { var symbol = row[edge]; row[edge] = row[edge + 1]; row[edge + 1] = symbol; }
            return row;
        }

        PuzzleState ResolveContacts(PuzzleState state, List<PuzzleEvent> events)
        {
            var floor = Level.Floors[state.ActiveFloor];
            var bomb = floor.Bomb;
            if (bomb != null && state.BombPhase == BombPhase.Armed && state.Row[bomb.Slot] == bomb.RequiredSymbol)
            {
                state = state.With(bombPhase: BombPhase.Defused, bombTicks: 0, defusedCount: state.DefusedCount + 1);
                events.Add(new PuzzleEvent(PuzzleEventKind.BombDefused, state, bomb.Slot));
            }
            if (floor.Key != null && !state.KeyUsed && state.Row[floor.Key.Slot] == floor.Key.RequiredSymbol)
            {
                state = state.With(keyUsed: true);
                events.Add(new PuzzleEvent(PuzzleEventKind.KeyOpened, state, floor.Key.EdgeToOpen));
            }
            if (floor.Repair != null && !state.RepairUsed && state.Row[floor.Repair.Slot] == floor.Repair.RequiredSymbol)
            {
                state = state.With(repairUsed: true, durability: state.Durability + floor.Repair.Amount);
                events.Add(new PuzzleEvent(PuzzleEventKind.Repaired, state, floor.Repair.Amount));
            }
            int stampBit = 1 << state.ActiveFloor;
            if (floor.OptionalStamp != null && (state.StampBits & stampBit) == 0
                && state.Row[floor.OptionalStamp.Slot] == floor.OptionalStamp.RequiredSymbol)
            {
                state = state.With(stampBits: state.StampBits | stampBit);
                events.Add(new PuzzleEvent(PuzzleEventKind.StampCollected, state, stampBit));
            }
            return state;
        }

        PuzzleState TickBomb(PuzzleState state, List<PuzzleEvent> events)
        {
            if (state.BombPhase != BombPhase.Armed) return state;
            var bomb = Level.Floors[state.ActiveFloor].Bomb;
            state = state.With(bombTicks: state.BombTicks - 1);
            events.Add(new PuzzleEvent(PuzzleEventKind.BombTicked, state, state.BombTicks));
            if (state.BombTicks == 0)
            {
                state = state.With(bombPhase: BombPhase.Exploded, durability: state.Durability - bomb.Damage,
                    explodedCount: state.ExplodedCount + 1);
                events.Add(new PuzzleEvent(PuzzleEventKind.BombExploded, state, bomb.Damage));
            }
            return state;
        }

        PuzzleState LeaveFloor(PuzzleState state, List<PuzzleEvent> events)
        {
            if (state.BombPhase == BombPhase.Armed)
            {
                state = state.With(bombPhase: BombPhase.Abandoned, abandonedCount: state.AbandonedCount + 1);
                events.Add(new PuzzleEvent(PuzzleEventKind.BombAbandoned, state));
            }
            events.Add(new PuzzleEvent(PuzzleEventKind.FloorExited, state));
            return state;
        }

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
                bombPhase: floor.Bomb == null ? BombPhase.Absent : BombPhase.Armed,
                bombTicks: floor.Bomb == null ? 0 : floor.Bomb.Ticks, keyUsed: false, repairUsed: false, bridgeUsed: false);
            events.Add(new PuzzleEvent(PuzzleEventKind.FloorEntered, entered, floor.BaseDurability));
            if (floor.EntryTransform != EntryTransform.None)
            {
                var row = entered.Row.ToArray();
                switch (floor.EntryTransform)
                {
                    case EntryTransform.ShiftLeftOne: row = row.Skip(1).Concat(row.Take(1)).ToArray(); break;
                    case EntryTransform.ShiftRightOne: row = row.Skip(row.Length - 1).Concat(row.Take(row.Length - 1)).ToArray(); break;
                    case EntryTransform.MirrorNext: Array.Reverse(row); break;
                }
                entered = entered.With(row: row);
                events.Add(new PuzzleEvent(PuzzleEventKind.EntryPermuted, entered, (int)floor.EntryTransform));
            }
            return ResolveContacts(entered, events);
        }
    }
}
