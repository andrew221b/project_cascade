Shader "Cascade/NumberSymbolOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Symbol", 2D) = "white" {}
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 local:TEXCOORD1; };
            sampler2D _MainTex;
            float4 _ClipRect;
            v2f vert(appdata v) { v2f o; o.local=v.vertex; o.vertex=UnityObjectToClipPos(v.vertex); o.color=v.color; o.uv=v.uv; return o; }
            float silhouette(float2 uv) { return smoothstep(.12,.45,tex2D(_MainTex,uv).a); }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 source=tex2D(_MainTex,i.uv);
                float centre=smoothstep(.12,.45,source.a);
                // A thin rendered contour, independent of atlas resolution and UI
                // size. Dilate alpha only: transparent corners remain empty.
                float2 step=fwidth(i.uv)*.55;
                float edge=centre;
                edge=max(edge,silhouette(i.uv+float2(step.x,0)));
                edge=max(edge,silhouette(i.uv-float2(step.x,0)));
                edge=max(edge,silhouette(i.uv+float2(0,step.y)));
                edge=max(edge,silhouette(i.uv-float2(0,step.y)));
                edge=max(edge,silhouette(i.uv+step*.707));
                edge=max(edge,silhouette(i.uv-step*.707));
                edge=max(edge,silhouette(i.uv+float2(step.x,-step.y)*.707));
                edge=max(edge,silhouette(i.uv+float2(-step.x,step.y)*.707));
                fixed4 c=fixed4(lerp(fixed3(.025,.015,.008),source.rgb,centre/max(edge,.001)),edge)*i.color;
                #ifdef UNITY_UI_CLIP_RECT
                c.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
