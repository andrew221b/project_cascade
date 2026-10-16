using UnityEngine;
namespace Cascade.Presentation
{
    // Decorative controls share one restrained family; vegetation belongs to the scene.
    public static class CleanMenuArt
    {
        public const string Path="CascadeArt/CleanMenuOct14/";
        public static Sprite Get(string name)=>Resources.Load<Sprite>(Path+name);
        public static Sprite WorldTitle(int world)=>Get("WorldTitle"+Mathf.Clamp(world,0,8));
    }
}
