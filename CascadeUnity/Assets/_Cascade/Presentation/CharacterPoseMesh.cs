using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Cache topology/landmark weights. Only positions change while a pose lives.
    public sealed class CharacterPoseMesh:BaseMeshEffect
    {
        CharacterPoseView owner;readonly CharacterPoseGeometry geometry=new CharacterPoseGeometry();
        readonly UIVertex[] corners=new UIVertex[4];
        public Vector2 Low{get;private set;}public Vector2 High{get;private set;}
        public Vector2 UvLow{get;private set;}public Vector2 UvHigh{get;private set;}
        public bool HasBounds{get;private set;}
        public void Initialize(CharacterPoseView view){owner=view;graphic.SetVerticesDirty();}
        public void Invalidate(){geometry.Invalidate();HasBounds=false;graphic.SetVerticesDirty();}
        public override void ModifyMesh(VertexHelper h)
        {
            if(!IsActive()||owner==null||owner.Profile==null||h.currentVertCount==0)return;
            Low=UvLow=Vector2.one*float.PositiveInfinity;High=UvHigh=Vector2.one*float.NegativeInfinity;
            for(int i=0;i<h.currentVertCount;i++){
                var v=UIVertex.simpleVert;h.PopulateUIVertex(ref v,i);
                Low=Vector2.Min(Low,v.position);High=Vector2.Max(High,v.position);UvLow=Vector2.Min(UvLow,v.uv0);UvHigh=Vector2.Max(UvHigh,v.uv0);
            }
            HasBounds=true;geometry.Build(h,owner,this);
        }
    }
    public sealed class CharacterPoseGeometry
    {
        readonly List<UIVertex> stream=new List<UIVertex>();readonly List<int> triangles=new List<int>();
        CharacterPoseMotion.Binding[] bindings;Vector2[] uv;int columns;Sprite sprite;
        public void Invalidate(){sprite=null;}
        public void Build(VertexHelper h,CharacterPoseView owner,CharacterPoseMesh bounds)
        {
            h.Clear();if(!bounds.HasBounds||owner.Profile==null)return;
            int count=owner.Quality>1?36:18,rows=Mathf.Clamp(Mathf.RoundToInt(count*owner.SourceSprite.rect.height/owner.SourceSprite.rect.width),count,64);
            if(sprite!=owner.SourceSprite||columns!=count||bindings==null||bindings.Length!=(count+1)*(rows+1)){
                sprite=owner.SourceSprite;columns=count;bindings=new CharacterPoseMotion.Binding[(count+1)*(rows+1)];uv=new Vector2[bindings.Length];
                stream.Clear();if(stream.Capacity<bindings.Length)stream.Capacity=bindings.Length;
                for(int y=0;y<=rows;y++)for(int x=0;x<=count;x++){
                    int i=y*(count+1)+x;uv[i]=new Vector2(Mathf.Lerp(bounds.UvLow.x,bounds.UvHigh.x,x/(float)count),Mathf.Lerp(bounds.UvLow.y,bounds.UvHigh.y,y/(float)rows));
                    bindings[i]=new CharacterPoseMotion.Binding(owner.Profile.Point(sprite,uv[i]),owner.Profile);stream.Add(UIVertex.simpleVert);
                }
                triangles.Clear();if(triangles.Capacity<count*rows*6)triangles.Capacity=count*rows*6;
                for(int y=0;y<rows;y++)for(int x=0;x<count;x++){int a=y*(count+1)+x,b=a+count+1;triangles.Add(a);triangles.Add(b);triangles.Add(b+1);triangles.Add(a);triangles.Add(b+1);triangles.Add(a+1);}
            }
            var dimensions=owner.SourceSprite.texture;var cell=owner.Profile.Cell;
            var uvScale=new Vector2((bounds.High.x-bounds.Low.x)/(bounds.UvHigh.x-bounds.UvLow.x),(bounds.High.y-bounds.Low.y)/(bounds.UvHigh.y-bounds.UvLow.y));
            var pixelScale=new Vector2(uvScale.x/dimensions.width,uvScale.y/dimensions.height);
            var offset=new Vector2(bounds.Low.x+(cell.x/dimensions.width-bounds.UvLow.x)*uvScale.x,bounds.Low.y+(1-cell.y/dimensions.height-bounds.UvLow.y)*uvScale.y);
            var vertex=UIVertex.simpleVert;vertex.color=owner.Hero.color;
            for(int i=0;i<bindings.Length;i++){
                Vector2 p=owner.MotionActive?owner.Motion.Deform(bindings[i]):bindings[i].Source;
                vertex.position=new Vector3(offset.x+p.x*cell.width*pixelScale.x,offset.y-p.y*cell.height*pixelScale.y);vertex.uv0=uv[i];stream[i]=vertex;
            }
            h.AddUIVertexStream(stream,triangles);
        }
    }
    public sealed class CharacterArtPart:MaskableGraphic
    {
        CharacterPoseView owner;readonly CharacterPoseGeometry geometry=new CharacterPoseGeometry();
        public override Texture mainTexture=>owner==null||owner.SourceSprite==null?s_WhiteTexture:owner.SourceSprite.texture;
        public void Initialize(CharacterPoseView view,Material appearance){owner=view;material=appearance;raycastTarget=false;SetAllDirty();}
        public void Invalidate(){geometry.Invalidate();SetAllDirty();}
        protected override void OnPopulateMesh(VertexHelper h)
        {
            if(owner==null){h.Clear();return;}
            h.Clear();if(!owner.Mesh.HasBounds||owner.Profile==null)return;
            var bounds=owner.Mesh;var vertex=UIVertex.simpleVert;vertex.color=owner.Hero.color;
            var size=bounds.High-bounds.Low;var uvScale=new Vector2(size.x/(bounds.UvHigh.x-bounds.UvLow.x),size.y/(bounds.UvHigh.y-bounds.UvLow.y));
            for(int i=0;i<4;i++){
                bool right=i>=2,top=i==1||i==2;var source=new Vector2(right?.995f:.735f,top?.23f:.69f);var posed=owner.MotionActive?owner.Motion.DeformLantern(source):source;
                var uv=owner.Profile.Uv(owner.SourceSprite,posed);vertex.position=bounds.Low+Vector2.Scale(uv-bounds.UvLow,uvScale);vertex.uv0=owner.Profile.Uv(owner.SourceSprite,source);h.AddVert(vertex);
            }
            h.AddTriangle(0,1,2);h.AddTriangle(0,2,3);
        }
    }
}
