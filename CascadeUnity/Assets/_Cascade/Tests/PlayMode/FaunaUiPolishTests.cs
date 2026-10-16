#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Content;
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
    public sealed class FaunaUiPolishTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture portrait,oldTarget;string directory;bool background,audio;
        object Field(string name)=>typeof(CascadeGame).GetField(name,Private).GetValue(game);
        static Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        static string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/FaunaUiOct15"));
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeNavigationReviewFaunaUi",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;
            Resize(390,844);game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));
            game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;game.Coach.Enabled=false;game.ReducedMotion=false;
            var profile=(PlayerProfile)Field("profile");profile.AccountIntroSeen=true;profile.Missions.NextAfterClears=20000;
            ((HashSet<int>)Field("heardStory")).UnionWith(Enumerable.Range(0,10000));game.ShowHome();yield return null;
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
            yield return new WaitForSecondsRealtime(.55f);
            yield return (IEnumerator)EditorType("PlayableCapture").GetMethod("CaptureSettled").Invoke(null,new object[]{"FaunaUiOct15",name+".png",portrait.width,portrait.height,pose});
        }
        static Rect BoundsIn(RectTransform target,RectTransform space)
        {
            var corners=new Vector3[4];target.GetWorldCorners(corners);var low=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var high=-low;
            foreach(var p in corners){var v=(Vector2)space.InverseTransformPoint(p);low=Vector2.Min(low,v);high=Vector2.Max(high,v);}return Rect.MinMaxRect(low.x,low.y,high.x,high.y);
        }
        void AccountCentres()
        {
            var controls=game.GetComponentsInChildren<Button>().Where(b=>b.name=="Sign in with Apple"||b.name=="Sign in with Google"||b.name=="Continue without an account").ToArray();Assert.That(controls.Length,Is.EqualTo(3));
            foreach(var button in controls){
                var target=(RectTransform)button.transform;var stage=(RectTransform)target.parent;
                Assert.That(Mathf.Abs(BoundsIn(target,stage).center.x-stage.rect.center.x),Is.LessThan(.1f),button.name+" button centre");
                var body=(RectTransform)target.Find("Reference shop button body");var icon=button.GetComponentsInChildren<Image>().Single(i=>i.name.StartsWith("Account provider icon ",StringComparison.Ordinal));
                var copy=button.GetComponentsInChildren<TextMeshProUGUI>().Single();copy.ForceMeshUpdate();Assert.That(copy.isTextOverflowing,Is.False,copy.text);
                var ink=copy.textInfo.characterInfo.Take(copy.textInfo.characterCount).Where(c=>c.isVisible).ToArray();Assert.That(ink.Length,Is.GreaterThan(0));
                var left=BoundsIn(icon.rectTransform,body).xMin;var right=ink.Max(c=>body.InverseTransformPoint(copy.transform.TransformPoint(c.topRight)).x);
                Assert.That(Mathf.Abs((left+right)*.5f-body.rect.center.x),Is.LessThan(3),button.name+" visible icon/caption group must be centred");
                var iconBounds=BoundsIn(icon.rectTransform,body);Assert.That(iconBounds.center.y,Is.EqualTo(0).Within(.1f));
                foreach(var c in ink){var p=body.InverseTransformPoint(copy.transform.TransformPoint(c.bottomLeft));Assert.That(p.y,Is.GreaterThan(body.rect.yMin+6));Assert.That(p.x,Is.GreaterThanOrEqualTo(iconBounds.xMax+3.4f*CenteredContentRow.Unit(body)));}
            }
        }
        void IconsFit()
        {
            var icons=game.GetComponentsInChildren<Image>().Where(i=>i.name=="Catalogue object"&&i.GetComponentInParent<Button>()?.name.StartsWith("Shop object ",StringComparison.Ordinal)==true).ToArray();Assert.That(icons.Length,Is.GreaterThan(0));
            foreach(var icon in icons){var card=(RectTransform)icon.GetComponentInParent<Button>().transform;var box=BoundsIn(icon.rectTransform,card);
                Assert.That(Vector2.Distance(box.center,card.rect.center),Is.LessThan(.1f));Assert.That(box.xMin,Is.GreaterThan(card.rect.xMin+24));Assert.That(box.xMax,Is.LessThan(card.rect.xMax-24));Assert.That(box.yMin,Is.GreaterThan(card.rect.yMin+20));Assert.That(box.yMax,Is.LessThan(card.rect.yMax-20));}
        }
        [UnityTest] public IEnumerator LoginGroupsAndShopIconsFitCentredOnPhoneLocales()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(string locale in new[]{"en","ru","uk"})foreach(bool large in new[]{false,true})foreach(int width in new[]{320,390}){
                Resize(width,width==320?693:844);game.SetUiLocale(locale);game.Preferences.LargeText=large;game.ShowAccountGate();yield return new WaitForSecondsRealtime(.6f);AccountCentres();
                yield return Shot(locale+"-welcome-"+width+(large?"-large":""),g=>AccountCentres());
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Continue without an account").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
                typeof(CascadeGame).GetField("shopTab",Private).SetValue(game,0);game.ShowShop();yield return new WaitForSecondsRealtime(.6f);IconsFit();
                if(large)yield return Shot(locale+"-shop-tools-"+width,g=>IconsFit());
                typeof(CascadeGame).GetField("shopTab",Private).SetValue(game,1);typeof(CascadeGame).GetMethod("DrawMenu",Private).Invoke(game,null);yield return new WaitForSecondsRealtime(.6f);IconsFit();
                if(large)yield return Shot(locale+"-shop-trophies-"+width,g=>IconsFit());
                if(large&&locale=="ru"){game.ShowCoinPacks();yield return Shot("ru-shop-coins-"+width);}
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator ClingingFeetStayOnTheFrameAndTakeoffCannotSnap()
        {
            string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(int world in new[]{0,5,6,8}){
                EditorType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});game.ShowHome();yield return null;
                var env=game.GetComponentsInChildren<MenuWorldEnvironment>().Single();env.SetReviewAge(0);var resident=game.GetComponentsInChildren<WorldFrameResident>().Single();
                var actor=(RectTransform)typeof(WorldFrameResident).GetField("actor",Private).GetValue(resident);var body=(RectTransform)typeof(WorldFrameResident).GetField("body",Private).GetValue(resident);
                float now=0;for(;now<16&&!resident.Resting;now+=.02f)env.SetReviewAge(now);Assert.That(resident.Resting,Is.True,"Every frame resident must land");
                var contact=resident.FeetPosition;var rotation=actor.rotation;
                if(WildlifePerchMotion.Clings(resident.Species)){
                    Assert.That(Mathf.Abs(Mathf.DeltaAngle(0,actor.localEulerAngles.z)),Is.EqualTo(90).Within(.1f));
                    var forward=actor.TransformVector(Vector3.right).normalized;Assert.That(Vector3.Dot(forward,Vector3.up),Is.GreaterThan(.99f),"Clinging insect's head should point along the post, not out into the sky");
                    var corners=new Vector3[4];body.GetWorldCorners(corners);var normal=actor.TransformDirection(Vector3.up).normalized;float bottom=corners.Min(p=>Vector3.Dot(p-contact,normal));float height=Vector3.Distance(corners[0],corners[1]);
                    Assert.That(Mathf.Abs(bottom),Is.LessThan(height*.04f),"The painted feet meet the support plane");
                }
                for(int i=0;i<50;i++){env.SetReviewAge(now+i*.02f);Assert.That(Vector3.Distance(resident.FeetPosition,contact),Is.LessThan(.00001f),"Resting feet cannot slide with breathing/wing folding");}
                yield return Shot("world-"+world+"-frame-contact",g=>env.SetReviewAge(now+.5f));
                var before=actor.rotation;Vector3 beforePoint=resident.FeetPosition;float at=now+.5f;
                for(;at<28&&resident.Resting;at+=.01f){before=actor.rotation;beforePoint=resident.FeetPosition;env.SetReviewAge(at);}
                Assert.That(resident.Resting,Is.False);Assert.That(Quaternion.Angle(before,actor.rotation),Is.LessThan(2),"Takeoff begins at the landed orientation");
                Assert.That(Vector3.Distance(beforePoint,resident.FeetPosition),Is.LessThan(.02f),"Takeoff begins at the actual planted feet");
                env.SetReviewAge(at);var still=resident.FeetPosition;env.SetReviewAge(at);Assert.That(resident.FeetPosition,Is.EqualTo(still));
                env.EndReview();
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        static Color32[] Pixels(Camera view,RenderTexture render,string name=null)
        {
            var previous=RenderTexture.active;var copy=new Texture2D(render.width,render.height,TextureFormat.RGBA32,false);
            try{Canvas.ForceUpdateCanvases();view.Render();RenderTexture.active=render;copy.ReadPixels(new Rect(0,0,render.width,render.height),0,0);copy.Apply();if(name!=null){Directory.CreateDirectory(Path.Combine(Output,"Motion"));File.WriteAllBytes(Path.Combine(Output,"Motion",name+".png"),copy.EncodeToPNG());}return copy.GetPixels32();}
            finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(copy);}
        }
        [UnityTest] public IEnumerator RearArtIsRenderedAndNoseFollowsTheCurvedRouteForOldAndNewFauna()
        {
            var asm=typeof(CascadeGame).Assembly;var libraryType=asm.GetType("Cascade.Presentation.WildlifeRigLibrary");var rigType=asm.GetType("Cascade.Presentation.WildlifeRig");
            var library=Activator.CreateInstance(libraryType,new[]{Field("art")});var holder=new GameObject("Isolated fauna render owner");holder.layer=31;
            var view=new GameObject("Isolated fauna capture camera",typeof(Camera)).GetComponent<Camera>();view.transform.position=new Vector3(0,0,-10);view.orthographic=true;view.orthographicSize=2.4f;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.10f,.36f,.52f);view.cullingMask=1<<31;
            var render=new RenderTexture(512,512,24);view.targetTexture=render;object rig=null;
            try{
                rig=Activator.CreateInstance(rigType,new object[]{holder.transform,"Native anatomy review",library,null,view});var root=(Transform)rigType.GetField("Root").GetValue(rig);
                foreach(var species in new[]{WildlifeSpecies.Swallow,WildlifeSpecies.Gull,WildlifeSpecies.Butterfly,WildlifeSpecies.Dragonfly,WildlifeSpecies.Bullfinch,WildlifeSpecies.BarnOwl,WildlifeSpecies.Bumblebee}){
                    Assert.That(WildlifeRearArt.Get(species),Is.Not.Null);rigType.GetMethod("Select").Invoke(rig,new object[]{species});
                    WildlifeMotion.Route(-1,.62f,true,false,out var a,out var b,out var c,out var d);bool rearSeen=false;
                    // Sample actual mobile frame intervals: the dorsal pose near a
                    // vertical tangent can last less than the old 300 ms sampling gap.
                    for(int i=0;i<=540;i++){
                        float age=i/30f;rigType.GetMethod("Fly").Invoke(rig,new object[]{view,a,b,c,d,age,18f,species==WildlifeSpecies.Bumblebee?10.8f:species==WildlifeSpecies.BarnOwl?1.3f:4.7f,.62f,.006f,0,-1f,.35f,10f,Color.white,.85f,Vector2.zero,-1f});
                        float rear=(float)rigType.GetProperty("RearAmount").GetValue(rig);rearSeen|=rear>.95f;
                        if(rear<.4f){var torso=root.Find("Side view/Torso");var forward=torso.TransformVector(Vector3.right).normalized;var h=WildlifeMotion.Heading(a,b,c,d,WildlifeMotion.Style(species),age,18,.62f,0,-1);Assert.That(Vector3.Dot(forward,h),Is.GreaterThan(.92f),species+" nose must follow travel, including the return arc");}
                        // A close-up magnifies the same sampled rig without changing its articulation.
                        root.position=Vector3.zero;root.localScale*=4;
                        if(i%36==0)Pixels(view,render,species+"-"+i.ToString("D3"));
                    }
                    Assert.That(rearSeen,Is.True,"The curve must actually show the rear art for "+species);
                    if(WildlifeFlightArt.Has(species)){
                        var torso=root.Find("Side view/Torso/Painted part");Assert.That(torso.localScale.x/torso.localScale.y,Is.GreaterThan(2.5f),"Flying birds must tuck their feet and use the horizontal torso");
                        rigType.GetMethod("PaintOnFeet").Invoke(rig,new object[]{Vector3.zero,Quaternion.identity,.35f,1f,WildlifeMotion.Pose(WildlifeMotion.Style(species),.4f,4,.3f),Color.white,1f});
                        Assert.That(torso.localScale.x/torso.localScale.y,Is.LessThan(1.5f),"Landing restores the original body with planted toes");
                    }
                    if(species==WildlifeSpecies.Swallow||species==WildlifeSpecies.Gull||species==WildlifeSpecies.Dragonfly){
                        var pose=WildlifeMotion.Pose(WildlifeMotion.Style(species),.4f,4,.3f);
                        rigType.GetMethod("Paint").Invoke(rig,new object[]{Vector3.zero,Quaternion.identity,1f,1f,pose,Color.white});
                        rigType.GetMethod("PaintRear",Private).Invoke(rig,new object[]{0f,Vector3.right,0f,pose,Color.white});
                        var appendage=root.Find("Side view/Tail or hindwing/Painted part").GetComponent<MeshRenderer>();Assert.That(appendage.enabled,Is.True,"The profile must keep the separately painted tail/hindwing: "+species);
                        var withTail=Pixels(view,render,species+"-profile");appendage.enabled=false;var withoutTail=Pixels(view,render);int tailPixels=0;
                        for(int p=0;p<withTail.Length;p++)if(Math.Abs(withTail[p].r-withoutTail[p].r)+Math.Abs(withTail[p].g-withoutTail[p].g)+Math.Abs(withTail[p].b-withoutTail[p].b)>16)tailPixels++;
                        Assert.That(tailPixels,Is.GreaterThan(20),"The preserved appendage must contribute actual visible pixels: "+species);
                    }
                    rigType.GetMethod("Paint").Invoke(rig,new object[]{Vector3.zero,Quaternion.identity,1f,1f,WildlifeMotion.Pose(WildlifeMotion.Style(species),.4f,4,.3f),Color.white});
                    rigType.GetMethod("PaintRear",Private).Invoke(rig,new object[]{1f,Vector3.up,0f,WildlifeMotion.Pose(WildlifeMotion.Style(species),.4f,4,.3f),Color.white});
                    var rearBody=root.Find("Rear view/Torso/Painted part").GetComponent<MeshRenderer>();Assert.That(rearBody.enabled,Is.True);
                    var with=Pixels(view,render,species+"-rear");rearBody.enabled=false;var without=Pixels(view,render);int changed=0;
                    for(int p=0;p<with.Length;p++)if(Math.Abs(with[p].r-without[p].r)+Math.Abs(with[p].g-without[p].g)+Math.Abs(with[p].b-without[p].b)>16)changed++;
                    Assert.That(changed,Is.GreaterThan(30),"Rear body must contribute actual rendered pixels: "+species);
                }
            }finally{
                if(rig!=null)rigType.GetMethod("Dispose").Invoke(rig,null);libraryType.GetMethod("Dispose").Invoke(library,null);
                UnityEngine.Object.DestroyImmediate(holder);UnityEngine.Object.DestroyImmediate(view.gameObject);render.Release();UnityEngine.Object.DestroyImmediate(render);
            }
            yield return null;
        }
        [UnityTest] public IEnumerator HomeWildlifeUsesCurvesAndStopsWithTheScreen()
        {
            game.ShowHome();yield return null;var home=game.GetComponentsInChildren<HomeSkyLife>().Single();var roots=home.GetComponentsInChildren<RectTransform>().Where(t=>t.name=="Distant bird"||t.name=="Distant insect").ToArray();Assert.That(roots.Length,Is.EqualTo(2));
            float minY=float.PositiveInfinity,maxY=float.NegativeInfinity;
            home.SetReviewSequence(197);
            home.SetReviewAge(0);
            for(int i=0;i<420;i++){
                float age=i/30f;home.SetReviewAge(age);var bird=roots.Single(t=>t.name=="Distant bird");
                if(!bird.GetComponentsInChildren<Image>().Any(p=>p.enabled))continue;
                minY=Mathf.Min(minY,bird.anchoredPosition.y);maxY=Mathf.Max(maxY,bird.anchoredPosition.y);
                if(i%20==0)Pixels(camera,portrait,"Home-flight-"+i.ToString("D3"));
            }
            Assert.That(maxY-minY,Is.GreaterThan(65),"Home wildlife needs broad curves rather than the old 22-unit sine lane");
            home.SetReviewAge(12);var positions=roots.Select(r=>r.anchoredPosition).ToArray();home.SetReviewAge(12);Assert.That(roots.Select(r=>r.anchoredPosition),Is.EqualTo(positions));
            game.ReducedMotion=true;home.SetReviewAge(12);Assert.That(home.GetComponentsInChildren<Image>().All(i=>!i.enabled),Is.True);game.ReducedMotion=false;
            home.EndReview();game.ShowShop();yield return null;Assert.That(game.GetComponentsInChildren<HomeSkyLife>().Length,Is.Zero);
        }
    }
}
#endif
