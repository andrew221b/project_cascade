using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Presentation;
using Cascade.Domain;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public static class OutcomeMotionCapture
    {
        const string Phase="OutcomeMotion";
        static string Folder=>Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase);
        static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Run()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required.");
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Shots());
        }
        public static void WoodFix()
        {UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(WoodShots());}
        static IEnumerator WoodShots()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();typeof(CascadeGame).GetField("persistenceReady",Flags).SetValue(g,false);
            g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;g.ReducedMotion=false;g.SetUiLocale("en");
            var camera=Camera.main;var scaler=g.GetComponentInChildren<CanvasScaler>();var oldTarget=camera.targetTexture;
            var render=new RenderTexture(390,844,24);bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            scaler.enabled=false;camera.targetTexture=render;scaler.enabled=true;
            try {
                g.SendMessage("OnApplicationFocus",true);g.LoadNode("C04");g.ShowGame();yield return Ready(g);g.RefreshLayout();yield return new WaitForSecondsRealtime(.6f);Snapshot(g,"wood-fresh");
                g.Interaction.Execute(PuzzleCommand.Swap(0));yield return Ready(g);yield return new WaitForSecondsRealtime(.4f);Snapshot(g,"wood-worn");
                g.Interaction.Execute(PuzzleCommand.Swap(0));double until=Time.realtimeSinceStartupAsDouble+20;
                while((g.EventPlayer.CurrentEvent!=PuzzleEventKind.FloorExited||g.Machine.FloorBreakProgress(0)<.72f)&&g.Interaction.IsPresenting&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                Snapshot(g,"wood-break");yield return Ready(g);g.LoadNode("C13");g.ShowGame();yield return Ready(g);
                yield return PlayableCapture.CaptureSettled(Phase,"connected-tool.png",390,844);
                Debug.Log("CASCADE supported wood and clear caption confirmation complete.");
            }finally{scaler.enabled=false;camera.targetTexture=oldTarget;scaler.enabled=true;render.Release();UnityEngine.Object.Destroy(render);UnityEngine.Application.runInBackground=background;g.RefreshLayout();}
        }
        static void Snapshot(CascadeGame g,string name)
        {
            var camera=Camera.main;var render=camera.targetTexture;string hash=g.Interaction.Attempt.CurrentStateHash;
            var old=RenderTexture.active;var pixels=new Texture2D(render.width,render.height,TextureFormat.RGB24,false);
            try {
                camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,render.width,render.height),0,0);pixels.Apply();
                Directory.CreateDirectory(Folder);File.WriteAllBytes(Path.Combine(Folder,name+".png"),pixels.EncodeToPNG());
                File.AppendAllText(Path.Combine(Folder,"motion.csv"),string.Join(",",name,DateTime.UtcNow.ToString("o"),g.CurrentScreen,g.EventPlayer.CurrentEvent,g.Outcome.Progress.ToString("F3",System.Globalization.CultureInfo.InvariantCulture),g.Machine.LoadedSag.ToString("F4",System.Globalization.CultureInfo.InvariantCulture),hash)+"\n");
                if(hash!=g.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Rendering changed state.");
            }finally{RenderTexture.active=old;UnityEngine.Object.Destroy(pixels);}
        }
        static IEnumerator Ready(CascadeGame g)
        {
            double start=Time.realtimeSinceStartupAsDouble;
            while((!g.PreparationReady||g.IsPlanningDescent||g.IsDescending||g.Interaction.IsPresenting)&&Time.realtimeSinceStartupAsDouble-start<30)yield return null;
            if(!g.PreparationReady||g.IsPlanningDescent||g.IsDescending||g.Interaction.IsPresenting)throw new InvalidOperationException("Readiness timed out.");
        }
        static IEnumerator EndShots(CascadeGame g,bool won)
        {
            g.LoadNode("C01");g.ShowGame();yield return Ready(g);yield return null;
            g.Interaction.Execute(won?PuzzleCommand.Swap(0):PuzzleCommand.Drop);
            foreach(float mark in new[]{.24f,.64f}){
                double start=Time.realtimeSinceStartupAsDouble;
                while((!g.Outcome.Running||g.Outcome.Progress<mark)&&g.Interaction.IsPresenting&&Time.realtimeSinceStartupAsDouble-start<20)yield return null;
                if(!g.Outcome.Running)throw new InvalidOperationException("Missed outcome frame.");
                Snapshot(g,(won?"win-":"fail-")+Mathf.RoundToInt(mark*100));
            }
            yield return Ready(g);yield return null;Snapshot(g,won?"win-settled":"fail-settled");
        }
        static IEnumerator Shots()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();typeof(CascadeGame).GetField("persistenceReady",Flags).SetValue(g,false);
            g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;g.ReducedMotion=false;
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            var camera=Camera.main;var scaler=g.GetComponentInChildren<CanvasScaler>();var oldTarget=camera.targetTexture;
            var phone=new RenderTexture(390,844,24);scaler.enabled=false;camera.targetTexture=phone;scaler.enabled=true;
            try {
                g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.SetUiLocale("en");g.RefreshLayout();yield return null;g.RefreshLayout();yield return null;
                yield return EndShots(g,true);yield return EndShots(g,false);
                g.LoadNode("C04");g.ShowGame();yield return Ready(g);yield return new WaitForSecondsRealtime(.6f);Snapshot(g,"wood-fresh");
                g.Interaction.Execute(PuzzleCommand.Swap(0));yield return Ready(g);yield return new WaitForSecondsRealtime(.4f);Snapshot(g,"wood-worn");
                g.Interaction.Execute(PuzzleCommand.Swap(0));double until=Time.realtimeSinceStartupAsDouble+20;
                while((g.EventPlayer.CurrentEvent!=PuzzleEventKind.FloorExited||g.Machine.FloorBreakProgress(0)<.3f)&&g.Interaction.IsPresenting&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                Snapshot(g,"wood-break");yield return Ready(g);
                g.ShowTravelBag();yield return PlayableCapture.CaptureSettled(Phase,"workshop-menu.png",390,844);
                g.ShowWorkshop();yield return PlayableCapture.CaptureSettled(Phase,"workshop-model.png",390,844);
                g.SetUiLocale("ru");g.ShowCollection();yield return PlayableCapture.CaptureSettled(Phase,"keepsakes-small-ru.png",320,693);
                g.ShowHowToPlay();yield return PlayableCapture.CaptureSettled(Phase,"guide-small-ru.png",320,693);
                g.SetUiLocale("en");g.ShowTravelBag();yield return PlayableCapture.CaptureSettled(Phase,"workshop-wide.png",1440,900);
                if(g.Catalog.TryGetLocalizedBinding("C13","ru",out var words))g.LoadLevel(words.Definition,"ru");g.SetUiLocale("ru");g.ShowGame();yield return Ready(g);
                yield return PlayableCapture.CaptureSettled(Phase,"bolted-word-tag.png",320,693);
                g.SetUiLocale("en");g.StartDescentLevel(g.Catalog.GetPrimary("C81").Definition);g.ShowGame();yield return Ready(g);
                yield return PlayableCapture.CaptureSettled(Phase,"first-stage-tool.png",390,844);
                g.ReducedMotion=true;
                foreach(string c in g.Descent.Stages[0].Witness){g.Interaction.Execute(c=="D"?PuzzleCommand.Drop:PuzzleCommand.Swap(int.Parse(c)));yield return Ready(g);}
                g.GoLower();yield return Ready(g);g.ReducedMotion=false;
                yield return PlayableCapture.CaptureSettled(Phase,"second-stage-tools.png",390,844);
                Debug.Log("CASCADE outcome/workshop native motion and menu batch complete.");
            }finally{scaler.enabled=false;camera.targetTexture=oldTarget;scaler.enabled=true;phone.Release();UnityEngine.Object.Destroy(phone);UnityEngine.Application.runInBackground=background;g.RefreshLayout();}
        }
    }
}
