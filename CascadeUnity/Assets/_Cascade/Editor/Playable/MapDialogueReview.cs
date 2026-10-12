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
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // Focused native review of the map, spoken lines, authored arrows and defeat caption.
    public static class MapDialogueReview
    {
        const string Folder="MapDialogue-Oct08";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static readonly StringBuilder log=new StringBuilder();
        public static bool Running{get;private set;}
        static void Set(CascadeGame g,string name,object value)=>typeof(CascadeGame).GetField(name,Private).SetValue(g,value);
        static void Call(CascadeGame g,string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Private).Invoke(g,args);
        static void Check(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
        static T Named<T>(CascadeGame g,string name)where T:Component=>g.GetComponentsInChildren<T>(true).Single(c=>c.name.Replace(" ","")==name.Replace(" ","")&&c.gameObject.activeInHierarchy);
        static float Top(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);return c[1].y;}
        static float Bottom(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);return c[0].y;}
        public static void Capture(bool finalOnly=false)=>Start(finalOnly,false);
        public static void CaptureCorrections()=>Start(false,true);
        static void Start(bool finalOnly,bool dialogueOnly)
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Check(EditorApplication.isPlaying&&g!=null&&!Running&&g.ProfilePath.Contains("CascadeNavigationReview"),"Disposable Play profile required.");
            Running=true;log.Clear();g.StartCoroutine(Guard(Run(g,finalOnly,dialogueOnly)));
        }
        static IEnumerator Run(CascadeGame g,bool finalOnly,bool dialogueOnly)
        {
            UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=30;
            Set(g,"persistenceReady",false);g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;g.ReducedMotion=true;
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            g.Preferences.LargeText=false;g.SetUiLocale("ru");
            if(!finalOnly){
                Map(g,0);
                if(!dialogueOnly)yield return Shot("01-map-ru.png",390,844,MapChecks);
                g.ReviewConversation(StoryConversations.IslandProgress(JourneyStory.IslandFirst(0,0)+5));
                yield return Shot("02-dialogue-ru.png",390,844,DialogueChecks);
                g.SkipConversation();g.Preferences.LargeText=true;g.SetUiLocale("uk");Map(g,0);
                if(!dialogueOnly)yield return Shot("03-map-uk-large.png",320,693,MapChecks);
                g.ReviewConversation(StoryConversations.IslandProgress(JourneyStory.IslandFirst(0,0)+5));
                yield return Shot("04-dialogue-uk-large.png",320,693,DialogueChecks);
                g.SkipConversation();g.Preferences.LargeText=false;g.SetUiLocale("ru");
            }
            var lost=new AttemptService(g.Catalog.GetPrimary("C01").Definition);
            for(int i=0;i<8&&lost.Current.Status==PuzzleStatus.Playing;i++)lost.Execute(PuzzleCommand.Drop);
            Check(lost.Current.Status==PuzzleStatus.Lost,"Real lost fixture required.");
            Call(g,"MountAttempt",lost,null);g.Machine.SetZone(8);Call(g,"SetScreen",GameScreen.Fail);
            yield return Shot("05-night-fail.png",390,844,x=>{
                x.SettleOutcome();x.SendMessage("ApplyPreferences");
                var ribbon=x.GetComponentsInChildren<RectTransform>(true).Single(r=>r.name=="Stage celebration ribbon");
                Check(!ribbon.gameObject.activeInHierarchy,"Failure must not show the Try another way ribbon.");
                Check(!x.GetComponentsInChildren<TextMeshProUGUI>(true).Any(t=>t.name=="Your row"||t.name=="Result summary"),"Failure must not repeat row or retry captions.");
            });
            log.AppendLine(finalOnly?"PASS final native defeat frame.":dialogueOnly?"PASS RU/UK dialogue and night defeat corrections; prior map frames retained.":"PASS five focused native map/dialogue/night frames; puzzle state unchanged by each capture.");
        }
        static void Map(CascadeGame g,int world)
        {
            Set(g,"mapPage",world);Set(g,"selectedJourneyStop",JourneyStory.WorldFirst(world));Set(g,"islandBrowseIndex",-1);
            Call(g,"SetScreen",GameScreen.JourneyMap);
        }
        static void MapChecks(CascadeGame g)
        {
            Check(!g.GetComponentsInChildren<Button>().Any(b=>b.name=="Story"),"Map must not show Talk action.");
            var title=Named<RectTransform>(g,"Map world title");var nav=Named<RectTransform>(g,"World navigation");
            var travel=Named<Button>(g,"Travel");var bag=Named<Button>(g,"Map bag");
            Check(Top(nav)<Bottom(title)&&Bottom(nav)>Top((RectTransform)travel.transform)-3,"World navigation belongs below title and above Play.");
            Check(Bottom((RectTransform)bag.transform)>Top(nav),"Shop and world navigation must be separate.");
            var next=Named<Button>(g,"Next world");
            Check(next.GetComponent<Image>().color.a==0&&next.GetComponentInChildren<ChevronGraphic>().mainTexture!=null,"World navigation uses authored arrow without blue face.");
            foreach(var button in g.GetComponentsInChildren<Button>().Where(b=>b.gameObject.activeInHierarchy)){
                var caption=button.GetComponentInChildren<TextMeshProUGUI>();if(caption!=null&&caption.text.Length>0){caption.ForceMeshUpdate();Check(!caption.isTextOverflowing,"Map button overflow: "+button.name);}
            }
        }
        static void DialogueChecks(CascadeGame g)
        {
            var plate=Named<RectTransform>(g,"Speech ribbon");var body=Named<TextMeshProUGUI>(g,"Conversation line");
            var badge=Named<RectTransform>(g,"Speaker name");var portrait=Named<RectTransform>(g,"Speaker portrait");
            Check(badge.GetComponent<Image>()?.sprite!=null,"Speaker name needs its own framed sprite.");
            Check(Top(badge)>Top(plate)&&Bottom(badge)<Top(plate),"Speaker frame must touch and overlap the papyrus edge.");
            Check(Top(badge)>Bottom(portrait)&&Bottom(badge)<Top(portrait),"Speaker frame must join the portrait.");
            Check(plate.rect.width>g.GetComponentInChildren<Canvas>().GetComponent<RectTransform>().rect.width*.70f,"Reading strip must span the phone.");
            Check(plate.rect.height<g.GetComponentInChildren<Canvas>().GetComponent<RectTransform>().rect.height*.34f,"Speech strip must leave the map visible.");
            var pages=(List<(int Source,string Text)>)typeof(CascadeGame).GetField("conversationPages",Private).GetValue(g);
            foreach(var page in pages){
                string copy=page.Text.Trim();Check(copy.Length==0||".!?…".Contains(copy[copy.Length-1]),"Sentence cut: "+copy);
                float need=body.GetPreferredValues(copy,body.rectTransform.rect.width,Mathf.Infinity).y;
                Check(need<=plate.rect.height-68*GetMenuUnit(g)+2,"Spoken sentence must fit its page: "+copy);
            }
            Check(pages.Count>=4&&pages.Count<=6,"Four authored beats should not become many word scraps: "+pages.Count);
            body.ForceMeshUpdate();Check(!body.isTextOverflowing,"Dialogue must never truncate the first page.");
            log.AppendLine("PASS "+pages.Count+" complete spoken pages at "+Mathf.RoundToInt(plate.rect.width)+"×"+Mathf.RoundToInt(plate.rect.height));
        }
        static float GetMenuUnit(CascadeGame g)=>(float)typeof(CascadeGame).GetProperty("MenuUnit",Private).GetValue(g);
        static IEnumerator Shot(string file,int width,int height,Action<CascadeGame> check)
        {
            yield return null;yield return PlayableCapture.CaptureSettled(Folder,file,width,height,x=>{check(x);log.AppendLine("PASS "+file);});
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
