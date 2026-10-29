using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class PersonalMissionFlowTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;string directory;bool oldBackground;Camera camera;RenderTexture portrait,oldTarget;
        T Read<T>(string name)=>(T)typeof(CascadeGame).GetField(name,Hidden).GetValue(game);
        [UnitySetUp] public IEnumerator SetUp(){
            oldBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadePersonalMissionFlow",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Configure();
            Read<HashSet<int>>("heardStory").UnionWith(new[]{1000}.Concat(Enumerable.Range(0,JourneyStory.PageCount))
                .Concat(Enumerable.Range(2000,JourneyStory.WorldCount*10)).Concat(Enumerable.Range(4000,JourneyStory.TotalLevels))
                .Concat(Enumerable.Range(6000,JourneyStory.WorldCount)).Concat(Enumerable.Range(7000,JourneyStory.TotalLevels))
                .Concat(Enumerable.Range(8000,JourneyStory.WorldCount*10)));
            Read<PlayerProfile>("profile").SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.SetUiLocale("en");
        }
        void Configure(){
            camera=Camera.main;oldTarget=camera.targetTexture;if(portrait==null)portrait=new RenderTexture(390,844,24);camera.targetTexture=portrait;game.RefreshLayout();
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Preferences.Tutorials=false;game.Preferences.ReducedMotion=true;game.ReducedMotion=true;
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
        }
        [UnityTearDown] public IEnumerator TearDown(){
            if(camera!=null)camera.targetTexture=oldTarget;if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}
            if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=oldBackground;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Ready(Func<bool> condition,string step){
            float end=Time.realtimeSinceStartup+20;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;
            var memory=Read<PlayerProfile>("profile").Missions;
            Assert.That(condition(),Is.True,$"Mission flow: {step}; screen={game.CurrentScreen}, offered={game.PersonalMissionAvailable}, active={memory.Active}, due={memory.NextAfterClears}, clears={game.JourneyClearCount}, planning={Read<System.Threading.Tasks.Task<DescentRun>>("missionPlan")!=null}, control={game.Interaction.CanControl}, play={game.Interaction.CanPlay}, saveError={game.HasSaveError}");
        }
        void Clear(string node){
            var content=game.Catalog.GetPrimary(node);var profile=Read<PlayerProfile>("profile");
            profile.Clears.Add(new ClearReceipt{CampaignNodeId=node,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=content.Definition.ContentHash,BestStars=3,BestScore=1600});
            Read<HashSet<string>>("sessionClears").Add(node);
            typeof(CascadeGame).GetMethod("InitializeRewards",Hidden).Invoke(game,null);
        }
        static PuzzleCommand Parse(string c)=>c=="D"?PuzzleCommand.Drop:c.StartsWith("R")?PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c));
        [UnityTest] public IEnumerator MapOfferDeclineAcceptanceRestartAndThreeStagesPreserveCampaign()
        {
            Clear("C01");Clear("C02");Clear("C03");game.ShowJourney();
            yield return Ready(()=>game.ConversationOpen&&game.PersonalMissionAvailable,"offer");
            Assert.That(game.ConversationReceipt,Is.EqualTo(-1));
            var captions=game.GetComponentsInChildren<TextMeshProUGUI>().Select(t=>t.text).ToArray();
            Assert.That(captions,Does.Contain("Accept"));Assert.That(captions,Does.Contain("Decline"));
            yield return Shot("offer-en.png",390,844);
            var memory=Read<PlayerProfile>("profile").Missions;game.DeclinePersonalMission();
            Assert.That(memory.Offer,Is.Null);Assert.That(memory.NextAfterClears,Is.InRange(6,8));
            game.SetUiLocale("uk");game.Preferences.LargeText=true;yield return new WaitForSecondsRealtime(.35f);game.ShowJourney();
            yield return Shot("map-uk.png",320,693);
            Assert.That(game.GetComponentsInChildren<Button>().Single(b=>b.name=="MapmodeMixed").GetComponentsInChildren<TextMeshProUGUI>().Select(t=>t.text),Does.Contain("Мікс"));
            game.ShowJournal();yield return null;
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.text=="Просьбы друзей"||t.text=="Прохання друзів"),Is.False);
            game.ShowJourney();game.Preferences.LargeText=false;game.SetUiLocale("en");
            for(int n=4;n<=7;n++)Clear("C"+n.ToString("00"));
            yield return Ready(()=>game.ConversationOpen&&game.PersonalMissionAvailable,"offer");
            string campaignHash=game.Interaction.Attempt.CurrentStateHash;
            int clears=Read<PlayerProfile>("profile").Clears.Count;var rewardNodes=game.Rewards.SeenClearNodes.ToArray();
            game.AcceptPersonalMission();yield return Ready(()=>game.PersonalMissionActive&&game.Interaction.CanPlay,"accept");
            var saved=new ProfileStore(game.ProfilePath).Load();Assert.That(saved.Recovery,Is.EqualTo(SaveRecovery.None));
            Assert.That(saved.Profile.Missions.Active,Is.True);
            game.Interaction.Execute(Parse(game.Descent.Stages[0].Witness[0]));yield return Ready(()=>game.Interaction.CanControl,"command");
            string missionHash=game.Interaction.Attempt.CurrentStateHash;
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Configure();game.ShowGame();
            yield return Ready(()=>game.PersonalMissionActive&&game.Interaction.CanControl,"restart");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(missionHash));
            game.ShowJourney();yield return null;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Travel").onClick.Invoke();
            yield return Ready(()=>game.CurrentScreen==GameScreen.Playing&&game.Interaction.CanControl,"resume");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(missionHash));
            for(int stage=0;stage<3;stage++){
                int used=game.Interaction.Attempt.RouteLength;
                foreach(var command in game.Descent.Stages[stage].Witness.Skip(used).Select(Parse)){
                    Assert.That(game.Interaction.Execute(command).Accepted,Is.True);yield return Ready(()=>game.Interaction.CanControl,"command");
                }
                if(stage<2){game.GoLower();yield return Ready(()=>game.Descent.StageIndex==stage+1&&game.Interaction.CanPlay,"next stage");}
            }
            yield return Ready(()=>game.CurrentScreen==GameScreen.Win,"complete");
            Assert.That(Read<PlayerProfile>("profile").Missions.Completed,Is.EqualTo(1));
            Assert.That(Read<PlayerProfile>("profile").Clears.Count,Is.EqualTo(clears));Assert.That(game.Rewards.SeenClearNodes,Is.EquivalentTo(rewardNodes));
            Assert.That(game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.name=="Earned coins"),Is.False);
            game.ContinueToMap();yield return null;
            Assert.That(game.PersonalMissionActive,Is.False);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(campaignHash));
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
        }
        IEnumerator Shot(string name,int width,int height)
        {
            var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();var old=camera.targetTexture;var active=RenderTexture.active;
            var target=new RenderTexture(width,height,24);var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{
                scaler.enabled=false;camera.targetTexture=target;scaler.enabled=true;game.RefreshLayout();yield return null;game.RefreshLayout();yield return null;
                foreach(var text in game.GetComponentsInChildren<TextMeshProUGUI>()){text.ForceMeshUpdate();if(text.name=="Conversation line")Assert.That(text.isTextOverflowing,Is.False);}
                camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();
                string reports=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/PersonalMissions-Oct09"));Directory.CreateDirectory(reports);File.WriteAllBytes(Path.Combine(reports,name),pixels.EncodeToPNG());
            }finally{
                scaler.enabled=false;camera.targetTexture=old;scaler.enabled=true;RenderTexture.active=active;
                UnityEngine.Object.Destroy(pixels);target.Release();UnityEngine.Object.Destroy(target);game.RefreshLayout();
            }
        }
    }
}
