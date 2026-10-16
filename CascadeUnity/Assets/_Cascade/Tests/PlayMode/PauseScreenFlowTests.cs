using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed class PauseScreenFlowTests
    {
        CascadeGame game;string directory;bool background;Camera camera;RenderTexture portrait,oldTarget;
        Button Button(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        void Open()=>game.SendMessage("TogglePause");
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;directory=Path.Combine(Path.GetTempPath(),"CascadePauseTests",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            void PreloadLog(string message,string stack,LogType type){if(type==LogType.Error&&message.StartsWith("OperationException : Failed to initialize localization, could not preload string tables.",StringComparison.Ordinal))LogAssert.Expect(type,new System.Text.RegularExpressions.Regex("^OperationException : Failed to initialize localization, could not preload string tables\\."));}
            UnityEngine.Application.logMessageReceived+=PreloadLog;
            try{yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;foreach(string locale in new[]{"en","ru","uk"}){var t=UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));yield return t;Assert.That(t.Status,Is.EqualTo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded));}}
            finally{UnityEngine.Application.logMessageReceived-=PreloadLog;}
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.Preferences.Tutorials=false;game.LoadNode("C02");game.ShowGame();
            // A batch Editor defaults to a wide Game view. Exercise the actual phone
            // layout, rather than letting its landscape gate suspend Retry on return.
            camera=Camera.main;oldTarget=camera.targetTexture;portrait=new RenderTexture(720,1280,24);
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=portrait;scaler.enabled=true;
            game.SetEditorSafeInsets(new Vector4(0,.00001f,0,.00001f));game.RefreshLayout();
            double until=Time.realtimeSinceStartupAsDouble+15;while(!game.PreparationReady&&Time.realtimeSinceStartupAsDouble<until)yield return null;Assert.That(game.PreparationReady,Is.True);game.Interaction.SetSuspended(false);yield return null;
        }
        [UnityTearDown] public IEnumerator Teardown(){if(camera!=null)camera.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=background;if(Directory.Exists(directory))Directory.Delete(directory,true);}
        void CommitMove(){Assert.That(game.Interaction.Execute(PuzzleCommand.Swap(0))?.Accepted,Is.True);Assert.That(game.Interaction.Attempt.Current.Status,Is.EqualTo(PuzzleStatus.Playing));}
        [UnityTest] public IEnumerator ContinuePreservesCommittedAttemptAndPauseBlocksPuzzleCommands()
        {
            CommitMove();string hash=game.Interaction.Attempt.CurrentStateHash;var id=game.Interaction.Attempt.AttemptId;Open();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Pause));Assert.That(game.Interaction.IsPaused,Is.True);Assert.That(game.Interaction.Execute(PuzzleCommand.Swap(1)),Is.Null);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            Assert.That(game.GetComponentsInChildren<Button>().Select(b=>b.name),Is.EquivalentTo(new[]{"Home","Settings","Retry","Resume"}));Button("Resume").onClick.Invoke();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));Assert.That(game.Interaction.IsPaused,Is.False);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.AttemptId,Is.EqualTo(id));Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator RetryStartsFreshAttemptOfTheSamePuzzleAndSavesTheReset()
        {
            CommitMove();string pin=game.Interaction.Attempt.PinnedContentHash;var id=game.Interaction.Attempt.AttemptId;Open();yield return null;Button("Retry").onClick.Invoke();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));Assert.That(game.Interaction.Attempt.PinnedContentHash,Is.EqualTo(pin));Assert.That(game.Interaction.Attempt.AttemptId,Is.Not.EqualTo(id));Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
            Assert.That(game.Interaction.Attempt.Current.Row,Is.EqualTo(game.Interaction.Attempt.Level.InitialSymbolIds));var saved=new ProfileStore(game.ProfilePath).Load().Profile;Assert.That(saved.Attempt.StateHash,Is.EqualTo(game.Interaction.Attempt.CurrentStateHash));
        }
        [UnityTest] public IEnumerator SettingsReturnsToPauseAndHomeRetainsTheAttempt()
        {
            CommitMove();Open();yield return null;string hash=game.Interaction.Attempt.CurrentStateHash;Button("Settings").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Settings));Button("Back").onClick.Invoke();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Pause));Assert.That(game.Interaction.IsPaused,Is.True);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Button("Home").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));game.ShowGame();yield return null;Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator AllLocalesFitAndImagesAndActionRowKeepTheirProportions()
        {
            game.Preferences.LargeText=true;
            foreach(string locale in new[]{"en","ru","uk"}){game.SetUiLocale(locale);Open();yield return null;Canvas.ForceUpdateCanvases();
                foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){t.ForceMeshUpdate();Assert.That(t.isTextOverflowing||t.isTextTruncated,Is.False,locale+" "+t.name);}
                Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Reference shop pause heading").text,Is.EqualTo(locale=="en"?"Take a break?":locale=="ru"?"Передохнём?":"Перепочинемо?"));
                foreach(var art in game.GetComponentsInChildren<Image>().Where(i=>i.name=="Pause resting Mika and bag"||i.name=="Pause brass lantern")){Assert.That(art.preserveAspect,Is.True);Assert.That(art.color,Is.EqualTo(Color.white));}
                Assert.That(((RectTransform)Button("Retry").transform).anchoredPosition.y,Is.EqualTo(((RectTransform)Button("Resume").transform).anchoredPosition.y));Button("Resume").onClick.Invoke();yield return null;
            }
        }
        [UnityTest] public IEnumerator LampFlowersAndLeavesMoveWhilePuzzleAndWorldRemainPaused()
        {
            Open();yield return null;var view=game.GetComponentsInChildren<PauseReferenceAmbiance>().Single();var leaf=game.GetComponentsInChildren<MenuLeafDrift>().Single();var flower=game.GetComponentsInChildren<RectTransform>().First(r=>r.name.StartsWith("Pause daisies "));var light=game.GetComponentsInChildren<PauseLanternLight>().Last();float age=view.AnimationAge,phase=game.Machine.AmbientPhase,brightness=light.Intensity;var rotation=flower.localRotation;string hash=game.Interaction.Attempt.CurrentStateHash;
            yield return new WaitForSecondsRealtime(.4f);Assert.That(view.AnimationAge,Is.GreaterThan(age));Assert.That(light.Intensity,Is.Not.EqualTo(brightness));Assert.That(flower.localRotation,Is.Not.EqualTo(rotation));Assert.That(leaf.RunsOnPause&&leaf.VisibleCount>0,Is.True);Assert.That(game.Machine.AmbientPhase,Is.EqualTo(phase));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            Canvas.ForceUpdateCanvases();Assert.That(light.canvasRenderer.GetMesh().vertexCount,Is.GreaterThan(100));
        }
        [UnityTest] public IEnumerator AppPauseAndReducedMotionFreezeDecorationsWithoutChangingTheAttempt()
        {
            Open();yield return new WaitForSecondsRealtime(.1f);var view=game.GetComponentsInChildren<PauseReferenceAmbiance>().Single();game.SendMessage("OnApplicationPause",true);float age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(view.AnimationAge,Is.EqualTo(age));game.SendMessage("OnApplicationPause",false);
            game.ReducedMotion=true;age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(view.AnimationAge,Is.EqualTo(age));Assert.That(game.GetComponentsInChildren<MenuLeafDrift>().Single().VisibleCount,Is.Zero);Assert.That(game.GetComponentsInChildren<RectTransform>().Where(r=>r.name.StartsWith("Pause daisies ")).All(r=>r.localRotation==Quaternion.identity),Is.True);
            game.ReducedMotion=false;game.Preferences.EffectQuality=0;age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(view.AnimationAge,Is.EqualTo(age));
        }
        [UnityTest] public IEnumerator RestrictedPracticeCannotUseThePauseRetryAction()
        {
            game.Interaction.RestrictPractice(PuzzleCommand.Swap(0));string hash=game.Interaction.Attempt.CurrentStateHash;Open();Assert.That(Button("Retry").interactable,Is.False);Button("Retry").onClick.Invoke();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Pause));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));yield return null;
        }
    }
}
