using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Content;
using Cascade.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class LevelTriadReview
    {
        const string Folder="LevelTriads-Oct08";
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Use an isolated review profile.");
            game.StartCoroutine(Review(game));
        }
        static IEnumerator Ready(Func<bool> ready)
        {
            float deadline=Time.realtimeSinceStartup+40;while(!ready()&&Time.realtimeSinceStartup<deadline)yield return null;
            if(!ready())throw new InvalidOperationException("Triad native review timed out.");
        }
        static IEnumerator Review(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;game.Preferences.Tutorials=false;
            game.Preferences.ReducedMotion=false;game.ReducedMotion=false;game.SetUiLocale("uk");
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Hidden).GetValue(game);
            profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            foreach(var lesson in profile.SeenMechanics)profile.SeenCoachPrompts.Add("triad-guide-v6-"+(int)lesson);
            var source=game.Catalog.GetPrimary("C02").Definition;game.StartCampaignLevel(source);game.ShowGame();
            yield return Ready(()=>game.Descent?.Version==6&&game.Interaction.CanPlay);
            game.Preferences.LargeText=true;game.Preferences.Tutorials=true;
            typeof(CascadeGame).GetMethod("OpenMechanicPopup",Hidden).Invoke(game,new object[]{MechanicLesson.Repair,null,default(UiCopy),null});
            yield return PlayableCapture.CaptureSettled(Folder,"repair-lesson-uk-small.png",320,693);
            typeof(CascadeGame).GetMethod("CloseMechanicPopup",Hidden).Invoke(game,null);
            game.Preferences.LargeText=false;game.Preferences.Tutorials=false;game.RefreshLayout();
            yield return Ready(()=>game.Interaction.CanPlay);
            yield return PlayableCapture.CaptureSettled(Folder,"repair-stage-1.png",390,844);
            var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();var previous=camera.targetTexture;var active=RenderTexture.active;
            var render=new RenderTexture(390,844,24);var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            string directory=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Folder));
            try{
                scaler.enabled=false;camera.targetTexture=render;scaler.enabled=true;game.RefreshLayout();yield return null;
                var first=CampaignTriadAudit.Parse(game.Descent.Stages[0].Witness[0]);game.Interaction.Execute(first);
                yield return Ready(()=>game.GetComponentInChildren<GameReactionBanner>()?.Elapsed>=.96f);
                Shot("repair-consequence.png");yield return Ready(()=>game.Interaction.CanControl);
                foreach(var command in game.Descent.Stages[0].Witness.Skip(1).Select(CampaignTriadAudit.Parse)){
                    game.Interaction.Execute(command);yield return Ready(()=>game.Interaction.CanControl);
                }
                var heard=(System.Collections.Generic.HashSet<int>)typeof(CascadeGame).GetField("heardStory",Hidden).GetValue(game);
                int stop=game.Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId=="C02");
                heard.Add(StoryConversations.Checkpoint(stop).Receipt);
                var clears=(System.Collections.Generic.HashSet<string>)typeof(CascadeGame).GetField("sessionClears",Hidden).GetValue(game);
                var offered=(System.Collections.Generic.IEnumerable<LevelContent>)typeof(CascadeGame).GetMethod("JourneySequence",Hidden).Invoke(game,null);
                clears.UnionWith(offered.TakeWhile(c=>c.Definition.CampaignNodeId!="C02").Select(c=>c.Definition.CampaignNodeId));
                game.GoLower();yield return Ready(()=>game.Descent.StageIndex==1&&game.Interaction.CanPlay);
                Shot("repair-stage-2.png");
                foreach(var command in game.Descent.Stages[1].Witness.Select(CampaignTriadAudit.Parse)){
                    game.Interaction.Execute(command);yield return Ready(()=>game.Interaction.CanControl);
                }
                game.GoLower();yield return Ready(()=>game.Descent.StageIndex==2&&game.Interaction.CanPlay);Shot("repair-stage-3.png");
                foreach(var command in game.Descent.Stages[2].Witness.Select(CampaignTriadAudit.Parse)){
                    game.Interaction.Execute(command);yield return Ready(()=>game.Interaction.CanControl);
                }
                yield return Ready(()=>game.CurrentScreen==GameScreen.Win&&game.GetComponentInChildren<RewardCelebration>()?.Progress>=1);Shot("completion-next-map.png");
                game.ContinueToMap();yield return null;
                if(game.CurrentScreen==GameScreen.Prologue)game.FinishWorldComic();
                for(int page=0;page<12&&game.ConversationOpen;page++){game.SkipConversation();yield return null;}
                yield return Ready(()=>game.CurrentScreen==GameScreen.JourneyMap);Shot("map-centred-stars.png");
            }finally{
                scaler.enabled=false;camera.targetTexture=previous;scaler.enabled=true;RenderTexture.active=active;
                UnityEngine.Object.Destroy(pixels);render.Release();UnityEngine.Object.Destroy(render);game.RefreshLayout();
            }
            game.Preferences.LargeText=true;yield return PlayableCapture.CaptureSettled(Folder,"map-uk-small.png",320,693);
            File.WriteAllText(Path.Combine(directory,"native-review.txt"),"PASS: native UK lesson, three repair stages, actual durability reaction, next/map result and centred map balance. Visual captures are not a physical-device performance measurement.\n");
            void Shot(string name){camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(directory,name),pixels.EncodeToPNG());}
        }
    }
}
