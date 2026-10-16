Shader "CASCADE/IslandScenery"
{
 Properties { [MainTexture] _BaseMap("Island",2D)="white"{} _SkyTop("Upper sky",Color)=(.42,.77,.9,1) _SkyBottom("Lower sky",Color)=(1,.92,.72,1) _ImageRect("Image bounds",Vector)=(0,0,1,1) _MistColor("Morning air",Color)=(.98,.93,.84,1) _MistStrength("Mist strength",Range(0,.2))=.08 _MistPhase("Mist drift",Float)=0 _Workshop("Workshop light",Float)=0 _Night("Night world",Float)=0 _SkyOnly("Atmosphere without surface relief",Float)=0 }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
 Pass { Cull Off ZWrite On
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Assets/_Cascade/Presentation/IllustratedLighting.hlsl"
 struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;};struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 half4 _SkyTop,_SkyBottom,_MistColor;float4 _ImageRect;float _MistStrength,_MistPhase,_Workshop,_Night,_SkyOnly;
 CBUFFER_END
 V Vert(A v){V o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;return o;}
 half4 Frag(V i):SV_Target{
 float2 p=(i.uv-_ImageRect.xy)/_ImageRect.zw;
 half3 island=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,saturate(p)).rgb;
 float sky=smoothstep(.018,.09,island.b-island.r)*smoothstep(.022,.085,island.b-island.g);
 if(_Night>.5)sky=smoothstep(.008,.035,island.b-island.r)*smoothstep(.007,.028,island.b-island.g);
 if(_SkyOnly<.5){half3 surface=CascadeIllustratedLight(island,CascadeVolumeNormal(p,.50),max(_Night,_Workshop));
 island=lerp(surface,island,sky);}
 float horizon=smoothstep(.06,.22,i.uv.y)*(1-smoothstep(.37,.70,i.uv.y));
 float drift=.91+.09*sin(i.uv.x*8+_MistPhase*.08);
 float lantern=exp(-dot(p-float2(.055,.845),p-float2(.055,.845))*90);
 float flicker=.97+.025*sin(_MistPhase*2.4)*sin(_MistPhase*3.7+.7);
 island+=_Workshop*lantern*flicker*half3(.060,.027,.005);
 float2 moon=(p-float2(.155,.865))*float2(.58,1);
 island+=_Night*exp(-dot(moon,moon)*210)*half3(.006,.011,.020);
 return half4(lerp(island,_MistColor.rgb,horizon*drift*_MistStrength),1);
 }
 ENDHLSL
 }
 }
}
