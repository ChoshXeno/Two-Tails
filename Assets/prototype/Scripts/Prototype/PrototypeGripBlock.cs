using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PrototypeGripBlock : NetworkBehaviour
    {
        [SerializeField, Range(2, 3)] private int weight = 2;
        [SerializeField] private bool online;
        private readonly NetworkVariable<Vector3> position = new();
        private readonly NetworkVariable<bool> airborne = new();
        private readonly Dictionary<PrototypeMotor, (Vector3 direction, float expires)> grips = new();
        private readonly List<PrototypeMotor> expired = new();
        private BoxCollider shape;
        private Vector3 start;
        private float fallSpeed;
        private bool falling;

        private void Awake()
        {
            shape = GetComponent<BoxCollider>();
            start = transform.position;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer) position.Value = start;
            else transform.position = position.Value;
            position.OnValueChanged += ApplyPosition;
        }

        public override void OnNetworkDespawn()
        {
            position.OnValueChanged -= ApplyPosition;
            grips.Clear();
        }

        private void ApplyPosition(Vector3 previous, Vector3 next) => transform.position = next;

        internal bool CanReach(PrototypeMotor motor, bool facing)
        {
            if (falling || (online && (!IsSpawned || airborne.Value)) || motor == null || !motor.gameObject.activeInHierarchy ||
                motor.GetComponent<AnimalMechanicsActor>()?.Locked == true ||
                motor.GetComponent<PrototypeAnimal>() is not { } animal || animal.Species == PrototypeSpecies.Rabbit)
                return false;
            Vector3 origin = motor.transform.position + Vector3.up;
            Vector3 delta = shape.ClosestPoint(origin) - origin;
            if (delta.magnitude > 1.5f) return false;
            if (!facing) return true;
            return Vector3.Angle(motor.Facing.forward, Vector3.ProjectOnPlane(delta, Vector3.up)) <= 45f &&
                !PrototypeInteractor.IsBlocked(origin, delta, delta.magnitude, motor.transform, transform, ~0, sight);
        }

        private readonly RaycastHit[] sight = new RaycastHit[24];

        internal void Submit(PrototypeMotor motor, bool held, Vector3 direction)
        {
            if (!online || IsServer) Accept(motor, held, direction);
            else if (IsSpawned) GripRpc(held, direction);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void GripRpc(bool held, Vector3 direction, RpcParams rpc = default)
        {
            if (!NetworkManager.ConnectedClients.TryGetValue(rpc.Receive.SenderClientId, out var client)) return;
            var player = client.PlayerObject != null ? client.PlayerObject.GetComponent<NetworkPlayerPrototype>() : null;
            if (NetworkPuzzleOccupancy.IsCurrentPlayer(NetworkManager, player)) Accept(player.Motor, held, direction);
        }

        private void Accept(PrototypeMotor motor, bool held, Vector3 direction)
        {
            if (!held) { grips.Remove(motor); return; }
            if (!float.IsFinite(direction.x) || !float.IsFinite(direction.z) ||
                !CanReach(motor, !grips.ContainsKey(motor))) return;
            grips[motor] = (Vector3.ClampMagnitude(Vector3.ProjectOnPlane(direction, Vector3.up), 1f), Time.time + .25f);
        }

        private void FixedUpdate()
        {
            if (online && (!IsSpawned || !IsServer)) return;
            if (StepGravity()) return;
            expired.Clear();
            Vector3 direction = Vector3.zero;
            float speed = float.PositiveInfinity;
            int moving = 0;
            bool elephant = false;
            bool opposed = false;
            foreach (var pair in grips)
            {
                var motor = pair.Key;
                if (pair.Value.expires < Time.time || !CanReach(motor, false))
                {
                    expired.Add(motor);
                    continue;
                }
                Vector3 intent = pair.Value.direction;
                if (intent.sqrMagnitude < .01f) continue;
                if (moving > 0 && Vector3.Dot(direction.normalized, intent.normalized) < .65f) opposed = true;
                direction += intent;
                speed = Mathf.Min(speed, motor.GripSpeed);
                moving++;
                elephant |= motor.GetComponent<PrototypeAnimal>().Weight >= 3;
            }
            foreach (var motor in expired) grips.Remove(motor);
            if (opposed || moving == 0 || (weight == 3 && !elephant && moving < 2)) return;

            Vector3 delta = direction / moving * speed * Time.fixedDeltaTime;
            var bounds = shape.bounds;
            if (Blocked(bounds.center, bounds.extents - Vector3.one * .03f, delta, transform)) return;
            foreach (var motor in grips.Keys)
            {
                var capsule = motor.GetComponent<CharacterController>();
                Vector3 center = motor.transform.TransformPoint(capsule.center);
                Vector3 extents = new Vector3(capsule.radius, capsule.height * .5f, capsule.radius) - Vector3.one * .04f;
                if (Blocked(center, extents, delta, motor.transform)) return;
            }
            transform.position += delta;
            Physics.SyncTransforms();
            if (online) position.Value = transform.position;
        }

        private bool StepGravity()
        {
            Bounds bounds = shape.bounds;
            Vector3 center = bounds.center + Vector3.up * .04f;
            Vector3 extents = bounds.extents;
            extents.x = Mathf.Max(.01f, extents.x - .02f);
            extents.z = Mathf.Max(.01f, extents.z - .02f);
            float dt = Time.fixedDeltaTime;
            float step = (fallSpeed + Mathf.Max(0f, -Physics.gravity.y) * dt) * dt;
            float distance = step;
            int count = Physics.BoxCastNonAlloc(center, extents, Vector3.down, obstacles,
                Quaternion.identity, step + .04f, ~0, QueryTriggerInteraction.Ignore);
            bool supported = false;
            for (int i = 0; i < count; i++)
            {
                var hit = obstacles[i];
                if (hit.transform.IsChildOf(transform) || hit.transform.GetComponentInParent<PrototypeMotor>() != null ||
                    hit.normal.y < .5f) continue;
                float gap = Mathf.Max(0f, hit.distance - .04f);
                if (gap > distance) continue;
                distance = gap;
                supported = true;
            }
            if (supported && distance < .001f)
            {
                falling = false;
                fallSpeed = 0f;
                if (online) airborne.Value = false;
                return false;
            }
            falling = !supported;
            fallSpeed = supported ? 0f : step / dt;
            grips.Clear();
            transform.position += Vector3.down * distance;
            if (transform.position.y < start.y - 25f)
            {
                transform.position = start;
                falling = false;
                fallSpeed = 0f;
            }
            Physics.SyncTransforms();
            if (online)
            {
                airborne.Value = falling;
                position.Value = transform.position;
            }
            return true;
        }

        private readonly RaycastHit[] obstacles = new RaycastHit[32];

        private bool Blocked(Vector3 center, Vector3 extents, Vector3 delta, Transform mover)
        {
            int count = Physics.BoxCastNonAlloc(center, extents, delta.normalized, obstacles,
                Quaternion.identity, delta.magnitude + .015f, ~0, QueryTriggerInteraction.Ignore);
            if (count == obstacles.Length) return true;
            for (int i = 0; i < count; i++)
            {
                Transform hit = obstacles[i].transform;
                if (hit.IsChildOf(transform) || hit.IsChildOf(mover)) continue;
                var player = hit.GetComponentInParent<PrototypeMotor>();
                if (player != null && grips.ContainsKey(player)) continue;
                return true;
            }
            return false;
        }
    }
}
