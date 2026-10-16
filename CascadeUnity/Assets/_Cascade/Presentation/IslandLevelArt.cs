using UnityEngine;
namespace Cascade.Presentation
{
    public static class IslandLevelArt
    {
        public const string Path="CascadeArt/IslandLevelsOct16/";
        static readonly string[] worlds={"WindGarden","RainMill","CopperOrchard","CloudPost","GlassLagoon","MossArchive","SnowSpire","GreatBeacon","StarObservatory"};
        static Sprite row,progress;
        public static Sprite Background(int world)=>Resources.Load<Sprite>(Path+worlds[Mathf.Clamp(world,0,worlds.Length-1)]);
        public static Sprite Row=>row!=null?row:(row=Region("LevelRow",2048,768,new Rect(32,193,1984,425),Vector4.zero));
        public static Sprite Progress=>progress!=null?progress:(progress=Region("ProgressScroll",2206,713,new Rect(45,62,2115,601),new Vector4(245,170,245,170)));
        static Sprite Region(string name,float width,float height,Rect source,Vector4 border)
        {
            var texture=Resources.Load<Texture2D>(Path+name);if(texture==null)return null;
            float x=texture.width/width,y=texture.height/height;
            return Sprite.Create(texture,new Rect(source.x*x,source.y*y,source.width*x,source.height*y),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(border.x*x,border.y*y,border.z*x,border.w*y));
        }
    }
}
