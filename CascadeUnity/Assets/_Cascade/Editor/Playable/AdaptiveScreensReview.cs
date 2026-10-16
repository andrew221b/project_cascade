using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Domain;
using Cascade.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
namespace Cascade.Editor.Playable {
public static class AdaptiveScreensReview {
 const string Phase="AdaptiveScreensOct14";const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
 static readonly List<string> checks=new List<string>();
 public static void Start(){var host=new GameObject("Isolated adaptive review");UnityEngine.Object.DontDestroyOnLoad(host);host.AddComponent<ReferenceShopLayout>().StartCoroutine(Run(host));}
 static IEnumerator Run(GameObject host){var dir=Path.Combine(Path.GetTempPath(),"CascadeAdaptive",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=dir;UnityEngine.Application.runInBackground=true;yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();WorldMenusReview.ConfigureWorld(game,0);game.SetUiLocale("ru");game.SetEditorSafeInsets(new Vector4(.025f,.025f,.025f,.035f));
 foreach(var screen in new[]{GameScreen.Home,GameScreen.JourneyMap,GameScreen.Shop,GameScreen.TravelBag,GameScreen.Supplies,GameScreen.Collection,GameScreen.Pause}){
  typeof(CascadeGame).GetMethod("SetScreen",Flags).Invoke(game,new object[]{screen});yield return new WaitForSecondsRealtime(.4f);yield return Views(game,screen.ToString());
 }
 foreach(string node in new[]{"C01","C139"}){game.LoadNode(node);game.SetUiLocale("ru");game.ShowGame();float until=Time.realtimeSinceStartup+20;while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<until)yield return null;yield return new WaitForSecondsRealtime(.5f);yield return Views(game,"Playing-"+node);}
 game.ToggleFieldToolTray();yield return new WaitForSecondsRealtime(.5f);yield return Views(game,"Field-help");game.ToggleFieldToolTray();yield return new WaitForSecondsRealtime(.4f);
 game.ShowHome();yield return null;game.SetEditorSafeInsets(Vector4.zero);File.WriteAllLines(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase,"review.txt"),checks);CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");UnityEngine.Object.Destroy(host);Directory.Delete(dir,true);EditorApplication.isPlaying=false;
 }
 static IEnumerator Views(CascadeGame g,string name){foreach(var size in new[]{new Vector2Int(390,844),new Vector2Int(360,640),new Vector2Int(768,1024),new Vector2Int(1024,768),new Vector2Int(844,390)})yield return PlayableCapture.CaptureSettled(Phase,name+"-"+size.x+"x"+size.y+".png",size.x,size.y,game=>{
  var cam=Camera.main;var corners=new Vector3[4];var safe=(RectTransform)typeof(CascadeGame).GetField("safeRoot",Flags).GetValue(game);var floors=(RectTransform)typeof(CascadeGame).GetField("floorArea",Flags).GetValue(game);
  checks.Add(name+" "+size+" safe="+safe.rect.size+" floor="+floors.rect.size);
  var gate=game.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(r=>r.name=="Portrait game layout");
  bool wide=size.x>size.y*1.12f;
  if(wide){if(gate==null||!gate.gameObject.activeInHierarchy||!game.Interaction.IsSuspended)throw new InvalidOperationException("Landscape guidance failed: "+name+" "+size);checks.Add("PORTRAIT GUIDANCE "+name+" "+size);return;}
  if(gate!=null&&gate.gameObject.activeInHierarchy)throw new InvalidOperationException("Portrait blocked: "+name+" "+size);
  foreach(var rect in game.GetComponentsInChildren<RectTransform>().Where(t=>t.name=="Reference shop composition"||t.name=="Home reference layout"||t.name=="Tool belt reference composition"))if(Mathf.Abs(rect.localScale.x-rect.localScale.y)>.0001f)throw new InvalidOperationException("Nonuniform scale "+rect.name);
  foreach(var button in game.GetComponentsInChildren<Button>()){
   ((RectTransform)button.transform).GetWorldCorners(corners);var low=cam.WorldToScreenPoint(corners[0]);var high=cam.WorldToScreenPoint(corners[2]);
   if(low.x<-.6f||low.y<-.6f||high.x>size.x+.6f||high.y>size.y+.6f)checks.Add("OUTSIDE "+name+" "+size+" "+button.name+" "+low+" "+high);
   if(high.x-low.x<43||high.y-low.y<43)checks.Add("SMALL "+name+" "+size+" "+button.name+" "+(high-low));
  }
  if(game.CurrentScreen==GameScreen.Playing&&floors.rect.height<=0)checks.Add("NO FLOOR SPACE "+name+" "+size);
 });}
}
}
