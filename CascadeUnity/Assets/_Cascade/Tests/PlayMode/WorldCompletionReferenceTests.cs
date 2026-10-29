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
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class WorldCompletionReferenceTests
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture target,oldTarget;string directory;bool oldAudio,background;
        object Field(string key)=>typeof(CascadeGame).GetField(key,Flags).GetValue(game);
        void Set(string key,object value)=>typeof(CascadeGame).GetField(key,Flags).SetValue(game,value);
        object Call(string key,params object[] values)=>typeof(CascadeGame).GetMethod(key,Flags).Invoke(game,values);
        string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/WorldCompletionOct16"));
        static Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;oldAudio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeWorldCompletion",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;
            Resize(720,1280);game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Preferences.Tutorials=false;game.Coach.Enabled=false;game.Preferences.EffectQuality=2;game.ReducedMotion=false;yield return null;
        }
        void Resize(int width,int height)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}target=new RenderTexture(width,height,24);camera.targetTexture=target;scaler.enabled=true;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=background;AudioListener.pause=oldAudio;if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        void Configure(int world,PuzzleMode mode=PuzzleMode.Mixed,bool show=true)
        {
            var p=(PlayerProfile)Field("profile");p.Clears.Clear();p.ModeJourneys.Clear();p.RatingRevision=1;p.Rewards=new JourneyRewards{CoinEconomyRevision=1};p.AccountIntroSeen=true;p.Missions.NextAfterClears=20000;p.PreferredMode=mode.ToString();typeof(CascadeGame).GetProperty("PreferredMode").SetValue(game,mode);
            var session=(HashSet<string>)Field("sessionClears");session.Clear();var stories=(HashSet<int>)Field("heardStory");stories.Clear();stories.UnionWith(Enumerable.Range(0,9500));p.HeardStory=stories.ToList();
            foreach(var c in game.Catalog.PrimaryLevels.Take((world+1)*60)){
                p.Clears.Add(new ClearReceipt{CampaignNodeId=c.Definition.CampaignNodeId,ContentHash=c.Definition.ContentHash,AttemptId=Guid.NewGuid().ToString("D"),BestStars=2,BestScore=1800});session.Add(c.Definition.CampaignNodeId);
            }
            var ids=game.Catalog.PrimaryLevels.Skip(world*60).Take(60).Select(c=>c.Definition.CampaignNodeId).ToHashSet();
            var last=((IEnumerable<LevelContent>)Call("JourneySequence")).Last(c=>ids.Contains(c.Definition.CampaignNodeId));
            p.Completion=new LevelCompletion{NodeId=last.Definition.CampaignNodeId,AttemptId=Guid.NewGuid().ToString("D"),RatingVersion=3,Stars=2,Score=1800,Stages=3,StatsSeen=true,Acknowledged=true};
            Set("journeyActive",false);Call("InitializeRewards");Set("mapPage",world);Set("islandBrowseIndex",-1);
            if(show)Assert.That((bool)Call("TryWorldCompletion"),Is.True);
        }
        IEnumerator Shot(string name,float age=2.7f)
        {
            yield return new WaitForSecondsRealtime(.3f);game.RefreshLayout();Canvas.ForceUpdateCanvases();
            yield return (IEnumerator)EditorType("PlayableCapture").GetMethod("CaptureSettled").Invoke(null,new object[]{"WorldCompletionOct16",name+".png",target.width,target.height,(Action<CascadeGame>)(_=>{game.GetComponentsInChildren<WorldCompletionMika>().Single().SetReviewAge(age);Verify(name);})});
        }
        Rect Ink(RectTransform r,RectTransform area)
        {
            Bounds b;var text=r.GetComponent<TextMeshProUGUI>();
            if(text!=null){text.ForceMeshUpdate();b=CenteredContentRow.GlyphBounds(text);}
            else if(r.GetComponent<PauseGlyphGraphic>() is PauseGlyphGraphic glyph&&glyph.canvasRenderer.GetMesh() is Mesh mesh&&mesh.vertexCount>0)b=mesh.bounds;
            else b=new Bounds(r.rect.center,r.rect.size);
            var a=(Vector2)area.InverseTransformPoint(r.TransformPoint(b.min));var z=(Vector2)area.InverseTransformPoint(r.TransformPoint(b.max));return Rect.MinMaxRect(Mathf.Min(a.x,z.x),Mathf.Min(a.y,z.y),Mathf.Max(a.x,z.x),Mathf.Max(a.y,z.y));
        }
        void Verify(string name)
        {
            foreach(var label in game.GetComponentsInChildren<TextMeshProUGUI>()){
                label.ForceMeshUpdate();Assert.That(label.isTextOverflowing||label.isTextTruncated,Is.False,name+" / "+label.text);
                if(label.name=="world completion heading"){
                    var area=(RectTransform)label.transform.parent;var ink=Ink(label.rectTransform,area);float padding=8*CenteredContentRow.Unit(area);
                    Assert.That(ink.xMin,Is.GreaterThanOrEqualTo(area.rect.xMin+padding-.8f),name+" title left padding");
                    Assert.That(ink.xMax,Is.LessThanOrEqualTo(area.rect.xMax-padding+.8f),name+" title right padding");
                }
                if(label.name.Contains("world completion story"))Assert.That(label.textInfo.lineCount,Is.EqualTo(4),name+" keeps four complete lines");
            }
            foreach(var group in game.GetComponentsInChildren<CenteredContentRow>()){
                var b=group.Elements.Select(r=>Ink(r,group.Surface)).ToArray();float unit=group.LocalUnit;
                Assert.That((b.Min(r=>r.xMin)+b.Max(r=>r.xMax))*.5f,Is.EqualTo(CenteredContentRow.ReadingRect(group.Surface).center.x).Within(.8f*unit),name+" content centre x");
                Assert.That((b.Min(r=>r.yMin)+b.Max(r=>r.yMax))*.5f,Is.EqualTo(CenteredContentRow.ReadingRect(group.Surface).center.y).Within(.8f*unit),name+" content centre y");
                Assert.That(b.Min(r=>r.xMin),Is.GreaterThanOrEqualTo(group.Surface.rect.xMin-1));Assert.That(b.Max(r=>r.xMax),Is.LessThanOrEqualTo(group.Surface.rect.xMax+1));
                for(int i=1;i<b.Length;i++)Assert.That(b[i].xMin-b[i-1].xMax,Is.EqualTo(4*unit).Within(.65f*unit),name+" painted icon/label gap");
            }
            foreach(var button in game.GetComponentsInChildren<Button>()){
                var safe=(RectTransform)Field("safeRoot");var b=Ink((RectTransform)button.transform,safe);float pixels=game.GetComponentInChildren<Canvas>().scaleFactor;
                Assert.That(b.width*pixels,Is.GreaterThanOrEqualTo(43.8f));Assert.That(b.height*pixels,Is.GreaterThanOrEqualTo(43.8f));Assert.That(b.yMin,Is.GreaterThanOrEqualTo(safe.rect.yMin-.5f));
            }
            var hero=game.GetComponentsInChildren<RawImage>().Single(r=>r.name=="World completion Mika");Assert.That(hero.rectTransform.rect.width/hero.rectTransform.rect.height,Is.EqualTo(1).Within(.001f));
            Assert.That(game.EnvironmentWorld,Is.EqualTo(game.CompletedWorld));Assert.That(game.GetComponentsInChildren<MenuWorldEnvironment>().Single().World,Is.EqualTo(game.CompletedWorld));
        }
        [UnityTest] public IEnumerator ReferencePreview()
        {
            game.SetUiLocale("ru");game.SetEditorSafeInsets(Vector4.zero);Configure(0);yield return Shot("reference-wind-ru-720");
            Configure(8);yield return Shot("reference-stars-ru-720");
        }
        [UnityTest,Timeout(300000)] public IEnumerator NineWorldsKeepTheReferenceAndFourPixelGroupsOnLocalizedPhones()
        {
            foreach(int world in Enumerable.Range(0,9))foreach(string locale in new[]{"en","ru","uk"})foreach(int width in new[]{320,390,430}){
                Resize(width,width==320?693:width==430?932:844);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));game.SetUiLocale(locale);game.Preferences.LargeText=true;Configure(world);
                yield return Shot("world-"+world+"-"+locale+"-"+width);
            }
        }
        [UnityTest] public IEnumerator CompleteWorldAppearsBeforeNextComicAndBothButtonsKeepTheSave()
        {
            Configure(0,show:false);var p=(PlayerProfile)Field("profile");p.Completion.Acknowledged=false;string hash=game.Interaction.Attempt.CurrentStateHash;int coins=game.ShopCoins;
            int nextComic=WorldPrologues.ForWorld(1).Receipt;((HashSet<int>)Field("heardStory")).Remove(nextComic);p.HeardStory.Remove(nextComic);
            Call("FinishCompletion");yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.WorldComplete));Assert.That(game.ComicWorld,Is.EqualTo(-1));Assert.That(p.Completion.Acknowledged,Is.True);
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Open next world").onClick.Invoke();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Prologue));Assert.That(game.ComicWorld,Is.EqualTo(1));game.FinishWorldComic();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));Assert.That(game.MapWorld,Is.EqualTo(1));Assert.That((bool)Call("TryWorldCompletion"),Is.False);
            Assert.That(game.ShopCoins,Is.EqualTo(coins));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            Configure(1);game.GetComponentsInChildren<Button>().Single(b=>b.name=="World completion home").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That((bool)Call("TryWorldCompletion"),Is.False);p.Validate();
            var saved=((ProfileStore)Field("profileStore")).Load();Assert.That(saved.Recovery,Is.EqualTo(SaveRecovery.None));Assert.That(saved.Profile.RemembersStory((int)Call("WorldCompletionReceipt",1)),Is.True,"The acknowledgement survives a real disk reload");
        }
        [UnityTest] public IEnumerator PartialWorldsNeverCelebrateAndEachModeHasItsOwnReceipt()
        {
            foreach(var mode in new[]{PuzzleMode.Shapes,PuzzleMode.Words,PuzzleMode.Mixed}){
                Configure(0,mode,false);var session=(HashSet<string>)Field("sessionClears");var offered=((IEnumerable<LevelContent>)Call("JourneySequence")).First();session.Remove(offered.Definition.CampaignNodeId);
                Assert.That((bool)Call("TryWorldCompletion"),Is.False);session.Add(offered.Definition.CampaignNodeId);Assert.That((bool)Call("TryWorldCompletion"),Is.True);
                int receipt=(int)Call("WorldCompletionReceipt",0);Assert.That(receipt,Is.InRange(9600,9640));Assert.That(game.CanSwitchJourneyMode,Is.False);
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="World completion home").onClick.Invoke();yield return null;Assert.That(((PlayerProfile)Field("profile")).HeardStory,Does.Contain(receipt));
            }
            Configure(8);game.GetComponentsInChildren<Button>().Single(b=>b.name=="Open next world").onClick.Invoke();yield return null;Assert.That(game.MapWorld,Is.EqualTo(8));Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
        }
        [UnityTest] public IEnumerator CompletePoseMotionLoopsSmoothlyAndSuspendsWithTheApplication()
        {
            Resize(390,844);Configure(0);var view=game.GetComponentsInChildren<WorldCompletionMika>().Single();float age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(view.AnimationAge,Is.GreaterThan(age));
            foreach(string policy in new[]{"focus","pause","reduced","quality"}){
                if(policy=="focus")game.SendMessage("OnApplicationFocus",false);else if(policy=="pause")game.SendMessage("OnApplicationPause",true);else if(policy=="reduced")game.ReducedMotion=true;else game.Preferences.EffectQuality=0;
                yield return null;age=view.AnimationAge;yield return new WaitForSecondsRealtime(.15f);Assert.That(view.AnimationAge,Is.EqualTo(age));
                game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.ReducedMotion=false;game.Preferences.EffectQuality=2;
            }
            float step=0;for(int i=1;i<=1800;i++)step=Mathf.Max(step,Mathf.Abs(WorldCompletionMika.FrameAt(i/120f)-WorldCompletionMika.FrameAt((i-1)/120f)));Assert.That(step,Is.LessThan(.22f));
            foreach(float t in new[]{0f,1.3f,2.7f,3.66f,5f,8.3f})yield return Shot("pose-"+Mathf.RoundToInt(t*100),t);
            var folder=Path.Combine(Output,"frames");Directory.CreateDirectory(folder);var previous=RenderTexture.active;var pixels=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            for(int i=0;i<300;i++){view.SetReviewAge(i/30f);camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,i.ToString("D4")+".jpg"),pixels.EncodeToJPG(87));}
            RenderTexture.active=previous;UnityEngine.Object.Destroy(pixels);
        }
        [UnityTest] public IEnumerator CompactAndTabletKeepTheBoardAndReachableButtons()
        {
            game.SetUiLocale("uk");game.Preferences.LargeText=true;
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(768,1024)}){
                Resize(size.x,size.y);game.SetEditorSafeInsets(new Vector4(0,.035f,0,.035f));Configure(8);yield return Shot("compact-stars-uk-"+size.x);
            }
        }
    }
}
#endif
