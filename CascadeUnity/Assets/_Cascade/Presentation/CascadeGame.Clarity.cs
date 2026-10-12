using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        // A real authored paper strip belongs to a thought, never an entire screen.
        TextMeshProUGUI ReadingStrip(string name,Vector2 min,Vector2 max,UiCopy copy,int size=18,bool centred=false)
        {
            var paper=Rect(name+" papyrus",menuRoot,min,max);PaintStoryPaper(paper,false);
            var label=Text(name,paper,Vector2.zero,Vector2.one,size,copy);
            label.fontSize=(Preferences.LargeText?size+2:size)*MenuUnit;
            label.enableAutoSizing=false;label.margin=new Vector4(16,size<=14?4:10,16,size<=14?4:10)*MenuUnit;
            label.alignment=centred?TextAlignmentOptions.Midline:TextAlignmentOptions.MidlineLeft;
            label.textWrappingMode=TextWrappingModes.Normal;label.lineSpacing=0;
            StyleCelebrationType(label,CelebrationType.BodyCopy);
            return label;
        }
        void PaperFact(string name,Vector2 min,Vector2 max,Sprite icon,UiCopy caption,string value,bool compactGroup=false)
        {
            var paper=Rect(name+" papyrus",menuRoot,min,max);PaintStoryPaper(paper,false);
            Picture(name+" icon",paper,new Vector2(compactGroup?.24f:.07f,.17f),new Vector2(compactGroup?.40f:.30f,.83f),icon,true);
            var title=Text(name+" caption",paper,new Vector2(compactGroup?.42f:.34f,.62f),new Vector2(compactGroup?.78f:.91f,.94f),14,caption);
            title.fontSize=(Preferences.LargeText?16:14)*MenuUnit;title.margin=Vector4.zero;title.alignment=TextAlignmentOptions.Midline;
            StyleCelebrationType(title,CelebrationType.BodyCopy);
            var amount=Text(name+" value",paper,new Vector2(compactGroup?.42f:.34f,.10f),new Vector2(compactGroup?.78f:.91f,.61f),26,UiCopy.Raw(value));
            amount.fontSize=26*MenuUnit;amount.margin=Vector4.zero;amount.alignment=TextAlignmentOptions.Midline;
            amount.textWrappingMode=TextWrappingModes.NoWrap;StyleCelebrationType(amount,CelebrationType.SkyNumber);
        }
        void ObjectNameStrip(string name,Vector2 min,Vector2 max,UiCopy title)
        {
            var label=ReadingStrip(name,min,max,title,20,true);
            label.textWrappingMode=TextWrappingModes.NoWrap;label.enableAutoSizing=true;
            label.fontSizeMax=(Preferences.LargeText?22:20)*MenuUnit;label.fontSizeMin=16*MenuUnit;
            StyleCelebrationType(label,CelebrationType.PaperTitle);
        }
        bool IsOnStoryPaper(TextMeshProUGUI label)
        {
            for(var parent=label.transform.parent;parent!=null&&parent!=safeRoot;parent=parent.parent)
                if(parent.name.EndsWith("papyrus",System.StringComparison.Ordinal)||parent.name=="Speech ribbon"||parent.name=="Comic speech ribbon")return true;
            return false;
        }
    }
}
