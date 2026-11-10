using UnityEngine;

namespace TwoTails.Prototype
{
    public sealed class PrototypeFollowCamera : MonoBehaviour
    {
        [SerializeField, Tooltip("P1's controller root.")] private Transform target;
        [SerializeField, Tooltip("P1's visual forward at spawn. Used to set the initial orbit yaw once.")] private Transform spawnFacing;
        [SerializeField, Tooltip("Orbit pivot height above P1's feet in metres.")] private float pivotHeight = 1.2f;
        [SerializeField, Min(0.1f), Tooltip("Initial unobstructed orbit distance in metres.")] private float distance = 5f;
        [SerializeField, Min(0.1f), Tooltip("Closest user-selected orbit distance in metres. Obstructions can bring the camera closer.")] private float minimumDistance = 2f;
        [SerializeField, Min(0.1f), Tooltip("Farthest user-selected orbit distance in metres.")] private float maximumDistance = 8f;
        [SerializeField, Min(0.01f), Tooltip("Smoothing time for wheel and pinch zoom.")] private float zoomTime = 0.12f;
        [SerializeField, Min(0f), Tooltip("Metres of zoom per standard mouse wheel tick.")] private float wheelSensitivity = 0.5f;
        [SerializeField, Min(0f), Tooltip("Metres of zoom per screen short edge of pinch movement.")] private float pinchSensitivity = 6f;
        [SerializeField, Tooltip("Initial downwards orbit angle in degrees.")] private float initialPitch = 25f;
        [SerializeField, Range(-85f, 85f), Tooltip("Minimum downwards orbit angle in degrees.")] private float minimumPitch = -10f;
        [SerializeField, Range(-85f, 85f), Tooltip("Maximum downwards orbit angle in degrees.")] private float maximumPitch = 65f;
        [SerializeField, Min(0f), Tooltip("Degrees of orbit per mouse drag pixel.")] private float mouseSensitivity = 0.22f;
        [SerializeField, Min(0f), Tooltip("Degrees of orbit per touch drag pixel.")] private float touchSensitivity = 0.16f;
        [SerializeField, Tooltip("Reverse the vertical direction of mouse and touch drags.")] private bool invertY;
        [SerializeField, Min(0.01f), Tooltip("Smoothing time for the position of the follow pivot.")] private float smoothTime = 0.10f;
        [SerializeField, Min(0.01f), Tooltip("Smoothing time for the drag angle.")] private float rotationTime = 0.075f;
        [SerializeField, Tooltip("Solid prototype geometry. Characters and decorative props are excluded.")] private LayerMask obstructionMask = 1;
        [SerializeField, Min(0.01f), Tooltip("SphereCast radius in metres.")] private float obstructionRadius = 0.2f;
        [SerializeField, Min(0f), Tooltip("Gap in metres to leave before a camera obstruction.")] private float obstructionPadding = 0.08f;
        [SerializeField, Min(0.01f), Tooltip("Smoothing time for returning to full distance after an obstruction.")] private float recoveryTime = 0.22f;

        private Vector3 pivot;
        private Vector3 pivotVelocity;
        private float desiredYaw;
        private float desiredPitch;
        private float yaw;
        private float pitch;
        private float yawVelocity;
        private float pitchVelocity;
        private float desiredDistance;
        private float zoomDistance;
        private float zoomVelocity;
        private float obstructionLimit;
        private float obstructionVelocity;
        private Vector3 menuPosition;
        private Quaternion menuRotation;

        private float DistanceLimit => Mathf.Max(minimumDistance, maximumDistance);

        private void Awake()
        {
            menuPosition = transform.position;
            menuRotation = transform.rotation;
        }

        private void Start() => ResetOrbit();

        public void Bind(Transform player, Transform facing)
        {
            target = player;
            spawnFacing = facing;
            if (target == null)
                transform.SetPositionAndRotation(menuPosition, menuRotation);
            else
                ResetOrbit();
        }

        private void ResetOrbit()
        {
            if (target == null)
                return;

            desiredYaw = yaw = (spawnFacing != null ? spawnFacing : target).eulerAngles.y;
            desiredPitch = pitch = Mathf.Clamp(initialPitch, minimumPitch, maximumPitch);
            pivotVelocity = Vector3.zero;
            yawVelocity = pitchVelocity = zoomVelocity = obstructionVelocity = 0f;
            pivot = target.position + Vector3.up * pivotHeight;
            desiredDistance = zoomDistance = Mathf.Clamp(distance, minimumDistance, DistanceLimit);
            obstructionLimit = DistanceLimit;
            PlaceCamera();
        }

        public void AddDrag(Vector2 pixelDelta, bool touch)
        {
            float sensitivity = touch ? touchSensitivity : mouseSensitivity;
            desiredYaw = Mathf.Repeat(desiredYaw + pixelDelta.x * sensitivity, 360f);
            float vertical = pixelDelta.y * sensitivity * (invertY ? 1f : -1f);
            desiredPitch = Mathf.Clamp(desiredPitch + vertical, minimumPitch, maximumPitch);
        }

        public void AddOrbitDegrees(Vector2 delta)
        {
            desiredYaw = Mathf.Repeat(desiredYaw + delta.x, 360f);
            desiredPitch = Mathf.Clamp(desiredPitch - delta.y, minimumPitch, maximumPitch);
        }
        public void AddZoomMetres(float delta) => ChangeZoom(delta);

        public void AddWheelZoom(float ticks) => ChangeZoom(ticks * wheelSensitivity);

        public void AddPinchZoom(float normalizedSpread) => ChangeZoom(normalizedSpread * pinchSensitivity);

        private void ChangeZoom(float amount)
        {
            desiredDistance = Mathf.Clamp(desiredDistance - amount, minimumDistance, DistanceLimit);
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            pivot = Vector3.SmoothDamp(pivot, target.position + Vector3.up * pivotHeight,
                ref pivotVelocity, smoothTime);
            yaw = Mathf.SmoothDampAngle(yaw, desiredYaw, ref yawVelocity, rotationTime);
            pitch = Mathf.SmoothDamp(pitch, desiredPitch, ref pitchVelocity, rotationTime);
            zoomDistance = Mathf.SmoothDamp(zoomDistance, desiredDistance, ref zoomVelocity, zoomTime);
            PlaceCamera();
        }

        private void PlaceCamera()
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 backwards = rotation * Vector3.back;
            float allowed = DistanceLimit;
            if (Physics.SphereCast(pivot, obstructionRadius, backwards, out RaycastHit hit,
                    zoomDistance, obstructionMask, QueryTriggerInteraction.Ignore))
                allowed = Mathf.Max(0.01f, hit.distance - obstructionPadding);

            if (obstructionLimit > allowed)
            {
                obstructionLimit = allowed;
                obstructionVelocity = 0f;
            }
            else
            {
                obstructionLimit = Mathf.SmoothDamp(obstructionLimit, allowed, ref obstructionVelocity, recoveryTime);
            }

            transform.SetPositionAndRotation(pivot + backwards * Mathf.Min(zoomDistance, obstructionLimit), rotation);
        }
    }
}
