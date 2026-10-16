using Cascade.Presentation;
using Cascade.Application;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class WindAudioEnvelopeTests
    {
        [Test] public void WeatherHasRealCalmsVariedSwellsAndContinuousEdges()
        {
            int calm=0,run=0,longest=0;float low=1,high=0;bool left=false,right=false;
            for(int cycle=0;cycle<20;cycle++){
                float peak=0;
                for(int i=0;i<560;i++){
                    float t=cycle*28+i*.05f,p=WorkshopWind.Sample(t);
                    Assert.That(p,Is.InRange(-1,1));
                    Assert.That(System.Math.Abs(p-WorkshopWind.Sample(t+.016f)),Is.LessThan(.025f));
                    if(p==0){calm++;run++;longest=System.Math.Max(longest,run);}else run=0;
                    peak=System.Math.Max(peak,System.Math.Abs(p));left|=p<-.2f;right|=p>.2f;
                }
                low=System.Math.Min(low,peak);high=System.Math.Max(high,peak);
            }
            Assert.That(calm/11200f,Is.InRange(.35f,.8f));Assert.That(longest*.05f,Is.GreaterThan(4));
            Assert.That(high-low,Is.GreaterThan(.12f));Assert.That(left&&right,Is.True);
        }
        [Test] public void SharedWeatherAlsoLeavesAudibleSilenceBetweenGusts()
        {
            var envelope=new WindAudioEnvelope();int silence=0,audible=0;
            for(int i=0;i<12000;i++){
                float volume=envelope.Tick(WorkshopWind.Sample(i*.02f),1,true,.02f);
                if(volume==0)silence++;if(volume>.02f)audible++;
            }
            Assert.That(silence,Is.GreaterThan(3500));Assert.That(audible,Is.GreaterThan(400));
        }
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
