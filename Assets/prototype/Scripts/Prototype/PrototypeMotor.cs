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

        [SerializeField, Min(0), Tooltip("Jump apex height in metres.")] private float jumpHeight=1.2f;
        [SerializeField, Min(0), Tooltip("Climb probe distance beyond controller front edge.")] private float climbDistance=0.4f;
        [SerializeField, Range(0,89), Tooltip("Maximum facing and input angle toward a vine front.")] private float climbAngle=45;
        [SerializeField, Min(0), Tooltip("Vertical climbing speed in metres per second.")] private float climbSpeed=2;
        [SerializeField, Min(1f), Tooltip("Rabbit apex height relative to the ordinary jump.")]
        private float rabbitJumpMultiplier = 2f;
        [SerializeField, Tooltip("Scale the temporary capsule visual for Rabbit. Disable for a correctly sized model.")]
        private bool scaleRabbitPlaceholder = true;
        private int jumpsUsed;
        private bool rabbit;
        private float standingHeight, standingRadius, standingStep, standingSkin;
        private Vector3 standingCenter, visualScale, visualPosition;
        private bool jumpPending;

        public Vector3 PlaceholderScale => visualScale * (rabbit && scaleRabbitPlaceholder ? .5f : 1f);

        public void ApplySpecies(PrototypeSpecies species)
        {
            bool next = species == PrototypeSpecies.Rabbit;
            if (next == rabbit) return;
            rabbit = next;
            ClearPendingInput();
            jumpsUsed = 0;
            verticalSpeed = 0;
            climbing = null;
            controller.radius = standingRadius * (rabbit ? .5f : 1f);
            controller.height = standingHeight * (rabbit ? .5f : 1f);
            controller.skinWidth = standingSkin * (rabbit ? .5f : 1f);
            controller.stepOffset = standingStep * (rabbit ? .5f : 1f);
            controller.center = standingCenter - Vector3.up * ((standingHeight - controller.height) * .5f);
            if (visual != null)
            {
                visual.localScale = PlaceholderScale;
                visual.localPosition = visualPosition - Vector3.up *
                    (rabbit && scaleRabbitPlaceholder ? standingHeight * .25f : 0f);
            }
            GetComponent<NetworkPlayerPrototype>()?.ResizeCollisionProxy(controller);
        }
        private PrototypeClimbSurface climbing;
        public bool IsClimbing => climbing!=null;
        public PrototypeMotion Motion => IsClimbing ? PrototypeMotion.Climb : controller!=null && !controller.isGrounded ? (verticalSpeed>0 ? PrototypeMotion.Jump : PrototypeMotion.Fall) : moveInput.sqrMagnitude>0.01f ? PrototypeMotion.Run : PrototypeMotion.Idle;
        private void LateUpdate() { if(TryGetComponent<PrototypeAnimal>(out var animal)) animal.ApplyMotion(Motion); }
        public float VerticalSpeed => verticalSpeed;
        public void RequestJump() { if(controller!=null && (rabbit ? jumpsUsed < 2 : controller.isGrounded) && !IsClimbing && GetComponent<PrototypeSwimmer>()?.IsSwimming!=true) jumpPending=true; }
        public void ClearPendingInput() { jumpPending=false; moveInput=Vector2.zero; GetComponent<PrototypeSwimmer>()?.ClearInput(); }
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.TryGetComponent<CooperativeWeightBox>(out var box)) box.Push(this);
            if (hit.collider.TryGetComponent<ElephantFreePushBlock>(out var freeBlock)) freeBlock.TryPush(this, hit.moveDirection);
        }
        public void Warp(Vector3 position){GetComponent<PrototypeSwimmer>()?.ResetState();ClearPendingInput();climbing=null;verticalSpeed=0;jumpsUsed=0;controller.enabled=false;transform.position=position;controller.enabled=true;}
        public void Ride(Vector3 delta){if(enabled&&controller.enabled&&!IsClimbing&&verticalSpeed<=0)controller.Move(delta);}
        public void ResetToSpawn()
        {
            ClearPendingInput(); GetComponent<PrototypeSwimmer>()?.ResetState(); climbing=null; verticalSpeed=0; jumpsUsed=0;
            controller.enabled=false; transform.position=spawnPosition; controller.enabled=true;
        }
        private bool ClearAt(Vector3 feet)
        {
            foreach(var hit in Physics.OverlapCapsule(feet+Vector3.up*(controller.radius+0.06f),feet+Vector3.up*(controller.height-controller.radius),controller.radius*0.95f,~0,QueryTriggerInteraction.Ignore))
                if(!hit.transform.IsChildOf(transform)) return false;
            return true;
        }
        private bool Climb(Vector3 direction,float dt)
        {
            if(climbing==null && TryGetComponent<PrototypeAnimal>(out var animal) && animal.Species==PrototypeSpecies.Monkey && direction.sqrMagnitude>0.1f)
            {
                var origin=transform.position+Vector3.up;
                if(Physics.Raycast(origin,direction.normalized,out var hit,controller.radius+climbDistance,~(1<<gameObject.layer),QueryTriggerInteraction.Ignore))
                {
                    var surface=hit.collider.GetComponent<PrototypeClimbSurface>();
                    if(surface!=null && surface.HasLanding && Vector3.Dot(hit.normal,surface.Normal)>0.9f && Vector3.Angle(Facing.forward,-surface.Normal)<=climbAngle && Vector3.Angle(direction,-surface.Normal)<=climbAngle)
                        climbing=surface;
                }
            }
            if(climbing==null)return false;
            jumpPending=false; verticalSpeed=0;
            float inward=Vector3.Dot(direction,-climbing.Normal);
            if(inward < -0.1f){climbing=null;return false;}
            if(inward<=0.1f)return true;
            Vector3 target=climbing.Landing;
            if(transform.position.y>=target.y-0.05f)
            {
                if(ClearAt(target) && Physics.Raycast(target+Vector3.up*0.1f,Vector3.down,0.2f,~(1<<gameObject.layer),QueryTriggerInteraction.Ignore))
                { controller.Move(target-transform.position); if((transform.position-target).sqrMagnitude<0.01f) climbing=null; }
                return true;
            }
            controller.Move(Vector3.up*Mathf.Min(climbSpeed*dt,target.y-transform.position.y));
            return true;
        }

        public Transform Facing => visual != null ? visual : transform;

        private CharacterController controller;
        private Vector3 spawnPosition;
        private Vector2 moveInput;
        private float verticalSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            standingHeight = controller.height;
            standingRadius = controller.radius;
            standingStep = controller.stepOffset;
            standingSkin = controller.skinWidth;
            standingCenter = controller.center;
            visualScale = visual != null ? visual.localScale : Vector3.one;
            visualPosition = visual != null ? visual.localPosition : Vector3.zero;
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
            ClearPendingInput(); climbing=null;
            verticalSpeed = 0f;
        }

        private void Update()
        {
            if(GetComponent<AnimalMechanicsActor>()?.Locked==true){ClearPendingInput();return;}
            if (transform.position.y < resetHeight)
            {
                ResetToSpawn();
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
            if(GetComponent<PrototypeSwimmer>()?.Move(direction,deltaTime,controller)==true){jumpPending=false;climbing=null;verticalSpeed=0;if(visual!=null&&direction.sqrMagnitude>.0001f)visual.rotation=Quaternion.RotateTowards(visual.rotation,Quaternion.LookRotation(direction),turnSpeed*deltaTime);return;}
            if(Climb(direction,deltaTime))return;
            bool jump=jumpPending && (rabbit ? jumpsUsed < 2 : controller.isGrounded); jumpPending=false;
            verticalSpeed = controller.isGrounded && verticalSpeed <= 0f
                ? -2f
                : verticalSpeed - gravity * deltaTime;
            if (jump)
            {
                jumpsUsed++;
                verticalSpeed = Mathf.Sqrt(2f * gravity * jumpHeight * (rabbit ? rabbitJumpMultiplier : 1f));
            }
            AnimalMechanicsWorld.Instance?.RequestHeavyPush(this, direction);
            CollisionFlags collisions = controller.Move(
                (direction * speed + Vector3.up * verticalSpeed) * deltaTime);
            if ((collisions & CollisionFlags.Above)!=0 && verticalSpeed>0) verticalSpeed=0;
            if (!jump && verticalSpeed <= 0f && (collisions & CollisionFlags.Below) != 0)
            {
                verticalSpeed = -2f;
                jumpsUsed = 0;
            }

            if (visual != null && direction.sqrMagnitude > 0.0001f)
                visual.rotation = Quaternion.RotateTowards(visual.rotation,
                    Quaternion.LookRotation(direction), turnSpeed * deltaTime);
        }
    }
}



