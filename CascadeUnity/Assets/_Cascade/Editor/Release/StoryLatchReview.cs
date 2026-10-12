using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
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
    public static class StoryLatchReview
    {
        const string Prefix="Cascade.StoryLatchReview.";
        static StoryLatchReview(){if(SessionState.GetBool(Prefix+"Auto",false))EditorApplication.update+=Tick;}
        public static void Begin()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var tables=LocalizationEditorSettings.GetStringTableCollection(UiCopy.Table);
            string source=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Data/story_revision_oct09.tsv"));
            foreach(var row in File.ReadAllLines(source).Skip(1)){
                var cells=row.Split('\t');
                foreach(var table in tables.StringTables){
                    int column=table.LocaleIdentifier.Code=="ru"?2:table.LocaleIdentifier.Code=="uk"?3:1;
                    table.AddEntry(cells[0],cells[column]);EditorUtility.SetDirty(table);
                }
            }
            foreach(var table in tables.StringTables)AssetDatabase.SaveAssetIfDirty(table);
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
            g.Preferences.Tutorials=false;g.Preferences.ReducedMotion=false;g.ReducedMotion=false;g.SetUiLocale("en");
            var row=new[]{"C","B","A"}.Select(s=>SymbolId.Parse("s:"+s)).ToArray();
            var target=new[]{"A","B","C"}.Select(s=>SymbolId.Parse("s:"+s)).ToArray();
            g.LoadLevel(new LevelDefinition("latch-review","C02","2.0","2.0","review",row,target,
                new[]{new TargetSegment(SegmentKind.Shapes,0,3)},new[]{new FloorDefinition(8,new[]{true,true},oneUseBridgeEdge:0)}));
            g.ShowGame();yield return Until(()=>g.Interaction.CanPlay,"Latch fixture not ready.");
            yield return new WaitForSecondsRealtime(.7f);
            yield return PlayableCapture.CaptureSettled("StoryLatch-Oct09","passage-open.png",390,844);
            string hash=g.Interaction.Attempt.CurrentStateHash;
            g.Interaction.TapSlot(0);g.Interaction.TapSlot(1);
            yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Bridge swap stuck.");
            yield return new WaitForSecondsRealtime(.7f);
            Check(g.Interaction.Attempt.Current.BridgeUsed,"Bridge did not close after one use.");
            yield return PlayableCapture.CaptureSettled("StoryLatch-Oct09","passage-closed.png",390,844);
            g.GetComponentsInChildren<Button>().Single(b=>b.name=="Undo").onClick.Invoke();
            yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Bridge Undo stuck.");
            yield return new WaitForSecondsRealtime(.7f);
            Check(g.Interaction.Attempt.CurrentStateHash==hash,"Bridge Undo changed puzzle snapshot.");
            yield return PlayableCapture.CaptureSettled("StoryLatch-Oct09","passage-undo.png",390,844);
            g.ReducedMotion=true;g.ReviewConversation(StoryConversations.IslandProgress(JourneyStory.IslandFirst(0,0)+5));
            yield return PlayableCapture.CaptureSettled("StoryLatch-Oct09","dialogue-en.png",390,844,game=>{
                var text=game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single(t=>t.name=="Conversation line");
                text.maxVisibleCharacters=int.MaxValue;text.ForceMeshUpdate();Check(text.text.Contains("far anchor"),"Wrong dialogue captured.");
            });
            string dir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/StoryLatch-Oct09"));
            File.WriteAllText(Path.Combine(dir,"native-review.txt"),"PASS: actual swap closes one-use passage; actual Undo restores whole snapshot; native 390x844 captures of open, closed, Undo and authored dialogue. Localized source imported EN/RU/UK. Runtime translated loading and physical-device test not run. Isolated profile, no phone build/install.");
            Debug.Log("CASCADE Story/latch review passed.");
        }
    }
}
