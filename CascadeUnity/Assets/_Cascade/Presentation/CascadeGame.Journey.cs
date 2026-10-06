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
            for(int i=0;i<Catalog.PrimaryLevels.Count;i++)if(!(!storyRoute&&PreferredMode==PuzzleMode.Words&&i==25)&&!sessionClears.Contains(Catalog.PrimaryLevels[i].Definition.CampaignNodeId))return i;
            return Catalog.PrimaryLevels.Count-1;
        }
        public void ShowJourney()
        {
            selectedJourneyStop=journeyActive&&(Interaction.Attempt.Current.Status==PuzzleStatus.Playing||IsCheckpoint)?Math.Max(0,Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node)):NextJourneyStop();mapPage=JourneyStory.PageFor(selectedJourneyStop);SetScreen(GameScreen.JourneyMap);
        }
        public void ShowLessons(){chapterPage=0;SetScreen(GameScreen.Lessons);}
        void DrawJourney()
        {
            int first=JourneyStory.FirstStop(mapPage),zone=JourneyStory.Zone(first);machine.SetZone(zone);
            var map=Resources.Load<Sprite>("CascadeArt/"+(zone<3?"Journey/":"Woodland/")+JourneyStory.MapAsset(zone));
            var sky=Resources.Load<Texture2D>("CascadeArt/Navigation/MapSky");machine.LivingWorld.SetMapSky(sky);
            var world=Rect("Parallax island world",menuRoot,Vector2.zero,Vector2.one);Background(world,Color.clear,true);
            world.gameObject.AddComponent<MapParallax>().Initialize(this);
            world.gameObject.AddComponent<MenuSwipe>().Initialize(TurnJourneyMap);
            int unlocked=NextJourneyStop();Vector2[] places=MapPlaces(zone);
            if(map!=null){var scenery=Rect("Island map",world,new Vector2(0,.19f),new Vector2(1,.89f)).gameObject.AddComponent<SoftSceneryGraphic>();scenery.Initialize(map);scenery.SetAvailability(places.Select(p=>new Vector2(p.x,(p.y-.19f)/.70f)).ToArray(),unlocked-first);scenery.SetSky(sky,uiCamera);}
            DrawMapAtmosphere(zone,world);
            for(int i=0;i<JourneyStory.StopsOnPage(mapPage)&&first+i<Catalog.PrimaryLevels.Count;i++) {
                int index=first+i;var point=places[i];
                var b=Button(UiCopy.Raw("Stop"+(index+1)),world,point,point,()=>{selectedJourneyStop=index;DrawMenu();});
                ((RectTransform)b.transform).sizeDelta=new Vector2(76,72);b.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
                b.GetComponent<Image>().color=Color.clear;b.interactable=index<=unlocked;
                if(index!=unlocked)continue;
                var marker=Rect("You are here marker",world,point+new Vector2(0,.128f),point+new Vector2(0,.128f));marker.sizeDelta=new Vector2(116,35);Panel(marker,MenuPaper);
                var caption=Text("You are here",marker,new Vector2(.05f,.12f),new Vector2(.95f,.96f),16,"You are here");caption.font=art.titleFont;caption.enableAutoSizing=true;caption.fontSizeMin=14;caption.fontSizeMax=16;
                var arrow=Rect("Here arrow",marker,new Vector2(.45f,-.28f),new Vector2(.55f,.08f));arrow.gameObject.AddComponent<ShapeFaceGraphic>().SetShape(art.shapes[0],Ink);arrow.localRotation=Quaternion.Euler(0,0,180);
                var hero=Picture("Mika on the path",world,point+new Vector2(-.050f,-.002f),point+new Vector2(.050f,.085f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
                var heroTouch=Button(UiCopy.Raw(""),world,point+new Vector2(0,.045f),point+new Vector2(0,.045f),()=>OpenJourneyStory(index,false));heroTouch.name="TalktoMika";
                ((RectTransform)heroTouch.transform).sizeDelta=new Vector2(64*MenuUnit,80*MenuUnit);heroTouch.GetComponent<Image>().color=Color.clear;
                // Her feet stay on the island; scenery, hero and hit target share parallax.
                if(Rewards.EquippedGift!=null)Picture("Mika's keepsake",hero.transform,new Vector2(.69f,.30f),new Vector2(.98f,.56f),Resources.Load<Sprite>("CascadeArt/Workshop/"+GiftAssets[Array.IndexOf(JourneyRewards.Gifts,Rewards.EquippedGift)]),true);
            }
            var top=Rect("Map heading",menuRoot,new Vector2(.04f,.875f),new Vector2(.96f,.98f));Panel(top,MenuPaper);
            var back=DirectionButton("Back",top,new Vector2(.015f,.12f),new Vector2(.18f,.90f),false,ShowPlayChoice);back.GetComponent<Image>().color=Color.clear;
            var title=Text("Region",top,new Vector2(.20f,.43f),new Vector2(.95f,.95f),26,JourneyStory.ZoneName(zone));title.font=art.titleFont;title.alignment=TextAlignmentOptions.MidlineLeft;title.enableAutoSizing=true;title.fontSizeMin=20;title.fontSizeMax=26;
            var goal=Text("Journey goal",top,new Vector2(.20f,.07f),new Vector2(.95f,.44f),16,"Goal: Great Beacon");goal.alignment=TextAlignmentOptions.MidlineLeft;goal.enableAutoSizing=true;goal.fontSizeMin=14;goal.fontSizeMax=17;
            var bottom=Rect("Journey task",menuRoot,new Vector2(.04f,.025f),new Vector2(.96f,.287f));Panel(bottom,MenuPaper);
            bool future=selectedJourneyStop>unlocked;
            var task=Text("Next task",bottom,new Vector2(.06f,.775f),new Vector2(.94f,.975f),22,future?new UiCopy("Stops to this island: {0}",selectedJourneyStop-unlocked):JourneyStory.Task(mapPage));task.font=art.titleFont;task.enableAutoSizing=true;task.fontSizeMin=18;task.fontSizeMax=23;
            bool resume=journeyActive&&(Interaction.Attempt.Current.Status==PuzzleStatus.Playing||descent!=null&&Interaction.Attempt.Current.Status!=PuzzleStatus.Won||IsCheckpoint)
                &&(Interaction.Attempt.RouteLength>0||descent?.StageIndex>0)&&Catalog.PrimaryLevels[selectedJourneyStop].Definition.CampaignNodeId==node;
            var go=Button("Travel",bottom,new Vector2(.05f,.405f),new Vector2(.95f,.715f),()=>{if(future)ShowJourney();else if(resume)ShowGame();else BeginJourney(selectedJourneyStop);});((RectTransform)go.transform).sizeDelta=new Vector2(0,8*MenuUnit);
            go.GetComponentInChildren<TextMeshProUGUI>().SetCopy(future?"Back to my path":resume?"Continue":new UiCopy("Go to level {0}",selectedJourneyStop+1));go.GetComponent<Image>().sprite=enamelControls[1];go.GetComponent<Image>().color=Color.white;
            var talk=Button("Story",bottom,new Vector2(.05f,.035f),new Vector2(.95f,.285f),()=>OpenJourneyStory(selectedJourneyStop,false));((RectTransform)talk.transform).sizeDelta=new Vector2(0,16*MenuUnit);talk.interactable=!future;
            talk.GetComponentInChildren<TextMeshProUGUI>().SetCopy(new UiCopy("Talk to {0}",new UiCopy(JourneyStory.Speaker(zone))));talk.GetComponent<Image>().color=new Color(.84f,.91f,.87f);
        }
        public void TurnJourneyMap(int direction)
        {
            int next=Mathf.Clamp(mapPage+direction,0,JourneyStory.PageCount-1);if(next==mapPage)return;
            mapPage=next;int first=JourneyStory.FirstStop(mapPage);selectedJourneyStop=Mathf.Clamp(NextJourneyStop(),first,first+JourneyStory.StopsOnPage(mapPage)-1);
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
            if(zone<3)return new[]{new Vector2(.25f,.397f),new Vector2(.59f,.469f),new Vector2(.40f,.545f),new Vector2(.65f,.608f),new Vector2(.81f,.687f)};
            Vector2[][] plazas={
                new[]{new Vector2(.316f,.763f),new Vector2(.667f,.612f),new Vector2(.326f,.467f),new Vector2(.612f,.329f),new Vector2(.810f,.191f)},
                new[]{new Vector2(.366f,.786f),new Vector2(.666f,.620f),new Vector2(.321f,.458f),new Vector2(.567f,.329f),new Vector2(.793f,.166f)},
                new[]{new Vector2(.280f,.820f),new Vector2(.681f,.694f),new Vector2(.280f,.533f),new Vector2(.615f,.371f),new Vector2(.805f,.194f)},
                new[]{new Vector2(.278f,.791f),new Vector2(.690f,.627f),new Vector2(.330f,.510f),new Vector2(.640f,.358f),new Vector2(.814f,.198f)},
                new[]{new Vector2(.325f,.759f),new Vector2(.661f,.607f),new Vector2(.344f,.479f),new Vector2(.630f,.344f),new Vector2(.796f,.196f)}};
            var result=plazas[zone-3];for(int i=0;i<result.Length;i++)result[i]=new Vector2(result[i].x,.19f+(1-result[i].y)*.70f);return result;
        }
        void BeginJourney(int index)
        {
            selectedJourneyStop=index;var primary=Catalog.PrimaryLevels[index];
            pendingJourney=primary;
            if((storyRoute?index>=3&&index%3!=0:PreferredMode!=PuzzleMode.Shapes) && Catalog.TryGetLocalizedBinding(primary.Definition.CampaignNodeId,Preferences.PuzzleLocale,out var binding))pendingJourney=binding;
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
            int zone=JourneyStory.Zone(selectedJourneyStop);
            string speakerName=dialogueLine==0?JourneyStory.Speaker(zone):"Mika";
            var portrait=Resources.Load<Sprite>("CascadeArt/Journey/"+speakerName);
            if(portrait!=null)Picture("Speaking friend",menuRoot,new Vector2(.20f,.44f),new Vector2(.80f,.91f),portrait,true).gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,2),0,.35f);
            var panel=Rect("Dialogue frame",menuRoot,new Vector2(.04f,.08f),new Vector2(.96f,.46f));Panel(panel,Paper);
            var edge=Rect("Brass corner",panel,new Vector2(.025f,.88f),new Vector2(.975f,.90f));Background(edge,new Color(.92f,.70f,.31f),false);
            var speaker=Text("Speaker",panel,new Vector2(.07f,.71f),new Vector2(.93f,.88f),27,speakerName);speaker.font=art.titleFont;speaker.alignment=TextAlignmentOptions.MidlineLeft;
            var line=Text("Dialogue line",panel,new Vector2(.07f,.27f),new Vector2(.93f,.72f),22,JourneyStory.Lines(JourneyStory.PageFor(selectedJourneyStop))[dialogueLine]);line.alignment=TextAlignmentOptions.TopLeft;
            var next=Button("Continue",panel,new Vector2(.52f,.04f),new Vector2(.93f,.24f),AdvanceDialogue);next.GetComponent<Image>().sprite=enamelControls[1];next.GetComponent<Image>().color=Color.white;
            Button("Skip story",panel,new Vector2(.07f,.04f),new Vector2(.48f,.24f),()=>{heardStory.Add(JourneyStory.PageFor(selectedJourneyStop));PersistProgress();EnterJourneyPuzzle();}).GetComponentInChildren<TextMeshProUGUI>().fontSize=16;
        }
        void Panel(RectTransform parent,Color tint)
        {var image=Background(parent,tint,true);image.sprite=art.panel;image.type=Image.Type.Sliced;}
        Image Picture(string name,Transform parent,Vector2 min,Vector2 max,Sprite sprite,bool fit)
        {var image=Rect(name,parent,min,max).gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=fit;image.raycastTarget=false;return image;}
    }
}
