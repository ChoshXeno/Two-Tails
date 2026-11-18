using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TwoTails.Prototype;

public static class SaveRabbitElephantCourse
{
    public static void Run()
    {
        foreach (string name in new[] { "Prototype_OnlineTest", "Prototype_SplitScreen" })
        {
            string path = $"Assets/prototype/Scenes/{name}.unity";
            var scene = EditorSceneManager.OpenScene(path);
            var world = Object.FindAnyObjectByType<AnimalMechanicsWorld>();
            world.ConfigureExtendedCourseInEditor();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new System.Exception(path);
            scene = EditorSceneManager.OpenScene(path);
            world = Object.FindAnyObjectByType<AnimalMechanicsWorld>();
            foreach (string child in new[] { "Rabbit Display", "Elephant Display", "Rabbit and Elephant Course" })
                if (world.transform.Find(child) == null) throw new System.Exception(child);
            Debug.Log("Verified saved course in " + path);
        }
        AssetDatabase.SaveAssets();
    }
}
