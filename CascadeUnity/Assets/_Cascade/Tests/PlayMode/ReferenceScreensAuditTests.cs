#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Domain;
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
    // Native reference captures with an isolated profile, including the actual selected states.
    public sealed class ReferenceScreensAuditTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;
        string directory;
        bool oldBackground;
        Camera camera;
        RenderTexture portraitTarget,oldTarget;
        PlayerProfile Profile=>(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
        AdService Economy=>(AdService)typeof(CascadeGame).GetProperty("GameEconomy",Private).GetValue(game);
        static Type ReviewType(string name)=>AppDomain.CurrentDomain.GetAssemblies()
            .Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        void Route(GameScreen screen)=>typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{screen});
        void Field(string name,object value)=>typeof(CascadeGame).GetField(name,Private).SetValue(game,value);
        void World(int world)=>ReviewType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});

        [UnitySetUp] public IEnumerator Setup()
        {
            oldBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeReferenceAudit",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();World(0);
            camera=Camera.main;oldTarget=camera.targetTexture;portraitTarget=new RenderTexture(720,1280,24);
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;
            camera.targetTexture=portraitTarget;scaler.enabled=true;yield return null;game.RefreshLayout();
            game.SetUiLocale("ru");game.SetEditorSafeInsets(new Vector4(0,.00001f,0,.00001f));
            game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            var ledger=Economy.Ledger;
            ledger.UndoSpools=3;ledger.SignalLanterns=1;ledger.LandingLenses=2;
            foreach(string id in new[]{"leaf","wind_chime","moon","rain_badge"})
                if(!ledger.Cosmetics.Contains(id))ledger.Cosmetics.Add(id);
            game.AdProvider.UseLocalPreviewForReview(true);
            yield return null;
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            if(camera!=null)camera.targetTexture=oldTarget;
            if(portraitTarget!=null){portraitTarget.Release();UnityEngine.Object.Destroy(portraitTarget);}
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=oldBackground;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Shot(string name,int width=720,int height=1280)
        {
            yield return new WaitForSecondsRealtime(.45f);
            Action<CascadeGame> check=g=>{
                foreach(var label in g.GetComponentsInChildren<TMP_Text>().Where(t=>t.name.StartsWith("Reference shop"))){
                    label.ForceMeshUpdate();Assert.That(label.isTextOverflowing||label.isTextTruncated,Is.False,name+": "+label.name);
                }
                if(g.GetComponentsInChildren<ReferenceShopLayout>().Any()){
                    var floor=g.GetComponentsInChildren<RawImage>().Single(i=>i.name=="Continuous menu floor");
                    Assert.That(floor.texture,Is.Not.Null,name+": no ground");
                    Assert.That(floor.raycastTarget,Is.False);
                    var corners=new Vector3[4];floor.rectTransform.GetWorldCorners(corners);
                    var canvas=(RectTransform)floor.GetComponentInParent<Canvas>().transform;
                    Assert.That(canvas.InverseTransformPoint(corners[0]).y,Is.EqualTo(canvas.rect.yMin).Within(.03f),name+": ground gap");
                    Assert.That(floor.uvRect.yMax,Is.EqualTo(1).Within(.001f),name+": repeated ground");
                    var frame=g.GetComponentsInChildren<Image>().Single(i=>i.name=="World-specific architectural frame");
                    Assert.That(frame.raycastTarget,Is.False,name+": frame blocks controls");
                    Assert.That(frame.sprite,Is.SameAs(g.CurrentScreen==GameScreen.JourneyMap?WorldFrameArt.MapFrame():WorldFrameArt.MenuFrame()));
                    Assert.That(frame.material.GetFloat("_HaveMenuPosts"),Is.EqualTo(1),name+": missing continuous post art");
                }
                if(g.CurrentScreen==GameScreen.JourneyMap){
                    var bridges=g.GetComponentsInChildren<Component>().Where(c=>c.GetType().Name=="MapRopeBridge").ToArray();
                    Assert.That(bridges,Is.Empty,name+": decorative map bridges were removed at the user's request");
                }
                if(g.CurrentScreen==GameScreen.Home){
                    var shortcut=g.GetComponentsInChildren<Button>().Single(b=>b.name=="Remove ads shortcut");
                    Assert.That(((RectTransform)shortcut.transform).anchoredPosition.y,Is.LessThanOrEqualTo(-150),"Shortcut escaped its reference band");
                }
            };
            yield return (IEnumerator)ReviewType("PlayableCapture").GetMethod("CaptureSettled")
                .Invoke(null,new object[]{"ReferenceAuditOct15",name+".png",width,height,check});
        }
        static IEnumerator Await(Func<bool> ready,string message)
        {
            double until=Time.realtimeSinceStartupAsDouble+15;
            while(!ready()&&Time.realtimeSinceStartupAsDouble<until)yield return null;
            Assert.That(ready(),Is.True,message);
        }
        [UnityTest] public IEnumerator SuppliedReferenceStatesAndAllWorldFrames()
        {
            game.ShowHome();yield return Shot("01-home");
            game.ShowTravelBag();yield return Shot("09-my-things-green");
            game.ShowSupplies();game.SelectShopItem(1);yield return Shot("08-items-spool");
            game.ShowShop();game.SelectShopTab(0);game.SelectShopItem(0);yield return Shot("03-shop-help");
            game.SelectShopTab(1);game.SelectShopItem(0);yield return Shot("04-shop-purchased-leaf");
            game.ShowCollection();game.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,"wind_chime"));
            Profile.Rewards.EquippedGift=null;Route(GameScreen.Collection);yield return Shot("02-trophy-unapplied");
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action").onClick.Invoke();
            yield return Shot("05-trophy-applied");
            Assert.That(Profile.Rewards.EquippedGift,Is.EqualTo("wind_chime"));
            game.ShowTravelBag();game.ShowParcel();
            var parcel=Profile.Rewards.Parcels.First(p=>!p.Claimed);
            Field("parcelOrder",parcel.Order);Field("parcelAnimationShown",false);Field("parcelOpened",false);Field("parcelBonusRound",false);
            Route(GameScreen.Parcel);game.GetComponentInChildren<ParcelRevealView>().SetClosedReviewWithCards();yield return Shot("07-parcel-closed");
            game.GetComponentInChildren<ParcelRevealView>().SetReviewPose(4.65f);yield return Shot("07-parcel-open");
            var bonus=Profile.Rewards.Parcels.First(p=>ParcelLoot.Bonus(p.Order).Count==3&&!p.Claimed);
            Assert.That(Economy.TryClaimParcel(bonus.Order,Profile.Rewards),Is.True);
            var grant=new GrantReceipt{Id="reference_audit_bonus_"+bonus.Order,Kind="coins",Product=MonetizationLedger.ParcelSubject(bonus.Order),Amount=40};
            game.Monetization.Ledger.Grants.Add(grant);game.Monetization.Ledger.BonusCoins+=40;
            Field("parcelOrder",bonus.Order);Field("parcelBonusRound",true);Route(GameScreen.Parcel);
            game.GetComponentInChildren<ParcelRevealView>().SetReviewPose(4.65f);yield return Shot("07-parcel-three-rewards");
            Assert.That(game.GetComponentsInChildren<Image>().Count(i=>i.name.StartsWith("Parcel reward icon ")),Is.EqualTo(3));
            Route(GameScreen.JourneyMap);yield return Shot("10-map");
            foreach(int world in Enumerable.Range(0,9)){
                World(world);game.ShowTravelBag();yield return Shot("world-"+world+"-my-things",390,844);
                game.ShowShop();game.SelectShopTab(0);yield return Shot("world-"+world+"-shop",390,844);
                Route(GameScreen.JourneyMap);yield return Shot("world-"+world+"-map",390,844);
            }
            World(0);game.LoadNode(game.Catalog.PrimaryLevels[18].Definition.CampaignNodeId);
            yield return Await(()=>game.PreparationReady&&game.Interaction.CanPlay,"Reference puzzle must become playable");yield return Shot("gameplay");
            game.ToggleFieldToolTray();Assert.That(game.FieldToolTrayOpen,Is.True);yield return Shot("field-help");
            Assert.That(game.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Field tool ")),Is.EqualTo(6));
            typeof(CascadeGame).GetMethod("SetToolTray",Private).Invoke(game,new object[]{false,true});
            var swap=Enumerable.Range(0,game.Interaction.Attempt.Current.Row.Count-1).Select(PuzzleCommand.Swap)
                .First(c=>game.Interaction.Attempt.Resolver.Preview(game.Interaction.Attempt.Current,c).IsLegal);
            Assert.That(game.Interaction.Execute(swap).Accepted,Is.True);
            yield return Await(()=>game.Interaction.CanPlay,"First swap must settle");game.Undo();
            yield return Await(()=>game.Interaction.CanPlay,"Free undo must settle");
            Assert.That(game.Interaction.Execute(swap).Accepted,Is.True);
            yield return Await(()=>game.Interaction.CanPlay,"Second swap must settle");
            game.ToggleFieldToolTray();Assert.That(game.FieldToolTrayOpen,Is.True);
            yield return new WaitForSecondsRealtime(.5f);
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Field tool 1").onClick.Invoke();
            yield return Shot("field-help-spool-selected");
            Assert.That(game.GetComponentsInChildren<Button>().Single(b=>b.name=="Apply field tool").interactable,Is.True);
            typeof(CascadeGame).GetMethod("SetToolTray",Private).Invoke(game,new object[]{false,true});
            Route(GameScreen.Pause);yield return Shot("pause");Route(GameScreen.Fail);yield return Shot("failure");
            Route(GameScreen.NoAds);yield return Shot("subscriptions");
            game.ShowCoinPacks();yield return Shot("coin-packs");game.ShowSettings();yield return Shot("settings");
        }
    }
}
#endif
