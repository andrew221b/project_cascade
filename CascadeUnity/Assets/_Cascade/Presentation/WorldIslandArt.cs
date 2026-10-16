using System;
using UnityEngine;
namespace Cascade.Presentation
{
    // One authored set per world, shared by map landmarks, menus and camera-space scenery.
    public static class WorldIslandArt
    {
        public const string Path="CascadeArt/WorldIslandsOct14/";
        static readonly string[] themes={"WindGarden","RainMill","CopperOrchard","CloudPost","GlassLagoon","MossArchive","SnowSpire","GreatBeacon","StarObservatory"};
        public static string Theme(int world)=>themes[Mathf.Clamp(world,0,8)];
        public static Sprite Get(int world,int index)
        {
            var sprite=Resources.Load<Sprite>(Path+Theme(world)+(index%6));
            if(sprite==null)throw new InvalidOperationException("Missing authored island: "+Theme(world)+index);
            return sprite;
        }
        public static Texture2D Texture(int world,int index)=>Resources.Load<Texture2D>(Path+Theme(world)+(index%6));
        // Map bridge points are measured on the fitted sprite, not on its bounding rectangle.
        public static Vector2 Point(int world,int index,Vector2 rectangle,Vector2 normalizedFromTop)
        {
            var sprite=Get(world,index);float ratio=sprite.rect.width/sprite.rect.height;
            Vector2 fitted=rectangle.x/rectangle.y>ratio?new Vector2(rectangle.y*ratio,rectangle.y):new Vector2(rectangle.x,rectangle.x/ratio);
            return (rectangle-fitted)*.5f+Vector2.Scale(fitted,normalizedFromTop);
        }
        public static Vector2 Chimney(int world)=>new[]{new Vector2(.74f,.025f),new Vector2(.64f,.07f),new Vector2(.73f,.03f),new Vector2(.22f,.05f),new Vector2(.70f,.04f),new Vector2(.70f,.025f),new Vector2(.65f,.035f),new Vector2(.70f,.025f),new Vector2(.75f,.025f)}[Mathf.Clamp(world,0,8)];
    }
}
