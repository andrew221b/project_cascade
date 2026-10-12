using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cascade.Presentation;
using Cascade.Persistence;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class SaveCapture
    {
        public static void Prepare()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Prepare in Play mode before capture.");
            CascadeGame.EditorProfileDirectory=Path.Combine(Path.GetTempPath(),"CascadeSaveCapture",Guid.NewGuid().ToString("N"));
            UnityEngine.SceneManagement.SceneManager.LoadScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
        }
        public static void Run()
        {
            if(!EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Requires current Play mode.");
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Capture());
        }
        static IEnumerator Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            var ready=typeof(CascadeGame).GetField("persistenceReady",BindingFlags.NonPublic|BindingFlags.Instance);
            ready.SetValue(game,false);game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            var findings=new List<string>();
            try {
                foreach(string locale in new[]{"en","ru","uk"}) {
                    game.SetUiLocale(locale);game.SendMessage("OnApplicationFocus",true);
                    foreach(var recovery in new[]{SaveRecovery.Backup,SaveRecovery.Unreadable,SaveRecovery.NewerVersion,SaveRecovery.AttemptUnavailable,SaveRecovery.WriteFailed}) {
                        typeof(CascadeGame).GetProperty("Recovery").SetValue(game,recovery);
                        typeof(CascadeGame).GetMethod("SetScreen",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game,new object[]{GameScreen.SaveRecovery});
                        string name=locale+"-"+recovery;
                        yield return PlayableCapture.CaptureSettled("P16",name+".png",320,693,g=> {
                            foreach(var text in g.GetComponentsInChildren<TMP_Text>()) {
                                text.ForceMeshUpdate();
                                if(text.isTextOverflowing || text.text.Contains("No translation"))findings.Add(name+" / "+text.name+" / "+text.text);
                            }
                        });
                    }
                }
            }
            finally {
                typeof(CascadeGame).GetProperty("Recovery").SetValue(game,SaveRecovery.None);
                game.SetUiLocale("en");game.ShowHome();ready.SetValue(game,true);UnityEngine.Application.runInBackground=background;
            }
            File.WriteAllLines(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/P16/ui-fit-findings.txt")),findings);
            Debug.Log("CASCADE save recovery captures complete: "+findings.Count+" findings");
        }
    }
}
