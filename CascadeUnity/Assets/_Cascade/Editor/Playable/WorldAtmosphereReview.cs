using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using Cascade.Application;
using Cascade.Content;
using Cascade.Persistence;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // An isolated Editor profile and explicit poses keep this review out of player saves.
    [InitializeOnLoad]
    public static class WorldAtmosphereReview
    {
        const string Folder="WorldAtmosphere-Oct07";
        const string ProfileKey="Cascade.WorldAtmosphereReviewProfile";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static WorldAtmosphereReview()
        {
            string isolated=SessionState.GetString(ProfileKey,"");
            if(isolated.Length>0)CascadeGame.EditorProfileDirectory=isolated;
            EditorApplication.playModeStateChanged+=state=>{
                if(state!=PlayModeStateChange.EnteredEditMode)return;
                if(SessionState.GetString(ProfileKey,"").Length==0)return;
                SessionState.EraseString(ProfileKey);CascadeGame.EditorProfileDirectory=null;
            };
        }
        [MenuItem("CASCADE/QA/Prepare world atmosphere review")]
        public static void Prepare()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Prepare outside Play Mode.");
            CascadeGame.EditorProfileDirectory=Path.Combine(Path.GetTempPath(),"CascadeWorldAtmosphereReview");
            SessionState.SetString(ProfileKey,CascadeGame.EditorProfileDirectory);
            Debug.Log("CASCADE atmosphere review uses an isolated temporary profile.");
        }
        [MenuItem("CASCADE/QA/Capture world atmosphere")]
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeWorldAtmosphereReview"))
                throw new InvalidOperationException("Prepare the isolated atmosphere profile, then start Play Mode.");
            game.StartCoroutine(Review(game));
        }
        static IEnumerator Review(CascadeGame game)
        {
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Folder));Directory.CreateDirectory(folder);
            var log=new StringBuilder();
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
            typeof(CascadeGame).GetField("persistenceReady",Private).SetValue(game,false);
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.SetUiLocale("en");game.LoadNode("C02");game.ShowGame();
            yield return new WaitForSecondsRealtime(1);
            game.Machine.SetAmbientPaused(true);game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            var world=game.Machine.LivingWorld;world.ReducedMotion=false;world.EffectQuality=2;
            var wildlife=world.GetComponent<WorldLifeView>();var night=world.GetComponent<WorldNightSkyView>();
            var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
            var render=new RenderTexture(390,844,24);var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            string hash=game.Interaction.Attempt.CurrentStateHash;
            try {
                scaler.enabled=false;camera.targetTexture=render;scaler.enabled=true;
                yield return null;game.RefreshLayout();yield return null;game.RefreshLayout();
                for(int zone=0;zone<JourneyStory.WorldCount;zone++) {
                    game.Machine.SetZone(zone);game.RenderFrame(game.VisibleFrame);world.SetQuietFlight(false);
                    float pose=0,best=-1;var tint=new MaterialPropertyBlock();
                    for(float phase=0;phase<60;phase+=.4f){
                        world.Paint(phase);float visibility=0;
                        foreach(string name in new[]{"Near wildlife","Far wildlife"}){
                            var actor=world.transform.Find(name);var renderer=actor.GetComponentInChildren<MeshRenderer>();
                            if(!renderer.enabled)continue;var p=camera.WorldToViewportPoint(actor.position);
                            if(p.x<.015f||p.x>.985f)continue;renderer.GetPropertyBlock(tint);visibility+=tint.GetColor("_BaseColor").a;
                        }
                        if(visibility>best){best=visibility;pose=phase;}
                    }
                    world.Paint(pose);Write($"world-{zone}-a.png");
                    log.AppendLine($"World {zone}: wildlife={wildlife.ActiveCreatureCount}; stars={night.ActiveStarCount}; pose={pose:F2}; source={game.Machine.GetComponentInChildren<Light>().color}; ambient={RenderSettings.ambientLight}");
                    var creature=world.transform.Find("Near wildlife");var before=creature.position;
                    world.Paint(pose);if(creature.position!=before)throw new InvalidOperationException("Same presentation phase changed wildlife position.");
                    world.Paint(pose+1.1f);Write($"world-{zone}-b.png");
                }
                game.Machine.SetZone(8);world.Paint(19.9f);Write("night-meteor.png");
                if(!night.MeteorVisible)throw new InvalidOperationException("Expected meteor at review pose.");
                world.ReducedMotion=true;world.Paint(19.9f);
                if(night.MeteorVisible||wildlife.ActiveCreatureCount!=0)throw new InvalidOperationException("Reduced Motion did not suppress moving wildlife/meteor.");
                Write("night-reduced.png");world.ReducedMotion=false;world.EffectQuality=0;world.Paint(19.9f);
                if(night.ActiveStarCount!=0||wildlife.ActiveCreatureCount!=0||world.ActiveWeatherCount!=0)throw new InvalidOperationException("Effects Off left optional scenery enabled.");
                world.EffectQuality=2;
                if(hash!=game.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Atmosphere mutated puzzle state.");
                var lights=game.Machine.GetComponentsInChildren<Light>();
                if(lights.Length!=1||lights[0].type!=LightType.Directional)throw new InvalidOperationException("World must have one directional source.");
                log.AppendLine("PASS: all9 worlds resolve wildlife; same phase stable; night meteor; Reduced Motion; effects off; one directional source; puzzle hash unchanged.");
            } finally {
                scaler.enabled=false;camera.targetTexture=oldTarget;scaler.enabled=true;RenderTexture.active=oldActive;
                render.Release();UnityEngine.Object.Destroy(render);UnityEngine.Object.Destroy(pixels);game.RefreshLayout();
            }
            typeof(CascadeGame).GetField("mapPage",Private).SetValue(game,8);
            typeof(CascadeGame).GetField("selectedJourneyStop",Private).SetValue(game,JourneyStory.WorldFirst(8));
            typeof(CascadeGame).GetField("islandBrowseIndex",Private).SetValue(game,-1);
            typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{GameScreen.JourneyMap});
            typeof(ToyMachineView).GetField("windTime",Private).SetValue(game.Machine,19.9f);game.Machine.SetAmbientPaused(true);
            yield return new WaitForSecondsRealtime(.5f);
            yield return PlayableCapture.CaptureSettled(Folder,"night-map.png",390,844);
            yield return PlayableCapture.CaptureSettled(Folder,"night-map-small.png",320,693);
            log.AppendLine("Native Editor renders:9 worlds at two poses, night meteor/reduced mode and map390/320. Explicit poses do not establish human enjoyment or device performance. No phone build.");
            File.WriteAllText(Path.Combine(folder,"validation.txt"),log.ToString());
            game.SetEditorSafeInsets(Vector4.zero);game.ShowHome();game.Machine.SetAmbientPaused(false);
            Debug.Log("CASCADE world atmosphere review complete.");
            void Write(string file){camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,file),pixels.EncodeToPNG());}
        }
    }
}
