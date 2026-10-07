using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Cascade.Tests
{
    public sealed class RoundLoadingLifecycleTests
    {
        const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        CascadeGame game;
        string profileDirectory;
        bool oldRunInBackground;
        readonly List<TaskCompletionSource<DescentRun>> heldPlans = new List<TaskCompletionSource<DescentRun>>();

        [UnitySetUp] public IEnumerator SetUp()
        {
            oldRunInBackground = UnityEngine.Application.runInBackground;
            UnityEngine.Application.runInBackground = true;
            profileDirectory = Path.Combine(Path.GetTempPath(), "CascadeRoundLifecycle", Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory = profileDirectory;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game = UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Assert.That(game, Is.Not.Null);
            game.SendMessage("OnApplicationFocus", true);
            game.SendMessage("OnApplicationPause", false);
            game.Preferences.ReducedMotion = true;
            game.ReducedMotion = true;
            game.Feedback.MusicEnabled = game.Feedback.SoundEnabled = false;
            Read<PlayerProfile>("profile").SeenMechanics = Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            game.ShowGame();
            yield return Await(() => game.PreparationReady && game.Interaction.CanPlay, "Initial puzzle must become playable");
            Invoke("PersistProgress");
        }

        [UnityTearDown] public IEnumerator TearDown()
        {
            if (game != null) { UnityEngine.Object.Destroy(game.gameObject); yield return null; }
            foreach (var held in heldPlans) held.TrySetCanceled();
            heldPlans.Clear();
            CascadeGame.EditorProfileDirectory = null;
            UnityEngine.Application.runInBackground = oldRunInBackground;
            if (profileDirectory != null && Directory.Exists(profileDirectory)) Directory.Delete(profileDirectory, true);
        }

        T Read<T>(string name) => (T)typeof(CascadeGame).GetField(name, PrivateInstance).GetValue(game);
        void Write(string name, object value) => typeof(CascadeGame).GetField(name, PrivateInstance).SetValue(game, value);
        void Invoke(string name, params object[] arguments) => typeof(CascadeGame).GetMethod(name, PrivateInstance).Invoke(game, arguments);
        PlayerProfile Saved() => new ProfileStore(game.ProfilePath).Load().Profile;

        static IEnumerator Await(Func<bool> ready, string message)
        {
            double started = Time.realtimeSinceStartupAsDouble;
            while (!ready() && Time.realtimeSinceStartupAsDouble - started < 30) yield return null;
            Assert.That(ready(), Is.True, message);
        }

        TaskCompletionSource<DescentRun> HoldPlan(out Task<DescentRun> worker)
        {
            worker = Read<Task<DescentRun>>("descentTask");
            Assert.That(worker, Is.Not.Null, "Starting a descent must schedule its proof");
            // Keep the real worker and its immutable result; control only when Update can consume it.
            worker.ContinueWith(t => { var ignored = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            var held = new TaskCompletionSource<DescentRun>();
            heldPlans.Add(held);
            Write("descentTask", held.Task);
            return held;
        }

        void AssertSavedAttempt(AttemptRecord expected)
        {
            var saved = Saved();
            Assert.That(saved.Attempt.AttemptId, Is.EqualTo(expected.AttemptId));
            Assert.That(saved.Attempt.ContentHash, Is.EqualTo(expected.ContentHash));
            Assert.That(saved.Attempt.StateHash, Is.EqualTo(expected.StateHash));
            Assert.That(saved.RestoreAttempt(game.Catalog).CurrentStateHash, Is.EqualTo(expected.StateHash));
        }

        [UnityTest] public IEnumerator CompletingAPlanKeepsPauseSettingsAndHomeAndNeverSavesAPlaceholder()
        {
            var source = game.Catalog.GetPrimary("C03").Definition;
            foreach (var screen in new[] { GameScreen.Pause, GameScreen.Settings, GameScreen.Home }) {
                var before = Saved().Attempt;
                game.StartDescentLevel(source);
                var held = HoldPlan(out var worker);
                Invoke("SetScreen", screen);
                Invoke("PersistProgress");
                AssertSavedAttempt(before);
                var waitingState = game.Interaction.Attempt.Current;
                Assert.That(game.Interaction.Execute(PuzzleCommand.Drop), Is.Null);
                Assert.That(game.Interaction.Attempt.Current, Is.SameAs(waitingState));

                yield return Await(() => worker.IsCompleted, "C03 descent proof must finish");
                Assert.That(worker.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                Assert.That(worker.Result, Is.Not.Null);
                held.SetResult(worker.Result);
                yield return Await(() => !game.IsPlanningDescent && game.Descent != null && game.PreparationReady,
                    "Completed proof must mount its prepared stage");
                yield return null;

                Assert.That(game.CurrentScreen, Is.EqualTo(screen), "Worker completion must preserve navigation");
                Assert.That(game.Interaction.IsPaused, Is.True);
                Assert.That(game.Interaction.CanPlay, Is.False);
                Assert.That(game.Descent.StageIndex, Is.Zero);
                Assert.That(game.Interaction.Attempt.PinnedContentHash, Is.EqualTo(game.Descent.Level(source, 0).ContentHash));
                var saved = Saved();
                Assert.That(saved.Descent.SourceHash, Is.EqualTo(source.ContentHash));
                Assert.That(saved.RestoreAttempt(game.Catalog).CurrentStateHash, Is.EqualTo(game.Interaction.Attempt.CurrentStateHash));
                game.ShowGame();
                yield return Await(() => game.Interaction.CanPlay, "Returning from the preserved menu must unlock the proven stage");
            }
        }

        [UnityTest] public IEnumerator DisablingDuringAPlanKeepsTheSavedAttemptAndRestartsTheRequestedDescent()
        {
            var source = game.Catalog.GetPrimary("C03").Definition;
            var before = Saved().Attempt;
            game.StartDescentLevel(source);
            var held = HoldPlan(out _);
            Invoke("SetScreen", GameScreen.Pause);
            game.enabled = false;
            Assert.That(game.IsPlanningDescent, Is.True, "The requested descent must survive cancellation of its worker");
            AssertSavedAttempt(before);
            var stoppedState = game.Interaction.Attempt.Current;
            Assert.That(game.Interaction.Execute(PuzzleCommand.Drop), Is.Null);
            Assert.That(game.Interaction.Attempt.Current, Is.SameAs(stoppedState));
            yield return null;

            game.enabled = true;
            var restarted = Read<Task<DescentRun>>("descentTask");
            Assert.That(restarted, Is.Not.Null);
            Assert.That(restarted, Is.Not.SameAs(held.Task));
            yield return Await(() => !game.IsPlanningDescent && game.Descent != null && game.PreparationReady,
                "Re-enabling must finish the requested descent rather than expose a single-stage placeholder");
            Assert.That(game.CurrentScreen, Is.EqualTo(GameScreen.Pause));
            Assert.That(game.Interaction.CanPlay, Is.False);
            Assert.That(game.Descent.SourceHash, Is.EqualTo(source.ContentHash));
            Assert.That(game.Descent.StageIndex, Is.Zero);
            Assert.That(game.Interaction.Attempt.PinnedContentHash, Is.EqualTo(game.Descent.Level(source, 0).ContentHash));
            var saved = Saved();
            Assert.That(saved.Descent.StageIndex, Is.Zero);
            Assert.That(saved.RestoreAttempt(game.Catalog).CurrentStateHash, Is.EqualTo(game.Interaction.Attempt.CurrentStateHash));
            game.ShowGame();
            yield return Await(() => game.Interaction.CanPlay, "Resumed descent must accept input after returning to play");
            Assert.That(saved.Clears, Is.Empty);
        }

        [UnityTest] public IEnumerator RestoredCheckpointSurvivesDisableAndCanAdvanceToTheNextSavedStage()
        {
            var source = game.Catalog.GetPrimary("C03").Definition;
            var planning = Task.Run(() => DescentRun.Build(source, version: 4));
            yield return Await(() => planning.IsCompleted, "C03 checkpoint fixture must be proved");
            Assert.That(planning.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            var run = planning.Result;
            Assert.That(run, Is.Not.Null);
            var completed = new AttemptService(run.Level(source, 0), direction: run.Direction);
            foreach (string encoded in run.Stages[0].Witness) {
                var command = encoded == "D" ? PuzzleCommand.Drop : encoded.StartsWith("R")
                    ? PuzzleCommand.Reach(int.Parse(encoded.Substring(1).Split(':')[0]), int.Parse(encoded.Split(':')[1]))
                    : PuzzleCommand.Swap(int.Parse(encoded));
                Assert.That(completed.Execute(command).Accepted, Is.True);
            }
            Assert.That(completed.Current.Status, Is.EqualTo(PuzzleStatus.Won));
            Invoke("MountAttempt", completed, run);
            Invoke("PersistProgress");
            var saved = Saved();
            var restored = saved.RestoreAttempt(game.Catalog);
            Assert.That(restored.CurrentStateHash, Is.EqualTo(completed.CurrentStateHash));
            // Match startup: the saved checkpoint replaces an ordinary live placeholder attempt.
            Invoke("MountAttempt", new AttemptService(game.Catalog.GetPrimary("C01").Definition), null);
            Write("profile", saved);
            Invoke("MountAttempt", restored, saved.Descent);
            game.ShowHome();
            game.ShowGame();
            yield return Await(() => game.PreparationReady && game.Interaction.CanControl,
                "A restored terminal checkpoint must not wait for Playing-only mechanic discovery");
            Assert.That(game.CurrentScreen, Is.EqualTo(GameScreen.Checkpoint));
            Assert.That(game.Interaction.Attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));

            game.enabled = false;
            yield return null;
            game.enabled = true;
            yield return Await(() => game.Interaction.CanControl, "Disabling a checkpoint must not reinstate its preparation gate");
            Assert.That(game.CurrentScreen, Is.EqualTo(GameScreen.Checkpoint));
            game.GoLower();
            Assert.That(game.Descent.StageIndex, Is.EqualTo(1));
            yield return Await(() => game.PreparationReady && game.Interaction.CanPlay && !game.IsDescending,
                "The next stage must be playable after its own preparation");
            Assert.That(game.CurrentScreen, Is.EqualTo(GameScreen.Playing));
            saved = Saved();
            Assert.That(saved.Descent.StageIndex, Is.EqualTo(1));
            Assert.That(saved.RestoreAttempt(game.Catalog).CurrentStateHash, Is.EqualTo(game.Interaction.Attempt.CurrentStateHash));
            Assert.That(saved.Clears, Is.Empty, "A checkpoint is not a whole-level reward");
        }

        [UnityTest] public IEnumerator WinningToolPublishesCompletionBeforeShowingTheResultAndCannotPublishTwice()
        {
            game.LoadNode("C01");
            game.ShowGame();
            yield return Await(() => game.PreparationReady && game.Interaction.CanPlay, "C01 must be ready for the tool step");
            var attempt = game.Interaction.Attempt;
            var hint = AttemptService.CalculateHint(attempt.CaptureHintRequest());
            Assert.That(attempt.TryUseToolAtomically(TravelTool.SignalLantern, () => true, hint), Is.True);
            Assert.That(attempt.Current.Status, Is.EqualTo(PuzzleStatus.Won));
            var transition = attempt.LastToolTransition;

            Assert.That(game.Interaction.PresentToolTransition(transition), Is.True);
            var completion = Read<PlayerProfile>("profile").Completion;
            Assert.That(completion, Is.Not.Null, "The real OnCommitted handler must record the winning tool step");
            Assert.That(completion.AttemptId, Is.EqualTo(attempt.AttemptId.ToString("D")));
            Assert.That(game.CurrentScreen, Is.EqualTo(GameScreen.Playing), "Presentation precedes the result screen");
            Assert.That(game.Interaction.IsPresenting, Is.True);
            var saved = Saved();
            Assert.That(saved.Completion.AttemptId, Is.EqualTo(completion.AttemptId));
            Assert.That(saved.Clears.Count(c => c.CampaignNodeId == "C01"), Is.EqualTo(1));
            Assert.That(saved.RestoreAttempt(game.Catalog).CurrentStateHash, Is.EqualTo(attempt.CurrentStateHash));

            yield return Await(() => !game.Interaction.IsPresenting && game.CurrentScreen == GameScreen.Win,
                "The recorded delivery must reach its result screen after presentation");
            Assert.That(game.Interaction.PresentToolTransition(transition), Is.False,
                "Finishing presentation must not make the same inventory transaction publishable again");
            Assert.That(Saved().Clears.Count(c => c.CampaignNodeId == "C01"), Is.EqualTo(1));
        }

        [UnityTest] public IEnumerator FailedDescentKeepsTheSavedAttemptAndItsRetryButtonRestartsTheSameRequest()
        {
            var source = game.Catalog.GetPrimary("C01").Definition;
            var before = Saved().Attempt;
            game.StartDescentLevel(source, "en", DeliveryMood.Recovery);
            Write("journeyActive",true);Write("descentTravel",true);
            var held = HoldPlan(out _);
            held.SetResult(null);
            yield return Await(() => Read<bool>("descentPlanFailed"), "A failed proof must expose retry");
            Invoke("PersistProgress");
            AssertSavedAttempt(before);
            Assert.That(game.CurrentScreen, Is.EqualTo(GameScreen.Playing));
            var waitingState = game.Interaction.Attempt.Current;
            Assert.That(game.Interaction.CanPlay, Is.False);
            Assert.That(game.Interaction.Execute(PuzzleCommand.Drop), Is.Null);
            Assert.That(game.Interaction.Attempt.Current, Is.SameAs(waitingState));

            game.ShowHome();game.ShowJourney();
            var travel=game.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Travel");
            Assert.That(travel.GetComponentInChildren<TMPro.TextMeshProUGUI>().text,Is.EqualTo("Continue"));
            travel.onClick.Invoke();
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing),"A failed request must resume from the map without starting its story again.");
            Assert.That(game.Interaction.Attempt.Current,Is.SameAs(waitingState));

            var retry = Read<UnityEngine.UI.Button>("retry");
            Assert.That(retry.gameObject.activeInHierarchy, Is.True);
            Assert.That(retry.interactable, Is.True, "Failed descent planning must not disable its own recovery action");
            retry.onClick.Invoke();
            Assert.That(game.IsPlanningDescent, Is.True);
            Assert.That(Read<Task<DescentRun>>("descentTask"), Is.Not.Null);
            Assert.That(Read<Task<DescentRun>>("descentTask"), Is.Not.SameAs(held.Task));
            Assert.That(Read<LevelDefinition>("descentSource").ContentHash, Is.EqualTo(source.ContentHash));
            Assert.That(Read<DeliveryMood>("descentMood"), Is.EqualTo(DeliveryMood.Recovery));
            AssertSavedAttempt(before);
            yield return Await(() => !game.IsPlanningDescent && game.Descent != null && game.PreparationReady && game.Interaction.CanPlay,
                "Retry must finish the original descent request and release input");
            Assert.That(game.Descent.SourceHash, Is.EqualTo(source.ContentHash));
            Assert.That(game.Descent.StageIndex, Is.Zero);
            var saved = Saved();
            Assert.That(saved.Descent.SourceHash, Is.EqualTo(source.ContentHash));
            Assert.That(saved.RestoreAttempt(game.Catalog).CurrentStateHash, Is.EqualTo(game.Interaction.Attempt.CurrentStateHash));
        }

        [UnityTest] public IEnumerator DisablingAnIntroductionNeverPaintsAlreadyDestroyedControls()
        {
            Read<PlayerProfile>("profile").SeenMechanics.Remove(MechanicLesson.Gust);
            Assert.That(game.RequireGustIntroduction(),Is.True);
            Assert.That(game.MechanicPopupOpen,Is.True);
            // Unity may destroy canvas children before disabling the owner when leaving Play Mode.
            UnityEngine.Object.DestroyImmediate(Read<UnityEngine.UI.Button>("undo").GetComponentInChildren<TMPro.TextMeshProUGUI>().gameObject);
            game.enabled=false;
            yield return null;
            Assert.That(game.MechanicPopupOpen,Is.False);
            Assert.That(game.Interaction.IsSuspended,Is.True);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest] public IEnumerator RewardGrantWhileLoadingKeepsTheLastSavedAttemptAndDescentTogether()
        {
            var source = game.Catalog.GetPrimary("C03").Definition;
            game.StartDescentLevel(source);
            yield return Await(() => !game.IsPlanningDescent && game.Descent != null && game.PreparationReady && game.Interaction.CanPlay,
                "The reward fixture must first have a real saved descent");
            Invoke("PersistProgress");
            var before = Saved();
            Assert.That(before.Descent, Is.Not.Null);
            var service = Read<AdService>("monetization");
            int coins = service.Ledger.BonusCoins;

            game.StartDescentLevel(source, "en", DeliveryMood.Recovery);
            var held = HoldPlan(out var worker);
            game.ShowShop();
            var waitingState = game.Interaction.Attempt.Current;
            var ticket = service.TryRewarded(new AdContext {
                Surface = AdSurface.Inventory, CoreAccepted = true, PrivacyReady = true, ProviderEnabled = true
            }, "coins", true);
            Assert.That(ticket, Is.Not.Null);
            Assert.That(service.Opened(ticket.Id), Is.True);
            Assert.That(service.RewardConfirmed(ticket.Id, "round-loading-reward", true), Is.True);
            service.Closed(ticket.Id);

            Assert.That(service.Ledger.BonusCoins, Is.EqualTo(coins + 40));
            Assert.That(game.Interaction.Attempt.Current, Is.SameAs(waitingState));
            Assert.That(game.Interaction.CanPlay, Is.False);
            AssertSavedAttempt(before.Attempt);
            var saved = Saved();
            Assert.That(saved.Descent.ContentHash, Is.EqualTo(before.Descent.ContentHash));
            Assert.That(saved.Descent.StageIndex, Is.EqualTo(before.Descent.StageIndex));
            Assert.That(saved.Monetization.BonusCoins, Is.EqualTo(service.PreviewOnly ? before.Monetization.BonusCoins : coins + 40),
                "Only the preview service keeps test currency out of the durable ledger");

            yield return Await(() => worker.IsCompleted, "The held C03 proof must complete");
            Assert.That(worker.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            Assert.That(worker.Result, Is.Not.Null);
            held.SetResult(worker.Result);
            yield return Await(() => !game.IsPlanningDescent && game.Descent != null && game.PreparationReady,
                "The later plan completion must still mount a valid saved stage");
            saved = Saved();
            Assert.That(saved.RestoreAttempt(game.Catalog).CurrentStateHash, Is.EqualTo(game.Interaction.Attempt.CurrentStateHash));
            Assert.That(game.CurrentScreen, Is.EqualTo(GameScreen.Shop));
        }
    }
}
