using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Solver;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class ShopTests
    {
        static AttemptService Board(int wood=12)=>new AttemptService(LevelDefinitionTests.Level(LevelDefinitionTests.Row("DCBA"),LevelDefinitionTests.Row("ABCD"),
            new[]{new FloorDefinition(wood,new[]{true,true,true}),new FloorDefinition(12,new[]{true,true,true})}));
        [Test] public void TwoFreeUndosDoNotRefillAfterCommandsOrReload()
        {
            var a=Board();for(int i=0;i<2;i++){a.Execute(PuzzleCommand.Swap(0));Assert.That(a.Undo(),Is.True);}
            a.Execute(PuzzleCommand.Swap(0));string hash=a.CurrentStateHash;Assert.That(a.Undo(),Is.False);Assert.That(a.CurrentStateHash,Is.EqualTo(hash));
            var restored=AttemptService.Restore(a.Level,AttemptRecord.Capture(a));Assert.That(restored.FreeUndoRemaining,Is.Zero);Assert.That(restored.CanUndo,Is.False);
            restored.Retry();Assert.That(restored.FreeUndoRemaining,Is.EqualTo(2));
        }
        [Test] public void UndoAfterCollapseAndHourglassRestoreExactCheckpoint()
        {
            var a=Board(1);var before=a.Current;a.Execute(PuzzleCommand.Swap(0));a.Execute(PuzzleCommand.Swap(1));
            Assert.That(a.Current.ActiveFloor,Is.EqualTo(1));Assert.That(a.Undo(),Is.True);Assert.That(a.CanUndo,Is.False);
            Assert.That(a.TryUseToolAtomically(TravelTool.UndoSpool,()=>true),Is.False);
            Assert.That(a.TryUseToolAtomically(TravelTool.ReturnHourglass,()=>true),Is.True);Assert.That(a.Current,Is.SameAs(before));
            Assert.That(a.History,Is.Empty);Assert.That(a.HourglassUsed&&a.HintUsed,Is.True);
            a.Execute(PuzzleCommand.Swap(0));Assert.That(a.TryUseToolAtomically(TravelTool.ReturnHourglass,()=>true),Is.False);
            Assert.That(AttemptService.Restore(a.Level,AttemptRecord.Capture(a)).HourglassUsed,Is.True);
        }
        [Test] public void PaidSpoolIsOneExtraLocalUndoAndCannotBeReused()
        {
            var a=Board();for(int i=0;i<2;i++){a.Execute(PuzzleCommand.Swap(0));a.Undo();}a.Execute(PuzzleCommand.Swap(0));
            var service=new AdService(new MonetizationLedger{UndoSpools=2},_=>{});
            Assert.That(service.TryUseTool(TravelTool.UndoSpool,a),Is.True);Assert.That(a.RouteLength,Is.Zero);Assert.That(a.UndoCount,Is.EqualTo(3));
            Assert.That(service.Ledger.UndoSpools,Is.EqualTo(1));a.Execute(PuzzleCommand.Swap(0));Assert.That(service.TryUseTool(TravelTool.UndoSpool,a),Is.False);
            Assert.That(service.Ledger.UndoSpools,Is.EqualTo(1));Assert.That(a.FreeUndoRemaining,Is.Zero);
        }
        [Test] public void PurchaseCapsWalletAndPermanentOwnershipAreEnforced()
        {
            var s=new AdService(new MonetizationLedger(),_=>{});var rewards=new JourneyRewards();
            Assert.That(s.TryBuyTool(TravelTool.ReturnHourglass,299),Is.False);Assert.That(s.Ledger.SpentCoins,Is.Zero);
            for(int i=0;i<5;i++)Assert.That(s.TryBuyTool(TravelTool.UndoSpool,1000),Is.True);
            Assert.That(s.TryBuyTool(TravelTool.UndoSpool,1000),Is.False);Assert.That(s.CoinBalance(1000),Is.EqualTo(400));
            Assert.That(s.TryBuyKeepsake("leaf",1000,rewards),Is.True);Assert.That(s.TryBuyKeepsake("leaf",1000,rewards),Is.False);
            Assert.That(rewards.Equip("leaf",s.Ledger.Cosmetics),Is.True);rewards.Validate(s.Ledger.Cosmetics);
            Assert.That(s.CoinBalance(1000),Is.EqualTo(40));Assert.That(s.TryBuyTool((TravelTool)99,1000),Is.False);
        }
        [Test] public void SaveFailureRollsBackCoinsStockHistoryAndToolFlags()
        {
            var a=Board(1);a.Execute(PuzzleCommand.Swap(0));string hash=a.CurrentStateHash;
            var s=new AdService(new MonetizationLedger{ReturnHourglasses=1},_=>throw new IOException("disk full"));
            Assert.That(s.TryBuyTool(TravelTool.UndoSpool,1000),Is.False);Assert.That(s.Ledger.SpentCoins,Is.Zero);Assert.That(s.Ledger.UndoSpools,Is.Zero);
            Assert.That(s.TryUseTool(TravelTool.ReturnHourglass,a),Is.False);Assert.That(s.Ledger.ReturnHourglasses,Is.EqualTo(1));
            Assert.That(a.CurrentStateHash,Is.EqualTo(hash));Assert.That(a.HourglassUsed||a.HintUsed,Is.False);Assert.That(a.History.Count,Is.EqualTo(1));
        }
        [Test] public void GuideRequiresCurrentProofExpiresAtCollapseAndPersistsUsage()
        {
            var a=Board();var s=new AdService(new MonetizationLedger{WayfinderScrolls=2},_=>{});var stale=AttemptService.CalculateHint(a.CaptureHintRequest());
            a.Execute(PuzzleCommand.Swap(0));Assert.That(s.TryUseTool(TravelTool.Wayfinder,a,stale),Is.False);Assert.That(s.Ledger.WayfinderScrolls,Is.EqualTo(2));
            var proof=AttemptService.CalculateHint(a.CaptureHintRequest());Assert.That(s.TryUseTool(TravelTool.Wayfinder,a,proof),Is.True);Assert.That(a.GuideActive&&a.HintUsed,Is.True);
            var loaded=AttemptService.Restore(a.Level,AttemptRecord.Capture(a));Assert.That(loaded.GuideActive,Is.True);
            a.Execute(PuzzleCommand.Drop);Assert.That(a.GuideActive,Is.False);Assert.That(a.GuideFloor,Is.EqualTo(-1));
            Assert.That(AttemptService.Restore(a.Level,AttemptRecord.Capture(a)).WayfinderUsed,Is.True);
            Assert.That(s.TryUseTool(TravelTool.Wayfinder,a,AttemptService.CalculateHint(a.CaptureHintRequest())),Is.False);
        }
        [Test] public void AssistanceCannotEarnThreeStarsOrRewindAnAwardedDelivery()
        {
            var a=Board();var guide=AttemptService.CalculateHint(a.CaptureHintRequest());a.TryUseToolAtomically(TravelTool.Wayfinder,()=>true,guide);
            var route=PuzzleSolver.FindShortest(a.Resolver,a.Current).Path;foreach(var c in route)a.Execute(c);
            Assert.That(a.Current.Status,Is.EqualTo(PuzzleStatus.Won));Assert.That(LevelCompletion.From(a,route.Count).Stars,Is.LessThan(3));
            Assert.That(a.Undo(),Is.False);Assert.That(a.TryUseToolAtomically(TravelTool.ReturnHourglass,()=>true),Is.False);
            var rewards=new JourneyRewards();rewards.RecordDelivery(a);Assert.That(rewards.Records.Single().BestClean,Is.Null);
        }
        [Test] public void AnAlreadyReadyRowDoesNotWasteAScroll()
        {
            var a=new AttemptService(LevelDefinitionTests.Level(LevelDefinitionTests.Row("ABCD"),LevelDefinitionTests.Row("ABCD"),new[]{new FloorDefinition(3,new[]{true,true,true})}));
            var s=new AdService(new MonetizationLedger{WayfinderScrolls=1},_=>{});
            Assert.That(s.TryUseTool(TravelTool.Wayfinder,a,AttemptService.CalculateHint(a.CaptureHintRequest())),Is.False);
            Assert.That(s.Ledger.WayfinderScrolls,Is.EqualTo(1));Assert.That(a.HintUsed||a.WayfinderUsed,Is.False);
        }
        [Test] public void DurableTransactionReloadKeepsPurchasedStockAndHourglassExpense()
        {
            string dir=Path.Combine(Path.GetTempPath(),"CascadeShopTests",Guid.NewGuid().ToString("N"));
            try {
                var a=Board(1);var profile=new PlayerProfile{Monetization=new MonetizationLedger{BonusCoins=600}};var store=new ProfileStore(Path.Combine(dir,"save.json"));
                var s=MonetizationTransaction.Create(profile,store,()=>a);Assert.That(s.TryBuyTool(TravelTool.ReturnHourglass,0),Is.True);
                a.Execute(PuzzleCommand.Swap(0));Assert.That(s.TryUseTool(TravelTool.ReturnHourglass,a),Is.True);
                var saved=store.Load().Profile;Assert.That(saved.Monetization.ReturnHourglasses,Is.Zero);Assert.That(saved.Monetization.SpentCoins,Is.EqualTo(300));
                var restored=AttemptService.Restore(a.Level,saved.Attempt);Assert.That(restored.CurrentStateHash,Is.EqualTo(a.CurrentStateHash));Assert.That(restored.HourglassUsed,Is.True);
            } finally {if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
        [Test] public void LanternCommitsExactlyOneProvedMoveWithStockAndRestoresProof()
        {
            var a=Board();var before=a.Current;var hint=AttemptService.CalculateHint(a.CaptureHintRequest());
            var expected=a.Resolver.Resolve(before,hint.Result.Command.Value);
            var service=new AdService(new MonetizationLedger{SignalLanterns=2},_=>{});
            Assert.That(service.TryUseTool(TravelTool.SignalLantern,a,hint),Is.True);
            Assert.That(a.Current,Is.EqualTo(expected.State));Assert.That(a.RouteLength,Is.EqualTo(1));
            Assert.That(service.ToolStock(TravelTool.SignalLantern),Is.EqualTo(1));Assert.That(a.LanternUsed&&a.HintUsed,Is.True);
            Assert.That(a.LastToolTransition.State,Is.SameAs(a.Current));
            Assert.That(AttemptService.Restore(a.Level,AttemptRecord.Capture(a)).LanternUsed,Is.True);
            Assert.That(service.TryUseTool(TravelTool.SignalLantern,a,AttemptService.CalculateHint(a.CaptureHintRequest())),Is.False);
            Assert.That(service.ToolStock(TravelTool.SignalLantern),Is.EqualTo(1));
        }
        [Test] public void NewToolsRollbackOnFailedPersistenceAndLensNeverChangesRow()
        {
            foreach(var tool in new[]{TravelTool.SignalLantern,TravelTool.LandingLens}){
                var a=Board();string start=a.CurrentStateHash;var proof=AttemptService.CalculateHint(a.CaptureHintRequest());
                var failing=new AdService(new MonetizationLedger{SignalLanterns=1,LandingLenses=1},_=>throw new IOException("full"));
                Assert.That(failing.TryUseTool(tool,a,proof),Is.False);Assert.That(a.CurrentStateHash,Is.EqualTo(start));
                Assert.That(a.History,Is.Empty);Assert.That(a.PurchasedHelpUsed,Is.False);Assert.That(failing.ToolStock(tool),Is.EqualTo(1));
            }
            var lens=new AdService(new MonetizationLedger{LandingLenses=2},_=>{});var board=Board();string hash=board.CurrentStateHash;
            var predicted=board.Predict(PuzzleCommand.Drop);Assert.That(lens.TryUseTool(TravelTool.LandingLens,board),Is.True);
            Assert.That(board.CurrentStateHash,Is.EqualTo(hash));Assert.That(board.LensUsed&&board.HintUsed,Is.True);
            Assert.That(AttemptService.Restore(board.Level,AttemptRecord.Capture(board)).LensUsed,Is.True);
            Assert.That(lens.TryUseTool(TravelTool.LandingLens,board),Is.False);board.Execute(PuzzleCommand.Drop);
            Assert.That(board.Current,Is.EqualTo(predicted.State));
        }
        [Test] public void ExpandedTrophiesEquipWithoutChangingParcelReceiptsOrPuzzleStats()
        {
            var rewards=new JourneyRewards();var shop=new AdService(new MonetizationLedger{BonusCoins=5000},_=>{});
            foreach(string trophy in ShopCatalog.TrophyIds.Skip(4).Take(4)){
                Assert.That(shop.TryBuyKeepsake(trophy,5000,rewards),Is.True);Assert.That(shop.TryBuyKeepsake(trophy,5000,rewards),Is.False);
                Assert.That(rewards.Equip(trophy,shop.Ledger.Cosmetics),Is.True);rewards.Validate(shop.Ledger.Cosmetics);shop.Ledger.Validate();
            }
            rewards.Synchronize(Enumerable.Range(1,420).Select(i=>"C"+i));
            foreach(string trophy in ShopCatalog.BonusTrophyIds){
                Assert.That(shop.TryBuyKeepsake(trophy,5000,rewards),Is.True);Assert.That(shop.TryBuyKeepsake(trophy,5000,rewards),Is.False);
                Assert.That(rewards.Equip(trophy,shop.Ledger.Cosmetics),Is.True);
            }
            Assert.That(JourneyRewards.Gifts.Length,Is.EqualTo(4));
            Assert.That(ShopCatalog.MikaTrophyIds.Length,Is.EqualTo(30));
            var old=Newtonsoft.Json.JsonConvert.DeserializeObject<MonetizationLedger>("{}");old.Validate();
            Assert.That(old.SignalLanterns+old.LandingLenses,Is.Zero);
        }
        [Test] public void PremiumLooksNeedOnlyVirtualCoinsAndNeverSpendStars()
        {
            var rewards=new JourneyRewards();var shop=new AdService(new MonetizationLedger{BonusCoins=5000},_=>{});
            Assert.That(shop.TryBuyKeepsake("sky_pilot",0,rewards),Is.True);
            Assert.That(shop.Ledger.SpentCoins,Is.EqualTo(1440));
            Assert.That(shop.TryBuyKeepsake("moth_mask",0,rewards),Is.True);
            Assert.That(rewards.CosmeticClearCount,Is.Zero);Assert.That(rewards.Parcels,Is.Empty);
        }
        [TestCase("wind_chime",15)]
        [TestCase("rain_badge",30)]
        [TestCase("copper_acorn",45)]
        [TestCase("beacon_brooch",60)]
        public void TrophyMilestonesRequireTheirExactUniqueClearThreshold(string id,int threshold)
        {
            var rewards=new JourneyRewards();
            rewards.Synchronize(Enumerable.Range(1,threshold-1).Select(i=>"C"+i));
            Assert.That(ShopCatalog.EarnedClearMilestone(id),Is.EqualTo(threshold));
            Assert.That(rewards.Owns(id),Is.False);Assert.That(rewards.Equip(id),Is.False);
            rewards.Synchronize(new[]{"C"+threshold});
            Assert.That(rewards.Owns(id),Is.True);Assert.That(rewards.Equip(id),Is.True);rewards.Validate();
            Assert.That(JourneyRewards.Gifts.All(g=>!rewards.Owns(g)),Is.True,"Milestones must not silently claim parcel fragments.");
            Assert.That(rewards.Records,Is.Empty);Assert.That(rewards.Deliveries,Is.Empty);
        }
        [Test] public void TrophyReplayCannotFarmOwnershipAndEarnedTrophyCannotBeBoughtAgain()
        {
            var rewards=new JourneyRewards();var nodes=Enumerable.Range(1,14).Select(i=>"C"+i).ToArray();
            for(int replay=0;replay<4;replay++)rewards.Synchronize(nodes);
            Assert.That(rewards.CosmeticClearCount,Is.EqualTo(14));Assert.That(rewards.Owns("wind_chime"),Is.False);
            rewards.Synchronize(new[]{"C15","C15"});Assert.That(rewards.CosmeticClearCount,Is.EqualTo(15));
            var shop=new AdService(new MonetizationLedger{BonusCoins=500},_=>{});
            Assert.That(shop.TryBuyKeepsake("wind_chime",0,rewards),Is.False);
            Assert.That(shop.Ledger.SpentCoins,Is.Zero);Assert.That(shop.Ledger.Cosmetics,Is.Empty);
            Assert.That(rewards.Owns("unknown"),Is.False);Assert.That(ShopCatalog.KeepsakePrice,Is.EqualTo(360));
        }
        [Test] public void PurchasedTrophyStaysPermanentWhenItsMilestoneIsLaterEarned()
        {
            var rewards=new JourneyRewards();var shop=new AdService(new MonetizationLedger{BonusCoins=500},_=>{});
            Assert.That(shop.TryBuyKeepsake("rain_badge",0,rewards),Is.True);
            Assert.That(rewards.Equip("rain_badge",shop.Ledger.Cosmetics),Is.True);
            rewards.Synchronize(Enumerable.Range(1,30).Select(i=>"C"+i));
            Assert.That(rewards.Equip("rain_badge"),Is.True);Assert.That(shop.TryBuyKeepsake("rain_badge",0,rewards),Is.False);
            Assert.That(shop.Ledger.Cosmetics,Is.EquivalentTo(new[]{"rain_badge"}));Assert.That(shop.Ledger.SpentCoins,Is.EqualTo(360));
            rewards.Validate(shop.Ledger.Cosmetics);
        }
        [Test] public void EarnedCosmeticAndEquipmentSurviveModeSwitchAndDurableReload()
        {
            string dir=Path.Combine(Path.GetTempPath(),"CascadeTrophyMilestones",Guid.NewGuid().ToString("N"));
            try {
                var profile=new PlayerProfile();
                for(int i=1;i<=15;i++)profile.Clears.Add(new ClearReceipt{CampaignNodeId="C"+i,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64)});
                profile.Rewards.Synchronize(profile.AllClears.Select(c=>c.CampaignNodeId));
                Assert.That(profile.Rewards.Equip("wind_chime"),Is.True);
                profile.SelectJourney("Words");Assert.That(profile.Clears,Is.Empty);Assert.That(profile.Rewards.Owns("wind_chime"),Is.True);
                var store=new ProfileStore(Path.Combine(dir,"save.json"));store.Save(profile);
                var loaded=store.Load().Profile;loaded.Validate();
                Assert.That(loaded.Rewards.CosmeticClearCount,Is.EqualTo(15));Assert.That(loaded.Rewards.EquippedGift,Is.EqualTo("wind_chime"));
                loaded.SelectJourney("Mixed");loaded.Rewards.Synchronize(loaded.AllClears.Select(c=>c.CampaignNodeId));loaded.Validate();
                Assert.That(loaded.Rewards.Owns("wind_chime"),Is.True);Assert.That(loaded.Rewards.CosmeticClearCount,Is.EqualTo(15));
                Assert.That(loaded.Monetization.Cosmetics,Is.Empty);Assert.That(loaded.Rewards.Records,Is.Empty);
            } finally {if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
    }
}
