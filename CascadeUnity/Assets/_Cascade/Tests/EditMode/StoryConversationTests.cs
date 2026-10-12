using System.Linq;
using Cascade.Content;
using Cascade.Persistence;
using NUnit.Framework;
namespace Cascade.Tests
{
    public sealed class StoryConversationTests
    {
        [Test] public void IslandScenesCoverTheFullJourneyWithDistinctReceiptsAndRealExchanges()
        {
            var scenes=Enumerable.Range(0,JourneyStory.WorldCount).SelectMany(w=>Enumerable.Range(0,5).Select(i=>StoryConversations.Island(w,i))).ToArray();
            Assert.That(scenes.Length,Is.EqualTo(45));Assert.That(scenes.Select(s=>s.Receipt).Distinct().Count(),Is.EqualTo(45));
            foreach(var scene in scenes){Assert.That(scene.Lines.Length,Is.EqualTo(2));Assert.That(scene.Lines[0].Speaker,Is.Not.EqualTo("Mika"));Assert.That(scene.Lines[1].Speaker,Is.EqualTo("Mika"));Assert.That(scene.Lines.Select(l=>l.Text),Is.All.Not.Empty);}
            Assert.That(scenes.SelectMany(s=>s.Lines).Select(l=>l.Text).Distinct().Count(),Is.EqualTo(90));
            Assert.That(Enumerable.Range(0,JourneyStory.WorldCount).Select(StoryConversations.Resident).Distinct().Count(),Is.EqualTo(9));
        }
        [Test] public void CheckpointsOnlyInterruptBetweenSecondAndThirdStageAndDoNotCollideWithIslandOrIntroIds()
        {
            foreach(int stop in Enumerable.Range(0,JourneyStory.TotalLevels)){
                Assert.That(StoryConversations.HasCheckpoint(stop,0),Is.False);Assert.That(StoryConversations.HasCheckpoint(stop,2),Is.False);
                int node=int.Parse(JourneyStory.NodeIds[stop].Substring(1));
                Assert.That(StoryConversations.HasCheckpoint(stop,1),Is.EqualTo(stop==JourneyStory.WorldFirst(JourneyStory.Zone(stop))+1));
                var scene=StoryConversations.Checkpoint(stop);Assert.That(scene.Receipt,Is.EqualTo(4000+node-1));
                Assert.That(StoryConversations.BeforeLevel(stop).Receipt,Is.LessThan(JourneyStory.PageCount));
            }
            Assert.That(StoryConversations.HasCheckpoint(-1,1),Is.False);Assert.That(StoryConversations.HasCheckpoint(JourneyStory.TotalLevels,1),Is.False);
        }
        [Test] public void KnownResidentsGiveCurrentIslandGoalInsteadOfRepeatingTheirIntroduction()
        {
            foreach(int stop in Enumerable.Range(0,JourneyStory.TotalLevels)){
                var meeting=StoryConversations.Visit(stop,alreadyMet:true);Assert.That(meeting.Lines.Length,Is.EqualTo(2));
                Assert.That(meeting.Lines.Select(line=>line.Goal),Does.Contain(StoryConversations.Task(stop)));Assert.That(meeting.Lines[1].Speaker,Is.EqualTo("Mika"));
                Assert.That(StoryConversations.BeforeLevel(stop).Lines.Length,Is.EqualTo(2));
                Assert.That(StoryConversations.BeforeLevel(stop).Receipt,Is.EqualTo(StoryConversations.BeforeLevel(JourneyStory.WorldFirst(JourneyStory.Zone(stop))).Receipt));
            }
        }
        [Test] public void ReadingInOneModeDoesNotConsumeStoryInAnotherMode()
        {
            var p=new PlayerProfile();p.HeardStory.Add(2000);p.HeardStory.Add(4001);p.SelectJourney("Words");
            Assert.That(p.HeardStory,Is.Empty);p.HeardStory.Add(2001);p.SelectJourney("Mixed");Assert.That(p.HeardStory,Is.Empty);
            p.SelectJourney("Shapes");Assert.That(p.HeardStory,Is.EquivalentTo(new[]{2000,4001}));p.SelectJourney("Words");Assert.That(p.HeardStory,Is.EqualTo(new[]{2001}));
        }
        [Test] public void MidIslandScenesCoverEveryIslandWithoutReplacingPublishedLaterAsides()
        {
            Assert.That(StoryConversations.HasLevelAside(-1),Is.False);
            Assert.That(StoryConversations.HasLevelAside(JourneyStory.TotalLevels),Is.False);
            var midpoints=Enumerable.Range(0,JourneyStory.WorldCount).SelectMany(w=>Enumerable.Range(0,5).Select(i=>JourneyStory.IslandFirst(w,i)+5)).ToArray();
            foreach(int stop in midpoints){
                Assert.That(StoryConversations.HasLevelAside(stop),Is.True);
                Assert.That(StoryConversations.HasIslandProgress(stop-1),Is.False);
                Assert.That(StoryConversations.HasIslandProgress(stop+1),Is.False);
                var scene=StoryConversations.LevelAside(stop);
                Assert.That(scene.Lines.Length,Is.EqualTo(2));
                Assert.That(scene.Lines[0].Speaker,Is.EqualTo(StoryConversations.Resident(JourneyStory.Zone(stop))));
                Assert.That(scene.Lines[1].Speaker,Is.EqualTo("Mika"));
                Assert.That(scene.Lines.All(line=>line.Text.Length>0&&line.Text.Length<=105),Is.True);
                Assert.That(scene.Receipt,Is.EqualTo(7000+int.Parse(JourneyStory.NodeIds[stop].Substring(1))-1));
            }
            foreach(int world in Enumerable.Range(0,JourneyStory.WorldCount)){
                int oldStop=JourneyStory.IslandFirst(world,3)+1;
                Assert.That(StoryConversations.HasLevelAside(oldStop),Is.True);
                Assert.That(StoryConversations.HasIslandProgress(oldStop),Is.False);
            }
            var scenes=Enumerable.Range(0,JourneyStory.TotalLevels).Where(StoryConversations.HasLevelAside).Select(StoryConversations.LevelAside).ToArray();
            Assert.That(scenes.Length,Is.EqualTo(54));
            Assert.That(scenes.Select(s=>s.Receipt).Distinct().Count(),Is.EqualTo(54));
            Assert.That(midpoints.SelectMany(stop=>StoryConversations.LevelAside(stop).Lines).Select(line=>line.Text).Distinct().Count(),Is.EqualTo(90));
        }
    }
}
