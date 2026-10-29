#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class WildlifeAudioTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture portrait,oldTarget;string directory;bool background,audio;
        Image picture;WildlifeAudioEmitter actor;WildlifeAudioEmitter[] residents;
        ToyFeedbackPool Feedback=>game.Feedback;
        AudioSource[] Voices=>Feedback.GetComponentsInChildren<AudioSource>().Where(s=>s.name.StartsWith("Positioned wildlife voice ")).ToArray();
        AudioSource Cricket=>Feedback.GetComponentsInChildren<AudioSource>().Single(s=>s.name=="Night observatory crickets");
        object Field(string name)=>typeof(CascadeGame).GetField(name,Private).GetValue(game);
        void Invoke(string method,params object[] args)=>typeof(ToyFeedbackPool).GetMethod(method,Private).Invoke(Feedback,args);
        void Pulse(float seconds,bool allowed=true){for(float at=0;at<seconds-.0001f;at+=.05f)Invoke("TickWildlifeAudio",.05f,allowed);}
        void Position(float x,float width=40)
        {
            picture.rectTransform.sizeDelta=new Vector2(width,width);
            float depth=camera.WorldToViewportPoint(picture.transform.position).z;
            picture.transform.position=camera.ViewportToWorldPoint(new Vector3(x,.6f,depth));
        }
        static IEnumerator Await(Func<bool> condition,string message)
        {
            float end=Time.realtimeSinceStartup+6;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;
            Assert.That(condition(),Is.True,message);
        }
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=false;
            directory=Path.Combine(Path.GetTempPath(),"CascadeWildlifeAudio",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;portrait=new RenderTexture(390,844,24);
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=portrait;scaler.enabled=true;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.SetEditorSafeInsets(new Vector4(0,.00001f,0,.00001f));
            game.Preferences.Tutorials=false;game.Preferences.Sound=true;game.Preferences.Music=false;game.Coach.Enabled=false;
            var profile=(PlayerProfile)Field("profile");profile.AccountIntroSeen=true;profile.Missions.NextAfterClears=20000;
            profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            ((HashSet<int>)Field("heardStory")).UnionWith(Enumerable.Range(0,10000));game.ShowHome();game.SendMessage("ApplyPreferences");yield return null;yield return null;
            Assert.That(game.ConversationOpen,Is.False);Assert.That(game.Interaction.IsSuspended,Is.False);
            residents=game.GetComponentsInChildren<WildlifeAudioEmitter>(true);foreach(var resident in residents)resident.enabled=false;
            var body=new GameObject("Audio test painted actor",typeof(RectTransform),typeof(Image));body.transform.SetParent(game.GetComponentInChildren<Canvas>().transform,false);
            picture=body.GetComponent<Image>();picture.raycastTarget=false;picture.color=Color.white;actor=body.AddComponent<WildlifeAudioEmitter>();actor.Bind(game,picture,WildlifeSpecies.Robin);Position(.2f);
            Invoke("StopWildlifeAudio");typeof(ToyFeedbackPool).GetField("duck",Private).SetValue(Feedback,0f);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}CascadeGame.EditorProfileDirectory=null;AudioListener.pause=audio;UnityEngine.Application.runInBackground=background;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        [UnityTest] public IEnumerator LicensedPhrasesLoadAsSmallMonoResidentClips()
        {
            int samples=0;
            foreach(string name in new[]{"SongbirdA","SongbirdB","Seabird","Owl","Bumblebee"}){
                var clip=Resources.Load<AudioClip>("CascadeArt/Audio/Wildlife/"+name);Assert.That(clip,Is.Not.Null,name);
                yield return Await(()=>clip.loadState==AudioDataLoadState.Loaded,"Preloaded "+name);
                Assert.That(clip.channels,Is.EqualTo(1));Assert.That(clip.frequency,Is.EqualTo(32000));Assert.That(clip.loadType,Is.EqualTo(AudioClipLoadType.DecompressOnLoad));
                var pcm=new float[clip.samples];Assert.That(clip.GetData(pcm,0),Is.True);Assert.That(pcm.All(v=>!float.IsNaN(v)&&!float.IsInfinity(v)&&Math.Abs(v)<.99f),Is.True);
                Assert.That(pcm.Max(v=>Math.Abs(v)),Is.GreaterThan(.2f));samples+=clip.samples;
            }
            Assert.That(samples*4,Is.LessThan(1024*1024),"The five short resident phrases stay below 1 MiB decoded");Assert.That(Voices.Length,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator FlightContactsPerchedSongsAndNeighboursHaveDifferentVoices()
        {
            Pulse(1.5f);var source=Voices.Single(s=>s.isPlaying);
            Assert.That(source.clip.name,Does.Contain("Contact"));Assert.That(source.clip.length,Is.LessThan(.6f));
            source.Stop();actor.Resting=true;Pulse(14);source=Voices.Single(s=>s.isPlaying);
            Assert.That(source.clip.name,Does.StartWith("SongbirdRobin"));Assert.That(source.clip.length,Is.GreaterThan(2));var first=source.clip;
            source.Stop();Pulse(14);source=Voices.Single(s=>s.isPlaying);Assert.That(source.clip,Is.Not.SameAs(first),"An individual does not repeat its previous phrase");
            var body=new GameObject("Neighbour songbird",typeof(RectTransform),typeof(Image));body.transform.SetParent(picture.transform.parent,false);
            var image=body.GetComponent<Image>();image.rectTransform.sizeDelta=new Vector2(40,40);image.color=Color.white;
            image.transform.position=camera.ViewportToWorldPoint(new Vector3(.8f,.6f,camera.WorldToViewportPoint(picture.transform.position).z));
            var neighbour=body.AddComponent<WildlifeAudioEmitter>();neighbour.Bind(game,image,WildlifeSpecies.WinterWren);neighbour.Resting=true;
            Pulse(2);Assert.That(Voices.Count(s=>s.isPlaying),Is.EqualTo(2));
            Assert.That(Voices.Select(s=>s.clip).Distinct().Count(),Is.EqualTo(2));Assert.That(Voices.Any(s=>s.clip.name.StartsWith("SongbirdWren")),Is.True);
            Assert.That(Voices.Min(s=>s.panStereo),Is.LessThan(-.3f));Assert.That(Voices.Max(s=>s.panStereo),Is.GreaterThan(.3f));yield return null;
        }
        [UnityTest] public IEnumerator CricketChangesItsChirpPatternAndBeePitchFollowsMotion()
        {
            actor.SetSpecies(WildlifeSpecies.FieldCricket);actor.Resting=true;
            var phrases=new HashSet<AudioClip>();
            for(int i=0;i<3;i++){Pulse(i==0?4.7f:12);Assert.That(Cricket.isPlaying,Is.True);Assert.That(Cricket.clip.name,Does.StartWith("Cricket"));phrases.Add(Cricket.clip);Cricket.Stop();}
            Assert.That(phrases.Count,Is.EqualTo(3));
            actor.SetSpecies(WildlifeSpecies.Bumblebee);actor.Resting=false;Pulse(2);var source=Voices.Single(s=>s.isPlaying);Pulse(1);float restingPitch=source.pitch;
            for(int i=1;i<=8;i++){Position(.2f+i*.06f);Pulse(.1f);}
            Assert.That(source.pitch,Is.GreaterThan(restingPitch+.05f),"Flight effort changes the wing buzz smoothly");
            actor.Resting=true;Pulse(.4f);Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero);yield return null;
        }
        [UnityTest] public IEnumerator AdditionalNaturalPhrasesAreDistinctMonoAndBoundedInMemory()
        {
            int samples=0;var digests=new HashSet<int>();
            foreach(string name in new[]{"SongbirdContactA","SongbirdContactB","SongbirdRobinA","SongbirdRobinB","SongbirdWrenA","SongbirdWrenB","SeabirdContact","OwlAnswer","CricketA","CricketB","CricketC"}){
                var clip=Resources.Load<AudioClip>("CascadeArt/Audio/Wildlife/"+name);Assert.That(clip,Is.Not.Null,name);
                yield return Await(()=>clip.loadState==AudioDataLoadState.Loaded,"Resident "+name);
                Assert.That(clip.channels,Is.EqualTo(1));Assert.That(clip.frequency,Is.EqualTo(32000));Assert.That(clip.loadType,Is.EqualTo(AudioClipLoadType.DecompressOnLoad));
                var pcm=new float[clip.samples];Assert.That(clip.GetData(pcm,0),Is.True);Assert.That(pcm.Max(v=>Math.Abs(v)),Is.InRange(.2f,.95f));
                int digest=17;for(int i=0;i<pcm.Length;i+=37)unchecked{digest=digest*31+(int)(pcm[i]*32767);}Assert.That(digests.Add(digest),Is.True,name);samples+=clip.samples;
            }
            Assert.That(samples*4,Is.LessThan(2300*1024));Assert.That(Voices.Length,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator AVisibleBirdFollowsThePaintedPositionAndFadesWhenHidden()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;Pulse(1.5f);var source=Voices.Single(s=>s.isPlaying);
            Assert.That(source.clip.name,Does.StartWith("Songbird"));Assert.That(source.panStereo,Is.LessThan(-.3f));Assert.That(source.volume,Is.InRange(.01f,.12f));
            Position(.8f);Pulse(.5f);Assert.That(source.panStereo,Is.GreaterThan(.3f));float near=source.volume;
            Position(.8f,10);picture.color=new Color(1,1,1,.4f);Pulse(.25f);Assert.That(source.volume,Is.LessThan(near*.6f));
            picture.enabled=false;Pulse(.3f);Assert.That(Voices.All(s=>!s.isPlaying&&s.volume==0),Is.True);
            picture.enabled=true;Position(1.2f);Pulse(4);
            Assert.That(source.volume==0||source.clip.name.StartsWith("Songbird"),Is.True);
            Assert.That(Voices.Where(v=>v.isPlaying).All(v=>!v.loop&&v.clip.name.StartsWith("Songbird")),Is.True,"Only the quiet distant birds remain outside the viewport");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));yield return null;
        }
        [UnityTest] public IEnumerator InsectsUseTheirActualFlightOrRestStateWithoutInventedButterflySounds()
        {
            actor.SetSpecies(WildlifeSpecies.Bumblebee);Pulse(1.5f);Assert.That(Voices.Count(s=>s.isPlaying&&s.loop&&s.clip.name=="Bumblebee"),Is.EqualTo(1));
            actor.Resting=true;Pulse(.4f);Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero);
            foreach(var species in new[]{WildlifeSpecies.Butterfly,WildlifeSpecies.LunaMoth,WildlifeSpecies.Dragonfly}){
                actor.SetSpecies(species);Pulse(8);
                Assert.That(Voices.Where(v=>v.isPlaying).All(v=>!v.loop&&v.clip.name.StartsWith("Songbird")),Is.True,"Silent insect adds no voice of its own: "+species);
            }
            actor.SetSpecies(WildlifeSpecies.FieldCricket);actor.Resting=false;Pulse(6);Assert.That(Cricket.isPlaying,Is.False);
            actor.Resting=true;Pulse(4.7f);Assert.That(Cricket.isPlaying,Is.True);Assert.That(Cricket.panStereo,Is.LessThan(-.3f));
            picture.enabled=false;Pulse(.1f);Assert.That(Cricket.isPlaying,Is.False);Assert.That(Cricket.volume,Is.Zero);yield return null;
        }
        [UnityTest] public IEnumerator MuteFocusAdSuspensionAndDeviceResetStopVoicesWithoutReturnBursts()
        {
            actor.SetSpecies(WildlifeSpecies.Bumblebee);Pulse(1.5f);Assert.That(Feedback.ActiveWildlifeVoices,Is.GreaterThan(0));
            game.Preferences.Sound=false;game.SendMessage("ApplyPreferences");yield return null;Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero);
            game.Preferences.Sound=true;game.SendMessage("ApplyPreferences");Pulse(14);yield return null;Assert.That(Feedback.ActiveWildlifeVoices,Is.GreaterThan(0));
            game.SendMessage("OnApplicationFocus",false);yield return null;Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero);
            game.SendMessage("OnApplicationFocus",true);yield return null;Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero,"No burst on foreground return");
            Pulse(14);game.Interaction.SetSuspended(true);yield return null;Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero,"Native ad suspension silences live fauna");
            game.Interaction.SetSuspended(false);Pulse(14);Invoke("AudioConfigurationChanged",true);yield return null;Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero,"Device reset has a quiet re-entry");
            Pulse(14);Feedback.enabled=false;Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero);Feedback.enabled=true;yield return null;Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero);
        }
        [UnityTest] public IEnumerator SongbirdPhrasesComeInReadableBurstsWithNaturalBreathingRoom()
        {
            actor.Resting=true;var clips=new HashSet<AudioClip>();int calls=0;float last=-100,maxGap=0;
            int owned=Feedback.GetComponentsInChildren<AudioSource>().Length;
            for(int step=0;step<1200;step++){
                Pulse(.05f);
                foreach(var voice in Voices)if(voice.isPlaying){
                    calls++;clips.Add(voice.clip);float at=step*.05f;if(last>=0)maxGap=Mathf.Max(maxGap,at-last);last=at;
                    // Advance the scheduler with a completed recording, not wall-clock waiting.
                    voice.Stop();
                }
            }
            Assert.That(calls,Is.InRange(10,24));Assert.That(clips.Count,Is.GreaterThanOrEqualTo(4));
            Assert.That(maxGap,Is.LessThan(10));Assert.That(Feedback.GetComponentsInChildren<AudioSource>().Length,Is.EqualTo(owned));
            Assert.That(Voices.Length,Is.EqualTo(2));yield return null;
        }
        [UnityTest] public IEnumerator EmptySkyKeepsQuietDistantAnswersAndSuspensionStopsThem()
        {
            actor.enabled=false;Invoke("StopWildlifeAudio");
            int owned=Feedback.GetComponentsInChildren<AudioSource>().Length;var clips=new HashSet<AudioClip>();
            bool left=false,right=false;int calls=0;float last=-100,maxGap=0;
            for(int step=0;step<1200;step++){
                Pulse(.05f);
                foreach(var voice in Voices)if(voice.isPlaying){
                    calls++;clips.Add(voice.clip);left|=voice.panStereo<-.2f;right|=voice.panStereo>.2f;
                    float at=step*.05f;if(last>=0)maxGap=Mathf.Max(maxGap,at-last);last=at;voice.Stop();
                }
            }
            Assert.That(calls,Is.InRange(9,22));Assert.That(clips.Count,Is.GreaterThanOrEqualTo(4));
            Assert.That(left&&right,Is.True);Assert.That(maxGap,Is.LessThan(10));
            Assert.That(Feedback.AmbientBirdCallsStarted,Is.GreaterThanOrEqualTo(calls));
            Assert.That(Feedback.GetComponentsInChildren<AudioSource>().Length,Is.EqualTo(owned));
            Pulse(10);Pulse(.05f,false);Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero);
            Pulse(.5f);Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero,"Quiet re-entry after suspension");
            game.OpenWorldComic(0);Pulse(15);Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero,"Reading remains quiet");
            yield return null;
        }
        [UnityTest] public IEnumerator NewRobinAndWrenRecordingsAreDistinctSmallMonoPhrases()
        {
            int samples=0;var hashes=new HashSet<int>();
            foreach(string name in new[]{"SongbirdRobinC","SongbirdRobinD","SongbirdWrenC","SongbirdWrenD"}){
                var clip=Resources.Load<AudioClip>("CascadeArt/Audio/Wildlife/"+name);Assert.That(clip,Is.Not.Null);
                yield return Await(()=>clip.loadState==AudioDataLoadState.Loaded,name);
                Assert.That(clip.channels,Is.EqualTo(1));Assert.That(clip.frequency,Is.EqualTo(32000));
                Assert.That(clip.loadType,Is.EqualTo(AudioClipLoadType.DecompressOnLoad));Assert.That(clip.length,Is.InRange(2,3));
                var pcm=new float[clip.samples];Assert.That(clip.GetData(pcm,0),Is.True);
                Assert.That(pcm.All(v=>!float.IsNaN(v)&&!float.IsInfinity(v)&&Math.Abs(v)<.99f),Is.True);
                int hash=17;for(int i=0;i<pcm.Length;i+=37)unchecked{hash=hash*31+(int)(pcm[i]*32767);}
                Assert.That(hashes.Add(hash),Is.True);samples+=clip.samples;
            }
            Assert.That(samples*4,Is.LessThan(1300*1024));
        }
        [UnityTest] public IEnumerator LiveMenuAndGameplayActorsRegisterWithoutGrowingAudioVoices()
        {
            UnityEngine.Object.Destroy(picture.gameObject);yield return null;foreach(var resident in residents)if(resident!=null)resident.enabled=true;yield return null;
            var home=game.GetComponentsInChildren<HomeSkyLife>().Single();Assert.That(home.GetComponentsInChildren<WildlifeAudioEmitter>().Length,Is.EqualTo(2));
            var frame=game.GetComponentsInChildren<WorldFrameResident>().Single();Assert.That(frame.GetComponent<WildlifeAudioEmitter>(),Is.Not.Null);
            int count=Feedback.GetComponentsInChildren<AudioSource>().Length;
            for(int i=0;i<3;i++){game.ShowSettings();yield return null;game.ShowHome();yield return null;Assert.That(Feedback.GetComponentsInChildren<AudioSource>().Length,Is.EqualTo(count));}
            game.LoadNode("C02");game.ShowGame();yield return Await(()=>game.Interaction.CanPlay,"Live gameplay is ready");
            var life=game.Machine.LivingWorld.GetComponent<WorldLifeView>();Assert.That(life.GetComponentsInChildren<WildlifeAudioEmitter>().Count(e=>e.name.Contains("wildlife")||e.name.Contains("visitor")),Is.EqualTo(7));
            Assert.That(Feedback.RegisteredWildlifeCount,Is.InRange(7,16));Assert.That(Voices.Length,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator MusicAndWildlifeKeepIndependentPreferencesInAPortraitGame()
        {
            actor.SetSpecies(WildlifeSpecies.Bumblebee);Pulse(1.5f);
            game.Preferences.Music=true;game.SendMessage("ApplyPreferences");
            var scores=Feedback.GetComponentsInChildren<AudioSource>().Where(s=>s.name.StartsWith("CASCADE score")).ToArray();
            yield return Await(()=>scores.Any(s=>s.isPlaying&&s.volume>.1f),"Music starts independently of the animal voices");
            game.Preferences.Sound=false;game.SendMessage("ApplyPreferences");yield return null;
            Assert.That(Feedback.ActiveWildlifeVoices,Is.Zero);Assert.That(scores.Any(s=>s.isPlaying&&s.volume>.1f),Is.True);
            game.Preferences.Music=false;game.Preferences.Sound=true;game.SendMessage("ApplyPreferences");Pulse(14);
            yield return Await(()=>Feedback.MusicSleeping,"Muted music fades out");Assert.That(Feedback.ActiveWildlifeVoices,Is.GreaterThan(0));
        }
        [UnityTest] public IEnumerator TheListenerReceivesNonzeroStereoSoundFromTheVisibleSide()
        {
            foreach(var source in Feedback.GetComponentsInChildren<AudioSource>())if(!source.name.StartsWith("Positioned wildlife voice "))source.mute=true;
            actor.SetSpecies(WildlifeSpecies.Bumblebee);Position(.15f);Pulse(1.5f);
            var left=new float[1024];var right=new float[1024];double leftEnergy=0,rightEnergy=0;
            yield return new WaitForSecondsRealtime(.2f);
            for(int i=0;i<12;i++){
                AudioListener.GetOutputData(left,0);AudioListener.GetOutputData(right,1);
                for(int n=0;n<left.Length;n++){leftEnergy+=left[n]*left[n];rightEnergy+=right[n]*right[n];}
                yield return new WaitForSecondsRealtime(.025f);
            }
            Assert.That(leftEnergy,Is.GreaterThan(.000001));Assert.That(leftEnergy,Is.GreaterThan(rightEnergy*1.2),"Actual output favours the bird/insect side");
            Position(.85f);yield return new WaitForSecondsRealtime(.5f);leftEnergy=rightEnergy=0;
            for(int i=0;i<12;i++){
                AudioListener.GetOutputData(left,0);AudioListener.GetOutputData(right,1);
                for(int n=0;n<left.Length;n++){leftEnergy+=left[n]*left[n];rightEnergy+=right[n]*right[n];}
                yield return new WaitForSecondsRealtime(.025f);
            }
            Assert.That(rightEnergy,Is.GreaterThan(.000001));Assert.That(rightEnergy,Is.GreaterThan(leftEnergy*1.2),"Output follows the actor across the sky");
        }
    }
}
#endif
