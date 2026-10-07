using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int mapPage, selectedJourneyStop;
        bool journeyActive;
        RectTransform journeyFooter;
        Button[] journeyFooterButtons;
        LevelContent pendingJourney;
        readonly HashSet<int> heardStory = new HashSet<int>();
        public bool IsJourneyAttempt => journeyActive;
        public int JourneyClearCount => JourneySequence().Count(c=>sessionClears.Contains(c.Definition.CampaignNodeId));
        System.Collections.Generic.IReadOnlyList<LevelContent> JourneySequence()=>PreferredLevels();
        int NextJourneyStop()
        {
            var sequence=JourneySequence();var content=sequence.FirstOrDefault(c=>!sessionClears.Contains(c.Definition.CampaignNodeId))??sequence.LastOrDefault();
            return content==null?0:Math.Max(0,Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==content.Definition.CampaignNodeId));
        }
        public void ShowJourney()
        {
            selectedJourneyStop=journeyActive&&(Interaction.Attempt.Current.Status==PuzzleStatus.Playing||IsCheckpoint)?Math.Max(0,Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node)):NextJourneyStop();mapPage=JourneyStory.Zone(selectedJourneyStop);SetScreen(GameScreen.JourneyMap);TryIslandConversation();
        }
        public void ShowLessons(){chapterPage=0;SetScreen(GameScreen.Lessons);}
        void DrawJourney()
        {
            int first=JourneyStory.WorldFirst(mapPage),zone=mapPage;machine.SetZone(zone);
            var map=Resources.Load<Sprite>("CascadeArt/"+(zone<3?"Journey/":"Woodland/")+JourneyStory.MapAsset(zone));
            var sky=Resources.Load<Texture2D>("CascadeArt/Navigation/MapSky");machine.LivingWorld.SetMapSky(sky);
            var world=Rect("Parallax island world",menuRoot,Vector2.zero,Vector2.one);Background(world,Color.clear,true);
            world.gameObject.AddComponent<MapParallax>().Initialize(this);
            world.gameObject.AddComponent<MenuSwipe>().Initialize(TurnJourneyMap);
            int unlocked=NextJourneyStop(),markerStop=IsPinnedJourneyStop(selectedJourneyStop)?selectedJourneyStop:unlocked;Vector2[] places=MapPlaces(zone);
            if(map!=null){var scenery=Rect("Island map",world,new Vector2(0,.29f),new Vector2(1,.80f)).gameObject.AddComponent<SoftSceneryGraphic>();scenery.Initialize(map);scenery.SetAvailability(places.Select(p=>new Vector2(p.x,(p.y-.29f)/.51f)).ToArray(),Enumerable.Range(0,5).Select(i=>Enumerable.Range(JourneyStory.IslandFirst(zone,i),JourneyStory.IslandLevels(zone,i)).Any(IsJourneyStopOpen)).ToArray());scenery.SetSky(sky,uiCamera);}
            DrawMapAtmosphere(zone,world);
            for(int i=0;i<5;i++) {
                int island=i,begin=JourneyStory.IslandFirst(zone,i),count=JourneyStory.IslandLevels(zone,i);var point=places[i];
                int index=Enumerable.Range(begin,count).Where(n=>IsJourneyStopOpen(n)&&!sessionClears.Contains(Catalog.PrimaryLevels[n].Definition.CampaignNodeId)).DefaultIfEmpty(-1).First();
                if(index<0)index=begin;
                int target=index;
                if(!IsJourneyStopOpen(target))target=Enumerable.Range(begin,count).FirstOrDefault(IsJourneyStopOpen);
                var b=Button(UiCopy.Raw("Island"+(i+1)),world,point,point,()=>{selectedJourneyStop=target;DrawMenu();});
                ((RectTransform)b.transform).sizeDelta=new Vector2(76*MenuUnit,72*MenuUnit);b.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
                b.GetComponent<Image>().color=Color.clear;b.interactable=Enumerable.Range(begin,count).Any(IsJourneyStopOpen);
                if(JourneyStory.Zone(markerStop)!=zone||JourneyStory.IslandFor(markerStop)!=island)continue;
                var marker=Rect("You are here marker",world,point+new Vector2(-.21f,-.124f),point+new Vector2(.21f,-.067f));
                var caption=Text("You are here",marker,Vector2.zero,Vector2.one,17,new UiCopy("You are here · {0}/{1}",Enumerable.Range(begin,count).Count(n=>sessionClears.Contains(Catalog.PrimaryLevels[n].Definition.CampaignNodeId)),count));caption.font=art.titleFont;caption.color=MenuInk;caption.enableAutoSizing=true;caption.fontSizeMin=14;caption.fontSizeMax=17;
                var arrow=Rect("Here arrow",marker,new Vector2(.47f,.94f),new Vector2(.53f,1.15f));arrow.gameObject.AddComponent<ShapeFaceGraphic>().SetShape(art.shapes[0],Ink);
                var hero=Picture("Mika on the path",world,point+new Vector2(-.050f,-.002f),point+new Vector2(.050f,.085f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
                var heroTouch=Button(UiCopy.Raw(""),world,point+new Vector2(0,.045f),point+new Vector2(0,.045f),()=>OpenJourneyStory(markerStop,false));heroTouch.name="TalktoMika";heroTouch.interactable=!IsPlanningDescent&&!IsDescending;
                ((RectTransform)heroTouch.transform).sizeDelta=new Vector2(64*MenuUnit,80*MenuUnit);heroTouch.GetComponent<Image>().color=Color.clear;
                if(EquippedGift!=null)Picture("Mika's keepsake",hero.transform,new Vector2(.69f,.30f),new Vector2(.98f,.56f),TrophyArt(Array.IndexOf(ShopCatalog.TrophyIds,EquippedGift)),true);
            }
            var top=Rect("Map heading",menuRoot,new Vector2(.04f,.904f),new Vector2(.96f,.99f));
            var back=DirectionButton("Back",top,new Vector2(.015f,.12f),new Vector2(.18f,.90f),false,ShowHome);back.GetComponent<Image>().sprite=enamelControls[0];back.GetComponent<Image>().color=Color.white;foreach(var chevron in back.GetComponentsInChildren<ChevronGraphic>())chevron.color=new Color(1,.97f,.84f);
            var title=Text("Region",top,new Vector2(.22f,.43f),new Vector2(.95f,.95f),26,JourneyStory.ZoneName(zone));title.font=art.titleFont;title.alignment=TextAlignmentOptions.MidlineLeft;title.enableAutoSizing=true;title.fontSizeMin=20;title.fontSizeMax=26;
            var goal=Text("Journey goal",top,new Vector2(.22f,.07f),new Vector2(.95f,.44f),16,"Goal: Great Beacon");goal.alignment=TextAlignmentOptions.MidlineLeft;goal.enableAutoSizing=true;goal.fontSizeMin=14;goal.fontSizeMax=17;
            title.rectTransform.anchorMax=new Vector2(.79f,.95f);goal.rectTransform.anchorMax=new Vector2(.79f,.44f);
            var bag=Button("My things",top,new Vector2(.91f,.52f),new Vector2(.91f,.52f),ShowTravelBag);bag.name="Map bag";
            ((RectTransform)bag.transform).sizeDelta=Vector2.one*48*MenuUnit;bag.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
            menuSquares.Add(new System.Collections.Generic.KeyValuePair<RectTransform,float>((RectTransform)bag.transform,48));
            bag.GetComponent<Image>().sprite=enamelControls[0];bag.GetComponent<Image>().type=Image.Type.Sliced;bag.GetComponent<Image>().color=Color.white;
            LoadNavigationArt();MenuIcon("Inventory icon",bag.transform,new Vector2(.5f,.5f),navigationIcons[1],34);
            DrawJourneyModeTabs();
            var bottom=Rect("Journey task",menuRoot,new Vector2(.055f,0),new Vector2(.945f,0));
            bool future=!IsJourneyStopOpen(selectedJourneyStop);
            var task=Text("Next task",bottom,new Vector2(.06f,.805f),new Vector2(.94f,.985f),22,future?new UiCopy("Stops to this island: {0}",selectedJourneyStop-unlocked):StoryConversations.Task(selectedJourneyStop));task.font=art.titleFont;task.enableAutoSizing=true;task.fontSizeMin=18;task.fontSizeMax=23;
            var offered=JourneySequence().Select(c=>c.Definition.CampaignNodeId).ToHashSet();
            var stepNodes=Catalog.PrimaryLevels.Skip(first).Take(JourneyStory.WorldLevels(mapPage)).Select(c=>c.Definition.CampaignNodeId).Where(offered.Contains).ToArray();
            int remaining=CampaignProgress.Remaining(stepNodes,sessionClears);
            CompletionText("Map step progress",bottom,new Vector2(.06f,.635f),new Vector2(.94f,.795f),16,
                remaining==0?new UiCopy("World complete · {0}/{0} levels",stepNodes.Length):new UiCopy(remaining==1?"Next world in {0} level · {1}/{2}":"Next world in {0} levels · {1}/{2}",remaining,stepNodes.Length-remaining,stepNodes.Length));
            bool resume=CanResumeSelectedJourney(selectedJourneyStop);
            var go=Button("Travel",bottom,new Vector2(.17f,.36f),new Vector2(.83f,.62f),TravelSelectedJourney);
            go.GetComponentInChildren<TextMeshProUGUI>().SetCopy(future?"Back to my path":resume?"Continue":new UiCopy("Level {0}",selectedJourneyStop+1));go.GetComponent<Image>().sprite=enamelControls[1];go.GetComponent<Image>().color=Color.white;
            bool homecoming=JourneyClearCount==JourneySequence().Count;
            var talk=Button("Story",bottom,new Vector2(.05f,.055f),new Vector2(.95f,.315f),()=>{if(homecoming)SetScreen(GameScreen.JourneyEnd);else OpenJourneyStory(selectedJourneyStop,false);});talk.interactable=(homecoming||!future)&&!IsPlanningDescent&&!IsDescending;
            talk.GetComponentInChildren<TextMeshProUGUI>().SetCopy(homecoming?new UiCopy("The beacon is lit!"):new UiCopy("Talk to {0}",new UiCopy(StoryConversations.Resident(zone))));talk.GetComponent<Image>().color=new Color(.84f,.91f,.87f);
            talk.GetComponent<Image>().sprite=enamelControls[0];talk.GetComponent<Image>().type=UnityEngine.UI.Image.Type.Sliced;talk.GetComponent<Image>().color=Color.white;navigationLightLabels.Add(talk.GetComponentInChildren<TextMeshProUGUI>());
            journeyFooter=bottom;var previous=DirectionButton("Previous opened level",bottom,new Vector2(0,.36f),new Vector2(.145f,.62f),false,()=>SelectOpenedJourneyLevel(-1));
            var next=DirectionButton("Next opened level",bottom,new Vector2(.855f,.36f),new Vector2(1,.62f),true,()=>SelectOpenedJourneyLevel(1));
            foreach(var direction in new[]{previous,next}){direction.GetComponent<Image>().sprite=enamelControls[0];direction.GetComponent<Image>().color=Color.white;foreach(var chevron in direction.GetComponentsInChildren<ChevronGraphic>())chevron.color=new Color(1,.97f,.84f);}
            previous.interactable=Enumerable.Range(first,Math.Max(0,selectedJourneyStop-first)).Any(IsJourneyStopOpen);
            next.interactable=Enumerable.Range(selectedJourneyStop+1,Math.Max(0,first+JourneyStory.WorldLevels(zone)-selectedJourneyStop-1)).Any(IsJourneyStopOpen);
            journeyFooterButtons=new[]{go,talk,previous,next,back};FitJourneyFooter();
        }
        void FitJourneyFooter()
        {
            if(CurrentScreen!=GameScreen.JourneyMap||journeyFooter==null||journeyFooterButtons==null)return;
            FitJourneyModeTabs();
            journeyFooter.pivot=new Vector2(.5f,0);journeyFooter.sizeDelta=new Vector2(0,216*MenuUnit);journeyFooter.anchoredPosition=Vector2.zero;
            foreach(var control in journeyFooterButtons)if(control!=null){
                var rect=(RectTransform)control.transform;
                rect.sizeDelta=new Vector2(0,0);Canvas.ForceUpdateCanvases();
                if(rect.rect.height<48*MenuUnit)rect.sizeDelta=new Vector2(0,48*MenuUnit-rect.rect.height);
                if(rect.rect.width<48*MenuUnit)rect.sizeDelta+=Vector2.right*(48*MenuUnit-rect.rect.width);
            }
        }
        void SelectOpenedJourneyLevel(int direction)
        {
            int first=JourneyStory.WorldFirst(mapPage),last=first+JourneyStory.WorldLevels(mapPage);
            for(int next=selectedJourneyStop+direction;next>=first&&next<last;next+=direction)if(IsJourneyStopOpen(next)){
                selectedJourneyStop=next;feedback.Sound(ToyCue.Select);DrawMenu();return;
            }
        }
        public void TurnJourneyMap(int direction)
        {
            if(direction==0)return;int next=mapPage+Math.Sign(direction);var offered=JourneySequence().Select(c=>c.Definition.CampaignNodeId).ToHashSet();
            while(next>=0&&next<JourneyStory.WorldCount&&!Catalog.PrimaryLevels.Skip(JourneyStory.WorldFirst(next)).Take(JourneyStory.WorldLevels(next)).Any(c=>offered.Contains(c.Definition.CampaignNodeId)))next+=Math.Sign(direction);
            if(next<0||next>=JourneyStory.WorldCount||next==mapPage)return;
            mapPage=next;int first=JourneyStory.WorldFirst(mapPage);selectedJourneyStop=Mathf.Clamp(NextJourneyStop(),first,first+JourneyStory.WorldLevels(mapPage)-1);
            feedback.Sound(ToyCue.Select);DrawMenu();menuTransition.Arrive();
        }
        void DrawJourneyEnd()
        {
            machine.SetZone(7);
            MenuTitle("The beacon is lit!","Our little light is home.");
            var hero=Picture("Mika at home",menuRoot,new Vector2(.24f,.35f),new Vector2(.76f,.75f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            hero.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,3),0,1);
            Text("Journey ending",menuRoot,new Vector2(.10f,.24f),new Vector2(.90f,.36f),21,"We found the way together.");
            MenuButton("Look at our journey",.13f,ShowJourney,true);MenuButton("My journal",.025f,ShowJournal);
        }
        void DrawMapAtmosphere(int zone,Transform world)
        {
            Rect("Map weather",world,new Vector2(.02f,.29f),new Vector2(.98f,.85f)).gameObject.AddComponent<WorldWeatherGraphic>().InitializeRegion(this,zone);
            var sprite=Resources.Load<Sprite>("CascadeArt/Journey/Cloud");
            if(sprite!=null)for(int i=0;i<2;i++) {
                float y=i==0?.785f:.315f;var cloud=Picture("Map cloud "+i,world,new Vector2(i==0?-.10f:.62f,y),new Vector2(i==0?.35f:1.10f,y+.075f),sprite,true);
                cloud.color=new Color(1,1,1,.65f);cloud.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(i==0?22:-18,2),i*2);
            }
        }
        Vector2[] MapPlaces(int zone)
        {
            if(zone<3)return LiftMapPlaces(new[]{new Vector2(.25f,.397f),new Vector2(.59f,.469f),new Vector2(.40f,.545f),new Vector2(.65f,.608f),new Vector2(.81f,.687f)});
            Vector2[][] plazas={
                new[]{new Vector2(.316f,.763f),new Vector2(.667f,.612f),new Vector2(.326f,.467f),new Vector2(.612f,.329f),new Vector2(.810f,.191f)},
                new[]{new Vector2(.366f,.786f),new Vector2(.666f,.620f),new Vector2(.321f,.458f),new Vector2(.567f,.329f),new Vector2(.793f,.166f)},
                new[]{new Vector2(.280f,.820f),new Vector2(.681f,.694f),new Vector2(.280f,.533f),new Vector2(.615f,.371f),new Vector2(.805f,.194f)},
                new[]{new Vector2(.278f,.791f),new Vector2(.690f,.627f),new Vector2(.330f,.510f),new Vector2(.640f,.358f),new Vector2(.814f,.198f)},
                new[]{new Vector2(.325f,.759f),new Vector2(.661f,.607f),new Vector2(.344f,.479f),new Vector2(.630f,.344f),new Vector2(.796f,.196f)}};
            var result=plazas[zone-3];for(int i=0;i<result.Length;i++)result[i]=new Vector2(result[i].x,.19f+(1-result[i].y)*.70f);return LiftMapPlaces(result);
        }
        Vector2[] LiftMapPlaces(Vector2[] points){for(int i=0;i<points.Length;i++)points[i]=new Vector2(points[i].x,.29f+(points[i].y-.19f)/.70f*.51f);return points;}
        void BeginJourney(int index)
        {
            if(index<0||index>=Catalog.PrimaryLevels.Count||!IsJourneyStopOpen(index))return;
            selectedJourneyStop=index;var primary=Catalog.PrimaryLevels[index];
            pendingJourney=primary;
            pendingJourney=JourneySequence().FirstOrDefault(c=>c.Definition.CampaignNodeId==primary.Definition.CampaignNodeId);
            if(!heardStory.Contains(JourneyStory.PageFor(JourneyStory.WorldFirst(JourneyStory.Zone(index)))))OpenJourneyStory(index,true);else EnterJourneyPuzzle();
        }
        void OpenJourneyStory(int index,bool enterAfter)
        {
            if(ConversationOpen||IsPlanningDescent||IsDescending||!IsJourneyStopOpen(index))return;
            selectedJourneyStop=index;machine.SetZone(JourneyStory.Zone(index));
            if(enterAfter){SetScreen(GameScreen.JourneyMap);OpenConversation(StoryConversations.BeforeLevel(index),"Continue",EnterJourneyPuzzle);}
            else {
                pendingJourney=null;
                int receipt=StoryConversations.BeforeLevel(index).Receipt;
                OpenConversation(StoryConversations.Visit(index,receipt,heardStory.Contains(receipt)),CanResumeSelectedJourney(index)?new UiCopy("Continue"):new UiCopy("Level {0}",index+1),TravelSelectedJourney,()=>{});
            }
        }
        void EnterJourneyPuzzle()
        {
            if(pendingJourney==null){ShowJourney();return;}
            var content=pendingJourney;pendingJourney=null;
            StartDescentLevel(content.Definition,content.Locale,profile.Director.Choose(content.Definition));
            descentTravel=journeyActive=true;
            PersistProgress();
            levelLabel.SetCopy(StoryConversations.Task(selectedJourneyStop));levelLabel.fontSize=21;
            ShowGame();
        }
        void DrawDialogue(){OpenConversation(StoryConversations.Visit(selectedJourneyStop),"Back to the map",ShowJourney);}
        void Panel(RectTransform parent,Color tint)
        {var image=Background(parent,tint,true);image.sprite=art.panel;image.type=Image.Type.Sliced;}
        Image Picture(string name,Transform parent,Vector2 min,Vector2 max,Sprite sprite,bool fit)
        {var image=Rect(name,parent,min,max).gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=fit;image.raycastTarget=false;return image;}
    }
}
