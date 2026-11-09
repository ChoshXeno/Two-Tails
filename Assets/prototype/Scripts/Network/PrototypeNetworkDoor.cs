using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class PrototypeNetworkDoor : NetworkBehaviour
    {
        private struct Travel : INetworkSerializable, IEquatable<Travel>
        {
            public bool DemandOpen;
            public bool Blocked;
            public float Origin;
            public double Started;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref DemandOpen);
                serializer.SerializeValue(ref Blocked);
                serializer.SerializeValue(ref Origin);
                serializer.SerializeValue(ref Started);
            }

            public bool Equals(Travel other) => DemandOpen == other.DemandOpen && Blocked == other.Blocked &&
                Origin.Equals(other.Origin) && Started.Equals(other.Started);
        }

        [SerializeField] private PrototypeDoor panel;
        [SerializeField, Tooltip("Fixed passage volume used by the server to pause closing.")]
        private BoxCollider passage;
        [SerializeField, Tooltip("Layers containing the owner controller and server-side player proxy.")]
        private LayerMask playerLayers = 1 << 2;

        private readonly NetworkVariable<Travel> travel = new();
        private readonly NetworkPuzzleOccupancy occupancy = new();
        private readonly HashSet<ulong> blockers = new();
        public bool DemandOpen => travel.Value.DemandOpen;
        public bool ClosingBlocked => travel.Value.Blocked;
        public float Progress => IsSpawned ? Evaluate(travel.Value, NetworkManager.ServerTime.Time) : 0f;
        public event Action<bool> DemandChanged;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                travel.Value = default;
            Apply(travel.Value);
            DemandChanged?.Invoke(travel.Value.DemandOpen);
            travel.OnValueChanged += Changed;
        }

        public override void OnNetworkDespawn()
        {
            travel.OnValueChanged -= Changed;
            panel.ApplyTravel(0f);
            blockers.Clear();
        }

        public void SetDemand(bool open)
        {
            if (!IsSpawned || !IsServer || DemandOpen == open)
                return;

            double now = NetworkManager.ServerTime.Time;
            float current = Evaluate(travel.Value, now);
            SetTravel(open, !open && current > 0f && PassageOccupied(), current, now);
        }

        private void FixedUpdate()
        {
            if (!IsSpawned || !IsServer || DemandOpen)
                return;

            double now = NetworkManager.ServerTime.Time;
            float current = Evaluate(travel.Value, now);
            bool blocked = current > 0f && PassageOccupied();
            if (blocked != ClosingBlocked)
                SetTravel(false, blocked, current, now);
        }

        private void Update()
        {
            if (IsSpawned)
                panel.ApplyTravel(Progress);
        }

        private bool PassageOccupied()
        {
            Physics.SyncTransforms();
            occupancy.Sample(NetworkManager, passage, playerLayers, blockers);
            return blockers.Count > 0;
        }

        private void SetTravel(bool open, bool blocked, float current, double now)
        {
            travel.Value = new Travel { DemandOpen = open, Blocked = blocked, Origin = current, Started = now };
        }

        private float Evaluate(Travel state, double now)
        {
            if (state.Blocked)
                return state.Origin;

            float amount = (float)Math.Max(0, now - state.Started) / panel.TravelDuration;
            return Mathf.MoveTowards(state.Origin, state.DemandOpen ? 1f : 0f, amount);
        }

        private void Changed(Travel previous, Travel current)
        {
            Apply(current);
            if (previous.DemandOpen != current.DemandOpen)
                DemandChanged?.Invoke(current.DemandOpen);
        }

        private void Apply(Travel state) => panel.ApplyTravel(Evaluate(state, NetworkManager.ServerTime.Time));
    }
}
