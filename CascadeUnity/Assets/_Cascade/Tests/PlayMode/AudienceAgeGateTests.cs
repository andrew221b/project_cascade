#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class AudienceAgeGateTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture portrait,oldTarget;string directory;
        bool oldBackground,hadAge;int oldAge;

        [UnitySetUp] public IEnumerator Setup()
        {
            oldBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            hadAge=PlayerPrefs.HasKey(AudiencePolicy.PreferenceKey);oldAge=PlayerPrefs.GetInt(AudiencePolicy.PreferenceKey,0);
            directory=Path.Combine(Path.GetTempPath(),"CascadeAudienceAge",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=directory;CascadeGame.EditorAudienceAge=AudienceAgeBand.Unknown;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            portrait=new RenderTexture(390,844,24);
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;
            camera.targetTexture=portrait;game.SetEditorSafeInsets(new Vector4(0,.00001f,0,.00001f));
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            yield return null;game.RefreshLayout();
        }

        [UnityTearDown] public IEnumerator Teardown()
        {
            if(hadAge)PlayerPrefs.SetInt(AudiencePolicy.PreferenceKey,oldAge);else PlayerPrefs.DeleteKey(AudiencePolicy.PreferenceKey);
            PlayerPrefs.Save();
            if(camera!=null)camera.targetTexture=oldTarget;
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}
            CascadeGame.EditorProfileDirectory=null;CascadeGame.EditorAudienceAge=null;UnityEngine.Application.runInBackground=oldBackground;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }

        Button Button(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name&&b.gameObject.activeInHierarchy);
        bool HasButton(string name)=>game.GetComponentsInChildren<Button>().Any(b=>b.name==name&&b.gameObject.activeInHierarchy);
        void SetAge(AudienceAgeBand value)=>typeof(CascadeGame).GetField("audienceAgeBand",Private).SetValue(game,value);

        void Resize(int width,int height)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=null;
            portrait.Release();UnityEngine.Object.Destroy(portrait);portrait=new RenderTexture(width,height,24);camera.targetTexture=portrait;
            scaler.enabled=true;game.RefreshLayout();
        }

        void Capture(string filename)
        {
            var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/AgeGateFlowOct27"));
            Directory.CreateDirectory(folder);Canvas.ForceUpdateCanvases();camera.Render();var active=RenderTexture.active;
            var pixels=new Texture2D(portrait.width,portrait.height,TextureFormat.RGB24,false);
            try{RenderTexture.active=portrait;pixels.ReadPixels(new Rect(0,0,portrait.width,portrait.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,filename),pixels.EncodeToPNG());}
            finally{RenderTexture.active=active;UnityEngine.Object.Destroy(pixels);}
        }

        Vector2 ScreenPoint(Transform transform,Vector2 local)=>RectTransformUtility.WorldToScreenPoint(camera,transform.TransformPoint(local));
        Rect ScreenBounds(RectTransform rect)
        {
            var corners=new Vector3[4];rect.GetWorldCorners(corners);
            var low=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);var high=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
            return Rect.MinMaxRect(low.x,low.y,high.x,high.y);
        }
        void VerifyAgeChoice(string locale)
        {
            foreach(string name in new[]{"Age 16 or older yes","Age under 16 no"}){
                var button=Button(name);var rect=(RectTransform)button.transform;
                var corners=new Vector3[4];rect.GetWorldCorners(corners);
                var lo=camera.WorldToScreenPoint(corners[0]);var hi=camera.WorldToScreenPoint(corners[2]);
                Assert.That(hi.x-lo.x,Is.GreaterThanOrEqualTo(44),locale+" "+name+" width");
                Assert.That(hi.y-lo.y,Is.GreaterThanOrEqualTo(44),locale+" "+name+" height");
                var label=button.GetComponentInChildren<TextMeshProUGUI>();label.ForceMeshUpdate();
                Assert.That(label.isTextOverflowing||label.isTextTruncated,Is.False,locale+" "+name);
                var body=(RectTransform)label.transform.parent;
                var painted=ScreenPoint(label.transform,CenteredContentRow.GlyphBounds(label).center);
                var centre=ScreenPoint(body,body.rect.center);
                Assert.That(Vector2.Distance(painted,centre),Is.LessThan(1.6f),locale+" "+name+" must be optically centred");
            }
            foreach(var label in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.Contains("age choice"))){
                label.ForceMeshUpdate();Assert.That(label.isTextOverflowing||label.isTextTruncated,Is.False,locale+" "+label.name+" must stay inside its frame");
            }
            var panel=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Age choice paper");
            var panelBounds=ScreenBounds(panel.rectTransform);
            var question=game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name.EndsWith("age choice explanation",StringComparison.Ordinal));
            var questionBounds=ScreenBounds(question.rectTransform);
            var yes=ScreenBounds((RectTransform)Button("Age 16 or older yes").transform);
            var no=ScreenBounds((RectTransform)Button("Age under 16 no").transform);
            Assert.That(panelBounds.center.x,Is.EqualTo(portrait.width*.5f).Within(1),locale+" panel horizontal centre");
            foreach(var item in new[]{questionBounds,yes,no}){
                Assert.That(item.center.x,Is.EqualTo(panelBounds.center.x).Within(1),locale+" content horizontal centre");
                Assert.That(item.xMin,Is.GreaterThan(panelBounds.xMin+8),locale+" left inset");
                Assert.That(item.xMax,Is.LessThan(panelBounds.xMax-8),locale+" right inset");
                Assert.That(item.yMin,Is.GreaterThan(panelBounds.yMin+8),locale+" lower inset");
                Assert.That(item.yMax,Is.LessThan(panelBounds.yMax-8),locale+" upper inset");
            }
            Assert.That(questionBounds.Overlaps(yes),Is.False,locale+" question overlaps Yes");
            Assert.That(yes.Overlaps(no),Is.False,locale+" Yes overlaps No");
            float unit=panel.GetComponentInParent<Canvas>().scaleFactor;
            Assert.That((yes.yMin-no.yMax)/unit,Is.EqualTo(16).Within(.4f),locale+" logical gap between buttons");
            var group=Rect.MinMaxRect(questionBounds.xMin,no.yMin,questionBounds.xMax,questionBounds.yMax);
            Assert.That(group.center.y,Is.EqualTo(panelBounds.center.y).Within(1.5f),locale+" content stack vertical centre");
            Assert.That(Button("Age under 16 no").GetComponentsInChildren<Image>().Any(i=>i.sprite?.name=="Account Gold"),Is.True,locale+" No retains yellow skin");
            Assert.That(HasButton("Age 9 to 12"),Is.False);
            Assert.That(HasButton("Age 13 to 15"),Is.False);
            Assert.That(HasButton("Sign in with Apple"),Is.False);
            Assert.That(HasButton("Sign in with Google"),Is.False);
            Assert.That(HasButton("Continue without an account"),Is.False);
        }

        [UnityTest] public IEnumerator AgeChoiceRoutesAdultsThroughAccountBeforeHome()
        {
            SetAge(AudienceAgeBand.Unknown);game.ShowAccountGate();yield return null;
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);game.ShowAccountGate();yield return null;Canvas.ForceUpdateCanvases();
                VerifyAgeChoice(locale);
            }

            Assert.That(game.PurchaseStore,Is.Null,"Store preparation must not block first-run account choice.");
            Button("Age 16 or older yes").onClick.Invoke();yield return null;
            Assert.That(game.AudienceAgeBand,Is.EqualTo(AudienceAgeBand.SixteenPlus));
            Assert.That(PlayerPrefs.GetInt(AudiencePolicy.PreferenceKey),Is.EqualTo((int)AudienceAgeBand.SixteenPlus));
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.AccountGate),"Adults choose account sign-in before entering Home.");
            Assert.That(HasButton("Age 16 or older yes"),Is.False,"The age question must be replaced, not stacked over sign-in.");
            Assert.That(HasButton("Sign in with Apple")&&HasButton("Sign in with Google"),Is.True,"The account-choice screen must be visible.");
            Assert.That(HasButton("Continue without an account"),Is.True,"Account linking remains optional.");
            Assert.That(game.PurchaseStore,Is.Not.Null,"Native prices preload after the age choice while account UI remains usable.");
            Assert.That(game.AdProvider.Showing,Is.False,"An account choice never opens an ad.");
            Capture("02-account-after-yes-ru-390x844.png");
            Button("Continue without an account").onClick.Invoke();yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            Assert.That(game.PurchaseStore,Is.Not.Null,"The preloaded store remains available on Home.");

            // Choosing a different age in the actual app reloads the scene; do not
            // reuse the adult service objects from the first half of this test.
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;camera.targetTexture=portrait;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.SetEditorSafeInsets(new Vector4(0,.00001f,0,.00001f));yield return null;game.RefreshLayout();
            Assert.That(game.AudienceAgeNeedsChoice,Is.True);Assert.That(game.PurchaseStore,Is.Null);
            Button("Age under 16 no").onClick.Invoke();yield return null;
            Assert.That(game.AudienceAgeBand,Is.EqualTo(AudienceAgeBand.NineToTwelve));
            Assert.That(PlayerPrefs.GetInt(AudiencePolicy.PreferenceKey),Is.EqualTo((int)AudienceAgeBand.NineToTwelve));
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            Assert.That(typeof(CascadeGame).GetField("cloudSave",Private).GetValue(game),Is.Null);
            Assert.That(game.PurchaseStore,Is.Not.Null,"Under-16 players retain native purchases.");
            Assert.That(typeof(NativePurchaseStore).GetField("publicVerifier",Private).GetValue(game.PurchaseStore),Is.Not.Null,"Signed public receipt validation does not require a child cloud account.");
            Assert.That(typeof(NativePurchaseStore).GetField("purchaseIdentity",Private).GetValue(game.PurchaseStore),Is.Null,"No anonymous UGS purchase identity is created for the child.");
            Assert.That(game.AudienceIsChild&&game.AudienceUnderAgeOfConsent,Is.True);

        }

        [UnityTest] public IEnumerator AgeQuestionFitsSupportedPhoneSizesAndLanguages()
        {
            foreach((int width,int height) in new[]{(390,844),(320,693),(430,932),(768,1024)}){
                Resize(width,height);yield return null;
                foreach(string locale in new[]{"en","ru","uk"}){
                    game.SetUiLocale(locale);yield return new WaitForSecondsRealtime(.15f);
                    SetAge(AudienceAgeBand.Unknown);game.ShowAccountGate();yield return null;Canvas.ForceUpdateCanvases();
                    VerifyAgeChoice(locale+" "+width+"x"+height);
                    Capture("age-"+locale+"-"+width+"x"+height+".png");
                }
            }

            Resize(390,844);game.SetUiLocale("ru");yield return new WaitForSecondsRealtime(.20f);
            SetAge(AudienceAgeBand.Unknown);game.ShowAccountGate();yield return null;
            Capture("01-age-question-ru-390x844.png");
        }
    }
}
#endif
