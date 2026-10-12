using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Cascade.Presentation
{
 // Explicit diagnostic; never runs in ordinary play. Standalone flag is for reproducible local profiling only.
 public sealed class CraftPerformanceProbe:MonoBehaviour
 {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void StartRequested(){if(Environment.GetCommandLineArgs().Contains("-cascade-perf"))new GameObject("CASCADE performance probe").AddComponent<CraftPerformanceProbe>().StartCoroutine(Measure());}
  public static void RunEditor(){var go=new GameObject("CASCADE performance probe");go.AddComponent<CraftPerformanceProbe>().StartCoroutine(Measure());}
  static IEnumerator Measure()
  {
   var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(game==null)yield break;
   if(!UnityEngine.Application.isEditor)game.PreparePerformanceProbe();
   bool oldBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
   var samples=new double[240];int oldRate=UnityEngine.Application.targetFrameRate;UnityEngine.Application.targetFrameRate=60;
   for(int warm=0;warm<90;warm++)yield return null;
   var gc=Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Memory,"GC Allocated In Frame",1);long gcTotal=0,gcMax=0;
   long memory=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();double previous=Time.realtimeSinceStartupAsDouble;
   for(int i=0;i<samples.Length;i++){yield return null;double now=Time.realtimeSinceStartupAsDouble;samples[i]=(now-previous)*1000;previous=now;long bytes=gc.Valid?gc.LastValue:0;gcTotal+=bytes;gcMax=Math.Max(gcMax,bytes);}
   Array.Sort(samples);
   string report="hardware="+SystemInfo.deviceModel+" GPU="+SystemInfo.graphicsDeviceName+"\ncontext="+(UnityEngine.Application.isEditor?"Editor":"Player")+" resolution="+Screen.width+"x"+Screen.height+"\nframes=240 warmup=90 target=60\nmedian_ms="+samples[120].ToString("F3")+" p95_ms="+samples[228].ToString("F3")+"\nallocated_start="+memory+" allocated_end="+UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()+"\ngc_counter_valid="+gc.Valid+" gc_mean_bytes="+(gcTotal/240)+" gc_max_bytes="+gcMax+"\nrenderers="+UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length+"\n";
   string folder=UnityEngine.Application.isEditor?Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/Craft")):Path.Combine(Path.GetTempPath(),"CascadeCraftBenchmark");
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,UnityEngine.Application.isEditor?"editor-performance.txt":"craft-player-performance.txt"),report);Debug.Log("CASCADE PERF "+report);
   gc.Dispose();UnityEngine.Application.targetFrameRate=oldRate;UnityEngine.Application.runInBackground=oldBackground;
  }
 }
 public sealed partial class CascadeGame
 {
  internal void PreparePerformanceProbe(){persistenceReady=false;LoadNode("C160");ShowGame();OnApplicationFocus(true);}
 }
}
