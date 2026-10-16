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
    public static class ManualTrophyReview
    {
        const string Prefix="Cascade.ManualTrophyReview.";
        const string Phase="ManualTrophies-Oct10";
        public static void BeginBatch()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
            LocalizationAssets.Build();SessionState.SetBool(Prefix+"Batch",true);Begin();
        }
        static ManualTrophyReview()
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
            if(SessionState.GetBool(Prefix+"Batch",false)){
                SessionState.SetBool(Prefix+"Batch",false);runner=ScriptableObject.CreateInstance<TestRunnerApi>();runner.hideFlags=HideFlags.HideAndDontSave;
                runner.RegisterCallbacks(new Results());runner.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,testNames=new[]{"Cascade.Tests.ScreenCosmeticTests","Cascade.Tests.TrophyEconomyTests","Cascade.Tests.RewardTests","Cascade.Tests.WardrobeParcelTests","Cascade.Tests.CloudReconcilerTests"}}));
            }
        }
        static TestRunnerApi runner;
        sealed class Results:ICallbacks
        {
            public void RunStarted(ITestAdaptor t){}public void TestStarted(ITestAdaptor t){}public void TestFinished(ITestResultAdaptor r){}
            public void RunFinished(ITestResultAdaptor r){TestRunnerApi.SaveResultToFile(r,Path.Combine(Output,"editmode.xml"));Debug.Log("CASCADE manual trophies: "+r.PassCount+" passed; "+r.FailCount+" failed.");EditorApplication.Exit(r.FailCount==0?0:2);}
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
        static TrophyAmbiance Global(CascadeGame g)=>g.GetComponentsInChildren<TrophyAmbiance>(true).Single(v=>v.transform.parent.name=="Cosmetic atmosphere behind the interface");
        static void CheckRewardLayout(CascadeGame g)
        {
            var label=FindText(g,"Collected trophy location");label.ForceMeshUpdate();Check(!label.isTextOverflowing,"Trophy location overflow: "+label.text);
            var left=(RectTransform)FindButton(g,"Keepgift").transform;var right=(RectTransform)FindButton(g,"View collected trophy").transform;
            var a=new Vector3[4];var b=new Vector3[4];left.GetWorldCorners(a);right.GetWorldCorners(b);
            Check(a[2].x<b[0].x,"Reward buttons overlap");
            foreach(var button in new[]{FindButton(g,"Keepgift"),FindButton(g,"View collected trophy")}){
                var caption=button.GetComponentInChildren<TMPro.TMP_Text>();caption.ForceMeshUpdate();Check(!caption.isTextOverflowing,"Reward button text clipped");
            }
        }
        static IEnumerator Run(CascadeGame g)
        {
            Directory.CreateDirectory(Output);g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.Preferences.NotificationsEnabled=false;
            g.Preferences.Tutorials=false;g.Preferences.ReducedMotion=true;g.ReducedMotion=true;g.SetUiLocale("en");
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(g);
            var economy=(AdService)typeof(CascadeGame).GetProperty("GameEconomy",Private).GetValue(g);
            var nodes=g.Catalog.PrimaryLevels.Take(9).Select(c=>c.Definition.CampaignNodeId).ToArray();
            foreach(string node in nodes)profile.Clears.Add(new ClearReceipt{CampaignNodeId=node,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64)});
            g.Rewards.Synchronize(nodes);g.Rewards.Parcels.Clear();
            for(int order=1;order<=3;order++)g.Rewards.Parcels.Add(new ParcelReceipt{Order=order,GiftId="leaf",RecipeVersion=3});
            Check(economy.TryClaimParcel(1,g.Rewards)&&economy.TryClaimParcel(2,g.Rewards),"Could not claim partial leaves");
            Check(!g.OwnsGift("leaf")&&g.EquippedGift==null,"Partial leaves activated appearance");
            var camera=Camera.main;var oldTarget=camera.targetTexture;var portrait=new RenderTexture(390,844,24);portrait.Create();camera.targetTexture=portrait;
            g.ShowHome();g.ShowParcel();yield return null;FindButton(g,"Openparcelgift").onClick.Invoke();yield return null;
            FindButton(g,"Claimgift").onClick.Invoke();yield return null;
            Check(g.OwnsGift("leaf")&&g.EquippedGift==null&&Global(g).VisibleCount==0,"Third part automatically enabled leaves");
            yield return PlayableCapture.CaptureSettled(Phase,"leaf-collected.png",390,844);CheckRewardLayout(g);
            foreach(string locale in new[]{"ru","uk"}){
                camera.targetTexture=null;portrait.Release();UnityEngine.Object.Destroy(portrait);portrait=new RenderTexture(320,693,24);portrait.Create();camera.targetTexture=portrait;
                g.SetUiLocale(locale);g.Preferences.LargeText=true;g.RefreshLayout();
                yield return PlayableCapture.CaptureSettled(Phase,"leaf-collected-"+locale+".png",320,693);CheckRewardLayout(g);
            }
            FindButton(g,"View collected trophy").onClick.Invoke();yield return null;
            Check(g.CurrentScreen==GameScreen.Collection&&(int)typeof(CascadeGame).GetField("shopItem",Private).GetValue(g)==Array.IndexOf(ShopCatalog.TrophyIds,"leaf"),"Reward did not link to the leaf controls");
            FindButton(g,"Trophy action").onClick.Invoke();yield return null;
            Check(g.EquippedGift=="leaf"&&Global(g).VisibleCount>0,"Explicit enable did not start leaves");
            yield return PlayableCapture.CaptureSettled(Phase,"leaf-remove-control.png",320,693);
            var heading=FindText(g,"Page heading");heading.ForceMeshUpdate();Check(!heading.isTextOverflowing,"Collection heading overflow");
            var titleCorners=new Vector3[4];var badgeCorners=new Vector3[4];heading.rectTransform.GetWorldCorners(titleCorners);
            g.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Collection badge").GetWorldCorners(badgeCorners);
            Check(titleCorners[2].x<badgeCorners[0].x,"Collection heading overlaps the counter");
            var action=FindButton(g,"Trophy action");Check(action.interactable,"Equipped Mika trophy could not be removed");action.onClick.Invoke();yield return null;
            Check(g.EquippedGift==null&&g.OwnsGift("leaf")&&Global(g).VisibleCount==0,"Removal lost ownership or left drifting leaves");
            var saved=new ProfileStore(g.ProfilePath).Load();Check(saved.Recovery==SaveRecovery.None&&saved.Profile.Rewards.EquippedGift==null&&saved.Profile.Rewards.Owns("leaf"),"Off choice did not survive save/reload");
            g.ShowHome();g.ShowCollection();yield return null;Check(g.EquippedGift==null&&Global(g).VisibleCount==0,"Navigation reactivated leaves");
            g.ShowShop();g.SelectShopTab(1);g.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,"leaf"));yield return null;
            FindButton(g,"Buy selected item").onClick.Invoke();yield return null;Check(g.EquippedGift=="leaf","Owned Shop style did not enable");
            FindButton(g,"Buy selected item").onClick.Invoke();yield return null;Check(g.EquippedGift==null,"Owned Shop style could not disable");
            camera.targetTexture=oldTarget;portrait.Release();UnityEngine.Object.Destroy(portrait);
            Check(SessionState.GetInt(Prefix+"Errors",0)==0,"Native review recorded errors");
            File.WriteAllText(Path.Combine(Output,"native-review.txt"),"PASS: third legacy leaf part grants ownership without auto-equipping; EN/RU/UK reward guidance and direct collection link, including 320px Large Text; real enable/remove callbacks immediately start/stop global foliage while preserving ownership; saved off choice survives reload/navigation; inline Shop owned style also toggles. No phone build or live cloud deployment.");
            Debug.Log("CASCADE manual trophy UI review passed.");SessionState.SetBool(Prefix+"Finishing",true);EditorApplication.update+=Finish;EditorApplication.isPlaying=false;
        }
    }
}
