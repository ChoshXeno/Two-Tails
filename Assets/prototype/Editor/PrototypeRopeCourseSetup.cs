using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using UnityEngine;
using TwoTails.Prototype;

namespace TwoTails.Editor
{
    public static class PrototypeRopeCourseSetup
    {
        public static void Save()
        {
            var previous = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            string pathBefore = previous.path;
            if (previous.isDirty || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Save pending scene edits and exit Play Mode.");
            try
            {
                foreach (string name in new[] { "Prototype_OnlineTest", "Prototype_SplitScreen" })
                {
                    bool online = name == "Prototype_OnlineTest";
                    var scene = EditorSceneManager.OpenScene($"Assets/prototype/Scenes/{name}.unity");
                    if (GameObject.Find("Monkey Rope Test") != null) continue;
                    var root = new GameObject("Monkey Rope Test").transform;
                    var stone = AssetDatabase.LoadAssetAtPath<Material>("Assets/prototype/Materials/PrototypeBasinStone.mat");
                    var wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/prototype/Materials/PrototypeDamWood.mat");
                    GameObject Cube(string label, Transform parent, Vector3 position, Vector3 size, Material material)
                    {
                        var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        instance.name = label;
                        instance.transform.SetParent(parent);
                        instance.transform.position = position;
                        instance.transform.localScale = size;
                        instance.GetComponent<Renderer>().sharedMaterial = material;
                        return instance;
                    }
                    void Rope(string label, Vector3 anchor, float length)
                    {
                        var rope = new GameObject(label);
                        rope.transform.SetParent(root);
                        rope.transform.position = anchor;
                        var mesh = Cube("Rope Visual", rope.transform, anchor + Vector3.down * (length * .5f),
                            new Vector3(.12f, length, .12f), wood);
                        Object.DestroyImmediate(mesh.GetComponent<Collider>());
                        if (online) rope.AddComponent<NetworkObject>();
                        var settings = new SerializedObject(rope.AddComponent<PrototypeSwingRope>());
                        settings.FindProperty("online").boolValue = online;
                        settings.FindProperty("length").floatValue = length;
                        settings.FindProperty("visual").objectReferenceValue = mesh.transform;
                        settings.ApplyModifiedPropertiesWithoutUndo();
                    }
                    Cube("Safety Floor", root, new Vector3(33, -.25f, -10), new Vector3(21, .5f, 8), stone);
                    Cube("Launch Step", root, new Vector3(24.3f,.3f,-10), new Vector3(1,.6f,2.8f), stone);
                    Cube("Launch Platform", root, new Vector3(25.8f,.65f,-10), new Vector3(2,1.3f,2.8f), stone);
                    Cube("Landing Platform", root, new Vector3(40.6f,.65f,-10), new Vector3(2,1.3f,2.8f), stone);
                    Rope("Climb Rope", new Vector3(25.4f,6f,-7.4f), 5f);
                    Rope("Rope A", new Vector3(28.9f,7.5f,-10), 5.6f);
                    Rope("Rope B", new Vector3(35f,7.5f,-10), 5.6f);
                    var sign = new GameObject("Rope Instructions").AddComponent<TextMesh>();
                    sign.transform.SetParent(root);
                    sign.transform.position = new Vector3(24,2,-10);
                    sign.transform.rotation = Quaternion.Euler(0,-90,0);
                    sign.text = "Monkey Rope Test\nJump to grab | Up / Down to climb\nMove with the swing | Jump to release";
                    sign.anchor = TextAnchor.MiddleCenter;
                    sign.fontSize = 48;
                    sign.characterSize = .022f;
                    var lamp = new GameObject("Rope Test Lamp").AddComponent<Light>();
                    lamp.transform.SetParent(root);
                    lamp.transform.position = new Vector3(33,8,-10);
                    lamp.transform.rotation = Quaternion.Euler(90,0,0);
                    lamp.type = LightType.Spot;
                    lamp.range = 23f;
                    lamp.spotAngle = 110f;
                    lamp.intensity = 35f;
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(pathBefore)) EditorSceneManager.OpenScene(pathBefore);
            }
        }
    }
}
