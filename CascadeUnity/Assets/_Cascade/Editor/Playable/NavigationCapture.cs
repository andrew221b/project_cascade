using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Domain;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    [InitializeOnLoad]
    public static class NavigationCapture
    {
        const string Key="Cascade.NavigationReviewProfile";
        static NavigationCapture(){var folder=SessionState.GetString(Key,"");if(folder.Length>0)CascadeGame.EditorProfileDirectory=folder;}
        public static void Prepare()
        {
            var directory=Path.Combine(Path.GetTempPath(),"CascadeNavigationReview",Guid.NewGuid().ToString("N"));
            SessionState.SetString(Key,directory);CascadeGame.EditorProfileDirectory=directory;
        }
        public static void Clear(){SessionState.EraseString(Key);CascadeGame.EditorProfileDirectory=null;}
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(game==null||!EditorApplication.isPlaying)throw new InvalidOperationException("Requires compiled running game.");
            game.StartCoroutine(Matrix(game));
        }
        public static void CaptureMapFix()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(game==null||!EditorApplication.isPlaying)throw new InvalidOperationException("Requires compiled running game.");game.StartCoroutine(MapFix(game));
        }
        static IEnumerator MapFix(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            game.SetUiLocale("ru");game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.ChooseStory();game.ShowJourney();yield return new WaitForSecondsRealtime(.35f);
            string hash=game.Interaction.Attempt.CurrentStateHash;var motion=game.GetComponentInChildren<MapParallax>();var before=motion.Offset;yield return new WaitForSecondsRealtime(1);var after=motion.Offset;
            game.ReducedMotion=true;yield return null;yield return null;var quiet=motion.Offset;game.ReducedMotion=false;
            yield return PlayableCapture.CaptureSettled("Navigation","map-ru.png",390,844);
            yield return PlayableCapture.CaptureSettled("Navigation","map-ru-small.png",320,693);
            game.SetEditorSafeInsets(Vector4.zero);yield return PlayableCapture.CaptureSettled("Navigation","map-wide.png",1440,900);
            string file=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/Navigation/map-fix-runtime.txt");
            File.WriteAllText(file,"MCP Editor map reaction: before="+before+"; after="+after+"; reduced-motion="+quiet+"; unchanged puzzle hash="+(hash==game.Interaction.Attempt.CurrentStateHash)+"\nPhysical phone gravity was not tested.\n");
            Debug.Log("CASCADE map fix capture complete.");
        }
        static IEnumerator Matrix(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.SetUiLocale("en");game.ShowHome();yield return Settle();yield return Shot("home-en",390,844);
            game.SetUiLocale("ru");game.Preferences.LargeText=true;game.ShowHome();yield return Settle();yield return Shot("home-ru-small",320,693);
            game.Preferences.LargeText=false;game.SetUiLocale("uk");game.ShowPlayChoice();yield return Settle();yield return Shot("modes-uk",390,844);
            game.SetUiLocale("ru");game.ChooseSoloMode(PuzzleMode.Words);yield return Settle();yield return Shot("levels-ru-small",320,693);
            game.ShowPrologue();yield return Settle();yield return Shot("opening-1",390,844);
            game.AdvancePrologue();yield return Settle();yield return Shot("opening-2",390,844);
            game.AdvancePrologue();yield return Settle();yield return Shot("opening-3",390,844);
            game.AdvancePrologue();yield return Settle();yield return Shot("map-ru",390,844);
            game.SetEditorSafeInsets(Vector4.zero);yield return Shot("map-wide",1440,900);
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.ShowShop();yield return Settle();yield return Shot("shop-ru",390,844);
            game.ShowCollection();yield return Settle();yield return Shot("keepsakes-ru",390,844);
            game.ShowHome();game.ShowSettings();yield return Settle();yield return Shot("settings-ru-small",320,693);
            game.SetUiLocale("uk");game.ShowNoAds();yield return Settle();yield return Shot("no-ads-uk",390,844);
            game.SetUiLocale("ru");game.ShowJourney();game.GetComponentsInChildren<Button>().Single(b=>b.name=="Story").onClick.Invoke();yield return Settle();yield return Shot("dialogue-ru",390,844);
            game.ShowHome();game.SetEditorSafeInsets(Vector4.zero);
            File.WriteAllText(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/Navigation/capture-complete.txt"),"14 native Editor render captures; isolated profile; portrait safe insets and wide map; no physical-device visual/gyro claim.\n");
            Debug.Log("CASCADE navigation visual matrix complete.");
            IEnumerator Shot(string name,int w,int h){yield return PlayableCapture.CaptureSettled("Navigation",name+".png",w,h);}
            IEnumerator Settle(){yield return null;yield return new WaitForSecondsRealtime(.30f);}
        }
    }
}
