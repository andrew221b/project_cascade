using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Threading;
using Cascade.Application;using Cascade.Content;using Cascade.Domain;using Cascade.Persistence;using Cascade.Solver;
class PublishedCampaignAudit {
 static bool WinsWithoutTransform(LevelDefinition l,int floor,bool gust,IEnumerable<PuzzleCommand> commands){
 var changed=new LevelDefinition(l.PuzzleId,l.CampaignNodeId,l.SchemaVersion,l.RulesVersion,"audit-without-transform",l.InitialSymbolIds,l.TargetSymbolIds,l.TargetSegments,
 l.Floors.Select((f,i)=>i!=floor?f:new FloorDefinition(f.BaseDurability,f.OpenEdges,f.FragileEdges,f.LinkedPair,gust?f.EntryTransform:EntryTransform.None,f.Bomb,f.Key,f.Repair,f.OneUseBridgeEdge,f.OptionalStamp,f.Powerup,gust?0:f.GustAfterSwaps,gust?EntryTransform.None:f.GustTransform,gust?0:f.GustDamage)),l.ReachCharges,l.TeachingNode);
 var resolver=new PuzzleResolver(changed);var state=resolver.CreateInitialState();
 foreach(var command in commands){var result=resolver.Resolve(state,command);if(!result.Accepted)return false;state=result.State;}
 return state.Status==PuzzleStatus.Won;
 }
 static LevelDefinition Reduce(LevelDefinition l,int floor,int hp)=>new LevelDefinition(l.PuzzleId,l.CampaignNodeId,l.SchemaVersion,l.RulesVersion,"audit-reduced",l.InitialSymbolIds,l.TargetSymbolIds,l.TargetSegments,
 l.Floors.Select((f,i)=>i!=floor?f:new FloorDefinition(hp,f.OpenEdges,f.FragileEdges,f.LinkedPair,f.EntryTransform,f.Bomb,f.Key,f.Repair,f.OneUseBridgeEdge,f.OptionalStamp,f.Powerup,f.GustAfterSwaps,f.GustTransform,f.GustDamage)),l.ReachCharges,l.TeachingNode);
 static void Main(string[] args){
 string root=args[0],folder=args.Length>1?Path.GetFullPath(args[1]):Path.Combine(root,"Reports","CampaignRoadmap-Oct09");Directory.CreateDirectory(folder);
 string Read(string n)=>File.ReadAllText(Path.Combine(root,"Data",n+".json"));
 var c=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
 var store=new PublishedTriadStore(Read("production_triad_recipes"),Read("production_triad_manifest"));
 int position=0,stages=0,bombs=0,probes=0,replays=0,benefits=0,entryTransforms=0,gustTransforms=0;var fingerprints=new HashSet<string>();
 var rows=new List<string>{"position,node,world,island,stage,principle,width,boards,minimum,repair,key,bomb,ribbon,frost,gust,entry"};
 foreach(var content in c.PrimaryLevels){
 using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(30));
 var run=store.Bind(content.Definition,content.Locale,c.ContentHash);if(run==null)throw new Exception("Missing published run");
 if(!fingerprints.Add(string.Join("|",run.Stages.Select(s=>s.Triad.Fingerprint))))throw new Exception("Duplicate triad "+content.Definition.CampaignNodeId);
 foreach(var localized in c.LocalizedLevels.Where(l=>l.Definition.CampaignNodeId==content.Definition.CampaignNodeId)){store.Bind(localized.Definition,localized.Locale,c.ContentHash);replays+=3;}
 for(int stage=0;stage<3;stage++){
  var l=run.Level(content.Definition,stage);var family=(MechanicLesson)run.Stages[stage].Triad.Lesson;var resolver=new PuzzleResolver(l);
  if(l.Floors[0].EntryTransform!=EntryTransform.None||l.Floors[0].GustAfterSwaps!=0)throw new Exception("Starting transform");
  var proof=PuzzleSolver.FindShortest(resolver,stateBudget:60000,cancellationToken:stop.Token);
  if(proof.Status!=SearchStatus.Solved||proof.OptimalActions!=run.Stages[stage].Minimum||!CampaignTriads.UsesPrinciple(l,proof.Path,family))throw new Exception("Wrong optimal certificate "+l.CampaignNodeId+" stage "+stage);
  if(!CampaignTriads.BenefitsRequired(l,stop.Token))throw new Exception("Bypassable benefit "+l.CampaignNodeId);
  benefits+=l.Floors.Sum(f=>(f.Repair!=null?1:0)+(f.Key!=null?1:0)+(f.Powerup!=null?1:0));
  if(!CampaignTriads.HasLiveBombWindows(l))throw new Exception("Inert bomb deadline");
  if(l.Floors.Any(f=>f.Bomb!=null)&&CampaignTriads.WithMissedBomb(l,stop.Token).Status!=SearchStatus.Unsolvable)throw new Exception("Ignored bomb win");
  var state=resolver.CreateInitialState();var floors=new HashSet<int>{0};
  foreach(var command in proof.Path){int oldFloor=state.ActiveFloor;var result=resolver.Resolve(state,command);
   if(!result.Accepted||!LevelValidator.SameMultiset(state.Row,result.State.Row))throw new Exception("Non-atomic exchange");
   if(command.Kind!=CommandKind.EarlyDrop)floors.Add(oldFloor);
   IReadOnlyList<SymbolId> preceding=state.Row;
   foreach(var e in result.Events){
    if(e.Kind==PuzzleEventKind.EntryPermuted||e.Kind==PuzzleEventKind.WindPermuted){
     if(preceding.SequenceEqual(e.Row))throw new Exception("Transform fires without changing row: "+l.CampaignNodeId);
     if(e.Kind==PuzzleEventKind.EntryPermuted){entryTransforms++;floors.Add(e.Floor);}else gustTransforms++;
    }
    preceding=e.Row;
   }
   state=result.State;
  }
  if(state.Status!=PuzzleStatus.Won||floors.Count!=l.Floors.Count)throw new Exception("Idle board or failed delivery");
  for(int f=0;f<l.Floors.Count;f++){
   if(l.Floors[f].EntryTransform!=EntryTransform.None&&WinsWithoutTransform(l,f,false,proof.Path))throw new Exception("Entry transform does not affect certified solution: "+l.CampaignNodeId+" stage "+stage+" floor "+f);
   if(l.Floors[f].GustAfterSwaps>0&&WinsWithoutTransform(l,f,true,proof.Path))throw new Exception("Gust does not affect certified solution: "+l.CampaignNodeId+" stage "+stage+" floor "+f);
   if(l.Floors[f].Bomb!=null)bombs++;
   for(int hp=1;hp<l.Floors[f].BaseDurability;hp++){
    var reduced=Reduce(l,f,hp);var p=PuzzleSolver.FindShortest(new PuzzleResolver(reduced),stateBudget:60000,cancellationToken:stop.Token);probes++;
    if(p.Status==SearchStatus.Unknown||p.Status==SearchStatus.Solved&&CampaignTriads.UsesPrinciple(reduced,p.Path,family))throw new Exception("Excess durability "+l.CampaignNodeId+" stage "+stage+" floor "+f);
   }
  }
  rows.Add(string.Join(",",position+1,l.CampaignNodeId,position/60+1,position/12+1,stage+1,family,l.TargetSymbolIds.Count,l.Floors.Count,proof.OptimalActions,l.Floors.Count(f=>f.Repair!=null),l.Floors.Count(f=>f.Key!=null),l.Floors.Count(f=>f.Bomb!=null),l.Floors.Count(f=>f.Powerup?.Kind==PowerupKind.Ribbon),l.Floors.Count(f=>f.Powerup?.Kind==PowerupKind.Hourglass),l.Floors.Count(f=>f.GustAfterSwaps>0),l.Floors.Count(f=>f.EntryTransform!=EntryTransform.None)));stages++;replays++;
 }
 position++;if(position%60==0)Console.WriteLine(position+" packaged triads checked; "+probes+" lower-durability counterexample probes");
 File.WriteAllText(Path.Combine(folder,"audit-progress.txt"),position+" triads checked\n");
 }
 File.WriteAllLines(Path.Combine(folder,"progression.csv"),rows);
 string summary=$"PASS: {position} distinct packaged triads; {stages} optimal stage certificates; {replays} primary/localized witness replays; {bombs} live bomb deadlines; {benefits} benefit ablations; {probes} lower-durability probes; {entryTransforms} effective entry permutations and {gustTransforms} effective gusts whose removal invalidates the certified solution; atomic multisets and no idle/starting-transform boards. Human enjoyment/device input are not proved.";
 File.WriteAllText(Path.Combine(folder,"audit-result.txt"),summary+"\n");Console.WriteLine(summary);
 }
}
