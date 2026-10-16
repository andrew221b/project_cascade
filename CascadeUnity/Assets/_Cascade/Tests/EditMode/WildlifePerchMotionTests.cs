using Cascade.Presentation;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class WildlifePerchMotionTests
    {
        [Test] public void LandingSlowsAtContactWhileTakeoffAcceleratesFromRest()
        {
            float time=WildlifePerchMotion.ApproachTime;
            Assert.That(WildlifePerchMotion.LandingProgress(0),Is.EqualTo(0));
            Assert.That(WildlifePerchMotion.LandingProgress(time),Is.EqualTo(1));
            float early=WildlifePerchMotion.LandingProgress(.1f);
            float last=1-WildlifePerchMotion.LandingProgress(time-.1f);
            Assert.That(last,Is.LessThan(early*.25f),"The landing must decelerate before contact.");
            float departure=WildlifePerchMotion.DepartureTime;
            Assert.That(WildlifePerchMotion.LeavingProgress(0),Is.EqualTo(0));
            Assert.That(WildlifePerchMotion.LeavingProgress(departure),Is.EqualTo(1));
            Assert.That(WildlifePerchMotion.LeavingProgress(.1f),Is.LessThan(WildlifePerchMotion.LeavingProgress(.2f)-WildlifePerchMotion.LeavingProgress(.1f)));
            float earlyWing=WildlifePerchMotion.LandingWingClock(.3f)-WildlifePerchMotion.LandingWingClock(.2f);
            float landingWing=WildlifePerchMotion.LandingWingClock(time-.1f)-WildlifePerchMotion.LandingWingClock(time-.2f);
            Assert.That(landingWing,Is.GreaterThan(earlyWing*1.4f),"Wing beats accelerate while the body brakes for landing");
        }
        [Test] public void OccupiedSupportsWaitLongerAndWingFoldingCannotSnap()
        {
            Assert.That(WildlifePerchMotion.OccupiedIdle,Is.GreaterThanOrEqualTo(9));
            Assert.That(WildlifePerchMotion.OccupiedIdle,Is.GreaterThan(WildlifePerchMotion.EmptyIdle*2));
            float previous=0;
            for(int frame=0;frame<40;frame++){
                float fold=WildlifePerchMotion.Fold(frame/60f);
                Assert.That(fold,Is.InRange(previous,previous+.09f));previous=fold;
            }
            Assert.That(previous,Is.EqualTo(1));
        }
    }
}
