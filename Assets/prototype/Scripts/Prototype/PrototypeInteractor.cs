using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(PrototypeMotor))]
    public sealed class PrototypeInteractor : MonoBehaviour
    {
        public enum Feedback { Hidden, FaceLever, OpenDoor, CloseDoor, Opening, Closing }

        [SerializeField, Tooltip("Chest-height origin for range and line-of-sight checks.")] private Transform interactionOrigin;
        [SerializeField, Min(0f), Tooltip("Maximum chest-to-lever distance in metres.")] private float maximumDistance = 1.5f;
        [SerializeField, Range(0f, 180f), Tooltip("Maximum horizontal angle from the character visual's forward direction.")] private float maximumAngle = 45f;
        [SerializeField, Tooltip("Layers tested for visibility. The actor and target's own colliders are filtered out.")] private LayerMask visibilityMask = ~0;
        [SerializeField, Tooltip("Lever targets available to this actor in this scene.")] private PrototypeLever[] levers;

        private PrototypeMotor motor;
        private readonly RaycastHit[] hits = new RaycastHit[12];

        private void Awake() => motor = GetComponent<PrototypeMotor>();

        public Feedback GetFeedback()
        {
            PrototypeLever target = FindTarget(out bool needsFacing);
            if (target != null)
                return target.TargetOpen ? Feedback.CloseDoor : Feedback.OpenDoor;
            if (needsFacing)
                return Feedback.FaceLever;

            if (levers == null)
                return Feedback.Hidden;
            foreach (PrototypeLever lever in levers)
            {
                if (lever != null && lever.IsMoving)
                    return lever.TargetOpen ? Feedback.Opening : Feedback.Closing;
            }

            return Feedback.Hidden;
        }

        public bool TryInteract()
        {
            PrototypeLever target = FindTarget(out _);
            return target != null && target.TryToggle();
        }

        private PrototypeLever FindTarget(out bool needsFacing)
        {
            needsFacing = false;
            if (interactionOrigin == null || levers == null)
                return null;

            PrototypeLever best = null;
            float bestDistance = maximumDistance;
            foreach (PrototypeLever lever in levers)
            {
                if (lever == null || !lever.isActiveAndEnabled || !lever.CanOperate)
                    continue;

                Vector3 offset = lever.Position - interactionOrigin.position;
                float distance = offset.magnitude;
                if (distance > maximumDistance || IsBlocked(lever, offset, distance))
                    continue;

                Vector3 horizontal = Vector3.ProjectOnPlane(offset, Vector3.up);
                Vector3 forward = Vector3.ProjectOnPlane(motor.Facing.forward, Vector3.up);
                if (Vector3.Angle(forward, horizontal) > maximumAngle)
                {
                    needsFacing = true;
                    continue;
                }

                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = lever;
                }
            }

            return best;
        }

        private bool IsBlocked(PrototypeLever lever, Vector3 offset, float distance)
        {
            if (distance <= Mathf.Epsilon)
                return false;

            var ray = new Ray(interactionOrigin.position, offset / distance);
            int count = Physics.RaycastNonAlloc(ray, hits, distance, visibilityMask, QueryTriggerInteraction.Ignore);
            RaycastHit[] results = count == hits.Length
                ? Physics.RaycastAll(ray, distance, visibilityMask, QueryTriggerInteraction.Ignore)
                : hits;
            if (results != hits)
                count = results.Length;

            for (int index = 0; index < count; index++)
            {
                Transform hitTransform = results[index].collider.transform;
                if (!hitTransform.IsChildOf(transform) && !hitTransform.IsChildOf(lever.transform))
                    return true;
            }

            return false;
        }
    }
}
