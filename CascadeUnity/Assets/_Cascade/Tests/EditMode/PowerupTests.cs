using System.Linq;
using Cascade.Domain;
using Cascade.Content;
using Cascade.Application;
using Cascade.Solver;
using NUnit.Framework;
namespace Cascade.Tests
{
    public sealed class PowerupTests
    {
        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)]
        public void AuthoredTacticalLessonIsSolvableAndReplayable(int index)
        {
            var level=PowerupChallenges.Create()[index].Definition;var a=new AttemptService(level,"en");var proof=PuzzleSolver.FindShortest(a.Resolver);
            Assert.That(proof.Status,Is.EqualTo(SearchStatus.Solved));
            bool pickup=false;foreach(var command in proof.Path){var step=a.Execute(command);Assert.That(step.Accepted,Is.True);pickup|=step.Events.Any(e=>e.Kind==PuzzleEventKind.RibbonCollected||e.Kind==PuzzleEventKind.HourglassCollected);Assert.That(LevelValidator.SameMultiset(a.Current.Row,level.InitialSymbolIds),Is.True);}
            Assert.That(pickup,Is.True,"The shortest solution must use the taught pickup");Assert.That(a.Current.Status,Is.EqualTo(PuzzleStatus.Won));
            Assert.That(AttemptService.Restore(level,AttemptRecord.Capture(a)).CurrentStateHash,Is.EqualTo(a.CurrentStateHash));
            while(a.Undo()){}Assert.That(a.Current.PowerupUsed,Is.False);Assert.That(a.Current.FrozenTicks,Is.Zero);Assert.That(a.Current.ReachCharges,Is.Zero);
        }
        [Test] public void HourglassProtectsTwoTicksButNeverProtectsWood()
        {
            var a=new AttemptService(PowerupChallenges.Create()[3].Definition,"en");
            var first=a.Execute(PuzzleCommand.Swap(0));Assert.That(a.Current.BombTicks,Is.EqualTo(2));Assert.That(a.Current.FrozenTicks,Is.EqualTo(1));Assert.That(a.Current.Durability,Is.EqualTo(3));
            Assert.That(first.Events.Count(e=>e.Kind==PuzzleEventKind.HourglassCollected),Is.EqualTo(1));
            a.Execute(PuzzleCommand.Swap(0));Assert.That(a.Current.BombTicks,Is.EqualTo(2));Assert.That(a.Current.FrozenTicks,Is.Zero);
            a.Execute(PuzzleCommand.Swap(0));Assert.That(a.Current.BombTicks,Is.EqualTo(1));Assert.That(a.Current.PowerupUsed,Is.True);Assert.That(a.Current.Durability,Is.EqualTo(1));
        }
        [Test] public void RibbonCarriesDownAndCannotBeCollectedTwice()
        {
            var a=new AttemptService(PowerupChallenges.Create()[2].Definition,"en");a.Execute(PuzzleCommand.Swap(0));
            Assert.That(a.Current.ActiveFloor,Is.EqualTo(1));Assert.That(a.Current.ReachCharges,Is.EqualTo(1));Assert.That(a.Current.PowerupUsed,Is.False);
            a.Execute(PuzzleCommand.Reach(1,4));Assert.That(a.Current.ReachCharges,Is.Zero);a.Undo();Assert.That(a.Current.ReachCharges,Is.EqualTo(1));
        }
    }
}
