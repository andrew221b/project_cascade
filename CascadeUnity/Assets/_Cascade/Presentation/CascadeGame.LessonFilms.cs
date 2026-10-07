using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        sealed class FilmPlan
        {
            public LevelDefinition level;
            public PuzzleState before, after;
            public IReadOnlyList<SymbolId> exchanged;
            public PuzzleCommand command;
            public int deviceKind = -1, blockedEdge = -1;
            public ContactDefinition contact;
            public bool descent, recovery, tool, preview;
            public int woodBefore, woodAfter;
        }

        // All rule outcomes are computed once from an isolated resolver / attempt.
        // Painting interpolates those outcomes; it cannot execute a live command.
        FilmPlan LessonFilmPlan(MechanicLesson lesson, bool? right)
        {
            bool words = Interaction.Attempt.Mode == PuzzleMode.Words;
            bool mixed = Interaction.Attempt.Mode == PuzzleMode.Mixed;
            string locale = Interaction.Attempt.ActiveLocale;
            string alphabet = locale == "en" ? "STAR" : locale == "ru" ? "МОСТ" : "МІСТ";
            var row = words ? alphabet.Select(c => SymbolId.Parse("g:" + locale + ":" + c)).ToArray()
                : new[] { "circle", "triangle", "square", "diamond" }.Select(s => SymbolId.Parse("s:" + s)).ToArray();
            if (mixed)
            {
                row[2] = SymbolId.Parse("g:" + locale + ":" + alphabet[0]);
                row[3] = SymbolId.Parse("g:" + locale + ":" + alphabet[1]);
            }
            var open = new[] { true, true, true };
            var floor = new FloorDefinition(4, open);
            var lower = new FloorDefinition(4, open);
            int kind = -1;
            ContactDefinition contact = null;
            var command = PuzzleCommand.Swap(0);
            bool descentFilm = false, recovery = false, preview = false;
            bool tool = lesson >= MechanicLesson.Wayfinder && lesson <= MechanicLesson.LandingLens
                || lesson == MechanicLesson.Hint || lesson == MechanicLesson.Undo || lesson == MechanicLesson.Toolbag;
            switch (lesson)
            {
                case MechanicLesson.Bomb:
                    kind = 2; contact = new BombDefinition(1, row[0], 2, 2);
                    floor = new FloorDefinition(4, open, bomb: (BombDefinition)contact); break;
                case MechanicLesson.Key:
                    kind = 3; contact = new KeyDefinition(1, row[0], 2);
                    floor = new FloorDefinition(4, new[] { true, true, false }, key: (KeyDefinition)contact); break;
                case MechanicLesson.Repair:
                    kind = 4; contact = new RepairDefinition(1, row[0], 2);
                    floor = new FloorDefinition(2, open, repair: (RepairDefinition)contact); break;
                case MechanicLesson.Ribbon:
                    kind = 9; contact = new PowerupDefinition(1, row[0], PowerupKind.Ribbon);
                    floor = new FloorDefinition(4, open, powerup: (PowerupDefinition)contact); break;
                case MechanicLesson.Stamp:
                    kind = 8; contact = new ContactDefinition(1, row[0]);
                    floor = new FloorDefinition(4, open, optionalStamp: contact); break;
                case MechanicLesson.BombHourglass:
                    kind = 10; contact = new PowerupDefinition(1, row[0], PowerupKind.Hourglass);
                    floor = new FloorDefinition(4, open, bomb: new BombDefinition(3, row[0], 3, 2), powerup: (PowerupDefinition)contact); break;
                case MechanicLesson.Bridge:
                    command = PuzzleCommand.Swap(1); floor = new FloorDefinition(4, open, oneUseBridgeEdge: 1); break;
                case MechanicLesson.LinkedPairs:
                    floor = new FloorDefinition(4, open, linkedPair: new LinkedPair(0, 2)); break;
                case MechanicLesson.Fragile:
                    floor = new FloorDefinition(4, open, fragileEdges: new[] { 0 }); break;
                case MechanicLesson.ClosedPath:
                    command = PuzzleCommand.Swap(1); floor = new FloorDefinition(4, new[] { false, true, true }); break;
                case MechanicLesson.Rollers:
                case MechanicLesson.LandingWind:
                case MechanicLesson.Mirror:
                    lower = new FloorDefinition(4, open, entryTransform: lesson == MechanicLesson.Mirror ? EntryTransform.MirrorNext
                        : right == true ? EntryTransform.ShiftRightOne : EntryTransform.ShiftLeftOne);
                    command = PuzzleCommand.Drop; descentFilm = true; break;
                case MechanicLesson.Descent:
                    command = PuzzleCommand.Drop; descentFilm = true; break;
                case MechanicLesson.LiveGust:
                    floor = new FloorDefinition(4, open, gustAfterSwaps: 1, gustTransform: EntryTransform.ShiftLeftOne, gustDamage: 1); break;
                case MechanicLesson.ReturnHourglass:
                    floor = new FloorDefinition(1, open); recovery = true; break;
                case MechanicLesson.LandingLens:
                    lower = new FloorDefinition(4, open, entryTransform: EntryTransform.MirrorNext); preview = true; break;
                case MechanicLesson.FarSwap:
                    command = PuzzleCommand.Reach(0, 3); break;
            }
            var target = row.ToArray(); var temp = target[0]; target[0] = target[1]; target[1] = temp;
            var segment = new TargetSegment(words ? SegmentKind.Word : SegmentKind.Shapes, 0, 4, words ? "film-word" : null);
            var segments = mixed ? new[] { new TargetSegment(SegmentKind.Shapes, 0, 2), new TargetSegment(SegmentKind.Word, 2, 2, "film-word") }
                : new[] { segment };
            var level = new LevelDefinition("lesson-film-" + (int)lesson, "lesson-film", "1", "2.0", "isolated-lesson-film",
                row, target, segments, new[] { floor, lower }, lesson == MechanicLesson.FarSwap ? 1 : 0);
            var resolver = new PuzzleResolver(level);
            var before = resolver.CreateInitialState();
            TransitionResult result;
            if (lesson == MechanicLesson.Undo || lesson == MechanicLesson.UndoSpool || recovery)
            {
                var sample = new AttemptService(level, locale);
                if (lesson == MechanicLesson.UndoSpool)
                    for (int n = 0; n < 2; n++) { sample.Execute(PuzzleCommand.Swap(0)); sample.Undo(); }
                sample.Execute(recovery ? PuzzleCommand.Swap(0) : command);
                before = sample.Current;
                bool used = lesson == MechanicLesson.Undo ? sample.Undo()
                    : sample.TryUseToolAtomically(recovery ? TravelTool.ReturnHourglass : TravelTool.UndoSpool, () => true);
                if (!used) throw new InvalidOperationException("Isolated tutorial recovery is not legal.");
                return new FilmPlan { level = level, before = before, after = sample.Current, exchanged = sample.Current.Row,
                    command = command, recovery = recovery, tool = true, woodBefore = before.Durability, woodAfter = sample.Current.Durability };
            }
            if (lesson == MechanicLesson.Wayfinder || lesson == MechanicLesson.SignalLantern || lesson == MechanicLesson.Hint || preview)
            {
                var sample = new AttemptService(level, locale);
                var offer = AttemptService.CalculateHint(sample.CaptureHintRequest());
                command = offer.Result.Command.Value;
                if (preview)
                {
                    if (!sample.TryUseToolAtomically(TravelTool.LandingLens, () => true)) throw new InvalidOperationException("Tutorial lens is not legal.");
                    command = PuzzleCommand.Drop;
                }
                else if (lesson != MechanicLesson.Hint)
                {
                    var choice = lesson == MechanicLesson.SignalLantern ? TravelTool.SignalLantern : TravelTool.Wayfinder;
                    if (!sample.TryUseToolAtomically(choice, () => true, offer)) throw new InvalidOperationException("Tutorial guidance is not legal.");
                }
            }
            result = resolver.Resolve(before, command);
            if (!result.Accepted) throw new InvalidOperationException("Tutorial command is not legal: " + lesson);
            var exchange = result.Events.FirstOrDefault(e => e.Kind == PuzzleEventKind.RowPermuted)?.Row ?? before.Row;
            var after = lesson == MechanicLesson.Gust || lesson == MechanicLesson.Toolbag ? before : result.State;
            if (lesson == MechanicLesson.Ribbon)
            {
                var reach = resolver.Resolve(after, PuzzleCommand.Reach(0, 3));
                if (!reach.Accepted) throw new InvalidOperationException("Tutorial ribbon did not grant a far swap.");
                after = reach.State;
            }
            var plan = new FilmPlan { level = level, before = before, after = after, exchanged = exchange, command = command,
                descent = descentFilm, recovery = recovery, preview = preview, tool = tool, deviceKind = kind, contact = contact,
                woodBefore = before.Durability, woodAfter = after.Durability,
                blockedEdge = lesson == MechanicLesson.ClosedPath ? 0 : lesson == MechanicLesson.Key ? 2 : -1 };
            return plan;
        }

        RectTransform FilmPiece(RectTransform parent, SymbolId symbol, string name)
        {
            var tile = Rect(name, parent, new Vector2(.5f, .5f), new Vector2(.5f, .5f));
            if (symbol.Kind == SymbolKind.Glyph)
            {
                var face = Background(tile, new Color(1, .96f, .83f), false); face.sprite = art.panel; face.type = Image.Type.Sliced;
                var glyph = Text("Raised lesson letter", tile, new Vector2(.06f, .04f), new Vector2(.94f, .96f), 35, UiCopy.Raw(symbol.Value));
                glyph.font = art.titleFont; glyph.enableAutoSizing = true; glyph.fontSizeMin = 22; glyph.fontSizeMax = 38;
                FinishLetter(glyph); glyph.raycastTarget = false;
                var rim = Rect("Glazed tile highlight", tile, new Vector2(.15f, .89f), new Vector2(.85f, .93f));
                Background(rim, new Color(1, 1, .96f, .65f), false).sprite = art.panel;
            }
            else tile.gameObject.AddComponent<ShapeFaceGraphic>().SetShape(art.SymbolMesh(symbol), art.SymbolMaterial(symbol).GetColor("_BaseColor"), true);
            return tile;
        }

        int[] FilmDestinations(IReadOnlyList<SymbolId> before, IReadOnlyList<SymbolId> after)
        {
            var result = new int[before.Count]; var taken = new bool[before.Count];
            for (int i = 0; i < before.Count; i++) for (int j = 0; j < after.Count; j++)
                if (!taken[j] && before[i] == after[j]) { result[i] = j; taken[j] = true; break; }
            return result;
        }

        void BuildLessonFilm(RectTransform root, MechanicLesson lesson, bool? right = null)
        {
            var plan = LessonFilmPlan(lesson, right);
            // Single-board lessons centre their one action instead of reserving
            // the lower half for a landing that the demonstration never uses.
            float rowY = plan.contact == null && !plan.tool && !plan.descent && !plan.recovery && !plan.preview
                && lesson != MechanicLesson.Gust && lesson != MechanicLesson.LiveGust ? .61f : .79f;
            float rowOffset = rowY - .79f;
            var stage = Rect("Lesson film", root, Vector2.zero, Vector2.one);
            var opacity = stage.gameObject.AddComponent<CanvasGroup>(); opacity.blocksRaycasts = false; opacity.interactable = false;
            var ground = Background(stage, new Color(.20f, .44f, .46f, .45f), false); ground.sprite = art.panel; ground.type = Image.Type.Sliced;
            var upper = Rect("Film upper wood", stage, new Vector2(.03f, .635f + rowOffset), new Vector2(.97f, .70f + rowOffset));
            var upperWood = upper.gameObject.AddComponent<WoodBoardGraphic>(); upperWood.SetSurface(plan.level.Floors[0]);
            var lower = Rect("Film landing wood", stage, new Vector2(.03f, .22f), new Vector2(.97f, .285f));
            var lowerWood = lower.gameObject.AddComponent<WoodBoardGraphic>(); lowerWood.SetSurface(plan.level.Floors[1]);
            lower.gameObject.SetActive(plan.descent || plan.recovery || plan.preview || lesson == MechanicLesson.LiveGust);
            var pieces = new RectTransform[4]; var glows = new ShapeFaceGraphic[4]; var glowAlpha = new CanvasGroup[4];
            var exchangeDest = FilmDestinations(plan.before.Row, plan.exchanged);
            var finalDest = FilmDestinations(plan.before.Row, plan.after.Row);
            for (int i = 0; i < 4; i++)
            {
                var glow = Rect("Film piece silhouette light " + i, stage, new Vector2(.5f, .5f), new Vector2(.5f, .5f));
                glows[i] = glow.gameObject.AddComponent<ShapeFaceGraphic>();
                glows[i].SetShape(art.SymbolMesh(plan.before.Row[i]), new Color(1, .87f, .36f, .26f));
                glowAlpha[i] = glow.gameObject.AddComponent<CanvasGroup>();
                pieces[i] = FilmPiece(stage, plan.before.Row[i], "Film piece " + i);
            }
            // Inset numeric wood counter, rather than another floating caption.
            var counter = Picture("Film wood inset", upper, new Vector2(.39f, -.14f), new Vector2(.61f, 1.08f), PlatformProp(3), true);
            counter.material = ToolSilhouette;
            var count = Text("Film wood number", counter.transform, Vector2.zero, Vector2.one, 15, UiCopy.Raw(plan.woodBefore.ToString()));
            count.font = art.titleFont; count.fontStyle = FontStyles.Bold; count.color = new Color(.34f, .16f, .05f); count.margin = Vector4.zero;
            count.raycastTarget = false; count.gameObject.AddComponent<SymbolGlyphCenter>();
            var button = Rect("Film action button", stage, new Vector2(.30f, .025f), new Vector2(.70f, .17f));
            var buttonFace = Background(button, Color.white, false); buttonFace.sprite = enamelControls[1]; buttonFace.type = Image.Type.Sliced;
            var caption = Text("Film action label", button, new Vector2(.09f, .12f), new Vector2(.91f, .88f), 17,
                plan.descent ? "Descend" : lesson == MechanicLesson.Undo ? "Undo" : lesson == MechanicLesson.Hint ? "Hint" : lesson == MechanicLesson.Toolbag ? "Tool bag" : "Use one");
            caption.font = art.titleFont; caption.fontStyle = FontStyles.Bold; caption.color = MenuInk;
            caption.enableAutoSizing = true; caption.fontSizeMin = 13; caption.fontSizeMax = 17; caption.textWrappingMode = TextWrappingModes.NoWrap;
            bool buttonShown = plan.descent || plan.tool;
            button.gameObject.SetActive(buttonShown);
            RectTransform prop = null; Image propFace = null; CanvasGroup propAlpha = null;
            ToolSuspensionRope cord = null; RectTransform cordRect = null;
            if (plan.deviceKind >= 0)
            {
                float x = (plan.contact.Slot + .5f) / 4;
                cordRect = Rect("Film single device cord", stage, Vector2.zero, Vector2.one);
                cord = cordRect.gameObject.AddComponent<ToolSuspensionRope>();
                propFace = Picture("Film contact body", stage, new Vector2(x - .11f, .43f), new Vector2(x + .11f, .615f), ToolBody(plan.deviceKind), true);
                propFace.material = ToolSilhouette; prop = propFace.rectTransform;
                if (plan.deviceKind == 2) BuildBombMark(prop, plan.contact.RequiredSymbol);
                else BuildToolBodyMark(prop, plan.deviceKind, plan.contact.RequiredSymbol);
                propAlpha = prop.gameObject.AddComponent<CanvasGroup>();
            }
            else if (plan.tool)
            {
                int index = lesson == MechanicLesson.ReturnHourglass ? 2 : lesson == MechanicLesson.UndoSpool || lesson == MechanicLesson.Undo ? 1
                    : lesson == MechanicLesson.SignalLantern || lesson == MechanicLesson.Hint ? 3 : lesson == MechanicLesson.LandingLens ? 4 : 0;
                var sprite = lesson == MechanicLesson.Toolbag && navigationIcons != null ? navigationIcons[4] : ToolArt(index);
                propFace = Picture("Film tool", stage, new Vector2(.395f, .37f), new Vector2(.605f, .575f), sprite, true);
                propFace.material = ToolSilhouette; prop = propFace.rectTransform; propAlpha = prop.gameObject.AddComponent<CanvasGroup>();
            }
            RectTransform branch = null, bridge = null;
            if (plan.blockedEdge >= 0 || lesson == MechanicLesson.Bridge)
            {
                int edge = plan.blockedEdge >= 0 ? plan.blockedEdge : 1; float x = (edge + 1) / 4f;
                if (introBranch == null)
                {
                    var tex = Resources.Load<Texture2D>("CascadeArt/Woodland/BranchStops");
                    introBranch = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100);
                }
                branch = Picture("Film branch stop", stage, new Vector2(x - .06f, .64f), new Vector2(x + .06f, .815f), introBranch, true).rectTransform;
                branch.GetComponent<Image>().material = ToolSilhouette;
                if (lesson == MechanicLesson.Bridge)
                {
                    branch.gameObject.SetActive(false);
                    bridge = Rect("Film one use bridge", upper, new Vector2(.36f, .62f), new Vector2(.64f, 1.2f));
                    bridge.gameObject.AddComponent<WoodBoardGraphic>(); bridge.pivot = new Vector2(0, .5f);
                }
            }
            if (lesson == MechanicLesson.LinkedPairs)
            {
                void LinkedCord(Vector2 a, Vector2 b)
                {
                    a.y += rowOffset; b.y += rowOffset;
                    var segment = Rect("Film linked pair rope", stage, Vector2.zero, Vector2.zero);
                    Background(segment, new Color(.77f, .65f, .39f), false);
                    segment.gameObject.AddComponent<DemoLine>().Initialize(stage, a, b, 3);
                }
                LinkedCord(new Vector2(.12f, .69f), new Vector2(.12f, .64f));
                LinkedCord(new Vector2(.12f, .64f), new Vector2(.38f, .64f));
                LinkedCord(new Vector2(.38f, .64f), new Vector2(.38f, .69f));
                LinkedCord(new Vector2(.62f, .69f), new Vector2(.62f, .64f));
                LinkedCord(new Vector2(.62f, .64f), new Vector2(.88f, .64f));
                LinkedCord(new Vector2(.88f, .64f), new Vector2(.88f, .69f));
                LinkedCord(new Vector2(.38f, .64f), new Vector2(.62f, .64f));
            }
            Image wind = null;
            if (lesson == MechanicLesson.Gust || lesson == MechanicLesson.LiveGust || lesson == MechanicLesson.LandingWind)
            {
                wind = Picture("Film warning windsock", stage, new Vector2(.70f, .38f), new Vector2(.96f, .61f), PlatformProp(0), true);
                wind.material = ToolSilhouette;
            }
            Image bomb = null;
            if (lesson == MechanicLesson.BombHourglass)
            {
                bomb = Picture("Film frost stops bomb", stage, new Vector2(.78f, .43f), new Vector2(.96f, .61f), ToolBody(2), true);
                bomb.material = ToolSilhouette; BuildBombMark(bomb.rectTransform, plan.before.Row[0]);
            }
            RectTransform lens = null;
            if (plan.preview)
            {
                lens = Rect("Film landing preview", stage, new Vector2(.02f, .28f), new Vector2(.98f, .47f));
                lens.gameObject.AddComponent<CanvasGroup>().alpha = .75f;
                for (int i = 0; i < 4; i++)
                {
                    var ghost = FilmPiece(lens, plan.after.Row[i], "Film predicted landing " + i);
                    ghost.anchorMin = new Vector2(i / 4f + .035f, 0); ghost.anchorMax = new Vector2((i + 1) / 4f - .035f, 1);
                }
            }
            var hand = Picture("Film tutorial hand above pieces", stage, new Vector2(.5f, .5f), new Vector2(.5f, .5f), tutorialHand.sprite, true);
            hand.material = tutorialOverlayMaterial; hand.raycastTarget = false; hand.rectTransform.pivot = new Vector2(.64f, .88f);
            hand.rectTransform.sizeDelta = new Vector2(54, 65) * MenuUnit;
            // The original hand's fingertip, not its palm, indicates the action point.
            hand.rectTransform.SetAsLastSibling();
            int lastWood = -1, lastDamage = -1;
            Sprite safeBomb = plan.deviceKind == 2 ? Resources.Load<Sprite>("CascadeArt/Devices/BombSafe") : null;
            var normalProp = propFace != null ? propFace.sprite : null;
            var film = stage.gameObject.AddComponent<LessonFilm>();
            Image[] motes = null;
            if (plan.contact != null)
            {
                int motif = plan.deviceKind == 4 ? 1 : plan.deviceKind == 8 ? 2 : plan.deviceKind == 9 ? 3 : plan.deviceKind == 10 ? 4 : 0;
                var sprite = WorkshopSpriteSet.Tool(Resources.Load<Texture2D>("CascadeArt/Reactions/ToolMotifsClean"), motif);
                film.OwnMotif(sprite); motes = new Image[plan.deviceKind == 8 ? 1 : 4];
                for (int i = 0; i < motes.Length; i++)
                {
                    motes[i] = Picture("Film contact reaction " + i, stage, new Vector2(.5f, .5f), new Vector2(.5f, .5f), sprite, true);
                    motes[i].material = ToolSilhouette; motes[i].raycastTarget = false;
                }
                hand.rectTransform.SetAsLastSibling();
            }
            RectTransform tray = null;
            if (lesson == MechanicLesson.Toolbag)
            {
                tray = Rect("Film tool selection", stage, new Vector2(.17f, .37f), new Vector2(.83f, .61f));
                var shelf = Rect("Film tool tray wood", tray, new Vector2(0, 0), new Vector2(1, .18f)); shelf.gameObject.AddComponent<WoodBoardGraphic>();
                for (int i = 0; i < 3; i++) Picture("Film bag choice " + i, tray, new Vector2(i / 3f + .03f, .18f), new Vector2((i + 1) / 3f - .03f, 1), ToolArt(i), true).material = ToolSilhouette;
                hand.rectTransform.SetAsLastSibling();
            }
            film.Initialize(this, opacity, (time, still) =>
            {
                // The next loop explains the equally valid tap / tap gesture.
                // Selection lifts the original; movement starts only after the second tap.
                bool tapSwap = lesson == MechanicLesson.Swap && film.CycleCount % 2 == 1;
                float exchange = EaseFilm((time - 2.45f) / .95f);
                float result = EaseFilm((time - 3.65f) / 1.05f);
                float action = lesson == MechanicLesson.ClosedPath ? EaseFilm((time - 3.05f) / .95f) : exchange;
                bool after = time >= 4.7f;
                float gust = still ? 0 : Mathf.Sin(Mathf.Clamp01((time - 2.4f) / 2.4f) * Mathf.PI);
                bool windy = lesson == MechanicLesson.Gust || lesson == MechanicLesson.LiveGust || lesson == MechanicLesson.LandingWind;
                float sway = windy && !still ? Mathf.Sin(time * 9) * gust * (lesson == MechanicLesson.LiveGust ? 5 : 2.2f) : 0;
                upper.localRotation = Quaternion.Euler(0, 0, sway);
                lower.localRotation = Quaternion.Euler(0, 0, windy && !still ? Mathf.Sin(time * 7 + 1) * gust * 1.4f : 0);
                for (int i = 0; i < 4; i++)
                {
                    float from = (i + .5f) / 4, via = (exchangeDest[i] + .5f) / 4, to = (finalDest[i] + .5f) / 4;
                    float x = Mathf.Lerp(from, via, action), y = rowY;
                    if (lesson == MechanicLesson.Undo || lesson == MechanicLesson.UndoSpool) x = Mathf.Lerp(from, to, exchange);
                    else if (plan.recovery) { x = from; y = Mathf.Lerp(.38f, rowY, result); }
                    else if (plan.descent) { y = Mathf.Lerp(rowY, .38f, exchange * exchange); x = Mathf.Lerp(from, to, result); }
                    else if (lesson == MechanicLesson.LiveGust) x = Mathf.Lerp(x, to, result);
                    else if (lesson == MechanicLesson.Ribbon) x = Mathf.Lerp(x, to, EaseFilm((time - 4.7f) / .9f));
                    else if (plan.preview || lesson == MechanicLesson.Toolbag || lesson == MechanicLesson.Gust) x = from;
                    if (!still && !plan.descent && !plan.recovery && !plan.preview && lesson != MechanicLesson.Gust && lesson != MechanicLesson.Toolbag)
                        y += Mathf.Sin(action * Mathf.PI) * (via > from ? .095f : -.045f);
                    if (tapSwap && i == plan.command.Edge && time >= 1.45f && time < 2.45f)
                        y += still ? .06f : .06f * EaseFilm((time - 1.45f) / .22f);
                    if (!still && windy) { x += sway * .0022f; y += Mathf.Abs(Mathf.Sin(time * 6 + i)) * gust * .045f; }
                    PoseFilmPiece(pieces[i], stage, x, y, 1 + (still ? 0 : Mathf.Sin(action * Mathf.PI) * .07f));
                    pieces[i].localRotation = Quaternion.Euler(0, 0, still ? 0 : sway + (via != from ? Mathf.Sin(action * Mathf.PI) * (via > from ? -7 : 5) : 0));
                    PoseFilmPiece(glows[i].rectTransform, stage, x, y, 1.20f);
                    bool selected = time > (tapSwap ? 1.45f : 1) && time < 3.5f && (i == plan.command.Edge || (lesson == MechanicLesson.LinkedPairs && i == 2));
                    glowAlpha[i].alpha = selected || after && plan.contact != null && finalDest[i] == plan.contact.Slot ? 1 : 0;
                }
                bool woodDone = time >= (plan.contact != null ? 4.7f : 3.4f);
                int wood = woodDone ? plan.woodAfter : plan.woodBefore;
                if (lesson == MechanicLesson.Ribbon && time >= 3.4f && time < 5.6f) wood = plan.woodBefore - 1;
                if (lesson == MechanicLesson.Repair && time >= 3.4f && time < 4.7f) wood = plan.woodBefore - 1;
                if (wood != lastWood) { count.SetCopy(UiCopy.Raw(wood.ToString())); lastWood = wood; }
                int damage = lesson == MechanicLesson.Repair ? (after ? 0 : 2) : lesson == MechanicLesson.Fragile && after ? 2
                    : lesson == MechanicLesson.LiveGust && after ? 1 : 0;
                if (lesson == MechanicLesson.Fragile && !after) damage = 1;
                if (damage != lastDamage) { upperWood.SetDamage(damage); lastDamage = damage; }
                var counterParent = plan.descent && time >= 3.4f || plan.recovery && time < 4.3f ? lower : upper;
                if (counter.rectTransform.parent != counterParent) counter.rectTransform.SetParent(counterParent, false);
                upper.gameObject.SetActive(!(plan.descent && time >= 3.5f) && !(plan.recovery && time < 4.3f));
                if (prop != null)
                {
                    if (plan.contact != null)
                    {
                        float fromX = (plan.contact.Slot + .5f) / 4;
                        float flight = EaseFilm((time - 3.5f) / .75f);
                        float px = Mathf.Lerp(fromX, .5f, flight), py = Mathf.Lerp(.53f, .48f, flight);
                        prop.anchorMin = new Vector2(px - .105f, py - .10f); prop.anchorMax = new Vector2(px + .105f, py + .10f);
                        prop.localScale = Vector3.one * (1 + (still ? 0 : Mathf.Sin(flight * Mathf.PI) * .35f));
                        prop.localRotation = Quaternion.Euler(0, 0, still ? 0 : Mathf.Sin(time * 2) * 4 * (1 - flight));
                        cordRect.gameObject.SetActive(time < 3.5f);
                        if (time < 3.5f) cord.SetEndpoints(Vector2.Scale(new Vector2(fromX, .68f), stage.rect.size),
                            Vector2.Scale(new Vector2(px, py + .09f), stage.rect.size), .08f, Time.unscaledDeltaTime, time, still);
                        propAlpha.alpha = time >= 6.6f ? 1 - EaseFilm((time - 6.6f) / .65f) : 1;
                        if (plan.deviceKind == 2 && safeBomb != null) propFace.sprite = after ? safeBomb : normalProp;
                        propFace.color = plan.deviceKind == 10 && after ? new Color(.65f, .91f, 1) : Color.white;
                    }
                    else
                    {
                        prop.localScale = Vector3.one * (1 + (still ? 0 : Mathf.Sin(result * Mathf.PI) * .12f));
                        propAlpha.alpha = time < 2.7f ? 1 : .45f;
                    }
                }
                if (bomb != null) bomb.color = after ? new Color(.57f, .90f, 1) : Color.white;
                if (motes != null)
                    for (int i = 0; i < motes.Length; i++)
                    {
                        float bloom = EaseFilm((time - 4.25f) / .9f);
                        var mote = motes[i]; float angle = i * 2.39996f;
                        float radius = still ? .115f : .06f + bloom * .12f;
                        var point = new Vector2(.5f + Mathf.Cos(angle) * radius, .48f + Mathf.Sin(angle) * radius);
                        mote.rectTransform.anchorMin = mote.rectTransform.anchorMax = point;
                        mote.rectTransform.sizeDelta = Vector2.one * Mathf.Min(stage.rect.width * .095f, stage.rect.height * .095f);
                        mote.rectTransform.localRotation = Quaternion.Euler(0, 0, still ? 0 : Mathf.Cos(angle) * bloom * 22);
                        mote.gameObject.SetActive(time >= 4.25f && time <= 6.3f);
                        mote.color = new Color(1, 1, 1, still ? .75f : 1 - EaseFilm((time - 5.45f) / .85f));
                    }
                if (tray != null)
                {
                    tray.gameObject.SetActive(time >= 2.4f); prop.gameObject.SetActive(time < 2.4f);
                }
                if (branch != null)
                {
                    branch.gameObject.SetActive(lesson == MechanicLesson.Bridge ? after : lesson == MechanicLesson.Key ? !after : true);
                    float shake = lesson == MechanicLesson.ClosedPath && !still && time > 1.55f && time < 2.15f ? Mathf.Sin((time - 1.55f) * 23) * 5 : 0;
                    branch.localRotation = Quaternion.Euler(0, 0, shake);
                }
                if (bridge != null) bridge.localRotation = Quaternion.Euler(0, 0, -75 * result);
                if (wind != null) wind.rectTransform.localRotation = Quaternion.Euler(0, 0, still ? 0 : Mathf.Sin(time * 5) * gust * 12);
                if (lens != null) lens.gameObject.SetActive(time >= 3.4f);
                Vector2 handPoint;
                bool showHand = time >= .8f && time <= (lesson == MechanicLesson.Ribbon ? 5.8f : 4.2f) && lesson != MechanicLesson.Gust;
                if (buttonShown && time < 2.4f) handPoint = new Vector2(.5f, .115f);
                else if (lesson == MechanicLesson.ClosedPath && time < 2.25f) handPoint = new Vector2(.125f + .25f * EaseFilm((time - 1.2f) / .45f), rowY);
                else
                {
                    int start = Mathf.Max(0, plan.command.Edge);
                    int end = plan.command.Kind == CommandKind.LongReach ? plan.command.End : start + 1;
                    float fromX = (start + .5f) / 4, toX = (end + .5f) / 4;
                    float move = lesson == MechanicLesson.ClosedPath ? action : exchange;
                    handPoint = plan.descent || plan.recovery || lesson == MechanicLesson.Undo || lesson == MechanicLesson.UndoSpool
                        ? new Vector2(.5f, .115f) : new Vector2(Mathf.Lerp(fromX, toX, move), rowY + (still ? 0 : Mathf.Sin(move * Mathf.PI) * .085f));
                }
                if (lesson == MechanicLesson.Ribbon && time >= 4.5f)
                    handPoint = new Vector2(Mathf.Lerp(.125f, .875f, EaseFilm((time - 4.7f) / .9f)), rowY);
                if (lesson == MechanicLesson.Toolbag && time >= 2.4f) handPoint = new Vector2(.28f, .51f);
                if (tapSwap)
                {
                    float approach = EaseFilm((time - 1.85f) / .42f);
                    handPoint = new Vector2(Mathf.Lerp((plan.command.Edge + .5f) / 4, (plan.command.Edge + 1.5f) / 4, approach), rowY);
                    showHand = time >= .8f && time <= 2.75f;
                }
                hand.gameObject.SetActive(showHand || still && lesson != MechanicLesson.Gust);
                hand.rectTransform.anchorMin = hand.rectTransform.anchorMax = handPoint;
                hand.rectTransform.anchoredPosition = Vector2.zero;
                bool secondTap = tapSwap && time > 2.3f && time < 2.48f;
                hand.rectTransform.localScale = Vector3.one * (still ? 1 : time > 1.25f && time < 1.55f || secondTap ? .93f : 1);
                button.localScale = Vector3.one * (still || !buttonShown ? 1 : time > 1.25f && time < 1.55f ? .96f : 1);
            });
        }

        static float EaseFilm(float progress) => Mathf.SmoothStep(0, 1, Mathf.Clamp01(progress));
        static void PoseFilmPiece(RectTransform piece, RectTransform stage, float x, float y, float scale)
        {
            piece.anchorMin = piece.anchorMax = new Vector2(x, y); piece.anchoredPosition = Vector2.zero;
            float side = Mathf.Min(stage.rect.width * .195f, stage.rect.height * .21f);
            piece.sizeDelta = Vector2.one * side; piece.localScale = Vector3.one * scale;
        }
    }
}
