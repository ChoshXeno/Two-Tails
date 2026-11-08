using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class PrototypeSafeArea : MonoBehaviour
    {
        private RectTransform container;
        private Rect lastArea;
        private Vector2Int lastSize;

        private void Awake()
        {
            container = GetComponent<RectTransform>();
        }

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
