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
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Release
{
    // Real UI callbacks, isolated temporary save; never touches the device profile.
    [InitializeOnLoad]
    public static class PlatformReadabilityReview
    {
        const string Prefix="Cascade.PlatformReadabilityReview.";
        const string Phase="PlatformReadability-Oct10";
        public static void BeginBatch()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
            SessionState.SetBool(Prefix+"Batch",true);Begin();
        }
        static PlatformReadabilityReview()
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
            if(SessionState.GetBool(Prefix+"Batch",false)&&EditorApplication.timeSinceStartup-SessionState.GetFloat(Prefix+"Began",0)>180){
                Debug.LogError("CASCADE platform review timed out; no successful validation.");EditorApplication.Exit(2);return;
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
        static IEnumerator Until(Func<bool> ready,string message)
        {float end=Time.realtimeSinceStartup+35;while(!ready()){if(Time.realtimeSinceStartup>end)throw new InvalidOperationException(message);yield return null;}}
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        static IEnumerator Run(CascadeGame g)
        {
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.Preferences.NotificationsEnabled=false;
            g.Preferences.ReducedMotion=false;g.ReducedMotion=false;g.SetUiLocale("en");
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(g);
            var row=new[]{"B","A","D","C"}.Select(s=>SymbolId.Parse("s:"+s)).ToArray();
            var target=new[]{"A","B","C","D"}.Select(s=>SymbolId.Parse("s:"+s)).ToArray();
            foreach(var lesson in new[]{MechanicLesson.Mirror,MechanicLesson.Rollers,MechanicLesson.Fragile,MechanicLesson.LinkedPairs}){
                profile.SeenMechanics.Clear();profile.SeenMechanics.AddRange(Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().Where(x=>x!=lesson));
                foreach(var marker in new[]{"controls-two-ways-v1","controls-undo-v1","closed-path-v2"})if(!profile.SeenCoachPrompts.Contains(marker))profile.SeenCoachPrompts.Add(marker);
                FloorDefinition[] floors=lesson==MechanicLesson.Mirror||lesson==MechanicLesson.Rollers?
                    new[]{new FloorDefinition(4,new[]{true,true,true}),new FloorDefinition(4,new[]{true,true,true},entryTransform:lesson==MechanicLesson.Mirror?EntryTransform.MirrorNext:EntryTransform.ShiftRightOne)}:
                    new[]{new FloorDefinition(8,new[]{true,true,true},fragileEdges:lesson==MechanicLesson.Fragile?new[]{1}:null,linkedPair:lesson==MechanicLesson.LinkedPairs?new LinkedPair(0,2):(LinkedPair?)null)};
                g.Preferences.Tutorials=true;
                g.LoadLevel(new LevelDefinition("platform-review-"+lesson,"C02","2.0","2.0","review",row,target,
                    new[]{new TargetSegment(SegmentKind.Shapes,0,4)},floors));g.ShowGame();
                string initial=g.Interaction.Attempt.CurrentStateHash;
                yield return Until(()=>g.MechanicPopupOpen,"No automatic lesson for "+lesson);
                Check(g.CurrentMechanic==lesson,"Wrong opening lesson: "+g.CurrentMechanic);
                Check(!g.Interaction.Attempt.CanUndo&&g.Interaction.Attempt.CurrentStateHash==initial,"Lesson needed or changed a move.");
                yield return new WaitForSecondsRealtime(.45f);
                yield return PlayableCapture.CaptureSettled(Phase,lesson+"-lesson.png",390,844);
                g.AcknowledgeMechanic();g.Preferences.Tutorials=false;
                yield return Until(()=>g.Interaction.CanPlay,"Controls not released after lesson.");
                yield return new WaitForSecondsRealtime(.4f);
                yield return PlayableCapture.CaptureSettled(Phase,lesson+"-board.png",390,844);
                if(lesson==MechanicLesson.Fragile||lesson==MechanicLesson.LinkedPairs){
                    var mark=g.GetComponentsInChildren<PlatformMechanismGraphic>().Single();
                    Check(mark.GetComponent<CanvasRenderer>()!=null&&mark.canvasRenderer.GetMesh().vertexCount>0,"Missing visible platform mechanism mesh.");
                }
                if(lesson==MechanicLesson.LinkedPairs){
                    g.Interaction.TapSlot(0);g.Interaction.TapSlot(1);
                    yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Linked exchange stuck.");
                    Check(g.Interaction.Attempt.Current.Row.SequenceEqual(target),"Linked pair did not exchange both neighbours atomically.");
                }
            }
            g.Preferences.Tutorials=false;
            foreach(string kind in new[]{"crack","locked","latch","gust"}){
                var floor=kind=="crack"?new FloorDefinition(8,new[]{true,true,true},fragileEdges:new[]{1}):
                    kind=="locked"?new FloorDefinition(8,new[]{true,false,true},key:new KeyDefinition(0,target[0],1)):
                    kind=="latch"?new FloorDefinition(8,new[]{true,true,true},oneUseBridgeEdge:0):
                    new FloorDefinition(8,new[]{true,true,true},gustAfterSwaps:1,gustTransform:EntryTransform.ShiftRightOne,gustDamage:1);
                g.LoadLevel(new LevelDefinition("mark-review-"+kind,"C02","2.0","2.0","review",row,target,
                    new[]{new TargetSegment(SegmentKind.Shapes,0,4)},kind=="gust"?new[]{floor,new FloorDefinition(4,new[]{true,true,true})}:new[]{floor}));g.ShowGame();
                yield return Until(()=>g.Interaction.CanPlay,"Mark fixture blocked: "+kind);
                yield return new WaitForSecondsRealtime(.45f);
                yield return PlayableCapture.CaptureSettled(Phase,kind+"-before.png",390,844);
                string hash=g.Interaction.Attempt.CurrentStateHash;
                int edge=kind=="crack"?1:0;
                g.Interaction.TapSlot(edge);g.Interaction.TapSlot(edge+1);
                yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Mark exchange stuck: "+kind);
                Check(g.Interaction.Attempt.CurrentStateHash!=hash,"Mark fixture never accepted exchange: "+kind);
                if(kind=="locked")Check(g.VisibleFrame.KeyUsed&&g.VisibleFrame.EdgeVisibleOpen(floor,1),"Key failed to open displayed latch.");
                if(kind=="latch")Check(g.VisibleFrame.BridgeUsed&&!g.VisibleFrame.EdgeVisibleOpen(floor,0),"One-use latch did not close.");
                if(kind=="crack")Check(g.VisibleFrame.Durability==6,"Cracked edge did not cost two durability.");
                yield return new WaitForSecondsRealtime(.45f);
                yield return PlayableCapture.CaptureSettled(Phase,kind+"-after.png",390,844);
                g.GetComponentsInChildren<Button>().Single(b=>b.name=="Undo").onClick.Invoke();
                yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Mark Undo stuck: "+kind);
                Check(g.Interaction.Attempt.CurrentStateHash==hash,"Mark Undo did not restore actual snapshot: "+kind);
            }
            var dense=new[]{"F","B","A","E","D","C"}.Select(s=>SymbolId.Parse("s:"+s)).ToArray();
            g.LoadLevel(new LevelDefinition("dense-mark-review","C403","2.0","2.0","review",dense,dense.Reverse(),
                new[]{new TargetSegment(SegmentKind.Shapes,0,6)},new[]{
                    new FloorDefinition(8,new[]{true,false,true,true,true},fragileEdges:new[]{3}),
                    new FloorDefinition(4,new[]{true,true,true,true,true},entryTransform:EntryTransform.MirrorNext),
                    new FloorDefinition(4,new[]{true,true,true,true,true},entryTransform:EntryTransform.ShiftLeftOne,gustAfterSwaps:2,gustTransform:EntryTransform.ShiftRightOne),
                    new FloorDefinition(4,new[]{true,true,true,true,true},linkedPair:new LinkedPair(0,3))}));g.ShowGame();
            yield return Until(()=>g.Interaction.CanPlay,"Dense mark fixture blocked.");
            yield return new WaitForSecondsRealtime(.45f);
            yield return PlayableCapture.CaptureSettled(Phase,"dense-small.png",320,693);
            Check(SessionState.GetInt(Prefix+"Errors",0)==0,"Unity logged errors during platform review.");
            string dir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase));
            File.WriteAllText(Path.Combine(dir,"native-review.txt"),"PASS: four automatic pre-move lessons; visible CanvasRenderer mesh; actual linked atomic swap; fragile two-durability exchange; key opening and one-use closure; gust exchange; actual Undo restores all four snapshots. Seventeen native renders including 320x693 dense boards. Isolated profile. No iPhone build/install. Human visual inspection recorded separately.");
            Debug.Log("CASCADE platform readability review passed.");
            SessionState.SetBool(Prefix+"Finishing",true);EditorApplication.update+=Finish;EditorApplication.isPlaying=false;
        }
    }
}
