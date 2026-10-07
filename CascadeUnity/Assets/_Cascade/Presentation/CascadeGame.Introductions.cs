using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly Queue<MechanicLesson> firstMechanics=new Queue<MechanicLesson>();
        bool needsMechanicScan,mechanicWasPaused;
        int mechanicMountFrame;
        RectTransform mechanicOverlay,mechanicSafe;
        CanvasGroup mechanicOpacity;
        Sequence mechanicArrival;
        Action afterMechanic;
        bool? mechanicWindRight;
        Sprite introBranch;
        public bool MechanicPopupOpen=>mechanicOverlay!=null;
        public MechanicLesson? CurrentMechanic { get; private set; }
        public IReadOnlyList<MechanicLesson> SeenMechanics=>profile.SeenMechanics.AsReadOnly();
        internal bool IntroductionAnimationAllowed=>applicationFocused&&!applicationPaused&&!nativeAdSuspended;
        static readonly string[] MechanicTitles={"Bomb","Key","Repair","Ribbon spool","Stamp","Frost flask","One-use bridge","Linked pairs","Fragile edge","Closed path","Entry rollers","Mirror on entry","Wind on landing","A gust of wind","Wayfinder scroll","Undo spool","Return hourglass","Signal lantern","Landing lens","Strong gust"};
        void ResetMechanicDiscovery()
        {
            CloseMechanicPopup();ClearLessonPractice();rehearsedLessons.Clear();practiceFloorOverride=null;firstMechanics.Clear();needsMechanicScan=Interaction==null||Interaction.Attempt.Current.Status==PuzzleStatus.Playing;mechanicMountFrame=Time.frameCount;
        }
        void TryFirstMechanic()
        {
            if(!flowReady||profile==null||CurrentScreen!=GameScreen.Playing||MechanicPopupOpen||!IntroductionAnimationAllowed
                ||PendingLesson.HasValue||!PreparationReady||Interaction.IsPresenting||IsDescending||IsPlanningDescent||IsPlanningDirection||Time.frameCount<=mechanicMountFrame||saveBlocked||HasSaveError)return;
            if(needsMechanicScan){
                firstMechanics.Clear();
                var visible=MechanicLessons.DiscoverVisible(Interaction.Attempt.Level,Interaction.Attempt.Current);
                if(!profile.SeenMechanics.Contains(MechanicLesson.Swap))firstMechanics.Enqueue(MechanicLesson.Swap);
                foreach(var lesson in visible)
                    if(lesson!=MechanicLesson.FarSwap&&!profile.SeenMechanics.Contains(lesson))firstMechanics.Enqueue(lesson);
                needsMechanicScan=false;
                Interaction.SetPreparing(descentPlanFailed||!PreparationReady||IsDescending||IsPlanningDescent);RenderControls();
            }
            while(firstMechanics.Count>0){var next=firstMechanics.Dequeue();if(!profile.SeenMechanics.Contains(next)){OpenMechanicPopup(next);break;}}
        }
        public bool RequireGustIntroduction()
        {
            if(MechanicPopupOpen||PendingLesson.HasValue&&PendingLesson!=MechanicLesson.Gust)return true;
            if(PendingLesson==MechanicLesson.Gust)return false;
            if(!flowReady||CurrentScreen!=GameScreen.Playing||profile.SeenMechanics.Contains(MechanicLesson.Gust))return false;
            OpenMechanicPopup(MechanicLesson.Gust);return true;
        }
        bool IntroduceTool(TravelTool tool)
        {
            var lesson=MechanicLessons.Tool(tool);if(profile.SeenMechanics.Contains(lesson)||rehearsedLessons.ContainsKey(lesson))return false;
            OpenMechanicPopup(lesson);return true;
        }
        bool IntroduceDescentEntry()
        {
            var upcoming=descent.Level(DescentPublishedSource(),descent.StageIndex+1);
            var arrival=upcoming.Floors[0].EntryTransform;if(arrival==EntryTransform.None)return false;
            var lesson=arrival==EntryTransform.MirrorNext?MechanicLesson.Mirror:MechanicLesson.Rollers;
            if(profile.SeenMechanics.Contains(lesson)||rehearsedLessons.ContainsKey(lesson))return false;
            OpenMechanicPopup(lesson,windRight:arrival==EntryTransform.ShiftRightOne);return true;
        }
        void OpenMechanicPopup(MechanicLesson lesson,Action requestedAction=null,UiCopy acknowledgement=default,bool? windRight=null)
        {
            if(MechanicPopupOpen)return;
            CurrentMechanic=lesson;afterMechanic=requestedAction;mechanicWasPaused=Interaction.IsPaused;mechanicWindRight=windRight;
            EndTokenHold();Interaction.CancelPointer();Interaction.SetPaused(true);machine.SetAmbientPaused(true);
            HideInstruction();coachRoot.gameObject.SetActive(false);tutorialHand.gameObject.SetActive(false);
            mechanicOverlay=Rect("First mechanic explanation",(RectTransform)safeRoot.parent,Vector2.zero,Vector2.one);
            var canvas=mechanicOverlay.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=40;
            mechanicOverlay.gameObject.AddComponent<GraphicRaycaster>();Background(mechanicOverlay,new Color(.035f,.12f,.13f,.73f),true);
            mechanicOpacity=mechanicOverlay.gameObject.AddComponent<CanvasGroup>();mechanicOpacity.alpha=1;
            mechanicSafe=Rect("Explanation safe area",mechanicOverlay,safeRoot.anchorMin,safeRoot.anchorMax);FitMechanicSafeArea();
            var panel=Rect("Explanation cloth",mechanicSafe,new Vector2(.035f,.12f),new Vector2(.965f,.90f));
            if(!ReducedMotion){
                mechanicOpacity.alpha=0;panel.localScale=Vector3.one*.97f;
                mechanicArrival=DOTween.Sequence().SetUpdate(true).SetTarget(mechanicOverlay);
                mechanicArrival.Append(DOTween.To(()=>mechanicOpacity.alpha,v=>mechanicOpacity.alpha=v,1,.22f).SetEase(Ease.OutCubic));
                mechanicArrival.Join(panel.DOScale(1,.34f).SetEase(Ease.OutQuint));
            }
            var face=Background(panel,Color.white,false);face.sprite=enamelControls[0];face.type=Image.Type.Sliced;
            var title=Text("New mechanic title",panel,new Vector2(.07f,.86f),new Vector2(.77f,.97f),26,LessonTitle(lesson));
            StyleCelebrationType(title,CelebrationType.EnamelLabel);title.alignment=TextAlignmentOptions.MidlineLeft;
            title.enableAutoSizing=true;title.fontSizeMin=18*MenuUnit;title.fontSizeMax=26*MenuUnit;
            var close=Button(UiCopy.Raw(""),panel,new Vector2(.87f,.91f),new Vector2(.87f,.91f),AcknowledgeMechanic);
            close.name="Close lesson film";((RectTransform)close.transform).sizeDelta=Vector2.one*48*MenuUnit;
            close.GetComponent<Image>().sprite=enamelControls[0];close.GetComponent<Image>().color=Color.white;
            var cross=Rect("Close lesson mark",close.transform,new Vector2(.20f,.20f),new Vector2(.80f,.80f)).gameObject.AddComponent<InstructionCloseMark>();cross.color=MenuPaper;cross.raycastTarget=false;
            var example=Rect("Mechanic illustration",panel,new Vector2(.06f,.17f),new Vector2(.94f,.82f));
            BuildLessonFilm(example,lesson,windRight);
            var button=Button("Try it",panel,new Vector2(.18f,.035f),new Vector2(.82f,.035f),AcknowledgeMechanic);
            button.name="Acknowledge mechanic";((RectTransform)button.transform).pivot=new Vector2(.5f,0);((RectTransform)button.transform).sizeDelta=new Vector2(0,48*MenuUnit);
            button.GetComponent<Image>().sprite=enamelControls[0];button.GetComponent<Image>().type=Image.Type.Sliced;button.GetComponent<Image>().color=Color.white;
            var label=button.GetComponentInChildren<TextMeshProUGUI>();label.fontSizeMin=18*MenuUnit;label.fontSizeMax=22*MenuUnit;label.color=MenuPaper;RenderControls();
        }
        static UiCopy LessonTitle(MechanicLesson lesson)=>lesson==MechanicLesson.Swap?(UiCopy)"Swap neighbours":lesson==MechanicLesson.Descent?(UiCopy)"Descend":lesson==MechanicLesson.Undo?(UiCopy)"Undo":lesson==MechanicLesson.Hint?(UiCopy)"Hint":lesson==MechanicLesson.FarSwap?(UiCopy)"One far swap ready":lesson==MechanicLesson.Toolbag?(UiCopy)"My tools":MechanicTitles[(int)lesson];
        public void AcknowledgeMechanic()
        {
            if(!MechanicPopupOpen||!IntroductionAnimationAllowed||saveBlocked)return;
            var lesson=CurrentMechanic.Value;CloseMechanicPopup();BeginLessonPractice(lesson);
            if(lesson==MechanicLesson.Gust)machine.DelayAmbientGust(.8f);
            feedback.Sound(ToyCue.Select);
        }
        void CloseMechanicPopup()
        {
            if(!MechanicPopupOpen)return;
            mechanicArrival?.Kill();mechanicArrival=null;mechanicOverlay.gameObject.SetActive(false);Destroy(mechanicOverlay.gameObject);mechanicOverlay=null;mechanicSafe=null;CurrentMechanic=null;afterMechanic=null;
            Interaction?.SetPaused(mechanicWasPaused);machine.SetAmbientPaused(CurrentScreen==GameScreen.Pause||CurrentScreen==GameScreen.SaveRecovery);if(CanPresent)RenderControls();
        }
        void FitMechanicSafeArea(){if(mechanicSafe==null)return;mechanicSafe.anchorMin=safeRoot.anchorMin;mechanicSafe.anchorMax=safeRoot.anchorMax;mechanicSafe.offsetMin=safeRoot.offsetMin;mechanicSafe.offsetMax=safeRoot.offsetMax;}
        void TickMechanicIntroduction()
        {
            TryFirstMechanic();if(!MechanicPopupOpen)return;FitMechanicSafeArea();
            if(mechanicArrival!=null&&mechanicArrival.IsActive()){
                if(ReducedMotion)mechanicArrival.Complete();
                else if(IntroductionAnimationAllowed)mechanicArrival.Play();else mechanicArrival.Pause();
            }
        }
    }
}
