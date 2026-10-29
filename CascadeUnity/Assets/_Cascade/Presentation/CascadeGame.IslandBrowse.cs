using System;
using System.Linq;
using Cascade.Application;
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
        internal bool IslandTapDiscovered=>profile.SeenCoachPrompts.Contains("map_island_tap");

        int[] IslandJourneyStops(int world,int island)
        {
            if(world<0||world>=JourneyStory.WorldCount||island<0||island>=5)return Array.Empty<int>();
            PreferredLevels();var offered=preferredOfferedNodes;
            return Enumerable.Range(JourneyStory.IslandFirst(world,island),JourneyStory.IslandLevels(world,island))
                .Where(n=>n<Catalog.PrimaryLevels.Count&&offered.Contains(Catalog.PrimaryLevels[n].Definition.CampaignNodeId)).ToArray();
        }
        public void OpenJourneyIsland(int island)
        {
            if(CurrentScreen!=GameScreen.JourneyMap||ConversationOpen||IsPlanningDescent||IsDescending)return;
            if(!IslandJourneyStops(mapPage,island).Any(IsJourneyStopOpen))return;
            if(!profile.SeenCoachPrompts.Contains("map_island_tap")){profile.SeenCoachPrompts.Add("map_island_tap");PersistProgress();}
            islandBrowseIndex=island;feedback.Sound(ToyCue.Select);SetScreen(GameScreen.JourneyMap);menuTransition.Arrive();
        }
        public void CloseJourneyIsland()
        {
            islandBrowseIndex=-1;SetScreen(GameScreen.JourneyMap);menuTransition.Arrive(true);
        }
        public void ReplayJourneyLevel(int stop)
        {
            if(CurrentScreen!=GameScreen.JourneyMap||islandBrowseIndex<0||ConversationOpen||saveBlocked||IsPlanningDescent||IsDescending
                ||!IslandJourneyStops(mapPage,islandBrowseIndex).Contains(stop)||!IsJourneyStopOpen(stop))return;
            if(profile.Completion!=null&&!profile.Completion.Acknowledged){ShowGame();return;}
            if(PersonalMissionActive){RestorePersonalCampaign(false);if(HasSaveError)return;}
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
            var top=Rect("Map heading",menuRoot,new Vector2(.04f,.930f),new Vector2(.96f,.995f));
            var back=MapSquare("Back",top,.04f,false,ShowHome);
            foreach(var chevron in back.GetComponentsInChildren<ChevronGraphic>())Destroy(chevron.gameObject);
            var home=Rect("Menu home",back.transform,new Vector2(.23f,.23f),new Vector2(.77f,.77f)).gameObject.AddComponent<HomeGlyphGraphic>();home.color=MenuPaper;home.raycastTarget=false;
            var bag=MapSquare("My things",top,.96f,true,ShowTravelBag);bag.name="Map bag";
            foreach(var chevron in bag.GetComponentsInChildren<ChevronGraphic>())Destroy(chevron.gameObject);
            LoadNavigationArt();MenuIcon("Inventory icon",bag.transform,new Vector2(.5f,.5f),navigationIcons[1],30);
            var titleRow=Rect("Map world title",menuRoot,new Vector2(.10f,.846f),new Vector2(.90f,.923f));
            var title=Text("Region",titleRow,Vector2.zero,Vector2.one,21,JourneyStory.ZoneName(zone));
            title.font=art.titleFont;title.enableAutoSizing=true;title.fontSizeMin=17*MenuUnit;title.fontSizeMax=21*MenuUnit;
            title.textWrappingMode=TextWrappingModes.Normal;title.margin=Vector4.zero;if(zone==8)title.color=MenuPaper;
            DrawJourneyModeTabs();
            var bottom=Rect("Journey task",menuRoot,new Vector2(MenuColumnInset,0),new Vector2(1-MenuColumnInset,0));
            bool earlier=CanTurnJourneyMap(-1),later=CanTurnJourneyMap(1);
            var worldNav=Rect("World navigation",bottom,new Vector2(.08f,.53f),new Vector2(.92f,1));
            Button previous=null,next=null;
            if(earlier)previous=MapSquare("Previous world",worldNav,.03f,false,()=>TurnJourneyMap(-1));
            if(later)next=MapSquare("Next world",worldNav,.97f,true,()=>TurnJourneyMap(1));
            var go=Button("Travel",bottom,new Vector2(.06f,.03f),new Vector2(.94f,.45f),TravelSelectedJourney);
            go.GetComponentInChildren<TextMeshProUGUI>().SetCopy(PersonalMissionActive?(UiCopy)"Continue mission":CanResumeSelectedJourney(selectedJourneyStop)?(UiCopy)"Continue":new UiCopy("Level {0}",DisplayLevelNumber(Catalog.PrimaryLevels[selectedJourneyStop].Definition.CampaignNodeId)));
            StyleConversationButton(go,enamelControls[0],MenuPaper);
            var label=go.GetComponentInChildren<TextMeshProUGUI>();label.fontSizeMin=15*MenuUnit;label.fontSizeMax=18*MenuUnit;
            label.margin=new Vector4(7,3,7,3);label.enableAutoSizing=true;label.textWrappingMode=TextWrappingModes.NoWrap;
            journeyFooter=bottom;journeyFooterButtons=new[]{go,previous,next};FitJourneyFooter();
        }
        Button MapSquare(string caption,Transform parent,float x,bool right,Action action)
        {
            var button=DirectionButton(caption,parent,new Vector2(x,.5f),new Vector2(x,.5f),right,action);
            ((RectTransform)button.transform).sizeDelta=Vector2.one*44*MenuUnit;
            menuSquares.Add(new System.Collections.Generic.KeyValuePair<RectTransform,float>((RectTransform)button.transform,44));
            var face=button.GetComponent<Image>();
            if(caption!="Previous world"&&caption!="Next world"){
                face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=Color.white;
                foreach(var chevron in button.GetComponentsInChildren<ChevronGraphic>())chevron.color=MenuPaper;
                button.targetGraphic=face;
            }
            return button;
        }

        void DrawJourneyIsland()
        {
            var stops=IslandJourneyStops(mapPage,islandBrowseIndex);
            if(stops.Length==0||!stops.Any(IsJourneyStopOpen)){islandBrowseIndex=-1;DrawJourney();return;}
            journeyFooter=null;journeyFooterButtons=null;journeyModeTabs=null;
            DrawReferenceIsland(stops);
        }
    }
}
