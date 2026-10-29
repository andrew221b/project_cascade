using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed class MapScreenFlowTests
    {
        const BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
        CascadeGame game;string directory;bool background;
        T Read<T>(string name)=>(T)typeof(CascadeGame).GetField(name,Private).GetValue(game);
        void Invoke(string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Private).Invoke(game,args);
        Button Button(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        TextMeshProUGUI Text(string name)=>game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Reference shop "+name);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeMapTests",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            void PreloadLog(string message,string stack,LogType type){if(type==LogType.Error&&message.StartsWith("OperationException : Failed to initialize localization, could not preload string tables.",StringComparison.Ordinal))LogAssert.Expect(type,new System.Text.RegularExpressions.Regex("^OperationException : Failed to initialize localization, could not preload string tables\\."));}
            UnityEngine.Application.logMessageReceived+=PreloadLog;
            try{yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;foreach(string locale in new[]{"en","ru","uk"}){var t=UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));yield return t;Assert.That(t.Status,Is.EqualTo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded));}}finally{UnityEngine.Application.logMessageReceived-=PreloadLog;}
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;
            Read<PlayerProfile>("profile").Missions.NextAfterClears=20000;Read<HashSet<int>>("heardStory").UnionWith(Enumerable.Range(0,10000));Read<PlayerProfile>("profile").SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();game.ShowJourney();yield return null;
        }
        [UnityTearDown] public IEnumerator Teardown(){if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=background;if(Directory.Exists(directory))Directory.Delete(directory,true);}
        void EarnFirstEighteen()
        {
            var p=Read<PlayerProfile>("profile");var session=Read<HashSet<string>>("sessionClears");int index=0;
            foreach(var content in game.Catalog.PrimaryLevels.Take(18)){var r=new ClearReceipt{CampaignNodeId=content.Definition.CampaignNodeId,ContentHash=content.Definition.ContentHash,AttemptId=Guid.NewGuid().ToString("D"),BestStars=index<12?2:index<17?3:2,BestHalfStar=index<8,BestScore=1800};p.Clears.Add(r);session.Add(r.CampaignNodeId);index++;}Invoke("InitializeRewards");game.ShowJourney();
        }
        [UnityTest] public IEnumerator NewProfileShowsOneOpenIslandAndFourLockedIslands()
        {
            Assert.That(game.GetComponentsInChildren<Image>().Count(i=>i.name.StartsWith("Reference map island artwork")),Is.EqualTo(5));Assert.That(game.GetComponentsInChildren<Image>().Count(i=>i.name.StartsWith("Reference map island lock")),Is.EqualTo(4));
            Assert.That(Button("Island1").interactable,Is.True);for(int i=2;i<=5;i++){Assert.That(Button("Island"+i).interactable,Is.False);Button("Island"+i).onClick.Invoke();Assert.That(game.BrowsedIsland,Is.EqualTo(-1));}
            Assert.That(Text("map island stars 0").text,Is.EqualTo("0 / 36"));Button("Island1").onClick.Invoke();yield return null;Assert.That(game.BrowsedIsland,Is.Zero);
        }
        [UnityTest] public IEnumerator StarsIncludingHalvesAndCompletionUseActualSavedReceipts()
        {
            EarnFirstEighteen();yield return null;Assert.That(Text("map island stars 0").text,Is.EqualTo("28 / 36"));Assert.That(Text("map island stars 1").text,Is.EqualTo("17 / 36"));Assert.That(Text("map completed 0"),Is.Not.Null);
            var receipt=Read<PlayerProfile>("profile").Clears[0];receipt.BestHalfStar=false;game.ShowJourney();yield return null;Assert.That(Text("map island stars 0").text,Is.EqualTo("27.5 / 36"));
            Assert.That(game.GetComponentsInChildren<MapIslandLight>().Single().transform.parent.name,Is.EqualTo("Floating island 1"));
        }
        [UnityTest] public IEnumerator SpacedIslandsMoveWithoutMapBridgesOrChangingAttemptAndReceipts()
        {
            EarnFirstEighteen();yield return null;var view=game.GetComponentsInChildren<MapReferenceAmbiance>().Single();string hash=game.Interaction.Attempt.CurrentStateHash;var id=game.Interaction.Attempt.AttemptId;int clears=game.JourneyClearCount;
            view.SetReviewAge(0);Assert.That(game.GetComponentsInChildren<Component>().Any(c=>c.GetType().Name=="MapRopeBridge"),Is.False);
            var roots=game.GetComponentsInChildren<RectTransform>().Where(r=>r.name.StartsWith("Floating island ")).ToArray();var initial=roots.Select(r=>r.anchoredPosition).ToArray();
            var sizes=new[]{new Vector2(302,260),new Vector2(264,271),new Vector2(234,220),new Vector2(216,270),new Vector2(220,211)};
            for(int i=0;i<5;i++)Assert.That(roots.Single(r=>r.name=="Floating island "+i).sizeDelta,Is.EqualTo(sizes[i]),"Island size changed");
            for(int frame=1;frame<=360;frame++)view.SetReviewAge(frame/60f);
            Assert.That(roots.Where((r,i)=>Vector2.Distance(initial[i],r.anchoredPosition)>.1f).Count(),Is.EqualTo(5));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Interaction.Attempt.AttemptId,Is.EqualTo(id));Assert.That(game.JourneyClearCount,Is.EqualTo(clears));view.EndReview();yield return null;
        }
        [UnityTest] public IEnumerator MotionStopsInBackgroundAndRespectsReducedMotionAndQuality()
        {
            var view=game.GetComponentsInChildren<MapReferenceAmbiance>().Single();var leaves=game.GetComponentsInChildren<MenuLeafDrift>().Single();float age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(view.AnimationAge,Is.GreaterThan(age));
            game.SendMessage("OnApplicationPause",true);age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(view.AnimationAge,Is.EqualTo(age));game.SendMessage("OnApplicationPause",false);
            game.ReducedMotion=true;age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(view.AnimationAge,Is.EqualTo(age));Assert.That(leaves.VisibleCount,Is.Zero);Assert.That(leaves.DepthOfField,Is.True);
            game.ReducedMotion=false;game.Preferences.EffectQuality=0;age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(view.AnimationAge,Is.EqualTo(age));
        }
        [UnityTest] public IEnumerator SettingsReturnsToMapAndHomePreservesTheAttempt()
        {
            EarnFirstEighteen();yield return null;string hash=game.Interaction.Attempt.CurrentStateHash;Button("Map settings").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Settings));Button("Back").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(Text("map island stars 1").text,Is.EqualTo("17 / 36"));Button("Back").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator EveryLanguageFitsLargeTextAndAllWorldsKeepProportionalArt()
        {
            game.Preferences.LargeText=true;
            foreach(string locale in new[]{"en","ru","uk"}){game.SetUiLocale(locale);for(int world=0;world<JourneyStory.WorldCount;world++){
                typeof(CascadeGame).GetField("mapPage",Private).SetValue(game,world);Invoke("DrawMenu");yield return null;Canvas.ForceUpdateCanvases();foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){t.ForceMeshUpdate();Assert.That(t.isTextOverflowing||t.isTextTruncated,Is.False,locale+" world "+world+" "+t.name);}
                foreach(var image in game.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Reference map island artwork"))){Assert.That(image.preserveAspect,Is.True);Assert.That(image.color,Is.EqualTo(Color.white));Assert.That(image.sprite,Is.Not.Null);}Assert.That(game.GetComponentsInChildren<Component>().Any(c=>c.GetType().Name=="MapRopeBridge"),Is.False);
            }}
        }
        [UnityTest] public IEnumerator SharedArtAndLegacyJournalEntryUseCurrentScreensWithoutChangingProgress()
        {
            foreach(string name in new[]{"Card","CoinCard","GrayAction","GreenAction","InventoryStatCard","PurchasedRibbon","RedAction","SelectedCard","TabsStrip"}){
                var shared=CleanMenuArt.Get(name);Assert.That(shared,Is.Not.Null,name);
                Assert.That(typeof(CascadeGame).GetMethod("ReferenceShopArt",Private).Invoke(game,new object[]{name}),Is.SameAs(shared),name);
            }
            foreach(string name in new[]{"AmberAction","RewardCard"}){
                var shared=CleanMenuArt.Get(name);Assert.That(shared,Is.Not.Null,name);
                Assert.That(typeof(CascadeGame).GetMethod("ParcelArt",Private).Invoke(game,new object[]{name}),Is.SameAs(shared),name);
            }
            for(int world=0;world<9;world++){
                Assert.That(MenuWorldArt.Sky(world),Is.Not.Null,"sky "+world);
                Assert.That(CleanMenuArt.WorldTitle(world),Is.Not.Null,"title "+world);
                Assert.That(WorldFrameArt.Texture(world),Is.SameAs(GameplayReferenceArt.Texture("GameplayFrame")));
                for(int island=0;island<6;island++)Assert.That(WorldIslandArt.Get(world,island),Is.Not.Null,"island "+world+" / "+island);
            }
            string hash=game.Interaction.Attempt.CurrentStateHash;int clears=Read<PlayerProfile>("profile").Clears.Count;
            game.ShowHome();game.ShowJournal();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.text=="My journal"),Is.False);
            Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Inventory claim reward"),Is.True);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(Read<PlayerProfile>("profile").Clears.Count,Is.EqualTo(clears));
        }
        [UnityTest] public IEnumerator RetiredScreenEntryPointsForwardWithoutChangingAttemptOrReceipts()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            var profile=Read<PlayerProfile>("profile");int clears=profile.Clears.Count,parcels=game.Rewards.Parcels.Count;
            foreach(var old in new[]{GameScreen.Journal,GameScreen.Records,GameScreen.Workshop,GameScreen.Contracts,GameScreen.Lessons}){
                game.ShowHome();Invoke("SetScreen",old);yield return null;
                var expected=old==GameScreen.Workshop?GameScreen.Shop:old==GameScreen.Contracts||old==GameScreen.Lessons?GameScreen.JourneyMap:GameScreen.TravelBag;
                Assert.That(game.CurrentScreen,Is.EqualTo(expected),old.ToString());
                Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(profile.Clears.Count,Is.EqualTo(clears));Assert.That(game.Rewards.Parcels.Count,Is.EqualTo(parcels));
            }
            game.ShowHome();game.ShowWorkshop();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Shop));
            game.ShowHome();game.ShowRecords();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));Button("Back").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            game.ShowContracts();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
            game.ShowLessons();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
            Invoke("SetScreen",GameScreen.JourneyEnd);game.ShowJournal();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));Button("Back").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyEnd));
        }
        [UnityTest] public IEnumerator CaptureRemainingScreenCompositionsForTheAudit()
        {
            var editor=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable.PlayableCapture")).First(t=>t!=null);
            var capture=editor.GetMethod("CaptureSettled");game.SetUiLocale("ru");game.SetEditorSafeInsets(new Vector4(.02f,.055f,.02f,.05f));
            IEnumerator Shot(string name)=>(IEnumerator)capture.Invoke(null,new object[]{"RemainingScreensCleanupOct15",name+".png",390,844,(Action<CascadeGame>)null});
            game.ShowJourney();game.OpenJourneyIsland(0);yield return Shot("island-level-selection");
            Invoke("SetScreen",GameScreen.JourneyEnd);yield return Shot("journey-ending");
            Invoke("SetScreen",GameScreen.SaveRecovery);yield return Shot("save-recovery");
        }
        [UnityTest] public IEnumerator RetiredCatalogueAndLanguageConfirmationStayOutOfTheCurrentFlow()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            game.ShowDevices();yield return null;
            Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Previous example"||b.name=="Next example"),Is.False);
            Assert.That(game.GetComponentsInChildren<RectTransform>().Any(r=>r.name=="Device example"),Is.True,"Contextual effect help remains available");
            game.CloseMenu();game.ShowSettings();game.ProposePuzzleLocale("uk");game.ConfirmPuzzleLocale();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Settings));
            Assert.That(game.UiLanguage,Is.EqualTo("uk"));Assert.That(game.Preferences.PuzzleLocale,Is.EqualTo("uk"));
            Assert.That(game.ProposedPuzzleLocale,Is.Null);
            Invoke("SetScreen",GameScreen.WordLanguage);yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Settings));Assert.That(Button("GameLanguageuk"),Is.Not.Null);
            Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Usefornewpuzzles"||b.name=="Keepmychoice"),Is.False);
            var saved=new ProfileStore(game.ProfilePath).Load().Profile;
            Assert.That(saved.Preferences.UiLocale,Is.EqualTo("uk"));Assert.That(saved.Preferences.PuzzleLocale,Is.EqualTo("uk"));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator GroundedMapFooterFitsAllWorldsOnPhonesAndTablet()
        {
            Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
            var measure=EditorType("MapPolishReview").GetMethod("Measure",BindingFlags.Static|BindingFlags.NonPublic);
            var capture=EditorType("PlayableCapture").GetMethod("CaptureSettled");
            game.SetEditorSafeInsets(new Vector4(.02f,.055f,.02f,.05f));
            for(int world=0;world<9;world++){
                EditorType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});
                foreach(bool large in new[]{false,true}){
                    game.Preferences.LargeText=large;game.SetUiLocale(large?"ru":"en");yield return null;Invoke("SetScreen",GameScreen.JourneyMap);
                    foreach(var size in new[]{new Vector2Int(320,693),new Vector2Int(390,844),new Vector2Int(768,1024)}){
                        string name="world-"+world+(large?"-ru-large":"-en")+"-"+size.x+"x"+size.y;
                        yield return (IEnumerator)capture.Invoke(null,new object[]{"MapLandingCleanupOct15",name+".png",size.x,size.y,(Action<CascadeGame>)(g=>measure.Invoke(null,new object[]{g,name,true}))});
                    }
                }
            }
        }
        [UnityTest] public IEnumerator IllustratedMapFooterAndCompletedRibbonsFitPhoneLanguages()
        {
            Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
            var capture=EditorType("PlayableCapture").GetMethod("CaptureSettled");
            var measure=EditorType("MapPolishReview").GetMethod("Measure",BindingFlags.Static|BindingFlags.NonPublic);
            var metrics=new List<string>();string hash=game.Interaction.Attempt.CurrentStateHash;
            game.ReducedMotion=true;game.Machine.SetAmbientPaused(true);game.SetEditorSafeInsets(new Vector4(.02f,.055f,.02f,.05f));
            IEnumerator Shot(string name,int width,int height)
            {
                yield return new WaitForSecondsRealtime(.4f);
                yield return (IEnumerator)capture.Invoke(null,new object[]{"MapControlsOct15",name+".png",width,height,(Action<CascadeGame>)(g=>{
                    metrics.Add(measure.Invoke(null,new object[]{g,name,true}).ToString());
                    var images=g.GetComponentsInChildren<Image>();var wood=Resources.Load<Sprite>("CascadeArt/HomeOct13/PlayButton");
                    var face=images.Single(i=>i.name=="Reference map continue wooden frame");Assert.That(face.sprite,Is.SameAs(wood));Assert.That(face.type,Is.EqualTo(Image.Type.Sliced));
                    Assert.That(g.GetComponentsInChildren<MapModeSurface>().Length,Is.EqualTo(2),"One cream mode surface and one green selection, without wooden borders");
                    Assert.That(images.Any(i=>i.name=="Reference map mode wooden frame"||i.name=="Reference map selected mode"),Is.False,"Mode controls must not reuse baked wooden borders");
                    Assert.That(images.Any(i=>i.name=="Reference map mode board"),Is.False,"One wide selector surround, with no smaller nested plaque");
                    foreach(var text in g.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop map completed "))){
                        Assert.That(text.transform.parent.name,Does.StartWith("Reference map completed ribbon "));
                        var parent=(RectTransform)text.transform.parent;var corners=new Vector3[4];text.rectTransform.GetWorldCorners(corners);
                        foreach(var corner in corners){var point=parent.InverseTransformPoint(corner);Assert.That(parent.rect.Contains(point),Is.True,"Completed caption must stay on its own ribbon");}
                    }
                })});
            }
            for(int world=0;world<9;world++){
                EditorType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});
                typeof(CascadeGame).GetField("selectedJourneyStop",Private).SetValue(game,Math.Min(JourneyStory.WorldFirst(world)+18,game.Catalog.PrimaryLevels.Count-1));
                foreach(string locale in new[]{"en","ru","uk"}){
                    game.Preferences.LargeText=locale!="en";game.SetUiLocale(locale);Invoke("SetScreen",GameScreen.JourneyMap);
                    foreach(var size in new[]{new Vector2Int(320,693),new Vector2Int(390,844)})
                        yield return Shot("world-"+world+"-"+locale+(game.Preferences.LargeText?"-large":"")+"-"+size.x+"x"+size.y,size.x,size.y);
                }
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            EditorType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,0});
            typeof(CascadeGame).GetField("selectedJourneyStop",Private).SetValue(game,18);game.Preferences.LargeText=false;game.SetGameLanguage("ru");Invoke("SetScreen",GameScreen.JourneyMap);
            yield return Shot("reference-ru-720x1280",720,1280);yield return Shot("tablet-ru-768x1024",768,1024);
            foreach(var mode in new[]{PuzzleMode.Words,PuzzleMode.Mixed,PuzzleMode.Shapes}){
                Button("Mapmode"+mode).onClick.Invoke();float end=Time.realtimeSinceStartup+20;
                while((!game.PreparationReady||game.CurrentScreen!=GameScreen.JourneyMap)&&Time.realtimeSinceStartup<end)yield return null;
                Assert.That(game.PreferredMode,Is.EqualTo(mode));Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
                yield return Shot("selected-mode-"+mode+"-ru",390,844);
            }
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","MapControlsOct15");
            File.WriteAllText(Path.Combine(folder,"layout-metrics.json"),"["+string.Join(",\n",metrics)+"]");
        }
        [UnityTest] public IEnumerator GameplayIslandMeshesDoNotGhostThroughMenuTransitions()
        {
            game.ReducedMotion=true;game.Machine.SetAmbientPaused(true);game.SetUiLocale("ru");EarnFirstEighteen();
            game.SetEditorSafeInsets(new Vector4(.02f,.055f,.02f,.05f));var world=game.Machine.LivingWorld;
            var islands=world.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("Floating world island ")).ToArray();
            var gameplayScenery=world.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("Floating world island ")||r.name.StartsWith("Distant travelling aircraft ")||r.name=="Travelling glider"||r.name=="Glass Lagoon sky skiff"||r.name=="Thematic level framing").ToArray();
            game.ShowGame();world.Paint(16);Assert.That(islands.Count(i=>i.gameObject.activeSelf),Is.EqualTo(2));
            Invoke("SetScreen",GameScreen.Pause);Assert.That(gameplayScenery.All(i=>!i.gameObject.activeSelf),Is.True,"A paused menu must hide all gameplay scenery without waiting for an ambient update");
            game.ShowGame();world.Paint(16);
            game.ShowJourney();Assert.That(islands.All(i=>!i.gameObject.activeSelf),Is.True,"Hide gameplay scenery immediately, even while ambient motion is paused");
            game.Machine.SetAmbientPaused(true);Assert.That(gameplayScenery.All(i=>!i.gameObject.activeSelf),Is.True);
            world.Paint(16);Assert.That(islands.All(i=>!i.gameObject.activeSelf),Is.True,"Background updates must not reactivate gameplay islands on a menu");
            var group=Read<RectTransform>("menuRoot").GetComponent<CanvasGroup>();group.alpha=.7f;
            var capture=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable.PlayableCapture")).First(t=>t!=null).GetMethod("CaptureSettled");
            IEnumerator Shot(string name,bool forceOld)=>(IEnumerator)capture.Invoke(null,new object[]{"MapControlsOct15",name+".png",390,844,(Action<CascadeGame>)(g=>{
                foreach(var image in islands)image.gameObject.SetActive(forceOld&&(image.name.EndsWith(" 2")||image.name.EndsWith(" 3")));
            })});
            string hash=game.Interaction.Attempt.CurrentStateHash;
            yield return Shot("ghost-positive-control-old-islands",true);yield return Shot("ghost-fixed-no-gameplay-islands",false);group.alpha=1;
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            game.ShowGame();world.Paint(16);Assert.That(islands.Count(i=>i.gameObject.activeSelf),Is.EqualTo(2),"Returning to gameplay restores its scenery");
        }
        [UnityTest] public IEnumerator CornerFlowersAnimateAndLongHeadersRespectMobileSafeAreas()
        {
            Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
            var review=EditorType("MapPolishReview");var measure=review.GetMethod("Measure",BindingFlags.Static|BindingFlags.NonPublic);
            var capture=EditorType("PlayableCapture").GetMethod("CaptureSettled");
            game.SetEditorSafeInsets(new Vector4(.02f,.055f,.02f,.05f));game.Preferences.LargeText=true;game.SetUiLocale("ru");
            foreach(int world in new[]{5,8}){
                EditorType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});
                typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{GameScreen.JourneyMap});
                yield return new WaitForSecondsRealtime(.4f);
                foreach(var size in new[]{new Vector2Int(320,693),new Vector2Int(390,844)}){
                    string name="safe-world-"+world+"-"+size.x+"x"+size.y;
                    Action<CascadeGame> check=g=>measure.Invoke(null,new object[]{g,name,true});
                    yield return (IEnumerator)capture.Invoke(null,new object[]{"MapPolishNativeExtraOct15",name+".png",size.x,size.y,check});
                }
            }
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","MapPolishNativeExtraOct15");Directory.CreateDirectory(folder);
            var motion=review.GetMethod("CornerMotion",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{game,folder});
            File.WriteAllText(Path.Combine(folder,"corner-motion.json"),motion.ToString()+"\n");
        }
    }
}
