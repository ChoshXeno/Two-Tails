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
   float4 _BaseMap_ST;
   CBUFFER_END
   TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
   #include "PrototypeVisibility.hlsl"
   struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
   struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; float2 uv:TEXCOORD2; };
   Varyings Vert(Attributes a){ Varyings o;o.positionWS=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(a.normalOS);o.uv=TRANSFORM_TEX(a.uv,_BaseMap);return o; }
   half4 Frag(Varyings i):SV_Target
   {
    half3 normal=normalize(i.normalWS);
    float visibility=AnimalVisibility(i.positionWS+normal*.025);

    InputData data=(InputData)0;
    data.positionWS=i.positionWS;data.normalWS=normal;
    data.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);
    data.shadowCoord=TransformWorldToShadowCoord(i.positionWS);
    data.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);
    data.shadowMask=half4(1,1,1,1);
    // Low cool hemispherical fill reveals shapes without flattening direct-light shadows.
    half hemi=0.18+0.82*saturate(dot(normal,normalize(half3(-0.4,0.8,-0.3))));
    bool nearReveal=_AnimalVisionEnabled>.5 && _AnimalVision.w<.5;
    data.bakedGI=nearReveal?half3(0,0,0):lerp(half3(.03,.04,.06),_AnimalNightTint.rgb*visibility*_AnimalVision.z*hemi,_AnimalVisionEnabled);
    SurfaceData surface=(SurfaceData)0;
    surface.albedo=_BaseColor.rgb*SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;surface.alpha=1;surface.occlusion=1;
    surface.smoothness=0.18;surface.normalTS=half3(0,0,1);
    half4 lit=UniversalFragmentPBR(data,surface);
    float localLight=AnimalLocalLight(i.positionWS,i.positionCS);
    if(nearReveal)
    {
     // Fixed diffuse fill carries no view-dependent highlight or shadow map.
     // The locally lit component eases into darkness instead of switching at
     // the visibility threshold; real scene-light shadows are suppressed at night.
     half3 fill=surface.albedo*_AnimalNightTint.rgb*_AnimalVision.z*hemi*visibility;
     float lightReveal=smoothstep(0,max(_AnimalLightFadeWidth,.001),saturate(localLight));
     lit.rgb=lit.rgb*max(visibility,lightReveal)+fill;
    }
    else lit.rgb=AnimalFinish(lit.rgb,visibility,localLight);
    return lit;
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
