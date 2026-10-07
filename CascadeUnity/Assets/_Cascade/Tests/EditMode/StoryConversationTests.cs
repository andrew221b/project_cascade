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
                Assert.That(StoryConversations.HasCheckpoint(stop,1),Is.EqualTo(stop%3==1));
                var scene=StoryConversations.Checkpoint(stop);Assert.That(scene.Receipt,Is.EqualTo(4000+stop));
                Assert.That(StoryConversations.BeforeLevel(stop).Receipt,Is.LessThan(JourneyStory.PageCount));
            }
            Assert.That(StoryConversations.HasCheckpoint(-1,1),Is.False);Assert.That(StoryConversations.HasCheckpoint(JourneyStory.TotalLevels,1),Is.False);
        }
        [Test] public void KnownResidentsGiveCurrentIslandGoalInsteadOfRepeatingTheirIntroduction()
        {
            foreach(int stop in Enumerable.Range(0,JourneyStory.TotalLevels)){
                var meeting=StoryConversations.Visit(stop,alreadyMet:true);Assert.That(meeting.Lines.Length,Is.EqualTo(2));
                Assert.That(meeting.Lines[0].Goal,Is.EqualTo(StoryConversations.Task(stop)));Assert.That(meeting.Lines[1].Speaker,Is.EqualTo("Mika"));
                Assert.That(StoryConversations.BeforeLevel(stop).Lines.Length,Is.EqualTo(3));
                Assert.That(StoryConversations.BeforeLevel(stop).Receipt,Is.EqualTo(StoryConversations.BeforeLevel(JourneyStory.WorldFirst(JourneyStory.Zone(stop))).Receipt));
            }
        }
        [Test] public void ReadingInOneModeDoesNotConsumeStoryInAnotherMode()
        {
            var p=new PlayerProfile();p.HeardStory.Add(2000);p.HeardStory.Add(4001);p.SelectJourney("Words");
            Assert.That(p.HeardStory,Is.Empty);p.HeardStory.Add(2001);p.SelectJourney("Mixed");Assert.That(p.HeardStory,Is.Empty);
            p.SelectJourney("Shapes");Assert.That(p.HeardStory,Is.EquivalentTo(new[]{2000,4001}));p.SelectJourney("Words");Assert.That(p.HeardStory,Is.EqualTo(new[]{2001}));
        }
    }
}
