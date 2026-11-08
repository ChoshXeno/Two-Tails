using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace TwoTails.Prototype
{
    // The background receives a pointer only when no foreground UI claims its press.
    [DefaultExecutionOrder(10)]
    public sealed class PrototypeOrbitDrag : MonoBehaviour, IPointerDownHandler, IDragHandler,
        IPointerUpHandler, IInitializePotentialDragHandler, ICancelHandler, IScrollHandler
    {
        [SerializeField, Tooltip("P1 camera that receives screen-space drag deltas.")] private PrototypeFollowCamera orbit;
        private int? mousePointer;
        private readonly List<(int Id, Vector2 Position)> touches = new();
        private int? previousFirst;
        private int? previousSecond;
        private Vector2 previousPosition;
        private float previousSpread;

        public void OnInitializePotentialDrag(PointerEventData data) => data.useDragThreshold = false;

        public void OnPointerDown(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left)
                return;

            if (data is ExtendedPointerEventData extended &&
                extended.pointerType != UIPointerType.Touch && extended.pointerType != UIPointerType.MouseOrPen)
                return;

            if (data is ExtendedPointerEventData pointer && pointer.pointerType == UIPointerType.Touch)
            {
                if (FindTouch(data.pointerId) < 0)
                    touches.Add((data.pointerId, data.position));
                mousePointer = null;
            }
            else if (touches.Count == 0 && !mousePointer.HasValue)
            {
                mousePointer = data.pointerId;
            }
        }

        public void OnDrag(PointerEventData data)
        {
            int index = FindTouch(data.pointerId);
            if (index >= 0)
                touches[index] = (data.pointerId, data.position);
            else if (mousePointer == data.pointerId && touches.Count == 0 && orbit != null)
                orbit.AddDrag(data.delta, false);
        }

        public void OnPointerUp(PointerEventData data)
        {
            int index = FindTouch(data.pointerId);
            if (index >= 0)
                touches.RemoveAt(index);
            else if (mousePointer == data.pointerId)
                mousePointer = null;
        }

        public void OnScroll(PointerEventData data)
        {
            if (orbit != null && touches.Count == 0 && data.currentInputModule != null)
                orbit.AddWheelZoom(data.currentInputModule.ConvertPointerEventScrollDeltaToTicks(data.scrollDelta).y);
        }

        private void Update()
        {
            if (touches.Count == 0 || orbit == null)
            {
                previousFirst = previousSecond = null;
                return;
            }

            var first = touches[0];
            if (touches.Count == 1)
            {
                if (previousFirst == first.Id && !previousSecond.HasValue)
                    orbit.AddDrag(first.Position - previousPosition, true);
                previousFirst = first.Id;
                previousSecond = null;
                previousPosition = first.Position;
                return;
            }

            var second = touches[1];
            float spread = Vector2.Distance(first.Position, second.Position);
            float shortEdge = Mathf.Min(Screen.width, Screen.height);
            if (previousFirst == first.Id && previousSecond == second.Id && shortEdge > 0f)
                orbit.AddPinchZoom((spread - previousSpread) / shortEdge);
            previousFirst = first.Id;
            previousSecond = second.Id;
            previousSpread = spread;
        }

        private int FindTouch(int id)
        {
            for (int index = 0; index < touches.Count; index++)
            {
                if (touches[index].Id == id)
                    return index;
            }

            return -1;
        }

        private void ClearPointers()
        {
            mousePointer = previousFirst = previousSecond = null;
            touches.Clear();
        }

        public void OnCancel(BaseEventData data) => ClearPointers();
        private void OnDisable() => ClearPointers();
        private void OnApplicationFocus(bool focused) { if (!focused) ClearPointers(); }
        private void OnApplicationPause(bool paused) { if (paused) ClearPointers(); }
    }
}
