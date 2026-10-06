using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Solver;
using NUnit.Framework;
namespace Cascade.Tests
{
    public sealed class ReachTests
    {
        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)]
        public void WorkshopTrailHasProofAndPinnedReplay(int index)
        {
            var level=ReachChallenges.Create()[index].Definition;var attempt=new AttemptService(level,"en");
            var proof=PuzzleSolver.FindShortest(attempt.Resolver);
            Assert.That(proof.Status,Is.EqualTo(SearchStatus.Solved));Assert.That(proof.Path.Any(c=>c.Kind==CommandKind.LongReach),Is.True);
            foreach(var command in proof.Path){Assert.That(attempt.Execute(command).Accepted,Is.True);Assert.That(LevelValidator.SameMultiset(attempt.Current.Row,level.InitialSymbolIds),Is.True);}
            Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
            var restored=AttemptService.Restore(level,AttemptRecord.Capture(attempt));Assert.That(restored.CurrentStateHash,Is.EqualTo(attempt.CurrentStateHash));
            Assert.That(attempt.Undo(),Is.False);attempt.Retry();Assert.That(attempt.Current.ReachCharges,Is.EqualTo(1));
        }
        [Test] public void DragAcrossTwoPiecesConsumesChargeAndLocalUndoRestoresIt()
        {
            var source=ReachChallenges.Create()[1].Definition;var local=new LevelDefinition("reach-local","reach-local","1",source.RulesVersion,"test",source.InitialSymbolIds,source.TargetSymbolIds,source.TargetSegments,new[]{new FloorDefinition(4,new[]{true,true,true})},1);
            var attempt=new AttemptService(local,"en");var input=new PuzzleInteraction(attempt);
            Assert.That(input.PointerDown(7,0),Is.True);Assert.That(input.PointerUp(7,3,true).Accepted,Is.True);
            Assert.That(attempt.Current.ReachCharges,Is.Zero);Assert.That(attempt.Current.Row.Select(s=>s.Value),Is.EqualTo(new[]{"D","B","C","A"}));
            input.FinishPresentation();Assert.That(input.Undo(),Is.True);Assert.That(attempt.Current.ReachCharges,Is.EqualTo(1));
        }
        [Test] public void ClosedCorridorAndInvalidEndpointsDoNotSpendCharge()
        {
            var source=ReachChallenges.Create()[0].Definition;
            var closed=new LevelDefinition("closed","closed","1","2.1-longreach","test",source.InitialSymbolIds,source.TargetSymbolIds,source.TargetSegments,new[]{new FloorDefinition(2,new[]{true,false})},1);
            var resolver=new PuzzleResolver(closed);var before=resolver.CreateInitialState();var rejected=resolver.Resolve(before,PuzzleCommand.Reach(0,2));
            Assert.That(rejected.Accepted,Is.False);Assert.That(rejected.State,Is.SameAs(before));Assert.That(before.ReachCharges,Is.EqualTo(1));
            Assert.That(resolver.Resolve(before,PuzzleCommand.Reach(-1,2)).Accepted,Is.False);
        }
        [Test] public void RegularCampaignCannotSpendAnUnownedRibbon()
        {
            var s=ReachChallenges.Create()[0].Definition;var plain=new LevelDefinition("plain","plain","1","2.0","test",s.InitialSymbolIds,s.TargetSymbolIds,s.TargetSegments,s.Floors);
            var resolver=new PuzzleResolver(plain);Assert.That(resolver.Resolve(resolver.CreateInitialState(),PuzzleCommand.Reach(0,2)).Accepted,Is.False);
        }
    }
}
