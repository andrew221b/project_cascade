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
        public bool PublishedStart;
        internal string Fingerprint => (int)Arrival+":"+string.Join(",",Wood??Array.Empty<int>())+":"+Minimum+":"+string.Join(",",Witness??Array.Empty<string>())+(PublishedStart?":published-start":"");
    }
    public sealed class DeliveryPerformance
    {
        public int Route,MovesMade,Undos,Minimum,Exploded,ToolFlags;
        public double? ActiveSeconds;
        public bool HintUsed;
        public string ContentHash;
        public string SourceHash;
    }

    // A checkpoint sequence owns only recipes and replay proofs. Each stage uses the same resolver.
    public sealed class DescentRun
    {
        public int Version=2;
        public int CompletedExplosions;
        public int? CompletedToolFlags;
        public string SourceHash;
        public DirectionRecipe Direction;
        public DescentStage[] Stages;
        public List<AttemptRecord> Completed=new List<AttemptRecord>();
        public int StageIndex=>Completed.Count;
        public bool IsFinal=>StageIndex==2;
        public string ContentHash=>Hash("descent-v"+Version+"|"+SourceHash+"|"+DirectionKey()+"|"+string.Join("|",Stages.Select(s=>s?.Fingerprint??"pending")));
        public string StageHash(int stage)=>Hash(ContentHash+"|stage|"+stage);
        string DirectionKey()=>Direction==null?"steady":((int)Direction.Mood+":"+string.Join(",",Direction.Durability)+":"+Direction.WindFloor+":"+Direction.WindRight);
        static string Hash(string value){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-","").ToLowerInvariant();}
        LevelDefinition Directed(LevelDefinition source)=>Direction?.Apply(source)??source;
        public LevelDefinition Level(LevelDefinition source,int stage)
        {
            if(stage<0||stage>2||source.ContentHash!=SourceHash)throw new ArgumentException("Unavailable descent source.");
            var basis=Directed(source);var recipe=Stages[stage];int n=basis.TargetSymbolIds.Count;
            var open=Enumerable.Repeat(true,n-1).ToArray();var initial=basis.TargetSymbolIds.ToArray();
            if(stage==2&&Version>=3&&recipe.PublishedStart)initial=basis.InitialSymbolIds.ToArray();
            if(stage==0) {
                int first=Enumerable.Range(0,n-1).First(i=>initial[i]!=initial[i+1]);
                Swap(initial,first);int second=Enumerable.Range(0,n-1).FirstOrDefault(i=>i!=first&&initial[i]!=initial[i+1]);
                if(second!=first&&initial[second]!=initial[second+1])Swap(initial,second);
            }
            FloorDefinition LiveWind(FloorDefinition f) => Version>=5&&stage==1&&NodeNumber(source)>=2
                ?new FloorDefinition(f.BaseDurability,f.OpenEdges,f.FragileEdges,f.LinkedPair,f.EntryTransform,f.Bomb,f.Key,f.Repair,f.OneUseBridgeEdge,f.OptionalStamp,f.Powerup,
                    1,recipe.Arrival,NodeNumber(source)>=4?1:0):f;
            FloorDefinition[] floors;
            if(stage==0) {
                RepairDefinition repair=null;
                int lesson=Version>=4?EarlyLesson(source):0;
                if(Version>=2&&(Version<3||Version==3&&NodeNumber(source)>=20||Version>=4&&(NodeNumber(source)>40||lesson==4))) {
                    // Undoing the second warm-up exchange restores this displaced symbol first.
                    var restored=initial.ToArray();int first=Enumerable.Range(0,n-1).First(i=>basis.TargetSymbolIds[i]!=basis.TargetSymbolIds[i+1]);
                    var once=basis.TargetSymbolIds.ToArray();Swap(once,first);
                    int second=Enumerable.Range(0,n-1).FirstOrDefault(i=>i!=first&&once[i]!=once[i+1]);
                    Swap(restored,second);
                    int slot=Enumerable.Range(0,n).First(i=>initial[i]!=basis.TargetSymbolIds[i]&&restored[i]==basis.TargetSymbolIds[i]);
                    repair=new RepairDefinition(slot,basis.TargetSymbolIds[slot],1);
                }
                KeyDefinition key=null;int? bridge=null;LinkedPair? linked=null;int[] fragile=null;ContactDefinition stamp=null;
                int firstEdge=Enumerable.Range(0,n-1).First(i=>basis.TargetSymbolIds[i]!=basis.TargetSymbolIds[i+1]);
                var keyOnce=basis.TargetSymbolIds.ToArray();Swap(keyOnce,firstEdge);
                int secondEdge=Enumerable.Range(0,n-1).FirstOrDefault(i=>i!=firstEdge&&keyOnce[i]!=keyOnce[i+1]);
                var keyRestored=initial.ToArray();Swap(keyRestored,secondEdge);
                int contactSlot=Enumerable.Range(0,n).FirstOrDefault(i=>initial[i]!=basis.TargetSymbolIds[i]&&keyRestored[i]==basis.TargetSymbolIds[i]);
                if(lesson==3&&secondEdge!=firstEdge){open[firstEdge]=false;key=new KeyDefinition(contactSlot,basis.TargetSymbolIds[contactSlot],firstEdge);}
                if(lesson==7){bridge=secondEdge;}
                if(lesson==8)stamp=new ContactDefinition(contactSlot,basis.TargetSymbolIds[contactSlot]);
                if(lesson==5)fragile=new[]{secondEdge};
                if(lesson==6&&n>=4){
                    int other=Enumerable.Range(0,n-1).Where(i=>Math.Abs(i-firstEdge)>1&&basis.TargetSymbolIds[i]!=basis.TargetSymbolIds[i+1]).DefaultIfEmpty(-1).First();
                    if(other>=0){initial=basis.TargetSymbolIds.ToArray();Swap(initial,firstEdge);Swap(initial,other);linked=new LinkedPair(firstEdge,other);}
                }
                floors=new[]{new FloorDefinition(recipe.Wood[0],open,fragile,linked,key:key,repair:repair,oneUseBridgeEdge:bridge,optionalStamp:stamp)};
            }
            else if(stage==1) {
                int slot=recipe.Arrival==EntryTransform.ShiftRightOne?0:n-1;
                int node=NodeNumber(source);bool early=Version>=4&&node<=40&&n<=6;
                bool hourglass=early&&(node==7||node>=11&&node%3==2),ribbon=early&&(node==5||node>=11&&node%3==1);
                var bomb=Version>=2&&(Version<3||node>=41||early&&(node==3||node>=11&&node%3==0||hourglass))?new BombDefinition(slot,basis.TargetSymbolIds[slot],Math.Min(6,n+1),1):null;
                var stamp=Version>=2&&(Version<3||node>=33||early&&node>=6)?new ContactDefinition(n/2,basis.TargetSymbolIds[n/2]):null;
                PowerupDefinition powerup=null;
                if(hourglass||ribbon){
                    var arrived=Rotate(initial,recipe.Arrival);
                    int mark=Enumerable.Range(0,n).Where(i=>i!=slot&&arrived[i]!=basis.TargetSymbolIds[i]
                        &&(i>0&&arrived[i-1]==basis.TargetSymbolIds[i]||i+1<n&&arrived[i+1]==basis.TargetSymbolIds[i])).DefaultIfEmpty((slot+1)%n).First();
                    powerup=new PowerupDefinition(mark,basis.TargetSymbolIds[mark],hourglass?PowerupKind.Hourglass:PowerupKind.Ribbon);
                }
                var landing=Version<5&&early&&node>=2&&!Rotate(Rotate(initial,recipe.Arrival),recipe.Arrival).SequenceEqual(initial)?recipe.Arrival:EntryTransform.None;
                if(early&&(node==3||node==7)){
                    // Calibrate introductory contacts against the canonical useful route, including the landing shift.
                    ContactDefinition FirstExchange(FloorDefinition first,int avoid=-1) {
                        var preview=new LevelDefinition(basis.PuzzleId,basis.CampaignNodeId,basis.SchemaVersion,basis.RulesVersion,
                            StageHash(stage),initial,basis.TargetSymbolIds,basis.TargetSegments,new[]{LiveWind(first),new FloorDefinition(recipe.Wood[1],open,entryTransform:landing,optionalStamp:stamp)});
                        var resolver=new PuzzleResolver(preview);var state=resolver.CreateInitialState();
                        var proof=PuzzleSolver.FindShortest(resolver,stateBudget:10000);
                        foreach(var command in proof.Path){
                            if(state.ActiveFloor==0&&command.Kind==CommandKind.Swap&&state.Row[command.Edge]!=state.Row[command.Edge+1]){
                                int mark=command.Edge==avoid?command.Edge+1:command.Edge;
                                return new ContactDefinition(mark,Version>=5?resolver.Resolve(state,command).State.Row[mark]:state.Row[mark==command.Edge?mark+1:mark-1]);
                            }
                            state=resolver.Resolve(state,command).State;
                        }
                        throw new InvalidOperationException("An introductory contact needs a proved first-floor exchange.");
                    }
                    if(node==7){var mark=FirstExchange(new FloorDefinition(recipe.Wood[0],open,entryTransform:recipe.Arrival));powerup=new PowerupDefinition(mark.Slot,mark.RequiredSymbol,PowerupKind.Hourglass);}
                    var bombMark=FirstExchange(new FloorDefinition(recipe.Wood[0],open,entryTransform:recipe.Arrival,powerup:powerup),powerup?.Slot??-1);
                    bomb=new BombDefinition(bombMark.Slot,bombMark.RequiredSymbol,Math.Min(6,n+1),1);
                }
                floors=new[]{new FloorDefinition(recipe.Wood[0],open,entryTransform:recipe.Arrival,bomb:bomb,powerup:powerup),new FloorDefinition(recipe.Wood[1],open,entryTransform:landing,optionalStamp:stamp)};
            }
            else floors=basis.Floors.Select((f,i)=>new FloorDefinition(recipe.Wood[i],f.OpenEdges,f.FragileEdges,f.LinkedPair,
                i==0?recipe.Arrival:f.EntryTransform,f.Bomb,f.Key,f.Repair,f.OneUseBridgeEdge,f.OptionalStamp,f.Powerup)).ToArray();
            if(stage==1)floors[0]=LiveWind(floors[0]);
            return new LevelDefinition(basis.PuzzleId,basis.CampaignNodeId,basis.SchemaVersion,basis.RulesVersion,
                StageHash(stage),initial,basis.TargetSymbolIds,basis.TargetSegments,floors,stage==2?basis.ReachCharges:0);
        }
        static void Swap(SymbolId[] row,int edge){var a=row[edge];row[edge]=row[edge+1];row[edge+1]=a;}
        static int NodeNumber(LevelDefinition source)=>int.TryParse(source.CampaignNodeId.Substring(1),out int node)?node:0;
        static SymbolId[] Rotate(SymbolId[] row,EntryTransform transform)=>transform==EntryTransform.ShiftRightOne?row.Skip(row.Length-1).Concat(row.Take(row.Length-1)).ToArray():row.Skip(1).Concat(row.Take(1)).ToArray();
        static int EarlyLesson(LevelDefinition source)
        {
            int node=NodeNumber(source);if(node>40)return 4;
            if(node<=10)return new[]{0,0,4,0,3,0,8,0,7,6,5}[node];
            return new[]{4,8,7,4,3,5}[node%6];
        }
        public static bool CanDescend(LevelDefinition source)=>source!=null&&source.CampaignNodeId.StartsWith("C",StringComparison.Ordinal)
            &&NodeNumber(source)>=1&&NodeNumber(source)<=160;
        static bool Prove(DescentRun run,LevelDefinition source,int stage,CancellationToken cancellation,int budget)
        {
            var result=PuzzleSolver.FindShortest(new PuzzleResolver(run.Level(source,stage)),stateBudget:budget,cancellationToken:cancellation);
            if(result.Status!=SearchStatus.Solved)return false;
            run.Stages[stage].Minimum=result.OptimalActions.Value;run.Stages[stage].Witness=result.Path.Select(c=>c.ToString()).ToArray();return true;
        }
        public static DescentRun Build(LevelDefinition source,DeliveryMood mood=DeliveryMood.Steady,
            CancellationToken cancellation=default,int budget=PuzzleSolver.DefaultStateBudget,int version=2)
        {
            if((version<1||version>5)||!(version>=3?CanDescend(source):DeliveryDirector.CanDirect(source))||source.TargetSymbolIds.Distinct().Count()<2)return null;
            var direction=DeliveryDirector.Plan(source,mood);
            var run=new DescentRun {Version=version,SourceHash=source.ContentHash,Direction=direction,Stages=new DescentStage[3]};
            var basis=run.Directed(source);int n=basis.TargetSymbolIds.Count;
            bool early=version>=4&&NodeNumber(source)<=40;int lesson=early?EarlyLesson(source):0;
            run.Stages[0]=new DescentStage{Wood=new[]{early?(lesson==4?1:lesson==5?3:2):version==1||version==3&&NodeNumber(source)<20?2:1}};
            run.Stages[1]=new DescentStage{Arrival=int.Parse(source.CampaignNodeId.Substring(1))%2==0?EntryTransform.ShiftRightOne:EntryTransform.ShiftLeftOne,
                Wood=early&&n<=6?new[]{Math.Max(2,(n-1)/2),Math.Min(12,n+n/2)}:new[]{Math.Max(1,(n-1)/2),Math.Max(1,n-(n-1)/2)}};
            if(version>=5&&NodeNumber(source)>=2){run.Stages[1].Wood=new[]{NodeNumber(source)%3==0?2:3,Math.Min(12,n+n/2+1)};}
            if(!Prove(run,source,0,cancellation,budget)||!Prove(run,source,1,cancellation,budget)
                ||run.Stages[1].Minimum<=run.Stages[0].Minimum)return null;
            // The device finale retains the published topology and contacts. Try at most three arrivals.
            bool found=false;
            var arrivals=version>=4&&NodeNumber(source)<=40&&n>=7?new[]{EntryTransform.ShiftRightOne,EntryTransform.ShiftLeftOne,EntryTransform.MirrorNext}:new[]{EntryTransform.MirrorNext,EntryTransform.ShiftRightOne,EntryTransform.ShiftLeftOne};
            foreach(var arrival in arrivals) {
                if(cancellation.IsCancellationRequested)return null;
                int allowance=arrival==EntryTransform.MirrorNext?n*(n-1)/2+2:n+3;
                var wood=basis.Floors.Select(f=>Math.Min(12,Math.Max(f.BaseDurability,(allowance+basis.Floors.Count-1)/basis.Floors.Count))).ToArray();
                run.Stages[2]=new DescentStage{Arrival=arrival,Wood=wood};
                if(Prove(run,source,2,cancellation,budget)&&run.Stages[2].Minimum>=run.Stages[1].Minimum){found=true;break;}
            }
            if(!found&&version>=3){
                run.Stages[2]=new DescentStage{Arrival=EntryTransform.None,PublishedStart=true,Wood=basis.Floors.Select(f=>f.BaseDurability).ToArray()};
                if(Prove(run,source,2,cancellation,budget))return cancellation.IsCancellationRequested?null:run;
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
            if((Version<1||Version>5)||CompletedExplosions<0||source.ContentHash!=SourceHash||!(Version>=3?CanDescend(source):DeliveryDirector.CanDirect(source))||Stages==null||Stages.Length!=3
                ||Completed==null||Completed.Count>2||source.TargetSymbolIds.Distinct().Count()<2)throw new ArgumentException("Invalid descent checkpoint.");
            Directed(source);
            int explosions=0,flags=0;
            for(int i=0;i<3;i++) {
                var s=Stages[i];if(s==null||s.Wood==null||s.Wood.Length!=(i==0?1:i==1?2:source.Floors.Count)
                    ||s.Wood.Any(w=>w<1||w>12)||!Enum.IsDefined(typeof(EntryTransform),s.Arrival)
                    ||(i==0&&s.Arrival!=EntryTransform.None)||(i==1&&s.Arrival!=EntryTransform.ShiftLeftOne&&s.Arrival!=EntryTransform.ShiftRightOne)
                    ||s.PublishedStart&&(Version<3||i!=2||s.Arrival!=EntryTransform.None||!s.Wood.SequenceEqual(Directed(source).Floors.Select(f=>f.BaseDurability)))
                    ||s.Minimum<1||s.Minimum>1000||s.Witness==null||s.Witness.Length!=s.Minimum)
                    throw new ArgumentException("Invalid descent stage recipe.");
                Replay(Level(source,i),locale,s.Witness);
                if(i<Completed.Count) {
                    var done=AttemptService.Restore(Level(source,i),Completed[i]);explosions+=done.Current.ExplodedCount;flags|=JourneyRewards.ToolFlags(done);
                    if(done.Current.Status!=PuzzleStatus.Won||done.Goal.RequiredStampBits!=0||!done.Current.Row.SequenceEqual(source.TargetSymbolIds))throw new ArgumentException("Unfinished descent checkpoint.");
                }
            }
            if(explosions!=CompletedExplosions)throw new ArgumentException("Invalid descent outcome totals.");
            if(CompletedToolFlags.HasValue&&CompletedToolFlags.Value!=flags)throw new ArgumentException("Invalid descent tool totals.");
            if(!CompletedToolFlags.HasValue)CompletedToolFlags=flags; // Reconstruct optional evidence from old canonical replays.
            if(Stages[0].Minimum>=Stages[1].Minimum||!Stages[2].PublishedStart&&Stages[1].Minimum>Stages[2].Minimum)throw new ArgumentException("Invalid descent curve.");
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
            CompletedExplosions+=attempt.Current.ExplodedCount;
            if(!CompletedToolFlags.HasValue)CompletedToolFlags=Completed.Select((r,i)=>JourneyRewards.ToolFlags(AttemptService.Restore(Level(source,i),r))).Aggregate(0,(a,b)=>a|b);
            CompletedToolFlags|=JourneyRewards.ToolFlags(attempt);
            Completed.Add(AttemptRecord.Capture(attempt));
        }
        public DeliveryPerformance Performance(AttemptService attempt)=>new DeliveryPerformance {
            ToolFlags=(CompletedToolFlags??0)|JourneyRewards.ToolFlags(attempt),
            MovesMade=Completed.Sum(a=>a.MovesMade??a.Commands.Length+a.UndoCount)+attempt.MovesMade,
            ActiveSeconds=attempt.ActiveSeconds.HasValue&&Completed.All(a=>a.ActiveSeconds.HasValue)?Completed.Sum(a=>a.ActiveSeconds.Value)+attempt.ActiveSeconds.Value:(double?)null,
            Route=Completed.Sum(a=>a.Commands.Length)+attempt.RouteLength,Undos=Completed.Sum(a=>a.UndoCount)+attempt.UndoCount,
            HintUsed=Completed.Any(a=>a.HintUsed)||attempt.HintUsed,Minimum=Stages.Sum(s=>s.Minimum),
            Exploded=CompletedExplosions+attempt.Current.ExplodedCount,ContentHash=ContentHash,SourceHash=SourceHash
        };
    }
}
