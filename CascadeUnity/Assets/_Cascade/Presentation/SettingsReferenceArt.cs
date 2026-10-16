using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Presentation
{
    // Original generated atlases remain intact; sprites select their authored silhouettes.
    public static class SettingsReferenceArt
    {
        public const string Path="CascadeArt/SettingsOct15/";
        static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        static readonly Rect[] icons={
            new Rect(67,72,240,199),new Rect(390,75,299,198),new Rect(793,50,235,237),new Rect(1166,50,215,239),
            new Rect(56,318,255,252),new Rect(444,336,233,232),new Rect(813,336,196,221),new Rect(1150,339,243,212),
            new Rect(75,620,192,195),new Rect(419,615,230,211),new Rect(765,631,275,177),new Rect(1155,607,225,225),
            new Rect(55,885,278,157),new Rect(411,883,284,159),new Rect(829,877,171,172),new Rect(1178,875,172,172)};
        public static Sprite Icon(int index)=>Region("Icon "+index,"Icons",icons[index]);
        public static Sprite Get(string name)
        {
            switch(name){
                case "Header":return Region(name,"Surfaces",new Rect(58,0,872,512));
                case "Panel":return Region(name,"Surfaces",new Rect(1004,47,472,448),new Vector4(100,100,100,100));
                case "Row":return Region(name,"Surfaces",new Rect(45,536,712,171),new Vector4(65,50,65,50));
                case "Selected":return Region(name,"Surfaces",new Rect(778,536,713,172),new Vector4(65,50,65,50));
                case "Restore":return Region(name,"Surfaces",new Rect(164,774,704,180),new Vector4(110,60,110,60));
                case "Cord":return Region(name,"Surfaces",new Rect(183,0,27,140));
                case "Frame":return Region(name,name,new Rect(0,0,941,1672),new Vector4(191,280,191,160));
                default:
                    var texture=Resources.Load<Texture2D>(Path+name);
                    return texture==null?null:Region(name,name,new Rect(0,0,texture.width,texture.height));
            }
        }
        static Sprite Region(string key,string asset,Rect topLeft,Vector4 border=default)
        {
            if(sprites.TryGetValue(key,out var cached)&&cached!=null)return cached;
            var texture=Resources.Load<Texture2D>(Path+asset);if(texture==null)return null;
            var rect=new Rect(topLeft.x,texture.height-topLeft.y-topLeft.height,topLeft.width,topLeft.height);
            var sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,border);
            sprite.name="Settings "+key;sprites[key]=sprite;return sprite;
        }
    }
}
