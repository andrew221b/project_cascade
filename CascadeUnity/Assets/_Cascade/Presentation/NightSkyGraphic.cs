using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Sparse light glints over authored constellations; never affect puzzle timing.
    public sealed class NightSkyGraphic : MaskableGraphic
    {
        CascadeGame game;
        public void Initialize(CascadeGame owner){game=owner;raycastTarget=false;}
        void Update(){if(game!=null&&!game.ReducedMotion&&game.Preferences.EffectQuality>0)SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(game==null||game.Preferences.EffectQuality==0)return;
            var r=rectTransform.rect;float phase=game.ReducedMotion?0:game.Machine.AmbientPhase;
            for(int i=0;i<16;i++){
                float x=Mathf.Repeat(i*.618f+.13f,1),y=.25f+Mathf.Repeat(i*.417f,.63f);
                float glow=.24f+.30f*(.5f+.5f*Mathf.Sin(phase*.65f+i*1.83f));
                var c=new Color(.87f,.95f,1,glow);float size=(i%3==0?2.4f:1.4f)*Mathf.Clamp(r.width/390f,.75f,1.8f);
                Vector2 p=new Vector2(r.x+x*r.width,r.y+y*r.height);
                Add(p+Vector2.left*size,p+Vector2.up*size*.32f,p+Vector2.right*size,p+Vector2.down*size*.32f,c);
                Add(p+Vector2.down*size,p+Vector2.left*size*.32f,p+Vector2.up*size,p+Vector2.right*size*.32f,c);
            }
            void Add(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color tint){int index=vh.currentVertCount;vh.AddVert(a,tint,Vector2.zero);vh.AddVert(b,tint,Vector2.zero);vh.AddVert(c,tint,Vector2.zero);vh.AddVert(d,tint,Vector2.zero);vh.AddTriangle(index,index+1,index+2);vh.AddTriangle(index,index+2,index+3);}
        }
    }
}
