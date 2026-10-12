using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using Cascade.Solver;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Cascade.Tests
{
    public sealed class JourneyBrowseTests
    {
        const BindingFlags PrivateInstance=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;
        string directory;
        bool oldRunInBackground;

        [UnitySetUp] public IEnumerator SetUp()
        {
            oldRunInBackground=UnityEngine.Application.runInBackground;
            UnityEngine.Application.runInBackground=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeJourneyBrowse",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();ConfigureGame();
            var profile=Read<PlayerProfile>("profile");
            Read<HashSet<int>>("heardStory").UnionWith(StoryReceipts());
            // Browsing is tested after the stories have been read in each journey.
            foreach(var mode in new[]{PuzzleMode.Words,PuzzleMode.Mixed})profile.ModeJourneys.Add(new ModeJourney{
                Mode=mode.ToString(),HeardStory=StoryReceipts().Distinct().ToList(),
                SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList()});
            game.ShowGame();
            yield return Await(()=>game.PreparationReady&&game.Interaction.CanPlay,"Initial puzzle must become playable");
        }

        [UnityTearDown] public IEnumerator TearDown()
        {
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            CascadeGame.EditorProfileDirectory=null;
            UnityEngine.Application.runInBackground=oldRunInBackground;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }

        static IEnumerable<int> StoryReceipts()=>new[]{1000,9000,9001,9002,9010,9011,9012}.Concat(Enumerable.Range(0,JourneyStory.PageCount))
            .Concat(Enumerable.Range(2000,JourneyStory.WorldCount*10)).Concat(Enumerable.Range(4000,JourneyStory.TotalLevels))
            .Concat(Enumerable.Range(6000,JourneyStory.WorldCount)).Concat(Enumerable.Range(7000,JourneyStory.TotalLevels))
            .Concat(Enumerable.Range(8000,JourneyStory.WorldCount*10));
        void ConfigureGame()
        {
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Preferences.ReducedMotion=true;game.ReducedMotion=true;
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            Read<PlayerProfile>("profile").SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
        }
        T Read<T>(string name)=>(T)typeof(CascadeGame).GetField(name,PrivateInstance).GetValue(game);
        void Write(string name,object value)=>typeof(CascadeGame).GetField(name,PrivateInstance).SetValue(game,value);
        void Invoke(string name,params object[] arguments)=>typeof(CascadeGame).GetMethod(name,PrivateInstance).Invoke(game,arguments);
        PlayerProfile Saved()
        {
            var load=new ProfileStore(game.ProfilePath).Load();
            Assert.That(load.Recovery,Is.EqualTo(SaveRecovery.None),"Browse fixtures must remain valid saved profiles");
            return load.Profile;
        }
        static IEnumerator Await(Func<bool> ready,string message)
        {
            double began=Time.realtimeSinceStartupAsDouble;
            while(!ready()&&Time.realtimeSinceStartupAsDouble-began<30)yield return null;
            Assert.That(ready(),Is.True,message);
        }
        static void Solve(AttemptService attempt)
        {
            var solution=PuzzleSolver.FindShortest(attempt.Resolver,attempt.Current);
            Assert.That(solution.Status,Is.EqualTo(SearchStatus.Solved));
            foreach(var command in solution.Path)Assert.That(attempt.Execute(command).Accepted,Is.True);
            Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
        }
        ClearReceipt AddPublishedClear(LevelContent content,bool fourStars=false)
        {
            // Navigation fixtures bind valid receipts to actual published content; they do not test delivery or ratings.
            var profile=Read<PlayerProfile>("profile");
            var receipt=new ClearReceipt{CampaignNodeId=content.Definition.CampaignNodeId,AttemptId=Guid.NewGuid().ToString("D"),
                ContentHash=content.Definition.ContentHash,BestStars=fourStars?4:5,BestScore=fourStars?1300:1400};
            profile.Clears.Add(receipt);
            Read<HashSet<string>>("sessionClears").Add(receipt.CampaignNodeId);
            profile.ModeRecords.Add(new MasteryRecord{PuzzleId=content.Definition.PuzzleId,ContentHash=content.Definition.ContentHash,
                Mode=content.Mode.ToString(),Locale=content.Locale,BestRoute=1,BestWithoutHint=1,BestClean=1});
            Invoke("InitializeRewards");profile.Validate();
            return receipt;
        }
        IEnumerator MountMiddleStage(int stop)
        {
            foreach(var content in game.Catalog.PrimaryLevels.Take(stop))AddPublishedClear(content);
            var source=game.Catalog.PrimaryLevels[stop].Definition;var run=DescentRun.Build(source,version:5);
            Assert.That(run,Is.Not.Null);
            var first=new AttemptService(run.Level(source,0),direction:run.Direction);Solve(first);run.AcceptCheckpoint(source,first);
            Invoke("MountAttempt",new AttemptService(run.Level(source,1),direction:run.Direction),run);
            Write("journeyActive",true);Write("descentTravel",true);game.ShowGame();
            yield return Await(()=>game.Interaction.CanPlay,"Middle stage must become playable");
            var solution=PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver);
            Assert.That(solution.Status,Is.EqualTo(SearchStatus.Solved));
            Assert.That(game.Interaction.Execute(solution.Path.First()).Accepted,Is.True);
            yield return Await(()=>!game.Interaction.IsPresenting,"Middle-stage exchange must finish before browsing");
            Assert.That(game.Interaction.Attempt.Current.Status,Is.EqualTo(PuzzleStatus.Playing));Invoke("PersistProgress");
            Assert.That(game.HasSaveError,Is.False);
        }
        void AssertPinned(PlayerProfile before,AttemptService attempt,PuzzleState row,DescentRun run)
        {
            Assert.That(game.Interaction.Attempt,Is.SameAs(attempt));Assert.That(attempt.Current,Is.SameAs(row));
            Assert.That(game.Descent,Is.SameAs(run));Assert.That(run.StageIndex,Is.EqualTo(1));
            Assert.That(attempt.RouteLength,Is.EqualTo(before.Attempt.Commands.Length));
            Invoke("PersistProgress");Assert.That(game.HasSaveError,Is.False);var after=Saved();
            Assert.That(after.Attempt.AttemptId,Is.EqualTo(before.Attempt.AttemptId));
            Assert.That(after.Attempt.StateHash,Is.EqualTo(before.Attempt.StateHash));
            Assert.That(after.Descent.ContentHash,Is.EqualTo(before.Descent.ContentHash));
            Assert.That(after.Descent.StageIndex,Is.EqualTo(before.Descent.StageIndex));
            Assert.That(after.Clears.Select(c=>c.CampaignNodeId),Is.EquivalentTo(before.Clears.Select(c=>c.CampaignNodeId)));
        }
        static string[] ClearStats(PlayerProfile profile)=>profile.Clears.Select(c=>c.CampaignNodeId+":"+c.BestStars+":"+c.BestScore).OrderBy(s=>s).ToArray();
        static string[] Records(PlayerProfile profile)=>profile.ModeRecords.Select(r=>r.Key+":"+r.BestRoute+":"+r.BestWithoutHint+":"+r.BestClean).OrderBy(s=>s).ToArray();

        [UnityTest] public IEnumerator WorldAndIslandBrowsingPreserveThePinnedMiddleStageAndResumeItsRoute()
        {
            int stop=JourneyStory.WorldFirst(1);yield return MountMiddleStage(stop);
            var before=Saved();var attempt=game.Interaction.Attempt;var row=attempt.Current;var run=game.Descent;
            game.ShowJourney();yield return null;
            Assert.That(game.MapWorld,Is.EqualTo(1));Assert.That(game.BrowsedIsland,Is.EqualTo(-1));
            game.OpenJourneyIsland(0);yield return null;Assert.That(game.BrowsedIsland,Is.Zero);
            Assert.That(game.ConversationOpen,Is.False);AssertPinned(before,attempt,row,run);
            game.CloseJourneyIsland();yield return null;
            Assert.That(game.MapWorld,Is.EqualTo(1));Assert.That(game.BrowsedIsland,Is.EqualTo(-1));
            game.TurnJourneyMap(1);yield return null;Assert.That(game.MapWorld,Is.EqualTo(1),"The next world is still locked");
            game.TurnJourneyMap(-1);yield return null;Assert.That(game.MapWorld,Is.Zero);
            game.OpenJourneyIsland(0);yield return null;
            game.ReplayJourneyLevel(stop);yield return null;AssertPinned(before,attempt,row,run);
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap),"An opened level in another world is outside this island list");
            game.CloseJourneyIsland();game.TurnJourneyMap(-1);yield return null;Assert.That(game.MapWorld,Is.Zero);
            game.TurnJourneyMap(1);yield return null;Assert.That(game.MapWorld,Is.EqualTo(1));
            game.OpenJourneyIsland(0);yield return null;game.ReplayJourneyLevel(stop);
            yield return Await(()=>game.Interaction.CanPlay,"Replaying the pinned level must resume its current stage");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));AssertPinned(before,attempt,row,run);
        }

        [UnityTest] public IEnumerator ClosedWorldsIslandsAndLevelsRejectBrowseAndReplayInEveryMode()
        {
            foreach(var mode in new[]{PuzzleMode.Shapes,PuzzleMode.Words,PuzzleMode.Mixed}){
                game.SelectMode(mode);yield return null;game.ShowJourney();yield return null;
                Assert.That(game.PreferredMode,Is.EqualTo(mode));Assert.That(game.JourneyClearCount,Is.Zero);
                Assert.That(game.MapWorld,Is.Zero);Assert.That(game.BrowsedIsland,Is.EqualTo(-1));
                var attempt=game.Interaction.Attempt;string hash=attempt.CurrentStateHash;
                game.OpenJourneyIsland(-1);game.OpenJourneyIsland(5);game.OpenJourneyIsland(1);yield return null;
                Assert.That(game.BrowsedIsland,Is.EqualTo(-1));
                game.TurnJourneyMap(-1);game.TurnJourneyMap(1);yield return null;Assert.That(game.MapWorld,Is.Zero);
                game.OpenJourneyIsland(0);yield return null;Assert.That(game.BrowsedIsland,Is.Zero);
                foreach(int stop in new[]{-1,1,JourneyStory.IslandFirst(0,1),JourneyStory.TotalLevels})game.ReplayJourneyLevel(stop);
                yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
                Assert.That(game.Interaction.Attempt,Is.SameAs(attempt));Assert.That(attempt.CurrentStateHash,Is.EqualTo(hash));
                game.CloseJourneyIsland();game.ReplayJourneyLevel(0);yield return null;
                Assert.That(game.BrowsedIsland,Is.EqualTo(-1));Assert.That(game.Interaction.Attempt,Is.SameAs(attempt));
                Invoke("PersistProgress");Assert.That(game.HasSaveError,Is.False);Assert.That(Saved().Clears,Is.Empty);
            }
        }

        [UnityTest] public IEnumerator AnOpenedCompletedLevelCanBeReplayedOnlyFromItsSelectedIsland()
        {
            yield return MountMiddleStage(JourneyStory.IslandFirst(0,1));
            var before=Saved();var original=game.Interaction.Attempt;
            game.ShowJourney();game.OpenJourneyIsland(1);yield return null;
            Assert.That(game.BrowsedIsland,Is.EqualTo(1));game.ReplayJourneyLevel(0);yield return null;
            Assert.That(game.Interaction.Attempt,Is.SameAs(original));
            game.CloseJourneyIsland();game.OpenJourneyIsland(0);yield return null;game.ReplayJourneyLevel(0);
            yield return Await(()=>game.Interaction.CanPlay,"Selecting an earned level must start its published descent");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));Assert.That(game.Interaction.Attempt,Is.Not.SameAs(original));
            Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo(game.Catalog.PrimaryLevels[0].Definition.CampaignNodeId));
            Assert.That(game.Descent.StageIndex,Is.Zero);Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
            Assert.That(game.IsJourneyAttempt,Is.True);Assert.That(ClearStats(Saved()),Is.EqualTo(ClearStats(before)));
        }

        [UnityTest] public IEnumerator BestClearStatsAndMasteryStayWithTheirModeAfterBrowseAndReload()
        {
            foreach(var content in game.Catalog.PrimaryLevels.Take(3))AddPublishedClear(content);
            Invoke("PersistProgress");var shapes=Saved();Assert.That(shapes.ModeRecords,Is.Not.Empty);
            game.ShowJourney();game.OpenJourneyIsland(0);game.CloseJourneyIsland();yield return null;
            Assert.That(ClearStats(Saved()),Is.EqualTo(ClearStats(shapes)));
            game.SelectMode(PuzzleMode.Words);yield return null;
            Assert.That(game.JourneyClearCount,Is.Zero);Assert.That(Saved().Clears,Is.Empty);Assert.That(Saved().ModeRecords,Is.Empty);
            game.OpenJourneyIsland(1);yield return null;Assert.That(game.BrowsedIsland,Is.EqualTo(-1));
            Assert.That(game.Catalog.TryGetLocalizedBinding("C01","en",out var wordsContent),Is.True);
            var wordsReceipt=AddPublishedClear(wordsContent,fourStars:true);Invoke("PersistProgress");var words=Saved();
            Assert.That(wordsReceipt.BestStars,Is.GreaterThan(0));Assert.That(wordsReceipt.BestScore,Is.GreaterThan(0));
            Assert.That(wordsReceipt.BestStars,Is.Not.EqualTo(shapes.Clears.First().BestStars),"Each mode has its own earned rating");
            game.ShowJourney();game.OpenJourneyIsland(0);game.CloseJourneyIsland();game.SelectMode(PuzzleMode.Shapes);yield return null;
            Assert.That(ClearStats(Saved()),Is.EqualTo(ClearStats(shapes)));Assert.That(Records(Saved()),Is.EqualTo(Records(shapes)));
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Mode stars total").text,
                Is.EqualTo(shapes.Clears.Sum(c=>c.BestStars).ToString()));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();ConfigureGame();game.ShowJourney();yield return null;
            Assert.That(game.PreferredMode,Is.EqualTo(PuzzleMode.Shapes));Assert.That(ClearStats(Saved()),Is.EqualTo(ClearStats(shapes)));
            Assert.That(Records(Saved()),Is.EqualTo(Records(shapes)));
            game.SelectMode(PuzzleMode.Words);yield return null;
            Assert.That(ClearStats(Saved()),Is.EqualTo(ClearStats(words)));Assert.That(Records(Saved()),Is.EqualTo(Records(words)));
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Mode stars total").text,
                Is.EqualTo(wordsReceipt.BestStars.ToString()));
            game.TurnJourneyMap(1);game.OpenJourneyIsland(1);yield return null;
            Assert.That(game.MapWorld,Is.Zero);Assert.That(game.BrowsedIsland,Is.EqualTo(-1));
        }
    }
}
