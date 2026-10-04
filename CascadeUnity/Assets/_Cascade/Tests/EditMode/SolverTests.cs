using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cascade.Domain;
using Cascade.Solver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class SolverTests
    {
        public static IEnumerable<string> CanonicalCases => ReferenceFixtures.Levels.Select(l => (string)l["id"]);

        [TestCaseSource(nameof(CanonicalCases))]
        public void FortyCanonicalGraphsMatchPythonMetricsAndPath(string id)
        {
            var source = (JObject)ReferenceFixtures.Levels.Single(l => (string)l["id"] == id);
            var expected = ReferenceFixtures.Balance["levels"].Single(l => (string)l["id"] == id);
            var resolver = new PuzzleResolver(ReferenceFixtures.Parse(source));
            var analysis = PuzzleSolver.Analyze(resolver); var metrics = analysis.Metrics;
            Assert.That(analysis.Status, Is.EqualTo(SearchStatus.Solved));
            Assert.That(analysis.OptimalActions, Is.EqualTo((int)expected["optimal_actions"]));
            Assert.That(metrics.States, Is.EqualTo((int)expected["states"]));
            Assert.That(metrics.Transitions, Is.EqualTo((int)expected["transitions"]));
            Assert.That(metrics.DeadStates, Is.EqualTo((int)expected["dead_states"]));
            Assert.That(metrics.EarliestDeadDepth, Is.EqualTo((int?)expected["earliest_dead_depth"]));
            Assert.That(metrics.NearOptimalFirstStatesPlusTwo, Is.EqualTo((int)expected["near_optimal_first_states_plus2"]));
            Assert.That(metrics.ForcedRatio, Is.EqualTo((double)expected["forced_ratio"]).Within(0.00001));
            Assert.That(metrics.TerminalVariants, Is.EqualTo((int)expected["terminal_variants"]));
            Assert.That(analysis.Path.Select(c => c.ToString()), Is.EqualTo(expected["path"].Select(v => v.ToString())));
            var state = resolver.CreateInitialState();
            foreach (var command in analysis.Path) { var step = resolver.Resolve(state, command); Assert.That(step.Accepted, Is.True); state = step.State; }
            Assert.That(PuzzleGoal.Delivery.IsSatisfied(state), Is.True);
            Assert.That(PuzzleSolver.FindShortest(resolver).OptimalActions, Is.EqualTo(analysis.OptimalActions));
        }

        [Test] public void BudgetAndCancellationReturnUnknownWithoutDeadStateClaims()
        {
            var resolver = new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row("DCBA"), LevelDefinitionTests.Row("ABCD"),
                new[] { new FloorDefinition(8, new[] { true, true, true }) }));
            var capped = PuzzleSolver.Analyze(resolver, stateBudget: 1);
            Assert.That(capped.Status, Is.EqualTo(SearchStatus.Unknown)); Assert.That(capped.Metrics, Is.Null);
            Assert.That(capped.OptimalActions, Is.Null); Assert.That(capped.CanPublishGoal, Is.False);
            Assert.That(PuzzleSolver.FindShortest(resolver, stateBudget: 1).Status, Is.EqualTo(SearchStatus.Unknown));
            using (var cancellation = new CancellationTokenSource())
            { cancellation.Cancel(); Assert.That(PuzzleSolver.FindShortest(resolver, cancellationToken: cancellation.Token).Status, Is.EqualTo(SearchStatus.Unknown)); }
        }
        [Test] public void ExhaustedGraphCanProveUnsolvable()
        {
            var resolver = new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row("DCBA"), LevelDefinitionTests.Row("ABCD"),
                new[] { new FloorDefinition(1, new[] { true, true, true }) }));
            Assert.That(PuzzleSolver.Analyze(resolver).Status, Is.EqualTo(SearchStatus.Unsolvable));
            Assert.That(PuzzleSolver.FindShortest(resolver).Status, Is.EqualTo(SearchStatus.Unsolvable));
        }
        [Test] public void C35SeparateGoalMinimaAndReverseDistancesMatch()
        {
            var resolver = BridgeStampTests.C35(); var delivery = PuzzleSolver.Analyze(resolver); var contract = PuzzleSolver.Analyze(resolver, new PuzzleGoal(1));
            Assert.That(delivery.OptimalActions, Is.EqualTo(2)); Assert.That(contract.OptimalActions, Is.EqualTo(4));
            Assert.That(contract.OptimalActions, Is.EqualTo(BridgeStampTests.IndependentMinimum(resolver, new PuzzleGoal(1))));
            var s = resolver.CreateInitialState(); int distance = contract.ReverseDistances[s];
            foreach (var command in contract.Path) { s = resolver.Resolve(s, command).State; Assert.That(contract.ReverseDistances[s], Is.EqualTo(--distance)); }
            Assert.That(new PuzzleGoal(1).IsSatisfied(s), Is.True);
        }
        [Test] public void LinkedEquivalentCommandsCountAsOneChoice()
        {
            var resolver = new PuzzleResolver(ReferenceFixtures.Parse((JObject)ReferenceFixtures.Levels.Single(l => (string)l["id"] == "C25")));
            var analysis = PuzzleSolver.Analyze(resolver);
            Assert.That(analysis.Metrics.Transitions, Is.EqualTo(4)); // both linked buttons, middle swap, Drop
            Assert.That(analysis.Metrics.NearOptimalFirstStatesPlusTwo, Is.EqualTo(1));
            Assert.That(analysis.Metrics.ForcedRatio, Is.EqualTo(1));
        }
        [Test] public void UnreachableContractCannotBePublished()
        {
            var resolver = new PuzzleResolver(LevelDefinitionTests.Level(LevelDefinitionTests.Row("BACD"), LevelDefinitionTests.Row("ABCD"),
                new[] { new FloorDefinition(1, new[] { true, true, true }, optionalStamp: new ContactDefinition(3, LevelDefinitionTests.S("A"))) }));
            Assert.That(PuzzleSolver.Analyze(resolver).CanPublishGoal, Is.True);
            var proof = PuzzleSolver.Analyze(resolver, new PuzzleGoal(1));
            Assert.That(proof.Status, Is.EqualTo(SearchStatus.Unsolvable)); Assert.That(proof.CanPublishGoal, Is.False);
        }
        [Test] public void SearchCanStartFromCurrentSnapshotAndNeedsNoVisualIdentity()
        {
            var resolver = BridgeStampTests.C35(); var s = resolver.Resolve(resolver.CreateInitialState(), PuzzleCommand.Swap(1)).State;
            var hint = PuzzleSolver.FindShortest(resolver, s);
            Assert.That(hint.OptimalActions, Is.EqualTo(1)); Assert.That(hint.Path[0], Is.EqualTo(PuzzleCommand.Drop));
            var terminal = resolver.Resolve(s, PuzzleCommand.Drop).State;
            Assert.That(PuzzleSolver.FindShortest(resolver, terminal).OptimalActions, Is.Zero);
        }
        [Test] public void LegacyGlyphShorthandPreservesPersistedId()
        {
            var s = SymbolId.Parse("g:C"); Assert.That(s, Is.EqualTo(SymbolId.Parse("g:en:C")));
            Assert.That(s.PersistedId, Is.EqualTo("g:C"));
            Assert.That(SymbolId.Parse("g:К", "uk"), Is.EqualTo(SymbolId.Parse("g:uk:К")));
        }
        [Test] public void ExportVerifiedCanonicalMetrics()
        {
            var results = ReferenceFixtures.Levels.Select(source =>
            {
                var level = ReferenceFixtures.Parse((JObject)source); var result = PuzzleSolver.Analyze(new PuzzleResolver(level));
                return new { id = level.PuzzleId, status = result.Status.ToString(), optimal_actions = result.OptimalActions,
                    path = result.Path.Select(c => c.ToString()).ToArray(), metrics = result.Metrics };
            }).ToArray();
            Assert.That(results.Length, Is.EqualTo(40));
            string path = Path.Combine(ReferenceFixtures.PackageRoot, "Reports", "P07", "csharp_balance.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, JsonConvert.SerializeObject(results, Formatting.Indented));
        }
    }
}
