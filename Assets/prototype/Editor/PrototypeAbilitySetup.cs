using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using TwoTails.Prototype;
namespace TwoTails.Editor
{
 public static class PrototypeAbilitySetup
 {
  [MenuItem("Tools/Two Tails/Configure Animal Test")]
  public static void Configure()
  {
   if(EditorApplication.isPlayingOrWillChangePlaymode || UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save scene and exit Play Mode first.");
   string previous=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
   foreach(string path in new[]{"Assets/prototype/Input/TwoTailsInput.inputactions","Assets/prototype/Input/PrototypeSplitInput.inputactions"})
   {
    var asset=AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
    foreach(var map in asset.actionMaps.Where(m=>m.name=="Gameplay"||m.name=="DebugPlayer2"))
    {
     var jump=map.FindAction("Jump")??map.AddAction("Jump",InputActionType.Button);
     if(jump.bindings.Count==0){jump.AddBinding(map.name=="Gameplay"?"<Keyboard>/space":"<Keyboard>/enter");if(map.name!="Gameplay")jump.AddBinding("<Keyboard>/numpadEnter");}
     if(map.name=="Gameplay"&&!jump.bindings.Any(b=>b.path=="<Gamepad>/buttonWest"))jump.AddBinding("<Gamepad>/buttonWest");
    }
    System.IO.File.WriteAllText(path,asset.ToJson());AssetDatabase.ImportAsset(path);
   }
   string prefab="Assets/prototype/Prefabs/NetworkPlayerPrototype.prefab";
   var root=PrefabUtility.LoadPrefabContents(prefab);
   if(root.GetComponent<PrototypeAnimal>()==null)root.AddComponent<PrototypeAnimal>();
   PrefabUtility.SaveAsPrefabAsset(root,prefab);PrefabUtility.UnloadPrefabContents(root);
   foreach(string path in new[]{PrototypeTestBuild.Local,PrototypeTestBuild.Online})
   {
    var scene=EditorSceneManager.OpenScene(path);
    foreach(var motor in UnityEngine.Object.FindObjectsByType<PrototypeMotor>(FindObjectsInactive.Include))
    {
     var identity=motor.GetComponent<PrototypeAnimal>()??motor.gameObject.AddComponent<PrototypeAnimal>();
     identity.Apply(motor.name.Contains("Monkey")?PrototypeSpecies.Monkey:PrototypeSpecies.Fox);
     EditorUtility.SetDirty(identity);
    }
    if(GameObject.Find("Animal Test Geometry")==null)
    {
     var group=new GameObject("Animal Test Geometry");
     GameObject Box(string name,Vector3 position,Vector3 scale){var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(group.transform);obj.transform.position=position;obj.transform.localScale=scale;return obj;}
     var wall=Box("Vine Wall",new Vector3(-6,1.5f,1),new Vector3(2.5f,3,0.5f));
     var materialPath="Assets/prototype/Materials/PrototypeVine.mat";
     var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
     if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=Color.green;AssetDatabase.CreateAsset(material,materialPath);}
     wall.GetComponent<Renderer>().sharedMaterial=material;
     var surface=wall.AddComponent<PrototypeClimbSurface>();
     Box("Top Platform",new Vector3(-6,2.75f,2.5f),new Vector3(2.5f,0.5f,2.5f));
     var landing=new GameObject("Landing");landing.transform.SetParent(group.transform);landing.transform.position=new Vector3(-6,3.03f,1.9f);
     var so=new SerializedObject(surface);so.FindProperty("landing").objectReferenceValue=landing.transform;so.ApplyModifiedPropertiesWithoutUndo();
     Box("Normal Wall",new Vector3(6,1.5f,1),new Vector3(2.5f,3,0.5f));
     Box("Jump Step",new Vector3(5,0.4f,-3),new Vector3(2,0.8f,2));
    }
    if(path==PrototypeTestBuild.Online)
    {
     var source=UnityEngine.Object.FindObjectsByType<PrototypeScreenButton>(FindObjectsInactive.Include).FirstOrDefault();
     if(source!=null && source.transform.parent.Find("Jump Button")==null)
     {
      var button=UnityEngine.Object.Instantiate(source.gameObject,source.transform.parent);button.name="Jump Button";
      button.GetComponent<PrototypeScreenButton>().controlPath="<Gamepad>/buttonWest";
      ((RectTransform)button.transform).anchoredPosition+=new Vector2(-200,0);
      foreach(var label in button.GetComponentsInChildren<UnityEngine.UI.Text>())label.text="Jump";
     }
    }
    EditorSceneManager.SaveScene(scene);
   }
   EditorSceneManager.OpenScene(previous);AssetDatabase.SaveAssets();
  }
 }
}
