using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class CatalogFlowReview
    {
        const string Folder="CatalogFlow-Oct08";
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null)throw new InvalidOperationException("Start the isolated playable scene first.");
            game.StartCoroutine(Review(game));
        }
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        static Button Button(CascadeGame game,string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        static bool Visible(CascadeGame game,string name)=>game.GetComponentsInChildren<RectTransform>().Any(r=>r.name==name);
        static IEnumerator Review(CascadeGame game)
        {
            yield return new WaitForSecondsRealtime(.5f);
            UnityEngine.Application.runInBackground=true;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            game.SetUiLocale("ru");game.ShowHome();game.ShowShop();
            yield return new WaitForSecondsRealtime(.25f);
            Check(game.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Shop object ",StringComparison.Ordinal))==5,"Tool shelf must have five items, without the guide.");
            Button(game,"Shop object 2").onClick.Invoke();
            Check(game.CurrentScreen==GameScreen.Shop&&!Visible(game,"Selected tool illustration")&&!Visible(game,"Selected object description"),"Tool tap must select without opening preview or repeating its description.");
            yield return PlayableCapture.CaptureSettled(Folder,"01-shop-selected-ru.png",390,844);
            Button(game,"Lookcloser").onClick.Invoke();
            Check(Visible(game,"Selected tool illustration"),"View must open tool detail.");
            Button(game,"Back").onClick.Invoke();
            Check(Visible(game,"Shop object 2")&&!Visible(game,"Selected tool illustration"),"Back must return to selected tool.");

            game.SelectShopTab(1);
            Check(Visible(game,"Shop object 0"),"First trophy page missing.");
            yield return PlayableCapture.CaptureSettled(Folder,"02-trophies-page1-ru.png",390,844);
            Button(game,"Nexttrophies").onClick.Invoke();
            Check(game.CurrentScreen==GameScreen.Shop&&!Visible(game,"Trophy appearance preview")&&Visible(game,"Shop object 8"),"Pager must not open preview.");
            yield return PlayableCapture.CaptureSettled(Folder,"03-trophies-page2-ru.png",390,844);
            Button(game,"Lookcloser").onClick.Invoke();
            Check(Visible(game,"Trophy appearance preview"),"View must open trophy preview.");
            yield return PlayableCapture.CaptureSettled(Folder,"04-trophy-preview-ru.png",390,844);
            Button(game,"Back").onClick.Invoke();
            Check(!Visible(game,"Trophy appearance preview")&&Visible(game,"Shop object 8"),"Back must return to the same trophy page.");
            game.SelectShopItem(0);Button(game,"Lookcloser").onClick.Invoke();
            yield return PlayableCapture.CaptureSettled(Folder,"05-fragment-preview-ru.png",390,844);
            Button(game,"Back").onClick.Invoke();

            game.ShowHome();game.ShowShop();Button(game,"Back").onClick.Invoke();
            Check(game.CurrentScreen==GameScreen.Home,"Shop back must reach Home.");
            yield return PlayableCapture.CaptureSettled(Folder,"06-home-ru.png",390,844);
            Button(game,"Mythings").onClick.Invoke();
            Check(game.CurrentScreen==GameScreen.TravelBag,"My things must open from Home.");
            yield return PlayableCapture.CaptureSettled(Folder,"07-bag-ru.png",390,844);
            Button(game,"Myjournal").onClick.Invoke();Check(game.CurrentScreen==GameScreen.Journal,"Journal must be usable with an empty profile.");
            yield return PlayableCapture.CaptureSettled(Folder,"08-journal-ru.png",390,844);
            Button(game,"Back").onClick.Invoke();Check(game.CurrentScreen==GameScreen.TravelBag,"Journal back must return to My things.");
            Button(game,"Back").onClick.Invoke();Check(game.CurrentScreen==GameScreen.Home,"My things back must return Home.");
            game.ShowTravelBag();game.ShowSupplies();
            Check(game.CurrentScreen==GameScreen.Supplies&&!Visible(game,"Shop object 5"),"Bag must not contain the guide tile.");
            yield return PlayableCapture.CaptureSettled(Folder,"09-items-ru.png",390,844);
            game.Preferences.LargeText=true;game.SetUiLocale("uk");
            yield return PlayableCapture.CaptureSettled(Folder,"10-items-uk-large.png",320,693);
            game.ShowShop();Check(game.CurrentScreen==GameScreen.Shop,"Items to shop failed.");
            Button(game,"Back").onClick.Invoke();Check(game.CurrentScreen==GameScreen.Supplies,"Shop back must return to Items.");
            game.ShowCollection();game.SelectShopItem(0);
            yield return PlayableCapture.CaptureSettled(Folder,"11-trophies-uk-large.png",320,693);
            game.ShowHome();game.ShowSettings();yield return PlayableCapture.CaptureSettled(Folder,"12-settings-uk-large.png",320,693);
            Button(game,"Languages").onClick.Invoke();yield return PlayableCapture.CaptureSettled(Folder,"13-languages-uk-large.png",320,693);

            var folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Folder);
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"validation.txt"),"PASS: item tap selects only, description appears in preview, trophy pager stays in catalogue, Shop back reaches Home, My things and Journal navigation return correctly, guide tile absent. Thirteen native RU/UK captures; isolated profile. Physical-device touch not run.\n");
            Debug.Log("CASCADE catalogue flow review complete.");
        }
    }
}
