using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    // A single celestial key, sky irradiance and reflected environment. Never puzzle state.
    public static class WorldLighting
    {
        public readonly struct Sample
        {
            public readonly Vector3 Direction;
            public readonly Color Key, Sky, Horizon, Ground;
            public readonly float Intensity, Transmission, Shadow;
            public Sample(Vector3 direction, Color key, Color sky, Color horizon, Color ground,
                float intensity, float transmission, float shadow)
            { Direction=direction.normalized;Key=key;Sky=sky;Horizon=horizon;Ground=ground;
                Intensity=intensity;Transmission=transmission;Shadow=shadow; }
            public Vector3 ShadowOffset(float lift)
                => -Direction * (Mathf.Max(0,lift)/Mathf.Max(.25f,Direction.y));
            public Color Ambient(Vector3 normal)
                => Color.Lerp(Horizon,normal.y>=0?Sky:Ground,Mathf.Abs(normal.normalized.y));
        }
        readonly struct Profile
        {
            public readonly Vector3 Direction;
            public readonly Color Key, Sky, Horizon, Ground;
            public readonly float Intensity, Cover, Shadow;
            public Profile(Vector3 direction,Color key,Color sky,Color horizon,Color ground,float intensity,float cover,float shadow)
            { Direction=direction;Key=key;Sky=sky;Horizon=horizon;Ground=ground;Intensity=intensity;Cover=cover;Shadow=shadow; }
        }
        // Irradiance/radiance colours below are linear. Unity Light.color takes sRGB.
        static readonly Profile[] profiles={
            new Profile(new Vector3(-.43f,.64f,-.64f),new Color(1,.95f,.82f),new Color(.29f,.40f,.51f),new Color(.22f,.26f,.29f),new Color(.11f,.12f,.08f),1.52f,.10f,.76f),
            new Profile(new Vector3(.25f,.72f,-.65f),new Color(.88f,.95f,1),new Color(.26f,.35f,.42f),new Color(.22f,.26f,.29f),new Color(.09f,.12f,.13f),1.26f,.18f,.60f),
            new Profile(new Vector3(-.58f,.43f,-.69f),new Color(1,.79f,.56f),new Color(.28f,.31f,.34f),new Color(.26f,.25f,.22f),new Color(.17f,.10f,.055f),1.62f,.07f,.78f),
            new Profile(new Vector3(.36f,.69f,-.63f),new Color(1,.98f,.92f),new Color(.34f,.44f,.54f),new Color(.25f,.29f,.33f),new Color(.14f,.15f,.16f),1.44f,.12f,.70f),
            new Profile(new Vector3(-.32f,.65f,-.69f),new Color(.91f,1,.97f),new Color(.25f,.39f,.45f),new Color(.21f,.29f,.31f),new Color(.075f,.17f,.19f),1.43f,.12f,.70f),
            new Profile(new Vector3(.43f,.59f,-.68f),new Color(.96f,1,.85f),new Color(.25f,.35f,.28f),new Color(.21f,.26f,.21f),new Color(.09f,.13f,.06f),1.43f,.17f,.78f),
            new Profile(new Vector3(-.44f,.64f,-.63f),new Color(.92f,.96f,1),new Color(.32f,.43f,.56f),new Color(.27f,.32f,.39f),new Color(.21f,.27f,.34f),1.46f,.10f,.72f),
            new Profile(new Vector3(.58f,.40f,-.71f),new Color(1,.75f,.43f),new Color(.28f,.31f,.39f),new Color(.27f,.23f,.21f),new Color(.18f,.10f,.055f),1.74f,.06f,.80f),
            new Profile(new Vector3(-.39f,.58f,-.72f),new Color(.54f,.72f,1),new Color(.08f,.16f,.31f),new Color(.065f,.11f,.22f),new Color(.02f,.045f,.09f),1.75f,.13f,.83f),
            new Profile(new Vector3(-.46f,.58f,-.67f),new Color(1,.88f,.69f),new Color(.24f,.32f,.38f),new Color(.25f,.23f,.19f),new Color(.14f,.085f,.04f),1.44f,.035f,.74f)
        };
        public static Sample Evaluate(int world,bool workshop,float phase,bool moving=true)
        {
            var p=profiles[workshop?9:Mathf.Clamp(world,0,8)];
            if(float.IsNaN(phase)||float.IsInfinity(phase))phase=0;
            // Astronomical sources do not wobble. Passing high cloud only attenuates light.
            float cloud=moving?Mathf.SmoothStep(0,1,.5f+.5f*Mathf.Sin(phase*.105f+world*.71f)):0;
            float transmission=1-p.Cover*cloud;
            return new Sample(p.Direction,p.Key,p.Sky,p.Horizon,p.Ground,p.Intensity,transmission,p.Shadow);
        }
        public sealed class Rig : IDisposable
        {
            static readonly int DirectionId=Shader.PropertyToID("_CascadeKeyDirection"), KeyId=Shader.PropertyToID("_CascadeKeyRadiance"),
                SkyId=Shader.PropertyToID("_CascadeSkyIrradiance"), HorizonId=Shader.PropertyToID("_CascadeHorizonIrradiance"),
                GroundId=Shader.PropertyToID("_CascadeGroundIrradiance"), ActiveId=Shader.PropertyToID("_CascadeLightActive");
            readonly Light light;
            readonly Cubemap[] reflections=new Cubemap[10];
            readonly AmbientMode oldAmbientMode;
            readonly Color oldSky,oldEquator,oldGround;
            readonly SphericalHarmonicsL2 oldProbe;
            readonly DefaultReflectionMode oldReflectionMode;
            readonly Texture oldReflection;
            readonly float oldReflectionIntensity;
            readonly Light oldSun;
            int context=-1;
            bool disposed;
            public Sample Current {get;private set;}
            public Rig(Light source)
            {
                light=source;oldAmbientMode=RenderSettings.ambientMode;oldSky=RenderSettings.ambientSkyColor;
                oldEquator=RenderSettings.ambientEquatorColor;oldGround=RenderSettings.ambientGroundColor;
                oldProbe=RenderSettings.ambientProbe;oldReflectionMode=RenderSettings.defaultReflectionMode;
                oldReflection=RenderSettings.customReflectionTexture;oldReflectionIntensity=RenderSettings.reflectionIntensity;
                oldSun=RenderSettings.sun;
            }
            public void Paint(int world,bool workshop,float phase,bool moving)
            {
                if(disposed||light==null)return;
                Current=Evaluate(world,workshop,phase,moving);
                int next=workshop?9:Mathf.Clamp(world,0,8);
                if(context!=next){
                    context=next;light.color=Current.Key.gamma;light.transform.rotation=Quaternion.LookRotation(-Current.Direction);
                    light.shadowStrength=Current.Shadow;light.shadowBias=.008f;light.shadowNormalBias=.025f;
                    RenderSettings.sun=light;RenderSettings.ambientMode=AmbientMode.Trilight;
                    RenderSettings.ambientSkyColor=Current.Sky.gamma;RenderSettings.ambientEquatorColor=Current.Horizon.gamma;
                    RenderSettings.ambientGroundColor=Current.Ground.gamma;
                    var probe=new SphericalHarmonicsL2();
                    probe.AddAmbientLight(Current.Horizon*.72f);
                    probe.AddDirectionalLight(Vector3.up,Current.Sky,.50f);
                    probe.AddDirectionalLight(Vector3.down,Current.Ground,.40f);
                    probe.AddDirectionalLight(Vector3.back,Current.Horizon,.34f);
                    RenderSettings.ambientProbe=probe;
                    if(reflections[next]==null)reflections[next]=MakeReflection(Current,next);
                    RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;
                    RenderSettings.customReflectionTexture=reflections[next];RenderSettings.reflectionIntensity=.80f;
                    Shader.SetGlobalColor(SkyId,Current.Sky);Shader.SetGlobalColor(HorizonId,Current.Horizon);
                    Shader.SetGlobalColor(GroundId,Current.Ground);
                    Shader.SetGlobalVector(DirectionId,Current.Direction);Shader.SetGlobalFloat(ActiveId,1);
                }
                light.intensity=Current.Intensity*Current.Transmission;
                RenderSettings.reflectionIntensity=.80f*Current.Transmission;
                Shader.SetGlobalColor(KeyId,Current.Key*light.intensity);
            }
            static Cubemap MakeReflection(Sample p,int index)
            {
                const int size=64;
                var cube=new Cubemap(size,TextureFormat.RGBAHalf,true){name="CASCADE sky reflection "+index,filterMode=FilterMode.Trilinear,wrapMode=TextureWrapMode.Clamp};
                var pixels=new Color[size*size];
                for(int face=0;face<6;face++){
                    for(int y=0;y<size;y++)for(int x=0;x<size;x++){
                        float u=(x+.5f)/size*2-1,v=(y+.5f)/size*2-1;
                        Vector3 n=face==0?new Vector3(1,-v,-u):face==1?new Vector3(-1,-v,u):face==2?new Vector3(u,1,v):
                            face==3?new Vector3(u,-1,-v):face==4?new Vector3(u,-v,1):new Vector3(-u,-v,-1);
                        n.Normalize();var c=p.Ambient(n)*.68f;
                        // Finite source plus atmospheric aureole. No runtime reflection camera.
                        float toward=Mathf.Max(0,Vector3.Dot(n,p.Direction));
                        c+=p.Key*p.Intensity*(Mathf.Pow(toward,850)*1.4f+Mathf.Pow(toward,36)*.12f);
                        c.a=1;pixels[y*size+x]=c;
                    }
                    cube.SetPixels(pixels,(CubemapFace)face);
                }
                cube.Apply(true,true);return cube;
            }
            public void Dispose()
            {
                if(disposed)return;disposed=true;
                RenderSettings.ambientMode=oldAmbientMode;RenderSettings.ambientSkyColor=oldSky;
                RenderSettings.ambientEquatorColor=oldEquator;RenderSettings.ambientGroundColor=oldGround;RenderSettings.ambientProbe=oldProbe;
                RenderSettings.defaultReflectionMode=oldReflectionMode;RenderSettings.customReflectionTexture=oldReflection;
                RenderSettings.reflectionIntensity=oldReflectionIntensity;Shader.SetGlobalFloat(ActiveId,0);
                RenderSettings.sun=oldSun;
                foreach(var cube in reflections)if(cube!=null){
                    if(UnityEngine.Application.isPlaying)UnityEngine.Object.Destroy(cube);else UnityEngine.Object.DestroyImmediate(cube);
                }
            }
        }
    }
}
