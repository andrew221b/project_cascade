using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Cascade.Tests
{
    public sealed class ReceiverPresentationTests
    {
        [TestCase(false)] [TestCase(true)] public void DescentIsMonotoneBoundedAndSettlesWithoutLateralOvershoot(bool receiver)
        {
            var start=new Vector3(-.6f,4,-.35f);var seat=new Vector3(-.54f,0,-.35f);float previous=start.y;
            for(int i=0;i<=1000;i++){
                var p=DeliveryFlight.Fall(start,seat,i/1000f,receiver);
                Assert.That(p.y,Is.InRange(seat.y,previous+.00001f));previous=p.y;
                Assert.That(p.x,Is.InRange(start.x,seat.x));
            }
            Assert.That(DeliveryFlight.Fall(start,seat,1,receiver),Is.EqualTo(seat));
            Assert.That((DeliveryFlight.Fall(start,seat,1,receiver)-DeliveryFlight.Fall(start,seat,.999f,receiver)).magnitude,Is.LessThan(.0002f));
        }
        [TestCase(false)] [TestCase(true)] public void GravityAndCaptureJoinHaveContinuousVelocity(bool receiver)
        {
            float join=receiver?.65f:.88f,h=.0001f;var start=Vector3.up*4;var seat=Vector3.zero;
            float left=(DeliveryFlight.Fall(start,seat,join,receiver).y-DeliveryFlight.Fall(start,seat,join-h,receiver).y)/h;
            float right=(DeliveryFlight.Fall(start,seat,join+h,receiver).y-DeliveryFlight.Fall(start,seat,join,receiver).y)/h;
            Assert.That(Mathf.Abs(right-left),Is.LessThan(.03f));
        }
        [Test] public void WholeIncorrectRowIsRejectedAndRemainsAvailableForComparison()
        {
            for(int slot=0;slot<8;slot++){
                var start=new Vector3(slot*.7f,0,0);var comparison=start+Vector3.down*.9f;
                Assert.That(DeliveryFlight.Reject(start,comparison,0,slot,8),Is.EqualTo(start));
                Assert.That(DeliveryFlight.Reject(start,comparison,1,slot,8),Is.EqualTo(comparison));
                if(slot==0)Assert.That(DeliveryFlight.Reject(start,comparison,.4f,slot,8).x,Is.LessThan(start.x));
                if(slot==7)Assert.That(DeliveryFlight.Reject(start,comparison,.4f,slot,8).x,Is.GreaterThan(start.x));
            }
        }
        [Test] public void HeavyReceiverIsBoundedAcrossGustsSlowFramesPauseAndReducedMotion()
        {
            var motion=new ReceiverSuspension();motion.Impulse(1,1);
            for(int i=0;i<3000;i++){
                motion.Tick(i%5==0?.2f:1f/60,Mathf.Sin(i*.03f),false);
                Assert.That(Mathf.Abs(motion.Sway),Is.LessThanOrEqualTo(.14f));Assert.That(Mathf.Abs(motion.Roll),Is.LessThanOrEqualTo(1.2f));
                Assert.That(float.IsFinite(motion.Heave),Is.True);
            }
            var pose=new Vector3(motion.Sway,motion.Roll,motion.Heave);motion.Tick(0,-1,false);
            Assert.That(new Vector3(motion.Sway,motion.Roll,motion.Heave),Is.EqualTo(pose));
            motion.Tick(.03f,1,true);Assert.That(new Vector3(motion.Sway,motion.Roll,motion.Heave),Is.EqualTo(Vector3.zero));
        }
        [Test] public void HangingArtFittingReservesVisibleCordAndTimerBeforeItsRotatedSilhouette()
        {
            float width=HangingPropBody.FitWidth(88,140,1,26,1.1f);
            float envelope=Mathf.Sqrt(.83f*.83f+.775f*.775f)*1.1f*width;
            Assert.That(envelope+Mathf.Clamp((140-12-26)*.4f,18,52)+12+26,Is.LessThanOrEqualTo(140.001f));Assert.That(width,Is.GreaterThan(35));
        }
    }
}
