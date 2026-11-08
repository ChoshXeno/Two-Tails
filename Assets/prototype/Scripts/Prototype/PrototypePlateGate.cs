using UnityEngine;

namespace TwoTails.Prototype
{
    [DefaultExecutionOrder(20)]
    public sealed class PrototypePlateGate : MonoBehaviour
    {
        [SerializeField, Tooltip("First pressure plate in the AND gate.")] private PrototypePressurePlate plateA;
        [SerializeField, Tooltip("Second pressure plate in the AND gate.")] private PrototypePressurePlate plateB;
        [SerializeField, Tooltip("Door controlled exclusively by these pressure plates.")] private PrototypeDoor door;

        private void FixedUpdate()
        {
            if (door == null || plateA == null || plateB == null || plateA == plateB)
                return;

            bool open = plateA.HasDistinctOccupantFrom(plateB);
            if (door.TargetOpen != open)
                door.SetOpen(open);
        }
    }
}
