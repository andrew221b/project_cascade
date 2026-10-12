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
        int giftPage,workshopPage,recordPage,contractPage,parcelOrder;
        bool parcelOpened,parcelAnimationShown;
        GameScreen collectionReturn=GameScreen.TravelBag,workshopReturn=GameScreen.TravelBag;
        LevelContent[] contracts;
        static readonly string[] GiftNames={"Leaf pin","Sun button","Sky ribbon","Moon patch","Wind chime","Rain badge","Copper acorn","Beacon brooch","Sky pilot","Moth mask","Garden festival","Cloud gardener","Explorer hat","Frog suit","Star knight","Fox mask","Mushroom hat","Bee costume","Winter hood","Sky pirate","Cloud jester","Star mage","Paper crane","Little comet","Dew bell","Night flower","Cloud kite","Fern compass","Amber moth","Moon boat"};
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
                bagReturnScreen=CurrentScreen==GameScreen.Shop?GameScreen.Home:CurrentScreen;
                if(CurrentScreen==GameScreen.Shop)bagShopLocation=CaptureShop();
            }
            SetScreen(GameScreen.TravelBag);
        }
        void BackFromBag()
        {
            if(bagReturnScreen==GameScreen.Shop){RestoreShop(bagShopLocation);return;}
            SetScreen(bagReturnScreen==GameScreen.TravelBag?GameScreen.Home:bagReturnScreen);
        }
        public void ShowJournal(){if(CurrentScreen==GameScreen.TravelBag||CurrentScreen==GameScreen.JourneyEnd||CurrentScreen==GameScreen.Home)journalReturnScreen=CurrentScreen;SetScreen(GameScreen.Journal);}
        public void ShowCollection()
        {
            if(CurrentScreen==GameScreen.Shop&&shopReturnScreen==GameScreen.Collection){BackFromShop();return;}
            if(CurrentScreen!=GameScreen.Collection){
                collectionReturn=CurrentScreen;
                if(CurrentScreen==GameScreen.Shop)collectionShopLocation=CaptureShop();
                giftPage=Math.Max(0,Array.IndexOf(JourneyRewards.Gifts,Rewards.SelectedGift));
            }
            shopDetailOpen=false;SetScreen(GameScreen.Collection);
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
        public void ShowRequests(){ShowJourney();}
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
            parcelOrder=receipt.Order;parcelOpened=false;parcelAnimationShown=false;SetScreen(GameScreen.Parcel);
        }
        void DrawTravelBag()
        {
            DrawCraftBag();
        }
        void DrawJournal()
        {
            MenuTitle("My journal","");
            Picture("Mika reads",menuRoot,new Vector2(.08f,.50f),new Vector2(.46f,.85f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true).gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1.5f),0,.7f);
            PaperFact("Restored lights",new Vector2(.49f,.68f),new Vector2(.945f,.81f),RewardEmblem(1),"Lights",JourneyClearCount.ToString());
            PaperFact("Visit stamps",new Vector2(.49f,.53f),new Vector2(.945f,.66f),ResultMedal(3),"Visits",Rewards.VisitDates.Count.ToString());
            PaperFact("Next parcel",new Vector2(.055f,.35f),new Vector2(.945f,.47f),Resources.Load<Sprite>("CascadeArt/Workshop/Parcel"),"Next parcel",Rewards.OrderProgress+" / "+Rewards.NextParcelGoal,true);
            MenuButton(WaitingParcel!=null?"Open parcel":"Trophies",.235f,WaitingParcel!=null?(System.Action)ShowParcel:ShowCollection,true);
            MenuButton("Back",.025f,()=>SetScreen(journalReturnScreen));
        }
        void MetaArrows(Action previousAction,Action nextAction)
        {
            var previous=DirectionButton("Previous",menuRoot,new Vector2(.08f,0),new Vector2(.26f,0),false,previousAction);
            var next=DirectionButton("Next",menuRoot,new Vector2(.74f,0),new Vector2(.92f,0),true,nextAction);
            foreach(var button in new[]{previous,next}){var rect=(RectTransform)button.transform;menuActions.Add(rect);menuBottoms.Add(new MenuBottom{rect=rect,bottom=16});button.GetComponent<UnityEngine.UI.Image>().color=Color.clear;}
        }
        void RecordArrows(Action previousAction,Action nextAction)
        {
            var ribbon=menuRoot.Find("Menu description papyrus") as RectTransform;
            if(ribbon!=null){
                ribbon.anchorMin=new Vector2(.15f,ribbon.anchorMin.y);
                ribbon.anchorMax=new Vector2(.85f,ribbon.anchorMax.y);
            }
            void Add(string name,float x,bool right,Action action){
                var button=DirectionButton(name,menuRoot,new Vector2(x,.855f),new Vector2(x,.855f),right,action);
                var rect=(RectTransform)button.transform;rect.sizeDelta=Vector2.one*48*MenuUnit;
                menuSquares.Add(new System.Collections.Generic.KeyValuePair<RectTransform,float>(rect,48));
                var shadow=Rect(name+" outline",rect,new Vector2(.14f,.22f),new Vector2(.86f,.78f)).gameObject.AddComponent<ChevronGraphic>();
                shadow.PointsRight=right;shadow.Weight=0;shadow.color=new Color(.07f,.16f,.16f,.95f);
                shadow.raycastTarget=false;shadow.transform.SetAsFirstSibling();
                var arrow=rect.Find("Navigation arrow") as RectTransform;
                if(arrow!=null){arrow.anchorMin=new Vector2(.19f,.27f);arrow.anchorMax=new Vector2(.81f,.73f);}
            }
            Add("Previous record",.245f,false,previousAction);
            Add("Next record",.755f,true,nextAction);
        }
        UnityEngine.UI.Image RewardPicture(string asset,Vector2 min,Vector2 max)
            =>Picture(asset,menuRoot,min,max,Resources.Load<Sprite>("CascadeArt/Workshop/"+asset),true);
        void DrawCollection(){shopItem=giftPage;DrawTravelCounter(false,true);}
        void DrawWorkshop()
        {
            int complete=Rewards.WorkshopCount(workshopPage,OrderedNodes);bool built=complete==10;
            CraftTitle("Workshop","");
            for(int i=0;i<4;i++){int pick=i;ShelfChoice("",WorkshopAssets[i],.08f+i*.215f,.655f,.195f,.14f,()=>{workshopPage=pick;DrawMenu();},i==workshopPage);}
            Shelf(.64f);
            var picture=RewardPicture(WorkshopAssets[workshopPage],new Vector2(.15f,.35f),new Vector2(.85f,.63f));
            picture.color=built?Color.white:new Color(.90f,.91f,.84f,1);
            picture.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1),0,.5f);
            ObjectNameStrip("Workshop model name",new Vector2(.055f,.28f),new Vector2(.945f,.35f),WorkshopNames[workshopPage]);
            PaperFact("Workshop progress",new Vector2(.055f,.145f),new Vector2(.945f,.26f),RewardEmblem(1),built?(UiCopy)"Ready!":"Lights",complete+" / 10");
            MenuButton("Back",.025f,BackFromWorkshop);
        }
        void DrawParcel()
        {
            var receipt=Rewards.Parcels.Single(p=>p.Order==parcelOrder);int index=Array.IndexOf(ShopCatalog.TrophyIds,receipt.GiftId);
            bool bonus=!string.IsNullOrEmpty(receipt.Kind),coins=receipt.Kind=="coins"||receipt.DuplicateCoins;
            bool toolPrize=receipt.Kind=="tool"&&ShopCatalog.TryTool(receipt.GiftId,out _);
            ShopCatalog.TryTool(receipt.GiftId,out var tool);
            bool opened=parcelOpened||receipt.Claimed;
            LoadNavigationArt();
            var ribbon=Picture("Gift ribbon",menuRoot,new Vector2(.06f,.83f),new Vector2(.94f,.978f),CompletionArt(1),true);
            CompletionText("Gift title",ribbon.transform,new Vector2(.23f,.22f),new Vector2(.77f,.80f),26,"You earned",CelebrationType.PaperTitle);
            if(opened)ObjectNameStrip("Gift name",new Vector2(.08f,.754f),new Vector2(.92f,.825f),
                coins?new UiCopy("{0} coins",JourneyRewards.ParcelCoins(receipt)):toolPrize?ToolTitle((int)tool):TrophyTitle(receipt.GiftId));
            var parcel=Picture("Illustrated parcel",menuRoot,new Vector2(.14f,.39f),new Vector2(.86f,.73f),opened?CompletionArt(2):Resources.Load<Sprite>("CascadeArt/Workshop/Parcel"),true);

            var picture=opened?Picture("Parcel prize",menuRoot,new Vector2(.27f,.535f),new Vector2(.73f,.788f),
                coins?RewardEmblem(0):toolPrize?ToolArt((int)tool):TrophyArt(index),true):null;
            bool showPartGuidance=opened&&receipt.Claimed&&!bonus&&!coins&&!Rewards.Owns(receipt.GiftId);
            bool showToolLocation=opened&&receipt.Claimed&&toolPrize&&!coins;
            bool showTrophyLocation=opened&&receipt.Claimed&&!coins&&!toolPrize&&OwnsGift(receipt.GiftId);
            if(showTrophyLocation)ReadingStrip("Collected trophy location",new Vector2(.08f,.275f),new Vector2(.92f,.405f),"My things → Trophies\nTurn this look on or off there.",16,true);
            if(showToolLocation||showPartGuidance){
                if(showToolLocation)ReadingStrip("Parcel promise",new Vector2(.08f,.275f),new Vector2(.92f,.355f),"Added to your bag",18,true);
                else {
                    int pieces=Rewards.Fragments(receipt.GiftId);
                    for(int i=0;i<3;i++){
                        float x=.26f+i*.17f;
                        var piece=Picture("Parcel part "+i,menuRoot,new Vector2(x,.285f),new Vector2(x+.14f,.365f),TrophyArt(index),true);
                        if(i>=pieces)piece.material=TrophyUnavailableMaterial();
                    }
                }
            }
            if(opened&&!coins&&JourneyRewards.ParcelCoins(receipt)>0)
                RecordMetric("Parcel coins",menuRoot,new Vector2(.35f,.19f),new Vector2(.65f,.255f),RewardEmblem(0),JourneyRewards.ParcelCoins(receipt).ToString());
            var note=!opened||!receipt.Claimed?CompletionText("Gift piece note",menuRoot,new Vector2(.12f,.237f),new Vector2(.88f,.292f),15,UiCopy.Raw("")):null;
            bool canWatchParcel=opened&&adProvider?.RewardReady==true&&RewardedOffersAllowed&&!saveBlocked&&!HasSaveError
                &&monetization!=null&&!monetization.Ledger.HideVideoOffers&&monetization.CanReceiveGrant
                &&!monetization.HasParcelBonus(MonetizationLedger.ParcelSubject(parcelOrder));
            UiCopy actionLabel=!opened?(UiCopy)"Open parcel":receipt.Claimed?(UiCopy)"Continue":"Claim";
            System.Action parcelAction=()=>{
                if(!opened){parcelOpened=true;DrawMenu();return;}
                if(!receipt.Claimed){
                    if(!saveBlocked&&GameEconomy?.TryClaimParcel(parcelOrder,Rewards)==true){CreateGlobalCosmeticAmbiance();RefreshFieldTools();DrawMenu();}
                    else note.SetCopy("Could not save. Try again.");
                    return;
                }
                if(parcelReturnsToMap)FinishCompletion();else SetScreen(parcelReturnScreen);
            };
            var next=MenuButton(actionLabel,.025f,parcelAction,true,canWatchParcel ? .025f : MenuColumnInset,canWatchParcel ? .49f : 1-MenuColumnInset);
            next.name=!opened?"Openparcelgift":receipt.Claimed?"Keepgift":"Claimgift";next.GetComponent<UnityEngine.UI.Image>().sprite=enamelControls[0];next.GetComponent<UnityEngine.UI.Image>().color=Color.white;
            next.interactable=!saveBlocked;
            if(canWatchParcel){var watch=MenuButton("Watch ad +40 coins",.025f,()=>WatchParcelBonus(parcelOrder),false,.51f,.975f);watch.name="Parcel bonus video";watch.interactable=adProvider?.RewardReady==true;}
            if(showTrophyLocation){
                var view=MenuButton("View in bag",.105f,()=>ShowCollectedTrophy(receipt.GiftId),true,.24f,.76f);
                view.name="View collected trophy";view.interactable=!saveBlocked;
            }
            if(opened&&!parcelAnimationShown){parcelAnimationShown=true;if(!ReducedMotion)StartCoroutine(GiftArrival(picture.rectTransform,parcel));
                menuRoot.gameObject.AddComponent<RewardCelebration>().Initialize(this,null,0,null,0,CompletionArt(3),true,ribbon.rectTransform);}
        }
        void ShowCollectedTrophy(string id)
        {
            if(!OwnsGift(id))return;
            ShowCollection();shopItem=giftPage=Array.IndexOf(ShopCatalog.TrophyIds,id);shopDetailOpen=true;DrawMenu();
        }
        IEnumerator GiftArrival(RectTransform picture,UnityEngine.UI.Image parcel)
        {
            var rest=picture.anchoredPosition;
            var opened=parcel.sprite;parcel.sprite=Resources.Load<Sprite>("CascadeArt/Workshop/Parcel");var gift=picture.GetComponent<UnityEngine.UI.Image>();gift.color=new Color(1,1,1,0);
            bool openedSound=false,revealSound=false;
            // A parcel should have a visible breath before it opens. This only
            // presents an already-earned receipt; opening state remains owned by
            // the profile before this coroutine begins.
            const float anticipateEnd=.86f,openAt=.94f,revealStart=1.10f,revealDuration=1.34f,duration=3.25f;
            for(float t=0;t<duration;){if(picture==null||parcel==null)yield break;
                if(ReducedMotion)break;
                if(!CelebrationMotionAllowed){yield return null;continue;}
                float prepare=Mathf.Clamp01(t/anticipateEnd),pulse=Mathf.Sin(prepare*Mathf.PI*3f)*(1-prepare*.35f);
                parcel.rectTransform.localScale=new Vector3(1+.018f*pulse,1-.014f*pulse,1);
                parcel.rectTransform.localRotation=Quaternion.Euler(0,0,t<openAt?Mathf.Sin(prepare*Mathf.PI*3f)*(1.2f+prepare*2.5f):0);
                float opening=Mathf.Clamp01((t-openAt)/.32f);
                if(t>=openAt){parcel.sprite=opened;parcel.rectTransform.localScale=new Vector3(1+.085f*Mathf.Sin(opening*Mathf.PI),1-.052f*Mathf.Sin(opening*Mathf.PI),1);if(!openedSound){openedSound=true;feedback.Sound(ToyCue.Stamp);}}
                float p=Mathf.Clamp01((t-revealStart)/revealDuration),back=1+2.70158f*Mathf.Pow(p-1,3)+1.70158f*Mathf.Pow(p-1,2);
                float alpha=Mathf.SmoothStep(0,1,Mathf.Clamp01(p/.26f));gift.color=new Color(1,1,1,alpha);
                picture.localScale=Vector3.one*(Mathf.Lerp(.16f,1,back)+.11f*Mathf.Sin(p*Mathf.PI));
                picture.anchoredPosition=rest+Vector2.down*(1-back)*118+Vector2.up*Mathf.Sin(p*Mathf.PI)*28;
                picture.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(-14,0,back)+Mathf.Sin(p*Mathf.PI*2)*(1-p)*4);
                if(p>.30f&&!revealSound){revealSound=true;feedback.Sound(ToyCue.Success);feedback.Haptic(ToyHapticMoment.Parcel);}
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
            levelLabel.SetCopy(new UiCopy("Stamp trail {0}",DisplayLevelNumber(content.Definition.CampaignNodeId)));PersistProgress();ShowGame();
        }
        void DrawContracts()
        {
            var choices=StampTrails();MenuTitle("Stamp trails","");
            if(choices.Length==0){MenuButton("Back",.10f,ShowRequests);return;}
            contractPage=Math.Min(contractPage,choices.Length-1);var content=choices[contractPage];
            MiniMachine(Rect("Stamp target",menuRoot,new Vector2(.17f,.52f),new Vector2(.83f,.78f)),content.Definition.InitialSymbolIds,content.Definition.TargetSymbolIds);
            MetaArrows(()=>{contractPage=(contractPage+choices.Length-1)%choices.Length;DrawMenu();},()=>{contractPage=(contractPage+1)%choices.Length;DrawMenu();});
            ObjectNameStrip("Contract stop",new Vector2(.055f,.40f),new Vector2(.945f,.47f),new UiCopy("Stamp trail {0}",DisplayLevelNumber(content.Definition.CampaignNodeId)));
            ReadingStrip("Contract freedom",new Vector2(.055f,.24f),new Vector2(.945f,.38f),HasResumableJourneyAttempt?(UiCopy)"Finish your current level before starting a stamp trail.":"Bring the row home with its stamp.",18,true);
            var start=MenuButton(HasResumableJourneyAttempt?"Finish current level first":"Try this trail",.12f,()=>StartStampTrail(content.Definition.CampaignNodeId),true);
            start.name="Trythistrail";start.interactable=!HasResumableJourneyAttempt;
            MenuButton("Back",.01f,ShowRequests,false,.30f,.70f);
        }
        void DrawRecords()
        {
            var records=profile.ModeRecords.Where(r=>!Catalog.BonusLevels.Any(c=>c.Definition.PuzzleId==r.PuzzleId)).OrderBy(r=>r.PuzzleId,StringComparer.Ordinal).ThenBy(r=>r.StampGoal).ToArray();
            if(records.Length==0){MenuTitle("My routes","");ReadingStrip("Empty journal",new Vector2(.055f,.40f),new Vector2(.945f,.63f),"Finish a puzzle to leave a note here.",18,true);MenuButton("Back",.025f,ShowJournal);return;}
            recordPage=Math.Min(recordPage,records.Length-1);var record=records[recordPage];
            var content=Catalog.PrimaryLevels.Concat(Catalog.LocalizedLevels).FirstOrDefault(c=>c.Definition.PuzzleId==record.PuzzleId&&c.Definition.ContentHash==(record.SourceHash??record.ContentHash));
            MenuTitle("My routes",content==null?(UiCopy)"A note from an earlier journey.":new UiCopy("Level {0}",DisplayLevelNumber(content.Definition.CampaignNodeId)));
            if(content!=null)MiniMachine(Rect("Record target",menuRoot,new Vector2(.19f,.575f),new Vector2(.81f,.79f)),content.Definition.InitialSymbolIds,content.Definition.TargetSymbolIds);
            RecordArrows(()=>{recordPage=(recordPage+records.Length-1)%records.Length;DrawMenu();},()=>{recordPage=(recordPage+1)%records.Length;DrawMenu();});
            var clears=record.Mode==profile.PreferredMode?profile.Clears:profile.ModeJourneys.FirstOrDefault(j=>j.Mode==record.Mode)?.Clears;
            var earned=content==null||record.StageCount!=3?null:clears?.FirstOrDefault(c=>c.CampaignNodeId==content.Definition.CampaignNodeId);
            var summary=Rect("Route summary",menuRoot,new Vector2(.08f,.31f),new Vector2(.92f,.43f));
            RecordMetric("Route moves",summary,new Vector2(0,0),new Vector2(.46f,1),ResultMedal(1),record.BestRoute.ToString());
            RecordMetric("Route stars",summary,new Vector2(.54f,0),new Vector2(1,1),CompletionArt(0),earned==null||earned.BestStars==0?"—":LevelCompletion.FormatStars(earned.BestStarRating));
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
        void RecordMetric(string name,RectTransform parent,Vector2 min,Vector2 max,Sprite icon,string value)
        {
            var group=Rect(name,parent,min,max);
            Picture(name+" icon",group,new Vector2(.02f,.07f),new Vector2(.40f,.93f),icon,true);
            var number=Text(name+" value",group,new Vector2(.43f,.07f),new Vector2(.99f,.93f),26,UiCopy.Raw("× "+value));
            number.alignment=TextAlignmentOptions.MidlineLeft;number.margin=Vector4.zero;
            number.textWrappingMode=TextWrappingModes.NoWrap;StyleCelebrationType(number,CelebrationType.SkyNumber);
        }
    }
}
