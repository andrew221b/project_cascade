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
    public sealed class PreparationTests
    {
        static string Read(string file) => File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",file));
        static ContentCatalog Catalog() => ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
        static void Finish(PuzzlePreparation preparation)
        {
            Assert.That(SpinWait.SpinUntil(()=>{preparation.Poll();return !preparation.IsPending;},15000),Is.True);
        }
        [Test] public void FinalDevicesAndStampGoalAreSolvedWithoutMutatingAttempt()
        {
            var catalog=Catalog();
            using(var preparation=new PuzzlePreparation())
            foreach(var content in catalog.PrimaryLevels.Where(c=>c.Definition.CampaignNodeId=="C88"||c.Definition.CampaignNodeId=="C160")
                .Concat(catalog.BonusLevels)) {
                var level=content.Definition;
                int marks=level.Floors.Select((f,i)=>f.OptionalStamp==null?0:1<<i).Aggregate(0,(a,b)=>a|b);
                var attempt=new AttemptService(level,goal:new PuzzleGoal(marks));var hash=attempt.CurrentStateHash;
                preparation.Begin(level,attempt.Goal);Finish(preparation);
                Assert.That(preparation.IsReady,Is.True);Assert.That(attempt.CurrentStateHash,Is.EqualTo(hash));
                foreach(var command in preparation.Result.Path)Assert.That(attempt.Execute(command).Accepted,Is.True);
                Assert.That(attempt.Goal.IsSatisfied(attempt.Current),Is.True);
                Assert.That(attempt.RouteLength,Is.EqualTo(preparation.Result.OptimalActions));
                preparation.Begin(level,attempt.Goal);Assert.That(preparation.IsPending,Is.False,"Exact content+goal proof is cached");
            }
        }
        [Test] public void CancelledOrUnknownWorkCannotUnlockOrSupplyAMinimum()
        {
            var catalog=Catalog();
            using(var preparation=new PuzzlePreparation()) {
                preparation.Begin(catalog.GetPrimary("C148").Definition,PuzzleGoal.Delivery);
                preparation.Begin(catalog.GetPrimary("C01").Definition,PuzzleGoal.Delivery);Finish(preparation);
                Assert.That(preparation.Result.OptimalActions,Is.EqualTo(1));
            }
            using(var limited=new PuzzlePreparation(1)) {
                limited.Begin(catalog.GetPrimary("C88").Definition,PuzzleGoal.Delivery);Finish(limited);
                Assert.That(limited.IsReady,Is.False);Assert.That(limited.HasFailed,Is.True);
                Assert.That(limited.Result.OptimalActions,Is.Null);
                limited.Begin(catalog.GetPrimary("C88").Definition,PuzzleGoal.Delivery);
                Assert.That(limited.IsPending,Is.True,"Unknown is never cached as proof");
            }
        }
        [Test] public void PreparationGateCannotBeBypassedByFocusOrPresentationCompletion()
        {
            var input=new PuzzleInteraction(new AttemptService(Catalog().GetPrimary("C01").Definition));
            var before=input.Attempt.CurrentStateHash;input.SetPreparing(true);
            input.SetSuspended(false);input.FinishPresentation();input.SetPaused(false);
            Assert.That(input.Execute(PuzzleCommand.Drop),Is.Null);Assert.That(input.PointerDown(1,0),Is.False);
            Assert.That(input.Retry(),Is.False);Assert.That(input.Attempt.CurrentStateHash,Is.EqualTo(before));
            input.SetPreparing(false);Assert.That(input.Execute(PuzzleCommand.Swap(0)).Accepted,Is.True);
        }
        [Test] public void ChangedBudgetRestoresAllPreviousPinsAndTheirExactRoutes()
        {
            var catalog=Catalog();
            var legacy=ContentImporter.ImportLegacyDurability(catalog,Read("campaign_extension.json"),Read("campaign_durability_v1.json"));
            Assert.That(legacy.Count,Is.EqualTo(103*4));
            foreach(var content in legacy) {
                var attempt=new AttemptService(content.Definition,content.Locale);
                // Compatibility checks need committed prefixes, not 412 repeated full searches.
                var first=Enumerable.Range(0,attempt.Current.Row.Count-1).Select(PuzzleCommand.Swap)
                    .FirstOrDefault(c=>attempt.Resolver.Preview(attempt.Current,c).IsLegal);
                if(!attempt.Resolver.Preview(attempt.Current,first).IsLegal)first=PuzzleCommand.Drop;
                attempt.Execute(first);attempt.Execute(PuzzleCommand.Drop);
                var profile=new PlayerProfile{Attempt=AttemptRecord.Capture(attempt)};
                var restored=profile.RestoreAttempt(catalog,legacy);
                Assert.That(restored.CurrentStateHash,Is.EqualTo(attempt.CurrentStateHash));
                Assert.That(restored.PinnedContentHash,Is.EqualTo(attempt.PinnedContentHash));
                if(restored.CanUndo){var before=restored.History.Last().Before;restored.Undo();Assert.That(restored.Current,Is.EqualTo(before));}
            }
        }
    }
}
