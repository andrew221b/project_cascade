using System.Linq;
using System.IO;
using NUnit.Framework;
using Cascade.Domain;
using Cascade.Application;
using Cascade.Content;
using Cascade.Persistence;
using Cascade.Solver;
using UnityEngine;
namespace Cascade.Tests.EditMode
{
    public sealed class LiveGustTests
    {
        static LevelDefinition Fixture(int damage=1)
        {
            var row=new[]{"s:A","s:B","s:C","s:D"}.Select(x=>SymbolId.Parse(x)).ToArray();
            return new LevelDefinition("live-wind","C81","1","2.0","live-wind",row,new[]{row[3],row[1],row[0],row[2]},
                new[]{new TargetSegment(SegmentKind.Shapes,0,4)},new[]{
                    new FloorDefinition(2,new[]{true,true,true},gustAfterSwaps:1,gustTransform:EntryTransform.ShiftRightOne,gustDamage:damage),
                    new FloorDefinition(4,new[]{true,true,true})});
        }
        [Test] public void GustUsesFinalContactsAndCanBreakBeforeTheReceiver()
        {
            var level=Fixture();var resolver=new PuzzleResolver(level);var before=resolver.CreateInitialState();
            var result=resolver.Resolve(before,PuzzleCommand.Swap(0));
            Assert.That(result.State.ActiveFloor,Is.EqualTo(1));Assert.That(result.State.Status,Is.EqualTo(PuzzleStatus.Playing));
            Assert.That(result.State.Row,Is.EqualTo(level.TargetSymbolIds));Assert.That(before.GustTicks,Is.EqualTo(1));
            Assert.That(result.Events.Count(e=>e.Kind==PuzzleEventKind.WindPermuted),Is.EqualTo(1));
            Assert.That(result.Events.Any(e=>e.Kind==PuzzleEventKind.DurabilityResolved&&e.Value==0),Is.True);
            Assert.That(resolver.Resolve(result.State,PuzzleCommand.Drop).State.Status,Is.EqualTo(PuzzleStatus.Won));
            Assert.That(LevelValidator.SameMultiset(before.Row,result.State.Row),Is.True);
        }
        [Test] public void NoOpDoesNotTickAndEarlyDescentAvoidsGust()
        {
            var r=new PuzzleResolver(Fixture());var s=r.CreateInitialState();
            Assert.That(r.Resolve(s,PuzzleCommand.Swap(-1)).State,Is.EqualTo(s));
            var drop=r.Resolve(s,PuzzleCommand.Drop);Assert.That(drop.Events.Any(e=>e.Kind==PuzzleEventKind.WindPermuted),Is.False);
            Assert.That(drop.State.GustTicks,Is.Zero);
        }
        [Test] public void PredictionReplayAndUndoKeepExactGustState()
        {
            var a=new AttemptService(Fixture(0));var expected=a.Predict(PuzzleCommand.Swap(0));var result=a.Execute(PuzzleCommand.Swap(0));
            Assert.That(result.State,Is.EqualTo(expected.State));var record=AttemptRecord.Capture(a);
            var restored=AttemptService.Restore(a.Level,record);Assert.That(restored.CurrentStateHash,Is.EqualTo(a.CurrentStateHash));
            Assert.That(a.Undo(),Is.True);Assert.That(a.Current.GustTicks,Is.EqualTo(1));
            Assert.That(a.Execute(PuzzleCommand.Swap(0)).State,Is.EqualTo(result.State));
        }
        [TestCase("C02")][TestCase("C03")][TestCase("C04")][TestCase("C06")][TestCase("C07")][TestCase("C29")][TestCase("C81")]
        public void NewRecipesProveAllStagesIncludingDamage(string node)
        {
            string Read(string file)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",file));
            var catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
            var source=catalog.GetPrimary(node).Definition;var run=DescentRun.Build(source,version:5);
            Assert.That(run,Is.Not.Null,node);run.Validate(source,"en");
            for(int stage=0;stage<3;stage++){
                var resolver=new PuzzleResolver(run.Level(source,stage));var proof=PuzzleSolver.FindShortest(resolver);
                Assert.That(proof.Status,Is.EqualTo(SearchStatus.Solved));Assert.That(proof.OptimalActions,Is.EqualTo(run.Stages[stage].Minimum));
                var state=resolver.CreateInitialState();foreach(var command in proof.Path)state=resolver.Resolve(state,command).State;
                Assert.That(state.Status,Is.EqualTo(PuzzleStatus.Won));
                Assert.That(resolver.Level.Floors.Last().GustAfterSwaps,Is.Zero);
            }
            Assert.That(run.Level(source,1).Floors[0].GustAfterSwaps,Is.EqualTo(1));
        }
    }
}
