using System;
using Cascade.Application;

namespace Cascade.Persistence
{
    public static class MonetizationTransaction
    {
        // Pending puzzle planning exposes a temporary view attempt; save the ledger with the last verified attempt instead.
        public static AdService CreatePreview(PlayerProfile profile,ProfileStore store,Func<AttemptService> activeAttempt,Func<bool> skipAttemptCapture=null)
        {
            return new AdService(profile.Monetization.Copy(),candidate=>{
                var old=profile.Attempt;
                try{if(skipAttemptCapture?.Invoke()!=true)profile.Attempt=AttemptRecord.Capture(activeAttempt());store.Save(profile);}
                catch{profile.Attempt=old;throw;}
            },profile.Clears.Count,previewOnly:true);
        }
        public static AdService Create(PlayerProfile profile,ProfileStore store,Func<AttemptService> activeAttempt=null,Func<bool> skipAttemptCapture=null)
        {
            return new AdService(profile.Monetization,candidate=>{
                var oldLedger=profile.Monetization;var oldAttempt=profile.Attempt;
                try{profile.Monetization=candidate;if(activeAttempt!=null&&skipAttemptCapture?.Invoke()!=true)profile.Attempt=AttemptRecord.Capture(activeAttempt());store.Save(profile);}
                catch{profile.Monetization=oldLedger;profile.Attempt=oldAttempt;throw;}
            },profile.Clears.Count);
        }
    }
}
