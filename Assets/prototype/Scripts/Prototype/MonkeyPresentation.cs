using UnityEngine;

namespace TwoTails.Prototype
{
    // Shares the same visual between local players, remote players, and lobby displays.
    internal sealed class MonkeyPresentation : MonoBehaviour
    {
        private static GameObject prefab;
        private GameObject model;
        private Animator animator;
        private string current;
        private float releaseAt;

        internal static void Show(GameObject owner, Transform mount, Renderer placeholder,
            bool monkey, PrototypeMotion motion, bool lobby = false)
        {
            var visual = owner.GetComponent<MonkeyPresentation>();
            if (visual == null)
            {
                if (!monkey) return;
                visual = owner.AddComponent<MonkeyPresentation>();
            }

            if (monkey && visual.model == null)
            {
                if (prefab == null) prefab = Resources.Load<GameObject>("TwoTailsMonkeyVisual");
                if (prefab == null) return;
                visual.model = Instantiate(prefab, mount);
                visual.model.transform.localRotation = Quaternion.identity;
                visual.animator = visual.model.GetComponentInChildren<Animator>();
            }

            if (visual.model == null) return;
            // The placeholder mount is centred on the capsule and can change scale
            // during an exchange. Keep the model's authored feet at the player root.
            visual.model.transform.localPosition = lobby
                ? Vector3.zero
                : mount.InverseTransformPoint(owner.transform.position);
            bool wasActive = visual.model.activeSelf;
            visual.model.SetActive(monkey);
            if (placeholder != null) placeholder.enabled = !monkey;
            if (!lobby)
            {
                var marker = mount.Find("FacingMarker");
                if (marker != null && marker.TryGetComponent<Renderer>(out var markerRenderer))
                    markerRenderer.enabled = !monkey;
            }
            if (!monkey) return;
            if (!lobby && owner.TryGetComponent<PrototypeMotor>(out var motor) &&
                motor.ClimbSurface is { } surface)
            {
                Vector3 normal = surface.Normal;
                var wall = surface.GetComponent<BoxCollider>();
                Vector3 feet = owner.transform.position;
                Vector3 onWall = wall.ClosestPoint(feet + Vector3.up);
                float separation = Vector3.Dot(feet - onWall, normal);
                visual.model.transform.rotation = Quaternion.LookRotation(-normal);
                visual.model.transform.position = feet + normal * (.34f - separation);
            }
            else
            {
                visual.model.transform.localRotation = Quaternion.identity;
            }
            if (!wasActive) { visual.current = null; visual.releaseAt = 0f; }
            visual.Play(lobby ? "Sit" : motion.ToString());
        }

        private void Play(string requested)
        {
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
            animator.CrossFadeInFixedTime("Monkey_" + requested, .08f);
        }
    }
}
