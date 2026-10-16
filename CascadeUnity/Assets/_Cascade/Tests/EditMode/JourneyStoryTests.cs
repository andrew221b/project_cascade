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
            Assert.That(Enumerable.Range(0,9).Select(JourneyStory.WorldLevels),Is.All.EqualTo(60));
            foreach(int world in Enumerable.Range(0,JourneyStory.WorldCount)){
                var islands=Enumerable.Range(0,5).SelectMany(i=>Enumerable.Range(JourneyStory.IslandFirst(world,i),JourneyStory.IslandLevels(world,i))).ToArray();
                Assert.That(islands,Is.EqualTo(Enumerable.Range(JourneyStory.WorldFirst(world),JourneyStory.WorldLevels(world))));
                foreach(int i in Enumerable.Range(0,5)){
                    Assert.That(JourneyStory.IslandLevels(world,i),Is.EqualTo(12));
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
            Assert.That(JourneyStory.Zone(479),Is.EqualTo(7));
        }
        [Test] public void ExpandedRouteKeepsExistingPageAnchorsAndNightWorld()
        {
            Assert.That(Enumerable.Range(0,9).Select(JourneyStory.WorldFirst),Is.EqualTo(Enumerable.Range(0,9).Select(w=>w*60)));
            var anchors=new[]{0,5,10,15,20,25,30,35,40,45,50,55,60,64,69,74,79,84,88,93,98,103,108,112,117,122,127,132,136,141,146,151,156,160,165,170,175,180};
            Assert.That(Enumerable.Range(0,JourneyStory.PageCount).Select(p=>JourneyStory.NodeIds[JourneyStory.FirstStop(p)]),Is.EqualTo(anchors.Select(n=>"C"+(n+1).ToString("D2"))));
            Assert.That(JourneyStory.WorldCount,Is.EqualTo(9));Assert.That(JourneyStory.WorldFirst(8),Is.EqualTo(480));
            Assert.That(JourneyStory.WorldLevels(8),Is.EqualTo(60));Assert.That(JourneyStory.Zone(480),Is.EqualTo(8));
            Assert.That(JourneyStory.MapAsset(8),Is.EqualTo("StarObservatoryMap"));Assert.That(JourneyStory.Portrait(8),Is.EqualTo("Aster"));
            Assert.That(JourneyStory.Zone(539),Is.EqualTo(8));Assert.That(JourneyStory.Task(37),Is.EqualTo("Open the night travellers' route"));
        }
    }
}
