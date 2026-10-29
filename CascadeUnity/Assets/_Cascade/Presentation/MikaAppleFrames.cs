using UnityEngine;
namespace Cascade.Presentation
{
    // Complete authored drawings; the held final frame keeps the actual bite.
    public static class MikaAppleFrames
    {
        public const int Count=12;
        public static float Frame(float age)=>Mathf.Clamp((age-1.8f)*10,0,Count-1);
    }
}
