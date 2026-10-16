Shader "Cascade/WorldIslandSurface" {
Properties {[MainTexture] _BaseMap("Authored island",2D)="white"{} [MainColor] _BaseColor("Color",Color)=(1,1,1,1) _SkyTex("World sky",2D)="white"{} _ImageRect("Sky viewport",Vector)=(0,0,1,1) _Haze("Distance",Float)=.16 }
SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
Pass {CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float4 screen:TEXCOORD1;};
sampler2D _BaseMap,_SkyTex;fixed4 _BaseColor;float4 _ImageRect;float _Haze;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.screen=ComputeScreenPos(o.pos);o.uv=v.uv;return o;}
fixed4 frag(v2f i):SV_Target{fixed4 c=tex2D(_BaseMap,i.uv)*_BaseColor;clip(c.a-.018);float2 screen=i.screen.xy/i.screen.w;float2 uv=(screen-_ImageRect.xy)/max(_ImageRect.zw,.001);fixed3 sky=tex2D(_SkyTex,saturate(uv)).rgb;c.rgb=lerp(c.rgb,sky,saturate(_Haze));return c;}
ENDCG }
}
}
