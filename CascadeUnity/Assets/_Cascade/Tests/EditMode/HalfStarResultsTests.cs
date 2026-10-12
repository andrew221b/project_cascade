using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Newtonsoft.Json;
using NUnit.Framework;
namespace Cascade.Tests
{
    public sealed class HalfStarResultsTests
    {
        [TestCase(12,0,false,true,80,6)]
        [TestCase(12,0,false,true,127,5)]
        [TestCase(12,1,false,true,80,5)]
        [TestCase(14,1,false,true,80,4)]
        [TestCase(19,2,false,true,80,3)]
        [TestCase(21,0,false,true,80,2)]
        [TestCase(12,0,true,true,80,2)]
        [TestCase(12,0,false,false,80,2)]
        [TestCase(10,0,false,true,80,2)]
        public void FiveExactRatingSteps(int moves,int undos,bool help,bool clean,double seconds,int units)
            =>Assert.That(LevelCompletion.RateHalfUnits(moves,12,undos,help,clean,seconds,3),Is.EqualTo(units));
        [Test] public void OldWholeRatingsRemainWholeAndMalformedHalvesAreRejected()
        {
            var old=new LevelCompletion{NodeId="C01",AttemptId=Guid.NewGuid().ToString("D"),Stars=2,Stages=3,RatingVersion=2,Score=1700};
            var restored=JsonConvert.DeserializeObject<LevelCompletion>(JsonConvert.SerializeObject(old));
            Assert.That(restored.StarRating,Is.EqualTo(2));Assert.That(restored.IsValid(),Is.True);
            old.HalfStar=true;Assert.That(old.IsValid(),Is.False);old.RatingVersion=3;Assert.That(old.IsValid(),Is.True);
            old.Stars=3;Assert.That(old.IsValid(),Is.False);
        }
        [Test] public void ActualSlowOptimalWitnessAwardsAndPersistsTwoAndHalf()
        {
            var source=ContentTests.LoadCatalog().GetPrimary("C01");var run=DescentRun.Build(source.Definition,version:6);AttemptService final=null;
            for(int stage=0;stage<3;stage++){
                final=new AttemptService(run.Level(source.Definition,stage),source.Locale);final.AdvanceActiveTime(300);
                foreach(string command in run.Stages[stage].Witness)final.Execute(Parse(command));
                Assert.That(final.Current.Status,Is.EqualTo(PuzzleStatus.Won));if(stage<2)run.AcceptCheckpoint(source.Definition,final);
            }
            var result=LevelCompletion.From(final,null,run.Performance(final));Assert.That(result.StarRating,Is.EqualTo(2.5));Assert.That(result.IsValid(),Is.True);
            var profile=new PlayerProfile{Descent=run,Completion=result,Attempt=AttemptRecord.Capture(final)};
            Assert.That(profile.RecordClear(final,result),Is.True);profile.Rewards.Synchronize(new[]{"C01"});profile.Validate();
            string json=JsonConvert.SerializeObject(profile);var restored=JsonConvert.DeserializeObject<PlayerProfile>(json);restored.MigrateRatings();restored.Validate();
            Assert.That(restored.Completion.StarRating,Is.EqualTo(2.5));Assert.That(restored.Clears.Single().BestStarRating,Is.EqualTo(2.5));
            Assert.That(restored.RecordClear(restored.RestoreAttempt(ContentTests.LoadCatalog()),restored.Completion),Is.False);
            Assert.That(restored.Rewards.ReplayCredits,Is.Null,"same attempt never grants replay credit");
        }
        static PuzzleCommand Parse(string c)=>c=="D"?PuzzleCommand.Drop:c.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c));
        [Test] public void ReplayHalfCreditIsBoundedMonotoneAndPaidOnce()
        {
            var rewards=new JourneyRewards();rewards.Synchronize(new[]{"C01"});int initial=rewards.EarnedCoins(new[]{"C01"});
            rewards.RecordReplayCredit("C01",2);Assert.That(rewards.EarnedCoins(new[]{"C01"}),Is.EqualTo(initial+4));
            rewards.RecordReplayCredit("C01",2,true);Assert.That(rewards.EarnedCoins(new[]{"C01"}),Is.EqualTo(initial+5));
            rewards.RecordReplayCredit("C01",2,true);rewards.RecordReplayCredit("C01",1,true);Assert.That(rewards.EarnedCoins(new[]{"C01"}),Is.EqualTo(initial+5));
            rewards.RecordReplayCredit("C01",3);Assert.That(rewards.EarnedCoins(new[]{"C01"}),Is.EqualTo(initial+6));
            Assert.Throws<ArgumentException>(()=>rewards.RecordReplayCredit("C01",3,true));rewards.Validate();
        }
        [Test] public void CloudMergeKeepsHalfAndSeparateBestScore()
        {
            var a=new PlayerProfile{RatingRevision=1};a.Clears.Add(new ClearReceipt{CampaignNodeId="C01",AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64),BestStars=2,BestHalfStar=true,BestScore=1700});a.Rewards.Synchronize(new[]{"C01"});
            var b=JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(a));b.Clears[0].BestHalfStar=false;b.Clears[0].BestScore=1750;
            var merged=CloudReconciler.Merge(new PlayerProfile(),a,b,new[]{"C01"}).Profile;
            Assert.That(merged.Clears.Single().BestStarRating,Is.EqualTo(2.5));Assert.That(merged.Clears.Single().BestScore,Is.EqualTo(1750));merged.Validate();
            Assert.That(LevelCompletion.FormatStars(merged.Clears.Sum(c=>c.BestStarRating)),Is.EqualTo("2.5"));
        }
    }
}
