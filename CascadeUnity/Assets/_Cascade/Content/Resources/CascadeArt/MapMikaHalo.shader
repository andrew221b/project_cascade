Shader "Cascade/Map Mika Silhouette Halo"
{
    Properties
    {
        [PerRendererData] _MainTex ("Original sprite", 2D) = "white" {}
        _Color ("Halo tint", Color) = (1, .94, .66, 1)
        _SpriteUVRect ("Sprite UV bounds", Vector) = (0,0,1,1)
        _GlowStep ("Contour spread", Vector) = (.01,.01,0,0)
        _AlphaFloor ("Physical silhouette", Range(0,1)) = .72
        _AlphaFeather ("Silhouette feather", Range(.001,.2)) = .09
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 local:TEXCOORD1; };
            sampler2D _MainTex;
            fixed4 _Color, _TextureSampleAdd;
            float4 _SpriteUVRect, _GlowStep, _ClipRect;
            float _AlphaFloor, _AlphaFeather;
            v2f vert(appdata v)
            {
                v2f o; o.local=v.vertex; o.vertex=UnityObjectToClipPos(v.vertex);
                o.uv=v.uv; o.color=v.color*_Color; return o;
            }
            half silhouette(float2 uv)
            {
                // Reject samples beyond this sprite so atlas neighbors cannot produce a box.
                half inside=step(_SpriteUVRect.x,uv.x)*step(uv.x,_SpriteUVRect.z)
                    *step(_SpriteUVRect.y,uv.y)*step(uv.y,_SpriteUVRect.w);
                half alpha=(tex2D(_MainTex,clamp(uv,_SpriteUVRect.xy,_SpriteUVRect.zw))+_TextureSampleAdd).a;
                return smoothstep(_AlphaFloor,_AlphaFloor+_AlphaFeather,alpha)*inside;
            }
            half ring(float2 uv,float2 spread)
            {
                half total=silhouette(uv+float2(spread.x,0))+silhouette(uv-float2(spread.x,0))
                    +silhouette(uv+float2(0,spread.y))+silhouette(uv-float2(0,spread.y));
                spread*=.70710678;
                total+=silhouette(uv+spread)+silhouette(uv-spread)
                    +silhouette(uv+float2(spread.x,-spread.y))+silhouette(uv+float2(-spread.x,spread.y));
                return total*.125;
            }
            fixed4 frag(v2f i):SV_Target
            {
                // Two soft rings follow the real alpha contour, leaving the character interior clear.
                half inner=ring(i.uv,_GlowStep.xy*.45), outer=ring(i.uv,_GlowStep.xy);
                half alpha=saturate((inner*.8+outer*.45)*(1-silhouette(i.uv)))*i.color.a;
                #ifdef UNITY_UI_CLIP_RECT
                alpha*=UnityGet2DClipping(i.local.xy,_ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha-.001);
                #endif
                return fixed4(i.color.rgb,alpha);
            }
            ENDCG
        }
    }
}
