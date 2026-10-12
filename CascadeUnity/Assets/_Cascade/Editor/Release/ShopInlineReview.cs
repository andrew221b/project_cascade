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
    public static class ShopInlineReview
    {
        const string Prefix="Cascade.ShopInlineReview.";
        const string Phase="ShopInline-Oct10";
        public static void BeginBatch()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
            LocalizationAssets.Build();SessionState.SetBool(Prefix+"Batch",true);Begin();
        }
        static ShopInlineReview()
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
                runner.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,testNames=new[]{"Cascade.Tests.RepairKitShopTests","Cascade.Tests.CloudReconcilerTests","Cascade.Tests.StageHelpEconomyTests","Cascade.Tests.RewardTests","Cascade.Tests.HintLampPolicyTests","Cascade.Tests.AdServiceTests","Cascade.Tests.ShopTests","Cascade.Tests.CoinEconomyTests","Cascade.Tests.MechanicLessonTests","Cascade.Tests.ScreenCosmeticTests","Cascade.Tests.TrophyEconomyTests","Cascade.Tests.WardrobeParcelTests"}}));
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
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        static IEnumerator Until(Func<bool> condition,string failure,float seconds=18)
        {
            float deadline=Time.realtimeSinceStartup+seconds;
            while(!condition()){Check(Time.realtimeSinceStartup<deadline,failure);yield return null;}
        }
        static LevelDefinition Fixture(string id,int width=4,int floors=1)
        {
            var row=new[]{"square","circle","triangle","diamond","star"}.Take(width).Select(x=>SymbolId.Parse("s:"+x)).ToArray();
            return new LevelDefinition(id,"C02","2.0","2.0","review",row,row.Reverse(),new[]{new TargetSegment(SegmentKind.Shapes,0,width)},Enumerable.Range(0,floors).Select(i=>new FloorDefinition(8,Enumerable.Repeat(true,width-1))));
        }
        static Button FindButton(CascadeGame g,string name)=>g.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        static TMPro.TMP_Text FindText(CascadeGame g,string name)=>g.GetComponentsInChildren<TMPro.TMP_Text>().Single(t=>t.name==name);
        static void CheckStack(CascadeGame g)
        {
            var root=g.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Selected catalogue information");
            RectTransform previous=null;
            foreach(RectTransform child in root){
                var corners=new Vector3[4];child.GetWorldCorners(corners);
                if(previous!=null){var above=new Vector3[4];previous.GetWorldCorners(above);Check(root.InverseTransformPoint(above[0]).y>=root.InverseTransformPoint(corners[1]).y-.25f,"Catalogue stack overlap: "+child.name);}
                var bounds=new Vector3[4];root.GetWorldCorners(bounds);Check(root.InverseTransformPoint(corners[0]).y>=root.InverseTransformPoint(bounds[0]).y-.25f,"Catalogue button fell out of frame");
                previous=child;
            }
            foreach(var label in root.GetComponentsInChildren<TMPro.TMP_Text>()){
                label.ForceMeshUpdate();Check(!label.isTextOverflowing,"Catalogue text overflow: "+label.name+" / "+label.text);
            }
        }
        static IEnumerator Run(CascadeGame g)
        {
            Directory.CreateDirectory(Output);g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.Preferences.NotificationsEnabled=false;
            g.Preferences.Tutorials=false;g.Preferences.ReducedMotion=true;g.ReducedMotion=true;g.SetUiLocale("en");
            var camera=Camera.main;var oldTarget=camera.targetTexture;var portrait=new RenderTexture(390,844,24);portrait.Create();camera.targetTexture=portrait;
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(g);
            var economy=(AdService)typeof(CascadeGame).GetProperty("GameEconomy",Private).GetValue(g);
            g.ShowShop();yield return null;
            Check(g.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Shop object "))==6,"Shop did not contain six tools");
            var top=(RectTransform)FindButton(g,"Shop object 0").transform;var bottom=(RectTransform)FindButton(g,"Shop object 3").transform;
            Check(Mathf.Abs(top.anchorMin.x-bottom.anchorMin.x)<.001f,"Second grid row shifted");
            Check(!g.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.text=="Look closer"),"Shop kept extra description step");
            Check(FindButton(g,"Buy selected item").interactable,"Poor wallet silently disabled Buy");
            FindButton(g,"Buy selected item").onClick.Invoke();yield return null;
            Check(economy.Ledger.SpentCoins==0&&g.Stock(TravelTool.Wayfinder)==0,"Insufficient buy spent inventory");
            Check(FindText(g,"Purchase limits").text.Contains((120-g.ShopCoins).ToString())&&FindText(g,"Wallet coins").color.r>.7f,"Missing shortfall/wallet feedback: "+FindText(g,"Purchase limits").text+" / "+FindText(g,"Wallet coins").color);
            yield return PlayableCapture.CaptureSettled(Phase,"shop-insufficient.png",390,844);CheckStack(g);
            economy.Ledger.BonusCoins=100;g.NotifyStoreChanged();g.SelectShopItem(1);yield return null;
            Check(FindText(g,"Selected object title").text=="Undo spool","Selection did not update inline parchment");
            FindButton(g,"Buy selected item").onClick.Invoke();yield return null;
            Check(g.Stock(TravelTool.UndoSpool)==1&&g.ShopCoins==20,"Inline purchase did not charge exact price");
            Check(g.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Shop object "))==6,"Purchase left the grid");
            economy.Ledger.BonusCoins=500;g.NotifyStoreChanged();g.SelectShopItem(5);FindButton(g,"Buy selected item").onClick.Invoke();yield return null;
            Check(g.Stock(TravelTool.RepairKit)==1&&economy.Ledger.SpentCoins==220,"Sixth tool buy failed");
            yield return PlayableCapture.CaptureSettled(Phase,"shop-six-tools.png",390,844);CheckStack(g);
            g.ShowSupplies();yield return PlayableCapture.CaptureSettled(Phase,"bag-six-tools.png",390,844);CheckStack(g);
            foreach(string locale in new[]{"ru","uk"}){
                g.SetUiLocale(locale);g.SelectShopItem(5);
                camera.targetTexture=null;portrait.Release();UnityEngine.Object.Destroy(portrait);portrait=new RenderTexture(320,693,24);portrait.Create();camera.targetTexture=portrait;
                yield return PlayableCapture.CaptureSettled(Phase,"bag-compact-"+locale+".png",320,693);CheckStack(g);
                g.ShowShop();g.SelectShopItem(5);yield return PlayableCapture.CaptureSettled(Phase,"shop-compact-"+locale+".png",320,693);CheckStack(g);g.ShowSupplies();
            }
            g.SetUiLocale("en");g.LoadLevel(Fixture("kit-native-review"));g.ShowGame();yield return Until(()=>g.Interaction.CanPlay,"Kit board blocked");
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(lesson!=MechanicLesson.RepairKit&&!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
            profile.SeenMechanics.Remove(MechanicLesson.RepairKit);g.Preferences.Tutorials=true;g.ToggleFieldToolTray();yield return null;
            var kit=FindButton(g,"Field tool 5");int hp=g.Interaction.Attempt.Current.Durability;var row=g.Interaction.Attempt.Current.Row.ToArray();
            kit.onClick.Invoke();Check(g.MechanicPopupOpen&&g.Stock(TravelTool.RepairKit)==1,"Repair film consumed stock");
            yield return PlayableCapture.CaptureSettled(Phase,"repair-kit-film.png",320,693);
            g.AcknowledgeMechanic();yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Kit animation left controls blocked");
            Check(g.Interaction.Attempt.Current.Durability==hp+2&&g.Stock(TravelTool.RepairKit)==0&&g.Interaction.Attempt.Current.Row.SequenceEqual(row),"Kit effect/stock mismatch");
            g.ShowShop();g.SelectShopTab(1);yield return null;Check(g.GetComponentsInChildren<Button>().Any(b=>b.name=="Buy selected item"),"Trophy shopping kept extra step");
            yield return PlayableCapture.CaptureSettled(Phase,"trophy-inline.png",320,693);CheckStack(g);
            camera.targetTexture=oldTarget;portrait.Release();UnityEngine.Object.Destroy(portrait);
            Check(SessionState.GetInt(Prefix+"Errors",0)==0,"Native review recorded errors");
            File.WriteAllText(Path.Combine(Output,"native-review.txt"),"PASS: actual six-card Shop/Bag grid, inline selection and purchase, exact prices, insufficient wallet feedback with no spending, EN/RU/UK compact layout/overflow, sixth-tool purchase, isolated repair film and actual kit application, direct trophy shopping. No device build/install or live purchase/advertisement test.");
            Debug.Log("CASCADE inline shop review passed.");SessionState.SetBool(Prefix+"Finishing",true);EditorApplication.update+=Finish;EditorApplication.isPlaying=false;
        }
    }
}
