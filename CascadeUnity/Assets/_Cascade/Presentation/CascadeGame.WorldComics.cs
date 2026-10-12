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
        int worldComic=-1,worldComicPage,worldComicLine,worldComicChunk;
        string[] worldComicChunks=Array.Empty<string>();
        float comicReadingWidth,comicReadingRoom,comicReadingFont;
        string comicReadingSource;
        Action worldComicDone;
        readonly Sprite[][] worldComicPanels=new Sprite[5][];
        Sprite comicWoodRail;
        public int ComicWorld=>worldComic;
        public int ComicPage=>worldComicPage;
        bool TryWorldComic(int world,Action done)
        {
            if(profile.Completion!=null&&!profile.Completion.Acknowledged||RemembersStory(WorldPrologues.ForWorld(world).Receipt))return false;
            OpenWorldComic(world,done);return true;
        }
        public void OpenWorldComic(int world,Action done=null)
        {
            CancelConversation();worldComic=Mathf.Clamp(world,0,JourneyStory.WorldCount-1);
            worldComicPage=worldComicLine=worldComicChunk=0;worldComicChunks=Array.Empty<string>();worldComicDone=done??ShowJourney;machine.SetZone(worldComic);
            SetScreen(GameScreen.Prologue);
        }
        void AdvanceWorldComic()
        {
            if(worldComic<0||CurrentScreen!=GameScreen.Prologue)return;
            var story=WorldPrologues.ForWorld(worldComic);var page=story.Pages[worldComicPage];
            if(worldComicChunk+1<worldComicChunks.Length){worldComicChunk++;DrawMenu();return;}
            worldComicChunk=0;
            if(worldComicLine+1<page.Lines.Length){worldComicLine++;DrawMenu();return;}
            if(worldComicPage+1<story.Pages.Length){worldComicPage++;worldComicLine=0;DrawMenu();menuTransition.Arrive();return;}
            FinishWorldComic();
        }
        public void FinishWorldComic()
        {
            if(worldComic<0)return;
            heardStory.Add(WorldPrologues.ForWorld(worldComic).Receipt);if(worldComic==0)heardStory.Add(PrologueReceipt);PersistProgress(true);
            if(HasSaveError)return;
            var done=worldComicDone;worldComic=-1;worldComicDone=null;done?.Invoke();
        }
        void DrawWorldComic()
        {
            var story=WorldPrologues.ForWorld(worldComic);var page=story.Pages[worldComicPage];var line=page.Lines[worldComicLine];
            int sheet=worldComic/3;
            if(worldComicPanels[sheet]==null)worldComicPanels[sheet]=NavigationSlices("Navigation/WorldComic"+sheet,3,3);
            if(worldComicPage>=3&&worldComicPanels[3]==null)worldComicPanels[3]=NavigationSlices("Navigation/WorldComicDepartureIdentity",3,3);
            bool correctedResident=(worldComic==1||worldComic==2)&&worldComicPage>=1&&worldComicPage<3;
            if(correctedResident&&worldComicPanels[4]==null)worldComicPanels[4]=NavigationSlices("Navigation/WorldComic0Identity",3,3);
            NavigationHeader(JourneyStory.ZoneName(worldComic),FinishWorldComic);
            var title=menuRoot.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Page heading");
            title.name="Comic title";title.alignment=TextAlignmentOptions.Center;title.rectTransform.anchorMin=new Vector2(.19f,.03f);title.rectTransform.anchorMax=new Vector2(.98f,.97f);
            title.fontSizeMin=14*MenuUnit;title.fontSizeMax=22*MenuUnit;title.margin=new Vector4(0,0,4*MenuUnit,0);
            var frame=Rect("World comic illustration",menuRoot,Vector2.zero,Vector2.one);
            var scene=worldComicPage<3?worldComicPanels[correctedResident?4:sheet][worldComic%3*3+worldComicPage]:worldComicPanels[3][worldComic];
            var illustration=Picture("World comic scene",frame,Vector2.zero,Vector2.one,scene,true);
            if(comicWoodRail==null){
                var timber=Resources.Load<Texture2D>("CascadeArt/Woodland/WoodBoard");
                // Use the published plank crop, rather than squashing its transparent canvas into the rail.
                comicWoodRail=Sprite.Create(timber,new Rect(timber.width*.023020258f,timber.height*.382596672f,
                    timber.width*.953959465f,timber.height*.255524874f),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            }
            var rails=new RectTransform[4];
            for(int i=0;i<rails.Length;i++){
                var rail=Picture("Comic wooden rail "+i,frame,Vector2.zero,Vector2.one,comicWoodRail,false);
                rail.raycastTarget=false;rail.preserveAspect=false;rails[i]=rail.rectTransform;
            }
            var speech=Rect("World comic speech ribbon",menuRoot,Vector2.zero,Vector2.one);
            PaintStoryPaper(speech,false);
            var portrait=Rect("World comic speaker portrait",speech,Vector2.zero,Vector2.one).gameObject.AddComponent<UnityEngine.UI.Image>();
            portrait.sprite=ConversationPortrait(line.Speaker);portrait.preserveAspect=true;portrait.raycastTarget=false;
            if(line.Speaker=="Mika")AttachMikaCosmetic(portrait,null,true);
            var nameRibbon=Rect("World comic speaker name",speech,Vector2.zero,Vector2.one);
            var speaker=Text("World comic speaker",nameRibbon,Vector2.zero,Vector2.one,18,line.Speaker);speaker.font=art.titleFont;speaker.color=MenuInk;speaker.alignment=TextAlignmentOptions.MidlineLeft;
            var reading=Rect("World comic reading area",speech,Vector2.zero,Vector2.one);
            var viewport=Rect("World comic reading viewport",reading,Vector2.zero,Vector2.one);Background(viewport,Color.white,true);
            var mask=viewport.gameObject.AddComponent<UnityEngine.UI.Mask>();mask.showMaskGraphic=false;
            var body=Text("World comic spoken line",viewport,new Vector2(0,1),Vector2.one,18,UiCopy.Raw(new UiCopy(line.Text).ToString()));
            body.alignment=TextAlignmentOptions.TopLeft;body.textWrappingMode=TextWrappingModes.Normal;body.overflowMode=TextOverflowModes.Overflow;
            body.color=MenuInk;
            body.rectTransform.pivot=new Vector2(.5f,1);body.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            var scroll=reading.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.viewport=viewport;scroll.content=body.rectTransform;scroll.horizontal=false;scroll.inertia=false;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            bool last=worldComicPage==story.Pages.Length-1&&worldComicLine==page.Lines.Length-1&&worldComicChunk==worldComicChunks.Length-1;
            var next=Button(last?"To the map":"Continue",speech,Vector2.zero,Vector2.one,AdvanceWorldComic);
            next.name="Next comic page";StyleConversationButton(next,enamelControls[0],MenuPaper);
            var number=Text("World comic progress",menuRoot,Vector2.zero,Vector2.one,15,UiCopy.Raw((worldComicPage+1)+" / "+story.Pages.Length));number.color=MenuInk;
            speech.gameObject.AddComponent<WorldComicLayout>().Initialize(this,menuRoot,(RectTransform)title.transform.parent,illustration,rails,portrait,nameRibbon,speaker,body,scroll,next,number,worldComicChunks);
        }
        internal string FitComicCopy(float width,float room,TextMeshProUGUI measure)
        {
            var story=WorldPrologues.ForWorld(worldComic);var page=story.Pages[worldComicPage];
            string source=new UiCopy(page.Lines[worldComicLine].Text).ToString();
            if(worldComicChunks.Length==0||comicReadingWidth!=width||comicReadingRoom!=room||comicReadingFont!=measure.fontSize||comicReadingSource!=source){
                worldComicChunks=DialoguePages.Split(source,t=>measure.GetPreferredValues(t,width,Mathf.Infinity).y<=room).ToArray();
                comicReadingWidth=width;comicReadingRoom=room;comicReadingFont=measure.fontSize;comicReadingSource=source;
                worldComicChunk=Mathf.Clamp(worldComicChunk,0,worldComicChunks.Length-1);
            }
            return worldComicChunks[worldComicChunk];
        }
        internal UiCopy ComicAdvanceCaption=>worldComicPage==WorldPrologues.ForWorld(worldComic).Pages.Length-1
            &&worldComicLine==WorldPrologues.ForWorld(worldComic).Pages[worldComicPage].Lines.Length-1&&worldComicChunk==worldComicChunks.Length-1?(UiCopy)"To the map":"Continue";
        void TryJourneyOpening()
        {
            if(ConversationOpen||!IsJourneyStopOpen(selectedJourneyStop)||profile.Completion!=null&&!profile.Completion.Acknowledged)return;
            int world=JourneyStory.Zone(selectedJourneyStop),island=JourneyStory.IslandFor(selectedJourneyStop);
            var opener=StoryConversations.IslandOpening(world,island);
            if(!RemembersStory(opener.Receipt)){OpenConversation(opener,"To the map",ShowSelectedJourney);return;}
            if(!StoryConversations.HasLevelAside(selectedJourneyStop))return;
            var aside=StoryConversations.LevelAside(selectedJourneyStop);
            if(!RemembersStory(aside.Receipt))OpenConversation(aside,"To the map",ShowSelectedJourney);
        }
    }
}
