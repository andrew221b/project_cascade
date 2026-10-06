using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Domain;

namespace Cascade.Application
{
    // A committed receipt, never a reward issued by an animation or a menu callback.
    public sealed class LevelCompletion
    {
        public string NodeId,AttemptId;
        public int Stars,Score,Moves,Minimum,Undos,Stages,Coins,Lights,ParcelOrder;
        public bool HintUsed,NewBest,StatsSeen,Acknowledged;
        public static LevelCompletion From(AttemptService attempt,int? minimum,DeliveryPerformance performance=null)
        {
            if(attempt.Current.Status!=PuzzleStatus.Won)throw new ArgumentException("Only a delivered level has a completion.");
            int moves=performance?.Route??attempt.RouteLength,undos=performance?.Undos??attempt.UndoCount;
            int? best=performance?.Minimum??minimum;bool hint=performance?.HintUsed??attempt.HintUsed;
            int extra=Math.Max(0,moves-(best??moves));bool clean=(performance?.Exploded??attempt.Current.ExplodedCount)==0;
            int stars=best.HasValue&&clean&&!hint&&undos<=1&&extra<=Math.Max(1,best.Value/10)?3:
                best.HasValue&&clean&&extra<=Math.Max(3,best.Value/3)?2:1;
            int stages=performance==null?1:3;
            return new LevelCompletion{NodeId=attempt.Level.CampaignNodeId,AttemptId=attempt.AttemptId.ToString("D"),
                Stars=stars,Score=300*stages+Math.Max(0,600-40*extra)+100*stars+((performance!=null?(performance.ToolFlags&4)!=0:attempt.Current.StampBits!=0)?100:0),
                Moves=moves,Minimum=best??0,Undos=undos,Stages=stages,HintUsed=hint};
        }
        public bool IsValid()=>!string.IsNullOrWhiteSpace(NodeId)&&NodeId.Length<=128&&Guid.TryParse(AttemptId,out var id)&&id!=Guid.Empty
            &&Stars>=1&&Stars<=3&&Score>=0&&Score<=1900&&Moves>=0&&Moves<=10000&&Minimum>=0&&Minimum<=10000
            &&Undos>=0&&Undos<=10000&&(Stages==1||Stages==3)&&Coins>=0&&Coins<=140&&Lights>=0&&Lights<=1&&ParcelOrder>=0&&ParcelOrder<=12;
    }
    public static class CampaignProgress
    {
        public static string Next(IReadOnlyList<string> ordered,IEnumerable<string> completed)
        {var clears=new HashSet<string>(completed,StringComparer.Ordinal);return ordered.FirstOrDefault(n=>!clears.Contains(n));}
        public static bool IsOpen(IReadOnlyList<string> ordered,IEnumerable<string> completed,string node)
        {var clears=new HashSet<string>(completed,StringComparer.Ordinal);return ordered.Contains(node)&&(clears.Contains(node)||Next(ordered,clears)==node);}
        public static int Remaining(IEnumerable<string> step,IEnumerable<string> completed)
        {var clears=new HashSet<string>(completed,StringComparer.Ordinal);return step.Count(n=>!clears.Contains(n));}
    }
}
