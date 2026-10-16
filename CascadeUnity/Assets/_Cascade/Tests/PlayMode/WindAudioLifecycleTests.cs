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
    public sealed class WindAudioLifecycleTests
    {
        static IEnumerator Await(System.Func<bool> ready,string message)
        {
            double deadline=Time.realtimeSinceStartupAsDouble+6;
            while(!ready()&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
            Assert.That(ready(),Is.True,message);
        }
        [UnityTest] public IEnumerator DisablingFeedbackSilencesWindWithoutRestartingOrReplacingItsLoop()
        {
            var owner = new GameObject("Isolated wind lifecycle");
            var clip = AudioClip.Create("Lifecycle wind", 44100, 1, 44100, false);
            try {
                var feedback = owner.AddComponent<ToyFeedbackPool>();
                var voice = owner.AddComponent<AudioSource>();
                voice.playOnAwake = false; voice.loop = true; voice.clip = clip; voice.volume = .1f;
                typeof(ToyFeedbackPool).GetField("windSource", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(feedback, voice);
                voice.Play(); yield return new WaitForSecondsRealtime(.1f);
                Assert.That(voice.isPlaying, Is.True);
                feedback.enabled = false;
                Assert.That(feedback.WindVolume, Is.Zero); Assert.That(voice.isPlaying, Is.False);
                float position = voice.time;
                yield return new WaitForSecondsRealtime(.1f);
                Assert.That(voice.time, Is.EqualTo(position).Within(.01f));
                Assert.That(voice.clip, Is.SameAs(clip));
            } finally { Object.Destroy(owner); Object.Destroy(clip); }
        }
        [UnityTest] public IEnumerator IndependentMusicAndSoundControlsAndOwnerDisablePreserveLoopPositions()
        {
            string directory=Path.Combine(Path.GetTempPath(),"CascadeAudioControls",System.Guid.NewGuid().ToString("N"));
            bool background=UnityEngine.Application.runInBackground;CascadeGame game=null;
            Camera camera=null;RenderTexture portrait=null,oldTarget=null;
            try {
                UnityEngine.Application.runInBackground=true;CascadeGame.EditorProfileDirectory=directory;
                yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
                game=Object.FindFirstObjectByType<CascadeGame>();Assert.That(game,Is.Not.Null);
                camera=Camera.main;oldTarget=camera.targetTexture;portrait=new RenderTexture(390,844,24);
                var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=portrait;scaler.enabled=true;
                game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.ShowHome();
                game.Machine.SetZone(1);game.Preferences.Music=true;game.Preferences.Sound=false;game.SendMessage("ApplyPreferences");
                var feedback=game.Feedback;var sources=feedback.GetComponentsInChildren<AudioSource>();
                var scores=sources.Where(s=>s.name=="CASCADE score A"||s.name=="CASCADE score B").ToArray();
                var wind=sources.Single(s=>s.name=="Living island wind");var rain=sources.Single(s=>s.name=="Rain beneath the mill");
                yield return Await(()=>feedback.MusicCrossfade==1&&scores.Sum(s=>s.volume)>.1f&&wind.volume+rain.volume==0,
                    "Music on / sound off must settle: screen="+game.CurrentScreen+" suspended="+game.Interaction.IsSuspended+" sleeping="+feedback.MusicSleeping+" current="+feedback.CurrentMusicClip?.name);
                Assert.That(scores.Sum(s=>s.volume),Is.GreaterThan(.1f));Assert.That(wind.volume+rain.volume,Is.Zero);
                Assert.That(feedback.CurrentMusicClip.name,Is.EqualTo("Menu_HomeAmongCloudsBalanced"));
                game.Preferences.Music=false;game.Preferences.Sound=false;game.SendMessage("ApplyPreferences");
                yield return Await(()=>feedback.MusicSleeping&&scores.Sum(s=>s.volume)==0&&wind.volume+rain.volume==0,
                    "Music off / sound off must silence both independent groups");
                Assert.That(scores.All(s=>!s.isPlaying),Is.True);Assert.That(wind.isPlaying||rain.isPlaying,Is.False);
                game.Preferences.Music=false;game.Preferences.Sound=true;game.SendMessage("ApplyPreferences");
                yield return Await(()=>feedback.MusicSleeping&&scores.Sum(s=>s.volume)==0&&rain.volume>.01f,
                    "Music off / sound on must resume ambience while the score stays asleep");
                Assert.That(feedback.MusicSleeping,Is.True);Assert.That(scores.Sum(s=>s.volume),Is.Zero);Assert.That(rain.volume,Is.GreaterThan(.01f));
                game.Preferences.Music=true;game.Preferences.Sound=true;game.SendMessage("ApplyPreferences");
                yield return Await(()=>!feedback.MusicSleeping&&feedback.MusicCrossfade==1&&scores.Sum(s=>s.volume)>.1f&&rain.volume>.01f,
                    "Music on / sound on must independently resume both groups");
                var owned=scores.Concat(new[]{wind,rain}).ToArray();float crossfade=feedback.MusicCrossfade;
                var clips=owned.Select(s=>s.clip).ToArray();feedback.Sound(ToyCue.Select);
                var cues=sources.Where(s=>s.name.StartsWith("Pooled toy voice")).ToArray();Assert.That(cues.Any(s=>s.isPlaying),Is.True);
                feedback.enabled=false;Assert.That(cues.All(s=>!s.isPlaying),Is.True);
                var positions=owned.Select(s=>s.time).ToArray();
                Assert.That(owned.All(s=>!s.isPlaying&&s.volume==0),Is.True);
                yield return new WaitForSecondsRealtime(.15f);
                for(int i=0;i<owned.Length;i++){Assert.That(owned[i].time,Is.EqualTo(positions[i]).Within(.03f));Assert.That(owned[i].clip,Is.SameAs(clips[i]));}
                Assert.That(feedback.MusicCrossfade,Is.EqualTo(crossfade));
                feedback.enabled=true;
                yield return Await(()=>!feedback.MusicSleeping&&scores.Sum(s=>s.volume)>.1f&&rain.volume>.01f,
                    "Re-enabling the audio owner must restore both enabled groups through their gain ramps");
                Assert.That(feedback.MusicSleeping,Is.False);Assert.That(scores.Sum(s=>s.volume),Is.GreaterThan(.1f));Assert.That(rain.volume,Is.GreaterThan(.01f));
                var incoming=scores.Single(s=>s.clip==feedback.CurrentMusicClip);
                int index=System.Array.IndexOf(owned,incoming);Assert.That(Mathf.Repeat(incoming.time-positions[index],incoming.clip.length),Is.GreaterThan(.05f));
            } finally {
                if(camera!=null)camera.targetTexture=oldTarget;
                if(game!=null)Object.Destroy(game.gameObject);
                if(portrait!=null){portrait.Release();Object.Destroy(portrait);}
                CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=background;
            }
            yield return null;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
    }
}
