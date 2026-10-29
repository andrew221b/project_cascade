using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Domain;

namespace Cascade.Application
{
    // Persistent tutorial identifiers. Append new values; never renumber existing ones.
    public enum MechanicLesson
    {
        Bomb=0, Key=1, Repair=2, Ribbon=3, Stamp=4, BombHourglass=5,
        Bridge=6, LinkedPairs=7, Fragile=8, ClosedPath=9, Rollers=10,
        Mirror=11, LandingWind=12, Gust=13, Wayfinder=14, UndoSpool=15, ReturnHourglass=16, SignalLantern=17, LandingLens=18, LiveGust=19,
        Swap=20, Descent=21, Undo=22, Hint=23, FarSwap=24, Toolbag=25, RepairKit=26
    }
    public static class MechanicLessons
    {
        public static IReadOnlyList<MechanicLesson> Discover(LevelDefinition level,int firstFloor,IEnumerable<int> windFloors)
        {
            var found=new List<MechanicLesson>();var wind=new HashSet<int>(windFloors);
            for(int i=Math.Max(0,firstFloor);i<level.Floors.Count;i++){
                var f=level.Floors[i];
                if(f.Bomb!=null)found.Add(MechanicLesson.Bomb);
                if(f.Key!=null)found.Add(MechanicLesson.Key);
                if(f.Repair!=null)found.Add(MechanicLesson.Repair);
                if(f.Powerup!=null)found.Add(f.Powerup.Kind==PowerupKind.Ribbon?MechanicLesson.Ribbon:MechanicLesson.BombHourglass);
                if(f.OptionalStamp!=null)found.Add(MechanicLesson.Stamp);
                if(f.OneUseBridgeEdge.HasValue)found.Add(MechanicLesson.Bridge);
                if(f.LinkedPair.HasValue)found.Add(MechanicLesson.LinkedPairs);
                if(f.FragileEdges.Count>0)found.Add(MechanicLesson.Fragile);
                if(f.Key==null&&f.OpenEdges.Where((open,edge)=>!open&&edge!=f.OneUseBridgeEdge).Any())found.Add(MechanicLesson.ClosedPath);
                if(f.GustAfterSwaps>0)found.Add(MechanicLesson.LiveGust);
                // The current floor's entry has already happened, including initial floor zero.
                if(i>firstFloor){
                    if(wind.Contains(i)&&f.EntryTransform!=EntryTransform.None)found.Add(MechanicLesson.LandingWind);
                    else if(f.EntryTransform==EntryTransform.MirrorNext)found.Add(MechanicLesson.Mirror);
                    else if(f.EntryTransform!=EntryTransform.None)found.Add(MechanicLesson.Rollers);
                }
            }
            return found.Distinct().ToArray();
        }
        // Explain an entry transform while the next board is visible, before the
        // player starts preparing the row. Never teach an already applied entry.
        public static IReadOnlyList<MechanicLesson> DiscoverVisible(LevelDefinition level,PuzzleState state,bool nextIsWind=false,bool includeNextMechanics=false)
        {
            if(level==null)throw new ArgumentNullException(nameof(level));
            if(state==null)throw new ArgumentNullException(nameof(state));
            var found=new List<MechanicLesson>();
            if(state.Status!=PuzzleStatus.Playing||state.ActiveFloor<0||state.ActiveFloor>=level.Floors.Count)return found;
            var f=level.Floors[state.ActiveFloor];
            if(f.Bomb!=null&&state.BombPhase==BombPhase.Armed)found.Add(MechanicLesson.Bomb);
            if(f.Key!=null&&!state.KeyUsed)found.Add(MechanicLesson.Key);
            if(f.Repair!=null&&!state.RepairUsed)found.Add(MechanicLesson.Repair);
            if(f.Powerup!=null&&!state.PowerupUsed)found.Add(f.Powerup.Kind==PowerupKind.Ribbon?MechanicLesson.Ribbon:MechanicLesson.BombHourglass);
            if(f.OptionalStamp!=null&&(state.StampBits&(1<<state.ActiveFloor))==0)found.Add(MechanicLesson.Stamp);
            if(f.OneUseBridgeEdge.HasValue&&!state.BridgeUsed)found.Add(MechanicLesson.Bridge);
            if(f.LinkedPair.HasValue)found.Add(MechanicLesson.LinkedPairs);
            if(f.FragileEdges.Count>0)found.Add(MechanicLesson.Fragile);
            // The key owns its branch lesson. Other closed branches still need an example.
            if(f.OpenEdges.Where((open,edge)=>!open&&edge!=f.OneUseBridgeEdge
                &&(f.Key==null||edge!=f.Key.EdgeToOpen)).Any())found.Add(MechanicLesson.ClosedPath);
            if(f.GustAfterSwaps>0&&state.GustTicks>0)found.Add(MechanicLesson.LiveGust);
            if(state.ReachCharges>0)found.Add(MechanicLesson.FarSwap);
            var next=state.ActiveFloor+1;
            if(next<level.Floors.Count){
                if(includeNextMechanics){
                    var upcoming=level.Floors[next];
                    if(upcoming.Bomb!=null)found.Add(MechanicLesson.Bomb);
                    if(upcoming.Key!=null)found.Add(MechanicLesson.Key);
                    if(upcoming.Repair!=null)found.Add(MechanicLesson.Repair);
                    if(upcoming.Powerup!=null)found.Add(upcoming.Powerup.Kind==PowerupKind.Ribbon?MechanicLesson.Ribbon:MechanicLesson.BombHourglass);
                    if(upcoming.OptionalStamp!=null)found.Add(MechanicLesson.Stamp);
                    if(upcoming.OneUseBridgeEdge.HasValue)found.Add(MechanicLesson.Bridge);
                    if(upcoming.LinkedPair.HasValue)found.Add(MechanicLesson.LinkedPairs);
                    if(upcoming.FragileEdges.Count>0)found.Add(MechanicLesson.Fragile);
                    if(upcoming.OpenEdges.Where((open,edge)=>!open&&edge!=upcoming.OneUseBridgeEdge&&(upcoming.Key==null||edge!=upcoming.Key.EdgeToOpen)).Any())found.Add(MechanicLesson.ClosedPath);
                    if(upcoming.GustAfterSwaps>0)found.Add(MechanicLesson.LiveGust);
                }
                var entry=level.Floors[next].EntryTransform;
                if(entry!=EntryTransform.None)found.Add(entry==EntryTransform.MirrorNext?MechanicLesson.Mirror:
                    nextIsWind?MechanicLesson.LandingWind:MechanicLesson.Rollers);
            }
            return found.Distinct().ToArray();
        }
        public static MechanicLesson Tool(TravelTool tool)=>tool==TravelTool.Wayfinder?MechanicLesson.Wayfinder:tool==TravelTool.UndoSpool?MechanicLesson.UndoSpool:tool==TravelTool.ReturnHourglass?MechanicLesson.ReturnHourglass:tool==TravelTool.SignalLantern?MechanicLesson.SignalLantern:tool==TravelTool.RepairKit?MechanicLesson.RepairKit:MechanicLesson.LandingLens;
        public static bool Valid(IEnumerable<MechanicLesson> seen)=>seen!=null&&seen.All(x=>Enum.IsDefined(typeof(MechanicLesson),x))&&seen.Distinct().Count()==seen.Count();
    }
}
