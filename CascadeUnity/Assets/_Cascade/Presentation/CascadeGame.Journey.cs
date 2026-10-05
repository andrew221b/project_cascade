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
            selectedJourneyStop=journeyActive&&Interaction.Attempt.Current.Status==PuzzleStatus.Playing?Math.Max(0,Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node)):NextJourneyStop();mapPage=JourneyStory.PageFor(selectedJourneyStop);SetScreen(GameScreen.Home);
        }
        public void ShowLessons(){SetScreen(GameScreen.Lessons);}
        void DrawJourney()
        {
            menuWash.color=Color.clear;
            int first=JourneyStory.FirstStop(mapPage),zone=JourneyStory.Zone(first);machine.SetZone(zone);
            var map=Resources.Load<Sprite>("CascadeArt/"+(zone<3?"Journey/":"Woodland/")+JourneyStory.MapAsset(zone));
            if(map!=null){var scenery=Rect("Island map",menuRoot,new Vector2(0,.19f),new Vector2(1,.89f)).gameObject.AddComponent<SoftSceneryGraphic>();scenery.Initialize(map);scenery.SetAvailability(MapPlaces(zone).Select(p=>new Vector2(p.x,(p.y-.19f)/.70f)).ToArray(),NextJourneyStop()-first);}
            DrawMapAtmosphere(zone);
            var top=Rect("Map heading",menuRoot,new Vector2(.04f,.86f),new Vector2(.96f,.98f));Panel(top,Paper);
            var title=Text("Region",top,new Vector2(.05f,.50f),new Vector2(.75f,.98f),27,JourneyStory.ZoneName(zone));title.font=art.titleFont;title.alignment=TextAlignmentOptions.MidlineLeft;title.enableAutoSizing=true;title.fontSizeMin=21;title.fontSizeMax=27;
            var count=Text("Restored lights",top,new Vector2(.05f,.04f),new Vector2(.60f,.46f),15,new UiCopy("{0} / {1} lights restored",JourneyClearCount,Catalog.PrimaryLevels.Count));count.alignment=TextAlignmentOptions.MidlineLeft;
            Button("Settings",top,new Vector2(.76f,.52f),new Vector2(.98f,.96f),ShowTravelBag).GetComponentInChildren<TextMeshProUGUI>().SetCopy("Menu");
            Vector2[] places=MapPlaces(zone);
            int unlocked=NextJourneyStop();
            for(int i=0;i<JourneyStory.StopsOnPage(mapPage)&&first+i<Catalog.PrimaryLevels.Count;i++) {
                int index=first+i;var content=Catalog.PrimaryLevels[index];bool done=sessionClears.Contains(content.Definition.CampaignNodeId);
                var point=places[i];
                var b=Button(UiCopy.Raw("Stop"+(index+1)),menuRoot,point,point,()=>{selectedJourneyStop=index;DrawMenu();});
                var rect=(RectTransform)b.transform;rect.sizeDelta=new Vector2(76,72);b.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
                b.GetComponent<Image>().color=Color.clear;b.interactable=index<=unlocked;
                if(index==unlocked) {
                    var marker=Rect("You are here marker",menuRoot,point+new Vector2(0,.10f),point+new Vector2(0,.10f));marker.sizeDelta=new Vector2(124,44);Panel(marker,new Color(1,.91f,.66f,.96f));
                    var caption=Text("You are here",marker,new Vector2(.05f,.42f),new Vector2(.95f,.98f),15,"You are here");caption.font=art.titleFont;
                    var arrow=Rect("Here arrow",marker,new Vector2(.44f,-.22f),new Vector2(.56f,.34f));arrow.gameObject.AddComponent<ShapeFaceGraphic>().SetShape(art.shapes[0],Ink);arrow.localRotation=Quaternion.Euler(0,0,180);
                    marker.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,2),0,.7f);
                    var portrait=Resources.Load<Sprite>("CascadeArt/Journey/Mika");
                    if(portrait!=null) {
                        var m=Picture("Mika on the path",menuRoot,point+new Vector2(-.17f,-.04f),point+new Vector2(-.04f,.06f),portrait,true);m.raycastTarget=false;m.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,3),0,.9f);
                        if(Rewards.EquippedGift!=null)Picture("Mika's keepsake",menuRoot,point+new Vector2(-.10f,.005f),point+new Vector2(-.04f,.045f),
                            Resources.Load<Sprite>("CascadeArt/Workshop/"+GiftAssets[Array.IndexOf(JourneyRewards.Gifts,Rewards.EquippedGift)]),true);
                    }
                }
            }
            var bottom=Rect("Journey task",menuRoot,new Vector2(.04f,.02f),new Vector2(.96f,.28f));Panel(bottom,Paper);
            bool future=selectedJourneyStop>unlocked;
            var task=Text("Next task",bottom,new Vector2(.06f,.70f),new Vector2(.94f,.97f),21,future?new UiCopy("Stops to this island: {0}",selectedJourneyStop-unlocked):JourneyStory.Task(mapPage));task.font=art.titleFont;
            bool resume=journeyActive&&(Interaction.Attempt.Current.Status==PuzzleStatus.Playing||descent!=null&&Interaction.Attempt.Current.Status!=PuzzleStatus.Won||IsCheckpoint)
                &&(Interaction.Attempt.RouteLength>0||descent?.StageIndex>0)&&Catalog.PrimaryLevels[selectedJourneyStop].Definition.CampaignNodeId==node;
            var go=Button("Travel",bottom,new Vector2(.05f,.40f),new Vector2(.95f,.68f),()=>{if(future)ShowJourney();else if(resume)ShowGame();else BeginJourney(selectedJourneyStop);});
            go.GetComponentInChildren<TextMeshProUGUI>().SetCopy(future?"Back to my path":resume?"Continue":sessionClears.Contains(Catalog.PrimaryLevels[selectedJourneyStop].Definition.CampaignNodeId)?"Play again":"Let's go");go.GetComponent<Image>().color=new Color(.98f,.74f,.32f);
            Button("Lessons",bottom,new Vector2(.05f,.05f),new Vector2(.49f,.33f),ShowLessons);
            Button("Story",bottom,new Vector2(.51f,.05f),new Vector2(.95f,.33f),()=>OpenJourneyStory(selectedJourneyStop,false)).interactable=selectedJourneyStop<=unlocked;
            if(mapPage>0)MapTurn("Earlier",top,new Vector2(.63f,.25f),()=>{mapPage--;selectedJourneyStop=JourneyStory.FirstStop(mapPage);DrawMenu();});
            if(mapPage+1<JourneyStory.PageCount)MapTurn("Later",top,new Vector2(.89f,.25f),()=>{mapPage++;int pageFirst=JourneyStory.FirstStop(mapPage);selectedJourneyStop=Math.Min(NextJourneyStop(),pageFirst+JourneyStory.StopsOnPage(mapPage)-1);if(selectedJourneyStop<pageFirst)selectedJourneyStop=pageFirst;DrawMenu();});
            if(!future&&PreferredMode==PuzzleMode.Words&&selectedJourneyStop==25) {task.SetCopy("A shapes lesson. You can skip it.");go.GetComponentInChildren<TextMeshProUGUI>().SetCopy("Play shapes");}
        }
        void DrawJourneyEnd()
        {
            machine.SetZone(7);menuWash.color=new Color(1,.96f,.84f,.92f);
            MenuTitle("The beacon is lit!","Our little light is home.");
            var hero=Picture("Mika at home",menuRoot,new Vector2(.24f,.35f),new Vector2(.76f,.75f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            hero.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,3),0,1);
            Text("Journey ending",menuRoot,new Vector2(.10f,.24f),new Vector2(.90f,.36f),21,"We found the way together.");
            MenuButton("Look at our journey",.13f,ShowJourney,true);MenuButton("My journal",.025f,ShowJournal);
        }
        void DrawMapAtmosphere(int zone)
        {
            Rect("Map weather",menuRoot,new Vector2(.02f,.29f),new Vector2(.98f,.85f)).gameObject.AddComponent<WorldWeatherGraphic>().InitializeRegion(this,zone);
            var sprite=Resources.Load<Sprite>("CascadeArt/Journey/Cloud");
            if(sprite!=null)for(int i=0;i<2;i++) {
                float y=i==0?.785f:.315f;var cloud=Picture("Map cloud "+i,menuRoot,new Vector2(i==0?-.10f:.62f,y),new Vector2(i==0?.35f:1.10f,y+.075f),sprite,true);
                cloud.color=new Color(1,1,1,.65f);cloud.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(i==0?22:-18,2),i*2);
            }
        }
        Vector2[] MapPlaces(int zone)
        {
            if(zone<3)return new[]{new Vector2(.25f,.397f),new Vector2(.59f,.469f),new Vector2(.40f,.545f),new Vector2(.65f,.608f),new Vector2(.81f,.687f)};
            Vector2[][] plazas={
                new[]{new Vector2(.316f,.763f),new Vector2(.667f,.612f),new Vector2(.326f,.467f),new Vector2(.612f,.329f),new Vector2(.810f,.191f)},
                new[]{new Vector2(.366f,.786f),new Vector2(.666f,.620f),new Vector2(.321f,.458f),new Vector2(.567f,.329f),new Vector2(.793f,.166f)},
                new[]{new Vector2(.280f,.820f),new Vector2(.681f,.694f),new Vector2(.280f,.533f),new Vector2(.615f,.371f),new Vector2(.805f,.194f)},
                new[]{new Vector2(.278f,.791f),new Vector2(.690f,.627f),new Vector2(.330f,.510f),new Vector2(.640f,.358f),new Vector2(.814f,.198f)},
                new[]{new Vector2(.325f,.759f),new Vector2(.661f,.607f),new Vector2(.344f,.479f),new Vector2(.630f,.344f),new Vector2(.796f,.196f)}};
            var result=plazas[zone-3];for(int i=0;i<result.Length;i++)result[i]=new Vector2(result[i].x,.19f+(1-result[i].y)*.70f);return result;
        }
        void MapTurn(string name,Transform parent,Vector2 point,Action action)
        {
            var button=DirectionButton(name,parent,point,point,name=="Later",action);
            ((RectTransform)button.transform).sizeDelta=new Vector2(64,44);
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
            if(index==JourneyStory.FirstStop(JourneyStory.PageFor(index))&&!heardStory.Contains(JourneyStory.PageFor(index)))OpenJourneyStory(index,true);else EnterJourneyPuzzle();
        }
        void OpenJourneyStory(int index,bool enterAfter)
        {
            selectedJourneyStop=index;dialogueLine=0;if(!enterAfter)pendingJourney=null;machine.SetZone(JourneyStory.Zone(index));SetScreen(GameScreen.Dialogue);
        }
        void EnterJourneyPuzzle()
        {
            if(pendingJourney==null){ShowJourney();return;}
            var content=pendingJourney;pendingJourney=null;
            if(sessionClears.Contains(content.Definition.CampaignNodeId))LoadLevel(content.Definition,content.Locale);
            else if(DeliveryDirector.CanDirect(content.Definition))StartDescentLevel(content.Definition,content.Locale,profile.Director.Choose(content.Definition));
            else LoadLevel(content.Definition,content.Locale);
            journeyActive=true;
            PersistProgress();
            levelLabel.SetCopy(JourneyStory.Task(JourneyStory.PageFor(selectedJourneyStop)));levelLabel.fontSize=21;
            ShowGame();
        }
        void AdvanceDialogue()
        {
            if(++dialogueLine<JourneyStory.Lines(JourneyStory.PageFor(selectedJourneyStop)).Length){DrawMenu();return;}
            heardStory.Add(JourneyStory.PageFor(selectedJourneyStop));PersistProgress();EnterJourneyPuzzle();
        }
        void DrawDialogue()
        {
            menuWash.color=new Color(1,.96f,.84f,.13f);int zone=JourneyStory.Zone(selectedJourneyStop);
            var portrait=Resources.Load<Sprite>("CascadeArt/Journey/"+JourneyStory.Portrait(zone));
            if(portrait!=null)Picture("Speaking friend",menuRoot,new Vector2(.12f,.39f),new Vector2(.88f,.92f),portrait,true);
            var panel=Rect("Dialogue frame",menuRoot,new Vector2(.04f,.08f),new Vector2(.96f,.46f));Panel(panel,Paper);
            var edge=Rect("Brass corner",panel,new Vector2(.025f,.88f),new Vector2(.975f,.90f));Background(edge,new Color(.92f,.70f,.31f),false);
            var speaker=Text("Speaker",panel,new Vector2(.07f,.71f),new Vector2(.93f,.88f),27,JourneyStory.Speaker(zone));speaker.font=art.titleFont;speaker.alignment=TextAlignmentOptions.MidlineLeft;
            var line=Text("Dialogue line",panel,new Vector2(.07f,.27f),new Vector2(.93f,.72f),22,JourneyStory.Lines(JourneyStory.PageFor(selectedJourneyStop))[dialogueLine]);line.alignment=TextAlignmentOptions.TopLeft;
            Button("Continue",panel,new Vector2(.52f,.04f),new Vector2(.93f,.24f),AdvanceDialogue).GetComponent<Image>().color=new Color(.98f,.74f,.32f);
            Button("Skip story",panel,new Vector2(.07f,.04f),new Vector2(.48f,.24f),()=>{heardStory.Add(JourneyStory.PageFor(selectedJourneyStop));PersistProgress();EnterJourneyPuzzle();}).GetComponentInChildren<TextMeshProUGUI>().fontSize=16;
        }
        void Panel(RectTransform parent,Color tint)
        {var image=Background(parent,tint,true);image.sprite=art.panel;image.type=Image.Type.Sliced;}
        Image Picture(string name,Transform parent,Vector2 min,Vector2 max,Sprite sprite,bool fit)
        {var image=Rect(name,parent,min,max).gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=fit;image.raycastTarget=false;return image;}
    }
}
