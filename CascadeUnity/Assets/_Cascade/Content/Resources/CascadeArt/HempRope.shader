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
 float phase=i.uv.x*76+across*3.2;
 float detail=1-saturate(fwidth(phase)*.6);
 float braid=.5+.5*cos(phase);
 float light=.48+.44*cylinder-.12*across+detail*(braid-.5)*.22;
 float edge=1-smoothstep(.78,1,abs(across));
 return fixed4(_Color.rgb*light,i.color.a*edge);
 }
 ENDCG }
 }
}
