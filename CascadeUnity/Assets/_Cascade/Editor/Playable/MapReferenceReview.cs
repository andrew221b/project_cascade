using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public sealed class MapReferenceReview
    {
        const string Phase="MapReferenceOct14",Scene="Assets/_Cascade/Scenes/CascadePlayable.unity";
        CascadeGame game;GameObject host;readonly List<string> checks=new List<string>();
        public static void Start(){if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play required");UnityEngine.Application.runInBackground=true;var h=new GameObject("Isolated map reference review");UnityEngine.Object.DontDestroyOnLoad(h);h.AddComponent<ReferenceShopLayout>().StartCoroutine(new MapReferenceReview{host=h}.Run());}
        IEnumerator Run()
        {
            string directory=Path.Combine(Path.GetTempPath(),"CascadeMapReview",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync(Scene);game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();MapReferenceBaseline.Configure(game);game.SetUiLocale("ru");game.ShowJourney();
            yield return Shot("reference-composition",720,1280);
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);game.Preferences.LargeText=false;game.ShowJourney();yield return Shot(locale+"-regular",720,1280);
                game.Preferences.LargeText=true;game.ShowJourney();yield return Shot(locale+"-small",320,693);
                game.SetEditorSafeInsets(new Vector4(0,.035f,0,.055f));yield return Shot(locale+"-safe",390,844);game.SetEditorSafeInsets(Vector4.zero);
            }
            game.Preferences.LargeText=false;game.SetUiLocale("ru");game.ShowJourney();yield return Shot("tablet",768,1024);
            var view=game.GetComponentsInChildren<MapReferenceAmbiance>().Single();string hash=game.Interaction.Attempt.CurrentStateHash;float age=view.AnimationAge;
            yield return new WaitForSecondsRealtime(.4f);checks.Add("Map moves without changing puzzle: "+(view.AnimationAge>age&&game.Interaction.Attempt.CurrentStateHash==hash));
            checks.Add("Focus glow: "+(game.GetComponentsInChildren<MapIslandLight>().SingleOrDefault()?.transform.parent.name??"missing"));
            game.SendMessage("OnApplicationPause",true);age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);checks.Add("Background freezes art: "+Mathf.Approximately(age,view.AnimationAge));game.SendMessage("OnApplicationPause",false);
            yield return MapAnimationCapture.Run(game);yield return Shot("animated-pose",720,1280);
            game.ReducedMotion=true;age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);checks.Add("Reduced motion stops art: "+Mathf.Approximately(age,view.AnimationAge));yield return Shot("reduced-motion",390,844);game.ReducedMotion=false;
            var button=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Map settings");button.onClick.Invoke();yield return null;checks.Add("Settings opens: "+(game.CurrentScreen==GameScreen.Settings));game.GetComponentsInChildren<Button>().Single(b=>b.name=="Back").onClick.Invoke();yield return null;checks.Add("Back returns map with unchanged puzzle: "+(game.CurrentScreen==GameScreen.JourneyMap&&game.Interaction.Attempt.CurrentStateHash==hash));
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Island2").onClick.Invoke();yield return null;checks.Add("Earned island opens levels: "+(game.BrowsedIsland==1));game.CloseJourneyIsland();yield return null;game.GetComponentsInChildren<Button>().Single(b=>b.name=="Island3").onClick.Invoke();yield return null;checks.Add("Locked island stays closed: "+(game.BrowsedIsland<0));
            File.WriteAllLines(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase,"review.txt"),checks);
            CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync(Scene);UnityEngine.Object.Destroy(host);if(Directory.Exists(directory))Directory.Delete(directory,true);EditorApplication.isPlaying=false;
        }
        IEnumerator Shot(string name,int width,int height)
        {
            yield return new WaitForSecondsRealtime(.2f);yield return PlayableCapture.CaptureSettled(Phase,name+".png",width,height,g=>{g.GetComponentsInChildren<MapReferenceAmbiance>().Single().PaintNow();Canvas.ForceUpdateCanvases();CheckNative(name);});
        }
        void CheckNative(string name)
        {
            foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){t.ForceMeshUpdate();if(t.isTextOverflowing||t.isTextTruncated)checks.Add("OVERFLOW "+name+": "+t.name);}
            var hits=new List<RaycastResult>();int total=0,ok=0;
            foreach(var b in game.GetComponentsInChildren<Button>().Where(b=>b.interactable)){total++;var r=(RectTransform)b.transform;EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(Camera.main,r.TransformPoint(r.rect.center))},hits);if(hits.FirstOrDefault().gameObject?.GetComponentInParent<Button>()==b)ok++;hits.Clear();}
            checks.Add("Buttons "+name+": "+ok+"/"+total);
            checks.Add("Bridges attached "+name+": "+game.GetComponentsInChildren<MapRopeBridge>().All(b=>b.AttachmentError<.001f));
            checks.Add("Proportional island art "+name+": "+game.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Reference map island artwork")).All(i=>i.preserveAspect&&i.color==Color.white));
        }
    }
    public static class MapAnimationCapture
    {
        public static IEnumerator Run(CascadeGame game)
        {
            var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();var old=camera.targetTexture;var active=RenderTexture.active;int oldRate=UnityEngine.Application.targetFrameRate;float oldTime=Time.captureDeltaTime;
            var view=game.GetComponentsInChildren<MapReferenceAmbiance>().Single();float start=view.AnimationAge;var leaves=game.GetComponentsInChildren<MenuLeafDrift>().Single();leaves.SetReviewMode(true);
            string folder=Path.Combine(Path.GetTempPath(),"cascade-map-frames");Directory.CreateDirectory(folder);var target=new RenderTexture(360,640,24);var pixels=new Texture2D(360,640,TextureFormat.RGB24,false);string hash=game.Interaction.Attempt.CurrentStateHash;float error=0;
            try{
                UnityEngine.Application.targetFrameRate=60;Time.captureDeltaTime=1f/60;scaler.enabled=false;camera.targetTexture=target;scaler.enabled=true;yield return null;game.RefreshLayout();
                for(int i=0;i<360;i++){yield return null;view.SetReviewAge(start+i/60f);leaves.AdvanceReview(1f/60);Canvas.ForceUpdateCanvases();foreach(var b in view.Bridges)error=Mathf.Max(error,b.AttachmentError);camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,360,640),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,i.ToString("D4")+".png"),pixels.EncodeToPNG());}
                if(hash!=game.Interaction.Attempt.CurrentStateHash||error>.001f)throw new InvalidOperationException("Map motion changed puzzle or detached bridge");
                File.WriteAllText(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","MapReferenceOct14","motion-native.json"),"{\"frames\":360,\"fps\":60,\"duration_seconds\":6,\"bridge_error\":"+error.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"puzzle_unchanged\":true}");
            }finally{view.EndReview();leaves.SetReviewMode(false);Time.captureDeltaTime=oldTime;UnityEngine.Application.targetFrameRate=oldRate;scaler.enabled=false;camera.targetTexture=old;scaler.enabled=true;RenderTexture.active=active;game.RefreshLayout();UnityEngine.Object.DestroyImmediate(pixels);target.Release();UnityEngine.Object.DestroyImmediate(target);}
        }
    }
}
