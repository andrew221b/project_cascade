using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Cascade.Application;
using Cascade.Persistence;
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

 // A non-persistent contact sheet source for reviewing every first-use demonstration.
 public static class LessonFilmVisualReview
 {
  const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
  const string Phase="LessonFilms-Oct07";
  static readonly float[] Times={1.55f,3.15f,5.25f};
  [MenuItem("CASCADE/QA/Inspect 26 first-use films")]
  public static void Run()
  {
   if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorUtility.scriptCompilationFailed)
    throw new InvalidOperationException("Start the compiled playable in Play Mode first.");
   var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
   if(game==null)throw new InvalidOperationException("CASCADE playable is not open.");
   game.StartCoroutine(Capture(game));
  }
  static void Call(CascadeGame game,string name,params object[] args)
   =>typeof(CascadeGame).GetMethod(name,Private).Invoke(game,args);
  static IEnumerator Capture(CascadeGame game)
  {
   var ready=typeof(CascadeGame).GetField("persistenceReady",Private);
   bool wasReady=(bool)ready.GetValue(game);ready.SetValue(game,false);
   var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
   var seen=new System.Collections.Generic.List<MechanicLesson>(profile.SeenMechanics);
   string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase));
   Directory.CreateDirectory(folder);
   try{
    game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
    game.SetUiLocale("en");game.LoadNode("C01");game.ShowGame();
    foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))
     if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
    yield return null;
    foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson))){
     Call(game,"CloseMechanicPopup");
     Call(game,"OpenMechanicPopup",lesson,null,default(UiCopy),null);
     // The modal itself fades in for .22 s. Sampling during that fade made the
     // underlying HUD appear to overlap otherwise opaque lesson artwork.
     yield return new WaitForSecondsRealtime(.42f);
     var film=UnityEngine.Object.FindFirstObjectByType<LessonFilm>();
     if(film==null)throw new InvalidOperationException("No film for "+lesson);
     film.enabled=false;
     var elapsed=typeof(LessonFilm).GetField("elapsed",Private);
     var paint=typeof(LessonFilm).GetMethod("Paint",Private);
     for(int i=0;i<Times.Length;i++){
      float pose=Times[i];
      yield return PlayableCapture.CaptureSettled(Phase,$"{(int)lesson:00}-{lesson}-{i}.png",390,844,_=>{
       elapsed.SetValue(film,pose);paint.Invoke(film,null);
      });
     }
     if(lesson==MechanicLesson.Swap){
      typeof(LessonFilm).GetField("<CycleCount>k__BackingField",Private).SetValue(film,1);
      for(int i=0;i<Times.Length;i++){
       float pose=Times[i];
       yield return PlayableCapture.CaptureSettled(Phase,$"20-Swap-tap-{i}.png",390,844,_=>{
        elapsed.SetValue(film,pose);paint.Invoke(film,null);
       });
      }
     }
     File.WriteAllText(Path.Combine(folder,"status.txt"),"Captured "+lesson+" ("+((int)lesson+1)+"/26)\n");
    }
    File.WriteAllText(Path.Combine(folder,"status.txt"),"Captured 26 films after modal arrival at 1.55, 3.15 and 5.25 seconds; both swap gestures. No gameplay command was executed.\n");
   }finally{
    Call(game,"CloseMechanicPopup");
    profile.SeenMechanics.Clear();profile.SeenMechanics.AddRange(seen);
    ready.SetValue(game,wasReady);
   }
  }
 }
}
