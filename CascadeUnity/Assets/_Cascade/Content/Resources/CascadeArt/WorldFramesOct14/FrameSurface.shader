Shader "Cascade/UI/SeasonalFrame" {
Properties { [PerRendererData] _MainTex("Frame",2D)="white"{} _ThemeAccentTex("Corner reskins",2D)="black"{} _MenuPostTex("Continuous original-style side posts",2D)="black"{} _HaveMenuPosts("Use repaired menu posts",Float)=0 _World("World",Float)=0 _Age("Time",Float)=0 _MapCornerWind("Map corner wind",Float)=0 _CornerMotionAllowed("Corner movement enabled",Float)=1 _CleanCutout("Clean menu cutout",Float)=0 _Header("Preserve heading paper",Float)=0 _LightTint("Light",Color)=(1,1,1,1)
_ContinuousLanding("Continuous menu paving",Float)=0
_StencilComp("Stencil Comparison",Float)=8 _Stencil("Stencil ID",Float)=0 _StencilOp("Stencil Operation",Float)=0 _StencilWriteMask("Stencil Write Mask",Float)=255 _StencilReadMask("Stencil Read Mask",Float)=255 _ColorMask("Color Mask",Float)=15 }
SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]} Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
Pass {CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
#include "UnityCG.cginc"
#include "UnityUI.cginc"
struct a {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};struct v {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float4 local:TEXCOORD1;float4 color:COLOR;};
sampler2D _MainTex,_MenuPostTex;float4 _MainTex_TexelSize,_LightTint,_ClipRect;float _World,_Age,_CleanCutout,_Header,_HaveMenuPosts,_MapCornerWind,_CornerMotionAllowed,_ContinuousLanding;
#include "FrameReskin.cginc"
v vert(a x){v o;o.pos=UnityObjectToClipPos(x.vertex);o.local=x.vertex;o.uv=x.uv;o.color=x.color;return o;}
fixed4 frag(v i):SV_Target{
// Bend the complete attached sprig, including white flowers and its alpha edge.
// The roots stay fixed; timber, lamps, heading and the centre remain stationary.
float side=min(i.uv.x,1-i.uv.x),right=step(.5,i.uv.x);
float corner=smoothstep(.76,.84,i.uv.y)*(1-smoothstep(.12,.34,side))*_MapCornerWind*_CornerMotionAllowed;
float angle=(sin(_Age*.83+right*1.9)*.045+sin(_Age*1.47+right*2.3)*.012)*corner;
float2 root=float2(lerp(.025,.975,right),1.015),delta=(i.uv-root)*_MainTex_TexelSize.zw;
float sn=sin(angle),cs=cos(angle);
float2 sourceUv=root+float2(delta.x*cs-delta.y*sn,delta.x*sn+delta.y*cs)*_MainTex_TexelSize.xy;
fixed4 original=tex2D(_MainTex,sourceUv),posts=tex2D(_MenuPostTex,sourceUv);
// Repair only the cropped middle side posts. Original rope corners, lantern,
// crossbar, bottom supports and source layout remain separate and unchanged.
float lamp=step(i.uv.x,.10)*step(.746,i.uv.y)*step(i.uv.y,.885);
// Full-width menu terraces already own their paving. Reuse transparent posts
// down to their feet so the frame cannot overlay a mismatched strip of old floor.
float lowerRepair=lerp(smoothstep(.145,.18,i.uv.y),1,_ContinuousLanding);
float repair=_HaveMenuPosts*step(.5,_CleanCutout)*(1-step(.5,_Header))*lowerRepair*(1-smoothstep(.82,.855,i.uv.y))*(1-lamp);
fixed4 base=lerp(original,posts,repair);float plant=smoothstep(.04,.14,base.g-base.r)*smoothstep(.04,.14,base.g-base.b);float flower=smoothstep(.10,.24,base.r-base.g)*smoothstep(.03,.12,base.b-base.g);float movement=_World==6?0:max(plant,flower);
float2 uv=sourceUv+float2(sin(_Age*.8+sourceUv.y*18)*1.6,sin(_Age*.53+sourceUv.x*14)*.65)*_MainTex_TexelSize.xy*movement;
fixed4 c=lerp(tex2D(_MainTex,uv),tex2D(_MenuPostTex,uv),repair);c.a=lerp(CleanFrameAlpha(original,sourceUv,_CleanCutout),posts.a,repair);
if(_Header>.5){
 float paper=smoothstep(.30,.55,min(base.r,base.g))*smoothstep(.10,.27,base.b);
 c.rgb=lerp(FrameMaterial(c.rgb,base.rgb,_World),c.rgb,paper);
 c.rgb*=lerp(1,_LightTint.rgb,.20*(1-paper));
}else{
 c=ReskinFrame(c,base.rgb,sourceUv,_World,_Age);
 if(_CleanCutout>.5&&_World>.5)c=FrameOver(c,MenuSideAccent(i.uv,_World,_Age));
 c.rgb*=lerp(1,_LightTint.rgb,.25);
}
c*=i.color;clip(c.a-.018);
if(_World==4||_World==6){float bright=saturate((c.b-c.r)*4);c.rgb+=bright*.025*(.5+.5*sin(_Age*1.2+i.uv.y*23));}
#ifdef UNITY_UI_CLIP_RECT
c.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
#endif
return c;}
ENDCG }
}}
