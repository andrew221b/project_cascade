using System;
using System.IO;
using Cascade.Persistence;
using NUnit.Framework;
namespace Cascade.Tests
{
    public sealed class CloudJournalTests
    {
        string directory,path;
        [SetUp] public void SetUp(){directory=Path.Combine(Path.GetTempPath(),"cascade-journal-"+Guid.NewGuid().ToString("N"));path=Path.Combine(directory,"journal.json");}
        [TearDown] public void TearDown(){if(Directory.Exists(directory))Directory.Delete(directory,true);}
        [Test] public void PendingRequestSurvivesRestart(){var store=new CloudJournalStore(path);var journal=new CloudJournal{Sequence=7,Revision=11,RequestId="7:"+Guid.NewGuid().ToString("D"),PendingJson=CloudJson.Encode(new PlayerProfile()),PlayerId="player",ProjectId="project",Environment="development",AccountLoadPending=true};store.Save(journal);var restored=new CloudJournalStore(path).Load();Assert.That(restored.RequestId,Is.EqualTo(journal.RequestId));Assert.That(restored.PendingJson,Is.EqualTo(journal.PendingJson));Assert.That(restored.DeviceId,Is.EqualTo(journal.DeviceId));Assert.That(restored.Sequence,Is.EqualTo(7));Assert.That(restored.AccountLoadPending,Is.True);}
        [Test] public void CorruptMainUsesPreviousAtomicJournal(){var store=new CloudJournalStore(path);var journal=new CloudJournal{Sequence=1};store.Save(journal);journal.Sequence=2;store.Save(journal);File.WriteAllText(path,"broken");Assert.That(store.Load().Sequence,Is.EqualTo(1));}
        [Test] public void BothCorruptPreserveMainAndCreateDifferentDevice(){var store=new CloudJournalStore(path);var original=new CloudJournal();store.Save(original);File.WriteAllText(path,"broken");var recovered=store.Load();Assert.That(recovered.DeviceId,Is.Not.EqualTo(original.DeviceId));Assert.That(File.ReadAllText(path+".unreadable"),Is.EqualTo("broken"));}
        [Test] public void InvalidPendingPairIsNeverSaved(){Assert.Throws<InvalidDataException>(()=>new CloudJournalStore(path).Save(new CloudJournal{PendingJson="{}"}));Assert.That(File.Exists(path),Is.False);}
        [Test] public void PastPendingSequenceIsRejected(){Assert.Throws<InvalidDataException>(()=>new CloudJournalStore(path).Save(new CloudJournal{Sequence=2,RequestId="1:"+Guid.NewGuid().ToString("D"),PendingJson="{}"}));}
        [Test] public void ExplicitProgressRejectionRetiresSnapshotButNeverReusesItsSequence()
        {
            var journal=new CloudJournal{Sequence=9,RequestId="9:"+Guid.NewGuid().ToString("D"),PendingJson="old"};
            Assert.That(journal.RetireRejectedProgress("NetworkTimeout"),Is.False);
            Assert.That(journal.PendingJson,Is.EqualTo("old"));
            Assert.That(journal.RetireRejectedProgress("UnverifiedProgress"),Is.True);
            new CloudJournalStore(path).Save(journal);var restored=new CloudJournalStore(path).Load();
            Assert.That(restored.Sequence,Is.EqualTo(9));Assert.That(restored.RequestId,Is.Null);Assert.That(restored.PendingJson,Is.Null);
        }
        [Test] public void JsonProfileRoundTripHasNoDerivedProperties(){string data=CloudJson.Encode(new PlayerProfile());Assert.That(data,Does.Not.Contain("AllClears"));var restored=CloudJson.Decode<PlayerProfile>(data);restored.Validate();}
        [Test] public void ExistingAccountCandidateKeepsOriginalSessionNamespaceOnRestart()
        {
            var journal=new CloudJournal{AuthenticationProfile="default",AccountCandidateProfile="cascade-candidate",AccountLoadPending=true};
            new CloudJournalStore(path).Save(journal);var loaded=new CloudJournalStore(path).Load();
            Assert.That(loaded.AuthenticationProfile,Is.EqualTo("default"));
            Assert.That(loaded.AccountCandidateProfile,Is.EqualTo("cascade-candidate"));
            Assert.That(loaded.AccountLoadPending,Is.True);
            Assert.That(CloudJson.Encode(loaded),Does.Not.Contain("AccessToken"));
        }
        [Test] public void LegacyJournalUsesOriginalDefaultSdkSession()
        {
            var loaded=CloudJson.Decode<CloudJournal>("{\"Schema\":1,\"DeviceId\":\""+Guid.NewGuid().ToString("D")+"\",\"AccountLoadPending\":true}");
            loaded.Validate();Assert.That(loaded.AuthenticationProfile,Is.EqualTo("default"));Assert.That(loaded.AccountCandidateProfile,Is.Null);
        }
        [Test] public void SavedReminderOptOutSurvivesNewDefaultAndPermissionMarker()
        {
            var p=CloudJson.Decode<PlayerProfile>("{\"Preferences\":{\"NotificationsEnabled\":false}}");
            Assert.That(p.Preferences.NotificationsEnabled,Is.False);
            Assert.That(p.Preferences.NotificationPermissionRequested,Is.False);
            var restored=CloudJson.Clone(p);Assert.That(restored.Preferences.NotificationsEnabled,Is.False);
        }
    }
}
