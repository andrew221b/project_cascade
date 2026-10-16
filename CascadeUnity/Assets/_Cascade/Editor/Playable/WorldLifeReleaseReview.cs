using System;
using System.Collections;
using System.IO;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cascade.Editor.Playable
{
    // Menu-driven fallback uses the existing Editor MCP menu capability when its
    // optional dynamic-command discovery is unavailable. Never saves an Editor scene.
    [InitializeOnLoad]
    public static class WorldLifeReleaseReview
    {
        const string Key="Cascade.WorldLifeOct15Review",Previous="Cascade.WorldLifeOct15Previous";
        static WorldLifeReleaseReview()
        {
            string current=SessionState.GetString(Key,"");if(current.Length>0)CascadeGame.EditorProfileDirectory=current;
            EditorApplication.playModeStateChanged+=state=>{
                if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key,"").Length>0)EditorApplication.delayCall+=Start;
                if(state==PlayModeStateChange.EnteredEditMode)Restore();
            };
        }
        [MenuItem("CASCADE/QA/Review seasonal life and mobile footer")]
        public static void Prepare()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Run this review from stable Edit Mode.");
            string path=Path.Combine(Path.GetTempPath(),"CascadeWorldLifeMcpOct15-"+Guid.NewGuid().ToString("N"));
            var profile=new PlayerProfile{AccountIntroSeen=true,FirstGestureShown=true};profile.Preferences.Tutorials=false;profile.Preferences.EffectQuality=2;profile.Missions.NextAfterClears=20000;
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))profile.SeenMechanics.Add(lesson);for(int i=0;i<1000;i++)profile.HeardStory.Add(i);
            new ProfileStore(Path.Combine(path,"cascade-profile.json")).Save(profile);
            SessionState.SetString(Previous,CascadeGame.EditorProfileDirectory??"");SessionState.SetString(Key,path);CascadeGame.EditorProfileDirectory=path;EditorApplication.isPlaying=true;
        }
        static void Restore()
        {
            if(SessionState.GetString(Key,"").Length==0)return;
            string previous=SessionState.GetString(Previous,"");CascadeGame.EditorProfileDirectory=previous.Length>0?previous:null;SessionState.EraseString(Key);SessionState.EraseString(Previous);
        }
        static void Start()
        {
            if(!EditorApplication.isPlaying)return;
            var host=new GameObject("Temporary seasonal mobile review"){hideFlags=HideFlags.DontSave};UnityEngine.Object.DontDestroyOnLoad(host);
            host.AddComponent<SettingsReferenceLayout>().StartCoroutine(Capture(host));
        }
        static IEnumerator Capture(GameObject host)
        {
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            try{
                yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
                var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.SetUiLocale("ru");game.SetEditorSafeInsets(new Vector4(0,.00001f,0,.00001f));
                Debug.Log("WORLD_LIFE_MCP_STARTED: fresh runtime scene, isolated profile; original Editor scene remains unsaved.");
                game.ShowHome();yield return new WaitForSecondsRealtime(7.5f);yield return PlayableCapture.CaptureSettled("WorldLifeMcpOct15","home-frame-390.png",390,844);
                game.LoadNode("C02");game.ShowGame();game.Machine.DelayAmbientGust(1000);
                foreach(int world in new[]{0,6,8}){
                    game.Machine.SetZone(world);game.RenderFrame(game.VisibleFrame);yield return new WaitForSecondsRealtime(7.5f);int zone=world;
                    yield return PlayableCapture.CaptureSettled("WorldLifeMcpOct15","world-"+world+"-390.png",390,844,g=>{g.Machine.SetZone(zone);g.RenderFrame(g.VisibleFrame);g.Machine.LivingWorld.Paint(g.Machine.AmbientPhase);});
                }
                game.SetEditorSafeInsets(new Vector4(.02f,.055f,.02f,.05f));yield return PlayableCapture.CaptureSettled("WorldLifeMcpOct15","footer-small-safe.png",320,693);game.ShowHome();
                Debug.Log("WORLD_LIFE_MCP_PASS: Five current native phone captures; natural clocks, seasonal weather and frame residents; isolated profile; no build/install/OAuth.");
            }finally{UnityEngine.Application.runInBackground=background;Restore();UnityEngine.Object.Destroy(host);}
        }
    }
}
