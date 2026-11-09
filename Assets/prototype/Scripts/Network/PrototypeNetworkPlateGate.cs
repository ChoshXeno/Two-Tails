using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(PrototypeNetworkDoor))]
    public sealed class PrototypeNetworkPlateGate : NetworkBehaviour
    {
        [SerializeField] private PrototypePressurePlate plateA;
        [SerializeField] private PrototypePressurePlate plateB;
        [SerializeField, Tooltip("Layers containing player controllers and server-side puzzle proxies.")]
        private LayerMask playerLayers = 1 << 2;

        private readonly NetworkVariable<byte> flags = new();
        private readonly NetworkPuzzleOccupancy occupancy = new();
        private readonly HashSet<ulong> a = new();
        private readonly HashSet<ulong> b = new();
        private BoxCollider volumeA;
        private BoxCollider volumeB;
        private PrototypeNetworkDoor door;
        public bool PlateAActive => (flags.Value & 1) != 0;
        public bool PlateBActive => (flags.Value & 2) != 0;

        private void Awake()
        {
            door = GetComponent<PrototypeNetworkDoor>();
            volumeA = plateA.GetComponent<BoxCollider>();
            volumeB = plateB.GetComponent<BoxCollider>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                flags.Value = 0;
            Apply(flags.Value);
            flags.OnValueChanged += Changed;
        }

        public override void OnNetworkDespawn()
        {
            flags.OnValueChanged -= Changed;
            Apply(0);
            a.Clear();
            b.Clear();
        }

        private void FixedUpdate()
        {
            if (!IsSpawned || !IsServer)
                return;

            Physics.SyncTransforms();
            occupancy.Sample(NetworkManager, volumeA, playerLayers, a);
            occupancy.Sample(NetworkManager, volumeB, playerLayers, b);

            byte current = (byte)((a.Count > 0 ? 1 : 0) | (b.Count > 0 ? 2 : 0));
            if (current != flags.Value)
                flags.Value = current;

            bool demand = HasDistinctOccupants(a, b);
            if (door.DemandOpen != demand)
                door.SetDemand(demand);
        }

        private static bool HasDistinctOccupants(HashSet<ulong> first, HashSet<ulong> second)
        {
            foreach (ulong left in first)
            {
                foreach (ulong right in second)
                {
                    if (left != right)
                        return true;
                }
            }
            return false;
        }

        private void Changed(byte previous, byte current) => Apply(current);

        private void Apply(byte value)
        {
            plateA.ApplyOccupiedVisual((value & 1) != 0);
            plateB.ApplyOccupiedVisual((value & 2) != 0);
        }

        public string GetStatus()
        {
            if (!IsSpawned)
                return "";
            string left = PlateAActive ? "Active" : "Inactive";
            string right = PlateBActive ? "Active" : "Inactive";
            string gate = door.ClosingBlocked ? "Closing blocked" :
                door.DemandOpen ? (door.Progress >= 1f ? "Open" : "Opening") :
                (door.Progress <= 0f ? "Closed" : "Closing");
            return $"Plate A: {left}  |  Plate B: {right}  |  Gate: {gate}";
        }
    }
}
