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
        int worldComic=-1,worldComicPage,worldComicLine;
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
            worldComicPage=worldComicLine=0;worldComicDone=done??ShowJourney;machine.SetZone(worldComic);
            SetScreen(GameScreen.Prologue);
        }
        void AdvanceWorldComic()
        {
            if(worldComic<0||CurrentScreen!=GameScreen.Prologue)return;
            var page=WorldPrologues.ForWorld(worldComic).Pages[worldComicPage];
            if(++worldComicLine<page.Lines.Length){DrawMenu();return;}
            worldComicLine=0;
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
            var story=WorldPrologues.ForWorld(worldComic);var page=story.Pages[worldComicPage];var line=page.Lines[worldComicLine];
            int sheet=worldComic/3;
            if(worldComicPanels[sheet]==null)worldComicPanels[sheet]=NavigationSlices("Navigation/WorldComic"+sheet,3,3);
            NavigationHeader(JourneyStory.ZoneName(worldComic),FinishWorldComic);
            var title=menuRoot.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Page heading");
            title.fontSizeMin=16*MenuUnit;title.fontSizeMax=26*MenuUnit;title.margin=new Vector4(0,0,4*MenuUnit,0);
            var frame=Rect("World comic illustration",menuRoot,Vector2.zero,Vector2.one);
            var illustration=Picture("World comic scene",frame,Vector2.zero,Vector2.one,worldComicPanels[sheet][worldComic%3*3+worldComicPage],true);
            var speech=Rect("World comic speech ribbon",menuRoot,Vector2.zero,Vector2.one);
            var lining=Rect("Opaque enamel lining",speech,new Vector2(.025f,.025f),new Vector2(.975f,.975f));Background(lining,new Color(.04f,.235f,.245f),false);
            var rim=Rect("Enamel ribbon rim",speech,Vector2.zero,Vector2.one);var enamel=Background(rim,Color.white,false);enamel.sprite=enamelControls[0];enamel.type=UnityEngine.UI.Image.Type.Sliced;
            var portrait=Rect("World comic speaker portrait",speech,Vector2.zero,Vector2.one).gameObject.AddComponent<UnityEngine.UI.Image>();
            portrait.sprite=ConversationPortrait(line.Speaker);portrait.preserveAspect=true;portrait.raycastTarget=false;
            if(line.Speaker=="Mika")AttachMikaCosmetic(portrait,null,true);
            var nameRibbon=Rect("World comic speaker ribbon",speech,Vector2.zero,Vector2.one);var cloth=Background(nameRibbon,Color.white,false);cloth.sprite=CompletionArt(1);cloth.type=UnityEngine.UI.Image.Type.Sliced;
            var speaker=Text("World comic speaker name",nameRibbon,new Vector2(.10f,.08f),new Vector2(.90f,.92f),20,line.Speaker);speaker.font=art.titleFont;
            var reading=Rect("World comic reading area",speech,Vector2.zero,Vector2.one);
            var viewport=Rect("World comic reading viewport",reading,Vector2.zero,Vector2.one);Background(viewport,Color.white,true);
            var mask=viewport.gameObject.AddComponent<UnityEngine.UI.Mask>();mask.showMaskGraphic=false;
            var body=Text("World comic spoken line",viewport,new Vector2(0,1),Vector2.one,20,line.Text);
            body.alignment=TextAlignmentOptions.TopLeft;body.textWrappingMode=TextWrappingModes.Normal;body.overflowMode=TextOverflowModes.Overflow;
            body.rectTransform.pivot=new Vector2(.5f,1);body.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            navigationLightLabels.Add(body);
            var scroll=reading.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.viewport=viewport;scroll.content=body.rectTransform;scroll.horizontal=false;scroll.inertia=false;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            bool last=worldComicPage==2&&worldComicLine==page.Lines.Length-1;
            var next=Button(last?"To the map":"Continue",menuRoot,Vector2.zero,Vector2.one,AdvanceWorldComic);
            next.name="Next comic page";StyleConversationButton(next,enamelControls[0],MenuPaper);
            var number=Text("World comic progress",menuRoot,Vector2.zero,Vector2.one,15,UiCopy.Raw((worldComicPage+1)+" / 3"));number.color=MenuInk;
            speech.gameObject.AddComponent<WorldComicLayout>().Initialize(this,menuRoot,(RectTransform)title.transform.parent,illustration,portrait,nameRibbon,speaker,body,scroll,next,number);
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
