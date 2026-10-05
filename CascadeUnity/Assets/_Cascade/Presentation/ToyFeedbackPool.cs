using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    public sealed class ToyFeedbackPool : MonoBehaviour
    {
        sealed class Dust
        {
            public Transform transform; public MeshRenderer renderer; public MaterialPropertyBlock properties;
            public Vector3 origin, velocity; public Color color; public float age, life, size,spin; public bool reduced,wood;
        }
        Dust[] particles;
        AudioSource[] voices;
        AudioSource music;
        AudioSource ambience;
        AudioClip windBed,rainBed;
        ToyAudioLibrary library;
        int nextParticle, nextVoice, variant;
        float duck;
        CascadeGame game;
        public bool SoundEnabled { get; set; } = true;
        public bool MusicEnabled { get; set; } = true;
        public bool HapticsEnabled { get; set; } = true;
        public int Quality { get; set; } = 2;
        public int HapticRequests { get; private set; }
        public int ActiveParticleCount { get; private set; }
        public void Initialize(CascadeGame owner, ToyArt art, MotionProfile profile)
        {
            game = owner; library = Resources.Load<ToyAudioLibrary>("CascadeArt/ToyAudio");
            particles = new Dust[Mathf.Clamp(profile.dustCapacity, 0, 64)];
            for (int i = 0; i < particles.Length; i++) {
                var go = new GameObject("Pooled workshop dust " + i, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(transform, false);
                go.GetComponent<MeshFilter>().sharedMesh = art.quad; var renderer = go.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = art.shadow; renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
                particles[i] = new Dust { transform = go.transform, renderer = renderer, properties = new MaterialPropertyBlock() }; go.SetActive(false);
            }
            voices = new AudioSource[Mathf.Clamp(profile.voiceCapacity, 1, 16)];
            for (int i = 0; i < voices.Length; i++) { var go = new GameObject("Pooled toy voice " + i, typeof(AudioSource)); go.transform.SetParent(transform, false); voices[i] = go.GetComponent<AudioSource>(); voices[i].playOnAwake = false; voices[i].spatialBlend = 0; }
            var musicObject = new GameObject("Cloud workshop music", typeof(AudioSource)); musicObject.transform.SetParent(transform, false);
            music = musicObject.GetComponent<AudioSource>(); music.playOnAwake = false; music.loop = true; music.spatialBlend = 0; music.volume = .12f;
            if (library != null) { music.clip = library.music; music.Play(); }
            var ambientObject=new GameObject("Quiet region ambience",typeof(AudioSource));ambientObject.transform.SetParent(transform,false);
            ambience=ambientObject.GetComponent<AudioSource>();ambience.playOnAwake=false;ambience.loop=true;ambience.spatialBlend=0;ambience.volume=.08f;
            windBed=Resources.Load<AudioClip>("CascadeArt/Woodland/WindBed");rainBed=Resources.Load<AudioClip>("CascadeArt/Woodland/RainBed");
        }
        public void Sound(ToyCue cue)
        {
            if (!SoundEnabled || library == null || library.cues == null || (int)cue >= library.cues.Length) return;
            var clip = library.cues[(int)cue];
            if ((variant++ & 1) != 0 && library.alternates != null && (int)cue < library.alternates.Length && library.alternates[(int)cue] != null) clip = library.alternates[(int)cue];
            if (clip == null) return;
            var voice = voices[nextVoice++ % voices.Length]; voice.Stop(); voice.clip = clip; voice.volume = cue == ToyCue.Bomb ? .48f : .65f; voice.pitch = 1; voice.Play(); duck = .22f;
        }
        public void Burst(Vector3 origin, Color color, int count, bool reduced)
        {
            if (particles == null || particles.Length == 0 || Quality<=0) return;
            count = Mathf.Min(count, Quality==1?4:particles.Length); if (reduced) count = Mathf.Min(2, count);
            for (int i = 0; i < count; i++) {
                var p = particles[nextParticle++ % particles.Length]; float angle = i * 2.399963f;
                p.origin = origin; p.origin.z = -.7f; p.color = color; p.age = 0; p.life = .38f + (i % 3) * .10f; p.size = .08f + (i % 4) * .02f; p.reduced = reduced;
                p.wood=false;p.spin=0;p.transform.rotation=Quaternion.identity;
                p.velocity = new Vector3(Mathf.Cos(angle) * .45f, Mathf.Sin(angle) * .28f + .12f, 0);
                p.transform.position = p.origin; p.transform.localScale = Vector3.one * p.size; p.transform.gameObject.SetActive(true);
            }
        }
        public void WoodChips(Vector3 origin,int count,bool reduced)
        {
            if(particles==null||particles.Length==0||Quality<=0)return;
            count=Mathf.Min(count,Quality==1?4:particles.Length);if(reduced)count=Mathf.Min(2,count);
            for(int i=0;i<count;i++){
                var p=particles[nextParticle++%particles.Length];p.origin=origin+Vector3.right*((i-(count-1)*.5f)*.16f);p.origin.z=-.7f;
                p.color=i%2==0?new Color(.64f,.32f,.12f):new Color(.89f,.60f,.28f);p.age=0;p.life=.5f+(i%3)*.08f;p.size=.10f+(i%4)*.02f;
                p.reduced=reduced;p.wood=true;p.spin=(i%2==0?1:-1)*(150+i*19);
                p.velocity=new Vector3((i-(count-1)*.5f)*.14f,.5f+(i%3)*.11f,0);
                p.transform.position=p.origin;p.transform.localScale=new Vector3(p.size,p.size*.22f,1);p.transform.rotation=Quaternion.Euler(0,0,i*31);p.transform.gameObject.SetActive(true);
            }
        }
        public void Haptic()
        {
            if (!HapticsEnabled) return; HapticRequests++;
            // The OS owns the vibration. This feedback path cannot call gameplay.
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
        void Update()
        {
            if (game == null || game.Interaction == null) return;
            bool paused = !game.enabled || game.Interaction.IsPaused || game.Interaction.IsSuspended;
            bool worldPaused=!game.enabled||game.Interaction.IsSuspended||game.CurrentScreen==GameScreen.Pause||game.CurrentScreen==GameScreen.SaveRecovery||(game.CurrentScreen==GameScreen.Playing&&game.Interaction.IsPaused);
            if(ambience!=null){
                int zone=game.Machine.LivingWorld.Zone;var bed=zone==1||zone==4?rainBed:windBed;
                if(ambience.clip!=bed){ambience.Stop();ambience.clip=bed;if(bed!=null)ambience.Play();}
                ambience.mute=!SoundEnabled||worldPaused;
            }
            if (music != null) {
                music.mute = !MusicEnabled || worldPaused; duck = Mathf.Max(0, duck - Time.unscaledDeltaTime);
                music.volume = Mathf.MoveTowards(music.volume, duck > 0 ? .075f : .12f, Time.unscaledDeltaTime * .4f);
            }
            if (paused) { foreach (var voice in voices) if (voice.isPlaying) voice.Stop(); return; }
            ActiveParticleCount = 0;
            foreach (var p in particles) {
                if (!p.transform.gameObject.activeSelf) continue;
                p.age += Time.unscaledDeltaTime; float t = p.age / p.life;
                if (t >= 1) { p.transform.gameObject.SetActive(false); continue; }
                ActiveParticleCount++;
                p.transform.position = p.reduced ? p.origin : p.origin + p.velocity * p.age + Vector3.down * ((p.wood?2.8f:.38f) * p.age * p.age);
                p.transform.localScale=p.wood?new Vector3(p.size,p.size*.22f,1):Vector3.one*(p.size*(1+t));
                if(p.wood&&!p.reduced)p.transform.rotation=Quaternion.Euler(0,0,p.spin*p.age);
                var color = p.color; color.a *= (1-t)*(1-t);
                p.properties.SetColor("_BaseColor", color); p.renderer.SetPropertyBlock(p.properties);
            }
        }
        public void ClearTails()
        {
            if (particles != null) foreach (var p in particles) p.transform.gameObject.SetActive(false);
            if (voices != null) foreach (var voice in voices) voice.Stop(); ActiveParticleCount = 0;
        }
    }
}
