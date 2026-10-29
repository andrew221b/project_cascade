#if UNITY_EDITOR
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
    public sealed class FirstUseHelpRegressionTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;string directory;bool background,audio;Camera camera;RenderTexture portrait,oldTarget;
        PlayerProfile Profile=>(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeFirstUseHelp",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=directory;
            var profile=new PlayerProfile{AccountIntroSeen=true};profile.Missions.NextAfterClears=20000;
            new ProfileStore(Path.Combine(directory,"cascade-profile.json")).Save(profile);
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Assert.That(game,Is.Not.Null);
            camera=Camera.main;oldTarget=camera.targetTexture;portrait=new RenderTexture(390,844,24);
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=portrait;scaler.enabled=true;game.RefreshLayout();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Preferences.ReducedMotion=true;game.ReducedMotion=true;
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}
            CascadeGame.EditorProfileDirectory=null;
            UnityEngine.Application.runInBackground=background;AudioListener.pause=audio;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Wait(Func<bool> ready,string context)
        {
            float deadline=Time.realtimeSinceStartup+30;
            while(!ready()&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(ready(),Is.True,context+"; screen="+game.CurrentScreen+" preparing="+game.Interaction.IsPreparing+" ready="+game.PreparationReady+" lesson="+game.CurrentMechanic+" pending="+game.PendingLesson+" seen="+Profile.SeenMechanics.Count+" films="+Profile.SeenCoachPrompts.Count+" tutorials="+game.Preferences.Tutorials);
        }
        void Capture(string name)
        {
            var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/FirstUseHelpOct28"));Directory.CreateDirectory(folder);
            Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=portrait;
            var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);
            pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();
            File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());
            RenderTexture.active=old;UnityEngine.Object.Destroy(pixels);
        }
        [UnityTest] public IEnumerator ToolTutorialReplacesBagInEnglish(){yield return ToolTutorialReplacesBag("en",false);}
        [UnityTest] public IEnumerator ToolTutorialReplacesBagInRussian(){yield return ToolTutorialReplacesBag("ru",true);}
        [UnityTest] public IEnumerator ToolTutorialReplacesBagInUkrainian(){yield return ToolTutorialReplacesBag("uk",false);}
        IEnumerator ToolTutorialReplacesBag(string locale,bool dismiss)
        {
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(lesson!=MechanicLesson.RepairKit)Profile.SeenMechanics.Add(lesson);
            game.SetUiLocale(locale);while((bool)typeof(CascadeGame).GetField("applyingUiLanguage",Private).GetValue(game))yield return null;
            var row=new[]{SymbolId.Parse("s:C"),SymbolId.Parse("s:B"),SymbolId.Parse("s:A")};
            game.LoadLevel(new LevelDefinition("tool-dialog","tool-dialog","1","2.0","tool-dialog",row,row.Reverse().ToArray(),new[]{new TargetSegment(SegmentKind.Shapes,0,3)},new[]{new FloorDefinition(8,new[]{true,true}),new FloorDefinition(8,new[]{true,true})}));
            game.ShowGame();yield return Wait(()=>game.Interaction.CanPlay,"Tool fixture ready");game.Monetization.Ledger.RepairKits=1;
            string before=game.Interaction.Attempt.CurrentStateHash;
            game.ToggleFieldToolTray();yield return null;Assert.That(game.FieldToolTrayOpen,Is.True);
            var select=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Field tool "+(int)TravelTool.RepairKit);select.onClick.Invoke();
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Apply field tool").onClick.Invoke();yield return null;
            Assert.That(game.CurrentMechanic,Is.EqualTo(MechanicLesson.RepairKit));
            Assert.That(game.FieldToolTrayOpen,Is.False,"The inventory must close before its first-use film opens");
            Assert.That(game.Monetization.Ledger.RepairKits,Is.EqualTo(1));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(before));
            // Capture after the existing film entrance; the modal-state assertions above are immediate.
            yield return new WaitForSecondsRealtime(.45f);Capture("single-tool-tutorial-"+locale);
            if(dismiss){game.DismissMechanic();yield return null;Assert.That(game.Monetization.Ledger.RepairKits,Is.EqualTo(1));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(before));}
            else{game.AcknowledgeMechanic();yield return Wait(()=>game.Interaction.CanPlay,"Confirmed tool returns to gameplay");Assert.That(game.Monetization.Ledger.RepairKits,Is.Zero);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.Not.EqualTo(before));Assert.That(game.FieldToolTrayOpen,Is.False);}
        }
        [UnityTest] public IEnumerator VerifiedVideoRewardRestoresBulbAndCannotGrantOnFailedAd()
        {
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))Profile.SeenMechanics.Add(lesson);
            var row=new[]{SymbolId.Parse("s:D"),SymbolId.Parse("s:C"),SymbolId.Parse("s:B"),SymbolId.Parse("s:A")};
            game.LoadLevel(new LevelDefinition("reward-lamp","reward-lamp","1","2.0","reward-lamp",row,row.Reverse().ToArray(),new[]{new TargetSegment(SegmentKind.Shapes,0,4)},new[]{new FloorDefinition(12,new[]{true,true,true})}));game.ShowGame();
            yield return Wait(()=>game.Interaction.CanPlay,"Reward fixture ready");game.RequestHint();yield return Wait(()=>game.HintVisible,"Initial free hint ready");
            var offer=(HintOffer)typeof(CascadeGame).GetField("offeredHint",Private).GetValue(game);Assert.That(game.Interaction.Execute(offer.Result.Command.Value).Accepted,Is.True);yield return Wait(()=>game.Interaction.CanPlay,"First guided move settled");
            // A real preload remains pending here; test the signed grant independently
            // of the separate, unavailable-video free-help path.
            typeof(NativeAdProvider).GetField("loadingReward",Private).SetValue(game.AdProvider,true);
            game.RefreshLayout();var film=game.GetComponentInChildren<VideoHintMark>(true);var lamp=game.GetComponentInChildren<HintMark>(true);Assert.That(film.enabled,Is.True);
            var context=new AdContext{Surface=AdSurface.Inventory,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true};var failed=game.Monetization.TryRewarded(context,"hint",true);Assert.That(failed,Is.Not.Null);game.Monetization.Opened(failed.Id);game.Monetization.Failed(failed.Id);game.NotifyStoreChanged();
            Assert.That(game.Monetization.Ledger.HintTokens,Is.Zero);Assert.That(film.enabled,Is.True,"A failed video cannot manufacture a verified hint");
            var rewarded=game.Monetization.TryRewarded(context,"hint",true);game.Monetization.Opened(rewarded.Id);Assert.That(game.Monetization.RewardConfirmed(rewarded.Id,"verified-hint-fixture",true),Is.True);game.Monetization.Closed(rewarded.Id);
            // A delayed server confirmation can arrive through ordinary cloud sync,
            // after the fullscreen ad callback already finished.
            typeof(CascadeGame).GetMethod("CloudStatusChanged",Private).Invoke(game,null);
            Assert.That(lamp.enabled,Is.True,"Cloud-delivered hint grants must repaint the bulb without another tap");Assert.That(film.enabled,Is.False);
            game.ShowAdReward(rewarded);yield return null;
            Assert.That(lamp.enabled,Is.True);Assert.That(film.enabled,Is.False);game.RequestHint();yield return Wait(()=>game.HintVisible,"Earned hint becomes usable");Assert.That(game.Monetization.Ledger.HintTokens,Is.Zero);Assert.That(lamp.enabled,Is.True);Assert.That(film.enabled,Is.False);
        }
        [UnityTest] public IEnumerator UnavailableVideoRevealsSolverHintWithoutCurrencyOrPlaceholder(){yield return UnavailableHintFlow("en");}
        [UnityTest] public IEnumerator UnavailableVideoHelpIsLocalizedInRussian(){yield return UnavailableHintFlow("ru");}
        [UnityTest] public IEnumerator UnavailableVideoHelpIsLocalizedInUkrainian(){yield return UnavailableHintFlow("uk");}
        IEnumerator UnavailableHintFlow(string locale)
        {
            game.SetUiLocale(locale);while((bool)typeof(CascadeGame).GetField("applyingUiLanguage",Private).GetValue(game))yield return null;
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))Profile.SeenMechanics.Add(lesson);
            var row=new[]{SymbolId.Parse("s:E"),SymbolId.Parse("s:D"),SymbolId.Parse("s:C"),SymbolId.Parse("s:B"),SymbolId.Parse("s:A")};
            game.LoadLevel(new LevelDefinition("no-video-hint","no-video-hint","1","2.0","no-video-hint",row,row.Reverse().ToArray(),new[]{new TargetSegment(SegmentKind.Shapes,0,5)},new[]{new FloorDefinition(12,new[]{true,true,true,true})}));game.ShowGame();
            yield return Wait(()=>game.Interaction.CanPlay,"Fallback fixture ready");
            game.RequestHint();yield return Wait(()=>game.HintVisible,"First free hint ready");
            var first=(HintOffer)typeof(CascadeGame).GetField("offeredHint",Private).GetValue(game);
            Assert.That(game.Interaction.Execute(first.Result.Command.Value).Accepted,Is.True);yield return Wait(()=>game.Interaction.CanPlay,"First move settled");
            Assert.That(game.AdProvider.RewardReady,Is.False);Assert.That(game.AdProvider.RewardLoading,Is.False);
            game.RefreshLayout();Assert.That(game.GetComponentInChildren<VideoHintMark>(true).enabled,Is.True,
                "The rewarded offer must remain visible before fallback, even when no ad is loaded");
            Assert.That(game.GetComponentInChildren<HintMark>(true).enabled,Is.False);
            Capture("oct29-rewarded-offer-unloaded-"+locale);
            var ledger=CloudJson.Encode(game.Monetization.Ledger);var state=game.Interaction.Attempt.Current;int moves=game.Interaction.Attempt.MovesMade;
            var delayedReward=game.Monetization.TryRewarded(new AdContext{Surface=AdSurface.Inventory,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true},"hint",true);
            game.Monetization.Opened(delayedReward.Id);game.Monetization.Closed(delayedReward.Id);
            typeof(CascadeGame).GetField("pendingHintVideoAttempt",Private).SetValue(game,game.Interaction.Attempt.AttemptId);
            typeof(CascadeGame).GetField("pendingHintVideoState",Private).SetValue(game,game.Interaction.Attempt.CurrentStateHash);
            game.RequestHint();yield return Wait(()=>game.HintVisible,"Unavailable video must not prevent a solver hint");
            Assert.That(game.Interaction.Attempt.FreeHintStates.Count,Is.EqualTo(2));Assert.That(game.Interaction.Attempt.HintUsed,Is.True);
            Assert.That(CloudJson.Encode(game.Monetization.Ledger),Is.EqualTo(ledger),"Emergency help cannot mint/spend money, tokens or signed grants");
            Assert.That(game.Interaction.Attempt.Current,Is.SameAs(state));Assert.That(game.Interaction.Attempt.MovesMade,Is.EqualTo(moves));
            Assert.That(game.AdProvider.Showing,Is.False);Assert.That(game.Interaction.CanPlay,Is.True);
            Assert.That(typeof(CascadeGame).GetField("previewAd",Private).GetValue(game),Is.Null,"No placeholder or fake advertising screen");
            game.RefreshLayout();Assert.That(game.GetComponentInChildren<HintMark>(true).enabled,Is.True);
            var banner=game.GetComponentInChildren<GameReactionBanner>(true);
            Assert.That(banner.IsShowing,Is.True);
            Assert.That(banner.Message,Is.EqualTo(locale=="ru"?"Нет видео. Подсказка бесплатно.":locale=="uk"?"Немає відео. Підказка безкоштовна.":"No video. This hint is free."),
                "Emergency help must not imply an ad was watched");
            Capture("oct29-no-video-help-"+locale);
            for(int step=0;step<2;step++){
                var shown=(HintOffer)typeof(CascadeGame).GetField("offeredHint",Private).GetValue(game);
                Assert.That(game.Interaction.Execute(shown.Result.Command.Value).Accepted,Is.True);yield return Wait(()=>game.Interaction.CanPlay,"Next hinted move settled");
                if(step==0){game.RequestHint();yield return Wait(()=>game.HintVisible,"The next ordinary hint remains free");}
            }
            int count=game.Interaction.Attempt.FreeHintStates.Count;Assert.That(count,Is.EqualTo(3));
            game.RefreshLayout();Assert.That(game.GetComponentInChildren<VideoHintMark>(true).enabled,Is.True,
                "Cooldown must not silently remove the rewarded offer");
            game.RequestHint();yield return new WaitForSecondsRealtime(.25f);
            Assert.That(game.HintVisible,Is.False,"Another emergency reveal waits for active play, without a blocking modal");
            Assert.That(game.Interaction.Attempt.FreeHintStates.Count,Is.EqualTo(count));Assert.That(game.Interaction.CanPlay,Is.True);
            Capture("oct29-no-video-cooldown-"+locale);
            Assert.That(CloudJson.Encode(game.Monetization.Ledger),Is.EqualTo(ledger));
            game.Interaction.Attempt.AdvanceActiveTime(30);game.RequestHint();yield return Wait(()=>game.HintVisible,"Emergency help becomes available after active play");
            Assert.That(game.Interaction.Attempt.FreeHintStates.Count,Is.EqualTo(4));Assert.That(CloudJson.Encode(game.Monetization.Ledger),Is.EqualTo(ledger));
            Assert.That(typeof(CascadeGame).GetField("pendingHintVideoState",Private).GetValue(game),Is.Null);
            Assert.That(game.Monetization.RewardConfirmed(delayedReward.Id,"late-verified-after-free-help",true),Is.True);
            game.ShowAdReward(delayedReward);yield return null;
            Assert.That(game.HintVisible,Is.True,"A delayed verified token must not hide an explicitly requested free hint");
            Assert.That(game.Monetization.Ledger.HintTokens,Is.EqualTo(1),"The real reward remains available for a later state");
        }
        [UnityTest] public IEnumerator UnknownEmergencySearchCannotAnnounceAFreeHint()
        {
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))Profile.SeenMechanics.Add(lesson);
            var row=new[]{SymbolId.Parse("s:E"),SymbolId.Parse("s:D"),SymbolId.Parse("s:C"),SymbolId.Parse("s:B"),SymbolId.Parse("s:A")};
            game.LoadLevel(new LevelDefinition("unknown-video-hint","unknown-video-hint","1","2.0","unknown-video-hint",row,row.Reverse().ToArray(),new[]{new TargetSegment(SegmentKind.Shapes,0,5)},new[]{new FloorDefinition(12,new[]{true,true,true,true})}));game.ShowGame();
            yield return Wait(()=>game.Interaction.CanPlay,"Unknown search fixture ready");
            var attempt=game.Interaction.Attempt;var offer=AttemptService.CalculateHint(attempt.CaptureHintRequest(),1);
            Assert.That(offer.Result.Status,Is.EqualTo(Cascade.Solver.HintStatus.Unknown));
            var ledger=CloudJson.Encode(game.Monetization.Ledger);var state=attempt.Current;
            var banner=game.GetComponentInChildren<GameReactionBanner>(true);banner.Clear();
            // Feed the actual budget-limited solver result through the same main-thread
            // completion boundary as an unavailable-video search. No success is fabricated.
            typeof(CascadeGame).GetField("unavailableHintAttempt",Private).SetValue(game,attempt.AttemptId);
            typeof(CascadeGame).GetField("unavailableHintState",Private).SetValue(game,attempt.CurrentStateHash);
            typeof(CascadeGame).GetField("hintTask",Private).SetValue(game,System.Threading.Tasks.Task.FromResult(offer));
            yield return null;yield return null;
            Assert.That(game.HintVisible,Is.False);Assert.That(banner.IsShowing,Is.False,"Unknown is not a delivered hint");
            Assert.That(attempt.FreeHintStates,Is.Empty);Assert.That(attempt.Current,Is.SameAs(state));
            Assert.That(CloudJson.Encode(game.Monetization.Ledger),Is.EqualTo(ledger));
        }
        [UnityTest] public IEnumerator HiddenVideosStillAllowHelpWithACachedReadyAd(){yield return BlockedRewardStillAllowsHelp(true);}
        [UnityTest] public IEnumerator FullGrantLedgerStillAllowsHelpWithACachedReadyAd(){yield return BlockedRewardStillAllowsHelp(false);}
        IEnumerator BlockedRewardStillAllowsHelp(bool hidden)
        {
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))Profile.SeenMechanics.Add(lesson);
            var row=new[]{SymbolId.Parse("s:E"),SymbolId.Parse("s:D"),SymbolId.Parse("s:C"),SymbolId.Parse("s:B"),SymbolId.Parse("s:A")};
            game.LoadLevel(new LevelDefinition("blocked-reward-help","blocked-reward-help","1","2.0","blocked-reward-help",row,row.Reverse().ToArray(),new[]{new TargetSegment(SegmentKind.Shapes,0,5)},new[]{new FloorDefinition(12,new[]{true,true,true,true})}));game.ShowGame();
            yield return Wait(()=>game.Interaction.CanPlay,"Blocked reward fixture ready");game.RequestHint();yield return Wait(()=>game.HintVisible,"Initial free hint ready");
            var first=(HintOffer)typeof(CascadeGame).GetField("offeredHint",Private).GetValue(game);
            Assert.That(game.Interaction.Execute(first.Result.Command.Value).Accepted,Is.True);yield return Wait(()=>game.Interaction.CanPlay,"First move settled");
            // Isolated fixtures emulate readiness only. No native ad is loaded or shown.
            foreach(var field in new[]{"localPreview","privacyReady","initialized"})typeof(NativeAdProvider).GetField(field,Private).SetValue(game.AdProvider,true);
            if(hidden)game.Monetization.Ledger.HideVideoOffers=true;
            else{
                for(int i=0;i<MonetizationLedger.MaxGrantReceipts;i++)game.Monetization.Ledger.Grants.Add(new GrantReceipt{Id="full-ledger-fixture-"+i,Kind="coins",Amount=40});
                game.Monetization.Ledger.BonusCoins=40*MonetizationLedger.MaxGrantReceipts;
            }
            Assert.That(game.AdProvider.RewardReady,Is.True);Assert.That(game.Monetization.Ledger.HideVideoOffers||!game.Monetization.CanReceiveGrant,Is.True);
            string before=CloudJson.Encode(game.Monetization.Ledger);var state=game.Interaction.Attempt.Current;
            game.RequestHint();yield return Wait(()=>game.HintVisible,"Unavailable policy reward must not block free information");
            Assert.That(game.Interaction.Attempt.FreeHintStates.Count,Is.EqualTo(2));Assert.That(game.Interaction.Attempt.Current,Is.SameAs(state));
            Assert.That(CloudJson.Encode(game.Monetization.Ledger),Is.EqualTo(before));Assert.That(game.AdProvider.Showing,Is.False);
        }
        [UnityTest] public IEnumerator FreshProfileStartsWithSwapFilmBeforeFirstMove()
        {
            Assert.That(Profile.SeenMechanics,Is.Empty);Assert.That(game.Preferences.Tutorials,Is.True);
            game.StartCampaignLevel(game.Catalog.GetPrimary("C01").Definition);game.ShowGame();
            yield return Wait(()=>game.CurrentMechanic==MechanicLesson.Swap,"A genuinely fresh profile must see the first controls film");
            Assert.That(game.MechanicPopupOpen,Is.True);Assert.That(game.Interaction.Attempt.MovesMade,Is.Zero);
            Assert.That(Profile.SeenMechanics,Is.Empty,"Opening a film does not acknowledge it");yield return new WaitForSecondsRealtime(.45f);Capture("automatic-first-swap");
        }
        [UnityTest] public IEnumerator FreshMixedProfileStartsItsFirstWordAndShapeWithTheSwapFilm(){yield return FreshCategoryFilm(PuzzleMode.Mixed,"uk",4);}
        [UnityTest] public IEnumerator FreshWordProfileStartsItsFirstRealWordWithTheSwapFilm(){yield return FreshCategoryFilm(PuzzleMode.Words,"ru",3);}
        IEnumerator FreshCategoryFilm(PuzzleMode mode,string locale,int width)
        {
            var remembered=Enumerable.Range(0,10000).ToList();Profile.HeardStory=remembered;
            ((System.Collections.Generic.HashSet<int>)typeof(CascadeGame).GetField("heardStory",Private).GetValue(game)).UnionWith(remembered);
            game.Preferences.PuzzleLocale=locale;game.SetUiLocale(locale);game.ShowJourney();game.SelectMode(mode);yield return null;
            Assert.That(game.PreferredMode,Is.EqualTo(mode));Assert.That(Profile.SeenMechanics,Is.Empty);
            game.StartCampaignLevel(game.Catalog.GetPrimary("C01").Definition,locale);game.ShowGame();
            yield return Wait(()=>game.Descent?.Version==7&&game.CurrentMechanic==MechanicLesson.Swap,"Fresh category must show its first film on the personalised board");
            Assert.That(game.Interaction.Attempt.Mode,Is.EqualTo(mode));Assert.That(game.Interaction.Attempt.Level.TargetSymbolIds.Count,Is.EqualTo(width));Assert.That(game.Interaction.Attempt.MovesMade,Is.Zero);Assert.That(game.MechanicPopupOpen,Is.True);
        }
        [UnityTest] public IEnumerator NewFarSwapIsIntroducedEvenWithOptionalControlTipsOff()
        {
            game.Preferences.Tutorials=false;
            var row=new[]{SymbolId.Parse("s:C"),SymbolId.Parse("s:B"),SymbolId.Parse("s:A")};
            var level=new LevelDefinition("far-help","far-help","1","2.0","far-help",row,row.Reverse().ToArray(),
                new[]{new TargetSegment(SegmentKind.Shapes,0,3)},new[]{new FloorDefinition(8,new[]{true,true})},reachCharges:1);
            game.LoadLevel(level);game.ShowGame();
            yield return Wait(()=>game.CurrentMechanic==MechanicLesson.FarSwap,"A new board mechanic is mandatory even when optional control coaching is off");
            Assert.That(game.Interaction.Attempt.MovesMade,Is.Zero);
        }
        [UnityTest] public IEnumerator PlainBoardKeepsQuestionMarkAndBasicReplayInEveryLocale()
        {
            game.Preferences.Tutorials=false;
            var row=new[]{SymbolId.Parse("s:C"),SymbolId.Parse("s:B"),SymbolId.Parse("s:A")};
            var target=row.Reverse().ToArray();
            var level=new LevelDefinition("plain-help","plain-help","1","2.0","plain-help",row,target,
                new[]{new TargetSegment(SegmentKind.Shapes,0,3)},new[]{new FloorDefinition(8,new[]{true,true}),new FloorDefinition(8,new[]{true,true})});
            game.LoadLevel(level);game.ShowGame();yield return Wait(()=>game.Interaction.CanPlay,"Plain floor becomes playable");
            var help=(Button)typeof(CascadeGame).GetField("help",Private).GetValue(game);
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);
                while((bool)typeof(CascadeGame).GetField("applyingUiLanguage",Private).GetValue(game))yield return null;
                game.RefreshLayout();yield return null;
                Assert.That(help.gameObject.activeInHierarchy,Is.True,locale+": basic help must remain available without devices");
                Capture("plain-question-mark-"+locale);
                Assert.That(help.interactable,Is.True);help.onClick.Invoke();
                Assert.That(game.CurrentMechanic,Is.EqualTo(MechanicLesson.Swap));yield return new WaitForSecondsRealtime(.45f);Capture("basic-replay-"+locale);
                Assert.That(game.GetComponentsInChildren<Button>().Count(b=>b.interactable),Is.GreaterThanOrEqualTo(1));
                game.DismissMechanic();yield return Wait(()=>game.Interaction.CanPlay,"Closing replay restores play");
            }
        }
    }
}
#endif
