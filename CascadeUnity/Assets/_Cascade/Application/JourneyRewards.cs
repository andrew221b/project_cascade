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

    // Durable earned rewards. No calendar loss, random reroll or gameplay mutation.
    public sealed class JourneyRewards
    {
        public static readonly string[] Gifts={"leaf","sun","sky","moon"};
        public string SelectedGift="leaf",EquippedGift;
        public List<string> SeenClearNodes=new List<string>();
        public List<ParcelReceipt> Parcels=new List<ParcelReceipt>();
        public List<DeliveryFacts> Deliveries=new List<DeliveryFacts>();
        public List<MasteryRecord> Records=new List<MasteryRecord>();
        public List<string> VisitDates=new List<string>();
        public int Fragments(string id)=>Parcels.Count(p=>p.GiftId==id&&p.Claimed);
        public int EarnedFragments(string id)=>Parcels.Count(p=>p.GiftId==id);
        public bool Owns(string id)=>Fragments(id)==3;
        public bool AllGiftsEarned=>Parcels.Count==12;
        public int OrderProgress=>AllGiftsEarned?3:SeenClearNodes.Count%3;
        public int CompletedRequests=>(QuestCount(0)>=1?1:0)+(QuestCount(1)>=2?1:0)+(QuestCount(2)>=3?1:0);
        public int KeeperPoints=>SeenClearNodes.Count+CompletedRequests*2;
        public int Rank=>KeeperPoints>=40?4:KeeperPoints>=25?3:KeeperPoints>=10?2:KeeperPoints>=3?1:0;

        public void Synchronize(IEnumerable<string> clearNodes)
        {
            foreach(string node in clearNodes) {
                if(SeenClearNodes.Contains(node))continue;
                SeenClearNodes.Add(node);
                if(SeenClearNodes.Count%3==0&&!AllGiftsEarned) {
                    EnsureSelection();Parcels.Add(new ParcelReceipt{Order=SeenClearNodes.Count/3,GiftId=SelectedGift});EnsureSelection();
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
        public bool ClaimParcel(int order)
        {
            var receipt=Parcels.FirstOrDefault(p=>p.Order==order);
            if(receipt==null||receipt.Claimed)return false;
            receipt.Claimed=true;
            if(EquippedGift==null&&Owns(receipt.GiftId))EquippedGift=receipt.GiftId;
            return true;
        }
        public bool Equip(string id)
        {
            if(!Gifts.Contains(id)||!Owns(id))return false;EquippedGift=id;return true;
        }
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
            =>orderedNodes.Count(SeenClearNodes.Contains)*40+Enumerable.Range(0,4).Count(c=>WorkshopCount(c,orderedNodes)==10)*100;

        public void RecordDelivery(AttemptService attempt,DeliveryPerformance performance=null)
        {
            if(attempt.Current.Status!=PuzzleStatus.Won)return;
            int flags=attempt.Current.StampBits!=0?4:0;
            var initial=attempt.Resolver.CreateInitialState();if(initial.RepairUsed)flags|=1;
            foreach(var entry in attempt.History)foreach(var e in attempt.Resolver.Resolve(entry.Before,entry.Command).Events) {
                if(e.Kind==PuzzleEventKind.Repaired)flags|=1;
                if(e.Kind==PuzzleEventKind.BridgeConsumed)flags|=2;
            }
            var facts=Deliveries.FirstOrDefault(d=>d.NodeId==attempt.Level.CampaignNodeId);
            if(facts==null){facts=new DeliveryFacts{NodeId=attempt.Level.CampaignNodeId};Deliveries.Add(facts);}facts.Flags|=flags;
            Record(attempt,0,performance);
            if(attempt.Goal.RequiredStampBits!=0&&attempt.Goal.IsSatisfied(attempt.Current))Record(attempt,attempt.Goal.RequiredStampBits,performance);
        }
        void Record(AttemptService attempt,int goal,DeliveryPerformance performance)
        {
            int route=performance?.Route??attempt.RouteLength;bool hinted=performance?.HintUsed??attempt.HintUsed;int undos=performance?.Undos??attempt.UndoCount;
            var candidate=new MasteryRecord{PuzzleId=attempt.Level.PuzzleId,ContentHash=performance?.ContentHash??attempt.PinnedContentHash,
                SourceHash=performance?.SourceHash,StageCount=performance==null?0:3,
                Mode=attempt.Mode.ToString(),Locale=attempt.ActiveLocale,StampGoal=goal,BestRoute=route};
            var record=Records.FirstOrDefault(r=>r.Key==candidate.Key);
            if(record==null){record=candidate;Records.Add(record);}
            record.BestRoute=Math.Min(record.BestRoute,route);
            if(!hinted)record.BestWithoutHint=Math.Min(record.BestWithoutHint??int.MaxValue,route);
            if(!hinted&&undos==0)record.BestClean=Math.Min(record.BestClean??int.MaxValue,route);
        }
        public void Validate()
        {
            if(!Gifts.Contains(SelectedGift)||EquippedGift!=null&&!Gifts.Contains(EquippedGift)
                ||SeenClearNodes==null||SeenClearNodes.Count>10000||SeenClearNodes.Any(string.IsNullOrWhiteSpace)
                ||SeenClearNodes.Distinct().Count()!=SeenClearNodes.Count||Parcels==null||Parcels.Count>12
                ||Parcels.Any(p=>p==null||p.Order<1||p.Order>SeenClearNodes.Count/3||!Gifts.Contains(p.GiftId))
                ||Parcels.Select(p=>p.Order).Distinct().Count()!=Parcels.Count||Gifts.Any(g=>EarnedFragments(g)>3)
                ||EquippedGift!=null&&!Owns(EquippedGift)||Deliveries==null||Deliveries.Count>10000
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
