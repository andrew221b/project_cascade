using UnityEngine;
namespace Cascade.Presentation
{
    public static class GameplayReferenceArt
    {
        public const string Path="CascadeArt/GameplayReferenceOct14/";
        static readonly System.Collections.Generic.Dictionary<string,Sprite> sprites=new System.Collections.Generic.Dictionary<string,Sprite>();
        static readonly System.Collections.Generic.Dictionary<string,Texture2D> textures=new System.Collections.Generic.Dictionary<string,Texture2D>();
        public static Sprite Sprite(string name){if(!sprites.TryGetValue(name,out var s)||s==null)sprites[name]=s=Resources.Load<Sprite>(Path+name);return s;}
        public static Texture2D Texture(string name){if(!textures.TryGetValue(name,out var t)||t==null)textures[name]=t=Resources.Load<Texture2D>(Path+name);return t;}
        static Sprite boardSprite;
        public static Sprite BoardSprite(){if(boardSprite==null)boardSprite=UnityEngine.Sprite.Create(Texture("BoardStates"),new Rect(8,808,372,124),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);return boardSprite;}
        public static Vector4 BoardUv(int style,int damage)
        {
            int row=Mathf.Clamp(damage,0,3),column=Mathf.Clamp(style+1,0,3);int[] tops={92,348,586,808};
            return new Vector4(372f/1536f,124f/1024f,(8+column*384)/1536f,(1024-tops[row]-124)/1024f);
        }
        // Opaque crown of the intact authored timber, sampled at the strip's 17 columns.
        // Top geometry meets this painted silhouette rather than an unrelated fixed height.
        static readonly float[] crown={.84f,.831f,.847f,.782f,.774f,.758f,.758f,.75f,.75f,.75f,.758f,.766f,.774f,.782f,.855f,.815f,.84f};
        public static float BoardCrown(float u){float column=Mathf.Clamp01(u)*16;int i=Mathf.Min(15,Mathf.FloorToInt(column));return Mathf.Lerp(crown[i],crown[i+1],column-i);}
    }
    // Slightly different paths through each world, with exactly the same island sizes.
    public static class WorldMapComposition
    {
        static readonly Vector2[][] offsets={
            new[]{new Vector2(0,0),new Vector2(-25,10),new Vector2(-5,-22),new Vector2(0,-26),new Vector2(-38,-16)},
            new[]{new Vector2(12,-4),new Vector2(2,0),new Vector2(-28,-30),new Vector2(-8,-16),new Vector2(-44,-12)},
            new[]{new Vector2(4,-10),new Vector2(-36,-10),new Vector2(-44,-12),new Vector2(2,-28),new Vector2(-30,-28)},
            new[]{new Vector2(18,0),new Vector2(-6,2),new Vector2(-35,-26),new Vector2(-12,-10),new Vector2(-48,-30)},
            new[]{new Vector2(5,-6),new Vector2(-30,12),new Vector2(-16,-16),new Vector2(-5,-22),new Vector2(-48,-6)},
            new[]{new Vector2(16,-4),new Vector2(-14,-4),new Vector2(-30,-14),new Vector2(-16,-24),new Vector2(-54,-22)},
            new[]{new Vector2(0,-4),new Vector2(-32,8),new Vector2(-8,-24),new Vector2(2,-30),new Vector2(-26,-12)},
            new[]{new Vector2(10,-10),new Vector2(-8,6),new Vector2(-38,-18),new Vector2(-8,-26),new Vector2(-44,-20)},
            new[]{new Vector2(8,0),new Vector2(-24,-8),new Vector2(-20,-30),new Vector2(-4,-18),new Vector2(-50,0)}
        };
        public static void Apply(Vector4[] places,int world){for(int i=0;i<places.Length;i++){var p=places[i];var d=offsets[Mathf.Clamp(world,0,8)][i];places[i]=new Vector4(p.x+d.x,p.y+d.y,p.z,p.w);}}
    }
}
