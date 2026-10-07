using System;
using System.Linq;
namespace Cascade.Application
{
    public sealed partial class AdService
    {
        public bool TryPreviewItem(TravelTool tool,string gift=null)
        {
            if(!PreviewOnly||FullscreenActive)return false;
            var next=Ledger.Copy();
            if(gift!=null){if(!ShopCatalog.TrophyIds.Contains(gift)||next.Cosmetics.Contains(gift))return false;next.Cosmetics.Add(gift);}
            else {if(!ShopCatalog.IsTool(tool)||ToolStock(tool)>=ShopCatalog.StockLimit)return false;AddTool(next,tool,1);}
            return Commit(next);
        }
        public bool TryPreviewNoAds()
        {
            if(!PreviewOnly||FullscreenActive||Ledger.NoAutomaticAds)return false;
            var next=Ledger.Copy();next.NoAutomaticAds=true;return Commit(next);
        }
        public bool TryPreviewCoinPack(string product)
        {
            int amount=PurchaseCatalog.Coins(product);if(!PreviewOnly||FullscreenActive||amount==0)return false;
            var next=Ledger.Copy();next.BonusCoins=checked(next.BonusCoins+amount);return Commit(next);
        }
        public int CoinBalance(int earnedCoins) => (int)Math.Max(0L,(long)earnedCoins+Ledger.BonusCoins-Ledger.SpentCoins);
        public int ToolStock(TravelTool tool) => tool==TravelTool.Wayfinder?Ledger.WayfinderScrolls:tool==TravelTool.UndoSpool?Ledger.UndoSpools:tool==TravelTool.ReturnHourglass?Ledger.ReturnHourglasses:tool==TravelTool.SignalLantern?Ledger.SignalLanterns:tool==TravelTool.LandingLens?Ledger.LandingLenses:0;
        static void AddTool(MonetizationLedger ledger,TravelTool tool,int amount)
        {if(tool==TravelTool.Wayfinder)ledger.WayfinderScrolls+=amount;else if(tool==TravelTool.UndoSpool)ledger.UndoSpools+=amount;else if(tool==TravelTool.ReturnHourglass)ledger.ReturnHourglasses+=amount;else if(tool==TravelTool.SignalLantern)ledger.SignalLanterns+=amount;else if(tool==TravelTool.LandingLens)ledger.LandingLenses+=amount;}
        public bool TryBuyTool(TravelTool tool,int earnedCoins)
        {
            if(FullscreenActive||earnedCoins<0||!ShopCatalog.IsTool(tool)||ToolStock(tool)>=ShopCatalog.StockLimit||CoinBalance(earnedCoins)<ShopCatalog.Price(tool))return false;
            var next=Ledger.Copy();next.SpentCoins=checked(next.SpentCoins+ShopCatalog.Price(tool));AddTool(next,tool,1);return Commit(next);
        }
        public bool TryBuyKeepsake(string gift,int earnedCoins,JourneyRewards rewards)
        {
            if(FullscreenActive||earnedCoins<0||rewards==null||!ShopCatalog.TrophyIds.Contains(gift)||rewards.Owns(gift)||Ledger.Cosmetics.Contains(gift)||CoinBalance(earnedCoins)<ShopCatalog.KeepsakePrice)return false;
            var next=Ledger.Copy();next.SpentCoins=checked(next.SpentCoins+ShopCatalog.KeepsakePrice);next.Cosmetics.Add(gift);return Commit(next);
        }
        public bool TryUseTool(TravelTool tool,AttemptService attempt,HintOffer guide=null)
        {
            if(FullscreenActive||attempt==null||!ShopCatalog.IsTool(tool)||ToolStock(tool)==0)return false;
            var next=Ledger.Copy();AddTool(next,tool,-1);
            return attempt.TryUseToolAtomically(tool,()=>Commit(next),guide);
        }
    }
}
