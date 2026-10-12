using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Presentation;
using Cascade.Editor.Playable;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Release
{
    // Real UI callbacks, isolated temporary save; never touches the device profile.
    [InitializeOnLoad]
    public static class WildlifeFlightReview
    {
        const string Prefix="Cascade.WildlifeFlightReview.";
        const string Phase="WildlifeFlight-Oct10";
        public static void BeginBatch()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
            ConfigureArt();SessionState.SetBool(Prefix+"Batch",true);Begin();
        }
        static WildlifeFlightReview()
        {
            if(SessionState.GetBool(Prefix+"Finishing",false)){EditorApplication.update+=Finish;return;}
            if(!SessionState.GetBool(Prefix+"Auto",false))return;
            // A source reload during this isolated review restarts its fixtures.
            SessionState.SetBool(Prefix+"Started",false);UnityEngine.Application.logMessageReceived+=RecordError;EditorApplication.update+=Tick;
        }
        static void Finish()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)return;
            EditorApplication.update-=Finish;SessionState.SetBool(Prefix+"Finishing",false);RestoreSession();
            if(SessionState.GetBool(Prefix+"Batch",false)) {
                SessionState.SetBool(Prefix+"Batch",false);
                runner=ScriptableObject.CreateInstance<TestRunnerApi>();runner.hideFlags=HideFlags.HideAndDontSave;
                runner.RegisterCallbacks(new MathResults());
                runner.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,testNames=new[]{"Cascade.Tests.WildlifeMotionTests","Cascade.Tests.WildlifePerchMotionTests"}}));
            }
        }
        static TestRunnerApi runner;
        sealed class MathResults:ICallbacks
        {
            public void RunStarted(ITestAdaptor tests){}public void TestStarted(ITestAdaptor test){}public void TestFinished(ITestResultAdaptor result){}
            public void RunFinished(ITestResultAdaptor result) {
                TestRunnerApi.SaveResultToFile(result,Path.Combine(Output,"editmode-final.xml"));
                Debug.Log("CASCADE wildlife math: "+result.PassCount+" passed; "+result.FailCount+" failed.");
                EditorApplication.Exit(result.FailCount==0?0:2);
            }
        }
        public static void Begin()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            SessionState.SetString(Prefix+"OldProfile",SessionState.GetString("Cascade.NavigationReviewProfile",""));
            SessionState.SetBool(Prefix+"OldBackground",UnityEngine.Application.runInBackground);
            NavigationCapture.Prepare();UnityEngine.Application.runInBackground=true;
            SessionState.SetInt(Prefix+"Errors",0);SessionState.SetFloat(Prefix+"Began",(float)EditorApplication.timeSinceStartup);
            UnityEngine.Application.logMessageReceived-=RecordError;UnityEngine.Application.logMessageReceived+=RecordError;
            SessionState.SetBool(Prefix+"Auto",true);SessionState.SetBool(Prefix+"Started",false);
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
        }
        static void RecordError(string text,string trace,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)SessionState.SetInt(Prefix+"Errors",SessionState.GetInt(Prefix+"Errors",0)+1);}
        static void Tick()
        {
            if(SessionState.GetBool(Prefix+"Batch",false)&&EditorApplication.timeSinceStartup-SessionState.GetFloat(Prefix+"Began",0)>180){
                Debug.LogError("CASCADE wildlife review timed out; no successful validation.");EditorApplication.Exit(2);return;
            }
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))return;
            EditorApplication.QueuePlayerLoopUpdate();
            if(SessionState.GetBool(Prefix+"Started",false))return;
            SessionState.SetBool(Prefix+"Started",true);game.StartCoroutine(Run(game));
        }
        public static void RestoreSession()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            UnityEngine.Application.logMessageReceived-=RecordError;EditorApplication.update-=Tick;SessionState.SetBool(Prefix+"Auto",false);SessionState.SetBool(Prefix+"Started",false);
            string old=SessionState.GetString(Prefix+"OldProfile","");
            if(old.Length==0)NavigationCapture.Clear();else{SessionState.SetString("Cascade.NavigationReviewProfile",old);CascadeGame.EditorProfileDirectory=old;}
            UnityEngine.Application.runInBackground=SessionState.GetBool(Prefix+"OldBackground",false);
            SessionState.EraseString(Prefix+"OldProfile");
        }
        const System.Reflection.BindingFlags Private=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        static string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase));
        static void ConfigureArt()
        {
            const string path="Assets/_Cascade/Resources/CascadeArt/Atmosphere/WildlifeRearOct10.png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;
            importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Trilinear;importer.mipmapEnabled=true;
            importer.maxTextureSize=2048;importer.npotScale=TextureImporterNPOTScale.None;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
        }
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        static IEnumerator Run(CascadeGame game)
        {
            Directory.CreateDirectory(Output);
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;game.Preferences.NotificationsEnabled=false;
            game.Preferences.Tutorials=false;game.ReducedMotion=false;game.Preferences.EffectQuality=2;
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
            typeof(CascadeGame).GetField("persistenceReady",Private).SetValue(game,false);
            game.LoadNode("C03");game.ShowGame();
            float timeout=Time.realtimeSinceStartup+35;
            while(!game.PreparationReady){Check(Time.realtimeSinceStartup<timeout,"Gameplay preparation timed out.");yield return null;}
            game.Machine.SetAmbientPaused(true);
            var world=game.Machine.LivingWorld;world.ReducedMotion=false;world.EffectQuality=2;world.SetQuietFlight(false);
            var life=world.GetComponent<WorldLifeView>();var extra=world.GetComponentInChildren<WorldLifeExtraView>();
            string hash=game.Interaction.Attempt.CurrentStateHash;int maximum=0;float best=0;int renders=0;
            int pooled=life.GetComponentsInChildren<MeshRenderer>().Count(r=>r.name=="Painted part");
            for(int zone=0;zone<9;zone++) {
                game.Machine.SetZone(zone);game.RenderFrame(game.VisibleFrame);world.SetQuietFlight(false);
                for(int frame=0;frame<1800;frame++) {
                    float phase=frame/10f;world.Paint(phase);
                    int count=life.ActiveCreatureCount+life.ExtraCreatureCount;
                    Check(count<=4&&life.ExtraCreatureCount<=1,"Population limit exceeded.");
                    if(count>maximum){maximum=count;best=phase;}
                    if(frame%150==0)yield return null;
                }
                Check(life.GetComponentsInChildren<MeshRenderer>().Count(r=>r.name=="Painted part")==pooled,"Wildlife allocated extra pooled objects.");
            }
            game.Machine.SetZone(0);game.RenderFrame(game.VisibleFrame);world.SetQuietFlight(false);
            world.Paint(best);var positions=life.GetComponentsInChildren<Transform>().Select(t=>t.localToWorldMatrix).ToArray();
            world.Paint(best);var same=life.GetComponentsInChildren<Transform>().Select(t=>t.localToWorldMatrix).ToArray();
            Check(positions.SequenceEqual(same),"Frozen ambient clock moved a rig.");
            yield return PlayableCapture.CaptureSettled(Phase,"gameplay.png",390,844,g=>world.Paint(best));renders++;
            world.ReducedMotion=true;world.Paint(best+.1f);
            Check(life.ActiveCreatureCount+life.ExtraCreatureCount==0,"Reduced Motion left visitors visible.");world.ReducedMotion=false;
            yield return Film();
            Check(hash==game.Interaction.Attempt.CurrentStateHash,"Decorative fauna changed puzzle state.");
            Check(SessionState.GetInt(Prefix+"Errors",0)==0,"Unity logged an error during wildlife review.");
            File.WriteAllText(Path.Combine(Output,"native-review.txt"),"PASS: nine worlds sampled for 180 seconds at 10 Hz; max visitors="+maximum+" (max one extra); stable pooled renderers="+pooled+"; freeze/resume; Reduced Motion; unchanged puzzle hash. All 17 dorsal/side species captured; actual Fly route recorded for four styles over 18 seconds at 12 fps. Native Unity camera renders, not synthetic screenshots. No phone build or physical-device performance test.\n");
            Debug.Log("CASCADE wildlife flight review passed.");
            game.Machine.SetAmbientPaused(false);SessionState.SetBool(Prefix+"Finishing",true);EditorApplication.update+=Finish;EditorApplication.isPlaying=false;
        }
        static IEnumerator Film()
        {
            var assembly=typeof(WorldLifeView).Assembly;
            var libraryType=assembly.GetType("Cascade.Presentation.WildlifeRigLibrary");var rigType=assembly.GetType("Cascade.Presentation.WildlifeRig");
            var library=Activator.CreateInstance(libraryType,new object[]{ToyArt.Load()});
            var select=rigType.GetMethod("Select");var fly=rigType.GetMethod("Fly");var hide=rigType.GetMethod("Hide");
            var paintRear=rigType.GetMethod("PaintRear",Private);var paint=rigType.GetMethod("Paint");
            var rootField=rigType.GetField("Root");
            var parent=new GameObject("Temporary dorsal wildlife review");parent.layer=30;
            var cameraObject=new GameObject("Temporary wildlife render camera");var camera=cameraObject.AddComponent<Camera>();
            camera.enabled=false;camera.cullingMask=1<<30;camera.orthographic=true;camera.orthographicSize=5;
            camera.aspect=800f/680;camera.transform.position=new Vector3(100,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.57f,.81f,.93f);camera.nearClipPlane=.1f;camera.farClipPlane=30;
            var render=new RenderTexture(800,680,24);var pixels=new Texture2D(800,680,TextureFormat.RGB24,false);camera.targetTexture=render;
            var previous=RenderTexture.active;var rigs=new object[17];
            try {
                for(int i=0;i<17;i++){rigs[i]=Activator.CreateInstance(rigType,new object[]{parent.transform,"Review species "+i,library});select.Invoke(rigs[i],new object[]{(WildlifeSpecies)i});}
                camera.orthographicSize=6.2f;
                foreach(bool rear in new[]{false,true}) {
                    for(int i=0;i<17;i++) {
                        var pose=WildlifeMotion.Pose(WildlifeMotion.Style((WildlifeSpecies)i),1.3f,1.3f,.3f);
                        paint.Invoke(rigs[i],new object[]{new Vector3(100+(i%4-1.5f)*3,4.5f-i/4*2.3f,0),Quaternion.identity,.68f,1f,pose,Color.white});
                        if(rear)paintRear.Invoke(rigs[i],new object[]{1f,Vector3.up,0f,pose,Color.white});
                    }
                    Write(Path.Combine(Output,rear?"rear-species.png":"side-species.png"));
                }
                foreach(var rig in rigs)hide.Invoke(rig,null);
                camera.orthographicSize=5;int[] subjects={0,9,8,16};float[] rates={4,1.45f,7.8f,1.15f};
                string frames=Path.Combine(Output,"frames");Directory.CreateDirectory(frames);
                for(int frame=0;frame<=216;frame++) {
                    float age=frame/12f;
                    for(int n=0;n<subjects.Length;n++) {
                        var rig=rigs[subjects[n]];float seed=.62f+n*.013f;
                        WildlifeMotion.Route(-1,seed,true,false,out var a,out var b,out var c,out var d);
                        fly.Invoke(rig,new object[]{camera,a,b,c,d,age,18f,rates[n],seed,.006f,0,-1f,1.2f,10f,Color.white,.95f,Vector2.zero,.8f});
                        var root=(Transform)rootField.GetValue(rig);
                        // Only the film layout is scaled. Fly supplies the actual path, heading,
                        // deformation and rear blend used by the live game.
                        root.position=new Vector3(100+(n%2==0?-2.8f:2.8f)+(root.position.x-100)*.36f,(n<2?2.3f:-2.3f)+root.position.y*.48f,0);
                        root.localScale*=1.35f;
                        Check(Mathf.Abs(root.localScale.x)>.01f,"A visitor collapsed into an edge.");
                    }
                    Write(Path.Combine(frames,frame.ToString("D4")+".png"));
                    if(frame%6==0)yield return null;
                }
            }
            finally {
                RenderTexture.active=previous;camera.targetTexture=null;foreach(var rig in rigs)if(rig!=null)rigType.GetMethod("Dispose").Invoke(rig,null);
                libraryType.GetMethod("Dispose").Invoke(library,null);UnityEngine.Object.Destroy(parent);UnityEngine.Object.Destroy(cameraObject);
                render.Release();UnityEngine.Object.Destroy(render);UnityEngine.Object.Destroy(pixels);
            }
            void Write(string path){camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,800,680),0,0);pixels.Apply();File.WriteAllBytes(path,pixels.EncodeToPNG());}
        }
    }
}
