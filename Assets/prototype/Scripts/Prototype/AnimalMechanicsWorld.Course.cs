using System;
using UnityEngine;

namespace TwoTails.Prototype
{
    public sealed partial class AnimalMechanicsWorld
    {
        public const int StrikeTarget = 31;
        [SerializeField, Min(.1f), Tooltip("Speed of the marked heavy block along its guide, in metres per second.")]
        private float heavyPushSpeed = 1.5f;
        private const float HeavyTravel = 1.5f;
        [SerializeField] private Transform savedRabbit, savedElephant, savedStrikeBoard;
        [SerializeField] private Renderer savedRabbitVisual, savedElephantVisual;
        [SerializeField] private BoxCollider savedHeavyBlock;
        private Transform strikeBoard;
        private BoxCollider heavyBlock;
        private Vector3 heavyStart, startC, startD;
        private AnimalMechanicsActor pushActor;
        private double pushUntil, nextPushRequest;
        private Material courseBlue;

        private static PrototypeSpecies DisplaySpecies(AnimalMechanicsState s, int i) =>
            (PrototypeSpecies)(i switch { 0 => s.SpeciesA, 1 => s.SpeciesB, 2 => s.SpeciesC, _ => s.SpeciesD });

        private static double DisplayBusy(AnimalMechanicsState s, int i) =>
            i switch { 0 => s.BusyA, 1 => s.BusyB, 2 => s.BusyC, _ => s.BusyD };

        private static void SetDisplay(ref AnimalMechanicsState s, int i, PrototypeSpecies species, Vector3 position, double busy)
        {
            switch (i)
            {
                case 0: s.SpeciesA = (byte)species; s.PositionA = position; s.BusyA = busy; break;
                case 1: s.SpeciesB = (byte)species; s.PositionB = position; s.BusyB = busy; break;
                case 2: s.SpeciesC = (byte)species; s.PositionC = position; s.BusyC = busy; break;
                case 3: s.SpeciesD = (byte)species; s.PositionD = position; s.BusyD = busy; break;
            }
        }

        private void ConfigureExtendedCourse()
        {
            if (savedRabbit != null && savedElephant != null && savedHeavyBlock != null && savedStrikeBoard != null)
            {
                displays = new[] { displays[0], displays[1], savedRabbit, savedElephant };
                displayVisuals = new[] { displayVisuals[0], displayVisuals[1], savedRabbitVisual, savedElephantVisual };
                startC = savedRabbit.position;
                startD = savedElephant.position;
                heavyBlock = savedHeavyBlock;
                heavyStart = heavyBlock.transform.position;
                strikeBoard = savedStrikeBoard;
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            CreateExtendedCourse();
        }

#if UNITY_EDITOR
        public void ConfigureExtendedCourseInEditor()
        {
            if (savedRabbit != null && savedElephant != null && savedHeavyBlock != null && savedStrikeBoard != null)
                return;
            if (transform.Find("Rabbit and Elephant Course") != null)
                throw new InvalidOperationException("Found an existing unbound animal course.");
            var oldDisplays = displays;
            var oldVisuals = displayVisuals;
            CreateExtendedCourse();
            savedRabbit = displays[2];
            savedElephant = displays[3];
            savedRabbitVisual = displayVisuals[2];
            savedElephantVisual = displayVisuals[3];
            savedHeavyBlock = heavyBlock;
            savedStrikeBoard = strikeBoard;
            displays = oldDisplays;
            displayVisuals = oldVisuals;
            const string path = "Assets/prototype/Materials/PrototypeCourseBlue.mat";
            var sharedBlue = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if (sharedBlue == null)
                UnityEditor.AssetDatabase.CreateAsset(courseBlue, path);
            else
            {
                foreach (var renderer in transform.Find("Rabbit and Elephant Course").GetComponentsInChildren<Renderer>())
                    if (renderer.sharedMaterial == courseBlue) renderer.sharedMaterial = sharedBlue;
                DestroyImmediate(courseBlue);
            }
            courseBlue = null;
            SetDisplayPreview(savedRabbitVisual, PrototypeSpecies.Rabbit);
            SetDisplayPreview(savedElephantVisual, PrototypeSpecies.Elephant);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private static void SetDisplayPreview(Renderer visual, PrototypeSpecies species)
        {
            string path = $"Assets/prototype/Materials/Prototype{species}Display.mat";
            var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(visual.sharedMaterial);
                material.SetColor("_BaseColor", PrototypeAnimal.ColorFor(species));
                UnityEditor.AssetDatabase.CreateAsset(material, path);
            }
            visual.sharedMaterial = material;
        }
#endif

        private void CreateExtendedCourse()
        {
            Material stone = displayVisuals[0].sharedMaterial;
            Material wood = installedLogs[0].GetComponent<Renderer>().sharedMaterial;
            courseBlue = new Material(stone);
            courseBlue.SetColor("_BaseColor", new Color(.12f, .35f, .85f));

            var template = displays[0];
            Array.Resize(ref displays, 4);
            Array.Resize(ref displayVisuals, 4);
            for (int i = 2; i < 4; i++)
            {
                string animal = i == 2 ? "Rabbit" : "Elephant";
                displays[i] = Instantiate(template, new Vector3(i == 2 ? -2.8f : 2.8f, .04f, -3.2f), Quaternion.identity, transform);
                displays[i].name = animal + " Display";
                displays[i].GetComponentInChildren<TextMesh>().text = animal;
                displayVisuals[i] = displays[i].Find("Visual").GetComponent<Renderer>();
            }
            startC = displays[2].position;
            startD = displays[3].position;

            var root = new GameObject("Rabbit and Elephant Course").transform;
            root.SetParent(transform, false);
            GameObject Box(string name, float x, float y, float z, float w, float h, float d, Material mat)
            {
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = name;
                box.transform.SetParent(root);
                box.transform.position = new Vector3(x, y, z);
                box.transform.localScale = new Vector3(w, h, d);
                box.GetComponent<Renderer>().sharedMaterial = mat;
                return box;
            }
            void Label(string caption, float x, float z)
            {
                var text = new GameObject(caption).AddComponent<TextMesh>();
                text.transform.SetParent(root);
                text.transform.position = new Vector3(x, 2.4f, z);
                text.transform.rotation = Quaternion.Euler(0, 180, 0);
                text.text = caption;
                text.anchor = TextAnchor.MiddleCenter;
                text.characterSize = .025f;
                text.fontSize = 48;
            }

            Box("Rabbit Entry", -3, -.25f, -15, 3, .5f, 4, stone);
            Box("High Platform", -3, 1.25f, -18.5f, 3, 3.5f, 3, stone);
            Box("Far Platform", -3, 2.75f, -25.7f, 3, .5f, 3, stone);
            Box("Rabbit Landing", -3, -.25f, -29.2f, 3, .5f, 4, stone);
            Label("Rabbit: High Jump / Long Jump", -3, -15);

            Box("Elephant Entry", 2, -.25f, -16, 3, .5f, 6, stone);
            Box("Push Access Shelf", 4.8f, -.25f, -18.75f, 1.15f, .5f, 4.5f, stone);
            Box("Elephant Landing", 2, -.25f, -26, 3, .5f, 10, stone);
            heavyStart = new Vector3(3.5f, -1f, -20);
            heavyBlock = Box("Heavy Gap Block", heavyStart.x, heavyStart.y, heavyStart.z, 1.45f, 2, 2, wood).GetComponent<BoxCollider>();
            Box("Heavy Block Guide Bottom", 3, -2.25f, -20, 4, .5f, 2, stone);
            
            Box("Push Access Entry", 3.85f, -.25f, -17, .85f, .5f, 1, stone);
            Label("Elephant: Push Heavy Block", 2, -16);

            const float opening = .46f;
            const float headroom = 1.3f;
            const float sideWidth = (3f - opening) / 2f;
            const float sideOffset = opening / 2f + sideWidth / 2f;
            void Passage(float z, float length, Material material)
            {
                Box("Passage Left Wall", 2-sideOffset, 3, z, sideWidth, 6, length, material);
                Box("Passage Right Wall", 2+sideOffset, 3, z, sideWidth, 6, length, material);
                Box("Passage Roof", 2, (6+headroom)/2, z, opening, 6-headroom, length, material);
            }
            Box("Blue Barricade", 2-sideOffset, 3, -24, sideWidth, 6, .45f, courseBlue);
            Box("Passage Right Wall", 2+sideOffset, 3, -24, sideWidth, 6, .45f, courseBlue);
            Box("Passage Roof", 2, (6+headroom)/2, -24, opening, 6-headroom, .45f, courseBlue);
            strikeBoard = Box("Strike Board", 2, headroom/2, -23.7f, opening, headroom, .15f, wood).transform;
            Passage(-28, 4, stone);
            Label("Elephant: Strike / Rabbit: Crawlspace", 2, -22);

            var lamp = new GameObject("Animal Course Lamp").AddComponent<Light>();
            lamp.transform.SetParent(root);
            lamp.transform.position = new Vector3(0, 7, -21);
            lamp.transform.rotation = Quaternion.Euler(90, 0, 0);
            lamp.type = LightType.Spot;
            lamp.spotAngle = 120;
            lamp.range = 20;
            lamp.intensity = 35;
        }

        private void ApplyCourseState()
        {
            heavyBlock.transform.position = heavyStart - Vector3.right * State.HeavyOffset;
            strikeBoard.gameObject.SetActive(!State.BoardBroken);
        }

        internal void RequestHeavyPush(PrototypeMotor motor, Vector3 direction)
        {
            if (heavyBlock == null || direction.x >= -.65f || Now < nextPushRequest ||
                motor.GetComponent<PrototypeAnimal>()?.Species != PrototypeSpecies.Elephant) return;
            var actor = motor.GetComponent<AnimalMechanicsActor>();
            if (!NearHeavyFace(actor)) return;
            nextPushRequest = Now + .06;
            if (Authority) AcceptHeavyPush(actor);
            else actor.GetComponent<PrototypeNetworkInteraction>()?.RequestHeavyPush();
        }

        private bool NearHeavyFace(AnimalMechanicsActor actor)
        {
            if (actor == null) return false;
            var p = actor.transform.position;
            var bounds = heavyBlock.bounds;
            return p.x >= bounds.max.x && p.x <= bounds.max.x + .62f &&
                   p.y < bounds.max.y + .20f && p.y + 1.8f > bounds.min.y &&
                   Mathf.Abs(p.z - bounds.center.z) <= bounds.extents.z + .1f;
        }

        internal void AcceptHeavyPush(AnimalMechanicsActor actor)
        {
            if (!Authority || !Valid(actor) || actor.Locked || !NearHeavyFace(actor) ||
                actor.GetComponent<PrototypeAnimal>().Species != PrototypeSpecies.Elephant ||
                Vector3.Dot(actor.GetComponent<PrototypeMotor>().Facing.forward, Vector3.left) < .65f) return;
            pushActor = actor;
            pushUntil = Now + .16;
        }

        private void StepHeavyPush()
        {
            if (Now >= pushUntil || !Valid(pushActor) || pushActor.Locked || !NearHeavyFace(pushActor) ||
                pushActor.GetComponent<PrototypeAnimal>().Species != PrototypeSpecies.Elephant) return;
            float step = Mathf.Min(HeavyTravel - State.HeavyOffset, heavyPushSpeed * Time.fixedDeltaTime);
            if (step <= 0) return;
            Physics.SyncTransforms();
            var bounds = heavyBlock.bounds;
            Vector3 half = bounds.extents - Vector3.one * .015f;
            half.x += step * .5f;
            foreach (var collider in Physics.OverlapBox(bounds.center - Vector3.right * (step * .5f),
                         half, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
                if (collider != heavyBlock) return;
            var next = State;
            next.HeavyOffset += step;
            State = next;
            ApplyCourseState();
            Publish();
        }
    }
}
