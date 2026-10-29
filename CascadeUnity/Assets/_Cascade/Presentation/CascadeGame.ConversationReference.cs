using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        void BuildConversationReference()
        {
            LoadNavigationArt();conversationWorld=EnvironmentWorld;
            // This remains the same paused conversation, with its original callbacks.
            var canvas=conversationRoot.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=60;canvas.worldCamera=uiCamera;
            conversationRoot.gameObject.AddComponent<GraphicRaycaster>();Background(conversationRoot,Color.white,true);
            var backdrop=Rect("Reference shop environment",conversationRoot,Vector2.one*.5f,Vector2.one*.5f);
            backdrop.SetAsFirstSibling();var sky=backdrop.gameObject.AddComponent<Image>();sky.sprite=ReferenceShopArt("Backdrop");sky.raycastTarget=false;
            conversationStage=Rect("Conversation reference composition",conversationSafe,Vector2.one*.5f,Vector2.one*.5f);conversationStage.sizeDelta=new Vector2(720,1280);
            var fit=conversationStage.gameObject.AddComponent<ReferenceShopLayout>();fit.Initialize(conversationSafe,backdrop);fit.BindContinuousGround(1018,"ContinuousStoneGround");
            var skyLife=Rect("Living home sky",conversationStage,Vector2.zero,Vector2.one);skyLife.SetAsFirstSibling();
            var islands=new RectTransform[3];
            islands[0]=ReferencePicture("Drifting home island 0",skyLife,WorldIslandArt.Get(conversationWorld,0),-72,112,344,410).rectTransform;
            islands[1]=ReferencePicture("Drifting home island 1",skyLife,WorldIslandArt.Get(conversationWorld,1),382,12,224,246).rectTransform;
            islands[2]=ReferencePicture("Drifting home island 2",skyLife,WorldIslandArt.Get(conversationWorld,2),540,270,166,212).rectTransform;
            var foliageSurface=Rect("Conversation planted foreground",conversationRoot,Vector2.one*.5f,Vector2.one*.5f);
            var bough=ReferencePicture("Conversation rooted bough",foliageSurface,MapReferenceArt("Foliage"),-44,-28,329,292).rectTransform;bough.pivot=new Vector2(.07f,.94f);bough.anchoredPosition=new Vector2(-21,-45);
            conversationBust=ReferenceRect("Speaker portrait",conversationStage,130,330,460,420);
            conversationPortrait=ReferencePicture("Speaker portrait",conversationBust,null,0,0,460,420);conversationBust.gameObject.AddComponent<RectMask2D>();
            conversationPlate=ReferenceRect("Speech ribbon",conversationStage,28,712,664,330);PaintStoryPaper(conversationPlate,true);
            var left=ReferencePicture("Conversation grounded flowers left",foliageSurface,FailureReferenceArt.Sprig(conversationWorld),0,0,170,166);var right=ReferencePicture("Conversation grounded flowers right",foliageSurface,FailureReferenceArt.Sprig(conversationWorld),0,0,154,152);
            left.rectTransform.anchorMin=left.rectTransform.anchorMax=Vector2.zero;left.rectTransform.pivot=new Vector2(.32f,0);left.rectTransform.anchoredPosition=new Vector2(0,-18);
            right.rectTransform.anchorMin=right.rectTransform.anchorMax=Vector2.right;right.rectTransform.pivot=new Vector2(.68f,0);right.rectTransform.anchoredPosition=new Vector2(0,-18);right.rectTransform.localScale=new Vector3(-1,1,1);
            conversationName=ReferencePicture("Speaker name",conversationStage,ReferenceShopArt("HeaderBoard"),36,664,330,104,false).rectTransform;
            var nameFace=conversationName.GetComponent<Image>();nameFace.type=Image.Type.Sliced;nameFace.pixelsPerUnitMultiplier=4;conversationName.gameObject.AddComponent<AttachedPlants>().Initialize(this,conversationWorld,true);
            var nameArea=ReferenceRect("Conversation name reading inset",conversationName,29,26,272,52);
            conversationSpeaker=ReferenceText("conversation speaker",nameArea,UiCopy.Raw(""),0,0,272,52,32);conversationSpeaker.name="Speaker";conversationSpeaker.fontSizeMin=20;
            var nameFit=CentreContent(nameArea,conversationSpeaker.rectTransform);nameFit.Padding=4;nameFit.Refresh();
            conversationBody=ReferenceText("conversation line",conversationPlate,UiCopy.Raw(""),56,66,560,250,34,false,true);conversationBody.name="Conversation line";
            conversationBody.font=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceBody")??art.titleFont;conversationBody.fontStyle=FontStyles.Bold;conversationBody.alignment=TextAlignmentOptions.TopLeft;conversationBody.overflowMode=TextOverflowModes.Overflow;
            conversationCount=ReferenceText("conversation progress",conversationPlate,UiCopy.Raw(""),548,20,80,44,23);conversationCount.name="Conversation progress";conversationCount.enableAutoSizing=false;conversationCount.fontSize=conversationCount.fontSizeMin=conversationCount.fontSizeMax=23;conversationCount.alignment=TextAlignmentOptions.MidlineRight;
            conversationNext=ReferenceButton("Next line",conversationStage,399,1090,293,104,AdvanceConversation,"GreenAction");
            ReferenceButtonBody(conversationNext).gameObject.AddComponent<AttachedPlants>().Initialize(this,conversationWorld,true);
            var next=ReferenceText("conversation continue",ReferenceButtonBody(conversationNext),ReferenceCopy("Continue","Далее","Далі"),18,14,257,76,42,true);CentreContent((RectTransform)ReferenceButtonBody(conversationNext),next.rectTransform).Padding=14;
            conversationSkip=ReferenceButton("Not now",conversationStage,55,1092,236,112,SkipConversation);
            RegisterButtonDesign(conversationSkip,GameButtonRole.TertiaryLabel).PreserveSlot=true;
            var skip=ReferenceText("conversation skip",ReferenceButtonBody(conversationSkip),ReferenceCopy("Skip","Пропустить","Пропустити"),8,10,220,56,31,true);skip.fontStyle=FontStyles.Underline;skip.outlineColor=new Color(.012f,.035f,.06f,1);skip.outlineWidth=.18f;
            var skipInk=skip.fontMaterial;skipInk.EnableKeyword("UNDERLAY_ON");skipInk.SetColor(ShaderUtilities.ID_UnderlayColor,new Color(.012f,.035f,.06f,1));skipInk.SetFloat(ShaderUtilities.ID_UnderlayOffsetY,-.6f);skipInk.SetFloat(ShaderUtilities.ID_UnderlayDilate,.25f);skipInk.SetFloat(ShaderUtilities.ID_UnderlaySoftness,.10f);skip.UpdateMeshPadding();CentreContent((RectTransform)ReferenceButtonBody(conversationSkip),skip.rectTransform).Padding=14;
            var plants=new RectTransform[5];plants[0]=bough;plants[1]=left.rectTransform;plants[2]=right.rectTransform;
            for(int i=0;i<2;i++){
                var flower=ReferencePicture("Conversation paper attached flowers "+i,conversationPlate,FailureReferenceArt.Sprig(conversationWorld),0,0,74,76).rectTransform;
                flower.anchorMin=flower.anchorMax=i==0?Vector2.up:Vector2.right;flower.pivot=new Vector2(.5f,.1f);flower.anchoredPosition=i==0?new Vector2(18,-24):new Vector2(-16,8);if(i==1)flower.localScale=new Vector3(-1,1,1);plants[i+3]=flower;
            }
            var environment=conversationRoot.gameObject.AddComponent<MenuWorldEnvironment>();environment.InitializeConversation(this,conversationRoot,conversationWorld);
            conversationLayout=conversationStage.gameObject.AddComponent<ConversationReferenceLayout>();conversationLayout.Initialize(this,environment,islands,plants,backdrop,foliageSurface);
        }
    }
}
