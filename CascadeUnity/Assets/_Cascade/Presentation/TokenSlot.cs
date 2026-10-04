using UnityEngine.EventSystems;

namespace Cascade.Presentation
{
    public sealed class TokenSlot : UnityEngine.MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public CascadeGame Game { get; set; }
        public int Slot { get; set; }
        bool accepted, dragged, released;
        public void OnPointerDown(PointerEventData eventData)
        {
            accepted = Game.Interaction.PointerDown(eventData.pointerId, Slot); dragged = released = false;
            if (accepted) Game.PreviewSlot(Slot);
        }
        public void OnBeginDrag(PointerEventData eventData) { if (accepted) dragged = true; }
        public void OnDrag(PointerEventData eventData)
        {
            if (!accepted) return;
            var target = eventData.pointerCurrentRaycast.gameObject?.GetComponentInParent<TokenSlot>();
            Game.PreviewDrag(Slot, target != null && target.Game == Game ? target.Slot : -1);
        }
        void Release(PointerEventData eventData)
        {
            if (!accepted || released) return; released = true;
            var target = eventData.pointerCurrentRaycast.gameObject?.GetComponentInParent<TokenSlot>();
            int destination = target != null && target.Game == Game ? target.Slot : -1;
            Game.ReportInput(Game.Interaction.PointerUp(eventData.pointerId, destination, dragged));
        }
        public void OnPointerUp(PointerEventData eventData) => Release(eventData);
        public void OnEndDrag(PointerEventData eventData) => Release(eventData);
        void OnDisable() { if (accepted && !released) Game?.Interaction?.CancelPointer(); accepted = false; }
    }
}
