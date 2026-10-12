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
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class WorldComicFlowTests
    {
        const BindingFlags PrivateInstance=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;
        string directory;
        bool oldRunInBackground;

        [UnitySetUp] public IEnumerator SetUp()
        {
            oldRunInBackground=UnityEngine.Application.runInBackground;
            UnityEngine.Application.runInBackground=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeWorldComicFlow",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();ConfigureGame();
            var heard=Read<HashSet<int>>("heardStory");heard.Add(1000);
            heard.UnionWith(Enumerable.Range(0,JourneyStory.PageCount));
            heard.UnionWith(Enumerable.Range(4000,JourneyStory.TotalLevels));
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
        PlayerProfile Saved()=>new ProfileStore(game.ProfilePath).Load().Profile;
        static IEnumerator Await(Func<bool> ready,string message)
        {
            double began=Time.realtimeSinceStartupAsDouble;
            while(!ready()&&Time.realtimeSinceStartupAsDouble-began<30)yield return null;
            Assert.That(ready(),Is.True,message);
        }
        IEnumerator Click(string name)
        {
            yield return null;
            var button=game.GetComponentsInChildren<Button>().Single(b=>b.name==name&&b.gameObject.activeInHierarchy);
            Assert.That(button.interactable,Is.True,name+" must be available");
            button.onClick.Invoke();yield return null;
        }
        static void Solve(AttemptService attempt)
        {
            var solution=PuzzleSolver.FindShortest(attempt.Resolver,attempt.Current);
            Assert.That(solution.Status,Is.EqualTo(SearchStatus.Solved));
            foreach(var command in solution.Path)Assert.That(attempt.Execute(command).Accepted,Is.True);
        }
        IEnumerator MountMiddleStage()
        {
            var profile=Read<PlayerProfile>("profile");
            foreach(string node in new[]{"C01","C02"}){
                var completed=new AttemptService(game.Catalog.GetPrimary(node).Definition);Solve(completed);
                Assert.That(profile.RecordClear(completed),Is.True);Read<HashSet<string>>("sessionClears").Add(node);
            }
            var source=game.Catalog.GetPrimary("C03").Definition;var run=DescentRun.Build(source,version:5);
            var first=new AttemptService(run.Level(source,0),direction:run.Direction);Solve(first);run.AcceptCheckpoint(source,first);
            Invoke("MountAttempt",new AttemptService(run.Level(source,1),direction:run.Direction),run);
            Write("journeyActive",true);Write("descentTravel",true);Invoke("InitializeRewards");game.ShowGame();
            yield return Await(()=>game.Interaction.CanPlay,"Middle stage must become playable");
            var command=PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path.First();
            Assert.That(game.Interaction.Execute(command).Accepted,Is.True);
            yield return Await(()=>!game.Interaction.IsPresenting,"Exchange must finish before opening a comic");
            Assert.That(game.Interaction.Attempt.Current.Status,Is.EqualTo(PuzzleStatus.Playing));
            Invoke("PersistProgress");
        }
        void OpenPublishedRoutesInMemory()
        {
            // Fixture availability only: no puzzle execution or clear receipts are fabricated.
            Read<HashSet<string>>("sessionClears").UnionWith(game.Catalog.PrimaryLevels.Select(c=>c.Definition.CampaignNodeId));
            Write("journeyActive",true);
            Read<HashSet<int>>("heardStory").UnionWith(Enumerable.Range(6000,JourneyStory.WorldCount));
        }
        IEnumerator FinishOpening(int expectedReceipt)
        {
            Assert.That(game.ConversationOpen,Is.True);Assert.That(game.ConversationReceipt,Is.EqualTo(expectedReceipt));
            var footer=Read<RectTransform>("journeyFooter");
            if(footer!=null)Assert.That(footer.gameObject.activeSelf,Is.False,"Map actions must not show beneath the dialogue strip");
            int selected=Read<int>("selectedJourneyStop"),world=Read<int>("mapPage");
            game.SkipConversation();yield return null;
            Assert.That(game.HasSaveError,Is.False);Assert.That(game.ConversationOpen,Is.False);
            if(game.CurrentScreen==GameScreen.JourneyMap&&game.BrowsedIsland<0)
                Assert.That(Read<RectTransform>("journeyFooter").gameObject.activeSelf,Is.True,"Closing a map dialogue must restore map actions");
            Assert.That(Read<int>("selectedJourneyStop"),Is.EqualTo(selected),"Finishing dialogue must retain the browsed level");
            Assert.That(Read<int>("mapPage"),Is.EqualTo(world),"Finishing dialogue must retain the browsed world");
            Assert.That(Saved().HeardStory,Does.Contain(expectedReceipt));
        }

        [UnityTest] public IEnumerator EveryWorldComicHasFourRealPanelsAndPreservesThePinnedMiddleStage()
        {
            yield return MountMiddleStage();
            var attempt=game.Interaction.Attempt;var row=attempt.Current;var run=game.Descent;
            string hash=attempt.CurrentStateHash;int route=attempt.RouteLength;var panels=new HashSet<string>();
            for(int world=0;world<JourneyStory.WorldCount;world++){
                game.OpenWorldComic(world,game.ShowHome);yield return null;
                for(int page=0;page<WorldPrologues.ForWorld(world).Pages.Length;page++){
                    Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Prologue));Assert.That(game.ComicWorld,Is.EqualTo(world));Assert.That(game.ComicPage,Is.EqualTo(page));
                    var image=game.GetComponentsInChildren<Image>().Single(i=>i.name=="World comic scene");
                    game.RefreshLayout();
                    var frame=(RectTransform)image.transform.parent;
                    Assert.That(frame.rect.width/frame.rect.height,Is.EqualTo(image.sprite.rect.width/image.sprite.rect.height).Within(.001f),"Each new panel must bind its active heading and preserve the illustration aspect");
                    var scenePosition=frame.anchoredPosition;var sceneSize=frame.sizeDelta;
                    Assert.That(image.sprite,Is.Not.Null);Assert.That(image.sprite.texture,Is.Not.Null);Assert.That(image.sprite.rect.width,Is.GreaterThan(0));
                    panels.Add(image.sprite.texture.GetInstanceID()+":"+image.sprite.rect);
                    Assert.That(game.Interaction.IsPaused,Is.True);Assert.That(game.Interaction.Attempt,Is.SameAs(attempt));
                    Assert.That(attempt.Current,Is.SameAs(row));Assert.That(attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(attempt.RouteLength,Is.EqualTo(route));
                    Assert.That(game.Descent,Is.SameAs(run));Assert.That(game.Descent.StageIndex,Is.EqualTo(1));
                    yield return Click("Next comic page");
                    Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Prologue));Assert.That(game.ComicPage,Is.EqualTo(page),"The first click reveals the next speaker on the same illustration");
                    game.RefreshLayout();
                    var nextScene=(RectTransform)game.GetComponentsInChildren<Image>().Single(i=>i.name=="World comic scene").transform.parent;
                    Assert.That(Vector2.Distance(nextScene.anchoredPosition,scenePosition),Is.LessThan(.01f),"Changing speaker must keep the illustration still");
                    Assert.That(Vector2.Distance(nextScene.sizeDelta,sceneSize),Is.LessThan(.01f),"Changing speaker must keep the illustration size");
                    yield return Click("Next comic page");
                }
                Assert.That(game.ComicWorld,Is.EqualTo(-1));Assert.That(game.HasSaveError,Is.False);
                Assert.That(Saved().HeardStory,Does.Contain(WorldPrologues.ForWorld(world).Receipt));
                Assert.That(Saved().Attempt.StateHash,Is.EqualTo(hash));Assert.That(Saved().Descent.StageIndex,Is.EqualTo(1));
            }
            Assert.That(panels.Count,Is.EqualTo(36),"All worlds must use their own four illustration panels");
        }

        [UnityTest] public IEnumerator ModeCrossingsRememberTheStoryAndDoNotReplayIntroductionsAfterReload()
        {
            game.OpenWorldComic(0,game.ShowHome);game.FinishWorldComic();yield return null;
            Read<HashSet<int>>("heardStory").Add(StoryConversations.IslandOpening(0,0).Receipt);
            foreach(var mode in new[]{PuzzleMode.Words,PuzzleMode.Mixed}){
                game.ChooseSoloMode(mode);yield return null;
                Assert.That(game.PreferredMode,Is.EqualTo(mode));Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
                Assert.That(game.ComicWorld,Is.EqualTo(-1));Assert.That(game.ConversationReceipt,Is.EqualTo(9000+(int)mode));
                Assert.That(Saved().HeardStory.Contains(6000),Is.False,"Do not copy another journey's local records");
                Assert.That(Saved().RemembersStory(6000),Is.True);
                string hash=game.Interaction.Attempt.CurrentStateHash;
                game.SkipConversation();yield return null;
                Assert.That(Saved().RemembersStory(9000+(int)mode),Is.True);
                Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
                Assert.That(game.ConversationOpen,Is.False);
                yield return Await(()=>game.PreparationReady&&!game.Interaction.IsPresenting,"Mode preparation must finish");
            }
            game.SelectMode(PuzzleMode.Shapes);yield return null;
            Assert.That(game.ConversationReceipt,Is.EqualTo(9010));game.SkipConversation();yield return null;
            string pinned=game.Interaction.Attempt.CurrentStateHash;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();ConfigureGame();game.ShowJourney();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(game.ComicWorld,Is.EqualTo(-1));Assert.That(game.ConversationOpen,Is.False);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(pinned));Assert.That(Saved().RemembersStory(6000),Is.True);
        }

        [UnityTest] public IEnumerator IslandButtonsAndWorldBrowsingShowEveryUnseenOpeningOnceWithoutMovingTheRow()
        {
            // Mixed offers every published node, including the original mixed-symbol lessons.
            game.SelectMode(PuzzleMode.Mixed);yield return null;
            Assert.That(game.PreferredMode,Is.EqualTo(PuzzleMode.Mixed));
            game.OpenWorldComic(0,game.ShowHome);game.FinishWorldComic();yield return null;
            OpenPublishedRoutesInMemory();Read<HashSet<int>>("heardStory").UnionWith(Enumerable.Range(7000,JourneyStory.TotalLevels));
            var attempt=game.Interaction.Attempt;string hash=attempt.CurrentStateHash;
            game.ShowJourney();yield return null;yield return FinishOpening(StoryConversations.IslandOpening(0,0).Receipt);
            for(int world=0;world<JourneyStory.WorldCount;world++){
                if(world>0){
                    game.TurnJourneyMap(1);yield return null;Assert.That(game.MapWorld,Is.EqualTo(world));
                    if(game.ConversationOpen)yield return FinishOpening(StoryConversations.IslandOpening(world,JourneyStory.IslandFor(Read<int>("selectedJourneyStop"))).Receipt);
                }
                for(int island=0;island<5;island++){
                    int receipt=StoryConversations.IslandOpening(world,island).Receipt;
                    bool heard=Read<HashSet<int>>("heardStory").Contains(receipt);int selected=Read<int>("selectedJourneyStop");
                    yield return Click("Island"+(island+1));
                    Assert.That(game.BrowsedIsland,Is.EqualTo(island));Assert.That(Read<int>("selectedJourneyStop"),Is.EqualTo(selected));
                    Assert.That(game.ConversationOpen,Is.False,"An island tap opens its read-only list");
                    Assert.That(Read<HashSet<int>>("heardStory").Contains(receipt),Is.EqualTo(heard),"Browsing must not consume an unread story");
                    game.CloseJourneyIsland();yield return null;Assert.That(game.MapWorld,Is.EqualTo(world));
                    yield return Click("Island"+(island+1));Assert.That(game.ConversationOpen,Is.False,"Reopening the list must keep it free of dialogue");
                    Assert.That(Read<HashSet<int>>("heardStory").Contains(receipt),Is.EqualTo(heard));
                    Assert.That(game.Interaction.Attempt,Is.SameAs(attempt));Assert.That(attempt.CurrentStateHash,Is.EqualTo(hash));
                    game.CloseJourneyIsland();yield return null;
                }
            }
            Assert.That(Read<HashSet<int>>("heardStory").Where(r=>r>=8000&&r<8100),
                Is.EquivalentTo(Enumerable.Range(0,JourneyStory.WorldCount).Select(w=>StoryConversations.IslandOpening(w,0).Receipt)),
                "World navigation reads its first island opening; other island stories wait for Play");
        }

        [UnityTest] public IEnumerator MidIslandExchangeUsesTheExistingOnceOnlyFlowAndLeavesThePuzzleUntouched()
        {
            OpenPublishedRoutesInMemory();Read<HashSet<int>>("heardStory").UnionWith(Enumerable.Range(8000,JourneyStory.WorldCount*10));
            game.ShowJourney();yield return null;
            int stop=JourneyStory.IslandFirst(0,0)+5;
            int receipt=StoryConversations.IslandProgress(stop).Receipt;
            var attempt=game.Interaction.Attempt;string hash=attempt.CurrentStateHash;
            yield return Click("Island1");
            yield return Click("Island level "+(stop+1));
            Assert.That(game.ConversationReceipt,Is.EqualTo(receipt));
            Assert.That(game.Interaction.Attempt,Is.SameAs(attempt));Assert.That(attempt.CurrentStateHash,Is.EqualTo(hash));
            Assert.That(game.Interaction.IsPaused,Is.True);
            yield return FinishOpening(receipt);
            yield return Await(()=>game.PreparationReady&&game.Interaction.CanPlay,"Continue must start the selected level");
            Assert.That(Saved().HeardStory,Does.Contain(receipt));
            game.ShowJourney();yield return null;
            Assert.That(game.ConversationOpen,Is.False,"Returning to the map must not repeat a heard exchange");
            game.ReplayJourneyLevel(stop);yield return null;
            Assert.That(game.ConversationOpen,Is.False,"Replaying the same level must not repeat a heard exchange");
        }

        [UnityTest] public IEnumerator OccasionalExchangeAppearsOnlyOnItsExactLevelAndKeepsTheBrowsedStop()
        {
            OpenPublishedRoutesInMemory();Read<HashSet<int>>("heardStory").UnionWith(Enumerable.Range(8000,JourneyStory.WorldCount*10));
            game.ShowJourney();yield return null;
            for(int world=0;world<JourneyStory.WorldCount;world++){
                if(world>0){game.TurnJourneyMap(1);yield return null;}
                Assert.That(game.MapWorld,Is.EqualTo(world));
                var original=game.Interaction.Attempt;var row=original.Current;string hash=original.CurrentStateHash;
                yield return Click("Island4");Assert.That(game.ConversationOpen,Is.False);
                Assert.That(game.BrowsedIsland,Is.EqualTo(3));
                int stop=JourneyStory.IslandFirst(world,3)+1;
                yield return Click("Island level "+(stop+1));Assert.That(Read<int>("selectedJourneyStop"),Is.EqualTo(stop));
                Assert.That(game.Interaction.Attempt,Is.SameAs(original));Assert.That(original.Current,Is.SameAs(row));
                Assert.That(original.CurrentStateHash,Is.EqualTo(hash),"Reading the exact level's exchange must retain the current attempt until Continue");
                yield return FinishOpening(StoryConversations.LevelAside(stop).Receipt);
                yield return Await(()=>game.PreparationReady&&game.Interaction.CanPlay,"Continue must launch the selected published level");
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));
                Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo(game.Catalog.PrimaryLevels[stop].Definition.CampaignNodeId));
                var launched=game.Interaction.Attempt;row=launched.Current;hash=launched.CurrentStateHash;
                game.ShowJourney();yield return null;Assert.That(game.ConversationOpen,Is.False,"A read exchange must not repeat on the return to the map");
                yield return Click("Island4");Assert.That(game.ConversationOpen,Is.False,"The neighboring level cards must not reuse the exchange");
                game.ReplayJourneyLevel(stop);yield return null;Assert.That(game.ConversationOpen,Is.False,"Playing the pinned level again must resume without repeating its exchange");
                Assert.That(game.Interaction.Attempt,Is.SameAs(launched));Assert.That(launched.Current,Is.SameAs(row));Assert.That(launched.CurrentStateHash,Is.EqualTo(hash));
                game.ShowJourney();yield return null;
            }
            Assert.That(Read<HashSet<int>>("heardStory").Count(r=>r>=7000&&r<7200),Is.EqualTo(9));
        }
    }
}
