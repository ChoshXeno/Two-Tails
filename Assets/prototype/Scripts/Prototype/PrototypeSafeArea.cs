using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class PrototypeSafeArea : MonoBehaviour
    {
        [SerializeField, Min(15), Tooltip("Frame rate target on mobile devices only.")]
        private int mobileFrameRate = 30;
        private RectTransform container;
        private Rect lastArea;
        private Vector2Int lastSize;

        private void Awake()
        {
            container = GetComponent<RectTransform>();
            if (Application.isMobilePlatform)
                Application.targetFrameRate = mobileFrameRate;
        }

        private void ClearTouchControls()
        {
            // Reset physical touches first so isolated OnScreenStick receives cancellation.
            if (Touchscreen.current != null)
                InputSystem.ResetDevice(Touchscreen.current);
            foreach (var stick in GetComponentsInChildren<OnScreenStick>(true))
                if (stick.isActiveAndEnabled && !stick.useIsolatedInputActions)
                    stick.OnPointerUp(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
            foreach (var control in GetComponentsInChildren<OnScreenControl>(true))
                if (control.control != null)
                    InputSystem.ResetDevice(control.control.device);
        }

        private void OnApplicationPause(bool paused) { if (paused) ClearTouchControls(); }
        private void OnApplicationFocus(bool focused) { if (!focused) ClearTouchControls(); }

        private void Update()
        {
            Rect area = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            if (size.x <= 0 || size.y <= 0 || (area == lastArea && size == lastSize))
                return;

            var pixels = new Vector2(size.x, size.y);
            container.anchorMin = area.min / pixels;
            container.anchorMax = area.max / pixels;
            container.offsetMin = Vector2.zero;
            container.offsetMax = Vector2.zero;
            lastArea = area;
            lastSize = size;
        }
    }
}
