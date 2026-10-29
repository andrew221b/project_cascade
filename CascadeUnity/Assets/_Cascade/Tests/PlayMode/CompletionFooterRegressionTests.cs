#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using Cascade.Solver;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests {
 public sealed partial class AccountReferenceTests {
  string CompletionFooterFolder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/CompletionFooterOct17"));
  IEnumerator FooterWait(Func<bool> ready,string message){float end=Time.realtimeSinceStartup+20;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Assert.That(ready(),Is.True,message);}
  IEnumerator WinFooterFixture(){
   WelcomePhone();var settings=Resources.Load<MonetizationSettings>("CascadeArt/MonetizationSettings");bool previous=settings.testMonetization;
   // Monetization now starts before Home so catalog loading can overlap sign-in.
   // Supply preview configuration before the scene initializes its services.
   try{settings.testMonetization=true;yield return Load();game.ShowHome();}finally{settings.testMonetization=previous;}
   game.SetUiLocale("en");game.ReducedMotion=true;game.Preferences.ReducedMotion=true;game.Preferences.Tutorials=false;game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
   Profile.AccountIntroSeen=true;Profile.Missions.NextAfterClears=20000;Profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
   game.LoadNode("C01");typeof(CascadeGame).GetField("journeyActive",Private).SetValue(game,true);game.ShowGame();yield return FooterWait(()=>game.Interaction.CanPlay,"First completion fixture is playable");
   foreach(var command in PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path){yield return FooterWait(()=>game.Interaction.CanPlay,"Exchange input ready");Assert.That(game.Interaction.Execute(command).Accepted,Is.True);yield return FooterWait(()=>!game.Interaction.IsPresenting,"Exchange settles");}
   yield return FooterWait(()=>game.CurrentScreen==GameScreen.Win,"First level result opens");yield return null;
  }
  void FooterShot(string name){
   Directory.CreateDirectory(CompletionFooterFolder);Canvas.ForceUpdateCanvases();camera.Render();var before=RenderTexture.active;RenderTexture.active=portrait;var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(CompletionFooterFolder,name+".png"),pixels.EncodeToPNG());RenderTexture.active=before;UnityEngine.Object.DestroyImmediate(pixels);
  }
  Rect FooterScreenRect(RectTransform rect){var corners=new Vector3[4];rect.GetWorldCorners(corners);var a=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);var b=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);return Rect.MinMaxRect(a.x,a.y,b.x,b.y);}
  [UnityTest] public IEnumerator CompletionFooterFirstVictoryHasRetryNextAndMap(){
   yield return WinFooterFixture();FooterShot("first-victory-en");
   Assert.That(Control("Continue completion").GetComponentInChildren<TMP_Text>().text,Is.EqualTo("Next level"),"The first victory must retain the same next-level action as later ones");
   Assert.That(Control("Retry completed level").interactable,Is.True);Assert.That(Control("Completion map").interactable,Is.True);
  }
  [UnityTest] public IEnumerator CompletionFooterActionsFitInsideTimberAtPhoneSizesAndLocales(){
   yield return WinFooterFixture();var other=game.Catalog.GetPrimary("C02").Definition;
   Profile.Clears.Add(new ClearReceipt{CampaignNodeId="C02",ContentHash=other.ContentHash,AttemptId=Guid.NewGuid().ToString("D"),BestStars=3});
   foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(430,932)})foreach(string locale in new[]{"en","ru","uk"}){
    camera.aspect=size.x/(float)size.y;camera.ResetProjectionMatrix();WelcomePhone(size.x,size.y);game.SetUiLocale(locale);game.Preferences.LargeText=true;game.ShowGame();yield return null;game.RefreshLayout();yield return null;Canvas.ForceUpdateCanvases();
    var selectedTable=UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));yield return selectedTable;Assert.That(selectedTable.Status,Is.EqualTo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded));
    yield return FooterWait(()=>Control("Continue completion").GetComponentInChildren<TMP_Text>().text==(size.x<360?(locale=="ru"?"Дальше":locale=="uk"?"Далі":"Next"):(locale=="ru"?"Следующий уровень":locale=="uk"?"Наступний рівень":"Next level")),"Capture the actual selected language after its asynchronous caption update");
    // The view binds after the new locale has constructed its control content.
    yield return null;yield return null;game.RefreshLayout();
    foreach(var row in game.GetComponentsInChildren<CenteredContentRow>())row.Refresh();Canvas.ForceUpdateCanvases();
    FooterShot("footer-"+locale+"-"+size.x);var ordered=new[]{Control("Retry completed level"),Control("Continue completion"),Control("Completion map")};
    for(int i=0;i<ordered.Length;i++){
     var button=ordered[i];var rect=FooterScreenRect((RectTransform)button.transform);float logicalScale=1;
     Assert.That(rect.xMin,Is.GreaterThanOrEqualTo(size.x*.09f-.05f),button.name+": its full hit area must clear the painted side timber/foliage");Assert.That(rect.xMax,Is.LessThanOrEqualTo(size.x*.91f+.05f),button.name+": its right rim must remain inside the frame");
     var design=button.GetComponent<GameButtonDesign>();string diagnostic=button.name+" "+size+" / rect "+rect+" / local "+((RectTransform)button.transform).rect+" / unit "+CenteredContentRow.Unit((RectTransform)button.transform)+" / canvas "+button.GetComponentInParent<Canvas>().rootCanvas.scaleFactor+" / preset "+design?.Preset?.name+" / desired "+design?.DesiredSize(((RectTransform)button.transform).rect.size);
     Assert.That(rect.width,Is.GreaterThanOrEqualTo(44*logicalScale-.05f),diagnostic);Assert.That(rect.height,Is.GreaterThanOrEqualTo(44*logicalScale-.05f),diagnostic);for(int j=0;j<i;j++){
      var otherRect=FooterScreenRect((RectTransform)ordered[j].transform);float gapX=Mathf.Max(rect.xMin-otherRect.xMax,otherRect.xMin-rect.xMax),gapY=Mathf.Max(rect.yMin-otherRect.yMax,otherRect.yMin-rect.yMax);
      Assert.That(Mathf.Max(gapX,gapY),Is.GreaterThanOrEqualTo(4*logicalScale-.05f),"The shared two-row footer keeps complete touch areas separate");
     }
     foreach(var text in button.GetComponentsInChildren<TMP_Text>()){
      text.ForceMeshUpdate();Assert.That(text.isTextOverflowing||text.isTextTruncated,Is.False,locale+" "+text.text);
      var bounds=CenteredContentRow.GlyphBounds(text);var min=RectTransformUtility.WorldToScreenPoint(camera,text.transform.TransformPoint(bounds.min));var max=RectTransformUtility.WorldToScreenPoint(camera,text.transform.TransformPoint(bounds.max));
      Assert.That(min.x,Is.GreaterThanOrEqualTo(rect.xMin+8),locale+" visible glyphs require left padding; font="+text.fontSize);Assert.That(max.x,Is.LessThanOrEqualTo(rect.xMax-8),locale+" visible glyphs require right padding; font="+text.fontSize);
     }
    }
   }
  }
  [UnityTest] public IEnumerator CompletionFooterRetryPreservesTheEarnedReward(){
   yield return WinFooterFixture();int count=Profile.AllClears.Count(),coins=game.ShopCoins;string before=game.Interaction.Attempt.AttemptId.ToString("D");Control("Retry completed level").onClick.Invoke();yield return FooterWait(()=>game.Interaction.CanPlay&&game.Descent?.StageIndex==0,"Retry opens the same level at its first stage");
   Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo("C01"));Assert.That(game.Interaction.Attempt.AttemptId.ToString("D"),Is.Not.EqualTo(before));Assert.That(Profile.AllClears.Count(),Is.EqualTo(count));Assert.That(game.ShopCoins,Is.EqualTo(coins));
  }
 }
}
#endif
