#ifndef CASCADE_FRAME_RESKIN_INCLUDED
#define CASCADE_FRAME_RESKIN_INCLUDED
sampler2D _ThemeAccentTex;

float FrameFoliage(float3 c)
{
    return saturate(smoothstep(.82,1.13,c.g/max(c.r,.025))*smoothstep(.035,.16,c.g-c.b));
}

float3 FrameTint(float3 c,float3 tint,float strength)
{
    float light=dot(c,float3(.299,.587,.114));
    return lerp(c,tint*(.25+light*.95),strength);
}

float4 FrameAccent(float2 uv,float world,float age)
{
    float side=min(uv.x,1-uv.x);
    float upper=step(.83,uv.y)*step(side,.25);
    float lower=step(uv.y,.105)*step(side,.15);
    float2 local=float2(side/lerp(.15,.25,upper),lerp(uv.y/.105,(uv.y-.83)/.17,upper));
    // Every theme uses the same small exterior corner patch with its own clean silhouette.
    if(world!=6)local.x+=sin(age*.8+local.y*3)*.006;
    float index=clamp(round(world)-1,0,7);
    float2 cell=float2(fmod(index,4),1-floor(index/4));
    float4 accent=tex2D(_ThemeAccentTex,(cell+clamp(local,.015,.985))/float2(4,2));
    accent.a*=max(upper,lower)*smoothstep(0,.04,min(min(local.x,1-local.x),min(local.y,1-local.y)));
    return accent;
}

float4 MenuSideAccent(float2 uv,float world,float age)
{
    float side=min(uv.x,1-uv.x);
    float centre=.23;
    if(abs(uv.y-.44)<abs(uv.y-centre))centre=.44;
    if(abs(uv.y-.65)<abs(uv.y-centre))centre=.65;
    float2 local=float2((side-.005)/.115,(uv.y-centre)/.065+.5);
    if(world!=6)local.x+=sin(age*.8+centre*13)*.012;
    float inside=step(0,local.x)*step(local.x,1)*step(0,local.y)*step(local.y,1);
    float index=clamp(round(world)-1,0,7);
    float2 cell=float2(fmod(index,4),1-floor(index/4));
    float4 accent=tex2D(_ThemeAccentTex,(cell+clamp(local,.015,.985))/float2(4,2));
    accent.a*=inside*smoothstep(0,.04,min(min(local.x,1-local.x),min(local.y,1-local.y)));
    return accent;
}

float4 FrameOver(float4 base,float4 accent)
{
    float alpha=accent.a*.88,combined=alpha+base.a*(1-alpha);
    base.rgb=(accent.rgb*alpha+base.rgb*base.a*(1-alpha))/max(combined,.001);
    base.a=combined;return base;
}

float3 FrameMaterial(float3 c,float3 original,float world)
{
    if(world<.5)return c;
    float leaf=FrameFoliage(original);
    float wood=(1-leaf)*smoothstep(.045,.20,original.r-original.b);
    float whiteFlower=(1-leaf)*smoothstep(.71,.91,min(original.r,min(original.g,original.b)));
    if(world==1){c=FrameTint(c,float3(.31,.39,.31),wood*.68);c=FrameTint(c,float3(.23,.42,.20),leaf*.84);}
    else if(world==2){c=FrameTint(c,float3(.88,.31,.055),leaf*.96);c=FrameTint(c,float3(.95,.55,.22),wood*.19);}
    else if(world==3){c=FrameTint(c,float3(.65,.87,.83),leaf*.92);c=FrameTint(c,float3(.89,.75,.48),wood*.47);}
    else if(world==4){c=FrameTint(c,float3(.28,.69,.46),leaf*.67);c=FrameTint(c,float3(.55,.80,.76),wood*.57);c=FrameTint(c,float3(1,.45,.64),whiteFlower*.84);}
    else if(world==5){c=FrameTint(c,float3(.24,.42,.13),leaf*.80);c=FrameTint(c,float3(.37,.36,.20),wood*.51);c=FrameTint(c,float3(.72,.40,.84),whiteFlower*.79);}
    else if(world==6){float light=dot(original,float3(.299,.587,.114));c=lerp(c,float3(.77,.88,1)*(.53+light*.65),max(leaf*.99,wood*.87));}
    else if(world==7){c=FrameTint(c,float3(.99,.61,.16),leaf*.92);c=FrameTint(c,float3(.89,.56,.20),wood*.34);}
    else if(world==8){c=FrameTint(c,float3(.12,.25,.51),leaf*.95);c=FrameTint(c,float3(.14,.20,.37),wood*.85);c=FrameTint(c,float3(.70,.84,1),whiteFlower*.65);}
    return c;
}

float CleanFrameAlpha(float4 original,float2 uv,float enabled)
{
    // Remove white/blue sky accidentally retained inside the menu's timber cutout.
    // Gameplay and map frames keep their authored flowers and alpha unchanged.
    float side=min(uv.x,1-uv.x);
    float lamp=step(uv.x,.10)*step(.746,uv.y)*step(uv.y,.885);
    float innerEdge=step(side,.24)*step(.15,uv.y)*step(uv.y,.96)*(1-lamp);
    // Texture samples are linear: the pale sRGB sky remnants are much darker here.
    float pale=min(original.r,min(original.g,original.b));
    float whiteSky=smoothstep(.32,.55,pale)*smoothstep(.70,.80,pale/max(.001,max(original.r,max(original.g,original.b))));
    float blueSky=smoothstep(.005,.055,original.b-original.r)*smoothstep(.16,.40,original.b);
    float sideLeaves=step(side,.13)*step(.14,uv.y)*step(uv.y,.83)*smoothstep(.03,.18,FrameFoliage(original.rgb));
    // The source matte has a straight crop through its side foliage at x=53/680.
    // Remove that whole inner strip, including stems and warm cloud pixels, rather
    // than leaving disconnected pieces after colour-keying only the green leaves.
    // Complete thematic sprigs are composited separately over the original posts.
    float croppedInterior=smoothstep(.058,.062,side)*step(side,.24)*step(.15,uv.y)*step(uv.y,.83);
    return original.a*(1-saturate(max(max(max(whiteSky,blueSky)*innerEdge,sideLeaves),croppedInterior)*enabled));
}

float4 ReskinFrame(float4 c,float3 original,float2 uv,float world,float age)
{
    if(world<.5)return c;
    c.rgb=FrameMaterial(c.rgb,original,world);float leaf=FrameFoliage(original);
    float4 accent=FrameAccent(uv,world,age);
    c=FrameOver(c,accent);
    // Wet paving and icy highlights remain on the original stone faces.
    float floor=(1-smoothstep(.04,.13,uv.y))*(1-leaf);
    if(world==1)c.rgb+=floor*.027*(.5+.5*sin(uv.x*34+age*.65));
    if(world==6)c.rgb+=floor*.020*(.5+.5*sin(uv.x*41+age*.7));
    return c;
}
#endif
