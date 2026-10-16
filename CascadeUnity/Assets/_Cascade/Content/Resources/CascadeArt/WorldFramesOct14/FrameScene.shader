Shader "Cascade/SeasonalFrameScene" {
Properties {[MainTexture] _BaseMap("Frame",2D)="white"{} _ThemeAccentTex("Corner reskins",2D)="black"{} [MainColor] _BaseColor("Color",Color)=(1,1,1,1) _Age("Time",Float)=0 _World("World",Float)=0 _LightTint("Light",Color)=(1,1,1,1)}
SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
Pass {CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct a {float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};sampler2D _BaseMap;fixed4 _BaseColor,_LightTint;float4 _BaseMap_TexelSize;float _Age,_World;
#include "FrameReskin.cginc"
v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.uv=i.uv;return o;}
fixed4 frag(v i):SV_Target{
float side=min(i.uv.x,1-i.uv.x),right=step(.5,i.uv.x);
float corner=smoothstep(.76,.84,i.uv.y)*(1-smoothstep(.12,.34,side));
float angle=(sin(_Age*.83+right*1.9)*.032+sin(_Age*1.47+right*2.3)*.008)*corner;
float2 root=float2(lerp(.025,.975,right),1.015),delta=(i.uv-root)*_BaseMap_TexelSize.zw;
float sn=sin(angle),cs=cos(angle);
float2 sourceUv=root+float2(delta.x*cs-delta.y*sn,delta.x*sn+delta.y*cs)*_BaseMap_TexelSize.xy;
fixed4 first=tex2D(_BaseMap,sourceUv);
float plant=smoothstep(.04,.14,first.g-first.r)*smoothstep(.04,.14,first.g-first.b);
float pink=smoothstep(.1,.24,first.r-first.g)*smoothstep(.03,.12,first.b-first.g);
float move=_World==6?0:max(plant,pink);
float2 uv=sourceUv+float2(sin(_Age*.8+sourceUv.y*18)*2.4,sin(_Age*.53+sourceUv.x*14)*1.1)*_BaseMap_TexelSize.xy*move;
fixed4 c=tex2D(_BaseMap,uv);c.a=first.a;c=ReskinFrame(c,first.rgb,sourceUv,_World,_Age)*_BaseColor;
clip(c.a-.018);c.rgb*=lerp(1,_LightTint.rgb,.25);return c;}
ENDCG }
}}
