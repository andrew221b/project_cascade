using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class ModeJourneyTests
    {
        [Test] public void JourneysKeepIndependentClearsDirectorAndStageAfterSaveReload()
        {
            var catalog=ContentTests.LoadCatalog();var source=catalog.GetPrimary("C02").Definition;
            var run=DescentRun.Build(source,version:3);var first=new AttemptService(run.Level(source,0));
            foreach(var command in run.Stages[0].Witness)first.Execute(command=="D"?PuzzleCommand.Drop:PuzzleCommand.Swap(int.Parse(command)));
            run.AcceptCheckpoint(source,first);
            var second=new AttemptService(run.Level(source,1));second.AdvanceActiveTime(12);
            var p=new PlayerProfile{Attempt=AttemptRecord.Capture(second),Descent=run,JourneyAttempt=true};
            p.Clears.Add(new ClearReceipt{CampaignNodeId="C01",AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64),BestStars=5,BestScore=2000});
            p.Rewards.Synchronize(new[]{"C01"});var director=p.Director;
            p.Rewards.RecordDelivery(first,records:p.ModeRecords);Assert.That(p.ModeRecords.Count,Is.EqualTo(1));
            p.SelectJourney("Words");Assert.That(p.Clears,Is.Empty);Assert.That(p.Attempt,Is.Null);Assert.That(p.Descent,Is.Null);Assert.That(p.Director,Is.Not.SameAs(director));
            Assert.That(p.ModeRecords,Is.Empty);
            p.HeardStory.Add(2);p.SelectJourney("Mixed");Assert.That(p.HeardStory,Is.Empty);p.SelectJourney("Shapes");
            Assert.That(p.Clears.Single().BestStars,Is.EqualTo(5));Assert.That(p.Director,Is.SameAs(director));Assert.That(p.Descent.StageIndex,Is.EqualTo(1));
            Assert.That(p.ModeRecords.Count,Is.EqualTo(1));
            string dir=Path.Combine(Path.GetTempPath(),"CascadeModes",Guid.NewGuid().ToString("N"));
            try {var store=new ProfileStore(Path.Combine(dir,"save.json"));store.Save(p);var loaded=store.Load();Assert.That(loaded.Recovery,Is.EqualTo(SaveRecovery.None));
                p=loaded.Profile;Assert.That(p.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(second.CurrentStateHash));Assert.That(p.Attempt.ActiveSeconds,Is.EqualTo(12));
                Assert.That(p.ModeJourneys.Count,Is.EqualTo(2));p.SelectJourney("Words");Assert.That(p.HeardStory,Is.EqualTo(new[]{2}));Assert.That(p.Clears,Is.Empty);
                Assert.That(p.Rewards.SeenClearNodes,Is.EqualTo(new[]{"C01"}));p.Validate();
            } finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
        [Test] public void LegacySchemaAssignsTheJourneyOnlyToItsSavedMode()
        {
            var profile=new PlayerProfile{PreferredMode="Words"};profile.HeardStory.Add(3);
            var catalog=ContentTests.LoadCatalog();var won=new AttemptService(catalog.GetPrimary("C01").Definition);won.Execute(PuzzleCommand.Swap(0));profile.Rewards.RecordDelivery(won);
            var envelope=JObject.Parse(ProfileStore.Encode(profile));var payload=JObject.Parse((string)envelope["payload"]);payload.Remove("ModeJourneys");payload.Remove("ModeRecords");
            string raw=payload.ToString(Newtonsoft.Json.Formatting.None);envelope["schema"]=1;envelope["payload"]=raw;envelope["sha256"]=ContentImporter.Hash(raw);
            string dir=Path.Combine(Path.GetTempPath(),"CascadeLegacyModes",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
            try{string path=Path.Combine(dir,"save.json");File.WriteAllText(path,envelope.ToString());var load=new ProfileStore(path).Load();
                Assert.That(load.Recovery,Is.EqualTo(SaveRecovery.None));Assert.That(load.Profile.HeardStory,Is.EqualTo(new[]{3}));
                Assert.That(load.Profile.ModeRecords.Count,Is.EqualTo(1));
                load.Profile.SelectJourney("Shapes");Assert.That(load.Profile.HeardStory,Is.Empty);load.Profile.SelectJourney("Words");Assert.That(load.Profile.HeardStory,Is.EqualTo(new[]{3}));
            }finally{Directory.Delete(dir,true);}
        }
        [Test] public void DuplicateOrActiveArchiveIsRejectedAndRewardsCannotBeDuplicated()
        {
            var p=new PlayerProfile();p.ModeJourneys.Add(new ModeJourney{Mode="Shapes"});Assert.Throws<InvalidDataException>(()=>p.Validate());
            p.ModeJourneys.Clear();p.Rewards.Synchronize(new[]{"C01"});Assert.Throws<InvalidDataException>(()=>p.Validate());
            p.Clears.Add(new ClearReceipt{CampaignNodeId="C01",AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64)});
            p.SelectJourney("Words");p.Rewards.Synchronize(new[]{"C01"});Assert.That(p.Rewards.SeenClearNodes.Count,Is.EqualTo(1));p.Validate();
            Assert.Throws<ArgumentException>(()=>p.SelectJourney("Unknown"));Assert.That(p.PreferredMode,Is.EqualTo("Words"));
        }
    }
}
