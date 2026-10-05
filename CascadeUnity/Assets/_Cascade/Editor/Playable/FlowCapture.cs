using System;
using System.Collections;
using System.Linq;
using Cascade.Domain;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class FlowCapture
    {
        public static void CaptureReviewSet()
        {
            if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed||EditorApplication.isCompiling)
                throw new InvalidOperationException("Requires a compiled running game.");
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.StartCoroutine(Capture(game));
        }
        static IEnumerator Capture(CascadeGame game)
        {
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            try {
                game.ShowHome();yield return null;
                yield return Shot("home-phone",390,844);yield return Shot("home-small",320,693);yield return Shot("home-wide",1440,900);
                game.SelectMode(PuzzleMode.Words);yield return null;yield return Shot("home-words",390,844);
                game.SelectMode(PuzzleMode.Shapes);game.LoadNode("C01");yield return null;yield return Shot("first-gesture",390,844);
                game.Interaction.Execute(PuzzleCommand.Swap(0));yield return Settle();yield return Shot("win-phone",390,844);
                game.LoadNode("C01");game.Interaction.Execute(PuzzleCommand.Drop);yield return Settle();yield return Shot("fail-small",320,693);
                game.LoadNode("C13");game.Coach.Advance(9,true);yield return null;yield return Shot("device-coaching",390,844);
                game.SendMessage("TogglePause");yield return null;yield return Shot("pause-phone",390,844);
                game.ShowDevices();yield return null;yield return Shot("device-guide",390,844);
                game.ShowGame();game.ShowSettings();yield return null;yield return Shot("settings-sound",390,844);
                Click("Comfort");yield return null;Click("Highcontrast:Off");yield return null;Click("Largertext:Off");yield return null;
                yield return Shot("settings-comfort-small",320,693);
                game.CloseMenu();game.LoadNode("C32");yield return null;yield return Shot("mixed8-large-text",320,693);
            }
            finally {UnityEngine.Application.runInBackground=background;game.ShowHome();}
            Debug.Log("CASCADE P14 screen matrix captured.");
            IEnumerator Shot(string file,int w,int h)
            { yield return PlayableCapture.CaptureSettled("P14",file+".png",w,h); }
            IEnumerator Settle()
            {float until=Time.realtimeSinceStartup+4;while(game.EventPlayer.IsRunning&&Time.realtimeSinceStartup<until)yield return null;yield return null;}
            void Click(string name)
            {game.GetComponentsInChildren<Button>().Single(b=>b.name==name).onClick.Invoke();}
        }
    }
}
