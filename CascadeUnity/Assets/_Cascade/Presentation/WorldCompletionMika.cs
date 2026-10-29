using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // The complete sleeve/arm/face is authored in every frame. Offline dense
    // vectors interpolate the whole drawing; no independently translated limb.
    public sealed class WorldCompletionMika:MonoBehaviour
    {
        public const string ArtPath="CascadeArt/WorldCompletionOct16/";
        CascadeGame game;RawImage hero,paw;Image board;Material face,front,paper;Texture2D flow;int world;float age;bool review;
        static readonly int FrameId=Shader.PropertyToID("_Frame"),EarsId=Shader.PropertyToID("_Ears"),LightId=Shader.PropertyToID("_LightTint");
        public float AnimationAge=>age;
        public float CurrentFrame=>FrameAt(age);
        public void Initialize(CascadeGame owner,RawImage body,RawImage restingPaw,Image panel,int biome)
        {
            game=owner;hero=body;paw=restingPaw;board=panel;world=biome;
            hero.texture=paw.texture=Resources.Load<Texture2D>(ArtPath+"MikaFrames");
            var data=Resources.Load<TextAsset>(ArtPath+"MikaFlow");
            flow=new Texture2D(384,288,TextureFormat.RGBA32,false,true){name="Offline whole Mika celebration transitions",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};flow.LoadRawTextureData(data.bytes);flow.Apply(false,true);
            face=new Material(Resources.Load<Shader>(ArtPath+"Mika")){name="Whole Mika thumbs-up"};face.SetTexture("_FlowTex",flow);hero.material=face;
            front=new Material(face){name="Same whole pose at board contact"};front.SetFloat("_FrontPaw",1);paw.material=front;
            paper=new Material(Resources.Load<Shader>(WorldFrameArt.Path+"FrameSurface")){name="Completed world paper and rooted flowers"};paper.SetFloat("_World",world);paper.SetFloat("_Header",1);paper.SetFloat("_MapCornerWind",1);board.material=paper;
            Paint();
        }
        public static float FrameAt(float seconds)
        {
            float t=Mathf.Repeat(Mathf.Max(0,seconds),12.8f);
            if(t<.55f)return 0;
            if(t<2.25f)return 7*CharacterPoseMotion.Ease(.55f,2.25f,t);
            if(t<3.5f)return 7;
            if(t<3.82f)return Mathf.Lerp(7,11,CharacterPoseMotion.Ease(3.5f,3.82f,t));
            if(t<6.5f)return 11;
            if(t<6.82f)return Mathf.Lerp(11,7,CharacterPoseMotion.Ease(6.5f,6.82f,t));
            if(t<7.4f)return 7;
            return 7*(1-CharacterPoseMotion.Ease(7.4f,9,t));
        }
        void LateUpdate()
        {
            if(review||game==null)return;
            if(game.CelebrationMotionAllowed&&!game.ReducedMotion&&game.Preferences.EffectQuality>0)age+=Mathf.Min(Time.unscaledDeltaTime,.05f);
            Paint();
        }
        void Paint()
        {
            if(face==null)return;
            bool moving=!game.ReducedMotion&&game.Preferences.EffectQuality>0;float t=moving?age:2.7f;
            float a=moving?.034f*Mathf.Sin(t*1.7f)+.015f*Mathf.Sin(t*3.6f+.8f):0;
            float b=moving?.026f*Mathf.Sin(t*1.4f+1.6f)+.019f*Mathf.Sin(t*2.9f):0;
            var ears=new Vector4(a,b,0,0);var light=MenuWorldArt.Light(world,t,moving);
            face.SetFloat(FrameId,FrameAt(t));front.SetFloat(FrameId,FrameAt(t));face.SetVector(EarsId,ears);front.SetVector(EarsId,ears);
            // Keep the face legible at night; it belongs to the warm foreground.
            var warm=Color.Lerp(Color.white,light,.25f);face.SetColor(LightId,warm);front.SetColor(LightId,warm);
            paper.SetColor(LightId,light);paper.SetFloat("_Age",t);paper.SetFloat("_Wind",moving?Cascade.Application.WorkshopWind.Sample(t+16):0);paper.SetFloat("_CornerMotionAllowed",moving?1:0);
        }
#if UNITY_EDITOR
        public void SetReviewAge(float seconds){review=true;age=seconds;Paint();}
        public void EndReview(){review=false;}
#endif
        void OnDestroy(){if(face!=null)Destroy(face);if(front!=null)Destroy(front);if(paper!=null)Destroy(paper);if(flow!=null)Destroy(flow);}
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WorldCompletionDivider:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;var tint=new Color(.61f,.32f,.09f,.8f);
            void Quad(float x,float y,float width,float height){int n=vh.currentVertCount;vh.AddVert(new Vector2(x,y),tint,Vector2.zero);vh.AddVert(new Vector2(x,y+height),tint,Vector2.zero);vh.AddVert(new Vector2(x+width,y+height),tint,Vector2.zero);vh.AddVert(new Vector2(x+width,y),tint,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
            float mid=r.center.x,y=r.center.y;Quad(r.xMin,y-1,r.width*.5f-19,2);Quad(mid+19,y-1,r.width*.5f-19,2);
            int k=vh.currentVertCount;foreach(var v in new[]{new Vector2(mid-8,y),new Vector2(mid,y+8),new Vector2(mid+8,y),new Vector2(mid,y-8)})vh.AddVert(v,tint,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }
    }
}
