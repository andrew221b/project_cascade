using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class DescentTests
    {
        ContentCatalog catalog;
        [OneTimeSetUp] public void Setup()
        {
            string Read(string file)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",file));
            catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
        }
        static PuzzleCommand Command(string c)=>c=="D"?PuzzleCommand.Drop:c.StartsWith("R",StringComparison.Ordinal)
            ?PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c));
        static AttemptService Solve(DescentRun run,LevelDefinition source,string locale="en")
        {
            var attempt=new AttemptService(run.Level(source,run.StageIndex),locale,direction:run.Direction);
            foreach(string c in run.Stages[run.StageIndex].Witness)Assert.That(attempt.Execute(Command(c)).Accepted,Is.True);
            Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));return attempt;
        }
        [Test] public void PublishedDescentCurvesAreProvenAndPreserveTheReceiverAndFinalDevices()
        {
            var lines=new List<string>{"puzzle,stage1,stage2,stage3,mood"};int proven=0;
            var content=catalog.PrimaryLevels.Where(c=>new[]{"C41","C81","C121","C160","C180","C540"}.Contains(c.Definition.CampaignNodeId)&&DeliveryDirector.CanDirect(c.Definition)).Concat(
                catalog.LocalizedLevels.Where(c=>new[]{"C41","C81","C121","C160"}.Contains(c.Definition.CampaignNodeId)));
            foreach(var c in content) {
                var source=c.Definition;var run=DescentRun.Build(source);
                lines.Add(c.Definition.PuzzleId+","+(run==null?"fallback":string.Join(",",run.Stages.Select(s=>s.Minimum)))+",steady");
                if(run==null)continue;proven++;run.Validate(source,c.Locale);
                Assert.That(run.Stages[0].Minimum,Is.EqualTo(2));Assert.That(run.Stages[1].Minimum,Is.GreaterThan(2));
                Assert.That(run.Stages[2].Minimum,Is.GreaterThanOrEqualTo(run.Stages[1].Minimum));
                for(int s=0;s<3;s++) {
                    var level=run.Level(source,s);LevelValidator.ThrowIfInvalid(level);
                    Assert.That(level.TargetSymbolIds,Is.EqualTo(source.TargetSymbolIds));Assert.That(level.TargetSegments,Is.EqualTo(source.TargetSegments));
                    Assert.That(LevelValidator.SameMultiset(level.InitialSymbolIds,source.TargetSymbolIds),Is.True);
                    if(s>0)Assert.That(level.InitialSymbolIds,Is.EqualTo(source.TargetSymbolIds),"Carry the delivered row before its atomic arrival transform");
                }
                Assert.That(run.Version,Is.EqualTo(2));
                Assert.That(run.Level(source,0).Floors[0].Repair,Is.Not.Null);
                Assert.That(run.Level(source,1).Floors[0].Bomb,Is.Not.Null);
                Assert.That(run.Level(source,1).Floors[1].OptionalStamp,Is.Not.Null);
                var final=run.Level(source,2);for(int f=0;f<source.Floors.Count;f++) {
                    Assert.That(final.Floors[f].Bomb,Is.SameAs(source.Floors[f].Bomb));Assert.That(final.Floors[f].Key,Is.SameAs(source.Floors[f].Key));
                    Assert.That(final.Floors[f].Repair,Is.SameAs(source.Floors[f].Repair));Assert.That(final.Floors[f].Powerup,Is.SameAs(source.Floors[f].Powerup));
                }
            }
            string dir=Path.Combine(ReferenceFixtures.PackageRoot,"Reports","OutcomeMotion");Directory.CreateDirectory(dir);File.WriteAllLines(Path.Combine(dir,"curves.csv"),lines);
            Assert.That(proven,Is.EqualTo(content.Count()));
        }
        [Test] public void VersionOneRecipesKeepTheirExactDeviceFreeStagesAndOldHashFormula()
        {
            var source=catalog.GetPrimary("C81").Definition;var old=DescentRun.Build(source,version:1);
            Assert.That(old.Version,Is.EqualTo(1));Assert.That(old.Level(source,0).Floors[0].Repair,Is.Null);
            Assert.That(old.Level(source,1).Floors.All(f=>f.Bomb==null&&f.OptionalStamp==null),Is.True);
            var first=Solve(old,source);old.AcceptCheckpoint(source,first);old.Validate(source,"en");
            var current=new AttemptService(old.Level(source,1),direction:old.Direction);current.Execute(Command(old.Stages[1].Witness[0]));
            var profile=new PlayerProfile{Descent=old,Attempt=AttemptRecord.Capture(current)};
            Assert.That(profile.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(current.CurrentStateHash));
            var modern=DescentRun.Build(source);Assert.That(modern.ContentHash,Is.Not.EqualTo(old.ContentHash));
            old.Version=2;Assert.Throws<ArgumentException>(()=>profile.RestoreAttempt(catalog));
        }
        [Test] public void CheckpointSaveRestoresExactlyAndRejectsAChangedFutureRecipe()
        {
            var source=catalog.GetPrimary("C81").Definition;var run=DescentRun.Build(source,DeliveryMood.Wind);
            var first=Solve(run,source);run.AcceptCheckpoint(source,first);
            var current=new AttemptService(run.Level(source,1),direction:run.Direction);current.Execute(Command(run.Stages[1].Witness[0]));
            var profile=new PlayerProfile{Descent=run,Attempt=AttemptRecord.Capture(current)};
            string dir=Path.Combine(Path.GetTempPath(),"CascadeDescent",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
            try {
                var store=new ProfileStore(Path.Combine(dir,"profile.json"));store.Save(profile);var saved=store.Load().Profile;
                var restored=saved.RestoreAttempt(catalog);Assert.That(restored.CurrentStateHash,Is.EqualTo(current.CurrentStateHash));
                Assert.That(saved.Descent.StageIndex,Is.EqualTo(1));Assert.That(saved.Clears,Is.Empty);
                saved.Descent.Stages[2].Wood[0]++;Assert.Throws<ArgumentException>(()=>saved.RestoreAttempt(catalog));
            }finally{Directory.Delete(dir,true);}
        }
        [Test] public void OnlyTheThirdDeliveryClearsAndRecordsUseWholeDescentPerformance()
        {
            var source=catalog.GetPrimary("C81").Definition;var run=DescentRun.Build(source);var profile=new PlayerProfile{Descent=run};
            var first=new AttemptService(run.Level(source,0));
            var hint=AttemptService.CalculateHint(first.CaptureHintRequest());Assert.That(first.RevealHint(hint),Is.True);
            first.Execute(Command(run.Stages[0].Witness[0]));Assert.That(first.Undo(),Is.True);
            foreach(var c in run.Stages[0].Witness)first.Execute(Command(c));
            Assert.That(profile.RecordClear(first),Is.False);run.AcceptCheckpoint(source,first);
            var second=Solve(run,source);Assert.That(profile.RecordClear(second),Is.False);run.AcceptCheckpoint(source,second);
            var final=Solve(run,source);Assert.That(profile.RecordClear(final),Is.True);Assert.That(profile.RecordClear(final),Is.False);
            var performance=run.Performance(final);Assert.That(performance.Route,Is.EqualTo(run.Stages.Sum(s=>s.Minimum)));
            Assert.That(performance.HintUsed,Is.True);Assert.That(performance.Undos,Is.EqualTo(1));
            profile.Rewards.RecordDelivery(final,performance);var record=profile.Rewards.Records.Single();
            Assert.That(record.BestRoute,Is.EqualTo(performance.Route));Assert.That(record.ContentHash,Is.EqualTo(run.ContentHash));
            Assert.That(record.StageCount,Is.EqualTo(3));Assert.That(record.SourceHash,Is.EqualTo(source.ContentHash));
            Assert.That(record.BestWithoutHint,Is.Null);Assert.That(record.BestClean,Is.Null);
            Assert.That(profile.Clears.Single().ContentHash,Is.EqualTo(run.ContentHash));
            var memory=new DirectorMemory();memory.ObserveTerminal(final,final.RouteLength,performance);Assert.That(memory.CleanWins,Is.Zero);
        }
        [Test] public void AdvanceRequiresDeliveryAndRetryKeepsTheCheckpointAndConditions()
        {
            var source=catalog.GetPrimary("C81").Definition;var run=DescentRun.Build(source);
            var unfinished=new AttemptService(run.Level(source,0));Assert.Throws<InvalidOperationException>(()=>run.AcceptCheckpoint(source,unfinished));
            run.AcceptCheckpoint(source,Solve(run,source));var second=new AttemptService(run.Level(source,1));
            string hash=second.PinnedContentHash;second.Execute(PuzzleCommand.Drop);second.Execute(PuzzleCommand.Drop);
            Assert.That(second.Current.Status,Is.EqualTo(PuzzleStatus.Lost));second.Retry();
            Assert.That(run.StageIndex,Is.EqualTo(1));Assert.That(second.PinnedContentHash,Is.EqualTo(hash));Assert.That(second.RouteLength,Is.Zero);
            run.Validate(source,"en");
        }
        [Test] public void EarlyLessonsUseStagesUnprovenPlansFallBackAndLegacySingleSavesRestore()
        {
            Assert.That(DescentRun.Build(catalog.GetPrimary("C13").Definition),Is.Not.Null);
            Assert.That(DescentRun.Build(catalog.GetPrimary("C81").Definition,budget:1),Is.Null);
            var single=new AttemptService(catalog.GetPrimary("C81").Definition);single.Execute(PuzzleCommand.Swap(0));
            var profile=new PlayerProfile{Attempt=AttemptRecord.Capture(single)};
            Assert.That(profile.Descent,Is.Null);Assert.That(profile.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(single.CurrentStateHash));
        }
    }
}
