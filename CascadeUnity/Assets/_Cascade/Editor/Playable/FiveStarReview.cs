using System;
using System.Collections;
using System.Collections.Generic;
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
        static readonly Dictionary<Sprite,Rect> spriteInk=new Dictionary<Sprite,Rect>();
        static string Folder=>Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/FiveStarResults");
        [MenuItem("CASCADE/Review/Victory/Prepare whole rating")]
        public static void Prepare()
        {
            MapCelebrationReview.Prepare();
            var store=new ProfileStore(Path.Combine(CascadeGame.EditorProfileDirectory,"cascade-profile.json"));var p=store.Load().Profile;
            string Read(string n)=>Resources.Load<TextAsset>("CascadeContent/"+n).text;
            var c=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
            p.Descent.Completed[0].ActiveSeconds=10;p.Descent.Completed[1].ActiveSeconds=11;p.Attempt.ActiveSeconds=12;
            var a=p.RestoreAttempt(c);p.Completion=LevelCompletion.From(a,null,p.Descent.Performance(a));p.Completion.Coins=40;p.Completion.Lights=1;p.Completion.ParcelOrder=1;p.Completion.NewBest=true;
            var clear=p.Clears.Single(x=>x.CampaignNodeId=="C03");clear.BestStars=p.Completion.Stars;clear.BestHalfStar=p.Completion.HalfStar;clear.BestScore=p.Completion.Score;store.Save(p);
        }
        [MenuItem("CASCADE/Review/Victory/Check layout")]
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
            yield return Wait(5.15f);Check(ceremony.Progress>=early&&ceremony.Progress==1,"finite three-star animation");yield return Shot("result-ru",390,844);
            yield return Shot("result-ru-360",360,648);
            Check(g.LastCompletion.Stars==3&&g.GetComponentsInChildren<Image>().Count(x=>x.name.StartsWith("Earned star "))==3,"three stars");
            var victoryNumbers=g.GetComponentsInChildren<VictoryScoreDigits>();
            Check(victoryNumbers.Single(n=>n.name=="Earned score glyphs").FormattedValue=="1800","score has no thousands separator");
            Check(victoryNumbers.Single(n=>n.name=="Time statistic").FormattedValue=="0:33","active time uses the themed digit set");
            Check(victoryNumbers.Single(n=>n.name=="Earned coins").FormattedValue=="×40"
                &&victoryNumbers.Single(n=>n.name=="Earned light").FormattedValue=="×1","reward counts use the themed digit set");
            Check(g.GetComponentsInChildren<Image>().Any(x=>x.name=="Themed multiplication glyph"&&x.gameObject.activeInHierarchy)
                &&g.GetComponentsInChildren<Image>().Any(x=>x.name=="Themed colon glyph"&&x.gameObject.activeInHierarchy),"generated multiplication and time symbols are visible");
            Check(!g.GetComponentsInChildren<Button>().Any(b=>b.name=="Retry completed level"||b.name=="Completion map"),"claim-trophy state has no side actions");
            var plaque=g.GetComponentsInChildren<Image>().Single(x=>x.name=="Score medal").rectTransform;
            Check(plaque.anchorMax.x-plaque.anchorMin.x>=.779f&&Mathf.Abs(plaque.anchorMin.y-.5285f)<.002f
                &&Mathf.Abs(plaque.anchorMax.y-.6985f)<.002f,"score plaque matches the reference wide, shallow proportions and position");
            var earnedScoreArea=(RectTransform)g.GetComponentsInChildren<VictoryScoreDigits>().Single(n=>n.name=="Earned score glyphs").transform;
            Check(Mathf.Abs(earnedScoreArea.anchorMin.y-.20f)<.002f&&Mathf.Abs(earnedScoreArea.anchorMax.y-.52f)<.002f,
                "score digits stay inside the lower plaque field with balanced padding");
            var starRects=g.GetComponentsInChildren<Image>().Where(x=>x.name.StartsWith("Earned star ")).Select(x=>x.rectTransform).ToArray();
            Check(starRects.Length==3&&Mathf.Abs(starRects[0].anchorMin.x-.0475f)<.002f&&Mathf.Abs(starRects[1].anchorMin.x-.3525f)<.002f
                &&Mathf.Abs(starRects[2].anchorMin.x-.6575f)<.002f&&Mathf.Abs(starRects[0].anchorMin.y-.6903f)<.002f,"three large stars match the reference spacing and vertical gap");
            Check(starRects.All(x=>Mathf.Abs((x.anchorMax.x-x.anchorMin.x)-.295f)<.002f)
                &&starRects.Zip(starRects.Skip(1),(a,b)=>b.anchorMin.x-a.anchorMax.x).All(gap=>gap>=-.001f),"three star images stay in adjacent ordered slots");
            var banner=g.GetComponentsInChildren<Image>().Single(x=>x.name=="Completion ribbon").rectTransform;
            Check(banner.anchoredPosition.y<0,"victory ribbon has an inward safe-area margin");
            var completionTitle=g.GetComponentsInChildren<TMP_Text>().Single(x=>x.name=="Completion title");
            Check(completionTitle.fontStyle==FontStyles.Normal&&completionTitle.rectTransform.anchoredPosition.y>0,"completion headline is normal-weight and raised to the ribbon's visual center");
            Check(g.GetComponentsInChildren<Image>().Count(x=>x.name.StartsWith("Mika opacity correction pass "))==0,"Mika has no duplicate opacity layers");
            var mika=g.GetComponentsInChildren<Image>().Single(x=>x.name=="Mika victory lantern");
            Check(mika.color.a==1&&mika.GetComponent<CanvasGroup>()==null
                &&Mathf.Abs(mika.rectTransform.localScale.x-.72f)<.002f&&mika.rectTransform.anchoredPosition.y>8,"Mika keeps full opacity, authored proportions and a raised position above the claim action");
            var sunWash=g.GetComponentsInChildren<RectTransform>().Single(x=>x.name=="Victory sun wash");
            Check(mika.transform.GetSiblingIndex()>sunWash.GetSiblingIndex(),"Mika renders above the warm screen wash");
            Check(mika.sprite.rect.height/mika.sprite.rect.width<1.35f&&Mathf.Abs(mika.rectTransform.localScale.x-mika.rectTransform.localScale.y)<.001f,"Mika uses a compact source cutout and uniform scaling");
            var scoreDigits=g.GetComponentsInChildren<VictoryScoreDigits>().Single(n=>n.name=="Earned score glyphs");
            Check(scoreDigits.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Score digit ")).All(i=>i.GetComponent<Outline>()!=null),"score numerals retain the dark reference outline");
            var scoreRect=(RectTransform)scoreDigits.transform;var scoreCorners=new Vector3[4];scoreRect.GetWorldCorners(scoreCorners);
            for(int i=0;i<scoreCorners.Length;i++)scoreCorners[i]=plaque.InverseTransformPoint(scoreCorners[i]);
            Check(scoreCorners.All(p=>Mathf.Abs(p.x)<=plaque.rect.width*.5f&&Mathf.Abs(p.y)<=plaque.rect.height*.5f),"score glyph area stays inside the record plaque");
            Check(g.GetComponentsInChildren<Image>().Where(x=>x.name.EndsWith(" medal")&&new[]{"Time medal","Moves medal","Undos medal"}.Contains(x.name))
                .All(x=>Mathf.Abs(x.rectTransform.localScale.x-.80f)<.002f&&Mathf.Abs(x.rectTransform.localScale.y-.80f)<.002f),"result statistic medals are reduced evenly without stretching");
            var multiplication=g.GetComponentsInChildren<Image>().First(x=>x.name=="Themed multiplication glyph"&&x.gameObject.activeInHierarchy);
            var rewardDigits=victoryNumbers.Single(n=>n.name=="Earned coins").GetComponentsInChildren<Image>();
            var firstDigit=rewardDigits.First(x=>x.name.StartsWith("Score digit ")&&x.gameObject.activeInHierarchy);
            Check(multiplication.GetComponent<Outline>()==null
                &&multiplication.rectTransform.rect.height<firstDigit.rectTransform.rect.height*.7f,
                "multiplication sign is smaller than the reward digits and does not receive a square uGUI outline");
            var oneRewardDigit=victoryNumbers.Single(n=>n.name=="Earned light").GetComponentsInChildren<Image>()
                .First(x=>x.name.StartsWith("Score digit ")&&x.gameObject.activeInHierarchy);
            Check(Mathf.Abs(firstDigit.rectTransform.rect.height-oneRewardDigit.rectTransform.rect.height)<.5f,
                "coin and light reward amounts use the same digit size");
            Check(!g.GetComponentsInChildren<Image>().Any(x=>x.name=="Completion cloth"||x.name=="Earned resources cloth"),"cream result cards absent");
            g.Preferences.LargeText=true;g.SetUiLocale("uk");yield return Shot("result-uk-small",320,693);
            var headline=g.GetComponentsInChildren<TMP_Text>().Single(x=>x.name=="Completion title");string settledHeadline=headline.text;
            headline.text="Beautifully done!";headline.ForceMeshUpdate();
            Check(!headline.isTextOverflowing&&headline.textInfo.lineCount==1,"the longest completion rank title shrinks onto the ribbon");
            headline.text=settledHeadline;headline.ForceMeshUpdate();
            g.Preferences.LargeText=false;g.SetUiLocale("en");g.SetEditorSafeInsets(Vector4.zero);yield return Shot("result-wide",1440,900);
            var bounds=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(Folder,"result-uk-small.json")));foreach(var b in bounds["buttons"])Check((float)b["height"]>=47.9f&&(float)b["width"]>=47.9f,"48px completion action");
            Check(g.JourneyClearCount==clears&&g.Interaction.Attempt.CurrentStateHash==hash,"ceremony never mutates receipts or puzzle");
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.SetUiLocale("ru");g.ShowShop();yield return Shot("shop-ru",390,844);
            g.ShowSupplies();Check(!g.GetComponentsInChildren<RectTransform>(true).Any(r=>r.name=="Catalogue guidance")
                &&!g.GetComponentsInChildren<TMP_Text>(true).Any(t=>t.text=="Choose a tool"),"travel bag has no redundant instruction papyrus");
            yield return Shot("bag-ru",390,844);g.Preferences.LargeText=true;g.SetUiLocale("uk");g.ShowSupplies();yield return Shot("bag-final-uk",320,693);
            g.ShowHome();g.ShowTravelBag();yield return Shot("my-things-final-uk",320,693);
            var journalButton=g.GetComponentsInChildren<Button>().Single(b=>b.name=="My journal navigation");var journalCaption=journalButton.GetComponentInChildren<TMP_Text>();
            journalCaption.ForceMeshUpdate();Check(!journalCaption.isTextOverflowing&&journalCaption.textInfo.lineCount==1,"Ukrainian journal tab fits in a single line");
            g.Preferences.LargeText=false;g.SetUiLocale("ru");g.ShowHome();g.ShowSettings();g.Preferences.LargeText=true;yield return Shot("settings-ru-small",320,693);
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
            File.WriteAllText(Path.Combine(Folder,"runtime.txt"),"PASS: isolated three-stage witness, three-star ceremony/time0:33, 1800 score without separator, visible star gaps, reference-proportioned score plaque, lowered reward/stat groups inside the safe area, smaller statistic digits, closer separated ribbon/stars, restored larger record plaque, regular-weight headline with light outline, compact one-line bounded claim button, visible Mika, smaller ribbon title, stars > reward icons > statistic medals, rendered statistic gaps >=6px, immutable receipts/hash, no redundant travel-bag instruction plate, active time counts foreground playable time and excludes menus/background/pause/preparation and first-use lesson when shown. Portrait390, small320 LargeText/simulated safe insets, wide1440 captures. No phone build/install/full suite/device/performance/human enjoyment.\n");Debug.Log("CASCADE three-star review complete");
            IEnumerator Shot(string name,int w,int h){yield return PlayableCapture.CaptureSettled("FiveStarResults",name+".png",w,h,x=>{if(name=="result-ru"||name=="result-ru-360"||name=="result-uk-small"||name=="result-wide")CheckVictorySpacing(x);});foreach(var t in g.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Check(!t.text.Contains("No translation found")&&!t.isTextOverflowing,"text fits "+t.name+" / "+t.text);}}
            IEnumerator Wait(float seconds){yield return new WaitForSecondsRealtime(seconds);}
        }
        [MenuItem("CASCADE/Review/Victory/Prepare half rating")]
        public static void PrepareHalf()
        {
            Prepare();var store=new ProfileStore(Path.Combine(CascadeGame.EditorProfileDirectory,"cascade-profile.json"));var p=store.Load().Profile;
            string Read(string n)=>Resources.Load<TextAsset>("CascadeContent/"+n).text;
            var c=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
            p.Descent.Completed[0].ActiveSeconds=p.Descent.Completed[1].ActiveSeconds=p.Attempt.ActiveSeconds=300;
            var a=p.RestoreAttempt(c);p.Completion=LevelCompletion.From(a,null,p.Descent.Performance(a));p.Completion.Coins=40;p.Completion.Lights=1;p.Completion.ParcelOrder=1;p.Completion.NewBest=true;
            Check(p.Completion.StarRating==2.5,"actual slow optimal witness earns 2.5");
            var clear=p.Clears.Single(x=>x.CampaignNodeId=="C03");clear.BestStars=p.Completion.Stars;clear.BestHalfStar=p.Completion.HalfStar;clear.BestScore=p.Completion.Score;store.Save(p);
        }
        [MenuItem("CASCADE/Review/Victory/Check half rating")]
        public static void RunHalf()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated profile required");g.StartCoroutine(InspectHalf(g));
        }
        static IEnumerator InspectHalf(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
            g.ReducedMotion=false;g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.SetUiLocale("ru");g.ShowGame();yield return null;
            Check(g.LastCompletion.StarRating==2.5&&new ProfileStore(g.ProfilePath).Load().Profile.Completion.StarRating==2.5,"real half-rating survives profile save and restore");
            var ceremony=g.GetComponentsInChildren<RewardCelebration>().Last();ceremony.SetPreview(4.85f);
            var fills=g.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Star colour fill ")).OrderBy(i=>i.name).ToArray();
            Check(fills.Length==3&&fills[0].fillAmount==1&&fills[1].fillAmount==1&&fills[2].fillAmount==.5f,"real receipt drives a half-filled third star");
            yield return PlayableCapture.CaptureSettled("FiveStarResults","real-half-rating-ru.png",390,844,_=>ceremony.SetPreview(4.85f));
            g.Preferences.LargeText=true;g.SetUiLocale("uk");g.ShowGame();yield return null;ceremony=g.GetComponentsInChildren<RewardCelebration>().Last();ceremony.SetPreview(4.85f);
            yield return PlayableCapture.CaptureSettled("FiveStarResults","real-half-rating-uk-small.png",320,693,_=>ceremony.SetPreview(4.85f));
            g.ShowJourney();yield return PlayableCapture.CaptureSettled("FiveStarResults","real-half-map-uk.png",320,693);
            g.OpenJourneyIsland(0);yield return PlayableCapture.CaptureSettled("FiveStarResults","real-half-levels-uk.png",320,693);
            Check(g.GetComponentsInChildren<Image>().Any(i=>i.name.StartsWith("Rating fill ")&&i.fillAmount==.5f),"level list displays the actual half-star best");
            File.WriteAllText(Path.Combine(Folder,"half-runtime.txt"),"PASS: real slow optimal three-stage witness rates 2.5, saves/restores as 2.5, drives two full and one half star, and appears with halves in level cards and map totals. RU390 and UK320 Large Text native captures. No device build/install or live cloud sync claim.\n");Debug.Log("CASCADE real half-star native review complete");
        }
        [MenuItem("CASCADE/Review/Victory/Check animation")]
        public static void RunAnimation()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated profile required");
            g.StartCoroutine(InspectAnimation(g));
        }
        static IEnumerator InspectAnimation(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.ReducedMotion=false;g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.SetUiLocale("ru");g.ShowGame();yield return null;
            var ceremony=g.GetComponentsInChildren<RewardCelebration>().Last();
            string hash=g.Interaction.Attempt.CurrentStateHash,receipt=Newtonsoft.Json.JsonConvert.SerializeObject(g.LastCompletion);
            var stars=g.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Earned star ")).OrderBy(i=>i.name).ToArray();
            var fills=g.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Star colour fill ")).OrderBy(i=>i.name).ToArray();
            var counters=g.GetComponentsInChildren<VictoryScoreDigits>().ToDictionary(c=>c.name,c=>c);
            ceremony.SetPreview(4.85f);var targets=counters.ToDictionary(p=>p.Key,p=>p.Value.FormattedValue);
            ceremony.SetPreview(0);
            Check(stars.Length==3&&fills.Length==3&&stars.All(i=>i.color.a==0)&&fills.All(i=>i.fillAmount==0),"all stars start invisible and unfilled");
            Check(counters.All(p=>p.Value.FormattedValue==(p.Key=="Time statistic"?"0:00":p.Key=="Earned coins"||p.Key=="Earned light"?"×0":"0")),"all illustrated counters begin at zero");
            ceremony.SetPreview(.38f);Check(stars[0].color.a>0&&stars[1].color.a==0&&stars[2].color.a==0,"first empty star arrives alone");
            ceremony.SetPreview(.78f);Check(stars[0].color.a==1&&stars[1].color.a>0&&stars[2].color.a==0,"second empty star follows the first");
            ceremony.SetPreview(1.74f);Check(stars.All(i=>i.color.a==1)&&fills.All(i=>i.fillAmount==0),"all three hollow stars settle before filling starts");
            ceremony.SetPreview(2.30f);Check(fills[0].fillAmount>0&&fills[0].fillAmount<1&&fills[1].fillAmount==0&&fills[2].fillAmount==0,"gold fills continuously from the first star");
            Check(int.Parse(counters["Earned score glyphs"].FormattedValue)>0&&int.Parse(counters["Earned score glyphs"].FormattedValue)<g.LastCompletion.Score,"score is intermediate during count-up");
            Check(int.Parse(counters["Earned coins"].FormattedValue.Substring(1))>0&&int.Parse(counters["Earned coins"].FormattedValue.Substring(1))<g.LastCompletion.Coins,"coins are intermediate during count-up");
            float[] previousFills={0,0,0};int[] previousNumbers=new int[counters.Count];
            for(int frame=0;frame<=21;frame++){
                float time=frame*.23f;ceremony.SetPreview(time);
                yield return PlayableCapture.CaptureSettled("FiveStarResults","animation-"+frame.ToString("00")+".png",390,844,_=>ceremony.SetPreview(time));
                for(int i=0;i<fills.Length;i++){Check(fills[i].fillAmount>=previousFills[i],"star fill never reverses");previousFills[i]=fills[i].fillAmount;}
                int at=0;foreach(var counter in counters.Values){string text=counter.FormattedValue;var parts=text.Split(':');int value=parts.Length==2?int.Parse(parts[0])*60+int.Parse(parts[1]):int.Parse(text.TrimStart('×'));Check(value>=previousNumbers[at],"counter never reverses: "+counter.name);previousNumbers[at++]=value;}
            }
            Check(counters.All(p=>p.Value.FormattedValue==targets[p.Key])&&fills.All(i=>i.fillAmount==1),"all counters and earned star fills finish exactly at receipt values");
            ceremony.SetPreviewRating(2.5f);ceremony.SetPreview(4.85f);
            Check(fills[0].fillAmount==1&&fills[1].fillAmount==1&&fills[2].fillAmount==.5f,"2.5 preview finishes with exactly half of the third star");
            yield return PlayableCapture.CaptureSettled("FiveStarResults","animation-half-rating.png",390,844,_=>ceremony.SetPreview(4.85f));
            ceremony.SetPreviewRating(1);ceremony.SetPreview(1.74f);Check(stars.All(i=>i.color.a==1)&&fills.All(i=>i.fillAmount==0),"unearned stars also arrive empty");
            ceremony.SetPreview(4.85f);Check(fills[0].fillAmount==1&&fills[1].fillAmount==0&&fills[2].fillAmount==0,"unearned stars remain hollow");
            ceremony.SetPreviewRating((float)g.LastCompletion.StarRating);g.ReducedMotion=true;ceremony.SetPreview(0);
            Check(counters.All(p=>p.Value.FormattedValue==targets[p.Key])&&fills.All(i=>i.fillAmount==1),"reduced motion shows the final receipt immediately");
            g.ReducedMotion=false;ceremony.SetPreview(.5f);ceremony.ResumePreview();g.SendMessage("OnApplicationFocus",false);float before=ceremony.Progress;
            yield return new WaitForSecondsRealtime(.15f);Check(ceremony.Progress==before,"background freezes the animation clock");g.SendMessage("OnApplicationFocus",true);
            ceremony.SetPreview(4.85f);
            Check(g.Interaction.Attempt.CurrentStateHash==hash&&Newtonsoft.Json.JsonConvert.SerializeObject(g.LastCompletion)==receipt,"animation and half-rating preview never change the receipt or puzzle state");
            File.WriteAllText(Path.Combine(Folder,"animation-runtime.txt"),"PASS: three empty stars arrive sequentially before any fill; continuous left-to-right gold fill; 1, 2.5 preview and 3 final layouts; all six illustrated counters start at zero, advance monotonically and finish at exact receipt values; background clock pause and reduced-motion final state; immutable receipt and gameplay hash. Half-rating preview is independent of the receipt; real version-3 ratings also support halves. No device build/install/full TestRunner suite.\n");
            Debug.Log("CASCADE victory animation review complete");
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
            File.WriteAllText(Path.Combine(Folder,"runtime.txt"),"PASS: three-star result ceremony/time0:33 and immutable receipts/hash in main pass; no cream result/reading cards; corrected language captions/arrows and dialogue/wind lesson/target view/test ad native captures; foreground playable time counts and menus,background,pause,preparation frame,first-use modal,target view and local test ad exclude time. Small320 LargeText/simulated safe insets,portrait390,wide1440. No phone build/install/full suite/physical devices/performance/human enjoyment.\n");Debug.Log("CASCADE three-star native confirmation complete");
            IEnumerator Shot(string name,int w,int h){yield return PlayableCapture.CaptureSettled("FiveStarResults",name+".png",w,h,x=>{if(name=="result-ru"||name=="result-ru-360"||name=="result-uk-small"||name=="result-wide")CheckVictorySpacing(x);});foreach(var t in g.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Check(!t.text.Contains("No translation found")&&!t.isTextOverflowing,"text fits "+t.name+" / "+t.text);}}
            IEnumerator Wait(float seconds){yield return new WaitForSecondsRealtime(seconds);}
        }
        static void CheckVictorySpacing(CascadeGame g)
        {
            var primary=g.GetComponentsInChildren<Button>().Single(b=>b.name=="Claim trophy");
            var caption=primary.GetComponentInChildren<TMP_Text>();caption.ForceMeshUpdate();
            var buttonRect=(RectTransform)primary.transform;
            var canvas=buttonRect.GetComponentInParent<Canvas>();
            var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            Rect Bounds(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);var pixels=c.Select(point=>RectTransformUtility.WorldToScreenPoint(camera,point)).ToArray();return Rect.MinMaxRect(pixels.Min(p=>p.x),pixels.Min(p=>p.y),pixels.Max(p=>p.x),pixels.Max(p=>p.y));}
            Rect VisibleBounds(Image illustration){
                var bounds=Bounds(illustration.rectTransform);var sprite=illustration.sprite;
                if(illustration.preserveAspect){
                    float aspect=sprite.rect.width/sprite.rect.height;
                    float width=bounds.width,height=bounds.height;
                    if(width/height>aspect)width=height*aspect;else height=width/aspect;
                    bounds=new Rect(bounds.center-new Vector2(width,height)*.5f,new Vector2(width,height));
                }
                var ink=SpriteInk(sprite);
                return new Rect(bounds.x+ink.x*bounds.width,bounds.y+ink.y*bounds.height,ink.width*bounds.width,ink.height*bounds.height);
            }
            var ribbon=g.GetComponentsInChildren<Image>().Single(i=>i.name=="Completion ribbon");
            var safe=Bounds(g.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="SafeArea"));
            var ribbonBounds=Bounds(ribbon.rectTransform);
            var starBounds=g.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Earned star ")).Select(VisibleBounds).ToArray();
            var plaqueBounds=VisibleBounds(g.GetComponentsInChildren<Image>().Single(i=>i.name=="Score medal"));
            Check(ribbonBounds.yMax<=safe.yMax-7.9f,"victory ribbon remains below the actual safe-area ceiling");
            Check(VisibleBounds(ribbon).yMin-starBounds.Max(b=>b.yMax)>=6,"ribbon and stars have a visible gap");
            Check(starBounds.Min(b=>b.yMin)-plaqueBounds.yMax>=6,"stars and record plaque have a visible gap");
            Check(starBounds.OrderBy(b=>b.x).Zip(starBounds.OrderBy(b=>b.x).Skip(1),(a,b)=>b.xMin-a.xMax).All(gap=>gap>=6),"closer star silhouettes remain separate");
            float expectedHeight=62f*Mathf.Max(1,canvas.scaleFactor);
            Check(Mathf.Abs(Bounds(buttonRect).height-expectedHeight)<.3f,"claim button retains its compact height after menu fitting");
            float IllustrationSpan(Image illustration){
                var bounds=Bounds(illustration.rectTransform);float width=bounds.width,height=bounds.height;
                float aspect=illustration.sprite.rect.width/illustration.sprite.rect.height;
                if(illustration.preserveAspect){if(width/height>aspect)width=height*aspect;else height=width/aspect;}
                return Mathf.Max(width,height);
            }
            var illustrations=g.GetComponentsInChildren<Image>();
            float smallestStar=illustrations.Where(i=>i.name.StartsWith("Earned star ")).Min(IllustrationSpan);
            var rewards=illustrations.Where(i=>i.name=="Earned gold illustration"||i.name=="Earned lights illustration").Select(IllustrationSpan).ToArray();
            float largestStatistic=illustrations.Where(i=>new[]{"Time medal","Moves medal","Undos medal"}.Contains(i.name)).Max(IllustrationSpan);
            Check(rewards.Length==2&&smallestStar>rewards.Max()&&rewards.Min()>largestStatistic*1.3f,"stars exceed rewards, and both reward illustrations exceed statistic medals by at least 30 percent");
            Check(caption.textInfo.lineCount==1&&!caption.isTextOverflowing,"claim trophy stays on one line in every locale and size");
            var textBounds=caption.textBounds;
            foreach(var point in new[]{textBounds.min,textBounds.max}){
                var local=buttonRect.InverseTransformPoint(caption.transform.TransformPoint(point));
                Check(buttonRect.rect.Contains(local),"claim caption remains inside the button face");
            }
            RectTransform Find(string name)=>g.GetComponentsInChildren<RectTransform>().Single(r=>r.name==name);
            var medals=new[]{"Time","Moves","Undos"}.Select(name=>Bounds(Find(name+" medal"))).ToArray();
            Check(medals.Zip(medals.Skip(1),(a,b)=>b.xMin-a.xMax).All(gap=>gap>=6),"statistic icons retain a visible horizontal gap");
            foreach(string name in new[]{"Time","Moves","Undos"}){
                var icon=Bounds(Find(name+" medal"));
                var images=Find(name+" statistic").GetComponentsInChildren<Image>().Where(i=>i.gameObject.activeInHierarchy).ToArray();
                Check(images.Length>0&&icon.yMin-images.Max(i=>Bounds(i.rectTransform).yMax)>=6,"icon clears statistic numerals: "+name);
            }
        }
        static Rect SpriteInk(Sprite sprite)
        {
            if(spriteInk.TryGetValue(sprite,out var ink))return ink;
            string path=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).FullName,AssetDatabase.GetAssetPath(sprite.texture));
            var source=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try{
                if(!ImageConversion.LoadImage(source,File.ReadAllBytes(path)))throw new InvalidOperationException("Unreadable illustration: "+path);
                var pixels=source.GetPixels32();var crop=sprite.rect;
                float sx=source.width/(float)sprite.texture.width,sy=source.height/(float)sprite.texture.height;
                crop=new Rect(crop.x*sx,crop.y*sy,crop.width*sx,crop.height*sy);
                int left=source.width,right=-1,bottom=source.height,top=-1;
                for(int y=Mathf.Max(0,Mathf.FloorToInt(crop.yMin));y<Mathf.Min(source.height,Mathf.CeilToInt(crop.yMax));y++)
                    for(int x=Mathf.Max(0,Mathf.FloorToInt(crop.xMin));x<Mathf.Min(source.width,Mathf.CeilToInt(crop.xMax));x++)
                        if(pixels[y*source.width+x].a>=32){left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}
                if(right<left||top<bottom)throw new InvalidOperationException("Empty illustration");
                ink=new Rect((left-crop.xMin)/crop.width,(bottom-crop.yMin)/crop.height,(right-left+1)/crop.width,(top-bottom+1)/crop.height);
                spriteInk[sprite]=ink;return ink;
            }finally{UnityEngine.Object.DestroyImmediate(source);}
        }
        static void Check(bool pass,string why){if(!pass)throw new InvalidOperationException("Five-star review: "+why);}
    }
}
