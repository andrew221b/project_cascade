using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    // One uniform reference scale; taller phones reveal more rows, not stretched cards.
    public sealed class IslandLevelLayout:MonoBehaviour
    {
        public const int EndOfListGap=96;
        RectTransform surface,stage,backdrop,canvas,viewport,bar;
        readonly Vector3[] corners=new Vector3[4];
        readonly List<KeyValuePair<RectTransform,Vector2>> touches=new List<KeyValuePair<RectTransform,Vector2>>();
        public void Initialize(RectTransform root,RectTransform background)
        {
            surface=root;stage=(RectTransform)transform;backdrop=background;
            canvas=(RectTransform)root.GetComponentInParent<Canvas>().rootCanvas.transform;Fit();
        }
        public void BindScroll(RectTransform view,RectTransform scrollbar){viewport=view;bar=scrollbar;Fit();}
        public void BindTouch(RectTransform rect,Vector2 size){touches.Add(new KeyValuePair<RectTransform,Vector2>(rect,size));Fit();}
        public void Fit()
        {
            if(surface==null||canvas==null)return;
            float scale=Mathf.Max(.001f,Mathf.Min(surface.rect.width/720,surface.rect.height/980));
            stage.localScale=Vector3.one*scale;stage.sizeDelta=new Vector2(720,surface.rect.height/scale);
            var image=backdrop.GetComponent<Image>();if(image!=null&&image.sprite!=null){
                float cover=Mathf.Max(canvas.rect.width/image.sprite.rect.width,canvas.rect.height/image.sprite.rect.height);
                backdrop.localScale=Vector3.one;backdrop.sizeDelta=image.sprite.rect.size*cover;backdrop.position=canvas.position;
            }
            float minimum=44/Mathf.Max(.001f,scale*canvas.GetComponent<Canvas>().scaleFactor);
            canvas.GetWorldCorners(corners);float extension=Mathf.Max(0,-stage.rect.height-stage.InverseTransformPoint(corners[0]).y);
            float h=Mathf.Max(100,stage.rect.height-390+extension);
            if(viewport!=null){
                viewport.sizeDelta=new Vector2(572,h);var scroll=viewport.GetComponent<ScrollRect>();
                if(scroll!=null&&scroll.content!=null){
                    var layout=scroll.content.GetComponent<VerticalLayoutGroup>();int padding=Mathf.CeilToInt(extension+EndOfListGap);
                    if(layout.padding.bottom!=padding){layout.padding.bottom=padding;scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,scroll.content.childCount*123-5+padding);LayoutRebuilder.MarkLayoutForRebuild(scroll.content);}
                }
            }
            if(bar!=null){if(viewport!=null){var scroll=viewport.GetComponent<ScrollRect>();bar.gameObject.SetActive(scroll!=null&&scroll.content.rect.height>h+.5f);}var position=bar.anchoredPosition;bar.sizeDelta=new Vector2(Mathf.Max(44,minimum),Mathf.Min(655,h-60));bar.anchoredPosition=new Vector2(position.x,-429-bar.rect.height*.5f);}
            foreach(var item in touches)if(item.Key!=null)item.Key.sizeDelta=new Vector2(Mathf.Max(GameButtonSize(item.Key,item.Value).x,minimum),Mathf.Max(GameButtonSize(item.Key,item.Value).y,minimum));
        }
        static Vector2 GameButtonSize(RectTransform target,Vector2 authored){var design=target.GetComponent<GameButtonDesign>();return design==null?authored:design.DesiredSize(authored);}
        void LateUpdate()=>Fit();
    }
}
