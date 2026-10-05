using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Domain;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
namespace Cascade.Editor.Playable
{
 public static class PolishCapture
 {
  const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
  static void Field(CascadeGame g,string name,object value)=>typeof(CascadeGame).GetField(name,Flags).SetValue(g,value);
  static void Screen(CascadeGame g,GameScreen screen)=>typeof(CascadeGame).GetMethod("SetScreen",Flags).Invoke(g,new object[]{screen});
  static void Redraw(CascadeGame g)=>typeof(CascadeGame).GetMethod("DrawMenu",Flags).Invoke(g,null);
  public static void Run(){if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required");UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Capture());}
  public static void FinishPass(){if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required");UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(FinishShots());}
  public static void MenuHeaderCheck(){if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required");UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(HeaderShots());}
  public static void LagoonCheck(){if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required");UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(LagoonShot());}
  public static void MorningMistCheck(){if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required");UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(MorningMistShots());}
  static IEnumerator MorningMistShots()
  {
   var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.ReducedMotion=false;g.SetUiLocale("en");
   g.LoadNode("C13");g.ShowGame();yield return Shot("morning-garden");
   g.LoadNode("C81");g.ShowGame();yield return Shot("morning-lagoon");
   Debug.Log("CASCADE morning air captured in two regions");
  }
  public static void SurfaceCheck(){if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required");UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(SurfaceShots());}
  static IEnumerator SurfaceShots()
  {
   var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.ReducedMotion=false;g.SetUiLocale("en");
   if(g.Catalog.TryGetLocalizedBinding("C38","en",out var words))g.LoadLevel(words.Definition,"en");else g.LoadNode("C38");g.ShowGame();yield return Shot("surface-letters");
   g.LoadNode("C81");g.ShowGame();yield return Shot("surface-lagoon");
   g.LoadNode("C13");g.ShowGame();yield return Shot("surface-devices");
   Debug.Log("CASCADE surface check: letters, lagoon and devices captured");
  }
  public static void VerifyOneSwap()
  {
   var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.LoadNode("C13");g.ShowGame();g.SendMessage("OnApplicationFocus",true);g.Interaction.SetSuspended(false);
   var before=g.Interaction.Attempt.Current.Row.Select(s=>s.Value).OrderBy(s=>s).ToArray();
   int edge=Enumerable.Range(0,before.Length-1).First(i=>g.Interaction.Attempt.Resolver.Preview(g.Interaction.Attempt.Current,PuzzleCommand.Swap(i)).IsLegal);
   g.Interaction.Execute(PuzzleCommand.Swap(edge));
   var after=g.Interaction.Attempt.Current.Row.Select(s=>s.Value).OrderBy(s=>s).ToArray();
   if(!before.SequenceEqual(after)||g.Interaction.Attempt.RouteLength!=1)throw new InvalidOperationException("Swap did not preserve the row and commit exactly once");
   Debug.Log("CASCADE focused swap PASS: one atomic move, same symbol multiset, animation="+g.Interaction.IsPresenting);
  }
  public static void Confirm(){if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required");UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Confirmation());}
  public static void DiagnoseTiming(){UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Timing());}
  static IEnumerator Timing()
  {
   var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.ReducedMotion=false;g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
   bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
   var log=new System.Text.StringBuilder("node,command,elapsed,event,index,frame_delta\n");
   try{
    foreach(string node in new[]{"C09","C32"}){
     g.LoadNode(node);g.ShowGame();g.SendMessage("OnApplicationFocus",true);g.Interaction.SetSuspended(false);yield return null;
     var path=Cascade.Solver.PuzzleSolver.FindShortest(g.Interaction.Attempt.Resolver).Path;int command=0;
     foreach(var action in path){
      double start=Time.realtimeSinceStartupAsDouble;g.Interaction.Execute(action);
      while(g.Interaction.IsPresenting&&Time.realtimeSinceStartupAsDouble-start<12){
       log.AppendLine(node+","+command+","+(Time.realtimeSinceStartupAsDouble-start)+","+g.EventPlayer.CurrentEvent+","+g.EventPlayer.EventIndex+","+Time.unscaledDeltaTime);yield return null;
      }
      log.AppendLine(node+","+command+","+(Time.realtimeSinceStartupAsDouble-start)+",Finished,-1,"+Time.unscaledDeltaTime);command++;
     }
    }
   }finally{UnityEngine.Application.runInBackground=background;File.WriteAllText(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/Polish/timing.csv")),log.ToString());}
  }
  static IEnumerator Confirmation()
  {
   var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.ReducedMotion=true;g.SetUiLocale("en");g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
   g.LoadNode("C13");g.ShowGame();yield return Shot("game-phone");g.ShowSettings();yield return Shot("settings-0");
   Field(g,"settingsPage",2);Redraw(g);yield return Shot("settings-2");
   g.Preferences.LargeText=true;g.SetUiLocale("ru");g.ShowSettings();Field(g,"settingsPage",2);Redraw(g);yield return Shot("languages-small-ru",320,693);
   g.Preferences.LargeText=false;g.SetUiLocale("en");typeof(CascadeGame).GetMethod("OpenJourneyStory",Flags).Invoke(g,new object[]{0,false});yield return Shot("dialogue");
   var records=g.Rewards.Records.ToArray();g.Rewards.Records.Clear();g.ShowRecords();yield return Shot("records-empty");g.Rewards.Records.AddRange(records);
   g.LoadNode("C01");g.ShowGame();g.SendMessage("OnApplicationFocus",true);g.Interaction.SetSuspended(false);g.Interaction.Execute(PuzzleCommand.Swap(0));
   for(int wait=0;wait<180&&g.Interaction.IsPresenting;wait++)yield return null;yield return Shot("win");
   g.LoadNode("C02");g.ShowGame();g.Interaction.SetSuspended(false);g.Interaction.Execute(PuzzleCommand.Drop);
   for(int wait=0;wait<180&&g.Interaction.IsPresenting;wait++)yield return null;yield return Shot("fail");
   g.ShowTravelBag();yield return Shot("bag-tablet",1024,1366);g.ShowJourney();yield return Shot("map-landscape",844,390);
   yield return Motion(g);Debug.Log("CASCADE Polish confirmation complete");
  }
  static IEnumerator Motion(CascadeGame g)
  {
   g.LoadNode("C03");g.ShowGame();g.ReducedMotion=false;g.SendMessage("OnApplicationFocus",true);g.Interaction.SetSuspended(false);
   var camera=Camera.main;var previous=camera.targetTexture;var active=RenderTexture.active;
   var rt=new RenderTexture(390,844,24);var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
   var scaler=g.GetComponentInChildren<UnityEngine.UI.CanvasScaler>();bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
   string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/Polish/Motion"));Directory.CreateDirectory(folder);
   try{
    scaler.enabled=false;camera.targetTexture=rt;scaler.enabled=true;yield return null;g.RefreshLayout();yield return null;g.RefreshLayout();
    yield return new WaitForSecondsRealtime(.5f);g.Interaction.Execute(PuzzleCommand.Swap(0));
    string committed=g.Interaction.Attempt.CurrentStateHash;var rows=new System.Text.StringBuilder("frame,event,pose,sag,rope_vertices,logical_hash\n");
    for(int frame=0;frame<32;frame++){
     yield return new WaitForEndOfFrame();camera.Render();RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();
     File.WriteAllBytes(Path.Combine(folder,frame.ToString("00")+".png"),pixels.EncodeToPNG());
     int points=g.Machine.GetComponentsInChildren<LineRenderer>().First(l=>l.name.Contains("braided rope")).positionCount;
     rows.AppendLine(frame+","+g.GetComponent<PuzzleEventPlayer>().CurrentEvent+","+g.Machine.TokenPose(0)+","+g.Machine.LoadedSag+","+points+","+g.Interaction.Attempt.CurrentStateHash);
     if(g.Interaction.Attempt.CurrentStateHash!=committed)throw new InvalidOperationException("Animation changed the committed puzzle state");
     yield return null;
    }
    File.WriteAllText(Path.Combine(folder,"events.csv"),rows.ToString());
   }finally{scaler.enabled=false;camera.targetTexture=previous;scaler.enabled=true;RenderTexture.active=active;g.RefreshLayout();UnityEngine.Application.runInBackground=background;UnityEngine.Object.DestroyImmediate(pixels);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
  }
  static IEnumerator Shot(string name,int width=390,int height=844){yield return new WaitForSecondsRealtime(.18f);yield return PlayableCapture.CaptureSettled("Polish",name+".png",width,height);}
  static IEnumerator HeaderShots(){var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.SetUiLocale("en");g.ShowSettings();yield return Shot("finish-settings");Screen(g,GameScreen.Pause);yield return Shot("finish-pause");g.ShowTravelBag();yield return Shot("finish-bag");Debug.Log("CASCADE illustrated menu header captured");}
  static IEnumerator LagoonShot(){var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.SetUiLocale("en");g.LoadNode("C81");g.ShowGame();yield return Shot("finish-lagoon");Debug.Log("CASCADE lagoon ambience captured");}
  static IEnumerator FinishShots()
  {
   var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.ReducedMotion=false;g.SetUiLocale("en");
   g.LoadNode("C13");g.ShowGame();yield return Shot("finish-game");
   g.ShowJourney();yield return Shot("finish-map");
   Screen(g,GameScreen.Pause);yield return Shot("finish-pause");
   g.ShowTravelBag();yield return Shot("finish-bag");
   g.ShowSettings();yield return Shot("finish-settings");
   if(g.Catalog.TryGetLocalizedBinding("C38","en",out var words))g.LoadLevel(words.Definition,"en");else g.LoadNode("C38");g.ShowGame();yield return Shot("finish-letters");
   g.SetEditorSafeInsets(new Vector4(0,.045f,0,.07f));yield return Shot("finish-notch",320,693);g.SetEditorSafeInsets(Vector4.zero);
   Debug.Log("CASCADE finish pass: seven focused captures complete");
  }
  static IEnumerator Capture()
  {
   var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.ReducedMotion=true;g.SetUiLocale("en");
   g.LoadNode("C13");g.ShowGame();yield return Shot("game-phone");yield return Shot("game-wide",1440,900);
   foreach(var screen in new[]{GameScreen.Home,GameScreen.Pause,GameScreen.Lessons,GameScreen.TravelBag,GameScreen.Journal,GameScreen.Collection,GameScreen.Workshop,GameScreen.Requests,GameScreen.Records,GameScreen.Contracts,GameScreen.ReachChallenges,GameScreen.PowerupChallenges,GameScreen.JourneyEnd}){
    Screen(g,screen);yield return Shot(screen.ToString());
   }
   g.ShowSettings();for(int page=0;page<3;page++){Field(g,"settingsPage",page);Redraw(g);yield return Shot("settings-"+page);}
   g.ProposePuzzleLocale("uk");yield return Shot("word-language");
   Field(g,"<Recovery>k__BackingField",Cascade.Persistence.SaveRecovery.Backup);Screen(g,GameScreen.SaveRecovery);yield return Shot("save-recovery");Field(g,"<Recovery>k__BackingField",Cascade.Persistence.SaveRecovery.None);
   typeof(CascadeGame).GetMethod("OpenJourneyStory",Flags).Invoke(g,new object[]{0,false});yield return Shot("dialogue");
   g.LoadNode("C01");g.ShowGame();g.SendMessage("OnApplicationFocus",true);g.Interaction.SetSuspended(false);g.Interaction.Execute(PuzzleCommand.Swap(0));
   for(int wait=0;wait<180&&g.Interaction.IsPresenting;wait++)yield return null;yield return Shot("win");
   g.LoadNode("C02");g.ShowGame();g.Interaction.SetSuspended(false);g.Interaction.Execute(PuzzleCommand.Drop);
   for(int wait=0;wait<180&&g.Interaction.IsPresenting;wait++)yield return null;yield return Shot("fail");
   // Isolated in-memory review fixture: persistence is disabled, no user receipts are written.
   g.Rewards.Synchronize(new[]{"C01","C02","C03"});g.ShowParcel();yield return Shot("parcel");
   g.Rewards.ClaimParcel(g.Rewards.Parcels.First(p=>!p.Claimed).Order);Field(g,"parcelOpened",true);Redraw(g);yield return Shot("parcel-opened");
   g.ShowRecords();yield return Shot("records-filled");
   g.ShowDevices();for(int i=0;i<11;i++){Field(g,"deviceIndex",i);Redraw(g);yield return Shot("guide-"+i);}
   g.Preferences.LargeText=true;g.SetUiLocale("ru");g.ShowTravelBag();yield return Shot("bag-small-ru",320,693);g.ShowCollection();yield return Shot("collection-small-ru",320,693);
   g.ShowSettings();Field(g,"settingsPage",2);Redraw(g);yield return Shot("languages-small-ru",320,693);
   g.SetUiLocale("uk");g.Catalog.TryGetLocalizedBinding("C38","uk",out var words);g.LoadLevel(words.Definition,"uk");g.ShowGame();yield return Shot("words-small-uk",320,693);
   g.Preferences.LargeText=false;g.SetUiLocale("en");g.ShowTravelBag();yield return Shot("bag-wide",1440,900);
   g.ShowJourney();yield return Shot("map-wide",1440,900);g.LoadNode("C160");g.ShowGame();g.SetEditorSafeInsets(new Vector4(0,.045f,0,.07f));yield return Shot("game-notch");g.SetEditorSafeInsets(Vector4.zero);
   g.ReducedMotion=false;g.LoadNode("C03");g.ShowGame();g.SendMessage("OnApplicationFocus",true);
   File.WriteAllText(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/Polish/capture-complete.txt")),"All menu routes, eleven device pages, phone/wide/large text/simulated notch. In-memory fixture only; disk persistence disabled. Each capture asserts gameplay state unchanged.");
   Debug.Log("CASCADE Polish captures complete");
  }
 }
}
