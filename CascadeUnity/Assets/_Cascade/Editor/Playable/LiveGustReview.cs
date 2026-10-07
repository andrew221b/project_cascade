using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public static class LiveGustReview
    {
        const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static string Folder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/LiveGust"));
        static void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);File.AppendAllText(Path.Combine(Folder,"native.txt"),"PASS "+text+"\n");}
        public static void Run(){var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("QA profile required");Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"native.txt"),"");g.StartCoroutine(Review(g));}
        static IEnumerator Review(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.SetUiLocale("ru");g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            g.ShowGame();yield return null;
            var ceremony=g.GetComponentsInChildren<RewardCelebration>().Last();string hash=g.Interaction.Attempt.CurrentStateHash;
            while(ceremony.Progress<.10f)yield return null;Check(g.GetComponentsInChildren<Image>().Any(i=>i.name.StartsWith("Earned star")&&i.color.a>.5f&&i.transform.localScale.x>2),"star begins at screen scale");
            yield return PlayableCapture.CaptureSettled("LiveGust","star-arriving.png",390,844);
            while(ceremony.Progress<1)yield return null;Check(ceremony.Progress==1,"ceremony completes");Check(g.Interaction.Attempt.CurrentStateHash==hash,"ceremony preserves state");
            yield return PlayableCapture.CaptureSettled("LiveGust","stars-settled.png",390,844);
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Flags).GetValue(g);profile.Completion=null;
            foreach(MechanicLesson l in Enum.GetValues(typeof(MechanicLesson)))if(l!=MechanicLesson.LiveGust&&!profile.SeenMechanics.Contains(l))profile.SeenMechanics.Add(l);
            var source=g.Catalog.GetPrimary("C06").Definition;var run=DescentRun.Build(source,version:5);var attempt=new AttemptService(run.Level(source,1));
            g.LoadLevel(attempt.Level);g.ShowGame();yield return new WaitForSecondsRealtime(.4f);
            float introDeadline=Time.realtimeSinceStartup+8;while(!g.MechanicPopupOpen&&Time.realtimeSinceStartup<introDeadline)yield return null;
            Check(g.CurrentMechanic==MechanicLesson.LiveGust,"first-use live gust popup pauses play");
            yield return PlayableCapture.CaptureSettled("LiveGust","gust-introduction.png",390,844);
            while(g.MechanicPopupOpen){g.AcknowledgeMechanic();yield return null;}
            var command=Cascade.Solver.PuzzleSolver.FindShortest(new PuzzleResolver(attempt.Level)).Path.First(c=>c.Kind!=CommandKind.EarlyDrop);
            var before=g.Interaction.Attempt.Current;var result=g.Interaction.Execute(command);g.ReportInput(result);
            Check(result.Events.Any(e=>e.Kind==PuzzleEventKind.WindPermuted&&e.Floor==0),"gust occurs on playable upper board");
            Check(result.Events.Any(e=>e.Kind==PuzzleEventKind.FloorExited),"weak board breaks after gust damage");
            while(!g.ReactionBanner.IsWindWarning)yield return null;yield return PlayableCapture.CaptureSettled("LiveGust","wind-warning.png",390,844);
            while(g.ReactionBanner.IsShowing)yield return null;yield return new WaitForSecondsRealtime(.18f);yield return PlayableCapture.CaptureSettled("LiveGust","wind-sweep.png",390,844);
            float timeout=Time.realtimeSinceStartup+15;while(g.Interaction.IsPresenting){if(Time.realtimeSinceStartup>timeout)throw new Exception("Wind presentation timeout");yield return null;}
            Check(g.Interaction.Attempt.Current.Status==PuzzleStatus.Playing&&g.Interaction.Attempt.Current.ActiveFloor==1,"gust leaves a playable lower board");
            yield return PlayableCapture.CaptureSettled("LiveGust","wind-landed.png",390,844);
            var world=g.Machine.LivingWorld;world.SetZone(0);var island=world.IslandPosition(0);yield return new WaitForSecondsRealtime(2);Check(Vector3.Distance(island,world.IslandPosition(0))>.03f,"visible garden island drift over two seconds");
            yield return PlayableCapture.CaptureSettled("LiveGust","garden-ambience.png",390,844);
            for(int zone=1;zone<8;zone++){world.SetZone(zone);world.Paint(world.Phase+.5f);Check(world.ActiveWeatherCount>0||zone==3||zone==7,"region ambient layer "+zone);}
            world.SetZone(4);yield return PlayableCapture.CaptureSettled("LiveGust","lagoon-ambience.png",390,844);
            g.ShowJourney();g.Preferences.LargeText=true;yield return PlayableCapture.CaptureSettled("LiveGust","map-small.png",320,693);
            File.WriteAllText(Path.Combine(Folder,"native-status.txt"),"PASS");Debug.Log("Live gust native review PASS");
        }
    }
}
