using System.Collections;
using System.Linq;
using Cascade.Domain;
using Cascade.Presentation;
using Cascade.Solver;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Cascade.Tests
{
    public sealed class PlayableTests
    {
        CascadeGame game;
        Mouse mouse;
        Touchscreen touch;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game = UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            // Virtual input must not depend on which desktop window the test runner focused.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            game.SendMessage("OnApplicationFocus", true); game.SendMessage("OnApplicationPause", false);
            mouse = InputSystem.AddDevice<Mouse>(); touch = InputSystem.AddDevice<Touchscreen>();
            yield return null;
            game.SendMessage("OnApplicationFocus", true); game.SendMessage("OnApplicationPause", false);
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if (mouse != null) InputSystem.RemoveDevice(mouse); if (touch != null) InputSystem.RemoveDevice(touch);
            InputSystem.settings.backgroundBehavior = oldBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorInput;
            yield return null;
        }
        Vector2 Point(int slot)
        {
            var rect = (RectTransform)game.SlotViews[slot].transform;
            return RectTransformUtility.WorldToScreenPoint(Camera.main, rect.TransformPoint(rect.rect.center));
        }
        IEnumerator ClickButton(string name)
        {
            var button = game.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b => b.name == name);
            Assert.That(button.interactable, Is.True, name + " must be available");
            var rect = (RectTransform)button.transform;
            var point = RectTransformUtility.WorldToScreenPoint(Camera.main, rect.TransformPoint(rect.rect.center));
            yield return MouseFrame(point, true); yield return MouseFrame(point, false);
        }
        IEnumerator AwaitPresentation()
        {
            float until = Time.realtimeSinceStartup + 3;
            while (game.Interaction.IsPresenting && Time.realtimeSinceStartup < until) yield return null;
            Assert.That(game.Interaction.IsPresenting, Is.False, "Presentation must release the input gate");
        }
        IEnumerator MouseFrame(Vector2 point, bool held)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left, held));
            yield return null; yield return null;
        }
        IEnumerator TouchFrame(int id, Vector2 point, UnityEngine.InputSystem.TouchPhase phase)
        {
            InputSystem.QueueStateEvent(touch, new TouchState { touchId = id, position = point, phase = phase });
            yield return null; yield return null;
        }
        [UnityTest] public IEnumerator MouseTapTapWinsAndUndoRestores()
        {
            Assert.That(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(game.GetComponentInChildren<Canvas>().GetComponent<UnityEngine.UI.GraphicRaycaster>(), Is.Not.Null);
            yield return MouseFrame(Point(0), true); yield return MouseFrame(Point(0), false);
            yield return MouseFrame(Point(1), true); yield return MouseFrame(Point(1), false);
            Assert.That(game.Interaction.Attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(game.Interaction.Attempt.RouteLength, Is.EqualTo(1));
            yield return new WaitForSecondsRealtime(.20f); game.Undo();
            Assert.That(game.Interaction.Attempt.Current.Status, Is.EqualTo(PuzzleStatus.Playing));
            Assert.That(game.Interaction.Attempt.RouteLength, Is.Zero);
        }
        [UnityTest] public IEnumerator TouchAdjacentDragWinsWithoutSpawningTokens()
        {
            int views = game.SlotViews.Count; yield return TouchFrame(1, Point(0), UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(1, Point(1), UnityEngine.InputSystem.TouchPhase.Moved);
            yield return TouchFrame(1, Point(1), UnityEngine.InputSystem.TouchPhase.Ended);
            Assert.That(game.Interaction.Attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(game.Interaction.Attempt.RouteLength, Is.EqualTo(1)); Assert.That(game.SlotViews.Count, Is.EqualTo(views));
        }
        [UnityTest] public IEnumerator LongTouchDragAndCancelledTouchAreFree()
        {
            game.LoadNode("C02"); yield return null; var before = game.Interaction.Attempt.Current;
            yield return TouchFrame(1, Point(0), UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(1, Point(2), UnityEngine.InputSystem.TouchPhase.Moved);
            yield return TouchFrame(1, Point(2), UnityEngine.InputSystem.TouchPhase.Ended);
            Assert.That(game.Interaction.Attempt.Current, Is.SameAs(before));
            yield return TouchFrame(2, Point(0), UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(2, new Vector2(-100, -100), UnityEngine.InputSystem.TouchPhase.Canceled);
            Assert.That(game.Interaction.Attempt.Current, Is.SameAs(before)); Assert.That(game.Interaction.Attempt.History, Is.Empty);
        }
        [UnityTest] public IEnumerator SecondFingerOnSameSlotCannotStealFirstGesture()
        {
            yield return TouchFrame(1, Point(0), UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(2, Point(0), UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(2, Point(0), UnityEngine.InputSystem.TouchPhase.Ended);
            yield return TouchFrame(1, Point(1), UnityEngine.InputSystem.TouchPhase.Moved);
            yield return TouchFrame(1, Point(1), UnityEngine.InputSystem.TouchPhase.Ended);
            Assert.That(game.Interaction.Attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(game.Interaction.Attempt.RouteLength, Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator SystemCancelledDragOverNeighbourCannotCommit()
        {
            var before = game.Interaction.Attempt.Current;
            yield return TouchFrame(1, Point(0), UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(1, Point(1), UnityEngine.InputSystem.TouchPhase.Moved);
            yield return TouchFrame(1, Point(1), UnityEngine.InputSystem.TouchPhase.Canceled);
            Assert.That(game.Interaction.Attempt.Current, Is.SameAs(before));
            Assert.That(game.Interaction.Attempt.History, Is.Empty);
        }
        [UnityTest] public IEnumerator SecondaryMouseButtonCannotReleasePrimaryGesture()
        {
            var before = game.Interaction.Attempt.Current;
            yield return MouseFrame(Point(0), true);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = Point(0) }
                .WithButton(MouseButton.Left, true).WithButton(MouseButton.Right, true));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = Point(0) }.WithButton(MouseButton.Left, true));
            yield return null; yield return null;
            Assert.That(game.Interaction.SelectedSlot, Is.Null, "Right release must not complete the held left gesture");
            yield return MouseFrame(Point(1), true); yield return MouseFrame(Point(1), false);
            Assert.That(game.Interaction.Attempt.Current, Is.Not.SameAs(before));
            Assert.That(game.Interaction.Attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(game.Interaction.Attempt.RouteLength, Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator NewGestureRecoversAfterFocusCancelledOldCapture()
        {
            yield return TouchFrame(1, Point(0), UnityEngine.InputSystem.TouchPhase.Began);
            game.SendMessage("OnApplicationFocus", false);
            game.SendMessage("OnApplicationFocus", true);
            yield return TouchFrame(2, Point(0), UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(1, Point(0), UnityEngine.InputSystem.TouchPhase.Ended);
            yield return TouchFrame(2, Point(1), UnityEngine.InputSystem.TouchPhase.Moved);
            yield return TouchFrame(2, Point(1), UnityEngine.InputSystem.TouchPhase.Ended);
            Assert.That(game.Interaction.Attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
            Assert.That(game.Interaction.Attempt.RouteLength, Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator EightMixedSymbolsHaveSeparateBoundsAndStableFullTarget()
        {
            Assert.That(game.Catalog.TryGetLocalizedBinding("C32", "en", out var content), Is.True);
            game.LoadLevel(content.Definition, "en"); game.ForcePreciseInput = true; yield return null; yield return null;
            Assert.That(game.PreciseInputActive, Is.True);
            var target = game.TargetRow.GetComponentsInChildren<TextMeshProUGUI>().Select(t => t.text).ToArray();
            Assert.That(target.Length, Is.EqualTo(8)); Assert.That(game.SlotViews.Count, Is.EqualTo(8));
            for (int i = 0; i < 7; i++) {
                var a = (RectTransform)game.SlotViews[i].transform; var b = (RectTransform)game.SlotViews[i + 1].transform;
                var cornersA = new Vector3[4]; var cornersB = new Vector3[4]; a.GetWorldCorners(cornersA); b.GetWorldCorners(cornersB);
                Assert.That(cornersA[2].x, Is.LessThan(cornersB[0].x));
            }
            game.Interaction.FocusSlot(0); var result = game.Interaction.SwapFocused(1);
            if (result != null && result.Accepted) yield return new WaitForSecondsRealtime(.20f);
            Assert.That(game.TargetRow.GetComponentsInChildren<TextMeshProUGUI>().Select(t => t.text), Is.EqualTo(target));
        }
        [UnityTest] public IEnumerator FocusPauseAndDisableCannotRepeatCommittedCommand()
        {
            game.LoadNode("C02"); yield return null; game.Interaction.Execute(PuzzleCommand.Swap(0));
            var committed = game.Interaction.Attempt.Current; int count = game.Interaction.Attempt.RouteLength;
            game.SendMessage("OnApplicationFocus", false); game.SendMessage("OnApplicationPause", false);
            Assert.That(game.Interaction.CanPlay, Is.False);
            game.enabled = false; yield return null; game.enabled = true; game.SendMessage("OnApplicationFocus", true);
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(game.Interaction.Attempt.Current, Is.SameAs(committed)); Assert.That(game.Interaction.Attempt.RouteLength, Is.EqualTo(count));
            Assert.That(game.Interaction.IsPresenting, Is.False);
        }
        [UnityTest] public IEnumerator PrecisionButtonsAndHintRemainUsableTogether()
        {
            game.Catalog.TryGetLocalizedBinding("C32", "en", out var content);
            game.LoadLevel(content.Definition); game.ForcePreciseInput = true; yield return null; yield return null;
            yield return ClickButton("Hint");
            float until = Time.realtimeSinceStartup + 5;
            while (!game.Interaction.Attempt.HintUsed && Time.realtimeSinceStartup < until) yield return null;
            Assert.That(game.Interaction.Attempt.HintUsed, Is.True);
            var buttons = game.GetComponentsInChildren<UnityEngine.UI.Button>();
            var hintButton = buttons.Single(b => b.name == "Hint");
            foreach (string name in new[] { "Swapleft", "Swapright" }) {
                var a = (RectTransform)hintButton.transform; var b = (RectTransform)buttons.Single(v => v.name == name).transform;
                var ac = new Vector3[4]; var bc = new Vector3[4]; a.GetWorldCorners(ac); b.GetWorldCorners(bc);
                Assert.That(ac[2].y, Is.LessThan(bc[0].y), "Hint cannot cover precise input");
            }
            yield return ClickButton("Hint"); yield return AwaitPresentation();
            Assert.That(game.Interaction.Attempt.RouteLength, Is.EqualTo(1));
            yield return ClickButton("Undo"); Assert.That(game.Interaction.Attempt.RouteLength, Is.Zero);
            Assert.That(game.Interaction.Attempt.HintUsed, Is.True);
            game.Interaction.FocusSlot(0); game.RefreshLayout(); yield return ClickButton("Swapright"); yield return AwaitPresentation();
            Assert.That(game.Interaction.Attempt.RouteLength, Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator UsedBridgeIsShownClosedAndUndoRestoresIt()
        {
            game.LoadNode("C40"); yield return null; yield return null;
            yield return MouseFrame(Point(1), true); yield return MouseFrame(Point(1), false);
            yield return MouseFrame(Point(2), true); yield return MouseFrame(Point(2), false); yield return AwaitPresentation();
            Assert.That(game.Interaction.Attempt.Current.BridgeUsed, Is.True);
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t => t.name == "Devices" && t.text.Contains("Locked 2")), Is.True);
            yield return ClickButton("Undo");
            Assert.That(game.Interaction.Attempt.Current.BridgeUsed, Is.False);
            Assert.That(game.Interaction.Attempt.Resolver.IsEdgeOpen(game.Interaction.Attempt.Current, 1), Is.True);
        }
        [UnityTest] public IEnumerator ToyPresentationFreezesAmbientAndNeverChangesPuzzleState()
        {
            game.LoadNode("C02"); yield return null; game.RefreshLayout();
            var hash = game.Interaction.Attempt.CurrentStateHash;
            Assert.That(game.Machine.TokenCount, Is.EqualTo(game.SlotViews.Count));
            Assert.That(game.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
            Assert.That(game.GetComponentsInChildren<Collider>(true), Is.Empty);
            float initial = game.Machine.AmbientPhase;
            yield return new WaitForSecondsRealtime(.08f);
            Assert.That(game.Machine.AmbientPhase, Is.GreaterThan(initial));
            game.ReducedMotion = true; float stopped = game.Machine.AmbientPhase;
            yield return new WaitForSecondsRealtime(.08f);
            Assert.That(game.Machine.AmbientPhase, Is.EqualTo(stopped));
            game.ReducedMotion = false; game.Interaction.SetPaused(true);
            yield return new WaitForSecondsRealtime(.08f);
            Assert.That(game.Machine.AmbientPhase, Is.EqualTo(stopped));
            Assert.That(game.Interaction.Attempt.CurrentStateHash, Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator TargetMagnifierKeepsShapesAndGlyphsVisibleAndBlocksInput()
        {
            game.LoadNode("C32"); yield return null; yield return ClickButton("Receiver"); yield return null;
            Assert.That(game.Interaction.IsPaused, Is.True);
            var faces = game.GetComponentsInChildren<ShapeFaceGraphic>(); Assert.That(faces.Length, Is.EqualTo(2));
            foreach (var face in faces) { Assert.That(face.canvasRenderer, Is.Not.Null); Assert.That(face.rectTransform.rect.width, Is.GreaterThan(20)); }
            var hash = game.Interaction.Attempt.CurrentStateHash;
            Assert.That(game.Interaction.Execute(PuzzleCommand.Drop), Is.Null);
            Assert.That(game.Interaction.Attempt.CurrentStateHash, Is.EqualTo(hash));
            yield return ClickButton("Closetarget"); Assert.That(game.Interaction.CanPlay, Is.True);
        }
        [UnityTest] public IEnumerator AllFortyLevelsCanBeDeliveredThroughActualMouseAndButtons()
        {
            var events = new HashSet<PuzzleEventKind>(); int commands = 0;
            foreach (var content in game.Catalog.PrimaryLevels) {
                game.LoadNode(content.Definition.CampaignNodeId); yield return null; yield return null;
                var attempt = game.Interaction.Attempt; var views = game.SlotViews.ToArray();
                game.Interaction.Committed += result => { foreach (var e in result.Events) events.Add(e.Kind); };
                var solution = PuzzleSolver.FindShortest(attempt.Resolver);
                Assert.That(solution.Status, Is.EqualTo(SearchStatus.Solved), content.Definition.PuzzleId);
                foreach (var command in solution.Path) {
                    var predicted = attempt.Predict(command).State; int before = attempt.RouteLength;
                    if (command.Kind == CommandKind.EarlyDrop) yield return ClickButton("Drop");
                    else {
                        yield return MouseFrame(Point(command.Edge), true); yield return MouseFrame(Point(command.Edge), false);
                        yield return MouseFrame(Point(command.Edge + 1), true); yield return MouseFrame(Point(command.Edge + 1), false);
                    }
                    Assert.That(attempt.RouteLength, Is.EqualTo(before + 1), content.Definition.PuzzleId + " " + command.Kind);
                    Assert.That(attempt.Current, Is.EqualTo(predicted), content.Definition.PuzzleId);
                    Assert.That(game.SlotViews, Is.EqualTo(views), "The same row views must survive every floor");
                    yield return AwaitPresentation(); commands++;
                }
                Assert.That(attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won), content.Definition.PuzzleId);
                Assert.That(attempt.Current.Row, Is.EqualTo(attempt.Level.TargetSymbolIds));
            }
            foreach (var required in new[] { PuzzleEventKind.BombDefused, PuzzleEventKind.BombExploded, PuzzleEventKind.KeyOpened,
                PuzzleEventKind.Repaired, PuzzleEventKind.BridgeConsumed, PuzzleEventKind.StampCollected, PuzzleEventKind.EntryPermuted })
                Assert.That(events.Contains(required), Is.True, "Playable path coverage: " + required);
            Debug.Log("CASCADE actual UI acceptance: 40 levels, " + commands + " commands; events: " + string.Join(", ", events));
        }
    }
}
