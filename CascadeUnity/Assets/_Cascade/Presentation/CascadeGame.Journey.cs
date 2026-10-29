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
            var sequence=PreferredLevels();string next=CampaignProgress.Next(preferredNodeIds,sessionClears,preferredTeachingNodes);
            var content=sequence.FirstOrDefault(c=>c.Definition.CampaignNodeId==next)??sequence.LastOrDefault();
            return content==null?0:Math.Max(0,PrimaryLevelIndex(content.Definition.CampaignNodeId));
        }
        int PrimaryLevelIndex(string nodeId)
        {
            for(int i=0;i<Catalog.PrimaryLevels.Count;i++)if(Catalog.PrimaryLevels[i].Definition.CampaignNodeId==nodeId)return i;
            return -1;
        }
        public void ShowJourney()
        {
            SelectJourneyDestination();
            ShowSelectedJourney();
        }
        void SelectJourneyDestination()
        {
            islandBrowseIndex=-1;selectedJourneyStop=HasResumableJourneyAttempt?Math.Max(0,Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node)):NextJourneyStop();mapPage=JourneyStory.Zone(selectedJourneyStop);
        }
        void ShowSelectedJourney()
        {
            if(TryWorldCompletion())return;
            if(TryParallelArrival())return;
            if(TryWorldComic(mapPage,ShowSelectedJourney))return;
            SetScreen(GameScreen.JourneyMap);TryIslandConversation();if(!ConversationOpen)TryJourneyOpening();
        }
        public void ShowLessons()=>ShowJourney();
        void DrawJourney()
        {
            if(islandBrowseIndex>=0){DrawJourneyIsland();return;}
            DrawReferenceMap();
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
            Picture("Journey end Mika landing",menuRoot,new Vector2(.14f,.375f),new Vector2(.86f,.425f),ReferenceShopArt("PreviewPlatform"),true);
            Picture("Mika at home",menuRoot,new Vector2(.24f,.35f),new Vector2(.76f,.75f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            ReadingStrip("Journey ending",new Vector2(.055f,.19f),new Vector2(.945f,.37f),ReferenceCopy(
                "Rin takes the first watch. Ada keeps the log; Felix joins the repair crew. The north has lamps at last.",
                "Первая вахта Рин. Ада ведёт журнал, Феликс чинит огни под присмотром Вальтера. На севере больше не темно.",
                "Перша варта Рін. Ада веде журнал, Фелікс лагодить вогні під наглядом Вальтера. На півночі більше не темно."),16,true);
            MenuButton("Look at our journey",.13f,ShowJourney,true);MenuButton("My things",.025f,ShowTravelBag);
        }
        void BeginJourney(int index)
        {
            if(index<0||index>=Catalog.PrimaryLevels.Count||!IsJourneyStopOpen(index))return;
            if(TryWorldComic(JourneyStory.Zone(index),()=>BeginJourney(index)))return;
            selectedJourneyStop=index;var primary=Catalog.PrimaryLevels[index];
            pendingJourney=primary;
            pendingJourney=JourneySequence().FirstOrDefault(c=>c.Definition.CampaignNodeId==primary.Definition.CampaignNodeId);
            var opener=StoryConversations.IslandOpening(JourneyStory.Zone(index),JourneyStory.IslandFor(index));
            if(!RemembersStory(opener.Receipt)){OpenConversation(opener,"Continue",()=>BeginJourney(index));return;}
            var aside=StoryConversations.HasLevelAside(index)?StoryConversations.LevelAside(index):null;
            if(aside!=null&&!RemembersStory(aside.Receipt)){OpenConversation(aside,"Continue",()=>BeginJourney(index));return;}
            // The island opening already supplies this scene; the legacy page would repeat it.
            EnterJourneyPuzzle();
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
            StartDescentLevel(source,locale,stageMode:PreferredMode);
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
