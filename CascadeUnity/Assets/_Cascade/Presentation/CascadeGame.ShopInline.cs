using System.Collections.Generic;
using System.Linq;
using Cascade.Application;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        sealed class CatalogueStackItem
        {
            public RectTransform rect;
            public float height,minimum;
        }
        RectTransform catalogueInfo;
        float shortfallUntil;
        readonly List<CatalogueStackItem> catalogueStack=new List<CatalogueStackItem>();
        void DecorateToolIcon(Image picture,int tool)
        {
            if(tool!=(int)TravelTool.RepairKit)return;
            var mark=Rect("Repair kit brass tools",picture.transform,new Vector2(.34f,.30f),new Vector2(.69f,.53f));
            mark.gameObject.AddComponent<RepairKitMarkGraphic>().raycastTarget=false;
        }
        void DrawShopAwning()
        {
            var canopy=Rect("Shop canvas canopy",menuRoot,new Vector2(.055f,.843f),new Vector2(.945f,.899f));
            canopy.gameObject.AddComponent<ShopCanopyGraphic>().raycastTarget=false;
            canopy.SetAsFirstSibling();
        }
        void AddCatalogueItem(RectTransform rect,float height,float minimum)
        {
            rect.SetParent(catalogueInfo,false);
            rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            catalogueStack.Add(new CatalogueStackItem{rect=rect,height=height,minimum=minimum});
        }
        TextMeshProUGUI CataloguePaper(string name,UiCopy copy,int size,float height,float minimum)
        {
            var label=ReadingStrip(name,Vector2.zero,Vector2.one,copy,size,true);
            label.enableAutoSizing=true;label.fontSizeMax=size*MenuUnit;label.fontSizeMin=(size>16?14:11)*MenuUnit;
            label.margin=new Vector4(14,4,14,4)*MenuUnit;
            AddCatalogueItem((RectTransform)label.transform.parent,height,minimum);return label;
        }
        void AddCatalogueButton(Button button)
        {
            menuRows.RemoveAll(row=>row.rect==(RectTransform)button.transform);
            AddCatalogueItem((RectTransform)button.transform,48,44);
        }
        void DrawInlineCatalogueActions(bool bag,bool trophy)
        {
            catalogueInfo=Rect("Selected catalogue information",menuRoot,new Vector2(.08f,0),new Vector2(.92f,trophy?.29f:.38f));
            string gift=trophy?ShopCatalog.TrophyIds[shopItem]:null;
            var tool=(TravelTool)shopItem;
            var title=CataloguePaper("Selected object title",trophy?TrophyTitle(gift):ToolTitle(shopItem),20,44,34);
            title.textWrappingMode=TextWrappingModes.NoWrap;StyleCelebrationType(title,CelebrationType.PaperTitle);
            if(!trophy)CataloguePaper("Selected object benefit",ToolDescription(shopItem),18,68,44);
            UiCopy limits=!string.IsNullOrEmpty(shopNotice.Key)?shopNotice:trophy?UiCopy.Raw(""):
                UiCopy.Join("\n",new UiCopy[]{"2 tools per stage. Each type once.","Using tools: max 2 stars for the level."});
            if(!string.IsNullOrEmpty(limits.Key)){
                var rules=Text("Purchase limits",catalogueInfo,Vector2.zero,Vector2.one,14,limits);
                rules.enableAutoSizing=true;rules.fontSizeMax=14*MenuUnit;rules.fontSizeMin=12*MenuUnit;rules.margin=new Vector4(2,2,2,2)*MenuUnit;
                rules.alignment=TextAlignmentOptions.Midline;rules.color=MenuInk;rules.textWrappingMode=TextWrappingModes.Normal;
                AddCatalogueItem(rules.rectTransform,trophy?42:60,trophy?36:54);
            }
            // Inventory is a reference screen. The gameplay bag owns tool activation.
            if(!bag){
                bool owned=trophy&&OwnsGift(gift),full=!trophy&&Stock(tool)>=ShopCatalog.StockLimit;
                UiCopy caption=full?(UiCopy)"Bag is full":owned?(TrophyEquipped(gift)?(UiCopy)"Remove style":"Use this style"):(UiCopy)"Buy";
                var buy=CounterButton("Buy selected item",caption,.095f,.905f,0,1,()=>BuySelectedShopItem(),true);
                buy.interactable=!saveBlocked&&!HasSaveError&&GameEconomy!=null&&monetization?.FullscreenActive!=true&&!full;
                if(!owned&&!full)CompactCoinPrice(buy,trophy?ShopCatalog.TrophyPrice(gift):ShopCatalog.Price(tool));
                QuietDisabledCounter(buy);AddCatalogueButton(buy);
                if(trophy){
                    var preview=CounterButton("Preview selected item","Preview",.095f,.905f,0,1,OpenSelectedShopItem);
                    AddCatalogueButton(preview);
                }
            }
            FitCatalogueStack();
        }
        void FitCatalogueStack()
        {
            if(catalogueInfo==null||catalogueInfo.parent!=menuRoot||catalogueStack.Count==0)return;
            float unit=MenuUnit,available=catalogueInfo.rect.height/unit,total=12+(catalogueStack.Count-1)*6,minimum=12+(catalogueStack.Count-1)*4;
            foreach(var item in catalogueStack){total+=item.height;minimum+=item.minimum;}
            float fit=Mathf.InverseLerp(minimum,total,available),gap=Mathf.Lerp(4,6,fit);
            // Keep the selected item immediately below its grid and above its action.
            float y=available-6;
            foreach(var item in catalogueStack){
                if(item.rect==null)continue;
                float height=Mathf.Lerp(item.minimum,item.height,fit);
                item.rect.anchorMin=new Vector2(0,0);item.rect.anchorMax=new Vector2(1,0);item.rect.pivot=new Vector2(.5f,1);
                item.rect.sizeDelta=new Vector2(0,height*unit);item.rect.anchoredPosition=new Vector2(0,y*unit);y-=height+gap;
            }
            FitCatalogueHeading();
        }
        void FitCatalogueHeading()
        {
            var heading=menuRoot.GetComponentsInChildren<TextMeshProUGUI>().FirstOrDefault(label=>label.name=="Page heading");
            var purse=menuRoot.GetComponentsInChildren<RectTransform>().FirstOrDefault(rect=>rect.name=="Earned coin pouch");
            if(heading==null||purse==null)return;
            float half=Mathf.Min(.29f,Mathf.Max(.10f,purse.anchorMin.x-.5f-8*MenuUnit/menuRoot.rect.width));
            heading.rectTransform.anchorMin=new Vector2(.5f-half,heading.rectTransform.anchorMin.y);
            heading.rectTransform.anchorMax=new Vector2(.5f+half,heading.rectTransform.anchorMax.y);
            heading.rectTransform.offsetMin=heading.rectTransform.offsetMax=Vector2.zero;
            heading.margin=Vector4.zero;heading.textWrappingMode=TextWrappingModes.NoWrap;heading.enableAutoSizing=true;
            heading.fontSizeMax=26*MenuUnit;heading.fontSizeMin=14*MenuUnit;
        }
        void ShowInsufficientCoins()
        {
            feedback.Sound(ToyCue.Reject);feedback.Haptic(.22f);
            var button=menuRoot.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name=="Buy selected item")?.transform as RectTransform;
            shortfallUntil=Time.unscaledTime+3;
            PaintShopShortfall();
            if(button!=null&&!ReducedMotion)button.DOShakeAnchorPos(.32f,new Vector2(4*MenuUnit,0),12,0,false,true).SetUpdate(true).SetLink(button.gameObject);
        }
        void PaintShopShortfall()
        {
            if(CurrentScreen!=GameScreen.Shop||menuRoot==null||shortfallUntil<=0)return;
            if(Time.unscaledTime>=shortfallUntil){shortfallUntil=0;FinishMenuContrast();return;}
            float strength=ReducedMotion?1:.7f+.3f*Mathf.Cos((shortfallUntil-Time.unscaledTime)*Mathf.PI*3);
            var shade=Color.Lerp(MenuInk,new Color(.78f,.21f,.10f),strength);
            // Destroy is deferred; Find may still return the previous inactive menu.
            var labels=menuRoot.GetComponentsInChildren<TextMeshProUGUI>();
            var wallet=labels.FirstOrDefault(label=>label.name=="Wallet coins");
            var price=labels.FirstOrDefault(label=>label.name=="Coin price");
            if(wallet!=null)wallet.color=shade;if(price!=null)price.color=shade;
        }
    }
}
