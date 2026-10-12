using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class GuidedLessonFlowTests
    {
        const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        CascadeGame game;
        string directory;
        bool oldRunInBackground;

        [UnitySetUp] public IEnumerator SetUp()
        {
            oldRunInBackground = UnityEngine.Application.runInBackground;
            UnityEngine.Application.runInBackground = true;
            directory = Path.Combine(Path.GetTempPath(), "CascadeGuidedLessonFlow", Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory = directory;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game = UnityEngine.Object.FindFirstObjectByType<CascadeGame>(); Assert.That(game, Is.Not.Null);
            game.SendMessage("OnApplicationFocus", true); game.SendMessage("OnApplicationPause", false);
            game.Preferences.ReducedMotion = true; game.ReducedMotion = true;
            game.Feedback.MusicEnabled = game.Feedback.SoundEnabled = false;
            Profile.SeenMechanics = Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            Profile.SeenCoachPrompts.Add("controls-two-ways-v1");Profile.SeenCoachPrompts.Add("controls-undo-v1");
            foreach(var lesson in Profile.SeenMechanics)Profile.SeenCoachPrompts.Add("triad-guide-v6-"+(int)lesson);
            game.ShowGame();
            yield return Await(() => game.PreparationReady && game.Interaction.CanPlay, "Initial puzzle must become playable");
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if (game != null) { UnityEngine.Object.Destroy(game.gameObject); yield return null; }
            CascadeGame.EditorProfileDirectory = null;
            UnityEngine.Application.runInBackground = oldRunInBackground;
            if (directory != null && Directory.Exists(directory)) Directory.Delete(directory, true);
        }
        PlayerProfile Profile => Read<PlayerProfile>("profile");
        [UnityTest] public IEnumerator FirstStageTeachesUndoBeforeIndependentStages()
        {
            Profile.SeenMechanics.Remove(MechanicLesson.Undo);Profile.SeenCoachPrompts.Remove("controls-undo-v1");
            game.StartCampaignLevel(game.Catalog.GetPrimary("C01").Definition);game.ShowGame();
            yield return Await(()=>game.Interaction.CanPlay,"First exchange becomes playable");
            game.Interaction.Execute(Cascade.Solver.PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path[0]);
            yield return Await(()=>game.CurrentScreen==GameScreen.Playing&&game.CurrentMechanic==MechanicLesson.Undo,"Undo is shown before the first stage ends");
            game.AcknowledgeMechanic();yield return Await(()=>game.IsTutorialHandVisible,"The hand must show the actual Undo");
            int moves=game.Interaction.Attempt.MovesMade,undos=game.Interaction.Attempt.UndoCount,remaining=game.Interaction.Attempt.FreeUndoRemaining;
            game.Undo();yield return Await(()=>!game.Interaction.IsPresenting&&game.CurrentScreen==GameScreen.Playing,"Undo restores the first stage");
            Assert.That(game.Interaction.Attempt.UndoCount,Is.EqualTo(undos));Assert.That(game.Interaction.Attempt.FreeUndoRemaining,Is.EqualTo(remaining));
            Assert.That(game.Interaction.Attempt.MovesMade,Is.EqualTo(moves-1),"The rehearsed exchange is not a rating penalty");
            Assert.That(game.Interaction.Attempt.Current.Row,Is.EqualTo(game.Descent.Level(game.Catalog.GetPrimary("C01").Definition,0).InitialSymbolIds));
            Assert.That(Profile.SeenCoachPrompts,Does.Contain("controls-undo-v1"));
        }
        [UnityTest] public IEnumerator PrimaryMechanicIsExplainedInStageOneAndLaterStagesAreIndependent()
        {
            Profile.SeenCoachPrompts.Remove("triad-guide-v6-"+(int)MechanicLesson.Repair);
            var source=game.Catalog.GetPrimary("C02").Definition;game.StartCampaignLevel(source);game.ShowGame();
            yield return Await(()=>game.CurrentMechanic==MechanicLesson.Repair,"Repair should be explained before playing");
            Assert.That(game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Any(t=>t.name=="What this mechanic changes"&&!string.IsNullOrWhiteSpace(t.text)),Is.True);
            game.AcknowledgeMechanic();yield return Await(()=>game.Interaction.PracticeCommand.HasValue,"A repair-producing exchange must be shown");
            var first=game.Interaction.PracticeCommand.Value;var transition=game.Interaction.Execute(first);
            Assert.That(transition.Accepted,Is.True);Assert.That(transition.Events.Any(e=>e.Kind==PuzzleEventKind.Repaired),Is.True);
            yield return Await(()=>game.Interaction.CanControl,"Repair should finish its visible reaction");
            Assert.That(Profile.SeenCoachPrompts,Does.Contain("triad-guide-v6-"+(int)MechanicLesson.Repair));
            var proof=Cascade.Solver.PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver,game.Interaction.Attempt.Current);
            foreach(var command in proof.Path){Assert.That(game.Interaction.Execute(command).Accepted,Is.True);yield return Await(()=>game.Interaction.CanControl,"Stage one exchange settles");}
            Write("journeyActive",false);game.GoLower();
            yield return Await(()=>game.Descent.StageIndex==1&&game.Interaction.CanPlay,"Stage two must be independently playable");
            Assert.That(game.MechanicPopupOpen||game.PendingLesson.HasValue,Is.False);
            foreach(var command in Cascade.Solver.PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path){
                Assert.That(game.Interaction.Execute(command).Accepted,Is.True);yield return Await(()=>game.Interaction.CanControl,"Stage two exchange settles");
            }
            game.GoLower();yield return Await(()=>game.Descent.StageIndex==2&&game.Interaction.CanPlay,"Stage three must be independently playable");
            Assert.That(game.MechanicPopupOpen||game.PendingLesson.HasValue,Is.False);
        }
        [UnityTest] public IEnumerator CompletionOffersNextAndMapAndNextStartsTheActualOpenNode()
        {
            game.Preferences.Tutorials=false;game.LoadNode("C01");game.ShowGame();Write("journeyActive",true);
            yield return Await(()=>game.Interaction.CanPlay,"Completion fixture starts");
            foreach(var command in Cascade.Solver.PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path){
                game.Interaction.Execute(command);yield return Await(()=>!game.Interaction.IsPresenting,"Completion exchange settles");
            }
            yield return Await(()=>game.CurrentScreen==GameScreen.Win,"Result must open");
            var buttons=game.GetComponentsInChildren<Button>();
            Assert.That(buttons.Any(b=>b.name=="Next completed level"&&b.interactable),Is.True);
            Assert.That(buttons.Any(b=>b.name=="Completion map"&&b.interactable),Is.True);
            game.ContinueToNextLevel();string destination=game.LastCompletion.NextNode;
            Assert.That(destination,Is.Not.Null.And.Not.EqualTo("C01"));
            for(int step=0;step<8&&game.ConversationOpen;step++){game.SkipConversation();yield return null;}
            yield return Await(()=>game.PreparationReady&&game.Descent?.Version==6&&game.Interaction.Attempt.Level.CampaignNodeId==destination,"Next enters the saved destination");
            var heading=game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single(t=>t.name=="Level");
            Assert.That(heading.text,Does.Contain("Level 2"),"Next level uses the selected mode's visible sequence");
            Assert.That(Profile.Clears.Count(c=>c.CampaignNodeId=="C01"),Is.EqualTo(1),"Moving to the next level cannot grant the clear twice");
        }
        T Read<T>(string name) => (T)typeof(CascadeGame).GetField(name, PrivateInstance).GetValue(game);
        void Write(string name, object value) => typeof(CascadeGame).GetField(name, PrivateInstance).SetValue(game, value);
        void Invoke(string name, params object[] arguments) => typeof(CascadeGame).GetMethod(name, PrivateInstance).Invoke(game, arguments);
        static SymbolId[] Row(string value) => value.Select(c => SymbolId.Parse("s:" + c)).ToArray();
        [UnityTest] public IEnumerator ProgressedSaveDoesNotRestartOpeningGestureLesson()
        {
            Profile.SeenMechanics.Remove(MechanicLesson.Swap);Profile.SeenCoachPrompts.Remove("controls-two-ways-v1");
            var opening=game.Catalog.GetPrimary("C01").Definition;
            Profile.Clears.Add(new ClearReceipt{CampaignNodeId="C01",ContentHash=opening.ContentHash,AttemptId=Guid.NewGuid().ToString("D"),BestStars=3});
            game.StartCampaignLevel(game.Catalog.PrimaryLevels.Skip(5).First().Definition);game.ShowGame();
            yield return Await(()=>game.PreparationReady&&game.Interaction.CanPlay,"A progressed save must load the sixth puzzle without opening controls");
            Assert.That(game.CurrentMechanic,Is.Not.EqualTo(MechanicLesson.Swap));
            Assert.That((bool)typeof(CascadeGame).GetMethod("NeedsControlLesson",PrivateInstance).Invoke(game,new object[]{MechanicLesson.Swap}),Is.False);
            game.StartCampaignLevel(opening);game.ShowGame();
            yield return Await(()=>game.CurrentMechanic==MechanicLesson.Swap,"Replaying the opening can still show the newer two-way controls");
        }
        [UnityTest] public IEnumerator SixthIslandLevelUsesItsOwnTriadAndCompletesIntoSeventh()
        {
            game.Preferences.Tutorials=false;var nodes=game.Catalog.PrimaryLevels.Take(7).ToArray();
            foreach(var content in nodes.Take(5))Profile.Clears.Add(new ClearReceipt{CampaignNodeId=content.Definition.CampaignNodeId,
                ContentHash=content.Definition.ContentHash,AttemptId=Guid.NewGuid().ToString("D"),BestStars=3});
            Profile.Rewards.Synchronize(nodes.Take(5).Select(c=>c.Definition.CampaignNodeId));
            Read<System.Collections.Generic.HashSet<string>>("sessionClears").UnionWith(nodes.Take(5).Select(c=>c.Definition.CampaignNodeId));
            game.ShowJourney();if(game.CurrentScreen==GameScreen.Prologue)game.FinishWorldComic();
            for(int i=0;i<20&&game.ConversationOpen;i++){game.SkipConversation();yield return null;}
            game.OpenJourneyIsland(0);game.ReplayJourneyLevel(5);
            for(int i=0;i<20&&game.ConversationOpen;i++){game.SkipConversation();yield return null;}
            yield return Await(()=>game.PreparationReady&&game.Descent?.Version==6&&game.Interaction.CanPlay,"Sixth level must finish loading its own published triad");
            string sixth=nodes[5].Definition.CampaignNodeId,seventh=nodes[6].Definition.CampaignNodeId;
            Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo(sixth));Assert.That(game.IsJourneyAttempt,Is.True);
            Assert.That((MechanicLesson)game.Descent.Stages[0].Triad.Lesson,Is.EqualTo(MechanicLesson.Bridge));
            for(int stage=0;stage<3;stage++){
                foreach(var command in Cascade.Solver.PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path){
                    Assert.That(game.Interaction.Execute(command)?.Accepted,Is.True);yield return Await(()=>game.Interaction.CanControl,"Each sixth-level exchange must settle");
                }
                if(stage<2){
                    for(int i=0;i<20&&game.Descent.StageIndex==stage;i++){game.GoLower();if(game.ConversationOpen)game.SkipConversation();yield return null;}
                    yield return Await(()=>game.Descent.StageIndex==stage+1&&game.Interaction.CanPlay,"The selected level must continue through its three stages");
                    Assert.That(game.IsJourneyAttempt,Is.True);
                }
            }
            Assert.That(Profile.Clears.Any(c=>c.CampaignNodeId==sixth),Is.True);Assert.That(game.LastCompletion.NodeId,Is.EqualTo(sixth));
            game.ContinueToNextLevel();
            if(game.CurrentScreen==GameScreen.Parcel){
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Openparcelgift").onClick.Invoke();yield return null;
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Claimgift").onClick.Invoke();yield return null;
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Keepgift").onClick.Invoke();yield return null;
            }
            for(int i=0;i<20&&game.ConversationOpen;i++){game.SkipConversation();yield return null;}
            yield return Await(()=>game.PreparationReady&&game.Interaction.CanPlay&&game.Interaction.Attempt.Level.CampaignNodeId==seventh,"Next must load level seven, never level one");
            Assert.That(game.IsJourneyAttempt,Is.True);
        }
        [UnityTest] public IEnumerator BagExpandsInlineAndAppliesItsSelectedIconExactlyOnce()
        {
            game.Preferences.Tutorials=false;game.ReducedMotion=false;yield return Mount(Fixture("CBA"));
            var economy=(AdService)typeof(CascadeGame).GetProperty("GameEconomy",PrivateInstance).GetValue(game);
            economy.Ledger.WayfinderScrolls=economy.Ledger.UndoSpools=economy.Ledger.ReturnHourglasses=economy.Ledger.SignalLanterns=economy.Ledger.LandingLenses=1;
            game.ShowGame();var receiver=game.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Receiver");
            float bottom=receiver.position.y;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Field tools").onClick.Invoke();
            yield return new WaitForSecondsRealtime(.45f);
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));Assert.That(game.FieldToolTrayOpen,Is.True);
            Assert.That(receiver.position.y,Is.GreaterThan(bottom));
            Assert.That(game.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Field tool ")),Is.EqualTo(6));
            game.ToggleFieldToolTray();yield return new WaitForSecondsRealtime(.25f);Assert.That(game.FieldToolTrayOpen,Is.False);
            game.ToggleFieldToolTray();yield return new WaitForSecondsRealtime(.45f);
            var before=game.Interaction.Attempt.Current.Row.ToArray();
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Field tool 3").onClick.Invoke();
            yield return Await(()=>game.Stock(TravelTool.SignalLantern)==0&&game.Interaction.IsPresenting,"The selected lantern must commit one use and start its appearance");
            string committed=game.Interaction.Attempt.CurrentStateHash;
            Assert.That(game.ReactionBanner.Source.name,Is.EqualTo("Field tool icon 3"));
            game.UseShopTool(TravelTool.SignalLantern);Assert.That(game.Stock(TravelTool.SignalLantern),Is.Zero);
            yield return new WaitForSecondsRealtime(.12f);Assert.That(game.VisibleFrame.Row,Is.EqualTo(before),"The selected icon appears before the committed move is animated");
            yield return Await(()=>game.Interaction.CanPlay,"The lantern move must finish and release touch");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(committed));Assert.That(game.FieldToolTrayOpen,Is.False);
            game.ToggleFieldToolTray();yield return new WaitForSecondsRealtime(.45f);
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Field tool 0").onClick.Invoke();
            yield return Await(()=>game.Stock(TravelTool.Wayfinder)==0&&game.Interaction.IsPresenting,"The scroll must begin its appearance");
            game.ShowHome();game.ShowGame();yield return Await(()=>game.Interaction.CanPlay,"Navigation during appearance must release its presentation gate");
            Assert.That(game.Stock(TravelTool.Wayfinder),Is.Zero);Assert.That(game.FieldToolTrayOpen,Is.False);
        }
        static LevelDefinition Fixture(string start = "ABC", RepairDefinition repair = null) =>
            new LevelDefinition("guided-fixture", "guided-fixture", "1", "2.0", "guided-fixture",
                Row(start), Row("ABC"), new[] { new TargetSegment(SegmentKind.Shapes, 0, 3) },
                new[] { new FloorDefinition(8, new[] { true, true }, repair: repair) });
        static IEnumerator Await(Func<bool> ready, string message)
        {
            double began = Time.realtimeSinceStartupAsDouble;
            while (!ready() && Time.realtimeSinceStartupAsDouble - began < 30) yield return null;
            var owner=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            string state=owner==null?"no game":$"screen={owner.CurrentScreen}, stage={owner.Descent?.StageIndex}, status={owner.Interaction.Attempt.Current.Status}, ready={owner.PreparationReady}, play={owner.Interaction.CanPlay}, control={owner.Interaction.CanControl}, preparing={owner.Interaction.IsPreparing}, presenting={owner.Interaction.IsPresenting}, paused={owner.Interaction.IsPaused}, popup={owner.CurrentMechanic}, pending={owner.PendingLesson}, saveError={owner.HasSaveError}, conversation={owner.ConversationOpen}";
            Assert.That(ready(), Is.True, message+" | "+state);
        }
        IEnumerator Mount(LevelDefinition level)
        {
            game.LoadLevel(level); game.ShowGame();
            yield return Await(() => game.PreparationReady && game.Interaction.CanPlay, "Fixture must become playable");
        }
        [UnityTest] public IEnumerator LowerBoardRepairPracticeKeepsBothTouchMethodsResponsive()
        {
            game.Preferences.ReducedMotion=false;game.ReducedMotion=false;
            var level=new LevelDefinition("landing-repair-input","landing-repair-input","1","2.0","landing-repair-input",
                Row("CBA"),Row("ABC"),new[]{new TargetSegment(SegmentKind.Shapes,0,3)},
                new[]{new FloorDefinition(1,new[]{true,true}),new FloorDefinition(2,new[]{true,true},repair:new RepairDefinition(0,SymbolId.Parse("s:A"),2))});
            yield return Mount(level);Profile.SeenMechanics.Remove(MechanicLesson.Repair);
            Invoke("PlayCommand",PuzzleCommand.Drop);
            yield return Await(()=>game.CurrentMechanic==MechanicLesson.Repair,"Landing must explain the lower board's repair");
            game.AcknowledgeMechanic();yield return Await(()=>game.Interaction.CanPlay&&game.Interaction.PracticeCommand.HasValue,"Landing must release input for the shown repair route");
            for(int step=0;step<2;step++){
                if(step==0)Write("needsMechanicScan",true);
                var command=game.Interaction.PracticeCommand.Value;int first=command.Edge,second=first+1;
                var pointer=Pointer(first,81+step);var handler=game.SlotViews[first];handler.OnPointerDown(pointer);
                Assert.That(game.Interaction.OwnsPointer(pointer.pointerId),Is.True,"The shown lower-board piece must accept the finger");
                yield return null;
                Assert.That(game.Interaction.OwnsPointer(pointer.pointerId),Is.True,"A tutorial frame must not discard a held touch");
                if(step==0){handler.OnPointerUp(pointer);var next=Pointer(second,81+step);game.SlotViews[second].OnPointerDown(next);game.SlotViews[second].OnPointerUp(next);}
                else{pointer.position=Pointer(second).position;pointer.pointerCurrentRaycast=Pointer(second).pointerCurrentRaycast;handler.OnBeginDrag(pointer);handler.OnDrag(pointer);handler.OnPointerUp(pointer);handler.OnEndDrag(pointer);}
                yield return Await(()=>game.Interaction.CanPlay&&(game.PendingLesson==null||game.Interaction.PracticeCommand.HasValue),"Each guided exchange must release the landing input gate");
            }
            Assert.That(game.PendingLesson,Is.Null);Assert.That(game.Interaction.Attempt.Current.RepairUsed,Is.True);
            Assert.That(Read<Button>("undo").interactable,Is.True,"Undo must work after a stable lower-board exchange");
            string repaired=game.Interaction.Attempt.CurrentStateHash;game.Undo();
            yield return Await(()=>game.Interaction.CanPlay,"Undo on the lower board must animate and release input");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.Not.EqualTo(repaired));
        }
        [UnityTest] public IEnumerator ManualDescentUndoReturnsToUpperBoardAndAcceptsTheNextTouch()
        {
            game.Preferences.Tutorials=false;game.Preferences.ReducedMotion=false;game.ReducedMotion=false;
            yield return Mount(new LevelDefinition("manual-descent-undo","manual-descent-undo","1","2.0","manual-descent-undo",
                Row("CBA"),Row("ABC"),new[]{new TargetSegment(SegmentKind.Shapes,0,3)},
                new[]{new FloorDefinition(4,new[]{true,true}),new FloorDefinition(4,new[]{true,true})}));
            string initial=game.Interaction.Attempt.CurrentStateHash;Invoke("PlayCommand",PuzzleCommand.Drop);
            yield return Await(()=>game.Interaction.CanPlay,"The manual landing must finish");
            Assert.That(Read<Button>("undo").interactable,Is.True);game.Undo();
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(initial));Assert.That(game.Interaction.IsPresenting,Is.True);
            yield return Await(()=>game.Interaction.CanPlay,"Returning to the intact upper board must finish");
            Assert.That(game.VisibleFrame.ActiveFloor,Is.Zero);
            var pointer=Pointer(0);var slot=game.SlotViews[0];slot.OnPointerDown(pointer);
            pointer.position=Pointer(1).position;pointer.pointerCurrentRaycast=Pointer(1).pointerCurrentRaycast;
            slot.OnBeginDrag(pointer);slot.OnDrag(pointer);slot.OnPointerUp(pointer);slot.OnEndDrag(pointer);
            Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));
            yield return Await(()=>game.Interaction.CanPlay,"A touch after the return must remain usable");
        }
        UnityEngine.EventSystems.PointerEventData Pointer(int slot,int pointer=71)
        {
            var rect=(RectTransform)game.SlotViews[slot].transform;
            return new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=pointer,
                button=UnityEngine.EventSystems.PointerEventData.InputButton.Left,
                position=RectTransformUtility.WorldToScreenPoint(Camera.main,rect.TransformPoint(rect.rect.center)),
                pointerCurrentRaycast=new UnityEngine.EventSystems.RaycastResult{gameObject=game.SlotViews[slot].gameObject}};
        }
        [UnityTest] public IEnumerator FingerWobbleRemainsATapAndNextTouchSettlesOnlyAnOrdinaryExchange()
        {
            game.Preferences.ReducedMotion=false;game.ReducedMotion=false;yield return Mount(Fixture());
            var first=Pointer(0);var handler=game.SlotViews[0];handler.OnPointerDown(first);
            first.position+=Vector2.right*game.GetComponentInChildren<Canvas>().scaleFactor*5;
            handler.OnBeginDrag(first);handler.OnDrag(first);handler.OnPointerUp(first);
            Assert.That(game.Interaction.SelectedSlot,Is.EqualTo(0));Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
            var second=Pointer(1);game.SlotViews[1].OnPointerDown(second);game.SlotViews[1].OnPointerUp(second);
            Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));Assert.That(game.Interaction.IsPresenting,Is.True);
            Assert.That(game.Interaction.Execute(PuzzleCommand.Swap(1)),Is.Null,"Core still rejects commands during presentation; there is no command queue");
            yield return null;
            var third=Pointer(2);game.SlotViews[2].OnPointerDown(third);game.SlotViews[2].OnPointerUp(third);
            Assert.That(game.Interaction.SelectedSlot,Is.EqualTo(2),"A new deliberate touch must not be lost in the last exchange animation");
            Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator RealDragCommitsOnceAndCancelledTouchPreservesTheRow()
        {
            game.Preferences.Tutorials=false;game.Preferences.ReducedMotion=false;game.ReducedMotion=false;yield return Mount(Fixture());
            var handler=game.SlotViews[0];var pointer=Pointer(0);handler.OnPointerDown(pointer);
            yield return null;Assert.That(game.Interaction.OwnsPointer(pointer.pointerId),Is.True,"Tutorial opt-out must not cancel a held finger on the next frame");
            pointer.position=Pointer(1).position;pointer.pointerCurrentRaycast=Pointer(1).pointerCurrentRaycast;
            handler.OnBeginDrag(pointer);handler.OnDrag(pointer);handler.OnPointerUp(pointer);handler.OnEndDrag(pointer);
            Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));Assert.That(game.Interaction.Attempt.Current.Row,Is.EqualTo(Row("BAC")));
            yield return Await(()=>!game.Interaction.IsPresenting,"Real drag must settle");
            var before=game.Interaction.Attempt.Current;pointer=Pointer(2);game.SlotViews[2].OnPointerDown(pointer);
            game.SendMessage("OnApplicationPause",true);game.SlotViews[2].OnPointerUp(pointer);game.SendMessage("OnApplicationPause",false);
            Assert.That(game.Interaction.Attempt.Current,Is.SameAs(before));Assert.That(game.IsHoldingToken,Is.False);
        }
        [UnityTest] public IEnumerator UndoIsIntroducedOnFirstAvailabilityAndRestoresWithMotion()
        {
            Profile.SeenMechanics.Remove(MechanicLesson.Undo);Profile.SeenCoachPrompts.Remove("controls-undo-v1");
            game.Preferences.ReducedMotion=false;game.ReducedMotion=false;yield return Mount(Fixture());
            string initial=game.Interaction.Attempt.CurrentStateHash;
            Assert.That(game.MechanicPopupOpen,Is.False,"Undo has nothing to restore before the first real move");
            game.Interaction.Execute(PuzzleCommand.Swap(0));
            yield return Await(()=>game.CurrentMechanic==MechanicLesson.Undo,"Undo introduction must open without first pressing the button");
            game.AcknowledgeMechanic();yield return Await(()=>game.IsTutorialHandVisible,"The finger must point to Undo");
            var start=game.Machine.TokenVisualPosition(0);game.Undo();
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(initial));Assert.That(game.Interaction.IsPresenting,Is.True);
            Assert.That(Vector3.Distance(game.Machine.TokenVisualPosition(1),start),Is.LessThan(.01f),"Restored piece starts at its pre-Undo position, not its destination");
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(game.Interaction.IsPresenting,Is.True);Assert.That(game.Machine.TokenPose(1),Is.EqualTo(PiecePose.Exchanging));
            yield return Await(()=>!game.Interaction.IsPresenting,"Undo motion must finish");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(initial));Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
            Assert.That(Profile.SeenCoachPrompts,Does.Contain("controls-undo-v1"));
            game.Interaction.Execute(PuzzleCommand.Swap(0));yield return Await(()=>!game.Interaction.IsPresenting,"Second exchange must finish");yield return null;
            Assert.That(game.MechanicPopupOpen||game.PendingLesson.HasValue,Is.False,"Undo must be taught once");
        }
        [UnityTest] public IEnumerator BothControlMethodsFitTogetherOnTheFirstExplanation()
        {
            yield return Mount(Fixture("BAC"));Profile.SeenCoachPrompts.Remove("controls-two-ways-v1");
            game.SetUiLocale("uk");game.Preferences.LargeText=true;game.ReducedMotion=false;
            Invoke("OpenMechanicPopup",MechanicLesson.Swap,null,default(UiCopy),null);
            Assert.That(game.GetComponentsInChildren<LessonFilm>().Length,Is.EqualTo(2));
            yield return Capture("two-controls-uk-small.png",320,693);
            Assert.That(game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Where(t=>t.name=="Control method").All(t=>!t.isTextOverflowing),Is.True);
            game.AcknowledgeMechanic();yield return Await(()=>game.Interaction.PracticeCommand.HasValue,"An executable swap must be indicated");
            yield return Perform(game.Interaction.PracticeCommand.Value);
            Assert.That(Profile.SeenCoachPrompts,Does.Contain("controls-two-ways-v1"));
        }
        [UnityTest] public IEnumerator FreshCampaignUsesRelatedTriadsAndRetryKeepsThePuzzle()
        {
            foreach(string node in new[]{"C01","C02","C09"}){
                var source=game.Catalog.GetPrimary(node).Definition;
                game.StartCampaignLevel(source);game.ShowGame();
                yield return Await(()=>game.PreparationReady&&game.Interaction.CanPlay,"Published campaign must become playable");
                Assert.That(game.Descent.Version,Is.EqualTo(6));
                Assert.That(game.Interaction.Attempt.PinnedContentHash,Is.EqualTo(game.Descent.StageHash(0)));
                Assert.That(game.Interaction.Attempt.Current.Row,Is.EqualTo(game.Descent.Level(source,0).InitialSymbolIds));
                Assert.That(game.MechanicPopupOpen||game.PendingLesson.HasValue,Is.False,"Seen lessons must stay retired");
                string hash=game.Interaction.Attempt.CurrentStateHash;
                game.Retry();yield return Await(()=>game.PreparationReady&&game.Interaction.CanPlay,"Retry must be ready");
                Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
                var proof=Cascade.Solver.PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver);int entries=0,exits=0;
                foreach(var command in proof.Path){
                    var actual=game.Interaction.Execute(command);
                    Assert.That(actual.Accepted,Is.True);
                    entries+=actual.Events.Count(e=>e.Kind==PuzzleEventKind.EntryPermuted);
                    exits+=actual.Events.Count(e=>e.Kind==PuzzleEventKind.FloorExited);
                    yield return Await(()=>!game.Interaction.IsPresenting,"Floor animation must settle");
                }
                Assert.That(game.Interaction.Attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
                Assert.That(exits,Is.EqualTo(game.Interaction.Attempt.Level.Floors.Count));
                Assert.That(entries,Is.EqualTo(game.Interaction.Attempt.Level.Floors.Count(f=>f.EntryTransform!=EntryTransform.None)),"Entry device runs exactly on the actual landing");
            }
        }
        [UnityTest] public IEnumerator DisablingTutorialsDoesNotLeaveAnInputLockOrOpenFilms()
        {
            Profile.SeenMechanics.Clear();game.Preferences.Tutorials=false;
            game.StartCampaignLevel(game.Catalog.GetPrimary("C01").Definition);game.ShowGame();
            yield return Await(()=>game.PreparationReady&&game.Interaction.CanPlay,"Tutorials off must remain playable");
            Assert.That(game.MechanicPopupOpen,Is.False);Assert.That(game.PendingLesson,Is.Null);
            Invoke("PlayCommand",PuzzleCommand.Swap(0));
            Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));
            Assert.That(game.MechanicPopupOpen,Is.False);Assert.That(game.Interaction.PracticeRestricted,Is.False);
            yield return Await(()=>!game.Interaction.IsPresenting,"Disabled tutorial move must finish");
            game.StartCampaignLevel(game.Catalog.GetPrimary("C01").Definition);game.ShowGame();
            yield return Await(()=>game.Interaction.CanPlay,"Reloading a cached puzzle with tutorials off must release its preparation gate");
        }
        [UnityTest] public IEnumerator DeferredPracticeDoesNotReopenOnTheSameBoardOrRetry()
        {
            yield return Mount(Fixture("ABC",new RepairDefinition(0,SymbolId.Parse("s:C"),1)));
            Profile.SeenMechanics.Remove(MechanicLesson.Repair);Invoke("BeginLessonPractice",MechanicLesson.Repair);
            Invoke("DeferLessonPractice");Write("needsMechanicScan",true);Invoke("TryFirstMechanic");
            Assert.That(game.MechanicPopupOpen,Is.False);Assert.That(game.PendingLesson,Is.Null);
            game.Retry();yield return Await(()=>game.Interaction.CanPlay,"Retry must not revive an unavailable cue");
            Assert.That(game.MechanicPopupOpen,Is.False);Assert.That(Profile.SeenMechanics,Has.No.Member(MechanicLesson.Repair));
        }
        IEnumerator Begin(MechanicLesson lesson)
        {
            Profile.SeenMechanics.Remove(lesson); Invoke("BeginLessonPractice", lesson);
            Assert.That(game.Interaction.PracticeRestricted, Is.True, "Searching must already deny unrelated actions");
            if(!game.Interaction.PracticeGuidanceOnly)Assert.That(game.Interaction.Execute(PuzzleCommand.Drop), Is.Null);
            yield return Await(() => game.Interaction.PracticeCommand.HasValue && game.IsTutorialHandVisible,
                "A proved practice command and its hand must become available");
            Assert.That(game.PendingLesson, Is.EqualTo(lesson)); Assert.That(game.HasSaveError, Is.False);
        }
        IEnumerator Perform(PuzzleCommand command)
        {
            int route = game.Interaction.Attempt.RouteLength;
            Invoke("PlayCommand", command);
            Assert.That(game.Interaction.Attempt.RouteLength, Is.EqualTo(route + 1), "The indicated real command must commit exactly once");
            yield return Await(() => !game.Interaction.IsPresenting && game.Interaction.CanPlay, "The real exchange must finish");
        }
        void AssertHandAndContours(PuzzleCommand command)
        {
            Assert.That(command.Kind, Is.EqualTo(CommandKind.Swap));
            Invoke("UpdateLessonPractice", true);
            var hand = Read<Image>("tutorialHand"); var machine = game.Machine;
            Assert.That(game.IsTutorialHandVisible, Is.True);
            Assert.That(Vector3.Distance(hand.transform.position,
                machine.TokenVisualPosition(command.Edge) + machine.TokenBodyCentre(command.Edge)), Is.LessThan(.002f),
                "Reduced Motion keeps the authored fingertip on the indicated source");
            Assert.That(hand.rectTransform.pivot, Is.EqualTo(new Vector2(.60f, .865f)));
            var contours = (IList)typeof(ToyMachineView).GetField("selections", PrivateInstance).GetValue(machine);
            for (int i = 0; i < contours.Count; i++) {
                var part = contours[i]; var transform = (Transform)part.GetType().GetField("transform").GetValue(part);
                Assert.That(transform.gameObject.activeInHierarchy, Is.EqualTo(i == command.Edge || i == command.Edge + 1),
                    "Only the indicated pair must have a lesson contour");
            }
        }
        void AssertWrongCommandsPreserveState(PuzzleCommand allowed)
        {
            var attempt = game.Interaction.Attempt; var state = attempt.Current; string hash = attempt.CurrentStateHash;
            int route = attempt.RouteLength; var id = attempt.AttemptId;
            int wrongSlot = Enumerable.Range(0, state.Row.Count).First(i => !game.Interaction.AllowsSlot(i));
            Assert.That(game.Interaction.PointerDown(91, wrongSlot), Is.False);
            game.Interaction.FocusSlot(wrongSlot); Assert.That(game.Interaction.SelectedSlot, Is.Null);
            Assert.That(game.Interaction.TapSlot(wrongSlot), Is.Null);
            var wrongMove = PuzzleCommand.Swap(allowed.Edge == 0 ? 1 : 0);
            Assert.That(game.Interaction.Execute(wrongMove), Is.Null);
            Invoke("PlayCommand", wrongMove); Invoke("PlayCommand", PuzzleCommand.Drop);
            Assert.That(game.Interaction.Undo(), Is.False); game.Undo(); game.Retry(); game.RequestHint();
            Assert.That(Read<object>("hintTask"), Is.Null, "A denied hint must not start a search");
            Assert.That(game.Interaction.Attempt, Is.SameAs(attempt)); Assert.That(attempt.Current, Is.SameAs(state));
            Assert.That(attempt.CurrentStateHash, Is.EqualTo(hash)); Assert.That(attempt.RouteLength, Is.EqualTo(route));
            Assert.That(attempt.AttemptId, Is.EqualTo(id)); Assert.That(game.Interaction.PracticeCommand, Is.EqualTo(allowed));
        }
        void AssertUnlocked(MechanicLesson lesson)
        {
            Assert.That(game.PendingLesson, Is.Null); Assert.That(game.Interaction.PracticeRestricted, Is.False);
            Assert.That(Profile.SeenMechanics, Does.Contain(lesson)); Assert.That(game.HasSaveError, Is.False);
            Assert.That(new ProfileStore(game.ProfilePath).Load().Profile.SeenMechanics, Does.Contain(lesson));
            Assert.That(game.Interaction.AllowsCommand(PuzzleCommand.Drop), Is.True);
            Assert.That(game.Interaction.AllowsCommand(PuzzleCommand.Swap(0)), Is.True);
            Assert.That(game.Interaction.AllowsCommand(PuzzleCommand.Swap(1)), Is.True);
            Assert.That(game.Interaction.AllowsControl(PracticeControl.Hint), Is.True);
            Assert.That(game.Interaction.AllowsTool(TravelTool.SignalLantern), Is.True);
        }
        IEnumerator Capture(string filename, int width = 390, int height = 844)
        {
            var camera = Read<Camera>("uiCamera"); var scaler = game.GetComponentInChildren<CanvasScaler>();
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active; bool oldEnabled = scaler.enabled;
            var render = new RenderTexture(width, height, 24); var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
            string hash = game.Interaction.Attempt.CurrentStateHash;
            try {
                scaler.enabled = false; camera.targetTexture = render; scaler.enabled = true;
                yield return null; game.RefreshLayout(); yield return null; game.RefreshLayout(); yield return new WaitForEndOfFrame();
                game.RefreshLayout(); Invoke("UpdateLessonPractice", true);
                camera.Render(); RenderTexture.active = render;
                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                string folder = Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName, "Reports", "GuidedLessons-Oct08");
                Directory.CreateDirectory(folder); File.WriteAllBytes(Path.Combine(folder, filename), pixels.EncodeToPNG());
                Assert.That(game.Interaction.Attempt.CurrentStateHash, Is.EqualTo(hash), "A native capture must not change the puzzle");
            }
            finally {
                scaler.enabled = false; camera.targetTexture = oldTarget; scaler.enabled = oldEnabled; RenderTexture.active = oldActive;
                game.RefreshLayout(); UnityEngine.Object.Destroy(pixels); render.Release(); UnityEngine.Object.Destroy(render);
            }
        }

        [UnityTest] public IEnumerator SwapRejectsOtherInputsToolsAndHintsAndShowsTheExactPair()
        {
            yield return Mount(Fixture()); game.SetUiLocale("ru");
            Assert.That(game.Interaction.Execute(PuzzleCommand.Swap(0)).Accepted, Is.True);
            yield return Await(() => !game.Interaction.IsPresenting, "Pre-existing move must finish");
            Assert.That(game.Interaction.Attempt.CanUndo, Is.True, "Undo would be legal without the practice gate");
            var economy = (AdService)typeof(CascadeGame).GetProperty("GameEconomy", PrivateInstance).GetValue(game);
            economy.Ledger.SignalLanterns = 1; Write("toolsReturnScreen", GameScreen.Playing);
            yield return Begin(MechanicLesson.Swap);
            var allowed = game.Interaction.PracticeCommand.Value; AssertHandAndContours(allowed);
            AssertWrongCommandsPreserveState(allowed);
            var state = game.Interaction.Attempt.Current;
            var offer = AttemptService.CalculateHint(game.Interaction.Attempt.CaptureHintRequest());
            Assert.That(offer.Result.Status, Is.EqualTo(Cascade.Solver.HintStatus.Step));
            Assert.That(game.Interaction.Execute(offer.Result.Command.Value), Is.Null);
            Write("offeredHint", offer); Read<Button>("hint").onClick.Invoke();
            Assert.That(game.Interaction.Attempt.HintUsed, Is.False);
            game.ShowSupplies(); Assert.That(game.CurrentScreen, Is.EqualTo(GameScreen.Playing));
            Assert.That((bool)typeof(CascadeGame).GetMethod("ToolEligible", PrivateInstance).Invoke(game, new object[] { TravelTool.SignalLantern }), Is.True);
            game.UseShopTool(TravelTool.SignalLantern);
            Assert.That(Read<bool>("toolSearchPending"), Is.False); Assert.That(game.Stock(TravelTool.SignalLantern), Is.EqualTo(1));
            Assert.That(game.Interaction.Attempt.LanternUsed, Is.False); Assert.That(game.Interaction.Attempt.Current, Is.SameAs(state));
            yield return Capture("swap-start-ru.png");
            yield return Perform(allowed); AssertUnlocked(MechanicLesson.Swap);
            yield return Capture("swap-finish-ru.png");
        }

        [UnityTest] public IEnumerator PauseResumeKeepsTheRestrictionAndOnlyTheNextLessonLocksAgain()
        {
            yield return Mount(Fixture(repair: new RepairDefinition(2, SymbolId.Parse("s:B"), 1)));
            Assert.That(game.Interaction.Execute(PuzzleCommand.Swap(0)).Accepted, Is.True);
            yield return Await(() => !game.Interaction.IsPresenting, "Pre-existing move must finish");
            yield return Begin(MechanicLesson.Swap);
            var allowed = game.Interaction.PracticeCommand.Value; var state = game.Interaction.Attempt.Current;
            Invoke("TogglePause"); yield return null;
            Assert.That(game.CurrentScreen, Is.EqualTo(GameScreen.Pause)); Assert.That(game.Interaction.PracticeRestricted, Is.True);
            Assert.That(game.Interaction.Execute(allowed), Is.Null); Assert.That(game.Interaction.Attempt.Current, Is.SameAs(state));
            game.ShowGame();
            yield return Await(() => game.Interaction.CanPlay && game.IsTutorialHandVisible, "Resuming must restore the same practice");
            Assert.That(game.Interaction.PracticeCommand, Is.EqualTo(allowed)); AssertWrongCommandsPreserveState(allowed);
            yield return Perform(allowed); AssertUnlocked(MechanicLesson.Swap);
            yield return null; Assert.That(game.Interaction.PracticeRestricted, Is.False, "Normal play stays available between lessons");
            yield return Begin(MechanicLesson.Repair);
            Assert.That(game.Interaction.AllowsCommand(PuzzleCommand.Drop), Is.True,"Device explanations suggest without blocking normal play");
            yield return Perform(game.Interaction.PracticeCommand.Value); AssertUnlocked(MechanicLesson.Repair);
        }

        [UnityTest] public IEnumerator RepairPreparationKeepsTheGateUntilTheRealContactSucceeds()
        {
            yield return Mount(Fixture("BCA", new RepairDefinition(0, SymbolId.Parse("s:A"), 1)));
            game.SetUiLocale("uk"); game.Preferences.LargeText = true;
            game.SetEditorSafeInsets(new Vector4(.02f, .06f, .02f, .07f));
            yield return Begin(MechanicLesson.Repair);
            var first = game.Interaction.PracticeCommand.Value; Assert.That(first, Is.EqualTo(PuzzleCommand.Swap(1)));
            AssertHandAndContours(first); yield return Capture("repair-start-uk-small.png", 320, 693);
            yield return Perform(first);
            Assert.That(game.PendingLesson, Is.EqualTo(MechanicLesson.Repair)); Assert.That(Profile.SeenMechanics, Has.No.Member(MechanicLesson.Repair));
            Assert.That(game.Interaction.PracticeRestricted, Is.True); Assert.That(game.Interaction.Attempt.Current.RepairUsed, Is.False);
            yield return Await(() => game.Interaction.PracticeCommand.HasValue && game.IsTutorialHandVisible, "The next real contact step must become available");
            var contact = game.Interaction.PracticeCommand.Value; Assert.That(contact, Is.EqualTo(PuzzleCommand.Swap(0)));
            Assert.That(game.Interaction.PracticeGuidanceOnly,Is.True);Assert.That(Read<Button>("undo").interactable,Is.True);
            Assert.That(Enumerable.Range(0,game.Interaction.Attempt.Current.Row.Count).All(game.Interaction.AllowsSlot),Is.True);AssertHandAndContours(contact);
            yield return Perform(contact);
            Assert.That(game.Interaction.Attempt.Current.RepairUsed, Is.True); AssertUnlocked(MechanicLesson.Repair);
            yield return Capture("repair-finish-uk-small.png", 320, 693);
        }

        [UnityTest] public IEnumerator StampPracticeSuggestsTheBonusWithoutBlockingAWrongMove()
        {
            var level=new LevelDefinition("stamp-choice","stamp-choice","1","2.0","stamp-choice",
                Row("CBA"),Row("ABC"),new[]{new TargetSegment(SegmentKind.Shapes,0,3)},
                new[]{new FloorDefinition(8,new[]{true,true},optionalStamp:new ContactDefinition(0,SymbolId.Parse("s:A")))});
            yield return Mount(level);Profile.SeenMechanics.Remove(MechanicLesson.Stamp);Invoke("BeginLessonPractice",MechanicLesson.Stamp);
            yield return Await(()=>game.Interaction.PracticeCommand.HasValue&&game.IsTutorialHandVisible,"Stamp route must be shown as a suggestion");
            Assert.That(game.Interaction.PracticeGuidanceOnly,Is.True,"The stamp cue must not lock the row to the suggested command");
            var suggested=game.Interaction.PracticeCommand.Value;
            var wrong=new[]{PuzzleCommand.Swap(0),PuzzleCommand.Swap(1)}.First(command=>!command.Equals(suggested));
            var result=game.Interaction.Execute(wrong);
            Assert.That(result,Is.Not.Null);Assert.That(result.Accepted,Is.True,"A legal wrong placement remains the player's choice");
            Assert.That(result.State.StampBits,Is.Zero,"A wrong symbol simply leaves the optional stamp uncollected");
            Assert.That(game.PendingLesson,Is.EqualTo(MechanicLesson.Stamp));
        }
    }
}
