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
        static readonly string[] TrophyBenefits={
            "Leaf pin, drifting leaves","Sun button, warm motes","Sky outfit, falling feathers","Moon outfit, twinkling stars",
            "Chime pin, floating petals","Raincoat, soft raindrops","Acorn pin, amber leaves","Brooch, glowing fireflies"
        };

        bool DurableTrophyOwned(string id)=>Rewards.Owns(id)||profile.Monetization?.Cosmetics.Contains(id)==true;

        UiCopy TrophyAcquisition(string id)
        {
            int goal=ShopCatalog.EarnedClearMilestone(id);
            if(DurableTrophyOwned(id))return "Collected";
            UiCopy progress=goal>0?new UiCopy("Levels: {0}/{1}",Mathf.Min(Rewards.CosmeticClearCount,goal),goal):new UiCopy("Parcel parts: {0}/3",Rewards.Fragments(id));
            UiCopy state=DurableTrophyOwned(id)?(UiCopy)"Collected":OwnsGift(id)?"Test look":"or coins";
            return UiCopy.Join("\n",new[]{progress,state});
        }

        TextMeshProUGUI TrophyDetailText(string name,Vector2 min,Vector2 max,int size,UiCopy copy,bool title=false)
        {
            var label=Text(name,menuRoot,min,max,size,copy);
            label.alignment=TextAlignmentOptions.MidlineLeft;label.enableAutoSizing=true;
            label.fontSizeMin=(title?18:16)*MenuUnit;label.fontSizeMax=size*MenuUnit*(Preferences.LargeText?1.10f:1);
            label.margin=Vector4.zero;label.textWrappingMode=title?TextWrappingModes.NoWrap:TextWrappingModes.Normal;
            if(title)label.font=art.titleFont;
            return label;
        }

        void DrawTrophySelection(bool collection)
        {
            string id=ShopCatalog.TrophyIds[shopItem];bool owned=OwnsGift(id);
            var preview=Rect("Trophy appearance preview",menuRoot,new Vector2(.085f,.280f),new Vector2(.350f,.475f));
            CosmeticAmbiance(preview,id,true);
            var hero=Picture("Trophy Mika preview",preview,new Vector2(.07f,.02f),new Vector2(.93f,.98f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            AttachMikaCosmetic(hero,id);
            TrophyDetailText("Selected object title",new Vector2(.385f,.420f),new Vector2(.920f,.474f),24,GiftNames[shopItem],true);
            TrophyDetailText("Selected object benefit",new Vector2(.385f,.346f),new Vector2(.920f,.415f),18,TrophyBenefits[shopItem]);
            TrophyDetailText("Trophy acquisition",new Vector2(.385f,.280f),new Vector2(.920f,.342f),17,TrophyAcquisition(id));
            var scope=Text("Trophy cosmetic scope",menuRoot,new Vector2(.095f,.230f),new Vector2(.905f,.270f),16,TestMonetization&&!DurableTrophyOwned(id)?(UiCopy)"Preview only; resets on restart":"Appearance only");
            scope.enableAutoSizing=true;scope.fontSizeMin=16*MenuUnit;scope.fontSizeMax=17*MenuUnit;scope.margin=Vector4.zero;
            scope.textWrappingMode=TextWrappingModes.NoWrap;
            if(!string.IsNullOrEmpty(shopNotice.Key))scope.SetCopy(shopNotice);

            bool pending=Rewards.Parcels.Any(p=>p.GiftId==id&&!p.Claimed);
            bool parcelTrophy=ShopCatalog.EarnedClearMilestone(id)==0;
            UiCopy caption;Action action;bool enabled;
            if(owned){
                caption=EquippedGift==id?(UiCopy)"With Mika":"Wear this look";
                action=()=>EquipShopGift(id);enabled=EquippedGift!=id&&!saveBlocked;
            }else if(pending){
                caption="Open parcel";action=()=>OpenSelectedTrophyParcel(id);enabled=!saveBlocked;
            }else if(collection){
                caption=parcelTrophy?(Rewards.SelectedGift==id?(UiCopy)"Next trophy":"Choose this trophy"):(UiCopy)"Find in shop";
                action=()=>{
                    if(parcelTrophy){Rewards.SelectGift(id);PersistProgress();DrawMenu();}
                    else{ShowShop();shopTab=1;shopItem=Array.IndexOf(ShopCatalog.TrophyIds,id);DrawMenu();}
                };
                enabled=!saveBlocked&&(!parcelTrophy||Rewards.SelectedGift!=id);
            }else{
                caption=TestMonetization?(UiCopy)"Try this look":new UiCopy("Buy for {0} coins",ShopCatalog.KeepsakePrice);
                action=()=>BuySelectedShopItem();
                enabled=!saveBlocked&&monetization!=null&&!monetization.FullscreenActive&&(TestMonetization||ShopCoins>=ShopCatalog.KeepsakePrice);
                if(!TestMonetization&&ShopCoins<ShopCatalog.KeepsakePrice)caption=new UiCopy("Need {0} more coins",ShopCatalog.KeepsakePrice-ShopCoins);
            }
            var primary=CounterButton(collection?"Trophy action":"Buy selected item",caption,.095f,.905f,.138f,.217f,action,true);
            primary.interactable=enabled;QuietDisabledCounter(primary);
            if(!collection){
                bool fromTools=shopReturnScreen==GameScreen.Supplies||shopReturnScreen==GameScreen.Playing||shopReturnScreen==GameScreen.Pause||shopReturnScreen==GameScreen.Fail;
                CounterButton("My tools link",fromTools?"Tool bag":shopReturnScreen==GameScreen.Collection?"Trophies":"My things",.095f,.905f,.049f,.125f,
                    fromTools?(Action)ShowSupplies:shopReturnScreen==GameScreen.Collection?(Action)ShowCollection:ShowTravelBag);
            }
        }

        void OpenSelectedTrophyParcel(string id)
        {
            var receipt=Rewards.Parcels.FirstOrDefault(p=>p.GiftId==id&&!p.Claimed);if(receipt==null)return;
            parcelReturnsToMap=false;parcelReturnScreen=CurrentScreen;parcelOrder=receipt.Order;parcelOpened=false;SetScreen(GameScreen.Parcel);
        }
    }
}
