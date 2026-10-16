using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Decorative leaves have their own pool and never read or change trophy equipment.
    public sealed class MenuLeafDrift:MonoBehaviour
    {
        sealed class Leaf { public Image Image;public RectTransform Rect;public Vector2 Position,Velocity;public float Delay,Seed,Angle,Age;public int Cycle;public bool Flying; }
        readonly Leaf[] leaves=new Leaf[8];CascadeGame game;RectTransform root;float clock;bool depth,pauseDecoration;Material softLeaf,farLeaf;
        public bool DepthOfField=>depth;
        public bool RunsOnPause=>pauseDecoration;
#if UNITY_EDITOR
        bool review;public void SetReviewMode(bool value){review=value;}public void AdvanceReview(float delta){Tick(delta);}
#endif
        public int VisibleCount {get;private set;}
        public void Initialize(CascadeGame owner,Sprite sprite,bool depthOfField=false,bool animateOnPause=false)
        {
            game=owner;root=(RectTransform)transform;depth=depthOfField;pauseDecoration=animateOnPause;
            if(depth){var shader=Resources.Load<Shader>("CascadeArt/ItemsReferenceOct14/LeafDepthBlur");if(shader!=null){softLeaf=new Material(shader){name="Soft distant foliage"};softLeaf.SetFloat("_Blur",.065f);farLeaf=new Material(shader){name="Far out of focus foliage"};farLeaf.SetFloat("_Blur",.1f);}}
            if(sprite==null){enabled=false;return;}
            for(int i=0;i<leaves.Length;i++){
                var go=new GameObject("Natural falling leaf "+i,typeof(RectTransform),typeof(Image));go.transform.SetParent(transform,false);
                var image=go.GetComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;image.enabled=false;
                if(depth&&i%4!=0)image.material=i%2==0?farLeaf:softLeaf;
                var rect=image.rectTransform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
                leaves[i]=new Leaf{Image=image,Rect=rect,Seed=Mathf.Repeat(.13f+i*.618034f,1),Delay=.1f+i*1.1f};
            }
            // Enter pause with leaves already in flight, using the same offscreen trajectories.
            if(pauseDecoration)for(int n=0;n<600;n++)Tick(1f/30);
        }
        public void WarmStart(){for(int n=0;n<600;n++)Tick(1f/30);}
        void OnDestroy(){if(softLeaf!=null)Destroy(softLeaf);if(farLeaf!=null)Destroy(farLeaf);}
        void LateUpdate()
        {
#if UNITY_EDITOR
            if(review)return;
#endif
            Tick(Mathf.Min(Time.unscaledDeltaTime,.05f));
        }
        void Tick(float dt)
        {
            if(game==null||root==null)return;
            bool motion=pauseDecoration?game.CurrentScreen==GameScreen.Pause&&game.CelebrationMotionAllowed&&!game.MechanicPopupOpen:game.CosmeticAmbianceMotionAllowed;
            bool allowed=!game.ReducedMotion&&game.Preferences.EffectQuality>0&&motion&&game.EnvironmentWorld!=6&&game.EnvironmentWorld!=4&&game.EnvironmentWorld!=8;
            int capacity=game.Preferences.EffectQuality==1?2:3;
            if(!allowed){VisibleCount=0;for(int i=0;i<leaves.Length;i++)if(leaves[i]!=null)leaves[i].Image.enabled=false;return;}
            clock+=dt;VisibleCount=0;
            float gust=.055f*Mathf.Sin(clock*.31f)+.11f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(clock*.18f-.8f)),3);
            for(int i=0;i<leaves.Length;i++){
                var leaf=leaves[i];if(leaf==null)continue;
                if(i>=capacity){leaf.Image.enabled=false;continue;}
                if(!leaf.Flying){
                    leaf.Delay-=dt;if(leaf.Delay>0){leaf.Image.enabled=false;continue;}
                    // The whole leaf starts beyond the upper mask; no visible mid-screen spawn.
                    leaf.Position=new Vector2(.05f+.90f*Mathf.Repeat(leaf.Seed+leaf.Cycle*.381966f,1),1.045f);
                    leaf.Velocity=new Vector2(gust,-.023f);leaf.Age=0;leaf.Angle=leaf.Seed*180;leaf.Flying=true;
                }
                leaf.Age+=dt;
                float flutter=.024f*Mathf.Sin(clock*1.4f+leaf.Seed*8);
                leaf.Velocity+=new Vector2((gust+flutter-leaf.Velocity.x)*1.15f,-.075f*(.7f+leaf.Seed*.5f)-leaf.Velocity.y*1.55f)*dt;
                leaf.Position+=leaf.Velocity*dt*(depth?(i%4==0?1.12f:.74f):1);leaf.Angle+=(gust*130+Mathf.Sin(clock*1.5f+leaf.Seed*9)*28)*dt;
                if(leaf.Position.y<-.055f||leaf.Position.x<-.06f||leaf.Position.x>1.06f){leaf.Flying=false;leaf.Image.enabled=false;leaf.Delay=.7f+leaf.Seed*2;leaf.Cycle++;continue;}
                leaf.Image.enabled=true;VisibleCount++;
                leaf.Rect.anchoredPosition=Vector2.Scale(leaf.Position-Vector2.one*.5f,root.rect.size);
                float size=Mathf.Min(root.rect.width,root.rect.height)*(pauseDecoration?(.058f+leaf.Seed*.016f):depth?(.046f+leaf.Seed*.021f):(.029f+leaf.Seed*.015f));
                leaf.Rect.sizeDelta=new Vector2(size*1.1f,size);
                leaf.Rect.localRotation=Quaternion.Euler(0,0,leaf.Angle);
                leaf.Rect.localScale=new Vector3(.4f+.6f*Mathf.Abs(Mathf.Sin(leaf.Age*1.9f+leaf.Seed*8)),.92f+.08f*Mathf.Sin(leaf.Age),1);
                leaf.Image.color=game.EnvironmentWorld==2?new Color(1,.64f,.24f,depth?(i%4==0?.85f:.67f):1):depth?new Color(1,1,1,i%4==0?.85f:.67f):Color.white;
            }
        }
    }
}
