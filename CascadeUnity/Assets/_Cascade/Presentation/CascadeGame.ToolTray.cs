using DG.Tweening;
using Cascade.Application;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        RectTransform fieldTray;
        CanvasGroup fieldTrayGroup;
        readonly CanvasGroup[] fieldActionGroups=new CanvasGroup[3];
        readonly Button[] fieldToolButtons=new Button[ShopCatalog.ToolCount];
        readonly Image[] fieldToolIcons=new Image[ShopCatalog.ToolCount];
        readonly TMP_Text[] fieldToolCounts=new TMP_Text[ShopCatalog.ToolCount];
        Tween fieldTrayTween;
        float fieldTrayProgress;
        bool fieldTrayOpen;
        TMP_Text fieldTrayLimit;
        RectTransform fieldTrayPaper;
        float fieldTrayDockHeight;
        public bool FieldToolTrayOpen=>fieldTrayOpen;
        void BuildToolTray()
        {
            fieldTray=Rect("Field tool tray",safeRoot,new Vector2(.05f,0),new Vector2(.95f,0));fieldTray.pivot=new Vector2(.5f,0);
            fieldTrayGroup=fieldTray.gameObject.AddComponent<CanvasGroup>();
            var controls=new[]{undo,hint,drop};
            for(int i=0;i<controls.Length;i++)fieldActionGroups[i]=controls[i].gameObject.AddComponent<CanvasGroup>();
            for(int i=0;i<ShopCatalog.ToolCount;i++){
                int item=i;
                var button=Button("",fieldTray,Vector2.zero,Vector2.zero,()=>UseFieldTool(item));
                button.name="Field tool "+i;fieldToolButtons[i]=button;
                var face=button.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=Color.white;
                var count=button.GetComponentInChildren<TextMeshProUGUI>();count.rectTransform.anchorMin=new Vector2(.02f,0);count.rectTransform.anchorMax=new Vector2(.98f,.30f);
                count.margin=Vector4.zero;count.fontStyle=FontStyles.Bold;count.textWrappingMode=TextWrappingModes.NoWrap;count.color=new Color(1,.97f,.84f);fieldToolCounts[i]=count;
                fieldToolIcons[i]=Picture("Field tool icon "+i,button.transform,new Vector2(.12f,.30f),new Vector2(.88f,.96f),ToolArt(i),true);
                DecorateToolIcon(fieldToolIcons[i],i);
            }
            fieldTrayPaper=Rect("Tool belt limits paper",fieldTray,Vector2.zero,Vector2.zero);
            var paper=Background(fieldTrayPaper,Color.white,false);paper.sprite=enamelControls[2];paper.type=Image.Type.Sliced;paper.pixelsPerUnitMultiplier=3;
            fieldTrayLimit=Text("Field tool limit",fieldTrayPaper,Vector2.zero,Vector2.one,12,"");
            fieldTrayLimit.color=MenuInk;fieldTrayLimit.margin=new Vector4(12,4,12,4);
            SetToolTray(false,true);
        }
        public void ToggleFieldToolTray()
        {
            if(CurrentScreen!=GameScreen.Playing||!Interaction.CanPlay||!Interaction.AllowsControl(PracticeControl.Toolbag)||MechanicPopupOpen)return;
            if(!fieldTrayOpen&&!PendingLesson.HasValue&&RequireActionLesson(MechanicLesson.Toolbag))return;
            CompleteLessonPractice(MechanicLesson.Toolbag);SetToolTray(!fieldTrayOpen);RefreshToolTray();
        }
        void SetToolTray(bool open,bool immediate=false)
        {
            if(fieldTray==null)return;
            fieldTrayTween?.Kill();fieldTrayTween=null;fieldTrayOpen=open;
            fieldTrayGroup.interactable=fieldTrayGroup.blocksRaycasts=false;
            if(open)fieldTray.gameObject.SetActive(true);
            if(immediate||ReducedMotion||!IntroductionAnimationAllowed){fieldTrayProgress=open?1:0;PaintToolTray();fieldTray.gameObject.SetActive(open);}
            else fieldTrayTween=DOTween.To(()=>fieldTrayProgress,value=>{fieldTrayProgress=value;PaintToolTray();},open?1:0,Mathf.Max(.08f,(open?.40f:.30f)*Mathf.Abs((open?1:0)-fieldTrayProgress)))
                .SetEase(Ease.InOutCubic).SetUpdate(true).SetTarget(fieldTray).SetLink(gameObject)
                .OnComplete(()=>{fieldTrayTween=null;if(!fieldTrayOpen){fieldTray.gameObject.SetActive(false);RefreshFieldTools();}});
            if(Interaction!=null)ApplySafeArea();
        }
        void PaintToolTray()
        {
            if(fieldTray==null)return;
            float p=Mathf.Clamp01(fieldTrayProgress),unit=ControlUnit;
            fieldTrayGroup.alpha=Mathf.SmoothStep(0,1,p);
            fieldTrayGroup.interactable=fieldTrayGroup.blocksRaycasts=fieldTrayOpen&&p>=.995f;
            fieldTray.localScale=Vector3.one;
            fieldTray.anchoredPosition=new Vector2(0,8*unit);
            // One calm row expands horizontally from the bag. It keeps the tool order legible
            // and makes closing the tray a direct reverse of opening it.
            float width=actions.rect.width,gap=4*unit;
            float cell=Mathf.Min(50*unit,(width-gap*(fieldToolButtons.Length-1))/fieldToolButtons.Length);
            float bagCenter=width*.5f;
            float rowWidth=fieldToolButtons.Length*cell+(fieldToolButtons.Length-1)*gap;
            float first=(width-rowWidth)*.5f+cell*.5f;
            for(int i=0;i<fieldToolButtons.Length;i++){
                if(fieldToolButtons[i]==null)continue;
                float x=first+i*(cell+gap);
                var rect=(RectTransform)fieldToolButtons[i].transform;
                rect.anchorMin=rect.anchorMax=Vector2.zero;rect.pivot=new Vector2(.5f,0);
                rect.sizeDelta=new Vector2(cell,52*unit);
                rect.anchoredPosition=new Vector2(Mathf.Lerp(bagCenter,x,p),0);
                float spring=Mathf.Sin(p*Mathf.PI*3+i*.35f)*(1-p)*.08f;
                rect.localScale=Vector3.one*(.72f+.28f*p+spring);
            }
            LayoutPlayActions();
            if(Interaction!=null&&receiver!=null){
                var previous=receiver.anchoredPosition;var previousFloor=floorArea.offsetMin;
                LayoutLowerGameArea(unit);
                if(previous!=receiver.anchoredPosition||previousFloor!=floorArea.offsetMin){
                    machine?.Sync();SyncTokenFaces();SyncDeviceSuspension(0);feedback?.RefreshAnchors();RestoreTouchPose();
                }
            }
        }
        void LayoutToolTray(float unit)
        {
            if(fieldTray==null)return;
            fieldTrayLimit.fontSize=12*unit*(Preferences.LargeText?1.12f:1);
            float paperHeight=Mathf.Max(36*unit,fieldTrayLimit.GetPreferredValues(fieldTrayLimit.text,Mathf.Max(1,actions.rect.width-24),0).y+12*unit);
            fieldTrayDockHeight=62*unit+paperHeight;
            fieldTray.sizeDelta=new Vector2(0,fieldTrayDockHeight);
            fieldTrayPaper.anchorMin=fieldTrayPaper.anchorMax=Vector2.zero;fieldTrayPaper.pivot=Vector2.zero;
            fieldTrayPaper.anchoredPosition=new Vector2(0,62*unit);fieldTrayPaper.sizeDelta=new Vector2(actions.rect.width,paperHeight);
            PaintToolTray();
            foreach(var count in fieldToolCounts)if(count!=null)count.fontSize=13*unit;
        }
        void RefreshToolTray()
        {
            if(fieldTray==null||Interaction==null)return;
            fieldTrayLimit.SetCopy(UiCopy.Join("\n",new UiCopy[]{new UiCopy("Uses left this stage: {0}/2. Each type once.",Interaction.Attempt.ToolUsesRemaining),"Using a tool limits this level to 2 stars."}));
            LayoutToolTray(ControlUnit);
            if(fieldTrayOpen&&(CurrentScreen!=GameScreen.Playing||!HasFieldTools))SetToolTray(false,true);
            for(int i=0;i<ShopCatalog.ToolCount;i++){
                var tool=(TravelTool)i;int stock=Stock(tool);fieldToolCounts[i].text="× "+stock;
                fieldToolButtons[i].interactable=fieldTrayOpen&&!toolSearchPending&&!MechanicPopupOpen&&Interaction.CanPlay&&stock>0&&Interaction.AllowsTool(tool);
                if(fieldToolIcons[i].transform.IsChildOf(fieldTray))fieldToolIcons[i].color=new Color(1,1,1,stock>0&&ToolEligible(tool)?1:.40f);
            }
        }
        void UseFieldTool(int item)
        {
            if(!fieldTrayOpen||!fieldTrayGroup.interactable)return;
            var tool=(TravelTool)item;
            if(!ToolEligible(tool)){reactionBanner.Show(ToolEligibilityText(tool),"");return;}
            toolsReturnScreen=GameScreen.Playing;toolsItem=shopItem=item;
            UseShopTool((TravelTool)item,fieldToolIcons[item].rectTransform);
        }
    }
}
