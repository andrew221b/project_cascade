using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // The magnifier uses the same authored silhouette as the world mesh.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ShapeFaceGraphic : MaskableGraphic
    {
        [SerializeField] Mesh source;
        [SerializeField] bool polished;
        public void SetShape(Mesh mesh, Color shade,bool gloss=false) { if(source==mesh&&color==shade&&polished==gloss)return;source = mesh; color = shade;polished=gloss; raycastTarget = false; SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear(); if (source == null) return;
            var vertices = source.vertices; int count = (vertices.Length - 2) / 5;
            var bounds = source.bounds; var rect = rectTransform.rect;
            float scale = Mathf.Min(rect.width / bounds.size.x, rect.height / bounds.size.y) * .88f;
            if(polished){
                Fan(.99f,new Vector2(1,-2),new Color(.08f,.18f,.17f,color.a*.18f),false);
                Fan(1,Vector2.zero,Color.Lerp(color,new Color(.09f,.22f,.22f,color.a),.30f),false);
                Fan(.88f,Vector2.up*Mathf.Min(1,rect.height*.025f),color,true);return;
            }
            helper.AddVert(rect.center, color, Vector2.zero);
            for (int i = 0; i < count; i++) helper.AddVert(rect.center + (Vector2)vertices[count * 2 + i] * scale, color, Vector2.zero);
            for (int i = 0; i < count; i++) helper.AddTriangle(0, i + 1, (i + 1) % count + 1);
            void Fan(float inset,Vector2 offset,Color shade,bool lit){
                int first=helper.currentVertCount;helper.AddVert(rect.center+offset,lit?Color.Lerp(shade,Color.white,.14f):shade,Vector2.zero);
                for(int i=0;i<count;i++){
                    var p=(Vector2)vertices[count*2+i];float light=Mathf.Clamp01(.35f+p.y/bounds.size.y-p.x/bounds.size.x);
                    var c=lit?Color.Lerp(shade,new Color(1,1,.96f,shade.a),light*.48f):shade;
                    helper.AddVert(rect.center+offset+p*scale*inset,c,Vector2.zero);
                }
                for(int i=0;i<count;i++)helper.AddTriangle(first,first+i+1,first+(i+1)%count+1);
            }
        }
    }
}
