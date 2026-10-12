using System;using System.Collections.Generic;using System.IO;using System.Linq;using System.Threading;
using Cascade.Application;using Cascade.Content;using Cascade.Domain;using Cascade.Solver;using Cascade.Tests;
class BombAudit {
 static void Main(string[] args){
  string root=args[0],folder=Path.Combine(root,"Reports","BombChallenge-Oct09");Directory.CreateDirectory(folder);ReferenceFixtures.PackageRoot=root;
  string Read(string f)=>File.ReadAllText(Path.Combine(root,"Data",f));
  var catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
  if(args.Contains("--save-only")){
   var save=new BombChallengeTests();save.Load();save.PinnedEqualFuseRecipeRetainsItsReplayAndAttemptAfterSave();
   File.WriteAllText(Path.Combine(folder,"pinned-save-result.txt"),"PASS: actual ProfileStore round trip preserves old equal-fuse recipe hash and in-progress attempt.\n");
   Console.WriteLine("PASS: pinned old bomb recipe/save compatibility.");return;
  }
  bool baseline=args.Contains("--baseline"),full=args.Contains("--full");
  var sample=new[]{"C03","C07","C37","C65","C120","C360","C500"};
  var lines=new List<string>{"node,stage,floor,hp,fuse,unsafe_delivery,witness"};int runs=0,replays=0,bombs=0,lowerProbes=0;
  var sources=full?catalog.PrimaryLevels.AsEnumerable():sample.Select(n=>catalog.GetPrimary(n));
  foreach(var content in sources){
   using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(30));
   try{
    var source=content.Definition;var run=DescentRun.Build(source,version:6,cancellation:stop.Token);run.Validate(source,content.Locale);runs++;replays+=3;
    if(full)foreach(var binding in catalog.LocalizedLevels.Where(c=>c.Definition.CampaignNodeId==source.CampaignNodeId)){
     new DescentRun{Version=6,SourceHash=binding.Definition.ContentHash,Stages=run.Stages}.Validate(binding.Definition,binding.Locale);replays+=3;
    }
    for(int stage=0;stage<3;stage++){
     var level=run.Level(source,stage);var unsafeWin=level.Floors.Any(f=>f.Bomb!=null)?CampaignTriads.WithMissedBomb(level,stop.Token):null;
     for(int floor=0;floor<level.Floors.Count;floor++)if(level.Floors[floor].Bomb!=null){
      var f=level.Floors[floor];bombs++;
      lines.Add(string.Join(",",source.CampaignNodeId,stage+1,floor+1,f.BaseDurability,f.Bomb.Ticks,unsafeWin.Status,"\""+string.Join(" ",unsafeWin.Path)+"\""));
     }
     if(!baseline&&(!CampaignTriads.HasLiveBombWindows(level)||unsafeWin!=null&&unsafeWin.Status!=SearchStatus.Unsolvable))throw new Exception("Inert/ignored/unknown bomb");
     if(!baseline&&!CampaignTriads.BenefitsRequired(level,stop.Token))throw new Exception("Benefit bypass");
     if(full)for(int floor=0;floor<level.Floors.Count;floor++)for(int hp=1;hp<level.Floors[floor].BaseDurability;hp++){
      var reduced=DurabilityBudgetTests.Reduce(level,floor,hp);
      var proof=PuzzleSolver.FindShortest(new PuzzleResolver(reduced),stateBudget:60000,cancellationToken:stop.Token);lowerProbes++;
      if(proof.Status==SearchStatus.Unknown||proof.Status==SearchStatus.Solved&&CampaignTriads.UsesPrinciple(reduced,proof.Path,(MechanicLesson)run.Stages[stage].Triad.Lesson))
       throw new Exception("Reducible/unknown lesson budget");
     }
    }
    if(full&&runs%100==0)Console.WriteLine(runs+" campaign recipes checked");
   }catch(Exception error){File.WriteAllLines(Path.Combine(folder,"partial.csv"),lines);File.WriteAllText(Path.Combine(folder,"failure.txt"),content.Definition.CampaignNodeId+"\n"+error);throw;}
  }
  File.WriteAllLines(Path.Combine(folder,baseline?"baseline.csv":full?"catalog.csv":"sample.csv"),lines);
  int methods=0;
  if(!baseline){
   var tests=new BombChallengeTests();tests.Load();
   tests.EqualFuseAndWoodAddsNoDeadlineToTheTwoExchangeSolution();tests.MissingAnEarlierDeadlineLosesWhileCorrectPriorityStillWins();
   tests.LaterPlatformsCanExposeAWinningExplosionBypass();tests.AbandonmentAndUnknownCannotPassTheBombChallengeGuard();methods+=4;
   tests.PinnedEqualFuseRecipeRetainsItsReplayAndAttemptAfterSave();methods++;
   foreach(string n in sample){tests.CampaignAndMissionBombsHaveMeaningfulWindowsAndNoIgnoredWin(n);methods++;}
   var durability=new DurabilityBudgetTests();durability.Load();
   foreach(string n in new[]{"C01","C02","C07","C08","C09","C10","C11","C12","C13","C14","C37","C65","C120","C360","C500"}){
    durability.CampaignAndMissionHaveNoReducibleLessonBudgetOrStartingTransform(n);methods++;
   }
   durability.CancellationNeverAcceptsAnUncalibratedBudget();methods++;
   foreach(bool right in new[]{false,true})foreach(bool fragile in new[]{false,true}){durability.WindAndCoupledFragileBridgeResolveFinalContactsBeforeBreak(right,fragile);methods++;}
   var effects=new EffectBudgetTests();effects.Load();foreach(string n in new[]{"C02","C04","C06","C07","C65","C120","C360","C500"}){effects.EveryBenefitIsNeededInCampaignAndMissionStages(n);methods++;}
   var missions=new PersonalMissionTests();missions.Load();foreach(string n in new[]{"C03","C06","C07","C11","C14","C37","C65","C120","C360","C500"}){missions.MissionsAreHarderWithinABoundedBandAndEveryStageReplays(n);methods++;}
   missions.AcceptedMissionAndCampaignCheckpointSurviveSaveAndModeSwitch();missions.DeclineUsesProgressIntervalAndCancellationNeverReturnsAnOffer();methods+=2;
   var triads=new CampaignTriadTests();triads.Load();triads.RepairActuallyAddsNeededDurabilityAndDisablingItLosesTheSamePuzzle();triads.RibbonAndFrostHaveRealGameplayConsequencesInAllThreeTasks();triads.TriadCheckpointRoundTripsThroughTheActualProfileStoreAndRetainsUndo();methods+=3;
  }
  string summary=$"{(baseline?"BASELINE":"PASS")}: {runs} campaign triads, {replays} primary/localized stage replays, {bombs} bombs audited, {lowerProbes} lower-HP probes, {methods} actual NUnit methods.";
  File.WriteAllText(Path.Combine(folder,baseline?"baseline.txt":full?"catalog-result.txt":"sample-result.txt"),summary+"\n");Console.WriteLine(summary);
 }
}
namespace Cascade.Tests { public static class ReferenceFixtures { public static string PackageRoot; } }
