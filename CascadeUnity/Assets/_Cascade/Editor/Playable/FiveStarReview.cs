using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Persistence;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public static class FiveStarReview
    {
        static string Folder=>Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/FiveStarResults");
        public static void Prepare()
        {
            MapCelebrationReview.Prepare();
            var store=new ProfileStore(Path.Combine(CascadeGame.EditorProfileDirectory,"cascade-profile.json"));var p=store.Load().Profile;
            string Read(string n)=>Resources.Load<TextAsset>("CascadeContent/"+n).text;
            var c=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
            p.Descent.Completed[0].ActiveSeconds=10;p.Descent.Completed[1].ActiveSeconds=11;p.Attempt.ActiveSeconds=12;
            var a=p.RestoreAttempt(c);p.Completion=LevelCompletion.From(a,null,p.Descent.Performance(a));p.Completion.Coins=40;p.Completion.Lights=1;p.Completion.ParcelOrder=1;p.Completion.NewBest=true;
            var clear=p.Clears.Single(x=>x.CampaignNodeId=="C03");clear.BestStars=p.Completion.Stars;clear.BestScore=p.Completion.Score;store.Save(p);
        }
        public static void Run()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated profile required");g.StartCoroutine(Inspect(g));
        }
        static IEnumerator Inspect(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.Preferences.LargeText=false;g.SetUiLocale("ru");g.ShowGame();yield return null;
            string hash=g.Interaction.Attempt.CurrentStateHash;int clears=g.JourneyClearCount;
            var ceremony=g.GetComponentsInChildren<RewardCelebration>().Last();yield return Shot("result-arrival",390,844);float early=ceremony.Progress;
            yield return Wait(3.5f);Check(ceremony.Progress>early&&ceremony.Progress==1,"finite five-star animation");yield return Shot("result-ru",390,844);
            Check(g.LastCompletion.Stars==5&&g.GetComponentsInChildren<Image>().Count(x=>x.name.StartsWith("Earned star "))==5,"five stars");
            Check(g.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Time statistic").text=="0:33","active time statistic");
            Check(!g.GetComponentsInChildren<Image>().Any(x=>x.name=="Completion cloth"||x.name=="Earned resources cloth"),"cream result cards absent");
            g.Preferences.LargeText=true;g.SetUiLocale("uk");yield return Shot("result-uk-small",320,693);
            g.Preferences.LargeText=false;g.SetUiLocale("en");g.SetEditorSafeInsets(Vector4.zero);yield return Shot("result-wide",1440,900);
            Check(g.JourneyClearCount==clears&&g.Interaction.Attempt.CurrentStateHash==hash,"ceremony never mutates receipts or puzzle");
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.SetUiLocale("ru");g.ShowShop();yield return Shot("shop-ru",390,844);
            g.ShowTravelBag();yield return Shot("bag-ru",390,844);g.ShowHome();g.ShowSettings();g.Preferences.LargeText=true;yield return Shot("settings-ru-small",320,693);
            g.GetComponentsInChildren<Button>().Single(b=>b.name=="Languages").onClick.Invoke();yield return Shot("languages-ru-small",320,693);
            g.Preferences.LargeText=false;g.ShowHome();yield return Shot("home-ru",390,844);
            g.LoadLevel(g.Catalog.GetPrimary("C02").Definition);g.ShowGame();g.RequireGustIntroduction();yield return Wait(.7f);
            if(g.MechanicPopupOpen){double before=g.Interaction.Attempt.ActiveSeconds.Value;yield return Shot("lesson-ru",390,844);yield return Wait(.2f);Check(g.Interaction.Attempt.ActiveSeconds==before,"first-use lesson excludes time");}
            for(int i=0;i<16&&g.MechanicPopupOpen;i++)g.AcknowledgeMechanic();
            yield return Wait(.4f);Check(g.Interaction.CanPlay,"timer test playable");double initial=g.Interaction.Attempt.ActiveSeconds.Value;yield return Wait(.3f);Check(g.Interaction.Attempt.ActiveSeconds>initial,"active game counts time");
            g.ShowSettings();double time=g.Interaction.Attempt.ActiveSeconds.Value;yield return Wait(.3f);Check(g.Interaction.Attempt.ActiveSeconds==time,"menu excludes time");
            g.CloseMenu();g.SendMessage("OnApplicationFocus",false);time=g.Interaction.Attempt.ActiveSeconds.Value;yield return Wait(.3f);Check(g.Interaction.Attempt.ActiveSeconds==time,"background excludes time");g.SendMessage("OnApplicationFocus",true);
            g.Interaction.SetPaused(true);time=g.Interaction.Attempt.ActiveSeconds.Value;yield return Wait(.3f);Check(g.Interaction.Attempt.ActiveSeconds==time,"pause excludes time");g.Interaction.SetPaused(false);
            g.Interaction.SetPreparing(true);time=g.Interaction.Attempt.ActiveSeconds.Value;yield return null;Check(g.Interaction.Attempt.ActiveSeconds==time,"preparation frame excludes time");g.Interaction.SetPreparing(false);
            var bounds=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(Folder,"result-uk-small.json")));foreach(var b in bounds["buttons"])Check((float)b["height"]>=47.9f&&(float)b["width"]>=47.9f,"48px completion action");
            File.WriteAllText(Path.Combine(Folder,"runtime.txt"),"PASS: isolated three-stage witness,5-star ceremony/time0:33, immutable receipts/hash, no cream results/menu reading plates, active time counts foreground playable time and excludes menus/background/pause/preparation and first-use lesson when shown. Portrait390,small320 LargeText/simulated safe insets,wide1440 captures. No phone build/install/full suite/device/performance/human enjoyment.\n");Debug.Log("CASCADE five-star review complete");
            IEnumerator Shot(string name,int w,int h){yield return PlayableCapture.CaptureSettled("FiveStarResults",name+".png",w,h);foreach(var t in g.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Check(!t.text.Contains("No translation found")&&!t.isTextOverflowing,"text fits "+t.name+" / "+t.text);}}
            IEnumerator Wait(float seconds){float elapsed=0;while(elapsed<seconds){elapsed+=Mathf.Min(.05f,Time.unscaledDeltaTime);yield return null;}}
        }
        public static void Resume()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated profile required");g.StartCoroutine(Remainder(g));
        }
        static IEnumerator Remainder(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            g.Preferences.LargeText=true;g.SetUiLocale("ru");g.ShowSettings();g.GetComponentsInChildren<Button>().Single(b=>b.name=="Languages").onClick.Invoke();yield return Shot("languages-ru-small",320,693);
            g.Preferences.LargeText=false;g.ShowJourney();g.GetComponentsInChildren<Button>().Single(b=>b.name=="Story").onClick.Invoke();yield return Shot("dialogue-ru",390,844);
            g.LoadLevel(g.Catalog.GetPrimary("C02").Definition);g.ShowGame();g.RequireGustIntroduction();yield return Wait(.7f);
            Check(g.MechanicPopupOpen,"explicit first-use wind lesson");double time=g.Interaction.Attempt.ActiveSeconds.Value;yield return Shot("lesson-ru",390,844);yield return Wait(.2f);Check(g.Interaction.Attempt.ActiveSeconds==time,"first-use modal excludes time");
            for(int i=0;i<16&&g.MechanicPopupOpen;i++)g.AcknowledgeMechanic();yield return Wait(.3f);
            g.SendMessage("ToggleTargetZoom");time=g.Interaction.Attempt.ActiveSeconds.Value;yield return Shot("target-ru",390,844);Check(g.Interaction.Attempt.ActiveSeconds==time,"target view excludes time");g.SendMessage("ToggleTargetZoom");
            g.ShowPreviewAd(false,ok=>{});time=g.Interaction.Attempt.ActiveSeconds.Value;yield return Shot("test-ad-ru",390,844);yield return Wait(.2f);Check(g.Interaction.Attempt.ActiveSeconds==time,"test-ad modal excludes time");g.GetComponentsInChildren<Button>().Single(b=>b.name=="Close test advertisement").onClick.Invoke();
            yield return Wait(.3f);Check(g.Interaction.CanPlay,"timer fixture playable");time=g.Interaction.Attempt.ActiveSeconds.Value;yield return Wait(.3f);Check(g.Interaction.Attempt.ActiveSeconds>time,"playable foreground counts time");
            g.ShowSettings();time=g.Interaction.Attempt.ActiveSeconds.Value;yield return Wait(.3f);Check(g.Interaction.Attempt.ActiveSeconds==time,"menu excludes time");g.CloseMenu();
            g.SendMessage("OnApplicationFocus",false);time=g.Interaction.Attempt.ActiveSeconds.Value;yield return Wait(.3f);Check(g.Interaction.Attempt.ActiveSeconds==time,"background excludes time");g.SendMessage("OnApplicationFocus",true);
            g.Interaction.SetPaused(true);time=g.Interaction.Attempt.ActiveSeconds.Value;yield return Wait(.3f);Check(g.Interaction.Attempt.ActiveSeconds==time,"pause excludes time");g.Interaction.SetPaused(false);
            g.Interaction.SetPreparing(true);time=g.Interaction.Attempt.ActiveSeconds.Value;yield return null;Check(g.Interaction.Attempt.ActiveSeconds==time,"preparation frame excludes time");g.Interaction.SetPreparing(false);
            var bounds=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(Folder,"result-uk-small.json")));foreach(var b in bounds["buttons"])Check((float)b["height"]>=47.9f&&(float)b["width"]>=47.9f,"48px result action");
            File.WriteAllText(Path.Combine(Folder,"runtime.txt"),"PASS: five-star result ceremony/time0:33 and immutable receipts/hash in main pass; no cream result/reading cards; corrected language captions/arrows and dialogue/wind lesson/target view/test ad native captures; foreground playable time counts and menus,background,pause,preparation frame,first-use modal,target view and local test ad exclude time. Small320 LargeText/simulated safe insets,portrait390,wide1440. No phone build/install/full suite/physical devices/performance/human enjoyment.\n");Debug.Log("CASCADE five-star native confirmation complete");
            IEnumerator Shot(string name,int w,int h){yield return PlayableCapture.CaptureSettled("FiveStarResults",name+".png",w,h);foreach(var t in g.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Check(!t.text.Contains("No translation found")&&!t.isTextOverflowing,"text fits "+t.name+" / "+t.text);}}
            IEnumerator Wait(float seconds){float elapsed=0;while(elapsed<seconds){elapsed+=Mathf.Min(.05f,Time.unscaledDeltaTime);yield return null;}}
        }
        static void Check(bool pass,string why){if(!pass)throw new InvalidOperationException("Five-star review: "+why);}
    }
}
