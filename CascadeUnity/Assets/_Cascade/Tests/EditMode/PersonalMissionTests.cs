using System;
using System.IO;
using System.Linq;
using System.Threading;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class PersonalMissionTests
    {
        ContentCatalog catalog;
        PublishedTriadStore published;
        [OneTimeSetUp] public void Load(){
            string Read(string f)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",f));
            catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
            published=new PublishedTriadStore(Read("production_triad_recipes.json"),Read("production_triad_manifest.json"));
        }
        static PuzzleCommand Parse(string c)=>c=="D"?PuzzleCommand.Drop:c.StartsWith("R")?PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c));
        [Test] public void ChallengeRespondsToConfidenceAndRecoveryWithoutInventoryInputs()
        {
            Assert.That(PersonalMissionMemory.ChallengeFor(null),Is.EqualTo(1));
            var director=new DirectorMemory{CleanWins=2};
            Assert.That(PersonalMissionMemory.ChallengeFor(director),Is.EqualTo(2));
            director.Struggle=2;
            Assert.That(PersonalMissionMemory.ChallengeFor(director),Is.Zero);
            director.Struggle=0;director.CleanWins=0;
            Assert.That(PersonalMissionMemory.ChallengeFor(director),Is.EqualTo(1));
        }
        [TestCase("C251")][TestCase("C500")]
        public void ConfidentMissionUsesPublishedDifficultyAndIsSolvableWithEmptyBag(string node)
        {
            var source=catalog.GetPrimary(node).Definition;var baseline=published.Bind(source,"en",catalog.ContentHash);
            using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var run=CampaignTriads.BuildPersonalMission(source,1,null,stop.Token,reference:baseline,challenge:2);
            Assert.That(run,Is.Not.Null);run.Validate(source,"en");
            int minimum=baseline.Stages.Sum(s=>s.Minimum);
            Assert.That(run.Stages.Sum(s=>s.Minimum)-minimum,Is.InRange(2,Math.Max(4,minimum/3)));
            for(int stage=0;stage<3;stage++){
                Assert.That(run.Stages[stage].Triad.Lesson,Is.EqualTo(baseline.Stages[0].Triad.Lesson));
                var attempt=new AttemptService(run.Level(source,stage));
                foreach(var command in run.Stages[stage].Witness.Select(Parse))Assert.That(attempt.Execute(command).Accepted,Is.True);
                Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
                Assert.That(attempt.ToolsUsed,Is.Zero);Assert.That(attempt.HintUsed,Is.False);
            }
        }
        [TestCase("C03")][TestCase("C06")][TestCase("C07")][TestCase("C11")][TestCase("C14")][TestCase("C37")]
        [TestCase("C65")][TestCase("C120")][TestCase("C360")][TestCase("C500")]
        public void MissionsAreHarderWithinABoundedBandAndEveryStageReplays(string node)
        {
            var source=catalog.GetPrimary(node).Definition;var baseline=published.Bind(source,"en",catalog.ContentHash);
            using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var run=CampaignTriads.BuildPersonalMission(source,1,null,stop.Token,reference:baseline);
            Assert.That(run,Is.Not.Null,node);
            int minimum=baseline.Stages.Sum(s=>s.Minimum),extra=run.Stages.Sum(s=>s.Minimum)-minimum;
            Assert.That(extra,Is.InRange(1,Math.Max(4,minimum/3)));
            Assert.That(run.Stages[0].Minimum,Is.GreaterThan(baseline.Stages[0].Minimum));
            Assert.That(run.Stages.Skip(1).Select((s,i)=>s.Minimum>=baseline.Stages[i+1].Minimum).All(x=>x),Is.True);
            run.Validate(source,"en");
            for(int stage=0;stage<3;stage++){
                var level=run.Level(source,stage);var attempt=new AttemptService(level);
                Assert.That(CampaignTriads.UsesPrinciple(level,run.Stages[stage].Witness.Select(Parse),(MechanicLesson)run.Stages[stage].Triad.Lesson),Is.True);
                foreach(var command in run.Stages[stage].Witness.Select(Parse))Assert.That(attempt.Execute(command).Accepted,Is.True);
                Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
                Assert.That(attempt.Current.Row,Is.EqualTo(source.TargetSymbolIds));
            }
            if(node=="C03"||node=="C500"){
                var other=CampaignTriads.BuildPersonalMission(source,2,run.ContentHash,stop.Token,reference:baseline);
                Assert.That(other,Is.Not.Null);Assert.That(other.ContentHash,Is.Not.EqualTo(run.ContentHash));
            }
        }
        [Test] public void AcceptedMissionAndCampaignCheckpointSurviveSaveAndModeSwitch()
        {
            var source=catalog.GetPrimary("C03").Definition;var run=CampaignTriads.BuildPersonalMission(source,1,null,CancellationToken.None);
            var campaignSource=catalog.GetPrimary("C02").Definition;var campaign=DescentRun.Build(campaignSource,version:6);
            var original=new AttemptService(campaign.Level(campaignSource,0));
            original.Execute(Parse(campaign.Stages[0].Witness[0]));
            var mission=new AttemptService(run.Level(source,0));
            var profile=new PlayerProfile{Attempt=AttemptRecord.Capture(mission),Descent=run};
            profile.Missions.Present(new PersonalMissionOffer{Serial=1,Hero=0,Locale="en",SourceHash=source.ContentHash,Run=run},3);
            profile.Missions.Active=true;profile.Missions.CampaignAttempt=AttemptRecord.Capture(original);profile.Missions.CampaignDescent=campaign;profile.Missions.CampaignJourney=true;
            string directory=Path.Combine(Path.GetTempPath(),"CascadePersonalMission",Guid.NewGuid().ToString("N"));
            try{
                var store=new ProfileStore(Path.Combine(directory,"profile.json"));store.Save(profile);var load=store.Load();
                Assert.That(load.Recovery,Is.EqualTo(SaveRecovery.None));var saved=load.Profile;
                Assert.That(saved.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(mission.CurrentStateHash));
                saved.SelectJourney("Words");Assert.That(saved.Missions.Active,Is.False);saved.SelectJourney("Shapes");
                Assert.That(saved.Missions.Active,Is.True);
                var backup=new PlayerProfile{Attempt=saved.Missions.CampaignAttempt,Descent=saved.Missions.CampaignDescent};
                Assert.That(backup.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(original.CurrentStateHash));
                Assert.That(saved.Clears,Is.Empty);Assert.That(saved.Rewards.SeenClearNodes,Is.Empty);
            }finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test] public void DeclineUsesProgressIntervalAndCancellationNeverReturnsAnOffer()
        {
            var memory=new PersonalMissionMemory();Assert.That(memory.Due(2),Is.False);Assert.That(memory.Due(3),Is.True);
            var source=catalog.GetPrimary("C03").Definition;var run=DescentRun.Build(source,version:6);
            memory.Present(new PersonalMissionOffer{Serial=1,Hero=0,Locale="en",SourceHash=source.ContentHash,Run=run},3);
            memory.Decline();Assert.That(memory.Due(3),Is.False);Assert.That(memory.Due(6),Is.False);Assert.That(memory.Due(7),Is.True);
            using var stop=new CancellationTokenSource();stop.Cancel();
            Assert.Throws<OperationCanceledException>(()=>CampaignTriads.BuildPersonalMission(source,2,null,stop.Token,reference:run));
        }
    }
}
