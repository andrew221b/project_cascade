using System;
using Cascade.Content;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int worldComic=-1,worldComicPage;
        Action worldComicDone;
        readonly Sprite[][] worldComicPanels=new Sprite[3][];
        public int ComicWorld=>worldComic;
        public int ComicPage=>worldComicPage;
        bool TryWorldComic(int world,Action done)
        {
            if(profile.Completion!=null&&!profile.Completion.Acknowledged||heardStory.Contains(WorldPrologues.ForWorld(world).Receipt))return false;
            OpenWorldComic(world,done);return true;
        }
        public void OpenWorldComic(int world,Action done=null)
        {
            CancelConversation();worldComic=Mathf.Clamp(world,0,JourneyStory.WorldCount-1);
            worldComicPage=0;worldComicDone=done??ShowJourney;machine.SetZone(worldComic);
            SetScreen(GameScreen.Prologue);
        }
        void AdvanceWorldComic()
        {
            if(++worldComicPage<3){DrawMenu();menuTransition.Arrive();return;}
            FinishWorldComic();
        }
        public void FinishWorldComic()
        {
            if(worldComic<0)return;
            heardStory.Add(WorldPrologues.ForWorld(worldComic).Receipt);if(worldComic==0)heardStory.Add(PrologueReceipt);PersistProgress();
            if(HasSaveError)return;
            var done=worldComicDone;worldComic=-1;worldComicDone=null;done?.Invoke();
        }
        void DrawWorldComic()
        {
            var story=WorldPrologues.ForWorld(worldComic);var page=story.Pages[worldComicPage];
            int sheet=worldComic/3;
            if(worldComicPanels[sheet]==null)worldComicPanels[sheet]=NavigationSlices("Navigation/WorldComic"+sheet,3,3);
            NavigationHeader(JourneyStory.ZoneName(worldComic),FinishWorldComic);
            var title=menuRoot.Find("Navigation heading/Page heading").GetComponent<TextMeshProUGUI>();
            title.fontSizeMin=16*MenuUnit;title.fontSizeMax=26*MenuUnit;
            title.rectTransform.anchorMax=new Vector2(.945f,.92f);title.margin=new Vector4(0,0,4*MenuUnit,0);
            var frame=Rect("World comic illustration",menuRoot,new Vector2(.095f,.47f),new Vector2(.905f,.865f));
            Picture("World comic scene",frame,Vector2.zero,Vector2.one,worldComicPanels[sheet][worldComic%3*3+worldComicPage],true);
            frame.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(1.4f,1),worldComicPage,.13f);
            var heading=CompletionText("World comic caption",menuRoot,new Vector2(.075f,.409f),new Vector2(.925f,.475f),22,page.Caption);
            heading.enableAutoSizing=true;heading.fontSizeMin=18*MenuUnit;heading.fontSizeMax=23*MenuUnit;
            for(int i=0;i<page.Lines.Length;i++){
                float top=.408f-i*.118f;
                var line=Text("World story line "+i,menuRoot,new Vector2(.095f,top-.109f),new Vector2(.905f,top),18,
                    new UiCopy("{0}: {1}",new UiCopy(page.Lines[i].Speaker),new UiCopy(page.Lines[i].Text)));
                line.alignment=TextAlignmentOptions.MidlineLeft;line.color=MenuInk;line.enableAutoSizing=true;
                line.fontSizeMin=15*MenuUnit;line.fontSizeMax=19*MenuUnit*(Preferences.LargeText?1.12f:1);line.margin=new Vector4(4,3,4,3);
            }
            var next=Button(worldComicPage==2?"To the map":"Continue",menuRoot,new Vector2(.11f,.072f),new Vector2(.89f,.145f),AdvanceWorldComic);
            next.name="Next comic page";StyleConversationButton(next,enamelControls[1],MenuInk);
            var number=Text("World comic progress",menuRoot,new Vector2(.36f,.018f),new Vector2(.64f,.063f),15,UiCopy.Raw((worldComicPage+1)+" / 3"));number.color=MenuInk;
        }
        void TryJourneyOpening()
        {
            if(ConversationOpen||!IsJourneyStopOpen(selectedJourneyStop)||profile.Completion!=null&&!profile.Completion.Acknowledged)return;
            int world=JourneyStory.Zone(selectedJourneyStop),island=JourneyStory.IslandFor(selectedJourneyStop);
            var opener=StoryConversations.IslandOpening(world,island);
            if(!heardStory.Contains(opener.Receipt)){OpenConversation(opener,"To the map",ShowSelectedJourney);return;}
            if(!StoryConversations.HasLevelAside(selectedJourneyStop))return;
            var aside=StoryConversations.LevelAside(selectedJourneyStop);
            if(!heardStory.Contains(aside.Receipt))OpenConversation(aside,"To the map",ShowSelectedJourney);
        }
    }
}
