using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PrototypeMotor : MonoBehaviour
    {
        [SerializeField, Tooltip("Child that turns to face movement. The controller root stays upright and unrotated.")] private Transform visual;
        [SerializeField, Tooltip("Camera used for screen-relative movement. Defaults to Main Camera.")] private Transform movementCamera;
        [SerializeField, Min(0f), Tooltip("Full-input movement speed in metres per second.")] private float speed = 4f;
        [SerializeField, Min(0f), Tooltip("Visual turning speed in degrees per second.")] private float turnSpeed = 720f;
        [SerializeField, Min(0f), Tooltip("Downward acceleration in metres per second squared.")] private float gravity = 24f;
        [SerializeField, Tooltip("World Y below which the controller returns to its spawn.")] private float resetHeight = -5f;

        public Transform Facing => visual != null ? visual : transform;

        private CharacterController controller;
        private Vector3 spawnPosition;
        private Vector2 moveInput;
        private float verticalSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            spawnPosition = transform.position;
            if (movementCamera == null && Camera.main != null)
                movementCamera = Camera.main.transform;
        }

        // Local or future remote input can drive the same motor.
        public void SetMoveInput(Vector2 input)
        {
            moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void SetLocalContext(Transform camera)
        {
            movementCamera = camera;
            spawnPosition = transform.position;
        }

        private void OnDisable()
        {
            moveInput = Vector2.zero;
            verticalSpeed = 0f;
        }

        private void Update()
        {
            if (transform.position.y < resetHeight)
            {
                controller.enabled = false;
                transform.position = spawnPosition;
                controller.enabled = true;
                verticalSpeed = 0f;
                return;
            }

            Vector3 forward = movementCamera == null
                ? Vector3.forward
                : Vector3.ProjectOnPlane(movementCamera.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 direction = forward * moveInput.y + right * moveInput.x;

            float deltaTime = Time.deltaTime;
            verticalSpeed = controller.isGrounded && verticalSpeed <= 0f
                ? -2f
                : verticalSpeed - gravity * deltaTime;
            CollisionFlags collisions = controller.Move(
                (direction * speed + Vector3.up * verticalSpeed) * deltaTime);
            if ((collisions & CollisionFlags.Below) != 0)
                verticalSpeed = -2f;

            if (visual != null && direction.sqrMagnitude > 0.0001f)
                visual.rotation = Quaternion.RotateTowards(visual.rotation,
                    Quaternion.LookRotation(direction), turnSpeed * deltaTime);
        }
    }
}
