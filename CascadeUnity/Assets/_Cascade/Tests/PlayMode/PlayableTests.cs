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
using System.IO;
using Cascade.Persistence;

namespace Cascade.Tests
{
    public sealed class PlayableTests
    {
        CascadeGame game;
        Mouse mouse;
        Touchscreen touch;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        string isolatedProfile;
        bool oldRunInBackground;
        [UnitySetUp] public IEnumerator SetUp()
        {
            oldRunInBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            isolatedProfile=Path.Combine(Path.GetTempPath(),"CascadePlayTests",System.Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=isolatedProfile;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game = UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.ShowGame();
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            // Virtual input must not depend on which desktop window the test runner focused.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            game.SendMessage("OnApplicationFocus", true); game.SendMessage("OnApplicationPause", false);
            game.Feedback.MusicEnabled = false; game.Feedback.SoundEnabled = false;
            mouse = InputSystem.AddDevice<Mouse>(); touch = InputSystem.AddDevice<Touchscreen>();
            yield return null;
            game.SendMessage("OnApplicationFocus", true); game.SendMessage("OnApplicationPause", false);
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if (mouse != null) InputSystem.RemoveDevice(mouse); if (touch != null) InputSystem.RemoveDevice(touch);
            InputSystem.settings.backgroundBehavior = oldBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorInput;
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            CascadeGame.EditorProfileDirectory=null;
            UnityEngine.Application.runInBackground=oldRunInBackground;
            if(Directory.Exists(isolatedProfile))Directory.Delete(isolatedProfile,true);
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
        // Functional stage/ordering checks, not a frame-rate benchmark. A locked macOS Editor
        // can render at 1 Hz; keep a 30-second real-time hang watchdog.
        static bool PresentationBudget(double started)=>Time.realtimeSinceStartupAsDouble-started<30;
        IEnumerator AwaitPresentation()
        {
            double started=Time.realtimeSinceStartupAsDouble;
            while (game.Interaction.IsPresenting && PresentationBudget(started)) yield return null;
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
            yield return AwaitPresentation(); game.Undo();
            Assert.That(game.Interaction.Attempt.Current.Status, Is.EqualTo(PuzzleStatus.Playing));
            Assert.That(game.Interaction.Attempt.RouteLength, Is.Zero);
        }
        [UnityTest] public IEnumerator WordDeviceSocketsShowLiteralLettersInEveryUiLocale()
        {
            foreach(var locale in new[]{"en","ru","uk"}) {
                game.SetUiLocale(locale);Assert.That(game.Catalog.TryGetLocalizedBinding("C38",locale,out var binding),Is.True);
                game.LoadLevel(binding.Definition,locale);game.ShowGame();yield return null;
                var letters=game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name=="Required letter").ToArray();
                Assert.That(letters.Length,Is.EqualTo(2));
                foreach(var letter in letters){Assert.That(letter.text,Is.EqualTo(binding.Definition.Floors[0].Key.RequiredSymbol.Value));Assert.That(letter.isTextOverflowing,Is.False);}
            }
        }
        [UnityTest] public IEnumerator PickupDragThenFarSwapWinsAndUndoRestoresTool()
        {
            game.LoadLevel(game.Catalog.BonusLevels[6].Definition);game.ShowGame();yield return null;game.RefreshLayout();
            yield return TouchFrame(21,Point(0),UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(21,Point(1),UnityEngine.InputSystem.TouchPhase.Moved);
            Assert.That(game.Interaction.Attempt.Current.ReachCharges,Is.Zero,"Preview cannot grant a tool");
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.name=="Device state"&&t.text=="Far swap"),Is.True);
            yield return TouchFrame(21,Point(1),UnityEngine.InputSystem.TouchPhase.Ended);yield return AwaitPresentation();
            Assert.That(game.Interaction.Attempt.Current.ReachCharges,Is.EqualTo(1));
            yield return TouchFrame(22,Point(1),UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(22,Point(3),UnityEngine.InputSystem.TouchPhase.Moved);
            yield return TouchFrame(22,Point(3),UnityEngine.InputSystem.TouchPhase.Ended);yield return AwaitPresentation();
            Assert.That(game.Interaction.Attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
            game.Undo();Assert.That(game.Interaction.Attempt.Current.ReachCharges,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator DeviceTapExplainsWithoutMovingAndClockSurvivesReload()
        {
            game.LoadLevel(game.Catalog.BonusLevels[9].Definition);game.ShowGame();yield return null;game.RefreshLayout();
            string before=game.Interaction.Attempt.CurrentStateHash;
            yield return ClickButton("DeviceHourglass");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Devices));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(before));
            yield return ClickButton("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));
            yield return TouchFrame(23,Point(0),UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(23,Point(1),UnityEngine.InputSystem.TouchPhase.Moved);
            yield return TouchFrame(23,Point(1),UnityEngine.InputSystem.TouchPhase.Ended);yield return AwaitPresentation();
            Assert.That(game.Interaction.Attempt.Current.FrozenTicks,Is.EqualTo(1));Assert.That(game.VisibleFrame.FrozenTicks,Is.EqualTo(1));
            string saved=game.Interaction.Attempt.CurrentStateHash;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(saved));
            Assert.That(game.Interaction.Attempt.Current.FrozenTicks,Is.EqualTo(1));game.ShowGame();game.SendMessage("OnApplicationFocus",true);game.Undo();
            Assert.That(game.Interaction.Attempt.Current.PowerupUsed,Is.False);Assert.That(game.Interaction.Attempt.Current.FrozenTicks,Is.Zero);
        }
        [UnityTest] public IEnumerator RibbonDragPersistsAndUndoRestoresCharge()
        {
            game.LoadLevel(game.Catalog.BonusLevels[2].Definition);game.ShowGame();yield return null;game.RefreshLayout();
            yield return TouchFrame(11,Point(0),UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(11,Point(3),UnityEngine.InputSystem.TouchPhase.Moved);
            Assert.That(game.Interaction.Attempt.Current.ReachCharges,Is.EqualTo(1));
            yield return TouchFrame(11,Point(3),UnityEngine.InputSystem.TouchPhase.Ended);yield return AwaitPresentation();
            Assert.That(game.Interaction.Attempt.Current.ReachCharges,Is.Zero);
            string hash=game.Interaction.Attempt.CurrentStateHash;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            game.ShowGame();game.SendMessage("OnApplicationFocus",true);game.Undo();
            Assert.That(game.Interaction.Attempt.Current.ReachCharges,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator ConnectedRopesSurviveReducedMotionAndResize()
        {
            game.LoadNode("C03");game.ReducedMotion=true;yield return null;game.RefreshLayout();
            var ropes=game.Machine.GetComponentsInChildren<LineRenderer>().Where(l=>l.name.Contains("continuous hemp")).ToArray();
            Assert.That(ropes.Length,Is.EqualTo(2));string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(var rope in ropes){Assert.That(rope.enabled,Is.True);Assert.That(rope.positionCount,Is.GreaterThan(25));Assert.That(rope.bounds.size.y,Is.GreaterThan(1));}
            game.SetEditorSafeInsets(new Vector4(.06f,.045f,.06f,.07f));yield return null;
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            foreach(var rope in ropes)for(int i=0;i<rope.positionCount;i++)Assert.That(float.IsNaN(rope.GetPosition(i).x),Is.False);
            game.SetEditorSafeInsets(Vector4.zero);
        }
        [UnityTest] public IEnumerator EffectsOffRemovesWeatherAndPresentationSyncPreservesBoardWeight()
        {
            game.LoadNode("C03");game.ShowGame();yield return new WaitForSecondsRealtime(.6f);
            string hash=game.Interaction.Attempt.CurrentStateHash;float sag=game.Machine.LoadedSag;
            Assert.That(sag,Is.GreaterThan(.015f));game.RefreshLayout();
            Assert.That(game.Machine.LoadedSag,Is.EqualTo(sag).Within(.001f),"Layout refresh must not erase the spring's stored weight.");
            game.Preferences.EffectQuality=0;game.ShowSettings();yield return null;game.ShowGame();
            game.Machine.LivingWorld.EffectQuality=0;yield return null;
            Assert.That(game.Machine.LivingWorld.ActiveWeatherCount,Is.Zero);
            game.ShowTravelBag();yield return null;yield return null;Canvas.ForceUpdateCanvases();
            foreach(var weather in game.GetComponentsInChildren<WorldWeatherGraphic>())
                Assert.That(weather.canvasRenderer.GetMesh().vertexCount,Is.Zero);
            game.Preferences.EffectQuality=2;game.Machine.LivingWorld.EffectQuality=2;game.ShowGame();yield return null;
            Assert.That(game.Machine.LivingWorld.ActiveWeatherCount,Is.GreaterThan(0));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
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
            if (result != null && result.Accepted) yield return AwaitPresentation();
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
            Assert.That(game.Machine.GetComponentsInChildren<MeshRenderer>().Any(t => t.name == "Edge latch 1" && t.enabled && t.gameObject.activeInHierarchy), Is.True);
            yield return ClickButton("Undo");
            Assert.That(game.Interaction.Attempt.Current.BridgeUsed, Is.False);
            Assert.That(game.Interaction.Attempt.Resolver.IsEdgeOpen(game.Interaction.Attempt.Current, 1), Is.True);
        }
        [UnityTest] public IEnumerator DeviceGuidesAndWeatherHaveNativeRenderersWithoutChangingTheAttempt()
        {
            game.LoadNode("C13");yield return null;string hash=game.Interaction.Attempt.CurrentStateHash;
            game.ReducedMotion=true;game.ShowDevices();
            for(int i=0;i<9;i++){
                yield return null;yield return null;Canvas.ForceUpdateCanvases();
                foreach(var graphic in game.GetComponentsInChildren<WoodBoardGraphic>()){
                    Assert.That(graphic.canvasRenderer,Is.Not.Null);Assert.That(graphic.canvasRenderer.GetMesh().vertexCount,Is.EqualTo(4));
                    Assert.That(graphic.mainTexture.name,Is.EqualTo("WoodBoard"));
                }
                Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));yield return ClickButton("Next");
            }
            game.ShowJourney();yield return ClickButton("Later");yield return ClickButton("Later");yield return ClickButton("Later");
            yield return null;Canvas.ForceUpdateCanvases();var weather=game.GetComponentsInChildren<WorldWeatherGraphic>();Assert.That(weather.Length,Is.EqualTo(1));
            Assert.That(weather[0].canvasRenderer,Is.Not.Null);Assert.That(weather[0].canvasRenderer.GetMesh().vertexCount,Is.GreaterThan(0));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator WoodWeightAndGlyphDepthRemainPresentationOnly()
        {
            game.LoadNode("C37");var words=game.Catalog.LocalizedLevels.First(c=>c.Definition.CampaignNodeId=="C37"&&c.Locale=="ru");game.LoadLevel(words.Definition,"ru");
            yield return null;game.RefreshLayout();yield return new WaitForSecondsRealtime(.7f);
            string hash=game.Interaction.Attempt.CurrentStateHash;float loaded=game.Machine.LoadedSag;
            Assert.That(loaded,Is.GreaterThan(.015f));
            var label=game.SlotViews[0].GetComponentInChildren<TextMeshProUGUI>();
            Assert.That(label.text,Is.Not.Empty);Assert.That(label.transform.position.z,Is.LessThan(game.Machine.TokenVisualPosition(0).z));
            game.Machine.SetHeldWeight(0);game.Machine.SetPiecePose(0,PiecePose.Held);
            yield return new WaitForSecondsRealtime(.7f);Assert.That(game.Machine.LoadedSag,Is.LessThan(loaded-.002f));
            game.Interaction.SetPaused(true);float paused=game.Machine.LoadedSag;
            yield return new WaitForSecondsRealtime(.1f);Assert.That(game.Machine.LoadedSag,Is.EqualTo(paused));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
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
        sealed class AnimationWitness
        {
            public LevelDefinition level; public PuzzleCommand[] prefix; public PuzzleCommand command; public int eventIndex; public PuzzleEventKind kind;
        }
        [UnityTest] public IEnumerator EveryAnimatedStageCanBeInterruptedWithoutReapplyingRules()
        {
            var witnesses = new Dictionary<PuzzleEventKind, AnimationWitness>();
            foreach (var content in game.Catalog.PrimaryLevels.Take(40)) {
                var resolver = new PuzzleResolver(content.Definition); var state = resolver.CreateInitialState(); var prefix = new List<PuzzleCommand>();
                foreach (var command in PuzzleSolver.FindShortest(resolver).Path) {
                    var result = resolver.Resolve(state, command);
                    for (int i = 0; i < result.Events.Count; i++) {
                        var e = result.Events[i];
                        if (e.Kind == PuzzleEventKind.CommandAccepted || e.Kind == PuzzleEventKind.BombAbandoned || (e.Kind == PuzzleEventKind.DurabilityResolved && result.DurabilityCost <= 1)) continue;
                        if (!witnesses.ContainsKey(e.Kind)) witnesses.Add(e.Kind, new AnimationWitness { level = content.Definition, prefix = prefix.ToArray(), command = command, eventIndex = i, kind = e.Kind });
                    }
                    state = result.State; prefix.Add(command);
                }
            }
            Assert.That(witnesses.Count, Is.EqualTo(13), "Every timed event kind needs an interruption witness");
            foreach (var witness in witnesses.Values) for (int mode = 0; mode < 4; mode++) {
                game.LoadLevel(witness.level); yield return null;
                foreach (var command in witness.prefix) { game.Interaction.Execute(command); game.EventPlayer.Skip(); }
                var views = game.SlotViews.ToArray(); int completions = game.EventPlayer.CompletedPresentations;
                game.Interaction.Execute(witness.command); var committed = game.Interaction.Attempt.Current;
                string hash = game.Interaction.Attempt.CurrentStateHash; int route = game.Interaction.Attempt.RouteLength;
                double started=Time.realtimeSinceStartupAsDouble;
                while (game.EventPlayer.IsRunning && game.EventPlayer.EventIndex != witness.eventIndex && PresentationBudget(started)) yield return null;
                Assert.That(game.EventPlayer.IsRunning && game.EventPlayer.EventIndex == witness.eventIndex, Is.True, "Missed " + witness.kind+" on "+witness.level.CampaignNodeId+" mode "+mode+" current "+game.EventPlayer.CurrentEvent+" index "+game.EventPlayer.EventIndex+" running "+game.EventPlayer.IsRunning+" delta "+Time.unscaledDeltaTime);
                if (mode == 0) game.EventPlayer.Skip();
                else if (mode == 1) game.SendMessage("OnApplicationFocus", false);
                else if (mode == 2) game.enabled = false;
                else game.SendMessage("TogglePause");
                yield return null;
                Assert.That(game.Interaction.Attempt.Current, Is.SameAs(committed), witness.kind.ToString());
                Assert.That(game.Interaction.Attempt.CurrentStateHash, Is.EqualTo(hash)); Assert.That(game.Interaction.Attempt.RouteLength, Is.EqualTo(route));
                Assert.That(game.Interaction.IsPresenting, Is.False); Assert.That(game.EventPlayer.CompletedPresentations, Is.EqualTo(completions + 1));
                Assert.That(game.VisibleFrame.Row, Is.EqualTo(committed.Row)); Assert.That(game.VisibleFrame.Durability, Is.EqualTo(committed.Durability));
                Assert.That(game.VisibleFrame.ActiveFloor, Is.EqualTo(committed.ActiveFloor)); Assert.That(game.SlotViews, Is.EqualTo(views));
                if (mode == 1) game.SendMessage("OnApplicationFocus", true);
                else if (mode == 2) game.enabled = true;
                else if (mode == 3) game.SendMessage("TogglePause");
            }
            Debug.Log("CASCADE P13 interruption matrix: 13 stages × 4 interruption paths verified.");
        }
        [UnityTest] public IEnumerator DestroyingEventPlayerSynchronizesTheCommittedResult()
        {
            game.LoadNode("C01"); yield return null; game.Interaction.Execute(PuzzleCommand.Swap(0));
            var committed = game.Interaction.Attempt.Current; string hash = game.Interaction.Attempt.CurrentStateHash;
            UnityEngine.Object.Destroy(game.EventPlayer); yield return null;
            Assert.That(game.Interaction.Attempt.Current, Is.SameAs(committed)); Assert.That(game.Interaction.Attempt.CurrentStateHash, Is.EqualTo(hash));
            Assert.That(game.VisibleFrame.Row, Is.EqualTo(committed.Row)); Assert.That(game.Interaction.IsPresenting, Is.False);
        }
        [UnityTest] public IEnumerator DustTailsDoNotKeepUndoLockedAndFeedbackStaysBounded()
        {
            var library = Resources.Load<ToyAudioLibrary>("CascadeArt/ToyAudio");
            Assert.That(library.cues.Length, Is.EqualTo(12)); Assert.That(library.cues.All(c => c != null && c.samples > 0), Is.True);
            Assert.That(library.alternates.Count(c => c != null), Is.EqualTo(4)); Assert.That(library.music.length, Is.EqualTo(24).Within(.02f));
            int objects = game.Feedback.GetComponentsInChildren<Transform>(true).Length;
            game.LoadNode("C01"); yield return null; game.Interaction.Execute(PuzzleCommand.Swap(0)); yield return AwaitPresentation();
            Assert.That(game.Interaction.CanControl, Is.True); Assert.That(game.Feedback.ActiveParticleCount, Is.GreaterThan(0));
            game.Undo(); Assert.That(game.Interaction.Attempt.RouteLength, Is.Zero);
            for (int i = 0; i < 30; i++) game.Feedback.Burst(Vector3.zero, Color.yellow, 50, false);
            yield return null; Assert.That(game.Feedback.ActiveParticleCount, Is.LessThanOrEqualTo(32));
            Assert.That(game.Feedback.GetComponentsInChildren<Transform>(true).Length, Is.EqualTo(objects));
        }
        [UnityTest] public IEnumerator SelectedLetterTracksItsBodyThroughoutSelectionMotion()
        {
            game.LoadNode("C32"); yield return null; game.Interaction.FocusSlot(0); game.RefreshLayout();
            for (int i = 0; i < 10; i++) {
                yield return null; var face = game.SlotViews[0].GetComponentInChildren<TextMeshProUGUI>().rectTransform;
                Assert.That(face.position.y, Is.EqualTo(game.Machine.PresentedTokenPosition(0).y).Within(.01f));
            }
        }
        [UnityTest] public IEnumerator ReducedMotionKeepsPosesStillThroughoutCommittedEvents()
        {
            game.LoadNode("C37"); game.ReducedMotion = true; yield return null;
            var solution = PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver);
            foreach (var command in solution.Path) {
                game.Interaction.Execute(command); string hash = game.Interaction.Attempt.CurrentStateHash;
                float until = Time.realtimeSinceStartup + 3;
                do {
                    yield return null;
                    Assert.That(game.Interaction.Attempt.CurrentStateHash, Is.EqualTo(hash));
                    for (int i = 0; i < game.SlotViews.Count; i++)
                        Assert.That(Vector3.Distance(game.Machine.PresentedTokenPosition(i), game.Machine.TokenPosition(i)), Is.LessThan(.001f));
                } while (game.Interaction.IsPresenting && Time.realtimeSinceStartup < until);
                Assert.That(game.Interaction.IsPresenting, Is.False);
            }
            Assert.That(game.Interaction.Attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
        }
        [UnityTest] public IEnumerator SceneReloadDuringAnimationReleasesOldAttemptWithoutExceptions()
        {
            game.Interaction.Execute(PuzzleCommand.Swap(0)); var old = game.Interaction;
            string hash = old.Attempt.CurrentStateHash;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game = UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Assert.That(old.IsPresenting, Is.False); Assert.That(old.Attempt.CurrentStateHash, Is.EqualTo(hash));
            Assert.That(game.Interaction.Attempt.RouteLength, Is.EqualTo(1));
            Assert.That(game.Interaction.Attempt.CurrentStateHash, Is.EqualTo(hash));
            Assert.That(game.JourneyClearCount, Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator DeliveryKeepsTargetVisibleAndWaitsBeforeShowingResultControls()
        {
            game.LoadNode("C32"); yield return null;
            foreach (var command in PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path) {
                game.Interaction.Execute(command);
                double started=Time.realtimeSinceStartupAsDouble;
                while (game.EventPlayer.IsRunning && PresentationBudget(started)) {
                    Assert.That(game.TargetRow.GetComponentsInChildren<TextMeshProUGUI>().All(t => t.color.a > .99f), Is.True);
                    Assert.That(game.GetComponentsInChildren<MeshRenderer>(true).Where(r => r.name.StartsWith("Target ")).All(r => r.gameObject.activeInHierarchy), Is.True);
                    Assert.That(game.GetComponentsInChildren<UnityEngine.UI.Button>().Any(b => b.name == "Retry"), Is.False);
                    yield return null;
                }
                Assert.That(game.EventPlayer.IsRunning, Is.False,"Delivery timed out at "+game.EventPlayer.CurrentEvent+" index "+game.EventPlayer.EventIndex+" delta "+Time.unscaledDeltaTime);
            }
            Assert.That(game.GetComponentsInChildren<UnityEngine.UI.Button>().Any(b => b.name == "Retry" && b.interactable), Is.True);
        }
        [UnityTest] public IEnumerator HeldPieceAndLetterFollowPointerWithoutCommittingUntilRelease()
        {
            game.LoadNode("C32"); yield return null;
            string hash=game.Interaction.Attempt.CurrentStateHash;
            Vector3 start=game.Machine.PresentedTokenPosition(0);
            yield return MouseFrame(Point(0),true);
            Assert.That(game.IsHoldingToken,Is.True);
            Assert.That(game.Machine.PresentedTokenPosition(0).y,Is.GreaterThan(start.y));
            yield return MouseFrame(Point(1),true);
            Assert.That(game.Machine.PresentedTokenPosition(0).x,Is.GreaterThan(start.x));
            var face=game.SlotViews[0].GetComponentInChildren<TextMeshProUGUI>().rectTransform.position;
            Assert.That(Mathf.Abs(face.x-game.Machine.PresentedTokenPosition(0).x),Is.LessThan(.0001f));
            Assert.That(Mathf.Abs(face.y-game.Machine.PresentedTokenPosition(0).y),Is.LessThan(.0001f));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            yield return MouseFrame(Point(1),false);
            Assert.That(game.IsHoldingToken,Is.False);Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator DefaultHudHasOneMenuAndNoForcedSlotSelection()
        {
            game.LoadNode("C32");yield return null;
            Assert.That(game.PreciseInputActive,Is.False);
            var names=game.GetComponentsInChildren<UnityEngine.UI.Button>().Select(b=>b.name).ToArray();
            foreach(var hidden in new[]{"Home","Settings","Devices","Swappleft","Swapright","Focusprevious","Focusnext","Retry"})
                Assert.That(names,Does.Not.Contain(hidden));
            foreach(var visible in new[]{"Pause","Undo","Hint","Drop","Receiver"})Assert.That(names,Does.Contain(visible));
            yield return ClickButton("Pause");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Pause));
            yield return ClickButton("Settings");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Settings));
            yield return ClickButton("Back");yield return ClickButton("Resume");Assert.That(game.Interaction.CanPlay,Is.True);
        }
        [UnityTest] public IEnumerator JourneyDialogueBlocksPuzzleThenClearOpensNextStop()
        {
            game.ShowJourney();yield return null;var hash=game.Interaction.Attempt.CurrentStateHash;
            yield return ClickButton("Travel");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Dialogue));
            Assert.That(game.Interaction.CanPlay,Is.False);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            yield return ClickButton("Continue");yield return ClickButton("Continue");
            Assert.That(game.IsJourneyAttempt,Is.True);Assert.That(game.Interaction.CanPlay,Is.True);
            yield return MouseFrame(Point(0),true);yield return MouseFrame(Point(1),true);yield return MouseFrame(Point(1),false);
            yield return AwaitPresentation();Assert.That(game.JourneyClearCount,Is.EqualTo(1));
            game.ShowJourney();yield return null;
            var stops=game.GetComponentsInChildren<UnityEngine.UI.Button>();
            Assert.That(stops.Single(b=>b.name=="Stop2").interactable,Is.True);
            Assert.That(stops.Single(b=>b.name=="Stop3").interactable,Is.False);
        }
        [UnityTest] public IEnumerator LayeredCloudsMoveWithoutStateChangesAndFreezeWhenPaused()
        {
            game.LoadNode("C02");yield return null;var world=game.Machine.LivingWorld;
            Assert.That(world.Ready,Is.True);var hash=game.Interaction.Attempt.CurrentStateHash;
            var cloud=world.CloudPosition;var glider=world.GliderPosition;
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(world.CloudPosition,Is.Not.EqualTo(cloud));Assert.That(world.GliderPosition,Is.Not.EqualTo(glider));
            game.Interaction.SetPaused(true);cloud=world.CloudPosition;glider=world.GliderPosition;
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(world.CloudPosition,Is.EqualTo(cloud));Assert.That(world.GliderPosition,Is.EqualTo(glider));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator UiLanguageChangesNeverTranslateOrRestartPuzzleSymbols()
        {
            Assert.That(game.UiLanguage,Is.EqualTo("en"));Assert.That(game.Preferences.PuzzleLocale,Is.EqualTo("en"));
            game.Catalog.TryGetLocalizedBinding("C32","ru",out var binding);game.LoadLevel(binding.Definition,"ru");yield return null;
            var hash=game.Interaction.Attempt.CurrentStateHash;var state=game.Interaction.Attempt.Current;
            foreach(string locale in new[]{"ru","uk","en"}) {
                game.SetUiLocale(locale);yield return null;yield return null;yield return null;
                Assert.That(game.UiLanguage,Is.EqualTo(locale));Assert.That(game.Interaction.Attempt.ActiveLocale,Is.EqualTo("ru"));
                Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.Current,Is.SameAs(state));
                Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.name=="ReceiverLabel"&&t.text==(locale=="ru"?"Собери этот ряд":locale=="uk"?"Склади цей ряд":"Make this row")),Is.True);
                foreach(var label in game.GetComponentsInChildren<TextMeshProUGUI>())Assert.That(label.text,Does.Not.Contain("No translation"));
                game.ShowJourney();yield return null;yield return null;
                var menu=game.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Settings").GetComponentInChildren<TextMeshProUGUI>();
                Assert.That(menu.text,Is.EqualTo(locale=="en"?"Menu":"Меню"));game.ShowGame();
            }
        }
        [UnityTest] public IEnumerator WordLanguageNeedsConfirmationAndAppliesOnlyAtNextStart()
        {
            var before=game.Interaction.Attempt.Current;game.ShowSettings();yield return ClickButton("Comfort");yield return ClickButton("Languages");
            yield return ClickButton("PuzzleLocaleuk");Assert.That(game.ProposedPuzzleLocale,Is.EqualTo("uk"));
            yield return ClickButton("Keepmychoice");Assert.That(game.Preferences.PuzzleLocale,Is.EqualTo("en"));
            yield return ClickButton("PuzzleLocaleru");yield return ClickButton("Usefornewpuzzles");
            Assert.That(game.Preferences.PuzzleLocale,Is.EqualTo("ru"));Assert.That(game.Interaction.Attempt.Current,Is.SameAs(before));
            game.SelectMode(PuzzleMode.Words);game.ShowJourney();yield return ClickButton("Travel");yield return ClickButton("Skipstory");
            Assert.That(game.Interaction.Attempt.ActiveLocale,Is.EqualTo("ru"));Assert.That(game.Interaction.Attempt.Current.Row.All(v=>v.Locale=="ru"),Is.True);
        }
        [UnityTest] public IEnumerator JourneyMapContinuePreservesAnUnfinishedRoute()
        {
            game.ShowJourney();yield return ClickButton("Travel");yield return ClickButton("Skipstory");
            for(int level=0;level<2;level++) {
                foreach(var command in PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path) {
                    game.Interaction.Execute(command);yield return AwaitPresentation();
                }
                yield return ClickButton("Nextdelivery");
            }
            Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo("C03"));
            game.Interaction.Execute(PuzzleCommand.Swap(0));yield return AwaitPresentation();
            var before=game.Interaction.Attempt.Current;var route=game.Interaction.Attempt.RouteLength;
            game.ShowJourney();yield return ClickButton("Travel");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));Assert.That(game.Interaction.Attempt.Current,Is.SameAs(before));
            Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(route));
        }
        [UnityTest] public IEnumerator FutureMapIsAPreviewAndReturnRestoresTheActualWorld()
        {
            game.LoadNode("C22");var hash=game.Interaction.Attempt.CurrentStateHash;
            game.ShowJourney();yield return ClickButton("Later");yield return ClickButton("Later");yield return ClickButton("Later");
            Assert.That(game.Machine.LivingWorld.Zone,Is.EqualTo(1));
            Assert.That(game.GetComponentsInChildren<Transform>().Any(t=>t.name=="Mika on the path"),Is.False);
            var travel=game.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Travel");
            Assert.That(travel.GetComponentInChildren<TextMeshProUGUI>().text,Is.EqualTo("Back to my path"));
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.text=="Stops to this island: 15"),Is.True);
            yield return ClickButton("Travel");
            Assert.That(game.GetComponentsInChildren<Transform>().Any(t=>t.name=="Mika on the path"),Is.True);
            Assert.That(game.JourneyClearCount,Is.Zero);game.ShowGame();
            Assert.That(game.Machine.LivingWorld.Zone,Is.EqualTo(1));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator GliderWholeSilhouetteStaysInTheEmptyFlightBand()
        {
            foreach(var node in new[]{"C01","C22","C32","C37"}) {
                game.LoadNode(node);yield return null;game.RefreshLayout();var world=game.Machine.LivingWorld;
                Assert.That(world.FlightBand.y,Is.GreaterThan(world.FlightBand.x));
                for(int phase=0;phase<400;phase+=2) {
                    world.Paint(phase);var bounds=world.GliderBounds;
                    Assert.That(bounds.min.y,Is.GreaterThanOrEqualTo(world.FlightBand.x),node+" bottom at "+phase);
                    Assert.That(bounds.max.y,Is.LessThanOrEqualTo(world.FlightBand.y),node+" top at "+phase);
                }
            }
        }
        [UnityTest] public IEnumerator FutureStoryFixtureSelectsItsOwnRegionWithoutRewards()
        {
            var hash=game.Interaction.Attempt.CurrentStateHash;
            var story=typeof(CascadeGame).GetMethod("OpenJourneyStory",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            foreach(int index in new[]{15,30,0}) {
                story.Invoke(game,new object[]{index,false});yield return null;
                Assert.That(game.Machine.LivingWorld.Zone,Is.EqualTo(Cascade.Content.JourneyStory.Zone(index)));
                Assert.That(game.JourneyClearCount,Is.Zero);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            }
        }
        [UnityTest] public IEnumerator HomePauseAndSettingsPreserveAttemptAndFreezeCoaching()
        {
            game.LoadNode("C02"); game.Coach.Advance(6,true);float idle=game.Coach.IdleSeconds;
            string hash=game.Interaction.Attempt.CurrentStateHash;
            game.ShowHome();yield return new WaitForSecondsRealtime(.08f);
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That(game.Interaction.CanPlay,Is.False);
            Assert.That(game.Coach.IdleSeconds,Is.EqualTo(idle));
            yield return ClickButton("Settings");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));
            yield return ClickButton("Settings");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Settings));
            yield return ClickButton("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));
            yield return ClickButton("Backtomap");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            game.ShowGame();Assert.That(game.Interaction.CanPlay,Is.True);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator HomeWordsChoiceStartsAnExistingPinnedBinding()
        {
            game.ShowHome();yield return ClickButton("Lessons");yield return ClickButton("Words");yield return ClickButton("PlayWords·1");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));Assert.That(game.Interaction.Attempt.Mode,Is.EqualTo(PuzzleMode.Words));
            Assert.That(game.Interaction.Attempt.Current.Row.All(s=>s.Kind==SymbolKind.Glyph),Is.True);
            game.ShowLessons();yield return ClickButton("2");Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo("C02"));
            var before=game.Interaction.Attempt.Current;game.SelectMode(PuzzleMode.Shapes);
            Assert.That(game.Interaction.Attempt.Current,Is.SameAs(before));
        }
        [UnityTest] public IEnumerator FailureOffersFreeRetryAndUndo()
        {
            game.Interaction.Execute(PuzzleCommand.Drop);yield return AwaitPresentation();
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Fail));AssertResultControlsClearReceiver();yield return ClickButton("Retry");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
            game.Interaction.Execute(PuzzleCommand.Drop);yield return AwaitPresentation();yield return ClickButton("Undo");
            Assert.That(game.Interaction.Attempt.Current.Status,Is.EqualTo(PuzzleStatus.Playing));Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
        }
        void AssertResultControlsClearReceiver()
        {
            var receiver=(RectTransform)game.TargetRow.parent;var receiverCorners=new Vector3[4];receiver.GetWorldCorners(receiverCorners);
            foreach(var b in game.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b=>b.name=="Retry"||b.name=="Undo"||b.name=="Nextdelivery"||b.name=="Resulthome")) {
                var corners=new Vector3[4];((RectTransform)b.transform).GetWorldCorners(corners);
                Assert.That(corners[2].y,Is.LessThan(receiverCorners[0].y),b.name+" cannot cover the target");
            }
        }
        [UnityTest] public IEnumerator SettingsChangeActualPresentationPreferences()
        {
            game.ShowSettings();yield return ClickButton("Reducedmotion:Off");Assert.That(game.ReducedMotion,Is.True);
            yield return ClickButton("Effects:Full");Assert.That(game.Feedback.Quality,Is.Zero);
            yield return ClickButton("Comfort");yield return ClickButton("Highcontrast:Off");yield return ClickButton("Largertext:Off");
            Assert.That(game.Preferences.HighContrast&&game.Preferences.LargeText,Is.True);
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name=="Menu title").Single().color.r,Is.LessThan(.1f));
            yield return ClickButton("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));
        }
        [UnityTest] public IEnumerator HomeDuringAnimationKeepsTheCommittedRow()
        {
            game.LoadNode("C03");game.Interaction.Execute(PuzzleCommand.Swap(0));var committed=game.Interaction.Attempt.Current;
            game.ShowHome();yield return null;Assert.That(game.Interaction.Attempt.Current,Is.SameAs(committed));
            Assert.That(game.Interaction.IsPresenting,Is.False);Assert.That(game.Interaction.CanPlay,Is.False);
            game.ShowGame();Assert.That(game.VisibleFrame.Row,Is.EqualTo(committed.Row));Assert.That(game.Interaction.CanPlay,Is.True);
        }
        [UnityTest] public IEnumerator TutorialGestureCannotExecuteACommand()
        {
            game.Coach.Advance(16,true);string hash=game.Interaction.Attempt.CurrentStateHash;yield return null;yield return null;
            Assert.That(game.IsTutorialHandVisible,Is.True);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            game.Interaction.Execute(PuzzleCommand.Swap(0));yield return AwaitPresentation();
            Assert.That(game.Coach.LessonCompleted,Is.True);Assert.That(game.IsTutorialHandVisible,Is.False);
            Assert.That(game.Interaction.Attempt.HintUsed,Is.False);
        }
        [UnityTest] public IEnumerator ProvenDeadEndDoesNotConsumeOrRevealAHint()
        {
            game.LoadNode("C02");game.Interaction.Execute(PuzzleCommand.Swap(0));yield return AwaitPresentation();
            string hash=game.Interaction.Attempt.CurrentStateHash;float until=Time.realtimeSinceStartup+3;
            while(!game.HasDeadEndWarning&&Time.realtimeSinceStartup<until)yield return null;
            Assert.That(game.HasDeadEndWarning,Is.True);Assert.That(game.Interaction.Attempt.HintUsed,Is.False);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));yield return ClickButton("Undo");
            Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
        }
        [UnityTest] public IEnumerator CommitIsOnDiskBeforeAnimationAndReceiptSurvivesUndo()
        {
            game.Interaction.Execute(PuzzleCommand.Swap(0));
            Assert.That(game.Interaction.IsPresenting,Is.True);
            var loaded=new ProfileStore(game.ProfilePath).Load().Profile;
            Assert.That(loaded.Attempt.StateHash,Is.EqualTo(game.Interaction.Attempt.CurrentStateHash));
            Assert.That(loaded.Attempt.Commands.Length,Is.EqualTo(1));Assert.That(loaded.Clears.Count,Is.EqualTo(1));
            yield return AwaitPresentation();game.Undo();
            loaded=new ProfileStore(game.ProfilePath).Load().Profile;
            Assert.That(loaded.Attempt.Commands,Is.Empty);Assert.That(loaded.Attempt.UndoCount,Is.EqualTo(1));
            Assert.That(loaded.Clears.Count,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator ReloadAfterLandingPreservesEntryTransformAndUndo()
        {
            game.LoadNode("C37");
            foreach(var command in PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path) {
                game.Interaction.Execute(command);yield return AwaitPresentation();
                if(game.Interaction.Attempt.Current.ActiveFloor>0)break;
            }
            var attempt=game.Interaction.Attempt;string hash=attempt.CurrentStateHash;
            var before=attempt.History.Last().Before;int route=attempt.RouteLength;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.ShowGame();
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(route));
            game.SendMessage("OnApplicationFocus",false);game.SendMessage("OnApplicationFocus",true);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));game.Undo();
            Assert.That(game.Interaction.Attempt.Current,Is.EqualTo(before));
        }
        [UnityTest] public IEnumerator SettingsStoryAndWordPreferenceSurviveSceneReload()
        {
            game.ShowJourney();yield return ClickButton("Travel");yield return ClickButton("Skipstory");
            game.SetUiLocale("uk");game.ShowSettings();game.ProposePuzzleLocale("ru");game.ConfirmPuzzleLocale();
            game.SelectMode(PuzzleMode.Words);game.Preferences.Music=false;game.SendMessage("OnApplicationFocus",false);
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);
            Assert.That(game.UiLanguage,Is.EqualTo("uk"));Assert.That(game.Preferences.PuzzleLocale,Is.EqualTo("ru"));
            Assert.That(game.PreferredMode,Is.EqualTo(PuzzleMode.Words));Assert.That(game.Preferences.Music,Is.False);
            Assert.That(new ProfileStore(game.ProfilePath).Load().Profile.HeardStory,Does.Contain(0));
            yield return ClickButton("Travel");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing),"Heard story must not repeat");
        }
        [UnityTest] public IEnumerator IncompatibleAttemptOffersExplicitRestartAndKeepsLights()
        {
            game.Interaction.Execute(PuzzleCommand.Swap(0));yield return AwaitPresentation();
            string path=game.ProfilePath;UnityEngine.Object.Destroy(game.gameObject);yield return null;
            var store=new ProfileStore(path);var p=store.Load().Profile;p.Attempt.ContentHash=new string('0',64);store.Save(p);
            string disk=File.ReadAllText(path);
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);
            Assert.That(game.Recovery,Is.EqualTo(SaveRecovery.AttemptUnavailable));Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.SaveRecovery));
            Assert.That(game.JourneyClearCount,Is.EqualTo(1));Assert.That(File.ReadAllText(path),Is.EqualTo(disk));
            yield return ClickButton("Startanewpuzzle");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That(game.Recovery,Is.EqualTo(SaveRecovery.None));
            Assert.That(game.JourneyClearCount,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator FailedWritePausesAndCanRetryWithoutLosingCommittedRow()
        {
            string path=game.ProfilePath;File.Delete(path);Directory.CreateDirectory(path);
            game.Interaction.Execute(PuzzleCommand.Swap(0));string hash=game.Interaction.Attempt.CurrentStateHash;
            yield return null;yield return null;
            Assert.That(game.HasSaveError,Is.True);Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.SaveRecovery));
            Directory.Delete(path);yield return ClickButton("Trysavingagain");
            Assert.That(game.HasSaveError,Is.False);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            Assert.That(new ProfileStore(path).Load().Profile.Attempt.StateHash,Is.EqualTo(hash));
        }
        IEnumerator DeliverNode(string node)
        {
            game.LoadNode(node);yield return null;
            foreach(var command in PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path) {
                Assert.That(game.Interaction.Execute(command).Accepted,Is.True);yield return AwaitPresentation();
            }
        }
        [UnityTest] public IEnumerator EarnedParcelClaimIsDurableAndCannotBeFarmed()
        {
            foreach(string id in new[]{"C01","C02","C03"})yield return DeliverNode(id);
            Assert.That(game.Rewards.Parcels.Count,Is.EqualTo(1));game.ShowParcel();yield return ClickButton("Unwrap");
            Assert.That(game.Rewards.Fragments("leaf"),Is.EqualTo(1));
            Assert.That(new ProfileStore(game.ProfilePath).Load().Profile.Rewards.Parcels[0].Claimed,Is.True);
            yield return ClickButton("Keepgoing");yield return DeliverNode("C01");game.Undo();yield return null;
            Assert.That(game.Rewards.Parcels.Count,Is.EqualTo(1));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Assert.That(game.Rewards.ClaimParcel(1),Is.False);Assert.That(game.Rewards.Fragments("leaf"),Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator GiftChoiceChangesNextOrderAndMenusBlockPuzzleInput()
        {
            foreach(string id in new[]{"C01","C02","C03"})yield return DeliverNode(id);
            game.LoadNode("C02");game.ShowCollection();var before=game.Interaction.Attempt.CurrentStateHash;
            yield return ClickButton("SunButtonChoice");yield return ClickButton("Choosethisgift");
            Assert.That(game.Rewards.SelectedGift,Is.EqualTo("sun"));Assert.That(game.Rewards.Parcels[0].GiftId,Is.EqualTo("leaf"));
            Assert.That(game.Interaction.CanPlay,Is.False);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(before));
            Assert.That(new ProfileStore(game.ProfilePath).Load().Profile.Rewards.SelectedGift,Is.EqualTo("sun"));
        }
        [UnityTest] public IEnumerator StampTrailAndNormalDeliveryHaveSeparateRecords()
        {
            game.StartStampTrail("C35");yield return null;var attempt=game.Interaction.Attempt;
            Assert.That(PuzzleSolver.FindShortest(attempt.Resolver,goal:attempt.Goal).OptimalActions,Is.EqualTo(4));
            foreach(var command in PuzzleSolver.FindShortest(attempt.Resolver).Path){game.Interaction.Execute(command);yield return AwaitPresentation();}
            Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));Assert.That(attempt.Goal.IsSatisfied(attempt.Current),Is.False);
            Assert.That(game.JourneyClearCount,Is.EqualTo(1));Assert.That(game.Rewards.Records.Count,Is.EqualTo(1));
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.text=="The row is home. The stamp is still missing."),Is.True);
            game.StartStampTrail("C35");attempt=game.Interaction.Attempt;
            foreach(var command in PuzzleSolver.FindShortest(attempt.Resolver,goal:attempt.Goal).Path){game.Interaction.Execute(command);yield return AwaitPresentation();}
            Assert.That(game.JourneyClearCount,Is.EqualTo(1));Assert.That(game.Rewards.Records.Count,Is.EqualTo(2));
            Assert.That(game.Rewards.QuestCount(2),Is.EqualTo(1));
            game.ShowRecords();yield return ClickButton("Next");yield return ClickButton("Tryagain");
            Assert.That(game.Interaction.Attempt.Goal.RequiredStampBits,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator RouteRetryKeepsItsConcretePuzzleLocale()
        {
            game.Catalog.TryGetLocalizedBinding("C35","uk",out var level);game.LoadLevel(level.Definition,"uk");yield return null;
            foreach(var command in PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path){game.Interaction.Execute(command);yield return AwaitPresentation();}
            game.Preferences.PuzzleLocale="ru";game.SetUiLocale("ru");game.ShowRecords();yield return null;
            yield return ClickButton("Tryagain");Assert.That(game.Interaction.Attempt.ActiveLocale,Is.EqualTo("uk"));
            Assert.That(game.Interaction.Attempt.PinnedContentHash,Is.EqualTo(level.Definition.ContentHash));
        }
        [UnityTest] public IEnumerator NewRegionsCanBeDeliveredThroughDirectDragAndButtons()
        {
            foreach(string id in new[]{"C41","C65","C89","C113","C137","C160"}) {
                game.LoadNode(id);yield return null;yield return null;
                var attempt=game.Interaction.Attempt;var views=game.SlotViews.ToArray();
                var route=PuzzleSolver.FindShortest(attempt.Resolver);Assert.That(route.Status,Is.EqualTo(SearchStatus.Solved));
                foreach(var command in route.Path) {
                    int before=attempt.RouteLength;var predicted=attempt.Predict(command).State;
                    if(command.Kind==CommandKind.EarlyDrop)yield return ClickButton("Drop");
                    else {yield return MouseFrame(Point(command.Edge),true);yield return MouseFrame(Point(command.Edge+1),true);yield return MouseFrame(Point(command.Edge+1),false);}
                    Assert.That(attempt.RouteLength,Is.EqualTo(before+1),id+" "+command);Assert.That(attempt.Current,Is.EqualTo(predicted));
                    yield return AwaitPresentation();
                }
                Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));Assert.That(game.SlotViews,Is.EqualTo(views));
            }
        }
        [UnityTest] public IEnumerator AllFortyLevelsCanBeDeliveredThroughActualMouseAndButtons()
        {
            var events = new HashSet<PuzzleEventKind>(); int commands = 0;
            foreach (var content in game.Catalog.PrimaryLevels.Take(40)) {
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
