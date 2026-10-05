using System;
using System.Linq;
using Cascade.Domain;

namespace Cascade.Application
{
    // Optional coaching observes stable snapshots. It cannot issue puzzle commands.
    public sealed class TutorialCoach
    {
        readonly LevelDefinition level;
        readonly string lesson;
        PuzzleState observed;
        RejectionReason lastInvalid;
        int invalidEdge = -1, repeatedInvalid;
        public bool Enabled { get; set; } = true;
        public bool LessonCompleted { get; private set; }
        public float IdleSeconds { get; private set; }
        public bool HighlightRequested => Enabled && IdleSeconds >= 8;
        public bool GestureReplayRequested => Enabled && IdleSeconds >= 15;
        public bool ExplainInvalid => Enabled && repeatedInvalid >= 2;
        public string Lesson => lesson;
        public TutorialCoach(LevelDefinition definition, string lessonId)
        { level = definition ?? throw new ArgumentNullException(nameof(definition)); lesson = lessonId ?? "swap"; }

        public void Activity() { IdleSeconds = 0; }
        public void Advance(float seconds, bool foregroundPlaying)
        {
            if (!Enabled || !foregroundPlaying || observed == null || observed.Status != PuzzleStatus.Playing) return;
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds <= 0) return;
            IdleSeconds = Math.Min(60, IdleSeconds + seconds);
        }
        public void Observe(PuzzleState state, int routeLength)
        {
            if (!ReferenceEquals(observed, state)) { observed = state; Activity(); repeatedInvalid = 0; }
            // Passing the teaching platform always retires its cue, even if it vanished
            // because of a blast, an early drop or a multi-event transition.
            if (state.Status != PuzzleStatus.Playing || state.ActiveFloor > 0) LessonCompleted = true;
            if (routeLength == 0) return;
            if (lesson == "bomb" || lesson == "bomb_repair")
                LessonCompleted |= state.DefusedCount + state.ExplodedCount + state.AbandonedCount > 0;
            else if (lesson == "key" || lesson == "key_linked" || lesson == "mixed_key") LessonCompleted |= state.KeyUsed;
            else if (lesson == "repair" || lesson == "fragile_repair" || lesson == "key_repair") LessonCompleted |= state.RepairUsed || state.KeyUsed;
            else if (lesson == "bridge" || lesson == "mixed_bridge") LessonCompleted |= state.BridgeUsed;
            else if (lesson == "stamp" || lesson == "stamp_conveyor") LessonCompleted |= state.StampBits != 0;
            else LessonCompleted = true;
        }
        public void Invalid(RejectionReason reason, int edge)
        {
            Activity(); repeatedInvalid = reason == lastInvalid && edge == invalidEdge ? repeatedInvalid + 1 : 1;
            lastInvalid = reason; invalidEdge = edge;
        }
        public string Prompt(PuzzleState state)
        {
            if (!Enabled || state.Status != PuzzleStatus.Playing) return "";
            if (ExplainInvalid) {
                if (lastInvalid == RejectionReason.NoOp) return "These pieces are the same. Try another pair.";
                var floor = level.Floors[state.ActiveFloor];
                return floor.Key != null && floor.Key.EdgeToOpen == invalidEdge
                    ? "Put the same piece on the key to open the gate."
                    : "This way is closed. Try the other side or Descend.";
            }
            if (state.Row.SequenceEqual(level.TargetSymbolIds)) return state.ActiveFloor == level.Floors.Count-1
                ? "Ready! Tap Deliver." : "Ready here. Check the board below.";
            if (LessonCompleted && !HighlightRequested && level.Floors[state.ActiveFloor].Powerup==null) return "";
            var f = level.Floors[state.ActiveFloor];
            if(f.Powerup!=null&&!state.PowerupUsed)return f.Powerup.Kind==PowerupKind.Ribbon?"Match the spool picture to earn one far swap.":"Match the hourglass picture to pause the bomb.";
            if(state.FrozenTicks>0)return "The bomb is paused. The wood still wears down.";
            if (f.Bomb != null && state.BombPhase == BombPhase.Armed) return "Put the same piece on the bomb to stop it.";
            if (f.Key != null && !state.KeyUsed) return "Put the same piece on the key.";
            if (f.Repair != null && !state.RepairUsed) return "Put the same piece on the patch to mend the board.";
            if (f.LinkedPair.HasValue) return "These two pairs swap together.";
            if (f.OneUseBridgeEdge.HasValue && !state.BridgeUsed) return "You can use this bridge once.";
            if (f.OptionalStamp != null && (state.StampBits & (1 << state.ActiveFloor)) == 0) return "Can you reach the stamp too?";
            if (f.FragileEdges.Count > 0) return "Careful! This crack wears the board down faster.";
            if (lesson == "conveyor" || lesson == "fragile_conveyor") return "The next board moves the row. Look at its arrow.";
            if (lesson == "drop") return "Tap Descend to take the row down.";
            if (lesson == "collapse") return "At zero, the board breaks and the row drops.";
            return "Drag a piece onto its neighbour.";
        }
    }
}
