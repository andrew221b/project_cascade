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
        GameScreen collectionReturn=GameScreen.TravelBag,workshopReturn=GameScreen.TravelBag;
        LevelContent[] contracts;
        static readonly string[] GiftNames={"Leaf pin","Sun button","Sky ribbon","Moon patch","Wind chime","Rain badge","Copper acorn","Beacon brooch"};
        static readonly string[] GiftAssets={"LeafPin","SunButton","SkyRibbon","MoonPatch"};
        static readonly string[] WorkshopNames={"Bridge lantern","Rain pump","Post glider","Beacon model"};
        static readonly string[] WorkshopAssets={"BridgeLantern","RainPump","PostGlider","BeaconModel"};
        static readonly string[] RankNames={"Apprentice","Garden friend","Mill keeper","Pathfinder","Beacon keeper"};
        ParcelReceipt WaitingParcel=>Rewards.Parcels.FirstOrDefault(p=>!p.Claimed);
        string[] OrderedNodes=>Catalog.PrimaryLevels.Select(c=>c.Definition.CampaignNodeId).ToArray();
        GameScreen bagReturnScreen=GameScreen.Home,journalReturnScreen=GameScreen.TravelBag;
        ShopLocation bagShopLocation,collectionShopLocation,workshopShopLocation;
        GameScreen parcelReturnScreen=GameScreen.Journal;
        public void ShowTravelBag()
        {
            if(CurrentScreen==GameScreen.Shop&&shopReturnScreen==GameScreen.TravelBag){BackFromShop();return;}
            if(CurrentScreen==GameScreen.Home||CurrentScreen==GameScreen.Shop||CurrentScreen==GameScreen.JourneyMap){
                bagReturnScreen=CurrentScreen;
                if(CurrentScreen==GameScreen.Shop)bagShopLocation=CaptureShop();
            }
            SetScreen(GameScreen.TravelBag);
        }
        void BackFromBag()
        {
            if(bagReturnScreen==GameScreen.Shop){RestoreShop(bagShopLocation);return;}
            SetScreen(bagReturnScreen==GameScreen.TravelBag?GameScreen.Home:bagReturnScreen);
        }
        public void ShowJournal(){if(CurrentScreen==GameScreen.TravelBag||CurrentScreen==GameScreen.JourneyEnd)journalReturnScreen=CurrentScreen;SetScreen(GameScreen.Journal);}
        public void ShowCollection()
        {
            if(CurrentScreen==GameScreen.Shop&&shopReturnScreen==GameScreen.Collection){BackFromShop();return;}
            if(CurrentScreen!=GameScreen.Collection){
                collectionReturn=CurrentScreen;
                if(CurrentScreen==GameScreen.Shop)collectionShopLocation=CaptureShop();
                giftPage=Math.Max(0,Array.IndexOf(JourneyRewards.Gifts,Rewards.SelectedGift));
            }
            SetScreen(GameScreen.Collection);
        }
        void BackFromCollection()
        {
            if(collectionReturn==GameScreen.Shop){RestoreShop(collectionShopLocation);return;}
            SetScreen(collectionReturn);
        }
        public void ShowWorkshop()
        {
            if(CurrentScreen==GameScreen.Shop&&shopReturnScreen==GameScreen.Workshop){BackFromShop();return;}
            if(CurrentScreen!=GameScreen.Workshop){
                workshopReturn=CurrentScreen;
                if(CurrentScreen==GameScreen.Shop)workshopShopLocation=CaptureShop();
            }
            SetScreen(GameScreen.Workshop);
        }
        void BackFromWorkshop()
        {
            if(workshopReturn==GameScreen.Shop){RestoreShop(workshopShopLocation);return;}
            SetScreen(workshopReturn);
        }
        public void ShowRequests(){SetScreen(GameScreen.Requests);}
        public void ShowRecords(){recordPage=0;SetScreen(GameScreen.Records);}
        public void ShowContracts(){contractPage=0;SetScreen(GameScreen.Contracts);}
        public void ShowParcel()
            =>OpenParcel(false);
        void OpenParcel(bool returnToMap)
        {
            var receipt=returnToMap&&profile.Completion?.ParcelOrder>0?Rewards.Parcels.FirstOrDefault(p=>p.Order==profile.Completion.ParcelOrder):WaitingParcel;
            if(receipt==null){if(returnToMap)FinishCompletion();return;}
            parcelReturnsToMap=returnToMap;
            if(!returnToMap&&CurrentScreen!=GameScreen.Parcel)parcelReturnScreen=CurrentScreen;
            parcelOrder=receipt.Order;parcelOpened=false;SetScreen(GameScreen.Parcel);
        }
        void DrawTravelBag()
        {
            DrawCraftBag();
        }
        void DrawJournal()
        {
            MenuTitle("My journal",RankNames[Rewards.Rank]);
            Picture("Mika reads",menuRoot,new Vector2(.10f,.51f),new Vector2(.46f,.805f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true).gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1.5f),0,.7f);
            MenuReadingSurface("Journal visits cloth",new Vector2(.48f,.455f),new Vector2(.88f,.72f));
            Text("Journal lights",menuRoot,new Vector2(.52f,.73f),new Vector2(.90f,.80f),19,new UiCopy("Lights restored: {0}",JourneyClearCount)).font=art.titleFont;
            Text("Visit stamps",menuRoot,new Vector2(.52f,.60f),new Vector2(.90f,.71f),16,new UiCopy("Visits: {0} / 7\nCome back when you like.",Rewards.VisitDates.Count));
            UiCopy order=Rewards.AllGiftsEarned?"Every gift part is found!":new UiCopy("New lights for a parcel: {0} / 3",Rewards.OrderProgress);
            MenuReadingSurface("Current order cloth",new Vector2(.12f,.345f),new Vector2(.88f,.445f));
            Text("Current order",menuRoot,new Vector2(.10f,.335f),new Vector2(.90f,.43f),18,order);
            if(WaitingParcel!=null)MenuButton("Open parcel",.26f,ShowParcel,true);
            else MenuButton("Trophies",.26f,ShowCollection,true);
            MenuButton("Requests",.14f,ShowRequests,false,.08f,.48f);MenuButton("My routes",.14f,ShowRecords,false,.52f,.92f);
            MenuButton("Back",.025f,()=>SetScreen(journalReturnScreen));
        }
        void MetaArrows(Action previousAction,Action nextAction)
        {
            var previous=DirectionButton("Previous",menuRoot,new Vector2(.08f,0),new Vector2(.26f,0),false,previousAction);
            var next=DirectionButton("Next",menuRoot,new Vector2(.74f,0),new Vector2(.92f,0),true,nextAction);
            foreach(var button in new[]{previous,next}){var rect=(RectTransform)button.transform;menuActions.Add(rect);menuBottoms.Add(new MenuBottom{rect=rect,bottom=16});button.GetComponent<UnityEngine.UI.Image>().color=new Color(.84f,.91f,.87f);}
        }
        UnityEngine.UI.Image RewardPicture(string asset,Vector2 min,Vector2 max)
            =>Picture(asset,menuRoot,min,max,Resources.Load<Sprite>("CascadeArt/Workshop/"+asset),true);
        void DrawCollection(){shopItem=giftPage;DrawTravelCounter(false,true);}
        void DrawWorkshop()
        {
            int complete=Rewards.WorkshopCount(workshopPage,OrderedNodes);bool built=complete==10;
            CraftTitle("Workshop",WorkshopNames[workshopPage]);
            for(int i=0;i<4;i++){int pick=i;ShelfChoice("",WorkshopAssets[i],.08f+i*.215f,.655f,.195f,.14f,()=>{workshopPage=pick;DrawMenu();},i==workshopPage);}
            Shelf(.64f);
            var picture=RewardPicture(WorkshopAssets[workshopPage],new Vector2(.15f,.35f),new Vector2(.85f,.63f));
            picture.color=built?Color.white:new Color(.90f,.91f,.84f,1);
            picture.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1),0,.5f);
            MenuReadingSurface("Model progress cloth",new Vector2(.12f,.19f),new Vector2(.88f,.385f));
            Text("Workshop progress",menuRoot,new Vector2(.10f,.282f),new Vector2(.90f,.35f),23,built?"Ready for the islands!":new UiCopy("Lights restored: {0} / 10",complete)).font=art.titleFont;
            WorkshopProgress(complete);
            Text("Workshop range",menuRoot,new Vector2(.10f,.211f),new Vector2(.90f,.255f),20,new UiCopy("From stop {0} to stop {1}",workshopPage*10+1,workshopPage*10+10));
            MenuButton("Back",.025f,BackFromWorkshop);
        }
        void DrawRequests()
        {
            string[] friends={"Pip","Mika","Rue"};int[] needs={1,2,3};
            string[] requests={"Mend a board, then bring the row home.","Use a bridge and finish two different puzzles.","Bring home stamps from three different puzzles."};
            int progress=Math.Min(needs[requestPage],Rewards.QuestCount(requestPage));
            MenuTitle(friends[requestPage],"A friend's request");
            Picture("Request friend",menuRoot,new Vector2(.20f,.49f),new Vector2(.80f,.81f),Resources.Load<Sprite>("CascadeArt/Journey/"+friends[requestPage]),true).gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1.5f),0,.6f);
            MetaArrows(()=>{requestPage=(requestPage+2)%3;DrawMenu();},()=>{requestPage=(requestPage+1)%3;DrawMenu();});
            MenuReadingSurface("Friend request cloth",new Vector2(.12f,.19f),new Vector2(.88f,.425f));
            Text("Request detail",menuRoot,new Vector2(.10f,.31f),new Vector2(.90f,.465f),18,requests[requestPage]);
            Text("Request progress",menuRoot,new Vector2(.10f,.215f),new Vector2(.90f,.28f),19,progress==needs[requestPage]?"Thank you! You helped.":new UiCopy("Done: {0} / {1}",progress,needs[requestPage]));
            var requestExample=Catalog.GetPrimary(requestPage==0?"C18":"C33");
            var tryRequest=MenuButton(requestPage==2?"Stamp trails":"Try a puzzle",.10f,()=>{if(requestPage==2)ShowContracts();else StartContent(requestExample);},true);
            tryRequest.interactable=requestPage==2||IsContentOpen(requestExample);
            MenuButton("Back",.01f,ShowJournal,false,.30f,.70f);
        }
        void DrawParcel()
        {
            var receipt=Rewards.Parcels.Single(p=>p.Order==parcelOrder);int index=Array.IndexOf(JourneyRewards.Gifts,receipt.GiftId);
            bool opened=parcelOpened||receipt.Claimed;
            LoadNavigationArt();
            var ribbon=Picture("Gift ribbon",menuRoot,new Vector2(.06f,.84f),new Vector2(.94f,.978f),CompletionArt(1),true);
            CompletionText("Gift title",ribbon.transform,new Vector2(.16f,.22f),new Vector2(.84f,.80f),23,opened?"A gift for you!":"You earned a gift!");
            CompletionText("Gift name",menuRoot,new Vector2(.12f,.758f),new Vector2(.88f,.827f),20,new UiCopy(GiftNames[index]));
            var parcel=Picture("Illustrated parcel",menuRoot,new Vector2(.14f,.39f),new Vector2(.86f,.73f),opened?CompletionArt(2):Resources.Load<Sprite>("CascadeArt/Workshop/Parcel"),true);

            var picture=opened?RewardPicture(GiftAssets[index],new Vector2(.27f,.535f),new Vector2(.73f,.788f)):null;
            MenuReadingSurface("Parcel note cloth",new Vector2(.10f,.235f),new Vector2(.90f,.369f));
            CompletionText("Parcel promise",menuRoot,new Vector2(.12f,.285f),new Vector2(.88f,.36f),22,opened?
                Rewards.Owns(receipt.GiftId)?"Your gift is ready!":new UiCopy("Parts found: {0} / 3",Rewards.Fragments(receipt.GiftId)):
                "You earned this with three new lights.");
            CompletionText("Gift piece note",menuRoot,new Vector2(.12f,.237f),new Vector2(.88f,.292f),15,
                opened?"Find three parts to make a keepsake for Mika.":"Let's see what is inside.");
            var next=MenuButton(opened?(parcelReturnsToMap?"Back to map":"Keep going"):"Unwrap",.095f,()=>{
                if(opened){if(parcelReturnsToMap)FinishCompletion();else SetScreen(parcelReturnScreen);return;}
                if(Rewards.ClaimParcel(parcelOrder)){PersistProgress();parcelOpened=true;DrawMenu();}
            },true,.15f,.85f);next.name=opened?"Keepgift":"Unwrapgift";next.GetComponent<UnityEngine.UI.Image>().sprite=enamelControls[1];next.GetComponent<UnityEngine.UI.Image>().color=Color.white;
            if(opened){if(!ReducedMotion)StartCoroutine(GiftArrival(picture.rectTransform,parcel));
                menuRoot.gameObject.AddComponent<RewardCelebration>().Initialize(this,null,0,null,0,CompletionArt(3),true,ribbon.rectTransform);}
            if(!parcelReturnsToMap&&!opened)MenuButton("Back",.005f,()=>SetScreen(parcelReturnScreen));
        }
        IEnumerator GiftArrival(RectTransform picture,UnityEngine.UI.Image parcel)
        {
            var rest=picture.anchoredPosition;
            var opened=parcel.sprite;parcel.sprite=Resources.Load<Sprite>("CascadeArt/Workshop/Parcel");var gift=picture.GetComponent<UnityEngine.UI.Image>();gift.color=new Color(1,1,1,0);
            bool openedSound=false,revealSound=false;
            for(float t=0;t<2.35f;){if(picture==null||parcel==null)yield break;
                if(ReducedMotion)break;
                if(!CelebrationMotionAllowed){yield return null;continue;}
                float prepare=Mathf.Clamp01(t/.55f);parcel.rectTransform.localScale=new Vector3(1+.035f*Mathf.Sin(prepare*Mathf.PI),1-.025f*Mathf.Sin(prepare*Mathf.PI),1);
                parcel.rectTransform.localRotation=Quaternion.Euler(0,0,t<.55f?Mathf.Sin(prepare*Mathf.PI*2)*2.2f:Mathf.Sin(Mathf.Clamp01((t-.55f)/.6f)*Mathf.PI)*-1.4f);
                if(t>=.55f){parcel.sprite=opened;if(!openedSound){openedSound=true;feedback.Sound(ToyCue.Stamp);}}
                float p=Mathf.Clamp01((t-.62f)/1.35f),ease=1-Mathf.Pow(1-p,3);gift.color=new Color(1,1,1,Mathf.Clamp01(p*5));
                picture.localScale=Vector3.one*(Mathf.Lerp(.45f,1,ease)+.08f*Mathf.Sin(p*Mathf.PI));
                picture.anchoredPosition=rest+Vector2.down*(1-ease)*62+Vector2.up*Mathf.Sin(p*Mathf.PI)*16;
                picture.localRotation=Quaternion.Euler(0,0,Mathf.Sin(p*Mathf.PI)*-6);
                if(p>.30f&&!revealSound){revealSound=true;feedback.Sound(ToyCue.Success);feedback.Haptic();}
                t+=Mathf.Min(.05f,Time.unscaledDeltaTime);yield return null;}
            if(picture!=null){picture.anchoredPosition=rest;picture.localRotation=Quaternion.identity;}
            if(parcel!=null){parcel.sprite=opened;parcel.rectTransform.localScale=Vector3.one;parcel.rectTransform.localRotation=Quaternion.identity;}
            if(picture!=null){picture.localScale=Vector3.one;gift.color=Color.white;}
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
            if(HasResumableJourneyAttempt)return;
            var goal=new PuzzleGoal(stampGoal);
            if(PuzzleSolver.FindShortest(new PuzzleResolver(content.Definition),goal:goal).Status!=SearchStatus.Solved)return;
            MountAttempt(new AttemptService(content.Definition,content.Locale,goal));
            levelLabel.SetCopy(new UiCopy("Stamp trail {0}",content.Definition.CampaignNodeId.TrimStart('C','0')));PersistProgress();ShowGame();
        }
        void DrawContracts()
        {
            var choices=StampTrails();MenuTitle("Stamp trails","Bring the row home with its stamp.");
            if(choices.Length==0){MenuButton("Back",.10f,ShowRequests);return;}
            contractPage=Math.Min(contractPage,choices.Length-1);var content=choices[contractPage];
            MiniMachine(Rect("Stamp target",menuRoot,new Vector2(.17f,.52f),new Vector2(.83f,.78f)),content.Definition.InitialSymbolIds,content.Definition.TargetSymbolIds);
            MetaArrows(()=>{contractPage=(contractPage+choices.Length-1)%choices.Length;DrawMenu();},()=>{contractPage=(contractPage+1)%choices.Length;DrawMenu();});
            MenuReadingSurface("Stamp trail note cloth",new Vector2(.12f,.215f),new Vector2(.88f,.455f));
            Text("Contract stop",menuRoot,new Vector2(.10f,.40f),new Vector2(.90f,.47f),22,new UiCopy("Stamp trail {0}",content.Definition.CampaignNodeId.TrimStart('C','0')));
            Text("Contract freedom",menuRoot,new Vector2(.10f,.235f),new Vector2(.90f,.365f),18,HasResumableJourneyAttempt?"Finish your current level before starting a stamp trail.":"This is an extra challenge. Your path stays open.");
            var start=MenuButton(HasResumableJourneyAttempt?"Finish current level first":"Try this trail",.12f,()=>StartStampTrail(content.Definition.CampaignNodeId),true);
            start.name="Trythistrail";start.interactable=!HasResumableJourneyAttempt;
            MenuButton("Back",.01f,ShowRequests,false,.30f,.70f);
        }
        void DrawRecords()
        {
            var records=profile.ModeRecords.OrderBy(r=>r.PuzzleId,StringComparer.Ordinal).ThenBy(r=>r.StampGoal).ToArray();
            if(records.Length==0){MenuTitle("My routes","Your own best paths.");MenuReadingSurface("Empty route book cloth",new Vector2(.12f,.38f),new Vector2(.88f,.65f));Text("Empty journal",menuRoot,new Vector2(.16f,.40f),new Vector2(.84f,.63f),22,"Finish a puzzle to leave a note here.");MenuButton("Back",.10f,ShowJournal);return;}
            recordPage=Math.Min(recordPage,records.Length-1);var record=records[recordPage];
            var content=Catalog.PrimaryLevels.Concat(Catalog.LocalizedLevels).Concat(Catalog.BonusLevels).FirstOrDefault(c=>c.Definition.PuzzleId==record.PuzzleId&&c.Definition.ContentHash==(record.SourceHash??record.ContentHash));
            MenuTitle("My routes",content==null?"A note from an earlier journey.":new UiCopy(record.StageCount==3?"Level {0} · 3 stages":"Level {0}",content.Definition.CampaignNodeId.TrimStart('C','0')));
            if(content!=null)MiniMachine(Rect("Record target",menuRoot,new Vector2(.19f,.49f),new Vector2(.81f,.72f)),content.Definition.InitialSymbolIds,content.Definition.TargetSymbolIds);
            MetaArrows(()=>{recordPage=(recordPage+records.Length-1)%records.Length;DrawMenu();},()=>{recordPage=(recordPage+1)%records.Length;DrawMenu();});
            MenuReadingSurface("Route identity cloth",new Vector2(.12f,.655f),new Vector2(.88f,.72f));
            var identity=Text("Record identity",menuRoot,new Vector2(.12f,.73f),new Vector2(.88f,.825f),16,UiCopy.Join(" · ",new[]{new UiCopy(record.Mode),LanguageName(record.Locale),new UiCopy(record.StampGoal==0?"Delivery":"With stamp")}));
            identity.enableAutoSizing=true;identity.fontSizeMin=15*MenuUnit;identity.fontSizeMax=17*MenuUnit;identity.textWrappingMode=TMPro.TextWrappingModes.Normal;
            MenuReadingSurface("Route notes cloth",new Vector2(.12f,.18f),new Vector2(.88f,.455f));
            Text("Record route",menuRoot,new Vector2(.10f,.39f),new Vector2(.90f,.47f),22,new UiCopy("Best route: {0} moves",record.BestRoute));
            Text("Record help",menuRoot,new Vector2(.10f,.30f),new Vector2(.90f,.37f),18,record.BestWithoutHint.HasValue?new UiCopy("Without a hint: {0}",record.BestWithoutHint.Value):"Try again without a hint.");
            Text("Record clean",menuRoot,new Vector2(.10f,.23f),new Vector2(.90f,.28f),16,record.BestClean.HasValue?new UiCopy("Without hint or Undo: {0}",record.BestClean.Value):"Two free undos per stage. Only on the current board.");
            if(content!=null){
                bool blockedTrail=record.StageCount!=3&&record.StampGoal!=0&&HasResumableJourneyAttempt;
                var replay=MenuButton(blockedTrail?"Finish current level first":record.StageCount==3?"Play this level":"Try again",.10f,()=>{
                    if(record.StageCount==3)StartContent(content);
                    else if(record.StampGoal!=0)StartExactTrail(content,record.StampGoal);else StartContent(content);
                },true);
                replay.name=record.StageCount==3?"Playthislevel":"Tryagain";replay.interactable=!blockedTrail;
            }
            MenuButton("Back",.01f,ShowJournal,false,.30f,.70f);
        }
    }
}
