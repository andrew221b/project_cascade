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
        }
        TrophyAmbiance[] GlobalEmitters()=>game.GetComponentsInChildren<TrophyAmbiance>(true).Where(v=>!IsCollectionPreview(v.transform)).ToArray();
        static bool IsCollectionPreview(Transform t){for(;t!=null;t=t.parent)if(t.name=="Miniature game")return true;return false;}
        [UnityTest] public IEnumerator CollectionMagnifiesRealPinsAndCentresWholeCostumes()
        {
            Profile.Monetization.Cosmetics.AddRange(ShopCatalog.TrophyIds.Where(t=>!ShopCatalog.IsParcelExclusive(t)));
            foreach(var content in game.Catalog.PrimaryLevels.GroupBy(c=>c.Definition.CampaignNodeId).Select(g=>g.First()).Take(3)){
                Profile.Rewards.SeenClearNodes.Add(content.Definition.CampaignNodeId);
                Profile.Clears.Add(new ClearReceipt{CampaignNodeId=content.Definition.CampaignNodeId,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=content.Definition.ContentHash});
            }
            Profile.Rewards.Parcels.Add(new ParcelReceipt{Order=1,RecipeVersion=4,Kind="cosmetic",GiftId="paper_crane",Claimed=true});Profile.Validate();
            foreach(var id in new[]{"leaf","wind_chime","paper_crane"}){
                OpenCollectionItem(id);yield return null;
                var hero=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Trophy Mika preview");var view=hero.GetComponent<MikaCosmeticView>();
                Assert.That(game.GetComponentsInChildren<TrophyMagnifierView>().Length,Is.EqualTo(1));
                Assert.That(game.GetComponentsInChildren<Image>().Single(i=>i.name=="Magnified actual Mika sprite").sprite,Is.SameAs(hero.sprite));
                Assert.That(game.GetComponentsInChildren<Image>().Single(i=>i.name=="Magnified actual trophy sprite").sprite,Is.SameAs(view.PreviewBadge.sprite));
                Assert.That(Profile.Rewards.EquippedGift,Is.Null,"Selection is a preview, not an equip");
            }
            foreach(var id in new[]{"sky","moon","rain_badge","sky_pilot","moth_mask","garden_festival","cloud_gardener","explorer_hat","frog_suit","star_knight","fox_mask","mushroom_cap","bee_suit","winter_hood","pirate_hat","jester_suit","constellation_mage"}){
                OpenCollectionItem(id);yield return null;
                var hero=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Trophy Mika preview");
                Assert.That(hero.GetComponent<MikaCosmeticView>().WearsCostume,Is.True,id);
                Assert.That(game.GetComponentsInChildren<TrophyMagnifierView>().Length,Is.Zero,id);
                Assert.That(hero.rectTransform.anchoredPosition.x+hero.rectTransform.rect.width*.5f,Is.EqualTo(360).Within(.1f),id);
                Assert.That(hero.preserveAspect,Is.True);
            }
        }
        [UnityTest] public IEnumerator CollectionActionsUpdateAppliedStateAndCarouselStaysOwnedOnly()
        {
            Profile.Monetization.Cosmetics.AddRange(new[]{"leaf","wind_chime","moon","rain_badge","explorer_hat","frog_suit"});
            OpenCollectionItem("leaf");yield return null;
            var action=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action");
            Assert.That(action.GetComponentsInChildren<Image>().Any(i=>i.sprite!=null&&i.sprite.name=="GreenAction"),Is.True);
            action.onClick.Invoke();yield return null;
            Assert.That(Profile.Rewards.EquippedGift,Is.EqualTo("leaf"));
            Assert.That(game.GetComponentsInChildren<Image>().Any(i=>i.sprite!=null&&i.sprite.name=="RedAction"),Is.True);
            Assert.That(game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Any(t=>t.name=="Reference shop applied card leaf"),Is.True);
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();yield return null;
            Assert.That(Profile.Rewards.EquippedGift,Is.Null);
            Assert.That(game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Any(t=>t.name=="Reference shop applied card leaf"),Is.False);
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Next owned trophies").onClick.Invoke();yield return null;
            var cards=game.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Shop object ")).ToArray();
            Assert.That(cards.Length,Is.EqualTo(2));Assert.That(cards.All(b=>game.OwnsGift(ShopCatalog.TrophyIds[int.Parse(b.name.Substring(12))])),Is.True);
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Next owned trophies").onClick.Invoke();yield return null;
            Assert.That(game.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Shop object ")),Is.EqualTo(4));
            game.ShowHome();yield return null;Assert.That(game.GetComponentsInChildren<TrophyMagnifierView>().Any(),Is.False);
            Assert.That(GlobalEmitters().All(v=>!v.isActiveAndEnabled),Is.True);
        }
        [UnityTest] public IEnumerator ShopSelectionNeverOpensPreviewAndOnlyCollectionCanApplyStyles()
        {
            foreach(string id in ShopCatalog.SceneryTrophyIds.Concat(ShopCatalog.FrameTrophyIds)){
                int index=Array.IndexOf(ShopCatalog.TrophyIds,id);
                game.ShowShop();game.SelectShopTab(1);game.SelectShopItem(index);yield return null;
                var pictures=game.GetComponentsInChildren<Image>().Where(i=>i.name=="Catalogue object").ToArray();
                var shopTrophies=ShopCatalog.TrophyIds.Where(t=>!ShopCatalog.IsParcelExclusive(t)).ToArray();
                int pageStart=Array.IndexOf(shopTrophies,id)/6*6;
                Assert.That(pictures.Length,Is.EqualTo(Math.Min(6,shopTrophies.Length-pageStart)));
                Assert.That(pictures.All(i=>i.sprite!=null),Is.True,id);
                Assert.That(game.GetComponentsInChildren<Button>().Any(b=>b.name.Contains("Preview")),Is.False);
                Assert.That(pictures.Single(i=>i.GetComponentInParent<Button>().name=="Shop object "+index).material.name,Does.Not.Contain("grayscale"),"Selected unowned trophy stays colorful");
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Shop object "+index).onClick.Invoke();yield return null;
                Assert.That(game.GetComponentsInChildren<RectTransform>().Any(t=>t.name=="Trophy Mika preview"||t.name=="Miniature game"),Is.False,id);
                Profile.Monetization.Cosmetics.Add(id);game.SelectShopItem(index);yield return null;
                var buy=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Buy selected item");
                Assert.That(buy.interactable,Is.False,"Owned trophies cannot be bought or applied in shop");
                Assert.That(game.GetComponentsInChildren<Image>().Single(i=>i.name=="Catalogue object"&&i.GetComponentInParent<Button>().name=="Shop object "+index).material.name,Does.Contain("grayscale"));
                int coins=game.ShopCoins;
                Assert.That(game.BuySelectedShopItem(),Is.False);Assert.That(game.ShopCoins,Is.EqualTo(coins));
                OpenCollectionItem(id);yield return null;
                Assert.That(game.GetComponentsInChildren<RectTransform>().Any(t=>t.name=="Miniature game"),Is.True,id);
                Assert.That(game.GetComponentsInChildren<Image>().Single(i=>i.name=="Trophy Mika preview").material.name,Does.Not.Contain("grayscale"));
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();yield return null;
                Assert.That(ShopCatalog.IsFrame(id)?Profile.Rewards.EquippedFrame:Profile.Rewards.EquippedScenery,Is.EqualTo(id));
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Back").onClick.Invoke();yield return null;
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Shop),"Back returns to the collection origin");
            }
        }
        [UnityTest] public IEnumerator BuyingTrophyMarksItPurchasedWithoutApplyingOrChargingTwice()
        {
            Profile.Monetization.BonusCoins=1000;
            game.ShowShop();game.SelectShopTab(1);game.SelectShopItem(1);yield return null;
            int coins=game.ShopCoins;
            Assert.That(game.BuySelectedShopItem(),Is.True);yield return null;
            Assert.That(game.OwnsGift("sun"),Is.True);
            Assert.That(game.ShopCoins,Is.EqualTo(coins-ShopCatalog.TrophyPrice("sun")));
            Assert.That(Profile.Rewards.EquippedGift,Is.Null);
            var action=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Buy selected item");Assert.That(action.interactable,Is.False);
            Assert.That(game.GetComponentsInChildren<Image>().Count(i=>i.name=="Reference shop purchased ribbon face"),Is.EqualTo(1));
            Assert.That(game.GetComponentsInChildren<RectTransform>().Any(t=>t.name=="Trophy Mika preview"),Is.False);
            coins=game.ShopCoins;Assert.That(game.BuySelectedShopItem(),Is.False);Assert.That(game.ShopCoins,Is.EqualTo(coins));
            OpenCollectionItem("sun");yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();yield return null;
            Assert.That(Profile.Rewards.EquippedGift,Is.EqualTo("sun"));
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
        [UnityTest] public IEnumerator InventoryFourPanelsNavigateWithoutChangingOwnership()
        {
            Profile.Monetization.Cosmetics.Add("leaf");Profile.Rewards.EquippedGift="leaf";
            string[] panels={"Inventory lights","Inventory items","Inventory balance","Inventory trophies"};
            GameScreen[] destinations={GameScreen.JourneyMap,GameScreen.Supplies,GameScreen.CoinPacks,GameScreen.Collection};
            for(int i=0;i<panels.Length;i++){
                game.ShowHome();game.ShowTravelBag();yield return null;
                Assert.That(game.GetComponentsInChildren<Button>().Count(b=>panels.Contains(b.name)),Is.EqualTo(4));
                Assert.That(game.GetComponentsInChildren<Image>().Single(x=>x.name=="Inventory Mika").preserveAspect,Is.True);
                game.GetComponentsInChildren<Button>().Single(b=>b.name==panels[i]).onClick.Invoke();yield return null;
                Assert.That(game.CurrentScreen,Is.EqualTo(destinations[i]));Assert.That(Profile.Rewards.EquippedGift,Is.EqualTo("leaf"));
            }
            game.ShowTravelBag();yield return null;
            Assert.That(game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single(t=>t.name=="Reference shop Inventory trophies count").text,Is.EqualTo("1 / 46"));
            game.ReducedMotion=true;yield return null;Assert.That(game.GetComponentsInChildren<MenuLeafDrift>().Single().VisibleCount,Is.Zero);
            game.ReducedMotion=false;game.Preferences.EffectQuality=0;yield return null;Assert.That(game.GetComponentsInChildren<MenuLeafDrift>().Single().VisibleCount,Is.Zero);
        }
        [UnityTest] public IEnumerator InventoryAndShopHaveClearActionsAndLargerCards()
        {
            game.ShowHome();game.ShowSupplies();yield return null;
            Assert.That(game.GetComponentsInChildren<RectTransform>(true).Any(r=>r.name=="Catalogue guidance"),Is.False);
            Assert.That(game.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true).Any(t=>t.text=="Choose a tool"),Is.False);
            game.ShowSupplies();yield return null;
            var buttons=game.GetComponentsInChildren<Button>();
            Assert.That(buttons.Any(b=>b.name=="Use selected tool"||b.name=="Open shop"),Is.False);
            Assert.That(buttons.Any(b=>b.name=="Items visit shop"),Is.True);
            var heading=game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single(t=>t.name=="Reference shop items heading");
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
        [UnityTest] public IEnumerator EquippedSceneryRemainsAboveMenuBackgroundAndResumesAfterAbsence()
        {
            Profile.Monetization.Cosmetics.AddRange(new[]{"leaf","petal_drift"});Profile.Rewards.EquippedGift="leaf";Profile.Rewards.EquippedScenery="petal_drift";
            game.Preferences.EffectQuality=2;game.ReducedMotion=false;game.Preferences.ReducedMotion=false;
            foreach(var screen in new[]{GameScreen.JourneyMap,GameScreen.Shop,GameScreen.Supplies,GameScreen.TravelBag}){
                typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{screen});yield return null;Canvas.ForceUpdateCanvases();
                var stage=game.GetComponentsInChildren<ReferenceShopLayout>().Single().transform;
                var emitters=stage.GetComponentsInChildren<TrophyAmbiance>();Assert.That(emitters.Length,Is.EqualTo(2),screen.ToString());
                var leaves=emitters.SelectMany(v=>v.GetComponentsInChildren<Image>()).Where(i=>i.enabled&&i.name.StartsWith("Trophy flourish ")).ToArray();Assert.That(leaves.Length,Is.GreaterThan(0));
                var background=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Reference shop environment");
                Assert.That(leaves.All(i=>i.canvasRenderer.absoluteDepth>background.canvasRenderer.absoluteDepth),Is.True,"Equipped leaves must be above the opaque sky");
                game.SendMessage("OnApplicationPause",true);game.SendMessage("OnApplicationFocus",false);var positions=leaves.Select(i=>i.rectTransform.anchoredPosition).ToArray();
                yield return null;yield return null;Assert.That(leaves.Select(i=>i.rectTransform.anchoredPosition),Is.EqualTo(positions));
                game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
                for(int i=0;i<20;i++)yield return new WaitForSecondsRealtime(.04f);
                Assert.That(leaves.Where((i,k)=>(i.rectTransform.anchoredPosition-positions[k]).sqrMagnitude>.0001f).Any(),Is.True,"Equipped scenery must resume on "+screen);
            }
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
            var emitters=GlobalEmitters();
            Assert.That(emitters.Length,Is.GreaterThanOrEqualTo(2));
            Assert.That(emitters.Count(view=>view.isActiveAndEnabled&&view.VisibleCount>0),Is.EqualTo(1),
                "the independently equipped scenery keeps only its own atmosphere active");

            OpenCollectionItem("petal_drift");yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();yield return null;
            Assert.That(Profile.Rewards.EquippedScenery,Is.Null);
            emitters=GlobalEmitters();
            Assert.That(emitters.All(view=>!view.isActiveAndEnabled),Is.True,
                "removing the scenery style disables the background leaf emitter");
            game.ShowHome();yield return null;
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(game.GetComponentsInChildren<Image>(true).Where(i=>i.name.StartsWith("Trophy flourish ")).All(i=>!i.isActiveAndEnabled),Is.True);

            OpenCollectionItem("leaf");yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();yield return null;
            emitters=GlobalEmitters();
            Assert.That(emitters.Any(view=>view.isActiveAndEnabled&&view.VisibleCount>0),Is.True);
            OpenCollectionItem("leaf");yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();yield return null;
            emitters=GlobalEmitters();
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
