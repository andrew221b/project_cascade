using System;
using System.IO;
using System.Linq;
using System.Threading;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Solver;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class WindConflictTests
    {
        ContentCatalog catalog;

        [OneTimeSetUp] public void Setup()
        {
            string Read(string file) => File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot, "Data", file));
            catalog = ContentImporter.Import(Read("levels.json"), Read("localized_levels.json"), Read("words_en.json"),
                Read("words_ru.json"), Read("words_uk.json"), Read("campaign_extension.json"));
        }

        static PuzzleCommand Decode(string encoded) => encoded == "D" ? PuzzleCommand.Drop
            : encoded.StartsWith("R", StringComparison.Ordinal)
                ? PuzzleCommand.Reach(int.Parse(encoded.Substring(1).Split(':')[0]), int.Parse(encoded.Split(':')[1]))
                : PuzzleCommand.Swap(int.Parse(encoded));

        [TestCase("C29", DeliveryMood.Wind)]
        [TestCase("C32", DeliveryMood.Wind)]
        [TestCase("C17", DeliveryMood.Tight)]
        [TestCase("C42", DeliveryMood.Tight)]
        public void VerifiedPlannerReplacesAnImpossiblePeakWithProvedRecovery(string node, DeliveryMood mood)
        {
            var source = catalog.GetPrimary(node).Definition;
            var raw = DeliveryDirector.Plan(source, mood);
            string oldPin = raw.Apply(source).ContentHash;
            Assert.That(PuzzleSolver.FindShortest(new PuzzleResolver(raw.Apply(source))).Status,
                Is.EqualTo(SearchStatus.Unsolvable));

            var calibrated = DeliveryDirector.PlanVerified(source, mood);
            Assert.That(calibrated, Is.Not.Null);
            Assert.That(calibrated.Mood, Is.EqualTo(DeliveryMood.Recovery));
            Assert.That(calibrated.WindFloor, Is.EqualTo(-1));
            Assert.That(PuzzleSolver.FindShortest(new PuzzleResolver(calibrated.Apply(source))).Status,
                Is.EqualTo(SearchStatus.Solved));
            Assert.That(raw.Apply(source).ContentHash, Is.EqualTo(oldPin));
        }

        [Test] public void VerifiedPlannerPreservesAProvedWindRecipeAndItsRecoveryLanding()
        {
            var source = catalog.GetPrimary("C81").Definition;
            var raw = DeliveryDirector.Plan(source, DeliveryMood.Wind);
            var calibrated = DeliveryDirector.PlanVerified(source, DeliveryMood.Wind);
            Assert.That(calibrated.Apply(source).ContentHash, Is.EqualTo(raw.Apply(source).ContentHash));

            var recovery = DeliveryDirector.PlanVerified(source, DeliveryMood.Recovery, raw);
            Assert.That(recovery.Mood, Is.EqualTo(DeliveryMood.Recovery));
            Assert.That(recovery.WindFloor, Is.EqualTo(raw.WindFloor));
            Assert.That(recovery.WindRight, Is.EqualTo(raw.WindRight));
            Assert.That(PuzzleSolver.FindShortest(new PuzzleResolver(recovery.Apply(source))).Status,
                Is.EqualTo(SearchStatus.Solved));
        }

        [Test] public void VerifiedPlannerDoesNotPublishUnknownOrCanceledCalibration()
        {
            var source = catalog.GetPrimary("C29").Definition;
            Assert.That(DeliveryDirector.PlanVerified(source, DeliveryMood.Wind, budget: 1), Is.Null);
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                Assert.Throws<OperationCanceledException>(() =>
                    DeliveryDirector.PlanVerified(source, DeliveryMood.Wind, cancellation: cancellation.Token));
            }
            Assert.That(PuzzleSolver.FindShortest(new PuzzleResolver(source)).Status, Is.EqualTo(SearchStatus.Solved));
        }

        [Test] public void OldRawWindSaveKeepsItsExactPinAndCommandsAfterFreshCalibration()
        {
            var source = catalog.GetPrimary("C29").Definition;
            var oldRecipe = DeliveryDirector.Plan(source, DeliveryMood.Wind);
            var old = new AttemptService(oldRecipe.Apply(source), direction: oldRecipe);
            Assert.That(old.Execute(PuzzleCommand.Swap(0)).Accepted, Is.True);
            var profile = new PlayerProfile { Attempt = AttemptRecord.Capture(old) };
            var fresh = DeliveryDirector.PlanVerified(source, DeliveryMood.Wind);
            Assert.That(fresh.Mood, Is.EqualTo(DeliveryMood.Recovery));

            var restored = profile.RestoreAttempt(catalog);
            Assert.That(restored.Direction.Mood, Is.EqualTo(DeliveryMood.Wind));
            Assert.That(restored.PinnedContentHash, Is.EqualTo(old.PinnedContentHash));
            Assert.That(restored.CurrentStateHash, Is.EqualTo(old.CurrentStateHash));
            Assert.That(restored.History.Select(h => h.Command), Is.EqualTo(old.History.Select(h => h.Command)));
            restored.Retry();
            Assert.That(restored.PinnedContentHash, Is.EqualTo(old.PinnedContentHash));
        }

        [TestCase("C29", "en")]
        [TestCase("C29", "ru")]
        [TestCase("C29", "uk")]
        [TestCase("C32", "en")]
        [TestCase("C32", "ru")]
        [TestCase("C32", "uk")]
        public void WindDescentProvesActualCombinedConditionsAndRestoresItsCheckpoint(string node, string locale)
        {
            var content = locale == "en" ? catalog.GetPrimary(node)
                : catalog.LocalizedLevels.Single(c => c.Definition.CampaignNodeId == node && c.Locale == locale);
            var source = content.Definition;
            var run = DescentRun.Build(source, DeliveryMood.Wind, version: 4);
            Assert.That(run, Is.Not.Null, source.PuzzleId);
            run.Validate(source, locale);

            for (int stage = 0; stage < 3; stage++)
            {
                var attempt = new AttemptService(run.Level(source, stage), locale, direction: run.Direction);
                foreach (string encoded in run.Stages[stage].Witness)
                {
                    var command = Decode(encoded);
                    var predicted = attempt.Predict(command);
                    var result = attempt.Execute(command);
                    Assert.That(result.Accepted, Is.True, source.PuzzleId + " stage " + stage);
                    Assert.That(result.State, Is.EqualTo(predicted.State));
                    Assert.That(LevelValidator.SameMultiset(result.State.Row, source.TargetSymbolIds), Is.True);
                }
                Assert.That(attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
                Assert.That(attempt.RouteLength, Is.EqualTo(run.Stages[stage].Minimum));
                if (stage == 2) continue;

                run.AcceptCheckpoint(source, attempt);
                var next = new AttemptService(run.Level(source, run.StageIndex), locale, direction: run.Direction);
                var profile = new PlayerProfile { Descent = run, Attempt = AttemptRecord.Capture(next) };
                var restored = profile.RestoreAttempt(catalog);
                Assert.That(restored.PinnedContentHash, Is.EqualTo(next.PinnedContentHash));
                Assert.That(restored.CurrentStateHash, Is.EqualTo(next.CurrentStateHash));
            }
        }

        [Test] public void LinkedFragileFallAndWindResolveAllContactsOnTheFinalRow()
        {
            var a = SymbolId.Parse("s:A"); var b = SymbolId.Parse("s:B");
            var c = SymbolId.Parse("s:C"); var d = SymbolId.Parse("s:D");
            var level = new LevelDefinition("wind-combined", "C81", "1", "2.0", new string('c', 64),
                new[] { b, a, d, c }, new[] { d, a, b, c },
                new[] { new TargetSegment(SegmentKind.Shapes, 0, 4) },
                new[] {
                    new FloorDefinition(1, new[] { true, true, true }, new[] { 0, 2 }, new LinkedPair(0, 2)),
                    new FloorDefinition(1, new[] { false, false, false }, entryTransform: EntryTransform.ShiftRightOne,
                        bomb: new BombDefinition(0, d, 1, 2), key: new KeyDefinition(1, a, 2),
                        repair: new RepairDefinition(2, b, 1), optionalStamp: new ContactDefinition(0, d),
                        powerup: new PowerupDefinition(3, c, PowerupKind.Hourglass))
                });
            var resolver = new PuzzleResolver(level);
            var initial = resolver.CreateInitialState();
            var result = resolver.Resolve(initial, PuzzleCommand.Swap(0));

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.DurabilityCost, Is.EqualTo(3));
            Assert.That(initial.Row, Is.EqualTo(new[] { b, a, d, c }));
            Assert.That(result.State.Row, Is.EqualTo(level.TargetSymbolIds));
            Assert.That(result.State.ActiveFloor, Is.EqualTo(1));
            Assert.That(result.State.Durability, Is.EqualTo(2));
            Assert.That(result.State.BombPhase, Is.EqualTo(BombPhase.Defused));
            Assert.That(result.State.ExplodedCount, Is.Zero);
            Assert.That(result.State.KeyUsed && result.State.RepairUsed && result.State.PowerupUsed, Is.True);
            Assert.That(result.State.FrozenTicks, Is.EqualTo(2));
            Assert.That(result.State.StampBits, Is.EqualTo(2));
            Assert.That(resolver.IsEdgeOpen(result.State, 2), Is.True);
            Assert.That(LevelValidator.SameMultiset(initial.Row, result.State.Row), Is.True);

            var events = result.Events.ToList();
            int wind = events.FindIndex(e => e.Kind == PuzzleEventKind.EntryPermuted);
            foreach (var kind in new[] { PuzzleEventKind.BombDefused, PuzzleEventKind.KeyOpened,
                PuzzleEventKind.Repaired, PuzzleEventKind.HourglassCollected, PuzzleEventKind.StampCollected })
                Assert.That(events.FindIndex(e => e.Kind == kind), Is.GreaterThan(wind), kind.ToString());

            var proof = PuzzleSolver.FindShortest(resolver);
            Assert.That(proof.Status, Is.EqualTo(SearchStatus.Solved));
            Assert.That(proof.OptimalActions, Is.EqualTo(2));
            var state = initial;
            foreach (var command in proof.Path) state = resolver.Resolve(state, command).State;
            Assert.That(state.Status, Is.EqualTo(PuzzleStatus.Won));
        }
    }
}
