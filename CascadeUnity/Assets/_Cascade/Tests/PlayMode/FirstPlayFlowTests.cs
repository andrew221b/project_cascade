#if UNITY_EDITOR
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
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class FirstPlayFlowTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;
        string directory;
        bool oldBackground;
        Camera camera;
        RenderTexture portraitTarget,oldTarget;
        T Read<T>(string name)=>(T)typeof(CascadeGame).GetField(name,Private).GetValue(game);
        PlayerProfile Profile=>Read<PlayerProfile>("profile");
        Button Control(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name&&b.gameObject.activeInHierarchy);
        void Persist()=>typeof(CascadeGame).GetMethod("PersistProgress",Private).Invoke(game,new object[]{true});
        PlayerProfile Saved()=>new ProfileStore(game.ProfilePath).Load().Profile;

        [UnitySetUp] public IEnumerator Setup()
        {
            oldBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeFirstPlay",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            portraitTarget=new RenderTexture(720,1280,24);
            yield return LoadGame();
        }
        IEnumerator LoadGame()
        {
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            camera=Camera.main;oldTarget=camera.targetTexture;
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=portraitTarget;scaler.enabled=true;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Preferences.Tutorials=false;game.Preferences.ReducedMotion=true;game.ReducedMotion=true;
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            Profile.AccountIntroSeen=true;Profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            yield return null;game.RefreshLayout();game.ShowHome();yield return null;
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            if(camera!=null)camera.targetTexture=oldTarget;
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(portraitTarget!=null){portraitTarget.Release();UnityEngine.Object.Destroy(portraitTarget);}
            CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=oldBackground;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        static IEnumerator Await(Func<bool> ready,string message)
        {
            float deadline=Time.realtimeSinceStartup+30;
            while(!ready()&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(ready(),Is.True,message);
        }
        IEnumerator Click(string name)
        {
            var button=Control(name);Assert.That(button.interactable,Is.True,name);button.onClick.Invoke();yield return null;
        }
        IEnumerator ReadFirstComic()
        {
            var pages=new HashSet<int>();bool sawPlay=false;
            for(int click=0;click<80&&game.CurrentScreen==GameScreen.Prologue;click++){
                Assert.That(game.ComicWorld,Is.Zero);pages.Add(game.ComicPage);
                string caption=Control("Next comic page").GetComponentInChildren<TMP_Text>().text;
                if(caption=="Play")sawPlay=true;
                yield return Click("Next comic page");
                Assert.That(game.CurrentScreen==GameScreen.Prologue||game.CurrentScreen==GameScreen.Playing,Is.True,"No map or extra conversation between comic and puzzle");
            }
            Assert.That(pages.Count,Is.EqualTo(WorldPrologues.ForWorld(0).Pages.Length));Assert.That(sawPlay,Is.True);
            yield return Await(()=>game.CurrentScreen==GameScreen.Playing&&game.PreparationReady&&game.Interaction.CanPlay,"First level must start after the comic");
            Assert.That(game.ConversationOpen,Is.False);Assert.That(game.IsJourneyAttempt,Is.True);
            Assert.That(game.Descent.StageIndex,Is.Zero);Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo("C01"));
        }
        IEnumerator SolveCurrentStage()
        {
            var attempt=game.Interaction.Attempt;var solution=PuzzleSolver.FindShortest(attempt.Resolver,attempt.Current);
            Assert.That(solution.Status,Is.EqualTo(SearchStatus.Solved));
            foreach(var command in solution.Path){
                yield return Await(()=>game.Interaction.CanPlay,"A committed command must settle before the next one");
                Assert.That(game.Interaction.Execute(command).Accepted,Is.True);
                yield return Await(()=>!game.Interaction.IsPresenting,"Command presentation must complete");
            }
            yield return Await(()=>game.CurrentScreen==GameScreen.Checkpoint||game.CurrentScreen==GameScreen.Win,"Stage must reach its result");
        }

        [UnityTest] public IEnumerator FirstPlayReadsComicStartsLevelOneAndItsVictoryOffersNextAndMap()
        {
            Assert.That(Profile.AllClears,Is.Empty);yield return Click("Startgame");yield return ReadFirstComic();
            for(int stage=0;stage<3;stage++){
                Assert.That(game.Descent.StageIndex,Is.EqualTo(stage));yield return SolveCurrentStage();
                if(stage<2){game.GoLower();yield return Await(()=>game.CurrentScreen==GameScreen.Playing&&game.Interaction.CanPlay,"Next stage must start");}
            }
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Win));Assert.That(Profile.AllClears.Count(),Is.EqualTo(1));
            Assert.That(Control("Continue completion").GetComponentInChildren<TMP_Text>().text,Is.EqualTo("Next level"));
            yield return Click("Completion map");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(game.ConversationOpen,Is.False);
            Assert.That(game.LastCompletion.Acknowledged,Is.True);Assert.That(game.LastCompletion.NextNode,Is.Null);
            string delivered=game.Interaction.Attempt.CurrentStateHash;
            game.ShowHome();yield return Click("Startgame");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(delivered));
            yield return Click("Travel");yield return Await(()=>game.CurrentScreen==GameScreen.Playing&&game.Interaction.CanPlay,"Second level must require selection on the map");
            Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo("C02"));
        }
        [UnityTest] public IEnumerator ReloadAfterFirstComicKeepsTheUnfinishedFirstLevelAndLaterPlayOpensMap()
        {
            yield return Click("Startgame");yield return ReadFirstComic();
            var attempt=game.Interaction.Attempt;var command=PuzzleSolver.FindShortest(attempt.Resolver,attempt.Current).Path.First();
            Assert.That(game.Interaction.Execute(command).Accepted,Is.True);yield return Await(()=>!game.Interaction.IsPresenting,"First move must settle");
            string hash=attempt.CurrentStateHash,id=attempt.AttemptId.ToString("D");Persist();Assert.That(game.HasSaveError,Is.False);
            Assert.That(Saved().RemembersStory(6000),Is.True);yield return LoadGame();
            yield return Click("Startgame");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(game.ComicWorld,Is.EqualTo(-1));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.AttemptId.ToString("D"),Is.EqualTo(id));
            yield return Click("Travel");yield return Await(()=>game.CurrentScreen==GameScreen.Playing&&game.Interaction.CanPlay,"Map must resume the same first level");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.AttemptId.ToString("D"),Is.EqualTo(id));
        }
        [UnityTest] public IEnumerator ReturningPlayerEntersMapAndUnseenWorldComicWaitsForLevelSelection()
        {
            // A concrete unfinished attempt must not be reset even if an old save lacks comic receipts.
            game.StartCampaignLevel(game.Catalog.GetPrimary("C01").Definition);game.ShowGame();
            yield return Await(()=>game.Interaction.CanPlay,"Existing journey must be ready");
            var attempt=game.Interaction.Attempt;string hash=attempt.CurrentStateHash;game.ShowHome();yield return Click("Startgame");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(game.Interaction.Attempt,Is.SameAs(attempt));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.ComicWorld,Is.EqualTo(-1));
            // With no pinned attempt, entering an unread world from the map shows its comic, rather than Home Play doing so.
            typeof(CascadeGame).GetField("journeyActive",Private).SetValue(game,false);
            yield return Click("Travel");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Prologue));Assert.That(game.ComicWorld,Is.Zero);
            Assert.That(Control("Next comic page").GetComponentInChildren<TMP_Text>().text,Is.EqualTo("Continue"));
        }
        [UnityTest] public IEnumerator FailedComicSaveDoesNotStartTheFirstLevelOrMarkTheComicRead()
        {
            yield return Click("Startgame");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Prologue));
            string blocker=Path.Combine(directory,"not_a_directory");File.WriteAllText(blocker,"fixture");
            typeof(CascadeGame).GetField("profileStore",Private).SetValue(game,new ProfileStore(Path.Combine(blocker,"profile.json")));
            var attempt=game.Interaction.Attempt;game.FinishWorldComic();
            Assert.That(game.HasSaveError,Is.True);Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Prologue));
            Assert.That(game.Interaction.Attempt,Is.SameAs(attempt));Assert.That(game.IsJourneyAttempt,Is.False);
            Assert.That(Profile.RemembersStory(6000),Is.False);Assert.That(Profile.RemembersStory(1000),Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator ExplicitOpeningComicRetainsItsDefaultMapDestination()
        {
            game.OpenWorldComic(0);yield return null;game.FinishWorldComic();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(game.IsJourneyAttempt,Is.False);
            Assert.That(Saved().RemembersStory(6000),Is.True);
        }
    }
}
#endif
