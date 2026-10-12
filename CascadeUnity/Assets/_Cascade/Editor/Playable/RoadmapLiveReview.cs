using System;
using System.IO;
using Cascade.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    // Runs actual current UI in an isolated editor profile, including offscreen captures.
    [InitializeOnLoad]
    public static class RoadmapLiveReview
    {
        const string Key="Cascade.LiveRoadmap";
        const string ProfileKey="Cascade.NavigationReviewProfile";
        static bool started;
        static double nextFrame;
        static RoadmapLiveReview()
        {
            if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;
        }
        public static void Begin(string phase)
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling||EditorUtility.scriptCompilationFailed)
                throw new InvalidOperationException("Stopped compiled editor required.");
            if(Path.GetFileName(phase)!=phase)throw new ArgumentException("Report folder required.");
            var scene=EditorSceneManager.GetActiveScene();
            if(scene.path!="Assets/_Cascade/Scenes/CascadePlayable.unity")
                throw new InvalidOperationException("Open the playable scene without losing unsaved edits first.");
            SessionState.SetString(Key+"OldProfile",SessionState.GetString(ProfileKey,""));
            ScreenGalleryCapture.Prepare();
            SessionState.SetString(Key+"Phase",phase);
            SessionState.SetString(Key+"Start",DateTime.UtcNow.ToString("O"));
            SessionState.SetBool(Key,true);started=false;
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;
            UnityEngine.Application.runInBackground=true;
            EditorApplication.isPlaying=true;
        }
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isPlaying)return;
            if(DateTime.TryParse(SessionState.GetString(Key+"Start",""),out var start)&&DateTime.UtcNow-start.ToUniversalTime()>TimeSpan.FromMinutes(8)){
                Finish("Timed out");return;
            }
            if(!EditorApplication.isPlaying){if(started)Cleanup();return;}
            if(EditorApplication.timeSinceStartup>=nextFrame){
                nextFrame=EditorApplication.timeSinceStartup+1.0/30;
                EditorApplication.QueuePlayerLoopUpdate();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(game==null||!game.PreparationReady)return;
            UnityEngine.Application.runInBackground=true;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            if(!started){started=true;ScreenGalleryCapture.Capture(SessionState.GetString(Key+"Phase",""));return;}
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+SessionState.GetString(Key+"Phase","")));
            if(!ScreenGalleryCapture.Running&&File.Exists(Path.Combine(folder,"complete.txt")))Finish("Completed");
        }
        static void Finish(string status)
        {
            SessionState.SetString(Key+"Status",status);EditorApplication.isPlaying=false;
            started=true;
        }
        static void Cleanup()
        {
            EditorApplication.update-=Tick;SessionState.SetBool(Key,false);
            NavigationCapture.Clear();
            string old=SessionState.GetString(Key+"OldProfile","");
            if(old.Length>0){SessionState.SetString(ProfileKey,old);CascadeGame.EditorProfileDirectory=old;}
            UnityEngine.Application.runInBackground=false;
        }
    }
}
