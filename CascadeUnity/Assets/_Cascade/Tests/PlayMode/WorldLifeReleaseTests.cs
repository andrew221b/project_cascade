#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class WorldLifeReleaseTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture portrait,oldTarget;string directory;bool background,audio;
        object Field(string name)=>typeof(CascadeGame).GetField(name,Private).GetValue(game);
        PlayerProfile Profile=>(PlayerProfile)Field("profile");
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeWorldLifeRelease",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            void Preload(string m,string s,LogType t){if(t==LogType.Error&&m.StartsWith("OperationException : Failed to initialize localization, could not preload string tables.",StringComparison.Ordinal))LogAssert.Expect(t,new System.Text.RegularExpressions.Regex("^OperationException : Failed to initialize localization, could not preload string tables\\."));}
            UnityEngine.Application.logMessageReceived+=Preload;
            try{yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;foreach(string locale in new[]{"en","ru","uk"}){var table=UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));yield return table;Assert.That(table.Status,Is.EqualTo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded));}}
            finally{UnityEngine.Application.logMessageReceived-=Preload;}
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;portrait=new RenderTexture(390,844,24);
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=portrait;scaler.enabled=true;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.SetEditorSafeInsets(new Vector4(0,.00001f,0,.00001f));game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;game.ReducedMotion=false;game.Coach.Enabled=false;Profile.AccountIntroSeen=true;Profile.Missions.NextAfterClears=20000;
            Profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();((HashSet<int>)Field("heardStory")).UnionWith(Enumerable.Range(0,10000));game.SetUiLocale("ru");game.LoadNode("C02");game.ShowGame();
            for(int i=0;i<120&&!game.Interaction.CanPlay;i++)yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Playing));Assert.That(game.Interaction.CanPlay,Is.True);game.Machine.SetAmbientPaused(true);yield return null;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}CascadeGame.EditorProfileDirectory=null;AudioListener.pause=audio;UnityEngine.Application.runInBackground=background;if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        Rect ScreenRect(RectTransform rect){var c=new Vector3[4];rect.GetWorldCorners(c);var a=camera.WorldToScreenPoint(c[0]);var b=camera.WorldToScreenPoint(c[2]);return Rect.MinMaxRect(a.x,a.y,b.x,b.y);}
        void FooterClear(CascadeGame current)
        {
            float sill=camera.targetTexture.height*WorldFrameArt.SillViewportHeight(camera.aspect);var actions=ScreenRect((RectTransform)Field("actions"));var receiver=ScreenRect((RectTransform)Field("receiver"));
            Assert.That(actions.yMin,Is.GreaterThan(sill+8),"Entire action row must be in sky above the authored sill");Assert.That(receiver.yMin,Is.GreaterThan(actions.yMax+12),"Receiver must move with the action row");
            foreach(var button in current.GetComponentsInChildren<Button>().Where(b=>b.interactable)){
                var r=ScreenRect((RectTransform)button.transform);Assert.That(r.width,Is.GreaterThanOrEqualTo(43.9f),button.name);Assert.That(r.height,Is.GreaterThanOrEqualTo(43.9f),button.name);
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=r.center},hits);Assert.That(hits.Count,Is.GreaterThan(0),button.name);Assert.That(hits[0].gameObject.GetComponentInParent<Button>(),Is.SameAs(button),button.name);
            }
        }
        IEnumerator Shot(string name,int width=390,int height=844,Action<CascadeGame> pose=null)
        {
            yield return new WaitForSecondsRealtime(.5f);
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable.PlayableCapture")).First(t=>t!=null);
            yield return (IEnumerator)type.GetMethod("CaptureSettled").Invoke(null,new object[]{"WorldLifeReleaseOct15",name+".png",width,height,pose});
        }
        [UnityTest] public IEnumerator GameplayFooterAndAuthoredWeatherAreVisibleInEveryWorld()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;var world=game.Machine.LivingWorld;
            for(int zone=0;zone<9;zone++){
                int biome=zone;yield return Shot("world-"+zone+"-gameplay",pose:g=>{world.SetZone(biome);g.RenderFrame(g.VisibleFrame);world.Paint(12.3f);FooterClear(g);Assert.That(world.ActiveWeatherCount,Is.GreaterThan(0));Assert.That(world.GetComponentsInChildren<WorldSeasonalWeather>().Length,Is.EqualTo(1));if(biome==8)Assert.That(g.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single(t=>t.name=="ReceiverLabel").color.r,Is.GreaterThan(.7f));WeatherPixels();});
            }
            foreach(var size in new[]{new Vector2Int(320,693),new Vector2Int(720,1280),new Vector2Int(768,1024)})yield return Shot("footer-"+size.x+"x"+size.y,size.x,size.y,FooterClear);
            game.SetEditorSafeInsets(new Vector4(.02f,.055f,.02f,.05f));game.Preferences.LargeText=true;yield return Shot("footer-safe-large",pose:FooterClear);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            game.LoadNode("C32");game.ShowGame();for(int i=0;i<120&&!game.Interaction.CanPlay;i++)yield return null;yield return Shot("footer-complex-small",320,693,FooterClear);
        }
        void WeatherPixels()
        {
            var weather=game.Machine.LivingWorld.GetComponentInChildren<WorldSeasonalWeather>();var face=weather.GetComponent<MeshRenderer>();var render=camera.targetTexture;
            var old=RenderTexture.active;var pixels=new Texture2D(render.width,render.height,TextureFormat.RGB24,false);
            try{camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,render.width,render.height),0,0);pixels.Apply();var with=pixels.GetPixels32();face.enabled=false;camera.Render();pixels.ReadPixels(new Rect(0,0,render.width,render.height),0,0);pixels.Apply();var without=pixels.GetPixels32();int changed=0;for(int i=0;i<with.Length;i++)if(Math.Abs(with[i].r-without[i].r)+Math.Abs(with[i].g-without[i].g)+Math.Abs(with[i].b-without[i].b)>12)changed++;Assert.That(changed,Is.GreaterThan(12),"World "+game.Machine.LivingWorld.Zone+": seasonal effects must actually affect visible mobile pixels");}
            finally{face.enabled=true;RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(pixels);}
        }
        [UnityTest] public IEnumerator WildlifeLandsOnPaintedFrameContactsAndLeavesSmoothlyOnInput()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;var world=game.Machine.LivingWorld;var life=world.GetComponent<WorldLifeView>();var wind=typeof(ToyMachineView).GetField("windTime",Private);var seen=new HashSet<WildlifeSpecies>();
            for(int zone=0;zone<9;zone++){
                world.SetZone(zone);float start=game.Machine.AmbientPhase;bool landed=false;
                for(int step=0;step<7200;step++){
                    float phase=start+step/60f;wind.SetValue(game.Machine,phase);world.Paint(phase);
                    if(life.BirdState!=WildlifePerchState.Resting||life.BirdSupportKind!=WildlifePerchKind.Frame)continue;
                    phase+=.65f;wind.SetValue(game.Machine,phase);world.Paint(phase);
                    var feet=life.BirdFeet;var p=camera.WorldToViewportPoint(feet);Assert.That(p.x<.08f||p.x>.92f,Is.True);Assert.That(p.y,Is.InRange(.20f,.83f));seen.Add(life.PerchedSpecies);
                    world.Paint(phase);Assert.That(life.BirdFeet,Is.EqualTo(feet),"Frozen clock may not move a planted foot");int biome=zone;float held=phase;
                    yield return Shot("world-"+zone+"-frame-rest",pose:g=>{world.SetZone(biome);wind.SetValue(game.Machine,held);world.Paint(held);});
                    game.Machine.NotifyWildlifeInput();wind.SetValue(game.Machine,phase+.01f);world.Paint(phase+.01f);Assert.That(life.BirdState,Is.EqualTo(WildlifePerchState.Departing));Assert.That(Vector3.Distance(life.BirdFeet,feet),Is.LessThan(.04f),"Takeoff must begin at the actual planted feet");landed=true;break;
                }
                Assert.That(landed,Is.True,"World "+zone+" must receive a frame visitor");
            }
            Assert.That(seen.Contains(WildlifeSpecies.Bullfinch),Is.True);Assert.That(seen.Any(s=>s==WildlifeSpecies.FieldCricket||s==WildlifeSpecies.BarnOwl),Is.True);Assert.That(seen.Count,Is.GreaterThanOrEqualTo(4));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            world.ReducedMotion=true;world.Paint(game.Machine.AmbientPhase);Assert.That(life.ActiveCreatureCount,Is.Zero);world.ReducedMotion=false;world.EffectQuality=0;world.Paint(game.Machine.AmbientPhase);Assert.That(world.ActiveWeatherCount,Is.Zero);Assert.That(life.ActiveCreatureCount,Is.Zero);
        }
        [UnityTest] public IEnumerator MenusHaveRegionalVisitorsAndFlowersWithoutBlockingControls()
        {
            var session=(HashSet<string>)Field("sessionClears");string hash=game.Interaction.Attempt.CurrentStateHash;
            for(int zone=0;zone<9;zone++){
                Profile.Clears.Clear();session.Clear();foreach(var level in game.Catalog.PrimaryLevels.Take(zone*60+18)){Profile.Clears.Add(new ClearReceipt{CampaignNodeId=level.Definition.CampaignNodeId,ContentHash=level.Definition.ContentHash,AttemptId=Guid.NewGuid().ToString("D"),BestStars=2});session.Add(level.Definition.CampaignNodeId);}
                typeof(CascadeGame).GetField("journeyActive",Private).SetValue(game,false);game.ShowHome();yield return null;
                var env=game.GetComponentsInChildren<MenuWorldEnvironment>().Single();Assert.That(env.World,Is.EqualTo(zone));env.SetReviewAge(0);env.SetReviewAge(10);
                var flecks=game.GetComponentsInChildren<MenuSeasonalFlecks>().Single();Assert.That(flecks.VisibleCount,Is.GreaterThan(0));Assert.That(game.GetComponentsInChildren<WorldFrameResident>().Single().Resting,Is.True);
                flecks.Initialize(zone);Assert.That(flecks.GetComponentsInChildren<Image>().Length,Is.EqualTo(8),"Reinitialization must reuse the pool");
                if(zone==0){var entries=(Image[])typeof(MenuSeasonalFlecks).GetField("flecks",Private).GetValue(flecks);Array.Clear(entries,0,entries.Length);flecks.Pose(10,2);Assert.That(flecks.VisibleCount,Is.EqualTo(4));Assert.That(flecks.GetComponentsInChildren<Image>().Length,Is.EqualTo(8),"Reload must rebind retained objects without duplicate emitters");}
                yield return Shot("world-"+zone+"-menu-life",pose:g=>env.SetReviewAge(10));
                Assert.That(game.GetComponentsInChildren<Graphic>().Where(g=>g.GetComponentInParent<WorldFrameResident>()!=null||g.GetComponentInParent<MenuSeasonalFlecks>()!=null).All(g=>!g.raycastTarget),Is.True);
                game.ReducedMotion=true;env.EndReview();yield return null;Assert.That(game.GetComponentsInChildren<WorldFrameResident>().Single().Resting,Is.False);game.ReducedMotion=false;
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
    }
}
#endif
