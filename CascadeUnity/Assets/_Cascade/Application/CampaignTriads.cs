using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cascade.Domain;
using Cascade.Solver;

namespace Cascade.Application
{
    // Public fields are pinned by the existing profile serializer, including contact symbol indices.
    public sealed class TriadBoard
    {
        public int Hp;
        public bool[] Open;
        public int[] Fragile=Array.Empty<int>();
        public int LinkA=-1,LinkB=-1,Entry,Bridge=-1;
        public int BombSlot=-1,BombSymbol,BombTicks=2,BombDamage=1;
        public int KeySlot=-1,KeySymbol,KeyEdge=-1;
        public int RepairSlot=-1,RepairSymbol,RepairAmount;
        public int StampSlot=-1,StampSymbol;
        public int PowerSlot=-1,PowerSymbol,PowerKind;
        public int GustAfter,GustEntry,GustDamage;
        internal string Fingerprint=>string.Join(",",Hp,LinkA,LinkB,Entry,Bridge,BombSlot,BombSymbol,BombTicks,BombDamage,
            KeySlot,KeySymbol,KeyEdge,RepairSlot,RepairSymbol,RepairAmount,StampSlot,StampSymbol,PowerSlot,PowerSymbol,PowerKind,
            GustAfter,GustEntry,GustDamage)+":"+string.Join("",Open.Select(x=>x?"1":"0"))+":"+string.Join(",",Fragile);
        internal FloorDefinition Bind(IReadOnlyList<SymbolId> symbols)
        {
            if(Open==null||Fragile==null||new[]{BombSymbol,KeySymbol,RepairSymbol,StampSymbol,PowerSymbol}.Any(i=>i<0||i>=symbols.Count)
                ||new[]{BombSlot,KeySlot,RepairSlot,StampSlot,PowerSlot,LinkA,LinkB,Bridge,KeyEdge}.Any(i=>i< -1))
                throw new ArgumentException("Invalid triad board bindings.");
            return new FloorDefinition(Hp,Open,Fragile,
            LinkA<0?(LinkedPair?)null:new LinkedPair(LinkA,LinkB),(EntryTransform)Entry,
            BombSlot<0?null:new BombDefinition(BombSlot,symbols[BombSymbol],BombTicks,BombDamage),
            KeySlot<0?null:new KeyDefinition(KeySlot,symbols[KeySymbol],KeyEdge),
            RepairSlot<0?null:new RepairDefinition(RepairSlot,symbols[RepairSymbol],RepairAmount),Bridge<0?(int?)null:Bridge,
            StampSlot<0?null:new ContactDefinition(StampSlot,symbols[StampSymbol]),
            PowerSlot<0?null:new PowerupDefinition(PowerSlot,symbols[PowerSymbol],(PowerupKind)PowerKind),
            GustAfter,(EntryTransform)GustEntry,GustDamage);
        }
    }
    public sealed class TriadRecipe
    {
        public int Lesson;
        public int[] Start;
        public TriadBoard[] Boards;
        internal string Fingerprint=>Lesson+":"+string.Join(",",Start)+":"+string.Join("/",Boards.Select(b=>b.Fingerprint));
        internal LevelDefinition Bind(LevelDefinition source,string hash)
        {
            if(Start==null||Boards==null||Start.Any(i=>i<0||i>=source.TargetSymbolIds.Count)||Boards.Any(b=>b==null))
                throw new ArgumentException("Invalid triad row bindings.");
            return new LevelDefinition(source.PuzzleId,source.CampaignNodeId,
            source.SchemaVersion,source.RulesVersion,hash,Start.Select(i=>source.TargetSymbolIds[i]),source.TargetSymbolIds,
            source.TargetSegments,Boards.Select(b=>b.Bind(source.TargetSymbolIds)),teachingNode:source.TeachingNode);
        }
    }
    public static class CampaignTriads
    {
        static readonly MechanicLesson[] early={MechanicLesson.Swap,MechanicLesson.Repair,MechanicLesson.Bomb,MechanicLesson.Key,
            MechanicLesson.Stamp,MechanicLesson.Ribbon,MechanicLesson.BombHourglass,MechanicLesson.Bridge,
            MechanicLesson.LinkedPairs,MechanicLesson.Fragile,MechanicLesson.Mirror,MechanicLesson.Rollers,
            MechanicLesson.LiveGust,MechanicLesson.ClosedPath};
        static readonly MechanicLesson[] opening={MechanicLesson.Swap,MechanicLesson.Repair,MechanicLesson.Bomb,MechanicLesson.Key,
            MechanicLesson.Ribbon,MechanicLesson.Bridge,MechanicLesson.Mirror,MechanicLesson.Fragile,
            MechanicLesson.LinkedPairs,MechanicLesson.ClosedPath,MechanicLesson.Rollers,MechanicLesson.BombHourglass,
            MechanicLesson.LiveGust,MechanicLesson.Stamp};
        static readonly string[] openingNodes={"C01","C02","C03","C185","C186","C187","C188","C189","C190","C191","C192","C193","C04","C05"};
        public static MechanicLesson Primary(LevelDefinition source)
        {
            int position=Array.IndexOf(openingNodes,source.CampaignNodeId)+1;
            if(position>=1&&position<=opening.Length)return opening[position-1];
            int lesson=source.CampaignLessonNumber;
            if(lesson>=1&&lesson<=early.Length)return early[lesson-1];
            var vocabulary=Vocabulary(source);
            return vocabulary.Count==0?MechanicLesson.Swap:vocabulary[(lesson+source.TargetSymbolIds.Count)%vocabulary.Count];
        }
        static List<MechanicLesson> Vocabulary(LevelDefinition source)
        {
            var kinds=new List<MechanicLesson>();
            foreach(var f in source.Floors){
                if(f.Repair!=null)kinds.Add(MechanicLesson.Repair);
                if(f.Key!=null)kinds.Add(MechanicLesson.Key);
                if(f.Bomb!=null)kinds.Add(f.Powerup?.Kind==PowerupKind.Hourglass?MechanicLesson.BombHourglass:MechanicLesson.Bomb);
                if(f.Powerup?.Kind==PowerupKind.Ribbon&&source.TargetSymbolIds.Distinct().Count()>=3)kinds.Add(MechanicLesson.Ribbon);
                if(f.OptionalStamp!=null)kinds.Add(MechanicLesson.Stamp);
                if(f.OneUseBridgeEdge.HasValue)kinds.Add(MechanicLesson.Bridge);
                if(f.LinkedPair.HasValue)kinds.Add(MechanicLesson.LinkedPairs);
                if(f.FragileEdges.Count>0)kinds.Add(MechanicLesson.Fragile);
                if(f.OpenEdges.Any(x=>!x))kinds.Add(MechanicLesson.ClosedPath);
                if(f.EntryTransform!=EntryTransform.None)kinds.Add(f.EntryTransform==EntryTransform.MirrorNext?MechanicLesson.Mirror:MechanicLesson.Rollers);
                if(f.GustAfterSwaps>0)kinds.Add(MechanicLesson.LiveGust);
            }
            return kinds.Distinct().ToList();
        }
        public static DescentRun Build(LevelDefinition source,CancellationToken cancellation,int budget)
            =>BuildVariant(source,cancellation,budget,0,0,null);
        public static DescentRun BuildAlternative(LevelDefinition source,int variation,CancellationToken cancellation,int budget)
            =>BuildVariant(source,cancellation,budget,unchecked(variation*104729),0,null);
        public static IReadOnlyList<MechanicLesson> AuthoredPrinciples(LevelDefinition source)=>Vocabulary(source).AsReadOnly();
        // Offline authoring only: the publisher bounds whole-triad difficulty;
        // each stage still receives effect, bomb and optimal-budget proofs.
        public static DescentRun BuildCampaignRevision(LevelDefinition source,MechanicLesson lesson,int variation,DescentRun baseline,CancellationToken cancellation,int budget=60000)
        {
            if(baseline?.Version!=6||baseline.SourceHash!=source.ContentHash||!Vocabulary(source).Contains(lesson))
                throw new ArgumentException("Revision must retain the authored vocabulary and baseline.");
            return BuildVariant(source,cancellation,budget,unchecked(variation*104729),1,null,lesson);
        }
        public static DescentRun BuildPersonalMission(LevelDefinition source,int serial,string previous,CancellationToken cancellation,int budget=60000,DescentRun reference=null,int challenge=1)
        {
            if(challenge<1||challenge>2)throw new ArgumentOutOfRangeException(nameof(challenge));
            var baseline=reference?.Version==6&&reference.SourceHash==source.ContentHash?reference:Build(source,cancellation,budget);
            if(baseline==null)return null;
            int minimum=baseline.Stages.Sum(s=>s.Minimum),allowance=Math.Max(4,minimum/3);
            for(int variation=0;variation<16;variation++){
                cancellation.ThrowIfCancellationRequested();
                DescentRun run;
                try{run=BuildVariant(source,cancellation,budget,unchecked(serial*104729+variation*15485863),1+variation%2,baseline,(MechanicLesson)baseline.Stages[0].Triad.Lesson);}
                catch(InvalidOperationException){continue;}
                int extra=run.Stages.Sum(s=>s.Minimum)-minimum;
                if(extra>=challenge&&extra<=allowance&&run.ContentHash!=previous)return run;
            }
            return null;
        }
        static DescentRun BuildVariant(LevelDefinition source,CancellationToken cancellation,int budget,int variation,int boost,DescentRun baseline,MechanicLesson? authoredLesson=null)
        {
            if(!DescentRun.CanDescend(source))return null;
            var run=new DescentRun{Version=6,SourceHash=source.ContentHash,Stages=new DescentStage[3]};
            var family=authoredLesson??Primary(source);int node=int.Parse(source.CampaignNodeId.Substring(1));
            for(int stage=0;stage<3;stage++){
                bool found=false;
                // Bounded deterministic candidates; work occurs off the Unity thread.
                for(int trial=0;trial<48&&!found;trial++){
                    cancellation.ThrowIfCancellationRequested();
                    var recipe=Construct(source,family,stage,unchecked(node*7919+stage*137+trial*43+variation),stage==0?boost:0);
                    if(recipe==null)continue;
                    var candidate=new DescentStage{Triad=recipe,Wood=recipe.Boards.Select(b=>b.Hp).ToArray(),Minimum=1,Witness=new[]{"0"}};
                    run.Stages[stage]=candidate;
                    var level=recipe.Bind(source,"triad-candidate");
                    if(LevelValidator.Validate(level).Count!=0)continue;
                    var proof=PuzzleSolver.FindShortest(new PuzzleResolver(level),stateBudget:Math.Min(budget,60000),cancellationToken:cancellation);
                    if(proof.Status!=SearchStatus.Solved||proof.Path.Count<1)continue;
                    if(PruneIdleBoards(recipe,level,proof.Path)){
                        candidate.Wood=recipe.Boards.Select(b=>b.Hp).ToArray();level=recipe.Bind(source,"trimmed-triad");
                        if(LevelValidator.Validate(level).Count!=0)continue;
                        proof=PuzzleSolver.FindShortest(new PuzzleResolver(level),stateBudget:Math.Min(budget,60000),cancellationToken:cancellation);
                    }
                    // Constructive path length is only an upper bound. Recalculate wood
                    // against every lower durability, including altered fall/gust timing.
                    proof=CalibrateDurability(recipe,source,family,proof,cancellation,Math.Min(budget,60000));
                    level=recipe.Bind(source,"calibrated-triad");candidate.Wood=recipe.Boards.Select(b=>b.Hp).ToArray();
                    if(proof.Status!=SearchStatus.Solved||proof.Path.Count<1||!UsesPrinciple(level,proof.Path,family))continue;
                    if(baseline!=null&&(proof.Path.Count<baseline.Stages[stage].Minimum+(stage==0?1:0)
                        ||proof.Path.Count>baseline.Stages[stage].Minimum+Math.Max(3,baseline.Stages[stage].Minimum/3)))continue;
                    if(!BenefitsRequired(level,cancellation,Math.Min(budget,60000)))continue;
                    if(!BombsRequired(level,cancellation,Math.Min(budget,60000)))continue;
                    candidate.Minimum=proof.OptimalActions.Value;candidate.Witness=proof.Path.Select(c=>c.ToString()).ToArray();
                    if(stage>0&&run.Stages.Take(stage).Any(s=>s.Triad.Start.SequenceEqual(recipe.Start)
                        &&s.Triad.Fingerprint==recipe.Fingerprint))continue;
                    found=true;
                }
                if(!found)throw new InvalidOperationException("Unproved triad "+source.CampaignNodeId+" stage "+(stage+1)+" / "+family);
            }
            return run;
        }
        static SearchResult CalibrateDurability(TriadRecipe recipe,LevelDefinition source,MechanicLesson family,SearchResult proof,CancellationToken cancellation,int budget)
        {
            if(proof.Status!=SearchStatus.Solved)return proof;
            bool changed;
            do{
                changed=false;
                foreach(var board in recipe.Boards){
                    int original=board.Hp;bool trimmed=false;
                    for(int hp=1;hp<original;hp++){
                        cancellation.ThrowIfCancellationRequested();board.Hp=hp;
                        var reduced=recipe.Bind(source,"durability-probe");
                        var test=PuzzleSolver.FindShortest(new PuzzleResolver(reduced),stateBudget:budget,cancellationToken:cancellation);
                        if(test.Status==SearchStatus.Unknown){board.Hp=original;return test;}
                        if(test.Status!=SearchStatus.Solved||!UsesPrinciple(reduced,test.Path,family))continue;
                        proof=test;changed=true;trimmed=true;break;
                    }
                    if(!trimmed)board.Hp=original;
                }
            }while(changed);
            return proof;
        }
        static bool PruneIdleBoards(TriadRecipe recipe,LevelDefinition level,IEnumerable<PuzzleCommand> commands)
        {
            var resolver=new PuzzleResolver(level);var current=resolver.CreateInitialState();var used=new HashSet<int>{0};
            foreach(var command in commands){
                if(command.Kind!=CommandKind.EarlyDrop)used.Add(current.ActiveFloor);
                var transition=resolver.Resolve(current,command);
                foreach(var e in transition.Events)if(e.Kind==PuzzleEventKind.EntryPermuted)used.Add(e.Floor);
                current=transition.State;
            }
            if(used.Count==recipe.Boards.Length)return false;
            recipe.Boards=recipe.Boards.Where((b,index)=>used.Contains(index)).ToArray();return true;
        }
        static TriadRecipe Construct(LevelDefinition source,MechanicLesson family,int stage,int seed,int boost=0)
        {
            int n=source.TargetSymbolIds.Count,lesson=source.CampaignLessonNumber;
            if(family==MechanicLesson.LinkedPairs&&n<4)return null;
            var random=new Random(seed);
            int boards=lesson<=10?(stage==2?2:1):Math.Min(4,Math.Max(source.Floors.Count,1+stage+(lesson>=80?1:0)));
            if(family==MechanicLesson.Ribbon)boards=2;
            bool entry=family==MechanicLesson.Mirror||family==MechanicLesson.Rollers;
            if(entry)boards=Math.Max(2,boards);
            if(family==MechanicLesson.LiveGust)boards=Math.Max(3,boards);
            if(family==MechanicLesson.ClosedPath)boards=2;
            if(boost>0&&family!=MechanicLesson.Ribbon&&family!=MechanicLesson.ClosedPath)boards=Math.Min(4,boards+boost);
            if(boost>0&&family==MechanicLesson.ClosedPath&&n<4)boards=Math.Min(4,boards+boost);
            var result=new TriadRecipe{Lesson=(int)family,Boards=new TriadBoard[boards]};
            var end=Enumerable.Range(0,n).ToArray();var allCommands=new List<PuzzleCommand[]>();
            var vocabulary=Vocabulary(source);
            for(int floor=boards-1;floor>=0;floor--){
                int primaryFloor=family==MechanicLesson.LiveGust?1:0;
                var kind=floor==primaryFloor?family:MechanicLesson.Swap;
                // Later levels combine authored, already introduced contacts on different boards.
                if(lesson>14&&floor>0&&floor!=primaryFloor&&vocabulary.Count>1&&family!=MechanicLesson.Ribbon){
                    var secondary=vocabulary[(floor+stage)%vocabulary.Count];
                    if(secondary!=MechanicLesson.Mirror&&secondary!=MechanicLesson.Rollers&&secondary!=MechanicLesson.LiveGust&&secondary!=MechanicLesson.ClosedPath)kind=secondary;
                }
                bool ribbonExit=family==MechanicLesson.Ribbon&&floor==boards-1;
                var board=new TriadBoard{Open=new bool[n-1]};
                int length=lesson==1?(stage==1?3:2):Math.Min(6,2+stage+Math.Min(2,lesson/100));
                if(kind==MechanicLesson.LinkedPairs)length=stage+1+Math.Min(2,lesson/100);
                if(kind==MechanicLesson.BombHourglass)length=3;
                if(floor>0)length=Math.Min(5,1+stage+Math.Min(2,lesson/80));
                if(kind==MechanicLesson.BombHourglass)length=3;
                if(kind==MechanicLesson.Ribbon)length=family==MechanicLesson.Ribbon?1+stage+Math.Min(2,lesson/100):2;
                if(ribbonExit)length=1;
                if(kind==MechanicLesson.Repair||kind==MechanicLesson.Key)length=Math.Max(kind==MechanicLesson.Repair&&floor==0?3:2,length);
                if(kind==MechanicLesson.Bomb)length=Math.Max(2,length);
                if(entry&&floor==0)length=1+stage;
                if(kind==MechanicLesson.LiveGust)length=Math.Max(3,length);
                // The hourglass certificate needs its authored three contacts; an extra
                // useful board raises difficulty without making that timing impossible.
                if(floor==0&&kind!=MechanicLesson.BombHourglass)length+=boost;
                int width=Math.Min(n-1,lesson<15?n-1:4),offset=random.Next(n-width);
                if(floor>0&&kind==MechanicLesson.Swap&&family!=MechanicLesson.ClosedPath&&!ribbonExit){width=1;offset=random.Next(n-1);length=1;}
                if(family==MechanicLesson.ClosedPath){width=kind==MechanicLesson.ClosedPath?Math.Max(1,Math.Min(n-2,Math.Min(2,(n-1)/2)+boost)):Math.Min(2,n-1);offset=random.Next(n-width);}
                if(kind==MechanicLesson.LinkedPairs){board.LinkA=0;board.LinkB=n-2;width=n-1;offset=0;}
                if(entry&&floor==1)board.Entry=(int)(family==MechanicLesson.Mirror?EntryTransform.MirrorNext:stage%2==0?EntryTransform.ShiftLeftOne:EntryTransform.ShiftRightOne);
                if(!ribbonExit&&kind!=MechanicLesson.LinkedPairs){
                    int factorial=1;for(int k=2;k<=width+1;k++)factorial*=k;
                    foreach(var group in end.Skip(offset).Take(width+1).GroupBy(i=>source.TargetSymbolIds[i]))
                        for(int k=2;k<=group.Count();k++)factorial/=k;
                    length=Math.Min(length,factorial-1);if(length<1||(kind==MechanicLesson.Repair||kind==MechanicLesson.Bomb)&&length<2||kind==MechanicLesson.BombHourglass&&length<3)return null;
                }
                var commands=new PuzzleCommand[length];var begin=end.ToArray();var states=new HashSet<string>{RowKey(begin,source)};
                bool valid=true;
                // Reverse a sequence of actual useful permutations, with no return to an earlier row.
                for(int action=length-1;action>=0;action--){
                    if(kind==MechanicLesson.LiveGust&&action==1){
                        board.GustAfter=2;board.GustEntry=(int)(stage%2==0?EntryTransform.ShiftLeftOne:EntryTransform.ShiftRightOne);board.GustDamage=1;
                        begin=Transform(begin,(EntryTransform)board.GustEntry,true);
                    }
                    var options=new List<PuzzleCommand>();
                    if(ribbonExit||kind==MechanicLesson.Ribbon&&family!=MechanicLesson.Ribbon&&action==length-1){
                        for(int i=0;i<n;i++)for(int j=i+2;j<n&&j<=i+3;j++)options.Add(PuzzleCommand.Reach(i,j));
                    }else for(int e=offset;e<offset+width;e++)options.Add(PuzzleCommand.Swap(e));
                    options=options.OrderBy(_=>random.Next()).ToList();bool selected=false;
                    foreach(var command in options){
                        var previous=Apply(begin,command,board);
                        if(previous.Select(i=>source.TargetSymbolIds[i]).SequenceEqual(begin.Select(i=>source.TargetSymbolIds[i])))continue;
                        if(!states.Add(RowKey(previous,source)))continue;
                        commands[action]=command;begin=previous;selected=true;break;
                    }
                    if(!selected){valid=false;break;}
                }
                if(!valid)return null;
                var row=begin.ToArray();var after=new List<int[]>();
                foreach(var command in commands){
                    row=Apply(row,command,board);
                    if(board.GustAfter>0&&after.Count+1==board.GustAfter)row=Transform(row,(EntryTransform)board.GustEntry,false);
                    after.Add(row.ToArray());
                    if(command.Kind==CommandKind.LongReach)for(int edge=command.Edge;edge<command.End;edge++)board.Open[edge]=true;
                    if(command.Kind==CommandKind.Swap){board.Open[command.Edge]=true;if(board.LinkA>=0&&(command.Edge==board.LinkA||command.Edge==board.LinkB)){board.Open[board.LinkA]=board.Open[board.LinkB]=true;}}
                }
                if(!row.Select(i=>source.TargetSymbolIds[i]).SequenceEqual(end.Select(i=>source.TargetSymbolIds[i])))return null;
                int contact=Enumerable.Range(0,n).Where(i=>source.TargetSymbolIds[begin[i]]!=source.TargetSymbolIds[after[0][i]]).OrderBy(_=>random.Next()).DefaultIfEmpty(-1).First();
                if(contact<0)return null;
                board.Hp=length+board.GustDamage;
                if(kind==MechanicLesson.Repair){board.RepairSlot=contact;board.RepairSymbol=after[0][contact];board.RepairAmount=Math.Min(3,length-1);board.Hp-=board.RepairAmount;}
                if(kind==MechanicLesson.Bomb){board.BombSlot=contact;board.BombSymbol=after[0][contact];board.BombTicks=Math.Min(2,board.Hp-1);board.BombDamage=Math.Min(4,Math.Max(1,length-1));}
                if(kind==MechanicLesson.Key){
                    int edge=commands.Skip(1).Where(c=>c.Kind==CommandKind.Swap&&c.Edge!=commands[0].Edge).Select(c=>c.Edge).DefaultIfEmpty(-1).First();
                    if(edge<0)return null;board.Open[edge]=false;board.KeySlot=contact;board.KeySymbol=after[0][contact];board.KeyEdge=edge;
                }
                if(kind==MechanicLesson.Ribbon){board.PowerSlot=contact;board.PowerSymbol=after[0][contact];board.PowerKind=(int)PowerupKind.Ribbon;}
                if(kind==MechanicLesson.BombHourglass){
                    int slot=Enumerable.Range(0,n).Where(i=>i!=contact&&after.Take(length-1).All(a=>source.TargetSymbolIds[a[i]]!=source.TargetSymbolIds[end[i]])).DefaultIfEmpty(-1).First();
                    if(slot<0)return null;board.PowerSlot=contact;board.PowerSymbol=after[0][contact];board.PowerKind=(int)PowerupKind.Hourglass;
                    board.BombSlot=slot;board.BombSymbol=end[slot];board.BombTicks=1;
                    // With later boards, one lost exchange could still be recovered below.
                    // The missing freeze must consume both exchanges it would have saved.
                    board.BombDamage=boards>1?2:1;
                }
                if(kind==MechanicLesson.Bridge){
                    int edge=commands.Where(c=>c.Kind==CommandKind.Swap).GroupBy(c=>c.Edge).Where(g=>g.Count()==1).Select(g=>g.Key).DefaultIfEmpty(-1).First();
                    if(edge<0)return null;board.Bridge=edge;
                }
                if(kind==MechanicLesson.Fragile){board.Fragile=new[]{commands[0].Edge};board.Hp+=commands.Count(c=>c.Kind==CommandKind.Swap&&c.Edge==commands[0].Edge);}
                if(kind==MechanicLesson.Stamp){board.StampSlot=contact;board.StampSymbol=after[0][contact];}
                // An entry device acts on landing, never on the starting platform.
                if(board.Entry!=0)begin=Transform(begin,(EntryTransform)board.Entry,true);
                if(family==MechanicLesson.ClosedPath&&floor<boards-1){board.Hp+=1+stage;commands=commands.Concat(new[]{PuzzleCommand.Drop}).ToArray();}
                result.Boards[floor]=board;end=begin;allCommands.Insert(0,commands);
            }
            result.Start=end;
            // A constructive certificate is checked before asking the shortest-path solver.
            var level=result.Bind(source,"constructive-triad");if(LevelValidator.Validate(level).Count!=0)return null;
            var resolver=new PuzzleResolver(level);var current=resolver.CreateInitialState();
            foreach(var command in allCommands.SelectMany(x=>x)){
                var transition=resolver.Resolve(current,command);if(!transition.Accepted)return null;current=transition.State;
            }
            return current.Status==PuzzleStatus.Won?result:null;
        }
        static string RowKey(int[] row,LevelDefinition source)=>string.Join("|",row.Select(i=>source.TargetSymbolIds[i].ToString()));
        static int[] Apply(int[] row,PuzzleCommand command,TriadBoard board)
        {
            var next=row.ToArray();void Swap(int a,int b){int value=next[a];next[a]=next[b];next[b]=value;}
            if(command.Kind==CommandKind.LongReach)Swap(command.Edge,command.End);
            else if(board.LinkA>=0&&(command.Edge==board.LinkA||command.Edge==board.LinkB)){Swap(board.LinkA,board.LinkA+1);Swap(board.LinkB,board.LinkB+1);}
            else Swap(command.Edge,command.Edge+1);
            return next;
        }
        static int[] Transform(int[] row,EntryTransform transform,bool inverse)
        {
            if(transform==EntryTransform.MirrorNext)return row.Reverse().ToArray();
            bool right=transform==EntryTransform.ShiftRightOne;if(inverse)right=!right;
            return right?row.Skip(row.Length-1).Concat(row.Take(row.Length-1)).ToArray():row.Skip(1).Concat(row.Take(1)).ToArray();
        }
        public static SearchResult WithoutBenefit(LevelDefinition level,int floor,PuzzleEventKind benefit,
            CancellationToken cancellation=default,int budget=60000)
        {
            // Keep the same row, receiver, wood, hazards and other devices. Only this
            // benefit is absent, so a solved result exposes an actual budget bypass.
            var floors=level.Floors.Select((f,i)=>i!=floor?f:new FloorDefinition(f.BaseDurability,f.OpenEdges,
                f.FragileEdges,f.LinkedPair,f.EntryTransform,f.Bomb,
                benefit==PuzzleEventKind.KeyOpened?null:f.Key,
                benefit==PuzzleEventKind.Repaired?null:f.Repair,f.OneUseBridgeEdge,f.OptionalStamp,
                benefit==PuzzleEventKind.HourglassCollected||benefit==PuzzleEventKind.RibbonCollected?null:f.Powerup,
                f.GustAfterSwaps,f.GustTransform,f.GustDamage));
            var counterfactual=new LevelDefinition(level.PuzzleId,level.CampaignNodeId,level.SchemaVersion,level.RulesVersion,
                "without-benefit",level.InitialSymbolIds,level.TargetSymbolIds,level.TargetSegments,floors,level.ReachCharges,level.TeachingNode);
            return PuzzleSolver.FindShortest(new PuzzleResolver(counterfactual),stateBudget:budget,cancellationToken:cancellation);
        }
        public static bool BenefitsRequired(LevelDefinition level,CancellationToken cancellation=default,int budget=60000)
        {
            for(int i=0;i<level.Floors.Count;i++){
                var floor=level.Floors[i];
                if(floor.Repair!=null&&WithoutBenefit(level,i,PuzzleEventKind.Repaired,cancellation,budget).Status!=SearchStatus.Unsolvable)return false;
                if(floor.Key!=null&&WithoutBenefit(level,i,PuzzleEventKind.KeyOpened,cancellation,budget).Status!=SearchStatus.Unsolvable)return false;
                if(floor.Powerup!=null&&WithoutBenefit(level,i,floor.Powerup.Kind==PowerupKind.Hourglass?
                    PuzzleEventKind.HourglassCollected:PuzzleEventKind.RibbonCollected,cancellation,budget).Status!=SearchStatus.Unsolvable)return false;
            }
            return true;
        }
        public static bool HasLiveBombWindows(LevelDefinition level)
            =>level.Floors.All(f=>f.Bomb==null||f.Bomb.Ticks<f.BaseDurability)
                &&new PuzzleResolver(level).CreateInitialState().DefusedCount==0;
        public static SearchResult WithMissedBomb(LevelDefinition level,CancellationToken cancellation=default,int budget=60000)
            =>PuzzleSolver.FindShortestOutcome(new PuzzleResolver(level),
                s=>s.Status==PuzzleStatus.Won&&(s.ExplodedCount>0||s.AbandonedCount>0),
                stateBudget:budget,cancellationToken:cancellation);
        public static bool BombsRequired(LevelDefinition level,CancellationToken cancellation=default,int budget=60000)
            =>!level.Floors.Any(f=>f.Bomb!=null)||HasLiveBombWindows(level)
                &&WithMissedBomb(level,cancellation,budget).Status==SearchStatus.Unsolvable;
        public static bool UsesPrinciple(LevelDefinition level,IEnumerable<PuzzleCommand> commands,MechanicLesson family)
        {
            if(!HasLiveBombWindows(level))return false;
            var resolver=new PuzzleResolver(level);var current=resolver.CreateInitialState();var events=new HashSet<PuzzleEventKind>();
            bool reach=false,linked=false,fragile=false,drop=false;var worked=new HashSet<int>();
            foreach(var command in commands){
                var floor=level.Floors[current.ActiveFloor];
                if(command.Kind!=CommandKind.EarlyDrop||floor.EntryTransform!=EntryTransform.None)worked.Add(current.ActiveFloor);
                reach|=command.Kind==CommandKind.LongReach;drop|=current.ActiveFloor==0&&command.Kind==CommandKind.EarlyDrop;
                linked|=current.ActiveFloor==0&&command.Kind==CommandKind.Swap&&floor.LinkedPair.HasValue&&floor.LinkedPair.Value.Contains(command.Edge);
                fragile|=current.ActiveFloor==0&&command.Kind==CommandKind.Swap&&floor.FragileEdges.Contains(command.Edge);
                var transition=resolver.Resolve(current,command);if(!transition.Accepted)return false;
                events.UnionWith(transition.Events.Where(e=>e.Floor==0||e.Kind==PuzzleEventKind.EntryPermuted||e.Kind==PuzzleEventKind.WindPermuted).Select(e=>e.Kind));current=transition.State;
            }
            if(current.Status!=PuzzleStatus.Won||current.ExplodedCount>0||worked.Count!=level.Floors.Count)return false;
            switch(family){
                case MechanicLesson.Repair:return events.Contains(PuzzleEventKind.Repaired);
                case MechanicLesson.Key:return events.Contains(PuzzleEventKind.KeyOpened);
                case MechanicLesson.Bomb:return events.Contains(PuzzleEventKind.BombDefused);
                case MechanicLesson.Ribbon:return events.Contains(PuzzleEventKind.RibbonCollected)&&reach;
                case MechanicLesson.BombHourglass:return events.Contains(PuzzleEventKind.HourglassCollected)&&events.Contains(PuzzleEventKind.BombDefused)&&events.Contains(PuzzleEventKind.BombFrozen);
                case MechanicLesson.Stamp:return events.Contains(PuzzleEventKind.StampCollected);
                case MechanicLesson.Bridge:return events.Contains(PuzzleEventKind.BridgeConsumed);
                case MechanicLesson.LinkedPairs:return linked;
                case MechanicLesson.Fragile:return fragile;
                case MechanicLesson.Mirror:case MechanicLesson.Rollers:return events.Contains(PuzzleEventKind.EntryPermuted);
                case MechanicLesson.LiveGust:return events.Contains(PuzzleEventKind.WindPermuted);
                case MechanicLesson.ClosedPath:return drop;
                default:return true;
            }
        }
    }
}
