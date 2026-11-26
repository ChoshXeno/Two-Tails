using UnityEngine;

namespace TwoTails.Prototype
{
    // Shares the same visual between local players, remote players, and lobby displays.
    internal sealed class MonkeyPresentation : MonoBehaviour
    {
        private static GameObject prefab;
        private static GameObject foxPrefab;
        private static GameObject otterPrefab;
        private static GameObject beaverPrefab;
        private static GameObject rabbitPrefab;
        private static GameObject elephantPrefab;
        private PrototypeSpecies modelSpecies;
        private GameObject model;
        private Animator animator;
        private string current;
        private float releaseAt;
        private bool rabbitRejumpRestarted;
        private Transform foxPaw;
        private Transform[] foxPawChain;
        private Vector3 lastVisualPosition;
        private bool hasVisualPosition;
        private Transform observedBlock;
        private Vector3 lastBlockPosition;
        private readonly Transform[] mantlePaws = new Transform[2];
        private readonly Transform[][] mantleChains = new Transform[2][];
        private static readonly int GripSpeed = Animator.StringToHash("GripSpeed");

        internal static void Show(GameObject owner, Transform mount, Renderer placeholder,
            PrototypeSpecies species, PrototypeMotion motion, bool lobby = false)
        {
            bool monkey = species == PrototypeSpecies.Monkey;
            bool fox = species == PrototypeSpecies.Fox;
            bool otter = species == PrototypeSpecies.Otter;
            bool beaver = species == PrototypeSpecies.Beaver;
            bool rabbit = species == PrototypeSpecies.Rabbit;
            bool elephant = species == PrototypeSpecies.Elephant;
            bool supported = monkey || fox || otter || beaver || rabbit || elephant;
            var visual = owner.GetComponent<MonkeyPresentation>();
            if (visual == null)
            {
                if (!supported) return;
                visual = owner.AddComponent<MonkeyPresentation>();
            }

            if (supported && visual.model != null && visual.modelSpecies != species)
            {
                visual.model.SetActive(false);
                Destroy(visual.model);
                visual.model = null;
                visual.current = null;
                visual.releaseAt = 0f;
            }

            if (supported && visual.model == null)
            {
                if (monkey && prefab == null) prefab = Resources.Load<GameObject>("TwoTailsMonkeyVisual");
                if (fox && foxPrefab == null) foxPrefab = Resources.Load<GameObject>("TwoTailsFoxVisual");
                if (otter && otterPrefab == null) otterPrefab = Resources.Load<GameObject>("TwoTailsOtterVisual");
                if (beaver && beaverPrefab == null) beaverPrefab = Resources.Load<GameObject>("TwoTailsBeaverVisual");
                if (rabbit && rabbitPrefab == null) rabbitPrefab = Resources.Load<GameObject>("TwoTailsRabbitVisual");
                if (elephant && elephantPrefab == null) elephantPrefab = Resources.Load<GameObject>("TwoTailsElephantVisual");
                var selectedPrefab = elephant ? elephantPrefab : monkey ? prefab : fox ? foxPrefab : otter ? otterPrefab : beaver ? beaverPrefab : rabbitPrefab;
                if (selectedPrefab == null)
                {
                    if (placeholder != null) placeholder.enabled = true;
                    return;
                }
                visual.model = Instantiate(selectedPrefab, mount);
                if (rabbit && !lobby)
                    visual.model.transform.localScale *= 2f;
                visual.modelSpecies = species;
                visual.hasVisualPosition = false;
                visual.model.transform.localRotation = Quaternion.identity;
                visual.animator = visual.model.GetComponentInChildren<Animator>();
                if (monkey)
                    foreach (var bone in visual.animator.GetComponentsInChildren<Transform>())
                    {
                        int index = bone.name == "frontFoot.L" ? 0 : bone.name == "frontFoot.R" ? 1 : -1;
                        if (index < 0) continue;
                        visual.mantlePaws[index] = bone;
                        visual.mantleChains[index] = new[] { bone.parent, bone.parent.parent, bone.parent.parent.parent };
                    }
                if (fox)
                {
                    foreach (var bone in visual.animator.GetComponentsInChildren<Transform>())
                    {
                        if (bone.name != "frontFoot.L") continue;
                        visual.foxPaw = bone;
                        visual.foxPawChain = new[] { bone.parent, bone.parent.parent, bone.parent.parent.parent };
                        break;
                    }
                }
            }

            if (visual.model == null) return;
            // The placeholder mount is centred on the capsule and can change scale
            // during an exchange. Keep the model's authored feet at the player root.
            visual.model.transform.localPosition = lobby
                ? Vector3.zero
                : mount.InverseTransformPoint(owner.transform.position);
            bool wasActive = visual.model.activeSelf;
            visual.model.SetActive(supported);
            if (placeholder != null) placeholder.enabled = !supported;
            if (!lobby)
            {
                var marker = mount.Find("FacingMarker");
                if (marker != null && marker.TryGetComponent<Renderer>(out var markerRenderer))
                    markerRenderer.enabled = !supported;
            }
            if (!supported) return;
            if (monkey && !lobby && owner.TryGetComponent<PrototypeMotor>(out var motor) &&
                motor.ClimbSurface is { } surface)
            {
                Vector3 normal = surface.Normal;
                var wall = surface.GetComponent<BoxCollider>();
                Vector3 feet = owner.transform.position;
                Vector3 onWall = wall.ClosestPoint(feet + Vector3.up);
                float separation = Vector3.Dot(feet - onWall, normal);
                visual.model.transform.rotation = Quaternion.LookRotation(-normal);
                float wallWeight = motor.IsMantling ? 1f - Mathf.SmoothStep(0f, 1f, motor.MantleProgress / .5f) : 1f;
                visual.model.transform.position = feet + normal * ((.34f - separation) * wallWeight);
                if (motor.IsMantling && motor.MantleProgress < .66f)
                {
                    Vector3 side = Vector3.Cross(Vector3.up, normal) * .16f;
                    float planted = 1f - Mathf.SmoothStep(0f, 1f, (motor.MantleProgress - .48f) / .18f);
                    for (int i = 0; i < 2; i++)
                        if (visual.mantlePaws[i] != null)
                        {
                            ReachPoint(visual.mantlePaws[i], visual.mantleChains[i],
                                motor.MantleEdge + side * (i == 0 ? 1f : -1f) + Vector3.up*.025f, planted);
                            var paw = visual.mantlePaws[i];
                            var toe = paw.Find(i == 0 ? "frontToe.L" : "frontToe.R");
                            if (toe != null)
                            {
                                Vector3 forward = -normal + Vector3.up * .25f;
                                var correction = Quaternion.FromToRotation(toe.position - paw.position, forward);
                                paw.rotation = Quaternion.Slerp(Quaternion.identity, correction, planted) * paw.rotation;
                            }
                        }
                }
            }
            else if (!otter || lobby)
            {
                visual.model.transform.localRotation = Quaternion.identity;
            }
            if (!lobby && owner.TryGetComponent<PrototypeMotor>(out var movement))
                visual.UpdateMovementVisual(owner.transform.position, mount, motion, otter, movement.GrippedBlock);
            if (!wasActive) { visual.current = null; visual.releaseAt = 0f; }
            visual.Play(lobby ? "Sit" : motion.ToString());
            if (!lobby && monkey && owner.TryGetComponent<PrototypeMotor>(out var ropeMotor) && ropeMotor.IsOnRope)
                visual.HoldRope(ropeMotor, mount);
            if (!lobby && fox && motion == PrototypeMotion.Interact &&
                owner.TryGetComponent<PrototypeMotor>(out var foxMotor))
                visual.ReachLever(foxMotor);
        }

        private void HoldRope(PrototypeMotor motor, Transform mount)
        {
            if (mantlePaws[0] == null || mantlePaws[1] == null) return;
            Vector3 up = motor.RopeGripUp;
            if (up.sqrMagnitude < .5f) up = Vector3.up;
            Vector3 forward = Vector3.ProjectOnPlane(mount.forward, up);
            if (forward.sqrMagnitude < .01f) forward = Vector3.ProjectOnPlane(mount.right, up);
            // Change the visual only; the motor keeps its existing rope position and momentum.
            model.transform.rotation = Quaternion.LookRotation(forward.normalized, up);
            Vector3 grip = motor.RopeGripPoint;
            Vector3 centre = (mantlePaws[0].position + mantlePaws[1].position) * .5f;
            model.transform.position += grip - centre;
            for (int i = 0; i < 2; i++)
            {
                Vector3 target = grip + up * (i == 0 ? .055f : -.055f);
                ReachPoint(mantlePaws[i], mantleChains[i], target, 1f);
            }
        }

        private void UpdateMovementVisual(Vector3 position, Transform mount, PrototypeMotion motion, bool otter, Transform block)
        {
            Vector3 velocity = hasVisualPosition && Time.deltaTime > 0f
                ? (position - lastVisualPosition) / Time.deltaTime : Vector3.zero;
            lastVisualPosition = position;
            hasVisualPosition = true;
            if (velocity.sqrMagnitude > 100f) velocity = Vector3.zero;

            bool gripping = motion is PrototypeMotion.Push or PrototypeMotion.Pull;
            float speed = gripping ? Vector3.ProjectOnPlane(velocity, Vector3.up).magnitude : 0f;
            if (block != null)
            {
                speed = observedBlock == block && Time.deltaTime > 0f
                    ? Vector3.ProjectOnPlane(block.position - lastBlockPosition, Vector3.up).magnitude / Time.deltaTime : 0f;
                lastBlockPosition = block.position;
            }
            observedBlock = block;
            if (animator.parameters.Length > 0)
                animator.SetFloat(GripSpeed, speed < .07f ? 0f : speed, .08f, Time.deltaTime);

            if (!otter) return;
            Quaternion target = Quaternion.identity;
            if (motion == PrototypeMotion.Dive_Loop && velocity.y > .08f)
            {
                Vector3 horizontal = Vector3.ProjectOnPlane(velocity, Vector3.up);
                Vector3 forward = horizontal.sqrMagnitude > .02f ? horizontal.normalized : mount.forward;
                float pitch = Mathf.Min(65f, Mathf.Atan2(velocity.y, horizontal.magnitude) * Mathf.Rad2Deg);
                Vector3 direction = forward * Mathf.Cos(pitch * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(pitch * Mathf.Deg2Rad);
                target = Quaternion.Inverse(mount.rotation) * Quaternion.LookRotation(direction, Vector3.up);
            }
            model.transform.localRotation = Quaternion.Slerp(model.transform.localRotation, target,
                1f - Mathf.Exp(-10f * Time.deltaTime));
        }

        private void ReachLever(PrototypeMotor motor)
        {
            if (foxPaw == null || motor.InteractionLever == null) return;
            float phase = motor.InteractionProgress;
            float reach = Mathf.SmoothStep(0f, 1f, phase / .28f) *
                (1f - Mathf.SmoothStep(0f, 1f, (phase - .67f) / .33f));
            ReachPoint(foxPaw, foxPawChain, motor.InteractionLever.PawContact, reach);
        }

        private static void ReachPoint(Transform paw, Transform[] chain, Vector3 contact, float reach)
        {
            Vector3 target = Vector3.Lerp(paw.position, contact, reach);
            for (int pass = 0; pass < 4; pass++)
            {
                foreach (Transform joint in chain)
                {
                    Vector3 from = paw.position - joint.position;
                    Vector3 to = target - joint.position;
                    if (from.sqrMagnitude < 1e-6f || to.sqrMagnitude < 1e-6f) continue;
                    Quaternion turn = Quaternion.FromToRotation(from, to);
                    joint.rotation = Quaternion.RotateTowards(Quaternion.identity, turn, 35f) * joint.rotation;
                }
            }
        }

        private void Play(string requested)
        {
            if (modelSpecies is PrototypeSpecies.Fox or PrototypeSpecies.Otter or PrototypeSpecies.Beaver or PrototypeSpecies.Rabbit or PrototypeSpecies.Elephant)
            {
                PlayFox(requested);
                return;
            }
            if (requested == "Jump") requested = "Jump_Start";
            if (requested == "Fall") requested = "Jump_Air";
            if (requested == "Climb") requested = "Climb_Loop";
            if (requested == "Rope") requested = "Vine_Hang_Idle";
            if (requested == "Ability") requested = "Idle";

            bool ground = requested == "Idle" || requested == "Run";
            if (ground && Time.time < releaseAt) return;
            if (ground && (current == "Jump_Air" || current == "Jump_Start"))
            {
                requested = "Jump_Land";
                releaseAt = Time.time + .25f;
            }
            if (requested == "Jump_Start" && (current == "Jump_Air" ||
                (current == "Jump_Start" && Time.time >= releaseAt)))
                requested = "Jump_Air";
            if (requested == current) return;
            if (requested == "Jump_Start") releaseAt = Time.time + .26f;
            current = requested;
            animator.CrossFadeInFixedTime(modelSpecies + "_" + requested, .08f);
        }

        private void PlayFox(string motion)
        {
            bool rabbitRejump = modelSpecies == PrototypeSpecies.Rabbit && motion == "Rejump";
            bool restartRabbitTakeoff = rabbitRejump && !rabbitRejumpRestarted;
            if (modelSpecies == PrototypeSpecies.Rabbit)
            {
                if (rabbitRejump) motion = "Jump_Start";
                else rabbitRejumpRestarted = false;
            }
            bool locomotion = motion == "Idle" || motion == "Run";
            string next = motion;
            if (restartRabbitTakeoff)
                next = "Jump_Start";
            else if (motion == "Jump")
                next = current == "Jump_Air" || (current == "Jump_Start" && Time.time >= releaseAt)
                    ? "Jump_Air" : "Jump_Start";
            else if (motion == "Fall")
                next = "Jump_Air";
            else if (locomotion && (current == "Jump_Start" || current == "Jump_Air" || current == "Rejump"))
                next = "Jump_Land";
            else if (locomotion && current == "Jump_Land" && Time.time < releaseAt)
                return;

            if (next == current && !restartRabbitTakeoff) return;
            int state = Animator.StringToHash("Base Layer." + modelSpecies + "_" + next);
            if (!animator.HasState(0, state)) return;
            // Re-enter the same takeoff state at time zero for Rabbit's second jump.
            // Rabbit's gameplay state remains Jump_Start through the rest of airtime.
            if (restartRabbitTakeoff)
            {
                animator.Play(state, 0, 0f);
                current = next;
                rabbitRejumpRestarted = true;
                releaseAt = Time.time + .16f;
                return;
            }
            if (next == "Jump_Start") releaseAt = Time.time + .20f;
            if (next == "Jump_Land") releaseAt = Time.time + .22f;
            float blend = next is "Jump_Start" or "Jump_Land" or "Rejump" ? .07f : .13f;
            if (locomotion && next != "Jump_Land" && animator.IsInTransition(0) &&
                current is "Idle" or "Run") return;
            animator.CrossFadeInFixedTime(state, blend, 0);
            current = next;
        }
    }
}

