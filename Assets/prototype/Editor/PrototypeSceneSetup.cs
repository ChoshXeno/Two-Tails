using System;
using System.Linq;
using TwoTails.Prototype;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using static TwoTails.Editor.PrototypeGreyboxBuilder;

namespace TwoTails.Editor
{
    internal static class PrototypeSceneSetup
    {
        private const string Materials = "Assets/prototype/Materials/";

        internal static void UpgradeExisting()
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool openedForSetup = !scene.isLoaded;
            if (openedForSetup)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            try
            {
                GameObject[] roots = scene.GetRootGameObjects();
                if (roots.Any(root => root.GetComponentInChildren<PrototypePlateGate>(true) != null))
                {
                    Debug.Log("Prototype greybox is already configured.");
                    return;
                }

                if (scene.isDirty)
                    throw new InvalidOperationException("Save the current prototype scene before applying the setup.");

                SceneManager.SetActiveScene(scene);
                if (roots.Any(root => root.GetComponent<PrototypeDebugPlayers>() != null))
                    ConfigureMechanisms(roots);
                else
                    Configure(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Unity could not save the prototype greybox.");
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                if (openedForSetup)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        internal static void Configure(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            Transform room = roots.Single(root => root.name == "Greybox").transform;
            Transform panel = room.Find("DoorPlaceholder");
            var camera = roots.Select(root => root.GetComponent<PrototypeFollowCamera>()).Single(item => item != null);
            var player = roots.Single(root => root.GetComponent<PrototypeMotor>() != null);
            Transform hud = roots.Single(root => root.name == "Prototype HUD").transform;
            Transform safeArea = hud.Find("SafeArea");
            var label = safeArea.Find("Status/InteractionCount").GetComponent<UnityEngine.UI.Text>();
            var red = AssetDatabase.LoadAssetAtPath<Material>(Materials + "PrototypeRed.mat");
            var wall = AssetDatabase.LoadAssetAtPath<Material>(Materials + "GreyboxWall.mat");
            var yellow = AssetDatabase.LoadAssetAtPath<Material>(Materials + "PrototypeYellow.mat");
            if (panel == null || panel.GetComponent<BoxCollider>() == null || red == null || wall == null || yellow == null)
                throw new InvalidOperationException("The existing door placeholder or greybox materials are missing.");

            UpgradePlayerPrefab();
            PrototypeDoor door = SetupDoor(room, panel, wall, "LeverDoor", new Vector3(0f, 0f, 5f));
            PrototypeLever lever = SetupLever(room, door, wall, yellow);
            ConfigureActor(player, camera, lever, "Gameplay");
            SetReference(camera, "spawnFacing", player.GetComponent<PrototypeMotor>().Facing);
            Quaternion initial = Quaternion.Euler(25f, player.GetComponent<PrototypeMotor>().Facing.eulerAngles.y, 0f);
            camera.transform.SetPositionAndRotation(player.transform.position + Vector3.up * 1.2f +
                initial * Vector3.back * 5f, initial);

            GameObject player2 = AddDebugPlayer(scene, player, camera, lever, red);
            SetupHud(hud, safeArea, label, player, player2, camera);
            ConfigureMechanisms(scene.GetRootGameObjects());
        }

        internal static void ConfigurePlayerPrefab(GameObject player)
        {
            // Ignore Raycast is excluded from the camera mask, but still collides with controllers and room geometry.
            player.layer = 2;
            Transform visual = player.GetComponent<PrototypeMotor>().Facing;
            Transform origin = visual.Find("InteractionOrigin");
            if (origin == null)
                origin = Child(visual, "InteractionOrigin", new Vector3(0f, 0.2f, 0.42f));
            var interactor = player.GetComponent<PrototypeInteractor>() ?? player.AddComponent<PrototypeInteractor>();
            SetReference(interactor, "interactionOrigin", origin);
            SetReference(player.GetComponent<PrototypeLocalInput>(), "interactor", interactor);
        }

        private static void UpgradePlayerPrefab()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (asset.GetComponent<PrototypeInteractor>() != null)
                return;

            GameObject contents = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                ConfigurePlayerPrefab(contents);
                if (PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath) == null)
                    throw new InvalidOperationException("Could not update PrototypePlayer in place.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static PrototypeDoor SetupDoor(Transform room, Transform panel, Material wall, string name, Vector3 position)
        {
            Transform frame = Child(room, name, position);
            panel.SetParent(frame, false);
            panel.name = "DoorPanel";
            panel.localPosition = Vector3.up;
            Cube(frame, "LeftPost", new Vector3(-1.55f, 1.3f, 0f), new Vector3(0.7f, 2.6f, 0.65f), wall);
            Cube(frame, "RightPost", new Vector3(1.55f, 1.3f, 0f), new Vector3(0.7f, 2.6f, 0.65f), wall);
            Cube(frame, "Lintel", new Vector3(0f, 2.42f, 0.42f), new Vector3(3.8f, 0.36f, 0.4f), wall);
            return panel.gameObject.AddComponent<PrototypeDoor>();
        }

        private static void ConfigureMechanisms(GameObject[] roots)
        {
            Transform room = roots.Single(root => root.name == "Greybox").transform;
            Transform leverFrame = room.Find("LeverDoor") ?? room.Find("Door");
            Transform left = room.Find("LeftPlatePlaceholder");
            Transform right = room.Find("RightPlatePlaceholder");
            var hud = roots.Single(root => root.name == "Prototype HUD").transform;
            Transform safeArea = hud.Find("SafeArea");
            var controls = safeArea.Find("Hint/Controls").GetComponent<UnityEngine.UI.Text>();
            var module = roots.Select(root => root.GetComponent<InputSystemUIInputModule>()).Single(item => item != null);
            string uiPath = AssetDatabase.GetAssetPath(module.actionsAsset);
            var scroll = AssetDatabase.LoadAllAssetsAtPath(uiPath).OfType<InputActionReference>()
                .FirstOrDefault(reference => reference.action?.name == "ScrollWheel" && reference.action.actionMap.name == "UI");
            var wall = AssetDatabase.LoadAssetAtPath<Material>(Materials + "GreyboxWall.mat");
            var blue = AssetDatabase.LoadAssetAtPath<Material>(Materials + "PrototypeBlue.mat");
            if (leverFrame == null || left == null || right == null || scroll == null || wall == null || blue == null ||
                room.Find("PressurePlateDoor") != null || room.Find("Plate A") != null || room.Find("Plate B") != null)
                throw new InvalidOperationException("The prototype scene does not match the expected door and plate setup.");

            leverFrame.name = "LeverDoor";
            Transform panel = Cube(room, "PressurePlatePanel", Vector3.zero, new Vector3(2.4f, 2f, 0.35f), blue).transform;
            PrototypeDoor door = SetupDoor(room, panel, wall, "PressurePlateDoor", new Vector3(4.8f, 0f, 4.8f));
            PrototypePressurePlate a = SetupPlate(room, left, "Plate A", new Vector3(3.2f, 0f, 3.35f));
            PrototypePressurePlate b = SetupPlate(room, right, "Plate B", new Vector3(6.4f, 0f, 3.35f));
            var gate = door.transform.parent.gameObject.AddComponent<PrototypePlateGate>();
            SetReference(gate, "plateA", a);
            SetReference(gate, "plateB", b);
            SetReference(gate, "door", door);

            module.scrollWheel = scroll;
            EditorUtility.SetDirty(module);
            controls.text = "WASD / Stick to move  |  Drag to orbit  |  Scroll or pinch to zoom";
            var instructions = Corner("Pressure Plate Instructions", safeArea, new Vector2(0.5f, 1f),
                new Vector2(0f, -170f), new Vector2(620f, 36f));
            Label("PressurePlateHint", instructions, controls.font, "Both pressure plates must be occupied", 22);
        }

        private static PrototypePressurePlate SetupPlate(Transform room, Transform visual, string name, Vector3 position)
        {
            Transform root = Child(room, name, position);
            visual.SetParent(root, false);
            visual.name = "Visual";
            visual.localPosition = new Vector3(0f, 0.03f, 0f);
            var plate = root.gameObject.AddComponent<PrototypePressurePlate>();
            var trigger = root.GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.18f, 0f);
            trigger.size = new Vector3(1.22f, 0.44f, 1.22f);
            SetReference(plate, "visual", visual.GetComponent<Renderer>());
            return plate;
        }

        private static PrototypeLever SetupLever(Transform room, PrototypeDoor door, Material wall, Material yellow)
        {
            Transform root = Child(room, "Lever", new Vector3(-2f, 0f, 3.5f));
            var lever = root.gameObject.AddComponent<PrototypeLever>();
            Cube(root, "Pedestal", new Vector3(0f, 0.45f, 0f), new Vector3(0.36f, 0.9f, 0.36f), wall, false);
            Transform handle = Child(root, "Handle", new Vector3(0f, 0.9f, 0f));
            handle.localRotation = Quaternion.Euler(-20f, 0f, 0f);
            Cube(handle, "Shaft", new Vector3(0f, 0.22f, 0f), new Vector3(0.09f, 0.44f, 0.09f), yellow, false);
            Cube(handle, "Grip", new Vector3(0f, 0.45f, 0f), new Vector3(0.24f, 0.11f, 0.12f), yellow, false);
            Transform point = Child(root, "InteractionPoint", new Vector3(0f, 1.2f, 0f));
            SetReference(lever, "interactionPoint", point);
            SetReference(lever, "handle", handle);
            SetReference(lever, "door", door);
            return lever;
        }

        private static void ConfigureActor(GameObject player, PrototypeFollowCamera camera, PrototypeLever lever, string map)
        {
            var input = new SerializedObject(player.GetComponent<PrototypeLocalInput>());
            input.FindProperty("actionMap").stringValue = map;
            input.ApplyModifiedPropertiesWithoutUndo();
            SetReference(player.GetComponent<PrototypeMotor>(), "movementCamera", camera.transform);
            var interactor = new SerializedObject(player.GetComponent<PrototypeInteractor>());
            SerializedProperty targets = interactor.FindProperty("levers");
            targets.arraySize = 1;
            targets.GetArrayElementAtIndex(0).objectReferenceValue = lever;
            interactor.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject AddDebugPlayer(Scene scene, GameObject player, PrototypeFollowCamera camera,
            PrototypeLever lever, Material red)
        {
            var group = new GameObject("Prototype Debug Players");
            var settings = group.AddComponent<PrototypeDebugPlayers>();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var player2 = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            player2.name = "DebugPlayer2";
            player2.transform.SetParent(group.transform, false);
            player2.transform.SetPositionAndRotation(player.transform.position + Vector3.right * 2f, player.transform.rotation);
            player2.GetComponent<PrototypeMotor>().Facing.GetComponent<Renderer>().sharedMaterial = red;
            ConfigureActor(player2, camera, lever, "DebugPlayer2");
            player2.SetActive(false);
            SetReference(settings, "player2", player2);
            return player2;
        }

        private static void SetupHud(Transform hud, Transform safeArea, UnityEngine.UI.Text label, GameObject player,
            GameObject player2, PrototypeFollowCamera camera)
        {
            RectTransform background = Rect("World Orbit Input", hud);
            background.SetAsFirstSibling();
            background.anchorMin = Vector2.zero;
            background.anchorMax = Vector2.one;
            background.offsetMin = background.offsetMax = Vector2.zero;
            var surface = background.gameObject.AddComponent<UnityEngine.UI.Image>();
            surface.color = Color.clear;
            SetReference(background.gameObject.AddComponent<PrototypeOrbitDrag>(), "orbit", camera);
            safeArea.Find("StickBase").GetComponent<UnityEngine.UI.Image>().raycastTarget = true;

            label.gameObject.name = "Player1Prompt";
            label.text = "";
            SetReference(label.gameObject.AddComponent<PrototypeInteractionPrompt>(), "interactor",
                player.GetComponent<PrototypeInteractor>());
            safeArea.Find("Hint/Controls").GetComponent<UnityEngine.UI.Text>().text =
                "WASD / Stick to move   |   Drag the world to orbit";

            RectTransform debug = Corner("DebugStatus", safeArea, new Vector2(0.5f, 1f),
                new Vector2(0f, -134f), new Vector2(420f, 32f));
            var debugText = Label("Player2Prompt", debug, label.font, "", 20);
            var prompt = debugText.gameObject.AddComponent<PrototypeInteractionPrompt>();
            SetReference(prompt, "interactor", player2.GetComponent<PrototypeInteractor>());
            var serialized = new SerializedObject(prompt);
            serialized.FindProperty("debugPlayer").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void ConfigureWindowsProfile()
        {
            const string profilePath = "Assets/Settings/Build Profiles/Windows.asset";
            var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(profilePath);
            if (profile == null)
                return;

            var scenes = profile.scenes;
            if (!profile.overrideGlobalScenes)
                scenes = Array.Empty<EditorBuildSettingsScene>();
            if (scenes.Any(entry => entry.path == ScenePath && entry.enabled))
                return;

            profile.overrideGlobalScenes = true;
            profile.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }
                .Concat(scenes.Where(entry => entry.path != ScenePath)).ToArray();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
        }

        private static Transform Child(Transform parent, string name, Vector3 position)
        {
            Transform child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = position;
            return child;
        }
    }
}
