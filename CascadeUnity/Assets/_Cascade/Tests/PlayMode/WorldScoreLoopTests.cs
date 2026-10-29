#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Cascade.Tests
{
    public sealed class WorldScoreLoopTests
    {
        [Serializable] sealed class Observation
        {
            public string clip,path;
            public int frequency,channels,samples,wraps;
            public float seconds,minimumTime,maximumTime;
            public double listenerEnergy;
            public bool streaming,background,looping;
        }
        [Serializable] sealed class Evidence { public Observation[] clips; }

        [UnityTest] public IEnumerator AllTenPreparedScoresImportStreamAndWrapWithLiveAudio()
        {
            var library=Resources.Load<ToyAudioLibrary>("CascadeArt/ToyAudio");
            Assert.That(library,Is.Not.Null);
            var clips=new[]{library.menuMusic}.Concat(library.worldMusic).ToArray();
            Assert.That(clips.Length,Is.EqualTo(10));
            Assert.That(clips.All(c=>c!=null),Is.True);
            Assert.That(clips.Distinct().Count(),Is.EqualTo(10));
            var listeners=UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            var enabled=listeners.Select(l=>l.enabled).ToArray();
            foreach(var listener in listeners)listener.enabled=false;
            bool oldPause=AudioListener.pause;AudioListener.pause=false;
            bool oldBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            var go=new GameObject("Temporary prepared score loop validation");
            go.AddComponent<AudioListener>();
            var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=true;
            source.spatialBlend=0;source.volume=.6f;source.ignoreListenerPause=true;
            var observations=new List<Observation>();var pcm=new float[2048];
            try {
                foreach(var clip in clips){
                    string path=AssetDatabase.GetAssetPath(clip);
                    var importer=AssetImporter.GetAtPath(path) as AudioImporter;
                    Assert.That(importer,Is.Not.Null,path);
                    Assert.That(clip.channels,Is.EqualTo(2),clip.name);
                    Assert.That(clip.frequency,Is.EqualTo(48000),clip.name);
                    Assert.That(clip.length,Is.InRange(45f,180f),clip.name);
                    Assert.That(importer.defaultSampleSettings.loadType,Is.EqualTo(AudioClipLoadType.Streaming),clip.name);
                    Assert.That(importer.loadInBackground,Is.True,clip.name);
                    clip.LoadAudioData();float deadline=Time.realtimeSinceStartup+12;
                    while(clip.loadState!=AudioDataLoadState.Loaded&&Time.realtimeSinceStartup<deadline)yield return null;
                    Assert.That(clip.loadState,Is.EqualTo(AudioDataLoadState.Loaded),clip.name);
                    source.Stop();source.clip=clip;source.Play();yield return null;
                    source.timeSamples=clip.samples-Mathf.RoundToInt(.55f*clip.frequency);
                    yield return null;
                    int wraps=0;float previous=source.time,min=clip.length,max=0;
                    double energy=0;float until=Time.realtimeSinceStartup+1.35f;
                    while(Time.realtimeSinceStartup<until){
                        Assert.That(source.isPlaying,Is.True,clip.name+" stopped near the wrap");
                        float position=source.time;
                        if(previous-position>.15f)wraps++;
                        previous=position;min=Mathf.Min(min,position);max=Mathf.Max(max,position);
                        AudioListener.GetOutputData(pcm,0);
                        energy+=pcm.Sum(v=>(double)v*v);
                        yield return new WaitForSecondsRealtime(.02f);
                    }
                    Assert.That(wraps,Is.EqualTo(1),clip.name+" must cross its actual loop boundary once");
                    Assert.That(energy,Is.GreaterThan(.00001),clip.name+" must reach actual listener output");
                    Assert.That(source.time,Is.LessThan(2f),clip.name+" should continue at the beginning");
                    observations.Add(new Observation{clip=clip.name,path=path,frequency=clip.frequency,channels=clip.channels,
                        samples=clip.samples,seconds=clip.length,wraps=wraps,minimumTime=min,maximumTime=max,
                        listenerEnergy=energy,streaming=clip.loadType==AudioClipLoadType.Streaming,
                        background=importer.loadInBackground,looping=source.loop});
                }
                string output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../worldscore-native-loop.json"));
                File.WriteAllText(output,JsonUtility.ToJson(new Evidence{clips=observations.ToArray()},true));
            }
            finally {
                source.Stop();UnityEngine.Object.Destroy(go);AudioListener.pause=oldPause;UnityEngine.Application.runInBackground=oldBackground;
                for(int i=0;i<listeners.Length;i++)if(listeners[i]!=null)listeners[i].enabled=enabled[i];
            }
        }
    }
}
#endif
