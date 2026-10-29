#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Content;
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
    public sealed class ConversationReferenceTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture target,oldTarget;string directory;bool background,audio;
        object Field(string name)=>typeof(CascadeGame).GetField(name,Hidden).GetValue(game);
        object Call(string name,params object[] values)=>typeof(CascadeGame).GetMethod(name,Hidden|BindingFlags.Public).Invoke(game,values);
        TextMeshProUGUI Label(string name)=>game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name==name);
        static Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeConversationReference",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;
            Resize(720,1280);game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Preferences.Tutorials=false;game.Coach.Enabled=false;game.Preferences.EffectQuality=2;game.ReducedMotion=true;
            var profile=(PlayerProfile)Field("profile");profile.AccountIntroSeen=true;profile.Missions.NextAfterClears=20000;
            ((HashSet<int>)Field("heardStory")).UnionWith(Enumerable.Range(0,10000));game.ShowJourney();yield return null;
        }
        void Resize(int width,int height)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}target=new RenderTexture(width,height,24);camera.targetTexture=target;scaler.enabled=true;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}
            CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=background;AudioListener.pause=audio;if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        void Open(int world,StoryConversation scene,UiCopy? final=null,Action done=null,Action dismiss=null)
        {
            if(game.ConversationOpen)Call("CancelConversation");EditorType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});game.ShowJourney();
            Call("OpenConversation",scene,final??new UiCopy("To the map"),done??(Action)(()=>{}),dismiss??(Action)(()=>{}));
        }
        IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(.35f);Label("Conversation line").maxVisibleCharacters=int.MaxValue;Canvas.ForceUpdateCanvases();
            yield return (IEnumerator)EditorType("PlayableCapture").GetMethod("CaptureSettled").Invoke(null,new object[]{"ConversationReferenceOct16",name+".png",target.width,target.height,(Action<CascadeGame>)(_=>Verify(name))});
        }
        static Rect Ink(TextMeshProUGUI label,RectTransform area)
        {
            label.ForceMeshUpdate();var b=CenteredContentRow.GlyphBounds(label);var a=(Vector2)area.InverseTransformPoint(label.transform.TransformPoint(b.min));var z=(Vector2)area.InverseTransformPoint(label.transform.TransformPoint(b.max));return Rect.MinMaxRect(a.x,a.y,z.x,z.y);
        }
        void Verify(string name)
        {
            Call("RefreshConversationLayout");Label("Conversation line").maxVisibleCharacters=int.MaxValue;Canvas.ForceUpdateCanvases();
            foreach(var label in new[]{Label("Speaker"),Label("Conversation line"),Label("Conversation progress")}){
                var area=label.GetComponentInParent<CenteredContentRow>()?.Surface??label.rectTransform;var b=Ink(label,area);
                Assert.That(label.isTextTruncated,Is.False,name+" / "+label.name);Assert.That(b.xMin,Is.GreaterThanOrEqualTo(area.rect.xMin-1),name+" left "+label.name);Assert.That(b.xMax,Is.LessThanOrEqualTo(area.rect.xMax+1),name+" right "+label.name);Assert.That(b.yMin,Is.GreaterThanOrEqualTo(area.rect.yMin-1),name+" bottom "+label.name);Assert.That(b.yMax,Is.LessThanOrEqualTo(area.rect.yMax+1),name+" top "+label.name);Assert.That(label.color.maxColorComponent,Is.LessThan(.5f));
            }
            var stage=game.GetComponentInChildren<ConversationReferenceLayout>();Assert.That(stage.World,Is.EqualTo(game.EnvironmentWorld));
            var hero=(Image)Field("conversationPortrait");Assert.That(hero.GetComponent<CharacterPoseView>().Profile,Is.Not.Null);
            var paper=(RectTransform)Field("conversationPlate");var portrait=(RectTransform)Field("conversationBust");Assert.That(-portrait.anchoredPosition.y-portrait.rect.height*.5f,Is.GreaterThanOrEqualTo(0),name+" hero stays below safe top");
            Assert.That(portrait.anchoredPosition.y-portrait.rect.height*.5f,Is.LessThan(paper.anchoredPosition.y+paper.rect.height*.5f),"Portrait meets the paper");
            Assert.That(game.GetComponentsInChildren<Image>().Any(i=>i.name.Contains("lock",StringComparison.OrdinalIgnoreCase)),Is.False,"No fake locked islands");
            foreach(string key in new[]{"conversationNext","conversationSkip"}){
                var button=(Button)Field(key);var r=(RectTransform)button.transform;var safe=(RectTransform)Field("conversationSafe");var corners=new Vector3[4];r.GetWorldCorners(corners);float height=Vector2.Distance(RectTransformUtility.WorldToScreenPoint(camera,corners[0]),RectTransformUtility.WorldToScreenPoint(camera,corners[1]));Assert.That(height,Is.GreaterThanOrEqualTo(43.8f),name+" "+key+" hit="+height+" rect="+r.sizeDelta+" scale="+r.lossyScale+" unit="+CenteredContentRow.Unit(r)+" canvas="+r.GetComponentInParent<Canvas>().scaleFactor);
                Assert.That(safe.InverseTransformPoint(corners[0]).y,Is.GreaterThanOrEqualTo(safe.rect.yMin-.5f));
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(camera,r.TransformPoint(r.rect.center))},hits);Assert.That(hits.FirstOrDefault().gameObject?.GetComponentInParent<Button>(),Is.SameAs(button),key+" actual hit");
                var group=button.GetComponentInChildren<CenteredContentRow>();var label=button.GetComponentInChildren<TextMeshProUGUI>();var b=Ink(label,group.Surface);Assert.That(b.center.x,Is.EqualTo(CenteredContentRow.ReadingRect(group.Surface).center.x).Within(.8f*group.LocalUnit));Assert.That(b.center.y,Is.EqualTo(CenteredContentRow.ReadingRect(group.Surface).center.y).Within(.8f*group.LocalUnit));
            }
        }
        [UnityTest] public IEnumerator ReferencePreview()
        {
            game.SetUiLocale("ru");game.SetEditorSafeInsets(Vector4.zero);Open(0,StoryConversations.IslandOpening(0,0));yield return Shot("reference-walter-ru-720");Open(8,StoryConversations.IslandOpening(8,0));yield return Shot("reference-stars-ru-720");Open(6,StoryConversations.IslandOpening(6,0));yield return Shot("reference-lumi-complete-ru-720");Resize(390,844);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));Open(6,StoryConversations.IslandOpening(6,0));yield return Shot("world-6-ru-390");
        }
        [UnityTest,Timeout(300000)] public IEnumerator AllWorldsInvitationsAndAuthoredTurnsFitPhoneLocales()
        {
            var names=(string[])typeof(CascadeGame).GetField("MissionResidents",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);var invitations=(string[])typeof(CascadeGame).GetField("MissionInvitations",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
            foreach(string locale in new[]{"en","ru","uk"})foreach(int width in new[]{320,390,430}){
                Resize(width,width==320?693:width==430?932:844);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));game.Preferences.LargeText=width!=390;game.SetUiLocale(locale);
                for(int world=0;world<9;world++){
                    var scene=StoryConversations.IslandOpening(world,0);Open(world,scene);yield return null;Verify(locale+"-"+width+"-"+world);yield return Shot("world-"+world+"-"+locale+"-"+width);
                    for(int turn=0;turn<scene.Lines.Length;turn++){Verify("opening "+locale);Assert.That(game.ConversationLine,Is.EqualTo(turn));if(locale=="ru"&&width==390&&world==0&&turn==1)yield return Shot("mika-response-ru-390");if(turn+1<scene.Lines.Length){game.AdvanceConversation();yield return null;}}
                    var mission=new StoryConversation(-1,new StoryLine(names[world],invitations[world],"Mend the boards and bring the row home."));Open(world,mission,"Accept");yield return null;Verify("mission "+locale+"-"+width+"-"+world);Assert.That(Label("Conversation line").text,Is.EqualTo(new UiCopy(invitations[world],new UiCopy("Mend the boards and bring the row home.")).ToString()));
                }
            }
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(768,1024)}){Resize(size.x,size.y);Open(7,StoryConversations.Island(7,4));yield return Shot("compact-uk-"+size.x);}
        }
        [UnityTest,Timeout(300000)] public IEnumerator EveryNarrativeExchangeFitsSmallPhonesInAllLocales()
        {
            var names=(string[])typeof(CascadeGame).GetField("MissionResidents",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
            var invitations=(string[])typeof(CascadeGame).GetField("MissionInvitations",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
            var goal=typeof(CascadeGame).GetMethod("MissionGoal",BindingFlags.Static|BindingFlags.NonPublic);
            var goals=Enum.GetValues(typeof(Cascade.Application.MechanicLesson)).Cast<Cascade.Application.MechanicLesson>()
                .Select(lesson=>(string)goal.Invoke(null,new object[]{lesson})).Distinct().ToArray();
            foreach(string locale in new[]{"en","ru","uk"})foreach(int width in new[]{320,390}){
                Resize(width,width==320?693:844);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));
                game.Preferences.LargeText=width==320;game.SetUiLocale(locale);
                for(int world=0;world<9;world++){
                    var scenes=new List<StoryConversation>();
                    for(int island=0;island<5;island++){
                        scenes.Add(StoryConversations.IslandOpening(world,island));
                        scenes.Add(StoryConversations.IslandProgress(JourneyStory.IslandFirst(world,island)+5));
                        scenes.Add(StoryConversations.Island(world,island));
                    }
                    scenes.Add(StoryConversations.Checkpoint(JourneyStory.WorldFirst(world)+1));
                    scenes.Add(StoryConversations.LevelAside(JourneyStory.IslandFirst(world,3)+1));
                    for(int index=0;index<scenes.Count;index++){
                        var scene=scenes[index];Open(world,scene);yield return null;
                        for(int turn=0;turn<scene.Lines.Length;turn++){
                            string key=locale+" / "+width+" / world "+world+" / scene "+index+" / turn "+turn;
                            Assert.That(Label("Conversation line").text,Is.EqualTo(new UiCopy(scene.Lines[turn].Text).ToString()),key);
                            Verify(key);
                            if(locale=="ru"&&width==390&&world==8&&index==9&&turn==0)yield return Shot("late-admission-ru-390");
                            if(turn+1<scene.Lines.Length){game.AdvanceConversation();yield return null;}
                        }
                    }
                }
                int longest=Enumerable.Range(0,invitations.Length).OrderByDescending(i=>new UiCopy(invitations[i],new UiCopy(goals[0])).ToString().Length).First();
                foreach(string task in goals){
                    Open(longest,new StoryConversation(-1,new StoryLine(names[longest],invitations[longest],task)),"Accept");yield return null;
                    Assert.That(Label("Conversation line").text,Is.EqualTo(new UiCopy(invitations[longest],new UiCopy(task)).ToString()));
                    Verify("mission variant "+locale+" / "+width+" / "+task);
                }
                if(locale=="ru")yield return Shot("mission-variant-ru-"+width);
            }
        }
        [UnityTest] public IEnumerator CompactPhoneRetainsItsTouchTargets()
        {
            Resize(320,568);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));game.SetUiLocale("uk");game.Preferences.LargeText=true;Open(7,StoryConversations.Island(7,4));yield return Shot("compact-touch-uk-320");
        }
        [UnityTest] public IEnumerator SkipNextAndCancellationKeepCallbacksPauseAndMap()
        {
            game.SetUiLocale("ru");bool done=false,skip=false;string hash=game.Interaction.Attempt.CurrentStateHash;
            var scene=new StoryConversation(-1,new StoryLine("Bram","First lamp lit. I'll sign the anchor. I've checked it often enough to know it."),new StoryLine("Mika","We'll ask him when we get the market route open."));
            Open(0,scene,"Accept",()=>done=true,()=>skip=true);yield return null;Assert.That(((RectTransform)Field("menuRoot")).gameObject.activeSelf,Is.False);Assert.That(game.Interaction.IsPaused,Is.True);
            game.ReducedMotion=false;Call("PaintConversationLine");((Button)Field("conversationNext")).onClick.Invoke();Assert.That(game.ConversationLine,Is.Zero);Assert.That(Label("Conversation line").maxVisibleCharacters,Is.EqualTo(int.MaxValue));
            ((Button)Field("conversationNext")).onClick.Invoke();yield return null;Assert.That(game.ConversationLine,Is.EqualTo(1));Label("Conversation line").maxVisibleCharacters=int.MaxValue;((Button)Field("conversationNext")).onClick.Invoke();Assert.That(done,Is.True);Assert.That(skip,Is.False);Assert.That(game.ConversationOpen,Is.False);Assert.That(((RectTransform)Field("menuRoot")).gameObject.activeSelf,Is.True);
            Open(0,scene,"Accept",()=>done=true,()=>skip=true);yield return null;((Button)Field("conversationSkip")).onClick.Invoke();Assert.That(skip,Is.True);Assert.That(game.ConversationOpen,Is.False);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator SpeechPortraitsAndRootedSceneryAnimateAndRespectFocus()
        {
            Resize(390,844);game.SetUiLocale("ru");game.ReducedMotion=false;Open(0,StoryConversations.IslandOpening(0,0));yield return new WaitForSecondsRealtime(.4f);
            var root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/ConversationReferenceOct16/frames"));Directory.CreateDirectory(root);var pixels=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);var active=RenderTexture.active;float peak=0,blink=0;
            for(int i=0;i<240;i++){
                if(i==120){Open(2,StoryConversations.IslandOpening(2,0));yield return new WaitForSecondsRealtime(.4f);}
                var composition=game.GetComponentInChildren<ConversationReferenceLayout>();composition.GetComponentInParent<MenuWorldEnvironment>().SetReviewAge(16+i%120/30f);composition.PaintNow();var view=((Image)Field("conversationPortrait")).GetComponent<CharacterPoseView>();if(i==0||i==120)view.ResetReview(263);Label("Conversation line").maxVisibleCharacters=int.MaxValue;view.AdvanceReview(1/30f,.35f,CharacterPoseView.SpeechShape(Label("Conversation line").text,i%120/30f));peak=Mathf.Max(peak,view.Motion.MouthOpen);blink=Mathf.Max(blink,view.Motion.Blink);camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(root,i.ToString("D4")+".jpg"),pixels.EncodeToJPG(87));
            }
            UnityEngine.Object.Destroy(pixels);RenderTexture.active=active;Assert.That(peak,Is.GreaterThan(.8f));Assert.That(blink,Is.GreaterThan(.8f));var last=((Image)Field("conversationPortrait")).GetComponent<CharacterPoseView>();last.EndReview();var env=game.GetComponentInChildren<ConversationReferenceLayout>().GetComponentInParent<MenuWorldEnvironment>();float time=env.AnimationAge;game.SendMessage("OnApplicationFocus",false);yield return new WaitForSecondsRealtime(.2f);Assert.That(env.AnimationAge,Is.EqualTo(time));game.SendMessage("OnApplicationFocus",true);game.ReducedMotion=true;yield return null;time=env.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(env.AnimationAge,Is.EqualTo(time));
        }
    }
}
#endif
