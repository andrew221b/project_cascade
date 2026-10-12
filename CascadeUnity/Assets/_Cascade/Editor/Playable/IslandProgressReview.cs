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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // Native screen and routing checks use disposable progress, never the player's save.
    public static class IslandProgressReview
    {
        const string Folder="ActiveIslandProgress-Oct09";
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Requires an isolated review profile.");
            game.StartCoroutine(Review(game));
        }
        static IEnumerable<int> Stories()=>new[]{1000}.Concat(Enumerable.Range(0,JourneyStory.PageCount))
            .Concat(Enumerable.Range(2000,JourneyStory.WorldCount*10)).Concat(Enumerable.Range(4000,JourneyStory.TotalLevels))
            .Concat(Enumerable.Range(6000,JourneyStory.WorldCount)).Concat(Enumerable.Range(7000,JourneyStory.TotalLevels))
            .Concat(Enumerable.Range(8000,JourneyStory.WorldCount*10));
        static T Read<T>(CascadeGame game,string name)=>(T)typeof(CascadeGame).GetField(name,Hidden).GetValue(game);
        static void Write(CascadeGame game,string name,object value)=>typeof(CascadeGame).GetField(name,Hidden).SetValue(game,value);
        static object Call(CascadeGame game,string name,params object[] arguments)=>typeof(CascadeGame).GetMethod(name,Hidden).Invoke(game,arguments);
        static void Require(bool condition,string detail){if(!condition)throw new InvalidOperationException(detail);}
        static TextMeshProUGUI Label(CascadeGame game,string name)=>game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name==name);
        static void Pose(CascadeGame game,float clock)
        {
            var cue=game.GetComponentInChildren<MapIslandTapCue>();
            typeof(MapIslandTapCue).GetField("clock",Hidden).SetValue(cue,clock);
            typeof(MapIslandTapCue).GetMethod("LateUpdate",Hidden).Invoke(cue,null);
        }
        static void CheckMap(CascadeGame game)
        {
            Canvas.ForceUpdateCanvases();
            var icon=game.GetComponentsInChildren<Image>().Single(t=>t.name=="Mode earned stars").rectTransform;
            var home=(RectTransform)game.GetComponentInChildren<HomeGlyphGraphic>().transform.parent;
            var bag=(RectTransform)game.GetComponentsInChildren<Button>().Single(t=>t.name=="Map bag").transform;
            var centre=(home.TransformPoint(home.rect.center)+bag.TransformPoint(bag.rect.center))/2;
            Require(Vector3.Distance(icon.TransformPoint(icon.rect.center),centre)<.005f,"Star emblem must sit at the midpoint of Home and Bag.");
            Require(!game.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.name=="World page"),"World ordinal must not appear on the map.");
            var badges=game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Island completed levels ")).ToArray();
            Require(badges.Length==1,"Only Mika's island must have a progress badge.");
            var hero=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Mika on the path");
            int island=int.Parse(hero.transform.parent.name.Substring("Floating island ".Length));
            {
                var stops=(int[])Call(game,"IslandJourneyStops",game.MapWorld,island);
                int done=stops.Count(n=>Read<HashSet<string>>(game,"sessionClears").Contains(game.Catalog.PrimaryLevels[n].Definition.CampaignNodeId));
                var label=Label(game,"Island completed levels "+(island+1));
                Require(label.text==done+" / "+stops.Length,"Island completion count must reflect the selected mode.");
                Require(!label.raycastTarget&&!label.transform.parent.GetComponent<Image>().raycastTarget,"Completion badge must not intercept taps.");
                label.ForceMeshUpdate();Require(!label.isTextOverflowing,"Island completion count must fit.");
                var target=(RectTransform)game.GetComponentsInChildren<Button>().Single(b=>b.name=="Island"+(island+1)).transform;
                var badge=(RectTransform)label.transform.parent;
                var centrePoint=target.TransformPoint(target.rect.center);var badgePoint=badge.TransformPoint(badge.rect.center);
                Require(Mathf.Abs(centrePoint.y-badgePoint.y)<.005f,"Badge must stay horizontally beside its island.");
                var corners=new Vector3[4];target.GetWorldCorners(corners);float left=corners[0].x,right=corners[2].x;
                badge.GetWorldCorners(corners);
                Require(corners[0].x>right||corners[2].x<left,"Badge must not overlap the island touch/art envelope.");
            }
            Require(!game.GetComponentInChildren<MapIslandTapCue>().GetComponent<Image>().raycastTarget,"The finger must not intercept taps.");
        }
        static void CheckIslandHeading(CascadeGame game)
        {
            Canvas.ForceUpdateCanvases();
            float axis=Label(game,"Island world").rectTransform.TransformPoint(Label(game,"Island world").rectTransform.rect.center).x;
            var heading=Label(game,"Page heading").rectTransform;
            Require(Mathf.Abs(heading.TransformPoint(heading.rect.center).x-axis)<.005f,"Island title and world subtitle must share a centre axis.");
            var icon=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Island star emblem").rectTransform;
            var digits=Label(game,"Island star total").rectTransform;var corners=new Vector3[4];
            icon.GetWorldCorners(corners);float left=corners[0].x;digits.GetWorldCorners(corners);float right=corners[2].x;
            Require(Mathf.Abs((left+right)*.5f-axis)<.005f,"Visible star and digits must share the heading centre axis.");
        }
        static IEnumerator Review(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;game.SetUiLocale("en");
            game.Preferences.Tutorials=true;game.Preferences.ReducedMotion=false;game.ReducedMotion=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            var profile=Read<PlayerProfile>(game,"profile");var heard=Stories().Distinct().ToList();
            Read<HashSet<int>>(game,"heardStory").UnionWith(heard);profile.HeardStory=heard;
            profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            foreach(var mode in new[]{PuzzleMode.Words,PuzzleMode.Mixed})profile.ModeJourneys.Add(new ModeJourney{Mode=mode.ToString(),HeardStory=heard.ToList(),SeenMechanics=profile.SeenMechanics.ToList()});
            AddClear(game,game.Catalog.GetPrimary("C01"),3);game.ShowJourney();yield return null;yield return null;
            yield return PlayableCapture.CaptureSettled(Folder,"map-en.png",390,844,g=>{Pose(g,0);CheckMap(g);});
            yield return PlayableCapture.CaptureSettled(Folder,"map-finger.png",390,844,g=>{Pose(g,4.1f);CheckMap(g);Require(g.GetComponentInChildren<MapIslandTapCue>().GetComponent<Image>().enabled,"Idle cue must be visible.");});
            var attempt=game.Interaction.Attempt;string hash=attempt.CurrentStateHash;int receipts=profile.Clears.Count;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Island1").onClick.Invoke();yield return null;yield return null;
            Require(game.BrowsedIsland==0,"A tap must open the selected island.");
            yield return PlayableCapture.CaptureSettled(Folder,"island-en.png",390,844,g=>{CheckIslandHeading(g);Require(Label(g,"Island star total").text=="3 / 60","Island stars must omit the multiplication mark.");});
            game.CloseJourneyIsland();yield return null;yield return null;
            Require(game.BrowsedIsland==-1&&game.Interaction.Attempt==attempt&&attempt.CurrentStateHash==hash&&profile.Clears.Count==receipts,"Browsing and returning must preserve the active attempt and rewards.");
            Require(profile.SeenCoachPrompts.Contains("map_island_tap"),"A real island tap must record discovery.");
            game.ReducedMotion=true;Pose(game,12);Require(!game.GetComponentInChildren<MapIslandTapCue>().GetComponent<Image>().enabled,"Reduce Motion must suppress the cue.");
            game.ReducedMotion=false;game.Preferences.Tutorials=false;Pose(game,12);Require(!game.GetComponentInChildren<MapIslandTapCue>().GetComponent<Image>().enabled,"Disabled tutorials must suppress the cue.");
            game.Preferences.Tutorials=true;game.Preferences.LargeText=true;game.SetUiLocale("uk");
            yield return new WaitForSecondsRealtime(.35f);game.ShowJourney();yield return null;yield return null;
            yield return PlayableCapture.CaptureSettled(Folder,"map-uk-small.png",320,693,g=>{Pose(g,0);CheckMap(g);});
            game.OpenJourneyIsland(0);yield return null;yield return null;
            yield return PlayableCapture.CaptureSettled(Folder,"island-uk-small.png",320,693,CheckIslandHeading);
            game.CloseJourneyIsland();yield return null;yield return null;game.Preferences.LargeText=false;game.SetUiLocale("en");
            game.SelectMode(PuzzleMode.Words);yield return null;yield return null;
            Require(game.PreferredMode==PuzzleMode.Words&&game.JourneyClearCount==0,"Words must not inherit Shapes completions.");
            yield return PlayableCapture.CaptureSettled(Folder,"map-words.png",390,844,g=>{Pose(g,0);CheckMap(g);});
            game.SelectMode(PuzzleMode.Shapes);yield return null;yield return null;
            foreach(var content in ((IEnumerable<LevelContent>)Call(game,"JourneySequence")).Where(c=>JourneyStory.Zone(game.Catalog.PrimaryLevels.ToList().FindIndex(p=>p.Definition.CampaignNodeId==c.Definition.CampaignNodeId))<8))
                if(!Read<HashSet<string>>(game,"sessionClears").Contains(content.Definition.CampaignNodeId))AddClear(game,content,3);
            Write(game,"journeyActive",false);Read<PlayerProfile>(game,"profile").JourneyAttempt=false;game.ShowJourney();yield return null;yield return null;
            Require(game.MapWorld==8,"Night fixture must reach the observatory.");
            yield return PlayableCapture.CaptureSettled(Folder,"map-night.png",390,844,g=>{Pose(g,0);CheckMap(g);});
            string directory=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Folder));
            File.WriteAllText(Path.Combine(directory,"native-review.txt"),"PASS: star emblem midpoint at native layouts; one mode-specific completed/total badge beside Mika's island with no touch-envelope overlap; no map world ordinal or × on island stars; actual island tap/back preserves attempt/rewards; discovered receipt; idle cue appears and does not intercept taps; reduced motion and tutorial opt-out suppress it; Words progress isolated; EN390 and UK320 large text plus night captures. Synthetic valid progress was confined to a disposable review profile. Not a physical-device touch/FPS test.\n");
            Debug.Log("CASCADE island progress native review complete.");
        }
        static void AddClear(CascadeGame game,LevelContent content,int stars)
        {
            Read<PlayerProfile>(game,"profile").Clears.Add(new ClearReceipt{CampaignNodeId=content.Definition.CampaignNodeId,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=content.Definition.ContentHash,BestStars=stars,BestScore=1680});
            Read<HashSet<string>>(game,"sessionClears").Add(content.Definition.CampaignNodeId);
        }
    }
}
