using System;
using Cascade.Application;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        const string ItemsArtPath="CascadeArt/ItemsReferenceOct14/";
        // Display order follows the reference; domain IDs and inventory are unchanged.
        static readonly int[] ItemsDisplayOrder={1,2,3,0,4,5};
        Sprite ItemsArt(string name)=>Resources.Load<Sprite>(ItemsArtPath+name);
        Sprite ItemsToolArt(int index)=>index==1?ItemsArt("BlueSpool")??ToolArt(index):ToolArt(index);
        void DrawReferenceSupplies()
        {
            shopTab=0;shopItem=toolsItem=Mathf.Clamp(toolsItem,0,ShopCatalog.ToolCount-1);
            var stage=ReferenceShopSurface(ItemsArt("Backdrop"));
            stage.GetComponent<ReferenceShopLayout>().BindContinuousGround(1030);
            ReferencePicture("Items striped canopy",stage,ReferenceShopArt("Awning"),121,160,508,85);
            var board=ReferencePicture("Items title board",stage,ReferenceShopArt("HeaderBoard"),153,84,342,120,false);
            board.type=Image.Type.Sliced;board.pixelsPerUnitMultiplier=4;
            var heading=ReferenceText("items heading",stage,ReferenceCopy("My items","Мои предметы","Мої предмети"),180,113,288,62,35);heading.characterSpacing=-1;CentreHeading(board,heading);
            ReferenceButton("Back",stage,51,115,84,84,BackFromTools,"BackButton");
            var wallet=ReferencePicture("Items wallet board",stage,ReferenceShopArt("WalletBoard"),491,108,214,90,false);
            wallet.type=Image.Type.Sliced;wallet.pixelsPerUnitMultiplier=3;
            ReferencePicture("Items wallet coins",stage,ItemsArt("WalletCoins")??ReferenceShopCoin(),501,113,86,79);
            ReferenceText("items wallet caption",stage,ReferenceCopy("My balance","Мой баланс","Мій баланс"),576,123,114,26,17);
            ReferenceText("items wallet count",stage,UiCopy.Raw(ShopCoins.ToString()),576,147,116,40,37);
            var lightWindow=ReferenceRect("Items sunlight sky clip",stage,54,244,612,379);
            lightWindow.gameObject.AddComponent<RectMask2D>();
            var light=ReferenceRect("Items animated sunlight",lightWindow,1,-6,610,430).gameObject.AddComponent<ItemSunlight>();light.Initialize(this);
            ReferencePicture("Items pale stone pedestal",stage,ItemsArt("Pedestal")??ReferenceShopArt("PreviewPlatform"),165,523,397,132);
            displayedShopObject=ReferencePicture("Items selected object",stage,ItemsToolArt(shopItem),171,260,374,374);
            DecorateToolIcon(displayedShopObject,shopItem);
            DrawItemsStock(stage,"selected",414,529,129,78,Stock((TravelTool)shopItem),52);
            var paper=ReferencePicture("Items description parchment",stage,ReferenceShopArt("DescriptionPaper"),76,618,568,181,false);
            paper.type=Image.Type.Sliced;paper.pixelsPerUnitMultiplier=1.5f;
            ReferenceText("items title",stage,ItemsToolTitle(shopItem),122,639,476,49,36).characterSpacing=-1;
            var benefit=ReferenceText("preview benefit",stage,ItemsToolDescription(shopItem),132,705,456,74,24,false,true);
            // Notices are useful when the bag was entered from play, but never turn into an extra panel.
            if(!string.IsNullOrEmpty(shopNotice.Key))benefit.SetCopy(shopNotice);
            DrawItemsStrip(stage);
            foreach(var label in stage.GetComponentsInChildren<TextMeshProUGUI>())label.color=new Color(.13f,.045f,.014f);
            var shop=ReferenceButton("Items visit shop",stage,125,1028,470,130,OpenItemsShop,"GreenAction");
            var shopIcon=ReferencePicture("Items storefront icon",ReferenceButtonBody(shop),ItemsArt("Storefront")??HomeArt("ShopIcon"),88,15,104,96);
            var shopCaption=ReferenceText("items shop caption",ReferenceButtonBody(shop),"Shop",179,23,229,75,40,true);CentreContent((RectTransform)ReferenceButtonBody(shop),shopIcon.rectTransform,shopCaption.rectTransform);
            int current=Array.IndexOf(ItemsDisplayOrder,shopItem);
            SwipeReferencePage(stage,244,720,t=>t.name.StartsWith("Items ",StringComparison.Ordinal)&&!t.name.Contains("wallet")&&!t.name.Contains("title board")&&!t.name.Contains("striped")&&!t.name.Contains("visit shop")||t.name.StartsWith("Reference shop items title",StringComparison.Ordinal)||t.name=="Reference shop preview benefit"||t.name.StartsWith("Reference shop items stock",StringComparison.Ordinal)||t.name.StartsWith("Shop object ",StringComparison.Ordinal),direction=>SelectShopItem(ItemsDisplayOrder[current+direction]),direction=>current+direction>=0&&current+direction<ItemsDisplayOrder.Length);
        }
        void OpenItemsShop()
        {
            int selected=toolsItem;ShowShop();shopTab=0;shopItem=selected;shopDetailOpen=false;shopNotice=default;DrawMenu();
        }
        void DrawItemsStock(Transform parent,string key,float x,float y,float width,float height,int amount,float size)
        {
            // A shadow and generous inset keep the pill clear of neighbouring cards.
            var shadow=ReferencePicture("Items stock shadow "+key,parent,ReferenceShopArt("StockBadge"),x+1,y+3,width,height,false);
            shadow.type=Image.Type.Sliced;shadow.pixelsPerUnitMultiplier=height<40?6:1.8f;shadow.color=new Color(.21f,.105f,.035f,.28f);
            var face=ReferencePicture("Items stock badge "+key,parent,ReferenceShopArt("StockBadge"),x,y,width,height,false);
            face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=height<40?6:1.8f;
            float px=height<40?4:9,py=height<40?2:5;
            ReferenceText("items stock "+key,parent,UiCopy.Raw("× "+amount),x+px,y+py,width-2*px,height-2*py,size);
        }
        void DrawItemsStrip(RectTransform stage)
        {
            int selected=Array.IndexOf(ItemsDisplayOrder,shopItem),first=selected/4*4,last=Math.Min(first+4,ItemsDisplayOrder.Length);
            DrawItemsArrow(stage,"Items previous page",74,true,first>0,()=>SelectShopItem(ItemsDisplayOrder[Math.Max(0,first-4)]));
            DrawItemsArrow(stage,"Items next page",577,false,last<ItemsDisplayOrder.Length,()=>SelectShopItem(ItemsDisplayOrder[last]));
            int count=last-first;float left=(720-(count*95+(count-1)*3))*.5f;
            for(int i=first;i<last;i++){
                int index=ItemsDisplayOrder[i];float x=left+(i-first)*98;bool selectedCard=index==shopItem,empty=Stock((TravelTool)index)==0;
                var face=ReferencePicture("Items card "+index,stage,ReferenceShopArt(selectedCard?"SelectedCard":"Card"),x-(selectedCard?6:0),836-(selectedCard?6:0),95+(selectedCard?12:0),100+(selectedCard?12:0),false);
                face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=selectedCard?15:7;
                if(empty&&!selectedCard){face.material=OwnedTrophyMaterial();face.color=new Color(.78f,.78f,.78f);}
                var button=ReferenceButton("Shop object "+index,stage,x,837,95,98,()=>SelectShopItem(index));
                button.interactable=!toolSearchPending&&(!PendingLesson.HasValue||PendingLesson<MechanicLesson.Wayfinder||PendingLesson>MechanicLesson.LandingLens||index==(int)PendingLesson.Value-(int)MechanicLesson.Wayfinder);
                var body=ReferenceButtonBody(button);
                var picture=ReferencePicture("Catalogue object",body,ItemsToolArt(index),12.5f,13,70,72);DecorateToolIcon(picture,index);
                if(empty)picture.material=OwnedTrophyMaterial();
                DrawItemsStock(body,"card "+index,47,66,46,30,Stock((TravelTool)index),21);
            }
        }
        void DrawItemsArrow(RectTransform stage,string name,float x,bool left,bool active,Action action)
        {
            var button=ReferenceButton(name,stage,x,848,72,76,action,"BackButton");button.interactable=active;
            var drawing=ReferenceButtonBody(button).GetComponentInChildren<Image>();
            if(!left){drawing.rectTransform.pivot=new Vector2(.5f,.5f);drawing.rectTransform.anchoredPosition=new Vector2(36,-38);drawing.rectTransform.localScale=new Vector3(-1,1,1);}
            // Keep the button face intact; the disabled arrow is pale, as in the shop pager.
            if(!active)drawing.color=new Color(.79f,.79f,.79f);
        }
        UiCopy ItemsToolTitle(int index)=>index==1?ReferenceCopy("Strong thread","Крепкая нить","Міцна нитка"):ToolTitle(index);
        UiCopy ItemsToolDescription(int index)
        {
            switch((TravelTool)index){
                case TravelTool.Wayfinder:return ReferenceCopy("A trusty guide through a tricky board.\nShows the way until this board breaks.","Надёжный проводник на сложной доске.\nПоказывает путь, пока доска не сломается.","Надійний провідник на складній дошці.\nПоказує шлях, доки дошка не зламається.");
                case TravelTool.UndoSpool:return ReferenceCopy("A small mistake can be unwound.\nGives one extra undo on this board\nafter your free undos run out.","Небольшую ошибку можно распутать.\nДаёт ещё одну отмену на этой доске,\nкогда бесплатные уже закончились.","Невелику помилку можна розплутати.\nДає ще одне скасування на цій дошці,\nколи безкоштовні вже закінчилися.");
                case TravelTool.ReturnHourglass:return ReferenceCopy("Turn back a moment of your journey.\nReturns the last broken board\nand the row that stood on it.","Возвращает мгновение путешествия.\nВосстанавливает последнюю сломанную\nдоску и ряд, который на ней стоял.","Повертає мить подорожі.\nВідновлює останню зламану дошку\nта ряд, що стояв на ній.");
                case TravelTool.SignalLantern:return ReferenceCopy("A warm light when the way is unclear.\nMakes one safe move for you.\nThe move still uses wood.","Тёплый свет, когда путь неясен.\nДелает за тебя один верный ход.\nХод по-прежнему расходует дерево.","Тепле світло, коли шлях неясний.\nРобить за тебе один правильний хід.\nХід усе одно витрачає дерево.");
                case TravelTool.LandingLens:return ReferenceCopy("Take a peek before the next step.\nShows where your row will land\nafter the next drop.","Загляни вперёд перед новым шагом.\nПоказывает, где окажется ряд\nпосле следующего спуска.","Зазирни вперед перед новим кроком.\nПоказує, де опиниться ряд\nпісля наступного спуску.");
                default:return ReferenceCopy("Give a tired board a little strength.\nAdds 2 durability. Bombs and wind\nkeep working as before.","Добавляет уставшей доске прочности: +2.\nБомбы и ветер продолжают\nработать как прежде.","Додає втомленій дошці міцності: +2.\nБомби й вітер продовжують\nпрацювати як раніше.");
            }
        }
    }
}
