using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cascade.Content;
using Cascade.Persistence;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class StoryVisualReview
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        const string Folder="StoryComic-Oct07";
        [MenuItem("CASCADE/QA/Capture story and comic frames")]
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeWorldAtmosphereReview"))
                throw new InvalidOperationException("Prepare the isolated review profile, then start Play Mode.");
            game.StartCoroutine(Review(game));
        }
        static void Invoke(CascadeGame game,string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Private).Invoke(game,args);
        static void Write(CascadeGame game,string name,object value)=>typeof(CascadeGame).GetField(name,Private).SetValue(game,value);
        static IEnumerator Review(CascadeGame game)
        {
            Write(game,"persistenceReady",false);
            var heard=(HashSet<int>)typeof(CascadeGame).GetField("heardStory",Private).GetValue(game);
            for(int world=0;world<JourneyStory.WorldCount;world++)heard.Add(WorldPrologues.ForWorld(world).Receipt);
            heard.Add(1000);heard.Add(StoryConversations.IslandOpening(0,0).Receipt);
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.Preferences.LargeText=false;game.ReducedMotion=true;game.SetUiLocale("en");
            int frames=0;
            for(int world=0;world<JourneyStory.WorldCount;world++){
                game.OpenWorldComic(world,game.ShowHome);
                for(int page=0;page<WorldPrologues.ForWorld(world).Pages.Length;page++){
                    Write(game,"worldComicPage",page);Write(game,"worldComicLine",0);Invoke(game,"DrawMenu");
                    yield return PlayableCapture.CaptureSettled(Folder,$"comic-{world}-page-{page+1}.png",390,844);frames++;
                }
            }
            foreach(string locale in new[]{"ru","uk"}){
                game.SetUiLocale(locale);game.Preferences.LargeText=true;game.OpenWorldComic(5,game.ShowHome);
                Write(game,"worldComicPage",3);Write(game,"worldComicLine",1);Invoke(game,"DrawMenu");
                yield return PlayableCapture.CaptureSettled(Folder,"comic-small-"+locale+".png",320,693);frames++;
            }
            game.FinishWorldComic();game.Preferences.LargeText=false;game.SetUiLocale("ru");game.ShowJourney();
            for(int world=0;world<JourneyStory.WorldCount;world++){
                Write(game,"mapPage",world);Write(game,"selectedJourneyStop",JourneyStory.WorldFirst(world));
                Invoke(game,"SetScreen",GameScreen.JourneyMap);
                var authored=StoryConversations.IslandOpening(world,0);
                var scene=new StoryConversation(-1,authored.Lines);
                Invoke(game,"OpenConversation",scene,(UiCopy)"Back to the map",(Action)(()=>{}),null);
                yield return PlayableCapture.CaptureSettled(Folder,"dialogue-"+StoryConversations.Resident(world)+".png",390,844);frames++;
                game.SkipConversation();
            }
            game.SetUiLocale("uk");game.Preferences.LargeText=true;
            var next=StoryConversations.Visit(JourneyStory.WorldFirst(8),-1);
            Invoke(game,"OpenConversation",next,(UiCopy)"Back to the map",(Action)(()=>{}),null);game.AdvanceConversation();game.AdvanceConversation();
            yield return PlayableCapture.CaptureSettled(Folder,"dialogue-small-uk.png",320,693);frames++;game.SkipConversation();
            game.Preferences.LargeText=false;game.SetUiLocale("en");game.ReducedMotion=false;game.ShowHome();
            string path=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Folder));
            File.WriteAllText(Path.Combine(path,"capture-complete.txt"),frames+" isolated native Unity captures. Puzzle hash checked per frame; no player save used.");
            Debug.Log("CASCADE story comic capture complete: "+frames+" frames.");
        }
    }
}
