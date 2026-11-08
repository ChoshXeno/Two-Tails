using System;
using System.Linq;
using TwoTails.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace TwoTails.Editor
{
    public static class PrototypeGreyboxBuilder
    {
        private const string Root = "Assets/prototype";
        internal const string ScenePath = Root + "/Scenes/Prototype_Greybox.unity";
        internal const string PrefabPath = Root + "/Prefabs/PrototypePlayer.prefab";
        private const string InputPath = Root + "/Input/TwoTailsInput.inputactions";
        private const string UiInputPath = "Assets/InputSystem_Actions.inputactions";

        [MenuItem("Tools/Two Tails/Build Prototype Greybox")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before building the prototype greybox.");

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null ||
                SceneManager.GetSceneByPath(ScenePath).isLoaded)
            {
                PrototypeSceneSetup.UpgradeExisting();
                PrototypeSceneSetup.ConfigureWindowsProfile();
                return;
            }

            InputActionAsset input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            var uiActions = AssetDatabase.LoadAllAssetsAtPath(UiInputPath).OfType<InputActionReference>()
                .Where(reference => reference.action?.actionMap.name == "UI")
                .GroupBy(reference => reference.action.name)
                .ToDictionary(group => group.Key, group => group.First());
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Sprite circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (input == null || !uiActions.ContainsKey("Point") || !uiActions.ContainsKey("Click") ||
                shader == null || circle == null || font == null)
                throw new InvalidOperationException("The prototype requires the imported input assets, URP/Lit and built-in uGUI resources.");

            EnsureFolder("Scenes");
            EnsureFolder("Prefabs");
            EnsureFolder("Materials");
            var floor = GetMaterial("GreyboxFloor", new Color(0.42f, 0.46f, 0.50f), shader);
            var wall = GetMaterial("GreyboxWall", new Color(0.62f, 0.66f, 0.70f), shader);
            var blue = GetMaterial("PrototypeBlue", new Color(0.12f, 0.40f, 0.94f), shader);
            var yellow = GetMaterial("PrototypeYellow", new Color(1f, 0.78f, 0.08f), shader);
            var red = GetMaterial("PrototypeRed", new Color(0.86f, 0.14f, 0.10f), shader);

            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                BuildRoom(floor, wall, yellow, red);
                GameObject prefab = GetPlayerPrefab(input, blue, yellow);
                var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                player.transform.position = new Vector3(0f, 0.1f, -5f);
                BuildCamera(player);
                BuildUI(uiActions["Point"], uiActions["Click"], circle, font);
                PrototypeSceneSetup.Configure(scene);
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new InvalidOperationException("Unity could not save the prototype greybox scene.");
                PrototypeSceneSetup.ConfigureWindowsProfile();
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void EnsureFolder(string name)
        {
            if (!AssetDatabase.IsValidFolder(Root + "/" + name))
                AssetDatabase.CreateFolder(Root, name);
        }

        private static Material GetMaterial(string name, Color color, Shader shader)
        {
            string path = Root + "/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            material = new Material(shader);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.2f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void BuildRoom(Material floor, Material wall, Material yellow, Material red)
        {
            Transform room = new GameObject("Greybox").transform;
            Cube(room, "Ground", new Vector3(0, -0.25f, 0), new Vector3(20, 0.5f, 16), floor);
            Cube(room, "NorthWall", new Vector3(0, 1, 7.5f), new Vector3(20, 2, 1), wall);
            Cube(room, "SouthWall", new Vector3(0, 1, -7.5f), new Vector3(20, 2, 1), wall);
            Cube(room, "WestWall", new Vector3(-9.5f, 1, 0), new Vector3(1, 2, 14), wall);
            Cube(room, "EastWall", new Vector3(9.5f, 1, 0), new Vector3(1, 2, 14), wall);
            Cube(room, "LeftObstacle", new Vector3(-4, 0.6f, -1), new Vector3(2, 1.2f, 3), wall);
            Cube(room, "RightObstacle", new Vector3(4, 0.6f, 1), new Vector3(2, 1.2f, 3), wall);
            Cube(room, "LeftPlatePlaceholder", new Vector3(-2.4f, 0.03f, 4.8f),
                new Vector3(1.4f, 0.06f, 1.4f), yellow, false);
            Cube(room, "RightPlatePlaceholder", new Vector3(2.4f, 0.03f, 4.8f),
                new Vector3(1.4f, 0.06f, 1.4f), yellow, false);
            Cube(room, "DoorPlaceholder", new Vector3(0, 1, 6.6f), new Vector3(2.4f, 2, 0.35f), red);

            var sun = new GameObject("Directional Light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        internal static GameObject Cube(Transform parent, string name, Vector3 position, Vector3 scale,
            Material material, bool solid = true)
        {
            return Primitive(PrimitiveType.Cube, parent, name, position, scale, material, solid);
        }

        private static GameObject Primitive(PrimitiveType type, Transform parent, string name,
            Vector3 position, Vector3 scale, Material material, bool solid)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid)
                UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
            return obj;
        }

        private static GameObject GetPlayerPrefab(InputActionAsset input, Material blue, Material yellow)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
                return existing;

            var root = new GameObject("PrototypePlayer");
            var controller = root.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f;
            controller.center = Vector3.up;
            controller.skinWidth = 0.04f;
            controller.minMoveDistance = 0f;

            GameObject visual = Primitive(PrimitiveType.Capsule, root.transform, "Visual", Vector3.up,
                new Vector3(0.8f, 1, 0.8f), blue, false);
            Cube(visual.transform, "FacingMarker", new Vector3(0, 0.35f, 0.48f),
                new Vector3(0.22f, 0.18f, 0.34f), yellow, false);
            SetReference(root.AddComponent<PrototypeMotor>(), "visual", visual.transform);
            SetReference(root.AddComponent<PrototypeLocalInput>(), "actions", input);
            PrototypeSceneSetup.ConfigurePlayerPrefab(root);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab == null)
                throw new InvalidOperationException("Unity could not save PrototypePlayer.prefab.");
            return prefab;
        }

        private static void BuildCamera(GameObject player)
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(player.transform.position + new Vector3(0, 10, -8),
                Quaternion.LookRotation(new Vector3(0, -9, 8)));
            cameraObject.GetComponent<Camera>().fieldOfView = 54f;
            SetReference(cameraObject.AddComponent<PrototypeFollowCamera>(), "target", player.transform);
            SetReference(player.GetComponent<PrototypeMotor>(), "movementCamera", cameraObject.transform);
        }

        private static void BuildUI(InputActionReference point, InputActionReference click,
            Sprite circle, Font font)
        {
            var canvas = new GameObject("Prototype HUD", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform safeArea = Rect("SafeArea", canvas.transform);
            safeArea.anchorMin = Vector2.zero;
            safeArea.anchorMax = Vector2.one;
            safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
            safeArea.gameObject.AddComponent<PrototypeSafeArea>();

            var stickBase = Corner("StickBase", safeArea, Vector2.zero,
                new Vector2(155, 155), new Vector2(224, 224));
            AddCircle(stickBase, circle, new Color(0.05f, 0.10f, 0.18f, 0.75f), false);
            var handle = Corner("StickHandle", stickBase, new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(104, 104));
            AddCircle(handle, circle, new Color(0.45f, 0.78f, 1f), true);
            var stick = handle.gameObject.AddComponent<PrototypeScreenStick>();
            stick.controlPath = "<Gamepad>/leftStick";
            stick.movementRange = 68f;

            var button = Corner("InteractButton", safeArea, Vector2.right,
                new Vector2(-155, 155), new Vector2(174, 174));
            AddCircle(button, circle, new Color(0.08f, 0.20f, 0.34f, 0.94f), true);
            button.gameObject.AddComponent<PrototypeScreenButton>().controlPath = "<Gamepad>/buttonSouth";
            Label("InteractLabel", button, font, "Interact", 28);

            var hint = Corner("Hint", safeArea, new Vector2(0.5f, 1), new Vector2(0, -48), new Vector2(890, 62));
            Label("Controls", hint, font, "WASD / Arrow Keys / Drag Stick to Move, E to Interact", 30);
            var status = Corner("Status", safeArea, new Vector2(0.5f, 1), new Vector2(0, -94), new Vector2(420, 48));
            Label("InteractionCount", status, font, "", 26);

            var events = new GameObject("EventSystem", typeof(EventSystem));
            events.SetActive(false);
            var module = events.AddComponent<InputSystemUIInputModule>();
            module.actionsAsset = point.asset;
            module.point = point;
            module.leftClick = click;
            events.SetActive(true);
        }

        internal static RectTransform Rect(string name, Transform parent)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            return rect;
        }

        internal static RectTransform Corner(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            RectTransform rect = Rect(name, parent);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static void AddCircle(RectTransform rect, Sprite sprite, Color color, bool raycast)
        {
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycast;
        }

        internal static UnityEngine.UI.Text Label(string name, RectTransform parent, Font font, string value, int size)
        {
            RectTransform rect = Rect(name, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = rect.gameObject.AddComponent<UnityEngine.UI.Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = value;
            text.raycastTarget = false;
            var shadow = rect.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.9f);
            shadow.effectDistance = new Vector2(2, -2);
            return text;
        }

        internal static void SetReference(Component component, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
