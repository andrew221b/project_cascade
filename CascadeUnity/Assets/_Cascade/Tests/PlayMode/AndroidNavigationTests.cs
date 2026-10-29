#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cascade.Application;
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
    public sealed class AndroidNavigationTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;string directory;bool background,audio;Camera camera;RenderTexture portrait,oldTarget;
        T Read<T>(string name)=>(T)typeof(CascadeGame).GetField(name,Private).GetValue(game);
        void Set(string name,object value)=>typeof(CascadeGame).GetField(name,Private).SetValue(game,value);
        void Invoke(string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Private).Invoke(game,args);
        Button Button(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeAndroidNavigation",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=directory;CascadeGame.EditorAudienceAge=AudienceAgeBand.SixteenPlus;
            var profile=new PlayerProfile{AccountIntroSeen=true};profile.Missions.NextAfterClears=20000;
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))profile.SeenMechanics.Add(lesson);
            new ProfileStore(Path.Combine(directory,"cascade-profile.json")).Save(profile);
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Assert.That(game,Is.Not.Null);
            camera=Camera.main;oldTarget=camera.targetTexture;portrait=new RenderTexture(390,844,24);
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=portrait;scaler.enabled=true;game.RefreshLayout();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Preferences.ReducedMotion=true;game.ReducedMotion=true;
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            Read<System.Collections.Generic.HashSet<int>>("heardStory").UnionWith(Enumerable.Range(0,10000));yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;
            ProfileStore store=game!=null?Read<ProfileStore>("profileStore"):null;
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            store?.Flush();
            if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}
            CascadeGame.EditorProfileDirectory=null;CascadeGame.EditorAudienceAge=null;
            UnityEngine.Application.runInBackground=background;AudioListener.pause=audio;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Ready()
        {
            float deadline=Time.realtimeSinceStartup+25;
            while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(game.Interaction.CanPlay,Is.True);
        }
        [UnityTest] public IEnumerator BackPausesThenResumesWithoutChangingAttempt()
        {
            game.ShowGame();yield return Ready();string hash=game.Interaction.Attempt.CurrentStateHash;
            Assert.That(game.HandleSystemBack(),Is.True);Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Pause));
            Assert.That(game.HandleSystemBack(),Is.True);Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator BackClosesBagBeforePausing()
        {
            game.ShowGame();yield return Ready();game.ToggleFieldToolTray();yield return null;
            Assert.That(game.FieldToolTrayOpen,Is.True);game.HandleSystemBack();Assert.That(game.FieldToolTrayOpen,Is.False);
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));game.HandleSystemBack();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Pause));
        }
        [UnityTest] public IEnumerator BackDismissesToolLessonWithoutUsingItem()
        {
            Read<PlayerProfile>("profile").SeenMechanics.Remove(MechanicLesson.RepairKit);
            var row=new[]{SymbolId.Parse("s:C"),SymbolId.Parse("s:B"),SymbolId.Parse("s:A")};
            game.LoadLevel(new LevelDefinition("back-tool","back-tool","1","2.0","back-tool",row,row.Reverse().ToArray(),new[]{new TargetSegment(SegmentKind.Shapes,0,3)},new[]{new FloorDefinition(8,new[]{true,true}),new FloorDefinition(8,new[]{true,true})}));
            game.ShowGame();yield return Ready();game.Monetization.Ledger.RepairKits=1;string hash=game.Interaction.Attempt.CurrentStateHash;
            game.ToggleFieldToolTray();Button("Field tool "+(int)TravelTool.RepairKit).onClick.Invoke();Button("Apply field tool").onClick.Invoke();yield return null;
            Assert.That(game.MechanicPopupOpen,Is.True);game.HandleSystemBack();Assert.That(game.MechanicPopupOpen,Is.False);
            Assert.That(game.Monetization.Ledger.RepairKits,Is.EqualTo(1));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator SettingsBackClosesSubpageThenReturnsToMap()
        {
            game.ShowJourney();game.ShowSettings();yield return null;Button("Comfort").onClick.Invoke();yield return null;
            Assert.That(Read<int>("settingsPage"),Is.EqualTo(1));game.HandleSystemBack();Assert.That(Read<int>("settingsPage"),Is.EqualTo(-1));
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Settings));game.HandleSystemBack();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
            game.HandleSystemBack();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
        }
        [UnityTest] public IEnumerator LegalBackClosesOnlyDocument()
        {
            game.ShowHome();game.ShowSettings();game.OpenLegalDocument("privacy");yield return null;
            Assert.That(Read<RectTransform>("legalOverlay"),Is.Not.Null);game.HandleSystemBack();Assert.That(Read<RectTransform>("legalOverlay"),Is.Null);
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Settings));
        }
        [UnityTest] public IEnumerator AgeGateBackDoesNotAnswerAgeOrCreateAccount()
        {
            Set("audienceAgeBand",AudienceAgeBand.Unknown);game.ShowAccountGate();yield return null;
            game.HandleSystemBack();Assert.That(game.AudienceAgeNeedsChoice,Is.True);Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.AccountGate));
        }
        [UnityTest] public IEnumerator NativeAdAndBackgroundCannotNavigateUnderlyingGame()
        {
            game.ShowHome();Set("nativeAdSuspended",true);Assert.That(game.HandleSystemBack(),Is.False);Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            Set("nativeAdSuspended",false);game.SendMessage("OnApplicationFocus",false);Assert.That(game.HandleSystemBack(),Is.False);
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",true);Assert.That(game.HandleSystemBack(),Is.False);
            game.SendMessage("OnApplicationPause",false);yield return null;
        }
        [UnityTest] public IEnumerator BusyPurchaseBlocksBackButDeferredApprovalDoesNotTrapPlayer()
        {
            game.ShowHome();game.ShowSettings();yield return null;
            var store=Read<NativePurchaseStore>("purchaseStore");Assert.That(store,Is.Not.Null);var state=store.State;
            var property=typeof(NativePurchaseStore).GetProperty("State");int balance=game.ShopCoins;
            try{
                property.SetValue(store,PurchaseState.Busy);Assert.That(game.HandleSystemBack(),Is.True);
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Settings));Assert.That(game.ShopCoins,Is.EqualTo(balance));
                property.SetValue(store,PurchaseState.Deferred);Assert.That(game.HandleSystemBack(),Is.True);
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That(game.ShopCoins,Is.EqualTo(balance));Assert.That(store.State,Is.EqualTo(PurchaseState.Deferred));
            }finally{property.SetValue(store,state);}
        }
        [UnityTest] public IEnumerator CollectionBackReturnsHomeWithoutChangingOwnership()
        {
            game.ShowHome();game.ShowCollection();yield return null;var rewards=CloudJson.Encode(game.Rewards);
            game.HandleSystemBack();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That(CloudJson.Encode(game.Rewards),Is.EqualTo(rewards));
        }
        [UnityTest] public IEnumerator WorldCompletionBackClosesCelebrationWithoutGrantingAnything()
        {
            game.ShowHome();Set("completedWorld",0);Invoke("SetScreen",GameScreen.WorldComplete);yield return null;
            int balance=game.ShopCoins;var ledger=CloudJson.Encode(game.Monetization.Ledger);
            game.HandleSystemBack();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            Assert.That(game.ShopCoins,Is.EqualTo(balance));Assert.That(CloudJson.Encode(game.Monetization.Ledger),Is.EqualTo(ledger));
        }
        [UnityTest] public IEnumerator IslandBackReturnsToMapBeforeHome()
        {
            game.ShowJourney();yield return null;Button("Island1").onClick.Invoke();yield return null;Assert.That(game.BrowsedIsland,Is.Zero);
            game.HandleSystemBack();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(game.BrowsedIsland,Is.EqualTo(-1));
            game.HandleSystemBack();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
        }
        [UnityTest] public IEnumerator EscapeInputIsRoutedOnceThroughTheSameBackHandler()
        {
            game.ShowGame();yield return Ready();var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            try{
                keyboard.MakeCurrent();typeof(UnityEngine.InputSystem.InputSystem).GetMethod("Update",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{UnityEngine.InputSystem.LowLevel.InputUpdateType.Dynamic});
                Assert.That(keyboard.escapeKey.wasPressedThisFrame,Is.False);
                UnityEngine.InputSystem.LowLevel.InputState.Change(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Escape),UnityEngine.InputSystem.LowLevel.InputUpdateType.Dynamic);
                Assert.That(keyboard.escapeKey.wasPressedThisFrame,Is.True,"Input fixture supplies a committed Escape");Invoke("PollSystemBack");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Pause));
                typeof(UnityEngine.InputSystem.InputSystem).GetMethod("Update",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{UnityEngine.InputSystem.LowLevel.InputUpdateType.Dynamic});UnityEngine.InputSystem.LowLevel.InputState.Change(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(),UnityEngine.InputSystem.LowLevel.InputUpdateType.Dynamic);
                Invoke("PollSystemBack");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Pause));
            }finally{UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);}
        }
        [UnityTest] public IEnumerator YellowAgeButtonStaysBlackAndFitsAllLanguages()
        {
            Set("audienceAgeBand",AudienceAgeBand.Unknown);game.ShowAccountGate();
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);while(Read<bool>("applyingUiLanguage"))yield return null;game.RefreshLayout();yield return null;Canvas.ForceUpdateCanvases();
                var no=Button("Age under 16 no").GetComponentInChildren<TMP_Text>();var yes=Button("Age 16 or older yes").GetComponentInChildren<TMP_Text>();
                no.ForceMeshUpdate();Assert.That(no.color,Is.EqualTo(Color.black),locale);Assert.That(yes.color,Is.EqualTo(Color.white),locale);
                Assert.That(no.isTextOverflowing||no.isTextTruncated,Is.False,locale);Assert.That(yes.isTextOverflowing||yes.isTextTruncated,Is.False,locale);
                Capture("age-black-"+locale);
            }
        }
        [UnityTest] public IEnumerator AndroidNavigationBarAndSideCutoutKeepControlsInsideSafeRegion()
        {
            foreach(var inset in new[]{new Vector4(0,.07f,0,.035f),new Vector4(.09f,.025f,.04f,.04f)}){
                game.SetEditorSafeInsets(inset);game.ShowHome();game.ShowSettings();yield return null;Canvas.ForceUpdateCanvases();
                var safe=Read<RectTransform>("safeRoot");var corners=new Vector3[4];safe.GetWorldCorners(corners);float bottom=corners[0].y,top=corners[2].y,left=corners[0].x,right=corners[2].x;
                foreach(var button in Read<RectTransform>("menuRoot").GetComponentsInChildren<Button>().Where(b=>b.interactable)){
                    ((RectTransform)button.transform).GetWorldCorners(corners);
                    Assert.That(corners[0].x,Is.GreaterThanOrEqualTo(left-.02f),button.name);Assert.That(corners[2].x,Is.LessThanOrEqualTo(right+.02f),button.name);
                    Assert.That(corners[0].y,Is.GreaterThanOrEqualTo(bottom-.02f),button.name);Assert.That(corners[2].y,Is.LessThanOrEqualTo(top+.02f),button.name);
                }
            }
        }
#if UNITY_ANDROID
        [UnityTest] public IEnumerator AndroidPauseFocusAndDisableDoNotWaitForBusySaveDisk()
        {
            var store=Read<ProfileStore>("profileStore");store.Flush();var disk=typeof(ProfileStore).GetField("disk",Private).GetValue(store);
            using(var acquired=new ManualResetEventSlim())using(var release=new ManualResetEventSlim()){
                var worker=Task.Run(()=>{lock(disk){acquired.Set();release.Wait(3000);}});Assert.That(acquired.Wait(2000),Is.True);
                var watch=System.Diagnostics.Stopwatch.StartNew();
                try{game.SendMessage("OnApplicationPause",true);game.SendMessage("OnApplicationFocus",false);game.SendMessage("OnDisable");}
                finally{watch.Stop();release.Set();worker.GetAwaiter().GetResult();}
                Assert.That(watch.ElapsedMilliseconds,Is.LessThan(1500),"Lifecycle must enqueue rather than wait for the disk lock");
            }
            store.Flush();game.SendMessage("OnEnable");game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);yield return null;
            Assert.That(store.Load().Recovery,Is.EqualTo(SaveRecovery.None));
        }
#endif
        void Capture(string name)
        {
            var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/AndroidNavigationOct29"));Directory.CreateDirectory(folder);
            Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=portrait;
            var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();
            File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.Destroy(pixels);
        }
    }
}
#endif
