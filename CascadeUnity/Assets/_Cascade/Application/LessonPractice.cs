using System.Linq;
using Cascade.Domain;

namespace Cascade.Application
{
    // Observes a committed outcome only. A demonstration, pointer animation or
    // modal acknowledgement cannot complete practice or issue puzzle commands.
    public static class LessonPractice
    {
        public static bool CompletedBy(MechanicLesson lesson,int fromFloor,TransitionResult result,PuzzleCommand command)
        {
            if(result==null||!result.Accepted||fromFloor<0)return false;
            // These lessons complete only when their Application action or actual
            // ambient gust succeeds. Leaving a board does not use a tool or control.
            if(lesson==MechanicLesson.Gust||lesson>=MechanicLesson.Wayfinder&&lesson<=MechanicLesson.LandingLens
                ||lesson==MechanicLesson.Undo||lesson==MechanicLesson.Hint||lesson==MechanicLesson.Toolbag)return false;
            bool Entry(EntryTransform transform)=>result.Events.Any(e=>e.Kind==PuzzleEventKind.EntryPermuted
                &&e.Floor==fromFloor+1&&e.Value==(int)transform);
            if(lesson==MechanicLesson.Mirror)return Entry(EntryTransform.MirrorNext);
            if(lesson==MechanicLesson.Rollers||lesson==MechanicLesson.LandingWind)
                return Entry(EntryTransform.ShiftLeftOne)||Entry(EntryTransform.ShiftRightOne);
            bool Has(PuzzleEventKind kind)=>result.Events.Any(e=>e.Floor==fromFloor&&e.Kind==kind);
            // Leaving early can retire a gesture cue, but cannot teach a gesture
            // the player never performed. Presentation retires such unfinished cues.
            if(lesson==MechanicLesson.Swap)return command.Kind==CommandKind.Swap&&Has(PuzzleEventKind.RowPermuted);
            if(lesson==MechanicLesson.FarSwap)return command.Kind==CommandKind.LongReach&&Has(PuzzleEventKind.RowPermuted);
            // A vanished teaching board cannot keep its cue waiting for an
            // unreachable contact. Entry lessons above require the actual effect.
            if(Has(PuzzleEventKind.FloorExited))return true;
            if(!result.Events.Any(e=>e.Kind==PuzzleEventKind.CommandAccepted))return false;
            switch(lesson){
                case MechanicLesson.Descent:
                    return command.Kind==CommandKind.EarlyDrop&&Has(PuzzleEventKind.CommandAccepted);
                case MechanicLesson.Bomb:
                    return Has(PuzzleEventKind.BombDefused)||Has(PuzzleEventKind.BombExploded)||Has(PuzzleEventKind.BombAbandoned);
                case MechanicLesson.Key:return Has(PuzzleEventKind.KeyOpened);
                case MechanicLesson.Repair:return Has(PuzzleEventKind.Repaired);
                case MechanicLesson.Ribbon:return Has(PuzzleEventKind.RibbonCollected);
                case MechanicLesson.Stamp:return Has(PuzzleEventKind.StampCollected);
                case MechanicLesson.BombHourglass:return Has(PuzzleEventKind.HourglassCollected)||Has(PuzzleEventKind.BombFrozen);
                case MechanicLesson.Bridge:return Has(PuzzleEventKind.BridgeConsumed);
                case MechanicLesson.LinkedPairs:
                    return result.Events.Any(e=>e.Floor==fromFloor&&e.Kind==PuzzleEventKind.RowPermuted&&e.AffectedEdges.Count>1);
                case MechanicLesson.Fragile:return result.DurabilityCost>1&&Has(PuzzleEventKind.RowPermuted);
                case MechanicLesson.ClosedPath:return Has(PuzzleEventKind.RowPermuted);
                case MechanicLesson.LiveGust:return Has(PuzzleEventKind.WindPermuted);
                // Undo, hints, the bag and shop tools are successful Application
                // actions rather than resolver commands. Their owner observes them.
                default:return false;
            }
        }
    }
}
