using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cascade.Content;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public static class StoryMenuReviewOct11
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        const string Folder="StoryMenu-Oct11";
        static readonly StringBuilder log=new StringBuilder();
        static string PathFor(string file)=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Folder+"/"+file));
        public static void Prepare(){NavigationCapture.Prepare();LocalizationAssets.Build();}
        public static void Run()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated QA profile required.");
            log.Clear();Directory.CreateDirectory(Path.GetDirectoryName(PathFor("runtime.txt")));g.StartCoroutine(Guard(Inspect(g)));
        }
        static IEnumerator Guard(IEnumerator routine)
        {
            while(true){object next;try{if(!routine.MoveNext())break;next=routine.Current;}catch(Exception e){log.AppendLine("FAIL "+e);File.WriteAllText(PathFor("runtime.txt"),log.ToString());Debug.LogException(e);yield break;}yield return next;}
            log.AppendLine("PASS native menus and dialogue; production save untouched.");File.WriteAllText(PathFor("runtime.txt"),log.ToString());
        }
        static void Call(CascadeGame g,string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Private).Invoke(g,args);
        static IEnumerator Inspect(CascadeGame g)
        {
            typeof(CascadeGame).GetField("persistenceReady",Private).SetValue(g,false);
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.ReducedMotion=true;
            foreach(string locale in new[]{"en","ru","uk"}){
                g.SetUiLocale(locale);g.Preferences.LargeText=locale=="uk";
                string next=new UiCopy("Next stage").ToString();
                if(next!=(locale=="en"?"Next":locale=="ru"?"Далее":"Далі"))throw new InvalidOperationException("Incorrect between-stage caption: "+next);
                log.AppendLine("PASS "+locale+" between-stage caption: "+next);
                foreach(var screen in new[]{GameScreen.Shop,GameScreen.Supplies,GameScreen.CoinPacks,GameScreen.Journal}){
                    Call(g,"SetScreen",screen);
                    yield return null;
                    yield return PlayableCapture.CaptureSettled(Folder,screen+"-"+locale+".png",locale=="uk"?320:390,locale=="uk"?693:844,x=>CheckCounters(x,screen));
                }
                foreach(int world in Enumerable.Range(0,JourneyStory.WorldCount)){
                    g.OpenWorldComic(world,g.ShowHome);
                    typeof(CascadeGame).GetField("worldComicPage",Private).SetValue(g,2);
                    typeof(CascadeGame).GetField("worldComicLine",Private).SetValue(g,0);Call(g,"DrawMenu");
                    yield return PlayableCapture.CaptureSettled(Folder,"comic-"+world+"-"+locale+".png",locale=="uk"?320:390,locale=="uk"?693:844);
                    g.FinishWorldComic();
                }
                g.ShowJourney();g.ReviewConversation(StoryConversations.Island(0,0));
                yield return PlayableCapture.CaptureSettled(Folder,"conversation-"+locale+".png",locale=="uk"?320:390,locale=="uk"?693:844);
                g.SkipConversation();
                g.ShowJourney();yield return null;
                g.OpenJourneyIsland(0);yield return null;
                if(g.BrowsedIsland!=0)throw new InvalidOperationException("Could not open the first island level list for numeral review.");
                yield return PlayableCapture.CaptureSettled(Folder,"island-levels-"+locale+".png",locale=="uk"?320:390,locale=="uk"?693:844,x=>CheckIllustratedNumbers(x,GameScreen.JourneyMap));
                g.CloseJourneyIsland();g.ShowGame();yield return null;
                yield return PlayableCapture.CaptureSettled(Folder,"game-hud-"+locale+".png",locale=="uk"?320:390,locale=="uk"?693:844,x=>CheckIllustratedNumbers(x,GameScreen.Playing));
            }
            g.Preferences.LargeText=false;g.SetUiLocale("en");g.ShowHome();
        }
        static void CheckCounters(CascadeGame g,GameScreen screen)
        {
            var counters=g.GetComponentsInChildren<MenuSpriteCounter>().Where(c=>c.gameObject.activeInHierarchy).ToArray();
            foreach(var counter in counters){
                var source=counter.GetComponent<TMP_Text>();var digits=counter.GetComponentInChildren<VictoryScoreDigits>();
                if(source.canvasRenderer.GetAlpha()!=0||digits==null)throw new InvalidOperationException("Source caption visible or counter missing: "+source.name);
                var rect=(RectTransform)digits.transform;
                foreach(var image in digits.GetComponentsInChildren<Image>().Where(i=>i.gameObject.activeInHierarchy)){
                    if(image.sprite==null||image.raycastTarget)throw new InvalidOperationException("Counter art or input hit target: "+source.name);
                    var r=image.rectTransform;float edge=Mathf.Abs(r.anchoredPosition.x)+r.rect.width*.5f;
                    if(edge>rect.rect.width*.5f+1||r.rect.height>rect.rect.height+1)throw new InvalidOperationException("Counter overflow: "+source.name);
                }
            }
            CheckIllustratedNumbers(g,screen);
            if(counters.Length==0&&screen!=GameScreen.JourneyMap&&screen!=GameScreen.Playing)throw new InvalidOperationException("No illustrated counters: "+screen);
            log.AppendLine("PASS "+screen+": "+counters.Length+" standalone counters bounded.");
        }
        static void CheckIllustratedNumbers(CascadeGame g,GameScreen screen)
        {
            int inlineCount=0,illustratedCount=0;
            var labels=g.GetComponentsInChildren<TextMeshProUGUI>().Where(x=>x!=null&&x.gameObject.activeInHierarchy).ToArray();
            foreach(var label in labels){
                if(label.name=="Illustrated numeral copy")continue;
                string value=label.text??string.Empty;
                if(!value.Any(char.IsDigit)||IsSymbolFace(label.name)||label.name=="Mini glyph")continue;
                if(label.name.StartsWith("Island completed levels ",StringComparison.Ordinal))continue;
                if(label.name=="Purchase limits")continue;
                if(label.GetComponent<MenuSpriteCounter>()!=null){illustratedCount++;continue;}
                var inline=label.GetComponent<InlineIllustratedDigits>();
                if(inline==null)throw new InvalidOperationException("Unillustrated digits on "+screen+": "+label.name+" = "+value);
                var copy=label.transform.Find("Illustrated numeral copy")?.GetComponent<TextMeshProUGUI>();
                if(copy==null||label.canvasRenderer.GetAlpha()!=0||!copy.text.Contains("CASCADE_DIGITS_"))
                    throw new InvalidOperationException("Inline numeral copy missing sprites: "+label.name);
                copy.ForceMeshUpdate();
                int expectedSprites=CountOccurrences(copy.text,"<sprite=\"CASCADE_DIGITS_");
                if(expectedSprites==0||copy.textInfo.spriteCount<expectedSprites)
                    throw new InvalidOperationException("TMP did not resolve every authored digit sprite: "+label.name);
                if(copy.isTextOverflowing)throw new InvalidOperationException("Inline numeral text overflow: "+label.name+" = "+value);
                var sourceRect=label.rectTransform;var copyRect=copy.rectTransform;
                if(Vector2.Distance(sourceRect.rect.size,copyRect.rect.size)>1)
                    throw new InvalidOperationException("Inline numeral copy changed its layout box: "+label.name);
                inlineCount++;illustratedCount++;
            }
            log.AppendLine("PASS "+screen+": "+illustratedCount+" numeric labels use outlined numerals ("+inlineCount+" inline; overflow checked).");
        }
        static int CountOccurrences(string value,string token)
        {
            int count=0,offset=0;while((offset=value.IndexOf(token,offset,StringComparison.Ordinal))>=0){count++;offset+=token.Length;}return count;
        }
        static bool IsSymbolFace(string name)=>name=="Body painted letter"||name=="Bomb painted letter"||name=="Required letter"||name=="Symbol"||name=="TargetSymbol"||name=="DeviceGlyph"||name=="ZoomSymbol";
    }
}
