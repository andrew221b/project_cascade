#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed class IslandLevelReferenceTests
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture target,oldTarget;string directory;bool oldBackground,oldAudio;
        object Field(string name)=>typeof(CascadeGame).GetField(name,Flags).GetValue(game);
        void Set(string name,object value)=>typeof(CascadeGame).GetField(name,Flags).SetValue(game,value);
        void Call(string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Flags).Invoke(game,args);
        static Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        [UnitySetUp] public IEnumerator Setup()
        {
            oldBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;oldAudio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeIslandReference",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;
            Resize(720,1280);game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Preferences.Tutorials=false;game.Coach.Enabled=false;
            game.Preferences.EffectQuality=2;game.ReducedMotion=false;yield return null;
        }
        void Resize(int width,int height)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}target=new RenderTexture(width,height,24);camera.targetTexture=target;scaler.enabled=true;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=oldBackground;AudioListener.pause=oldAudio;if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        void Configure(int world,int island=0)
        {
            var p=(PlayerProfile)Field("profile");p.Clears.Clear();p.Rewards=new JourneyRewards{CoinEconomyRevision=1};p.AccountIntroSeen=true;p.RatingRevision=1;p.PreferredMode="Mixed";typeof(CascadeGame).GetProperty("PreferredMode").SetValue(game,PuzzleMode.Mixed);
            ((HashSet<string>)Field("sessionClears")).Clear();int first=world*60+island*12;
            EditorType("MapReferenceBaseline").GetMethod("Configure").Invoke(null,new object[]{game,first+3});
            int[] stars={2,3,1},score={1860,2000,1800};for(int i=0;i<3;i++){var receipt=p.Clears.First(c=>c.CampaignNodeId==game.Catalog.PrimaryLevels[first+i].Definition.CampaignNodeId);receipt.BestStars=stars[i];receipt.BestHalfStar=false;receipt.BestScore=score[i];}
            Set("journeyActive",false);Set("mapPage",world);Set("islandBrowseIndex",-1);Call("SetScreen",GameScreen.JourneyMap);game.OpenJourneyIsland(island);
            Assert.That(game.BrowsedIsland,Is.EqualTo(island));
        }
        IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(.55f);game.RefreshLayout();Canvas.ForceUpdateCanvases();
            yield return (IEnumerator)EditorType("PlayableCapture").GetMethod("CaptureSettled").Invoke(null,new object[]{"IslandLevelsOct16",name+".png",target.width,target.height,(Action<CascadeGame>)(_=>VerifyRows(name))});
            VerifyRenderedCard(name);
        }
        void VerifyRenderedCard(string name)
        {
            var scroll=game.GetComponentsInChildren<ScrollRect>().Single();
            var row=game.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Island level ")).First(b=>{var r=Ink((RectTransform)b.transform,scroll.viewport);return r.yMax<=scroll.viewport.rect.yMax+1&&r.yMin>=scroll.viewport.rect.yMin;});
            var body=(RectTransform)row.transform.Find("Reference shop button body");
            var pixels=new Texture2D(2,2,TextureFormat.RGB24,false);
            try{
                pixels.LoadImage(File.ReadAllBytes(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/IslandLevelsOct16",name+".png"))));
                Color Sample(float x,float y){var screen=camera.WorldToScreenPoint(body.TransformPoint(new Vector3(body.rect.xMin+x,body.rect.yMax-y)));return pixels.GetPixel(Mathf.Clamp(Mathf.RoundToInt(screen.x),0,pixels.width-1),Mathf.Clamp(Mathf.RoundToInt(screen.y),0,pixels.height-1));}
                var paper=Sample(112,70);var timber=Sample(6,70);
                Assert.That(paper.r-paper.b,Is.GreaterThan(.12f),name+" actually renders the cream card above the background");
                Assert.That(paper.grayscale,Is.GreaterThan(timber.grayscale+.12f),name+" actually renders the raised timber frame beside its paper");
            }finally{UnityEngine.Object.Destroy(pixels);}
        }
        Rect Ink(RectTransform target,RectTransform space)
        {
            var label=target.GetComponent<TextMeshProUGUI>();Bounds bounds;
            if(label!=null){label.ForceMeshUpdate();Assert.That(label.textInfo.lineCount,Is.LessThanOrEqualTo(1),label.name+" stays on one line after layout and render");bounds=label.textBounds;}else bounds=new Bounds(target.rect.center,target.rect.size);
            Assert.That(bounds.size.x,Is.GreaterThanOrEqualTo(0),target.name+" has a real painted bound");
            var a=(Vector2)space.InverseTransformPoint(target.TransformPoint(bounds.min));var b=(Vector2)space.InverseTransformPoint(target.TransformPoint(bounds.max));
            return Rect.MinMaxRect(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y),Mathf.Max(a.x,b.x),Mathf.Max(a.y,b.y));
        }
        void VerifyRows(string screen)
        {
            foreach(var row in game.GetComponentsInChildren<CenteredContentRow>()){
                var bounds=row.Elements.Select(r=>Ink(r,row.Surface)).ToArray();if(bounds.Length==0)continue;
                float left=bounds.Min(r=>r.xMin),right=bounds.Max(r=>r.xMax),low=bounds.Min(r=>r.yMin),high=bounds.Max(r=>r.yMax),unit=row.LocalUnit;
                Assert.That((left+right)*.5f,Is.EqualTo(row.Surface.rect.center.x).Within(.8f*unit),screen+" / "+row.Surface.name+" horizontal centre");
                Assert.That((low+high)*.5f,Is.EqualTo(row.Surface.rect.center.y).Within(.8f*unit),screen+" / "+row.Surface.name+" vertical centre");
                Assert.That(left,Is.GreaterThanOrEqualTo(row.Surface.rect.xMin-1*unit),screen+" / "+row.Surface.name+" left fit");
                Assert.That(right,Is.LessThanOrEqualTo(row.Surface.rect.xMax+1*unit),screen+" / "+row.Surface.name+" right fit");
                for(int i=1;i<bounds.Length;i++){
                    float gap=row.Vertical?bounds[i-1].yMin-bounds[i].yMax:bounds[i].xMin-bounds[i-1].xMax;
                    Assert.That(gap,Is.EqualTo(4*unit).Within(.6f*unit),screen+" / "+row.Surface.name+" icon/label gap "+i);
                }
            }
        }

        [UnityTest] public IEnumerator ReferencePreview()
        {
            game.SetEditorSafeInsets(Vector4.zero);game.SetUiLocale("ru");Configure(0);yield return Shot("reference-wind-ru-720");
            Configure(8);yield return Shot("reference-night-ru-720");
        }
        [UnityTest,Timeout(600000)] public IEnumerator NineWorldsHaveUniqueArtAndCentredLocalizedRowsOnPhones()
        {
            var textures=new HashSet<Texture>();
            for(int world=0;world<9;world++){
                Assert.That(IslandLevelArt.Background(world),Is.Not.Null);Assert.That(textures.Add(IslandLevelArt.Background(world).texture),Is.True,"Each world has distinct authored scenery");
                foreach(string locale in new[]{"en","ru","uk"})foreach(int width in new[]{320,390,430}){
                    Resize(width,width==320?693:width==430?932:844);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));game.SetUiLocale(locale);game.Preferences.LargeText=true;Configure(world);
                    yield return Shot("world-"+world+"-"+locale+"-"+width);
                    Assert.That(game.GetComponentsInChildren<MenuWorldEnvironment>().Length,Is.Zero,"The reference has open sky, without the old side frame or floor");
                    Assert.That(game.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Island level ")),Is.EqualTo(12));
                    Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Reference shop island progress").text,Is.EqualTo("6 / 36"));
                    foreach(var button in game.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Island action ")||b.name=="Back")){
                        var rect=(RectTransform)button.transform;var bounds=Ink(rect,(RectTransform)Field("safeRoot"));float pixels=game.GetComponentInChildren<Canvas>().scaleFactor;
                        Assert.That(bounds.width*pixels,Is.GreaterThanOrEqualTo(43.8f),button.name+" touch width");Assert.That(bounds.height*pixels,Is.GreaterThanOrEqualTo(43.8f),button.name+" touch height");
                    }
                    var sky=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Island illustrated background");float painted=sky.rectTransform.rect.width/sky.rectTransform.rect.height,source=sky.sprite.rect.width/sky.sprite.rect.height;
                    Assert.That(painted,Is.EqualTo(source).Within(.001f),"Background never stretches");
                    Assert.That(game.EnvironmentWorld,Is.EqualTo(world));
                }
            }
        }
        [UnityTest] public IEnumerator RealVerticalDragAndScrollbarReachTheLastRowWithoutStartingALevel()
        {
            Resize(390,844);game.SetEditorSafeInsets(new Vector4(0,.04f,0,.035f));Configure(0);yield return new WaitForSecondsRealtime(.6f);
            var scroll=game.GetComponentsInChildren<ScrollRect>().Single();var bar=game.GetComponentsInChildren<Scrollbar>().Single();string hash=game.Interaction.Attempt.CurrentStateHash;int coins=game.ShopCoins;
            var point=RectTransformUtility.WorldToScreenPoint(camera,scroll.viewport.TransformPoint(scroll.viewport.rect.center));
            var pointer=new PointerEventData(EventSystem.current){position=point,pressPosition=point,button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Assert.That(hits.Count,Is.GreaterThan(0));
            var handler=ExecuteEvents.GetEventHandler<IDragHandler>(hits[0].gameObject);Assert.That(handler.GetComponent<ScrollRect>(),Is.SameAs(scroll));
            ExecuteEvents.Execute(handler,pointer,ExecuteEvents.initializePotentialDrag);ExecuteEvents.Execute(handler,pointer,ExecuteEvents.beginDragHandler);
            for(int i=1;i<=6;i++){pointer.position=point+new Vector2(0,i*35);ExecuteEvents.Execute(handler,pointer,ExecuteEvents.dragHandler);yield return null;}
            ExecuteEvents.Execute(handler,pointer,ExecuteEvents.endDragHandler);yield return null;Assert.That(scroll.content.anchoredPosition.y,Is.GreaterThan(50));
            Assert.That(bar.value,Is.LessThan(.95f));Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.ShopCoins,Is.EqualTo(coins));
            scroll.StopMovement();bar.value=0;Canvas.ForceUpdateCanvases();yield return Shot("scrolled-bottom-390");Assert.That(scroll.verticalNormalizedPosition,Is.EqualTo(0).Within(.001f));
            var last=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Island level 12");var bounds=Ink((RectTransform)last.transform,scroll.viewport);Assert.That(bounds.yMin,Is.GreaterThanOrEqualTo(scroll.viewport.rect.yMin-.5f));
            var locked=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Island action 12");Assert.That(locked.interactable,Is.False);locked.onClick.Invoke();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
            game.CloseJourneyIsland();yield return null;Assert.That(game.BrowsedIsland,Is.EqualTo(-1));Assert.That(game.MapWorld,Is.Zero);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator ExplicitPlayAndReplayButtonsRouteToTheirPublishedLevels()
        {
            Configure(0);yield return new WaitForSecondsRealtime(.6f);
            var play=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Island action 4");Assert.That(play.interactable,Is.True);play.onClick.Invoke();yield return null;
            Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo(game.Catalog.PrimaryLevels[3].Definition.CampaignNodeId));Assert.That(game.BrowsedIsland,Is.EqualTo(-1));
            float deadline=Time.realtimeSinceStartup+20;while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(game.Interaction.CanPlay,Is.True);game.ShowJourney();game.OpenJourneyIsland(0);yield return new WaitForSecondsRealtime(.6f);Assert.That(game.BrowsedIsland,Is.Zero);
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Island action 1").onClick.Invoke();yield return null;
            Assert.That(game.Interaction.Attempt.Level.CampaignNodeId,Is.EqualTo(game.Catalog.PrimaryLevels[0].Definition.CampaignNodeId));
        }
        [UnityTest,Timeout(120000)] public IEnumerator CategoryFiltersKeepTheirRowsAndHideAnUnneededScrollTrack()
        {
            Resize(390,844);game.SetEditorSafeInsets(new Vector4(0,.04f,0,.035f));
            foreach(int world in Enumerable.Range(0,9))foreach(var mode in new[]{PuzzleMode.Shapes,PuzzleMode.Words,PuzzleMode.Mixed}){
                Configure(world);var p=(PlayerProfile)Field("profile");p.PreferredMode=mode.ToString();typeof(CascadeGame).GetProperty("PreferredMode").SetValue(game,mode);
                var expected=(int[])typeof(CascadeGame).GetMethod("IslandJourneyStops",Flags).Invoke(game,new object[]{world,0});Assert.That(expected.Length,Is.GreaterThan(0));
                Call("SetScreen",GameScreen.JourneyMap);yield return new WaitForSecondsRealtime(.08f);Canvas.ForceUpdateCanvases();
                Assert.That(game.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Island level ")),Is.EqualTo(expected.Length),world+" "+mode+" retains the existing category filter");
                var scroll=game.GetComponentsInChildren<ScrollRect>().Single();var bar=game.GetComponentsInChildren<Scrollbar>(true).Single();
                Assert.That(bar.gameObject.activeSelf,Is.EqualTo(scroll.content.rect.height>scroll.viewport.rect.height+.5f));
                if(world==2)yield return Shot("category-"+mode+"-390");
            }
        }
        [UnityTest] public IEnumerator WeatherAndMaskedStarsRespectFocusQualityAndReducedMotion()
        {
            Configure(6);yield return new WaitForSecondsRealtime(.6f);var owner=game.GetComponentsInChildren<IslandLevelAmbiance>().Single();
            var particles=owner.GetComponentInChildren<MenuWorldParticles>();var age=typeof(IslandLevelAmbiance).GetField("age",Flags);string hash=game.Interaction.Attempt.CurrentStateHash;
            float before=(float)age.GetValue(owner);yield return new WaitForSecondsRealtime(.2f);Assert.That((float)age.GetValue(owner),Is.GreaterThan(before));
            var empty=game.GetComponentsInChildren<Image>().First(i=>i.name.StartsWith("Island earned star "));Assert.That(empty.materialForRendering.GetFloat("_Stencil"),Is.GreaterThan(0),"Grey stars use the same stencil clip as their scrolling cards");
            foreach(string policy in new[]{"focus","pause","reduced","quality"}){
                if(policy=="focus")game.SendMessage("OnApplicationFocus",false);else if(policy=="pause")game.SendMessage("OnApplicationPause",true);else if(policy=="reduced")game.ReducedMotion=true;else game.Preferences.EffectQuality=0;
                yield return null;before=(float)age.GetValue(owner);yield return new WaitForSecondsRealtime(.15f);Canvas.ForceUpdateCanvases();
                if(policy!="quality")Assert.That((float)age.GetValue(owner),Is.EqualTo(before));
                Assert.That(particles.canvasRenderer.GetMesh().vertexCount,Is.Zero,policy+" suppresses animated weather");
                game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.ReducedMotion=false;game.Preferences.EffectQuality=2;yield return null;
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));game.CloseJourneyIsland();yield return null;Assert.That(owner==null,Is.True,"Closing the page removes its weather owner");
        }
        [UnityTest] public IEnumerator CompactAndTabletLayoutsKeepReachableRowsAndStableNavigation()
        {
            game.SetUiLocale("uk");game.Preferences.LargeText=true;
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(768,1024)}){
                Resize(size.x,size.y);game.SetEditorSafeInsets(new Vector4(0,.035f,0,.035f));Configure(8,4);yield return Shot("compact-night-"+size.x);
                var scroll=game.GetComponentsInChildren<ScrollRect>().Single();Assert.That(scroll.viewport.rect.height,Is.GreaterThan(300));
                Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name=="Reference shop island level number").All(t=>t.text.Length==3),Is.True,"Published level numbers are retained");
                var back=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Back");back.onClick.Invoke();yield return null;Assert.That(game.BrowsedIsland,Is.EqualTo(-1));Assert.That(game.MapWorld,Is.EqualTo(8));
            }
        }
    }
}
#endif
