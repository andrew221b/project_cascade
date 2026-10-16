Shader "Cascade/Tutorial/BackdropBlur" {
 Properties { _MainTex("Source",2D)="white" {} _Step("Sample step",Vector)=(0.001,0,0,0) }
 SubShader { Tags { "RenderType"="Opaque" } Cull Off ZWrite Off ZTest Always
  Pass { CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _Step;
   fixed4 frag(v2f_img i):SV_Target {
    fixed4 c=tex2D(_MainTex,i.uv)*.227027;
    c+=(tex2D(_MainTex,i.uv+_Step.xy*1.384615)+tex2D(_MainTex,i.uv-_Step.xy*1.384615))*.316216;
    c+=(tex2D(_MainTex,i.uv+_Step.xy*3.230769)+tex2D(_MainTex,i.uv-_Step.xy*3.230769))*.070270;
    return c;
   }
   ENDCG }
 }
}
