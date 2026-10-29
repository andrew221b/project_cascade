using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed class PauseReferenceAmbiance:MonoBehaviour
    {
        CascadeGame game;PauseLanternLight halo,flame;RectTransform[] flowers;float clock;
        public float AnimationAge=>clock;
#if UNITY_EDITOR
        bool review;public void SetReviewAge(float age){review=true;clock=age;Paint();}public void EndReview(){review=false;}
#endif
        public void Initialize(CascadeGame owner,PauseLanternLight ambient,PauseLanternLight glass,RectTransform[] stems){game=owner;halo=ambient;flame=glass;flowers=stems;Paint();}
        void LateUpdate(){
#if UNITY_EDITOR
            if(review)return;
#endif
            // The puzzle is paused, these menu decorations use the foreground clock.
            if(game==null||!game.CelebrationMotionAllowed||game.CurrentScreen!=GameScreen.Pause)return;
            if(!game.ReducedMotion&&game.Preferences.EffectQuality>0)clock+=Mathf.Min(Time.unscaledDeltaTime,.05f);
            Paint();
        }
        void Paint()
        {
            if(game==null||halo==null||flame==null)return;
            bool quiet=game.ReducedMotion||game.Preferences.EffectQuality==0;
            float t=quiet?0:clock;
            float light=quiet?1:1+.055f*Mathf.Sin(t*2.3f)+.028f*Mathf.Sin(t*5.1f+.7f)+.014f*Mathf.Sin(t*8.3f);
            halo.Pose(t,light);flame.Pose(t,light);
            for(int i=0;i<flowers.Length;i++)if(flowers[i]!=null)flowers[i].localRotation=Quaternion.Euler(0,0,quiet?0:WorldSeasonalArt.PlantSway(t+16,i)*.38f);
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PauseLanternLight:MaskableGraphic
    {
        bool halo;float age,intensity=1;
        public float Intensity=>intensity;
        public void Configure(bool ambient){halo=ambient;raycastTarget=false;SetVerticesDirty();}
        public void Pose(float t,float brightness){age=t;intensity=brightness;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;var center=r.center;
            const int segments=64,rings=8;
            for(int ring=0;ring<=rings;ring++)for(int i=0;i<=segments;i++){
                float p=ring/(float)rings,a=i*Mathf.PI*2/segments;
                float alpha=Mathf.Pow(1-p,halo?2.1f:1.35f)*(halo?.17f:.27f)*intensity;
                float sway=halo?0:Mathf.Sin(age*1.7f+p*3)*r.width*.016f*p;
                vh.AddVert(center+new Vector2(Mathf.Cos(a)*r.width*.5f*p+sway,Mathf.Sin(a)*r.height*.5f*p),new Color(1,halo?.77f:.95f,halo?.20f:.62f,alpha),Vector2.zero);
            }
            for(int ring=0;ring<rings;ring++)for(int i=0;i<segments;i++){int k=ring*(segments+1)+i;vh.AddTriangle(k,k+segments+1,k+1);vh.AddTriangle(k+1,k+segments+1,k+segments+2);}
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PauseGlyphGraphic:MaskableGraphic
    {
        int kind,measuredKind=-1;Rect measuredRect;Bounds paintedBounds;
        public Bounds PaintedBounds
        {
            get{
                if(measuredKind!=kind||measuredRect!=rectTransform.rect)using(var helper=new VertexHelper())OnPopulateMesh(helper);
                return paintedBounds;
            }
        }
        public void Configure(int value){kind=value;color=value==3?new Color(1,.98f,.88f):new Color(.19f,.055f,.014f);raycastTarget=false;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            PopulateGlyph(vh);measuredRect=rectTransform.rect;measuredKind=kind;
            var vertex=UIVertex.simpleVert;paintedBounds=new Bounds(measuredRect.center,Vector3.zero);
            for(int i=0;i<vh.currentVertCount;i++){vh.PopulateUIVertex(ref vertex,i);if(i==0)paintedBounds=new Bounds(vertex.position,Vector3.zero);else paintedBounds.Encapsulate(vertex.position);}
        }
        void PopulateGlyph(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;float size=Mathf.Min(r.width,r.height);var c=r.center;
            Vector2 P(float x,float y)=>c+(new Vector2(x,y)-Vector2.one*.5f)*size;
            void Triangle(Vector2 a,Vector2 b,Vector2 d){int k=vh.currentVertCount;vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);vh.AddVert(d,color,Vector2.zero);vh.AddTriangle(k,k+1,k+2);}
            void Box(float x,float y,float w,float h){var a=P(x,y);var b=P(x+w,y);var d=P(x,y+h);var e=P(x+w,y+h);Triangle(a,b,d);Triangle(d,b,e);}
            if(kind==0){
                Triangle(P(.06f,.53f),P(.5f,.95f),P(.5f,.76f));Triangle(P(.06f,.53f),P(.5f,.76f),P(.16f,.44f));
                Triangle(P(.5f,.95f),P(.94f,.53f),P(.84f,.44f));Triangle(P(.5f,.95f),P(.84f,.44f),P(.5f,.76f));
                Box(.73f,.73f,.085f,.19f);Box(.22f,.10f,.205f,.48f);Box(.575f,.10f,.205f,.48f);Box(.22f,.39f,.56f,.23f);
                Triangle(P(.22f,.62f),P(.5f,.81f),P(.78f,.62f));return;
            }
            if(kind==3){Triangle(P(.16f,.10f),P(.86f,.5f),P(.16f,.9f));return;}
            if(kind==4){
                void Ellipse(Vector2 center,Vector2 axis,float major,float minor,Color tint){const int n=16;for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n,b=(i+1)*Mathf.PI*2/n;var normal=new Vector2(-axis.y,axis.x);int k=vh.currentVertCount;vh.AddVert(center,tint,Vector2.zero);vh.AddVert(center+axis*Mathf.Cos(a)*major+normal*Mathf.Sin(a)*minor,tint,Vector2.zero);vh.AddVert(center+axis*Mathf.Cos(b)*major+normal*Mathf.Sin(b)*minor,tint,Vector2.zero);vh.AddTriangle(k,k+1,k+2);}}
                for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Ellipse(c+d*size*.245f,d,size*.18f,size*.095f,new Color(1,.985f,.93f));}
                Ellipse(c,Vector2.right,size*.145f,size*.145f,new Color(1,.72f,.13f));return;
            }
            if(kind==1){
                const int n=96;
                for(int i=0;i<n;i++){
                    float a=i*Mathf.PI*2/n,b=(i+1)*Mathf.PI*2/n;
                    float Radius(float angle){float tooth=Mathf.Repeat(angle/(Mathf.PI*2)*8,1);return tooth>.24f&&tooth<.76f?.44f:.335f;}
                    var a0=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*size*Radius(a);var b0=c+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*size*Radius(b);
                    var a1=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*size*.15f;var b1=c+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*size*.15f;Triangle(a0,b0,a1);Triangle(a1,b0,b1);
                }return;
            }
            const int pieces=64;float radius=size*.32f,thickness=size*.16f;
            for(int i=0;i<pieces;i++){
                float a=Mathf.Lerp(90,400,i/(float)pieces)*Mathf.Deg2Rad,b=Mathf.Lerp(90,400,(i+1)/(float)pieces)*Mathf.Deg2Rad;
                var da=new Vector2(Mathf.Cos(a),Mathf.Sin(a));var db=new Vector2(Mathf.Cos(b),Mathf.Sin(b));
                Triangle(c+da*(radius+thickness*.5f),c+db*(radius+thickness*.5f),c+da*(radius-thickness*.5f));Triangle(c+da*(radius-thickness*.5f),c+db*(radius+thickness*.5f),c+db*(radius-thickness*.5f));
            }
            Triangle(P(.80f,.85f),P(.53f,1f),P(.51f,.65f));
        }
    }
}
