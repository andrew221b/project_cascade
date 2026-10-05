using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cascade.Domain;
using Cascade.Solver;

namespace Cascade.Application
{
    public sealed class DescentStage
    {
        public EntryTransform Arrival;
        public int[] Wood;
        public int Minimum;
        public string[] Witness;
        internal string Fingerprint => (int)Arrival+":"+string.Join(",",Wood??Array.Empty<int>())+":"+Minimum+":"+string.Join(",",Witness??Array.Empty<string>());
    }
    public sealed class DeliveryPerformance
    {
        public int Route,Undos,Minimum,Exploded;
        public bool HintUsed;
        public string ContentHash;
        public string SourceHash;
    }

    // A checkpoint sequence owns only recipes and replay proofs. Each stage uses the same resolver.
    public sealed class DescentRun
    {
        public int Version=1;
        public string SourceHash;
        public DirectionRecipe Direction;
        public DescentStage[] Stages;
        public List<AttemptRecord> Completed=new List<AttemptRecord>();
        public int StageIndex=>Completed.Count;
        public bool IsFinal=>StageIndex==2;
        public string ContentHash=>Hash("descent-v1|"+SourceHash+"|"+DirectionKey()+"|"+string.Join("|",Stages.Select(s=>s?.Fingerprint??"pending")));
        public string StageHash(int stage)=>Hash(ContentHash+"|stage|"+stage);
        string DirectionKey()=>Direction==null?"steady":((int)Direction.Mood+":"+string.Join(",",Direction.Durability)+":"+Direction.WindFloor+":"+Direction.WindRight);
        static string Hash(string value){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-","").ToLowerInvariant();}
        LevelDefinition Directed(LevelDefinition source)=>Direction?.Apply(source)??source;
        public LevelDefinition Level(LevelDefinition source,int stage)
        {
            if(stage<0||stage>2||source.ContentHash!=SourceHash)throw new ArgumentException("Unavailable descent source.");
            var basis=Directed(source);var recipe=Stages[stage];int n=basis.TargetSymbolIds.Count;
            var open=Enumerable.Repeat(true,n-1).ToArray();var initial=basis.TargetSymbolIds.ToArray();
            if(stage==0) {
                int first=Enumerable.Range(0,n-1).First(i=>initial[i]!=initial[i+1]);
                Swap(initial,first);int second=Enumerable.Range(0,n-1).FirstOrDefault(i=>i!=first&&initial[i]!=initial[i+1]);
                if(second!=first&&initial[second]!=initial[second+1])Swap(initial,second);
            }
            FloorDefinition[] floors;
            if(stage==0)floors=new[]{new FloorDefinition(recipe.Wood[0],open)};
            else if(stage==1)floors=new[]{new FloorDefinition(recipe.Wood[0],open,entryTransform:recipe.Arrival),new FloorDefinition(recipe.Wood[1],open)};
            else floors=basis.Floors.Select((f,i)=>new FloorDefinition(recipe.Wood[i],f.OpenEdges,f.FragileEdges,f.LinkedPair,
                i==0?recipe.Arrival:f.EntryTransform,f.Bomb,f.Key,f.Repair,f.OneUseBridgeEdge,f.OptionalStamp,f.Powerup)).ToArray();
            return new LevelDefinition(basis.PuzzleId,basis.CampaignNodeId,basis.SchemaVersion,basis.RulesVersion,
                StageHash(stage),initial,basis.TargetSymbolIds,basis.TargetSegments,floors,stage==2?basis.ReachCharges:0);
        }
        static void Swap(SymbolId[] row,int edge){var a=row[edge];row[edge]=row[edge+1];row[edge+1]=a;}
        static bool Prove(DescentRun run,LevelDefinition source,int stage,CancellationToken cancellation,int budget)
        {
            var result=PuzzleSolver.FindShortest(new PuzzleResolver(run.Level(source,stage)),stateBudget:budget,cancellationToken:cancellation);
            if(result.Status!=SearchStatus.Solved)return false;
            run.Stages[stage].Minimum=result.OptimalActions.Value;run.Stages[stage].Witness=result.Path.Select(c=>c.ToString()).ToArray();return true;
        }
        public static DescentRun Build(LevelDefinition source,DeliveryMood mood=DeliveryMood.Steady,
            CancellationToken cancellation=default,int budget=PuzzleSolver.DefaultStateBudget)
        {
            if(!DeliveryDirector.CanDirect(source)||source.TargetSymbolIds.Distinct().Count()<2)return null;
            var direction=DeliveryDirector.Plan(source,mood);
            var run=new DescentRun {SourceHash=source.ContentHash,Direction=direction,Stages=new DescentStage[3]};
            var basis=run.Directed(source);int n=basis.TargetSymbolIds.Count;
            run.Stages[0]=new DescentStage{Wood=new[]{2}};
            run.Stages[1]=new DescentStage{Arrival=int.Parse(source.CampaignNodeId.Substring(1))%2==0?EntryTransform.ShiftRightOne:EntryTransform.ShiftLeftOne,
                Wood=new[]{Math.Max(1,(n-1)/2),Math.Max(1,n-(n-1)/2)}};
            if(!Prove(run,source,0,cancellation,budget)||!Prove(run,source,1,cancellation,budget)
                ||run.Stages[1].Minimum<=run.Stages[0].Minimum)return null;
            // The device finale retains the published topology and contacts. Try at most three arrivals.
            bool found=false;
            foreach(var arrival in new[]{EntryTransform.MirrorNext,EntryTransform.ShiftRightOne,EntryTransform.ShiftLeftOne}) {
                if(cancellation.IsCancellationRequested)return null;
                int allowance=arrival==EntryTransform.MirrorNext?n*(n-1)/2+2:n+3;
                var wood=basis.Floors.Select(f=>Math.Min(12,Math.Max(f.BaseDurability,(allowance+basis.Floors.Count-1)/basis.Floors.Count))).ToArray();
                run.Stages[2]=new DescentStage{Arrival=arrival,Wood=wood};
                if(Prove(run,source,2,cancellation,budget)&&run.Stages[2].Minimum>=run.Stages[1].Minimum){found=true;break;}
            }
            if(!found)return null;
            // Trim only two candidate units: all devices and automatic falls are included in each proof.
            for(int trim=0;trim<(mood==DeliveryMood.Tight?3:2);trim++) {
                int f=run.Stages[2].Wood.Length-1-trim%run.Stages[2].Wood.Length;
                var previous=run.Stages[2];if(previous.Wood[f]<=1)continue;
                var wood=previous.Wood.ToArray();wood[f]--;
                run.Stages[2]=new DescentStage{Arrival=previous.Arrival,Wood=wood};
                if(!Prove(run,source,2,cancellation,budget)||run.Stages[2].Minimum<run.Stages[1].Minimum)run.Stages[2]=previous;
            }
            if(mood==DeliveryMood.Recovery) {
                var previous=run.Stages[2];run.Stages[2]=new DescentStage{Arrival=previous.Arrival,Wood=previous.Wood.Select(w=>Math.Min(12,w+1)).ToArray()};
                if(!Prove(run,source,2,cancellation,budget)||run.Stages[2].Minimum<run.Stages[1].Minimum)run.Stages[2]=previous;
            }
            return cancellation.IsCancellationRequested?null:run;
        }
        public void Validate(LevelDefinition source,string locale)
        {
            if(Version!=1||source.ContentHash!=SourceHash||!DeliveryDirector.CanDirect(source)||Stages==null||Stages.Length!=3
                ||Completed==null||Completed.Count>2||source.TargetSymbolIds.Distinct().Count()<2)throw new ArgumentException("Invalid descent checkpoint.");
            Directed(source);
            for(int i=0;i<3;i++) {
                var s=Stages[i];if(s==null||s.Wood==null||s.Wood.Length!=(i==0?1:i==1?2:source.Floors.Count)
                    ||s.Wood.Any(w=>w<1||w>12)||!Enum.IsDefined(typeof(EntryTransform),s.Arrival)
                    ||(i==0&&s.Arrival!=EntryTransform.None)||(i==1&&s.Arrival!=EntryTransform.ShiftLeftOne&&s.Arrival!=EntryTransform.ShiftRightOne)
                    ||s.Minimum<1||s.Minimum>1000||s.Witness==null||s.Witness.Length!=s.Minimum)
                    throw new ArgumentException("Invalid descent stage recipe.");
                Replay(Level(source,i),locale,s.Witness);
                if(i<Completed.Count) {
                    var done=AttemptService.Restore(Level(source,i),Completed[i]);
                    if(done.Current.Status!=PuzzleStatus.Won||done.Goal.RequiredStampBits!=0||!done.Current.Row.SequenceEqual(source.TargetSymbolIds))throw new ArgumentException("Unfinished descent checkpoint.");
                }
            }
            if(Stages[0].Minimum>=Stages[1].Minimum||Stages[1].Minimum>Stages[2].Minimum)throw new ArgumentException("Invalid descent curve.");
        }
        static void Replay(LevelDefinition level,string locale,string[] commands)
        {
            var attempt=new AttemptService(level,locale);foreach(string encoded in commands) {
                PuzzleCommand command;if(encoded=="D")command=PuzzleCommand.Drop;
                else if(int.TryParse(encoded,out int edge)&&encoded==edge.ToString())command=PuzzleCommand.Swap(edge);
                else if(encoded!=null&&encoded.StartsWith("R",StringComparison.Ordinal)&&encoded.Split(':').Length==2
                    &&int.TryParse(encoded.Substring(1).Split(':')[0],out int first)&&int.TryParse(encoded.Split(':')[1],out int last)
                    &&encoded==PuzzleCommand.Reach(first,last).ToString())command=PuzzleCommand.Reach(first,last);
                else throw new ArgumentException("Invalid descent witness.");
                if(!attempt.Execute(command).Accepted)throw new ArgumentException("Broken descent proof.");
            }
            if(attempt.Current.Status!=PuzzleStatus.Won)throw new ArgumentException("Descent proof does not deliver its row.");
        }
        public void AcceptCheckpoint(LevelDefinition source,AttemptService attempt)
        {
            if(IsFinal||attempt.PinnedContentHash!=Level(source,StageIndex).ContentHash||attempt.Current.Status!=PuzzleStatus.Won
                ||attempt.Goal.RequiredStampBits!=0)throw new InvalidOperationException("The row must be delivered before going lower.");
            Completed.Add(AttemptRecord.Capture(attempt));
        }
        public DeliveryPerformance Performance(AttemptService attempt)=>new DeliveryPerformance {
            Route=Completed.Sum(a=>a.Commands.Length)+attempt.RouteLength,Undos=Completed.Sum(a=>a.UndoCount)+attempt.UndoCount,
            HintUsed=Completed.Any(a=>a.HintUsed)||attempt.HintUsed,Minimum=Stages.Sum(s=>s.Minimum),
            Exploded=attempt.Current.ExplodedCount,ContentHash=ContentHash,SourceHash=SourceHash
        };
    }
}
