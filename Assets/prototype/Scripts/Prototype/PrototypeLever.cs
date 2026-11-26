using UnityEngine;

namespace TwoTails.Prototype
{
    public sealed class PrototypeLever : MonoBehaviour
    {
        [SerializeField, Tooltip("Chest-height point used for the interaction range and visibility tests.")] private Transform interactionPoint;
        [SerializeField, Tooltip("The lever handle pivot that changes pose when used.")] private Transform handle;
        [SerializeField, Tooltip("Local angle change of the activated handle, in degrees.")] private float pullAngle = 65f;
        [SerializeField, Tooltip("Door panel toggled by this lever. Both characters use the same instance.")] private PrototypeDoor door;
        [SerializeField, Min(0f), Tooltip("Seconds between accepted toggles. At most one toggle is allowed per frame.")] private float debounce = 0.15f;

        private int lastFrame = -1;
        private float nextToggleTime;
        private Quaternion restRotation;
        private float pawMotionStarted = float.NegativeInfinity;
        private bool pawMotionOpen;

        public Vector3 Position => interactionPoint != null ? interactionPoint.position : transform.position;
        internal Vector3 PawContact => handle != null
            ? handle.TransformPoint(Vector3.up * .45f)
            : Position;
        public bool CanOperate => door != null && door.isActiveAndEnabled;
        public bool TargetOpen => door != null && door.TargetOpen;
        public bool IsMoving => door != null && door.IsMoving;

        private void Awake()
        {
            if (handle != null)
                restRotation = handle.localRotation;
        }

        public bool TryToggle()
        {
            if (!CanOperate || Time.frameCount == lastFrame || Time.time < nextToggleTime)
                return false;

            lastFrame = Time.frameCount;
            nextToggleTime = Time.time + debounce;
            door.SetOpen(!door.TargetOpen);
            ApplyTargetPose(door.TargetOpen);
            return true;
        }

        public void ApplyTargetPose(bool open)
        {
            if (Time.time < pawMotionStarted + .45f && open == pawMotionOpen) return;
            pawMotionStarted = float.NegativeInfinity;
            if (handle != null)
                handle.localRotation = HandleRotation(open);
        }

        internal void ShowPawToggle(bool open)
        {
            if (handle == null) return;
            pawMotionOpen = open;
            pawMotionStarted = Time.time;
            handle.localRotation = HandleRotation(!open);
        }

        private Quaternion HandleRotation(bool open) =>
            restRotation * Quaternion.Euler(open ? pullAngle : 0f, 0f, 0f);

        private void Update()
        {
            if (float.IsNegativeInfinity(pawMotionStarted) || handle == null) return;
            float elapsed = Time.time - pawMotionStarted;
            float progress = Mathf.SmoothStep(0f, 1f, (elapsed - .19f) / .24f);
            handle.localRotation = Quaternion.Slerp(HandleRotation(!pawMotionOpen), HandleRotation(pawMotionOpen), progress);
            if (elapsed >= .45f) pawMotionStarted = float.NegativeInfinity;
        }
    }
}
