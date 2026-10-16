using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using Cascade.Solver;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class NavigationResumeTests
    {
        const BindingFlags PrivateInstance=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;
        string directory;
        bool oldRunInBackground;
        Camera camera;
        RenderTexture portraitTarget,oldTarget;

        [UnitySetUp] public IEnumerator SetUp()
        {
            oldRunInBackground=UnityEngine.Application.runInBackground;
            UnityEngine.Application.runInBackground=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeNavigationResume",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Preferences.Tutorials=false;
            game.Preferences.ReducedMotion=true;game.ReducedMotion=true;
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            Read<PlayerProfile>("profile").SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            // These fixtures exercise navigation after stories have already been read.
            var heard=Read<HashSet<int>>("heardStory");heard.Add(1000);
            heard.UnionWith(Enumerable.Range(0,JourneyStory.PageCount));
            heard.UnionWith(Enumerable.Range(2000,JourneyStory.WorldCount*10));
            heard.UnionWith(Enumerable.Range(4000,JourneyStory.TotalLevels));
            heard.UnionWith(Enumerable.Range(6000,JourneyStory.WorldCount));
            heard.UnionWith(Enumerable.Range(7000,JourneyStory.TotalLevels));
            heard.UnionWith(Enumerable.Range(8000,JourneyStory.WorldCount*10));
            // Batch-mode Game view defaults to landscape. Exercise resume in the
            // supported portrait viewport instead of the intentional rotation gate.
            camera=Camera.main;oldTarget=camera.targetTexture;portraitTarget=new RenderTexture(720,1280,24);
            var scaler=game.GetComponentInChildren<UnityEngine.UI.CanvasScaler>();scaler.enabled=false;
            camera.targetTexture=portraitTarget;scaler.enabled=true;yield return null;game.RefreshLayout();
            game.ShowGame();
            yield return Await(()=>game.PreparationReady&&game.Interaction.CanPlay,"Initial puzzle must become playable");
        }

        [UnityTearDown] public IEnumerator TearDown()
        {
            if(camera!=null)camera.targetTexture=oldTarget;
            if(portraitTarget!=null){portraitTarget.Release();UnityEngine.Object.Destroy(portraitTarget);}
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            CascadeGame.EditorProfileDirectory=null;
            UnityEngine.Application.runInBackground=oldRunInBackground;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }

        T Read<T>(string name)=>(T)typeof(CascadeGame).GetField(name,PrivateInstance).GetValue(game);
        void Write(string name,object value)=>typeof(CascadeGame).GetField(name,PrivateInstance).SetValue(game,value);
        void Invoke(string name,params object[] arguments)=>typeof(CascadeGame).GetMethod(name,PrivateInstance).Invoke(game,arguments);
        PlayerProfile Saved()=>new ProfileStore(game.ProfilePath).Load().Profile;
        static IEnumerator Await(Func<bool> ready,string message)
        {
            double began=Time.realtimeSinceStartupAsDouble;
            while(!ready()&&Time.realtimeSinceStartupAsDouble-began<30)yield return null;
            Assert.That(ready(),Is.True,message);
        }
        IEnumerator Click(string name)
        {
            yield return null;
            var button=game.GetComponentsInChildren<Button>().SingleOrDefault(b=>b.name==name);
            Assert.That(button,Is.Not.Null,"Expected control "+name+" on "+game.CurrentScreen);
            Assert.That(button.interactable,Is.True,name+" must be available");
            button.onClick.Invoke();yield return null;
        }
        static void Solve(AttemptService attempt)
        {
            var solution=PuzzleSolver.FindShortest(attempt.Resolver,attempt.Current);
            Assert.That(solution.Status,Is.EqualTo(SearchStatus.Solved));
            foreach(var command in solution.Path)Assert.That(attempt.Execute(command).Accepted,Is.True);
            Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
        }
        IEnumerator MountPublishedLevelAt(string campaignNode,int stage)
        {
            var profile=Read<PlayerProfile>("profile");
            foreach(var content in game.Catalog.PrimaryLevels.TakeWhile(c=>c.Definition.CampaignNodeId!=campaignNode)){
                var prior=new AttemptService(content.Definition);Solve(prior);
                Assert.That(profile.RecordClear(prior),Is.True);
                Read<HashSet<string>>("sessionClears").Add(content.Definition.CampaignNodeId);
            }
            var source=game.Catalog.GetPrimary(campaignNode).Definition;
            var run=DescentRun.Build(source,version:5);Assert.That(run,Is.Not.Null);
            while(run.StageIndex<stage){
                var prior=new AttemptService(run.Level(source,run.StageIndex),direction:run.Direction);Solve(prior);
                run.AcceptCheckpoint(source,prior);
            }
            Invoke("MountAttempt",new AttemptService(run.Level(source,stage),direction:run.Direction),run);
            Write("journeyActive",true);Write("descentTravel",true);
            Invoke("InitializeRewards");Invoke("PersistProgress",true);game.ShowGame();
            yield return Await(()=>game.Interaction.CanPlay,"Mounted descent stage must become playable");
        }
        IEnumerator MountThirdLevelAt(int stage)=>MountPublishedLevelAt("C03",stage);
        IEnumerator SolveCurrent()
        {
            var solution=PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver,game.Interaction.Attempt.Current);
            Assert.That(solution.Status,Is.EqualTo(SearchStatus.Solved));
            foreach(var command in solution.Path){
                Assert.That(game.Interaction.Execute(command).Accepted,Is.True);
                yield return Await(()=>!game.Interaction.IsPresenting,"Committed command must finish presenting");
            }
            yield return Await(()=>game.CurrentScreen==GameScreen.Checkpoint||game.CurrentScreen==GameScreen.Win,"Delivery must reach its result");
        }
        IEnumerator HomeMenusThenMap()
        {
            game.ShowHome();game.ShowSettings();game.CloseMenu();game.ShowShop();game.ShowHome();game.ChooseStory();
            yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
        }

        [UnityTest] public IEnumerator ForegroundRefreshCannotResurrectRoundControlsOnTheMap()
        {
            yield return MountThirdLevelAt(1);game.ReducedMotion=false;
            var current=game.Interaction.Attempt.Current;
            Assert.That(current.Row.SequenceEqual(game.Interaction.Attempt.Level.TargetSymbolIds),Is.False);
            int drops=game.Interaction.Attempt.Level.Floors.Count-current.ActiveFloor;
            for(int i=0;i<drops;i++){
                Assert.That(game.Interaction.Execute(PuzzleCommand.Drop).Accepted,Is.True);
                yield return Await(()=>!game.Interaction.IsPresenting,"Drop must finish");
            }
            yield return Await(()=>game.CurrentScreen==GameScreen.Fail,"Loss must reach the failure view");
            Invoke("RenderControls");
            Assert.That(Read<RectTransform>("resultRoot").gameObject.activeInHierarchy,Is.False,"Old results must stay hidden behind the reference failure view");
            game.ShowJourney();yield return null;
            Write("descentPlanFailed",true);
            for(int cycle=0;cycle<3;cycle++){
                game.SendMessage("OnApplicationFocus",false);game.SendMessage("OnApplicationPause",true);
                Write("absentSince",Time.realtimeSinceStartup-3600);
                game.SendMessage("OnApplicationPause",false);game.SendMessage("OnApplicationFocus",true);
                yield return null;
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
                foreach(string field in new[]{"retry","hint","drop","undo","precise","help"})
                    Assert.That(Read<Component>(field).gameObject.activeInHierarchy,Is.False,field+" leaked onto the map after foreground return");
                Assert.That(game.GetComponentsInChildren<RectTransform>().Any(r=>r.name=="Reference failure result"),Is.False);
            }
        }

        [UnityTest] public IEnumerator FinalDeliveryInterruptedByBackgroundReturnsToOneCompletion()
        {
            yield return MountThirdLevelAt(2);game.ReducedMotion=false;
            var attempt=game.Interaction.Attempt;var solution=PuzzleSolver.FindShortest(attempt.Resolver,attempt.Current);
            foreach(var command in solution.Path.Take(solution.Path.Count-1)){
                Assert.That(game.Interaction.Execute(command).Accepted,Is.True);
                yield return Await(()=>!game.Interaction.IsPresenting,"Exchange must finish");
            }
            Assert.That(game.Interaction.Execute(solution.Path.Last()).Accepted,Is.True);
            game.SendMessage("OnApplicationPause",true);game.SendMessage("OnApplicationFocus",false);
            Write("absentSince",Time.realtimeSinceStartup-3600);yield return null;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            yield return Await(()=>game.CurrentScreen==GameScreen.Win,"Interrupted delivery must finalize on return");
            Assert.That(game.Interaction.Attempt,Is.SameAs(attempt));
            Assert.That(game.LastCompletion.AttemptId,Is.EqualTo(attempt.AttemptId.ToString("D")));
            Assert.That(Saved().Clears.Count(c=>c.CampaignNodeId=="C03"),Is.EqualTo(1));
        }

        [UnityTest] public IEnumerator LostContinueRetriesTheSameMiddleAndFinalStageAndRetainsClears()
        {
            yield return MountThirdLevelAt(1);
            for(int stage=1;stage<=2;stage++){
                Assert.That(game.Descent.StageIndex,Is.EqualTo(stage));
                string pinned=game.Interaction.Attempt.PinnedContentHash;
                var run=game.Descent;
                int drops=game.Interaction.Attempt.Level.Floors.Count-game.Interaction.Attempt.Current.ActiveFloor;
                var wrongDelivery=game.Interaction.Attempt.Current;
                for(int drop=0;drop<drops;drop++){
                    var preview=game.Interaction.Attempt.Resolver.Resolve(wrongDelivery,PuzzleCommand.Drop);
                    Assert.That(preview.Accepted,Is.True);wrongDelivery=preview.State;
                }
                Assert.That(wrongDelivery.Status,Is.EqualTo(PuzzleStatus.Lost),"The fixture must prove an incorrect receiver delivery before testing recovery");
                for(int drop=0;drop<drops;drop++){
                    Assert.That(game.Interaction.Execute(PuzzleCommand.Drop).Accepted,Is.True);
                    yield return Await(()=>!game.Interaction.IsPresenting,"Wrong-route descent must finish presenting before its next command");
                }
                Assert.That(game.Interaction.Attempt.Current.Status,Is.EqualTo(PuzzleStatus.Lost));
                yield return Await(()=>game.CurrentScreen==GameScreen.Fail,"Wrong delivery must show failure");
                yield return HomeMenusThenMap();
                Assert.That(game.GetComponentsInChildren<Button>().Single(b=>b.name=="Travel").GetComponentInChildren<TextMeshProUGUI>().text,Is.EqualTo("Continue"));
                yield return Click("Travel");
                yield return Await(()=>game.Interaction.CanPlay,"Continue after failure must make the same stage playable");
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));
                Assert.That(game.Descent,Is.SameAs(run));Assert.That(game.Descent.StageIndex,Is.EqualTo(stage));
                Assert.That(game.Interaction.Attempt.PinnedContentHash,Is.EqualTo(pinned));
                Assert.That(game.Interaction.Attempt.RouteLength,Is.Zero);
                Assert.That(Saved().Clears.Select(c=>c.CampaignNodeId),Is.EquivalentTo(new[]{"C01","C02"}));
                if(stage==1){yield return SolveCurrent();game.GoLower();yield return Await(()=>game.Interaction.CanPlay,"Final stage must become playable");}
            }
        }

        [UnityTest] public IEnumerator ContinuePreservesPlayingRouteAndDeliveredCheckpoint()
        {
            yield return MountThirdLevelAt(1);
            var command=PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path.First();
            Assert.That(game.Interaction.Execute(command).Accepted,Is.True);
            yield return Await(()=>!game.Interaction.IsPresenting,"First exchange must finish");
            Assert.That(game.Interaction.Attempt.Current.Status,Is.EqualTo(PuzzleStatus.Playing));
            var row=game.Interaction.Attempt.Current;var id=game.Interaction.Attempt.AttemptId;
            yield return HomeMenusThenMap();yield return Click("Travel");
            Assert.That(game.Interaction.Attempt.Current,Is.SameAs(row));Assert.That(game.Interaction.Attempt.AttemptId,Is.EqualTo(id));
            Assert.That(game.Interaction.Attempt.RouteLength,Is.EqualTo(1));
            yield return SolveCurrent();row=game.Interaction.Attempt.Current;
            yield return HomeMenusThenMap();yield return Click("Travel");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Checkpoint));Assert.That(game.Descent.StageIndex,Is.EqualTo(1));
            Assert.That(game.Interaction.Attempt.Current,Is.SameAs(row));Assert.That(game.Interaction.Attempt.AttemptId,Is.EqualTo(id));
        }

        [UnityTest] public IEnumerator PendingFinalCompletionReturnsToTheSameReceipt()
        {
            yield return MountThirdLevelAt(2);yield return SolveCurrent();
            var completion=game.LastCompletion;var row=game.Interaction.Attempt.Current;
            Assert.That(completion.Acknowledged,Is.False);
            yield return HomeMenusThenMap();yield return Click("Travel");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Win));Assert.That(game.LastCompletion,Is.SameAs(completion));
            Assert.That(game.Interaction.Attempt.Current,Is.SameAs(row));Assert.That(game.LastCompletion.Acknowledged,Is.False);
            Assert.That(Saved().Completion.AttemptId,Is.EqualTo(completion.AttemptId));
        }

        [UnityTest] public IEnumerator BagAndGuideHaveNoPracticeEntryAndLegacyActionsRetainSavedDescent()
        {
            yield return MountThirdLevelAt(1);
            var before=Saved();var row=game.Interaction.Attempt.Current;
            game.ShowJourney();game.ShowTravelBag();yield return null;
            Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Practice"),Is.False);
            foreach(var open in new Action[]{game.ShowPowerupChallenges,game.ShowReachChallenges}){
                open();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));
                Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Trythistrail"),Is.False);
            }
            game.ShowDevices();yield return null;
            Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Trythisexample"),Is.False);
            Assert.That(game.Interaction.Attempt.Current,Is.SameAs(row));
            Invoke("PersistProgress",true);var after=Saved();
            Assert.That(after.Attempt.AttemptId,Is.EqualTo(before.Attempt.AttemptId));
            Assert.That(after.Attempt.StateHash,Is.EqualTo(before.Attempt.StateHash));
            Assert.That(after.Descent.StageIndex,Is.EqualTo(1));Assert.That(after.Descent.ContentHash,Is.EqualTo(before.Descent.ContentHash));
            Assert.That(after.Clears.Select(c=>c.CampaignNodeId),Is.EquivalentTo(new[]{"C01","C02"}));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.ShowJourney();yield return Click("Travel");
            yield return Await(()=>game.Interaction.CanPlay,"Reload must continue the saved campaign stage");
            Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo("C03"));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(before.Attempt.StateHash));
            Assert.That(game.Descent.StageIndex,Is.EqualTo(1));Assert.That(game.JourneyClearCount,Is.EqualTo(2));
        }

        [UnityTest] public IEnumerator OpenedArrowsBrowseOnlyEarnedLevelsWithoutReplacingThePinnedRow()
        {
            int first=JourneyStory.IslandFirst(0,1);
            yield return MountPublishedLevelAt(game.Catalog.PrimaryLevels[first].Definition.CampaignNodeId,1);string hash=game.Interaction.Attempt.CurrentStateHash;
            game.ShowJourney();yield return null;
            Assert.That(game.MapWorld,Is.Zero);Assert.That(game.BrowsedIsland,Is.EqualTo(-1));
            Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Previousworld"||b.name=="Nextworld"),Is.False);
            game.OpenJourneyIsland(0);yield return null;Assert.That(game.BrowsedIsland,Is.Zero);
            game.ReplayJourneyLevel(first);yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap),"An opened level outside the selected island cannot be replayed");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            game.CloseJourneyIsland();yield return null;Assert.That(game.MapWorld,Is.Zero);
            game.OpenJourneyIsland(1);yield return null;Assert.That(game.BrowsedIsland,Is.EqualTo(1));
            game.ReplayJourneyLevel(first+1);yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap),"Later levels on the opened island remain locked");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            game.ReplayJourneyLevel(first);yield return Await(()=>game.Interaction.CanPlay,"Selecting the pinned level must resume the current stage");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            Assert.That(game.Descent.StageIndex,Is.EqualTo(1));Assert.That(game.JourneyClearCount,Is.EqualTo(first));
        }

        [UnityTest] public IEnumerator OptionalStampTrailCannotReplaceAResumableCampaignFromUiOrDirectCalls()
        {
            yield return MountThirdLevelAt(1);
            var before=Saved();var row=game.Interaction.Attempt.Current;var run=game.Descent;
            game.ShowContracts();yield return null;
            var launch=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trythistrail");
            Assert.That(launch.interactable,Is.False);
            Assert.That(launch.GetComponentInChildren<TextMeshProUGUI>().text,Is.EqualTo("Finish level first"));
            var content=Read<LevelContent[]>("contracts").First();
            int stampGoal=content.Definition.Floors.Select((f,i)=>f.OptionalStamp==null?0:1<<i).Aggregate(0,(a,b)=>a|b);
            // Direct API calls and a stale UI callback must obey the same central guard.
            game.StartStampTrail(content.Definition.CampaignNodeId);
            Invoke("StartExactTrail",content,stampGoal);launch.onClick.Invoke();
            Assert.That(game.Interaction.Attempt.Current,Is.SameAs(row));Assert.That(game.Descent,Is.SameAs(run));
            Assert.That(Saved().Attempt.StateHash,Is.EqualTo(before.Attempt.StateHash));
            Assert.That(Saved().Attempt.AttemptId,Is.EqualTo(before.Attempt.AttemptId));

            var stampAttempt=new AttemptService(content.Definition,content.Locale,new PuzzleGoal(stampGoal));
            var solution=PuzzleSolver.FindShortest(stampAttempt.Resolver,goal:stampAttempt.Goal);
            Assert.That(solution.Status,Is.EqualTo(SearchStatus.Solved));
            foreach(var command in solution.Path)Assert.That(stampAttempt.Execute(command).Accepted,Is.True);
            Assert.That(stampAttempt.Goal.IsSatisfied(stampAttempt.Current),Is.True);
            var records=Read<PlayerProfile>("profile").ModeRecords;
            new JourneyRewards().RecordDelivery(stampAttempt,records:records);
            records.RemoveAll(r=>r.StampGoal==0);
            Assert.That(records.Count,Is.EqualTo(1));
            game.ShowRecords();yield return null;
            var replay=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Tryagain");
            Assert.That(replay.interactable,Is.False);
            Assert.That(replay.GetComponentInChildren<TextMeshProUGUI>().text,Is.EqualTo("Finish level first"));
            replay.onClick.Invoke();Invoke("PersistProgress",true);
            Assert.That(game.Interaction.Attempt.Current,Is.SameAs(row));Assert.That(game.Descent,Is.SameAs(run));
            var after=Saved();Assert.That(after.Attempt.StateHash,Is.EqualTo(before.Attempt.StateHash));
            Assert.That(after.Attempt.AttemptId,Is.EqualTo(before.Attempt.AttemptId));
            Assert.That(after.Descent.ContentHash,Is.EqualTo(before.Descent.ContentHash));Assert.That(after.Descent.StageIndex,Is.EqualTo(1));
            Assert.That(after.Clears.Select(c=>c.CampaignNodeId),Is.EquivalentTo(new[]{"C01","C02"}));
        }

        [UnityTest] public IEnumerator PublishedCampaignEntryUsesVerifiedPackagedRecipes()
        {
            var source=game.Catalog.GetPrimary("C526");game.StartCampaignLevel(source.Definition,source.Locale);game.ShowGame();
            yield return Await(()=>game.PreparationReady&&!game.IsPlanningDescent&&game.Interaction.CanPlay,"Packaged campaign entry must finish");
            var certificates=Read<Task<PublishedTriadStore>>("publishedTriads");
            Assert.That(certificates.IsCompleted&&!certificates.IsFaulted,Is.True);Assert.That(certificates.Result,Is.Not.Null);
            var expected=certificates.Result.Bind(source.Definition,source.Locale,game.Catalog.ContentHash);
            Assert.That(game.Descent.ContentHash,Is.EqualTo(expected.ContentHash));Assert.That(game.Descent.StageIndex,Is.Zero);
        }

        [UnityTest] public IEnumerator FirstIslandHidesUnavailableArrowsAndCannotBrowseLockedWorlds()
        {
            game.ShowJourney();yield return null;
            Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Previousopenedlevel"||b.name=="Nextopenedlevel"),Is.False);
            Assert.That(Read<int>("selectedJourneyStop"),Is.Zero);Assert.That(Read<int>("mapPage"),Is.Zero);
            game.TurnJourneyMap(1);yield return null;
            Assert.That(Read<int>("selectedJourneyStop"),Is.Zero);Assert.That(Read<int>("mapPage"),Is.Zero);
            Assert.That(game.Machine.LivingWorld.Zone,Is.Zero);
        }

        string SelectedCounterTitle()=>game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Selected object title").text;
        void AssertPinnedAttempt(PlayerProfile before)
        {
            Invoke("PersistProgress",true);var after=Saved();
            Assert.That(after.Attempt.AttemptId,Is.EqualTo(before.Attempt.AttemptId));
            Assert.That(after.Attempt.StateHash,Is.EqualTo(before.Attempt.StateHash));
            Assert.That(after.Descent.ContentHash,Is.EqualTo(before.Descent.ContentHash));
            Assert.That(after.Descent.StageIndex,Is.EqualTo(before.Descent.StageIndex));
            Assert.That(after.Clears.Select(c=>c.CampaignNodeId),Is.EquivalentTo(before.Clears.Select(c=>c.CampaignNodeId)));
        }

        [UnityTest] public IEnumerator MapBagTrophiesBackReturnsToBagThenTheSameMap()
        {
            yield return MountThirdLevelAt(1);var before=Saved();
            game.ShowJourney();yield return Click("Map bag");yield return Click("Trophies");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Collection));
            Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Buy selected item"||b.name=="Coins tab"),Is.False);
            Read<AdService>("virtualEconomy").Ledger.Cosmetics.Add("leaf");game.SelectShopItem(0);yield return Click("Lookcloser");yield return Click("Trophy action");
            Assert.That(game.EquippedGift,Is.EqualTo("leaf"));Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Collection));
            game.ShowCollection();yield return Click("Back");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));
            yield return Click("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
            AssertPinnedAttempt(before);
        }

        [UnityTest] public IEnumerator ExplicitTrophyShopVisitReturnsToTheSelectedTrophyThenBagAndMap()
        {
            yield return MountThirdLevelAt(1);var before=Saved();
            game.ShowJourney();yield return Click("Map bag");yield return Click("Trophies");
            game.SelectShopItem(4);yield return Click("Lookcloser");yield return Click("Trophy action");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Shop));
            game.SelectShopTab(0);game.SelectShopItem(2);yield return Click("Back");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Collection));
            Assert.That(SelectedCounterTitle(),Is.EqualTo("Wind chime"));
            yield return Click("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));
            yield return Click("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
            AssertPinnedAttempt(before);
        }

        [UnityTest] public IEnumerator BuyShopBagAndToolsVisitsRestoreTheirSelectionsAndOriginalHome()
        {
            yield return MountThirdLevelAt(1);var before=Saved();
            Read<AdService>("virtualEconomy").Ledger.BonusCoins=1000;
            game.ShowHome();yield return Click("Shop");game.SelectShopTab(0);game.SelectShopItem(2);
            Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Buy selected item"),Is.False,"Selecting shows the title, not the preview");
            int stock=game.Stock(TravelTool.ReturnHourglass);yield return Click("Lookcloser");
            yield return Click("Buy selected item");Assert.That(game.Stock(TravelTool.ReturnHourglass),Is.EqualTo(stock+1));
            yield return Click("Back");yield return Click("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            yield return Click("Mythings");yield return Click("Mytools");game.SelectShopItem(4);
            yield return Click("Lookcloser");yield return Click("Open shop");
            game.SelectShopItem(1);yield return Click("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Supplies));
            Assert.That(SelectedCounterTitle(),Is.EqualTo("Landing lens"));
            yield return Click("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));
            yield return Click("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            AssertPinnedAttempt(before);
        }

        [UnityTest] public IEnumerator HomeShopCoinAndBagLinksCannotCreateBackLoops()
        {
            yield return MountThirdLevelAt(1);var before=Saved();
            game.ShowHome();yield return Click("Shop");game.SelectShopTab(1);game.SelectShopItem(6);
            yield return Click("Coins tab");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.CoinPacks));
            game.ShowShop();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Shop));
            Assert.That(SelectedCounterTitle(),Is.EqualTo("Copper acorn"));
            Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="My tools link"),Is.False,"Cabinet belongs to Home");
            game.ShowShop();yield return Click("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            yield return Click("Mythings");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));
            yield return Click("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            AssertPinnedAttempt(before);
        }

    }
}
