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
        public string NextNode;
        public int Stars,Score,Moves,Minimum,Undos,Stages,Coins,Lights,ParcelOrder;
        public bool HintUsed,NewBest,StatsSeen,Acknowledged;
        [Newtonsoft.Json.JsonProperty(DefaultValueHandling=Newtonsoft.Json.DefaultValueHandling.Ignore)] public bool HalfStar;
        [Newtonsoft.Json.JsonIgnore] public int StarHalfUnits=>Stars*2+(HalfStar?1:0);
        [Newtonsoft.Json.JsonIgnore] public double StarRating=>StarHalfUnits/2.0;
        public static string FormatStars(double value)=>value.ToString("0.#",System.Globalization.CultureInfo.InvariantCulture);
        public int RatingVersion;
        public double? ActiveSeconds;
        public static double FastTimeLimit(int minimum,int stages)=>Math.Max(45,minimum*8+stages*10);
        public static int Rate(int moves,int? minimum,int undos,bool assisted,bool clean,double? seconds,int stages)
        {
            if(!minimum.HasValue||minimum.Value<0||moves<minimum.Value||undos<0)return 1;
            int extra=Math.Max(0,moves-minimum.Value),allowance=Math.Max(4,minimum.Value/3);
            if(clean){
                if(extra==0&&undos==0&&seconds.HasValue&&seconds.Value<=FastTimeLimit(minimum.Value,stages))return 3;
                if(extra<=allowance&&undos<=2)return 2;
            }
            return 1;
        }
        public static int RateHalfUnits(int moves,int? minimum,int undos,bool assisted,bool clean,double? seconds,int stages)
        {
            int whole=Rate(moves,minimum,undos,assisted,clean,seconds,stages);
            if(whole==3)return 6;
            if(!minimum.HasValue||minimum.Value<0||moves<minimum.Value||undos<0||!clean)return whole*2;
            int extra=moves-minimum.Value,allowance=Math.Max(4,minimum.Value/3);
            if(extra==0&&undos<=1)return 5;
            if(whole==2)return 4;
            if(extra<=allowance*2&&undos<=3)return 3;
            return 2;
        }
        public static LevelCompletion From(AttemptService attempt,int? minimum,DeliveryPerformance performance=null)
        {
            if(attempt.Current.Status!=PuzzleStatus.Won)throw new ArgumentException("Only a delivered level has a completion.");
            int moves=performance?.MovesMade??attempt.MovesMade,undos=performance?.Undos??attempt.UndoCount;
            int? best=performance?.Minimum??minimum;bool hint=performance?.HintUsed??attempt.HintUsed;
            int extra=Math.Max(0,moves-(best??moves));bool clean=(performance?.Exploded??attempt.Current.ExplodedCount)==0;
            int stages=performance==null?1:3;double? seconds=performance==null?attempt.ActiveSeconds:performance.ActiveSeconds;
            int halfUnits=RateHalfUnits(moves,best,undos,hint,clean,seconds,stages),stars=halfUnits/2;
            return new LevelCompletion{NodeId=attempt.Level.CampaignNodeId,AttemptId=attempt.AttemptId.ToString("D"),
                RatingVersion=3,ActiveSeconds=seconds,Stars=stars,HalfStar=(halfUnits%2)!=0,Score=300*stages+Math.Max(0,600-40*extra)+50*halfUnits+((performance!=null?(performance.ToolFlags&4)!=0:attempt.Current.StampBits!=0)?100:0),
                Moves=moves,Minimum=best??0,Undos=undos,Stages=stages,HintUsed=hint};
        }
        public bool IsValid()=>!string.IsNullOrWhiteSpace(NodeId)&&NodeId.Length<=128&&Guid.TryParse(AttemptId,out var id)&&id!=Guid.Empty
            &&RatingVersion>=0&&RatingVersion<=3&&(!HalfStar||RatingVersion==3&&Stars>=1&&Stars<=2)&&Stars>=1&&Stars<=(RatingVersion==1?5:3)&&Score>=0&&Score<=(RatingVersion==0?1900:2100)&&Moves>=0&&Moves<=100000
            &&(!ActiveSeconds.HasValue||!double.IsNaN(ActiveSeconds.Value)&&!double.IsInfinity(ActiveSeconds.Value)&&ActiveSeconds.Value>=0&&ActiveSeconds.Value<=1814400)&&Minimum>=0&&Minimum<=10000
            &&Undos>=0&&Undos<=10000&&(Stages==1||Stages==3)&&Coins>=0&&Coins<=140&&Lights>=0&&Lights<=1&&ParcelOrder>=0&&ParcelOrder<=540
            &&(NextNode==null||!string.IsNullOrWhiteSpace(NextNode)&&NextNode.Length<=128);
    }
    public static class CampaignProgress
    {
        public static string Next(IReadOnlyList<string> ordered,IEnumerable<string> completed,IEnumerable<string> inserted=null)
        {
            var clears=new HashSet<string>(completed,StringComparer.Ordinal);var additions=new HashSet<string>(inserted??Array.Empty<string>(),StringComparer.Ordinal);
            int frontier=-1;for(int i=0;i<ordered.Count;i++)if(clears.Contains(ordered[i]))frontier=i;
            for(int i=0;i<ordered.Count;i++)if(!clears.Contains(ordered[i])&&!(i<frontier&&additions.Contains(ordered[i])))return ordered[i];
            return null;
        }
        public static bool IsOpen(IReadOnlyList<string> ordered,IEnumerable<string> completed,string node,IEnumerable<string> inserted=null)
        {
            var clears=new HashSet<string>(completed,StringComparer.Ordinal);if(!ordered.Contains(node))return false;
            if(clears.Contains(node)||Next(ordered,clears,inserted)==node)return true;
            if(inserted==null||!inserted.Contains(node))return false;
            int position=-1,frontier=-1;for(int i=0;i<ordered.Count;i++){if(ordered[i]==node)position=i;if(clears.Contains(ordered[i]))frontier=i;}
            return position<frontier;
        }
        public static int Remaining(IEnumerable<string> step,IEnumerable<string> completed)
        {var clears=new HashSet<string>(completed,StringComparer.Ordinal);return step.Count(n=>!clears.Contains(n));}
    }
}
