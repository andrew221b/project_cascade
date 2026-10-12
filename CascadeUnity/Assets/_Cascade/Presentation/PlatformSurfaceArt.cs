using Cascade.Domain;
using UnityEngine;

namespace Cascade.Presentation
{
    // Authored mirror / roller / gust boards, each with four matching damage states.
    public static class PlatformSurfaceArt
    {
        public static Texture2D Texture=>Resources.Load<Texture2D>("CascadeArt/Reactions/PlatformSurfaces");
        public static int Style(FloorDefinition floor,bool entered=false)=>entered&&floor.GustAfterSwaps>0?2:floor.EntryTransform==EntryTransform.MirrorNext?0:
            floor.EntryTransform!=EntryTransform.None?1:floor.GustAfterSwaps>0?2:-1;
        public static Vector4 Uv(int style,int damage)
        {
            int left=style==0?9:style==1?633:1264;
            int top=damage<=0?95:damage==1?268:damage==2?444:622;
            return new Vector4(606/1881f,110/836f,left/1881f,(836-top-110)/836f);
        }
        public static float FaceUvX(float x)
        {
            // Preserve round end hardware while stretching the long inset, not its sockets.
            const float end=.065f,source=.15f;
            return x<end?x/end*source:x>1-end?1-(1-x)/end*source:
                source+(x-end)/(1-2*end)*(1-2*source);
        }
    }
}
