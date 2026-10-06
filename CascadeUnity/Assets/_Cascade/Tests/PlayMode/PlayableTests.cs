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
            yield return AwaitPreparation();
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
            if(!game.PreparationReady)yield return AwaitPreparation();
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
            if(!game.PreparationReady)yield return AwaitPreparation();
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left, held));
            yield return null; yield return null;
        }
        IEnumerator TouchFrame(int id, Vector2 point, UnityEngine.InputSystem.TouchPhase phase)
        {
            if(!game.PreparationReady)yield return AwaitPreparation();
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
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>(true).Any(t=>t.name=="Device state"),Is.False,"Forecasts do not announce outcomes under hanging tools.");
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
        [UnityTest] public IEnumerator FlexibleRopesHaveIndependentCurvatureAndReconnectAfterAFloorBreak()
        {
            game.LoadNode("C30");game.ShowGame();game.ReducedMotion=false;yield return AwaitPreparation();
            var ropes=game.Machine.GetComponentsInChildren<LineRenderer>().Where(l=>l.name.Contains("braided rope")).ToArray();
            string hash=game.Interaction.Attempt.CurrentStateHash;float irregularity=0,independence=0;
            float Offset(LineRenderer line,int i){var a=line.GetPosition(0);var b=line.GetPosition(24);var chord=Vector3.Lerp(a,b,i/24f);return line.GetPosition(i).x-chord.x;}
            double until=Time.realtimeSinceStartupAsDouble+4;
            while(Time.realtimeSinceStartupAsDouble<until){
                irregularity=Mathf.Max(irregularity,Mathf.Abs(Offset(ropes[0],12)-(Offset(ropes[0],6)+Offset(ropes[0],18))*.7071068f));
                independence=Mathf.Max(independence,Mathf.Abs(Offset(ropes[0],12)-Offset(ropes[1],12)));
                yield return null;
            }
            Assert.That(irregularity,Is.GreaterThan(.0005f),"A flexible rope is not one prescribed sine bow.");
            Assert.That(independence,Is.GreaterThan(.002f),"Opposite ropes respond to different local air flow.");
            game.Interaction.SetPaused(true);var frozen=new Vector3[ropes[0].positionCount];ropes[0].GetPositions(frozen);
            yield return new WaitForSecondsRealtime(.15f);for(int i=0;i<frozen.Length;i++)Assert.That(ropes[0].GetPosition(i),Is.EqualTo(frozen[i]));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));game.Interaction.SetPaused(false);
            game.LoadNode("C03");yield return AwaitPreparation();ropes=game.Machine.GetComponentsInChildren<LineRenderer>().Where(l=>l.name.Contains("braided rope")).ToArray();int old=ropes[0].positionCount;
            game.Interaction.SetSuspended(false);game.Interaction.Execute(PuzzleCommand.Swap(0));yield return AwaitPresentation();yield return null;
            Assert.That(ropes[0].positionCount,Is.LessThan(old));
            var knots=game.Machine.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Rope knot").ToArray();
            foreach(var line in ropes){Assert.That(knots.Min(k=>Vector2.Distance(k.transform.position,line.GetPosition(24))),Is.LessThan(.002f));for(int i=0;i<line.positionCount;i++)Assert.That(float.IsNaN(line.GetPosition(i).x),Is.False);}
        }
        [UnityTest] public IEnumerator ReloadingASevenPieceTargetExcludesOldLayoutChildrenImmediately()
        {
            game.LoadNode("C03");game.ShowGame();game.LoadNode("C30");game.ShowGame();yield return null;yield return null;
            var labels=game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name=="TargetSymbol").ToArray();Assert.That(labels.Length,Is.EqualTo(7));
            var bodies=game.Machine.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.StartsWith("Target ")&&!r.name.StartsWith("Target socket")).ToArray();Assert.That(bodies.Length,Is.EqualTo(7));
            for(int i=0;i<labels.Length;i++){var rect=labels[i].rectTransform;var center=rect.TransformPoint(rect.rect.center);var body=bodies.Single(b=>b.name=="Target "+i);Assert.That(Vector2.Distance(center,body.transform.position),Is.LessThan(.001f),"The live glyph and physical target body share their new layout center.");}
        }
        [UnityTest] public IEnumerator PhysicalDebrisFallsInItsOwnSceneAndRespectsPauseAndQuiet()
        {
            game.LoadNode("C18");game.ShowGame();game.ReducedMotion=false;yield return AwaitPreparation();string hash=game.Interaction.Attempt.CurrentStateHash;
            game.Feedback.WoodChips(game.Machine.FloorPosition(0)+Vector3.down*.5f,4,false,1);
            var bodies=Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Where(b=>b.name.StartsWith("Pooled workshop dust")&&!b.isKinematic).ToArray();
            Assert.That(bodies.Length,Is.EqualTo(4));Assert.That(bodies[0].gameObject.scene.GetPhysicsScene(),Is.Not.EqualTo(Physics.defaultPhysicsScene));
            float height=bodies[0].position.y;var orientation=bodies[0].rotation;
            yield return new WaitForSecondsRealtime(.4f);Assert.That(bodies[0].position.y,Is.LessThan(height-.1f));Assert.That(Quaternion.Angle(orientation,bodies[0].rotation),Is.GreaterThan(3));
            game.Interaction.SetPaused(true);var positions=bodies.Select(b=>b.position).ToArray();yield return new WaitForSecondsRealtime(.12f);
            for(int i=0;i<bodies.Length;i++)Assert.That(bodies[i].position,Is.EqualTo(positions[i]));
            game.Interaction.SetPaused(false);game.ReducedMotion=true;yield return null;yield return null;
            positions=bodies.Select(b=>b.position).ToArray();yield return new WaitForSecondsRealtime(.1f);for(int i=0;i<bodies.Length;i++){Assert.That(bodies[i].isKinematic,Is.True);Assert.That(bodies[i].position,Is.EqualTo(positions[i]));}
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));game.Feedback.ClearTails();Assert.That(game.Feedback.ActiveParticleCount,Is.Zero);
        }
        [UnityTest] public IEnumerator ContactMomentumReachesDeckAndIndependentRopeSpans()
        {
            game.LoadNode("C30");game.ShowGame();game.ReducedMotion=false;yield return AwaitPreparation();yield return new WaitForSecondsRealtime(.4f);
            string hash=game.Interaction.Attempt.CurrentStateHash;float sag=game.Machine.LoadedSag;
            var ropes=game.Machine.GetComponentsInChildren<LineRenderer>().Where(l=>l.name.Contains("braided rope")).ToArray();var before=ropes.Select(l=>l.GetPosition(12)).ToArray();
            game.Machine.ContactImpulse(0,0,.9f,1);float peak=sag,travel=0;double until=Time.realtimeSinceStartupAsDouble+.4;
            while(Time.realtimeSinceStartupAsDouble<until){peak=Mathf.Max(peak,game.Machine.LoadedSag);for(int i=0;i<ropes.Length;i++)travel=Mathf.Max(travel,Vector3.Distance(before[i],ropes[i].GetPosition(12)));yield return null;}
            Assert.That(peak,Is.GreaterThan(sag+.003f));Assert.That(travel,Is.GreaterThan(.003f));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            game.Interaction.Execute(PuzzleCommand.Swap(0));string committed=game.Interaction.Attempt.CurrentStateHash;yield return AwaitPresentation();
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(committed));Assert.That(game.Machine.TokenPose(0),Is.EqualTo(PiecePose.Resting));
            game.ReducedMotion=true;yield return null;sag=game.Machine.LoadedSag;game.Machine.ContactImpulse(0,0,1,1);yield return new WaitForSecondsRealtime(.1f);Assert.That(game.Machine.LoadedSag,Is.EqualTo(sag).Within(.001f));
        }
        [UnityTest] public IEnumerator ConnectedRopesSurviveReducedMotionAndResize()
        {
            game.LoadNode("C03");game.ShowGame();game.ReducedMotion=true;yield return AwaitPreparation();game.RefreshLayout();
            var ropes=game.Machine.GetComponentsInChildren<LineRenderer>().Where(l=>l.name.Contains("braided rope")).ToArray();
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
        [UnityTest] public IEnumerator HeldLetterKeepsItsLiftAndCancelledDragSettlesWithoutMove()
        {
            game.Catalog.TryGetLocalizedBinding("C18","ru",out var words);game.LoadLevel(words.Definition,"ru");
            yield return AwaitPreparation();game.ShowGame();game.RefreshLayout();
            string hash=game.Interaction.Attempt.CurrentStateHash;
            yield return TouchFrame(41,Point(0),UnityEngine.InputSystem.TouchPhase.Began);
            yield return new WaitForSecondsRealtime(.18f);
            Assert.That(game.Machine.TokenVisualPosition(0).y-game.Machine.TokenPosition(0).y-game.Machine.TokenSupportOffset(0).y,Is.GreaterThan(game.Machine.SelectionLiftWorld(0)*1.5f));
            yield return TouchFrame(41,Point(1),UnityEngine.InputSystem.TouchPhase.Moved);
            yield return new WaitForSecondsRealtime(.1f);
            var held=game.Machine.TokenVisualPosition(0);
            Assert.That(held.x-game.Machine.TokenPosition(0).x,Is.GreaterThan(.1f),"Preview must not reset the held pose.");
            for(int i=0;i<6;i++) {
                yield return null;
                var face=game.SlotViews[0].GetComponentInChildren<TextMeshProUGUI>().rectTransform;
                Assert.That(Vector2.Distance(face.position,game.Machine.TokenVisualPosition(0)),Is.LessThan(.002f));
                Assert.That(Mathf.Abs(Mathf.DeltaAngle(face.eulerAngles.z,game.Machine.TokenVisualAngle(0))),Is.LessThan(.01f));
            }
            yield return TouchFrame(41,Point(1),UnityEngine.InputSystem.TouchPhase.Canceled);
            Assert.That(game.IsHoldingToken,Is.False);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            yield return new WaitForSecondsRealtime(.35f);yield return null;
            Assert.That(game.OwnsTokenPose(0),Is.False);
            Assert.That(Vector3.Distance(game.Machine.TokenVisualPosition(0),game.Machine.TokenPosition(0)+game.Machine.TokenSupportOffset(0)),Is.LessThan(.01f));
            Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
        }
        [UnityTest] public IEnumerator ReleasedDragTransfersItsPoseIntoAtomicExchange()
        {
            game.LoadNode("C18");yield return AwaitPreparation();game.ShowGame();game.RefreshLayout();
            var symbol=game.Interaction.Attempt.Current.Row[0];int views=game.SlotViews.Count;
            var pointer=new PointerEventData(EventSystem.current){pointerId=804,button=PointerEventData.InputButton.Left,position=Point(0),pointerCurrentRaycast=new RaycastResult{gameObject=game.SlotViews[0].gameObject}};
            game.SlotViews[0].OnPointerDown(pointer);yield return new WaitForSecondsRealtime(.15f);
            pointer.position=Point(1);pointer.pointerCurrentRaycast=new RaycastResult{gameObject=game.SlotViews[1].gameObject};
            game.SlotViews[0].OnBeginDrag(pointer);game.SlotViews[0].OnDrag(pointer);yield return new WaitForSecondsRealtime(.1f);
            Vector3 position=game.Machine.TokenVisualPosition(0),scale=game.Machine.TokenVisualScale(0);
            game.SlotViews[0].OnPointerUp(pointer);
            Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));
            Assert.That(game.VisibleFrame.Row[1],Is.EqualTo(symbol));
            Assert.That(Vector3.Distance(game.Machine.TokenVisualPosition(1),position),Is.LessThan(.002f),"Exchange starts where the hand released the body.");
            Assert.That(Vector3.Distance(game.Machine.TokenVisualScale(1),scale),Is.LessThan(.002f),"Held enlargement must not snap back before travel.");
            string hash=game.Interaction.Attempt.CurrentStateHash;
            game.SlotViews[0].OnEndDrag(pointer);yield return AwaitPresentation();
            Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            Assert.That(game.SlotViews.Count,Is.EqualTo(views));
        }
        [UnityTest] public IEnumerator TapSelectionUsesTheBodyContourAndNeverPaintsASlotPanel()
        {
            yield return AwaitPreparation();game.ReducedMotion=false;
            yield return TouchFrame(711,Point(0),UnityEngine.InputSystem.TouchPhase.Began);
            yield return TouchFrame(711,Point(0),UnityEngine.InputSystem.TouchPhase.Ended);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(game.Interaction.SelectedSlot,Is.EqualTo(0));
            var body=game.GetComponentsInChildren<MeshFilter>().Single(m=>m.name=="Persistent token 0");
            var contour=game.GetComponentsInChildren<MeshFilter>().Single(m=>m.name=="Piece contour 0");
            Assert.That(contour.sharedMesh,Is.SameAs(body.sharedMesh));
            foreach(var slot in game.SlotViews)Assert.That(slot.GetComponent<UnityEngine.UI.Image>().color.a,Is.Zero);
            float lift=game.Machine.TokenVisualPosition(0).y-game.Machine.TokenPosition(0).y-game.Machine.TokenSupportOffset(0).y;
            Assert.That(lift,Is.GreaterThan(.025f));Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
        }
        [UnityTest] public IEnumerator DragFollowsFingerOutsideNeighbourClampAndCancelsFreely()
        {
            game.LoadNode("C18");yield return AwaitPreparation();game.RefreshLayout();
            foreach(bool quiet in new[]{false,true}) {
                game.ReducedMotion=quiet;
                string hash=game.Interaction.Attempt.CurrentStateHash;Vector3 original=game.Machine.TokenVisualPosition(0);
                var pointer=new PointerEventData(EventSystem.current){pointerId=712,button=PointerEventData.InputButton.Left,position=Point(0),pointerCurrentRaycast=new RaycastResult{gameObject=game.SlotViews[0].gameObject}};
                game.SlotViews[0].OnPointerDown(pointer);var start=pointer.position;
                // Deliberate vertical-only movement is allowed even with Reduced Motion.
                pointer.position=start+Vector2.up*90;pointer.pointerCurrentRaycast=new RaycastResult();game.SlotViews[0].OnBeginDrag(pointer);game.SlotViews[0].OnDrag(pointer);yield return null;
                var origin=Camera.main.WorldToScreenPoint(original);var displayed=Camera.main.WorldToScreenPoint(game.Machine.TokenVisualPosition(0));
                Assert.That(displayed.x-origin.x,Is.EqualTo(0).Within(2));Assert.That(displayed.y-origin.y,Is.GreaterThan(80));
                // Then move diagonally across several cells; legality never limits artwork travel.
                pointer.position=Point(3)+Vector2.up*90;game.SlotViews[0].OnDrag(pointer);yield return null;
                displayed=Camera.main.WorldToScreenPoint(game.Machine.TokenVisualPosition(0));
                Assert.That(displayed.x-origin.x,Is.EqualTo(pointer.position.x-start.x).Within(2));
                Assert.That(displayed.y-origin.y,Is.GreaterThan(80));
                foreach(var slot in game.SlotViews)Assert.That(slot.GetComponent<UnityEngine.UI.Image>().color.a,Is.Zero);
                game.SlotViews[0].OnPointerUp(pointer);yield return new WaitForSecondsRealtime(.3f);
                Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.IsHoldingToken,Is.False);
            }
        }
        [UnityTest] public IEnumerator PhysicalLandingTumblesPausesAndReturnsTheCommittedRowToItsSeats()
        {
            game.LoadNode("C03");yield return AwaitPreparation();game.ReducedMotion=false;game.ShowGame();
            game.Interaction.Execute(PuzzleCommand.Drop);
            float limit=Time.realtimeSinceStartup+3;while(!game.Feedback.RowPhysics.Active&&Time.realtimeSinceStartup<limit)yield return null;
            Assert.That(game.Feedback.RowPhysics.Active,Is.True);string hash=game.Interaction.Attempt.CurrentStateHash;
            var row=game.Interaction.Attempt.Current.Row.ToArray();yield return new WaitForSecondsRealtime(.18f);
            Assert.That(Enumerable.Range(0,game.SlotViews.Count).Max(i=>Mathf.Abs(game.Feedback.RowPhysics.Angle(i))),Is.GreaterThan(4));
            game.Interaction.SetPaused(true);var paused=game.Feedback.RowPhysics.Position(0);yield return new WaitForSecondsRealtime(.16f);
            Assert.That(Vector3.Distance(paused,game.Feedback.RowPhysics.Position(0)),Is.LessThan(.001f));
            game.Interaction.SetPaused(false);yield return AwaitPresentation();
            Assert.That(game.Feedback.RowPhysics.Active,Is.False);Assert.That(game.Feedback.RowPhysics.LandedCount,Is.GreaterThan(0));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.Current.Row,Is.EqualTo(row));
            for(int i=0;i<game.SlotViews.Count;i++)Assert.That(Vector3.Distance(game.Machine.TokenVisualPosition(i),game.Machine.TokenPosition(i)+game.Machine.TokenSupportOffset(i)),Is.LessThan(.012f));
        }
        [UnityTest] public IEnumerator PhoneShakeUnsettlesPresentationOnlyAndRespectsReducedMotion()
        {
            yield return AwaitPreparation();game.ReducedMotion=false;
            var acceleration=InputSystem.AddDevice<Accelerometer>();var feedback=game.GetComponent<PhoneShakeFeedback>();
            try{
                yield return null;InputSystem.QueueDeltaStateEvent(acceleration.acceleration,new Vector3(0,-1,0));InputSystem.Update();yield return null;
                string hash=game.Interaction.Attempt.CurrentStateHash;int count=feedback.AppliedImpulses;
                InputSystem.QueueDeltaStateEvent(acceleration.acceleration,new Vector3(1.6f,-1.5f,0));InputSystem.Update();yield return null;
                Assert.That(feedback.AppliedImpulses,Is.EqualTo(count+1));Assert.That(game.Feedback.RowPhysics.Shaking,Is.True);
                yield return new WaitForSecondsRealtime(.15f);Assert.That(Mathf.Abs(game.Machine.TokenVisualAngle(0)),Is.GreaterThan(1));
                yield return new WaitForSecondsRealtime(1);Assert.That(game.Feedback.RowPhysics.Active,Is.False);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
                game.ReducedMotion=true;count=feedback.AppliedImpulses;feedback.SampleAcceleration(new Vector3(-2,-1,0),.04f);yield return null;
                Assert.That(feedback.AppliedImpulses,Is.EqualTo(count));Assert.That(game.Feedback.RowPhysics.Active,Is.False);
            }finally{InputSystem.RemoveDevice(acceleration);}
        }
        [UnityTest] public IEnumerator UnregisteredStoreCannotChargeAndAdSuspensionKeepsFocusLossClosed()
        {
            game.SetUiLocale("ru");game.ShowNoAds();yield return null;
            Assert.That(game.PurchaseStore.State,Is.EqualTo(PurchaseState.Unconfigured));Assert.That(game.PurchaseStore.CanBuy,Is.False);
            Assert.That(game.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Purchasequiet").interactable,Is.False);
            game.ShowGame();game.SetNativeAdSuspended(true);game.SendMessage("OnApplicationFocus",false);game.SetNativeAdSuspended(false);
            Assert.That(game.Interaction.IsSuspended,Is.True);game.SendMessage("OnApplicationFocus",true);Assert.That(game.Interaction.IsSuspended,Is.False);
        }
        [UnityTest] public IEnumerator FarDragKeepsUnmovedDuplicateInItsOwnSeat()
        {
            var source=game.Catalog.GetPrimary("C18").Definition;
            var row=new[]{source.InitialSymbolIds[0],source.InitialSymbolIds[0],source.InitialSymbolIds[2],source.InitialSymbolIds[3]};
            var target=new[]{row[3],row[1],row[2],row[0]};
            game.LoadLevel(new LevelDefinition(source.PuzzleId,source.CampaignNodeId,source.SchemaVersion,source.RulesVersion,"touch-duplicate-fixture",row,target,source.TargetSegments,new[]{new FloorDefinition(4,new[]{true,true,true})},1));
            yield return AwaitPreparation();game.ShowGame();game.RefreshLayout();
            var pointer=new PointerEventData(EventSystem.current){pointerId=805,button=PointerEventData.InputButton.Left,position=Point(0),pointerCurrentRaycast=new RaycastResult{gameObject=game.SlotViews[0].gameObject}};
            game.SlotViews[0].OnPointerDown(pointer);yield return new WaitForSecondsRealtime(.15f);
            pointer.position=Point(3);pointer.pointerCurrentRaycast=new RaycastResult{gameObject=game.SlotViews[3].gameObject};
            game.SlotViews[0].OnBeginDrag(pointer);game.SlotViews[0].OnDrag(pointer);yield return new WaitForSecondsRealtime(.1f);
            var held=game.Machine.TokenVisualPosition(0);var unmoved=game.Machine.TokenVisualPosition(1);var scale=game.Machine.TokenVisualScale(1);
            game.SlotViews[0].OnPointerUp(pointer);
            Assert.That(Vector3.Distance(game.Machine.TokenVisualPosition(3),held),Is.LessThan(.002f));
            Assert.That(Vector3.Distance(game.Machine.TokenVisualPosition(1),unmoved),Is.LessThan(.002f));
            Assert.That(Vector3.Distance(game.Machine.TokenVisualScale(1),scale),Is.LessThan(.002f));
            Assert.That(game.Interaction.Attempt.Current.ReachCharges,Is.Zero);yield return AwaitPresentation();
            Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));
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
        [UnityTest] public IEnumerator ContextualInstructionsWaitCanBeDismissedAndDoNotTouchPuzzleState()
        {
            game.Coach.Enabled=false;game.ReducedMotion=true;
            var paper=game.GetComponentsInChildren<CanvasGroup>(true).Single(c=>c.name=="Quiet instruction paper");
            var close=game.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.name=="Dismiss instruction");
            string hash=game.Interaction.Attempt.CurrentStateHash;int route=game.Interaction.Attempt.RouteLength;
            game.SetPresentationFeedback("Drag a piece onto its neighbour.");yield return null;
            Assert.That(paper.alpha,Is.Zero);Assert.That(paper.blocksRaycasts,Is.False);
            game.SendMessage("TogglePause");yield return new WaitForSecondsRealtime(.3f);
            Assert.That(paper.alpha,Is.Zero);game.SendMessage("TogglePause");
            yield return new WaitForSecondsRealtime(3.4f);
            Assert.That(paper.alpha,Is.EqualTo(1));Assert.That(paper.blocksRaycasts,Is.True);
            close.onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);
            Assert.That(paper.alpha,Is.Zero);Assert.That(paper.blocksRaycasts,Is.False);
            game.SetPresentationFeedback("Tap Descend to take the row down.");yield return null;
            Assert.That(paper.alpha,Is.Zero,"A new instruction also waits before appearing.");
            yield return new WaitForSecondsRealtime(3.4f);Assert.That(paper.alpha,Is.EqualTo(1));
            close.onClick.Invoke();game.LoadNode("C01");yield return AwaitPreparation();
            Assert.That(paper.alpha,Is.Zero,"A newly loaded level starts with an uncluttered header.");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(route));Assert.That(game.Interaction.Attempt.HintUsed,Is.False);
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
                Assert.That(ac[0].y, Is.GreaterThan(bc[2].y), "The header hint cannot cover precise input");
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
            game.ShowJourney();game.TurnJourneyMap(1);yield return null;game.TurnJourneyMap(1);yield return null;game.TurnJourneyMap(1);yield return null;
            yield return null;Canvas.ForceUpdateCanvases();var weather=game.GetComponentsInChildren<WorldWeatherGraphic>();Assert.That(weather.Length,Is.EqualTo(1));
            Assert.That(weather[0].canvasRenderer,Is.Not.Null);Assert.That(weather[0].canvasRenderer.GetMesh().vertexCount,Is.GreaterThan(0));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator DirectedWindIsForecastInTheGuideAndPlayableThroughRealInput()
        {
            game.SetUiLocale("en");game.LoadDirectedLevel(game.Catalog.GetPrimary("C81").Definition,"en",Cascade.Application.DeliveryMood.Wind);
            game.ShowGame();yield return AwaitPreparation();game.RefreshLayout();yield return null;
            var attempt=game.Interaction.Attempt;Assert.That(attempt.Direction.Mood,Is.EqualTo(Cascade.Application.DeliveryMood.Wind));
            string hash=attempt.CurrentStateHash;yield return ClickButton("Landing movement");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Devices));
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.text=="Wind on landing"),Is.True);
            Assert.That(attempt.CurrentStateHash,Is.EqualTo(hash));game.CloseMenu();
            var proof=PuzzleSolver.FindShortest(attempt.Resolver);Assert.That(game.MinimumMoves,Is.EqualTo(proof.OptimalActions));bool gust=false,forecast=false;
            game.Interaction.Committed+=r=>gust|=r.Events.Any(e=>e.Kind==PuzzleEventKind.EntryPermuted&&game.IsWindFloor(e.Floor));
            foreach(var command in proof.Path) {
                var predicted=attempt.Predict(command).State;
                if(command.Kind==CommandKind.EarlyDrop)yield return ClickButton("Drop");
                else {yield return MouseFrame(Point(command.Edge),true);yield return MouseFrame(Point(command.Edge+1),true);yield return MouseFrame(Point(command.Edge+1),false);}
                Assert.That(attempt.Current,Is.EqualTo(predicted));
                double deadline=Time.realtimeSinceStartupAsDouble+30;
                while(game.Interaction.IsPresenting&&Time.realtimeSinceStartupAsDouble<deadline){
                    if(game.ReactionBanner.IsWindWarning&&!forecast){
                        forecast=true;float age=game.ReactionBanner.Elapsed;string committed=attempt.CurrentStateHash;
                        game.Interaction.SetPaused(true);yield return new WaitForSecondsRealtime(.12f);
                        Assert.That(game.ReactionBanner.Elapsed,Is.EqualTo(age));Assert.That(attempt.CurrentStateHash,Is.EqualTo(committed));game.Interaction.SetPaused(false);
                    }
                    yield return null;
                }
                Assert.That(game.Interaction.IsPresenting,Is.False);
            }
            Assert.That(forecast,Is.True);Assert.That(gust,Is.True);Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
            var loaded=new ProfileStore(game.ProfilePath).Load();var restored=loaded.Profile.RestoreAttempt(game.Catalog);
            Assert.That(restored.CurrentStateHash,Is.EqualTo(attempt.CurrentStateHash));Assert.That(loaded.Profile.Clears.Any(c=>c.CampaignNodeId=="C81"),Is.True);
        }
        [UnityTest] public IEnumerator UnprovenTightVariantFallsBackBeforeAnyInputOpens()
        {
            var source=game.Catalog.GetPrimary("C42").Definition;
            var recipe=Cascade.Application.DeliveryDirector.Plan(source,Cascade.Application.DeliveryMood.Tight);
            Assert.That(PuzzleSolver.FindShortest(new PuzzleResolver(recipe.Apply(source))).Status,Is.EqualTo(SearchStatus.Unsolvable));
            game.LoadDirectedLevel(source,"en",Cascade.Application.DeliveryMood.Tight);game.ShowGame();
            Assert.That(game.Interaction.CanPlay,Is.False);Assert.That(game.Interaction.PointerDown(91,0),Is.False);
            yield return AwaitPreparation();Assert.That(game.Interaction.Attempt.Direction,Is.Null);
            Assert.That(game.Interaction.Attempt.PinnedContentHash,Is.EqualTo(source.ContentHash));Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
        }
        [UnityTest] public IEnumerator RepeatedJourneyFailuresOfferNewWoodAndOrdinaryRetryKeepsTheSamePuzzle()
        {
            // Pinned single attempts retain their recovery flow; fresh journeys now have checkpoints.
            game.LoadNode("C81");game.ShowGame();
            typeof(CascadeGame).GetField("journeyActive",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(game,true);
            yield return AwaitPreparation();string pin=game.Interaction.Attempt.PinnedContentHash;
            for(int failure=0;failure<2;failure++) {
                yield return ClickButton("Drop");int pressure=game.Director.Struggle;game.Retry();
                Assert.That(game.Director.Struggle,Is.EqualTo(pressure),"A blocked retry during falling is not a new failure");
                yield return AwaitPresentation();yield return ClickButton("Drop");yield return AwaitPresentation();
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Fail));
                if(failure==0){game.Retry();Assert.That(game.Interaction.Attempt.PinnedContentHash,Is.EqualTo(pin));}
            }
            Assert.That(game.Director.Struggle,Is.GreaterThanOrEqualTo(4));yield return ClickButton("Nextdelivery");
            yield return AwaitPreparation();Assert.That(game.Interaction.Attempt.Direction.Mood,Is.EqualTo(Cascade.Application.DeliveryMood.Recovery));
            Assert.That(game.Interaction.Attempt.Level.Floors[0].BaseDurability,Is.EqualTo(game.Catalog.GetPrimary("C81").Definition.Floors[0].BaseDurability+1));
            Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);Assert.That(game.JourneyClearCount,Is.Zero);
        }
        IEnumerator AwaitDescentReady()
        {
            double began=Time.realtimeSinceStartupAsDouble;
            while((game.IsPlanningDescent||game.IsDescending||game.Interaction.IsPresenting||!game.PreparationReady)&&Time.realtimeSinceStartupAsDouble-began<30)yield return null;
            Assert.That(game.IsPlanningDescent,Is.False);Assert.That(game.IsDescending,Is.False);Assert.That(game.Interaction.IsPresenting,Is.False);Assert.That(game.PreparationReady,Is.True);Assert.That(game.Descent,Is.Not.Null);
        }
        IEnumerator PlayDescentStage()
        {
            yield return AwaitDescentReady();game.RefreshLayout();yield return null;
            foreach(string encoded in game.Descent.Stages[game.Descent.StageIndex].Witness) {
                var command=encoded=="D"?PuzzleCommand.Drop:PuzzleCommand.Swap(int.Parse(encoded));
                var predicted=game.Interaction.Attempt.Predict(command).State;
                if(command.Kind==CommandKind.EarlyDrop)yield return ClickButton("Drop");
                else {yield return MouseFrame(Point(command.Edge),true);yield return MouseFrame(Point(command.Edge+1),true);yield return MouseFrame(Point(command.Edge+1),false);}
                Assert.That(game.Interaction.Attempt.Current,Is.EqualTo(predicted),"stage "+game.Descent.StageIndex+" command "+encoded+" actual route "+game.Interaction.Attempt.RouteLength+" canPlay "+game.Interaction.CanPlay+" current "+string.Join(",",game.Interaction.Attempt.Current.Row)+" expected "+string.Join(",",predicted.Row));yield return AwaitPresentation();yield return null;
            }
            Assert.That(game.Interaction.Attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
        }
        [UnityTest] public IEnumerator SoloMixedMapKeepsItsCheckpointAndItsOwnSequenceAcrossReload()
        {
            game.ReducedMotion=true;game.SendMessage("ChooseSoloMode",(object)PuzzleMode.Mixed);
            var sequence=game.Catalog.LocalizedLevels.Where(c=>c.Mode==PuzzleMode.Mixed&&c.Locale=="en").ToArray();
            game.SendMessage("StartContent",sequence[0]);yield return PlayDescentStage();
            game.GoLower();yield return AwaitDescentReady();string pin=game.Interaction.Attempt.PinnedContentHash;
            game.ShowJourney();Assert.That(game.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Travel").GetComponentInChildren<TextMeshProUGUI>().text,Is.EqualTo("Continue"));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.ShowJourney();yield return null;
            Assert.That(game.PreferredMode,Is.EqualTo(PuzzleMode.Mixed));Assert.That(game.Descent.StageIndex,Is.EqualTo(1));
            yield return ClickButton("Travel");Assert.That(game.Interaction.Attempt.PinnedContentHash,Is.EqualTo(pin));
            yield return PlayDescentStage();yield return ClickButton("Nextdelivery");yield return PlayDescentStage();yield return ClickButton("Continuecompletion");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(game.IsContentOpen(sequence[1]),Is.True);Assert.That(game.IsContentOpen(sequence[2]),Is.False);
            var caption=game.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Travel").GetComponentInChildren<TextMeshProUGUI>().text;
            Assert.That(caption,Does.Contain(sequence[1].Definition.CampaignNodeId.TrimStart('C','0')));
        }
        [UnityTest] public IEnumerator EarlyLevelUsesThreeStagesThenStatsThenMapAndKeepsFutureLevelsClosed()
        {
            game.ReducedMotion=true;var first=game.Catalog.GetPrimary("C01");var future=game.Catalog.GetPrimary("C03");
            Assert.That(game.IsContentOpen(first),Is.True);Assert.That(game.IsContentOpen(future),Is.False);
            string before=game.Interaction.Attempt.PinnedContentHash;game.SendMessage("StartContent",future);yield return null;
            Assert.That(game.Interaction.Attempt.PinnedContentHash,Is.EqualTo(before));
            game.SendMessage("StartContent",first);yield return PlayDescentStage();
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Checkpoint));Assert.That(game.JourneyClearCount,Is.Zero);
            Assert.That(new ProfileStore(game.ProfilePath).Load().Profile.Completion,Is.Null);
            yield return ClickButton("Nextdelivery");yield return PlayDescentStage();Assert.That(game.JourneyClearCount,Is.Zero);
            yield return ClickButton("Nextdelivery");yield return PlayDescentStage();
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Win));Assert.That(game.LastCompletion.Stages,Is.EqualTo(3));
            Assert.That(game.LastCompletion.Stars,Is.EqualTo(3));Assert.That(game.LastCompletion.Coins,Is.EqualTo(40));Assert.That(game.LastCompletion.Lights,Is.EqualTo(1));
            Assert.That(game.LastCompletion.Moves,Is.EqualTo(game.Descent.Stages.Sum(s=>s.Minimum)));
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.text.Contains("all 3 stages complete")),Is.True);
            yield return ClickButton("Continuecompletion");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.text.Contains("Next step in 4 levels")),Is.True);
            Assert.That(game.IsContentOpen(game.Catalog.GetPrimary("C02")),Is.True);Assert.That(game.IsContentOpen(future),Is.False);
            game.SendMessage("StartContent",first);yield return PlayDescentStage();yield return ClickButton("Nextdelivery");
            yield return PlayDescentStage();yield return ClickButton("Nextdelivery");yield return PlayDescentStage();
            Assert.That(game.LastCompletion.Coins,Is.Zero);Assert.That(game.LastCompletion.Lights,Is.Zero);Assert.That(game.JourneyClearCount,Is.EqualTo(1));
            Assert.That(new ProfileStore(game.ProfilePath).Load().Profile.Clears.Single().BestStars,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator ThreeCompletedLevelsCelebrateStatsThenOneGiftThenMapAndResumeWithoutAnotherGrant()
        {
            game.ReducedMotion=true;
            for(int n=1;n<=3;n++){
                game.SendMessage("StartContent",game.Catalog.GetPrimary("C0"+n));
                yield return PlayDescentStage();yield return ClickButton("Nextdelivery");yield return PlayDescentStage();
                yield return ClickButton("Nextdelivery");yield return PlayDescentStage();
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Win));
                if(n<3)yield return ClickButton("Continuecompletion");
            }
            Assert.That(game.LastCompletion.ParcelOrder,Is.EqualTo(1));yield return ClickButton("Continuecompletion");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Parcel));var saved=new ProfileStore(game.ProfilePath).Load().Profile;
            Assert.That(saved.Completion.StatsSeen,Is.True);Assert.That(saved.Rewards.Parcels.Single().Claimed,Is.False);
            yield return ClickButton("Unwrapgift");saved=new ProfileStore(game.ProfilePath).Load().Profile;
            Assert.That(saved.Rewards.Fragments("leaf"),Is.EqualTo(1));Assert.That(saved.Completion.Acknowledged,Is.False);
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Parcel));Assert.That(game.LastCompletion.Coins,Is.EqualTo(40));
            yield return ClickButton("Keepgift");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
            saved=new ProfileStore(game.ProfilePath).Load().Profile;Assert.That(saved.Completion.Acknowledged,Is.True);
            Assert.That(saved.Rewards.Parcels.Count,Is.EqualTo(1));Assert.That(saved.Rewards.Fragments("leaf"),Is.EqualTo(1));Assert.That(saved.Clears.Count,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator DescentCheckpointSurvivesFocusInterruptionAndRetryWithoutGrantingAClear()
        {
            game.StartDescentLevel(game.Catalog.GetPrimary("C81").Definition);game.ShowGame();game.ReducedMotion=false;
            Assert.That(game.Interaction.CanPlay,Is.False);Assert.That(game.Interaction.PointerDown(909,0),Is.False);
            yield return PlayDescentStage();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Checkpoint));
            var saved=new ProfileStore(game.ProfilePath).Load().Profile;Assert.That(saved.Clears,Is.Empty);Assert.That(saved.Rewards.Records,Is.Empty);
            var delivered=game.Interaction.Attempt.Current.Row.ToArray();yield return ClickButton("Nextdelivery");
            Assert.That(game.Descent.StageIndex,Is.EqualTo(1));Assert.That(game.Interaction.Attempt.Level.InitialSymbolIds,Is.EqualTo(delivered));
            game.SendMessage("OnApplicationFocus",false);yield return null;
            string current=game.Interaction.Attempt.CurrentStateHash;
            Assert.That(game.IsDescending,Is.False);Assert.That(UnityEngine.Object.FindObjectsByType<DescentGhost>(FindObjectsSortMode.None),Is.Empty);
            saved=new ProfileStore(game.ProfilePath).Load().Profile;Assert.That(saved.Descent.StageIndex,Is.EqualTo(1));
            Assert.That(saved.RestoreAttempt(game.Catalog).CurrentStateHash,Is.EqualTo(current));Assert.That(saved.Clears,Is.Empty);
            game.SendMessage("OnApplicationFocus",true);yield return AwaitDescentReady();
            string pin=game.Interaction.Attempt.PinnedContentHash;
            game.Interaction.Execute(PuzzleCommand.Drop);yield return AwaitPresentation();game.Interaction.Execute(PuzzleCommand.Drop);yield return AwaitPresentation();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Fail));yield return ClickButton("Retry");
            Assert.That(game.Descent.StageIndex,Is.EqualTo(1));Assert.That(game.Interaction.Attempt.PinnedContentHash,Is.EqualTo(pin));
            Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);Assert.That(new ProfileStore(game.ProfilePath).Load().Profile.Clears,Is.Empty);
        }
        [UnityTest] public IEnumerator ThreeStagesUseRealInputAndOnlyTheFinalReceiverPublishesTheWholeDelivery()
        {
            game.ReducedMotion=true;game.StartDescentLevel(game.Catalog.GetPrimary("C81").Definition);game.ShowGame();
            yield return PlayDescentStage();yield return ClickButton("Nextdelivery");
            Assert.That(game.Descent.StageIndex,Is.EqualTo(1));Assert.That(game.IsDescending,Is.False);game.GoLower();Assert.That(game.Descent.StageIndex,Is.EqualTo(1));
            yield return AwaitDescentReady();yield return ClickButton("Landing movement");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Devices));
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.text=="Wind on landing"),Is.True);game.CloseMenu();
            yield return PlayDescentStage();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Checkpoint));
            Assert.That(new ProfileStore(game.ProfilePath).Load().Profile.Clears,Is.Empty);yield return ClickButton("Nextdelivery");
            yield return PlayDescentStage();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Win));
            var saved=new ProfileStore(game.ProfilePath).Load().Profile;
            Assert.That(saved.Clears.Count(c=>c.CampaignNodeId=="C81"),Is.EqualTo(1));
            var record=saved.Rewards.Records.Single();Assert.That(record.BestRoute,Is.EqualTo(game.Descent.Stages.Sum(s=>s.Minimum)));
            Assert.That(record.ContentHash,Is.EqualTo(game.Descent.ContentHash));Assert.That(saved.Descent.StageIndex,Is.EqualTo(2));
            Assert.That(record.StageCount,Is.EqualTo(3));Assert.That(record.SourceHash,Is.EqualTo(game.Catalog.GetPrimary("C81").Definition.ContentHash));
            Assert.That(saved.RestoreAttempt(game.Catalog).CurrentStateHash,Is.EqualTo(game.Interaction.Attempt.CurrentStateHash));
            yield return null;Assert.That(UnityEngine.Object.FindObjectsByType<DescentGhost>(FindObjectsSortMode.None),Is.Empty);
            game.ShowRecords();yield return null;
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.text=="Level 81 · 3 stages"),Is.True);
            yield return ClickButton("Playthislevel");yield return AwaitDescentReady();
            Assert.That(game.Descent.StageIndex,Is.Zero);Assert.That(new ProfileStore(game.ProfilePath).Load().Profile.Clears.Count(c=>c.CampaignNodeId=="C81"),Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator ToolReactionFreezesExpiresAndRespectsQuietFeedbackWithoutChangingState()
        {
            foreach(bool quiet in new[]{false,true}){
                game.LoadNode("C18");game.ShowGame();game.ReducedMotion=quiet;game.Feedback.Quality=quiet?0:2;game.Preferences.EffectQuality=quiet?0:2;yield return AwaitPreparation();
                var original=game.GetComponentsInChildren<UnityEngine.UI.Image>().First(i=>i.name=="Recognizable device");var home=original.transform.parent;
                game.Interaction.Execute(PuzzleCommand.Swap(0));double until=Time.realtimeSinceStartupAsDouble+30;
                while(!game.ReactionBanner.IsShowing&&game.Interaction.IsPresenting&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                Assert.That(game.ReactionBanner.IsShowing,Is.True);Assert.That(game.ReactionBanner.Message,Does.Contain("mended"));
                var cloth=game.ReactionBanner.GetComponentsInChildren<CanvasGroup>().First(c=>c.name=="Woven announcement");
                var focus=game.ReactionBanner.transform.Find("Reaction focus");
                Assert.That(focus.GetChild(0).gameObject,Is.SameAs(original.gameObject));Assert.That(game.ReactionBanner.Source,Is.SameAs(original.rectTransform));
                Assert.That(home.gameObject.activeInHierarchy,Is.False,"The source dock is empty, with no duplicate or dangling cord.");
                Assert.That(focus.childCount,Is.EqualTo(1));Assert.That(focus.GetComponentsInChildren<ToolSuspensionRope>(),Is.Empty);
                Assert.That(focus.GetComponentsInChildren<UnityEngine.UI.Outline>(),Is.Empty);
                if(!quiet){Assert.That(cloth.alpha,Is.Zero);Assert.That(game.ReactionBanner.StatusVisible,Is.False);}
                string hash=game.Interaction.Attempt.CurrentStateHash;int length=game.Interaction.Attempt.RouteLength;
                game.Interaction.SetPaused(true);var age=game.ReactionBanner.Elapsed;var scale=game.ReactionBanner.transform.localScale;
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(game.ReactionBanner.Elapsed,Is.EqualTo(age));Assert.That(game.ReactionBanner.transform.localScale,Is.EqualTo(scale));
                if(quiet){Assert.That(game.ReactionBanner.GetComponentsInChildren<UnityEngine.UI.Image>().Count(i=>i.name.StartsWith("Reaction motif")),Is.Zero);Assert.That(game.Feedback.ActiveParticleCount,Is.Zero);Assert.That(scale,Is.EqualTo(Vector3.one));}
                game.Interaction.SetPaused(false);
                if(!quiet){
                    while(game.ReactionBanner.Elapsed<.50f)yield return null;
                    Assert.That(game.ReactionBanner.StatusVisible,Is.False);Assert.That(cloth.alpha,Is.Zero);
                    Assert.That(game.ReactionBanner.GetComponentsInChildren<UnityEngine.UI.Image>().Any(i=>i.name.StartsWith("Reaction motif")),Is.True);
                    while(game.ReactionBanner.Elapsed<1.10f)yield return null;
                    Assert.That(cloth.alpha,Is.GreaterThan(.9f));Assert.That(game.ReactionBanner.StatusVisible,Is.True);
                }
                yield return AwaitPresentation();
                until=Time.realtimeSinceStartupAsDouble+10;while(game.ReactionBanner.IsShowing&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                Assert.That(game.ReactionBanner.IsShowing,Is.False);Assert.That(original.transform.parent,Is.SameAs(home));
                Assert.That(original.gameObject.activeInHierarchy,Is.False,"Consumed tools do not reappear on the hanger.");Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(length));
                var status=game.GetComponentsInChildren<TextMeshProUGUI>().First(t=>t.name=="Feedback");Assert.That(status.enabled,Is.True);
                game.AnnounceReaction("Ribbon ready!","RibbonSpool");Assert.That(game.ReactionBanner.IsShowing,Is.True);
                game.ShowSettings();Assert.That(game.ReactionBanner.IsShowing,Is.False);game.ShowGame();Assert.That(status.enabled,Is.True);
                game.Undo();yield return null;Assert.That(original.gameObject.activeInHierarchy,Is.True);Assert.That(original.transform.parent,Is.SameAs(home));
            }
        }
        [UnityTest] public IEnumerator GustMovesDeckAndRopesMoreThanAmbientWithoutChangingThePuzzle()
        {
            game.LoadNode("C18");game.ShowGame();game.ReducedMotion=false;yield return AwaitPreparation();
            string hash=game.Interaction.Attempt.CurrentStateHash;float ambient=0,gust=0;
            var rope=game.Machine.GetComponentsInChildren<LineRenderer>().First(r=>r.name=="Left braided rope");
            double until=Time.realtimeSinceStartupAsDouble+3;
            while(Time.realtimeSinceStartupAsDouble<until){ambient=Mathf.Max(ambient,Mathf.Abs(game.Machine.FloorVisualOffset(0).x));yield return null;}
            var before=rope.GetPosition(12);float ropeTravel=0;
            game.Machine.CueWind(true);game.CueDeviceWind(true);until=Time.realtimeSinceStartupAsDouble+1.4;
            while(Time.realtimeSinceStartupAsDouble<until){gust=Mathf.Max(gust,Mathf.Abs(game.Machine.FloorVisualOffset(0).x));ropeTravel=Mathf.Max(ropeTravel,Vector3.Distance(before,rope.GetPosition(12)));yield return null;}
            Assert.That(gust,Is.GreaterThan(ambient*1.4f));Assert.That(ropeTravel,Is.GreaterThan(.02f));
            Assert.That(Mathf.Abs(game.Machine.FloorVisualAngle(0)),Is.LessThanOrEqualTo(2.4f));
            game.Interaction.SetPaused(true);var held=rope.GetPosition(12);var deck=game.Machine.FloorVisualOffset(0);
            yield return new WaitForSecondsRealtime(.15f);Assert.That(rope.GetPosition(12),Is.EqualTo(held));Assert.That(game.Machine.FloorVisualOffset(0),Is.EqualTo(deck));
            game.Interaction.SetPaused(false);game.ReducedMotion=true;yield return null;
            var quiet=rope.GetPosition(12);game.Machine.CueWind(false);yield return new WaitForSecondsRealtime(.15f);Assert.That(rope.GetPosition(12),Is.EqualTo(quiet));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
        }
        [UnityTest] public IEnumerator HangingDevicesFollowDeckWindAndFreezeWithoutSpendingMoves()
        {
            game.LoadNode("C13");game.ShowGame();game.ReducedMotion=false;yield return AwaitPreparation();game.RefreshLayout();
            string hash=game.Interaction.Attempt.CurrentStateHash;
            var device=(RectTransform)game.GetComponentsInChildren<UnityEngine.UI.Button>().First(b=>b.name=="DeviceBomb").transform;
            Assert.That(device.GetComponent<HangingToolTag>(),Is.Null);
            Assert.That(device.Find("Tool's carved crossbar"),Is.Null);Assert.That(device.Find("Tag hemp cords"),Is.Null);
            var mark=(RectTransform)device.Find("Recognizable device/Bomb painted mark");Assert.That(mark,Is.Not.Null);
            Canvas.ForceUpdateCanvases();var paintedPosition=mark.localPosition;
            var board=game.Machine.GetComponentsInChildren<MeshRenderer>().First(r=>r.name=="Wooden suspended board").transform;
            var start=device.position;var rotation=board.rotation;float maxTravel=0,maxTilt=0;
            double until=Time.realtimeSinceStartupAsDouble+2;
            while(Time.realtimeSinceStartupAsDouble<until) {
                maxTravel=Mathf.Max(maxTravel,Vector2.Distance(start,device.position));
                Assert.That(mark.localPosition,Is.EqualTo(paintedPosition),"Paint stays fixed on the swaying bomb body.");
                maxTilt=Mathf.Max(maxTilt,Quaternion.Angle(rotation,board.rotation));yield return null;
            }
            Assert.That(maxTravel,Is.GreaterThan(.002f));Assert.That(maxTilt,Is.GreaterThan(.02f));
            var ropes=game.Machine.GetComponentsInChildren<LineRenderer>().Where(r=>r.name.Contains("braided rope")).ToArray();
            var knots=game.Machine.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Rope knot").ToArray();
            foreach(var rope in ropes)
                Assert.That(knots.Min(k=>Vector2.Distance(k.transform.position,rope.GetPosition(24))),Is.LessThan(.002f));
            var cords=game.GetComponentsInChildren<ToolSuspensionRope>();
            Assert.That(cords.Length,Is.EqualTo(game.GetComponentsInChildren<UnityEngine.UI.Button>().Count(b=>b.name.StartsWith("Device"))));
            Assert.That(cords.All(c=>c.canvasRenderer.GetMesh().vertexCount>0),Is.True,"Each tool has one visible continuous rope.");
            game.Interaction.SetPaused(true);var held=device.position;var heldRotation=device.rotation;var heldBoard=board.position;var heldMark=mark.position;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(device.position,Is.EqualTo(held));Assert.That(device.rotation,Is.EqualTo(heldRotation));Assert.That(board.position,Is.EqualTo(heldBoard));
            Assert.That(mark.position,Is.EqualTo(heldMark));
            game.Interaction.SetPaused(false);game.ReducedMotion=true;game.RefreshLayout();yield return null;
            Assert.That(Quaternion.Angle(device.rotation,Quaternion.identity),Is.LessThan(.001f));
            var quiet=device.position;yield return new WaitForSecondsRealtime(.15f);Assert.That(device.position,Is.EqualTo(quiet));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);

            Assert.That(game.Catalog.TryGetLocalizedBinding("C13","ru",out var words),Is.True);
            game.LoadLevel(words.Definition,"ru");yield return AwaitPreparation();game.RefreshLayout();
            var letter=game.GetComponentsInChildren<TextMeshProUGUI>().First(t=>t.name=="Bomb painted letter");
            Assert.That(letter.text,Is.EqualTo(words.Definition.Floors[0].Bomb.RequiredSymbol.Value));
            Assert.That(letter.color.r,Is.GreaterThan(.95f));
            game.ShowSettings();yield return null;game.Preferences.HighContrast=true;game.SendMessage("ApplyPreferences");game.ShowGame();yield return null;
            Assert.That(letter.color.r,Is.GreaterThan(.95f),"Preference changes must not turn the painted contact dark.");
            game.Interaction.SetSuspended(false);game.Interaction.Execute(PuzzleCommand.Swap(0));yield return AwaitPresentation();
            Assert.That(game.Interaction.Attempt.Current.BombPhase,Is.EqualTo(BombPhase.Defused),"The symbol painted on the body still uses the canonical contact rule.");
            var safeIcon=letter.transform.parent.parent.GetComponent<UnityEngine.UI.Image>();Assert.That(safeIcon.sprite.name,Is.EqualTo("BombSafe"));
            Assert.That(safeIcon.transform.parent.Find("Contact outcome ribbon"),Is.Null,"The central reaction is the only outcome ribbon.");

            // Every tool keeps its painted symbol on the same body.
            game.LoadNode("C18");game.ReducedMotion=false;yield return AwaitPreparation();game.RefreshLayout();
            hash=game.Interaction.Attempt.CurrentStateHash;
            Assert.That(game.GetComponentsInChildren<HangingToolTag>(),Is.Empty);
            Assert.That(game.GetComponentsInChildren<ToolTagCordGraphic>(),Is.Empty);
            var paint=game.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Body painted symbol");
            var local=paint.localPosition;var startPaint=paint.position;float paintTravel=0;
            until=Time.realtimeSinceStartupAsDouble+2;
            while(Time.realtimeSinceStartupAsDouble<until){paintTravel=Mathf.Max(paintTravel,Vector3.Distance(startPaint,paint.position));Assert.That(paint.localPosition,Is.EqualTo(local));yield return null;}
            Assert.That(paintTravel,Is.GreaterThan(.002f));
            game.Interaction.SetPaused(true);var heldPaint=paint.position;yield return new WaitForSecondsRealtime(.1f);Assert.That(paint.position,Is.EqualTo(heldPaint));
            game.Interaction.SetPaused(false);game.ReducedMotion=true;yield return null;Assert.That(paint.localPosition,Is.EqualTo(local));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
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
                    for (int i = 0; i < game.SlotViews.Count; i++) {
                        var pose=game.Machine.TokenPose(i);
                        var support=pose==PiecePose.Falling||pose==PiecePose.Exchanging?Vector3.zero:game.Machine.TokenSupportOffset(i);
                        Assert.That(Vector3.Distance(game.Machine.PresentedTokenPosition(i),game.Machine.TokenPosition(i)+support),Is.LessThan(.001f),"Reduced motion keeps the static wood support, without extra animated displacement.");
                    }
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
        [UnityTest] public IEnumerator BackgroundDepthAndFloatingIslandsStayInsideTheQuietFlightBand()
        {
            game.LoadNode("C02");yield return null;game.Machine.SetZone(0);
            var world=game.Machine.LivingWorld;var hash=game.Interaction.Attempt.CurrentStateHash;
            world.SetQuietFlight(false);world.Paint(0);var near=world.GliderScale;var island=world.IslandPosition(0);
            for(int i=0;i<32;i++) {
                world.Paint(i*Mathf.PI/(16*.08f));var bounds=world.GliderBounds;
                Assert.That(bounds.min.y,Is.GreaterThanOrEqualTo(world.FlightBand.x));
                Assert.That(bounds.max.y,Is.LessThanOrEqualTo(world.FlightBand.y));
            }
            world.Paint(Mathf.PI/.08f);Assert.That(world.GliderScale.y,Is.LessThan(near.y*.75f));
            Assert.That(world.IslandPosition(0),Is.Not.EqualTo(island));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator WorldScoresCrossfadeAndMutePreservesPlaybackPosition()
        {
            var library=Resources.Load<ToyAudioLibrary>("CascadeArt/ToyAudio");
            Assert.That(library.worldMusic.Length,Is.EqualTo(8));
            Assert.That(library.worldMusic.Distinct().Count(),Is.EqualTo(8));
            Assert.That(library.worldMusic.All(c=>c!=null&&c.channels==2&&c.length>50),Is.True);
            Assert.That(library.ScoreFor(GameScreen.Home,7),Is.SameAs(library.menuMusic));
            for(int i=0;i<8;i++)Assert.That(library.ScoreFor(GameScreen.JourneyMap,i),Is.SameAs(library.worldMusic[i]));
            game.ShowHome();game.Feedback.MusicEnabled=true;
            yield return new WaitForSecondsRealtime(1.8f);
            Assert.That(game.Feedback.CurrentMusicClip,Is.SameAs(library.menuMusic));
            game.ShowGame();game.Machine.SetZone(2);yield return null;
            Assert.That(game.Feedback.CurrentMusicClip,Is.SameAs(library.worldMusic[2]));
            Assert.That(game.Feedback.MusicCrossfade,Is.LessThan(1));
            yield return new WaitForSecondsRealtime(1.8f);
            Assert.That(game.Feedback.MusicCrossfade,Is.EqualTo(1));
            game.Feedback.MusicEnabled=false;yield return new WaitForSecondsRealtime(.45f);
            Assert.That(game.Feedback.MusicSleeping,Is.True);float position=game.Feedback.MusicPosition;
            yield return new WaitForSecondsRealtime(.15f);Assert.That(game.Feedback.MusicPosition,Is.EqualTo(position).Within(.03f));
            game.Feedback.MusicEnabled=true;yield return new WaitForSecondsRealtime(.2f);
            Assert.That(game.Feedback.MusicSleeping,Is.False);Assert.That(game.Feedback.MusicPosition,Is.GreaterThan(position));
        }
        [UnityTest] public IEnumerator WindKeepsItsPositionAcrossRegionsAndFadesBeforeSleeping()
        {
            game.LoadNode("C02");yield return null;game.Feedback.SoundEnabled=true;
            yield return new WaitForSecondsRealtime(.25f);
            var wind=game.Feedback.GetComponentsInChildren<AudioSource>().Single(s=>s.name=="Living island wind");
            Assert.That(wind.clip.name,Is.EqualTo("LivingWind"));Assert.That(wind.isPlaying,Is.True);
            var hash=game.Interaction.Attempt.CurrentStateHash;float before=wind.time;
            game.Machine.SetZone(1);yield return new WaitForSecondsRealtime(.12f);
            Assert.That(Mathf.Repeat(wind.time-before,wind.clip.length),Is.GreaterThan(.04f));
            game.Feedback.SoundEnabled=false;yield return new WaitForSecondsRealtime(.24f);
            Assert.That(wind.volume,Is.Zero);Assert.That(wind.isPlaying,Is.False);
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
            yield return ClickButton("PuzzleLocalePrevious");Assert.That(game.Preferences.PuzzleLocale,Is.EqualTo("en"));
            yield return ClickButton("PuzzleLocaleuk");Assert.That(game.ProposedPuzzleLocale,Is.EqualTo("uk"));
            yield return ClickButton("Keepmychoice");Assert.That(game.Preferences.PuzzleLocale,Is.EqualTo("en"));
            yield return ClickButton("UiLocaleNext");Assert.That(game.UiLanguage,Is.EqualTo("ru"));
            yield return ClickButton("PuzzleLocaleNext");yield return ClickButton("PuzzleLocaleru");yield return ClickButton("Usefornewpuzzles");
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
            game.ShowJourney();game.TurnJourneyMap(1);yield return null;game.TurnJourneyMap(1);yield return null;game.TurnJourneyMap(1);yield return null;
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
            game.LoadNode("C02");game.Coach.Advance(6,true);float idle=game.Coach.IdleSeconds;string hash=game.Interaction.Attempt.CurrentStateHash;
            game.ShowHome();yield return new WaitForSecondsRealtime(.08f);
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That(game.Interaction.CanPlay,Is.False);
            Assert.That(game.Coach.IdleSeconds,Is.EqualTo(idle));
            Assert.That(game.GetComponentsInChildren<UnityEngine.UI.Button>().Select(b=>b.name),Is.EquivalentTo(new[]{"Startgame","Shop","Settings","Removeads"}));
            yield return ClickButton("Settings");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Settings));
            yield return ClickButton("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            yield return ClickButton("Shop");yield return ClickButton("Keepsakes");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Collection));
            yield return ClickButton("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Shop));
            yield return ClickButton("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            game.ShowGame();Assert.That(game.Interaction.CanPlay,Is.True);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator HomeWordsChoiceStartsAnExistingPinnedBinding()
        {
            game.ShowHome();yield return ClickButton("Startgame");yield return ClickButton("Words");yield return ClickButton("1");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));Assert.That(game.Interaction.Attempt.Mode,Is.EqualTo(PuzzleMode.Words));
            Assert.That(game.Interaction.Attempt.Current.Row.All(s=>s.Kind==SymbolKind.Glyph),Is.True);
            game.ShowLessons();yield return ClickButton("2");Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo("C02"));
            var before=game.Interaction.Attempt.Current;game.SelectMode(PuzzleMode.Shapes);Assert.That(game.Interaction.Attempt.Current,Is.SameAs(before));
        }
        [UnityTest] public IEnumerator StoryOpeningLeadsToMapAndKeepsPuzzleStateUntouched()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;game.ShowHome();yield return ClickButton("Startgame");yield return ClickButton("Storymode");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Prologue));
            for(int i=0;i<3;i++)yield return ClickButton("Continueopening");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(game.IsStoryRoute,Is.True);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.JourneyClearCount,Is.Zero);
            yield return ClickButton("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.PlayChoice));
            yield return ClickButton("Storymode");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
            yield return ClickButton("Story");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Dialogue));
            yield return ClickButton("Skipstory");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
        }
        [UnityTest] public IEnumerator LevelShelfSwipeDoesNotClickAPuzzleAndAllModesAreExact()
        {
            game.ChooseSoloMode(PuzzleMode.Words);yield return null;var button=game.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="5");
            var rect=(RectTransform)button.transform;Vector2 start=RectTransformUtility.WorldToScreenPoint(Camera.main,rect.TransformPoint(rect.rect.center));
            yield return MouseFrame(start,true);yield return MouseFrame(start+Vector2.left*35,true);yield return MouseFrame(start+Vector2.left*110,true);yield return MouseFrame(start+Vector2.left*110,false);
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Lessons));Assert.That(game.LevelShelfPage,Is.EqualTo(1));Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
            yield return ClickButton("10");Assert.That(game.Interaction.Attempt.Mode,Is.EqualTo(PuzzleMode.Words));Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo("C10"));
            foreach(var mode in new[]{PuzzleMode.Shapes,PuzzleMode.Mixed}){
                game.ChooseSoloMode(mode);yield return null;var pick=game.GetComponentsInChildren<UnityEngine.UI.Button>().First(b=>int.TryParse(b.name,out _));
                yield return ClickButton(pick.name);Assert.That(game.Interaction.Attempt.Mode,Is.EqualTo(mode));
            }
        }
        [UnityTest] public IEnumerator MapParallaxAndNoAdsNavigationNeverChangePuzzleState()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;game.ShowJourney();yield return null;
            var world=game.GetComponentInChildren<MapParallax>();Assert.That(world,Is.Not.Null);
            game.ReducedMotion=true;yield return null;Assert.That(world.Offset,Is.EqualTo(Vector2.zero));
            var hero=game.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="TalktoMika");var hit=(RectTransform)hero.transform;
            // Exercise the invisible margin, beyond the small portrait, through real input.
            var margin=RectTransformUtility.WorldToScreenPoint(Camera.main,hit.TransformPoint(new Vector2(hit.rect.xMin+2,hit.rect.center.y)));
            yield return MouseFrame(margin,true);yield return MouseFrame(margin,false);Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Dialogue));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            game.ShowHome();yield return ClickButton("Removeads");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.NoAds));
            yield return ClickButton("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.JourneyClearCount,Is.Zero);
        }
        [UnityTest] public IEnumerator OutcomeCeremoniesKeepControlsBackAndInterruptionSettlesBothResults()
        {
            foreach(bool success in new[]{true,false})foreach(bool reduced in new[]{false,true}){
                game.LoadNode("C01");game.ShowGame();game.ReducedMotion=reduced;yield return AwaitPreparation();
                game.Interaction.Execute(success?PuzzleCommand.Swap(0):PuzzleCommand.Drop);
                string hash=game.Interaction.Attempt.CurrentStateHash;int moves=game.Interaction.Attempt.RouteLength;
                double started=Time.realtimeSinceStartupAsDouble;
                while(!game.Outcome.Running&&game.Interaction.IsPresenting&&PresentationBudget(started))yield return null;
                Assert.That(game.Outcome.Running,Is.True);Assert.That(game.Outcome.Success,Is.EqualTo(success));
                Assert.That(game.GetComponentsInChildren<UnityEngine.UI.Button>().Any(b=>b.name=="Retry"),Is.False);
                game.SendMessage("OnApplicationFocus",false);yield return null;
                Assert.That(game.Outcome.Running,Is.False);Assert.That(game.Interaction.IsPresenting,Is.False);
                Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(moves));
                game.SendMessage("OnApplicationFocus",true);yield return null;
                Assert.That(game.CurrentScreen,Is.EqualTo(success?GameScreen.Win:GameScreen.Fail));AssertResultControlsClearReceiver();
                game.ShowHowToPlay();yield return null;
                Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.text=="How to play"),Is.True);
                game.CloseMenu();yield return null;Assert.That(game.Outcome.Running,Is.False);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            }
        }
        [UnityTest] public IEnumerator WearDeepensTheSupportedDeckAndReducedMotionKeepsItsStaticWeight()
        {
            var source=game.Catalog.GetPrimary("C03").Definition;
            var floors=source.Floors.Select((f,i)=>i==0?new FloorDefinition(4,f.OpenEdges):f).ToArray();
            game.LoadLevel(new LevelDefinition(source.PuzzleId,source.CampaignNodeId,source.SchemaVersion,source.RulesVersion,"presentation-weight-fixture",source.InitialSymbolIds,source.TargetSymbolIds,source.TargetSegments,floors));
            game.ShowGame();game.ReducedMotion=false;yield return AwaitPreparation();yield return new WaitForSecondsRealtime(.5f);
            float fresh=game.Machine.LoadedSag;
            var board=game.Machine.GetComponentsInChildren<MeshRenderer>().First(r=>r.name=="Wooden suspended board").transform;
            var vertices=board.GetComponent<MeshFilter>().sharedMesh.vertices;
            float thickness=Vector2.Distance(Camera.main.WorldToScreenPoint(board.TransformPoint(vertices[0])),Camera.main.WorldToScreenPoint(board.TransformPoint(vertices[1])));
            Assert.That(thickness,Is.GreaterThan(4),"The rendered wood must retain visible thickness even with an expanded culling bound.");
            for(int i=0;i<3;i++){game.Interaction.Execute(PuzzleCommand.Swap(0));yield return AwaitPresentation();}
            yield return new WaitForSecondsRealtime(.5f);Assert.That(game.Machine.LoadedSag,Is.GreaterThan(fresh+.07f));
            string hash=game.Interaction.Attempt.CurrentStateHash;game.ReducedMotion=true;yield return null;
            Assert.That(game.Machine.LoadedSag,Is.GreaterThan(.20f));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            game.Interaction.Execute(PuzzleCommand.Swap(0));yield return AwaitPresentation();
            Assert.That(game.Feedback.ActiveParticleCount,Is.LessThanOrEqualTo(32));
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
            game.LoadNode("C03");yield return AwaitPreparation();game.Interaction.Execute(PuzzleCommand.Swap(0));var committed=game.Interaction.Attempt.Current;
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
            game.LoadNode("C02");yield return AwaitPreparation();game.Interaction.Execute(PuzzleCommand.Swap(0));yield return AwaitPresentation();
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
        IEnumerator AwaitPreparation()
        {
            double started=Time.realtimeSinceStartupAsDouble;
            while(!game.PreparationReady && Time.realtimeSinceStartupAsDouble-started<30)yield return null;
            Assert.That(game.PreparationReady,Is.True,"Final level must be proven before input opens");
        }
        [UnityTest] public IEnumerator ReceiverGlyphsAndPreparedRoutesRemainAlignedAndPlayable()
        {
            game.ReducedMotion=true;
            foreach(string locale in new[]{"en","ru","uk"}) {
                Assert.That(game.Catalog.TryGetLocalizedBinding("C38",locale,out var binding),Is.True);
                game.LoadLevel(binding.Definition,locale);game.ShowGame();yield return AwaitPreparation();game.RefreshLayout();yield return null;
                var receiver=(RectTransform)game.TargetRow.parent;
                var center=receiver.InverseTransformPoint(game.TargetRow.TransformPoint(game.TargetRow.rect.center));
                Assert.That(center.x,Is.EqualTo(receiver.rect.center.x).Within(.01));
                Assert.That(center.y,Is.EqualTo(receiver.rect.yMin+receiver.rect.height*.53f).Within(.01));
                for(int i=0;i<game.SlotViews.Count;i++) {
                    var face=game.SlotViews[i].GetComponentInChildren<TextMeshProUGUI>();
                    // Refresh must be correct immediately, even without a following LateUpdate.
                    game.RefreshLayout();
                    Assert.That(Vector2.Distance(face.transform.position,game.Machine.TokenVisualPosition(i)),Is.LessThan(.001f));
                }
                foreach(var label in game.TargetRow.GetComponentsInChildren<TextMeshProUGUI>()
                    .Concat(game.SlotViews.Select(s=>s.GetComponentInChildren<TextMeshProUGUI>()))) {
                    label.ForceMeshUpdate();var min=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var max=-min;
                    foreach(var c in label.textInfo.characterInfo.Take(label.textInfo.characterCount).Where(c=>c.isVisible))
                        for(int v=c.vertexIndex;v<c.vertexIndex+4;v++) {
                            var point=label.textInfo.meshInfo[c.materialReferenceIndex].vertices[v];
                            min=Vector2.Min(min,point);max=Vector2.Max(max,point);
                        }
                    Assert.That(Vector2.Distance((min+max)*.5f,label.rectTransform.rect.center),Is.LessThan(.01f),locale+" "+label.text);
                }
            }
            foreach(var content in new[]{game.Catalog.GetPrimary("C81"),game.Catalog.GetPrimary("C88"),game.Catalog.BonusLevels[8],game.Catalog.BonusLevels[9]}) {
                game.LoadLevel(content.Definition);game.ShowGame();yield return AwaitPreparation();game.RefreshLayout();
                var attempt=game.Interaction.Attempt;var proof=PuzzleSolver.FindShortest(attempt.Resolver,goal:attempt.Goal);
                Assert.That(game.MinimumMoves,Is.EqualTo(proof.OptimalActions));
                foreach(var command in proof.Path) {
                    var expected=attempt.Predict(command).State;int before=attempt.RouteLength;
                    if(command.Kind==CommandKind.EarlyDrop)yield return ClickButton("Drop");
                    else {
                        int end=command.Kind==CommandKind.LongReach?command.End:command.Edge+1;
                        yield return MouseFrame(Point(command.Edge),true);yield return MouseFrame(Point(end),true);yield return MouseFrame(Point(end),false);
                    }
                    Assert.That(attempt.RouteLength,Is.EqualTo(before+1));Assert.That(attempt.Current,Is.EqualTo(expected));
                    yield return AwaitPresentation();
                }
                Assert.That(attempt.Goal.IsSatisfied(attempt.Current),Is.True);
                Assert.That(attempt.RouteLength,Is.EqualTo(game.MinimumMoves));
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
