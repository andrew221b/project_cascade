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
    public sealed class AtmosphereStoryPolishTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture portrait,oldTarget;string directory;bool background,audio;
        object Field(string name)=>typeof(CascadeGame).GetField(name,Private).GetValue(game);
        static Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        void Route(GameScreen screen)=>typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{screen});
        TextMeshProUGUI Label(string name)=>game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name==name);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeNavigationReviewOct15Polish",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;
            Resize(390,844);game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));
            game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;game.ReducedMotion=false;game.Coach.Enabled=false;
            var profile=(PlayerProfile)Field("profile");profile.AccountIntroSeen=true;profile.Missions.NextAfterClears=20000;profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            ((HashSet<int>)Field("heardStory")).UnionWith(Enumerable.Range(0,10000));game.ShowHome();yield return null;game.RefreshLayout();
        }
        void Resize(int width,int height)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;
            if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}portrait=new RenderTexture(width,height,24);camera.targetTexture=portrait;scaler.enabled=true;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}CascadeGame.EditorProfileDirectory=null;AudioListener.pause=audio;UnityEngine.Application.runInBackground=background;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Shot(string name,Action<CascadeGame> pose=null)
        {
            yield return new WaitForSecondsRealtime(.6f);
            yield return (IEnumerator)EditorType("PlayableCapture").GetMethod("CaptureSettled").Invoke(null,new object[]{"AtmosphereStoryPolishOct15",name+".png",portrait.width,portrait.height,pose});
        }
        void Fits(TextMeshProUGUI label,bool fullHeight=true)
        {
            label.maxVisibleCharacters=int.MaxValue;label.ForceMeshUpdate();
            Assert.That(label.isTextOverflowing,Is.False,label.name+": "+label.text);
            var preferred=label.GetPreferredValues(label.text,label.rectTransform.rect.width,Mathf.Infinity);
            Assert.That(preferred.x,Is.LessThanOrEqualTo(label.rectTransform.rect.width+.5f),label.name+" width");
            if(fullHeight)Assert.That(preferred.y,Is.LessThanOrEqualTo(label.rectTransform.rect.height+.5f),label.name+" height");
        }
        void PaddedActionFits(Button button)
        {
            var label=button.GetComponentInChildren<TextMeshProUGUI>();label.ForceMeshUpdate();var control=(RectTransform)button.transform;var r=control.rect;
            Assert.That(label.isTextOverflowing,Is.False,button.name+": "+label.text+" font "+label.fontSize+" rect "+r+" margin "+label.margin);
            float unit=CenteredContentRow.Unit(control);
            foreach(var glyph in label.textInfo.characterInfo.Take(label.textInfo.characterCount).Where(c=>c.isVisible)){
                var low=control.InverseTransformPoint(label.transform.TransformPoint(glyph.bottomLeft));var high=control.InverseTransformPoint(label.transform.TransformPoint(glyph.topRight));
                Assert.That(low.x,Is.GreaterThanOrEqualTo(r.xMin+12*unit-.5f),button.name+" left padding: "+label.text);
                Assert.That(high.x,Is.LessThanOrEqualTo(r.xMax-12*unit+.5f),button.name+" right padding: "+label.text);
                Assert.That(low.y,Is.GreaterThanOrEqualTo(r.yMin+4*unit-.5f),button.name+" bottom padding");
                Assert.That(high.y,Is.LessThanOrEqualTo(r.yMax-4*unit+.5f),button.name+" top padding");
            }
        }
        Color32[] Pixels()
        {
            var before=RenderTexture.active;var image=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);
            try{camera.Render();RenderTexture.active=portrait;image.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);image.Apply();return image.GetPixels32();}
            finally{RenderTexture.active=before;UnityEngine.Object.DestroyImmediate(image);}
        }
        static int Difference(Color32[] a,Color32[] b)
        {
            int changed=0;for(int i=0;i<a.Length;i++)if(Math.Abs(a[i].r-b[i].r)+Math.Abs(a[i].g-b[i].g)+Math.Abs(a[i].b-b[i].b)>14)changed++;return changed;
        }
        [UnityTest] public IEnumerator WeatherActuallyChangesVisiblePixelsAcrossWorldsAndScreens()
        {
            var metrics=new List<string>();string hash=game.Interaction.Attempt.CurrentStateHash;
            for(int world=0;world<9;world++){
                EditorType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});
                foreach(var screen in new[]{GameScreen.Home,GameScreen.JourneyMap,GameScreen.Settings,GameScreen.Shop}){
                    Route(screen);yield return null;var env=game.GetComponentsInChildren<MenuWorldEnvironment>().Single();env.SetReviewAge(14);
                    Assert.That(env.World,Is.EqualTo(world));Canvas.ForceUpdateCanvases();var with=Pixels();
                    var effects=env.transform.parent.GetComponentsInChildren<Graphic>().Where(g=>g is MenuWorldParticles||g.GetComponentInParent<MenuSeasonalFlecks>()!=null).ToArray();
                    foreach(var g in effects)g.enabled=false;
                    var settingsFrame=screen==GameScreen.Settings?game.GetComponentsInChildren<Image>().Single(i=>i.name=="Settings outer wooden frame"):null;
                    if(settingsFrame!=null)settingsFrame.material.SetFloat("_Age",0);
                    var without=Pixels();int changed=Difference(with,without);metrics.Add(world+","+screen+","+changed);
                    Assert.That(changed,Is.GreaterThan(24),world+" "+screen+": weather must reach the actual phone pixels");
                    foreach(var g in effects)g.enabled=true;env.SetReviewAge(14);
                    if(screen==GameScreen.JourneyMap){Assert.That(game.GetComponentsInChildren<Image>().Any(i=>i.name=="Reference map mode wooden frame"),Is.False,"The user removed this brown surround");Assert.That(game.GetComponentsInChildren<Image>().Any(i=>i.name=="Reference map mode board"),Is.False);}
                    yield return Shot("world-"+world+"-"+screen, g=>env.SetReviewAge(14));
                    if(WorldSeasonalArt.Rooted(world)&&screen==GameScreen.Home){
                        var plants=game.GetComponentInChildren<MenuSeasonalFlecks>();var flower=plants.GetComponentsInChildren<Image>().First(i=>i.enabled);Vector3 root=flower.transform.position;Quaternion turn=flower.transform.rotation;
                        env.SetReviewAge(15.5f);Assert.That(flower.transform.position,Is.EqualTo(root),"Flower stem remains attached");Assert.That(Quaternion.Angle(turn,flower.transform.rotation),Is.GreaterThan(1));
                        yield return Shot("world-"+world+"-Home-wind",g=>env.SetReviewAge(15.5f));
                    }
                }
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            var folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/AtmosphereStoryPolishOct15");Directory.CreateDirectory(folder);File.WriteAllLines(Path.Combine(folder,"visible-effects.csv"),new[]{"world,screen,changed_pixels"}.Concat(metrics));
        }
        [UnityTest] public IEnumerator ComicTurnsStayWholeAndFitEveryPhoneLocaleAndSpeakerName()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(string locale in new[]{"en","ru","uk"})foreach(bool large in new[]{false,true}){
                Resize(large?320:390,large?693:844);game.Preferences.LargeText=large;game.SetUiLocale(locale);yield return null;
                for(int world=0;world<9;world++){
                    game.OpenWorldComic(world);yield return null;
                    for(int page=0;page<4;page++)for(int turn=0;turn<2;turn++){
                        Canvas.ForceUpdateCanvases();game.GetComponentInChildren<WorldComicLayout>().RefreshLayout();Canvas.ForceUpdateCanvases();
                        var source=WorldPrologues.ForWorld(world).Pages[page].Lines[turn];var body=Label("World comic spoken line");
                        Assert.That(body.text,Is.EqualTo(new UiCopy(source.Text).ToString()),"An entire turn must appear in one beat");
                        foreach(var ink in new[]{body,Label("World comic speaker"),Label("World comic progress")})Assert.That(ink.color.maxColorComponent,Is.LessThan(.5f),"Ink on comic paper must remain dark in the night world: "+ink.name);
                        Fits(Label("World comic speaker"));Fits(body);Fits(Label("World comic progress"));PaddedActionFits(game.GetComponentsInChildren<Button>().Single(b=>b.name=="Next comic page"));Assert.That(Label("World comic progress").GetComponent<MenuSpriteCounter>(),Is.Null,"The reading counter keeps its slash and plain ink");var reader=body.GetComponentInParent<ScrollRect>();Assert.That(reader.vertical,Is.False,"All authored turns fit without scrolling");
                        Assert.That(game.ComicPage,Is.EqualTo(page));
                        if(turn==0)yield return Shot(locale+"-"+(large?"320-large":"390")+"-comic-"+world+"-"+page);
                        game.GetComponentsInChildren<Button>().Single(b=>b.name=="Next comic page").onClick.Invoke();yield return null;
                    }
                    Assert.That(game.ComicWorld,Is.EqualTo(-1));
                }
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator GameplayWeatherAndFlowersReachTheVisiblePhoneImage()
        {
            game.LoadNode("C02");game.ShowGame();for(int i=0;i<180&&!game.Interaction.CanPlay;i++)yield return null;
            Assert.That(game.Interaction.CanPlay,Is.True);game.Machine.SetAmbientPaused(true);string hash=game.Interaction.Attempt.CurrentStateHash;
            var world=game.Machine.LivingWorld;var weather=world.GetComponentInChildren<WorldSeasonalWeather>();var face=weather.GetComponent<MeshRenderer>();
            for(int zone=0;zone<9;zone++){
                world.SetZone(zone);world.Paint(14.3f);var with=Pixels();face.enabled=false;int changed=Difference(with,Pixels());
                Assert.That(changed,Is.GreaterThan(24),"Gameplay weather/flowers must affect pixels in world "+zone);face.enabled=true;
                int biome=zone;yield return Shot("world-"+zone+"-gameplay",g=>{world.SetZone(biome);world.Paint(14.3f);});
                if(zone==0||zone==1||zone==6)yield return Shot("world-"+zone+"-gameplay-wind",g=>{world.SetZone(biome);world.Paint(15.5f);});
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator MapDialoguesKeepWholeTurnsAndAllNamesInsideTheirRibbons()
        {
            Resize(320,693);game.Preferences.LargeText=true;
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);Route(GameScreen.JourneyMap);yield return null;
                for(int world=0;world<9;world++){
                    EditorType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});Route(GameScreen.JourneyMap);yield return null;
                    var scenes=new List<StoryConversation>();
                    for(int island=0;island<5;island++){
                        scenes.Add(StoryConversations.IslandOpening(world,island));scenes.Add(StoryConversations.Island(world,island));
                        scenes.Add(StoryConversations.IslandProgress(JourneyStory.IslandFirst(world,island)+5));
                    }
                    scenes.Add(StoryConversations.LevelAside(JourneyStory.IslandFirst(world,3)+1));scenes.Add(StoryConversations.Checkpoint(JourneyStory.WorldFirst(world)+1));
                    foreach(var scene in scenes){
                        game.ReviewConversation(scene);yield return null;
                        for(int turn=0;turn<scene.Lines.Length;turn++){
                            Assert.That(game.ConversationLine,Is.EqualTo(turn));var line=scene.Lines[turn];
                            Assert.That(Label("Conversation line").text,Is.EqualTo((line.Goal==null?new UiCopy(line.Text):new UiCopy(line.Text,new UiCopy(line.Goal))).ToString()));
                            Fits(Label("Speaker"));Fits(Label("Conversation line"));
                            PaddedActionFits((Button)Field("conversationNext"));PaddedActionFits((Button)Field("conversationSkip"));
                            if(turn+1<scene.Lines.Length){game.AdvanceConversation();game.AdvanceConversation();yield return null;}
                        }
                    }
                    yield return Shot(locale+"-map-dialogue-"+world,g=>Label("Conversation line").maxVisibleCharacters=int.MaxValue);
                }
            }
        }
        void MotionFrame(string name)
        {
            var pixels=Pixels();var image=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);
            try{
                image.SetPixels32(pixels);image.Apply();var folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/AtmosphereStoryPolishOct15/Transitions");Directory.CreateDirectory(folder);
                File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());
            }finally{UnityEngine.Object.DestroyImmediate(image);}
        }
        [UnityTest] public IEnumerator LayeredTransitionsKeepArchitectureAndTouchTargetsStable()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(var screen in new[]{GameScreen.Shop,GameScreen.Settings,GameScreen.Home,GameScreen.JourneyMap,GameScreen.Collection}){
                Route(screen);var transition=((RectTransform)Field("menuRoot")).GetComponent<MenuTransition>();Assert.That(transition.IsArriving,Is.True);
                var frame=game.GetComponentsInChildren<WorldFrameFit>().FirstOrDefault();var settingsFrame=game.GetComponentsInChildren<Image>().FirstOrDefault(i=>i.name=="Settings outer wooden frame");
                Transform architecture=frame!=null?frame.transform:settingsFrame.transform;Vector3 point=architecture.position,scale=architecture.lossyScale;
                for(int i=0;i<12;i++){
                    MotionFrame(screen+"-"+i.ToString("D2"));Assert.That(Vector3.Distance(architecture.position,point),Is.LessThan(.0001f));Assert.That(Vector3.Distance(architecture.lossyScale,scale),Is.LessThan(.0001f));
                    yield return new WaitForSecondsRealtime(.05f);
                }
                Assert.That(transition.IsArriving,Is.False);Assert.That(transition.GetComponent<CanvasGroup>().alpha,Is.EqualTo(1));
            }
            game.ShowHome();yield return null;
            var shop=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Shop");var corners=new Vector3[4];((RectTransform)shop.transform).GetWorldCorners(corners);
            var hits=new List<RaycastResult>();var center=camera.WorldToScreenPoint((corners[0]+corners[2])*.5f);EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=center},hits);
            Assert.That(hits.First().gameObject.GetComponentInParent<Button>(),Is.SameAs(shop),"Button remains usable during arrival");
            var feel=shop.GetComponent<ButtonFeel>();feel.OnPointerDown(new PointerEventData(EventSystem.current));yield return new WaitForSecondsRealtime(.1f);feel.OnPointerUp(new PointerEventData(EventSystem.current));
            yield return new WaitForSecondsRealtime(.5f);foreach(Transform child in shop.transform)Assert.That(child.localScale.x,Is.EqualTo(1).Within(.0001f),"Press and entrance must settle through the same artwork scale owner");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator RapidNavigationBackgroundingAndReducedMotionLeaveNoTransitionTails()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            for(int i=0;i<12;i++){Route(i%3==0?GameScreen.Shop:i%3==1?GameScreen.Settings:GameScreen.Home);yield return null;}
            var page=(RectTransform)Field("menuRoot");var transition=page.GetComponent<MenuTransition>();game.SendMessage("OnApplicationFocus",false);yield return null;
            Assert.That(transition.IsArriving,Is.False);Assert.That(page.GetComponent<CanvasGroup>().alpha,Is.EqualTo(1));
            game.SendMessage("OnApplicationFocus",true);game.ShowSettings();yield return null;game.SendMessage("OnApplicationPause",true);yield return null;
            Assert.That(transition.IsArriving,Is.False);game.SendMessage("OnApplicationPause",false);
            game.ReducedMotion=true;game.ShowHome();Assert.That(transition.IsArriving,Is.False);Assert.That(page.GetComponent<CanvasGroup>().alpha,Is.EqualTo(1));
            Assert.That(game.GetComponentsInChildren<RectTransform>().Count(t=>t.name=="Animated heading entrance"),Is.LessThanOrEqualTo(1));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator GameplayEffectsStopBehindMenusAndCannotEmitHiddenBirdCalls()
        {
            game.LoadNode("C02");game.ShowGame();for(int i=0;i<180&&!game.Interaction.CanPlay;i++)yield return null;Assert.That(game.Interaction.CanPlay,Is.True);
            string hash=game.Interaction.Attempt.CurrentStateHash;var world=game.Machine.LivingWorld;
            foreach(int zone in new[]{0,1,8}){
                game.ShowGame();world.SetZone(zone);world.Paint(14);Assert.That(world.ActiveWeatherCount,Is.GreaterThan(0));
                game.ShowHome();Assert.That(world.ActiveWeatherCount,Is.Zero);Assert.That(world.GetComponent<WorldLifeView>().ActiveCreatureCount,Is.Zero);
                world.Paint(20);Assert.That(world.ActiveWeatherCount,Is.Zero,"The next paint may not reactivate hidden 3D effects");
                foreach(var actor in world.GetComponentsInChildren<WildlifeAudioEmitter>().Where(a=>a.name.Contains("wildlife")||a.name.StartsWith("World visitor")))Assert.That(actor.TrySample(out _,out _),Is.False,"Hidden gameplay visitors cannot chirp behind menu artwork");
                yield return null;
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
    }
}
#endif
