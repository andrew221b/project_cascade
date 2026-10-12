#ifndef CASCADE_ILLUSTRATED_LIGHTING
#define CASCADE_ILLUSTRATED_LIGHTING
float3 _CascadeKeyDirection;
half3 _CascadeKeyRadiance, _CascadeSkyIrradiance, _CascadeHorizonIrradiance, _CascadeGroundIrradiance;
float _CascadeLightActive;

// Bounded 2.5D surface relief; painted skies remain emitted radiance.
half3 CascadeIllustratedLight(half3 paint, float3 normal, float emission)
{
    normal=normalize(normal);
    half3 ambient=lerp(_CascadeHorizonIrradiance,normal.y>=0?_CascadeSkyIrradiance:_CascadeGroundIrradiance,abs(normal.y));
    half3 irradiance=ambient*2+_CascadeKeyRadiance*saturate(dot(normal,_CascadeKeyDirection))*.55;
    half3 lit=paint*clamp(irradiance,.32,1.48);
    // Existing bright windows/lanterns are emissive, not darkened by the moon.
    float warm=smoothstep(.025,.15,paint.r-paint.b)*smoothstep(.20,.46,max(paint.r,paint.g));
    lit=lerp(lit,paint,warm*emission);
    return lerp(paint,lit,saturate(_CascadeLightActive));
}
float3 CascadeVolumeNormal(float2 local, float depth)
{
    float2 slope=(local-.5)*depth;
    return normalize(float3(slope,-sqrt(max(.24,1-dot(slope,slope)))));
}
#endif
