using System;using System.Collections.Generic;using System.IO;using System.Linq;using System.Threading;using Cascade.Application;using Cascade.Content;using Cascade.Domain;using Cascade.Solver;
class Audit {
 static PuzzleCommand Parse(string c)=>c=="D"?PuzzleCommand.Drop:c.StartsWith("R")?PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c));
 static void Main(string[] args){
  string root=args[0],outdir=Path.Combine(root,"Reports","EffectBudget-Oct09");Directory.CreateDirectory(outdir);
  string Read(string f)=>File.ReadAllText(Path.Combine(root,"Data",f));
  var catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
  var rows=new List<string>{"node,type,stage,floor,benefit,minimum,status_without"};int fail=0,stages=0;
  Cascade.Tests.ReferenceFixtures.PackageRoot=root;
  var effects=new Cascade.Tests.EffectBudgetTests();effects.Load();
  var missions=new Cascade.Tests.PersonalMissionTests();missions.Load();
  var triads=new Cascade.Tests.CampaignTriadTests();triads.Load();
  int tests=0;
  foreach(string n in new[]{"C02","C04","C06","C07","C65","C120","C360","C500"}){effects.EveryBenefitIsNeededInCampaignAndMissionStages(n);tests++;}
  foreach(string n in new[]{"C03","C06","C07","C11","C14","C37","C65","C120","C360","C500"}){missions.MissionsAreHarderWithinABoundedBandAndEveryStageReplays(n);tests++;}
  missions.AcceptedMissionAndCampaignCheckpointSurviveSaveAndModeSwitch();missions.DeclineUsesProgressIntervalAndCancellationNeverReturnsAnOffer();
  triads.RepairActuallyAddsNeededDurabilityAndDisablingItLosesTheSamePuzzle();triads.RibbonAndFrostHaveRealGameplayConsequencesInAllThreeTasks();triads.TriadCheckpointRoundTripsThroughTheActualProfileStoreAndRetainsUndo();tests+=5;
  File.WriteAllText(Path.Combine(outdir,"pure-tests.txt"),"PASS: "+tests+" focused NUnit methods invoked on the actual production sources, using Mono outside the locked Editor.\n");Console.WriteLine("Focused NUnit methods passed: "+tests);
  int campaign=0; if(args.Contains("--full")) foreach(var content in catalog.PrimaryLevels){
   using(var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(15))){
    var run=DescentRun.Build(content.Definition,version:6,cancellation:deadline.Token);run.Validate(content.Definition,content.Locale);
    foreach(var binding in catalog.LocalizedLevels.Where(c=>c.Definition.CampaignNodeId==content.Definition.CampaignNodeId)){
     var localized=new DescentRun{Version=6,SourceHash=binding.Definition.ContentHash,Stages=run.Stages};localized.Validate(binding.Definition,binding.Locale);
    }
    for(int k=0;k<3;k++){var level=run.Level(content.Definition,k);if(!CampaignTriads.BenefitsRequired(level,deadline.Token))throw new Exception(content.Definition.CampaignNodeId+" stage "+k+" bypass");}
    campaign++;
   }
  }if(campaign>0)Console.WriteLine("Campaign recipes checked: "+campaign);
  foreach(string node in new[]{"C02","C04","C06","C07","C65","C120","C360","C500"}){
   using(var stop=new CancellationTokenSource(TimeSpan.FromSeconds(30))){
    var source=catalog.GetPrimary(node).Definition;var baseline=DescentRun.Build(source,version:6,cancellation:stop.Token);
    var mission=CampaignTriads.BuildPersonalMission(source,1,null,stop.Token,reference:baseline);
    if(mission==null){Console.WriteLine(node+" no mission");fail++;continue;}
    foreach(var run in new[]{baseline,mission})for(int stage=0;stage<3;stage++){
     var level=run.Level(source,stage);var actual=new AttemptService(level);
     foreach(var c in run.Stages[stage].Witness.Select(Parse))if(!actual.Execute(c).Accepted)throw new Exception("Rejected witness");
     if(actual.Current.Status!=PuzzleStatus.Won||!actual.Current.Row.SequenceEqual(source.TargetSymbolIds))throw new Exception("Wrong receiver");stages++;
     for(int floor=0;floor<level.Floors.Count;floor++){
      var f=level.Floors[floor];var benefits=new[]{f.Repair!=null?(PuzzleEventKind?)PuzzleEventKind.Repaired:null,f.Key!=null?(PuzzleEventKind?)PuzzleEventKind.KeyOpened:null,f.Powerup==null?null:(PuzzleEventKind?)(f.Powerup.Kind==PowerupKind.Hourglass?PuzzleEventKind.HourglassCollected:PuzzleEventKind.RibbonCollected)};
      foreach(var b in benefits.Where(b=>b.HasValue)){
       var proof=CampaignTriads.WithoutBenefit(level,floor,b.Value,stop.Token);string label=node+","+(run==baseline?"campaign":"mission")+","+(stage+1)+","+(floor+1)+","+b+","+run.Stages[stage].Minimum+","+proof.Status;rows.Add(label);
       if(proof.Status!=SearchStatus.Unsolvable){Console.WriteLine(label);fail++;}
      }
     }
    }
   }
  }
  File.WriteAllLines(Path.Combine(outdir,"benefit-audit.csv"),rows);string result=(campaign>0?"Campaign recipes checked: "+campaign+"; ":"")+"Stages replayed in detailed sample: "+stages+"; counterfactuals: "+(rows.Count-1)+"; bypasses or unknown: "+fail;File.WriteAllText(Path.Combine(outdir,"logic-result.txt"),result+"\n");Console.WriteLine(result);Environment.ExitCode=fail==0?0:1;
 }
}

namespace Cascade.Tests { public static class ReferenceFixtures { public static string PackageRoot; } }
