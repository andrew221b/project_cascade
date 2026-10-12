using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cascade.Domain;

namespace Cascade.Application
{
    public sealed class ParcelReceipt
    {
        public int Order;
        public string GiftId;
        public bool Claimed;
        // Empty for the twelve published gift-part receipts. Later receipts
        // have a fixed prize determined by Order, never by a client RNG roll.
        public string Kind;
        public bool DuplicateCoins;
        // Zero/missing preserves all receipts minted by the original recipe.
        // Omit zero so existing cloud baseline hashes retain their published JSON.
        [Newtonsoft.Json.JsonProperty(DefaultValueHandling=Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public int RecipeVersion;
    }
    public sealed class DeliveryFacts
    {
        public string NodeId;
        public int Flags; // 1 repair, 2 bridge, 4 stamp; evidence is merged per unique node.
    }
    public sealed class MasteryRecord
    {
        public string PuzzleId, ContentHash, Mode, Locale;
        public string SourceHash;
        public int StageCount;
        public int StampGoal, BestRoute;
        public int? BestWithoutHint, BestClean;
        public string Key=>string.Join("|",PuzzleId,ContentHash,Mode,Locale,StampGoal.ToString(CultureInfo.InvariantCulture));
    }
    public sealed class ReplayCredit {
        public string NodeId;public int Stars;
        [Newtonsoft.Json.JsonProperty(DefaultValueHandling=Newtonsoft.Json.DefaultValueHandling.Ignore)] public bool HalfStar;
        [Newtonsoft.Json.JsonIgnore] public int HalfUnits=>Stars*2+(HalfStar?1:0);
    }

    // Durable earned rewards. No calendar loss, random reroll or gameplay mutation.
    public sealed class JourneyRewards
    {
        public static readonly string[] Gifts={"leaf","sun","sky","moon"};
        static readonly TravelTool[] ParcelTools={TravelTool.LandingLens,TravelTool.UndoSpool,TravelTool.Wayfinder,TravelTool.SignalLantern,TravelTool.ReturnHourglass};
        static readonly string[] ParcelCosmetics=ShopCatalog.SceneryTrophyIds.Concat(ShopCatalog.FrameTrophyIds).Concat(ShopCatalog.ParcelTrophyIds).ToArray();
        public string SelectedGift="leaf",EquippedGift;
        [Newtonsoft.Json.JsonProperty(DefaultValueHandling=Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public string EquippedScenery;
        [Newtonsoft.Json.JsonProperty(DefaultValueHandling=Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public string EquippedFrame;
        public List<string> SeenClearNodes=new List<string>();
        // Revision 0 saves were earned at 40 coins per clear plus 100 per ten clears.
        // The first wallet read carries that already-earned value into the new schedule.
        [Newtonsoft.Json.JsonProperty(DefaultValueHandling=Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public int CoinEconomyRevision;
        [Newtonsoft.Json.JsonProperty(DefaultValueHandling=Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public int CoinIncomeCarryover;
        [Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
        public List<ReplayCredit> ReplayCredits;
        public void RecordReplayCredit(string node,int stars,bool halfStar=false)
        {
            if(!SeenClearNodes.Contains(node)||stars<1||stars>3||halfStar&&stars==3)throw new ArgumentException("Replay requires a prior clear and a valid rating.");
            if(ReplayCredits==null)ReplayCredits=new List<ReplayCredit>();
            var credit=ReplayCredits.FirstOrDefault(c=>c.NodeId==node);
            if(credit==null)ReplayCredits.Add(new ReplayCredit{NodeId=node,Stars=stars,HalfStar=halfStar});else if(stars*2+(halfStar?1:0)>credit.HalfUnits){credit.Stars=stars;credit.HalfStar=halfStar;}
        }
        public List<ParcelReceipt> Parcels=new List<ParcelReceipt>();
        public List<DeliveryFacts> Deliveries=new List<DeliveryFacts>();
        public List<MasteryRecord> Records=new List<MasteryRecord>();
        public List<string> VisitDates=new List<string>();
        public int Fragments(string id)=>Parcels.Count(p=>p.GiftId==id&&p.Claimed);
        public int EarnedFragments(string id)=>Parcels.Count(p=>string.IsNullOrEmpty(p.Kind)&&p.GiftId==id);
        public int CosmeticClearCount=>SeenClearNodes.Where(n=>!string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.Ordinal).Count();
        public bool Owns(string id)
        {
            if(Parcels.Any(p=>p.Claimed&&p.Kind=="cosmetic"&&!p.DuplicateCoins&&p.GiftId==id))return true;
            int milestone=ShopCatalog.EarnedClearMilestone(id);
            return milestone>0 ? CosmeticClearCount>=milestone : Gifts.Contains(id)&&Fragments(id)==3;
        }
        public bool AllGiftsEarned=>Gifts.All(g=>EarnedFragments(g)==3);
        public int OrderProgress=>SeenClearNodes.Count>=36?Math.Max(0,SeenClearNodes.Count-36)%12:SeenClearNodes.Count%3;
        public int NextParcelGoal=>SeenClearNodes.Count>=36?12:3;
        public int CompletedRequests=>(QuestCount(0)>=1?1:0)+(QuestCount(1)>=2?1:0)+(QuestCount(2)>=3?1:0);
        public int KeeperPoints=>SeenClearNodes.Count+CompletedRequests*2;
        public int Rank=>KeeperPoints>=40?4:KeeperPoints>=25?3:KeeperPoints>=10?2:KeeperPoints>=3?1:0;

        public void Synchronize(IEnumerable<string> clearNodes)
        {
            foreach(string node in clearNodes) {
                if(SeenClearNodes.Contains(node))continue;
                SeenClearNodes.Add(node);
                if(SeenClearNodes.Count%3==0&&SeenClearNodes.Count<=36) {
                    Parcels.Add(VarietyParcel(SeenClearNodes.Count/3));
                }
                else if(SeenClearNodes.Count>36&&SeenClearNodes.Count%12==0){
                    Parcels.Add(VarietyParcel(SeenClearNodes.Count/3));
                }
            }
        }
        void EnsureSelection()
        {
            if(EarnedFragments(SelectedGift)<3)return;
            SelectedGift=Gifts.FirstOrDefault(g=>EarnedFragments(g)<3)??SelectedGift;
        }
        public bool SelectGift(string id)
        {
            if(!Gifts.Contains(id)||EarnedFragments(id)>=3)return false;
            SelectedGift=id;return true;
        }
        public bool ClaimParcel(int order,IEnumerable<string> purchased=null)
            =>ClaimParcelCore(order,purchased,false,false);
        internal bool ClaimParcelCore(int order,IEnumerable<string> purchased,bool toolClaim,bool toolDuplicate)
        {
            var receipt=Parcels.FirstOrDefault(p=>p.Order==order);
            if(receipt==null||receipt.Claimed||receipt.Kind=="tool"&&!toolClaim)return false;
            if(receipt.Kind=="cosmetic"&&(Owns(receipt.GiftId)||purchased?.Contains(receipt.GiftId)==true))receipt.DuplicateCoins=true;
            if(receipt.Kind=="tool")receipt.DuplicateCoins=toolDuplicate;
            receipt.Claimed=true;
            return true;
        }
        public bool Equip(string id,IEnumerable<string> purchased=null)
        {
            if(id!=null&&(!ShopCatalog.MikaTrophyIds.Contains(id)||!Owns(id)&&purchased?.Contains(id)!=true))return false;EquippedGift=id;return true;
        }
        public bool EquipScenery(string id,IEnumerable<string> purchased=null)
        {if(id!=null&&(!ShopCatalog.IsScenery(id)||!Owns(id)&&purchased?.Contains(id)!=true))return false;EquippedScenery=id;return true;}
        public bool EquipFrame(string id,IEnumerable<string> purchased=null)
        {if(id!=null&&(!ShopCatalog.IsFrame(id)||!Owns(id)&&purchased?.Contains(id)!=true))return false;EquippedFrame=id;return true;}
        public bool RegisterVisit(DateTime localDate)
        {
            string date=localDate.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);
            if(VisitDates.Count==7||VisitDates.Contains(date))return false;
            VisitDates.Add(date);return true;
        }
        public int QuestCount(int quest)
        {
            int flag=quest==0?1:quest==1?2:4;
            return Deliveries.Count(d=>(d.Flags&flag)!=0);
        }
        public int WorkshopCount(int chapter,IReadOnlyList<string> orderedNodes)
        {
            if(chapter<0||chapter>=4)throw new ArgumentOutOfRangeException(nameof(chapter));
            return orderedNodes.Skip(chapter*10).Take(10).Count(SeenClearNodes.Contains);
        }
        public int EarnedCoins(IReadOnlyList<string> orderedNodes)
        {
            int clears=orderedNodes.Where(SeenClearNodes.Contains).Distinct(StringComparer.Ordinal).Count();
            if(CoinEconomyRevision==0){CoinIncomeCarryover=checked(CoinIncomeCarryover+clears*40+(clears/10)*100-(clears*20+(clears/10)*50));CoinEconomyRevision=1;}
            return clears*20+(clears/10)*50+CoinIncomeCarryover
                +(ReplayCredits??new List<ReplayCredit>()).Where(c=>orderedNodes.Contains(c.NodeId)).Sum(c=>c.HalfUnits)
                +Parcels.Where(p=>p.Claimed).Sum(ParcelCoins);
        }
        public static int ParcelCoins(ParcelReceipt receipt)
        {
            if(receipt.RecipeVersion==5)return receipt.Kind=="coins"?12:receipt.DuplicateCoins?20:0;
            return receipt.RecipeVersion==4 ? (receipt.Kind=="coins"?12+(receipt.Order%4)*2:6+(receipt.Order%3)*2)+(receipt.DuplicateCoins?8:0) : (receipt.RecipeVersion==3?20:0)
                +(receipt.Kind=="coins"||(receipt.Kind=="cosmetic"||receipt.Kind=="tool")&&receipt.DuplicateCoins?20:0);
        }

        public static string ParcelToolGrantId(int order)=>"parcel_tool_"+order.ToString(CultureInfo.InvariantCulture);
        public bool MatchesToolGrant(GrantReceipt grant)=>grant!=null&&grant.Kind=="parcel_tool"&&grant.Amount==1
            &&Parcels.Any(p=>p.Claimed&&!p.DuplicateCoins&&p.Kind=="tool"&&p.RecipeVersion>=1&&p.RecipeVersion<=5
                &&p.GiftId==grant.Product&&ParcelToolGrantId(p.Order)==grant.Id);
        public void ValidateToolClaims(MonetizationLedger ledger)
        {
            if(ledger.Grants.Any(g=>g.Kind=="parcel_tool"&&!MatchesToolGrant(g))
                ||Parcels.Any(p=>p.Claimed&&p.Kind=="tool"&&!p.DuplicateCoins
                    &&!ledger.Grants.Any(g=>MatchesToolGrant(g)&&g.Id==ParcelToolGrantId(p.Order))))
                throw new ArgumentException("Tool parcel claims require their matching inventory receipt.");
        }

        public static int ToolFlags(AttemptService attempt)
        {
            int flags=attempt.Current.StampBits!=0?4:0;
            var initial=attempt.Resolver.CreateInitialState();if(initial.RepairUsed)flags|=1;
            foreach(var entry in attempt.History)foreach(var e in attempt.Resolver.Resolve(entry.Before,entry.Command).Events) {
                if(e.Kind==PuzzleEventKind.Repaired)flags|=1;
                if(e.Kind==PuzzleEventKind.BridgeConsumed)flags|=2;
            }
            return flags;
        }
        public void RecordDelivery(AttemptService attempt,DeliveryPerformance performance=null,List<MasteryRecord> records=null)
        {
            if(attempt.Current.Status!=PuzzleStatus.Won)return;
            int flags=(performance?.ToolFlags??0)|ToolFlags(attempt);
            var facts=Deliveries.FirstOrDefault(d=>d.NodeId==attempt.Level.CampaignNodeId);
            if(facts==null){facts=new DeliveryFacts{NodeId=attempt.Level.CampaignNodeId};Deliveries.Add(facts);}facts.Flags|=flags;
            Record(attempt,0,performance,records??Records);
            if(attempt.Goal.RequiredStampBits!=0&&attempt.Goal.IsSatisfied(attempt.Current))Record(attempt,attempt.Goal.RequiredStampBits,performance,records??Records);
        }
        void Record(AttemptService attempt,int goal,DeliveryPerformance performance,List<MasteryRecord> records)
        {
            int route=performance?.Route??attempt.RouteLength;bool hinted=performance?.HintUsed??attempt.HintUsed;int undos=performance?.Undos??attempt.UndoCount;
            var candidate=new MasteryRecord{PuzzleId=attempt.Level.PuzzleId,ContentHash=performance?.ContentHash??attempt.PinnedContentHash,
                SourceHash=performance?.SourceHash,StageCount=performance==null?0:3,
                Mode=attempt.Mode.ToString(),Locale=attempt.ActiveLocale,StampGoal=goal,BestRoute=route};
            var record=records.FirstOrDefault(r=>r.Key==candidate.Key);
            if(record==null){record=candidate;records.Add(record);}
            record.BestRoute=Math.Min(record.BestRoute,route);
            if(!hinted)record.BestWithoutHint=Math.Min(record.BestWithoutHint??int.MaxValue,route);
            if(!hinted&&undos==0)record.BestClean=Math.Min(record.BestClean??int.MaxValue,route);
        }
        public static ParcelReceipt BonusParcel(int order,int recipeVersion)
        {
            if(order<16||order%4!=0||recipeVersion<0||recipeVersion>3)throw new ArgumentException("Unknown parcel recipe.");
            int index=order/4-3;string kind="coins",gift=null;
            if(index>=4&&(index-4)%9==0){kind="cosmetic";gift=ShopCatalog.BonusTrophyIds[((index-4)/9)%ShopCatalog.BonusTrophyIds.Length];}
            else if(recipeVersion>=1&&index>=8&&(index-8)%9==0){kind="cosmetic";gift=ShopCatalog.NewTrophyIds[((index-8)/9)%ShopCatalog.NewTrophyIds.Length];}
            else if(recipeVersion>=2&&index>=2&&(index%9==2||index%9==5)){
                kind="cosmetic";int expansion=2*((index-2)/9)+(index%9==5?1:0);
                gift=ShopCatalog.ExpansionTrophyIds[expansion%ShopCatalog.ExpansionTrophyIds.Length];
            }
            else if(recipeVersion>=1&&index%4==1){kind="tool";gift=ParcelTools[((index-1)/4)%ParcelTools.Length].ToString();}
            return new ParcelReceipt{Order=order,RecipeVersion=recipeVersion,Kind=kind,GiftId=gift};
        }
        public static ParcelReceipt VarietyParcel(int order)
        {
            if(order<1||order>12&&(order<16||order%4!=0))throw new ArgumentException("Invalid variety parcel order.");
            int sequence=order<=12?order:12+(order-12)/4;
            if(sequence%20==0){
                string cosmetic=ParcelCosmetics[(sequence/20-1)%ParcelCosmetics.Length];
                return new ParcelReceipt{Order=order,RecipeVersion=5,Kind="cosmetic",GiftId=cosmetic};
            }
            if(sequence%5==0){
                string tool=ParcelTools[(sequence/5-1)%ParcelTools.Length].ToString();
                return new ParcelReceipt{Order=order,RecipeVersion=5,Kind="tool",GiftId=tool};
            }
            return new ParcelReceipt{Order=order,RecipeVersion=5,Kind="coins"};
        }
        static ParcelReceipt VarietyParcelV4(int order)
        {
            if(order<1||order>12&&(order<16||order%4!=0))throw new ArgumentException("Invalid variety parcel order.");
            string kind,gift=null;
            if(order<=12){
                kind=order%3==2?"coins":"cosmetic";
                if(kind=="cosmetic")gift=ShopCatalog.ParcelTrophyIds[(order-1)-(order+1)/3];
            }else{
                int index=order/4-4;
                kind=index%4==1?"tool":index%4==3?"coins":"cosmetic";
                if(kind=="tool")gift=ParcelTools[(index/4)%ParcelTools.Length].ToString();
                if(kind=="cosmetic"){
                    var pool=ShopCatalog.ParcelTrophyIds.Concat(Gifts).ToArray();
                    gift=pool[(index/4*2+(index%4==2?1:0))%pool.Length];
                }
            }
            return new ParcelReceipt{Order=order,RecipeVersion=4,Kind=kind,GiftId=gift};
        }
        static bool ValidVarietyParcel(ParcelReceipt receipt)
        {
            if(receipt.Order<1||receipt.Order>12&&(receipt.Order<16||receipt.Order%4!=0))return false;
            var recipe=receipt.RecipeVersion==4?VarietyParcelV4(receipt.Order):VarietyParcel(receipt.Order);
            return receipt.Kind==recipe.Kind&&receipt.GiftId==recipe.GiftId
                &&(!receipt.DuplicateCoins||receipt.Claimed&&(receipt.Kind=="cosmetic"||receipt.Kind=="tool"));
        }
        static bool ValidBonusParcel(ParcelReceipt receipt)
        {
            if(receipt.Order<16||receipt.Order%4!=0||receipt.RecipeVersion<0||receipt.RecipeVersion>3)return false;
            var recipe=BonusParcel(receipt.Order,receipt.RecipeVersion);
            return receipt.Kind==recipe.Kind&&receipt.GiftId==recipe.GiftId
                &&(!receipt.DuplicateCoins||receipt.Claimed&&(receipt.Kind=="cosmetic"||receipt.Kind=="tool"));
        }
        public void Validate(IEnumerable<string> purchased=null)
        {
            if(ReplayCredits!=null&&(ReplayCredits.Count>540||ReplayCredits.Any(c=>c==null||c.Stars<1||c.Stars>3||c.HalfStar&&c.Stars==3||!SeenClearNodes.Contains(c.NodeId))
                ||ReplayCredits.Select(c=>c.NodeId).Distinct(StringComparer.Ordinal).Count()!=ReplayCredits.Count))throw new ArgumentException("Invalid replay credits.");
            if(CoinEconomyRevision<0||CoinEconomyRevision>1||CoinIncomeCarryover<0||CoinIncomeCarryover>1000000
                ||!Gifts.Contains(SelectedGift)||EquippedGift!=null&&!ShopCatalog.MikaTrophyIds.Contains(EquippedGift)
                ||EquippedScenery!=null&&(!ShopCatalog.IsScenery(EquippedScenery)||!Owns(EquippedScenery)&&purchased?.Contains(EquippedScenery)!=true)
                ||EquippedFrame!=null&&(!ShopCatalog.IsFrame(EquippedFrame)||!Owns(EquippedFrame)&&purchased?.Contains(EquippedFrame)!=true)
                ||SeenClearNodes==null||SeenClearNodes.Count>10000||SeenClearNodes.Any(string.IsNullOrWhiteSpace)
                ||SeenClearNodes.Distinct().Count()!=SeenClearNodes.Count||Parcels==null||Parcels.Count>12+SeenClearNodes.Count/12
                ||Parcels.Any(p=>p==null||p.Order<1||p.Order>SeenClearNodes.Count/3||
                    (p.RecipeVersion==4||p.RecipeVersion==5?!ValidVarietyParcel(p):p.Order<=12?(!string.IsNullOrEmpty(p.Kind)||!Gifts.Contains(p.GiftId)||p.DuplicateCoins||(p.RecipeVersion!=0&&p.RecipeVersion!=3)):!ValidBonusParcel(p)))
                ||Parcels.Select(p=>p.Order).Distinct().Count()!=Parcels.Count||Gifts.Any(g=>EarnedFragments(g)>3)
                ||EquippedGift!=null&&!Owns(EquippedGift)&&purchased?.Contains(EquippedGift)!=true||Deliveries==null||Deliveries.Count>10000
                ||Deliveries.Any(d=>d==null||string.IsNullOrWhiteSpace(d.NodeId)||d.Flags<0||d.Flags>7)
                ||Deliveries.Select(d=>d.NodeId).Distinct().Count()!=Deliveries.Count||VisitDates==null||VisitDates.Count>7
                ||VisitDates.Distinct().Count()!=VisitDates.Count||VisitDates.Any(d=>!DateTime.TryParseExact(d,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out _))
                ||Records==null||Records.Count>20000||Records.Any(r=>r==null||string.IsNullOrWhiteSpace(r.PuzzleId)
                    ||r.ContentHash==null||r.ContentHash.Length!=64||!new[]{"Shapes","Words","Mixed"}.Contains(r.Mode)
                    ||!new[]{"en","ru","uk"}.Contains(r.Locale)||r.StampGoal<0||r.StampGoal>15||r.BestRoute<0
                    ||r.BestWithoutHint<r.BestRoute||r.BestClean<r.BestRoute||r.StageCount!=0&&r.StageCount!=3
                    ||r.StageCount==3&&(r.SourceHash==null||r.SourceHash.Length!=64))
                ||Records.Select(r=>r.Key).Distinct().Count()!=Records.Count)
                throw new ArgumentException("Invalid journey reward data.");
        }
    }
}
