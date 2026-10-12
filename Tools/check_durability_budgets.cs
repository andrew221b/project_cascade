using System;using System.Collections.Generic;using System.IO;using System.Linq;using System.Threading;
using Cascade.Application;using Cascade.Content;using Cascade.Domain;using Cascade.Solver;using Cascade.Tests;
class Audit {
 static PuzzleCommand Parse(string c)=>c=="D"?PuzzleCommand.Drop:c.StartsWith("R")?PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c));
 static void Main(string[] args){
  string root=args[0],output=Path.Combine(root,"Reports","Durability-Oct09");Directory.CreateDirectory(output);ReferenceFixtures.PackageRoot=root;
  string Read(string file)=>File.ReadAllText(Path.Combine(root,"Data",file));
  var catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
  var rows=new List<string>{"node,stage,lesson,row,floors,hp,minimum,witness"};var probes=new List<string>{"node,stage,floor,hp,status,keeps_principle"};
  int primary=0,replays=0,benefits=0;long slowest=0;
  foreach(var content in args.Contains("--focused")?Enumerable.Empty<LevelContent>():catalog.PrimaryLevels){
   var watch=System.Diagnostics.Stopwatch.StartNew();using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(30));
   try{
    var source=content.Definition;var run=DescentRun.Build(source,version:6,cancellation:deadline.Token);run.Validate(source,content.Locale);replays+=3;
    foreach(var binding in catalog.LocalizedLevels.Where(c=>c.Definition.CampaignNodeId==source.CampaignNodeId)){
     var clone=new DescentRun{Version=6,SourceHash=binding.Definition.ContentHash,Stages=run.Stages};clone.Validate(binding.Definition,binding.Locale);replays+=3;
    }
    for(int stage=0;stage<3;stage++){
     var level=run.Level(source,stage);var family=(MechanicLesson)run.Stages[stage].Triad.Lesson;
     if(level.Floors[0].EntryTransform!=EntryTransform.None||level.Floors[0].GustAfterSwaps!=0)throw new Exception("Starting transform");
     if(!CampaignTriads.UsesPrinciple(level,run.Stages[stage].Witness.Select(Parse),family))throw new Exception("Unused principle/board");
     if(!CampaignTriads.BenefitsRequired(level,deadline.Token))throw new Exception("Unnecessary benefit");
     benefits+=level.Floors.Sum(f=>(f.Repair!=null?1:0)+(f.Key!=null?1:0)+(f.Powerup!=null?1:0));
     for(int floor=0;floor<level.Floors.Count;floor++)for(int hp=1;hp<level.Floors[floor].BaseDurability;hp++){
      var reduced=DurabilityBudgetTests.Reduce(level,floor,hp);var proof=PuzzleSolver.FindShortest(new PuzzleResolver(reduced),stateBudget:60000,cancellationToken:deadline.Token);
      bool keeps=proof.Status==SearchStatus.Solved&&CampaignTriads.UsesPrinciple(reduced,proof.Path,family);
      probes.Add(string.Join(",",source.CampaignNodeId,stage+1,floor+1,hp,proof.Status,keeps));
      if(proof.Status==SearchStatus.Unknown||keeps)throw new Exception("Reducible/unknown budget");
     }
     rows.Add(string.Join(",",source.CampaignNodeId,stage+1,family,source.TargetSymbolIds.Count,level.Floors.Count,"\""+string.Join(" ",level.Floors.Select(f=>f.BaseDurability))+"\"",run.Stages[stage].Minimum,"\""+string.Join(" ",run.Stages[stage].Witness)+"\""));
    }
    primary++;slowest=Math.Max(slowest,watch.ElapsedMilliseconds);
    if(primary%50==0)Console.WriteLine(primary+" campaign recipes / "+replays+" localized stage replays");
   }catch(Exception error){File.WriteAllLines(Path.Combine(output,"partial-proofs.csv"),rows);File.WriteAllText(Path.Combine(output,"failure.txt"),content.Definition.CampaignNodeId+"\n"+error);throw;}
  }
  if(primary>0){File.WriteAllLines(Path.Combine(output,"campaign-proofs.csv"),rows);File.WriteAllLines(Path.Combine(output,"lower-hp-probes.csv"),probes);}
  var tests=new DurabilityBudgetTests();tests.Load();int methods=0;
  foreach(string node in new[]{"C01","C02","C07","C08","C09","C10","C11","C12","C13","C14","C37","C65","C120","C360","C500"}){tests.CampaignAndMissionHaveNoReducibleLessonBudgetOrStartingTransform(node);methods++;}
  tests.CancellationNeverAcceptsAnUncalibratedBudget();methods++;
  foreach(bool right in new[]{false,true})foreach(bool fragile in new[]{false,true}){tests.WindAndCoupledFragileBridgeResolveFinalContactsBeforeBreak(right,fragile);methods++;}
  var effects=new EffectBudgetTests();effects.Load();foreach(string n in new[]{"C02","C04","C06","C07","C65","C120","C360","C500"}){effects.EveryBenefitIsNeededInCampaignAndMissionStages(n);methods++;}
  var missions=new PersonalMissionTests();missions.Load();foreach(string n in new[]{"C03","C06","C07","C11","C14","C37","C65","C120","C360","C500"}){missions.MissionsAreHarderWithinABoundedBandAndEveryStageReplays(n);methods++;}
  missions.AcceptedMissionAndCampaignCheckpointSurviveSaveAndModeSwitch();missions.DeclineUsesProgressIntervalAndCancellationNeverReturnsAnOffer();methods+=2;
  var triads=new CampaignTriadTests();triads.Load();triads.RepairActuallyAddsNeededDurabilityAndDisablingItLosesTheSamePuzzle();triads.RibbonAndFrostHaveRealGameplayConsequencesInAllThreeTasks();triads.TriadCheckpointRoundTripsThroughTheActualProfileStoreAndRetainsUndo();methods+=3;
  string summary=$"PASS: {primary} campaign triads; {replays} exact primary/localized stage replays; {probes.Count-1} lower-HP probes; {benefits} benefit ablations; {methods} actual focused NUnit methods. Slowest campaign plus audits {slowest} ms.";
  File.WriteAllText(Path.Combine(output,primary>0?"result.txt":"focused-result.txt"),summary+"\n");Console.WriteLine(summary);
 }
}
namespace Cascade.Tests { public static class ReferenceFixtures { public static string PackageRoot; } }
