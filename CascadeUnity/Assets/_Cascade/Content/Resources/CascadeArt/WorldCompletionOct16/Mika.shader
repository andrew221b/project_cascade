Shader "Cascade/UI/WholeMikaCelebration" {
Properties {
 [PerRendererData] _MainTex("Whole pose atlas",2D)="white"{} _FlowTex("Offline bidirectional transitions",2D)="gray"{}
 _Frame("Authored frame",Float)=0 _Ears("Connected ear motion",Vector)=(0,0,0,0) _FrontPaw("Board contact layer",Float)=0 _LightTint("Foreground light",Color)=(1,1,1,1)
 _StencilComp("Stencil Comparison",Float)=8 _Stencil("Stencil ID",Float)=0 _StencilOp("Stencil Operation",Float)=0 _StencilWriteMask("Stencil Write Mask",Float)=255 _StencilReadMask("Stencil Read Mask",Float)=255 _ColorMask("Color Mask",Float)=15
}
SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]} Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
Pass {CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
#include "UnityCG.cginc"
#include "UnityUI.cginc"
struct a {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
struct v {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float4 local:TEXCOORD1;fixed4 color:COLOR;};
sampler2D _MainTex,_FlowTex;float4 _MainTex_TexelSize,_Ears,_ClipRect;fixed4 _LightTint;float _Frame,_FrontPaw;
v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.local=i.vertex;o.uv=i.uv;o.color=i.color;return o;}
float2 Ear(float2 p,float2 centre,float2 radius,float2 root,float angle){
 float2 d=(p-centre)/radius;float weight=(1-smoothstep(.36,1,dot(d,d)))*(1-smoothstep(root.y-.04,root.y+.01,p.y));
 float2 q=p-root;float s=sin(angle),c=cos(angle);return lerp(p,root+float2(q.x*c-q.y*s,q.x*s+q.y*c),weight);
}
fixed4 frag(v i):SV_Target{
 float2 p=float2(i.uv.x,1-i.uv.y);
 if(_FrontPaw>.5)clip(min(p.x-.76,p.y-.80));
 p=Ear(p,float2(.20,.28),float2(.18,.20),float2(.31,.39),_Ears.x);
 p=Ear(p,float2(.74,.14),float2(.17,.20),float2(.66,.29),_Ears.y);
 float frame=min(11,floor(_Frame)),next=min(11,frame+1),blend=frac(_Frame);float2 grid=float2(4,3);
 float2 tile=float2(fmod(frame,4),2-floor(frame/4)),nextTile=float2(fmod(next,4),2-floor(next/4));
 float4 flow=(tex2D(_FlowTex,(tile+float2(p.x,1-p.y))/grid)*255-128)/510;
 float2 q=p-float2(flow.x,-flow.y)*blend,z=p-float2(flow.z,-flow.w)*(1-blend);
 float2 gutter=_MainTex_TexelSize.xy*grid*.55;
 fixed4 first=tex2D(_MainTex,(tile+clamp(float2(q.x,1-q.y),gutter,1-gutter))/grid);
 fixed4 second=tex2D(_MainTex,(nextTile+clamp(float2(z.x,1-z.y),gutter,1-gutter))/grid);
 float alpha=lerp(first.a,second.a,blend);fixed4 c=fixed4(lerp(first.rgb*first.a,second.rgb*second.a,blend)/max(.0001,alpha),alpha);
 c.rgb*=_LightTint.rgb;c*=i.color;
#ifdef UNITY_UI_CLIP_RECT
 c.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
#endif
 return c;
}
ENDCG}
}}
