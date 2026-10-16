using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Solver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class StarObservatoryLevelTests
    {
        ContentCatalog catalog;
        JArray balance;
        static string Read(string file)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",file));
        public static IEnumerable<string> Cases=>Enumerable.Range(161,24).Select(node=>"C"+node);
        [OneTimeSetUp] public void Setup()
        {
            catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
            balance=JArray.Parse(File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Reports/NightLevels/python_balance.json")));
        }
        [Test] public void PublishedAppendPreservesAll637OldPuzzlePinsAndHasDistinctLayouts()
        {
            var previous=catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).Where(content=>int.Parse(content.Definition.CampaignNodeId.Substring(1))<=160).ToArray();
            Assert.That(previous.Length,Is.EqualTo(637));
            string pins=string.Join("\n",previous.OrderBy(content=>content.Definition.PuzzleId,StringComparer.Ordinal)
                .Select(content=>content.Definition.PuzzleId+":"+content.Definition.ContentHash));
            Assert.That(ContentImporter.Hash(pins),Is.EqualTo("227d7ca0bc11c4f1ac6deb19df10ff4d7d33f904ccadb2ce874e920b2ccfd33c"));
            Assert.That(catalog.PrimaryLevels.Count,Is.EqualTo(540));Assert.That(catalog.LocalizedLevels.Count,Is.EqualTo(1617));
            Assert.That(catalog.PrimaryLevels.Where(c=>int.Parse(c.Definition.CampaignNodeId.Substring(1))>=161&&int.Parse(c.Definition.CampaignNodeId.Substring(1))<=184)
                .Select(content=>content.Definition.PuzzleId),Is.EqualTo(Cases));
            Assert.That(balance.Select(row=>(string)row["id"]),Is.EqualTo(Cases));
            Assert.That(balance.Select(row=>(string)row["structure_sha256"]).Distinct().Count(),Is.EqualTo(24));
            // All new Shapes rows use the same ordered alphabet, so layout comparison is literal.
            var sources=JArray.Parse(Read("levels.json")).Concat((JArray)JObject.Parse(Read("campaign_extension.json"))["levels"]).ToArray();
            string Layout(JToken source)=>new JObject { ["start"]=source["start"].DeepClone(),["floors"]=source["floors"].DeepClone() }.ToString(Formatting.None);
            var older=new HashSet<string>(sources.Take(160).Select(Layout),StringComparer.Ordinal);
            var added=sources.Skip(160).Take(24).Select(Layout).ToArray();
            Assert.That(added.Distinct().Count(),Is.EqualTo(24));Assert.That(added.Any(older.Contains),Is.False);
            Assert.That(sources.Skip(160).Take(24).All(source=>((JArray)source["floors"]).Count==3),Is.True);
        }
        [TestCaseSource(nameof(Cases))] public void EachObservatorySourceAndAllWordBindingsHaveThePublishedMinimum(string node)
        {
            var expected=balance.Single(row=>(string)row["id"]==node);
            var source=catalog.GetPrimary(node);int minimum=(int)expected["optimal_actions"];
            Assert.That(minimum,Is.InRange(8,13));
            var primary=new AttemptService(source.Definition);var result=PuzzleSolver.Analyze(primary.Resolver);
            Assert.That(result.Status,Is.EqualTo(SearchStatus.Solved));Assert.That(result.OptimalActions,Is.EqualTo(minimum));
            Assert.That(result.Metrics.States,Is.EqualTo((int)expected["states"]));
            Assert.That(result.Metrics.Transitions,Is.EqualTo((int)expected["transitions"]));
            Assert.That(result.Metrics.DeadStates,Is.EqualTo((int)expected["dead_states"]));
            Assert.That(result.Metrics.NearOptimalFirstStatesPlusTwo,Is.GreaterThanOrEqualTo(2));
            Assert.That(result.Path.Select(command=>command.ToString()),Is.EqualTo(expected["path"].Select(command=>command.ToString())));
            Replay(primary,result.Path);
            foreach(string locale in new[]{"en","ru","uk"}) {
                Assert.That(catalog.TryGetLocalizedBinding(node,locale,out var content),Is.True);
                Assert.That(content.Mode,Is.EqualTo(PuzzleMode.Words));
                Assert.That(LevelValidator.Validate(content.Definition,catalog.Words),Is.Empty);
                var attempt=new AttemptService(content.Definition,locale);var proof=PuzzleSolver.FindShortest(attempt.Resolver);
                Assert.That(proof.Status,Is.EqualTo(SearchStatus.Solved));Assert.That(proof.OptimalActions,Is.EqualTo(minimum));
                Replay(attempt,proof.Path);
                var restored=AttemptService.Restore(attempt.Level,AttemptRecord.Capture(attempt));
                Assert.That(restored.CurrentStateHash,Is.EqualTo(attempt.CurrentStateHash));
            }
        }
        [TestCaseSource(nameof(Cases))] public void EachObservatoryDescentProvesThreeStagesIncludingTheExistingLiveGust(string node)
        {
            var source=catalog.GetPrimary(node).Definition;
            Assert.That(DeliveryDirector.CanDirect(source),Is.True);Assert.That(DescentRun.CanDescend(source),Is.True);
            var run=DescentRun.Build(source,version:5);Assert.That(run,Is.Not.Null,node);run.Validate(source,"en");
            Assert.That(run.Stages[0].Minimum,Is.EqualTo(2));Assert.That(run.Stages[1].Minimum,Is.GreaterThan(2));
            Assert.That(run.Stages[2].Minimum,Is.GreaterThanOrEqualTo(run.Stages[1].Minimum));
            Assert.That(run.Level(source,1).Floors[0].GustAfterSwaps,Is.EqualTo(1));
            Assert.That(run.Level(source,1).Floors[0].GustDamage,Is.EqualTo(1));
            for(int stage=0;stage<3;stage++) {
                var level=run.Level(source,stage);var attempt=new AttemptService(level,direction:run.Direction);
                var proof=PuzzleSolver.FindShortest(attempt.Resolver);
                Assert.That(proof.Status,Is.EqualTo(SearchStatus.Solved));Assert.That(proof.OptimalActions,Is.EqualTo(run.Stages[stage].Minimum));
                Assert.That(level.TargetSymbolIds,Is.EqualTo(source.TargetSymbolIds));
                Replay(attempt,proof.Path);
                if(stage!=2)run.AcceptCheckpoint(source,attempt);
            }
        }
        static void Replay(AttemptService attempt,IEnumerable<PuzzleCommand> path)
        {
            foreach(var command in path) {
                var predicted=attempt.Predict(command);var actual=attempt.Execute(command);
                Assert.That(actual.Accepted,Is.True);Assert.That(actual.State,Is.EqualTo(predicted.State));
                Assert.That(LevelValidator.SameMultiset(actual.State.Row,attempt.Level.TargetSymbolIds),Is.True);
            }
            Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
        }
    }
}
