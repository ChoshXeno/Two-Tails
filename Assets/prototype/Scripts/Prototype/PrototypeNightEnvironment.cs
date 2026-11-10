using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace TwoTails.Prototype
{
 public sealed class PrototypeNightEnvironment : MonoBehaviour
 {
  [SerializeField, Tooltip("Prototype-only URP material shader, never applied outside this scene.")] private Shader nightShader;
  [SerializeField, Min(0), Tooltip("Monkey fully clear radius in world metres from its feet.")] private float visibleRadius=3;
  [SerializeField, Min(0.01f), Tooltip("World-space fade width beyond the clear radius.")] private float fadeWidth=3;
  [SerializeField, Range(0,2), Tooltip("Unlit surface brightness for animal night vision.")] private float nightBrightness=0.28f;
  [SerializeField, Tooltip("Cool night fill, modulated by world-space surface orientation.")] private Color nightTint=new Color(0.38f,0.52f,0.75f);
  public bool FoxNightVision { get; private set; }=true;
  [SerializeField, Tooltip("Local developer daylight source; never networked.")] private Light daylight;
  public bool DeveloperLights { get; private set; }
  private readonly Dictionary<Material,Material> copies=new();


  private GameObject ui;
  private void Awake(){DeveloperLights=false;FoxNightVision=true;if(daylight!=null)daylight.enabled=false;}
  private void OnEnable(){RenderPipelineManager.beginCameraRendering+=BeforeCamera;RenderPipelineManager.endCameraRendering+=AfterCamera;}
  private void Start()
  {
   foreach(var renderer in FindObjectsByType<MeshRenderer>())Prepare(renderer);
   if(Application.isEditor||Debug.isDebugBuild)
   {
    ui=PrototypeTestMenu.CanvasRoot("Developer Lighting");
    var safe=PrototypeTestMenu.Rect("SafeArea",ui.transform,Vector2.zero,Vector2.zero);
    safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;safe.gameObject.AddComponent<PrototypeSafeArea>();
    PrototypeTestMenu.ButtonAt(safe,"Developer Lights",new Vector2(0.87f,0.82f),Toggle,new Vector2(320,90));
    PrototypeTestMenu.ButtonAt(safe,"Fox Vision A/B (N)",new Vector2(0.87f,0.72f),ToggleFox,new Vector2(320,90));
   }
  }
  public void Prepare(Renderer renderer)
  {
   if(renderer==null||nightShader==null||renderer.GetComponent<TextMesh>()!=null)return;
   var materials=renderer.sharedMaterials;
   for(int i=0;i<materials.Length;i++)
   {
    var source=materials[i];if(source==null||source.shader==nightShader)continue;
    if(!copies.TryGetValue(source,out var clone))
    {
     clone=new Material(nightShader){name=source.name+" (Level 1 runtime)"};
     if(source.HasProperty("_BaseColor"))clone.SetColor("_BaseColor",source.GetColor("_BaseColor"));
     copies.Add(source,clone);
    }
    materials[i]=clone;
   }
   renderer.sharedMaterials=materials;
  }
  public void ToggleFox(){if(Application.isEditor||Debug.isDebugBuild)FoxNightVision=!FoxNightVision;}
  public void Toggle()
  {
   if(!(Application.isEditor||Debug.isDebugBuild))return;
   DeveloperLights=!DeveloperLights;if(daylight!=null)daylight.enabled=DeveloperLights;
  }
  private void Update()
  {
   if(!(Application.isEditor||Debug.isDebugBuild)||!Application.isFocused)return;
   if(EventSystem.current?.currentSelectedGameObject?.GetComponent<InputField>()?.isFocused==true)return;
   if(Keyboard.current?.pKey.wasPressedThisFrame==true)Toggle();
   if(Keyboard.current?.nKey.wasPressedThisFrame==true)ToggleFox();
  }
  private void BeforeCamera(ScriptableRenderContext context,Camera camera)
  {
   var follow=camera.GetComponent<PrototypeFollowCamera>();
   var actor=follow!=null&&follow.Target!=null?follow.Target.GetComponent<PrototypeAnimal>():null;
   bool fox=actor!=null&&actor.Species==PrototypeSpecies.Fox;
   Shader.SetGlobalVector("_AnimalVision",new Vector4(visibleRadius,fadeWidth,nightBrightness,DeveloperLights||(fox&&FoxNightVision)?1:0));
   Shader.SetGlobalVector("_AnimalPosition",actor!=null?actor.transform.position:Vector3.zero);
   Shader.SetGlobalColor("_AnimalNightTint",nightTint);
   Shader.SetGlobalFloat("_AnimalVisionEnabled",1);
   camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=DeveloperLights?new Color(0.25f,0.35f,0.5f):Color.black;
  }
  private void AfterCamera(ScriptableRenderContext context,Camera camera){Shader.SetGlobalFloat("_AnimalVisionEnabled",0);}
  private void OnDisable(){RenderPipelineManager.beginCameraRendering-=BeforeCamera;RenderPipelineManager.endCameraRendering-=AfterCamera;Shader.SetGlobalFloat("_AnimalVisionEnabled",0);DeveloperLights=false;FoxNightVision=true;if(daylight!=null)daylight.enabled=false;}
  private void OnDestroy(){foreach(var m in copies.Values)Destroy(m);if(ui!=null)Destroy(ui);}
 }
}



