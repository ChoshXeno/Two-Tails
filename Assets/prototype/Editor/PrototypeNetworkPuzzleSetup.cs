using System;
using System.Linq;
using TwoTails.Prototype;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TwoTails.Editor.PrototypeGreyboxBuilder;

namespace TwoTails.Editor
{
    internal static class PrototypeNetworkPuzzleSetup
    {
        private const string PlayerPath = "Assets/prototype/Prefabs/NetworkPlayerPrototype.prefab";

        [MenuItem("Tools/Two Tails/Configure Network Puzzles")]
        private static void ConfigureSavedScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before configuring network puzzles.");

            Scene previous = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(PrototypeNetworkBuilder.NetworkScene);
            bool opened = !scene.isLoaded;
            if (opened)
                scene = EditorSceneManager.OpenScene(PrototypeNetworkBuilder.NetworkScene, OpenSceneMode.Additive);
            try
            {
                if (scene.isDirty)
                    throw new InvalidOperationException("Save the network scene before configuring puzzles.");
                SceneManager.SetActiveScene(scene);
                bool changed = Configure(scene);
                changed |= ConfigurePlates(scene);
                if (changed)
                    EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                if (opened)
                    EditorSceneManager.CloseScene(scene, true);
            }
            RegisterEditorScene();
        }

        internal static bool Configure(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            var room = roots.Single(root => root.name == "Greybox").transform;
            Transform leverRoot = room.Find("Lever");
            if (leverRoot.GetComponent<PrototypeNetworkLever>() != null)
                return false;

            UpgradePlayer();
            var manager = roots.Select(root => root.GetComponent<NetworkManager>()).Single(item => item != null);
            manager.NetworkConfig.EnableSceneManagement = true;
            EditorUtility.SetDirty(manager);

            var door = AddNetworkDoor(room.Find("LeverDoor"));
            var appearance = leverRoot.gameObject.AddComponent<PrototypeLever>();
            appearance.enabled = false;
            SetReference(appearance, "handle", leverRoot.Find("Handle"));
            SetReference(appearance, "interactionPoint", leverRoot.Find("InteractionPoint"));
            SetReference(leverRoot.gameObject.AddComponent<PrototypeNetworkLever>(), "door", door);
            BuildHud(roots.Single(root => root.name == "Prototype HUD").transform, manager.GetComponent<PrototypeNetworkSession>());
            return true;
        }

        internal static bool ConfigurePlates(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            Transform room = roots.Single(root => root.name == "Greybox").transform;
            Transform frame = room.Find("PressurePlateDoor");
            if (frame.GetComponent<PrototypeNetworkPlateGate>() != null)
                return false;

            var door = AddNetworkDoor(frame);
            var gate = door.gameObject.AddComponent<PrototypeNetworkPlateGate>();
            SetReference(gate, "plateA", AddPlateAppearance(room.Find("Plate A")));
            SetReference(gate, "plateB", AddPlateAppearance(room.Find("Plate B")));
            Transform hud = roots.Single(root => root.name == "Prototype HUD").transform;
            var presenter = hud.GetComponentInChildren<PrototypeNetworkPuzzleHud>(true);
            var prompt = presenter.GetComponentInChildren<UnityEngine.UI.Text>();
            var area = Corner("Pressure Gate Status", hud.Find("SafeArea"), new Vector2(0.5f, 1),
                new Vector2(0f, -328f), new Vector2(780f, 34f));
            SetReference(presenter, "gate", gate);
            SetReference(presenter, "plateStatus", Label("Text", area, prompt.font, "", 21));
            return true;
        }

        private static PrototypePressurePlate AddPlateAppearance(Transform root)
        {
            var trigger = root.GetComponent<BoxCollider>();
            trigger.size = new Vector3(1.5f, trigger.size.y, 1.5f);
            Transform mesh = root.Find("Visual");
            mesh.localScale = new Vector3(1.5f, mesh.localScale.y, 1.5f);
            var plate = root.gameObject.AddComponent<PrototypePressurePlate>();
            plate.enabled = false;
            SetReference(plate, "visual", mesh.GetComponent<Renderer>());
            return plate;
        }

        private static PrototypeNetworkDoor AddNetworkDoor(Transform frame)
        {
            var panel = frame.Find("DoorPanel").gameObject.AddComponent<PrototypeDoor>();
            panel.enabled = false;
            var passageObject = new GameObject("Server Passage");
            passageObject.transform.SetParent(frame, false);
            var passage = passageObject.AddComponent<BoxCollider>();
            passage.isTrigger = true;
            passage.center = new Vector3(0f, 1.2f, 0f);
            passage.size = new Vector3(2.4f, 2.4f, 1.2f);

            var door = frame.gameObject.AddComponent<PrototypeNetworkDoor>();
            SetReference(door, "panel", panel);
            SetReference(door, "passage", passage);
            return door;
        }

        private static void UpgradePlayer()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath);
            if (prefab.GetComponent<PrototypeNetworkInteraction>() != null)
                return;

            var root = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                var avatar = root.GetComponent<NetworkPlayerPrototype>();
                var source = root.GetComponent<CharacterController>();
                var proxyObject = new GameObject("Server Puzzle Proxy");
                proxyObject.layer = root.layer;
                proxyObject.transform.SetParent(root.transform, false);
                var proxy = proxyObject.AddComponent<CapsuleCollider>();
                proxy.isTrigger = true;
                proxy.center = source.center;
                proxy.height = source.height;
                proxy.radius = source.radius;
                proxy.enabled = false;
                SetReference(avatar, "serverProxy", proxy);
                var interactor = root.AddComponent<PrototypeNetworkInteraction>();
                SetReference(interactor, "interactionOrigin", root.GetComponent<PrototypeMotor>().Facing.Find("InteractionOrigin"));
                if (PrefabUtility.SaveAsPrefabAsset(root, PlayerPath) == null)
                    throw new InvalidOperationException("Could not update the network player prefab.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void BuildHud(Transform hud, PrototypeNetworkSession session)
        {
            Transform safeArea = hud.Find("SafeArea");
            var title = hud.GetComponentsInChildren<UnityEngine.UI.Text>(true)
                .Single(label => label.text.StartsWith("Movement networking test.", StringComparison.Ordinal));
            title.text = "Network puzzle test";
            Font font = title.font;
            var interact = Corner("Network Interact", safeArea, Vector2.right,
                new Vector2(-155, 155), new Vector2(174, 174));
            var image = interact.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            image.color = new Color(0.08f, 0.20f, 0.34f, 0.94f);
            var button = interact.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            Label("Text", interact, font, "Interact", 26);
            interact.gameObject.SetActive(false);
            var holder = Corner("Network Puzzle HUD", safeArea, new Vector2(0.5f, 1f),
                new Vector2(0, -286), new Vector2(740, 40));
            var prompt = Label("Interaction Prompt", holder, font, "", 26);
            var presenter = holder.gameObject.AddComponent<PrototypeNetworkPuzzleHud>();
            SetReference(presenter, "session", session);
            SetReference(presenter, "interact", button);
            SetReference(presenter, "prompt", prompt);
        }

        internal static void RegisterEditorScene()
        {
            string path = PrototypeNetworkBuilder.NetworkScene;
            var scenes = EditorBuildSettings.globalScenes;
            if (scenes.Any(item => item.path == path && item.enabled))
                return;

            EditorBuildSettings.globalScenes = scenes.Where(item => item.path != path)
                .Append(new EditorBuildSettingsScene(path, true)).ToArray();
        }
    }
}
