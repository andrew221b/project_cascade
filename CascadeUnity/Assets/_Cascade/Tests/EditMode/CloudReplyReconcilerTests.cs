using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Persistence;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class CloudReplyReconcilerTests
    {
        static readonly string[] Nodes=Enumerable.Range(1,10).Select(i=>"C"+i.ToString("D2")).ToArray();
        static PlayerProfile SentPurchase()
        {
            var profile=new PlayerProfile();
            foreach(string node in Nodes)profile.Clears.Add(new ClearReceipt{CampaignNodeId=node,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64)});
            profile.Rewards.Synchronize(Nodes);
            profile.Monetization.SpentCoins=80;profile.Monetization.UndoSpools=1;profile.Validate();return profile;
        }
        static PlayerProfile SecondPurchase(PlayerProfile sent)
        {
            var profile=CloudJson.Clone(sent);profile.Monetization.SpentCoins=160;profile.Monetization.UndoSpools=2;
            profile.HeardStory.Add(6000);profile.Preferences.UiLocale="uk";profile.Validate();return profile;
        }

        [Test] public void RecoveredOlderPendingRequestDoesNotReplayAlreadyAcceptedPurchase()
        {
            var sent=SentPurchase();var current=SecondPurchase(sent);var remote=CloudJson.Clone(current);remote.HeardStory.Clear();
            // A third purchase would still fit this wallet: overspend validation alone cannot catch replay.
            Assert.That(remote.Rewards.EarnedCoins(Nodes),Is.GreaterThanOrEqualTo(240));
            var reply=CloudReplyReconciler.Apply(remote,sent,current,Nodes,1,2,false);
            Assert.That(reply.EconomyConflict,Is.True);
            Assert.That(reply.Profile.Monetization.SpentCoins,Is.EqualTo(160));Assert.That(reply.Profile.Monetization.UndoSpools,Is.EqualTo(2));
            Assert.That(reply.Profile.HeardStory,Does.Contain(6000));Assert.That(reply.Profile.Preferences.UiLocale,Is.EqualTo("uk"));
            Assert.That(sent.Monetization.SpentCoins,Is.EqualTo(80));Assert.That(current.Monetization.SpentCoins,Is.EqualTo(160));reply.Profile.Validate();
        }

        [Test] public void CurrentPurchaseMadeDuringRequestIsMergedOnce()
        {
            var sent=SentPurchase();var remote=CloudJson.Clone(sent);var current=SecondPurchase(sent);
            var reply=CloudReplyReconciler.Apply(remote,sent,current,Nodes,1,1,false);
            Assert.That(reply.EconomyConflict,Is.False);
            Assert.That(reply.Profile.Monetization.SpentCoins,Is.EqualTo(160));Assert.That(reply.Profile.Monetization.UndoSpools,Is.EqualTo(2));
            var repeated=CloudReplyReconciler.Apply(remote,sent,reply.Profile,Nodes,1,1,false);
            Assert.That(repeated.EconomyConflict,Is.False);
            Assert.That(repeated.Profile.Monetization.SpentCoins,Is.EqualTo(160));Assert.That(repeated.Profile.Monetization.UndoSpools,Is.EqualTo(2));
            Assert.That(remote.Monetization.SpentCoins,Is.EqualTo(80));repeated.Profile.Validate();
        }

        [Test] public void ServerEconomyConflictAdoptsRemoteDespiteUnsubmittedCurrentPurchase()
        {
            var sent=SentPurchase();var remote=CloudJson.Clone(sent);var current=SecondPurchase(sent);
            var reply=CloudReplyReconciler.Apply(remote,sent,current,Nodes,1,1,true);
            Assert.That(reply.EconomyConflict,Is.True);
            Assert.That(reply.Profile.Monetization.SpentCoins,Is.EqualTo(80));Assert.That(reply.Profile.Monetization.UndoSpools,Is.EqualTo(1));
            Assert.That(reply.Profile.HeardStory,Does.Contain(6000));Assert.That(reply.Profile.Preferences.UiLocale,Is.EqualTo("uk"));
            Assert.That(current.Monetization.SpentCoins,Is.EqualTo(160));reply.Profile.Validate();
        }

        static PlayerProfile RecoverAppliedReply(PlayerProfile sent,PlayerProfile remote,PlayerProfile current,bool conflict=false,bool corruptJournal=false)
        {
            string directory=Path.Combine(Path.GetTempPath(),"cascade-reply-"+Guid.NewGuid().ToString("N"));
            try{
                string journalPath=Path.Combine(directory,"journal.json"),profilePath=Path.Combine(directory,"profile.json");
                var journalStore=new CloudJournalStore(journalPath);var profileStore=new ProfileStore(profilePath);
                var journal=new CloudJournal{Sequence=1,RequestId="1:"+Guid.NewGuid().ToString("D"),PendingJson=CloudJson.Encode(sent)};
                journalStore.Save(journal);
                var reply=CloudReplyReconciler.Apply(remote,sent,current,Nodes,1,1,conflict);
                journal.EconomyConflict=reply.EconomyConflict;journalStore.Save(journal);
                reply.Profile.CloudAppliedRequestId=journal.RequestId;reply.Profile.CloudAppliedEconomyConflict=reply.EconomyConflict;
                profileStore.Save(reply.Profile);
                // Process stops here: the profile is durable but the journal still contains the old request.
                if(corruptJournal)File.WriteAllText(journalPath,"broken");
                var recoveredJournal=new CloudJournalStore(journalPath).Load();var recoveredProfile=new ProfileStore(profilePath).Load();
                Assert.That(recoveredProfile.Recovery,Is.EqualTo(SaveRecovery.None));
                Assert.That(recoveredJournal.PendingJson,Is.EqualTo(journal.PendingJson));
                Assert.That(CloudReplyReconciler.CanAcknowledgeApplied(recoveredProfile.Profile,recoveredJournal.RequestId,recoveredJournal.Sequence,1),Is.True);
                Assert.That(recoveredJournal.EconomyConflict||recoveredProfile.Profile.CloudAppliedEconomyConflict,Is.EqualTo(reply.EconomyConflict));
                return recoveredProfile.Profile;
            }finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }

        [Test] public void AtomicProfileMarkerAcknowledgesCrashReplayWithoutDuplicatingRemotePurchase()
        {
            var sent=SentPurchase();var remote=SecondPurchase(sent);
            var recovered=RecoverAppliedReply(sent,remote,CloudJson.Clone(sent));
            Assert.That(recovered.Monetization.SpentCoins,Is.EqualTo(160));Assert.That(recovered.Monetization.UndoSpools,Is.EqualTo(2));
            Assert.That(recovered.CloudAppliedEconomyConflict,Is.False);
        }

        [Test] public void AtomicProfileMarkerRetainsAlreadyMergedInflightPurchaseOnReplay()
        {
            var sent=SentPurchase();var current=SecondPurchase(sent);
            var recovered=RecoverAppliedReply(sent,CloudJson.Clone(sent),current);
            Assert.That(recovered.Monetization.SpentCoins,Is.EqualTo(160));Assert.That(recovered.Monetization.UndoSpools,Is.EqualTo(2));
            Assert.That(recovered.HeardStory,Does.Contain(6000));Assert.That(recovered.Preferences.UiLocale,Is.EqualTo("uk"));
        }

        [Test] public void RecoveredJournalBackupRetainsAppliedProfileConflictAndMarker()
        {
            var sent=SentPurchase();var recovered=RecoverAppliedReply(sent,CloudJson.Clone(sent),SecondPurchase(sent),true,true);
            Assert.That(recovered.Monetization.SpentCoins,Is.EqualTo(80));Assert.That(recovered.Monetization.UndoSpools,Is.EqualTo(1));
            Assert.That(recovered.CloudAppliedEconomyConflict,Is.True);
        }

        [Test] public void ReplyAcknowledgmentRequiresExactDeviceRequestAndServerAcceptance()
        {
            var profile=SentPurchase();string request="1:"+Guid.NewGuid().ToString("D");profile.CloudAppliedRequestId=request;
            Assert.That(CloudReplyReconciler.CanAcknowledgeApplied(profile,request,1,1),Is.True);
            Assert.That(CloudReplyReconciler.CanAcknowledgeApplied(profile,request,1,2),Is.True);
            Assert.That(CloudReplyReconciler.CanAcknowledgeApplied(profile,request,1,0),Is.False);
            Assert.That(CloudReplyReconciler.CanAcknowledgeApplied(profile,"1:"+Guid.NewGuid().ToString("D"),1,1),Is.False);
            Assert.That(CloudReplyReconciler.CanAcknowledgeApplied(new PlayerProfile(),null,1,1),Is.False);
        }

        [Test] public void ReconciliationKeepsLocalMarkersWithoutAdoptingAnotherDeviceMarker()
        {
            var baseline=SentPurchase();var remote=CloudJson.Clone(baseline);var local=CloudJson.Clone(baseline);
            remote.CloudAppliedRequestId="1:"+Guid.NewGuid().ToString("D");remote.CloudAppliedEconomyConflict=true;
            var reply=CloudReconciler.Merge(remote,baseline,local,Nodes).Profile;
            Assert.That(reply.CloudAppliedRequestId,Is.Null);Assert.That(reply.CloudAppliedEconomyConflict,Is.False);
            local.CloudAppliedRequestId="2:"+Guid.NewGuid().ToString("D");
            reply=CloudReconciler.Merge(remote,baseline,local,Nodes).Profile;
            Assert.That(reply.CloudAppliedRequestId,Is.EqualTo(local.CloudAppliedRequestId));Assert.That(reply.CloudAppliedEconomyConflict,Is.False);
        }

        [Test] public void VerifiedGrantReplyPreservesPurchaseMadeDuringVerification()
        {
            var baseline=SentPurchase();var current=SecondPurchase(baseline);var remote=CloudJson.Clone(baseline);
            remote.Monetization.Grants.Add(new GrantReceipt{Id="verified-store-receipt",Kind="store_coins",Product=PurchaseCatalog.Coins240,Amount=240});remote.Monetization.BonusCoins=240;
            var reply=CloudReconciler.Merge(remote,baseline,current,Nodes,true);
            Assert.That(reply.EconomyConflict,Is.False);Assert.That(reply.Profile.Monetization.SpentCoins,Is.EqualTo(160));Assert.That(reply.Profile.Monetization.UndoSpools,Is.EqualTo(2));
            Assert.That(reply.Profile.Monetization.BonusCoins,Is.EqualTo(240));Assert.That(reply.Profile.Monetization.Grants.Single().Id,Is.EqualTo("verified-store-receipt"));reply.Profile.Validate();
        }
    }
}
