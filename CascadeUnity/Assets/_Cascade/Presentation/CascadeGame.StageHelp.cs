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
        RectTransform hintVideoOffer;
        void MarkLessonSeen(MechanicLesson lesson)
        {
            if(profile.SeenMechanics.Contains(lesson))return;
            profile.SeenMechanics.Add(lesson);PersistProgress(true);
            if(HasSaveError)profile.SeenMechanics.Remove(lesson);
        }
        void RefreshReplayLessons()
        {
            replayLessons.Clear();
            if(Interaction?.Attempt?.Current==null)return;
            var level=Interaction.Attempt.Level;int first=Interaction.Attempt.Current.ActiveFloor;
            if(first<0||first>=level.Floors.Count)return;
            var windFloors=Enumerable.Range(first,level.Floors.Count-first).Where(IsWindFloor);
            replayLessons.AddRange(MechanicLessons.Discover(level,first,windFloors).Where(lesson=>!IsSurpriseWindLesson(lesson)));
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
            MarkLessonSeen(CurrentMechanic.Value);CloseMechanicPopup();replayLessonIndex=(replayLessonIndex+1)%replayLessons.Count;
            lessonReplay=true;OpenMechanicPopup(replayLessons[replayLessonIndex]);
        }
        void LayoutPlayActions()
        {
            float p=CurrentScreen==GameScreen.Playing?Mathf.Clamp01(fieldTrayProgress):0;
            foreach(var group in fieldActionGroups)if(group!=null){
                group.alpha=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(p/.65f));
                group.interactable=group.blocksRaycasts=p<=.001f;
            }
            if(actions==null||undo==null||undo.transform.parent!=actions)return;
            float unit=ControlUnit, width=48*unit,gap=10*unit;
            void Put(Button button,float x,float span){
                var rect=(RectTransform)button.transform;rect.anchorMin=new Vector2(0,0);rect.anchorMax=new Vector2(0,1);
                rect.offsetMin=new Vector2(x,0);rect.offsetMax=new Vector2(x+span,0);
            }
            Put(undo,Mathf.Lerp(0,-2*(width+gap),p),width);
            Put(hint,Mathf.Lerp(width+gap,-width-gap,p),width);
            if(fieldTools!=null){
                Put(fieldTools,Mathf.Lerp(2*(width+gap),(actions.rect.width-width)*.5f,p),width);
                fieldTools.transform.localScale=Vector3.one*(1+.045f*Mathf.Sin(p*Mathf.PI));
            }
            float dropLeft=(HasFieldTools||p>0?3:2)*(width+gap);
            Put(drop,Mathf.Lerp(dropLeft,actions.rect.width+gap,p),Mathf.Max(width,actions.rect.width-dropLeft));
            if(hintMark==null)return;
            var lamp=(RectTransform)hintMark.transform;lamp.anchorMin=new Vector2(.16f,.19f);lamp.anchorMax=new Vector2(.84f,.91f);
        }
        void PaintHintCounter()
        {
            var label=hint.GetComponentInChildren<TextMeshProUGUI>();var attempt=Interaction.Attempt;
            bool videoDue=!attempt.GuideActive&&RewardedHintDue(attempt)&&GameEconomy.Ledger.HintTokens<=0;
            string count=videoDue?"▶":attempt.FreeHintsRemaining>0?attempt.FreeHintsRemaining.ToString():BonusHintAvailable?(GameEconomy.Ledger.HintTokens>0?"1":"+1"):"0";
            label.SetCopy(UiCopy.Raw(count));label.rectTransform.anchorMin=new Vector2(.59f,-.015f);label.rectTransform.anchorMax=new Vector2(1.03f,.33f);
            label.margin=Vector4.zero;label.textWrappingMode=TextWrappingModes.NoWrap;label.fontSize=12*ControlUnit;
            label.color=MenuPaper;
        }
        void OfferStageHintVideo()
        {
            if(!Interaction.Attempt.CanShowBonusHint||hintVideoOffer!=null)return;
            if(adProvider?.RewardReady!=true||TestMonetization||monetization.Ledger.HideVideoOffers||!RewardedOffersAllowed){
                reactionBanner.Show("Video unavailable. This hint is still saved; try again later.","");return;
            }
            Interaction.SetPaused(true);machine.SetAmbientPaused(true);
            hintVideoOffer=Rect("Optional hint video",safeRoot,Vector2.zero,Vector2.one);
            var canvas=hintVideoOffer.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=41;
            hintVideoOffer.gameObject.AddComponent<GraphicRaycaster>();Background(hintVideoOffer,new Color(.035f,.12f,.13f,.78f),true);
            var panel=Rect("Hint video panel",hintVideoOffer,new Vector2(.04f,.30f),new Vector2(.96f,.70f));
            var face=Background(panel,Color.white,false);face.sprite=enamelControls[0];face.type=Image.Type.Sliced;
            Text("Video explanation",panel,new Vector2(.08f,.52f),new Vector2(.92f,.89f),20,"Watch a video to reveal one move. You still make the move.").color=MenuPaper;
            Button("Watch video +1 hint",panel,new Vector2(.10f,.28f),new Vector2(.90f,.47f),()=>{
                CloseHintVideoOffer();if(!saveBlocked&&!HasSaveError&&Interaction.Attempt.CanShowBonusHint)
                    adProvider.OfferHint(AdRequest(AdSurface.Inventory));
            });
            Button("Keep playing",panel,new Vector2(.10f,.055f),new Vector2(.90f,.24f),CloseHintVideoOffer);
        }
        void CloseHintVideoOffer()
        {
            if(hintVideoOffer==null)return;
            hintVideoOffer.gameObject.SetActive(false);Destroy(hintVideoOffer.gameObject);hintVideoOffer=null;
            Interaction.SetPaused(CurrentScreen!=GameScreen.Playing);machine.SetAmbientPaused(CurrentScreen!=GameScreen.Playing);RenderControls();
        }
        public void ShowAdReward(AdTicket ticket)
        {
            if(ticket.RewardKind=="parcel_bonus"){if(CurrentScreen==GameScreen.Parcel)DrawMenu();ShowCoinReward(40);return;}
            if(ticket.RewardKind=="coins"){ShowCoinReward();return;}
            if(CurrentScreen==GameScreen.Playing){reactionBanner.Show("Extra hint ready. Tap the bulb.","");RenderControls();}
        }
    }
}
