using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class GuidedPracticeTests
    {
        static SymbolId[] Row(string row)=>row.Select(c=>SymbolId.Parse("s:"+c)).ToArray();
        static PuzzleInteraction Input(int reach=0)=>new PuzzleInteraction(new AttemptService(new LevelDefinition(
            "practice", "C01", "1", reach>0?"2.1-longreach":"2.0", "fixture",Row("CBA"),Row("ABC"),
            new[]{new TargetSegment(SegmentKind.Shapes,0,3)},new[]{new FloorDefinition(6,new[]{true,true})},reach)));

        [Test] public void OtherTapDragKeyboardDropAndHintCannotSpendAMove()
        {
            var input=Input();var state=input.Attempt.Current;var hash=input.Attempt.CurrentStateHash;
            var hint=AttemptService.CalculateHint(input.Attempt.CaptureHintRequest());
            input.RestrictPractice(PuzzleCommand.Swap(0));
            Assert.That(input.PointerDown(1,2),Is.False);input.TapSlot(2);input.FocusSlot(2);
            Assert.That(input.SelectedSlot,Is.Null);input.FocusSlot(1);
            Assert.That(input.SwapFocused(1),Is.Null);Assert.That(input.Execute(PuzzleCommand.Drop),Is.Null);
            Assert.That(input.AllowsControl(PracticeControl.Hint),Is.False);Assert.That(input.Undo(),Is.False);Assert.That(input.Retry(),Is.False);
            Assert.That(input.PointerDown(1,1),Is.True);Assert.That(input.PointerUp(1,2,true),Is.Null);
            Assert.That(input.Attempt.Current,Is.SameAs(state));Assert.That(input.Attempt.CurrentStateHash,Is.EqualTo(hash));
            Assert.That(input.Attempt.History,Is.Empty);
        }
        [TestCase(false)][TestCase(true)]
        public void ShownPairAcceptsTapOrDragThenExplicitlyReleases(bool drag)
        {
            var input=Input();input.RestrictPractice(PuzzleCommand.Swap(0));TransitionResult result;
            if(drag){input.PointerDown(4,1);result=input.PointerUp(4,0,true);}
            else{input.TapSlot(1);result=input.TapSlot(0);}
            Assert.That(result.Accepted,Is.True);Assert.That(input.Attempt.RouteLength,Is.EqualTo(1));
            input.FinishPresentation();Assert.That(input.Execute(PuzzleCommand.Swap(0)),Is.Null,"Stale practice cannot repeat a move");
            input.ClearPracticeRestriction();Assert.That(input.Execute(PuzzleCommand.Swap(1)).Accepted,Is.True);
        }
        [Test] public void ObservationAllowsLifecycleButNoPlayerActions()
        {
            var input=Input();input.RestrictPractice();Assert.That(input.CanPlay,Is.True);
            Assert.That(input.PointerDown(4,0),Is.False);Assert.That(input.Execute(PuzzleCommand.Swap(0)),Is.Null);
            Assert.That(input.Execute(PuzzleCommand.Drop),Is.Null);Assert.That(input.Undo(),Is.False);Assert.That(input.Retry(),Is.False);
            Assert.That(input.AllowsControl(PracticeControl.Hint),Is.False);Assert.That(input.AllowsTool(TravelTool.SignalLantern),Is.False);
        }
        [Test] public void MechanicSuggestionPreservesHeldTouchAndAllowsUndoAndOtherPairs()
        {
            var input=Input();input.RestrictPractice(PuzzleCommand.Swap(0),guidanceOnly:true);
            Assert.That(input.AllowsSlot(2),Is.True);Assert.That(input.AllowsControl(PracticeControl.Undo),Is.True);
            Assert.That(input.PointerDown(7,2),Is.True);
            input.RestrictPractice(PuzzleCommand.Swap(1),guidanceOnly:true);
            Assert.That(input.OwnsPointer(7),Is.True,"Updating a suggested route must not drop the finger");
            Assert.That(input.PointerUp(7,1,true).Accepted,Is.True);input.FinishPresentation();
            Assert.That(input.Undo(),Is.True,"Undo remains available while a device is being explained");
            Assert.That(input.Attempt.Current.Row,Is.EqualTo(Row("CBA")));
        }
        [Test] public void PauseResumeCannotLoseOrQueueTheRequiredAction()
        {
            var input=Input();input.RestrictPractice(PuzzleCommand.Swap(0));input.PointerDown(4,0);input.SetPaused(true);
            Assert.That(input.PointerUp(4,1,true),Is.Null);input.SetPaused(false);
            Assert.That(input.Execute(PuzzleCommand.Swap(1)),Is.Null);Assert.That(input.Execute(PuzzleCommand.Swap(0)).Accepted,Is.True);
        }
        [Test] public void UndoLessonAllowsOnlyUndoAndCannotRepeatAfterStateChanges()
        {
            var input=Input();input.Execute(PuzzleCommand.Swap(0));input.FinishPresentation();
            input.RestrictPractice(control:PracticeControl.Undo);
            Assert.That(input.Execute(PuzzleCommand.Swap(1)),Is.Null);Assert.That(input.Undo(),Is.True);
            Assert.That(input.Undo(),Is.False);Assert.That(input.AllowsControl(PracticeControl.Hint),Is.False);
        }
        [Test] public void FirstUndoLessonRestoresWithoutRatingOrAllowanceCostAndSurvivesSave()
        {
            var input=Input();var before=input.Attempt.CurrentStateHash;
            input.Execute(PuzzleCommand.Swap(0));input.FinishPresentation();input.RestrictPractice(control:PracticeControl.Undo);
            Assert.That(input.Undo(firstLesson:true),Is.True);
            Assert.That(input.Attempt.CurrentStateHash,Is.EqualTo(before));
            Assert.That(input.Attempt.MovesMade,Is.Zero);Assert.That(input.Attempt.UndoCount,Is.Zero);
            Assert.That(input.Attempt.FreeUndoRemaining,Is.EqualTo(2));Assert.That(input.Undo(firstLesson:true),Is.False);
            var restored=AttemptService.Restore(input.Attempt.Level,AttemptRecord.Capture(input.Attempt));
            Assert.That(restored.MovesMade,Is.Zero);Assert.That(restored.UndoCount,Is.Zero);Assert.That(restored.CurrentStateHash,Is.EqualTo(before));
            Assert.That(LevelCompletion.Rate(3,3,restored.UndoCount,false,true,20,1),Is.EqualTo(3));
            input.ClearPracticeRestriction();input.Execute(PuzzleCommand.Swap(0));input.FinishPresentation();
            Assert.That(input.Undo(),Is.True);Assert.That(input.Attempt.UndoCount,Is.EqualTo(1));Assert.That(input.Attempt.FreeUndoRemaining,Is.EqualTo(1));
        }
        [TestCase(false)][TestCase(true)] public void FirstLessonFlagDoesNotExemptAnUnguidedUndo(bool guidanceOnly)
        {
            var input=Input();input.Execute(PuzzleCommand.Swap(0));input.FinishPresentation();
            if(guidanceOnly)input.RestrictPractice(control:PracticeControl.Undo,guidanceOnly:true);
            Assert.That(input.Undo(firstLesson:true),Is.True);Assert.That(input.Attempt.UndoCount,Is.EqualTo(1));
            Assert.That(input.Attempt.MovesMade,Is.EqualTo(1));Assert.That(input.Attempt.FreeUndoRemaining,Is.EqualTo(1));
        }
        [Test] public void ToolLessonAuthorizesOnlyItsInventoryTransactionEvenInPausedBag()
        {
            var input=Input();input.RestrictPractice(control:PracticeControl.Tool,tool:TravelTool.LandingLens);input.SetPaused(true);
            Assert.That(input.CanControl,Is.False);Assert.That(input.AllowsControl(PracticeControl.Toolbag),Is.True);
            Assert.That(input.AllowsTool(TravelTool.LandingLens),Is.True);Assert.That(input.AllowsTool(TravelTool.SignalLantern),Is.False);
            Assert.That(input.AllowsControl(PracticeControl.Undo),Is.False);Assert.That(input.Retry(),Is.False);
        }
        [Test] public void HintRequestPhaseCannotMovePiecesAndMovePhaseCannotUseAnotherHint()
        {
            var input=Input();var offer=AttemptService.CalculateHint(input.Attempt.CaptureHintRequest());
            input.RestrictPractice(control:PracticeControl.Hint);Assert.That(input.AllowsControl(PracticeControl.Hint),Is.True);
            Assert.That(input.Execute(offer.Result.Command.Value),Is.Null);Assert.That(input.Execute(offer.Result.Command.Value),Is.Null);
            input.RestrictPractice(offer.Result.Command);Assert.That(input.AllowsControl(PracticeControl.Hint),Is.False);
            Assert.That(input.Execute(offer.Result.Command.Value).Accepted,Is.True);
        }
        [TestCase(false)][TestCase(true)] public void RequiredReachHasOnlyItsTwoEndpointsAndNoAdjacentAlternative(bool precision)
        {
            var input=Input(1);input.RestrictPractice(PuzzleCommand.Reach(0,2));
            Assert.That(input.PointerDown(4,1),Is.False);Assert.That(input.Execute(PuzzleCommand.Swap(0)),Is.Null);
            if(precision){input.FocusSlot(2);Assert.That(input.SwapFocused(1),Is.Null);Assert.That(input.SwapFocused(-1).Accepted,Is.True);}
            else{input.PointerDown(4,2);Assert.That(input.PointerUp(4,0,true).Accepted,Is.True);}
            Assert.That(input.Attempt.Current.Status,Is.EqualTo(PuzzleStatus.Playing));
            Assert.That(input.Attempt.Current.ReachCharges,Is.Zero);
        }
    }
}
