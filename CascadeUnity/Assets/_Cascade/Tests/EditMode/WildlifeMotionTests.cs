using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Cascade.Tests
{
    public sealed class WildlifeMotionTests
    {
        [TestCase(WildlifeFlightStyle.SmallBird)]
        [TestCase(WildlifeFlightStyle.SoaringBird)]
        [TestCase(WildlifeFlightStyle.Butterfly)]
        [TestCase(WildlifeFlightStyle.Dragonfly)]
        [TestCase(WildlifeFlightStyle.Moth)]
        [TestCase(WildlifeFlightStyle.Beetle)]
        public void OpenSkyFlightsReturnAlongABroadArcWithoutTeleporting(WildlifeFlightStyle style)
        {
            WildlifeMotion.Route(-1,.62f,true,false,out var a,out var b,out var c,out var d);
            bool forward=false,returned=false;float lastX=0;
            for(int frame=0;frame<=1800;frame++) {
                float age=frame/100f;
                var p=WildlifeMotion.Path(a,b,c,d,style,age,18,.62f,.006f,0,-1);
                var before=WildlifeMotion.Path(a,b,c,d,style,Mathf.Max(0,age-.001f),18,.62f,.006f,0,-1);
                Assert.That(Vector3.Distance(p,before),Is.LessThan(.001f),"Continuous route at every join/hover.");
                if(frame>0){float dx=p.x-lastX;forward|=dx>.0002f;returned|=dx<-.0002f;}
                var heading=WildlifeMotion.Heading(a,b,c,d,style,age,18,.62f,0,-1);
                Assert.That(heading.magnitude,Is.EqualTo(1).Within(.00001f));
                lastX=p.x;
            }
            Assert.That(forward&&returned,Is.True,"The visitor must turn back, not simply follow an S-shaped crossing.");
        }
        [TestCase(WildlifeFlightStyle.SmallBird)]
        [TestCase(WildlifeFlightStyle.Butterfly)]
        [TestCase(WildlifeFlightStyle.Dragonfly)]
        [TestCase(WildlifeFlightStyle.Moth)]
        [TestCase(WildlifeFlightStyle.Beetle)]
        public void SideVisitorsEnterAndLeaveBeyondTheScreenOnCurvedRoutes(WildlifeFlightStyle style)
        {
            for(int sideIndex=0;sideIndex<2;sideIndex++)for(int visit=0;visit<32;visit++){
                float side=sideIndex==0?-1:1,seed=WildlifeMotion.Hash(visit*173+sideIndex*7919);
                WildlifeMotion.Route(side,seed,false,false,out var a,out var b,out var c,out var d);
                Assert.That(side*a.x,Is.GreaterThan(.69f),"A visitor must start beyond the viewport edge.");
                Assert.That(side*d.x,Is.GreaterThan(.69f),"A visitor must finish beyond the viewport edge.");
                float nearestEdge=1,curve=0;
                for(int sample=0;sample<=100;sample++){
                    float t=sample/100f;
                    var point=WildlifeMotion.Bezier(a,b,c,d,t);
                    nearestEdge=Mathf.Min(nearestEdge,side*point.x);
                    curve=Mathf.Max(curve,Mathf.Abs(point.x-Mathf.Lerp(a.x,d.x,t)));
                    Assert.That(float.IsNaN(point.x)||float.IsInfinity(point.y),Is.False);
                }
                Assert.That(nearestEdge,Is.LessThan(.60f),"The route should travel visibly into its side corridor.");
                Assert.That(curve,Is.GreaterThan(.12f),"The route must not read as a vertical straight drop.");
                var atFadeOut=WildlifeMotion.Path(a,b,c,d,style,.84f,1,seed,.006f,0,side);
                Assert.That(side*atFadeOut.x,Is.GreaterThan(.60f),"The final fade must begin after the body has left the visible play area.");
            }
        }
        [Test] public void FreshArrivalsStartOutsideTheFrameAndNeverUseStraightDrops()
        {
            for(int visit=0;visit<128;visit++){
                float side=visit%2==0?-1:1,seed=WildlifeMotion.Hash(visit*173+19);
                WildlifeMotion.Route(side,seed,false,false,out var a,out var b,out var c,out var d);
                Assert.That(side*a.x,Is.GreaterThan(.69f));Assert.That(side*d.x,Is.GreaterThan(.69f));
                float minX=float.PositiveInfinity,maxX=float.NegativeInfinity;
                for(int sample=0;sample<=40;sample++){
                    float t=sample/40f;float x=WildlifeMotion.Bezier(a,b,c,d,t).x;
                    minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);
                }
                Assert.That(maxX-minX,Is.GreaterThan(.14f),"An edge visitor must sweep sideways, not fall vertically.");

                WildlifeMotion.Route(side,seed,true,false,out a,out b,out c,out d);
                Assert.That(Mathf.Max(Mathf.Abs(a.x),Mathf.Abs(a.y)),Is.GreaterThan(.69f),"Crossing visitors enter beyond an edge.");
                Assert.That(Mathf.Max(Mathf.Abs(d.x),Mathf.Abs(d.y)),Is.GreaterThan(.69f),"Crossing visitors leave beyond an edge.");
                minX=float.PositiveInfinity;maxX=float.NegativeInfinity;
                for(int sample=0;sample<=40;sample++){
                    float x=WildlifeMotion.Bezier(a,b,c,d,sample/40f).x;minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);
                }
                Assert.That(maxX-minX,Is.GreaterThan(.28f),"High visitors also make a visible turn.");
            }
        }
        [Test] public void AViewThatAppearsMidFlightWaitsForTheNextArrival()
        {
            var gate=new WildlifeMotion.VisitGate();
            Assert.That(gate.Admit(4,2.4f),Is.False,"A newly visible view must not materialize a mid-flight animal.");
            Assert.That(gate.Admit(4,3.1f),Is.False,"The same visit stays hidden until its next route.");
            Assert.That(gate.Admit(5,-.4f),Is.True,"The next route is admitted before its offscreen entry.");
            Assert.That(gate.Admit(5,.31f),Is.True,"The offscreen entrance window remains admitted.");
            Assert.That(gate.Admit(6,.33f),Is.False,"A late first frame waits for another route.");
            gate.Reset();Assert.That(gate.Admit(6,-.1f),Is.True,"Rebinding begins from a fresh entrance.");
        }
        [Test] public void TurningIsCoveredByAFullRearViewBeforeTheFacingFlips()
        {
            for(int step=-100;step<=100;step++) {
                var heading=new Vector3(step/100f,1,0).normalized;
                float rear=WildlifeMotion.RearView(heading);
                Assert.That(rear,Is.InRange(0,1));
                if(Mathf.Abs(heading.x)<.08f)Assert.That(rear,Is.EqualTo(1));
            }
            Assert.That(WildlifeMotion.RearView(Vector3.right),Is.EqualTo(0));
        }
        [Test] public void DragonflyKeepsTheReturnArcHeadingDuringItsHover()
        {
            WildlifeMotion.Route(-1,.62f,true,false,out var a,out var b,out var c,out var d);
            var paused=WildlifeMotion.Heading(a,b,c,d,WildlifeFlightStyle.Dragonfly,18*.60f,18,.62f,0,-1);
            var leaving=WildlifeMotion.Heading(a,b,c,d,WildlifeFlightStyle.Dragonfly,18*.68f,18,.62f,0,-1);
            Assert.That(Vector3.Dot(paused,leaving),Is.GreaterThan(.90f),"Hover must not turn 180 degrees back towards the unmodified Bezier.");
        }
        [Test] public void BirdRoutesVaryTheirCurvatureSpeedAndPerspectiveWithoutClockDrift()
        {
            for(int visit=0;visit<80;visit++){
                float seed=WildlifeMotion.Hash(visit*173+19);
                WildlifeMotion.Route(visit%2==0?-1:1,seed,true,false,out var a,out var b,out var c,out var d);
                float previous=0;
                for(int frame=0;frame<=240;frame++){
                    float t=frame/240f,travel=WildlifeMotion.Travel(WildlifeFlightStyle.SmallBird,t,seed);
                    Assert.That(travel,Is.GreaterThanOrEqualTo(previous));previous=travel;
                    var position=WildlifeMotion.Path(a,b,c,d,WildlifeFlightStyle.SmallBird,t*8,8,seed,.004f,0,1);
                    Assert.That(position,Is.EqualTo(WildlifeMotion.Path(a,b,c,d,WildlifeFlightStyle.SmallBird,t*8,8,seed,.004f,0,1)));
                    Assert.That(float.IsNaN(position.x)||float.IsInfinity(position.y),Is.False);
                }
                Assert.That(Mathf.Abs(WildlifeMotion.Depth(0,seed)-WildlifeMotion.Depth(1,seed)),Is.GreaterThan(.7f));
                float curvature=0;
                for(int sample=1;sample<10;sample++){float t=sample/10f;curvature=Mathf.Max(curvature,Vector3.Distance(WildlifeMotion.Bezier(a,b,c,d,t),Vector3.Lerp(a,d,t)));}
                Assert.That(curvature,Is.GreaterThan(.02f));
            }
        }

        [Test] public void SharedHookBodiesSettleWithContactsAndReactToAnImpulse()
        {
            var bodies=new HangingPropBody[3];
            for(int i=0;i<bodies.Length;i++){
                bodies[i]=new HangingPropBody{Anchor=Vector2.zero,Length=100+i*5,Offset=new Vector2(0,-20),HalfSize=new Vector2(20,18),Bounds=Rect.MinMaxRect(-180,-210,180,-2)};
                bodies[i].Initialize(i*137+19);
            }
            for(int frame=0;frame<480;frame++){
                foreach(var body in bodies)body.Integrate(1f/120,new Vector2(frame<240?0:70,-650));
                for(int iteration=0;iteration<16;iteration++){
                    foreach(var body in bodies)body.Constrain();
                    for(int i=0;i<bodies.Length;i++)for(int j=i+1;j<bodies.Length;j++)HangingPropBody.Contact(bodies[i],bodies[j]);
                }
                if(frame>100)for(int i=0;i<bodies.Length;i++)for(int j=i+1;j<bodies.Length;j++){
                    var delta=bodies[i].Center-bodies[j].Center;var extent=bodies[i].Extent+bodies[j].Extent;
                    Assert.That(Mathf.Abs(delta.x)>=extent.x-.1f||Mathf.Abs(delta.y)>=extent.y-.1f,Is.True,"Contact bodies must not interpenetrate");
                }
            }
            Assert.That(bodies[0].Velocity.sqrMagnitude+bodies[1].Velocity.sqrMagnitude+bodies[2].Velocity.sqrMagnitude,Is.GreaterThan(.01f));
            foreach(var body in bodies){Assert.That(Vector2.Distance(body.Position,body.Anchor),Is.LessThanOrEqualTo(body.Length+1));Assert.That(float.IsNaN(body.Position.x),Is.False);}
        }
        [TestCase(WildlifeFlightStyle.SmallBird,4f)]
        [TestCase(WildlifeFlightStyle.SoaringBird,1.6f)]
        public void BirdsAlternateFlapsAndStableGlidesWithoutAJump(WildlifeFlightStyle style,float rate)
        {
            bool flap=false,glide=false;float min=100,max=-100;
            for(int step=0;step<1200;step++) {
                float age=step*.01f;var pose=WildlifeMotion.Pose(style,age,rate,.37f);
                if(pose.Gliding){glide=true;Assert.That(Mathf.Abs(pose.NearWing),Is.LessThanOrEqualTo(2.01f));}
                else{flap=true;min=Mathf.Min(min,pose.NearWing);max=Mathf.Max(max,pose.NearWing);}
                if(step>0) {
                    var before=WildlifeMotion.Pose(style,age-.0001f,rate,.37f);
                    Assert.That(Mathf.Abs(pose.NearWing-before.NearWing),Is.LessThan(.4f),"No wing snap at burst/glide boundaries.");
                }
            }
            Assert.That(flap&&glide,Is.True);Assert.That(max-min,Is.GreaterThan(80));
        }

        [Test] public void DragonflyHoversAndDartsWithContinuousAcceleration()
        {
            var style=WildlifeFlightStyle.Dragonfly;
            Assert.That(WildlifeMotion.Travel(style,.24f,.3f),Is.EqualTo(WildlifeMotion.Travel(style,.29f,.3f)));
            Assert.That(WildlifeMotion.Travel(style,.55f,.3f),Is.EqualTo(WildlifeMotion.Travel(style,.61f,.3f)));
            Assert.That(WildlifeMotion.Travel(style,.46f,.3f)-WildlifeMotion.Travel(style,.32f,.3f),Is.GreaterThan(.25f));
            float previous=0;
            for(int step=0;step<=1000;step++) {
                float value=WildlifeMotion.Travel(style,step/1000f,.3f);
                Assert.That(value,Is.GreaterThanOrEqualTo(previous));Assert.That(value-previous,Is.LessThan(.004f));previous=value;
            }
            Assert.That(previous,Is.EqualTo(1));
        }

        [Test] public void NightMothLoopsNearLightAndFrozenClockRetainsItsPose()
        {
            var a=new Vector3(.59f,.27f);var b=new Vector3(.47f,.28f);var c=new Vector3(.47f,-.13f);var d=new Vector3(.58f,-.25f);
            float low=1,high=-1;
            for(int step=30;step<=80;step++) {
                float age=step*.1f;
                var p=WildlifeMotion.Path(a,b,c,d,WildlifeFlightStyle.Moth,age,11,.3f,.006f,8,1);
                Assert.That(p,Is.EqualTo(WildlifeMotion.Path(a,b,c,d,WildlifeFlightStyle.Moth,age,11,.3f,.006f,8,1)));
                Assert.That(p.x,Is.InRange(.45f,.48f));low=Mathf.Min(low,p.y);high=Mathf.Max(high,p.y);
            }
            Assert.That(high-low,Is.GreaterThan(.035f));
            var pose=WildlifeMotion.Pose(WildlifeFlightStyle.Moth,4.2f,1.15f,.3f);
            var again=WildlifeMotion.Pose(WildlifeFlightStyle.Moth,4.2f,1.15f,.3f);
            Assert.That(again.NearWing,Is.EqualTo(pose.NearWing));Assert.That(again.FarWing,Is.EqualTo(pose.FarWing));
        }
    }
}
