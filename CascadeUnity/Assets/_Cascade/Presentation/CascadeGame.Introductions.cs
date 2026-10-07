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
        static readonly string[] MechanicCopy={
            "Put its pictured piece above the bomb. Stop it before the count reaches zero.",
            "Put its pictured piece above the key. The branch opens and you can swap there.",
            "Put its pictured piece above the toolbox. It repairs this board once.",
            "Match the spool picture. You earn one swap past one or two pieces.",
            "Match the stamp picture, then finish the level to keep your bonus.",
            "Match the flask picture. The bomb waits two swaps. The wood still wears down.",
            "You can swap across this bridge once. Then that path closes.",
            "These pairs are tied together. Swap one pair and the other swaps too.",
            "A swap at the cracked spot uses twice as much wood. Try another path.",
            "The branch blocks this swap. Use another pair or go down.",
            "This board slides the whole row when it lands. Watch the arrow before going down.",
            "This board turns the row around on landing. The last piece becomes the first.",
            "Wind moves the whole row one place on landing. Watch this example before going down.",
            "A gust rocks the boards and pieces. They settle back into place. Your order stays safe.",
            "Follow the hand for each next move. The scroll lasts until this board breaks.",
            "After two free undos, this spool undoes one more move on the same board.",
            "The hourglass brings back the last broken board in this stage. Later moves are undone.",
            "The lantern makes one safe move for you. It uses wood just like your own move. Once per stage.",
            "See where the pieces will land if you go down now. Nothing moves until you choose Descend.",
            "After your next swap, wind shifts the row. It can crack the board. Plan your move, or descend before the gust."
        };
        void ResetMechanicDiscovery()
        {
            CloseMechanicPopup();firstMechanics.Clear();needsMechanicScan=Interaction==null||Interaction.Attempt.Current.Status==PuzzleStatus.Playing;mechanicMountFrame=Time.frameCount;
        }
        void TryFirstMechanic()
        {
            if(!flowReady||profile==null||CurrentScreen!=GameScreen.Playing||MechanicPopupOpen||!IntroductionAnimationAllowed
                ||!PreparationReady||Interaction.IsPresenting||IsDescending||IsPlanningDescent||IsPlanningDirection||Time.frameCount<=mechanicMountFrame||saveBlocked||HasSaveError)return;
            if(needsMechanicScan){
                firstMechanics.Clear();
                foreach(var lesson in MechanicLessons.Discover(Interaction.Attempt.Level,Interaction.Attempt.Current.ActiveFloor,
                    Enumerable.Range(0,Interaction.Attempt.Level.Floors.Count).Where(IsWindFloor)))
                    if(!profile.SeenMechanics.Contains(lesson))firstMechanics.Enqueue(lesson);
                needsMechanicScan=false;
                Interaction.SetPreparing(descentPlanFailed||!PreparationReady||IsDescending||IsPlanningDescent);RenderControls();
            }
            while(firstMechanics.Count>0){var next=firstMechanics.Dequeue();if(!profile.SeenMechanics.Contains(next)){OpenMechanicPopup(next);break;}}
        }
        public bool RequireGustIntroduction()
        {
            if(MechanicPopupOpen)return true;
            if(!flowReady||CurrentScreen!=GameScreen.Playing||profile.SeenMechanics.Contains(MechanicLesson.Gust))return false;
            OpenMechanicPopup(MechanicLesson.Gust);return true;
        }
        bool IntroduceTool(TravelTool tool)
        {
            var lesson=MechanicLessons.Tool(tool);if(profile.SeenMechanics.Contains(lesson))return false;
            OpenMechanicPopup(lesson,()=>UseShopTool(tool),"Use it");return true;
        }
        bool IntroduceDescentEntry()
        {
            var upcoming=descent.Level(DescentPublishedSource(),descent.StageIndex+1);
            var arrival=upcoming.Floors[0].EntryTransform;if(arrival==EntryTransform.None)return false;
            var lesson=arrival==EntryTransform.MirrorNext?MechanicLesson.Mirror:MechanicLesson.Rollers;
            if(profile.SeenMechanics.Contains(lesson))return false;
            OpenMechanicPopup(lesson,GoLower,"Continue",arrival==EntryTransform.ShiftRightOne);return true;
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
            var panel=Rect("Explanation cloth",mechanicSafe,new Vector2(.04f,.10f),new Vector2(.96f,.90f));
            if(!ReducedMotion){
                mechanicOpacity.alpha=0;panel.localScale=Vector3.one*.97f;
                mechanicArrival=DOTween.Sequence().SetUpdate(true).SetTarget(mechanicOverlay);
                mechanicArrival.Append(DOTween.To(()=>mechanicOpacity.alpha,v=>mechanicOpacity.alpha=v,1,.20f).SetEase(Ease.OutCubic));
                mechanicArrival.Join(panel.DOScale(1,.32f).SetEase(Ease.OutQuint));
            }
            var face=Background(panel,Color.white,false);face.sprite=enamelControls[0];face.type=Image.Type.Sliced;
            var title=Text("New mechanic title",panel,new Vector2(.07f,.81f),new Vector2(.93f,.97f),26,MechanicTitles[(int)lesson]);title.font=art.titleFont;title.color=MenuPaper;
            title.enableAutoSizing=true;title.fontSizeMin=22*MenuUnit;title.fontSizeMax=28*MenuUnit;
            var example=Rect("Mechanic illustration",panel,new Vector2(.10f,.51f),new Vector2(.90f,.78f));
            int kind=lesson==MechanicLesson.Bomb?2:lesson==MechanicLesson.Key?3:lesson==MechanicLesson.Repair?4:lesson==MechanicLesson.Ribbon?9:lesson==MechanicLesson.Stamp?8:lesson==MechanicLesson.BombHourglass?10:lesson==MechanicLesson.Bridge?7:lesson==MechanicLesson.LinkedPairs?6:-1;
            if(kind>=0){
                var fixture=Catalog.PrimaryLevels.Concat(Catalog.BonusLevels).Select(c=>c.Definition).First(l=>FloorHasLesson(l.Floors[0],lesson));
                LiveDeviceDemo(example,fixture,kind,true);
            }else DrawMechanicIllustration(example,lesson);
            var copy=Text("New mechanic explanation",panel,new Vector2(.08f,.22f),new Vector2(.92f,.45f),20,MechanicCopy[(int)lesson]);
            copy.color=MenuPaper;copy.enableAutoSizing=true;copy.fontSizeMin=(Preferences.LargeText?20:18)*MenuUnit;copy.fontSizeMax=22*MenuUnit*(Preferences.LargeText?1.12f:1);
            var button=Button(string.IsNullOrEmpty(acknowledgement.Key)?(UiCopy)"Got it":acknowledgement,panel,new Vector2(.10f,.05f),new Vector2(.90f,.19f),AcknowledgeMechanic);
            button.name="Acknowledge mechanic";button.GetComponent<Image>().sprite=enamelControls[1];button.GetComponent<Image>().type=Image.Type.Sliced;button.GetComponent<Image>().color=Color.white;
            var label=button.GetComponentInChildren<TextMeshProUGUI>();label.fontSizeMin=20*MenuUnit;label.fontSizeMax=24*MenuUnit;label.color=MenuInk;RenderControls();
        }
        static bool FloorHasLesson(FloorDefinition f,MechanicLesson lesson)=>lesson==MechanicLesson.Bomb?f.Bomb!=null:lesson==MechanicLesson.Key?f.Key!=null:lesson==MechanicLesson.Repair?f.Repair!=null:lesson==MechanicLesson.Ribbon?f.Powerup?.Kind==PowerupKind.Ribbon:lesson==MechanicLesson.Stamp?f.OptionalStamp!=null:lesson==MechanicLesson.BombHourglass?f.Powerup?.Kind==PowerupKind.Hourglass:lesson==MechanicLesson.Bridge?f.OneUseBridgeEdge.HasValue:f.LinkedPair.HasValue;
        void DrawMechanicIllustration(RectTransform root,MechanicLesson lesson)
        {
            if(lesson>=MechanicLesson.Wayfinder&&lesson<=MechanicLesson.LandingLens){Picture("Travel tool",root,Vector2.zero,Vector2.one,ToolArt((int)lesson-(int)MechanicLesson.Wayfinder),true);return;}
            if(lesson==MechanicLesson.Gust||lesson==MechanicLesson.LiveGust){var flag=Picture("Gust windsock",root,Vector2.zero,Vector2.one,PlatformProp(0),true);root.gameObject.AddComponent<DeviceDemonstration>().Initialize(this,(time,bad)=>flag.rectTransform.localRotation=Quaternion.Euler(0,0,ReducedMotion?0:Mathf.Sin(time*1.3f)*9),true);return;}
            if(lesson==MechanicLesson.LandingWind||lesson==MechanicLesson.Rollers||lesson==MechanicLesson.Mirror){
                var actual=lesson==MechanicLesson.Mirror?EntryTransform.MirrorNext:mechanicWindRight.HasValue?EntryTransform.None:lesson==MechanicLesson.LandingWind?
                    Interaction.Attempt.Level.Floors[Enumerable.Range(0,Interaction.Attempt.Level.Floors.Count).First(IsWindFloor)].EntryTransform:
                    Interaction.Attempt.Level.Floors.First(f=>lesson==MechanicLesson.Mirror?f.EntryTransform==EntryTransform.MirrorNext:f.EntryTransform==EntryTransform.ShiftLeftOne||f.EntryTransform==EntryTransform.ShiftRightOne).EntryTransform;
                var fixture=WindGuide(mechanicWindRight??actual==EntryTransform.ShiftRightOne);
                if(lesson==MechanicLesson.Mirror)fixture=new LevelDefinition("mirror-guide","mirror-guide","1","2.0","mirror-guide",fixture.InitialSymbolIds,fixture.InitialSymbolIds.Reverse(),fixture.TargetSegments,
                    new[]{fixture.Floors[0],new FloorDefinition(1,new[]{true,true,true},entryTransform:EntryTransform.MirrorNext)});
                LiveDeviceDemo(root,fixture,1,true);return;
            }
            var plank=Rect("Lesson wood",root,new Vector2(0,.16f),new Vector2(1,.38f)).gameObject.AddComponent<WoodBoardGraphic>();plank.SetDamage(lesson==MechanicLesson.Fragile?2:0);
            for(int i=0;i<3;i++){var cell=Rect("Example piece",root,new Vector2(i/3f+.04f,.38f),new Vector2((i+1)/3f-.04f,.96f));cell.gameObject.AddComponent<ShapeFaceGraphic>().SetShape(art.shapes[i],new[]{new Color(1,.75f,.21f),new Color(.18f,.82f,.65f),new Color(1,.48f,.43f)}[i],true);}
            if(lesson==MechanicLesson.ClosedPath){
                if(introBranch==null){var texture=Resources.Load<Texture2D>("CascadeArt/Woodland/BranchStops");introBranch=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);}
                Picture("Branch blocks the pair",root,new Vector2(.29f,.2f),new Vector2(.40f,.65f),introBranch,true);
            }
        }
        public void AcknowledgeMechanic()
        {
            if(!MechanicPopupOpen||!IntroductionAnimationAllowed||saveBlocked)return;
            var lesson=CurrentMechanic.Value;bool added=!profile.SeenMechanics.Contains(lesson);if(added)profile.SeenMechanics.Add(lesson);
            PersistProgress();if(HasSaveError){if(added)profile.SeenMechanics.Remove(lesson);return;}
            var action=afterMechanic;CloseMechanicPopup();if(lesson==MechanicLesson.Gust)machine.DelayAmbientGust(.8f);
            feedback.Sound(ToyCue.Select);action?.Invoke();TryFirstMechanic();
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
