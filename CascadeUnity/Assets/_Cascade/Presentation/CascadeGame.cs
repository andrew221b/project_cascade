using System;
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
    public sealed class CascadeGame : MonoBehaviour
    {
        [SerializeField] string initialNode = "C01";
        [SerializeField] Camera uiCamera;
        [SerializeField] TMP_FontAsset font;
        public PuzzleInteraction Interaction { get; private set; }
        public ContentCatalog Catalog { get; private set; }
        public bool ForcePreciseInput { get; set; }
        public bool PreciseInputActive { get; private set; }
        public IReadOnlyList<TokenSlot> SlotViews => slots;
        public RectTransform TargetRow => targetRow;
        RectTransform safeRoot, floorArea, receiver, tokenRow, targetRow, precise, targetZoom;
        TextMeshProUGUI levelLabel, status, focusLabel, receiverLabel;
        UnityEngine.UI.Button undo, retry, drop, hint, applyHint, pause, previous, next, swapLeft, swapRight;
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
        Coroutine presentation;
        static readonly Color Ink = new Color(.94f, .95f, .96f);
        static readonly Color Paper = new Color(.16f, .18f, .20f);
        static readonly Color Active = new Color(.30f, .34f, .37f);
        static readonly Color Selected = new Color(.55f, .62f, .65f);

        static string Source(string name) => Resources.Load<TextAsset>("CascadeContent/" + name)?.text
            ?? throw new InvalidOperationException("Missing CASCADE content: " + name);
        void Awake()
        {
            Catalog = ContentImporter.Import(Source("levels"), Source("localized_levels"), Source("words_en"), Source("words_ru"), Source("words_uk"));
            if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font == null) throw new InvalidOperationException("CASCADE requires imported TMP essentials.");
            if (uiCamera == null) uiCamera = Camera.main;
            BuildScreen(); LoadNode(initialNode);
        }
        void OnEnable() { if (Interaction != null) RenderState(); }
        void OnDisable()
        {
            CancelHint(); StopPresentation(); Interaction?.CancelPointer();
        }
        void OnDestroy() { if (Interaction != null) Interaction.Committed -= OnCommitted; }
        void OnApplicationFocus(bool focused)
        {
            applicationFocused = focused; Interaction?.SetSuspended(applicationPaused || !applicationFocused); if (!focused) StopPresentation();
            if (Interaction != null) RenderState();
        }
        void OnApplicationPause(bool paused)
        {
            applicationPaused = paused; Interaction?.SetSuspended(applicationPaused || !applicationFocused); if (paused) StopPresentation();
            if (Interaction != null) RenderState();
        }
        void Update()
        {
            if (safeArea != Screen.safeArea || screenWidth != Screen.width || screenHeight != Screen.height) {
                ApplySafeArea(); if (Interaction != null) RenderState();
            }
            if (hintTask == null || !hintTask.IsCompleted || !Interaction.CanPlay) return;
            var completed = hintTask; hintTask = null; hintCancellation?.Dispose(); hintCancellation = null;
            if (completed.IsFaulted) { status.text = "Hint unavailable. You can keep playing."; return; }
            if (completed.IsCanceled) return;
            var offer = completed.Result;
            if (!Interaction.Attempt.RevealHint(offer)) return;
            offeredHint = offer;
            status.text = offer.Result.Status == HintStatus.Step ? "Hint: " + CommandText(offer.Result.Command.Value)
                : offer.Result.Status == HintStatus.DeadEnd ? "No route from here. Undo lets you try another order."
                : offer.Result.Status == HintStatus.Unknown ? "Search limit reached. You can keep playing." : "Target delivered.";
            applyHint.gameObject.SetActive(offer.Result.Status == HintStatus.Step); RenderControls();
        }
        public void LoadNode(string campaignNode)
        {
            var content = Catalog.GetPrimary(campaignNode); LoadLevel(content.Definition, content.Locale); node = campaignNode;
            levelLabel.text = "CASCADE  /  " + campaignNode.Substring(1); RenderControls();
        }
        public void LoadLevel(LevelDefinition level, string locale = "en")
        {
            CancelHint(); StopPresentation(); if (Interaction != null) Interaction.Committed -= OnCommitted;
            Interaction = new PuzzleInteraction(new AttemptService(level, locale)); Interaction.Committed += OnCommitted;
            Interaction.SetSuspended(applicationPaused || !applicationFocused);
            userPaused = zoomOpen = false; targetZoom.gameObject.SetActive(false);
            RebuildLevelViews(level); RenderState();
        }
        void OnCommitted(TransitionResult result)
        {
            CancelHint(); RenderState(); presentation = StartCoroutine(PresentGrayTransition());
        }
        IEnumerator PresentGrayTransition()
        {
            // P11 gate only. P13 replaces this with the single event/tween player.
            yield return new WaitForSecondsRealtime(.16f);
            Interaction.FinishPresentation(); presentation = null; RenderControls();
        }
        void StopPresentation()
        {
            if (presentation != null) StopCoroutine(presentation); presentation = null; Interaction?.FinishPresentation();
        }
        public void ReportInput(TransitionResult result)
        {
            if (result != null && !result.Accepted) status.text = result.Rejection == RejectionReason.NoOp
                ? "This exchange leaves the row unchanged." : "That edge is locked. No durability spent.";
            else if (result == null) RenderFocus();
        }
        public void PreviewSlot(int slot)
        {
            var preview = Interaction.PreviewNeighbour(slot);
            if (preview != null) ShowPreview(PuzzleCommand.Swap(Math.Min(slot, Interaction.SelectedSlot.Value)));
        }
        public void PreviewDrag(int from, int to)
        {
            if (Math.Abs(from - to) == 1 && to >= 0) ShowPreview(PuzzleCommand.Swap(Math.Min(from, to)));
            else status.text = "Release over one adjacent symbol. A long swipe is cancelled.";
        }
        void ShowPreview(PuzzleCommand command)
        {
            var preview = Interaction.Attempt.Resolver.Preview(Interaction.Attempt.Current, command);
            if (!preview.IsLegal) { status.text = preview.Rejection == RejectionReason.NoOp ? "No change · free to cancel" : "Locked edge · free to cancel"; return; }
            var prediction = Interaction.Attempt.Predict(command);
            string exit = prediction.State.ActiveFloor != Interaction.Attempt.Current.ActiveFloor || prediction.State.Status != PuzzleStatus.Playing ? " · row drops" : "";
            status.text = "Durability −" + preview.DurabilityCost + exit;
            RenderFocus(); foreach (int edge in preview.AffectedEdges) { slotImages[edge].color = Selected; slotImages[edge + 1].color = Selected; }
        }
        public void Undo()
        {
            if (!Interaction.Undo()) return; CancelHint(); RenderState();
        }
        public void Retry()
        {
            if (!Interaction.Retry()) return; CancelHint(); RenderState();
        }
        public void RequestHint()
        {
            if (!Interaction.CanPlay || hintTask != null) return;
            CancelHint(); var request = Interaction.Attempt.CaptureHintRequest();
            hintCancellation = new CancellationTokenSource(); var token = hintCancellation.Token;
            hintTask = Task.Run(() => AttemptService.CalculateHint(request, cancellationToken: token)); status.text = "Looking for one step…"; RenderControls();
        }
        public void RefreshLayout() { Canvas.ForceUpdateCanvases(); RenderControls(); }
        void CancelHint()
        {
            hintCancellation?.Cancel(); hintCancellation?.Dispose(); hintCancellation = null;
            if (hintTask != null) hintTask.ContinueWith(t => { var ignored = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            hintTask = null; offeredHint = null; if (applyHint != null) applyHint.gameObject.SetActive(false);
        }
        void MoveFocus(int direction)
        {
            int selected = Interaction.SelectedSlot ?? 0;
            Interaction.FocusSlot(Math.Max(0, Math.Min(slots.Count - 1, selected + direction))); RenderFocus();
        }
        void TogglePause()
        {
            userPaused = !userPaused; Interaction.SetPaused(userPaused || zoomOpen);
            if (userPaused) StopPresentation(); RenderState();
        }
        void ToggleTargetZoom()
        {
            zoomOpen = !zoomOpen; Interaction.SetPaused(userPaused || zoomOpen); targetZoom.gameObject.SetActive(zoomOpen);
            if (zoomOpen) {
                foreach (Transform child in targetZoom) if (child.name.StartsWith("ZoomToken")) Destroy(child.gameObject);
                var row = Rect("ZoomTokens", targetZoom, new Vector2(.05f, .40f), new Vector2(.95f, .60f)); row.name = "ZoomTokenRow";
                AddSymbolLabels(row, Interaction.Attempt.Level.TargetSymbolIds, 34, new List<TextMeshProUGUI>());
            }
            RenderControls();
        }
        static string DisplaySymbol(SymbolId symbol) => symbol.Kind == SymbolKind.Shape ? "[" + symbol.Value + "]" : symbol.Value;
        static string CommandText(PuzzleCommand command) => command.Kind == CommandKind.EarlyDrop ? "Drop the row" : "swap slots " + (command.Edge + 1) + " & " + (command.Edge + 2);
        void BuildScreen()
        {
            var canvasRect = Rect("GameCanvas", transform, Vector2.zero, Vector2.one);
            var canvas = canvasRect.gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera; canvas.planeDistance = 1;
            var scaler = canvasRect.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(390, 844); scaler.matchWidthOrHeight = 1;
            canvasRect.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Background(canvasRect, Paper, false); safeRoot = Rect("SafeArea", canvasRect, Vector2.zero, Vector2.one); ApplySafeArea();
            var header = Rect("Header", safeRoot, new Vector2(.05f, .88f), new Vector2(.95f, .99f));
            levelLabel = Text("Level", header, new Vector2(0, .48f), new Vector2(.7f, 1), 21, "CASCADE"); levelLabel.alignment = TextAlignmentOptions.Left;
            pause = Button("Pause", header, new Vector2(.75f, .48f), Vector2.one, TogglePause);
            previous = Button("Previous", header, Vector2.zero, new Vector2(.28f, .45f), () => ChangeNode(-1));
            Text("Prototype", header, new Vector2(.29f, 0), new Vector2(.71f, .45f), 12, "PLAYABLE LAB");
            next = Button("Next", header, new Vector2(.72f, 0), new Vector2(1, .45f), () => ChangeNode(1));
            status = Text("Feedback", safeRoot, new Vector2(.055f, .79f), new Vector2(.945f, .87f), 15, "");
            floorArea = Rect("Floors", safeRoot, new Vector2(.045f, .37f), new Vector2(.955f, .78f));
            receiver = Rect("Receiver", safeRoot, new Vector2(.05f, .235f), new Vector2(.95f, .35f)); Background(receiver, new Color(.24f, .27f, .29f), true);
            receiver.gameObject.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(ToggleTargetZoom);
            receiverLabel = Text("ReceiverLabel", receiver, new Vector2(.02f, .65f), new Vector2(.98f, .96f), 13, "EXACT RECEIVER · tap to enlarge");
            targetRow = Rect("TargetRow", receiver, new Vector2(.02f, .08f), new Vector2(.98f, .62f));
            precise = Rect("PreciseInput", safeRoot, new Vector2(.05f, .09f), new Vector2(.95f, .225f));
            Button("‹", precise, new Vector2(0, .5f), new Vector2(.2f, 1), () => MoveFocus(-1));
            focusLabel = Text("Focus", precise, new Vector2(.22f, .5f), new Vector2(.78f, 1), 20, "Choose a symbol");
            Button("›", precise, new Vector2(.8f, .5f), Vector2.one, () => MoveFocus(1));
            swapLeft = Button("Swap left", precise, Vector2.zero, new Vector2(.48f, .45f), () => ReportInput(Interaction.SwapFocused(-1)));
            swapRight = Button("Swap right", precise, new Vector2(.52f, 0), new Vector2(1, .45f), () => ReportInput(Interaction.SwapFocused(1)));
            var actions = Rect("Actions", safeRoot, new Vector2(.05f, .012f), new Vector2(.95f, .078f));
            undo = Button("Undo", actions, Vector2.zero, new Vector2(.30f, 1), Undo);
            hint = Button("Hint", actions, new Vector2(.35f, 0), new Vector2(.65f, 1), RequestHint);
            drop = Button("Drop", actions, new Vector2(.70f, 0), Vector2.one, () => ReportInput(Interaction.Execute(PuzzleCommand.Drop)));
            retry = Button("Retry", safeRoot, new Vector2(.05f, .09f), new Vector2(.43f, .155f), Retry);
            applyHint = Button("Play hint", safeRoot, new Vector2(.55f, .16f), new Vector2(.95f, .22f), () => ReportInput(Interaction.ApplyHint(offeredHint)));
            targetZoom = Rect("TargetZoom", safeRoot, Vector2.zero, Vector2.one); Background(targetZoom, Paper, true);
            Text("ZoomTitle", targetZoom, new Vector2(.05f, .7f), new Vector2(.95f, .8f), 22, "Deliver this exact order");
            Button("Close target", targetZoom, new Vector2(.2f, .22f), new Vector2(.8f, .30f), ToggleTargetZoom);
            targetZoom.gameObject.SetActive(false); applyHint.gameObject.SetActive(false);
        }
        void ChangeNode(int direction)
        {
            if (!Interaction.CanControl) return;
            int index = Catalog.PrimaryLevels.ToList().FindIndex(l => l.Definition.CampaignNodeId == node);
            index = Math.Max(0, Math.Min(Catalog.PrimaryLevels.Count - 1, index + direction)); LoadNode(Catalog.PrimaryLevels[index].Definition.CampaignNodeId);
        }
        void RebuildLevelViews(LevelDefinition level)
        {
            foreach (var view in floorViews) Destroy(view.gameObject);
            foreach (Transform child in targetRow) Destroy(child.gameObject);
            if (tokenRow != null) Destroy(tokenRow.gameObject);
            floorViews.Clear(); floorLabels.Clear(); deviceLabels.Clear(); slots.Clear(); slotImages.Clear(); tokenLabels.Clear(); targetLabels.Clear();
            int count = level.Floors.Count;
            for (int i = 0; i < count; i++) {
                float top = 1f - (float)i / count, bottom = 1f - (float)(i + 1) / count;
                var card = Rect("Floor" + (i + 1), floorArea, new Vector2(0, bottom + .012f), new Vector2(1, top - .012f));
                Background(card, Active, false); floorViews.Add(card);
                var title = Text("Durability", card, new Vector2(.025f, .78f), new Vector2(.975f, .99f), 14, ""); title.alignment = TextAlignmentOptions.Left; floorLabels.Add(title);
                var devices = Text("Devices", card, new Vector2(.025f, .015f), new Vector2(.975f, .32f), 11, ""); devices.alignment = TextAlignmentOptions.Left; deviceLabels.Add(devices);
            }
            tokenRow = Rect("PersistentRow", floorViews[0], new Vector2(.02f, .34f), new Vector2(.98f, .76f));
            var layout = tokenRow.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.spacing = 3; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = layout.childForceExpandHeight = true;
            for (int i = 0; i < level.InitialSymbolIds.Count; i++) {
                var cell = Rect("Slot" + (i + 1), tokenRow, Vector2.zero, Vector2.one);
                var image = Background(cell, new Color(.42f, .46f, .49f), true); slotImages.Add(image);
                var slot = cell.gameObject.AddComponent<TokenSlot>(); slot.Game = this; slot.Slot = i; slots.Add(slot);
                tokenLabels.Add(Text("Symbol", cell, Vector2.zero, Vector2.one, 23, ""));
            }
            AddSymbolLabels(targetRow, level.TargetSymbolIds, 23, targetLabels); Canvas.ForceUpdateCanvases();
        }
        void AddSymbolLabels(RectTransform parent, IEnumerable<SymbolId> symbols, int size, List<TextMeshProUGUI> labels)
        {
            var layout = parent.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>() ?? parent.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.spacing = 3; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = layout.childForceExpandHeight = true;
            foreach (var symbol in symbols) labels.Add(Text("TargetSymbol", parent, Vector2.zero, Vector2.one, size, DisplaySymbol(symbol)));
        }
        void ApplySafeArea()
        {
            safeArea = Screen.safeArea; screenWidth = Screen.width; screenHeight = Screen.height;
            if (safeRoot == null || screenWidth == 0 || screenHeight == 0) return;
            safeRoot.anchorMin = new Vector2(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight);
            safeRoot.anchorMax = new Vector2(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight);
        }
        void RenderState()
        {
            var attempt = Interaction.Attempt; var state = attempt.Current; var level = attempt.Level;
            for (int i = 0; i < level.Floors.Count; i++) {
                var floor = level.Floors[i]; bool active = state.Status == PuzzleStatus.Playing && i == state.ActiveFloor;
                bool past = i < state.ActiveFloor || state.Status != PuzzleStatus.Playing;
                floorViews[i].GetComponent<UnityEngine.UI.Image>().color = active ? Active : past ? new Color(.20f, .22f, .24f) : new Color(.24f, .27f, .30f);
                floorLabels[i].text = "FLOOR " + (i + 1) + "  ·  " + (past ? "BROKEN" : "Durability " + (active ? state.Durability : floor.BaseDurability));
                deviceLabels[i].text = DescribeFloor(floor, active ? state : null);
            }
            tokenRow.SetParent(state.Status == PuzzleStatus.Playing ? floorViews[state.ActiveFloor] : receiver, false);
            tokenRow.anchorMin = new Vector2(.02f, state.Status == PuzzleStatus.Playing ? .34f : .08f);
            tokenRow.anchorMax = new Vector2(.98f, state.Status == PuzzleStatus.Playing ? .76f : .62f); tokenRow.offsetMin = tokenRow.offsetMax = Vector2.zero;
            tokenRow.gameObject.SetActive(state.Status == PuzzleStatus.Playing);
            for (int i = 0; i < tokenLabels.Count; i++) tokenLabels[i].text = DisplaySymbol(state.Row[i]);
            receiverLabel.text = state.Status == PuzzleStatus.Won ? "DELIVERED · " + attempt.RouteLength + " actions" : "EXACT RECEIVER · tap to enlarge";
            status.text = userPaused ? "Paused. Devices wait for your next command." : state.Status == PuzzleStatus.Won ? "Exact order delivered." : state.Status == PuzzleStatus.Lost
                ? "The delivered row differs from the target. Retry or Undo." : "Swap neighbours. Match the receiver below.";
            if (state.Status == PuzzleStatus.Lost) {
                receiverLabel.text = "DELIVERED ROW · target remains above";
                tokenRow.gameObject.SetActive(true); tokenRow.anchorMin = new Vector2(.02f, -.36f); tokenRow.anchorMax = new Vector2(.98f, .06f);
            }
            RenderFocus(); RenderControls();
        }
        static string DescribeFloor(FloorDefinition floor, PuzzleState state)
        {
            var parts = new List<string>();
            if (floor.EntryTransform != EntryTransform.None) parts.Add("Entry: " + floor.EntryTransform);
            parts.Add("Edges: " + string.Join(" ", floor.OpenEdges.Select((open, i) => (i + 1) + (open || (state?.KeyUsed == true && floor.Key?.EdgeToOpen == i) ? " open" : " locked")
                + (floor.FragileEdges.Contains(i) ? " −2" : ""))));
            if (floor.LinkedPair.HasValue) parts.Add("Linked " + (floor.LinkedPair.Value.FirstEdge + 1) + " + " + (floor.LinkedPair.Value.SecondEdge + 1));
            if (floor.Bomb != null) parts.Add("Bomb " + (state == null ? floor.Bomb.Ticks.ToString() : state.BombPhase == BombPhase.Armed ? state.BombTicks.ToString() : state.BombPhase.ToString()) + " / slot " + (floor.Bomb.Slot + 1) + DisplaySymbol(floor.Bomb.RequiredSymbol));
            if (floor.Key != null) parts.Add("Key slot " + (floor.Key.Slot + 1) + DisplaySymbol(floor.Key.RequiredSymbol) + " → edge " + (floor.Key.EdgeToOpen + 1));
            if (floor.Repair != null) parts.Add("Repair slot " + (floor.Repair.Slot + 1) + DisplaySymbol(floor.Repair.RequiredSymbol) + " +" + floor.Repair.Amount);
            if (floor.OneUseBridgeEdge.HasValue) parts.Add("Bridge " + (floor.OneUseBridgeEdge.Value + 1) + (state?.BridgeUsed == true ? " used" : " once"));
            if (floor.OptionalStamp != null) parts.Add("Stamp slot " + (floor.OptionalStamp.Slot + 1) + DisplaySymbol(floor.OptionalStamp.RequiredSymbol));
            return string.Join(" · ", parts);
        }
        void RenderFocus()
        {
            if (Interaction == null) return;
            for (int i = 0; i < slotImages.Count; i++) slotImages[i].color = Interaction.SelectedSlot == i ? Selected : new Color(.42f, .46f, .49f);
            int? selected = Interaction.SelectedSlot;
            focusLabel.text = selected.HasValue ? "Slot " + (selected.Value + 1) + "  " + DisplaySymbol(Interaction.Attempt.Current.Row[selected.Value]) : "Choose a symbol";
            swapLeft.interactable = Interaction.CanPlay && selected.HasValue && selected > 0;
            swapRight.interactable = Interaction.CanPlay && selected.HasValue && selected < slots.Count - 1;
        }
        void RenderControls()
        {
            if (Interaction == null) return;
            undo.interactable = Interaction.CanControl && Interaction.Attempt.CanUndo;
            retry.interactable = Interaction.CanControl; retry.gameObject.SetActive(Interaction.Attempt.Current.Status != PuzzleStatus.Playing);
            drop.interactable = Interaction.CanPlay; hint.interactable = Interaction.CanPlay && hintTask == null;
            applyHint.interactable = Interaction.CanPlay && offeredHint != null && Interaction.Attempt.IsCurrent(offeredHint);
            pause.GetComponentInChildren<TextMeshProUGUI>().text = userPaused ? "Resume" : "Pause";
            previous.interactable = next.interactable = Interaction.CanControl;
            PreciseInputActive = ForcePreciseInput || PuzzleInteraction.NeedsPreciseInput(slots.Count, safeRoot.rect.width);
            precise.gameObject.SetActive(PreciseInputActive && Interaction.Attempt.Current.Status == PuzzleStatus.Playing);
            RenderFocus();
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
        TextMeshProUGUI Text(string name, Transform parent, Vector2 min, Vector2 max, int size, string content)
        {
            var rect = Rect(name, parent, min, max); var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.fontSize = size; text.enableAutoSizing = false; text.color = Ink;
            text.text = content; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal; return text;
        }
        UnityEngine.UI.Button Button(string label, Transform parent, Vector2 min, Vector2 max, Action clicked)
        {
            var rect = Rect(label.Replace(" ", ""), parent, min, max); Background(rect, new Color(.34f, .38f, .41f), true);
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.onClick.AddListener(() => clicked());
            var text = Text("Label", rect, Vector2.zero, Vector2.one, 16, label); text.margin = new Vector4(3, 2, 3, 2); return button;
        }
    }
}
