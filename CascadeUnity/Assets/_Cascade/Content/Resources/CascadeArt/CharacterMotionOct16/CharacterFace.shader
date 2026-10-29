Shader "Cascade/UI/CharacterFace" {
Properties {
 [PerRendererData] _MainTex("Original character",2D)="white"{} _PartMask("Original arm mask",2D)="black"{} _ContourMask("Original cutout contour",2D)="white"{} _CleanTex("Hidden fabric plate",2D)="white"{}
 _HalfBlinkTex("Shared half eyelids",2D)="white"{} _ClosedBlinkTex("Authored closed eyelids",2D)="white"{} _MouseBlink("Reuse Mika eyelids",Float)=0
 _NearLidUv("Near eyelid patch",Vector)=(0,0,1,1) _FarLidUv("Far eyelid patch",Vector)=(0,0,1,1) _LidScale("Patch sampling scale",Float)=1
 _SpeechValid("Complete source face expression",Float)=0 _SpeechRect("Source face crop",Vector)=(0,0,1,1) _SpeechMuzzle("Whole jaw region",Vector)=(0,0,0,0) _SpeechTop("Protect original eyes",Float)=0 _SpeechUv("Edited expression cell",Vector)=(0,0,1,1) _SpeechOpenTex("Exact face open",2D)="white"{} _SpeechHalfTex("Exact face half open",2D)="white"{}
 _Cell("Full art cell",Vector)=(0,0,1,1) _NearEye("Near eye",Vector)=(0,0,0,0) _FarEye("Far eye",Vector)=(0,0,0,0) _Mouth("Mouth",Vector)=(0,0,0,0)
 _Blink("Eyelids",Range(0,1))=0 _Gaze("Gaze",Vector)=(0,0,0,0) _Speaking("Articulation",Range(0,1))=0
 _UseAppleFrames("Authored whole bite",Float)=0 _AppleFrame("Bite frame",Float)=0 _AppleFrames("Bite atlas",2D)="white"{} _UseContour("Contour mask",Float)=0 _CutSky("Remove photo sky fringe",Float)=0
 _LumiEarTips("Complete original rabbit ear tips",2D)="white"{} _HaveLumiEarTips("Repair clipped rabbit ears",Float)=0
 _LightTint("World light",Color)=(1,1,1,1)
 _MicroMotion("Original whisker fan and expression",Vector)=(0,0,0,0)
 _World("Original world lighting",Float)=0
 _LanternMask("Original lantern mask",2D)="black"{} _UseLantern("Separate rigid lantern",Float)=0 _LanternLayer("Draw lantern only",Float)=0
 _StencilComp("Stencil Comparison",Float)=8 _Stencil("Stencil ID",Float)=0 _StencilOp("Stencil Operation",Float)=0 _StencilWriteMask("Stencil Write Mask",Float)=255 _StencilReadMask("Stencil Read Mask",Float)=255 _ColorMask("Color Mask",Float)=15
 [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip",Float)=0
}
SubShader {Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"}
Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
Pass {CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
#include "UnityCG.cginc"
#include "UnityUI.cginc"
struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
struct v2f {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;float4 local:TEXCOORD1;};
sampler2D _MainTex,_AppleFrames,_ContourMask,_HalfBlinkTex,_ClosedBlinkTex,_SpeechOpenTex,_SpeechHalfTex,_LanternMask,_LumiEarTips;float4 _ClipRect,_Cell,_NearEye,_FarEye,_Mouth,_Gaze,_NearLidUv,_FarLidUv,_SpeechRect,_SpeechMuzzle,_SpeechUv,_MicroMotion;fixed4 _TextureSampleAdd,_LightTint;
float _MouseBlink,_LidScale,_SpeechValid,_SpeechTop,_HaveLumiEarTips;
float _Blink,_Speaking,_UseAppleFrames,_AppleFrame,_UseContour,_CutSky,_UseLantern,_LanternLayer,_World;
v2f vert(appdata v){v2f o;o.local=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
float2 ArtUv(float2 p){return float2(_Cell.x+p.x*_Cell.z,1-_Cell.y-p.y*_Cell.w);}
float Oval(float2 p,float4 box){return box.z>0&&box.w>0?1-smoothstep(.85,1,length((p-box.xy)/box.zw)):0;}
fixed3 EyeLid(fixed3 original,float2 p,float4 eye,float far){
 float2 q=(p-eye.xy)/max(eye.zw,.0001);float weight=1-smoothstep(.86,1,length(q));
 if(weight<.001)return original;
 float2 uv=ArtUv(p);fixed3 eyelid;
 if(_MouseBlink>.5){
  float2 centre=lerp(float2(486,348),float2(680,291),far),radius=lerp(float2(71,83),float2(51,72),far);
  float2 source=centre+q*radius;uv=float2(source.x/1024,1-source.y/1536);
  eyelid=lerp(tex2D(_HalfBlinkTex,uv).rgb,tex2D(_ClosedBlinkTex,uv).rgb,saturate((_Blink-.5)*2));
 }else{float4 cell=lerp(_NearLidUv,_FarLidUv,far);uv=cell.xy+(.5+float2(q.x,-q.y)*.5*_LidScale)*cell.zw;eyelid=tex2D(_ClosedBlinkTex,uv).rgb;}
 return lerp(original,eyelid,weight*(_MouseBlink>.5?saturate(_Blink*2):_Blink));
}
fixed4 frag(v2f i):SV_Target{
 float2 p=float2((i.uv.x-_Cell.x)/_Cell.z,((1-i.uv.y)-_Cell.y)/_Cell.w);
 float eyeBand=max(_NearEye.y+_NearEye.w,_FarEye.y+_FarEye.w);
 float pupil=0;if(p.y<eyeBand&&p.y>min(_NearEye.y-_NearEye.w,_FarEye.y-_FarEye.w))pupil=max(Oval(p,float4(_NearEye.xy,_NearEye.zw*.58)),Oval(p,float4(_FarEye.xy,_FarEye.zw*.58)));
 // Fan the existing painted strands, keeping their roots and the rest of
 // the face fixed. One original texture sample; no duplicate whisker layer.
 float2 detailUv=i.uv;
 UNITY_BRANCH if(_MicroMotion.w>.5&&abs(p.y-_Mouth.y)<.10&&abs(p.x-_Mouth.x)<.31){
  float side=p.x-_Mouth.x;
  float fan=smoothstep(.045,.11,abs(side))*(1-smoothstep(.20,.31,abs(side)))*(1-smoothstep(.03,.10,abs(p.y-_Mouth.y)));
  detailUv.y+=_MicroMotion.x*side*.018*fan*_Cell.w;
 }
 fixed4 c=tex2D(_MainTex,detailUv+float2(-_Gaze.x*_Cell.z*.004,-_Gaze.y*_Cell.w*.003)*pupil*(1-_Blink))+_TextureSampleAdd;
 // Only the old atlas's clipped ear band is replaced. Original eyes,
 // muzzle, authored speech states and clothing keep their exact source pixels.
 UNITY_BRANCH if(_HaveLumiEarTips>.5&&p.y<.68){
  fixed4 tips=tex2D(_LumiEarTips,float2(p.x,1-p.y*(512.0/368.0)));
  float crown=1-smoothstep(.16,.21,p.y);
  float ear=(1-smoothstep(.40,.46,p.x))*(1-smoothstep(.63,.68,p.y));
  c=lerp(c,tips,max(crown,ear));
 }
 if(_UseAppleFrames>.5){
  float frame=clamp(floor(_AppleFrame+.5),0,11);
  // One complete authored pose per frame: no optical-flow stretching or double paws.
  float2 q=(p-float2(.015,.015))/float2(.95,.94);
  float2 grid=float2(4,3),tile=float2(fmod(frame,4),2-floor(frame/4));
  c=tex2D(_AppleFrames,(tile+float2(clamp(q.x,.001,.999),1-clamp(q.y,.001,.999)))/grid);
  if(_Blink>.001&&(frame<.5||frame>10.5)){
   // Reuse this character's own drawn closed eyes. A descending eyelid wipe
   // avoids the transparent double iris produced by a whole-eye dissolve.
   fixed3 closedEyes=tex2D(_AppleFrames,(float2(1,1)+float2(q.x-.004,1-(q.y-.016)))/grid).rgb;
   float nearMask=Oval(p,float4(_NearEye.xy,_NearEye.zw*1.35)),farMask=Oval(p,float4(_FarEye.xy,_FarEye.zw*1.35));
   float edge=lerp(-1.45,1.45,_Blink);
   float nearLid=(1-smoothstep(edge-.07,edge+.07,(p.y-_NearEye.y)/_NearEye.w))*nearMask;
   float farLid=(1-smoothstep(edge-.07,edge+.07,(p.y-_FarEye.y)/_FarEye.w))*farMask;
   c.rgb=lerp(c.rgb,closedEyes,max(nearLid,farLid));
  }
  c.a*=step(0,q.x)*step(q.x,1)*step(0,q.y)*step(q.y,1);
 }
 if(_UseLantern>.5){float mask=tex2D(_LanternMask,float2(p.x,1-p.y)).a;c.a*=_LanternLayer>.5?mask:1-mask;}
 if(_LanternLayer<.5){
  if(_UseAppleFrames<.5&&_Blink>.001&&p.y<eyeBand){c.rgb=EyeLid(c.rgb,p,_NearEye,0);c.rgb=EyeLid(c.rgb,p,_FarEye,1);}
  if(_UseAppleFrames<.5&&_SpeechValid>.5&&_Speaking>.001){
   float mask=Oval(p,_SpeechMuzzle)*smoothstep(_SpeechTop,_SpeechTop+.018,p.y);
   float2 q=(p-_SpeechRect.xy)/_SpeechRect.zw;
   mask*=step(0,q.x)*step(q.x,1)*step(0,q.y)*step(q.y,1);
   if(mask>.001){
    float amount=saturate(_Speaking);
    float2 stateUv=_SpeechUv.xy+float2(clamp(q.x,.003,.997),1-clamp(q.y,.003,.997))*_SpeechUv.zw;
    fixed4 halfOpen=tex2D(_SpeechHalfTex,stateUv),fullOpen=tex2D(_SpeechOpenTex,stateUv);
    float transition=step(.72,amount);
    float stateAlpha=lerp(halfOpen.a,fullOpen.a,transition);
    fixed4 drawn=fixed4(lerp(halfOpen.rgb*halfOpen.a,fullOpen.rgb*fullOpen.a,transition)/max(stateAlpha,.0001),stateAlpha);
    // Use exactly one complete authored expression per speech phase.
    // No dissolving tongues/lips or the original closed smile beneath a new mouth.
    float replace=mask*step(.16,amount);
    float alpha=lerp(c.a,drawn.a,replace);
    c=fixed4(lerp(c.rgb*c.a,drawn.rgb*drawn.a,replace)/max(alpha,.0001),alpha);
   }
  }
 }
 if(_UseContour>.5)c.a*=tex2D(_ContourMask,float2(p.x,1-p.y)).a;
 if(_CutSky>.5)c.a*=1-smoothstep(.03,.14,c.b-c.r)*smoothstep(.025,.12,c.b-c.g);
 // Match WorldSurface's existing world treatment, including the warm lamp
 // highlights. Animation must not dim the lanterns or recolor the home world.
 if(_World>.5){half warm=smoothstep(.10,.27,c.r-c.b)*smoothstep(.69,.95,max(c.r,c.g));fixed3 lit=c.rgb*lerp(fixed3(1,1,1),_LightTint.rgb,.9);c.rgb=lerp(lit,c.rgb,warm*.76);}
 c*=i.color;
#ifdef UNITY_UI_CLIP_RECT
 c.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
#endif
#ifdef UNITY_UI_ALPHACLIP
 clip(c.a-.001);
#endif
 return c;
}
ENDCG }
}
}
