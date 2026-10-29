using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Skin the existing drawing in one UI batch; no detached silhouette seams.
    [RequireComponent(typeof(Image))]
    public sealed class WelcomeMikaMesh:BaseMeshEffect
    {
        WelcomeMikaView owner;
        readonly UIVertex[] quad=new UIVertex[4];
        WelcomeMikaMotion.SkinPoint[] bindings;Vector2[] coordinates;int cachedColumns;
        readonly List<UIVertex> stream=new List<UIVertex>();readonly List<int> triangles=new List<int>();
        public void Initialize(WelcomeMikaView view){owner=view;graphic.SetVerticesDirty();}
        public override void ModifyMesh(VertexHelper vertices)
        {
            if(!IsActive()||owner==null||!owner.SkinActive||vertices.currentVertCount!=4)return;
            for(int i=0;i<4;i++)vertices.PopulateUIVertex(ref quad[i],i);
            Vector2 low=quad[0].position,high=quad[2].position,uvLow=quad[0].uv0,uvHigh=quad[2].uv0;
            int columns=owner.Quality>1?48:24,rows=columns*3/2;
            var source=owner.SourceSprite.rect;var dimensions=owner.SourceSprite.texture;
            float textureWidth=dimensions.width,textureHeight=dimensions.height;
            if(bindings==null||cachedColumns!=columns){
                cachedColumns=columns;bindings=new WelcomeMikaMotion.SkinPoint[(columns+1)*(rows+1)];coordinates=new Vector2[bindings.Length];
                for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++){
                    int i=y*(columns+1)+x;var uv=new Vector2(Mathf.Lerp(uvLow.x,uvHigh.x,x/(float)columns),Mathf.Lerp(uvLow.y,uvHigh.y,y/(float)rows));
                    coordinates[i]=uv;bindings[i]=new WelcomeMikaMotion.SkinPoint(new Vector2(uv.x*textureWidth,(1-uv.y)*textureHeight));
                }
                stream.Clear();if(stream.Capacity<bindings.Length)stream.Capacity=bindings.Length;
                for(int i=0;i<bindings.Length;i++)stream.Add(UIVertex.simpleVert);
                triangles.Clear();if(triangles.Capacity<columns*rows*6)triangles.Capacity=columns*rows*6;
                for(int y=0;y<rows;y++)for(int x=0;x<columns;x++){
                    int a=y*(columns+1)+x,b=a+columns+1;triangles.Add(a);triangles.Add(b);triangles.Add(b+1);triangles.Add(a);triangles.Add(b+1);triangles.Add(a+1);
                }
            }
            vertices.Clear();var vertex=UIVertex.simpleVert;vertex.color=quad[0].color;
            var scale=new Vector2((high.x-low.x)/source.width,(high.y-low.y)/source.height);
            var offset=new Vector2(low.x-source.x*scale.x,low.y+(textureHeight-source.y)*scale.y);
            for(int i=0;i<bindings.Length;i++){
                var posed=owner.Motion.Deform(bindings[i]);vertex.position=new Vector3(offset.x+posed.x*scale.x,offset.y-posed.y*scale.y);vertex.uv0=coordinates[i];stream[i]=vertex;
            }
            // Preserve topology and send all vertex channels through uGUI's native
            // bulk path, avoiding thousands of managed AddVert/AddTriangle calls.
            vertices.AddUIVertexStream(stream,triangles);
        }
    }
}
