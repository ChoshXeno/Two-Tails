using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.Collections.Generic;
public static class BuildElephant
{
 const string Dir="Assets/Art/Finished Animation/Elephant/";
 public static object Build()
 {
  if(AssetDatabase.LoadAssetAtPath<AnimatorController>(Dir+"Elephant.controller")!=null)
   throw new System.InvalidOperationException("Elephant assets already exist; edit existing assets to preserve GUIDs.");
  var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Elephant.fbx");
  var wrapper=new GameObject("TwoTailsElephantVisual");
  var model=Object.Instantiate(source,wrapper.transform); model.name="Elephant";
  var old=model.GetComponent<Animator>(); if(old!=null)Object.DestroyImmediate(old);
  var skin=model.GetComponentInChildren<SkinnedMeshRenderer>();
  skin.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/MaterialPreparation/Elephant/Preview_URP.mat");
  var mesh=new Mesh(); skin.BakeMesh(mesh); var verts=mesh.vertices.Select(v=>wrapper.transform.InverseTransformPoint(skin.transform.TransformPoint(v))).ToArray();
  float height=verts.Max(v=>v.y)-verts.Min(v=>v.y); float scale=1.65f/height;
  model.transform.localScale=Vector3.one*scale; model.transform.localPosition=Vector3.up*(-verts.Min(v=>v.y)*scale);
  var bones=skin.bones.Distinct().ToArray(); var rest=bones.ToDictionary(b=>b,b=>b.localRotation);
  var root=model.transform.Find("purebind_armature/root");
  var avatar=AvatarBuilder.BuildGenericAvatar(model, "purebind_armature"); avatar.name="ElephantAvatar";
  AssetDatabase.CreateAsset(avatar,Dir+"ElephantAvatar.asset");
  var ctrl=AnimatorController.CreateAnimatorControllerAtPath(Dir+"Elephant.controller"); ctrl.AddParameter("GripSpeed",AnimatorControllerParameterType.Float);
  var clips=new Dictionary<string,AnimationClip>();
  string[] names={"Idle","Sit","Run","Jump_Start","Jump_Air","Jump_Land","Interact","Death","Push","Push_Hold","Pull","Pull_Hold","Strike"};
  foreach(string name in names)
  {
   float length=name=="Sit"||name=="Idle"?2.4f:name=="Run"||name.Contains("Push")||name.Contains("Pull")?.9f:name=="Jump_Start"?.27f:name=="Jump_Land"?.4f:name=="Strike"?.8f:.65f;
   bool loop=name=="Idle"||name=="Sit"||name=="Run"||name=="Jump_Air"||name.Contains("Push")||name.Contains("Pull");
   var clip=new AnimationClip{name="Elephant_"+name,frameRate=60};
   foreach(var b in bones)
   {
    var keys=new List<Keyframe>[] {new List<Keyframe>(),new List<Keyframe>(),new List<Keyframe>(),new List<Keyframe>()};
    for(int i=0;i<=60;i++)
    {
     float t=i/60f, wave=Mathf.Sin(t*Mathf.PI*2), pulse=Mathf.Sin(t*Mathf.PI); float angle=0;
     bool step=name=="Run"||name=="Push"||name=="Pull"; bool front=b.name.StartsWith("frontLwr"); bool rear=b.name.StartsWith("rearLwr");
     float phase=t*Mathf.PI*2+(b.name.EndsWith(".R")?Mathf.PI:0)+(rear?Mathf.PI:0);
     if(step&&(front||rear))angle=Mathf.Sin(phase)*(name=="Run"?18:11)*(name=="Pull"?-1:1);
     if(step&&(b.name.StartsWith("frontLeg")||b.name.StartsWith("rearHorselink")))angle=Mathf.Max(0,Mathf.Cos(phase))*12;
     if(b.name=="chest")angle+=(name=="Idle"||name=="Sit"?.35f*wave:0);
     if(name=="Sit"&&rear)angle+=32;
     if(name=="Sit"&&b.name=="hips")angle-=12;
     if(b.name=="neck1")angle+=name=="Interact"?-10*pulse:name.Contains("Push")?9:name.Contains("Pull")?-7:0;
     if(name=="Strike"&&b.name=="frontLwr.L")angle-=45*pulse;
     if(name=="Strike"&&b.name=="frontLeg.L_03")angle+=25*Mathf.Sin(t*Mathf.PI*2);
     if(name.StartsWith("Jump")&&(front||rear))angle+=name=="Jump_Air"?-15:-12*pulse;
     if(name=="Death"&&b.name=="chest")angle+=25*t;
     Quaternion q=rest[b]*Quaternion.AngleAxis(angle,Vector3.right);
     float[] v={q.x,q.y,q.z,q.w}; for(int k=0;k<4;k++)keys[k].Add(new Keyframe(t*length,v[k]));
    }
    string path=AnimationUtility.CalculateTransformPath(b,model.transform); string[] props={"x","y","z","w"};
    for(int k=0;k<4;k++)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(path,typeof(Transform),"m_LocalRotation."+props[k]),new AnimationCurve(keys[k].ToArray()));
   }
   clip.EnsureQuaternionContinuity(); var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);
   AssetDatabase.CreateAsset(clip,Dir+clip.name+".anim"); clips[name]=clip;
   if(name.EndsWith("_Hold"))continue;
   var state=ctrl.layers[0].stateMachine.AddState(clip.name);state.motion=clip;
  }
  foreach(string grip in new[]{"Push","Pull"})
  {
   var state=ctrl.layers[0].stateMachine.states.First(s=>s.state.name=="Elephant_"+grip).state;
   var tree=new BlendTree{name="Elephant_"+grip+"Speed",blendType=BlendTreeType.Simple1D,blendParameter="GripSpeed",useAutomaticThresholds=false};
   AssetDatabase.AddObjectToAsset(tree,ctrl);tree.AddChild(clips[grip+"_Hold"],0);tree.AddChild(clips[grip],1.2f);state.motion=tree;
  }
  var animator=model.AddComponent<Animator>();animator.avatar=avatar;animator.runtimeAnimatorController=ctrl;animator.applyRootMotion=false;
  AssetDatabase.CreateFolder(Dir.TrimEnd('/'),"Resources");
  PrefabUtility.SaveAsPrefabAsset(wrapper,Dir+"Resources/TwoTailsElephantVisual.prefab");
  Object.DestroyImmediate(wrapper);Object.DestroyImmediate(mesh);AssetDatabase.SaveAssets();
  return new {height,scale,clips=clips.Count,avatarValid=avatar.isValid};
 }
}
