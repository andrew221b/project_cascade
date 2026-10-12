using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cascade.Content;
using Cascade.Presentation;
using Cascade.Persistence;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // Bounded review of the supplied screenshot defects, using a disposable profile.
    public static class LocalizedJournalReview
    {
        const string Phase="LocalizedJournal-Oct08";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        public static bool Running {get;private set;}
        static void Set(CascadeGame g,string name,object value)=>typeof(CascadeGame).GetField(name,Private).SetValue(g,value);
        static void Call(CascadeGame g,string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Private).Invoke(g,args);
        static void Check(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
        static RectTransform Rect(CascadeGame g,string name)=>g.GetComponentsInChildren<RectTransform>().Single(r=>r.name==name&&r.gameObject.activeInHierarchy);
        static TextMeshProUGUI Text(CascadeGame g,string name)=>g.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name==name&&t.gameObject.activeInHierarchy);
        static Button Button(CascadeGame g,string name)=>g.GetComponentsInChildren<Button>().Single(b=>b.name.Replace(" ","")==name.Replace(" ","")&&b.gameObject.activeInHierarchy);
        static string PathFor(string file)=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase+"/"+file));
        static readonly StringBuilder log=new StringBuilder();
        public static void Capture(bool finalOnly=false,bool recordsOnly=false)
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Check(EditorApplication.isPlaying&&!Running&&g!=null&&g.ProfilePath.Contains("CascadeNavigationReview"),"Temporary native profile required.");
            log.Clear();Running=true;g.StartCoroutine(Guard(Run(g,finalOnly,recordsOnly)));
        }
        static IEnumerator Run(CascadeGame g,bool finalOnly,bool recordsOnly)
        {
            UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=30;
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;g.ReducedMotion=true;
            Set(g,"persistenceReady",false);g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(g);
            g.Rewards.RecordDelivery(g.Interaction.Attempt,g.Descent?.Performance(g.Interaction.Attempt),profile.ModeRecords);
            string puzzle=g.Interaction.Attempt.CurrentStateHash;int coins=g.ShopCoins;
            if(recordsOnly){
                g.SetUiLocale("ru");g.Preferences.LargeText=false;g.ShowRecords();
                yield return Shot("13-records-inline-ru.png",390,844,Records);
                g.SetUiLocale("uk");g.Preferences.LargeText=true;g.ShowRecords();
                yield return Shot("14-records-inline-uk.png",320,693,Records);
                Check(g.Interaction.Attempt.CurrentStateHash==puzzle&&g.ShopCoins==coins,"Record layout must preserve puzzle and wallet.");
                log.AppendLine("PASS compact inline route metrics in RU and narrow UK Large Text; no paper fact cards; puzzle and wallet unchanged.");
                yield break;
            }
            if(!finalOnly){
            g.SetUiLocale("ru");g.Preferences.LargeText=false;
            g.ShowHome();g.ShowParcel();yield return Shot("01-unopened.png",390,844,x=>{
                Check(!x.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.name=="Gift name"),"Pending parcel must hide prize name.");
                Check(Rect(x,"Menu").GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Label").text=="Получить","Pending parcel has one Collect action.");
            });
            g.ShowRequests();yield return Shot("02-requests-ru.png",390,844,Requests);
            g.ShowRecords();yield return Shot("03-records-ru.png",390,844,Records);
            g.ShowWorkshop();yield return Shot("04-workshop.png",390,844,x=>{
                Check(Text(x,"Workshop progress caption").text=="Готово!","Built model must show its actual localized Ready caption.");FitText(Text(x,"Workshop progress caption"));
            });
            }
            g.Preferences.LargeText=true;g.SetUiLocale("uk");
            if(!finalOnly){
            g.ShowSettings();Set(g,"settingsPage",0);Call(g,"DrawMenu");yield return Shot("05-settings-uk.png",320,693,Labels);
            g.ShowRequests();yield return Shot("06-requests-uk.png",320,693,Requests);
            g.ShowRecords();yield return Shot("07-records-uk.png",320,693,Records);
            }
            g.ShowContracts();yield return Shot("08-trails-uk.png",320,693,x=>{ClearFooter(x,"Contract freedom papyrus","Trythistrail");Labels(x);});
            g.ShowShop();g.SelectShopTab(1);g.SelectShopItem(19);yield return Shot("09-costume-uk.png",320,693,x=>{FitText(Text(x,"Selected object title"));FitText(Text(x,"Wallet coins"));Labels(x);});
            if(!finalOnly){
            Map();yield return Shot("10-map-tabs-uk.png",320,693,Labels);
            g.ReviewConversation(StoryConversations.IslandProgress(JourneyStory.IslandFirst(0,0)+5));
            yield return Shot("11-dialogue-uk.png",320,693,x=>{
                int pages=0;
                while(x.ConversationOpen){Canvas.ForceUpdateCanvases();FitText(Text(x,"Conversation line"));Labels(x);pages++;Check(pages<=60,"Conversation must terminate.");x.AdvanceConversation();}
                log.AppendLine("PASS all "+pages+" measured Ukrainian dialogue pages with Large Text; no overflowing body/action.");
                x.ReviewConversation(StoryConversations.IslandProgress(JourneyStory.IslandFirst(0,0)+5));Canvas.ForceUpdateCanvases();
            });
            }
            g.SkipConversation();g.OpenWorldComic(8,Map);
            yield return Shot("12-comic-uk.png",320,693,x=>{
                int pages=0;
                while(x.ComicWorld>=0){
                    Canvas.ForceUpdateCanvases();x.GetComponentInChildren<WorldComicLayout>().RefreshLayout();Canvas.ForceUpdateCanvases();
                    FitText(Text(x,"Comic title"));
                    var body=Text(x,"World comic spoken line");body.ForceMeshUpdate();var viewport=Rect(x,"World comic reading viewport");
                    float needed=body.GetPreferredValues(body.text,body.rectTransform.rect.width,Mathf.Infinity).y;
                    Check(needed<=viewport.rect.height+1,"Comic chunk must fit the actual reading viewport.");Labels(x);
                    pages++;Check(pages<=80,"Comic must terminate.");Button(x,"Next comic page").onClick.Invoke();
                }
                log.AppendLine("PASS all "+pages+" Ukrainian night-comic text chunks fit the reading viewport; navigation terminates.");
                x.OpenWorldComic(8,Map);Canvas.ForceUpdateCanvases();x.GetComponentInChildren<WorldComicLayout>().RefreshLayout();Canvas.ForceUpdateCanvases();
            });
            Check(g.Interaction.Attempt.CurrentStateHash==puzzle&&g.ShopCoins==coins,"UI review must preserve puzzle and wallet.");
            log.AppendLine("PASS puzzle hash and wallet unchanged. "+(finalOnly?"Final confirmation of three corrected screens; earlier 11 distinct captures retained.":"Completed 12 targeted native frames; no full-gallery rerun."));
            void Map(){g.SkipConversation();Set(g,"worldComic",-1);Set(g,"islandBrowseIndex",-1);Set(g,"mapPage",0);Set(g,"selectedJourneyStop",0);Call(g,"SetScreen",GameScreen.JourneyMap);}
        }
        static IEnumerator Shot(string file,int width,int height,Action<CascadeGame> check)
        {
            yield return null;yield return PlayableCapture.CaptureSettled(Phase,file,width,height,x=>{check(x);log.AppendLine("PASS "+file);});
        }
        static void FitText(TextMeshProUGUI t){t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Text overflow: "+t.name+" / "+t.text);}
        static void Labels(CascadeGame g){foreach(var label in Rect(g,"Menu").GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name=="Label"))FitText(label);if(g.ConversationOpen){FitText(Button(g,"Next line").GetComponentInChildren<TextMeshProUGUI>());FitText(Button(g,"Not now").GetComponentInChildren<TextMeshProUGUI>());}}
        static void ClearFooter(CascadeGame g,string paper,string action)
        {
            var p=Rect(g,paper);var b=(RectTransform)Button(g,action).transform;
            var corners=new Vector3[4];var parent=p.parent;p.GetWorldCorners(corners);float bottom=parent.InverseTransformPoint(corners[0]).y;b.GetWorldCorners(corners);
            float scale=g.GetComponentInChildren<Canvas>().scaleFactor;
            Check((bottom-parent.InverseTransformPoint(corners[1]).y)*scale>=15.5f,"Footer action needs a real 16-unit gap below "+paper);
            foreach(var t in p.GetComponentsInChildren<TextMeshProUGUI>())FitText(t);
        }
        static void Records(CascadeGame g)
        {
            Check(!g.GetComponentsInChildren<RectTransform>().Any(r=>r.name=="Best route papyrus"||r.name=="Clean route papyrus"),"Route metrics must have no paper cards.");
            Check(!g.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.name=="Route moves caption"||t.name=="Route stars caption"),"Metric captions must be absent.");
            var row=Rect(g,"Route summary");var action=(RectTransform)Button(g,"Playthislevel").transform;
            var corners=new Vector3[4];row.GetWorldCorners(corners);float bottom=row.parent.InverseTransformPoint(corners[0]).y;
            action.GetWorldCorners(corners);float gap=(bottom-row.parent.InverseTransformPoint(corners[1]).y)*g.GetComponentInChildren<Canvas>().scaleFactor;
            Check(gap>=15.5f,"Metric row needs a 16-unit footer gap.");
            var moves=Text(g,"Route moves value");var stars=Text(g,"Route stars value");
            Check(moves.text.StartsWith("× ")&&stars.text.StartsWith("× "),"Both metrics need icon × count.");
            Check(stars.text!="× 13","A clean route's move count must never masquerade as earned stars.");
            var ribbon=Rect(g,"Menu description papyrus");
            foreach(var arrowName in new[]{"Previousrecord","Nextrecord"}){
                var arrow=(RectTransform)Button(g,arrowName).transform;
                Check(Mathf.Abs(arrow.position.y-ribbon.position.y)<12,"Record arrow must align with the level ribbon.");
                Check(arrow.Find(arrowName.Replace("record", " record")+" outline")!=null,"Record arrow needs a contrasting outline.");
                var arrowCorners=new Vector3[4];var paperCorners=new Vector3[4];
                arrow.GetWorldCorners(arrowCorners);ribbon.GetWorldCorners(paperCorners);
                Check(arrowCorners[0].x>=paperCorners[0].x+.02f&&arrowCorners[2].x<=paperCorners[2].x-.02f,
                    "Record arrow must stay completely inside the level ribbon.");
            }
            foreach(var t in row.GetComponentsInChildren<TextMeshProUGUI>())FitText(t);Labels(g);
        }
        static void Requests(CascadeGame g)
        {
            Check(!g.GetComponentsInChildren<RectTransform>().Any(r=>r.name=="Request speaker papyrus"),"Name must have no papyrus.");
            var name=Rect(g,"Request speaker");var hero=Rect(g,"Request friend");var corners=new Vector3[4];name.GetWorldCorners(corners);float bottom=corners[0].y;hero.GetWorldCorners(corners);
            Check(bottom>corners[1].y,"Name belongs above the character.");
            FitText(Text(g,"Request speaker"));FitText(Text(g,"Request detail"));Labels(g);ClearFooter(g,"Request detail papyrus","Tryapuzzle");
        }
        static IEnumerator Guard(IEnumerator routine)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathFor("validation.txt")));var stack=new Stack<IEnumerator>();stack.Push(routine);bool passed=true;
            try{
                while(stack.Count>0){object next=null;bool moved=false;Exception error=null;
                    try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){error=e;}
                    if(error!=null){passed=false;log.AppendLine("FAIL "+error.GetBaseException());while(stack.Count>0)(stack.Pop() as IDisposable)?.Dispose();yield break;}
                    if(!moved){(stack.Pop() as IDisposable)?.Dispose();continue;}
                    if(next is IEnumerator nested)stack.Push(nested);else yield return next;
                }
            }
            finally{Running=false;UnityEngine.Application.runInBackground=false;log.AppendLine("Completed: "+passed);File.WriteAllText(PathFor("validation.txt"),log.ToString());}
        }
    }
}
