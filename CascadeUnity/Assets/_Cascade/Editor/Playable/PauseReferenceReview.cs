using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Domain;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public sealed class PauseReferenceReview
    {
        const string Phase="PauseReferenceOct14",Scene="Assets/_Cascade/Scenes/CascadePlayable.unity";
        CascadeGame game;GameObject host;readonly List<string> checks=new List<string>();
        public static void StartReview(){if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play required");UnityEngine.Application.runInBackground=true;var h=new GameObject("Isolated pause reference review");UnityEngine.Object.DontDestroyOnLoad(h);h.AddComponent<ReferenceShopLayout>().StartCoroutine(new PauseReferenceReview{host=h}.Run());}
        Button Button(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        void Open(){game.ShowGame();game.SendMessage("TogglePause");}
        IEnumerator Run()
        {
            string directory=Path.Combine(Path.GetTempPath(),"CascadePauseReview",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync(Scene);game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;game.LoadNode("C02");game.ShowGame();
            double until=Time.realtimeSinceStartupAsDouble+15;while(!game.PreparationReady&&Time.realtimeSinceStartupAsDouble<until)yield return null;if(!game.PreparationReady)throw new InvalidOperationException("C02 preparation did not finish");
            game.SetUiLocale("ru");Open();yield return Shot("reference-composition",720,1280);
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);game.Preferences.LargeText=false;Open();yield return Shot(locale+"-regular",720,1280);
                game.Preferences.LargeText=true;Open();yield return Shot(locale+"-small",320,693);
                game.SetEditorSafeInsets(new Vector4(0,.035f,0,.055f));yield return Shot(locale+"-safe",390,844);game.SetEditorSafeInsets(Vector4.zero);
            }
            game.Preferences.LargeText=false;game.SetUiLocale("ru");Open();yield return Shot("tablet",768,1024);
            var view=game.GetComponentsInChildren<PauseReferenceAmbiance>().Single();var leaves=game.GetComponentsInChildren<MenuLeafDrift>().Single();
            string hash=game.Interaction.Attempt.CurrentStateHash;float phase=game.Machine.AmbientPhase,age=view.AnimationAge;
            yield return new WaitForSecondsRealtime(.5f);checks.Add("Pause art animates while puzzle and world freeze: "+(view.AnimationAge>age&&Mathf.Approximately(game.Machine.AmbientPhase,phase)&&game.Interaction.Attempt.CurrentStateHash==hash));
            game.SendMessage("OnApplicationPause",true);age=view.AnimationAge;yield return new WaitForSecondsRealtime(.3f);checks.Add("Background freezes art: "+Mathf.Approximately(age,view.AnimationAge));game.SendMessage("OnApplicationPause",false);
            yield return PauseAnimationCapture.Run(game);yield return Shot("animated-pose",720,1280);
            game.ReducedMotion=true;age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);checks.Add("Reduced motion freezes art and hides leaves: "+(Mathf.Approximately(age,view.AnimationAge)&&leaves.VisibleCount==0));yield return Shot("reduced-motion",390,844);game.ReducedMotion=false;
            Button("Settings").onClick.Invoke();yield return null;checks.Add("Gear opens Settings: "+(game.CurrentScreen==GameScreen.Settings));Button("Back").onClick.Invoke();yield return null;checks.Add("Settings back returns to paused same attempt: "+(game.CurrentScreen==GameScreen.Pause&&game.Interaction.IsPaused&&game.Interaction.Attempt.CurrentStateHash==hash));
            Button("Home").onClick.Invoke();yield return null;checks.Add("Home opens main menu and preserves attempt: "+(game.CurrentScreen==GameScreen.Home&&game.Interaction.Attempt.CurrentStateHash==hash));
            Open();Button("Resume").onClick.Invoke();yield return null;checks.Add("Continue resumes unchanged attempt: "+(game.CurrentScreen==GameScreen.Playing&&!game.Interaction.IsPaused&&game.Interaction.Attempt.CurrentStateHash==hash));
            if(game.Interaction.Execute(PuzzleCommand.Swap(0))?.Accepted!=true)throw new InvalidOperationException("Expected C02 move");var id=game.Interaction.Attempt.AttemptId;string pin=game.Interaction.Attempt.PinnedContentHash;Open();Button("Retry").onClick.Invoke();yield return null;
            checks.Add("Restart resets moves and keeps puzzle: "+(game.CurrentScreen==GameScreen.Playing&&game.Interaction.Attempt.AttemptId!=id&&game.Interaction.Attempt.PinnedContentHash==pin&&game.Interaction.Attempt.RouteLength==0));
            File.WriteAllLines(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase,"review.txt"),checks);
            CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync(Scene);UnityEngine.Object.Destroy(host);if(Directory.Exists(directory))Directory.Delete(directory,true);EditorApplication.isPlaying=false;
        }
        IEnumerator Shot(string name,int width,int height)
        {
            yield return new WaitForSecondsRealtime(.2f);yield return PlayableCapture.CaptureSettled(Phase,name+".png",width,height);
            foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){t.ForceMeshUpdate();if(t.isTextOverflowing||t.isTextTruncated)checks.Add("OVERFLOW "+name+": "+t.name);}
            var hits=new List<RaycastResult>();int total=0,ok=0;
            foreach(var b in game.GetComponentsInChildren<Button>().Where(b=>b.interactable)){total++;var r=(RectTransform)b.transform;EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(Camera.main,r.TransformPoint(r.rect.center))},hits);if(hits.FirstOrDefault().gameObject?.GetComponentInParent<Button>()==b)ok++;hits.Clear();}
            checks.Add("Buttons "+name+": "+ok+"/"+total);
            checks.Add("Bright proportional Mika "+name+": "+game.GetComponentsInChildren<Image>().Where(i=>i.name=="Pause resting Mika and bag").All(i=>i.preserveAspect&&i.color==Color.white));
        }
    }
    public static class PauseAnimationCapture
    {
        public static IEnumerator Run(CascadeGame game)
        {
            var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();var old=camera.targetTexture;var active=RenderTexture.active;int oldRate=UnityEngine.Application.targetFrameRate;float oldTime=Time.captureDeltaTime;
            var view=game.GetComponentsInChildren<PauseReferenceAmbiance>().Single();float start=view.AnimationAge;var leaves=game.GetComponentsInChildren<MenuLeafDrift>().Single();leaves.SetReviewMode(true);
            string folder=Path.Combine(Path.GetTempPath(),"cascade-pause-frames");Directory.CreateDirectory(folder);var target=new RenderTexture(360,640,24);var pixels=new Texture2D(360,640,TextureFormat.RGB24,false);string hash=game.Interaction.Attempt.CurrentStateHash;float phase=game.Machine.AmbientPhase;
            try{
                UnityEngine.Application.targetFrameRate=60;Time.captureDeltaTime=1f/60;scaler.enabled=false;camera.targetTexture=target;scaler.enabled=true;yield return null;game.RefreshLayout();
                for(int i=0;i<360;i++){yield return null;view.SetReviewAge(start+i/60f);leaves.AdvanceReview(1f/60);Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,360,640),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,i.ToString("D4")+".png"),pixels.EncodeToPNG());}
                if(hash!=game.Interaction.Attempt.CurrentStateHash||!Mathf.Approximately(phase,game.Machine.AmbientPhase))throw new InvalidOperationException("Pause animation changed puzzle or world");
            }finally{view.EndReview();leaves.SetReviewMode(false);Time.captureDeltaTime=oldTime;UnityEngine.Application.targetFrameRate=oldRate;scaler.enabled=false;camera.targetTexture=old;scaler.enabled=true;RenderTexture.active=active;game.RefreshLayout();UnityEngine.Object.DestroyImmediate(pixels);target.Release();UnityEngine.Object.DestroyImmediate(target);}
        }
    }
}
