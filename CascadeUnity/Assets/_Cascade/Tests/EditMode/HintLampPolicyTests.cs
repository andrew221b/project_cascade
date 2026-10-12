using System;
using System.IO;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Solver;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class HintLampPolicyTests
    {
        static AttemptService Attempt()=>new AttemptService(LevelDefinitionTests.Level(
            LevelDefinitionTests.Row("DCBA"),LevelDefinitionTests.Row("ABCD"),new[]{new FloorDefinition(10,new[]{true,true,true})}));
        static HintOffer Hint(AttemptService a,int budget=PuzzleSolver.DefaultStateBudget)=>AttemptService.CalculateHint(a.CaptureHintRequest(),budget);
        [Test] public void FiftyRevealsCannotMovePiecesAdvanceTimeOrSpendWood()
        {
            var a=Attempt();var before=a.Current;string hash=a.CurrentStateHash;var offer=Hint(a);
            for(int i=0;i<50;i++)Assert.That(a.TryRevealFreeHint(offer,()=>true),Is.True);
            Assert.That(a.Current,Is.SameAs(before));Assert.That(a.CurrentStateHash,Is.EqualTo(hash));
            Assert.That(a.History,Is.Empty);Assert.That(a.MovesMade,Is.Zero);Assert.That(a.RouteLength,Is.Zero);
            Assert.That(a.FreeHintStateHash,Is.EqualTo(hash));Assert.That(a.RevealedHintStates.Count,Is.EqualTo(1));
        }
        [Test] public void ManualMoveConsumesTheOpportunityForAnotherFreeStep()
        {
            var a=Attempt();var offer=Hint(a);Assert.That(a.TryRevealFreeHint(offer,()=>true),Is.True);
            Assert.That(a.Execute(offer.Result.Command.Value).Accepted,Is.True);var state=a.Current;
            Assert.That(a.CanShowFreeHint,Is.False);Assert.That(a.TryRevealFreeHint(Hint(a),()=>true),Is.False);
            Assert.That(a.Current,Is.SameAs(state));Assert.That(a.RouteLength,Is.EqualTo(1));
        }
        [Test] public void UndoAndReloadOnlyReopenThePreviouslyGrantedState()
        {
            var a=Attempt();var offer=Hint(a);a.TryRevealFreeHint(offer,()=>true);a.Execute(offer.Result.Command.Value);
            var loaded=AttemptService.Restore(a.Level,AttemptRecord.Capture(a));
            Assert.That(loaded.CanShowFreeHint,Is.False);Assert.That(loaded.TryRevealFreeHint(Hint(loaded),()=>true),Is.False);
            Assert.That(loaded.Undo(),Is.True);Assert.That(loaded.CanShowFreeHint,Is.True);
            Assert.That(loaded.TryRevealFreeHint(Hint(loaded),()=>true),Is.True);
            Assert.That(loaded.FreeHintStateHash,Is.EqualTo(offer.StateHash));
            loaded.Execute(PuzzleCommand.Swap(2));Assert.That(loaded.CanShowFreeHint,Is.False);
        }
        [Test] public void UnknownStaleAndNonPlayingOffersDoNotConsumeAllowance()
        {
            var a=Attempt();var unknown=Hint(a,1);Assert.That(unknown.Result.Status,Is.EqualTo(HintStatus.Unknown));
            Assert.That(a.TryRevealFreeHint(unknown,()=>true),Is.False);Assert.That(a.FreeHintStateHash,Is.Null);
            var stale=Hint(a);a.Execute(PuzzleCommand.Swap(0));Assert.That(a.TryRevealFreeHint(stale,()=>true),Is.False);
            a.Execute(PuzzleCommand.Drop);Assert.That(a.TryRevealFreeHint(Hint(a),()=>true),Is.False);
            Assert.That(a.FreeHintStateHash,Is.Null);Assert.That(a.HintUsed,Is.False);
        }
        [TestCase(false)][TestCase(true)] public void FailedPersistenceRollsBackTheGrant(bool throws)
        {
            var a=Attempt();var offer=Hint(a);
            if(throws)Assert.Throws<IOException>(()=>a.TryRevealFreeHint(offer,()=>throw new IOException("fixture")));
            else Assert.That(a.TryRevealFreeHint(offer,()=>false),Is.False);
            Assert.That(a.FreeHintStateHash,Is.Null);Assert.That(a.HintUsed,Is.False);Assert.That(a.RevealedHintStates,Is.Empty);
            Assert.That(a.History,Is.Empty);
        }
        [Test] public void RetryAndANewStageHaveOneGrantAndLegacySavesStillLoad()
        {
            var a=Attempt();a.TryRevealFreeHint(Hint(a),()=>true);a.Execute(PuzzleCommand.Swap(0));
            a.Retry();Assert.That(a.CanShowFreeHint,Is.True);Assert.That(a.FreeHintStateHash,Is.Null);
            Assert.That(Attempt().CanShowFreeHint,Is.True);
            var old=AttemptRecord.Capture(a);old.HintUsed=true;old.FreeHintStateHash=null;
            Assert.That(AttemptService.Restore(a.Level,old).CanShowFreeHint,Is.True);
            old.FreeHintStateHash="invalid";Assert.Throws<ArgumentException>(()=>AttemptService.Restore(a.Level,old));
        }
        [Test] public void WayfinderDoesNotCreateAnotherFreeGrantAndOnlyLanternCanCommitAToolMove()
        {
            var a=Attempt();a.TryRevealFreeHint(Hint(a),()=>true);a.Execute(PuzzleCommand.Swap(0));var before=a.Current;
            Assert.That(a.TryUseToolAtomically(TravelTool.Wayfinder,()=>true,Hint(a)),Is.True);
            Assert.That(a.Current,Is.SameAs(before));Assert.That(a.CanShowFreeHint,Is.False);
            a.RevealHint(Hint(a));Assert.That(a.Current,Is.SameAs(before));
            a.Execute(PuzzleCommand.Swap(1));Assert.That(a.GuideActive,Is.False);Assert.That(a.CanShowFreeHint,Is.False);
            Assert.That(a.TryUseToolAtomically(TravelTool.SignalLantern,()=>true,Hint(a)),Is.True);
            Assert.That(a.LanternUsed,Is.True);Assert.That(a.ToolUsesRemaining,Is.Zero);
            Assert.That(a.TryUseToolAtomically(TravelTool.SignalLantern,()=>true,Hint(a)),Is.False);
        }
    }
}
