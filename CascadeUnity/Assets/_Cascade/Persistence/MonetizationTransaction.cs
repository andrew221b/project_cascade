using System;
using Cascade.Application;

namespace Cascade.Persistence
{
    public static class MonetizationTransaction
    {
        public static AdService Create(PlayerProfile profile,ProfileStore store,Func<AttemptService> activeAttempt=null)
        {
            return new AdService(profile.Monetization,candidate=>{
                var oldLedger=profile.Monetization;var oldAttempt=profile.Attempt;
                try{profile.Monetization=candidate;if(activeAttempt!=null)profile.Attempt=AttemptRecord.Capture(activeAttempt());store.Save(profile);}
                catch{profile.Monetization=oldLedger;profile.Attempt=oldAttempt;throw;}
            },profile.Clears.Count);
        }
    }
}
