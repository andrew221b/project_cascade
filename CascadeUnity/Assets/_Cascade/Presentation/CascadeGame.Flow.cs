using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cascade.Solver;
using System.Threading;
using System.Threading.Tasks;

namespace Cascade.Presentation
{
    public enum GameScreen { Home, Playing, Pause, Win, Fail, Settings, Devices, Lessons, Dialogue, WordLanguage, SaveRecovery, TravelBag, Journal, Collection, Workshop, Requests, Records, Contracts, Parcel, JourneyEnd, ReachChallenges, PowerupChallenges, Checkpoint, PlayChoice, JourneyMap, Prologue, Shop, NoAds, Supplies, CoinPacks, AccountGate }
    public sealed partial class CascadeGame
    {
        public GameScreen CurrentScreen { get; private set; } = GameScreen.Playing;
        public GamePreferences Preferences { get; private set; } = new GamePreferences();
        public PuzzleMode PreferredMode { get; private set; } = PuzzleMode.Shapes;
        public TutorialCoach Coach { get; private set; }
        public bool HasDeadEndWarning => knownDeadEnd;
        public bool IsTutorialHandVisible => tutorialHand!=null&&tutorialHand.gameObject.activeInHierarchy;
        RectTransform menuRoot, resultRoot, coachRoot;
        Image tutorialHand, reachBadge;
        Image menuWash;
        readonly List<RectTransform> menuActions=new List<RectTransform>();
        MenuTransition menuTransition,gameTransition;
        TextMeshProUGUI coachText;
        GameScreen returnScreen = GameScreen.Home;
        int chapterPage, deviceIndex, settingsPage;
        bool flowReady, firstGestureShown;
        float gestureTime, absentSince;
        readonly HashSet<string> sessionClears = new HashSet<string>();
        readonly Dictionary<string,int> sessionLosses = new Dictionary<string,int>();
        readonly Dictionary<TextMeshProUGUI,float> baseTextSizes = new Dictionary<TextMeshProUGUI,float>();
        Task<HintOffer> deadEndTask;
        CancellationTokenSource deadEndCancellation;
        bool knownDeadEnd;
        static readonly string[] DeviceTitles = { "Durability", "Entry rollers", "Bomb", "Circular saw", "Repair", "Fragile edge", "Linked pairs", "One-use bridge", "Stamp", "Ribbon spool", "Frost flask" };
        static readonly string[] DeviceNodes = { "C03", "C09", "C13", "C17", "C18", "C21", "C25", "C33", "C35", "B01", "B04" };
        static readonly string[] DeviceCopy = {
            "Each swap uses a move. At zero, the wood breaks. Descend leaves early.",
            "On landing, this board slides or reverses the whole row.",
            "Move the pictured piece above the bomb to stop it. Each swap counts down. At zero, the blast damages the wood.",
            "Put the pictured piece above the saw. It cuts the branch and opens that exchange.",
            "Put this piece on the patch. It fixes the board once.",
            "A cracked spot wears the board twice as fast.",
            "Swap one tied pair. Both pairs swap.",
            "This bridge works once. Then it folds shut.",
            "Put this piece on the stamp. Bring the row home to keep it.",
            "Move the pictured piece above the spool. You get one swap across one or two pieces. Save it for a lower board if you like.",
            "Move the pictured piece above the frost flask. The bomb waits for two swaps, including this one. The wood still wears down."
        };

        void BuildFlow()
        {
            resultRoot = Rect("Result controls", safeRoot, new Vector2(.05f,0), new Vector2(.95f,0));
            resultRoot.pivot = new Vector2(.5f,.5f); resultRoot.anchoredPosition = new Vector2(0,88); resultRoot.sizeDelta = new Vector2(0,160);
            Button("Next delivery",resultRoot,new Vector2(.52f,.35f),new Vector2(1,.65f),StartNextPreferred);
            var home=Button("Result home",resultRoot,new Vector2(.52f,0),new Vector2(1,.30f),ShowJourney);home.GetComponentInChildren<TextMeshProUGUI>().SetCopy("Map");
            resultRoot.gameObject.SetActive(false);
            coachRoot = Rect("Contextual coaching", safeRoot, Vector2.zero, Vector2.zero);
            coachRoot.sizeDelta = new Vector2(300,52); coachRoot.pivot = new Vector2(.5f,1);
            coachText = Text("Coaching",coachRoot,Vector2.zero,Vector2.one,15,""); coachText.raycastTarget=false;
            // Graphics are children of the sorting canvas, so later menu siblings cannot cover the finger.
            var pointerLayer=Rect("Tutorial pointer layer",safeRoot,Vector2.zero,Vector2.one);
            tutorialPointerCanvas=pointerLayer.gameObject.AddComponent<Canvas>();tutorialPointerCanvas.overrideSorting=true;tutorialPointerCanvas.sortingOrder=30;
            var handRect = Rect("First exchange hand",pointerLayer,Vector2.zero,Vector2.zero);
            // PreserveAspect adds a short vertical inset: this pivot meets the painted fingertip.
            handRect.pivot=new Vector2(.60f,.865f); handRect.sizeDelta=new Vector2(58,70);
            tutorialHand=handRect.gameObject.AddComponent<Image>(); tutorialHand.sprite=Resources.Load<Sprite>("CascadeArt/TutorialHand");
            tutorialHand.preserveAspect=true; tutorialHand.raycastTarget=false; tutorialHand.gameObject.SetActive(false);
            tutorialOverlayMaterial=new Material(Resources.Load<Shader>("CascadeArt/TutorialOverlay")){name="Tutorial hand above the puzzle"};tutorialHand.material=tutorialOverlayMaterial;
            BuildOutcomes();
            menuRoot = Rect("Menu",safeRoot,Vector2.zero,Vector2.one);
            menuTransition=menuRoot.gameObject.AddComponent<MenuTransition>();menuTransition.Initialize(this);
            gameTransition=floorArea.gameObject.AddComponent<MenuTransition>();gameTransition.Initialize(this,false);
            reachBadge=Picture("Ribbon charge",receiver,new Vector2(.88f,.95f),new Vector2(.96f,1.23f),Resources.Load<Sprite>("CascadeArt/Devices/RibbonSpool"),true);
            flowReady=true; ApplyPreferences();
        }
        public void ShowHome() { SetScreen(GameScreen.Home); }
        public void ShowAccountGate() { SetScreen(GameScreen.AccountGate); }
        public void ShowGame()
        {
            if(Interaction.Attempt.Current.Status==PuzzleStatus.Won&&!IsCheckpoint&&profile.Completion?.Acknowledged==true
                &&profile.Completion.AttemptId==Interaction.Attempt.AttemptId.ToString("D")){ShowJourney();return;}
            machine.SetZone(JourneyStory.Zone(Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node)));
            SetScreen(Interaction.Attempt.Current.Status==PuzzleStatus.Won ? (IsCheckpoint?GameScreen.Checkpoint:GameScreen.Win)
                : Interaction.Attempt.Current.Status==PuzzleStatus.Lost ? GameScreen.Fail : GameScreen.Playing);
        }
        bool HasResumableJourneyAttempt=>journeyActive&&(Interaction.Attempt.Current.Status!=PuzzleStatus.Won||IsCheckpoint
            ||profile.Completion?.Acknowledged==false&&profile.Completion.AttemptId==Interaction.Attempt.AttemptId.ToString("D"));
        void ResumeJourneyPuzzle()
        {
            if(Interaction.Attempt.Current.Status==PuzzleStatus.Lost){SetScreen(GameScreen.Playing);Retry();}
            else ShowGame();
        }
        public void ShowSettings()
        {
            if(CurrentScreen!=GameScreen.Settings)returnScreen=CurrentScreen; settingsPage=-1; languagePreview=null; SetScreen(GameScreen.Settings);
        }
        public void ShowDevices()
        {
            returnScreen=CurrentScreen; deviceIndex=DeviceForCurrentFloor(); SetScreen(GameScreen.Devices);
        }
        public void CloseMenu() { SetScreen(returnScreen); }
        public void SelectMode(PuzzleMode mode) { SwitchJourneyMode(mode); }
        void SetScreen(GameScreen screen)
        {
            if (!flowReady) return;
            if(screen==GameScreen.ReachChallenges||screen==GameScreen.PowerupChallenges)screen=GameScreen.TravelBag;
            if(screen==GameScreen.Requests)screen=GameScreen.JourneyMap;
            CloseLegalDocument();CancelConversation();CloseLandingPreview();CloseHintVideoOffer();
            if(screen!=GameScreen.Playing&&MechanicPopupOpen)ResetMechanicDiscovery();
            if(screen!=GameScreen.Playing)SettleDescent();
            if(screen!=GameScreen.Playing) { CancelHint(); StopPresentation(); }
            bool changed=CurrentScreen!=screen;
            if(changed||screen!=GameScreen.Playing)reactionBanner?.Clear();
            CurrentScreen=screen;machine.LivingWorld.SetMapSky(null);machine.LivingWorld.SetQuietFlight(screen==GameScreen.Win||screen==GameScreen.Fail||screen==GameScreen.Prologue);
            CreateGlobalCosmeticAmbiance();
            machine.LivingWorld.SetWorkshop(screen!=GameScreen.JourneyMap&&screen!=GameScreen.Dialogue&&screen!=GameScreen.JourneyEnd&&screen!=GameScreen.Playing&&screen!=GameScreen.Win&&screen!=GameScreen.Fail&&screen!=GameScreen.Checkpoint); userPaused=screen==GameScreen.Pause;
            Interaction.SetPaused(MechanicPopupOpen||screen!=GameScreen.Playing && screen!=GameScreen.Win && screen!=GameScreen.Fail && screen!=GameScreen.Checkpoint);
            bool machineVisible=screen==GameScreen.Playing || screen==GameScreen.Fail || screen==GameScreen.Checkpoint;
            if(!machineVisible)feedback.ClearTails(false);
            machine.SetGameplayVisible(machineVisible);machine.SetAmbientPaused(MechanicPopupOpen||screen==GameScreen.Pause||screen==GameScreen.SaveRecovery);
            header.gameObject.SetActive(machineVisible); status.gameObject.SetActive(machineVisible);statusPaper.gameObject.SetActive(machineVisible);
            floorArea.gameObject.SetActive(machineVisible); receiver.gameObject.SetActive(machineVisible);
            actions.gameObject.SetActive(machineVisible); precise.gameObject.SetActive(machineVisible && PreciseInputActive);
            coachRoot.gameObject.SetActive(false); tutorialHand.gameObject.SetActive(false);
            if(screen!=GameScreen.Playing)HideInstruction();
            menuRoot.gameObject.SetActive(!machineVisible); resultRoot.gameObject.SetActive(screen==GameScreen.Fail||screen==GameScreen.Checkpoint);
            if(machineVisible) { RenderState(); ArrangeResultControls();if(changed)gameTransition.Arrive(); } else {DrawMenu();if(changed&&!changingJourneyMode)menuTransition.Arrive(screen==GameScreen.Home||screen==GameScreen.PlayChoice||screen==GameScreen.JourneyMap);}
            outcome?.ShowResult(screen);
            Canvas.ForceUpdateCanvases();
        }
        void StartCoach()
        {
            string lesson = Catalog.PrimaryLevels.Concat(Catalog.BonusLevels).FirstOrDefault(c=>c.Definition.CampaignNodeId==node)?.Lesson ?? "swap";
            if(descent!=null&&descent.StageIndex<2){
                var first=Interaction.Attempt.Level.Floors[0];
                lesson=first.Powerup!=null?"powerup":first.Bomb!=null?"bomb":first.Key!=null?"key":first.Repair!=null?"repair":first.OptionalStamp!=null?"stamp":first.LinkedPair.HasValue?"linked":first.OneUseBridgeEdge.HasValue?"bridge":first.FragileEdges.Count>0?"fragile":descent.StageIndex==1&&descent.Version<5?"arrival_wind":"swap";
            }
            int wind=Enumerable.Range(0,Interaction.Attempt.Level.Floors.Count).Where(IsWindFloor).DefaultIfEmpty(-1).First();
            Coach=new TutorialCoach(Interaction.Attempt.Level,lesson,wind) {Enabled=Preferences.Tutorials};
            Coach.Observe(Interaction.Attempt.Current,Interaction.Attempt.RouteLength); gestureTime=0;
            ResetInstruction();
        }
        void UpdateFlow()
        {
            if(!flowReady || Interaction==null) return;
            TickMechanicIntroduction();
            bool foreground=applicationFocused&&!applicationPaused&&!Interaction.IsSuspended;
            reachBadge.gameObject.SetActive(Interaction.Attempt.Level.ReachCharges>0||Interaction.Attempt.Current.ReachCharges>0);reachBadge.color=Interaction.Attempt.Current.ReachCharges>0?Color.white:new Color(.5f,.5f,.5f,.5f);
            Coach?.Observe(Interaction.Attempt.Current,Interaction.Attempt.RouteLength);
            if(Coach!=null)ObserveCoachingProgress();
            Coach?.Advance(Time.unscaledDeltaTime,foreground && CurrentScreen==GameScreen.Playing && Interaction.CanPlay && !zoomOpen);
            UpdateDeadEnd();
            FinalizeScreenIfNeeded();
            UpdateCoaching(foreground);
            UpdateInstruction(foreground,Time.unscaledDeltaTime);
            if(HasSaveError && CurrentScreen!=GameScreen.SaveRecovery) {Recovery=Cascade.Persistence.SaveRecovery.WriteFailed;SetScreen(GameScreen.SaveRecovery);}
        }
        void FinalizeScreenIfNeeded()
        {
            if(!flowReady || !applicationFocused || applicationPaused||nativeAdSuspended)return;
            if(CurrentScreen==GameScreen.Playing && !Interaction.IsPresenting) {
                if(Interaction.Attempt.Current.Status==PuzzleStatus.Won) {SetScreen(IsCheckpoint?GameScreen.Checkpoint:GameScreen.Win); }
                else if(Interaction.Attempt.Current.Status==PuzzleStatus.Lost) {
                    sessionLosses[node]=sessionLosses.TryGetValue(node,out var n)?n+1:1; SetScreen(GameScreen.Fail);
                }
            }
        }
        void UpdateCoaching(bool foreground)
        {
            bool show=foreground && CurrentScreen==GameScreen.Playing && Interaction.CanPlay && !zoomOpen && !IsHoldingToken && reactionBanner?.IsShowing!=true && Coach!=null && Coach.Enabled;
            if(UpdateLessonPractice(foreground)){coachRoot.gameObject.SetActive(false);return;}
            coachRoot.gameObject.SetActive(false);
            show=show&&profile!=null&&!LessonWasSeen(MechanicLesson.Swap);
            bool first=show&&!firstGestureShown&&!dismissedInstructions.Contains("Drag a piece onto its neighbour.")&&node=="C01"&&Interaction.Attempt.RouteLength==0;
            bool replay=show&&!knownDeadEnd&&!Coach.LessonCompleted&&!dismissedInstructions.Contains("Drag a piece onto its neighbour.")&&Coach.GestureReplayRequested;
            tutorialHand.gameObject.SetActive((first||replay)&&!ReducedMotion&&tutorialHand.sprite!=null);
            if(tutorialHand.gameObject.activeSelf) {
                tutorialPointerCanvas.overrideSorting=true;tutorialPointerCanvas.sortingOrder=30;
                gestureTime+=Time.unscaledDeltaTime; float t=Mathf.Repeat(gestureTime,2.6f);
                int edge=0; while(edge<slots.Count-1 && !Interaction.Attempt.Resolver.Preview(Interaction.Attempt.Current,PuzzleCommand.Swap(edge)).IsLegal)edge++;
                if(edge>=slots.Count-1){ tutorialHand.gameObject.SetActive(false); return; }
                float slide=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.5f)/.8f));
                tutorialHand.rectTransform.position=Vector3.Lerp(
                    machine.TokenVisualPosition(edge)+machine.TokenBodyCentre(edge),
                    machine.TokenVisualPosition(edge+1)+machine.TokenBodyCentre(edge+1),slide);
                tutorialHand.color=new Color(1,1,1,t>2.1f?Mathf.Clamp01((2.6f-t)/.5f):1);
                if(first&&gestureTime>=2.6f)firstGestureShown=true;
            }
        }
        void ArrangeResultControls()
        {
            bool result=CurrentScreen==GameScreen.Fail||CurrentScreen==GameScreen.Checkpoint;
            resultRoot.gameObject.SetActive(result&&!ConversationOpen);
            PlaceControl(undo,result?resultRoot:actions,result?Vector2.zero:new Vector2(.12f,0),result?new Vector2(.48f,.30f):new Vector2(.47f,1));
            // Checkpoint already preserves the stage state; its old Undo counter was both redundant and misleading.
            undo.gameObject.SetActive(!result);
            PlaceControl(retry,result?resultRoot:safeRoot,result?new Vector2(0,.35f):new Vector2(.05f,.09f),result?new Vector2(CurrentScreen==GameScreen.Fail&&!CanOfferStrongerWood?1:.48f,.65f):new Vector2(.43f,.155f));
            retry.gameObject.SetActive(result||preparation.HasFailed||descentPlanFailed); hint.gameObject.SetActive(!result&&!preparation.HasFailed&&!descentPlanFailed); drop.gameObject.SetActive(!result&&!preparation.HasFailed&&!descentPlanFailed); precise.gameObject.SetActive(!result&&PreciseInputActive);
            if(preparation.HasFailed||descentPlanFailed)PlaceControl(retry,actions,new Vector2(.35f,0),new Vector2(.65f,1));
            if(result) {
                resultRoot.Find("Resulthome").GetComponentInChildren<TextMeshProUGUI>().SetCopy("Map");
                resultRoot.Find("Nextdelivery").gameObject.SetActive(CurrentScreen==GameScreen.Win||CurrentScreen==GameScreen.Checkpoint||CanOfferStrongerWood);
                LoadNavigationArt();
                void DressResult(Button button,bool primary){var face=button.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=Color.white;button.GetComponentInChildren<TextMeshProUGUI>().color=MenuPaper;}
                DressResult(undo,false);
                var undoColors=undo.colors;undoColors.disabledColor=new Color(.64f,.73f,.71f,1);undo.colors=undoColors;
                DressResult(resultRoot.Find("Nextdelivery").GetComponent<Button>(),true);
                DressResult(retry,CurrentScreen==GameScreen.Fail&&!CanOfferStrongerWood);
                DressResult(resultRoot.Find("Resulthome").GetComponent<Button>(),false);
                resultRoot.Find("Nextdelivery").GetComponentInChildren<TextMeshProUGUI>().SetCopy(CurrentScreen==GameScreen.Checkpoint?"Next stage":CanOfferStrongerWood?"More wood":journeyActive&&node=="C160"?"Light the beacon":"Next delivery");
                if(CurrentScreen==GameScreen.Checkpoint){
                    LoadNavigationArt();retry.gameObject.SetActive(false);var descend=resultRoot.Find("Nextdelivery").GetComponent<Button>();
                    PlaceControl(descend,resultRoot,new Vector2(0,.35f),new Vector2(1,.78f));descend.GetComponent<Image>().sprite=enamelControls[0];descend.GetComponent<Image>().type=Image.Type.Sliced;descend.GetComponent<Image>().color=Color.white;
                    var map=resultRoot.Find("Resulthome").GetComponent<Button>();PlaceControl(map,resultRoot,new Vector2(0,0),new Vector2(1,.30f));map.GetComponent<Image>().sprite=enamelControls[0];map.GetComponent<Image>().type=Image.Type.Sliced;map.GetComponent<Image>().color=Color.white;map.GetComponentInChildren<TextMeshProUGUI>().color=new Color(1,.97f,.84f);
                }
                else PlaceControl(resultRoot.Find("Nextdelivery").GetComponent<Button>(),resultRoot,new Vector2(.52f,.35f),new Vector2(1,.65f));
                LayoutResultControls(MenuUnit);
            }
            RefreshFieldTools();
        }
        static void PlaceControl(Button button,Transform parent,Vector2 min,Vector2 max)
        { var r=(RectTransform)button.transform; r.SetParent(parent,false); r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero; }
        void LayoutResultControls(float unit)
        {
            if(CurrentScreen!=GameScreen.Fail&&CurrentScreen!=GameScreen.Checkpoint)return;
            FixedBand(resultRoot,8*unit,140*unit);
            var map=resultRoot.Find("Resulthome").GetComponent<Button>();
            var descend=resultRoot.Find("Nextdelivery").GetComponent<Button>();
            foreach(var button in new[]{undo,map,retry,descend}){
                var rect=(RectTransform)button.transform;rect.localScale=Vector3.one;rect.localRotation=Quaternion.identity;
                FixedBand(rect,button==undo||button==map?0:54*unit,48*unit);
                var colors=button.colors;colors.disabledColor=new Color(.65f,.73f,.71f,1);button.colors=colors;
                var label=button.GetComponentInChildren<TextMeshProUGUI>();ApplyControlCaption(label,unit);
                label.margin=new Vector4(14*unit,5*unit,14*unit,5*unit);
            }
        }
        void ResetMenu()
        {
            foreach(var ceremony in menuRoot.GetComponents<RewardCelebration>())ceremony.enabled=false;
            menuActions.Clear();menuRows.Clear();menuBottoms.Clear();menuColumns.Clear();menuSquares.Clear();navigationLightLabels.Clear();
            settingsContent=null;settingsContentScroll=null;
            foreach(var key in baseTextSizes.Keys.Where(k=>k==null||k.transform.IsChildOf(menuRoot)).ToArray())baseTextSizes.Remove(key);
            foreach(Transform child in menuRoot){ child.gameObject.SetActive(false); Destroy(child.gameObject); }
            // The illustrated scene is the menu surface; ink sits on its quiet sky window.
            var wash=Rect("Menu hit surface",menuRoot,Vector2.zero,Vector2.one);menuWash=Background(wash,Color.clear,false);
            if(CurrentScreen!=GameScreen.JourneyMap&&CurrentScreen!=GameScreen.Dialogue){
                var motes=Rect("Window light and drifting dust",menuRoot,new Vector2(.13f,.12f),new Vector2(.87f,.85f)).gameObject.AddComponent<WorldWeatherGraphic>();
                motes.InitializeRegion(this,7);motes.color=new Color(1,.85f,.51f,.15f);
            }
        }
        void DrawMenu()
        {
            LayoutMenuSurface();
            ResetMenu();
            if(CurrentScreen==GameScreen.Home) DrawMainMenu();
            else if(CurrentScreen==GameScreen.Win) DrawCompletion();
            else if(CurrentScreen==GameScreen.JourneyMap) DrawJourney();
            else if(CurrentScreen==GameScreen.PlayChoice) DrawPlayChoice();
            else if(CurrentScreen==GameScreen.Prologue) DrawPrologue();
            else if(CurrentScreen==GameScreen.Shop) DrawShop();
            else if(CurrentScreen==GameScreen.Supplies) DrawSupplies();
            else if(CurrentScreen==GameScreen.NoAds) DrawNoAds();
            else if(CurrentScreen==GameScreen.CoinPacks) DrawCoinPacks();
            else if(CurrentScreen==GameScreen.AccountGate) DrawAccountGate();
            else if(CurrentScreen==GameScreen.Lessons) DrawLessons();
            else if(CurrentScreen==GameScreen.Dialogue) DrawDialogue();
            else if(CurrentScreen==GameScreen.Settings) DrawSettings();
            else if(CurrentScreen==GameScreen.WordLanguage) DrawWordLanguageConfirmation();
            else if(CurrentScreen==GameScreen.SaveRecovery) DrawSaveRecovery();
            else if(CurrentScreen==GameScreen.TravelBag) DrawTravelBag();
            else if(CurrentScreen==GameScreen.Journal) DrawJournal();
            else if(CurrentScreen==GameScreen.Collection) DrawCollection();
            else if(CurrentScreen==GameScreen.Workshop) DrawWorkshop();
            else if(CurrentScreen==GameScreen.Records) DrawRecords();
            else if(CurrentScreen==GameScreen.Contracts) DrawContracts();
            else if(CurrentScreen==GameScreen.Parcel) DrawParcel();
            else if(CurrentScreen==GameScreen.JourneyEnd)DrawJourneyEnd();
            else if(CurrentScreen==GameScreen.Devices) DrawDevices();
            else if(CurrentScreen==GameScreen.Pause) {
                MenuTitle("Take a break", "");
                var friend=Picture("Mika rests beside the puzzle",menuRoot,new Vector2(.30f,.64f),new Vector2(.70f,.82f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
                friend.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,2),0,.8f);
                Rect("Mika's wooden shelf",menuRoot,new Vector2(.24f,.625f),new Vector2(.76f,.642f)).gameObject.AddComponent<WoodBoardGraphic>();
                MenuButton("Resume",.50f,ShowGame,true); MenuButton("Retry",.40f,()=>{SetScreen(GameScreen.Playing);Retry();}).interactable=Interaction.AllowsRetry;
                MenuButton("My tools",.30f,ShowSupplies).interactable=Interaction.AllowsControl(PracticeControl.Toolbag); MenuButton("How to play",.20f,ShowHowToPlay);
                MenuButton("Settings",.10f,ShowSettings,false,.08f,.48f);MenuButton("Home",.10f,ShowHome,false,.52f,.92f);
            }
            FinishMenuContrast();
            FitMenuRows();ApplyMenuTypography();
        }
        static readonly Color MenuInk=new Color(.055f,.16f,.17f);
        static readonly Color MenuPaper=new Color(.99f,.98f,.92f);
        void MenuReadingSurface(string name,Vector2 min,Vector2 max)
        {
            Rect(name,menuRoot,min,max); // Layout group only; the illustrated world remains visible.
        }
        void FinishMenuContrast()
        {
            foreach(var label in menuRoot.GetComponentsInChildren<TextMeshProUGUI>())if(label.name!="Illustrated numeral copy"&&!IsSymbolFace(label.name)&&label.name!="Mini glyph")label.color=label.name=="Earned score"?new Color(1,.88f,.46f):navigationLightLabels.Contains(label)||label.GetComponentInParent<Button>()?.GetComponent<Image>()?.sprite==enamelControls[0]?new Color(1,.97f,.84f):MenuInk;
            bool nightMap=CurrentScreen==GameScreen.JourneyMap&&islandBrowseIndex<0&&mapPage==8;
            bool nightResult=(CurrentScreen==GameScreen.Win||CurrentScreen==GameScreen.Parcel||CurrentScreen==GameScreen.JourneyEnd)&&machine.LivingWorld.Zone==8;
            foreach(var label in menuRoot.GetComponentsInChildren<TextMeshProUGUI>()){
                if(label.name=="Illustrated numeral copy")continue;
                bool mapCopy=nightMap&&label.GetComponentInParent<Button>()==null;
                bool resultCopy=nightResult&&(label.name=="Completed stages"||label.name=="Rank guidance"||label.name=="Earned coins"||label.name=="Earned light"||label.name=="Completion reward note"||label.name=="Journey ending"||label.name=="Menu title"||label.name=="Menu description");
                if(mapCopy||resultCopy)label.color=MenuPaper;
            }
            FinishThemedTypeStyles();
            ApplyMenuTypography();PaintShopShortfall();
            foreach(var button in menuRoot.GetComponentsInChildren<Button>())if(!button.interactable){
                var image=button.GetComponent<Image>();if(image!=null&&image.color.a>0)image.color=new Color(.86f,.91f,.87f);
                var colors=button.colors;colors.disabledColor=Color.white;button.colors=colors;
                foreach(var label in button.GetComponentsInChildren<TextMeshProUGUI>())if(!IsSymbolFace(label.name))label.color=image?.sprite==enamelControls[0]||navigationLightLabels.Contains(label)?MenuPaper:MenuInk;
            }
            foreach(var button in menuRoot.GetComponentsInChildren<Button>())if(button.name=="Buy selected item")
                foreach(var label in button.GetComponentsInChildren<TextMeshProUGUI>())if(!IsSymbolFace(label.name))label.color=Color.white;
        }
        void MenuTitle(UiCopy title,UiCopy body)
        {
            var label=Text("Menu title",menuRoot,new Vector2(.06f,.905f),new Vector2(.94f,.978f),26,title);
            StyleCelebrationType(label,CelebrationType.DisplayTitle);label.alignment=TextAlignmentOptions.Midline;
            label.enableAutoSizing=false;label.textWrappingMode=TextWrappingModes.Normal;menuHeading=label;
            menuDescription=string.IsNullOrEmpty(body.Key)?null:ReadingStrip("Menu description",new Vector2(.06f,.815f),new Vector2(.94f,.896f),body,18,true);
        }
        Button MenuButton(UiCopy label,float bottom,Action action,bool primary=false,float left=MenuColumnInset,float right=1-MenuColumnInset)
        {
            float unit=MenuUnit;float fixedBottom=bottom<=.04f?16:bottom<=.15f?80:-1;
            var button=Button(label,menuRoot,new Vector2(left,bottom),new Vector2(right,bottom),()=>{feedback.Sound(ToyCue.Select);action();});
            var r=(RectTransform)button.transform;r.pivot=new Vector2(.5f,0);r.sizeDelta=new Vector2(0,52*unit);
            menuActions.Add(r);if(fixedBottom>=0)menuBottoms.Add(new MenuBottom{rect=r,bottom=fixedBottom});var caption=button.GetComponentInChildren<TextMeshProUGUI>();caption.margin=new Vector4(8,3,8,3);caption.fontSize=(right-left<.51f?20:22)*(Preferences.LargeText?1.10f:1);caption.textWrappingMode=TextWrappingModes.NoWrap;
            caption.enableAutoSizing=true;caption.fontSizeMin=16*unit;caption.fontSizeMax=(right-left<.51f?20:22)*unit*(Preferences.LargeText?1.10f:1);
            var face=button.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=Color.white;
            return button;
        }
        IReadOnlyList<LevelContent> PreferredLevels()
        {
            if(PreferredMode==PuzzleMode.Shapes) return Catalog.PrimaryLevels.Where(c=>c.Mode==PuzzleMode.Shapes).ToArray();
            if(PreferredMode==PuzzleMode.Mixed)return Catalog.PrimaryLevels.Select((primary,i)=>
                Catalog.TryGetLocalizedBinding(primary.Definition.CampaignNodeId,Preferences.PuzzleLocale,out var localized)
                &&(localized.Mode==PuzzleMode.Mixed||i%2==1)?localized:primary).ToArray();
            return Catalog.LocalizedLevels.Where(c=>c.Locale==Preferences.PuzzleLocale&&c.Mode==PreferredMode).ToArray();
        }
        void StartContent(LevelContent content)
        {
            if(content!=null&&DescentRun.CanDescend(content.Definition))content=PreferredLevels().FirstOrDefault(c=>c.Definition.CampaignNodeId==content.Definition.CampaignNodeId);
            if(!IsContentOpen(content))return;
            storyRoute=false;
            if(DescentRun.CanDescend(content.Definition))StartCampaignLevel(content.Definition,content.Locale);
            else LoadLevel(content.Definition,content.Locale);
            ShowGame();
        }
        void StartNextPreferred()
        {
            if(CurrentScreen==GameScreen.Checkpoint){GoLower();return;}
            if(CurrentScreen==GameScreen.Win){ContinueCompletion();return;}
            if(CanOfferStrongerWood){RestartWithStrongerWood();return;}
            int bonus=Catalog.BonusLevels.ToList().FindIndex(c=>c.Definition.PuzzleId==Interaction.Attempt.Level.PuzzleId);
            if(bonus>=0){ShowJourney();return;}
            if(journeyActive) {ShowJourney();return;}
            var levels=PreferredLevels();int index=levels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node);
            if(index>=0&&index+1<levels.Count)StartContent(levels[index+1]);else ShowHome();
        }
        int DeviceForCurrentFloor()
        {
            var lessons=MechanicLessons.DiscoverVisible(Interaction.Attempt.Level,Interaction.Attempt.Current);
            return lessons.Count>0?(int)lessons[0]:(int)MechanicLesson.Swap;
        }
        void DrawDevices()
        {
            if(deviceIndex<0)deviceIndex=(int)MechanicLesson.Swap;
            var lesson=(MechanicLesson)deviceIndex;
            MenuTitle(LessonTitle(lesson),"");
            int total=Enum.GetValues(typeof(MechanicLesson)).Length;
            MenuPager("example",UiCopy.Raw((deviceIndex+1)+" / "+total),.855f,
                ()=>{deviceIndex=(deviceIndex+total-1)%total;DrawMenu();},
                ()=>{deviceIndex=(deviceIndex+1)%total;DrawMenu();});
            BuildLessonFilm(Rect("Device example",menuRoot,new Vector2(.12f,.22f),new Vector2(.88f,.77f)),lesson);
            MenuButton("Back",.025f,CloseMenu);
        }
        void DrawSettings()
        {
            MenuTitle(settingsPage<0?(UiCopy)"Settings":SettingsSections[settingsPage],"");
            BuildSettingsList();
            if(settingsPage<0){DrawSettingsSections();SettingsButton("Remove ads",ShowNoAds);DrawLegalControls();}
            if(settingsPage==0) {
                Setting("Music",Preferences.Music,0,()=>Preferences.Music=!Preferences.Music);
                Setting("Sound effects",Preferences.Sound,1,()=>Preferences.Sound=!Preferences.Sound);
                Setting("Haptics",Preferences.Haptics,2,()=>Preferences.Haptics=!Preferences.Haptics);
                Setting("Reduced motion",Preferences.ReducedMotion,3,()=>Preferences.ReducedMotion=!Preferences.ReducedMotion);
                Setting("Screen shake",Preferences.ScreenShake,4,()=>Preferences.ScreenShake=!Preferences.ScreenShake);
                SettingsButton(new UiCopy("Effects: {0}",new UiCopy(new[]{"Off","Low","Full"}[Preferences.EffectQuality])),()=>{Preferences.EffectQuality=(Preferences.EffectQuality+1)%3;ApplyPreferences();DrawMenu();});
            } else if(settingsPage==1) {
                Setting("High contrast",Preferences.HighContrast,0,()=>Preferences.HighContrast=!Preferences.HighContrast);
                Setting("Larger text",Preferences.LargeText,1,()=>Preferences.LargeText=!Preferences.LargeText);
                Setting("Precise input",Preferences.PreciseInput,2,()=>Preferences.PreciseInput=!Preferences.PreciseInput);
                Setting("Tutorial help",Preferences.Tutorials,3,()=>Preferences.Tutorials=!Preferences.Tutorials);
                Setting("Dead-end warning",Preferences.DeadEndWarnings,4,()=>Preferences.DeadEndWarnings=!Preferences.DeadEndWarnings);
            }
            if(settingsPage==2)DrawLanguages();
            if(settingsPage==3)DrawNotificationSettings();
            if(settingsPage==4)DrawCloudSettings();
            if(settingsPage==1&&adProvider?.PrivacyOptionsRequired==true)SettingsButton("Advertising privacy",()=>adProvider.ShowPrivacyOptions());
            MenuButton("Back",.025f,()=>{if(settingsPage>=0){settingsPage=-1;languagePreview=null;DrawMenu();}else CloseMenu();});
            FitSettingsLayout();
        }
        Button Setting(string label,bool value,int index,Action toggle)
        {
            var button=SettingsButton(label,()=>{toggle();ApplyPreferences();DrawMenu();});
            var text=button.GetComponentInChildren<TextMeshProUGUI>();text.SetCopy(label);text.alignment=TextAlignmentOptions.MidlineLeft;
            text.rectTransform.anchorMin=new Vector2(.065f,0);text.rectTransform.anchorMax=new Vector2(.75f,1);text.margin=new Vector4(4,5,8,5);
            var track=Rect("Switch",button.transform,new Vector2(.78f,.24f),new Vector2(.94f,.76f));var trackImage=Background(track,value?new Color(.12f,.60f,.49f):new Color(.04f,.22f,.25f),false);trackImage.sprite=art.panel;trackImage.type=Image.Type.Sliced;
            var knob=Rect("Knob",track,new Vector2(value?.52f:.04f,.09f),new Vector2(value?.96f:.48f,.91f));var knobImage=Background(knob,Paper,false);knobImage.sprite=art.panel;knobImage.type=Image.Type.Sliced;
            return button;
        }
        void MiniMachine(RectTransform root,IReadOnlyList<SymbolId> row,IReadOnlyList<SymbolId> target)
        {
            var upper=Rect("Wood rail",root,new Vector2(0,.47f),new Vector2(1,.57f));upper.gameObject.AddComponent<WoodBoardGraphic>();
            var lower=Rect("Gold receiver",root,new Vector2(0,0),new Vector2(1,.31f));var rim=Background(lower,new Color(.94f,.73f,.32f),false);rim.sprite=art.panel;rim.type=Image.Type.Sliced;
            Draw(row,.58f,.97f);Draw(target,.035f,.275f);
            void Draw(IReadOnlyList<SymbolId> symbols,float low,float high)
            {
                for(int i=0;i<symbols.Count;i++) {
                    float x=(i+.5f)/symbols.Count,w=.38f/symbols.Count;
                    var slot=Rect("Mini symbol",root,new Vector2(x-w,low),new Vector2(x+w,high));
                    var shape=slot.gameObject.AddComponent<ShapeFaceGraphic>();shape.raycastTarget=false;
                    shape.SetShape(art.SymbolMesh(symbols[i]),art.SymbolMaterial(symbols[i]).GetColor("_BaseColor"),true);
                    if(symbols[i].Kind==SymbolKind.Glyph)Text("Mini glyph",slot,Vector2.zero,Vector2.one,18,symbols[i].Value);
                }
            }
        }
        void ApplyPreferences()
        {
            PersistProgress();
            feedback.MusicEnabled=Preferences.Music;feedback.SoundEnabled=Preferences.Sound;feedback.HapticsEnabled=Preferences.Haptics;
            feedback.Quality=Preferences.EffectQuality;machine.LivingWorld.EffectQuality=Preferences.EffectQuality;ReducedMotion=Preferences.ReducedMotion;ForcePreciseInput=Preferences.PreciseInput;
            machine.ScreenShakeEnabled=Preferences.ScreenShake;
            if(Preferences.EffectQuality==0)feedback.ClearTails(false);
            if(!Preferences.DeadEndWarnings)CancelDeadEnd();
            if(Coach!=null)Coach.Enabled=Preferences.Tutorials;
            foreach(var entry in baseTextSizes.ToArray()) {
                if(entry.Key==null){baseTextSizes.Remove(entry.Key);continue;}
                if(entry.Key.name=="Label"&&entry.Key.GetComponentInParent<Button>()!=null){
                    float unit=1/Mathf.Min(1,entry.Key.GetComponentInParent<Canvas>().scaleFactor);ApplyControlCaption(entry.Key,unit);
                }else entry.Key.fontSize=entry.Value*(Preferences.LargeText&&!IsSymbolFace(entry.Key.name)?1.12f:1);
                if(entry.Key.name!="Bomb painted letter")entry.Key.color=Preferences.HighContrast?new Color(.06f,.17f,.18f):menuRoot!=null&&entry.Key.transform.IsChildOf(menuRoot)?MenuInk:Ink;
            }
            PaintControlLabels();
            if(menuRoot!=null&&menuRoot.gameObject.activeSelf)FinishMenuContrast();
            PaintGameplayTextContrast();
        }
        static bool IsSymbolFace(string label)=>label=="Body painted letter"||label=="Bomb painted letter"||label=="Required letter"||label=="Symbol"||label=="TargetSymbol"||label=="DeviceGlyph"||label=="ZoomSymbol";
        void ScheduleDeadEnd()
        {
            CancelDeadEnd();
            if(!Preferences.DeadEndWarnings||Interaction.Attempt.Current.Status!=PuzzleStatus.Playing)return;
            var request=Interaction.Attempt.CaptureHintRequest();deadEndCancellation=new CancellationTokenSource();var token=deadEndCancellation.Token;
            deadEndTask=Task.Run(()=>AttemptService.CalculateHint(request,cancellationToken:token));
        }
        void CancelDeadEnd()
        {
            deadEndCancellation?.Cancel();deadEndCancellation?.Dispose();deadEndCancellation=null;knownDeadEnd=false;
            if(deadEndTask!=null)deadEndTask.ContinueWith(t=>{var ignored=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
            deadEndTask=null;
        }
        void UpdateDeadEnd()
        {
            if(deadEndTask==null||!deadEndTask.IsCompleted)return;
            var completed=deadEndTask;deadEndTask=null;deadEndCancellation?.Dispose();deadEndCancellation=null;
            if(completed.IsCanceled)return;
            if(completed.IsFaulted){var ignored=completed.Exception;return;}
            // Proving a dead end must not reveal or charge a hint.
            knownDeadEnd=Preferences.DeadEndWarnings&&Interaction.Attempt.IsCurrent(completed.Result)&&completed.Result.Result.Status==HintStatus.DeadEnd;
        }
    }
}
