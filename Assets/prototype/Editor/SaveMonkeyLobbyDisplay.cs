using System;
using TwoTails.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SaveMonkeyLobbyDisplay
{
    [MenuItem("Tools/Two Tails/Save Monkey Lobby Display")]
    public static void Run()
    {
        foreach (var name in new[] { "Prototype_OnlineTest", "Prototype_SplitScreen" })
        {
            string path = $"Assets/prototype/Scenes/{name}.unity";
            var scene = EditorSceneManager.OpenScene(path);
            var world = UnityEngine.Object.FindAnyObjectByType<AnimalMechanicsWorld>();
            if (world == null) throw new InvalidOperationException($"Animal Mechanisms missing in {path}");
            world.ConfigureMonkeyDisplayInEditor();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException($"Could not save {path}");
            scene = EditorSceneManager.OpenScene(path);
            world = UnityEngine.Object.FindAnyObjectByType<AnimalMechanicsWorld>();
            if (world.transform.Find("Monkey Display") == null)
                throw new InvalidOperationException($"Monkey Display was not saved in {path}");
            Debug.Log($"Saved Monkey Display in {path}");
        }
        AssetDatabase.SaveAssets();
    }
}
