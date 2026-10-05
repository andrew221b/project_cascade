using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using Cascade.Domain;
using Cascade.Solver;
using UnityEngine.UI;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    // Isolated native fixtures. These do not claim actual campaign completion.
    public static class WoodlandCapture
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Field(CascadeGame g,string n,object v)=>typeof(CascadeGame).GetField(n,Private).SetValue(g,v);
        static void Invoke(CascadeGame g,string n)=>typeof(CascadeGame).GetMethod(n,Private).Invoke(g,null);
        public static void Still()
        {
            if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Current Play mode required");
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();g.StartCoroutine(Capture(g));
        }
        public static void Confirm()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();g.StartCoroutine(Confirmation(g));
        }
        public static void ReviewerFix()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();g.StartCoroutine(FixCapture(g));
        }
        static IEnumerator FixCapture(CascadeGame g)
        {
            Field(g,"persistenceReady",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.ReducedMotion=true;
            foreach(string locale in new[]{"en","ru","uk"}){
                g.SetUiLocale(locale);g.ShowTravelBag();yield return PlayableCapture.CaptureSettled("Woodland","bag-small-"+locale+".png",320,693);
            }
            g.SetUiLocale("en");g.ShowTravelBag();yield return PlayableCapture.CaptureSettled("Woodland","bag-small.png",320,693);
            foreach(int page in new[]{0,3,6,8,13,18,23,28,32}){
                g.ShowJourney();Field(g,"mapPage",page);Field(g,"selectedJourneyStop",Cascade.Content.JourneyStory.FirstStop(page));Invoke(g,"DrawMenu");
                yield return PlayableCapture.CaptureSettled("Woodland","map-"+page+".png",390,844);
            }
            g.LoadNode("C13");g.ShowDevices();
            foreach(int index in Enumerable.Range(0,9)){
                Field(g,"deviceIndex",index);Invoke(g,"DrawMenu");yield return PlayableCapture.CaptureSettled("Woodland","guide-"+index+".png",320,693);
            }
            g.ReducedMotion=false;
            foreach(int index in new[]{7}){
                Field(g,"deviceIndex",index);Invoke(g,"DrawMenu");
                // Sample the independent visual demonstration at fixed clock phases without issuing a puzzle command.
                foreach(float phase in new[]{.6f,1.4f,1.9f,2.3f}){
                    typeof(ToyMachineView).GetField("windTime",Private).SetValue(g.Machine,phase);yield return null;
                    yield return PlayableCapture.CaptureSettled("Woodland","guide-"+index+"-phase-"+phase.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+".png",320,693);
                }
            }
            Debug.Log("CASCADE Woodland reviewer fix captures complete");
        }
        static IEnumerator Confirmation(CascadeGame g)
        {
            Field(g,"persistenceReady",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.SetUiLocale("ru");g.ReducedMotion=true;
            g.LoadNode("C37");if(g.Catalog.TryGetLocalizedBinding("C37","ru",out var words))g.LoadLevel(words.Definition,"ru");
            yield return PlayableCapture.CaptureSettled("Woodland","words-small-ru.png",320,693);
            g.SetUiLocale("en");g.LoadNode("C13");g.ShowDevices();
            for(int device=0;device<9;device++){
                Field(g,"deviceIndex",device);Invoke(g,"DrawMenu");
                yield return PlayableCapture.CaptureSettled("Woodland","guide-"+device+".png",320,693);
            }
            g.LoadNode("C160");yield return PlayableCapture.CaptureSettled("Woodland","beacon-wide.png",1440,900);
            g.ReducedMotion=false;
            var camera=Camera.main;var scaler=g.GetComponentInChildren<CanvasScaler>();var previous=camera.targetTexture;var active=RenderTexture.active;
            var render=new RenderTexture(390,844,24);var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/Woodland/motion"));Directory.CreateDirectory(folder);
            var report=new System.Collections.Generic.List<string>();
            try{
                scaler.enabled=false;camera.targetTexture=render;scaler.enabled=true;yield return null;g.RefreshLayout();yield return null;
                foreach(var id in new[]{"C03","C13","C25","C38"}){
                    g.LoadNode(id);g.SendMessage("OnApplicationFocus",true);yield return null;g.RefreshLayout();yield return null;
                    System.Collections.Generic.IEnumerable<PuzzleCommand> route=PuzzleSolver.FindShortest(g.Interaction.Attempt.Resolver).Path;int step=0;
                    if(id=="C13"){
                        var state=g.Interaction.Attempt.Current;var resolver=g.Interaction.Attempt.Resolver;
                        route=Enumerable.Range(0,state.Row.Count-1).Select(PuzzleCommand.Swap).Where(c=>resolver.Resolve(state,c).Events.Any(e=>e.Kind==PuzzleEventKind.BombExploded)).Take(1).ToArray();
                    }
                    foreach(var command in route){
                        var result=g.Interaction.Execute(command);if(result==null||!result.Accepted)throw new InvalidOperationException("Motion rejected");
                        string hash=g.Interaction.Attempt.CurrentStateHash;var seen=new HashSet<PuzzleEventKind>();float start=Time.realtimeSinceStartup;
                        do{
                            yield return null;if(hash!=g.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Animation changed state");
                            var stage=g.EventPlayer.CurrentEvent;
                            if(seen.Add(stage))Save(id+"-"+step+"-"+stage);
                            if(Time.realtimeSinceStartup-start>5)throw new InvalidOperationException("Unfinished motion");
                        }while(g.EventPlayer.IsRunning);
                        Save(id+"-"+step+"-settled");step++;
                    }
                }
                void Save(string name){
                    camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());
                    report.Add(name+" | "+g.Interaction.Attempt.CurrentStateHash+" | pose="+g.Machine.TokenPose(0)+" | sag="+g.Machine.LoadedSag);
                }
                File.WriteAllLines(Path.Combine(folder,"timeline.txt"),report);Debug.Log("CASCADE Woodland confirmation complete; motion frames="+report.Count);
            }finally{
                g.EventPlayer.Skip();scaler.enabled=false;camera.targetTexture=previous;scaler.enabled=true;RenderTexture.active=active;
                render.Release();UnityEngine.Object.DestroyImmediate(render);UnityEngine.Object.DestroyImmediate(pixels);UnityEngine.Application.runInBackground=background;g.LoadNode("C01");g.ShowJourney();
            }
        }
        static IEnumerator Capture(CascadeGame g)
        {
            Field(g,"persistenceReady",false);g.ReducedMotion=true;g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
            var findings=new List<string>();g.SetUiLocale("en");g.SendMessage("OnApplicationFocus",true);
            foreach(var id in new[]{"C01","C13","C35","C160"}){
                g.LoadNode(id);yield return null;yield return Shot(id+"-phone",390,844);
            }
            g.LoadNode("C160");yield return Shot("beacon-wide",1440,900);
            g.LoadNode("C37");g.SetUiLocale("ru");
            if(g.Catalog.TryGetLocalizedBinding("C37","ru",out var words))g.LoadLevel(words.Definition,"ru");
            yield return Shot("words-small-ru",320,693);g.SetUiLocale("en");
            foreach(int page in new[]{0,3,6,8,13,18,23,28,32}){
                g.ShowJourney();Field(g,"mapPage",page);Field(g,"selectedJourneyStop",Cascade.Content.JourneyStory.FirstStop(page));Invoke(g,"DrawMenu");
                yield return Shot("map-"+page,390,844);
            }
            g.ShowTravelBag();yield return Shot("bag-small",320,693);
            g.LoadNode("C13");g.ShowDevices();Field(g,"deviceIndex",2);Invoke(g,"DrawMenu");yield return Shot("bomb-guide",320,693);
            typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(g,new object[]{GameScreen.JourneyEnd});
            yield return Shot("ending",390,844);
            IEnumerator Shot(string name,int w,int h){
                yield return PlayableCapture.CaptureSettled("Woodland",name+".png",w,h,game=>{
                    foreach(var t in game.GetComponentsInChildren<TMP_Text>()){
                        t.ForceMeshUpdate();if(t.isTextOverflowing||t.text.Contains("No translation"))findings.Add(name+" / "+t.name+" / "+t.text);
                    }
                });
            }
            File.WriteAllLines(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/Woodland/still-fit.txt")),findings);
            Debug.Log("CASCADE Woodland still batch complete; findings="+findings.Count);
        }
    }
}
