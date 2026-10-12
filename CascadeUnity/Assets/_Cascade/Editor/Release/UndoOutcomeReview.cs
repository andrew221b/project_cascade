using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Domain;
using Cascade.Presentation;
using Cascade.Solver;
using Cascade.Editor.Playable;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Release
{
    // Uses an isolated profile; the user's save and device are never touched.
    [InitializeOnLoad]
    public static class UndoOutcomeReview
    {
        const string Prefix="Cascade.UndoOutcomeReview.";
        static UndoOutcomeReview(){if(SessionState.GetBool(Prefix+"Auto",false))EditorApplication.update+=WaitForPlay;}
        public static void Begin()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            SessionState.SetString(Prefix+"OldProfile",SessionState.GetString("Cascade.NavigationReviewProfile",""));
            SessionState.SetBool(Prefix+"OldBackground",UnityEngine.Application.runInBackground);
            NavigationCapture.Prepare();UnityEngine.Application.runInBackground=true;
            SessionState.SetBool(Prefix+"Auto",true);EditorApplication.update-=WaitForPlay;EditorApplication.update+=WaitForPlay;
            EditorApplication.isPlaying=true;
        }
        static void WaitForPlay()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))return;
            EditorApplication.update-=WaitForPlay;SessionState.SetBool(Prefix+"Auto",false);game.StartCoroutine(Run(game));
        }
        public static void RestoreSession()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            string old=SessionState.GetString(Prefix+"OldProfile","");
            if(old.Length==0)NavigationCapture.Clear();else{SessionState.SetString("Cascade.NavigationReviewProfile",old);CascadeGame.EditorProfileDirectory=old;}
            UnityEngine.Application.runInBackground=SessionState.GetBool(Prefix+"OldBackground",false);
            SessionState.EraseString(Prefix+"OldProfile");
        }
        static IEnumerator Until(Func<bool> condition,string message)
        {
            float end=Time.realtimeSinceStartup+40;
            while(!condition()){if(Time.realtimeSinceStartup>end)throw new InvalidOperationException(message);yield return null;}
        }
        static Button UndoButton(CascadeGame game)=>game.GetComponentsInChildren<Button>().Single(b=>b.name=="Undo");
        static void Undo(CascadeGame game)
        {
            var button=UndoButton(game);
            if(!button.interactable||!game.Interaction.CanControl)throw new InvalidOperationException("Actual Undo button is disabled.");
            button.onClick.Invoke();
            if(!game.Interaction.IsPresenting)throw new InvalidOperationException("Undo did not start animated restoration.");
        }
        static IEnumerator Run(CascadeGame game)
        {
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;game.Preferences.NotificationsEnabled=false;
            game.Preferences.Tutorials=false;game.Preferences.ReducedMotion=false;game.ReducedMotion=false;game.SetUiLocale("en");
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            game.StartCampaignLevel(game.Catalog.GetPrimary("C02").Definition);game.ShowGame();
            yield return Until(()=>game.Interaction.CanPlay,"C02 did not prepare.");
            string hash=game.Interaction.Attempt.CurrentStateHash;
            game.Interaction.Execute(PuzzleCommand.Drop);
            yield return Until(()=>!game.Interaction.IsPresenting,"C02 descent never released input.");
            if(game.Interaction.Attempt.Current.ActiveFloor==0&&game.Interaction.Attempt.Current.Status!=PuzzleStatus.Lost)throw new InvalidOperationException("C02 did not fall to a floor or rejected receiver.");
            Undo(game);yield return Until(()=>!game.Interaction.IsPresenting,"C02 Undo never released input.");
            if(hash!=game.Interaction.Attempt.CurrentStateHash||!game.Interaction.CanPlay||game.CurrentScreen!=GameScreen.Playing)throw new InvalidOperationException("C02 Undo did not restore the playable snapshot.");
            // Finish all three real C02 stages and watch every final terminal frame.
            int finalFrames=0;
            for(int stage=0;stage<3;stage++){
                while(game.Interaction.Attempt.Current.Status==PuzzleStatus.Playing){
                    yield return Until(()=>game.Interaction.CanPlay,"C02 controls remained locked.");
                    var a=game.Interaction.Attempt;var route=PuzzleSolver.FindShortest(a.Resolver,a.Current);
                    if(route.Path.Count==0)throw new InvalidOperationException("C02 lost its solution.");
                    game.Interaction.Execute(route.Path[0]);
                    while(game.Interaction.IsPresenting){
                        if(stage==2&&game.VisibleFrame.Status==PuzzleStatus.Won){
                            finalFrames++;
                            var story=game.GetComponentsInChildren<RectTransform>(true).Single(t=>t.name=="Lantern delivery story");
                            var ribbon=game.GetComponentsInChildren<RectTransform>(true).Single(t=>t.name=="Stage celebration ribbon");
                            if(story.gameObject.activeInHierarchy||ribbon.gameObject.activeInHierarchy||game.CurrentScreen==GameScreen.Fail)
                                throw new InvalidOperationException("Victory flashed the intermediate outcome panel.");
                        }
                        yield return null;
                    }
                }
                if(stage<2){
                    if(game.CurrentScreen!=GameScreen.Checkpoint)throw new InvalidOperationException("Checkpoint screen was lost.");
                    game.GoLower();if(game.ConversationOpen){game.SkipConversation();game.GoLower();}yield return Until(()=>game.Descent.StageIndex==stage+1&&game.Interaction.CanPlay,"Next C02 stage did not start.");
                }
            }
            yield return Until(()=>game.CurrentScreen==GameScreen.Win,"Victory did not open the stars.");
            if(finalFrames==0)throw new InvalidOperationException("No final terminal frames were observed.");
            // Valid four-piece puzzles: collapse, reject the receiver, undo, then win.
            var row=new[]{"B","A","C","D"}.Select(s=>SymbolId.Parse("s:"+s)).ToArray();
            var target=new[]{"A","B","C","D"}.Select(s=>SymbolId.Parse("s:"+s)).ToArray();
            game.LoadLevel(new LevelDefinition("collapse-review","C02","2.0","2.0","review",row,target,
                new[]{new TargetSegment(SegmentKind.Shapes,0,4)},new[]{new FloorDefinition(1,new[]{true,true,true}),new FloorDefinition(4,new[]{true,true,true})}));game.ShowGame();
            yield return Until(()=>game.Interaction.CanPlay,"Collapse fixture did not prepare.");hash=game.Interaction.Attempt.CurrentStateHash;
            game.Interaction.Execute(PuzzleCommand.Swap(1));
            yield return Until(()=>!game.Interaction.IsPresenting,"Automatic collapse never released controls.");
            if(game.Interaction.Attempt.Current.ActiveFloor!=1)throw new InvalidOperationException("Collapse did not reach its lower platform.");
            Undo(game);yield return Until(()=>game.Interaction.CanPlay&&!game.Interaction.IsPresenting,"Collapse Undo remained locked.");
            if(hash!=game.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Collapse Undo did not restore its whole snapshot.");
            game.LoadLevel(new LevelDefinition("outcome-review","C02","2.0","2.0","review",row,target,
                new[]{new TargetSegment(SegmentKind.Shapes,0,4)},new[]{new FloorDefinition(1,new[]{true,true,true})}));game.ShowGame();
            yield return Until(()=>game.Interaction.CanPlay,"Receiver fixture did not prepare.");hash=game.Interaction.Attempt.CurrentStateHash;
            game.Interaction.Execute(PuzzleCommand.Swap(1));
            yield return Until(()=>game.CurrentScreen==GameScreen.Fail&&!game.Interaction.IsPresenting,"Failure did not settle.");
            yield return PlayableCapture.CaptureSettled("UndoOutcome-Oct09","retry-phone.png",390,844,g=>{
                g.Outcome.SetPreview(1);g.RefreshLayout();
                var title=g.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Outcome heading");title.ForceMeshUpdate();
                if(title.text!="Try again"||title.isTextOverflowing)throw new InvalidOperationException("Failure ribbon missing or overflowing.");
                if(!UndoButton(g).interactable)throw new InvalidOperationException("Failure Undo is disabled.");
            });
            Undo(game);yield return Until(()=>game.Interaction.CanPlay&&!game.Interaction.IsPresenting&&game.CurrentScreen==GameScreen.Playing,"Failure Undo remained locked.");
            if(hash!=game.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Failure Undo changed the restored snapshot.");
            game.Interaction.Execute(PuzzleCommand.Swap(0));
            yield return Until(()=>game.CurrentScreen==GameScreen.Win,"Restored puzzle could not win.");
            yield return PlayableCapture.CaptureSettled("UndoOutcome-Oct09","victory-phone.png",390,844,g=>g.GetComponentInChildren<RewardCelebration>().SetPreview(3.2f));
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/UndoOutcome-Oct09"));
            File.WriteAllText(Path.Combine(folder,"native-review.txt"),"PASS: real C02 first descent/rejected receiver + actual Undo button + animated restoration; all three C02 stages solved; "+finalFrames+" final delivery frames without intermediate panel; checkpoints retained; rejected receiver Undo + subsequent winning touch; automatic wood collapse + actual animated Undo; English Try again ribbon inspected at 390x844. Isolated profile. No phone build/install.");
            Debug.Log("CASCADE Undo/outcome review passed.");
        }
    }
}
