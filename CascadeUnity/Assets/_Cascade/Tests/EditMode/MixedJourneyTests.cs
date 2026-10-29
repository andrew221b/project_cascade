using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using NUnit.Framework;
namespace Cascade.Tests
{
    public sealed class MixedJourneyTests
    {
        static string Read(string name)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",name+".json"));
        static ContentCatalog Catalog()=>ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
        [TestCase("en")][TestCase("ru")][TestCase("uk")]
        public void ShortRowsAlternateAndEveryLongRowContainsWordsAndShapes(string locale)
        {
            var catalog=Catalog();var route=MixedJourney.Sequence(catalog,locale);
            Assert.That(route.Select(c=>c.Definition.CampaignNodeId),Is.EqualTo(catalog.PrimaryLevels.Select(c=>c.Definition.CampaignNodeId)));
            int shortRows=0;
            foreach(var level in route){
                Assert.That(LevelValidator.Validate(level.Definition,catalog.Words),Is.Empty);
                Assert.That(level.Mode,Is.EqualTo(level.Definition.TargetSymbolIds.Count==3?(shortRows++%2==0?PuzzleMode.Shapes:PuzzleMode.Words):PuzzleMode.Mixed),level.Definition.PuzzleId);
            }
            Assert.That(route.Take(3).Select(c=>c.Mode),Is.EqualTo(new[]{PuzzleMode.Shapes,PuzzleMode.Words,PuzzleMode.Mixed}));
        }
        [TestCase("en")][TestCase("ru")][TestCase("uk")]
        public void AllNewVariantsReplayPublishedCertificatesAndKeepHistoricalCheckpoints(string locale)
        {
            var catalog=Catalog();var store=new PublishedTriadStore(Read("production_triad_recipes"),Read("production_triad_manifest"));
            foreach(var source in catalog.MixedLevels.Where(c=>c.Locale==locale)){
                var run=store.Bind(source.Definition,locale,catalog.ContentHash);Assert.That(run,Is.Not.Null,source.Definition.PuzzleId);
                var primary=catalog.GetPrimary(source.Definition.CampaignNodeId);
                Assert.That(run.Stages.Select(s=>s.Minimum),Is.EqualTo(store.Bind(primary.Definition,primary.Locale,catalog.ContentHash).Stages.Select(s=>s.Minimum)));
            }
            foreach(var source in new[]{catalog.MixedLevels.First(c=>c.Locale==locale),catalog.GetPrimary("C03"),catalog.LocalizedLevels.First(c=>c.Locale==locale&&c.Definition.CampaignNodeId=="C03")}){
                var run=store.Bind(source.Definition,source.Locale,catalog.ContentHash);var first=new AttemptService(run.Level(source.Definition,0),source.Locale);
                foreach(string command in run.Stages[0].Witness){var action=command=="D"?PuzzleCommand.Drop:command.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(command.Substring(1).Split(':')[0]),int.Parse(command.Split(':')[1])):PuzzleCommand.Swap(int.Parse(command));Assert.That(first.Execute(action).Accepted,Is.True);}
                run.AcceptCheckpoint(source.Definition,first);var second=new AttemptService(run.Level(source.Definition,1),source.Locale);
                var profile=CloudJson.Clone(new PlayerProfile{PreferredMode="Mixed",Descent=run,Attempt=AttemptRecord.Capture(second)});
                Assert.That(profile.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(second.CurrentStateHash));Assert.That(profile.Descent.StageIndex,Is.EqualTo(1));
            }
        }
    }
}
