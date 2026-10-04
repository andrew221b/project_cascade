using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Cascade.Domain
{
    public enum PuzzleEventKind
    {
        CommandAccepted, RowPermuted, BombDefused, KeyOpened, Repaired, StampCollected,
        BombTicked, BombExploded, BombAbandoned, DurabilityResolved, BridgeConsumed,
        FloorExited, FloorEntered, EntryPermuted, ReceiverEvaluated
    }
    public enum RejectionReason { None, Terminal, InvalidCommand, InvalidState, ClosedEdge, NoOp }

    public sealed class PuzzleEvent
    {
        public PuzzleEventKind Kind { get; }
        public int Floor { get; }
        public int Value { get; }
        public ReadOnlyCollection<SymbolId> Row { get; }
        public ReadOnlyCollection<int> AffectedEdges { get; }
        internal PuzzleEvent(PuzzleEventKind kind, PuzzleState state, int value = 0, IEnumerable<int> edges = null)
        {
            Kind = kind; Floor = state.ActiveFloor; Value = value;
            Row = state.Row; AffectedEdges = Frozen.Copy(edges ?? System.Array.Empty<int>());
        }
    }

    public sealed class CommandPreview
    {
        public bool IsLegal => Rejection == RejectionReason.None;
        public RejectionReason Rejection { get; }
        public int DurabilityCost { get; }
        public ReadOnlyCollection<int> AffectedEdges { get; }
        internal CommandPreview(RejectionReason rejection, int durabilityCost = 0, IEnumerable<int> affectedEdges = null)
        { Rejection = rejection; DurabilityCost = durabilityCost; AffectedEdges = Frozen.Copy(affectedEdges ?? System.Array.Empty<int>()); }
    }

    public sealed class TransitionResult
    {
        public PuzzleState State { get; }
        public bool Accepted { get; }
        public int DurabilityCost { get; }
        public RejectionReason Rejection { get; }
        public ReadOnlyCollection<PuzzleEvent> Events { get; }
        internal TransitionResult(PuzzleState state, bool accepted, int cost,
            RejectionReason rejection, IEnumerable<PuzzleEvent> events)
        { State = state; Accepted = accepted; DurabilityCost = cost; Rejection = rejection; Events = Frozen.Copy(events); }
    }
}
