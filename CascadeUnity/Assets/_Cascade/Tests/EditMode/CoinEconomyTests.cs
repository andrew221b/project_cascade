using System;
using System.IO;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class CoinEconomyTests
    {
        [Test]public void FullReceiptHistoryRejectsNewGrantsWithoutThrowingAndAcknowledgesPaidDuplicates()
        {
            var ledger=new MonetizationLedger();
            ledger.Grants.Add(new GrantReceipt{Id="old_paid",Kind="store_coins",Product=PurchaseCatalog.Coins240,Amount=240});
            for(int i=1;i<MonetizationLedger.MaxGrantReceipts;i++)ledger.Grants.Add(new GrantReceipt{Id="old_ad_"+i,Kind="coins",Amount=40});
            ledger.BonusCoins=240+(MonetizationLedger.MaxGrantReceipts-1)*40;
            int writes=0;var service=new AdService(ledger,_=>writes++);
            Assert.That(service.CanReceiveGrant,Is.False);
            Assert.That(service.VerifiedStorePurchase(PurchaseCatalog.Coins240,"old_paid",true),Is.True);
            Assert.That(service.VerifiedStorePurchase(PurchaseCatalog.NoAutomaticAds,"new_paid",true),Is.False);
            Assert.That(service.TryRewarded(new AdContext{Surface=AdSurface.Inventory,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true},"coins",true),Is.Null);
            Assert.That(writes,Is.Zero);Assert.That(service.Ledger.Grants.Count,Is.EqualTo(MonetizationLedger.MaxGrantReceipts));
        }
        static AttemptService Board()=>new AttemptService(LevelDefinitionTests.Level(LevelDefinitionTests.Row("DCBA"),LevelDefinitionTests.Row("ABCD"),new[]{new FloorDefinition(12,new[]{true,true,true}),new FloorDefinition(12,new[]{true,true,true})}));
        [Test] public void VerifiedPacksAreExactlyOnceAndCannotChangeProductOnReplay()
        {
            int saves=0;var s=new AdService(new MonetizationLedger(),_=>saves++);
            Assert.That(s.VerifiedStorePurchase(PurchaseCatalog.Coins240,"receipt",false),Is.False);
            Assert.That(s.VerifiedStorePurchase("fake","receipt",true),Is.False);
            Assert.That(s.VerifiedStorePurchase(PurchaseCatalog.Coins240,"receipt",true),Is.True);
            Assert.That(s.VerifiedStorePurchase(PurchaseCatalog.Coins240,"receipt",true),Is.True);
            Assert.That(s.VerifiedStorePurchase(PurchaseCatalog.Coins480,"receipt",true),Is.False);
            Assert.That(s.VerifiedStorePurchase(PurchaseCatalog.NoAutomaticAds,"receipt",true),Is.False);
            Assert.That(s.Ledger.BonusCoins,Is.EqualTo(240));Assert.That(saves,Is.EqualTo(1));s.Ledger.Validate();
            Assert.That(s.VerifiedStorePurchase(PurchaseCatalog.Coins480,"another",true),Is.True);
            Assert.That(s.Ledger.BonusCoins,Is.EqualTo(720));Assert.That(s.Ledger.NoAutomaticAds,Is.False);
        }
        [Test] public void PaidPendingValueIsDeliveredAboveWalletThresholdAndSaveFailureCanRetry()
        {
            bool fail=true;var s=new AdService(new MonetizationLedger{BonusCoins=1600},_=>{if(fail)throw new IOException("disk");});
            Assert.That(s.VerifiedStorePurchase(PurchaseCatalog.Coins480,"paid",true),Is.False);
            Assert.That(s.Ledger.BonusCoins,Is.EqualTo(1600));Assert.That(s.Ledger.Grants,Is.Empty);
            fail=false;Assert.That(s.VerifiedStorePurchase(PurchaseCatalog.Coins480,"paid",true),Is.True);
            Assert.That(s.Ledger.BonusCoins,Is.EqualTo(2080));
        }
        [Test] public void ThirdToolCanBeUsedAfterReload()
        {
            var board=Board();var service=new AdService(new MonetizationLedger{LandingLenses=1,WayfinderScrolls=1,SignalLanterns=1},_=>{});
            Assert.That(service.TryUseTool(TravelTool.LandingLens,board),Is.True);
            Assert.That(service.TryUseTool(TravelTool.Wayfinder,board,AttemptService.CalculateHint(board.CaptureHintRequest())),Is.True);
            var restored=AttemptService.Restore(board.Level,AttemptRecord.Capture(board));string before=restored.CurrentStateHash;
            Assert.That(restored.ToolUsesRemaining,Is.GreaterThan(0));
            Assert.That(service.TryUseTool(TravelTool.SignalLantern,restored,AttemptService.CalculateHint(restored.CaptureHintRequest())),Is.True);
            Assert.That(service.Ledger.SignalLanterns,Is.Zero);Assert.That(restored.CurrentStateHash,Is.Not.EqualTo(before));
            restored.Retry();Assert.That(restored.ToolUsesRemaining,Is.GreaterThan(0));
        }
        [Test] public void OneStepGuideExpiresOnSwapAndFailedToolSavePreservesItsState()
        {
            var board=Board();var hint=AttemptService.CalculateHint(board.CaptureHintRequest());
            Assert.That(board.TryUseToolAtomically(TravelTool.Wayfinder,()=>true,hint),Is.True);
            Assert.That(board.TryUseToolAtomically(TravelTool.SignalLantern,()=>false,hint),Is.False);
            Assert.That(board.GuideActive,Is.True);Assert.That(board.ToolsUsed,Is.EqualTo(1));
            board.Execute(hint.Result.Command.Value);Assert.That(board.GuideActive,Is.False);
            Assert.That(AttemptService.Restore(board.Level,AttemptRecord.Capture(board)).GuideFloor,Is.EqualTo(-1));
        }
        [Test] public void UndoExpiresGuideAndLegacyToolFlagsRemainLoadable()
        {
            var board=Board();board.Execute(PuzzleCommand.Swap(0));
            Assert.That(board.TryUseToolAtomically(TravelTool.Wayfinder,()=>true,AttemptService.CalculateHint(board.CaptureHintRequest())),Is.True);
            Assert.That(board.Undo(),Is.True);Assert.That(board.GuideActive,Is.False);
            var record=AttemptRecord.Capture(board);record.LensUsed=true;record.SpoolUsed=true;
            var restored=AttemptService.Restore(board.Level,record);Assert.That(restored.ToolsUsed,Is.EqualTo(3));Assert.That(restored.ToolUsesRemaining,Is.GreaterThan(0));
        }
        [Test] public void PreviewCoinPackNeverWritesFakeValueIntoProfile()
        {
            string dir=Path.Combine(Path.GetTempPath(),"CascadeCoinPreview",Guid.NewGuid().ToString("N"));
            try{
                var profile=new PlayerProfile();var store=new ProfileStore(Path.Combine(dir,"profile.json"));var board=Board();
                var s=MonetizationTransaction.CreatePreview(profile,store,()=>board);
                Assert.That(s.TryPreviewCoinPack(PurchaseCatalog.Coins240),Is.True);Assert.That(s.Ledger.BonusCoins,Is.EqualTo(240));
                Assert.That(s.TryPreviewCoinPack("fake"),Is.False);Assert.That(profile.Monetization.BonusCoins,Is.Zero);
                Assert.That(store.Load().Profile.Monetization.BonusCoins,Is.Zero);Assert.That(store.Load().Profile.Monetization.Grants,Is.Empty);
            }finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
    }
}
