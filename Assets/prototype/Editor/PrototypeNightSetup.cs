using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TwoTails.Prototype;
namespace TwoTails.Editor
{
 public static class PrototypeNightSetup
 {
  [MenuItem("Tools/Two Tails/Configure Night Test")]
  public static void Configure()
  {
   if(EditorApplication.isPlayingOrWillChangePlaymode || UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save scene and exit Play Mode.");
   string previous=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
   foreach(string path in new[]{PrototypeTestBuild.Local,PrototypeTestBuild.Online})
   {
    var scene=EditorSceneManager.OpenScene(path);
    var environment=UnityEngine.Object.FindAnyObjectByType<PrototypeNightEnvironment>();
    if(environment==null)
    {
     var root=new GameObject("Level 1 Night Test");environment=root.AddComponent<PrototypeNightEnvironment>();
     var sun=UnityEngine.Object.FindObjectsByType<Light>().FirstOrDefault(l=>l.type==LightType.Directional);
     var so=new SerializedObject(environment);so.FindProperty("nightShader").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Shader>("Assets/prototype/Shaders/PrototypeAnimalVision.shader");so.FindProperty("daylight").objectReferenceValue=sun;so.ApplyModifiedPropertiesWithoutUndo();
     var lamp=new GameObject("Lit Area Lamp",typeof(Light));lamp.transform.SetParent(root.transform);lamp.transform.position=new Vector3(6,5,-3);
     lamp.transform.rotation=Quaternion.Euler(90,0,0);var light=lamp.GetComponent<Light>();light.type=LightType.Spot;light.spotAngle=100;light.innerSpotAngle=70;light.range=9;light.intensity=14;light.shadows=LightShadows.Soft;light.color=new Color(1,0.85f,0.6f);
     GameObject Box(string name,Vector3 position,Vector3 scale){var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(root.transform);obj.transform.position=position;obj.transform.localScale=scale;return obj;}
     Box("Unlit Landmark",new Vector3(-8,1.5f,5),new Vector3(1,3,1));
     Box("Lit Landmark",new Vector3(7,0.75f,-3),new Vector3(1,1.5f,1));
     var wall=Box("Air Catch Vine",new Vector3(0,2.7f,0),new Vector3(2,2.4f,0.5f));wall.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/prototype/Materials/PrototypeVine.mat");
     var surface=wall.AddComponent<PrototypeClimbSurface>();var landing=new GameObject("Air Catch Landing");landing.transform.SetParent(root.transform);landing.transform.position=new Vector3(0,3.93f,1);
     var ss=new SerializedObject(surface);ss.FindProperty("landing").objectReferenceValue=landing.transform;ss.ApplyModifiedPropertiesWithoutUndo();
     Box("Air Catch Platform",new Vector3(0,3.65f,1.25f),new Vector3(2,0.5f,2));
    }
    if(path==PrototypeTestBuild.Online)
    {
     var interact=UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsInactive.Include).Single(b=>b.name=="Network Interact");
     var existing=interact.transform.parent.Find("Jump Button");
     GameObject button=existing!=null?existing.gameObject:UnityEngine.Object.Instantiate(interact.gameObject,interact.transform.parent);
     button.name="Jump Button";
     var oldButton=button.GetComponent<UnityEngine.UI.Button>();if(oldButton!=null)UnityEngine.Object.DestroyImmediate(oldButton);
     var jump=button.GetComponent<PrototypeScreenButton>()??button.AddComponent<PrototypeScreenButton>();jump.controlPath="<Gamepad>/buttonWest";
     foreach(var text in button.GetComponentsInChildren<UnityEngine.UI.Text>())text.text="Jump";
     var jr=(RectTransform)button.transform;var ir=(RectTransform)interact.transform;
     jr.anchorMin=ir.anchorMin;jr.anchorMax=ir.anchorMax;jr.anchoredPosition=ir.anchoredPosition+new Vector2(-190,190);
     var hud=UnityEngine.Object.FindAnyObjectByType<PrototypeNetworkPuzzleHud>();var hs=new SerializedObject(hud);hs.FindProperty("jump").objectReferenceValue=button;hs.ApplyModifiedPropertiesWithoutUndo();
    }
    EditorSceneManager.SaveScene(scene);
   }
   EditorSceneManager.OpenScene(previous);AssetDatabase.SaveAssets();
  }
 }
}
