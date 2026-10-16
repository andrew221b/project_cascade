using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Cascade.Solver;

namespace Cascade.Application
{
    public sealed class GrantReceipt { public string Id,Kind,Product;public int Amount; }
    public sealed class MonetizationLedger
    {
        public const int MaxGrantReceipts=10000,MaxPaidHintStates=20000;
        public int BonusCoins,HintTokens;
        public int SpentCoins,WayfinderScrolls,UndoSpools,ReturnHourglasses,SignalLanterns,LandingLenses;
        [Newtonsoft.Json.JsonProperty(DefaultValueHandling=Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public int RepairKits;
        // NoAutomaticAds is a grandfathered lifetime purchase. Subscription expiry is
        // server-verified and never promoted into that permanent flag.
        public bool NoAutomaticAds,HideVideoOffers;
        public long NoAdsWeekExpiresUnix,NoAdsMonthExpiresUnix,NoAdsYearExpiresUnix;
        public long SubscriptionExpiry(string product)=>product==PurchaseCatalog.NoAdsWeek?NoAdsWeekExpiresUnix:product==PurchaseCatalog.NoAdsMonth?NoAdsMonthExpiresUnix:product==PurchaseCatalog.NoAdsYear?NoAdsYearExpiresUnix:0;
        public void SetSubscriptionExpiry(string product,long expiry)
        {
            if(expiry<0||expiry>253402300799L)throw new ArgumentOutOfRangeException(nameof(expiry));
            if(product==PurchaseCatalog.NoAdsWeek)NoAdsWeekExpiresUnix=expiry;
            else if(product==PurchaseCatalog.NoAdsMonth)NoAdsMonthExpiresUnix=expiry;
            else if(product==PurchaseCatalog.NoAdsYear)NoAdsYearExpiresUnix=expiry;
            else throw new ArgumentException("Unknown subscription product.",nameof(product));
        }
        public bool HasNoAutomaticAds=>NoAutomaticAds||Math.Max(NoAdsWeekExpiresUnix,Math.Max(NoAdsMonthExpiresUnix,NoAdsYearExpiresUnix))>DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string ActiveNoAdsSubscription=>NoAdsYearExpiresUnix>DateTimeOffset.UtcNow.ToUnixTimeSeconds()?PurchaseCatalog.NoAdsYear:NoAdsMonthExpiresUnix>DateTimeOffset.UtcNow.ToUnixTimeSeconds()?PurchaseCatalog.NoAdsMonth:NoAdsWeekExpiresUnix>DateTimeOffset.UtcNow.ToUnixTimeSeconds()?PurchaseCatalog.NoAdsWeek:null;
        public List<GrantReceipt> Grants=new List<GrantReceipt>();
        public List<string> Cosmetics=new List<string>(),PaidHintStates=new List<string>();
        public static bool IsParcelSubject(string value)
        {
            const string prefix="parcel:";
            return value!=null&&value.StartsWith(prefix,StringComparison.Ordinal)
                &&int.TryParse(value.Substring(prefix.Length),NumberStyles.None,CultureInfo.InvariantCulture,out var order)
                &&order>0&&order<=540&&value==prefix+order.ToString(CultureInfo.InvariantCulture);
        }
        public static string ParcelSubject(int order)=>"parcel:"+order.ToString(CultureInfo.InvariantCulture);
        public MonetizationLedger Copy()=>new MonetizationLedger{BonusCoins=BonusCoins,HintTokens=HintTokens,
            SpentCoins=SpentCoins,WayfinderScrolls=WayfinderScrolls,UndoSpools=UndoSpools,ReturnHourglasses=ReturnHourglasses,SignalLanterns=SignalLanterns,LandingLenses=LandingLenses,RepairKits=RepairKits,
            NoAutomaticAds=NoAutomaticAds,HideVideoOffers=HideVideoOffers,NoAdsWeekExpiresUnix=NoAdsWeekExpiresUnix,NoAdsMonthExpiresUnix=NoAdsMonthExpiresUnix,NoAdsYearExpiresUnix=NoAdsYearExpiresUnix,Cosmetics=new List<string>(Cosmetics),
            PaidHintStates=new List<string>(PaidHintStates),Grants=Grants.Select(g=>new GrantReceipt{Id=g.Id,Kind=g.Kind,Product=g.Product,Amount=g.Amount}).ToList()};
        public void Validate()
        {
            if(NoAdsWeekExpiresUnix<0||NoAdsMonthExpiresUnix<0||NoAdsYearExpiresUnix<0||NoAdsWeekExpiresUnix>253402300799L||NoAdsMonthExpiresUnix>253402300799L||NoAdsYearExpiresUnix>253402300799L||BonusCoins<0||HintTokens<0||SpentCoins<0||WayfinderScrolls<0||WayfinderScrolls>ShopCatalog.StockLimit||UndoSpools<0||UndoSpools>ShopCatalog.StockLimit
                ||SignalLanterns<0||SignalLanterns>ShopCatalog.StockLimit||LandingLenses<0||LandingLenses>ShopCatalog.StockLimit||RepairKits<0||RepairKits>ShopCatalog.StockLimit
                ||ReturnHourglasses<0||ReturnHourglasses>ShopCatalog.StockLimit||Grants==null||Grants.Count>MaxGrantReceipts||Grants.Any(g=>g==null||string.IsNullOrWhiteSpace(g.Id)||g.Id.Length>256
                ||!(g.Kind=="coins"&&g.Amount==40||g.Kind=="store_coins"&&PurchaseCatalog.Coins(g.Product)>0&&g.Amount==PurchaseCatalog.Coins(g.Product)||g.Kind=="hint"&&g.Amount==1||g.Kind=="no_auto"&&g.Amount==0&&g.Product==PurchaseCatalog.NoAutomaticAds||g.Kind=="subscription"&&g.Amount==0&&PurchaseCatalog.IsNoAdsSubscription(g.Product)||g.Kind=="cosmetic"&&g.Amount==0&&ShopCatalog.MikaTrophyIds.Contains(g.Product)&&!ShopCatalog.ExpansionTrophyIds.Contains(g.Product)&&!ShopCatalog.IsParcelExclusive(g.Product)
                    ||g.Kind=="parcel_tool"&&g.Amount==1&&ShopCatalog.TryTool(g.Product,out _)&&g.Id.StartsWith("parcel_tool_",StringComparison.Ordinal)))
                ||Grants.Select(g=>g.Id).Distinct().Count()!=Grants.Count||Cosmetics==null||Cosmetics.Distinct().Count()!=Cosmetics.Count
                ||Grants.Where(g=>g.Kind=="coins"&&IsParcelSubject(g.Product)).Select(g=>g.Product).Distinct().Count()!=Grants.Count(g=>g.Kind=="coins"&&IsParcelSubject(g.Product))
                ||Cosmetics.Any(g=>!ShopCatalog.TrophyIds.Contains(g)||ShopCatalog.IsParcelExclusive(g))||PaidHintStates==null||PaidHintStates.Count>MaxPaidHintStates
                ||PaidHintStates.Distinct().Count()!=PaidHintStates.Count||PaidHintStates.Any(k=>k==null||k.Length!=101||!Guid.TryParse(k.Substring(0,36),out _)||k[36]!='|'
                    ||!k.Substring(37).All(c=>c>='0'&&c<='9'||c>='a'&&c<='f')))
                throw new ArgumentException("Invalid monetization ledger.");
        }
    }
    public enum AdSurface { Gameplay,Tutorial,Fail,Retry,Language,Win,Inventory,Other }
    public sealed class RetryAdCadence
    {
        int retriesSinceBreak,nextInterval=2;
        public bool RecordRetry()
        {
            if(retriesSinceBreak<nextInterval)retriesSinceBreak++;
            return retriesSinceBreak>=nextInterval;
        }
        public void AdShown(){if(retriesSinceBreak<nextInterval)return;retriesSinceBreak=0;nextInterval=nextInterval==2?3:2;}
    }
    public sealed class AdContext
    {
        public AdSurface Surface;
        public bool CoreAccepted,PrivacyReady,ProviderEnabled,FirstSession,SavedWin,BaseRewardGiven,ContinueVisible,ChapterFinal,Replay;
        public string NodeId,RewardSubject;public int UniqueClears;
    }
    public sealed class AdTicket
    {
        public string Id {get;internal set;} public string RewardKind {get;internal set;} public string RewardSubject {get;internal set;}
        public int UniqueClears {get;internal set;} public bool Rewarded {get;internal set;} public bool Opened {get;internal set;} public bool RetryInterstitial {get;internal set;}
    }
    // Disabled unless explicitly supplied an accepted core, privacy-ready mock/provider.
    // Provider callbacks can grant inventory; they never execute puzzle commands.
    public sealed partial class AdService
    {
        readonly Action<MonetizationLedger> persist;
        readonly HashSet<string> observedWins=new HashSet<string>();
        readonly Dictionary<string,AdTicket> pendingRewards=new Dictionary<string,AdTicket>();
        readonly RetryAdCadence retryAdCadence=new RetryAdCadence();
        AdTicket active;
        int intervalAnchor,automaticShows;
        double foregroundSeconds;
        public MonetizationLedger Ledger {get;private set;}
        public bool FullscreenActive=>active!=null;
        public int AutomaticShows=>automaticShows;
        public bool PreviewOnly {get;}
        public bool CanReceiveGrant=>Ledger.Grants.Count+pendingRewards.Count<MonetizationLedger.MaxGrantReceipts;
        public bool HasParcelBonus(string subject)=>MonetizationLedger.IsParcelSubject(subject)&&Ledger.Grants.Any(g=>g.Kind=="coins"&&g.Product==subject);
        public void ReloadLedger(MonetizationLedger authoritative){if(PreviewOnly)return;authoritative.Validate();Ledger=authoritative;}
        public AdService(MonetizationLedger ledger,Action<MonetizationLedger> persist,int uniqueAtSessionStart=0,bool previewOnly=false)
        {ledger.Validate();Ledger=ledger;this.persist=persist??throw new ArgumentNullException(nameof(persist));intervalAnchor=uniqueAtSessionStart;PreviewOnly=previewOnly;}
        public void AdvanceForeground(double seconds)
        {if(seconds<0||double.IsNaN(seconds)||double.IsInfinity(seconds))throw new ArgumentOutOfRangeException(nameof(seconds));if(active==null)foregroundSeconds+=seconds;}
        static bool Ready(AdContext c)=>c!=null&&c.CoreAccepted&&c.PrivacyReady&&c.ProviderEnabled;
        public AdTicket TryAutomatic(AdContext c,bool adReady)
        {
            if(c==null||c.Surface!=AdSurface.Win||string.IsNullOrWhiteSpace(c.NodeId)||!observedWins.Add(c.NodeId))return null;
            if(!Ready(c)||active!=null||Ledger.HasNoAutomaticAds||c.UniqueClears<2||c.UniqueClears%2!=0||!c.SavedWin||!c.BaseRewardGiven||c.Replay
                ||c.ContinueVisible||c.ChapterFinal||!adReady)return null;
            return active=new AdTicket{Id=Guid.NewGuid().ToString("D"),UniqueClears=c.UniqueClears};
        }
        public AdTicket TryAutomaticRetry(AdContext c,bool adReady)
        {
            // Automatic breaks are attached only to alternating completed levels.
            return null;
        }
        public AdTicket TryRewarded(AdContext c,string rewardKind,bool adReady)
        {
            if(!Ready(c)||c.Surface!=AdSurface.Inventory||active!=null||Ledger.HideVideoOffers||!adReady
                ||!new[]{"coins","hint","parcel_bonus"}.Contains(rewardKind)
                ||rewardKind=="parcel_bonus"&&(!MonetizationLedger.IsParcelSubject(c.RewardSubject)||HasParcelBonus(c.RewardSubject))
                ||pendingRewards.Count>=32||!CanReceiveGrant)return null;
            active=new AdTicket{Id=Guid.NewGuid().ToString("D"),Rewarded=true,RewardKind=rewardKind,RewardSubject=rewardKind=="parcel_bonus"?c.RewardSubject:null,UniqueClears=c.UniqueClears};
            pendingRewards.Add(active.Id,active);return active;
        }
        public bool Opened(string ticket)
        {
            if(active==null||active.Id!=ticket||active.Opened)return false;
            active.Opened=true;if(!active.Rewarded)automaticShows++;
            if(active.RetryInterstitial)retryAdCadence.AdShown();
            intervalAnchor=active.UniqueClears;foregroundSeconds=0;return true;
        }
        public void Closed(string ticket){if(active?.Id==ticket)active=null;}
        public void Failed(string ticket){Closed(ticket);if(ticket!=null)pendingRewards.Remove(ticket);}
        bool Commit(MonetizationLedger candidate)
        {
            candidate.Validate();
            try{persist(candidate);Ledger=candidate;return true;}
            catch(Exception e)when(e is IOException||e is UnauthorizedAccessException||e is InvalidOperationException){return false;}
        }
        public bool RewardConfirmed(string ticket,string receipt,bool providerVerified)
        {
            if(!providerVerified||ticket==null||string.IsNullOrWhiteSpace(receipt)||receipt.Length>256||!pendingRewards.TryGetValue(ticket,out var request)
                ||!request.Opened||Ledger.Grants.Count>=MonetizationLedger.MaxGrantReceipts||Ledger.Grants.Any(g=>g.Id==receipt))return false;
            var next=Ledger.Copy();int amount=request.RewardKind=="coins"||request.RewardKind=="parcel_bonus"?40:1;
            if(request.RewardKind=="parcel_bonus"&&HasParcelBonus(request.RewardSubject))return false;
            string grantKind=request.RewardKind=="parcel_bonus"?"coins":request.RewardKind;
            string product=request.RewardKind=="parcel_bonus"?request.RewardSubject:null;
            next.Grants.Add(new GrantReceipt{Id=receipt,Kind=grantKind,Product=product,Amount=amount});
            if(request.RewardKind=="coins"||request.RewardKind=="parcel_bonus")next.BonusCoins=checked(next.BonusCoins+amount);else next.HintTokens=checked(next.HintTokens+amount);
            if(!Commit(next))return false;pendingRewards.Remove(ticket);return true;
        }
        public bool VerifiedMockPurchase(string product,string receipt,bool providerVerified,bool restoring=false)
        {
            if(!providerVerified||!CanReceiveGrant||string.IsNullOrWhiteSpace(receipt)||receipt.Length>256||Ledger.Grants.Any(g=>g.Id==receipt))return false;
            string gift=product!=null&&product.StartsWith("mock.keepsake.",StringComparison.Ordinal)?product.Substring(14):null;
            bool noAuto=product=="mock.no_automatic_ads";
            if(!noAuto&&(!ShopCatalog.MikaTrophyIds.Contains(gift)||ShopCatalog.NewTrophyIds.Contains(gift)||ShopCatalog.ExpansionTrophyIds.Contains(gift)||ShopCatalog.IsParcelExclusive(gift)))return false;
            var next=Ledger.Copy();next.Grants.Add(new GrantReceipt{Id=receipt,Kind=noAuto?"no_auto":"cosmetic",Product=noAuto?PurchaseCatalog.NoAutomaticAds:gift});
            if(noAuto)next.NoAutomaticAds=true;else if(!next.Cosmetics.Contains(gift))next.Cosmetics.Add(gift);
            return Commit(next); // Only non-consumables exist in this mock catalog; restore never awards coins/tokens.
        }
        public bool VerifiedStorePurchase(string product,string receipt,bool providerVerified)
        {
            if(!providerVerified||(product!=PurchaseCatalog.NoAutomaticAds&&PurchaseCatalog.Coins(product)==0)||string.IsNullOrWhiteSpace(receipt)||receipt.Length>256)return false;
            var existing=Ledger.Grants.FirstOrDefault(g=>g.Id==receipt);
            int amount=PurchaseCatalog.Coins(product);string kind=amount>0?"store_coins":"no_auto";
            if(existing!=null)return existing.Kind==kind&&existing.Product==product&&existing.Amount==amount;
            if(!CanReceiveGrant)return false;
            var next=Ledger.Copy();if(amount>0)next.BonusCoins=checked(next.BonusCoins+amount);else next.NoAutomaticAds=true;
            next.Grants.Add(new GrantReceipt{Id=receipt,Kind=kind,Product=product,Amount=amount});
            return Commit(next);
        }
        public bool TrySpendHint(AttemptService attempt,HintOffer offer)
        {
            if(attempt==null||!attempt.CanShowBonusHint||attempt.RevealedHintStates.ContainsKey(offer.StateHash)||!attempt.IsCurrent(offer)||offer.Result.Status!=HintStatus.Step)return false;
            string key=attempt.AttemptId.ToString("D")+"|"+offer.StateHash;
            bool paid=Ledger.PaidHintStates.Contains(key);if(!paid&&(Ledger.HintTokens==0||Ledger.PaidHintStates.Count>=MonetizationLedger.MaxPaidHintStates))return false;
            var next=Ledger.Copy();if(!paid){next.HintTokens--;next.PaidHintStates.Add(key);}
            return attempt.TryRevealBonusHint(offer,()=>Commit(next));
        }
    }
}
