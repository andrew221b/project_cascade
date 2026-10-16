using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed class ItemsScreenFlowTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;string directory;bool background;
        AdService Economy=>(AdService)typeof(CascadeGame).GetProperty("GameEconomy",Private).GetValue(game);
        Button Button(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeItemsTests",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            void PreloadLog(string message,string stack,LogType type){if(type==LogType.Error&&message.StartsWith("OperationException : Failed to initialize localization, could not preload string tables.",StringComparison.Ordinal))LogAssert.Expect(type,new System.Text.RegularExpressions.Regex("^OperationException : Failed to initialize localization, could not preload string tables\\."));}
            UnityEngine.Application.logMessageReceived+=PreloadLog;
            try{
                yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
                foreach(string locale in new[]{"en","ru","uk"}){var t=UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));yield return t;Assert.That(t.Status,Is.EqualTo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded));}
            }finally{UnityEngine.Application.logMessageReceived-=PreloadLog;}
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Preferences.Tutorials=false;game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            Economy.Ledger.BonusCoins=362;Economy.Ledger.UndoSpools=1;Economy.Ledger.SignalLanterns=1;game.ShowTravelBag();game.ShowSupplies();yield return null;
        }
        [UnityTearDown] public IEnumerator Teardown(){if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=background;if(Directory.Exists(directory))Directory.Delete(directory,true);}
        [UnityTest] public IEnumerator FourCardPagesReachEveryToolAndSelectionNeverConsumesInventory()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;int coins=game.ShopCoins;
            var first=game.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Shop object ")).Select(b=>b.name).ToArray();Assert.That(first,Is.EqualTo(new[]{"Shop object 1","Shop object 2","Shop object 3","Shop object 0"}));
            Assert.That(Button("Items previous page").interactable,Is.False);Button("Items next page").onClick.Invoke();yield return null;
            var second=game.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Shop object ")).Select(b=>b.name).ToArray();Assert.That(second,Is.EqualTo(new[]{"Shop object 4","Shop object 5"}));Assert.That(Button("Items next page").interactable,Is.False);
            Button("Shop object 5").onClick.Invoke();yield return null;Button("Items previous page").onClick.Invoke();yield return null;Assert.That(Button("Shop object 1"),Is.Not.Null);
            Assert.That(game.ShopCoins,Is.EqualTo(coins));Assert.That(game.Stock(TravelTool.UndoSpool),Is.EqualTo(1));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator ShopOpensSelectedToolAndReturnsToOriginalItemVisit()
        {
            game.SelectShopItem(3);Button("Items visit shop").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Shop));
            var selected=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Catalogue object"&&i.GetComponentInParent<Button>().name=="Shop object 3");Assert.That(selected,Is.Not.Null);
            Button("Back").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Supplies));Assert.That(game.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Reference shop items stock selected").text,Is.EqualTo("× 1"));
            Button("Back").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.TravelBag));
        }
        [UnityTest] public IEnumerator EveryLocaleAndToolFitsAtLargeTextAndPreservesSpriteAspect()
        {
            game.Preferences.LargeText=true;
            foreach(string locale in new[]{"en","ru","uk"}){game.SetUiLocale(locale);foreach(int item in new[]{0,1,2,3,4,5}){
                game.SelectShopItem(item);yield return null;Canvas.ForceUpdateCanvases();
                Assert.That(game.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Reference shop items heading").text,Is.EqualTo(locale=="en"?"My items":locale=="ru"?"Мои предметы":"Мої предмети"));foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){t.ForceMeshUpdate();Assert.That(t.isTextOverflowing||t.isTextTruncated,Is.False,locale+" "+item+" "+t.name);}
                var icon=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Items selected object");Assert.That(icon.preserveAspect,Is.True);Assert.That(icon.color,Is.EqualTo(Color.white));
            }}
        }
        [UnityTest] public IEnumerator ZeroAndLargeStocksUseContainedPillsAndEmptyThumbnailsAreMuted()
        {
            game.SetUiLocale("ru");game.SelectShopItem(1);Economy.Ledger.UndoSpools=0;game.NotifyStoreChanged();yield return null;
            var thumbnail=Button("Shop object 1").GetComponentsInChildren<Image>().Single(i=>i.name=="Catalogue object");Assert.That(thumbnail.material.name,Does.Contain("grayscale"));
            Assert.That(game.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Reference shop items stock selected").text,Is.EqualTo("× 0"));
            Economy.Ledger.UndoSpools=129;game.NotifyStoreChanged();yield return null;
            foreach(var t in game.GetComponentsInChildren<TMP_Text>().Where(t=>t.name.StartsWith("Reference shop items stock"))){t.ForceMeshUpdate();Assert.That(t.isTextOverflowing||t.isTextTruncated,Is.False);}
            Assert.That(game.GetComponentsInChildren<Image>().Single(i=>i.name=="Items selected object").material.name,Does.Not.Contain("grayscale"));
        }
        [UnityTest] public IEnumerator SunlightHasVisibleMeshAndHonoursPauseAndReducedMotion()
        {
            game.SelectShopItem(1);yield return null;Canvas.ForceUpdateCanvases();var light=game.GetComponentsInChildren<ItemSunlight>().Single();var mesh=light.canvasRenderer.GetMesh();Assert.That(mesh.vertexCount,Is.GreaterThan(500));Assert.That(mesh.colors.Max(c=>c.a),Is.GreaterThan(.2f));Assert.That(light.canvasRenderer.cull,Is.False);
            float before=light.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(light.AnimationAge,Is.GreaterThan(before));
            game.SendMessage("OnApplicationPause",true);before=light.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(light.AnimationAge,Is.EqualTo(before));game.SendMessage("OnApplicationPause",false);
            game.ReducedMotion=true;before=light.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(light.AnimationAge,Is.EqualTo(before));Assert.That(game.GetComponentsInChildren<MenuLeafDrift>().Single().VisibleCount,Is.Zero);
            game.ReducedMotion=false;game.Preferences.EffectQuality=0;before=light.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(light.AnimationAge,Is.EqualTo(before));
        }
        [UnityTest] public IEnumerator DepthFoliageIsScopedToItemsAndTrophiesAndHasSharpAndSoftLayers()
        {
            var leaves=game.GetComponentsInChildren<MenuLeafDrift>().Single();Assert.That(leaves.DepthOfField,Is.True);
            var art=leaves.GetComponentsInChildren<Image>();Assert.That(art.Any(i=>i.material.name=="Default UI Material"),Is.True);Assert.That(art.Count(i=>i.material.shader.name=="Cascade/UI/LeafDepthBlur"),Is.EqualTo(6));
            Assert.That(art.All(i=>!i.raycastTarget),Is.True);game.ShowCollection();yield return null;Assert.That(game.GetComponentsInChildren<MenuLeafDrift>().Single().DepthOfField,Is.True);
            game.ShowShop();yield return null;Assert.That(game.GetComponentsInChildren<MenuLeafDrift>().Single().DepthOfField,Is.False);
        }
    }
}
