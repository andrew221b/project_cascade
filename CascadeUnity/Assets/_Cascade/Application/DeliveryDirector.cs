using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cascade.Domain;

namespace Cascade.Application
{
    public enum DeliveryMood { Steady, Tight, Recovery, Wind }

    // Optional save recipe. It binds a fresh delivery to its published source.
    public sealed class DirectionRecipe
    {
        public int Version=1;
        public string SourceHash;
        public DeliveryMood Mood;
        public int[] Durability;
        public int WindFloor=-1;
        public bool WindRight;
        public DirectionRecipe Copy()=>new DirectionRecipe { Version=Version,SourceHash=SourceHash,Mood=Mood,
            Durability=Durability?.ToArray(),WindFloor=WindFloor,WindRight=WindRight };
        public LevelDefinition Apply(LevelDefinition source)
        {
            if(Version!=1||source==null||source.ContentHash!=SourceHash||!Enum.IsDefined(typeof(DeliveryMood),Mood)
                ||Mood==DeliveryMood.Steady||Durability==null||Durability.Length!=source.Floors.Count
                ||WindFloor < -1||WindFloor>=source.Floors.Count||WindFloor==0)
                throw new ArgumentException("Invalid directed delivery recipe.");
            if(WindFloor>=0&&source.Floors[WindFloor].EntryTransform!=EntryTransform.None)
                throw new ArgumentException("Wind must not replace an existing entry mechanism.");
            for(int i=0;i<Durability.Length;i++) {
                int delta=Durability[i]-source.Floors[i].BaseDurability;
                if(Durability[i]<1||Durability[i]>12||delta < -1||delta>1
                    ||(Mood==DeliveryMood.Tight&&(i!=Durability.Length-1?delta!=0:delta!=-1))
                    ||(Mood==DeliveryMood.Recovery&&delta<0)
                    ||(Mood==DeliveryMood.Wind&&delta!=(i==WindFloor&&source.Floors[i].BaseDurability<12?1:0)))
                    throw new ArgumentException("Directed wood allowance is outside its bounds.");
            }
            if((Mood==DeliveryMood.Wind&&WindFloor<1)||(Mood==DeliveryMood.Tight&&WindFloor!=-1))
                throw new ArgumentException("Invalid wind placement for delivery mood.");
            string fingerprint="director-v1|"+SourceHash+"|"+(int)Mood+"|"+
                string.Join(",",Durability.Select(n=>n.ToString(CultureInfo.InvariantCulture)))+"|"+WindFloor+"|"+(WindRight?1:0);
            string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(fingerprint))).Replace("-","").ToLowerInvariant();
            var floors=source.Floors.Select((f,i)=>new FloorDefinition(Durability[i],f.OpenEdges,f.FragileEdges,f.LinkedPair,
                i==WindFloor?(WindRight?EntryTransform.ShiftRightOne:EntryTransform.ShiftLeftOne):f.EntryTransform,
                f.Bomb,f.Key,f.Repair,f.OneUseBridgeEdge,f.OptionalStamp,f.Powerup)).ToArray();
            return new LevelDefinition(source.PuzzleId,source.CampaignNodeId,source.SchemaVersion,source.RulesVersion,
                hash,source.InitialSymbolIds,source.TargetSymbolIds,source.TargetSegments,floors,source.ReachCharges);
        }
    }

    public sealed class DirectorMemory
    {
        public int CleanWins,Struggle,Deliveries,SincePeak=3,SinceWind=3;
        public string LastTerminalAttempt,LastRetryAttempt;
        public DeliveryMood Choose(LevelDefinition source)
        {
            if(!DeliveryDirector.CanDirect(source))return DeliveryMood.Steady;
            if(Struggle>=2||(Deliveries>0&&SincePeak==0))return DeliveryMood.Recovery;
            if(CleanWins<2||SincePeak<2)return DeliveryMood.Steady;
            return SinceWind>=3&&DeliveryDirector.WindFloor(source)>=1?DeliveryMood.Wind:DeliveryMood.Tight;
        }
        public void ObserveTerminal(AttemptService attempt,int? minimum,DeliveryPerformance performance=null)
        {
            string id=attempt.AttemptId.ToString("D");
            if(attempt.Current.Status==PuzzleStatus.Playing||LastTerminalAttempt==id)return;
            LastTerminalAttempt=id;
            if(attempt.Current.Status==PuzzleStatus.Lost){Struggle=Math.Min(6,Struggle+2);CleanWins=0;return;}
            bool clean=performance!=null?performance.Route<=performance.Minimum+1&&!performance.HintUsed&&performance.Undos<=1&&performance.Exploded==0
                :minimum.HasValue&&attempt.RouteLength<=minimum.Value+1&&!attempt.HintUsed&&attempt.UndoCount<=1&&attempt.Current.ExplodedCount==0;
            CleanWins=clean?Math.Min(6,CleanWins+1):0;Struggle=Math.Max(0,Struggle-(clean?2:1));
            Deliveries=Math.Min(10000,Deliveries+1);SincePeak=Math.Min(9,SincePeak+1);SinceWind=Math.Min(9,SinceWind+1);
            if(attempt.Direction?.Mood==DeliveryMood.Tight||attempt.Direction?.Mood==DeliveryMood.Wind)SincePeak=0;
            if(attempt.Direction?.Mood==DeliveryMood.Wind)SinceWind=0;
        }
        public void ObserveRetry(AttemptService attempt)
        {
            ObserveRetry(attempt.AttemptId,attempt.Current.Status==PuzzleStatus.Playing&&attempt.RouteLength>0);
        }
        public void ObserveRetry(Guid attemptId,bool hadMoves)
        {
            string id=attemptId.ToString("D");
            if(!hadMoves||LastRetryAttempt==id)return;
            LastRetryAttempt=id;Struggle=Math.Min(6,Struggle+1);CleanWins=0;
        }
        public bool IsValid()=>CleanWins>=0&&CleanWins<=6&&Struggle>=0&&Struggle<=6&&Deliveries>=0&&Deliveries<=10000
            &&SincePeak>=0&&SincePeak<=9&&SinceWind>=0&&SinceWind<=9
            &&(LastTerminalAttempt==null||Guid.TryParse(LastTerminalAttempt,out _))
            &&(LastRetryAttempt==null||Guid.TryParse(LastRetryAttempt,out _));
    }

    public static class DeliveryDirector
    {
        public static bool CanDirect(LevelDefinition source)=>source!=null&&source.CampaignNodeId.StartsWith("C",StringComparison.Ordinal)
            &&int.TryParse(source.CampaignNodeId.Substring(1),out int node)&&node>=41&&node<=160;
        public static int WindFloor(LevelDefinition source)
        {for(int i=1;i<source.Floors.Count;i++)if(source.Floors[i].EntryTransform==EntryTransform.None)return i;return -1;}
        public static DirectionRecipe Plan(LevelDefinition source,DeliveryMood mood,DirectionRecipe previous=null)
        {
            if(!CanDirect(source)||mood==DeliveryMood.Steady)return null;
            var recipe=new DirectionRecipe {SourceHash=source.ContentHash,Mood=mood,Durability=source.Floors.Select(f=>f.BaseDurability).ToArray()};
            if(mood==DeliveryMood.Tight){int last=recipe.Durability.Length-1;if(recipe.Durability[last]<=1)return null;recipe.Durability[last]--;}
            if(mood==DeliveryMood.Recovery) {
                for(int i=0;i<recipe.Durability.Length;i++)recipe.Durability[i]=Math.Min(12,recipe.Durability[i]+1);
                if(previous!=null){recipe.WindFloor=previous.WindFloor;recipe.WindRight=previous.WindRight;}
            }
            if(mood==DeliveryMood.Wind) {
                recipe.WindFloor=WindFloor(source);if(recipe.WindFloor<1)return null;
                recipe.WindRight=int.Parse(source.CampaignNodeId.Substring(1))%2==0;
                recipe.Durability[recipe.WindFloor]=Math.Min(12,recipe.Durability[recipe.WindFloor]+1);
            }
            return recipe;
        }
        public static bool MatchesSource(AttemptService attempt,LevelDefinition source)
        {
            if(source.PuzzleId!=attempt.Level.PuzzleId||source.CampaignNodeId!=attempt.Level.CampaignNodeId)return false;
            if(attempt.Direction==null)return source.ContentHash==attempt.PinnedContentHash;
            if(!CanDirect(source)||source.ContentHash!=attempt.Direction.SourceHash)return false;
            try{return attempt.Direction.Apply(source).ContentHash==attempt.PinnedContentHash;}
            catch(ArgumentException){return false;}
        }
    }
}
