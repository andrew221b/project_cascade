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
using Newtonsoft.Json;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // Measurements and native captures only. Run against the existing isolated QA profile.
    public static class VisualAuditReview
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static readonly Vector4 Insets=new Vector4(.018f,.04f,.018f,.066f);
        static readonly List<string> findings=new List<string>();
        static string phase;
        public static string Status { get; private set; }="Not started";
        public static bool Running { get; private set; }
        static string Folder=>Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",phase);
        sealed class Pose
        {
            public string Name;public Action Setup;public int Width=390,Height=844;
            public bool Small,Wide;public float Settle=.45f;
        }
        public static void Prepare()=>FiveStarReview.Prepare();
        public static string DiagnosticTerminalShop()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(g==null||!EditorApplication.isPlaying||g.ProfilePath==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated review required.");
            var log=new List<string>();var oldAttempt=g.Interaction.Attempt;var oldRun=g.Descent;var oldScreen=g.CurrentScreen;
            bool persistence=(bool)Field(g,"persistenceReady");Field(g,"persistenceReady",false);
            try {
                var saved=new ProfileStore(g.ProfilePath).Load().Profile;var terminal=saved.RestoreAttempt(g.Catalog);
                if(terminal==null||terminal.Current.Status!=PuzzleStatus.Won)throw new InvalidOperationException("Saved terminal fixture is unavailable.");
                Call(g,"MountAttempt",terminal,saved.Descent);
                log.Add("Terminal floor="+terminal.Current.ActiveFloor+" floors="+terminal.Level.Floors.Count+" state="+terminal.CurrentStateHash);
                Action[] steps={g.ShowHome,g.ShowShop,()=>g.SelectShopTab(0),()=>g.SelectShopItem(3),g.ShowSupplies};
                string[] names={"ShowHome","ShowShop","SelectShopTab(0)","SelectShopItem(3)","ShowSupplies"};
                for(int i=0;i<steps.Length;i++)try{steps[i]();log.Add(names[i]+" succeeded");}catch(Exception e){log.Add(names[i]+" failed:\n"+e);}
            }catch(Exception e){log.Add("Terminal diagnostic failed:\n"+e);}
            finally{Call(g,"MountAttempt",oldAttempt,oldRun);Screen(g,oldScreen);Field(g,"persistenceReady",persistence);}
            if(string.IsNullOrEmpty(phase))phase="VisualAudit-Before";Directory.CreateDirectory(Folder);
            string result=string.Join("\n",log);File.WriteAllText(Path.Combine(Folder,"terminal-shop-diagnostic.txt"),result);return result;
        }
        public static void Run(bool after=false)
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(Running)throw new InvalidOperationException("Visual audit already running.");
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling||g==null||g.ProfilePath==null||!g.ProfilePath.Contains("CascadeNavigationReview"))
                throw new InvalidOperationException("Use VisualAuditReview.Prepare before loading the playable; an isolated compiled Play Mode session is required.");
            phase=after?"VisualAudit-After":"VisualAudit-Before";Directory.CreateDirectory(Folder);findings.Clear();
            Running=true;g.StartCoroutine(Inspect(g));
        }
        static object Field(CascadeGame g,string name)=>typeof(CascadeGame).GetField(name,Private).GetValue(g);
        static void Field(CascadeGame g,string name,object value)=>typeof(CascadeGame).GetField(name,Private).SetValue(g,value);
        static object Call(CascadeGame g,string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Private).Invoke(g,args);
        static void Screen(CascadeGame g,GameScreen screen)=>Call(g,"SetScreen",screen);
        static void Draw(CascadeGame g)=>Call(g,"DrawMenu");
        static IEnumerator Inspect(CascadeGame g)
        {
            bool background=UnityEngine.Application.runInBackground;
            bool persistence=(bool)Field(g,"persistenceReady");
            try {
                UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
                Field(g,"persistenceReady",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
                var profile=(PlayerProfile)Field(g,"profile");
                foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
                // Keep the valid terminal fixture available before any LoadLevel replaces it.
                var terminal=g.Interaction.Attempt;
                var checkpoint=g.Descent==null?null:JsonConvert.DeserializeObject<DescentRun>(JsonConvert.SerializeObject(g.Descent));
                if(checkpoint!=null)checkpoint.Completed.Clear();
                var poses=new List<Pose>();
                void Add(string name,Action setup,bool small=false,bool wide=false,float settle=.45f)
                    =>poses.Add(new Pose{Name=name,Setup=setup,Small=small,Wide=wide,Width=small?320:wide?1440:390,Height=small?693:wide?900:844,Settle=settle});
                void Settings(int page){g.ShowSettings();Field(g,"settingsPage",page);Draw(g);}
                void Load(string node,string locale="en"){
                    if(locale!="en"&&g.Catalog.TryGetLocalizedBinding(node,locale,out var words))g.LoadLevel(words.Definition,locale);
                    else g.LoadLevel(g.Catalog.GetPrimary(node).Definition,locale);
                    g.ShowGame();
                }
                Add("result-ru",()=>Screen(g,GameScreen.Win),settle:3.8f);
                Add("result-uk-small",()=>Screen(g,GameScreen.Win),small:true,settle:3.8f);
                Add("parcel-closed-ru",g.ShowParcel);
                Add("parcel-open-ru",()=>{int order=(int)Field(g,"parcelOrder");g.Rewards.ClaimParcel(order);Field(g,"parcelOpened",true);Screen(g,GameScreen.Parcel);},settle:3.8f);
                Add("home-ru",g.ShowHome);Add("home-wide",g.ShowHome,wide:true);
                Add("prologue-1-ru",g.ShowPrologue);
                Add("prologue-2-ru",g.AdvancePrologue);Add("prologue-3-ru",g.AdvancePrologue);
                Add("map-ru",g.ShowJourney);Add("map-wide",g.ShowJourney,wide:true);
                Add("lessons-ru",g.ShowLessons);
                Add("bag-ru",g.ShowTravelBag);Add("bag-uk-small",g.ShowTravelBag,small:true);
                Add("journal-ru",g.ShowJournal);Add("collection-ru",g.ShowCollection);
                Add("workshop-ru",g.ShowWorkshop);Add("requests-ru",()=>{g.ShowRequests();Field(g,"requestPage",2);Draw(g);});
                Add("records-ru",()=>{if(terminal.Current.Status==PuzzleStatus.Won)profile.Rewards.RecordDelivery(terminal,g.Descent?.Performance(terminal),profile.ModeRecords);g.ShowRecords();});
                Add("contracts-ru",g.ShowContracts);Add("journey-end-ru",()=>Screen(g,GameScreen.JourneyEnd));
                Add("powerups-ru",g.ShowPowerupChallenges);Add("reach-ru",g.ShowReachChallenges);
                Add("settings-sound-ru",()=>Settings(0));Add("settings-comfort-ru",()=>Settings(1));Add("settings-language-ru",()=>Settings(2));
                Add("word-confirm-ru",()=>g.ProposePuzzleLocale(g.Preferences.PuzzleLocale=="uk"?"ru":"uk"));
                Add("settings-sound-uk-small",()=>Settings(0),small:true);Add("settings-comfort-uk-small",()=>Settings(1),small:true);Add("settings-language-uk-small",()=>Settings(2),small:true);
                Add("shop-ru",()=>{g.ShowShop();g.SelectShopTab(0);g.SelectShopItem(3);});
                Add("shop-uk-small",()=>{g.ShowShop();g.SelectShopTab(0);g.SelectShopItem(4);},small:true);
                Add("no-ads-ru",g.ShowNoAds);Add("coin-packs-ru",g.ShowCoinPacks);
                Add("supplies-ru",()=>{var l=g.Monetization.Ledger;l.WayfinderScrolls=l.UndoSpools=l.ReturnHourglasses=l.SignalLanterns=l.LandingLenses=1;g.ShowSupplies();});
                Add("save-recovery-ru",()=>{typeof(CascadeGame).GetProperty("Recovery").SetValue(g,SaveRecovery.AttemptUnavailable);Screen(g,GameScreen.SaveRecovery);});
                Add("dialogue-ru",()=>{Screen(g,GameScreen.Dialogue);Field(g,"conversationAge",1000f);});
                Add("dialogue-uk-small",()=>{Screen(g,GameScreen.Dialogue);Field(g,"conversationAge",1000f);},small:true);
                Add("game-ru",()=>{typeof(CascadeGame).GetProperty("Recovery").SetValue(g,SaveRecovery.None);Load("C03");});
                Add("game-wide",g.ShowGame,wide:true);
                Add("pause-ru",()=>Screen(g,GameScreen.Pause));Add("basics-ru",g.ShowHowToPlay);
                Add("device-ru",()=>{g.ShowDevices();Field(g,"deviceIndex",2);Draw(g);});
                Add("mixed-uk-small",()=>Load("C32"),small:true);
                Add("words-ru",()=>Load("C37","ru"));
                Add("mechanic-ru",()=>{Load("C02");Call(g,"OpenMechanicPopup",MechanicLesson.Gust,null,default(UiCopy),null);});
                Add("target-ru",()=>{Call(g,"CloseMechanicPopup");g.ShowGame();Call(g,"ToggleTargetZoom");});
                Add("ad-modal-ru",()=>{Call(g,"ToggleTargetZoom");g.ShowPreviewAd(true,_=>{});});
                Add("fail-ru",()=>{
                    Call(g,"FinishPreviewAd",false);var a=new AttemptService(g.Catalog.GetPrimary("C01").Definition);
                    for(int i=0;i<8&&a.Current.Status==PuzzleStatus.Playing;i++)a.Execute(PuzzleCommand.Drop);
                    if(a.Current.Status!=PuzzleStatus.Lost)throw new InvalidOperationException("C01 drop fixture did not lose.");
                    Call(g,"MountAttempt",a,null);g.ShowGame();
                });
                Add("checkpoint-ru",()=>{
                    if(checkpoint==null)throw new InvalidOperationException("Prepare's three-stage fixture is unavailable.");
                    var source=g.Catalog.PrimaryLevels.First(c=>c.Definition.ContentHash==checkpoint.SourceHash).Definition;
                    var a=new AttemptService(checkpoint.Level(source,0));
                    foreach(string token in checkpoint.Stages[0].Witness){var parts=token.TrimStart('R').Split(':');var command=token=="D"?PuzzleCommand.Drop:token.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(parts[0]),int.Parse(parts[1])):PuzzleCommand.Swap(int.Parse(token));if(!a.Execute(command).Accepted)throw new InvalidOperationException("Checkpoint witness rejected.");}
                    Call(g,"MountAttempt",a,checkpoint);g.ShowGame();
                });
                File.WriteAllText(Path.Combine(Folder,"routes.txt"),"All GameScreen values: "+string.Join(", ",Enum.GetNames(typeof(GameScreen)))+"\nPlayChoice is an unused legacy alias of JourneyMap; no duplicate image. Dialogue is captured as its conversation overlay. Devices includes basics and a bomb guide. Settings has three pages. Prologue has three pages. Terminal, failed and checkpoint states use valid isolated attempts. Other private SetScreen calls exercise presentation routes, not progression eligibility.\nNative Editor captures only; simulated safe insets; no physical-device, sound-playback, release-language or human-enjoyment claim.\nThe Music and Sound effects preferences are separate controls; visual captures do not validate audio playback.\n");
                int done=0;
                foreach(var pose in poses){
                    Status="Capturing "+(++done)+"/"+poses.Count+" "+pose.Name;WriteStatus();
                    yield return Guard(Capture(g,pose),pose.Name);
                }
                Status="Complete: "+poses.Count+" poses attempted; "+findings.Count+" measurement/fixture notes";WriteStatus();
                Debug.Log("CASCADE visual audit complete: "+phase+"; "+Status);
            }
            finally {
                File.WriteAllLines(Path.Combine(Folder,"findings.txt"),findings);Running=false;
                Field(g,"persistenceReady",persistence);UnityEngine.Application.runInBackground=background;
            }
        }
        static IEnumerator Capture(CascadeGame g,Pose pose)
        {
            g.Preferences.LargeText=pose.Small;g.SetEditorSafeInsets(pose.Wide?Vector4.zero:Insets);g.SetUiLocale(pose.Small?"uk":"ru");
            pose.Setup();yield return null;
            float until=Time.realtimeSinceStartup+15;
            while(g.Interaction.IsPreparing&&!g.MechanicPopupOpen&&Time.realtimeSinceStartup<until)yield return null;
            if(g.Interaction.IsPreparing&&!g.MechanicPopupOpen)findings.Add(pose.Name+" | readiness timeout; captured current state");
            for(float t=0;t<pose.Settle;t+=Mathf.Min(.05f,Time.unscaledDeltaTime))yield return null;
            var before=State(g);object measurements=null;
            yield return PlayableCapture.CaptureSettled(phase,pose.Name+".png",pose.Width,pose.Height,live=>measurements=Measure(live,pose));
            var after=State(g);
            File.WriteAllText(Path.Combine(Folder,pose.Name+".audit.json"),JsonConvert.SerializeObject(new{pose=pose.Name,before,after,unchangedGameplay=before.Hash==after.Hash&&before.Attempt==after.Attempt,measurements},Formatting.Indented));
            if(before.Hash!=after.Hash||before.Attempt!=after.Attempt)findings.Add(pose.Name+" | gameplay identity/hash changed during capture");
            File.WriteAllLines(Path.Combine(Folder,"findings.txt"),findings);
        }
        sealed class StateSnapshot
        {
            public string Screen,Attempt,Hash,Status;public int Route,Stage,Clears;public bool Preparing,Paused;
        }
        static StateSnapshot State(CascadeGame g)=>new StateSnapshot{Screen=g.CurrentScreen.ToString(),Attempt=g.Interaction.Attempt.AttemptId.ToString("D"),Hash=g.Interaction.Attempt.CurrentStateHash,Status=g.Interaction.Attempt.Current.Status.ToString(),Route=g.Interaction.Attempt.RouteLength,Stage=g.Descent?.StageIndex??-1,Clears=g.JourneyClearCount,Preparing=g.Interaction.IsPreparing,Paused=g.Interaction.IsPaused};
        static object Measure(CascadeGame g,Pose pose)
        {
            var labels=new List<object>();var buttons=new List<object>();
            foreach(var button in g.GetComponentsInChildren<Button>()){
                if(!button.isActiveAndEnabled)continue;
                buttons.Add(new{name=button.name,path=PathOf(button.transform),interactable=button.interactable,bounds=Bounds((RectTransform)button.transform)});
            }
            foreach(var text in g.GetComponentsInChildren<TMP_Text>()){
                if(!text.isActiveAndEnabled||string.IsNullOrWhiteSpace(text.text))continue;text.ForceMeshUpdate();
                var button=text.GetComponentInParent<Button>();var split=new List<string>();
                if(button!=null){
                    // TMP can retain old wordInfo entries after an atlas/locale refresh.
                    // Read only the current character mesh rather than stale word indices.
                    string word="";int firstLine=-1,lastLine=-1;
                    void FinishWord(){if(word.Length>0&&firstLine!=lastLine)split.Add(word);word="";firstLine=lastLine=-1;}
                    int count=Math.Min(text.textInfo.characterCount,text.textInfo.characterInfo.Length);
                    for(int i=0;i<count;i++){
                        var character=text.textInfo.characterInfo[i];
                        if(char.IsWhiteSpace(character.character)){FinishWord();continue;}
                        if(firstLine<0)firstLine=character.lineNumber;lastLine=character.lineNumber;word+=character.character;
                    }
                    FinishWord();
                }
                if(text.isTextOverflowing)findings.Add(pose.Name+" | text overflow | "+PathOf(text.transform)+" | "+text.text.Replace('\n',' '));
                foreach(string word in split)findings.Add(pose.Name+" | button word split | "+PathOf(text.transform)+" | "+word);
                var canvas=text.GetComponentInParent<Canvas>();
                labels.Add(new{name=text.name,path=PathOf(text.transform),text=text.text,button=button==null?null:PathOf(button.transform),bounds=Bounds(text.rectTransform),fontPixels=text.fontSize*(canvas==null?1:canvas.scaleFactor),overflow=text.isTextOverflowing,splitWords=split});
            }
            var insets=pose.Wide?Vector4.zero:Insets;
            return new{width=pose.Width,height=pose.Height,locale=pose.Small?"uk":"ru",largeText=pose.Small,simulatedInsets=new[]{insets.x,insets.y,insets.z,insets.w},buttons,labels};
        }
        static object Bounds(RectTransform rect)
        {
            var corners=new Vector3[4];rect.GetWorldCorners(corners);var camera=Camera.main;
            var low=camera.WorldToScreenPoint(corners[0]);var high=camera.WorldToScreenPoint(corners[2]);
            return new{x=low.x,y=low.y,width=high.x-low.x,height=high.y-low.y};
        }
        static string PathOf(Transform t){var parts=new List<string>();for(;t!=null;t=t.parent)parts.Add(t.name);parts.Reverse();return string.Join("/",parts);}
        static void WriteStatus()=>File.WriteAllText(Path.Combine(Folder,"status.txt"),Status+"\n");
        public static void CaptureCatalogueSupplement()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated audit profile required.");
            phase="VisualAudit-After";g.StartCoroutine(CatalogueSupplement(g));
        }
        static IEnumerator CatalogueSupplement(CascadeGame g)
        {
            findings.Clear();bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            try {
                g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
                yield return Guard(Capture(g,new Pose{Name="shop-ru",Setup=()=>{g.ShowShop();g.SelectShopTab(0);g.SelectShopItem(3);}}),"shop-ru");
                yield return Guard(Capture(g,new Pose{Name="shop-uk-small",Small=true,Width=320,Height=693,Setup=()=>{g.ShowShop();g.SelectShopTab(0);g.SelectShopItem(4);}}),"shop-uk-small");
                yield return Guard(Capture(g,new Pose{Name="supplies-ru",Setup=g.ShowSupplies}),"supplies-ru");
                yield return Guard(Capture(g,new Pose{Name="device-ru",Setup=()=>{g.ShowDevices();Field(g,"deviceIndex",2);Draw(g);}}),"device-ru");
                File.WriteAllLines(Path.Combine(Folder,"catalogue-supplement-findings.txt"),findings);
                Status="Catalogue supplement complete: "+findings.Count+" notes";WriteStatus();
            } finally {UnityEngine.Application.runInBackground=background;}
        }
        // Flatten nested iterators so one unavailable route records a note and the batch continues.
        static IEnumerator Guard(IEnumerator routine,string name)
        {
            var stack=new Stack<IEnumerator>();stack.Push(routine);
            while(stack.Count>0){
                object next=null;bool moved=false;Exception failure=null;
                try {moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
                if(failure!=null){
                    findings.Add(name+" | fixture/capture unavailable | "+failure.GetBaseException());
                    while(stack.Count>0)(stack.Pop() as IDisposable)?.Dispose();yield break;
                }
                if(!moved){(stack.Pop() as IDisposable)?.Dispose();continue;}
                if(next is IEnumerator nested)stack.Push(nested);else yield return next;
            }
        }
    }
}
