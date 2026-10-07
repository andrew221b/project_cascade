using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Cascade.Presentation
{
    public sealed class TokenSlot : UnityEngine.MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public CascadeGame Game { get; set; }
        public int Slot { get; set; }
        bool accepted, dragged, released;
        int capturedPointer;
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (accepted && !released && Game.Interaction.OwnsPointer(capturedPointer)) return;
            accepted = Game.Interaction.PointerDown(eventData.pointerId, Slot); dragged = released = false;
            if (accepted) { capturedPointer = eventData.pointerId; Game.PreviewSlot(Slot); Game.BeginTokenHold(Slot, eventData.position); }
        }
        public void OnBeginDrag(PointerEventData eventData) { if (Owns(eventData)) dragged = true; }
        public void OnDrag(PointerEventData eventData)
        {
            if (!Owns(eventData)) return;
            var target = eventData.pointerCurrentRaycast.gameObject?.GetComponentInParent<TokenSlot>();
            Game.PreviewDrag(Slot, Game.TokenAtScreenPoint(eventData.position,target != null && target.Game == Game ? target.Slot : -1,Slot));
            Game.MoveTokenHold(eventData.position);
        }
        void Release(PointerEventData eventData)
        {
            if (!Owns(eventData)) return; released = true; Game.ReleaseTokenHold();
            if (WasCancelled(eventData)) { Game.Interaction.CancelPointer(); Game.ReportInput(null); return; }
            var target = eventData.pointerCurrentRaycast.gameObject?.GetComponentInParent<TokenSlot>();
            int destination = Game.TokenAtScreenPoint(eventData.position,target != null && target.Game == Game ? target.Slot : -1,dragged?Slot:-1);
            Game.ReportInput(Game.Interaction.PointerUp(eventData.pointerId, destination, dragged));
        }
        public void OnPointerUp(PointerEventData eventData) => Release(eventData);
        public void OnEndDrag(PointerEventData eventData) => Release(eventData);
        bool Owns(PointerEventData data) => accepted && !released && data.pointerId == capturedPointer
            && data.button == PointerEventData.InputButton.Left && Game.Interaction.OwnsPointer(capturedPointer);
        static bool WasCancelled(PointerEventData data)
        {
            if (!(data is ExtendedPointerEventData extended) || !(extended.device is Touchscreen screen)) return false;
            foreach (var touch in screen.touches)
                if (touch.touchId.ReadValue() == extended.touchId && touch.phase.ReadValue() == TouchPhase.Canceled) return true;
            return false;
        }
        void OnDisable()
        {
            if (accepted && !released && Game?.Interaction?.OwnsPointer(capturedPointer) == true) { Game.EndTokenHold(); Game.Interaction.CancelPointer(); }
            accepted = false;
        }
    }
}
