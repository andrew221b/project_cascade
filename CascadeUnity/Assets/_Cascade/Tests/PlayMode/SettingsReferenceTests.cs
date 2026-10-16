#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class SettingsReferenceTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;string directory;Camera camera;RenderTexture portrait,oldTarget;
        bool oldBackground,oldAudioPause;
        PlayerProfile Profile=>(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
        Button Control(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name&&b.gameObject.activeInHierarchy);
        int Page=>(int)typeof(CascadeGame).GetField("settingsPage",Private).GetValue(game);
        [UnitySetUp] public IEnumerator Setup()
        {
            oldBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            oldAudioPause=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeSettingsReference",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            // The isolated Editor has no player Addressables cache. Warm the actual tables
            // before rapidly switching locales; only its known cold-preload diagnostic is expected.
            void PreloadLog(string message,string stack,LogType type){if(type==LogType.Error&&message.StartsWith("OperationException : Failed to initialize localization, could not preload string tables.",StringComparison.Ordinal))LogAssert.Expect(type,new System.Text.RegularExpressions.Regex("^OperationException : Failed to initialize localization, could not preload string tables\\."));}
            UnityEngine.Application.logMessageReceived+=PreloadLog;
            try{
                yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
                foreach(string locale in new[]{"en","ru","uk"}){
                    var table=UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
                    yield return table;Assert.That(table.Status,Is.EqualTo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded),locale+" table must load");
                }
            }finally{UnityEngine.Application.logMessageReceived-=PreloadLog;}
            portrait=new RenderTexture(720,1280,24);yield return Load();game.SetUiLocale("ru");
        }
        IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=portrait;scaler.enabled=true;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            Profile.AccountIntroSeen=true;game.SetEditorSafeInsets(new Vector4(0,.00001f,0,.00001f));
            game.ShowHome();yield return null;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            if(camera!=null)camera.targetTexture=oldTarget;
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}
            CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=oldBackground;AudioListener.pause=oldAudioPause;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Click(string name)
        {
            var button=Control(name);Assert.That(button.interactable,Is.True,name);button.onClick.Invoke();yield return null;
        }
        void CheckLayout(CascadeGame current)
        {
            Assert.That(current.CurrentScreen,Is.EqualTo(GameScreen.Settings));
            var stage=current.GetComponentsInChildren<SettingsReferenceLayout>().Single().GetComponent<RectTransform>();
            Assert.That(stage.localScale.x,Is.EqualTo(stage.localScale.y).Within(.0001f));
            Assert.That(current.GetComponentsInChildren<Image>().Count(i=>i.name=="Settings outer wooden frame"),Is.EqualTo(1));
            Assert.That(current.GetComponentsInChildren<Image>().Any(i=>i.name=="World-specific architectural frame"),Is.False,"The weather owner must not add a second architectural frame");
            var frame=current.GetComponentsInChildren<Image>().Single(i=>i.name=="Settings outer wooden frame");Assert.That(frame.material.GetFloat("_HaveMenuPosts"),Is.Zero);
            foreach(var t in current.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop settings"))){
                t.ForceMeshUpdate();Assert.That(t.isTextTruncated||t.isTextOverflowing,Is.False,t.name+": "+t.text);
                Assert.That(t.rectTransform.rect.height,Is.GreaterThan(0));
            }
            string[] expected=Page<0?new[]{"Back","Soundeffects","Comfort","Languages","Notifications","Remove ads","Privacy Policy","Terms of Use","Restore purchases"}:
                Page==0?new[]{"Back","Music","Soundeffects","Haptics"}:
                Page==1?new[]{"Back","Largertext","Tutorialhelp"}:new[]{"Back","GameLanguageen","GameLanguageru","GameLanguageuk"};
            Assert.That(current.GetComponentsInChildren<Button>().Select(b=>b.name),Is.EquivalentTo(expected));
            var regions=current.GetComponentsInChildren<Button>().Select(b=>{
                var corners=new Vector3[4];((RectTransform)b.transform).GetWorldCorners(corners);
                var low=camera.WorldToScreenPoint(corners[0]);var high=camera.WorldToScreenPoint(corners[2]);
                return (Name:b.name,Rect:new Rect(low.x,low.y,high.x-low.x,high.y-low.y));
            }).ToArray();
            foreach(var r in regions){
                Assert.That(r.Rect.width,Is.GreaterThanOrEqualTo(43.9f),r.Name);Assert.That(r.Rect.height,Is.GreaterThanOrEqualTo(43.9f),r.Name);
                Assert.That(r.Rect.xMin,Is.GreaterThanOrEqualTo(-.2f),r.Name);Assert.That(r.Rect.xMax,Is.LessThanOrEqualTo(camera.targetTexture.width+.2f),r.Name);
                Assert.That(r.Rect.yMin,Is.GreaterThanOrEqualTo(-.2f),r.Name);Assert.That(r.Rect.yMax,Is.LessThanOrEqualTo(camera.targetTexture.height+.2f),r.Name);
            }
            for(int i=0;i<regions.Length;i++)for(int j=i+1;j<regions.Length;j++)Assert.That(regions[i].Rect.Overlaps(regions[j].Rect),Is.False,regions[i].Name+" overlaps "+regions[j].Name);
            Assert.That(EventSystem.current,Is.Not.Null);
            foreach(var button in current.GetComponentsInChildren<Button>().Where(b=>b.interactable)){
                var region=regions.Single(r=>r.Name==button.name);
                var pointer=new PointerEventData(EventSystem.current){position=region.Rect.center};
                var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
                Assert.That(hits.Count,Is.GreaterThan(0),button.name+": no touch receiver");
                Assert.That(hits[0].gameObject.GetComponentInParent<Button>(),Is.SameAs(button),button.name+": artwork blocks the touch");
            }
            foreach(var arrow in current.GetComponentsInChildren<SettingsChevronGraphic>()){
                Assert.That(arrow.GetComponent<CanvasRenderer>(),Is.Not.Null);Assert.That(arrow.canvasRenderer.GetMesh().vertexCount,Is.GreaterThan(0));
            }
        }
        IEnumerator Shot(string name,int width,int height)
        {
            yield return new WaitForSecondsRealtime(.4f);
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable.PlayableCapture")).First(t=>t!=null);
            yield return (IEnumerator)type.GetMethod("CaptureSettled").Invoke(null,new object[]{"SettingsReferenceOct15",name+".png",width,height,(Action<CascadeGame>)CheckLayout});
        }
        [UnityTest] public IEnumerator FourReferencePagesFitPhoneTabletAndAllThreeLanguages()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            game.Preferences.LargeText=true;
            foreach(string locale in new[]{"ru","en","uk"}){
                game.SetGameLanguage(locale);
                var sizes=locale=="ru"?new[]{new Vector2Int(720,1280),new Vector2Int(390,844),new Vector2Int(360,640),new Vector2Int(768,1024)}:new[]{new Vector2Int(390,844)};
                foreach(var size in sizes){
                    string suffix=locale+"-"+size.x+"x"+size.y;game.ShowSettings();yield return Shot("01-main-"+suffix,size.x,size.y);
                    yield return Click("Soundeffects");yield return Shot("03-sound-"+suffix,size.x,size.y);yield return Click("Back");
                    yield return Click("Comfort");yield return Shot("04-comfort-"+suffix,size.x,size.y);yield return Click("Back");
                    yield return Click("Languages");yield return Shot("02-languages-"+suffix,size.x,size.y);yield return Click("Back");
                }
            }
            game.SetGameLanguage("ru");game.ShowSettings();game.SetEditorSafeInsets(new Vector4(0,.04f,0,.055f));
            yield return Shot("main-phone-safe-area",390,844);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator SoundComfortAndLanguageControlsApplyAndPersistAfterReload()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;game.ShowSettings();yield return Click("Soundeffects");
            yield return Click("Music");Assert.That(game.Preferences.Music||game.Feedback.MusicEnabled,Is.False);
            yield return Click("Soundeffects");Assert.That(game.Preferences.Sound||game.Feedback.SoundEnabled,Is.False);
            yield return Click("Haptics");Assert.That(game.Preferences.Haptics||game.Feedback.HapticsEnabled,Is.False);
            yield return Click("Back");yield return Click("Comfort");yield return Click("Largertext");yield return Click("Tutorialhelp");
            Assert.That(game.Preferences.LargeText,Is.True);Assert.That(game.Preferences.Tutorials,Is.False);Assert.That(game.Coach.Enabled,Is.False);
            yield return Click("Back");yield return Click("Languages");yield return Click("GameLanguageuk");
            Assert.That(game.UiLanguage,Is.EqualTo("uk"));Assert.That(game.Preferences.PuzzleLocale,Is.EqualTo("uk"));
            var selected=game.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Settings language radio")).ToArray();
            Assert.That(selected.Count(i=>i.sprite==SettingsReferenceArt.Icon(15)),Is.EqualTo(1));
            var saved=new ProfileStore(game.ProfilePath).Load().Profile;
            Assert.That(saved.Preferences.Music||saved.Preferences.Sound||saved.Preferences.Haptics||saved.Preferences.Tutorials,Is.False);
            Assert.That(saved.Preferences.LargeText,Is.True);Assert.That(saved.Preferences.UiLocale,Is.EqualTo("uk"));
            yield return Load();game.ShowSettings();yield return Click("Languages");
            Assert.That(game.UiLanguage,Is.EqualTo("uk"));Assert.That(game.Preferences.LargeText,Is.True);
            Assert.That(game.Preferences.Music||game.Preferences.Sound||game.Preferences.Haptics||game.Preferences.Tutorials,Is.False);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator LegalSubscriptionAndUnavailableNativeActionsPreserveThePuzzleAndWallet()
        {
            game.ShowSettings();string hash=game.Interaction.Attempt.CurrentStateHash;
            int spent=Profile.Monetization.SpentCoins,coins=Profile.Monetization.BonusCoins,grants=Profile.Monetization.Grants.Count;
            foreach(string document in new[]{"Privacy Policy","Terms of Use"}){
                yield return Click(document);Assert.That(game.GetComponentsInChildren<ScrollRect>().Any(r=>r.name=="Legal reading viewport"),Is.True);
                Assert.That(game.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Legal document text").text.Length,Is.GreaterThan(200));
                game.CloseLegalDocument();yield return null;
            }
            yield return Click("Remove ads");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.NoAds));yield return Click("Back");
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Settings));Assert.That(Page,Is.EqualTo(-1));
            Assert.That(Control("Restore purchases").interactable,Is.False,"Editor must not fake a connected production store");
            Control("Restore purchases").onClick.Invoke();yield return null;
            if(!game.LocalRemindersAvailable)Assert.That(Control("Notifications").interactable,Is.False);
            Assert.That(Profile.Monetization.SpentCoins,Is.EqualTo(spent));Assert.That(Profile.Monetization.BonusCoins,Is.EqualTo(coins));Assert.That(Profile.Monetization.Grants.Count,Is.EqualTo(grants));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator HeaderBackClosesSubpagesThenReturnsToTheOriginalScreen()
        {
            foreach(var origin in new[]{GameScreen.Home,GameScreen.JourneyMap,GameScreen.Pause}){
                typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{origin});game.ShowSettings();yield return null;
                foreach(string section in new[]{"Soundeffects","Comfort","Languages"}){
                    yield return Click(section);Assert.That(Page,Is.GreaterThanOrEqualTo(0));yield return Click("Back");Assert.That(Page,Is.EqualTo(-1));
                }
                yield return Click("Back");Assert.That(game.CurrentScreen,Is.EqualTo(origin));
            }
        }
    }
}
#endif
