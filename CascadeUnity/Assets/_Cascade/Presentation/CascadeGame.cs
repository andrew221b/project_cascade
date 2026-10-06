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
        TextMeshProUGUI levelLabel, status, focusLabel, receiverLabel, wrongRowLabel;
        UnityEngine.UI.Button undo, retry, drop, hint, pause, previous, next, swapLeft, swapRight;
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
            ReadProfile();
            InitializeUiLanguage();
            Catalog = ContentImporter.Import(Source("levels"), Source("localized_levels"), Source("words_en"), Source("words_ru"), Source("words_uk"),Source("campaign_extension"));
            art = ToyArt.Load(); font = art.bodyFont;
            if (uiCamera == null) uiCamera = Camera.main;
            machine = gameObject.AddComponent<ToyMachineView>(); machine.Initialize(art, uiCamera);
            motion = Resources.Load<MotionProfile>("CascadeArt/MotionProfile") ?? throw new InvalidOperationException("Build CASCADE motion assets before playing.");
            feedback = gameObject.AddComponent<ToyFeedbackPool>(); feedback.Initialize(this, art, motion);
            eventPlayer = gameObject.AddComponent<PuzzleEventPlayer>(); eventPlayer.Initialize(this, motion, feedback);
            gameObject.AddComponent<PhoneShakeFeedback>().Initialize(this);
            BuildScreen();BuildControlArt();BuildFieldTools(); LoadNode(initialNode); BuildFlow(); RestoreProfile();InitializeMonetization();
        }
        void OnEnable() { disabling=false;if (Interaction != null) { if(!preparation.IsReady&&!preparation.IsPending)BeginPreparation(); Interaction.SetSuspended(nativeAdSuspended||applicationPaused || !applicationFocused); RenderState(); } }
        void OnDisable()
        {
            disabling=true;
            CloseLandingPreview();ResetMechanicDiscovery();
            PersistProgress();
            SettleDescent();CancelDescentWork();
            EndTokenHold(); CancelHint(); CancelDeadEnd(); preparation.Cancel(); StopPresentation(); Interaction?.SetSuspended(true); feedback?.ClearTails();
        }
        void OnDestroy() { mechanicArrival?.Kill();previewArrival?.Kill();coinArrival?.Kill();if(introBranch!=null)Destroy(introBranch);if(purchaseStore!=null)purchaseStore.Changed-=NotifyStoreChanged;DisposeNavigationArt();if(treasureArt!=null)foreach(var sprite in treasureArt)if(sprite!=null)Destroy(sprite);if(travelArt!=null)foreach(var sprite in travelArt)if(sprite!=null)Destroy(sprite);DisposeCompletionArt();DisposePlatformProps(); preparation.Dispose(); if (Interaction != null) Interaction.Committed -= OnCommitted; if(glyphMaterial!=null)Destroy(glyphMaterial);if(tutorialOverlayMaterial!=null)Destroy(tutorialOverlayMaterial);if(toolSilhouette!=null)Destroy(toolSilhouette);foreach(var sprite in toolBodySprites.Values)if(sprite!=null)Destroy(sprite);foreach(var sprite in enamelControls)if(sprite!=null)Destroy(sprite); }
        void OnApplicationFocus(bool focused)
        {
            if(!focused)SettleDescent();
            if(!focused)PersistProgress();
            if(!focused)absentSince=Time.realtimeSinceStartup;
            else if(absentSince>0 && Time.realtimeSinceStartup-absentSince>60 && CurrentScreen==GameScreen.Playing)Coach?.Advance(15,true);
            applicationFocused = focused; Interaction?.SetSuspended(nativeAdSuspended||applicationPaused || !applicationFocused); if (!focused) { EndTokenHold(); StopPresentation(); }
            if (Interaction != null) RenderState();
        }
        void OnApplicationPause(bool paused)
        {
            if(paused)SettleDescent();
            if(paused)PersistProgress();
            applicationPaused = paused; Interaction?.SetSuspended(nativeAdSuspended||applicationPaused || !applicationFocused); if (paused) { EndTokenHold(); StopPresentation(); }
            if (Interaction != null) RenderState();
        }
        void Update()
        {
            TickPreviewAd();
            if (safeArea != Screen.safeArea || screenWidth != Screen.width || screenHeight != Screen.height) {
                SettleDescent();
                StopPresentation(); ApplySafeArea(); if (Interaction != null) RenderState();
            }
            UpdateFlow();
            TickToolGuide();
            if(applicationFocused&&!applicationPaused&&monetization!=null)monetization.AdvanceForeground(Mathf.Min(Time.unscaledDeltaTime,.5f));
            UpdatePreparation();
            UpdateDescent();
            if (hintTask == null || !hintTask.IsCompleted || !Interaction.CanPlay) return;
            var completed = hintTask; hintTask = null; hintCancellation?.Dispose(); hintCancellation = null;
            if (completed.IsFaulted) { var ignored = completed.Exception; status.SetCopy("Hint unavailable. You can keep playing."); RenderControls(); return; }
            if (completed.IsCanceled) return;
            var offer = completed.Result;
            if (!Interaction.Attempt.RevealHint(offer)) return;
            PersistProgress();
            offeredHint = offer;
            status.SetCopy(offer.Result.Status == HintStatus.Step ? new UiCopy("Try this: {0}", CommandText(offer.Result.Command.Value))
                : offer.Result.Status == HintStatus.DeadEnd ? "Stuck? Undo and try a different move."
                : offer.Result.Status == HintStatus.Unknown ? "Search limit reached. You can keep playing." : "Target delivered.");
            RenderControls();
        }
        public void LoadNode(string campaignNode)
        {
            var content = Catalog.GetPrimary(campaignNode); LoadLevel(content.Definition, content.Locale); node = campaignNode;
            levelLabel.SetCopy(new UiCopy("Level {0}", int.Parse(campaignNode.Substring(1)))); RenderControls();
        }
        public void LoadLevel(LevelDefinition level, string locale = "en")
        {
            MountAttempt(new AttemptService(level,locale));PersistProgress();
        }
        void MountAttempt(AttemptService attempt,DescentRun run=null)
        {
            ResetMechanicDiscovery();
            reactionBanner?.Clear();
            feedback?.ClearTails();
            SettleDescent();CancelDescentWork();descent=run;descentPlanFailed=false;descentTravel=false;paintedDescentStage=-1;machine.LivingWorld.DescentDepth=run?.StageIndex??0;
            var level=attempt.Level;
            EndTokenHold(); CancelHint(); CancelDeadEnd(); StopPresentation(); if (Interaction != null) Interaction.Committed -= OnCommitted;
            Interaction = new PuzzleInteraction(attempt); Interaction.Committed += OnCommitted;
            Interaction.SetSuspended(applicationPaused || !applicationFocused);
            BeginPreparation();
            journeyActive=false; machine.SetZone(JourneyStory.Zone(Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==level.CampaignNodeId)));
            userPaused = zoomOpen = false; targetZoom.gameObject.SetActive(false);
            node = level.CampaignNodeId; levelLabel.SetCopy(new UiCopy("Level {0}", node.TrimStart('C', '0')));
            RebuildLevelViews(level); RenderState(); StartCoach(); if(flowReady)SetScreen(GameScreen.Playing);
        }
        void OnCommitted(TransitionResult result)
        {
            TransferTouchPoseToExchange();
            CancelHint(); Coach?.Observe(result.State,Interaction.Attempt.RouteLength);
            if(node=="C01")firstGestureShown=true;
            RecordCommittedProgress();
            eventPlayer.Play(result); ScheduleDeadEnd(); RenderControls();
        }
        void StopPresentation()
        {
            feedback?.RowPhysics?.CancelShake(true);
            EndTokenHold(); if (eventPlayer != null) eventPlayer.Skip(); else Interaction?.FinishPresentation();
        }
        public void ReportInput(TransitionResult result)
        {
            Coach?.Activity();
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
            if(previewNeighbour==to)return;
            if(to>=0 && Math.Abs(from-to)>=2 && Math.Abs(from-to)<=3 && Interaction.Attempt.Current.ReachCharges>0)ShowPreview(PuzzleCommand.Reach(from,to));
            else if (Math.Abs(from - to) == 1 && to >= 0) ShowPreview(PuzzleCommand.Swap(Math.Min(from, to)));
            else { ClearDeviceForecast(); machine.PreviewForecast(null); status.SetCopy("Move onto the piece next to it."); }
            HighlightDragNeighbour(from, to); RestoreTouchPose();
        }
        void ShowPreview(PuzzleCommand command)
        {
            ClearDeviceForecast();
            var preview = Interaction.Attempt.Resolver.Preview(Interaction.Attempt.Current, command);
            if (!preview.IsLegal) { status.SetCopy(preview.Rejection == RejectionReason.NoOp ? "No change · free to cancel" : "Locked edge · free to cancel"); return; }
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
                effects.Add(prediction.Events.First(e=>e.Kind==PuzzleEventKind.EntryPermuted&&IsWindFloor(e.Floor)).Value==(int)EntryTransform.ShiftRightOne?"Wind slides the row right":"Wind slides the row left");
            status.SetCopy(UiCopy.Join(" · ",effects));
            RenderFocus(); machine.PreviewForecast(prediction); PreviewDeviceDocks(prediction);
            // Forecasts belong to the actual pieces and hanging devices, never rectangular slot panels.
        }
        public void Undo()
        {
            if(!Interaction.Attempt.CanUndo){
                var tool=Interaction.Attempt.CanUseSpool&&Stock(TravelTool.UndoSpool)>0?TravelTool.UndoSpool:TravelTool.ReturnHourglass;
                if(Stock(tool)>0){ShowSupplies();SelectShopItem((int)tool);}return;
            }
            if (!Interaction.Undo()) return; PersistProgress(); CancelHint(); ScheduleDeadEnd(); RenderState(); if(flowReady)ShowGame();
        }
        public void Retry()
        {
            if(descentPlanFailed){var source=descentSource;string locale=descentLocale;var mood=descentMood;bool travel=descentTravel;StartDescentLevel(source,locale,mood);descentTravel=journeyActive=travel;return;}
            if(Interaction.IsPreparing){if(!preparation.IsPending)BeginPreparation();RenderControls();return;}
            var attemptId=Interaction.Attempt.AttemptId;
            bool hadMoves=Interaction.Attempt.Current.Status==PuzzleStatus.Playing&&Interaction.Attempt.RouteLength>0;
            if (!Interaction.Retry()) return;
            if(journeyActive&&!sessionClears.Contains(node))profile.Director.ObserveRetry(attemptId,hadMoves);
            PersistProgress(); CancelHint(); CancelDeadEnd(); StartCoach(); RenderState(); if(flowReady)ShowGame();
        }
        public void RequestHint()
        {
            if (!Interaction.CanPlay || hintTask != null) return;
            CancelHint(); var request = Interaction.Attempt.CaptureHintRequest();
            hintCancellation = new CancellationTokenSource(); var token = hintCancellation.Token;
            hintTask = Task.Run(() => AttemptService.CalculateHint(request, cancellationToken: token)); status.SetCopy("Looking for one step…"); RenderControls();
        }
        public void RefreshLayout() {
            Canvas.ForceUpdateCanvases(); ApplySafeArea();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(safeRoot); Canvas.ForceUpdateCanvases(); FitJourneyFooter(); Canvas.ForceUpdateCanvases();
            if(uiCamera!=null&&machine!=null&&floorViews.Count>0&&floorViews[0]!=null&&status!=null) {
                floorViews[0].GetWorldCorners(flightCorners);float floorTop=flightCorners[2].y;
                status.rectTransform.GetWorldCorners(flightCorners);float statusBottom=flightCorners[0].y;
                float inset=uiCamera.orthographicSize*2/Mathf.Max(1,uiCamera.pixelHeight)*6;
                machine.LivingWorld.SetFlightBand(floorTop+inset,statusBottom-inset);
            }
            machine?.Sync(); SyncTokenFaces(); SyncDeviceSuspension(0); RenderControls();
            feedback?.RefreshAnchors();reactionBanner?.RefreshLayout();
            RestoreTouchPose();
        }
        void CancelHint()
        {
            hintCancellation?.Cancel(); hintCancellation?.Dispose(); hintCancellation = null;
            if (hintTask != null) hintTask.ContinueWith(t => { var ignored = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            hintTask = null; offeredHint = null;
        }
        void MoveFocus(int direction)
        {
            int selected = Interaction.SelectedSlot ?? 0;
            Interaction.FocusSlot(Math.Max(0, Math.Min(slots.Count - 1, selected + direction))); RenderFocus();
        }
        void TogglePause()
        {
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
            statusPaper=Rect("Quiet instruction paper",safeRoot,new Vector2(.055f,.79f),new Vector2(.945f,.86f));var paper=Background(statusPaper,new Color(1,.96f,.84f,.93f),false);paper.sprite=art.panel;paper.type=UnityEngine.UI.Image.Type.Sliced;
            status = Text("Feedback", statusPaper, Vector2.zero, Vector2.one, 16, "");
            BuildInstructionControls();
            reactionBanner=Rect("Tool announcement",safeRoot,Vector2.zero,Vector2.one).gameObject.AddComponent<GameReactionBanner>();
            reactionBanner.Initialize(this,statusPaper,status,art.titleFont);
            floorArea = Rect("Floors", safeRoot, new Vector2(.045f, .37f), new Vector2(.955f, .78f));
            receiver = Rect("Receiver", safeRoot, new Vector2(.5f, .245f), new Vector2(.5f, .365f)); receiver.sizeDelta = new Vector2(350, 0); Background(receiver, Color.clear, true);
            receiver.gameObject.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(ToggleTargetZoom);
            receiverLabel = Text("ReceiverLabel", receiver, new Vector2(0, .94f), new Vector2(1, 1.22f), 16, "Make this row"); receiverLabel.font = art.titleFont;receiverLabel.alignment=TextAlignmentOptions.Center;
            wrongRowLabel = Text("Your row", receiver, new Vector2(.02f,-.69f),new Vector2(.98f,-.46f),15,"Your row"); wrongRowLabel.gameObject.SetActive(false);
            targetRow = Rect("TargetRow", receiver, new Vector2(.5f, .28f), new Vector2(.5f, .78f));
            precise = Rect("PreciseInput", safeRoot, new Vector2(.05f, .09f), new Vector2(.95f, .225f));
            DirectionButton("Focus previous", precise, new Vector2(0, 56f/104), new Vector2(.2f, 1), false, () => MoveFocus(-1));
            focusLabel = Text("Focus", precise, new Vector2(.22f, 56f/104), new Vector2(.78f, 1), 20, "Choose a symbol");
            DirectionButton("Focus next", precise, new Vector2(.8f, 56f/104), Vector2.one, true, () => MoveFocus(1));
            swapLeft = Button("Swap left", precise, Vector2.zero, new Vector2(.48f, 48f/104), () => ReportInput(Interaction.SwapFocused(-1)));
            swapRight = Button("Swap right", precise, new Vector2(.52f, 0), new Vector2(1, 48f/104), () => ReportInput(Interaction.SwapFocused(1)));
            actions = Rect("Actions", safeRoot, new Vector2(.05f, .012f), new Vector2(.95f, .078f));
            undo = Button("Undo", actions, new Vector2(.12f,0), new Vector2(.47f, 1), Undo);
            hint = Button("Hint", header, Vector2.zero, Vector2.one, () => {
                if (offeredHint?.Result.Status == HintStatus.Step && Interaction.Attempt.IsCurrent(offeredHint))
                    ReportInput(Interaction.ApplyHint(offeredHint));
                else RequestHint();
            });
            hint.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
            hintMark=Rect("Hint lamp",hint.transform,Vector2.zero,Vector2.one).gameObject.AddComponent<HintMark>();hintMark.raycastTarget=false;
            drop = Button("Drop", actions, new Vector2(.53f, 0), new Vector2(.88f,1), () => ReportInput(Interaction.Execute(PuzzleCommand.Drop)));
            drop.GetComponent<UnityEngine.UI.Image>().color = new Color(.98f, .74f, .32f);
            retry = Button("Retry", safeRoot, new Vector2(.05f, .09f), new Vector2(.43f, .155f), Retry);
            targetZoom = Rect("TargetZoom", safeRoot, Vector2.zero, Vector2.one); Background(targetZoom, Paper, true);
            Text("ZoomTitle", targetZoom, new Vector2(.05f, .7f), new Vector2(.95f, .8f), 22, "Make the same row");
            Button("Close target", targetZoom, new Vector2(.2f, .22f), new Vector2(.8f, .30f), ToggleTargetZoom);
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
            for (int i = 0; i < count; i++) {
                float y = count == 1 ? .56f : .87f - i * .74f / (count - 1);
                var card = Rect("Floor" + (i + 1), floorArea, new Vector2(.5f, y), new Vector2(.5f, y));
                card.sizeDelta = new Vector2(Mathf.Min(342, level.InitialSymbolIds.Count * 76 + 24), count == 4 ? 110 : 132); floorViews.Add(card);
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
            float available = canvasRect.rect.width * (safeRoot.anchorMax.x - safeRoot.anchorMin.x);
            float inset = Mathf.Max(0, (available - 430) * .5f);
            safeRoot.offsetMin = new Vector2(inset, 0); safeRoot.offsetMax = new Vector2(-inset, 0);
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
                FixedBand(actions, 8 * unit, 52 * unit); FixedBand(precise, 64 * unit, 104 * unit);
                foreach(var button in actions.GetComponentsInChildren<UnityEngine.UI.Button>()){
                    var label=button.GetComponentInChildren<TextMeshProUGUI>();if(label==null)continue;
                    label.fontSize=17*unit*(Preferences.LargeText?1.12f:1);label.margin=new Vector4(5,2,5,2);
                    label.fontSizeMin=14*unit;label.fontSizeMax=17*unit*(Preferences.LargeText?1.12f:1);
                }
                if(resultRoot!=null)FixedBand(resultRoot,8*unit,160*unit);
                receiverLabel.fontSize=16*unit; if(wrongRowLabel!=null)wrongRowLabel.fontSize=15*unit;
                float receiverHeight = 100;
                float receiverBottom = CurrentScreen==GameScreen.Fail ? 232 : CurrentScreen==GameScreen.Win || CurrentScreen==GameScreen.Checkpoint || PreciseInputActive ? 184 : 82;
                FixedBand(receiver, receiverBottom * unit, receiverHeight);
                floorArea.anchorMin = new Vector2(.045f, 0); floorArea.anchorMax = new Vector2(.955f, 0);
                var lowest=Interaction.Attempt.Level.Floors[Interaction.Attempt.Level.Floors.Count-1];
                bool hangingBelow=lowest.Bomb!=null||lowest.Key!=null||lowest.Repair!=null||lowest.OptionalStamp!=null||lowest.Powerup!=null;
                floorArea.offsetMin = new Vector2(0, receiverBottom * unit + receiverHeight + (hangingBelow?105:64) * unit);
                floorArea.offsetMax = new Vector2(0, safeRoot.rect.height - 150 * unit);
                foreach (var label in deviceLabels) label.fontSize = 14 * unit;
                foreach (var floor in floorViews) foreach (Transform child in floor)
                    if (child.name == "DeviceGlyph") child.GetComponent<TextMeshProUGUI>().fontSize = 14 * unit;
            }
            if (floorViews.Count != 0 && Interaction != null) {
                float boardWidth = Mathf.Min(safeRoot.rect.width * .91f, Interaction.Attempt.Current.Row.Count * 76 + 24);
                foreach (var floor in floorViews) floor.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boardWidth);
                LayoutGameplayHeader(boardWidth);
                receiver.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(safeRoot.rect.width * .9f, Interaction.Attempt.Current.Row.Count * 68 + 40));
                int targetCount=Mathf.Max(1,targetLabels.Count);
                float targetWidth=Mathf.Min(receiver.rect.width*.87f,targetCount*64f+(targetCount-1)*4f);
                targetRow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,targetWidth);
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
            foreach (var label in targetLabels) label.color = Ink;
            for (int i = 0; i < tokenLabels.Count; i++) {
                tokenLabels[i].text = FaceText(state.Row[i]); tokenLabels[i].fontSize = state.Status == PuzzleStatus.Won ? 24 : 28;
            }
            receiverLabel.SetCopy(state.Status == PuzzleStatus.Won ? new UiCopy("Moves to deliver: {0}",attempt.RouteLength) : "Make this row");
            status.SetCopy(userPaused ? "Paused. Devices wait for your next command." : state.Status == PuzzleStatus.Won ? "Exact order delivered." : state.Status == PuzzleStatus.Lost
                ? "The rows do not match yet."
                : state.Row.SequenceEqual(level.TargetSymbolIds)
                    ? state.ActiveFloor == level.Floors.Count - 1 ? "Ready! Tap Deliver." : "Ready here. Check the board below."
                    : level.ReachCharges>0 ? Interaction.Attempt.Current.ReachCharges>0 ? "Drag past a neighbour. The ribbon works once." : "Ribbon used. You can still swap neighbours." : "Drag a piece onto its neighbour.");
            wrongRowLabel.gameObject.SetActive(state.Status==PuzzleStatus.Lost);
            if (state.Status == PuzzleStatus.Lost) {
                receiverLabel.SetCopy("Goal");
                tokenRow.gameObject.SetActive(true); tokenRow.anchorMin = new Vector2(.02f, -.36f); tokenRow.anchorMax = new Vector2(.98f, .06f);
            }
            RenderFocus(); RefreshLayout();
            for (int i = 0; i < tokenLabels.Count; i++) { glyphPositions[i] = tokenLabels[i].rectTransform.position; tokenLabels[i].rectTransform.localScale = Vector3.one; }
        }
        public void OffsetPresentedToken(int index, Vector3 offset, Vector2 stretch)
        {
            machine.OffsetToken(index, offset, stretch);
            var face=tokenLabels[index].rectTransform;var position=machine.TokenVisualPosition(index);position.z=face.position.z;face.position=position;
            tokenLabels[index].rectTransform.localScale = new Vector3(stretch.x, stretch.y, 1);
        }
        public void SetPresentationFeedback(UiCopy text) { status.SetCopy(text); }
        public void AnnounceReaction(UiCopy text,string tool) {
            int kind=tool.StartsWith("Bomb")?2:tool=="BrassKey"?3:tool=="RepairKit"?4:tool=="Stamp"?8:tool=="RibbonSpool"?9:tool=="Hourglass"?10:0;
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
            return UiCopy.Join(" · ",parts);
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
            swapLeft.interactable = Interaction.CanPlay && selected.HasValue && selected > 0;
            swapRight.interactable = Interaction.CanPlay && selected.HasValue && selected < slots.Count - 1;
            PrecisionLabel(swapLeft, "Left", selected.HasValue ? selected.Value - 1 : -1);
            PrecisionLabel(swapRight, "Right", selected ?? -1);
        }
        void PrecisionLabel(UnityEngine.UI.Button button, string direction, int edge)
        {
            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (!Interaction.SelectedSlot.HasValue || edge < 0 || edge >= slots.Count - 1) { label.SetCopy(direction); return; }
            var preview = Interaction.Attempt.Resolver.Preview(Interaction.Attempt.Current, PuzzleCommand.Swap(edge));
            label.SetCopy(new UiCopy(preview.IsLegal?"{0} −{1}":preview.Rejection==RejectionReason.NoOp?"{0} · same":"{0} · locked",new UiCopy(direction),preview.DurabilityCost));
        }
        void RenderControls()
        {
            RefreshFieldTools();
            if (Interaction == null) return;
            bool stockedUndo=Interaction.Attempt.CanUseSpool&&Stock(TravelTool.UndoSpool)>0;
            bool stockedReturn=Interaction.Attempt.CanUseHourglass&&Stock(TravelTool.ReturnHourglass)>0;
            undo.interactable = Interaction.CanControl && (Interaction.Attempt.CanUndo||stockedUndo||stockedReturn);
            undo.GetComponentInChildren<TextMeshProUGUI>().SetCopy(Interaction.Attempt.CanUndo?new UiCopy("Undo · {0}",Interaction.Attempt.FreeUndoRemaining):stockedUndo||stockedReturn?(UiCopy)"My tools":new UiCopy("Undo · {0}",Interaction.Attempt.FreeUndoRemaining));
            retry.interactable = Interaction.CanControl || preparation.HasFailed; retry.gameObject.SetActive(preparation.HasFailed || !Interaction.IsPresenting && Interaction.Attempt.Current.Status != PuzzleStatus.Playing);
            drop.interactable = Interaction.CanPlay; hint.interactable = Interaction.CanPlay && hintTask == null;
            drop.GetComponentInChildren<TextMeshProUGUI>().SetCopy(Interaction.Attempt.Current.Status == PuzzleStatus.Playing
                && Interaction.Attempt.Current.ActiveFloor == Interaction.Attempt.Level.Floors.Count - 1
                && Interaction.Attempt.Current.Row.SequenceEqual(Interaction.Attempt.Level.TargetSymbolIds) ? "Deliver" : "Descend");
            hint.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
            if(hintMark!=null)hintMark.color=offeredHint?.Result.Status==HintStatus.Step?new Color(1,.80f,.35f):new Color(1,.97f,.84f,hint.interactable?1:.5f);

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
            if(menuText)size=Mathf.Max(20,size);
            text.font = font; baseTextSizes[text]=size;
            text.fontSize = size*(Preferences.LargeText&&!IsSymbolFace(name)?1.12f:1); text.enableAutoSizing = false;
            text.color = Preferences.HighContrast?new Color(.06f,.17f,.18f):menuRoot!=null&&rect.IsChildOf(menuRoot)?MenuInk:Ink;
            text.SetCopy(IsSymbolFace(name)||name=="Mini glyph"?UiCopy.Raw(content.English):content); text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal; return text;
        }
        UnityEngine.UI.Button Button(UiCopy label, Transform parent, Vector2 min, Vector2 max, Action clicked)
        {
            var rect = Rect(label.English.Replace(" ", ""), parent, min, max); var image = Background(rect, Paper, true); image.sprite = art.panel; image.type = UnityEngine.UI.Image.Type.Sliced;
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.onClick.AddListener(() => clicked());
            var colors = button.colors; colors.highlightedColor = new Color(1, .94f, .78f); colors.pressedColor = new Color(.82f, .88f, .81f); colors.disabledColor = new Color(.88f, .90f, .84f, .7f); button.colors = colors;
            rect.gameObject.AddComponent<ButtonFeel>().Initialize(this);
            var text = Text("Label", rect, Vector2.zero, Vector2.one, 18, label); text.font = art.titleFont; text.margin = new Vector4(10, 5, 10, 5);
            text.textWrappingMode=TextWrappingModes.NoWrap;text.enableAutoSizing=true;text.fontSizeMin=14;text.fontSizeMax=18*(Preferences.LargeText?1.12f:1);return button;
        }
        UnityEngine.UI.Button DirectionButton(string name, Transform parent, Vector2 min, Vector2 max, bool right, Action clicked)
        {
            var button = Button(name, parent, min, max, clicked); button.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
            var icon = Rect("Chevron", button.transform, new Vector2(.28f, .20f), new Vector2(.72f, .80f)).gameObject.AddComponent<ChevronGraphic>();
            icon.PointsRight = right; icon.color = Ink; icon.raycastTarget = false;
            return button;
        }
    }
}
