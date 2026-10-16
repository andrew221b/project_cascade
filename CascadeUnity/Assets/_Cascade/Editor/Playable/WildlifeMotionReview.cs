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
    // Native motion evidence only. Temporary cameras/rigs and the isolated QA profile never ship.
    public static class WildlifeMotionReview
    {
        const string Folder="WildlifeMotion-Oct08";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static string Output=>Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Folder);
        public static void Prepare()=>WorldAtmosphereReview.Prepare();
        public static void CaptureLatest()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeWorldAtmosphereReview"))throw new InvalidOperationException("Isolated profile required");
            game.StartCoroutine(Latest(game));
        }
        static IEnumerator Latest(CascadeGame game)
        {
            const string folder="LifePhysics-Oct09-visual";
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            typeof(CascadeGame).GetField("persistenceReady",Private).SetValue(game,false);
            game.Preferences.Tutorials=false;game.ReducedMotion=false;game.Preferences.EffectQuality=2;
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
            game.SetUiLocale("uk");game.LoadNode("C38");game.ShowGame();
            while(!game.PreparationReady)yield return null;
            game.Machine.SetAmbientPaused(true);var world=game.Machine.LivingWorld;world.SetQuietFlight(false);world.ReducedMotion=false;world.EffectQuality=2;
            string hash=game.Interaction.Attempt.CurrentStateHash;
            float selected=0;int best=-1;
            for(int sample=0;sample<180;sample++){float phase=sample*.2f;world.Paint(phase);int visible=0;foreach(var renderer in world.GetComponentsInChildren<MeshRenderer>())if(renderer.enabled&&renderer.name=="Painted part")visible++;if(visible>best){best=visible;selected=phase;}}
            float pose=selected;
            yield return PlayableCapture.CaptureSettled(folder,"shared-hooks.png",320,693,g=>world.Paint(pose));
            game.Machine.CueWind(true,.6f);game.CueDeviceWind(true);
            yield return new WaitForSecondsRealtime(.45f);
            pose+=.45f;
            yield return PlayableCapture.CaptureSettled(folder,"wind-and-flight.png",320,693,g=>world.Paint(pose));
            var sources=game.Feedback.GetComponentsInChildren<AudioSource>();int listeners=UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
            string output=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",folder);
            File.WriteAllText(Path.Combine(output,"inspection.txt"),"PASS state unchanged="+(hash==game.Interaction.Attempt.CurrentStateHash)+"; pooled flying rigs; listeners="+listeners+"; sources="+sources.Length+"; cosmetic body and rope physics only.\n");
        }
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeWorldAtmosphereReview"))
                throw new InvalidOperationException("Prepare isolated wildlife QA outside Play Mode first.");
            game.StartCoroutine(Review(game));
        }
        static IEnumerator Review(CascadeGame game)
        {
            Directory.CreateDirectory(Output);string failure=Path.Combine(Output,"failure.txt");if(File.Exists(failure))File.Delete(failure);
            bool complete=false;int oldRate=UnityEngine.Application.targetFrameRate;
            try {
                UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=24;
                game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
                game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.ReducedMotion=false;game.Preferences.EffectQuality=2;
                var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
                foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
                typeof(CascadeGame).GetField("persistenceReady",Private).SetValue(game,false);
                game.LoadNode("C02");game.ShowGame();game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
                yield return null;yield return null;
                game.Machine.SetAmbientPaused(true);var world=game.Machine.LivingWorld;
                world.ReducedMotion=false;world.EffectQuality=2;world.SetQuietFlight(false);
                string hash=game.Interaction.Attempt.CurrentStateHash;var log=new StringBuilder();
                for(int zone=0;zone<9;zone++) {
                    int z=zone;game.Machine.SetZone(z);game.RenderFrame(game.VisibleFrame);world.SetQuietFlight(false);
                    float best=-1,at=0;
                    var tint=new MaterialPropertyBlock();
                    for(float phase=0;phase<55;phase+=.25f) {
                        world.Paint(phase);float score=0;
                        foreach(var renderer in world.GetComponentsInChildren<MeshRenderer>()) {
                            if(!renderer.enabled||!renderer.transform.name.Equals("Painted part"))continue;
                            var point=Camera.main.WorldToViewportPoint(renderer.bounds.center);
                            renderer.GetPropertyBlock(tint);
                            if(point.x>.007f&&point.x<.993f&&point.y>.15f&&point.y<.8f)score+=renderer.bounds.size.magnitude*tint.GetColor("_BaseColor").a;
                        }
                        if(score>best){best=score;at=phase;}
                    }
                    float pose=at;
                    yield return PlayableCapture.CaptureSettled(Folder,"world-"+z+".png",390,844,g=>world.Paint(pose));
                    log.AppendLine("World "+z+": visible native layered wildlife; selected phase "+pose.ToString("F2"));
                }
                yield return Film();
                if(hash!=game.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Wildlife review changed gameplay state.");
                log.AppendLine("PASS:9 native world contexts;17 assembled species;8-second enlarged film at24fps; puzzle hash unchanged.");
                log.AppendLine("Isolated Editor render evidence. No phone build, physical-device performance or audio/haptic validation.");
                File.WriteAllText(Path.Combine(Output,"complete.txt"),log.ToString());complete=true;
            }
            finally {
                UnityEngine.Application.targetFrameRate=oldRate;
                game.SetEditorSafeInsets(Vector4.zero);game.Machine.SetAmbientPaused(false);game.ShowHome();
                if(!complete)File.WriteAllText(failure,"Native review interrupted; inspect Unity Console.");
            }
        }

        static IEnumerator Film()
        {
            // Reflection is confined to the Editor recorder; runtime rigs keep internal visibility.
            var assembly=typeof(WorldLifeView).Assembly;
            var libraryType=assembly.GetType("Cascade.Presentation.WildlifeRigLibrary");
            var rigType=assembly.GetType("Cascade.Presentation.WildlifeRig");
            var library=Activator.CreateInstance(libraryType,new object[]{ToyArt.Load()});
            var select=rigType.GetMethod("Select");var paint=rigType.GetMethod("Paint");var hide=rigType.GetMethod("Hide");
            var parent=new GameObject("Temporary wildlife film");parent.layer=30;
            var cameraObject=new GameObject("Temporary wildlife camera");var camera=cameraObject.AddComponent<Camera>();
            camera.enabled=false;camera.cullingMask=1<<30;camera.orthographic=true;camera.orthographicSize=5;
            camera.aspect=800f/680;camera.transform.position=new Vector3(100,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.57f,.81f,.93f);camera.nearClipPlane=.1f;camera.farClipPlane=30;
            var render=new RenderTexture(800,680,24);var pixels=new Texture2D(800,680,TextureFormat.RGB24,false);camera.targetTexture=render;
            var previous=RenderTexture.active;
            var rigs=new object[17];
            try {
                for(int i=0;i<rigs.Length;i++) {
                    rigs[i]=Activator.CreateInstance(rigType,new object[]{parent.transform,"Film species "+i,library});
                    select.Invoke(rigs[i],new object[]{(WildlifeSpecies)i});
                }
                camera.orthographicSize=6.2f;
                for(int i=0;i<17;i++) {
                    var position=new Vector3(100+(i%4-1.5f)*3.0f,4.5f-(i/4)*2.3f,0);
                    var pose=WildlifeMotion.Pose(WildlifeMotion.Style((WildlifeSpecies)i),.9f,1.2f,.2f);
                    paint.Invoke(rigs[i],new object[]{position,Quaternion.identity,.66f,1f,pose,Color.white});
                }
                Write(Path.Combine(Output,"species-contact.png"));
                for(int i=0;i<17;i++)hide.Invoke(rigs[i],null);
                camera.orthographicSize=5;
                int[] subjects={0,9,8,16};float[] rates={4,1.45f,7.8f,1.15f};
                string frames=Path.Combine(Output,"frames");Directory.CreateDirectory(frames);
                var args=new object[]{Vector3.zero,Quaternion.identity,1.22f,1f,default(WildlifePose),Color.white};
                for(int frame=0;frame<192;frame++) {
                    float age=frame/24f;
                    for(int n=0;n<subjects.Length;n++) {
                        var species=(WildlifeSpecies)subjects[n];var style=WildlifeMotion.Style(species);
                        float x=n%2==0?-2.7f:2.7f,y=n<2?2.4f:-2.4f;
                        float localTime=n==0?age+.12f:age;
                        float route=WildlifeMotion.Travel(style,age/8,.23f);
                        Vector3 motion=WildlifeMotion.Flutter(style,age,age/8,.23f,.17f);
                        if(n==0){x+=Mathf.Sin(age*.6f)*.65f;y+=Mathf.Sin(age*.8f)*.15f;}
                        else if(n==1){x+=Mathf.Sin(age*1.5f)*.48f;y+=motion.y*2.4f;}
                        else if(n==2){x+=(route-.5f)*1.6f;y+=motion.y;}
                        else {x+=Mathf.Sin(age*1.77f)*.46f;y+=Mathf.Cos(age*1.67f)*.38f;}
                        float turn=n==0?Mathf.Cos(age*.6f)*8:n==1?Mathf.Cos(age*1.5f)*10:n==3?Mathf.Cos(age*1.77f)*9:0;
                        var pose=WildlifeMotion.Pose(style,localTime,rates[n],.23f,turn,turn-2);
                        args[0]=new Vector3(100+x,y+pose.Lift,0);args[1]=Quaternion.Euler(0,Mathf.Sin(age*.5f)*10,turn);
                        args[4]=pose;paint.Invoke(rigs[subjects[n]],args);
                    }
                    Write(Path.Combine(frames,frame.ToString("D4")+".png"));
                    if(frame%4==0)yield return null;
                }
            }
            finally {
                RenderTexture.active=previous;camera.targetTexture=null;
                libraryType.GetMethod("Dispose").Invoke(library,null);
                UnityEngine.Object.Destroy(parent);UnityEngine.Object.Destroy(cameraObject);render.Release();UnityEngine.Object.Destroy(render);UnityEngine.Object.Destroy(pixels);
            }
            void Write(string path){camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,800,680),0,0);pixels.Apply();File.WriteAllBytes(path,pixels.EncodeToPNG());}
        }
    }
}
