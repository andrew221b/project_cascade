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
            var sequence=JourneySequence();string next=CampaignProgress.Next(sequence.Select(c=>c.Definition.CampaignNodeId).ToArray(),sessionClears,sequence.Where(c=>c.Definition.TeachingNode>0).Select(c=>c.Definition.CampaignNodeId));
            var content=sequence.FirstOrDefault(c=>c.Definition.CampaignNodeId==next)??sequence.LastOrDefault();
            return content==null?0:Math.Max(0,Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==content.Definition.CampaignNodeId));
        }
        public void ShowJourney()
        {
            islandBrowseIndex=-1;selectedJourneyStop=HasResumableJourneyAttempt?Math.Max(0,Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node)):NextJourneyStop();mapPage=JourneyStory.Zone(selectedJourneyStop);
            ShowSelectedJourney();
        }
        void ShowSelectedJourney()
        {
            if(TryParallelArrival())return;
            if(TryWorldComic(mapPage,ShowSelectedJourney))return;
            SetScreen(GameScreen.JourneyMap);TryIslandConversation();if(!ConversationOpen)TryJourneyOpening();
        }
        public void ShowLessons(){chapterPage=0;SetScreen(GameScreen.Lessons);}
        void DrawJourney()
        {
            if(islandBrowseIndex>=0){DrawJourneyIsland();return;}
            int first=JourneyStory.WorldFirst(mapPage),zone=mapPage;machine.SetZone(zone);
            var map=Resources.Load<Sprite>("CascadeArt/"+(zone<3?"Journey/":"Woodland/")+JourneyStory.MapAsset(zone));
            var sky=Resources.Load<Texture2D>(zone==8?"CascadeArt/Woodland/StarObservatoryBase":"CascadeArt/Navigation/MapSky");machine.LivingWorld.SetMapSky(sky);
            var world=Rect("Parallax island world",menuRoot,Vector2.zero,Vector2.one);Background(world,Color.clear,true);
            world.gameObject.AddComponent<MapParallax>().Initialize(this);
            world.gameObject.AddComponent<MenuSwipe>().Initialize(TurnJourneyMap);
            int unlocked=NextJourneyStop(),markerStop=IsPinnedJourneyStop(selectedJourneyStop)?selectedJourneyStop:unlocked;Vector2[] places=MapPlaces(zone);
            var mapFrame=Rect("Map view",world,new Vector2(-.04f,.18f),new Vector2(1.04f,.770f));
            var mapSurface=Rect("Island map",mapFrame,Vector2.zero,Vector2.one);
            var crop=zone<3?new Rect(0,.18f,1,.69f):new Rect(0,.03f,1,.95f);
            var uvPoints=places.Select(p=>new Vector2(p.x,(p.y-.29f)/.51f)).ToArray();
            SoftSceneryGraphic scenery=null;
            if(map!=null){
                var fit=mapSurface.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=map.rect.width/(map.rect.height*crop.height);
                scenery=mapSurface.gameObject.AddComponent<SoftSceneryGraphic>();scenery.Initialize(map);scenery.SetMotion(this,crop);
                scenery.SetAvailability(uvPoints,Enumerable.Range(0,5).Select(i=>Enumerable.Range(JourneyStory.IslandFirst(zone,i),JourneyStory.IslandLevels(zone,i)).Any(IsJourneyStopOpen)).ToArray());scenery.SetSky(sky,uiCamera,zone==8);scenery.SetIslandSpacing(5*MenuUnit);
            }
            DrawMapAtmosphere(zone,world);
            if(zone!=8){
                var flight=Picture("Map glider",world,new Vector2(.2f,.66f),new Vector2(.2f,.66f),Resources.Load<Sprite>("CascadeArt/Journey/Glider"),true);
                flight.rectTransform.sizeDelta=new Vector2(38,23);flight.color=new Color(1,1,1,.8f);flight.gameObject.AddComponent<MapFlight>().Initialize(this);
            }
            var tapTargets=new List<RectTransform>();var tapIslands=new List<int>();
            int currentIsland=JourneyStory.Zone(markerStop)==zone?JourneyStory.IslandFor(markerStop):-1;
            LoadNavigationArt();
            for(int i=0;i<5;i++) {
                int island=i;
                var point=new Vector2(uvPoints[i].x,(uvPoints[i].y-crop.y)/crop.height);
                var islandRoot=Rect("Floating island "+i,mapSurface,Vector2.zero,Vector2.one);
                islandRoot.gameObject.AddComponent<MapIslandAnchor>().Initialize(scenery,uvPoints[i]);
                var b=Button(UiCopy.Raw("Island"+(i+1)),islandRoot,point,point,()=>OpenJourneyIsland(island));
                ((RectTransform)b.transform).sizeDelta=new Vector2(90*MenuUnit,84*MenuUnit);b.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
                var stops=IslandJourneyStops(zone,island);
                b.GetComponent<Image>().color=Color.clear;b.interactable=stops.Any(IsJourneyStopOpen);
                if(b.interactable){tapTargets.Add((RectTransform)b.transform);tapIslands.Add(island);}
                if(island!=currentIsland)continue;
                // World overlay is above drifting clouds, while its position follows the island.
                var progress=Picture("Island progress "+(island+1),world,Vector2.zero,Vector2.zero,enamelControls[0],false);
                progress.rectTransform.sizeDelta=new Vector2(66,28)*MenuUnit;
                // Face the open side of the screen, keeping both the badge and artwork apart.
                progress.gameObject.AddComponent<MapIslandProgressAnchor>().Initialize((RectTransform)b.transform,point.x<=.5f,28*MenuUnit);
                var progressText=CompletionText("Island completed levels "+(island+1),progress.transform,new Vector2(.10f,.10f),new Vector2(.90f,.90f),15,
                    UiCopy.Raw(stops.Count(stop=>sessionClears.Contains(Catalog.PrimaryLevels[stop].Definition.CampaignNodeId))+" / "+stops.Length),CelebrationType.EnamelLabel);
                progressText.margin=Vector4.zero;progressText.textWrappingMode=TextWrappingModes.NoWrap;progressText.fontSizeMin=13*MenuUnit;progressText.fontSizeMax=15*MenuUnit;
                navigationLightLabels.Add(progressText);progress.raycastTarget=progressText.raycastTarget=false;
                var hero=Picture("Mika on the path",islandRoot,point,point,Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
                hero.rectTransform.pivot=new Vector2(.5f,0);hero.rectTransform.sizeDelta=new Vector2(40,52)*MenuUnit;
                MapMikaHalo.Attach(this,hero);
                var heroTouch=Button(UiCopy.Raw(""),islandRoot,point+new Vector2(0,.045f),point+new Vector2(0,.045f),()=>OpenJourneyIsland(island));heroTouch.name="Browse Mika island";heroTouch.interactable=!IsPlanningDescent&&!IsDescending;
                ((RectTransform)heroTouch.transform).sizeDelta=new Vector2(64*MenuUnit,80*MenuUnit);heroTouch.GetComponent<Image>().color=Color.clear;
            }
            scenery?.SetFocus(currentIsland);
            if(tapTargets.Count>0){
                var hand=Picture("Island tap guide",world,Vector2.zero,Vector2.zero,Resources.Load<Sprite>("CascadeArt/TutorialHand"),true);
                hand.rectTransform.pivot=new Vector2(.60f,.865f);hand.rectTransform.sizeDelta=new Vector2(52,64)*MenuUnit;hand.raycastTarget=false;
                hand.gameObject.AddComponent<MapIslandTapCue>().Initialize(this,tapTargets.ToArray(),tapIslands.ToArray(),scenery,currentIsland);
            }
            DrawJourneyChrome(zone,first);
        }
        void FitJourneyFooter()
        {
            if(CurrentScreen!=GameScreen.JourneyMap||journeyFooter==null||journeyFooterButtons==null)return;
            FitJourneyModeTabs();
            journeyFooter.pivot=new Vector2(.5f,0);journeyFooter.sizeDelta=new Vector2(0,106*MenuUnit);journeyFooter.anchoredPosition=Vector2.zero;
            foreach(var control in journeyFooterButtons)if(control!=null){
                var rect=(RectTransform)control.transform;rect.sizeDelta=Vector2.zero;Canvas.ForceUpdateCanvases();
                if(rect.rect.height<44*MenuUnit)rect.sizeDelta=new Vector2(0,44*MenuUnit-rect.rect.height);
                if(rect.rect.width<44*MenuUnit)rect.sizeDelta+=Vector2.right*(44*MenuUnit-rect.rect.width);
            }
        }
        public void TurnJourneyMap(int direction)
        {
            if(CurrentScreen!=GameScreen.JourneyMap||ConversationOpen||IsPlanningDescent||IsDescending)return;
            int next=OpenedJourneyWorld(direction);if(next<0)return;
            islandBrowseIndex=-1;mapPage=next;
            int first=JourneyStory.WorldFirst(next),last=first+JourneyStory.WorldLevels(next);
            selectedJourneyStop=Enumerable.Range(first,last-first).Where(IsJourneyStopOpen)
                .OrderBy(n=>sessionClears.Contains(Catalog.PrimaryLevels[n].Definition.CampaignNodeId)?1:0).First();
            feedback.Sound(ToyCue.Select);ShowSelectedJourney();
        }
        void DrawJourneyEnd()
        {
            machine.SetZone(8);
            MenuTitle("The sky shines again!","");
            var hero=Picture("Mika at home",menuRoot,new Vector2(.24f,.35f),new Vector2(.76f,.75f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            hero.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,3),0,1);
            ReadingStrip("Journey ending",new Vector2(.055f,.245f),new Vector2(.945f,.38f),"Mika brought the light home. Together, her friends lit the way beyond it.",18,true);
            MenuButton("Look at our journey",.13f,ShowJourney,true);MenuButton("My journal",.025f,ShowJournal);
        }
        void DrawMapAtmosphere(int zone,Transform world)
        {
            if(zone==8){
                var sky=Rect("Quiet constellation light",world,Vector2.zero,Vector2.one);
                sky.SetAsFirstSibling();sky.gameObject.AddComponent<NightSkyGraphic>().Initialize(this);
            }
            Rect("Map weather",world,new Vector2(.02f,.29f),new Vector2(.98f,.85f)).gameObject.AddComponent<WorldWeatherGraphic>().InitializeRegion(this,zone);
            var sprite=Resources.Load<Sprite>("CascadeArt/Journey/Cloud");
            if(sprite!=null)for(int i=0;i<2;i++) {
                float y=i==0?.785f:.315f;var cloud=Picture("Map cloud "+i,world,new Vector2(i==0?-.10f:.62f,y),new Vector2(i==0?.35f:1.10f,y+.075f),sprite,true);
                cloud.color=zone==8?new Color(.45f,.65f,1,.28f):new Color(1,1,1,.65f);cloud.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(i==0?48:-40,4),i*2);
            }
        }
        Vector2[] MapPlaces(int zone)
        {
            if(zone==8)return LiftMapPlaces(new[]{new Vector2(.30f,.390f),new Vector2(.64f,.501f),new Vector2(.29f,.572f),new Vector2(.64f,.658f),new Vector2(.79f,.741f)});
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
            var opener=StoryConversations.IslandOpening(JourneyStory.Zone(index),JourneyStory.IslandFor(index));
            if(!RemembersStory(opener.Receipt)){OpenConversation(opener,"Continue",()=>BeginJourney(index));return;}
            var aside=StoryConversations.HasLevelAside(index)?StoryConversations.LevelAside(index):null;
            if(aside!=null&&!RemembersStory(aside.Receipt)){OpenConversation(aside,"Continue",()=>BeginJourney(index));return;}
            if(!RemembersStory(JourneyStory.PageFor(JourneyStory.WorldFirst(JourneyStory.Zone(index)))))OpenJourneyStory(index,true);else EnterJourneyPuzzle();
        }
        void OpenJourneyStory(int index,bool enterAfter)
        {
            if(ConversationOpen||IsPlanningDescent||IsDescending||!IsJourneyStopOpen(index))return;
            selectedJourneyStop=index;machine.SetZone(JourneyStory.Zone(index));
            if(enterAfter){SetScreen(GameScreen.JourneyMap);OpenConversation(StoryConversations.BeforeLevel(index),"Continue",EnterJourneyPuzzle);}
            else {
                pendingJourney=null;
                int receipt=StoryConversations.BeforeLevel(index).Receipt;
                OpenConversation(StoryConversations.Visit(index,receipt,RemembersStory(receipt)),CanResumeSelectedJourney(index)?new UiCopy("Continue"):new UiCopy("Level {0}",DisplayLevelNumber(Catalog.PrimaryLevels[index].Definition.CampaignNodeId)),TravelSelectedJourney,()=>{});
            }
        }
        void EnterJourneyPuzzle()
        {
            if(pendingJourney==null){ShowJourney();return;}
            var content=pendingJourney;pendingJourney=null;
            StartCampaignLevel(content.Definition,content.Locale);
            ShowGame();
        }
        public void StartCampaignLevel(LevelDefinition source,string locale="en")
        {
            if(PersonalMissionActive){RestorePersonalCampaign(false);if(HasSaveError)return;}
            if(!DescentRun.CanDescend(source))throw new ArgumentException("A campaign puzzle is required.",nameof(source));
            StartDescentLevel(source,locale);
            descentTravel=true;
            journeyActive=true;PersistProgress();
        }
        void DrawDialogue(){OpenConversation(StoryConversations.Visit(selectedJourneyStop),"Back to the map",ShowJourney);}
        void Panel(RectTransform parent,Color tint)
        {var image=Background(parent,tint,true);image.sprite=art.panel;image.type=Image.Type.Sliced;}
        Image Picture(string name,Transform parent,Vector2 min,Vector2 max,Sprite sprite,bool fit)
        {var image=Rect(name,parent,min,max).gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=fit;image.raycastTarget=false;
            if(sprite!=null&&(sprite.texture.name=="Mika"||sprite.texture.name=="MikaThinking"))AttachMikaCosmetic(image,null);
            return image;}
    }
}
