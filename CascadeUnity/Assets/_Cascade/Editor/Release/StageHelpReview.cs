using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Presentation;
using Cascade.Editor.Playable;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Release
{
    // Real UI callbacks, isolated temporary save; never touches the device profile.
    [InitializeOnLoad]
    public static class StageHelpReview
    {
        const string Prefix="Cascade.StageHelpReview.";
        const string Phase="StageHelp-Oct10";
        public static void BeginBatch()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
            ConfigureArt();SessionState.SetBool(Prefix+"Batch",true);Begin();
        }
        static StageHelpReview()
        {
            if(SessionState.GetBool(Prefix+"Finishing",false)){EditorApplication.update+=Finish;return;}
            if(!SessionState.GetBool(Prefix+"Auto",false))return;
            // A source reload during this isolated review restarts its fixtures.
            SessionState.SetBool(Prefix+"Started",false);UnityEngine.Application.logMessageReceived+=RecordError;EditorApplication.update+=Tick;
        }
        static void Finish()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)return;
            EditorApplication.update-=Finish;SessionState.SetBool(Prefix+"Finishing",false);RestoreSession();
            if(SessionState.GetBool(Prefix+"Batch",false)) {
                SessionState.SetBool(Prefix+"Batch",false);
                runner=ScriptableObject.CreateInstance<TestRunnerApi>();runner.hideFlags=HideFlags.HideAndDontSave;
                runner.RegisterCallbacks(new MathResults());
                runner.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,testNames=new[]{"Cascade.Tests.StageHelpEconomyTests","Cascade.Tests.RewardTests","Cascade.Tests.HintLampPolicyTests","Cascade.Tests.AdServiceTests","Cascade.Tests.ShopTests","Cascade.Tests.CoinEconomyTests","Cascade.Tests.MechanicLessonTests","Cascade.Tests.ScreenCosmeticTests","Cascade.Tests.TrophyEconomyTests","Cascade.Tests.WardrobeParcelTests"}}));
            }
        }
        static TestRunnerApi runner;
        sealed class MathResults:ICallbacks
        {
            public void RunStarted(ITestAdaptor tests){}public void TestStarted(ITestAdaptor test){}public void TestFinished(ITestResultAdaptor result){}
            public void RunFinished(ITestResultAdaptor result) {
                TestRunnerApi.SaveResultToFile(result,Path.Combine(Output,"editmode-final.xml"));
                Debug.Log("CASCADE receiver math: "+result.PassCount+" passed; "+result.FailCount+" failed.");
                EditorApplication.Exit(result.FailCount==0?0:2);
            }
        }
        public static void Begin()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            SessionState.SetString(Prefix+"OldProfile",SessionState.GetString("Cascade.NavigationReviewProfile",""));
            SessionState.SetBool(Prefix+"OldBackground",UnityEngine.Application.runInBackground);
            NavigationCapture.Prepare();UnityEngine.Application.runInBackground=true;
            SessionState.SetInt(Prefix+"Errors",0);SessionState.SetFloat(Prefix+"Began",(float)EditorApplication.timeSinceStartup);
            UnityEngine.Application.logMessageReceived-=RecordError;UnityEngine.Application.logMessageReceived+=RecordError;
            SessionState.SetBool(Prefix+"Auto",true);SessionState.SetBool(Prefix+"Started",false);
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
        }
        static void RecordError(string text,string trace,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)SessionState.SetInt(Prefix+"Errors",SessionState.GetInt(Prefix+"Errors",0)+1);}
        static void Tick()
        {
            if(SessionState.GetBool(Prefix+"Batch",false)&&EditorApplication.timeSinceStartup-SessionState.GetFloat(Prefix+"Began",0)>240){
                Debug.LogError("CASCADE receiver review timed out; no successful validation.");EditorApplication.Exit(2);return;
            }
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))return;
            EditorApplication.QueuePlayerLoopUpdate();
            if(SessionState.GetBool(Prefix+"Started",false))return;
            SessionState.SetBool(Prefix+"Started",true);game.StartCoroutine(Run(game));
        }
        public static void RestoreSession()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            UnityEngine.Application.logMessageReceived-=RecordError;EditorApplication.update-=Tick;SessionState.SetBool(Prefix+"Auto",false);SessionState.SetBool(Prefix+"Started",false);
            string old=SessionState.GetString(Prefix+"OldProfile","");
            if(old.Length==0)NavigationCapture.Clear();else{SessionState.SetString("Cascade.NavigationReviewProfile",old);CascadeGame.EditorProfileDirectory=old;}
            UnityEngine.Application.runInBackground=SessionState.GetBool(Prefix+"OldBackground",false);
            SessionState.EraseString(Prefix+"OldProfile");
        }
        const System.Reflection.BindingFlags Private=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        static string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase));
        static void ConfigureArt()
        {
            const string path="Assets/_Cascade/Resources/CascadeArt/Cosmetics/ParcelBrooches.png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
            LocalizationAssets.Build();
        }
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        static IEnumerator Until(Func<bool> condition,string failure,float seconds=18)
        {
            float deadline=Time.realtimeSinceStartup+seconds;
            while(!condition()){Check(Time.realtimeSinceStartup<deadline,failure);yield return null;}
        }
        static LevelDefinition Fixture(string id,int width=4,int floors=1)
        {
            var row=new[]{"square","circle","triangle","diamond","star"}.Take(width).Select(x=>SymbolId.Parse("s:"+x)).ToArray();
            return new LevelDefinition(id,"C02","2.0","2.0","review",row,row.Reverse(),new[]{new TargetSegment(SegmentKind.Shapes,0,width)},Enumerable.Range(0,floors).Select(i=>new FloorDefinition(12,Enumerable.Repeat(true,width-1))));
        }
        static IEnumerator Run(CascadeGame g)
        {
            Directory.CreateDirectory(Output);g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.Preferences.NotificationsEnabled=false;
            g.Preferences.Tutorials=false;g.Preferences.ReducedMotion=true;g.ReducedMotion=true;g.SetUiLocale("en");
            var camera=Camera.main;var oldTarget=camera.targetTexture;var portrait=new RenderTexture(390,844,24);portrait.Create();camera.targetTexture=portrait;
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(g);
            var economy=(AdService)typeof(CascadeGame).GetProperty("GameEconomy",Private).GetValue(g);
            var level=Fixture("hint-tray-review",5,2);g.LoadLevel(level);g.ShowGame();yield return Until(()=>g.Interaction.CanPlay,"Hint fixture blocked");
            var bulb=g.GetComponentsInChildren<Button>().Single(b=>b.name=="Hint");
            for(int n=0;n<2;n++){
                string before=g.Interaction.Attempt.CurrentStateHash;
                for(int i=0;i<100;i++)bulb.onClick.Invoke();
                yield return Until(()=>g.HintVisible,"Hint did not show");
                Check(g.Interaction.Attempt.CurrentStateHash==before,"Hint moved row");
                var command=AttemptService.CalculateHint(g.Interaction.Attempt.CaptureHintRequest()).Result.Command.Value;
                Check(g.Interaction.Execute(command).Accepted,"Manual recommended move rejected");
                yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Hint follow-up hung");
            }
            Check(g.Interaction.Attempt.FreeHintsRemaining==0,"Counter not exhausted");
            Check(!bulb.interactable,"Exhausted bulb active without video/token");
            string exhausted=g.Interaction.Attempt.CurrentStateHash;for(int n=0;n<100;n++)bulb.onClick.Invoke();yield return null;
            Check(g.Interaction.Attempt.CurrentStateHash==exhausted&&!g.HintVisible,"Empty hints changed state");
            g.Undo();yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Undo after hints hung");
            bulb.onClick.Invoke();yield return Until(()=>g.HintVisible,"Granted hint not restored by Undo");
            Check(g.Interaction.Attempt.FreeHintsRemaining==0,"Undo refilled hints");

            // Verified reward gives inventory only. The player's next bulb tap spends one token for information.
            var cached=AttemptService.CalculateHint(g.Interaction.Attempt.CaptureHintRequest());g.Interaction.Execute(cached.Result.Command.Value);
            yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Cached hint move stuck");
            var ticket=economy.TryRewarded(new AdContext{Surface=AdSurface.Inventory,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true},"hint",true);
            Check(ticket!=null&&economy.Opened(ticket.Id)&&economy.RewardConfirmed(ticket.Id,"native_hint_"+ticket.Id,true),"Verified hint reward failed");economy.Closed(ticket.Id);g.NotifyStoreChanged();Check(bulb.interactable,"Reward readiness did not refresh the bulb");
            int receipts=economy.Ledger.Grants.Count;string paidBefore=g.Interaction.Attempt.CurrentStateHash;g.RequestHint();
            yield return Until(()=>g.HintVisible,"Extra hint did not show");
            Check(economy.Ledger.HintTokens==0&&g.Interaction.Attempt.CurrentStateHash==paidBefore,"Extra hint spent incorrectly or executed move");
            var loaded=AttemptService.Restore(level,AttemptRecord.Capture(g.Interaction.Attempt));
            typeof(CascadeGame).GetMethod("MountAttempt",Private).Invoke(g,new object[]{loaded,null});g.ShowGame();
            yield return Until(()=>g.Interaction.CanPlay,"Reloaded extra hint blocked");g.RequestHint();
            yield return Until(()=>g.HintVisible,"Reload demanded another video for granted state");
            Check(economy.Ledger.HintTokens==0&&economy.Ledger.Grants.Count==receipts,"Reload paid twice");
            var afterPaid=AttemptService.CalculateHint(g.Interaction.Attempt.CaptureHintRequest());g.Interaction.Execute(afterPaid.Result.Command.Value);
            yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Extra-hint follow-up stuck");
            Check(!bulb.interactable,"More than one rewarded hint allowed this stage");

            // Opening/closing an unfamiliar tool film never spends the item. Confirmation performs its effect.
            economy.Ledger.LandingLenses=2;economy.Ledger.WayfinderScrolls=2;economy.Ledger.SignalLanterns=2;
            g.LoadLevel(Fixture("lens-confirm-review"));g.ShowGame();yield return Until(()=>g.Interaction.CanPlay,"Lens fixture blocked");
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(lesson!=MechanicLesson.LandingLens&&!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
            g.Preferences.Tutorials=true;profile.SeenMechanics.Remove(MechanicLesson.LandingLens);g.ToggleFieldToolTray();yield return null;
            var lens=g.GetComponentsInChildren<Button>().Single(b=>b.name=="Field tool 4");int stock=g.Stock(TravelTool.LandingLens);string hash=g.Interaction.Attempt.CurrentStateHash;
            lens.onClick.Invoke();Check(g.MechanicPopupOpen&&g.Stock(TravelTool.LandingLens)==stock,"Tool film consumed inventory");
            g.DismissMechanic();Check(!g.MechanicPopupOpen&&g.Stock(TravelTool.LandingLens)==stock,"Closing film consumed inventory");
            lens.onClick.Invoke();yield return Until(()=>g.GetComponentsInChildren<Button>().Any(b=>b.name=="Close landing preview"),"Lens did not open prediction");
            Check(g.Stock(TravelTool.LandingLens)==stock-1&&g.Interaction.Attempt.CurrentStateHash==hash,"Lens changed puzzle or stock incorrectly");
            yield return PlayableCapture.CaptureSettled(Phase,"lens-preview.png",390,844);
            g.CloseLandingPreview();yield return Until(()=>g.Interaction.CanPlay,"Lens close left input blocked");
            g.ToggleFieldToolTray();var scroll=g.GetComponentsInChildren<Button>().Single(b=>b.name=="Field tool 0");scroll.onClick.Invoke();
            yield return Until(()=>g.Interaction.Attempt.WayfinderUsed&&g.HintVisible&&!g.Interaction.IsPresenting,"Wayfinder had no result");
            Check(g.Interaction.Attempt.CurrentStateHash==hash&&g.Interaction.Attempt.ToolUsesRemaining==0,"Guide moved row or ignored cap");
            g.ToggleFieldToolTray();g.GetComponentsInChildren<Button>().Single(b=>b.name=="Field tool 3").onClick.Invoke();
            Check(g.Stock(TravelTool.SignalLantern)==2,"Unavailable tool spent stock");
            yield return PlayableCapture.CaptureSettled(Phase,"lower-controls.png",390,844);
            g.ToggleFieldToolTray();g.ReplayStageLessons();Check(g.MechanicPopupOpen&&!g.PendingLesson.HasValue,"Replay enabled guided practice");
            var overlay=(RectTransform)typeof(CascadeGame).GetField("mechanicOverlay",Private).GetValue(g);
            var hands=overlay.GetComponentsInChildren<Image>().Where(i=>i.name=="Film tutorial hand above pieces").ToArray();
            Check(hands.Length>0&&hands.All(h=>!h.enabled),"Replay showed finger");
            g.AcknowledgeMechanic();Check(!g.MechanicPopupOpen&&!g.PendingLesson.HasValue&&g.Interaction.CanPlay,"Replay did not release input");
            // A mechanic already taught in another world is shared, rather than rediscovered.
            profile.SeenMechanics.Remove(MechanicLesson.Repair);
            profile.ModeJourneys.Clear();profile.ModeJourneys.Add(new ModeJourney{Mode="Words",SeenMechanics=new System.Collections.Generic.List<MechanicLesson>{MechanicLesson.Repair}});
            var row=new[]{"square","circle","triangle"}.Select(x=>SymbolId.Parse("s:"+x)).ToArray();
            var repair=new LevelDefinition("known-repair","C02","2.0","2.0","review",row,row.Reverse(),new[]{new TargetSegment(SegmentKind.Shapes,0,3)},new[]{new FloorDefinition(10,new[]{true,true},repair:new RepairDefinition(0,row[0],2))});
            g.LoadLevel(repair);g.ShowGame();yield return Until(()=>g.Interaction.CanPlay,"Known mechanic blocked");
            for(int i=0;i<20;i++)yield return null;Check(!g.MechanicPopupOpen,"Known mechanic repeated across modes");
            for(int i=0;i<8;i++){
                var art=(Sprite)typeof(CascadeGame).GetMethod("ParcelBrooch",Private).Invoke(g,new object[]{ShopCatalog.ParcelTrophyIds[i]});
                Check(art!=null&&art.rect.width>100,"Missing parcel artwork");
            }
            g.ShowCollection();g.SelectShopItem(0);typeof(CascadeGame).GetMethod("OpenSelectedShopItem",Private).Invoke(g,null);yield return null;
            var trophyAction=g.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action");
            Check(trophyAction.GetComponentInChildren<TMPro.TMP_Text>().text=="Find in shop","Obsolete fragment selection survived new parcel recipe");
            camera.targetTexture=oldTarget;portrait.Release();UnityEngine.Object.Destroy(portrait);
            File.WriteAllText(Path.Combine(Output,"native-review.txt"),"PASS: 300 native bulb callbacks; two-step stage budget; no auto-play; exhausted control, Undo cache; tool tutorial close without spending; lens preview and close; Wayfinder visible/manual-only; tool cap without inventory loss; round Undo/lower Hint; passive question-mark replay without hand; known lesson across worlds; eight sprite resources. Paid hint token, zero-wallet reload without second video, one rewarded state cap. No iPhone build or live ad test.");
            Debug.Log("CASCADE stage help review passed.");SessionState.SetBool(Prefix+"Finishing",true);EditorApplication.update+=Finish;EditorApplication.isPlaying=false;
        }
    }
}
