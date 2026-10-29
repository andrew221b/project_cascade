using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using Cascade.Solver;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    // Native visual fixtures use solver-proved receipts in an isolated profile.
    // Actual claim/input/storage behavior is covered by PlayMode tests.
    public static class MetaCapture
    {
        const BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
        public static void Prepare()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Prepare in Play mode.");
            CascadeGame.EditorProfileDirectory=Path.Combine(Path.GetTempPath(),"CascadeMetaCapture",Guid.NewGuid().ToString("N"));
            UnityEngine.SceneManagement.SceneManager.LoadScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
        }
        public static void Run()
        {
            if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Needs current Play mode.");
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Capture());
        }
        public static void RunExtra()
        {
            if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Needs current Play mode.");
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Extra());
        }
        static IEnumerator Extra()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(game,"persistenceReady",false);
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);Seed(game,profile);
            var findings=new List<string>();game.SetUiLocale("en");game.SendMessage("OnApplicationFocus",true);
            game.ShowWorkshop();Invoke(game,"DrawMenu");yield return Shot("rain-pump","en",game,findings);
            Invoke(game,"DrawMenu");yield return Shot("post-glider","en",game,findings);
            game.ShowCollection();Field(game,"giftPage",2);Invoke(game,"DrawMenu");yield return Shot("sky-ribbon","en",game,findings);
            game.ShowJourney();Field(game,"persistenceReady",true);
            File.WriteAllLines(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/P17/extra-fit-findings.txt")),findings);
            Debug.Log("CASCADE extra meta frames complete: "+findings.Count+" findings.");
        }
        static void Invoke(CascadeGame game,string method)=>typeof(CascadeGame).GetMethod(method,Private).Invoke(game,null);
        static void Field(CascadeGame game,string field,object value)=>typeof(CascadeGame).GetField(field,Private).SetValue(game,value);
        static void Seed(CascadeGame game,PlayerProfile profile)
        {
            foreach(var content in game.Catalog.PrimaryLevels.Take(10).Concat(new[]{game.Catalog.GetPrimary("C18"),game.Catalog.GetPrimary("C33"),game.Catalog.GetPrimary("C35")})) {
                int goal=content.Definition.CampaignNodeId=="C35"?1:0;
                var a=new AttemptService(content.Definition,content.Locale,new PuzzleGoal(goal));
                foreach(var c in PuzzleSolver.FindShortest(a.Resolver,goal:a.Goal).Path)a.Execute(c);
                profile.RecordClear(a);profile.Rewards.RecordDelivery(a);
            }
            profile.Rewards.Synchronize(profile.Clears.Select(r=>r.CampaignNodeId));
            foreach(var p in profile.Rewards.Parcels.Take(3))profile.Rewards.ClaimParcel(p.Order);
            var clears=(HashSet<string>)typeof(CascadeGame).GetField("sessionClears",Private).GetValue(game);
            clears.UnionWith(profile.Clears.Select(r=>r.CampaignNodeId));
            for(int i=0;i<7;i++)profile.Rewards.RegisterVisit(new DateTime(2026,10,5).AddDays(i));
        }
        static IEnumerator Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Field(game,"persistenceReady",false);game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            var findings=new List<string>();
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
            try {
                game.ShowRecords();yield return Shot("empty-routes","en",game,findings);
                game.ShowTravelBag();yield return Shot("locked-bag","en",game,findings);
                Seed(game,profile);
                foreach(string locale in new[]{"en","ru","uk"}) {
                    game.SetUiLocale(locale);game.SendMessage("OnApplicationFocus",true);yield return null;
                    game.ShowTravelBag();yield return Shot("bag",locale,game,findings);
                    game.ShowJournal();yield return Shot("journal",locale,game,findings);
                    game.ShowCollection();Field(game,"giftPage",0);Invoke(game,"DrawMenu");yield return Shot("owned-gift",locale,game,findings);
                    Field(game,"giftPage",3);Invoke(game,"DrawMenu");yield return Shot("missing-gift",locale,game,findings);
                    game.ShowWorkshop();Invoke(game,"DrawMenu");yield return Shot("built-workshop",locale,game,findings);
                    Invoke(game,"DrawMenu");yield return Shot("future-workshop",locale,game,findings);
                    game.ShowRequests();Field(game,"requestPage",0);Invoke(game,"DrawMenu");yield return Shot("request-done",locale,game,findings);
                    Field(game,"requestPage",2);Invoke(game,"DrawMenu");yield return Shot("request-pending",locale,game,findings);
                    game.ShowParcel();yield return Shot("parcel",locale,game,findings);
                    var pending=profile.Rewards.Parcels.First(p=>!p.Claimed);profile.Rewards.ClaimParcel(pending.Order);
                    Field(game,"parcelOpened",true);Invoke(game,"DrawMenu");yield return Shot("parcel-open",locale,game,findings);
                    pending.Claimed=false; // Reset the disclosed visual fixture for the next language.
                    game.ShowContracts();yield return Shot("stamp-trail",locale,game,findings);
                    game.ShowRecords();yield return Shot("route",locale,game,findings);
                }
                game.SetUiLocale("en");game.ShowJournal();yield return Shot("journal-phone","en",game,findings,390,844);
                game.ShowWorkshop();Invoke(game,"DrawMenu");yield return Shot("workshop-wide","en",game,findings,1440,900);
                game.ShowJourney();yield return Shot("equipped-map","en",game,findings,390,844);
            }
            finally {game.SetUiLocale("en");game.ShowJourney();Field(game,"persistenceReady",true);UnityEngine.Application.runInBackground=background;}
            File.WriteAllLines(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/P17/ui-fit-findings.txt")),findings);
            Debug.Log("CASCADE meta captures complete: "+findings.Count+" findings.");
        }
        static IEnumerator Shot(string name,string locale,CascadeGame game,List<string> findings,int width=320,int height=693)
        {
            yield return PlayableCapture.CaptureSettled("P17",locale+"-"+name+".png",width,height,g=>{
                foreach(var t in g.GetComponentsInChildren<TMP_Text>()) {
                    t.ForceMeshUpdate();if(t.isTextOverflowing||t.text.Contains("No translation"))findings.Add(locale+"-"+name+" / "+t.name+" / "+t.text);
                }
                foreach(var image in g.GetComponentsInChildren<UnityEngine.UI.Image>())if(image.name=="Parcel"||image.name=="BeaconModel")
                    if(image.sprite==null)findings.Add(locale+"-"+name+" missing sprite "+image.name);
            });
        }
    }
}
