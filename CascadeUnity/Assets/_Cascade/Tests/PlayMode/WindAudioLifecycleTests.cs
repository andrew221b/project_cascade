using System.Collections;
using System.Reflection;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Cascade.Tests
{
    public sealed class WindAudioLifecycleTests
    {
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
    }
}
