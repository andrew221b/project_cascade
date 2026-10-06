using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Solver;

namespace Cascade.Application
{
    public sealed class GrantReceipt { public string Id,Kind,Product;public int Amount; }
    public sealed class MonetizationLedger
    {
        public int BonusCoins,HintTokens;
        public int SpentCoins,WayfinderScrolls,UndoSpools,ReturnHourglasses;
        public bool NoAutomaticAds,HideVideoOffers;
        public List<GrantReceipt> Grants=new List<GrantReceipt>();
        public List<string> Cosmetics=new List<string>(),PaidHintStates=new List<string>();
        public MonetizationLedger Copy()=>new MonetizationLedger{BonusCoins=BonusCoins,HintTokens=HintTokens,
            SpentCoins=SpentCoins,WayfinderScrolls=WayfinderScrolls,UndoSpools=UndoSpools,ReturnHourglasses=ReturnHourglasses,
            NoAutomaticAds=NoAutomaticAds,HideVideoOffers=HideVideoOffers,Cosmetics=new List<string>(Cosmetics),
            PaidHintStates=new List<string>(PaidHintStates),Grants=Grants.Select(g=>new GrantReceipt{Id=g.Id,Kind=g.Kind,Product=g.Product,Amount=g.Amount}).ToList()};
        public void Validate()
        {
            if(BonusCoins<0||HintTokens<0||SpentCoins<0||WayfinderScrolls<0||WayfinderScrolls>ShopCatalog.StockLimit||UndoSpools<0||UndoSpools>ShopCatalog.StockLimit
                ||ReturnHourglasses<0||ReturnHourglasses>ShopCatalog.StockLimit||Grants==null||Grants.Count>10000||Grants.Any(g=>g==null||string.IsNullOrWhiteSpace(g.Id)||g.Id.Length>256
                ||!(g.Kind=="coins"&&g.Amount==40||g.Kind=="hint"&&g.Amount==1||g.Kind=="no_auto"&&g.Amount==0||g.Kind=="cosmetic"&&g.Amount==0&&JourneyRewards.Gifts.Contains(g.Product)))
                ||Grants.Select(g=>g.Id).Distinct().Count()!=Grants.Count||Cosmetics==null||Cosmetics.Distinct().Count()!=Cosmetics.Count
                ||Cosmetics.Any(g=>!JourneyRewards.Gifts.Contains(g))||PaidHintStates==null||PaidHintStates.Count>20000
                ||PaidHintStates.Distinct().Count()!=PaidHintStates.Count||PaidHintStates.Any(k=>k==null||k.Length!=101||!Guid.TryParse(k.Substring(0,36),out _)||k[36]!='|'
                    ||!k.Substring(37).All(c=>c>='0'&&c<='9'||c>='a'&&c<='f')))
                throw new ArgumentException("Invalid monetization ledger.");
        }
    }
    public enum AdSurface { Gameplay,Tutorial,Fail,Retry,Language,Win,Inventory,Other }
    public sealed class AdContext
    {
        public AdSurface Surface;
        public bool CoreAccepted,PrivacyReady,ProviderEnabled,FirstSession,SavedWin,BaseRewardGiven,ContinueVisible,ChapterFinal,Replay;
        public string NodeId;public int UniqueClears;
    }
    public sealed class AdTicket
    {
        public string Id {get;internal set;} public string RewardKind {get;internal set;}
        public int UniqueClears {get;internal set;} public bool Rewarded {get;internal set;} public bool Opened {get;internal set;}
    }
    // Disabled unless explicitly supplied an accepted core, privacy-ready mock/provider.
    // Provider callbacks can grant inventory; they never execute puzzle commands.
    public sealed partial class AdService
    {
        readonly Action<MonetizationLedger> persist;
        readonly HashSet<string> observedWins=new HashSet<string>();
        readonly Dictionary<string,AdTicket> pendingRewards=new Dictionary<string,AdTicket>();
        AdTicket active;
        int intervalAnchor,automaticShows;
        double foregroundSeconds;
        public MonetizationLedger Ledger {get;private set;}
        public bool FullscreenActive=>active!=null;
        public int AutomaticShows=>automaticShows;
        public bool PreviewOnly {get;}
        public AdService(MonetizationLedger ledger,Action<MonetizationLedger> persist,int uniqueAtSessionStart=0,bool previewOnly=false)
        {ledger.Validate();Ledger=ledger;this.persist=persist??throw new ArgumentNullException(nameof(persist));intervalAnchor=uniqueAtSessionStart;PreviewOnly=previewOnly;}
        public void AdvanceForeground(double seconds)
        {if(seconds<0||double.IsNaN(seconds)||double.IsInfinity(seconds))throw new ArgumentOutOfRangeException(nameof(seconds));if(active==null)foregroundSeconds+=seconds;}
        static bool Ready(AdContext c)=>c!=null&&c.CoreAccepted&&c.PrivacyReady&&c.ProviderEnabled;
        public AdTicket TryAutomatic(AdContext c,bool adReady)
        {
            if(c==null||c.Surface!=AdSurface.Win||string.IsNullOrWhiteSpace(c.NodeId)||!observedWins.Add(c.NodeId))return null;
            if(!Ready(c)||active!=null||Ledger.NoAutomaticAds||c.UniqueClears<3||!c.SavedWin||!c.BaseRewardGiven||c.Replay
                ||c.ContinueVisible||c.ChapterFinal||c.UniqueClears-intervalAnchor<3||foregroundSeconds<180||automaticShows>=2||!adReady)return null;
            return active=new AdTicket{Id=Guid.NewGuid().ToString("D"),UniqueClears=c.UniqueClears};
        }
        public AdTicket TryRewarded(AdContext c,string rewardKind,bool adReady)
        {
            if(!Ready(c)||c.Surface!=AdSurface.Inventory||active!=null||Ledger.HideVideoOffers||!adReady
                ||!new[]{"coins","hint"}.Contains(rewardKind)||pendingRewards.Count>=32)return null;
            active=new AdTicket{Id=Guid.NewGuid().ToString("D"),Rewarded=true,RewardKind=rewardKind,UniqueClears=c.UniqueClears};
            pendingRewards.Add(active.Id,active);return active;
        }
        public bool Opened(string ticket)
        {
            if(active==null||active.Id!=ticket||active.Opened)return false;
            active.Opened=true;if(!active.Rewarded)automaticShows++;
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
                ||!request.Opened||Ledger.Grants.Any(g=>g.Id==receipt))return false;
            var next=Ledger.Copy();int amount=request.RewardKind=="coins"?40:1;
            next.Grants.Add(new GrantReceipt{Id=receipt,Kind=request.RewardKind,Amount=amount});
            if(request.RewardKind=="coins")next.BonusCoins=checked(next.BonusCoins+amount);else next.HintTokens=checked(next.HintTokens+amount);
            if(!Commit(next))return false;pendingRewards.Remove(ticket);return true;
        }
        public bool VerifiedMockPurchase(string product,string receipt,bool providerVerified,bool restoring=false)
        {
            if(!providerVerified||string.IsNullOrWhiteSpace(receipt)||receipt.Length>256||Ledger.Grants.Any(g=>g.Id==receipt))return false;
            string gift=product!=null&&product.StartsWith("mock.keepsake.",StringComparison.Ordinal)?product.Substring(14):null;
            bool noAuto=product=="mock.no_automatic_ads";
            if(!noAuto&&!JourneyRewards.Gifts.Contains(gift))return false;
            var next=Ledger.Copy();next.Grants.Add(new GrantReceipt{Id=receipt,Kind=noAuto?"no_auto":"cosmetic",Product=gift});
            if(noAuto)next.NoAutomaticAds=true;else if(!next.Cosmetics.Contains(gift))next.Cosmetics.Add(gift);
            return Commit(next); // Only non-consumables exist in this mock catalog; restore never awards coins/tokens.
        }
        public bool VerifiedStorePurchase(string product,string receipt,bool providerVerified)
        {
            if(!providerVerified||product!=PurchaseCatalog.NoAutomaticAds||string.IsNullOrWhiteSpace(receipt)||receipt.Length>256)return false;
            var existing=Ledger.Grants.FirstOrDefault(g=>g.Id==receipt);
            if(existing!=null)return existing.Kind=="no_auto"&&existing.Product==product;
            var next=Ledger.Copy();next.NoAutomaticAds=true;
            next.Grants.Add(new GrantReceipt{Id=receipt,Kind="no_auto",Product=product});
            return Commit(next);
        }
        public bool TrySpendHint(AttemptService attempt,HintOffer offer)
        {
            if(attempt==null||!attempt.IsCurrent(offer)||offer.Result.Status!=HintStatus.Step||attempt.RevealedHintStates.ContainsKey(offer.StateHash))return false;
            string key=attempt.AttemptId.ToString("D")+"|"+offer.StateHash;
            bool paid=Ledger.PaidHintStates.Contains(key);if(!paid&&Ledger.HintTokens==0)return false;
            var next=Ledger.Copy();if(!paid){next.HintTokens--;next.PaidHintStates.Add(key);}
            return attempt.TryRevealHintAtomically(offer,()=>Commit(next));
        }
    }
}
