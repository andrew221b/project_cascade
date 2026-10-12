Shader "Cascade/ReceiverMagic"
{
 Properties {_FlowPhase("Colour flow",Float)=0}
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+5" "RenderType"="Transparent"}
  Pass {
   Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
    float _FlowPhase;
   CBUFFER_END
   struct A {float4 positionOS:POSITION;half4 color:COLOR;float2 uv:TEXCOORD0;};
   struct V {float4 positionCS:SV_POSITION;half4 color:COLOR;float2 uv:TEXCOORD0;};
   V Vert(A v){V o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.color=v.color;o.uv=v.uv;return o;}
   half4 Frag(V i):SV_Target{
    half edge=1-smoothstep(.45h,1.0h,abs(i.uv.x*2-1));
    half ends=smoothstep(0.0h,.08h,i.uv.y)*(1-smoothstep(.92h,1.0h,i.uv.y));
    float hue=i.uv.y*.9-_FlowPhase*.28;
    half3 rainbow=.28h+.72h*saturate(abs(frac(hue+float3(0,2.0/3.0,1.0/3.0))*6-3)-1);
    return half4(rainbow*i.color.rgb,i.color.a*edge*ends);
   }
   ENDHLSL
  }
 }
}
