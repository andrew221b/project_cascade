using System;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using CascadeAccountModule;
using Newtonsoft.Json;
using NUnit.Framework;
namespace Cascade.Tests {
 public sealed class HalfStarServerChecks {
  static PlayerProfile Complete(bool slow,int undos=0,string node="C01"){
   var source=PublishedCatalog.Primary(node);var run=PublishedCatalog.Run(source.Definition);AttemptService final=null;
   for(int i=0;i<3;i++){
    final=new AttemptService(run.Level(source.Definition,i),source.Locale);final.AdvanceActiveTime(slow?300:1);
    for(int u=i;u<undos;u+=3){
     string first=run.Stages[0].Witness[0];final.Execute(first=="D"?PuzzleCommand.Drop:first.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(first.Substring(1).Split(':')[0]),int.Parse(first.Split(':')[1])):PuzzleCommand.Swap(int.Parse(first)));
     Assert.That(final.Undo(),Is.True,"the fixture undo must leave its floor intact");
    }
    foreach(string c in run.Stages[i].Witness)final.Execute(c=="D"?PuzzleCommand.Drop:c.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c)));
    if(i<2)run.AcceptCheckpoint(source.Definition,final);
   }
   var profile=new PlayerProfile{RatingRevision=1,Descent=run,Attempt=AttemptRecord.Capture(final),Completion=LevelCompletion.From(final,null,run.Performance(final))};
   profile.RecordClear(final,profile.Completion);profile.Rewards.Synchronize(new[]{node});profile.Validate();return profile;
  }
  [Test] public void PublishedReplayVerifierAcceptsRealHalfAndRejectsInflatedBest(){
   var half=Complete(true);Assert.That(half.Completion.StarRating,Is.EqualTo(2.5));Assert.That(PublishedCatalog.ValidProgress(half,new PlayerProfile()),Is.True);
   half.Clears[0].BestStars=3;half.Clears[0].BestHalfStar=false;Assert.That(PublishedCatalog.ValidProgress(half,new PlayerProfile()),Is.False);
  }
  [Test] public void ServerVerifiesHalfReplayCreditAndPreservesLegacyFloorCredit(){
   var trusted=Complete(false);var local=JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(trusted));var replay=Complete(true);
   local.Descent=replay.Descent;local.RecordClear(replay.RestoreAttempt(Catalog()),replay.Completion);local.Validate();
   Assert.That(local.Clears[0].ReplayRewardHalfUnits,Is.EqualTo(5));Assert.That(PublishedCatalog.ValidProgress(local,trusted),Is.True);
   local.Clears[0].ReplayRewardHalfStar=false;local.Rewards.ReplayCredits[0].HalfStar=false;
   Assert.That(PublishedCatalog.ValidProgress(local,trusted),Is.True,"older whole-floor credit remains valid under the new rating");
   local.Clears[0].ReplayRewardStars=3;local.Rewards.ReplayCredits[0].Stars=3;Assert.That(PublishedCatalog.ValidProgress(local,trusted),Is.False);
  }
  static Cascade.Content.ContentCatalog Catalog()=>ContentTests.LoadCatalog();
  [Test] public void ActualUndoWitnessEarnsOneAndHalfAndServerVerifiesIt(){
   var result=Complete(false,3,"C01");Assert.That(result.Completion.StarRating,Is.EqualTo(1.5));
   Assert.That(PublishedCatalog.ValidProgress(result,new PlayerProfile()),Is.True);
  }
  [Test] public void NewHalfFieldsDoNotAppearInLegacyJsonOrChangeItsRating(){
   var old=new ClearReceipt{BestStars=2};string json=JsonConvert.SerializeObject(old);
   Assert.That(json.Contains("HalfStar"),Is.False);Assert.That(JsonConvert.DeserializeObject<ClearReceipt>(json).BestStarRating,Is.EqualTo(2));
  }
 }
}
