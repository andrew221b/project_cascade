using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class MenuPolishReview
    {
        const string Phase="MenuPolishOct14";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        [MenuItem("Cascade/Review/Menu Ground And Alignment")]
        public static void Start()
        {
            // Recompile shared includes as well as scripts; external art edits can otherwise
            // leave the Editor's loaded shader version behind the source on disk.
            foreach(string asset in new[]{"FrameReskin.cginc","FrameSurface.shader","FrameScene.shader"})
                AssetDatabase.ImportAsset("Assets/_Cascade/Content/Resources/CascadeArt/WorldFramesOct14/"+asset,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
            var host=new GameObject("Isolated menu polish review");UnityEngine.Object.DontDestroyOnLoad(host);
            host.AddComponent<ReferenceShopLayout>().StartCoroutine(Run(host));
        }
        static IEnumerator Run(GameObject host)
        {
            var checks=new List<string>();
            string dir=Path.Combine(Path.GetTempPath(),"CascadeMenuPolish",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=dir;UnityEngine.Application.runInBackground=true;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SetUiLocale("ru");
            foreach(int world in new[]{8,0,1,2,3,4,5,6,7}){
                WorldMenusReview.ConfigureWorld(game,world);game.ShowTravelBag();yield return new WaitForSecondsRealtime(1.1f);
                yield return PlayableCapture.CaptureSettled(Phase,"world-"+world+"-inventory.png",390,844,g=>CheckInventory(g,checks));
            }
            WorldMenusReview.ConfigureWorld(game,8);
            foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(320,693),new Vector2Int(768,1024)}){
                game.ShowTravelBag();yield return new WaitForSecondsRealtime(1.1f);
                yield return PlayableCapture.CaptureSettled(Phase,"night-inventory-"+size.x+"x"+size.y+".png",size.x,size.y,g=>CheckInventory(g,checks));
            }
            foreach(string locale in new[]{"en","uk"}){
                game.SetUiLocale(locale);game.ShowTravelBag();yield return new WaitForSecondsRealtime(1.1f);
                yield return PlayableCapture.CaptureSettled(Phase,"night-inventory-"+locale+".png",390,844,g=>CheckInventory(g,checks));
            }
            game.SetUiLocale("ru");
            game.ShowSupplies();yield return new WaitForSecondsRealtime(1.1f);
            yield return PlayableCapture.CaptureSettled(Phase,"night-items.png",390,844);
            game.ShowCollection();yield return new WaitForSecondsRealtime(1.1f);
            yield return PlayableCapture.CaptureSettled(Phase,"night-trophies.png",390,844,g=>CheckGround(g));
            game.ShowShop();yield return new WaitForSecondsRealtime(1.1f);
            yield return PlayableCapture.CaptureSettled(Phase,"night-shop.png",390,844);
            game.ShowHome();yield return new WaitForSecondsRealtime(1.1f);
            yield return PlayableCapture.CaptureSettled(Phase,"night-home.png",390,844);
            game.LoadNode(game.Catalog.PrimaryLevels[8*60+18].Definition.CampaignNodeId);yield return new WaitForSecondsRealtime(1.1f);
            game.ToggleFieldToolTray();yield return new WaitForSecondsRealtime(.6f);
            yield return PlayableCapture.CaptureSettled(Phase,"night-help.png",390,844);
            typeof(CascadeGame).GetMethod("SetToolTray",Private).Invoke(game,new object[]{false,true});
            typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{GameScreen.Pause});yield return new WaitForSecondsRealtime(1.1f);
            yield return PlayableCapture.CaptureSettled(Phase,"night-pause.png",390,844,g=>CheckGround(g));
            string output=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase);
            File.WriteAllLines(Path.Combine(output,"verification.txt"),checks);Debug.Log("MENU POLISH REVIEW COMPLETE\n"+string.Join("\n",checks));
            CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            UnityEngine.Object.Destroy(host);if(Directory.Exists(dir))Directory.Delete(dir,true);EditorApplication.isPlaying=false;
        }
        static void CheckGround(CascadeGame game)
        {
            var ground=game.GetComponentsInChildren<RawImage>().Single(i=>i.name=="Continuous menu floor");
            if(ground.texture==null||ground.raycastTarget||ground.uvRect.height<=0||ground.uvRect.y<0||Mathf.Abs(ground.uvRect.yMax-1)>.001f)throw new InvalidOperationException("Floor is missing or repeats instead of cropping");
            var canvas=(RectTransform)ground.GetComponentInParent<Canvas>().transform;
            var corners=new Vector3[4];ground.rectTransform.GetWorldCorners(corners);
            var low=canvas.InverseTransformPoint(corners[0]);var high=canvas.InverseTransformPoint(corners[2]);
            if(Mathf.Abs(low.x-canvas.rect.xMin)>.02f||Mathf.Abs(low.y-canvas.rect.yMin)>.02f||Mathf.Abs(high.x-canvas.rect.xMax)>.02f)throw new InvalidOperationException("Ground does not reach viewport edges");
            var frame=game.GetComponentsInChildren<Image>().Single(i=>i.name=="World-specific architectural frame");
            if(!frame.material.HasProperty("_CleanCutout")||frame.material.GetFloat("_CleanCutout")<.5f)throw new InvalidOperationException("Menu frame cleanup is not enabled");
            if(frame.transform.GetSiblingIndex()<=ground.transform.GetSiblingIndex())throw new InvalidOperationException("Ground obscures frame details");
            MapFrameFocusedReview.CheckViewportFrame(frame);
        }
        static void CheckInventory(CascadeGame game,List<string> checks)
        {
            CheckGround(game);
            foreach(var icon in game.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Inventory ")&&i.name.EndsWith(" icon"))){
                var rect=icon.rectTransform;var body=(RectTransform)rect.parent;
                if(Mathf.Abs(-rect.anchoredPosition.y+rect.rect.height*.5f-body.rect.height*.5f)>.01f||rect.rect.width>100)throw new InvalidOperationException("Inventory icon is oversized or off centre");
            }
            foreach(string card in new[]{"Inventory lights","Inventory items","Inventory balance","Inventory trophies"}){
                var texts=game.GetComponentsInChildren<TMP_Text>();var title=texts.Single(t=>t.name=="Reference shop "+card+" title");var count=texts.Single(t=>t.name=="Reference shop "+card+" count");
                if(Mathf.Abs(title.rectTransform.anchoredPosition.x+title.rectTransform.rect.width*.5f-count.rectTransform.anchoredPosition.x-count.rectTransform.rect.width*.5f)>.01f)throw new InvalidOperationException("Card text centres differ");
                if(title.isTextOverflowing||count.isTextOverflowing)throw new InvalidOperationException("Inventory text overflow");
            }
            var target=Camera.main.targetTexture;
            checks.Add("PASS "+game.UiLanguage+" "+target.width+"x"+target.height+": one floor to viewport bottom, frame in front, compact centered icons, aligned card text without overflow");
        }
    }
}
