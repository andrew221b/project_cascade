using Cascade.Presentation;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class WindAudioEnvelopeTests
    {
        [Test] public void CalmAndTinyBreezeCannotSoundLikeAFullGust()
        {
            Assert.That(WindAudioEnvelope.Target(0), Is.Zero);
            Assert.That(WindAudioEnvelope.Target(.1f), Is.LessThan(WindAudioEnvelope.Target(1) * .02f));
            Assert.That(WindAudioEnvelope.Target(.5f), Is.GreaterThan(WindAudioEnvelope.Target(.1f)));
            Assert.That(WindAudioEnvelope.Target(-.7f), Is.EqualTo(WindAudioEnvelope.Target(.7f)));
            Assert.That(WindAudioEnvelope.Target(5), Is.EqualTo(WindAudioEnvelope.Target(1)));
            Assert.That(WindAudioEnvelope.Target(1, .35f), Is.LessThan(WindAudioEnvelope.Target(1)));
        }
        [Test] public void GustArrivesQuicklyAndCalmRemovesItsAudibleTail()
        {
            var sound = new WindAudioEnvelope();
            for (int i = 0; i < 15; i++) sound.Tick(1, 1, true, .02f);
            Assert.That(sound.Volume, Is.GreaterThan(.13f));
            for (int i = 0; i < 30; i++) sound.Tick(.05f, 1, true, .02f);
            Assert.That(sound.Volume, Is.LessThan(.005f));
            for (int i = 0; i < 60; i++) sound.Tick(0, 1, true, .02f);
            Assert.That(sound.Volume, Is.Zero);
        }
        [Test] public void EnvelopeDoesNotDependOnFramePartition()
        {
            var fast = new WindAudioEnvelope(); var slow = new WindAudioEnvelope();
            for (int i = 0; i < 50; i++) fast.Tick(.8f, 1, true, .01f);
            for (int i = 0; i < 10; i++) slow.Tick(.8f, 1, true, .05f);
            Assert.That(fast.Volume, Is.EqualTo(slow.Volume).Within(.00001f));
        }
        [Test] public void MuteOrSuspensionSilencesEvenAnOngoingGustAndResetLeavesNoTail()
        {
            var sound = new WindAudioEnvelope();
            for (int i = 0; i < 20; i++) sound.Tick(1, 1, true, .02f);
            for (int i = 0; i < 10; i++) sound.Tick(1, 1, false, .02f);
            Assert.That(sound.Volume, Is.Zero);
            sound.Tick(1, 1, true, .02f); Assert.That(sound.Volume, Is.GreaterThan(0));
            sound.Reset(); Assert.That(sound.Volume, Is.Zero);
            sound.Tick(0, 1, true, .02f); Assert.That(sound.Volume, Is.Zero);
        }
    }
}
