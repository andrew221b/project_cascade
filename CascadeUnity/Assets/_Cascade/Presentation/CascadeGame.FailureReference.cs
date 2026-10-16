using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        RectTransform failureReferenceRoot;
        void RefreshFailureReference()
        {
            bool show=CurrentScreen==GameScreen.Fail&&!ConversationOpen;
            if(!show){if(failureReferenceRoot!=null){failureReferenceRoot.gameObject.SetActive(false);Destroy(failureReferenceRoot.gameObject);failureReferenceRoot=null;}return;}
            if(failureReferenceRoot==null){
                failureReferenceRoot=Rect("Reference failure result",safeRoot.parent,Vector2.zero,Vector2.one);var canvas=failureReferenceRoot.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=12;failureReferenceRoot.gameObject.AddComponent<GraphicRaycaster>();
                var stage=Rect("Failure reference layout",failureReferenceRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f));stage.sizeDelta=new Vector2(720,1280);stage.gameObject.AddComponent<ReferenceViewportFit>();
                var rig=ReferenceRect("Failure hanging assembly",stage,0,0,720,1280);rig.pivot=new Vector2(.5f,.55f);rig.anchoredPosition=new Vector2(360,-576);
                var bench=ReferencePicture("Failure Mika wooden bench",rig,FailureReferenceArt.Get(2),94,536,538,68,false);
                var heading=ReferencePicture("Failure title board",stage,ReferenceShopArt("HeaderBoard"),112,94,496,144,false);heading.type=Image.Type.Sliced;heading.pixelsPerUnitMultiplier=4;
                var title=ReferenceText("failure heading",stage,ReferenceCopy("Don't give up!","Не отчаивайся!","Не здавайся!"),166,147,388,65,44);CentreHeading(heading,title);
                var heroBounds=FailureReferenceArt.HeroBounds;var hero=ReferenceRect("Failure hopeful Mika",rig,heroBounds.x,heroBounds.y,heroBounds.width,heroBounds.height).gameObject.AddComponent<ReferenceCutoutImage>();hero.sprite=FailureReferenceArt.Hero();hero.SetContour(FailureReferenceArt.HeroContour);
                var paper=ReferencePicture("Failure hanging results panel",rig,GameplayReferenceArt.Sprite("HelpPanel"),96,610,529,220,false);paper.gameObject.AddComponent<PrimaryButtonSkin>().Configure(GameplayReferenceArt.Sprite("HelpPanel"));
                ReferencePicture("Failure left gold eyelet",rig,FailureReferenceArt.Get(9),108,607,25,39);
                ReferencePicture("Failure right gold eyelet",rig,FailureReferenceArt.Get(10),586,607,25,39);
                for(int i=0;i<2;i++){var rope=ReferenceRect("Failure braided cord "+i,stage,0,0,720,1280).gameObject.AddComponent<HangingResultRope>();rope.transform.SetSiblingIndex(0);rope.Initialize(this,bench.rectTransform,paper.rectTransform,i);}
                string[] values={"0",Interaction.Attempt.MovesMade.ToString(),"0"};UiCopy[] names={"Score","Moves","Stars"};Sprite[] icons={FailureReferenceArt.Get(3),FailureReferenceArt.Get(4),FailureReferenceArt.Get(5)};
                for(int i=0;i<3;i++){float x=153+i*149;var medal=ReferenceRect("Failure result medal "+i,rig,x+7,636,102,112).gameObject.AddComponent<ReferenceCutoutImage>();medal.sprite=icons[i];medal.SetContour(FailureReferenceArt.MedalContour(i));ReferenceText("failure value "+i,rig,UiCopy.Raw(values[i]),x+9,746,98,40,36);ReferenceText("failure caption "+i,rig,names[i],x-4,779,124,28,23);}
                var retryButton=ReferenceButton("Retry",stage,156,873,427,110,Retry,"GreenAction");var retryMark=ReferenceRect("Failure retry mark",ReferenceButtonBody(retryButton),34,23,59,62).gameObject.AddComponent<PauseGlyphGraphic>();retryMark.Configure(2);retryMark.color=MenuPaper;var retryCaption=ReferenceText("failure retry caption",ReferenceButtonBody(retryButton),ReferenceCopy("Play again","Сыграть ещё раз","Зіграти ще раз"),113,22,285,65,31,true);CentreContent((RectTransform)ReferenceButtonBody(retryButton),retryMark.rectTransform,retryCaption.rectTransform);retryButton.interactable=Interaction.AllowsRetry&&!saveBlocked&&!HasSaveError;
                var map=ReferenceButton("Resulthome",stage,224,1023,288,109,ShowJourney,"AmberAction");var mapIcon=ReferencePicture("Failure map scroll",ReferenceButtonBody(map),FailureReferenceArt.Get(8),27,15,83,78);var mapCaption=ReferenceText("failure map caption",ReferenceButtonBody(map),"Map",107,21,144,64,36);mapCaption.color=new Color(.19f,.055f,.014f);CentreContent((RectTransform)ReferenceButtonBody(map),mapIcon.rectTransform,mapCaption.rectTransform);
                if(CanOfferStrongerWood){var stronger=ReferenceButton("Failure more wood",stage,232,1160,274,67,StartNextPreferred,"TealAction");ReferenceText("failure more wood",ReferenceButtonBody(stronger),"More wood",12,10,250,45,24,true);}
                rig.Find("Failure hopeful Mika").SetAsLastSibling();
                var atmosphere=failureReferenceRoot.gameObject.AddComponent<FailureReferenceAmbiance>();atmosphere.Initialize(this,stage,hero,bench,heading);
                var leaves=ReferenceRect("Natural failure foliage",stage,0,0,720,1280);leaves.gameObject.AddComponent<RectMask2D>();leaves.gameObject.AddComponent<MenuLeafDrift>().Initialize(this,ReferenceShopArt("MenuGreenLeaf"),true,false);leaves.SetAsFirstSibling();
                rig.gameObject.AddComponent<HangingResultAssembly>().Initialize(this);
                var arrival=stage.gameObject.AddComponent<MenuTransition>();arrival.Initialize(this);arrival.Arrive();
            }
            header.gameObject.SetActive(false);statusPaper.gameObject.SetActive(false);actions.gameObject.SetActive(false);receiver.gameObject.SetActive(false);floorArea.gameObject.SetActive(false);resultRoot.gameObject.SetActive(false);machine.SetRigVisible(false);outcome.HideReferenceFailure();
        }
    }
    public sealed class ReferenceViewportFit:MonoBehaviour
    {
        RectTransform rect,parent;void Awake(){rect=(RectTransform)transform;parent=(RectTransform)transform.parent;}
        void LateUpdate(){if(rect!=null&&parent!=null)rect.localScale=Vector3.one*Mathf.Min(parent.rect.width/720f,parent.rect.height/1280f);}
    }
    public sealed class HangingResultAssembly:MonoBehaviour
    {
        CascadeGame game;RectTransform rect;Vector2 anchor;readonly ReceiverSuspension spring=new ReceiverSuspension();
        public void Initialize(CascadeGame g){game=g;rect=(RectTransform)transform;anchor=rect.anchoredPosition;}
        void LateUpdate(){if(game==null||!game.CelebrationMotionAllowed)return;spring.Tick(Mathf.Min(Time.unscaledDeltaTime,.05f),game.Machine.WindAt(0),game.ReducedMotion||game.Preferences.EffectQuality==0);rect.anchoredPosition=anchor+new Vector2(spring.Sway*30,spring.Heave*25);rect.localRotation=Quaternion.Euler(0,0,spring.Roll);}
    }
    [DefaultExecutionOrder(20),RequireComponent(typeof(CanvasRenderer))]
    public sealed class HangingResultRope:MaskableGraphic
    {
        CascadeGame game;RectTransform bench,panel;int side;PresentationRope rope,lower;float age;readonly Vector3[] corners=new Vector3[4];
        public override Texture mainTexture=>GameplayReferenceArt.Texture("HempRope");
        public float AttachmentError{get;private set;}
        public void Initialize(CascadeGame owner,RectTransform support,RectTransform resultPanel,int index){game=owner;bench=support;panel=resultPanel;side=index;rope=new PresentationRope(15,71+index*43);lower=new PresentationRope(7,113+index*43);raycastTarget=false;}
        void LateUpdate(){
            if(bench==null||panel==null)return;bench.GetWorldCorners(corners);var a=(Vector2)rectTransform.InverseTransformPoint(corners[0]);var b=(Vector2)rectTransform.InverseTransformPoint(corners[3]);var bottom=Vector2.Lerp(a,b,side==0?.10f:.90f)+Vector2.up*bench.rect.height*.50f;
            // The reference stage can be inset vertically on a tall phone; suspension
            // still reaches the physical screen edge rather than hanging in the sky.
            ((RectTransform)GetComponentInParent<Canvas>().transform).GetWorldCorners(corners);var top=new Vector2(bottom.x,rectTransform.InverseTransformPoint(corners[1]).y+20);
            bool moving=game.CelebrationMotionAllowed;float dt=moving?Mathf.Min(Time.unscaledDeltaTime,.05f):0;age+=dt;bool reduced=game.ReducedMotion||game.Preferences.EffectQuality==0;
            rope.Advance(top,bottom,dt,age,game.Machine.WindAt(0),25,14,14,reduced);
            panel.GetWorldCorners(corners);var left=(Vector2)rectTransform.InverseTransformPoint(corners[1]);var right=(Vector2)rectTransform.InverseTransformPoint(corners[2]);var ring=Vector2.Lerp(left,right,side==0?.045f:.950f)-Vector2.up*17;
            lower.Advance(bottom,ring,dt,age,game.Machine.WindAt(0),8,4,4,reduced);AttachmentError=Mathf.Max(Vector2.Distance(rope.Sample(1),bottom),Vector2.Distance(lower.Sample(1),ring));color=Color.Lerp(Color.white,MenuWorldArt.Light(game.EnvironmentWorld,age,moving),.55f);SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(rope==null)return;Draw(vh,rope,40,7);Draw(vh,lower,10,6);}
        void Draw(VertexHelper vh,PresentationRope cord,int segments,float radius){float distance=0;for(int i=0;i<segments;i++){var a=cord.Sample(i/(float)segments);var b=cord.Sample((i+1)/(float)segments);float next=distance+Vector2.Distance(a,b);var across=new Vector2(-(b-a).y,(b-a).x).normalized*radius;int n=vh.currentVertCount;vh.AddVert(a-across,color,new Vector2(0,distance/145));vh.AddVert(a+across,color,new Vector2(1,distance/145));vh.AddVert(b+across,color,new Vector2(1,next/145));vh.AddVert(b-across,color,new Vector2(0,next/145));vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);distance=next;}}
    }
}
