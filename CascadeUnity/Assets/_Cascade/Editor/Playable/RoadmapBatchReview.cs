using System;
using System.IO;
using Cascade.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Cascade.Editor.Playable
{
    // Isolated editor review, never a player build or a real-player save.
    [InitializeOnLoad]
    public static class RoadmapBatchReview
    {
        const string Key="Cascade.RoadmapBatchReview";
        const string Phase="ScreenRoadmap-Oct09";
        static bool started;
        static RoadmapBatchReview(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
        public static void Begin()
        {
            string path=UnityEngine.Application.dataPath;
            bool temporary=path.StartsWith(Path.GetTempPath(),StringComparison.Ordinal)||path.StartsWith("/private/tmp/",StringComparison.Ordinal)||path.StartsWith("/tmp/",StringComparison.Ordinal);
            if(!UnityEngine.Application.isBatchMode||!temporary||!path.Contains("CascadeRoadmapReview"))
                throw new InvalidOperationException("Use an isolated temporary batch project.");
            ScreenGalleryCapture.Prepare();
            EditorSceneManager.OpenScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
            SessionState.SetBool(Key,true);SessionState.SetString(Key+"Start",DateTime.UtcNow.ToString("O"));
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
        }
        static void Tick()
        {
            if(DateTime.TryParse(SessionState.GetString(Key+"Start",""),out var start)&&DateTime.UtcNow-start.ToUniversalTime()>TimeSpan.FromMinutes(8)){Finish(2);return;}
            if(!EditorApplication.isPlaying)return;
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(game==null||!game.PreparationReady)return;
            if(!started){started=true;ScreenGalleryCapture.Capture(Phase);return;}
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase));
            if(!ScreenGalleryCapture.Running&&File.Exists(Path.Combine(folder,"complete.txt"))){
                Debug.Log(File.ReadAllText(Path.Combine(folder,"complete.txt")));
                Finish(new FileInfo(Path.Combine(folder,"failures.txt")).Length==0?0:1);
            }
        }
        static void Finish(int code){SessionState.EraseBool(Key);EditorApplication.update-=Tick;NavigationCapture.Clear();EditorApplication.Exit(code);}
    }
}
