using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class ElephantFreePushBlock : MonoBehaviour
    {
        [SerializeField, Min(0.1f), Tooltip("Horizontal push speed in metres per second.")]
        private float pushSpeed = 1.5f;
        [SerializeField, Tooltip("Block centre below this height starts the respawn delay.")]
        private float fallHeight = -5f;
        [SerializeField, Min(0f), Tooltip("Seconds after falling before returning to this block's starting pose.")]
        private float respawnDelay = 0.8f;
        [SerializeField, Tooltip("Use the authored transform as this block's individual respawn pose.")]
        private bool useInitialTransform = true;
        [SerializeField, Tooltip("Optional individual respawn point when Use Initial Transform is disabled.")]
        private Transform respawnPoint;

        private Rigidbody body;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float fallenAt = -1f;
        private Vector3 requestedDirection;
        private float verticalVelocity;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            body.constraints |= RigidbodyConstraints.FreezeRotation;
            body.isKinematic = true;
            body.useGravity = false;
        }

        public void TryPush(PrototypeMotor motor, Vector3 movement)
        {
            if (motor == null || motor.GetComponent<PrototypeAnimal>()?.Species != PrototypeSpecies.Elephant)
                return;
            if (motor.GetComponent<AnimalMechanicsActor>()?.Locked == true || fallenAt >= 0f)
                return;
            Vector3 horizontal = Vector3.ProjectOnPlane(movement, Vector3.up);
            if (horizontal.sqrMagnitude < 0.01f)
                return;
            requestedDirection = horizontal.normalized;
        }

        private void FixedUpdate()
        {
            Vector3 movement = requestedDirection * pushSpeed * Time.fixedDeltaTime;
            requestedDirection = Vector3.zero;
            verticalVelocity += Physics.gravity.y * Time.fixedDeltaTime;
            movement.y = verticalVelocity * Time.fixedDeltaTime;
            // A resting kinematic body can start flush with the floor. SweepTest
            // misses that contact, so sample below the box footprint as well.
            var bounds = GetComponent<BoxCollider>().bounds;
            float probeLength = Mathf.Max(0f, -movement.y) + .08f;
            Vector3 probe = new Vector3(bounds.center.x, bounds.min.y + .04f, bounds.center.z);
            if (Physics.Raycast(probe, Vector3.down, out var ground, probeLength,
                    ~0, QueryTriggerInteraction.Ignore) && ground.collider != GetComponent<BoxCollider>())
            {
                movement.y = -Mathf.Max(0f, ground.distance - .04f);
                verticalVelocity = 0f;
            }
            Vector3 horizontalMove = Vector3.ProjectOnPlane(movement, Vector3.up);
            if (horizontalMove.sqrMagnitude > 0f &&
                body.SweepTest(horizontalMove.normalized, out var obstacle, horizontalMove.magnitude + .02f))
                movement -= horizontalMove * (1f - Mathf.Clamp01((obstacle.distance - .02f) / horizontalMove.magnitude));
            body.MovePosition(body.position + movement);
            if (transform.position.y >= fallHeight)
            {
                fallenAt = -1f;
                return;
            }
            if (fallenAt < 0f)
                fallenAt = Time.fixedTime;
            if (Time.fixedTime - fallenAt < respawnDelay)
                return;

            requestedDirection = Vector3.zero;
            verticalVelocity = 0f;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = !useInitialTransform && respawnPoint != null ? respawnPoint.position : initialPosition;
            body.rotation = !useInitialTransform && respawnPoint != null ? respawnPoint.rotation : initialRotation;
            body.Sleep();
            fallenAt = -1f;
        }
    }
}
