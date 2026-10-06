using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        TextMeshProUGUI menuHeading,menuDescription;
        sealed class MenuRow { public RectTransform rect;public float top,height,gap,offset;public int index; }
        sealed class MenuBottom { public RectTransform rect;public float bottom; }
        readonly List<MenuRow> menuRows=new List<MenuRow>();
        readonly List<MenuBottom> menuBottoms=new List<MenuBottom>();
        float MenuUnit=>1/Mathf.Min(1,menuRoot.GetComponentInParent<Canvas>().scaleFactor);
        void RegisterMenuRow(Button button,float top,int index,float height=56,float gap=8)
        {
            var rect=(RectTransform)button.transform;
            if(!menuActions.Contains(rect))menuActions.Add(rect);
            menuRows.Add(new MenuRow{rect=rect,top=top,index=index,height=height,gap=gap});FitMenuRows();
        }
        void FitMenuRows()
        {
            if(menuRoot==null)return;float unit=MenuUnit;
            foreach(var row in menuRows)if(row.rect!=null){
                var r=row.rect;r.anchorMin=new Vector2(r.anchorMin.x,row.top);r.anchorMax=new Vector2(r.anchorMax.x,row.top);
                r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,row.height*unit);r.anchoredPosition=new Vector2(0,-(row.offset+row.index*(row.height+row.gap))*unit);
                if(CurrentScreen==GameScreen.Shop||CurrentScreen==GameScreen.Supplies||CurrentScreen==GameScreen.Collection){var caption=r.GetComponentInChildren<TextMeshProUGUI>();if(caption!=null&&r.GetComponent<Button>()!=null){caption.fontSizeMin=16*unit;caption.fontSizeMax=22*unit;}}
            }
            foreach(var item in menuBottoms)if(item.rect!=null){
                var r=item.rect;r.anchorMin=new Vector2(r.anchorMin.x,0);r.anchorMax=new Vector2(r.anchorMax.x,0);
                r.pivot=new Vector2(.5f,0);r.anchoredPosition=new Vector2(0,item.bottom*unit);r.sizeDelta=new Vector2(0,52*unit);
            }
        }
        void MarkSelection(Button button,bool selected)
        {
            button.GetComponent<Image>().color=selected?new Color(.98f,.74f,.32f):new Color(.84f,.91f,.87f);
            if(selected){var mark=Rect("Selected",button.transform,new Vector2(.79f,.05f),new Vector2(.95f,.25f));mark.gameObject.AddComponent<MenuTickGraphic>().color=MenuInk;}
        }
        void SettingsTabs()
        {
            string[] names={"Sound","Comfort tab","Language tab"};
            for(int i=0;i<3;i++){
                int page=i;float x=.08f+i*.285f;
                var tab=MenuButton(names[i],.755f,()=>{settingsPage=page;DrawMenu();},false,x,x+.27f);tab.name=new[]{"Soundeffects","Comfort","Languages"}[i];
                var label=tab.GetComponentInChildren<TextMeshProUGUI>();label.enableAutoSizing=true;label.fontSizeMin=16*MenuUnit;label.fontSizeMax=20*MenuUnit*(Preferences.LargeText?1.10f:1);label.margin=new Vector4(8*MenuUnit,5,8*MenuUnit,8);MarkSelection(tab,i==settingsPage);RegisterMenuRow(tab,.812f,0,52,0);
            }
        }
        void LanguageCarousel(string name,string current,float top,Action<string> action)
        {
            string[] codes={"en","ru","uk"};
            int index=Array.IndexOf(codes,current);
            var row=Rect(name+" carousel",menuRoot,new Vector2(.10f,top),new Vector2(.90f,top));
            row.pivot=new Vector2(.5f,1);menuRows.Add(new MenuRow{rect=row,top=top,height=64});
            var paper=Background(row,new Color(.84f,.91f,.87f),false);paper.sprite=art.panel;paper.type=Image.Type.Sliced;
            var left=DirectionButton(name+"Previous",row,Vector2.zero,new Vector2(.21f,1),false,()=>{feedback.Sound(ToyCue.Select);action(codes[(index+2)%3]);});
            var right=DirectionButton(name+"Next",row,new Vector2(.79f,0),Vector2.one,true,()=>{feedback.Sound(ToyCue.Select);action(codes[(index+1)%3]);});
            left.GetComponent<Image>().color=right.GetComponent<Image>().color=Color.clear;
            var label=Text("Selected language",row,new Vector2(.21f,0),new Vector2(.79f,1),22,NativeLanguageName(current));label.font=art.titleFont;label.textWrappingMode=TextWrappingModes.NoWrap;
            FitMenuRows();
        }
        void MenuNavRow(string label,string asset,int index,Action action,bool enabled=true)
        {
            var b=Button(label,menuRoot,new Vector2(.08f,.5f),new Vector2(.92f,.5f),action);b.interactable=enabled;
            b.GetComponent<Image>().color=new Color(.84f,.91f,.87f);RegisterMenuRow(b,.565f,index,62,8);
            var text=b.GetComponentInChildren<TextMeshProUGUI>();text.rectTransform.anchorMin=new Vector2(.26f,0);text.rectTransform.anchorMax=new Vector2(.90f,1);
            text.alignment=TextAlignmentOptions.MidlineLeft;text.fontSize=22*(Preferences.LargeText?1.10f:1);text.margin=new Vector4(3,3,3,3);
            Picture("Navigation object",b.transform,new Vector2(.03f,.05f),new Vector2(.23f,.95f),Resources.Load<Sprite>(asset=="RibbonSpool"||asset=="Hourglass"?"CascadeArt/Devices/"+asset:"CascadeArt/Workshop/"+asset),true);
            Rect("Open this page",b.transform,new Vector2(.92f,.36f),new Vector2(.97f,.64f)).gameObject.AddComponent<MenuChevronGraphic>().color=MenuInk;
        }
        void WorkshopProgress(int complete)
        {
            var rail=Rect("Lantern progress",menuRoot,new Vector2(.10f,.26f),new Vector2(.90f,.278f));var baseImage=Background(rail,new Color(.71f,.79f,.71f),false);baseImage.sprite=art.panel;baseImage.type=Image.Type.Sliced;
            var light=Rect("Restored light",rail,Vector2.zero,new Vector2(complete/10f,1));var fill=Background(light,new Color(.14f,.57f,.46f),false);fill.sprite=art.panel;fill.type=Image.Type.Sliced;
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
        protected override void OnPopulateMesh(VertexHelper h)
        {h.Clear();Stroke(h,rectTransform.rect,new Vector2(.18f,.86f),new Vector2(.80f,.50f),color);Stroke(h,rectTransform.rect,new Vector2(.80f,.50f),new Vector2(.18f,.14f),color);}
        public static void Stroke(VertexHelper h,Rect r,Vector2 a,Vector2 b,Color color)
        {
            a=r.min+Vector2.Scale(r.size,a);b=r.min+Vector2.Scale(r.size,b);var n=new Vector2(-(b-a).y,(b-a).x).normalized*Mathf.Min(r.width,r.height)*.08f;int i=h.currentVertCount;
            h.AddVert(a-n,color,Vector2.zero);h.AddVert(a+n,color,Vector2.zero);h.AddVert(b+n,color,Vector2.zero);h.AddVert(b-n,color,Vector2.zero);h.AddTriangle(i,i+1,i+2);h.AddTriangle(i,i+2,i+3);
        }
    }
}
