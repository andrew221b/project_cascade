using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Content;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        const int PrologueReceipt=1000;
        bool storyRoute;
        Sprite[] navigationIcons;
        readonly HashSet<TextMeshProUGUI> navigationLightLabels=new HashSet<TextMeshProUGUI>();
        public bool IsStoryRoute=>storyRoute;
        public int LevelShelfPage=>chapterPage;
        public void ShowPlayChoice(){ChooseStory();}
        struct ShopLocation
        {
            public GameScreen Origin;
            public int Tab,Item,Category;
            public bool Detail;
        }
        ShopLocation CaptureShop()=>new ShopLocation{Origin=shopReturnScreen,Tab=shopTab,Item=shopItem,Category=trophyCategory,Detail=shopDetailOpen};
        void RestoreShop(ShopLocation location)
        {
            shopReturnScreen=location.Origin;shopTab=location.Tab;shopItem=location.Item;trophyCategory=location.Category;shopDetailOpen=location.Detail;
            shopNotice=default;SetScreen(GameScreen.Shop);
        }
        public void ShowShop()
        {
            // A link back to an existing parent resumes that visit instead of making a loop.
            if(CurrentScreen==GameScreen.TravelBag&&bagReturnScreen==GameScreen.Shop){BackFromBag();return;}
            if(CurrentScreen==GameScreen.Collection&&collectionReturn==GameScreen.Shop){BackFromCollection();return;}
            if(CurrentScreen==GameScreen.Supplies&&toolsReturnScreen==GameScreen.Shop){BackFromTools();return;}
            if(CurrentScreen!=GameScreen.Shop&&CurrentScreen!=GameScreen.CoinPacks)shopReturnScreen=CurrentScreen;
            shopNotice=default;shopDetailOpen=false;SetScreen(GameScreen.Shop);
        }
        public void ShowNoAds(){if(CurrentScreen!=GameScreen.NoAds)noAdsReturnScreen=CurrentScreen;SetScreen(GameScreen.NoAds);}
        public void ChooseSoloMode(PuzzleMode mode){SelectMode(mode);}
        public void ChooseStory()
        {
            storyRoute=false;
            if(!RemembersStory(PrologueReceipt)&&!RemembersStory(WorldPrologues.ForWorld(0).Receipt)
                &&!profile.AllClears.Any()&&!HasResumableJourneyAttempt){
                OpenWorldComic(0,EnterFirstJourneyPuzzle);return;
            }
            // Keep an unfinished concrete attempt pinned; the choice affects future levels.
            SelectJourneyDestination();SetScreen(GameScreen.JourneyMap);
        }
        void EnterFirstJourneyPuzzle()
        {
            var first=JourneySequence().FirstOrDefault();if(first==null){ShowJourney();return;}
            selectedJourneyStop=Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==first.Definition.CampaignNodeId);
            mapPage=JourneyStory.Zone(selectedJourneyStop);islandBrowseIndex=-1;
            // The opening comic replaces the same introduction on the first island.
            heardStory.Add(StoryConversations.IslandOpening(mapPage,JourneyStory.IslandFor(selectedJourneyStop)).Receipt);
            heardStory.Add(JourneyStory.PageFor(JourneyStory.WorldFirst(mapPage)));
            pendingJourney=first;EnterJourneyPuzzle();PersistProgress(true);
        }
        public void ShowPrologue(){OpenWorldComic(0,ShowJourney);}
        public void AdvancePrologue()
        {
            AdvanceWorldComic();
        }
        public void TurnLevelShelf(int direction)
        {
            int pages=Math.Max(1,(PreferredLevels().Count+8)/9);
            int page=Mathf.Clamp(chapterPage+direction,0,pages-1);
            if(page==chapterPage)return;chapterPage=page;feedback.Sound(ToyCue.Select);DrawMenu();menuTransition.Arrive();
        }
        void DisposeNavigationArt()
        {
            if(referenceShopCoin!=null)Destroy(referenceShopCoin);
            if(navigationIcons!=null)foreach(var s in navigationIcons)if(s!=null)Destroy(s);
            foreach(var atlas in worldComicPanels)if(atlas!=null)foreach(var s in atlas)if(s!=null)Destroy(s);
            if(comicTimberFrame!=null)Destroy(comicTimberFrame);
        }
        void LoadNavigationArt()
        {
            if(navigationIcons==null)navigationIcons=NavigationIconSlices();
        }
        Sprite[] NavigationIconSlices()
        {
            var texture=Resources.Load<Texture2D>("CascadeArt/Navigation/MenuIcons");var sprites=new Sprite[8];
            if(texture==null)return sprites;
            // Authored silhouettes plus four source pixels of breathing room. Empty atlas fields
            // must not shift each icon below its button's optical centre.
            var bounds=new[]{new Rect(39,516,319,424),new Rect(386,512,358,363),new Rect(777,515,363,368),new Rect(1180,527,326,353),
                new Rect(10,82,374,355),new Rect(384,120,384,392),new Rect(800,105,337,328),new Rect(1174,107,351,312)};
            for(int i=0;i<sprites.Length;i++){
                var b=bounds[i];var rect=new Rect(b.x/1536*texture.width,b.y/1024*texture.height,b.width/1536*texture.width,b.height/1024*texture.height);
                sprites[i]=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            }
            return sprites;
        }
        Image MenuIcon(string name,Transform parent,Vector2 centre,Sprite sprite,float size=44)
        {
            var image=Picture(name,parent,centre,centre,sprite,true);
            image.type=Image.Type.Simple;image.rectTransform.sizeDelta=Vector2.one*size*MenuUnit;
            menuSquares.Add(new KeyValuePair<RectTransform,float>(image.rectTransform,size));
            return image;
        }
        Sprite[] NavigationSlices(string path,int columns,int rows)
        {
            var texture=Resources.Load<Texture2D>("CascadeArt/"+path);var sprites=new Sprite[columns*rows];
            if(texture==null)return sprites;
            float w=texture.width/(float)columns,h=texture.height/(float)rows;
            for(int i=0;i<sprites.Length;i++)sprites[i]=Sprite.Create(texture,new Rect(i%columns*w,(rows-1-i/columns)*h,w,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            return sprites;
        }
        RectTransform NavigationHeader(UiCopy title,Action back)
        {
            var headerPlate=Rect("Navigation heading",menuRoot,new Vector2(.06f,.88f),new Vector2(.94f,.975f));
            var button=DirectionButton("Back",headerPlate,new Vector2(.01f,.12f),new Vector2(.20f,.88f),false,back);
            var backRect=(RectTransform)button.transform;backRect.anchorMin=backRect.anchorMax=new Vector2(0,.5f);backRect.pivot=new Vector2(0,.5f);backRect.sizeDelta=Vector2.one*48*MenuUnit;menuSquares.Add(new KeyValuePair<RectTransform,float>(backRect,48));
            var heading=Text("Page heading",headerPlate,new Vector2(.19f,.08f),new Vector2(.94f,.92f),29,title);heading.font=art.titleFont;heading.alignment=TextAlignmentOptions.Midline;
            heading.enableAutoSizing=true;heading.fontSizeMin=21*MenuUnit;heading.fontSizeMax=29*MenuUnit;heading.textWrappingMode=TextWrappingModes.NoWrap;
            return headerPlate;
        }
        void CatalogueHeader(UiCopy title,Action back)
        {
            var header=NavigationHeader(title,back);
            header.anchorMin=new Vector2(MenuColumnInset,.906f);header.anchorMax=new Vector2(1-MenuColumnInset,.985f);
            menuRows.Add(new MenuRow{rect=header,top=.985f,height=56});FitMenuRows();
            var heading=header.Find("Page heading").GetComponent<TextMeshProUGUI>();
            heading.rectTransform.anchorMin=new Vector2(.21f,.05f);heading.rectTransform.anchorMax=new Vector2(.79f,.95f);
            heading.alignment=TextAlignmentOptions.Midline;heading.margin=Vector4.zero;
            heading.font=art.titleFont;heading.fontStyle=FontStyles.Bold;
        }
        Button NavigationButton(string name,UiCopy title,int icon,float bottom,Action action,bool primary=false,UiCopy detail=default)
        {
            LoadNavigationArt();bool described=!string.IsNullOrEmpty(detail.Key);
            var b=Button(title,menuRoot,new Vector2(MenuColumnInset,bottom),new Vector2(1-MenuColumnInset,bottom+(described?.118f:.085f)),()=>{feedback.Sound(ToyCue.Select);action();});b.name=name;
            var face=b.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=Color.white;
            if(primary)StylePrimaryFace(face);
            if(face.sprite==null){face.sprite=art.panel;face.color=new Color(.10f,.37f,.38f);}
            var label=b.GetComponentInChildren<TextMeshProUGUI>();label.rectTransform.anchorMin=new Vector2(.26f,described?.43f:.10f);label.rectTransform.anchorMax=new Vector2(.91f,described?.93f:.90f);
            label.alignment=TextAlignmentOptions.MidlineLeft;label.margin=new Vector4(4,2,7,2)*MenuUnit;label.fontSizeMin=18*MenuUnit;label.fontSizeMax=24*MenuUnit*(Preferences.LargeText?1.10f:1);label.enableAutoSizing=true;
            navigationLightLabels.Add(label);
            if(icon>=0)MenuIcon("Action illustration",b.transform,new Vector2(.14f,.5f),navigationIcons[icon]);
            if(described){var sub=Text("Action explanation",b.transform,new Vector2(.27f,.12f),new Vector2(.90f,.43f),16,detail);sub.alignment=TextAlignmentOptions.MidlineLeft;sub.enableAutoSizing=true;sub.fontSizeMin=14*MenuUnit;sub.fontSizeMax=17*MenuUnit;sub.margin=Vector4.one*(2*MenuUnit);navigationLightLabels.Add(sub);}
            return b;
        }
        void NavigationNote(string name,UiCopy text,float bottom,float top)
        {
            ReadingStrip(name,new Vector2(MenuColumnInset,bottom),new Vector2(1-MenuColumnInset,top),text,18,true);
        }
        void DrawMainMenu()
        {
            DrawReferenceHome();
        }
        void DrawPlayChoice()
        {
            DrawJourney(); // Legacy screen enum kept for compatibility; no duplicate mode selector.
        }
        void DrawShop() { DrawTravelCounter(false); }
        void DrawNoAds()
        {
            DrawReferenceNoAds();
        }
        void DrawPrologue()
        {
            if(worldComic<0){worldComic=worldComicPage=worldComicLine=0;worldComicDone=ShowJourney;machine.SetZone(0);}
            DrawWorldComic();
        }
    }
}
