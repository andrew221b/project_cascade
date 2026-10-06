using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    public sealed class ToyFeedbackPool : MonoBehaviour
    {
        sealed class Dust
        {
            public RectTransform source;public Vector3 sourceOffset;
            public Transform transform; public MeshRenderer renderer; public MaterialPropertyBlock properties;
            public Vector3 origin, velocity; public Color color; public float age, life, size,spin; public bool reduced,wood,seed;
        }
        Dust[] particles;
        AudioSource[] voices;
        AudioSource music;
        AudioSource ambience;
        AudioClip windBed,rainBed;
        ToyAudioLibrary library;
        Material dustMaterial,chipMaterial;
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
            dustMaterial=art.shadow;
            chipMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Carved wood splinters"};
            chipMaterial.SetTexture("_BaseMap",Resources.Load<Texture2D>("CascadeArt/Reactions/TimberStates"));
            chipMaterial.SetFloat("_Smoothness",.24f);chipMaterial.SetFloat("_Cull",(float)CullMode.Off);
            chipMaterial.SetFloat("_Surface",1);chipMaterial.SetFloat("_ZWrite",0);chipMaterial.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);chipMaterial.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);chipMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");chipMaterial.renderQueue=3000;
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
        public void Burst(Vector3 origin, Color color, int count, bool reduced,RectTransform source=null)
        {
            if (particles == null || particles.Length == 0 || Quality<=0) return;
            count = Mathf.Min(count, Quality==1?4:particles.Length); if (reduced) count = Mathf.Min(2, count);
            for (int i = 0; i < count; i++) {
                var p = particles[nextParticle++ % particles.Length]; float angle = i * 2.399963f;
                p.source=source;p.sourceOffset=source==null?Vector3.zero:origin-game.UIFeedbackPoint(source);
                p.origin = origin; p.origin.z = -.7f; p.color = color; p.age = 0; p.life = .38f + (i % 3) * .10f; p.size = .08f + (i % 4) * .02f; p.reduced = reduced;
                p.wood=p.seed=false;p.spin=0;p.renderer.sharedMaterial=dustMaterial;p.transform.rotation=Quaternion.identity;
                p.velocity = new Vector3(Mathf.Cos(angle) * .45f, Mathf.Sin(angle) * .28f + .12f, 0);
                p.transform.position = p.origin; p.transform.localScale = Vector3.one * p.size; p.transform.gameObject.SetActive(true);
            }
        }
        public void WoodChips(Vector3 origin,int count,bool reduced,float span=1.4f)
        {
            if(particles==null||particles.Length==0||Quality<=0)return;
            count=Mathf.Min(count,Quality==1?4:particles.Length);if(reduced)count=Mathf.Min(2,count);
            for(int i=0;i<count;i++){
                var p=particles[nextParticle++%particles.Length];p.source=null;p.origin=origin+Vector3.right*((i/(float)Mathf.Max(1,count-1)-.5f)*span);p.origin.z=-.7f;
                p.renderer.sharedMaterial=chipMaterial;p.color=i%2==0?new Color(.84f,.71f,.52f):Color.white;p.age=0;p.life=reduced?.60f:2.8f+(i%3)*.2f;p.size=i<6?span/6*.92f:.24f+(i%4)*.06f;
                p.reduced=reduced;p.wood=true;p.seed=false;p.spin=(i%2==0?1:-1)*(150+i*19);
                p.velocity=new Vector3(Mathf.Sin(i*2.399f)*.55f+game.Machine.WindAt(game.VisibleFrame.ActiveFloor)*.25f,.18f+(i%3)*.18f,0);
                p.properties.SetVector("_BaseMap_ST",WorkshopSpriteSet.ChipUv(i));p.properties.SetColor("_BaseColor",p.color);p.renderer.SetPropertyBlock(p.properties);
                p.transform.position=p.origin;p.transform.localScale=new Vector3(p.size,p.size*.40f,1);p.transform.rotation=Quaternion.Euler(0,0,i*31);p.transform.gameObject.SetActive(true);
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
        public void LanternSeeds(Vector3 origin,bool reduced,RectTransform source=null)
        {
            if(Quality<=0||particles==null||particles.Length==0)return;
            int count=reduced?2:Quality==1?6:18;
            for(int i=0;i<count;i++){
                var p=particles[nextParticle++%particles.Length];float angle=i*2.399963f;
                p.source=source;p.origin=origin+Vector3.right*Mathf.Sin(angle)*.22f;p.origin.z=-.7f;p.sourceOffset=source==null?Vector3.zero:p.origin-game.UIFeedbackPoint(source);
                p.age=0;p.life=1.10f+(i%3)*.12f;p.size=.045f+(i%3)*.018f;p.reduced=reduced;p.wood=false;p.seed=true;p.spin=angle;
                p.color=i%3==0?new Color(.51f,.89f,.77f):i%3==1?new Color(1,.78f,.31f):new Color(.68f,.74f,1);
                p.velocity=new Vector3(Mathf.Cos(angle)*.65f,1.55f+(i%4)*.16f,0);p.renderer.sharedMaterial=dustMaterial;
                p.transform.position=p.origin;p.transform.localScale=Vector3.one*p.size;p.transform.rotation=Quaternion.identity;p.transform.gameObject.SetActive(true);
            }
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
                if(p.source!=null)p.origin=game.UIFeedbackPoint(p.source)+p.sourceOffset;
                p.age += Time.unscaledDeltaTime; float t = p.age / p.life;
                if (t >= 1) { p.transform.gameObject.SetActive(false); continue; }
                ActiveParticleCount++;
                p.transform.position = p.reduced ? p.origin : p.origin + p.velocity * p.age + (p.seed?Vector3.right*Mathf.Sin(p.age*5+p.spin)*p.age*.12f:Vector3.down*((p.wood?3.8f:.38f)*p.age*p.age));
                p.transform.localScale=p.wood?new Vector3(p.size,p.size*.40f,1):Vector3.one*(p.size*(1+t));
                if(p.wood&&!p.reduced)p.transform.rotation=Quaternion.Euler(0,0,p.spin*p.age);
                var color = p.color; color.a *= p.wood?1-Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.62f)/.38f)):(1-t)*(1-t);
                p.properties.SetColor("_BaseColor", color); p.renderer.SetPropertyBlock(p.properties);
            }
        }
        public void ClearTails()
        {
            if (particles != null) foreach (var p in particles) p.transform.gameObject.SetActive(false);
            if (voices != null) foreach (var voice in voices) voice.Stop(); ActiveParticleCount = 0;
        }
        public void RefreshAnchors()
        {
            if(particles==null||game==null)return;
            foreach(var p in particles)if(p.transform.gameObject.activeSelf&&p.source!=null){
                var origin=game.UIFeedbackPoint(p.source)+p.sourceOffset;origin.z=-.7f;
                p.transform.position+=origin-p.origin;p.origin=origin;
            }
        }
        void OnDestroy(){if(chipMaterial!=null)Destroy(chipMaterial);}
    }
}
