using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Presentation;
using Cascade.Solver;
using TMPro;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class CampaignLogicReview
    {
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(game==null)throw new InvalidOperationException("Requires Play Mode with an isolated profile.");
            game.StartCoroutine(Review(game));
        }
        static IEnumerator Review(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;game.Preferences.Tutorials=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.SetUiLocale("en");
            foreach(string node in new[]{"C02","C03","C09","C36","C161"}){
                var source=game.Catalog.GetPrimary(node).Definition;game.StartCampaignLevel(source);game.ShowGame();
                yield return Ready(()=>game.PreparationReady&&game.Interaction.CanPlay);
                Check(game.Descent==null&&game.Interaction.Attempt.PinnedContentHash==source.ContentHash,"Published puzzle pin");
                yield return PlayableCapture.CaptureSettled("CampaignLogic-Oct08",node+"-authored.png",390,844);
                if(node!="C09")continue;
                var transition=game.Interaction.Execute(PuzzleCommand.Drop);
                Check(transition.Accepted&&transition.Events.Count(e=>e.Kind==PuzzleEventKind.EntryPermuted)==1,"Actual roller landing");
                yield return Ready(()=>!game.Interaction.IsPresenting);
                yield return PlayableCapture.CaptureSettled("CampaignLogic-Oct08",node+"-landed.png",390,844);
            }
            // Legacy saves still need their checkpoint ribbon to fit all locales.
            game.StartDescentLevel(game.Catalog.GetPrimary("C01").Definition);game.ShowGame();
            yield return Ready(()=>game.Descent!=null&&game.PreparationReady&&game.Interaction.CanPlay);
            var proof=PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver);
            foreach(var command in proof.Path){game.Interaction.Execute(command);yield return Ready(()=>!game.Interaction.IsPresenting);}
            yield return Ready(()=>game.CurrentScreen==GameScreen.Checkpoint);
            foreach(string language in new[]{"en","ru","uk"}){
                game.SetUiLocale(language);game.Preferences.LargeText=language=="uk";game.ShowGame();
                yield return new WaitForSecondsRealtime(.3f);
                yield return PlayableCapture.CaptureSettled("CampaignLogic-Oct08","checkpoint-"+language+".png",language=="uk"?320:390,language=="uk"?693:844);
            }
            string directory=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","CampaignLogic-Oct08");
            File.WriteAllText(Path.Combine(directory,"visual-review.txt"),"PASS: fresh authored pins; actual C09 entry transition; 9 native portrait captures, including EN/RU/UK legacy checkpoints.\nPhysical-device gameplay is not measured by these captures.\n");
            Debug.Log("CASCADE campaign visual review complete");
        }
        static IEnumerator Ready(Func<bool> condition)
        {
            double began=Time.realtimeSinceStartupAsDouble;
            while(!condition()&&Time.realtimeSinceStartupAsDouble-began<40)yield return null;
            Check(condition(),"Gameplay review timed out");
        }
        static void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    }
}
