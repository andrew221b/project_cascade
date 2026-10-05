using Cascade.Application;
using Cascade.Domain;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class TutorialCoachTests
    {
        [Test] public void HelpOnlyAccumulatesInForegroundAndNeverChangesState()
        {
            var level = LevelDefinitionTests.Level(); var resolver = new PuzzleResolver(level);
            var state = resolver.CreateInitialState(); var coach = new TutorialCoach(level, "swap"); coach.Observe(state, 0);
            coach.Advance(100, false); Assert.That(coach.IdleSeconds, Is.Zero);
            coach.Advance(7.9f, true); Assert.That(coach.HighlightRequested, Is.False);
            coach.Advance(.1f, true); Assert.That(coach.HighlightRequested, Is.True);
            coach.Advance(7, true); Assert.That(coach.GestureReplayRequested, Is.True);
            Assert.That(state, Is.EqualTo(resolver.CreateInitialState()));
            coach.Activity(); Assert.That(coach.IdleSeconds, Is.Zero);
        }
        [Test] public void DestroyedTeachingFloorCannotKeepTutorialWaiting()
        {
            var level = LevelDefinitionTests.Level(floors: new[] { new FloorDefinition(1,new[]{true,true}), new FloorDefinition(2,new[]{true,true}) });
            var resolver = new PuzzleResolver(level); var coach = new TutorialCoach(level,"bomb");
            var initial = resolver.CreateInitialState(); coach.Observe(initial, 0);
            var result = resolver.Resolve(initial, PuzzleCommand.Drop); coach.Observe(result.State, 1);
            Assert.That(coach.LessonCompleted, Is.True); Assert.That(coach.Prompt(result.State), Is.Empty);
        }
        [Test] public void RepeatedInvalidIsSpecificToTheSameEdgeAndResetsAfterAcceptedState()
        {
            var level = LevelDefinitionTests.Level(); var resolver = new PuzzleResolver(level);
            var initial = resolver.CreateInitialState(); var coach = new TutorialCoach(level, "swap"); coach.Observe(initial,0);
            coach.Invalid(RejectionReason.ClosedEdge,0); coach.Invalid(RejectionReason.ClosedEdge,1);
            Assert.That(coach.ExplainInvalid, Is.False);
            coach.Invalid(RejectionReason.ClosedEdge,1); Assert.That(coach.ExplainInvalid, Is.True);
            Assert.That(coach.Prompt(initial), Does.Contain("closed"));
            coach.Observe(resolver.Resolve(initial,PuzzleCommand.Swap(0)).State,1);
            Assert.That(coach.ExplainInvalid, Is.False);
        }
        [Test] public void CompletionAndDisabledCoachingDoNotAccumulateIdleHelp()
        {
            var level = LevelDefinitionTests.Level(floors: new[] { new FloorDefinition(1,new[]{true,true}) });
            var resolver = new PuzzleResolver(level); var coach = new TutorialCoach(level,"swap");
            coach.Observe(resolver.CreateInitialState(),0); coach.Enabled=false; coach.Advance(20,true);
            Assert.That(coach.IdleSeconds, Is.Zero); coach.Enabled=true;
            coach.Observe(resolver.Resolve(resolver.CreateInitialState(),PuzzleCommand.Swap(0)).State,1); coach.Advance(20,true);
            Assert.That(coach.IdleSeconds, Is.Zero); Assert.That(coach.LessonCompleted, Is.True);
        }
    }
}
