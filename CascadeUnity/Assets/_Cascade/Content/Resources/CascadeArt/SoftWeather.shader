Shader "Cascade/SoftWeather" {
 Properties { _Color("Tint",Color)=(1,1,1,.5) }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
 fixed4 _Color;
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target {float r=length(i.uv*2-1);return fixed4(_Color.rgb,_Color.a*(1-smoothstep(.12,1,r)));}
 ENDCG }
 }
}
