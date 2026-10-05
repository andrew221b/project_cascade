using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class LanguageCapture
    {
        public static void CaptureReviewSet()
        {
            if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Requires running current game.");
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Capture());
        }
        static IEnumerator Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            game.SendMessage("OnApplicationFocus",true);game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            var findings=new System.Collections.Generic.List<string>();
            try {
                foreach(string locale in new[]{"en","ru","uk"}) {
                    game.SetUiLocale(locale);yield return null;yield return null;
                    game.ShowJourney();yield return Shot(locale+"-map",320,693);
                    for(int page=0;page<3;page++){Click("Later");yield return null;}yield return Shot(locale+"-rain-map",320,693);
                    game.ShowJourney();Story(15);yield return Shot(locale+"-pip",320,693);game.ShowJourney();
                    Story(0);yield return Shot(locale+"-story",320,693);game.ShowGame();
                    game.Catalog.TryGetLocalizedBinding("C32",locale,out var mixed);game.LoadLevel(mixed.Definition,locale);yield return Shot(locale+"-mixed8",320,693);
                    game.Catalog.TryGetLocalizedBinding("C37",locale,out var words);game.LoadLevel(words.Definition,locale);yield return Shot(locale+"-words",390,844);
                    game.SendMessage("TogglePause");yield return Shot(locale+"-pause",320,693);
                    game.ShowSettings();yield return Shot(locale+"-settings-sound",320,693);Click("Comfort");yield return Shot(locale+"-settings-help",320,693);
                    Click("Languages");yield return Shot(locale+"-languages",320,693);
                    game.ProposePuzzleLocale("uk");yield return Shot(locale+"-word-confirm",320,693);Click("Keepmychoice");game.ShowGame();
                    game.LoadNode("C13");game.ShowDevices();yield return Shot(locale+"-guide",320,693);
                    game.ShowJourney();Story(30);yield return Shot(locale+"-rue",320,693);
                }
            }
            finally {UnityEngine.Application.runInBackground=background;game.SetUiLocale("en");game.ShowJourney();}
            string directory=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/P15"));Directory.CreateDirectory(directory);
            File.WriteAllLines(Path.Combine(directory,"ui-fit-findings.txt"),findings);Debug.Log("CASCADE language captures complete; fit findings="+findings.Count);
            IEnumerator Shot(string name,int w,int h) {
                yield return null;yield return null;
                yield return PlayableCapture.CaptureSettled("P15",name+".png",w,h,g=> {
                    foreach(var text in g.GetComponentsInChildren<TMP_Text>()) {
                        text.ForceMeshUpdate();if(string.IsNullOrWhiteSpace(text.text))continue;
                        if(text.text.Contains("No translation")||text.isTextOverflowing)findings.Add(name+" / "+text.name+" / "+text.text.Replace("\n"," | "));
                    }
                });
            }
            void Click(string name){game.GetComponentsInChildren<Button>().Single(b=>b.name==name).onClick.Invoke();}
            void Story(int index){typeof(CascadeGame).GetMethod("OpenJourneyStory",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(game,new object[]{index,false});}
        }
    }
}
