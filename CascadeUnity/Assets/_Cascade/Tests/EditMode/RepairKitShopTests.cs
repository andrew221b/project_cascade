using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class RepairKitShopTests
    {
        static AttemptService Board(int hp=4,int floors=2,bool bomb=false)
        {
            var row=LevelDefinitionTests.Row("CBA");
            return new AttemptService(new LevelDefinition("kit","C02","2.0","2.0","kit-fixture",row,row.Reverse(),
                new[]{new TargetSegment(SegmentKind.Shapes,0,3)},Enumerable.Range(0,floors).Select(i=>
                    new FloorDefinition(hp,new[]{true,true},bomb:bomb?new BombDefinition(0,row[2],3,2):null))));
        }
        [Test]public void KitCosts210AndAddsOnlyTwoDurability()
        {
            var a=Board(bomb:true);var before=a.Current;var shop=new AdService(new MonetizationLedger(),_=>{});
            Assert.That(shop.TryBuyTool(TravelTool.RepairKit,209),Is.False);
            Assert.That(shop.TryBuyTool(TravelTool.RepairKit,210),Is.True);
            Assert.That(shop.TryUseTool(TravelTool.RepairKit,a),Is.True);
            Assert.That(a.Current.Durability,Is.EqualTo(before.Durability+2));Assert.That(a.Current.Row,Is.EqualTo(before.Row));
            Assert.That(a.Current.BombTicks,Is.EqualTo(before.BombTicks));Assert.That(a.Current.GustTicks,Is.EqualTo(before.GustTicks));
            Assert.That(a.MovesMade+a.RouteLength,Is.Zero);Assert.That(a.RepairKitUsed&&a.PurchasedHelpUsed,Is.True);
            Assert.That(shop.ToolStock(TravelTool.RepairKit),Is.Zero);Assert.That(shop.CoinBalance(210),Is.Zero);
        }
        [Test]public void RepairReplayBeforeAndAfterMovesIsExact()
        {
            foreach(int boundary in new[]{0,1,2}){
                var a=Board(8);for(int i=0;i<boundary;i++)a.Execute(PuzzleCommand.Swap(0));
                Assert.That(a.TryUseToolAtomically(TravelTool.RepairKit,()=>true),Is.True);
                a.Execute(PuzzleCommand.Swap(1));var restored=AttemptService.Restore(a.Level,AttemptRecord.Capture(a));
                Assert.That(restored.CurrentStateHash,Is.EqualTo(a.CurrentStateHash));Assert.That(restored.RepairAtRoute,Is.EqualTo(boundary));
                Assert.That(restored.Undo(),Is.True);Assert.That(restored.Current.Durability,Is.EqualTo(10-boundary));
                Assert.That(AttemptService.Restore(a.Level,AttemptRecord.Capture(restored)).CurrentStateHash,Is.EqualTo(restored.CurrentStateHash));
            }
        }
        [Test]public void RewindingAcrossRepairLosesEffectWithoutRefundingAllowance()
        {
            var a=Board(8);a.Execute(PuzzleCommand.Swap(0));a.TryUseToolAtomically(TravelTool.RepairKit,()=>true);
            Assert.That(a.Undo(),Is.True);Assert.That(a.Current.Durability,Is.EqualTo(8));Assert.That(a.RepairAtRoute,Is.Null);
            Assert.That(a.RepairKitUsed,Is.True);Assert.That(a.TryUseToolAtomically(TravelTool.RepairKit,()=>true),Is.True);
            Assert.That(AttemptService.Restore(a.Level,AttemptRecord.Capture(a)).CurrentStateHash,Is.EqualTo(a.CurrentStateHash));
        }
        [Test]public void ReturningAcrossRepairedLowerBoardPreservesReplayProof()
        {
            var a=Board();a.Execute(PuzzleCommand.Drop);a.TryUseToolAtomically(TravelTool.RepairKit,()=>true);
            Assert.That(a.TryUseToolAtomically(TravelTool.ReturnHourglass,()=>true),Is.True);
            Assert.That(a.RepairAtRoute,Is.Null);Assert.That(a.ToolsUsed,Is.EqualTo(2));
            Assert.That(AttemptService.Restore(a.Level,AttemptRecord.Capture(a)).CurrentStateHash,Is.EqualTo(a.CurrentStateHash));
        }
        [Test]public void FailedSaveRestoresKitStockStateAndCheckpoint()
        {
            var a=Board();var before=a.CurrentStateHash;
            var shop=new AdService(new MonetizationLedger{RepairKits=1},_=>throw new IOException("full"));
            Assert.That(shop.TryUseTool(TravelTool.RepairKit,a),Is.False);Assert.That(shop.Ledger.RepairKits,Is.EqualTo(1));
            Assert.That(a.CurrentStateHash,Is.EqualTo(before));Assert.That(a.RepairKitUsed||a.HintUsed,Is.False);Assert.That(a.RepairAtRoute,Is.Null);
        }
        [Test]public void RepeatedRepairsAreAllowedAndForgedCheckpointsAreRejected()
        {
            var a=Board(12);Assert.That(a.TryUseToolAtomically(TravelTool.RepairKit,()=>true),Is.True);Assert.That(a.Current.Durability,Is.EqualTo(14));
            a=Board();a.TryUseToolAtomically(TravelTool.RepairKit,()=>true);a.TryUseToolAtomically(TravelTool.LandingLens,()=>true);
            Assert.That(a.ToolUsesRemaining,Is.GreaterThan(0));Assert.That(a.TryUseToolAtomically(TravelTool.RepairKit,()=>true),Is.True);
            var record=AttemptRecord.Capture(a);record.RepairAtRoute=record.Commands.Length+1;
            Assert.Throws<ArgumentException>(()=>AttemptService.Restore(a.Level,record));
            record=AttemptRecord.Capture(a);record.RepairKitUsed=false;
            Assert.Throws<ArgumentException>(()=>AttemptService.Restore(a.Level,record));
        }
        [Test]public void LegacyJsonAndCloudInventoryKeepCompatibleSixthStock()
        {
            Assert.That(JsonConvert.SerializeObject(new MonetizationLedger()),Does.Not.Contain("RepairKits"));
            Assert.That(JsonConvert.SerializeObject(AttemptRecord.Capture(Board())),Does.Not.Contain("RepairAtRoute").And.Not.Contain("RepairKitUsed"));
            var profile=new PlayerProfile();profile.Monetization.SpentCoins=140;profile.Monetization.RepairKits=1;
            // Virtual purchases are financed by earned clears and survive local account import.
            var nodes=Enumerable.Range(1,8).Select(i=>"C"+i.ToString("00")).ToArray();profile.Rewards.Synchronize(nodes);
            foreach(var node in nodes)profile.Clears.Add(new ClearReceipt{CampaignNodeId=node,BestStars=3,ContentHash=new string('a',64),AttemptId=Guid.NewGuid().ToString("D")});
            var imported=CloudReconciler.ImportLocal(profile,nodes);
            var baseline=JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(profile));baseline.Monetization.SpentCoins=0;baseline.Monetization.RepairKits=0;
            var remote=JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(baseline));remote.Monetization.SpentCoins=80;remote.Monetization.UndoSpools=1;
            var merged=CloudReconciler.Merge(remote,baseline,profile,nodes);
            Assert.That(merged.EconomyConflict,Is.False);Assert.That(merged.Profile.Monetization.RepairKits,Is.EqualTo(1));
            Assert.That(merged.Profile.Monetization.SpentCoins,Is.EqualTo(330));
            Assert.That(imported.Profile.Monetization.RepairKits,Is.EqualTo(1));Assert.That(imported.Profile.Monetization.Copy().RepairKits,Is.EqualTo(1));
        }
    }
}
