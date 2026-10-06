using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class ToyFeedbackPool
    {
        readonly AudioSource[] scoreSources = new AudioSource[2];
        AudioSource windSource, rainSource;
        int scoreIncoming;
        float scoreFade = 1, scoreGain, rainBlend;
        bool audioSleeping, scoreSleeping;
        public AudioClip CurrentMusicClip => scoreSources[scoreIncoming]?.clip;
        public float MusicPosition => scoreSources[scoreIncoming] == null ? 0 : scoreSources[scoreIncoming].time;
        public float MusicCrossfade => scoreFade;
        public bool MusicSleeping => scoreSleeping;
        public float WindVolume => windSource == null ? 0 : windSource.volume;

        AudioSource LoopSource(string name, AudioClip clip)
        {
            var sourceObject = new GameObject(name, typeof(AudioSource));
            sourceObject.transform.SetParent(transform, false);
            var source = sourceObject.GetComponent<AudioSource>();
            source.playOnAwake = false; source.loop = true; source.spatialBlend = 0; source.volume = 0;
            source.priority = 192; source.clip = clip;
            if (clip != null) source.Play();
            return source;
        }
        void InitializeAtmosphere()
        {
            scoreSources[0] = LoopSource("CASCADE score A", library?.menuMusic ?? library?.music);
            scoreSources[1] = LoopSource("CASCADE score B", null);
            windSource = LoopSource("Living island wind", library?.naturalWind ?? Resources.Load<AudioClip>("CascadeArt/Woodland/WindBed"));
            rainSource = LoopSource("Rain beneath the mill", Resources.Load<AudioClip>("CascadeArt/Woodland/RainBed"));
        }
        void TickAtmosphere(float seconds, bool paused)
        {
            float dt = Mathf.Clamp(seconds, 0, .1f);
            int zone = game.Machine.LivingWorld.Zone;
            var wanted = library?.ScoreFor(game.CurrentScreen, zone);
            // Reversing an existing blend keeps both playback positions and the instantaneous gain.
            if(wanted!=null&&wanted!=CurrentMusicClip&&wanted==scoreSources[1-scoreIncoming].clip) {
                scoreIncoming=1-scoreIncoming;scoreFade=1-scoreFade;
                if(!scoreSleeping)scoreSources[scoreIncoming].UnPause();
            }
            // Queue a third theme until the current blend completes. Never cut an audible track halfway through.
            if (wanted != null && wanted != CurrentMusicClip && scoreFade >= 1) {
                scoreIncoming = 1 - scoreIncoming;
                var incoming = scoreSources[scoreIncoming];
                incoming.Stop(); incoming.volume = 0; incoming.clip = wanted; incoming.Play();
                if (scoreSleeping) incoming.Pause();
                scoreFade = 0;
            }
            duck = Mathf.Max(0, duck - dt);
            float target = !MusicEnabled || paused ? 0 : duck > 0 ? .13f : .19f;
            scoreGain = Mathf.MoveTowards(scoreGain, target, dt * .7f);
            if (scoreSleeping && target > 0) { foreach (var source in scoreSources) source.UnPause(); scoreSleeping = false; }
            if (!scoreSleeping) scoreFade = Mathf.Min(1, scoreFade + dt / 1.6f);
            float blend = Mathf.SmoothStep(0, 1, scoreFade);
            scoreSources[scoreIncoming].volume = scoreGain * Mathf.Sqrt(blend);
            scoreSources[1 - scoreIncoming].volume = scoreGain * Mathf.Sqrt(1 - blend);
            if(scoreFade==1&&scoreSources[1-scoreIncoming].isPlaying)scoreSources[1-scoreIncoming].Pause();
            if (!scoreSleeping && scoreGain == 0) { foreach (var source in scoreSources) source.Pause(); scoreSleeping = true; }

            // Both beds keep their place. Region changes blend them rather than restarting the wind.
            bool soundsAllowed = SoundEnabled && !paused;
            if (audioSleeping && soundsAllowed) { windSource.UnPause(); rainSource.UnPause(); audioSleeping = false; }
            rainBlend = Mathf.MoveTowards(rainBlend, zone == 1 || zone == 4 ? 1 : 0, dt * .45f);
            float phase = game.Machine.AmbientPhase;
            float breath = .5f + .5f * Mathf.Sin(phase * .21f + Mathf.Sin(phase * .073f) * 2.1f);
            float swell = Mathf.SmoothStep(0, 1, breath);
            float pressure = Mathf.Abs(game.Machine.WindAt(0));
            float windTarget = soundsAllowed ? Mathf.Lerp(.018f, .080f, swell) + pressure * .07f : 0;
            bool indoors = game.CurrentScreen == GameScreen.Home || game.CurrentScreen == GameScreen.Settings || game.CurrentScreen == GameScreen.Shop;
            windTarget *= indoors ? .35f : 1 - rainBlend * .55f;
            windSource.volume = Mathf.MoveTowards(windSource.volume, windTarget, dt * (soundsAllowed?.12f:.9f));
            rainSource.volume = Mathf.MoveTowards(rainSource.volume, soundsAllowed ? rainBlend * .08f : 0, dt * (soundsAllowed?.16f:.7f));
            if (!audioSleeping && windSource.volume == 0 && rainSource.volume == 0) {
                windSource.Pause(); rainSource.Pause(); audioSleeping = true;
            }
        }
    }
}
