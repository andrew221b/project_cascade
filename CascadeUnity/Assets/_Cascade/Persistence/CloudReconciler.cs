using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cascade.Persistence
{
    public static class CloudReconciler
    {
        static readonly JsonSerializerSettings CopySettings=new JsonSerializerSettings{ContractResolver=new StoredFields(),TypeNameHandling=TypeNameHandling.None,MaxDepth=64};
        static readonly string[] CanonicalNodes=Enumerable.Range(1,JourneyStory.TotalLevels).Select(n=>"C"+n.ToString("D2",CultureInfo.InvariantCulture)).ToArray();
        internal static bool ValidMode(string mode)=>mode=="Shapes"||mode=="Words"||mode=="Mixed";
        internal static T Clone<T>(T value)=>JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value,CopySettings),CopySettings);
        sealed class StoredFields:DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type,MemberSerialization serialization)
                =>base.CreateProperties(type,serialization).Where(p=>p.Readable&&p.Writable).ToList();
        }

        // Production callers provide the ordered published catalog, rather than uploaded IDs.
        public static CloudMergeResult Merge(PlayerProfile remote,PlayerProfile baseProfile,PlayerProfile local)
            =>Merge(remote,baseProfile,local,CanonicalNodes);
        public static CloudMergeResult Merge(PlayerProfile remote,PlayerProfile baseProfile,PlayerProfile local,IReadOnlyList<string> allowedNodes,bool allowVerifiedGrants=false,bool retainRemoteEconomy=false)
        {
            if(remote==null||baseProfile==null||local==null)throw new ArgumentNullException("Cloud profiles are required.");
            remote=Clone(remote);baseProfile=Clone(baseProfile);local=Clone(local);
            remote.MigrateCoinEconomy();baseProfile.MigrateCoinEconomy();local.MigrateCoinEconomy();
            CheckNodes(allowedNodes);remote.Validate();baseProfile.Validate();local.Validate();ValidateAuthoritativeEconomy(remote,allowedNodes);
            var allowed=new HashSet<string>(allowedNodes,StringComparer.Ordinal);var result=Clone(local);
            foreach(string mode in new[]{"Shapes","Words","Mixed"}){
                var destination=Journey(result,mode,true);
                var sources=new[]{Journey(remote,mode,false),Journey(baseProfile,mode,false),Journey(local,mode,false)}.Where(j=>j!=null).ToArray();
                destination.Clears=MergeClears(sources.SelectMany(j=>j.Clears).Where(c=>allowed.Contains(c.CampaignNodeId)));
                destination.HeardStory=sources.SelectMany(j=>j.HeardStory).Distinct().OrderBy(n=>n).ToList();
                destination.SeenMechanics=sources.SelectMany(j=>j.SeenMechanics).Distinct().ToList();
                destination.FirstGestureShown=sources.Any(j=>j.FirstGestureShown);destination.Records=MergeRecords(sources.SelectMany(j=>j.Records));
                if(mode==result.PreferredMode)destination.Apply(result);
            }
            result.SeenCoachPrompts=remote.SeenCoachPrompts.Concat(baseProfile.SeenCoachPrompts).Concat(local.SeenCoachPrompts).Distinct(StringComparer.Ordinal).ToList();
            bool parcelConflict=MergeRewards(result,new[]{remote,baseProfile,local},allowed);
            var ledger=Clone(remote.Monetization);bool economic=retainRemoteEconomy;
            if(!retainRemoteEconomy)economic=TryEconomy(remote.Monetization,baseProfile.Monetization,local.Monetization,
                result.Rewards.EarnedCoins(allowedNodes),result.Rewards.CosmeticClearCount,result.Rewards,allowVerifiedGrants,out ledger);
            bool conflict=!economic||parcelConflict;
            result.Monetization=conflict?Clone(remote.Monetization):ledger;
            ReconcileParcelClaims(remote,result);
            if((long)result.Rewards.EarnedCoins(allowedNodes)+result.Monetization.BonusCoins-result.Monetization.SpentCoins<0){
                conflict=true;result.Monetization=Clone(remote.Monetization);ReconcileParcelClaims(remote,result);
            }
            result.Monetization.HideVideoOffers=local.Monetization.HideVideoOffers;
            result.Rewards.EquippedGift=retainRemoteEconomy?remote.Rewards.EquippedGift:
                MergeEquipment(remote.Rewards.EquippedGift,baseProfile.Rewards.EquippedGift,local.Rewards.EquippedGift);
            if(result.Rewards.EquippedGift!=null&&!result.Rewards.Owns(result.Rewards.EquippedGift)&&!result.Monetization.Cosmetics.Contains(result.Rewards.EquippedGift))result.Rewards.EquippedGift=null;
            result.Rewards.EquippedScenery=retainRemoteEconomy?remote.Rewards.EquippedScenery:
                MergeEquipment(remote.Rewards.EquippedScenery,baseProfile.Rewards.EquippedScenery,local.Rewards.EquippedScenery);
            result.Rewards.EquippedFrame=retainRemoteEconomy?remote.Rewards.EquippedFrame:
                MergeEquipment(remote.Rewards.EquippedFrame,baseProfile.Rewards.EquippedFrame,local.Rewards.EquippedFrame);
            ReconcileScreenEquipment(result,remote);
            result.Validate();return new CloudMergeResult(result,conflict);
        }

        static string MergeEquipment(string remote,string baseline,string local)=>local==baseline?remote:local;
        static void ReconcileScreenEquipment(PlayerProfile profile,PlayerProfile remote=null)
        {
            if(profile.Rewards.EquippedScenery!=null&&!profile.Rewards.Owns(profile.Rewards.EquippedScenery)&&!profile.Monetization.Cosmetics.Contains(profile.Rewards.EquippedScenery))
                profile.Rewards.EquippedScenery=remote?.Rewards.EquippedScenery;
            if(profile.Rewards.EquippedFrame!=null&&!profile.Rewards.Owns(profile.Rewards.EquippedFrame)&&!profile.Monetization.Cosmetics.Contains(profile.Rewards.EquippedFrame))
                profile.Rewards.EquippedFrame=remote?.Rewards.EquippedFrame;
        }

        static void ReconcileParcelClaims(PlayerProfile remote,PlayerProfile result)
        {
            // A rejected inventory delta must leave its tool chest available to retry.
            foreach(var parcel in result.Rewards.Parcels.Where(p=>p.Kind=="tool"&&p.Claimed&&!p.DuplicateCoins))
                if(!result.Monetization.Grants.Any(g=>result.Rewards.MatchesToolGrant(g)&&g.Id==JourneyRewards.ParcelToolGrantId(parcel.Order)))parcel.Claimed=false;
            foreach(var parcel in result.Rewards.Parcels.Where(p=>p.Kind=="cosmetic"&&p.Claimed&&p.DuplicateCoins))
                if(!remote.Rewards.Parcels.Any(p=>p.Order==parcel.Order&&p.Claimed)&&!result.Rewards.Owns(parcel.GiftId)
                    &&!result.Monetization.Cosmetics.Contains(parcel.GiftId)){parcel.Claimed=false;parcel.DuplicateCoins=false;}
        }

        public static CloudMergeResult ImportLocal(PlayerProfile local,IReadOnlyList<string> allowedNodes)
        {
            if(local==null)throw new ArgumentNullException(nameof(local));CheckNodes(allowedNodes);local.Validate();
            var candidate=Clone(local);var old=local.Monetization;var ledger=new MonetizationLedger{HideVideoOffers=old.HideVideoOffers,SpentCoins=old.SpentCoins,
                WayfinderScrolls=old.WayfinderScrolls,UndoSpools=old.UndoSpools,ReturnHourglasses=old.ReturnHourglasses,SignalLanterns=old.SignalLanterns,LandingLenses=old.LandingLenses,RepairKits=old.RepairKits};
            var paidCosmetics=old.Grants.Where(g=>g.Kind=="cosmetic").Select(g=>g.Product).ToHashSet(StringComparer.Ordinal);
            ledger.Cosmetics=old.Cosmetics.Where(c=>!paidCosmetics.Contains(c)).ToList();
            ledger.Grants=old.Grants.Where(g=>g.Kind=="parcel_tool").Select(Clone).ToList();candidate.Monetization=ledger;
            if(candidate.Rewards.EquippedGift!=null&&!candidate.Rewards.Owns(candidate.Rewards.EquippedGift)&&!ledger.Cosmetics.Contains(candidate.Rewards.EquippedGift))candidate.Rewards.EquippedGift=null;
            ReconcileScreenEquipment(candidate);
            var imported=Merge(new PlayerProfile(),new PlayerProfile(),candidate,allowedNodes);
            return imported;
        }

        static void CheckNodes(IReadOnlyList<string> nodes)
        {
            if(nodes==null||nodes.Count>10000||nodes.Any(string.IsNullOrWhiteSpace)||nodes.Distinct(StringComparer.Ordinal).Count()!=nodes.Count)
                throw new ArgumentException("A unique ordered published catalog is required.",nameof(nodes));
        }
        static void ValidateAuthoritativeEconomy(PlayerProfile profile,IReadOnlyList<string> allowedNodes)
        {
            var ledger=profile.Monetization;
            if(ledger.BonusCoins!=Coins(ledger.Grants)||ledger.HintTokens!=Hints(ledger.Grants)-ledger.PaidHintStates.Count
                ||ledger.NoAutomaticAds!=ledger.Grants.Any(g=>g.Kind=="no_auto")
                ||(long)profile.Rewards.EarnedCoins(allowedNodes)+ledger.BonusCoins-ledger.SpentCoins<0)
                throw new InvalidDataException("The authoritative cloud economy is inconsistent.");
            long assets=0;var stocks=Stocks(ledger);
            bool currentPrices=profile.Rewards.CoinEconomyRevision>=1;
            for(int i=0;i<stocks.Length;i++)assets+=(long)Math.Max(0,stocks[i]-ParcelToolCount(ledger.Grants,(TravelTool)i))
                *(currentPrices?ShopCatalog.Price((TravelTool)i):ShopCatalog.HistoricalPrice((TravelTool)i));
            var paid=ledger.Grants.Where(g=>g.Kind=="cosmetic").Select(g=>g.Product).ToHashSet(StringComparer.Ordinal);
            assets+=ledger.Cosmetics.Where(c=>!paid.Contains(c)).Sum(c=>(long)(currentPrices?ShopCatalog.TrophyPrice(c):ShopCatalog.HistoricalTrophyPrice(c)));
            if(assets>ledger.SpentCoins||ledger.Cosmetics.Any(c=>!paid.Contains(c)
                &&profile.Rewards.CosmeticClearCount<ShopCatalog.PurchaseClearMilestone(c)))
                throw new InvalidDataException("Cloud inventory is not financed by its ledger or progress.");
        }
        static ModeJourney Journey(PlayerProfile profile,string mode,bool create)
        {
            if(profile.PreferredMode==mode)return ModeJourney.Capture(profile);
            var journey=profile.ModeJourneys.FirstOrDefault(j=>j.Mode==mode);
            if(journey==null&&create){journey=new ModeJourney{Mode=mode};profile.ModeJourneys.Add(journey);}return journey;
        }
        static List<ClearReceipt> MergeClears(IEnumerable<ClearReceipt> receipts)
        {
            var result=new List<ClearReceipt>();
            foreach(var group in receipts.GroupBy(c=>c.CampaignNodeId,StringComparer.Ordinal)){
                int stars=group.Max(c=>c.BestStarHalfUnits),score=group.Max(c=>c.BestScore);
                var starSource=group.Where(c=>c.Replay!=null&&c.BestStarHalfUnits==stars).OrderByDescending(c=>c.BestScore).FirstOrDefault()??group.First();
                var receipt=Clone(starSource);
                if(starSource.BestScore<score){
                    var scoreSource=group.FirstOrDefault(c=>c.BestScore==score&&(c.ScoreReplay??c.Replay)!=null);
                    receipt.ScoreReplay=Clone(scoreSource?.ScoreReplay??scoreSource?.Replay);
                }
                var replaySource=group.OrderByDescending(c=>c.ReplayRewardHalfUnits).First();
                receipt.ReplayRewardStars=replaySource.ReplayRewardStars;receipt.ReplayRewardHalfStar=replaySource.ReplayRewardHalfStar;
                if(receipt.ReplayRewardStars>0){receipt.ReplayRewardProof=Clone(replaySource.RewardProof);receipt.ReplayRewardPriorProof=Clone(replaySource.PriorRewardProof);receipt.CompactRewardProofs();}
                receipt.BestStars=stars/2;receipt.BestHalfStar=(stars%2)!=0;receipt.BestScore=score;result.Add(receipt);
            }return result;
        }
        static int? Better(int? a,int? b)=>a.HasValue&&b.HasValue?Math.Min(a.Value,b.Value):a??b;
        static List<MasteryRecord> MergeRecords(IEnumerable<MasteryRecord> records)
        {
            var result=new List<MasteryRecord>();
            foreach(var group in records.GroupBy(r=>r.Key,StringComparer.Ordinal)){
                var record=Clone(group.First());foreach(var other in group){record.BestRoute=Math.Min(record.BestRoute,other.BestRoute);record.BestWithoutHint=Better(record.BestWithoutHint,other.BestWithoutHint);record.BestClean=Better(record.BestClean,other.BestClean);}result.Add(record);
            }return result;
        }
        static bool MergeRewards(PlayerProfile result,IEnumerable<PlayerProfile> profiles,HashSet<string> allowed)
        {
            var sources=profiles.Select(p=>p.Rewards).ToArray();var rewards=result.Rewards;var cleared=result.AllClears.Select(c=>c.CampaignNodeId).ToHashSet(StringComparer.Ordinal);
            rewards.SeenClearNodes=sources.SelectMany(r=>r.SeenClearNodes).Where(n=>allowed.Contains(n)&&cleared.Contains(n)).Distinct(StringComparer.Ordinal).ToList();
            rewards.CoinEconomyRevision=sources.All(r=>r.CoinEconomyRevision>=1)?1:0;
            rewards.CoinIncomeCarryover=rewards.CoinEconomyRevision==1?sources.Max(r=>r.CoinIncomeCarryover):0;
            var credits=sources.SelectMany(r=>r.ReplayCredits??Enumerable.Empty<ReplayCredit>()).Where(c=>allowed.Contains(c.NodeId)&&cleared.Contains(c.NodeId))
                .GroupBy(c=>c.NodeId,StringComparer.Ordinal).Select(g=>new ReplayCredit{NodeId=g.Key,Stars=g.Max(c=>c.HalfUnits)/2,HalfStar=(g.Max(c=>c.HalfUnits)%2)!=0}).ToList();
            rewards.ReplayCredits=credits.Count==0?null:credits;
            rewards.VisitDates=sources.SelectMany(r=>r.VisitDates).Distinct(StringComparer.Ordinal).OrderBy(d=>d,StringComparer.Ordinal).Take(7).ToList();
            rewards.Deliveries=sources.SelectMany(r=>r.Deliveries).Where(d=>allowed.Contains(d.NodeId)).GroupBy(d=>d.NodeId,StringComparer.Ordinal)
                .Select(g=>new DeliveryFacts{NodeId=g.Key,Flags=g.Aggregate(0,(flags,d)=>flags|d.Flags)}).ToList();
            rewards.Records=MergeRecords(sources.SelectMany(r=>r.Records));bool conflict=false;var parcels=new List<ParcelReceipt>();
            foreach(var source in sources)foreach(var candidate in source.Parcels.OrderBy(p=>p.Order)){
                if(candidate.Order>rewards.SeenClearNodes.Count/3)continue;
                var existing=parcels.FirstOrDefault(p=>p.Order==candidate.Order);
                if(candidate.Order>12||candidate.RecipeVersion>=4||existing?.RecipeVersion>=4){
                    if(existing==null){parcels.Add(Clone(candidate));continue;}
                    if(existing.Kind!=candidate.Kind||existing.GiftId!=candidate.GiftId||existing.RecipeVersion!=candidate.RecipeVersion){
                        if(existing.Claimed&&candidate.Claimed){conflict=true;continue;}
                        if(candidate.Claimed&&!existing.Claimed){parcels[parcels.IndexOf(existing)]=Clone(candidate);continue;}
                        if(!existing.Claimed&&!candidate.Claimed&&candidate.RecipeVersion>existing.RecipeVersion){parcels[parcels.IndexOf(existing)]=Clone(candidate);continue;}
                        continue;
                    }
                    if(existing.Kind=="tool"||existing.Kind=="cosmetic"){
                        if(existing.Claimed&&candidate.Claimed&&existing.DuplicateCoins!=candidate.DuplicateCoins){conflict=true;continue;}
                        if(!existing.Claimed&&candidate.Claimed)existing.DuplicateCoins=candidate.DuplicateCoins;
                    }else existing.DuplicateCoins|=candidate.DuplicateCoins;
                    existing.Claimed|=candidate.Claimed;
                    continue;
                }
                if(existing!=null){
                    if(existing.GiftId!=candidate.GiftId||existing.RecipeVersion!=candidate.RecipeVersion){conflict=true;continue;}
                    existing.Claimed|=candidate.Claimed;continue;
                }
                if(parcels.Count(p=>p.Order<=12)>=12)continue;
                if(parcels.Count(p=>p.GiftId==candidate.GiftId)>=3){conflict=true;continue;}
                parcels.Add(Clone(candidate));
            }
            for(int order=1;order<=Math.Min(12,rewards.SeenClearNodes.Count/3);order++){
                if(parcels.Any(p=>p.Order==order))continue;
                parcels.Add(JourneyRewards.VarietyParcel(order));
            }
            for(int order=16;order<=rewards.SeenClearNodes.Count/3;order+=4)
                if(!parcels.Any(p=>p.Order==order))parcels.Add(JourneyRewards.VarietyParcel(order));
            rewards.Parcels=parcels.OrderBy(p=>p.Order).ToList();return conflict;
        }

        static bool SameGrant(GrantReceipt a,GrantReceipt b)=>a.Id==b.Id&&a.Kind==b.Kind&&a.Product==b.Product&&a.Amount==b.Amount;
        static bool ExtendsGrants(MonetizationLedger before,Dictionary<string,GrantReceipt> after)=>before.Grants.All(a=>after.TryGetValue(a.Id,out var b)&&SameGrant(a,b));
        static long Coins(IEnumerable<GrantReceipt> grants)=>grants.Where(g=>g.Kind=="coins"||g.Kind=="store_coins").Sum(g=>(long)g.Amount);
        static long Hints(IEnumerable<GrantReceipt> grants)=>grants.Where(g=>g.Kind=="hint").Sum(g=>(long)g.Amount);
        static int ParcelToolCount(IEnumerable<GrantReceipt> grants,TravelTool tool)=>grants.Count(g=>g.Kind=="parcel_tool"&&g.Product==tool.ToString());
        static int[] Stocks(MonetizationLedger ledger)=>new[]{ledger.WayfinderScrolls,ledger.UndoSpools,ledger.ReturnHourglasses,ledger.SignalLanterns,ledger.LandingLenses,ledger.RepairKits};
        static void SetStocks(MonetizationLedger ledger,int[] stocks)
        {ledger.WayfinderScrolls=stocks[0];ledger.UndoSpools=stocks[1];ledger.ReturnHourglasses=stocks[2];ledger.SignalLanterns=stocks[3];ledger.LandingLenses=stocks[4];ledger.RepairKits=stocks[5];}
        static bool TryEconomy(MonetizationLedger remote,MonetizationLedger baseline,MonetizationLedger local,int earned,int uniqueClears,JourneyRewards localRewards,bool allowVerified,out MonetizationLedger merged)
        {
            merged=null;
            var remoteGrants=remote.Grants.ToDictionary(g=>g.Id,StringComparer.Ordinal);var localGrants=local.Grants.ToDictionary(g=>g.Id,StringComparer.Ordinal);
            var baselineGrants=new HashSet<string>(baseline.Grants.Select(g=>g.Id),StringComparer.Ordinal);
            var localHints=new HashSet<string>(local.PaidHintStates,StringComparer.Ordinal);var remoteHints=new HashSet<string>(remote.PaidHintStates,StringComparer.Ordinal);
            if(!ExtendsGrants(baseline,remoteGrants)||!ExtendsGrants(baseline,localGrants)||!baseline.Cosmetics.All(local.Cosmetics.Contains)||!baseline.Cosmetics.All(remote.Cosmetics.Contains)
                ||!baseline.PaidHintStates.All(localHints.Contains)||!baseline.PaidHintStates.All(remoteHints.Contains)
                ||local.SpentCoins<baseline.SpentCoins||remote.SpentCoins<baseline.SpentCoins)return false;
            var newLocal=local.Grants.Where(g=>!baselineGrants.Contains(g.Id)).ToArray();
            if(!allowVerified&&newLocal.Any(g=>!(remoteGrants.TryGetValue(g.Id,out var r)&&SameGrant(g,r))&&!localRewards.MatchesToolGrant(g)))return false;
            if(newLocal.Any(g=>remoteGrants.TryGetValue(g.Id,out var r)&&!SameGrant(g,r)))return false;
            if(baseline.BonusCoins!=Coins(baseline.Grants)||remote.BonusCoins!=Coins(remote.Grants)||local.BonusCoins-baseline.BonusCoins!=Coins(newLocal)
                ||baseline.HintTokens!=Hints(baseline.Grants)-baseline.PaidHintStates.Count||remote.HintTokens!=Hints(remote.Grants)-remote.PaidHintStates.Count
                ||local.HintTokens-baseline.HintTokens!=Hints(newLocal)-(local.PaidHintStates.Count-baseline.PaidHintStates.Count))return false;
            if(baseline.NoAutomaticAds&&!local.NoAutomaticAds||baseline.NoAutomaticAds&&!remote.NoAutomaticAds
                ||local.NoAutomaticAds&&!baseline.NoAutomaticAds&&!newLocal.Any(g=>g.Kind=="no_auto")
                ||remote.NoAutomaticAds!=remote.Grants.Any(g=>g.Kind=="no_auto"))return false;
            try{
                var candidate=Clone(remote);candidate.Grants.AddRange(newLocal.Where(g=>!remoteGrants.ContainsKey(g.Id)).Select(Clone));
                candidate.BonusCoins=checked((int)Coins(candidate.Grants));candidate.NoAutomaticAds=candidate.Grants.Any(g=>g.Kind=="no_auto");
                candidate.PaidHintStates=remote.PaidHintStates.Concat(local.PaidHintStates).Distinct(StringComparer.Ordinal).ToList();
                candidate.HintTokens=checked((int)(Hints(candidate.Grants)-candidate.PaidHintStates.Count));
                int localSpent=local.SpentCoins-baseline.SpentCoins;candidate.SpentCoins=checked(remote.SpentCoins+localSpent);
                var baseStocks=Stocks(baseline);var localStocks=Stocks(local);var remoteStocks=Stocks(remote);var stocks=new int[ShopCatalog.ToolCount];long minimumCost=0;
                for(int i=0;i<stocks.Length;i++){
                    int delta=localStocks[i]-baseStocks[i];
                    int received=ParcelToolCount(newLocal,(TravelTool)i);
                    int repeated=ParcelToolCount(newLocal.Where(g=>remoteGrants.TryGetValue(g.Id,out var r)&&SameGrant(g,r)),(TravelTool)i);
                    stocks[i]=remoteStocks[i]+delta-repeated;
                    if(stocks[i]<0||stocks[i]>ShopCatalog.StockLimit)return false;
                    minimumCost+=(long)Math.Max(0,delta-received)*ShopCatalog.Price((TravelTool)i);
                }
                var verifiedCosmetics=newLocal.Where(g=>g.Kind=="cosmetic").Select(g=>g.Product).ToHashSet(StringComparer.Ordinal);
                minimumCost+=local.Cosmetics.Where(c=>!baseline.Cosmetics.Contains(c)&&!verifiedCosmetics.Contains(c)).Sum(c=>(long)ShopCatalog.TrophyPrice(c));
                if(localSpent<minimumCost||(long)earned+candidate.BonusCoins-candidate.SpentCoins<0)return false;
                SetStocks(candidate,stocks);candidate.Cosmetics=remote.Cosmetics.Concat(local.Cosmetics).Concat(verifiedCosmetics).Distinct(StringComparer.Ordinal).ToList();
                if(candidate.Cosmetics.Any(c=>!candidate.Grants.Any(g=>g.Kind=="cosmetic"&&g.Product==c)
                    &&uniqueClears<ShopCatalog.PurchaseClearMilestone(c)))return false;
                candidate.Validate();merged=candidate;return true;
            }catch(Exception e)when(e is OverflowException||e is ArgumentException){return false;}
        }
    }
}
