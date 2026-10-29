using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Reuse the exact pixels of the original loose scarf. A separate mesh can
    // drape across the shoulder without dragging the original body with it.
    public sealed class WelcomeMikaScarf:MaskableGraphic
    {
        WelcomeMikaView owner;int cachedColumns;
        Vector2[] source;WelcomeSecondaryMotion.Binding[] bindings;
        readonly List<UIVertex> vertices=new List<UIVertex>();readonly List<int> triangles=new List<int>();
        public override Texture mainTexture=>owner!=null?owner.SourceSprite.texture:Texture2D.whiteTexture;
        public void Initialize(WelcomeMikaView view,Material clothMaterial){owner=view;material=clothMaterial;raycastTarget=false;SetVerticesDirty();}
        public static bool ContainsSource(Vector2 p)
        {
            float top=443+Mathf.Max(p.x-220,0)*.6f+Mathf.Max(p.x-320,0)*.18f-Mathf.Max(p.x-380,0)*1.08f;
            float bottom=620+Mathf.Max(p.x-210,0)*.5f-Mathf.Max(p.x-265,0)*.86f-Mathf.Max(p.x-380,0)*.04f;
            return p.x>=95&&p.x<=418&&p.y>=top&&p.y<=bottom;
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();if(owner==null||!owner.UsesSourceArt)return;
            int columns=owner.Quality>1?32:16,rows=columns/2;
            if(source==null||cachedColumns!=columns){
                cachedColumns=columns;source=new Vector2[(columns+1)*(rows+1)];bindings=new WelcomeSecondaryMotion.Binding[source.Length];vertices.Clear();triangles.Clear();
                if(vertices.Capacity<source.Length)vertices.Capacity=source.Length;
                for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++){
                    int i=y*(columns+1)+x;source[i]=new Vector2(Mathf.Lerp(95,418,x/(float)columns),Mathf.Lerp(438,650,y/(float)rows));bindings[i]=WelcomeSecondaryMotion.BindScarf(source[i]);vertices.Add(UIVertex.simpleVert);
                }
                if(triangles.Capacity<columns*rows*6)triangles.Capacity=columns*rows*6;
                for(int y=0;y<rows;y++)for(int x=0;x<columns;x++){int a=y*(columns+1)+x,b=a+columns+1;triangles.Add(a);triangles.Add(a+1);triangles.Add(b+1);triangles.Add(a);triangles.Add(b+1);triangles.Add(b);}
            }
            var sprite=owner.SourceSprite;var area=rectTransform.rect;float scale=Mathf.Min(area.width/sprite.rect.width,area.height/sprite.rect.height);
            var centre=new Vector2(sprite.rect.center.x,sprite.texture.height-sprite.rect.center.y);var vertex=UIVertex.simpleVert;vertex.color=color;
            for(int i=0;i<source.Length;i++){
                var p=owner.SkinActive?owner.Motion.ScarfPoint(source[i],bindings[i]):WelcomeSecondaryMotion.RestingScarfPoint(source[i]);
                vertex.position=area.center+Vector2.Scale(p-centre,new Vector2(scale,-scale));vertex.uv0=new Vector2(source[i].x/sprite.texture.width,1-source[i].y/sprite.texture.height);vertices[i]=vertex;
            }
            mesh.AddUIVertexStream(vertices,triangles);
        }
#if UNITY_EDITOR
        public void BuildReviewMesh(VertexHelper mesh)=>OnPopulateMesh(mesh);
#endif
    }
}
