using Unity.Netcode;
using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(NetworkObject), typeof(PrototypeLever))]
    public sealed class PrototypeNetworkLever : NetworkBehaviour
    {
        [SerializeField] private PrototypeNetworkDoor door;
        [SerializeField, Min(0f)] private float maximumDistance = 1.5f;
        [SerializeField, Range(0f, 180f)] private float maximumAngle = 45f;
        [SerializeField, Min(0f)] private float serverCooldown = 0.25f;
        [SerializeField, Tooltip("Solid geometry used for line of sight. Triggers are ignored.")]
        private LayerMask visibilityMask = 1;

        private PrototypeLever appearance;
        private readonly RaycastHit[] hits = new RaycastHit[12];
        private double nextUse;

        private void Awake() => appearance = GetComponent<PrototypeLever>();

        public override void OnNetworkSpawn()
        {
            nextUse = 0;
            appearance.ApplyTargetPose(door.DemandOpen);
            door.DemandChanged += appearance.ApplyTargetPose;
        }

        public override void OnNetworkDespawn()
        {
            door.DemandChanged -= appearance.ApplyTargetPose;
            appearance.ApplyTargetPose(false);
        }

        internal string Check(Transform origin, NetworkPlayerPrototype player)
        {
            if (!IsSpawned || !isActiveAndEnabled || !door.IsSpawned || !door.isActiveAndEnabled || origin == null)
                return "Interaction unavailable";

            Vector3 offset = appearance.Position - origin.position;
            float distance = offset.magnitude;
            if (distance > maximumDistance)
                return "Move closer";

            Vector3 direction = Vector3.ProjectOnPlane(offset, Vector3.up);
            Vector3 forward = Vector3.ProjectOnPlane(player.Motor.Facing.forward, Vector3.up);
            if (Vector3.Angle(forward, direction) > maximumAngle)
                return "Face the lever";
            return PrototypeInteractor.IsBlocked(origin.position, offset, distance, player.transform,
                transform, visibilityMask, hits) ? "Interaction blocked" : null;
        }

        internal string TryUse(Transform origin, NetworkPlayerPrototype player)
        {
            if (!IsServer)
                return "Interaction unavailable";

            Physics.SyncTransforms();
            string rejection = Check(origin, player);
            if (rejection != null)
                return rejection;

            double now = NetworkManager.ServerTime.Time;
            if (now < nextUse)
                return "Please wait";

            nextUse = now + serverCooldown;
            door.SetDemand(!door.DemandOpen);
            return null;
        }

        internal string Prompt => door.DemandOpen ? "E / Interact: Close door" : "E / Interact: Open door";
    }
}
