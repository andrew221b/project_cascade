Shader "Cascade/ContourGlow"
{
    Properties { _Color("Contour",Color)=(1,.95,.65,.46) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Input {float4 positionOS:POSITION;float3 normalOS:NORMAL;};
            struct Varying {float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;float3 positionWS:TEXCOORD1;};
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END
            Varying Vert(Input v){Varying o;o.positionWS=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(v.normalOS);return o;}
            half4 Frag(Varying i):SV_Target{half rim=1-abs(dot(normalize(i.normalWS),GetWorldSpaceNormalizeViewDir(i.positionWS)));return half4(_Color.rgb,_Color.a*(.18+.82*rim));}
            ENDHLSL
        }
    }
}
