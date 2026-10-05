using System;
using System.Collections;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Solver;
using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int giftPage,workshopPage,requestPage,recordPage,contractPage,parcelOrder;
        bool parcelOpened;
        LevelContent[] contracts;
        static readonly string[] GiftNames={"Leaf pin","Sun button","Sky ribbon","Moon patch"};
        static readonly string[] GiftAssets={"LeafPin","SunButton","SkyRibbon","MoonPatch"};
        static readonly string[] WorkshopNames={"Bridge lantern","Rain pump","Post glider","Beacon model"};
        static readonly string[] WorkshopAssets={"BridgeLantern","RainPump","PostGlider","BeaconModel"};
        static readonly string[] RankNames={"Apprentice","Garden friend","Mill keeper","Pathfinder","Beacon keeper"};
        ParcelReceipt WaitingParcel=>Rewards.Parcels.FirstOrDefault(p=>!p.Claimed);
        string[] OrderedNodes=>Catalog.PrimaryLevels.Select(c=>c.Definition.CampaignNodeId).ToArray();
        public void ShowTravelBag(){SetScreen(GameScreen.TravelBag);}
        public void ShowJournal(){SetScreen(GameScreen.Journal);}
        public void ShowCollection(){giftPage=Math.Max(0,Array.IndexOf(JourneyRewards.Gifts,Rewards.SelectedGift));SetScreen(GameScreen.Collection);}
        public void ShowWorkshop(){SetScreen(GameScreen.Workshop);}
        public void ShowRequests(){SetScreen(GameScreen.Requests);}
        public void ShowRecords(){recordPage=0;SetScreen(GameScreen.Records);}
        public void ShowContracts(){contractPage=0;SetScreen(GameScreen.Contracts);}
        public void ShowParcel()
        {
            if(WaitingParcel==null){ShowJournal();return;}
            parcelOrder=WaitingParcel.Order;parcelOpened=false;SetScreen(GameScreen.Parcel);
        }
        void DrawTravelBag()
        {
            DrawCraftBag();
        }
        void DrawJournal()
        {
            MenuTitle("My journal",RankNames[Rewards.Rank]);
            Picture("Mika reads",menuRoot,new Vector2(.06f,.44f),new Vector2(.47f,.75f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            Text("Journal lights",menuRoot,new Vector2(.48f,.60f),new Vector2(.94f,.70f),23,new UiCopy("Lights restored: {0}",JourneyClearCount)).font=art.titleFont;
            Text("Visit stamps",menuRoot,new Vector2(.48f,.46f),new Vector2(.94f,.58f),18,new UiCopy("Visits: {0} / 7\nCome back when you like.",Rewards.VisitDates.Count));
            UiCopy order=Rewards.AllGiftsEarned?"Every gift part is found!":new UiCopy("New lights for a parcel: {0} / 3",Rewards.OrderProgress);
            Text("Current order",menuRoot,new Vector2(.08f,.35f),new Vector2(.92f,.44f),20,order);
            if(WaitingParcel!=null)MenuButton("Open parcel",.26f,ShowParcel,true);
            else MenuButton("Choose a gift",.26f,ShowCollection,true);
            MenuButton("Requests",.14f,ShowRequests,false,.08f,.48f);MenuButton("Keepsakes",.14f,ShowCollection,false,.52f,.92f);
            MenuButton("Back",.025f,ShowTravelBag);
        }
        void MetaArrows(Action previousAction,Action nextAction)
        {
            DirectionButton("Previous",menuRoot,new Vector2(.04f,.48f),new Vector2(.21f,.57f),false,previousAction);
            DirectionButton("Next",menuRoot,new Vector2(.79f,.48f),new Vector2(.96f,.57f),true,nextAction);
        }
        UnityEngine.UI.Image RewardPicture(string asset,Vector2 min,Vector2 max)
            =>Picture(asset,menuRoot,min,max,Resources.Load<Sprite>("CascadeArt/Workshop/"+asset),true);
        void DrawCollection()
        {
            string id=JourneyRewards.Gifts[giftPage];bool owned=Rewards.Owns(id);
            CraftTitle(GiftNames[giftPage],"Your keepsakes");
            for(int i=0;i<4;i++){int pick=i;ShelfChoice("",GiftAssets[i],.08f+i*.215f,.65f,.195f,.115f,()=>{giftPage=pick;DrawMenu();},i==giftPage);}
            Shelf(.64f);
            var selected=RewardPicture(GiftAssets[giftPage],new Vector2(.25f,.435f),new Vector2(.75f,.635f));
            selected.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,2),0,1.5f);
            Text("Gift parts",menuRoot,new Vector2(.08f,.33f),new Vector2(.92f,.42f),22,owned?"You made it!":new UiCopy("Parts found: {0} / 3",Rewards.Fragments(id)));
            Text("Gift plan",menuRoot,new Vector2(.08f,.23f),new Vector2(.92f,.32f),18,owned?"A little keepsake for Mika.":"Three parts make this gift.");
            if(owned)MenuButton(Rewards.EquippedGift==id?"With Mika":"Take along",.14f,()=>{Rewards.Equip(id);PersistProgress();DrawMenu();},true);
            else if(Rewards.EarnedFragments(id)>=3)MenuButton("Open parcel",.14f,ShowParcel,true);
            else MenuButton(Rewards.SelectedGift==id?"Next gift":"Choose this gift",.14f,()=>{Rewards.SelectGift(id);PersistProgress();DrawMenu();},true);
            MenuButton("Back",.025f,ShowTravelBag);
        }
        void DrawWorkshop()
        {
            int complete=Rewards.WorkshopCount(workshopPage,OrderedNodes);bool built=complete==10;
            CraftTitle(WorkshopNames[workshopPage],built?"Ready for the islands!":"This is what we are building.");
            for(int i=0;i<4;i++){int pick=i;ShelfChoice("",WorkshopAssets[i],.08f+i*.215f,.65f,.195f,.115f,()=>{workshopPage=pick;DrawMenu();},i==workshopPage);}
            Shelf(.64f);
            var picture=RewardPicture(WorkshopAssets[workshopPage],new Vector2(.20f,.39f),new Vector2(.80f,.63f));
            picture.color=built?Color.white:new Color(.82f,.85f,.77f,.85f);
            picture.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1),0,.5f);
            Text("Workshop progress",menuRoot,new Vector2(.10f,.29f),new Vector2(.90f,.38f),22,new UiCopy("Lights restored: {0} / 10",complete));
            Text("Workshop range",menuRoot,new Vector2(.10f,.20f),new Vector2(.90f,.29f),18,new UiCopy("From stop {0} to stop {1}",workshopPage*10+1,workshopPage*10+10));
            MenuButton("Back to map",.10f,ShowJourney,true);
            MenuButton("Travel bag",.01f,ShowTravelBag);
        }
        void DrawRequests()
        {
            string[] friends={"Pip","Mika","Rue"};int[] needs={1,2,3};
            string[] requests={"Mend a board, then bring the row home.","Use a bridge and finish two different puzzles.","Bring home stamps from three different puzzles."};
            int progress=Math.Min(needs[requestPage],Rewards.QuestCount(requestPage));
            MenuTitle(friends[requestPage],"A friend's request");
            Picture("Request friend",menuRoot,new Vector2(.27f,.43f),new Vector2(.73f,.72f),Resources.Load<Sprite>("CascadeArt/Journey/"+friends[requestPage]),true);
            MetaArrows(()=>{requestPage=(requestPage+2)%3;DrawMenu();},()=>{requestPage=(requestPage+1)%3;DrawMenu();});
            Text("Request detail",menuRoot,new Vector2(.09f,.29f),new Vector2(.91f,.42f),20,requests[requestPage]);
            Text("Request progress",menuRoot,new Vector2(.10f,.20f),new Vector2(.90f,.29f),20,progress==needs[requestPage]?"Thank you! You helped.":new UiCopy("Done: {0} / {1}",progress,needs[requestPage]));
            MenuButton(requestPage==2?"Stamp trails":"Try a puzzle",.10f,()=>{if(requestPage==2)ShowContracts();else{LoadNode(requestPage==0?"C18":"C33");ShowGame();}},true);
            MenuButton("Back",.01f,ShowJournal);
        }
        void DrawParcel()
        {
            var receipt=Rewards.Parcels.Single(p=>p.Order==parcelOrder);int index=Array.IndexOf(JourneyRewards.Gifts,receipt.GiftId);
            bool opened=parcelOpened||receipt.Claimed;
            MenuTitle(opened?"A little piece of home":"A parcel for Mika",new UiCopy("Inside: a part for {0}",new UiCopy(GiftNames[index])));
            var picture=RewardPicture(opened?GiftAssets[index]:"Parcel",new Vector2(.18f,.38f),new Vector2(.82f,.70f));
            Text("Parcel promise",menuRoot,new Vector2(.10f,.25f),new Vector2(.90f,.36f),20,opened?
                Rewards.Owns(receipt.GiftId)?"Your gift is ready!":new UiCopy("Parts found: {0} / 3",Rewards.Fragments(receipt.GiftId)):
                "You earned this with three new lights.");
            MenuButton(opened?"Keep going":"Unwrap",.14f,()=>{
                if(opened){ShowJournal();return;}
                if(Rewards.ClaimParcel(parcelOrder)){PersistProgress();parcelOpened=true;feedback.Sound(ToyCue.Success);DrawMenu();}
            },true);
            if(opened&&!ReducedMotion)StartCoroutine(GiftArrival(picture.rectTransform));
            MenuButton("Back",.025f,ShowTravelBag);
        }
        IEnumerator GiftArrival(RectTransform picture)
        {
            for(float t=0;t<.28f;t+=Time.unscaledDeltaTime){if(picture==null)yield break;picture.localScale=Vector3.one*(1+.07f*Mathf.Sin(t/.28f*Mathf.PI));yield return null;}
            if(picture!=null)picture.localScale=Vector3.one;
        }
        static int StampGoal(LevelDefinition level)=>level.Floors.Select((f,i)=>f.OptionalStamp==null?0:1<<i).Aggregate(0,(a,b)=>a|b);
        LevelContent[] StampTrails()
        {
            if(contracts==null)contracts=Catalog.PrimaryLevels.Where(c=>StampGoal(c.Definition)!=0&&
                PuzzleSolver.FindShortest(new PuzzleResolver(c.Definition),goal:new PuzzleGoal(StampGoal(c.Definition))).Status==SearchStatus.Solved).ToArray();
            return contracts;
        }
        public void StartStampTrail(string campaignNode)
        {
            var content=StampTrails().Single(c=>c.Definition.CampaignNodeId==campaignNode);
            if(PreferredMode!=PuzzleMode.Shapes&&Catalog.TryGetLocalizedBinding(campaignNode,Preferences.PuzzleLocale,out var localized))content=localized;
            StartExactTrail(content,StampGoal(content.Definition));
        }
        void StartExactTrail(LevelContent content,int stampGoal)
        {
            var goal=new PuzzleGoal(stampGoal);
            if(PuzzleSolver.FindShortest(new PuzzleResolver(content.Definition),goal:goal).Status!=SearchStatus.Solved)return;
            MountAttempt(new AttemptService(content.Definition,content.Locale,goal));
            levelLabel.SetCopy(new UiCopy("Stamp trail {0}",content.Definition.CampaignNodeId.TrimStart('C','0')));PersistProgress();ShowGame();
        }
        void DrawContracts()
        {
            var choices=StampTrails();MenuTitle("Stamp trails","Bring the row home with its stamp.");
            if(choices.Length==0){MenuButton("Back",.10f,ShowTravelBag);return;}
            contractPage=Math.Min(contractPage,choices.Length-1);var content=choices[contractPage];
            MiniMachine(Rect("Stamp target",menuRoot,new Vector2(.23f,.48f),new Vector2(.77f,.65f)),content.Definition.InitialSymbolIds,content.Definition.TargetSymbolIds);
            MetaArrows(()=>{contractPage=(contractPage+choices.Length-1)%choices.Length;DrawMenu();},()=>{contractPage=(contractPage+1)%choices.Length;DrawMenu();});
            Text("Contract stop",menuRoot,new Vector2(.10f,.34f),new Vector2(.90f,.45f),23,new UiCopy("Stamp trail {0}",content.Definition.CampaignNodeId.TrimStart('C','0')));
            Text("Contract freedom",menuRoot,new Vector2(.10f,.22f),new Vector2(.90f,.34f),18,"This is an extra challenge. Your path stays open.");
            MenuButton("Try this trail",.12f,()=>StartStampTrail(content.Definition.CampaignNodeId),true);MenuButton("Back",.01f,ShowTravelBag);
        }
        void DrawRecords()
        {
            var records=Rewards.Records.OrderBy(r=>r.PuzzleId,StringComparer.Ordinal).ThenBy(r=>r.StampGoal).ToArray();
            if(records.Length==0){MenuTitle("My routes","Your own best paths.");Text("Empty journal",menuRoot,new Vector2(.10f,.38f),new Vector2(.90f,.65f),22,"Finish a puzzle to leave a note here.");MenuButton("Back",.10f,ShowTravelBag);return;}
            recordPage=Math.Min(recordPage,records.Length-1);var record=records[recordPage];
            var content=Catalog.PrimaryLevels.Concat(Catalog.LocalizedLevels).Concat(Catalog.BonusLevels).FirstOrDefault(c=>c.Definition.PuzzleId==record.PuzzleId&&c.Definition.ContentHash==record.ContentHash);
            MenuTitle("My routes",content==null?"A note from an earlier journey.":new UiCopy("Level {0}",content.Definition.CampaignNodeId.TrimStart('C','0')));
            if(content!=null)MiniMachine(Rect("Record target",menuRoot,new Vector2(.23f,.47f),new Vector2(.77f,.64f)),content.Definition.InitialSymbolIds,content.Definition.TargetSymbolIds);
            MetaArrows(()=>{recordPage=(recordPage+records.Length-1)%records.Length;DrawMenu();},()=>{recordPage=(recordPage+1)%records.Length;DrawMenu();});
            Text("Record identity",menuRoot,new Vector2(.08f,.61f),new Vector2(.92f,.70f),18,UiCopy.Join(" · ",new[]{new UiCopy(record.Mode),LanguageName(record.Locale),new UiCopy(record.StampGoal==0?"Delivery":"With stamp")}));
            Text("Record route",menuRoot,new Vector2(.08f,.36f),new Vector2(.92f,.45f),23,new UiCopy("Best route: {0} moves",record.BestRoute));
            Text("Record help",menuRoot,new Vector2(.10f,.26f),new Vector2(.90f,.35f),18,record.BestWithoutHint.HasValue?new UiCopy("Without a hint: {0}",record.BestWithoutHint.Value):"Try again without a hint.");
            Text("Record clean",menuRoot,new Vector2(.10f,.19f),new Vector2(.90f,.27f),16,record.BestClean.HasValue?new UiCopy("Without hint or Undo: {0}",record.BestClean.Value):"Undo is always welcome.");
            if(content!=null)MenuButton("Try again",.10f,()=>{if(record.StampGoal!=0)StartExactTrail(content,record.StampGoal);else StartContent(content);},true);
            MenuButton("Back",.01f,ShowTravelBag);
        }
    }
}
