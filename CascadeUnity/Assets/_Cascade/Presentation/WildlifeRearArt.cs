using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Presentation
{
    // Keep the seventeen published rear views; only the four missing species use the new atlas.
    public static class WildlifeRearArt
    {
        public const string ExistingPath="CascadeArt/Atmosphere/WildlifeRearOct10";
        public const string RegionalPath="CascadeArt/Atmosphere/RegionalRearOct15";
        static readonly Rect[] cells={
            new Rect(120,19,131,245),new Rect(409,22,140,242),new Rect(704,23,144,237),new Rect(992,36,139,219),
            new Rect(117,265,134,226),new Rect(387,264,187,236),new Rect(709,270,132,226),new Rect(1002,283,134,217),
            new Rect(141,512,89,233),new Rect(433,516,94,217),new Rect(710,519,131,211),new Rect(1015,500,107,247),
            new Rect(140,752,91,235),new Rect(400,767,157,191),new Rect(712,763,128,210),new Rect(993,761,147,208),
            new Rect(124,987,123,254),new Rect(184,55,287,548),new Rect(797,57,279,533),
            new Rect(187,687,308,463),new Rect(751,646,372,538)
        };
        static readonly Dictionary<WildlifeSpecies,Sprite> sprites=new Dictionary<WildlifeSpecies,Sprite>();
        public static bool Has(WildlifeSpecies species)=>(int)species>=0&&(int)species<cells.Length;
        public static Rect Pixels(WildlifeSpecies species)=>cells[(int)species];
        public static Rect TexturePixels(WildlifeSpecies species,Texture2D texture)
        {
            var p=Pixels(species);return new Rect(p.x*texture.width/1254f,p.y*texture.height/1254f,p.width*texture.width/1254f,p.height*texture.height/1254f);
        }
        public static bool IsRegional(WildlifeSpecies species)=>species>=WildlifeSpecies.Bullfinch;
        public static Sprite Get(WildlifeSpecies species)
        {
            if(!Has(species))return null;
            if(sprites.TryGetValue(species,out var value)&&value!=null)return value;
            var texture=Resources.Load<Texture2D>(IsRegional(species)?RegionalPath:ExistingPath);if(texture==null)return null;
            var p=TexturePixels(species,texture);return sprites[species]=Sprite.Create(texture,new Rect(p.x,texture.height-p.yMax,p.width,p.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
        }
    }
    public static class WildlifeFlightArt
    {
        public const string Path="CascadeArt/Atmosphere/RegionalFlightBodiesOct15";
        static readonly Rect[] cells={new Rect(37,219,938,338),new Rect(1009,221,942,345)};
        static readonly Sprite[] sprites=new Sprite[2];
        public static bool Has(WildlifeSpecies species)=>species==WildlifeSpecies.Bullfinch||species==WildlifeSpecies.BarnOwl;
        public static Rect TexturePixels(WildlifeSpecies species,Texture2D texture)
        {
            var p=cells[(int)species-(int)WildlifeSpecies.Bullfinch];return new Rect(p.x*texture.width/1983f,p.y*texture.height/793f,p.width*texture.width/1983f,p.height*texture.height/793f);
        }
        public static Sprite Get(WildlifeSpecies species)
        {
            if(!Has(species))return null;int index=(int)species-(int)WildlifeSpecies.Bullfinch;
            if(sprites[index]!=null)return sprites[index];var texture=Resources.Load<Texture2D>(Path);if(texture==null)return null;
            var p=TexturePixels(species,texture);return sprites[index]=Sprite.Create(texture,new Rect(p.x,texture.height-p.yMax,p.width,p.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
        }
    }
}
