using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed class AudioFeedbackTests
    {
        CascadeGame game;string directory;bool background;
        AudioSource[] Sources=>game.Feedback.GetComponentsInChildren<AudioSource>();
        AudioSource[] Scores=>Sources.Where(s=>s.name.StartsWith("CASCADE score")).ToArray();
        static IEnumerator Await(Func<bool> condition,string message){float end=Time.realtimeSinceStartup+6;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Assert.That(condition(),Is.True,message);}
        [UnitySetUp] public IEnumerator SetUp()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeAudioFeedback",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Preferences.Sound=game.Preferences.Music=game.Preferences.Haptics=true;game.SendMessage("ApplyPreferences");game.ShowHome();
            yield return Await(()=>Scores.Any(s=>s.isPlaying&&s.volume>.1f),"Home score must begin");yield return new WaitForSecondsRealtime(.15f);
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=background;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        [UnityTest] public IEnumerator MenuClickFinishesAcrossNavigationAndReducedEffectsWithoutDuplicateVoices()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            var shop=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Shop");shop.onClick.Invoke();
            var playing=Sources.Where(s=>!s.loop&&s.isPlaying).ToArray();Assert.That(playing.Length,Is.EqualTo(1),"One audible click, despite nested action wrappers");var cue=playing[0];
            Assert.That(game.Interaction.IsPaused,Is.True,"Menus pause puzzle commands");yield return new WaitForSecondsRealtime(.04f);Assert.That(cue.isPlaying,Is.True,"Navigation must preserve the click tail");
            float position=cue.time;game.Preferences.EffectQuality=0;game.SendMessage("ApplyPreferences");game.ShowSettings();yield return new WaitForSecondsRealtime(.035f);
            Assert.That(cue.isPlaying,Is.True);Assert.That(cue.time,Is.GreaterThan(position));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator SoundMuteStopsCuesWhileMusicContinuesAndNewClicksResume()
        {
            game.Feedback.Sound(ToyCue.Select);yield return null;Assert.That(Sources.Any(s=>!s.loop&&s.isPlaying),Is.True);
            game.Preferences.Sound=false;game.SendMessage("ApplyPreferences");yield return null;yield return null;
            Assert.That(Sources.Where(s=>!s.loop).All(s=>!s.isPlaying),Is.True);Assert.That(Scores.Any(s=>s.isPlaying&&s.volume>.1f),Is.True);
            game.Preferences.Sound=true;game.SendMessage("ApplyPreferences");yield return new WaitForSecondsRealtime(.08f);game.Feedback.Sound(ToyCue.Select);yield return new WaitForSecondsRealtime(.04f);
            Assert.That(Sources.Any(s=>!s.loop&&s.isPlaying),Is.True);
        }
        [UnityTest] public IEnumerator OutputResetAndForegroundReturnRecoverTheEnabledThemeNearItsOldPosition()
        {
            yield return new WaitForSecondsRealtime(.4f);var incoming=Scores.Single(s=>s.clip==game.Feedback.CurrentMusicClip);float position=incoming.time;
            foreach(var s in Sources.Where(s=>s.loop))s.Stop();
            typeof(ToyFeedbackPool).GetMethod("AudioConfigurationChanged",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game.Feedback,new object[]{true});
            float deadline=Time.realtimeSinceStartup+6;
            while((!incoming.isPlaying||incoming.volume<=.1f)&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(incoming.isPlaying&&incoming.volume>.1f,Is.True,"Stopped output loops must recover: screen="+game.CurrentScreen+" suspended="+game.Interaction.IsSuspended+" flags="+game.Feedback.MusicEnabled+" sleeping="+game.Feedback.MusicSleeping+" sources="+string.Join(";",Scores.Select(s=>s.name+":"+s.clip?.name+":"+s.clip?.loadState+":"+s.isPlaying+":"+s.volume+":"+s.time)));Assert.That(incoming.time,Is.GreaterThan(position-.1f));
            float held=incoming.time;game.SendMessage("OnApplicationFocus",false);yield return Await(()=>game.Feedback.MusicSleeping,"Focus loss silences owned loops");
            game.Feedback.Sound(ToyCue.Select);Assert.That(Sources.Any(s=>!s.loop&&s.isPlaying),Is.False);
            yield return new WaitForSecondsRealtime(.12f);game.SendMessage("OnApplicationFocus",true);
            yield return Await(()=>incoming.isPlaying&&incoming.volume>.1f,"Foreground must restore the existing enabled theme");Assert.That(incoming.time,Is.GreaterThanOrEqualTo(held));
        }
        [UnityTest] public IEnumerator HapticsAreThrottledAndRespectPreferenceAndSuspension()
        {
            int before=game.Feedback.HapticRequests;game.Feedback.Haptic(.18f);game.Feedback.Haptic(.6f);Assert.That(game.Feedback.HapticRequests,Is.EqualTo(before+1));
            yield return new WaitForSecondsRealtime(.13f);game.Preferences.Haptics=false;game.SendMessage("ApplyPreferences");game.Feedback.Haptic();Assert.That(game.Feedback.HapticRequests,Is.EqualTo(before+1));
            game.Preferences.Haptics=true;game.SendMessage("ApplyPreferences");game.SendMessage("OnApplicationFocus",false);game.Feedback.Haptic();Assert.That(game.Feedback.HapticRequests,Is.EqualTo(before+1));
            game.SendMessage("OnApplicationFocus",true);game.Feedback.Haptic(.42f);Assert.That(game.Feedback.HapticRequests,Is.EqualTo(before+2));
        }
    }
}
