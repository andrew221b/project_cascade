using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class ModeJourneyReview
    {
        static string Folder=>Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/ModeJourneys");
        public static void Prepare()
        {
            NavigationCapture.Prepare();string Read(string n)=>Resources.Load<TextAsset>("CascadeContent/"+n).text;
            var catalog=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
            var source=catalog.GetPrimary("C02").Definition;var run=DescentRun.Build(source,version:3);
            var first=new AttemptService(run.Level(source,0));foreach(var c in run.Stages[0].Witness)first.Execute(c=="D"?PuzzleCommand.Drop:PuzzleCommand.Swap(int.Parse(c)));
            run.AcceptCheckpoint(source,first);var next=new AttemptService(run.Level(source,1));next.AdvanceActiveTime(12);
            var profile=new PlayerProfile{Attempt=AttemptRecord.Capture(next),Descent=run,JourneyAttempt=true};profile.HeardStory.Add(1000);
            profile.Clears.Add(new ClearReceipt{CampaignNodeId="C01",AttemptId=Guid.NewGuid().ToString("D"),ContentHash=catalog.GetPrimary("C01").Definition.ContentHash,BestStars=5,BestScore=2000});profile.Rewards.Synchronize(new[]{"C01"});
            new ProfileStore(Path.Combine(CascadeGame.EditorProfileDirectory,"cascade-profile.json")).Save(profile);
        }
        public static void Run()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated profile required.");
            g.StartCoroutine(Inspect(g));
        }
        static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);Debug.Log("MODE REVIEW PASS: "+label);}
        static IEnumerator Shot(string name,int w=390,int h=844)=>PlayableCapture.CaptureSettled("ModeJourneys",name+".png",w,h);
        static IEnumerator Inspect(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.Preferences.LargeText=true;g.SetUiLocale("ru");g.ShowJourney();yield return null;
            string shapeHash=g.Interaction.Attempt.CurrentStateHash;Check(g.Descent.StageIndex==1,"restored middle stage");yield return Shot("map-shapes");yield return Shot("map-small",320,693);
            var tabs=g.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Mapmode")).ToArray();Check(tabs.Length==3,"exactly three map tabs");
            tabs.Single(b=>b.name=="MapmodeWords").onClick.Invoke();yield return null;
            Check(g.PreferredMode==PuzzleMode.Words&&g.JourneyClearCount==0&&g.Descent==null,"Words starts independently");yield return Shot("map-words");
            g.GetComponentsInChildren<Button>().Single(b=>b.name=="MapmodeMixed").onClick.Invoke();yield return null;
            Check(g.PreferredMode==PuzzleMode.Mixed&&g.JourneyClearCount==0,"Mixed starts independently");
            Check(g.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Selected mode progress").text.Contains("160"),"Mixed has full 160-node route");
            g.GetComponentsInChildren<Button>().Single(b=>b.name=="MapmodeShapes").onClick.Invoke();yield return null;
            Check(g.Descent.StageIndex==1&&g.Interaction.Attempt.CurrentStateHash==shapeHash&&g.JourneyClearCount==1,"tab round trip keeps stage and exact row");
            var loaded=new ProfileStore(g.ProfilePath).Load();Check(loaded.Recovery==SaveRecovery.None&&loaded.Profile.ModeJourneys.Count==2&&loaded.Profile.Attempt.ActiveSeconds==12,"disk reload keeps three journeys and time");
            g.ShowHome();yield return Shot("home");Check(!g.GetComponentsInChildren<Button>().Any(b=>b.name=="Shapes"||b.name=="Words"||b.name=="Mixed"),"no duplicated home mode selector");
            g.ShowLessons();yield return Shot("level-tiles");g.ShowJourney();yield return null;
            var root=g.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Map mode tabs").parent.gameObject;
            var celebration=root.AddComponent<RewardCelebration>();celebration.Initialize(g,null,0,null,0,null,true);
            while(celebration.Progress<.30f)yield return null;
            var paper=g.GetComponentsInChildren<UnityEngine.UI.Image>().Where(i=>i.name.StartsWith("Celebration paper ")).ToArray();
            Check(paper.Length==24&&paper.Select(i=>i.sprite).Distinct().Count()==6,"24 individual pieces, six sliced sprites");
            var positions=paper.Select(i=>i.rectTransform.anchoredPosition).ToArray();for(int i=0;i<12;i++)yield return null;
            Check(paper.Where((p,i)=>(p.rectTransform.anchoredPosition-positions[i]).sqrMagnitude>1).Count()==24,"every paper body moves independently");
            g.SendMessage("OnApplicationFocus",false);yield return Shot("paper-arrival");g.SendMessage("OnApplicationFocus",true);
            while(celebration.Progress<.60f)yield return null;
            g.SendMessage("OnApplicationFocus",false);yield return Shot("paper-fall");g.SendMessage("OnApplicationFocus",true);
            while(celebration.Progress<1)yield return null;Check(paper.All(i=>i.color.a==0)&&celebration.Progress==1,"finite fade and completion");
            UnityEngine.Object.Destroy(celebration);yield return null;
            g.ReducedMotion=true;var quiet=root.AddComponent<RewardCelebration>();quiet.Initialize(g,null,0,null,0,null,true);yield return null;
            Check(!g.GetComponentsInChildren<UnityEngine.UI.Image>().Any(i=>i.name.StartsWith("Celebration paper ")),"Reduced Motion suppresses confetti");UnityEngine.Object.Destroy(quiet);g.ReducedMotion=false;
            Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"runtime.txt"),"PASS: native map tabs, separate progress, 160-node Mixed route, exact mid-stage and time disk round trip, no home mode selector, 24 independent cosmetic paper bodies / six alpha sprites / finite fade / Reduced Motion.\nEditor camera captures with simulated safe insets. No phone build/device test.\n");Debug.Log("CASCADE MODE REVIEW COMPLETE");
        }
    }
}
