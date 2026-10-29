using System;
using System.IO;
using Cascade.Application;
using Cascade.Domain;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class UnavailableHintAllowanceTests
    {
        static AttemptService Attempt()=>new AttemptService(LevelDefinitionTests.Level(LevelDefinitionTests.Row("EDCBA"),LevelDefinitionTests.Row("ABCDE"),new[]{new FloorDefinition(12,new[]{true,true,true,true})}));
        static HintOffer Hint(AttemptService a,int budget=Cascade.Solver.PuzzleSolver.DefaultStateBudget)=>AttemptService.CalculateHint(a.CaptureHintRequest(),budget);
        [Test] public void ImmediateHelpOnlyRevealsInformationAndStartsThirtyActiveSecondCooldown()
        {
            var a=Attempt();var policy=new UnavailableHintAllowance();var state=a.Current;var offer=Hint(a);
            Assert.That(policy.TryReveal(a,offer,()=>true),Is.True);Assert.That(a.Current,Is.SameAs(state));Assert.That(a.HintUsed,Is.True);
            Assert.That(a.MovesMade,Is.Zero);Assert.That(a.FreeHintStates.Count,Is.EqualTo(1));Assert.That(policy.Remaining(a),Is.EqualTo(30));
            a.Execute(offer.Result.Command.Value);Assert.That(policy.TryReveal(a,Hint(a),()=>true),Is.False);
            a.AdvanceActiveTime(29);Assert.That(policy.Remaining(a),Is.EqualTo(1));a.AdvanceActiveTime(1);Assert.That(policy.TryReveal(a,Hint(a),()=>true),Is.True);
        }
        [Test] public void ReusingARevealedStateDoesNotResetCooldown()
        {
            var a=Attempt();var policy=new UnavailableHintAllowance();var offer=Hint(a);Assert.That(policy.TryReveal(a,offer,()=>true),Is.True);
            a.AdvanceActiveTime(10);Assert.That(policy.TryReveal(a,offer,()=>true),Is.True);Assert.That(policy.Remaining(a),Is.EqualTo(20));Assert.That(a.FreeHintStates.Count,Is.EqualTo(1));
        }
        [TestCase(false)][TestCase(true)] public void FailedSaveRollsBackHintAndDoesNotChargeCooldown(bool throws)
        {
            var a=Attempt();var policy=new UnavailableHintAllowance();var offer=Hint(a);
            if(throws)Assert.Throws<IOException>(()=>policy.TryReveal(a,offer,()=>throw new IOException("fixture")));
            else Assert.That(policy.TryReveal(a,offer,()=>false),Is.False);
            Assert.That(a.FreeHintStates,Is.Empty);Assert.That(a.HintUsed,Is.False);Assert.That(policy.Remaining(a),Is.Zero);Assert.That(policy.TryReveal(a,offer,()=>true),Is.True);
        }
        [Test] public void UnknownAndStaleSolverRepliesDoNotConsumeHelp()
        {
            var a=Attempt();var policy=new UnavailableHintAllowance();Assert.That(policy.TryReveal(a,Hint(a,1),()=>true),Is.False);
            var stale=Hint(a);a.Execute(stale.Result.Command.Value);Assert.That(policy.TryReveal(a,stale,()=>true),Is.False);Assert.That(policy.Remaining(a),Is.Zero);Assert.That(a.HintUsed,Is.False);
        }
        [Test] public void RetryGetsANewAttemptAndCannotReceiveAnOldReply()
        {
            var a=Attempt();var policy=new UnavailableHintAllowance();var offer=Hint(a);policy.TryReveal(a,offer,()=>true);a.Retry();
            Assert.That(policy.Remaining(a),Is.Zero);Assert.That(policy.TryReveal(a,offer,()=>true),Is.False);Assert.That(policy.TryReveal(a,Hint(a),()=>true),Is.True);
        }
        [Test] public void UnknownLegacyStatisticsKeepASessionOnlyActiveHelpClock()
        {
            var source=Attempt();var record=AttemptRecord.Capture(source);record.ActiveSeconds=null;
            var a=AttemptService.Restore(source.Level,record);var policy=new UnavailableHintAllowance();
            Assert.That(policy.TryReveal(a,Hint(a),()=>true),Is.True);policy.WaitForVideo(a);policy.Advance(a,12);
            Assert.That(policy.LoadingWaitExpired(a),Is.True);Assert.That(policy.Remaining(a),Is.EqualTo(18));policy.Advance(a,18);
            Assert.That(policy.Remaining(a),Is.Zero);Assert.That(a.ActiveSeconds,Is.Null,"Do not invent historical performance statistics");
        }
        [Test] public void AReadyVideoClearsTheWaitSoANewLoadGetsAFreshWindow()
        {
            var a=Attempt();var policy=new UnavailableHintAllowance();policy.WaitForVideo(a);a.AdvanceActiveTime(12);
            Assert.That(policy.TakeLoadingWaitExpiry(a),Is.True);policy.ClearVideoWait();Assert.That(policy.LoadingWaitExpired(a),Is.False);
            policy.WaitForVideo(a);a.AdvanceActiveTime(11);Assert.That(policy.LoadingWaitExpired(a),Is.False);
            a.AdvanceActiveTime(1);Assert.That(policy.TakeLoadingWaitExpiry(a),Is.True);
        }
        [Test] public void LoadingWaitStartsAtExplicitRequestAndHasOneExpiryNotification()
        {
            var a=Attempt();var policy=new UnavailableHintAllowance();a.AdvanceActiveTime(50);Assert.That(policy.LoadingWaitExpired(a),Is.False);
            policy.WaitForVideo(a);a.AdvanceActiveTime(11);Assert.That(policy.LoadingWaitExpired(a),Is.False);policy.WaitForVideo(a);
            a.AdvanceActiveTime(1);Assert.That(policy.LoadingWaitExpired(a),Is.True);Assert.That(policy.TakeLoadingWaitExpiry(a),Is.True);Assert.That(policy.TakeLoadingWaitExpiry(a),Is.False);
            a.Retry();Assert.That(policy.LoadingWaitExpired(a),Is.False);
        }
    }
}
