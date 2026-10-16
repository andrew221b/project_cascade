Shader "Cascade/SoftWeather" {
 Properties { _MainTex("Seasonal atlas",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;float2 kind:TEXCOORD1;fixed4 color:COLOR;};
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float2 kind:TEXCOORD1;fixed4 color:COLOR;};
 fixed4 _Color;sampler2D _MainTex;
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.kind=v.kind;o.color=v.color*_Color;return o;}
 fixed4 frag(v2f i):SV_Target {
   if(i.kind.x>.5)return tex2D(_MainTex,i.uv)*i.color;
   float2 p=i.uv*2-1;float r=length(p);float alpha=1-smoothstep(.10,1,r);
   if(i.kind.y>.5){float angle=atan2(p.y,p.x);float rays=pow(abs(cos(angle*3)),12);alpha=max(alpha*.25,rays*(1-smoothstep(.25,.95,r)));}
   return fixed4(i.color.rgb,i.color.a*alpha);
 }
 ENDCG }
 }
}
