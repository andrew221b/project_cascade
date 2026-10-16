using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
namespace Cascade.Tests
{
    public sealed class SkyPerspectiveTests
    {
        [Test] public void FleetBudgetSeparatesHeightsDepthAndStylesAcrossAllWorlds()
        {
            var seen=new bool[6];int pairs=0;
            for(int world=0;world<9;world++)for(float t=0;t<420;t+=.125f){
                int count=0;SkyFlightPose previous=default;
                for(int kind=0;kind<6;kind++){
                    if(!SkyFlightMotion.TryFleet(t,kind,world,2,out var pose))continue;seen[kind]=true;count++;
                    Assert.That(SkyFlightMotion.Width(kind,pose),Is.LessThan(.14f));
                    if(count==2){pairs++;Assert.That(Mathf.Abs(pose.Position.y-previous.Position.y),Is.GreaterThan(.25f));Assert.That(Mathf.Abs(pose.Proximity-previous.Proximity),Is.GreaterThan(.28f));}
                    previous=pose;
                }
                Assert.That(count,Is.LessThanOrEqualTo(2));int low=0;for(int kind=0;kind<6;kind++)if(SkyFlightMotion.TryFleet(t,kind,world,1,out _))low++;Assert.That(low,Is.LessThanOrEqualTo(1));
            }
            Assert.That(pairs,Is.GreaterThan(1000));Assert.That(seen,Is.All.True);
        }
        [Test] public void SpeciesKeepTheirBodyProportionsAtEverySharedDistance()
        {
            for(int i=0;i<=100;i++){
                float depth=i/100f;float bird=WildlifePerspective.BodySize(WildlifeSpecies.Swallow,720,depth);
                Assert.That(WildlifePerspective.BodySize(WildlifeSpecies.Bumblebee,720,depth),Is.LessThan(bird*.25f));
                Assert.That(WildlifePerspective.BodySize(WildlifeSpecies.Ladybird,720,depth),Is.LessThan(bird*.12f));
                Assert.That(WildlifePerspective.BodySize(WildlifeSpecies.BarnOwl,720,depth),Is.GreaterThan(bird*2));
            }
            Assert.That(WildlifePerspective.Scale(.85f)/WildlifePerspective.Scale(.15f),Is.GreaterThan(2.5f));
        }
    }
}
