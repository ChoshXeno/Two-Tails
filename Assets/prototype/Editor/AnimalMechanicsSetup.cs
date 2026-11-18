using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using TwoTails.Prototype;
namespace TwoTails.Editor
{
 public static class AnimalMechanicsSetup
 {
  private static void ConfigureInput()
  {
   foreach(string path in new[]{"Assets/prototype/Input/TwoTailsInput.inputactions","Assets/prototype/Input/PrototypeSplitInput.inputactions"})
   {
    var asset=InputActionAsset.FromJson(System.IO.File.ReadAllText(path));
    foreach(var map in asset.actionMaps)
    {
     if(map.FindAction("Jump")==null||map.FindAction("Surface")!=null)continue;
     var action=map.AddAction("Surface",InputActionType.Button);action.AddBinding(map.name=="DebugPlayer2"?"<Keyboard>/rightShift":"<Keyboard>/q");
     if(map.name!="DebugPlayer2")action.AddBinding("<Gamepad>/buttonNorth");
    }
    System.IO.File.WriteAllText(path,asset.ToJson());UnityEngine.Object.DestroyImmediate(asset);AssetDatabase.ImportAsset(path);
   }
  }
  private static void ConfigureHud()
  {
   var session=UnityEngine.Object.FindFirstObjectByType<PrototypeNetworkSession>();if(session==null)return;
   var jump=UnityEngine.Object.FindObjectsByType<PrototypeScreenButton>(FindObjectsInactive.Include).FirstOrDefault(x=>x.controlPath=="<Gamepad>/buttonWest");if(jump==null)return;
   var parent=jump.transform.parent;var surface=parent.Find("Surface");
   if(surface==null){surface=UnityEngine.Object.Instantiate(jump.gameObject,parent).transform;surface.name="Surface";surface.GetComponent<PrototypeScreenButton>().controlPath="<Gamepad>/buttonNorth";surface.GetComponentInChildren<UnityEngine.UI.Text>().text="Surface";}
   ((RectTransform)jump.transform).anchoredPosition=new Vector2(-330,170);((RectTransform)surface).anchoredPosition=new Vector2(-330,360);
   var hud=parent.GetComponent<PrototypeSwimmingHud>();if(hud==null)hud=parent.gameObject.AddComponent<PrototypeSwimmingHud>();Ref(hud,"session",session);Ref(hud,"jumpLabel",jump.GetComponentInChildren<UnityEngine.UI.Text>());Ref(hud,"surface",surface.gameObject);surface.gameObject.SetActive(false);
  }
  private static Material stone,wood,water,marker;
  private static void Ref(UnityEngine.Object o,string field,UnityEngine.Object value){var s=new SerializedObject(o);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
  private static void Float(UnityEngine.Object o,string field,float value){var s=new SerializedObject(o);s.FindProperty(field).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
  private static void Array(UnityEngine.Object o,string field,UnityEngine.Object[] value){var s=new SerializedObject(o);var p=s.FindProperty(field);p.arraySize=value.Length;for(int i=0;i<value.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=value[i];s.ApplyModifiedPropertiesWithoutUndo();}
  private static Material Mat(string name,Color color,bool transparent=false)
  {string path="Assets/prototype/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find(transparent?"Two Tails/Prototype Visible Transparent":"Universal Render Pipeline/Lit"));m.SetColor("_BaseColor",color);AssetDatabase.CreateAsset(m,path);}return m;}
  private static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,Material material,bool solid=true)
  {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.position=position;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=material;if(!solid)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
  private static Transform Point(Transform parent,string name,Vector3 pos){var g=new GameObject(name);g.transform.SetParent(parent);g.transform.position=pos;return g.transform;}
  private static TextMesh Label(Transform parent,string text,Vector3 position)
  {var t=Point(parent,"Label",position).gameObject.AddComponent<TextMesh>();t.text=text;t.fontSize=48;t.characterSize=.018f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;return t;}
  private static void Ramp(Transform parent,string name,Vector3 a,Vector3 b)
  {var rotation=Quaternion.LookRotation((b-a).normalized,Vector3.up);var g=Box(parent,name,(a+b)*.5f-rotation*Vector3.up*.15f,new Vector3(4,.3f,Vector3.Distance(a,b)+.5f),stone);g.transform.rotation=rotation;}
  [MenuItem("Tools/Two Tails/Configure Animal Mechanisms")]
  public static void Configure()
  {
   if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save scene and exit Play Mode.");
   ConfigureInput();
   string previous=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
   stone=Mat("PrototypeBasinStone",new Color(.35f,.42f,.46f));wood=Mat("PrototypeDamWood",new Color(.35f,.14f,.045f));water=Mat("PrototypeWater",new Color(.08f,.4f,.85f,.35f),true);marker=Mat("PrototypeDamMarker",new Color(1,.08f,.05f,.35f),true);
   string prefab="Assets/prototype/Prefabs/NetworkPlayerPrototype.prefab";var player=PrefabUtility.LoadPrefabContents(prefab);if(player.GetComponent<AnimalMechanicsActor>()==null)player.AddComponent<AnimalMechanicsActor>();if(player.GetComponent<PrototypeSwimmer>()==null)player.AddComponent<PrototypeSwimmer>();PrefabUtility.SaveAsPrefabAsset(player,prefab);PrefabUtility.UnloadPrefabContents(player);
   foreach(string path in new[]{"Assets/prototype/Scenes/Prototype_SplitScreen.unity","Assets/prototype/Scenes/Prototype_OnlineTest.unity"})
   {
    var scene=EditorSceneManager.OpenScene(path);foreach(var p in UnityEngine.Object.FindObjectsByType<PrototypeMotor>(FindObjectsInactive.Include))if(p.GetComponent<AnimalMechanicsActor>()==null)p.gameObject.AddComponent<AnimalMechanicsActor>();
    foreach(var p in UnityEngine.Object.FindObjectsByType<PrototypeMotor>(FindObjectsInactive.Include))if(p.GetComponent<PrototypeSwimmer>()==null)p.gameObject.AddComponent<PrototypeSwimmer>();
    ConfigureHud();
    if(GameObject.Find("Animal Mechanisms")!=null)
    {
     var existing=GameObject.Find("Animal Mechanisms").GetComponent<AnimalMechanicsWorld>();
     if(existing!=null)existing.ConfigureExtendedCourseInEditor();
     EditorSceneManager.SaveScene(scene);
     continue;
    }
    var root=new GameObject("Animal Mechanisms").transform;var world=root.gameObject.AddComponent<AnimalMechanicsWorld>();bool online=path.Contains("Online");var ws=new SerializedObject(world);ws.FindProperty("networked").boolValue=online;ws.ApplyModifiedPropertiesWithoutUndo();if(online){root.gameObject.AddComponent<NetworkObject>();root.gameObject.AddComponent<AnimalMechanicsNetwork>();}
    var zone=Point(root,"Safe Swap Lobby",new Vector3(0,1.5f,-4.5f)).gameObject.AddComponent<BoxCollider>();zone.isTrigger=true;zone.size=new Vector3(16,3,5);Ref(world,"lobby",zone);
    var displays=new Transform[2];var visuals=new Renderer[2];
    for(int i=0;i<2;i++)
    {
     displays[i]=Point(root,i==0?"Otter Display":"Beaver Display",new Vector3(i==0?-5:5,.04f,-5));var col=displays[i].gameObject.AddComponent<CapsuleCollider>();col.height=2;col.radius=.4f;col.center=Vector3.up;
     var v=GameObject.CreatePrimitive(PrimitiveType.Capsule);v.name="Visual";v.transform.SetParent(displays[i],false);v.transform.localPosition=Vector3.up;UnityEngine.Object.DestroyImmediate(v.GetComponent<Collider>());visuals[i]=v.GetComponent<Renderer>();visuals[i].sharedMaterial=stone;Label(displays[i],i==0?"Otter":"Beaver",displays[i].position+Vector3.up*2.4f);
    }
    Array(world,"displays",displays);Array(world,"displayVisuals",visuals);
    var reset=Point(root,"Animal Test Reset",new Vector3(0,.04f,-6));Box(reset,"Reset Marker",reset.position+Vector3.up*.1f,new Vector3(1,.2f,1),wood);Label(reset,"Reset Animal Test\nInteract",reset.position+Vector3.up*.6f);Ref(world,"resetPoint",reset);
    GameObject.Find("Greybox").transform.Find("SouthWall").gameObject.SetActive(false);
    Box(root,"Animal Test Walkway",new Vector3(0,-.25f,-10),new Vector3(48,.5f,6),stone);
    Box(root,"Pool Under Ramp Wall",new Vector3(-14,-2.6f,-15),new Vector3(4,2.8f,.5f),stone);
    Box(root,"Reservoir Under Entry Wall",new Vector3(12,-3.05f,-15),new Vector3(4,3.9f,.5f),stone);
    Box(root,"Reservoir Under Exit Wall",new Vector3(12,-3.55f,-35),new Vector3(4,2.9f,.5f),stone);
    Pool(root,"Swimming Pool",new Vector3(-14,0,-23),12,16,-4,-.6f,-.6f,false);
    var reservoir=Pool(root,"Rising Reservoir",new Vector3(12,0,-25),12,20,-5,-4,0,true);Ref(world,"reservoir",reservoir);
    Ramp(root,"Reservoir Entry Ramp",new Vector3(12,0,-13),new Vector3(12,-4.5f,-25));
    Box(root,"Dam Footing",new Vector3(12,-4.75f,-25),new Vector3(4,.5f,3),stone);
    var dam=Point(root,"Dam",new Vector3(12,.04f,-10));Ref(world,"dam",dam);
    var mark=Box(dam,"Dam Marker",dam.position+Vector3.up*.6f,new Vector3(3,1.2f,.7f),marker,false);Ref(world,"damMarker",mark.GetComponent<Renderer>());Ref(world,"damLabel",Label(dam,"Add Log (0/3)",dam.position+Vector3.up*2));
    var placed=new GameObject[3];for(int i=0;i<3;i++)placed[i]=Box(dam,"Installed Log "+i,dam.position+new Vector3((i-1)*1.05f,.55f,0),new Vector3(.9f,.35f,.5f),wood);Array(world,"installedLogs",placed);
    var logs=new Transform[3];for(int i=0;i<3;i++){float z=-16-i*3;float y=(z+13)*.375f;logs[i]=Box(root,"Collectible Log "+i,new Vector3(13,y+.2f,z),new Vector3(.8f,.3f,.35f),wood).transform;Label(logs[i],"Log",logs[i].position+Vector3.up*.7f);}Array(world,"logs",logs);
    Ramp(root,"Far Bank Exit Ramp",new Vector3(12,-4.5f,-30),new Vector3(12,0,-38));Box(root,"Far Bank",new Vector3(12,-.25f,-40),new Vector3(10,.5f,4),stone);Label(root,"OTTER EXIT",new Vector3(12,2,-39));
    Label(root,"ANIMAL TESTS\nLeft: Swimming Pool | Right: Beaver Dam\nNon-Otter deep-water rule: return to shore",new Vector3(0,3,-13));
    Lamp(root,"Pool Lamp",new Vector3(-14,5,-20));Lamp(root,"Dam Lamp",new Vector3(12,4,-25));
    EditorSceneManager.SaveScene(scene);
   }
   EditorSceneManager.OpenScene(previous);AssetDatabase.SaveAssets();
  }
  private static PrototypeWaterVolume Pool(Transform parent,string name,Vector3 c,float width,float length,float bottom,float initial,float target,bool farExit)
  {
   var root=Point(parent,name,c);var waterVolume=root.gameObject.AddComponent<BoxCollider>();waterVolume.isTrigger=true;waterVolume.center=new Vector3(0,(bottom+target)*.5f-c.y,0);waterVolume.size=new Vector3(width,target-bottom+1,length);var waterComponent=root.gameObject.AddComponent<PrototypeWaterVolume>();Float(waterComponent,"bottom",bottom);Float(waterComponent,"initialLevel",initial);Float(waterComponent,"targetLevel",target);
   var visual=Box(root,"Water",new Vector3(c.x,(bottom+initial)*.5f,c.z),new Vector3(width,initial-bottom,length),water,false);Ref(waterComponent,"waterVisual",visual.transform);Ref(waterComponent,"safeShore",Point(root,"Safe Shore",new Vector3(c.x,.1f,-12)));
   Box(root,"Basin Bottom",new Vector3(c.x,bottom-.25f,c.z),new Vector3(width+.5f,.5f,length+.5f),stone);
   for(int sign=-1;sign<=1;sign+=2)Box(root,"Side Wall "+sign,new Vector3(c.x+sign*width*.5f,(bottom+1)*.5f,c.z),new Vector3(.5f,1-bottom,length),stone);
   for(int side=-1;side<=1;side+=2){float z=c.z+side*length*.5f;if(side==-1&&!farExit)Box(root,"Back Wall",new Vector3(c.x,(bottom+1)*.5f,z),new Vector3(width,1-bottom,.5f),stone);else for(int sign=-1;sign<=1;sign+=2)Box(root,"Ramp Opening Wall "+side+sign,new Vector3(c.x+sign*4,(bottom+1)*.5f,z),new Vector3(4,1-bottom,.5f),stone);}
   if(!farExit)Ramp(root,"Pool Entry Ramp",new Vector3(c.x,0,-13),new Vector3(c.x,bottom,-22));
   for(int i=1;i<=3;i++){Box(root,"Depth Reference "+i,new Vector3(c.x-width*.5f+1,bottom+i*.5f,c.z),new Vector3(.25f,i,.25f),wood);Label(root,"Depth "+i+"m",new Vector3(c.x-width*.5f+1,bottom+i+.2f,c.z));}
   return waterComponent;
  }
  private static void Lamp(Transform root,string name,Vector3 pos){var g=Point(root,name,pos);g.rotation=Quaternion.Euler(90,0,0);var l=g.gameObject.AddComponent<Light>();l.type=LightType.Spot;l.range=18;l.spotAngle=100;l.intensity=35;l.shadows=LightShadows.Soft;var d=g.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();var s=new SerializedObject(d);s.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=1;s.ApplyModifiedPropertiesWithoutUndo();}
 }
}




