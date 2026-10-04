using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Solver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class ContentTests
    {
        ContentCatalog catalog;
        static string Read(string file) => File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot, "Data", file));
        internal static ContentCatalog LoadCatalog() => ContentImporter.Import(Read("levels.json"), Read("localized_levels.json"), Read("words_en.json"), Read("words_ru.json"), Read("words_uk.json"));
        public static IEnumerable<string> LocalizedCases => JArray.Parse(Read("localized_levels.json")).Select(l => (string)l["id"]);
        [OneTimeSetUp] public void Import() { catalog = LoadCatalog(); }

        [Test] public void CountsDefaultsAndOriginalAssetsMatch()
        {
            Assert.That(catalog.PrimaryLevels.Count, Is.EqualTo(40)); Assert.That(catalog.LocalizedLevels.Count, Is.EqualTo(117));
            Assert.That(catalog.Words.Count, Is.EqualTo(111)); Assert.That(ContentCatalog.DefaultLocale, Is.EqualTo("en"));
            foreach (var pair in new Dictionary<string, int> { ["en"] = 40, ["ru"] = 35, ["uk"] = 36 })
                Assert.That(catalog.Words.Values.Count(w => w.Locale == pair.Key), Is.EqualTo(pair.Value));
            foreach (string name in new[] { "levels.json", "localized_levels.json", "words_en.json", "words_ru.json", "words_uk.json" })
            {
                string asset = Path.Combine(UnityEngine.Application.dataPath, "_Cascade/Content/Resources/CascadeContent", name);
                Assert.That(File.ReadAllBytes(asset), Is.EqualTo(File.ReadAllBytes(Path.Combine(ReferenceFixtures.PackageRoot, "Data", name))));
                var loaded = UnityEngine.Resources.Load<UnityEngine.TextAsset>("CascadeContent/" + Path.GetFileNameWithoutExtension(name));
                Assert.That(loaded, Is.Not.Null); Assert.That(loaded.text, Is.EqualTo(Read(name)));
            }
        }

        [TestCaseSource(nameof(LocalizedCases))]
        public void AllConcreteBindingsMatchPythonGraphs(string id)
        {
            var content = catalog.LocalizedLevels.Single(l => l.Definition.PuzzleId == id);
            var expected = JArray.Parse(File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot, "Reports/P09/python_localized_balance.json"))).Single(l => (string)l["id"] == id);
            Assert.That(LevelValidator.Validate(content.Definition, catalog.Words), Is.Empty);
            var attempt = new AttemptService(content.Definition, content.Locale);
            var result = PuzzleSolver.Analyze(attempt.Resolver); var metrics = result.Metrics;
            Assert.That(result.Status, Is.EqualTo(SearchStatus.Solved)); Assert.That(result.OptimalActions, Is.EqualTo((int)expected["optimal_actions"]));
            Assert.That(metrics.States, Is.EqualTo((int)expected["states"])); Assert.That(metrics.Transitions, Is.EqualTo((int)expected["transitions"]));
            Assert.That(metrics.DeadStates, Is.EqualTo((int)expected["dead_states"])); Assert.That(metrics.EarliestDeadDepth, Is.EqualTo((int?)expected["earliest_dead_depth"]));
            Assert.That(metrics.NearOptimalFirstStatesPlusTwo, Is.EqualTo((int)expected["near_optimal_first_states_plus2"]));
            Assert.That(metrics.ForcedRatio, Is.EqualTo((double)expected["forced_ratio"]).Within(0.00001));
            Assert.That(metrics.TerminalVariants, Is.EqualTo((int)expected["terminal_variants"]));
            Assert.That(result.Path.Select(c => c.ToString()), Is.EqualTo(expected["path"].Select(v => v.ToString())));
            foreach (var command in result.Path) Assert.That(attempt.Execute(command).Accepted, Is.True);
            Assert.That(attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
        }
        [Test] public void PrimaryImportKeepsIdsAndAllMinima()
        {
            foreach (var content in catalog.PrimaryLevels)
            {
                var source = ReferenceFixtures.Levels.Single(l => (string)l["id"] == content.Definition.PuzzleId);
                Assert.That(content.Definition.InitialSymbolIds.Select(s => s.PersistedId), Is.EqualTo(source["start"].Select(s => (string)s)));
                var expected = ReferenceFixtures.Balance["levels"].Single(l => (string)l["id"] == content.Definition.PuzzleId);
                Assert.That(PuzzleSolver.FindShortest(new PuzzleResolver(content.Definition)).OptimalActions, Is.EqualTo((int)expected["optimal_actions"]));
            }
        }
        [Test] public void C26IsExplicitlyShapeOnlyWithoutInventedWords()
        {
            Assert.That(catalog.IsShapeOnlyLesson("C26"), Is.True); Assert.That(catalog.GetPrimary("C26").Mode, Is.EqualTo(PuzzleMode.Shapes));
            foreach (string locale in new[] { "en", "ru", "uk" }) Assert.That(catalog.TryGetLocalizedBinding("C26", locale, out _), Is.False);
            Assert.That(catalog.LocalizedLevels.Any(l => l.Definition.CampaignNodeId == "C26"), Is.False);
        }
        [Test] public void MixedSevenAndTwoWordEightSegmentsAreBound()
        {
            Assert.That(catalog.TryGetLocalizedBinding("C30", "en", out var seven), Is.True);
            Assert.That(seven.Mode, Is.EqualTo(PuzzleMode.Mixed)); Assert.That(seven.Definition.TargetSymbolIds.Count, Is.EqualTo(7));
            Assert.That(seven.Definition.TargetSegments.Select(s => s.Length), Is.EqualTo(new[] { 2, 3, 2 }));
            Assert.That(catalog.TryGetLocalizedBinding("C32", "en", out var eight), Is.True);
            Assert.That(eight.Definition.TargetSymbolIds.Count, Is.EqualTo(8)); Assert.That(eight.WordIds, Is.EqualTo(new[] { "en_CAT", "en_DOG" }));
        }
        [Test] public void SameLettersInDifferentWordsShareSymbolId()
        {
            var source = new JObject { ["id"] = "fixture_shared_letters", ["locale"] = "en",
                ["start"] = new JArray("g:en:M", "g:en:A", "g:en:M", "g:en:A", "g:en:C", "g:en:A", "g:en:T"),
                ["target"] = new JArray("g:en:M", "g:en:A", "g:en:M", "g:en:A", "g:en:C", "g:en:A", "g:en:T"),
                ["word_ids"] = new JArray("en_MAMA", "en_CAT"),
                ["floors"] = new JArray(new JObject { ["hp"] = 1, ["edges"] = new JArray(true, true, true, true, true, true), ["fragile"] = new JArray() }) };
            var level = ContentImporter.ImportLevel(source, catalog.Words);
            Assert.That(level.TargetSegments.Select(s => s.Length), Is.EqualTo(new[] { 4, 3 }));
            Assert.That(level.TargetSymbolIds[1], Is.EqualTo(level.TargetSymbolIds[5]));
            Assert.That(level.TargetSymbolIds[3], Is.EqualTo(level.TargetSymbolIds[5]));
        }
        [Test] public void HashIsStableAcrossPropertyOrderAndPinsChangedContent()
        {
            var source = (JObject)ReferenceFixtures.Levels.Single(l => (string)l["id"] == "C01");
            var first = ContentImporter.ImportLevel(source, catalog.Words);
            var reordered = new JObject(source.Properties().Reverse().Select(p => new JProperty(p.Name, p.Value.DeepClone())));
            Assert.That(ContentImporter.ImportLevel(reordered, catalog.Words).ContentHash, Is.EqualTo(first.ContentHash));
            reordered["floors"][0]["hp"] = 2;
            Assert.That(ContentImporter.ImportLevel(reordered, catalog.Words).ContentHash, Is.Not.EqualTo(first.ContentHash));
            Assert.That(catalog.ContentHash.Length, Is.EqualTo(64));
        }
        [Test] public void RejectsCorruptWordSignatureAndUnsupportedRules()
        {
            var words = JArray.Parse(Read("words_en.json")); words[0]["equality_signature"][0] = 8;
            Assert.Throws<InvalidDataException>(() => ContentImporter.Import(Read("levels.json"), Read("localized_levels.json"), words.ToString(), Read("words_ru.json"), Read("words_uk.json")));
            var source = (JObject)ReferenceFixtures.Levels[0].DeepClone(); source["rules_version"] = "9.0";
            Assert.Throws<InvalidDataException>(() => ContentImporter.ImportLevel(source, catalog.Words));
        }
        [Test] public void EditorialAndLicenseLimitsRemainVisibleInMetadata()
        {
            Assert.That(catalog.WordEntries.Values.All(w => w.EditorialStatus == "draft_native_review_required"), Is.True);
            Assert.That(catalog.WordEntries.Values.All(w => w.LicenseReview == "required_before_release"), Is.True);
            Assert.That(catalog.WordEntries.Values.All(w => !string.IsNullOrEmpty(w.Origin)), Is.True);
        }
        [Test] public void ExportConcreteValidationSummary()
        {
            var results = catalog.LocalizedLevels.Select(content =>
            {
                var result = PuzzleSolver.Analyze(new PuzzleResolver(content.Definition));
                return new { id = content.Definition.PuzzleId, node = content.Definition.CampaignNodeId, locale = content.Locale,
                    mode = content.Mode.ToString(), hash = content.Definition.ContentHash, minimum = result.OptimalActions, metrics = result.Metrics };
            }).ToArray();
            Assert.That(results.All(r => r.minimum.HasValue), Is.True);
            File.WriteAllText(Path.Combine(ReferenceFixtures.PackageRoot, "Reports/P09/csharp_localized_balance.json"), JsonConvert.SerializeObject(results, Formatting.Indented));
        }
    }
}
