Shader "Two Tails/Prototype Animal Vision"
{
 Properties { _BaseColor("Color",Color)=(1,1,1,1) _BaseMap("Base",2D)="white"{} _Cutoff("Cutoff",Float)=0.5 _Cull("Cull",Float)=2 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
  Pass
  {
   Tags { "LightMode"="UniversalForwardOnly" }
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma target 3.5
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;
   CBUFFER_END
   float4 _AnimalVision;
   float3 _AnimalPosition;
   float _AnimalVisionEnabled;
   half4 _AnimalNightTint;
   struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
   struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; };
   Varyings Vert(Attributes a){ Varyings o;o.positionWS=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(a.normalOS);return o; }
   half4 Frag(Varyings i):SV_Target
   {
    half3 normal=normalize(i.normalWS);
    float visibility=1-smoothstep(_AnimalVision.x,_AnimalVision.x+max(_AnimalVision.y,0.01),distance(i.positionWS,_AnimalPosition));
    visibility=max(visibility,_AnimalVision.w);
    InputData data=(InputData)0;
    data.positionWS=i.positionWS;data.normalWS=normal;
    data.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);
    data.shadowCoord=TransformWorldToShadowCoord(i.positionWS);
    data.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);
    data.shadowMask=half4(1,1,1,1);
    // Low cool hemispherical fill reveals shapes without flattening direct-light shadows.
    half hemi=0.18+0.82*saturate(dot(normal,normalize(half3(-0.4,0.8,-0.3))));
    data.bakedGI=lerp(half3(.03,.04,.06),_AnimalNightTint.rgb*visibility*_AnimalVision.z*hemi,_AnimalVisionEnabled);
    SurfaceData surface=(SurfaceData)0;
    surface.albedo=_BaseColor.rgb;surface.alpha=1;surface.occlusion=1;
    surface.smoothness=0.18;surface.normalTS=half3(0,0,1);
    return UniversalFragmentPBR(data,surface);   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}

