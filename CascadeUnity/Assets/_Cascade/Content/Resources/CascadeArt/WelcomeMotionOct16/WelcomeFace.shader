Shader "Cascade/UI/WelcomeFace" {
Properties {
 [PerRendererData] _MainTex("Original Mika",2D)="white"{} _HalfBlinkTex("Half eyelids",2D)="white"{} _ClosedBlinkTex("Closed eyelids",2D)="white"{}
 _Blink("Blink closure",Range(0,1))=0 _Gaze("Eye direction",Vector)=(0,0,0,0)
 _SeparateScarf("Separate loose scarf",Float)=0 _ScarfLayer("Draw loose scarf only",Float)=0
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
struct v2f {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;float4 local:TEXCOORD1;};
sampler2D _MainTex,_HalfBlinkTex,_ClosedBlinkTex;float4 _ClipRect,_Gaze;fixed4 _TextureSampleAdd;float _Blink,_SeparateScarf,_ScarfLayer;
v2f vert(appdata v){v2f o;o.local=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
float EyeMask(float2 p,float2 centre,float2 size){float d=length((p-centre)/size);return 1-smoothstep(.82,1,d);}
float LooseScarf(float2 p,float edge){
 float top=443+max(p.x-220,0)*.6+max(p.x-320,0)*.18-max(p.x-380,0)*1.08;
 float bottom=620+max(p.x-210,0)*.5-max(p.x-265,0)*.86-max(p.x-380,0)*.04;
 return step(95,p.x)*step(p.x,edge)*step(top,p.y)*step(p.y,bottom);
}
fixed4 frag(v2f i):SV_Target{
 float2 p=float2(i.uv.x*1024,(1-i.uv.y)*1536);
 float eye=0,pupil=0;
 // Most of this sprite is clothing or transparent sky. Avoid eye-mask math
 // there, and sample the two eyelid textures only during a visible blink.
 UNITY_BRANCH if(p.x>415&&p.x<738&&p.y>210&&p.y<432){
  eye=max(EyeMask(p,float2(486,348),float2(71,83)),EyeMask(p,float2(680,291),float2(51,72)));
  pupil=max(EyeMask(p,float2(489,355),float2(39,46)),EyeMask(p,float2(682,303),float2(27,37)));
 }
 float2 gazeUv=float2(-_Gaze.x*4/1024,-_Gaze.y*3/1536)*pupil*(1-_Blink);
 fixed4 original=tex2D(_MainTex,i.uv+gazeUv)+_TextureSampleAdd;
 UNITY_BRANCH if(_Blink>.0001){
  UNITY_BRANCH if(eye>.001){
   fixed4 halfBlink=tex2D(_HalfBlinkTex,i.uv),closed=tex2D(_ClosedBlinkTex,i.uv);
   fixed3 blink=lerp(halfBlink.rgb,closed.rgb,saturate((_Blink-.5)*2));
   original.rgb=lerp(original.rgb,blink,eye*saturate(_Blink*2));
  }
 }
 fixed4 c=original*i.color;
 UNITY_BRANCH if(_SeparateScarf>.5){
  if(_ScarfLayer>.5){c.a*=LooseScarf(p,418);c.a*=1-smoothstep(.72,.88,original.b/max(original.r,.001));}
  else c.a*=1-LooseScarf(p,398);
 }
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
