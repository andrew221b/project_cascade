using System;
using System.Linq;
using Cascade.Application;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        const string ReferenceShopArtPath="CascadeArt/ShopReferenceOct13/";
        bool ReferenceIslandScreen=>CurrentScreen==GameScreen.JourneyMap&&islandBrowseIndex>=0;
        bool ReferenceShopScreen=>ReferenceMapScreen||ReferenceIslandScreen||CurrentScreen==GameScreen.AccountGate||CurrentScreen==GameScreen.Settings||CurrentScreen==GameScreen.Shop||CurrentScreen==GameScreen.CoinPacks||CurrentScreen==GameScreen.NoAds||CurrentScreen==GameScreen.Collection||CurrentScreen==GameScreen.TravelBag||CurrentScreen==GameScreen.Parcel||CurrentScreen==GameScreen.Supplies||CurrentScreen==GameScreen.Pause;
        Sprite referenceShopCoin;
        Sprite ReferenceShopCoin(){if(referenceShopCoin==null){var texture=Resources.Load<Texture2D>("CascadeArt/Workshop/RewardEmblems");if(texture!=null)referenceShopCoin=AuthoredIconRegion(texture,new Rect(100,138,679,603),1774,887);}return referenceShopCoin;}
        Sprite ReferenceShopArt(string name)=>name=="HeaderBoard"?Resources.Load<Sprite>(ReferenceShopArtPath+name):CleanMenuArt.Get(name)??Resources.Load<Sprite>(ReferenceShopArtPath+name);
        UiCopy ReferenceCopy(string en,string ru,string uk)=>UiCopy.Raw(UiLanguage=="ru"?ru:UiLanguage=="uk"?uk:en);
        RectTransform ReferenceRect(string name,Transform parent,float x,float y,float width,float height)
        {
            var r=Rect(name,parent,new Vector2(0,1),new Vector2(0,1));r.pivot=new Vector2(0,1);
            r.sizeDelta=new Vector2(width,height);r.anchoredPosition=new Vector2(x,-y);return r;
        }
        Image ReferencePicture(string name,Transform parent,Sprite sprite,float x,float y,float width,float height,bool preserve=true)
        {
            var image=ReferenceRect(name,parent,x,y,width,height).gameObject.AddComponent<Image>();
            image.sprite=sprite;image.color=Color.white;image.preserveAspect=preserve;image.raycastTarget=false;
            if(sprite!=null&&CleanMenuArt.Get(sprite.name)==sprite&&sprite.name!="GreenAction"&&sprite.name!="AmberAction"&&sprite.name!="RedAction"&&sprite.name!="GrayAction")image.gameObject.AddComponent<PrimaryButtonSkin>().Configure(sprite);
            return image;
        }
        TextMeshProUGUI ReferenceText(string name,Transform parent,UiCopy copy,float x,float y,float width,float height,float max,bool light=false,bool wrap=false)
        {
            var r=ReferenceRect("Reference shop "+name,parent,x,y,width,height);
            var label=r.gameObject.AddComponent<TextMeshProUGUI>();label.SetCopy(copy);
            label.font=Resources.Load<TMP_FontAsset>((name=="selected benefit"||name=="preview benefit")?"CascadeArt/Fonts/InterfaceBody":"CascadeArt/Fonts/InterfaceTitle")??art.titleFont;
            label.fontStyle=FontStyles.Normal;label.alignment=TextAlignmentOptions.Midline;
            label.fontSize=label.fontSizeMax=max*(Preferences.LargeText?1.08f:1);label.fontSizeMin=max*(name.StartsWith("store price",StringComparison.Ordinal)?.5f:.66f);
            label.enableAutoSizing=true;label.textWrappingMode=wrap?TextWrappingModes.Normal:TextWrappingModes.NoWrap;
            label.overflowMode=TextOverflowModes.Truncate;label.margin=Vector4.zero;label.raycastTarget=false;
            label.color=light?MenuPaper:new Color(.24f,.095f,.045f);if(light)navigationLightLabels.Add(label);return label;
        }
        Button ReferenceButton(string name,Transform parent,float x,float y,float width,float height,Action action,string sprite=null)
        {
            var r=ReferenceRect(name,parent,x,y,width,height);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x+width*.5f,-y-height*.5f);var face=r.gameObject.AddComponent<Image>();face.color=Color.clear;
            var button=r.gameObject.AddComponent<Button>();r.gameObject.AddComponent<ButtonFeel>().Initialize(this);button.targetGraphic=face;button.transition=Selectable.Transition.None;
            button.onClick.AddListener(()=>{feedback.Sound(ToyCue.Select);feedback.Haptic(.045f);action();});
            var body=Rect("Reference shop button body",r,new Vector2(.5f,.5f),new Vector2(.5f,.5f));body.sizeDelta=new Vector2(width,height);
            if(sprite!=null){
                var drawing=ReferencePicture("Reference shop action artwork",body,ReferenceShopArt(sprite),0,0,width,height);
                button.targetGraphic=drawing;button.transition=Selectable.Transition.ColorTint;var tint=button.colors;tint.highlightedColor=Color.white;tint.pressedColor=new Color(.93f,.97f,.90f);tint.disabledColor=new Color(.70f,.74f,.70f);button.colors=tint;
                if(sprite=="GreenAction"||sprite=="TealAction"&&name!="Preview selected item")StylePrimaryFace(drawing);
                else if(sprite=="GrayAction"||sprite=="RedAction"||sprite=="AmberAction")drawing.gameObject.AddComponent<PrimaryButtonSkin>().Configure(ReferenceShopArt(sprite));
            }
            parent.GetComponentInParent<ReferenceShopLayout>()?.BindTouch(r,new Vector2(width,height));
            return button;
        }
        Transform ReferenceButtonBody(Button button)=>button.transform.Find("Reference shop button body");
        RectTransform ReferenceShopSurface(Sprite backdrop=null)
        {
            LoadNavigationArt();catalogueInfo=null;catalogueStack.Clear();displayedShopObject=null;
            var frame=Rect("Reference shop environment",menuRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            var image=frame.gameObject.AddComponent<Image>();image.sprite=backdrop??ReferenceShopArt("Backdrop");image.type=Image.Type.Sliced;image.color=Color.white;image.raycastTarget=false;
            var stage=Rect("Reference shop composition",menuRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            stage.sizeDelta=new Vector2(720,1280);stage.gameObject.AddComponent<ReferenceShopLayout>().Initialize(menuRoot,frame);
            stage.GetComponent<ReferenceShopLayout>().BindContinuousGround();
            var worn=Rect("Reference worn trophy atmosphere",stage,Vector2.zero,Vector2.one);CosmeticAmbiance(worn,EquippedGift,false);
            var scenery=Rect("Reference scenery trophy atmosphere",stage,Vector2.zero,Vector2.one);CosmeticAmbiance(scenery,EquippedScenery,false);
            AddMenuLeaves(stage,true);
            return stage;
        }
        void ReferenceShopHeader(RectTransform stage,bool collection,bool preview=false,bool coins=false)
        {
            float rise=coins?-39:0;
            ReferencePicture("Reference shop awning",stage,ReferenceShopArt("Awning"),121,201+rise,508,85);
            var board=ReferencePicture("Reference shop title board",stage,ReferenceShopArt("HeaderBoard"),156,122+rise,350,132,false);board.type=Image.Type.Sliced;board.pixelsPerUnitMultiplier=4;
            var heading=ReferenceText("heading",stage,collection||preview?(UiCopy)"Trophies":"Shop",210,156+rise,263,77,46);CentreHeading(board,heading);
            ReferenceButton("Back",stage,51,159+rise,86,85,()=>{
                if(shopDetailOpen){shopDetailOpen=false;shopNotice=default;DrawMenu();}
                else if(collection)BackFromCollection();else BackFromShop();
            },"BackButton");
            bool trophies=collection||preview;float wx=trophies?493:508,ww=trophies?214:190;
            var wallet=ReferencePicture("Reference shop wallet board",stage,ReferenceShopArt("WalletBoard"),wx,153+rise,ww,94,false);wallet.type=Image.Type.Sliced;wallet.pixelsPerUnitMultiplier=3;
            var emblem=ReferencePicture("Reference shop wallet emblem",wallet.transform,trophies?Resources.Load<Sprite>("CascadeArt/Workshop/LeafPin"):ReferenceShopCoin(),9,16,56,59);var walletCopy=ReferenceRect("Reference shop wallet text",wallet.transform,66,14,ww-78,67);
            ReferenceText("wallet caption",walletCopy,trophies?ReferenceCopy("My trophies","Мои трофеи","Мої трофеї"):ReferenceCopy("My balance","Мой баланс","Мій баланс"),0,0,ww-78,27,16);
            string value=trophies?ShopCatalog.TrophyIds.Count(OwnsGift)+" / "+ShopCatalog.TrophyIds.Length:ShopCoins.ToString();
            ReferenceText("wallet count",walletCopy,UiCopy.Raw(value),0,27,ww-78,40,38);CentreInfoColumn(walletCopy);CentreContent(wallet.rectTransform,emblem.rectTransform,walletCopy);
            if(collection||preview)return;
            ReferencePicture("Reference shop tabs strip",stage,ReferenceShopArt("TabsStrip"),39,291+rise,643,89,false);
            string[] names={"Tool tab","Trophies","Coins"};int selected=coins?2:shopTab;
            for(int i=0;i<3;i++){
                int tab=i;float left=75+i*197;
                var choice=ReferenceButton(names[i]+" tab",stage,left,301+rise,194,67,()=>{
                    if(tab==2)ShowCoinPacks();else{shopTab=tab;shopItem=0;shopDetailOpen=false;shopNotice=default;SetScreen(GameScreen.Shop);}
                });
                var body=ReferenceButtonBody(choice);
                if(i==selected)ReferencePicture("Reference shop active tab",body,ReferenceShopArt("TabActive"),0,5,194,57);
                var tabLabel=ReferenceText("tab "+i,body,names[i],7,5,178,57,35,i==selected);CentreContent((RectTransform)body,tabLabel.rectTransform);
            }
        }
        void DrawReferenceShop(bool collection=false)
        {
            if(collection){DrawReferenceCollection();return;}
            if(shopTab==0)shopItem=Mathf.Clamp(shopItem,0,ShopCatalog.ToolCount-1);
            if(!collection)shopDetailOpen=false;
            var stage=ReferenceShopSurface();ReferenceShopHeader(stage,collection,collection&&shopDetailOpen);
            string[] group=shopTab==1?CurrentTrophyGroup:null;int count=shopTab==1?group.Length:ShopCatalog.ToolCount;
            if(count==0){
                ReferencePicture("Reference shop empty collection paper",stage,ReferenceShopArt("DescriptionPaper"),71,883,578,175);
                ReferenceText("empty collection",stage,"No trophies yet",121,925,478,75,38);
                var open=ReferenceButton("Find trophies in shop",stage,93,1079,532,112,()=>{ShowShop();shopTab=1;shopItem=0;DrawMenu();},"TealAction");
                ReferenceText("empty collection action",ReferenceButtonBody(open),"Find in shop",35,19,462,77,44,true);return;
            }
            int selected=shopTab==1?Array.IndexOf(group,ShopCatalog.TrophyIds[Mathf.Clamp(shopItem,0,ShopCatalog.TrophyIds.Length-1)]):Mathf.Clamp(shopItem,0,count-1);
            if(selected<0){selected=0;shopItem=Array.IndexOf(ShopCatalog.TrophyIds,group[0]);if(collection)giftPage=shopItem;}
            int first=shopTab==1?selected/6*6:0,last=Math.Min(first+6,count);
            for(int item=first;item<last;item++){
                int index=shopTab==1?Array.IndexOf(ShopCatalog.TrophyIds,group[item]):item,chosen=index,slot=item-first;
                float cardHeight=shopTab==1?198:229;
                float x=52+(slot%3)*210,y=(shopTab==1?398:403)+(slot/3)*(shopTab==1?204:232);
                bool selectedCard=index==shopItem,owned=shopTab==1&&OwnsGift(ShopCatalog.TrophyIds[index]);
                float rim=selectedCard?10:0;
                var card=ReferencePicture("Reference shop card "+index,stage,ReferenceShopArt(selectedCard?"SelectedCard":"Card"),x-rim,y-rim,207+rim*2,cardHeight+rim*2,false);
                card.type=Image.Type.Sliced;card.pixelsPerUnitMultiplier=5;
                if(!collection&&owned&&!selectedCard){card.material=OwnedTrophyMaterial();card.color=new Color(.84f,.84f,.84f);}
                var button=ReferenceButton("Shop object "+index,stage,x,y,207,cardHeight,()=>{
                    if(collection){shopItem=giftPage=chosen;shopDetailOpen=true;shopNotice=default;DrawMenu();}
                    else SelectShopItem(chosen);
                });
                var picture=ReferencePicture("Catalogue object",ReferenceButtonBody(button),shopTab==0?ItemsToolArt(index):TrophyArt(index),39.5f,(cardHeight-128)*.5f,128,128);
                if(shopTab==0)DecorateToolIcon(picture,index);
                card.rectTransform.SetParent(ReferenceButtonBody(button),true);card.rectTransform.SetAsFirstSibling();
                if(index==shopItem)displayedShopObject=picture;
                if(shopTab==1&&!collection&&owned)picture.material=OwnedTrophyMaterial();
                if(shopTab==0){
                    var badge=ReferencePicture("Reference shop stock badge",ReferenceButtonBody(button),ReferenceShopArt("StockBadge"),117,cardHeight-70,82,60,false);
                    badge.type=Image.Type.Sliced;badge.pixelsPerUnitMultiplier=2;
                    var shadow=badge.gameObject.AddComponent<Shadow>();
                    shadow.effectColor=new Color(.18f,.09f,.035f,.38f);shadow.effectDistance=new Vector2(0,-3);shadow.useGraphicAlpha=true;
                    
                    var marker=ReferenceText("stock marker "+index,badge.transform,UiCopy.Raw("×"),0,0,22,29,27);
                    var countLabel=ReferenceText("stock "+index,badge.transform,UiCopy.Raw(Stock((TravelTool)index).ToString()),0,0,40,49,41);
                    CentreContent(badge.rectTransform,marker.rectTransform,countLabel.rectTransform);
                }else if(owned&&!collection){
                    var ribbon=ReferenceRect("Reference shop purchased ribbon",ReferenceButtonBody(button),12,86,184,56);
                    ribbon.pivot=new Vector2(.5f,.5f);ribbon.anchoredPosition=new Vector2(104,-114);ribbon.localRotation=Quaternion.Euler(0,0,13);
                    var banner=ReferencePicture("Reference shop purchased ribbon face",ribbon,ReferenceShopArt("PurchasedRibbon"),0,0,184,56,false);
                    banner.type=Image.Type.Sliced;banner.pixelsPerUnitMultiplier=5.5f;
                    if(selectedCard)banner.material=OwnedTrophyMaterial();
                    ReferenceText("purchased card "+index,ribbon,ReferenceCopy("Purchased","Куплено","Куплено"),24,10,136,36,24);
                }
            }
            if(shopTab==1){
                int pages=(count+5)/6,page=first/6;
                if(pages>1){
                    ReferencePager(stage,"Previous trophies",105,page>0,true,()=>SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,group[Math.Max(0,first-6)])));
                    ReferencePager(stage,"Next trophies",530,page<pages-1,false,()=>SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,group[Math.Min(count-1,first+6)])));
                    ReferenceText("page count",stage,UiCopy.Raw((page+1)+" / "+pages),262,843,196,40,35);
                }
            }
            bool trophy=shopTab==1;
            var paper=ReferencePicture("Reference shop description paper",stage,ReferenceShopArt("DescriptionPaper"),70,trophy?871:880,582,trophy?186:177);
            if(trophy){paper.type=Image.Type.Sliced;paper.preserveAspect=false;paper.pixelsPerUnitMultiplier=1.5f;}
            string id=trophy?ShopCatalog.TrophyIds[shopItem]:null;
            ReferenceText("selected title",stage,trophy?ReferenceTrophyTitle(id):ToolTitle(shopItem),117,trophy?891:906,484,trophy?55:62,trophy?30:40);
            UiCopy benefit=!string.IsNullOrEmpty(shopNotice.Key)?shopNotice:!trophy?ToolDescription(shopItem):ReferenceTrophyDescription(id);
            ReferenceText("selected benefit",stage,benefit,120,trophy?957:979,480,trophy?88:59,trophy?27:28,false,true);
            bool ownedSelection=trophy&&OwnsGift(id);
            var buy=ReferenceButton(collection?"Preview":"Buy selected item",stage,80,1078,560,113,()=>{
                if(collection)OpenSelectedShopItem();else BuySelectedShopItem();
            },ownedSelection&&!collection?"GrayAction":"GreenAction");
            buy.interactable=!saveBlocked&&!HasSaveError&&GameEconomy!=null&&monetization?.FullscreenActive!=true
                &&(collection||trophy&&!ownedSelection||!trophy&&Stock((TravelTool)shopItem)<ShopCatalog.StockLimit);
            if(collection)ReferenceText("apply preview",ReferenceButtonBody(buy),ReferenceCopy("Apply style","Применить стиль","Застосувати стиль"),33,17,466,80,44,true);
            else if(ownedSelection)ReferenceText("purchased action",ReferenceButtonBody(buy),ReferenceCopy("Purchased","Куплено","Куплено"),33,17,494,80,44,true);
            else ReferenceCoinPrice(buy,trophy?ShopCatalog.TrophyPrice(id):ShopCatalog.Price((TravelTool)shopItem));
            bool Content(Transform t)=>t.name.StartsWith("Shop object ",StringComparison.Ordinal)||t.name.StartsWith("Reference shop description",StringComparison.Ordinal)||t.name=="Reference shop selected title"||t.name=="Reference shop selected benefit"||t.name=="Reference shop page count";
            int current=selected;
            SwipeReferencePage(stage,383,679,Content,direction=>{
                int next=trophy?Mathf.Clamp(first+direction*6,0,count-1):Mathf.Clamp(current+direction,0,count-1);
                SelectShopItem(trophy?Array.IndexOf(ShopCatalog.TrophyIds,group[next]):next);
            },direction=>trophy?(direction<0?first>0:first+6<count):current+direction>=0&&current+direction<count);
        }
        UiCopy ReferenceTrophyTitle(string id)=>id=="sun"&&UiLanguage=="ru"?UiCopy.Raw("Солнечная пуговица"):TrophyTitle(id);
        void ReferencePager(RectTransform stage,string name,float x,bool active,bool left,Action action)
        {
            var button=ReferenceButton(name,stage,x,838,84,55,()=>TurnMenuPage(action,left?-1:1),left?"PagerLeft":"PagerRight");button.interactable=active;
            var arrow=ReferenceButtonBody(button).GetComponentInChildren<Image>();
            arrow.rectTransform.sizeDelta=new Vector2(70,40);arrow.rectTransform.anchoredPosition=new Vector2(7,-11);
            arrow.color=active?new Color(.40f,.15f,.045f):Color.white;
        }
        void ReferenceCoinPrice(Button button,int price)
        {
            // Measure the translated words and digit count before placing the whole group.
            var body=ReferenceButtonBody(button);
            var caption=ReferenceText("buy caption",body,ReferenceCopy("Buy for","Купить за","Купити за"),0,23,210,67,36,true);
            var number=ReferenceText("coin price",body,UiCopy.Raw(price.ToString()),0,23,130,67,42,true);
            var coin=ReferencePicture("Reference shop price coin",body,ReferenceShopCoin(),0,20,60,66);
            CentreContent((RectTransform)body,caption.rectTransform,coin.rectTransform,number.rectTransform);
        }

        void DrawReferenceCoinPacks()
        {
            var stage=ReferenceShopSurface();ReferenceShopHeader(stage,false,false,true);
            for(int i=0;i<PurchaseCatalog.CoinProducts.Length;i++){
                int index=i;string product=PurchaseCatalog.CoinProducts[i];int amount=PurchaseCatalog.Coins(product);
                float x=55+(i%2)*310,y=348+(i/2)*267;
                var card=ReferencePicture("Reference shop coin offer "+amount,stage,ReferenceShopArt("CoinCard"),x,y,299,253,false);card.type=Image.Type.Sliced;card.pixelsPerUnitMultiplier=3;
                if(i<2){
                    ReferencePicture("Reference shop coin package "+amount,stage,ReferenceShopCoin(),x+75.5f,y+30,148,102);
                    if(i==1)ReferencePicture("Reference shop extra coin stack",stage,ReferenceShopCoin(),x+148,y+76,68,48);
                }else ReferencePicture("Reference shop coin package "+amount,stage,ReferenceShopArt("CoinPack"+(i-2)),x+51.5f,y+29,196,104);
                ReferencePicture("Reference shop pack quantity coin",stage,ReferenceShopCoin(),x+59,y+140,43,43);
                ReferenceText("pack quantity "+amount,stage,UiCopy.Raw("× "+amount),x+103,y+138,156,47,42);
                bool enough=ShopCoins>=PurchaseCatalog.CoinPurchaseWalletThreshold;
                UiCopy caption=enough?"You have enough coins":TestMonetization?(UiCopy)"Test shop":purchaseStore?.CanBuyProduct(product)==true?new UiCopy("Buy {0}",UiCopy.Raw(purchaseStore.ProductPrice(product))):purchaseStore?.State==PurchaseState.Retry?"Try again":"Coming soon";
                var buy=ReferenceButton("Purchase "+product,stage,x+16,y+183,267,63,()=>{
                    if(purchaseStore?.State==PurchaseState.Retry)purchaseStore.RetryConnection();else purchaseStore?.Buy(product);
                },"TealAction");
                ReferenceText("store price "+amount,ReferenceButtonBody(buy),caption,29,7,209,48,29,true);
                buy.interactable=!enough&&purchaseStore!=null&&(purchaseStore.CanBuyProduct(product)||purchaseStore.State==PurchaseState.Retry);
            }
        }
    }
}
