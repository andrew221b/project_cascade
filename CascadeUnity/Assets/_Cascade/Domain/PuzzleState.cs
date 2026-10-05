using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cascade.Domain
{
    public enum PuzzleStatus { Playing, Won, Lost }
    public enum BombPhase { Absent, Armed, Defused, Exploded, Abandoned }

    public sealed class PuzzleState : IEquatable<PuzzleState>
    {
        public ReadOnlyCollection<SymbolId> Row { get; }
        public int ActiveFloor { get; }
        public int Durability { get; }
        public BombPhase BombPhase { get; }
        public int BombTicks { get; }
        public bool KeyUsed { get; }
        public bool RepairUsed { get; }
        public bool BridgeUsed { get; }
        public int StampBits { get; }
        public int DefusedCount { get; }
        public int ExplodedCount { get; }
        public int AbandonedCount { get; }
        public PuzzleStatus Status { get; }
        public int ReachCharges { get; }
        public bool PowerupUsed { get; }
        public int FrozenTicks { get; }

        internal PuzzleState(IEnumerable<SymbolId> row, int activeFloor, int durability,
            BombPhase bombPhase = BombPhase.Absent, int bombTicks = 0, bool keyUsed = false,
            bool repairUsed = false, bool bridgeUsed = false, int stampBits = 0,
            int defusedCount = 0, int explodedCount = 0, int abandonedCount = 0,
            PuzzleStatus status = PuzzleStatus.Playing, int reachCharges = 0, bool powerupUsed = false, int frozenTicks = 0)
        {
            Row = Frozen.Copy(row); ActiveFloor = activeFloor; Durability = durability;
            BombPhase = bombPhase; BombTicks = bombTicks; KeyUsed = keyUsed; RepairUsed = repairUsed;
            BridgeUsed = bridgeUsed; StampBits = stampBits; DefusedCount = defusedCount;
            ExplodedCount = explodedCount; AbandonedCount = abandonedCount; Status = status; ReachCharges = reachCharges; PowerupUsed=powerupUsed; FrozenTicks=frozenTicks;
        }

        internal PuzzleState With(IEnumerable<SymbolId> row = null, int? activeFloor = null,
            int? durability = null, BombPhase? bombPhase = null, int? bombTicks = null,
            bool? keyUsed = null, bool? repairUsed = null, bool? bridgeUsed = null,
            int? stampBits = null, int? defusedCount = null, int? explodedCount = null,
            int? abandonedCount = null, PuzzleStatus? status = null, int? reachCharges = null, bool? powerupUsed = null, int? frozenTicks = null) =>
            new PuzzleState(row ?? Row, activeFloor ?? ActiveFloor, durability ?? Durability,
                bombPhase ?? BombPhase, bombTicks ?? BombTicks, keyUsed ?? KeyUsed,
                repairUsed ?? RepairUsed, bridgeUsed ?? BridgeUsed, stampBits ?? StampBits,
                defusedCount ?? DefusedCount, explodedCount ?? ExplodedCount,
                abandonedCount ?? AbandonedCount, status ?? Status, reachCharges ?? ReachCharges, powerupUsed ?? PowerupUsed, frozenTicks ?? FrozenTicks);

        public bool Equals(PuzzleState other) => other != null && ActiveFloor == other.ActiveFloor
            && Durability == other.Durability && BombPhase == other.BombPhase && BombTicks == other.BombTicks
            && KeyUsed == other.KeyUsed && RepairUsed == other.RepairUsed && BridgeUsed == other.BridgeUsed
            && StampBits == other.StampBits && DefusedCount == other.DefusedCount
            && ExplodedCount == other.ExplodedCount && AbandonedCount == other.AbandonedCount
            && PowerupUsed==other.PowerupUsed && FrozenTicks==other.FrozenTicks && ReachCharges == other.ReachCharges && Status == other.Status && Row.SequenceEqual(other.Row);
        public override bool Equals(object obj) => Equals(obj as PuzzleState);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var symbol in Row) hash = hash * 31 + symbol.GetHashCode();
                hash = hash * 31 + ActiveFloor; hash = hash * 31 + Durability;
                hash = hash * 31 + (int)BombPhase; hash = hash * 31 + BombTicks;
                hash = hash * 31 + (KeyUsed ? 1 : 0); hash = hash * 31 + (RepairUsed ? 1 : 0);
                hash = hash * 31 + (BridgeUsed ? 1 : 0); hash = hash * 31 + StampBits;
                hash = hash * 31 + DefusedCount; hash = hash * 31 + ExplodedCount;
                hash = hash * 31 + AbandonedCount; return (hash * 31 + (int)Status) ^ ReachCharges * 7919 ^ (PowerupUsed?104729:0) ^ FrozenTicks*13007;
            }
        }
    }

    public enum CommandKind { Invalid, Swap, EarlyDrop, LongReach }
    public readonly struct PuzzleCommand : IEquatable<PuzzleCommand>
    {
        public CommandKind Kind { get; }
        public int Edge { get; }
        public int End { get; }
        private PuzzleCommand(CommandKind kind, int edge, int end = -1) { Kind = kind; Edge = edge; End = end; }
        public static PuzzleCommand Reach(int first, int last) => new PuzzleCommand(CommandKind.LongReach, Math.Min(first,last), Math.Max(first,last));
        public static PuzzleCommand Swap(int edge) => new PuzzleCommand(CommandKind.Swap, edge);
        public static PuzzleCommand Drop => new PuzzleCommand(CommandKind.EarlyDrop, -1);
        public bool Equals(PuzzleCommand other) => Kind == other.Kind && Edge == other.Edge && End == other.End;
        public override bool Equals(object obj) => obj is PuzzleCommand other && Equals(other);
        public override int GetHashCode() => (int)Kind * 397 ^ Edge ^ (End + 1) * 7919;
        public override string ToString() => Kind == CommandKind.LongReach ? "R" + Edge + ":" + End : Kind == CommandKind.EarlyDrop ? "D" : Kind == CommandKind.Swap ? Edge.ToString() : "Invalid";
    }
}
