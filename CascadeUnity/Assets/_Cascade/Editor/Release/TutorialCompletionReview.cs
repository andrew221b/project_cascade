using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using Cascade.Solver;
using Cascade.Editor.Playable;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Release
{
    // Isolated native reproduction; never opens or rewrites the player's profile.
    [InitializeOnLoad]
    public static class TutorialCompletionReview
    {
        const string Prefix="Cascade.TutorialCompletionReview.";
        static TutorialCompletionReview(){if(SessionState.GetBool(Prefix+"Auto",false))EditorApplication.update+=WaitForPlay;}
        public static void Begin()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before isolated review.");
            SessionState.SetString(Prefix+"OldProfile",SessionState.GetString("Cascade.NavigationReviewProfile",""));
            SessionState.SetBool(Prefix+"OldBackground",UnityEngine.Application.runInBackground);
            NavigationCapture.Prepare();UnityEngine.Application.runInBackground=true;
            SessionState.SetBool(Prefix+"Auto",true);EditorApplication.update-=WaitForPlay;EditorApplication.update+=WaitForPlay;EditorApplication.isPlaying=true;
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
            var old=SessionState.GetString(Prefix+"OldProfile","");
            if(old.Length==0)NavigationCapture.Clear();else{SessionState.SetString("Cascade.NavigationReviewProfile",old);CascadeGame.EditorProfileDirectory=old;}
            UnityEngine.Application.runInBackground=SessionState.GetBool(Prefix+"OldBackground",false);SessionState.EraseString(Prefix+"OldProfile");
        }
        static IEnumerator Until(Func<bool> condition,string description)
        {
            float deadline=Time.realtimeSinceStartup+20;
            while(!condition()){if(Time.realtimeSinceStartup>deadline)throw new InvalidOperationException(description);yield return null;}
        }
        static LineRenderer[] Ropes(CascadeGame game)=>game.Machine.GetComponentsInChildren<LineRenderer>(true).Where(r=>r.name=="Left braided rope"||r.name=="Right braided rope").ToArray();
        static void CheckRopes(CascadeGame game){var ropes=Ropes(game);if(ropes.Length!=2||ropes.Any(r=>!r.enabled||!r.gameObject.activeInHierarchy||r.positionCount<25))throw new InvalidOperationException("Delivered row lost its hanging ropes.");}
        static IEnumerator Run(CascadeGame game)
        {
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;game.Preferences.NotificationsEnabled=false;game.Preferences.Tutorials=true;
            game.ReducedMotion=true;game.SetUiLocale("en");
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().Where(m=>m!=MechanicLesson.Undo).ToList();
            profile.SeenCoachPrompts.Add("controls-two-ways-v1");
            foreach(var lesson in profile.SeenMechanics)profile.SeenCoachPrompts.Add("triad-guide-v6-"+(int)lesson);
            var source=game.Catalog.GetPrimary("C01").Definition;game.StartCampaignLevel(source);game.ShowGame();
            yield return Until(()=>game.Interaction.CanPlay,"First stage did not become playable.");
            game.Interaction.Execute(PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path[0]);
            yield return Until(()=>game.CurrentMechanic==MechanicLesson.Undo,"First Undo tutorial did not open.");
            game.AcknowledgeMechanic();yield return Until(()=>game.IsTutorialHandVisible,"Undo hand did not appear.");
            game.Undo();yield return Until(()=>!game.Interaction.IsPresenting,"Undo presentation remained locked.");
            var attempt=game.Interaction.Attempt;
            if(attempt.UndoCount!=0||attempt.MovesMade!=0||attempt.FreeUndoRemaining!=2||!profile.SeenMechanics.Contains(MechanicLesson.Undo))throw new InvalidOperationException("First tutorial Undo was charged.");
            game.Preferences.Tutorials=false; // Other control lessons are outside this focused reproduction.
            int ropeChecks=0;
            for(int stage=0;stage<3;stage++){
                while(game.Interaction.Attempt.Current.Status==PuzzleStatus.Playing){
                    yield return Until(()=>game.Interaction.CanPlay,"Stage controls remained locked.");
                    attempt=game.Interaction.Attempt;var route=PuzzleSolver.FindShortest(attempt.Resolver,attempt.Current);
                    if(route.Path.Count==0)throw new InvalidOperationException("No completion witness.");
                    game.Interaction.Execute(route.Path[0]);
                    while(game.Interaction.IsPresenting){if(game.VisibleFrame.Status==PuzzleStatus.Won&&game.CurrentScreen==GameScreen.Playing){CheckRopes(game);ropeChecks++;}yield return null;}
                }
                if(stage<2){CheckRopes(game);ropeChecks++;game.GoLower();yield return Until(()=>game.Descent.StageIndex==stage+1&&game.Interaction.CanPlay,"Next stage did not start.");}
            }
            yield return Until(()=>game.CurrentScreen==GameScreen.Win,"Completion screen did not open.");
            if(game.LastCompletion.Undos!=0||game.LastCompletion.Moves!=game.LastCompletion.Minimum||game.LastCompletion.Stars!=5)throw new InvalidOperationException("Tutorial reduced the final rating.");
            game.LastCompletion.Stars=4;game.Preferences.Tutorials=false;game.ReducedMotion=false;
            foreach(var locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);game.ShowGame();
                yield return new WaitForSecondsRealtime(.15f);
                yield return PlayableCapture.CaptureSettled("TutorialCompletion-Oct09","completion-"+locale+".png",390,844,g=>{
                    g.GetComponentInChildren<RewardCelebration>().SetPreview(3.2f);
                    var title=g.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Completion title");title.ForceMeshUpdate();
                    if(locale!="en"&&title.text=="Beautifully done!")throw new InvalidOperationException("Localized title did not load: "+locale);
                    if(title.isTextOverflowing||title.textInfo.lineCount!=1)throw new InvalidOperationException("Completion title does not fit: "+locale);
                });
            }
            game.SetUiLocale("en");game.ShowGame();
            yield return PlayableCapture.CaptureSettled("TutorialCompletion-Oct09","stars-arrival.png",390,844,g=>g.GetComponentInChildren<RewardCelebration>().SetPreview(1.2f));
            var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/TutorialCompletion-Oct09"));
            File.WriteAllText(Path.Combine(folder,"native-review.txt"),"PASS: actual first C01 Undo tutorial, uncharged move/undo/allowance, all three stages, optimal five-star receipt, "+ropeChecks+" terminal-frame/checkpoint rope checks, EN/RU/UK one-line ribbons and sequenced star preview. Isolated profile; no phone build or installation.");
        }
    }
}
