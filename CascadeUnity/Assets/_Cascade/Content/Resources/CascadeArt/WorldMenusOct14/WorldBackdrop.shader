Shader "Cascade/UI/WorldBackdrop" {
Properties {
 [PerRendererData] _MainTex("Original frame",2D)="white"{} _SkyTex("World sky",2D)="white"{} _FrameTex("Frame alpha",2D)="white"{} _HaveFrame("Use authored matte",Float)=0 _World("World",Float)=0 _Mode("Window kind",Float)=1 _Age("Atmosphere time",Float)=0 _SkyScale("Cover",Vector)=(1,1,0,0) _LightTint("World light",Color)=(1,1,1,1)
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
sampler2D _MainTex,_SkyTex,_FrameTex;fixed4 _TextureSampleAdd,_LightTint;float4 _ClipRect,_SkyScale;float _World,_Mode,_HaveFrame,_Age;
v2f vert(appdata v){v2f o;o.local=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.screen=ComputeScreenPos(o.vertex);o.uv=v.uv;o.color=v.color;return o;}
fixed4 frag(v2f i):SV_Target {
 fixed4 original=(tex2D(_MainTex,i.uv)+_TextureSampleAdd)*i.color;
 // Even the first world replaces baked scenery with its own sky.
 {
  float2 screen=i.screen.xy/i.screen.w;float2 uv=.5+(screen-.5)*_SkyScale.xy;
  uv+=float2(sin(_Age*.045)*.007,sin(_Age*.032+1.1)*.004);
  fixed4 backdrop=tex2D(_SkyTex,saturate(uv));
  float keep=0;
  if(_Mode>1.5){
   float corner=smoothstep(.80,.87,i.uv.y)*(1-smoothstep(.22,.35,min(i.uv.x,1-i.uv.x)));
   float vegetation=smoothstep(.005,.065,original.g-original.r)*smoothstep(.035,.12,original.g-original.b);
   float wood=smoothstep(.015,.10,original.r-original.b)*smoothstep(.04,.18,original.r-original.g);
   keep=max(1-smoothstep(.088,.115,i.uv.y),corner*saturate(vegetation+wood));
  }else if(_Mode>.5){
   float edge=1-smoothstep(.055,.082,min(i.uv.x,1-i.uv.x));
   float vertical=max(1-smoothstep(.087,.12,i.uv.y),smoothstep(.91,.95,i.uv.y));
   float foliage=smoothstep(.006,.065,original.g-original.r)*smoothstep(.04,.12,original.g-original.b);
   float nearEdge=1-smoothstep(.085,.17,min(i.uv.x,1-i.uv.x));
   float skyColor=smoothstep(.01,.09,original.b-original.r);
   float beamColor=1-skyColor;
   keep=max(max(edge,vertical)*beamColor,foliage*nearEdge);
  }
  if(_HaveFrame>.5)keep=tex2D(_FrameTex,i.uv).a;
  original.rgb*=lerp(1,_LightTint.rgb,.90);
  original.rgb=lerp(backdrop.rgb*i.color.rgb,original.rgb,saturate(keep));
 }
#ifdef UNITY_UI_CLIP_RECT
 original.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
#endif
#ifdef UNITY_UI_ALPHACLIP
 clip(original.a-.001);
#endif
 return original;
}
ENDCG }
}
}
