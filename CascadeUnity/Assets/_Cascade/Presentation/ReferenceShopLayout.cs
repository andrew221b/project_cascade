using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Cascade.Presentation
{
    // All controls keep the reference's 720x1280 geometry and one uniform scale.
    // The wooden border fills the viewport independently; it never stretches icons.
    public sealed class ReferenceShopLayout : MonoBehaviour
    {
        RectTransform surface,content,backdrop,canvas;
        RawImage continuousGround;
        float groundTop;
        float compactGroundFraction;
        RectTransform landingControls;
        readonly Vector3[] groundCorners=new Vector3[4];
        readonly List<KeyValuePair<RectTransform,Vector2>> touches=new List<KeyValuePair<RectTransform,Vector2>>();
        public RectTransform Backdrop=>backdrop;
        public void BindTouch(RectTransform rect,Vector2 artSize)=>touches.Add(new KeyValuePair<RectTransform,Vector2>(rect,artSize));
        public void Initialize(RectTransform parent,RectTransform frame)
        {
            surface=parent;content=(RectTransform)transform;backdrop=frame;
            canvas=(RectTransform)parent.GetComponentInParent<Canvas>().transform;Fit();
        }
        public void BindContinuousGround(float top=1090,string asset="ContinuousGround")
        {
            groundTop=top;compactGroundFraction=0;
            if(continuousGround==null){
                var go=new GameObject("Continuous menu floor",typeof(RectTransform),typeof(RawImage));
                go.transform.SetParent(content,false);go.transform.SetAsFirstSibling();
                continuousGround=go.GetComponent<RawImage>();
            }
            continuousGround.texture=Resources.Load<Texture2D>(WorldFrameArt.Path+asset);
            continuousGround.raycastTarget=false;
            var rect=continuousGround.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);Fit();
        }
        // Maps keep a short landing at the physical bottom instead of filling the
        // whole letterbox below the reference composition with paving.
        public void BindCompactGround(float viewportFraction)
        {
            BindContinuousGround();compactGroundFraction=Mathf.Clamp(viewportFraction,.01f,.15f);Fit();
        }
        public void BindLandingControls(RectTransform controls){landingControls=controls;Fit();}
        public void Fit()
        {
            if(surface==null||canvas==null)return;
            float scale=Mathf.Max(.001f,Mathf.Min(surface.rect.width/720f,surface.rect.height/1280f));
            content.sizeDelta=new Vector2(720,1280);content.localScale=Vector3.one*scale;
            backdrop.localScale=Vector3.one*scale;backdrop.sizeDelta=canvas.rect.size/scale;
            backdrop.position=canvas.position;
            float pixelScale=scale*canvas.GetComponent<Canvas>().scaleFactor;
            float minimum=44/Mathf.Max(.001f,pixelScale);
            if(continuousGround!=null&&continuousGround.texture!=null){
                canvas.GetWorldCorners(groundCorners);
                var low=content.InverseTransformPoint(groundCorners[0]);var high=content.InverseTransformPoint(groundCorners[2]);
                float height=Mathf.Max(0,content.rect.yMax-groundTop-low.y),width=high.x-low.x;
                if(compactGroundFraction>0)height=(high.y-low.y)*compactGroundFraction;
                var rect=continuousGround.rectTransform;rect.anchoredPosition=new Vector2(low.x-content.rect.xMin,low.y+height-content.rect.yMax);rect.sizeDelta=new Vector2(width,height);
                // A single camera-facing surface reaches the physical bottom. Crop from its top
                // at uniform scale, rather than repeating the last rows below the safe area.
                float rows=Mathf.Min(1,height/Mathf.Max(1,width)*continuousGround.texture.width/continuousGround.texture.height);
                continuousGround.uvRect=new Rect(0,1-rows,1,rows);continuousGround.enabled=height>.01f;
                if(landingControls!=null){
                    surface.GetWorldCorners(groundCorners);
                    float safeBottom=content.InverseTransformPoint(groundCorners[0]).y;
                    // The button's painted lower edge meets the brick landing. Keep
                    // the entire control above the home indicator on tall phones.
                    float bottom=Mathf.Max(low.y+height-12,safeBottom+12);
                    landingControls.anchoredPosition=new Vector2(0,bottom+landingControls.rect.height-content.rect.yMax);
                }
            }
            foreach(var touch in touches)if(touch.Key!=null)touch.Key.sizeDelta=new Vector2(Mathf.Max(GameButtonSize(touch.Key,touch.Value).x,minimum),Mathf.Max(GameButtonSize(touch.Key,touch.Value).y,minimum));
        }
        static Vector2 GameButtonSize(RectTransform target,Vector2 authored){var design=target.GetComponent<GameButtonDesign>();return design==null?authored:design.DesiredSize(authored);}
        void LateUpdate()=>Fit();
    }
}
