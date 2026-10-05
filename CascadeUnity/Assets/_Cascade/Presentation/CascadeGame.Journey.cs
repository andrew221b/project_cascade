using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Content;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int mapPage, selectedJourneyStop, dialogueLine;
        bool journeyActive;
        LevelContent pendingJourney;
        readonly HashSet<int> heardStory = new HashSet<int>();
        public bool IsJourneyAttempt => journeyActive;
        public int JourneyClearCount => Catalog.PrimaryLevels.Count(c=>sessionClears.Contains(c.Definition.CampaignNodeId));
        int NextJourneyStop()
        {
            for(int i=0;i<Catalog.PrimaryLevels.Count;i++)if(!(PreferredMode==PuzzleMode.Words&&i==25)&&!sessionClears.Contains(Catalog.PrimaryLevels[i].Definition.CampaignNodeId))return i;
            return Catalog.PrimaryLevels.Count-1;
        }
        public void ShowJourney()
        {
            selectedJourneyStop=journeyActive&&Interaction.Attempt.Current.Status==PuzzleStatus.Playing?Math.Max(0,Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node)):NextJourneyStop();mapPage=selectedJourneyStop/5;SetScreen(GameScreen.Home);
        }
        public void ShowLessons(){SetScreen(GameScreen.Lessons);}
        void DrawJourney()
        {
            menuWash.color=new Color(1,.96f,.84f,.08f);
            int first=mapPage*5,zone=JourneyStory.Zone(first);machine.SetZone(zone);
            string[] maps={"WindGardenMap","RainMillMap","CopperOrchardMap"};
            var map=Resources.Load<Sprite>("CascadeArt/Journey/"+maps[zone]);
            if(map!=null)Picture("Island map",menuRoot,new Vector2(0,.19f),new Vector2(1,.89f),map,false);
            var top=Rect("Map heading",menuRoot,new Vector2(.04f,.86f),new Vector2(.96f,.98f));Panel(top,Paper);
            var title=Text("Region",top,new Vector2(.05f,.40f),new Vector2(.78f,.98f),27,JourneyStory.ZoneName(zone));title.font=art.titleFont;title.alignment=TextAlignmentOptions.MidlineLeft;
            var count=Text("Restored lights",top,new Vector2(.05f,.04f),new Vector2(.78f,.40f),16,new UiCopy("{0} / {1} lights restored",JourneyClearCount,Catalog.PrimaryLevels.Count));count.alignment=TextAlignmentOptions.MidlineLeft;
            Button("Settings",top,new Vector2(.79f,.16f),new Vector2(.98f,.84f),ShowSettings).GetComponentInChildren<TextMeshProUGUI>().SetCopy("Menu");
            Vector2[] places={new Vector2(.25f,.397f),new Vector2(.59f,.469f),new Vector2(.40f,.545f),new Vector2(.65f,.608f),new Vector2(.81f,.687f)};
            int unlocked=NextJourneyStop();
            for(int i=0;i<5&&first+i<Catalog.PrimaryLevels.Count;i++) {
                int index=first+i;var content=Catalog.PrimaryLevels[index];bool done=sessionClears.Contains(content.Definition.CampaignNodeId);
                var point=places[i];
                if(i>0)MapPath(places[i-1],point,first+i<=unlocked);
                var b=Button(UiCopy.Raw("Stop"+(index+1)),menuRoot,point,point,()=>{selectedJourneyStop=index;DrawMenu();});
                var rect=(RectTransform)b.transform;rect.sizeDelta=new Vector2(62,60);b.GetComponentInChildren<TextMeshProUGUI>().SetCopy((index+1).ToString());
                b.GetComponent<Image>().color=done?new Color(.49f,.78f,.66f):index==selectedJourneyStop&&index<=unlocked?new Color(.99f,.76f,.32f):Paper;
                b.interactable=index<=unlocked;
                if(index==unlocked) {
                    var portrait=Resources.Load<Sprite>("CascadeArt/Journey/Mika");
                    if(portrait!=null) { var m=Picture("Mika on the path",menuRoot,point+new Vector2(-.19f,.01f),point+new Vector2(-.04f,.14f),portrait,true);m.raycastTarget=false; }
                }
            }
            var bottom=Rect("Journey task",menuRoot,new Vector2(.04f,.02f),new Vector2(.96f,.28f));Panel(bottom,Paper);
            bool future=selectedJourneyStop>unlocked;
            var task=Text("Next task",bottom,new Vector2(.06f,.70f),new Vector2(.94f,.97f),21,future?new UiCopy("Stops to this island: {0}",selectedJourneyStop-unlocked):JourneyStory.Task(mapPage));task.font=art.titleFont;
            bool resume=journeyActive&&Interaction.Attempt.Current.Status==PuzzleStatus.Playing&&Interaction.Attempt.RouteLength>0&&Catalog.PrimaryLevels[selectedJourneyStop].Definition.CampaignNodeId==node;
            var go=Button("Travel",bottom,new Vector2(.05f,.40f),new Vector2(.95f,.68f),()=>{if(future)ShowJourney();else if(resume)ShowGame();else BeginJourney(selectedJourneyStop);});
            go.GetComponentInChildren<TextMeshProUGUI>().SetCopy(future?"Back to my path":resume?"Continue":sessionClears.Contains(Catalog.PrimaryLevels[selectedJourneyStop].Definition.CampaignNodeId)?"Play again":"Let's go");go.GetComponent<Image>().color=new Color(.98f,.74f,.32f);
            Button("Lessons",bottom,new Vector2(.05f,.05f),new Vector2(.49f,.33f),ShowLessons);
            Button("Story",bottom,new Vector2(.51f,.05f),new Vector2(.95f,.33f),()=>OpenJourneyStory(selectedJourneyStop,false)).interactable=selectedJourneyStop<=unlocked;
            if(mapPage>0)MapTurn("Earlier",new Vector2(.15f,.81f),()=>{mapPage--;selectedJourneyStop=mapPage*5;DrawMenu();});
            if((mapPage+1)*5<Catalog.PrimaryLevels.Count)MapTurn("Later",new Vector2(.85f,.81f),()=>{mapPage++;selectedJourneyStop=Math.Min(NextJourneyStop(),mapPage*5+4);if(selectedJourneyStop<mapPage*5)selectedJourneyStop=mapPage*5;DrawMenu();});
            if(!future&&PreferredMode==PuzzleMode.Words&&selectedJourneyStop==25) {task.SetCopy("A shapes lesson. You can skip it.");go.GetComponentInChildren<TextMeshProUGUI>().SetCopy("Play shapes");}
        }
        void MapTurn(string name,Vector2 point,Action action)
        {
            var button=Button(name,menuRoot,point,point,action);((RectTransform)button.transform).sizeDelta=new Vector2(76,60);
        }
        void MapPath(Vector2 start,Vector2 end,bool open)
        {
            for(int dot=1;dot<6;dot++) {
                var p=Vector2.Lerp(start,end,dot/6f);var r=Rect("Path pebble",menuRoot,p,p);r.sizeDelta=new Vector2(8,5);
                var image=Background(r,open?new Color(.88f,.60f,.22f):new Color(.51f,.60f,.53f),false);image.sprite=art.panel;
            }
        }
        void BeginJourney(int index)
        {
            selectedJourneyStop=index;var primary=Catalog.PrimaryLevels[index];
            pendingJourney=primary;
            if(PreferredMode!=PuzzleMode.Shapes && Catalog.TryGetLocalizedBinding(primary.Definition.CampaignNodeId,Preferences.PuzzleLocale,out var binding))pendingJourney=binding;
            if(index%5==0&&!heardStory.Contains(index/5))OpenJourneyStory(index,true);else EnterJourneyPuzzle();
        }
        void OpenJourneyStory(int index,bool enterAfter)
        {
            selectedJourneyStop=index;dialogueLine=0;if(!enterAfter)pendingJourney=null;machine.SetZone(JourneyStory.Zone(index));SetScreen(GameScreen.Dialogue);
        }
        void EnterJourneyPuzzle()
        {
            if(pendingJourney==null){ShowJourney();return;}
            var content=pendingJourney;pendingJourney=null;LoadLevel(content.Definition,content.Locale);journeyActive=true;
            PersistProgress();
            levelLabel.SetCopy(JourneyStory.Task(selectedJourneyStop/5));levelLabel.fontSize=21;
            ShowGame();
        }
        void AdvanceDialogue()
        {
            if(++dialogueLine<JourneyStory.Lines(selectedJourneyStop/5).Length){DrawMenu();return;}
            heardStory.Add(selectedJourneyStop/5);PersistProgress();EnterJourneyPuzzle();
        }
        void DrawDialogue()
        {
            menuWash.color=new Color(1,.96f,.84f,.13f);int zone=JourneyStory.Zone(selectedJourneyStop);
            var portrait=Resources.Load<Sprite>("CascadeArt/Journey/"+JourneyStory.Portrait(zone));
            if(portrait!=null)Picture("Speaking friend",menuRoot,new Vector2(.12f,.39f),new Vector2(.88f,.92f),portrait,true);
            var panel=Rect("Dialogue frame",menuRoot,new Vector2(.04f,.08f),new Vector2(.96f,.46f));Panel(panel,Paper);
            var edge=Rect("Brass corner",panel,new Vector2(.025f,.88f),new Vector2(.975f,.90f));Background(edge,new Color(.92f,.70f,.31f),false);
            var speaker=Text("Speaker",panel,new Vector2(.07f,.71f),new Vector2(.93f,.88f),27,JourneyStory.Speaker(zone));speaker.font=art.titleFont;speaker.alignment=TextAlignmentOptions.MidlineLeft;
            var line=Text("Dialogue line",panel,new Vector2(.07f,.27f),new Vector2(.93f,.72f),22,JourneyStory.Lines(selectedJourneyStop/5)[dialogueLine]);line.alignment=TextAlignmentOptions.TopLeft;
            Button("Continue",panel,new Vector2(.52f,.04f),new Vector2(.93f,.24f),AdvanceDialogue).GetComponent<Image>().color=new Color(.98f,.74f,.32f);
            Button("Skip story",panel,new Vector2(.07f,.04f),new Vector2(.48f,.24f),()=>{heardStory.Add(selectedJourneyStop/5);PersistProgress();EnterJourneyPuzzle();}).GetComponentInChildren<TextMeshProUGUI>().fontSize=16;
        }
        void Panel(RectTransform parent,Color tint)
        {var image=Background(parent,tint,true);image.sprite=art.panel;image.type=Image.Type.Sliced;}
        Image Picture(string name,Transform parent,Vector2 min,Vector2 max,Sprite sprite,bool fit)
        {var image=Rect(name,parent,min,max).gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=fit;image.raycastTarget=false;return image;}
    }
}
