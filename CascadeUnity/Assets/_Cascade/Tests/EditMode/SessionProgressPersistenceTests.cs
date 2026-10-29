using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Persistence;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class SessionProgressPersistenceTests
    {
        string directory;
        [SetUp]public void Setup(){directory=Path.Combine(Path.GetTempPath(),"CascadeSessionProgress",Guid.NewGuid().ToString("N"));}
        [TearDown]public void Cleanup(){if(Directory.Exists(directory))Directory.Delete(directory,true);}
        static PlayerProfile Profile()
        {
            var p=new PlayerProfile{AccountIntroSeen=true,JourneyAttempt=true,Attempt=new AttemptRecord{AttemptId=Guid.NewGuid().ToString("D")}};
            p.Clears.Add(new ClearReceipt{CampaignNodeId="C01",AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64),BestStars=2,BestScore=1400});
            p.Preferences.UiLocale="uk";p.SeenMechanics.Add(MechanicLesson.Swap);
            p.ModeJourneys.Add(new ModeJourney{Mode="Words",JourneyAttempt=true,Attempt=new AttemptRecord{AttemptId=Guid.NewGuid().ToString("D")}});
            p.Validate();return p;
        }
        [Test]public void ProgressSnapshotKeepsEarnedRecordsAndDoesNotMutateRunningRound()
        {
            var p=Profile();var before=CloudJson.Encode(p);var snapshot=p.ProgressSnapshot();
            Assert.That(snapshot.Attempt,Is.Null);Assert.That(snapshot.Descent,Is.Null);Assert.That(snapshot.Completion,Is.Null);
            Assert.That(snapshot.JourneyAttempt,Is.False);Assert.That(snapshot.ModeJourneys.Single().Attempt,Is.Null);
            Assert.That(snapshot.Clears.Single().BestScore,Is.EqualTo(1400));Assert.That(snapshot.Clears.Single().BestStars,Is.EqualTo(2));
            Assert.That(CloudJson.Encode(snapshot.Monetization),Is.EqualTo(CloudJson.Encode(p.Monetization)));
            Assert.That(snapshot.Preferences.UiLocale,Is.EqualTo("uk"));Assert.That(snapshot.SeenMechanics,Is.EqualTo(p.SeenMechanics));
            Assert.That(CloudJson.Encode(p),Is.EqualTo(before));snapshot.Validate();
        }
        [TestCase(false)][TestCase(true)]public void GameWritesKeepProgressWithoutSavingAnUnfinishedRound(bool queued)
        {
            string path=Path.Combine(directory,"profile.json");var store=new ProfileStore(path,persistSessionAttempts:false);var p=Profile();
            if(queued){store.QueueSave(p);store.Flush();}else store.Save(p);
            var saved=new ProfileStore(path).Load().Profile;
            Assert.That(saved.Attempt,Is.Null);Assert.That(saved.ModeJourneys.Single().Attempt,Is.Null);
            Assert.That(saved.Clears.Single().BestScore,Is.EqualTo(1400));Assert.That(p.Attempt,Is.Not.Null);
        }
        [Test]public void LegacySavedAttemptsAreDiscardedWithoutLosingProgress()
        {
            string path=Path.Combine(directory,"profile.json");new ProfileStore(path).Save(Profile());
            var loaded=new ProfileStore(path,persistSessionAttempts:false).Load();
            Assert.That(loaded.Recovery,Is.EqualTo(SaveRecovery.None));Assert.That(loaded.Profile.Attempt,Is.Null);
            Assert.That(loaded.Profile.ModeJourneys.Single().Attempt,Is.Null);Assert.That(loaded.Profile.Clears.Count,Is.EqualTo(1));
        }
    }
}
