using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using Cascade.Solver;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class IntroductionReview
    {
        public static void Run()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Needs isolated review profile.");
            game.StartCoroutine(Inspect(game));
        }
        static IEnumerator Inspect(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;game.Preferences.Tutorials=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.SetUiLocale("ru");ShowFixture(MechanicLesson.Bomb);
            yield return Wait(()=>game.MechanicPopupOpen);
            Require(game.CurrentMechanic==MechanicLesson.Bomb,"first bomb before input, even when optional hints are off");
            string hash=game.Interaction.Attempt.CurrentStateHash;float clock=game.Machine.AmbientPhase;
            Require(!game.Interaction.PointerDown(17,0)&&game.Interaction.Execute(PuzzleCommand.Swap(0))==null,"input blocked");
            yield return new WaitForSecondsRealtime(.6f);Require(game.Interaction.Attempt.CurrentStateHash==hash&&Mathf.Abs(clock-game.Machine.AmbientPhase)<.001f,"row and wind frozen while reading");
            Require(!game.SeenMechanics.Contains(MechanicLesson.Bomb),"not acknowledged merely by appearance");
            game.SendMessage("OnApplicationFocus",false);yield return null;game.SendMessage("OnApplicationFocus",true);
            Require(game.MechanicPopupOpen&&!game.Interaction.CanPlay,"focus return does not dismiss or resume");
            yield return Shot("bomb-ru",390,844);Ack();yield return Ready();
            Require(new ProfileStore(game.ProfilePath).Load().Profile.SeenMechanics.Contains(MechanicLesson.Bomb),"acknowledgement saved");
            ShowFixture(MechanicLesson.Bomb);yield return Ready();Require(!game.MechanicPopupOpen,"known bomb is not repeated");
            // Every early effect is instantiated and acknowledged through the real UI.
            foreach(string node in new[]{"C02","C03","C04","C05","C06"}){Start(node);yield return Ready();}
            game.SetUiLocale("uk");game.Preferences.LargeText=true;ShowFixture(MechanicLesson.BombHourglass);
            yield return Wait(()=>game.MechanicPopupOpen);Require(game.CurrentMechanic==MechanicLesson.BombHourglass,"new hanging hourglass has its own explanation");
            yield return Shot("hourglass-uk-small",320,693);Ack();yield return Ready();game.Preferences.LargeText=false;
            foreach(string node in new[]{"C08","C09","C10"}){Start(node);yield return Ready();}
            foreach(var lesson in Enumerable.Range(0,12).Select(i=>(MechanicLesson)i).Where(l=>l!=MechanicLesson.LandingWind&&!game.SeenMechanics.Contains(l))){
                var fixture=game.Catalog.PrimaryLevels.Concat(game.Catalog.BonusLevels).Select(c=>c.Definition).First(l=>MechanicLessons.Discover(l,0,Array.Empty<int>()).Contains(lesson));
                game.LoadLevel(fixture);game.ShowGame();yield return Ready();Require(game.SeenMechanics.Contains(lesson),"queued "+lesson);
            }
            game.SetUiLocale("en");game.StartDescentLevel(game.Catalog.GetPrimary("C01").Definition);game.ShowGame();yield return Ready();
            foreach(var step in PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path){game.Interaction.Execute(step);yield return Ready();}
            yield return Wait(()=>game.CurrentScreen==GameScreen.Checkpoint);
            Require(game.CurrentScreen==GameScreen.Checkpoint,"first stage reached checkpoint");int stage=game.Descent.StageIndex;hash=game.Interaction.Attempt.CurrentStateHash;
            game.GoLower();Require(!game.MechanicPopupOpen&&game.Descent.StageIndex==stage,"landing wind does not interrupt descent with a tutorial");yield return Ready();
            Require(game.Descent.StageIndex==stage+1&&!game.SeenMechanics.Contains(MechanicLesson.LandingWind),"landing effect stays undisclosed until play");
            game.LoadNode("C01");game.ShowGame();yield return Ready();
            clock=game.Machine.AmbientPhase;hash=game.Interaction.Attempt.CurrentStateHash;
            yield return new WaitForSecondsRealtime(.5f);Require(!game.MechanicPopupOpen&&game.Interaction.Attempt.CurrentStateHash==hash&&game.Machine.AmbientPhase>clock,"ambient gust remains an unannounced visual surprise");
            var source=game.Catalog.GetPrimary("C02").Definition;int n=source.TargetSymbolIds.Count;
            game.LoadLevel(new LevelDefinition("intro-tool-review","intro-tool-review",new string('a',64),source.RulesVersion,"review",source.TargetSymbolIds.Reverse(),source.TargetSymbolIds,source.TargetSegments,
                new[]{new FloorDefinition(12,Enumerable.Repeat(true,n-1)),new FloorDefinition(12,Enumerable.Repeat(true,n-1))}));game.ShowGame();yield return Ready();
            game.Monetization.Ledger.WayfinderScrolls=1;game.SetUiLocale("ru");game.ShowSupplies();Click("Use selected tool");
            Require(game.CurrentMechanic==MechanicLesson.Wayfinder&&game.Stock(TravelTool.Wayfinder)==1&&!game.Interaction.Attempt.GuideActive,"tool explanation precedes consumption");
            yield return Shot("scroll-ru",390,844);game.ShowHome();
            Require(!game.SeenMechanics.Contains(MechanicLesson.Wayfinder)&&game.Stock(TravelTool.Wayfinder)==1,"interruption does not acknowledge or consume");
            game.ShowGame();yield return Ready();game.ShowSupplies();Click("Use selected tool");Ack();yield return Wait(()=>game.Stock(TravelTool.Wayfinder)==0);yield return Ready();
            var saved=new ProfileStore(game.ProfilePath).Load().Profile;
            Require(saved.SeenMechanics.Contains(MechanicLesson.Wayfinder)&&saved.Monetization.WayfinderScrolls==0&&game.Interaction.Attempt.GuideActive,"requested tool resumes once and saves both lesson and stock");
            for(int i=0;i<2;i++){game.Interaction.Execute(PuzzleCommand.Swap(0));yield return Ready();game.Undo();yield return null;}
            game.Interaction.Execute(PuzzleCommand.Swap(0));yield return Ready();game.Monetization.Ledger.UndoSpools=1;
            game.ShowSupplies();game.SelectShopItem(1);Click("Use selected tool");
            Require(game.CurrentMechanic==MechanicLesson.UndoSpool&&game.Stock(TravelTool.UndoSpool)==1,"spool pauses before consumption");Ack();yield return Ready();
            Require(game.Stock(TravelTool.UndoSpool)==0&&game.Interaction.Attempt.SpoolUsed,"spool resumes once");
            hash=game.Interaction.Attempt.CurrentStateHash;game.Interaction.Execute(PuzzleCommand.Drop);yield return Ready();game.Monetization.Ledger.ReturnHourglasses=1;
            game.ShowSupplies();game.SelectShopItem(2);Click("Use selected tool");
            Require(game.CurrentMechanic==MechanicLesson.ReturnHourglass&&game.Stock(TravelTool.ReturnHourglass)==1,"return hourglass pauses before consumption");Ack();yield return Ready();
            Require(game.Stock(TravelTool.ReturnHourglass)==0&&game.Interaction.Attempt.CurrentStateHash==hash,"hourglass resumes exact return once");
            game.LoadNode("C03");game.ShowGame();yield return Ready();
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/Introductions");
            File.WriteAllText(Path.Combine(folder,"runtime.txt"),"PASS: real modal button, input/row/weather pause, focus-return gate, acknowledgement and durable seen metadata, no repeat, non-wind puzzle effect lessons, hanging hourglass, no pre-descent wind tutorial, surprise gust, all three deferred tool consumptions, interrupted popup, exactly-once resumed Use. Optional hints off; first explanations still required. Five native Editor views, simulated safe insets, EN/RU/UK + small Large Text. Not physical-device input or player build.\n");
            Debug.Log("CASCADE first-mechanic review complete.");
            void Start(string id){game.StartDescentLevel(game.Catalog.GetPrimary(id).Definition);game.ShowGame();}
            void ShowFixture(MechanicLesson lesson){var level=game.Catalog.PrimaryLevels.Concat(game.Catalog.BonusLevels).Select(c=>c.Definition).First(l=>lesson==MechanicLesson.Bomb?l.Floors[0].Bomb!=null:l.Floors[0].Powerup?.Kind==PowerupKind.Hourglass);game.LoadLevel(level);game.ShowGame();}
            IEnumerator Ready(){float end=Time.realtimeSinceStartup+15;while(game.Interaction.IsPreparing||game.Interaction.IsPresenting||game.MechanicPopupOpen||game.IsDescending){if(game.MechanicPopupOpen){CheckText();Ack();}Require(Time.realtimeSinceStartup<end,"ready timeout");yield return null;}yield return null;}
            IEnumerator Wait(Func<bool> ready,float seconds=12){float end=Time.realtimeSinceStartup+seconds;while(!ready()){Require(Time.realtimeSinceStartup<end,"wait timeout");yield return null;}}
            IEnumerator Shot(string name,int w,int h){yield return new WaitForSecondsRealtime(.35f);yield return PlayableCapture.CaptureSettled("Introductions",name+".png",w,h);CheckText();}
            void Ack(){Click("Acknowledge mechanic");}
            void Click(string name){var b=game.GetComponentsInChildren<Button>().Single(b=>b.name==name);Require(b.interactable,"button enabled "+name);b.onClick.Invoke();}
            void CheckText(){foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.GetComponentInParent<Canvas>().sortingOrder==40)){t.ForceMeshUpdate();Require(!t.isTextOverflowing,"popup text overflow: "+t.name+" / "+t.text);}}
        }
        static void Require(bool ok,string detail){if(!ok)throw new InvalidOperationException("Introduction review: "+detail);}
    }
}
