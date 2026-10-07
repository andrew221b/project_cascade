using System;
using System.Linq;
using Cascade.Content;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int islandBrowseIndex=-1;
        public int MapWorld=>mapPage;
        public int BrowsedIsland=>islandBrowseIndex;

        int[] IslandJourneyStops(int world,int island)
        {
            if(world<0||world>=JourneyStory.WorldCount||island<0||island>=5)return Array.Empty<int>();
            var offered=JourneySequence().Select(c=>c.Definition.CampaignNodeId).ToHashSet();
            return Enumerable.Range(JourneyStory.IslandFirst(world,island),JourneyStory.IslandLevels(world,island))
                .Where(n=>n<Catalog.PrimaryLevels.Count&&offered.Contains(Catalog.PrimaryLevels[n].Definition.CampaignNodeId)).ToArray();
        }
        public void OpenJourneyIsland(int island)
        {
            if(CurrentScreen!=GameScreen.JourneyMap||ConversationOpen||IsPlanningDescent||IsDescending)return;
            if(!IslandJourneyStops(mapPage,island).Any(IsJourneyStopOpen))return;
            islandBrowseIndex=island;feedback.Sound(ToyCue.Select);SetScreen(GameScreen.JourneyMap);
        }
        public void CloseJourneyIsland()
        {
            islandBrowseIndex=-1;SetScreen(GameScreen.JourneyMap);
        }
        public void ReplayJourneyLevel(int stop)
        {
            if(CurrentScreen!=GameScreen.JourneyMap||islandBrowseIndex<0||ConversationOpen||saveBlocked||IsPlanningDescent||IsDescending
                ||!IslandJourneyStops(mapPage,islandBrowseIndex).Contains(stop)||!IsJourneyStopOpen(stop))return;
            if(profile.Completion!=null&&!profile.Completion.Acknowledged){ShowGame();return;}
            selectedJourneyStop=stop;islandBrowseIndex=-1;TravelSelectedJourney();
        }
        int OpenedJourneyWorld(int direction)
        {
            if(direction==0)return -1;
            int next=mapPage+Math.Sign(direction);
            while(next>=0&&next<JourneyStory.WorldCount){
                if(Enumerable.Range(0,5).Any(i=>IslandJourneyStops(next,i).Any(IsJourneyStopOpen)))return next;
                next+=Math.Sign(direction);
            }
            return -1;
        }
        public bool CanTurnJourneyMap(int direction)=>OpenedJourneyWorld(direction)>=0;

        void DrawJourneyChrome(int zone,int first)
        {
            var top=Rect("Map heading",menuRoot,new Vector2(.04f,.928f),new Vector2(.96f,.997f));
            var back=MapSquare("Back",top,.04f,false,ShowHome);
            foreach(var chevron in back.GetComponentsInChildren<ChevronGraphic>())Destroy(chevron.gameObject);
            var home=Rect("Menu home",back.transform,new Vector2(.23f,.23f),new Vector2(.77f,.77f)).gameObject.AddComponent<HomeGlyphGraphic>();home.color=MenuPaper;home.raycastTarget=false;
            var bag=MapSquare("My things",top,.96f,true,ShowTravelBag);bag.name="Map bag";
            foreach(var chevron in bag.GetComponentsInChildren<ChevronGraphic>())Destroy(chevron.gameObject);
            LoadNavigationArt();MenuIcon("Inventory icon",bag.transform,new Vector2(.5f,.5f),navigationIcons[1],30);
            bool earlier=CanTurnJourneyMap(-1),later=CanTurnJourneyMap(1);
            if(earlier)MapSquare("Previous world",top,.24f,false,()=>TurnJourneyMap(-1));
            if(later)MapSquare("Next world",top,.76f,true,()=>TurnJourneyMap(1));
            var title=Text("Region",top,new Vector2(earlier?.33f:.18f,.02f),new Vector2(later?.67f:.82f,.98f),23,JourneyStory.ZoneName(zone));
            title.font=art.titleFont;title.enableAutoSizing=true;title.fontSizeMin=15*MenuUnit;title.fontSizeMax=23*MenuUnit;
            title.textWrappingMode=TextWrappingModes.Normal;title.margin=Vector4.zero;if(zone==8)title.color=MenuPaper;
            DrawJourneyModeTabs();
            var bottom=Rect("Journey task",menuRoot,new Vector2(.085f,0),new Vector2(.915f,0));
            var stops=Enumerable.Range(0,5).SelectMany(i=>IslandJourneyStops(zone,i)).ToArray();
            int complete=stops.Count(n=>sessionClears.Contains(Catalog.PrimaryLevels[n].Definition.CampaignNodeId));
            var progress=CompletionText("Map step progress",bottom,new Vector2(.06f,.79f),new Vector2(.94f,1.015f),15,new UiCopy("Completed: {0}/{1}",complete,stops.Length));
            progress.fontSize=15*MenuUnit;progress.margin=Vector4.zero;progress.textWrappingMode=TextWrappingModes.NoWrap;
            var go=Button("Travel",bottom,new Vector2(.17f,.395f),new Vector2(.83f,.76f),TravelSelectedJourney);
            go.GetComponentInChildren<TextMeshProUGUI>().SetCopy(CanResumeSelectedJourney(selectedJourneyStop)?(UiCopy)"Continue":new UiCopy("Level {0}",selectedJourneyStop+1));
            StyleConversationButton(go,enamelControls[0],MenuPaper);
            bool homecoming=JourneyClearCount==JourneySequence().Count;
            var talk=Button("Story",bottom,new Vector2(.10f,0),new Vector2(.90f,.365f),()=>{if(homecoming)SetScreen(GameScreen.JourneyEnd);else OpenJourneyStory(selectedJourneyStop,false);});
            talk.GetComponentInChildren<TextMeshProUGUI>().SetCopy(homecoming?new UiCopy("The beacon is lit!"):new UiCopy("Talk to {0}",new UiCopy(StoryConversations.Resident(zone))));
            talk.interactable=(homecoming||IsJourneyStopOpen(selectedJourneyStop))&&!IsPlanningDescent&&!IsDescending;
            StyleConversationButton(talk,enamelControls[0],MenuPaper);
            foreach(var control in new[]{go,talk}){var label=control.GetComponentInChildren<TextMeshProUGUI>();label.fontSizeMin=15*MenuUnit;label.fontSizeMax=18*MenuUnit;label.margin=new Vector4(7,3,7,3);label.enableAutoSizing=true;label.textWrappingMode=TextWrappingModes.NoWrap;}
            journeyFooter=bottom;journeyFooterButtons=new[]{go,talk};FitJourneyFooter();
        }
        Button MapSquare(string caption,Transform parent,float x,bool right,Action action)
        {
            var button=DirectionButton(caption,parent,new Vector2(x,.5f),new Vector2(x,.5f),right,action);
            ((RectTransform)button.transform).sizeDelta=Vector2.one*44*MenuUnit;
            menuSquares.Add(new System.Collections.Generic.KeyValuePair<RectTransform,float>((RectTransform)button.transform,44));
            var face=button.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=Color.white;
            foreach(var chevron in button.GetComponentsInChildren<ChevronGraphic>())chevron.color=MenuPaper;
            return button;
        }

        void DrawJourneyIsland()
        {
            var stops=IslandJourneyStops(mapPage,islandBrowseIndex);
            if(stops.Length==0||!stops.Any(IsJourneyStopOpen)){islandBrowseIndex=-1;DrawJourney();return;}
            journeyFooter=null;journeyFooterButtons=null;journeyModeTabs=null;
            machine.LivingWorld.SetMapSky(null);machine.LivingWorld.SetWorkshop(true);
            NavigationHeader(new UiCopy("Island {0}",islandBrowseIndex+1),CloseJourneyIsland);
            var back=menuRoot.GetComponentsInChildren<Button>().Single(b=>b.name=="Back");
            StyleConversationButton(back,enamelControls[0],MenuPaper);
            foreach(var chevron in back.GetComponentsInChildren<ChevronGraphic>())chevron.color=MenuPaper;
            var world=CompletionText("Island world",menuRoot,new Vector2(.11f,.817f),new Vector2(.89f,.87f),20,JourneyStory.ZoneName(mapPage));world.margin=Vector4.zero;
            var earned=stops.Select(n=>profile.Clears.FirstOrDefault(c=>c.CampaignNodeId==Catalog.PrimaryLevels[n].Definition.CampaignNodeId)).Where(c=>c!=null).ToArray();
            var total=Rect("Island earned stars",menuRoot,new Vector2(.5f,.775f),new Vector2(.5f,.775f));total.sizeDelta=new Vector2(104,34)*MenuUnit;
            Picture("Island star emblem",total,new Vector2(.03f,.04f),new Vector2(.35f,.96f),CompletionArt(0),true);
            CompletionText("Island star total",total,new Vector2(.39f,0),Vector2.one,19,UiCopy.Raw(earned.Sum(c=>c.BestStars)+" / "+stops.Length*5));
            float pitch=Mathf.Min(.175f,.65f/stops.Length),top=.724f;
            for(int i=0;i<stops.Length;i++)DrawIslandLevel(stops[i],top-i*pitch-pitch+.010f,top-i*pitch);
            var tip=CompletionText("Island replay cue",menuRoot,new Vector2(.10f,.045f),new Vector2(.90f,.092f),16,"Tap a level to play");tip.margin=Vector4.zero;
        }
        void DrawIslandLevel(int stop,float bottom,float top)
        {
            var content=Catalog.PrimaryLevels[stop];var receipt=profile.Clears.FirstOrDefault(c=>c.CampaignNodeId==content.Definition.CampaignNodeId);
            bool open=IsJourneyStopOpen(stop),resume=CanResumeSelectedJourney(stop);
            var button=Button(UiCopy.Raw(""),menuRoot,new Vector2(.10f,bottom),new Vector2(.90f,top),()=>ReplayJourneyLevel(stop));button.name="Island level "+(stop+1);
            var face=button.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=open?Color.white:new Color(.60f,.66f,.64f);
            button.interactable=open&&!saveBlocked;var colors=button.colors;colors.disabledColor=Color.white;button.colors=colors;button.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
            var level=CompletionText("Level number",button.transform,new Vector2(.055f,.20f),new Vector2(.22f,.83f),30,UiCopy.Raw((stop+1).ToString()),CelebrationType.Number);level.margin=Vector4.zero;
            for(int s=0;s<5;s++){
                float x=.275f+s*.090f;var star=Picture("Earned star "+s,button.transform,new Vector2(x,.57f),new Vector2(x+.080f,.85f),CompletionArt(0),true);
                if(s>=(receipt?.BestStars??0))star.color=new Color(.54f,.72f,.71f,.40f);
            }
            UiCopy score=receipt!=null?new UiCopy("Best score: {0}",receipt.BestScore.ToString("N0")):open?(UiCopy)"Your next level":"Locked";
            var record=CompletionText("Level best score",button.transform,new Vector2(.26f,.145f),new Vector2(.755f,.46f),16,score,CelebrationType.EnamelLabel);
            record.enableAutoSizing=true;record.fontSizeMin=13*MenuUnit;record.fontSizeMax=16*MenuUnit;record.margin=Vector4.zero;record.textWrappingMode=TextWrappingModes.NoWrap;
            var arrow=Rect("Play direction",button.transform,new Vector2(.80f,.46f),new Vector2(.925f,.76f)).gameObject.AddComponent<ChevronGraphic>();arrow.PointsRight=true;arrow.color=MenuPaper;arrow.raycastTarget=false;
            arrow.gameObject.SetActive(open);
            var action=CompletionText("Level action",button.transform,new Vector2(.755f,.14f),new Vector2(.95f,.43f),14,resume?(UiCopy)"Continue":receipt!=null?"Replay":open?"Play":"",CelebrationType.EnamelLabel);
            action.fontSizeMin=12*MenuUnit;action.fontSizeMax=14*MenuUnit;action.margin=Vector4.zero;action.textWrappingMode=TextWrappingModes.NoWrap;
        }
    }
}
