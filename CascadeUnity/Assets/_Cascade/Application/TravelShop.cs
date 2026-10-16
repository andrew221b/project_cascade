using System;
using System.Linq;
namespace Cascade.Application
{
    public sealed partial class AdService
    {
        public bool TryPreviewItem(TravelTool tool,string gift=null)
        {
            if(!PreviewOnly||FullscreenActive||ShopCatalog.IsParcelExclusive(gift))return false;
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
        public int CoinBalance(int earnedCoins) => (int)Math.Min(int.MaxValue,Math.Max(0L,(long)earnedCoins+Ledger.BonusCoins-Ledger.SpentCoins));
        public int ToolStock(TravelTool tool) => tool==TravelTool.Wayfinder?Ledger.WayfinderScrolls:tool==TravelTool.UndoSpool?Ledger.UndoSpools:tool==TravelTool.ReturnHourglass?Ledger.ReturnHourglasses:tool==TravelTool.SignalLantern?Ledger.SignalLanterns:tool==TravelTool.LandingLens?Ledger.LandingLenses:tool==TravelTool.RepairKit?Ledger.RepairKits:0;
        static void AddTool(MonetizationLedger ledger,TravelTool tool,int amount)
        {checked{if(tool==TravelTool.Wayfinder)ledger.WayfinderScrolls+=amount;else if(tool==TravelTool.UndoSpool)ledger.UndoSpools+=amount;else if(tool==TravelTool.ReturnHourglass)ledger.ReturnHourglasses+=amount;else if(tool==TravelTool.SignalLantern)ledger.SignalLanterns+=amount;else if(tool==TravelTool.LandingLens)ledger.LandingLenses+=amount;else if(tool==TravelTool.RepairKit)ledger.RepairKits+=amount;}}
        public bool TryBuyTool(TravelTool tool,int earnedCoins)
        {
            if(FullscreenActive||earnedCoins<0||!ShopCatalog.IsTool(tool)||ToolStock(tool)>=ShopCatalog.StockLimit||Ledger.SpentCoins>int.MaxValue-ShopCatalog.Price(tool)||CoinBalance(earnedCoins)<ShopCatalog.Price(tool))return false;
            var next=Ledger.Copy();next.SpentCoins=checked(next.SpentCoins+ShopCatalog.Price(tool));AddTool(next,tool,1);return Commit(next);
        }
        public bool TryBuyKeepsake(string gift,int earnedCoins,JourneyRewards rewards)
        {
            if(FullscreenActive||earnedCoins<0||rewards==null||ShopCatalog.IsParcelExclusive(gift)||!ShopCatalog.TrophyIds.Contains(gift)||rewards.Owns(gift)||Ledger.Cosmetics.Contains(gift)
                ||rewards.CosmeticClearCount<ShopCatalog.PurchaseClearMilestone(gift)||CoinBalance(earnedCoins)<ShopCatalog.TrophyPrice(gift))return false;
            var next=Ledger.Copy();next.SpentCoins=checked(next.SpentCoins+ShopCatalog.TrophyPrice(gift));next.Cosmetics.Add(gift);return Commit(next);
        }
        public bool TryEquipScenery(string id,JourneyRewards rewards)=>TryEquipScreenCosmetic(id,rewards,false);
        public bool TryEquipFrame(string id,JourneyRewards rewards)=>TryEquipScreenCosmetic(id,rewards,true);
        public bool TryEquipMika(string id,JourneyRewards rewards)
        {
            if(PreviewOnly||FullscreenActive||rewards==null||rewards.EquippedGift==id)return false;
            string previous=rewards.EquippedGift;
            if(!rewards.Equip(id,Ledger.Cosmetics))return false;
            if(Commit(Ledger.Copy()))return true;
            rewards.EquippedGift=previous;return false;
        }
        bool TryEquipScreenCosmetic(string id,JourneyRewards rewards,bool frame)
        {
            if(PreviewOnly||FullscreenActive||rewards==null)return false;
            string previous=frame?rewards.EquippedFrame:rewards.EquippedScenery;if(previous==id)return false;
            if(!(frame?rewards.EquipFrame(id,Ledger.Cosmetics):rewards.EquipScenery(id,Ledger.Cosmetics)))return false;
            if(Commit(Ledger.Copy()))return true;
            if(frame)rewards.EquippedFrame=previous;else rewards.EquippedScenery=previous;return false;
        }
        // The durable transaction's save sees both the claimed parcel and candidate inventory.
        public bool TryClaimParcel(int order,JourneyRewards rewards)
        {
            if(PreviewOnly||FullscreenActive||rewards==null)return false;
            var receipt=rewards.Parcels.FirstOrDefault(p=>p.Order==order);
            if(receipt==null||receipt.Claimed)return false;
            if(ParcelLoot.IsLoot(receipt))return TryClaimLoot(receipt,rewards,false);
            var next=Ledger.Copy();bool tool=receipt.Kind=="tool",duplicate=false;
            if(tool){
                if(!ShopCatalog.TryTool(receipt.GiftId,out var item)||next.Grants.Any(g=>g.Id==JourneyRewards.ParcelToolGrantId(order)))return false;
                duplicate=ToolStock(item)>=ShopCatalog.StockLimit;
                if(!duplicate&&!CanReceiveGrant)return false;
                if(!duplicate){AddTool(next,item,1);next.Grants.Add(new GrantReceipt{
                    Id=JourneyRewards.ParcelToolGrantId(order),Kind="parcel_tool",Product=receipt.GiftId,Amount=1});}
            }
            bool oldDuplicate=receipt.DuplicateCoins;string oldEquipped=rewards.EquippedGift;
            if(!rewards.ClaimParcelCore(order,next.Cosmetics,tool,duplicate))return false;
            if(Commit(next))return true;
            receipt.Claimed=false;receipt.DuplicateCoins=oldDuplicate;rewards.EquippedGift=oldEquipped;return false;
        }
        public bool TryUseTool(TravelTool tool,AttemptService attempt,HintOffer guide=null)
        {
            if(FullscreenActive||attempt==null||!ShopCatalog.IsTool(tool)||ToolStock(tool)==0)return false;
            var next=Ledger.Copy();AddTool(next,tool,-1);
            return attempt.TryUseToolAtomically(tool,()=>Commit(next),guide);
        }
    }
}
