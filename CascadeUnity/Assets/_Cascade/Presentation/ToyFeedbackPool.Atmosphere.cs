using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class ToyFeedbackPool
    {
        readonly AudioSource[] scoreSources = new AudioSource[2];
        AudioSource windSource, rainSource, cricketSource;
        AudioLowPassFilter windFilter;
        int scoreIncoming;
        float scoreFade = 1, scoreGain, rainBlend;
        bool audioSleeping, scoreSleeping;
        bool audioConfigurationChanged;
        readonly int[] scoreSamples=new int[2];
        int windSamples,rainSamples;
        float cricketWait=4.5f;
        int cricketPhrase;
        readonly WindAudioEnvelope windEnvelope = new WindAudioEnvelope();
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
            if (clip != null) {
                if(clip.loadState==AudioDataLoadState.Unloaded)clip.LoadAudioData();
                if(clip.loadState==AudioDataLoadState.Loaded)source.Play();
            }
            return source;
        }
        void InitializeAtmosphere()
        {
            scoreSources[0] = LoopSource("CASCADE score A", library?.menuMusic ?? library?.music);
            scoreSources[1] = LoopSource("CASCADE score B", null);
            windSource = LoopSource("Living island wind", library?.naturalWind ?? Resources.Load<AudioClip>("CascadeArt/Woodland/WindBed"));
            windFilter=windSource.gameObject.AddComponent<AudioLowPassFilter>();
            windFilter.cutoffFrequency=900;windFilter.lowpassResonanceQ=1;
            rainSource = LoopSource("Rain beneath the mill", Resources.Load<AudioClip>("CascadeArt/Woodland/RainBed"));
            cricketSource=LoopSource("Night observatory crickets",null);
            cricketSource.loop=false;cricketSource.priority=224;
            cricketSource.clip=Resources.Load<AudioClip>("CascadeArt/Audio/Atmosphere/NightCrickets");
            AudioSettings.OnAudioConfigurationChanged+=AudioConfigurationChanged;
        }
        void AudioConfigurationChanged(bool deviceChanged){audioConfigurationChanged=true;}
        void RememberLoopPositions()
        {
            for(int i=0;i<scoreSources.Length;i++)if(scoreSources[i]!=null&&scoreSources[i].isPlaying)scoreSamples[i]=scoreSources[i].timeSamples;
            if(windSource!=null&&windSource.isPlaying)windSamples=windSource.timeSamples;
            if(rainSource!=null&&rainSource.isPlaying)rainSamples=rainSource.timeSamples;
        }
        static bool ResumeLoop(AudioSource source,int position)
        {
            if(source==null||source.clip==null)return true;
            // Stopping a streaming source can unload its data. Merely waiting never reloads it.
            if(source.clip.loadState==AudioDataLoadState.Unloaded)source.clip.LoadAudioData();
            if(source.clip.loadState!=AudioDataLoadState.Loaded)return false;
            source.UnPause();
            // UnPause cannot restart a source stopped by an output-device reset.
            if(!source.isPlaying){source.Play();if(position>0)source.timeSamples=Mathf.Min(position,source.clip.samples-1);}
            return source.isPlaying;
        }
        void DisposeAtmosphere(){AudioSettings.OnAudioConfigurationChanged-=AudioConfigurationChanged;}
        void StopNightCrickets()
        {
            if(cricketSource!=null){if(cricketSource.isPlaying)cricketSource.Stop();cricketSource.volume=0;}
            // Returning outdoors begins with quiet, rather than resuming a cut-off insect phrase.
            cricketWait=4.5f;
        }
        void TickNightCrickets(float dt,bool allowed)
        {
            if(!allowed){StopNightCrickets();return;}
            if(cricketSource==null||cricketSource.clip==null)return;
            cricketSource.volume=Mathf.MoveTowards(cricketSource.volume,duck>0?.025f:.085f,dt*.4f);
            // One owned reusable source prevents overlapping phrases and never steals a puzzle cue voice.
            if(cricketSource.isPlaying)return;
            cricketWait=Mathf.Max(0,cricketWait-dt);
            if(cricketWait>0||duck>0)return;
            if(cricketSource.clip.loadState==AudioDataLoadState.Unloaded)cricketSource.clip.LoadAudioData();
            if(cricketSource.clip.loadState!=AudioDataLoadState.Loaded)return;
            cricketSource.pitch=.96f+(cricketPhrase%5)*.02f;
            cricketSource.panStereo=(cricketPhrase%3-1)*.14f;
            cricketSource.Play();
            cricketWait=9+(cricketPhrase++*5%7);
        }
        void OnApplicationPause(bool paused){if(paused)StopNightCrickets();}
        void OnApplicationFocus(bool focused){if(!focused)StopNightCrickets();}
        void TickAtmosphere(float seconds, bool paused)
        {
            float dt = Mathf.Clamp(seconds, 0, .1f);
            // Score fades are clock envelopes, not a physics simulation. Limiting
            // them to 0.1 s per frame makes music transitions stall at low FPS.
            float scoreDt=Mathf.Clamp(seconds,0,2);
            if(audioConfigurationChanged){audioConfigurationChanged=false;scoreSleeping=audioSleeping=true;scoreGain=0;StopNightCrickets();}
            else RememberLoopPositions();
            int zone = game.Machine.LivingWorld.Zone;
            var wanted = library?.ScoreFor(game.CurrentScreen, zone);
            // Streaming/background loading can take longer than the screen transition.
            // Keep the old theme until the new clip can play; never fade to an unloaded clip.
            if(wanted!=null&&wanted.loadState==AudioDataLoadState.Unloaded)wanted.LoadAudioData();
            bool wantedReady=wanted!=null&&wanted.loadState==AudioDataLoadState.Loaded;
            // Reversing an existing blend keeps both playback positions and the instantaneous gain.
            if(wantedReady&&wanted!=CurrentMusicClip&&wanted==scoreSources[1-scoreIncoming].clip) {
                scoreIncoming=1-scoreIncoming;scoreFade=1-scoreFade;
                if(!scoreSleeping)ResumeLoop(scoreSources[scoreIncoming],scoreSamples[scoreIncoming]);
            }
            // Queue a third theme until the current blend completes. Never cut an audible track halfway through.
            if (wantedReady && wanted != CurrentMusicClip && scoreFade >= 1) {
                scoreIncoming = 1 - scoreIncoming;
                var incoming = scoreSources[scoreIncoming];
                incoming.Stop(); incoming.volume = 0; incoming.clip = wanted; incoming.Play();
                scoreSamples[scoreIncoming]=0;
                if (scoreSleeping) incoming.Pause();
                scoreFade = 0;
            }
            duck = Mathf.Max(0, duck - dt);
            // An instructional film pauses puzzle commands, not the score's clock.
            float target = !MusicEnabled || paused ? 0 : duck > 0 || game.MechanicPopupOpen ? .16f : .24f;
            scoreGain = Mathf.MoveTowards(scoreGain, target, scoreDt * .7f);
            if (scoreSleeping && target > 0) {
                bool ready=ResumeLoop(scoreSources[scoreIncoming],scoreSamples[scoreIncoming]);
                if(scoreFade<1)ResumeLoop(scoreSources[1-scoreIncoming],scoreSamples[1-scoreIncoming]);
                scoreSleeping=!ready;
            }
            // Also covers the initial async menu load and a source stopped independently
            // of the output reset callback, without restarting a playing theme.
            if(!scoreSleeping&&target>0&&!scoreSources[scoreIncoming].isPlaying)
                ResumeLoop(scoreSources[scoreIncoming],scoreSamples[scoreIncoming]);
            if (!scoreSleeping&&scoreSources[scoreIncoming].isPlaying) scoreFade = Mathf.Min(1, scoreFade + scoreDt / 1.6f);
            float blend = Mathf.SmoothStep(0, 1, scoreFade);
            scoreSources[scoreIncoming].volume = scoreGain * Mathf.Sqrt(blend);
            scoreSources[1 - scoreIncoming].volume = scoreGain * Mathf.Sqrt(1 - blend);
            if(scoreFade==1&&scoreSources[1-scoreIncoming].isPlaying)scoreSources[1-scoreIncoming].Pause();
            if (!scoreSleeping && scoreGain == 0) { foreach (var source in scoreSources) source.Pause(); scoreSleeping = true; }

            // Both beds keep their place. Region changes blend them rather than restarting the wind.
            bool soundsAllowed = SoundEnabled && !paused && !game.MechanicPopupOpen;
            if (audioSleeping && soundsAllowed) {audioSleeping=!(ResumeLoop(windSource,windSamples)&ResumeLoop(rainSource,rainSamples));}
            rainBlend = Mathf.MoveTowards(rainBlend, zone == 1 || zone == 4 ? 1 : 0, dt * .45f);
            bool atRig = game.CurrentScreen == GameScreen.Playing || game.CurrentScreen == GameScreen.Win
                || game.CurrentScreen == GameScreen.Fail || game.CurrentScreen == GameScreen.Checkpoint;
            int floor = atRig ? game.VisibleFrame?.ActiveFloor ?? 0 : 0;
            float pressure = game.Machine.WindAt(floor);
            bool indoors = !atRig && game.CurrentScreen != GameScreen.JourneyMap
                && game.CurrentScreen != GameScreen.Dialogue && game.CurrentScreen != GameScreen.JourneyEnd;
            TickNightCrickets(dt,soundsAllowed&&zone==8&&!indoors);
            // The same pressure drives the visible rig and this loop; no independent audio gust clock.
            windSource.volume = windEnvelope.Tick(pressure, indoors ? .35f : 1 - rainBlend * .55f, soundsAllowed, dt);
            windSource.pitch = Mathf.Lerp(.84f,1.07f,Mathf.Clamp01(Mathf.Abs(pressure)));
            windFilter.cutoffFrequency=Mathf.Lerp(850,4600,Mathf.Pow(Mathf.Clamp01(Mathf.Abs(pressure)),1.4f));
            float windStrength=Mathf.Abs(pressure);
            // A quiet button tick can occupy the first threshold-crossing frame.
            // Retry while the breeze persists, then let the five-second haptic cooldown rest.
            if(game.CurrentScreen==GameScreen.Playing&&!paused&&!game.Interaction.IsPresenting
                &&windStrength>=.24f&&Time.realtimeSinceStartup-lastBreezeHaptic>5f)
                Haptic(ToyHapticMoment.Breeze);
            rainSource.volume = Mathf.MoveTowards(rainSource.volume, soundsAllowed ? rainBlend * .08f : 0, dt * (soundsAllowed?.16f:.7f));
            if (!audioSleeping && windSource.volume == 0 && rainSource.volume == 0) {
                windSource.Pause(); rainSource.Pause(); audioSleeping = true;
            }
        }
        void OnDisable()
        {
            if(hapticPattern!=null){StopCoroutine(hapticPattern);hapticPattern=null;}
            RememberLoopPositions();
            // This component owns these loops; stopping Update must not leave its audio audible.
            // Pause rather than Stop so re-enabling keeps every theme/ambience playback position.
            if (voices != null) foreach (var source in voices) if (source != null) source.Stop();
            duck = 0;
            scoreGain = 0; scoreSleeping = true;
            foreach (var source in scoreSources) if (source != null) { source.volume = 0; source.Pause(); }
            windEnvelope.Reset();
            if (windSource != null) { windSource.volume = 0; windSource.Pause(); }
            if (rainSource != null) { rainSource.volume = 0; rainSource.Pause(); }
            StopNightCrickets();
            audioSleeping = true;
        }
    }
}
