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
    public sealed class CompletionProgressTests
    {
        ContentCatalog catalog;
        [OneTimeSetUp] public void Setup(){string Read(string file)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",file));
            catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));}
        static PuzzleCommand Decode(string c)=>c=="D"?PuzzleCommand.Drop:c.StartsWith("R",StringComparison.Ordinal)?
            PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c));
        static AttemptService Deliver(DescentRun run,LevelDefinition source,string locale="en")
        {var a=new AttemptService(run.Level(source,run.StageIndex),locale,direction:run.Direction);foreach(string c in run.Stages[run.StageIndex].Witness)Assert.That(a.Execute(Decode(c)).Accepted,Is.True);return a;}
        [Test]public void IndependentPersonalBestsRetainTheirOwnEvidenceUntilOneRunDominates()
        {
            var source=catalog.GetPrimary("C01").Definition;
            string Read(string name)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",name+".json"));
            var store=new PublishedTriadStore(Read("production_triad_recipes"),Read("production_triad_manifest"));var profile=new PlayerProfile();
            string Record(int stars,int score){
                var run=store.Bind(source,"en",catalog.ContentHash);AttemptService final=null;
                for(int stage=0;stage<3;stage++){final=Deliver(run,source);if(stage<2)run.AcceptCheckpoint(source,final);}
                profile.Descent=run;profile.RecordClear(final,new LevelCompletion{Stars=stars,Score=score});
                profile.Rewards.Synchronize(profile.Clears.Select(c=>c.CampaignNodeId));return final.AttemptId.ToString("D");
            }
            string first=Record(4,1800),second=Record(3,1900);var receipt=profile.Clears.Single();
            Assert.That(receipt.AttemptId,Is.EqualTo(first));Assert.That(receipt.ScoreReplay.Stages[2].AttemptId,Is.EqualTo(second));
            Assert.That(receipt.BestStars,Is.EqualTo(4));Assert.That(receipt.BestScore,Is.EqualTo(1900));
            string third=Record(5,1750);Assert.That(receipt.AttemptId,Is.EqualTo(third));Assert.That(receipt.ScoreReplay.Stages[2].AttemptId,Is.EqualTo(second));
            Record(5,2000);Assert.That(receipt.ScoreReplay,Is.Null);Assert.That(receipt.BestScore,Is.EqualTo(2000));profile.Validate();
        }
        [Test] public void LegacyVersionThreeRepresentativeBindingsKeepThreeReplayableStages()
        {
            var lines=new List<string>{"puzzle,locale,first,second,final"};var failed=new List<string>();
            foreach(var content in catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).Where(c=>new[]{"C01","C02","C06","C08","C27","C41","C81","C121","C160","C180","C540"}.Contains(c.Definition.CampaignNodeId))){
                var run=DescentRun.Build(content.Definition,version:3);if(run==null){failed.Add(content.Definition.PuzzleId);continue;}
                run.Validate(content.Definition,content.Locale);lines.Add(content.Definition.PuzzleId+","+content.Locale+","+string.Join(",",run.Stages.Select(s=>s.Minimum)));
                for(int i=0;i<3;i++){
                    var level=run.Level(content.Definition,i);LevelValidator.ThrowIfInvalid(level);
                    Assert.That(LevelValidator.SameMultiset(level.InitialSymbolIds,content.Definition.TargetSymbolIds),Is.True);
                    var a=Deliver(run,content.Definition,content.Locale);Assert.That(a.Current.Status,Is.EqualTo(PuzzleStatus.Won));
                    if(i<2)run.AcceptCheckpoint(content.Definition,a);
                }
            }
            string dir=Path.Combine(ReferenceFixtures.PackageRoot,"Reports","CompletionProgress");Directory.CreateDirectory(dir);File.WriteAllLines(Path.Combine(dir,"curves.csv"),lines);
            Assert.That(failed,Is.Empty,"Fresh levels may never silently fall back to one stage: "+string.Join(",",failed));
        }
        [Test] public void GatedFinalesKeepTheirPublishedTargetsAndDevicesAndProveTheSameTwelveFailingBindings()
        {
            var report=Path.Combine(ReferenceFixtures.PackageRoot,"Reports","CompletionProgress","curves.csv");var lines=new List<string>();
            foreach(var content in catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).Where(c=>new[]{"C06","C08","C27"}.Contains(c.Definition.CampaignNodeId))){
                var run=DescentRun.Build(content.Definition,version:3);Assert.That(run,Is.Not.Null,content.Definition.PuzzleId);run.Validate(content.Definition,content.Locale);
                Assert.That(run.Stages[2].PublishedStart,Is.True);var final=run.Level(content.Definition,2);
                Assert.That(final.TargetSymbolIds,Is.EqualTo(content.Definition.TargetSymbolIds));Assert.That(final.InitialSymbolIds,Is.EqualTo(content.Definition.InitialSymbolIds));
                for(int i=0;i<3;i++){var a=Deliver(run,content.Definition,content.Locale);Assert.That(a.Current.Status,Is.EqualTo(PuzzleStatus.Won));if(i<2)run.AcceptCheckpoint(content.Definition,a);}
                lines.Add(content.Definition.PuzzleId+","+content.Locale+","+string.Join(",",run.Stages.Select(s=>s.Minimum)));
            }
            Assert.That(lines.Count,Is.EqualTo(12));File.AppendAllLines(report,lines);
        }
        [Test] public void AStampFromAnIntermediateStageSurvivesCheckpointSaveAndAddsItsFullLevelBonus()
        {
            var source=catalog.GetPrimary("C81").Definition;var run=DescentRun.Build(source,version:3);run.AcceptCheckpoint(source,Deliver(run,source));
            var second=Deliver(run,source);run.AcceptCheckpoint(source,second);run.Validate(source,"en");
            int flags=(run.CompletedToolFlags??0)|JourneyRewards.ToolFlags(second);Assert.That(run.Performance(Deliver(run,source)).ToolFlags&flags,Is.EqualTo(flags));
            var profile=new PlayerProfile{Descent=run,Attempt=AttemptRecord.Capture(Deliver(run,source))};var restored=profile.RestoreAttempt(catalog);
            Assert.That(LevelCompletion.From(restored,null,run.Performance(restored)).IsValid(),Is.True);
        }
        [Test] public void AccessOpensOnlyTheNextNodeAndExplicitReplaysWithoutErasingOlderOutOfOrderClears()
        {
            var order=new[]{"C01","C02","C03","C04"};var clears=new List<string>{"C03"};
            Assert.That(CampaignProgress.IsOpen(order,clears,"C01"),Is.True);Assert.That(CampaignProgress.IsOpen(order,clears,"C03"),Is.True);
            Assert.That(CampaignProgress.IsOpen(order,clears,"C02"),Is.False);Assert.That(CampaignProgress.IsOpen(order,clears,"C04"),Is.False);
            Assert.That(CampaignProgress.Remaining(order,clears),Is.EqualTo(3));clears.Add("C01");Assert.That(CampaignProgress.Next(order,clears),Is.EqualTo("C02"));
            clears.Add("C02");Assert.That(CampaignProgress.Next(order,clears),Is.EqualTo("C04"));
        }
        [Test] public void InsertedLevelsBeforeAnExistingClearStayReplayableButDoNotResetContinue()
        {
            var order=new[]{"C01","C185","C02","C186","C03"};var inserted=new[]{"C185","C186"};
            var existing=new[]{"C01","C02"};
            Assert.That(CampaignProgress.Next(order,existing,inserted),Is.EqualTo("C186"));
            Assert.That(CampaignProgress.IsOpen(order,existing,"C185",inserted),Is.True);
            Assert.That(CampaignProgress.IsOpen(order,existing,"C03",inserted),Is.False);
            var fresh=new[]{"C01"};Assert.That(CampaignProgress.Next(order,fresh,inserted),Is.EqualTo("C185"));
        }
        [Test] public void WholeRunRatingResourcesAndCheckpointRestoreAreDurableAndReplayCannotFarmLights()
        {
            var source=catalog.GetPrimary("C01").Definition;var run=DescentRun.Build(source,version:3);var profile=new PlayerProfile{Descent=run};
            var first=Deliver(run,source);Assert.That(profile.RecordClear(first),Is.False);run.AcceptCheckpoint(source,first);
            var mid=new AttemptService(run.Level(source,1));profile.Attempt=AttemptRecord.Capture(mid);
            Assert.That(profile.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(mid.CurrentStateHash));
            var second=Deliver(run,source);Assert.That(profile.RecordClear(second),Is.False);run.AcceptCheckpoint(source,second);
            var final=Deliver(run,source);var result=LevelCompletion.From(final,null,run.Performance(final));
            Assert.That(result.Stages,Is.EqualTo(3));Assert.That(result.Stars,Is.EqualTo(3));Assert.That(result.Moves,Is.EqualTo(run.Stages.Sum(s=>s.Minimum)));
            Assert.That(profile.RecordClear(final,result),Is.True);profile.Rewards.Synchronize(profile.Clears.Select(c=>c.CampaignNodeId));
            Assert.That(profile.RecordClear(final,result),Is.False);profile.Rewards.Synchronize(profile.Clears.Select(c=>c.CampaignNodeId));
            Assert.That(profile.Clears.Single().BestStars,Is.EqualTo(3));Assert.That(profile.Rewards.EarnedCoins(new[]{"C01"}),Is.EqualTo(40));
            result.Coins=40;result.Lights=1;profile.Completion=result;profile.Attempt=AttemptRecord.Capture(final);
            var folder=Path.Combine(Path.GetTempPath(),"CascadeCompletion",Guid.NewGuid().ToString("N"));
            try{var store=new ProfileStore(Path.Combine(folder,"save.json"));store.Save(profile);var saved=store.Load().Profile;
                Assert.That(saved.Completion.Score,Is.EqualTo(result.Score));Assert.That(saved.Completion.Acknowledged,Is.False);
                Assert.That(saved.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(final.CurrentStateHash));
            }finally{if(Directory.Exists(folder))Directory.Delete(folder,true);}
        }
        [Test] public void StarsRespondToHintRouteAndExplosionAndOldVersionTwoPlansStillRestore()
        {
            var source=catalog.GetPrimary("C81").Definition;var old=DescentRun.Build(source,version:2);old.AcceptCheckpoint(source,Deliver(old,source));
            var profile=new PlayerProfile{Descent=old,Attempt=AttemptRecord.Capture(new AttemptService(old.Level(source,1),direction:old.Direction))};
            Assert.That(profile.RestoreAttempt(catalog),Is.Not.Null);Assert.That(profile.Completion,Is.Null);
            var intro=catalog.GetPrimary("C01").Definition;var run=DescentRun.Build(intro,version:3);
            run.AcceptCheckpoint(intro,Deliver(run,intro));run.AcceptCheckpoint(intro,Deliver(run,intro));var final=Deliver(run,intro);
            var performance=run.Performance(final);Assert.That(LevelCompletion.From(final,null,performance).Stars,Is.EqualTo(3));
            performance.HintUsed=true;Assert.That(LevelCompletion.From(final,null,performance).Stars,Is.EqualTo(1));
            performance.Exploded=1;Assert.That(LevelCompletion.From(final,null,performance).Stars,Is.EqualTo(1));
            performance.Exploded=0;performance.MovesMade+=20;Assert.That(LevelCompletion.From(final,null,performance).Score,Is.LessThan(1800));
        }
    }
}
