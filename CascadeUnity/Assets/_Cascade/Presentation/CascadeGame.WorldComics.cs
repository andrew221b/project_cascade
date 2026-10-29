using System;
using System.Linq;
using DG.Tweening;
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
        Sprite comicTimberFrame;
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
            if(worldComic<0||CurrentScreen!=GameScreen.Prologue)return;
            int receipt=WorldPrologues.ForWorld(worldComic).Receipt;
            bool newWorld=heardStory.Add(receipt),newPrologue=worldComic==0&&heardStory.Add(PrologueReceipt);PersistProgress(true);
            if(HasSaveError){
                if(newWorld)heardStory.Remove(receipt);if(newPrologue)heardStory.Remove(PrologueReceipt);
                profile.HeardStory=heardStory.OrderBy(p=>p).ToList();return;
            }
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
            LoadNavigationArt();
            var backdrop=Rect("Reference shop environment",menuRoot,Vector2.one*.5f,Vector2.one*.5f);
            var sky=backdrop.gameObject.AddComponent<Image>();sky.sprite=ReferenceShopArt("Backdrop");sky.raycastTarget=false;
            var stage=Rect("Comic reference composition",menuRoot,Vector2.one*.5f,Vector2.one*.5f);stage.sizeDelta=new Vector2(720,1280);
            var fit=stage.gameObject.AddComponent<ReferenceShopLayout>();fit.Initialize(menuRoot,backdrop);
            var titleBoard=ReferencePicture("Comic title board",stage,ReferenceShopArt("HeaderBoard"),128,34,464,139,false);titleBoard.type=Image.Type.Sliced;titleBoard.pixelsPerUnitMultiplier=3;
            var titleArea=ReferenceRect("Comic title reading inset",titleBoard.transform,52,37.5f,360,64);
            var title=ReferenceText("comic title",titleArea,JourneyStory.ZoneName(worldComic),0,0,360,64,43);title.name="Comic title";title.fontSizeMin=25;title.fontStyle=FontStyles.Bold;
            var titleFit=CentreContent(titleArea,title.rectTransform);titleFit.Padding=4;titleFit.Refresh();titleBoard.gameObject.AddComponent<AttachedPlants>().Initialize(this,worldComic,true);
            var frame=ReferenceRect("World comic illustration",stage,44,214,632,421);
            var scene=worldComicPage<3?worldComicPanels[correctedResident?4:sheet][worldComic%3*3+worldComicPage]:worldComicPanels[3][worldComic];
            var illustration=Picture("World comic scene",frame,Vector2.zero,Vector2.one,scene,true);
            if(comicTimberFrame==null){
                var texture=Resources.Load<Texture2D>("CascadeArt/ComicReferenceOct16/TimberFrame");
                comicTimberFrame=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,
                    new Vector4(180,200,180,180)*texture.width/1536f);
            }
            var timber=ReferencePicture("World comic carved timber",stage,comicTimberFrame,14,184,692,484,false);timber.type=Image.Type.Sliced;timber.pixelsPerUnitMultiplier=6;
            var speech=ReferenceRect("World comic speech ribbon",stage,36,848,648,240);PaintStoryPaper(speech,false);
            var portrait=ReferencePicture("World comic speaker portrait",stage,ConversationPortrait(line.Speaker),46,684,162,152);
            if(line.Speaker=="Mika")AttachMikaCosmetic(portrait,null,true);
            var nameRibbon=ReferencePicture("World comic speaker name",stage,ReferenceShopArt("HeaderBoard"),28,807,300,81,false);nameRibbon.type=Image.Type.Sliced;nameRibbon.pixelsPerUnitMultiplier=4;
            nameRibbon.gameObject.AddComponent<AttachedPlants>().Initialize(this,worldComic,true);
            var nameArea=ReferenceRect("Comic name reading inset",nameRibbon.transform,29,20,242,41);
            var speaker=ReferenceText("comic speaker",nameArea,line.Speaker,0,0,242,41,29);speaker.name="World comic speaker";speaker.fontSizeMin=18;
            var nameFit=CentreContent(nameArea,speaker.rectTransform);nameFit.Padding=4;nameFit.Refresh();
            var reading=ReferenceRect("World comic reading area",speech,42,66,564,154);
            var viewport=Rect("World comic reading viewport",reading,Vector2.zero,Vector2.one);Background(viewport,Color.white,true);viewport.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            var body=ReferenceText("comic spoken line",viewport,UiCopy.Raw(new UiCopy(line.Text).ToString()),0,0,564,154,34,false,true);body.name="World comic spoken line";
            body.font=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceBody")??art.titleFont;body.fontStyle=FontStyles.Bold;body.alignment=TextAlignmentOptions.TopLeft;body.overflowMode=TextOverflowModes.Overflow;
            body.rectTransform.anchorMin=new Vector2(0,1);body.rectTransform.anchorMax=Vector2.one;body.rectTransform.pivot=new Vector2(.5f,1);body.rectTransform.anchoredPosition=Vector2.zero;
            var scroll=reading.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=body.rectTransform;scroll.horizontal=false;scroll.inertia=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            CharacterPoseView.Attach(this,portrait).SetSpeechSource(body);
            var next=ReferenceButton("Next comic page",stage,377,1120,299,102,AdvanceWorldComic,"GreenAction");
            ReferenceButtonBody(next).gameObject.AddComponent<AttachedPlants>().Initialize(this,worldComic,true);
            var nextLabel=ReferenceText("comic next",ReferenceButtonBody(next),ComicAdvanceCaption,20,13,259,76,42,true);CentreContent((RectTransform)ReferenceButtonBody(next),nextLabel.rectTransform);fit.BindTouch((RectTransform)next.transform,new Vector2(299,102));
            var skip=ReferenceButton("Skip comic",stage,66,1134,211,74,FinishWorldComic);
            RegisterButtonDesign(skip,GameButtonRole.TertiaryLabel).PreserveSlot=true;
            var skipLabel=ReferenceText("comic skip",ReferenceButtonBody(skip),ReferenceCopy("Skip","Пропустить","Пропустити"),8,10,195,54,31,true);skipLabel.fontStyle=FontStyles.Underline;
            skipLabel.outlineColor=new Color(.012f,.035f,.06f,1);skipLabel.outlineWidth=.18f;
            var skipInk=skipLabel.fontMaterial;skipInk.EnableKeyword("UNDERLAY_ON");skipInk.SetColor(ShaderUtilities.ID_UnderlayColor,new Color(.012f,.035f,.06f,1));
            skipInk.SetFloat(ShaderUtilities.ID_UnderlayOffsetY,-.6f);skipInk.SetFloat(ShaderUtilities.ID_UnderlayDilate,.25f);skipInk.SetFloat(ShaderUtilities.ID_UnderlaySoftness,.10f);skipLabel.UpdateMeshPadding();CentreContent((RectTransform)ReferenceButtonBody(skip),skipLabel.rectTransform);fit.BindTouch((RectTransform)skip.transform,new Vector2(211,74));
            // Every speaker turn is a visible reading page, even when the illustration stays.
            int readingPage=story.Pages.Take(worldComicPage).Sum(p=>p.Lines.Length)+worldComicLine+1;
            int readingPages=story.Pages.Sum(p=>p.Lines.Length);
            var number=ReferenceText("comic progress",speech,UiCopy.Raw(readingPage+" / "+readingPages),534,12,84,46,23);number.name="World comic progress";
            foreach(var label in new[]{title,speaker,body,number})themedLabels.Add(label);
            stage.gameObject.AddComponent<WorldComicLayout>().Initialize(this,stage,illustration,timber,portrait,nameRibbon.rectTransform,title,speaker,body,scroll,next,nextLabel,number,page.Lines.Select(l=>new UiCopy(l.Text).ToString()).ToArray());
            if(!ReducedMotion){portrait.rectTransform.localScale=Vector3.one*.98f;portrait.rectTransform.DOScale(1,.28f).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(portrait.gameObject);}
        }

        internal string FitComicCopy(float width,float room,TextMeshProUGUI measure)
        {
            var story=WorldPrologues.ForWorld(worldComic);var page=story.Pages[worldComicPage];
            string source=new UiCopy(page.Lines[worldComicLine].Text).ToString();
            if(worldComicChunks.Length==0||comicReadingWidth!=width||comicReadingRoom!=room||comicReadingFont!=measure.fontSize||comicReadingSource!=source){
                // One authored turn is one reading beat. Layout reserves enough room;
                // the reader can scroll at extreme accessibility sizes without one-word pages.
                worldComicChunks=new[]{source};
                comicReadingWidth=width;comicReadingRoom=room;comicReadingFont=measure.fontSize;comicReadingSource=source;
                worldComicChunk=Mathf.Clamp(worldComicChunk,0,worldComicChunks.Length-1);
            }
            return worldComicChunks[worldComicChunk];
        }
        internal UiCopy ComicAdvanceCaption=>worldComicPage==WorldPrologues.ForWorld(worldComic).Pages.Length-1
            &&worldComicLine==WorldPrologues.ForWorld(worldComic).Pages[worldComicPage].Lines.Length-1&&worldComicChunk==worldComicChunks.Length-1
                ?(worldComicDone==(Action)EnterFirstJourneyPuzzle?(UiCopy)"Play":"Map"):ReferenceCopy("Continue","Далее","Далі");
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
