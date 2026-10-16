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
        [Serializable] sealed class MusicObservation{public int zone;public string clip;public float seconds,gain;public double listenerEnergy;}
        [Serializable] sealed class MusicEvidence{public MusicObservation[] worlds;}
        CascadeGame game;string directory;bool background;Camera camera;RenderTexture portrait,oldTarget;
        AudioSource[] Sources=>game.Feedback.GetComponentsInChildren<AudioSource>();
        AudioSource[] Scores=>Sources.Where(s=>s.name.StartsWith("CASCADE score")).ToArray();
        static IEnumerator Await(Func<bool> condition,string message){float end=Time.realtimeSinceStartup+6;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Assert.That(condition(),Is.True,message);}
        [UnitySetUp] public IEnumerator SetUp()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeAudioFeedback",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            camera=Camera.main;oldTarget=camera.targetTexture;portrait=new RenderTexture(390,844,24);
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=portrait;scaler.enabled=true;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Preferences.Tutorials=false;
            game.Preferences.Sound=game.Preferences.Music=game.Preferences.Haptics=true;game.SendMessage("ApplyPreferences");game.ShowHome();
            yield return Await(()=>Scores.Any(s=>s.isPlaying&&s.volume>.1f),"Home score must begin");yield return new WaitForSecondsRealtime(.15f);
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if(camera!=null)camera.targetTexture=oldTarget;
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=background;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        [UnityTest] public IEnumerator MenuClickFinishesAcrossNavigationAndReducedEffectsWithoutDuplicateVoices()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            var shop=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Shop");shop.onClick.Invoke();
            var playing=Sources.Where(s=>!s.loop&&s.isPlaying).ToArray();Assert.That(playing.Length,Is.EqualTo(1),"One audible click, despite nested action wrappers");var cue=playing[0];
            Assert.That(game.Interaction.IsPaused,Is.True,"Menus pause puzzle commands");
            Assert.That(cue.isPlaying,Is.True,"Navigation preserves the existing click voice");
            game.Preferences.EffectQuality=0;game.SendMessage("ApplyPreferences");game.ShowSettings();
            Assert.That(cue.isPlaying,Is.True,"Reduced visuals and navigation do not stop audio");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            yield return Await(()=>!cue.isPlaying,"The click finishes naturally without looping");
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
        [UnityTest] public IEnumerator MapEntryChangesFromMenuToTheAudibleWorldScoreAndReturns()
        {
            var library=Resources.Load<ToyAudioLibrary>("CascadeArt/ToyAudio");
            game.ShowJourney();if(game.ComicWorld>=0)game.FinishWorldComic();
            for(int page=0;page<8&&game.ConversationOpen;page++)game.SkipConversation();yield return null;
            yield return Await(()=>game.Feedback.CurrentMusicClip==library.worldMusic[game.EnvironmentWorld]&&game.Feedback.MusicCrossfade==1,"Map must play its selected world's theme");
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Travel").onClick.Invoke();
            float deadline=Time.realtimeSinceStartup+30;
            while(game.CurrentScreen!=GameScreen.Playing&&Time.realtimeSinceStartup<deadline){
                if(game.ConversationOpen)game.SkipConversation();yield return null;
            }
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing),"The real map action enters a level");
            yield return Await(()=>game.Feedback.CurrentMusicClip==library.worldMusic[0]&&game.Feedback.MusicCrossfade==1,
                "Starting a level must replace the menu theme: zone="+game.Machine.LivingWorld.Zone+" screen="+game.CurrentScreen+" current="+game.Feedback.CurrentMusicClip?.name+" desired="+library.worldMusic[0].name+" fade="+game.Feedback.MusicCrossfade+" suspended="+game.Interaction.IsSuspended+" sleeping="+game.Feedback.MusicSleeping+" sources="+string.Join(";",Scores.Select(s=>s.name+":"+s.clip?.name+":"+s.clip?.loadState+":"+s.isPlaying+":"+s.volume)));
            var active=Scores.Single(s=>s.clip==library.worldMusic[0]);
            Assert.That(active.isPlaying&&active.volume>.1f,Is.True,"The selected world score must actually be audible");
            Assert.That(Scores.Where(s=>s!=active).All(s=>s.volume==0),Is.True,"Menu score finishes fading out");
            // Every world has its own score; a stopped source recovers without a settings toggle.
            for(int zone=1;zone<library.worldMusic.Length;zone++){
                game.StartCampaignLevel(game.Catalog.GetPrimary(Cascade.Content.JourneyStory.NodeIds[zone*60]).Definition);game.ShowGame();
                float readyUntil=Time.realtimeSinceStartup+30;while(!game.PreparationReady&&Time.realtimeSinceStartup<readyUntil)yield return null;
                Assert.That(game.PreparationReady,Is.True,"World entry finishes preparation");
                yield return Await(()=>game.Feedback.CurrentMusicClip==library.worldMusic[zone]&&game.Feedback.MusicCrossfade==1,
                    "World "+zone+" score arrives: zone="+game.Machine.LivingWorld.Zone+" current="+game.Feedback.CurrentMusicClip?.name+" desired="+library.worldMusic[zone].name+" fade="+game.Feedback.MusicCrossfade+" suspended="+game.Interaction.IsSuspended);
                Assert.That(Scores.Single(s=>s.clip==library.worldMusic[zone]).isPlaying,Is.True);
            }
            active=Scores.Single(s=>s.clip==game.Feedback.CurrentMusicClip);float position=active.time;active.Stop();
            yield return Await(()=>active.isPlaying&&active.volume>.1f,"Stopped score resumes without a reset callback");
            Assert.That(active.time,Is.GreaterThanOrEqualTo(position-.1f));
            game.ShowJourney();yield return Await(()=>game.Feedback.CurrentMusicClip==library.worldMusic[game.EnvironmentWorld]&&game.Feedback.MusicCrossfade==1,
                "Returning to the map plays the displayed world's theme");
            game.ShowHome();yield return Await(()=>game.Feedback.CurrentMusicClip==library.menuMusic&&game.Feedback.MusicCrossfade==1,"Home restores the menu theme");
            Assert.That(Scores.Single(s=>s.clip==library.menuMusic).isPlaying,Is.True);
        }
        [UnityTest] public IEnumerator HapticsAreThrottledAndRespectPreferenceAndSuspension()
        {
            int before=game.Feedback.HapticRequests;game.Feedback.Haptic(.18f);game.Feedback.Haptic(.6f);Assert.That(game.Feedback.HapticRequests,Is.EqualTo(before+1));
            yield return new WaitForSecondsRealtime(.13f);game.Preferences.Haptics=false;game.SendMessage("ApplyPreferences");game.Feedback.Haptic();Assert.That(game.Feedback.HapticRequests,Is.EqualTo(before+1));
            game.Preferences.Haptics=true;game.SendMessage("ApplyPreferences");game.SendMessage("OnApplicationFocus",false);game.Feedback.Haptic();Assert.That(game.Feedback.HapticRequests,Is.EqualTo(before+1));
            game.SendMessage("OnApplicationFocus",true);game.Feedback.Haptic(.42f);Assert.That(game.Feedback.HapticRequests,Is.EqualTo(before+2));
            // A wood impact on the same frame must not be swallowed by the quiet tap cooldown.
            game.Feedback.Haptic(ToyHapticMoment.WoodCrack);
            Assert.That(game.Feedback.HapticRequests,Is.EqualTo(before+3));
            yield return new WaitForSecondsRealtime(.13f);
            game.Feedback.Haptic(ToyHapticMoment.Breeze);
            game.Feedback.Haptic(ToyHapticMoment.Breeze);
            Assert.That(game.Feedback.HapticRequests,Is.EqualTo(before+4),"A small breeze pulses once, then rests.");
        }
        [UnityTest] public IEnumerator EveryMapWorldIsAudibleAndRapidReturnReversesTheExistingFade()
        {
            var library=Resources.Load<ToyAudioLibrary>("CascadeArt/ToyAudio");
            var page=typeof(CascadeGame).GetField("mapPage",BindingFlags.Instance|BindingFlags.NonPublic);
            var navigate=typeof(CascadeGame).GetMethod("SetScreen",BindingFlags.Instance|BindingFlags.NonPublic);
            game.Preferences.Sound=false;game.SendMessage("ApplyPreferences");AudioListener.pause=false;
            page.SetValue(game,0);navigate.Invoke(game,new object[]{GameScreen.JourneyMap});
            yield return Await(()=>game.Feedback.CurrentMusicClip==library.worldMusic[0],"Map starts its world score");
            yield return new WaitForSecondsRealtime(.35f);
            Assert.That(Scores.Count(s=>s.isPlaying&&s.volume>.015f),Is.EqualTo(2),"Both compositions take part in the fade");
            var outgoing=Scores.Single(s=>s.clip==library.menuMusic);float held=outgoing.time,gain=outgoing.volume;
            game.ShowHome();yield return null;
            Assert.That(game.Feedback.CurrentMusicClip,Is.SameAs(library.menuMusic));
            Assert.That(outgoing.time,Is.GreaterThanOrEqualTo(held),"A reversed transition continues the existing performance");
            Assert.That(outgoing.volume,Is.EqualTo(gain).Within(.065f),"No gain jump when reversing a fade");
            yield return Await(()=>game.Feedback.MusicCrossfade==1,"Reversal finishes");
            var observations=new System.Collections.Generic.List<MusicObservation>();var pcm=new float[1024];
            for(int zone=0;zone<9;zone++){
                page.SetValue(game,zone);navigate.Invoke(game,new object[]{GameScreen.JourneyMap});
                yield return Await(()=>game.Feedback.CurrentMusicClip==library.worldMusic[zone]&&game.Feedback.MusicCrossfade==1,"Selected map world "+zone);
                double energy=0;for(int n=0;n<8;n++){AudioListener.GetOutputData(pcm,0);energy+=pcm.Sum(v=>(double)v*v);yield return new WaitForSecondsRealtime(.025f);}
                var incoming=Scores.Single(s=>s.clip==library.worldMusic[zone]);
                Assert.That(incoming.isPlaying&&incoming.volume>.1f,Is.True);Assert.That(energy,Is.GreaterThan(.00001),"Actual listener output, world "+zone);
                Assert.That(Scores.Where(s=>s!=incoming).All(s=>s.volume==0&&!s.isPlaying),Is.True);
                observations.Add(new MusicObservation{zone=zone,clip=incoming.clip.name,seconds=incoming.time,gain=incoming.volume,listenerEnergy=energy});
            }
            string report=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../audio-worlds-listener.json"));
            File.WriteAllText(report,JsonUtility.ToJson(new MusicEvidence{worlds=observations.ToArray()},true));
        }
        [UnityTest] public IEnumerator CalmGameplayHasNoAutomaticWindTailButAnExplicitGustStillResponds()
        {
            game.LoadNode("C02");game.ShowGame();game.Coach.Enabled=false;
            for(int i=0;i<30&&!game.Interaction.CanPlay;i++){if(game.MechanicPopupOpen)game.AcknowledgeMechanic();yield return null;}
            yield return Await(()=>game.Interaction.CanPlay,"The real gameplay is ready");
            game.Preferences.Music=false;game.Preferences.Sound=true;game.SendMessage("ApplyPreferences");
            var machine=game.Machine;var type=machine.GetType();const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var clock=type.GetField("windTime",flags);type.GetField("gustStarted",flags).SetValue(machine,-100f);
            type.GetField("gustDirection",flags).SetValue(machine,0f);type.GetField("nextAmbientGust",flags).SetValue(machine,0f);
            string hash=game.Interaction.Attempt.CurrentStateHash;clock.SetValue(machine,26.6f);
            type.GetMethod("LateUpdate",flags).Invoke(machine,null);clock.SetValue(machine,27.1f);
            Assert.That(machine.WindAt(0),Is.Zero,"Automatic scenery impulses must not create wind during a calm");
            var tick=typeof(ToyFeedbackPool).GetMethod("TickAtmosphere",flags);
            for(int i=0;i<40;i++)tick.Invoke(game.Feedback,new object[]{.05f,false});
            Assert.That(game.Feedback.WindVolume,Is.Zero);
            machine.CueWind(true);clock.SetValue(machine,27.6f);
            for(int i=0;i<6;i++)tick.Invoke(game.Feedback,new object[]{.05f,false});
            Assert.That(machine.WindAt(0),Is.GreaterThan(.6f));Assert.That(game.Feedback.WindVolume,Is.GreaterThan(.02f));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
    }
}
