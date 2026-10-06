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
    public static class JourneyCapture
    {
        public static void CaptureReviewSet()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Requires current running game.");
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Capture());
        }
        static IEnumerator Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            try {
                game.ShowJourney();yield return null;yield return Shot("map-phone",390,844);yield return Shot("map-small",320,693);yield return Shot("map-wide",1440,900);
                Click("Travel");yield return null;yield return Shot("mika-dialogue",390,844);yield return Shot("mika-dialogue-small",320,693);Click("Skipstory");yield return null;
                yield return Shot("direct-c01",390,844);yield return Shot("direct-c01-small",320,693);
                yield return PlayableCapture.CaptureSettled("Journey","held-piece.png",390,844,g=> {
                    var a=(RectTransform)g.SlotViews[0].transform;var b=(RectTransform)g.SlotViews[1].transform;
                    g.BeginTokenHold(0,RectTransformUtility.WorldToScreenPoint(Camera.main,a.TransformPoint(a.rect.center)));
                    g.PreviewDrag(0,1);g.MoveTokenHold(RectTransformUtility.WorldToScreenPoint(Camera.main,b.TransformPoint(b.rect.center)));
                });
                game.Interaction.Execute(PuzzleCommand.Swap(0));yield return Settle();yield return Shot("win",390,844);
                game.LoadNode("C01");game.Interaction.Execute(PuzzleCommand.Drop);yield return Settle();yield return Shot("fail",320,693);
                game.LoadNode("C13");game.Coach.Advance(9,true);yield return null;yield return Shot("bomb-garden",390,844);
                game.LoadNode("C22");yield return null;yield return Shot("rain-mill",390,844);yield return Shot("rain-small",320,693);
                game.LoadNode("C37");yield return null;yield return FlightShot("copper-orchard",390,844);
                game.LoadNode("C32");yield return null;yield return FlightShot("mixed8",320,693);
                game.SendMessage("TogglePause");yield return null;yield return Shot("pause",390,844);
                game.ShowSettings();yield return null;yield return Shot("settings",320,693);game.CloseMenu();game.ShowGame();
                game.ShowDevices();yield return null;yield return Shot("guide",390,844);game.ShowJourney();
                // Read-only future-story fixtures: no unlocks or rewards are granted.
                StoryFixture(15);yield return null;yield return Shot("pip-dialogue",390,844);game.ShowJourney();
                Click("Later");yield return null;Click("Later");yield return null;Click("Later");yield return null;yield return Shot("rain-map",390,844);
                StoryFixture(30);yield return null;yield return Shot("rue-dialogue-small",320,693);game.ShowJourney();
                for(int page=0;page<6;page++){Click("Later");yield return null;}yield return Shot("copper-map",390,844);
            }
            finally {UnityEngine.Application.runInBackground=background;game.EndTokenHold();game.ShowJourney();}
            Debug.Log("CASCADE Journey first visual matrix captured.");
            IEnumerator Shot(string name,int w,int h){yield return PlayableCapture.CaptureSettled("Journey",name+".png",w,h);}
            IEnumerator FlightShot(string name,int w,int h){yield return PlayableCapture.CaptureSettled("Journey",name+".png",w,h,g=>g.Machine.LivingWorld.Paint(3*Mathf.PI/(2*.056f)));}
            IEnumerator Settle(){float until=Time.realtimeSinceStartup+4;while(game.EventPlayer.IsRunning&&Time.realtimeSinceStartup<until)yield return null;yield return null;}
            void Click(string name){if(name=="Later"){game.TurnJourneyMap(1);return;}game.GetComponentsInChildren<Button>().Single(b=>b.name==name).onClick.Invoke();}
            void StoryFixture(int index){typeof(CascadeGame).GetMethod("OpenJourneyStory",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(game,new object[]{index,false});}
        }
    }
}
