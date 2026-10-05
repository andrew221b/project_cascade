using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
 public static class CraftCapture
 {
  const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
  static void Field(CascadeGame g,string field,object value)=>typeof(CascadeGame).GetField(field,Flags).SetValue(g,value);
  public static void Run(){if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed||EditorApplication.isCompiling)throw new InvalidOperationException("Capture needs a successfully compiled running scene");UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Capture());}
  static IEnumerator Capture()
  {
   var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.ReducedMotion=true;
   g.SetUiLocale("en");
   foreach(string id in new[]{"C03","C13","C160"}){g.LoadNode(id);g.ShowGame();yield return PlayableCapture.CaptureSettled("Craft",id+"-phone.png",390,844);}
   g.LoadNode("C01");g.ShowGame();yield return PlayableCapture.CaptureSettled("Craft","game-wide.png",1440,900);
   yield return PlayableCapture.CaptureSettled("Craft","game-notch.png",390,844,x=>x.SetEditorSafeInsets(new Vector4(0,.045f,0,.07f)));g.SetEditorSafeInsets(Vector4.zero);
   yield return PlayableCapture.CaptureSettled("Craft","game-landscape-notch.png",844,390,x=>x.SetEditorSafeInsets(new Vector4(.065f,.05f,.065f,0)));g.SetEditorSafeInsets(Vector4.zero);
   g.ShowTravelBag();yield return PlayableCapture.CaptureSettled("Craft","bag-phone.png",390,844);yield return PlayableCapture.CaptureSettled("Craft","bag-wide.png",1440,900);
   g.ShowCollection();yield return PlayableCapture.CaptureSettled("Craft","collection-phone.png",390,844);
   g.ShowWorkshop();yield return PlayableCapture.CaptureSettled("Craft","workshop-phone.png",390,844);
   g.ShowReachChallenges();yield return PlayableCapture.CaptureSettled("Craft","reach-phone.png",390,844);
   g.LoadLevel(g.Catalog.BonusLevels[1].Definition);g.ShowGame();yield return PlayableCapture.CaptureSettled("Craft","reach-game.png",390,844);
   g.SetUiLocale("ru");g.ShowTravelBag();yield return PlayableCapture.CaptureSettled("Craft","bag-small-ru.png",320,693);
   g.ShowCollection();yield return PlayableCapture.CaptureSettled("Craft","collection-small-ru.png",320,693);
   g.SetUiLocale("uk");g.Catalog.TryGetLocalizedBinding("C37","uk",out var words);g.LoadLevel(words.Definition,"uk");g.ShowGame();yield return PlayableCapture.CaptureSettled("Craft","words-small-uk.png",320,693);
   g.SetUiLocale("en");g.ShowJourney();yield return PlayableCapture.CaptureSettled("Craft","map-phone.png",390,844);yield return PlayableCapture.CaptureSettled("Craft","map-tablet.png",1024,1366);
   Field(g,"mapPage",13);Field(g,"selectedJourneyStop",64);typeof(CascadeGame).GetMethod("DrawMenu",Flags).Invoke(g,null);yield return PlayableCapture.CaptureSettled("Craft","map-lagoon.png",390,844);
   g.ReducedMotion=false;g.LoadNode("C03");g.ShowGame();g.SendMessage("OnApplicationFocus",true);
   File.WriteAllText(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/Craft/capture-complete.txt")),"18 native captures; notched layouts are simulated safe insets. No gameplay commands from captures.");
   yield return Motion(g);
   Debug.Log("CASCADE Craft captures complete");
  }
  static IEnumerator Motion(CascadeGame g)
  {
   var camera=Camera.main;var previous=camera.targetTexture;var active=RenderTexture.active;
   var rt=new RenderTexture(390,844,24);var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
   var scaler=g.GetComponentInChildren<CanvasScaler>();bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
   string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/Craft/Motion"));Directory.CreateDirectory(folder);
   try{
    scaler.enabled=false;camera.targetTexture=rt;scaler.enabled=true;yield return null;g.RefreshLayout();yield return null;g.RefreshLayout();
    g.Interaction.SetSuspended(false);g.ReducedMotion=false;g.Interaction.Execute(Cascade.Domain.PuzzleCommand.Swap(0));
    var rows=new System.Text.StringBuilder("frame,event,pose,x,y,logical_hash\n");
    for(int frame=0;frame<20;frame++){
     yield return new WaitForEndOfFrame();camera.Render();RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();
     File.WriteAllBytes(Path.Combine(folder,frame.ToString("00")+".png"),pixels.EncodeToPNG());
     var point=g.Machine.TokenVisualPosition(0);rows.AppendLine(frame+","+g.GetComponent<PuzzleEventPlayer>().CurrentEvent+","+g.Machine.TokenPose(0)+","+point.x+","+point.y+","+g.Interaction.Attempt.CurrentStateHash);
     yield return null;
    }
    File.WriteAllText(Path.Combine(folder,"events.csv"),rows.ToString());
   }finally{scaler.enabled=false;camera.targetTexture=previous;scaler.enabled=true;RenderTexture.active=active;g.RefreshLayout();UnityEngine.Application.runInBackground=background;UnityEngine.Object.DestroyImmediate(pixels);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
  }

 }
}
