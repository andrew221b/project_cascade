Shader "Cascade/SeasonalFrameScene" {
Properties {[MainTexture] _BaseMap("Frame",2D)="white"{} _ThemeAccentTex("Corner reskins",2D)="black"{} [MainColor] _BaseColor("Color",Color)=(1,1,1,1) _Age("Time",Float)=0 _Wind("Foliage wind",Float)=0 _World("World",Float)=0 _LightTint("Light",Color)=(1,1,1,1)}
SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
Pass {CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct a {float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};sampler2D _BaseMap;fixed4 _BaseColor,_LightTint;float4 _BaseMap_TexelSize;float _Age,_Wind,_World;
#include "FrameReskin.cginc"
v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.uv=i.uv;return o;}
fixed4 frag(v i):SV_Target{
float side=min(i.uv.x,1-i.uv.x),right=step(.5,i.uv.x);
float corner=smoothstep(.76,.84,i.uv.y)*(1-smoothstep(.12,.34,side));
float angle=(_Wind*.075+sin(_Age*1.03+right*1.9)*.030+sin(_Age*.57+right*2.3)*.010)*corner;
float2 root=float2(lerp(.025,.975,right),1.015),delta=(i.uv-root)*_BaseMap_TexelSize.zw;
float sn=sin(angle),cs=cos(angle);
// Wind cannot move the underlying beam, rope joints or lantern.
// Classify in the unwarped artwork before tinting it for another world.
fixed4 anchored=tex2D(_BaseMap,i.uv);
float vegetation=FrameFoliage(anchored.rgb)*anchored.a;
float2 bentUv=root+float2(delta.x*cs-delta.y*sn,delta.x*sn+delta.y*cs)*_BaseMap_TexelSize.xy;
float2 sourceUv=lerp(i.uv,bentUv,vegetation);
fixed4 first=tex2D(_BaseMap,sourceUv);
float plant=smoothstep(.04,.14,first.g-first.r)*smoothstep(.04,.14,first.g-first.b);
float pink=smoothstep(.1,.24,first.r-first.g)*smoothstep(.03,.12,first.b-first.g);
float move=_World==6?0:max(plant,pink);
float2 uv=sourceUv+float2(-_Wind*4.8+sin(_Age*1.03+sourceUv.y*18)*3.4,sin(_Age*.71+sourceUv.x*14)*(1.3+abs(_Wind)*1.5))*_BaseMap_TexelSize.xy*move;
fixed4 c=tex2D(_BaseMap,uv);c.a=first.a;c=ReskinFrame(c,first.rgb,sourceUv,_World,_Age)*_BaseColor;
clip(c.a-.018);c.rgb*=lerp(1,_LightTint.rgb,.25);return c;}
ENDCG }
}}
