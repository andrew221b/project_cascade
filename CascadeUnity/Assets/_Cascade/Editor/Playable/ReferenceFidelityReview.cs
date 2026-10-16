using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public static class ReferenceFidelityReview
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static readonly List<string> checks=new List<string>();
        [MenuItem("Cascade/Review/Reference Fidelity Baseline")]
        public static void Baseline()=>Start("ReferenceFidelityOct14-Before",false);
        [MenuItem("Cascade/Review/Reference Fidelity")]
        public static void Final()=>Start("ReferenceFidelityOct14",true);
        static void Start(string phase,bool full){
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode before capturing reference screens.");
            foreach(string file in new[]{"FailureAtlas.png","FailureReferenceAtlas.png","ReferenceDaySky.png","FailureReferenceSource.jpg"}){
            string path="Assets/_Cascade/Content/Resources/CascadeArt/GameplayReferenceOct14/"+file;
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            }
            AssetDatabase.ImportAsset("Assets/_Cascade/Content/Resources/CascadeArt/WorldFramesOct14/FrameSurface.shader",ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset("Assets/_Cascade/Content/Resources/CascadeArt/WorldMenusOct14/WorldSurface.shader",ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
            checks.Clear();
            var host=new GameObject("Isolated reference fidelity review");UnityEngine.Object.DontDestroyOnLoad(host);host.AddComponent<ReferenceShopLayout>().StartCoroutine(Run(host,phase,full));
        }
        static void Route(CascadeGame game,GameScreen screen)=>typeof(CascadeGame).GetMethod("SetScreen",Flags).Invoke(game,new object[]{screen});
        static IEnumerator Shot(CascadeGame game,string phase,string name,int width=720,int height=1280){yield return new WaitForSecondsRealtime(.8f);yield return PlayableCapture.CaptureSettled(phase,name+".png",width,height,g=>{
            if(g.CurrentScreen==GameScreen.Fail){
                var root=g.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Reference failure result");if(root.parent!=g.GetComponentInChildren<Canvas>().transform)throw new InvalidOperationException("Failure composition is inset twice");
                if(g.GetComponentsInChildren<HangingResultRope>().Length!=2||g.GetComponentsInChildren<HangingResultRope>().Any(r=>r.AttachmentError>.015f))throw new InvalidOperationException("Result rope attachment gap");
                var buttons=g.GetComponentsInChildren<Button>();if(buttons.Count(b=>b.name=="Retry")!=1||buttons.Count(b=>b.name=="Resulthome")!=1)throw new InvalidOperationException("Duplicate failure actions");
                if(g.GetComponentsInChildren<Image>().Count(i=>i.name.StartsWith("Failure result medal")&&i.sprite.texture.name=="FailureReferenceSource")!=3)throw new InvalidOperationException("Wrong failure medal family");
            }
            if(g.CurrentScreen==GameScreen.NoAds){foreach(string item in new[]{"No ads illustrated emblem","No ads parchment explanation","Calendar 7","Calendar 30","Calendar 365"})if(g.GetComponentsInChildren<Image>().Single(i=>i.name==item).sprite==null)throw new InvalidOperationException("Blank subscription illustration: "+item);}
            foreach(var text in g.GetComponentsInChildren<TMP_Text>().Where(t=>t.name.StartsWith("Reference shop"))){text.ForceMeshUpdate();if(text.isTextOverflowing||text.isTextTruncated)throw new InvalidOperationException("Reference overflow "+name+" "+text.name);}
            checks.Add("PASS "+name+" "+width+"x"+height+": reference text fits"+(g.CurrentScreen==GameScreen.Fail?", one retry/map pair, three silver medallions, attached cords, full viewport composition":""));
        });}
        static IEnumerator Run(GameObject host,string phase,bool full)
        {
            string dir=Path.Combine(Path.GetTempPath(),"CascadeReferenceFidelity",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=dir;
            UnityEngine.Application.runInBackground=true;yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            WorldMenusReview.ConfigureWorld(game,0);game.SetUiLocale("ru");game.SetEditorSafeInsets(new Vector4(0,.00001f,0,.00001f));game.LoadNode(game.Catalog.PrimaryLevels[18].Definition.CampaignNodeId);
            yield return Shot(game,phase,"gameplay");Route(game,GameScreen.Fail);yield return Shot(game,phase,"failure");
            if(full){
                foreach(var screen in new[]{GameScreen.Home,GameScreen.JourneyMap,GameScreen.TravelBag,GameScreen.Supplies,GameScreen.Collection,GameScreen.Shop,GameScreen.Pause,GameScreen.NoAds}){Route(game,screen);yield return Shot(game,phase,screen.ToString());}
                Route(game,GameScreen.Playing);game.ToggleFieldToolTray();yield return Shot(game,phase,"help");typeof(CascadeGame).GetMethod("SetToolTray",Flags).Invoke(game,new object[]{false,true});
                game.ShowTravelBag();game.ShowParcel();var parcel=game.GetComponentInChildren<ParcelRevealView>();if(parcel!=null)parcel.SetReviewPose(4.65f);yield return Shot(game,phase,"parcel");
                foreach(int world in new[]{1,4,6,8}){WorldMenusReview.ConfigureWorld(game,world);game.LoadNode(game.Catalog.PrimaryLevels[world*60+18].Definition.CampaignNodeId);Route(game,GameScreen.Fail);yield return Shot(game,phase,"world-"+world+"-failure",390,844);}
                foreach(string locale in new[]{"en","uk"}){game.SetUiLocale(locale);Route(game,GameScreen.Playing);Route(game,GameScreen.Fail);yield return Shot(game,phase,locale+"-failure",390,844);}
                yield return GameplayReferenceReview.Extra(game,"ReferenceFidelityBoardsOct14");
            }
            string output=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",phase);File.WriteAllLines(Path.Combine(output,"verification.txt"),checks);
            CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");UnityEngine.Object.Destroy(host);if(Directory.Exists(dir))Directory.Delete(dir,true);Debug.Log("REFERENCE FIDELITY "+phase+" COMPLETE");EditorApplication.isPlaying=false;
        }
    }
}
