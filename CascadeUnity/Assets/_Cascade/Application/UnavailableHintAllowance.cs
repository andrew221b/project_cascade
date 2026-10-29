using System;
using System.Linq;

namespace Cascade.Application
{
    // Session-only help when an optional video cannot be supplied. This grants
    // information, never currency or an advertising receipt.
    public sealed class UnavailableHintAllowance
    {
        public const double CooldownSeconds=30;
        Guid attemptId;
        double? grantedAt,loadingRequestedAt;
        double legacyActiveSeconds;
        bool loadingAppearanceReported;
        void Bind(AttemptService attempt)
        {
            if(attemptId==attempt.AttemptId)return;
            attemptId=attempt.AttemptId;grantedAt=loadingRequestedAt=null;loadingAppearanceReported=false;legacyActiveSeconds=0;
        }
        double ActiveTime(AttemptService attempt)=>attempt.ActiveSeconds??legacyActiveSeconds;
        public void Advance(AttemptService attempt,double seconds)
        {
            if(attempt==null||double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
            Bind(attempt);
            // Legacy statistics stay unknown; only this session's help clock advances.
            if(!attempt.ActiveSeconds.HasValue)legacyActiveSeconds=Math.Min(604800,legacyActiveSeconds+seconds);
        }
        public void WaitForVideo(AttemptService attempt)
        {
            Bind(attempt);
            if(!loadingRequestedAt.HasValue){loadingRequestedAt=ActiveTime(attempt);loadingAppearanceReported=false;}
        }
        public void ClearVideoWait(){loadingRequestedAt=null;loadingAppearanceReported=false;}
        public bool LoadingWaitExpired(AttemptService attempt)
        {
            if(attempt==null)return false;
            Bind(attempt);
            return loadingRequestedAt.HasValue&&ActiveTime(attempt)-loadingRequestedAt.Value>=12;
        }
        public bool TakeLoadingWaitExpiry(AttemptService attempt)
        {
            if(!LoadingWaitExpired(attempt)||loadingAppearanceReported)return false;
            loadingAppearanceReported=true;return true;
        }
        public double Remaining(AttemptService attempt)
        {
            if(attempt==null)return CooldownSeconds;
            Bind(attempt);
            return grantedAt.HasValue?Math.Max(0,CooldownSeconds-Math.Max(0,ActiveTime(attempt)-grantedAt.Value)):0;
        }
        public bool TryReveal(AttemptService attempt,HintOffer offer,Func<bool> commit)
        {
            if(attempt==null||offer==null||commit==null||!attempt.IsCurrent(offer)
                ||offer.Result.Status!=Cascade.Solver.HintStatus.Step||!attempt.CanShowFreeHint)return false;
            Bind(attempt);
            bool already=attempt.FreeHintStates.Contains(offer.StateHash);
            if(!already&&Remaining(attempt)>0)return false;
            if(!attempt.TryRevealFreeHint(offer,commit))return false;
            if(!already)grantedAt=ActiveTime(attempt);
            loadingRequestedAt=null;loadingAppearanceReported=false;
            return true;
        }
    }
}
