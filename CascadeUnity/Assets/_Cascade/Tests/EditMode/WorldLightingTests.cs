using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Tests
{
    public sealed class WorldLightingTests
    {
        [Test]
        public void MoonComesFromUpperLeftAndCastsLiftedShadowToRight()
        {
            var moon=WorldLighting.Evaluate(8,false,0,false);
            Assert.That(moon.Direction.x,Is.LessThan(0));Assert.That(moon.Direction.y,Is.GreaterThan(0));
            Assert.That(moon.Direction.z,Is.LessThan(0));
            var a=moon.ShadowOffset(.2f);var b=moon.ShadowOffset(.4f);
            Assert.That(a.x,Is.GreaterThan(0));Assert.That(b.x,Is.EqualTo(a.x*2).Within(.00001f));
            Assert.That(moon.ShadowOffset(-1),Is.EqualTo(Vector3.zero));
        }
        [Test]
        public void EveryWorldHasSkyFillGroundBounceAndBoundedCloudTransmission()
        {
            for(int world=0;world<9;world++){
                var still=WorldLighting.Evaluate(world,false,0,false);
                Assert.That(still.Direction.magnitude,Is.EqualTo(1).Within(.0001));
                Assert.That(still.Direction.y,Is.GreaterThan(.25f));
                Assert.That(still.Ambient(Vector3.up).maxColorComponent,Is.GreaterThan(still.Ambient(Vector3.down).maxColorComponent));
                for(int t=0;t<60;t++){
                    var moving=WorldLighting.Evaluate(world,false,t*2f,true);
                    Assert.That(moving.Direction,Is.EqualTo(still.Direction));
                    Assert.That(moving.Transmission,Is.InRange(.80f,1f));
                    Assert.That(moving.Intensity,Is.EqualTo(still.Intensity));
                    Assert.That(WorldLighting.Evaluate(world,false,t*2f,false).Transmission,Is.EqualTo(1f));
                }
            }
        }
        [Test]
        public void WorkshopUsesSameWarmLampAfterEveryWorld()
        {
            var expected=WorldLighting.Evaluate(0,true,12);
            for(int w=1;w<9;w++){
                var actual=WorldLighting.Evaluate(w,true,12,false);
                Assert.That(actual.Direction,Is.EqualTo(expected.Direction));Assert.That(actual.Key,Is.EqualTo(expected.Key));
                Assert.That(actual.Key.r,Is.GreaterThan(actual.Key.b));
            }
        }
        [Test]
        public void RoundedCapsKeepSourceOutlineAndLeaveOriginalAssetsUntouched()
        {
            foreach(var original in ToyArt.Load().shapes){
                var before=original.vertices;var copy=ToySurfaceGeometry.RoundFace(original);
                try{
                    Assert.That(copy.bounds.size.x,Is.EqualTo(original.bounds.size.x).Within(.0001));
                    Assert.That(copy.bounds.size.y,Is.EqualTo(original.bounds.size.y).Within(.0001));
                    Assert.That(copy.bounds.min.z,Is.LessThan(original.bounds.min.z));
                    Assert.That(copy.vertexCount,Is.GreaterThan(original.vertexCount));
                    Assert.That(original.vertices,Is.EqualTo(before));
                    Assert.That(copy.normals[before.Length-2].z,Is.LessThan(-.95f));
                }finally{Object.DestroyImmediate(copy);}
            }
        }
        [Test]
        public void ChangingPhaseReusesEnvironmentAndDisposalRestoresRenderSettings()
        {
            var oldProbe=RenderSettings.ambientProbe;var oldReflection=RenderSettings.customReflectionTexture;
            var oldMode=RenderSettings.ambientMode;var oldSun=RenderSettings.sun;
            var go=new GameObject("Lighting lifecycle test");var light=go.AddComponent<Light>();
            var rig=new WorldLighting.Rig(light);
            try{
                rig.Paint(8,false,0,true);var first=RenderSettings.customReflectionTexture;
                Assert.That(first,Is.Not.Null);Assert.That(RenderSettings.ambientMode,Is.EqualTo(AmbientMode.Trilight));
                rig.Paint(8,false,21,true);Assert.That(RenderSettings.customReflectionTexture,Is.SameAs(first));
                Assert.That(Vector3.Distance(light.transform.forward,-rig.Current.Direction),Is.LessThan(.0001f));
                rig.Paint(8,true,21,true);Assert.That(RenderSettings.customReflectionTexture,Is.Not.SameAs(first));
                rig.Dispose();Assert.That(RenderSettings.customReflectionTexture,Is.SameAs(oldReflection));
                Assert.That(RenderSettings.ambientMode,Is.EqualTo(oldMode));Assert.That(RenderSettings.sun,Is.SameAs(oldSun));
                for(int c=0;c<3;c++)for(int k=0;k<9;k++)Assert.That(RenderSettings.ambientProbe[c,k],Is.EqualTo(oldProbe[c,k]));
            }finally{rig.Dispose();Object.DestroyImmediate(go);}
        }
    }
}
