using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    // Two authored PNG sprites. Existing direction controls keep their input and serialization.
    static class NavigationArrowArt
    {
        static Texture2D atlas;
        static Sprite[] arrows;
        static readonly Vector4[] SourceCuts={new Vector4(0,.075f,.765f,1),new Vector4(0,.065f,.852f,1)};
        public static Texture Texture=>atlas!=null?atlas:atlas=Resources.Load<Texture2D>("CascadeArt/Navigation/ArrowPairOct08");
        static Sprite Arrow(int weight){
            if(arrows==null){
                var texture=(Texture2D)Texture;
                if(texture==null)return null;
                var rects=new[]{new Rect(119,497,1301,426),new Rect(122,146,1294,222)};
                arrows=new Sprite[2];
                for(int i=0;i<2;i++)arrows[i]=Sprite.Create(texture,rects[i],new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            }
            return arrows[Mathf.Clamp(weight,0,1)];
        }
        public static void Draw(VertexHelper h,Rect rect,int weight,bool right,Color tint){
            h.Clear();var sprite=Arrow(weight);if(sprite==null)return;
            // Shorten the shaft with three horizontal slices, preserving the tip and rounded cap.
            float ratio=weight==0?2.3f:3;
            var size=new Vector2(Mathf.Min(rect.width,rect.height*ratio),Mathf.Min(rect.height,rect.width/ratio));
            var area=new Rect(rect.center-size*.5f,size);var uv=sprite.textureRect;
            var source=SourceCuts[Mathf.Clamp(weight,0,1)];
            float nativeRatio=sprite.rect.width/sprite.rect.height;
            float cap=source.y*nativeRatio*size.y,tip=(1-source.z)*nativeRatio*size.y;
            var target=new Vector4(0,cap,size.x-tip,size.x);
            float top=uv.yMax/sprite.texture.height,bottom=uv.yMin/sprite.texture.height;
            for(int i=0;i<3;i++){
                float x0=area.xMin+target[i],x1=area.xMin+target[i+1];
                float left=(uv.xMin+source[i]*uv.width)/sprite.texture.width,rightUv=(uv.xMin+source[i+1]*uv.width)/sprite.texture.width;
                if(!right){x0=area.xMax-target[i+1];x1=area.xMax-target[i];float swap=left;left=rightUv;rightUv=swap;}
                int start=h.currentVertCount;
                h.AddVert(new Vector2(x0,area.yMin),tint,new Vector2(left,bottom));
                h.AddVert(new Vector2(x0,area.yMax),tint,new Vector2(left,top));
                h.AddVert(new Vector2(x1,area.yMax),tint,new Vector2(rightUv,top));
                h.AddVert(new Vector2(x1,area.yMin),tint,new Vector2(rightUv,bottom));
                h.AddTriangle(start,start+1,start+2);h.AddTriangle(start,start+2,start+3);
            }
        }
    }
}
