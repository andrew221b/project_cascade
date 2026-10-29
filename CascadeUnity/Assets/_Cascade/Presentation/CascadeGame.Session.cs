using System;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        static readonly TimeSpan SessionAbsenceLimit=TimeSpan.FromMinutes(10);
        DateTime? sessionAbsentUtc;
        bool sessionResetPending;
        bool SessionSystemFlow=>nativeAdSuspended||purchaseStore?.State==PurchaseState.Busy
            ||purchaseStore?.State==PurchaseState.Deferred||cloudSave?.CanCancelSignIn==true||cloudSave?.CompletingSignIn==true;
        void ObserveSessionPresence(DateTime now)
        {
            if(!applicationFocused||applicationPaused){
                if(!sessionAbsentUtc.HasValue&&!SessionSystemFlow)sessionAbsentUtc=now;
                return;
            }
            if(sessionAbsentUtc.HasValue&&now-sessionAbsentUtc.Value>=SessionAbsenceLimit)sessionResetPending=true;
            sessionAbsentUtc=null;
            TryResetInactiveSession();
        }
        void TryResetInactiveSession()
        {
            if(!sessionResetPending||!applicationFocused||applicationPaused||!flowReady||SessionSystemFlow
                ||saveBlocked||HasSaveError||cloudSave?.AccountOperationPending==true)return;
            sessionResetPending=false;
            ResetSessionRound();returnScreen=GameScreen.Home;
            if(NeedsAccountIntro())ShowAccountGate();else ShowHome();
            PersistProgress(true);
        }
        void ResetSessionRound()
        {
            StopPresentation();CancelHint();CancelDeadEnd();CancelMissionPlan();
            SettleDescent();CancelDirectionWork();CancelDescentWork();
            profile.DiscardSessionAttempts();journeyActive=false;storyRoute=false;pendingJourney=null;
            bool ready=flowReady;flowReady=false;
            try{LoadNode(initialNode);}finally{flowReady=ready;}
        }
    }
}
