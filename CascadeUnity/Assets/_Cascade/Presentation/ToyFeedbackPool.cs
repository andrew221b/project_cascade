using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Cascade.Presentation
{
    public enum ToyHapticMoment { Swap, Landing, Breeze, Gust, WoodCrack, WoodBreak, Bomb, Star, Parcel }

    public sealed partial class ToyFeedbackPool : MonoBehaviour
    {
        sealed class Dust
        {
            public RectTransform source;public Vector3 sourceOffset;
            public Transform transform; public MeshRenderer renderer; public MaterialPropertyBlock properties;
            public Rigidbody body;public BoxCollider collider;
            public Vector3 origin, velocity; public Color color; public float age, life, size,spin; public bool reduced,wood,seed;
        }
        Dust[] particles;
        AudioSource[] voices;
        ToyAudioLibrary library;
        AudioClip timberSnap;
        Material dustMaterial,chipMaterial;
        int nextParticle, nextVoice, variant;
        float duck;
        float lastSelectSound=-1,lastHaptic=-1;
        float lastBreezeHaptic=-10;
        ToyHapticMoment lastEventHaptic;
        float lastEventHapticTime=-1;
        Coroutine hapticPattern;
        CascadeGame game;
        Scene debrisScene;PhysicsScene debrisPhysics;Transform debrisRoot;
        readonly BoxCollider[] supports=new BoxCollider[12];
        float physicsRemainder;
        public ToyRowPhysics RowPhysics {get;private set;}
        public bool SoundEnabled { get; set; } = true;
        public bool MusicEnabled { get; set; } = true;
        public bool HapticsEnabled { get; set; } = true;
        public int Quality { get; set; } = 2;
        public int HapticRequests { get; private set; }
        public int ActiveParticleCount { get; private set; }
        public void Initialize(CascadeGame owner, ToyArt art, MotionProfile profile)
        {
            game = owner; library = Resources.Load<ToyAudioLibrary>("CascadeArt/ToyAudio");
            timberSnap=Resources.Load<AudioClip>("CascadeArt/Audio/Atmosphere/TimberSnap");
            debrisScene=SceneManager.CreateScene("CASCADE cosmetic debris "+GetInstanceID(),new CreateSceneParameters(LocalPhysicsMode.Physics3D));debrisPhysics=debrisScene.GetPhysicsScene();
            var root=new GameObject("Isolated cosmetic debris");SceneManager.MoveGameObjectToScene(root,debrisScene);debrisRoot=root.transform;
            RowPhysics=new ToyRowPhysics(game,debrisScene,debrisRoot);
            for(int i=0;i<supports.Length;i++){
                var rail=new GameObject("Cosmetic plank contact "+i,typeof(BoxCollider),typeof(Rigidbody));SceneManager.MoveGameObjectToScene(rail,debrisScene);rail.transform.SetParent(debrisRoot,false);
                var body=rail.GetComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;supports[i]=rail.GetComponent<BoxCollider>();supports[i].enabled=false;
            }
            dustMaterial=art.shadow;
            chipMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Carved wood splinters"};
            chipMaterial.SetTexture("_BaseMap",Resources.Load<Texture2D>("CascadeArt/Reactions/TimberStates"));
            chipMaterial.SetFloat("_Smoothness",.24f);chipMaterial.SetFloat("_Cull",(float)CullMode.Off);
            chipMaterial.SetFloat("_AlphaClip",1);chipMaterial.SetFloat("_Cutoff",.12f);chipMaterial.EnableKeyword("_ALPHATEST_ON");
            chipMaterial.SetFloat("_Surface",1);chipMaterial.SetFloat("_ZWrite",0);chipMaterial.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);chipMaterial.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);chipMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");chipMaterial.renderQueue=3000;
            particles = new Dust[Mathf.Clamp(profile.dustCapacity, 0, 64)];
            for (int i = 0; i < particles.Length; i++) {
                var go = new GameObject("Pooled workshop dust " + i, typeof(MeshFilter), typeof(MeshRenderer),typeof(Rigidbody),typeof(BoxCollider));SceneManager.MoveGameObjectToScene(go,debrisScene);go.transform.SetParent(debrisRoot,false);
                go.GetComponent<MeshFilter>().sharedMesh = art.quad; var renderer = go.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = art.shadow; renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
                var body=go.GetComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;body.constraints=RigidbodyConstraints.FreezePositionZ;body.interpolation=RigidbodyInterpolation.None;
                var collider=go.GetComponent<BoxCollider>();collider.size=new Vector3(.65f,.8f,.12f);collider.enabled=false;
                particles[i] = new Dust { transform = go.transform, renderer = renderer, properties = new MaterialPropertyBlock(),body=body,collider=collider }; go.SetActive(false);
            }
            voices = new AudioSource[Mathf.Clamp(profile.voiceCapacity, 1, 16)];
            for (int i = 0; i < voices.Length; i++) { var go = new GameObject("Pooled toy voice " + i, typeof(AudioSource)); go.transform.SetParent(transform, false); voices[i] = go.GetComponent<AudioSource>(); voices[i].playOnAwake = false; voices[i].spatialBlend = 0; }
            InitializeAtmosphere();
        }
        public void Sound(ToyCue cue)
        {
            if (!SoundEnabled || game==null || !game.enabled || game.Interaction?.IsSuspended==true || library == null || library.cues == null || (int)cue >= library.cues.Length) return;
            if(cue==ToyCue.Select){float now=Time.realtimeSinceStartup;if(now-lastSelectSound<.075f)return;lastSelectSound=now;}
            var clip = library.cues[(int)cue];
            if ((variant++ & 1) != 0 && library.alternates != null && (int)cue < library.alternates.Length && library.alternates[(int)cue] != null) clip = library.alternates[(int)cue];
            if (clip == null) return;
            var voice = voices[nextVoice++ % voices.Length]; voice.Stop(); voice.clip = clip; voice.volume = cue == ToyCue.Bomb ? .48f : .65f;
            voice.pitch = cue <= ToyCue.Fragile ? ((variant & 1) == 0 ? .985f : 1.015f) : 1;
            voice.Play(); duck = .22f;
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
                p.body.isKinematic=true;p.collider.enabled=false;p.renderer.shadowCastingMode=ShadowCastingMode.Off;p.renderer.receiveShadows=false;
                p.velocity = new Vector3(Mathf.Cos(angle) * .45f, Mathf.Sin(angle) * .28f + .12f, 0);
                p.transform.position = p.origin; p.transform.localScale = Vector3.one * p.size; p.transform.gameObject.SetActive(true);
            }
        }
        public void WoodSnap()
        {
            if(!SoundEnabled||timberSnap==null||voices==null||game?.Interaction?.IsSuspended==true)return;
            var voice=voices[nextVoice++%voices.Length];voice.Stop();voice.clip=timberSnap;voice.volume=.58f;voice.pitch=1;voice.Play();duck=.30f;
        }
        public void StarChime(int index)
        {
            if(!SoundEnabled||library?.cues==null||voices==null||game?.Interaction?.IsSuspended==true)return;
            var clip=library.cues[(int)ToyCue.Repair];if(clip==null)return;
            var voice=voices[nextVoice++%voices.Length];voice.Stop();voice.clip=clip;
            voice.pitch=Mathf.Clamp(.87f+index*.085f,.87f,1.25f);voice.volume=.32f;voice.Play();duck=.12f;
        }
        public void WoodChips(Vector3 origin,int count,bool reduced,float span=1.4f)
        {
            if(particles==null||particles.Length==0||Quality<=0)return;
            count=Mathf.Min(count,Quality==1?4:particles.Length);if(reduced)count=Mathf.Min(2,count);
            for(int i=0;i<count;i++){
                var p=particles[nextParticle++%particles.Length];p.source=null;p.origin=origin+Vector3.right*((i/(float)Mathf.Max(1,count-1)-.5f)*span);p.origin.z=-.7f;
                p.renderer.sharedMaterial=chipMaterial;p.color=i%2==0?new Color(.84f,.71f,.52f):Color.white;p.age=0;p.life=reduced?.60f:4.3f+(i%3)*.2f;p.size=i<6?span/6*.92f:.24f+(i%4)*.06f;
                p.reduced=reduced;p.wood=true;p.seed=false;p.spin=(i%2==0?1:-1)*(150+i*19);
                p.velocity=new Vector3(Mathf.Sin(i*2.399f)*.55f+game.Machine.WindAt(game.VisibleFrame.ActiveFloor)*.25f,.18f+(i%3)*.18f,0);
                p.properties.SetVector("_BaseMap_ST",WorkshopSpriteSet.ChipUv(i));p.properties.SetColor("_BaseColor",p.color);p.renderer.SetPropertyBlock(p.properties);
                p.transform.position=p.origin;p.transform.localScale=new Vector3(p.size,p.size*.40f,1);p.transform.rotation=Quaternion.Euler(0,0,i*31);p.transform.gameObject.SetActive(true);
                p.body.isKinematic=reduced;p.body.useGravity=!reduced;p.collider.enabled=!reduced;
                p.body.mass=i<6?.075f:.018f+(i%4)*.006f;p.body.linearDamping=.18f;p.body.angularDamping=.32f;
                p.renderer.shadowCastingMode=ShadowCastingMode.On;p.renderer.receiveShadows=true;
                if(!reduced){p.body.position=p.origin;p.body.rotation=p.transform.rotation;p.body.linearVelocity=p.velocity;p.body.angularVelocity=new Vector3(.35f*(i%3-1),.22f*(i%2==0?1:-1),p.spin*Mathf.Deg2Rad);p.body.WakeUp();}
            }
        }
        public void Haptic(float strength=.35f)
        {
            if (!HapticsEnabled || float.IsNaN(strength) || strength<=0 || game==null || !game.enabled || game.Interaction?.IsSuspended==true) return;
            float now=Time.realtimeSinceStartup;if(now-lastHaptic<.12f)return;lastHaptic=now;HapticRequests++;
            NativeHaptics.Pulse(Mathf.Clamp01(strength));
        }
        public void Haptic(ToyHapticMoment moment)
        {
            if(!HapticsEnabled||game==null||!game.enabled||game.Interaction?.IsSuspended==true)return;
            float now=Time.realtimeSinceStartup;
            // Debounce duplicate presentation callbacks, while letting a crack or
            // impact override the tiny tick of the button that caused it.
            if(moment==lastEventHaptic&&now-lastEventHapticTime<.10f)return;
            lastEventHaptic=moment;lastEventHapticTime=now;
            if(moment==ToyHapticMoment.Breeze){
                if(hapticPattern!=null||now-lastBreezeHaptic<5||now-lastHaptic<.12f)return;
                lastBreezeHaptic=now;Haptic(.09f);return;
            }
            if(moment==ToyHapticMoment.Gust)lastBreezeHaptic=now;
            if(hapticPattern!=null){StopCoroutine(hapticPattern);hapticPattern=null;}
            float[] strengths;
            float[] gaps;
            switch(moment){
                case ToyHapticMoment.Swap: strengths=new[]{.09f};gaps=null;break;
                case ToyHapticMoment.Landing: strengths=new[]{.27f};gaps=null;break;
                case ToyHapticMoment.Gust: strengths=new[]{.19f,.48f,.27f};gaps=new[]{.13f,.19f};break;
                case ToyHapticMoment.WoodCrack: strengths=new[]{.24f};gaps=null;break;
                case ToyHapticMoment.WoodBreak: strengths=new[]{.19f,.78f,.32f};gaps=new[]{.40f,.24f};break;
                case ToyHapticMoment.Bomb: strengths=new[]{.70f,.36f};gaps=new[]{.16f};break;
                case ToyHapticMoment.Star: strengths=new[]{.12f};gaps=null;break;
                case ToyHapticMoment.Parcel: strengths=new[]{.20f,.48f};gaps=new[]{.19f};break;
                default: return;
            }
            // The first pulse is emitted on the same frame as the visible event.
            if(strengths.Length==1){lastHaptic=now;HapticRequests++;NativeHaptics.Pulse(strengths[0]);return;}
            hapticPattern=StartCoroutine(PlayHapticPattern(strengths,gaps));
        }
        IEnumerator PlayHapticPattern(float[] strengths,float[] gaps)
        {
            for(int i=0;i<strengths.Length;i++){
                if(i>0){
                    float waited=0;
                    while(waited<gaps[i-1]){
                        if(!HapticsEnabled||game==null||!game.enabled){hapticPattern=null;yield break;}
                        if(game.IntroductionAnimationAllowed&&game.Interaction?.IsSuspended!=true&&game.Interaction?.IsPaused!=true)
                            waited+=Mathf.Min(.05f,Time.unscaledDeltaTime);
                        yield return null;
                    }
                }
                if(!HapticsEnabled||game==null||!game.enabled||game.Interaction?.IsSuspended==true){hapticPattern=null;yield break;}
                lastHaptic=Time.realtimeSinceStartup;HapticRequests++;NativeHaptics.Pulse(strengths[i]);
            }
            hapticPattern=null;
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
                p.body.isKinematic=true;p.collider.enabled=false;p.renderer.shadowCastingMode=ShadowCastingMode.Off;p.renderer.receiveShadows=false;
                p.transform.position=p.origin;p.transform.localScale=Vector3.one*p.size;p.transform.rotation=Quaternion.identity;p.transform.gameObject.SetActive(true);
            }
        }
        void Update()
        {
            if (game == null || game.Interaction == null) return;
            bool paused = !game.enabled || game.Interaction.IsPaused || game.Interaction.IsSuspended;
            bool worldPaused=!game.enabled||game.Interaction.IsSuspended||game.CurrentScreen==GameScreen.Pause||game.CurrentScreen==GameScreen.SaveRecovery;
            TickAtmosphere(Time.unscaledDeltaTime, worldPaused);
            // Pausing puzzle commands for menus must not pause the menu's audio feedback.
            if (!SoundEnabled||!game.enabled||game.Interaction.IsSuspended)foreach(var voice in voices)if(voice.isPlaying)voice.Stop();
            if (paused) return;
            StepDebris(Mathf.Clamp(Time.unscaledDeltaTime,0,.2f));
            ActiveParticleCount = 0;
            foreach (var p in particles) {
                if (!p.transform.gameObject.activeSelf) continue;
                if(p.source!=null)p.origin=game.UIFeedbackPoint(p.source)+p.sourceOffset;
                p.age += Time.unscaledDeltaTime; float t = p.age / p.life;
                if (t >= 1) { p.transform.gameObject.SetActive(false); continue; }
                ActiveParticleCount++;
                if(!p.wood||p.reduced)p.transform.position = p.reduced ? p.origin : p.origin + p.velocity * p.age + (p.seed?Vector3.right*Mathf.Sin(p.age*5+p.spin)*p.age*.12f:Vector3.down*(.38f*p.age*p.age));
                p.transform.localScale=p.wood?new Vector3(p.size,p.size*.40f,1):Vector3.one*(p.size*(1+t));
                var color = p.color; color.a *= p.wood?1-Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.76f)/.24f)):(1-t)*(1-t);
                p.properties.SetColor("_BaseColor", color); p.renderer.SetPropertyBlock(p.properties);
            }
        }
        public void ClearTails(bool stopAudio=true)
        {
            if(hapticPattern!=null){StopCoroutine(hapticPattern);hapticPattern=null;}
            physicsRemainder=0;
            RowPhysics?.End();
            // The local scene may unload before the owner during Editor shutdown or scene replacement.
            if (particles != null) foreach (var p in particles) if(p.transform!=null)p.transform.gameObject.SetActive(false);
            if (stopAudio&&voices != null) foreach (var voice in voices) if(voice!=null)voice.Stop(); ActiveParticleCount = 0;
        }
        public void RefreshAnchors()
        {
            if(particles==null||game==null)return;
            foreach(var p in particles)if(p.transform.gameObject.activeSelf&&p.source!=null){
                var origin=game.UIFeedbackPoint(p.source)+p.sourceOffset;origin.z=-.7f;
                p.transform.position+=origin-p.origin;p.origin=origin;
            }
        }
        void StepDebris(float seconds)
        {
            if(game.ReducedMotion)foreach(var p in particles)if(p.wood&&!p.reduced&&p.transform.gameObject.activeSelf){p.reduced=true;p.origin=p.transform.position;p.body.isKinematic=true;p.collider.enabled=false;}
            bool active=RowPhysics?.Active==true;foreach(var p in particles)if(p.wood&&!p.reduced&&p.transform.gameObject.activeSelf){active=true;break;}
            if(!active){physicsRemainder=0;return;}
            for(int i=0;i<supports.Length;i++){
                bool visible=game.Machine.DebrisSupport(i/3,.5f+(i%3-1)*.95f/3,out var position,out float width,out float angle);var rail=supports[i];rail.enabled=visible;
                if(visible){rail.transform.SetPositionAndRotation(position-Vector3.up*.045f,Quaternion.Euler(0,0,angle));rail.size=new Vector3(width,.09f,1.4f);}
            }
            physicsRemainder+=seconds;const float dt=1f/90;
            while(physicsRemainder>=dt){
                foreach(var p in particles)if(p.wood&&!p.reduced&&p.transform.gameObject.activeSelf){
                    float wind=game.Machine.WindAt(Mathf.Min(game.VisibleFrame.ActiveFloor,game.Interaction.Attempt.Level.Floors.Count-1));
                    p.body.AddForce(Vector3.right*(wind*.016f),ForceMode.Force);
                }
                RowPhysics?.Step(dt);debrisPhysics.Simulate(dt);physicsRemainder-=dt;
            }
        }
        void OnDestroy(){DisposeAtmosphere();RowPhysics?.Dispose();if(chipMaterial!=null)Destroy(chipMaterial);if(debrisScene.IsValid()&&debrisScene.isLoaded)SceneManager.UnloadSceneAsync(debrisScene);}
    }
}
