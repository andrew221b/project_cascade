using System.Collections;
using System.Linq;
using Cascade.Domain;
using Cascade.Presentation;
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
        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game = UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SendMessage("OnApplicationFocus", true); game.SendMessage("OnApplicationPause", false);
            mouse = InputSystem.AddDevice<Mouse>(); touch = InputSystem.AddDevice<Touchscreen>();
            yield return null;
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if (mouse != null) InputSystem.RemoveDevice(mouse); if (touch != null) InputSystem.RemoveDevice(touch);
            yield return null;
        }
        Vector2 Point(int slot)
        {
            var rect = (RectTransform)game.SlotViews[slot].transform;
            return RectTransformUtility.WorldToScreenPoint(Camera.main, rect.TransformPoint(rect.rect.center));
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
        [UnityTest] public IEnumerator EightMixedSymbolsHaveSeparateBoundsAndStableFullTarget()
        {
            Assert.That(game.Catalog.TryGetLocalizedBinding("C32", "en", out var content), Is.True);
            game.LoadLevel(content.Definition, "en"); game.ForcePreciseInput = true; yield return null; yield return null;
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
    }
}
