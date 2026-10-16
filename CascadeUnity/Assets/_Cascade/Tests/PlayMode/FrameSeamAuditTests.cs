#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class FrameSeamAuditTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture target,previousTarget;string directory;bool background,audio;
        readonly List<Measurement> measurements=new List<Measurement>();
        [Serializable] sealed class Measurement { public string screen;public int world,width,height,legacy_strip_difference_pixels,legacy_strip_control_pixels;public bool continuous_ground,full_width_ground;public float original_landing_mode; }
        [Serializable] sealed class Report { public List<Measurement> measurements; }
        bool Baseline=>File.Exists(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../frame-seam-baseline.flag")));
        string Phase=>Baseline?"FrameSeamsOct16Before":"FrameSeamsOct16";
        string Folder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports",Phase));
        object Field(string name)=>typeof(CascadeGame).GetField(name,Private).GetValue(game);
        void Set(string name,object value)=>typeof(CascadeGame).GetField(name,Private).SetValue(game,value);
        void Call(string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Private).Invoke(game,args);
        static Type Review(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        void Route(GameScreen screen){Set("islandBrowseIndex",-1);if(screen==GameScreen.Parcel){game.ShowParcel();Assert.That(game.CurrentScreen,Is.EqualTo(screen));}else Call("SetScreen",screen);}
        void World(int world)=>Review("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});
        [UnitySetUp] public IEnumerator Setup()
        {
            measurements.Clear();
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeFrameSeams",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;previousTarget=camera.targetTexture;Resize(390,844);
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Preferences.Tutorials=false;game.Coach.Enabled=false;game.Preferences.EffectQuality=2;game.ReducedMotion=true;
            var p=(PlayerProfile)Field("profile");p.AccountIntroSeen=true;p.Missions.NextAfterClears=20000;p.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            ((HashSet<int>)Field("heardStory")).UnionWith(Enumerable.Range(0,10000));game.SetUiLocale("ru");game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));game.ShowHome();yield return null;
        }
        void Resize(int width,int height)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}target=new RenderTexture(width,height,24);camera.targetTexture=target;scaler.enabled=true;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,TestContext.CurrentContext.Test.Name+"-measurements.json"),JsonUtility.ToJson(new Report{measurements=measurements},true));
            if(camera!=null)camera.targetTexture=previousTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}CascadeGame.EditorProfileDirectory=null;AudioListener.pause=audio;UnityEngine.Application.runInBackground=background;if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        Color32[] Pixels()
        {
            var render=camera.targetTexture;var old=RenderTexture.active;var pixels=new Texture2D(render.width,render.height,TextureFormat.RGB24,false);
            try{Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,render.width,render.height),0,0);pixels.Apply();return pixels.GetPixels32();}
            finally{RenderTexture.active=old;UnityEngine.Object.Destroy(pixels);}
        }
        int Difference(Color32[] first,Color32[] second)
        {
            int changed=0;for(int y=0;y<target.height*.32f;y++)for(int x=0;x<target.width;x++){
                if(x>target.width*.24f&&x<target.width*.76f)continue;int i=y*target.width+x;
                if(Math.Abs(first[i].r-second[i].r)+Math.Abs(first[i].g-second[i].g)+Math.Abs(first[i].b-second[i].b)>9)changed++;
            }return changed;
        }
        void Verify(string key)
        {
            var frame=game.GetComponentsInChildren<Image>().FirstOrDefault(i=>i.name=="World-specific architectural frame");
            var ground=game.GetComponentsInChildren<RawImage>().FirstOrDefault(i=>i.name=="Continuous menu floor");
            var metric=new Measurement{screen=key,world=game.EnvironmentWorld,width=target.width,height=target.height,continuous_ground=ground!=null,full_width_ground=ground==null};
            if(ground!=null){
                var corners=new Vector3[4];ground.rectTransform.GetWorldCorners(corners);var low=camera.WorldToScreenPoint(corners[0]);var high=camera.WorldToScreenPoint(corners[2]);
                metric.full_width_ground=low.x<.5f&&high.x>target.width-.5f&&Mathf.Abs(low.y)<.5f;
                Assert.That(metric.full_width_ground,Is.True,key+": floor reaches both physical edges and bottom");Assert.That(ground.raycastTarget,Is.False);
            }
                if(frame!=null&&game.CurrentScreen!=GameScreen.JourneyMap){
                    var environment=game.GetComponentsInChildren<MenuWorldEnvironment>().Single();environment.SetReviewAge(22);
                    var original=frame.material;metric.original_landing_mode=original.GetFloat("_ContinuousLanding");var actual=Pixels();
                    var repairedMaterial=new Material(original);repairedMaterial.SetFloat("_ContinuousLanding",1);
                    var legacyMaterial=new Material(original);legacyMaterial.SetFloat("_ContinuousLanding",0);
                    try{
                        frame.material=repairedMaterial;var repaired=Pixels();metric.legacy_strip_difference_pixels=Difference(actual,repaired);
                        frame.material=legacyMaterial;metric.legacy_strip_control_pixels=Difference(Pixels(),repaired);
                        if(key.StartsWith("reference-NoAds",StringComparison.Ordinal))Assert.That(metric.legacy_strip_control_pixels,Is.GreaterThan(20),"The A/B probe must read actual rendered strip pixels");
                    }finally{frame.material=original;UnityEngine.Object.Destroy(repairedMaterial);UnityEngine.Object.Destroy(legacyMaterial);}
                    if(!Baseline)Assert.That(metric.legacy_strip_difference_pixels,Is.LessThanOrEqualTo(3),key+": legacy paving strips remain over the full-width floor");
                }
            if(frame!=null){var corners=new Vector3[4];frame.rectTransform.GetWorldCorners(corners);var low=camera.WorldToScreenPoint(corners[0]);var high=camera.WorldToScreenPoint(corners[2]);Assert.That(low.x,Is.LessThan(.5f));Assert.That(high.x,Is.GreaterThan(target.width-.5f));Assert.That(frame.raycastTarget,Is.False);}
            measurements.Add(metric);
        }
        IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(.15f);game.RefreshLayout();
            yield return (IEnumerator)Review("PlayableCapture").GetMethod("CaptureSettled").Invoke(null,new object[]{Phase,name+".png",target.width,target.height,(Action<CascadeGame>)(_=>Verify(name))});
        }
        [UnityTest] public IEnumerator CaptureRepresentativeSeams()
        {
            foreach(var screen in new[]{GameScreen.NoAds,GameScreen.Shop,GameScreen.Pause}){Route(screen);yield return Shot("reference-"+screen+"-ru-390");}
        }
        [UnityTest] public IEnumerator AllCurrentScreensAndOverlaysAtFourPortraitSizes()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(430,932),new Vector2Int(768,1024)}){
                Resize(size.x,size.y);World(0);
                foreach(var screen in new[]{GameScreen.Home,GameScreen.AccountGate,GameScreen.TravelBag,GameScreen.Supplies,GameScreen.Collection,GameScreen.Parcel,GameScreen.Shop,GameScreen.CoinPacks,GameScreen.NoAds,GameScreen.Pause,GameScreen.JourneyMap,GameScreen.JourneyEnd,GameScreen.Prologue,GameScreen.SaveRecovery,GameScreen.Playing,GameScreen.Fail}){
                    Route(screen);yield return Shot("screen-"+screen+"-"+size.x);
                }
                foreach(int page in new[]{-1,0,1,2}){Set("settingsPage",page);Route(GameScreen.Settings);yield return Shot("settings-"+page+"-"+size.x);}
                foreach(string document in new[]{"privacy","terms"}){game.OpenLegalDocument(document);yield return Shot(document+"-"+size.x);game.CloseLegalDocument();}
                Route(GameScreen.Shop);game.SelectShopTab(1);yield return Shot("shop-trophies-"+size.x);game.SelectShopTab(2);yield return Shot("shop-coins-"+size.x);
                Route(GameScreen.JourneyMap);game.OpenJourneyIsland(0);yield return Shot("island-levels-"+size.x);
                Route(GameScreen.Dialogue);yield return Shot("dialogue-"+size.x);
                game.Preferences.Tutorials=true;Route(GameScreen.Playing);Call("OpenMechanicPopup",MechanicLesson.Undo,null,default(UiCopy),null);yield return Shot("tutorial-single-"+size.x);Call("CloseMechanicPopup");
                Call("OpenMechanicPopup",MechanicLesson.Swap,null,default(UiCopy),null);yield return Shot("tutorial-pages-"+size.x);Call("CloseMechanicPopup");game.Preferences.Tutorials=false;
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator EveryWorldUsesContinuousFloorBesideItsPosts()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(int world in Enumerable.Range(0,9)){
                World(world);foreach(var screen in new[]{GameScreen.Home,GameScreen.AccountGate,GameScreen.Shop,GameScreen.NoAds,GameScreen.TravelBag,GameScreen.Supplies,GameScreen.Collection,GameScreen.Parcel,GameScreen.Pause,GameScreen.JourneyMap}){Route(screen);yield return Shot("world-"+world+"-"+screen);}
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator DeliveredLevelVictoryAtFourPortraitSizes()
        {
            var row=new[]{"circle","triangle","square"}.Select(s=>SymbolId.Parse("s:"+s)).ToArray();
            var fixture=new LevelDefinition("frame-seam-win","frame-seam-win","1","2.0","frame-seam-win",row,row,new[]{new TargetSegment(SegmentKind.Shapes,0,3)},new[]{new FloorDefinition(8,new[]{true,true})});
            var delivered=new AttemptService(fixture);Assert.That(delivered.Execute(PuzzleCommand.Drop).State.Status,Is.EqualTo(PuzzleStatus.Won));
            Call("MountAttempt",delivered,null);string hash=delivered.CurrentStateHash;
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(430,932),new Vector2Int(768,1024)}){Resize(size.x,size.y);Route(GameScreen.Win);yield return Shot("screen-Win-"+size.x);}
            Assert.That(delivered.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator CompletedCampaignStageCheckpointAtFourPortraitSizes()
        {
            game.StartCampaignLevel(game.Catalog.GetPrimary("C02").Definition);game.ShowGame();
            float deadline=Time.realtimeSinceStartup+30;while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<deadline)yield return null;Assert.That(game.Interaction.CanPlay,Is.True);
            foreach(var command in Cascade.Solver.PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path){
                Assert.That(game.Interaction.Execute(command).Accepted,Is.True);deadline=Time.realtimeSinceStartup+30;while(game.Interaction.IsPresenting&&Time.realtimeSinceStartup<deadline)yield return null;
            }
            deadline=Time.realtimeSinceStartup+30;while(game.CurrentScreen!=GameScreen.Checkpoint&&Time.realtimeSinceStartup<deadline)yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Checkpoint));
            string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(430,932),new Vector2Int(768,1024)}){Resize(size.x,size.y);yield return Shot("screen-Checkpoint-"+size.x);}
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
    }
}
#endif
