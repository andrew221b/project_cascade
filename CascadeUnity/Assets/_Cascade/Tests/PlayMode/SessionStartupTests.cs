#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Cascade.Application;
using Cascade.Content;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Cascade.Tests
{
    public sealed class SessionStartupTests
    {
        CascadeGame game;
        string directory;
        bool background,audio;
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeSessionStartup",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            CascadeGame.EditorProfileDirectory=null;
            UnityEngine.Application.runInBackground=background;AudioListener.pause=audio;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        IEnumerator LoadFresh()
        {
            new ProfileStore(Path.Combine(directory,"cascade-profile.json")).Save(new PlayerProfile{AccountIntroSeen=true});
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            yield return null;
        }
        [UnityTest] public IEnumerator ShortAbsenceKeepsRoundButLongAbsenceReturnsHome()
        {
            yield return LoadFresh();game.ShowGame();
            var attempt=game.Interaction.Attempt;var id=attempt.AttemptId;
            game.SendMessage("OnApplicationPause",true);
            typeof(CascadeGame).GetField("sessionAbsentUtc",Private).SetValue(game,DateTime.UtcNow.AddMinutes(-1));
            game.SendMessage("OnApplicationPause",false);yield return null;
            Assert.That(game.Interaction.Attempt.AttemptId,Is.EqualTo(id));
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));
            game.SendMessage("OnApplicationPause",true);
            typeof(CascadeGame).GetField("sessionAbsentUtc",Private).SetValue(game,DateTime.UtcNow.AddMinutes(-11));
            game.SendMessage("OnApplicationPause",false);yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            Assert.That(game.Interaction.Attempt.AttemptId,Is.Not.EqualTo(id));
            Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
            var saved=new ProfileStore(game.ProfilePath).Load().Profile;
            Assert.That(saved.Attempt,Is.Null);Assert.That(saved.Descent,Is.Null);
            Assert.That(game.PurchaseStore,Is.Not.Null);Assert.That(game.AdProvider,Is.Not.Null);
        }
        [UnityTest] public IEnumerator RewardedHintRendersBeforeEconomyInitialization()
        {
            yield return LoadFresh();
            var record=AttemptRecord.Capture(game.Interaction.Attempt);
            record.HintUsed=true;record.FreeHintStates=new[]{new string('a',64)};record.FreeHintStateHash=record.FreeHintStates[0];
            var restored=AttemptService.Restore(game.Interaction.Attempt.Level,record);
            var field=typeof(CascadeGame).GetField("monetization",Private);var ledger=field.GetValue(game);
            var virtualField=typeof(CascadeGame).GetField("virtualEconomy",Private);var virtualLedger=virtualField.GetValue(game);
            field.SetValue(game,null);virtualField.SetValue(game,null);
            try{
                Assert.DoesNotThrow(()=>typeof(CascadeGame).GetMethod("MountAttempt",Private).Invoke(game,new object[]{restored,null}));
            }finally{field.SetValue(game,ledger);virtualField.SetValue(game,virtualLedger);}
        }
        static string Source(string name)=>Resources.Load<TextAsset>("CascadeContent/"+name).text;
        [UnityTest] public IEnumerator SavedRewardedHintDoesNotInterruptStartup()
        {
            var catalog=ContentImporter.Import(Source("levels"),Source("localized_levels"),Source("words_en"),Source("words_ru"),Source("words_uk"),Source("campaign_extension"));
            var attempt=new AttemptService(catalog.GetPrimary("C01").Definition);
            var record=AttemptRecord.Capture(attempt);
            record.HintUsed=true;record.FreeHintStateHash=new string('a',64);record.FreeHintStates=new[]{record.FreeHintStateHash};
            var profile=new PlayerProfile{AccountIntroSeen=true,JourneyAttempt=true,Attempt=record};
            new ProfileStore(Path.Combine(directory,"cascade-profile.json")).Save(profile);
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Assert.That(game,Is.Not.Null);
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            yield return null;
            Assert.That(game.Recovery,Is.EqualTo(SaveRecovery.None));
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            Assert.That(game.Monetization,Is.Not.Null,"Restoring a used hint must not abort Awake before services start.");
            Assert.That(game.PurchaseStore,Is.Not.Null);
            Assert.That(game.AdProvider,Is.Not.Null);
            Assert.That(game.Interaction.Attempt.HintUsed,Is.False,"A cold start discards the old round, keeping earned progress.");
            Assert.That(new ProfileStore(game.ProfilePath).Load().Profile.Attempt,Is.Null);
        }
    }
}
#endif
