#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests {
 public sealed partial class AccountReferenceTests {
  const string ReportedMotionReport="ReportedMotionOct17";
  string ReportedMotionFolder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports",ReportedMotionReport));
  [Test] public void ReportedWelcomeMotionSurvivesUnevenPhoneFrames() {
   var motion=new WelcomeMikaMotion(197);float maxStep=0,maxStretch=0,maxDisplacement=0;var tail=new Vector2(140,850);var previous=tail;var scarf=new Vector2(143,509);var scarfBinding=WelcomeSecondaryMotion.BindScarf(scarf);var previousScarf=scarf;float maxScarfStep=0,maxScarfAge=0,maxScarfDt=0;
   var timing=new[]{1f/60+.00003f,1f/60-.00002f,1f/90+.00002f,1f/30+.00004f,1f/120+.00003f,.05f,.0167f,.00835f};
   for(int i=0;i<7200;i++){
    motion.Advance(timing[i%timing.Length],Mathf.Sin(motion.Age*.8f),Vector2.zero,false);
    var cloth=motion.ScarfPoint(scarf,scarfBinding);if(i>0&&Vector2.Distance(cloth,previousScarf)>maxScarfStep){maxScarfStep=Vector2.Distance(cloth,previousScarf);maxScarfAge=motion.Age;maxScarfDt=timing[i%timing.Length];}previousScarf=cloth;var next=motion.Deform(tail);maxStep=Mathf.Max(maxStep,Vector2.Distance(previous,next));previous=next;maxStretch=Mathf.Max(maxStretch,motion.MaxSecondaryStretch);maxDisplacement=Mathf.Max(maxDisplacement,Vector2.Distance(next,tail));
   }
   Directory.CreateDirectory(ReportedMotionFolder);File.WriteAllText(Path.Combine(ReportedMotionFolder,"uneven-frames.txt"),$"max tail step={maxStep}\nmax tail displacement={maxDisplacement}\nmax link stretch={maxStretch}\nmax scarf step={maxScarfStep}\nmax scarf age={maxScarfAge}\nmax scarf frame duration={maxScarfDt}\n");
   Assert.That(float.IsNaN(maxStep)||float.IsInfinity(maxStep)||float.IsNaN(maxStretch),Is.False,"Constraint positions must remain finite");Assert.That(maxStep,Is.LessThan(12),"Uneven phone frame durations cannot whip the tail between frames");Assert.That(float.IsNaN(maxScarfStep)||float.IsInfinity(maxScarfStep),Is.False);Assert.That(maxScarfStep,Is.LessThan(22));Assert.That(maxDisplacement,Is.LessThan(150));Assert.That(maxStretch,Is.LessThan(.005f));
  }
  [Test] public void ReportedDialogueSpeechUsesASlowRepeatingPhrase() {
   string text="Откроем старые ворота и проверим фонарь.";float peak=0;int opens=0;bool wasOpen=false;
   for(int i=0;i<30*120;i++){
    float amount=CharacterPoseView.SpeechShape(text,i/120f);bool open=amount>.72f;if(open&&!wasOpen)opens++;wasOpen=open;if(i>12*120)peak=Mathf.Max(peak,amount);
   }
   Assert.That(peak,Is.GreaterThan(.92f),"Speech must continue looping while its dialogue page remains visible");Assert.That(opens,Is.InRange(15,65),"The mouth must use a calm cadence, with readable open/close phases");
  }
  [UnityTest] public IEnumerator ReportedAppleBiteKeepsEachAuthoredPoseIntact() {
   game.ShowHome();game.ReducedMotion=false;game.Preferences.EffectQuality=2;yield return null;
   var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(c=>c.enabled).ToArray();
   var oldTarget=camera.targetTexture;var target=new RenderTexture(480,640,24);target.Create();var pixels=new Texture2D(480,640,TextureFormat.RGB24,false);
   var host=new GameObject("Intact bite drawing verification",typeof(RectTransform),typeof(Canvas));host.layer=5;var canvas=host.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
   var obj=new GameObject("Whole apple pose",typeof(RectTransform),typeof(Image));obj.layer=5;obj.transform.SetParent(host.transform,false);var hero=obj.GetComponent<Image>();hero.sprite=Resources.Load<Sprite>("CascadeArt/PauseReferenceOct14/RestingMika");hero.preserveAspect=true;hero.rectTransform.anchorMin=hero.rectTransform.anchorMax=Vector2.one*.5f;hero.rectTransform.sizeDelta=new Vector2(450,600);var view=CharacterPoseView.Attach(game,hero);view.ResetReview();
   Color32[] Shot(float frame){hero.material.SetFloat("_AppleFrame",frame);Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,480,640),0,0);pixels.Apply();RenderTexture.active=old;return pixels.GetPixels32();}
   try{
    foreach(var c in canvases)c.enabled=false;camera.targetTexture=target;yield return null;view.SetReviewFace(0,0);Directory.CreateDirectory(ReportedMotionFolder);float largest=0,drawnDifference=0;Color32[] previous=null;
    for(int frame=0;frame<12;frame++){
     var exact=Shot(frame);var held=Shot(Mathf.Min(11,frame+.35f));float error=0;
     for(int i=0;i<exact.Length;i++){error+=Mathf.Abs(exact[i].r-held[i].r)+Mathf.Abs(exact[i].g-held[i].g)+Mathf.Abs(exact[i].b-held[i].b);if(previous!=null)drawnDifference+=Mathf.Abs(exact[i].r-previous[i].r)+Mathf.Abs(exact[i].g-previous[i].g)+Mathf.Abs(exact[i].b-previous[i].b);}
     largest=Mathf.Max(largest,error/(exact.Length*3));previous=exact;
     if(frame==0||frame==3||frame==5||frame==6||frame==11)File.WriteAllBytes(Path.Combine(ReportedMotionFolder,"intact-bite-"+frame.ToString("D2")+".png"),pixels.EncodeToPNG());
    }
    File.WriteAllText(Path.Combine(ReportedMotionFolder,"bite-pixel-check.txt"),$"Maximum held drawing RGB difference: {largest} / 255\nFull-sequence drawing difference: {drawnDifference}\n");
    Assert.That(largest,Is.LessThan(.05f),"No warped or doubled intermediate paws, apple, sleeve or muzzle");Assert.That(drawnDifference,Is.GreaterThan(100000),"The renderer must actually advance through different complete poses");
   }finally{camera.targetTexture=oldTarget;foreach(var c in canvases)if(c!=null)c.enabled=true;UnityEngine.Object.DestroyImmediate(host);target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);}
  }
  [UnityTest] public IEnumerator ReportedCaptureWelcomeWithPhoneFrameJitter() {
   WelcomePhone();game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.ShowAccountGate();yield return new WaitForSecondsRealtime(.5f);
   var view=game.GetComponentInChildren<WelcomeMikaView>();view.ResetReview();var timing=new[]{.0167f,.00835f,.03336f,.01113f};
   for(int i=0;i<600;i++){view.AdvanceReview(timing[i%4],.55f,Vector2.zero);if(i%60==59){Canvas.ForceUpdateCanvases();camera.Render();Directory.CreateDirectory(ReportedMotionFolder);WindShot("reported-jitter-"+i.ToString("D4"));}if(i%60==0)yield return null;}
  }
 }
}
#endif
