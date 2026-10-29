Shader "Cascade/HempRope" {
 Properties { _BaseMap("Surface",2D)="white"{} _BaseColor("Hemp shadow",Color)=(1,1,1,1) _Color("Hemp",Color)=(0.78,0.59,0.33,1) _Cutoff("Cutout",Float)=0.12 _Authored("Authored braid",Float)=0 }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
 Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass {
 Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 struct appdata { float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;float3 normal:NORMAL;float4 tangent:TANGENT; };
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;float3 tangent:TEXCOORD3;};
 half4 _Color;float _Authored;TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 v2f vert(appdata v){v2f o;o.world=TransformObjectToWorld(v.vertex.xyz);o.vertex=TransformWorldToHClip(o.world);o.uv=v.uv;o.color=v.color;o.normal=TransformObjectToWorldNormal(v.normal);o.tangent=TransformObjectToWorldDir(v.tangent.xyz);return o;}
 half4 frag(v2f i):SV_Target {
 float across=i.uv.y*2-1;
 float cylinder=sqrt(saturate(1-across*across));
 float phase=i.uv.x*68+across*2.7;
 float aa=max(fwidth(phase),.001);
 float braid=.5+.5*cos(phase)*saturate(1.2-aa*.34);
 float strand=.5+.5*cos(across*8.4-phase*.33);
 half3 n=normalize(i.normal*cylinder+cross(i.normal,i.tangent)*across);
 Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
 half3 diffuse=SampleSH(n)+sun.color*saturate(dot(n,sun.direction))*sun.shadowAttenuation;
 half3 h=normalize(sun.direction+GetWorldSpaceNormalizeViewDir(i.world));
 half sheen=pow(saturate(dot(n,h)),28)*.09*sun.shadowAttenuation;
 half fiber=.78+(braid-.5)*.16+(strand-.5)*.08;
 float edge=1-smoothstep(.52,1,abs(across));
 half4 painted=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,float2(i.uv.y,frac(i.uv.x*1.1)));
 half3 hemp=lerp(_Color.rgb*fiber,painted.rgb,_Authored);float alpha=lerp(edge,painted.a,_Authored);clip(alpha-.06);
 return half4(hemp*diffuse+sun.color*sheen,i.color.a*alpha);
 }
 ENDHLSL }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 }
}
