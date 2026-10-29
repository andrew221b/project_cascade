#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed partial class UiPolishReviewTests
    {
        void VerifyHomeArtworkAndTouches(string context)
        {
            foreach(var entry in new[]{("Mythings","BagIcon"),("Shop","ShopIcon"),("Settings","SettingsIcon")}){
                var control=game.GetComponentsInChildren<Button>().Single(b=>b.name==entry.Item1);
                Assert.That(control.GetComponent<GameButtonDesign>(),Is.Null,context+" / illustrated Home navigation keeps its own layout");
                Assert.That(((RectTransform)control.transform).rect.size,Is.EqualTo(new Vector2(165,157)));
                var image=control.GetComponentsInChildren<Image>().Single(i=>i.name=="Home menu icon");
                Assert.That(image.sprite,Is.SameAs(Resources.Load<Sprite>("CascadeArt/HomeOct13/"+entry.Item2)));
                Assert.That(((Image)control.targetGraphic).sprite,Is.SameAs(Resources.Load<Sprite>("CascadeArt/HomeOct13/NavButtonFrame")));
                Assert.That(image.rectTransform.rect.height,Is.GreaterThan(60),"Original illustrations remain large, not the shared 48-unit glyph");
                var caption=control.GetComponentInChildren<TextMeshProUGUI>();caption.ForceMeshUpdate();
                Assert.That(caption.textInfo.characterInfo.Take(caption.textInfo.characterCount).Any(c=>c.isVisible),Is.True,"All three Home captions remain visible");
                Assert.That(control.GetComponent<CenteredContentRow>().Vertical,Is.True,"Illustration and caption retain their authored stacked layout");
            }
            var account=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Sign in"||b.name=="Cloud account");
            Assert.That(account.GetComponent<GameButtonDesign>(),Is.Null);Assert.That(account.GetComponent<Image>().sprite,Is.SameAs(AccountReferenceArt.Get("SignIn")));
            var noAds=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Remove ads shortcut");
            Assert.That(noAds.GetComponent<GameButtonDesign>(),Is.Null);Assert.That(noAds.GetComponent<Image>().sprite,Is.SameAs(Resources.Load<Sprite>("CascadeArt/HomeOct13/NoAds")));
            Assert.That(game.GetComponentsInChildren<Image>().Any(i=>i.name=="Account identity icon"||i.name=="Remove ads icon"),Is.False,"Do not replace baked shortcut artwork with a new glyph");
            var regions=new List<Rect>();
            foreach(var control in game.GetComponentsInChildren<Button>().Where(b=>b.interactable)){
                var corners=new Vector3[4];((RectTransform)control.transform).GetWorldCorners(corners);var a=camera.WorldToScreenPoint(corners[0]);var b=camera.WorldToScreenPoint(corners[2]);var box=Rect.MinMaxRect(a.x,a.y,b.x,b.y);regions.Add(box);
                Assert.That(box.width,Is.GreaterThanOrEqualTo(43.8f),context+control.name);Assert.That(box.height,Is.GreaterThanOrEqualTo(43.8f),context+control.name);
                Assert.That(box.xMin,Is.GreaterThanOrEqualTo(-.2f));Assert.That(box.yMin,Is.GreaterThanOrEqualTo(-.2f));Assert.That(box.xMax,Is.LessThanOrEqualTo(portrait.width+.2f));Assert.That(box.yMax,Is.LessThanOrEqualTo(portrait.height+.2f));
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=box.center},hits);
                Assert.That(hits.Count,Is.GreaterThan(0));Assert.That(hits[0].gameObject.GetComponentInParent<Button>(),Is.SameAs(control),context+control.name+" remains reachable");
            }
            for(int i=0;i<regions.Count;i++)for(int j=i+1;j<regions.Count;j++)Assert.That(regions[i].Overlaps(regions[j]),Is.False,context+" Home touch regions remain separate");
        }
        [UnityTest,Timeout(300000)] public IEnumerator HomeNavigationKeepsOriginalIllustrationsAndMapContinueHasOnlyText()
        {
            game.ReducedMotion=true;
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(430,932)})foreach(string locale in new[]{"en","ru","uk"})foreach(bool large in new[]{false,true}){
                Resize(size.x,size.y);game.SetUiLocale(locale);game.Preferences.LargeText=large;game.ShowHome();string context="restored-"+locale+"-"+size.x+(large?"-large":"");
                yield return SettleButtons(context+"-Home");VerifyHomeArtworkAndTouches(context);
                if(size.x==390&&!large)yield return Shot(context+"-Home");
                Route(GameScreen.JourneyMap);yield return SettleButtons(context+"-Map");
                var travel=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Travel");
                Assert.That(travel.GetComponentsInChildren<PauseGlyphGraphic>().Length,Is.Zero,"Map Continue has no play triangle");
                Assert.That(travel.transform.Find("Reference map continue icon"),Is.Null);
                var row=travel.GetComponentInChildren<CenteredContentRow>();Assert.That(row.Elements.Length,Is.EqualTo(1));Assert.That(row.Elements[0].GetComponent<TextMeshProUGUI>(),Is.Not.Null);
                Assert.That(travel.GetComponent<GameButtonDesign>().Preset.Role,Is.EqualTo(GameButtonRole.PrimaryGreen));
                if(size.x==390&&!large)yield return Shot(context+"-Map");
            }
            Resize(390,844);game.Preferences.LargeText=false;
            foreach(var entry in new[]{("Mythings",GameScreen.TravelBag),("Shop",GameScreen.Shop),("Settings",GameScreen.Settings),("Remove ads shortcut",GameScreen.NoAds)}){
                game.ShowHome();yield return SettleButtons("restored Home routing "+entry.Item1);game.GetComponentsInChildren<Button>().Single(b=>b.name==entry.Item1).onClick.Invoke();
                Assert.That(game.CurrentScreen,Is.EqualTo(entry.Item2),"Original Home action still opens its intended screen");
            }
        }
    }
}
#endif
