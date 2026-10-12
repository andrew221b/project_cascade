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
    public static class ToolBeltReview
    {
        const string Prefix="Cascade.ToolBeltReview.";
        const string Phase="ToolBelt-Oct10";
        public static void BeginBatch()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
            LocalizationAssets.Build();SessionState.SetBool(Prefix+"Batch",true);Begin();
        }
        static ToolBeltReview()
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
            if(SessionState.GetBool(Prefix+"Batch",false)){SessionState.SetBool(Prefix+"Batch",false);EditorApplication.Exit(0);}
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
        static T Read<T>(CascadeGame g,string name)=>(T)typeof(CascadeGame).GetField(name,Private).GetValue(g);
        static Rect Bounds(CascadeGame g,RectTransform r)
        {
            var root=Read<RectTransform>(g,"safeRoot");var corners=new Vector3[4];r.GetWorldCorners(corners);
            var min=root.InverseTransformPoint(corners[0]);var max=root.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        }
        static void CheckOpen(CascadeGame g)
        {
            var tray=Read<RectTransform>(g,"fieldTray");var root=Read<RectTransform>(g,"safeRoot");
            var receiver=Read<RectTransform>(g,"receiver");var bag=Bounds(g,(RectTransform)FindButton(g,"Field tools").transform);
            var scale=tray.GetComponentInParent<Canvas>().scaleFactor;
            var cards=Enumerable.Range(0,6).Select(i=>Bounds(g,(RectTransform)FindButton(g,"Field tool "+i).transform)).ToArray();
            var receiverBounds=Bounds(g,receiver);var rootBounds=Bounds(g,root);
            for(int i=0;i<6;i++){
                Check(cards[i].width*scale>=44-.1f&&cards[i].height*scale>=44-.1f,"Small tool touch target");
                Check(cards[i].xMin>=rootBounds.xMin&&cards[i].xMax<=rootBounds.xMax,"Tool left viewport");
                Check(cards[i].yMin>=rootBounds.yMin&&cards[i].yMax<receiverBounds.yMin,"Tool overlaps receiver");
                Check(!cards[i].Overlaps(bag),"Tool covers bag toggle");
                for(int j=i+1;j<6;j++)Check(!cards[i].Overlaps(cards[j]),"Tools overlap");
                Check(FindButton(g,"Field tool "+i).GetComponentInChildren<TMPro.TMP_Text>().text=="× 1","Wrong tool count");
            }
            var limit=FindText(g,"Field tool limit");limit.ForceMeshUpdate();
            Check(!limit.isTextOverflowing,"Limit text clipped: "+limit.text);
            Check(Bounds(g,limit.rectTransform).yMax<receiverBounds.yMin,"Limits cover receiver");
            Check(Read<CanvasGroup>(g,"fieldTrayGroup").blocksRaycasts,"Open tool belt cannot receive touches");
            foreach(var group in Read<CanvasGroup[]>(g,"fieldActionGroups"))Check(group.alpha<.001f&&!group.blocksRaycasts,"Offscreen control still receives touches");
        }
        static IEnumerator Run(CascadeGame g)
        {
            Directory.CreateDirectory(Output);g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.Preferences.NotificationsEnabled=false;
            g.Preferences.Tutorials=false;g.Preferences.ReducedMotion=false;g.ReducedMotion=false;g.SetUiLocale("en");
            var economy=(AdService)typeof(CascadeGame).GetProperty("GameEconomy",Private).GetValue(g);
            economy.Ledger.WayfinderScrolls=economy.Ledger.UndoSpools=economy.Ledger.ReturnHourglasses=economy.Ledger.SignalLanterns=economy.Ledger.LandingLenses=economy.Ledger.RepairKits=1;
            var camera=Camera.main;var oldTarget=camera.targetTexture;var portrait=new RenderTexture(390,844,24);portrait.Create();camera.targetTexture=portrait;
            g.LoadLevel(Fixture("tool-belt-native"));g.ShowGame();yield return Until(()=>g.Interaction.CanPlay,"Board blocked");g.RefreshLayout();
            var names=new[]{"Undo","Hint","Field tools","Drop"};
            var closed=names.Select(n=>((RectTransform)FindButton(g,n).transform).anchoredPosition).ToArray();
            var initialRow=g.Interaction.Attempt.Current.Row.ToArray();int hp=g.Interaction.Attempt.Current.Durability;
            yield return PlayableCapture.CaptureSettled(Phase,"belt-closed.png",390,844);
            FindButton(g,"Field tools").onClick.Invoke();yield return null;
            Check(!Read<CanvasGroup>(g,"fieldTrayGroup").blocksRaycasts,"Moving cards became touchable too early");
            yield return Until(()=>Read<float>(g,"fieldTrayProgress")>=.999f,"Open tween stalled");CheckOpen(g);
            yield return PlayableCapture.CaptureSettled(Phase,"belt-open.png",390,844);
            FindButton(g,"Field tools").onClick.Invoke();yield return Until(()=>Read<object>(g,"fieldTrayTween")==null,"Close tween stalled");
            for(int i=0;i<names.Length;i++)Check(Vector2.Distance(closed[i],((RectTransform)FindButton(g,names[i]).transform).anchoredPosition)<.01f,"Control did not return: "+names[i]);
            // Interrupt opening/closing repeatedly. No cumulative position drift or hidden hit targets.
            for(int i=0;i<24;i++){FindButton(g,"Field tools").onClick.Invoke();yield return new WaitForSecondsRealtime(.035f);}
            if(g.FieldToolTrayOpen)FindButton(g,"Field tools").onClick.Invoke();
            yield return Until(()=>Read<object>(g,"fieldTrayTween")==null,"Repeated reversal stranded controls");
            Check(!Read<RectTransform>(g,"fieldTray").gameObject.activeSelf,"Closed tray stayed active");
            Check(g.Interaction.Attempt.Current.Row.SequenceEqual(initialRow)&&g.Interaction.Attempt.Current.Durability==hp,"Layout animation changed gameplay");
            Time.timeScale=0;FindButton(g,"Field tools").onClick.Invoke();yield return Until(()=>Read<float>(g,"fieldTrayProgress")>=.999f,"Unscaled open failed");CheckOpen(g);
            FindButton(g,"Field tools").onClick.Invoke();yield return Until(()=>Read<float>(g,"fieldTrayProgress")<=.001f,"Unscaled close failed");Time.timeScale=1;
            foreach(string locale in new[]{"ru","uk"}){
                camera.targetTexture=null;portrait.Release();UnityEngine.Object.Destroy(portrait);portrait=new RenderTexture(320,693,24);portrait.Create();camera.targetTexture=portrait;
                g.SetUiLocale(locale);g.Preferences.LargeText=true;g.RefreshLayout();
                FindButton(g,"Field tools").onClick.Invoke();yield return Until(()=>Read<float>(g,"fieldTrayProgress")>=.999f,"Compact open failed");CheckOpen(g);
                yield return PlayableCapture.CaptureSettled(Phase,"belt-compact-"+locale+".png",320,693);
                g.ShowShop();yield return null;Check(!g.FieldToolTrayOpen&&Read<float>(g,"fieldTrayProgress")==0,"Navigation kept tray alive");g.ShowGame();yield return Until(()=>g.Interaction.CanPlay,"Return from Shop blocked");
            }
            g.Preferences.ReducedMotion=true;g.ReducedMotion=true;FindButton(g,"Field tools").onClick.Invoke();Check(Read<float>(g,"fieldTrayProgress")==1,"Reduced Motion did not settle");CheckOpen(g);
            FindButton(g,"Field tools").onClick.Invoke();Check(Read<float>(g,"fieldTrayProgress")==0,"Reduced Motion did not restore");
            g.Preferences.ReducedMotion=false;g.ReducedMotion=false;FindButton(g,"Field tools").onClick.Invoke();yield return new WaitForSecondsRealtime(.10f);
            g.SendMessage("OnApplicationPause",true);yield return null;Check(Read<float>(g,"fieldTrayProgress")==0,"Suspend left a half-open belt");
            g.SendMessage("OnApplicationPause",false);g.SendMessage("OnApplicationFocus",true);yield return Until(()=>g.Interaction.CanPlay,"Resume blocked");
            FindButton(g,"Field tools").onClick.Invoke();yield return Until(()=>Read<float>(g,"fieldTrayProgress")>=.999f,"Resume open failed");
            FindButton(g,"Field tool 5").onClick.Invoke();yield return Until(()=>!g.FieldToolTrayOpen&&!g.Interaction.IsPresenting,"Tool selection did not close/restore");
            Check(g.Stock(TravelTool.RepairKit)==0&&g.Interaction.Attempt.Current.Durability==hp+2,"Actual card did not apply kit once");
            camera.targetTexture=oldTarget;portrait.Release();UnityEngine.Object.Destroy(portrait);
            Check(SessionState.GetInt(Prefix+"Errors",0)==0,"Native review recorded errors");
            File.WriteAllText(Path.Combine(Output,"native-review.txt"),"PASS: actual bag/card callbacks; six lateral tools, readable counts and limits, 44+px targets and no receiver overlap on 390/320 portraits, EN/RU/UK and Large Text; exact close restoration, 24 mid-animation reversals, unscaled animation, Reduced Motion, navigation and suspend/resume, unchanged gameplay during toggling, one real repair-kit use. No phone build or physical-device test.");
            Debug.Log("CASCADE lateral tool belt review passed.");SessionState.SetBool(Prefix+"Finishing",true);EditorApplication.update+=Finish;EditorApplication.isPlaying=false;
        }
    }
}
