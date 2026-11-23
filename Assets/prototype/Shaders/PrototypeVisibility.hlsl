#ifndef TWO_TAILS_VISIBILITY
#define TWO_TAILS_VISIBILITY
float4 _AnimalVision;
float3 _AnimalPosition;
float _AnimalVisionEnabled;
float _AnimalLightFadeWidth;
half4 _AnimalNightTint;
float4 _AnimalFox; // enabled, brightness, gamma, developer daylight
int _AnimalBlockerCount;
float4x4 _AnimalBlockers[64];
float AnimalVisibility(float3 positionWS)
{
 if(_AnimalVisionEnabled<.5)return 1;
 // Both flags are assigned for the camera currently rendering, never the other viewport.
 if(_AnimalFox.w>.5 || _AnimalFox.x>.5)return 1;
 float v=1-smoothstep(_AnimalVision.x,_AnimalVision.x+max(_AnimalVision.y,.01),distance(positionWS,_AnimalPosition));
 if(v<=0)return 0;
 float3 origin=_AnimalPosition+float3(0,1,0);
 for(int i=0;i<_AnimalBlockerCount;i++)
 {
  float3 a=mul(_AnimalBlockers[i],float4(origin,1)).xyz;
  float3 b=mul(_AnimalBlockers[i],float4(positionWS,1)).xyz;
  float3 inv=1.0/(b-a+float3(.0000001,.0000001,.0000001));
  float3 t0=(-.5-a)*inv,t1=(.5-a)*inv;
  float3 nearT=min(t0,t1),farT=max(t0,t1);
  float enter=max(max(nearT.x,nearT.y),nearT.z),leave=min(min(farT.x,farT.y),farT.z);
  if(enter<=leave&&leave>.001&&enter<.995)return 0;
 }
 return v;
}
float AnimalLocalLight(float3 positionWS,float4 positionCS)
{
 InputData inputData=(InputData)0;inputData.positionWS=positionWS;inputData.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(positionCS);
 float amount=0;uint count=GetAdditionalLightsCount();
 LIGHT_LOOP_BEGIN(count)
  Light l=GetAdditionalLight(lightIndex,positionWS,half4(1,1,1,1));
  amount+=max(l.color.r,max(l.color.g,l.color.b))*l.distanceAttenuation*l.shadowAttenuation;
 LIGHT_LOOP_END
 return saturate(amount);
}
half3 AnimalFinish(half3 lit,float visible,float localLight)
{
 float lightReveal=smoothstep(0,max(_AnimalLightFadeWidth,.001),saturate(localLight));
 float combinedReveal=max(visible,lightReveal);
 // Multiplication after PBR also removes reflection-probe/specular leakage at exactly zero visibility.
 if(_AnimalVisionEnabled>.5&&combinedReveal<=.00001)return 0;
 if(_AnimalVisionEnabled>.5)lit*=saturate(combinedReveal);
 if(_AnimalFox.x>.5)lit=pow(max(lit*_AnimalFox.y,0),1/max(_AnimalFox.z,.1));
 return lit;
}
float AnimalCombinedReveal(float visible,float localLight)
{
 return max(visible,smoothstep(0,max(_AnimalLightFadeWidth,.001),saturate(localLight)));
}
#endif

