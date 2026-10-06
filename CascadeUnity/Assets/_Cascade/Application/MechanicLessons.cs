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
        Mirror=11, LandingWind=12, Gust=13, Wayfinder=14, UndoSpool=15, ReturnHourglass=16, SignalLantern=17, LandingLens=18
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
                if(wind.Contains(i))found.Add(MechanicLesson.LandingWind);
                else if(f.EntryTransform==EntryTransform.MirrorNext)found.Add(MechanicLesson.Mirror);
                else if(f.EntryTransform!=EntryTransform.None)found.Add(MechanicLesson.Rollers);
            }
            return found.Distinct().ToArray();
        }
        public static MechanicLesson Tool(TravelTool tool)=>tool==TravelTool.Wayfinder?MechanicLesson.Wayfinder:tool==TravelTool.UndoSpool?MechanicLesson.UndoSpool:tool==TravelTool.ReturnHourglass?MechanicLesson.ReturnHourglass:tool==TravelTool.SignalLantern?MechanicLesson.SignalLantern:MechanicLesson.LandingLens;
        public static bool Valid(IEnumerable<MechanicLesson> seen)=>seen!=null&&seen.All(x=>Enum.IsDefined(typeof(MechanicLesson),x))&&seen.Distinct().Count()==seen.Count();
    }
}
