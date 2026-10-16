using Cascade.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Inlays follow the rendered timber, never the authoritative puzzle state.
    // The same hemp-and-brass drawing is used in the lesson and on the real board.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PlatformMechanismGraphic : MaskableGraphic
    {
        CascadeGame game;FloorDefinition floor;int index,count;bool demo;
        readonly Color dark=new Color(.22f,.105f,.035f),hemp=new Color(.78f,.56f,.28f),edgeLight=new Color(1,.86f,.51f);
        public void Initialize(CascadeGame owner,FloorDefinition data,int floorIndex,int symbols)
        {game=owner;floor=data;index=floorIndex;count=symbols;raycastTarget=false;SetVerticesDirty();}
        public void InitializeDemo(){demo=true;raycastTarget=false;SetVerticesDirty();}
        void LateUpdate()
        {
            if(game==null||game.Machine==null||!game.Machine.HasFloorView(index,rectTransform.parent as RectTransform))return;
            canvasRenderer.SetAlpha(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.22f,.65f,game.Machine.FloorBreakProgress(index))));
            SetVerticesDirty();
        }
        Vector2 Surface(float x,float y)=>rectTransform.InverseTransformPoint(game.Machine.SurfacePoint(index,x,y));
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();float unit=Mathf.Max(.65f,rectTransform.rect.width/330f);
            if(demo){DrawLinked(mesh,new Vector2(rectTransform.rect.xMin+rectTransform.rect.width*.25f,rectTransform.rect.center.y),
                new Vector2(rectTransform.rect.xMin+rectTransform.rect.width*.75f,rectTransform.rect.center.y),unit);return;}
            if(game==null||game.VisibleFrame==null||!game.VisibleFrame.FloorVisible(index)||game.Machine==null||!game.Machine.HasFloorView(index,rectTransform.parent as RectTransform))return;
            foreach(int edge in floor.FragileEdges)DrawCrack(mesh,.045f+.91f*(edge+1f)/count,unit);
            var state=game.VisibleFrame;
            for(int edge=0;edge<floor.OpenEdges.Count;edge++){
                bool active=state.ActiveFloor==index;
                bool open=active?state.EdgeVisibleOpen(floor,edge):floor.OpenEdges[edge];
                if(floor.OneUseBridgeEdge==edge)DrawLatchFoot(mesh,Surface(.045f+.91f*(edge+1f)/count,.32f),unit,!open,true);
                // Branch stops already show the blocked gap. No redundant padlock badge.
            }
            if(floor.LinkedPair.HasValue){var pair=floor.LinkedPair.Value;
                DrawLinked(mesh,Surface(.045f+.91f*(pair.FirstEdge+1f)/count,.27f),
                    Surface(.045f+.91f*(pair.SecondEdge+1f)/count,.27f),unit);
            }
            if(floor.EntryTransform==EntryTransform.MirrorNext){
                DrawArrow(mesh,Surface(.23f,.31f),1,unit,false);DrawArrow(mesh,Surface(.77f,.31f),-1,unit,false);
            }else if(floor.EntryTransform!=EntryTransform.None){
                int direction=floor.EntryTransform==EntryTransform.ShiftLeftOne?-1:1;
                DrawArrow(mesh,Surface(.23f,.31f),direction,unit,true);DrawArrow(mesh,Surface(.77f,.31f),direction,unit,true);
            }
            if(floor.GustAfterSwaps>0){
                float y=floor.EntryTransform==EntryTransform.None?.31f:.19f;
                DrawWind(mesh,Surface(.18f,y),unit);DrawWind(mesh,Surface(.82f,y),unit);
            }
        }
        void DrawCrack(VertexHelper mesh,float x,float u)
        {
            // Irregular grain separation stays inside the timber silhouette, with small exposed fibres.
            var points=new[]{Surface(x-.010f,.397f),Surface(x+.002f,.363f),Surface(x-.004f,.340f),
                Surface(x+.009f,.306f),Surface(x-.005f,.276f),Surface(x+.002f,.246f),Surface(x-.007f,.211f)};
            var grain=new Color(.91f,.66f,.34f);
            for(int i=1;i<points.Length;i++){
                var a=points[i-1];var b=points[i];
                Line(mesh,a+Vector2.right*2*u,b+Vector2.right*2*u,2.3f*u,grain);
                Line(mesh,a,b,(i%2==0?3.5f:4.5f)*u,dark);
            }
            Line(mesh,points[2],Surface(x-.035f,.357f),2.2f*u,dark);
            Line(mesh,points[3],Surface(x+.043f,.337f),2.5f*u,dark);
            Line(mesh,points[4],Surface(x-.031f,.244f),1.6f*u,dark);
            Line(mesh,Surface(x+.024f,.323f),Surface(x+.048f,.342f),1.3f*u,grain);
            Line(mesh,Surface(x-.022f,.258f),Surface(x-.042f,.242f),1.3f*u,grain);
        }
        void DrawLatchFoot(VertexHelper mesh,Vector2 p,float u,bool closed,bool once)
        {
            // A brass socket around the actual wooden obstruction, never a floating lock badge.
            Line(mesh,p+Vector2.left*10*u,p+Vector2.right*10*u,13*u,dark);
            Line(mesh,p+Vector2.left*8*u,p+Vector2.right*8*u,10*u,hemp);
            Line(mesh,p+new Vector2(-8,4)*u,p+new Vector2(8,4)*u,1.5f*u,edgeLight);
            for(int side=-1;side<=1;side+=2){Disc(mesh,p+Vector2.right*side*8*u,1.5f*u,dark);Disc(mesh,p+new Vector2(side*8,1)*u,.65f*u,edgeLight);}
            var ink=closed?dark:new Color(.055f,.36f,.29f);
            if(once){Disc(mesh,p,3.1f*u,ink);Disc(mesh,p+Vector2.up*.7f*u,1.4f*u,edgeLight);}
            else if(closed){Disc(mesh,p+Vector2.up*1.5f*u,2.2f*u,ink);Line(mesh,p,p+Vector2.down*3.5f*u,2.6f*u,ink);}
            else{Line(mesh,p+new Vector2(-3,0)*u,p+new Vector2(-.5f,-2)*u,2*u,ink);Line(mesh,p+new Vector2(-.5f,-2)*u,p+new Vector2(4,3)*u,2*u,ink);}
        }
        void DrawArrow(VertexHelper mesh,Vector2 p,int direction,float u,bool loop)
        {
            Vector2 Tilt(Vector2 q)=>p+(Vector2)(Quaternion.Euler(0,0,game.Machine.FloorVisualAngle(index))*q)*u;
            var a=Tilt(new Vector2(-direction*13,0));var b=Tilt(new Vector2(direction*13,0));
            var up=Tilt(new Vector2(direction*7,4));var down=Tilt(new Vector2(direction*7,-4));
            Line(mesh,a,b,5.5f*u,dark);Line(mesh,b,up,5.5f*u,dark);Line(mesh,b,down,5.5f*u,dark);
            Line(mesh,a,b,2.6f*u,edgeLight);Line(mesh,b,up,2.6f*u,edgeLight);Line(mesh,b,down,2.6f*u,edgeLight);
            if(loop){
                var end=Tilt(new Vector2(-direction*13,-5));var turn=Tilt(new Vector2(-direction*6,-5));
                Line(mesh,a,end,3.6f*u,dark);Line(mesh,end,turn,3.6f*u,dark);
                Line(mesh,a,end,1.6f*u,edgeLight);Line(mesh,end,turn,1.6f*u,edgeLight);
            }
        }
        void DrawWind(VertexHelper mesh,Vector2 p,float u)
        {
            var ink=new Color(.035f,.29f,.32f);
            for(int strand=0;strand<2;strand++){
                var previous=p+new Vector2(-12,-strand*4)*u;
                for(int j=1;j<=16;j++){
                    float t=j/16f;var next=p+new Vector2(-12+24*t,Mathf.Sin(t*Mathf.PI*1.3f)*3-strand*4)*u;
                    Line(mesh,previous,next,4.2f*u,edgeLight);Line(mesh,previous,next,2*u,ink);previous=next;
                }
            }
        }
        void DrawLinked(VertexHelper mesh,Vector2 a,Vector2 b,float unit)
        {
            // Two matching pulleys, joined by one slack hemp belt under the face.
            Vector2 previous=a;
            for(int i=1;i<=24;i++){
                float t=i/24f;var p=Vector2.Lerp(a,b,t)+Vector2.down*(Mathf.Sin(t*Mathf.PI)*14*unit);
                Line(mesh,previous,p,5.5f*unit,dark);Line(mesh,previous+Vector2.up*unit,p+Vector2.up*unit,3.2f*unit,hemp);
                if(i%2==0)Line(mesh,p+new Vector2(-1,-2)*unit,p+new Vector2(1,1)*unit,1*unit,edgeLight);
                previous=p;
            }
            Pulley(mesh,a,unit);Pulley(mesh,b,unit);
        }
        void Pulley(VertexHelper mesh,Vector2 p,float u)
        {
            Disc(mesh,p+Vector2.down*u,11*u,dark);Disc(mesh,p,9.5f*u,hemp);Disc(mesh,p+Vector2.up*u,7.5f*u,edgeLight);
            Disc(mesh,p,5.5f*u,dark);
            for(int spoke=0;spoke<4;spoke++){
                float angle=spoke*Mathf.PI*.5f;var axis=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                Line(mesh,p+axis*2*u,p+axis*6*u,2*u,hemp);
            }
            Disc(mesh,p,2.5f*u,hemp);Disc(mesh,p+Vector2.up*.6f*u,1.2f*u,edgeLight);
            // Opposing carved arrowheads show that the neighbouring pieces exchange.
            for(int side=-1;side<=1;side+=2){
                var tip=p+Vector2.right*side*15*u;
                Line(mesh,p+Vector2.right*side*9*u,tip,2*u,edgeLight);
                Line(mesh,tip,tip+new Vector2(-side*3,3)*u,2*u,edgeLight);
                Line(mesh,tip,tip+new Vector2(-side*3,-3)*u,2*u,edgeLight);
            }
        }
        static void Line(VertexHelper mesh,Vector2 a,Vector2 b,float width,Color color)
        {
            var d=b-a;var n=new Vector2(-d.y,d.x).normalized*width*.5f;int k=mesh.currentVertCount;
            mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(a+n,color,Vector2.zero);
            mesh.AddVert(b+n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);
            mesh.AddTriangle(k,k+1,k+2);mesh.AddTriangle(k,k+2,k+3);
        }
        static void Disc(VertexHelper mesh,Vector2 p,float radius,Color color)
        {
            int k=mesh.currentVertCount;mesh.AddVert(p,color,Vector2.zero);
            for(int i=0;i<=24;i++){float a=i*Mathf.PI/12;mesh.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,color,Vector2.zero);}
            for(int i=0;i<24;i++)mesh.AddTriangle(k,k+i+1,k+i+2);
        }
    }
}
