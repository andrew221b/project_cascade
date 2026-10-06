Shader "Cascade/HempRope" {
 Properties { _MainTex("Surface",2D)="white"{} _Color("Hemp",Color)=(0.78,0.59,0.33,1) }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata { float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 fixed4 _Color;
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
 fixed4 frag(v2f i):SV_Target {
 float across=i.uv.y*2-1;
 float cylinder=sqrt(saturate(1-across*across));
 float phase=i.uv.x*68+across*2.7;
 float aa=max(fwidth(phase),.001);
 float braid=.5+.5*cos(phase)*saturate(1.2-aa*.34);
 float strand=.5+.5*cos(across*8.4-phase*.33);
 float light=.53+.37*cylinder-.08*across+(braid-.5)*.16+(strand-.5)*.08;
 float edge=1-smoothstep(.52,1,abs(across));
 return fixed4(_Color.rgb*light,i.color.a*edge);
 }
 ENDCG }
 }
}
