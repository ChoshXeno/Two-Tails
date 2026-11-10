using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using TwoTails.Prototype;
namespace TwoTails.Editor
{
 public static class CooperativePuzzleSetup
 {
  private static Material floor,wall,accent;
  private static void Ref(UnityEngine.Object obj,string name,UnityEngine.Object value){var s=new SerializedObject(obj);s.FindProperty(name).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
  private static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,Material mat)
  {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;return g;}
  private static BoxCollider Volume(Transform parent,string name,Vector3 pos,Vector3 size)
  {var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;var c=g.AddComponent<BoxCollider>();c.size=size;c.isTrigger=true;return c;}
  private static TextMesh Sign(Transform parent,string name,string text,Vector3 p)
  {var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=p;var t=g.AddComponent<TextMesh>();t.text=text;t.fontSize=48;t.characterSize=.06f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=Color.white;return t;}
  private static Material Mat(string name,Color color)
  {string p="Assets/prototype/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.18f);AssetDatabase.CreateAsset(m,p);}return m;}
  [MenuItem("Tools/Two Tails/Configure Cooperative Test Areas")]
  public static void Configure()
  {
   if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save scene and exit Play Mode.");
   string old=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
   floor=Mat("PrototypeCoopFloor",new Color(.3f,.37f,.4f));wall=Mat("PrototypeCoopWall",new Color(.48f,.5f,.54f));accent=Mat("PrototypeCoopAccent",new Color(.9f,.58f,.12f));
   foreach(string path in new[]{"Assets/prototype/Scenes/Prototype_SplitScreen.unity","Assets/prototype/Scenes/Prototype_OnlineTest.unity"})
   {
    var scene=EditorSceneManager.OpenScene(path);bool online=path.Contains("Online");
    if(GameObject.Find("Cooperative Test Areas")!=null)continue;
    var root=new GameObject("Cooperative Test Areas").transform;
    GameObject.Find("Greybox").transform.Find("NorthWall").gameObject.SetActive(false);
    Box(root,"Arrival Plaza",new Vector3(0,-.25f,11),new Vector3(66,.5f,8),floor);
    Sign(root,"Directory","COOPERATIVE TESTS\nLeft: TT-P05 | Center: TT-P01 | Right: TT-P02",new Vector3(0,3,14));
    Build(root,CooperativePuzzleKind.Switchback,new Vector3(-22,0,20),online);
    Build(root,CooperativePuzzleKind.Bridge,new Vector3(0,0,20),online);
    Build(root,CooperativePuzzleKind.Counterweight,new Vector3(22,0,20),online);
    EditorSceneManager.SaveScene(scene);
   }
   EditorSceneManager.OpenScene(old);AssetDatabase.SaveAssets();
  }
  private static void Build(Transform parent,CooperativePuzzleKind kind,Vector3 pos,bool online)
  {
   string id=kind==CooperativePuzzleKind.Switchback?"TT-P05 Switchback Gates":kind==CooperativePuzzleKind.Bridge?"TT-P01 Bridge Handoff":"TT-P02 Counterweight Lift";
   var root=new GameObject(id).transform;root.SetParent(parent,false);root.localPosition=pos;
   var puzzle=root.gameObject.AddComponent<CooperativePuzzle>();var so=new SerializedObject(puzzle);so.FindProperty("kind").enumValueIndex=(int)kind;so.FindProperty("networked").boolValue=online;so.ApplyModifiedPropertiesWithoutUndo();
   if(online){root.gameObject.AddComponent<NetworkObject>();root.gameObject.AddComponent<CooperativePuzzleNetwork>();}
   Sign(root,"Instructions",id+"\n"+(kind==CooperativePuzzleKind.Counterweight?"Push Weight 1 crates onto right lift\nBoth players + crates: 4 / 4 locks exit":"A holds first plate; B reaches second\nHandoff locks route until Reset"),new Vector3(0,6,3));
   var reset=Volume(root,"Reset Tile",new Vector3(-6,.2f,-3),new Vector3(2,.5f,2));Ref(puzzle,"resetPad",reset);
   Box(root,"Reset Tile Visual",new Vector3(-6,.04f,-3),new Vector3(2,.08f,2),accent);
   Sign(root,"Reset Instructions","RESET\nStand here 2 seconds",new Vector3(-6,1.6f,-3));
   Box(root,"Reset Ground",new Vector3(-6,-.25f,-3),new Vector3(4,.5f,4),floor);
   Transform ma,mb;BoxCollider a,b;Renderer av,bv;
   if(kind==CooperativePuzzleKind.Switchback)
   {
    Box(root,"Route Floor",new Vector3(0,-.25f,8),new Vector3(10,.5f,26),floor);
    Box(root,"Divider",new Vector3(0,2,6),new Vector3(.5f,4,22),wall);
    Box(root,"West Wall",new Vector3(-5,2,8),new Vector3(.5f,4,26),wall);Box(root,"East Wall",new Vector3(5,2,8),new Vector3(.5f,4,26),wall);
    ma=Box(root,"Gate A",new Vector3(-2.5f,2,6),new Vector3(4.5f,4,.5f),accent).transform;
    mb=Box(root,"Gate B",new Vector3(2.5f,2,2),new Vector3(4.5f,4,.5f),accent).transform;
    a=Plate(root,"Plate A",new Vector3(-2.5f,0,0),out av);b=Plate(root,"Plate B",new Vector3(2.5f,0,8),out bv);
    Sign(root,"Meeting","SAFE MEETING POINT",new Vector3(0,2,20));
   }
   else if(kind==CooperativePuzzleKind.Bridge)
   {
    Box(root,"Start Bank",new Vector3(0,-.25f,0),new Vector3(10,.5f,8),floor);
    ma=Box(root,"Front Bridge",new Vector3(0,-.25f,8),new Vector3(4,.5f,8),accent).transform;
    Box(root,"Middle Island",new Vector3(0,-.25f,14),new Vector3(6,.5f,4),floor);
    mb=Box(root,"Rear Bridge",new Vector3(0,-.25f,20),new Vector3(4,.5f,8),accent).transform;
    Box(root,"Exit Bank",new Vector3(0,-.25f,27),new Vector3(10,.5f,6),floor);
    a=Plate(root,"Plate A",new Vector3(-2,0,1),out av);b=Plate(root,"Plate B",new Vector3(0,0,14),out bv);
    Sign(root,"Exit","MEET HERE\nReset after a failed attempt",new Vector3(0,2,28));
   }
   else
   {
    Box(root,"Loading Deck",new Vector3(0,-.25f,-3),new Vector3(18,.5f,6),floor);
    ma=Box(root,"Left Lift",new Vector3(-3.5f,-.25f,3),new Vector3(5,.5f,5),accent).transform;
    mb=Box(root,"Right Lift",new Vector3(3.5f,-.25f,3),new Vector3(5,.5f,5),accent).transform;
    // Detection transforms keep world-unit size despite scaled cube meshes.
    a=Volume(root,"Left Load",new Vector3(-3.5f,.2f,3),new Vector3(5,.45f,5));a.transform.SetParent(ma,true);
    b=Volume(root,"Right Load",new Vector3(3.5f,.2f,3),new Vector3(5,.45f,5));b.transform.SetParent(mb,true);
    av=ma.GetComponent<Renderer>();bv=mb.GetComponent<Renderer>();
    Box(root,"Exit Deck",new Vector3(0,-.25f,8),new Vector3(14,.5f,5),floor);
    var gate=Box(root,"Balance Exit Gate",new Vector3(0,2,6),new Vector3(14,4,.5f),wall);Ref(puzzle,"exitGate",gate.transform);
    for(int k=0;k<2;k++)
    {
     var deck=k==0?ma:mb;var ramp=Box(root,"Loading Ramp "+k,new Vector3(k==0?-3.5f:3.5f,-.2f,.1f),new Vector3(5,.2f,2),floor);Ref(puzzle,k==0?"rampA":"rampB",ramp.transform);
    }
    var arr=new CooperativeWeightBox[3];
    for(int i=0;i<3;i++)
    {
     var g=Box(root,"Weight "+(i==0?2:1)+" Crate "+i,i==0?new Vector3(-3.5f,.5f,3):new Vector3(2.5f+(i-1)*2,.5f,-2),Vector3.one,accent);g.layer=7;
     arr[i]=g.AddComponent<CooperativeWeightBox>();var ws=new SerializedObject(arr[i]);ws.FindProperty("weight").intValue=i==0?2:1;ws.ApplyModifiedPropertiesWithoutUndo();
     Sign(g.transform,"Weight Label","Weight "+(i==0?2:1),new Vector3(0,.8f,0));
    }
    var ps=new SerializedObject(puzzle);var bp=ps.FindProperty("boxes");bp.arraySize=3;for(int i=0;i<3;i++)bp.GetArrayElementAtIndex(i).objectReferenceValue=arr[i];ps.ApplyModifiedPropertiesWithoutUndo();
   }
   Ref(puzzle,"plateA",a);Ref(puzzle,"plateB",b);Ref(puzzle,"plateVisualA",av);Ref(puzzle,"plateVisualB",bv);Ref(puzzle,"movingA",ma);Ref(puzzle,"movingB",mb);
   Ref(puzzle,"status",Sign(root,"State","Ready",new Vector3(0,4,3)));
   var lightObj=new GameObject("Route Light",typeof(Light));lightObj.transform.SetParent(root,false);lightObj.transform.localPosition=new Vector3(0,7,6);lightObj.transform.localRotation=Quaternion.Euler(90,0,0);var lamp=lightObj.GetComponent<Light>();lamp.type=LightType.Spot;lamp.range=22;lamp.spotAngle=120;lamp.intensity=45;lamp.shadows=LightShadows.Soft;lamp.shadowBias=.02f;lamp.shadowNormalBias=.2f;var ld=lightObj.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();var ls=new SerializedObject(ld);ls.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=1;ls.ApplyModifiedPropertiesWithoutUndo();
  }
  private static BoxCollider Plate(Transform root,string name,Vector3 feet,out Renderer visual)
  {var volume=Volume(root,name,feet+Vector3.up*.2f,new Vector3(1.8f,.45f,1.8f));visual=Box(root,name+" Visual",feet+Vector3.up*.04f,new Vector3(1.8f,.08f,1.8f),accent).GetComponent<Renderer>();return volume;}
 }
}



