using System;
using System.IO;
using System.Linq;
using System.Threading;
using Cascade.Persistence;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class ReleasePersistenceTests
    {
        string directory;ProfileStore store;
        [SetUp]public void Setup(){directory=Path.Combine(Path.GetTempPath(),"CascadeWriteQueue",Guid.NewGuid().ToString("N"));store=new ProfileStore(Path.Combine(directory,"profile.json"));}
        [TearDown]public void Cleanup(){store.Flush();if(Directory.Exists(directory))Directory.Delete(directory,true);}
        [Test]public void AutosaveCapturesAnImmutableSnapshotAndKeepsNewestCheckpoint()
        {
            var profile=new PlayerProfile();profile.Preferences.UiLocale="uk";store.QueueSave(profile);profile.Preferences.UiLocale="ru";
            store.Flush();Assert.That(store.Load().Profile.Preferences.UiLocale,Is.EqualTo("uk"));
            for(int i=0;i<30;i++){profile.FirstGestureShown=i==29;store.QueueSave(profile);}
            store.Flush();Assert.That(store.Load().Profile.FirstGestureShown,Is.True);
        }
        [Test]public void DurablePurchaseBarrierCannotBeOverwrittenByAnOlderAutosave()
        {
            var profile=new PlayerProfile();store.QueueSave(profile);
            profile.Preferences.UiLocale="ru";store.Save(profile);store.Flush();
            Assert.That(store.Load().Profile.Preferences.UiLocale,Is.EqualTo("ru"));
        }
        [Test]public void FailedBackgroundWriteRemainsRetryableAndReportsTheError()
        {
            Directory.CreateDirectory(store.Path+".tmp");store.QueueSave(new PlayerProfile());
            Assert.That(SpinWait.SpinUntil(()=>store.WriteError!=null,5000),Is.True);
            Assert.Throws<IOException>(()=>store.Flush());Directory.Delete(store.Path+".tmp");store.Flush();
            Assert.That(store.WriteError,Is.Null);Assert.That(store.Load().Recovery,Is.EqualTo(SaveRecovery.None));
        }
        [Test]public void SmallCompressedBombCannotExpandPastTheDecodedBudget()
        {
            using(var output=new MemoryStream()){
                using(var zip=new System.IO.Compression.GZipStream(output,System.IO.Compression.CompressionLevel.Fastest,true)){
                    var block=new byte[8192];for(int i=0;i<=CloudProfilePayload.MaxDecodedBytes/block.Length;i++)zip.Write(block,0,block.Length);
                }
                string packed="gz1:"+Convert.ToBase64String(output.ToArray());
                Assert.That(packed.Length,Is.LessThan(CloudProfilePayload.MaxWireBytes));
                Assert.Throws<InvalidDataException>(()=>CloudProfilePayload.Unpack(packed));
            }
        }
        [Test]public void LargeProfileTransportRetainsEveryReceiptAndAcceptsLegacyJson()
        {
            string json="{\"receipts\":["+string.Join(",",Enumerable.Range(0,10000).Select(i=>"{\"id\":\"purchase_"+i+"\",\"amount\":40}"))+"]}";
            string packed=CloudProfilePayload.Pack(json);
            Assert.That(packed.Length,Is.LessThan(CloudProfilePayload.MaxWireBytes));Assert.That(CloudProfilePayload.Unpack(packed),Is.EqualTo(json));
            Assert.That(CloudProfilePayload.Unpack("{}"),Is.EqualTo("{}"));
            Assert.Throws<InvalidDataException>(()=>CloudProfilePayload.Unpack("gz1:bad!!"));
            Assert.Throws<InvalidDataException>(()=>CloudProfilePayload.Pack(new string('x',CloudProfilePayload.MaxDecodedBytes+1)));
        }
    }
}
