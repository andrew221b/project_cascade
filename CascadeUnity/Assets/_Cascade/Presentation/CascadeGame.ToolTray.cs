using DG.Tweening;
using Cascade.Application;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        RectTransform fieldTray,fieldTrayStage,fieldTrayPaper;CanvasGroup fieldTrayGroup;
        readonly CanvasGroup[] fieldActionGroups=new CanvasGroup[3];
        readonly Button[] fieldToolButtons=new Button[ShopCatalog.ToolCount];
        readonly Image[] fieldToolIcons=new Image[ShopCatalog.ToolCount];
        readonly TMP_Text[] fieldToolCounts=new TMP_Text[ShopCatalog.ToolCount];
        Tween fieldTrayTween;float fieldTrayProgress,fieldTrayDockHeight;bool fieldTrayOpen;
        TMP_Text fieldTrayLimit,fieldTrayDescription,fieldApplyCaption,fieldHeading,fieldShopCaption;Image fieldHeadingArt;Button fieldApplyButton;
        int fieldSelected=-1;public bool FieldToolTrayOpen=>fieldTrayOpen;
        void BuildToolTray()
        {
            fieldTray=Rect("Field tool tray",safeRoot.parent,Vector2.zero,Vector2.one);
            fieldTrayGroup=fieldTray.gameObject.AddComponent<CanvasGroup>();
            var overlay=fieldTray.gameObject.AddComponent<Canvas>();overlay.overrideSorting=true;overlay.sortingOrder=40;fieldTray.gameObject.AddComponent<GraphicRaycaster>();
            Background(fieldTray,new Color(.025f,.035f,.04f,.70f),true);
            fieldTrayStage=Rect("Field tools reference layout",fieldTray,new Vector2(.5f,.5f),new Vector2(.5f,.5f));fieldTrayStage.sizeDelta=new Vector2(720,1280);
            ReferencePicture("Field tools warm panel",fieldTrayStage,GameplayReferenceArt.Sprite("FieldHelpPanel"),50,239,620,713,false);
            fieldHeadingArt=ReferencePicture("Field tools heading board",fieldTrayStage,GameplayReferenceArt.Sprite("FieldHelpTitle"),143,235,434,115,false);
            fieldHeadingArt.gameObject.AddComponent<ReferenceHeaderTheme>().Initialize(this);
            fieldHeading=ReferenceText("field tools heading",fieldTrayStage,ReferenceCopy("My things","Мои вещи","Мої речі"),210,263,300,60,46);
            fieldHeading.fontStyle=FontStyles.Bold;
            var close=ReferenceButton("Close field tools",fieldTrayStage,566,251,78,82,()=>SetToolTray(false),"RedAction");RegisterButtonDesign(close,GameButtonRole.IconSecondary);ReferenceText("field tools close",ReferenceButtonBody(close),UiCopy.Raw("×"),13,9,52,64,57,true);
            for(int display=0;display<ShopCatalog.ToolCount;display++){
                int item=ItemsDisplayOrder[display],column=display%3,row=display/3;float x=120+column*165,y=361+row*158;
                var button=ReferenceButton("Field tool "+item,fieldTrayStage,x,y,149,149,()=>SelectFieldTool(item));fieldToolButtons[item]=button;
                button.GetComponent<Image>().sprite=CleanMenuArt.Get("Card");button.GetComponent<Image>().type=Image.Type.Sliced;button.gameObject.AddComponent<PrimaryButtonSkin>().Configure(CleanMenuArt.Get("Card"));
                fieldToolIcons[item]=ReferencePicture("Field tool icon "+item,ReferenceButtonBody(button),FieldHelpToolArt(item),18.5f,18.5f,112,112);
                DrawItemsStock(ReferenceButtonBody(button),"field "+item,80,95,64,50,0,24);
                fieldToolCounts[item]=ReferenceButtonBody(button).Find("Reference shop items stock field "+item).GetComponent<TMP_Text>();
            }
            fieldTrayPaper=ReferenceRect("Tool belt limits paper",fieldTrayStage,91,660,538,155);
            var paper=Background(fieldTrayPaper,Color.white,false);paper.sprite=GameplayReferenceArt.Sprite("FieldHelpDescription");paper.preserveAspect=false;
            fieldTrayLimit=ReferenceText("field selected title",fieldTrayPaper,UiCopy.Raw(""),57,46,425,35,26);
            fieldTrayDescription=ReferenceText("field selected description",fieldTrayPaper,UiCopy.Raw(""),58,82,423,50,21,false,true);
            fieldTrayDescription.alignment=TextAlignmentOptions.Center;
            fieldApplyButton=ReferenceButton("Apply field tool",fieldTrayStage,64,801,480,104,ApplySelectedFieldTool,"GrayAction");fieldApplyCaption=ReferenceText("field apply caption",ReferenceButtonBody(fieldApplyButton),ReferenceCopy("Choose help","Выбери помощь","Обери допомогу"),18,16,248,58,30,true);
            var shop=ReferenceButton("Field tools shop",fieldTrayStage,560,805,96,96,OpenFieldToolsShop,"GreenAction");ReferencePicture("Field tools storefront",ReferenceButtonBody(shop),ItemsArt("Storefront"),23,13,68,65);RegisterButtonDesign(shop,GameButtonRole.IconPrimary);fieldShopCaption=ReferenceText("field shop caption",ReferenceButtonBody(shop),ReferenceCopy("Shop","Магазин","Магазин"),96,17,108,55,28,true);
            SetToolTray(false,true);
        }
        public void ToggleFieldToolTray()
        {
            if(CurrentScreen!=GameScreen.Playing||!Interaction.CanPlay||!Interaction.AllowsControl(PracticeControl.Toolbag)||MechanicPopupOpen)return;
            if(!fieldTrayOpen&&!PendingLesson.HasValue&&RequireActionLesson(MechanicLesson.Toolbag))return;
            CompleteLessonPractice(MechanicLesson.Toolbag);toolsReturnScreen=GameScreen.Playing;SetToolTray(!fieldTrayOpen);RefreshToolTray();
        }
        void SetToolTray(bool open,bool immediate=false)
        {
            if(fieldTray==null)return;
            fieldTrayTween?.Kill();fieldTrayTween=null;fieldTrayOpen=open;fieldTrayGroup.interactable=fieldTrayGroup.blocksRaycasts=false;
            if(open){fieldSelected=-1;Interaction?.CancelPointer();EndTokenHold();fieldTray.gameObject.SetActive(true);machine?.SetAmbientPaused(true);}
            else machine?.SetAmbientPaused(CurrentScreen!=GameScreen.Playing||MechanicPopupOpen);
            if(immediate||ReducedMotion||!IntroductionAnimationAllowed){fieldTrayProgress=open?1:0;PaintToolTray();fieldTray.gameObject.SetActive(open);}
            else fieldTrayTween=DOTween.To(()=>fieldTrayProgress,value=>{fieldTrayProgress=value;PaintToolTray();},open?1:0,Mathf.Max(.08f,(open?.38f:.26f)*Mathf.Abs((open?1:0)-fieldTrayProgress))).SetEase(Ease.InOutCubic).SetUpdate(true).SetTarget(fieldTray).SetLink(gameObject).OnComplete(()=>{fieldTrayTween=null;if(!fieldTrayOpen){fieldTray.gameObject.SetActive(false);RefreshFieldTools();}});
            if(Interaction!=null&&isActiveAndEnabled&&menuRoot!=null)ApplySafeArea();
        }
        void PaintToolTray()
        {
            if(fieldTray==null)return;float p=Mathf.Clamp01(fieldTrayProgress);fieldTrayGroup.alpha=Mathf.SmoothStep(0,1,p);fieldTrayGroup.interactable=fieldTrayGroup.blocksRaycasts=fieldTrayOpen&&p>=.995f;
            fieldTrayStage.localScale=Vector3.one*Mathf.Min(fieldTray.rect.width/720f,fieldTray.rect.height/1280f)*(1-.055f*(1-p));
            fieldTrayStage.anchoredPosition=Vector2.down*(1-p)*18;
        }
        void LayoutToolTray(float unit){fieldTrayDockHeight=0;PaintToolTray();}
        Sprite FieldHelpToolArt(int item)
        {
            switch((TravelTool)item){
                case TravelTool.SignalLantern:return GameplayReferenceArt.Sprite("FieldHelpLantern")??ItemsToolArt(item);
                case TravelTool.LandingLens:return Resources.Load<Sprite>("CascadeArt/Devices/BrassKey")??ItemsToolArt(item);
                case TravelTool.RepairKit:return GameplayReferenceArt.Sprite("FieldHelpPickaxe")??ItemsToolArt(item);
                default:return ItemsToolArt(item);
            }
        }
        void RefreshToolTray()
        {
            if(fieldTray==null||Interaction==null)return;
            if(fieldTrayOpen&&CurrentScreen!=GameScreen.Playing)SetToolTray(false,true);
            for(int i=0;i<ShopCatalog.ToolCount;i++){
                int stock=Stock((TravelTool)i);fieldToolCounts[i].text="×"+stock;
                fieldToolButtons[i].interactable=fieldTrayOpen&&!toolSearchPending&&!MechanicPopupOpen;
                var face=fieldToolButtons[i].GetComponent<Image>();var skin=face.GetComponent<PrimaryButtonSkin>();skin.Configure(CleanMenuArt.Get(i==fieldSelected?"SelectedCard":"Card"));
                fieldToolIcons[i].material=stock==0?OwnedTrophyMaterial():null;fieldToolIcons[i].color=Color.white;
            }
            fieldHeading.SetCopy(ReferenceCopy("My things","Мои вещи","Мої речі"));
            fieldShopCaption.SetCopy(ReferenceCopy("Shop","Магазин","Магазин"));
            int zone=machine?.LivingWorld.Zone??0;
            fieldHeadingArt.sprite=GameplayReferenceArt.Sprite("FieldHelpTitle");
            bool selected=fieldSelected>=0;
            fieldTrayLimit.gameObject.SetActive(selected);
            var descriptionRect=fieldTrayDescription.rectTransform;
            descriptionRect.anchoredPosition=new Vector2(58,selected?-82:-54);
            descriptionRect.sizeDelta=new Vector2(423,selected?50:69);
            fieldTrayDescription.fontSizeMax=selected?21:27;
            fieldTrayDescription.fontSizeMin=selected?15:21;
            if(!selected)fieldTrayDescription.SetCopy(ReferenceCopy("Use items to make levels easier!","Используй предметы, чтобы проходить уровни легче!","Використовуй предмети, щоб проходити рівні легше!"));
            else{fieldTrayLimit.SetCopy(ItemsToolTitle(fieldSelected));fieldTrayDescription.SetCopy(ToolDescription(fieldSelected));}
            bool usable=fieldSelected>=0&&Stock((TravelTool)fieldSelected)>0&&ToolEligible((TravelTool)fieldSelected)&&!toolSearchPending&&Interaction.CanPlay&&Interaction.AllowsTool((TravelTool)fieldSelected);
            fieldApplyButton.interactable=usable;var drawing=ReferenceButtonBody(fieldApplyButton).GetComponentInChildren<Image>();drawing.GetComponent<PrimaryButtonSkin>().Configure(CleanMenuArt.Get(usable?"GreenAction":"GrayAction"));
            fieldApplyCaption.SetCopy(fieldSelected<0?ReferenceCopy("Choose help","Выбери помощь","Обери допомогу"):ReferenceCopy("Apply","Применить","Застосувати"));
            PaintToolTray();
        }
        void SelectFieldTool(int item){if(!fieldTrayOpen||!fieldTrayGroup.interactable||toolSearchPending)return;fieldSelected=toolsItem=shopItem=item;RefreshToolTray();}
        void ApplySelectedFieldTool(){if(fieldSelected>=0)UseFieldTool(fieldSelected);}
        void OpenFieldToolsShop(){int selected=Mathf.Max(0,fieldSelected);SetToolTray(false,true);ShowShop();shopTab=0;shopItem=selected;DrawMenu();}
        void UseFieldTool(int item)
        {
            if(!fieldTrayOpen||!fieldTrayGroup.interactable)return;var tool=(TravelTool)item;
            if(!ToolEligible(tool)){fieldTrayDescription.SetCopy(ToolEligibilityText(tool));return;}
            toolsReturnScreen=GameScreen.Playing;toolsItem=shopItem=item;UseShopTool(tool,fieldToolIcons[item].rectTransform);
        }
    }
}
