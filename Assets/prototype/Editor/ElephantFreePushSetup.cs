using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TwoTails.Prototype;

namespace TwoTails.Editor
{
    public static class ElephantFreePushSetup
    {
        [MenuItem("Tools/Two Tails/Configure Elephant Free Push Test")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Exit Play Mode before editing scenes.");
            var previous = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            string previousPath = previous.path;
            if (previous.isDirty)
                throw new System.InvalidOperationException("Save the current scene before configuring the free push test.");
            foreach (string path in new[]
            {
                "Assets/prototype/Scenes/Prototype_SplitScreen.unity",
                "Assets/prototype/Scenes/Prototype_OnlineTest.unity"
            })
            {
                var scene = EditorSceneManager.OpenScene(path);
                var parent = GameObject.Find("Animal Mechanisms")?.transform;
                if (parent == null)
                    throw new System.InvalidOperationException("Animal Mechanisms is missing in " + path);
                if (parent.Find("Elephant Free Push Test Block") == null)
                {
                    var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    block.name = "Elephant Free Push Test Block";
                    block.transform.SetParent(parent);
                    block.transform.position = new Vector3(0f, 0.55f, -10f);
                    block.transform.localScale = new Vector3(1.6f, 1.1f, 1.6f);
                    block.GetComponent<Renderer>().sharedMaterial =
                        AssetDatabase.LoadAssetAtPath<Material>("Assets/prototype/Materials/PrototypeDamWood.mat");
                    var body = block.AddComponent<Rigidbody>();
                    body.mass = 20f;
                    body.isKinematic = true;
                    body.useGravity = false;
                    body.linearDamping = 8f;
                    body.angularDamping = 8f;
                    body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    body.interpolation = RigidbodyInterpolation.Interpolate;
                    body.constraints = RigidbodyConstraints.FreezeRotation;
                    block.AddComponent<ElephantFreePushBlock>();
                }
                EditorSceneManager.SaveScene(scene);
                EditorSceneManager.OpenScene(path);
                if (GameObject.Find("Elephant Free Push Test Block") == null)
                    throw new System.InvalidOperationException("Block did not persist in " + path);
            }
            EditorSceneManager.OpenScene(previousPath);
        }
    }
}
