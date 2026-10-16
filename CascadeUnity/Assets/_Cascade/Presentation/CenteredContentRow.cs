using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    // Four logical UI pixels between painted content; the complete group owns
    // the centre, rather than independently centred icon and caption columns.
    public sealed class CenteredContentRow:MonoBehaviour
    {
        public const float Gap=4;
        public float Padding{get;set;}=12;
        RectTransform surface;RectTransform[] elements;Vector2[] sizes;TextMeshProUGUI[] labels;
        int previous;bool refreshing,vertical;float[] widths,heights;Vector2[] inkCenters;
        public RectTransform[] Elements=>elements;
        public RectTransform Surface=>surface;
        public bool Vertical=>vertical;
        public float LocalUnit{get;private set;}=1;
        public void Initialize(RectTransform area,params RectTransform[] content)
        {
            surface=area;elements=content;sizes=new Vector2[content.Length];labels=new TextMeshProUGUI[content.Length];
            for(int i=0;i<content.Length;i++){
                sizes[i]=content[i].rect.size;labels[i]=content[i].GetComponent<TextMeshProUGUI>();
                content[i].SetParent(area,false);
            }
            widths=new float[content.Length];heights=new float[content.Length];inkCenters=new Vector2[content.Length];previous=0;Refresh();
        }
        public void InitializeVertical(RectTransform area,params RectTransform[] content){vertical=true;Initialize(area,content);}
        public static float Unit(RectTransform area)
        {
            var canvas=area.GetComponentInParent<Canvas>();if(canvas==null)return 1;
            var camera=canvas.rootCanvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.rootCanvas.worldCamera;
            var a=RectTransformUtility.WorldToScreenPoint(camera,area.TransformPoint(Vector3.zero));
            var b=RectTransformUtility.WorldToScreenPoint(camera,area.TransformPoint(Vector3.right));
            return canvas.rootCanvas.scaleFactor/Mathf.Max(.001f,Vector2.Distance(a,b));
        }
        void LateUpdate()=>Refresh();
        public void Refresh()
        {
            if(refreshing||surface==null||elements==null||surface.rect.width<=0)return;
            float observedUnit=Unit(surface);bool dirty=false;for(int i=0;i<labels.Length;i++)if(labels[i]!=null&&(labels[i].havePropertiesChanged||labels[i].margin!=Vector4.zero||labels[i].textWrappingMode!=TextWrappingModes.NoWrap||labels[i].enableAutoSizing||labels[i].alignment!=TextAlignmentOptions.Center))dirty=true;
            int key=surface.rect.size.GetHashCode()+Mathf.RoundToInt(observedUnit*1000);unchecked{
                key=key*31+Mathf.RoundToInt(surface.lossyScale.x*1000)+Mathf.RoundToInt(surface.lossyScale.y*1000);
                var canvas=surface.GetComponentInParent<Canvas>();key=key*31+(canvas==null?0:canvas.scaleFactor.GetHashCode());
                for(int i=0;i<labels.Length;i++)if(labels[i]!=null)key=key*31+(labels[i].text??"").GetHashCode()+labels[i].fontSizeMax.GetHashCode()+labels[i].characterSpacing.GetHashCode()+labels[i].font.GetInstanceID();
            }
            if(key==previous&&!dirty)return;previous=key;refreshing=true;
            try{
                LocalUnit=observedUnit;float gap=Gap*LocalUnit,padding=Padding*LocalUnit;
                float available=Mathf.Max(1,surface.rect.width-2*padding),fixedWidth=0,textWidth=0;
                System.Array.Clear(inkCenters,0,inkCenters.Length);
                for(int i=0;i<elements.Length;i++){
                    var label=labels[i];
                    if(label!=null){
                        label.enableAutoSizing=false;label.fontSize=label.fontSizeMax;label.margin=Vector4.zero;
                        label.textWrappingMode=TextWrappingModes.NoWrap;label.alignment=TextAlignmentOptions.Center;label.overflowMode=TextOverflowModes.Overflow;
                        widths[i]=label.GetPreferredValues(label.text,Mathf.Infinity,Mathf.Infinity).x;textWidth+=widths[i];
                    }else{
                        var size=sizes[i];var image=elements[i].GetComponent<UnityEngine.UI.Image>();
                        if(image!=null&&image.preserveAspect&&image.sprite!=null){float aspect=image.sprite.rect.width/image.sprite.rect.height;float h=Mathf.Min(size.y,size.x/aspect);size=new Vector2(h*aspect,h);}
                        float fit=Mathf.Min(1,(surface.rect.height-12*LocalUnit)/Mathf.Max(1,size.y));
                        widths[i]=size.x*fit;heights[i]=size.y*fit;fixedWidth+=widths[i];
                    }
                }
                float maxText=0;for(int i=0;i<labels.Length;i++)if(labels[i]!=null)maxText=Mathf.Max(maxText,widths[i]);
                float textFit=vertical?Mathf.Clamp01(available/Mathf.Max(1,maxText)):Mathf.Clamp01((available-fixedWidth-gap*(elements.Length-1))/Mathf.Max(1,textWidth));
                float total=gap*(elements.Length-1);
                for(int i=0;i<elements.Length;i++){
                    var item=elements[i];var label=labels[i];item.anchorMin=item.anchorMax=item.pivot=Vector2.one*.5f;
                    if(label!=null){
                        label.fontSize=Mathf.Clamp(label.fontSizeMax*textFit,label.fontSizeMin,label.fontSizeMax);
                        float width=label.GetPreferredValues(label.text,Mathf.Infinity,Mathf.Infinity).x+1;
                        float preferredHeight=label.GetPreferredValues(label.text,Mathf.Infinity,Mathf.Infinity).y;item.sizeDelta=new Vector2(width,Mathf.Max(surface.rect.height,preferredHeight+2));
                        label.ForceMeshUpdate();var bounds=label.textBounds;
                        bool valid=bounds.size.x>=0&&bounds.size.y>=0&&!float.IsNaN(bounds.size.x)&&!float.IsInfinity(bounds.size.x);
                        widths[i]=valid?bounds.size.x:width;heights[i]=valid?bounds.size.y:label.fontSize;inkCenters[i]=valid?(Vector2)bounds.center:Vector2.zero;
                    }else item.sizeDelta=new Vector2(widths[i],heights[i]);
                    total+=vertical?heights[i]:widths[i];
                }
                float x=-total*.5f;
                for(int i=0;i<elements.Length;i++){
                    elements[i].anchoredPosition=(vertical?new Vector2(0,-x-heights[i]*.5f):new Vector2(x+widths[i]*.5f,0))-inkCenters[i];x+=(vertical?heights[i]:widths[i])+gap;
                }
            }finally{refreshing=false;}
        }
    }
}
