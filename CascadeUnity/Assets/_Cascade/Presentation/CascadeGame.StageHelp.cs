using System.Collections.Generic;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        bool LessonWasSeen(MechanicLesson lesson)=>profile.SeenMechanics.Contains(lesson)
            ||profile.ModeJourneys.Any(j=>j.SeenMechanics.Contains(lesson));
        bool BonusHintAvailable=>Interaction.Attempt.CanShowBonusHint
            &&(HasHintForCurrentState(Interaction.Attempt)||GameEconomy?.Ledger.HintTokens>0
                ||adProvider?.RewardReady==true&&!TestMonetization&&!monetization.Ledger.HideVideoOffers);
        readonly UnavailableHintAllowance unavailableHints=new UnavailableHintAllowance();
        System.Guid unavailableHintAttempt;string unavailableHintState;
        bool HintVideoUnavailable=>!TestMonetization&&adProvider!=null&&!adProvider.Showing
            &&(monetization.Ledger.HideVideoOffers||!monetization.CanReceiveGrant||!RewardedOffersAllowed
                ||!adProvider.RewardReady&&(!adProvider.RewardLoading||unavailableHints.LoadingWaitExpired(Interaction.Attempt)));
        void TickUnavailableHintWait()
        {
            if(adProvider?.RewardReady==true){unavailableHints.ClearVideoWait();return;}
            if(CurrentScreen==GameScreen.Playing&&adProvider?.RewardLoading==true
                &&unavailableHints.TakeLoadingWaitExpiry(Interaction.Attempt))RenderControls();
        }
        bool TryUnavailableHint(AttemptService attempt)
        {
            adProvider.ReportUnavailableHint();
            int seconds=(int)System.Math.Ceiling(unavailableHints.Remaining(attempt));
            if(seconds<=0)return true;
            reactionBanner.Show(ReferenceCopy("Hint in "+seconds+"s","Подсказка через "+seconds+" с","Підказка через "+seconds+" с"),"");
            return false;
        }
        int GrantedHintStates(AttemptService attempt)
        {
            string prefix=attempt.AttemptId.ToString("D")+"|";
            var paid=GameEconomy?.Ledger.PaidHintStates;
            return attempt.FreeHintStates.Count+(paid?.Count(state=>state.StartsWith(prefix,System.StringComparison.Ordinal))??0);
        }
        bool HasHintForCurrentState(AttemptService attempt)
        {
            if(attempt.FreeHintStates.Contains(attempt.CurrentStateHash)||attempt.BonusHintStateHash==attempt.CurrentStateHash)return true;
            return GameEconomy?.Ledger.PaidHintStates.Contains(attempt.AttemptId.ToString("D")+"|"+attempt.CurrentStateHash)==true;
        }
        bool RewardedHintDue(AttemptService attempt)=>StageHintBudget.RewardedHintDue(
            GrantedHintStates(attempt),attempt.FreeHintsRemaining,HasHintForCurrentState(attempt));
        float ControlUnit=>1/Mathf.Min(1,actions.GetComponentInParent<Canvas>().scaleFactor);
        bool lessonReplay;
        readonly List<MechanicLesson> replayLessons=new List<MechanicLesson>();
        int replayLessonIndex;
        RectTransform hintVideoOffer;VideoHintMark hintVideoMark;System.Guid pendingHintVideoAttempt;string pendingHintVideoState;
        void MarkLessonSeen(MechanicLesson lesson)
        {
            bool added=!profile.SeenMechanics.Contains(lesson);
            string film=MechanicFilmMarker(lesson);bool filmAdded=!profile.SeenCoachPrompts.Contains(film);
            if(!added&&!filmAdded)return;
            if(added)profile.SeenMechanics.Add(lesson);if(filmAdded)profile.SeenCoachPrompts.Add(film);
            PersistProgress(true);
            if(HasSaveError){if(added)profile.SeenMechanics.Remove(lesson);if(filmAdded)profile.SeenCoachPrompts.Remove(film);}
        }
        void RefreshReplayLessons()
        {
            if(MechanicPopupOpen||changingTutorialPage)return;
            replayLessons.Clear();
            if(Interaction?.Attempt?.Current==null)return;
            var level=Interaction.Attempt.Level;int first=Interaction.Attempt.Current.ActiveFloor;
            if(first<0||first>=level.Floors.Count)return;
            var windFloors=Enumerable.Range(first,level.Floors.Count-first).Where(IsWindFloor);
            // The question mark is the player's manual, including on a plain board.
            // Automatic first-use discovery remains limited to unseen, visible mechanics.
            replayLessons.AddRange(MechanicLessons.Discover(level,first,windFloors));
            if(Interaction.Attempt.Current.ReachCharges>0&&!replayLessons.Contains(MechanicLesson.FarSwap))replayLessons.Add(MechanicLesson.FarSwap);
            replayLessons.AddRange(new[]{MechanicLesson.Swap,MechanicLesson.Descent,MechanicLesson.Undo,MechanicLesson.Hint,MechanicLesson.Toolbag});
        }
        public void DismissMechanic()
        {
            if(!MechanicPopupOpen||!IntroductionAnimationAllowed||saveBlocked)return;
            var lesson=CurrentMechanic.Value;bool replay=lessonReplay;CloseMechanicPopup();
            MarkLessonSeen(lesson);
            ClearLessonPractice();needsMechanicScan=true;
        }
        public void ReplayStageLessons()
        {
            if(CurrentScreen!=GameScreen.Playing||MechanicPopupOpen||!Interaction.CanControl||Interaction.IsPresenting)return;
            if(PendingLesson.HasValue){MarkLessonSeen(PendingLesson.Value);ClearLessonPractice();}
            RefreshReplayLessons();
            if(replayLessons.Count==0){help.gameObject.SetActive(false);return;}
            replayLessonIndex=0;lessonReplay=true;OpenMechanicPopup(replayLessons[0]);
        }
        void NextReplayLesson()
        {
            if(!lessonReplay||replayLessons.Count==0)return;
            SelectTutorialPage((replayLessonIndex+1)%replayLessons.Count);
        }
        void LayoutPlayActions()
        {
            if(actions==null||undo==null||undo.transform.parent!=actions)return;
            actions.anchorMin=new Vector2(.01f,actions.anchorMin.y);actions.anchorMax=new Vector2(.99f,actions.anchorMax.y);
            float unit=ControlUnit;float design=Mathf.Min(safeRoot.rect.width/720f,safeRoot.rect.height/1280f);float width=96*design,gap=8*design;
            void Put(Button b,float x,float span){var r=(RectTransform)b.transform;r.anchorMin=new Vector2(0,0);r.anchorMax=new Vector2(0,1);r.offsetMin=new Vector2(x,0);r.offsetMax=new Vector2(x+span,0);}
            Put(undo,0,width);if(fieldTools!=null)Put(fieldTools,actions.rect.width-width,width);Put(drop,width+gap,Mathf.Max(48*unit,actions.rect.width-2*(width+gap)));
            var action=(RectTransform)drop.transform;action.offsetMin+=Vector2.up*(4*unit);action.offsetMax-=Vector2.up*(4*unit);
            FitGameplayToolIcon(undoMeaning);FitGameplayToolIcon(fieldToolsMeaning);
            SeatGameplayToolFlowers(undo);SeatGameplayToolFlowers(fieldTools);
            if(hintMark!=null&&hint.GetComponent<GameButtonDesign>()?.Bound!=true){var lamp=(RectTransform)hintMark.transform;lamp.anchorMin=new Vector2(.03f,.03f);lamp.anchorMax=new Vector2(.97f,.97f);}
        }
        void PaintHintCounter()
        {
            var label=hint.GetComponentInChildren<TextMeshProUGUI>();var attempt=Interaction.Attempt;
            // Layout can render a saved hint before the economy is constructed in Awake.
            int hintTokens=GameEconomy?.Ledger.HintTokens??profile.Monetization.HintTokens;
            // Availability changes the request outcome, not the meaning of the offer.
            bool videoDue=!attempt.GuideActive&&RewardedHintDue(attempt)&&hintTokens<=0;
            string count=videoDue?"":hintTokens>0&&RewardedHintDue(attempt)?"1":"";
            if(hintVideoMark==null){var r=Rect("Watch video hint icon",hint.transform,new Vector2(.06f,.10f),new Vector2(.94f,.90f));hintVideoMark=r.gameObject.AddComponent<VideoHintMark>();hintVideoMark.raycastTarget=false;hintVideoMark.color=new Color(.20f,.055f,.014f);}
            hintVideoMark.enabled=videoDue;if(hintMark!=null)hintMark.enabled=!videoDue;
            if(hint.GetComponent<GameButtonDesign>() is GameButtonDesign design&&design.Bound)design.UseGlyph(videoDue?(Graphic)hintVideoMark:hintMark);
            label.SetCopy(UiCopy.Raw(count));label.rectTransform.anchorMin=new Vector2(.59f,-.015f);label.rectTransform.anchorMax=new Vector2(1.03f,.33f);
            label.margin=Vector4.zero;label.textWrappingMode=TextWrappingModes.NoWrap;label.fontSize=12*ControlUnit;
            label.color=MenuPaper;
        }
        void OfferStageHintVideo()
        {
            if(!Interaction.Attempt.CanShowBonusHint||hintVideoOffer!=null)return;
            if(adProvider?.RewardReady!=true||TestMonetization||monetization.Ledger.HideVideoOffers||!RewardedOffersAllowed){
                reactionBanner.Show(adProvider?.RewardLoading==true?(UiCopy)"Loading video…":"Video unavailable. Try again later.","");return;
            }
            pendingHintVideoAttempt=Interaction.Attempt.AttemptId;pendingHintVideoState=Interaction.Attempt.CurrentStateHash;
            if(!adProvider.OfferHint(AdRequest(AdSurface.Inventory)))pendingHintVideoState=null;
        }
        System.Collections.IEnumerator RevealHintAfterVideo(System.Guid attempt,string state)
        {
            float deadline=Time.realtimeSinceStartup+8;
            while(!Interaction.CanPlay&&Time.realtimeSinceStartup<deadline)yield return null;
            if(CurrentScreen==GameScreen.Playing&&Interaction.Attempt.AttemptId==attempt&&Interaction.Attempt.CurrentStateHash==state&&Interaction.CanPlay)RequestHint();
        }
        void CloseHintVideoOffer()
        {
            if(hintVideoOffer==null)return;
            hintVideoOffer.gameObject.SetActive(false);Destroy(hintVideoOffer.gameObject);hintVideoOffer=null;
            Interaction.SetPaused(CurrentScreen!=GameScreen.Playing);machine.SetAmbientPaused(CurrentScreen!=GameScreen.Playing);RenderControls();
        }
        public void ShowAdReward(AdTicket ticket)
        {
            if(ticket.RewardKind=="parcel_open"){ParcelOpenConfirmed(ticket);return;}
            if(ticket.RewardKind=="parcel_bonus"){ParcelBonusConfirmed(ticket);return;}
            if(ticket.RewardKind=="coins"){NotifyStoreChanged();ShowCoinReward();return;}
            if(ticket.RewardKind=="hint"&&pendingHintVideoState!=null){var id=pendingHintVideoAttempt;var state=pendingHintVideoState;pendingHintVideoState=null;StartCoroutine(RevealHintAfterVideo(id,state));}
            else if(CurrentScreen==GameScreen.Playing){reactionBanner.Show("Extra hint ready. Tap the bulb.","");RenderControls();}
        }
    }
}
