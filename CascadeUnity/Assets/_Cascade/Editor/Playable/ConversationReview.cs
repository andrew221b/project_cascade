using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public static class ConversationReview
    {
        static string Folder=>Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/JourneyConversations");
        static void Check(bool pass,string why){if(!pass)throw new InvalidOperationException("Conversation review: "+why);Debug.Log("CONVERSATION PASS: "+why);}
        static CascadeGame Game()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated profile required.");
            UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.Preferences.LargeText=true;g.SetUiLocale("ru");return g;
        }
        public static void Checkpoint(){var g=Game();g.StartCoroutine(InspectCheckpoint(g));}
        static IEnumerator Finish(CascadeGame g){for(int i=0;i<10&&g.ConversationOpen;i++){g.AdvanceConversation();yield return null;}Check(!g.ConversationOpen,"explicit Next closes scene");}
        static IEnumerator Wait(float seconds){float elapsed=0;while(elapsed<seconds){elapsed+=Mathf.Min(.1f,Time.unscaledDeltaTime);yield return null;}}
        static IEnumerator InspectCheckpoint(CascadeGame g)
        {
            g.ShowJourney();yield return null;var hash=g.Interaction.Attempt.CurrentStateHash;double? time=g.Interaction.Attempt.ActiveSeconds;
            g.GetComponentsInChildren<Button>().Last(b=>b.name=="Story").onClick.Invoke();Check(g.ConversationOpen&&g.ConversationReceipt==0,"map Talk opens Bram plus Mika");
            Check(g.Interaction.IsPaused&&!g.CanSwitchJourneyMode,"story pauses input and mode switching");
            Check(!new ProfileStore(g.ProfilePath).Load().Profile.HeardStory.Contains(0),"unread scene not marked by arrival");
            yield return Wait(.35f);Check(g.Interaction.Attempt.ActiveSeconds==time&&g.Interaction.Attempt.CurrentStateHash==hash,"speech animation excludes clock and keeps exact puzzle");
            g.AdvanceConversation();yield return PlayableCapture.CaptureSettled("JourneyConversations","visit-ru.png",390,844);yield return PlayableCapture.CaptureSettled("JourneyConversations","visit-small.png",320,693);
            yield return Finish(g);Check(g.Descent.StageIndex==1&&g.CurrentScreen==GameScreen.Playing&&g.Interaction.Attempt.CurrentStateHash==hash,"Talk final action resumes exact saved stage");
            g.ShowJourney();yield return null;g.GetComponentsInChildren<Button>().Last(b=>b.name=="Story").onClick.Invoke();
            g.AdvanceConversation();yield return null;var body=g.GetComponentsInChildren<TextMeshProUGUI>().Last(t=>t.name=="Conversation line");
            Check(body.text.Contains("Зажги первый мост")&&!body.text.Contains("Я Брам"),"repeat Talk gives contextual next goal rather than greeting");
            g.SkipConversation();Check(g.CurrentScreen==GameScreen.JourneyMap&&!g.ConversationOpen,"Not now returns to map");
            g.GetComponentsInChildren<Button>().Last(b=>b.name=="Travel").onClick.Invoke();yield return null;
            var route=g.Descent.Stages[1].Witness.ToArray();g.ReducedMotion=true;
            foreach(var c in route){
                int frames=0;while(!g.Interaction.CanPlay&&frames++<1000){if(g.MechanicPopupOpen)g.AcknowledgeMechanic();yield return null;}Check(g.Interaction.CanPlay,"checkpoint witness input ready");
                var command=c=="D"?PuzzleCommand.Drop:c.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c));
                Check(g.Interaction.Execute(command)?.Accepted==true,"checkpoint witness committed");while(g.Interaction.IsPresenting)yield return null;
            }
            yield return null;g.ReducedMotion=false;Check(g.CurrentScreen==GameScreen.Checkpoint,"real second stage completed");g.GoLower();if(g.MechanicPopupOpen){g.AcknowledgeMechanic();yield return null;g.GoLower();}
            Check(g.ConversationOpen&&g.ConversationReceipt==4001&&g.Descent.StageIndex==1,"C02 banter precedes third stage");
            hash=g.Interaction.Attempt.CurrentStateHash;g.AdvanceConversation();g.AdvanceConversation();g.AdvanceConversation();yield return Wait(.32f);
            Check(g.Interaction.Attempt.CurrentStateHash==hash&&g.Descent.StageIndex==1,"typing and slide cannot advance checkpoint");
            yield return PlayableCapture.CaptureSettled("JourneyConversations","checkpoint-mika.png",390,844);
            g.SkipConversation();Check(!g.ConversationOpen&&g.Descent.StageIndex==2,"skip reads scene then explicit continuation descends");
            var p=new ProfileStore(g.ProfilePath).Load().Profile;Check(p.HeardStory.Contains(4001)&&p.HeardStory.Contains(0),"conversation receipts saved to disk");
            File.WriteAllText(Path.Combine(Folder,"checkpoint-runtime.txt"),"PASS: meaningful map Talk, exact saved stage resume, paused input/clock, real second-stage witness, C02 automatic conversation before third stage, no tween-driven checkpoint change, explicit skip descent and disk receipts. Native 390/320 with Large Text and simulated safe insets.\n");
            Debug.Log("CASCADE CONVERSATION CHECKPOINT COMPLETE");
        }
        public static void FreshModes(){var g=Game();g.StartCoroutine(InspectFreshModes(g));}
        static IEnumerator InspectFreshModes(CascadeGame g)
        {
            g.ShowJourney();string original=g.Interaction.Attempt.CurrentStateHash;int stage=g.Descent.StageIndex;
            foreach(string mode in new[]{"Words","Mixed"}){
                g.GetComponentsInChildren<Button>().Last(b=>b.name=="Mapmode"+mode).onClick.Invoke();yield return null;
                Check(g.Descent==null,"fresh "+mode+" has no started descent");
                g.GetComponentsInChildren<Button>().Last(b=>b.name=="Travel").onClick.Invoke();yield return null;
                Check(g.ConversationOpen,"fresh "+mode+" has resident introduction");g.SkipConversation();
                int frames=0;while(g.IsPlanningDescent&&frames++<1000)yield return null;
                Check(g.Descent!=null&&g.Descent.StageIndex==0&&g.Descent.Stages.Length==3,"fresh "+mode+" starts a three-stage level");
                while(g.IsDescending)yield return null;g.ShowJourney();yield return null;
            }
            g.GetComponentsInChildren<Button>().Last(b=>b.name=="MapmodeShapes").onClick.Invoke();yield return null;
            Check(g.Descent.StageIndex==stage&&g.Interaction.Attempt.CurrentStateHash==original,"mode round trip preserves exact Shapes checkpoint");
            File.WriteAllText(Path.Combine(Folder,"fresh-modes-runtime.txt"),"PASS: fresh Words and Mixed introduction -> explicit skip -> three-stage descent at stage0; Shapes exact stage/state restored. Existing fresh-attempt/resume distinction retained. No phone build.\n");
            Debug.Log("CASCADE CONVERSATION FRESH MODES COMPLETE");
        }
        public static void Island(){var g=Game();g.StartCoroutine(InspectIsland(g));}
        static IEnumerator InspectIsland(CascadeGame g)
        {
            g.ShowGame();yield return null;Check(!g.ConversationOpen&&g.CurrentScreen==GameScreen.Win,"stats precede island scene");
            g.ContinueCompletion();yield return null;Check(g.CurrentScreen==GameScreen.Parcel&&!g.ConversationOpen,"trophy precedes island scene");
            g.GetComponentsInChildren<Button>().Last(b=>b.name=="Unwrapgift").onClick.Invoke();yield return null;Check(!g.ConversationOpen,"trophy reveal has no competing story");
            g.GetComponentsInChildren<Button>().Last(b=>b.name=="Keepgift").onClick.Invoke();yield return null;
            Check(g.CurrentScreen==GameScreen.JourneyMap&&g.ConversationReceipt==2000,"first island conversation follows stats and trophy");
            g.AdvanceConversation();yield return PlayableCapture.CaptureSettled("JourneyConversations","island-ru.png",390,844);yield return Finish(g);
            var p=new ProfileStore(g.ProfilePath).Load().Profile;Check(p.HeardStory.Contains(2000),"island scene receipt saved");g.ShowJourney();Check(!g.ConversationOpen,"map revisit never repeats read automatic island");
            // Inspect every authored translation in the actual TMP ribbon at narrow portrait size.
            int lines=0;g.ReducedMotion=true;
            var camera=Camera.main;var previous=camera.targetTexture;var canvas=g.GetComponentInChildren<UnityEngine.UI.CanvasScaler>();
            var narrow=new RenderTexture(320,693,24);narrow.Create();canvas.enabled=false;camera.targetTexture=narrow;canvas.enabled=true;g.RefreshLayout();
            try {
            foreach(string locale in new[]{"en","ru","uk"}){
                g.SetUiLocale(locale);yield return null;
                var scenes=Enumerable.Range(0,8).Select(w=>StoryConversations.Visit(JourneyStory.WorldFirst(w))).Concat(Enumerable.Range(0,8).SelectMany(w=>Enumerable.Range(0,5).Select(i=>StoryConversations.Island(w,i)))).Concat(Enumerable.Range(0,8).SelectMany(w=>new[]{StoryConversations.Checkpoint(JourneyStory.WorldFirst(w)+1),StoryConversations.Checkpoint(JourneyStory.IslandFirst(w,1))})).ToArray();
                foreach(var scene in scenes){
                    g.ReviewConversation(scene);yield return null;g.RefreshLayout();
                    foreach(var line in scene.Lines){
                        var text=g.GetComponentsInChildren<TextMeshProUGUI>().Last(t=>t.name=="Conversation line");text.ForceMeshUpdate();
                        if(text.text.Contains("No translation found")||text.isTextOverflowing)throw new InvalidOperationException("Localized ribbon does not fit: "+locale+" / "+line.Speaker+" / "+text.text);
                        lines++;
                        if(g.ConversationLine+1<scene.Lines.Length){g.AdvanceConversation();yield return null;}
                    }
                    g.ShowJourney(); // abort inspection; no story receipt is marked by drawing.
                }
            }
            } finally {canvas.enabled=false;camera.targetTexture=previous;canvas.enabled=true;g.RefreshLayout();narrow.Release();UnityEngine.Object.Destroy(narrow);}
            g.SetUiLocale("uk");g.ReviewConversation(StoryConversations.Visit(112));yield return null;
            yield return PlayableCapture.CaptureSettled("JourneyConversations","lumi-uk-small.png",320,693);
            g.SetUiLocale("en");g.ReviewConversation(StoryConversations.Island(4,0));yield return null;
            yield return PlayableCapture.CaptureSettled("JourneyConversations","neri-wide.png",1440,900);g.ShowJourney();g.ReducedMotion=false;
            File.WriteAllText(Path.Combine(Folder,"island-runtime.txt"),"PASS: stats > trophy > island conversation > map; first-clear receipt, disk save and no replay on map revisit. "+lines+" authored localized lines inspected in native TMP. Native portrait/small/wide screenshots; Reduced Motion immediate copy. No physical phone test or build.\n");Debug.Log("CASCADE CONVERSATION ISLAND COMPLETE");
        }
    }
}
