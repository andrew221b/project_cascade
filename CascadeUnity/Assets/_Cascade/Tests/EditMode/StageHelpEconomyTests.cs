using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using NUnit.Framework;
using Newtonsoft.Json;
namespace Cascade.Tests
{
    public sealed class StageHelpEconomyTests
    {
        static AttemptService Board(int width=4,int floors=1,string node="C09"){
            var symbols=LevelDefinitionTests.Row(width==3?"CBA":width==4?"DCBA":"EDCBA");
            return new AttemptService(new LevelDefinition("hint-budget",node,"2.0","2.0","fixture",symbols,symbols.Reverse(),
                new[]{new TargetSegment(SegmentKind.Shapes,0,width)},Enumerable.Range(0,floors).Select(i=>new FloorDefinition(12,Enumerable.Repeat(true,width-1)))));
        }
        static HintOffer Hint(AttemptService a)=>AttemptService.CalculateHint(a.CaptureHintRequest());
        [TestCase(3,1,"C01",3)][TestCase(4,1,"C09",1)][TestCase(5,1,"C20",2)][TestCase(4,3,"C20",2)]
        public void BudgetMatchesStageComplexity(int width,int floors,string node,int limit)=>Assert.That(Board(width,floors,node).FreeHintLimit,Is.EqualTo(int.MaxValue));
        [TestCase(0,1,false,false)][TestCase(1,1,false,true)][TestCase(2,1,false,false)][TestCase(3,1,false,true)]
        [TestCase(1,0,false,true)][TestCase(0,0,false,false)][TestCase(1,0,true,false)]
        public void RewardedHintsAlternateWithFreeHints(int granted,int free,bool sameState,bool due)
            =>Assert.That(StageHintBudget.RewardedHintDue(granted,free,sameState),Is.EqualTo(due));
        [Test]public void FreeHintHistorySurvivesReloadWithoutAStageCap(){
            var a=Board(5);for(int i=0;i<3;i++){var hint=Hint(a);Assert.That(a.TryRevealFreeHint(hint,()=>true),Is.True);Assert.That(a.Execute(hint.Result.Command.Value).Accepted,Is.True);}
            var loaded=AttemptService.Restore(a.Level,JsonConvert.DeserializeObject<AttemptRecord>(JsonConvert.SerializeObject(AttemptRecord.Capture(a))));Assert.That(loaded.FreeHintStates.Count,Is.EqualTo(3));Assert.That(loaded.FreeHintsRemaining,Is.GreaterThan(0));Assert.That(loaded.Undo(),Is.True);Assert.That(loaded.TryRevealFreeHint(Hint(loaded),()=>true),Is.True);Assert.That(loaded.FreeHintStates.Count,Is.EqualTo(3));
        }
        [Test]public void RewardedHintsSpendOneTokenPerNewStateAndNeverExecuteCommands(){
            var a=Board();var free=Hint(a);a.TryRevealFreeHint(free,()=>true);a.Execute(free.Result.Command.Value);
            var ads=new AdService(new MonetizationLedger{HintTokens=3},_=>{});var hint=Hint(a);var before=a.Current;
            Assert.That(ads.TrySpendHint(a,hint),Is.True);Assert.That(a.Current,Is.SameAs(before));Assert.That(ads.Ledger.HintTokens,Is.EqualTo(2));
            Assert.That(ads.TrySpendHint(a,hint),Is.False);a.Execute(hint.Result.Command.Value);
            Assert.That(ads.TrySpendHint(a,Hint(a)),Is.True);Assert.That(ads.Ledger.HintTokens,Is.EqualTo(1));
        }
        [Test]public void PaidHintReloadWithEmptyWalletReopensOnlyItsGrantedState(){
            var a=Board();var free=Hint(a);a.TryRevealFreeHint(free,()=>true);a.Execute(free.Result.Command.Value);
            var ads=new AdService(new MonetizationLedger{HintTokens=1},_=>{});Assert.That(ads.TrySpendHint(a,Hint(a)),Is.True);
            var loaded=AttemptService.Restore(a.Level,AttemptRecord.Capture(a));Assert.That(ads.Ledger.HintTokens,Is.Zero);
            Assert.That(ads.TrySpendHint(loaded,Hint(loaded)),Is.True);Assert.That(ads.Ledger.HintTokens,Is.Zero);
            loaded.Execute(Hint(loaded).Result.Command.Value);Assert.That(ads.TrySpendHint(loaded,Hint(loaded)),Is.False);
        }
        [Test]public void FailedBonusSaveLeavesInventoryAndAllowanceIntact(){
            var a=Board();var ads=new AdService(new MonetizationLedger{HintTokens=1},_=>throw new IOException("fixture"));
            Assert.That(ads.TrySpendHint(a,Hint(a)),Is.False);Assert.That(a.BonusHintStateHash,Is.Null);Assert.That(ads.Ledger.HintTokens,Is.EqualTo(1));Assert.That(a.HintUsed,Is.False);
        }
        [Test]public void FirstTwelveParcelsFavorCoinsAndHoldBackCosmeticDrops(){
            var rewards=new JourneyRewards();var nodes=Enumerable.Range(1,36).Select(i=>"C"+i).ToArray();rewards.Synchronize(nodes);rewards.Parcels=rewards.Parcels.Select(p=>JourneyRewards.VarietyParcel(p.Order)).ToList();
            Assert.That(rewards.Parcels.Count,Is.EqualTo(12));Assert.That(rewards.Parcels.Count(p=>p.Kind=="coins"),Is.EqualTo(10));
            Assert.That(rewards.Parcels.Count(p=>p.Kind=="tool"),Is.EqualTo(2));Assert.That(rewards.Parcels.Count(p=>p.Kind=="cosmetic"),Is.Zero);
            var old=JsonConvert.SerializeObject(rewards.Parcels);rewards.Synchronize(nodes.Reverse());Assert.That(JsonConvert.SerializeObject(rewards.Parcels),Is.EqualTo(old));rewards.Validate();
            var shop=new AdService(new MonetizationLedger(),_=>{});foreach(var id in ShopCatalog.ParcelTrophyIds)Assert.That(shop.TryBuyKeepsake(id,99999,rewards),Is.False);
        }
        [Test]public void ExclusiveAppearanceCannotBeForgedAsShopOrMockOwnership(){
            var ledger=new MonetizationLedger();ledger.Cosmetics.Add("paper_crane");Assert.Throws<ArgumentException>(()=>ledger.Validate());
            var ads=new AdService(new MonetizationLedger(),_=>{});Assert.That(ads.VerifiedMockPurchase("mock.keepsake.paper_crane","fake",true),Is.False);
            var preview=new AdService(new MonetizationLedger(),_=>{},previewOnly:true);Assert.That(preview.TryPreviewItem(TravelTool.Wayfinder,"paper_crane"),Is.False);
        }
        [Test]public void FullCampaignGiftsAreBoundedAndClaimsAreAtomic(){
            var rewards=new JourneyRewards();rewards.Synchronize(Enumerable.Range(1,540).Select(i=>"C"+i));rewards.Parcels=rewards.Parcels.Select(p=>JourneyRewards.VarietyParcel(p.Order)).ToList();
            bool fail=true;var shop=new AdService(new MonetizationLedger(),_=>{if(fail)throw new IOException("fixture");});
            Assert.That(shop.TryClaimParcel(1,rewards),Is.False);Assert.That(rewards.Owns("petal_drift"),Is.False);
            fail=false;foreach(var parcel in rewards.Parcels){Assert.That(shop.TryClaimParcel(parcel.Order,rewards),Is.True);Assert.That(JourneyRewards.ParcelCoins(parcel),Is.InRange(0,20));}
            Assert.That(shop.TryClaimParcel(1,rewards),Is.False);Assert.That(rewards.Owns("petal_drift"),Is.True);rewards.Validate();rewards.ValidateToolClaims(shop.Ledger);
            Assert.That(rewards.Parcels.Sum(JourneyRewards.ParcelCoins),Is.LessThan(900));
        }
        [Test]public void LensWorksAtFinalDeliveryAndConsumesOnlyOneItem(){
            var a=Board();var before=a.Current;var predicted=a.Predict(PuzzleCommand.Drop);var shop=new AdService(new MonetizationLedger{LandingLenses=2},_=>{});
            Assert.That(predicted.Accepted,Is.True);Assert.That(shop.TryUseTool(TravelTool.LandingLens,a),Is.True);Assert.That(a.Current,Is.SameAs(before));
            Assert.That(shop.ToolStock(TravelTool.LandingLens),Is.EqualTo(1));Assert.That(shop.TryUseTool(TravelTool.LandingLens,a),Is.True);Assert.That(shop.ToolStock(TravelTool.LandingLens),Is.Zero);
        }
        [Test]public void LanternCanPerformSolverRecommendedSafeDrop(){
            var a=Board();var same=new LevelDefinition("ready","C09","2.0","2.0","fixture",a.Level.TargetSymbolIds,a.Level.TargetSymbolIds,a.Level.TargetSegments,a.Level.Floors);
            a=new AttemptService(same);var shop=new AdService(new MonetizationLedger{SignalLanterns=1},_=>{});
            Assert.That(Hint(a).Result.Command.Value.Kind,Is.EqualTo(CommandKind.EarlyDrop));Assert.That(shop.TryUseTool(TravelTool.SignalLantern,a,Hint(a)),Is.True);Assert.That(a.Current.Status,Is.EqualTo(PuzzleStatus.Won));
        }
    }
}
