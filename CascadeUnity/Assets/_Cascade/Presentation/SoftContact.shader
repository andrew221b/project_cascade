Shader "CASCADE/SoftContact"
{
    Properties { _BaseColor ("Shadow tint", Color) = (0.12,0.22,0.22,0.2) }
    SubShader {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            Varyings Vert(Attributes v) { Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.uv=v.uv; return o; }
            half4 Frag(Varyings i) : SV_Target {
                float2 p=(i.uv-.5)*2;
                float alpha=1-smoothstep(.25,1,length(p));
                return half4(_BaseColor.rgb,_BaseColor.a*alpha*alpha);
            }
            ENDHLSL
        }
    }
}
