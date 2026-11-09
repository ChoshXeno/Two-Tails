using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PrototypeDoor : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Vertical panel travel in metres. The collider moves with the panel.")] private float openHeight = 3f;
        [SerializeField, Min(0.01f), Tooltip("Approximate seconds for a full open or close.")] private float openDuration = 0.6f;
        [SerializeField, Tooltip("Character layers for the panel's anti-crush check.")] private LayerMask characterLayers = 1 << 2;

        private Vector3 closedPosition;
        private BoxCollider panel;
        private float progress;
        private float velocity;
        private bool contactBypass;
        public bool TargetOpen { get; private set; }
        public bool IsMoving => progress != (TargetOpen ? 1f : 0f);
        public float TravelDuration => openDuration;

        public void ApplyTravel(float value)
        {
            transform.localPosition = closedPosition + Vector3.up * (openHeight * Mathf.Clamp01(value));
        }

        private void Awake()
        {
            closedPosition = transform.localPosition;
            panel = GetComponent<BoxCollider>();
        }

        public void SetOpen(bool open)
        {
            TargetOpen = open;
        }

        private void Update()
        {
            if (!IsMoving && !contactBypass)
                return;

            float target = TargetOpen ? 1f : 0f;
            float next = Mathf.Clamp01(Mathf.SmoothDamp(progress, target, ref velocity, openDuration / 4f));
            if (Mathf.Abs(next - target) < 0.002f)
            {
                next = target;
                velocity = 0f;
            }

            Vector3 nextPosition = closedPosition + Vector3.up * (openHeight * next);
            if (next < progress || contactBypass)
            {
                // The panel still closes on schedule. Its collider comes back once the contact volume is clear.
                contactBypass = WouldHitCharacter(nextPosition);
                panel.enabled = !contactBypass;
            }

            transform.localPosition = nextPosition;
            progress = next;
        }

        private bool WouldHitCharacter(Vector3 nextPosition)
        {
            Vector3 localTravel = nextPosition - transform.localPosition;
            Vector3 travel = transform.parent != null ? transform.parent.TransformVector(localTravel) : localTravel;
            Vector3 half = Vector3.Scale(panel.size * 0.5f, transform.lossyScale);
            half = new Vector3(Mathf.Abs(half.x), Mathf.Abs(half.y), Mathf.Abs(half.z));
            Vector3 sweep = Quaternion.Inverse(transform.rotation) * travel;
            half += new Vector3(Mathf.Abs(sweep.x), Mathf.Abs(sweep.y), Mathf.Abs(sweep.z)) * 0.5f;
            return Physics.CheckBox(transform.TransformPoint(panel.center) + travel * 0.5f,
                half + Vector3.one * 0.025f, transform.rotation, characterLayers, QueryTriggerInteraction.Ignore);
        }
    }
}
