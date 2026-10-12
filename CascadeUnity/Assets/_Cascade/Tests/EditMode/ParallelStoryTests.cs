using System;
using System.IO;
using System.Linq;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class ParallelStoryTests
    {
        [Test] public void SwitchingKeepsCharacterMemoryButDoesNotCopyLevelProgress()
        {
            var p=new PlayerProfile();int introduction=WorldPrologues.ForWorld(0).Receipt;
            p.HeardStory.Add(introduction);p.HeardStory.Add(StoryConversations.IslandOpening(0,0).Receipt);
            p.SelectJourney("Words");
            Assert.That(p.HeardStory,Is.Empty);Assert.That(p.RemembersStory(introduction),Is.True);
            Assert.That(p.Clears,Is.Empty);Assert.That(p.Attempt,Is.Null);
            int arrival=StoryConversations.ParallelSky(PuzzleMode.Words,0,false).Receipt;
            p.HeardStory.Add(arrival);p.SelectJourney("Mixed");
            Assert.That(p.RemembersStory(arrival),Is.True);Assert.That(p.RemembersStory(introduction),Is.True);
            p=JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(p));p.Validate();
            p.SelectJourney("Shapes");Assert.That(p.RemembersStory(arrival),Is.True);
            Assert.That(p.HeardStory,Does.Contain(introduction));Assert.That(p.Clears,Is.Empty);
        }
        [Test] public void NewPlayerStillHasNoStoryMemoryAndUnknownWorldIsNotSpoiled()
        {
            var p=new PlayerProfile();Assert.That(p.HasStoryMemory,Is.False);
            p.SelectJourney("Words");Assert.That(p.HasStoryMemory,Is.False);
            p.HeardStory.Add(WorldPrologues.ForWorld(0).Receipt);p.SelectJourney("Mixed");
            Assert.That(p.HasStoryMemory,Is.True);Assert.That(p.RemembersStory(WorldPrologues.ForWorld(1).Receipt),Is.False);
        }
        [Test] public void FirstVisitsAndReturnsAreDistinctLocalizedAndFitExistingSaves()
        {
            var keys=File.ReadAllLines(Path.Combine(ReferenceFixtures.PackageRoot,"Data","ui_localization.tsv"))
                .Skip(1).Where(l=>l.Length>0).Select(l=>l.Split('\t')).ToDictionary(c=>c[0]);
            var modes=(PuzzleMode[])Enum.GetValues(typeof(PuzzleMode));
            var scenes=modes.SelectMany(m=>new[]{StoryConversations.ParallelSky(m,0,false),StoryConversations.ParallelSky(m,0,true)}).ToArray();
            Assert.That(scenes.Select(s=>s.Receipt).Distinct().Count(),Is.EqualTo(6));
            foreach(var scene in scenes){
                var p=new PlayerProfile();p.HeardStory.Add(scene.Receipt);p.Validate();
                foreach(var line in scene.Lines){Assert.That(keys.ContainsKey(line.Text),Is.True,line.Text);Assert.That(keys[line.Text],Has.Length.EqualTo(4));Assert.That(keys[line.Text].All(t=>t.Length>0),Is.True);}
            }
            foreach(var mode in modes)foreach(int world in Enumerable.Range(0,JourneyStory.WorldCount)){
                var first=StoryConversations.ParallelSky(mode,world,false);var back=StoryConversations.ParallelSky(mode,world,true);
                Assert.That(first.Lines.Length,Is.EqualTo(3));Assert.That(back.Lines.Length,Is.EqualTo(2));
                Assert.That(first.Lines[1].Speaker,Is.EqualTo(StoryConversations.Resident(world)));
                Assert.That(back.Lines[1].Speaker,Is.EqualTo(StoryConversations.Resident(world)));
                Assert.That(first.Lines.Select(l=>l.Text),Is.Not.EqualTo(back.Lines.Select(l=>l.Text)));
            }
        }
        [Test] public void CheckingMemoryCannotMutateAnArchivedJourneyOrMarkNewScenesRead()
        {
            var p=new PlayerProfile();p.HeardStory.Add(1000);p.SelectJourney("Words");
            string before=JsonConvert.SerializeObject(p);
            Assert.That(p.RemembersStory(1000),Is.True);Assert.That(p.RemembersStory(9001),Is.False);
            Assert.That(JsonConvert.SerializeObject(p),Is.EqualTo(before));
        }
    }
}
