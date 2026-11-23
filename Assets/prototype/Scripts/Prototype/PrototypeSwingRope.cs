using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TwoTails.Prototype
{
    public sealed class PrototypeSwingRope : NetworkBehaviour
    {
        internal static readonly HashSet<PrototypeSwingRope> Active = new();
        [SerializeField] private bool online;
        [SerializeField, Min(1f)] private float length = 5f;
        [SerializeField, Min(0f)] private float pushForce = 2.4f;
        [SerializeField, Min(0f)] private float damping = .35f;
        [SerializeField, Range(5f, 75f)] private float maxAngle = 58f;
        [SerializeField, Min(.1f)] private float climbSpeed = 2f;
        [SerializeField] private Transform visual;

        private readonly NetworkVariable<Vector4> replicated = new();
        private readonly NetworkVariable<PrototypeRopeChainState> replicatedLinks = new();
        private const int Links = 8;
        private readonly Vector3[] points = new Vector3[Links + 1];
        private readonly Vector3[] previous = new Vector3[Links + 1];
        private readonly Transform[] segments = new Transform[Links];
        private bool chainReady;
        private float drivenDepth;
        private Vector2 angle, angularVelocity, input, lastDirection;
        private float burst, burstDeadline, inputExpires, nextSend;

        internal float Length => length;
        internal float ClimbSpeed => climbSpeed;
        internal bool Ready => !online || IsSpawned;

        private void Awake()
        {
            InitializeChain();
            if (visual == null) return;
            for (int i = 0; i < Links; i++)
            {
                var segment = Instantiate(visual.gameObject, transform);
                segment.name = $"Rope Link {i + 1}";
                segments[i] = segment.transform;
            }
            visual.gameObject.SetActive(false);
        }

        private void InitializeChain()
        {
            points[0] = transform.position;
            for (int i = 1; i <= Links; i++)
                points[i] = previous[i] = transform.position + Vector3.down * (length * i / Links);
            chainReady = true;
        }

        private void OnEnable() => Active.Add(this);
        private void OnDisable() => Active.Remove(this);

        private static Vector3 RopeDirection(Vector2 tilt)
        {
            float radians = tilt.magnitude;
            if (radians < .00001f) return Vector3.down;
            Vector2 horizontal = tilt * (Mathf.Sin(radians) / radians);
            return new Vector3(horizontal.x, -Mathf.Cos(radians), horizontal.y);
        }

        internal Vector3 Point(float depth)
        {
            if (!chainReady) InitializeChain();
            float scaled = Mathf.Clamp(depth, 0f, length) * Links / length;
            int index = Mathf.Min(Links - 1, Mathf.FloorToInt(scaled));
            return Vector3.Lerp(points[index], points[index + 1], scaled - index);
        }

        internal Vector3 Velocity(float depth)
        {
            float scaled = Mathf.Clamp(depth, 0f, length) * Links / length;
            int index = Mathf.Min(Links - 1, Mathf.FloorToInt(scaled));
            return Vector3.Lerp(points[index] - previous[index], points[index + 1] - previous[index + 1], scaled - index) /
                Mathf.Max(Time.fixedDeltaTime, .001f);
        }

        internal float DepthAt(Vector3 point)
        {
            float best = float.PositiveInfinity, depth = .6f;
            for (int i = 0; i < Links; i++)
            {
                Vector3 segment = points[i + 1] - points[i];
                float t = Mathf.Clamp01(Vector3.Dot(point - points[i], segment) / Mathf.Max(segment.sqrMagnitude, .0001f));
                float distance = (point - (points[i] + segment * t)).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                depth = (i + t) * length / Links;
            }
            return Mathf.Clamp(depth, .6f, length);
        }

        internal void Drive(PrototypeMotor motor, Vector3 direction)
        {
            if (!online || IsServer)
            {
                SetDrive(direction, DepthAt(motor.RopeGripPoint));
                return;
            }
            if (Time.unscaledTime < nextSend) return;
            nextSend = Time.unscaledTime + .045f;
            DriveRpc(new Vector2(direction.x, direction.z), DepthAt(motor.RopeGripPoint));
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone, Delivery = RpcDelivery.Unreliable)]
        private void DriveRpc(Vector2 direction, float requestedDepth, RpcParams rpc = default)
        {
            if (!IsServer || !float.IsFinite(direction.x) || !float.IsFinite(direction.y) ||
                !NetworkManager.ConnectedClients.TryGetValue(rpc.Receive.SenderClientId, out var client)) return;
            var player = client.PlayerObject;
            if (player == null || player.GetComponent<PrototypeAnimal>()?.Species != PrototypeSpecies.Monkey) return;
            Vector3 hand = player.transform.position + Vector3.up * PrototypeMotor.RopeGripHeight;
            float depth = DepthAt(hand);
            if (Mathf.Abs(requestedDepth - depth) > 1f || (hand - Point(depth)).sqrMagnitude > 1.2f * 1.2f) return;
            SetDrive(new Vector3(direction.x, 0f, direction.y), depth);
        }

        private void SetDrive(Vector3 direction, float depth)
        {
            input = Vector2.ClampMagnitude(new Vector2(direction.x, direction.z), 1f);
            drivenDepth = depth;
            inputExpires = Time.time + .16f;
        }

        private void FixedUpdate()
        {
            if (!Ready || (online && !IsServer)) return;
            float dt = Time.fixedDeltaTime;
            Vector2 drive = Time.time <= inputExpires ? input : Vector2.zero;
            if (drive.sqrMagnitude < .01f)
            {
                lastDirection = Vector2.zero;
            }
            else if (lastDirection == Vector2.zero || Vector2.Dot(lastDirection, drive.normalized) < .35f)
            {
                lastDirection = drive.normalized;
                burst = .32f;
                burstDeadline = Time.time + 1.2f;
            }

            float radians = angle.magnitude;
            Vector2 acceleration = radians > .00001f
                ? -angle / radians * (Physics.gravity.magnitude / length * Mathf.Sin(radians))
                : Vector2.zero;
            if (burst > 0f && drive.sqrMagnitude >= .01f && Time.time <= burstDeadline)
            {
                float alignment = Vector2.Dot(angularVelocity, drive);
                if (alignment > .012f || (angle.sqrMagnitude < .002f && angularVelocity.sqrMagnitude < .002f))
                {
                    acceleration += drive * pushForce;
                    burst -= dt;
                }
            }
            angularVelocity += (acceleration - damping * angularVelocity) * dt;
            angle += angularVelocity * dt;
            float limit = maxAngle * Mathf.Deg2Rad;
            if (angle.sqrMagnitude > limit * limit)
            {
                Vector2 outward = angle.normalized;
                angle = outward * limit;
                angularVelocity -= outward * Mathf.Max(0f, Vector2.Dot(angularVelocity, outward));
            }
            SimulateLinks(dt);
            if (online)
            {
                replicated.Value = new Vector4(angle.x, angle.y, angularVelocity.x, angularVelocity.y);
                var state = new PrototypeRopeChainState();
                for (int i = 1; i <= Links; i++) state.Set(i, points[i] - transform.position);
                replicatedLinks.Value = state;
            }
        }

        private void SimulateLinks(float dt)
        {
            points[0] = transform.position;
            int grip = Time.time <= inputExpires ? Mathf.Clamp(Mathf.RoundToInt(drivenDepth * Links / length), 1, Links) : 1;
            float rest = length / Links;
            Vector3 direction = RopeDirection(angle);
            for (int i = 1; i <= grip; i++)
            {
                previous[i] = points[i];
                points[i] = transform.position + direction * (i * rest);
            }
            for (int i = grip + 1; i <= Links; i++)
            {
                Vector3 current = points[i];
                Vector3 velocity = (current - previous[i]) * Mathf.Max(0f, 1f - damping * dt);
                points[i] = current + velocity + Physics.gravity * (dt * dt);
                previous[i] = current;
            }
            // Keep the grabbed portion on the pendulum while the free tail bends.
            for (int pass = 0; pass < 12; pass++)
            {
                for (int i = grip; i < Links; i++)
                {
                    Vector3 delta = points[i + 1] - points[i];
                    float distance = delta.magnitude;
                    if (distance < .0001f) continue;
                    Vector3 correction = delta * ((distance - rest) / distance);
                    if (i == grip) points[i + 1] -= correction;
                    else { points[i] += correction * .5f; points[i + 1] -= correction * .5f; }
                }
            }
        }

        private void Update()
        {
            if (online && IsSpawned && !IsServer)
            {
                Vector4 state = replicated.Value;
                angle = Vector2.Lerp(angle, new Vector2(state.x, state.y), 1f - Mathf.Exp(-18f * Time.deltaTime));
                angularVelocity = new Vector2(state.z, state.w);
                var links = replicatedLinks.Value;
                points[0] = transform.position;
                if (links.P8.sqrMagnitude > .01f)
                for (int i = 1; i <= Links; i++)
                {
                    previous[i] = points[i];
                    points[i] = Vector3.Lerp(points[i], transform.position + links.Get(i), 1f - Mathf.Exp(-18f * Time.deltaTime));
                }
            }
            UpdateVisual();
        }

        private void OnValidate() => UpdateVisual();

        private void UpdateVisual()
        {
            if (visual == null) return;
            if (!Application.isPlaying)
            {
                visual.SetPositionAndRotation(transform.position + Vector3.down * (length * .5f),
                    Quaternion.FromToRotation(Vector3.up, Vector3.down));
                visual.localScale = new Vector3(.12f, length, .12f);
            }
            for (int i = 0; i < Links; i++)
            {
                if (segments[i] == null) continue;
                Vector3 delta = points[i + 1] - points[i];
                segments[i].SetPositionAndRotation((points[i] + points[i + 1]) * .5f,
                    Quaternion.FromToRotation(Vector3.up, delta.normalized));
                segments[i].localScale = new Vector3(.12f, delta.magnitude, .12f);
            }
        }
    }
}
