using Cascade.Domain;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int heldToken = -1, previewNeighbour = -1;
        Vector3 holdOrigin, holdOffset, wantedOffset;
        float holdScale = 1;
        public bool IsHoldingToken => heldToken >= 0;

        public void BeginTokenHold(int slot, Vector2 screenPoint)
        {
            EndTokenHold();
            heldToken = slot; previewNeighbour = -1; Coach?.Activity();
            machine.LivingWorld.SetTokenHeld(true);machine.SetHeldWeight(slot);machine.SetPiecePose(slot,PiecePose.Held);
            if(tutorialHand!=null)tutorialHand.gameObject.SetActive(false);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(tokenRow, screenPoint, uiCamera, out holdOrigin);
            holdOffset = wantedOffset = Vector3.zero;
            var r = (RectTransform)slots[slot].transform;
            // Keep a narrow-row token legible above the finger; hit regions never move.
            float screenSpan = Vector2.Distance(RectTransformUtility.WorldToScreenPoint(uiCamera, r.TransformPoint(new Vector2(r.rect.xMin,0))),
                RectTransformUtility.WorldToScreenPoint(uiCamera, r.TransformPoint(new Vector2(r.rect.xMax,0))));
            holdScale = Mathf.Clamp(64 / Mathf.Max(1, screenSpan*.78f), 1.10f, 2.5f);
            wantedOffset = Vector3.up * machine.SelectionLiftWorld(slot) * 3.5f;
            feedback.Sound(ToyCue.Select); feedback.Haptic();
            PaintHeldToken(true);
        }
        public void MoveTokenHold(Vector2 screenPoint)
        {
            if (heldToken < 0) return;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(tokenRow, screenPoint, uiCamera, out var world);
            float spacing = slots.Count > 1 ? Mathf.Abs(machine.TokenPosition(1).x-machine.TokenPosition(0).x) : .5f;
            var delta = world-holdOrigin;
            wantedOffset = new Vector3(Mathf.Clamp(delta.x,-spacing*(Interaction.Attempt.Current.ReachCharges>0?3.15f:1.15f),spacing*(Interaction.Attempt.Current.ReachCharges>0?3.15f:1.15f)),
                Mathf.Clamp(delta.y,0,spacing*.45f)+machine.SelectionLiftWorld(heldToken)*3.5f, -.3f);
            PaintHeldToken(true);
        }
        void PaintHeldToken(bool immediate)
        {
            if (heldToken < 0 || Interaction.IsPresenting) return;
            holdOffset = immediate || ReducedMotion ? wantedOffset
                : Vector3.Lerp(holdOffset,wantedOffset,1-Mathf.Exp(-30*Time.unscaledDeltaTime));
            OffsetPresentedToken(heldToken,holdOffset,Vector2.one*holdScale);
        }
        public void EndTokenHold()
        {
            if (heldToken < 0) return;
            int old = heldToken; heldToken = previewNeighbour = -1;
            machine?.LivingWorld.SetTokenHeld(false);machine?.SetHeldWeight(-1);machine?.SetPiecePose(old,PiecePose.Resting);machine?.PreviewForecast(null);
            if (old < tokenLabels.Count && machine != null) OffsetPresentedToken(old,Vector3.zero,Vector2.one);
            for(int i=0;i<slotImages.Count;i++)slotImages[i].color=Color.clear;
        }
        void HighlightDragNeighbour(int from,int to)
        {
            int span=Mathf.Abs(from-to);
            bool adjacent = to >= 0 && (span==1 || Interaction.Attempt.Current.ReachCharges>0 && span>=2 && span<=3);
            if(previewNeighbour==to)return;
            previewNeighbour=to;
            for(int i=0;i<slotImages.Count;i++)slotImages[i].color=Color.clear;
            if(!adjacent)return;
            bool legal=Interaction.Attempt.Resolver.Preview(Interaction.Attempt.Current,span==1?PuzzleCommand.Swap(Mathf.Min(from,to)):PuzzleCommand.Reach(from,to)).IsLegal;
            slotImages[to].color=legal?new Color(.35f,.76f,.66f,.45f):new Color(.92f,.36f,.25f,.25f);
            if(legal)feedback.Sound(ToyCue.Select);
        }
    }
}
