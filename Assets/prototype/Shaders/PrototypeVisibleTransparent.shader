Shader "Two Tails/Prototype Visible Transparent"
{
 Properties{_BaseColor("Color",Color)=(.1,.4,.8,.45) _MainTex("Texture",2D)="white"{} _IsText("Font mask",Float)=0}
 SubShader{
 Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
 Pass{
 Tags{"LightMode"="UniversalForward"}
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off
 ZTest LEqual
 Cull Off
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile _ _ADDITIONAL_LIGHTS
 #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 #include "PrototypeVisibility.hlsl"
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;float _IsText;
 CBUFFER_END
 struct A{float4 p:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;float2 uv:TEXCOORD1;half4 color:COLOR;};
 V Vert(A a){V o;o.world=TransformObjectToWorld(a.p.xyz);o.p=TransformWorldToHClip(o.world);o.uv=a.uv;o.color=a.color;return o;}
 half4 Frag(V i):SV_Target
 {
  float v=AnimalVisibility(i.world);float l=AnimalLocalLight(i.world,i.p);
  half alpha=_BaseColor.a*(_IsText>.5?SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a*i.color.a:1);
  float reveal=v;
  if(_AnimalVisionEnabled>.5&&reveal<=.00001)return 0;
  half3 color=_BaseColor.rgb*(_IsText>.5?i.color.rgb:half3(.25,.25,.25)+l);
  return half4(AnimalFinish(color,v,0),alpha*saturate(reveal));
 }
 ENDHLSL
 }
 }
}
