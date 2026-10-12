using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Cascade.Editor.Playable
{
    public static class IntegrationCapture
    {
        public static void Run()
        {
            if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Requires current Play mode.");
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Capture(true));
        }
        public static void ConfirmViews()
            =>UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Capture(false));
        static IEnumerator Capture(bool sample)
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();const BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
            typeof(CascadeGame).GetField("persistenceReady",flags).SetValue(game,false);
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            game.Preferences.LargeText=game.Preferences.HighContrast=true;
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.SetUiLocale("ru");
            game.Catalog.TryGetLocalizedBinding("C32","ru",out var level);game.LoadLevel(level.Definition,"ru");
            typeof(CascadeGame).GetMethod("ApplyPreferences",flags).Invoke(game,null);game.SendMessage("OnApplicationFocus",true);
            var findings=new List<string>();
            yield return Shot("large-mixed8-ru",game,findings);
            game.ShowDevices();typeof(CascadeGame).GetField("deviceIndex",flags).SetValue(game,2);
            typeof(CascadeGame).GetMethod("DrawMenu",flags).Invoke(game,null);yield return Shot("large-bomb-guide-ru",game,findings);
            game.SetUiLocale("uk");game.ShowJourney();yield return Shot("large-map-uk",game,findings);
            game.SetUiLocale("en");game.ShowGame();game.LoadNode("C37");
            string hash=game.Interaction.Attempt.CurrentStateHash;var intervals=new List<double>();
            long startMemory=Profiler.GetTotalAllocatedMemoryLong();
            for(int i=0;i<(sample?120:0);i++){yield return null;intervals.Add(Time.unscaledDeltaTime*1000);}
            intervals.Sort();var report=new {
                scope="Unity macOS Editor frame intervals, not mobile CPU/GPU/device performance",frames=intervals.Count,
                medianMs=sample?intervals[intervals.Count/2]:0,p95Ms=sample?intervals[(int)(intervals.Count*.95)]:0,
                allocatedStartBytes=startMemory,allocatedEndBytes=Profiler.GetTotalAllocatedMemoryLong(),monoUsedBytes=Profiler.GetMonoUsedSizeLong(),
                gameplayUnchanged=hash==game.Interaction.Attempt.CurrentStateHash,largeText=true,highContrast=true
            };
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/P18"));
            if(sample)File.WriteAllText(folder+"/editor-performance.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
            File.WriteAllLines(folder+"/large-text-findings.txt",findings);
            typeof(CascadeGame).GetField("persistenceReady",flags).SetValue(game,true);UnityEngine.Application.runInBackground=background;
            Debug.Log("CASCADE integration views complete: "+findings.Count+" findings; Editor baseline saved.");
        }
        static IEnumerator Shot(string name,CascadeGame game,List<string> findings)
        {
            yield return PlayableCapture.CaptureSettled("P18",name+".png",320,693,g=>{
                foreach(var t in g.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();if(t.isTextOverflowing||t.text.Contains("No translation"))findings.Add(name+" / "+t.name+" / "+t.text);}
            });
        }
    }
}
