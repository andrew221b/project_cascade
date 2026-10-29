#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Presentation;
using Cascade.Solver;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests {
 public sealed partial class AccountReferenceTests {
  string FinishFrameFolder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/InventoryFinishFrameOct17"));
  void FinishFrameShot(string name){
   Directory.CreateDirectory(FinishFrameFolder);Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=portrait;
   var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(FinishFrameFolder,name+".png"),pixels.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(pixels);
  }
  [UnityTest] public IEnumerator InventoryHelpAndUndoExplanationsExistInEveryLanguage(){
   Directory.CreateDirectory(FinishFrameFolder);var failures=new System.Collections.Generic.List<string>();
   var explain=typeof(CascadeGame).GetMethod("MechanicExplanation",Private);
   foreach(string locale in new[]{"en","ru","uk"}){
    game.SetUiLocale(locale);yield return null;
    foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson))){
     var copy=(UiCopy)explain.Invoke(game,new object[]{lesson});string value=copy.ToString();
     if(string.IsNullOrWhiteSpace(value)||value.Contains("No translation"))failures.Add(locale+" / "+lesson+" / "+copy.Key);
    }
   }
   File.WriteAllLines(Path.Combine(FinishFrameFolder,"translation-audit.txt"),failures.Count>0?failures:new[]{"All mechanic explanations resolve in EN/RU/UK."}.ToList());
   Assert.That(failures,Is.Empty,string.Join("\n",failures));
  }
  [UnityTest] public IEnumerator InventoryFirstOpenShowsTranslatedHelpAndTray(){
   WelcomePhone();game.Preferences.Tutorials=false;game.ReducedMotion=true;Profile.AccountIntroSeen=true;Profile.Missions.NextAfterClears=20000;
   Profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().Where(l=>l!=MechanicLesson.Toolbag).ToList();
   game.LoadNode("C01");game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Playable bag fixture");game.Preferences.Tutorials=true;
   game.ToggleFieldToolTray();yield return null;Assert.That(game.CurrentMechanic,Is.EqualTo(MechanicLesson.Toolbag));
   yield return new WaitForSecondsRealtime(.15f);
   foreach(var caption in game.GetComponentsInChildren<TMP_Text>())Assert.That(caption.text.Contains("No translation"),Is.False,caption.name+": "+caption.text);
   FinishFrameShot("bag-first-help-ru");game.AcknowledgeMechanic();game.ToggleFieldToolTray();yield return null;
   Assert.That((bool)Field("fieldTrayOpen"),Is.True);Assert.That(game.MechanicPopupOpen,Is.False);
   foreach(string locale in new[]{"en","ru","uk"}){
    game.SetUiLocale(locale);typeof(CascadeGame).GetMethod("RefreshToolTray",Private).Invoke(game,null);yield return null;
    FinishFrameShot("bag-tray-"+locale);
    for(int tool=0;tool<ShopCatalog.ToolCount;tool++){
     typeof(CascadeGame).GetMethod("SelectFieldTool",Private).Invoke(game,new object[]{tool});yield return null;
     foreach(var caption in game.GetComponentsInChildren<TMP_Text>())Assert.That(caption.text.Contains("No translation"),Is.False,locale+" "+caption.name+": "+caption.text);
    }
   }
  }
  [UnityTest] public IEnumerator FrameWindKeepsTimberAndLanternStationary(){
   Directory.CreateDirectory(FinishFrameFolder);
   var texture=Resources.Load<Texture2D>("CascadeArt/HomeOct13/FrameForeground");Assert.That(texture,Is.Not.Null);
   var target=new RenderTexture(texture.width,texture.height,0,RenderTextureFormat.ARGB32);target.Create();
   var pixels=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);var previous=RenderTexture.active;
   try{
    foreach(string shader in new[]{"FrameSurface","FrameScene"}){
     var source=shader=="FrameScene"?WorldFrameArt.Texture(0):texture;
     var material=new Material(Resources.Load<Shader>(WorldFrameArt.Path+shader));
     try{
      material.SetTexture("_BaseMap",source);material.SetFloat("_World",0);material.SetFloat("_MapCornerWind",1);material.SetFloat("_CornerMotionAllowed",1);material.SetColor("_LightTint",Color.white);
      Color32[] Shot(float age,float wind,string name){
       material.SetFloat("_Age",age);material.SetFloat("_Wind",wind);RenderTexture.active=target;GL.Clear(true,true,Color.clear);Graphics.Blit(source,target,material);RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(FinishFrameFolder,name+".png"),pixels.EncodeToPNG());return pixels.GetPixels32();
      }
      Graphics.Blit(source,target);RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);pixels.Apply();var unwarped=pixels.GetPixels32();
      var a=Shot(.3f,.7f,shader+"-a");var b=Shot(3.2f,-.8f,shader+"-b");int timber=0,changed=0,foliageChanged=0;
      for(int y=(int)(texture.height*.82f);y<texture.height-4;y++)for(int x=4;x<texture.width-4;x++){
       int i=y*texture.width+x;var c=unwarped[i];int d=Mathf.Abs(a[i].r-b[i].r)+Mathf.Abs(a[i].g-b[i].g)+Mathf.Abs(a[i].b-b[i].b)+Mathf.Abs(a[i].a-b[i].a);
       if(c.a>250&&c.r>c.g*1.35f&&c.g>c.b*1.45f&&c.g>25){timber++;if(d>8)changed++;}
       if(c.a>230&&c.g>c.r*1.3f&&d>8)foliageChanged++;
      }
      File.WriteAllText(Path.Combine(FinishFrameFolder,shader+"-pixels.txt"),$"Timber pixels: {timber}\nMoving timber: {changed}\nMoving foliage: {foliageChanged}\n");
      Assert.That(timber,Is.GreaterThan(100),"The test must render the actual timber");Assert.That(changed,Is.LessThan(10),shader+": wind cannot move timber, rope supports or lamp");Assert.That(foliageChanged,Is.GreaterThan(20),"Wind still animates foliage");
     }finally{UnityEngine.Object.DestroyImmediate(material);}
    }
   }finally{RenderTexture.active=previous;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);}
   yield return null;
  }
  [UnityTest] public IEnumerator CheckpointLandingStartsAndEndsAtTheActualReceiverPose(){
   WelcomePhone();game.Preferences.Tutorials=false;game.ReducedMotion=false;game.StartDescentLevel(game.Catalog.GetPrimary("C01").Definition);game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Checkpoint landing fixture");
   var receiver=(RectTransform)Field("receiver");game.RefreshLayout();Canvas.ForceUpdateCanvases();
   var start=receiver.anchoredPosition;var size=receiver.rect.size;var hash=game.Interaction.Attempt.CurrentStateHash;
   var clip=game.Outcome.Play(true,true);
   try{
    Assert.That(clip.MoveNext(),Is.True);Assert.That(Vector2.Distance(receiver.anchoredPosition,start),Is.LessThan(.1f),"The first celebration frame cannot jump away from the actual landed receiver");
    Assert.That(Vector2.Distance(receiver.rect.size,size),Is.LessThan(.1f),"The board cannot shrink when its celebration starts");
    while(clip.MoveNext())yield return clip.Current;
    var settled=receiver.anchoredPosition;FinishFrameShot("checkpoint-landed");
    typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{GameScreen.Checkpoint});Canvas.ForceUpdateCanvases();
    Assert.That(Vector2.Distance(receiver.anchoredPosition,settled),Is.LessThan(.1f),"The checkpoint layout must meet the last animation frame without a snap");
    Assert.That(Vector2.Distance(receiver.rect.size,size),Is.LessThan(.1f));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
   }finally{(clip as IDisposable)?.Dispose();}
  }
  [UnityTest] public IEnumerator LevelFinishTransitionReleasesSnapshotOnInterruption(){
   WelcomePhone();game.Preferences.Tutorials=false;game.ReducedMotion=false;game.ShowHome();yield return null;
   var bridge=game.gameObject.AddComponent<LevelResultTransition>();typeof(CascadeGame).GetField("resultTransition",Private).SetValue(game,bridge);
   string hash=game.Interaction.Attempt.CurrentStateHash;int coins=game.ShopCoins,clears=Profile.AllClears.Count();
   var canvas=(RectTransform)game.GetComponentInChildren<Canvas>().transform;
   Assert.That(bridge.Capture(game,camera,canvas),Is.True);bridge.Reveal();yield return null;Assert.That(bridge.Opacity,Is.GreaterThan(.8f));
   bridge.SendMessage("OnApplicationFocus",false);Assert.That(bridge.Opacity,Is.Zero);
   Assert.That(bridge.Capture(game,camera,canvas),Is.True);bridge.Reveal();game.ShowTravelBag();yield return null;Assert.That(bridge.Opacity,Is.Zero,"Navigation never leaves a stale screen covering the new one");
   Assert.That(bridge.Capture(game,camera,canvas),Is.True);bridge.Reveal();game.ReducedMotion=true;yield return null;Assert.That(bridge.Opacity,Is.Zero);
   Assert.That(bridge.Capture(game,camera,canvas),Is.False,"Reduced Motion avoids the temporary full-screen allocation");
   Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.ShopCoins,Is.EqualTo(coins));Assert.That(Profile.AllClears.Count(),Is.EqualTo(clears));
   Assert.That(game.GetComponentsInChildren<RawImage>(true).Any(i=>i.name=="Landed level dissolve"),Is.False);
  }
  [UnityTest] public IEnumerator LevelFinishDissolvesWithoutChangingRewardsOrBlockingNavigation(){
   WelcomePhone();game.SetUiLocale("en");game.Preferences.Tutorials=false;game.ReducedMotion=false;game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;Profile.AccountIntroSeen=true;Profile.Missions.NextAfterClears=20000;Profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
   game.LoadNode("C01");game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"Animated final delivery fixture");
   foreach(var command in PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path){yield return FooterWait(()=>game.Interaction.CanPlay,"Exchange ready");game.Interaction.Execute(command);yield return FooterWait(()=>!game.Interaction.IsPresenting,"Exchange finishes");}
   yield return FooterWait(()=>game.CurrentScreen==GameScreen.Win,"Result opens");
   var bridge=game.GetComponents<MonoBehaviour>().SingleOrDefault(c=>c.GetType().Name=="LevelResultTransition");Assert.That(bridge,Is.Not.Null,"The landed frame must bridge to the result rather than disappear in one frame");
   float Opacity()=>(float)bridge.GetType().GetProperty("Opacity").GetValue(bridge);
   FinishFrameShot("finish-00");Assert.That(Opacity(),Is.GreaterThan(.92f));int clears=Profile.AllClears.Count(),coins=game.ShopCoins;float prior=Opacity(),elapsed=0,nextShot=.08f;int shot=1;
   while(elapsed<.85f){yield return null;float a=Opacity();Assert.That(a,Is.LessThanOrEqualTo(prior+.0001f));Assert.That(prior-a,Is.LessThan(.35f),"No abrupt opacity cut");prior=a;elapsed+=Time.unscaledDeltaTime;if(elapsed>=nextShot){FinishFrameShot("finish-"+(shot++).ToString("D2"));nextShot+=.08f;}}
   Assert.That(Opacity(),Is.Zero);Assert.That(Profile.AllClears.Count(),Is.EqualTo(clears));Assert.That(game.ShopCoins,Is.EqualTo(coins));
   game.ShowHome();yield return null;Assert.That(Opacity(),Is.Zero);Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
  }
 }
}
#endif
