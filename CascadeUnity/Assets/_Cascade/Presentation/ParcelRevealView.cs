using System;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    // One foreground presentation clock owns every layer; transactions live in CascadeGame.
    public sealed class ParcelRevealView:MonoBehaviour
    {
        public const float RevealDuration=4.65f,CollectionDuration=1.45f;
        CascadeGame game;Image box,body,trim,opened,envelope,emptyEnvelope;ParcelSunlight light;ParcelOpeningFlash flash;
        RectTransform[] cards;Vector2[] rests;CanvasGroup[] groups;
        ParcelFabricMotion cloth,rope;float age,collectAge,idle;bool settled,collecting,bonus,collected,sound,review;
        Action revealed,picked;
        public bool Busy=>!settled||collecting;
        public float RevealAge=>age;
        public void Initialize(CascadeGame owner,Image closed,Image fabric,Image adornments,Image open,Image inner,Image empty,ParcelSunlight glow,RectTransform[] rewards,bool claimed,bool skip,bool second,Action ready)
        {
            game=owner;box=closed;body=fabric;trim=adornments;opened=open;envelope=inner;emptyEnvelope=empty;light=glow;cards=rewards;
            flash=GetComponentInChildren<ParcelOpeningFlash>();bonus=second;collected=claimed;revealed=ready;rests=new Vector2[cards.Length];groups=new CanvasGroup[cards.Length];
            for(int i=0;i<cards.Length;i++){rests[i]=cards[i].anchoredPosition;groups[i]=cards[i].gameObject.AddComponent<CanvasGroup>();groups[i].blocksRaycasts=false;}
            cloth=box.gameObject.AddComponent<ParcelFabricMotion>();rope=trim.gameObject.AddComponent<ParcelFabricMotion>();
            age=skip?RevealDuration:0;settled=skip;ApplyReveal();if(skip)ready?.Invoke();
        }
        public void Collect(Action complete){if(Busy)return;collecting=true;collectAge=0;picked=complete;}
        void Update()
        {
            if(game==null||review||!game.CelebrationMotionAllowed)return;
            float dt=Mathf.Min(.05f,Time.unscaledDeltaTime);idle+=dt;
            if(collecting){collectAge+=dt;ApplyCollection();if(collectAge>=CollectionDuration){collecting=false;collected=true;var action=picked;picked=null;action?.Invoke();}return;}
            if(!settled){age=Mathf.Min(RevealDuration,age+dt*(game.ReducedMotion?6:1));ApplyReveal();
                if(age>=2.15f&&!sound){sound=true;game.Feedback.Sound(ToyCue.Success);game.Feedback.Haptic(ToyHapticMoment.Parcel);}
                if(age>=RevealDuration){settled=true;revealed?.Invoke();}
            }else{ApplyIdle();light.Pose(idle,0,game.Preferences.EffectQuality>0&&!game.ReducedMotion);}
        }
        static float Ease(float p)=>1-Mathf.Pow(1-Mathf.Clamp01(p),3);
        static void Alpha(Image image,float a){image.color=new Color(1,1,1,a);}
        void ApplyIdle()
        {
            cloth.Clock=rope.Clock=idle;cloth.Amplitude=rope.Amplitude=game.ReducedMotion?0:1;
            box.rectTransform.localRotation=Quaternion.identity;box.rectTransform.localScale=Vector3.one;
            envelope.rectTransform.localRotation=Quaternion.Euler(0,0,game.ReducedMotion?0:Mathf.Sin(idle*1.15f)*2);
        }
        void ApplyReveal()
        {
            float unwrap=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-1.55f)/.5f));
            float open=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-2.05f)/.55f));
            Alpha(emptyEnvelope,bonus?open:0);
            if(game.ReducedMotion||game.Preferences.EffectQuality==0){unwrap=age>=1.8f?1:0;open=age>=2.25f?1:0;Alpha(emptyEnvelope,bonus?open:0);}
            flash?.SetPower(game.ReducedMotion||game.Preferences.EffectQuality==0?0:Mathf.Sin(open*Mathf.PI));
            if(bonus){Alpha(box,0);Alpha(body,0);Alpha(trim,0);Alpha(opened,1);Alpha(envelope,1-open);}
            else{Alpha(box,1-unwrap);Alpha(body,unwrap*(1-open));Alpha(trim,unwrap*(1-open));Alpha(opened,open);Alpha(envelope,0);}
            float shake=game.ReducedMotion?0:Mathf.Clamp01((age-.55f)/.85f)*(1-open);
            float angle=Mathf.Sin(age*34)*2.7f*shake;
            box.rectTransform.localRotation=body.rectTransform.localRotation=trim.rectTransform.localRotation=Quaternion.Euler(0,0,angle);
            box.rectTransform.localScale=body.rectTransform.localScale=trim.rectTransform.localScale=Vector3.one*(1+Mathf.Sin(age*17)*.007f*shake);
            cloth.Clock=rope.Clock=idle;cloth.Amplitude=game.ReducedMotion?0:1;rope.Amplitude=game.ReducedMotion?0:1;rope.Release=game.ReducedMotion?0:Mathf.SmoothStep(0,1,Mathf.Clamp01((age-1.45f)/.65f));
            opened.rectTransform.localRotation=Quaternion.Euler(0,0,game.ReducedMotion?0:Mathf.Sin(open*Mathf.PI)*-2);
            envelope.rectTransform.localScale=Vector3.one*(1+Mathf.Sin(age*28)*shake*.025f);
            emptyEnvelope.rectTransform.localScale=Vector3.one*(1+Mathf.Sin(open*Mathf.PI)*.08f);
            emptyEnvelope.rectTransform.localRotation=Quaternion.Euler(0,0,game.ReducedMotion?0:Mathf.Sin(open*Mathf.PI)*-5);
            light.Pose(idle,Mathf.Sin(open*Mathf.PI),game.Preferences.EffectQuality>0&&!game.ReducedMotion);
            for(int i=0;i<cards.Length;i++){
                float p=Mathf.Clamp01((age-2.65f-i*.24f)/1.15f),ease=Ease(p);
                Vector2 start=new Vector2(360-cards[i].rect.width*.5f,-526);
                cards[i].anchoredPosition=Vector2.Lerp(start,rests[i],ease)+Vector2.up*Mathf.Sin(p*Mathf.PI)*112;
                cards[i].localScale=Vector3.one*Mathf.Lerp(.08f,1,ease);cards[i].localRotation=Quaternion.Euler(0,0,game.ReducedMotion?0:Mathf.Lerp((i%2==0?-1:1)*17,0,ease));
                groups[i].alpha=collected?0:Mathf.Clamp01(p*5);
            }
        }
        void ApplyCollection()
        {
            light.Pose(idle,Mathf.Sin(Mathf.Clamp01(collectAge/CollectionDuration)*Mathf.PI)*.55f,!game.ReducedMotion&&game.Preferences.EffectQuality>0);
            for(int i=0;i<cards.Length;i++){
                float p=Mathf.Clamp01((collectAge-i*.09f)/(CollectionDuration-.2f)),ease=Ease(p);
                float theta=p*Mathf.PI*3,radius=Mathf.Sin(p*Mathf.PI)*74*(1-p);
                Vector2 spiral=game.ReducedMotion?Vector2.zero:new Vector2(Mathf.Cos(theta),Mathf.Sin(theta)) *radius;
                cards[i].anchoredPosition=Vector2.Lerp(rests[i],new Vector2(360-cards[i].rect.width*.5f,-220),ease)+spiral;
                cards[i].localScale=Vector3.one*Mathf.Lerp(1,.035f,p*p);cards[i].localRotation=Quaternion.Euler(0,0,game.ReducedMotion?0:Mathf.Sin(theta)*14);
                groups[i].alpha=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((p-.55f)/.45f));
            }
        }
#if UNITY_EDITOR
        public void SetReviewPose(float seconds,bool pickup=false){review=true;if(pickup){collectAge=seconds;ApplyCollection();}else{age=seconds;idle=seconds;ApplyReveal();if(seconds>=RevealDuration&&!settled){settled=true;revealed?.Invoke();}}}
        public void ResumeReview(){review=false;}
        public void SetClosedReviewWithCards(){review=true;flash?.SetPower(0);Alpha(box,1);Alpha(body,0);Alpha(trim,0);Alpha(opened,0);Alpha(envelope,0);Alpha(emptyEnvelope,0);
            for(int i=0;i<cards.Length;i++){cards[i].anchoredPosition=rests[i];cards[i].localScale=Vector3.one;cards[i].localRotation=Quaternion.identity;groups[i].alpha=1;}
            light.Pose(0,0,true);
        }
#endif
    }
    public sealed class ParcelFabricMotion:BaseMeshEffect
    {
        public float Clock,Release,Amplitude=1;
        void LateUpdate(){if(graphic!=null&&Amplitude>0)graphic.SetVerticesDirty();}
        public override void ModifyMesh(VertexHelper mesh)
        {
            if(!IsActive()||mesh.currentVertCount!=4)return;
            var bl=new UIVertex();var tr=new UIVertex();mesh.PopulateUIVertex(ref bl,0);mesh.PopulateUIVertex(ref tr,2);mesh.Clear();
            const int n=18;
            for(int y=0;y<=n;y++)for(int x=0;x<=n;x++){
                float u=x/(float)n,v=y/(float)n;var p=Vector3.Lerp(bl.position,tr.position,0);p.x=Mathf.Lerp(bl.position.x,tr.position.x,u);p.y=Mathf.Lerp(bl.position.y,tr.position.y,v);
                float leaf=Mathf.SmoothStep(0,1,Mathf.Clamp01((v-.68f)/.2f))*Mathf.SmoothStep(0,1,Mathf.Clamp01((u-.5f)/.2f));
                float tag=Mathf.Exp(-Mathf.Pow((u-.8f)*8,2)-Mathf.Pow((v-.38f)*7,2));
                p.x+=Amplitude*(Mathf.Sin(Clock*2.1f+v*3)*leaf*2.1f+Mathf.Sin(Clock*1.7f)*tag*2.3f);
                p.y+=Amplitude*Mathf.Sin(Clock*2.6f+u*4)*leaf*.75f;
                float loop=Mathf.SmoothStep(0,1,Mathf.Clamp01((v-.48f)/.2f));
                p.x+=Release*(Mathf.Sign(u-.5f)*Mathf.Abs(u-.5f)*loop*132+tag*62);
                p.y-=Release*((1-loop)*Mathf.Sin(u*Mathf.PI)*28+tag*36);
                var vert=bl;vert.position=p;vert.uv0=new Vector4(Mathf.Lerp(bl.uv0.x,tr.uv0.x,u),Mathf.Lerp(bl.uv0.y,tr.uv0.y,v),0,0);mesh.AddVert(vert);
            }
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){int i=y*(n+1)+x;mesh.AddTriangle(i,i+n+1,i+1);mesh.AddTriangle(i+1,i+n+1,i+n+2);}
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ParcelSunlight:MaskableGraphic
    {
        float clock,burst;bool motion;
        protected override void Awake(){base.Awake();raycastTarget=false;}
        public void Pose(float t,float flash,bool moving){clock=t;burst=flash;motion=moving;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=rectTransform.rect;Vector2 c=rect.center;float r=rect.width*.5f;
            const int steps=144,rings=12;
            for(int ring=0;ring<=rings;ring++)for(int i=0;i<=steps;i++){
                float p=ring/(float)rings,a=i*Mathf.PI*2/steps,wave=Mathf.Pow(Mathf.Max(0,Mathf.Cos(a*18+(motion?clock*.17f:0))),10);
                float fall=1-p,alpha=fall*(.28f+wave*p*.95f)*(1+burst*.5f);
                var color=Color.Lerp(new Color(1,.96f,.59f,alpha),new Color(1,.83f,.25f,alpha),p);
                vh.AddVert(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*p,color,Vector2.zero);
            }
            for(int ring=0;ring<rings;ring++)for(int i=0;i<steps;i++){int k=ring*(steps+1)+i;vh.AddTriangle(k,k+steps+1,k+1);vh.AddTriangle(k+1,k+steps+1,k+steps+2);}
            if(!motion)return;
            for(int i=0;i<24;i++){
                float phase=Mathf.Repeat(clock*.2f+i*.618f,1),a=i*2.39996f+clock*.1f;
                Vector2 p=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*(.25f+phase*.65f);
                float size=(2.3f+Mathf.Sin(i)*.8f)*(1+burst*.5f);float alpha=Mathf.Sin(phase*Mathf.PI)*(.45f+burst*.45f);
                int k=vh.currentVertCount;var color=new Color(1,1,.73f,alpha);
                vh.AddVert(p+Vector2.up*size*2,color,Vector2.zero);vh.AddVert(p+Vector2.right*size,color,Vector2.zero);vh.AddVert(p-Vector2.up*size*2,color,Vector2.zero);vh.AddVert(p-Vector2.right*size,color,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
            }
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ParcelOpeningFlash:MaskableGraphic
    {
        float power;
        protected override void Awake(){base.Awake();raycastTarget=false;}
        public void SetPower(float value){power=value;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();if(power<.001f)return;var rect=rectTransform.rect;var center=rect.center;float rx=rect.width*.5f,ry=rect.height*.5f;
            const int rings=12,segments=64;
            for(int r=0;r<=rings;r++)for(int i=0;i<=segments;i++){
                float p=r/(float)rings,a=i*Mathf.PI*2/segments,alpha=power*(1-Mathf.Pow(p,6));
                mesh.AddVert(center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry)*p,new Color(1,.985f,.76f,alpha),Vector2.zero);
            }
            for(int r=0;r<rings;r++)for(int i=0;i<segments;i++){int k=r*(segments+1)+i;mesh.AddTriangle(k,k+segments+1,k+1);mesh.AddTriangle(k+1,k+segments+1,k+segments+2);}
        }
    }

}
