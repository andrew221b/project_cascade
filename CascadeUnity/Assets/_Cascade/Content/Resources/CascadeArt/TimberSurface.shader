Shader "Cascade/TimberSurface"
{
 Properties {
  [MainTexture] _BaseMap("Painted timber",2D)="white"{}
  [MainColor] _BaseColor("Tint",Color)=(1,1,1,1)
  _Cutoff("Transparent grain",Range(0,1))=.12
  _Smoothness("Polish",Range(0,1))=.32
  _Metallic("Metallic",Range(0,1))=0
  _SpecColor("Specular",Color)=(.2,.2,.2,1)
  _BumpScale("Normal strength",Float)=1
  _OcclusionStrength("Occlusion",Range(0,1))=1
  _EmissionColor("Emission",Color)=(0,0,0,1)
  _EmissionMap("Emission map",2D)="white"{}
  [HideInInspector] _Surface("Surface",Float)=0
  [HideInInspector] _AlphaClip("Clip",Float)=1
 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" "UniversalMaterialType"="Lit"}
  Pass {
   Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
   ZWrite On Cull Off
   HLSLPROGRAM
   #pragma target 2.0
   #pragma vertex LitPassVertex
   #pragma fragment LitPassFragment
   // Always clip, including player builds without a runtime Lit keyword variant.
   #define _ALPHATEST_ON 1
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
   ENDHLSL
  }
  Pass {
   Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"}
   ZWrite On ZTest LEqual ColorMask 0 Cull Off
   HLSLPROGRAM
   #pragma target 2.0
   #pragma vertex ShadowPassVertex
   #pragma fragment ShadowPassFragment
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   #define _ALPHATEST_ON 1
   #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
   ENDHLSL
  }
  Pass {
   Name "DepthOnly" Tags {"LightMode"="DepthOnly"}
   ZWrite On ColorMask R Cull Off
   HLSLPROGRAM
   #pragma target 2.0
   #pragma vertex DepthOnlyVertex
   #pragma fragment DepthOnlyFragment
   #define _ALPHATEST_ON 1
   #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
   ENDHLSL
  }
 }
}
