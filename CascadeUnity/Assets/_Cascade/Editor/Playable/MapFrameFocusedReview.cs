using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class MapFrameFocusedReview
    {
        const string Phase="FrameReskinOct14";
        const BindingFlags PrivateInstance=BindingFlags.Instance|BindingFlags.NonPublic;
        [MenuItem("Cascade/Review/Map World Frames")]
        public static void Start()
        {
            var host=new GameObject("Isolated map frame review");
            UnityEngine.Object.DontDestroyOnLoad(host);
            host.AddComponent<ReferenceShopLayout>().StartCoroutine(Run(host));
        }
        static IEnumerator Run(GameObject host)
        {
            var checks=new List<string>();Vector2 firstMapSize=Vector2.zero;Vector3[] firstGameplayMesh=null;
            string dir=Path.Combine(Path.GetTempPath(),"CascadeMapFrame",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=dir;
            UnityEngine.Application.runInBackground=true;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SetUiLocale("ru");
            for(int world=0;world<9;world++)
            {
                WorldMenusReview.ConfigureWorld(game,world);
                typeof(CascadeGame).GetField("selectedJourneyStop",PrivateInstance).SetValue(game,world*60+18);
                typeof(CascadeGame).GetMethod("SetScreen",PrivateInstance).Invoke(game,new object[]{GameScreen.JourneyMap});
                yield return new WaitForSecondsRealtime(1.1f);
                foreach(var size in world==0||world==6||world==8?new[]{new Vector2Int(390,844),new Vector2Int(720,1280),new Vector2Int(768,1024)}:new[]{new Vector2Int(390,844)})
                {
                    yield return PlayableCapture.CaptureSettled(Phase,"world-"+world+"-"+size.x+"x"+size.y+".png",size.x,size.y,g=>
                    {
                        var frame=g.GetComponentsInChildren<Image>().Single(x=>x.name=="World-specific architectural frame");
                        if(frame.sprite!=WorldFrameArt.MapFrame()||frame.raycastTarget||frame.type!=Image.Type.Sliced)throw new InvalidOperationException("Map does not use first-world silhouette");
                        CheckViewportFrame(frame);
                        if(size.x==390){if(world==0)firstMapSize=frame.rectTransform.rect.size;else if(Vector2.Distance(firstMapSize,frame.rectTransform.rect.size)>.01f)throw new InvalidOperationException("World map dimensions differ");}
                        if(frame.material.GetTexture("_ThemeAccentTex")!=WorldFrameArt.Accents()||WorldFrameArt.Accents()==null)throw new InvalidOperationException("Missing reskin accents");
                        var stage=g.GetComponentsInChildren<ReferenceShopLayout>().Single();
                        var islands=g.GetComponentsInChildren<RectTransform>().Where(x=>x.name.StartsWith("Floating island ")).ToArray();
                        if(islands.Length!=5)throw new InvalidOperationException("Map island composition incomplete");
                        if(stage.transform.parent!=frame.transform.parent)throw new InvalidOperationException("Map frame still scales with islands");
                        checks.Add("PASS world "+world+" map "+size+": first-world silhouette, equal geometry, five spaced islands without decorative bridges");
                    });
                }
                if(world==0||world==1||world==4||world==6||world==8)
                {
                    game.ShowHome();
                    yield return new WaitForSecondsRealtime(1.1f);
                    yield return PlayableCapture.CaptureSettled(Phase,"world-"+world+"-home.png",720,1280,g=>
                    {
                        var frame=g.GetComponentsInChildren<Image>().Single(x=>x.name=="World-specific architectural frame");
                        if(frame.sprite!=WorldFrameArt.MenuFrame())throw new InvalidOperationException("Home reskin changes the first-world frame");
                        CheckViewportFrame(frame);checks.Add("PASS world "+world+" home: original menu silhouette fills the viewport");
                    });
                    game.ShowTravelBag();yield return new WaitForSecondsRealtime(1.1f);
                    yield return PlayableCapture.CaptureSettled(Phase,"world-"+world+"-inventory.png",390,844,g=>CheckViewportFrame(g.GetComponentsInChildren<Image>().Single(x=>x.name=="World-specific architectural frame")));
                }
                game.LoadNode(game.Catalog.PrimaryLevels[world*60+18].Definition.CampaignNodeId);
                yield return new WaitForSecondsRealtime(.35f);
                yield return PlayableCapture.CaptureSettled(Phase,"world-"+world+"-playing.png",390,844,g=>
                {
                    var layer=g.GetComponentInChildren<LayeredWorld>();
                    var frame=layer.GetComponentsInChildren<MeshRenderer>().Single(x=>x.name=="Thematic level framing");
                    if(frame.sharedMaterial.GetTexture("_BaseMap")!=GameplayReferenceArt.Texture("GameplayFrame"))throw new InvalidOperationException("Gameplay reskin changes the first-world artwork");
                    var mesh=frame.GetComponent<MeshFilter>().sharedMesh.vertices;
                    if(world==0)firstGameplayMesh=mesh;else if(mesh.Length!=firstGameplayMesh.Length||mesh.Where((v,i)=>Vector3.Distance(v,firstGameplayMesh[i])>.0001f).Any())throw new InvalidOperationException("Gameplay frame geometry differs by world");
                    checks.Add("PASS world "+world+" gameplay: original silhouette and identical mesh");
                });
                if(world==0||world==1||world==4||world==6||world==8){
                    typeof(CascadeGame).GetMethod("SetScreen",PrivateInstance).Invoke(game,new object[]{GameScreen.Pause});
                    yield return new WaitForSecondsRealtime(1.1f);
                    yield return PlayableCapture.CaptureSettled(Phase,"world-"+world+"-pause.png",390,844,g=>CheckViewportFrame(g.GetComponentsInChildren<Image>().Single(x=>x.name=="World-specific architectural frame")));
                }
            }
            string output=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase);File.WriteAllLines(Path.Combine(output,"verification.txt"),checks);Debug.Log("FRAME RESKIN REVIEW COMPLETE\n"+string.Join("\n",checks));
            CascadeGame.EditorProfileDirectory=null;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            UnityEngine.Object.Destroy(host);
            if(Directory.Exists(dir))Directory.Delete(dir,true);
            EditorApplication.isPlaying=false;
        }
        internal static void CheckViewportFrame(Image frame)
        {
            var bounds=new Vector3[4];var screen=new Vector3[4];frame.rectTransform.GetWorldCorners(bounds);
            ((RectTransform)frame.GetComponentInParent<Canvas>().transform).GetWorldCorners(screen);
            for(int i=0;i<4;i++)if(Vector3.Distance(bounds[i],screen[i])>.01f)throw new InvalidOperationException("Frame does not meet physical screen edge "+i);
        }
    }
}
