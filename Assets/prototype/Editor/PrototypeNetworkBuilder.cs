using System;
using System.Linq;
using TwoTails.Prototype;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TwoTails.Editor.PrototypeGreyboxBuilder;

namespace TwoTails.Editor
{
    public static class PrototypeNetworkBuilder
    {
        public const string NetworkScene = "Assets/prototype/Scenes/Prototype_Network.unity";
        private const string NetworkPrefab = "Assets/prototype/Prefabs/NetworkPlayerPrototype.prefab";
        private const string Profile = "Assets/Settings/Build Profiles/Windows Network Prototype.asset";

        [MenuItem("Tools/Two Tails/Build Network Prototype")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before creating the network scene.");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(NetworkScene) != null)
            {
                CreateProfile();
                return;
            }

            Scene previous = SceneManager.GetActiveScene();
            Scene baseline = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !baseline.isLoaded;
            if (opened)
                baseline = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                if (baseline.isDirty || !EditorSceneManager.SaveScene(baseline, NetworkScene, true))
                    throw new InvalidOperationException("The saved local prototype is required.");
            }
            finally
            {
                if (opened)
                    EditorSceneManager.CloseScene(baseline, true);
            }

            Scene scene = EditorSceneManager.OpenScene(NetworkScene, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                Configure(scene);
                PrototypeNetworkPuzzleSetup.Configure(scene);
                PrototypeNetworkPuzzleSetup.ConfigurePlates(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Could not save the network prototype.");
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }

            CreateProfile();
            PrototypeNetworkPuzzleSetup.RegisterEditorScene();
        }

        private static void Configure(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            var camera = roots.Select(root => root.GetComponent<PrototypeFollowCamera>()).Single(item => item != null);
            Transform hud = roots.Single(root => root.name == "Prototype HUD").transform;
            Transform safeArea = hud.Find("SafeArea");
            SetReference(camera, "target", null);
            SetReference(camera, "spawnFacing", null);
            camera.transform.SetPositionAndRotation(new Vector3(0, 7, -9), Quaternion.Euler(30, 0, 0));

            foreach (GameObject root in roots)
            {
                if (root.GetComponent<PrototypeMotor>() != null || root.GetComponent<PrototypeDebugPlayers>() != null)
                    UnityEngine.Object.DestroyImmediate(root);
            }

            Transform room = roots.Single(root => root != null && root.name == "Greybox").transform;
            foreach (MonoBehaviour component in room.GetComponentsInChildren<MonoBehaviour>())
            {
                if (component is PrototypeDoor or PrototypeLever or PrototypePlateGate or PrototypePressurePlate)
                    UnityEngine.Object.DestroyImmediate(component);
            }
            foreach (Transform child in safeArea.Cast<Transform>().ToArray())
            {
                if (child.name != "StickBase")
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
            }

            var orbit = hud.GetComponentInChildren<PrototypeOrbitDrag>();
            orbit.enabled = false;
            GameObject stick = safeArea.Find("StickBase").gameObject;
            stick.SetActive(false);

            var network = new GameObject("Network Session");
            var manager = network.AddComponent<NetworkManager>();
            var transport = network.AddComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 7777, "127.0.0.1");
            transport.ConnectTimeoutMS = 500;
            transport.MaxConnectAttempts = 12;
            manager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
                PlayerPrefab = MakePlayer(scene),
                NetworkTopology = NetworkTopologyTypes.ClientServer,
                ConnectionApproval = true,
                EnableSceneManagement = false,
                ClientConnectionBufferTimeout = 8
            };

            var session = network.AddComponent<PrototypeNetworkSession>();
            var serialized = new SerializedObject(session);
            var points = serialized.FindProperty("spawnPoints");
            points.arraySize = 2;
            for (int i = 0; i < 2; i++)
            {
                var point = new GameObject($"Spawn {i + 1}").transform;
                point.SetParent(network.transform, false);
                point.position = new Vector3(i == 0 ? -1.5f : 1.5f, 0.1f, -4.6f);
                points.GetArrayElementAtIndex(i).objectReferenceValue = point;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            BuildHud(safeArea, session, camera, orbit, stick);
        }

        private static GameObject MakePlayer(Scene scene)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkPrefab);
            if (existing != null)
                return existing;

            var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var player = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab, scene);
            PrefabUtility.UnpackPrefabInstance(player, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            player.name = "NetworkPlayerPrototype";
            var motor = player.GetComponent<PrototypeMotor>();
            var input = player.GetComponent<PrototypeLocalInput>();
            SetReference(input, "interactor", null);
            UnityEngine.Object.DestroyImmediate(player.GetComponent<PrototypeInteractor>());
            motor.enabled = input.enabled = player.GetComponent<CharacterController>().enabled = false;
            player.AddComponent<NetworkObject>();
            var rootSync = player.AddComponent<NetworkTransform>();
            ConfigureSync(rootSync, false);
            rootSync.TickSyncChildren = true;
            ConfigureSync(motor.Facing.gameObject.AddComponent<NetworkTransform>(), true);
            var avatar = player.AddComponent<NetworkPlayerPrototype>();
            SetReference(avatar, "capsule", motor.Facing.GetComponent<Renderer>());
            SetReference(avatar, "hostMaterial", AssetDatabase.LoadAssetAtPath<Material>("Assets/prototype/Materials/PrototypeBlue.mat"));
            SetReference(avatar, "clientMaterial", AssetDatabase.LoadAssetAtPath<Material>("Assets/prototype/Materials/PrototypeRed.mat"));
            var prefab = PrefabUtility.SaveAsPrefabAsset(player, NetworkPrefab);
            UnityEngine.Object.DestroyImmediate(player);
            if (prefab == null)
                throw new InvalidOperationException("Could not create NetworkPlayerPrototype.");
            return prefab;
        }

        private static void ConfigureSync(NetworkTransform sync, bool yawOnly)
        {
            sync.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
            sync.InLocalSpace = yawOnly;
            sync.SyncPositionX = sync.SyncPositionY = sync.SyncPositionZ = !yawOnly;
            sync.SyncRotAngleX = sync.SyncRotAngleZ = false;
            sync.SyncRotAngleY = yawOnly;
            sync.SyncScaleX = sync.SyncScaleY = sync.SyncScaleZ = false;
            sync.Interpolate = true;
        }

        private static void BuildHud(Transform safeArea, PrototypeNetworkSession session,
            PrototypeFollowCamera camera, PrototypeOrbitDrag orbit, GameObject stick)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var panel = Corner("Connection", safeArea, new Vector2(0.5f, 1), new Vector2(0, -133), new Vector2(800, 250));
            panel.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0.06f, 0.09f, 0.14f, 0.92f);
            var hud = panel.gameObject.AddComponent<PrototypeNetworkHud>();
            SetReference(hud, "session", session);
            SetReference(hud, "followCamera", camera);
            SetReference(hud, "orbit", orbit);
            SetReference(hud, "stick", stick);
            TextAt(panel, font, "Movement networking test. Puzzle sync comes next.", 0, 94, 760, 25);
            TextAt(panel, font, "Address", -322, 43, 120, 24);
            TextAt(panel, font, "Port", 90, 43, 100, 24);
            SetReference(hud, "address", Field(panel, "Address", "127.0.0.1", -133, 220));
            SetReference(hud, "port", Field(panel, "Port", "7777", 213, 125));
            SetReference(hud, "host", Button(panel, font, "Start Host", -245));
            SetReference(hud, "client", Button(panel, font, "Start Client", 0));
            SetReference(hud, "disconnect", Button(panel, font, "Disconnect", 245));
            SetReference(hud, "status", TextAt(panel, font, "Status: Offline", 0, -61, 760, 24));
            SetReference(hud, "players", TextAt(panel, font, "Players: 0/2", 0, -99, 760, 22));
        }

        private static UnityEngine.UI.Text TextAt(Transform parent, Font font, string text, float x, float y, float width, int size)
        {
            var rect = Corner(text, parent, Vector2.one * 0.5f, new Vector2(x, y), new Vector2(width, 38));
            return Label("Text", rect, font, text, size);
        }

        private static UnityEngine.UI.InputField Field(Transform parent, string name, string value, float x, float width)
        {
            var obj = UnityEngine.UI.DefaultControls.CreateInputField(new UnityEngine.UI.DefaultControls.Resources());
            obj.name = name;
            var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f;
            rect.anchoredPosition = new Vector2(x, 43);
            rect.sizeDelta = new Vector2(width, 43);
            var field = obj.GetComponent<UnityEngine.UI.InputField>();
            field.text = value;
            field.textComponent.fontSize = 24;
            return field;
        }

        private static UnityEngine.UI.Button Button(Transform parent, Font font, string text, float x)
        {
            var rect = Corner(text, parent, Vector2.one * 0.5f, new Vector2(x, -12), new Vector2(206, 48));
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.17f, 0.34f, 0.53f);
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            Label("Text", rect, font, text, 23);
            return button;
        }

        private static void CreateProfile()
        {
            if (AssetDatabase.LoadAssetAtPath<BuildProfile>(Profile) != null)
                return;

            var windows = AssetDatabase.LoadAssetAtPath<BuildProfile>("Assets/Settings/Build Profiles/Windows.asset");
            var profile = UnityEngine.Object.Instantiate(windows);
            profile.name = "Windows Network Prototype";
            AssetDatabase.CreateAsset(profile, Profile);
            profile.overrideGlobalScenes = true;
            profile.scenes = new[] { new EditorBuildSettingsScene(NetworkScene, true) };
            var serialized = new SerializedObject(profile);
            var development = serialized.FindProperty("m_PlatformBuildProfile.m_Development");
            if (development != null)
                development.boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
        }
    }
}
