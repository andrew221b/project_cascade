using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Presentation
{
    public static class AccountReferenceArt
    {
        public const string Path="CascadeArt/AccountOct15/";
        static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        public static Sprite Get(string name)
        {
            if(sprites.TryGetValue(name,out var existing)&&existing!=null)return existing;
            string asset="Controls";Rect region;Vector4 border=default;
            switch(name){
                case "WelcomeMika":asset=name;region=new Rect(31,39,957,1440);break;
                case "HomeMika":asset=name;region=new Rect(21,16,954,1464);break;
                case "Airship":asset=name;region=new Rect(189,62,1056,967);break;
                case "Header":region=new Rect(8,68,609,270);border=new Vector4(180,90,180,90);break;
                case "Green":region=new Rect(623,102,536,227);border=new Vector4(125,80,125,80);break;
                case "Gold":region=new Rect(62,441,529,182);border=new Vector4(95,65,95,65);break;
                case "SignIn":region=new Rect(740,403,264,257);break;
                case "SignOut":region=new Rect(187,716,277,265);break;
                case "Apple":region=new Rect(750,729,223,267);break;
                case "Google":region=new Rect(196,1045,251,251);break;
                case "Guest":region=new Rect(773,1064,198,225);break;
                default:
                    asset=name;var whole=Resources.Load<Texture2D>(Path+asset);if(whole==null)return null;
                    region=new Rect(0,0,whole.width,whole.height);break;
            }
            var texture=Resources.Load<Texture2D>(Path+asset);if(texture==null)return null;
            var rect=new Rect(region.x,texture.height-region.y-region.height,region.width,region.height);
            var sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,border);
            sprite.name="Account "+name;sprites[name]=sprite;return sprite;
        }
    }
}
