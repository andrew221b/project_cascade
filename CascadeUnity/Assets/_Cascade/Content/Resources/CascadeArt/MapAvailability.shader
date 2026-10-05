Shader "Cascade/MapAvailability" {
 Properties { [PerRendererData] _MainTex ("Map",2D)="white"{} }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
 Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata { float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0; };
 struct v2f { float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0; };
 sampler2D _MainTex;float4 _Island0,_Island1,_Island2,_Island3,_Island4;float _Unlocked;
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target {
 fixed4 c=tex2D(_MainTex,i.uv)*i.color;
 float2 p=i.uv;float2 aspect=float2(1,1.6);
 float d=length((p-_Island0.xy)*aspect);int nearest=0;
 float d1=length((p-_Island1.xy)*aspect);if(d1<d){d=d1;nearest=1;}
 float d2=length((p-_Island2.xy)*aspect);if(d2<d){d=d2;nearest=2;}
 float d3=length((p-_Island3.xy)*aspect);if(d3<d){d=d3;nearest=3;}
 float d4=length((p-_Island4.xy)*aspect);if(d4<d){nearest=4;}
 // Keep the continuous sky blue; desaturate the illustrated island surfaces.
 float sky=smoothstep(.035,.14,c.b-c.r)*smoothstep(.05,.12,c.b-c.g)*smoothstep(.32,.60,c.b);
 if(nearest>_Unlocked)c.rgb=lerp(dot(c.rgb,float3(.299,.587,.114)).xxx,c.rgb,sky);
 return c;
 }
 ENDCG }
 }
}
