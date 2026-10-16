using Cascade.Application;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        public void ShowCoinPacks()
        {
            if(CurrentScreen!=GameScreen.Shop&&CurrentScreen!=GameScreen.CoinPacks)ShowShop();
            SetScreen(GameScreen.CoinPacks);
        }
        void DrawCoinPacks(){DrawReferenceCoinPacks();}
        void ShopCategoryButtons(bool coins=false)
        {
            string[] labels={"Tool tab","Trophies","Coins"};
            for(int i=0;i<3;i++){
                int tab=i;CatalogueTabBounds(i,out float left,out float right);var button=CounterButton(labels[i]+" tab",labels[i],left,right,.731f,.807f,()=>{
                    if(tab==2)ShowCoinPacks();else{shopTab=tab;shopItem=0;shopDetailOpen=false;shopNotice=default;SetScreen(GameScreen.Shop);}
                });
                RegisterMenuColumn(button,i,3,MenuControlGap);
                navigationLightLabels.Remove(button.GetComponentInChildren<TextMeshProUGUI>());MarkSelection(button,coins?i==2:i==shopTab);
                menuRows.RemoveAll(row=>row.rect==(RectTransform)button.transform);RegisterMenuRow(button,.799f,0,44,0,12,17);
            }
        }
        void CoinPackRow(string product,float top)
        {
            int amount=PurchaseCatalog.Coins(product);
            var row=Rect("Coin pack "+amount,menuRoot,new Vector2(.055f,top-.18f),new Vector2(.945f,top));PaintStoryPaper(row,false);
            Picture("Coin pouch",row,new Vector2(.04f,.27f),new Vector2(.29f,.84f),TravelArt(3),true);
            var title=Text("Pack quantity",row,new Vector2(.34f,.56f),new Vector2(.94f,.88f),26,UiCopy.Raw("× "+amount));title.alignment=TextAlignmentOptions.MidlineLeft;title.margin=Vector4.zero;StyleCelebrationType(title,CelebrationType.SkyNumber);
            bool enough=ShopCoins>=PurchaseCatalog.CoinPurchaseWalletThreshold;
            UiCopy caption=enough?"You have enough coins":TestMonetization?(UiCopy)"Test shop":purchaseStore?.CanBuyProduct(product)==true?new UiCopy("Buy {0}",UiCopy.Raw(purchaseStore.ProductPrice(product))):purchaseStore?.State==PurchaseState.Retry?"Try again":"Coming soon";
            var buy=Button(caption,row,new Vector2(.33f,.06f),new Vector2(.94f,.43f),()=>{if(purchaseStore.State==PurchaseState.Retry)purchaseStore.RetryConnection();else purchaseStore.Buy(product);});buy.name="Purchase "+product;
            var rect=(RectTransform)buy.transform;rect.anchorMax=new Vector2(.94f,.06f);rect.pivot=new Vector2(.5f,0);rect.sizeDelta=new Vector2(0,48*MenuUnit);
            var face=buy.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=Color.white;
            navigationLightLabels.Add(buy.GetComponentInChildren<TextMeshProUGUI>());
            buy.interactable=!enough&&purchaseStore!=null&&(purchaseStore.CanBuyProduct(product)||purchaseStore.State==PurchaseState.Retry);
        }
    }
}
