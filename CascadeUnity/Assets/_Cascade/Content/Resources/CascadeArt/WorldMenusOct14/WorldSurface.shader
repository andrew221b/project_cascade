Shader "Cascade/UI/WorldSurface" {
Properties {
 [PerRendererData] _MainTex("Art",2D)="white"{} _MaskTex("Authored foreground alpha",2D)="white"{} _UseMask("Use foreground mask",Float)=0 _World("World",Float)=0 _AuthoredIsland("Authored world island",Float)=0 _Haze("Distance haze",Float)=0 _SkyTex("World sky",2D)="white"{} _SkyScale("Sky cover",Vector)=(1,1,0,0) _Landscape("Biome foliage",Float)=0 _Locked("Locked",Float)=0 _Age("Decorative time",Float)=0 _CutSky("Clean frame sky remnants",Float)=0 _LightTint("World light",Color)=(1,1,1,1)
 _StencilComp("Stencil Comparison",Float)=8 _Stencil("Stencil ID",Float)=0 _StencilOp("Stencil Operation",Float)=0 _StencilWriteMask("Stencil Write Mask",Float)=255 _StencilReadMask("Stencil Read Mask",Float)=255 _ColorMask("Color Mask",Float)=15
 [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip",Float)=0
}
SubShader {Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"}
Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
Pass {CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
#include "UnityCG.cginc"
#include "UnityUI.cginc"
struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
struct v2f {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;float4 local:TEXCOORD1;float4 screen:TEXCOORD2;};
sampler2D _MainTex,_MaskTex,_SkyTex;fixed4 _TextureSampleAdd,_LightTint;float4 _ClipRect,_SkyScale;float _World,_Landscape,_Locked,_Age,_CutSky,_UseMask,_AuthoredIsland,_Haze;
#include "../WorldFramesOct14/FrameReskin.cginc"
v2f vert(appdata v){v2f o;o.local=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;o.screen=ComputeScreenPos(o.vertex);return o;}
fixed4 frag(v2f i):SV_Target{
 fixed4 c=(tex2D(_MainTex,i.uv)+_TextureSampleAdd)*i.color;
 if(_CutSky>.5){float blue=smoothstep(.03,.14,c.b-c.r)*smoothstep(.025,.12,c.b-c.g);c.a*=1-blue;}
 if(_UseMask>.5)c.a*=tex2D(_MaskTex,i.uv).a;
 if(_Landscape>.5){
  c.rgb=FrameMaterial(c.rgb,c.rgb,_World);
 }
 if(_World>.5&&_AuthoredIsland<.5){
  half warm=smoothstep(.10,.27,c.r-c.b)*smoothstep(.69,.95,max(c.r,c.g));
  half3 lit=c.rgb*lerp(1,_LightTint.rgb,.9);
  c.rgb=lerp(lit,c.rgb,warm*.76);
 }
 if(_AuthoredIsland>.5||_Haze>.001){clip(c.a-.018);float2 uv=.5+(i.screen.xy/i.screen.w-.5)*_SkyScale.xy;uv+=float2(sin(_Age*.045)*.007,sin(_Age*.032+1.1)*.004);c.rgb=lerp(c.rgb,tex2D(_SkyTex,saturate(uv)).rgb,saturate(_Haze));}
 if(_Locked>.5)c.rgb=dot(c.rgb,half3(.299,.587,.114)).xxx;
#ifdef UNITY_UI_CLIP_RECT
 c.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
#endif
#ifdef UNITY_UI_ALPHACLIP
 clip(c.a-.001);
#endif
 return c;
}
ENDCG }
}
}
