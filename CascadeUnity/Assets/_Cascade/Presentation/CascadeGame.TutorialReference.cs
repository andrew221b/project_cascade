using System;
using Cascade.Application;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        TutorialReferenceLayout tutorialReferenceLayout;int tutorialControlPage;bool changingTutorialPage;
        int TutorialPageCount=>lessonReplay?replayLessons.Count:CurrentMechanic==MechanicLesson.Swap?2:1;
        int TutorialPageIndex=>lessonReplay?replayLessonIndex:tutorialControlPage;
        void SelectTutorialPage(int index)
        {
            if(!MechanicPopupOpen||!IntroductionAnimationAllowed||saveBlocked||HasSaveError||index<0||index>=TutorialPageCount||index==TutorialPageIndex)return;
            var lesson=CurrentMechanic.Value;var action=afterMechanic;var wind=mechanicWindRight;bool replay=lessonReplay;
            if(replay){MarkLessonSeen(lesson);if(HasSaveError)return;}
            changingTutorialPage=true;
            try{
                CloseMechanicPopup();
                if(replay){replayLessonIndex=index;lessonReplay=true;lesson=replayLessons[index];}else tutorialControlPage=index;
                OpenMechanicPopup(lesson,action,windRight:wind);
            }finally{changingTutorialPage=false;}
        }
        void BuildReferenceTutorial(MechanicLesson lesson,Action requestedAction,bool? windRight)
        {
            var blurred=TutorialBackdrop.Capture(uiCamera,Preferences.EffectQuality);
            mechanicOverlay=Rect("First mechanic explanation",(RectTransform)safeRoot.parent,Vector2.zero,Vector2.one);
            var canvas=mechanicOverlay.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=40;canvas.worldCamera=uiCamera;mechanicOverlay.gameObject.AddComponent<GraphicRaycaster>();
            if(blurred!=null){var background=Rect("Tutorial frozen blurred game",mechanicOverlay,Vector2.zero,Vector2.one);background.gameObject.AddComponent<TutorialBackdrop>().Initialize(blurred);}
            var dimmer=Rect("Tutorial blocking dimmer",mechanicOverlay,Vector2.zero,Vector2.one);Background(dimmer,new Color(.004f,.007f,.004f,QualitySettings.activeColorSpace==ColorSpace.Linear?.84f:.60f),true);
            mechanicOpacity=mechanicOverlay.gameObject.AddComponent<CanvasGroup>();mechanicOpacity.alpha=1;
            mechanicSafe=Rect("Explanation safe area",mechanicOverlay,safeRoot.anchorMin,safeRoot.anchorMax);FitMechanicSafeArea();
            var stage=Rect("Tutorial reference composition",mechanicSafe,Vector2.one*.5f,Vector2.one*.5f);stage.sizeDelta=new Vector2(720,1280);
            tutorialReferenceLayout=stage.gameObject.AddComponent<TutorialReferenceLayout>();tutorialReferenceLayout.Initialize(mechanicSafe);
            var panel=ReferencePicture("Tutorial cream and flower frame",stage,TutorialReferenceArt.Panel,45,176,632,888,false);panel.type=Image.Type.Sliced;panel.pixelsPerUnitMultiplier=2;panel.color=new Color(1,.86f,.73f);
            var title=ReferencePicture("Tutorial wooden title board",stage,ReferenceShopArt("HeaderBoard"),130,184,452,130,false);
            var titleArea=ReferenceRect("Tutorial title readable inset",title.transform,61,44,330,64);
            var heading=ReferenceText("tutorial title",titleArea,ReferenceCopy("How to play?","Как играть?","Як грати?"),0,0,330,64,44);heading.name="New mechanic title";
            var headingFit=CentreContent(titleArea,heading.rectTransform);headingFit.Padding=0;headingFit.Refresh();title.gameObject.AddComponent<AttachedPlants>().Initialize(this,EnvironmentWorld,true);
            var close=ReferenceButton("Close lesson film",stage,578,205,92,92,DismissMechanic);
            TutorialActionSkin(close,"RedAction",92,92);var closeBody=(RectTransform)ReferenceButtonBody(close);
            var mark=ReferenceRect("Tutorial close icon area",closeBody,14,14,64,64);var cross=Rect("Close lesson mark",mark,Vector2.one*.5f,Vector2.one*.5f).gameObject.AddComponent<InstructionCloseMark>();cross.rectTransform.sizeDelta=Vector2.one*160;cross.color=MenuPaper;cross.raycastTarget=false;CentreContent(closeBody,mark);
            tutorialReferenceLayout.BindTouch((RectTransform)close.transform,new Vector2(92,92));
            UiCopy copy=lesson==MechanicLesson.Undo?ReferenceCopy(
                "Undo restores the pieces and board to the state before your last move. You have one free undo per level.",
                "Отмена хода возвращает фигуры и поле в состояние перед твоим последним ходом. У тебя есть одна бесплатная отмена на уровень.",
                "Скасування ходу повертає фігури й поле до стану перед твоїм останнім ходом. У тебе є одне безкоштовне скасування на рівень."):MechanicExplanation(lesson);
            if(lesson==MechanicLesson.Swap)copy=tutorialControlPage==0?ReferenceCopy("Drag one piece onto its neighbour. Every exchange wears down the board.","Перетащи фигуру на соседнюю. Каждый обмен расходует прочность дощечки.","Перетягни фігуру на сусідню. Кожен обмін витрачає міцність дощечки."):ReferenceCopy("Tap one piece, then its neighbour to exchange them. The result is the same as dragging.","Нажми на фигуру, затем на соседнюю, чтобы поменять их местами. Результат тот же, что при перетаскивании.","Натисни на фігуру, потім на сусідню, щоб поміняти їх місцями. Результат такий самий, як під час перетягування.");
            var explanation=ReferenceText("tutorial explanation",stage,copy,133,335,454,128,26,false,true);explanation.name="What this mechanic changes";explanation.font=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceBody")??art.titleFont;explanation.fontStyle=FontStyles.Bold;explanation.fontSizeMin=21;explanation.overflowMode=TextOverflowModes.Overflow;
            var example=ReferenceRect("Mechanic illustration",stage,108,492,508,370);BuildLessonFilm(example,lesson,windRight,lesson==MechanicLesson.Swap?tutorialControlPage==1:(bool?)null,true);
            int pages=TutorialPageCount;
            if(pages>1){
                var swipe=ReferenceRect("Tutorial page swipe area",stage,108,326,508,540);Background(swipe,Color.white,true);swipe.gameObject.AddComponent<Mask>().showMaskGraphic=false;
                var moving=Rect("Tutorial page contents",swipe,Vector2.zero,Vector2.one);explanation.rectTransform.SetParent(moving,true);example.SetParent(moving,true);
                swipe.gameObject.AddComponent<MenuSwipe>().Initialize(this,direction=>SelectTutorialPage(TutorialPageIndex+direction),moving,direction=>TutorialPageIndex+direction>=0&&TutorialPageIndex+direction<TutorialPageCount);
                var dots=ReferenceRect("Tutorial page control",stage,260,852,200,44);dots.pivot=Vector2.one*.5f;dots.anchoredPosition=new Vector2(360,-874);
                dots.gameObject.AddComponent<TutorialPageControl>().Initialize(pages,TutorialPageIndex,index=>{if(index!=TutorialPageIndex){feedback.Sound(ToyCue.Select);feedback.Haptic(.045f);SelectTutorialPage(index);}});tutorialReferenceLayout.BindTouch(dots,new Vector2(200,44));
            }
            float buttonY=pages>1?909:872;
            var apply=ReferenceButton("Acknowledge mechanic",stage,181,buttonY,359,88,AcknowledgeMechanic);TutorialActionSkin(apply,"TealAction",359,88);
            var label=ReferenceText("tutorial action",ReferenceButtonBody(apply),requestedAction!=null?(UiCopy)"Use this tool":"Try it",20,12,319,64,36,true);CentreContent((RectTransform)ReferenceButtonBody(apply),label.rectTransform);
            tutorialReferenceLayout.BindTouch((RectTransform)apply.transform,new Vector2(359,88));
            if(!ReducedMotion){
                mechanicOpacity.alpha=0;tutorialReferenceLayout.Entrance=.97f;tutorialReferenceLayout.Fit();
                mechanicArrival=DOTween.Sequence().SetUpdate(true).SetTarget(mechanicOverlay);mechanicArrival.Append(DOTween.To(()=>mechanicOpacity.alpha,v=>mechanicOpacity.alpha=v,1,.22f).SetEase(Ease.OutCubic));
                // Layout owns the reference scale; animate a separate content multiplier.
                var entrance=stage.gameObject.AddComponent<CanvasGroup>();entrance.alpha=.9f;mechanicArrival.Join(entrance.DOFade(1,.30f));mechanicArrival.Join(DOTween.To(()=>tutorialReferenceLayout==null?1:tutorialReferenceLayout.Entrance,value=>{if(tutorialReferenceLayout!=null){tutorialReferenceLayout.Entrance=value;tutorialReferenceLayout.Fit();}},1,.34f).SetEase(Ease.OutQuint));
            }
        }
        void TutorialActionSkin(Button button,string name,float width,float height)
        {
            var drawing=ReferencePicture("Tutorial shared action face",ReferenceButtonBody(button),ReferenceShopArt(name),0,0,width,height,false);drawing.type=Image.Type.Sliced;
            if(name=="TealAction")drawing.color=new Color(1,1,.76f);
            drawing.pixelsPerUnitMultiplier=name=="TealAction"?drawing.sprite.rect.height/height:2.8f;
            button.targetGraphic=drawing;button.transition=Selectable.Transition.ColorTint;var colors=button.colors;colors.normalColor=colors.highlightedColor=Color.white;colors.pressedColor=new Color(.93f,.97f,.90f);colors.disabledColor=Color.white;button.colors=colors;
        }
    }
}
