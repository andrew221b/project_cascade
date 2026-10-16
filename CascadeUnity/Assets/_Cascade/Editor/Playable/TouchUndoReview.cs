using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Domain;
using Cascade.Presentation;
using Cascade.Solver;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class TouchUndoReview
    {
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.StartCoroutine(Review(game));
        }
        static IEnumerator Ready(Func<bool> ready)
        {
            float deadline=Time.realtimeSinceStartup+30;
            while(!ready()&&Time.realtimeSinceStartup<deadline)yield return null;
            if(!ready())throw new InvalidOperationException("Touch/Undo native review timed out.");
        }
        static IEnumerator Review(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;game.Preferences.Tutorials=false;
            game.Preferences.ReducedMotion=false;game.ReducedMotion=false;game.SetUiLocale("ru");
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();var previous=camera.targetTexture;var active=RenderTexture.active;
            var render=new RenderTexture(390,844,24);var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/TouchUndo-Oct08"));Directory.CreateDirectory(folder);
            try{
                scaler.enabled=false;camera.targetTexture=render;scaler.enabled=true;yield return null;
                var row="ABAC".Select(c=>SymbolId.Parse("s:"+c)).ToArray();
                var fixture=new LevelDefinition("undo-visual","undo-visual","1","2.0","undo-visual",row,row,
                    new[]{new TargetSegment(SegmentKind.Shapes,0,4)},new[]{new FloorDefinition(8,new[]{true,true,true})});
                game.LoadLevel(fixture);game.ShowGame();yield return Ready(()=>game.Interaction.CanPlay);game.RefreshLayout();yield return null;
                game.Interaction.Execute(PuzzleCommand.Swap(0));yield return Ready(()=>!game.Interaction.IsPresenting);
                Shot("undo-before.png");string swapped=game.Interaction.Attempt.CurrentStateHash;
                game.Undo();yield return new WaitForSecondsRealtime(.13f);Shot("undo-moving.png");
                if(!game.Interaction.IsPresenting||game.Interaction.Attempt.CurrentStateHash==swapped)throw new InvalidOperationException("Undo must have committed and still be visibly moving.");
                yield return Ready(()=>!game.Interaction.IsPresenting);Shot("undo-restored.png");
                game.LoadNode("C01");game.ShowGame();yield return Ready(()=>game.Interaction.CanPlay);
                var proof=PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver);
                foreach(var command in proof.Path){game.Interaction.Execute(command);yield return Ready(()=>!game.Interaction.IsPresenting);}
                yield return Ready(()=>game.CurrentScreen==GameScreen.Win);
                yield return Ready(()=>game.GetComponentInChildren<RewardCelebration>()?.Progress>=1);Shot("result-no-diamond.png");
                if(game.GetComponentsInChildren<Transform>().Any(t=>t.name=="Score glint"))throw new InvalidOperationException("Score overlay must be gone.");
                File.WriteAllText(Path.Combine(folder,"native-review.txt"),"PASS: four native Unity renders; moving Undo with duplicate symbols, restored row, completion without score diamond. These do not measure physical-device touch latency or FPS.\n");
                Debug.Log("CASCADE touch/Undo native review complete.");
            }finally{
                scaler.enabled=false;camera.targetTexture=previous;scaler.enabled=true;RenderTexture.active=active;
                UnityEngine.Object.Destroy(pixels);render.Release();UnityEngine.Object.Destroy(render);game.RefreshLayout();
            }
            void Shot(string filename){camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,filename),pixels.EncodeToPNG());}
        }
    }
}
