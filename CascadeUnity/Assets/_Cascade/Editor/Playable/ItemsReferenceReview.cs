using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace Cascade.Editor.Playable
{
    public sealed class ItemsReferenceReview
    {
        const string Phase="ItemsReferenceOct14",Scene="Assets/_Cascade/Scenes/CascadePlayable.unity";
        CascadeGame game;GameObject host;readonly List<string> checks=new List<string>();
        public static void StartReview(bool firstOnly=false){if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play required");UnityEngine.Application.runInBackground=true;var h=new GameObject("Isolated items reference review");UnityEngine.Object.DontDestroyOnLoad(h);h.AddComponent<ReferenceShopLayout>().StartCoroutine(new ItemsReferenceReview{host=h}.Run(firstOnly));}
        Button Button(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        AdService Economy=>(AdService)typeof(CascadeGame).GetProperty("GameEconomy",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
        IEnumerator Run(bool firstOnly)
        {
            string directory=Path.Combine(Path.GetTempPath(),"CascadeItemsReview",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync(Scene);game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Interaction.SetSuspended(false);
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;
            Economy.Ledger.BonusCoins=362;Economy.Ledger.UndoSpools=1;Economy.Ledger.SignalLanterns=1;
            game.SetUiLocale("ru");game.ShowTravelBag();game.ShowSupplies();game.SelectShopItem(1);
            yield return Shot(firstOnly?"second-pass":"reference-spool",720,1280);
            if(!firstOnly){
                foreach(int index in new[]{2,3,0,4,5}){game.SelectShopItem(index);yield return Shot("item-"+index,720,1280);}
                foreach(string language in new[]{"en","ru","uk"}){
                    game.SetUiLocale(language);game.Preferences.LargeText=true;
                    foreach(int index in new[]{0,1,2,3,4,5}){game.SelectShopItem(index);yield return null;Canvas.ForceUpdateCanvases();CheckCopy(language+" item "+index);}
                    game.SelectShopItem(1);yield return Shot(language+"-small",320,693);
                    game.SetEditorSafeInsets(new Vector4(0,.035f,0,.055f));yield return Shot(language+"-safe",390,844);game.SetEditorSafeInsets(Vector4.zero);
                }
                game.Preferences.LargeText=false;game.SetUiLocale("ru");game.SelectShopItem(1);
                Economy.Ledger.UndoSpools=5;game.NotifyStoreChanged();yield return Shot("stock-five",720,1280);
                Economy.Ledger.UndoSpools=0;game.NotifyStoreChanged();yield return Shot("stock-zero",720,1280);
                Economy.Ledger.UndoSpools=1;game.NotifyStoreChanged();yield return Shot("tablet",768,1024);
                string before=game.Interaction.Attempt.CurrentStateHash;
                Button("Items visit shop").onClick.Invoke();yield return null;
                checks.Add("Shop link preserves selected spool: "+(game.CurrentScreen==GameScreen.Shop&&game.GetComponentsInChildren<Image>().Any(i=>i.name=="Catalogue object"&&i.GetComponentInParent<Button>().name=="Shop object 1")));
                Button("Back").onClick.Invoke();yield return null;checks.Add("Shop back resumes item visit: "+(game.CurrentScreen==GameScreen.Supplies));
                checks.Add("Navigation leaves puzzle unchanged: "+(before==game.Interaction.Attempt.CurrentStateHash));
                var glow=game.GetComponentsInChildren<ItemSunlight>().Single();float t=glow.AnimationAge;yield return new WaitForSecondsRealtime(.5f);checks.Add("Glow advances in foreground: "+(glow.AnimationAge>t));
                game.SendMessage("OnApplicationPause",true);t=glow.AnimationAge;yield return new WaitForSecondsRealtime(.25f);checks.Add("Glow freezes in background: "+Mathf.Approximately(glow.AnimationAge,t));game.SendMessage("OnApplicationPause",false);
                yield return new WaitForSecondsRealtime(9);yield return Shot("leaves-and-glow",720,1280);
                var leaves=game.GetComponentsInChildren<MenuLeafDrift>().Single();checks.Add("Depth leaves visible: "+(leaves.DepthOfField&&leaves.VisibleCount>0));
                yield return ItemsAnimationCapture.Run(game);
                game.ReducedMotion=true;t=glow.AnimationAge;yield return new WaitForSecondsRealtime(.2f);checks.Add("Reduced motion freezes light and hides leaves: "+(Mathf.Approximately(t,glow.AnimationAge)&&leaves.VisibleCount==0));yield return Shot("reduced-motion",390,844);game.ReducedMotion=false;
                game.ShowHome();game.ShowCollection();yield return null;checks.Add("Trophy screen uses depth foliage: "+game.GetComponentsInChildren<MenuLeafDrift>().Single().DepthOfField);
                checks.Add("Final compiler status: "+!EditorUtility.scriptCompilationFailed);
            }
            File.WriteAllLines(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase,firstOnly?"first-review.txt":"review.txt"),checks);
            CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync(Scene);UnityEngine.Object.Destroy(host);if(Directory.Exists(directory))Directory.Delete(directory,true);EditorApplication.isPlaying=false;
        }
        void CheckCopy(string phase){foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){t.ForceMeshUpdate();if(t.isTextOverflowing||t.isTextTruncated)checks.Add("OVERFLOW "+phase+": "+t.name);}}
        IEnumerator Shot(string name,int width,int height)
        {
            yield return new WaitForSecondsRealtime(.25f);yield return PlayableCapture.CaptureSettled(Phase,name+".png",width,height);CheckCopy(name);
            var hits=new List<RaycastResult>();int ok=0,total=0;
            foreach(var b in game.GetComponentsInChildren<Button>().Where(b=>b.interactable)){total++;var r=(RectTransform)b.transform;EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(Camera.main,r.TransformPoint(r.rect.center))},hits);if(hits.FirstOrDefault().gameObject?.GetComponentInParent<Button>()==b)ok++;hits.Clear();}
            checks.Add("Buttons "+name+": "+ok+"/"+total);
            var sprite=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Items selected object");checks.Add("Proportional item "+name+": "+sprite.preserveAspect);
        }
    }
    public static class ItemsAnimationCapture
    {
        public static IEnumerator Run(CascadeGame game)
        {
            var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();var old=camera.targetTexture;var active=RenderTexture.active;
            int oldRate=UnityEngine.Application.targetFrameRate;float oldTime=Time.captureDeltaTime;
            string folder=Path.Combine(Path.GetTempPath(),"cascade-items-frames");Directory.CreateDirectory(folder);
            var light=game.GetComponentsInChildren<ItemSunlight>().Single();float lightStart=light.AnimationAge;var leaves=game.GetComponentsInChildren<MenuLeafDrift>().Single();leaves.SetReviewMode(true);
            var target=new RenderTexture(360,640,24);var pixels=new Texture2D(360,640,TextureFormat.RGB24,false);string hash=game.Interaction.Attempt.CurrentStateHash;
            try{
                UnityEngine.Application.targetFrameRate=60;Time.captureDeltaTime=1f/60;scaler.enabled=false;camera.targetTexture=target;scaler.enabled=true;yield return null;game.RefreshLayout();
                for(int i=0;i<480;i++){
                    yield return null;light.SetReviewAge(lightStart+i/60f);leaves.AdvanceReview(1f/60);Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
                    pixels.ReadPixels(new Rect(0,0,360,640),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,i.ToString("D4")+".png"),pixels.EncodeToPNG());
                }
                if(hash!=game.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Animation changed puzzle");
            }finally{light.EndReview();leaves.SetReviewMode(false);Time.captureDeltaTime=oldTime;UnityEngine.Application.targetFrameRate=oldRate;scaler.enabled=false;camera.targetTexture=old;scaler.enabled=true;RenderTexture.active=active;game.RefreshLayout();UnityEngine.Object.DestroyImmediate(pixels);target.Release();UnityEngine.Object.DestroyImmediate(target);}
        }
    }
}
