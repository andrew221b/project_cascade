using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed class MapReferenceAmbiance:MonoBehaviour
    {
        struct Island {public RectTransform Rect;public Vector2 Rest;public int Index;}
        struct Stem {public RectTransform Rect;public float Angle;public bool Flower;public int Corner;public Vector2 Rest;}
        readonly List<Island> islands=new List<Island>();readonly List<Stem> stems=new List<Stem>();readonly List<MapRopeBridge> bridges=new List<MapRopeBridge>();
        CascadeGame game;RectTransform backdrop,header;readonly Vector3[] frameCorners=new Vector3[4];MapIslandLight light;Material grayscale,bridgeMaterial,contourMaterial;float clock;
        public float AnimationAge=>clock;
        public IReadOnlyList<MapRopeBridge> Bridges=>bridges;
#if UNITY_EDITOR
        bool review;public void SetReviewAge(float age){review=true;clock=age;PaintNow();}public void EndReview(){review=false;}
#endif
        public void Initialize(CascadeGame owner,RectTransform frame){game=owner;backdrop=frame;}
        public void BindHeader(RectTransform row){header=row;}
        public void BindIsland(RectTransform root,int index)=>islands.Add(new Island{Rect=root,Rest=root.anchoredPosition,Index=index});
        public void BindStem(RectTransform root,float rest,bool flower,int corner=-1)=>stems.Add(new Stem{Rect=root,Angle=rest,Flower=flower,Corner=corner,Rest=root.anchoredPosition});
        public void BindBridge(MapRopeBridge bridge){bridges.Add(bridge);if(bridgeMaterial==null){bridgeMaterial=new Material(Resources.Load<Shader>("CascadeArt/MapReferenceOct14/IslandGrayscale")){name="Natural pale wood bridge"};bridgeMaterial.SetFloat("_Saturation",.72f);}bridge.material=bridgeMaterial;bridge.color=new Color(.87f,.80f,.70f,1);}
        public void BindLight(MapIslandLight glow){light=glow;glow.raycastTarget=false;}
        public void BindContour(Image image){contourMaterial=new Material(Resources.Load<Shader>("CascadeArt/MapMikaHalo")){name="Active island living green contour"};contourMaterial.SetVector("_GlowStep",new Vector4(.019f,.019f,0,0));contourMaterial.SetColor("_Color",new Color(.58f,1,.18f,.40f));image.material=contourMaterial;}
        public void Desaturate(Image image){if(grayscale==null)grayscale=new Material(Resources.Load<Shader>("CascadeArt/MapReferenceOct14/IslandGrayscale")){name="Locked island grayscale"};image.material=grayscale;}
        void OnDestroy(){if(grayscale!=null)Destroy(grayscale);if(bridgeMaterial!=null)Destroy(bridgeMaterial);if(contourMaterial!=null)Destroy(contourMaterial);}
        void LateUpdate(){
#if UNITY_EDITOR
            if(review)return;
#endif
            if(game==null||!game.CosmeticAmbianceMotionAllowed||game.CurrentScreen!=GameScreen.JourneyMap||game.BrowsedIsland>=0)return;
            if(!game.ReducedMotion&&game.Preferences.EffectQuality>0)clock+=Mathf.Min(Time.unscaledDeltaTime,.05f);PaintNow();
        }
        public void PaintNow()
        {
            if(game==null)return;bool quiet=game.MapMotionStill;float t=quiet?0:clock;
            foreach(var i in islands){if(i.Rect==null)continue;float p=i.Index*1.37f;
                i.Rect.anchoredPosition=i.Rest+(quiet?Vector2.zero:new Vector2(Mathf.Sin(t*.36f+p)*1.5f,Mathf.Sin(t*.63f+p)*3.3f));
                i.Rect.localRotation=Quaternion.Euler(0,0,quiet?0:Mathf.Sin(t*.39f+p)*.55f);
            }
            var stage=(RectTransform)transform;Vector2 topLeft=Vector2.zero,topRight=Vector2.zero,bottomLeft=Vector2.zero,bottomRight=Vector2.zero;
            if(backdrop!=null){backdrop.GetWorldCorners(frameCorners);topLeft=(Vector2)stage.InverseTransformPoint(frameCorners[1])-new Vector2(stage.rect.xMin,stage.rect.yMax);topRight=(Vector2)stage.InverseTransformPoint(frameCorners[2])-new Vector2(stage.rect.xMax,stage.rect.yMax);bottomLeft=(Vector2)stage.InverseTransformPoint(frameCorners[0])-new Vector2(stage.rect.xMin,stage.rect.yMin);bottomRight=(Vector2)stage.InverseTransformPoint(frameCorners[3])-new Vector2(stage.rect.xMax,stage.rect.yMin);}
            if(header!=null&&stage.parent is RectTransform safe){
                safe.GetWorldCorners(frameCorners);float safeTop=stage.InverseTransformPoint(frameCorners[1]).y-stage.rect.yMax;
                // On taller phones use a little of the spare sky, while short phones
                // retain enough space between the header and the upper islands.
                header.anchoredPosition=new Vector2(0,safeTop-Mathf.Min(48,Mathf.Max(0,safeTop)*.5f));
            }
            for(int i=0;i<stems.Count;i++){var stem=stems[i];if(stem.Rect!=null)stem.Rect.anchoredPosition=stem.Rest+(stem.Corner==0?topLeft:stem.Corner==1?topRight:stem.Corner==2?bottomLeft:stem.Corner==3?bottomRight:Vector2.zero);if(stem.Rect!=null)stem.Rect.localRotation=Quaternion.Euler(0,0,stem.Angle+(quiet?0:(Mathf.Sin(t*.77f+i*1.4f)+.34f*Mathf.Sin(t*1.63f+i))*(stem.Flower?1.8f:2.3f)));}
            if(contourMaterial!=null)contourMaterial.SetColor("_Color",new Color(.58f,1,.18f,.40f*(quiet?1:1+.12f*Mathf.Sin(t*1.6f))));light?.Pose(t,quiet);foreach(var bridge in bridges)bridge.RefreshEndpoints();
        }
    }
    public sealed partial class CascadeGame {internal bool MapMotionStill=>ReducedMotion||Preferences.EffectQuality==0;}
    public readonly struct MapBridgeRoute
    {
        public readonly int From,To;public readonly Vector2 FromPoint,ToPoint;public readonly float Sag;
        public MapBridgeRoute(int from,Vector2 fromPoint,int to,Vector2 toPoint,float sag){From=from;To=to;FromPoint=fromPoint;ToPoint=toPoint;Sag=sag;}
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MapRopeBridge:MaskableGraphic
    {
        RectTransform from,to;Vector2 fromPoint,toPoint,sourceStart,sourceEnd;Sprite artwork;Vector2 a,b;float deckWidth;
        public Vector2 StartPoint=>a;public Vector2 EndPoint=>b;
        public float AttachmentError=>Mathf.Max(Vector2.Distance(a,Endpoint(from,fromPoint)),Vector2.Distance(b,Endpoint(to,toPoint)));
        public override Texture mainTexture=>artwork==null?base.mainTexture:artwork.texture;
        public void Initialize(RectTransform[] islands,MapBridgeRoute route,Sprite drawing,int index){from=islands[route.From];to=islands[route.To];fromPoint=route.FromPoint;toPoint=route.ToPoint;artwork=drawing;sourceStart=WorldFrameArt.BridgeStart[index];sourceEnd=WorldFrameArt.BridgeEnd[index];deckWidth=index==3?.68f:1.3f;raycastTarget=false;RefreshEndpoints();}
        Vector2 Endpoint(RectTransform island,Vector2 offset)=>rectTransform.InverseTransformPoint(island.TransformPoint(new Vector2(island.rect.xMin+offset.x,island.rect.yMax-offset.y)));
        public void RefreshEndpoints(){if(from==null||to==null)return;var x=Endpoint(from,fromPoint);var y=Endpoint(to,toPoint);if((a-x).sqrMagnitude+(b-y).sqrMagnitude>1e-10f)SetVerticesDirty();a=x;b=y;}
        // One uniform rotation/scale preserves the authored deck, rope and perspective.
        // Its two measured landings follow the same live island anchors every frame.
        protected override void OnPopulateMesh(VertexHelper vh){
            vh.Clear();if(artwork==null||from==null||to==null)return;
            var size=artwork.rect.size;var sourceDelta=Vector2.Scale(sourceEnd-sourceStart,size);float length=sourceDelta.magnitude;if(length<1)return;
            var along=sourceDelta/length;var across=new Vector2(-along.y,along.x);var target=b-a;float scale=target.magnitude/length;var direction=target.normalized;var normal=new Vector2(-direction.y,direction.x);var sr=artwork.textureRect;var tex=artwork.texture;
            void Vertex(float u,float v){var delta=Vector2.Scale(new Vector2(u,v)-sourceStart,size);var p=a+(direction*Vector2.Dot(delta,along)+normal*Vector2.Dot(delta,across)*deckWidth)*scale;vh.AddVert(p,color,new Vector2((sr.x+u*sr.width)/tex.width,(sr.y+v*sr.height)/tex.height));}
            Vertex(0,0);Vertex(0,1);Vertex(1,1);Vertex(1,0);vh.AddTriangle(0,1,2);vh.AddTriangle(0,2,3);
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MapIslandLight:MaskableGraphic
    {
        float age;bool still;public float Pulse=>still?1:1+.13f*Mathf.Sin(age*1.6f);
        public void Pose(float t,bool quiet){age=t;still=quiet;raycastTarget=false;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            // The actual island sprite has a pulsing alpha contour. Only a few loose
            // fireflies sit behind it; a filled radial disc made the island look circled.
            for(int i=0;i<11;i++)
            {
                float u=Mathf.Repeat(i*.618034f+.11f,1),v=Mathf.Repeat(i*.414214f+.19f,1);
                var c=r.min+Vector2.Scale(r.size,new Vector2(u,v));
                float z=1.2f+1.5f*(.5f+.5f*Mathf.Sin(age*1.7f+i*1.8f));
                var tint=new Color(.76f,1,.40f,(.17f+.20f*(.5f+.5f*Mathf.Sin(age*1.4f+i)))*Pulse);
                MenuWorldParticles.SoftDot(vh,c,new Vector2(z,z),tint);
            }
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MapModeGlyphs:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;
            void Polygon(params Vector2[] points){int n=vh.currentVertCount;foreach(var p in points)vh.AddVert(r.min+Vector2.Scale(r.size,p),color,Vector2.zero);for(int i=1;i<points.Length-1;i++)vh.AddTriangle(n,n+i,n+i+1);}
            var circle=new Vector2[25];for(int i=0;i<25;i++){float a=i*Mathf.PI*2/24;circle[i]=new Vector2(.13f+Mathf.Cos(a)*.105f,.5f+Mathf.Sin(a)*.43f);}Polygon(circle);Polygon(new Vector2(.365f,.07f),new Vector2(.5f,.94f),new Vector2(.635f,.07f));Polygon(new Vector2(.765f,.07f),new Vector2(.98f,.07f),new Vector2(.98f,.93f),new Vector2(.765f,.93f));}
    }
}
