using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        const float MenuControlHeight=56,MenuControlGap=12,MenuGroupGap=16;
        const float WorkshopSideInset=.04f,MenuColumnInset=.055f;
        TextMeshProUGUI menuHeading,menuDescription;
        sealed class MenuRow { public RectTransform rect;public float top,height,gap,offset,fontMin=16,fontMax=22;public int index; }
        sealed class MenuBottom { public RectTransform rect;public float bottom,height=48; }
        sealed class MenuColumn { public RectTransform rect;public int index,count;public float gap; }
        readonly List<MenuRow> menuRows=new List<MenuRow>();
        readonly List<MenuBottom> menuBottoms=new List<MenuBottom>();
        readonly List<MenuColumn> menuColumns=new List<MenuColumn>();
        readonly List<KeyValuePair<RectTransform,float>> menuSquares=new List<KeyValuePair<RectTransform,float>>();
        RectTransform settingsContent;
        ScrollRect settingsContentScroll;
        float MenuUnit=>1/Mathf.Min(1,menuRoot.GetComponentInParent<Canvas>().scaleFactor);
        bool WorkshopMenu=>CurrentScreen!=GameScreen.JourneyMap&&CurrentScreen!=GameScreen.Dialogue&&CurrentScreen!=GameScreen.JourneyEnd
            &&CurrentScreen!=GameScreen.Playing&&CurrentScreen!=GameScreen.Win&&CurrentScreen!=GameScreen.Fail&&CurrentScreen!=GameScreen.Checkpoint
            ||CurrentScreen==GameScreen.JourneyMap&&islandBrowseIndex>=0;
        void LayoutMenuSurface()
        {
            if(menuRoot==null)return;
            menuRoot.anchorMin=WorkshopMenu&&CurrentScreen!=GameScreen.Home&&!ReferenceShopScreen?new Vector2(WorkshopSideInset,.085f):Vector2.zero;
            menuRoot.anchorMax=WorkshopMenu&&CurrentScreen!=GameScreen.Home&&!ReferenceShopScreen?new Vector2(1-WorkshopSideInset,.965f):Vector2.one;
            menuRoot.offsetMin=menuRoot.offsetMax=Vector2.zero;
        }
        void RegisterMenuRow(Button button,float top,int index,float height=MenuControlHeight,float gap=MenuControlGap,float fontMin=15,float fontMax=19)
        {
            var rect=(RectTransform)button.transform;
            if(!menuActions.Contains(rect))menuActions.Add(rect);
            menuRows.Add(new MenuRow{rect=rect,top=top,index=index,height=height,gap=gap,fontMin=fontMin,fontMax=fontMax});FitMenuRows();
        }
        void FitMenuRows()
        {
            if(menuRoot==null)return;float unit=MenuUnit;
            foreach(var column in menuColumns)if(column.rect!=null){
                float gap=column.gap*unit/Mathf.Max(1,menuRoot.rect.width);
                float width=(1-2*MenuColumnInset-(column.count-1)*gap)/column.count;
                float left=MenuColumnInset+column.index*(width+gap);
                var r=column.rect;r.anchorMin=new Vector2(left,r.anchorMin.y);r.anchorMax=new Vector2(left+width,r.anchorMax.y);
            }
            if(CurrentScreen==GameScreen.TravelBag){
                bool compact=menuRoot.rect.height/unit<700;
                foreach(var row in menuRows){row.top=compact?.615f:.565f;row.height=compact?52:62;}
                var hero=menuRoot.Find("Mika at the bench") as RectTransform;
                var parcel=menuRoot.Find("Parcel") as RectTransform;
                var shelf=menuRoot.Find("Carved display shelf") as RectTransform;
                if(hero!=null){hero.anchorMin=new Vector2(.13f,compact?.64f:.59f);hero.anchorMax=new Vector2(.43f,.80f);}
                if(parcel!=null){parcel.anchorMin=new Vector2(.55f,compact?.665f:.62f);parcel.anchorMax=new Vector2(.86f,.79f);}
                if(shelf!=null){shelf.anchorMin=new Vector2(shelf.anchorMin.x,compact?.625f:.575f);shelf.anchorMax=new Vector2(shelf.anchorMax.x,compact?.648f:.598f);}
            }
            foreach(var row in menuRows)if(row.rect!=null){
                var r=row.rect;r.anchorMin=new Vector2(r.anchorMin.x,row.top);r.anchorMax=new Vector2(r.anchorMax.x,row.top);
                r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,row.height*unit);r.anchoredPosition=new Vector2(0,-(row.offset+row.index*(row.height+row.gap))*unit);
                if(CurrentScreen==GameScreen.Shop||CurrentScreen==GameScreen.Supplies||CurrentScreen==GameScreen.Collection){var caption=r.GetComponentInChildren<TextMeshProUGUI>();if(caption!=null&&r.GetComponent<Button>()!=null){caption.fontSizeMin=row.fontMin*unit;caption.fontSizeMax=row.fontMax*unit;}}
            }
            foreach(var item in menuBottoms)if(item.rect!=null){
                var r=item.rect;r.anchorMin=new Vector2(r.anchorMin.x,0);r.anchorMax=new Vector2(r.anchorMax.x,0);
                r.pivot=new Vector2(.5f,0);r.anchoredPosition=new Vector2(0,item.bottom*unit);r.sizeDelta=new Vector2(0,item.height*unit);
            }
            FitCompletionActions();
            foreach(var item in menuSquares)if(item.Key!=null)item.Key.sizeDelta=Vector2.one*item.Value*unit;
            FitSettingsLayout();ApplyMenuTypography();FitCatalogueStack();
        }
        (Button Previous,Button Next) MenuPager(string name,UiCopy count,float top,Action previousAction,Action nextAction)
        {
            var previous=DirectionButton("Previous "+name,menuRoot,new Vector2(.08f,top),new Vector2(.26f,top),false,previousAction);
            var next=DirectionButton("Next "+name,menuRoot,new Vector2(.74f,top),new Vector2(.92f,top),true,nextAction);
            RegisterMenuRow(previous,top,0,48,0);RegisterMenuRow(next,top,0,48,0);
            var label=Text("Navigation page count",menuRoot,new Vector2(.29f,top),new Vector2(.71f,top),22,count);
            label.textWrappingMode=TextWrappingModes.NoWrap;label.alignment=TextAlignmentOptions.Midline;
            menuRows.Add(new MenuRow{rect=label.rectTransform,top=top,height=48,gap=0});
            StyleCelebrationType(label,CelebrationType.SkyNumber);FitMenuRows();
            return (previous,next);
        }
        void MarkSelection(Button button,bool selected)
        {
            var face=button.GetComponent<Image>();face.sprite=enamelControls[selected?1:0];face.type=Image.Type.Sliced;face.color=Color.white;
            if(selected){var mark=Rect("Selected",button.transform,new Vector2(.79f,.05f),new Vector2(.95f,.25f));mark.gameObject.AddComponent<MenuTickGraphic>().color=MenuInk;}
        }
        static readonly string[] SettingsSections={"Sound & motion","Comfort & help","Languages","Reminders","Cloud save"};
        void BuildSettingsList()
        {
            var pane=Rect("Settings list",menuRoot,new Vector2(MenuColumnInset,0),new Vector2(1-MenuColumnInset,1));
            var viewport=Rect("Settings viewport",pane,Vector2.zero,Vector2.one);
            Background(viewport,Color.clear,true);viewport.gameObject.AddComponent<RectMask2D>();
            settingsContent=Rect("Settings content",viewport,new Vector2(0,1),Vector2.one);settingsContent.pivot=new Vector2(.5f,1);
            var layout=settingsContent.gameObject.AddComponent<VerticalLayoutGroup>();layout.childControlWidth=layout.childControlHeight=true;
            layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;layout.childAlignment=TextAnchor.UpperCenter;
            var fitter=settingsContent.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            settingsContentScroll=pane.gameObject.AddComponent<ScrollRect>();settingsContentScroll.viewport=viewport;settingsContentScroll.content=settingsContent;
            settingsContentScroll.horizontal=false;settingsContentScroll.vertical=true;settingsContentScroll.movementType=ScrollRect.MovementType.Clamped;
            settingsContentScroll.decelerationRate=.12f;FitSettingsLayout();
        }
        void CatalogueTabBounds(int index,out float left,out float right)
        {
            float gap=MenuControlGap*MenuUnit/Mathf.Max(1,menuRoot.rect.width);
            float width=(1-2*MenuColumnInset-2*gap)/3;
            left=MenuColumnInset+index*(width+gap);right=left+width;
        }
        void RegisterMenuColumn(Button button,int index,int count,float gap)
            =>menuColumns.Add(new MenuColumn{rect=(RectTransform)button.transform,index=index,count=count,gap=gap});
        void DrawSettingsSections()
        {
            string[] names={"Soundeffects","Comfort","Languages","Reminders","Cloudsave"};
            for(int i=0;i<SettingsSections.Length;i++){
                int page=i;var button=SettingsButton(SettingsSections[i],()=>{settingsPage=page;DrawMenu();});button.name=names[i];
                var caption=button.GetComponentInChildren<TextMeshProUGUI>();caption.alignment=TextAlignmentOptions.MidlineLeft;
                caption.rectTransform.anchorMin=Vector2.zero;caption.rectTransform.anchorMax=Vector2.one;
                caption.rectTransform.offsetMin=new Vector2(18*MenuUnit,6*MenuUnit);caption.rectTransform.offsetMax=new Vector2(-42*MenuUnit,-6*MenuUnit);
                caption.margin=Vector4.zero;
                var arrow=Rect("Open section",button.transform,new Vector2(1,.5f),new Vector2(1,.5f));arrow.sizeDelta=new Vector2(20,12)*MenuUnit;arrow.anchoredPosition=new Vector2(-30*MenuUnit,0);
                var chevron=arrow.gameObject.AddComponent<ChevronGraphic>();chevron.PointsRight=true;chevron.Weight=1;chevron.color=MenuPaper;chevron.raycastTarget=false;
            }
            FitSettingsLayout();
        }
        void FitSettingsLayout()
        {
            if(CurrentScreen!=GameScreen.Settings||settingsContentScroll==null)return;
            float unit=MenuUnit;
            if(menuHeading!=null){var r=menuHeading.rectTransform;r.anchorMin=new Vector2(.05f,1);r.anchorMax=new Vector2(.95f,1);r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,44*unit);r.anchoredPosition=new Vector2(0,-4*unit);}
            var pane=(RectTransform)settingsContentScroll.transform;pane.offsetMin=new Vector2(0,80*unit);pane.offsetMax=new Vector2(0,-66*unit);
            settingsContent.GetComponent<VerticalLayoutGroup>().spacing=12*unit;
            foreach(var element in settingsContent.GetComponentsInChildren<LayoutElement>()){
                if(element.name.EndsWith("papyrus",StringComparison.Ordinal)){
                    var note=element.GetComponentInChildren<TextMeshProUGUI>();note.fontSize=(Preferences.LargeText?20:18)*unit;
                    float width=Mathf.Max(160*unit,menuRoot.rect.width*(1-2*MenuColumnInset)-32*unit);
                    element.minHeight=element.preferredHeight=Mathf.Max(100*unit,note.GetPreferredValues(note.text,width,Mathf.Infinity).y+32*unit);
                }else {element.minHeight=element.preferredHeight=(element.name.EndsWith("carousel",StringComparison.Ordinal)?60:Preferences.LargeText?56:52)*unit;}

            }
        }
        Button SettingsButton(UiCopy caption,Action action)
        {
            var button=Button(caption,settingsContent,Vector2.zero,Vector2.one,()=>{feedback.Sound(ToyCue.Select);action();});
            var face=button.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=Color.white;
            var size=button.gameObject.AddComponent<LayoutElement>();size.minHeight=size.preferredHeight=52*MenuUnit;
            var label=button.GetComponentInChildren<TextMeshProUGUI>();label.enableAutoSizing=true;label.fontSizeMin=15*MenuUnit;label.fontSizeMax=20*MenuUnit*(Preferences.LargeText?1.1f:1);
            label.textWrappingMode=TextWrappingModes.NoWrap;label.margin=new Vector4(14*MenuUnit,6*MenuUnit,14*MenuUnit,6*MenuUnit);
            return button;
        }
        TextMeshProUGUI SettingsNote(string name,UiCopy caption)
        {
            var paper=Rect(name+" papyrus",settingsContent,Vector2.zero,Vector2.one);PaintStoryPaper(paper,false);
            var text=Text(name+" note",paper,Vector2.zero,Vector2.one,18,caption);
            text.enableAutoSizing=false;text.fontSize=(Preferences.LargeText?20:18)*MenuUnit;
            text.textWrappingMode=TextWrappingModes.Normal;text.alignment=TextAlignmentOptions.MidlineLeft;
            text.margin=new Vector4(16,12,16,12)*MenuUnit;
            float width=Mathf.Max(160*MenuUnit,menuRoot.rect.width*(1-2*MenuColumnInset)-40*MenuUnit);
            var size=paper.gameObject.AddComponent<LayoutElement>();size.minHeight=size.preferredHeight=Mathf.Max(100*MenuUnit,text.GetPreferredValues(text.text,width,Mathf.Infinity).y+32*MenuUnit);
            StyleCelebrationType(text,CelebrationType.BodyCopy);return text;
        }
        void LanguageCarousel(string name,string current,float top,Action<string> action)
        {
            string[] codes={"en","ru","uk"};
            int index=Array.IndexOf(codes,current);
            var row=Rect(name+" carousel",settingsContent,Vector2.zero,Vector2.one);
            var size=row.gameObject.AddComponent<LayoutElement>();size.minHeight=size.preferredHeight=60*MenuUnit;
            var paper=Background(row,Color.white,false);paper.sprite=enamelControls[0];paper.type=Image.Type.Sliced;
            var left=DirectionButton(name+"Previous",row,Vector2.zero,new Vector2(.21f,1),false,()=>{feedback.Sound(ToyCue.Select);action(codes[(index+2)%3]);});
            var right=DirectionButton(name+"Next",row,new Vector2(.79f,0),Vector2.one,true,()=>{feedback.Sound(ToyCue.Select);action(codes[(index+1)%3]);});
            left.GetComponent<Image>().color=right.GetComponent<Image>().color=Color.clear;
            foreach(var direction in new[]{left,right})foreach(var chevron in direction.GetComponentsInChildren<ChevronGraphic>())chevron.color=MenuPaper;
            var label=Text("Selected language",row,new Vector2(.21f,0),new Vector2(.79f,1),22,NativeLanguageName(current));label.font=art.titleFont;navigationLightLabels.Add(label);label.textWrappingMode=TextWrappingModes.NoWrap;
            FitMenuRows();
        }
        void MenuNavRow(string label,string asset,int index,Action action,bool enabled=true)
        {
            var b=Button(label,menuRoot,new Vector2(.08f,.5f),new Vector2(.92f,.5f),action);b.interactable=enabled;
            if(label=="My journal")b.name="My journal navigation";
            b.GetComponent<Image>().sprite=enamelControls[0];b.GetComponent<Image>().type=Image.Type.Sliced;b.GetComponent<Image>().color=Color.white;RegisterMenuRow(b,.565f,index,MenuControlHeight,MenuControlGap);
            var text=b.GetComponentInChildren<TextMeshProUGUI>();text.rectTransform.anchorMin=new Vector2(.26f,0);text.rectTransform.anchorMax=new Vector2(.86f,1);
            text.alignment=TextAlignmentOptions.MidlineLeft;text.fontSize=22*(Preferences.LargeText?1.10f:1);text.margin=new Vector4(3,3,3,3);
            MenuIcon("Navigation object",b.transform,new Vector2(.13f,.5f),Resources.Load<Sprite>(asset=="RibbonSpool"||asset=="Hourglass"?"CascadeArt/Devices/"+asset:"CascadeArt/Workshop/"+asset));
            var arrow=Rect("Open this page",b.transform,new Vector2(.89f,.37f),new Vector2(.95f,.63f)).gameObject.AddComponent<ChevronGraphic>();
            arrow.PointsRight=true;arrow.Weight=1;arrow.color=MenuPaper;arrow.raycastTarget=false;
        }
        void WorkshopProgress(int complete)
        {
            var rail=Rect("Lantern progress",menuRoot,new Vector2(.10f,.26f),new Vector2(.90f,.278f));var baseImage=Background(rail,new Color(.71f,.79f,.71f),false);baseImage.sprite=art.panel;baseImage.type=Image.Type.Sliced;
            var light=Rect("Restored light",rail,Vector2.zero,new Vector2(complete/10f,1));var fill=Background(light,new Color(.14f,.57f,.46f),false);fill.sprite=art.panel;fill.type=Image.Type.Sliced;
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SettingsGlyphGraphic:MaskableGraphic
    {
        public int Section;
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();var r=rectTransform.rect;
            void Line(Vector2 a,Vector2 b)=>MenuChevronGraphic.Stroke(h,r,a,b,color);
            void Polygon(params Vector2[] points){int first=h.currentVertCount;foreach(var p in points)h.AddVert(r.min+Vector2.Scale(r.size,p),color,Vector2.zero);for(int i=1;i<points.Length-1;i++)h.AddTriangle(first,first+i,first+i+1);}
            void Arc(Vector2 centre,Vector2 radius,float start,float end,int steps=16){var previous=centre+Vector2.Scale(radius,new Vector2(Mathf.Cos(start),Mathf.Sin(start)));for(int i=1;i<=steps;i++){float angle=Mathf.Lerp(start,end,i/(float)steps);var next=centre+Vector2.Scale(radius,new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)));Line(previous,next);previous=next;}}
            void Disc(Vector2 centre,float radius){int first=h.currentVertCount;h.AddVert(r.min+Vector2.Scale(r.size,centre),color,Vector2.zero);for(int i=0;i<=24;i++){float angle=i*Mathf.PI*2/24;h.AddVert(r.min+Vector2.Scale(r.size,centre+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius),color,Vector2.zero);if(i>0)h.AddTriangle(first,first+i,first+i+1);}}
            if(Section==0){
                Polygon(new Vector2(.08f,.35f),new Vector2(.28f,.35f),new Vector2(.51f,.14f),new Vector2(.51f,.86f),new Vector2(.28f,.65f),new Vector2(.08f,.65f));
                Arc(new Vector2(.51f,.50f),new Vector2(.22f,.26f),-1.05f,1.05f);Arc(new Vector2(.51f,.50f),new Vector2(.41f,.44f),-1.05f,1.05f);
            }else if(Section==1){
                Arc(new Vector2(.50f,.49f),new Vector2(.42f,.28f),0,Mathf.PI);Arc(new Vector2(.50f,.49f),new Vector2(.42f,.28f),Mathf.PI,Mathf.PI*2);
                Disc(new Vector2(.50f,.49f),.13f);
            }else if(Section==2){
                Arc(new Vector2(.50f,.50f),new Vector2(.43f,.43f),0,Mathf.PI*2,24);
                Arc(new Vector2(.50f,.50f),new Vector2(.20f,.43f),0,Mathf.PI*2,24);
                Line(new Vector2(.07f,.50f),new Vector2(.93f,.50f));
            }else if(Section==3){
                Arc(new Vector2(.50f,.53f),new Vector2(.29f,.32f),0,Mathf.PI);
                Line(new Vector2(.21f,.53f),new Vector2(.18f,.23f));Line(new Vector2(.79f,.53f),new Vector2(.82f,.23f));
                Line(new Vector2(.12f,.23f),new Vector2(.88f,.23f));Disc(new Vector2(.50f,.12f),.065f);Disc(new Vector2(.50f,.93f),.05f);
            }else{
                Disc(new Vector2(.24f,.43f),.20f);Disc(new Vector2(.46f,.60f),.28f);Disc(new Vector2(.72f,.47f),.23f);
                Polygon(new Vector2(.24f,.23f),new Vector2(.73f,.23f),new Vector2(.73f,.49f),new Vector2(.24f,.49f));
            }
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ContactRibbonGraphic:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();var r=rectTransform.rect;
            void Polygon(Color tint,params Vector2[] points){int first=h.currentVertCount;foreach(var p in points)h.AddVert(r.min+Vector2.Scale(r.size,p),tint,Vector2.zero);for(int i=1;i<points.Length-1;i++)h.AddTriangle(first,first+i,first+i+1);}
            var fold=Color.Lerp(color,Color.black,.24f);
            Polygon(fold,new Vector2(-.055f,.10f),new Vector2(.12f,.10f),new Vector2(.12f,.77f),new Vector2(-.055f,.77f),new Vector2(.005f,.44f));
            Polygon(fold,new Vector2(.88f,.10f),new Vector2(1.055f,.10f),new Vector2(.995f,.44f),new Vector2(1.055f,.77f),new Vector2(.88f,.77f));
            Polygon(color,new Vector2(0,.18f),new Vector2(1,.18f),new Vector2(1,.98f),new Vector2(0,.98f));
            Polygon(Color.Lerp(color,Color.white,.22f),new Vector2(.02f,.90f),new Vector2(.98f,.90f),new Vector2(.98f,.94f),new Vector2(.02f,.94f));
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuTickGraphic:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper h)
        {h.Clear();MenuChevronGraphic.Stroke(h,rectTransform.rect,new Vector2(.12f,.52f),new Vector2(.39f,.22f),color);MenuChevronGraphic.Stroke(h,rectTransform.rect,new Vector2(.39f,.22f),new Vector2(.89f,.80f),color);}
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuChevronGraphic:MaskableGraphic
    {
        public override Texture mainTexture=>NavigationArrowArt.Texture;
        protected override void OnPopulateMesh(VertexHelper h)
        {NavigationArrowArt.Draw(h,rectTransform.rect,1,true,color);}
        public static void Stroke(VertexHelper h,Rect r,Vector2 a,Vector2 b,Color color)
        {
            a=r.min+Vector2.Scale(r.size,a);b=r.min+Vector2.Scale(r.size,b);var n=new Vector2(-(b-a).y,(b-a).x).normalized*Mathf.Min(r.width,r.height)*.08f;int i=h.currentVertCount;
            h.AddVert(a-n,color,Vector2.zero);h.AddVert(a+n,color,Vector2.zero);h.AddVert(b+n,color,Vector2.zero);h.AddVert(b-n,color,Vector2.zero);h.AddTriangle(i,i+1,i+2);h.AddTriangle(i,i+2,i+3);
        }
    }
}
