using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Keep the existing paving and upper platform at the frame's width scale.
    // Only the plain timber between them absorbs the phone's extra bottom inset.
    [RequireComponent(typeof(RawImage))]
    public sealed class HomeGroundMesh:BaseMeshEffect
    {
        public override void ModifyMesh(VertexHelper vertices)
        {
            if(!IsActive())return;
            var image=(RawImage)graphic;if(image.texture==null)return;
            var area=image.rectTransform.rect;var uv=image.uvRect;
            float scale=area.width/image.texture.width;
            float lower=180*scale,upper=90*scale;
            float fit=Mathf.Min(1,area.height/Mathf.Max(.001f,lower+upper));lower*=fit;upper*=fit;
            vertices.Clear();
            Band(vertices,area,uv,0,lower,0,180f/image.texture.height);
            Band(vertices,area,uv,lower,area.height-upper,180f/image.texture.height,1-90f/image.texture.height);
            Band(vertices,area,uv,area.height-upper,area.height,1-90f/image.texture.height,1);
        }
        void Band(VertexHelper vertices,Rect area,Rect uv,float bottom,float top,float v0,float v1)
        {
            int index=vertices.currentVertCount;var vertex=UIVertex.simpleVert;vertex.color=graphic.color;
            vertex.position=new Vector3(area.xMin,area.yMin+bottom);vertex.uv0=new Vector2(uv.xMin,uv.yMin+uv.height*v0);vertices.AddVert(vertex);
            vertex.position=new Vector3(area.xMin,area.yMin+top);vertex.uv0=new Vector2(uv.xMin,uv.yMin+uv.height*v1);vertices.AddVert(vertex);
            vertex.position=new Vector3(area.xMax,area.yMin+top);vertex.uv0=new Vector2(uv.xMax,uv.yMin+uv.height*v1);vertices.AddVert(vertex);
            vertex.position=new Vector3(area.xMax,area.yMin+bottom);vertex.uv0=new Vector2(uv.xMax,uv.yMin+uv.height*v0);vertices.AddVert(vertex);
            vertices.AddTriangle(index,index+1,index+2);vertices.AddTriangle(index,index+2,index+3);
        }
    }
}
