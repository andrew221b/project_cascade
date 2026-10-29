using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cascade.Application;

namespace Cascade.Presentation
{
    [DefaultExecutionOrder(40)]
    public sealed class CharacterPoseView:MonoBehaviour
    {
        const string ArtPath="CascadeArt/CharacterMotionOct16/";
        static readonly int BlinkId=Shader.PropertyToID("_Blink"),GazeId=Shader.PropertyToID("_Gaze"),SpeakingId=Shader.PropertyToID("_Speaking"),TintId=Shader.PropertyToID("_LightTint"),WorldId=Shader.PropertyToID("_World"),MicroId=Shader.PropertyToID("_MicroMotion");
        CascadeGame game;Material face,lampMaterial,baseMaterial;CharacterArtPart lamp;Texture2D lampMask,contour,earTips;
        CharacterSpeechFaces.Pose speechPose;Material speechRendered;
        MikaCosmeticView costume;TextMeshProUGUI words;MenuWorldEnvironment environment;bool environmentBound;string spoken;float speechAge,lastPaintedAge=-1;bool lastActive;int lastQuality;
        bool review,appleBite=true;int seed;
        public Image Hero{get;private set;}
        public Sprite SourceSprite{get;private set;}
        public CharacterPoseProfile Profile{get;private set;}
        public CharacterPoseMotion Motion{get;private set;}
        public CharacterPoseMesh Mesh{get;private set;}
        public int Quality=>game.Preferences.EffectQuality;
        public bool MotionActive=>Profile!=null&&!game.ReducedMotion&&Quality>0;
        public bool AnimationAllowed=>game!=null&&game.CelebrationMotionAllowed&&!game.MechanicPopupOpen;
        public float AnimationAge=>Motion?.Age??0;
        public static CharacterPoseView Attach(CascadeGame owner,Image image,bool animateAppleBite=true)
        {
            if(owner==null||image==null)return null;var view=image.GetComponent<CharacterPoseView>()??image.gameObject.AddComponent<CharacterPoseView>();
            view.Initialize(owner,image,animateAppleBite);return view;
        }
        void Initialize(CascadeGame owner,Image image,bool animateAppleBite)
        {
            if(appleBite!=animateAppleBite)SourceSprite=null;appleBite=animateAppleBite;
            game=owner;Hero=image;costume=image.GetComponent<MikaCosmeticView>();if(seed==0)seed=System.Guid.NewGuid().GetHashCode()&0x3fffffff;
            Bind();
        }
        void Bind()
        {
            if(Hero==null||Hero.sprite==SourceSprite)return;
            SourceSprite=Hero.sprite;Profile=CharacterPoseProfile.For(SourceSprite);
            if(Profile==null){if(Hero.material==face)Hero.material=baseMaterial;if(lamp!=null)lamp.gameObject.SetActive(false);Mesh?.Invalidate();Motion=null;return;}
            if(face==null){face=new Material(Resources.Load<Shader>(ArtPath+"CharacterFace")){name="Authored character face and pose"};baseMaterial=Hero.material;}
            earTips=Profile.Name=="Lumi"?Resources.Load<Texture2D>(ArtPath+"LumiEarTips"):null;
            Motion=new CharacterPoseMotion(Profile,seed,appleBite);lastPaintedAge=-1;speechAge=0;
            // Keep animated vertices from rebuilding otherwise static menu UI.
            var canvas=Hero.GetComponent<Canvas>();if(canvas==null)canvas=Hero.gameObject.AddComponent<Canvas>();canvas.overrideSorting=false;
            // Trophy previews already have a touch action on the hero. A nested
            // canvas needs its own raycaster to preserve that existing action.
            var raycaster=Hero.GetComponent<GraphicRaycaster>();if(raycaster==null)Hero.gameObject.AddComponent<GraphicRaycaster>();
            Mesh=Hero.GetComponent<CharacterPoseMesh>()??Hero.gameObject.AddComponent<CharacterPoseMesh>();Mesh.Initialize(this);Mesh.Invalidate();
            var c=Profile.Cell;face.SetVector("_Cell",new Vector4(c.x/SourceSprite.texture.width,c.y/SourceSprite.texture.height,c.width/SourceSprite.texture.width,c.height/SourceSprite.texture.height));
            bool mouse=Profile.Kind!=CharacterPoseKind.Speaker;
            var near=Profile.NearEye;var far=Profile.FarEye;float eyeScale=Profile.Name.StartsWith("Mika",System.StringComparison.Ordinal)||mouse?1.18f:1.08f;
            if(Profile.Name.StartsWith("MothMask",System.StringComparison.Ordinal)||Profile.Name=="Aster"||Profile.Name=="Gran")eyeScale=1;
            near.z*=eyeScale;near.w*=eyeScale;far.z*=eyeScale;far.w*=eyeScale;
            face.SetVector("_NearEye",near);face.SetVector("_FarEye",far);face.SetVector("_Mouth",Profile.Mouth);
            face.SetFloat("_MouseBlink",mouse?1:0);
            face.SetTexture("_HalfBlinkTex",Resources.Load<Texture2D>("CascadeArt/WelcomeMotionOct16/HalfBlink"));
            face.SetTexture("_ClosedBlinkTex",Resources.Load<Texture2D>(mouse?"CascadeArt/WelcomeMotionOct16/ClosedBlink":ArtPath+"ResidentEyelids"));
            int npc=System.Array.IndexOf(new[]{"Pip","Rue","Aster","Bram","Sora","Neri","Toma","Lumi","Gran"},Profile.Name);
            if(npc>=0){face.SetVector("_NearLidUv",LidCell(npc*2));face.SetVector("_FarLidUv",LidCell(npc*2+1));face.SetFloat("_LidScale",eyeScale/1.35f);}
            speechPose=CharacterSpeechFaces.For(Profile,SourceSprite);speechRendered=null;CharacterSpeechFaces.Apply(face,speechPose);
            face.SetFloat("_UseContour",Profile.Contour?1:0);
            if(Profile.Contour){
                if(contour==null){var points=FailureReferenceArt.HeroContour;var normalized=new Vector2[points.Length];for(int i=0;i<points.Length;i++)normalized[i]=new Vector2((points[i].x-c.x)/c.width,(points[i].y-c.y)/c.height);contour=BakeMask(normalized,384,"Original failure cutout contour");}
                face.SetTexture("_ContourMask",contour);
            }
            bool eating=Profile.Kind==CharacterPoseKind.Apple&&appleBite;face.SetFloat("_UseAppleFrames",eating?1:0);
            if(eating)face.SetTexture("_AppleFrames",Resources.Load<Texture2D>(ArtPath+"AppleBiteFrames"));
            bool victory=Profile.Kind==CharacterPoseKind.Victory;face.SetFloat("_UseLantern",victory?1:0);
            if(victory)CreateVictoryLantern();else if(lamp!=null)lamp.gameObject.SetActive(false);
        }
        static Vector4 LidCell(int index)=>new Vector4(index%5/5f,1-(index/5+1)/4f,.2f,.25f);
        void CreateVictoryLantern()
        {
            if(lampMask==null)lampMask=BakeMask(new[]{new Vector2(.802f,.255f),new Vector2(.830f,.245f),new Vector2(.840f,.275f),new Vector2(.930f,.276f),new Vector2(.940f,.252f),new Vector2(.960f,.267f),new Vector2(.965f,.310f),new Vector2(.940f,.360f),new Vector2(.934f,.400f),new Vector2(.947f,.490f),new Vector2(.944f,.590f),new Vector2(.976f,.628f),new Vector2(.969f,.660f),new Vector2(.768f,.665f),new Vector2(.754f,.640f),new Vector2(.769f,.607f),new Vector2(.790f,.550f),new Vector2(.780f,.450f),new Vector2(.794f,.380f),new Vector2(.794f,.335f),new Vector2(.785f,.290f)},512,"Original victory lantern silhouette");
            face.SetTexture("_LanternMask",lampMask);
            if(lampMaterial==null)lampMaterial=new Material(face){name="Original rigid victory lantern"};else lampMaterial.CopyPropertiesFromMaterial(face);lampMaterial.SetFloat("_LanternLayer",1);
            if(lamp==null){var obj=new GameObject("Original rigid Mika lantern",typeof(RectTransform),typeof(CanvasRenderer),typeof(CharacterArtPart));var r=(RectTransform)obj.transform;r.SetParent(Hero.transform,false);r.pivot=Hero.rectTransform.pivot;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;lamp=obj.GetComponent<CharacterArtPart>();}
            lamp.gameObject.SetActive(true);lamp.Initialize(this,lampMaterial);lamp.Invalidate();
        }
        public void SetSpeechSource(TextMeshProUGUI body){words=body;spoken=body==null?null:body.text;speechAge=0;}
        public static float SpeechShape(string text,float seconds,int visible=int.MaxValue)
        {
            if(string.IsNullOrEmpty(text)||seconds<.25f||visible<=0)return 0;
            float reading=Mathf.Clamp(text.Length/10f,3,12),time=Mathf.Repeat(seconds-.25f,reading+1.15f);
            if(time>=reading)return 0;
            int index=Mathf.Min(text.Length-1,Mathf.FloorToInt(time/reading*text.Length));
            if(index>=visible||char.IsPunctuation(text[index]))return 0;
            float phase=Mathf.Repeat(time,.64f);
            return CharacterPoseMotion.Ease(.06f,.23f,phase)*(1-CharacterPoseMotion.Ease(.40f,.62f,phase))
                *CharacterPoseMotion.Ease(0,.24f,time)*(1-CharacterPoseMotion.Ease(reading-.25f,reading,time));
        }
        void LateUpdate()
        {
            if(game==null||Hero==null)return;Bind();if(Profile==null)return;
            if(!environmentBound){environment=game.GetComponentInChildren<MenuWorldEnvironment>();environmentBound=true;}
            if(Hero.material!=face){baseMaterial=Hero.material;Hero.material=face;}
            if(!review&&MotionActive&&AnimationAllowed&&!Hero.canvasRenderer.cull){
                float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);speechAge+=dt;
                if(words!=null&&spoken!=words.text){spoken=words.text;speechAge=0;}
                float speech=words==null?0:SpeechShape(spoken,speechAge,words.maxVisibleCharacters);
                Motion.Advance(dt,WorkshopWind.Sample(environment!=null?environment.AnimationAge:Motion.Age+(Profile.Contour?16:0)),speech);
            }
            Paint();
        }
        void Paint()
        {
            if(Profile==null)return;
            var tint=baseMaterial!=null&&baseMaterial.HasProperty(TintId)?baseMaterial.GetColor(TintId):Color.white;
            float cut=baseMaterial!=null&&baseMaterial.HasProperty("_CutSky")?baseMaterial.GetFloat("_CutSky"):0;
            PaintMaterial(face,tint,cut);var rendered=Hero.materialForRendering;if(rendered!=face){if(speechRendered!=rendered){CharacterSpeechFaces.Apply(rendered,speechPose);speechRendered=rendered;}PaintMaterial(rendered,tint,cut);}
            if(lamp!=null&&lamp.gameObject.activeSelf){PaintMaterial(lampMaterial,tint,0);var renderedLamp=lamp.materialForRendering;if(renderedLamp!=lampMaterial)PaintMaterial(renderedLamp,tint,0);}
            bool active=MotionActive;
            if(lastPaintedAge!=Motion.Age||lastActive!=active||lastQuality!=Quality){Hero.SetVerticesDirty();if(lamp!=null&&lamp.gameObject.activeSelf)lamp.SetVerticesDirty();lastPaintedAge=Motion.Age;lastActive=active;lastQuality=Quality;}
            if(costume!=null&&costume.PreviewBadge!=null&&costume.PreviewBadge.gameObject.activeSelf){
                var badge=costume.PreviewBadge;badge.material=baseMaterial;
                if(active&&Mesh.HasBounds){
                    var uv=Vector2.Lerp(Mesh.UvLow,Mesh.UvHigh,.5f);var size=Mesh.High-Mesh.Low;var local=(Vector2)badge.rectTransform.localPosition;
                    uv=new Vector2(Mathf.LerpUnclamped(Mesh.UvLow.x,Mesh.UvHigh.x,(local.x-Mesh.Low.x)/size.x),Mathf.LerpUnclamped(Mesh.UvLow.y,Mesh.UvHigh.y,(local.y-Mesh.Low.y)/size.y));
                    var p=Profile.Point(SourceSprite,uv);var posed=Motion.Deform(p);var newUv=Profile.Uv(SourceSprite,posed);var scale=new Vector2(size.x/(Mesh.UvHigh.x-Mesh.UvLow.x),size.y/(Mesh.UvHigh.y-Mesh.UvLow.y));
                    badge.rectTransform.anchoredPosition+=Vector2.Scale(newUv-uv,scale);
                    var edge=Motion.Deform(p+Vector2.right*.01f)-posed;badge.rectTransform.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(edge.y*Profile.Cell.height,edge.x*Profile.Cell.width)*Mathf.Rad2Deg);
                }
            }
        }
        void PaintMaterial(Material m,Color tint,float cut)
        {
            if(m==null)return;m.SetTexture("_LumiEarTips",earTips);m.SetFloat("_HaveLumiEarTips",earTips!=null?1:0);m.SetVector(MicroId,MotionActive?new Vector4(Motion.WhiskerFan,Motion.Expression,Motion.Sniff,Profile.Whiskers?1:0):Vector4.zero);m.SetFloat(BlinkId,MotionActive?Motion.Blink:0);m.SetVector(GazeId,MotionActive?Motion.Gaze:Vector2.zero);m.SetFloat(SpeakingId,MotionActive?Motion.MouthOpen:0);m.SetFloat("_UseAppleFrames",Profile.Kind==CharacterPoseKind.Apple&&appleBite?1:0);m.SetFloat("_AppleFrame",appleBite?MikaAppleFrames.Frame(Motion.Age):0);m.SetColor(TintId,tint);m.SetFloat(WorldId,baseMaterial!=null&&baseMaterial.HasProperty(WorldId)?baseMaterial.GetFloat(WorldId):0);m.SetFloat("_CutSky",cut);
        }
        internal void PaintMagnifiedMaterial(Material target){if(Profile==null)return;PaintMaterial(target,baseMaterial!=null&&baseMaterial.HasProperty(TintId)?baseMaterial.GetColor(TintId):Color.white,0);}
        static Texture2D BakeMask(Vector2[] polygon,int size,string name)
        {
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name=name,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){
                var p=new Vector2((x+.5f)/size,1-(y+.5f)/size);bool inside=InsidePolygon(p,polygon);
                pixels[y*size+x]=new Color32(255,255,255,inside?(byte)255:(byte)0);
            }
            texture.SetPixels32(pixels);texture.Apply(false,true);return texture;
        }
        static bool InsidePolygon(Vector2 p,Vector2[] polygon){bool inside=false;for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++){var a=polygon[i];var b=polygon[j];if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;}return inside;}
#if UNITY_EDITOR
        public void SetReviewFace(float blink,float articulation){face.SetFloat(BlinkId,blink);face.SetFloat(SpeakingId,articulation);var m=Hero.materialForRendering;if(m!=face){m.SetFloat(BlinkId,blink);m.SetFloat(SpeakingId,articulation);}}
        public void ResetReview(int salt=197){review=true;seed=salt;if(Profile!=null){Motion=new CharacterPoseMotion(Profile,salt,appleBite);Paint();}}
        public void AdvanceReview(float dt,float wind,float speech=0){review=true;Motion?.Advance(dt,wind,speech);Paint();}
        public void EndReview(){review=false;}
#endif
        void OnDisable(){lastPaintedAge=-1;}
        void OnDestroy(){if(face!=null)Destroy(face);if(lampMaterial!=null)Destroy(lampMaterial);if(lampMask!=null)Destroy(lampMask);if(contour!=null)Destroy(contour);}
    }
}
