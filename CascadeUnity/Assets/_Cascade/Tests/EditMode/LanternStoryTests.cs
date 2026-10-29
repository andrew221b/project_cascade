using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Content;
using Cascade.Presentation;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor.Localization;

namespace Cascade.Tests
{
    public sealed class LanternStoryTests
    {
        static JObject Source()=>JObject.Parse(File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data/story_lantern_oct16.json")));
        static IEnumerable<StoryLine> WorldLines(int world)
        {
            foreach(var p in WorldPrologues.ForWorld(world).Pages)foreach(var l in p.Lines)yield return l;
            for(int i=0;i<5;i++){
                foreach(var l in StoryConversations.IslandOpening(world,i).Lines)yield return l;
                foreach(var l in StoryConversations.IslandProgress(JourneyStory.IslandFirst(world,i)+5).Lines)yield return l;
                foreach(var l in StoryConversations.Island(world,i).Lines)yield return l;
            }
            foreach(var l in StoryConversations.Checkpoint(JourneyStory.WorldFirst(world)+1).Lines)yield return l;
            foreach(var l in StoryConversations.LevelAside(JourneyStory.IslandFirst(world,3)+1).Lines)yield return l;
        }
        [Test] public void AllNewStorySlotsReachTheActualLocaleTablesWithoutStaleOrEmptyValues()
        {
            var source=Source();var sections=(JObject)source["sections"];
            var collection=LocalizationEditorSettings.GetStringTableCollection(UiCopy.Table);
            int checkedValues=0;
            foreach(var section in ((JObject)source["keys"]).Properties()){
                var rows=(JArray)sections[section.Name];Assert.That(rows.Count,Is.EqualTo(((JArray)section.Value).Count));
                foreach(var row in rows){
                    string key=(string)row[0];int i=0;
                    foreach(string code in new[]{"en","ru","uk"}){
                        var table=collection.StringTables.Single(t=>t.LocaleIdentifier.Code==code);
                        var actual=table.GetEntry(key);Assert.That(actual,Is.Not.Null,key);
                        Assert.That(actual.Value,Is.EqualTo((string)row[i++]),code+" / "+key);checkedValues++;
                    }
                }
            }
            Assert.That(checkedValues,Is.EqualTo(1449));
            var keys=Enumerable.Range(0,9).SelectMany(WorldLines).Select(l=>l.Text).ToHashSet();
            var authored=new[]{"comics","openings","progress","payoffs","checkpoints","asides"}.SelectMany(n=>(JArray)sections[n]).Select(r=>(string)r[0]).ToArray();
            Assert.That(authored.Except(keys),Is.Empty,"Every mandatory authored turn is reachable through its existing story API");
        }
        [Test] public void RinHasOneDisplayNameAndKeepsTheExistingPortraitIdentity()
        {
            var collection=LocalizationEditorSettings.GetStringTableCollection(UiCopy.Table);var names=new Dictionary<string,string>{{"en","Rin"},{"ru","Рин"},{"uk","Рін"}};
            foreach(var table in collection.StringTables){
                Assert.That(table.GetEntry("Mika").Value,Is.EqualTo(names[table.LocaleIdentifier.Code]));
                Assert.That(table.Values.Select(e=>e.Value).Any(t=>t.Contains("Mika")||t.Contains("Мика")||t.Contains("Міка")),Is.False,"No player-facing old name");
            }
            Assert.That(WorldPrologues.ForWorld(0).Pages[0].Lines[1].Speaker,Is.EqualTo("Mika"),"Art identity is retained");
            foreach(int world in Enumerable.Range(0,9))Assert.That(JourneyStory.Speaker(world),Is.EqualTo(StoryConversations.Resident(world)));
        }
        [Test] public void TheConfessionIsReservedForTheFinalTwoIslandsAndTheReturnFollowsTheRescue()
        {
            var opening=WorldPrologues.ForWorld(0).Pages.SelectMany(p=>p.Lines).ToArray();
            Assert.That(opening[0].Text,Does.Contain("High Lantern is missing"));
            Assert.That(opening.Any(l=>l.Text.Contains("under the islands")),Is.True,"Ropes reach the lower lamps");
            Assert.That(opening.Any(l=>l.Text.Contains("same row")),Is.True,"Story describes the actual reorder-and-deliver mechanic");
            for(int world=0;world<8;world++)Assert.That(WorldLines(world).Any(l=>l.Text.Contains("I took it.")||l.Text.Contains("I wrote Ada's name")),Is.False,"No early confession in world "+world);
            var final=WorldPrologues.ForWorld(8).Pages.SelectMany(p=>p.Lines).ToArray();
            Assert.That(final.Any(l=>l.Text.Contains("I took it.")),Is.False,"The last world's prologue sets up the rescue, not the solution");
            for(int island=0;island<3;island++){
                var early=StoryConversations.IslandOpening(8,island).Lines
                    .Concat(StoryConversations.IslandProgress(JourneyStory.IslandFirst(8,island)+5).Lines)
                    .Concat(StoryConversations.Island(8,island).Lines);
                Assert.That(early.Any(l=>l.Text.Contains("I took it.")),Is.False,"No admission before the permit is checked");
            }
            var admission=StoryConversations.IslandOpening(8,3).Lines.Single(l=>l.Text.Contains("I took it."));
            Assert.That(admission.Speaker,Is.EqualTo("Aster"));
            Assert.That(JourneyStory.IslandFirst(8,3),Is.GreaterThanOrEqualTo(JourneyStory.TotalLevels*.95f));
            Assert.That(final.Last().Speaker,Is.EqualTo("Mika"),"Rin leads the rescue rather than the culprit");
            Assert.That(StoryConversations.Island(7,4).Lines.Any(l=>l.Text.Contains("High Lantern home")),Is.False,"Beacon remains empty before the finale");
            Assert.That(StoryConversations.Island(8,3).Lines[0].Text,Does.Contain("glider cleared"));
            Assert.That(StoryConversations.Island(8,4).Lines[0].Text,Does.Contain("High Lantern home"));
            Assert.That(StoryConversations.Island(8,4).Lines[0].Text,Does.Contain("repairs"),"Admission has consequences beyond a speech");
        }
        [Test] public void MissionGoalsAndOptionalScenesAreLocalisedWithoutSpoilingTheAdmission()
        {
            var source=Source();var collection=LocalizationEditorSettings.GetStringTableCollection(UiCopy.Table);
            foreach(var entry in ((JObject)source["supporting_copy"]).Properties()){
                int locale=0;
                foreach(string code in new[]{"en","ru","uk"}){
                    var table=collection.StringTables.Single(t=>t.LocaleIdentifier.Code==code);
                    Assert.That(table.GetEntry(entry.Name)?.Value,Is.EqualTo((string)entry.Value[locale++]),entry.Name+" / "+code);
                }
            }
            foreach(string section in new[]{"asides","checkpoints","missions"})
                foreach(var row in (JArray)source["sections"][section]){
                    Assert.That((string)row[0],Does.Not.Contain("I took it."),section);
                    Assert.That((string)row[1],Does.Not.Contain("Я забрал Фонарь"),section);
                    Assert.That((string)row[2],Does.Not.Contain("Я забрав Ліхтар"),section);
                }
            foreach(var row in (JArray)source["sections"]["missions"])
                foreach(var locale in row)Assert.That(((string)locale).Split(new[]{"{0}"},StringSplitOptions.None).Length,Is.EqualTo(2));
        }
        [Test] public void LegacyPageAccessorsCannotRetellTheDiscardedStormOnlyPlot()
        {
            for(int page=0;page<JourneyStory.PageCount;page++){
                int stop=JourneyStory.FirstStop(page);
                Assert.That(JourneyStory.Lines(page),Is.EqualTo(StoryConversations.IslandOpening(JourneyStory.Zone(stop),JourneyStory.IslandFor(stop)).Lines.Select(l=>l.Text)));
                Assert.That(JourneyStory.Task(page),Is.EqualTo(StoryConversations.Task(stop)));
            }
        }
    }
}
