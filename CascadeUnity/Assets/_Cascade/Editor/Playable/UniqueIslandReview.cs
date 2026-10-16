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
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace Cascade.Editor.Playable {
public static class UniqueIslandReview {
const string Phase="UniqueWorldIslandsOct14",Scene="Assets/_Cascade/Scenes/CascadePlayable.unity";const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
static CascadeGame game;static int world;static readonly List<string> checks=new List<string>();
static void Route(GameScreen s)=>typeof(CascadeGame).GetMethod("SetScreen",Flags).Invoke(game,new object[]{s});
public static void Start(){checks.Clear();var host=new GameObject("Isolated unique island review");UnityEngine.Object.DontDestroyOnLoad(host);host.AddComponent<ReferenceShopLayout>().StartCoroutine(Run(host));}
static IEnumerator Run(GameObject host){string dir=Path.Combine(Path.GetTempPath(),"CascadeUniqueIslandReview",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=dir;UnityEngine.Application.runInBackground=true;yield return SceneManager.LoadSceneAsync(Scene);game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SetUiLocale("ru");
for(world=0;world<9;world++){WorldMenusReview.ConfigureWorld(game,world);typeof(CascadeGame).GetField("selectedJourneyStop",Flags).SetValue(game,world*60+18);Route(GameScreen.JourneyMap);yield return Shot("world-"+world+"-map");game.ShowHome();yield return Shot("world-"+world+"-home");game.ShowShop();yield return Shot("world-"+world+"-shop");game.LoadNode(game.Catalog.PrimaryLevels[world*60+18].Definition.CampaignNodeId);yield return new WaitForSecondsRealtime(.35f);yield return PlayableCapture.CaptureSettled(Phase,"world-"+world+"-playing.png",390,844,g=>{var layer=g.GetComponentInChildren<LayeredWorld>();checks.Add("Gameplay "+world+" theme="+(layer.IslandSetWorld==world)+" variants="+layer.IslandVariantCount);});}
world=8;WorldMenusReview.ConfigureWorld(game,world);foreach(var s in new[]{GameScreen.TravelBag,GameScreen.Supplies,GameScreen.Collection,GameScreen.Pause,GameScreen.CoinPacks}){Route(s);yield return Shot("night-"+s);}
foreach(string locale in new[]{"en","ru","uk"}){game.SetUiLocale(locale);game.Preferences.LargeText=true;game.SetEditorSafeInsets(new Vector4(0,.035f,0,.055f));game.ShowHome();yield return Shot(locale+"-safe-home",390,844);Route(GameScreen.JourneyMap);yield return Shot(locale+"-safe-map",390,844);}
game.SetEditorSafeInsets(Vector4.zero);game.Preferences.LargeText=false;game.SetUiLocale("ru");WorldMenusReview.ConfigureWorld(game,4);world=4;Route(GameScreen.JourneyMap);yield return Shot("lagoon-map-close");yield return UniqueIslandMotion.Run(game);
string output=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase);File.WriteAllLines(Path.Combine(output,"review.txt"),checks);CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync(Scene);UnityEngine.Object.Destroy(host);if(Directory.Exists(dir))Directory.Delete(dir,true);EditorApplication.isPlaying=false;
}
static IEnumerator Shot(string name,int w=720,int h=1280){yield return new WaitForSecondsRealtime(.45f);yield return PlayableCapture.CaptureSettled(Phase,name+".png",w,h,g=>{Canvas.ForceUpdateCanvases();foreach(var image in g.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Reference map island artwork")||i.name.StartsWith("Drifting home island")||i.name.StartsWith("Themed distant island"))){checks.Add(name+" "+image.name+" theme="+image.sprite.name.StartsWith(WorldIslandArt.Theme(world))+" proportional="+image.preserveAspect);}foreach(var t in g.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){t.ForceMeshUpdate();if(t.isTextOverflowing||t.isTextTruncated)checks.Add("OVERFLOW "+name+" "+t.name);}foreach(var b in g.GetComponentsInChildren<MapRopeBridge>())if(b.AttachmentError>.001f)checks.Add("DETACHED bridge "+name);});}
}
public static class UniqueIslandMotion {
public static IEnumerator Run(CascadeGame game){var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();var old=camera.targetTexture;var active=RenderTexture.active;float oldTime=Time.captureDeltaTime;int oldRate=UnityEngine.Application.targetFrameRate;var view=game.GetComponentsInChildren<MapReferenceAmbiance>().Single();var env=game.GetComponentsInChildren<MenuWorldEnvironment>().Single();var leaves=game.GetComponentsInChildren<MenuLeafDrift>().Single();string hash=game.Interaction.Attempt.CurrentStateHash;float error=0;string folder=Path.Combine(Path.GetTempPath(),"cascade-unique-island-frames");Directory.CreateDirectory(folder);var target=new RenderTexture(360,640,24);var pixels=new Texture2D(360,640,TextureFormat.RGB24,false);try{UnityEngine.Application.targetFrameRate=60;Time.captureDeltaTime=1f/60;scaler.enabled=false;camera.targetTexture=target;scaler.enabled=true;yield return null;game.RefreshLayout();leaves.SetReviewMode(true);float start=view.AnimationAge,sky=env.AnimationAge;
for(int i=0;i<360;i++){yield return null;view.SetReviewAge(start+i/60f);env.SetReviewAge(sky+i/60f);leaves.AdvanceReview(1f/60);Canvas.ForceUpdateCanvases();foreach(var b in view.Bridges)error=Mathf.Max(error,b.AttachmentError);camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,360,640),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,i.ToString("D4")+".png"),pixels.EncodeToPNG());}
if(error>.001f||hash!=game.Interaction.Attempt.CurrentStateHash)throw new InvalidOperationException("Island motion detached bridge or changed puzzle");File.WriteAllText(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","UniqueWorldIslandsOct14","motion-native.json"),"{\"frames\":360,\"fps\":60,\"duration_seconds\":6,\"bridge_error\":"+error.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"puzzle_unchanged\":true}");
}finally{view.EndReview();env.EndReview();leaves.SetReviewMode(false);Time.captureDeltaTime=oldTime;UnityEngine.Application.targetFrameRate=oldRate;scaler.enabled=false;camera.targetTexture=old;scaler.enabled=true;RenderTexture.active=active;game.RefreshLayout();UnityEngine.Object.DestroyImmediate(pixels);target.Release();UnityEngine.Object.DestroyImmediate(target);}}
}
}
