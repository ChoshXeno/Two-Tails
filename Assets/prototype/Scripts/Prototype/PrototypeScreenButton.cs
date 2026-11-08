using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

namespace TwoTails.Prototype
{
    public sealed class PrototypeScreenButton : OnScreenButton, IPointerDownHandler, IPointerUpHandler
    {
        private int? pointerId;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (pointerId.HasValue || eventData.button != PointerEventData.InputButton.Left)
                return;

            pointerId = eventData.pointerId;
            base.OnPointerDown(eventData);
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
