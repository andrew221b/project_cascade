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
    public sealed class FiveStarResultsTests
    {
        [TestCase(12,0,false,true,80,5)]
        [TestCase(12,0,false,true,127,4)]
        [TestCase(14,1,false,true,80,4)]
        [TestCase(15,2,false,true,80,3)]
        [TestCase(12,3,false,true,80,2)]
        [TestCase(12,0,true,true,80,2)]
        [TestCase(12,0,false,false,80,1)]
        [TestCase(20,0,false,true,80,1)]
        [TestCase(10,0,false,true,80,1)]
        public void RankHasFiveMeaningfulStepsAndHelpRetainsItsCeiling(int moves,int undos,bool help,bool clean,double seconds,int expected)
            =>Assert.That(LevelCompletion.Rate(moves,12,undos,help,clean,seconds,3),Is.EqualTo(expected));
        [Test] public void UnknownLegacyTimeCannotEarnSpeedStarAndBoundaryIsInclusive()
        {
            Assert.That(LevelCompletion.Rate(12,12,0,false,true,null,3),Is.EqualTo(4));
            Assert.That(LevelCompletion.Rate(12,12,0,false,true,126,3),Is.EqualTo(5));
            Assert.That(LevelCompletion.Rate(12,null,0,false,true,1,3),Is.EqualTo(1));
        }
        static AttemptService Playing()=>new AttemptService(LevelDefinitionTests.Level(LevelDefinitionTests.Row("DCBA"),LevelDefinitionTests.Row("ABCD"),new[]{new FloorDefinition(12,new[]{true,true,true})}));
        [Test] public void UndoPreservesActualMovesAndTimeAndFailedCommandsDoNotCount()
        {
            var a=Playing();a.AdvanceActiveTime(7.25);a.Execute(PuzzleCommand.Swap(1));a.Execute(PuzzleCommand.Swap(0));a.Undo();a.Execute(PuzzleCommand.Swap(-1));
            Assert.That(a.RouteLength,Is.EqualTo(1));Assert.That(a.MovesMade,Is.EqualTo(2));Assert.That(a.ActiveSeconds,Is.EqualTo(7.25));
            var record=JsonConvert.DeserializeObject<AttemptRecord>(JsonConvert.SerializeObject(AttemptRecord.Capture(a)));var restored=AttemptService.Restore(a.Level,record);
            Assert.That(restored.MovesMade,Is.EqualTo(2));Assert.That(restored.ActiveSeconds,Is.EqualTo(7.25));Assert.That(restored.CurrentStateHash,Is.EqualTo(a.CurrentStateHash));
            restored.Retry();Assert.That(restored.MovesMade,Is.Zero);Assert.That(restored.ActiveSeconds,Is.Zero);
        }
        [Test] public void LegacyAttemptIsUnknownAndInvalidTelemetryIsRejected()
        {
            var a=Playing();a.Execute(PuzzleCommand.Swap(1));a.Undo();var r=AttemptRecord.Capture(a);r.MovesMade=null;r.ActiveSeconds=null;
            var old=AttemptService.Restore(a.Level,r);old.AdvanceActiveTime(4);Assert.That(old.ActiveSeconds,Is.Null);Assert.That(old.MovesMade,Is.EqualTo(1));
            r.ActiveSeconds=double.NaN;Assert.Throws<ArgumentException>(()=>AttemptService.Restore(a.Level,r));
            r.ActiveSeconds=1;r.MovesMade=0;Assert.Throws<ArgumentException>(()=>AttemptService.Restore(a.Level,r));
            Assert.Throws<ArgumentOutOfRangeException>(()=>a.AdvanceActiveTime(double.NaN));
        }
        [Test] public void AllThreeStageTelemetryAggregatesAndLegacyReceiptsAreNotRescored()
        {
            string Read(string n)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",n));
            var c=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
            var source=c.GetPrimary("C01").Definition;var run=DescentRun.Build(source,version:4);AttemptService a=null;
            for(int i=0;i<3;i++){
                a=new AttemptService(run.Level(source,i));a.AdvanceActiveTime(10+i);
                foreach(var cmd in run.Stages[i].Witness)a.Execute(cmd=="D"?PuzzleCommand.Drop:PuzzleCommand.Swap(int.Parse(cmd)));
                a.AdvanceActiveTime(100);Assert.That(a.ActiveSeconds,Is.EqualTo(10+i),"won time freezes");if(i<2)run.AcceptCheckpoint(source,a);
            }
            var perf=run.Performance(a);var receipt=LevelCompletion.From(a,null,perf);
            Assert.That(receipt.ActiveSeconds,Is.EqualTo(33));Assert.That(receipt.Stars,Is.EqualTo(5));Assert.That(receipt.Score,Is.EqualTo(2000));Assert.That(receipt.IsValid(),Is.True);
            var old=new LevelCompletion{NodeId="C01",AttemptId=Guid.NewGuid().ToString("D"),Stages=3,Stars=3,Score=1800};Assert.That(old.IsValid(),Is.True);
            old.Stars=5;Assert.That(old.IsValid(),Is.False);old.RatingVersion=1;Assert.That(old.IsValid(),Is.True);
            var profile=new PlayerProfile{Completion=receipt};Assert.That(profile.RecordClear(a,receipt),Is.True);Assert.That(profile.RecordClear(a,receipt),Is.False);Assert.That(profile.Clears.Single().BestStars,Is.EqualTo(5));profile.Validate();
            run.Completed[0].ActiveSeconds=null;Assert.That(run.Performance(a).ActiveSeconds,Is.Null);
        }
    }
}
