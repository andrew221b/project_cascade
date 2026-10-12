using UnityEngine;
namespace Cascade.Presentation
{
    // A sibling behind TMP, never a child over its glyph mesh. Tracks responsive text bounds.
    public sealed class ReadingPaperLayout : MonoBehaviour
    {
        RectTransform label, paper; Canvas canvas;
        public void Initialize(RectTransform target){label=target;paper=(RectTransform)transform;canvas=target.GetComponentInParent<Canvas>();Fit();}
        void OnEnable()=>Canvas.willRenderCanvases+=Fit;
        void OnDisable()=>Canvas.willRenderCanvases-=Fit;
        void Fit(){
            if(label==null||paper==null||canvas==null)return;
            float u=1/Mathf.Min(1,canvas.scaleFactor);
            paper.anchorMin=label.anchorMin;paper.anchorMax=label.anchorMax;paper.pivot=label.pivot;
            paper.offsetMin=label.offsetMin-new Vector2(6,3)*u;paper.offsetMax=label.offsetMax+new Vector2(6,3)*u;
            paper.localScale=label.localScale;paper.localRotation=label.localRotation;
        }
    }
}
