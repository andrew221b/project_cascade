using UnityEngine;

namespace Cascade.Presentation
{
    // The shared reference scaler owns phone fit; these small motions retain
    // each island's depth and every plant's attachment to its support.
    public sealed class ConversationReferenceLayout:MonoBehaviour
    {
        CascadeGame game;MenuWorldEnvironment environment;RectTransform[] islands,plants;Vector2[] rests;RectTransform backdrop,foreground;ReferenceShopLayout fit;float groundShift=float.NaN;bool subscribed;
        public int World=>environment.World;
        public float BottomShift=>Mathf.Max(0,(((RectTransform)transform.parent).rect.height/Mathf.Max(.001f,transform.localScale.y)-1280)*.5f);
        public void Initialize(CascadeGame owner,MenuWorldEnvironment world,RectTransform[] scenery,RectTransform[] rootedPlants,RectTransform background,RectTransform plantSurface)
        {
            game=owner;environment=world;fit=GetComponent<ReferenceShopLayout>();backdrop=background;foreground=plantSurface;islands=scenery;plants=rootedPlants;rests=new Vector2[islands.Length];for(int i=0;i<rests.Length;i++)rests[i]=islands[i].anchoredPosition;
            Subscribe();Refresh();
        }
        void OnEnable(){if(game!=null)Subscribe();}
        void OnDisable()=>Unsubscribe();
        void OnDestroy()=>Unsubscribe();
        void Subscribe(){if(!subscribed){Canvas.willRenderCanvases+=Refresh;subscribed=true;}}
        void Unsubscribe(){if(subscribed){Canvas.willRenderCanvases-=Refresh;subscribed=false;}}
        void Refresh(){if(game==null||!game.ConversationOpen||!isActiveAndEnabled)return;float shift=BottomShift;if(shift!=groundShift){groundShift=shift;fit.BindContinuousGround(1018+shift,"ContinuousStoneGround");}foreground.localScale=backdrop.localScale;foreground.sizeDelta=backdrop.sizeDelta;foreground.position=backdrop.position;game.RefreshConversationLayout();}
        void LateUpdate()=>PaintNow();
        public void PaintNow()
        {
            if(game==null||!game.ConversationOpen||!game.CelebrationMotionAllowed)return;
            bool moving=!game.ReducedMotion&&game.Preferences.EffectQuality>0;float t=moving?environment.AnimationAge:0,wind=moving?Cascade.Application.WorkshopWind.Sample(t):0;
            for(int i=0;i<islands.Length;i++){
                islands[i].anchoredPosition=rests[i]+(moving?new Vector2(Mathf.Sin(t*.18f+i*1.8f)*1.7f,Mathf.Sin(t*.37f+i*1.3f)*3.2f):Vector2.zero);
                islands[i].localRotation=Quaternion.Euler(0,0,moving?Mathf.Sin(t*.23f+i)*.35f:0);
            }
            for(int i=0;i<plants.Length;i++)plants[i].localRotation=Quaternion.Euler(0,0,moving?WorldSeasonalArt.PlantSway(t,i,wind)*(i==0?.45f:1):0);
        }
        // A full primary action and a flexible label action share the authored
        // row. Explicit opt-in keeps other reference screens unchanged.
        internal static void PlaceActionPair(UnityEngine.UI.Button primary,UnityEngine.UI.Button tertiary,float top)
        {
            var parent=(RectTransform)primary.transform.parent;const float margin=28;
            var design=primary.GetComponent<GameButtonDesign>();var preset=GameButtonPreset.Load(GameButtonRole.PrimaryGreen);
            var size=design==null?preset.Size:design.DesiredSize(preset.Size*design.DesignUnit);
            float gap=CenteredContentRow.Gap*CenteredContentRow.Unit(parent),left=parent.rect.width-margin-size.x;
            PlaceAction(primary,left,top,size.x,size.y);
            PlaceAction(tertiary,margin,top,left-margin-gap,size.y);
            design?.Refresh();tertiary.GetComponent<GameButtonDesign>()?.Refresh();
        }
        internal static void PlaceAction(UnityEngine.UI.Button button,float x,float y,float width,float height)
        {
            var rect=(RectTransform)button.transform;float unit=CenteredContentRow.Unit(rect)/Mathf.Clamp(rect.GetComponentInParent<Canvas>().rootCanvas.scaleFactor,.001f,1),w=Mathf.Max(width,44*unit),h=Mathf.Max(height,44*unit);
            Place(rect,x-(w-width)*.5f,y-(h-height)*.5f,w,h);
        }
        internal static void Place(RectTransform rect,float x,float y,float width,float height)
        {
            var anchor=new Vector2(0,1);if(rect.anchorMin!=anchor)rect.anchorMin=anchor;if(rect.anchorMax!=anchor)rect.anchorMax=anchor;
            if(rect.pivot!=Vector2.one*.5f)rect.pivot=Vector2.one*.5f;var position=new Vector2(x+width*.5f,-y-height*.5f);var size=new Vector2(width,height);
            if(rect.anchoredPosition!=position)rect.anchoredPosition=position;if(rect.sizeDelta!=size)rect.sizeDelta=size;
        }
    }
}
