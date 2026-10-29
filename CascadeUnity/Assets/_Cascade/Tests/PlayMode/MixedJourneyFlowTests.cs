#if UNITY_EDITOR
using System;using System.Collections;using System.Collections.Generic;using System.IO;using System.Linq;using System.Reflection;
using Cascade.Application;using Cascade.Content;using Cascade.Domain;using Cascade.Persistence;using Cascade.Presentation;
using NUnit.Framework;using TMPro;using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.TestTools;using UnityEngine.UI;
namespace Cascade.Tests
{
 public sealed class MixedJourneyFlowTests
 {
  const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
  CascadeGame game;string directory;Camera camera;RenderTexture target,oldTarget;bool oldBackground,oldAudio;
  object Field(string name)=>typeof(CascadeGame).GetField(name,Flags).GetValue(game);
  void Call(string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Flags).Invoke(game,args);
  PlayerProfile Profile=>(PlayerProfile)Field("profile");
  [UnitySetUp]public IEnumerator Setup(){
   oldBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;oldAudio=AudioListener.pause;AudioListener.pause=true;
   directory=Path.Combine(Path.GetTempPath(),"CascadeMixedJourney-"+Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
   var p=new PlayerProfile{AccountIntroSeen=true,FirstGestureShown=true,HeardStory=Enumerable.Range(0,10000).ToList(),Variety=new StageVarietyMemory{Seed="0123456789abcdef0123456789abcdef"}};p.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();p.Preferences.Tutorials=false;p.Preferences.FollowSystemLanguage=false;p.Missions.NextAfterClears=20000;
   new ProfileStore(Path.Combine(directory,"cascade-profile.json")).Save(p);
   yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
   foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
   yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;Resize(390,844);Configure();
   float end=Time.realtimeSinceStartup+30;while(Field("stageVariety")==null&&Time.realtimeSinceStartup<end)yield return null;Assert.That(Field("stageVariety"),Is.Not.Null);
  }
  void Configure(){game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Preferences.Tutorials=false;game.Coach.Enabled=false;game.ReducedMotion=true;game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));game.RefreshLayout();}
  void Resize(int width,int height){var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}target=new RenderTexture(width,height,24);camera.targetTexture=target;scaler.enabled=true;game.RefreshLayout();}
  [UnityTearDown]public IEnumerator Cleanup(){if(camera!=null)camera.targetTexture=oldTarget;if(game!=null)UnityEngine.Object.Destroy(game.gameObject);yield return null;if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=oldBackground;AudioListener.pause=oldAudio;if(Directory.Exists(directory))Directory.Delete(directory,true);}
  void DismissStory(){for(int i=0;i<8&&(game.CurrentScreen==GameScreen.Prologue||game.ConversationOpen);i++){if(game.CurrentScreen==GameScreen.Prologue)game.FinishWorldComic();if(game.ConversationOpen)game.SkipConversation();}}
  IEnumerator Ready(){float end=Time.realtimeSinceStartup+30;while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<end){if(game.MechanicPopupOpen)game.AcknowledgeMechanic();DismissStory();yield return null;}Assert.That(game.IsPlanningDescent,Is.False);Assert.That(game.Interaction.CanPlay,Is.True);Assert.That(game.Descent?.Version,Is.EqualTo(7));}
  IEnumerator Start(PuzzleMode mode,string locale,string node="C01"){
   game.Preferences.PuzzleLocale=locale;game.SetUiLocale(locale);game.ShowJourney();DismissStory();game.SelectMode(mode);DismissStory();yield return null;
   Assert.That(game.PreferredMode,Is.EqualTo(mode));Profile.Missions.NextAfterClears=20000;Profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();game.Coach.Enabled=false;
   game.StartCampaignLevel(game.Catalog.GetPrimary(node).Definition,locale);game.ShowGame();yield return Ready();
  }
  static PuzzleCommand Parse(string value)=>value=="D"?PuzzleCommand.Drop:value.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(value.Substring(1).Split(':')[0]),int.Parse(value.Split(':')[1])):PuzzleCommand.Swap(int.Parse(value));
  IEnumerator WinStage(){
   var stage=game.Descent.Stages[game.Descent.StageIndex];foreach(var command in stage.Witness.Select(Parse)){Assert.That(game.Interaction.Execute(command).Accepted,Is.True,command.ToString());float end=Time.realtimeSinceStartup+20;while(game.Interaction.IsPresenting&&Time.realtimeSinceStartup<end)yield return null;}
   float deadline=Time.realtimeSinceStartup+20;while(game.CurrentScreen!=GameScreen.Checkpoint&&Time.realtimeSinceStartup<deadline)yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Checkpoint));
  }
  IEnumerator Shot(string filename){
   yield return null;yield return null;game.RefreshLayout();Canvas.ForceUpdateCanvases();
   var heading=(TextMeshProUGUI)Field("levelLabel");heading.ForceMeshUpdate();var ink=CenteredContentRow.GlyphBounds(heading);var box=heading.rectTransform.rect;
   Assert.That(ink.min.x,Is.GreaterThanOrEqualTo(box.xMin-1),filename+" stage label left fit");Assert.That(ink.max.x,Is.LessThanOrEqualTo(box.xMax+1),filename+" stage label right fit");
   Assert.That(ink.min.y,Is.GreaterThanOrEqualTo(box.yMin-1),filename+" stage label bottom fit");Assert.That(ink.max.y,Is.LessThanOrEqualTo(box.yMax+1),filename+" stage label top fit");
   var capture=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable.PlayableCapture")).First(t=>t!=null);
   yield return (IEnumerator)capture.GetMethod("CaptureSettled").Invoke(null,new object[]{"StageVarietyOct29",filename+".png",target.width,target.height,null});
  }
  [UnityTest,Timeout(120000)]public IEnumerator ActualMapTravelStartsTheSelectedCategoryOnTheFirstRound(){
   game.Preferences.PuzzleLocale="uk";game.SetUiLocale("uk");
   foreach(var mode in new[]{PuzzleMode.Words,PuzzleMode.Mixed,PuzzleMode.Shapes}){
    game.ShowJourney();DismissStory();game.SelectMode(mode);DismissStory();yield return null;
    Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(Profile.JourneyAttempt,Is.False,"An unstarted category cannot resume its source placeholder");
    game.GetComponentsInChildren<Button>().Single(b=>b.name=="Travel").onClick.Invoke();yield return Ready();
    Assert.That(game.Interaction.Attempt.Mode,Is.EqualTo(mode));Assert.That(game.Interaction.Attempt.Level.TargetSymbolIds.Count,Is.EqualTo(mode==PuzzleMode.Mixed?4:3));
    if(mode!=PuzzleMode.Shapes)Assert.That(game.Interaction.Attempt.Level.TargetSymbolIds.Count(s=>s.Kind==SymbolKind.Glyph),Is.EqualTo(3));
   }
  }
  [UnityTest,Timeout(240000)]public IEnumerator FirstMixedRoundUsesANewWordAndShapeOnEachStageInEveryLanguage(){
   foreach(string locale in new[]{"en","ru","uk"}){
    yield return Start(PuzzleMode.Mixed,locale);var words=new HashSet<string>();var shapes=new HashSet<string>();
    for(int stage=0;stage<3;stage++){
     var level=game.Interaction.Attempt.Level;Assert.That(game.Interaction.Attempt.Mode,Is.EqualTo(PuzzleMode.Mixed));Assert.That(level.TargetSymbolIds.Count,Is.EqualTo(4));Assert.That(level.TargetSegments.Single(s=>s.Kind==SegmentKind.Word).Length,Is.EqualTo(3));
     Assert.That(words.Add(game.Descent.Variation.Symbols[stage].Word),Is.True);Assert.That(shapes.Add(string.Join(",",level.TargetSymbolIds.Where(s=>s.Kind==SymbolKind.Shape))),Is.True);
     yield return Shot(locale+"-mixed-level1-stage"+(stage+1));
     if(stage<2){yield return WinStage();game.GoLower();yield return Ready();Assert.That(game.Descent.StageIndex,Is.EqualTo(stage+1));}
    }
   }
  }
  [UnityTest,Timeout(180000)]public IEnumerator EightSlotWordsAndMixedFitCompactPhonesAndFontsCoverEveryAlphabet(){
   Resize(320,568);
   foreach(string locale in new[]{"en","ru","uk"})foreach(var mode in new[]{PuzzleMode.Words,PuzzleMode.Mixed}){
    yield return Start(mode,locale,"C32");Assert.That(game.Interaction.Attempt.Level.TargetSymbolIds.Count,Is.EqualTo(8));Assert.That(game.Interaction.Attempt.Mode,Is.EqualTo(mode));
    var font=game.GetComponentsInChildren<TextMeshProUGUI>().First(t=>t.GetComponent<SymbolGlyphCenter>()!=null).font;
    string alphabet=locale=="en"?"ABCDEFGHIJKLMNOPQRSTUVWXYZ":locale=="ru"?"АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ":"АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ";
    Assert.That(font.HasCharacters(alphabet,out uint[] missing,true,true),Is.True,locale+" glyph coverage: "+string.Join(",",missing??Array.Empty<uint>()));
    yield return Shot(locale+"-"+mode.ToString().ToLowerInvariant()+"-8slots-320");
   }
  }
  [UnityTest,Timeout(120000)]public IEnumerator ModeSwitchKeepsPinnedStagesAndColdStartKeepsUsedWords(){
   yield return Start(PuzzleMode.Mixed,"uk");yield return WinStage();game.GoLower();yield return Ready();string firstHash=game.Descent.StageHash(0),before=game.Descent.Variation.Symbols[1].WordId;
   game.Retry();yield return Ready();Assert.That(game.Descent.StageHash(0),Is.EqualTo(firstHash));Assert.That(game.Descent.Variation.Symbols[1].WordId,Is.Not.EqualTo(before));
   string hash=game.Interaction.Attempt.CurrentStateHash;var oldWords=game.Descent.Variation.Symbols.Select(s=>s.WordId).ToArray();
   game.ShowJourney();DismissStory();game.SelectMode(PuzzleMode.Words);DismissStory();yield return null;game.ShowJourney();DismissStory();game.SelectMode(PuzzleMode.Mixed);DismissStory();yield return null;
   Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.Descent.StageIndex,Is.EqualTo(1));
   game.SendMessage("OnApplicationPause",true);yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;camera.targetTexture=target;Configure();
   float end=Time.realtimeSinceStartup+30;while(Field("stageVariety")==null&&Time.realtimeSinceStartup<end)yield return null;
   Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));yield return Start(PuzzleMode.Mixed,"uk");Assert.That(game.Descent.Variation.Symbols.Select(s=>s.WordId).Intersect(oldWords),Is.Empty);
  }
 }
}
#endif
