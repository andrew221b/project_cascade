using System.Linq;
using Cascade.Content;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class JourneyStoryTests
    {
        [Test] public void WholeWorldMapsCoverEveryPublishedLevelAndEachIslandHasMultipleLevels()
        {
            var stops=Enumerable.Range(0,JourneyStory.WorldCount).SelectMany(w=>Enumerable.Range(JourneyStory.WorldFirst(w),JourneyStory.WorldLevels(w))).ToArray();
            Assert.That(stops,Is.EqualTo(Enumerable.Range(0,JourneyStory.TotalLevels)));
            Assert.That(Enumerable.Range(0,8).Select(JourneyStory.WorldLevels),Is.EqualTo(new[]{15,15,10,24,24,24,24,24}));
            foreach(int world in Enumerable.Range(0,JourneyStory.WorldCount)){
                var islands=Enumerable.Range(0,5).SelectMany(i=>Enumerable.Range(JourneyStory.IslandFirst(world,i),JourneyStory.IslandLevels(world,i))).ToArray();
                Assert.That(islands,Is.EqualTo(Enumerable.Range(JourneyStory.WorldFirst(world),JourneyStory.WorldLevels(world))));
                foreach(int i in Enumerable.Range(0,5)){
                    Assert.That(JourneyStory.IslandLevels(world,i),Is.InRange(2,5));
                    foreach(int stop in Enumerable.Range(JourneyStory.IslandFirst(world,i),JourneyStory.IslandLevels(world,i)))Assert.That(JourneyStory.IslandFor(stop),Is.EqualTo(i));
                }
            }
        }
        [Test] public void EveryPublishedStopBelongsToOnePageAndRegionWithoutCrossingBorders()
        {
            Assert.That(JourneyStory.PageCount,Is.EqualTo(38));
            var stops=Enumerable.Range(0,JourneyStory.PageCount).SelectMany(p=>Enumerable.Range(JourneyStory.FirstStop(p),JourneyStory.StopsOnPage(p))).ToArray();
            Assert.That(stops,Is.EqualTo(Enumerable.Range(0,JourneyStory.TotalLevels)));
            for(int stop=0;stop<JourneyStory.TotalLevels;stop++){
                int page=JourneyStory.PageFor(stop),first=JourneyStory.FirstStop(page);
                Assert.That(stop,Is.InRange(first,first+JourneyStory.StopsOnPage(page)-1));
                Assert.That(JourneyStory.Zone(stop),Is.EqualTo(JourneyStory.Zone(first)));
                Assert.That(JourneyStory.Lines(page).Length,Is.EqualTo(2));
                Assert.That(JourneyStory.Task(page),Is.Not.Empty);
            }
            Assert.That(JourneyStory.Zone(159),Is.EqualTo(7));
        }
        [Test] public void NightEpilogueAppendsWithoutMovingPublishedWorldsOrPages()
        {
            Assert.That(Enumerable.Range(0,8).Select(JourneyStory.WorldFirst),Is.EqualTo(new[]{0,15,30,40,64,88,112,136}));
            Assert.That(Enumerable.Range(0,33).Select(JourneyStory.FirstStop),Is.EqualTo(new[]{0,5,10,15,20,25,30,35,40,45,50,55,60,64,69,74,79,84,88,93,98,103,108,112,117,122,127,132,136,141,146,151,156}));
            Assert.That(JourneyStory.WorldCount,Is.EqualTo(9));Assert.That(JourneyStory.WorldFirst(8),Is.EqualTo(160));
            Assert.That(JourneyStory.WorldLevels(8),Is.EqualTo(24));Assert.That(JourneyStory.Zone(160),Is.EqualTo(8));
            Assert.That(JourneyStory.MapAsset(8),Is.EqualTo("StarObservatoryMap"));Assert.That(JourneyStory.Portrait(8),Is.EqualTo("Aster"));
            Assert.That(JourneyStory.Zone(183),Is.EqualTo(8));Assert.That(JourneyStory.Task(37),Is.EqualTo("Open the night travellers' route"));
        }
    }
}
