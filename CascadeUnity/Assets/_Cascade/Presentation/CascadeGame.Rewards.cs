using System;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Solver;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int giftPage,parcelOrder;
        bool parcelOpened,parcelAnimationShown;
        GameScreen collectionReturn=GameScreen.TravelBag;
        LevelContent[] contracts;
        static readonly string[] GiftNames={"Leaf pin","Sun button","Sky ribbon","Moon patch","Wind chime","Rain badge","Copper acorn","Beacon brooch","Sky pilot","Moth mask","Garden festival","Cloud gardener","Explorer hat","Frog suit","Star knight","Fox mask","Mushroom hat","Bee costume","Winter hood","Sky pirate","Cloud jester","Star mage","Paper crane","Little comet","Dew bell","Night flower","Cloud kite","Fern compass","Amber moth","Moon boat"};
        static readonly string[] GiftAssets={"LeafPin","SunButton","SkyRibbon","MoonPatch"};
        static readonly string[] RankNames={"Apprentice","Garden friend","Mill keeper","Pathfinder","Beacon keeper"};
        ParcelReceipt WaitingParcel=>Rewards.Parcels.FirstOrDefault(p=>!p.Claimed||!p.BonusClaimed&&GameEconomy?.HasParcelBonus(MonetizationLedger.ParcelSubject(p.Order))==true);
        string[] OrderedNodes=>Catalog.PrimaryLevels.Select(c=>c.Definition.CampaignNodeId).ToArray();
        GameScreen bagReturnScreen=GameScreen.Home;
        ShopLocation bagShopLocation,collectionShopLocation;
        GameScreen parcelReturnScreen=GameScreen.TravelBag;
        public void ShowTravelBag()
        {
            if(CurrentScreen==GameScreen.Shop&&shopReturnScreen==GameScreen.TravelBag){BackFromShop();return;}
            if(CurrentScreen==GameScreen.Home||CurrentScreen==GameScreen.Shop||CurrentScreen==GameScreen.JourneyMap||CurrentScreen==GameScreen.JourneyEnd){
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
        public void ShowJournal()=>ShowTravelBag();
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
            ShowShop();
        }
        public void ShowPowerupChallenges()=>ShowTravelBag();
        public void ShowReachChallenges()=>ShowTravelBag();
        public void ShowRequests(){ShowJourney();}
        public void ShowRecords()=>ShowTravelBag();
        public void ShowContracts()=>ShowJourney();
        public void ShowParcel()
            =>OpenParcel(false);
        void OpenParcel(bool returnToMap)
        {
            var receipt=returnToMap&&profile.Completion?.ParcelOrder>0?Rewards.Parcels.FirstOrDefault(p=>p.Order==profile.Completion.ParcelOrder):WaitingParcel;
            if(receipt==null){if(returnToMap)FinishCompletion();return;}
            parcelReturnsToMap=returnToMap;
            if(!returnToMap&&CurrentScreen!=GameScreen.Parcel)parcelReturnScreen=CurrentScreen;
            parcelOrder=receipt.Order;parcelOpened=false;parcelAnimationShown=false;parcelBonusRound=false;parcelPreviewBonusClaimed=false;SetScreen(GameScreen.Parcel);
        }
        void DrawTravelBag()
        {
            DrawReferenceInventory();
        }
        void DrawCollection(){shopItem=giftPage;DrawTravelCounter(false,true);}
        void DrawParcel()=>DrawReferenceParcel();
        void ShowCollectedTrophy(string id)
        {
            if(!OwnsGift(id))return;
            ShowCollection();shopItem=giftPage=Array.IndexOf(ShopCatalog.TrophyIds,id);shopDetailOpen=true;DrawMenu();
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
    }
}
