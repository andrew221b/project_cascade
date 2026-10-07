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
        void DrawCoinPacks()
        {
            LoadNavigationArt();NavigationHeader("Coins",()=>SetScreen(GameScreen.Shop));
            Picture("Coin wallet",menuRoot,new Vector2(.25f,.817f),new Vector2(.37f,.885f),TravelArt(3),true);
            var wallet=Text("Coin balance",menuRoot,new Vector2(.39f,.817f),new Vector2(.74f,.885f),26,UiCopy.Raw(ShopCoins.ToString("N0")));wallet.font=art.titleFont;wallet.alignment=TextAlignmentOptions.MidlineLeft;
            ShopCategoryButtons(true);
            CoinPackRow(PurchaseCatalog.Coins240,.699f);
            CoinPackRow(PurchaseCatalog.Coins480,.494f);
            var notice=Rect("Coin purchase explanation",menuRoot,new Vector2(.095f,.121f),new Vector2(.905f,.301f));
            var paper=Background(notice,Color.white,false);paper.sprite=enamelControls[0];paper.type=Image.Type.Sliced;
            var note=Text("Fair help rules",notice,new Vector2(.06f,.40f),new Vector2(.94f,.94f),17,"Up to two tools per stage. Help limits this run to two stars.");note.enableAutoSizing=true;note.fontSizeMin=16*MenuUnit;note.fontSizeMax=17*MenuUnit;navigationLightLabels.Add(note);
            var state=Text("Store availability",notice,new Vector2(.06f,.065f),new Vector2(.94f,.37f),16,TestMonetization?(UiCopy)"Test shop: no charge":purchaseStore?.State==PurchaseState.Deferred?"Waiting for purchase approval. You can keep playing.":purchaseStore?.State==PurchaseState.Busy?"Checking purchase…":purchaseStore?.State==PurchaseState.Retry?"The store could not finish. Try again.":purchaseStore?.State==PurchaseState.Unconfigured||purchaseStore?.State==PurchaseState.Unavailable?"The shop is not open yet.":"Coins can also be earned by playing.");state.enableAutoSizing=true;state.fontSizeMin=15*MenuUnit;state.fontSizeMax=16*MenuUnit;navigationLightLabels.Add(state);
        }
        void ShopCategoryButtons(bool coins=false)
        {
            float[] left={.085f,.369f,.653f};string[] labels={"Tool tab","Trophies","Coins"};
            for(int i=0;i<3;i++){
                int tab=i;var button=CounterButton(labels[i]+" tab",labels[i],left[i],left[i]+.262f,.731f,.807f,()=>{
                    if(tab==2)ShowCoinPacks();else{shopTab=tab;shopItem=0;shopNotice=default;SetScreen(GameScreen.Shop);}
                });
                navigationLightLabels.Remove(button.GetComponentInChildren<TextMeshProUGUI>());MarkSelection(button,coins?i==2:i==shopTab);
            }
        }
        void CoinPackRow(string product,float top)
        {
            int amount=PurchaseCatalog.Coins(product);
            var row=Rect("Coin pack "+amount,menuRoot,new Vector2(.085f,top-.180f),new Vector2(.915f,top));
            var face=Background(row,Color.white,false);face.sprite=enamelControls[0];face.type=Image.Type.Sliced;
            var pouch=Picture("Coin pouch",row,new Vector2(.045f,.25f),new Vector2(.255f,.91f),TravelArt(3),true);pouch.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1.5f),amount*.01f,.6f);
            var title=Text("Pack quantity",row,new Vector2(.29f,.71f),new Vector2(.945f,.97f),25,new UiCopy("{0} coins",amount));title.font=art.titleFont;title.alignment=TextAlignmentOptions.MidlineLeft;navigationLightLabels.Add(title);
            var detail=Text("Pack use",row,new Vector2(.29f,.44f),new Vector2(.945f,.72f),16,amount==240?(UiCopy)"A trophy or a few tools":"More for your collection");detail.alignment=TextAlignmentOptions.MidlineLeft;detail.enableAutoSizing=true;detail.fontSizeMin=15*MenuUnit;detail.fontSizeMax=17*MenuUnit;navigationLightLabels.Add(detail);
            bool enough=ShopCoins>=PurchaseCatalog.CoinPurchaseWalletThreshold;
            UiCopy label=enough?"You have enough coins":TestMonetization?new UiCopy("Try +{0}",amount):purchaseStore?.CanBuyProduct(product)==true?new UiCopy("Buy {0}",UiCopy.Raw(purchaseStore.ProductPrice(product))):purchaseStore?.State==PurchaseState.Busy?"Checking purchase…":purchaseStore?.State==PurchaseState.Retry?"Try again":"Coming soon";
            var buy=Button(label,row,new Vector2(.29f,.055f),new Vector2(.945f,.405f),()=>{if(purchaseStore.State==PurchaseState.Retry)purchaseStore.RetryConnection();else purchaseStore.Buy(product);});buy.name="Purchase "+product;((RectTransform)buy.transform).sizeDelta=new Vector2(0,12*MenuUnit);
            buy.GetComponent<Image>().sprite=enamelControls[0];buy.GetComponent<Image>().type=Image.Type.Sliced;
            var caption=buy.GetComponentInChildren<TextMeshProUGUI>();caption.textWrappingMode=TextWrappingModes.NoWrap;caption.enableAutoSizing=true;caption.fontSizeMin=14*MenuUnit;caption.fontSizeMax=19*MenuUnit;caption.margin=new Vector4(8,4,8,4);
            buy.interactable=!enough&&purchaseStore!=null&&(purchaseStore.CanBuyProduct(product)||purchaseStore.State==PurchaseState.Retry);
        }
    }
}
