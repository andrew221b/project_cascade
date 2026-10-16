using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Content;
using Cascade.Persistence;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Cascade.Editor.Playable
{
    public static class MapReferenceBaseline
    {
        const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        public static void Start(){var host=new GameObject("Isolated map baseline");UnityEngine.Object.DontDestroyOnLoad(host);host.AddComponent<ReferenceShopLayout>().StartCoroutine(Run(host));}
        public static void Configure(CascadeGame game,int clears=18)
        {
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;
            game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;
            var p=(PlayerProfile)typeof(CascadeGame).GetField("profile",Flags).GetValue(game);
            p.Missions.NextAfterClears=20000;var stories=(HashSet<int>)typeof(CascadeGame).GetField("heardStory",Flags).GetValue(game);
            stories.UnionWith(Enumerable.Range(0,10000));p.HeardStory=stories.ToList();
            p.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            var session=(HashSet<string>)typeof(CascadeGame).GetField("sessionClears",Flags).GetValue(game);
            int index=0;foreach(var content in game.Catalog.PrimaryLevels.Take(clears)){
                var r=new ClearReceipt{CampaignNodeId=content.Definition.CampaignNodeId,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=content.Definition.ContentHash,BestStars=index<12?2:index<17?3:2,BestHalfStar=index<8,BestScore=1800};p.Clears.Add(r);session.Add(r.CampaignNodeId);index++;
            }
            typeof(CascadeGame).GetMethod("InitializeRewards",Flags).Invoke(game,null);
        }
        static IEnumerator Run(GameObject host)
        {
            var dir=Path.Combine(Path.GetTempPath(),"CascadeMapBaseline",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=dir;UnityEngine.Application.runInBackground=true;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Configure(game);game.SetUiLocale("ru");game.ShowJourney();
            yield return new WaitForSecondsRealtime(.7f);yield return PlayableCapture.CaptureSettled("MapReferenceOct14","baseline.png",720,1280);
            CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");UnityEngine.Object.Destroy(host);if(Directory.Exists(dir))Directory.Delete(dir,true);EditorApplication.isPlaying=false;
        }
    }
}
