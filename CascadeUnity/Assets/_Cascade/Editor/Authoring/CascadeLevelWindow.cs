using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Solver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cascade.Editor.Authoring
{
    public sealed class CascadeLevelWindow : EditorWindow
    {
        const string LayoutPath = "Assets/_Cascade/Editor/Authoring/CascadeLevelWindow.uxml";
        [SerializeField] LevelDraft draft;
        [SerializeField] string templateId = "C01";
        ContentCatalog catalog;
        JArray primarySources;
        PublicationProof proof;
        Task<PublicationProof> solveTask;
        CancellationTokenSource cancellation;
        int revision, solvingRevision, replayIndex;
        string message = "Validate checks the row and devices. Solve proves the selected goal.";
        VisualElement body, report;
        List<string> symbols;
        readonly Dictionary<string, bool> expanded = new Dictionary<string, bool>();

        [MenuItem("CASCADE/Level Workshop")]
        public static CascadeLevelWindow ShowWindow()
        {
            var window = GetWindow<CascadeLevelWindow>(); window.titleContent = new GUIContent("CASCADE Workshop");
            window.minSize = new Vector2(820, 570); window.Show(); return window;
        }
        void OnEnable() { Undo.undoRedoPerformed += OnUndo; EditorApplication.update += PollSolver; }
        void OnDisable() { Undo.undoRedoPerformed -= OnUndo; EditorApplication.update -= PollSolver; CancelSolve(); }
        void OnDestroy() { if (draft != null) DestroyImmediate(draft); }
        void OnUndo() { Invalidate(); Build(); }
        static string Resource(string name) => Resources.Load<TextAsset>("CascadeContent/" + name)?.text
            ?? throw new InvalidOperationException("Missing content resource: " + name);
        public void CreateGUI()
        {
            catalog = ContentImporter.Import(Resource("levels"), Resource("localized_levels"), Resource("words_en"), Resource("words_ru"), Resource("words_uk"));
            primarySources = JArray.Parse(Resource("levels"));
            if (draft == null) { draft = CreateInstance<LevelDraft>(); draft.hideFlags = HideFlags.HideAndDontSave; }
            Build();
        }
        void Build()
        {
            if (catalog == null || draft == null) return;
            rootVisualElement.Clear(); AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LayoutPath).CloneTree(rootVisualElement);
            body = rootVisualElement.Q("draftBody"); report = rootVisualElement.Q("reportBody");
            symbols = catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).SelectMany(l => l.Definition.TargetSymbolIds)
                .Select(s => s.PersistedId).Concat(draft.initial).Concat(draft.target).Distinct().OrderBy(v => v, StringComparer.Ordinal).ToList();
            var toolbar = rootVisualElement.Q("toolbar");
            var templates = primarySources.Select(l => (string)l["id"]).ToList();
            var picker = new DropdownField("Example", templates, Math.Max(0, templates.IndexOf(templateId)));
            picker.RegisterValueChangedCallback(e => templateId = e.newValue); toolbar.Add(picker);
            Button(toolbar, "Load example", LoadExample);
            Button(toolbar, "New level", () => Edit("New CASCADE level", () => {
                var fresh = CreateInstance<LevelDraft>(); draft.Read(fresh.Source()); DestroyImmediate(fresh);
            }, true));
            Button(toolbar, "Duplicate", () => Edit("Duplicate CASCADE draft", () => {
                draft.puzzleId += "_copy"; draft.nodeId = draft.puzzleId; draft.title += " (copy)";
            }, true));
            Label(body, "Identity & row", "section-title");
            Field(body, new TextField("Puzzle ID") { isDelayed = true }, draft.puzzleId, v => draft.puzzleId = v);
            Field(body, new TextField("Campaign node") { isDelayed = true }, draft.nodeId, v => draft.nodeId = v);
            Field(body, new TextField("Title") { isDelayed = true }, draft.title, v => draft.title = v);
            Field(body, new TextField("Lesson") { isDelayed = true }, draft.lesson, v => draft.lesson = v);
            Field(body, new DropdownField("Puzzle language", new List<string> { "en", "ru", "uk" }, 0), draft.locale, v => draft.locale = v, true);
            Field(body, new IntegerField("Slots") { isDelayed = true }, draft.target.Length, draft.ResizeRow, true);
            RowEditor("Target / receiver", draft.target); RowEditor("Initial row", draft.initial);
            var rowTools = Row(body);
            Button(rowTools, "Copy target → initial", () => Edit("Copy target", () => draft.initial = (string[])draft.target.Clone(), true));
            Button(rowTools, "Rotate initial left", () => Edit("Rotate row", () => draft.initial = draft.initial.Skip(1).Concat(draft.initial.Take(1)).ToArray(), true));
            for (int i = 0; i < draft.initial.Length - 1; i++) {
                int edge = i; Button(rowTools, "Swap " + (i + 1) + "↔" + (i + 2), () => Edit("Scramble row", () => {
                    string first = draft.initial[edge]; draft.initial[edge] = draft.initial[edge + 1]; draft.initial[edge + 1] = first;
                }, true));
            }
            WordEditor();
            Label(body, "Floors / top to bottom", "section-title");
            Field(body, new IntegerField("Floor count") { isDelayed = true }, draft.floors.Length, draft.ResizeFloors, true);
            for (int i = 0; i < draft.floors.Length; i++) FloorEditor(i);
            Label(body, "Publication goal", "section-title");
            Field(body, new Toggle("Require every stamp"), draft.requireStamps, v => draft.requireStamps = v);
            Label(body, "Unchecked: exact receiver. Checked: exact receiver plus all placed stamps. Changing a field invalidates the proof.", "hint-text");
            DrawReport();
        }
        void LoadExample()
        {
            Edit("Load CASCADE example", () => draft.Read((JObject)primarySources.Single(l => (string)l["id"] == templateId)), true);
        }
        void RowEditor(string title, string[] values)
        {
            Label(body, title, "section-title"); var row = Row(body);
            for (int i = 0; i < values.Length; i++) {
                int slot = i; var cell = new VisualElement(); cell.AddToClassList("symbol-picker"); row.Add(cell);
                Label(cell, "SLOT " + (i + 1), "slot-caption");
                var field = SymbolField("", values[i]); Field(cell, field, values[i], v => values[slot] = v);
            }
        }
        DropdownField SymbolField(string label, string value)
        {
            var choices = new List<string>(symbols); if (!choices.Contains(value)) choices.Add(value);
            return new DropdownField(label, choices, Math.Max(0, choices.IndexOf(value)), SymbolLabel, SymbolLabel);
        }
        static string SymbolLabel(string id)
        {
            try { var symbol = SymbolId.Parse(id); return symbol.Kind == SymbolKind.Shape ? "Shape " + symbol.Value : symbol.Value + " · " + symbol.Locale; }
            catch { return id; }
        }
        void WordEditor()
        {
            var words = catalog.Words.Values.Where(w => w.Locale == draft.locale && w.Graphemes.Count <= draft.target.Length).OrderBy(w => w.DisplayText).ToArray();
            var fold = Fold(body, "words", "Word from dictionary", false);
            if (words.Length == 0) return;
            var choices = words.Select(w => w.WordId).ToList(); int slot = 0;
            var field = new DropdownField("Word", choices, 0, id => catalog.Words[id].DisplayText, id => catalog.Words[id].DisplayText); fold.Add(field);
            var start = new IntegerField("First slot") { value = 1 }; start.RegisterValueChangedCallback(e => slot = e.newValue - 1); fold.Add(start);
            Button(fold, "Place in target", () => {
                try { Edit("Place dictionary word", () => { draft.ApplyWord(catalog.Words[field.value], slot); RebuildWordIds(); }, true); }
                catch (Exception e) { message = e.Message; DrawReport(); }
            });
            Label(fold, "Place words first, then copy target to initial and scramble. Word metadata still needs native and license review before release.", "hint-text");
        }
        void RebuildWordIds()
        {
            var ids = new List<string>();
            for (int i = 0; i < draft.target.Length;) {
                if (SymbolId.Parse(draft.target[i], draft.locale).Kind == SymbolKind.Shape) { i++; continue; }
                int first = i;
                var word = catalog.Words.Values.Where(w => w.Locale == draft.locale && first + w.Graphemes.Count <= draft.target.Length)
                    .OrderByDescending(w => w.Graphemes.Count).ThenBy(w => w.WordId, StringComparer.Ordinal).FirstOrDefault(w => w.Graphemes
                    .Select((g, n) => SymbolId.Parse(draft.target[first + n], draft.locale).Equals(SymbolId.Parse("g:" + draft.locale + ":" + g))).All(v => v));
                if (word == null) { draft.wordIds = Array.Empty<string>(); return; }
                ids.Add(word.WordId); i += word.Graphemes.Count;
            }
            draft.wordIds = ids.ToArray();
        }
        void FloorEditor(int index)
        {
            var floor = draft.floors[index]; var fold = Fold(body, "floor" + index, "Floor " + (index + 1) + " / durability " + floor.hp, index == 0);
            fold.AddToClassList("card");
            Field(fold, new IntegerField("Durability") { isDelayed = true }, floor.hp, v => floor.hp = v);
            Field(fold, new EnumField("Entry transform", floor.entry), (Enum)floor.entry, v => floor.entry = (EntryTransform)v);
            Label(fold, "Edges — an exchange spends 1 + crossed fragile edges. Linked edges swap together.", "hint-text");
            var edges = Row(fold);
            for (int i = 0; i < floor.open.Length; i++) {
                int edge = i; var cell = new VisualElement(); cell.AddToClassList("edge-card"); edges.Add(cell);
                Label(cell, (i + 1) + " ↔ " + (i + 2), "slot-caption");
                Field(cell, new Toggle("Open"), floor.open[i], v => floor.open[edge] = v);
                Field(cell, new Toggle("Fragile"), floor.fragile[i], v => floor.fragile[edge] = v);
            }
            Field(fold, new Toggle("Linked pair"), floor.linked, v => floor.linked = v, true);
            if (floor.linked) { EdgeField(fold, "First linked edge", floor.firstEdge, v => floor.firstEdge = v); EdgeField(fold, "Second linked edge", floor.secondEdge, v => floor.secondEdge = v); }
            Field(fold, new Toggle("One-use bridge"), floor.bridge, v => floor.bridge = v, true);
            if (floor.bridge) EdgeField(fold, "Bridge edge", floor.bridgeEdge, v => floor.bridgeEdge = v);
            ContactEditor(fold, index, "Bomb", floor.bomb); ContactEditor(fold, index, "Key", floor.key);
            ContactEditor(fold, index, "Repair", floor.repair); ContactEditor(fold, index, "Stamp", floor.stamp);
        }
        void EdgeField(VisualElement parent, string label, int value, Action<int> set)
        {
            var choices = Enumerable.Range(0, draft.target.Length - 1).Select(i => (i + 1) + " ↔ " + (i + 2)).ToList();
            int safe = Math.Max(0, Math.Min(choices.Count - 1, value));
            Field(parent, new DropdownField(label, choices, safe), choices[safe], v => set(choices.IndexOf(v)));
        }
        void ContactEditor(VisualElement parent, int floor, string kind, ContactDraft contact)
        {
            var fold = Fold(parent, "contact" + floor + kind, kind, contact.enabled);
            Field(fold, new Toggle("Enabled"), contact.enabled, v => contact.enabled = v, true);
            if (!contact.enabled) return;
            Field(fold, new IntegerField("Sensor slot") { isDelayed = true }, contact.slot + 1, v => contact.slot = v - 1);
            Field(fold, SymbolField("Required symbol", contact.symbol), contact.symbol, v => contact.symbol = v);
            if (kind == "Bomb") {
                Field(fold, new IntegerField("Ticks") { isDelayed = true }, contact.ticks, v => contact.ticks = v);
                Field(fold, new IntegerField("Damage") { isDelayed = true }, contact.damage, v => contact.damage = v);
            }
            else if (kind == "Key") EdgeField(fold, "Opens edge", contact.value, v => contact.value = v);
            else if (kind == "Repair") Field(fold, new IntegerField("Repair amount") { isDelayed = true }, contact.value, v => contact.value = v);
        }
        void Edit(string name, Action mutation, bool rebuild = false)
        {
            Undo.RecordObject(draft, name); mutation(); EditorUtility.SetDirty(draft); Invalidate();
            if (rebuild) rootVisualElement.schedule.Execute(Build); else DrawReport();
        }
        void Invalidate() { revision++; proof = null; replayIndex = 0; CancelSolve(); message = "Draft changed. Validate and solve this version before export."; }
        void Field<T>(VisualElement parent, BaseField<T> field, T value, Action<T> set, bool rebuild = false)
        {
            field.SetValueWithoutNotify(value); field.AddToClassList("form-field"); parent.Add(field);
            field.RegisterValueChangedCallback(e => Edit("Edit CASCADE draft", () => set(e.newValue), rebuild));
        }
        Foldout Fold(VisualElement parent, string key, string title, bool initial)
        {
            var fold = new Foldout { text = title, value = expanded.TryGetValue(key, out bool value) ? value : initial }; parent.Add(fold);
            fold.RegisterValueChangedCallback(e => { if (e.target == fold) expanded[key] = e.newValue; }); return fold;
        }
        static VisualElement Row(VisualElement parent) { var row = new VisualElement(); row.AddToClassList("row"); parent.Add(row); return row; }
        static Label Label(VisualElement parent, string text, string className) { var label = new Label(text); label.AddToClassList(className); parent.Add(label); return label; }
        static Button Button(VisualElement parent, string title, Action action) { var button = new Button(action) { text = title }; parent.Add(button); return button; }
        void ValidateDraft()
        {
            try { var level = draft.Definition(catalog.Words); message = "Valid · " + level.InitialSymbolIds.Count + " slots · " + level.Floors.Count + " floors\nRules " + level.RulesVersion + " · schema " + level.SchemaVersion + "\nSHA256 " + level.ContentHash; }
            catch (Exception e) { message = "Validation failed\n" + e.Message; }
            DrawReport();
        }
        void StartSolve()
        {
            try {
                var level = draft.Definition(catalog.Words); var goal = draft.Goal(); CancelSolve(); proof = null; solvingRevision = revision;
                cancellation = new CancellationTokenSource(); var token = cancellation.Token;
                solveTask = Task.Run(() => PublicationProof.Analyze(level, goal, cancellation: token));
                message = "Solving in background · maximum 200,000 states. Cancel is safe.";
            }
            catch (Exception e) { message = "Cannot solve\n" + e.Message; }
            DrawReport();
        }
        void CancelSolve() { cancellation?.Cancel(); cancellation?.Dispose(); cancellation = null; solveTask = null; }
        void PollSolver()
        {
            if (solveTask == null || !solveTask.IsCompleted) return;
            var task = solveTask; solveTask = null; cancellation?.Dispose(); cancellation = null;
            if (solvingRevision != revision) return;
            if (task.IsFaulted) message = task.Exception?.GetBaseException().Message ?? "Solver failed.";
            else if (task.IsCanceled) message = "Cancelled. No publication proof.";
            else { proof = task.Result; replayIndex = 0; message = proof.Search.Status + " · minimum " + (proof.Search.OptimalActions?.ToString() ?? "unproved")
                + " actions\n" + proof.Search.ExploredStates + " explored states\n" + proof.Search.LimitReason; }
            DrawReport(); Repaint();
        }
        void DrawReport()
        {
            if (report == null) return; report.Clear(); Label(report, "Check & replay", "section-title");
            var tools = new VisualElement(); tools.AddToClassList("report-row"); report.Add(tools);
            Button(tools, "Validate", ValidateDraft);
            var solve = Button(tools, draft.requireStamps ? "Test optional objective" : "Solve", StartSolve); solve.AddToClassList("primary-button"); solve.SetEnabled(solveTask == null);
            if (solveTask != null) Button(tools, "Cancel", () => { CancelSolve(); message = "Cancelled. No publication proof."; DrawReport(); });
            Label(report, message, "report-text");
            if (proof == null) return;
            var metrics = proof.Search.Metrics;
            if (metrics != null) Label(report, metrics.States + " states / " + metrics.Transitions + " transitions\nDead states: " + metrics.DeadStates
                + " · first at " + (metrics.EarliestDeadDepth?.ToString() ?? "—") + "\nNear-optimal first choices: " + metrics.NearOptimalFirstStatesPlusTwo
                + "\nForced ratio: " + metrics.ForcedRatio.ToString("P0"), "report-text");
            foreach (string warning in proof.Warnings) Label(report, warning, "warning");
            if (!proof.Search.CanPublishGoal) return;
            Label(report, "Replay / " + replayIndex + " of " + proof.Search.Path.Count, "section-title");
            var resolver = new PuzzleResolver(proof.Level); var state = resolver.CreateInitialState(); TransitionResult last = null;
            for (int i = 0; i < replayIndex; i++) { last = resolver.Resolve(state, proof.Search.Path[i]); state = last.State; }
            var tokens = new VisualElement(); tokens.AddToClassList("replay-row"); report.Add(tokens);
            foreach (var symbol in state.Row) Label(tokens, SymbolLabel(symbol.PersistedId), "replay-token");
            Label(report, state.Status + " · floor " + Math.Min(state.ActiveFloor + 1, proof.Level.Floors.Count) + " · durability " + state.Durability
                + "\nBomb " + state.BombPhase + " / " + state.BombTicks + " · stamps " + state.StampBits
                + "\nKey " + state.KeyUsed + " · repair " + state.RepairUsed + " · bridge " + state.BridgeUsed, "report-text");
            if (last != null) Label(report, string.Join(" → ", last.Events.Select(e => e.Kind.ToString())), "hint-text");
            if (replayIndex < proof.Search.Path.Count) {
                var next = proof.Search.Path[replayIndex]; var preview = resolver.Preview(state, next);
                Label(report, "Next: " + (next.Kind == CommandKind.EarlyDrop ? "Drop" : "swap " + (next.Edge + 1) + "↔" + (next.Edge + 2))
                    + " · durability cost " + preview.DurabilityCost, "report-text");
            }
            var replayTools = Row(report); Button(replayTools, "Reset", () => { replayIndex = 0; DrawReport(); });
            Button(replayTools, "Previous", () => { replayIndex = Math.Max(0, replayIndex - 1); DrawReport(); }).SetEnabled(replayIndex > 0);
            Button(replayTools, "Next", () => { replayIndex = Math.Min(proof.Search.Path.Count, replayIndex + 1); DrawReport(); }).SetEnabled(replayIndex < proof.Search.Path.Count);
            Button(report, "Show dead states", () => {
                message = "Dead states: " + metrics.DeadStates + ". Earliest depth: " + (metrics.EarliestDeadDepth?.ToString() ?? "none") + ". Complete graph only; this does not measure fun."; DrawReport();
            });
            Button(report, "Export validated level…", Export).AddToClassList("primary-button");
            Label(report, "Export stores the source and a separate version/hash/goal proof. Existing campaign files are not overwritten.", "hint-text");
        }
        void Export()
        {
            try {
                var level = draft.Definition(catalog.Words); var metadata = proof.ExportMetadata(level, draft.Goal());
                string path = EditorUtility.SaveFilePanel("Export validated CASCADE level", UnityEngine.Application.dataPath, draft.puzzleId, "json");
                if (string.IsNullOrEmpty(path)) return;
                string proofPath = Path.ChangeExtension(path, ".proof.json");
                if (File.Exists(path) || File.Exists(proofPath)) throw new IOException("Choose a new filename to preserve existing level and proof files.");
                File.WriteAllText(path, draft.Source().ToString(Formatting.Indented));
                File.WriteAllText(proofPath, metadata.ToString(Formatting.Indented)); AssetDatabase.Refresh();
                message = "Exported " + Path.GetFileName(path) + " and matching proof."; DrawReport();
            }
            catch (Exception e) { message = "Export blocked\n" + e.Message; DrawReport(); }
        }
    }
}
