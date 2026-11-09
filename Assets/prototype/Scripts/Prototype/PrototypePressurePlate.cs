using System.Collections.Generic;
using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PrototypePressurePlate : MonoBehaviour
    {
        [SerializeField, Tooltip("The moving plate mesh. The trigger stays fixed.")] private Renderer visual;
        [SerializeField, Min(0f), Tooltip("Visual-only press depth in metres.")] private float pressDepth = 0.025f;
        [SerializeField, Tooltip("Plate color when no valid character is present.")] private Color inactiveColor = new(1f, 0.78f, 0.08f);
        [SerializeField, Tooltip("Plate color when a valid character is present.")] private Color activeColor = new(0.16f, 0.84f, 0.26f);
        [SerializeField, Tooltip("Layers checked for character colliders. Identity is verified separately.")] private LayerMask occupantLayers = ~0;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private readonly HashSet<PrototypeMotor> occupants = new();
        private readonly Collider[] overlapBuffer = new Collider[12];
        private BoxCollider trigger;
        private MaterialPropertyBlock colors;
        private Vector3 restPosition;
        public int OccupantCount => occupants.Count;

        public void ApplyOccupiedVisual(bool occupied) => UpdateVisual(occupied);

        private void Awake()
        {
            trigger = GetComponent<BoxCollider>();
            if (visual != null)
            {
                restPosition = visual.transform.localPosition;
                colors = new MaterialPropertyBlock();
                visual.GetPropertyBlock(colors);
                UpdateVisual(false);
            }
        }

        private void FixedUpdate()
        {
            bool wasOccupied = occupants.Count > 0;
            occupants.Clear();
            if (trigger.enabled)
                SampleOccupants();
            bool occupied = occupants.Count > 0;
            if (wasOccupied != occupied)
                UpdateVisual(occupied);
        }

        private void SampleOccupants()
        {
            // Rebuild from the trigger volume so disabled, destroyed, and teleported colliders cannot leave stale entries.
            Vector3 center = transform.TransformPoint(trigger.center);
            Vector3 half = Vector3.Scale(trigger.size * 0.5f, transform.lossyScale);
            half = new Vector3(Mathf.Abs(half.x), Mathf.Abs(half.y), Mathf.Abs(half.z));
            int count = Physics.OverlapBoxNonAlloc(center, half, overlapBuffer, transform.rotation,
                occupantLayers, QueryTriggerInteraction.Collide);
            Collider[] overlaps = overlapBuffer;
            if (count == overlaps.Length)
            {
                overlaps = Physics.OverlapBox(center, half, transform.rotation, occupantLayers, QueryTriggerInteraction.Collide);
                count = overlaps.Length;
            }

            for (int index = 0; index < count; index++)
            {
                var motor = overlaps[index].GetComponentInParent<PrototypeMotor>();
                if (motor != null && motor.isActiveAndEnabled &&
                    motor.TryGetComponent(out CharacterController controller) && controller.enabled)
                    occupants.Add(motor);
            }
        }

        public bool HasDistinctOccupantFrom(PrototypePressurePlate other)
        {
            foreach (PrototypeMotor first in occupants)
            {
                foreach (PrototypeMotor second in other.occupants)
                {
                    if (first != second)
                        return true;
                }
            }

            return false;
        }

        private void UpdateVisual(bool occupied)
        {
            if (visual == null)
                return;

            visual.transform.localPosition = restPosition - Vector3.up * (occupied ? pressDepth : 0f);
            colors.SetColor(BaseColor, occupied ? activeColor : inactiveColor);
            visual.SetPropertyBlock(colors);
        }

        private void OnDisable()
        {
            occupants.Clear();
            if (colors != null)
                UpdateVisual(false);
        }
    }
}
