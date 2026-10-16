using System;
using System.Collections.Generic;
using System.Linq;
namespace Cascade.Application
{
    public sealed class ParcelDrop { public string Kind,Id;public int Amount; }
    public static class ParcelLoot
    {
        static uint Hash(int order,int salt){uint h=(uint)(order*747796405+salt);h=(h^(h>>16))*2246822519u;return h^(h>>13);}
        static ParcelDrop Gold(int amount)=>new ParcelDrop{Kind="coins",Amount=amount};
        public static ParcelReceipt Create(int order)
        {
            // Retain the published set of campaign parcel positions.
            JourneyRewards.VarietyParcel(order);
            return new ParcelReceipt{Order=order,Kind="loot",RecipeVersion=6,Loot=Base(order)};
        }
        public static List<ParcelDrop> Base(int order)
        {
            uint h=Hash(order,103);int count=1+(int)(h%2),total=12+(int)(h%13);
            bool trophy=order==3||order==180;if(trophy)count=2;
            var list=new List<ParcelDrop>();
            if(count==1){list.Add(Gold(total));return list;}
            if(trophy){list.Add(Gold(total));list.Add(new ParcelDrop{Kind="cosmetic",Id=order==3?"leaf":"moon_boat",Amount=1});}
            else if(h%5==0){list.Add(Gold(total));list.Add(new ParcelDrop{Kind="tool",Id=((TravelTool)(h%ShopCatalog.ToolCount)).ToString(),Amount=1});}
            else{int first=total/2;list.Add(Gold(first));list.Add(Gold(total-first));}
            return list;
        }
        public static List<ParcelDrop> Bonus(int order)
        {
            uint h=Hash(order,907);int count=2+(int)(h%2);bool tool=h%5==0;
            int goldCards=count-(tool?1:0);var list=new List<ParcelDrop>();
            for(int i=0;i<goldCards;i++)list.Add(Gold(40/goldCards+(i<40%goldCards?1:0)));
            if(tool)list.Add(new ParcelDrop{Kind="tool",Id=((TravelTool)(h%ShopCatalog.ToolCount)).ToString(),Amount=1});
            return list;
        }
        public static List<ParcelDrop> Resolve(IEnumerable<ParcelDrop> plan,JourneyRewards rewards,MonetizationLedger ledger)
        {
            var stock=new[]{ledger.WayfinderScrolls,ledger.UndoSpools,ledger.ReturnHourglasses,ledger.SignalLanterns,ledger.LandingLenses,ledger.RepairKits};
            var result=new List<ParcelDrop>();
            foreach(var source in plan){var drop=new ParcelDrop{Kind=source.Kind,Id=source.Id,Amount=source.Amount};
                if(drop.Kind=="cosmetic"&&(rewards.Owns(drop.Id)||ledger.Cosmetics.Contains(drop.Id))){
                    int index=Array.FindIndex(stock,n=>n<ShopCatalog.StockLimit);
                    drop=index<0?Gold(20):new ParcelDrop{Kind="tool",Id=((TravelTool)index).ToString(),Amount=1};
                }
                if(drop.Kind=="tool"){
                    ShopCatalog.TryTool(drop.Id,out var tool);int index=(int)tool;
                    if(stock[index]>=ShopCatalog.StockLimit)drop=Gold(20);else stock[index]++;
                }
                result.Add(drop);
            }return result;
        }
        public static bool Same(IEnumerable<ParcelDrop> a,IEnumerable<ParcelDrop> b)=>a!=null&&b!=null&&a.Select(Key).SequenceEqual(b.Select(Key));
        static string Key(ParcelDrop p)=>p==null?"null":p.Kind+"|"+p.Id+"|"+p.Amount;
        public static bool Valid(List<ParcelDrop> actual,List<ParcelDrop> plan,bool claimed)
        {
            if(actual==null||actual.Count!=plan.Count)return false;
            for(int i=0;i<plan.Count;i++){
                var a=actual[i];var p=plan[i];if(a==null)return false;if(Key(a)==Key(p))continue;
                if(!claimed)return false;
                bool gold=a.Kind=="coins"&&a.Id==null&&a.Amount==20;
                if(p.Kind=="tool"&&gold)continue;
                if(p.Kind=="cosmetic"&&(gold||a.Kind=="tool"&&a.Amount==1&&ShopCatalog.TryTool(a.Id,out _)))continue;
                return false;
            }return true;
        }
        public static string GrantId(int order,bool bonus,int slot)=>"parcel_tool_"+order+"_"+(bonus?"bonus":"base")+"_"+slot;
        public static bool IsLoot(ParcelReceipt receipt)=>receipt?.RecipeVersion==6;
    }
    public sealed partial class AdService
    {
        public List<ParcelDrop> PreviewParcelDrops(ParcelReceipt receipt,JourneyRewards rewards,bool bonus=false)
            =>bonus?receipt.BonusClaimed?receipt.BonusLoot:ParcelLoot.Resolve(ParcelLoot.Bonus(receipt.Order),rewards,Ledger)
                :receipt.Claimed?receipt.Loot:ParcelLoot.Resolve(receipt.Loot,rewards,Ledger);
        bool TryClaimLoot(ParcelReceipt receipt,JourneyRewards rewards,bool bonus)
        {
            if(PreviewOnly||FullscreenActive||receipt==null||rewards==null)return false;
            if(bonus ? !receipt.Claimed||receipt.BonusClaimed||!HasParcelBonus(MonetizationLedger.ParcelSubject(receipt.Order)) : receipt.Claimed)return false;
            var old=bonus?receipt.BonusLoot:receipt.Loot;
            var drops=ParcelLoot.Resolve(bonus?ParcelLoot.Bonus(receipt.Order):receipt.Loot,rewards,Ledger);var next=Ledger.Copy();
            for(int i=0;i<drops.Count;i++)if(drops[i].Kind=="tool"){
                if(next.Grants.Count>=MonetizationLedger.MaxGrantReceipts||!ShopCatalog.TryTool(drops[i].Id,out var tool))return false;
                string id=ParcelLoot.GrantId(receipt.Order,bonus,i);if(next.Grants.Any(g=>g.Id==id))return false;
                AddTool(next,tool,1);next.Grants.Add(new GrantReceipt{Id=id,Kind="parcel_tool",Product=drops[i].Id,Amount=1});
            }
            if(bonus){receipt.BonusLoot=drops;receipt.BonusClaimed=true;}else{receipt.Loot=drops;receipt.Claimed=true;}
            if(Commit(next))return true;
            if(bonus){receipt.BonusLoot=old;receipt.BonusClaimed=false;}else{receipt.Loot=old;receipt.Claimed=false;}return false;
        }
        public bool TryClaimParcelBonus(int order,JourneyRewards rewards)
        {
            if(PreviewOnly||FullscreenActive||rewards==null)return false;
            return TryClaimLoot(rewards.Parcels.FirstOrDefault(p=>p.Order==order),rewards,true);
        }
    }
}
