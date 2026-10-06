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
        int prologuePage;
        bool storyRoute;
        Sprite[] navigationIcons,prologuePanels;
        readonly HashSet<TextMeshProUGUI> navigationLightLabels=new HashSet<TextMeshProUGUI>();
        public bool IsStoryRoute=>storyRoute;
        public int LevelShelfPage=>chapterPage;
        public void ShowPlayChoice(){SetScreen(GameScreen.PlayChoice);}
        public void ShowShop(){SetScreen(GameScreen.Shop);}
        public void ShowNoAds(){SetScreen(GameScreen.NoAds);}
        public void ChooseSoloMode(PuzzleMode mode){storyRoute=false;journeyActive=false;SelectMode(mode);ShowLessons();}
        public void ChooseStory()
        {
            storyRoute=true;
            // Keep an unfinished concrete attempt pinned; the choice affects future levels.
            if(!heardStory.Contains(PrologueReceipt))ShowPrologue();else ShowJourney();
        }
        public void ShowPrologue(){storyRoute=true;prologuePage=0;SetScreen(GameScreen.Prologue);}
        public void AdvancePrologue()
        {
            if(++prologuePage<3){DrawMenu();menuTransition.Arrive();return;}
            heardStory.Add(PrologueReceipt);PersistProgress();ShowJourney();
        }
        public void TurnLevelShelf(int direction)
        {
            int pages=Math.Max(1,(PreferredLevels().Count+8)/9);
            int page=Mathf.Clamp(chapterPage+direction,0,pages-1);
            if(page==chapterPage)return;chapterPage=page;feedback.Sound(ToyCue.Select);DrawMenu();menuTransition.Arrive();
        }
        void DisposeNavigationArt()
        {
            if(navigationIcons!=null)foreach(var s in navigationIcons)if(s!=null)Destroy(s);
            if(prologuePanels!=null)foreach(var s in prologuePanels)if(s!=null)Destroy(s);
        }
        void LoadNavigationArt()
        {
            if(navigationIcons==null)navigationIcons=NavigationSlices("Navigation/MenuIcons",4,2);
            if(prologuePanels==null)prologuePanels=NavigationSlices("Navigation/OpeningComic",3,1);
        }
        Sprite[] NavigationSlices(string path,int columns,int rows)
        {
            var texture=Resources.Load<Texture2D>("CascadeArt/"+path);var sprites=new Sprite[columns*rows];
            if(texture==null)return sprites;
            float w=texture.width/(float)columns,h=texture.height/(float)rows;
            for(int i=0;i<sprites.Length;i++)sprites[i]=Sprite.Create(texture,new Rect(i%columns*w,(rows-1-i/columns)*h,w,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            return sprites;
        }
        void NavigationHeader(UiCopy title,Action back)
        {
            var headerPlate=Rect("Navigation heading",menuRoot,new Vector2(.06f,.88f),new Vector2(.94f,.975f));Panel(headerPlate,MenuPaper);
            var button=DirectionButton("Back",headerPlate,new Vector2(.01f,.12f),new Vector2(.20f,.88f),false,back);
            button.GetComponent<Image>().color=Color.clear;((RectTransform)button.transform).sizeDelta=new Vector2(0,8*MenuUnit);
            var heading=Text("Page heading",headerPlate,new Vector2(.22f,.08f),new Vector2(.94f,.92f),29,title);heading.font=art.titleFont;heading.alignment=TextAlignmentOptions.MidlineLeft;
            heading.enableAutoSizing=true;heading.fontSizeMin=21*MenuUnit;heading.fontSizeMax=29*MenuUnit;heading.textWrappingMode=TextWrappingModes.NoWrap;
        }
        Button NavigationButton(string name,UiCopy title,int icon,float bottom,Action action,bool primary=false,UiCopy detail=default)
        {
            LoadNavigationArt();bool described=!string.IsNullOrEmpty(detail.Key);
            var b=Button(title,menuRoot,new Vector2(.08f,bottom),new Vector2(.92f,bottom+(described?.118f:.085f)),()=>{feedback.Sound(ToyCue.Select);action();});b.name=name;
            var face=b.GetComponent<Image>();face.sprite=enamelControls[primary?1:0];face.type=Image.Type.Sliced;face.color=Color.white;
            if(face.sprite==null){face.sprite=art.panel;face.color=primary?new Color(.98f,.74f,.32f):new Color(.10f,.37f,.38f);}
            var label=b.GetComponentInChildren<TextMeshProUGUI>();label.rectTransform.anchorMin=new Vector2(.26f,described?.43f:.10f);label.rectTransform.anchorMax=new Vector2(.91f,described?.93f:.90f);
            label.alignment=TextAlignmentOptions.MidlineLeft;label.margin=new Vector4(4,1,7,1);label.fontSizeMin=18*MenuUnit;label.fontSizeMax=24*MenuUnit*(Preferences.LargeText?1.10f:1);label.enableAutoSizing=true;
            if(!primary)navigationLightLabels.Add(label);
            if(icon>=0)Picture("Action illustration",b.transform,new Vector2(.045f,.09f),new Vector2(.24f,.91f),navigationIcons[icon],true);
            if(described){var sub=Text("Action explanation",b.transform,new Vector2(.27f,.12f),new Vector2(.90f,.43f),16,detail);sub.alignment=TextAlignmentOptions.MidlineLeft;sub.enableAutoSizing=true;sub.fontSizeMin=14*MenuUnit;sub.fontSizeMax=17*MenuUnit;sub.margin=Vector4.zero;if(!primary)navigationLightLabels.Add(sub);}
            return b;
        }
        void NavigationNote(string name,UiCopy text,float bottom,float top)
        {
            var root=Rect(name+" cloth",menuRoot,new Vector2(.09f,bottom),new Vector2(.91f,top));Panel(root,MenuPaper);
            var copy=Text(name,root,new Vector2(.06f,.08f),new Vector2(.94f,.92f),20,text);copy.enableAutoSizing=true;copy.fontSizeMin=17*MenuUnit;copy.fontSizeMax=21*MenuUnit;copy.margin=new Vector4(5,4,5,4);
        }
        void DrawMainMenu()
        {
            LoadNavigationArt();machine.SetZone(0);
            var logo=Text("CASCADE",menuRoot,new Vector2(.08f,.885f),new Vector2(.92f,.97f),43,UiCopy.Raw("CASCADE"));logo.font=art.titleFont;
            var hero=Picture("Mika welcomes you",menuRoot,new Vector2(.26f,.515f),new Vector2(.74f,.875f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            hero.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,2),0,.35f);
            Rect("Mika's home bench",menuRoot,new Vector2(.24f,.512f),new Vector2(.76f,.529f)).gameObject.AddComponent<WoodBoardGraphic>();
            NavigationButton("Startgame","Start game",0,.405f,ShowPlayChoice,true);
            NavigationButton("Shop","Shop",1,.303f,ShowShop);
            NavigationButton("Settings","Settings",2,.201f,ShowSettings);
            NavigationButton("Removeads","Remove ads",3,.099f,ShowNoAds);
        }
        void DrawPlayChoice()
        {
            NavigationHeader("Play",ShowHome);
            NavigationNote("Play question","How would you like to play?",.755f,.85f);
            NavigationButton("Storymode","Story",4,.595f,ChooseStory,true,"Mika's journey. A little of everything.");
            NavigationButton("Shapes","Shapes",5,.443f,()=>ChooseSoloMode(PuzzleMode.Shapes),false,"Just shapes.");
            NavigationButton("Words","Words",6,.291f,()=>ChooseSoloMode(PuzzleMode.Words),false,"Put the word in order.");
            NavigationButton("Mixed","Mixed",7,.139f,()=>ChooseSoloMode(PuzzleMode.Mixed),false,"Shapes and letters.");
            var opening=Button("Opening story",menuRoot,new Vector2(.24f,.035f),new Vector2(.76f,.10f),ShowPrologue);opening.GetComponent<Image>().color=MenuPaper;((RectTransform)opening.transform).sizeDelta=new Vector2(0,12*MenuUnit);
        }
        void DrawLessons()
        {
            LoadNavigationArt();NavigationHeader(new UiCopy("{0} levels",new UiCopy(PreferredMode.ToString())),ShowPlayChoice);
            var levels=PreferredLevels();int pages=Math.Max(1,(levels.Count+8)/9);chapterPage=Mathf.Clamp(chapterPage,0,pages-1);
            bool resume=Interaction.Attempt.Mode==PreferredMode&&Interaction.Attempt.ActiveLocale==(PreferredMode==PuzzleMode.Shapes?"en":Preferences.PuzzleLocale)
                &&Interaction.Attempt.Current.Status==PuzzleStatus.Playing&&Interaction.Attempt.RouteLength>0;
            var chosen=levels.FirstOrDefault(c=>!sessionClears.Contains(c.Definition.CampaignNodeId))??levels[levels.Count-1];
            NavigationNote("Shelf instruction","Tap a number to play. Swipe for more levels.",.77f,.85f);
            var shelf=Rect("Swipeable level shelf",menuRoot,new Vector2(.06f,.275f),new Vector2(.94f,.735f));Background(shelf,Color.clear,true);
            shelf.gameObject.AddComponent<MenuSwipe>().Initialize(TurnLevelShelf);
            for(int i=0;i<9&&chapterPage*9+i<levels.Count;i++){
                var content=levels[chapterPage*9+i];string id=content.Definition.CampaignNodeId;int col=i%3,row=i/3;
                var b=Button(UiCopy.Raw(id.TrimStart('C','0')),shelf,new Vector2(col/3f+.028f,1-(row+1)/3f+.035f),new Vector2((col+1)/3f-.028f,1-row/3f-.035f),()=>StartContent(content));
                var image=b.GetComponent<Image>();image.sprite=enamelControls[2];image.type=Image.Type.Sliced;image.color=Color.white;
                var number=b.GetComponentInChildren<TextMeshProUGUI>();number.fontSizeMax=30*MenuUnit;number.fontSizeMin=23*MenuUnit;number.margin=Vector4.zero;
                if(sessionClears.Contains(id))Rect("Completed level",b.transform,new Vector2(.73f,.13f),new Vector2(.89f,.31f)).gameObject.AddComponent<MenuTickGraphic>().color=new Color(.10f,.40f,.34f);
            }
            var page=Text("Level shelf page",menuRoot,new Vector2(.16f,.212f),new Vector2(.84f,.26f),18,new UiCopy("Page {0} of {1}",chapterPage+1,pages));page.font=art.titleFont;
            NavigationButton("Nextlevel",resume?"Continue":new UiCopy("Play level {0}",chosen.Definition.CampaignNodeId.TrimStart('C','0')),0,.08f,()=>{if(resume)ShowGame();else StartContent(chosen);},true);
        }
        void DrawShop()
        {
            NavigationHeader("Shop",ShowHome);
            var parcel=RewardPicture("Parcel",new Vector2(.32f,.60f),new Vector2(.68f,.83f));parcel.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,2),0,.6f);
            NavigationNote("Shop explanation","Restore lights. Collect parts. Make a gift for Mika.",.485f,.585f);
            var gifts=NavigationButton("Keepsakes","Keepsakes",-1,.365f,ShowCollection,true);
            Picture("Gift illustration",gifts.transform,new Vector2(.045f,.09f),new Vector2(.24f,.91f),Resources.Load<Sprite>("CascadeArt/Workshop/LeafPin"),true);
            NavigationButton("Workshop","Workshop",2,.255f,ShowWorkshop);
            NavigationButton("Mythings","My things",1,.145f,ShowTravelBag);
        }
        void DrawNoAds()
        {
            LoadNavigationArt();NavigationHeader("Remove ads",ShowHome);
            Picture("No adverts",menuRoot,new Vector2(.32f,.59f),new Vector2(.68f,.80f),navigationIcons[3],true);
            NavigationNote("No adverts explanation","Ads are already off in this version. Enjoy the game.",.36f,.54f);
            NavigationButton("Backtoplay","Start game",0,.17f,ShowPlayChoice,true);
        }
        void DrawPrologue()
        {
            LoadNavigationArt();NavigationHeader(new[]{"Before the storm","The last light","A way home"}[prologuePage],ShowPlayChoice);
            var frame=Rect("Opening comic frame",menuRoot,new Vector2(.055f,.43f),new Vector2(.945f,.855f));Panel(frame,MenuPaper);
            Picture("Opening comic scene",frame,new Vector2(.025f,.035f),new Vector2(.975f,.965f),prologuePanels[prologuePage],true);
            string[] lines={"Gran kept the Great Beacon burning. Its light showed everyone the way home.","Then the storm broke the bridges and put the beacon out. Mika saved one little light.","Mika: I'll bring it home, Gran. We'll mend the bridges, help our friends and light the beacon again."};
            NavigationNote("Opening story",lines[prologuePage],.235f,.405f);
            NavigationButton("Continueopening",prologuePage==2?"To the map":"Continue",0,.11f,AdvancePrologue,true);
            var skip=Button("Skip story",menuRoot,new Vector2(.24f,.025f),new Vector2(.76f,.083f),()=>{heardStory.Add(PrologueReceipt);PersistProgress();ShowJourney();});skip.GetComponent<Image>().color=MenuPaper;((RectTransform)skip.transform).sizeDelta=new Vector2(0,12*MenuUnit);
            var count=Text("Comic page",frame,new Vector2(.81f,.02f),new Vector2(.97f,.13f),16,UiCopy.Raw((prologuePage+1)+" / 3"));count.font=art.titleFont;
        }
    }
}
