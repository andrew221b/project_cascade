#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Cascade.Tests
{
    public sealed class LocalizationStartupRegressionTests
    {
        CascadeGame game;
        string directory;
        bool background,audio;
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeLocaleStartup",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=directory;CascadeGame.EditorSystemLanguage=null;
            yield return LocalizationSettings.InitializationOperation;
            Assert.That(LocalizationSettings.InitializationOperation.Status,Is.EqualTo(AsyncOperationStatus.Succeeded));
            LocalizationSettings.Instance.ResetState();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            // Let the current load finish before another case clears the database.
            yield return LocalizationSettings.InitializationOperation;
            CascadeGame.EditorProfileDirectory=null;CascadeGame.EditorSystemLanguage=null;
            UnityEngine.Application.runInBackground=background;AudioListener.pause=audio;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Load(SystemLanguage system)
        {
            CascadeGame.EditorSystemLanguage=system;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Assert.That(game,Is.Not.Null);
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
        }
        IEnumerator Ready(string code)
        {
            double end=Time.realtimeSinceStartupAsDouble+15;
            while(Time.realtimeSinceStartupAsDouble<end){
                var op=LocalizationSettings.InitializationOperation;
                if(op.IsDone&&op.Status==AsyncOperationStatus.Succeeded&&game.UiLanguage==code)break;
                yield return null;
            }
            Assert.That(LocalizationSettings.InitializationOperation.Status,Is.EqualTo(AsyncOperationStatus.Succeeded));
            Assert.That(game.UiLanguage,Is.EqualTo(code));
            foreach(string locale in new[]{"en","ru","uk"}){
                var table=LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,LocalizationSettings.AvailableLocales.GetLocale(locale));
                yield return table;Assert.That(table.Status,Is.EqualTo(AsyncOperationStatus.Succeeded),locale);
                Assert.That(table.Result.GetEntry("Shop").Value,Is.EqualTo(locale=="ru"?"Магазин":locale=="uk"?"Крамниця":"Shop"));
            }
            for(int i=0;i<5;i++)yield return null;
            foreach(var caption in game.GetComponentsInChildren<TMP_Text>())Assert.That(caption.text.Contains("No translation"),Is.False,caption.name);
        }
        [UnityTest] public IEnumerator ColdRussianWelcomeInitializesWithoutPreloadErrors()
        {
            yield return Load(SystemLanguage.Russian);yield return Ready("ru");
            var title=game.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Reference shop account heading");
            Assert.That(title.text,Is.EqualTo("Добро пожаловать!"));
        }
        [UnityTest] public IEnumerator ColdUkrainianWelcomeInitializesWithoutPreloadErrors()
        {
            yield return Load(SystemLanguage.Ukrainian);yield return Ready("uk");
            var title=game.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Reference shop account heading");
            Assert.That(title.text,Is.EqualTo("Ласкаво просимо!"));
        }
        [UnityTest] public IEnumerator RapidLanguageChoicesKeepLatestChoiceAndSurviveReload()
        {
            yield return Load(SystemLanguage.English);yield return Ready("en");
            string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(string code in new[]{"ru","en","uk","ru","en","uk"})game.SetGameLanguage(code);
            yield return Ready("uk");Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            yield return Load(SystemLanguage.Russian);yield return Ready("uk");
            Assert.That(game.Preferences.UiLocale,Is.EqualTo("uk"));Assert.That(game.Preferences.FollowSystemLanguage,Is.False);
        }
    }
}
#endif
