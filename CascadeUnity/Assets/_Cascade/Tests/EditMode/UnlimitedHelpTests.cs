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
    public sealed class UnlimitedHelpTests
    {
        static AttemptService Board()=>new AttemptService(LevelDefinitionTests.Level(LevelDefinitionTests.Row("DCBA"),LevelDefinitionTests.Row("ABCD"),new[]{new FloorDefinition(8,new[]{true,true,true}),new FloorDefinition(8,new[]{true,true,true})}));
        [Test]public void RepeatedRepairsAtSameAndLaterRoutePositionsReplayExactly(){var a=Board();var shop=new AdService(new MonetizationLedger{RepairKits=4},_=>{});Assert.That(shop.TryUseTool(TravelTool.RepairKit,a),Is.True);Assert.That(shop.TryUseTool(TravelTool.RepairKit,a),Is.True);a.Execute(PuzzleCommand.Swap(0));Assert.That(shop.TryUseTool(TravelTool.RepairKit,a),Is.True);a.Execute(PuzzleCommand.Swap(1));var record=JsonConvert.DeserializeObject<AttemptRecord>(JsonConvert.SerializeObject(AttemptRecord.Capture(a)));Assert.That(record.RepairRoutes,Is.EqualTo(new[]{0,0,1}));var restored=AttemptService.Restore(a.Level,record);Assert.That(restored.CurrentStateHash,Is.EqualTo(a.CurrentStateHash));Assert.That(restored.Undo(),Is.True);Assert.That(restored.Current.Durability,Is.EqualTo(13));Assert.That(restored.TryUseToolAtomically(TravelTool.UndoSpool,()=>true),Is.True);Assert.That(restored.Current.Durability,Is.EqualTo(12));Assert.That(restored.RepairRoutes,Is.EqualTo(new[]{0,0}));Assert.That(AttemptService.Restore(a.Level,AttemptRecord.Capture(restored)).CurrentStateHash,Is.EqualTo(restored.CurrentStateHash));Assert.That(shop.Ledger.RepairKits,Is.EqualTo(1));}
        [Test]public void FailedRepeatedRepairSaveRestoresStockCheckpointAndPuzzle(){var a=Board();bool fail=false;var shop=new AdService(new MonetizationLedger{RepairKits=3},_=>{if(fail)throw new InvalidOperationException("fixture");});Assert.That(shop.TryUseTool(TravelTool.RepairKit,a),Is.True);string hash=a.CurrentStateHash;fail=true;Assert.That(shop.TryUseTool(TravelTool.RepairKit,a),Is.False);Assert.That(a.CurrentStateHash,Is.EqualTo(hash));Assert.That(a.RepairRoutes,Is.EqualTo(new[]{0}));Assert.That(shop.Ledger.RepairKits,Is.EqualTo(2));}
        [Test]public void ThreeUsesOfOneTypeEachConsumeOneOwnedItem(){var a=Board();var shop=new AdService(new MonetizationLedger{LandingLenses=4},_=>{});string hash=a.CurrentStateHash;for(int i=0;i<3;i++)Assert.That(shop.TryUseTool(TravelTool.LandingLens,a),Is.True);Assert.That(shop.ToolStock(TravelTool.LandingLens),Is.EqualTo(1));Assert.That(a.CurrentStateHash,Is.EqualTo(hash));Assert.That(a.RouteLength,Is.Zero);Assert.That(AttemptService.Restore(a.Level,AttemptRecord.Capture(a)).CanUseLens,Is.True);}
        [Test]public void LegacyTwoSpentFreeUndosLoadWithNewAllowanceExhausted(){var a=Board();a.Execute(PuzzleCommand.Swap(0));var record=AttemptRecord.Capture(a);record.FreeUndoUsed=2;record.UndoCount=2;record.MovesMade=3;var loaded=AttemptService.Restore(a.Level,record);Assert.That(loaded.FreeUndoRemaining,Is.Zero);Assert.That(loaded.Undo(),Is.False);Assert.That(loaded.TryUseToolAtomically(TravelTool.UndoSpool,()=>true),Is.True);Assert.That(loaded.RouteLength,Is.Zero);}
        [Test]public void HintPolicyAlternatesWithoutStageExhaustionAndCachedStatesNeverNeedVideo(){for(int i=0;i<100;i++){Assert.That(StageHintBudget.RewardedHintDue(i,0,false),Is.EqualTo(i%2==1));Assert.That(StageHintBudget.RewardedHintDue(i,0,true),Is.False);}}
        [Test]public void EveryToolCanBeBoughtBeyondFiveSavedAndUsedRepeatedly()
        {
            string directory=Path.Combine(Path.GetTempPath(),"CascadeUnlimitedStock",Guid.NewGuid().ToString("N"));
            try{
                var board=Board();string hash=board.CurrentStateHash;var profile=new PlayerProfile{Monetization=new MonetizationLedger{BonusCoins=100000}};
                var store=new ProfileStore(Path.Combine(directory,"profile.json"));var shop=MonetizationTransaction.Create(profile,store,()=>board);
                foreach(TravelTool tool in Enum.GetValues(typeof(TravelTool)))for(int i=0;i<12;i++)Assert.That(shop.TryBuyTool(tool,0),Is.True,tool.ToString());
                var saved=store.Load().Profile;saved.Validate();var loaded=new AdService(saved.Monetization,_=>{});
                foreach(TravelTool tool in Enum.GetValues(typeof(TravelTool)))Assert.That(loaded.ToolStock(tool),Is.EqualTo(12));
                for(int i=0;i<7;i++)Assert.That(shop.TryUseTool(TravelTool.LandingLens,board),Is.True);
                saved=store.Load().Profile;Assert.That(saved.Monetization.LandingLenses,Is.EqualTo(5));
                Assert.That(AttemptService.Restore(board.Level,saved.Attempt).CurrentStateHash,Is.EqualTo(hash));
            }finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test]public void RepresentationOverflowCannotWrapStockOrSpendAndWalletStaysNonNegative()
        {
            var shop=new AdService(new MonetizationLedger{UndoSpools=int.MaxValue,BonusCoins=int.MaxValue},_=>{});
            Assert.That(shop.CoinBalance(int.MaxValue),Is.EqualTo(int.MaxValue));Assert.That(shop.TryBuyTool(TravelTool.UndoSpool,int.MaxValue),Is.False);
            Assert.That(shop.ToolStock(TravelTool.UndoSpool),Is.EqualTo(int.MaxValue));Assert.That(shop.Ledger.SpentCoins,Is.Zero);
            shop=new AdService(new MonetizationLedger{SpentCoins=int.MaxValue,BonusCoins=int.MaxValue},_=>{});
            Assert.That(shop.TryBuyTool(TravelTool.UndoSpool,int.MaxValue),Is.False);Assert.That(shop.Ledger.UndoSpools,Is.Zero);
        }
    }
}
