using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Persistence;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Cascade.Editor.Playable {
public static class WorldMenusReview {
const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
const string Phase="WorldMenusOct14",Scene="Assets/_Cascade/Scenes/CascadePlayable.unity";
static readonly List<string> checks=new List<string>();static CascadeGame game;static int expected;
public static void Start(){checks.Clear();var host=new GameObject("Isolated world menus review");UnityEngine.Object.DontDestroyOnLoad(host);host.AddComponent<ReferenceShopLayout>().StartCoroutine(Run(host));}
static void Route(GameScreen screen){typeof(CascadeGame).GetMethod("SetScreen",Flags).Invoke(game,new object[]{screen});}
public static void ConfigureWorld(CascadeGame g,int world){
var p=(PlayerProfile)typeof(CascadeGame).GetField("profile",Flags).GetValue(g);p.Clears.Clear();p.Rewards=new Cascade.Application.JourneyRewards{CoinEconomyRevision=1};((HashSet<string>)typeof(CascadeGame).GetField("sessionClears",Flags).GetValue(g)).Clear();MapReferenceBaseline.Configure(g,world*60+18);
typeof(CascadeGame).GetField("journeyActive",Flags).SetValue(g,false);typeof(CascadeGame).GetField("mapPage",Flags).SetValue(g,world);
}
static IEnumerator Run(GameObject host){
string dir=Path.Combine(Path.GetTempPath(),"CascadeWorldReview",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=dir;UnityEngine.Application.runInBackground=true;
yield return SceneManager.LoadSceneAsync(Scene);game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SetUiLocale("ru");
for(int w=0;w<9;w++){expected=w;ConfigureWorld(game,w);Route(GameScreen.JourneyMap);yield return Shot("world-"+w+"-map");game.ShowHome();yield return Shot("world-"+w+"-home");game.ShowShop();yield return Shot("world-"+w+"-shop");}
expected=8;foreach(var screen in new[]{GameScreen.TravelBag,GameScreen.Supplies,GameScreen.Collection,GameScreen.Pause,GameScreen.Settings}){Route(screen);yield return Shot("night-"+screen);}
foreach(string locale in new[]{"en","ru","uk"}){game.SetUiLocale(locale);game.Preferences.LargeText=true;game.SetEditorSafeInsets(new Vector4(0,.035f,0,.055f));game.ShowHome();yield return Shot(locale+"-safe-home",390,844);game.ShowShop();yield return Shot(locale+"-safe-shop",390,844);Route(GameScreen.JourneyMap);yield return Shot(locale+"-safe-map",390,844);}
game.SetEditorSafeInsets(Vector4.zero);game.Preferences.LargeText=false;game.SetUiLocale("ru");game.ShowHome();yield return null;
var env=game.GetComponentsInChildren<MenuWorldEnvironment>().Single();float t=env.AnimationAge;string hash=game.Interaction.Attempt.CurrentStateHash;yield return new WaitForSecondsRealtime(.3f);checks.Add("Motion advances: "+(env.AnimationAge>t));
game.SendMessage("OnApplicationPause",true);t=env.AnimationAge;yield return new WaitForSecondsRealtime(.2f);checks.Add("Background freezes: "+Mathf.Approximately(t,env.AnimationAge));game.SendMessage("OnApplicationPause",false);
game.ReducedMotion=true;t=env.AnimationAge;yield return new WaitForSecondsRealtime(.2f);checks.Add("Reduced motion freezes and hides aircraft: "+(Mathf.Approximately(t,env.AnimationAge)&&env.VisibleAircraft==0));game.ReducedMotion=false;
checks.Add("Puzzle unchanged by atmosphere: "+(hash==game.Interaction.Attempt.CurrentStateHash));
            yield return WorldMenuMotionCapture.Run(game);
            foreach(int w in new[]{1,6,8}){game.LoadNode(game.Catalog.PrimaryLevels[w*60+18].Definition.CampaignNodeId);yield return new WaitForSecondsRealtime(.35f);yield return PlayableCapture.CaptureSettled(Phase,"world-"+w+"-playing.png",390,844,g=>{var layer=g.GetComponentsInChildren<LayeredWorld>().Single();checks.Add("Gameplay "+w+" world="+layer.Zone+" aircraft="+layer.ExtraAircraftCount);});Route(GameScreen.Pause);expected=w;yield return Shot("world-"+w+"-pause-real");}File.WriteAllLines(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase,"review.txt"),checks);
CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync(Scene);UnityEngine.Object.Destroy(host);if(Directory.Exists(dir))Directory.Delete(dir,true);EditorApplication.isPlaying=false;
}
static IEnumerator Shot(string name,int width=720,int height=1280){yield return new WaitForSecondsRealtime(.45f);yield return PlayableCapture.CaptureSettled(Phase,name+".png",width,height,g=>{Canvas.ForceUpdateCanvases();Check(name);});}
static void Check(string name){var env=game.GetComponentsInChildren<MenuWorldEnvironment>().Single();checks.Add(name+" theme: "+(env.World==expected&&game.EnvironmentWorld==expected)+" aircraft="+env.VisibleAircraft+" smoke="+env.ChimneyCount);foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){t.ForceMeshUpdate();if(t.isTextOverflowing||t.isTextTruncated)checks.Add("OVERFLOW "+name+" "+t.name);}
var hits=new List<RaycastResult>();int total=0,ok=0;foreach(var b in game.GetComponentsInChildren<Button>().Where(b=>b.interactable)){total++;var r=(RectTransform)b.transform;EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(Camera.main,r.TransformPoint(r.rect.center))},hits);if(hits.FirstOrDefault().gameObject?.GetComponentInParent<Button>()==b)ok++;hits.Clear();}checks.Add(name+" buttons="+ok+"/"+total);}
}
}
