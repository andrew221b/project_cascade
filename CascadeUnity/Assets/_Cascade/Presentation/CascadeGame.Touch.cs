using Cascade.Domain;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int heldToken = -1, returningToken = -1, previewNeighbour = -1;
        Vector3 holdOrigin, holdOffset, wantedOffset, releasePosition, releaseScale, grabOffset;
        bool draggingToken;
        public int ReleasedTokenSlot { get; private set; }=-1;
        public bool ReleasedDrag { get; private set; }
        public int ReleasedPartnerSlot {get;private set;}=-1;
        public Vector3 ReleasedPartnerPosition {get;private set;}
        float holdScale = 1, wantedScale = 1, holdAngle, wantedAngle, releaseAngle, returnTime;
        const float ReturnDuration = .18f;
        public bool IsHoldingToken => heldToken >= 0;
        public bool OwnsTokenPose(int slot) => heldToken == slot || returningToken == slot || feedback?.RowPhysics?.Shaking==true;

        public void BeginTokenHold(int slot, Vector2 screenPoint)
        {
            feedback.RowPhysics.CancelShake(true);
            EndTokenHold();
            heldToken = slot; previewNeighbour = -2; draggingToken=false;ReleasedTokenSlot=-1;Coach?.Activity();
            machine.LivingWorld.SetTokenHeld(true);machine.SetHeldWeight(slot);machine.SetPiecePose(slot,PiecePose.Held);
            if(tutorialHand!=null)tutorialHand.gameObject.SetActive(false);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(tokenRow, screenPoint, uiCamera, out holdOrigin);
            grabOffset=machine.TokenVisualPosition(slot)-holdOrigin;
            var r = (RectTransform)slots[slot].transform;
            // A visible acknowledgement now, followed by a short lift; the hit region stays still.
            float screenSpan = Vector2.Distance(RectTransformUtility.WorldToScreenPoint(uiCamera, r.TransformPoint(new Vector2(r.rect.xMin,0))),
                RectTransformUtility.WorldToScreenPoint(uiCamera, r.TransformPoint(new Vector2(r.rect.xMax,0))));
            wantedScale = Mathf.Clamp(58 / Mathf.Max(1, screenSpan*.78f), 1.08f, 1.3f);
            holdScale = ReducedMotion ? 1 : 1.015f; holdAngle = wantedAngle = 0;
            wantedOffset = Vector3.up * (ReducedMotion?0:machine.SelectionLiftWorld(slot)*1.6f);
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
            draggingToken=true;
            var position=world+grabOffset+Vector3.up*(ReducedMotion?0:machine.SelectionLiftWorld(heldToken)*1.6f);
            wantedOffset=position-machine.TokenPosition(heldToken)-machine.TokenSupportOffset(heldToken);
            wantedOffset.z=-.3f;
            wantedScale=Mathf.Max(wantedScale,1.16f);
            wantedAngle = Mathf.Clamp((wantedOffset.x-holdOffset.x)/Mathf.Max(.01f,spacing)*-12,-7,7);
            holdOffset=wantedOffset;
            RestoreTouchPose();
        }
        public void AdvanceTokenTouch(float seconds)
        {
            if(Interaction==null || Interaction.IsPresenting || Interaction.IsPaused || Interaction.IsSuspended)return;
            float dt=Mathf.Clamp(seconds,0,.05f);
            if(heldToken>=0) {
                holdOffset.x=draggingToken?wantedOffset.x:Mathf.Lerp(holdOffset.x,wantedOffset.x,1-Mathf.Exp(-60*dt));
                holdOffset.y=draggingToken?wantedOffset.y:Mathf.Lerp(holdOffset.y,wantedOffset.y,1-Mathf.Exp(-30*dt));
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
                bool selected=Interaction.SelectedSlot==old;
                OffsetPresentedToken(old,Vector3.up*(selected?machine.SelectionLiftWorld(old):0),Vector2.one*(selected?1.04f:1));
                CompleteTouchExchange();
            }
        }
        void RestoreTouchPose()
        {
            if(machine==null || Interaction==null || Interaction.IsPresenting)return;
            if(heldToken>=0) {
                // Quiet motion still follows deliberate finger movement, without lift/zoom/tilt.
                var offset=ReducedMotion?new Vector3(wantedOffset.x,wantedOffset.y,-.3f):holdOffset;
                OffsetPresentedToken(heldToken,offset,Vector2.one*(ReducedMotion?1:holdScale));
                machine.SetPiecePose(heldToken,PiecePose.Held,ReducedMotion?0:holdAngle);
            }
            if(returningToken>=0) {
                float t=ReducedMotion?1:1-Mathf.Pow(1-returnTime/ReturnDuration,3);
                bool selected=Interaction.SelectedSlot==returningToken;
                var target=machine.TokenPosition(returningToken)+machine.TokenSupportOffset(returningToken)+Vector3.up*(selected?machine.SelectionLiftWorld(returningToken):0);
                var position=Vector3.Lerp(releasePosition,target,t);
                var size=Vector3.Lerp(releaseScale,machine.TokenBaseScale(returningToken)*(selected?1.04f:1),t);
                var basis=machine.TokenBaseScale(returningToken);
                OffsetPresentedToken(returningToken,position-machine.TokenPosition(returningToken)-machine.TokenSupportOffset(returningToken),new Vector2(size.x/basis.x,size.y/basis.y));
                machine.SetPiecePose(returningToken,PiecePose.Settling,Mathf.Lerp(releaseAngle,0,t));
            }
            SyncTokenFaces();
        }
        // Save the actual pose before PointerUp can synchronously start a committed exchange.
        public void ReleaseTokenHold()
        {
            if(heldToken<0)return;
            returningToken=heldToken;returnTime=0;
            ReleasedTokenSlot=heldToken;
            ReleasedDrag=draggingToken;
            ReleasedPartnerSlot=draggingToken&&previewNeighbour>=0&&previewNeighbour<slots.Count&&previewNeighbour!=heldToken?previewNeighbour:-1;
            if(ReleasedPartnerSlot>=0)ReleasedPartnerPosition=machine.TokenVisualPosition(ReleasedPartnerSlot);
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
            machine?.SetDragDestination(-1);
            for(int i=0;i<slotImages.Count;i++)slotImages[i].color=Color.clear;
        }
        // Lifecycle cancellation settles immediately; ordinary releases use the short return above.
        public void EndTokenHold()
        {
            int old=heldToken>=0?heldToken:returningToken;
            returningToken=-1;ClearHeldToken();
            CompleteTouchExchange();
            if(old>=0 && old<tokenLabels.Count && machine!=null) {
                machine.SetPiecePose(old,PiecePose.Resting);machine.PreviewForecast(null);
                OffsetPresentedToken(old,Vector3.zero,Vector2.one);
            }
        }
        void TransferTouchPoseToExchange() { feedback.RowPhysics.CancelShake(false);returningToken=-1; }
        public void CompleteTouchExchange(){ReleasedTokenSlot=ReleasedPartnerSlot=-1;ReleasedDrag=false;}
        void HighlightDragNeighbour(int from,int to)
        {
            int span=Mathf.Abs(from-to);
            bool adjacent = to >= 0 && (span==1 || Interaction.Attempt.Current.ReachCharges>0 && span>=2 && span<=3);
            previewNeighbour=to;
            for(int i=0;i<slotImages.Count;i++)slotImages[i].color=Color.clear;
            if(!adjacent){machine.SetDragDestination(-1);return;}
            bool legal=Interaction.Attempt.Resolver.Preview(Interaction.Attempt.Current,span==1?PuzzleCommand.Swap(Mathf.Min(from,to)):PuzzleCommand.Reach(from,to)).IsLegal;
            machine.SetDragDestination(to,!legal);
        }
    }
}
