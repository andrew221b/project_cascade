using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    // Four logical UI pixels between painted content; the complete group owns
    // the centre, rather than independently centred icon and caption columns.
    public sealed class CenteredContentRow:MonoBehaviour
    {
        public const float Gap=4;
        public const float MinimumInset=2;
        public float Padding{get;set;}=12;
        // Shared actions keep pictograms stable; long copy yields first.
        public bool FixedIconSize{get;set;}
        RectTransform surface;RectTransform[] elements;Vector2[] sizes,drawSizes;TextMeshProUGUI[] labels;PauseGlyphGraphic[] glyphs;
        int previous;bool refreshing,vertical;float[] widths,heights;Vector2[] inkCenters;
        public RectTransform[] Elements=>elements;
        public RectTransform Surface=>surface;
        public bool Vertical=>vertical;
        public float LocalUnit{get;private set;}=1;
        // Called once, after all reference-control children have been authored.
        // Explicit rows/columns retain ownership of composite cards and buttons.
        internal static void BindPlainButton(RectTransform surface)
        {
            if(surface.GetComponent<CenteredContentRow>()!=null)return;
            var content=new System.Collections.Generic.List<RectTransform>();TextMeshProUGUI caption=null;
            foreach(Transform child in surface){
                var text=child.GetComponent<TextMeshProUGUI>();
                if(text!=null){if(caption!=null||text.textWrappingMode!=TextWrappingModes.NoWrap)return;caption=text;content.Add(text.rectTransform);continue;}
                var image=child.GetComponent<UnityEngine.UI.Image>();if(image==null||image.sprite==null||image.color.a<=0)continue;
                var rect=image.rectTransform;
                bool background=child.name=="Reference shop action artwork"||rect.rect.width>=surface.rect.width*.72f&&rect.rect.height>=surface.rect.height*.65f;
                if(!background)content.Add(rect);
            }
            // A clickable settings/list/form row is not a centred action button.
            // Explicit leading/trailing text alignment always owns that layout.
            if(caption==null||caption.horizontalAlignment!=HorizontalAlignmentOptions.Center||content.Count>2)return;
            surface.gameObject.AddComponent<CenteredContentRow>().Initialize(surface,content.ToArray());
        }
        // TMP.textBounds uses font ascenders/descenders and advances, even for
        // letters without those strokes. Glyph bearings measure the drawn shapes
        // without empty line metrics or distance-field shader padding.
        public static Bounds GlyphBounds(TMP_Text label)
        {
            var low=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var high=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
            var info=label.textInfo;bool found=false;
            for(int i=0;i<info.characterCount;i++){
                var character=info.characterInfo[i];if(!character.isVisible||character.textElement?.glyph==null||character.scale<=0)continue;
                var metrics=character.textElement.glyph.metrics;float scale=character.scale;
                if(metrics.width<=0||metrics.height<=0)continue;
                float left=character.origin+metrics.horizontalBearingX*scale,top=character.baseLine+metrics.horizontalBearingY*scale;
                low=Vector2.Min(low,new Vector2(left,top-metrics.height*scale));high=Vector2.Max(high,new Vector2(left+metrics.width*scale,top));found=true;
            }
            return found?new Bounds((low+high)*.5f,high-low):new Bounds();
        }
        public static void CenterWrappedText(TextMeshProUGUI label,RectTransform area)
        {
            // Centre the visible paragraph as a whole; retain left-aligned lines
            // inside that block so a multi-line conversation remains easy to read.
            label.ForceMeshUpdate();var bounds=GlyphBounds(label);
            if(bounds.size.y<=0||bounds.size.y>area.rect.height+.5f||float.IsNaN(bounds.center.x))return;
            var target=area.TransformPoint(area.rect.center);var ink=label.transform.TransformPoint(bounds.center);
            var delta=label.transform.parent.InverseTransformVector(target-ink);
            label.rectTransform.anchoredPosition+=(Vector2)delta;
        }
        // Authored leaves, ornaments and suspension ropes are outside the writing
        // field. Map that field through the actual nine-slice/aspect settings;
        // the same skin then centres correctly at every control size.
        public static Rect ReadingRect(RectTransform area)
        {
            var board=area.GetComponent<UnityEngine.UI.Image>();
            if(board==null||!ReadingSource(board.sprite,out _)){
                board=null;
                // Button artwork lives below a transparent touch target. Its
                // wooden rim is not part of the label's available writing field.
                foreach(Transform child in area){
                    var image=child.GetComponent<UnityEngine.UI.Image>();
                    if(image!=null&&ReadingSource(image.sprite,out _)){board=image;break;}
                }
                if(board==null)board=area.parent==null?null:area.parent.GetComponent<UnityEngine.UI.Image>();
            }
            if(board==null||!ReadingSource(board.sprite,out var source))return area.rect;
            var rect=board.GetPixelAdjustedRect();var sprite=board.sprite;
            if(board.type==UnityEngine.UI.Image.Type.Sliced&&sprite.border!=Vector4.zero){
                float units=board.pixelsPerUnit*board.pixelsPerUnitMultiplier;var border=sprite.border;
                rect=Rect.MinMaxRect(
                    rect.xMin+SlicePoint(source.xMin,sprite.rect.width,rect.width,border.x,border.z,units),
                    rect.yMin+SlicePoint(source.yMin,sprite.rect.height,rect.height,border.y,border.w,units),
                    rect.xMin+SlicePoint(source.xMax,sprite.rect.width,rect.width,border.x,border.z,units),
                    rect.yMin+SlicePoint(source.yMax,sprite.rect.height,rect.height,border.y,border.w,units));
            }else{
                if(board.preserveAspect){
                    float aspect=sprite.rect.width/sprite.rect.height;var size=rect.size;
                    if(size.x/size.y>aspect)size.x=size.y*aspect;else size.y=size.x/aspect;
                    rect=new Rect(rect.center-size*.5f,size);
                }
                rect=new Rect(rect.min+Vector2.Scale(source.min,rect.size),Vector2.Scale(source.size,rect.size));
            }
            if(board.rectTransform==area)return rect;
            var low=(Vector2)area.InverseTransformPoint(board.transform.TransformPoint(rect.min));
            var high=(Vector2)area.InverseTransformPoint(board.transform.TransformPoint(rect.max));
            var sizeInside=Vector2.Min(area.rect.size,high-low);
            return new Rect((low+high-sizeInside)*.5f,sizeInside);
        }
        // Copper's highlighted rim needs a two-source-pixel optical offset;
        // validated against native 320/390/430-pixel parchment/letter renders.
        static readonly Rect[] worldWritingFields={
            PaperField(504,211,47,47,455,175),PaperField(503,212,56,48,447,175),PaperField(505,213,56,50,450,177),
            PaperField(504,211,54,47,443,175),PaperField(503,212,49,48,454,176),PaperField(505,211,54,48,447,175),
            PaperField(504,215,57,50,447,178),PaperField(503,214,57,51,440,178),PaperField(505,211,55,48,450,175)};
        static Rect PaperField(float width,float height,float left,float top,float right,float bottom)
            =>Rect.MinMaxRect(left/width,1-bottom/height,right/width,1-top/height);
        static bool ReadingSource(Sprite sprite,out Rect source)
        {
            source=default;if(sprite==null)return false;
            // Source rectangles exclude transparent export margins and the
            // painted bevel. SlicePoint preserves these clearances at any size.
            if(sprite.name=="GreenAction"){source=PaperField(416,140,28,33,388,113);return true;}
            if(sprite.name=="AmberAction"){source=PaperField(393,140,27,33,366,113);return true;}
            if(sprite.name=="RedAction"){source=PaperField(408,141,29,33,379,114);return true;}
            if(sprite.name=="GrayAction"){source=PaperField(436,137,28,31,408,111);return true;}
            if(sprite.name=="CoinCard"){source=PaperField(275,263,31,31,244,232);return true;}
            if(sprite.name=="HeaderBoard"){source=new Rect(160f/1400,71f/411,1080f/1400,205f/411);return true;}
            if(sprite.name=="Account Header"){source=PaperField(609,270,74,82,526,195);return true;}
            if(sprite.name=="Settings Header"){source=PaperField(872,512,120,255,748,405);return true;}
            if(sprite.name.StartsWith("WorldTitle",System.StringComparison.Ordinal)&&int.TryParse(sprite.name.Substring(10),out int world)&&world>=0&&world<worldWritingFields.Length){source=worldWritingFields[world];return true;}
            return false;
        }
        static float SlicePoint(float fraction,float source,float destination,float before,float after,float units)
        {
            float lower=before/units,upper=after/units;
            float fit=Mathf.Min(1,destination/Mathf.Max(.001f,lower+upper));lower*=fit;upper*=fit;
            float point=fraction*source;
            if(point<before)return before>0?point/before*lower:0;
            if(point>source-after)return destination-upper+(point-source+after)/Mathf.Max(.001f,after)*upper;
            return lower+(point-before)/Mathf.Max(.001f,source-before-after)*(destination-lower-upper);
        }
        public void Initialize(RectTransform area,params RectTransform[] content)
        {
            surface=area;elements=content;sizes=new Vector2[content.Length];drawSizes=new Vector2[content.Length];labels=new TextMeshProUGUI[content.Length];glyphs=new PauseGlyphGraphic[content.Length];
            for(int i=0;i<content.Length;i++){
                sizes[i]=content[i].rect.size;labels[i]=content[i].GetComponent<TextMeshProUGUI>();glyphs[i]=content[i].GetComponent<PauseGlyphGraphic>();
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
            var reading=ReadingRect(surface);float observedUnit=Unit(surface);bool dirty=false;for(int i=0;i<labels.Length;i++)if(labels[i]!=null&&(labels[i].havePropertiesChanged||labels[i].margin!=Vector4.zero||labels[i].textWrappingMode!=TextWrappingModes.NoWrap||labels[i].enableAutoSizing||labels[i].alignment!=TextAlignmentOptions.Center))dirty=true;
            int key=surface.rect.size.GetHashCode()+reading.GetHashCode()+Mathf.RoundToInt(observedUnit*1000);unchecked{
                key=key*31+Mathf.RoundToInt(surface.lossyScale.x*1000)+Mathf.RoundToInt(surface.lossyScale.y*1000)+Padding.GetHashCode();
                var canvas=surface.GetComponentInParent<Canvas>();key=key*31+(canvas==null?0:canvas.scaleFactor.GetHashCode());
                for(int i=0;i<labels.Length;i++)if(labels[i]!=null)key=key*31+(labels[i].text??"").GetHashCode()+labels[i].fontSizeMax.GetHashCode()+labels[i].characterSpacing.GetHashCode()+labels[i].font.GetInstanceID();
            }
            if(key==previous&&!dirty)return;previous=key;refreshing=true;
            try{
                LocalUnit=observedUnit;float gap=Gap*LocalUnit,padding=Mathf.Max(MinimumInset,Padding)*LocalUnit;
                float availableWidth=Mathf.Max(1,reading.width-2*padding);
                float availableHeight=Mathf.Max(1,reading.height-2*MinimumInset*LocalUnit);
                for(int i=0;i<elements.Length;i++){
                    var label=labels[i];
                    if(label!=null){
                        label.enableAutoSizing=false;label.fontSize=label.fontSizeMax;label.margin=Vector4.zero;
                        label.textWrappingMode=TextWrappingModes.NoWrap;label.alignment=TextAlignmentOptions.Center;label.overflowMode=TextOverflowModes.Overflow;
                    }else{
                        var size=sizes[i];var image=elements[i].GetComponent<UnityEngine.UI.Image>();
                        if(image!=null&&image.preserveAspect&&image.sprite!=null){float aspect=image.sprite.rect.width/image.sprite.rect.height;float h=Mathf.Min(size.y,size.x/aspect);size=new Vector2(h*aspect,h);}
                        drawSizes[i]=size;
                    }
                }
                float total=0;
                // Measure painted glyphs on both axes. A fixed minimum font size
                // must not force translated copy across the frame; fit the whole
                // group while keeping the icon-caption gap at four logical pixels.
                for(int pass=0;pass<4;pass++){
                    total=gap*(elements.Length-1);float cross=0;
                    for(int i=0;i<elements.Length;i++){
                        var item=elements[i];var label=labels[i];item.anchorMin=item.anchorMax=item.pivot=Vector2.one*.5f;
                        inkCenters[i]=Vector2.zero;
                        if(label!=null){
                            var preferred=label.GetPreferredValues(label.text,Mathf.Infinity,Mathf.Infinity);
                            item.sizeDelta=new Vector2(preferred.x+1,Mathf.Max(surface.rect.height,preferred.y+2));
                            label.ForceMeshUpdate();var bounds=GlyphBounds(label);
                            bool valid=bounds.size.x>=0&&bounds.size.y>=0&&!float.IsNaN(bounds.size.x)&&!float.IsInfinity(bounds.size.x);
                            widths[i]=valid?bounds.size.x:preferred.x;heights[i]=valid?bounds.size.y:label.fontSize;inkCenters[i]=valid?(Vector2)bounds.center:Vector2.zero;
                        }else{
                            item.sizeDelta=drawSizes[i];widths[i]=drawSizes[i].x;heights[i]=drawSizes[i].y;
                            if(glyphs[i]!=null){var bounds=glyphs[i].PaintedBounds;widths[i]=bounds.size.x;heights[i]=bounds.size.y;inkCenters[i]=bounds.center;}
                        }
                        total+=vertical?heights[i]:widths[i];cross=Mathf.Max(cross,vertical?widths[i]:heights[i]);
                    }
                    float spacing=gap*(elements.Length-1);
                    float along=Mathf.Max(.001f,(vertical?availableHeight:availableWidth)-spacing);
                    float across=vertical?availableWidth:availableHeight;
                    float fit=Mathf.Min(1,along/Mathf.Max(.001f,total-spacing),across/Mathf.Max(.001f,cross));
                    bool fixedIcons=FixedIconSize&&System.Array.Exists(labels,label=>label!=null);
                    if(fixedIcons){float fixedAlong=0,labelAlong=0,labelCross=0;
                        for(int i=0;i<elements.Length;i++)if(labels[i]==null)fixedAlong+=vertical?heights[i]:widths[i];else{labelAlong+=vertical?heights[i]:widths[i];labelCross=Mathf.Max(labelCross,vertical?widths[i]:heights[i]);}
                        fit=Mathf.Min(1,Mathf.Max(0,along-fixedAlong)/Mathf.Max(.001f,labelAlong),across/Mathf.Max(.001f,labelCross));
                    }
                    if(fit>=.9999f||pass==3)break;
                    fit=Mathf.Max(.001f,fit);
                    for(int i=0;i<elements.Length;i++)if(labels[i]!=null)labels[i].fontSize*=fit;else if(!fixedIcons)drawSizes[i]*=fit;
                }
                float x=-total*.5f;
                for(int i=0;i<elements.Length;i++){
                    elements[i].anchoredPosition=reading.center-surface.rect.center+(vertical?new Vector2(0,-x-heights[i]*.5f):new Vector2(x+widths[i]*.5f,0))-inkCenters[i];x+=(vertical?heights[i]:widths[i])+gap;
                }
            }finally{refreshing=false;}
        }
    }
    // This short-lived binder never leaves an uninitialised row on icon-only or
    // multi-part controls. Binding happens before their first canvas paint.
    internal sealed class ReferenceButtonCentering:MonoBehaviour
    {
        void OnEnable(){Canvas.willRenderCanvases+=Bind;}
        void OnDisable(){Canvas.willRenderCanvases-=Bind;}
        void Bind()
        {
            Canvas.willRenderCanvases-=Bind;
            if(isActiveAndEnabled)CenteredContentRow.BindPlainButton((RectTransform)transform);
            Destroy(this);
        }
    }

}
