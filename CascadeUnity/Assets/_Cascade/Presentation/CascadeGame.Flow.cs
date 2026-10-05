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
    public enum GameScreen { Home, Playing, Pause, Win, Fail, Settings, Devices, Lessons, Dialogue, WordLanguage, SaveRecovery, TravelBag, Journal, Collection, Workshop, Requests, Records, Contracts, Parcel, JourneyEnd, ReachChallenges, PowerupChallenges }
    public sealed partial class CascadeGame
    {
        public GameScreen CurrentScreen { get; private set; } = GameScreen.Playing;
        public GamePreferences Preferences { get; private set; } = new GamePreferences();
        public PuzzleMode PreferredMode { get; private set; } = PuzzleMode.Shapes;
        public TutorialCoach Coach { get; private set; }
        public bool HasDeadEndWarning => knownDeadEnd;
        public bool IsTutorialHandVisible => tutorialHand!=null&&tutorialHand.gameObject.activeInHierarchy;
        RectTransform menuRoot, resultRoot, coachRoot;
        Image tutorialHand, resultFriend, reachBadge;
        Image menuWash;
        MenuTransition menuTransition;
        CoachRingGraphic coachRing;
        TextMeshProUGUI coachText, resultText;
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
        static readonly string[] DeviceTitles = { "Durability", "Entry rollers", "Bomb", "Key", "Repair", "Fragile edge", "Linked pairs", "One-use bridge", "Stamp", "Ribbon spool", "Hourglass" };
        static readonly string[] DeviceNodes = { "C03", "C09", "C13", "C17", "C18", "C21", "C25", "C33", "C35", "B01", "B04" };
        static readonly string[] DeviceCopy = {
            "Each swap uses a move. At zero, the wood breaks. Descend leaves early.",
            "On landing, this board slides or reverses the whole row.",
            "Move the pictured piece above the bomb to stop it. Each swap counts down. At zero, the blast damages the wood.",
            "Put this piece on the key to open its gate.",
            "Put this piece on the patch. It fixes the board once.",
            "A cracked spot wears the board twice as fast.",
            "Swap one tied pair. Both pairs swap.",
            "This bridge works once. Then it folds shut.",
            "Put this piece on the stamp. Bring the row home to keep it.",
            "Move the pictured piece above the spool. You get one swap across one or two pieces. Save it for a lower board if you like.",
            "Move the pictured piece above the hourglass. The bomb waits for two swaps, including this one. The wood still wears down."
        };

        void BuildFlow()
        {
            resultRoot = Rect("Result controls", safeRoot, new Vector2(.05f,0), new Vector2(.95f,0));
            resultRoot.pivot = new Vector2(.5f,.5f); resultRoot.anchoredPosition = new Vector2(0,88); resultRoot.sizeDelta = new Vector2(0,160);
            resultText = Text("Result summary", resultRoot, new Vector2(0,.71f), Vector2.one,18,"");
            Button("Next delivery",resultRoot,new Vector2(.52f,.35f),new Vector2(1,.65f),StartNextPreferred);
            var home=Button("Result home",resultRoot,new Vector2(.52f,0),new Vector2(1,.30f),()=>{if(WaitingParcel!=null)ShowParcel();else ShowHome();});home.GetComponentInChildren<TextMeshProUGUI>().SetCopy("Home");
            resultRoot.gameObject.SetActive(false);
            coachRoot = Rect("Contextual coaching", safeRoot, Vector2.zero, Vector2.zero);
            coachRoot.sizeDelta = new Vector2(300,52); coachRoot.pivot = new Vector2(.5f,1);
            coachText = Text("Coaching",coachRoot,Vector2.zero,Vector2.one,15,""); coachText.raycastTarget=false;
            var ringRect=Rect("Device focus",safeRoot,Vector2.zero,Vector2.zero);ringRect.sizeDelta=new Vector2(48,48);
            coachRing=ringRect.gameObject.AddComponent<CoachRingGraphic>();coachRing.raycastTarget=false;
            coachRing.color=new Color(.95f,.69f,.23f,.9f);ringRect.gameObject.SetActive(false);
            var handRect = Rect("First exchange hand",safeRoot,Vector2.zero,Vector2.zero);
            handRect.pivot=new Vector2(.5f,.9f); handRect.sizeDelta=new Vector2(58,70);
            tutorialHand=handRect.gameObject.AddComponent<Image>(); tutorialHand.sprite=Resources.Load<Sprite>("CascadeArt/TutorialHand");
            tutorialHand.preserveAspect=true; tutorialHand.raycastTarget=false; tutorialHand.gameObject.SetActive(false);
            var mika=Resources.Load<Sprite>("CascadeArt/Journey/Mika");
            if(mika!=null) { resultFriend=Picture("Mika celebrates",safeRoot,new Vector2(.31f,.43f),new Vector2(.69f,.72f),mika,true);resultFriend.gameObject.SetActive(false); }
            menuRoot = Rect("Menu",safeRoot,Vector2.zero,Vector2.one);
            menuTransition=menuRoot.gameObject.AddComponent<MenuTransition>();menuTransition.Initialize(this);
            reachBadge=Picture("Ribbon charge",header,new Vector2(.69f,.05f),new Vector2(.80f,.95f),Resources.Load<Sprite>("CascadeArt/Devices/RibbonSpool"),true);
            flowReady=true; ApplyPreferences();
        }
        public void ShowHome() { ShowJourney(); }
        public void ShowGame()
        {
            machine.SetZone(JourneyStory.Zone(Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node)));
            SetScreen(Interaction.Attempt.Current.Status==PuzzleStatus.Won ? GameScreen.Win
                : Interaction.Attempt.Current.Status==PuzzleStatus.Lost ? GameScreen.Fail : GameScreen.Playing);
        }
        public void ShowSettings()
        {
            returnScreen=CurrentScreen; settingsPage=0; SetScreen(GameScreen.Settings);
        }
        public void ShowDevices()
        {
            returnScreen=CurrentScreen; deviceIndex=DeviceForCurrentFloor(); SetScreen(GameScreen.Devices);
        }
        public void CloseMenu() { SetScreen(returnScreen); }
        public void SelectMode(PuzzleMode mode) { PreferredMode=mode; PersistProgress(); chapterPage=0; if(CurrentScreen==GameScreen.Home||CurrentScreen==GameScreen.Lessons) DrawMenu(); }
        void SetScreen(GameScreen screen)
        {
            if (!flowReady) return;
            if(screen!=GameScreen.Playing) { CancelHint(); StopPresentation(); }
            bool changed=CurrentScreen!=screen;
            CurrentScreen=screen; machine.LivingWorld.SetWorkshop(screen==GameScreen.TravelBag||screen==GameScreen.Workshop||screen==GameScreen.Collection||screen==GameScreen.Parcel); userPaused=screen==GameScreen.Pause;
            if(resultFriend!=null)resultFriend.gameObject.SetActive(screen==GameScreen.Win);
            Interaction.SetPaused(screen!=GameScreen.Playing && screen!=GameScreen.Win && screen!=GameScreen.Fail);
            bool machineVisible=screen==GameScreen.Playing || screen==GameScreen.Win || screen==GameScreen.Fail;
            machine.SetGameplayVisible(machineVisible);machine.SetAmbientPaused(screen==GameScreen.Pause||screen==GameScreen.SaveRecovery);
            header.gameObject.SetActive(machineVisible); status.gameObject.SetActive(machineVisible);statusPaper.gameObject.SetActive(machineVisible);
            floorArea.gameObject.SetActive(machineVisible); receiver.gameObject.SetActive(machineVisible);
            actions.gameObject.SetActive(machineVisible); precise.gameObject.SetActive(machineVisible && PreciseInputActive);
            coachRoot.gameObject.SetActive(false); tutorialHand.gameObject.SetActive(false);coachRing.gameObject.SetActive(false);
            menuRoot.gameObject.SetActive(!machineVisible); resultRoot.gameObject.SetActive(screen==GameScreen.Win||screen==GameScreen.Fail);
            if(machineVisible) { RenderState(); ArrangeResultControls(); } else {DrawMenu();if(changed)menuTransition.Arrive();}
            Canvas.ForceUpdateCanvases();
        }
        void StartCoach()
        {
            string lesson = Catalog.PrimaryLevels.Concat(Catalog.BonusLevels).FirstOrDefault(c=>c.Definition.CampaignNodeId==node)?.Lesson ?? "swap";
            Coach=new TutorialCoach(Interaction.Attempt.Level,lesson,Interaction.Attempt.Direction?.WindFloor??-1) {Enabled=Preferences.Tutorials};
            Coach.Observe(Interaction.Attempt.Current,Interaction.Attempt.RouteLength); gestureTime=0;
        }
        void UpdateFlow()
        {
            if(!flowReady || Interaction==null) return;
            bool foreground=applicationFocused&&!applicationPaused&&!Interaction.IsSuspended;
            reachBadge.gameObject.SetActive(Interaction.Attempt.Level.ReachCharges>0||Interaction.Attempt.Current.ReachCharges>0);reachBadge.color=Interaction.Attempt.Current.ReachCharges>0?Color.white:new Color(.5f,.5f,.5f,.5f);
            Coach?.Observe(Interaction.Attempt.Current,Interaction.Attempt.RouteLength);
            Coach?.Advance(Time.unscaledDeltaTime,foreground && CurrentScreen==GameScreen.Playing && Interaction.CanPlay && !zoomOpen);
            UpdateDeadEnd();
            FinalizeScreenIfNeeded();
            UpdateCoaching(foreground);
            if(HasSaveError && CurrentScreen!=GameScreen.SaveRecovery) {Recovery=Cascade.Persistence.SaveRecovery.WriteFailed;SetScreen(GameScreen.SaveRecovery);}
        }
        void FinalizeScreenIfNeeded()
        {
            if(!flowReady || !applicationFocused || applicationPaused)return;
            if(CurrentScreen==GameScreen.Playing && !Interaction.IsPresenting) {
                if(Interaction.Attempt.Current.Status==PuzzleStatus.Won) { SetScreen(GameScreen.Win); }
                else if(Interaction.Attempt.Current.Status==PuzzleStatus.Lost) {
                    sessionLosses[node]=sessionLosses.TryGetValue(node,out var n)?n+1:1; SetScreen(GameScreen.Fail);
                }
            }
        }
        void UpdateCoaching(bool foreground)
        {
            bool show=foreground && CurrentScreen==GameScreen.Playing && Interaction.CanPlay && !zoomOpen && !IsHoldingToken && Coach!=null && Coach.Enabled;
            string prompt=show ? Coach.Prompt(Interaction.Attempt.Current) : "";
            if(show&&(Interaction.Attempt.Level.ReachCharges>0||Interaction.Attempt.Current.ReachCharges>0))prompt=Interaction.Attempt.Current.ReachCharges>0?"Drag past a neighbour. The ribbon works once.":"Ribbon used. You can still swap neighbours.";
            if(show&&knownDeadEnd)prompt="Stuck? Undo and try another move.";
            coachRoot.gameObject.SetActive(false);
            if(prompt.Length>0) {
                coachText.SetCopy(prompt); if(!IsHoldingToken && !Interaction.IsPresenting)status.SetCopy(prompt);
                var floor=floorViews[Interaction.Attempt.Current.ActiveFloor];
                coachRoot.position=floor.TransformPoint(new Vector3(0,-floor.rect.height*.64f,0));
                coachRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,Mathf.Min(310,safeRoot.rect.width*.90f));
            }
            coachRing.gameObject.SetActive(show&&!knownDeadEnd&&Coach.HighlightRequested);
            if(coachRing.gameObject.activeSelf) {
                var state=Interaction.Attempt.Current;var f=Interaction.Attempt.Level.Floors[state.ActiveFloor];ContactDefinition contact=null;
                if(f.Powerup!=null&&!state.PowerupUsed)contact=f.Powerup;
                else if(f.Bomb!=null&&state.BombPhase==BombPhase.Armed)contact=f.Bomb;
                else if(f.Key!=null&&!state.KeyUsed)contact=f.Key;
                else if(f.Repair!=null&&!state.RepairUsed)contact=f.Repair;
                else if(f.OptionalStamp!=null&&(state.StampBits&(1<<state.ActiveFloor))==0)contact=f.OptionalStamp;
                coachRing.rectTransform.position=contact==null?machine.FloorPosition(state.ActiveFloor):machine.ContactPosition(state.ActiveFloor,contact.Slot);
                coachRing.color=new Color(.95f,.69f,.23f,ReducedMotion?.9f:.65f+.25f*Mathf.Sin(Time.unscaledTime*3));
            }
            bool first=show&&!firstGestureShown&&node=="C01"&&Interaction.Attempt.RouteLength==0;
            bool replay=show&&!knownDeadEnd&&Coach.GestureReplayRequested;
            tutorialHand.gameObject.SetActive((first||replay)&&!ReducedMotion&&tutorialHand.sprite!=null);
            if(tutorialHand.gameObject.activeSelf) {
                gestureTime+=Time.unscaledDeltaTime; float t=Mathf.Repeat(gestureTime,2.6f);
                int edge=0; while(edge<slots.Count-1 && !Interaction.Attempt.Resolver.Preview(Interaction.Attempt.Current,PuzzleCommand.Swap(edge)).IsLegal)edge++;
                if(edge>=slots.Count-1){ tutorialHand.gameObject.SetActive(false); return; }
                float slide=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.5f)/.8f));
                tutorialHand.rectTransform.position=Vector3.Lerp(machine.TokenPosition(edge),machine.TokenPosition(edge+1),slide);
                tutorialHand.color=new Color(1,1,1,t>2.1f?Mathf.Clamp01((2.6f-t)/.5f):1);
                if(first&&gestureTime>=2.6f)firstGestureShown=true;
            }
        }
        void ArrangeResultControls()
        {
            bool result=CurrentScreen==GameScreen.Win||CurrentScreen==GameScreen.Fail;
            resultRoot.gameObject.SetActive(result);
            PlaceControl(undo,result?resultRoot:actions,result?Vector2.zero:Vector2.zero,result?new Vector2(.48f,.30f):new Vector2(.30f,1));
            PlaceControl(retry,result?resultRoot:safeRoot,result?new Vector2(0,.35f):new Vector2(.05f,.09f),result?new Vector2(.48f,.65f):new Vector2(.43f,.155f));
            retry.gameObject.SetActive(result||preparation.HasFailed); hint.gameObject.SetActive(!result&&!preparation.HasFailed); drop.gameObject.SetActive(!result&&!preparation.HasFailed); precise.gameObject.SetActive(!result&&PreciseInputActive);
            if(preparation.HasFailed)PlaceControl(retry,actions,new Vector2(.35f,0),new Vector2(.65f,1));
            if(result) {
                resultText.SetCopy(CurrentScreen==GameScreen.Win ? "You did it! The way is clear." : "Not quite. Undo and try again.");
                if(CurrentScreen==GameScreen.Win && !Interaction.Attempt.Goal.IsSatisfied(Interaction.Attempt.Current))resultText.SetCopy("The row is home. The stamp is still missing.");
                resultRoot.Find("Resulthome").GetComponentInChildren<TextMeshProUGUI>().SetCopy(WaitingParcel!=null?"Open parcel":"Home");
                if(CurrentScreen==GameScreen.Fail && sessionLosses.TryGetValue(node,out var losses)&&losses>=3)
                    resultText.SetCopy("Try the device guide. Undo and retry stay free.");
                resultRoot.Find("Nextdelivery").gameObject.SetActive(CurrentScreen==GameScreen.Win||CanOfferStrongerWood);
                resultRoot.Find("Nextdelivery").GetComponent<Image>().color=new Color(.98f,.74f,.32f);
                retry.GetComponent<Image>().color=CurrentScreen==GameScreen.Fail&&!CanOfferStrongerWood?new Color(.98f,.74f,.32f):Paper;
                resultRoot.Find("Nextdelivery").GetComponentInChildren<TextMeshProUGUI>().SetCopy(CanOfferStrongerWood?"More wood":journeyActive&&node=="C160"?"Light the beacon":"Next delivery");
            }
        }
        static void PlaceControl(Button button,Transform parent,Vector2 min,Vector2 max)
        { var r=(RectTransform)button.transform; r.SetParent(parent,false); r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero; }
        void ResetMenu()
        {
            foreach(var key in baseTextSizes.Keys.Where(k=>k==null||k.transform.IsChildOf(menuRoot)).ToArray())baseTextSizes.Remove(key);
            foreach(Transform child in menuRoot){ child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var wash=Rect("Warm paper",menuRoot,new Vector2(.018f,.012f),new Vector2(.982f,.988f));menuWash=Background(wash,new Color(1,.96f,.84f,.965f),true);menuWash.sprite=art.panel;menuWash.type=Image.Type.Sliced;
            if(CurrentScreen!=GameScreen.Home&&CurrentScreen!=GameScreen.Dialogue){
                var windowTexture=Resources.Load<Texture2D>("CascadeArt/Craft/WorkshopRoom");
                if(windowTexture!=null){
                    var window=Rect("Painted workshop window",menuRoot,new Vector2(.025f,.78f),new Vector2(.975f,.982f)).gameObject.AddComponent<RawImage>();
                    window.texture=windowTexture;window.uvRect=new Rect(0,.18f,1,.66f);window.raycastTarget=false;
                    Background(Rect("Quiet header veil",menuRoot,new Vector2(.025f,.78f),new Vector2(.975f,.982f)),new Color(1,.96f,.84f,.69f),false);
                }
                var motes=Rect("Window light and drifting dust",menuRoot,new Vector2(.035f,.03f),new Vector2(.965f,.97f)).gameObject.AddComponent<WorldWeatherGraphic>();
                motes.InitializeRegion(this,machine.LivingWorld.Zone);motes.color=new Color(1,.79f,.44f,.20f);
            }
        }
        void DrawMenu()
        {
            ResetMenu();
            if(CurrentScreen==GameScreen.Home) DrawJourney();
            else if(CurrentScreen==GameScreen.Lessons) DrawLessons();
            else if(CurrentScreen==GameScreen.Dialogue) DrawDialogue();
            else if(CurrentScreen==GameScreen.Settings) DrawSettings();
            else if(CurrentScreen==GameScreen.WordLanguage) DrawWordLanguageConfirmation();
            else if(CurrentScreen==GameScreen.SaveRecovery) DrawSaveRecovery();
            else if(CurrentScreen==GameScreen.TravelBag) DrawTravelBag();
            else if(CurrentScreen==GameScreen.Journal) DrawJournal();
            else if(CurrentScreen==GameScreen.ReachChallenges) DrawReachChallenges();
            else if(CurrentScreen==GameScreen.Collection) DrawCollection();
            else if(CurrentScreen==GameScreen.Workshop) DrawWorkshop();
            else if(CurrentScreen==GameScreen.Requests) DrawRequests();
            else if(CurrentScreen==GameScreen.Records) DrawRecords();
            else if(CurrentScreen==GameScreen.Contracts) DrawContracts();
            else if(CurrentScreen==GameScreen.Parcel) DrawParcel();
            else if(CurrentScreen==GameScreen.JourneyEnd)DrawJourneyEnd();
            else if(CurrentScreen==GameScreen.Devices) DrawDevices();
            else if(CurrentScreen==GameScreen.PowerupChallenges) DrawReachChallenges();
            else if(CurrentScreen==GameScreen.Pause) {
                MenuTitle("Take a break", "Your puzzle will wait.");
                var friend=Picture("Mika rests beside the puzzle",menuRoot,new Vector2(.34f,.615f),new Vector2(.66f,.77f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
                friend.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,2),0,.8f);
                Rect("Mika's wooden shelf",menuRoot,new Vector2(.24f,.602f),new Vector2(.76f,.62f)).gameObject.AddComponent<WoodBoardGraphic>();
                MenuButton("Resume",.50f,ShowGame,true); MenuButton("Retry",.40f,()=>{SetScreen(GameScreen.Playing);Retry();});
                MenuButton("Devices",.30f,ShowDevices); MenuButton("Settings",.20f,ShowSettings); MenuButton("Home",.10f,ShowHome);
            }
        }
        void MenuTitle(UiCopy title,UiCopy body)
        {
            var label=Text("Menu title",menuRoot,new Vector2(.08f,.865f),new Vector2(.92f,.955f),32,title);label.font=art.titleFont;label.alignment=TextAlignmentOptions.MidlineLeft;
            var description=Text("Menu description",menuRoot,new Vector2(.08f,.77f),new Vector2(.92f,.855f),17,body);description.alignment=TextAlignmentOptions.MidlineLeft;
        }
        Button MenuButton(UiCopy label,float bottom,Action action,bool primary=false,float left=.08f,float right=.92f)
        {
            var button=Button(label,menuRoot,new Vector2(left,bottom),new Vector2(right,bottom),()=>{feedback.Sound(ToyCue.Select);action();});
            var r=(RectTransform)button.transform;r.pivot=new Vector2(.5f,0);r.sizeDelta=new Vector2(0,52);
            button.GetComponent<Image>().color=primary?new Color(.98f,.74f,.32f):new Color(.94f,.885f,.75f);
            if(label.Key=="Back"||label.Key=="Travel bag")button.GetComponent<Image>().color=Color.clear;return button;
        }
        IReadOnlyList<LevelContent> PreferredLevels()
        {
            if(PreferredMode==PuzzleMode.Shapes) return Catalog.PrimaryLevels.Where(c=>c.Mode==PuzzleMode.Shapes).ToArray();
            return Catalog.LocalizedLevels.Where(c=>c.Locale==Preferences.PuzzleLocale&&c.Mode==PreferredMode).ToArray();
        }
        void DrawLessons()
        {
            menuWash.color=new Color(1,.96f,.84f,.975f);
            var logo=Text("CASCADE",menuRoot,new Vector2(.08f,.86f),new Vector2(.92f,.97f),42,"CASCADE");logo.font=art.titleFont;
            var example=Catalog.GetPrimary("C01").Definition;
            MiniMachine(Rect("Home machine",menuRoot,new Vector2(.25f,.73f),new Vector2(.75f,.85f)),example.InitialSymbolIds,example.TargetSymbolIds);
            Text("Home promise",menuRoot,new Vector2(.08f,.65f),new Vector2(.92f,.72f),18,"One row. Bring it home in order.");
            var levels=PreferredLevels();
            bool resume=Interaction.Attempt.Current.Status==PuzzleStatus.Playing&&Interaction.Attempt.RouteLength>0;
            int currentIndex=levels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node);
            int proposed=Interaction.Attempt.Current.Status==PuzzleStatus.Won?currentIndex+1:Math.Max(0,currentIndex);
            var chosen=levels[Math.Max(0,Math.Min(levels.Count-1,proposed))];
            UiCopy continueText=resume?new UiCopy("Continue {0} · {1}",node.TrimStart('C','0'),new UiCopy(Interaction.Attempt.Mode.ToString())):new UiCopy("Play {0} · {1}",new UiCopy(PreferredMode.ToString()),chosen.Definition.CampaignNodeId.TrimStart('C','0'));
            MenuButton(continueText,.56f,()=>{if(resume)ShowGame();else StartContent(chosen);},true);
            foreach(var mode in new[]{PuzzleMode.Shapes,PuzzleMode.Words,PuzzleMode.Mixed}) {
                int i=(int)mode; // Enum values are not used as persisted IDs.
                float left=.06f+i*.30f;
                var button=MenuButton(mode.ToString(),.45f,()=>SelectMode(mode),mode==PreferredMode,left,left+.28f);
                button.GetComponentInChildren<TextMeshProUGUI>().fontSize=16*(Preferences.LargeText?1.12f:1);
            }
            int pages=Math.Max(1,(levels.Count+3)/4); chapterPage=Math.Max(0,Math.Min(chapterPage,pages-1));
            Text("Chapter",menuRoot,new Vector2(.15f,.35f),new Vector2(.85f,.42f),20,"Choose a delivery");
            var rail=Rect("Chapter rail",menuRoot,new Vector2(.13f,.302f),new Vector2(.89f,.312f));Background(rail,new Color(.94f,.73f,.32f),false);
            for(int i=0;i<4&&chapterPage*4+i<levels.Count;i++) {
                var content=levels[chapterPage*4+i]; var id=content.Definition.CampaignNodeId;float left=.06f+i*.235f;
                var b=MenuButton(id.TrimStart('C','0'),.27f,()=>StartContent(content),false,left,left+.21f);
                if(sessionClears.Contains(id))b.GetComponent<Image>().color=new Color(.59f,.82f,.69f);
            }
            MenuButton("Earlier",.17f,()=>{chapterPage=Math.Max(0,chapterPage-1);DrawMenu();},false,.10f,.47f).interactable=chapterPage>0;
            MenuButton("Later",.17f,()=>{chapterPage=Math.Min(pages-1,chapterPage+1);DrawMenu();},false,.53f,.90f).interactable=chapterPage+1<pages;
            MenuButton("Devices",.065f,ShowDevices,false,.10f,.47f);MenuButton("Map",.065f,ShowJourney,false,.53f,.90f);
            if(PreferredMode==PuzzleMode.Words) {
                var noteRoot=Rect("Word route note",menuRoot,new Vector2(.10f,.006f),new Vector2(.90f,.06f));Background(noteRoot,new Color(1,.96f,.84f,.92f),false);
                var note=Text("Shape-only lesson",noteRoot,Vector2.zero,Vector2.one,15,"26 is a shapes lesson.\nWords can skip it.");note.raycastTarget=false;
            }
        }
        void StartContent(LevelContent content) { LoadLevel(content.Definition,content.Locale); ShowGame(); }
        void StartNextPreferred()
        {
            if(CanOfferStrongerWood){RestartWithStrongerWood();return;}
            int bonus=Catalog.BonusLevels.ToList().FindIndex(c=>c.Definition.PuzzleId==Interaction.Attempt.Level.PuzzleId);
            if(bonus>=0){if(bonus+1<Catalog.BonusLevels.Count)StartContent(Catalog.BonusLevels[bonus+1]);else ShowReachChallenges();return;}
            if(journeyActive) { int nextStop=selectedJourneyStop+1; if(PreferredMode==PuzzleMode.Words && nextStop==25)nextStop++; if(nextStop<Catalog.PrimaryLevels.Count)BeginJourney(nextStop);else SetScreen(GameScreen.JourneyEnd);return; }
            var levels=PreferredLevels();int index=levels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node);
            if(index>=0&&index+1<levels.Count)StartContent(levels[index+1]);else ShowHome();
        }
        int DeviceForCurrentFloor()
        {
            var state=Interaction.Attempt.Current;if(state.Status!=PuzzleStatus.Playing)return 0;var f=Interaction.Attempt.Level.Floors[state.ActiveFloor];
            return f.Powerup!=null?(f.Powerup.Kind==PowerupKind.Ribbon?9:10):f.Bomb!=null?2:f.Key!=null?3:f.Repair!=null?4:f.LinkedPair.HasValue?6:f.OneUseBridgeEdge.HasValue?7:f.OptionalStamp!=null?8:f.FragileEdges.Count>0?5:f.EntryTransform!=EntryTransform.None?1:0;
        }
        void DrawDevices()
        {
            bool wind=deviceIndex==1&&Interaction.Attempt.Direction?.WindFloor>=1;
            MenuTitle(wind?"Wind on landing":DeviceTitles[deviceIndex],new UiCopy("Device guide · {0} / {1}",deviceIndex+1,DeviceTitles.Length));
            var guideCaption=(RectTransform)menuRoot.Find("Menu description");guideCaption.anchorMin=new Vector2(.08f,.75f);guideCaption.anchorMax=new Vector2(.92f,.80f);
            var fixture=Catalog.PrimaryLevels.Concat(Catalog.BonusLevels).First(c=>c.Definition.CampaignNodeId==DeviceNodes[deviceIndex]).Definition;
            if(wind)fixture=WindGuide(Interaction.Attempt.Direction.WindRight);
            LiveDeviceDemo(Rect("Device example",menuRoot,new Vector2(.10f,.555f),new Vector2(.90f,.735f)),fixture,deviceIndex);
            var explanation=Text("Device explanation",menuRoot,new Vector2(.10f,.38f),new Vector2(.90f,.59f),20,wind?"Wind pushes the whole row one place when it lands. Follow the arrow. Take your time to think.":DeviceCopy[deviceIndex]);explanation.alignment=TextAlignmentOptions.MidlineLeft;
            MenuButton("Previous",.29f,()=>{deviceIndex=(deviceIndex+DeviceTitles.Length-1)%DeviceTitles.Length;DrawMenu();},false,.10f,.47f);
            MenuButton("Next",.29f,()=>{deviceIndex=(deviceIndex+1)%DeviceTitles.Length;DrawMenu();},false,.53f,.90f);
            MenuButton(wind?new UiCopy("Back to puzzle"):new UiCopy("Try level {0}",DeviceNodes[deviceIndex].TrimStart('C','0')),.18f,()=>{if(wind)CloseMenu();else StartContent(Catalog.PrimaryLevels.Concat(Catalog.BonusLevels).First(c=>c.Definition.CampaignNodeId==DeviceNodes[deviceIndex]));},true);
            MenuButton("Back",.07f,CloseMenu);
        }
        void DrawSettings()
        {
            MenuTitle("Make yourself at home",settingsPage==0?"Sound & motion":settingsPage==1?"Comfort & help":"Languages");
            if(settingsPage<2){
                var friend=Picture("Mika by the settings",menuRoot,new Vector2(.41f,.672f),new Vector2(.59f,.766f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
                friend.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1.5f),0,.85f);
                Rect("Settings shelf",menuRoot,new Vector2(.34f,.664f),new Vector2(.66f,.674f)).gameObject.AddComponent<WoodBoardGraphic>();
            }
            if(settingsPage==0) {
                Setting("Music",Preferences.Music,.60f,()=>Preferences.Music=!Preferences.Music);
                Setting("Sound effects",Preferences.Sound,.515f,()=>Preferences.Sound=!Preferences.Sound);
                Setting("Haptics",Preferences.Haptics,.43f,()=>Preferences.Haptics=!Preferences.Haptics);
                Setting("Reduced motion",Preferences.ReducedMotion,.345f,()=>Preferences.ReducedMotion=!Preferences.ReducedMotion);
                Setting("Screen shake",Preferences.ScreenShake,.26f,()=>Preferences.ScreenShake=!Preferences.ScreenShake);
                MenuButton(new UiCopy("Effects: {0}",new UiCopy(new[]{"Off","Low","Full"}[Preferences.EffectQuality])),.175f,()=>{Preferences.EffectQuality=(Preferences.EffectQuality+1)%3;ApplyPreferences();DrawMenu();});
            } else if(settingsPage==1) {
                Setting("High contrast",Preferences.HighContrast,.60f,()=>Preferences.HighContrast=!Preferences.HighContrast);
                Setting("Larger text",Preferences.LargeText,.50f,()=>Preferences.LargeText=!Preferences.LargeText);
                Setting("Precise input",Preferences.PreciseInput,.40f,()=>Preferences.PreciseInput=!Preferences.PreciseInput);
                Setting("Tutorial help",Preferences.Tutorials,.30f,()=>Preferences.Tutorials=!Preferences.Tutorials);
                Setting("Dead-end warning",Preferences.DeadEndWarnings,.20f,()=>Preferences.DeadEndWarnings=!Preferences.DeadEndWarnings);
            }
            if(settingsPage==2)DrawLanguages();
            MenuButton(settingsPage==0?"Comfort":settingsPage==1?"Languages":"Sound & motion",settingsPage==2?.025f:.085f,()=>{settingsPage=(settingsPage+1)%3;DrawMenu();},false,.10f,.55f);
            MenuButton("Back",settingsPage==2?.025f:.085f,CloseMenu,false,.59f,.90f);
        }
        void Setting(string label,bool value,float bottom,Action toggle)
        {
            var button=MenuButton(UiCopy.Raw(label+": "+(value?"On":"Off")),bottom,()=>{toggle();ApplyPreferences();DrawMenu();});
            button.GetComponent<Image>().color=new Color(.97f,.92f,.78f);
            var text=button.GetComponentInChildren<TextMeshProUGUI>();text.SetCopy(label);text.alignment=TextAlignmentOptions.MidlineLeft;
            text.rectTransform.anchorMax=new Vector2(.72f,1);text.margin=new Vector4(16,2,3,2);
            var track=Rect("Switch",button.transform,new Vector2(.77f,.28f),new Vector2(.95f,.72f));var trackImage=Background(track,value?new Color(.32f,.68f,.61f):new Color(.55f,.58f,.49f),false);trackImage.sprite=art.panel;trackImage.type=Image.Type.Sliced;
            var knob=Rect("Knob",track,new Vector2(value?.52f:.04f,.09f),new Vector2(value?.96f:.48f,.91f));var knobImage=Background(knob,Paper,false);knobImage.sprite=art.panel;knobImage.type=Image.Type.Sliced;
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
                    shape.SetShape(art.SymbolMesh(symbols[i]),art.SymbolMaterial(symbols[i]).GetColor("_BaseColor"));
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
            if(Preferences.EffectQuality==0)feedback.ClearTails();
            if(!Preferences.DeadEndWarnings)CancelDeadEnd();
            if(Coach!=null)Coach.Enabled=Preferences.Tutorials;
            foreach(var entry in baseTextSizes.ToArray()) {
                if(entry.Key==null){baseTextSizes.Remove(entry.Key);continue;}
                entry.Key.fontSize=entry.Value*(Preferences.LargeText&&!IsSymbolFace(entry.Key.name)?1.12f:1);
                entry.Key.color=Preferences.HighContrast?new Color(.06f,.17f,.18f):Ink;
            }
        }
        static bool IsSymbolFace(string label)=>label=="Required letter"||label=="Symbol"||label=="TargetSymbol"||label=="DeviceGlyph"||label=="ZoomSymbol";
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
