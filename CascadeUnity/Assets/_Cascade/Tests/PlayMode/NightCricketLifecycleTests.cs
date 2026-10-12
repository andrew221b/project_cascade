using System.Collections;
using System.Reflection;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Cascade.Tests
{
    public sealed class NightCricketLifecycleTests
    {
        [UnityTest] public IEnumerator CricketsBeginAfterQuietNeverOverlapAndStopWithTheirOwner()
        {
            var owner=new GameObject("Isolated cricket lifecycle");
            var clip=AudioClip.Create("Lifecycle cricket phrase",441000,1,44100,false);
            try {
                var feedback=owner.AddComponent<ToyFeedbackPool>();
                var source=owner.AddComponent<AudioSource>();
                source.playOnAwake=false;source.loop=false;source.clip=clip;
                typeof(ToyFeedbackPool).GetField("cricketSource",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(feedback,source);
                var tick=typeof(ToyFeedbackPool).GetMethod("TickNightCrickets",BindingFlags.Instance|BindingFlags.NonPublic);
                for(int i=0;i<44;i++)tick.Invoke(feedback,new object[]{.1f,true});
                Assert.That(source.isPlaying,Is.False,"Night should begin with quiet.");
                for(int i=0;i<2;i++)tick.Invoke(feedback,new object[]{.1f,true});
                Assert.That(source.isPlaying,Is.True);Assert.That(source.volume,Is.InRange(.01f,.085f));
                yield return new WaitForSecondsRealtime(.1f);
                float position=source.time;
                for(int i=0;i<200;i++)tick.Invoke(feedback,new object[]{.1f,true});
                Assert.That(source.isPlaying,Is.True);Assert.That(source.time,Is.GreaterThanOrEqualTo(position),"A new phrase must not replace a playing one.");
                source.Stop();
                for(int i=0;i<80;i++)tick.Invoke(feedback,new object[]{.1f,true});
                Assert.That(source.isPlaying,Is.False,"At least nine quiet seconds separate phrases.");
                for(int i=0;i<12;i++)tick.Invoke(feedback,new object[]{.1f,true});
                Assert.That(source.isPlaying,Is.True);
                tick.Invoke(feedback,new object[]{.1f,false});
                Assert.That(source.isPlaying,Is.False);Assert.That(source.volume,Is.Zero,"Disallowing ambience stops the phrase immediately.");
                for(int i=0;i<46;i++)tick.Invoke(feedback,new object[]{.1f,true});
                Assert.That(source.isPlaying,Is.True);
                feedback.SendMessage("OnApplicationPause",true);
                Assert.That(source.isPlaying,Is.False);Assert.That(source.volume,Is.Zero);
                for(int i=0;i<46;i++)tick.Invoke(feedback,new object[]{.1f,true});
                Assert.That(source.isPlaying,Is.True);
                feedback.enabled=false;
                Assert.That(source.isPlaying,Is.False);Assert.That(source.volume,Is.Zero);
            } finally { Object.Destroy(owner);Object.Destroy(clip); }
        }
    }
}
