Shader "Cascade/UI/LeafDepthBlur"
{
    Properties {
        [PerRendererData] _MainTex("Sprite",2D)="white" {} _Color("Tint",Color)=(1,1,1,1)
        _Blur("Depth blur",Float)=0.05
        _StencilComp("Stencil Comparison",Float)=8 _Stencil("Stencil ID",Float)=0
        _StencilOp("Stencil Operation",Float)=0 _StencilWriteMask("Stencil Write Mask",Float)=255
        _StencilReadMask("Stencil Read Mask",Float)=255 _ColorMask("Color Mask",Float)=15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip",Float)=0
    }
    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="False"}
        Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
            struct v2f {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;float4 local:TEXCOORD1;};
            sampler2D _MainTex;float4 _Color,_ClipRect;float _Blur;
            v2f vert(appdata v){v2f o;o.local=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
            float4 Tap(float2 uv){float4 c=tex2D(_MainTex,uv);return float4(c.rgb*c.a,c.a);}
            fixed4 frag(v2f i):SV_Target {
                float4 c=0;
                [unroll] for(int y=-2;y<=2;y++) [unroll] for(int x=-2;x<=2;x++) {
                    float wx=x==0?6:(abs(x)==1?4:1),wy=y==0?6:(abs(y)==1?4:1);
                    c+=Tap(i.uv+float2(x,y)*_Blur*.5)*wx*wy;
                }
                c/=256;c.rgb/=max(c.a,.0001);c*=i.color;
                #ifdef UNITY_UI_CLIP_RECT
                c.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(c.a-.001);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
