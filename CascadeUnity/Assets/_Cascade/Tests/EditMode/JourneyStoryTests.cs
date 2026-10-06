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
            Assert.That(stops,Is.EqualTo(Enumerable.Range(0,160)));
            Assert.That(Enumerable.Range(0,8).Select(JourneyStory.WorldLevels),Is.EqualTo(new[]{15,15,10,24,24,24,24,24}));
            foreach(int world in Enumerable.Range(0,8)){
                var islands=Enumerable.Range(0,5).SelectMany(i=>Enumerable.Range(JourneyStory.IslandFirst(world,i),JourneyStory.IslandLevels(world,i))).ToArray();
                Assert.That(islands,Is.EqualTo(Enumerable.Range(JourneyStory.WorldFirst(world),JourneyStory.WorldLevels(world))));
                foreach(int i in Enumerable.Range(0,5)){
                    Assert.That(JourneyStory.IslandLevels(world,i),Is.InRange(2,5));
                    foreach(int stop in Enumerable.Range(JourneyStory.IslandFirst(world,i),JourneyStory.IslandLevels(world,i)))Assert.That(JourneyStory.IslandFor(stop),Is.EqualTo(i));
                }
            }
        }
        [Test] public void All160StopsBelongToOnePageAndRegionWithoutCrossingBorders()
        {
            Assert.That(JourneyStory.PageCount,Is.EqualTo(33));
            var stops=Enumerable.Range(0,JourneyStory.PageCount).SelectMany(p=>Enumerable.Range(JourneyStory.FirstStop(p),JourneyStory.StopsOnPage(p))).ToArray();
            Assert.That(stops,Is.EqualTo(Enumerable.Range(0,160)));
            for(int stop=0;stop<160;stop++){
                int page=JourneyStory.PageFor(stop),first=JourneyStory.FirstStop(page);
                Assert.That(stop,Is.InRange(first,first+JourneyStory.StopsOnPage(page)-1));
                Assert.That(JourneyStory.Zone(stop),Is.EqualTo(JourneyStory.Zone(first)));
                Assert.That(JourneyStory.Lines(page).Length,Is.EqualTo(2));
                Assert.That(JourneyStory.Task(page),Is.Not.Empty);
            }
            Assert.That(JourneyStory.Zone(159),Is.EqualTo(7));
        }
    }
}
