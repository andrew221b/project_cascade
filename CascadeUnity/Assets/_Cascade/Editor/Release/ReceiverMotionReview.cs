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
    public static class ReceiverMotionReview
    {
        const string Prefix="Cascade.ReceiverMotionReview.";
        const string Phase="ReceiverMotion-Oct10";
        public static void BeginBatch()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
            ConfigureArt();SessionState.SetBool(Prefix+"Batch",true);Begin();
        }
        static ReceiverMotionReview()
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
                runner.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,testNames=new[]{"Cascade.Tests.ReceiverPresentationTests","Cascade.Tests.MechanicLessonTests","Cascade.Tests.WildlifeMotionTests","Cascade.Tests.KeyRepairTests"}}));
            }
        }
        static TestRunnerApi runner;
        sealed class MathResults:ICallbacks
        {
            public void RunStarted(ITestAdaptor tests){}public void TestStarted(ITestAdaptor test){}public void TestFinished(ITestResultAdaptor result){}
            public void RunFinished(ITestResultAdaptor result) {
                TestRunnerApi.SaveResultToFile(result,Path.Combine(Output,"editmode-final.xml"));
                Debug.Log("CASCADE receiver math: "+result.PassCount+" passed; "+result.FailCount+" failed.");
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
            if(SessionState.GetBool(Prefix+"Batch",false)&&EditorApplication.timeSinceStartup-SessionState.GetFloat(Prefix+"Began",0)>240){
                Debug.LogError("CASCADE receiver review timed out; no successful validation.");EditorApplication.Exit(2);return;
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
            const string path="Assets/_Cascade/Content/Resources/CascadeArt/Devices/PruningSaw.png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=512;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
            LocalizationAssets.Build();
            foreach(string resource in new[]{"CascadeArt/TimberSurface","CascadeArt/ReceiverMagic"}){
                var shader=Resources.Load<Shader>(resource);Check(shader!=null&&!ShaderUtil.ShaderHasError(shader),"Invalid shader: "+resource);
            }
        }
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        static IEnumerator Until(Func<bool> condition,string failure,float seconds=18)
        {
            float deadline=Time.realtimeSinceStartup+seconds;
            while(!condition()){Check(Time.realtimeSinceStartup<deadline,failure);yield return null;}
        }
        static LevelDefinition Fixture(string id,SymbolId[] row,SymbolId[] target,FloorDefinition[] floors,int reach=0)
            =>new LevelDefinition(id,"C02","2.0","2.0","review",row,target,
                new[]{new TargetSegment(row[0].Kind==SymbolKind.Glyph?SegmentKind.Word:SegmentKind.Shapes,0,row.Length)},floors,reach);
        static void CaptureCurrent(string filename)
        {
            var camera=Camera.main;var active=RenderTexture.active;var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            try{camera.Render();RenderTexture.active=camera.targetTexture;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(Output,filename),pixels.EncodeToPNG());}
            finally{RenderTexture.active=active;UnityEngine.Object.Destroy(pixels);}
        }
        static IEnumerator Run(CascadeGame g)
        {
            var reviewRender=new RenderTexture(390,844,24);var reviewCamera=Camera.main;var oldTarget=reviewCamera.targetTexture;reviewCamera.targetTexture=reviewRender;
            try {
            yield return null;g.RefreshLayout();yield return null;g.RefreshLayout();
            Directory.CreateDirectory(Output);g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.Preferences.NotificationsEnabled=false;
            g.ReducedMotion=false;g.Preferences.Tutorials=false;
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(g);
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
            profile.SeenCoachPrompts.Add("controls-two-ways-v1");profile.SeenCoachPrompts.Add("controls-undo-v1");profile.SeenCoachPrompts.Add("closed-path-v2");
            typeof(CascadeGame).GetField("persistenceReady",Private).SetValue(g,false);
            var row=new[]{"s:C","s:B","s:A","s:D"}.Select(x=>SymbolId.Parse(x)).ToArray();var open=new[]{true,true,true};
            g.LoadLevel(Fixture("receiver-fall",row,row.Reverse().ToArray(),new[]{new FloorDefinition(4,open),new FloorDefinition(4,open)}));g.ShowGame();
            yield return Until(()=>g.Interaction.CanPlay,"Fall fixture not ready");
            string hash=g.Interaction.Attempt.CurrentStateHash;
            for(int i=0;i<100;i++)g.Machine.Sync();
            var before=g.Machine.ReceiverTargetPosition(0);g.Machine.CueWind(true);
            for(int i=0;i<60;i++){g.Machine.TickWoodDynamics(1f/60);if(i%5==0)yield return null;}
            Check(Vector3.Distance(before,g.Machine.ReceiverTargetPosition(0))>.015f,"Receiver did not react to wind");
            Check(hash==g.Interaction.Attempt.CurrentStateHash,"Receiver altered puzzle state");
            var paused=g.Machine.ReceiverTargetPosition(0);g.Interaction.SetSuspended(true);g.Machine.TickWoodDynamics(.05f);
            Check(Vector3.Distance(paused,g.Machine.ReceiverTargetPosition(0))<.0001f,"Suspension moved while suspended");g.Interaction.SetSuspended(false);
            g.Feedback.RowPhysics.BeginShake(new Vector2(.8f,.7f));Check(!g.Feedback.RowPhysics.Active,"Phone shake launched row bodies");
            yield return PlayableCapture.CaptureSettled(Phase,"heavy-receiver.png",390,844);
            Check(g.Interaction.Execute(PuzzleCommand.Drop).Accepted,"First descent rejected");
            yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"First descent stuck");
            Check(g.VisibleFrame.ActiveFloor==1,"Descent did not reach lower board");
            Check(!g.Feedback.RowPhysics.Active,"Guided fall left free collision bodies active");
            g.Undo();yield return Until(()=>g.Interaction.CanPlay&&!g.Interaction.IsPresenting,"Undo after descent stuck");
            Check(g.Interaction.Attempt.CurrentStateHash==hash,"Undo after descent failed to restore snapshot");
            // Already owned reach and newly collected ribbon both open the lesson without another gesture.
            profile.SeenMechanics.Remove(MechanicLesson.FarSwap);profile.SeenCoachPrompts.Remove("triad-guide-v6-24");
            g.Preferences.Tutorials=true;
            g.LoadLevel(Fixture("automatic-owned-reach",row,row.Reverse().ToArray(),new[]{new FloorDefinition(8,open)},1));g.ShowGame();
            yield return Until(()=>g.CurrentMechanic==MechanicLesson.FarSwap,"Owned far-swap lesson waited for a gesture");
            g.Preferences.Tutorials=false;g.AcknowledgeMechanic();
            g.LoadLevel(Fixture("automatic-ribbon",row,row.Reverse().ToArray(),new[]{new FloorDefinition(8,open,powerup:new PowerupDefinition(1,row[0],PowerupKind.Ribbon))}));g.ShowGame();
            yield return Until(()=>g.Interaction.CanPlay,"Ribbon fixture not ready");g.Preferences.Tutorials=true;
            Check(g.Interaction.Execute(PuzzleCommand.Swap(0)).Accepted,"Ribbon exchange rejected");
            yield return Until(()=>g.CurrentMechanic==MechanicLesson.FarSwap,"Collected ribbon lesson waited for a gesture");
            Check(g.Interaction.Attempt.MovesMade==1,"Automatic lesson consumed an extra move");
            g.Preferences.Tutorials=false;g.AcknowledgeMechanic();
            var dense=new[]{"s:F","s:B","s:A","s:E","s:D","s:C"}.Select(x=>SymbolId.Parse(x)).ToArray();var denseOpen=new[]{true,false,true,true,true};
            g.LoadLevel(Fixture("long-tool-cords",dense,dense.Reverse().ToArray(),new[]{
                new FloorDefinition(7,denseOpen,key:new KeyDefinition(1,dense[0],1),bomb:new BombDefinition(1,dense[0],2,1),fragileEdges:new[]{3}),
                new FloorDefinition(3,new[]{true,true,true,true,true},entryTransform:EntryTransform.MirrorNext),
                new FloorDefinition(3,new[]{true,true,true,true,true}),new FloorDefinition(4,new[]{true,true,true,true,true})}));g.ShowGame();
            yield return Until(()=>g.Interaction.CanPlay,"Dense hanging fixture blocked");
            g.Machine.CueWind(true);g.CueDeviceWind(true);yield return new WaitForSecondsRealtime(.55f);
            yield return PlayableCapture.CaptureSettled(Phase,"saw-dense-cords.png",320,693,owner=>{
                typeof(CascadeGame).GetMethod("SyncDeviceSuspension",Private).Invoke(owner,new object[]{.016f});
                var docks=(System.Collections.IEnumerable)typeof(CascadeGame).GetField("deviceDocks",Private).GetValue(owner);
                int props=0;
                foreach(var dock in docks){var type=dock.GetType();var body=(HangingPropBody)type.GetField("body").GetValue(dock);float unit=(float)type.GetField("unit").GetValue(dock);
                    Check(body.Length>=14*unit,"Dense hanging prop has no visible cord: "+body.Length+" / unit "+unit);
                    Check(float.IsFinite(body.Position.x)&&float.IsFinite(body.Position.y),"Invalid prop body");props++;}
                Check(props==2,"Dense fixture lost a prop");
            });
            foreach(bool accepted in new[]{true,false}){
                var target=accepted?row:row.Reverse().ToArray();
                g.LoadLevel(Fixture(accepted?"receiver-success":"receiver-reject",row,target,new[]{new FloorDefinition(8,open)}));g.ShowGame();
                yield return Until(()=>g.Interaction.CanPlay,"Delivery fixture blocked");
                Check(g.Interaction.Execute(PuzzleCommand.Drop).Accepted,"Delivery rejected");
                bool captured=false;
                yield return Until(()=>g.GetComponent<PuzzleEventPlayer>().CurrentEvent==PuzzleEventKind.ReceiverEvaluated,"No receiver event");
                while(g.Interaction.IsPresenting){
                    var magic=g.GetComponentInChildren<ReceiverAttraction>();
                    if(!captured&&magic!=null&&magic.Strength>=.55f){
                        captured=true;CaptureCurrent(accepted?"magic-accept.png":"magic-reject.png");
                    }
                    yield return null;
                }
                Check(captured,"Missing receiver attraction/rejection visual");
                Check(g.CurrentScreen==(accepted?GameScreen.Win:GameScreen.Fail),"Incorrect delivery screen");
                yield return PlayableCapture.CaptureSettled(Phase,accepted?"delivered.png":"try-again.png",390,844);
            }
            Check(g.Catalog.TryGetLocalizedBinding("C03","en",out var words),"Word binding missing");
            g.LoadLevel(words.Definition,"en");g.ShowGame();yield return Until(()=>g.Interaction.CanPlay,"Word fixture blocked");
            hash=g.Interaction.Attempt.CurrentStateHash;g.Machine.CueWind(false);yield return new WaitForSecondsRealtime(.4f);
            for(int i=0;i<100;i++)g.Machine.Sync();
            var faces=g.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Where(t=>t.name=="Moving receiver glyph").ToArray();Check(faces.Length==g.Interaction.Attempt.Level.TargetSymbolIds.Count,"Word receiver lost glyph faces");
            Check(faces.All(t=>Mathf.Abs(t.rectTransform.rotation.eulerAngles.z-g.Machine.ReceiverAngle)<.01f||Mathf.Abs(t.rectTransform.rotation.eulerAngles.z-g.Machine.ReceiverAngle-360)<.01f),"Glyph did not follow receiver tilt");
            Check(hash==g.Interaction.Attempt.CurrentStateHash,"Word receiver motion changed state");
            yield return PlayableCapture.CaptureSettled(Phase,"moving-word.png",390,844);
            Check(SessionState.GetInt(Prefix+"Errors",0)==0,"Unity logged errors during receiver review");
            File.WriteAllText(Path.Combine(Output,"native-review.txt"),"PASS: receiver wind/rope movement, suspended freeze and unchanged hash; no phone-shake launch; actual lower-board descent and Undo; two automatic FarSwap lessons before further input; dense hanging saw+bomb; both committed receiver outcomes and magical delivery; word faces follow without drift. Native renders inspected separately. No phone build/install.");
            Debug.Log("CASCADE receiver review passed.");SessionState.SetBool(Prefix+"Finishing",true);EditorApplication.update+=Finish;EditorApplication.isPlaying=false;
            } finally {if(reviewCamera!=null)reviewCamera.targetTexture=oldTarget;reviewRender.Release();UnityEngine.Object.Destroy(reviewRender);}
        }
    }
}
