using Cascade.Domain;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int heldToken = -1, returningToken = -1, previewNeighbour = -1;
        Vector3 holdOrigin, holdOffset, wantedOffset, releasePosition, releaseScale;
        float holdScale = 1, wantedScale = 1, holdAngle, wantedAngle, releaseAngle, returnTime;
        const float ReturnDuration = .18f;
        public bool IsHoldingToken => heldToken >= 0;
        public bool OwnsTokenPose(int slot) => heldToken == slot || returningToken == slot;

        public void BeginTokenHold(int slot, Vector2 screenPoint)
        {
            EndTokenHold();
            heldToken = slot; previewNeighbour = -2; Coach?.Activity();
            machine.LivingWorld.SetTokenHeld(true);machine.SetHeldWeight(slot);machine.SetPiecePose(slot,PiecePose.Held);
            if(tutorialHand!=null)tutorialHand.gameObject.SetActive(false);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(tokenRow, screenPoint, uiCamera, out holdOrigin);
            var r = (RectTransform)slots[slot].transform;
            // A visible acknowledgement now, followed by a short lift; the hit region stays still.
            float screenSpan = Vector2.Distance(RectTransformUtility.WorldToScreenPoint(uiCamera, r.TransformPoint(new Vector2(r.rect.xMin,0))),
                RectTransformUtility.WorldToScreenPoint(uiCamera, r.TransformPoint(new Vector2(r.rect.xMax,0))));
            wantedScale = Mathf.Clamp(58 / Mathf.Max(1, screenSpan*.78f), 1.10f, 1.55f);
            holdScale = ReducedMotion ? 1 : 1.015f; holdAngle = wantedAngle = 0;
            wantedOffset = Vector3.up * machine.SelectionLiftWorld(slot) * 3;
            wantedOffset.z = -.3f;
            holdOffset = ReducedMotion ? wantedOffset : Vector3.up * machine.SelectionLiftWorld(slot) * .25f;
            feedback.Sound(ToyCue.Select); feedback.Haptic();
            RestoreTouchPose();
        }
        public void MoveTokenHold(Vector2 screenPoint)
        {
            if (heldToken < 0) return;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(tokenRow, screenPoint, uiCamera, out var world);
            float spacing = slots.Count > 1 ? Mathf.Abs(machine.TokenPosition(1).x-machine.TokenPosition(0).x) : .5f;
            var delta = world-holdOrigin;
            float reach = Interaction.Attempt.Current.ReachCharges>0 ? 3.15f : 1.15f;
            wantedOffset = new Vector3(Mathf.Clamp(delta.x,-spacing*reach,spacing*reach),
                Mathf.Clamp(delta.y,0,spacing*.45f)+machine.SelectionLiftWorld(heldToken)*3, -.3f);
            wantedAngle = Mathf.Clamp((wantedOffset.x-holdOffset.x)/Mathf.Max(.01f,spacing)*-12,-7,7);
            RestoreTouchPose();
        }
        public void AdvanceTokenTouch(float seconds)
        {
            if(Interaction==null || Interaction.IsPresenting || Interaction.IsPaused || Interaction.IsSuspended)return;
            float dt=Mathf.Clamp(seconds,0,.05f);
            if(heldToken>=0) {
                holdOffset.x=Mathf.Lerp(holdOffset.x,wantedOffset.x,1-Mathf.Exp(-60*dt));
                holdOffset.y=Mathf.Lerp(holdOffset.y,wantedOffset.y,1-Mathf.Exp(-30*dt));
                holdOffset.z=wantedOffset.z;
                holdScale=Mathf.Lerp(holdScale,wantedScale,1-Mathf.Exp(-30*dt));
                holdAngle=Mathf.Lerp(holdAngle,wantedAngle,1-Mathf.Exp(-24*dt));
                wantedAngle*=Mathf.Exp(-18*dt);
            }
            if(returningToken>=0) {
                returnTime=ReducedMotion?ReturnDuration:Mathf.Min(ReturnDuration,returnTime+dt);
            }
            RestoreTouchPose();
            if(returningToken>=0 && returnTime>=ReturnDuration) {
                int old=returningToken;returningToken=-1;
                machine.SetPiecePose(old,PiecePose.Resting);
                OffsetPresentedToken(old,Vector3.zero,Vector2.one);
            }
        }
        void RestoreTouchPose()
        {
            if(machine==null || Interaction==null || Interaction.IsPresenting)return;
            if(heldToken>=0) {
                // Quiet motion still follows deliberate finger movement, without lift/zoom/tilt.
                var offset=ReducedMotion?new Vector3(wantedOffset.x,0,-.3f):holdOffset;
                OffsetPresentedToken(heldToken,offset,Vector2.one*(ReducedMotion?1:holdScale));
                machine.SetPiecePose(heldToken,PiecePose.Held,ReducedMotion?0:holdAngle);
            }
            if(returningToken>=0) {
                float t=ReducedMotion?1:1-Mathf.Pow(1-returnTime/ReturnDuration,3);
                var target=machine.TokenPosition(returningToken)+machine.TokenSupportOffset(returningToken);
                var position=Vector3.Lerp(releasePosition,target,t);
                var size=Vector3.Lerp(releaseScale,machine.TokenBaseScale(returningToken),t);
                var basis=machine.TokenBaseScale(returningToken);
                OffsetPresentedToken(returningToken,position-target,new Vector2(size.x/basis.x,size.y/basis.y));
                machine.SetPiecePose(returningToken,PiecePose.Settling,Mathf.Lerp(releaseAngle,0,t));
            }
            SyncTokenFaces();
        }
        // Save the actual pose before PointerUp can synchronously start a committed exchange.
        public void ReleaseTokenHold()
        {
            if(heldToken<0)return;
            returningToken=heldToken;returnTime=0;
            releasePosition=machine.TokenVisualPosition(heldToken);
            releaseScale=machine.TokenVisualScale(heldToken);
            releaseAngle=holdAngle;
            ClearHeldToken();
            machine.PreviewForecast(null);
            RestoreTouchPose();
        }
        void ClearHeldToken()
        {
            heldToken=previewNeighbour=-1;
            machine?.LivingWorld.SetTokenHeld(false);machine?.SetHeldWeight(-1);
            for(int i=0;i<slotImages.Count;i++)slotImages[i].color=Color.clear;
        }
        // Lifecycle cancellation settles immediately; ordinary releases use the short return above.
        public void EndTokenHold()
        {
            int old=heldToken>=0?heldToken:returningToken;
            returningToken=-1;ClearHeldToken();
            if(old>=0 && old<tokenLabels.Count && machine!=null) {
                machine.SetPiecePose(old,PiecePose.Resting);machine.PreviewForecast(null);
                OffsetPresentedToken(old,Vector3.zero,Vector2.one);
            }
        }
        void TransferTouchPoseToExchange() { returningToken=-1; }
        void HighlightDragNeighbour(int from,int to)
        {
            int span=Mathf.Abs(from-to);
            bool adjacent = to >= 0 && (span==1 || Interaction.Attempt.Current.ReachCharges>0 && span>=2 && span<=3);
            previewNeighbour=to;
            for(int i=0;i<slotImages.Count;i++)slotImages[i].color=Color.clear;
            if(!adjacent)return;
            bool legal=Interaction.Attempt.Resolver.Preview(Interaction.Attempt.Current,span==1?PuzzleCommand.Swap(Mathf.Min(from,to)):PuzzleCommand.Reach(from,to)).IsLegal;
            slotImages[to].color=legal?new Color(.95f,.73f,.29f,.22f):new Color(.92f,.36f,.25f,.20f);
            if(legal)feedback.Sound(ToyCue.Select);
        }
    }
}
