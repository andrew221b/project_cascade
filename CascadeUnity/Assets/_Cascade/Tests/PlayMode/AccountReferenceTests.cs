#if UNITY_EDITOR
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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class AccountReferenceTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture portrait,oldTarget;string directory;
        bool oldBackground,oldAudio;
        PlayerProfile Profile=>(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
        Button Control(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name&&b.gameObject.activeInHierarchy);
        object Field(string name)=>typeof(CascadeGame).GetField(name,Private).GetValue(game);
        [UnitySetUp] public IEnumerator Setup()
        {
            CascadeGame.EditorSystemLanguage=null;
            oldBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            oldAudio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeAccountReference",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            void PreloadLog(string message,string stack,LogType type){if(type==LogType.Error&&message.StartsWith("OperationException : Failed to initialize localization, could not preload string tables.",StringComparison.Ordinal))LogAssert.Expect(type,new System.Text.RegularExpressions.Regex("^OperationException : Failed to initialize localization, could not preload string tables\\."));}
            UnityEngine.Application.logMessageReceived+=PreloadLog;
            try{
                yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
                foreach(string code in new[]{"en","ru","uk"}){
                    var table=UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(code));
                    yield return table;Assert.That(table.Status,Is.EqualTo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded));
                }
            }finally{UnityEngine.Application.logMessageReceived-=PreloadLog;}
            portrait=new RenderTexture(720,1280,24);yield return Load();game.SetUiLocale("ru");
        }
        IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=portrait;scaler.enabled=true;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.SetEditorSafeInsets(new Vector4(0,.00001f,0,.00001f));
            yield return null;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            if(camera!=null)camera.targetTexture=oldTarget;
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}
            CascadeGame.EditorProfileDirectory=null;AudioListener.pause=oldAudio;UnityEngine.Application.runInBackground=oldBackground;
            CascadeGame.EditorSystemLanguage=null;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Click(string name){var button=Control(name);Assert.That(button.interactable,Is.True,name);button.onClick.Invoke();yield return null;}
        Rect ScreenRect(RectTransform rect)
        {
            var corners=new Vector3[4];rect.GetWorldCorners(corners);var lo=camera.WorldToScreenPoint(corners[0]);var hi=camera.WorldToScreenPoint(corners[2]);return new Rect(lo.x,lo.y,hi.x-lo.x,hi.y-lo.y);
        }
        void Check(CascadeGame current)
        {
            foreach(var label in current.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop")||t.name=="Home reference caption")){
                label.ForceMeshUpdate();Assert.That(label.isTextOverflowing||label.isTextTruncated,Is.False,label.name+": "+label.text);
            }
            var layout=current.GetComponentsInChildren<HomeReferenceLayout>().FirstOrDefault();
            if(layout!=null){
                Assert.That(layout.transform.localScale.x,Is.EqualTo(layout.transform.localScale.y).Within(.0001f));
                foreach(var art in current.GetComponentsInChildren<Image>().Where(i=>i.name=="Mika waves hello"||i.name=="Mika welcomes you"||i.name.Contains("illustrated logo")))Assert.That(art.preserveAspect,Is.True,art.name);
                var ground=current.GetComponentsInChildren<RawImage>().Single(i=>i.name=="Home continuous reference ground");var extent=ScreenRect(ground.rectTransform);
                Assert.That(extent.xMin,Is.EqualTo(0).Within(.2f));Assert.That(extent.xMax,Is.EqualTo(camera.targetTexture.width).Within(.2f));Assert.That(extent.yMin,Is.EqualTo(0).Within(.2f));
                Assert.That(ground.uvRect,Is.EqualTo(new Rect(0,0,1,1)),"The complete floor must reach both side posts without horizontal cropping");
                using(var mesh=new VertexHelper()){
                    ground.GetComponent<HomeGroundMesh>().ModifyMesh(mesh);Assert.That(mesh.currentVertCount,Is.EqualTo(12));
                    foreach(int index in new[]{0,8}){
                        var a=new UIVertex();var b=new UIVertex();var c=new UIVertex();mesh.PopulateUIVertex(ref a,index);mesh.PopulateUIVertex(ref b,index+1);mesh.PopulateUIVertex(ref c,index+2);
                        float xScale=(c.position.x-b.position.x)/(ground.texture.width*(c.uv0.x-b.uv0.x));
                        float yScale=(b.position.y-a.position.y)/(ground.texture.height*(b.uv0.y-a.uv0.y));
                        Assert.That(xScale,Is.EqualTo(yScale).Within(.001f),"Paving and platform corners must retain their source proportions");
                    }
                }
            }
            var buttons=current.GetComponentsInChildren<Button>().Where(b=>b.interactable).ToArray();
            var regions=buttons.Select(b=>(Button:b,Rect:ScreenRect((RectTransform)b.transform))).ToArray();
            foreach(var r in regions){
                Assert.That(r.Rect.width,Is.GreaterThanOrEqualTo(43.9f),r.Button.name);Assert.That(r.Rect.height,Is.GreaterThanOrEqualTo(43.9f),r.Button.name);
                Assert.That(r.Rect.xMin,Is.GreaterThanOrEqualTo(-.2f),r.Button.name);Assert.That(r.Rect.xMax,Is.LessThanOrEqualTo(camera.targetTexture.width+.2f),r.Button.name);
                Assert.That(r.Rect.yMin,Is.GreaterThanOrEqualTo(-.2f),r.Button.name);Assert.That(r.Rect.yMax,Is.LessThanOrEqualTo(camera.targetTexture.height+.2f),r.Button.name);
                var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=r.Rect.center},hits);
                Assert.That(hits.Count,Is.GreaterThan(0));Assert.That(hits[0].gameObject.GetComponentInParent<Button>(),Is.SameAs(r.Button),r.Button.name+": raycast blocked");
            }
            for(int i=0;i<regions.Length;i++)for(int j=i+1;j<regions.Length;j++)Assert.That(regions[i].Rect.Overlaps(regions[j].Rect),Is.False,regions[i].Button.name+" overlaps "+regions[j].Button.name);
            if(current.CurrentScreen==GameScreen.Home){
                var account=ScreenRect((RectTransform)current.GetComponentsInChildren<Button>().Single(b=>b.name=="Sign in"||b.name=="Sign out").transform);
                var ads=ScreenRect((RectTransform)Control("Remove ads shortcut").transform);Assert.That(account.center.y,Is.EqualTo(ads.center.y).Within(.2f));
            }
            if(current.CurrentScreen==GameScreen.Playing){
                var left=ScreenRect((RectTransform)((Button)Field("pause")).transform);var right=ScreenRect((RectTransform)((Button)Field("hint")).transform);
                var header=(RectTransform)Field("header");var plaque=ScreenRect((RectTransform)header.Find("Carved chapter plaque"));
                Assert.That(plaque.center.x,Is.EqualTo((left.xMax+right.xMin)*.5f).Within(.2f),"Heading must centre between visible controls");
                Assert.That(left.center.y,Is.EqualTo(right.center.y).Within(.2f));Assert.That(plaque.center.y,Is.EqualTo(left.center.y).Within(.2f));
            }
            if(current.CurrentScreen==GameScreen.Pause){
                Assert.That(ScreenRect((RectTransform)Control("Home").transform).center.y,Is.EqualTo(ScreenRect((RectTransform)Control("Settings").transform).center.y).Within(.2f));
                var title=current.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Reference shop pause heading");var board=current.GetComponentsInChildren<Image>().Single(i=>i.name=="Pause wooden title board");
                title.ForceMeshUpdate();var paintedCentre=RectTransformUtility.WorldToScreenPoint(camera,title.transform.TransformPoint(title.textBounds.center));
                Assert.That(Vector2.Distance(paintedCentre,ScreenRect(board.rectTransform).center),Is.LessThan(.2f));
            }
        }
        IEnumerator Shot(string name,int width,int height)
        {
            yield return new WaitForSecondsRealtime(.5f);
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable.PlayableCapture")).First(t=>t!=null);
            string hash=game.Interaction.Attempt.CurrentStateHash;
            Action<CascadeGame> verify=current=>{if(name.Contains("linked-visual-fixture"))typeof(CascadeGame).GetMethod("ApplyHomeAccountShortcutState",Private).Invoke(current,new object[]{true,true});Check(current);};
            string phase=name.StartsWith("welcome-life",StringComparison.Ordinal)||name.StartsWith("welcome-system",StringComparison.Ordinal)?"WelcomeLifeLanguageOct16":"AccountReferenceOct15";
            yield return (IEnumerator)type.GetMethod("CaptureSettled").Invoke(null,new object[]{phase,name+".png",width,height,verify});
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [Test] public void EveryUnsupportedDeviceLanguageFallsBackToEnglish()
        {
            var select=typeof(CascadeGame).GetMethod("SystemUiLocale",BindingFlags.Static|BindingFlags.NonPublic);
            foreach(SystemLanguage language in Enum.GetValues(typeof(SystemLanguage))){
                string expected=language==SystemLanguage.Russian?"ru":language==SystemLanguage.Ukrainian?"uk":"en";
                Assert.That(select.Invoke(null,new object[]{language}),Is.EqualTo(expected),language.ToString());
            }
        }
        [UnityTest] public IEnumerator DeviceLanguageAppliesBeforeWelcomeAndManualEnglishSurvivesReload()
        {
            foreach(var system in new[]{SystemLanguage.Russian,SystemLanguage.Ukrainian,SystemLanguage.French}){
                game.Preferences.FollowSystemLanguage=true;game.Preferences.UiLocale=game.Preferences.PuzzleLocale="en";
                Profile.AccountIntroSeen=false;CascadeGame.EditorSystemLanguage=system;yield return Load();
                string expected=system==SystemLanguage.Russian?"ru":system==SystemLanguage.Ukrainian?"uk":"en";
                Assert.That(game.UiLanguage,Is.EqualTo(expected));Assert.That(game.Preferences.PuzzleLocale,Is.EqualTo(expected));
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.AccountGate));
                var title=game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Reference shop account heading");
                Assert.That(title.text,Is.EqualTo(expected=="ru"?"Добро пожаловать!":expected=="uk"?"Ласкаво просимо!":"Welcome!"));
                yield return Shot("welcome-system-"+expected,390,844);
                typeof(CascadeGame).GetMethod("PersistProgress",Private).Invoke(game,new object[]{true});
                var saved=new ProfileStore(game.ProfilePath).Load();Assert.That(saved.Recovery,Is.EqualTo(SaveRecovery.None));
                Assert.That(saved.Profile.Preferences.FollowSystemLanguage,Is.True);Assert.That(saved.Profile.Preferences.UiLocale,Is.EqualTo(expected));
            }
            game.SetGameLanguage("en");CascadeGame.EditorSystemLanguage=SystemLanguage.Russian;yield return Load();
            Assert.That(game.UiLanguage,Is.EqualTo("en"));Assert.That(game.Preferences.PuzzleLocale,Is.EqualTo("en"));Assert.That(game.Preferences.FollowSystemLanguage,Is.False);
        }
        [UnityTest] public IEnumerator LegacyLanguageChoicesAndManualUkrainianSurviveReload()
        {
            foreach(string legacy in new[]{"ru","uk","en"}){
                game.Preferences.FollowSystemLanguage=null;game.Preferences.UiLocale=game.Preferences.PuzzleLocale=legacy;
                CascadeGame.EditorSystemLanguage=SystemLanguage.Ukrainian;yield return Load();
                Assert.That(game.UiLanguage,Is.EqualTo(legacy=="en"?"uk":legacy));
                Assert.That(game.Preferences.FollowSystemLanguage,Is.EqualTo(legacy=="en"));
            }
            game.SetGameLanguage("uk");CascadeGame.EditorSystemLanguage=SystemLanguage.English;yield return Load();
            Assert.That(game.UiLanguage,Is.EqualTo("uk"));Assert.That(game.Preferences.PuzzleLocale,Is.EqualTo("uk"));Assert.That(game.Preferences.FollowSystemLanguage,Is.False);
        }
        Color32[] WelcomePixels()
        {
            var before=RenderTexture.active;var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGBA32,false);
            try{Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=portrait;pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();return pixels.GetPixels32();}
            finally{RenderTexture.active=before;UnityEngine.Object.DestroyImmediate(pixels);}
        }
        int PaintedPixels(Graphic[] graphics)
        {
            var flags=graphics.Select(g=>g.enabled).ToArray();var shown=WelcomePixels();Color32[] hidden;
            try{foreach(var graphic in graphics)graphic.enabled=false;hidden=WelcomePixels();}
            finally{for(int i=0;i<graphics.Length;i++)graphics[i].enabled=flags[i];}
            int count=0;for(int i=0;i<shown.Length;i++)if(Math.Abs(shown[i].r-hidden[i].r)+Math.Abs(shown[i].g-hidden[i].g)+Math.Abs(shown[i].b-hidden[i].b)>24)count++;
            return count;
        }
        [UnityTest] public IEnumerator WelcomeVisitorsArriveAndAnimateOnTheActualPhoneFrame()
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;portrait.Release();portrait.width=390;portrait.height=844;portrait.Create();scaler.enabled=true;
            game.Preferences.EffectQuality=2;game.ReducedMotion=false;game.Preferences.LargeText=false;game.ShowAccountGate();yield return null;game.RefreshLayout();
            var environment=game.GetComponentsInChildren<MenuWorldEnvironment>().Single();var life=game.GetComponentsInChildren<HomeSkyLife>().Single();
            var ships=environment.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Distant sky aircraft ",StringComparison.Ordinal)).ToArray();
            // The environment is attached to the menu root; aircraft live in its sky child.
            if(ships.Length==0)ships=game.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Distant sky aircraft ",StringComparison.Ordinal)).ToArray();
            var ship=ships.Single(i=>i.name=="Distant sky aircraft 3");Assert.That(ship.enabled,Is.True);Assert.That(PaintedPixels(new Graphic[]{ship}),Is.GreaterThan(20),"The welcome ship must paint visible sky pixels immediately");
            var start=ship.rectTransform.anchoredPosition;string hash=game.Interaction.Attempt.CurrentStateHash;var bird=(RectTransform)life.transform.Find("Distant bird");int visible=0;
            float until=Time.realtimeSinceStartup+9;
            while(Time.realtimeSinceStartup<until){yield return new WaitForSecondsRealtime(.2f);if(!bird.GetComponentsInChildren<Image>().Any(i=>i.enabled))continue;visible=PaintedPixels(bird.GetComponentsInChildren<Graphic>());if(visible>12)break;}
            Assert.That(visible,Is.GreaterThan(12),"A bird must be visibly flying in the opening seconds, not hidden behind the hero or logo");
            Assert.That(Vector2.Distance(start,ship.rectTransform.anchoredPosition),Is.GreaterThan(.1f));
            var birdStart=bird.anchoredPosition;var wing=bird.Find("Near wing joint");var wingStart=wing.localRotation;
            yield return Shot("welcome-life-bird-and-ship-00",390,844);
            for(int i=1;i<7;i++)yield return Shot("welcome-life-bird-and-ship-"+i.ToString("D2"),390,844);
            Assert.That(Vector2.Distance(birdStart,bird.anchoredPosition),Is.GreaterThan(1));Assert.That(Quaternion.Angle(wingStart,wing.localRotation),Is.GreaterThan(.05f));
            game.SendMessage("OnApplicationFocus",false);float held=environment.AnimationAge;var point=bird.anchoredPosition;yield return new WaitForSecondsRealtime(.2f);
            Assert.That(environment.AnimationAge,Is.EqualTo(held));Assert.That(bird.anchoredPosition,Is.EqualTo(point));
            game.SendMessage("OnApplicationFocus",true);game.ReducedMotion=true;yield return null;Assert.That(environment.VisibleAircraft,Is.Zero);Assert.That(life.GetComponentsInChildren<Image>().All(i=>!i.enabled),Is.True);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator WelcomeHomeAndExistingHeadersFitPhonesAndLocales()
        {
            game.Preferences.LargeText=true;game.Preferences.Tutorials=false;game.Coach.Enabled=false;
            foreach(string locale in new[]{"ru","en","uk"}){
                game.SetUiLocale(locale);var sizes=locale=="ru"?new[]{new Vector2Int(720,1280),new Vector2Int(390,844),new Vector2Int(320,693),new Vector2Int(768,1024)}:new[]{new Vector2Int(390,844)};
                foreach(var size in sizes){
                    string suffix=locale+"-"+size.x+"x"+size.y;Profile.AccountIntroSeen=false;game.ShowAccountGate();yield return Shot("01-welcome-"+suffix,size.x,size.y);
                    yield return Click("Continue without an account");yield return Shot("02-home-guest-"+suffix,size.x,size.y);
                    // Presentation fixture only. Actual sign-in/sign-out authority is exercised
                    // against the real CloudSyncClient by Tools/AccountFlowChecks.
                    typeof(CascadeGame).GetMethod("ApplyHomeAccountShortcutState",Private).Invoke(game,new object[]{true,true});yield return Shot("03-home-linked-visual-fixture-"+suffix,size.x,size.y);
                    game.ShowGame();yield return Shot("04-gameplay-header-"+suffix,size.x,size.y);game.SendMessage("TogglePause");yield return Shot("05-pause-"+suffix,size.x,size.y);
                    if(!Profile.Monetization.Cosmetics.Contains("wind_chime"))Profile.Monetization.Cosmetics.Add("wind_chime");game.ShowCollection();yield return Shot("06-trophies-"+suffix,size.x,size.y);
                }
            }
            game.SetUiLocale("ru");game.SetEditorSafeInsets(new Vector4(0,.04f,0,.055f));Profile.AccountIntroSeen=false;game.ShowAccountGate();yield return Shot("07-welcome-phone-safe",390,844);
            yield return Click("Continue without an account");yield return Shot("08-home-phone-safe",390,844);
        }
        [UnityTest] public IEnumerator GuestChoiceAndUnavailableProvidersKeepProgressAndPersist()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;int coins=game.ShopCoins;Profile.AccountIntroSeen=false;game.ShowAccountGate();
            Assert.That(game.GetComponentsInChildren<Button>().Select(b=>b.name),Is.EquivalentTo(new[]{"Sign in with Apple","Sign in with Google","Continue without an account"}));
            foreach(string provider in new[]{"Apple","Google"}){
                yield return Click("Sign in with "+provider);Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.AccountGate));Assert.That(Profile.AccountIntroSeen,Is.False);
                Assert.That(Control("Close account notice").interactable,Is.True);yield return Click("Close account notice");Assert.That(Control("Sign in with "+provider).interactable,Is.True);
            }
            yield return Click("Continue without an account");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That(Profile.AccountIntroSeen,Is.True);
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.ShopCoins,Is.EqualTo(coins));
            yield return Click("Sign in");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.AccountGate));yield return Click("Back");Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            yield return Load();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That(Profile.AccountIntroSeen,Is.True);
        }
        [UnityTest] public IEnumerator FailedGuestSaveCannotDismissWelcome()
        {
            Profile.AccountIntroSeen=false;game.ShowAccountGate();string hash=game.Interaction.Attempt.CurrentStateHash;
            var store=(ProfileStore)Field("profileStore");var path=typeof(ProfileStore).GetField("path",Private);string original=(string)path.GetValue(store);
            string blocked=Path.Combine(directory,"not-a-file");Directory.CreateDirectory(blocked);path.SetValue(store,blocked);
            yield return Click("Continue without an account");Assert.That(Profile.AccountIntroSeen,Is.False);Assert.That(game.CurrentScreen,Is.Not.EqualTo(GameScreen.Home));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));path.SetValue(store,original);
        }
        [UnityTest] public IEnumerator WelcomeAircraftMovesWithoutBlockingInputOrChangingThePuzzle()
        {
            game.Preferences.EffectQuality=2;game.ReducedMotion=false;game.ShowAccountGate();
            var environment=game.GetComponentsInChildren<MenuWorldEnvironment>().Single();
            for(float age=4;age<120;age+=.25f){environment.SetReviewAge(age);if(environment.VisibleAircraft>0)break;}
            var aircraft=game.GetComponentsInChildren<Image>().First(i=>i.name.StartsWith("Distant sky aircraft ",StringComparison.Ordinal)&&i.enabled&&i.color.a>.05f);environment.EndReview();
            Assert.That(aircraft.sprite,Is.Not.Null);Assert.That(aircraft.raycastTarget,Is.False);Assert.That(aircraft.preserveAspect,Is.True);
            string hash=game.Interaction.Attempt.CurrentStateHash;var before=aircraft.rectTransform.anchoredPosition;yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Vector2.Distance(before,aircraft.rectTransform.anchoredPosition),Is.GreaterThan(.01f));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
    }
}
#endif
