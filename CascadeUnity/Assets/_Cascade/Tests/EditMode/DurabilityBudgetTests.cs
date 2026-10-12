using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Solver;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class DurabilityBudgetTests
    {
        ContentCatalog catalog;
        [OneTimeSetUp] public void Load(){
            string Read(string f)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",f));
            catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
        }
        public static LevelDefinition Reduce(LevelDefinition level,int floor,int hp)
        {
            var floors=level.Floors.Select((f,i)=>i!=floor?f:new FloorDefinition(hp,f.OpenEdges,f.FragileEdges,
                f.LinkedPair,f.EntryTransform,f.Bomb,f.Key,f.Repair,f.OneUseBridgeEdge,f.OptionalStamp,f.Powerup,
                f.GustAfterSwaps,f.GustTransform,f.GustDamage));
            return new LevelDefinition(level.PuzzleId,level.CampaignNodeId,level.SchemaVersion,level.RulesVersion,
                "reduced-budget",level.InitialSymbolIds,level.TargetSymbolIds,level.TargetSegments,floors,level.ReachCharges,level.TeachingNode);
        }
        [TestCase("C01")][TestCase("C02")][TestCase("C07")][TestCase("C08")][TestCase("C09")]
        [TestCase("C10")][TestCase("C11")][TestCase("C12")][TestCase("C13")][TestCase("C14")]
        [TestCase("C37")][TestCase("C65")][TestCase("C120")][TestCase("C360")][TestCase("C500")]
        public void CampaignAndMissionHaveNoReducibleLessonBudgetOrStartingTransform(string node)
        {
            var source=catalog.GetPrimary(node).Definition;
            using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(25));
            var campaign=DescentRun.Build(source,version:6,cancellation:stop.Token);
            var mission=CampaignTriads.BuildPersonalMission(source,1,null,stop.Token,reference:campaign);
            Assert.That(mission,Is.Not.Null,node);
            var failures=new List<string>();
            foreach(var run in new[]{campaign,mission})for(int stage=0;stage<3;stage++){
                var level=run.Level(source,stage);var family=(MechanicLesson)run.Stages[stage].Triad.Lesson;
                Assert.That(level.Floors[0].EntryTransform,Is.EqualTo(EntryTransform.None));
                Assert.That(level.Floors[0].GustAfterSwaps,Is.Zero);
                for(int floor=0;floor<level.Floors.Count;floor++)for(int hp=1;hp<level.Floors[floor].BaseDurability;hp++){
                    var reduced=Reduce(level,floor,hp);
                    var proof=PuzzleSolver.FindShortest(new PuzzleResolver(reduced),stateBudget:60000,cancellationToken:stop.Token);
                    if(proof.Status==SearchStatus.Unknown||proof.Status==SearchStatus.Solved&&CampaignTriads.UsesPrinciple(reduced,proof.Path,family))
                        failures.Add($"{node} stage {stage+1} floor {floor+1} HP {hp}: {proof.Status}");
                }
            }
            Assert.That(failures,Is.Empty,string.Join("\n",failures));
        }
        [Test] public void CancellationNeverAcceptsAnUncalibratedBudget()
        {
            using var stop=new CancellationTokenSource();stop.Cancel();
            Assert.Throws<OperationCanceledException>(()=>DescentRun.Build(catalog.GetPrimary("C13").Definition,version:6,cancellation:stop.Token));
        }
        [TestCase(false,false)][TestCase(false,true)][TestCase(true,false)][TestCase(true,true)]
        public void WindAndCoupledFragileBridgeResolveFinalContactsBeforeBreak(bool right,bool fragile)
        {
            var row=new[]{"s:A","s:B","s:C","s:D"}.Select(x=>SymbolId.Parse(x)).ToArray();
            var target=(right?new[]{3,0,2,1}:new[]{0,3,1,2}).Select(i=>row[i]).ToArray();
            int contact=right?0:1,power=right?2:3,bomb=right?1:2;
            var level=new LevelDefinition("combined-budget","C99","1","2.0","combined-budget",row,target,
                new[]{new TargetSegment(SegmentKind.Shapes,0,4)},new[]{
                    new FloorDefinition(1,new[]{true,true,true}),
                    new FloorDefinition(fragile?3:2,new[]{true,false,true},fragile?new[]{0}:Array.Empty<int>(),new LinkedPair(0,2),
                        bomb:new BombDefinition(bomb,row[right?1:3],1,4),key:new KeyDefinition(contact,row[right?2:3],1),
                        repair:new RepairDefinition(contact,row[right?2:3],2),oneUseBridgeEdge:0,
                        powerup:new PowerupDefinition(power,row[right?1:0],PowerupKind.Hourglass),
                        gustAfterSwaps:1,gustTransform:right?EntryTransform.ShiftRightOne:EntryTransform.ShiftLeftOne,gustDamage:1),
                    new FloorDefinition(1,new[]{true,true,true},entryTransform:EntryTransform.MirrorNext)});
            var attempt=new AttemptService(level);attempt.Execute(PuzzleCommand.Swap(0));
            Assert.That(attempt.Current.ActiveFloor,Is.EqualTo(1));
            var changed=attempt.Execute(PuzzleCommand.Swap(0));
            Assert.That(changed.Accepted,Is.True);Assert.That(changed.State.ActiveFloor,Is.EqualTo(1));
            Assert.That(changed.State.Durability,Is.EqualTo(2));
            Assert.That(changed.State.Row,Is.EqualTo((right?new[]{2,0,1,3}:new[]{1,3,2,0}).Select(i=>row[i])));
            foreach(var kind in new[]{PuzzleEventKind.WindPermuted,PuzzleEventKind.Repaired,PuzzleEventKind.KeyOpened,
                PuzzleEventKind.HourglassCollected,PuzzleEventKind.BombFrozen,PuzzleEventKind.BridgeConsumed})
                Assert.That(changed.Events.Any(e=>e.Kind==kind),Is.True,kind.ToString());
            string stateHash=attempt.CurrentStateHash;
            Assert.That(attempt.Undo(),Is.True);Assert.That(attempt.Execute(PuzzleCommand.Swap(0)).Accepted,Is.True);
            Assert.That(attempt.CurrentStateHash,Is.EqualTo(stateHash));
            var defused=attempt.Execute(PuzzleCommand.Swap(1));
            Assert.That(defused.Events.Any(e=>e.Kind==PuzzleEventKind.BombDefused),Is.True);
            Assert.That(defused.Events.Any(e=>e.Kind==PuzzleEventKind.WindPermuted),Is.False);
            attempt.Execute(PuzzleCommand.Drop);attempt.Execute(PuzzleCommand.Swap(2));
            Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));Assert.That(attempt.Current.Row,Is.EqualTo(target));
            Assert.That(attempt.Current.ExplodedCount,Is.Zero);
        }
    }
}
