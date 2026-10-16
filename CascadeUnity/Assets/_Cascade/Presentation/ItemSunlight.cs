using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    // Mesh-based soft rays: no fullscreen filter, texture allocations or item tint.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ItemSunlight:MaskableGraphic
    {
        CascadeGame game;float clock;
#if UNITY_EDITOR
        bool review;public void SetReviewAge(float age){review=true;clock=age;SetVerticesDirty();}public void EndReview(){review=false;}
#endif
        public float AnimationAge=>clock;
        public void Initialize(CascadeGame owner){game=owner;raycastTarget=false;SetVerticesDirty();}
        void LateUpdate(){
#if UNITY_EDITOR
            if(review)return;
#endif
            if(game!=null&&!game.ReducedMotion&&game.Preferences.EffectQuality>0&&game.CosmeticAmbianceMotionAllowed){clock+=Mathf.Min(Time.unscaledDeltaTime,.05f);SetVerticesDirty();}}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=rectTransform.rect;Vector2 center=rect.center;
            float phase=game!=null&&game.ReducedMotion?0:clock,pulse=1+.05f*Mathf.Sin(phase*.7f);
            const int rays=128,rings=12;
            for(int ring=0;ring<=rings;ring++)for(int i=0;i<=rays;i++){
                float p=ring/(float)rings,a=i*Mathf.PI*2/rays;
                float ray=Mathf.Pow(Mathf.Max(0,Mathf.Cos(a*17+phase*.22f+Mathf.Sin(a*3)*.7f)),6);
                float alpha=Mathf.Pow(1-p,.9f)*(.95f+ray*p*1.7f)*pulse;
                vh.AddVert(center+new Vector2(Mathf.Cos(a)*rect.width*.5f,Mathf.Sin(a)*rect.height*.5f)*p,new Color(1,.94f,.56f,alpha),Vector2.zero);
            }
            for(int r=0;r<rings;r++)for(int i=0;i<rays;i++){int k=r*(rays+1)+i;vh.AddTriangle(k,k+rays+1,k+1);vh.AddTriangle(k+1,k+rays+1,k+rays+2);}
        }
    }
}
