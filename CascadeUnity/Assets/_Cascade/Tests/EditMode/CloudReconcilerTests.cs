using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class CloudReconcilerTests
    {
        static readonly string[] Nodes=Enumerable.Range(1,20).Select(i=>"C"+i.ToString("D2")).ToArray();
        static PlayerProfile Copy(PlayerProfile profile)=>JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(profile));
        static PlayerProfile Earned(int count=10)
        {
            var profile=new PlayerProfile();foreach(string node in Nodes.Take(count))profile.Clears.Add(Clear(node));
            profile.Rewards.Synchronize(Nodes.Take(count));profile.Validate();return profile;
        }
        static ClearReceipt Clear(string node,int stars=0,int score=0)=>new ClearReceipt{CampaignNodeId=node,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64),BestStars=stars,BestScore=score};
        static MasteryRecord Record(string hash,int route,int? clean=null)=>new MasteryRecord{PuzzleId="cloud-puzzle",ContentHash=new string(hash[0],64),Mode="Shapes",Locale="en",BestRoute=route,BestClean=clean};
        static AttemptService Board()=>new AttemptService(LevelDefinitionTests.Level(LevelDefinitionTests.Row("DCBA"),LevelDefinitionTests.Row("ABCD"),new[]{new FloorDefinition(12,new[]{true,true,true}),new FloorDefinition(12,new[]{true,true,true})}));
        [Test]public void MergedStarAndScoreBestsKeepEachSourceProof()
        {
            ClearReplay Proof(char marker)=>new ClearReplay{SourceHash=new string(marker,64),Version=6,Stages=Enumerable.Range(0,3).Select(_=>new AttemptRecord{Commands=Array.Empty<string>()}).ToArray()};
            var remote=Earned(1);var local=Copy(remote);remote.Clears[0].BestStars=4;remote.Clears[0].BestScore=1800;remote.Clears[0].Replay=Proof('a');
            local.Clears[0].BestStars=3;local.Clears[0].BestScore=1900;local.Clears[0].Replay=Proof('b');
            var merged=CloudReconciler.Merge(remote,new PlayerProfile(),local,Nodes).Profile.Clears.Single();
            Assert.That(merged.BestStars,Is.EqualTo(2));Assert.That(merged.BestScore,Is.EqualTo(1900));
            Assert.That(merged.Replay.SourceHash,Is.EqualTo(new string('a',64)));Assert.That(merged.ScoreReplay.SourceHash,Is.EqualTo(new string('b',64)));
            Assert.That(remote.Clears.Single().ScoreReplay,Is.Null);Assert.That(merged.Replay,Is.Not.SameAs(remote.Clears[0].Replay));
        }

        [Test] public void ProgressMergesPerModeWhileLocalAttemptAndPreferencesRemainCoherent()
        {
            var local=Earned(3);var board=Board();board.Execute(PuzzleCommand.Swap(0));local.Attempt=AttemptRecord.Capture(board);local.JourneyAttempt=true;
            local.Preferences.UiLocale="uk";local.Preferences.NotificationsEnabled=true;local.HeardStory.Add(6000);local.SeenCoachPrompts.Add("local");
            var remote=Earned(5);remote.Clears[0].BestStars=5;remote.Clears[0].BestScore=1900;remote.HeardStory.Add(6001);remote.SeenCoachPrompts.Add("remote");
            remote.SelectJourney("Words");remote.Clears.Add(Clear("C01",4,1200));remote.HeardStory.Add(6002);
            var merged=CloudReconciler.Merge(remote,new PlayerProfile(),local,Nodes);
            Assert.That(merged.EconomyConflict,Is.False);Assert.That(merged.Profile.PreferredMode,Is.EqualTo("Shapes"));
            Assert.That(merged.Profile.Clears.Count,Is.EqualTo(5));Assert.That(merged.Profile.Clears[0].BestStars,Is.EqualTo(3));
            Assert.That(merged.Profile.Attempt.StateHash,Is.EqualTo(local.Attempt.StateHash));Assert.That(merged.Profile.Attempt,Is.Not.SameAs(local.Attempt));
            Assert.That(merged.Profile.Preferences.UiLocale,Is.EqualTo("uk"));Assert.That(merged.Profile.Preferences.NotificationsEnabled,Is.True);
            Assert.That(merged.Profile.HeardStory,Is.EquivalentTo(new[]{6000,6001}));Assert.That(merged.Profile.SeenCoachPrompts,Is.EquivalentTo(new[]{"local","remote"}));
            var words=merged.Profile.ModeJourneys.Single(j=>j.Mode=="Words");Assert.That(words.Clears.Single().BestStars,Is.EqualTo(2));Assert.That(words.Attempt,Is.Null);
            Assert.That(remote.PreferredMode,Is.EqualTo("Words"));Assert.That(local.Clears.Count,Is.EqualTo(3));merged.Profile.Validate();
        }

        [Test] public void MasteryUsesContentIdentityAndBestRoutesWithoutCrossingVersions()
        {
            var local=Earned();local.ModeRecords.Add(Record("a",12,14));var remote=Earned();remote.ModeRecords.Add(Record("a",9,10));remote.ModeRecords.Add(Record("b",7,7));
            var merged=CloudReconciler.Merge(remote,new PlayerProfile(),local,Nodes).Profile;
            Assert.That(merged.ModeRecords.Count,Is.EqualTo(2));var record=merged.ModeRecords.Single(r=>r.ContentHash[0]=='a');
            Assert.That(record.BestRoute,Is.EqualTo(9));Assert.That(record.BestClean,Is.EqualTo(10));
        }

        [Test] public void IndependentSpendsAndConsumptionsMergeAsDeltasWithoutAddingBalances()
        {
            var baseline=Earned();baseline.Monetization.SpentCoins=120;baseline.Monetization.WayfinderScrolls=1;
            var remote=Copy(baseline);remote.Monetization.SpentCoins+=80;remote.Monetization.UndoSpools=1;
            var local=Copy(baseline);local.Monetization.SpentCoins+=100;local.Monetization.LandingLenses=1;local.Monetization.WayfinderScrolls=0;
            var merged=CloudReconciler.Merge(remote,baseline,local,Nodes);
            Assert.That(merged.EconomyConflict,Is.False);Assert.That(merged.Profile.Monetization.SpentCoins,Is.EqualTo(300));
            Assert.That(merged.Profile.Monetization.WayfinderScrolls,Is.Zero);Assert.That(merged.Profile.Monetization.UndoSpools,Is.EqualTo(1));Assert.That(merged.Profile.Monetization.LandingLenses,Is.EqualTo(1));
            Assert.That(merged.Profile.Rewards.EarnedCoins(Nodes),Is.EqualTo(500));
        }

        [Test] public void ConcurrentOverspendOrDoubleConsumptionKeepsRemoteEconomy()
        {
            var baseline=Earned(3);var remote=Copy(baseline);remote.Monetization.SpentCoins=100;remote.Monetization.LandingLenses=1;
            var local=Copy(baseline);local.Monetization.SpentCoins=80;local.Monetization.UndoSpools=1;
            var merged=CloudReconciler.Merge(remote,baseline,local,Nodes);Assert.That(merged.EconomyConflict,Is.True);
            Assert.That(merged.Profile.Monetization.SpentCoins,Is.EqualTo(100));Assert.That(merged.Profile.Monetization.LandingLenses,Is.EqualTo(1));Assert.That(merged.Profile.Monetization.UndoSpools,Is.Zero);
            baseline=Earned();baseline.Monetization.SpentCoins=120;baseline.Monetization.WayfinderScrolls=1;remote=Copy(baseline);local=Copy(baseline);
            remote.Monetization.WayfinderScrolls=local.Monetization.WayfinderScrolls=0;
            Assert.That(CloudReconciler.Merge(remote,baseline,local,Nodes).EconomyConflict,Is.True);
        }

        [Test] public void GrantsCannotChangeDisappearOrArriveThroughUnverifiedSync()
        {
            var baseline=Earned();baseline.Monetization.Grants.Add(new GrantReceipt{Id="paid",Kind="store_coins",Product=PurchaseCatalog.Coins240,Amount=240});baseline.Monetization.BonusCoins=240;
            var remote=Copy(baseline);var local=Copy(baseline);local.Monetization.Grants[0].Product=PurchaseCatalog.Coins480;local.Monetization.Grants[0].Amount=480;local.Monetization.BonusCoins=480;
            Assert.That(CloudReconciler.Merge(remote,baseline,local,Nodes).EconomyConflict,Is.True);
            local=Copy(baseline);local.Monetization.Grants.Clear();local.Monetization.BonusCoins=0;Assert.That(CloudReconciler.Merge(remote,baseline,local,Nodes).EconomyConflict,Is.True);
            local=Copy(baseline);local.Monetization.Grants.Add(new GrantReceipt{Id="reward",Kind="coins",Product="parcel:4",Amount=40});local.Monetization.BonusCoins+=40;
            Assert.That(CloudReconciler.Merge(remote,baseline,local,Nodes).EconomyConflict,Is.True);
            var merged=CloudReconciler.Merge(remote,baseline,local,Nodes,true);Assert.That(merged.Profile.Monetization.BonusCoins,Is.EqualTo(280));
            Assert.That(merged.Profile.Monetization.Grants.Single(g=>g.Id=="reward").Product,Is.EqualTo("parcel:4"));
        }

        [Test] public void VerifiedReceiptReplayAndHintStateReplayAreIdempotent()
        {
            var baseline=Earned();var remote=Copy(baseline);var local=Copy(baseline);
            foreach(var profile in new[]{remote,local}){profile.Monetization.Grants.Add(new GrantReceipt{Id="same",Kind="coins",Amount=40});profile.Monetization.BonusCoins=40;}
            var merge=CloudReconciler.Merge(remote,baseline,local,Nodes);Assert.That(merge.EconomyConflict,Is.False);Assert.That(merge.Profile.Monetization.BonusCoins,Is.EqualTo(40));Assert.That(merge.Profile.Monetization.Grants.Count,Is.EqualTo(1));
            baseline=Earned();baseline.Monetization.Grants.Add(new GrantReceipt{Id="hint",Kind="hint",Amount=1});baseline.Monetization.HintTokens=1;remote=Copy(baseline);local=Copy(baseline);
            string state=Guid.NewGuid().ToString("D")+"|"+new string('a',64);
            foreach(var profile in new[]{remote,local}){profile.Monetization.HintTokens=0;profile.Monetization.PaidHintStates.Add(state);}
            merge=CloudReconciler.Merge(remote,baseline,local,Nodes);Assert.That(merge.EconomyConflict,Is.False);Assert.That(merge.Profile.Monetization.HintTokens,Is.Zero);Assert.That(merge.Profile.Monetization.PaidHintStates.Count,Is.EqualTo(1));
        }

        [Test] public void FirstImportScrubsPaidValueAndRetainsOnlyEarnedFinancedTools()
        {
            var local=Earned();local.Monetization.BonusCoins=240;local.Monetization.NoAutomaticAds=true;local.Monetization.SpentCoins=120;local.Monetization.WayfinderScrolls=1;
            local.Monetization.Grants.Add(new GrantReceipt{Id="coins",Kind="store_coins",Product=PurchaseCatalog.Coins240,Amount=240});
            local.Monetization.Grants.Add(new GrantReceipt{Id="paid-gift",Kind="cosmetic",Product="moon"});local.Monetization.Cosmetics.Add("moon");local.Rewards.EquippedGift="moon";
            var imported=CloudReconciler.ImportLocal(local,Nodes);Assert.That(imported.EconomyConflict,Is.False);
            Assert.That(imported.Profile.Monetization.BonusCoins,Is.Zero);Assert.That(imported.Profile.Monetization.NoAutomaticAds,Is.False);Assert.That(imported.Profile.Monetization.Grants,Is.Empty);
            Assert.That(imported.Profile.Monetization.Cosmetics,Is.Empty);Assert.That(imported.Profile.Rewards.EquippedGift,Is.Null);Assert.That(imported.Profile.Monetization.WayfinderScrolls,Is.EqualTo(1));
            Assert.That(local.Monetization.BonusCoins,Is.EqualTo(240));Assert.That(local.Monetization.NoAutomaticAds,Is.True);
            local.Monetization.SpentCoins=600;Assert.That(CloudReconciler.ImportLocal(local,Nodes).EconomyConflict,Is.True);
        }

        [Test] public void ClaimedParcelsVisitsAndClearIncomeDoNotRepeatAcrossModesOrMerges()
        {
            var remote=Earned(6);
            // This regression is specifically about merging pre-recipe-5 leaf fragments.
            remote.Rewards.Parcels=remote.Rewards.Parcels.Select(p=>new ParcelReceipt{Order=p.Order,GiftId=JourneyRewards.Gifts[(p.Order-1)/3]}).ToList();
            remote.Rewards.ClaimParcel(1);remote.Rewards.RegisterVisit(new DateTime(2026,10,1));
            var local=Copy(remote);local.Rewards.ClaimParcel(2);local.Rewards.RegisterVisit(new DateTime(2026,10,2));local.SelectJourney("Words");local.Clears.Add(Clear("C01"));
            var merged=CloudReconciler.Merge(remote,new PlayerProfile(),local,Nodes).Profile;
            Assert.That(merged.Rewards.Parcels.Count,Is.EqualTo(2));Assert.That(merged.Rewards.Parcels.All(p=>p.Claimed),Is.True);Assert.That(merged.Rewards.VisitDates.Count,Is.EqualTo(2));
            Assert.That(merged.Rewards.EarnedCoins(Nodes),Is.EqualTo(264));
            Assert.That(merged.Rewards.Owns("paper_crane"),Is.True);
            var repeated=CloudReconciler.Merge(merged,merged,merged,Nodes).Profile;Assert.That(repeated.Rewards.Parcels.Count,Is.EqualTo(2));Assert.That(repeated.Rewards.ClaimParcel(1),Is.False);
        }

        [Test] public void ConflictingParcelClaimsKeepCanonicalGiftAndRemoteEconomy()
        {
            var remote=Earned(3);
            // Preserve a genuine pre-v4 fragment receipt; fixed v4 prizes cannot be edited.
            remote.Rewards.Parcels[0]=new ParcelReceipt{Order=1,GiftId="leaf"};remote.Rewards.ClaimParcel(1);var local=Copy(remote);
            local.Rewards.Parcels[0].GiftId="sun";local.Monetization.SpentCoins=80;local.Monetization.UndoSpools=1;
            var merge=CloudReconciler.Merge(remote,new PlayerProfile(),local,Nodes);
            Assert.That(merge.EconomyConflict,Is.True);Assert.That(merge.Profile.Rewards.Parcels.Single().GiftId,Is.EqualTo("leaf"));
            Assert.That(merge.Profile.Rewards.Parcels.Single().Claimed,Is.True);Assert.That(merge.Profile.Monetization.SpentCoins,Is.Zero);Assert.That(merge.Profile.Monetization.UndoSpools,Is.Zero);
        }

        [Test] public void UnknownUploadedNodesCannotFinanceInventory()
        {
            var local=Earned(3);local.Monetization.SpentCoins=120;local.Monetization.WayfinderScrolls=1;
            var result=CloudReconciler.Merge(new PlayerProfile(),new PlayerProfile(),local,new[]{"C01"});
            Assert.That(result.EconomyConflict,Is.True);Assert.That(result.Profile.Clears.Count,Is.EqualTo(1));Assert.That(result.Profile.Monetization.WayfinderScrolls,Is.Zero);
        }

        [Test] public void InvalidAuthorityCannotBecomeTheConflictFallback()
        {
            var remote=Earned(3);remote.Monetization.BonusCoins=240;
            Assert.Throws<InvalidDataException>(()=>CloudReconciler.Merge(remote,new PlayerProfile(),Earned(3),Nodes));
            remote=Earned(3);remote.Monetization.WayfinderScrolls=1;
            Assert.Throws<InvalidDataException>(()=>CloudReconciler.Merge(remote,new PlayerProfile(),Earned(3),Nodes));
            remote=Earned(3);remote.Monetization.SpentCoins=121;
            Assert.Throws<InvalidDataException>(()=>CloudReconciler.Merge(remote,new PlayerProfile(),Earned(3),Nodes));
        }

        [Test] public void DeviceCheckpointsAreBoundedLeanAndRestoreAttemptWithoutReplacingProgress()
        {
            var local=Earned(3);var attempt=Board();attempt.Execute(PuzzleCommand.Swap(0));local.Attempt=AttemptRecord.Capture(attempt);local.JourneyAttempt=true;
            var checkpoint=DeviceCheckpoint.Capture("device",10,local);Assert.That(checkpoint.Journeys.Single().Clears,Is.Empty);
            var remote=Earned(5);checkpoint.ApplyTo(remote);Assert.That(remote.Clears.Count,Is.EqualTo(5));Assert.That(remote.Attempt.StateHash,Is.EqualTo(local.Attempt.StateHash));
            var account=new CloudAccount{Profile=remote};for(int i=0;i<12;i++)account.UpsertDevice(DeviceCheckpoint.Capture("device-"+i,i,local));
            Assert.That(account.Devices.Count,Is.EqualTo(CloudAccount.MaxDevices));Assert.That(account.Devices.Any(d=>d.DeviceId=="device-0"),Is.False);account.Validate();
            account.UpsertDevice(DeviceCheckpoint.Capture("device-11",20,local));Assert.That(account.Devices.Count,Is.EqualTo(CloudAccount.MaxDevices));
        }

        [Test] public void MiddleDescentAndItsPinnedDirectionRemainOneDeviceCheckpoint()
        {
            var template=Board().Level;
            var source=new LevelDefinition("cloud-stage","C02",template.SchemaVersion,template.RulesVersion,new string('a',64),template.InitialSymbolIds,template.TargetSymbolIds,template.TargetSegments,template.Floors);
            var run=DescentRun.Build(source,version:5);Assert.That(run,Is.Not.Null);
            var first=new AttemptService(run.Level(source,0),direction:run.Direction);
            foreach(string command in run.Stages[0].Witness)first.Execute(command=="D"?PuzzleCommand.Drop:PuzzleCommand.Swap(int.Parse(command)));
            Assert.That(first.Current.Status,Is.EqualTo(PuzzleStatus.Won));run.AcceptCheckpoint(source,first);
            var middle=new AttemptService(run.Level(source,1),direction:run.Direction);middle.AdvanceActiveTime(23);
            var local=Earned(3);local.Attempt=AttemptRecord.Capture(middle);local.Descent=run;local.JourneyAttempt=true;
            var merged=CloudReconciler.Merge(Earned(5),new PlayerProfile(),local,Nodes).Profile;
            Assert.That(merged.Descent.StageIndex,Is.EqualTo(1));Assert.That(merged.Descent.SourceHash,Is.EqualTo(source.ContentHash));
            Assert.That(merged.Attempt.StateHash,Is.EqualTo(middle.CurrentStateHash));Assert.That(merged.Attempt.ActiveSeconds,Is.EqualTo(23));
            Assert.That(merged.Attempt.ContentHash,Is.EqualTo(merged.Descent.StageHash(1)));merged.Descent.Validate(source,"en");
            var checkpoint=DeviceCheckpoint.Capture("middle",1,merged);var restored=Earned(6);checkpoint.ApplyTo(restored);
            Assert.That(restored.Descent.StageIndex,Is.EqualTo(1));Assert.That(restored.Attempt.StateHash,Is.EqualTo(middle.CurrentStateHash));Assert.That(restored.Clears.Count,Is.EqualTo(6));
        }
    }
}
