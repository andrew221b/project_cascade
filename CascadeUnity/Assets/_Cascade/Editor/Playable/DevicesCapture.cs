using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
namespace Cascade.Editor.Playable
{
 public static class DevicesCapture
 {
  const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
  static void Field(CascadeGame g,string name,object value)=>typeof(CascadeGame).GetField(name,Flags).SetValue(g,value);
  public static void Run(){if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required");UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Capture());}
  public static void Preview(){var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.SetUiLocale("en");g.LoadLevel(g.Catalog.BonusLevels[6].Definition);g.ShowGame();g.StartCoroutine(PlayableCapture.CaptureSettled("Devices","pickup-preview.png",390,844,x=>x.PreviewDrag(0,1)));}
  public static void SharedContact(){var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.ReducedMotion=true;g.SetUiLocale("uk");g.Catalog.TryGetLocalizedBinding("C38","uk",out var words);g.LoadLevel(words.Definition,"uk");g.ShowGame();g.StartCoroutine(PlayableCapture.CaptureSettled("Devices","C38-shared-small-uk.png",320,693));}
  static IEnumerator Capture(){
   var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.ReducedMotion=true;g.SetUiLocale("en");
   foreach(string id in new[]{"C13","C160","C25"}){g.LoadNode(id);g.ShowGame();yield return PlayableCapture.CaptureSettled("Devices",id+"-phone.png",390,844);}
   g.ShowDevices();Field(g,"deviceIndex",2);typeof(CascadeGame).GetMethod("DrawMenu",Flags).Invoke(g,null);yield return PlayableCapture.CaptureSettled("Devices","bomb-guide.png",390,844);
   g.LoadLevel(g.Catalog.BonusLevels[9].Definition);g.ShowGame();yield return PlayableCapture.CaptureSettled("Devices","hourglass-before.png",390,844);
   g.SendMessage("OnApplicationFocus",true);g.Interaction.SetSuspended(false);g.Interaction.Execute(Cascade.Domain.PuzzleCommand.Swap(0));
   for(int wait=0;wait<120&&g.Interaction.IsPresenting;wait++)yield return null;
   yield return PlayableCapture.CaptureSettled("Devices","hourglass-after.png",390,844);
   g.LoadLevel(g.Catalog.BonusLevels[6].Definition);g.ShowGame();yield return PlayableCapture.CaptureSettled("Devices","ribbon-before.png",390,844);
   g.ShowPowerupChallenges();yield return PlayableCapture.CaptureSettled("Devices","powerup-lessons.png",390,844);
   g.SetUiLocale("ru");g.LoadNode("C160");g.ShowGame();yield return PlayableCapture.CaptureSettled("Devices","C160-small-ru.png",320,693);
   g.SetUiLocale("en");g.ShowJourney();Field(g,"mapPage",0);Field(g,"selectedJourneyStop",0);typeof(CascadeGame).GetMethod("DrawMenu",Flags).Invoke(g,null);
   yield return PlayableCapture.CaptureSettled("Devices","map-phone.png",390,844);yield return PlayableCapture.CaptureSettled("Devices","map-wide.png",1440,900);
   Field(g,"mapPage",13);Field(g,"selectedJourneyStop",64);typeof(CascadeGame).GetMethod("DrawMenu",Flags).Invoke(g,null);yield return PlayableCapture.CaptureSettled("Devices","map-lagoon.png",390,844);
   File.WriteAllText(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/Devices/capture-complete.txt")),"12 native captures. Gameplay executed only in the hourglass transition, outside the state-invariant capture.");
   g.ReducedMotion=false;g.LoadNode("C13");g.ShowGame();Debug.Log("CASCADE Devices captures complete");
  }
 }
}
