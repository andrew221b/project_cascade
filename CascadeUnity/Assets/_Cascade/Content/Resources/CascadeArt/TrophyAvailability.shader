Shader "Cascade/TrophyAvailability" {
 Properties {
  [PerRendererData] _MainTex ("Sprite",2D)="white" {}
  _Color ("Tint",Color)=(1,1,1,1)
  _Opacity ("Opacity",Range(0,1))=.84
  _StencilComp ("Stencil Comparison",Float)=8
  _Stencil ("Stencil ID",Float)=0
  _StencilOp ("Stencil Operation",Float)=0
  _StencilWriteMask ("Stencil Write Mask",Float)=255
  _StencilReadMask ("Stencil Read Mask",Float)=255
  _ColorMask ("Color Mask",Float)=15
 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True"}
  Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMask [_ColorMask]
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct input {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   struct output {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;};
   sampler2D _MainTex;fixed4 _Color;fixed _Opacity;
   output vert(input v){output o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color*_Color;o.uv=v.uv;return o;}
   fixed4 frag(output i):SV_Target {fixed4 c=tex2D(_MainTex,i.uv)*i.color;fixed gray=dot(c.rgb,fixed3(.299,.587,.114));c.rgb=lerp(gray.xxx,fixed3(.69,.70,.68),.16);c.a*=_Opacity;return c;}
   ENDCG }
 }
}
