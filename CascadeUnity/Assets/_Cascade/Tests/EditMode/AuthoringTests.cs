using System;
using System.IO;
using System.Linq;
using System.Threading;
using Cascade.Domain;
using Cascade.Editor.Authoring;
using Cascade.Solver;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cascade.Tests
{
    public sealed class AuthoringTests
    {
        LevelDraft draft;
        [SetUp] public void SetUp() { draft = ScriptableObject.CreateInstance<LevelDraft>(); draft.hideFlags = HideFlags.HideAndDontSave; }
        [TearDown] public void TearDown() { Undo.ClearUndo(draft); UnityEngine.Object.DestroyImmediate(draft); }
        void Load(string id) => draft.Read((JObject)ReferenceFixtures.Levels.Single(l => (string)l["id"] == id));
        [Test] public void NewDraftWithoutJsonValidatesSolvesAndReplays()
        {
            var words = ContentTests.LoadCatalog().Words; var level = draft.Definition(words);
            var proof = PublicationProof.Analyze(level, draft.Goal());
            Assert.That(proof.Search.Status, Is.EqualTo(SearchStatus.Solved));
            Assert.That(proof.ExportMetadata(level, draft.Goal())["content_hash"].ToString(), Is.EqualTo(level.ContentHash));
        }
        [TestCase("C01")][TestCase("C27")][TestCase("C35")][TestCase("C38")][TestCase("C40")]
        public void LoadedDraftPreservesRulesAndOriginalSymbolIds(string id)
        {
            Load(id); var source = (JObject)ReferenceFixtures.Levels.Single(l => (string)l["id"] == id);
            Assert.That(draft.Source()["start"].Select(t => (string)t), Is.EqualTo(source["start"].Select(t => (string)t)));
            Assert.That(JToken.DeepEquals(draft.Source()["floors"], source["floors"]), Is.True);
            Assert.That(draft.Definition(ContentTests.LoadCatalog().Words).PuzzleId, Is.EqualTo(id));
        }
        [Test] public void ResizeFloorsAndWordPlacementNeedNoRawJson()
        {
            var words = ContentTests.LoadCatalog().Words;
            draft.ResizeRow(7); draft.ResizeFloors(2); draft.ApplyWord(words["en_CAT"], 2);
            draft.initial = (string[])draft.target.Clone();
            var level = draft.Definition(words);
            Assert.That(level.TargetSegments.Select(s => s.Length), Is.EqualTo(new[] { 2, 3, 2 }));
            Assert.That(level.Floors.All(f => f.OpenEdges.Count == 6), Is.True);
        }
        [Test] public void ExportRoundTripPinsHashRulesAndStampGoal()
        {
            Load("C35"); draft.requireStamps = true; var words = ContentTests.LoadCatalog().Words;
            var level = draft.Definition(words); var proof = PublicationProof.Analyze(level, draft.Goal());
            Assert.That(proof.Search.OptimalActions, Is.EqualTo(4));
            var roundTrip = Cascade.Content.ContentImporter.ImportLevel(JObject.Parse(draft.Source().ToString()), words);
            Assert.That(roundTrip.ContentHash, Is.EqualTo(level.ContentHash));
            var metadata = proof.ExportMetadata(roundTrip, draft.Goal());
            Assert.That((int)metadata["required_stamp_bits"], Is.EqualTo(1)); Assert.That((string)metadata["rules_version"], Is.EqualTo("2.0"));
            string folder = Path.Combine(ReferenceFixtures.PackageRoot, "Reports/P10"); Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "C35_authoring_roundtrip.json"), draft.Source().ToString());
            File.WriteAllText(Path.Combine(folder, "C35_authoring_roundtrip.proof.json"), metadata.ToString());
        }
        [Test] public void ActualWorkshopExportMatchesItsProof()
        {
            string folder = Path.Combine(ReferenceFixtures.PackageRoot, "Reports/P10");
            draft.Read(JObject.Parse(File.ReadAllText(Path.Combine(folder, "QA_P10_001.json"))));
            var saved = JObject.Parse(File.ReadAllText(Path.Combine(folder, "QA_P10_001.proof.json")));
            var level = draft.Definition(ContentTests.LoadCatalog().Words);
            var proof = PublicationProof.Analyze(level, new PuzzleGoal((int)saved["required_stamp_bits"]));
            Assert.That(proof.ExportMetadata(level, proof.Goal)["content_hash"].ToString(), Is.EqualTo((string)saved["content_hash"]));
            Assert.That(proof.Search.OptimalActions, Is.EqualTo((int)saved["optimal_actions"]));
            Assert.That(proof.Search.Path.Select(c => c.ToString()), Is.EqualTo(saved["path"].Select(t => (string)t)));
        }
        [Test] public void StaleProofAndChangedObjectiveCannotExport()
        {
            Load("C35"); var words = ContentTests.LoadCatalog().Words; var level = draft.Definition(words);
            var proof = PublicationProof.Analyze(level, draft.Goal());
            draft.requireStamps = true; Assert.Throws<InvalidOperationException>(() => proof.ExportMetadata(level, draft.Goal()));
            draft.requireStamps = false; draft.floors[0].hp++;
            Assert.Throws<InvalidOperationException>(() => proof.ExportMetadata(draft.Definition(words), draft.Goal()));
        }
        [Test] public void UnreachableStampAndBudgetUnknownAreNotPublicationProofs()
        {
            Load("C35"); var words = ContentTests.LoadCatalog().Words; var level = draft.Definition(words);
            var unknown = PublicationProof.Analyze(level, PuzzleGoal.Delivery, 1);
            Assert.That(unknown.Search.Status, Is.EqualTo(SearchStatus.Unknown));
            Assert.Throws<InvalidOperationException>(() => unknown.ExportMetadata(level, PuzzleGoal.Delivery));
            draft.requireStamps = true; draft.floors[0].hp = 1; level = draft.Definition(words);
            var unreachable = PublicationProof.Analyze(level, draft.Goal());
            Assert.That(unreachable.Search.Status, Is.EqualTo(SearchStatus.Unsolvable));
            Assert.Throws<InvalidOperationException>(() => unreachable.ExportMetadata(level, draft.Goal()));
        }
        [Test] public void CancellationIsUnknownWithoutMetrics()
        {
            using (var cancellation = new CancellationTokenSource()) {
                cancellation.Cancel(); var proof = PublicationProof.Analyze(draft.Definition(ContentTests.LoadCatalog().Words), draft.Goal(), cancellation: cancellation.Token);
                Assert.That(proof.Search.Status, Is.EqualTo(SearchStatus.Unknown)); Assert.That(proof.Search.Metrics, Is.Null);
            }
        }
        [Test] public void BypassableAndOnEntryKeysProduceWarnings()
        {
            draft.floors[0].open[1] = false; var key = draft.floors[0].key;
            key.enabled = true; key.value = 1; key.slot = 0; key.symbol = "s:B";
            var proof = PublicationProof.Analyze(draft.Definition(ContentTests.LoadCatalog().Words), draft.Goal());
            Assert.That(proof.Warnings.Any(w => w.Contains("activates on entry")), Is.True);
            Assert.That(proof.Warnings.Any(w => w.Contains("can be bypassed")), Is.True);
        }
        [Test] public void ActualUnityUndoRedoRestoresDraftWithoutTouchingScene()
        {
            int sceneCount = EditorSceneManager.sceneCount; var active = EditorSceneManager.GetActiveScene(); bool dirty = active.isDirty;
            Undo.RecordObject(draft, "Test authoring change"); draft.ResizeRow(8); draft.ResizeFloors(4); Undo.FlushUndoRecordObjects();
            Undo.PerformUndo(); Assert.That(draft.target.Length, Is.EqualTo(3)); Assert.That(draft.floors.Length, Is.EqualTo(1));
            Undo.PerformRedo(); Assert.That(draft.target.Length, Is.EqualTo(8)); Assert.That(draft.floors.Length, Is.EqualTo(4));
            Assert.That(EditorSceneManager.sceneCount, Is.EqualTo(sceneCount)); Assert.That(EditorSceneManager.GetActiveScene().handle, Is.EqualTo(active.handle));
            Assert.That(active.isDirty, Is.EqualTo(dirty));
        }
    }
}
