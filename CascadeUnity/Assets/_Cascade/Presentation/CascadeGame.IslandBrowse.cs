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
            var offered=JourneySequence().Select(c=>c.Definition.CampaignNodeId).ToHashSet();
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
            machine.LivingWorld.SetMapSky(null);machine.LivingWorld.SetWorkshop(true);
            var header=NavigationHeader(new UiCopy("Island {0}",islandBrowseIndex+1),CloseJourneyIsland);
            var title=header.Find("Page heading").GetComponent<TextMeshProUGUI>();
            title.rectTransform.anchorMin=new Vector2(.19f,.08f);title.rectTransform.anchorMax=new Vector2(.81f,.92f);title.margin=Vector4.zero;
            var world=CompletionText("Island world",menuRoot,new Vector2(.11f,.817f),new Vector2(.89f,.87f),20,JourneyStory.ZoneName(mapPage));world.margin=Vector4.zero;world.alignment=TextAlignmentOptions.Midline;
            var earned=stops.Select(n=>profile.Clears.FirstOrDefault(c=>c.CampaignNodeId==Catalog.PrimaryLevels[n].Definition.CampaignNodeId)).Where(c=>c!=null).ToArray();
            var total=Rect("Island earned stars",menuRoot,new Vector2(.5f,.775f),new Vector2(.5f,.775f));total.sizeDelta=new Vector2(164,34)*MenuUnit;
            var emblem=Picture("Island star emblem",total,new Vector2(.02f,.04f),new Vector2(.22f,.96f),CompletionArt(0),true);
            var count=CompletionText("Island star total",total,new Vector2(.24f,0),new Vector2(.98f,1),19,UiCopy.Raw(LevelCompletion.FormatStars(earned.Sum(c=>c.BestStarRating))+" / "+stops.Length*3),CelebrationType.SkyNumber);
            count.alignment=TextAlignmentOptions.MidlineLeft;count.margin=Vector4.zero;count.textWrappingMode=TextWrappingModes.NoWrap;
            // Centre the visible icon and digits, rather than a wider box with unused space.
            count.enableAutoSizing=false;count.fontSize=count.fontSizeMax;
            float iconSize=30*MenuUnit,gap=8*MenuUnit;
            float digitsWidth=Mathf.Ceil(count.GetPreferredValues(count.text,Mathf.Infinity,Mathf.Infinity).x);
            total.pivot=new Vector2(.5f,.5f);total.sizeDelta=new Vector2(iconSize+gap+digitsWidth,34*MenuUnit);
            var iconRect=emblem.rectTransform;iconRect.anchorMin=iconRect.anchorMax=new Vector2(0,.5f);iconRect.pivot=new Vector2(0,.5f);
            iconRect.sizeDelta=Vector2.one*iconSize;iconRect.anchoredPosition=Vector2.zero;
            var digits=count.rectTransform;digits.anchorMin=digits.anchorMax=new Vector2(0,.5f);digits.pivot=new Vector2(0,.5f);
            digits.sizeDelta=new Vector2(digitsWidth,34*MenuUnit);digits.anchoredPosition=new Vector2(iconSize+gap,0);
            var viewport=Rect("Island level viewport",menuRoot,new Vector2(MenuColumnInset,.105f),new Vector2(1-MenuColumnInset,.724f));
            Background(viewport,Color.clear,true);viewport.gameObject.AddComponent<RectMask2D>();
            var list=Rect("Island levels",viewport,new Vector2(0,1),Vector2.one);list.pivot=new Vector2(.5f,1);
            list.sizeDelta=new Vector2(0,(stops.Length*82+12)*MenuUnit);
            var layout=list.gameObject.AddComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(6,6,6,6);layout.spacing=10*MenuUnit;
            layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=list;
            scroll.horizontal=false;scroll.vertical=true;scroll.inertia=true;scroll.decelerationRate=.12f;scroll.movementType=ScrollRect.MovementType.Clamped;
            foreach(int stop in stops){var row=DrawIslandLevel(stop,0,1,list);var size=row.gameObject.AddComponent<LayoutElement>();size.minHeight=size.preferredHeight=72*MenuUnit;}
            int focus=Array.FindIndex(stops,stop=>stop==selectedJourneyStop);
            Canvas.ForceUpdateCanvases();float overflow=Mathf.Max(0,list.rect.height-viewport.rect.height);
            list.anchoredPosition=new Vector2(0,Mathf.Clamp(focus*82*MenuUnit-82*MenuUnit,0,overflow));
            var tip=CompletionText("Island replay cue",menuRoot,new Vector2(.10f,.045f),new Vector2(.90f,.092f),16,"Tap a level to play");tip.margin=Vector4.zero;
        }
        Button DrawIslandLevel(int stop,float bottom,float top,Transform parent)
        {
            var content=Catalog.PrimaryLevels[stop];var receipt=profile.Clears.FirstOrDefault(c=>c.CampaignNodeId==content.Definition.CampaignNodeId);
            bool open=IsJourneyStopOpen(stop),resume=CanResumeSelectedJourney(stop);
            var button=Button(UiCopy.Raw(""),parent,new Vector2(0,bottom),new Vector2(1,top),()=>ReplayJourneyLevel(stop));button.name="Island level "+(stop+1);
            var face=button.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=open?Color.white:new Color(.60f,.66f,.64f);
            button.interactable=open&&!saveBlocked;var colors=button.colors;colors.disabledColor=Color.white;button.colors=colors;button.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
            var level=CompletionText("Level number",button.transform,new Vector2(.055f,.20f),new Vector2(.22f,.83f),30,UiCopy.Raw(DisplayLevelNumber(content.Definition.CampaignNodeId)),CelebrationType.Number);level.margin=Vector4.zero;
            for(int s=0;s<3;s++){
                float x=.355f+s*.105f;var star=RatedStar("Earned star "+s,button.transform,new Vector2(x,.57f),new Vector2(x+.094f,.85f),receipt?.BestStarRating??0,s);
                if(receipt==null)star.color=new Color(.54f,.72f,.71f,.40f);
            }
            string formattedRecord=receipt==null?null:receipt.BestScore.ToString("N0",System.Globalization.CultureInfo.InvariantCulture).Replace(',', '.');
            UiCopy score=receipt!=null?new UiCopy("Best score: {0}",formattedRecord):open?(UiCopy)"Your next level":"Locked";
            var record=CompletionText("Level best score",button.transform,new Vector2(.26f,.145f),new Vector2(.755f,.46f),16,score,CelebrationType.EnamelLabel);
            record.enableAutoSizing=true;record.fontSizeMin=13*MenuUnit;record.fontSizeMax=16*MenuUnit;record.margin=Vector4.zero;record.textWrappingMode=TextWrappingModes.NoWrap;
            var arrow=Rect("Play direction",button.transform,new Vector2(.81f,.55f),new Vector2(.91f,.75f)).gameObject.AddComponent<ChevronGraphic>();arrow.PointsRight=true;arrow.Weight=1;arrow.color=MenuPaper;arrow.raycastTarget=false;
            arrow.gameObject.SetActive(open);
            var action=CompletionText("Level action",button.transform,new Vector2(.79f,.34f),new Vector2(.94f,.53f),14,resume?(UiCopy)"Continue":receipt!=null?"Replay":open?"Play":"",CelebrationType.EnamelLabel);
            action.fontSizeMin=12*MenuUnit;action.fontSizeMax=14*MenuUnit;action.margin=Vector4.zero;action.textWrappingMode=TextWrappingModes.NoWrap;
            return button;
        }
    }
}
