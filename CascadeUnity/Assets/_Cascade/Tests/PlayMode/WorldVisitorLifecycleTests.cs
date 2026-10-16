using System.Collections;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Cascade.Tests
{
    public sealed class WorldVisitorLifecycleTests
    {
        [UnityTest] public IEnumerator VisitorsUseEightPhasesStableClockAndQualityLimits()
        {
            var owner = new GameObject("Isolated world visitors");
            var cameraObject = new GameObject("Visitor test camera");
            try {
                var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true;
                camera.orthographicSize = 5; camera.aspect = 390f / 844; camera.transform.position = Vector3.back * 10;
                var visitors = owner.AddComponent<WorldLifeExtraView>(); visitors.Initialize(ToyArt.Load(), camera);
                Assert.That(visitors.Ready, Is.True);
                var renderers = owner.GetComponentsInChildren<MeshRenderer>();
                Assert.That(renderers.Length, Is.EqualTo(21));
                visitors.Initialize(ToyArt.Load(), camera);
                Assert.That(owner.GetComponentsInChildren<MeshRenderer>().Length, Is.EqualTo(21), "Reinitialization must preserve the side/rear part pool.");
                var poses = new int[9]; float visiblePhase = -1;
                for (int zone = 0; zone < 9; zone++) {
                    // Sample at 60 fps: 20 fps aliases a 4 Hz wing cycle to only five poses.
                    for (int step = 0; step < 10800; step++) {
                        float phase = step / 60f;
                        visitors.Paint(phase, zone, 0, true, false, 2, true);
                        Assert.That(visitors.ActiveCreatureCount, Is.InRange(0, 1), "Visits must not turn into a swarm.");
                        if (visitors.SpeciesAt(0) >= 0) {
                            Assert.That(visitors.SpeciesAt(0), Is.EqualTo(zone==6?9:zone==8?10:zone==0||zone==2?11:zone));
                            poses[zone] |= 1 << visitors.FrameAt(0);
                            if (zone == 0) visiblePhase = phase;
                        }
                        foreach (var renderer in renderers) if (renderer.enabled) {
                            var min = camera.WorldToViewportPoint(renderer.bounds.min);
                            var max = camera.WorldToViewportPoint(renderer.bounds.max);
                            Assert.That(max.x <= .1051f || min.x >= .8949f, Is.True,
                                "The full map silhouette must remain in an outside gutter.");
                        }
                    }
                    Assert.That(poses[zone], Is.EqualTo(255), $"World {zone}: visitor must sample the eight wing phases.");
                }
                Assert.That(visiblePhase, Is.GreaterThanOrEqualTo(0));
                visitors.Paint(visiblePhase, 0, 0, true, false, 2, true);
                var body = visitors.BodyPositionAt(0); int frame = visitors.FrameAt(0), count = visitors.ActiveCreatureCount;
                visitors.Paint(visiblePhase, 0, 0, true, false, 2, true);
                Assert.That(visitors.BodyPositionAt(0), Is.EqualTo(body)); Assert.That(visitors.FrameAt(0), Is.EqualTo(frame));
                Assert.That(visitors.ActiveCreatureCount, Is.EqualTo(count), "A stopped presentation phase must be stable.");
                for (int step = 0; step < 120; step++) {
                    visitors.Paint(step * .5f, 2, 7, true, false, 1, false);
                    Assert.That(visitors.ActiveCreatureCount, Is.InRange(0, 1));
                }
                visitors.Paint(visiblePhase, 0, 0, true, true, 2, true);
                AssertHidden(visitors, renderers);
                visitors.Paint(visiblePhase, 0, 0, true, false, 0, true);
                AssertHidden(visitors, renderers);
                visitors.Paint(float.NaN, 0, 0, true, false, 2, true);
                AssertHidden(visitors, renderers);
                visitors.Paint(visiblePhase, 0, 0, true, false, 2, true); visitors.enabled = false;
                AssertHidden(visitors, renderers);
            } finally { Object.Destroy(owner); Object.Destroy(cameraObject); }
            yield return null;
        }

        [UnityTest] public IEnumerator StageAndSceneryVaryVisitsWithoutMovingTheirBodyPivot()
        {
            var owner = new GameObject("Isolated stage visitors");
            var cameraObject = new GameObject("Stage visitor test camera");
            var replacementObject = new GameObject("Replacement visitor camera");
            try {
                var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true;
                camera.orthographicSize = 5; camera.aspect = .6f; camera.transform.position = Vector3.back * 10;
                var visitors = owner.AddComponent<WorldLifeExtraView>(); visitors.Initialize(ToyArt.Load(), camera);
                Assert.That(visitors.Ready, Is.True);
                bool stageChanged = false, islandChanged = false, foundBandFlight = false;
                var renderers = owner.GetComponentsInChildren<MeshRenderer>();
                for (int step = 0; step < 1800; step++) {
                    float phase = step * .1f;
                    visitors.Paint(phase, 0, 0, true, false, 2, false, null, new Vector2(1.6f, 2.8f));
                    bool firstVisible = visitors.SpeciesAt(0) >= 0; var first = visitors.BodyPositionAt(0);
                    foreach (var renderer in renderers) if (renderer.enabled) {
                        var center = camera.WorldToViewportPoint(renderer.bounds.center);
                        if (center.x > .15f && center.x < .85f) {
                            foundBandFlight = true;
                            Assert.That(renderer.bounds.min.y, Is.GreaterThanOrEqualTo(1.599f));
                            Assert.That(renderer.bounds.max.y, Is.LessThanOrEqualTo(2.801f));
                        }
                    }
                    visitors.Paint(phase, 0, 1, true, false, 2, false, null, new Vector2(1.6f, 2.8f));
                    stageChanged |= firstVisible != (visitors.SpeciesAt(0) >= 0)
                        || firstVisible && visitors.SpeciesAt(0) >= 0 && Vector3.Distance(first, visitors.BodyPositionAt(0)) > .01f;
                    visitors.Paint(phase, 0, 0, true, false, 2, false, null, new Vector2(1.6f, 2.8f), 1);
                    islandChanged |= firstVisible != (visitors.SpeciesAt(0) >= 0)
                        || firstVisible && visitors.SpeciesAt(0) >= 0 && Vector3.Distance(first, visitors.BodyPositionAt(0)) > .01f;
                }
                Assert.That(stageChanged, Is.True, "A different stage must change the schedule or route.");
                Assert.That(islandChanged, Is.True, "A different island must change the schedule or route.");
                // Keep one context alive for a complete entrance. Rebinding three times
                // every sample deliberately admits only the offscreen start of each visit.
                for(int step=0;step<1800;step++){
                    visitors.Paint(step*.1f,0,0,true,false,2,false,null,new Vector2(1.6f,2.8f));
                    foreach(var renderer in renderers)if(renderer.enabled){
                        var center=camera.WorldToViewportPoint(renderer.bounds.center);
                        if(center.x>.15f&&center.x<.85f){foundBandFlight=true;Assert.That(renderer.bounds.min.y,Is.GreaterThanOrEqualTo(1.599f));Assert.That(renderer.bounds.max.y,Is.LessThanOrEqualTo(2.801f));}
                    }
                }
                Assert.That(foundBandFlight, Is.True, "The bird must make an upper flight while respecting the reserved band.");
                string[] scenery = { "petal_drift", "firefly_dance", "butterfly_garden", "swallow_flock",
                    "dragonfly_stream", "lantern_flight", "snow_wings", "comet_trails" };
                int[,] allowed = { {0,2}, {7,8}, {5,2}, {3,0}, {1,4}, {5,7}, {6,6}, {8,8} };
                for (int selection = 0; selection < scenery.Length; selection++) {
                    bool seen = false;
                    for (int step = 0; step < 160; step++) {
                        visitors.Paint(step * .5f, 0, 1, true, false, 1, true, scenery[selection]);
                        int row = visitors.SpeciesAt(0); if (row < 0) continue;
                        seen = true; Assert.That(row == allowed[selection,0] || row == allowed[selection,1], Is.True);
                    }
                    Assert.That(seen, Is.True, "Equipped scenery must remain visible at low quality.");
                }
                var replacement = replacementObject.AddComponent<Camera>(); replacement.orthographic = true;
                replacement.orthographicSize = 5; replacement.aspect = .6f; replacement.transform.position = new Vector3(20, 0, -10);
                visitors.Initialize(ToyArt.Load(), replacement);
                bool rebound = false;
                for (int step = 0; step < 120; step++) {
                    visitors.Paint(step * .5f, 0, 0, true, false, 1, true);
                    if (visitors.SpeciesAt(0) >= 0) { rebound = true; Assert.That(visitors.BodyPositionAt(0).x, Is.GreaterThan(15)); }
                }
                Assert.That(rebound, Is.True);
            } finally { Object.Destroy(owner); Object.Destroy(cameraObject); Object.Destroy(replacementObject); }
            yield return null;
        }

        static void AssertHidden(WorldLifeExtraView visitors, MeshRenderer[] renderers)
        {
            Assert.That(visitors.ActiveCreatureCount, Is.Zero);
            foreach (var renderer in renderers) Assert.That(renderer.enabled, Is.False);
        }
    }
}
