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

        public Vector3 Position => interactionPoint != null ? interactionPoint.position : transform.position;
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
            if (handle != null)
                handle.localRotation = restRotation * Quaternion.Euler(open ? pullAngle : 0f, 0f, 0f);
        }
    }
}
