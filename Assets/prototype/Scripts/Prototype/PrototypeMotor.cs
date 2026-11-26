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
            if (species != PrototypeSpecies.Monkey) { ClearRope(); climbing = null; }
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
        private float climbReattachAt;
        public bool IsClimbing => climbing!=null;
        internal PrototypeClimbSurface ClimbSurface => climbing;
        private float climbEndUntil;
        private float deathUntil;
        private PrototypeMotion MonkeyMotion
        {
            get
            {
                if (Time.time < deathUntil) return PrototypeMotion.Death;
                if (IsGripping)
                    return gripDirection < -.1f ? PrototypeMotion.Pull : PrototypeMotion.Push;
                if (IsOnRope) return Mathf.Abs(ropeClimb) > .01f ? PrototypeMotion.Vine_Climb : PrototypeMotion.Rope;
                if (IsClimbing) return moveInput.sqrMagnitude > .01f ? PrototypeMotion.Climb : PrototypeMotion.Wall_Hang_Idle;
                if (Time.time < climbEndUntil && verticalSpeed <= 0f) return PrototypeMotion.Climb_End;
                if (Time.time < interactUntil && controller.isGrounded) return PrototypeMotion.Interact;
                return GroundMotion;
            }
        }
        private float gripDirection;
        private float interactUntil;
        internal void ShowInteraction() => interactUntil = Time.time + .667f;
        public PrototypeMotion Motion => TryGetComponent<PrototypeAnimal>(out var animal) &&
            animal.Species == PrototypeSpecies.Monkey ? MonkeyMotion :
            IsOnRope ? PrototypeMotion.Rope : IsClimbing ? PrototypeMotion.Climb : GroundMotion;
        private PrototypeMotion GroundMotion => controller!=null && !controller.isGrounded ? (verticalSpeed>0 ? PrototypeMotion.Jump : PrototypeMotion.Fall) : moveInput.sqrMagnitude>0.01f ? PrototypeMotion.Run : PrototypeMotion.Idle;
        private void LateUpdate() { if(TryGetComponent<PrototypeAnimal>(out var animal)) animal.ApplyMotion(Motion); }
        public float VerticalSpeed => verticalSpeed;
        public void RequestJump() { if(IsOnRope || IsClimbing || (controller!=null && (rabbit ? jumpsUsed < 2 : controller.isGrounded) && GetComponent<PrototypeSwimmer>()?.IsSwimming!=true)) jumpPending=true; }
        public void ClearPendingInput() { ReleaseGrip(); jumpPending=false; moveInput=Vector2.zero; ropeClimb=0f; GetComponent<PrototypeSwimmer>()?.ClearInput(); }

        private PrototypeGripBlock grip;
        private Vector3 gripOffset;
        private float nextGripSend;
        internal float GripSpeed => speed;
        public bool IsGripping => grip != null;
        public Vector3 GripPoint => grip != null ? grip.GetComponent<BoxCollider>().ClosestPoint(transform.position + Vector3.up) : transform.position;

        internal bool TryGrip()
        {
            if (IsGripping) return true;
            if (IsClimbing || IsOnRope || !controller.isGrounded) return false;
            float nearest = float.PositiveInfinity;
            foreach (var candidate in FindObjectsByType<PrototypeGripBlock>())
            {
                float distance = (candidate.GetComponent<BoxCollider>().ClosestPoint(transform.position + Vector3.up) - transform.position - Vector3.up).sqrMagnitude;
                if (distance >= nearest || !candidate.CanReach(this, true)) continue;
                nearest = distance;
                grip = candidate;
            }
            if (grip == null) return false;
            gripOffset = transform.position - grip.transform.position;
            climbing = null;
            ClearRope();
            jumpPending = false;
            nextGripSend = 0;
            grip.Submit(this, true, Vector3.zero);
            return true;
        }

        internal void ReleaseGrip()
        {
            if (grip != null) grip.Submit(this, false, Vector3.zero);
            grip = null;
        }
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.TryGetComponent<CooperativeWeightBox>(out var box)) box.Push(this);
            if (hit.collider.TryGetComponent<ElephantFreePushBlock>(out var freeBlock)) freeBlock.TryPush(this, hit.moveDirection);
        }
        public void Warp(Vector3 position){ClearRope();GetComponent<PrototypeSwimmer>()?.ResetState();ClearPendingInput();climbing=null;climbReattachAt=0;verticalSpeed=0;jumpsUsed=0;controller.enabled=false;transform.position=position;controller.enabled=true;}
        public void Ride(Vector3 delta){if(enabled&&controller.enabled&&!IsClimbing&&verticalSpeed<=0)controller.Move(delta);}
        public void ResetToSpawn()
        {
            ClearRope(); ClearPendingInput(); GetComponent<PrototypeSwimmer>()?.ResetState(); climbing=null; climbReattachAt=0; verticalSpeed=0; jumpsUsed=0;
            controller.enabled=false; transform.position=spawnPosition; controller.enabled=true;
        }
        internal const float RopeGripHeight = 1.25f;
        private PrototypeSwingRope rope, releasedRope;
        private float ropeDepth, ropeClimb, regrabAt;
        private Vector3 ropeMomentum;
        public bool IsOnRope => rope != null && rope.Ready;
        public Vector3 RopeGripPoint => IsOnRope ? rope.Point(ropeDepth) : transform.position + Vector3.up * RopeGripHeight;

        public void SetRopeClimb(bool up, bool down) => ropeClimb = (up ? 1f : 0f) - (down ? 1f : 0f);

        private void ClearRope()
        {
            rope = releasedRope = null;
            ropeMomentum = Vector3.zero;
            ropeClimb = 0f;
        }

        private bool MoveOnRope(Vector3 direction, float dt)
        {
            if (GetComponent<PrototypeAnimal>()?.Species != PrototypeSpecies.Monkey) return false;
            if (!IsOnRope) rope = null;
            if (rope == null && !controller.isGrounded && !IsClimbing)
            {
                Vector3 hand = transform.position + Vector3.up * RopeGripHeight;
                float best = .58f * .58f;
                foreach (var candidate in PrototypeSwingRope.Active)
                {
                    if (!candidate.Ready || (candidate == releasedRope && Time.time < regrabAt)) continue;
                    float depth = candidate.DepthAt(hand);
                    Vector3 grip = candidate.Point(depth);
                    float distance = (hand - grip).sqrMagnitude;
                    if (distance >= best || Physics.Linecast(hand, grip, ~(1 << gameObject.layer), QueryTriggerInteraction.Ignore))
                        continue;
                    best = distance;
                    rope = candidate;
                    ropeDepth = depth;
                }
                if (rope != null)
                {
                    verticalSpeed = 0f;
                    ropeMomentum = Vector3.zero;
                    jumpPending = false;
                }
            }
            if (rope == null) return false;
            if (jumpPending)
            {
                Vector3 release = rope.Velocity(ropeDepth);
                ropeMomentum = Vector3.ProjectOnPlane(release, Vector3.up);
                verticalSpeed = release.y + Mathf.Sqrt(2f * gravity * jumpHeight);
                releasedRope = rope;
                regrabAt = Time.time + .32f;
                rope = null;
                jumpPending = false;
                controller.Move((ropeMomentum + direction * speed + Vector3.up * verticalSpeed) * dt);
                return true;
            }

            float nextDepth = Mathf.Clamp(ropeDepth - ropeClimb * rope.ClimbSpeed * dt, .6f, rope.Length);
            Vector3 desiredFeet = rope.Point(nextDepth) - Vector3.up * RopeGripHeight;
            controller.Move(desiredFeet - transform.position);
            if ((transform.position - desiredFeet).sqrMagnitude < .04f) ropeDepth = nextDepth;
            else if ((transform.position - desiredFeet).sqrMagnitude > .7f)
            {
                releasedRope = rope;
                regrabAt = Time.time + .32f;
                rope = null;
                return false;
            }
            rope.Drive(this, direction);
            return true;
        }

        private bool ClearAt(Vector3 feet)
        {
            foreach(var hit in Physics.OverlapCapsule(feet+Vector3.up*(controller.radius+0.06f),feet+Vector3.up*(controller.height-controller.radius),controller.radius*0.95f,~0,QueryTriggerInteraction.Ignore))
                if(!hit.transform.IsChildOf(transform)) return false;
            return true;
        }
        private bool Climb(Vector3 direction,float dt)
        {
            if(climbing==null && Time.time>=climbReattachAt && TryGetComponent<PrototypeAnimal>(out var animal) && animal.Species==PrototypeSpecies.Monkey && direction.sqrMagnitude>0.1f)
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
            if(jumpPending)
            {
                Vector3 away=climbing.Normal;
                climbing=null;climbReattachAt=Time.time+.3f;jumpPending=false;
                verticalSpeed=Mathf.Sqrt(2f*gravity*jumpHeight);
                controller.Move((away*speed*.5f+Vector3.up*verticalSpeed)*dt);
                return true;
            }
            verticalSpeed=0;
            // The input axes retain their screen meaning while the monkey is attached.
            Vector3 tangent=Vector3.Cross(Vector3.up,climbing.Normal).normalized;
            Vector3 cameraRight=movementCamera!=null?Vector3.ProjectOnPlane(movementCamera.right,Vector3.up).normalized:Vector3.right;
            if(Vector3.Dot(tangent,cameraRight)<0)tangent=-tangent;
            Vector2 climbAxes=Vector2.ClampMagnitude(moveInput,1f);
            Vector3 lateral=climbing.ConstrainWallDelta(transform.position,tangent*climbAxes.x*climbSpeed*dt,controller.radius);
            controller.Move(lateral);
            if(climbAxes.y<=0f)
            {
                float floor=climbing.GetComponent<BoxCollider>().bounds.min.y;
                if(climbAxes.y<0f)controller.Move(Vector3.down*Mathf.Min(-climbAxes.y*climbSpeed*dt,Mathf.Max(0f,transform.position.y-floor)));
                return true;
            }
            Vector3 target=climbing.Landing+Vector3.Project(transform.position-climbing.Landing,tangent);
            if(transform.position.y>=target.y-0.05f)
            {
                if(ClearAt(target) && Physics.Raycast(target+Vector3.up*0.1f,Vector3.down,0.2f,~(1<<gameObject.layer),QueryTriggerInteraction.Ignore))
                { controller.Move(target-transform.position); if((transform.position-target).sqrMagnitude<0.01f) { climbing=null; climbEndUntil=Time.time+.54f; } }
                return true;
            }
            controller.Move(Vector3.up*Mathf.Min(climbAxes.y*climbSpeed*dt,target.y-transform.position.y));
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
            ClearRope(); ClearPendingInput(); climbing=null;
            verticalSpeed = 0f;
        }

        private void Update()
        {
            if(GetComponent<AnimalMechanicsActor>()?.Locked==true){ClearRope();ClearPendingInput();return;}
            if (transform.position.y < resetHeight)
            {
                deathUntil = Time.time + .65f;
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

            if (grip != null)
            {
                if (!grip.CanReach(this, false)) ReleaseGrip();
                else
                {
                    jumpPending = false;
                    gripDirection = Vector3.Dot(direction, Facing.forward);
                    if (Time.unscaledTime >= nextGripSend)
                    {
                        grip.Submit(this, true, direction);
                        nextGripSend = Time.unscaledTime + .035f;
                    }
                    controller.Move(Vector3.ProjectOnPlane(grip.transform.position + gripOffset - transform.position, Vector3.up));
                    return;
                }
            }

            float deltaTime = Time.deltaTime;
            if(GetComponent<PrototypeSwimmer>()?.Move(direction,deltaTime,controller)==true){jumpPending=false;climbing=null;verticalSpeed=0;if(visual!=null&&direction.sqrMagnitude>.0001f)visual.rotation=Quaternion.RotateTowards(visual.rotation,Quaternion.LookRotation(direction),turnSpeed*deltaTime);return;}
            if (MoveOnRope(direction, deltaTime)) return;
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
                (direction * speed + ropeMomentum + Vector3.up * verticalSpeed) * deltaTime);
            if ((collisions & CollisionFlags.Above)!=0 && verticalSpeed>0) verticalSpeed=0;
            if (!jump && verticalSpeed <= 0f && (collisions & CollisionFlags.Below) != 0)
            {
                verticalSpeed = -2f;
                ropeMomentum = Vector3.zero;
                jumpsUsed = 0;
            }

            if (visual != null && direction.sqrMagnitude > 0.0001f)
                visual.rotation = Quaternion.RotateTowards(visual.rotation,
                    Quaternion.LookRotation(direction), turnSpeed * deltaTime);
        }
    }
}



