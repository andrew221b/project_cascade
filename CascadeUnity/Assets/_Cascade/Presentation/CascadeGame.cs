using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Solver;
using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame : MonoBehaviour
    {
        [SerializeField] string initialNode = "C01";
        [SerializeField] Camera uiCamera;
        [SerializeField] TMP_FontAsset font;
        ToyArt art;
        ToyMachineView machine;
        PuzzleEventPlayer eventPlayer;
        ToyFeedbackPool feedback;
        bool disabling;
        public bool CanPresent=>!disabling&&isActiveAndEnabled;
        MotionProfile motion;
        public PuzzleEventPlayer EventPlayer => eventPlayer;
        public ToyFeedbackPool Feedback => feedback;
        public ToyMachineView Machine => machine;
        public PresentationFrame VisibleFrame { get; private set; }
        readonly Vector3[] glyphPositions = new Vector3[8];
        readonly Vector3[] flightCorners = new Vector3[4];
        public bool ReducedMotion { get => machine != null && machine.ReducedMotion; set { if (machine != null) machine.ReducedMotion = value; } }
        public PuzzleInteraction Interaction { get; private set; }
        public ContentCatalog Catalog { get; private set; }
        bool forcePreciseInput;
        public bool ForcePreciseInput { get => forcePreciseInput; set { forcePreciseInput = value; RenderControls(); } }
        public bool PreciseInputActive { get; private set; }
        public IReadOnlyList<TokenSlot> SlotViews => slots;
        public RectTransform TargetRow => targetRow;
        RectTransform safeRoot, header, actions, floorArea, receiver, tokenRow, targetRow, precise, targetZoom;
        TextMeshProUGUI levelLabel, status, focusLabel, receiverLabel;
        UnityEngine.UI.Button undo, retry, drop, hint, help, pause, previous, next, swapLeft, swapRight;
        readonly List<RectTransform> floorViews = new List<RectTransform>();
        readonly List<TextMeshProUGUI> floorLabels = new List<TextMeshProUGUI>();
        readonly List<TextMeshProUGUI> deviceLabels = new List<TextMeshProUGUI>();
        readonly List<TokenSlot> slots = new List<TokenSlot>();
        readonly List<UnityEngine.UI.Image> slotImages = new List<UnityEngine.UI.Image>();
        readonly List<TextMeshProUGUI> tokenLabels = new List<TextMeshProUGUI>();
        readonly List<TextMeshProUGUI> targetLabels = new List<TextMeshProUGUI>();
        Task<HintOffer> hintTask;
        CancellationTokenSource hintCancellation;
        HintOffer offeredHint;
        bool hintVisible;
        public bool HintVisible=>hintVisible&&offeredHint!=null&&Interaction.Attempt.IsCurrent(offeredHint);
        bool userPaused, zoomOpen, applicationPaused;
        bool applicationFocused = true;
        string node;
        Rect safeArea;
        int screenWidth, screenHeight;
        static readonly Color Ink = new Color(.16f, .28f, .29f);
        static readonly Color Paper = new Color(1, .96f, .84f);
        static readonly Color Selected = new Color(.95f, .73f, .29f, .3f);

        static string Source(string name) => Resources.Load<TextAsset>("CascadeContent/" + name)?.text
            ?? throw new InvalidOperationException("Missing CASCADE content: " + name);
        void Awake()
        {
            AndroidBackBridge.Register();
            ReadProfile();
            InitializeAudiencePolicy();
            InitializeUiLanguage();
            Catalog = ContentImporter.Import(Source("levels"), Source("localized_levels"), Source("words_en"), Source("words_ru"), Source("words_uk"),Source("campaign_extension"));
            string stageWords=Source("stage_words_v1"),stageTemplates=Source("stage_variety_templates_v1"),stageRecipes=Source("production_triad_recipes");
            stageVarietyReady=System.Threading.Tasks.Task.Run(()=>new StageVarietyStore(stageWords,stageTemplates,stageRecipes));
            art = ToyArt.Load(); font = art.bodyFont;
            if (uiCamera == null) uiCamera = Camera.main;
            machine = gameObject.AddComponent<ToyMachineView>(); machine.Initialize(art, uiCamera);
            motion = Resources.Load<MotionProfile>("CascadeArt/MotionProfile") ?? throw new InvalidOperationException("Build CASCADE motion assets before playing.");
            feedback = gameObject.AddComponent<ToyFeedbackPool>(); feedback.Initialize(this, art, motion);
            eventPlayer = gameObject.AddComponent<PuzzleEventPlayer>(); eventPlayer.Initialize(this, motion, feedback);
            gameObject.AddComponent<PhoneShakeFeedback>().Initialize(this);
            BuildScreen();BuildControlArt();BuildFieldTools(); LoadNode(initialNode); BuildFlow(); RestoreProfile();InitializeLocalReminders();InitializeAudienceServices();
            StartCoroutine(WarmJourneyResources());
        }
        void OnEnable() { disabling=false;if (Interaction != null) { ResumeDirectionWork();ResumeDescentWork();if(!preparation.IsReady&&!preparation.IsPending)BeginPreparation(); Interaction.SetSuspended(nativeAdSuspended||applicationPaused || !applicationFocused||landscapeLayoutBlocked); RenderState(); } }
        void OnDisable()
        {
            if (Interaction == null) { disabling = true; feedback?.ClearTails(); return; }
            disabling=true;CloseLegalDocument();CancelConversation();
            CloseLandingPreview();ResetMechanicDiscovery();
            PersistLifecycleProgress();
            SettleDescent();CancelDescentWork(true);CancelDirectionWork(true);
            EndTokenHold(); CancelHint(); CancelDeadEnd(); preparation.Cancel(); StopPresentation(); Interaction?.SetSuspended(true); feedback?.ClearTails();
        }
        void OnDestroy() { AndroidBackBridge.Shutdown();CancelMissionPlan(); DisposeCloudSave(); DisposeMikaCosmetics();DisposeScreenCosmetics();DisposeConversationArt(); mechanicArrival?.Kill();previewArrival?.Kill();coinArrival?.Kill();if(introBranch!=null)Destroy(introBranch);if(purchaseStore!=null){purchaseStore.Changed-=NotifyStoreChanged;purchaseStore.CoinsGranted-=QueuePurchasedCoins;}DisposeNavigationArt();if(treasureArt!=null)foreach(var sprite in treasureArt)if(sprite!=null)Destroy(sprite);if(travelArt!=null)foreach(var sprite in travelArt)if(sprite!=null)Destroy(sprite);DisposeCompletionArt();DisposePlatformProps(); preparation.Dispose(); if (Interaction != null) Interaction.Committed -= OnCommitted; if(glyphMaterial!=null)Destroy(glyphMaterial);if(tutorialOverlayMaterial!=null)Destroy(tutorialOverlayMaterial);if(toolSilhouette!=null)Destroy(toolSilhouette);foreach(var sprite in toolBodySprites.Values)if(sprite!=null)Destroy(sprite);foreach(var sprite in enamelControls)if(sprite!=null)Destroy(sprite); }
        void OnApplicationFocus(bool focused)
        {
            if(!focused){CancelMissionPlan();SettleDescent();}
            if(!focused){AndroidBackBridge.Clear();PersistLifecycleProgress();}else AndroidBackBridge.Register();
            if(!focused)absentSince=Time.realtimeSinceStartup;
            else if(absentSince>0 && Time.realtimeSinceStartup-absentSince>60 && CurrentScreen==GameScreen.Playing)Coach?.Advance(15,true);
            applicationFocused = focused;ObserveSessionPresence(DateTime.UtcNow); Interaction?.SetSuspended(nativeAdSuspended||applicationPaused || !applicationFocused||landscapeLayoutBlocked); if (!focused) { EndTokenHold(); StopPresentation(); }
            if (Interaction != null&&focused&&!applicationPaused) RenderState();
        }
        void OnApplicationPause(bool paused)
        {
            if(paused){CancelMissionPlan();SettleDescent();}
            if(paused){AndroidBackBridge.Clear();PersistLifecycleProgress();}
            applicationPaused = paused;ObserveSessionPresence(DateTime.UtcNow); SetLocalReminderBackground(paused); Interaction?.SetSuspended(nativeAdSuspended||applicationPaused || !applicationFocused||landscapeLayoutBlocked); if (paused) { EndTokenHold(); StopPresentation(); }
            if (Interaction != null&&!paused&&applicationFocused) RenderState();
        }
        void Update()
        {
            // Entering Play Mode while scripts reload can leave this editor instance without
            // its runtime-only attempt. Wait for a fresh Play session instead of flooding
            // the Console or advancing an incomplete presentation.
            if (Interaction == null || machine == null) return;
            PollSystemBack();
            illustratedNumberScan-=Time.unscaledDeltaTime;
            if(illustratedNumberScan<=0){illustratedNumberScan=.35f;ApplyMenuSpriteCounters();}
            machine.LivingWorld.CosmeticStage=Mathf.Max(0,selectedJourneyStop*3+(descent?.StageIndex??0));
            ObserveSaveFailure();TryResetInactiveSession();TickPreviewAd();TickConversation();cloudSave?.Tick();FlushStoreFeedback();
            if (safeArea != Screen.safeArea || screenWidth != Screen.width || screenHeight != Screen.height) {
                SettleDescent();
                StopPresentation(); ApplySafeArea(); if (Interaction != null) RenderState();RefreshConversationLayout();
            }
            if(CurrentScreen==GameScreen.Playing&&Interaction.CanPlay&&!zoomOpen&&!MechanicPopupOpen&&landingPreview==null&&previewAd==null&&!fieldTrayOpen&&applicationFocused&&!applicationPaused&&!nativeAdSuspended)
            {
                float seconds=Mathf.Min(Time.unscaledDeltaTime,.5f);
                Interaction.Attempt.AdvanceActiveTime(seconds);unavailableHints.Advance(Interaction.Attempt,seconds);
            }
            TickUnavailableHintWait();UpdateFlow();
            TickToolGuide();
            if(applicationFocused&&!applicationPaused&&monetization!=null)monetization.AdvanceForeground(Mathf.Min(Time.unscaledDeltaTime,.5f));
            UpdatePreparation();
            UpdateDirection();
            UpdateDescent();
            TickPersonalMissions();
            if (hintTask == null || !hintTask.IsCompleted || !Interaction.CanPlay) return;
            var completed = hintTask; hintTask = null; hintCancellation?.Dispose(); hintCancellation = null;
            var fallbackAttempt=unavailableHintAttempt;string fallbackState=unavailableHintState;
            unavailableHintAttempt=System.Guid.Empty;unavailableHintState=null;
            if (completed.IsFaulted) { var ignored = completed.Exception; if(PendingLesson==MechanicLesson.Hint)DeferLessonPractice(); status.SetCopy("Hint unavailable. You can keep playing."); RenderControls(); return; }
            if (completed.IsCanceled) return;
            var offer = completed.Result;
            var attempt=Interaction.Attempt;
            if(!attempt.IsCurrent(offer))return;
            if(offer.Result.Status==HintStatus.DeadEnd){
                if(PendingLesson==MechanicLesson.Hint)DeferLessonPractice();
                knownDeadEnd=true;status.SetCopy(DeadEndCopy);RenderControls();return;
            }
            bool revealed;
            bool rewardedDue=!attempt.GuideActive&&RewardedHintDue(attempt);
            if(offer.Result.Status==HintStatus.Step&&fallbackAttempt==attempt.AttemptId&&fallbackState==offer.StateHash)
                revealed=unavailableHints.TryReveal(attempt,offer,()=>{PersistProgress(true);return !HasSaveError;});
            else if(offer.Result.Status==HintStatus.Step&&rewardedDue&&GameEconomy.Ledger.HintTokens>0)
                revealed=GameEconomy.TrySpendHint(attempt,offer);
            else if(offer.Result.Status==HintStatus.Step&&!attempt.GuideActive&&!rewardedDue&&attempt.CanShowFreeHint)
                revealed=attempt.TryRevealFreeHint(offer,()=>{PersistProgress(true);return !HasSaveError;});
            else if(offer.Result.Status==HintStatus.Step&&!attempt.GuideActive)
                revealed=GameEconomy.TrySpendHint(attempt,offer);
            else {revealed=attempt.RevealHint(offer);if(revealed)PersistProgress();}
            if(!revealed){RenderControls();return;}
            if(offer.Result.Status==HintStatus.Step&&fallbackAttempt==attempt.AttemptId&&fallbackState==offer.StateHash)
                reactionBanner.Show(ReferenceCopy("No video. This hint is free.","Нет видео. Подсказка бесплатно.","Немає відео. Підказка безкоштовна."),"");
            offeredHint = offer;hintVisible=offer.Result.Status==HintStatus.Step;
            if(PendingLesson==MechanicLesson.Hint&&offer.Result.Status!=HintStatus.Step)DeferLessonPractice();
            else if(PendingLesson==MechanicLesson.Hint)SyncLessonRestriction();
            status.SetCopy(offer.Result.Status == HintStatus.Step ? new UiCopy("Try this: {0}", CommandText(offer.Result.Command.Value))
                : offer.Result.Status == HintStatus.Unknown ? "Search limit reached. You can keep playing." : "Target delivered.");
            RenderControls();
        }
        internal string DisplayLevelNumber(string nodeId)
        {
            if(Catalog!=null){var offered=PreferredLevels();for(int index=0;index<offered.Count;index++)
                if(offered[index].Definition.CampaignNodeId==nodeId)return (index+1).ToString();}
            if(Catalog!=null)for(int index=0;index<Catalog.PrimaryLevels.Count;index++)
                if(Catalog.PrimaryLevels[index].Definition.CampaignNodeId==nodeId)return (index+1).ToString();
            return nodeId?.TrimStart('C','0')??"";
        }
        public void LoadNode(string campaignNode)
        {
            var content = Catalog.GetPrimary(campaignNode); LoadLevel(content.Definition, content.Locale); node = campaignNode;
            levelLabel.SetCopy(new UiCopy("Level {0}", DisplayLevelNumber(campaignNode))); RenderControls();
        }
        public void LoadLevel(LevelDefinition level, string locale = "en")
        {
            MountAttempt(new AttemptService(level,locale));PersistProgress();
        }
        void MountAttempt(AttemptService attempt,DescentRun run=null)
        {
            deferredLessons.Clear();
            ResetMechanicDiscovery();
            reactionBanner?.Clear();
            feedback?.ClearTails();
            SettleDescent();CancelDescentWork();CancelDirectionWork();descent=run;descentPlanFailed=false;descentTravel=false;paintedDescentStage=-1;machine.LivingWorld.DescentDepth=run?.StageIndex??0;
            var level=attempt.Level;
            EndTokenHold(); CancelHint(); CancelDeadEnd(); StopPresentation(); if (Interaction != null) Interaction.Committed -= OnCommitted;
            Interaction = new PuzzleInteraction(attempt); Interaction.Committed += OnCommitted;
            needsMechanicScan=attempt.Current.Status==PuzzleStatus.Playing;
            Interaction.SetSuspended(nativeAdSuspended||applicationPaused || !applicationFocused||landscapeLayoutBlocked);
            BeginPreparation();
            journeyActive=false; machine.SetZone(JourneyStory.Zone(Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==level.CampaignNodeId)));
            userPaused = zoomOpen = false; targetZoom.gameObject.SetActive(false);
            node = level.CampaignNodeId; levelLabel.SetCopy(new UiCopy("Level {0}", DisplayLevelNumber(node)));
            RebuildLevelViews(level); RenderState(); StartCoach(); if(flowReady)SetScreen(GameScreen.Playing);
        }
        void OnCommitted(TransitionResult result)
        {
            machine.NotifyWildlifeInput();
            ObserveLessonTransition(result);
            TransferTouchPoseToExchange();
            CancelHint(); Coach?.Observe(result.State,Interaction.Attempt.RouteLength);
            if(node=="C01")firstGestureShown=true;
            RecordCommittedProgress();
            float lead=toolCueLeadIn;toolCueLeadIn=0;
            eventPlayer.Play(result,lead); ScheduleDeadEnd(); RenderControls();
        }
        void StopPresentation()
        {
            if(fieldTrayOpen||fieldTrayTween!=null)SetToolTray(false,true);
            StopToolAppearance();
            feedback?.RowPhysics?.CancelShake(true);
            EndTokenHold(); if (eventPlayer != null) eventPlayer.Skip(); else Interaction?.FinishPresentation();
        }
        public void ReportInput(TransitionResult result)
        {
            Coach?.Activity();
            if(result!=null&&result.Accepted&&(Interaction.LastRequestedCommand?.Kind==CommandKind.Swap||Interaction.LastRequestedCommand?.Kind==CommandKind.LongReach))
                RetireInstruction("Drag a piece onto its neighbour.");
            if (result != null && !result.Accepted) {
                Coach?.Invalid(result.Rejection,Interaction.LastRequestedCommand?.Edge??-1);
                status.SetCopy(result.Rejection == RejectionReason.NoOp
                    ? "These pieces are the same." : "This way is closed. Try the other side.");
                feedback.Sound(ToyCue.Reject);
            }
            else if (result == null) RenderFocus();
        }
        public void PreviewSlot(int slot)
        {
            var preview = Interaction.PreviewNeighbour(slot);
            if (preview != null) ShowPreview(PuzzleCommand.Swap(Math.Min(slot, Interaction.SelectedSlot.Value)));
        }
        public void PreviewDrag(int from, int to)
        {
            if(Interaction.PracticeRestricted&&!Interaction.AllowsSlot(to))to=-1;
            if(previewNeighbour==to)return;
            if(to>=0 && Math.Abs(from-to)>=2 && Math.Abs(from-to)<=3 && Interaction.Attempt.Current.ReachCharges>0)ShowPreview(PuzzleCommand.Reach(from,to));
            else if (Math.Abs(from - to) == 1 && to >= 0) ShowPreview(PuzzleCommand.Swap(Math.Min(from, to)));
            else { ClearDeviceForecast(); machine.PreviewForecast(null); status.SetCopy("Move onto the piece next to it."); }
            HighlightDragNeighbour(from, to); RestoreTouchPose();
        }
        void ShowPreview(PuzzleCommand command)
        {
            ClearDeviceForecast();
            if(!Interaction.AllowsCommand(command)){machine.PreviewForecast(null);return;}
            var preview = Interaction.Attempt.Resolver.Preview(Interaction.Attempt.Current, command);
            if (!preview.IsLegal) { status.SetCopy(preview.Rejection == RejectionReason.NoOp ? "No change. Free to cancel" : "Closed edge. Free to cancel"); return; }
            var prediction = Interaction.Attempt.Predict(command);
            UiCopy exit = prediction.State.ActiveFloor != Interaction.Attempt.Current.ActiveFloor || prediction.State.Status != PuzzleStatus.Playing ? "then down" : "";
            var effects = new List<UiCopy>{new UiCopy("Board loses {0}",preview.DurabilityCost)};
            foreach (var e in prediction.Events.Where(e => e.Floor == Interaction.Attempt.Current.ActiveFloor)) {
                if (e.Kind == PuzzleEventKind.Repaired) effects.Add(new UiCopy("repair +{0}",e.Value));
                if (e.Kind == PuzzleEventKind.BombExploded) effects.Add(new UiCopy("bomb −{0}",e.Value));
                if (e.Kind == PuzzleEventKind.BombDefused) effects.Add("defuse");
                if (e.Kind == PuzzleEventKind.KeyOpened) effects.Add("unlock");
                if(e.Kind==PuzzleEventKind.RibbonCollected)effects.Add("One far swap ready");
                if(e.Kind==PuzzleEventKind.HourglassCollected)effects.Add("Bomb paused for two swaps");
            }
            if(!string.IsNullOrEmpty(exit.Key))effects.Add(exit);
            if(prediction.Events.Any(e=>e.Kind==PuzzleEventKind.EntryPermuted&&IsWindFloor(e.Floor)))
                effects.Add("The wind moved the row");
            status.SetCopy(UiCopy.Join(", ",effects));
            RenderFocus(); machine.PreviewForecast(prediction); PreviewDeviceDocks(prediction);
            // Forecasts belong to the actual pieces and hanging devices, never rectangular slot panels.
        }
        public void Undo()
        {
            if(!Interaction.AllowsControl(PracticeControl.Undo))return;
            if(!Interaction.Attempt.CanUndo)return;
            if(RequireActionLesson(Cascade.Application.MechanicLesson.Undo))return;
            bool firstLesson=PendingLesson==MechanicLesson.Undo&&!LessonWasSeen(MechanicLesson.Undo);
            if (!Interaction.Undo(firstLesson)) return;
            EndTokenHold();CompleteLessonPractice(Cascade.Application.MechanicLesson.Undo,false);
            PersistProgress();CancelHint();ScheduleDeadEnd();
            if(CurrentScreen==GameScreen.Fail){
                var before=VisibleFrame;SetScreen(GameScreen.Playing);RenderFrame(before);
            }
            eventPlayer.PlayUndo();RenderControls();
        }
        public void SettleExchangeForNewTouch()=>eventPlayer?.SettleExchangeForInput();
        public void Retry()
        {
            if(!Interaction.AllowsRetry)return;
            bool retryingFailedLevel=CurrentScreen==GameScreen.Fail&&Interaction.Attempt.Current.Status==PuzzleStatus.Lost;
            if(descentPlanFailed){
                var source=descentSource;string locale=descentLocale;var mood=descentMood;
                bool travel=descentTravel,journey=journeyActive;var screen=CurrentScreen;
                StartDescentLevel(source,locale,mood,descentMode);descentTravel=travel;journeyActive=journey;
                if(flowReady)SetScreen(screen);RenderControls();return;
            }
            if(Interaction.IsPreparing){if(!preparation.IsPending)BeginPreparation();RenderControls();return;}
            if(descent?.Version==7&&RetryVariedStage())return;
            var attemptId=Interaction.Attempt.AttemptId;
            bool hadMoves=Interaction.Attempt.Current.Status==PuzzleStatus.Playing&&Interaction.Attempt.RouteLength>0;
            if (!Interaction.Retry()) return;
            ResetMechanicDiscovery();
            if(journeyActive&&!sessionClears.Contains(node))profile.Director.ObserveRetry(attemptId,hadMoves);
            PersistProgress(); CancelHint(); CancelDeadEnd(); StartCoach(); RenderState(); if(flowReady)ShowGame();
        }
        UiCopy DeadEndCopy=>Interaction.Attempt.CanUndo?(UiCopy)"No path. Undo.":"No path. Retry.";
        public void RequestHint()
        {
            if (saveBlocked||HasSaveError||!persistenceReady||!Interaction.CanPlay || hintTask != null || !Interaction.AllowsControl(PracticeControl.Hint)) return;
            if(offeredHint?.Result.Status==HintStatus.Step&&Interaction.Attempt.IsCurrent(offeredHint)){
                hintVisible=!hintVisible;RenderControls();return;
            }
            if(knownDeadEnd){status.SetCopy(DeadEndCopy);RenderControls();return;}
            bool unavailableHint=false;
            if(!Interaction.Attempt.GuideActive&&RewardedHintDue(Interaction.Attempt)&&GameEconomy.Ledger.HintTokens<=0){
                if(HintVideoUnavailable){if(!TryUnavailableHint(Interaction.Attempt))return;unavailableHint=true;}
                else {if(adProvider?.RewardLoading==true)unavailableHints.WaitForVideo(Interaction.Attempt);if(PendingLesson==MechanicLesson.Hint)DeferLessonPractice();OfferStageHintVideo();return;}
            }
            if(!unavailableHint&&!Interaction.Attempt.GuideActive&&!Interaction.Attempt.CanShowFreeHint
                &&!(Interaction.Attempt.CanShowBonusHint&&(HasHintForCurrentState(Interaction.Attempt)||GameEconomy.Ledger.HintTokens>0))){
                if(PendingLesson==MechanicLesson.Hint)DeferLessonPractice();
                OfferStageHintVideo();return;
            }
            if(!Interaction.Attempt.GuideActive&&RequireActionLesson(Cascade.Application.MechanicLesson.Hint))return;
            CancelHint(); var request = Interaction.Attempt.CaptureHintRequest();
            if(unavailableHint){pendingHintVideoState=null;unavailableHintAttempt=Interaction.Attempt.AttemptId;unavailableHintState=Interaction.Attempt.CurrentStateHash;}
            hintCancellation = new CancellationTokenSource(); var token = hintCancellation.Token;
            hintTask = Task.Run(() => AttemptService.CalculateHint(request, cancellationToken: token)); status.SetCopy("Looking for one step…"); RenderControls();
        }
        public void RefreshLayout() {
            Canvas.ForceUpdateCanvases(); ApplySafeArea();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(safeRoot); Canvas.ForceUpdateCanvases(); FitJourneyFooter();RefreshConversationLayout(); Canvas.ForceUpdateCanvases();
            if(uiCamera!=null&&machine!=null&&floorViews.Count>0&&floorViews[0]!=null&&status!=null) {
                floorViews[0].GetWorldCorners(flightCorners);float floorTop=flightCorners[2].y;
                status.rectTransform.GetWorldCorners(flightCorners);float statusBottom=flightCorners[0].y;
                float inset=uiCamera.orthographicSize*2/Mathf.Max(1,uiCamera.pixelHeight)*6;
                machine.LivingWorld.SetFlightBand(floorTop+inset,statusBottom-inset);
            }
            machine?.Sync(); SyncTokenFaces(); SyncDeviceSuspension(0); RenderControls();
            feedback?.RefreshAnchors();reactionBanner?.RefreshLayout();
            RestoreTouchPose();
            PaintGameplayTextContrast();
            safeRoot.GetComponentsInChildren(false,buttonDesigns);foreach(var design in buttonDesigns)design.Refresh();
            if(worldComic>=0&&CurrentScreen==GameScreen.Prologue)menuRoot.GetComponentInChildren<WorldComicLayout>()?.RefreshLayout();
        }
        void CancelHint()
        {
            unavailableHintAttempt=System.Guid.Empty;unavailableHintState=null;
            hintCancellation?.Cancel(); hintCancellation?.Dispose(); hintCancellation = null;
            if (hintTask != null) hintTask.ContinueWith(t => { var ignored = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            hintTask = null; offeredHint = null;hintVisible=false;
        }
        void MoveFocus(int direction)
        {
            if(Interaction.PracticeRestricted&&!Interaction.PracticeGuidanceOnly&&Interaction.PracticeCommand.HasValue){
                var required=Interaction.PracticeCommand.Value;
                if(required.Kind!=CommandKind.EarlyDrop)Interaction.FocusSlot(direction<0?required.Edge:required.Kind==CommandKind.LongReach?required.End:required.Edge+1);
                RenderFocus();return;
            }
            int selected = Interaction.SelectedSlot ?? 0;
            Interaction.FocusSlot(Math.Max(0, Math.Min(slots.Count - 1, selected + direction))); RenderFocus();
        }
        void TogglePause()
        {
            if(legalOverlay!=null){CloseLegalDocument();return;}
            if(nativeAdSuspended)return;
            if(MechanicPopupOpen){AcknowledgeMechanic();return;}
            if(flowReady) { if(CurrentScreen==GameScreen.Pause)ShowGame();else SetScreen(GameScreen.Pause);return; }
            userPaused = !userPaused; Interaction.SetPaused(userPaused || zoomOpen);
            if (userPaused) StopPresentation(); RenderState();
        }
        void ToggleTargetZoom()
        {
            zoomOpen = !zoomOpen; Interaction.SetPaused(userPaused || zoomOpen); targetZoom.gameObject.SetActive(zoomOpen);
            if (zoomOpen) {
                foreach (Transform child in targetZoom) if (child.name.StartsWith("ZoomToken")) Destroy(child.gameObject);
                var row = Rect("ZoomTokens", targetZoom, new Vector2(.08f, .35f), new Vector2(.92f, .67f)); row.name = "ZoomTokenRow";
                var symbols = Interaction.Attempt.Level.TargetSymbolIds;
                for (int i = 0; i < symbols.Count; i++) {
                    int column = i % 4, line = i / 4;
                    var cell = Rect("ZoomToken" + i, row, new Vector2(column * .25f, 1 - (line + 1) * .5f), new Vector2((column + 1) * .25f, 1 - line * .5f));
                    if (symbols[i].Kind == SymbolKind.Shape) {
                        var face = Rect("ShapeFace", cell, new Vector2(.12f, .30f), new Vector2(.88f, .90f)).gameObject.AddComponent<ShapeFaceGraphic>();
                        face.SetShape(art.SymbolMesh(symbols[i]), art.SymbolMaterial(symbols[i]).GetColor("_BaseColor"));
                        Text("ShapeName", cell, new Vector2(0, .15f), new Vector2(1, .32f), 12, DisplaySymbol(symbols[i]));
                    } else {
                        var label = Text("ZoomSymbol", cell, new Vector2(.06f, .15f), new Vector2(.94f, .83f), 34, symbols[i].Value); label.font = art.titleFont;
                    }
                    Text("Position", cell, new Vector2(0, 0), new Vector2(1, .18f), 13, (i + 1).ToString());
                }
            }
            if(zoomOpen)foreach(var label in targetZoom.GetComponentsInChildren<TextMeshProUGUI>())label.color=label.GetComponentInParent<UnityEngine.UI.Button>()!=null?Ink:MenuPaper;
            RenderControls();
        }
        UiCopy DisplaySymbol(SymbolId symbol) => symbol.Kind == SymbolKind.Shape ? new[] { "triangle", "circle", "square", "diamond", "star", "hexagon" }[art.ShapeIndex(symbol)] : UiCopy.Raw(symbol.Value);
        static string FaceText(SymbolId symbol) => symbol.Kind == SymbolKind.Glyph ? symbol.Value : "";
        static UiCopy CommandText(PuzzleCommand command) => command.Kind == CommandKind.EarlyDrop ? "Drop the row" : new UiCopy("swap slots {0} & {1}",command.Edge+1,command.Kind==CommandKind.LongReach?command.End+1:command.Edge+2);
        RectTransform statusPaper;
        GameReactionBanner reactionBanner;
        public GameReactionBanner ReactionBanner => reactionBanner;
        Material glyphMaterial;
        void FinishLetter(TextMeshProUGUI label)
        {
            if(glyphMaterial==null){
                glyphMaterial=new Material(art.titleFont.material){name="Raised ink on ivory"};
                glyphMaterial.EnableKeyword("UNDERLAY_ON");
                glyphMaterial.SetColor("_UnderlayColor",new Color(.30f,.22f,.13f,.95f));
                glyphMaterial.SetFloat("_UnderlayOffsetX",.27f);
                glyphMaterial.SetFloat("_UnderlayOffsetY",-.35f);
                glyphMaterial.SetFloat("_UnderlayDilate",.20f);
                glyphMaterial.SetFloat("_UnderlaySoftness",0);
            }
            label.fontSharedMaterial=glyphMaterial;
            label.fontStyle=FontStyles.Bold;
            label.alignment=TextAlignmentOptions.Center;
            label.margin=Vector4.zero;
            label.gameObject.AddComponent<SymbolGlyphCenter>();
            label.enableVertexGradient=true;
            label.colorGradient=new VertexGradient(new Color(.29f,.39f,.36f),new Color(.29f,.39f,.36f),new Color(.10f,.22f,.23f),new Color(.10f,.22f,.23f));
        }
        void BuildScreen()
        {
            var canvasRect = Rect("GameCanvas", transform, Vector2.zero, Vector2.one);
            var canvas = canvasRect.gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera; canvas.planeDistance = 5;
            var scaler = canvasRect.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(390, 844); scaler.matchWidthOrHeight = 1;
            canvasRect.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            safeRoot = Rect("SafeArea", canvasRect, Vector2.zero, Vector2.one); ApplySafeArea();
            header = Rect("Header", safeRoot, new Vector2(.05f, 1), new Vector2(.95f, 1));
            var chapterPlaque=Rect("Carved chapter plaque",header,Vector2.zero,new Vector2(.76f,1));var plaque=Background(chapterPlaque,new Color(1,.96f,.84f,.91f),false);plaque.sprite=art.panel;plaque.type=UnityEngine.UI.Image.Type.Sliced;
            levelLabel = Text("Level", header, new Vector2(.045f,0), new Vector2(.74f,1), 17, "CASCADE"); levelLabel.alignment = TextAlignmentOptions.Center; levelLabel.font = art.titleFont;levelLabel.enableAutoSizing=true;levelLabel.fontSizeMin=12;levelLabel.fontSizeMax=17;
            pause = Button("Pause", header, new Vector2(.82f, 0), Vector2.one, TogglePause);
            previous = Button("Home", header, Vector2.zero, new Vector2(.28f, 48f/104), ShowHome);

            next = Button("Settings", header, new Vector2(.72f, 0), new Vector2(1, 48f/104), ShowSettings);
            previous.gameObject.SetActive(false); next.gameObject.SetActive(false);
            pause.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
            var pauseMark = Rect("Pause mark",pause.transform,Vector2.zero,Vector2.one).gameObject.AddComponent<PauseMark>(); pauseMark.raycastTarget=false;
            statusPaper=Rect("Quiet instruction paper",safeRoot,new Vector2(.055f,.79f),new Vector2(.945f,.86f));var paper=Background(statusPaper,Color.white,false);paper.sprite=enamelControls[0];paper.type=UnityEngine.UI.Image.Type.Sliced;
            status = Text("Feedback", statusPaper, Vector2.zero, Vector2.one, 16, "");status.color=MenuPaper;
            BuildInstructionControls();
            reactionBanner=Rect("Tool announcement",safeRoot,Vector2.zero,Vector2.one).gameObject.AddComponent<GameReactionBanner>();
            reactionBanner.Initialize(this,statusPaper,status,art.titleFont);
            floorArea = Rect("Floors", safeRoot, new Vector2(.045f, .37f), new Vector2(.955f, .78f));
            receiver = Rect("Receiver", safeRoot, new Vector2(.5f, .245f), new Vector2(.5f, .365f)); receiver.sizeDelta = new Vector2(350, 0); Background(receiver, Color.clear, true);
            receiver.gameObject.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(ToggleTargetZoom);
            receiverLabel = Text("ReceiverLabel", receiver, new Vector2(0, .94f), new Vector2(1, 1.22f), 16, "Make this row"); StyleCelebrationType(receiverLabel,CelebrationType.SkyCopy);receiverLabel.alignment=TextAlignmentOptions.Center;
            targetRow = Rect("TargetRow", receiver, new Vector2(.5f, .236f), new Vector2(.5f, .856f));
            precise = Rect("PreciseInput", safeRoot, new Vector2(.05f, .09f), new Vector2(.95f, .225f));
            DirectionButton("Focus previous", precise, new Vector2(0, 56f/104), new Vector2(.2f, 1), false, () => MoveFocus(-1));
            focusLabel = Text("Focus", precise, new Vector2(.22f, 56f/104), new Vector2(.78f, 1), 20, "Choose a symbol");
            DirectionButton("Focus next", precise, new Vector2(.8f, 56f/104), Vector2.one, true, () => MoveFocus(1));
            swapLeft = Button("Swap left", precise, Vector2.zero, new Vector2(.48f, 48f/104), () => PlayFocusedSwap(-1));
            swapRight = Button("Swap right", precise, new Vector2(.52f, 0), new Vector2(1, 48f/104), () => PlayFocusedSwap(1));
            actions = Rect("Actions", safeRoot, new Vector2(.05f, .012f), new Vector2(.95f, .078f));
            undo = Button("Undo", actions, new Vector2(.12f,0), new Vector2(.47f, 1), Undo);
            hint = Button("Hint", actions, new Vector2(.18f,0), new Vector2(.35f,1), RequestHint);
            help = Button(UiCopy.Raw("?"), header, Vector2.zero, Vector2.one, ReplayStageLessons);
            help.GetComponentInChildren<TextMeshProUGUI>().fontStyle=FontStyles.Bold;
            hint.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
            hintMark=Rect("Hint lamp",hint.transform,Vector2.zero,Vector2.one).gameObject.AddComponent<HintMark>();hintMark.raycastTarget=false;
            drop = Button("Drop", actions, new Vector2(.53f, 0), new Vector2(.88f,1), () => PlayCommand(PuzzleCommand.Drop));
            drop.GetComponent<UnityEngine.UI.Image>().color = new Color(.98f, .74f, .32f);
            retry = Button("Retry", safeRoot, new Vector2(.05f, .09f), new Vector2(.43f, .155f), Retry);
            targetZoom = Rect("TargetZoom", safeRoot, Vector2.zero, Vector2.one); Background(targetZoom,new Color(.035f,.19f,.20f,.96f),true);
            Text("ZoomTitle", targetZoom, new Vector2(.05f, .7f), new Vector2(.95f, .8f), 22, "Make the same row").color=MenuPaper;
            var closeTarget=Button("Close target", targetZoom, new Vector2(.2f, .22f), new Vector2(.8f, .30f), ToggleTargetZoom);closeTarget.GetComponent<UnityEngine.UI.Image>().sprite=enamelControls[0];closeTarget.GetComponent<UnityEngine.UI.Image>().color=Color.white;closeTarget.GetComponentInChildren<TextMeshProUGUI>().color=MenuPaper;
            targetZoom.gameObject.SetActive(false);
        }
        void ChangeNode(int direction)
        {
            if (!Interaction.CanControl) return;
            int index = Catalog.PrimaryLevels.ToList().FindIndex(l => l.Definition.CampaignNodeId == node);
            index = Math.Max(0, Math.Min(Catalog.PrimaryLevels.Count - 1, index + direction)); LoadNode(Catalog.PrimaryLevels[index].Definition.CampaignNodeId);
        }
        void RebuildLevelViews(LevelDefinition level)
        {
            foreach (var view in floorViews) {view.gameObject.SetActive(false);Destroy(view.gameObject);}
            // Destroy is deferred; inactive old children must stop contributing to the new row layout now.
            foreach (Transform child in targetRow) {child.gameObject.SetActive(false);Destroy(child.gameObject);}
            if (tokenRow != null) {tokenRow.gameObject.SetActive(false);Destroy(tokenRow.gameObject);}
            platformLegends.Clear();floorViews.Clear(); floorLabels.Clear(); deviceLabels.Clear(); contactFaces.Clear(); deviceDocks.Clear(); deviceOverlays.Clear(); slots.Clear(); slotImages.Clear(); tokenLabels.Clear(); targetLabels.Clear();
            int count = level.Floors.Count;
            float gapWeight=0,usedGap=0;
            for(int i=0;i<count-1;i++)gapWeight+=FloorGapWeight(level.Floors[i]);
            for (int i = 0; i < count; i++) {
                float y = count == 1 ? .50f : count>2?.94f-.84f*usedGap/gapWeight:.87f-i*.74f/(count-1);
                usedGap+=FloorGapWeight(level.Floors[i]);
                var card = Rect("Floor" + (i + 1), floorArea, new Vector2(.5f, y), new Vector2(.5f, y));
                card.sizeDelta = new Vector2(Mathf.Min(342, level.InitialSymbolIds.Count * 76 + 24), count == 4 ? 110 : count==1&&level.InitialSymbolIds.Count==3&&level.InitialSymbolIds.Any(s=>s.Kind==SymbolKind.Glyph)?152:132); floorViews.Add(card);
                var title = Text("Durability", card, new Vector2(.02f, .09f), new Vector2(.42f, .27f), 13, ""); title.alignment = TextAlignmentOptions.Center; floorLabels.Add(title);
                var devices = Text("Devices", card, Vector2.zero, Vector2.zero, 13, ""); deviceLabels.Add(devices);
                BuildDeviceDocks(card, level.Floors[i], level.InitialSymbolIds.Count, i);

            }
            tokenRow = Rect("PersistentRow", floorViews[0], new Vector2(.045f, .43f), new Vector2(.955f, 1.03f));
            var layout = tokenRow.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.spacing = 3; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = layout.childForceExpandHeight = true;
            for (int i = 0; i < level.InitialSymbolIds.Count; i++) {
                var cell = Rect("Slot" + (i + 1), tokenRow, Vector2.zero, Vector2.one);
                var image = Background(cell, Color.clear, true); image.sprite = art.panel; image.type = UnityEngine.UI.Image.Type.Sliced; slotImages.Add(image);
                var slot = cell.gameObject.AddComponent<TokenSlot>(); slot.Game = this; slot.Slot = i; slots.Add(slot);
                // The 3D body extends a little below the row's UI rectangle.
                var touch=Rect("Piece touch area",cell,new Vector2(0,-.18f),Vector2.one);Background(touch,Color.clear,true);
                var glyph = Text("Symbol", cell, Vector2.zero, Vector2.one, 28, ""); glyph.font = art.titleFont; FinishLetter(glyph); tokenLabels.Add(glyph);
            }
            AddSymbolLabels(targetRow, level.TargetSymbolIds, 24, targetLabels); Canvas.ForceUpdateCanvases();
            machine.Bind(Interaction, floorViews, slots, targetLabels, receiver);
        }
        void AddContactGlyph(RectTransform card, ContactDefinition contact, int count)
        {
            if (contact == null || contact.RequiredSymbol.Kind != SymbolKind.Glyph) return;
            float x = .045f + .91f * (contact.Slot + .5f) / count;
            var label = Text("DeviceGlyph", card, new Vector2(x - .039f, .188f), new Vector2(x + .039f, .432f), 16, contact.RequiredSymbol.Value);
            label.font = art.titleFont;contactFaces.Add(new ContactFace{floor=floorViews.IndexOf(card),slot=contact.Slot,label=label});
        }
        void AddSymbolLabels(RectTransform parent, IEnumerable<SymbolId> symbols, int size, List<TextMeshProUGUI> labels)
        {
            var layout = parent.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>() ?? parent.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.spacing = 4; layout.padding = new RectOffset(0,0,0,0); layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = layout.childForceExpandHeight = true;
            foreach (var symbol in symbols) {
                var label = Text("TargetSymbol", parent, Vector2.zero, Vector2.one, size, FaceText(symbol)); label.font = art.titleFont; FinishLetter(label);
                var sizing = label.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); sizing.minWidth = sizing.preferredWidth = 0; sizing.flexibleWidth = 1; labels.Add(label);
            }
        }
#if UNITY_EDITOR
        Vector4 editorSafeInsets;
        public void SetEditorSafeInsets(Vector4 insets){editorSafeInsets=insets;RefreshLayout();}
#endif
        void ApplySafeArea()
        {
            safeArea = Screen.safeArea; screenWidth = Screen.width; screenHeight = Screen.height;
            if (safeRoot == null || screenWidth == 0 || screenHeight == 0) return;
            safeRoot.anchorMin = new Vector2(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight);
            safeRoot.anchorMax = new Vector2(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight);
#if UNITY_EDITOR
            if(editorSafeInsets!=Vector4.zero){safeRoot.anchorMin=new Vector2(editorSafeInsets.x,editorSafeInsets.y);safeRoot.anchorMax=new Vector2(1-editorSafeInsets.z,1-editorSafeInsets.w);}
#endif
            safeRoot.offsetMin = safeRoot.offsetMax = Vector2.zero;
            var canvasRect = (RectTransform)safeRoot.parent;
            UpdateLandscapeLayoutGate(canvasRect);
            float available = canvasRect.rect.width * (safeRoot.anchorMax.x - safeRoot.anchorMin.x);
            float inset = (CurrentScreen==GameScreen.Home||ReferenceShopScreen) ? 0 : Mathf.Max(0, (available - 430) * .5f);
            safeRoot.offsetMin = new Vector2(inset, 0); safeRoot.offsetMax = new Vector2(-inset, 0);
            LayoutMenuSurface();
            if (header != null && actions != null) {
                var canvas = safeRoot.GetComponentInParent<Canvas>(true);
                if (canvas == null) return; // Scene teardown can disable the hierarchy before the presentation is cancelled.
                float scale = canvas.scaleFactor;
                float unit = 1 / Mathf.Min(1, scale); // Preserve 48-pixel hit regions in the smallest reference capture.
                foreach(var button in menuActions)if(button!=null)button.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,52*unit);
                FitCoinVideoOffer(unit);
                FitMenuRows();
                FixedBand(header, safeRoot.rect.height - 60 * unit, 48 * unit);
                FixedBand(statusPaper,safeRoot.rect.height-114*unit,44*unit);status.fontSize=16*unit*(Preferences.LargeText?1.12f:1);
                LayoutInstruction(unit);LayoutFieldTools(unit);
                LayoutPlatformLegends(unit);
                foreach(var dock in deviceDocks)if(dock.countdown!=null)dock.countdown.fontSize=13*unit;
                float lowerClearance=GameplaySillClearance(unit);
                FixedBand(actions, lowerClearance, 64 * unit); FixedBand(precise, lowerClearance+68*unit, 104 * unit);
                foreach(var button in actions.GetComponentsInChildren<UnityEngine.UI.Button>()){
                    var label=button.GetComponentInChildren<TextMeshProUGUI>();if(label==null)continue;
                    ApplyControlCaption(label,unit);
                }
                if(resultRoot!=null)LayoutResultControls(unit);
                receiverLabel.fontSize=16*unit;
                LayoutLowerGameArea(unit);
                foreach (var label in deviceLabels) label.fontSize = 14 * unit;
                foreach (var floor in floorViews) foreach (Transform child in floor)
                    if (child.name == "DeviceGlyph") child.GetComponent<TextMeshProUGUI>().fontSize = 14 * unit;
            }
            if (floorViews.Count != 0 && Interaction != null) {
                float boardWidth = GameplayBoardWidth(Interaction.Attempt.Current.Row.Count);
                // Match the ordinary three-shape row, even when letters share the plank.
                float referenceCellWidth=(GameplayBoardWidth(3)*.91f-6)/3;
                machine.MixedShapeReferenceWidth=floorViews[0].TransformVector(Vector3.right*(referenceCellWidth*.78f)).magnitude;
                foreach (var floor in floorViews) floor.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boardWidth);
                LayoutGameplayHeader(boardWidth);
                receiver.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(safeRoot.rect.width * .9f, Interaction.Attempt.Current.Row.Count * 68 + 40));
                int targetCount=Mathf.Max(1,targetLabels.Count);
                float targetWidth=Mathf.Min(receiver.rect.width*.87f,targetCount*64f+(targetCount-1)*4f);
                targetRow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,targetWidth);
            }
        }
        float GameplayBoardWidth(int count)
            =>Mathf.Min(safeRoot.rect.width*.91f,Mathf.Max(count*76+24,safeRoot.rect.width*.70f));
        void LayoutLowerGameArea(float unit)
        {
            if(Interaction==null)return;
            float receiverHeight=Interaction.Attempt.Level.Floors.Count==1?112:100;
            float receiverBottom=(CurrentScreen==GameScreen.Fail?232:CurrentScreen==GameScreen.Win||CurrentScreen==GameScreen.Checkpoint||PreciseInputActive?184:Interaction.Attempt.Level.Floors.Count==1?144:82)*unit;
            receiverBottom+=GameplaySillClearance(unit)-8*unit;
            if(CurrentScreen==GameScreen.Playing)
                receiverBottom=Mathf.Lerp(receiverBottom,Mathf.Max(receiverBottom,fieldTrayDockHeight+28*unit),Mathf.Clamp01(fieldTrayProgress));
            FixedBand(receiver,receiverBottom,receiverHeight);
            if(CurrentScreen==GameScreen.Fail||CurrentScreen==GameScreen.Checkpoint)outcome?.Layout(unit,receiver);
            floorArea.anchorMin=new Vector2(.045f,0);floorArea.anchorMax=new Vector2(.955f,0);
            var lowest=Interaction.Attempt.Level.Floors[Interaction.Attempt.Level.Floors.Count-1];
            bool hangingBelow=lowest.Bomb!=null||lowest.Key!=null||lowest.Repair!=null||lowest.OptionalStamp!=null||lowest.Powerup!=null;
            floorArea.offsetMin=new Vector2(0,receiverBottom+receiverHeight+(hangingBelow?105:64)*unit);
            floorArea.offsetMax=new Vector2(0,safeRoot.rect.height-150*unit);
            LayoutFloorSpacing(unit);
        }
        float GameplaySillClearance(float unit)
        {
            var canvas=(RectTransform)safeRoot.parent;
            float sill=canvas.rect.height*WorldFrameArt.SillViewportHeight(canvas.rect.width/Mathf.Max(1,canvas.rect.height));
            float safeBottom=canvas.rect.height*safeRoot.anchorMin.y;
            // The complete action face and its shadow sit in sky, above the physical sill.
            return Mathf.Max(8*unit,sill-safeBottom+14*unit);
        }
        float[] floorGaps=Array.Empty<float>();
        static float FloorGapWeight(FloorDefinition floor)=>floor.Key!=null?2.1f:floor.Bomb!=null||floor.Repair!=null||floor.OptionalStamp!=null||floor.Powerup!=null?1.7f:1;
        void LayoutFloorSpacing(float unit)
        {
            int gaps=floorViews.Count-1;if(gaps<2||Interaction==null||floorArea.rect.height<=0)return;
            if(floorGaps.Length!=gaps)floorGaps=new float[gaps];
            var floors=Interaction.Attempt.Level.Floors;
            float remaining=floorArea.rect.height*.84f,weight=0;
            // A hanging contact must not squeeze the other decks closer than a row plus air.
            float minimum=Mathf.Min(remaining/gaps,floorViews[0].rect.height*.60f+14*unit);
            for(int i=0;i<gaps;i++){floorGaps[i]=-1;weight+=FloorGapWeight(floors[i]);}
            for(int pass=0;pass<gaps;pass++){
                bool fixedGap=false;
                for(int i=0;i<gaps;i++)if(floorGaps[i]<0&&remaining*FloorGapWeight(floors[i])/weight<minimum){
                    floorGaps[i]=minimum;remaining-=minimum;weight-=FloorGapWeight(floors[i]);fixedGap=true;
                }
                if(!fixedGap)break;
            }
            float used=0;
            for(int i=0;i<floorViews.Count;i++){
                var rect=floorViews[i];float y=.94f-used/floorArea.rect.height;
                rect.anchorMin=new Vector2(rect.anchorMin.x,y);rect.anchorMax=new Vector2(rect.anchorMax.x,y);
                if(i<gaps){if(floorGaps[i]<0)floorGaps[i]=remaining*FloorGapWeight(floors[i])/weight;used+=floorGaps[i];}
            }
        }
        static void FixedBand(RectTransform rect, float bottom, float height)
        {
            rect.anchorMin = new Vector2(rect.anchorMin.x, 0); rect.anchorMax = new Vector2(rect.anchorMax.x, 0);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, bottom + height * .5f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
        void RenderState()
        {
            RenderFrame(new PresentationFrame(Interaction.Attempt.Current));
        }
        public void RenderFrame(PresentationFrame state, bool arriving = false)
        {
            VisibleFrame = state; machine.Frame = state;
            var attempt = Interaction.Attempt; var level = attempt.Level;
            for (int i = 0; i < level.Floors.Count; i++) {
                var floor = level.Floors[i]; bool active = state.Status == PuzzleStatus.Playing && i == state.ActiveFloor;
                bool past = !state.FloorVisible(i);
                deviceLabels[i].SetCopy("");
                deviceOverlays[i].gameObject.SetActive(!past);
                PaintDeviceDocks(state,i,past,active);
                foreach (Transform child in floorViews[i]) if (child.name == "DeviceGlyph") child.gameObject.SetActive(!past);
            }
            PaintPlatformLegends(state);
            tokenRow.SetParent(state.Status == PuzzleStatus.Playing ? floorViews[state.ActiveFloor] : receiver, false);
            tokenRow.anchorMin = new Vector2(state.Status == PuzzleStatus.Playing ? .045f : .07f, state.Status == PuzzleStatus.Playing ? .43f : .28f);
            tokenRow.anchorMax = new Vector2(state.Status == PuzzleStatus.Playing ? .955f : .93f, state.Status == PuzzleStatus.Playing ? 1.03f : .78f); tokenRow.offsetMin = tokenRow.offsetMax = Vector2.zero;
            tokenRow.gameObject.SetActive(state.Status == PuzzleStatus.Playing || arriving);
            PaintGameplayTextContrast();
            safeRoot.GetComponentsInChildren(false,buttonDesigns);foreach(var design in buttonDesigns)design.Refresh();
            foreach (var label in targetLabels) label.color = Ink;
            for (int i = 0; i < tokenLabels.Count; i++) {
                tokenLabels[i].text = FaceText(state.Row[i]); tokenLabels[i].fontSize = state.Status == PuzzleStatus.Won ? 24 : state.Row.Count==3&&state.Row[i].Kind==SymbolKind.Glyph?44:28;
            }
            receiverLabel.SetCopy(state.Status == PuzzleStatus.Won ? new UiCopy("Moves to deliver: {0}",attempt.RouteLength) : "Make this row");
            status.SetCopy(userPaused ? "Paused. Devices wait for your next command." : state.Status == PuzzleStatus.Won ? "Exact order delivered." : state.Status == PuzzleStatus.Lost
                ? "The rows do not match yet."
                : state.Row.SequenceEqual(level.TargetSymbolIds)
                    ? state.ActiveFloor == level.Floors.Count - 1 ? "Ready! Tap Deliver." : "Ready here. Check the board below."
                    : level.ReachCharges>0 ? Interaction.Attempt.Current.ReachCharges>0 ? "Drag past a neighbour. The ribbon works once." : "Ribbon used. You can still swap neighbours." : "Drag a piece onto its neighbour.");
            if (state.Status == PuzzleStatus.Lost) {
                receiverLabel.SetCopy("Goal");
                tokenRow.gameObject.SetActive(true); tokenRow.anchorMin = new Vector2(.02f, -.36f); tokenRow.anchorMax = new Vector2(.98f, .06f);
            }
            RenderFocus(); RefreshLayout();
            for (int i = 0; i < tokenLabels.Count; i++) { glyphPositions[i] = tokenLabels[i].rectTransform.position; tokenLabels[i].rectTransform.localScale = Vector3.one; }
            ApplyMenuSpriteCounters();if(flowReady)RefreshFailureReference();
        }
        public void OffsetPresentedToken(int index, Vector3 offset, Vector2 stretch)
        {
            machine.OffsetToken(index, offset, stretch);
            var face=tokenLabels[index].rectTransform;var position=machine.TokenVisualPosition(index);position.z=face.position.z;face.position=position;
            tokenLabels[index].rectTransform.localScale = new Vector3(stretch.x, stretch.y, 1);
        }
        public void SetPresentationFeedback(UiCopy text) { status.SetCopy(text); }
        public void AnnounceReaction(UiCopy text,string tool) {
            int kind=tool.StartsWith("Bomb")?2:tool=="PruningSaw"?3:tool=="RepairKit"?4:tool=="Stamp"?8:tool=="RibbonSpool"?9:tool=="FrostFlask"?10:0;
            reactionBanner?.Show(text,tool,kind==0?null:DeviceFeedbackAnchor(VisibleFrame.ActiveFloor,kind));
        }
        public void SyncPresentation() { machine.ResetWoodPoses();RenderState(); FinalizeScreenIfNeeded(); }
        public void CompleteDeliveryPose()
        {
            if (VisibleFrame.Status == PuzzleStatus.Won) { tokenRow.gameObject.SetActive(false); machine.Sync(); }
        }
        UiCopy DescribeFloor(FloorDefinition floor, PresentationFrame state)
        {
            var parts = new List<UiCopy>();
            if (floor.EntryTransform != EntryTransform.None) parts.Add(floor.EntryTransform == EntryTransform.MirrorNext ? "Mirror on entry" : floor.EntryTransform == EntryTransform.ShiftLeftOne ? "Shift left on entry" : "Shift right on entry");
            var closed = Enumerable.Range(0, floor.OpenEdges.Count).Where(i => state == null ? !floor.OpenEdges[i] : !state.EdgeVisibleOpen(floor, i)).ToArray();
            if (closed.Length != 0) parts.Add(new UiCopy("Locked {0}", UiCopy.Raw(string.Join(", ",closed.Select(i=>(i+1).ToString())))));
            if (floor.FragileEdges.Count != 0) parts.Add(new UiCopy("Fragile {0}", UiCopy.Raw(string.Join(", ",floor.FragileEdges.Select(i=>(i+1).ToString())))));
            if (floor.LinkedPair.HasValue) parts.Add(new UiCopy("Linked {0} + {1}",floor.LinkedPair.Value.FirstEdge+1,floor.LinkedPair.Value.SecondEdge+1));
            if (floor.Bomb != null) parts.Add(new UiCopy("Bomb {0}",new UiCopy(state == null ? floor.Bomb.Ticks.ToString() : state.BombPhase == BombPhase.Armed ? state.BombTicks.ToString() : state.BombPhase.ToString())));
            if (floor.Key != null) parts.Add(state?.KeyUsed == true ? "Unlocked" : new UiCopy("Key → {0}",floor.Key.EdgeToOpen+1));
            if (floor.Repair != null) parts.Add(state?.RepairUsed == true ? "Repaired" : new UiCopy("Repair +{0}",floor.Repair.Amount));
            if (floor.OneUseBridgeEdge.HasValue) parts.Add(new UiCopy(state?.BridgeUsed==true?"Bridge {0} used":"Bridge {0} once",floor.OneUseBridgeEdge.Value+1));
            if (floor.OptionalStamp != null) parts.Add("Stamp");
            return UiCopy.Join(", ",parts);
        }
        void RenderFocus()
        {
            ClearDeviceForecast();
            if (Interaction == null) return;
            for (int i = 0; i < slotImages.Count; i++) slotImages[i].color = Color.clear;
            for (int i = 0; i < tokenLabels.Count; i++) {
                var face = tokenLabels[i].rectTransform; float y = VisibleFrame.Status == PuzzleStatus.Won ? .5f : ToyMachineView.TokenFaceY(Interaction.SelectedSlot == i);
                face.anchorMin = new Vector2(0, y); face.anchorMax = new Vector2(1, y);
                face.sizeDelta = new Vector2(0, ((RectTransform)slots[i].transform).rect.height); face.anchoredPosition = Vector2.zero;
                glyphPositions[i] = face.position;
            }
            machine?.Sync(); RestoreTouchPose(); SyncTokenFaces();
            int? selected = Interaction.SelectedSlot;
            focusLabel.SetCopy(selected.HasValue ? DisplaySymbol(Interaction.Attempt.Current.Row[selected.Value]) : "Choose a symbol");
            var left=Interaction.FocusedCommand(-1);var right=Interaction.FocusedCommand(1);
            swapLeft.interactable = Interaction.CanPlay && left.HasValue && Interaction.AllowsCommand(left.Value);
            swapRight.interactable = Interaction.CanPlay && right.HasValue && Interaction.AllowsCommand(right.Value);
            PrecisionLabel(swapLeft, "Left", selected.HasValue ? selected.Value - 1 : -1);
            PrecisionLabel(swapRight, "Right", selected ?? -1);
        }
        void PrecisionLabel(UnityEngine.UI.Button button, string direction, int edge)
        {
            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (!Interaction.SelectedSlot.HasValue || edge < 0 || edge >= slots.Count - 1) { label.SetCopy(direction); return; }
            var preview = Interaction.Attempt.Resolver.Preview(Interaction.Attempt.Current, PuzzleCommand.Swap(edge));
            label.SetCopy(new UiCopy(preview.IsLegal?"{0} −{1}":preview.Rejection==RejectionReason.NoOp?"{0}: same":"{0}: locked",new UiCopy(direction),preview.DurabilityCost));
        }
        void RenderControls()
        {
            RefreshFieldTools();
            if (Interaction == null) return;
            RefreshReplayLessons();
            help.gameObject.SetActive(CurrentScreen==GameScreen.Playing&&replayLessons.Count>0);
            undo.interactable = Interaction.CanControl && Interaction.AllowsControl(PracticeControl.Undo) && Interaction.Attempt.CanUndo;
            DrawUndoCaption(false);
            retry.interactable = Interaction.AllowsRetry && (Interaction.CanControl || preparation.HasFailed || descentPlanFailed); retry.gameObject.SetActive(preparation.HasFailed || descentPlanFailed || !Interaction.IsPresenting && Interaction.Attempt.Current.Status != PuzzleStatus.Playing);
            drop.interactable = Interaction.CanPlay && Interaction.AllowsCommand(PuzzleCommand.Drop);
            hint.interactable = Interaction.CanPlay && hintTask == null && Interaction.AllowsControl(PracticeControl.Hint)
                &&(knownDeadEnd||Interaction.Attempt.CanShowFreeHint||Interaction.Attempt.GuideActive||(offeredHint?.Result.Status==HintStatus.Step&&Interaction.Attempt.IsCurrent(offeredHint))||BonusHintAvailable);
            help.interactable=Interaction.CanControl&&!MechanicPopupOpen&&!Interaction.IsPresenting;
            drop.GetComponentInChildren<TextMeshProUGUI>().SetCopy(Interaction.Attempt.Current.Status == PuzzleStatus.Playing
                && Interaction.Attempt.Current.ActiveFloor == Interaction.Attempt.Level.Floors.Count - 1
                && Interaction.Attempt.Current.Row.SequenceEqual(Interaction.Attempt.Level.TargetSymbolIds) ? "Deliver" : "Descend");
            PaintHintCounter();
            if(hintMark!=null)hintMark.color=HintVisible?new Color(.53f,.24f,.025f):new Color(.20f,.055f,.014f,hint.interactable?1:.5f);

            previous.interactable = next.interactable = Interaction.CanControl;
            PreciseInputActive = ForcePreciseInput;
            precise.gameObject.SetActive(PreciseInputActive && Interaction.Attempt.Current.Status == PuzzleStatus.Playing && (!flowReady||CurrentScreen==GameScreen.Playing));
            if (eventPlayer == null || !eventPlayer.IsRunning) RenderFocus();
            if(flowReady && resultRoot!=null && actions!=null)ArrangeResultControls();
            PaintControlLabels();
            PaintPreparation();
        }
        static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        static UnityEngine.UI.Image Background(RectTransform rect, Color color, bool interactive)
        {
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = color; image.raycastTarget = interactive; return image;
        }
        TextMeshProUGUI Text(string name, Transform parent, Vector2 min, Vector2 max, int size, UiCopy content)
        {
            var rect = Rect(name, parent, min, max); var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            bool menuText=CurrentScreen!=GameScreen.Home&&CurrentScreen!=GameScreen.Dialogue&&menuRoot!=null&&rect.IsChildOf(menuRoot)&&!IsSymbolFace(name)&&!name.StartsWith("Demo")&&name!="Wood moves"&&name!="Tool tag meaning"&&name!="Mini glyph";
            text.font = font; baseTextSizes[text]=size;
            text.fontSize = size*(Preferences.LargeText&&!IsSymbolFace(name)?1.12f:1); text.enableAutoSizing = false;
            text.color = Preferences.HighContrast?new Color(.06f,.17f,.18f):menuRoot!=null&&rect.IsChildOf(menuRoot)?MenuInk:Ink;
            text.SetCopy(IsSymbolFace(name)||name=="Mini glyph"?UiCopy.Raw(content.English):content); text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            if(!IsSymbolFace(name)&&name!="Mini glyph")text.margin=Vector4.one*(CenteredContentRow.MinimumInset*CenteredContentRow.Unit(rect));
            return text;
        }
        UnityEngine.UI.Button Button(UiCopy label, Transform parent, Vector2 min, Vector2 max, Action clicked, bool sharedDesign=true)
        {
            var rect = Rect(label.English.Replace(" ", ""), parent, min, max); var image = Background(rect, Paper, true); image.sprite = art.panel; image.type = UnityEngine.UI.Image.Type.Sliced;
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();button.onClick.AddListener(()=>{feedback?.Sound(ToyCue.Select);feedback?.Haptic(.07f);clicked();});
            var colors = button.colors; colors.highlightedColor = new Color(1, .94f, .78f); colors.pressedColor = new Color(.82f, .88f, .81f); colors.disabledColor = new Color(.88f, .90f, .84f, .7f); button.colors = colors;
            rect.gameObject.AddComponent<ButtonFeel>().Initialize(this);
            var text = Text("Label", rect, Vector2.zero, Vector2.one, 14, label); text.font = art.titleFont;
            ApplyControlCaption(text);if(sharedDesign)RegisterButtonDesign(button);return button;
        }
        UnityEngine.UI.Button DirectionButton(string name, Transform parent, Vector2 min, Vector2 max, bool right, Action clicked)
        {
            var button = Button(name, parent, min, max, clicked); button.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
            // The authored arrow is the visible control; its transparent 44-unit hit target stays accessible.
            var face=button.GetComponent<UnityEngine.UI.Image>();face.sprite=null;face.color=Color.clear;
            var icon = Rect("Navigation arrow", button.transform, new Vector2(.20f,.27f), new Vector2(.80f,.73f)).gameObject.AddComponent<ChevronGraphic>();
            icon.PointsRight = right; icon.Weight=name=="Back"||name=="Previous world"||name=="Next world"?0:1; icon.color = Color.white; icon.raycastTarget = false;
            button.targetGraphic=icon;
            return button;
        }
    }
}
