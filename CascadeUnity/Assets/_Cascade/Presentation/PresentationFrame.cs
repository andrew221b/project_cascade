using System.Collections.Generic;
using Cascade.Domain;

namespace Cascade.Presentation
{
    // A disposable picture of already committed events. Never passed to a resolver.
    public sealed class PresentationFrame
    {
        public IReadOnlyList<SymbolId> Row { get; private set; }
        public int ActiveFloor { get; private set; }
        public int Durability { get; private set; }
        public BombPhase BombPhase { get; private set; }
        public int BombTicks { get; private set; }
        public bool KeyUsed { get; private set; }
        public bool RepairUsed { get; private set; }
        public bool BridgeUsed { get; private set; }
        public int StampBits { get; private set; }
        public bool PowerupUsed { get; private set; }
        public int FrozenTicks { get; private set; }
        public PuzzleStatus Status { get; private set; }
        public int HiddenFloors { get; private set; }
        public PresentationFrame(PuzzleState state)
        {
            Row = state.Row; ActiveFloor = state.ActiveFloor; Durability = state.Durability;
            BombPhase = state.BombPhase; BombTicks = state.BombTicks; KeyUsed = state.KeyUsed;
            PowerupUsed=state.PowerupUsed;FrozenTicks=state.FrozenTicks;RepairUsed = state.RepairUsed; BridgeUsed = state.BridgeUsed; StampBits = state.StampBits; Status = state.Status;
            HiddenFloors = (1 << state.ActiveFloor) - 1;
        }
        public void Read(PuzzleEvent e, LevelDefinition level)
        {
            Row = e.Row;
            switch (e.Kind) {
                case PuzzleEventKind.FloorEntered:
                    ActiveFloor = e.Floor; Durability = e.Value; KeyUsed = RepairUsed = BridgeUsed = PowerupUsed = false; FrozenTicks=0;
                    var bomb = level.Floors[e.Floor].Bomb;
                    BombPhase = bomb == null ? BombPhase.Absent : BombPhase.Armed; BombTicks = bomb == null ? 0 : bomb.Ticks; break;
                case PuzzleEventKind.FloorExited: HiddenFloors |= 1 << e.Floor; break;
                case PuzzleEventKind.RibbonCollected: PowerupUsed=true;break;
                case PuzzleEventKind.HourglassCollected: PowerupUsed=true;FrozenTicks=2;break;
                case PuzzleEventKind.BombFrozen: FrozenTicks=e.Value;break;
                case PuzzleEventKind.BombTicked: BombTicks = e.Value; break;
                case PuzzleEventKind.BombDefused: BombPhase = BombPhase.Defused; BombTicks = 0; break;
                case PuzzleEventKind.BombExploded: BombPhase = BombPhase.Exploded; break;
                case PuzzleEventKind.BombAbandoned: BombPhase = BombPhase.Abandoned; break;
                case PuzzleEventKind.KeyOpened: KeyUsed = true; break;
                case PuzzleEventKind.Repaired: RepairUsed = true; break;
                case PuzzleEventKind.BridgeConsumed: BridgeUsed = true; break;
                case PuzzleEventKind.StampCollected: StampBits |= e.Value; break;
                case PuzzleEventKind.DurabilityResolved: Durability = e.Value; break;
                case PuzzleEventKind.ReceiverEvaluated:
                    ActiveFloor = level.Floors.Count; Status = e.Value == 1 ? PuzzleStatus.Won : PuzzleStatus.Lost; break;
            }
        }
        public bool FloorVisible(int floor) => Status == PuzzleStatus.Playing && (HiddenFloors & (1 << floor)) == 0;
        public bool EdgeVisibleOpen(FloorDefinition floor, int edge) => !(floor.OneUseBridgeEdge == edge && BridgeUsed)
            && (floor.OpenEdges[edge] || (floor.Key != null && KeyUsed && floor.Key.EdgeToOpen == edge));
    }
}
