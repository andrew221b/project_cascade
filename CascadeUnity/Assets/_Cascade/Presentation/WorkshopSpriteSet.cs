using UnityEngine;

namespace Cascade.Presentation
{
    // Authored atlas rectangles, in source pixels; source PNGs remain untouched.
    public static class WorkshopSpriteSet
    {
        static readonly int[] WoodTop={7,158,307,452},WoodBottom={153,302,447,592};
        public static Sprite Tool(Texture2D texture,int index)
        {
            float w=texture.width/3f,h=texture.height/2f;
            return Sprite.Create(texture,new Rect(index%3*w,(1-index/3)*h,w,h),new Vector2(.5f,.5f),100);
        }
        public static Sprite Control(Texture2D texture,int index)
        {
            var bounds=new[]{new Rect(34,209,692,327),new Rect(741,209,691,327),new Rect(1447,209,691,327)};
            var r=bounds[index];float s=texture.width/2172f;r=new Rect(r.x*s,r.y*s,r.width*s,r.height*s);
            return Sprite.Create(texture,r,new Vector2(.5f,.5f),300,0,SpriteMeshType.FullRect,new Vector4(r.width*.12f,r.height*.28f,r.width*.12f,r.height*.28f));
        }
        public static Vector4 WoodUv(int state)
        {
            // Generous alpha margins keep all four planks aligned, including exposed fibers.
            int i=Mathf.Clamp(state,0,3);
            return new Vector4(.96f,(WoodBottom[i]-WoodTop[i])/724f,.02f,(724-WoodBottom[i])/724f);
        }
        public static Vector4 ChipUv(int index)=>new Vector4(1f/6f,.172f,(index%6)/6f,.001f);
    }
}
