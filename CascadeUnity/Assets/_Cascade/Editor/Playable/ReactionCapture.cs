using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Domain;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class ReactionCapture
    {
        const string Phase="Reactions";
        static readonly List<string> evidence=new List<string>();
        public static void Run()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required.");
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Shots());
        }
        public static void RunSequence() => UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Sequence());
        static IEnumerator Sequence()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            typeof(CascadeGame).GetField("persistenceReady",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,false);
            g.SendMessage("OnApplicationFocus",true);UnityEngine.Application.runInBackground=true;
            g.SetUiLocale("ru");g.ReducedMotion=false;g.Preferences.EffectQuality=2;g.Feedback.Quality=2;
            foreach(var entry in new[]{("repair",PuzzleEventKind.Repaired),("key",PuzzleEventKind.KeyOpened),("stamp",PuzzleEventKind.StampCollected),("ribbon",PuzzleEventKind.RibbonCollected),("time",PuzzleEventKind.HourglassCollected),("safe",PuzzleEventKind.BombDefused),("blast",PuzzleEventKind.BombExploded),("words",PuzzleEventKind.Repaired)}){
                string id=entry.Item1=="repair"||entry.Item1=="words"?"C18":entry.Item1=="key"?"C17":entry.Item1=="stamp"?"C35":"C13";
                if(entry.Item1=="ribbon"||entry.Item1=="time")g.LoadLevel(g.Catalog.BonusLevels[entry.Item1=="ribbon"?6:9].Definition);
                else if(entry.Item1=="words"){g.Catalog.TryGetLocalizedBinding(id,"ru",out var words);g.LoadLevel(words.Definition,"ru");}else g.LoadNode(id);
                g.ShowGame();yield return Ready(g);
                if(entry.Item1=="repair")yield return PlayableCapture.CaptureSettled("ReactionSequence","single-rope.png",390,844);
                var route=Route(g.Interaction.Attempt.Resolver,entry.Item2);
                for(int i=0;i<route.Count;i++){
                    g.Interaction.SetSuspended(false);g.Interaction.Execute(route[i]);
                    if(i==route.Count-1){
                        while((!g.ReactionBanner.IsShowing||g.EventPlayer.CurrentEvent!=entry.Item2)&&g.Interaction.IsPresenting)yield return null;
                        foreach(var phase in new[]{("pulse",.55f),("status",1.12f)}){
                            while(g.ReactionBanner.IsShowing&&g.ReactionBanner.Elapsed<phase.Item2)yield return null;
                            if(!g.ReactionBanner.IsShowing)throw new InvalidOperationException("Missed reaction phase");
                            g.Interaction.SetPaused(true);float age=g.ReactionBanner.Elapsed;
                            yield return PlayableCapture.CaptureSettled("ReactionSequence",entry.Item1+"-"+phase.Item1+".png",390,844);
                            if(age!=g.ReactionBanner.Elapsed)throw new InvalidOperationException("Paused phase changed");
                            g.Interaction.SetPaused(false);
                        }
                    }
                    while(g.Interaction.IsPresenting)yield return null;
                }
            }
            g.LoadNode("C18");g.ShowGame();yield return Ready(g);
            g.StartCoroutine(SequenceMotion(g));
        }
        public static void ConfirmSequence() => UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Confirm());
        static IEnumerator Confirm(){var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();typeof(CascadeGame).GetField("persistenceReady",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,false);g.SetUiLocale("ru");g.ReducedMotion=false;g.LoadNode("C13");g.ShowGame();yield return Ready(g);g.Interaction.SetSuspended(false);g.Interaction.Execute(PuzzleCommand.Swap(1));while(!g.ReactionBanner.IsShowing)yield return null;while(g.ReactionBanner.Elapsed<.55f)yield return null;g.Interaction.SetPaused(true);yield return PlayableCapture.CaptureSettled("ReactionSequence","blast-pulse.png",390,844);g.Interaction.SetPaused(false);while(g.Interaction.IsPresenting)yield return null;g.LoadNode("C18");g.ShowGame();yield return Ready(g);yield return SequenceMotion(g);}
        static IEnumerator SequenceMotion(CascadeGame g)
        {
            var camera=Camera.main;var old=camera.targetTexture;var target=new RenderTexture(390,844,24);camera.targetTexture=target;
            var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);Directory.CreateDirectory("/tmp/cascade-sequence-motion");
            try{
                g.RefreshLayout();yield return null;yield return null;g.Interaction.SetSuspended(false);g.Interaction.Execute(PuzzleCommand.Swap(0));
                while(!g.ReactionBanner.IsShowing)yield return null;string hash=g.Interaction.Attempt.CurrentStateHash;
                for(int f=0;f<14;f++){
                    camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();RenderTexture.active=previous;
                    File.WriteAllBytes("/tmp/cascade-sequence-motion/"+f.ToString("D2")+".png",pixels.EncodeToPNG());
                    if(hash!=g.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Motion changed state");
                    yield return new WaitForSecondsRealtime(.08f);
                }
            }finally{camera.targetTexture=old;UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);g.RefreshLayout();}
            Debug.Log("CASCADE clean tool sequence complete");
        }
        public static void RunOriginal() => UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Original());
        static IEnumerator Original()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();typeof(CascadeGame).GetField("persistenceReady",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,false);
            g.SendMessage("OnApplicationFocus",true);UnityEngine.Application.runInBackground=true;g.SetUiLocale("ru");g.ReducedMotion=false;
            foreach(string id in new[]{"C18","C13","words"}){
                if(id=="words"){g.Catalog.TryGetLocalizedBinding("C18","ru",out var words);g.LoadLevel(words.Definition,"ru");}else g.LoadNode(id);
                g.ShowGame();yield return Ready(g);var original=g.GetComponentsInChildren<UnityEngine.UI.Image>().First(i=>i.name=="Recognizable device");int identity=original.GetInstanceID();var home=original.transform.parent;
                g.Interaction.SetSuspended(false);g.Interaction.Execute(PuzzleCommand.Swap(0));while(!g.ReactionBanner.IsShowing)yield return null;
                if(g.ReactionBanner.Source.GetInstanceID()!=original.rectTransform.GetInstanceID())throw new InvalidOperationException("Source was cloned");
                while(g.ReactionBanner.Elapsed<.55f)yield return null;g.Interaction.SetPaused(true);
                yield return PlayableCapture.CaptureSettled("OriginalDeparture",id+"-flight.png",390,844);
                if(home.gameObject.activeInHierarchy)throw new InvalidOperationException("Original dock stayed visible");g.Interaction.SetPaused(false);while(g.Interaction.IsPresenting)yield return null;
                if(original.GetInstanceID()!=identity||original.gameObject.activeInHierarchy)throw new InvalidOperationException("Original respawned or duplicated");
                yield return PlayableCapture.CaptureSettled("OriginalDeparture",id+"-used.png",390,844);
                g.Undo();yield return null;if(!original.gameObject.activeInHierarchy||original.transform.parent!=home)throw new InvalidOperationException("Undo failed to restore original");
            }
            g.LoadNode("C18");g.ShowGame();yield return Ready(g);yield return SequenceMotion(g);
            var camera=Camera.main;var old=camera.targetTexture;var target=new RenderTexture(390,844,24);camera.targetTexture=target;var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);Directory.CreateDirectory("/tmp/cascade-wind-suspension");
            try{
                g.LoadNode("C18");yield return Ready(g);g.ShowGame();g.RefreshLayout();yield return null;yield return null;string hash=g.Interaction.Attempt.CurrentStateHash;
                for(int f=0;f<36;f++){
                    if(f==15){g.Machine.CueWind(true);g.CueDeviceWind(true);}
                    camera.Render();var active=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();RenderTexture.active=active;
                    File.WriteAllBytes("/tmp/cascade-wind-suspension/"+f.ToString("D2")+".png",pixels.EncodeToPNG());
                    if(hash!=g.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Decorative wind changed state");yield return new WaitForSecondsRealtime(.10f);
                }
            }finally{camera.targetTexture=old;UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);g.RefreshLayout();}
            Debug.Log("CASCADE original departure and wind witnesses complete");
        }
        public static void RunStatusCleanup() => UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(StatusCleanup());
        static IEnumerator StatusCleanup()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();typeof(CascadeGame).GetField("persistenceReady",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,false);
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);UnityEngine.Application.runInBackground=true;g.SetUiLocale("ru");g.ReducedMotion=false;
            foreach(var entry in new[]{("repair",PuzzleEventKind.Repaired),("ribbon",PuzzleEventKind.RibbonCollected),("bomb",PuzzleEventKind.BombDefused)}) {
                if(entry.Item1=="ribbon")g.LoadLevel(g.Catalog.BonusLevels[6].Definition);else g.LoadNode(entry.Item1=="repair"?"C18":"C13");
                g.ShowGame();yield return Ready(g);g.Interaction.SetSuspended(false);
                var route=Route(g.Interaction.Attempt.Resolver,entry.Item2);
                for(int i=0;i<route.Count-1;i++){g.Interaction.Execute(route[i]);while(g.Interaction.IsPresenting)yield return null;}
                var command=route[route.Count-1];string hash=g.Interaction.Attempt.CurrentStateHash;
                if(command.Kind==CommandKind.Swap)g.PreviewDrag(command.Edge,command.Edge+1);else if(command.Kind==CommandKind.LongReach)g.PreviewDrag(command.Edge,command.End);
                if(hash!=g.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Preview changed state");
                if(g.GetComponentsInChildren<RectTransform>(true).Any(r=>r.name=="Contact outcome ribbon"||r.name=="Device state"))throw new InvalidOperationException("Local outcome still exists");
                yield return PlayableCapture.CaptureSettled("StatusCleanup",entry.Item1+"-preview.png",390,844,owner=>{
                    // Layout refresh clears forecasts; apply the preview to the actual captured frame.
                    if(command.Kind==CommandKind.Swap||command.Kind==CommandKind.LongReach){owner.PreviewDrag(command.Edge,command.Edge);owner.PreviewDrag(command.Edge,command.Kind==CommandKind.Swap?command.Edge+1:command.End);}
                });
                g.Interaction.SetSuspended(false);g.Interaction.Execute(command);
                while((!g.ReactionBanner.IsShowing||g.EventPlayer.CurrentEvent!=entry.Item2)&&g.Interaction.IsPresenting)yield return null;
                while(g.ReactionBanner.IsShowing&&g.ReactionBanner.Elapsed<1.1f)yield return null;
                if(!g.ReactionBanner.IsShowing||!g.ReactionBanner.StatusVisible)throw new InvalidOperationException("Central outcome did not appear");
                g.Interaction.SetPaused(true);yield return PlayableCapture.CaptureSettled("StatusCleanup",entry.Item1+"-central.png",390,844);g.Interaction.SetPaused(false);
                while(g.Interaction.IsPresenting)yield return null;
            }
            Debug.Log("CASCADE local statuses removed; preview and central reactions checked");
        }
        public static void RunTouchFeel() => UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(TouchFeel());
        static IEnumerator TouchFeel()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            typeof(CascadeGame).GetField("persistenceReady",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,false);
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);UnityEngine.Application.runInBackground=true;
            g.SetUiLocale("ru");g.ReducedMotion=false;
            var camera=Camera.main;var old=camera.targetTexture;var target=new RenderTexture(390,844,24);camera.targetTexture=target;
            var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            var oldBackground=UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior;
            var oldEditor=UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode;
            UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
            UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var touch=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Touchscreen>();
            Vector2 Point(int slot){var r=(RectTransform)g.SlotViews[slot].transform;return RectTransformUtility.WorldToScreenPoint(camera,r.TransformPoint(r.rect.center));}
            void Touch(Vector2 point,UnityEngine.InputSystem.TouchPhase phase)=>UnityEngine.InputSystem.InputSystem.QueueStateEvent(touch,new UnityEngine.InputSystem.LowLevel.TouchState{touchId=93,position=point,phase=phase});
            void Shot(string file){camera.Render();var active=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();RenderTexture.active=active;File.WriteAllBytes(file,pixels.EncodeToPNG());}
            string report=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","TouchFeel");Directory.CreateDirectory(report);
            var log=new List<string>();
            try{
                foreach(string mode in new[]{"shapes","letters"}) {
                    if(mode=="letters"){g.Catalog.TryGetLocalizedBinding("C18","ru",out var words);g.LoadLevel(words.Definition,"ru");}else g.LoadNode("C18");
                    g.ShowGame();yield return Ready(g);g.RefreshLayout();yield return null;yield return null;g.Interaction.SetSuspended(false);
                    string hash=g.Interaction.Attempt.CurrentStateHash;string path="/tmp/cascade-touch-feel-"+mode;Directory.CreateDirectory(path);
                    var start=Point(0);var end=Point(1);bool released=false;
                    for(int f=0;f<32;f++) {
                        if(f==2)Touch(start,UnityEngine.InputSystem.TouchPhase.Began);
                        if(f>=6&&f<=12)Touch(Vector2.Lerp(start,end,(f-5)/7f),UnityEngine.InputSystem.TouchPhase.Moved);
                        if(f==15){Touch(end,mode=="letters"?UnityEngine.InputSystem.TouchPhase.Canceled:UnityEngine.InputSystem.TouchPhase.Ended);released=true;}
                        yield return null;yield return null;
                        Shot(Path.Combine(path,f.ToString("D2")+".png"));
                        if(!released||mode=="letters")if(hash!=g.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Free gesture changed state");
                        if(f==13)Shot(Path.Combine(report,mode+"-held.png"));
                        yield return new WaitForSecondsRealtime(.025f);
                    }
                    while(g.Interaction.IsPresenting)yield return null;
                    log.Add(mode+": native InputSystem gesture, route="+g.Interaction.Attempt.RouteLength+", holding="+g.IsHoldingToken);
                    if(g.Interaction.Attempt.RouteLength!=(mode=="letters"?0:1))throw new InvalidOperationException("Gesture committed wrong move count");
                }
                g.ReducedMotion=true;yield return PlayableCapture.CaptureSettled("TouchFeel","quiet-narrow.png",320,693);
                File.WriteAllLines(Path.Combine(report,"native-check.txt"),log);
            }finally{UnityEngine.InputSystem.InputSystem.RemoveDevice(touch);UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=oldBackground;UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=oldEditor;camera.targetTexture=old;UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);g.EndTokenHold();g.RefreshLayout();}
            Debug.Log("CASCADE touch feel witnesses complete");
        }
        public static void RunPhysicalPresentation(bool lightingOnly=false)=>UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(PhysicalPresentation(lightingOnly));
        static IEnumerator PhysicalPresentation(bool lightingOnly)
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();typeof(CascadeGame).GetField("persistenceReady",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,false);
            g.SendMessage("OnApplicationFocus",true);UnityEngine.Application.runInBackground=true;g.SetUiLocale("ru");g.ReducedMotion=false;
            var camera=Camera.main;var old=camera.targetTexture;var target=new RenderTexture(390,844,24);camera.targetTexture=target;var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            Directory.CreateDirectory("/tmp/cascade-physical-presentation");string report=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","PhysicalPresentation");Directory.CreateDirectory(report);
            void Shot(string file){camera.Render();var active=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();RenderTexture.active=active;File.WriteAllBytes(file,pixels.EncodeToPNG());}
            try{
                foreach(string id in lightingOnly?new string[0]:new[]{"C30","C03"}){
                    g.LoadNode(id);g.ShowGame();yield return Ready(g);g.RefreshLayout();yield return null;yield return null;
                    g.Interaction.SetSuspended(false);g.Interaction.Execute(PuzzleCommand.Swap(0));string hash=g.Interaction.Attempt.CurrentStateHash;
                    for(int f=0;f<(id=="C30"?32:52);f++){Shot("/tmp/cascade-physical-presentation/"+id+"-"+f.ToString("D2")+".png");if(hash!=g.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Physical presentation changed state");yield return new WaitForSecondsRealtime(.06f);}
                    while(g.Interaction.IsPresenting)yield return null;
                }
                g.LoadNode("C30");g.ShowGame();yield return Ready(g);yield return new WaitForSecondsRealtime(.5f);g.Interaction.SetPaused(true);
                var key=g.Machine.GetComponentsInChildren<Light>().First(l=>l.name=="Warm workshop key light");var rotation=key.transform.rotation;float strength=key.shadowStrength;
                key.shadowStrength=1;Shot(Path.Combine(report,"light-and-shadows.png"));var on=pixels.GetPixels32();
                key.shadowStrength=0;Shot(Path.Combine(report,"shadows-off-control.png"));var off=pixels.GetPixels32();int changed=0;for(int i=0;i<on.Length;i++)if(Mathf.Abs(on[i].r-off[i].r)+Mathf.Abs(on[i].g-off[i].g)+Mathf.Abs(on[i].b-off[i].b)>4)changed++;
                key.shadowStrength=strength;key.transform.rotation=Quaternion.Euler(32,28,0);Shot(Path.Combine(report,"light-from-other-side.png"));key.transform.rotation=rotation;int quality=QualitySettings.GetQualityLevel();int mobile=Array.IndexOf(QualitySettings.names,"Mobile");if(mobile>=0){QualitySettings.SetQualityLevel(mobile,true);Shot(Path.Combine(report,"mobile-quality-light.png"));QualitySettings.SetQualityLevel(quality,true);}g.Interaction.SetPaused(false);
                File.WriteAllText(Path.Combine(report,"lighting-check.txt"),"Native shadow-on/off changed pixels: "+changed+"\n");if(changed<8)throw new InvalidOperationException("No visible realtime shadow response");
            }finally{camera.targetTexture=old;UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);g.RefreshLayout();g.Interaction.SetPaused(false);}
            Debug.Log("CASCADE physical presentation witnesses complete");
        }
        public static void RunFlexibleRopes() => UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(FlexibleRopes());
        static IEnumerator FlexibleRopes()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();typeof(CascadeGame).GetField("persistenceReady",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,false);
            g.SendMessage("OnApplicationFocus",true);UnityEngine.Application.runInBackground=true;g.SetUiLocale("ru");g.ReducedMotion=false;
            var camera=Camera.main;var old=camera.targetTexture;var target=new RenderTexture(390,844,24);camera.targetTexture=target;var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);Directory.CreateDirectory("/tmp/cascade-flexible-rope");
            try{
                foreach(string id in new[]{"C18","C30"}){
                    g.LoadNode(id);g.ShowGame();yield return Ready(g);g.RefreshLayout();yield return null;yield return null;string hash=g.Interaction.Attempt.CurrentStateHash;
                    for(int f=0;f<42;f++){
                        if(f==18){g.Machine.CueWind(id=="C18");g.CueDeviceWind(id=="C18");}
                        camera.Render();var active=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();RenderTexture.active=active;
                        File.WriteAllBytes("/tmp/cascade-flexible-rope/"+id+"-"+f.ToString("D2")+".png",pixels.EncodeToPNG());
                        if(hash!=g.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Rope changed puzzle state");yield return new WaitForSecondsRealtime(.10f);
                    }
                }
            }finally{camera.targetTexture=old;UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);g.RefreshLayout();}
            g.LoadNode("C03");g.ShowGame();yield return Ready(g);g.Interaction.SetSuspended(false);g.Interaction.Execute(PuzzleCommand.Swap(0));while(g.Interaction.IsPresenting)yield return null;
            yield return PlayableCapture.CaptureSettled("FlexibleRopes","removed-floor.png",390,844);
            g.ReducedMotion=true;yield return PlayableCapture.CaptureSettled("FlexibleRopes","reduced-wide.png",1440,900);
            Debug.Log("CASCADE flexible rope witnesses complete");
        }
        public static void RunHeader()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            typeof(CascadeGame).GetField("persistenceReady",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,false);
            g.SetUiLocale("ru");g.LoadLevel(g.Catalog.BonusLevels[6].Definition);g.ShowGame();
            g.StartCoroutine(PlayableCapture.CaptureSettled(Phase,"header-inset.png",390,844));
        }
        public static void RunMotion() => UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Motion());
        static IEnumerator Motion()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            typeof(CascadeGame).GetField("persistenceReady",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,false);
            UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SetUiLocale("ru");g.ReducedMotion=false;
            var camera=Camera.main;var old=camera.targetTexture;var target=new RenderTexture(390,844,24);camera.targetTexture=target;
            var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);Directory.CreateDirectory("/tmp/cascade-reaction-motion");
            try{
                foreach(string id in new[]{"C18","C03","wind"}){
                    if(id=="wind")g.LoadDirectedLevel(g.Catalog.GetPrimary("C81").Definition,"ru",Cascade.Application.DeliveryMood.Wind);else g.LoadNode(id);g.ShowGame();yield return Ready(g);g.RefreshLayout();yield return null;yield return null;
                    var kind=id=="C18"?PuzzleEventKind.Repaired:id=="wind"?PuzzleEventKind.EntryPermuted:PuzzleEventKind.FloorExited;
                    var route=Route(g.Interaction.Attempt.Resolver,kind);
                    for(int c=0;c<route.Count;c++){
                        g.Interaction.Execute(route[c]);
                        if(c<route.Count-1){while(g.Interaction.IsPresenting)yield return null;continue;}
                        while((id=="wind"?!g.ReactionBanner.IsWindWarning:!g.ReactionBanner.IsShowing||g.EventPlayer.CurrentEvent!=kind)&&g.Interaction.IsPresenting)yield return null;
                        string hash=g.Interaction.Attempt.CurrentStateHash;
                        for(int f=0;f<24;f++){
                            camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();RenderTexture.active=previous;
                            File.WriteAllBytes("/tmp/cascade-reaction-motion/"+id+"-"+f.ToString("D2")+".png",pixels.EncodeToPNG());
                            if(hash!=g.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Motion changed puzzle state");
                            yield return new WaitForSecondsRealtime(.08f);
                        }
                        while(g.Interaction.IsPresenting)yield return null;
                    }
                }
            }finally{camera.targetTexture=old;UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);g.RefreshLayout();}
            Debug.Log("CASCADE native motion capture complete");
        }
        static List<PuzzleCommand> Route(PuzzleResolver resolver,PuzzleEventKind target)
        {
            var queue=new Queue<(PuzzleState state,List<PuzzleCommand> path)>();var seen=new HashSet<PuzzleState>();
            var start=resolver.CreateInitialState();queue.Enqueue((start,new List<PuzzleCommand>()));seen.Add(start);
            while(queue.Count>0&&seen.Count<10000){
                var item=queue.Dequeue();if(item.path.Count>=12)continue;
                var commands=Enumerable.Range(0,item.state.Row.Count-1).Select(PuzzleCommand.Swap).Append(PuzzleCommand.Drop);
                foreach(var command in commands){
                    var result=resolver.Resolve(item.state,command);if(!result.Accepted)continue;
                    var path=new List<PuzzleCommand>(item.path){command};
                    if(result.Events.Any(e=>e.Kind==target&&(target!=PuzzleEventKind.FloorExited||item.state.Durability<=result.DurabilityCost)))return path;
                    if(result.State.Status==PuzzleStatus.Playing&&seen.Add(result.State))queue.Enqueue((result.State,path));
                }
            }
            throw new InvalidOperationException("No canonical event witness: "+target);
        }
        static IEnumerator Ready(CascadeGame g)
        {
            double until=Time.realtimeSinceStartupAsDouble+30;while(!g.PreparationReady&&Time.realtimeSinceStartupAsDouble<until)yield return null;
            if(!g.PreparationReady)throw new InvalidOperationException("Preparation timeout");
        }
        static void CheckControls(CascadeGame g)
        {
            var safe=g.GetComponentsInChildren<RectTransform>().First(r=>r.name=="SafeArea");var c=new Vector3[4];safe.GetWorldCorners(c);var min=c[0];var max=c[2];
            foreach(var button in g.GetComponentsInChildren<UnityEngine.UI.Button>()){
                var rect=(RectTransform)button.transform;rect.GetWorldCorners(c);
                if(c[0].x<min.x-.001f||c[2].x>max.x+.001f||c[0].y<min.y-.001f||c[2].y>max.y+.001f)throw new InvalidOperationException("Button outside safe area: "+button.name);
                var t=button.GetComponentInChildren<TextMeshProUGUI>();if(t==null||string.IsNullOrEmpty(t.text))continue;t.ForceMeshUpdate();
                if(t.isTextOverflowing||t.textBounds.size.x>t.rectTransform.rect.width-t.margin.x-t.margin.z+1)throw new InvalidOperationException("Caption exceeds its padding: "+button.name+" / "+t.text);
            }
            evidence.Add("Safe-area/button padding pass: "+g.CurrentScreen.ToString());
        }
        static IEnumerator Shots()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            typeof(CascadeGame).GetField("persistenceReady",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,false);
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;evidence.Clear();
            try{
                g.SendMessage("OnApplicationFocus",true);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.Feedback.Quality=2;
                foreach(var entry in new[]{("C18",PuzzleEventKind.Repaired),("C17",PuzzleEventKind.KeyOpened),("C35",PuzzleEventKind.StampCollected),("ribbon",PuzzleEventKind.RibbonCollected),("time",PuzzleEventKind.HourglassCollected),("C13",PuzzleEventKind.BombDefused),("blast",PuzzleEventKind.BombExploded),("wood",PuzzleEventKind.FloorExited),("small",PuzzleEventKind.Repaired),("quiet",PuzzleEventKind.HourglassCollected)}){
                    bool small=entry.Item1=="small",quiet=entry.Item1=="quiet";
                    g.ReducedMotion=quiet;g.Preferences.LargeText=small;g.SetUiLocale(small?"uk":"ru");
                    if(entry.Item1=="ribbon"||entry.Item1=="time"||quiet)g.LoadLevel(g.Catalog.BonusLevels[entry.Item1=="ribbon"?6:9].Definition);
                    else g.LoadNode(entry.Item1=="blast"?"C13":entry.Item1=="wood"?"C03":small?"C18":entry.Item1);
                    g.ShowGame();yield return Ready(g);var route=Route(g.Interaction.Attempt.Resolver,entry.Item2);
                    for(int i=0;i<route.Count;i++){
                        g.Interaction.SetSuspended(false);var result=g.Interaction.Execute(route[i]);if(!result.Accepted)throw new InvalidOperationException("Witness rejected.");
                        bool last=i==route.Count-1;double until=Time.realtimeSinceStartupAsDouble+30;
                        if(last){
                            while((!g.ReactionBanner.IsShowing||g.EventPlayer.CurrentEvent!=entry.Item2)&&g.Interaction.IsPresenting&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                            if(!g.ReactionBanner.IsShowing)throw new InvalidOperationException("Missed announcement: "+entry.Item1);
                            while(g.ReactionBanner.IsShowing&&g.ReactionBanner.Elapsed<(quiet?.18f:entry.Item1=="wood"?.38f:.54f)&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                            g.Interaction.SetPaused(true);float age=g.ReactionBanner.Elapsed;
                            yield return PlayableCapture.CaptureSettled(Phase,entry.Item1+".png",small?320:390,small?693:844);
                            if(g.ReactionBanner.Elapsed!=age)throw new InvalidOperationException("Paused announcement advanced.");
                            var title=g.ReactionBanner.GetComponentInChildren<TextMeshProUGUI>();title.ForceMeshUpdate();
                            if(title.isTextOverflowing)throw new InvalidOperationException("Announcement text overflows: "+entry.Item1);
                            evidence.Add(entry.Item1+": "+entry.Item2+"; commands="+string.Join(",",route)+"; title="+g.ReactionBanner.Message+"; paused age="+age+"; state="+g.Interaction.Attempt.CurrentStateHash);
                            g.Interaction.SetPaused(false);
                        }
                        while(g.Interaction.IsPresenting&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                        if(g.Interaction.IsPresenting)throw new InvalidOperationException("Presentation timeout.");
                    }
                }
                foreach(string id in new[]{"C17","C18","C35"}){
                    g.SetUiLocale("ru");g.Catalog.TryGetLocalizedBinding(id,"ru",out var words);g.LoadLevel(words.Definition,"ru");g.ShowGame();yield return Ready(g);
                    yield return PlayableCapture.CaptureSettled(Phase,"body-words-"+id+".png",390,844);
                    if(g.GetComponentsInChildren<HangingToolTag>().Length!=0)throw new InvalidOperationException("A separate matching tag survived.");
                }
                foreach(string locale in new[]{"en","ru","uk"}){
                    g.Preferences.LargeText=true;g.SetUiLocale(locale);g.SetEditorSafeInsets(new Vector4(.035f,.05f,.065f,.08f));g.ShowSettings();
                    if(locale!="en"){typeof(CascadeGame).GetField("settingsPage",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,2);typeof(CascadeGame).GetMethod("DrawMenu",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,null);}
                    yield return PlayableCapture.CaptureSettled(Phase,"settings-safe-"+locale+".png",320,693,CheckControls);
                }
                g.SetEditorSafeInsets(Vector4.zero);g.Preferences.LargeText=false;g.SetUiLocale("ru");g.LoadNode("C35");g.ShowGame();yield return Ready(g);
                for(int edge=0;edge<3;edge++){g.Interaction.Execute(PuzzleCommand.Swap(edge));while(g.Interaction.IsPresenting)yield return null;if(edge>0)yield return PlayableCapture.CaptureSettled(Phase,"wood-wear-"+edge+".png",390,844);}
                File.WriteAllLines(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase+"/witnesses.txt")),evidence);
                g.Preferences.LargeText=false;g.ReducedMotion=false;g.LoadNode("C18");g.ShowGame();Debug.Log("CASCADE reaction witnesses complete: "+evidence.Count);
            }finally{g.Interaction.SetPaused(false);UnityEngine.Application.runInBackground=background;}
        }
    }
}
