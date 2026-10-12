using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
    // Native surface/lifecycle regression review on disposable player data.
    public static class NightContrastReview
    {
        const string Folder="NightContrast-Oct08";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        public static bool Running{get;private set;}
        static readonly StringBuilder log=new StringBuilder();
        static void Set(CascadeGame g,string name,object value)=>typeof(CascadeGame).GetField(name,Private).SetValue(g,value);
        static object Call(CascadeGame g,string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Private).Invoke(g,args);
        static TextMeshProUGUI Label(CascadeGame g,string name)=>g.GetComponentsInChildren<TextMeshProUGUI>(true).Single(t=>t.name==name);
        static void Check(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
        public static void Capture()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Check(EditorApplication.isPlaying&&g!=null&&!Running&&g.ProfilePath.Contains("CascadeNavigationReview"),"Disposable review profile required.");
            log.Clear();Running=true;g.StartCoroutine(Guard(Run(g)));
        }
        static IEnumerator Run(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=30;
            Set(g,"persistenceReady",false);g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;g.ReducedMotion=true;
            g.Preferences.Music=g.Preferences.Sound=false;g.Preferences.ReducedMotion=true;
            g.Preferences.HighContrast=false;g.Preferences.LargeText=false;g.SetUiLocale("ru");
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            var p=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(g);
            p.Rewards.RecordDelivery(g.Interaction.Attempt,g.Descent?.Performance(g.Interaction.Attempt),p.ModeRecords);
            g.Machine.SetZone(8);Call(g,"SetScreen",GameScreen.Win);
            yield return new WaitForSecondsRealtime(1);
            yield return Shot("01-night-result.png",390,844,x=>{
                Apply(x);x.GetComponentInChildren<RewardCelebration>().SendMessage("Update");
                Bright(Label(x,"Completed stages"));Bright(Label(x,"Earned coins"));Dark(Label(x,"Completion title"));
            });
            g.LoadNode("C03");g.ShowGame();g.Machine.SetZone(8);g.RenderFrame(g.VisibleFrame);
            yield return Shot("02-night-game.png",390,844,x=>{Apply(x);Sky(x,true);});
            g.SetUiLocale("uk");g.Preferences.HighContrast=true;g.Preferences.LargeText=true;
            yield return Shot("03-night-settings-uk.png",320,693,x=>{
                Apply(x);x.RefreshLayout();Sky(x,true);Apply(x);Sky(x,true);
                Check(!Label(x,"ReceiverLabel").isTextOverflowing,"Target caption must fit the narrow Large Text row.");
                var undo=x.GetComponentsInChildren<Button>().Single(b=>b.name=="Undo");
                var count=undo.GetComponentInChildren<TextMeshProUGUI>();count.ForceMeshUpdate();
                Check(!count.isTextOverflowing&&count.text.StartsWith("× "),"Compact undo must fit without shrinking or wrapping.");
                Check(undo.GetComponentInChildren<Image>().sprite!=null&&undo.transform.Find("Undo meaning")!=null,"Existing rewind artwork required.");
            });
            g.SetUiLocale("ru");g.Preferences.HighContrast=false;g.Preferences.LargeText=false;
            var lost=new AttemptService(g.Catalog.GetPrimary("C01").Definition);
            for(int i=0;i<8&&lost.Current.Status==PuzzleStatus.Playing;i++)lost.Execute(PuzzleCommand.Drop);
            Check(lost.Current.Status==PuzzleStatus.Lost,"Real lost fixture required.");
            Call(g,"MountAttempt",lost,null);g.Machine.SetZone(8);Call(g,"SetScreen",GameScreen.Fail);
            yield return Shot("04-night-failure.png",390,844,x=>{
                x.SettleOutcome();Apply(x);Sky(x,true);
                Check(!x.GetComponentsInChildren<TextMeshProUGUI>(true).Any(t=>t.name=="Your row"||t.name=="Result summary"),"Redundant failure copy must be absent.");
                Check(!x.GetComponentsInChildren<RectTransform>(true).Single(r=>r.name=="Stage celebration ribbon").gameObject.activeInHierarchy,"Failure ribbon must be hidden.");
            });
            Set(g,"mapPage",8);Set(g,"selectedJourneyStop",JourneyStory.WorldFirst(8));Set(g,"islandBrowseIndex",-1);Call(g,"SetScreen",GameScreen.JourneyMap);
            g.ReviewConversation(StoryConversations.IslandProgress(JourneyStory.IslandFirst(8,0)+5));
            yield return Shot("05-night-map-dialogue.png",390,844,x=>{Apply(x);Bright(Label(x,"Region"));Dark(Label(x,"Conversation line"));Dark(Label(x,"Speaker"));});
            g.SkipConversation();g.LoadNode("C03");g.ShowGame();g.Machine.SetZone(0);g.RenderFrame(g.VisibleFrame);
            yield return Shot("06-day-return.png",390,844,x=>{Apply(x);Sky(x,false);});
            log.AppendLine("PASS six native contexts; repeated settings application, narrow Ukrainian Large Text/High Contrast, day return, night loss/result and paper dialogue.");
        }
        static void Apply(CascadeGame g)
        {
            g.SendMessage("ApplyPreferences");Canvas.ForceUpdateCanvases();
            var teal=((Sprite[])typeof(CascadeGame).GetField("enamelControls",Private).GetValue(g))[0];
            foreach(var b in g.GetComponentsInChildren<Button>()){
                var label=b.GetComponentInChildren<TextMeshProUGUI>();
                if(b.GetComponent<Image>()?.sprite==teal&&label!=null&&label.text.Length>0)
                    Check(label.color.r>.9f&&label.color.g>.9f&&label.color.b>.8f,"Teal button must use paper/cream ink: "+b.name+" / "+label.color);
            }
        }
        static void Bright(TextMeshProUGUI t)=>Check(t.color.r>.9f&&t.color.g>.9f&&t.color.b>.85f&&t.color.a>.99f,"Bare night copy must be light: "+t.name);
        static void Dark(TextMeshProUGUI t)=>Check(t.color.r<.3f&&t.color.g<.35f&&t.color.b<.35f,"Paper copy must keep dark ink: "+t.name);
        static void Sky(CascadeGame g,bool night)
        {
            var t=Label(g,"ReceiverLabel");t.ForceMeshUpdate();if(night)Bright(t);else Dark(t);
            Check(t.fontSharedMaterial.IsKeywordEnabled("UNDERLAY_ON")==night,"Sky caption shadow follows the visible world.");
        }
        static IEnumerator Shot(string file,int width,int height,Action<CascadeGame> verify)
        {
            yield return null;yield return PlayableCapture.CaptureSettled(Folder,file,width,height,x=>{
                string hash=x.Interaction.Attempt.CurrentStateHash;int coins=x.ShopCoins;
                verify(x);Check(hash==x.Interaction.Attempt.CurrentStateHash&&coins==x.ShopCoins,"Contrast review cannot alter puzzle or wallet.");
                log.AppendLine("PASS "+file);
            });
        }
        static IEnumerator Guard(IEnumerator routine)
        {
            var stack=new Stack<IEnumerator>();stack.Push(routine);bool passed=true;
            try{
                while(stack.Count>0){object next=null;bool moved=false;Exception error=null;
                    try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){error=e;}
                    if(error!=null){passed=false;log.AppendLine("FAIL "+error.GetBaseException());while(stack.Count>0)(stack.Pop() as IDisposable)?.Dispose();yield break;}
                    if(!moved){(stack.Pop() as IDisposable)?.Dispose();continue;}
                    if(next is IEnumerator nested)stack.Push(nested);else yield return next;
                }
            }
            finally{
                Running=false;UnityEngine.Application.runInBackground=false;log.AppendLine("Completed: "+passed);
                string path=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Folder+"/validation.txt"));
                Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,log.ToString());
            }
        }
    }
}
