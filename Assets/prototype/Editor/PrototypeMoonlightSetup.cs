using System;
using TwoTails.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TwoTails.Editor
{
    public static class PrototypeMoonlightSetup
    {
        public static void Save()
        {
            var previous = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (previous.isDirty || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Save pending scene edits and exit Play Mode first.");
            string previousPath = previous.path;
            try
            {
                foreach (var name in new[] { "Prototype_OnlineTest", "Prototype_SplitScreen" })
                {
                    var scene = EditorSceneManager.OpenScene($"Assets/prototype/Scenes/{name}.unity");
                    var environment = UnityEngine.Object.FindAnyObjectByType<PrototypeNightEnvironment>();
                    var area = GameObject.Find("Level 1 Night Test")?.transform;
                    if (environment == null || area == null) throw new InvalidOperationException($"Night test missing in {name}");
                    var moon = area.Find("Weak Moonlight")?.GetComponent<Light>();
                    if (moon == null)
                    {
                        moon = new GameObject("Weak Moonlight").AddComponent<Light>();
                        moon.transform.SetParent(area, false);
                    }
                    moon.type = LightType.Directional;
                    moon.transform.rotation = Quaternion.Euler(65f, -35f, 0f);
                    moon.color = new Color(.34f, .46f, .7f);
                    moon.intensity = .12f;
                    moon.shadows = LightShadows.Soft;
                    moon.shadowStrength = 1f;
                    moon.enabled = true;
                    var serialized = new SerializedObject(environment);
                    serialized.FindProperty("moonlight").objectReferenceValue = moon;
                    serialized.FindProperty("moonDirection").vector3Value = new Vector3(65f, -35f, 0f);
                    serialized.ApplyModifiedPropertiesWithoutUndo();

                    if (area.Find("Moonlight Roof Test") == null)
                    {
                        var root = new GameObject("Moonlight Roof Test").transform;
                        root.SetParent(area, false);
                        var stone = AssetDatabase.LoadAssetAtPath<Material>("Assets/prototype/Materials/PrototypeBasinStone.mat");
                        void Box(string label, Vector3 position, Vector3 size)
                        {
                            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.name = label;
                            cube.transform.SetParent(root);
                            cube.transform.position = position;
                            cube.transform.localScale = size;
                            cube.GetComponent<Renderer>().sharedMaterial = stone;
                        }
                        Box("Solid Cave Roof", new Vector3(6f, 2.7f, 5f), new Vector3(4f, .4f, 4f));
                        Box("Cave Left Wall", new Vector3(3.9f, 1.25f, 5f), new Vector3(.3f, 2.5f, 4f));
                        Box("Cave Right Wall", new Vector3(8.1f, 1.25f, 5f), new Vector3(.3f, 2.5f, 4f));
                        Box("Cave Back Wall", new Vector3(6f, 1.25f, 7.1f), new Vector3(4.5f, 2.5f, .3f));
                    }
                    // Shift the authored test away from the PressurePlateDoor RightPost.
                    area.Find("Moonlight Roof Test").position = new Vector3(-11f, 0f, 0f);
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException($"Could not save {name}");
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(previousPath)) EditorSceneManager.OpenScene(previousPath);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
