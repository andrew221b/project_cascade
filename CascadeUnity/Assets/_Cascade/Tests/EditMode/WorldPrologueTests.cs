using System;
using System.IO;
using System.Linq;
using Cascade.Content;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class WorldPrologueTests
    {
        [Test] public void EveryWorldHasThreeAuthoredComicPagesAndItsOwnResident()
        {
            var comics=Enumerable.Range(0,JourneyStory.WorldCount).Select(WorldPrologues.ForWorld).ToArray();
            foreach(var comic in comics){
                Assert.That(comic.Pages.Length,Is.EqualTo(3));Assert.That(comic.Receipt,Is.EqualTo(6000+comic.World));
                Assert.That(comic.Pages.SelectMany(p=>p.Lines).Select(l=>l.Speaker),Does.Contain("Mika"));
                Assert.That(comic.Pages.SelectMany(p=>p.Lines).Select(l=>l.Speaker),Does.Contain(StoryConversations.Resident(comic.World)));
                foreach(var page in comic.Pages){Assert.That(page.Caption,Is.Not.Empty);Assert.That(page.Lines.Length,Is.EqualTo(2));Assert.That(page.Lines.Select(l=>l.Text),Is.All.Not.Empty);}
            }
            Assert.That(comics.SelectMany(c=>c.Pages).Select(p=>p.Caption).Distinct().Count(),Is.EqualTo(27));
            Assert.That(comics.SelectMany(c=>c.Pages).SelectMany(p=>p.Lines).Select(l=>l.Text).Distinct().Count(),Is.EqualTo(54));
            Assert.Throws<ArgumentOutOfRangeException>(()=>WorldPrologues.ForWorld(-1));
            Assert.Throws<ArgumentOutOfRangeException>(()=>WorldPrologues.ForWorld(JourneyStory.WorldCount));
        }
        [Test] public void EveryIslandGetsADistinctOpeningWithItsCurrentGoal()
        {
            var openings=Enumerable.Range(0,JourneyStory.WorldCount).SelectMany(w=>Enumerable.Range(0,5).Select(i=>StoryConversations.IslandOpening(w,i))).ToArray();
            Assert.That(openings.Length,Is.EqualTo(45));Assert.That(openings.SelectMany(s=>s.Lines).Select(l=>l.Text).Distinct().Count(),Is.EqualTo(90));
            foreach(int world in Enumerable.Range(0,JourneyStory.WorldCount))foreach(int island in Enumerable.Range(0,5)){
                var scene=StoryConversations.IslandOpening(world,island);
                Assert.That(scene.Receipt,Is.EqualTo(8000+world*10+island));Assert.That(scene.Lines[0].Speaker,Is.EqualTo(StoryConversations.Resident(world)));
                Assert.That(scene.Lines[1].Speaker,Is.EqualTo("Mika"));Assert.That(scene.Lines[1].Goal,Is.EqualTo(StoryConversations.Task(JourneyStory.IslandFirst(world,island))));
            }
            Assert.Throws<ArgumentOutOfRangeException>(()=>StoryConversations.IslandOpening(9,0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>StoryConversations.IslandOpening(0,5));
        }
        [Test] public void OccasionalLevelAsidesAndAllStoryReceiptsFitExistingSaveValidation()
        {
            var stops=Enumerable.Range(0,JourneyStory.TotalLevels).Where(StoryConversations.HasLevelAside).ToArray();
            Assert.That(stops.Length,Is.EqualTo(9));Assert.That(stops.Select(JourneyStory.Zone),Is.EqualTo(Enumerable.Range(0,9)));
            foreach(int stop in stops){
                Assert.That(stop,Is.EqualTo(JourneyStory.IslandFirst(JourneyStory.Zone(stop),3)+1));
                Assert.That(StoryConversations.LevelAside(stop).Lines.Length,Is.EqualTo(2));
                Assert.That(StoryConversations.LevelAside(stop).Receipt,Is.EqualTo(7000+stop));
            }
            var receipts=Enumerable.Range(0,JourneyStory.WorldCount).Select(w=>StoryConversations.BeforeLevel(JourneyStory.WorldFirst(w)).Receipt)
                .Concat(Enumerable.Range(0,JourneyStory.WorldCount).Select(w=>WorldPrologues.ForWorld(w).Receipt))
                .Concat(Enumerable.Range(0,JourneyStory.WorldCount).SelectMany(w=>Enumerable.Range(0,5).SelectMany(i=>new[]{StoryConversations.Island(w,i).Receipt,StoryConversations.IslandOpening(w,i).Receipt})))
                .Concat(stops.Select(s=>StoryConversations.LevelAside(s).Receipt))
                .Concat(Enumerable.Range(0,JourneyStory.TotalLevels).Where(s=>StoryConversations.HasCheckpoint(s,1)).Select(s=>StoryConversations.Checkpoint(s).Receipt)).ToArray();
            Assert.That(receipts.Distinct().Count(),Is.EqualTo(receipts.Length));Assert.That(receipts,Is.All.InRange(0,10000));
            Assert.That(StoryConversations.HasLevelAside(-1),Is.False);Assert.That(StoryConversations.HasLevelAside(184),Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(()=>StoryConversations.LevelAside(0));
        }
        [Test] public void EveryStoryKeyHasAnEnglishRussianAndUkrainianSourceEntry()
        {
            var rows=new[]{"ui_localization.tsv","star_story_localization.tsv"}.SelectMany(file=>File.ReadAllLines(Path.Combine(ReferenceFixtures.PackageRoot,"Data",file)).Skip(1)).Where(line=>!string.IsNullOrWhiteSpace(line)).Select(line=>line.Split('\t')).ToArray();
            Assert.That(rows.All(r=>r.Length==4&&r.All(c=>!string.IsNullOrEmpty(c))),Is.True);
            var keys=rows.Select(r=>r[0]).ToHashSet();
            foreach(int world in Enumerable.Range(0,JourneyStory.WorldCount)){
                Assert.That(keys,Does.Contain(StoryConversations.Resident(world)));Assert.That(keys,Does.Contain(JourneyStory.ZoneName(world)));
                foreach(var page in WorldPrologues.ForWorld(world).Pages){Assert.That(keys,Does.Contain(page.Caption));foreach(var line in page.Lines)Assert.That(keys,Does.Contain(line.Text));}
                foreach(int island in Enumerable.Range(0,5))foreach(var line in StoryConversations.IslandOpening(world,island).Lines){Assert.That(keys,Does.Contain(line.Text));if(line.Goal!=null)Assert.That(keys,Does.Contain(line.Goal));}
            }
            foreach(int stop in Enumerable.Range(0,JourneyStory.TotalLevels).Where(StoryConversations.HasLevelAside))foreach(var line in StoryConversations.LevelAside(stop).Lines)Assert.That(keys,Does.Contain(line.Text));
        }
    }
}
