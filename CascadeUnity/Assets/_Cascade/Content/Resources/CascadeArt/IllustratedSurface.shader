Shader "CASCADE/IllustratedSurface"
{
 Properties {
  [MainTexture] _BaseMap("Illustration",2D)="white"{} [MainColor] _BaseColor("Tint",Color)=(1,1,1,1)
  _Relief("Surface volume",Range(0,2))=.75 _Emission("Painted lamps",Range(0,1))=0
  _Cutoff("Alpha cutoff",Range(0,1))=.025
 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
  Pass {
   Tags {"LightMode"="UniversalForward"}
   Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Assets/_Cascade/Presentation/IllustratedLighting.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseMap_ST;half4 _BaseColor;float _Relief,_Emission,_Cutoff;
   CBUFFER_END
   struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float2 local:TEXCOORD1;};
   V Vert(A v){V o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=TRANSFORM_TEX(v.uv,_BaseMap);o.local=v.uv;return o;}
   half4 Frag(V i):SV_Target{
    half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*_BaseColor;clip(c.a-_Cutoff);
    float3 normal=TransformObjectToWorldNormal(CascadeVolumeNormal(i.local,_Relief));
    c.rgb=CascadeIllustratedLight(c.rgb,normal,_Emission);return c;
   }
   ENDHLSL
  }
 }
}
