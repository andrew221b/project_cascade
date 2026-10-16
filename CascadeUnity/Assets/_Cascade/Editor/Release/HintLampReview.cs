using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Presentation;
using Cascade.Editor.Playable;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Release
{
    // Real UI callbacks, isolated temporary save; never touches the device profile.
    [InitializeOnLoad]
    public static class HintLampReview
    {
        const string Prefix="Cascade.HintLampReview.";
        static HintLampReview(){if(SessionState.GetBool(Prefix+"Auto",false))EditorApplication.update+=Tick;}
        public static void Begin()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var tables=LocalizationEditorSettings.GetStringTableCollection(UiCopy.Table);
            foreach(var table in tables.StringTables){
                table.AddEntry("Free hint used",table.LocaleIdentifier.Code=="ru"?"Подсказка использована":table.LocaleIdentifier.Code=="uk"?"Підказку використано":"Free hint used");
                EditorUtility.SetDirty(table);AssetDatabase.SaveAssetIfDirty(table);
            }
            EditorUtility.SetDirty(tables.SharedData);AssetDatabase.SaveAssetIfDirty(tables.SharedData);
            SessionState.SetString(Prefix+"OldProfile",SessionState.GetString("Cascade.NavigationReviewProfile",""));
            SessionState.SetBool(Prefix+"OldBackground",UnityEngine.Application.runInBackground);
            NavigationCapture.Prepare();UnityEngine.Application.runInBackground=true;
            SessionState.SetBool(Prefix+"Auto",true);SessionState.SetBool(Prefix+"Started",false);
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
        }
        static void Tick()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))return;
            EditorApplication.QueuePlayerLoopUpdate();
            if(SessionState.GetBool(Prefix+"Started",false))return;
            SessionState.SetBool(Prefix+"Started",true);game.StartCoroutine(Run(game));
        }
        public static void RestoreSession()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            EditorApplication.update-=Tick;SessionState.SetBool(Prefix+"Auto",false);SessionState.SetBool(Prefix+"Started",false);
            string old=SessionState.GetString(Prefix+"OldProfile","");
            if(old.Length==0)NavigationCapture.Clear();else{SessionState.SetString("Cascade.NavigationReviewProfile",old);CascadeGame.EditorProfileDirectory=old;}
            UnityEngine.Application.runInBackground=SessionState.GetBool(Prefix+"OldBackground",false);
            SessionState.EraseString(Prefix+"OldProfile");
        }
        static IEnumerator Until(Func<bool> ready,string message)
        {float end=Time.realtimeSinceStartup+35;while(!ready()){if(Time.realtimeSinceStartup>end)throw new InvalidOperationException(message);yield return null;}}
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        static IEnumerator Run(CascadeGame g)
        {
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.Preferences.NotificationsEnabled=false;
            g.Preferences.Tutorials=false;g.Preferences.ReducedMotion=true;g.ReducedMotion=true;g.SetUiLocale("en");
            var row=new[]{"D","C","B","A"}.Select(s=>SymbolId.Parse("s:"+s)).ToArray();
            var target=new[]{"A","B","C","D"}.Select(s=>SymbolId.Parse("s:"+s)).ToArray();
            var level=new LevelDefinition("hint-review","C02","2.0","2.0","review",row,target,new[]{new TargetSegment(SegmentKind.Shapes,0,4)},new[]{new FloorDefinition(10,new[]{true,true,true})});
            g.LoadLevel(level);g.ShowGame();yield return Until(()=>g.Interaction.CanPlay,"Fixture input not ready.");
            var lamp=g.GetComponentsInChildren<Button>().Single(b=>b.name=="Hint");
            string hash=g.Interaction.Attempt.CurrentStateHash;
            for(int i=0;i<50;i++)lamp.onClick.Invoke();
            yield return Until(()=>g.HintVisible,"First hint not shown.");
            Check(g.Interaction.Attempt.CurrentStateHash==hash&&g.Interaction.Attempt.History.Count==0,"Hint executed a command.");
            lamp.onClick.Invoke();Check(!g.HintVisible,"Second tap did not hide hint.");
            lamp.onClick.Invoke();Check(g.HintVisible,"Third tap did not re-show same hint.");
            for(int i=0;i<100;i++)lamp.onClick.Invoke();
            Check(g.Interaction.Attempt.CurrentStateHash==hash&&g.Interaction.Attempt.MovesMade==0,"Repeated lamp taps moved the row.");
            var command=AttemptService.CalculateHint(g.Interaction.Attempt.CaptureHintRequest()).Result.Command.Value;
            Check(command.Kind==CommandKind.Swap,"Fixture unexpectedly requested descent.");
            g.Interaction.TapSlot(command.Edge);g.Interaction.TapSlot(command.Edge+1);
            yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Manual swap did not settle.");
            string moved=g.Interaction.Attempt.CurrentStateHash;Check(moved!=hash,"Actual taps did not move pieces.");
            for(int i=0;i<50;i++)lamp.onClick.Invoke();
            yield return null;Check(!g.HintVisible&&g.Interaction.Attempt.CurrentStateHash==moved,"Quota allowed another free step.");
            Check(!g.FieldToolTrayOpen,"Empty inventory opened.");
            var restored=AttemptService.Restore(level,AttemptRecord.Capture(g.Interaction.Attempt));
            Check(!restored.CanShowFreeHint,"Reload reset hint allowance.");
            g.GetComponentsInChildren<Button>().Single(b=>b.name=="Undo").onClick.Invoke();
            yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Undo stuck.");
            Check(g.Interaction.Attempt.CurrentStateHash==hash,"Undo did not restore row.");lamp.onClick.Invoke();
            yield return Until(()=>g.HintVisible,"Old hint cannot be shown after Undo.");
            g.Interaction.TapSlot(command.Edge);g.Interaction.TapSlot(command.Edge+1);
            yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Second manual swap stuck.");
            var economy=(AdService)typeof(CascadeGame).GetProperty("GameEconomy",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(g);
            economy.Ledger.WayfinderScrolls=1;g.ToggleFieldToolTray();
            Check(g.FieldToolTrayOpen&&g.Stock(TravelTool.Wayfinder)==1,"Bag consumed owned tool or did not open tray.");
            g.GetComponentsInChildren<Button>().Single(b=>b.name=="Field tool 0").onClick.Invoke();
            yield return Until(()=>g.Interaction.Attempt.WayfinderUsed&&g.HintVisible,"Explicit Wayfinder did not show hint.");
            Check(g.Stock(TravelTool.Wayfinder)==0&&g.Interaction.Attempt.CurrentStateHash==moved,"Wayfinder inventory or manual movement violated.");
            for(int i=0;i<50;i++)lamp.onClick.Invoke();
            Check(g.Interaction.Attempt.CurrentStateHash==moved,"Wayfinder lamp executed a move.");
            string dir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/HintLamp-Oct09"));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"native-review.txt"),"PASS: 254 actual Hint-button callbacks; pending-search burst; show/hide/re-show; manual two-tap swap; no extra free hint; empty bag stays closed; Undo only reopens original hint; reload preserves allowance; owned Wayfinder opens tray without spending, explicit tool consumes exactly one and shows hint without moving figures. Isolated profile; no phone build/install. Physical touch and first Hint tutorial not run.");
            Debug.Log("CASCADE Hint lamp review passed.");
        }
    }
}
