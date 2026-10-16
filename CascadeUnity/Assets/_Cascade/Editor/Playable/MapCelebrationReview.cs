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
    public static class MapCelebrationReview
    {
        static string Folder=>Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/MapCelebration");
        public static void Prepare()
        {
            NavigationCapture.Prepare();
            string Read(string name)=>Resources.Load<TextAsset>("CascadeContent/"+name).text;
            var catalog=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
            var source=catalog.GetPrimary("C03").Definition;var run=DescentRun.Build(source,version:4);var p=new PlayerProfile{Descent=run,StoryRoute=true,JourneyAttempt=true,FirstGestureShown=true};p.HeardStory.Add(1000);
            AttemptService a=null;
            for(int stage=0;stage<3;stage++){
                a=new AttemptService(run.Level(source,stage));
                foreach(string c in run.Stages[stage].Witness){var command=c=="D"?PuzzleCommand.Drop:c.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c));Check(a.Execute(command).Accepted,"witness");}
                Check(a.Current.Status==PuzzleStatus.Won,"three-stage witness");if(stage<2)run.AcceptCheckpoint(source,a);
            }
            foreach(string id in new[]{"C01","C02"})p.Clears.Add(new ClearReceipt{CampaignNodeId=id,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=catalog.GetPrimary(id).Definition.ContentHash,BestStars=3});
            var completion=LevelCompletion.From(a,null,run.Performance(a));Check(p.RecordClear(a,completion),"committed completion");p.Rewards.Synchronize(p.Clears.Select(c=>c.CampaignNodeId));
            completion.Coins=40;completion.Lights=1;completion.ParcelOrder=1;p.Completion=completion;p.Attempt=AttemptRecord.Capture(a);
            new ProfileStore(Path.Combine(CascadeGame.EditorProfileDirectory,"cascade-profile.json")).Save(p);
        }
        public static void CaptureMapOnly()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated profile required");g.StartCoroutine(MapOnly(g));
        }
        static IEnumerator MapOnly(CascadeGame g)
        {
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.Preferences.LargeText=false;g.SetUiLocale("ru");g.ShowJourney();
            yield return PlayableCapture.CaptureSettled("MapCelebration","map-ru.png",390,844);
            g.Preferences.LargeText=true;g.SetUiLocale("uk");yield return PlayableCapture.CaptureSettled("MapCelebration","map-uk-small.png",320,693);
        }
        public static void Run()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated review required");g.StartCoroutine(Inspect(g));
        }
        static IEnumerator Inspect(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.SetUiLocale("ru");g.ShowGame();yield return null;
            string hash=g.Interaction.Attempt.CurrentStateHash;int clear=g.JourneyClearCount,parcels=g.Rewards.Parcels.Count;
            var ceremony=g.GetComponentsInChildren<RewardCelebration>().Last();Check(ceremony!=null,"statistics ceremony");
            yield return Shot("award-start",390,844);float early=ceremony.Progress;yield return Wait(.9f);yield return Shot("award-light",390,844);Check(ceremony.Progress>early,"award progresses");
            g.SendMessage("OnApplicationFocus",false);float paused=ceremony.Progress;yield return Wait(.25f);Check(Mathf.Abs(ceremony.Progress-paused)<.03f,"focus pauses ceremony");g.SendMessage("OnApplicationFocus",true);
            yield return Wait(3.3f);yield return Shot("award-settled",390,844);Check(ceremony.Progress>.99f,"award settled");
            g.ContinueCompletion();Check(g.CurrentScreen==GameScreen.Parcel,"parcel follows statistics");g.GetComponentsInChildren<Button>().Single(b=>b.name=="Openparcelgift").onClick.Invoke();
            Check(!g.Rewards.Parcels.Single(p=>p.Order==1).Claimed,"opening reveals without claiming");yield return Shot("parcel-open",390,844);yield return Wait(.65f);yield return Shot("parcel-reveal",390,844);yield return Wait(2.7f);yield return Shot("parcel-settled",390,844);g.ReducedMotion=true;yield return null;var quiet=g.GetComponentsInChildren<RewardCelebration>().Last();Check(quiet.Progress==1,"Reduced Motion settles reward");g.ReducedMotion=false;
            g.GetComponentsInChildren<Button>().Single(b=>b.name=="Claimgift").onClick.Invoke();Check(g.Rewards.Parcels.Single(p=>p.Order==1).Claimed,"claim commits after reveal");
            var prize=g.GetComponentsInChildren<Image>().Single(i=>i.name=="LeafPin");Check(prize.color.a>.99f&&Vector3.Distance(prize.transform.localScale,Vector3.one)<.001f,"prize final pose");
            g.GetComponentsInChildren<Button>().Single(b=>b.name=="Keepgift").onClick.Invoke();Check(g.CurrentScreen==GameScreen.JourneyMap,"map after reward");
            yield return Shot("map-ru",390,844);g.Preferences.LargeText=true;g.SetUiLocale("uk");yield return Shot("map-uk-small",320,693);
            var task=g.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Map step progress");Check(task.text.Contains("3/15"),"world pacing 3 of15");
            Check(!g.GetComponentsInChildren<Image>().Any(i=>i.name=="Map heading"||i.name=="Journey task"||i.name=="You are here marker"),"cream map cards removed");
            var previous=g.GetComponentsInChildren<Button>().Single(b=>b.name=="Previousopenedlevel");Check(previous.interactable,"replay available");previous.onClick.Invoke();Check(g.GetComponentsInChildren<Button>().Single(b=>b.name=="Nextopenedlevel").interactable,"return to current level");
            g.Preferences.LargeText=false;g.SetUiLocale("en");g.ReducedMotion=true;g.ShowJourney();yield return null;Check(g.GetComponentInChildren<MapParallax>().Offset==Vector2.zero,"reduced map motion");g.ReducedMotion=false;
            var bounds=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(Folder,"map-uk-small.json")));foreach(var b in bounds["buttons"])Check((float)b["height"]>=47.9f&&(float)b["width"]>=47.9f,"48px map target "+b["name"]);
            Check(g.JourneyClearCount==clear&&g.Rewards.Parcels.Count==parcels&&g.Interaction.Attempt.CurrentStateHash==hash,"presentation never grants clears or mutates row");
            File.WriteAllText(Path.Combine(Folder,"runtime.txt"),"PASS: replayed three-stage C03 witness in isolated profile (first two clear receipts fixture); statistics → claimed parcel → map, award timeline/focus pause/final pose, parcel reveal/final pose, immutable receipts/row, map 3/15 pacing, opened-level replay controls, no cream map cards, small Large Text and safe insets, Reduced Motion map. Eight maps cover unchanged160 levels. Editor only; devices/build/fun NOT RUN.\n");
            Debug.Log("CASCADE map and celebrations review complete.");
            IEnumerator Shot(string name,int w,int h){yield return PlayableCapture.CaptureSettled("MapCelebration",name+".png",w,h);foreach(var t in g.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Check(!t.text.Contains("No translation found")&&!t.isTextOverflowing,"text fits "+t.name+" / "+t.text);}}
            IEnumerator Wait(float seconds){float elapsed=0;while(elapsed<seconds){elapsed+=Mathf.Min(.05f,Time.unscaledDeltaTime);yield return null;}}
        }
        static void Check(bool pass,string why){if(!pass)throw new InvalidOperationException("Map celebration: "+why);}
    }
}
