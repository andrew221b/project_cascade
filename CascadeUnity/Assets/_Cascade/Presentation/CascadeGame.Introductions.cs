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
        readonly HashSet<(MechanicLesson Lesson,int Floor)> deferredLessons=new HashSet<(MechanicLesson,int)>();
        bool needsMechanicScan,mechanicWasPaused;
        int mechanicMountFrame;
        RectTransform mechanicOverlay,mechanicSafe;
        CanvasGroup mechanicOpacity;
        Sequence mechanicArrival;
        Action afterMechanic;
        bool? mechanicWindRight;
        Sprite introBranch;
        const string TwoWaysCoach="controls-two-ways-v1",UndoCoach="controls-undo-v1",ClosedPathCoach="closed-path-v2";
        bool IndependentTriad=>descent?.Version==6&&descent.StageIndex>0;
        string TriadCoach(MechanicLesson lesson)=>descent?.Version==6&&descent.StageIndex==0&&descent.Stages[0].Triad.Lesson==(int)lesson
            ?"triad-guide-v6-"+(int)lesson:null;
        // An upgraded save can lack the newer gesture-coach marker. A cleared
        // opening level proves basic control; do not interrupt a later puzzle
        // with the opening rehearsal. Replaying C01 can still teach both ways.
        static bool IsSurpriseWindLesson(MechanicLesson lesson)
            =>lesson==MechanicLesson.Gust||lesson==MechanicLesson.LiveGust||lesson==MechanicLesson.LandingWind;
        bool NeedsControlLesson(MechanicLesson lesson)=>!IsSurpriseWindLesson(lesson)&&!LessonWasSeen(lesson)
            &&!(lesson==MechanicLesson.Swap&&node!="C01"&&profile.Clears.Any(c=>c.CampaignNodeId=="C01"&&c.BestStars>0));
        public bool MechanicPopupOpen=>mechanicOverlay!=null;
        public MechanicLesson? CurrentMechanic { get; private set; }
        public IReadOnlyList<MechanicLesson> SeenMechanics=>profile.SeenMechanics.AsReadOnly();
        internal bool IntroductionAnimationAllowed=>applicationFocused&&!applicationPaused&&!nativeAdSuspended;
        static readonly string[] MechanicTitles={"Bomb","Circular saw","Repair","Ribbon spool","Stamp","Frost flask","One-use bridge","Linked pairs","Fragile edge","Closed path","Entry rollers","Mirror on entry","Wind on landing","A gust of wind","Wayfinder scroll","Undo spool","Return hourglass","Signal lantern","Landing lens","Strong gust"};
        void ResetMechanicDiscovery()
        {
            CloseMechanicPopup();ClearLessonPractice();rehearsedLessons.Clear();practiceFloorOverride=null;firstMechanics.Clear();needsMechanicScan=Interaction==null||Interaction.Attempt.Current.Status==PuzzleStatus.Playing;mechanicMountFrame=Time.frameCount;
        }
        void TryFirstMechanic()
        {
            if(!Preferences.Tutorials){
                needsMechanicScan=false;firstMechanics.Clear();
                if(MechanicPopupOpen)CloseMechanicPopup();
                if(PendingLesson.HasValue)ClearLessonPractice();
                bool preparing=descentPlanFailed||!PreparationReady||IsDescending||IsPlanningDescent||IsPlanningDirection;
                if(Interaction!=null&&Interaction.IsPreparing!=preparing)Interaction.SetPreparing(preparing);
                return;
            }
            if(!flowReady||profile==null||CurrentScreen!=GameScreen.Playing||MechanicPopupOpen||!IntroductionAnimationAllowed
                ||PendingLesson.HasValue||IsHoldingToken||!PreparationReady||Interaction.IsPresenting||IsDescending||IsPlanningDescent||IsPlanningDirection||Time.frameCount<=mechanicMountFrame||saveBlocked||HasSaveError)return;
            if(Interaction.Attempt.CanUndo&&NeedsControlLesson(MechanicLesson.Undo))needsMechanicScan=true;
            if(needsMechanicScan){
                firstMechanics.Clear();
                var visible=MechanicLessons.DiscoverVisible(Interaction.Attempt.Level,Interaction.Attempt.Current,IsWindFloor(Interaction.Attempt.Current.ActiveFloor+1));
                if(CurrentScreen==GameScreen.Playing&&NeedsControlLesson(MechanicLesson.Swap))firstMechanics.Enqueue(MechanicLesson.Swap);
                if(Interaction.Attempt.CanUndo&&NeedsControlLesson(MechanicLesson.Undo))firstMechanics.Enqueue(MechanicLesson.Undo);
                foreach(var lesson in visible)
                    if(!IsSurpriseWindLesson(lesson)&&NeedsControlLesson(lesson)
                        &&!deferredLessons.Contains((lesson,Interaction.Attempt.Current.ActiveFloor)))firstMechanics.Enqueue(lesson);
                needsMechanicScan=false;
                Interaction.SetPreparing(descentPlanFailed||!PreparationReady||IsDescending||IsPlanningDescent);RenderControls();
            }
            while(firstMechanics.Count>0){var next=firstMechanics.Dequeue();if(NeedsControlLesson(next)){OpenMechanicPopup(next);break;}}
        }
        // Retained for older QA callers. Ambient scenery never owns a lesson.
        public bool RequireGustIntroduction()=>false;
        bool IntroduceTool(TravelTool tool,Action use)
        {
            if(!Preferences.Tutorials)return false;
            var lesson=MechanicLessons.Tool(tool);if(LessonWasSeen(lesson)||rehearsedLessons.ContainsKey(lesson))return false;
            OpenMechanicPopup(lesson,use);return true;
        }
        bool IntroduceDescentEntry()
        {
            if(!Preferences.Tutorials)return false;
            var upcoming=descent.Level(DescentPublishedSource(),descent.StageIndex+1);
            var arrival=upcoming.Floors[0].EntryTransform;if(arrival==EntryTransform.None)return false;
            var lesson=arrival==EntryTransform.MirrorNext?MechanicLesson.Mirror:MechanicLesson.Rollers;
            if(LessonWasSeen(lesson)||rehearsedLessons.ContainsKey(lesson))return false;
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
            // The enamel art has a translucent centre. Give the demonstration an
            // opaque reading field so the live row and its HUD cannot show through it.
            Background(panel,new Color(.065f,.215f,.235f,1),false);
            var rim=Rect("Carved lesson frame",panel,Vector2.zero,Vector2.one);
            var face=Background(rim,Color.white,false);face.sprite=enamelControls[0];face.type=Image.Type.Sliced;
            var title=Text("New mechanic title",panel,new Vector2(.07f,.86f),new Vector2(.77f,.97f),26,LessonTitle(lesson));
            StyleCelebrationType(title,CelebrationType.EnamelLabel);title.alignment=TextAlignmentOptions.MidlineLeft;
            title.enableAutoSizing=true;title.fontSizeMin=18*MenuUnit;title.fontSizeMax=26*MenuUnit;
            var close=Button(UiCopy.Raw(""),panel,new Vector2(.87f,.91f),new Vector2(.87f,.91f),DismissMechanic);
            close.name="Close lesson film";((RectTransform)close.transform).sizeDelta=Vector2.one*48*MenuUnit;
            close.GetComponent<Image>().sprite=enamelControls[0];close.GetComponent<Image>().color=Color.white;
            var cross=Rect("Close lesson mark",close.transform,new Vector2(.20f,.20f),new Vector2(.80f,.80f)).gameObject.AddComponent<InstructionCloseMark>();cross.color=MenuPaper;cross.raycastTarget=false;
            var explanation=Text("What this mechanic changes",panel,new Vector2(.07f,.715f),new Vector2(.93f,.855f),18,MechanicExplanation(lesson));
            explanation.color=MenuPaper;explanation.margin=new Vector4(4,3,4,3);explanation.alignment=TextAlignmentOptions.Center;
            explanation.enableAutoSizing=true;explanation.fontSizeMin=15*MenuUnit;explanation.fontSizeMax=18*MenuUnit;
            var example=Rect("Mechanic illustration",panel,new Vector2(.06f,.17f),new Vector2(.94f,(lessonReplay&&replayLessons.Count>1)? .63f : .69f));
            BuildLessonFilm(example,lesson,windRight);
            if(lessonReplay){
                var group=example.gameObject.AddComponent<CanvasGroup>();group.interactable=group.blocksRaycasts=false;
                foreach(var hand in example.GetComponentsInChildren<Image>(true))if(hand.name=="Film tutorial hand above pieces")
                    hand.enabled=false;
                if(replayLessons.Count>1){
                    var next=Button(UiCopy.Raw(""),panel,new Vector2(.87f,.69f),new Vector2(.87f,.69f),NextReplayLesson);
                    var nextRect=(RectTransform)next.transform;nextRect.sizeDelta=Vector2.one*46*MenuUnit;
                    next.GetComponent<Image>().sprite=enamelControls[0];next.GetComponent<Image>().type=Image.Type.Sliced;next.GetComponent<Image>().color=Color.white;
                    var arrow=Rect("Next lesson arrow",next.transform,new Vector2(.17f,.17f),new Vector2(.83f,.83f)).gameObject.AddComponent<ChevronGraphic>();
                    arrow.PointsRight=true;arrow.Weight=1;arrow.color=MenuPaper;arrow.raycastTarget=false;next.targetGraphic=arrow;
                    explanation.rectTransform.anchorMin=new Vector2(.07f,.75f);
                }
            }
            var button=Button(lessonReplay?(UiCopy)"Back to game":requestedAction!=null?(UiCopy)"Use this tool":"Try it",panel,new Vector2(.18f,.035f),new Vector2(.82f,.035f),AcknowledgeMechanic);
            button.name="Acknowledge mechanic";((RectTransform)button.transform).pivot=new Vector2(.5f,0);((RectTransform)button.transform).sizeDelta=new Vector2(0,48*MenuUnit);
            button.GetComponent<Image>().sprite=enamelControls[0];button.GetComponent<Image>().type=Image.Type.Sliced;button.GetComponent<Image>().color=Color.white;
            var label=button.GetComponentInChildren<TextMeshProUGUI>();label.fontSizeMin=18*MenuUnit;label.fontSizeMax=22*MenuUnit;label.color=MenuPaper;RenderControls();
        }
        UiCopy MechanicExplanation(MechanicLesson lesson)
        {
            switch(lesson){
                case MechanicLesson.Swap:return "Drag one piece onto another, or tap both. Every exchange wears down the board.";
                case MechanicLesson.Repair:return "Put the pictured piece above the toolbox. It restores the shown durability for more exchanges.";
                case MechanicLesson.Key:return "Put the pictured piece above the saw. It cuts the branch so you can exchange across that gap.";
                case MechanicLesson.Bomb:return "Put the pictured piece above the bomb before its counter runs out. A blast removes board durability.";
                case MechanicLesson.Ribbon:case MechanicLesson.FarSwap:return "Collect the ribbon with the pictured piece. Then exchange two pieces up to three places apart once.";
                case MechanicLesson.BombHourglass:return "Collect the frost flask. The bomb waits for two exchanges, but the board still wears down.";
                case MechanicLesson.Stamp:return "Put the pictured piece above the stamp for 100 bonus points. The receiver still needs the exact row.";
                case MechanicLesson.Bridge:return "This link works once. Plan the row before crossing it: it closes after the exchange.";
                case MechanicLesson.LinkedPairs:return "Exchange one marked pair and both marked pairs move together. One move changes four positions.";
                case MechanicLesson.Fragile:return "An exchange across a cracked link costs two durability instead of one.";
                case MechanicLesson.ClosedPath:return "Pieces cannot cross a closed link. Go down to a board with the links you need.";
                case MechanicLesson.Mirror:return "When the row lands on this board, its order reverses. Prepare the row before going down.";
                case MechanicLesson.Rollers:case MechanicLesson.LandingWind:return "On landing, every piece moves one place in the arrow direction. The end piece wraps around.";
                case MechanicLesson.Gust:case MechanicLesson.LiveGust:return "After the shown number of exchanges, wind shifts the whole row one place. Plan for the shift.";
                case MechanicLesson.Undo:return "Undo returns the pieces and board to before your last move. You have two free returns.";
                case MechanicLesson.Descent:return "Go down early to use the next board. The row stays with you.";
                case MechanicLesson.Hint:return "The bulb shows one move; you make it. The counter is for this stage. When empty, an optional video adds one hint.";
                case MechanicLesson.Wayfinder:return "The scroll guides you through this board. Follow the hand; it never moves pieces for you.";
                case MechanicLesson.LandingLens:return "The lens previews the row after the next drop, including the next board’s effect. It does not move your pieces.";
                case MechanicLesson.SignalLantern:return "The lantern makes one safe move for you. One item is consumed.";
                case MechanicLesson.UndoSpool:return "After both free undos, use a spool to undo one more move on this board.";
                case MechanicLesson.ReturnHourglass:return "Return to the board you just left, with its row and durability restored.";
                case MechanicLesson.RepairKit:return "The kit adds two durability to this board. It does not stop bombs or wind. One kit is consumed.";
                case MechanicLesson.Toolbag:return "Tap the bag to open or fold the tray. Tap an available tool to use one. At most two tools per stage; using one caps this level at two stars.";
                default:return "Watch what changes, then try it on your row.";
            }
        }
        static UiCopy LessonTitle(MechanicLesson lesson)=>lesson==MechanicLesson.RepairKit?(UiCopy)"Repair kit":lesson==MechanicLesson.Swap?(UiCopy)"Swap neighbours":lesson==MechanicLesson.Descent?(UiCopy)"Descend":lesson==MechanicLesson.Undo?(UiCopy)"Undo":lesson==MechanicLesson.Hint?(UiCopy)"Hint":lesson==MechanicLesson.FarSwap?(UiCopy)"One far swap ready":lesson==MechanicLesson.Toolbag?(UiCopy)"My tools":MechanicTitles[(int)lesson];
        public void AcknowledgeMechanic()
        {
            if(!MechanicPopupOpen||!IntroductionAnimationAllowed||saveBlocked)return;
            var lesson=CurrentMechanic.Value;var action=afterMechanic;bool replay=lessonReplay;CloseMechanicPopup();
            if(replay){MarkLessonSeen(lesson);return;}
            if(action!=null){MarkLessonSeen(lesson);if(!HasSaveError)action();return;}
            BeginLessonPractice(lesson);
            if(lesson==MechanicLesson.Gust)machine.DelayAmbientGust(.8f);
            feedback.Sound(ToyCue.Select);
        }
        void CloseMechanicPopup()
        {
            if(!MechanicPopupOpen)return;
            mechanicArrival?.Kill();mechanicArrival=null;mechanicOverlay.gameObject.SetActive(false);Destroy(mechanicOverlay.gameObject);mechanicOverlay=null;mechanicSafe=null;CurrentMechanic=null;afterMechanic=null;lessonReplay=false;
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
