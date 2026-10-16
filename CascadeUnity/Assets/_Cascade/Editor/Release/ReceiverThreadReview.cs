using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using Cascade.Editor.Playable;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Release
{
    // Isolated native-camera art review; never builds a player or changes the device profile.
    [InitializeOnLoad]
    public static class ReceiverThreadReview
    {
        const string Prefix="Cascade.ReceiverThreadReview.",Phase="ReceiverThread-Oct10";
        const System.Reflection.BindingFlags Private=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        static string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase));
        static ReceiverThreadReview(){if(SessionState.GetBool(Prefix+"Finishing",false))EditorApplication.update+=Finish;else if(SessionState.GetBool(Prefix+"Auto",false)){SessionState.SetBool(Prefix+"Started",false);EditorApplication.update+=Tick;UnityEngine.Application.logMessageReceived+=RecordError;}}
        public static void BeginBatch()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
            var shader=Resources.Load<Shader>("CascadeArt/ReceiverMagic");
            Check(shader!=null&&!ShaderUtil.ShaderHasError(shader),"Receiver thread shader failed");
            NavigationCapture.Prepare();Directory.CreateDirectory(Output);
            SessionState.SetBool(Prefix+"Auto",true);SessionState.SetBool(Prefix+"Started",false);SessionState.SetInt(Prefix+"Errors",0);
            SessionState.SetFloat(Prefix+"Began",(float)EditorApplication.timeSinceStartup);
            UnityEngine.Application.logMessageReceived+=RecordError;EditorApplication.update+=Tick;UnityEngine.Application.runInBackground=true;EditorApplication.isPlaying=true;
        }
        static void RecordError(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)SessionState.SetInt(Prefix+"Errors",SessionState.GetInt(Prefix+"Errors",0)+1);}
        static void Tick()
        {
            if(EditorApplication.timeSinceStartup-SessionState.GetFloat(Prefix+"Began",0)>160){Debug.LogError("Receiver thread review timed out");EditorApplication.Exit(2);return;}
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))return;
            EditorApplication.QueuePlayerLoopUpdate();if(SessionState.GetBool(Prefix+"Started",false))return;
            SessionState.SetBool(Prefix+"Started",true);g.StartCoroutine(Run(g));
        }
        static void Finish()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)return;
            EditorApplication.update-=Tick;EditorApplication.update-=Finish;UnityEngine.Application.logMessageReceived-=RecordError;
            SessionState.SetBool(Prefix+"Auto",false);SessionState.SetBool(Prefix+"Finishing",false);NavigationCapture.Clear();
            EditorApplication.Exit(SessionState.GetInt(Prefix+"Errors",0)==0?0:2);
        }
        static void Check(bool ok,string reason){if(!ok)throw new InvalidOperationException(reason);}
        static IEnumerator Ready(CascadeGame g){float deadline=Time.realtimeSinceStartup+18;while(!g.Interaction.CanPlay){Check(Time.realtimeSinceStartup<deadline,"Fixture blocked");yield return null;}}
        static LevelDefinition Fixture(SymbolId[] row,bool accepted)
        {
            bool word=row[0].Kind==SymbolKind.Glyph;
            return new LevelDefinition("thread-review","C02","2.0","2.0","review",row,accepted?row:row.Reverse(),
                new[]{new TargetSegment(word?SegmentKind.Word:SegmentKind.Shapes,0,row.Length,word?"thread-review-word":null)},
                new[]{new FloorDefinition(8,Enumerable.Repeat(true,row.Length-1))});
        }
        static IEnumerator Until(Func<bool> ready,string error)
        {float deadline=Time.realtimeSinceStartup+18;while(!ready()){Check(Time.realtimeSinceStartup<deadline,error);yield return null;}}
        static void Capture(string name)
        {
            var camera=Camera.main;var active=RenderTexture.active;var image=new Texture2D(390,844,TextureFormat.RGB24,false);
            try{camera.Render();RenderTexture.active=camera.targetTexture;image.ReadPixels(new Rect(0,0,390,844),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Output,name),image.EncodeToPNG());}
            finally{RenderTexture.active=active;UnityEngine.Object.Destroy(image);}
        }
        static IEnumerator Run(CascadeGame g)
        {
            var camera=Camera.main;var old=camera.targetTexture;var portrait=new RenderTexture(390,844,24);camera.targetTexture=portrait;
            try{
                yield return null;g.RefreshLayout();yield return null;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
                g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.Preferences.NotificationsEnabled=false;g.Preferences.Tutorials=false;g.ReducedMotion=false;
                var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(g);
                foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
                profile.SeenCoachPrompts.Add("controls-two-ways-v1");profile.SeenCoachPrompts.Add("controls-undo-v1");profile.SeenCoachPrompts.Add("closed-path-v2");
                typeof(CascadeGame).GetField("persistenceReady",Private).SetValue(g,false);
                foreach(bool words in new[]{false,true})foreach(bool accepted in new[]{true,false}){
                    var row=(words?new[]{"g:en:W","g:en:A","g:en:R","g:en:D"}:new[]{"s:C","s:B","s:A","s:D"}).Select(x=>SymbolId.Parse(x)).ToArray();
                    string name=(words?"words":"shapes")+(accepted?"-accept":"-reject");g.LoadLevel(Fixture(row,accepted));g.ShowGame();yield return Ready(g);
                    Check(g.Interaction.Execute(PuzzleCommand.Drop).Accepted,"Delivery rejected");
                    ReceiverAttraction magic=null;
                    yield return Until(()=>{magic=g.GetComponentInChildren<ReceiverAttraction>();return g.GetComponent<PuzzleEventPlayer>().CurrentEvent==PuzzleEventKind.ReceiverEvaluated&&magic!=null&&magic.Strength>=.55f;},"Missing delivery thread");
                    g.Interaction.SetSuspended(true);string hash=g.Interaction.Attempt.CurrentStateHash;float strength=magic.Strength;
                    g.Machine.PaintReceiverAttraction(strength,.17f,accepted);Capture(name+"-a.png");
                    var material=magic.GetComponent<MeshRenderer>().sharedMaterial;
                    float phase=material.GetFloat("_FlowPhase");yield return null;Check(material.GetFloat("_FlowPhase")==phase,"Paused thread colour did not freeze");
                    g.Machine.PaintReceiverAttraction(strength,.92f,accepted);Capture(name+"-b.png");
                    Check(material.GetFloat("_FlowPhase")!=phase,"Thread colour flow did not advance");Check(hash==g.Interaction.Attempt.CurrentStateHash,"Thread changed puzzle state");
                    g.Interaction.SetSuspended(false);yield return Until(()=>!g.Interaction.IsPresenting,"Delivery did not settle");
                    Check(g.CurrentScreen==(accepted?GameScreen.Win:GameScreen.Fail),"Incorrect delivery outcome");Check(!magic.GetComponent<MeshRenderer>().enabled,"Thread stayed visible after delivery");
                }
                Check(SessionState.GetInt(Prefix+"Errors",0)==0,"Unity recorded review errors");
                File.WriteAllText(Path.Combine(Output,"native-review.txt"),"PASS: real successful/rejected shape and word delivery, one rainbow thread renderer, two colour-flow frames, frozen flow while suspended, unchanged puzzle hash and cleanup. Native shader compiled. No iPhone build/install or physical-device validation.");
                Debug.Log("CASCADE receiver thread review passed.");SessionState.SetBool(Prefix+"Finishing",true);EditorApplication.update+=Finish;EditorApplication.isPlaying=false;
            }finally{camera.targetTexture=old;portrait.Release();UnityEngine.Object.Destroy(portrait);}
        }
    }
}
