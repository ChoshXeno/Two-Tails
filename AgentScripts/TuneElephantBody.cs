using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
public static class TuneElephantBody
{
 public static object Run()
 {
  string dir="Assets/Art/Finished Animation/Elephant/";
  var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(dir+"Resources/TwoTailsElephantVisual.prefab"));
  var animator=go.GetComponentInChildren<Animator>(); animator.enabled=false;
  Transform model=animator.transform;
  var skin=go.GetComponentInChildren<SkinnedMeshRenderer>();
  var bones=skin.bones.Distinct().ToDictionary(t=>t.name);
  var restPositions=bones.Values.ToDictionary(b=>b,b=>b.localPosition);
  var restRotations=bones.Values.ToDictionary(b=>b,b=>b.localRotation);
  string[] names={"Run","Interact","Jump_Start","Jump_Air","Jump_Land","Strike"};
  var report=new List<object>();
  foreach(string name in names)
  {
   var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(dir+"Elephant_"+name+".anim");
   var source=Object.Instantiate(clip);
   foreach(var binding in AnimationUtility.GetCurveBindings(source))
    if(new[]{"hips","chest","neck_01"}.Any(n=>binding.path.EndsWith("/"+n)))AnimationUtility.SetEditorCurve(source,binding,null);
   var curves=new Dictionary<EditorCurveBinding,List<Keyframe>>();
   float lo=float.PositiveInfinity,hi=float.NegativeInfinity;
   for(int i=0;i<=60;i++)
   {
    float t=i/60f,time=t*clip.length;
    foreach(var b in bones.Values){b.localPosition=restPositions[b];b.localRotation=restRotations[b];}
    source.SampleAnimation(model.gameObject,time);
    float wave=Mathf.Sin(t*Mathf.PI*2), pulse=Mathf.Sin(t*Mathf.PI); pulse*=pulse;
    float pitch=0,roll=0,heave=0,chest=0;
    switch(name)
    {
     case "Run": pitch=2.4f*wave;roll=1.7f*Mathf.Sin(t*Mathf.PI*2+.6f);heave=.018f*(1-Mathf.Cos(t*Mathf.PI*4));chest=-pitch*.5f;break;
     case "Interact": pitch=5f*Mathf.Sin(t*Mathf.PI*2)*pulse;heave=-.022f*pulse;chest=3f*pulse;break;
     case "Strike": pitch=-5f*pulse;roll=3.5f*pulse;heave=-.025f*pulse;chest=7f*Mathf.Sin(t*Mathf.PI*2)*pulse;break;
     case "Jump_Start": pitch=-5f*pulse;heave=-.055f*pulse;chest=3f*pulse;break;
     case "Jump_Air": pitch=-2f;heave=0;chest=.6f*wave;break;
     case "Jump_Land": pitch=5f*pulse;heave=-.065f*pulse;chest=-3f*pulse;break;
    }
    // Rotate around model-space lateral/forward axes, independent of FBX bone axes.
    var hips=bones["hips"];var torso=bones["chest"];
    hips.rotation=Quaternion.AngleAxis(pitch,model.right)*Quaternion.AngleAxis(roll,model.forward)*hips.rotation;
    hips.position+=model.up*heave;
    torso.rotation=Quaternion.AngleAxis(chest,model.right)*torso.rotation;
    var neck=bones["neck_01"];
    neck.rotation=Quaternion.AngleAxis(-pitch*.35f,model.right)*neck.rotation;
    foreach(var bone in new[]{hips,torso,neck})
    {
     string path=AnimationUtility.CalculateTransformPath(bone,model);
     var q=bone.localRotation;float[] values={q.x,q.y,q.z,q.w};string[] axes={"x","y","z","w"};
     for(int k=0;k<4;k++)Add(curves,path,"m_LocalRotation."+axes[k],time,values[k]);
     if(bone==hips){var p=bone.localPosition;Add(curves,path,"m_LocalPosition.x",time,p.x);Add(curves,path,"m_LocalPosition.y",time,p.y);Add(curves,path,"m_LocalPosition.z",time,p.z);}
    }
    lo=Mathf.Min(lo,heave);hi=Mathf.Max(hi,heave);
   }
   foreach(var item in curves)
   {
    var curve=new AnimationCurve(item.Value.ToArray());
    for(int i=0;i<curve.length;i++){AnimationUtility.SetKeyLeftTangentMode(curve,i,AnimationUtility.TangentMode.ClampedAuto);AnimationUtility.SetKeyRightTangentMode(curve,i,AnimationUtility.TangentMode.ClampedAuto);}
    AnimationUtility.SetEditorCurve(clip,item.Key,curve);
   }
   clip.EnsureQuaternionContinuity();EditorUtility.SetDirty(clip);
   report.Add(new {name,clip.length,heaveMin=lo,heaveMax=hi});Object.DestroyImmediate(source);
  }
  AssetDatabase.SaveAssets();Object.DestroyImmediate(go);return report;
 }
 static void Add(Dictionary<EditorCurveBinding,List<Keyframe>> curves,string path,string property,float time,float value)
 {
  var binding=EditorCurveBinding.FloatCurve(path,typeof(Transform),property);
  if(!curves.ContainsKey(binding))curves[binding]=new List<Keyframe>();
  curves[binding].Add(new Keyframe(time,value));
 }
}
