using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

namespace TwoTails.Prototype
{
    // Keep ownership with the pointer that began the drag, even if another finger lands on the stick.
    public sealed class PrototypeScreenStick : OnScreenStick, IPointerDownHandler, IDragHandler,
        IPointerUpHandler, IInitializePotentialDragHandler
    {
        private int? pointerId;

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (pointerId.HasValue || eventData.button != PointerEventData.InputButton.Left)
                return;

            pointerId = eventData.pointerId;
            base.OnPointerDown(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (pointerId == eventData.pointerId)
                base.OnDrag(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (pointerId == eventData.pointerId)
                Release();
        }

        protected override void OnDisable()
        {
            Release();
            base.OnDisable();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
                Release();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                Release();
        }

        private void Release()
        {
            if (!pointerId.HasValue)
                return;

            pointerId = null;
            base.OnPointerUp(null);
        }
    }
}
