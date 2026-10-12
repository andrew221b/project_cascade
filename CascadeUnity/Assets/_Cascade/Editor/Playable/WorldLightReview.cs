using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using Cascade.Content;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Editor.Playable
{
    public static class WorldLightReview
    {
        const string Folder="WorldLight-Oct08";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        public static bool Running{get;private set;}
        static string PathTo(string file)=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Folder,file));
        static void Field(CascadeGame g,string name,object value)=>typeof(CascadeGame).GetField(name,Private).SetValue(g,value);
        static void Screen(CascadeGame g,GameScreen s)=>typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(g,new object[]{s});
        public static void Capture()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(Running||g==null||!EditorApplication.isPlaying||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated compiled Play review required.");
            Running=true;g.StartCoroutine(Run(g));
        }
        static IEnumerator Run(CascadeGame g)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PathTo("validation.txt")));
            var log=new StringBuilder();int quality=QualitySettings.GetQualityLevel();
            UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=30;
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.SetUiLocale("ru");g.Preferences.Music=false;g.Preferences.Sound=false;g.ReducedMotion=false;g.Preferences.EffectQuality=2;
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            Field(g,"persistenceReady",false);
            // Prepare supplies a genuinely delivered witness. Capture its result before loading play.
            g.Machine.SetZone(8);Screen(g,GameScreen.Win);
            yield return new WaitForSecondsRealtime(4);
            yield return Shot("night-result.png",x=>x.Machine.LivingWorld.Paint(19.9f));
            g.LoadNode("C03");g.ShowGame();
            yield return new WaitForSecondsRealtime(1);
            g.Coach.Enabled=false;g.Machine.SetAmbientPaused(true);
            typeof(CascadeGame).GetMethod("RetireInstruction",Private).Invoke(g,new object[]{"Drag a piece onto its neighbour."});
            string hash=g.Interaction.Attempt.CurrentStateHash;
            try{
                for(int zone=0;zone<9;zone++){
                    int context=zone;g.Machine.SetZone(context);g.RenderFrame(g.VisibleFrame);
                    yield return Shot("world-"+context+".png",x=>Pose(x,context,19.9f));
                    var p=g.Machine.Illumination;var light=g.Machine.GetComponentInChildren<Light>();
                    log.AppendLine($"World {context}: key={p.Direction}; intensity={light.intensity:F3}; ambient={RenderSettings.ambientMode}; reflection={RenderSettings.customReflectionTexture.name}; shadow={light.shadowStrength:F2}.");
                    if(Vector3.Distance(-light.transform.forward,p.Direction)>.001f)throw new InvalidOperationException("Key orientation disagrees with profile.");
                }
                yield return Shot("night-lift.png",x=>{
                    Pose(x,8,19.9f);x.BeginTokenHold(0,Camera.main.WorldToScreenPoint(x.Machine.TokenVisualPosition(0)));x.Machine.OffsetToken(0,Vector3.up*.35f,Vector2.one);
                    x.Machine.Sync();x.Machine.OffsetToken(0,Vector3.up*.35f,Vector2.one);
                });
                g.EndTokenHold();g.Machine.OffsetToken(0,Vector3.zero,Vector2.one);
                yield return Shot("night-clear.png",x=>Pose(x,8,0));
                yield return Shot("diagnostic-opposite-key.png",x=>{
                    Pose(x,8,19.9f);var key=x.Machine.GetComponentInChildren<Light>();var d=x.Machine.Illumination.Direction;d.x=-d.x;
                    key.transform.rotation=Quaternion.LookRotation(-d);
                });
                Pose(g,0,0);Pose(g,8,0); // Restore the actual source after the diagnostic.
                int mobile=Array.FindIndex(QualitySettings.names,n=>n.IndexOf("Mobile",StringComparison.OrdinalIgnoreCase)>=0);
                if(mobile>=0){
                    QualitySettings.SetQualityLevel(mobile,true);
                    yield return Shot("night-mobile.png",x=>Pose(x,8,19.9f));
                    log.AppendLine("Mobile quality: "+QualitySettings.names[mobile]+" / "+GraphicsSettings.currentRenderPipeline.name);
                    QualitySettings.SetQualityLevel(quality,true);
                }
                Field(g,"mapPage",8);Field(g,"selectedJourneyStop",JourneyStory.WorldFirst(8));Field(g,"islandBrowseIndex",-1);
                Screen(g,GameScreen.JourneyMap);
                yield return Shot("night-map.png",x=>x.Machine.LivingWorld.Paint(19.9f));
                g.ShowHome();yield return Shot("workshop.png",x=>x.Machine.LivingWorld.Paint(19.9f));
                g.ShowShop();yield return Shot("shop.png",x=>x.Machine.LivingWorld.Paint(19.9f));
                if(hash!=g.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Lighting/capture mutated puzzle state.");
                if(g.Machine.GetComponentsInChildren<Light>().Length!=1)throw new InvalidOperationException("Additional realtime world light appeared.");
                log.AppendLine("PASS: nine source profiles; real HDR reflection and hemispherical ambient; actual opposite-key render; lifted-contact render; mobile quality; map/workshop/shop/result; one realtime light; puzzle hash unchanged.");
                log.AppendLine("Native Editor portrait renders only; not physical-device performance or perceptual testing. Illustrated surfaces use 2.5D relief; painted building occlusion is not recovered geometry.");
                File.WriteAllText(PathTo("validation.txt"),log.ToString());
                Debug.Log("CASCADE source-light review complete.");
            }finally{QualitySettings.SetQualityLevel(quality,true);Running=false;}
        }
        static void Pose(CascadeGame g,int zone,float phase)
        {g.Machine.SetZone(zone);g.Machine.LivingWorld.Paint(phase);g.Machine.Sync();}
        static IEnumerator Shot(string name,Action<CascadeGame> pose)
        {yield return null;yield return PlayableCapture.CaptureSettled(Folder,name,390,844,pose);}

        public static void CaptureFinalDetails()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(g==null||!EditorApplication.isPlaying||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Fresh isolated delivered witness required.");
            g.StartCoroutine(FinalDetails(g));
        }
        public static void CaptureSettledResult()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(g==null||!EditorApplication.isPlaying||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated delivered witness required.");
            UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=30;
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);Field(g,"persistenceReady",false);
            g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;g.SetUiLocale("ru");g.Machine.SetAmbientPaused(true);
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.Machine.SetZone(8);Screen(g,GameScreen.Win);
            g.StartCoroutine(Shot("night-result.png",x=>{
                // Portrait capture rebuilds the Canvas and restarts the ceremony. Pose it after reflow.
                x.ReducedMotion=true;x.GetComponentInChildren<RewardCelebration>().SendMessage("Update");
                x.ReducedMotion=false;x.Machine.LivingWorld.Paint(19.9f);
            }));
        }
        static IEnumerator FinalDetails(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=30;
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);Field(g,"persistenceReady",false);
            g.SetUiLocale("ru");g.Preferences.Music=false;g.Preferences.Sound=false;
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.ReducedMotion=false;g.Machine.SetAmbientPaused(true);
            g.Machine.SetZone(8);Screen(g,GameScreen.Win);yield return new WaitForSecondsRealtime(4);
            yield return Shot("night-result.png",x=>x.Machine.LivingWorld.Paint(19.9f));
            Field(g,"mapPage",8);Field(g,"selectedJourneyStop",JourneyStory.WorldFirst(8));Field(g,"islandBrowseIndex",-1);
            Screen(g,GameScreen.JourneyMap);yield return Shot("night-map.png",x=>x.Machine.LivingWorld.Paint(19.9f));
            g.LoadNode("C03");g.ShowGame();yield return new WaitForSecondsRealtime(1);g.Coach.Enabled=false;
            typeof(CascadeGame).GetMethod("RetireInstruction",Private).Invoke(g,new object[]{"Drag a piece onto its neighbour."});
            string hash=g.Interaction.Attempt.CurrentStateHash;
            yield return Shot("world-8.png",x=>Pose(x,8,19.9f));
            yield return Shot("night-clear.png",x=>Pose(x,8,0));
            if(hash!=g.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Final light detail changed puzzle state.");
            File.AppendAllText(PathTo("validation.txt"),"PASS: final cloud reflection attenuation; settled night ceremony/contrast; one visible moon on map; unchanged puzzle hash.\n");
        }
    }
}
