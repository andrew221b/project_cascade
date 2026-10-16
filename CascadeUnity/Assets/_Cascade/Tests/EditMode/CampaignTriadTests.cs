using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Solver;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class CampaignTriadTests
    {
        ContentCatalog catalog;
        [OneTimeSetUp] public void Load()
        {
            string Read(string file)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",file));
            catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
        }
        static PuzzleCommand Parse(string encoded)=>encoded=="D"?PuzzleCommand.Drop:encoded.StartsWith("R",StringComparison.Ordinal)
            ?PuzzleCommand.Reach(int.Parse(encoded.Substring(1).Split(':')[0]),int.Parse(encoded.Split(':')[1])):PuzzleCommand.Swap(int.Parse(encoded));
        [Test] public void RepairActuallyAddsNeededDurabilityAndDisablingItLosesTheSamePuzzle()
        {
            var source=catalog.GetPrimary("C02").Definition;var run=DescentRun.Build(source,version:6);var level=run.Level(source,0);
            var first=level.Floors[0];var stripped=new LevelDefinition(level.PuzzleId,level.CampaignNodeId,level.SchemaVersion,level.RulesVersion,"no-repair",
                level.InitialSymbolIds,level.TargetSymbolIds,level.TargetSegments,new[]{new FloorDefinition(first.BaseDurability,first.OpenEdges)});
            var resolver=new PuzzleResolver(level);var state=resolver.CreateInitialState();bool changed=false;
            foreach(var command in run.Stages[0].Witness.Select(Parse)){
                var result=resolver.Resolve(state,command);
                if(result.Events.Any(e=>e.Kind==PuzzleEventKind.Repaired)){
                    Assert.That(result.Events.Single(e=>e.Kind==PuzzleEventKind.DurabilityResolved).Value,
                        Is.EqualTo(state.Durability-result.DurabilityCost+first.Repair.Amount));changed=true;
                }
                state=result.State;
            }
            Assert.That(changed,Is.True);Assert.That(state.Status,Is.EqualTo(PuzzleStatus.Won));
            Assert.That(PuzzleSolver.FindShortest(new PuzzleResolver(stripped)).Status,Is.EqualTo(SearchStatus.Unsolvable));
        }
        [Test] public void RibbonAndFrostHaveRealGameplayConsequencesInAllThreeTasks()
        {
            foreach(string node in new[]{"C06","C07"}){
                var source=catalog.GetPrimary(node).Definition;var run=DescentRun.Build(source,version:6);
                for(int stage=0;stage<3;stage++)Assert.That(CampaignTriads.UsesPrinciple(run.Level(source,stage),
                    run.Stages[stage].Witness.Select(Parse),(MechanicLesson)run.Stages[stage].Triad.Lesson),Is.True,node+" "+stage);
                Assert.That(run.Stages.Select(s=>string.Join(",",s.Triad.Start)+"/"+string.Join(",",s.Wood)).Distinct().Count(),Is.GreaterThanOrEqualTo(2));
            }
        }
        [Test] public void OpeningUsesFourteenDifferentMechanicsAndSixthIsNotTheFirstLesson()
        {
            var opening=catalog.PrimaryLevels.Take(14).Select(c=>CampaignTriads.Primary(c.Definition)).ToArray();
            Assert.That(opening.Distinct().Count(),Is.EqualTo(14));Assert.That(opening[5],Is.EqualTo(MechanicLesson.Bridge));
            foreach(var content in catalog.PrimaryLevels.Take(14)){
                var run=DescentRun.Build(content.Definition,version:6);run.Validate(content.Definition,content.Locale);
                foreach(var stage in run.Stages)foreach(var board in stage.Triad.Boards)
                    if(board.BombSlot>=0)Assert.That(board.BombTicks,Is.LessThan(board.Hp),content.Definition.CampaignNodeId);
            }
        }
        [Test] public void TriadCheckpointRoundTripsThroughTheActualProfileStoreAndRetainsUndo()
        {
            var source=catalog.GetPrimary("C02").Definition;var run=DescentRun.Build(source,version:6);
            var first=new AttemptService(run.Level(source,0));foreach(var command in run.Stages[0].Witness.Select(Parse))first.Execute(command);
            run.AcceptCheckpoint(source,first);var second=new AttemptService(run.Level(source,1));second.Execute(Parse(run.Stages[1].Witness[0]));
            string directory=Path.Combine(Path.GetTempPath(),"CascadeTriadSave",Guid.NewGuid().ToString("N"));
            try{
                Directory.CreateDirectory(directory);var store=new ProfileStore(Path.Combine(directory,"profile.json"));store.Save(new PlayerProfile{Descent=run,Attempt=AttemptRecord.Capture(second)});
                var loaded=store.Load().Profile;var restored=loaded.RestoreAttempt(catalog);
                Assert.That(loaded.Descent.ContentHash,Is.EqualTo(run.ContentHash));Assert.That(restored.CurrentStateHash,Is.EqualTo(second.CurrentStateHash));
                Assert.That(restored.Undo(),Is.True);Assert.That(restored.Current.Row,Is.EqualTo(run.Level(source,1).InitialSymbolIds));
            }finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
    }
}
