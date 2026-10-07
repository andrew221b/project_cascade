using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Solver;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class CampaignExtensionTests
    {
        ContentCatalog catalog;JArray balance;
        static string Read(string file)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",file));
        static string Extension=>Read("campaign_extension.json");
        public static IEnumerable<string> Cases=>((JArray)JObject.Parse(Extension)["levels"]).Select(l=>(string)l["id"]).Where(id=>int.Parse(id.Substring(1))<=160);
        [OneTimeSetUp] public void Setup()
        {
            catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Extension);
            balance=JArray.Parse(File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Reports/P20/python_campaign_balance.json")));
        }
        [Test] public void RuntimeExtensionPreservesOriginalPinsAndHasExactly184Nodes()
        {
            var original=ContentTests.LoadCatalog();Assert.That(catalog.PrimaryLevels.Count,Is.EqualTo(184));Assert.That(catalog.LocalizedLevels.Count,Is.EqualTo(549));
            foreach(var content in original.PrimaryLevels.Concat(original.LocalizedLevels)) {
                var current=catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).Single(c=>c.Definition.PuzzleId==content.Definition.PuzzleId);
                Assert.That(current.Definition.ContentHash,Is.EqualTo(content.Definition.ContentHash));
            }
            Assert.That(catalog.PrimaryLevels.Select(c=>c.Definition.CampaignNodeId),Is.EqualTo(Enumerable.Range(1,184).Select(i=>"C"+i.ToString("00"))));
            Assert.That(balance.Select(b=>(string)b["structure_sha256"]).Distinct().Count(),Is.EqualTo(120));
            Assert.That(File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath,"_Cascade/Content/Resources/CascadeContent/campaign_extension.json")),Is.EqualTo(Extension));
            Assert.That(UnityEngine.Resources.Load<UnityEngine.TextAsset>("CascadeContent/campaign_extension").text,Is.EqualTo(Extension));
        }
        [TestCaseSource(nameof(Cases))] public void EachRouteHasMatchingCompletedGraphAndReplayableOptionalGoal(string id)
        {
            var content=catalog.GetPrimary(id);var expected=balance.Single(b=>(string)b["id"]==id);var attempt=new AttemptService(content.Definition);
            var result=PuzzleSolver.Analyze(attempt.Resolver);Assert.That(result.Status,Is.EqualTo(SearchStatus.Solved));
            Assert.That(result.OptimalActions,Is.EqualTo((int)expected["optimal_actions"]));Assert.That(result.OptimalActions,Is.GreaterThanOrEqualTo(5));
            Assert.That(result.Metrics.States,Is.EqualTo((int)expected["states"]));Assert.That(result.Metrics.Transitions,Is.EqualTo((int)expected["transitions"]));
            Assert.That(result.Metrics.DeadStates,Is.EqualTo((int)expected["dead_states"]));
            Assert.That(result.Metrics.NearOptimalFirstStatesPlusTwo,Is.EqualTo((int)expected["near_optimal_first_states_plus2"]));
            Assert.That(result.Metrics.ForcedRatio,Is.EqualTo((double)expected["forced_ratio"]).Within(.00001));
            Assert.That(result.Path.Select(c=>c.ToString()),Is.EqualTo(expected["path"].Select(c=>c.ToString())));
            foreach(var command in result.Path){Assert.That(attempt.Execute(command).Accepted,Is.True);Assert.That(LevelValidator.SameMultiset(attempt.Current.Row,attempt.Level.TargetSymbolIds),Is.True);}
            Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
            var restored=AttemptService.Restore(attempt.Level,AttemptRecord.Capture(attempt));Assert.That(restored.CurrentStateHash,Is.EqualTo(attempt.CurrentStateHash));
            if(expected["stamp_objective"].Type!=JTokenType.Null) {
                var objective=PuzzleSolver.FindShortest(attempt.Resolver,goal:new PuzzleGoal(1));
                Assert.That(objective.OptimalActions,Is.EqualTo((int)expected["stamp_objective"]["optimal_actions"]));
                var stamped=new AttemptService(attempt.Level,goal:new PuzzleGoal(1));foreach(var c in objective.Path)stamped.Execute(c);
                Assert.That(stamped.Goal.IsSatisfied(stamped.Current),Is.True);Assert.That(objective.OptimalActions,Is.GreaterThan(result.OptimalActions));
            }
            foreach(string locale in new[]{"en","ru","uk"}) {
                Assert.That(catalog.TryGetLocalizedBinding(id,locale,out var word),Is.True);Assert.That(word.Mode,Is.EqualTo(PuzzleMode.Words));
                var a=new AttemptService(word.Definition,locale);var route=PuzzleSolver.FindShortest(a.Resolver);
                Assert.That(route.OptimalActions,Is.EqualTo(result.OptimalActions));foreach(var c in route.Path)a.Execute(c);
                Assert.That(a.Current.Status,Is.EqualTo(PuzzleStatus.Won));Assert.That(LevelValidator.Validate(a.Level,catalog.Words),Is.Empty);
            }
        }
        [Test] public void ExtensionRejectsDuplicateNodesAndMissingBindingsArray()
        {
            var extension=JObject.Parse(Extension);extension["levels"][0]["id"]="C01";
            Assert.Throws<InvalidDataException>(()=>ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),extension.ToString()));
            extension=JObject.Parse(Extension);extension.Remove("bindings");
            Assert.Throws<InvalidDataException>(()=>ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),extension.ToString()));
        }
    }
}
