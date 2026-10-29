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
    public sealed partial class AccountReferenceTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        const string MotionReport="WelcomeTailRangeOct16";
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
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            Assert.That(UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation.Status,Is.EqualTo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded));
            foreach(string code in new[]{"en","ru","uk"}){
                var table=UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(code));
                yield return table;Assert.That(table.Status,Is.EqualTo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded));
            }
            portrait=new RenderTexture(720,1280,24);yield return Load();game.SetUiLocale("ru");
            // Locale changes rebuild the menu asynchronously. Capture live view references
            // only after that documented lifecycle boundary has completed.
            yield return new WaitUntil(()=>!(bool)Field("applyingUiLanguage"));
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
                var account=ScreenRect((RectTransform)current.GetComponentsInChildren<Button>().Single(b=>b.name=="Sign in"||b.name=="Cloud account").transform);
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
                title.ForceMeshUpdate();var paintedCentre=RectTransformUtility.WorldToScreenPoint(camera,title.transform.TransformPoint(CenteredContentRow.GlyphBounds(title).center));
                Assert.That(Vector2.Distance(paintedCentre,(Vector2)camera.WorldToScreenPoint(board.transform.TransformPoint(CenteredContentRow.ReadingRect(board.rectTransform).center))),Is.LessThan(.2f));
            }
        }
        IEnumerator Shot(string name,int width,int height)
        {
            yield return new WaitForSecondsRealtime(.5f);
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable.PlayableCapture")).First(t=>t!=null);
            string hash=game.Interaction.Attempt.CurrentStateHash;
            Action<CascadeGame> verify=current=>{if(name.Contains("linked-visual-fixture"))typeof(CascadeGame).GetMethod("ApplyHomeAccountShortcutState",Private).Invoke(current,new object[]{true,true});Check(current);};
            string phase=name.StartsWith("welcome-motion",StringComparison.Ordinal)?MotionReport:name.StartsWith("welcome-life",StringComparison.Ordinal)||name.StartsWith("welcome-system",StringComparison.Ordinal)?"WelcomeLifeLanguageOct16":"AccountReferenceOct15";
            yield return (IEnumerator)type.GetMethod("CaptureSettled").Invoke(null,new object[]{phase,name+".png",width,height,verify});
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [Serializable] sealed class PoseRecord {public string id,label,context,asset;}
        [Serializable] sealed class PoseManifest {public PoseRecord[] poses;}
        [Serializable] sealed class IdleMeasurement {public float bellyVerticalSourcePixels,kneeHorizontalSourcePixels,minimumTailSpanOverFiveSeconds,maximumTailSpanOverFiveSeconds,tailFrameStepSourcePixels;}
        [UnityTest] public IEnumerator ExportCurrentMikaPosesAndWelcomeFooting()
        {
            WelcomePhone();game.ReducedMotion=true;game.ShowAccountGate();yield return new WaitForSecondsRealtime(.6f);
            var root=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","GameplayPolishOct16");Directory.CreateDirectory(root);
            foreach(var locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);game.ShowAccountGate();yield return new WaitForSecondsRealtime(.55f);game.RefreshLayout();yield return null;Canvas.ForceUpdateCanvases();
                camera.Render();var old=RenderTexture.active;RenderTexture.active=portrait;
                var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();
                File.WriteAllBytes(Path.Combine(root,"welcome-footing-"+locale+".png"),pixels.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.Destroy(pixels);Check(game);
            }
            var records=new System.Collections.Generic.List<PoseRecord>();var output=Path.Combine(root,"poses");Directory.CreateDirectory(output);
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(c=>c.enabled).ToArray();
            var oldMask=camera.cullingMask;var oldClear=camera.clearFlags;var oldColor=camera.backgroundColor;var oldRender=camera.targetTexture;
            var render=new RenderTexture(240,320,24);render.Create();var readback=new Texture2D(240,320,TextureFormat.RGB24,false);
            var host=new GameObject("Native Mika pose review",typeof(RectTransform),typeof(Canvas));host.layer=5;var canvas=host.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            try{
                foreach(var c in canvases)c.enabled=false;
                camera.targetTexture=render;camera.cullingMask=1<<5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.82f,.9f,.94f);
                Sprite From(string method,params object[] args)=>(Sprite)typeof(CascadeGame).GetMethod(method,Private).Invoke(game,args);
                void Pose(string id,string label,string context,Sprite sprite,bool failure=false,bool welcome=false){
                    Assert.That(sprite,Is.Not.Null,id);var obj=new GameObject(id,typeof(RectTransform));obj.layer=5;obj.transform.SetParent(host.transform,false);
                    var image=failure?(Image)obj.AddComponent<ReferenceCutoutImage>():obj.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
                    var rect=image.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(220,300);
                    if(failure){rect.sizeDelta=new Vector2(220,300);((ReferenceCutoutImage)image).SetContour(FailureReferenceArt.HeroContour);}
                    if(welcome){var life=obj.AddComponent<WelcomeMikaView>();life.Initialize(game,image);life.ResetReview();}
                    Canvas.ForceUpdateCanvases();camera.Render();var active=RenderTexture.active;RenderTexture.active=render;readback.ReadPixels(new Rect(0,0,240,320),0,0);readback.Apply();RenderTexture.active=active;
                    File.WriteAllBytes(Path.Combine(output,id+".png"),readback.EncodeToPNG());
                    records.Add(new PoseRecord{id=id,label=label,context=context,asset=sprite.texture.name+" "+sprite.rect});UnityEngine.Object.DestroyImmediate(obj);
                }
                Pose("01","Приветствие","Машет рукой, вход в игру",AccountReferenceArt.Get("WelcomeMika"),welcome:true);
                Pose("02","Главное меню","Стоит с фонарём",AccountReferenceArt.Get("HomeMika"));
                Pose("03","Мои вещи","Стоит с сумкой и фонарём",From("ReferenceShopArt","InventoryMika"));
                Pose("04","Пауза","Сидит с яблоком",Resources.Load<Sprite>("CascadeArt/PauseReferenceOct14/RestingMika"));
                Pose("05","Поражение","Сидит, подперев щёки",FailureReferenceArt.Hero(),failure:true);
                Pose("06","Победа","Поднимает фонарь",From("VictoryMika"));
                Pose("07","Надежда","Диалоги и завершение путешествия",Resources.Load<Sprite>("CascadeArt/Journey/Mika"));
                Pose("08","Раздумье","Диалоги и реакции на события",Resources.Load<Sprite>("CascadeArt/Journey/MikaThinking"));
                Pose("09","Портрет в диалоге","Кадрирование основной позы",From("MikaBasePortrait"));
                Pose("10","Примерка трофеев","Мышь на экране коллекции",From("HomeArt","Mika"));
                int index=11;foreach(string id in ShopCatalog.MikaTrophyIds){
                    var method=typeof(CascadeGame).GetMethod("MikaCostume",Private);var hopeful=(Sprite)method.Invoke(game,new object[]{id,false,false});if(hopeful==null)continue;
                    Pose((index++).ToString("00"),id+" · надежда","Костюм, основная поза",hopeful);
                    Pose((index++).ToString("00"),id+" · раздумье","Костюм, задумчивая поза",(Sprite)method.Invoke(game,new object[]{id,true,false}));
                }
                File.WriteAllText(Path.Combine(output,"manifest.json"),JsonUtility.ToJson(new PoseManifest{poses=records.ToArray()},true));
            }finally{
                UnityEngine.Object.DestroyImmediate(host);camera.targetTexture=oldRender;camera.cullingMask=oldMask;camera.clearFlags=oldClear;camera.backgroundColor=oldColor;
                foreach(var c in canvases)if(c!=null)c.enabled=true;render.Release();UnityEngine.Object.Destroy(render);UnityEngine.Object.Destroy(readback);game.RefreshLayout();
            }
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
        void WelcomePhone(int width=390,int height=844)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;portrait.Release();portrait.width=width;portrait.height=height;portrait.Create();scaler.enabled=true;game.RefreshLayout();
        }
        static bool Changed(Color32 a,Color32 b,int threshold=10)=>Math.Abs(a.r-b.r)+Math.Abs(a.g-b.g)+Math.Abs(a.b-b.b)>threshold;
        Vector2 WelcomeSourcePoint(Image hero,Vector2 point)
        {
            var sprite=AccountReferenceArt.Get("WelcomeMika");var area=hero.rectTransform.rect;float scale=Mathf.Min(area.width/sprite.rect.width,area.height/sprite.rect.height);
            var centre=new Vector2(sprite.rect.center.x,sprite.texture.height-sprite.rect.center.y);var local=area.center+Vector2.Scale(point-centre,new Vector2(scale,-scale));
            return camera.WorldToScreenPoint(hero.rectTransform.TransformPoint(local));
        }
        Rect WelcomeSourceBox(Image hero,Vector2 a,Vector2 b,float padding=0)
        {
            var p=WelcomeSourcePoint(hero,a);var q=WelcomeSourcePoint(hero,b);return Rect.MinMaxRect(Mathf.Min(p.x,q.x)-padding,Mathf.Min(p.y,q.y)-padding,Mathf.Max(p.x,q.x)+padding,Mathf.Max(p.y,q.y)+padding);
        }
        void WindShot(string name)
        {
            Canvas.ForceUpdateCanvases();camera.Render();var active=RenderTexture.active;RenderTexture.active=portrait;
            var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",MotionReport);Directory.CreateDirectory(folder);File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());
            RenderTexture.active=active;UnityEngine.Object.Destroy(pixels);
        }
        [UnityTest] public IEnumerator FlowersFollowWindWithoutLeavingTheirSupportsInMenuAndGameplay()
        {
            WelcomePhone();game.ReducedMotion=false;game.Preferences.EffectQuality=2;
            foreach(int plant in Enumerable.Range(0,4)){
                float left=WorldSeasonalArt.PlantSway(9,plant,-.8f),right=WorldSeasonalArt.PlantSway(9,plant,.8f);
                Assert.That(left-right,Is.GreaterThan(25),"Reversing the wind must reverse the lean");
            }
            game.ShowHome();yield return new WaitForSecondsRealtime(.5f);
            var environment=game.GetComponentInChildren<MenuWorldEnvironment>();var flecks=game.GetComponentInChildren<MenuSeasonalFlecks>();
            var flowers=flecks.GetComponentsInChildren<Image>().Where(i=>i.enabled).ToArray();Assert.That(flowers.Length,Is.EqualTo(4));
            var roots=flowers.Select(i=>i.rectTransform.anchoredPosition).ToArray();environment.SetReviewAge(2);WindShot("home-calm");var turns=flowers.Select(i=>i.rectTransform.localRotation).ToArray();
            environment.SetReviewAge(9);WindShot("home-gust");
            for(int i=0;i<flowers.Length;i++){Assert.That(flowers[i].rectTransform.anchoredPosition,Is.EqualTo(roots[i]));Assert.That(Quaternion.Angle(turns[i],flowers[i].rectTransform.localRotation),Is.GreaterThan(2));}
            var frame=game.GetComponentsInChildren<Image>().First(i=>i.material!=null&&i.material.name.StartsWith("First-world frame reskin"));
            Assert.That(frame.material.GetFloat("_Wind"),Is.EqualTo(WorkshopWind.Sample(9)).Within(.0001f));
            Profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();Profile.SeenCoachPrompts.Add("controls-two-ways-v1");Profile.SeenCoachPrompts.Add("controls-undo-v1");
            game.Preferences.Tutorials=false;game.ShowGame();float until=Time.realtimeSinceStartup+30;
            while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<until)yield return null;Assert.That(game.Interaction.CanPlay,Is.True);
            // Rooted worlds use the existing batched flowers. A rainy world
            // exercises the separate attached-sprig path without duplicating them.
            game.Machine.SetZone(1);typeof(CascadeGame).GetMethod("UpdateScreenCosmetics",Private).Invoke(game,null);yield return null;
            var attached=game.GetComponentsInChildren<AttachedPlants>().Single(p=>p.name=="Game frame attached plants");
            flowers=attached.GetComponentsInChildren<Image>();roots=flowers.Select(i=>i.rectTransform.anchoredPosition).ToArray();
            game.Machine.CueWind(true);yield return new WaitForSecondsRealtime(.95f);WindShot("gameplay-wind-right");
            float rightTurn=flowers.Average(i=>Mathf.DeltaAngle(0,i.rectTransform.localEulerAngles.z));
            game.Machine.CueWind(false);yield return new WaitForSecondsRealtime(.95f);WindShot("gameplay-wind-left");
            float leftTurn=flowers.Average(i=>Mathf.DeltaAngle(0,i.rectTransform.localEulerAngles.z));
            Assert.That(leftTurn-rightTurn,Is.GreaterThan(8),"The actual gameplay wind must drive the attached flowers");
            for(int i=0;i<flowers.Length;i++)Assert.That(flowers[i].rectTransform.anchoredPosition,Is.EqualTo(roots[i]));
            game.ReducedMotion=true;yield return null;turns=flowers.Select(i=>i.rectTransform.localRotation).ToArray();yield return new WaitForSecondsRealtime(.15f);
            for(int i=0;i<flowers.Length;i++)Assert.That(flowers[i].rectTransform.localRotation,Is.EqualTo(turns[i]));
        }
        [Test] public void WelcomeIdleKeepsBellyQuietAndMovesKneesAndTailInCalm()
        {
            var motion=new WelcomeMikaMotion(197);var belly=new Vector2(645,900);var knee=new Vector2(500,1140);var tail=new Vector2(140,850);
            Vector2 Local(Vector2 source){var p=motion.Deform(source)-WelcomeSecondaryMotion.BodyPivot-motion.BodyOffset;float a=-motion.BodyAngle*Mathf.Deg2Rad;return WelcomeSecondaryMotion.BodyPivot+new Vector2(p.x*Mathf.Cos(a)-p.y*Mathf.Sin(a),p.x*Mathf.Sin(a)+p.y*Mathf.Cos(a));}
            float bellyLo=float.PositiveInfinity,bellyHi=float.NegativeInfinity,kneeLo=float.PositiveInfinity,kneeHi=float.NegativeInfinity,minTailSpan=float.PositiveInfinity,maxTailSpan=0,maxStep=0;
            var low=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var high=new Vector2(float.NegativeInfinity,float.NegativeInfinity);var previous=tail;
            for(int frame=0;frame<60*30;frame++){
                motion.Advance(1f/30,0,Vector2.zero,false);var b=Local(belly);var k=Local(knee);var t=Local(tail);
                if(frame>=4*30){bellyLo=Mathf.Min(bellyLo,b.y);bellyHi=Mathf.Max(bellyHi,b.y);kneeLo=Mathf.Min(kneeLo,k.x);kneeHi=Mathf.Max(kneeHi,k.x);low=Vector2.Min(low,t);high=Vector2.Max(high,t);maxStep=Mathf.Max(maxStep,Vector2.Distance(previous,t));}
                previous=t;
                if(frame>=4*30&&(frame-4*30)%150==149){float span=(high-low).magnitude;minTailSpan=Mathf.Min(minTailSpan,span);maxTailSpan=Mathf.Max(maxTailSpan,span);low=Vector2.one*float.PositiveInfinity;high=Vector2.one*float.NegativeInfinity;}
                foreach(var foot in new[]{new Vector2(382,1432),new Vector2(760,1400),new Vector2(800,1388)})Assert.That(motion.Deform(foot),Is.EqualTo(foot));
            }
            var folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",MotionReport);Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"idle-calm.json"),JsonUtility.ToJson(new IdleMeasurement{bellyVerticalSourcePixels=bellyHi-bellyLo,kneeHorizontalSourcePixels=kneeHi-kneeLo,minimumTailSpanOverFiveSeconds=minTailSpan,maximumTailSpanOverFiveSeconds=maxTailSpan,tailFrameStepSourcePixels=maxStep},true));
            Assert.That(bellyHi-bellyLo,Is.LessThan(3.2f),"Breathing must not pump the belly");
            Assert.That(kneeHi-kneeLo,Is.GreaterThan(7),"The legs need independent, gentle weight transfer");
            Assert.That(minTailSpan,Is.GreaterThan(6),"The tail must keep moving in calm instead of waiting between short gestures");
            Assert.That(maxTailSpan/minTailSpan,Is.GreaterThan(1.4f),"Tail amplitude must swell and relax");Assert.That(maxStep,Is.LessThan(3),"Tail motion must stay continuous");
        }
        [Test] public void WelcomeSkinKeepsFeetAndMuzzleRigidAtDifferentFrameRates()
        {
            var slow=new WelcomeMikaMotion(197);var fast=new WelcomeMikaMotion(197);var feet=new[]{new Vector2(382,1432),new Vector2(760,1400),new Vector2(800,1388)};
            float maxError=0,maxCloth=0,maxTail=0;int blinks=0;bool shut=false;
            for(int frame=0;frame<900;frame++){
                float time=frame/30f;slow.Advance(1f/30,WorkshopWind.Sample(time),new Vector2(.7f,.1f),true);
                // Feed identical held wind samples to both timelines; compare
                // integration rather than two different input sampling histories.
                for(int sub=0;sub<4;sub++)fast.Advance(1f/120,WorkshopWind.Sample(time),new Vector2(.7f,.1f),true);
                foreach(var p in feet){Assert.That(slow.Deform(p),Is.EqualTo(p),"Soles may not skate or bob");Assert.That(fast.Deform(p),Is.EqualTo(p));}
                Assert.That(Vector2.Distance(slow.Deform(new Vector2(595,434)),slow.Deform(new Vector2(645,434))),Is.EqualTo(50).Within(.01f),"The smile and two teeth must retain their anatomy");
                foreach(var p in new[]{new Vector2(180,520),new Vector2(150,850),new Vector2(900,420),new Vector2(520,340),new Vector2(360,930)})maxError=Mathf.Max(maxError,Vector2.Distance(slow.Deform(p),fast.Deform(p)));
                foreach(var p in new[]{new Vector2(143,509),new Vector2(280,545)}){var bind=WelcomeSecondaryMotion.BindScarf(p);maxError=Mathf.Max(maxError,Vector2.Distance(slow.ScarfPoint(p,bind),fast.ScarfPoint(p,bind)));}
                maxCloth=Mathf.Max(maxCloth,slow.ScarfTipDisplacement);maxTail=Mathf.Max(maxTail,slow.TailTipDisplacement);
                if(slow.Blink>.9f&&!shut)blinks++;shut=slow.Blink>.9f;
            }
            Assert.That(maxError,Is.LessThan(1.2f),"Cloth/limb integration must remain stable at 30 and 120 Hz");Assert.That(maxCloth,Is.GreaterThan(2));Assert.That(maxTail,Is.GreaterThan(.5f));Assert.That(blinks,Is.GreaterThan(3));
        }
        [UnityTest] public IEnumerator WelcomeBlinkChangesOnlyEyesAndBootsStayOnThePlatform()
        {
            WelcomePhone();game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.ShowAccountGate();yield return new WaitForSecondsRealtime(.7f);
            var view=game.GetComponentInChildren<WelcomeMikaView>();view.ResetReview();var hero=view.GetComponent<Image>();var skin=hero.GetComponent<WelcomeMikaMesh>();var material=hero.material;
            var scarf=view.GetComponentInChildren<WelcomeMikaScarf>();var neutral=WelcomePixels();skin.enabled=false;hero.material=null;scarf.enabled=false;var original=WelcomePixels();skin.enabled=true;hero.material=material;scarf.enabled=true;
            var oldCloth=WelcomeSourceBox(hero,new Vector2(90,435),new Vector2(425,660),2);var restingCloth=WelcomeSourceBox(hero,new Vector2(240,535),new Vector2(485,900),2);
            int neutralError=0;for(int i=0;i<neutral.Length;i++)if(Changed(neutral[i],original[i])){var p=new Vector2(i%portrait.width,i/portrait.width);if(!oldCloth.Contains(p)&&!restingCloth.Contains(p))neutralError++;}
            Assert.That(neutralError,Is.LessThan(20),"Only the loose scarf changes its neutral pose; the face, body and original artwork remain intact elsewhere");
            view.SetReviewBlink(1);var closed=WelcomePixels();
            var nearEye=WelcomeSourceBox(hero,new Vector2(410,260),new Vector2(562,435),2);var farEye=WelcomeSourceBox(hero,new Vector2(625,210),new Vector2(737,366),2);
            int eyes=0,other=0;for(int i=0;i<neutral.Length;i++)if(Changed(neutral[i],closed[i])){var point=new Vector2(i%portrait.width,i/portrait.width);if(nearEye.Contains(point)||farEye.Contains(point))eyes++;else other++;}
            Assert.That(eyes,Is.GreaterThan(20),"The blink must paint visible eyelids");Assert.That(other,Is.Zero,"A blink cannot replace the mouth, cheeks, body or background");
            view.SetReviewBlink(0);yield return Shot("welcome-motion-neutral",390,844);
            var feet=WelcomeSourceBox(hero,new Vector2(300,1360),new Vector2(850,1477));var before=WelcomePixels();
            for(int i=0;i<240;i++)view.AdvanceReview(1f/60,.8f,Vector2.right,true);
            var after=WelcomePixels();int moving=0,sliding=0;
            for(int i=0;i<before.Length;i++)if(Changed(before[i],after[i])){moving++;if(feet.Contains(new Vector2(i%portrait.width,i/portrait.width)))sliding++;}
            Assert.That(moving,Is.GreaterThan(100));Assert.That(sliding,Is.Zero,"Foot pixels must remain planted while the body moves");
            var rendered=hero.canvasRenderer.GetMesh();
            Assert.That(rendered.vertexCount,Is.GreaterThan(1000));Assert.That(rendered.vertexCount,Is.LessThan(4000));
            yield return Shot("welcome-motion-wave-and-cloth",390,844);
        }
        [UnityTest] public IEnumerator WelcomeRigSuspendsAndIsAbsentFromOtherScreens()
        {
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.ShowAccountGate();yield return new WaitForSecondsRealtime(.4f);
            var view=game.GetComponentInChildren<WelcomeMikaView>();var hero=view.GetComponent<Image>();float age=view.AnimationAge;string hash=game.Interaction.Attempt.CurrentStateHash;
            yield return new WaitForSecondsRealtime(.25f);Assert.That(view.AnimationAge,Is.GreaterThan(age));
            Assert.That(hero.raycastTarget,Is.False);Assert.That(hero.GetComponent<Button>(),Is.Null);Assert.That(hero.GetComponent<GraphicRaycaster>(),Is.Null);
            game.SendMessage("OnApplicationFocus",false);float held=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(view.AnimationAge,Is.EqualTo(held));
            game.Preferences.EffectQuality=1;yield return null;Canvas.ForceUpdateCanvases();camera.Render();Assert.That(hero.canvasRenderer.GetMesh().vertexCount,Is.EqualTo(925));
            game.Preferences.EffectQuality=2;yield return null;Canvas.ForceUpdateCanvases();camera.Render();Assert.That(hero.canvasRenderer.GetMesh().vertexCount,Is.EqualTo(3577));
            Assert.That(view.AnimationAge,Is.EqualTo(held));game.SendMessage("OnApplicationFocus",true);
            game.ReducedMotion=true;yield return null;Assert.That(view.SkinActive,Is.False);float quiet=view.AnimationAge;yield return new WaitForSecondsRealtime(.15f);Assert.That(view.AnimationAge,Is.EqualTo(quiet));
            game.ReducedMotion=false;game.Preferences.EffectQuality=0;yield return null;Assert.That(view.SkinActive,Is.False);game.Preferences.EffectQuality=2;
            game.ShowHome();yield return null;Assert.That(game.GetComponentsInChildren<WelcomeMikaView>().Length,Is.Zero);
            game.ShowShop();yield return null;Assert.That(game.GetComponentsInChildren<WelcomeMikaView>().Length,Is.Zero);
            game.ShowAccountGate();yield return null;Assert.That(game.GetComponentsInChildren<WelcomeMikaView>().Length,Is.EqualTo(1));
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [Test] public void WelcomeGesturesKeepTheirArcsAndSkinAcrossSessions()
        {
            foreach(int columns in new[]{48,24})foreach(int seed in new[]{197,73,8931}){
                int rows=columns*3/2;var motion=new WelcomeMikaMotion(seed);var binds=new WelcomeMikaMotion.SkinPoint[(columns+1)*(rows+1)];var points=new Vector2[binds.Length];
                for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)binds[y*(columns+1)+x]=new WelcomeMikaMotion.SkinPoint(new Vector2(31+957*x/(float)columns,1479-1440*y/(float)rows));
                float minArea=float.PositiveInfinity,maxStep=0,minHand=float.PositiveInfinity,maxHand=float.NegativeInfinity,previousAngle=0,maxAngularStep=0;string worst="";
                var hand=motion.Deform(new Vector2(930,390));
                for(int frame=0;frame<42*120;frame++){
                    float time=frame/120f;var visitor=new Vector2(Mathf.Sin(time*.37f)*.8f,.1f);
                    motion.Advance(1f/120,Mathf.Sin(time*.8f),visitor,time%9<6);
                    var next=motion.Deform(new Vector2(930,390));maxStep=Mathf.Max(maxStep,Vector2.Distance(next,hand));hand=next;
                    minHand=Mathf.Min(minHand,hand.x);maxHand=Mathf.Max(maxHand,hand.x);maxAngularStep=Mathf.Max(maxAngularStep,Mathf.Abs(motion.WristAngle-previousAngle));previousAngle=motion.WristAngle;
                    if(frame%12!=0)continue;
                    for(int i=0;i<points.Length;i++)points[i]=motion.Deform(binds[i]);
                    for(int y=0;y<rows;y++)for(int x=0;x<columns;x++){
                        int a=y*(columns+1)+x,b=a+columns+1;var ab=points[b]-points[a];var ac=points[b+1]-points[a];var ad=points[a+1]-points[a];
                        float area=Mathf.Min(ab.x*ac.y-ab.y*ac.x,ac.x*ad.y-ac.y*ad.x);if(area<minArea){minArea=area;worst=" at "+time+" s, "+binds[a].Source+", columns "+columns;}
                    }
                }
                Assert.That(minArea,Is.GreaterThan(957f/columns*1440/rows*.12f),"No folds or collapsed skin, including later waves, both quality levels and different sessions; seed "+seed+worst);
                Assert.That(maxHand-minHand,Is.GreaterThan(15),"The greeting must produce a visible arc at phone scale");
                Assert.That(maxStep,Is.LessThan(2.5f),"The hand cannot snap between poses");Assert.That(maxAngularStep,Is.LessThan(.8f),"Wrist motion must blend smoothly at 120 Hz");
            }
        }
        [Test] public void WelcomeScarfRestsOnShoulderAndRisesOnlyWithWind()
        {
            var physics=new WelcomeSecondaryMotion();var tip=new Vector2(143,509);var binding=WelcomeSecondaryMotion.BindScarf(tip);float dt=1f/120;
            void Simulate(float seconds,float wind,float age){for(int i=0;i<seconds*120;i++)physics.Step(dt,Vector2.zero,0,wind,age+i*dt);physics.PrepareSkin();}
            Simulate(10,0,0);var resting=physics.ScarfPoint(tip,binding);
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",MotionReport);Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"rest-position.json"),FormattableString.Invariant($"{{\"x\":{resting.x:R},\"y\":{resting.y:R}")+"}");
            Assert.That(resting.y,Is.GreaterThan(760),"In calm the free scarf end must drape down the shoulder, not keep its painted airborne pose");
            Assert.That(resting.x,Is.InRange(260,360),"The resting scarf must stay against the mouse's back/shoulder");
            Simulate(4,.72f,10);var lifted=physics.ScarfPoint(tip,binding);
            Assert.That(lifted.y,Is.LessThan(590),"A gust must lift the cloth toward the original streaming pose");
            Assert.That(lifted.x,Is.LessThan(220));
            Simulate(8,0,14);var settled=physics.ScarfPoint(tip,binding);
            Assert.That(Vector2.Distance(resting,settled),Is.LessThan(3),"The cloth must return to its shoulder contact after the gust, without remaining suspended");
        }
        [Test] public void WelcomeSecondaryPhysicsKeepsAnchorsLengthAndSettlesInCalm()
        {
            var physics=new WelcomeSecondaryMotion();float dt=1f/120;
            for(int i=0;i<2400;i++)physics.Step(dt,Vector2.zero,0,0,i*dt);
            float calmEnergy=physics.KineticEnergy;Assert.That(calmEnergy,Is.LessThan(.1f),"Calm secondary parts must come to rest without a clock-driven wiggle");
            physics.KickForReview(new Vector2(35,-45),new Vector2(-12,-9),.1f);
            float peakEnergy=0,maxStretch=0,maxAnchor=0;
            for(int i=0;i<2400;i++){
                physics.Step(dt,Vector2.zero,0,0,20+i*dt);peakEnergy=Mathf.Max(peakEnergy,physics.KineticEnergy);maxStretch=Mathf.Max(maxStretch,physics.MaxRelativeStretch);maxAnchor=Mathf.Max(maxAnchor,physics.MaxAnchorError);
            }
            physics.PrepareSkin();
            Assert.That(maxStretch,Is.LessThan(.005f),"Scarf/tail links must retain length under an impulse");Assert.That(maxAnchor,Is.LessThan(.001f),"Pinned roots cannot detach");
            Assert.That(physics.KineticEnergy,Is.LessThan(peakEnergy*.005f+.01f),"Impulse energy must dissipate after the force stops");
            float bagLength=Vector2.Distance(WelcomeSecondaryMotion.BagHinge,WelcomeSecondaryMotion.BagCentre);
            Assert.That(Vector2.Distance(physics.BagPoint(WelcomeSecondaryMotion.BagHinge),physics.BagPoint(WelcomeSecondaryMotion.BagCentre)),Is.EqualTo(bagLength).Within(.001f),"A leather bag and strap may swing but cannot stretch");
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","WelcomePhysicsOct16");Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"calm-and-impulse.json"),FormattableString.Invariant($"{{\"calmEnergy\":{calmEnergy:R},\"peakImpulseEnergy\":{peakEnergy:R},\"endEnergy\":{physics.KineticEnergy:R},\"maxRelativeStretch\":{maxStretch:R},\"maxAnchorErrorSourcePixels\":{maxAnchor:R},\"scarfRestSagSourcePixels\":{physics.ScarfTipDisplacement:R},\"tailRestSagSourcePixels\":{physics.TailTipDisplacement:R}")+"}");
        }
        [Test] public void WelcomeSecondaryAttachmentsAndRigidBagFollowTheBody()
        {
            var physics=new WelcomeSecondaryMotion();var motion=new WelcomeMikaMotion(197);float maxStretch=0,maxAnchor=0,maxBagShapeError=0;
            for(int i=0;i<3600;i++){
                float t=i/120f;physics.Step(1f/120,new Vector2(Mathf.Sin(t)*3,Mathf.Sin(t*.7f)),Mathf.Sin(t*.8f)*.3f,Mathf.Sin(t*.63f),t);motion.Advance(1f/120,Mathf.Sin(t*.63f),Vector2.right,true);
                maxStretch=Mathf.Max(maxStretch,physics.MaxRelativeStretch);maxAnchor=Mathf.Max(maxAnchor,physics.MaxAnchorError);
                maxBagShapeError=Mathf.Max(maxBagShapeError,Mathf.Abs(Vector2.Distance(motion.Deform(new Vector2(340,940)),motion.Deform(new Vector2(390,940)))-50));
            }
            Assert.That(maxBagShapeError,Is.LessThan(.015f),"The pouch must keep its shape while the chest breathes");
            Assert.That(maxStretch,Is.LessThan(.005f));Assert.That(maxAnchor,Is.LessThan(.001f));
            physics.PrepareSkin();var a=new Vector2(280,530);var b=new Vector2(280,560);
            float scarfWidth=Vector2.Distance(physics.ScarfPoint(a,WelcomeSecondaryMotion.BindScarf(a)),physics.ScarfPoint(b,WelcomeSecondaryMotion.BindScarf(b)));
            Assert.That(scarfWidth,Is.EqualTo(30).Within(.5f),"Cloth width must not collapse when the chain bends");
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","WelcomePhysicsOct16");Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"moving-anchors.json"),FormattableString.Invariant($"{{\"maxRelativeStretch\":{maxStretch:R},\"maxAnchorErrorSourcePixels\":{maxAnchor:R},\"maxBagShapeErrorSourcePixels\":{maxBagShapeError:R},\"scarfWidthSourcePixels\":{scarfWidth:R}")+"}");
        }
        [UnityTest] public IEnumerator WelcomeMotionBudgetStaysAllocationFree()
        {
            WelcomePhone();game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.ShowAccountGate();yield return null;
            var view=game.GetComponentInChildren<WelcomeMikaView>();view.ResetReview();var skin=view.GetComponent<WelcomeMikaMesh>();var whiskers=view.GetComponentInChildren<WelcomeMikaWhiskers>();var scarf=view.GetComponentInChildren<WelcomeMikaScarf>();var sprite=view.SourceSprite;
            var uv=sprite.rect;var texture=sprite.texture;var quad=new UIVertex[4];var timings=new double[5];
            for(int i=0;i<4;i++){
                quad[i]=UIVertex.simpleVert;bool right=i>=2,top=i==1||i==2;
                quad[i].position=new Vector3(right?100:-100,top?150:-150);
                quad[i].uv0=new Vector2((right?uv.xMax:uv.xMin)/texture.width,(top?uv.yMax:uv.yMin)/texture.height);
            }
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",MotionReport);Directory.CreateDirectory(folder);
            string phase=Environment.GetEnvironmentVariable("CASCADE_WELCOME_BENCHMARK")=="baseline"?"baseline":"refined";
            var results=new System.Text.StringBuilder("quality,vertices,median_cpu_ms_per_simulation_and_mesh,managed_bytes_for_600_frames\n");
            using(var vertices=new VertexHelper())using(var strands=new VertexHelper())using(var cloth=new VertexHelper()){
                void Frame(){
                    view.Motion.Advance(1f/60,.7f,new Vector2(.6f,.1f),true);vertices.Clear();
                    for(int i=0;i<4;i++)vertices.AddVert(quad[i]);vertices.AddTriangle(0,1,2);vertices.AddTriangle(0,2,3);skin.ModifyMesh(vertices);whiskers.BuildReviewMesh(strands);scarf.BuildReviewMesh(cloth);
                }
                foreach(int quality in new[]{2,1}){
                    game.Preferences.EffectQuality=quality;for(int i=0;i<60;i++)Frame();
                    long allocated=GC.GetAllocatedBytesForCurrentThread();
                    for(int sample=0;sample<timings.Length;sample++){
                        long start=System.Diagnostics.Stopwatch.GetTimestamp();for(int i=0;i<120;i++)Frame();
                        timings[sample]=(System.Diagnostics.Stopwatch.GetTimestamp()-start)*1000.0/System.Diagnostics.Stopwatch.Frequency/120;
                    }
                    allocated=GC.GetAllocatedBytesForCurrentThread()-allocated;Array.Sort(timings);
                    Assert.That(allocated,Is.Zero,"After warmup the animation simulation and mesh builder must not allocate per frame");
                    Assert.That(vertices.currentVertCount,Is.EqualTo(quality==2?3577:925));
                    results.Append(quality).Append(',').Append(vertices.currentVertCount).Append(',').Append(timings[2].ToString("F6",System.Globalization.CultureInfo.InvariantCulture)).Append(',').Append(allocated).Append('\n');
                }
            }
            File.WriteAllText(Path.Combine(folder,"cpu-"+phase+".csv"),results.ToString());
        }
        [UnityTest] public IEnumerator WelcomeSecondaryMotionIsVisibleAtPhoneSize()
        {
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.Preferences.LargeText=false;
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
            texture.LoadImage(File.ReadAllBytes(Path.Combine(UnityEngine.Application.dataPath,"_Cascade/Content/Resources/CascadeArt/AccountOct15/WelcomeMika.png")));
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",MotionReport);Directory.CreateDirectory(folder);
            var report=new System.Text.StringBuilder("width,height,tail_px,scarf_px,bag_px,chest_px,whiskers_px,painted_whisker_pixels\n");
            try{
                foreach(var size in new[]{new Vector2Int(390,844),new Vector2Int(320,693)}){
                    WelcomePhone(size.x,size.y);game.ShowAccountGate();yield return new WaitForSecondsRealtime(.7f);
                    var view=game.GetComponentInChildren<WelcomeMikaView>();view.ResetReview();var hero=view.GetComponent<Image>();var whiskers=view.GetComponentInChildren<WelcomeMikaWhiskers>();var scarf=view.GetComponentInChildren<WelcomeMikaScarf>();
                    Canvas.ForceUpdateCanvases();var mesh=hero.canvasRenderer.GetMesh();var uv=mesh.uv;var posed=mesh.vertices;
                    var groups=new int[uv.Length];int hip=0;float closest=float.PositiveInfinity;
                    for(int i=0;i<uv.Length;i++){
                        var p=new Vector2(uv[i].x*texture.width,(1-uv[i].y)*texture.height);float d=(p-new Vector2(585,1110)).sqrMagnitude;if(d<closest){closest=d;hip=i;}
                        if(texture.GetPixelBilinear(uv[i].x,uv[i].y).a<.7f)continue;
                        if(new Rect(100,680,145,340).Contains(p))groups[i]=1;
                        else if(new Rect(260,850,170,200).Contains(p))groups[i]=3;
                        else if(new Rect(530,700,170,250).Contains(p))groups[i]=4;
                    }
                    var lo=Enumerable.Repeat(new Vector2(float.PositiveInfinity,float.PositiveInfinity),uv.Length).ToArray();var hi=Enumerable.Repeat(new Vector2(float.NegativeInfinity,float.NegativeInfinity),uv.Length).ToArray();
                    var strandMesh=whiskers.canvasRenderer.GetMesh();int count=strandMesh.vertexCount;
                    var strandLo=Enumerable.Repeat(new Vector2(float.PositiveInfinity,float.PositiveInfinity),count).ToArray();var strandHi=Enumerable.Repeat(new Vector2(float.NegativeInfinity,float.NegativeInfinity),count).ToArray();
                    var clothMesh=scarf.canvasRenderer.GetMesh();var clothUv=clothMesh.uv;
                    var clothSamples=Enumerable.Range(0,clothUv.Length).Where(i=>WelcomeMikaScarf.ContainsSource(new Vector2(clothUv[i].x*texture.width,(1-clothUv[i].y)*texture.height))&&texture.GetPixelBilinear(clothUv[i].x,clothUv[i].y).a>.7f).ToArray();
                    var clothLo=Enumerable.Repeat(new Vector2(float.PositiveInfinity,float.PositiveInfinity),clothUv.Length).ToArray();var clothHi=Enumerable.Repeat(new Vector2(float.NegativeInfinity,float.NegativeInfinity),clothUv.Length).ToArray();
                    for(int frame=0;frame<240;frame++){
                        view.AdvanceReview(1f/30,WorkshopWind.Sample(frame/30f+5),new Vector2(.6f,.12f),true);Canvas.ForceUpdateCanvases();
                        posed=hero.canvasRenderer.GetMesh().vertices;var anchor=camera.WorldToScreenPoint(hero.rectTransform.TransformPoint(posed[hip]));
                        for(int i=0;i<posed.Length;i++)if(groups[i]>0){Vector2 p=camera.WorldToScreenPoint(hero.rectTransform.TransformPoint(posed[i]))-anchor;lo[i]=Vector2.Min(lo[i],p);hi[i]=Vector2.Max(hi[i],p);}
                        var clothPositions=scarf.canvasRenderer.GetMesh().vertices;foreach(int i in clothSamples){Vector2 p=camera.WorldToScreenPoint(scarf.rectTransform.TransformPoint(clothPositions[i]))-anchor;clothLo[i]=Vector2.Min(clothLo[i],p);clothHi[i]=Vector2.Max(clothHi[i],p);}
                        var strands=whiskers.canvasRenderer.GetMesh().vertices;var muzzle=WelcomeSourcePoint(hero,view.Motion.Deform(new Vector2(620,410)));
                        for(int i=0;i<count;i++){Vector2 p=(Vector2)camera.WorldToScreenPoint(whiskers.rectTransform.TransformPoint(strands[i]))-muzzle;strandLo[i]=Vector2.Min(strandLo[i],p);strandHi[i]=Vector2.Max(strandHi[i],p);}
                        if(frame%30==0)yield return null;
                    }
                    var ranges=new float[5];
                    for(int group=1;group<=4;group++){
                        if(group==2){var clothValues=clothSamples.Select(i=>(clothHi[i]-clothLo[i]).magnitude).OrderBy(v=>v).ToArray();Assert.That(clothValues.Length,Is.GreaterThan(10));ranges[1]=clothValues[(clothValues.Length-1)*3/4];continue;}
                        var values=Enumerable.Range(0,uv.Length).Where(i=>groups[i]==group).Select(i=>(hi[i]-lo[i]).magnitude).OrderBy(v=>v).ToArray();Assert.That(values.Length,Is.GreaterThan(10));ranges[group-1]=values[(values.Length-1)*3/4];
                    }
                    var strandRange=Enumerable.Range(0,count).Select(i=>(strandHi[i]-strandLo[i]).magnitude).OrderBy(v=>v).ToArray();ranges[4]=strandRange[(count-1)*3/4];
                    int painted=PaintedPixels(new Graphic[]{whiskers});
                    report.Append(size.x).Append(',').Append(size.y);foreach(float range in ranges)report.Append(',').Append(range.ToString("F4",System.Globalization.CultureInfo.InvariantCulture));report.Append(',').Append(painted).Append('\n');
                    File.WriteAllText(Path.Combine(folder,"phone-readability.csv"),report.ToString());
                    float phoneScale=size.x/390f;var minimum=new[]{2.5f,2.5f,1.25f,.25f,1.5f};
                    for(int part=0;part<ranges.Length;part++)Assert.That(ranges[part],Is.GreaterThan(minimum[part]*phoneScale),"Secondary part "+part+" must remain visible in real phone pixels relative to its attachment, not only move in source coordinates");
                    Assert.That(ranges[0],Is.GreaterThan(6),"The tail arc must be clearly visible on both phone widths");
                    // The UI keeps the hero readable on both phone widths; cap
                    // the rendered torso excursion in physical pixels directly.
                    Assert.That(ranges[3],Is.LessThan(1.6f),"Breathing and balance must stay subtle at phone size");
                    Assert.That(painted,Is.GreaterThan(40),"Whisker geometry must actually paint visible phone pixels");
                }
            }finally{UnityEngine.Object.DestroyImmediate(texture);}
        }
        [UnityTest] public IEnumerator CaptureWelcomeCharacterMotionForReview()
        {
            WelcomePhone();game.SetUiLocale("en");game.ReducedMotion=false;game.Preferences.EffectQuality=2;game.Preferences.LargeText=false;game.ShowAccountGate();yield return new WaitForSecondsRealtime(.7f);
            var view=game.GetComponentInChildren<WelcomeMikaView>();view.ResetReview();var environment=game.GetComponentInChildren<MenuWorldEnvironment>();
            var birds=game.GetComponentInChildren<HomeSkyLife>();birds.SetReviewSequence(197);birds.SetReviewAge(0);
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",MotionReport,"frames");Directory.CreateDirectory(folder);
            string hash=game.Interaction.Attempt.CurrentStateHash;var pixel=new Texture2D(390,844,TextureFormat.RGBA32,false);var active=RenderTexture.active;
            var animatedLayers=new Graphic[]{view.GetComponent<Image>(),view.GetComponentInChildren<WelcomeMikaScarf>()};
            var plants=game.GetComponentsInChildren<AttachedPlants>();
            try{
                for(int i=0;i<540;i++){
                    float age=i/30f;float wind=WorkshopWind.Sample(60+age);environment.SetReviewAge(60+age);birds.SetReviewAge(age);view.AdvanceReview(1f/30,wind,new Vector2(.6f,.12f),age>2);
                    foreach(var plant in plants)plant.AdvanceReview(1f/30,wind);
                    Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=portrait;pixel.ReadPixels(new Rect(0,0,390,844),0,0);pixel.Apply();File.WriteAllBytes(Path.Combine(folder,i.ToString("D4")+".png"),pixel.EncodeToPNG());
                    foreach(var layer in animatedLayers){
                        var mesh=layer.canvasRenderer.GetMesh();var positions=mesh.vertices;var triangles=mesh.triangles;float worst=float.NegativeInfinity;
                        for(int j=0;j<triangles.Length;j+=3)worst=Mathf.Max(worst,Vector3.Cross(positions[triangles[j+1]]-positions[triangles[j]],positions[triangles[j+2]]-positions[triangles[j]]).z);
                        Assert.That(worst,Is.LessThan(-.05f),"The body and separate cloth cannot fold through themselves; "+layer.name+", native frame "+i);
                    }
                    if(i%16==0)yield return null;
                }
            }finally{RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(pixel);}
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            foreach(string locale in new[]{"ru","uk"}){game.SetUiLocale(locale);game.ShowAccountGate();yield return Shot("welcome-motion-"+locale,390,844);}
        }
        [UnityTest] public IEnumerator WelcomeCostumeFallbackAndSewnBadgeStayStable()
        {
            game.ReducedMotion=false;game.Preferences.EffectQuality=2;
            if(!Profile.Monetization.Cosmetics.Contains("sky"))Profile.Monetization.Cosmetics.Add("sky");Profile.Rewards.EquippedGift="sky";
            game.ShowAccountGate();yield return new WaitForSecondsRealtime(.3f);
            var view=game.GetComponentInChildren<WelcomeMikaView>();Assert.That(view.GetComponent<MikaCosmeticView>().WearsCostume,Is.True);Assert.That(view.SkinActive,Is.False);
            Assert.That(view.GetComponent<Image>().material.shader.name,Is.Not.EqualTo("Cascade/UI/WelcomeFace"),"Unrigged costumes keep their existing illustration");
            if(!Profile.Monetization.Cosmetics.Contains("leaf"))Profile.Monetization.Cosmetics.Add("leaf");Profile.Rewards.EquippedGift="leaf";
            game.ShowAccountGate();yield return new WaitForSecondsRealtime(.3f);view=game.GetComponentInChildren<WelcomeMikaView>();view.ResetReview();
            var badge=view.GetComponent<MikaCosmeticView>().PreviewBadge;Assert.That(badge.gameObject.activeSelf,Is.True);var rest=badge.rectTransform.anchoredPosition;
            var hero=view.GetComponent<Image>();var sprite=view.SourceSprite;var rect=hero.rectTransform.rect;float scale=Mathf.Min(rect.width/sprite.rect.width,rect.height/sprite.rect.height);
            var attachment=new Vector2(sprite.rect.center.x,sprite.texture.height-sprite.rect.center.y)+new Vector2(rest.x,-rest.y)/scale;
            for(int i=0;i<300;i++){
                view.AdvanceReview(1f/60,.5f,Vector2.right,true);Canvas.ForceUpdateCanvases();
                var expected=WelcomeSourcePoint(hero,view.Motion.Deform(attachment));var actual=RectTransformUtility.WorldToScreenPoint(camera,badge.rectTransform.position);
                Assert.That(Vector2.Distance(expected,actual),Is.LessThan(.2f),"The sewn pin must stay at its moving attachment without accumulating offsets");
            }
            Assert.That(badge.material.shader.name,Is.Not.EqualTo("Cascade/UI/WelcomeFace"));yield return Shot("welcome-motion-sewn-leaf",390,844);
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
