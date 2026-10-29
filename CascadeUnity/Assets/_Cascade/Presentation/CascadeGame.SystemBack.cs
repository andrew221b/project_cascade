using Cascade.Application;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        void PersistLifecycleProgress()
        {
#if UNITY_ANDROID
            // Snapshot on the game thread, atomic disk write on the existing worker.
            // Never wait for disk/fsync during an Android pause callback.
            PersistProgress();
#else
            PersistProgress(true);
#endif
        }
        void PollSystemBack()
        {
#if UNITY_ANDROID || UNITY_EDITOR
            bool committed=AndroidBackBridge.Consume();
            bool escape=Keyboard.current?.escapeKey.wasPressedThisFrame==true;
            if(committed||escape)HandleSystemBack();
#endif
        }
        public bool HandleSystemBack()
        {
            if(!flowReady||!CanPresent||!applicationFocused||applicationPaused||nativeAdSuspended||previewAd!=null)return false;
            // A transaction/native SDK window cannot be cancelled by navigation.
            if(purchaseStore?.State==PurchaseState.Busy)return true;
            // Deferred parental approval may take hours; navigation stays usable.
            if(legalOverlay!=null){CloseLegalDocument();return true;}
            if(accountDetailsOpen){accountDetailsOpen=false;accountNotice=default;DrawMenu();return true;}
            if(cloudSave?.CompletingSignIn==true||cloudSave?.NeedsAccountChoice==true)return true;
            if(cloudSave?.CanCancelSignIn==true){cloudSave.CancelSignIn();accountNotice=default;accountErrorAcknowledged=true;DrawMenu();return true;}
            if(!string.IsNullOrEmpty(accountNotice.Key)){accountNotice=default;accountErrorAcknowledged=true;DrawMenu();return true;}
            if(CurrentScreen==GameScreen.AccountGate&&!accountErrorAcknowledged&&!string.IsNullOrEmpty(cloudSave?.LastFailureCode)){accountErrorAcknowledged=true;DrawMenu();return true;}
            if(fieldTrayOpen){SetToolTray(false);return true;}
            if(MechanicPopupOpen){DismissMechanic();return true;}
            if(landingPreview!=null){CloseLandingPreview();return true;}
            if(zoomOpen){ToggleTargetZoom();return true;}
            if(hintVideoOffer!=null){CloseHintVideoOffer();return true;}
            if(ConversationOpen){CancelConversation();return true;}
            if(confirmAccountDeletion){confirmAccountDeletion=false;deletionNotice=default;DrawMenu();return true;}
            // Reuse authored navigation actions, including nested settings/shop
            // detail pages and their return locations. Never choose a purchase,
            // account-choice or reward button by caption.
            if(menuRoot!=null&&menuRoot.gameObject.activeInHierarchy)
                foreach(var button in menuRoot.GetComponentsInChildren<Button>())
                    if(button.name=="Back"&&button.isActiveAndEnabled&&button.interactable){button.onClick.Invoke();return true;}
            switch(CurrentScreen){
                case GameScreen.Playing: TogglePause();return true;
                case GameScreen.Pause: ShowGame();return true;
                case GameScreen.Home:
                case GameScreen.AccountGate:
                case GameScreen.SaveRecovery:
                    AndroidBackBridge.Background();return true;
                case GameScreen.Prologue: ShowHome();return true;
                case GameScreen.WorldComplete: WorldCompletionHome();return true;
                case GameScreen.Parcel: LeaveReferenceParcel();return true;
                default: ShowJourney();return true;
            }
        }
    }
}
