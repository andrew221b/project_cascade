using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class ScreenCosmeticFlowTests
    {
        CascadeGame game;string isolated;bool oldBackground;
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        PlayerProfile Profile=>(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
        [UnitySetUp] public IEnumerator Setup(){oldBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;isolated=Path.Combine(Path.GetTempPath(),"CascadeScreenCosmeticTests",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=isolated;yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.MusicEnabled=false;game.Feedback.SoundEnabled=false;yield return null;}
        [UnityTearDown] public IEnumerator Teardown(){if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=oldBackground;if(Directory.Exists(isolated))Directory.Delete(isolated,true);}
        void OpenCollectionItem(string id)
        {
            game.ShowCollection();game.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,id));
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Preview").onClick.Invoke();
        }
        [UnityTest] public IEnumerator EveryScreenStyleHasItsOwnPreviewAndUnifiedCatalogue()
        {
            foreach(string id in ShopCatalog.SceneryTrophyIds.Concat(ShopCatalog.FrameTrophyIds)){
                game.ShowShop();game.SelectShopTab(1);game.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,id));yield return null;
                Assert.That(game.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Trophy category ")),Is.Zero);
                var pictures=game.GetComponentsInChildren<Image>().Where(i=>i.name=="Catalogue object").ToArray();
                var shopTrophies=ShopCatalog.TrophyIds.Where(t=>!ShopCatalog.IsParcelExclusive(t)).ToArray();
                int pageStart=Array.IndexOf(shopTrophies,id)/6*6;
                Assert.That(pictures.Length,Is.EqualTo(Math.Min(6,ShopCatalog.TrophyIds.Count(t=>!ShopCatalog.IsParcelExclusive(t))-pageStart)));
                Assert.That(pictures.All(i=>i.sprite!=null),Is.True,id);
                var buy=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Buy selected item");
                var preview=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Preview selected item");
                Assert.That(((RectTransform)preview.transform).anchoredPosition.y,Is.LessThan(((RectTransform)buy.transform).anchoredPosition.y),"Preview is below Buy: "+id);
                preview.onClick.Invoke();yield return null;
                Assert.That(game.GetComponentsInChildren<RectTransform>().Any(r=>r.name=="Miniature game"),Is.True,id);
                Assert.That(game.GetComponentsInChildren<RectTransform>().Any(r=>r.name=="Trophy Mika preview"),Is.False,id);
                Assert.That(game.GetComponentsInChildren<Image>().First(i=>i.name=="Visitor preview"||i.name=="Frame preview").material.name,Does.Not.Contain("grayscale"));
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Back").onClick.Invoke();yield return null;
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Shop),"Back returns to shop: "+id);
            }
        }
        [UnityTest] public IEnumerator CollectionShowsOnlyOwnedTrophies()
        {
            const string owned="wind_chime";
            Profile.Monetization.Cosmetics.Add(owned);
            game.ShowCollection();yield return null;
            var cards=game.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Shop object ")).ToArray();
            Assert.That(cards.Select(b=>b.name),Is.EqualTo(new[]{"Shop object "+Array.IndexOf(ShopCatalog.TrophyIds,owned)}));
            Assert.That(game.GetComponentsInChildren<Image>().Where(i=>i.name=="Catalogue object").All(i=>i.material==null||!i.material.name.Contains("grayscale")),Is.True);
        }
        [UnityTest] public IEnumerator InventoryAndShopHaveClearActionsAndLargerCards()
        {
            game.ShowHome();game.ShowSupplies();yield return null;
            Assert.That(game.GetComponentsInChildren<RectTransform>(true).Any(r=>r.name=="Catalogue guidance"),Is.False);
            Assert.That(game.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true).Any(t=>t.text=="Choose a tool"),Is.False);
            game.ShowSupplies();yield return null;
            var buttons=game.GetComponentsInChildren<Button>();
            Assert.That(buttons.Any(b=>b.name=="Use selected tool"||b.name=="Open shop"),Is.False);
            var heading=game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single(t=>t.name=="Page heading");
            Assert.That(heading.textWrappingMode,Is.EqualTo(TMPro.TextWrappingModes.NoWrap));
            game.ShowShop();game.SelectShopTab(1);yield return null;
            buttons=game.GetComponentsInChildren<Button>();
            Assert.That(buttons.Any(b=>b.name.StartsWith("Trophy category ")),Is.False);
            var cards=buttons.Where(b=>b.name.StartsWith("Shop object ")).ToArray();Assert.That(cards.Length,Is.EqualTo(6));
            foreach(var card in cards){var rect=(RectTransform)card.transform;Assert.That(rect.rect.height/((RectTransform)rect.parent).rect.height,Is.GreaterThan(.13f));}
            Assert.That(game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Any(t=>t.name=="Selected object benefit"||t.name=="Purchase limits"),Is.False);
            game.ShowCoinPacks();yield return null;
            Assert.That(game.GetComponentsInChildren<RectTransform>().Any(t=>t.name=="Fair help rules papyrus"),Is.False);
            game.ShowJournal();yield return null;Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name=="Myroutes"),Is.False);
        }
        [UnityTest] public IEnumerator EquippedLeavesAreOpaqueAndDriftWithWind()
        {
            Profile.Monetization.Cosmetics.AddRange(new[]{"leaf","petal_drift"});Profile.Rewards.EquippedGift="leaf";Profile.Rewards.EquippedScenery="petal_drift";
            game.Preferences.EffectQuality=2;game.Preferences.ReducedMotion=false;game.ShowHome();game.CreateGlobalCosmeticAmbiance();yield return null;
            var leaves=game.GetComponentsInChildren<Image>().Where(i=>i.enabled&&i.name.StartsWith("Trophy flourish ")).ToArray();
            Assert.That(leaves.Length,Is.GreaterThan(0));Assert.That(leaves.All(i=>i.canvasRenderer.GetAlpha()>.99f),Is.True);
            var x=leaves.Select(i=>i.rectTransform.anchoredPosition.x).ToArray();
            // Editor focus and frame cadence vary while several suites run. Observe
            // enough foreground frames for the deliberately capped physics step.
            for(int frame=0;frame<24;frame++){
                game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
                yield return new WaitForSecondsRealtime(.04f);
            }
            Assert.That(leaves.Where((leaf,i)=>Mathf.Abs(leaf.rectTransform.anchoredPosition.x-x[i])>.02f).Any(),Is.True);
            Assert.That(leaves.All(i=>i.canvasRenderer.GetAlpha()>.99f),Is.True);

            // Exercise the same collection action as a player: removing a Mika
            // trophy must leave only a separately equipped scenery style alive.
            OpenCollectionItem("leaf");yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();yield return null;
            Assert.That(Profile.Rewards.EquippedGift,Is.Null);
            var emitters=game.GetComponentsInChildren<TrophyAmbiance>(true);
            Assert.That(emitters.Length,Is.GreaterThanOrEqualTo(2));
            Assert.That(emitters.Count(view=>view.isActiveAndEnabled&&view.VisibleCount>0),Is.EqualTo(1),
                "the independently equipped scenery keeps only its own atmosphere active");

            OpenCollectionItem("petal_drift");yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();yield return null;
            Assert.That(Profile.Rewards.EquippedScenery,Is.Null);
            Assert.That(emitters.All(view=>!view.isActiveAndEnabled),Is.True,
                "removing the scenery style disables the background leaf emitter");
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(game.GetComponentsInChildren<Image>(true).Where(i=>i.name.StartsWith("Trophy flourish ")).All(i=>!i.isActiveAndEnabled),Is.True);

            OpenCollectionItem("leaf");yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();yield return null;
            Assert.That(emitters.Any(view=>view.isActiveAndEnabled&&view.VisibleCount>0),Is.True);
            OpenCollectionItem("leaf");yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();yield return null;
            Assert.That(emitters.All(view=>!view.isActiveAndEnabled),Is.True,
                "removing the Mika leaf trophy through the collection action disables its emitter");
        }
        [UnityTest] public IEnumerator IndependentStylesCanEquipRemoveAndSurviveMenuRoundTrip()
        {
            Profile.Monetization.Cosmetics.AddRange(new[]{"explorer_hat","dragonfly_stream","lunar_frame"});Profile.Rewards.EquippedGift="explorer_hat";
            OpenCollectionItem("dragonfly_stream");yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();
            Assert.That(Profile.Rewards.EquippedScenery,Is.EqualTo("dragonfly_stream"));
            OpenCollectionItem("lunar_frame");yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();
            Assert.That(Profile.Rewards.EquippedFrame,Is.EqualTo("lunar_frame"));Assert.That(Profile.Rewards.EquippedGift,Is.EqualTo("explorer_hat"));
            game.ShowGame();yield return null;
            var corners=game.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Game frame corner")).ToArray();
            Assert.That(corners.Length,Is.EqualTo(4));Assert.That(corners.All(i=>i.enabled&&i.sprite!=null&&!i.raycastTarget),Is.True);
            game.ShowHome();yield return null;Assert.That(corners.All(i=>!i.enabled),Is.True);
            game.ShowGame();yield return null;Assert.That(corners.All(i=>i.enabled&&i.sprite!=null),Is.True);
            OpenCollectionItem("lunar_frame");yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();
            Assert.That(Profile.Rewards.EquippedFrame,Is.Null);Assert.That(Profile.Rewards.EquippedScenery,Is.EqualTo("dragonfly_stream"));Assert.That(Profile.Rewards.EquippedGift,Is.EqualTo("explorer_hat"));
        }
    }
}
